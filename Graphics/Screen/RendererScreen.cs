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

			isoValue = 30;
			alphaValue = 0.02f;
			range = 130;
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

			volumetricModel.drawWireframeBox = true;
			Debug.Execute(delegate() { volumetricModel.drawWireframeBox = true; });

			volumetricModel.TransferPoints = new TransferControlPoints(3, 6);

			// Add the color transfer points.
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 0);
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 1);
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 255);

			//// Add the alpha transfer points.
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue - 2));
			volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, isoValue);
			volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, (byte)(isoValue + range));
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue + range + 2));
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 255);

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
				input.IsKeyPressed(Keys.D1) || input.IsKeyPressed(Keys.D2) ||
				input.IsKeyPressed(Keys.D4) || input.IsKeyPressed(Keys.D5))
			{
				if (input.IsKeyPressed(Keys.D1))
				{
					isoValue = 30;
					alphaValue = 0.02f;
					range = 130;
				}
				else if (input.IsKeyPressed(Keys.D2))
				{
					isoValue = 30;
					alphaValue = 0.03f;
					range = 30;
				}
				else if (input.IsKeyPressed(Keys.D4))
				{
					isoValue = 60;
					alphaValue = 0.998f;
					range = 50;
				}
				else if (input.IsKeyPressed(Keys.D5))
				{
					isoValue = 130;
					alphaValue = 0.998f;
					range = 90;
				}
				else if (isoValue == 0)
					return;
				else if (input.IsKeyDown(Keys.OemPlus) && isoValue < 252 - range)
					isoValue++;
				else if (input.IsKeyDown(Keys.OemMinus) && isoValue > 3)
					isoValue--;
				else if (input.IsKeyDown(Keys.OemCloseBrackets) && alphaValue < 0.997f)
					alphaValue += 0.002f;
				else if (input.IsKeyDown(Keys.OemOpenBrackets) && alphaValue > 0.003f)
					alphaValue -= 0.002f;
				else if (input.IsKeyDown(Keys.OemQuotes) && range < 252 - isoValue)
					range++;
				else if (input.IsKeyDown(Keys.OemSemicolon) && range > 1)
					range--;

				volumetricModel.TransferPoints = new TransferControlPoints(3, 6);

				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 0);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 1);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 255);

				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue - 2));
				volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, isoValue);
				volumetricModel.TransferPoints.AddAlphaControlPoint(alphaValue, (byte)(isoValue + range));
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, (byte)(isoValue + range + 2));
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 255);

				volumetricModel.CreateTransferFunction();
			}

			// Transfer function testing.
			if (input.IsKeyPressed(Keys.D3))
			{
				isoValue = 0;
				alphaValue = 0f;
				range = 0;

				volumetricModel.TransferPoints = new TransferControlPoints(4, 6);

				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Blue, 0);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Blue, 58);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 62);
				volumetricModel.TransferPoints.AddRGBControlPoint(Color.Red, 255);

				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 28);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0.03f, 32);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0.02f, 130);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0.1f, 135);
				volumetricModel.TransferPoints.AddAlphaControlPoint(0.1f, 255);

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
			spriteBatch.DrawString(font, "Isovalues: " + isoValue.ToString() + " - " + (isoValue + range).ToString(), new Vector2(15f, 5f), Color.White);
			spriteBatch.DrawString(font, "Alpha: " + alphaValue.ToString(), new Vector2(15f, 40f), Color.White);
			spriteBatch.DrawString(font, "Range: " + range.ToString(), new Vector2(15f, 75f), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
		#endregion
	}
}
