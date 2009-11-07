#region File Description
//-------------------------------------------------------------------------------------------------
// InputState.cs
//
// Holds the current and previous states of the input devices, and provides high-level queries.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Diagnostics;
using Graphics.Diagnostics;
#endregion

namespace Graphics
{
	/// <summary>
	/// Holds the current and previous states of the input devices.
	/// </summary>
	public class InputState : Engine.Input.InputState
	{
		#region Initialization
		public InputState()
			: base() { }
		#endregion

		#region Menu Methods
		/// <summary>
		/// Returns whether or not the MenuUp command was invoked.
		/// </summary>
		/// <returns>True if the MenuUp command was invoked, false otherwise.</returns>
		public bool MenuUp()
		{
			return IsKeyPressed(Keys.Up, PlayerIndex.One);
		}

		/// <summary>
		/// Returns whether or not the MenuDown command was invoked.
		/// </summary>
		/// <returns>True if the MenuDown command was invoked, false otherwise.</returns>
		public bool MenuDown()
		{
			return IsKeyPressed(Keys.Down, PlayerIndex.One);
		}

		/// <summary>
		/// Returns whether or not the MenuSelect command was invoked.
		/// </summary>
		/// <returns>True if the MenuSelect command was invoked, false otherwise.</returns>
		public bool MenuSelect()
		{
			return IsKeyPressed(Keys.Enter, PlayerIndex.One);
		}
		#endregion
	}
}
