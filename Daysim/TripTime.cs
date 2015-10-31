// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Linq;
using Daysim.DomainModels;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Wrappers;

namespace Daysim {
	public sealed class TripTime {
		public const int TOTAL_TRIP_TIMES = DayPeriod.SMALL_DAY_PERIOD_TOTAL_TRIP_TIMES;

		private TripTime(int index, MinuteSpan departurePeriod) {
			Index = index;
			DeparturePeriod = departurePeriod;
		}

		public TripTime(int departureTime) {
			DecomposeTimesToPeriods(departureTime);
		}

		public int Index { get; private set; }

		public MinuteSpan DeparturePeriod { get; private set; }

		public static TripTime[] Times { get; private set; }

		private void DecomposeTimesToPeriods(int departureTime) {
			foreach (var period in DayPeriod.SmallDayPeriods.Where(period => departureTime.IsBetween(period.Start, period.End))) {
				DeparturePeriod = period;
			}

			foreach (var time in Times.Where(time => time.DeparturePeriod == DeparturePeriod)) {
				Index = time.Index;

				break;
			}
		}

		public int GetDepartureTime(ITripWrapper trip) {
			if (trip == null) {
				throw new ArgumentNullException("trip");
			}

			var timeWindow = trip.Tour.ParentTour == null ? trip.Tour.PersonDay.TimeWindow : trip.Tour.ParentTour.TimeWindow;
			var departureTime = timeWindow.GetAvailableMinute(trip.Household.RandomUtility, DeparturePeriod.Start, DeparturePeriod.End);

			//if (departureTime == Constants.DEFAULT_VALUE) {
			//	throw new InvalidDepartureTimeException();
			//}

			return departureTime;
		}

		public static void InitializeTripTimes() {
			if (Times != null) {
				return;
			}

			Times = new TripTime[TOTAL_TRIP_TIMES];

			var alternativeIndex = 0;

			foreach (var minuteSpan in DayPeriod.SmallDayPeriods) {
				var time = new TripTime(alternativeIndex, minuteSpan);

				Times[alternativeIndex++] = time;
			}
		}

		public bool Equals(TripTime other) {
			if (ReferenceEquals(null, other)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return other.Index == Index;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			if (ReferenceEquals(this, obj)) {
				return true;
			}

			return obj is TripTime && Equals((TripTime) obj);
		}

		public override int GetHashCode() {
			return Index;
		}
	}
}