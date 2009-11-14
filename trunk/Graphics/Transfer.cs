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
		public Vector4 color; // Needs to be a Vector4 so that 
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
	/// 
	/// </summary>
	public class CubicRGBA
	{
		#region Fields
		// These are the coefficients of the cubic equation a+bt+ct^2+dt^3, but each point on the 
		// line is an RGBA color so the coefficients need to be 4-dimensional.
		private Vector4 a;
		private Vector4 b;
		private Vector4 c;
		private Vector4 d;
		#endregion

		#region Initialization
		/// <summary>
		/// Constructor for the cubic equation a + bt + ct^2 + dt^3.
		/// </summary>
		public CubicRGBA(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}
		#endregion

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="transferPoints"></param>
		/// <returns></returns>
		public static CubicRGBA[] CalculateCubicSpline(List<TransferPoint> transferPoints)
		{
			return null;
		}
		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public static class Transfer
	{
		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="colorPoints"></param>
		/// <param name="alphaPoints"></param>
		/// <returns></returns>
		public static Color[] CreateTransferFunction(List<TransferPoint> colorPoints, List<TransferPoint> alphaPoints)
		{
			Debug.Assert(colorPoints.Count >= 2);
			Debug.Assert(alphaPoints.Count >= 2);

			// Calculate the cubic splines for the color and alpha values. We need to send copies 
			// of the lists because CalculateCubicSpline will modify the values in the lists that 
			// are sent to it.
			CubicRGBA[] colorCubics = CubicRGBA.CalculateCubicSpline(new List<TransferPoint>(colorPoints));
			CubicRGBA[] alphaCubics = CubicRGBA.CalculateCubicSpline(new List<TransferPoint>(alphaPoints));

			Color[] transferFunc = new Color[256];

			return transferFunc;
		}
		#endregion
	}
}
