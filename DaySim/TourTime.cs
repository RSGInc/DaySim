// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Linq;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim {
  public sealed class TourTime {
    public const int TOTAL_TOUR_TIMES = DayPeriod.SMALL_DAY_PERIOD_TOTAL_TOUR_TIME_COMBINATIONS;

    private TourTime(int index, MinuteSpan arrivalPeriod, MinuteSpan departurePeriod) {
      Index = index;
      ArrivalPeriod = arrivalPeriod;
      DeparturePeriod = departurePeriod;
    }

    public TourTime(int arrivalTime, int departureTime) {
      DecomposeTimesToPeriods(arrivalTime, departureTime);
    }

    public int Index { get; private set; }

    public MinuteSpan ArrivalPeriod { get; private set; }

    public MinuteSpan DeparturePeriod { get; private set; }

    public static TourTime[] Times { get; private set; }

    private void DecomposeTimesToPeriods(int arrivalTime, int departureTime) {
      foreach (MinuteSpan period in DayPeriod.SmallDayPeriods) {
        if (arrivalTime.IsBetween(period.Start, period.End)) {
          ArrivalPeriod = period;
        }

        if (departureTime.IsBetween(period.Start, period.End)) {
          DeparturePeriod = period;
        }
      }

      foreach (TourTime time in Times.Where(time => time.ArrivalPeriod == ArrivalPeriod && time.DeparturePeriod == DeparturePeriod)) {
        Index = time.Index;

        break;
      }
    }

    public IMinuteSpan GetDestinationTimes(ITourWrapper tour) {
      if (tour == null) {
        throw new ArgumentNullException("tour");
      }

      ITimeWindow timeWindow = tour.ParentTour == null ? tour.PersonDay.TimeWindow : tour.ParentTour.TimeWindow;

      return timeWindow.GetMinuteSpan(tour.Household.RandomUtility, ArrivalPeriod.Start, ArrivalPeriod.End, DeparturePeriod.Start, DeparturePeriod.End);
    }

    public bool SubtourIsWithinTour(TourWrapper subtour) {
      if (subtour == null) {
        throw new ArgumentNullException("subtour");
      }

      ITourWrapper tour = subtour.ParentTour;

      return ArrivalPeriod.Start >= tour.DestinationArrivalTime && DeparturePeriod.End <= tour.DestinationDepartureTime;
    }

    public static void InitializeTourTimes() {
      if (Times != null) {
        return;
      }

      Times = new TourTime[TOTAL_TOUR_TIMES];

      int alternativeIndex = 0;

      for (int arrivalPeriodIndex = 0; arrivalPeriodIndex < DayPeriod.SmallDayPeriods.Length; arrivalPeriodIndex++) {
        for (int departurePeriodIndex = arrivalPeriodIndex; departurePeriodIndex < DayPeriod.SmallDayPeriods.Length; departurePeriodIndex++) {
          TourTime time = new TourTime(alternativeIndex, DayPeriod.SmallDayPeriods[arrivalPeriodIndex], DayPeriod.SmallDayPeriods[departurePeriodIndex]);

          Times[alternativeIndex++] = time;
        }
      }
    }

    public bool Equals(TourTime other) {
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

      return obj is TourTime && Equals((TourTime)obj);
    }

    public override int GetHashCode() {
      return Index;
    }
  }
}