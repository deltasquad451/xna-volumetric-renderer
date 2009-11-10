#region File Description
//-------------------------------------------------------------------------------------------------
// AssertScreen.cs
//
// Debug screen that displays an assert.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
using Engine.Utility;
#endregion

namespace Graphics.Diagnostics
{
	/// <summary>
	/// Debug screen that displays an assert.
	/// </summary>
	public class AssertScreen : Engine.Screen.GameScreen
	{
		#region Structs
		/// <summary>
		/// Struct for containing a string of text along with its position.
		/// </summary>
		public struct AssertText
		{
			public string text;
			public Vector2 position;
		}
		#endregion

		#region Fields
		private List<AssertText> assertText;
		private Color flashColor;
		private TimeSpan flashRate;
		private int flashDir;
		#endregion

		#region Initialization
		/// <param name="args">Arguments passed from Debug.Assert.</param>
		public AssertScreen(DebugEventArgs args)
			: base()
		{
			assertText = new List<AssertText>();

			int width = VolumetricRenderer.Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
			SpriteFontEx fontEx = VolumetricRenderer.Game.ScreenManager.DefaultFontEx;

			// Set up all the text to be drawn.
			float yPos = 0;
			PlaceAssertText("ASSERT:", ref yPos, ref fontEx, width);
			
			if (args.Messages.Length > 0)
				for (int i = 0; i < args.Messages.Length; ++i)
				{
					PlaceAssertText(args.Messages[i], ref yPos, ref fontEx, width);
					yPos += fontEx.font.LineSpacing;
				}
			else
			{
				PlaceAssertText("(no info)", ref yPos, ref fontEx, width);
				yPos += fontEx.font.LineSpacing;
			}

			PlaceAssertText("IN:", ref yPos, ref fontEx, width);
			PlaceAssertText(args.MethodName, ref yPos, ref fontEx, width);
			PlaceAssertText(args.FileName + ": line " + args.LineNumber, ref yPos, ref fontEx, width);

			yPos += fontEx.font.LineSpacing * 2;
			PlaceAssertText("Press Enter to quit...", ref yPos, ref fontEx, width);

			// Shift all the text down a bit so it's not right at the top of the screen.
			int height = VolumetricRenderer.Game.GraphicsDevice.PresentationParameters.BackBufferHeight;
			int yOffset = (height - (fontEx.font.LineSpacing * (assertText.Count + 4))) / 3;
			if (yOffset < 5)
				yOffset = 5;

			for (int i = 0; i < assertText.Count; ++i)
			{
				AssertText at = assertText[i];
				at.position.Y += yOffset;
				assertText[i] = at;
			}

			// Set up the flashing text parameters.
			flashColor = Color.White;
			flashRate = TimeSpan.FromMilliseconds(700);
			flashDir = -1;
		}

		/// <summary>
		/// Places the specified text centered within the specified width. If the text is too long
		/// to fit on a single line, the text is optimally divided up and placed on consecutive lines.
		/// </summary>
		/// <param name="text">Text to draw.</param>
		/// <param name="yPos">Y-position to draw the text at.</param>
		/// <param name="fontEx">Font info for the text.</param>
		/// <param name="width">Width of the area to draw the text in.</param>
		private void PlaceAssertText(string text, ref float yPos, ref SpriteFontEx fontEx, int width)
		{
			AssertText at;
			float xPos;

			// Check if the text is too long to place on a single line.
			if ((xPos = (float)Text.CenterWidthwise(text, fontEx.width, width)) < 5f)
			{
				// Text can't fit on one line, so optimally divide it up so it can fit on multiple lines.
				string[] segments = Text.FitToWidthBalanced(text, fontEx.width, width - 10);

				// Place each segment on its own line and center it.
				foreach (string textSegment in segments)
				{
					at.text = textSegment;
					at.position.X = (float)Text.CenterWidthwise(textSegment, fontEx.width, width);
					at.position.Y = yPos;

					assertText.Add(at);
					yPos += fontEx.font.LineSpacing;
				}
			}
			else
			{
				// Text fits on one line.
				at.text = text;
				at.position.X = xPos;
				at.position.Y = yPos;

				assertText.Add(at);
				yPos += fontEx.font.LineSpacing;
			}
		}
		#endregion

		#region Update
		public override void Update(GameTime gameTime, bool hasFocus, bool isObscured)
		{
			base.Update(gameTime, hasFocus, isObscured);

			// Flash the bottom line of text so the player knows the game isn't frozen.
			float elapsed = Time.ElapsedTimeInMilliseconds(gameTime);
			int flashDelta = (int)(255 * ((elapsed / flashRate.TotalMilliseconds) * flashDir));

			int val = flashColor.R + flashDelta;
			if (val < 70)
			{
				val = 70 + (70 - val);
				flashDir = 1;
			}
			else if (val > 255)
			{
				val = 255 - (val - 255);
				flashDir = -1;
			}

			flashColor.R = (byte)val;
			flashColor.G = (byte)val;
			flashColor.B = (byte)val;
		}

		public override void HandleInput(Engine.Input.InputState input)
		{
			base.HandleInput(input);

			if (input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter, PlayerIndex.One))
				ScreenManager.Game.Exit();
		}
		#endregion

		#region Draw
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			global::Graphics.ScreenManager screenManager = (global::Graphics.ScreenManager)ScreenManager;

			screenManager.GraphicsDevice.Clear(Color.Black);
			screenManager.SpriteBatch.Begin();

			int i;
			for (i = 0; i < assertText.Count - 1; ++i)
				screenManager.SpriteBatch.DrawString(screenManager.DefaultFontEx.font, assertText[i].text, assertText[i].position, Color.White);

			screenManager.SpriteBatch.DrawString(screenManager.DefaultFontEx.font, assertText[i].text, assertText[i].position, flashColor);
			screenManager.SpriteBatch.End();
		}
		#endregion
	}
}
