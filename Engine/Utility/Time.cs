#region File Description
//-------------------------------------------------------------------------------------------------
// Time.cs
//
// Static class containing helper functions having to do with time conversions.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Renderer.Diagnostics;
#endregion

namespace Renderer.Utility
{
	/// <summary>
	/// Static class containing helper functions having to do with time conversions.
	/// </summary>
	public static class Time
	{
		#region Methods
		/// <summary>
		/// Returns the elapsed time in milliseconds. This is different from ElapsedGameTime.TotalMilliseconds
		/// in that it divides the elapsed ticks by the ticks per millisecond, which doesn't suffer
		/// from 
		/// </summary>
		/// <returns></returns>
		public static float ElapsedTimeInMilliseconds(GameTime gameTime)
		{
			return (float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static float ElapsedTimeInSeconds(GameTime gameTime)
		{
			return (float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerSecond;
		}
		#endregion
	}
}
