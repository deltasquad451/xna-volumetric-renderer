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
		private Texture2D background;

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
		}

		public override void LoadContent()
		{
			base.LoadContent();

			rendererContent = new ContentManager(ScreenManager.Game.Services, "Content\\Screen");
			background = rendererContent.Load<Texture2D>("renderer");
			font = rendererContent.Load<SpriteFont>("menufont");
			spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            volumetricModel = new VolumetricModel();

			// TODO: Experimentally determine the real transfer points when the model is finally visible.
			// Add the color transfer points.
			volumetricModel.ColorPoints.Add(new TransferPoint(Color.Black, 0));		// TEMP
			volumetricModel.ColorPoints.Add(new TransferPoint(Color.White, 255));	// TEMP

			// TODO: Experimentally determine the real transfer points when the model is finally visible.
			// Add the alpha transfer points.
			volumetricModel.AlphaPoints.Add(new TransferPoint(0f, 0));				// TEMP
			volumetricModel.AlphaPoints.Add(new TransferPoint(0.5f, 100));			// TEMP
			volumetricModel.AlphaPoints.Add(new TransferPoint(0.8f, 200));			// TEMP
			volumetricModel.AlphaPoints.Add(new TransferPoint(0f, 255));			// TEMP

			// Create the transfer function.
			volumetricModel.CreateTransferFunction();
		}

		public override void UnloadContent()
		{
			rendererContent.Unload();

			base.UnloadContent();
		}
		#endregion

		#region Update
		public override void Update(GameTime gameTime, bool hasFocus, bool isObscured)
		{
            if (hasFocus && !isObscured)
            {
                volumetricModel.Update(gameTime);
            }

			base.Update(gameTime, hasFocus, isObscured);
		}

		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape, PlayerIndex.One))
			{
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
//			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
//          Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
//			byte alpha = TransitionAlpha;

//			spriteBatch.Begin();
//			spriteBatch.Draw(background, fullscreen, new Color(alpha, alpha, alpha));
//			spriteBatch.DrawString(font, "Here's where our awesome renderer will be showcased!", new Vector2(250f, 450f), new Color(Color.White, alpha));
//			spriteBatch.DrawString(font, "ESC - Exit Renderer", new Vector2(10f, 915f), new Color(Color.Yellow, alpha));
//			spriteBatch.End();

            volumetricModel.Draw(gameTime);

			base.Draw(gameTime);
		}
		#endregion
	}
}
