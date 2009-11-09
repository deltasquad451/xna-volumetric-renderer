#region File Description
//-------------------------------------------------------------------------------------------------
// BackgroundScreen.cs
//
// A GameScreen instance that sits behind all other menu screens.
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
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// </summary>
    public class BackgroundScreen : MenuScreen
    {
        #region Fields
        private Texture2D background;
        #endregion

        #region Initialization
        public BackgroundScreen()
			: base()
        {
			TransitionOnTime = TimeSpan.FromSeconds(0.25);
			AlwaysVisible = true;
        }

        public override void LoadContent()
        {
			base.LoadContent();

			background = MenuContent.Load<Texture2D>("background");
        }
        #endregion

		#region Draw
        public override void Draw(GameTime gameTime)
        {
			base.Draw(gameTime);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            byte alpha = TransitionAlpha;

            SpriteBatch.Begin();
			SpriteBatch.Draw(background, fullscreen, new Color(alpha, alpha, alpha, 100));
			SpriteBatch.End();
        }
        #endregion
    }
}
