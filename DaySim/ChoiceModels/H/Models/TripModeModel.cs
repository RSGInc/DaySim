﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.H.Models {
  public class TripModeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "HTripModeModel";
    private const int MAX_PARAMETER = 199;
    private const int TOTAL_NESTED_ALTERNATIVES = 5;
    private const int TOTAL_LEVELS = 2;
    private const int THETA_PARAMETER = 199;

    private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 0, 23 };
    private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 0, 4 };

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.TripModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(IHouseholdDayWrapper householdDay, ITripWrapper trip) {
      if (trip == null) {
        throw new ArgumentNullException("trip");
      }

      trip.PersonDay.ResetRandom(40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 40 + trip.Sequence - 1);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(trip.Id);

      IParcelWrapper originParcel =
                trip.IsHalfTourFromOrigin
                     ? trip.DestinationParcel
                     : trip.OriginParcel;

      // for skims - use actual travel direction, not simulation direction
      IParcelWrapper destinationParcel =
                 trip.IsHalfTourFromOrigin
                      ? trip.OriginParcel
                      : trip.DestinationParcel;


      int departureTime = trip.IsHalfTourFromOrigin ? trip.LatestDepartureTime : trip.EarliestDepartureTime;

      if (departureTime < 1) {
        Global.PrintFile.WriteLine("From origin / latest / earliest  {0} {1} {2}", trip.IsHalfTourFromOrigin, trip.LatestDepartureTime, trip.EarliestDepartureTime);
      }

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (destinationParcel == null || originParcel == null || trip.Mode <= Global.Settings.Modes.None || trip.Mode == Global.Settings.Modes.ParkAndRide || trip.Mode == Global.Settings.Modes.Other) {
          return;
        }

        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.RunAll(
            trip.Household.RandomUtility,
                originParcel,
                destinationParcel,
                departureTime,
                0,
                trip.Tour.DestinationPurpose,
                trip.Tour.CostCoefficient,
                trip.Tour.TimeCoefficient,
                trip.Person.IsDrivingAge,
                trip.Household.VehiclesAvailable,
                trip.Person.TransitPassOwnership,
                trip.Household.OwnsAutomatedVehicles > 0,
                trip.Person.GetTransitFareDiscountFraction(),
                false);

        // there is no path type model for school bus, use HOV3
        int mode = trip.Mode == Global.Settings.Modes.SchoolBus ? Global.Settings.Modes.Hov3 : trip.Mode;
        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == mode);

        if (!pathTypeModel.Available) {
          return;
        }

        RunModel(choiceProbabilityCalculator, trip, pathTypeModels, originParcel, destinationParcel, trip.Mode);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.RunAll(
            trip.Household.RandomUtility,
                originParcel,
                destinationParcel,
                departureTime,
                0,
                trip.Tour.DestinationPurpose,
                trip.Tour.CostCoefficient,
                trip.Tour.TimeCoefficient,
                trip.Person.IsDrivingAge,
                trip.Household.VehiclesAvailable,
                 trip.Person.TransitPassOwnership,
                trip.Household.OwnsAutomatedVehicles > 0,
                trip.Person.GetTransitFareDiscountFraction(),
                false);

        RunModel(choiceProbabilityCalculator, trip, pathTypeModels, originParcel, destinationParcel);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", trip.PersonDay.Id);
          trip.Mode = Global.Settings.Modes.Hov3;
          if (!Global.Configuration.IsInEstimationMode) {
            trip.PersonDay.IsValid = false;
          }
          return;
        }

        int choice = (int)chosenAlternative.Choice;

        trip.Mode = choice;
        if (choice == Global.Settings.Modes.SchoolBus) {
          trip.PathType = 0;
        } else {
          IPathTypeModel chosenPathType = pathTypeModels.First(x => x.Mode == choice);
          trip.PathType = chosenPathType.PathType;
          // for transit trips, overwrite origin and destination zones with stop area ids
          if (Global.StopAreaIsEnabled && choice == Global.Settings.Modes.Transit
              && Global.Configuration.WriteStopAreaIDsInsteadOfZonesForTransitTrips) {
            trip.OriginZoneKey = chosenPathType.PathOriginStopAreaKey;
            trip.DestinationZoneKey = chosenPathType.PathDestinationStopAreaKey;
          }
        }
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITripWrapper trip, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper originParcel, IParcelWrapper destinationParcel,
                                                int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = trip.Household;
      Framework.DomainModels.Models.IHouseholdTotals householdTotals = household.HouseholdTotals;
      IPersonWrapper person = trip.Person;
      ITourWrapper tour = trip.Tour;
      Framework.DomainModels.Models.IHalfTour halfTour = trip.HalfTour;

      // household inputs
      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
      int income0To25KFlag = household.Has0To25KIncome.ToFlag();
      int income25To45KFlag = household.Has25To45KIncome.ToFlag();
      int childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      int nonworkingAdults = householdTotals.NonworkingAdults;
      int retiredAdults = householdTotals.RetiredAdults;
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(household.VehiclesAvailable);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(household.VehiclesAvailable);

      // person inputs
      int drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
      int maleFlag = person.IsMale.ToFlag();
      int ageLessThan35Flag = person.AgeIsLessThan35.ToFlag();

      // tour inputs
      int parkAndRideTourFlag = tour.IsParkAndRideMode().ToFlag();
      int transitTourFlag = tour.IsTransitMode().ToFlag();
      int schoolBusTourFlag = tour.IsSchoolBusMode().ToFlag();
      int hov3TourFlag = tour.IsHov3Mode().ToFlag();
      int hov2TourFlag = tour.IsHov2Mode().ToFlag();
      int sovTourFlag = tour.IsSovMode().ToFlag();
      int bikeTourFlag = tour.IsBikeMode().ToFlag();
      int walkTourFlag = tour.IsWalkMode().ToFlag();
      int homeBasedWorkTourFlag = (tour.IsHomeBasedTour && tour.IsWorkPurpose()).ToFlag();
      int homeBasedSchoolTourFlag = (tour.IsHomeBasedTour && tour.IsSchoolPurpose()).ToFlag();
      int homeBasedEscortTourFlag = (tour.IsHomeBasedTour && tour.IsEscortPurpose()).ToFlag();
      int homeBasedShoppingTourFlag = (tour.IsHomeBasedTour && tour.IsShoppingPurpose()).ToFlag();
      int homeBasedMealTourFlag = (tour.IsHomeBasedTour && tour.IsMealPurpose()).ToFlag();
      int homeBasedSocialTourFlag = (tour.IsHomeBasedTour && tour.IsSocialPurpose()).ToFlag();
      int notHomeBasedTourFlag = (!tour.IsHomeBasedTour).ToFlag();
      int homeBasedNotWorkSchoolEscortTourFlag = (tour.IsHomeBasedTour && tour.DestinationPurpose > Global.Settings.Purposes.Escort).ToFlag();
      int jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
      int partialHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.PartialHalfTour1Sequence > 0 : tour.PartialHalfTour2Sequence > 0) ? 1 : 0;
      int fullHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.FullHalfTour1Sequence > 0 : tour.FullHalfTour2Sequence > 0) ? 1 : 0;

      // trip inputs
      int originHomeEscortFlag = (trip.IsNoneOrHomePurposeByOrigin() && trip.IsEscortPurposeByDestination()).ToFlag();
      int originWorkEscortFlag = (trip.IsWorkPurposeByOrigin() && trip.IsEscortPurposeByDestination()).ToFlag();

      int destinationHomeEscortFlag = (trip.IsNoneOrHomePurposeByDestination() && trip.IsEscortPurposeByOrigin()).ToFlag();
      int destinationWorkEscortFlag = (trip.IsWorkPurposeByDestination() && trip.IsEscortPurposeByOrigin()).ToFlag();

      // only trip on first half-tour
      int onlyTripOnFirstHalfFlag = (trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && trip.IsToTourOrigin).ToFlag();

      // first trip on first half-tour, not only one
      int firstTripOnFirstHalfFlag = (trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && !trip.IsToTourOrigin).ToFlag();

      // last trip first half-tour, not only one
      int lastTripOnFirstHalfFlag = (trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips > 1 && trip.IsToTourOrigin).ToFlag();

      // only trip on second half-tour
      int onlyTripOnSecondHalfFlag = (!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && trip.IsToTourOrigin).ToFlag();

      // first trip on second half-tour, not only one
      int firstTripOnSecondHalfFlag = (!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && !trip.IsToTourOrigin).ToFlag();

      // last trip second half-tour, not only one
      int lastTripOnSecondHalfFlag = (!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips > 1 && trip.IsToTourOrigin).ToFlag();

      // remaining inputs
      int departureTime = trip.IsHalfTourFromOrigin ? trip.LatestDepartureTime : trip.EarliestDepartureTime;

      double originMixedDensity = originParcel.MixedUse4Index1();
      double originIntersectionDensity = originParcel.NetIntersectionDensity1();
      double destinationParkingCost = destinationParcel.ParkingCostBuffer1(2);
      int amPeriodFlag = departureTime.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.TenAM).ToFlag();
      int middayPeriodFlag = departureTime.IsLeftExclusiveBetween(Global.Settings.Times.TenAM, Global.Settings.Times.ThreePM).ToFlag();
      int pmPeriodFlag = departureTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.SevenPM).ToFlag();
      int eveningPeriodFlag = (departureTime > Global.Settings.Times.SevenPM).ToFlag();

      // availability
      bool[] tripModeAvailable = new bool[Global.Settings.Modes.TotalModes];

      bool isLastTripInTour = (!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips >= 1 && trip.IsToTourOrigin);
      int frequencyPreviousTripModeIsTourMode = 0;
      if (trip.IsHalfTourFromOrigin) {
        frequencyPreviousTripModeIsTourMode += tour.HalfTourFromOrigin.Trips.Where(x => x.Sequence < trip.Sequence).Count(x => tour.Mode == x.Mode);
      } else {
        if (tour.HalfTourFromOrigin != null) {
          frequencyPreviousTripModeIsTourMode += tour.HalfTourFromOrigin.Trips.Where(x => x.Sequence > 0).Count(x => tour.Mode == x.Mode);
        }
        frequencyPreviousTripModeIsTourMode += tour.HalfTourFromDestination.Trips.Where(x => x.Sequence < trip.Sequence).Count(x => tour.Mode == x.Mode);
      }

      // if a park and ride tour, only car is available
      if (tour.Mode == Global.Settings.Modes.ParkAndRide) {
        tripModeAvailable[Global.Settings.Modes.Sov] = tour.Household.VehiclesAvailable > 0 && tour.Person.IsDrivingAge;
        tripModeAvailable[Global.Settings.Modes.Hov2] = !tripModeAvailable[Global.Settings.Modes.Sov];
      }
      // if the last trip of the tour and tour mode not yet used, only the tour mode is available
      else if (isLastTripInTour && frequencyPreviousTripModeIsTourMode == 0) {
        tripModeAvailable[tour.Mode] = true;
      } else {
        // set availability based on tour mode
        for (int mode = Global.Settings.Modes.Walk; mode <= tour.Mode; mode++) {
          tripModeAvailable[mode] = true;
        }
      }

      // school bus is a special case - use HOV3 impedance and only available for school bus tours
      IPathTypeModel pathTypeExtra = pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Hov3);
      int modeExtra = Global.Settings.Modes.SchoolBus;
      bool availableExtra = pathTypeExtra.Available && tour.IsSchoolBusMode() && tripModeAvailable[modeExtra]
                 && (trip.IsHalfTourFromOrigin
                      ? trip.LatestDepartureTime - pathTypeExtra.PathTime >= trip.ArrivalTimeLimit
                      : trip.EarliestDepartureTime + pathTypeExtra.PathTime <= trip.ArrivalTimeLimit);
      double generalizedTimeLogsumExtra = pathTypeExtra.GeneralizedTimeLogsum;

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(modeExtra, availableExtra, choice == modeExtra);
      alternative.Choice = modeExtra;

      alternative.AddNestedAlternative(_nestedAlternativeIds[modeExtra], _nestedAlternativeIndexes[modeExtra], THETA_PARAMETER);

      if (availableExtra) {
        //    case Global.Settings.Modes.SchoolBus:
        alternative.AddUtilityTerm(2, generalizedTimeLogsumExtra * tour.TimeCoefficient);
        alternative.AddUtilityTerm(18, 1);
        alternative.AddUtilityTerm(100, schoolBusTourFlag);
        alternative.AddUtilityTerm(102, (schoolBusTourFlag * onlyTripOnFirstHalfFlag));
        alternative.AddUtilityTerm(103, (schoolBusTourFlag * onlyTripOnSecondHalfFlag));
        alternative.AddUtilityTerm(104, (schoolBusTourFlag * firstTripOnFirstHalfFlag));
        alternative.AddUtilityTerm(105, (schoolBusTourFlag * firstTripOnSecondHalfFlag));
        alternative.AddUtilityTerm(106, (schoolBusTourFlag * lastTripOnFirstHalfFlag));
        alternative.AddUtilityTerm(107, (schoolBusTourFlag * lastTripOnSecondHalfFlag));
        alternative.AddUtilityTerm(112, parkAndRideTourFlag);
        alternative.AddUtilityTerm(113, transitTourFlag);
      }

      foreach (IPathTypeModel pathTypeModel in pathTypeModels) {
        int mode = pathTypeModel.Mode;
        bool available = pathTypeModel.Available && tripModeAvailable[mode]
                     && (trip.IsHalfTourFromOrigin
                          ? trip.LatestDepartureTime - pathTypeModel.PathTime >= trip.ArrivalTimeLimit
                          : trip.EarliestDepartureTime + pathTypeModel.PathTime <= trip.ArrivalTimeLimit);
        double generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

        alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
        alternative.Choice = mode;

        alternative.AddNestedAlternative(_nestedAlternativeIds[mode], _nestedAlternativeIndexes[mode], THETA_PARAMETER);

        if (!available) {
          continue;
        }

        alternative.AddUtilityTerm(2, generalizedTimeLogsum * tour.TimeCoefficient);

        if (mode == Global.Settings.Modes.Transit) {
          alternative.AddUtilityTerm(20, 1);
          alternative.AddUtilityTerm(22, carsLessThanDriversFlag);
          alternative.AddUtilityTerm(100, transitTourFlag);
          alternative.AddUtilityTerm(102, (transitTourFlag * onlyTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(103, (transitTourFlag * onlyTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(104, (transitTourFlag * firstTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(105, (transitTourFlag * firstTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(106, (transitTourFlag * lastTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(107, (transitTourFlag * lastTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(181, jointTourFlag);
          alternative.AddUtilityTerm(182, fullHalfTourFlag + partialHalfTourFlag);
        } else if (mode == Global.Settings.Modes.Hov3) {
          alternative.AddUtilityTerm(30, 1);
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT3));
          alternative.AddUtilityTerm(32, childrenAge5Through15);
          alternative.AddUtilityTerm(34, (nonworkingAdults + retiredAdults));
          alternative.AddUtilityTerm(36, onePersonHouseholdFlag);
          alternative.AddUtilityTerm(37, twoPersonHouseholdFlag);
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(100, hov3TourFlag);
          alternative.AddUtilityTerm(102, (hov3TourFlag * onlyTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(103, (hov3TourFlag * onlyTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(104, (hov3TourFlag * firstTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(105, (hov3TourFlag * firstTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(106, (hov3TourFlag * lastTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(107, (hov3TourFlag * lastTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(114, parkAndRideTourFlag);
          alternative.AddUtilityTerm(115, transitTourFlag);
          alternative.AddUtilityTerm(116, schoolBusTourFlag);
          alternative.AddUtilityTerm(149, homeBasedWorkTourFlag);
          alternative.AddUtilityTerm(150, homeBasedSchoolTourFlag);
          alternative.AddUtilityTerm(152, homeBasedEscortTourFlag);
          alternative.AddUtilityTerm(153, homeBasedShoppingTourFlag);
          alternative.AddUtilityTerm(154, homeBasedMealTourFlag);
          alternative.AddUtilityTerm(155, homeBasedSocialTourFlag);
          alternative.AddUtilityTerm(161, (destinationWorkEscortFlag * amPeriodFlag));
          alternative.AddUtilityTerm(162, (originWorkEscortFlag * pmPeriodFlag));
          alternative.AddUtilityTerm(163, (originHomeEscortFlag * amPeriodFlag));
          alternative.AddUtilityTerm(164, (originHomeEscortFlag * middayPeriodFlag));
          alternative.AddUtilityTerm(165, (originHomeEscortFlag * pmPeriodFlag));
          alternative.AddUtilityTerm(166, (originHomeEscortFlag * eveningPeriodFlag));
          alternative.AddUtilityTerm(167, (destinationHomeEscortFlag * amPeriodFlag));
          alternative.AddUtilityTerm(168, (destinationHomeEscortFlag * middayPeriodFlag));
          alternative.AddUtilityTerm(169, (destinationHomeEscortFlag * pmPeriodFlag));
          alternative.AddUtilityTerm(170, (destinationHomeEscortFlag * eveningPeriodFlag));
          alternative.AddUtilityTerm(183, jointTourFlag);
          alternative.AddUtilityTerm(184, fullHalfTourFlag + partialHalfTourFlag);
        } else if (mode == Global.Settings.Modes.Hov2) {
          alternative.AddUtilityTerm(40, 1);
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT2));
          alternative.AddUtilityTerm(32, (childrenAge5Through15 * (1 - homeBasedEscortTourFlag)));
          alternative.AddUtilityTerm(34, ((nonworkingAdults + retiredAdults) * (1 - homeBasedEscortTourFlag)));
          alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(100, hov2TourFlag);
          alternative.AddUtilityTerm(102, (hov2TourFlag * onlyTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(103, (hov2TourFlag * onlyTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(104, (hov2TourFlag * firstTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(105, (hov2TourFlag * firstTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(106, (hov2TourFlag * lastTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(107, (hov2TourFlag * lastTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(118, transitTourFlag);
          alternative.AddUtilityTerm(119, schoolBusTourFlag);
          alternative.AddUtilityTerm(120, hov3TourFlag);
          alternative.AddUtilityTerm(149, homeBasedWorkTourFlag);
          alternative.AddUtilityTerm(150, homeBasedSchoolTourFlag);
          alternative.AddUtilityTerm(152, homeBasedEscortTourFlag);
          alternative.AddUtilityTerm(153, homeBasedShoppingTourFlag);
          alternative.AddUtilityTerm(154, homeBasedMealTourFlag);
          alternative.AddUtilityTerm(155, homeBasedSocialTourFlag);
          alternative.AddUtilityTerm(161, (destinationWorkEscortFlag * amPeriodFlag));
          alternative.AddUtilityTerm(162, (originWorkEscortFlag * pmPeriodFlag));
          alternative.AddUtilityTerm(163, (originHomeEscortFlag * amPeriodFlag));
          alternative.AddUtilityTerm(164, (originHomeEscortFlag * middayPeriodFlag));
          alternative.AddUtilityTerm(165, (originHomeEscortFlag * pmPeriodFlag));
          alternative.AddUtilityTerm(166, (originHomeEscortFlag * eveningPeriodFlag));
          alternative.AddUtilityTerm(167, (destinationHomeEscortFlag * amPeriodFlag));
          alternative.AddUtilityTerm(168, (destinationHomeEscortFlag * middayPeriodFlag));
          alternative.AddUtilityTerm(169, (destinationHomeEscortFlag * pmPeriodFlag));
          alternative.AddUtilityTerm(170, (destinationHomeEscortFlag * eveningPeriodFlag));
        } else if (mode == Global.Settings.Modes.Sov) {
          alternative.AddUtilityTerm(50, 1);
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient));
          alternative.AddUtilityTerm(52, carsLessThanDriversFlag);
          alternative.AddUtilityTerm(54, income0To25KFlag);
          alternative.AddUtilityTerm(55, income25To45KFlag);
          alternative.AddUtilityTerm(59, drivingAgeStudentFlag);
          alternative.AddUtilityTerm(100, sovTourFlag);
          alternative.AddUtilityTerm(102, (sovTourFlag * onlyTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(103, (sovTourFlag * onlyTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(104, (sovTourFlag * firstTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(105, (sovTourFlag * firstTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(106, (sovTourFlag * lastTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(107, (sovTourFlag * lastTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(121, parkAndRideTourFlag);
          alternative.AddUtilityTerm(122, transitTourFlag);
          alternative.AddUtilityTerm(124, hov3TourFlag);
          alternative.AddUtilityTerm(125, hov2TourFlag);
          alternative.AddUtilityTerm(185, jointTourFlag);
          alternative.AddUtilityTerm(186, fullHalfTourFlag + partialHalfTourFlag);
        } else if (mode == Global.Settings.Modes.Bike) {
          alternative.AddUtilityTerm(60, 1);
          alternative.AddUtilityTerm(61, maleFlag);
          alternative.AddUtilityTerm(62, ageLessThan35Flag);
          alternative.AddUtilityTerm(65, originIntersectionDensity);
          alternative.AddUtilityTerm(100, bikeTourFlag);
          alternative.AddUtilityTerm(102, (bikeTourFlag * onlyTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(103, (bikeTourFlag * onlyTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(104, (bikeTourFlag * firstTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(105, (bikeTourFlag * firstTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(106, (bikeTourFlag * lastTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(107, (bikeTourFlag * lastTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(127, transitTourFlag);
          alternative.AddUtilityTerm(128, schoolBusTourFlag);
          alternative.AddUtilityTerm(130, hov2TourFlag);
          alternative.AddUtilityTerm(131, sovTourFlag);
          alternative.AddUtilityTerm(147, notHomeBasedTourFlag);
          alternative.AddUtilityTerm(187, jointTourFlag);
          alternative.AddUtilityTerm(188, fullHalfTourFlag + partialHalfTourFlag);
        } else if (mode == Global.Settings.Modes.Walk) {
          alternative.AddUtilityTerm(72, ageLessThan35Flag);
          alternative.AddUtilityTerm(75, originIntersectionDensity);
          alternative.AddUtilityTerm(78, originMixedDensity); // origin and destination mixed use measures - geometric avg. - half mile from cell, in 1000s
          alternative.AddUtilityTerm(100, walkTourFlag);
          alternative.AddUtilityTerm(102, (walkTourFlag * onlyTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(103, (walkTourFlag * onlyTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(104, (walkTourFlag * firstTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(105, (walkTourFlag * firstTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(106, (walkTourFlag * lastTripOnFirstHalfFlag));
          alternative.AddUtilityTerm(107, (walkTourFlag * lastTripOnSecondHalfFlag));
          alternative.AddUtilityTerm(141, homeBasedWorkTourFlag);
          alternative.AddUtilityTerm(142, homeBasedSchoolTourFlag);
          alternative.AddUtilityTerm(187, jointTourFlag);
          alternative.AddUtilityTerm(188, fullHalfTourFlag + partialHalfTourFlag);
        }
      }
    }
  }
}
