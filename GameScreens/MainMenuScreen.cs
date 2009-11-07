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

			MenuEntry test1 = new MenuEntry("Test - Launch Renderer", font, new Vector2(10.0f, 0.0f));
			test1.Selected += new EventHandler<MenuEventArgs>(test1_Selected);
			MenuEntries.Add(test1);

			MenuEntry test2 = new MenuEntry("Test - Assertion Failure", font, new Vector2(10.0f, (float)font.LineSpacing + test1.Position.Y));
			test2.Selected += new EventHandler<MenuEventArgs>(test2_Selected);
			MenuEntries.Add(test2);

			MenuEntry test3 = new MenuEntry("Test - Exit App", font, new Vector2(10.0f, (float)font.LineSpacing + test2.Position.Y));
			test3.Selected += new EventHandler<MenuEventArgs>(test3_Selected);
			MenuEntries.Add(test3);
		}
		#endregion

		#region Events
		private void test1_Selected(object sender, MenuEventArgs args)
		{
			ScreenManager.AddScreen(new RendererScreen());
		}

		private void test2_Selected(object sender, MenuEventArgs args)
		{
			Debug.Assert(false, "false", "Here's where you can give extra info about the assert. "
				+ "A callstack of the assert is also outputted to the file assert.txt in the bin folder.");
		}

		private void test3_Selected(object sender, MenuEventArgs args)
		{
			VolumetricRenderer.Game.Exit();
		}
		#endregion
	}
}
