// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.H.Models {
    public class TripTimeModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HTripTimeModel";
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 156;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.TripTimeModelCoefficients, HTripTime.TOTAL_TRIP_TIMES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public void Run(IHouseholdDayWrapper householdDay, ITripWrapper trip) {
            if (trip == null) {
                throw new ArgumentNullException("trip");
            }

            trip.PersonDay.ResetRandom(40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 50 + trip.Sequence - 1);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(trip.Id);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                if (trip.DestinationParcel == null || trip.OriginParcel == null || trip.Mode <= Global.Settings.Modes.None || trip.Mode == Global.Settings.Modes.Other) {
                    return;
                }

                RunModel(choiceProbabilityCalculator, householdDay, trip, new HTripTime(trip.DepartureTime));

                choiceProbabilityCalculator.WriteObservation();
            } else {
                RunModel(choiceProbabilityCalculator, householdDay, trip);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);

                if (chosenAlternative == null) {
                    Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", trip.PersonDay.Id);
                    if (!Global.Configuration.IsInEstimationMode) {
                        trip.PersonDay.IsValid = false;
                    }
                    return;
                }

                var choice = (HTripTime)chosenAlternative.Choice;

                trip.DepartureTime = choice.GetRandomFeasibleMinute(trip, choice);

            }
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IHouseholdDayWrapper householdDay, ITripWrapper trip, HTripTime choice = null) {
            var person = trip.Person;
            var personDay = trip.PersonDay;
            var tour = trip.Tour;

            // person inputs
            var partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
            var nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
            var universityStudentFlag = person.IsUniversityStudent.ToFlag();
            var retiredAdultFlag = person.IsRetiredAdult.ToFlag();
            var drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
            var childAge5Through15Flag = person.IsChildAge5Through15.ToFlag();
            var childUnder5Flag = person.IsChildUnder5.ToFlag();

            // set tour inputs
            var workTourFlag = tour.IsWorkPurpose().ToFlag();
            var notWorkTourFlag = (!tour.IsWorkPurpose()).ToFlag();
            var notHomeBasedTourFlag = (!tour.IsHomeBasedTour).ToFlag();
            var jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
            var partialHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.PartialHalfTour1Sequence > 0 : tour.PartialHalfTour2Sequence > 0) ? 1 : 0;
            var fullHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.FullHalfTour1Sequence > 0 : tour.FullHalfTour2Sequence > 0) ? 1 : 0;

            // set trip inputs
            var originChangeMode = trip.Sequence > 1 && trip.GetPreviousTrip().DestinationPurpose == Global.Settings.Purposes.ChangeMode;
            var originSchoolFlag = trip.IsSchoolOriginPurpose().ToFlag();
            var originEscortFlag = trip.IsEscortOriginPurpose().ToFlag();
            var originShoppingFlag = trip.IsShoppingOriginPurpose().ToFlag();
            var originPersonalBusinessFlag = trip.IsPersonalBusinessOriginPurpose().ToFlag();
            var originMealFlag = trip.IsMealOriginPurpose().ToFlag();
            var originSocialFlag = trip.IsSocialOriginPurpose().ToFlag();
            var sovOrHovTripFlag = trip.UsesSovOrHovModes().ToFlag();
            var transitTripFlag = trip.IsTransitMode().ToFlag();
            var halfTourFromOriginFlag = trip.IsHalfTourFromOrigin.ToFlag();
            var halfTourFromDestinationFlag = (!trip.IsHalfTourFromOrigin).ToFlag();

            // set remaining inputs
            // set remaining inputs
            TimeWindow timeWindow = new TimeWindow();
            if (tour.JointTourSequence > 0) {
                foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
                    var tInJoint = pDay.Tours.Find(t => t.JointTourSequence == tour.JointTourSequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                    }
                }
            } else if (trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.FullHalfTour1Sequence > 0) {
                foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
                    var tInJoint = pDay.Tours.Find(t => t.FullHalfTour1Sequence == tour.FullHalfTour1Sequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                    }
                }
            } else if (trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.FullHalfTour2Sequence > 0) {
                foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
                    var tInJoint = pDay.Tours.Find(t => t.FullHalfTour2Sequence == tour.FullHalfTour2Sequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                    }
                }
            } else if (tour.ParentTour == null) {
                timeWindow.IncorporateAnotherTimeWindow(personDay.TimeWindow);
            } else {
                timeWindow.IncorporateAnotherTimeWindow(tour.ParentTour.TimeWindow);
            }

            //set the availability and impedances for the periods
            HTripTime.SetTimeImpedances(trip);

            var remainingToursCount = personDay.HomeBasedTours - personDay.GetTotalSimulatedTours();
            var tripRemainingInHalfTour = (trip.DestinationParcel != null && trip.DestinationParcel != tour.OriginParcel).ToFlag(); // we don't know exact #

            var previousArrivalTime = trip.IsHalfTourFromOrigin
                     ? (trip.Sequence == 1 ? tour.DestinationDepartureTime : trip.GetPreviousTrip().ArrivalTime)
                     : (trip.Sequence == 1 ? tour.DestinationArrivalTime : trip.GetPreviousTrip().ArrivalTime);

            var previousArrivalPeriod = new HTripTime(previousArrivalTime).DeparturePeriod;

            foreach (var time in HTripTime.Times[ParallelUtility.threadLocalAssignedIndex.Value]) {
                var period = time.DeparturePeriod;

                var departurePeriodFraction = timeWindow.TotalAvailableMinutes(period.Start, period.End) / (period.End - period.Start + 1D);

                var departureShift = period.Middle / 60.0;
                var durationShift = Math.Abs(period.Middle - previousArrivalPeriod.Middle) / 60.0;

                var available = time.Available && departurePeriodFraction > 0;

                var alternative = choiceProbabilityCalculator.GetAlternative(time.Index, available, choice != null && choice.Equals(time));


                if (!alternative.Available) {
                    continue;
                }

                alternative.Choice = time;

                var indicatedTravelTime = (int)time.ModeLOS.PathTime;
                var indicatedArrivalTime = trip.IsHalfTourFromOrigin
                     ? Math.Max(1, period.Middle - indicatedTravelTime)
                     : Math.Min(1440, period.Middle + indicatedTravelTime);

                var totalWindowRemaining = trip.IsHalfTourFromOrigin
                     ? timeWindow.TotalAvailableMinutesBefore(indicatedArrivalTime) + timeWindow.TotalAvailableMinutesAfter(previousArrivalTime)
                     : timeWindow.TotalAvailableMinutesAfter(indicatedArrivalTime) + timeWindow.TotalAvailableMinutesBefore(previousArrivalTime);

                var maxWindowRemaining = trip.IsHalfTourFromOrigin
                     ? timeWindow.MaxAvailableMinutesBefore(indicatedArrivalTime) + timeWindow.MaxAvailableMinutesAfter(previousArrivalTime)
                     : timeWindow.MaxAvailableMinutesAfter(indicatedArrivalTime) + timeWindow.MaxAvailableMinutesBefore(previousArrivalTime);

                if (trip.IsHalfTourFromOrigin) {
                    // outbound "departure" (arrival) period constants
                    alternative.AddUtilityTerm(11, time.DeparturePeriod.Middle.IsBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.SixAM).ToFlag());
                    alternative.AddUtilityTerm(12, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.SevenAM).ToFlag());
                    alternative.AddUtilityTerm(13, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.EightAM).ToFlag());
                    alternative.AddUtilityTerm(14, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.NineAM).ToFlag());
                    alternative.AddUtilityTerm(15, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.TenAM).ToFlag());
                    alternative.AddUtilityTerm(16, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenAM, Global.Settings.Times.OnePM).ToFlag());
                    alternative.AddUtilityTerm(17, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.OnePM, Global.Settings.Times.FourPM).ToFlag());
                    alternative.AddUtilityTerm(18, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.SevenPM).ToFlag());
                    alternative.AddUtilityTerm(19, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenPM, Global.Settings.Times.TenPM).ToFlag());
                    alternative.AddUtilityTerm(20, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenPM, Global.Settings.Times.MinutesInADay).ToFlag());
                } else {
                    // return departure period constants
                    alternative.AddUtilityTerm(21, time.DeparturePeriod.Middle.IsBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.SevenAM).ToFlag());
                    alternative.AddUtilityTerm(22, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.TenAM).ToFlag());
                    alternative.AddUtilityTerm(23, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenAM, Global.Settings.Times.OnePM).ToFlag());
                    alternative.AddUtilityTerm(24, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.OnePM, Global.Settings.Times.ThreePM).ToFlag());
                    alternative.AddUtilityTerm(124, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.FourPM).ToFlag());
                    alternative.AddUtilityTerm(25, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.FivePM).ToFlag());
                    alternative.AddUtilityTerm(26, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FivePM, Global.Settings.Times.SixPM).ToFlag());
                    alternative.AddUtilityTerm(27, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.SevenPM).ToFlag());
                    alternative.AddUtilityTerm(28, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenPM, Global.Settings.Times.NinePM).ToFlag());
                    alternative.AddUtilityTerm(29, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.NinePM, Global.Settings.Times.Midnight).ToFlag());
                    alternative.AddUtilityTerm(30, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.Midnight, Global.Settings.Times.MinutesInADay).ToFlag());
                }

                alternative.AddUtilityTerm(31, durationShift.IsRightExclusiveBetween(Global.Settings.Times.ZeroHours, Global.Settings.Times.OneHour).ToFlag()); // 0 - 1  
                alternative.AddUtilityTerm(32, durationShift.IsRightExclusiveBetween(Global.Settings.Times.OneHour, Global.Settings.Times.TwoHours).ToFlag()); // 1 - 2  
                alternative.AddUtilityTerm(33, durationShift.IsRightExclusiveBetween(Global.Settings.Times.TwoHours, Global.Settings.Times.ThreeHours).ToFlag()); // 2 - 3  
                alternative.AddUtilityTerm(34, durationShift.IsRightExclusiveBetween(Global.Settings.Times.ThreeHours, Global.Settings.Times.FiveHours).ToFlag()); // 3 - 5  
                alternative.AddUtilityTerm(35, durationShift.IsRightExclusiveBetween(Global.Settings.Times.FiveHours, Global.Settings.Times.SevenHours).ToFlag()); // 5 - 7  
                alternative.AddUtilityTerm(36, durationShift.IsRightExclusiveBetween(Global.Settings.Times.SevenHours, Global.Settings.Times.NineHours).ToFlag()); // 7 - 9  
                alternative.AddUtilityTerm(37, durationShift.IsRightExclusiveBetween(Global.Settings.Times.NineHours, Global.Settings.Times.TwelveHours).ToFlag()); // 9 - 12 
                alternative.AddUtilityTerm(38, durationShift.IsRightExclusiveBetween(Global.Settings.Times.TwelveHours, Global.Settings.Times.FourteenHours).ToFlag()); // 12 - 14  
                alternative.AddUtilityTerm(39, durationShift.IsRightExclusiveBetween(Global.Settings.Times.FourteenHours, Global.Settings.Times.EighteenHours).ToFlag()); // 14 - 18  
                alternative.AddUtilityTerm(40, (durationShift >= Global.Settings.Times.EighteenHours).ToFlag()); // 18 - 24  

                alternative.AddUtilityTerm(41, partTimeWorkerFlag * departureShift);
                alternative.AddUtilityTerm(43, nonworkingAdultFlag * departureShift);
                alternative.AddUtilityTerm(45, universityStudentFlag * departureShift);
                alternative.AddUtilityTerm(47, retiredAdultFlag * departureShift);
                alternative.AddUtilityTerm(49, drivingAgeStudentFlag * departureShift);
                alternative.AddUtilityTerm(51, childAge5Through15Flag * departureShift);
                alternative.AddUtilityTerm(53, childUnder5Flag * departureShift);
                alternative.AddUtilityTerm(61, jointTourFlag * departureShift);
                alternative.AddUtilityTerm(63, partialHalfTourFlag * departureShift);
                alternative.AddUtilityTerm(65, fullHalfTourFlag * departureShift);

                alternative.AddUtilityTerm(131, workTourFlag * halfTourFromOriginFlag * departureShift);
                alternative.AddUtilityTerm(133, workTourFlag * halfTourFromDestinationFlag * departureShift);
                alternative.AddUtilityTerm(135, notWorkTourFlag * halfTourFromDestinationFlag * departureShift);
                alternative.AddUtilityTerm(137, notHomeBasedTourFlag * departureShift);
                alternative.AddUtilityTerm(145, originEscortFlag * departureShift);
                alternative.AddUtilityTerm(147, originShoppingFlag * departureShift);
                alternative.AddUtilityTerm(149, originMealFlag * departureShift);
                alternative.AddUtilityTerm(151, originSocialFlag * departureShift);
                alternative.AddUtilityTerm(153, originPersonalBusinessFlag * departureShift);
                alternative.AddUtilityTerm(155, originSchoolFlag * departureShift);

                alternative.AddUtilityTerm(42, partTimeWorkerFlag * durationShift);
                alternative.AddUtilityTerm(44, nonworkingAdultFlag * durationShift);
                alternative.AddUtilityTerm(46, universityStudentFlag * durationShift);
                alternative.AddUtilityTerm(48, retiredAdultFlag * durationShift);
                alternative.AddUtilityTerm(50, drivingAgeStudentFlag * durationShift);
                alternative.AddUtilityTerm(52, childAge5Through15Flag * durationShift);
                alternative.AddUtilityTerm(54, childUnder5Flag * durationShift);
                alternative.AddUtilityTerm(62, jointTourFlag * durationShift);
                alternative.AddUtilityTerm(64, partialHalfTourFlag * durationShift);
                alternative.AddUtilityTerm(66, fullHalfTourFlag * durationShift);

                alternative.AddUtilityTerm(132, workTourFlag * halfTourFromOriginFlag * durationShift);
                alternative.AddUtilityTerm(134, workTourFlag * halfTourFromDestinationFlag * durationShift);
                alternative.AddUtilityTerm(136, notWorkTourFlag * halfTourFromDestinationFlag * durationShift);
                alternative.AddUtilityTerm(138, notHomeBasedTourFlag * durationShift);
                alternative.AddUtilityTerm(146, originEscortFlag * durationShift);
                alternative.AddUtilityTerm(148, originShoppingFlag * durationShift);
                alternative.AddUtilityTerm(150, originMealFlag * durationShift);
                alternative.AddUtilityTerm(152, originSocialFlag * durationShift);
                alternative.AddUtilityTerm(154, originPersonalBusinessFlag * durationShift);
                alternative.AddUtilityTerm(156, originSchoolFlag * durationShift);

                alternative.AddUtilityTerm(86, sovOrHovTripFlag * Math.Max(time.ModeLOS.GeneralizedTimeLogsum, 0) * tour.TimeCoefficient);
                alternative.AddUtilityTerm(88, transitTripFlag * Math.Max(time.ModeLOS.GeneralizedTimeLogsum, 0) * tour.TimeCoefficient);
                alternative.AddUtilityTerm(92, halfTourFromOriginFlag * Math.Log(departurePeriodFraction));
                alternative.AddUtilityTerm(92, halfTourFromDestinationFlag * Math.Log(departurePeriodFraction));
                alternative.AddUtilityTerm(99, tripRemainingInHalfTour / (Math.Max(1D, Math.Abs(trip.ArrivalTimeLimit - period.Middle))));
                alternative.AddUtilityTerm(97, remainingToursCount / (Math.Max(1D, totalWindowRemaining)));
                alternative.AddUtilityTerm(98, remainingToursCount / (Math.Max(1D, maxWindowRemaining)));

            }
        }
    }
}