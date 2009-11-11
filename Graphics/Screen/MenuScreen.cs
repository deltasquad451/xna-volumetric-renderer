#region File Description
//-------------------------------------------------------------------------------------------------
// MenuScreen.cs
//
// Base class for screens that contain a menu of options.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Renderer.Diagnostics;
using Renderer.Input;
using Renderer.Utility;
#endregion

namespace Renderer.Graphics.Screen
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
	/// Base class for screens that contain a menu of options. 'T' must be of type EventArgs, or a 
	/// derivation thereof, which represents the arguments passed to the event handlers of MenuEntry
	/// instances used in this menu.
	/// </summary>
	public abstract class MenuScreen : GameScreen
	{
		#region Fields
		private static ContentManager menuContent;
		private static SpriteFontEx menuFontEx;
		private static bool contentLoaded = false;

		private SpriteBatch spriteBatch;
		private List<MenuEntry> menuEntries;
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
		/// Gets the SpriteBatch used by this menu screen.
		/// </summary>
		public SpriteBatch SpriteBatch
		{
			get
			{ return spriteBatch; }
		}

		/// <summary>
		/// Gets the list of menu entries.
		/// </summary>
		public List<MenuEntry> MenuEntries
		{
			get
			{ return menuEntries; }
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
			: base()
		{
			Debug.Assert(contentLoaded);

			menuEntries = new List<MenuEntry>();
			highlightedEntry = 0;
		}

		/// <param name="spriteBatch">The SpriteBatch instance this menu screen will use.</param>
		public MenuScreen(SpriteBatch spriteBatch)
			: base()
		{
			Debug.Assert(contentLoaded);

			this.spriteBatch = spriteBatch;
			menuEntries = new List<MenuEntry>();
			highlightedEntry = 0;
		}

		public override void LoadContent()
		{
			base.LoadContent();

			if (spriteBatch == null)
				spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
		}

		/// <summary>
		/// Loads all the shared content for MenuScreen instances. This must be called before any
		/// MenuScreen instances are created.
		/// </summary>
		public static void LoadSharedContent()
		{
			Debug.Assert(!contentLoaded);

			menuContent = new ContentManager(VolumetricRenderer.Game.Services, "Content\\Screen");
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

			for (int i = 0; i < menuEntries.Count; ++i)
				menuEntries[i].Update(gameTime, this, (i == highlightedEntry ? true : false));
		}

		/// <summary>
		/// Handler for the input to this screen. This is called only when this screen has focus.
		/// </summary>
		/// <param name="input">The InputState instance to read input from.</param>
		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.MenuUp())
				if (--highlightedEntry < 0)
					highlightedEntry = menuEntries.Count - 1;

			if (input.MenuDown())
				if (++highlightedEntry > menuEntries.Count - 1)
					highlightedEntry = 0;

			if (input.MenuSelect())
				menuEntries[highlightedEntry].OnSelectEntry(new MenuEventArgs(highlightedEntry));
		}
		#endregion

		#region Draw
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			for (int i = 0; i < menuEntries.Count; ++i)
				menuEntries[i].Draw(gameTime, this, (i == highlightedEntry ? true : false));
		}
		#endregion
	}
}
