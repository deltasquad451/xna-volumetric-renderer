#region File Description
//-------------------------------------------------------------------------------------------------
// Text.cs
//
// Static class containing helper functions having to do with text manipulation.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Diagnostics;
#endregion

namespace Engine.Utility
{
	#region Structs
	/// <summary>
	/// Struct for containing a SpriteFont along with other information about the font.
	/// </summary>
	public struct SpriteFontEx
	{
		public SpriteFont font;

		/// <summary>
		/// Width of the font (only useful for fixed-width fonts). Because the SpriteFont class
		/// doesn't contain info about font width, this value needs to be set manually and will
		/// most likely take some experimentation to find the right value.
		/// </summary>
		public int width;
	}
	#endregion

	/// <summary>
	/// Static class containing helper functions having to do with text manipulation.
	/// </summary>
	public static class Text
	{
		#region Methods
		/// <summary>
		/// Determines where to place text so that it's centered within an area. This only works
		/// with fixed-width fonts.
		/// </summary>
		/// <param name="text">Text to center.</param>
		/// <param name="fontWidth">Width of the font.</param>
		/// <param name="areaWidth">Width of the area to center the text in.</param>
		/// <returns>The x-coordinate to draw the text at.</returns>
		public static int CenterWidthwise(string text, int fontWidth, int areaWidth)
		{
			Debug.Assert(text != null);
			Debug.Assert(fontWidth > 0);
			Debug.Assert(areaWidth > 0);

			return (areaWidth / 2) - ((text.Length * fontWidth) / 2);
		}

		/// <summary>
		/// Takes a string of text and optimally divides it up to fit within a given area's width.
		/// This is a dynamic programming solution that uses the least amount of divisions possible
		/// while minimizing the sum of the square of the leftover space on each line. Both the time
		/// and memory complexity is O(n^2); n is the number of words in the text. This only works
		/// with fixed-width fonts.
		/// </summary>
		/// <param name="text">Text to fit.</param>
		/// <param name="fontWidth">Width of the font.</param>
		/// <param name="areaWidth">Width of the area to fit the text in.</param>
		/// <returns>The in-order segmented text (one string per line).</returns>
		public static string[] FitToWidthBalanced(string text, int fontWidth, int areaWidth)
		{
			Debug.Assert(text != null);
			Debug.Assert(fontWidth > 0);
			Debug.Assert(areaWidth > 0 && areaWidth <= 46340);

			// Parse the text into individual words.
			char[] textDelimiter = { ' ' };
			string[] parsedText = text.Split(textDelimiter);

			// The penalty array was originally designed to be two-dimensional (length^2), but it
			// only uses about half the memory, which happens to be exactly characterized by 
			// (length * (length + 1) / 2), which is the sequence 1 + 2 + 3 + ... + length. So we're
			// still treating the penalty array as two-dimensional, but with only half the memory usage.
			int[] penalty = new int[(parsedText.Length * (parsedText.Length + 1)) / 2];
			int[] optimal = new int[parsedText.Length + 1];

			// Calculate the square of the leftover space (i.e. penalty) on a line for each text
			// segment word_i to word_j, i <= j.
			int partialIndex;
			int leftover;
			int prevPenalty;
			for (int i = 0; i < parsedText.Length; ++i)
			{
				// We can calculate all but the Y component of the penalty array index in advance.
				partialIndex = (i * parsedText.Length) - ((i * (i + 1)) / 2);

				// Set the penalty for a single word on a line.
				leftover = areaWidth - (parsedText[i].Length * fontWidth);
				Debug.Assert(leftover >= 0, "", "A single word is too large to fit within the area given.");
				penalty[partialIndex + i] = leftover;

				int j;
				for (j = i + 1; j < parsedText.Length; ++j)
				{
					// If the penalty for the previous text segment is int.MaxValue, then the penalty
					// for each subsequent text segment in this line will also be int.MaxValue.
					if ((prevPenalty = penalty[partialIndex + j - 1]) == int.MaxValue)
					{
						penalty[partialIndex + j] = int.MaxValue;
						continue;
					}

					// If the leftover space is negative, then the text segment doesn't fit on a line.
					leftover = prevPenalty - ((parsedText[j].Length + 1) * fontWidth);
					if (leftover < 0)
						penalty[partialIndex + j] = int.MaxValue;
					else
						penalty[partialIndex + j] = leftover;

					// Now that the previous penalty value is no longer needed, square it.
					penalty[partialIndex + j - 1] = prevPenalty * prevPenalty;
				}

				// Square the last penalty value.
				if ((prevPenalty = penalty[partialIndex + j - 1]) != int.MaxValue)
					penalty[partialIndex + j - 1] = prevPenalty * prevPenalty;
			}

			// Use the recurrence equation OPT(j) = min(penalty(i,j) + OPT(i-1)) over the range
			// 1 <= i <= j to calculate the optimal values of the solution for each j = 1 to n.
			// Side note: the initial values for both i and j should normally be 1 (as stated above)
			// but due to how I set up the penalty and optimal arrays, both arrays were being
			// indexed with i - 1 or j - 1 (and in the inner loop), so I adjusted the values
			// accordingly to remove the extra subtraction instructions.
			int min;
			int val;
			for(int j = 0; j < optimal.Length - 1; ++j)
			{
				min = int.MaxValue;
				for(int i = 0; i <= j; ++i)
				{
					val = penalty[(i * parsedText.Length) + j - ((i * (i + 1)) / 2)] + optimal[i];
					if (val >= 0 && val < min)
						min = val;
				}

				optimal[j + 1] = min;
			}

			// Using the optimal values just calculated, extract the text segments (all in one string)
			// of the optimal solution.
			string segmentDelimiter = "$^*";
			string segments = FindSegments(ref parsedText, ref penalty, ref optimal, parsedText.Length, segmentDelimiter);

			// We now have the answer to the problem, but it needs to be cleaned up. Parse it into
			// separate strings, trim off any leading or trailing whitespace, and reverse it (FindSegments
			// returns the answer in reverse order).
			string[] segDelimArray = { segmentDelimiter };
			parsedText = segments.Split(segDelimArray, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < parsedText.Length; ++i)
				parsedText[i] = parsedText[i].Trim();
			Array.Reverse(parsedText);

			return parsedText;
		}

		/// <summary>
		/// Recursive method for extracting the text segments of the optimal solution.
		/// </summary>
		/// <param name="parsedText">The individual words of the text.</param>
		/// <param name="penalty">The penalty values for each text segment.</param>
		/// <param name="optimal">The optimal values of the solution.</param>
		/// <param name="j">Index of the last word in the segment 1 to j.</param>
		/// <param name="delimiter">Delimiter to add to the end of the segment.</param>
		/// <returns>The text segments of the optimal solution, delimited by 'delimiter', in reverse order.</returns>
		private static string FindSegments(ref string[] parsedText, ref int[] penalty, ref int[] optimal, int j, string delimiter)
		{
			if (j == 0)
				return "";
			else
			{
				// Find the index of the first word, 1 <= i <= j, in the text segment word_i to word_j
				// that is part of the optimal solution.
				// Side note: the initial value of i has been adjusted, for the same reasons as stated
				// above in FitToWidthBalanced.
				int min = int.MaxValue;
				int val;
				int index = 0;
				for (int i = 0; i < j; ++i)
				{
					val = penalty[(i * parsedText.Length) + j - 1 - ((i * (i + 1)) / 2)] + optimal[i];
					if (val >= 0 && val < min)
					{
						min = val;
						index = i;
					}
				}

				// Build the text segment.
				string segment = "";
				for (int i = index; i < j; ++i)
					segment += parsedText[i] + " ";

				// Return the text segment along with the other segments from what's left over.
				return segment + delimiter + FindSegments(ref parsedText, ref penalty, ref optimal, index, delimiter);
			}
		}
		#endregion

		#region FitToWidthBalanced original solution (unoptimized)
		///// <summary>
		///// Takes a string of text and optimally divides it up to fit within a given area's width.
		///// This is a dynamic programming solution that uses the least amount of divisions possible
		///// while minimizing the sum of the square of the leftover space on each line. The complexity
		///// is O(n^3); n is the number of words in the text. This only works with fixed-width fonts.
		///// </summary>
		///// <param name="text">Text to fit.</param>
		///// <param name="fontWidth">Width of the font.</param>
		///// <param name="areaWidth">Width of the area to fit the text in.</param>
		///// <returns>The in-order segmented text (one string per line).</returns>
		//public static string[] FitToWidthBalanced(string text, int fontWidth, int areaWidth)
		//{
		//    Debug.Assert(text != null);
		//    Debug.Assert(fontWidth > 0);
		//    Debug.Assert(areaWidth > 0 && areaWidth <= 46340);

		//    // Parse the text into individual words.
		//    char[] textDelimiter = { ' ' };
		//    string[] parsedText = text.Split(textDelimiter);

		//    uint[,] penalty = new uint[parsedText.Length, parsedText.Length];
		//    uint[] optimal = new uint[parsedText.Length + 1];
		//    optimal[0] = 0;

		//    // Calculate the square of the leftover space (i.e. penalty) on a line for each word
		//    // segment i to j, i <= j.
		//    for (int j = 0; j < penalty.GetLength(0); ++j)
		//        for (int i = 0; i <= j; ++i)
		//            penalty[i, j] = CalculatePenalty(ref parsedText, i, j, fontWidth, areaWidth);

		//    // Use the recurrence equation OPT(j) = min(penalty(i,j) + OPT(i-1)) over the range
		//    // 1 <= i <= j to calculate the optimal values of the solution for each j = 1 to n.
		//    uint min;
		//    uint val;
		//    for (int j = 1; j < optimal.Length; ++j)
		//    {
		//        min = int.MaxValue;
		//        for (int i = 1; i <= j; ++i)
		//        {
		//            val = penalty[i - 1, j - 1] + optimal[i - 1];
		//            if (val < min)
		//                min = val;
		//        }

		//        optimal[j] = min;
		//    }

		//    // Using the optimal values just calculated, extract the text segments (all in one string)
		//    // of the optimal solution.
		//    string segmentDelimiter = "$^*";
		//    string segments = FindSegments(ref parsedText, ref penalty, ref optimal, parsedText.Length, segmentDelimiter);

		//    // We now have the answer to the problem, but it needs to be cleaned up. Parse it into
		//    // separate strings, trim off any leading or trailing whitespace, and reverse it (FindSegments
		//    // returns the answer in reverse order).
		//    string[] segDelimArray = { segmentDelimiter };
		//    parsedText = segments.Split(segDelimArray, StringSplitOptions.RemoveEmptyEntries);
		//    for (int i = 0; i < parsedText.Length; ++i)
		//        parsedText[i] = parsedText[i].Trim();
		//    Array.Reverse(parsedText);

		//    return parsedText;
		//}

		///// <summary>
		///// Helper method for calculating the square of the leftover space on a line for the given
		///// text segment word_i to word_j.
		///// </summary>
		///// <param name="parsedText">The individual words of the text.</param>
		///// <param name="i">Index of the first word in the segment.</param>
		///// <param name="j">Index of the last word in the segment.</param>
		///// <param name="fontWidth">Width of the font.</param>
		///// <param name="areaWidth">Width of the area to fit the text in.</param>
		///// <returns>The penalty of the given text segment.</returns>
		//private static uint CalculatePenalty(ref string[] parsedText, int i, int j, int fontWidth, int areaWidth)
		//{
		//    // Sum up the widths of all the words in the segment.
		//    int sum = 0;
		//    for (int k = i; k <= j; ++k)
		//        sum += parsedText[k].Length * fontWidth;

		//    // Add the widths of the spaces between them.
		//    sum += (j - i) * fontWidth;

		//    // If the leftover space is negative, then the text segment doesn't fit on a line.
		//    int leftover = areaWidth - sum;
		//    if (leftover < 0)
		//        return int.MaxValue;
		//    else
		//        return (uint)(leftover * leftover);
		//}

		///// <summary>
		///// Recursive method for extracting the text segments of the optimal solution.
		///// </summary>
		///// <param name="parsedText">The individual words of the text.</param>
		///// <param name="penalty">The penalty values for each text segment.</param>
		///// <param name="optimal">The optimal values of the solution.</param>
		///// <param name="j">Index of the last word in the segment 1 to j.</param>
		///// <param name="delimiter">Delimiter to add to the end of the segment.</param>
		///// <returns>The text segments of the optimal solution, delimited by 'delimiter', in reverse order.</returns>
		//private static string FindSegments(ref string[] parsedText, ref uint[,] penalty, ref uint[] optimal, int j, string delimiter)
		//{
		//    if (j == 0)
		//        return "";
		//    else
		//    {
		//        // Find the index of the first word, 1 <= i <= j, in the text segment word_i to word_j
		//        // that is part of the optimal solution.
		//        uint min = int.MaxValue;
		//        uint val;
		//        int index = 0;
		//        for (int i = 1; i <= j; ++i)
		//        {
		//            val = penalty[i - 1, j - 1] + optimal[i - 1];
		//            if (val < min)
		//            {
		//                min = val;
		//                index = i - 1;
		//            }
		//        }

		//        // Build the text segment.
		//        string segment = "";
		//        for (int i = index; i < j; ++i)
		//            segment += parsedText[i] + " ";

		//        // Return the text segment along with the other segments from what's left over.
		//        return segment + delimiter + FindSegments(ref parsedText, ref penalty, ref optimal, index, delimiter);
		//    }
		//}
		#endregion
	}
}
