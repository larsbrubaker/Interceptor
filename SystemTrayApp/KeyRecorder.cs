using Interceptor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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
					Debug.WriteLine(" Done Recording");
				}
				else
				{
					// start recording
					recording.Clear();
					Recording = true;
					Debug.Write("Recording ");
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
			else if (!Recording
				&& OnlyDown(new[] { Keys.Control, Keys.P }))
			{
				// play the stored keys back
				Debug.Write($"Playing ({recording.Count()}): ");

				// Don't process this key, the p will not be down due to the handled true
				keyState[Keys.P] = false;
				e.Handled = true;
				upsToConsume.Add(Keys.P);
				foreach (var tuple in recording)
				{
					if (tuple.key == Keys.Tilde)
					{
						Thread.Sleep(500);
					}
					else
					{
						input.SendKey(tuple.key, tuple.state, tuple.deviceId);
					}
				}

				Debug.WriteLine("Done");

				// exit before we get to the recording phase
				return;
			}

			if (Recording
				&& !e.Handled)
			{
				// add a pause to the output
				if(e.Key == Keys.Tilde)
				{
					Debug.Write("P");
					e.Handled = true;
					// only add the down as we are going to convert it to a wait
					upsToConsume.Add(Keys.Tilde);
					keyState[Keys.Tilde] = false;
					recording.Add((e.Key, e.State, e.DeviceId));
					return;
				}

				recording.Add((e.Key, e.State, e.DeviceId));
				if (e.Key != Keys.LeftShift
					&& e.Key != Keys.RightShift
					&& e.Key != Keys.Control
					&& e.Key != Keys.RightAlt
					&& !e.State.HasFlag(KeyState.Up))
				{
					Debug.Write(".");
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