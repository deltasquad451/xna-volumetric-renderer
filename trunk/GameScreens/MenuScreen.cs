#region File Description
//-------------------------------------------------------------------------------------------------
// MenuScreen.cs
//
// Base class for screens that contain a menu of options.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
using Engine.Utility;
using Graphics.Diagnostics;
#endregion

namespace Graphics.GameScreens
{
	/// <summary>
	/// Argument class used by MenuScreen and MenuEntry for events.
	/// </summary>
	public class MenuEventArgs : EventArgs
	{
		public int number; //testing

		public MenuEventArgs(int number)
		{
			this.number = number;
		}
	}

	/// <summary>
	/// Base class for screens that contain a menu of options. This screen and instances of 
	/// MenuEntry used by this screen use type MenuEventArgs for 'T'.
	/// </summary>
	public class MenuScreen : Engine.Screen.MenuScreen<MenuEventArgs>
	{
		#region Fields
		private static ContentManager menuContent;
		private static SpriteFontEx menuFontEx;
		private static bool contentLoaded = false;

		private int highlightedEntry;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the content manager shared by all MenuScreen screens.
		/// </summary>
		protected static ContentManager MenuContent
		{
			get
			{ return menuContent; }
		}

		/// <summary>
		/// Gets the menu font data shared among all MenuScreen screens.
		/// </summary>
		protected static SpriteFontEx MenuFontEx
		{
			get
			{ return menuFontEx; }
		}

		/// <summary>
		/// Gets the derived screen manager used by the game.
		/// </summary>
		protected ScreenManager ScreenManager_thisGame
		{
			get
			{ return (global::Graphics.ScreenManager)ScreenManager; }
		}

		/// <summary>
		/// Gets the index of the currently-highlighted menu entry.
		/// </summary>
		protected int HighlightedEntry
		{
			get
			{ return highlightedEntry; }
		}
		#endregion

		#region Initialization
		public MenuScreen()
			: base(VolumetricRenderer.Game.ScreenManager.SpriteBatch)
		{
			Debug.Assert(contentLoaded);

			highlightedEntry = 0;
		}

		/// <summary>
		/// Loads all the shared content for MenuScreen instances. This must be called before any
		/// MenuScreen instances are created.
		/// </summary>
		public static void LoadSharedContent()
		{
			Debug.Assert(!contentLoaded);

			menuContent = new ContentManager(VolumetricRenderer.Game.Services, "Content\\GameScreens");
			menuFontEx.font = menuContent.Load<SpriteFont>("menufont");
			menuFontEx.width = 0; // Not a fixed-width font.
			contentLoaded = true;
		}

		/// <summary>
		/// Unloads all the shared content for MenuScreen instances. This should be called when
		/// there are no longer any MenuScreen instances active.
		/// </summary>
		public static void UnloadSharedContent()
		{
			Debug.Assert(contentLoaded);

			menuContent.Unload();
			contentLoaded = false;
		}
		#endregion

		#region Update
		/// <param name="hasFocus">Indicates if this screen has focus.</param>
		/// <param name="isObscured">Indicates if this screen is completely obscured by another screen.</param>
		public override void Update(GameTime gameTime, bool hasFocus, bool isObscured)
		{
			base.Update(gameTime, hasFocus, isObscured);

			for (int i = 0; i < MenuEntries.Count; ++i)
				MenuEntries[i].Update(gameTime, this, (i == highlightedEntry ? true : false));
		}

		/// <summary>
		/// Handler for the input to this screen. This is called only when this screen has focus.
		/// </summary>
		/// <param name="input">The InputState instance to read input from.</param>
		public override void HandleInput(Engine.Input.InputState input)
		{
			base.HandleInput(input);

			InputState _input = input as InputState;

			if (_input.MenuUp())
				if (--highlightedEntry < 0)
					highlightedEntry = MenuEntries.Count - 1;

			if (_input.MenuDown())
				if (++highlightedEntry > MenuEntries.Count - 1)
					highlightedEntry = 0;

			if (_input.MenuSelect())
				MenuEntries[highlightedEntry].OnSelectEntry(new MenuEventArgs(highlightedEntry));
		}
		#endregion

		#region Draw
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			for (int i = 0; i < MenuEntries.Count; ++i)
				MenuEntries[i].Draw(gameTime, this, (i == highlightedEntry ? true : false));
		}
		#endregion
	}
}
