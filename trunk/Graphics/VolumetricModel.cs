#region File Description
//-------------------------------------------------------------------------------------------------
// VolumetricModel.cs
//
// A 3D volumetric model constructed from voxel data.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Renderer.Diagnostics;
using Renderer.Input;
#endregion

namespace Renderer.Graphics
{
	/// <summary>
	/// A 3D volumetric model constructed from voxel data.
	/// </summary>
    class VolumetricModel : Renderable
    {
        #region Fields
        public string volumeAssetName { get; set; }
        protected Texture3D volumeTexture { get; set; }
        protected int width;
        protected int height;
        protected int depth;
        //protected VertexDeclaration vertexDecl; // TEMP
        //protected Effect effect; // TEMP
        protected string technique;

		private TransferControlPoints transferPoints;
		private Color[] transferFunc;
        #endregion

        #region Properties
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

        /// <summary>
        /// Gets the 3D volume width.
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// Gets the 3D volume height.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Gets the 3D volume depth.
        /// </summary>
        public int Depth
        {
            get
            {
                return depth;
            }
        }

        /// <summary>
        /// Gets the technique name.
        /// </summary>
        public string Technique
        {
            get
            {
                return technique;
            }
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
		/// <param name="volumentAssetName">Asset name of the volumetric model.</param>
		/// <param name="width">Width of the volume.</param>
		/// <param name="height">Height of the volume.</param>
		/// <param name="depth">Depth of the volume.</param>
        public VolumetricModel(Game game, string volumeAssetName,
                                int width, int height, int depth)
            : base(game)
        {
            /*
            // TEMP
            vertexDecl = new VertexDeclaration(VolumetricRenderer.Game.GraphicsDevice, VertexPositionColor.VertexElements);

            VolumetricRenderer.Game.GraphicsDevice.VertexDeclaration = vertexDecl;
            // TEMP - end
            */
            effectAssetName = "Shaders/effects";
            modelAssetName = "Models/box";
            technique = "RayCast";

            this.volumeAssetName = volumeAssetName;
            this.width = width;
            this.height = height;
            this.depth = depth;

			//colorPoints = new List<TransferPoint>();
			//alphaPoints = new List<TransferPoint>();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            // Create a step size based on the largest side length and create a scale factor
            // for the shader
            float maxSideLength = (float)Math.Max(width, Math.Max(height, depth));
            Vector3 stepSize = new Vector3(1.0f / maxSideLength, 1.0f / maxSideLength, 1.0f / maxSideLength);
            Vector3 scaleFactor = new Vector3(width, height, depth) * stepSize;

            effect.Parameters["StepSize"].SetValue(stepSize);
            effect.Parameters["Iterations"].SetValue((int)maxSideLength * 2.0f);
            effect.Parameters["ScaleFactor"].SetValue(new Vector4(scaleFactor, 1.0f));

			// Get the scaled volume data.
			float[] volumeData;
            RawFileReader tempFileReader = new RawFileReader();
            tempFileReader.Open(volumeAssetName, width, height, depth);
            tempFileReader.GetRawData(out volumeData);
            tempFileReader.Close();

			// Compute the gradient at each voxel and combine them with the isovalues.
			HalfVector4[] textureData;
			CreateTextureData(volumeData, out textureData);

			// Set the data into our Texture3D object, for use in the shader.
			volumeTexture = new Texture3D(VolumetricRenderer.Game.GraphicsDevice, width, height, depth, 0,
				TextureUsage.None, SurfaceFormat.HalfVector4);
			volumeTexture.SetData<HalfVector4>(textureData);
        }

		/// <summary>
		/// Computes the gradients at each voxel using the 3D Central Differences method and places 
		/// them together with the isovalues. This method will try to load a gradient file for the
		/// asset if it exists; if it doesn't, it does the gradient computations and saves it to a file.
		/// </summary>
		/// <param name="volumeData">Scaled volume data of the model.</param>
		/// <param name="textureData">Texture data that is returned.</param>
		private void CreateTextureData(float[] volumeData, out HalfVector4[] textureData)
		{
			Vector3[] volumeGradients = new Vector3[width * height * depth];

			string ext = volumeAssetName.Substring(volumeAssetName.LastIndexOf('.'));
			string gradientFile = volumeAssetName.Replace(ext, ".grad");

			if (File.Exists(gradientFile))
			{
				// Gradient file already exists, so load it.
				FileStream file = new FileStream(gradientFile, FileMode.Open);
				BinaryReader reader = new BinaryReader(file);

				int index = 0;
				for (int i = 0; i < volumeGradients.Length; ++i)
				{
					Vector3 gradient;
					gradient.X = reader.ReadSingle();
					gradient.Y = reader.ReadSingle();
					gradient.Z = reader.ReadSingle();

					volumeGradients[index++] = gradient;
				}

				reader.Close();
				file.Close();
			}
			else
			{
				// Gradient file doesn't exist, so do the computations.
				int index = 0;
				for (int z = 0; z < depth; ++z)
					for (int y = 0; y < height; ++y)
						for (int x = 0; x < width; ++x)
						{
							// Get the "previous" data.
							Vector3 v1;
							v1.X = GetVolumeDataBounded(volumeData, x - 1, y, z);
							v1.Y = GetVolumeDataBounded(volumeData, x, y - 1, z);
							v1.Z = GetVolumeDataBounded(volumeData, x, y, z - 1);

							// Get the "next" data.
							Vector3 v2;
							v2.X = GetVolumeDataBounded(volumeData, x + 1, y, z);
							v2.Y = GetVolumeDataBounded(volumeData, x, y + 1, z);
							v2.Z = GetVolumeDataBounded(volumeData, x, y, z + 1);

							// Compute the gradient.
							volumeGradients[index] = v2 - v1;
							if (volumeGradients[index] != Vector3.Zero)
								volumeGradients[index] = Vector3.Normalize(volumeGradients[index]);

							Debug.Assert(!float.IsNaN(volumeGradients[index].X));
							index++;
						}

				// Save the gradients in binary format to a file so we don't have to recompute them again.
				FileStream file = new FileStream(gradientFile, FileMode.Create);
				BinaryWriter writer = new BinaryWriter(file);

				for (int i = 0; i < volumeGradients.Length; ++i)
				{
					writer.Write(volumeGradients[i].X);
					writer.Write(volumeGradients[i].Y);
					writer.Write(volumeGradients[i].Z);
				}

				writer.Close();
				file.Close();
			}

			// Pack the gradients and isovalues into a 16-bit Vector4 (accuracy should be sufficient, 
			// but with half the memory usage).
			textureData = new HalfVector4[width * height * depth];
			for (int i = 0; i < textureData.Length; ++i)
				textureData[i] = new HalfVector4(volumeGradients[i].X, volumeGradients[i].Y, volumeGradients[i].Z, volumeData[i]);
		}

		/// <summary>
		/// Gets the volume data at the specified x,y,z point, making sure that the point stays 
		/// within the bounds of the volume.
		/// </summary>
		private float GetVolumeDataBounded(float[] volumeData, int x, int y, int z)
		{
			x = (int)MathHelper.Clamp(x, 0, width - 1);
			y = (int)MathHelper.Clamp(y, 0, height - 1);
			z = (int)MathHelper.Clamp(z, 0, depth - 1);

			return volumeData[x + (y * width) + (z * width * height)];
		}
		#endregion

		#region Update
		public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            VolumetricRenderer.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }
		#endregion

		#region Draw
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            /*
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
            */
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
