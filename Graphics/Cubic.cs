#region File Description
//-------------------------------------------------------------------------------------------------
// Cubic.cs
//
// A collection of classes and structs used for calculating cubic splines.
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
	#region Cubic coefficient types
	/// <summary>
	/// Interface for cubic coefficient types.
	/// </summary>
	public interface ICubicType<T>
	{
		T Zero { get; }
		T One { get; }
		T Two { get; }
		T Four { get; }

		T Add(T v1, T v2);
		T Sub(T v1, T v2);
		T Mult(T v1, T v2);
		T Mult(T v, float factor);
		T Div(T v1, T v2);
		T Div(T v, float divisor);
	}

	/// <summary>
	/// Defines a cubic coefficient with one component.
	/// </summary>
	public struct Cubic1 : ICubicType<Cubic1>
	{
		#region Fields
		public float x;
		#endregion

		#region Properties
		public Cubic1 Zero
		{
			get
			{ return new Cubic1(0f); }
		}

		public Cubic1 One
		{
			get
			{ return new Cubic1(1f); }
		}

		public Cubic1 Two
		{
			get
			{ return new Cubic1(2f); }
		}

		public Cubic1 Four
		{
			get
			{ return new Cubic1(4f); }
		}
		#endregion

		#region Initialization
		public Cubic1(float x)
		{
			this.x = x;
		}
		#endregion

		#region Methods
		public Cubic1 Add(Cubic1 c1, Cubic1 c2)
		{
			return new Cubic1(c1.x + c2.x);
		}

		public Cubic1 Sub(Cubic1 c1, Cubic1 c2)
		{
			return new Cubic1(c1.x - c2.x);
		}

		public Cubic1 Mult(Cubic1 c1, Cubic1 c2)
		{
			return new Cubic1(c1.x * c2.x);
		}

		public Cubic1 Mult(Cubic1 c, float factor)
		{
			return new Cubic1(c.x * factor);
		}

		public Cubic1 Div(Cubic1 c1, Cubic1 c2)
		{
			return new Cubic1(c1.x / c2.x);
		}

		public Cubic1 Div(Cubic1 c, float divisor)
		{
			return new Cubic1(c.x / divisor);
		}

		public byte ToByte()
		{
			return (byte)(MathHelper.Clamp(this.x, 0f, 1f) * 255);
		}
		#endregion
	}

	/// <summary>
	/// Defines a cubic coefficient with two components.
	/// </summary>
	public struct Cubic2 : ICubicType<Cubic2>
	{
		#region Fields
		public float x;
		public float y;
		#endregion

		#region Properties
		public Cubic2 Zero
		{
			get
			{ return new Cubic2(0f, 0f); }
		}

		public Cubic2 One
		{
			get
			{ return new Cubic2(1f, 1f); }
		}

		public Cubic2 Two
		{
			get
			{ return new Cubic2(2f, 2f); }
		}

		public Cubic2 Four
		{
			get
			{ return new Cubic2(4f, 4f); }
		}
		#endregion

		#region Initialization
		public Cubic2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public Cubic2(Vector2 vec)
		{
			this.x = vec.X;
			this.y = vec.Y;

		}
		#endregion

		#region Methods
		public Cubic2 Add(Cubic2 c1, Cubic2 c2)
		{
			return new Cubic2(c1.x + c2.x, c1.y + c2.y);
		}

		public Cubic2 Sub(Cubic2 c1, Cubic2 c2)
		{
			return new Cubic2(c1.x - c2.x, c1.y - c2.y);
		}

		public Cubic2 Mult(Cubic2 c1, Cubic2 c2)
		{
			return new Cubic2(c1.x * c2.x, c1.y * c2.y);
		}

		public Cubic2 Mult(Cubic2 c, float factor)
		{
			return new Cubic2(c.x * factor, c.y * factor);
		}

		public Cubic2 Div(Cubic2 c1, Cubic2 c2)
		{
			return new Cubic2(c1.x / c2.x, c1.y / c2.y);
		}

		public Cubic2 Div(Cubic2 c, float divisor)
		{
			return new Cubic2(c.x / divisor, c.y / divisor);
		}

		public Vector2 ToVector2()
		{
			return new Vector2(this.x, this.y);
		}
		#endregion
	}

	/// <summary>
	/// Defines a cubic coefficient with three components.
	/// </summary>
	public struct Cubic3 : ICubicType<Cubic3>
	{
		#region Fields
		public float x;
		public float y;
		public float z;
		#endregion

		#region Properties
		public Cubic3 Zero
		{
			get
			{ return new Cubic3(0f, 0f, 0f); }
		}

		public Cubic3 One
		{
			get
			{ return new Cubic3(1f, 1f, 1f); }
		}

		public Cubic3 Two
		{
			get
			{ return new Cubic3(2f, 2f, 2f); }
		}

		public Cubic3 Four
		{
			get
			{ return new Cubic3(4f, 4f, 4f); }
		}
		#endregion

		#region Initialization
		public Cubic3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Cubic3(Vector3 vec)
		{
			this.x = vec.X;
			this.y = vec.Y;
			this.z = vec.Z;
		}

		public Cubic3(Color rgb)
		{
			this.x = rgb.R;
			this.y = rgb.G;
			this.z = rgb.B;
		}
		#endregion

		#region Methods
		public Cubic3 Add(Cubic3 c1, Cubic3 c2)
		{
			return new Cubic3(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);
		}

		public Cubic3 Sub(Cubic3 c1, Cubic3 c2)
		{
			return new Cubic3(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);
		}

		public Cubic3 Mult(Cubic3 c1, Cubic3 c2)
		{
			return new Cubic3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
		}

		public Cubic3 Mult(Cubic3 c, float factor)
		{
			return new Cubic3(c.x * factor, c.y * factor, c.z * factor);
		}

		public Cubic3 Div(Cubic3 c1, Cubic3 c2)
		{
			return new Cubic3(c1.x / c2.x, c1.y / c2.y, c1.z / c2.z);
		}

		public Cubic3 Div(Cubic3 c, float divisor)
		{
			return new Cubic3(c.x / divisor, c.y / divisor, c.z / divisor);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(this.x, this.y, this.z);
		}

		public Color ToColor()
		{
			byte r = (byte)(MathHelper.Clamp(this.x, 0f, 1f) * 255);
			byte g = (byte)(MathHelper.Clamp(this.y, 0f, 1f) * 255);
			byte b = (byte)(MathHelper.Clamp(this.z, 0f, 1f) * 255);

			return new Color(r, g, b, 255);
		}
		#endregion
	}

	/// <summary>
	/// Defines a cubic coefficient with four components.
	/// </summary>
	public struct Cubic4 : ICubicType<Cubic4>
	{
		#region Fields
		public float x;
		public float y;
		public float z;
		public float w;
		#endregion

		#region Properties
		public Cubic4 Zero
		{
			get
			{ return new Cubic4(0f, 0f, 0f, 0f); }
		}

		public Cubic4 One
		{
			get
			{ return new Cubic4(1f, 1f, 1f, 1f); }
		}

		public Cubic4 Two
		{
			get
			{ return new Cubic4(2f, 2f, 2f, 2f); }
		}

		public Cubic4 Four
		{
			get
			{ return new Cubic4(4f, 4f, 4f, 4f); }
		}
		#endregion

		#region Initialization
		public Cubic4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public Cubic4(Vector4 vec)
		{
			this.x = vec.X;
			this.y = vec.Y;
			this.z = vec.Z;
			this.w = vec.W;
		}

		public Cubic4(Color rgb)
		{
			this.x = rgb.R;
			this.y = rgb.G;
			this.z = rgb.B;
			this.w = rgb.A;
		}
		#endregion

		#region Methods
		public Cubic4 Add(Cubic4 c1, Cubic4 c2)
		{
			return new Cubic4(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z, c1.w + c2.w);
		}

		public Cubic4 Sub(Cubic4 c1, Cubic4 c2)
		{
			return new Cubic4(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z, c1.w - c2.w);
		}

		public Cubic4 Mult(Cubic4 c1, Cubic4 c2)
		{
			return new Cubic4(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z, c1.w * c2.w);
		}

		public Cubic4 Mult(Cubic4 c, float factor)
		{
			return new Cubic4(c.x * factor, c.y * factor, c.z * factor, c.w * factor);
		}

		public Cubic4 Div(Cubic4 c1, Cubic4 c2)
		{
			return new Cubic4(c1.x / c2.x, c1.y / c2.y, c1.z / c2.z, c1.w / c2.w);
		}

		public Cubic4 Div(Cubic4 c, float divisor)
		{
			return new Cubic4(c.x / divisor, c.y / divisor, c.z / divisor, c.w / divisor);
		}

		public Vector4 ToVector4()
		{
			return new Vector4(this.x, this.y, this.z, this.w);
		}

		public Color ToColor()
		{
			byte r = (byte)(MathHelper.Clamp(this.x, 0f, 1f) * 255);
			byte g = (byte)(MathHelper.Clamp(this.y, 0f, 1f) * 255);
			byte b = (byte)(MathHelper.Clamp(this.z, 0f, 1f) * 255);
			byte a = (byte)(MathHelper.Clamp(this.w, 0f, 1f) * 255);

			return new Color(r, g, b, a);
		}
		#endregion
	}
	#endregion

	/// <summary>
	/// Represents a cubic equation consisting of n-dimensional coefficients. The class was designed 
	/// for use with types Cubic1, Cubic2, Cubic3 and Cubic4.
	/// </summary>
	public class Cubic<T> where T : ICubicType<T>
	{
		#region Fields
		// These are the coefficients of the cubic equation a+bt+ct^2+dt^3.
		public T a;
		public T b;
		public T c;
		public T d;
		#endregion

		#region Initialization
		/// <summary>
		/// Constructor for the cubic equation a + bt + ct^2 + dt^3.
		/// </summary>
		public Cubic(T a, T b, T c, T d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}
		#endregion
	}

	/// <summary>
	/// Class for calculating and interpolating cubic splines. The class was designed for use with 
	/// types Cubic1, Cubic2, Cubic3 and Cubic4.
	/// </summary>
	public class CubicSpline<T> where T : ICubicType<T>, new()
	{
		#region Fields
		private Cubic<T>[] cubics;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the number of cubic equations that make up the spline.
		/// </summary>
		public int Count
		{
			get
			{ return cubics.Length; }
		}
		#endregion

		#region Methods
		/// <summary>
		/// Fits a cubic spline to the list of control points.
		/// </summary>
		/// <param name="transferPoints">Control points to fit a cubic spline to.</param>
		public void Calculate(List<T> controlPoints)
		{
			Debug.Assert(controlPoints.Count >= 3, "A cubic spline cannot be calculated with less " +
				"than three points. Otherwise it is just a straight line (with two points).");

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

			int numCubics = controlPoints.Count - 1;
			T t = new T();

			// Construct the coefficients a,b,c and the right-hand-side vector d.
			T[,] matrixCoef = new T[numCubics + 1, 4];

			matrixCoef[0, 0] = t.Zero;
			matrixCoef[0, 1] = t.Two;
			matrixCoef[0, 2] = t.One;
			matrixCoef[0, 3] = t.Mult(t.Sub(controlPoints[1], controlPoints[0]), 3f);

			for (int i = 1; i < numCubics; ++i)
			{
				matrixCoef[i, 0] = t.One;
				matrixCoef[i, 1] = t.Four;
				matrixCoef[i, 2] = t.One;
				matrixCoef[i, 3] = t.Mult(t.Sub(controlPoints[i + 1], controlPoints[i - 1]), 3f);
			}

			matrixCoef[numCubics, 0] = t.One;
			matrixCoef[numCubics, 1] = t.Two;
			matrixCoef[numCubics, 2] = t.Zero;
			matrixCoef[numCubics, 3] = t.Mult(t.Sub(controlPoints[numCubics], controlPoints[numCubics - 1]), 3f);

			// Modify the coefficients (Thomas algorithm).
			matrixCoef[0, 2] = t.Div(matrixCoef[0, 2], matrixCoef[0, 1]);
			matrixCoef[0, 3] = t.Div(matrixCoef[0, 3], matrixCoef[0, 1]);

			for (int i = 1; i < numCubics; ++i)
			{
				T val = t.Sub(matrixCoef[i, 1], t.Mult(matrixCoef[i - 1, 2], matrixCoef[i, 0]));
				matrixCoef[i, 2] = t.Div(matrixCoef[i, 2], val);
				matrixCoef[i, 3] = t.Div(t.Sub(matrixCoef[i, 3], t.Mult(matrixCoef[i - 1, 3], matrixCoef[i, 0])), val);
			}

			// Back substitute to solve for the derivaties (Thomas algorithm).
			T[] derivatives = new T[numCubics + 1];

			derivatives[numCubics - 1] = matrixCoef[numCubics - 1, 3];
			for (int i = numCubics - 2; i >= 0; --i)
				derivatives[i] = t.Sub(matrixCoef[i, 3], t.Mult(matrixCoef[i, 2], derivatives[i + 1]));

			// Use the derivatives to solve for the coefficients of the cubics.
			cubics = new Cubic<T>[numCubics];
			for (int i = 0; i < numCubics; ++i)
			{
				T a = controlPoints[i];
				T b = derivatives[i];
				T c = t.Sub(t.Sub(t.Mult(t.Sub(controlPoints[i + 1], controlPoints[i]), 3f), t.Mult(derivatives[i], 2f)), derivatives[i + 1]);
				T d = t.Add(t.Add(t.Mult(t.Sub(controlPoints[i], controlPoints[i + 1]), 2f), derivatives[i]), derivatives[i + 1]);
				cubics[i] = new Cubic<T>(a, b, c, d);
			}
		}

		/// <summary>
		/// Gets a point on the cubic spline.
		/// </summary>
		/// <param name="cubicNum">Cubic segment of interest.</param>
		/// <param name="position">0-1 position on the cubic.</param>
		/// <returns>Actual data point on the spline.</returns>
		public T GetPointOnSpline(int cubicNum, float position)
		{
			Debug.Assert(cubicNum >= 0 && cubicNum <= cubics.Length);
			Debug.Assert(position >= 0f && position <= 1f);

			Cubic<T> cubic = cubics[cubicNum];
			T t = new T();

			// a+t(b+t(c+dt)) is the same as a+bt+ct^2+dt^3, but has three less multiplications.
			return t.Add(t.Mult(t.Add(t.Mult(t.Add(t.Mult(cubic.d, position), cubic.c), position), cubic.b), position), cubic.a);
		}
		#endregion
	}
}