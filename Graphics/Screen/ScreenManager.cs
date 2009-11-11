#region File Description
//-------------------------------------------------------------------------------------------------
// ScreenManager.cs
//
// Manager for GameScreen instances.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
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
	/// Manager for GameScreen instances.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        #region Fields
		private ContentManager content;
		private Texture2D blank;
		private SpriteFontEx defaultFontEx;
		private SpriteBatch spriteBatch;
		private InputState input;

		// The screens are maintained as a list so that Update can process the screens top-down
		// and Draw can process them bottom-up.
		private List<GameScreen> screens;
		private List<GameScreen> screensToUpdate;
        #endregion

        #region Properties
		/// <summary>
		/// Gets the content manager for this ScreenManager object.
		/// </summary>
		public ContentManager Content
		{
			get
			{ return content; }
		}

		/// <summary>
		/// Gets the default font data shared among all screens.
		/// </summary>
		public SpriteFontEx DefaultFontEx
		{
			get
			{ return defaultFontEx; }
		}

		/// <summary>
		/// Gets the SpriteBatch instance shared among all screens.
		/// </summary>
		public SpriteBatch SpriteBatch
		{
			get
			{ return spriteBatch; }
		}

		/// <summary>
		/// Gets the InputState instance the manager is using.
		/// </summary>
		public InputState Input
		{
			get
			{ return input; }
		}

		/// <summary>
		/// Gets the list of current GameScreen instances.
		/// </summary>
		protected List<GameScreen> Screens
		{
			get
			{ return screens; }
		}
        #endregion

        #region Initialization
		/// <param name="input">The InputState instance this manager should use.</param>
		public ScreenManager(Game game, InputState input)
            : base(game)
        {
			screens = new List<GameScreen>();
			screensToUpdate = new List<GameScreen>();
			this.input = input;
        }

		protected override void LoadContent()
		{
			base.LoadContent();

			content = new ContentManager(Game.Services, "Content\\Screen");
			blank = content.Load<Texture2D>("blank");
			defaultFontEx.font = content.Load<SpriteFont>("defaultfont");
			defaultFontEx.width = 11;
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

        protected override void UnloadContent()
        {
			content.Unload();

            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
                screen.UnloadContent();

			base.UnloadContent();
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
			base.Update(gameTime);

			bool hasFocus = Game.IsActive;
			bool isObscured = false;

			// Make a temp copy of the list so new addtions and removals don't screw us up.
			screensToUpdate.Clear();
			foreach (GameScreen screen in screens)
			    screensToUpdate.Add(screen);

			while (screensToUpdate.Count > 0)
			{
				// "Pop" the topmost screen off the list (which is at the end) and update it.
				GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];
				screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
				screen.Update(gameTime, hasFocus, isObscured);

				if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
				{
					// If this is the first active screen we came across, give it a chance to handle input.
					if (hasFocus)
					{
						screen.HandleInput(input);
						hasFocus = false;
					}

					// If this is an active non-popup, inform any subsequent screens that they are covered by it.
					if (!screen.IsPopup)
						isObscured = true;
				}
			}
        }
		#endregion

		#region Draw
        public override void Draw(GameTime gameTime)
        {
			base.Draw(gameTime);

			// Tell each screen to draw itself.
            foreach (GameScreen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }
        #endregion

        #region Methods
        /// <summary>
		/// Adds a new screen to the screen manager.
        /// </summary>
        /// <param name="screen">Screen to add.</param>
        public virtual void AddScreen(GameScreen screen)
        {
			screen.ScreenManager = this;
			screen.LoadContent();
            screens.Add(screen);
        }

        /// <summary>
		/// Removes a screen from the screen manager.
        /// </summary>
        /// <param name="screen">Screen to remove.</param>
		public virtual void RemoveScreen(GameScreen screen)
		{
			screen.UnloadContent();
			screens.Remove(screen);
			screensToUpdate.Remove(screen);
		}

		/// <summary>
		/// Removes all screens from the screen manager.
		/// </summary>
		public virtual void RemoveAllScreens()
		{
			foreach (GameScreen screen in screens)
				screen.UnloadContent();

			screens.Clear();
			screensToUpdate.Clear();
		}

		/// <summary>
		/// Draws a translucent black fullscreen sprite, used for darkening the background.
		/// </summary>
		public void FadeBackBufferToBlack(byte alpha)
		{
			Viewport viewport = GraphicsDevice.Viewport;

			spriteBatch.Begin();
			spriteBatch.Draw(blank, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, alpha));
			spriteBatch.End();
		}

        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
		//public GameScreen[] GetScreens()
		//{
		//    return screens.ToArray();
		//}
        #endregion
    }
}
