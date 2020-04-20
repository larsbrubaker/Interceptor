﻿using Interceptor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DvorakKeyboard
{
	public class KeyRecorder
	{
		public bool Recording { get; private set; }
		private Dictionary<Keys, bool> keyState = new Dictionary<Keys, bool>();
		private Input input;
		private List<(Keys key, KeyState state, int deviceId)> recording = new List<(Keys key, KeyState state, int deviceId)>();
		private HashSet<Keys> upsToConsume = new HashSet<Keys>();

		public KeyRecorder(Input input)
		{
			this.input = input;
		}

		public void ProcessKey(ref KeyPressedEventArgs e)
		{
			if(upsToConsume.Count > 0
				&& e.State.HasFlag(KeyState.Up)
				&& upsToConsume.Contains(e.Key))
			{
				e.Handled = true;
				upsToConsume.Remove(e.Key);
				return;
			}

			keyState[e.Key] = (e.State.HasFlag(KeyState.Down) || e.State.HasFlag(KeyState.E0) || e.State.HasFlag(KeyState.E1))
				&& !e.State.HasFlag(KeyState.Up);

			if (OnlyDown(new[] { Keys.Control, Keys.R }))
			{
				if(Recording)
				{
					// stop recording
					Recording = false;
					Console.WriteLine("Done Recording");
				}
				else
				{
					// start recording
					recording.Clear();
					Recording = true;
					Console.Write("Recording");
				}

				// Don't process this key, the r will not be down due to the handled true
				keyState[Keys.R] = false;
				e.Handled = true;
				upsToConsume.Add(Keys.R);
				// make sure we know the control key is down
				recording.Add((Keys.Control, KeyState.Down, e.DeviceId));
				// exit before we get to the recording phase
				return;
			}
			else if (OnlyDown(new[] { Keys.Control, Keys.P }))
			{
				// play the stored keys back
				Console.WriteLine("Playing...");

				// Don't process this key, the p will not be down due to the handled true
				keyState[Keys.P] = false;
				e.Handled = true;
				upsToConsume.Add(Keys.P);
				foreach (var tuple in recording)
				{
					input.SendKey(tuple.key, tuple.state, tuple.deviceId);
				}

				// exit before we get to the recording phase
				return;
			}

			if (Recording
				&& !e.Handled)
			{
				recording.Add((e.Key, e.State, e.DeviceId));
				if (e.Key != Keys.LeftShift
					&& e.Key != Keys.RightShift
					&& e.Key != Keys.Control
					&& e.Key != Keys.RightAlt
					&& !e.State.HasFlag(KeyState.Up))
				{
					Console.Write($".");
				}
			}
		}

		/// <summary>
		/// Checks if a given  key is down
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>If it is down</returns>
		private bool IsDown(Keys key)
		{
			if(keyState.TryGetValue(key, out bool down))
			{
				return down;
			}

			return false;
		}

		/// <summary>
		/// Make sure all the keys we have requested are down and not any others.
		/// </summary>
		/// <param name="keys">The keys we are looking for.</param>
		/// <returns>If true if only the requested keys are down.</returns>
		private bool OnlyDown(Keys[] keys)
		{
			// get count of all down keys
			var downCount = keyState.Where(kvp => IsDown(kvp.Key)).Count();
			// is it the same as the number we are checking
			if(downCount == keys.Length)
			{
				// are all the keys we have requested down
				foreach(var key in keys)
				{
					if(!IsDown(key))
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}
	}
}