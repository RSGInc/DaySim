// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Diagnostics;

namespace DaySim.Framework.Core {
  public sealed class Timer {
    private readonly Stopwatch _stopwatch;
    private readonly string _timerName = null;

    public Timer(string timerName = null, bool saveTimerNameForEachOutput = true) {

      if (timerName != null) {
        if (saveTimerNameForEachOutput) {
          _timerName = timerName;
        }
        Console.WriteLine(timerName);
      }

      if (Global.PrintFile != null) {
        if (timerName != null) {
          Global.PrintFile.WriteLine(timerName);
        }

        Global.PrintFile.IncrementIndent();
      }

      _stopwatch = new Stopwatch();
      _stopwatch.Start();
    }

    public void Print(string message = null) {
      if (message == null) {
        message = "Time elapsed";
      }

      if (_timerName != null) {
        message = "[" + _timerName + "] " + message;
      }

      TimeSpan elapsed = _stopwatch.Elapsed;
      Console.WriteLine("{0}: {1}", message, elapsed);

      if (Global.PrintFile == null) {
        return;
      }

      Global.PrintFile.WriteLine("{0}: {1}", message, elapsed);

    }

    public void Stop(string message = null) {
      _stopwatch.Stop();
      Print(message);

      Global.PrintFile.DecrementIndent();
    }
  }
}