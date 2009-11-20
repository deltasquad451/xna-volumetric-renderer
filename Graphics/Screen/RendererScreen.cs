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
		#endregion

        #region Properties
        #endregion

        #region Initialization
        public RendererScreen()
			: base()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.25);
            VolumetricRenderer.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
		}

		public override void LoadContent()
		{
			base.LoadContent();

			rendererContent = new ContentManager(ScreenManager.Game.Services, "Content\\Screen");
			font = rendererContent.Load<SpriteFont>("menufont");
			spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            volumetricModel = new VolumetricModel(VolumetricRenderer.Game, "..\\..\\..\\BostonTeapot.raw", 256, 256, 178);
            volumetricModel.effectAssetName = "..\\..\\Shaders\\effects";
            volumetricModel.StepScale = 0.5f;
            volumetricModel.scale = 5.0f;

			volumetricModel.drawWireframeBox = false;
			Debug.Execute(delegate() { volumetricModel.drawWireframeBox = true; });

//#if DEBUG
//            volumetricModel.drawWireframeBox = true;
//#else
//            volumetricModel.drawWireframeBox = false;
//#endif
			volumetricModel.TransferPoints = new TransferControlPoints(3, 4);

			// TODO: Experimentally determine the real transfer points when the model is finally visible.
			// Add the color transfer points.
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Black, 0);		// TEMP
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.Aqua, 100);		// TEMP
			volumetricModel.TransferPoints.AddRGBControlPoint(Color.White, 255);	// TEMP

			// TODO: Experimentally determine the real transfer points when the model is finally visible.
			// Add the alpha transfer points.
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 0);				// TEMP
			volumetricModel.TransferPoints.AddAlphaControlPoint(0.5f, 100);			// TEMP
			volumetricModel.TransferPoints.AddAlphaControlPoint(0.8f, 200);			// TEMP
			volumetricModel.TransferPoints.AddAlphaControlPoint(0f, 255);			// TEMP

			// Create the transfer function.
			volumetricModel.CreateTransferFunction();

            VolumetricRenderer.Game.Components.Add(volumetricModel);

			// The volumetric model takes awhile to load (mostly due to VolumetricModel.CreateTextureData()) 
			// so reset the elapsed time so that our screen transition works properly.
			VolumetricRenderer.Game.ResetElapsedTime();

			//volumetricModel.ColorPoints.Add(new TransferPoint(Color.Black, 0));		// TEMP
			//volumetricModel.ColorPoints.Add(new TransferPoint(Color.Aqua, 100));	// TEMP
			//volumetricModel.ColorPoints.Add(new TransferPoint(Color.White, 255));	// TEMP
			//volumetricModel.AlphaPoints.Add(new TransferPoint(0f, 0));				// TEMP
			//volumetricModel.AlphaPoints.Add(new TransferPoint(0.5f, 100));			// TEMP
			//volumetricModel.AlphaPoints.Add(new TransferPoint(0.8f, 200));			// TEMP
			//volumetricModel.AlphaPoints.Add(new TransferPoint(0f, 255));			// TEMP
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

			if (input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape, PlayerIndex.One))
			{
				VolumetricRenderer.Game.Components.Remove(volumetricModel);

				// Reload the main menu and return.
				ScreenManager.AddScreen(new BackgroundScreen());
				ScreenManager.AddScreen(new MainMenuScreen());
				Finished();
			}
		}
		#endregion

        #region Draw
        public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.DrawString(font, "ESC - Exit Renderer", new Vector2(10f, 935f), 
				new Color(Color.Yellow, TransitionAlpha), 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
			spriteBatch.End();

			base.Draw(gameTime);
		}
		#endregion
	}
}
