using Interceptor;
using System;
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


		static int mainKeyboard = 3;

		private static void Input_OnKeyPressed(object sender, KeyPressedEventArgs e)
		{
			// if the key is going down and it is a key we want to map to Dvorák
			if (e.DeviceId == mainKeyboard
				&& e.State == KeyState.Down)
			{
				e.Key = QwertyToDvorak.MapKey(e.Key);
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
	}
}