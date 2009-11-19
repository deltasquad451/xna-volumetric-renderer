#region File Description
//-------------------------------------------------------------------------------------------------
// LoadingScreen.cs
//
// Displays a loading screen while waiting for another screen to finish loading.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.Diagnostics;
#endregion

namespace Renderer.Graphics.Screen
{
	public class LoadingScreen : MenuScreen
	{
		#region Fields
		private GameScreen screenToLoad;
		#endregion

		#region Initialization
		public LoadingScreen(GameScreen screenToLoad, TimeSpan otherTransOffTime)
			: base(VolumetricRenderer.Game.ScreenManager.SpriteBatch)
		{
			this.screenToLoad = screenToLoad;

			if (otherTransOffTime.TotalSeconds > 0.25)
				TransitionOnTime = otherTransOffTime;
			else
				TransitionOnTime = TimeSpan.FromSeconds(0.25);
		}
		#endregion

		#region Update
		public override void Update(GameTime gameTime, bool hasFocus, bool isObscured)
		{
			base.Update(gameTime, hasFocus, isObscured);

			if (TransitionProgress == 1f)
			{
				ScreenManager.RemoveAllScreens();
				ScreenManager.AddScreen(screenToLoad);
			}
		}
		#endregion

		#region Draw
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			
			SpriteBatch.Begin();
			SpriteBatch.DrawString(MenuFontEx.font, "Loading...", new Vector2(560f, 400f), 
				new Color(Color.White, TransitionAlpha), 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
			SpriteBatch.End();
		}
		#endregion
	}
}
