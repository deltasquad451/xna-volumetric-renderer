#region File Description
//-------------------------------------------------------------------------------------------------
// VolumetricModel.cs
//
// A 3D volumetric model constructed from voxel data.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
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
		private string filename;
		private int width;
		private int height;
		private int depth;

		private float[] volumeData;
		private Vector3[] volumeGradients;
        private Texture3D volumeTexture;

		private TransferControlPoints transferPoints;
		private Color[] transferFunc;

        private VertexDeclaration vertexDecl; // TEMP
        private Effect effect;	// TEMP
        #endregion

        #region Properties
        public Texture3D VolumeTexture
        {
            get { return volumeTexture; }
        }

		/// <summary>
		/// Gets or sets the transfer control points used by the transfer function.
		/// </summary>
		public TransferControlPoints TransferPoints
		{
			get
			{ return transferPoints; }

			set
			{ transferPoints = value; }
		}

		///// <summary>
		///// Gets the list of color transfer points used by the transfer functions.
		///// </summary>
		//public List<ColorTransferPoint> ColorPoints
		//{
		//    get
		//    { return colorPoints; }
		//}

		///// <summary>
		///// Gets the list of alpha transfer points used by the transfer functions.
		///// </summary>
		//public List<AlphaTransferPoint> AlphaPoints
		//{
		//    get
		//    { return alphaPoints; }
		//}
        #endregion

        #region Initialization
		/// <param name="filename">Filename of the volumetric model.</param>
		/// <param name="width">Width of the volume.</param>
		/// <param name="height">Height of the volume.</param>
		/// <param name="depth">Depth of the volume.</param>
        public VolumetricModel(string filename, int width, int height, int depth)
        {
			this.filename = filename;
			this.width = width;
			this.height = height;
			this.depth = depth;

			// Load the teapot volume data.
			RawFileReader tempFileReader = new RawFileReader();
			tempFileReader.Open(filename, width, height, depth);
			tempFileReader.GetRawData(out volumeData);
			tempFileReader.Close();

			// Compute the gradients at each voxel.
			ComputeGradients();

			volumeTexture = new Texture3D(VolumetricRenderer.Game.GraphicsDevice, width, height, depth, 1, TextureUsage.None, SurfaceFormat.HalfVector4);
			
			//colorPoints = new List<ColorTransferPoint>();
			//alphaPoints = new List<AlphaTransferPoint>();

            // TEMP
            vertexDecl = new VertexDeclaration(VolumetricRenderer.Game.GraphicsDevice, VertexPositionColor.VertexElements);

            VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = vertexDecl;
            effect = VolumetricRenderer.Game.Content.Load<Effect>("Shaders/effects");
            // TEMP - end
        }

		/// <summary>
		/// Computes the gradients at each voxel using the 3D Central Differences method.
		/// </summary>
		private void ComputeGradients()
		{
			volumeGradients = new Vector3[width * height * depth];

			for (int z = 0; z < depth; ++z)
			{
				for (int y = 0; y < height; ++y)
				{
					for (int x = 0; x < width; ++x)
					{
						// Get the "previous" data.
						Vector3 v1;
						v1.X = GetVolumeDataBounded(x - 1, y, z);
						v1.Y = GetVolumeDataBounded(x, y - 1, z);
						v1.Z = GetVolumeDataBounded(x, y, z - 1);

						// Get the "next" data.
						Vector3 v2;
						v2.X = GetVolumeDataBounded(x + 1, y, z);
						v2.Y = GetVolumeDataBounded(x, y + 1, z);
						v2.Z = GetVolumeDataBounded(x, y, z + 1);
					}
				}
			}
		}

		/// <summary>
		/// Gets the volume data at the specified x,y,z point, making sure that the point stays 
		/// within the bounds of the volume.
		/// </summary>
		private float GetVolumeDataBounded(int x, int y, int z)
		{
			x = (int)MathHelper.Clamp(x, 0, width - 1);
			y = (int)MathHelper.Clamp(y, 0, height - 1);
			z = (int)MathHelper.Clamp(z, 0, depth - 1);

			return volumeData[x + (y * width) + (z * width * height)];
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
			transferFunc = Transfer.CreateTransferFunction(transferPoints);

			// TODO: Convert the color array into a texture for use in a shader?
		}
		#endregion
	}
}
