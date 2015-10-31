// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Diagnostics;

namespace Daysim.Framework.Core {
	public sealed class Timer {
		private readonly Stopwatch _stopwatch;

		public Timer(string message = null) {
			if (message != null) {
				Console.WriteLine(message);
			}

			if (Global.PrintFile != null) {
				if (message != null) {
					Global.PrintFile.WriteLine(message);
				}

				Global.PrintFile.IncrementIndent();
			}

			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}

		public void Stop(string message = null) {
			_stopwatch.Stop();

			if (message == null) {
				message = "Time elapsed";
			}

			Console.WriteLine("{0}: {1}", message, _stopwatch.Elapsed);

			if (Global.PrintFile == null) {
				return;
			}

			Global.PrintFile.WriteLine("{0}: {1}", message, _stopwatch.Elapsed);
			Global.PrintFile.DecrementIndent();
		}
	}
}