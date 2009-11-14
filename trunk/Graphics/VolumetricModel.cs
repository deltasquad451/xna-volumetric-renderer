#region File Description
//-------------------------------------------------------------------------------------------------
// VolumetricModel.cs
//
// A 3D volumetric model constructed from voxel data.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.Diagnostics;
using Renderer.Input;
#endregion

namespace Renderer.Graphics
{
	/// <summary>
	/// A 3D volumetric model constructed from voxel data.
	/// </summary>
    class VolumetricModel
    {
        #region Fields
        private Texture3D volumeTexture;
        private VertexDeclaration vertexDecl; // TEMP
        private Effect effect; // TEMP

		private List<TransferPoint> colorPoints;
		private List<TransferPoint> alphaPoints;
		private Color[] transferFunc;
        #endregion

        #region Properties
        public Texture3D VolumeTexture
        {
            get { return volumeTexture; }
        }

		/// <summary>
		/// Gets the list of color transfer points used by the transfer functions.
		/// </summary>
		public List<TransferPoint> ColorPoints
		{
			get
			{ return colorPoints; }
		}

		/// <summary>
		/// Gets the list of alpha transfer points used by the transfer functions.
		/// </summary>
		public List<TransferPoint> AlphaPoints
		{
			get
			{ return alphaPoints; }
		}
        #endregion

        #region Initialization
        public VolumetricModel()
        {
            // TEMP
            vertexDecl = new VertexDeclaration(VolumetricRenderer.Game.GraphicsDevice,
                                    VertexPositionColor.VertexElements);

            VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = vertexDecl;
            effect = VolumetricRenderer.Game.Content.Load<Effect>("Shaders/effects");
            // TEMP - end

            // For now, just initialize this to teapot volume values
            volumeTexture = new Texture3D(VolumetricRenderer.Game.GraphicsDevice, 256, 256, 178, 0,
                                    TextureUsage.Linear, SurfaceFormat.Single);
            RawFileReader tempFileReader = new RawFileReader();
            tempFileReader.Open("..\\..\\..\\BostonTeapot.raw");
            tempFileReader.GetRawData(volumeTexture);
            tempFileReader.Close();

			colorPoints = new List<TransferPoint>();
			alphaPoints = new List<TransferPoint>();
        }
		#endregion

		#region Update
		public void Update(GameTime gameTime)
        {
            // TODO
        }
		#endregion

		#region Draw
		public void Draw(GameTime gameTime)
        {
            // TEMP - draw a triangle to test the camera
            VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xView"].SetValue(VolumetricRenderer.Game.Camera.viewMat);
            effect.Parameters["xProjection"].SetValue(VolumetricRenderer.Game.Camera.projectionMat);
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            int points = 8;
            VertexPositionColor[] pointList = new VertexPositionColor[points];

            for (int x = 0; x < points / 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    pointList[(x * 2) + y] = new VertexPositionColor(
                        new Vector3(x * 5, y * 5, 2), Color.DeepPink);
                }
            }

            short[] triangleStripIndices = new short[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

            VolumetricRenderer.Game.GraphicsDevice.Clear(Color.Black);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = vertexDecl;
                VolumetricRenderer.Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleStrip,
                    pointList,
                    0,  // vertex buffer offset to add to each element of the index buffer
                    8,  // number of vertices to draw
                    triangleStripIndices,
                    0,  // first index element to read
                    6   // number of primitives to draw
                );
                pass.End();
            }
            effect.End();
            // TEMP - end
        }
        #endregion

		#region Methods
		/// <summary>
		/// Creates a transfer function from volume's color and alpha transfer points.
		/// </summary>
		public void CreateTransferFunction()
		{
			transferFunc = Transfer.CreateTransferFunction(colorPoints, alphaPoints);
		}
		#endregion
	}
}
