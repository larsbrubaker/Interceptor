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
			Console.WriteLine("Ctrl+R : start/stop recording keystroke");
			Console.WriteLine("Ctrl+P : play back recorded keystroke");
			Console.WriteLine("Type 'A' in this window to [A]ctivate Dvorak mapping.");

			if(args.Length > 0 
				&& args[0] == "A")
			{
				enableDvorakMapping = true;
			}

			input = new Input();
			keyRecorder = new KeyRecorder(input);

			input.KeyboardFilterMode = KeyboardFilterMode.All;

			// Finally, load the driver
			input.Load();

			input.OnKeyPressed += Input_OnKeyPressed;

			// make sure we wait around and close up nicely
			handler = new ConsoleEventDelegate(ConsoleEventCallback);
			SetConsoleCtrlHandler(handler, true);
			while (true)
			{
				var line = Console.ReadLine();
				if(line == "a" || line == "A")
				{
					enableDvorakMapping = !enableDvorakMapping;

					if(enableDvorakMapping)
					{
						Console.WriteLine("Dvorak ON");
					}
					else
					{
						Console.WriteLine("Dvorak OFF");
					}
				}
			}
		}

		static int mapToDvorakKeyboardId = 3;
		private static bool enableKeyRecording = true;
		private static KeyRecorder keyRecorder;
		private static bool enableDvorakMapping;

		private static void Input_OnKeyPressed(object sender, KeyPressedEventArgs e)
		{
			// if the key is going down and it is a key we want to map to Dvorák
			if (enableDvorakMapping
				&& e.DeviceId == mapToDvorakKeyboardId)
			{
				e.Key = QwertyToDvorak.MapKey(e.Key);
			}

			if(enableKeyRecording)
			{
				// let the macro recorder at the event
				keyRecorder.ProcessKey(ref e);
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
	}
}