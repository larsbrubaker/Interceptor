using Interceptor;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DvorakKeyboard
{
	internal class Program
	{
		private static ConsoleEventDelegate handler;

		// Keeps it from getting garbage collected
		private static Input input;

		// P invoke
		private delegate bool ConsoleEventDelegate(int eventType);

		private static bool ConsoleEventCallback(int eventType)
		{
			if (eventType == 2)
			{
				// if the user exits the program
				input.Unload();
			}
			return false;
		}

		private static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			input = new Input();

			input.KeyboardFilterMode = KeyboardFilterMode.All;

			// Finally, load the driver
			input.Load();

			input.OnKeyPressed += Input_OnKeyPressed;

			// make sure we wait around and close up nicely
			handler = new ConsoleEventDelegate(ConsoleEventCallback);
			SetConsoleCtrlHandler(handler, true);
			Console.ReadLine();
		}

		static Dictionary<Keys, Keys> dvorakMapping = new Dictionary<Keys, Keys>()
		{
			[Keys.DashUnderscore] = Keys.OpenBracketBrace,
			[Keys.PlusEquals] = Keys.CloseBracketBrace,
			[Keys.Q] = Keys.SingleDoubleQuote,
			[Keys.W] = Keys.CommaLeftArrow,
			[Keys.E] = Keys.PeriodRightArrow,
			[Keys.R] = Keys.P,
			[Keys.T] = Keys.Y,
			[Keys.Y] = Keys.F,
			[Keys.U] = Keys.G,
			[Keys.I] = Keys.C,
			[Keys.O] = Keys.R,
			[Keys.P] = Keys.L,
			[Keys.OpenBracketBrace] = Keys.ForwardSlashQuestionMark,
			[Keys.CloseBracketBrace] = Keys.PlusEquals,
			//    [Keys.BACK_SLASH] = Keys.BACK_SLASH,
			//    [Keys.A] = Keys.A,
			[Keys.S] = Keys.O,
			[Keys.D] = Keys.E,
			[Keys.F] = Keys.U,
			[Keys.G] = Keys.I,
			[Keys.H] = Keys.D,
			[Keys.J] = Keys.H,
			[Keys.K] = Keys.T,
			[Keys.L] = Keys.N,
			[Keys.SemicolonColon] = Keys.S,
			[Keys.SingleDoubleQuote] = Keys.DashUnderscore,
			[Keys.Z] = Keys.SemicolonColon,
			[Keys.X] = Keys.Q,
			[Keys.C] = Keys.J,
			[Keys.V] = Keys.K,
			[Keys.B] = Keys.X,
			[Keys.N] = Keys.B,
			//    [Keys.M] = Keys.M,
			[Keys.CommaLeftArrow] = Keys.W,
			[Keys.PeriodRightArrow] = Keys.V,
			[Keys.ForwardSlashQuestionMark] = Keys.Z,
		};

		static int mainKeyboard = 3;

		private static void Input_OnKeyPressed(object sender, KeyPressedEventArgs e)
		{
			// if the key is going down and it is a key we want to map to Dvorák
			if (e.DeviceId == mainKeyboard
				&& e.State == KeyState.Down
				&& dvorakMapping.ContainsKey(e.Key))
			{
				e.Key = dvorakMapping[e.Key];
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
	}
}