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
    private const int MAX_PARAMETER = 299;

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
      //var drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag(); // excluded by GV
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


        alternative.AddUtilityTerm(92, Math.Log(departurePeriodFraction));
        //alternative.AddUtilityTerm(92, halfTourFromDestinationFlag * Math.Log(departurePeriodFraction));
        alternative.AddUtilityTerm(99, tripRemainingInHalfTour / (Math.Max(1D, Math.Abs(trip.ArrivalTimeLimit - period.Middle))));
        //alternative.AddUtilityTerm(97, remainingToursCount / (Math.Max(1D, totalWindowRemaining)));
        alternative.AddUtilityTerm(98, 1000 * remainingToursCount / (Math.Max(1D, maxWindowRemaining)));

      }
    }
  }
}
