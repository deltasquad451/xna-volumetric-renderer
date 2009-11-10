#region File Description
//-------------------------------------------------------------------------------------------------
// MainMenuScreen.cs
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
	public class MainMenuScreen : MenuScreen
	{
		#region Initialization
		public MainMenuScreen()
			: base()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.25);

			SpriteFont font = MenuFontEx.font;

			MenuEntry entry1 = new MenuEntry("Launch Renderer", font, new Vector2(530f, 400f));
			entry1.Selected += new EventHandler<MenuEventArgs>(LaunchRenderer_Selected);
			MenuEntries.Add(entry1);

			MenuEntry entry2 = new MenuEntry("Exit App", font, new Vector2(590f, (float)font.LineSpacing + entry1.Position.Y));
			entry2.Selected += new EventHandler<MenuEventArgs>(ExitApp_Selected);
			MenuEntries.Add(entry2);
		}
		#endregion

		#region Events
		private void LaunchRenderer_Selected(object sender, MenuEventArgs args)
		{
			ScreenManager.RemoveAllScreens();
			ScreenManager.AddScreen(new RendererScreen());
		}

		private void ExitApp_Selected(object sender, MenuEventArgs args)
		{
			ScreenManager.Game.Exit();
		}
		#endregion

		#region Draw
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			Color color = new Color(Color.White, TransitionAlpha);

			SpriteBatch.Begin();
			SpriteBatch.DrawString(MenuFontEx.font, "CS580 Final Project", new Vector2(375f, 80f), color, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
			SpriteBatch.DrawString(MenuFontEx.font, "Volumetric Renderer", new Vector2(370f, 150f), color, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
			SpriteBatch.DrawString(MenuFontEx.font, "Brandon Booth - Jorge Garrido - Evan Hatch - Brian Valdillez", new Vector2(200f, 870f), color);
			SpriteBatch.End();
		}
		#endregion
	}
}
