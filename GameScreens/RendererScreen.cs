#region File Description
//-------------------------------------------------------------------------------------------------
// RendererScreen.cs
//
// 
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
using Graphics.Diagnostics;
#endregion

namespace Graphics.GameScreens
{
	public class RendererScreen : MenuScreen
	{
		#region Fields
		private Texture2D background;
		#endregion

		#region Initialization
		public RendererScreen()
			: base()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.25);

			SpriteFont font = MenuFontEx.font;

			MenuEntry test1 = new MenuEntry("Test - Exit Renderer", font, new Vector2(10f, 0f));
			test1.Selected += new EventHandler<MenuEventArgs>(test1_Selected);
            MenuEntries.Add(test1);

			VolumetricRenderer.Game.backgroundScreen.AlwaysVisible = false; // temp hack
		}

		public override void LoadContent()
		{
			base.LoadContent();

			background = MenuContent.Load<Texture2D>("renderer");
		}
		#endregion

		#region Events
		private void test1_Selected(object sender, MenuEventArgs args)
		{
			VolumetricRenderer.Game.backgroundScreen.AlwaysVisible = true; // temp hack
			Finished();
		}
		#endregion

		#region Draw
		public override void Draw(GameTime gameTime)
		{
			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
			byte alpha = TransitionAlpha;

			SpriteBatch.Begin();
			SpriteBatch.Draw(background, fullscreen, new Color(alpha, alpha, alpha));
			SpriteBatch.DrawString(MenuFontEx.font, "Here's where our awesome renderer will be showcased!", new Vector2(250f, 450f), new Color(Color.White, alpha));
			SpriteBatch.End();

			base.Draw(gameTime);
		}
		#endregion
	}
}
