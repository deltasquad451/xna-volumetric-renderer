#region File Description
//-------------------------------------------------------------------------------------------------
// ScreenManager.cs
//
// Manager for GameScreen instances.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
using Engine.Utility;
using Graphics.Diagnostics;
#endregion

namespace Graphics
{
	/// <summary>
	/// Manager for GameScreen instances.
	/// </summary>
	public class ScreenManager : Engine.Screen.ScreenManager
	{
		#region Fields
		private ContentManager content;
		private Texture2D blank;
		private SpriteFontEx defaultFontEx;
		private SpriteBatch spriteBatch;
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
		#endregion

		#region Initialization
		/// <param name="input">The InputState instance this manager should use.</param>
		public ScreenManager(Game game, Engine.Input.InputState input)
			: base(game, input) { }

		protected override void LoadContent()
		{
			base.LoadContent();

			content = new ContentManager(Game.Services, "Content");
			blank = content.Load<Texture2D>("blank");
			defaultFontEx.font = content.Load<SpriteFont>("defaultfont");
			defaultFontEx.width = 11;
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void UnloadContent()
		{
			content.Unload();

			base.UnloadContent();
		}
		#endregion

		#region Methods
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
		#endregion
	}
}
