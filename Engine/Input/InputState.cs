#region File Description
//-------------------------------------------------------------------------------------------------
// InputState.cs
//
// Holds the current and previous states of the input devices, and provides high-level queries.
// In multi-threaded applications, the Update method must be done on the main thread for the input
// to be updated correctly.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Renderer.Diagnostics;
#endregion

namespace Renderer.Input
{
	#region Enums
	public enum MouseButtons
	{
		Left,
		Middle,
		Right,
		XButton1,
		XButton2
	}
	#endregion

	/// <summary>
	/// Holds the current and previous states of the input devices.
	/// </summary>
	public class InputState
	{
		#region Fields
		private KeyboardState[] currentKeyboardStates;
		private KeyboardState[] previousKeyboardStates;

		private GamePadState[] currentGamePadStates;
		private GamePadState[] previousGamePadStates;

		private MouseState currentMouseState;
		private MouseState previousMouseState;

		private const int maxInputs = 4;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the array of current keyboard states.
		/// </summary>
		protected KeyboardState[] CurrentKeyboardStates
		{
			get
			{ return currentKeyboardStates; }
		}

		/// <summary>
		/// Gets the array of previous keyboard states.
		/// </summary>
		protected KeyboardState[] PreviousKeyboardStates
		{
			get
			{ return previousKeyboardStates; }
		}

		/// <summary>
		/// Gets the array of current gamepad states.
		/// </summary>
		protected GamePadState[] CurrentGamePadStates
		{
			get
			{ return currentGamePadStates; }
		}

		/// <summary>
		/// Gets the array of previous gamepad states.
		/// </summary>
		protected GamePadState[] PreviousGamePadStates
		{
			get
			{ return previousGamePadStates; }
		}

		/// <summary>
		/// Gets the current mouse state.
		/// </summary>
		protected MouseState CurrentMouseState
		{
			get
			{ return currentMouseState; }
		}

		/// <summary>
		/// Gets the previous mouse state.
		/// </summary>
		protected MouseState PreviousMouseState
		{
			get
			{ return previousMouseState; }
		}

		/// <summary>
		/// Gets the mouse's current position, relative to the upper-left corner of the screen.
		/// </summary>
		public Vector2 MousePosition
		{
			get
			{
				Vector2 position;
				position.X = (float)currentMouseState.X;
				position.Y = (float)currentMouseState.Y;
				return position;
			}
		}

		/// <summary>
		/// Gets the value representing the mouse scroll wheel's cumulative movement.
		/// </summary>
		public int ScrollWheelValue
		{
			get
			{ return currentMouseState.ScrollWheelValue; }
		}

		/// <summary>
		/// Gets the number of maximum inputs allowed.
		/// </summary>
		public int MaxInputs
		{
			get
			{ return maxInputs; }
		}
		#endregion

		#region Initialization
		public InputState()
		{
			currentKeyboardStates = new KeyboardState[maxInputs];
			previousKeyboardStates = new KeyboardState[maxInputs];

			currentGamePadStates = new GamePadState[maxInputs];
			previousGamePadStates = new GamePadState[maxInputs];

			currentMouseState = new MouseState();
			previousMouseState = new MouseState();
		}
		#endregion

		#region Update
		/// <summary>
		/// Updates the states of all input devices.
		/// </summary>
		public virtual void Update()
		{
			for (int i = 0; i < maxInputs; ++i)
			{
				previousKeyboardStates[i] = currentKeyboardStates[i];
				currentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);

				previousGamePadStates[i] = currentGamePadStates[i];
				currentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
			}

			previousMouseState = currentMouseState;
			currentMouseState = Mouse.GetState();
		}
		#endregion

		#region Keyboard Methods
		/// <summary>
		/// Checks if the specified key is currently down.
		/// </summary>
		/// <param name="key">Key to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the key is down, false otherwise.</returns>
		public bool IsKeyDown(Keys key, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return currentKeyboardStates[index].IsKeyDown(key);
			}
			else
				// Check the input from all players.
				return (currentKeyboardStates[0].IsKeyDown(key) || currentKeyboardStates[1].IsKeyDown(key) || 
						currentKeyboardStates[2].IsKeyDown(key) || currentKeyboardStates[3].IsKeyDown(key));
		}

		/// <summary>
		/// Checks if the specified key has just been pressed.
		/// </summary>
		/// <param name="key">Key to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the key was just pressed, false otherwise.</returns>
		public bool IsKeyPressed(Keys key, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return (currentKeyboardStates[index].IsKeyDown(key) && previousKeyboardStates[index].IsKeyUp(key));
			}
			else
				// Check the input from all players.
				return ((currentKeyboardStates[0].IsKeyDown(key) && previousKeyboardStates[0].IsKeyUp(key)) ||
						(currentKeyboardStates[1].IsKeyDown(key) && previousKeyboardStates[1].IsKeyUp(key)) ||
						(currentKeyboardStates[2].IsKeyDown(key) && previousKeyboardStates[2].IsKeyUp(key)) ||
						(currentKeyboardStates[3].IsKeyDown(key) && previousKeyboardStates[3].IsKeyUp(key)));
		}

		/// <summary>
		/// Checks if the specified key is being held down.
		/// </summary>
		/// <param name="key">Key to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the key is being held down, false otherwise.</returns>
		public bool IsKeyHeld(Keys key, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return (currentKeyboardStates[index].IsKeyDown(key) && previousKeyboardStates[index].IsKeyDown(key));
			}
			else
				// Check the input from all players.
				return ((currentKeyboardStates[0].IsKeyDown(key) && previousKeyboardStates[0].IsKeyDown(key)) ||
						(currentKeyboardStates[1].IsKeyDown(key) && previousKeyboardStates[1].IsKeyDown(key)) ||
						(currentKeyboardStates[2].IsKeyDown(key) && previousKeyboardStates[2].IsKeyDown(key)) ||
						(currentKeyboardStates[3].IsKeyDown(key) && previousKeyboardStates[3].IsKeyDown(key)));
		}

		/// <summary>
		/// Checks if the specified key has just been released.
		/// </summary>
		/// <param name="key">Key to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the key was just released, false otherwise.</returns>
		public bool IsKeyReleased(Keys key, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return (currentKeyboardStates[index].IsKeyUp(key) && previousKeyboardStates[index].IsKeyDown(key));
			}
			else
				// Check the input from all players.
				return ((currentKeyboardStates[0].IsKeyUp(key) && previousKeyboardStates[0].IsKeyDown(key)) ||
						(currentKeyboardStates[1].IsKeyUp(key) && previousKeyboardStates[1].IsKeyDown(key)) ||
						(currentKeyboardStates[2].IsKeyUp(key) && previousKeyboardStates[2].IsKeyDown(key)) ||
						(currentKeyboardStates[3].IsKeyUp(key) && previousKeyboardStates[3].IsKeyDown(key)));
		}
		#endregion

		#region GamePad Methods
		/// <summary>
		/// Checks if the specified button is currently down.
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the button is down, false otherwise.</returns>
		public bool IsButtonDown(Buttons button, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return currentGamePadStates[index].IsButtonDown(button);
			}
			else
				// Check the input from all players.
				return (currentGamePadStates[0].IsButtonDown(button) || currentGamePadStates[1].IsButtonDown(button) ||
						currentGamePadStates[2].IsButtonDown(button) || currentGamePadStates[3].IsButtonDown(button));
		}

		/// <summary>
		/// Checks if the specified button has just been pressed.
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the button was just pressed, false otherwise.</returns>
		public bool IsButtonPressed(Buttons button, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return (currentGamePadStates[index].IsButtonDown(button) && previousGamePadStates[index].IsButtonUp(button));
			}
			else
				// Check the input from all players.
				return ((currentGamePadStates[0].IsButtonDown(button) && previousGamePadStates[0].IsButtonUp(button)) ||
						(currentGamePadStates[1].IsButtonDown(button) && previousGamePadStates[1].IsButtonUp(button)) ||
						(currentGamePadStates[2].IsButtonDown(button) && previousGamePadStates[2].IsButtonUp(button)) ||
						(currentGamePadStates[3].IsButtonDown(button) && previousGamePadStates[3].IsButtonUp(button)));
		}

		/// <summary>
		/// Checks if the specified button is being held down.
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the button is being held down, false otherwise.</returns>
		public bool IsButtonHeld(Buttons button, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return (currentGamePadStates[index].IsButtonDown(button) && previousGamePadStates[index].IsButtonDown(button));
			}
			else
				// Check the input from all players.
				return ((currentGamePadStates[0].IsButtonDown(button) && previousGamePadStates[0].IsButtonDown(button)) ||
						(currentGamePadStates[1].IsButtonDown(button) && previousGamePadStates[1].IsButtonDown(button)) ||
						(currentGamePadStates[2].IsButtonDown(button) && previousGamePadStates[2].IsButtonDown(button)) ||
						(currentGamePadStates[3].IsButtonDown(button) && previousGamePadStates[3].IsButtonDown(button)));
		}

		/// <summary>
		/// Checks if the specified button has just been released.
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <param name="playerIndex">Player index to check; NULL checks all players.</param>
		/// <returns>True if the button was just released, false otherwise.</returns>
		public bool IsButtonReleased(Buttons button, PlayerIndex? playerIndex)
		{
			if (playerIndex.HasValue)
			{
				// Check the input from the specified player.
				int index = (int)playerIndex.Value;
				return (currentGamePadStates[index].IsButtonUp(button) && previousGamePadStates[index].IsButtonDown(button));
			}
			else
				// Check the input from all players.
				return ((currentGamePadStates[0].IsButtonUp(button) && previousGamePadStates[0].IsButtonDown(button)) ||
						(currentGamePadStates[1].IsButtonUp(button) && previousGamePadStates[1].IsButtonDown(button)) ||
						(currentGamePadStates[2].IsButtonUp(button) && previousGamePadStates[2].IsButtonDown(button)) ||
						(currentGamePadStates[3].IsButtonUp(button) && previousGamePadStates[3].IsButtonDown(button)));
		}

		/// <summary>
		/// Gets the value representing how much the specified trigger is depressed.
		/// </summary>
		/// <param name="trigger">Trigger to check.</param>
		/// <param name="playerIndex">Player index to check.</param>
		/// <returns>UNKNOWN</returns>
		public float TriggerValue(Buttons trigger, PlayerIndex playerIndex)
		{
			Debug.Assert(trigger == Buttons.LeftTrigger || trigger == Buttons.RightTrigger);

			if (trigger == Buttons.LeftTrigger)
				return currentGamePadStates[(int)playerIndex].Triggers.Left;
			else
				return currentGamePadStates[(int)playerIndex].Triggers.Right;
		}

		/// <summary>
		/// Gets the value representing the position of the specified thumbstick.
		/// </summary>
		/// <param name="thumbstick">Thumbstick to check (use LeftStick or RightStick).</param>
		/// <param name="playerIndex">Player index to check.</param>
		/// <returns>UNKNOWN</returns>
		public Vector2 ThumbstickValue(Buttons thumbstick, PlayerIndex playerIndex)
		{
			Debug.Assert(thumbstick == Buttons.LeftStick || thumbstick == Buttons.RightStick);

			if (thumbstick == Buttons.LeftStick)
				return currentGamePadStates[(int)playerIndex].ThumbSticks.Left;
			else
				return currentGamePadStates[(int)playerIndex].ThumbSticks.Right;
		}
		#endregion

		#region Mouse Methods
		/// <summary>
		/// Checks if the specified mouse button is currently down.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <returns>True if the button is down, false otherwise.</returns>
		public bool IsMouseBtnDown(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					return (currentMouseState.LeftButton == ButtonState.Pressed ? true : false);

				case MouseButtons.Middle:
					return (currentMouseState.MiddleButton == ButtonState.Pressed ? true : false);

				case MouseButtons.Right:
					return (currentMouseState.RightButton == ButtonState.Pressed ? true : false);

				case MouseButtons.XButton1:
					return (currentMouseState.XButton1 == ButtonState.Pressed ? true : false);

				case MouseButtons.XButton2:
					return (currentMouseState.XButton2 == ButtonState.Pressed ? true : false);

				default:
					Debug.Assert(false);
					return false;
			}
		}

		/// <summary>
		/// Checks if the specified mouse button has just been pressed.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <returns>True if the mouse button was just pressed, false otherwise.</returns>
		public bool IsMouseBtnPressed(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					return ((currentMouseState.LeftButton == ButtonState.Pressed && 
							previousMouseState.LeftButton == ButtonState.Released) ? true : false);

				case MouseButtons.Middle:
					return ((currentMouseState.MiddleButton == ButtonState.Pressed &&
							previousMouseState.MiddleButton == ButtonState.Released) ? true : false);

				case MouseButtons.Right:
					return ((currentMouseState.RightButton == ButtonState.Pressed &&
							previousMouseState.RightButton == ButtonState.Released) ? true : false);

				case MouseButtons.XButton1:
					return ((currentMouseState.XButton1 == ButtonState.Pressed &&
							previousMouseState.XButton1 == ButtonState.Released) ? true : false);

				case MouseButtons.XButton2:
					return ((currentMouseState.XButton2 == ButtonState.Pressed &&
							previousMouseState.XButton2 == ButtonState.Released) ? true : false);

				default:
					Debug.Assert(false);
					return false;
			}
		}

		/// <summary>
		/// Checks if the specified mouse button is being held down.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <returns>True if the mouse button is being held down, false otherwise.</returns>
		public bool IsMouseBtnHeld(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					return ((currentMouseState.LeftButton == ButtonState.Pressed &&
							previousMouseState.LeftButton == ButtonState.Pressed) ? true : false);

				case MouseButtons.Middle:
					return ((currentMouseState.MiddleButton == ButtonState.Pressed &&
							previousMouseState.MiddleButton == ButtonState.Pressed) ? true : false);

				case MouseButtons.Right:
					return ((currentMouseState.RightButton == ButtonState.Pressed &&
							previousMouseState.RightButton == ButtonState.Pressed) ? true : false);

				case MouseButtons.XButton1:
					return ((currentMouseState.XButton1 == ButtonState.Pressed &&
							previousMouseState.XButton1 == ButtonState.Pressed) ? true : false);

				case MouseButtons.XButton2:
					return ((currentMouseState.XButton2 == ButtonState.Pressed &&
							previousMouseState.XButton2 == ButtonState.Pressed) ? true : false);

				default:
					Debug.Assert(false);
					return false;
			}
		}

		/// <summary>
		/// Checks if the specified mouse button has just been released.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <returns>True if the mouse button was just released, false otherwise.</returns>
		public bool IsMouseBtnReleased(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					return ((currentMouseState.LeftButton == ButtonState.Released &&
							previousMouseState.LeftButton == ButtonState.Pressed) ? true : false);

				case MouseButtons.Middle:
					return ((currentMouseState.MiddleButton == ButtonState.Released &&
							previousMouseState.MiddleButton == ButtonState.Pressed) ? true : false);

				case MouseButtons.Right:
					return ((currentMouseState.RightButton == ButtonState.Released &&
							previousMouseState.RightButton == ButtonState.Pressed) ? true : false);

				case MouseButtons.XButton1:
					return ((currentMouseState.XButton1 == ButtonState.Released &&
							previousMouseState.XButton1 == ButtonState.Pressed) ? true : false);

				case MouseButtons.XButton2:
					return ((currentMouseState.XButton2 == ButtonState.Released &&
							previousMouseState.XButton2 == ButtonState.Pressed) ? true : false);

				default:
					Debug.Assert(false);
					return false;
			}
		}
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
