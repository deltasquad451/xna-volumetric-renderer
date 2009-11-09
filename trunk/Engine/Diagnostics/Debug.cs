#region File Description
//-------------------------------------------------------------------------------------------------
// Debug.cs
//
// Static class containing various debugging methods. All methods in this class have the attribute 
// [Conditional("DEBUG")] declared so that they aren't called in Release builds, and all assertion
// methods are thread-safe.
//-------------------------------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
#endregion

namespace Engine.Diagnostics
{
	public delegate void DebugMethodDelegate();
	public delegate void ParamDebugMethodDelegate(params object[] args);

	/// <summary>
	/// Argument class used by Debug for events.
	/// </summary>
	public class DebugEventArgs : EventArgs
	{
		#region Fields
		private string[] messages;
		private string methodName;
		private string fileName;
		private int lineNumber;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the messages associated with the assert, if any.
		/// </summary>
		public string[] Messages
		{
			get
			{ return messages; }
		}

		/// <summary>
		/// Gets the method in which the assert occurred.
		/// </summary>
		public string MethodName
		{
			get
			{ return methodName; }
		}

		/// <summary>
		/// Gets the filename in which the assert occurred.
		/// </summary>
		public string FileName
		{
			get
			{ return fileName; }
		}

		/// <summary>
		/// Gets the line number of the assertion check.
		/// </summary>
		public int LineNumber
		{
			get
			{ return lineNumber; }
		}
		#endregion

		#region Initialization
		/// <param name="messages">Messages associated with the assert.</param>
		/// <param name="methodName">Method in which the assert occurred.</param>
		/// <param name="fileName">Filename in which the assert occurred.</param>
		/// <param name="lineNumber">Line number of the assertion check.</param>
		public DebugEventArgs(string[] messages, string methodName, string fileName, int lineNumber)
		{
			this.messages = messages;
			this.methodName = methodName;
			this.fileName = fileName;
			this.lineNumber = lineNumber;
		}
		#endregion
	}

	/// <summary>
	/// Static class containing various debugging methods.
	/// </summary>
	public static class Debug
	{
		#region Fields
		private static string rootDir = "";
		private static bool hasAsserted = false;
		private static bool blockAfterAssert = false;
		private static object debugLock = new object();
		private static object blockLock = new object();
		#endregion

		#region Properties
		/// <summary>
		/// Gets whether or not an assert has occurred.
		/// </summary>
		public static bool HasAsserted
		{
			get
			{ return hasAsserted; }
		}

		/// <summary>
		/// Indicates whether or not a thread should block after producing an assert. The blocking 
		/// occurs immediately after the OnAssert event is raised, and the assertion lock is held 
		/// while the thread remains blocked. Any subsequent calls to an assertion method by other 
		/// threads will also block. The original blocked thread can be unblocked with a call to 
		/// Debug.UnblockAssert() (which will unblock any threads waiting on the assertion lock).
		/// </summary>
		public static bool BlockAfterAssert
		{
			get
			{ return blockAfterAssert; }

			set
			{ blockAfterAssert = value; }
		}
		#endregion

		#region Events
		public static event EventHandler<DebugEventArgs> OnAssert;

		/// <summary>
		/// Raises the Assert event so the assert can be handled.
		/// </summary>
		/// <param name="args">Info to pass to event handlers.</param>
		private static void RaiseAssertEvent(DebugEventArgs args)
		{
			if (OnAssert != null)
				OnAssert(null, args);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Sets the name of the folder of the root development directory for the game, which the 
		/// Debug class uses to format callstack strings (it removes the path up to and including
		/// the folder name, since it's unnecessary).
		/// </summary>
		/// <param name="rootDirName">The name of folder of the root development directory, without any slashes.</param>
		[Conditional("DEBUG")]
		public static void SetRootDirName(string rootDirName)
		{
			StackTrace stackTrace = new StackTrace(true);
			StackFrame stackFrame = stackTrace.GetFrame(0);

			rootDirName = "\\" + rootDirName;
			rootDir = stackFrame.GetFileName();

			int index = rootDir.IndexOf(rootDirName + "\\");
			if (index < 0)
				rootDir = "";
			else
				rootDir = rootDir.Substring(0, index + rootDirName.Length);
		}

		/// <summary>
		/// Asserts that the supplied condition is true, and displays an error message if it's false.
		/// </summary>
		/// <param name="condition">Expression to verify.</param>
		/// <param name="messages">Optional messages for the assert.</param>
		[Conditional("DEBUG")]
		public static void Assert(bool condition, params string[] messages)
		{
			lock (debugLock)
			{
				if (!hasAsserted && !condition)
				{
					StackTrace stackTrace = new StackTrace(1, true);
					StackFrame[] stackFrames = stackTrace.GetFrames();
					StreamWriter streamWriter = File.CreateText(".\\assert.txt");

					streamWriter.WriteLine("ASSERT:");

					// Write any messages given about the assert.
					if (messages.Length > 0)
						for (int i = 0; i < messages.Length; ++i)
							streamWriter.WriteLine(messages[i] + streamWriter.NewLine);
					else
						streamWriter.WriteLine("(no info)" + streamWriter.NewLine);

					streamWriter.WriteLine("Callstack:");

					string fileName;
					int lineNumber;
					foreach (StackFrame stackFrame in stackFrames)
					{
						// If the line number is zero, then the method is not from one of our game's
						// assemblies, so we just ignore it (since Asserts are only caused by our code).
						if ((lineNumber = stackFrame.GetFileLineNumber()) == 0)
							continue;

						// Remove the base directory from the filename, since it's not really necessary.
						fileName = stackFrame.GetFileName();
						if (rootDir != "")
							fileName = fileName.Replace(rootDir, "");

						streamWriter.WriteLine("    " + stackFrame.GetMethod() + " in " + fileName + " :line " + lineNumber.ToString());
					}

					streamWriter.Close();

					// Remove the base directory from the filename, since it's not really necessary.
					fileName = stackFrames[0].GetFileName();
					if (rootDir != "")
						fileName = fileName.Replace(rootDir, "");

					hasAsserted = true;
					RaiseAssertEvent(new DebugEventArgs(messages, stackFrames[0].GetMethod().ToString(), fileName, stackFrames[0].GetFileLineNumber()));

					// Block until notified, if requested.
					if (blockAfterAssert)
						lock (blockLock)
							System.Threading.Monitor.Wait(blockLock);
				}
			}
		}

		/// <summary>
		/// Unblocks an asserted thread. Debug.BlockAfterAssert must have been set to true for the
		/// asserting thread to have blocked.
		/// </summary>
		[Conditional("DEBUG")]
		public static void UnblockAssert()
		{
			lock (blockLock)
				System.Threading.Monitor.Pulse(blockLock);
		}

		/// <summary>
		/// Runs the method supplied without any arguments. An anonymous delegate can be used
		/// with this method.
		/// </summary>
		/// <param name="method">Method to run.</param>
		[Conditional("DEBUG")]
		public static void Execute(DebugMethodDelegate method)
		{
			method();
		}

		/// <summary>
		/// Runs the method supplied with optional arguments. An anonymous delegate cannot be used
		/// with this method.
		/// </summary>
		/// <param name="method">Method to run.</param>
		/// <param name="args">Optional arguments.</param>
		[Conditional("DEBUG")]
		public static void Execute(ParamDebugMethodDelegate method, params object[] args)
		{
			method(args);
		}
		#endregion
	}
}
