#region File Description
//-------------------------------------------------------------------------------------------------
// MenuEntry.cs
//
// A single entry in a MenuScreen instance.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
#endregion

namespace Engine.Screen
{
	/// <summary>
	/// A single entry in a MenuScreen instance. 'T' must be of type EventArgs, or a derivation thereof,
	/// which represents the arguments passed to the event handlers.
	/// </summary>
	public class MenuEntry<T> where T : EventArgs
	{
		#region Fields
		private string text;
		private SpriteFont font;
		private Vector2 position;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the text of this menu entry.
		/// </summary>
		public string Text
		{
			get
			{ return text; }

			set
			{ text = value; }
		}

		/// <summary>
		/// Gets or sets the font of this menu entry.
		/// </summary>
		public SpriteFont Font
		{
			get
			{ return font; }

			set
			{ font = value; }
		}

		/// <summary>
		/// Gets or sets the screen position of this menu entry.
		/// </summary>
		public Vector2 Position
		{
			get
			{ return position; }

			set
			{ position = value; }
		}
		#endregion

		#region Initialization
		/// <param name="text">The text of the menu entry.</param>
		/// <param name="font">The font to use.</param>
		/// <param name="position">The position of the text.</param>
		public MenuEntry(string text, SpriteFont font, Vector2 position)
		{
			this.text = text;
			this.font = font;
			this.position = position;
		}
		#endregion

		#region Events
		public event EventHandler<T> Selected;

		/// <summary>
		/// Raises the Selected event.
		/// </summary>
		public virtual void OnSelectEntry(T args)
		{
			Debug.Assert(args != null);

			if (Selected != null)
				Selected(this, args);
		}
		#endregion

		#region Update
		/// <param name="menuScreen">The menu screen instance this entry belongs to.</param>
		/// <param name="isHighlighted">Indicates if this entry is currently highlighted.</param>
		public virtual void Update(GameTime gameTime, MenuScreen<T> menuScreen, bool isHighlighted) { }
		#endregion

		#region Draw
		/// <summary>
		/// Don't call base.Draw when overriding this function.
		/// </summary>
		/// <param name="menuScreen">The menu screen instance this entry belongs to.</param>
		/// <param name="isHighlighted">Indicates if this entry is currently highlighted.</param>
		public virtual void Draw(GameTime gameTime, MenuScreen<T> menuScreen, bool isHighlighted)
		{
			Color color = (isHighlighted ? Color.Yellow : Color.White);
			color.A = menuScreen.TransitionAlpha;

			menuScreen.SpriteBatch.Begin();
			menuScreen.SpriteBatch.DrawString(font, text, position, color);
			menuScreen.SpriteBatch.End();
		}
		#endregion
	}
}
