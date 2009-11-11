#region File Description
//-------------------------------------------------------------------------------------------------
// GameScreen.cs
//
// A single screen layer, managed by the ScreenManager, that can be combined with other layers to
// create multi-layered screens (e.g. a menu system).
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Renderer.Diagnostics;
using Renderer.Input;
using Renderer.Utility;
#endregion

namespace Renderer.Graphics.Screen
{
	#region Enums
	/// <summary>
    /// Describes the current state of the screen.
    /// </summary>
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden
	}
	#endregion

	/// <summary>
	/// A single screen layer, managed by the ScreenManager.
    /// </summary>
    public abstract class GameScreen
	{
		#region Enums
		/// <summary>
		/// Describes the direction of transition.
		/// </summary>
		private enum TransitionDir
		{
			Off = -1,
			On = 1
		}
		#endregion

		#region Fields
		private ScreenState screenState;
		private TimeSpan transitionOnTime;
		private TimeSpan transitionOffTime;
		private float transitionProgress;
		private bool hasFocus;
		private bool alwaysVisible;
		private bool isPopup;
		private bool isExiting;
		private ScreenManager screenManager;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the current screen transition state.
		/// </summary>
		public ScreenState ScreenState
		{
			get
			{ return screenState; }

			set
			{ screenState = value; }
		}

		/// <summary>
		/// Indicates how long the screen takes to transition on when it is activated.
		/// </summary>
		public TimeSpan TransitionOnTime
		{
			get
			{ return transitionOnTime; }

			set
			{ transitionOnTime = value; }
		}

		/// <summary>
		/// Indicates how long the screen takes to transition off when it is deactivated.
		/// </summary>
		public TimeSpan TransitionOffTime
		{
			get
			{ return transitionOffTime; }

			set
			{ transitionOffTime = value; }
		}

		/// <summary>
		/// Gets or sets the current position of the screen transition, ranging from zero (fully 
		/// active, no transition) to one (transitioned fully off to nothing).
		/// </summary>
		public float TransitionProgress
		{
			get
			{ return transitionProgress; }

			set
			{ transitionProgress = value; }
		}

		/// <summary>
		/// Gets the current alpha of the screen transition, ranging from 255 (fully active, no
		/// transition) to 0 (transitioned fully off to nothing).
		/// </summary>
		public byte TransitionAlpha
		{
			get
			{ return (byte)(transitionProgress * 255); }
		}

		/// <summary>
		/// Checks whether this screen is active and can respond to user input.
		/// </summary>
		public bool IsActive
		{
			get
			{ return hasFocus && (screenState == ScreenState.TransitionOn || screenState == ScreenState.Active); }
		}

		/// <summary>
		/// Indicates whether the screen is always visible or not (i.e. won't transition off
		/// if it's obscured by another screen).
		/// </summary>
		public bool AlwaysVisible
		{
			get
			{ return alwaysVisible; }

			set
			{ alwaysVisible = value; }
		}

		/// <summary>
        /// Indicates whether the screen is only a small popup, in which case screens underneath
		/// it do not need to bother transitioning off.
        /// </summary>
        public bool IsPopup
        {
			get
			{ return isPopup; }

			set
			{ isPopup = value; }
        }

        /// <summary>
        /// Indicates whether the screen is being permanently removed.
        /// </summary>
		public bool IsExiting
		{
			get
			{ return isExiting; }

			set
			{ isExiting = value; }
		}

        /// <summary>
        /// Gets or sets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager ScreenManager
        {
            get
			{ return screenManager; }

			set
			{ screenManager = value; }
        }
        #endregion

        #region Initialization
		public GameScreen()
		{
			screenState = ScreenState.TransitionOn;
			transitionOnTime = TimeSpan.Zero;
			transitionOffTime = TimeSpan.Zero;
			transitionProgress = 0;
			alwaysVisible = false;
			isPopup = false;
			isExiting = false;
		}

		public virtual void LoadContent() { }

		public virtual void UnloadContent() { }
        #endregion

        #region Update
        /// <param name="hasFocus">Indicates if this screen has focus.</param>
        /// <param name="isObscured">Indicates if this screen is completely obscured by another screen.</param>
		public virtual void Update(GameTime gameTime, bool hasFocus, bool isObscured)
        {
			this.hasFocus = hasFocus;

			if (isExiting)
			{
				// Transition off and remove ourselves.
				screenState = ScreenState.TransitionOff;
				if (UpdateTransition(gameTime, TransitionDir.Off))
					screenManager.RemoveScreen(this);
			}
			else if (isObscured && !alwaysVisible)
			{
				// Transition off and become hidden.
				screenState = ScreenState.TransitionOff;
				if (UpdateTransition(gameTime, TransitionDir.Off))
					screenState = ScreenState.Hidden;
			}
			else
			{
				// Transition on and become active.
				screenState = ScreenState.TransitionOn;
				if (UpdateTransition(gameTime, TransitionDir.On))
					screenState = ScreenState.Active;
			}
        }

		/// <summary>
		/// Helper for updating the screen transition progress.
		/// </summary>
		/// <param name="gameTime">Current snapshot of timing values.</param>
		/// <param name="transitionDir">Direction of transition, either On or Off.</param>
		/// <returns>True when the transition is complete; false otherwise.</returns>
		private bool UpdateTransition(GameTime gameTime, TransitionDir transitionDir)
		{
			int direction = (int)transitionDir;

			Debug.Assert(direction == 1 || direction == -1);
			Debug.Assert(transitionProgress >= 0 && transitionProgress <= 1);
			Debug.Assert(screenState == ScreenState.TransitionOn || screenState == ScreenState.TransitionOff);

			TimeSpan transitionTime = (direction == 1 ? transitionOnTime : transitionOffTime);

			// Find the amount of transition to do during this frame.
			float transitionDelta;
			if (transitionTime == TimeSpan.Zero)
				transitionDelta = 1;
			else
				transitionDelta = (float)(Time.ElapsedTimeInMilliseconds(gameTime) / transitionTime.TotalMilliseconds);

			// Update the transition progess.
			transitionProgress += transitionDelta * direction;

			// Check to see if we're done transitioning.
			if ((direction == 1 && transitionProgress >= 1) || (direction == -1 && transitionProgress <= 0))
			{
				transitionProgress = MathHelper.Clamp(transitionProgress, 0, 1);
				return true;
			}

			// Still transitioning.
			return false;
		}

		/// <summary>
		/// Handler for the input to this screen. This is called only when this screen has focus.
		/// </summary>
		/// <param name="input">The InputState instance to read input from.</param>
        public virtual void HandleInput(InputState input) { }
		#endregion

		#region Draw
		public virtual void Draw(GameTime gameTime) { }
        #endregion

        #region Methods
        /// <summary>
        /// Tells the screen to transition off and then remove itself from the ScreenManager.
        /// </summary>
		public void Finished()
		{
			// If the screen has a zero transition time, remove it immediately. Otherwise tell it to
			// transition off and exit.
			if (transitionOffTime == TimeSpan.Zero)
				screenManager.RemoveScreen(this);
			else
				isExiting = true;
		}
        #endregion
    }
}
