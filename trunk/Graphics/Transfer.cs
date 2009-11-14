#region File Description
//-------------------------------------------------------------------------------------------------
// Transfer.cs
//
// A collection of objects used for creating the transfer function, which is used by a volumetric
// model to determine the RBGA values of the voxels.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
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
		public Color color;
		public byte isoValue;

		#region Initialization
		/// <param name="color">RGB color of the point.</param>
		/// <param name="isoValue">Isovalue of the point.</param>
		public TransferPoint(Color color, byte isoValue)
		{
			this.color = color;
			this.isoValue = isoValue;
		}

		/// <param name="alpha">Alpha value of the point.</param>
		/// <param name="isoValue">Isovalue of the point.</param>
		public TransferPoint(float alpha, byte isoValue)
		{
			Debug.Assert(alpha >= 0f && alpha <= 1f);

			this.color = new Color(Color.Black, alpha);
			this.isoValue = isoValue;
		}
		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public class Transfer
	{
		
	}
}
