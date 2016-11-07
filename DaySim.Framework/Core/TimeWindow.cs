// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;

namespace DaySim.Framework.Core {
    public sealed class TimeWindow : ITimeWindow {
        private List<IMinuteSpan> _busyMinutes = new List<IMinuteSpan>();
        private List<IMinuteSpan> _availableMinutes = new List<IMinuteSpan>();

        public TimeWindow() {
            _availableMinutes = GetAvailableMinutes();
        }


        public void SetBusyMinutes(int inclusiveStart, int exclusiveEnd) {
            var duration = Math.Abs(exclusiveEnd - inclusiveStart);

            if (duration == 0) {
                return;
            }

            if (inclusiveStart > exclusiveEnd) {
                inclusiveStart = exclusiveEnd;
            }

            var inclusiveEnd = inclusiveStart + duration - 1;

            MergeBusyMinutes(inclusiveStart, inclusiveEnd);

            _availableMinutes = GetAvailableMinutes();

            if (ValidTimeWindow() == false) {
                bool testBreak = true;
            }

        }


        private void MergeBusyMinutes(int start, int end) {
            if (start < 1 || start > Global.Settings.Times.MinutesInADay || end < 1 || end > Global.Settings.Times.MinutesInADay) {
                return;
            }

            var containedInExistingRange = false;

            foreach (var busyMinute in _busyMinutes) {

                if (start.IsBetween(busyMinute.Start, busyMinute.End) && !end.IsBetween(busyMinute.Start, busyMinute.End)) {

                }

                if (busyMinute.Start.IsBetween(start, end) && busyMinute.End.IsBetween(start, end)) {
                    // busy minutes between start and end
                    // expand range of busy minutes

                    busyMinute.Start = start;
                    busyMinute.End = end;

                    containedInExistingRange = true;
                } else if (start.IsBetween(busyMinute.Start, busyMinute.End) && end.IsBetween(busyMinute.Start, busyMinute.End)) {
                    // start and end between busy minutes
                    // busy minutes already countains range

                    containedInExistingRange = true;
                } else if (start.IsBetween(busyMinute.Start, busyMinute.End)) {
                    // start between busy minutes

                    MergeBusyMinutes(busyMinute.Start, end);

                    containedInExistingRange = true;
                } else if (end.IsBetween(busyMinute.Start, busyMinute.End)) {
                    // end between busy minutes

                    MergeBusyMinutes(start, busyMinute.End);

                    containedInExistingRange = true;
                } else if (start - 1 == busyMinute.End || end + 1 == busyMinute.Start) {
                    // start or end is adjacent to busy minutes

                    if (start - 1 == busyMinute.End) {
                        start = busyMinute.Start;
                    }

                    if (end + 1 == busyMinute.Start) {
                        end = busyMinute.End;
                    }

                    MergeBusyMinutes(start, end);

                    containedInExistingRange = true;

                }
            }

            if (containedInExistingRange) {
                _busyMinutes = _busyMinutes.Distinct().ToList();
            } else {
                _busyMinutes.Add(new MinuteSpan(start, end));
            }


            _busyMinutes = _busyMinutes.OrderBy(x => x.Start).ToList();
        }


        private List<IMinuteSpan> GetAvailableMinutes() {
            var availableMinutes = new List<IMinuteSpan>();

            if (_busyMinutes.Count == 0) {
                availableMinutes.Add(new MinuteSpan(1, Global.Settings.Times.MinutesInADay));
            }

            IMinuteSpan lastBusyMinute = null;

            foreach (var busyMinute in _busyMinutes) {
                if (busyMinute.Start > 1) {
                    var start =
                        lastBusyMinute == null
                            ? 1
                            : lastBusyMinute.End + 1;

                    availableMinutes.Add(new MinuteSpan(start, busyMinute.Start - 1));
                }

                lastBusyMinute = busyMinute;
            }

            if (lastBusyMinute != null && lastBusyMinute.End < Global.Settings.Times.MinutesInADay) {
                availableMinutes.Add(new MinuteSpan(lastBusyMinute.End + 1, Global.Settings.Times.MinutesInADay));
            }

            _availableMinutes = availableMinutes;

            if (ValidTimeWindow() == false) {
                bool testBreak = true;
            }

            return availableMinutes;
        }

        public int TotalAvailableMinutes(int inclusiveStart, int inclusiveEnd) {
            // counts total unused minute slots, not unused duration.

            if (inclusiveStart < 1 || inclusiveStart > Global.Settings.Times.MinutesInADay || inclusiveEnd < 1 ||
                 inclusiveEnd > Global.Settings.Times.MinutesInADay) {
                return 0;
            }

            if (inclusiveStart > inclusiveEnd) {
                var temp = inclusiveStart;

                inclusiveStart = inclusiveEnd;
                inclusiveEnd = temp;
            }
            int totalTime = 0;
            foreach (MinuteSpan availableMinute in _availableMinutes) {
                if (inclusiveStart <= availableMinute.End && inclusiveEnd >= availableMinute.Start) {
                    int s = Math.Max(inclusiveStart, availableMinute.Start);
                    int e = Math.Min(inclusiveEnd, availableMinute.End);
                    totalTime += (e - s + 1);
                }
            }
            return totalTime;
        }

        public bool EntireSpanIsAvailable(int inclusiveStart, int inclusiveEnd) {
            // compares total unused minute slots, not unused duration.

            if (inclusiveStart < 1 || inclusiveStart > Global.Settings.Times.MinutesInADay || inclusiveEnd < 1 ||
                 inclusiveEnd > Global.Settings.Times.MinutesInADay) {
                return false;
            }

            if (inclusiveStart > inclusiveEnd) {
                var temp = inclusiveStart;

                inclusiveStart = inclusiveEnd;
                inclusiveEnd = temp;
            }

            var total = inclusiveEnd - inclusiveStart + 1;

            return total == TotalAvailableMinutes(inclusiveStart, inclusiveEnd);
        }

        public int AdjacentAvailableMinutesBefore(int minute) {
            var previous = minute - 1;

            return (from availableMinute in _availableMinutes
                    where previous.IsBetween(availableMinute.Start, availableMinute.End)
                    select previous - availableMinute.Start + 1).FirstOrDefault();
        }

        public IMinuteSpan AdjacentAvailableWindowBefore(int minute) {

            IMinuteSpan span = (from availableMinute in _availableMinutes
                                where minute.IsBetween(availableMinute.Start, availableMinute.End)
                                select availableMinute).FirstOrDefault();
            span.End = minute;
            return span;
        }

        public int AdjacentAvailableMinutesAfter(int minute) {
            var next = minute + 1;

            return (from availableMinute in _availableMinutes
                    where next.IsBetween(availableMinute.Start, availableMinute.End)
                    select availableMinute.End - next + 1).FirstOrDefault();
        }

        public IMinuteSpan AdjacentAvailableWindowAfter(int minute) {

            IMinuteSpan span = (from availableMinute in _availableMinutes
                                where minute.IsBetween(availableMinute.Start, availableMinute.End)
                                select availableMinute).FirstOrDefault();
            span.Start = minute;
            return span;
        }

        public IMinuteSpan LongestAvailableFeasibleWindow(int apEnd, int dpStart, double timeTo, double timeFrom, int mad)
        //find the longest window that starts at least timeTo before apEnd and ends at least timeFrom after dpStart, and has total
        //     duration of at least timeTo + mad + timeFrom  (used in HTourModeTime)
        {

            IMinuteSpan span = null;
            var maxDuration = 0;
            foreach (var availableMinute in _availableMinutes.Where(availableMinute =>
                                                                                      availableMinute.Start + timeTo <= apEnd
                                                                                      && availableMinute.End - timeFrom >= dpStart
                                                                                      &&
                                                                                      availableMinute.End - availableMinute.Start >=
                                                                                      timeTo + mad + timeFrom)) {
                if (availableMinute.End - availableMinute.Start > maxDuration) {
                    span = availableMinute;
                    maxDuration = span.End - span.Start;
                }
            }
            return span;
        }

        public int TotalAvailableMinutesBefore(int minute) {
            var previous = minute - 1;
            var total = 0;

            foreach (
                var availableMinute in
                    _availableMinutes.Where(
                        availableMinute =>
                        availableMinute.End <= previous || previous.IsBetween(availableMinute.Start, availableMinute.End))) {
                if (previous.IsBetween(availableMinute.Start, availableMinute.End)) {
                    total += (previous - availableMinute.Start + 1);
                } else {
                    total += (availableMinute.End - availableMinute.Start + 1);
                }
            }

            return total;
        }

        public int TotalAvailableMinutesAfter(int minute) {
            var next = minute + 1;
            var total = 0;

            foreach (
                var availableMinute in
                    _availableMinutes.Where(
                        availableMinute => availableMinute.Start >= next || next.IsBetween(availableMinute.Start, availableMinute.End))) {
                if (next.IsBetween(availableMinute.Start, availableMinute.End)) {
                    total += (availableMinute.End - next + 1);
                } else {
                    total += (availableMinute.End - availableMinute.Start + 1);
                }
            }

            return total;
        }

        public int MaxAvailableMinutesBefore(int minute) {
            var previous = minute - 1;
            var list =
                _availableMinutes.Where(
                    availableMinute =>
                    availableMinute.End <= previous || previous.IsBetween(availableMinute.Start, availableMinute.End))
                                      .Select(
                                          availableMinute =>
                                          previous.IsBetween(availableMinute.Start, availableMinute.End)
                                              ? new MinuteSpan(availableMinute.Start, previous)
                                              : availableMinute)
                                      .ToList();

            return list.Count == 0 ? 0 : list.Select(x => x.End - x.Start + 1).Max();
        }

        public int MaxAvailableMinutesAfter(int minute) {
            var next = minute + 1;
            var list =
                _availableMinutes.Where(
                    availableMinute => availableMinute.Start >= next || next.IsBetween(availableMinute.Start, availableMinute.End))
                                      .Select(
                                          availableMinute =>
                                          next.IsBetween(availableMinute.Start, availableMinute.End)
                                              ? new MinuteSpan(next, availableMinute.End)
                                              : availableMinute)
                                      .ToList();

            return list.Count == 0 ? 0 : list.Select(x => x.End - x.Start + 1).Max();
        }

        public int GetAvailableMinute(IRandomUtility randomUtility, int inclusiveStart, int exclusiveEnd,
                                                Bias bias = Bias.None) {
            if (inclusiveStart < 1 || inclusiveStart > Global.Settings.Times.MinutesInADay || exclusiveEnd < 1 ||
                 exclusiveEnd > Global.Settings.Times.MinutesInADay) {
                return 0;
            }

            if (inclusiveStart > exclusiveEnd) {
                var temp = inclusiveStart;

                inclusiveStart = exclusiveEnd;
                exclusiveEnd = temp;
            }

            foreach (var availableMinute in _availableMinutes) {
                if (inclusiveStart > availableMinute.End || exclusiveEnd < availableMinute.Start) {
                    continue;
                }

                double s = Math.Max(inclusiveStart, availableMinute.Start);
                double e = Math.Min(exclusiveEnd, availableMinute.End);

                var random = randomUtility.Uniform01();
                var range = random * (e - s);

                switch (bias) {
                    case Bias.Low:
                        e = s + range;
                        range = random * (e - s);

                        return (int)Math.Round(s + range);
                    case Bias.High:
                        s = e - range;
                        range = random * (e - s);

                        return (int)Math.Round(e - range);
                    default:
                        return (int)Math.Round(s + range);
                }
            }

            return Constants.DEFAULT_VALUE;
        }

        public IMinuteSpan GetMinuteSpan(IRandomUtility randomUtility, int p1Start, int p1End, int p2Start, int p2End) {
            int start;

            if (p1Start == p2Start && p1End == p2End) {
                start = GetAvailableMinute(randomUtility, p1Start, p1End, Bias.Low);
            } else {
                start = GetAvailableMinute(randomUtility, p1Start, p1End);
            }

            if (!start.IsBetween(p1Start, p1End)) {
                return null;
            }

            int end;

            if (p1Start == p2Start && p1End == p2End) {
                end = GetAvailableMinute(randomUtility, start, p2End, Bias.High);
            } else {
                end = GetAvailableMinute(randomUtility, p2Start, p2End);
            }

            return end.IsBetween(p2Start, p2End) ? new MinuteSpan(start, end) : null;
        }

        public int AvailableWindow(int minute, int direction) {
            if (direction == Global.Settings.TimeDirections.Before) {
                return AdjacentAvailableMinutesBefore(minute);
            } else if (direction == Global.Settings.TimeDirections.After) {
                return AdjacentAvailableMinutesAfter(minute);
            } else {
                return AdjacentAvailableMinutesBefore(minute) + AdjacentAvailableMinutesAfter(minute);
            }
        }

        public bool IsBusy(int minute) {
            return _busyMinutes.Any(busyMinute => minute.IsRightExclusiveBetween(busyMinute.Start, busyMinute.End));
        }

        public ITimeWindow DeepCloneToANewWindow() {
            var newTimeWindow = new TimeWindow();
            foreach (var busySpan in _busyMinutes) {
                var newBusySpan = new MinuteSpan(busySpan.Start, busySpan.End);
                newTimeWindow._busyMinutes.Add(newBusySpan);
            }
            foreach (var availableSpan in _availableMinutes) {
                var newAvailableSpan = new MinuteSpan(availableSpan.Start, availableSpan.End);
                newTimeWindow._availableMinutes.Add(newAvailableSpan);
            }
            return newTimeWindow;
        }

        public ITimeWindow IncorporateAnotherTimeWindow(ITimeWindow aOtherTimeWindow) {
            if (ValidTimeWindow() == false) {
                bool testBreak = true;
            }

            //replace this with deep clone method
            //TimeWindow otherTimeWindow = aOtherTimeWindow as TimeWindow;
            var otherTimeWindow = (TimeWindow)aOtherTimeWindow.DeepCloneToANewWindow();

            //set non-duplicates Keep to true in this time window
            int i = 0;
            int j = 0;
            foreach (var busySpan in _busyMinutes) {
                i++;
                busySpan.Keep = true;
                foreach (var otherBusySpan in _busyMinutes) {
                    j++;
                    if (i > j && busySpan.Start == otherBusySpan.Start
                        && busySpan.End == otherBusySpan.End) {
                        busySpan.Keep = false;
                    }
                }
            }
            i = 0;
            j = 0;
            foreach (var availableSpan in _availableMinutes) {
                availableSpan.Keep = true;
                foreach (var otherAvailableSpan in _availableMinutes) {
                    if (i > j && availableSpan.Start == otherAvailableSpan.Start
                        && availableSpan.End == otherAvailableSpan.End) {
                        availableSpan.Keep = false;
                    }
                }
            }

            //set non-duplicates Keep to true in other time window
            i = 0;
            j = 0;
            foreach (var busySpan in otherTimeWindow._busyMinutes) {
                busySpan.Keep = true;
                foreach (var otherBusySpan in _busyMinutes) {
                    if (i > j && busySpan.Start == otherBusySpan.Start
                        && busySpan.End == otherBusySpan.End) {
                        busySpan.Keep = false;
                    }
                }
            }
            i = 0;
            j = 0;
            foreach (var availableSpan in otherTimeWindow._availableMinutes) {
                availableSpan.Keep = true;
                foreach (var otherAvailableSpan in _availableMinutes) {
                    if (i > j && availableSpan.Start == otherAvailableSpan.Start
                        && availableSpan.End == otherAvailableSpan.End) {
                        availableSpan.Keep = false;
                    }
                }
            }

            //first, compare each pair of busy spans and don't keep ones that are completely surrounded by the other
            foreach (var newBusySpan in otherTimeWindow._busyMinutes) {
                //newBusySpan.Keep = true;

                foreach (var busySpan in _busyMinutes) {
                    if (busySpan.Keep) {

                        if (newBusySpan.Start <= busySpan.Start && newBusySpan.End >= busySpan.End) {
                            busySpan.Keep = false;
                        } else if (newBusySpan.Start >= busySpan.Start && newBusySpan.End <= busySpan.End) {
                            newBusySpan.Keep = false;
                        }
                    }
                }
            }

            // next compare again and combine those that partially overlap or are adjacent
            foreach (var newBusySpan in otherTimeWindow._busyMinutes) {
                foreach (var busySpan in _busyMinutes) {
                    if (newBusySpan.Keep && busySpan.Keep) {
                        if (busySpan.Start >= newBusySpan.Start && busySpan.Start <= newBusySpan.End + 1) {
                            busySpan.Start = newBusySpan.Start;
                            newBusySpan.Keep = false;
                        } else if (busySpan.End >= newBusySpan.Start - 1 && busySpan.End <= newBusySpan.End) {
                            busySpan.End = newBusySpan.End;
                            newBusySpan.Keep = false;
                        }
                    }
                }
            }
            // keep only newBusySpans that don't overlap (or completely surround) existing ones
            // check remaining existing ones for overlap, now that they may have been extended

            foreach (var busySpan in _busyMinutes) {
                foreach (var laterBusySpan in _busyMinutes.Where(s => s != busySpan && s.Keep && s.Start >= busySpan.Start)) {
                    if (laterBusySpan.Start <= busySpan.End) {
                        busySpan.End = laterBusySpan.End;
                        laterBusySpan.Keep = false;
                    }
                }
            }

            _busyMinutes.RemoveAll(s => !s.Keep);

            foreach (var newBusySpan in otherTimeWindow._busyMinutes.Where(s => s.Keep)) {
                _busyMinutes.Add(newBusySpan);
            }

            _busyMinutes = _busyMinutes.OrderBy(x => x.Start).ToList();

            _availableMinutes = GetAvailableMinutes();

            /* old code
            int firstBusy = 0;
            int nextAvailable = 0;
            for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
                if (otherTimeWindow.IsBusy(i) && firstBusy == 0) {
                    firstBusy = i;
                }
                if (!otherTimeWindow.IsBusy(i) && i > firstBusy && firstBusy > 0 && nextAvailable == 0) {
                    nextAvailable = i;
                }
                if (firstBusy > 0 && nextAvailable > firstBusy) {
                    //thisTimeWindow.SetBusyMinutes (firstBusy, nextAvailable - 1);
                    SetBusyMinutes(firstBusy, nextAvailable - 1);
                    firstBusy = 0;
                    nextAvailable = 0;
                }
            }
            if (firstBusy > 0) {
                SetBusyMinutes(firstBusy, Global.Settings.Times.MinutesInADay);
            */

            if (ValidTimeWindow() == false) {
                bool testBreak = true;
            }

            return this;
        }

        public bool ValidTimeWindow() {
            foreach (var busySpan in _busyMinutes) {
                if (busySpan.Start > busySpan.End) {
                    return false;
                }
                //foreach (var otherBusySpan in _busyMinutes) {
                //    if (busySpan.Index != otherBusySpan.Index) {
                //        if (busySpan.Start == otherBusySpan.Start || busySpan.End == otherBusySpan.End) {
                //            return false;
                //        }
                //    }
                //}
            }
            foreach (var span in _availableMinutes) {
                if (span.Start > span.End) {
                    return false;
                }
                //foreach (var otherSpan in _availableMinutes) {
                //    if (span.Index != otherSpan.Index) {
                //        if (span.Start == otherSpan.Start || span.End == otherSpan.End) {
                //            return false;
                //        }
                //    }
                //}
            }

            // overlapping busy and available spans
            foreach (var bSpan in _busyMinutes) {
                foreach (var aSpan in _availableMinutes) {
                    if ((bSpan.Start >= aSpan.Start && bSpan.Start <= aSpan.End)
                        || (bSpan.End >= aSpan.Start && bSpan.End <= aSpan.End)) {
                        return false;
                    }
                }
            }

            return true;
        }



    }
}