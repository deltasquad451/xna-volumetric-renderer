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
        protected RenderTarget2D front2DTex;
        protected RenderTarget2D back2DTex;
        public string volumeAssetName { get; set; }
        protected Texture3D volumeTexture { get; set; }
        protected int width;
        protected int height;
        protected int depth;
        protected float stepScale = 1.0f;
        protected string technique;
        public bool drawWireframeBox { get; set; }

        private static bool is2DTexInitialized = false;

		private TransferControlPoints transferPoints;
		private Color[] transferFunc;
        #endregion

        #region Properties
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
        /// Gets or sets the step scale for the ray cast.
        /// </summary>
        public float StepScale
        {
            get
            {
                return stepScale;
            }
            set
            {
                stepScale = value;
                if (effect != null)
                {
                    float maxSideLength = (float)Math.Max(width, Math.Max(height, depth));
                    Vector3 stepSize = new Vector3(1.0f / width, 1.0f / height, 1.0f / depth);
                    effect.Parameters["StepSize"].SetValue(stepSize * stepScale);
                    effect.Parameters["Iterations"].SetValue((int)maxSideLength * (1.0f / stepScale));
                }
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
		/// <param name="volumentAssetName">Asset name of the volumetric model.</param>
		/// <param name="width">Width of the volume.</param>
		/// <param name="height">Height of the volume.</param>
		/// <param name="depth">Depth of the volume.</param>
        public VolumetricModel(Game game, string volumeAssetName,
                                int width, int height, int depth)
            : base(game)
        {
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

            effect.Parameters["StepSize"].SetValue(stepSize);
            effect.Parameters["Iterations"].SetValue((int)maxSideLength * (1.0f / stepScale) * 2.0f);

            Vector3 sizes = new Vector3(width, height, depth);
            Vector3 scaleFactor = Vector3.One / ((Vector3.One * maxSideLength) / (sizes * Vector3.One));
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
            //volumeTexture = new Texture3D(VolumetricRenderer.Game.GraphicsDevice, width, height, depth, 0,
            //    TextureUsage.None, SurfaceFormat.Single);
            //volumeTexture.SetData(volumeData);
            effect.Parameters["Volume"].SetValue(volumeTexture);
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

            //set the technique to draw positions
            effect.CurrentTechnique = effect.Techniques["RenderPosition"];

            VolumetricRenderer.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;

            //draw front faces
            //draw the pixel positions to the texture
            VolumetricRenderer.Game.GraphicsDevice.SetRenderTarget(0, front2DTex);
            VolumetricRenderer.Game.GraphicsDevice.Clear(Color.Black);

            base.DrawCustomEffect();

            VolumetricRenderer.Game.GraphicsDevice.SetRenderTarget(0, null);

            //draw back faces
            //draw the pixel positions to the texture
            VolumetricRenderer.Game.GraphicsDevice.SetRenderTarget(0, back2DTex);
            VolumetricRenderer.Game.GraphicsDevice.Clear(Color.Black);
            VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            base.DrawCustomEffect();

            VolumetricRenderer.Game.GraphicsDevice.SetRenderTarget(0, null);
            VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
        }
		#endregion

		#region Draw
        protected override void DrawCustomEffect()
        {
            if (!is2DTexInitialized)
            {
                // BB - The 2D textures have to be initialized here because of the multithreading
                // going on.  C# throws an exception when trying to access a UI element in a
                // different thread than it was created.  Since the draw is running on our own
                // thread and 2DTextures are UI elements, this causes problems.
                PresentationParameters pp = Game.GraphicsDevice.PresentationParameters;
                SurfaceFormat format = pp.BackBufferFormat;
                MultiSampleType msType = pp.MultiSampleType;
                int msQuality = pp.MultiSampleQuality;

                //create the front and back position textures
                //check to make sure that there is a sutiable format supported
                SurfaceFormat rtFormat = SurfaceFormat.HalfVector4;
                if (isFormatSupported(SurfaceFormat.HalfVector4))
                {
                    rtFormat = SurfaceFormat.HalfVector4;
                }
                else if (isFormatSupported(SurfaceFormat.Vector4))
                {
                    rtFormat = SurfaceFormat.Vector4;
                }
                else if (isFormatSupported(SurfaceFormat.Rgba64))
                {
                    rtFormat = SurfaceFormat.Rgba64;
                }
                else //no suitable format found
                {
                    Debug.Assert(false, "Hardware must be SM 3.0 compliant and support RGBA16F, RGBA32F, or RGBA64. Error creating position render targets");
                    Game.Exit();
                }

                front2DTex = new RenderTarget2D(VolumetricRenderer.Game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                            1, rtFormat, msType, msQuality);
                back2DTex = new RenderTarget2D(VolumetricRenderer.Game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                            1, rtFormat, msType, msQuality);
                is2DTexInitialized = true;
            }
            else
            {

                //Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                VolumetricRenderer.Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                VolumetricRenderer.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                //draw wireframe
                if (drawWireframeBox)
                {
                    VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
                    VolumetricRenderer.Game.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

                    effect.CurrentTechnique = effect.Techniques["WireFrame"];
                    base.DrawCustomEffect();

                    VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                    VolumetricRenderer.Game.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
                }

                effect.CurrentTechnique = effect.Techniques[technique];
                effect.Parameters["Front"].SetValue(front2DTex.GetTexture());
                effect.Parameters["Back"].SetValue(back2DTex.GetTexture());

                base.DrawCustomEffect();
            }
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

        private bool isFormatSupported(SurfaceFormat format)
        {
            return VolumetricRenderer.Game.GraphicsDevice.CreationParameters.Adapter.CheckDeviceFormat(
                                         VolumetricRenderer.Game.GraphicsDevice.CreationParameters.DeviceType,
                                         VolumetricRenderer.Game.GraphicsDevice.DisplayMode.Format,
                                         TextureUsage.None,
                                         QueryUsages.None,
                                         ResourceType.RenderTarget,
                                         format);
        }
		#endregion
	}
}
