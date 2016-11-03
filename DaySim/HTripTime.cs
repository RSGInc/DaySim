// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.PathTypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaySim {
    public sealed class HTripTime {
        public const int TOTAL_TRIP_TIMES = DayPeriod.H_SMALL_DAY_PERIOD_TOTAL_TRIP_TIMES;

        private HTripTime(int index, MinuteSpan departurePeriod) {
            Index = index;
            DeparturePeriod = departurePeriod;
        }

        public HTripTime(int departureTime) {
            FindPeriod(departureTime);
        }

        public int Index { get; private set; }

        public MinuteSpan DeparturePeriod { get; private set; }

        public static HTripTime[][] Times { get; private set; }

        public bool Available;

        public IPathTypeModel ModeLOS;

        int EarliestFeasibleDepatureTime;

        int LatestFeasibleDepartureTime;

        private void FindPeriod(int departureTime) {
            foreach (var period in DayPeriod.HSmallDayPeriods.Where(period => departureTime.IsBetween(period.Start, period.End))) {
                DeparturePeriod = period;
            }

            foreach (
                var time in Times[ParallelUtility.threadLocalAssignedIndex.Value].Where(time => time.DeparturePeriod == DeparturePeriod)) {
                Index = time.Index;

                break;
            }
        }

        public int GetRandomFeasibleMinute(ITripWrapper trip, HTripTime time) {
            if (trip == null || time == null) {
                throw new ArgumentNullException("trip time");
            }

            var timeWindow = trip.Tour.ParentTour == null ? trip.Tour.PersonDay.TimeWindow : trip.Tour.ParentTour.TimeWindow;
            var departureTime = timeWindow.GetAvailableMinute(trip.Household.RandomUtility, time.EarliestFeasibleDepatureTime, time.LatestFeasibleDepartureTime);

            //if (departureTime == Constants.DEFAULT_VALUE) {
            //	throw new InvalidDepartureTimeException();
            //}

            return departureTime;
        }

        public static void InitializeTripTimes() {
            if (Times != null) {
                return;
            }


            Times = new HTripTime[ParallelUtility.NThreads][];
            for (int i = 0; i < ParallelUtility.NThreads; i++) {
                Times[i] = new HTripTime[TOTAL_TRIP_TIMES];
                var alternativeIndex = 0;

                foreach (var minuteSpan in DayPeriod.HSmallDayPeriods) {
                    var time = new HTripTime(alternativeIndex, minuteSpan);

                    Times[i][alternativeIndex++] = time;
                }
            }
        }

        public static void SetTimeImpedances(ITripWrapper trip) {

            foreach (var time in Times[ParallelUtility.threadLocalAssignedIndex.Value]) {
                SetTimeImpedanceAndWindow(trip, time);
            }
        }

        public static void SetTimeImpedanceAndWindow(ITripWrapper trip, HTripTime time) {

            var tour = trip.Tour;
            var alternativeIndex = time.Index;
            var period = time.DeparturePeriod;

            // set mode LOS and mode availability
            if (period.End < trip.EarliestDepartureTime || period.Start > trip.LatestDepartureTime) {
                time.Available = false;
            } else {
                var pathMode = (trip.Mode >= Global.Settings.Modes.SchoolBus - 1) ? Global.Settings.Modes.Hov3 : trip.Mode;

                IEnumerable<IPathTypeModel> pathTypeModels =
                                    PathTypeModelFactory.Singleton.Run(
                                    trip.Household.RandomUtility,
                                    trip.IsHalfTourFromOrigin ? trip.DestinationParcel : trip.OriginParcel,
                                    trip.IsHalfTourFromOrigin ? trip.OriginParcel : trip.DestinationParcel,
                                    period.Middle,
                                    0,
                                    tour.DestinationPurpose,
                                    tour.CostCoefficient,
                                    tour.TimeCoefficient,
                                    tour.Person.IsDrivingAge,
                                    tour.Household.VehiclesAvailable,
                                    tour.Person.GetTransitFareDiscountFraction(),
                                    true,
                                    pathMode);

                var pathTypeModel = pathTypeModels.First(x => x.Mode == pathMode);

                time.Available = pathTypeModel.Available;
                time.ModeLOS = pathTypeModel;

                //set the feasible window within the small period, accounting for travel time, and recheck availability
                if (time.Available) {

                    time.EarliestFeasibleDepatureTime = Math.Max(period.Start,
                            trip.IsHalfTourFromOrigin
                            //JLB 20130723 replace next line
                            //? trip.ArrivalTimeLimit + - (int) (time.ModeLOS.PathTime + 0.5)
                            ? trip.ArrivalTimeLimit + (int)(time.ModeLOS.PathTime + 0.5)
                            : trip.EarliestDepartureTime);

                    time.LatestFeasibleDepartureTime = Math.Min(period.End,
                            trip.IsHalfTourFromOrigin
                            ? trip.LatestDepartureTime
                            : trip.ArrivalTimeLimit - (int)(time.ModeLOS.PathTime + 0.5));

                    time.Available = time.EarliestFeasibleDepatureTime < time.LatestFeasibleDepartureTime;
                }
            }
        }


        public bool Equals(HTripTime other) {
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

            return obj is HTripTime && Equals((HTripTime)obj);
        }

        public override int GetHashCode() {
            return Index;
        }
    }
}