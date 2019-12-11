// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.ChoiceModels.H;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class TripTimeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumTripTimeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 499;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.TripTimeModelCoefficients, HTripTime.TOTAL_TRIP_TIMES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(HouseholdDayWrapper householdDay, TripWrapper trip) {
      if (trip == null) {
        throw new ArgumentNullException("trip");
      }

      trip.PersonDay.ResetRandom(40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 50 + trip.Sequence - 1);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(trip.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (trip.DestinationParcel == null || trip.OriginParcel == null || trip.Mode <= Global.Settings.Modes.None || trip.Mode > Global.Settings.Modes.WalkRideBike) {
          return;
        }

        RunModel(choiceProbabilityCalculator, householdDay, trip, new HTripTime(trip.DepartureTime));

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, householdDay, trip);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", trip.PersonDay.Id);
          if (Global.Configuration.IsInEstimationMode) {
            trip.PersonDay.IsValid = false;
          }
          return;
        }

        HTripTime choice = (HTripTime)chosenAlternative.Choice;

        trip.DepartureTime = choice.GetRandomFeasibleMinute(trip, choice);

      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, TripWrapper trip, HTripTime choice = null) {

      if (householdDay.Household.Id == 80066 && trip.Person.Sequence == 1 && trip.Tour.Sequence == 2
          && trip.Direction == 2 && trip.Sequence == 1) {
      }

      PersonWrapper person = (PersonWrapper)trip.Person;
      PersonDayWrapper personDay = (PersonDayWrapper)trip.PersonDay;
      TourWrapper tour = (TourWrapper)trip.Tour;

      // person inputs + househol_PFPT
      int partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
      int nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
      int universityStudentFlag = person.IsUniversityStudent.ToFlag();
      int retiredAdultFlag = person.IsRetiredAdult.ToFlag();
      int gymnasiumStudentFlag = person.IsDrivingAgeStudent.ToFlag();
      int primarySchoolChildFlag = person.IsChildAge5Through15.ToFlag();
      int preschoolChildFlag = person.IsChildUnder5.ToFlag();
      int femaleFlag = person.IsFemale.ToFlag();
      int fullTimeWorkerFlag = person.IsFulltimeWorker.ToFlag();
      int primaryFamilyTimeFlag = householdDay.PrimaryPriorityTimeFlag;

      // set tour inputs
      int workTourFlag = tour.IsWorkPurpose().ToFlag();
      int schoolTourFlag = tour.IsSchoolPurpose().ToFlag();
      int businessTourFlag = tour.IsBusinessPurpose().ToFlag();
      int escortTourFlag = tour.IsEscortPurpose().ToFlag();
      int personalBusinessTourFlag = tour.IsPersonalBusinessPurpose().ToFlag();
      int shoppingTourFlag = tour.IsShoppingPurpose().ToFlag();
      int socialTourFlag = tour.IsSocialPurpose().ToFlag();
      int workSchoolTourFlag = workTourFlag + schoolTourFlag;
      int notWorkSchoolTourFlag = 1 - workTourFlag - schoolTourFlag;
      int notWorkTourFlag = (!tour.IsWorkPurpose()).ToFlag();
      int notHomeBasedTourFlag = (!tour.IsHomeBasedTour).ToFlag();
      int jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
      int partialHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.PartialHalfTour1Sequence > 0 : tour.PartialHalfTour2Sequence > 0) ? 1 : 0;
      int fullHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.FullHalfTour1Sequence > 0 : tour.FullHalfTour2Sequence > 0) ? 1 : 0;

      // set trip inputs - travel purpose
      bool originChangeMode = trip.Sequence > 1 && trip.GetPreviousTrip().DestinationPurpose == Global.Settings.Purposes.ChangeMode;
      int originSchoolFlag = trip.IsSchoolOriginPurpose().ToFlag();
      int originEscortFlag = trip.IsEscortOriginPurpose().ToFlag();
      int originShoppingFlag = trip.IsShoppingOriginPurpose().ToFlag();
      int originPersonalBusinessFlag = trip.IsPersonalBusinessOriginPurpose().ToFlag();
      int originMealFlag = trip.IsMealOriginPurpose().ToFlag();
      int originSocialFlag = trip.IsSocialOriginPurpose().ToFlag();
      int originBusinessFlag = trip.IsBusinessOriginPurpose().ToFlag();

      // set trip inputs - travel modes
      int sovOrHovTripFlag = trip.UsesSovOrHovModes().ToFlag();
      int bikeTripFlag = trip.IsBikeMode().ToFlag();
      int walkTripFlag = trip.IsWalkMode().ToFlag();
      int transitTripFlag = trip.IsTransitMode().ToFlag();
      int carDriverAloneFlag = trip.IsSovMode().ToFlag();
      int carDriverNotAloneFlag = trip.IsHov2Mode().ToFlag();
      int carPassengerFlag = trip.IsHov3Mode().ToFlag();

      int halfTourFromOriginFlag = trip.IsHalfTourFromOrigin.ToFlag();
      int halfTourFromDestinationFlag = (!trip.IsHalfTourFromOrigin).ToFlag();

      // set remaining inputs
      // set remaining inputs
      TimeWindow timeWindow = new TimeWindow();
      if (tour.JointTourSequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.JointTourSequence == tour.JointTourSequence);
          if (!(tInJoint == null)) {
            // set jointTour time window
            timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
          }
        }
      } else if (trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.FullHalfTour1Sequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.FullHalfTour1Sequence == tour.FullHalfTour1Sequence);
          if (!(tInJoint == null)) {
            // set jointTour time window
            timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
          }
        }
      } else if (trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.FullHalfTour2Sequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.FullHalfTour2Sequence == tour.FullHalfTour2Sequence);
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

      int remainingToursCount = personDay.HomeBasedTours - personDay.GetTotalSimulatedTours();
      int tripRemainingInHalfTour = (trip.DestinationParcel != null && trip.DestinationParcel != tour.OriginParcel).ToFlag(); // we don't know exact #

      int previousArrivalTime = trip.IsHalfTourFromOrigin
                     ? (trip.Sequence == 1 ? tour.DestinationDepartureTime : trip.GetPreviousTrip().ArrivalTime)
                     : (trip.Sequence == 1 ? tour.DestinationArrivalTime : trip.GetPreviousTrip().ArrivalTime);

      MinuteSpan previousArrivalPeriod = new HTripTime(previousArrivalTime).DeparturePeriod;

      foreach (HTripTime time in HTripTime.Times[ParallelUtility.threadLocalAssignedIndex.Value]) {
        MinuteSpan period = time.DeparturePeriod;

        double departurePeriodFraction = timeWindow.TotalAvailableMinutes(period.Start, period.End) / (period.End - period.Start + 1D);

        double departureShiftHours = period.Middle / 60.0;
        int durationShiftMinutes = Math.Abs(period.Middle - previousArrivalPeriod.Middle);
        double durationShiftHours = durationShiftMinutes / 60.0;



        bool available = time.Available && departurePeriodFraction > 0;

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(time.Index, available, choice != null && choice.Equals(time));


        if (!alternative.Available) {
          continue;
        }

        alternative.Choice = time;

        int indicatedTravelTime = (int)time.ModeLOS.PathTime;
        int indicatedArrivalTime = trip.IsHalfTourFromOrigin
                     ? Math.Max(1, period.Middle - indicatedTravelTime)
                     : Math.Min(1440, period.Middle + indicatedTravelTime);

        int totalWindowRemaining = trip.IsHalfTourFromOrigin
                     ? timeWindow.TotalAvailableMinutesBefore(indicatedArrivalTime) + timeWindow.TotalAvailableMinutesAfter(previousArrivalTime)
                     : timeWindow.TotalAvailableMinutesAfter(indicatedArrivalTime) + timeWindow.TotalAvailableMinutesBefore(previousArrivalTime);

        int maxWindowRemaining = trip.IsHalfTourFromOrigin
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

          //alternative.AddUtilityTerm(17, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.OnePM, Global.Settings.Times.FourPM).ToFlag());
          //alternative.AddUtilityTerm(18, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.SevenPM).ToFlag());
          //GV changed to 3pm 
          alternative.AddUtilityTerm(17, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.OnePM, Global.Settings.Times.ThreePM).ToFlag());
          alternative.AddUtilityTerm(18, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.SixPM).ToFlag());
          alternative.AddUtilityTerm(19, time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.TenPM).ToFlag());
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

        alternative.AddUtilityTerm(31, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.ZeroHours, Global.Settings.Times.OneHour).ToFlag()); // 0 - 1  
        alternative.AddUtilityTerm(32, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.OneHour, Global.Settings.Times.TwoHours).ToFlag()); // 1 - 2  
        alternative.AddUtilityTerm(33, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwoHours, Global.Settings.Times.ThreeHours).ToFlag()); // 2 - 3  
        alternative.AddUtilityTerm(34, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.ThreeHours, Global.Settings.Times.FiveHours).ToFlag()); // 3 - 5  
        alternative.AddUtilityTerm(35, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.FiveHours, Global.Settings.Times.SevenHours).ToFlag()); // 5 - 7  
        alternative.AddUtilityTerm(36, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.SevenHours, Global.Settings.Times.NineHours).ToFlag()); // 7 - 9  
        alternative.AddUtilityTerm(37, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.NineHours, Global.Settings.Times.TwelveHours).ToFlag()); // 9 - 12 
        alternative.AddUtilityTerm(38, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwelveHours, Global.Settings.Times.FourteenHours).ToFlag()); // 12 - 14  
        alternative.AddUtilityTerm(39, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.FourteenHours, Global.Settings.Times.EighteenHours).ToFlag()); // 14 - 18  
        alternative.AddUtilityTerm(40, (durationShiftMinutes >= Global.Settings.Times.EighteenHours).ToFlag()); // 18 - 24  

        alternative.AddUtilityTerm(41, partTimeWorkerFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(43, nonworkingAdultFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(45, universityStudentFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(47, retiredAdultFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(49, femaleFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(51, primarySchoolChildFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(53, preschoolChildFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(61, jointTourFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(67, primaryFamilyTimeFlag * departureShiftHours * halfTourFromOriginFlag);

        alternative.AddUtilityTerm(131, workTourFlag * halfTourFromOriginFlag * departureShiftHours);
        alternative.AddUtilityTerm(133, workTourFlag * halfTourFromDestinationFlag * departureShiftHours);
        //alternative.AddUtilityTerm(135, notWorkTourFlag * halfTourFromDestinationFlag * departureShiftHours);
        //alternative.AddUtilityTerm(137, notHomeBasedTourFlag * departureShiftHours);
        alternative.AddUtilityTerm(145, originEscortFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(147, originShoppingFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(149, originBusinessFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(151, originSocialFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(153, originPersonalBusinessFlag * departureShiftHours * halfTourFromOriginFlag);
        alternative.AddUtilityTerm(155, originSchoolFlag * departureShiftHours * halfTourFromOriginFlag);

        alternative.AddUtilityTerm(172, partTimeWorkerFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(173, nonworkingAdultFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(174, universityStudentFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(175, retiredAdultFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(176, femaleFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(177, primarySchoolChildFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(178, preschoolChildFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(179, jointTourFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(180, primaryFamilyTimeFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(181, originEscortFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(182, originShoppingFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(183, originBusinessFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(184, originSocialFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(185, originPersonalBusinessFlag * departureShiftHours * halfTourFromDestinationFlag);
        alternative.AddUtilityTerm(186, originSchoolFlag * departureShiftHours * halfTourFromDestinationFlag);

        alternative.AddUtilityTerm(42, partTimeWorkerFlag * durationShiftHours);
        alternative.AddUtilityTerm(44, nonworkingAdultFlag * durationShiftHours);
        alternative.AddUtilityTerm(46, universityStudentFlag * durationShiftHours);
        alternative.AddUtilityTerm(48, retiredAdultFlag * durationShiftHours);
        alternative.AddUtilityTerm(50, femaleFlag * durationShiftHours);
        alternative.AddUtilityTerm(52, primarySchoolChildFlag * durationShiftHours);
        alternative.AddUtilityTerm(54, preschoolChildFlag * durationShiftHours);
        //alternative.AddUtilityTerm(62, jointTourFlag * durationShiftHours);
        //alternative.AddUtilityTerm(64, partialHalfTourFlag * durationShiftHours);
        //alternative.AddUtilityTerm(66, fullHalfTourFlag * durationShiftHours);
        alternative.AddUtilityTerm(68, primaryFamilyTimeFlag * durationShiftHours);

        alternative.AddUtilityTerm(132, workTourFlag * halfTourFromOriginFlag * durationShiftHours);
        alternative.AddUtilityTerm(134, workTourFlag * halfTourFromDestinationFlag * durationShiftHours);
        alternative.AddUtilityTerm(136, notWorkTourFlag * halfTourFromDestinationFlag * durationShiftHours);
        alternative.AddUtilityTerm(138, notHomeBasedTourFlag * durationShiftHours);
        alternative.AddUtilityTerm(146, originEscortFlag * durationShiftHours);
        alternative.AddUtilityTerm(148, originShoppingFlag * durationShiftHours);
        alternative.AddUtilityTerm(150, originBusinessFlag * durationShiftHours);
        alternative.AddUtilityTerm(152, originSocialFlag * durationShiftHours);
        alternative.AddUtilityTerm(154, originPersonalBusinessFlag * durationShiftHours);
        alternative.AddUtilityTerm(156, originSchoolFlag * durationShiftHours);



        alternative.AddUtilityTerm(158, workTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(159, workTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(160, schoolTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(161, schoolTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(162, businessTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(163, businessTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(164, escortTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(165, escortTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(166, personalBusinessTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(167, personalBusinessTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(168, shoppingTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(169, shoppingTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(170, socialTourFlag * halfTourFromOriginFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);
        alternative.AddUtilityTerm(171, socialTourFlag * halfTourFromDestinationFlag * (trip.Sequence == 1).ToFlag() * durationShiftHours);

        //alternative.AddUtilityTerm(172, workTourFlag * halfTourFromOriginFlag * (trip.Sequence==1).ToFlag() * departureShiftHours);


        alternative.AddUtilityTerm(86, sovOrHovTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        alternative.AddUtilityTerm(87, sovOrHovTripFlag * notWorkSchoolTourFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        alternative.AddUtilityTerm(88, transitTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        alternative.AddUtilityTerm(89, sovOrHovTripFlag * notWorkSchoolTourFlag * (trip.Sequence == 1).ToFlag() * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        alternative.AddUtilityTerm(90, bikeTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        alternative.AddUtilityTerm(91, walkTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        alternative.AddUtilityTerm(187, time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);

        // Split time terms by trip type.
        if (trip.Sequence == 1 && trip.IsHalfTourFromOrigin)
        {         
          alternative.AddUtilityTerm(466, sovOrHovTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(467, transitTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(468, bikeTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(469, walkTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);         
        } else if (trip.Sequence == 1) {
          alternative.AddUtilityTerm(439, sovOrHovTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(440, transitTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(441, bikeTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(442, walkTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        }  else {
          alternative.AddUtilityTerm(443, sovOrHovTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(444, transitTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(445, bikeTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
          alternative.AddUtilityTerm(446, walkTripFlag * time.ModeLOS.GeneralizedTimeLogsum * tour.TimeCoefficient);
        }


          alternative.AddUtilityTerm(92, Math.Log(departurePeriodFraction));
        //alternative.AddUtilityTerm(92, halfTourFromDestinationFlag * Math.Log(departurePeriodFraction));
        alternative.AddUtilityTerm(99, tripRemainingInHalfTour / (Math.Max(1D, Math.Abs(trip.ArrivalTimeLimit - period.Middle))));
        //alternative.AddUtilityTerm(97, remainingToursCount / (Math.Max(1D, totalWindowRemaining)));
        alternative.AddUtilityTerm(98, 1000 * remainingToursCount / (Math.Max(1D, maxWindowRemaining)));

        if (trip.Sequence == 1 && trip.IsHalfTourFromOrigin) {
          // 1st simulated trip on 1st half-tour.
          // betas 200-299
          // Modeling ‘departure’ period (arrival time at tour destination), given large time period of tour destination arrival.  
          // This is simply refining the tour destination arrival time.
          // Specify generalized time, departure period fraction, and trips/tours remaining
          // so there is a tendency to choose the time period with the best gen time of the trip ‘to’ known stop location by known mode.  
          // Constants for subintervals of the tour destination arrival big period
          // And departure shift (ie tour destination arrival shift) terms for market segments
          if (tour.DestinationArrivalBigPeriod.Index == 0) {  //early am  3-6am
            alternative.AddUtilityTerm(201, (period.Middle > 0 && period.Middle <=60).ToFlag());
            alternative.AddUtilityTerm(202, (period.Middle > 60 && period.Middle <=120).ToFlag());
            alternative.AddUtilityTerm(203, (period.Middle > 120 && period.Middle <=180).ToFlag());
          }
          else if(tour.DestinationArrivalBigPeriod.Index == 1) {  //am peak  6-9am
            alternative.AddUtilityTerm(221, (period.Middle > 180 && period.Middle <=210).ToFlag());
            alternative.AddUtilityTerm(222, (period.Middle > 210 && period.Middle <=240).ToFlag());
            alternative.AddUtilityTerm(223, (period.Middle > 240 && period.Middle <=270).ToFlag());
            alternative.AddUtilityTerm(224, (period.Middle > 270 && period.Middle <=300).ToFlag());
            alternative.AddUtilityTerm(225, (period.Middle > 300 && period.Middle <=330).ToFlag());
            alternative.AddUtilityTerm(226, (period.Middle > 330 && period.Middle <=360).ToFlag());

            alternative.AddUtilityTerm(231, departureShiftHours * workTourFlag * partTimeWorkerFlag);
            alternative.AddUtilityTerm(232, departureShiftHours * schoolTourFlag);
            alternative.AddUtilityTerm(233, departureShiftHours * schoolTourFlag * universityStudentFlag);
            alternative.AddUtilityTerm(234, departureShiftHours * schoolTourFlag * gymnasiumStudentFlag);

          }
          else if (tour.DestinationArrivalBigPeriod.Index == 2) { // midday  9am - 3:30pm
            alternative.AddUtilityTerm(241, (period.Middle > 360 && period.Middle <=390).ToFlag());
            alternative.AddUtilityTerm(242, (period.Middle > 390 && period.Middle <=450).ToFlag());
            alternative.AddUtilityTerm(243, (period.Middle > 450 && period.Middle <=510).ToFlag());
            alternative.AddUtilityTerm(244, (period.Middle > 510 && period.Middle <=570).ToFlag());
            alternative.AddUtilityTerm(245, (period.Middle > 570 && period.Middle <=630).ToFlag());
            alternative.AddUtilityTerm(246, (period.Middle > 630 && period.Middle <=690).ToFlag());
            alternative.AddUtilityTerm(247, (period.Middle > 690 && period.Middle <=750).ToFlag());

            alternative.AddUtilityTerm(447, departureShiftHours * shoppingTourFlag);
            alternative.AddUtilityTerm(448, departureShiftHours * personalBusinessTourFlag);
            alternative.AddUtilityTerm(449, departureShiftHours * socialTourFlag);
            alternative.AddUtilityTerm(450, departureShiftHours * escortTourFlag);
            alternative.AddUtilityTerm(451, departureShiftHours * businessTourFlag);
          }
          else if (tour.DestinationArrivalBigPeriod.Index == 3) { // pm peak  3:30 - 6:30pm
            alternative.AddUtilityTerm(261, (period.Middle > 750 && period.Middle <=810).ToFlag());
            alternative.AddUtilityTerm(262, (period.Middle > 810 && period.Middle <=870).ToFlag());
            alternative.AddUtilityTerm(263, (period.Middle > 870 && period.Middle <=930).ToFlag());

            alternative.AddUtilityTerm(452, departureShiftHours * shoppingTourFlag);
            alternative.AddUtilityTerm(453, departureShiftHours * personalBusinessTourFlag);
            alternative.AddUtilityTerm(454, departureShiftHours * socialTourFlag);
            alternative.AddUtilityTerm(455, departureShiftHours * escortTourFlag);
            alternative.AddUtilityTerm(456, departureShiftHours * businessTourFlag);
          }
          else if (tour.DestinationArrivalBigPeriod.Index == 4) { // evening  6:30-11pm
            alternative.AddUtilityTerm(281, (period.Middle > 930 && period.Middle <=960).ToFlag());
            alternative.AddUtilityTerm(282, (period.Middle > 960 && period.Middle <=1020).ToFlag());
            alternative.AddUtilityTerm(283, (period.Middle > 1020 && period.Middle <=1080).ToFlag());
            alternative.AddUtilityTerm(284, (period.Middle > 1080 && period.Middle <=1140).ToFlag());
            alternative.AddUtilityTerm(285, (period.Middle > 1140 && period.Middle <=1200).ToFlag());

            alternative.AddUtilityTerm(457, departureShiftHours * shoppingTourFlag);
            alternative.AddUtilityTerm(458, departureShiftHours * personalBusinessTourFlag);
            alternative.AddUtilityTerm(459, departureShiftHours * socialTourFlag);
            alternative.AddUtilityTerm(460, departureShiftHours * escortTourFlag);
            alternative.AddUtilityTerm(461, departureShiftHours * businessTourFlag);
          }
          else { // overnight  11pm-3am
            alternative.AddUtilityTerm(291, (period.Middle > 1200 && period.Middle <=1320).ToFlag());
            alternative.AddUtilityTerm(292, (period.Middle > 1320 && period.Middle <=1440).ToFlag());

          }
        } else if (trip.Sequence == 1) {
          // 1st simulated trip on 2nd half tour.
          // betas 300-399
          // Modeling departure period (departure time from tour destination), given small time period of tour destination arrival and large time period of tour destination departure.
          // Specify generalized time, so there is a tendency to  choose the time period with the best gen time of the trip ‘to’ known stop location by known mode.

          // workSchool tour duration constants for the duration of stay at tour destination
          if (workSchoolTourFlag == 1) {
            alternative.AddUtilityTerm(301, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.ZeroHours, Global.Settings.Times.OneHour).ToFlag()); // 0 - 1  
            alternative.AddUtilityTerm(302, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.OneHour, Global.Settings.Times.TwoHours).ToFlag()); // 1 - 2  
            alternative.AddUtilityTerm(303, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwoHours, Global.Settings.Times.ThreeHours).ToFlag()); // 2 - 3  
            alternative.AddUtilityTerm(304, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.ThreeHours, Global.Settings.Times.FiveHours).ToFlag()); // 3 - 5  
            alternative.AddUtilityTerm(305, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.FiveHours, Global.Settings.Times.SevenHours).ToFlag()); // 5 - 7  
            alternative.AddUtilityTerm(306, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.SevenHours, Global.Settings.Times.NineHours).ToFlag()); // 7 - 9  
            alternative.AddUtilityTerm(307, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.NineHours, Global.Settings.Times.TwelveHours).ToFlag()); // 9 - 12 
            alternative.AddUtilityTerm(308, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwelveHours, Global.Settings.Times.FourteenHours).ToFlag()); // 12 - 14  
            alternative.AddUtilityTerm(309, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.FourteenHours, Global.Settings.Times.EighteenHours).ToFlag()); // 14 - 18  
            alternative.AddUtilityTerm(310, (durationShiftMinutes >= Global.Settings.Times.EighteenHours).ToFlag());
            // durationShiftHour variables, with work tour as the base case, to capture differences in average duration.
            alternative.AddUtilityTerm(311, durationShiftHours * workTourFlag * partTimeWorkerFlag);
            alternative.AddUtilityTerm(312, durationShiftHours * schoolTourFlag);
            alternative.AddUtilityTerm(313, durationShiftHours * schoolTourFlag * universityStudentFlag);
            alternative.AddUtilityTerm(314, durationShiftHours * schoolTourFlag * gymnasiumStudentFlag);

            alternative.AddUtilityTerm(462, durationShiftMinutes.IsRightExclusiveBetween(420, 450).ToFlag()); // 7 - 7:30
            alternative.AddUtilityTerm(463, durationShiftMinutes.IsRightExclusiveBetween(450,480).ToFlag()); // 7:30 - 8:00
            alternative.AddUtilityTerm(464, durationShiftMinutes.IsRightExclusiveBetween(480,510).ToFlag()); // 8 - 8:30
            alternative.AddUtilityTerm(465, durationShiftMinutes.IsRightExclusiveBetween(510,540).ToFlag()); // 8:30 - 9:00

            // escort and non-home-based tour duration constants for the duration of stay at tour destination
          } else if (escortTourFlag == 1 || notHomeBasedTourFlag == 1) {
            alternative.AddUtilityTerm(321, durationShiftMinutes.IsRightExclusiveBetween(0, 30).ToFlag()); // 0 - 30 minutes  
            alternative.AddUtilityTerm(322, durationShiftMinutes.IsRightExclusiveBetween(30, 60).ToFlag()); // 30 -60 minutes  
            alternative.AddUtilityTerm(323, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.OneHour, Global.Settings.Times.TwoHours).ToFlag()); // 1 - 2  
            alternative.AddUtilityTerm(324, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwoHours, Global.Settings.Times.FourHours).ToFlag()); // 2 - 4  
            alternative.AddUtilityTerm(326, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.FourHours, Global.Settings.Times.EightHours).ToFlag()); // 4 - 8  
            alternative.AddUtilityTerm(326, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.EightHours, Global.Settings.Times.TwelveHours).ToFlag()); // 8 - 12  
            alternative.AddUtilityTerm(326, (durationShiftMinutes >= Global.Settings.Times.TwelveHours).ToFlag());
            // durationShiftHour variables, with escort tour as the base case, to capture differences in average duration.
            alternative.AddUtilityTerm(331, durationShiftHours * notHomeBasedTourFlag);

          } else {
            // duration constants for the duration of stay at tour destination, tour purposes other than work, school, escort
            alternative.AddUtilityTerm(341, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.ZeroHours, Global.Settings.Times.OneHour).ToFlag()); // 0 - 1  
            alternative.AddUtilityTerm(342, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.OneHour, Global.Settings.Times.TwoHours).ToFlag()); // 1 - 2  
            alternative.AddUtilityTerm(343, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwoHours, Global.Settings.Times.ThreeHours).ToFlag()); // 2 - 3  
            alternative.AddUtilityTerm(344, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.ThreeHours, Global.Settings.Times.FiveHours).ToFlag()); // 3 - 5  
            alternative.AddUtilityTerm(345, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.FiveHours, Global.Settings.Times.EightHours).ToFlag()); // 5 - 8  
            alternative.AddUtilityTerm(346, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.EightHours, Global.Settings.Times.TwelveHours).ToFlag()); // 8 - 12 
            alternative.AddUtilityTerm(347, durationShiftMinutes.IsRightExclusiveBetween(Global.Settings.Times.TwelveHours, Global.Settings.Times.EighteenHours).ToFlag()); // 14 - 18  
            alternative.AddUtilityTerm(348, (durationShiftMinutes >= Global.Settings.Times.EighteenHours).ToFlag());
            // durationShiftHour variables, with personal business tour as the base case, to capture differences in average duration.
            alternative.AddUtilityTerm(351, durationShiftHours * businessTourFlag);
            alternative.AddUtilityTerm(352, durationShiftHours * shoppingTourFlag);
            alternative.AddUtilityTerm(353, durationShiftHours * socialTourFlag);
            alternative.AddUtilityTerm(354, durationShiftHours * retiredAdultFlag);
            alternative.AddUtilityTerm(355, durationShiftHours * nonworkingAdultFlag);

          }


        } else {
          // trips other than 1st trip on half tour (for both half tours)
          // betas 400-499
          // Modeling, for trip ‘n’, the ‘departure’ period from intermediate stop location ‘n-1’, given ‘departure’ period and mode of trip ‘n-1’ and the purpose at intermediate stop ‘n-1’. Thus, this determines the duration of stay at intermediate stop ‘n-1’.  Stop location ‘n’ and mode of trip ‘n’ are also known.
          // Specify generalized time so there is a tendency to choose the time period with the best gen time of the trip to known stop location ‘n’ by known mode for trip ‘n’.

          if (originEscortFlag == 1) {
            // Escort stop duration constants for the duration of stay at stop ‘n-1’, which is abs(arrival time at stop ‘n-1’ minus ‘departure time for trip ‘n’).  Note that the purpose being considered is stop ‘n-1’ purpose.
            alternative.AddUtilityTerm(401, durationShiftMinutes.IsRightExclusiveBetween(0, 20).ToFlag());
            alternative.AddUtilityTerm(402, durationShiftMinutes.IsRightExclusiveBetween(20, 40).ToFlag());
            alternative.AddUtilityTerm(403, durationShiftMinutes.IsRightExclusiveBetween(40, 60).ToFlag());
            alternative.AddUtilityTerm(404, durationShiftMinutes.IsRightExclusiveBetween(60, 90).ToFlag());
            alternative.AddUtilityTerm(404, durationShiftMinutes.IsRightExclusiveBetween(90, 120).ToFlag());
            alternative.AddUtilityTerm(406, durationShiftMinutes.IsRightExclusiveBetween(120, 240).ToFlag());
            alternative.AddUtilityTerm(406, durationShiftMinutes.IsRightExclusiveBetween(240, 480).ToFlag());
            alternative.AddUtilityTerm(406, (durationShiftMinutes >= 480).ToFlag());
            // durationShiftHours variables, such as for 2nd half tour
            alternative.AddUtilityTerm(411, durationShiftHours * halfTourFromDestinationFlag);

          } else {
            // non-Escort stop duration constants for the duration of stay at stop ‘n-1’, which is abs(arrival time at stop ‘n-1’ minus ‘departure time for trip ‘n’).  Note that the purpose being considered is stop ‘n-1’ purpose.
            alternative.AddUtilityTerm(421, durationShiftMinutes.IsRightExclusiveBetween(0, 20).ToFlag());
            alternative.AddUtilityTerm(422, durationShiftMinutes.IsRightExclusiveBetween(20, 40).ToFlag());
            alternative.AddUtilityTerm(423, durationShiftMinutes.IsRightExclusiveBetween(40, 60).ToFlag());
            alternative.AddUtilityTerm(424, durationShiftMinutes.IsRightExclusiveBetween(60, 90).ToFlag());
            alternative.AddUtilityTerm(425, durationShiftMinutes.IsRightExclusiveBetween(90, 120).ToFlag());
            alternative.AddUtilityTerm(426, durationShiftMinutes.IsRightExclusiveBetween(120, 240).ToFlag());
            alternative.AddUtilityTerm(427, durationShiftMinutes.IsRightExclusiveBetween(240, 480).ToFlag());
            alternative.AddUtilityTerm(428, (durationShiftMinutes >= 480).ToFlag());
            // durationShiftHours variables, with personal business as base case, to capture differences in average duration.
            alternative.AddUtilityTerm(431, durationShiftHours * originSchoolFlag);
            alternative.AddUtilityTerm(432, durationShiftHours * originBusinessFlag);
            alternative.AddUtilityTerm(433, durationShiftHours * originShoppingFlag);
            alternative.AddUtilityTerm(434, durationShiftHours * originSocialFlag);
            alternative.AddUtilityTerm(435, durationShiftHours * halfTourFromDestinationFlag);
            alternative.AddUtilityTerm(436, durationShiftHours * (personDay.PatternType == 2).ToFlag());
            alternative.AddUtilityTerm(437, durationShiftHours * retiredAdultFlag);
            alternative.AddUtilityTerm(438, durationShiftHours * nonworkingAdultFlag);



          }
        }
      }
    }
  }
}
