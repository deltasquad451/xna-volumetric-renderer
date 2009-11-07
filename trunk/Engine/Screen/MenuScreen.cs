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
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
#endregion

namespace Engine.Screen
{
	/// <summary>
	/// Base class for screens that contain a menu of options. 'T' must be of type EventArgs, or a 
	/// derivation thereof, which represents the arguments passed to the event handlers of MenuEntry
	/// instances used in this menu.
	/// </summary>
	public abstract class MenuScreen<T> : GameScreen where T : EventArgs
	{
		#region Fields
		private SpriteBatch spriteBatch;
		private List<MenuEntry<T>> menuEntries;
		#endregion

		#region Properties
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
		public List<MenuEntry<T>> MenuEntries
		{
			get
			{ return menuEntries; }
		}
		#endregion

		#region Initialization
		public MenuScreen()
			: base()
		{
			menuEntries = new List<MenuEntry<T>>();
		}

		/// <param name="spriteBatch">The SpriteBatch instance this menu screen will use.</param>
		public MenuScreen(SpriteBatch spriteBatch)
			: base()
		{
			this.spriteBatch = spriteBatch;
			menuEntries = new List<MenuEntry<T>>();
		}

		public override void LoadContent()
		{
			base.LoadContent();

			if (spriteBatch == null)
				spriteBatch = new SpriteBatch(this.ScreenManager.GraphicsDevice);
		}
		#endregion
	}
}
