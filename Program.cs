using Interceptor;
using System;
using System.Runtime.InteropServices;

namespace DvorakKeyboard
{
	internal class Program
	{
		private static ConsoleEventDelegate handler;

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
	}
}