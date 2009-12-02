#region File Description
//-------------------------------------------------------------------------------------------------
// RendererScreen.cs
//
// The GameScreen instance that does the actual volumetric modeling.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Renderer.Diagnostics;
using Renderer.Input;
#endregion

namespace Renderer.Graphics.Screen
{
	/// <summary>
	/// Does the actual volumetric modeling.
	/// </summary>
	public class RendererScreen : GameScreen
	{
		#region Fields
		private ContentManager rendererContent;

		private SpriteFont font;
		private SpriteBatch spriteBatch;
        private VolumetricModel volumetricModel;

		private byte isoValue;
		private float alphaValue;
		private int range;

        private String volumeFile = "BostonTeapot.raw";
        private Vector3 volumeFileSize = new Vector3(256, 256, 178);
		#endregion

        #region Properties
        #endregion

        #region Initialization
        public RendererScreen()
			: base()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.25);
            VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;

			isoValue = 40;
			alphaValue = 0.1f;
			range = 90;
		}

		public override void LoadContent()
		{
			base.LoadContent();

			rendererContent = new ContentManager(ScreenManager.Game.Services, "Content\\Screen");
			font = rendererContent.Load<SpriteFont>("menufont");
			spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            LoadVolumeData();
		}

        public void LoadVolumeData()
        {
            volumetricModel = new VolumetricModel(VolumetricRenderer.Game, "..\\..\\..\\" + volumeFile, (int)volumeFileSize.X, (int)volumeFileSize.Y, (int)volumeFileSize.Z);
            volumetricModel.effectAssetName = "..\\..\\Shaders\\effects";
            volumetricModel.StepScale = 1.4f; //1.4f gives us a ray through the whole volume when viewed along a diagonal
            volumetricModel.scale = 5.0f;

			Debug.Execute(delegate() { volumetricModel.drawWireframeBox = true; });

			volumetricModel.TransferPoints = new TransferControlPoints(10, 10);

			// Add the color transfer points.
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 0);
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 1);
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 255);

			// Add the alpha transfer points.
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue - 2));
			volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, isoValue);
			volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, (byte)(isoValue + range));
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue + range + 2));
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 255);
			//volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);
			//volumetricModel.TransferPoints.AddAlphaControlPoint(0.1f, 75);
			//volumetricModel.TransferPoints.AddAlphaControlPoint(1.0f, 125);
			//volumetricModel.TransferPoints.AddAlphaControlPoint(0.1f, 175);
			//volumetricModel.TransferPoints.AddAlphaControlPoint(0.09f, 255);

			// Create the transfer function.
			volumetricModel.CreateTransferFunction();

            VolumetricRenderer.Game.Components.Add(volumetricModel);

			// The volumetric model takes awhile to load (mostly due to VolumetricModel.CreateTextureData()) 
			// so reset the elapsed time so that our screen transition works properly.
			VolumetricRenderer.Game.ResetElapsedTime();
        }

		public override void UnloadContent()
		{
			rendererContent.Unload();

			base.UnloadContent();
		}
		#endregion

		#region Update
		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.IsKeyPressed(Keys.Escape))
			{
				VolumetricRenderer.Game.Components.Remove(volumetricModel);

				// Reload the main menu and return.
				ScreenManager.AddScreen(new BackgroundScreen());
				ScreenManager.AddScreen(new MainMenuScreen());
				Finished();
			}

            // Volumetric Model
            if (input.IsKeyPressed(Keys.OemTilde))
            {
                if (volumeFile.Contains("skull"))
                {
                    volumeFile = "BostonTeapot.raw";
                    volumeFileSize = new Vector3(256, 256, 178);
                }
                else
                {
                    volumeFile = "skull.raw";
                    volumeFileSize = new Vector3(256, 256, 256);
                }

                VolumetricRenderer.Game.Components.Remove(volumetricModel);
                LoadVolumeData();
            }

            // Lighting
            if (input.IsKeyPressed(Keys.D8))
            {
                volumetricModel.ToggleLighting1();
            }
            else if (input.IsKeyPressed(Keys.D9))
            {
                volumetricModel.ToggleLighting2();
            }
            else if (input.IsKeyPressed(Keys.D0))
            {
                volumetricModel.ToggleLighting3();
            }

			// Transfer function testing.
            if (input.IsKeyPressed(Keys.D7))
            {
                volumetricModel.ToggleTransferFunction();
            }
			else if (input.IsKeyDown(Keys.OemPlus) || input.IsKeyDown(Keys.OemMinus) ||
				input.IsKeyDown(Keys.OemCloseBrackets) || input.IsKeyDown(Keys.OemOpenBrackets) ||
				input.IsKeyDown(Keys.OemQuotes) || input.IsKeyDown(Keys.OemSemicolon) || 
				input.IsKeyPressed(Keys.D1) || input.IsKeyPressed(Keys.D2) || input.IsKeyPressed(Keys.D3))
			{
				if (input.IsKeyDown(Keys.OemPlus) && isoValue < 252 - range)
					isoValue++;
				else if (input.IsKeyDown(Keys.OemMinus) && isoValue > 3)
					isoValue--;
				else if (input.IsKeyPressed(Keys.OemCloseBrackets) && alphaValue < 0.94f)
					alphaValue += 0.05f;
				else if (input.IsKeyPressed(Keys.OemOpenBrackets) && alphaValue > 0.06f)
					alphaValue -= 0.05f;
				else if (input.IsKeyDown(Keys.OemQuotes) && range < 252 - isoValue)
					range++;
				else if (input.IsKeyDown(Keys.OemSemicolon) && range > 1)
					range--;
				else if (input.IsKeyPressed(Keys.D1))
				{
					isoValue = 40;
					alphaValue = 0.1f;
					range = 90;
				}
				else if (input.IsKeyPressed(Keys.D2))
				{
					isoValue = 60;
					alphaValue = 0.1f;
					range = 50;
				}
				else if (input.IsKeyPressed(Keys.D3))
				{
					isoValue = 60;
					alphaValue = 0.25f;
					range = 20;
				}

				volumetricModel.TransferPoints = new TransferControlPoints(10, 10);

				// Add the color transfer points.
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 0);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 1);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 255);

				// Add the alpha transfer points.
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue - 2));
				volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, isoValue);
				volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, (byte)(isoValue + range));
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue + range + 2));
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 255);

				// Create the transfer function.
				volumetricModel.CreateTransferFunction();
			}
		}
		#endregion

        #region Draw
        public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.DrawString(font, "ESC - Exit Renderer", new Vector2(10f, 935f), 
				new Color(Color.Yellow, TransitionAlpha), 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
			spriteBatch.DrawString(font, "Isovalue: " + isoValue.ToString(), new Vector2(10f, 5f), Color.White);
			spriteBatch.DrawString(font, "Alpha: " + alphaValue.ToString(), new Vector2(10f, 40f), Color.White);
			spriteBatch.DrawString(font, "Range: " + range.ToString(), new Vector2(10f, 75f), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
		#endregion
	}
}
