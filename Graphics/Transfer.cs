#region File Description
//-------------------------------------------------------------------------------------------------
// Transfer.cs
//
// A collection of objects used for creating the transfer function, which is used by a volumetric
// model to determine the RBGA values of the voxels.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.Diagnostics;
#endregion

namespace Renderer.Graphics
{
	/// <summary>
	/// Represents a single transfer point.
	/// </summary>
	public struct TransferPoint
	{
		#region Fields
		public Vector4 color; // Needs to be a Vector4 because it has math operators defined.
		public byte isoValue;
		#endregion

		#region Initialization
		/// <param name="color">RGB color of the point.</param>
		/// <param name="isoValue">Isovalue of the point.</param>
		public TransferPoint(Color color, byte isoValue)
		{
			this.color = color.ToVector4();
			this.isoValue = isoValue;
		}

		/// <param name="alpha">Alpha value of the point.</param>
		/// <param name="isoValue">Isovalue of the point.</param>
		public TransferPoint(float alpha, byte isoValue)
		{
			Debug.Assert(alpha >= 0f && alpha <= 1f);

			this.color = new Vector4(Vector3.Zero, alpha);
			this.isoValue = isoValue;
		}
		#endregion
	}

	/// <summary>
	/// Represents a cubic equation consisting of RGBA coefficients.
	/// </summary>
	public class Cubic
	{
		#region Fields
		// These are the coefficients of the cubic equation a+bt+ct^2+dt^3, but each point on the 
		// line is an RGBA color so the coefficients need to be 4-dimensional.
		public Vector4 a;
		public Vector4 b;
		public Vector4 c;
		public Vector4 d;
		#endregion

		#region Initialization
		/// <summary>
		/// Constructor for the cubic equation a + bt + ct^2 + dt^3.
		/// </summary>
		public Cubic(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}
		#endregion
	}

	/// <summary>
	/// Class for calculating and interpolating cubic splines.
	/// </summary>
	public class CubicSpline
	{
		#region Fields
		private Cubic[] cubics;
		#endregion

		#region Methods
		/// <summary>
		/// Fits a cubic spline to the list of transfer control points.
		/// </summary>
		/// <param name="transferPoints">Transfer control points to fit a cubic spline to.</param>
		public void Calculate(List<TransferPoint> transferPoints)
		{
			// We need to solve the equation taken from: http://mathworld.wolfram.com/CubicSpline.html,
			// 
			// [2 1      0] [D[0]]   [3(y[1] - y[0])  ]
			// |1 4 1     | |D[1]|   |3(y[2] - y[0])  |
			// |  1 4 1   | | .  | = |       .        |
			// |    ..... | | .  |   |       .        |
			// |     1 4 1| | .  |   |3(y[n] - y[n-2])|
			// [0      1 2] [D[n]]   [3(y[n] - y[n-1])]
			// 
			// where n is the number of cubics and D[i] are the derivatives at the control points.
			// This linear system of equations happens to be tridiagonal and therefore can be solved
			// using the Thomas algorithm (aka TDMA), which is a simplified form of Gaussian elimination
			// that can obtain the solution in O(n) instead of O(n^3) required by Gaussian elimination.
			// The Thomas algorithm was taken from: http://en.wikipedia.org/wiki/Tridiagonal_matrix_algorithm.

			int numCubics = transferPoints.Count - 1;

			// Construct the coefficients a,b,c and the right-hand-side vector d.
			Vector4[,] matrixCoef = new Vector4[numCubics + 1, 4];

			matrixCoef[0, 0] = Vector4.Zero;
			matrixCoef[0, 1] = Vector4.One * 2;
			matrixCoef[0, 2] = Vector4.One;
			matrixCoef[0, 3] = (transferPoints[1].color - transferPoints[0].color) * 3;

			for (int i = 1; i < numCubics; ++i)
			{
				matrixCoef[i, 0] = Vector4.One;
				matrixCoef[i, 1] = Vector4.One * 4;
				matrixCoef[i, 2] = Vector4.One;
				matrixCoef[i, 3] = (transferPoints[i + 1].color - transferPoints[i - 1].color) * 3;
			}

			matrixCoef[numCubics, 0] = Vector4.One;
			matrixCoef[numCubics, 1] = Vector4.One * 2;
			matrixCoef[numCubics, 2] = Vector4.Zero;
			matrixCoef[numCubics, 3] = (transferPoints[numCubics].color - transferPoints[numCubics - 1].color) * 3;

			// Modify the coefficients.
			matrixCoef[0, 2] /= matrixCoef[0, 1];
			matrixCoef[0, 3] /= matrixCoef[0, 1];

			for (int i = 1; i < numCubics; ++i)
			{
				Vector4 val = matrixCoef[i, 1] - (matrixCoef[i - 1, 2] * matrixCoef[i, 0]);
				matrixCoef[i, 2] /= val;
				matrixCoef[i, 3] = (matrixCoef[i, 3] - (matrixCoef[i - 1, 3] * matrixCoef[i, 0])) / val;
			}

			// Back substitute to solve for the derivaties.
			Vector4[] derivatives = new Vector4[numCubics + 1];

			derivatives[numCubics - 1] = matrixCoef[numCubics - 1, 3];
			for (int i = numCubics - 2; i >= 0; --i)
				derivatives[i] = matrixCoef[i, 3] - (matrixCoef[i, 2] * derivatives[i + 1]);

			// Use the derivatives to solve for the coefficients of the cubics.
			cubics = new Cubic[numCubics];
			for (int i = 0; i < numCubics; ++i)
				cubics[i] = new Cubic(transferPoints[i].color, derivatives[i],
					(3 * (transferPoints[i + 1].color - transferPoints[i].color)) - (2 * derivatives[i]) - derivatives[i + 1],
					(2 * (transferPoints[i].color - transferPoints[i + 1].color)) + derivatives[i] + derivatives[i + 1]);
		}
		#endregion
	}

	/// <summary>
	/// Static class used for creating transfer functions.
	/// </summary>
	public static class Transfer
	{
		#region Methods
		/// <summary>
		/// Creates a transfer function from color and alpha cubic splines.
		/// </summary>
		/// <param name="colorPoints">Color transfer points to use.</param>
		/// <param name="alphaPoints">Alpha transfer points to use.</param>
		/// <returns>An array of RGBA colors corresponding to each isovalue 0-255.</returns>
		public static Color[] CreateTransferFunction(List<TransferPoint> colorPoints, List<TransferPoint> alphaPoints)
		{
			Debug.Assert(colorPoints.Count >= 2);
			Debug.Assert(alphaPoints.Count >= 2);

			// Calculate the cubic splines for the color and alpha values.
			CubicSpline colorSpline = new CubicSpline();
			CubicSpline alphaSpline = new CubicSpline();
			colorSpline.Calculate(colorPoints);
			alphaSpline.Calculate(alphaPoints);

			// Create the transfer function from the two splines.
			Color[] transferFunc = new Color[256];

			int index = 0;
			for (int i = 0; i < colorPoints.Count - 1; ++i)
			{
				int interval = colorPoints[i + 1].isoValue - colorPoints[i].isoValue;
				Debug.Assert(interval - 1 > 0);

				for (int j = 0; j < interval; ++j)
				{
					float point = j / (float)interval;
				}
			}

			return transferFunc;
		}
		#endregion
	}
}
