#region File Description
//-------------------------------------------------------------------------------------------------
// MenuEntry.cs
//
// A single entry in a MenuScreen instance.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
using Graphics.Diagnostics;
#endregion

namespace Graphics.GameScreens
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuEntry : Engine.Screen.MenuEntry<MenuEventArgs>
	{
		#region Initialization
		/// <param name="text">The text of the menu entry.</param>
		/// <param name="font">The font to use.</param>
		/// <param name="position">The position of the text.</param>
		public MenuEntry(string text, SpriteFont font, Vector2 position)
			: base(text, font, position) { }
		#endregion
	}
}
