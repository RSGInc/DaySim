// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.H {
  public sealed class HTourModeTime {
    public static readonly int TotalTourModeTimes = DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIME_COMBINATIONS * Global.Settings.Modes.MaxMode;

    private HTourModeTime(int index, int mode, MinuteSpan arrivalPeriod, MinuteSpan departurePeriod, int periodCombination) {
      Index = index;
      Mode = mode;
      ArrivalPeriod = arrivalPeriod;
      DeparturePeriod = departurePeriod;
      PeriodCombinationIndex = periodCombination;
    }

    public HTourModeTime(int mode, int arrivalTime, int departureTime) {
      FindIndexForModeAndPeriods(mode, arrivalTime, departureTime);
    }

    public int Index { get; private set; }

    public int Mode { get; private set; }

    public int PeriodCombinationIndex { get; private set; }

    public MinuteSpan ArrivalPeriod { get; private set; }

    public MinuteSpan DeparturePeriod { get; private set; }

    public double TravelTimeToDestination { get; private set; }

    public double TravelTimeFromDestination { get; private set; }

    public double GeneralizedTimeToDestination { get; private set; }

    public double GeneralizedTimeFromDestination { get; private set; }

    public bool ModeAvailableToDestination { get; private set; }

    public bool ModeAvailableFromDestination { get; private set; }

    public int ParkAndRideOriginStopAreaKey { get; private set; }

    public int ParkAndRideDestinationStopAreaKey { get; private set; }

    public IMinuteSpan LongestFeasibleWindow { get; private set; }


    //JLB
    public double TransitTime { get; private set; }
    public double TransitDistance { get; private set; }
    public double TransitCost { get; private set; }
    public double TransitGeneralizedTime { get; private set; }
    public double WalkTime { get; private set; }
    public double WalkDistance { get; private set; }
    public double BikeTime { get; private set; }
    public double BikeDistance { get; private set; }
    public double BikeCost { get; private set; }
    public int OriginAccessMode { get; private set; }
    public double OriginAccessTime { get; private set; }
    public double OriginAccessDistance { get; private set; }
    public double OriginAccessCost { get; private set; }
    public int DestinationAccessMode { get; private set; }
    public double DestinationAccessTime { get; private set; }
    public double DestinationAccessDistance { get; private set; }
    public double DestinationAccessCost { get; private set; }
    public double PathDistance { get; private set; }
    public double PathCost { get; private set; }

    public static HTourModeTime[][] ModeTimes { get; private set; }

    private void FindIndexForModeAndPeriods(int mode, int arrivalTime, int departureTime) {
      foreach (MinuteSpan period in DayPeriod.HBigDayPeriods) {
        if (arrivalTime.IsBetween(period.Start, period.End)) {
          ArrivalPeriod = period;
        }

        if (departureTime.IsBetween(period.Start, period.End)) {
          DeparturePeriod = period;
        }

      }
      Mode = mode;

      {

        foreach (
            HTourModeTime modeTime in
                ModeTimes[ParallelUtility.threadLocalAssignedIndex.Value].Where(
                    modeTime =>
                    modeTime.ArrivalPeriod == ArrivalPeriod && modeTime.DeparturePeriod == DeparturePeriod && modeTime.Mode == Mode)) {
          Index = modeTime.Index;

          /*
      TravelTimeToDestination = modeTime.TravelTimeToDestination;
      TravelTimeFromDestination = modeTime.TravelTimeFromDestination;
      GeneralizedTimeToDestination = modeTime.GeneralizedTimeToDestination;
      GeneralizedTimeFromDestination = modeTime.GeneralizedTimeFromDestination;
      ModeAvailableToDestination = modeTime.ModeAvailableToDestination;
      ModeAvailableFromDestination = modeTime.ModeAvailableFromDestination;
      LongestFeasibleWindow = modeTime.LongestFeasibleWindow;
      */
          break;
        }
      }
    }

    public int GetRandomDepartureTime(IHouseholdDayWrapper householdDay, ITourWrapper tour) {
      if (tour == null) {
        throw new ArgumentNullException("trip");
      }

      ITimeWindow timeWindow = tour.GetRelevantTimeWindow(householdDay);

      int departureTime = timeWindow.GetAvailableMinute(tour.Household.RandomUtility, DeparturePeriod.Start, DeparturePeriod.End);

      //if (departureTime == Constants.DEFAULT_VALUE) {
      //    throw new InvalidDepartureTimeException();
      //}

      return departureTime;
    }

    public IMinuteSpan GetRandomDestinationTimes(TimeWindow timeWindow, TourWrapper tour) {
      if (tour == null) {
        throw new ArgumentNullException("tour");
      }

      return timeWindow.GetMinuteSpan(tour.Household.RandomUtility, ArrivalPeriod.Start, ArrivalPeriod.End, DeparturePeriod.Start, DeparturePeriod.End);
    }

    public bool SubtourIsWithinTour(TourWrapper subtour) {
      if (subtour == null) {
        throw new ArgumentNullException("subtour");
      }

      ITourWrapper tour = subtour.ParentTour;

      return ArrivalPeriod.Start >= tour.DestinationArrivalTime && DeparturePeriod.End <= tour.DestinationDepartureTime;
    }

    public static void InitializeTourModeTimes() {
      {
        if (ModeTimes != null) {
          return;
        }

        ModeTimes = new HTourModeTime[ParallelUtility.NThreads][];
        for (int i = 0; i < ParallelUtility.NThreads; i++) {
          ModeTimes[i] = new HTourModeTime[TotalTourModeTimes];

          int alternativeIndex = 0;
          int periodCombinationIndex = -1;

          for (int arrivalPeriodIndex = 0;
               arrivalPeriodIndex < DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES;
               arrivalPeriodIndex++) {
            MinuteSpan arrivalPeriod = DayPeriod.HBigDayPeriods[arrivalPeriodIndex];
            for (int departurePeriodIndex = arrivalPeriodIndex;
                 departurePeriodIndex < DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES;
                 departurePeriodIndex++) {
              MinuteSpan departurePeriod = DayPeriod.HBigDayPeriods[departurePeriodIndex];
              periodCombinationIndex++;

              for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.MaxMode; mode++) {
                HTourModeTime modeTimes = new HTourModeTime(alternativeIndex, mode, arrivalPeriod, departurePeriod, periodCombinationIndex);

                ModeTimes[i][alternativeIndex++] = modeTimes;
              }
            }
          }
        }
      }
    }

    public static void SetModeTimeImpedances(IHouseholdDayWrapper householdDay, ITourWrapper tour,
        int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime, int constrainedHouseholdCars, double constrainedTransitDiscountFraction, IParcelWrapper alternativeDestination = null) {

      /*            if (householdDay.Household.Id == 80059 && tour.Person.Sequence == 2 && tour.Sequence == 2
                      && constrainedMode == 5 && constrainedArrivalTime == 354 && constrainedDepartureTime == 361) {
                      bool testBreak = true;
                  }
      */

      ITimeWindow timeWindow = (householdDay != null && tour != null) ? tour.GetRelevantTimeWindow(householdDay) : new TimeWindow();

      {
        foreach (HTourModeTime modeTimes in ModeTimes[ParallelUtility.threadLocalAssignedIndex.Value]) {
          modeTimes.LongestFeasibleWindow = null;
          if ((constrainedMode <= 0 || constrainedMode == modeTimes.Mode)
              &&
              (constrainedArrivalTime <= 0 ||
               constrainedArrivalTime.IsBetween(modeTimes.ArrivalPeriod.Start, modeTimes.ArrivalPeriod.End))
              &&
              (constrainedDepartureTime <= 0 ||
               constrainedDepartureTime.IsBetween(modeTimes.DeparturePeriod.Start, modeTimes.DeparturePeriod.End))) {

            SetImpedanceAndWindow(timeWindow, tour, modeTimes, constrainedHouseholdCars, constrainedTransitDiscountFraction, alternativeDestination);
          }
        }
      }
    }

    public static void SetImpedanceAndWindow(ITimeWindow timeWindow, ITourWrapper tour, HTourModeTime modeTimes, int constrainedHouseholdCars, double constrainedTransitDiscountFraction, IParcelWrapper alternativeDestination = null) {
      {

        int alternativeIndex = modeTimes.Index;
        MinuteSpan arrivalPeriod = modeTimes.ArrivalPeriod;
        MinuteSpan departurePeriod = modeTimes.DeparturePeriod;
        int mode = modeTimes.Mode;
        IParcelWrapper destinationParcel = (alternativeDestination != null) ? alternativeDestination : tour.DestinationParcel;

        int arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);
        int departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);
        int householdCars = constrainedHouseholdCars >= 0 ? constrainedHouseholdCars : tour.Household.VehiclesAvailable;
        double transitDiscountFraction = constrainedTransitDiscountFraction >= 0 ? constrainedTransitDiscountFraction : tour.Person.GetTransitFareDiscountFraction();


        // set round trip mode LOS and mode availability
        if (arrivalPeriodAvailableMinutes <= 0 || departurePeriodAvailableMinutes <= 0
            || (mode == Global.Settings.Modes.ParkAndRide && tour.DestinationPurpose != Global.Settings.Purposes.Work)
            || (mode == Global.Settings.Modes.SchoolBus && tour.DestinationPurpose != Global.Settings.Purposes.School)
                || (Global.Configuration.IsInEstimationMode && destinationParcel == null)) {
          modeTimes.ModeAvailableToDestination = false;
          modeTimes.ModeAvailableFromDestination = false;
        }
        //ACTUM must also use round trip path type to preserve the tour-based nonlinear gamma utility functions - disabled MB
        //else if (mode == Global.Settings.Modes.ParkAndRide) {
        /*else if (mode == Global.Settings.Modes.ParkAndRide || Global.Configuration.PathImpedance_UtilityForm_Auto == 1 ||
                 Global.Configuration.PathImpedance_UtilityForm_Transit == 1) {
          // park and ride has to use round-trip path type, approximate each half 
          IEnumerable<IPathTypeModel> pathTypeModels =
              PathTypeModelFactory.Singleton.Run(
                  tour.Household.RandomUtility,
                  tour.OriginParcel,
                  destinationParcel,
                  arrivalPeriod.Middle,
                  departurePeriod.Middle,
                  tour.DestinationPurpose,
                  tour.CostCoefficient,
                  tour.TimeCoefficient,
                  tour.Person.Age,
                  householdCars,
                  tour.Person.TransitPassOwnership,
                  tour.Household.OwnsAutomatedVehicles > 0,
                  tour.Person.PersonType,
                  false,
                  mode);

          IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == mode);

          modeTimes.ModeAvailableToDestination = pathTypeModel.Available;
          modeTimes.ModeAvailableFromDestination = pathTypeModel.Available;

          if (pathTypeModel.Available) {
            modeTimes.TravelTimeToDestination = pathTypeModel.PathTime / 2.0;
            modeTimes.GeneralizedTimeToDestination = pathTypeModel.GeneralizedTimeLogsum / 2.0;
            modeTimes.TravelTimeFromDestination = pathTypeModel.PathTime / 2.0;
            modeTimes.GeneralizedTimeFromDestination = pathTypeModel.GeneralizedTimeLogsum / 2.0;
            modeTimes.ParkAndRideOriginStopAreaKey = pathTypeModel.PathOriginStopAreaKey;
            modeTimes.ParkAndRideDestinationStopAreaKey = pathTypeModel.PathDestinationStopAreaKey;
            modeTimes.TransitTime = pathTypeModel.PathTransitTime;
            modeTimes.TransitDistance = pathTypeModel.PathTransitDistance;
            modeTimes.TransitCost = pathTypeModel.PathTransitCost;
            modeTimes.TransitGeneralizedTime = pathTypeModel.PathTransitGeneralizedTime;
            modeTimes.WalkTime = pathTypeModel.PathWalkTime;
            modeTimes.WalkDistance = pathTypeModel.PathWalkDistance;
            modeTimes.BikeTime = pathTypeModel.PathBikeTime;
            modeTimes.BikeDistance = pathTypeModel.PathBikeDistance;
            modeTimes.BikeCost = pathTypeModel.PathBikeCost;
            modeTimes.OriginAccessMode = pathTypeModel.PathOriginAccessMode;
            modeTimes.OriginAccessTime = pathTypeModel.PathOriginAccessTime;
            modeTimes.OriginAccessDistance = pathTypeModel.PathOriginAccessDistance;
            modeTimes.OriginAccessCost = pathTypeModel.PathOriginAccessCost;
            modeTimes.DestinationAccessMode = pathTypeModel.PathDestinationAccessMode;
            modeTimes.DestinationAccessTime = pathTypeModel.PathDestinationAccessTime;
            modeTimes.DestinationAccessDistance = pathTypeModel.PathDestinationAccessDistance;
            modeTimes.DestinationAccessCost = pathTypeModel.PathDestinationAccessCost;
            modeTimes.PathDistance = pathTypeModel.PathDistance;
            modeTimes.PathCost = pathTypeModel.PathCost;
          }
        }*/ else {
          // get times for each half tour separately, using HOV3 for school bus
          int pathMode = (mode == Global.Settings.Modes.SchoolBus) ? Global.Settings.Modes.Hov3 : mode;

          //if (tour.Household.Id == 80205 && tour.PersonDay.HouseholdDay.AttemptedSimulations == 9 && (pathMode == 1 || pathMode == 5)) {
          if (tour.Person.IsDrivingAge == true && tour.Household.VehiclesAvailable > 0 && pathMode == 3) {
          }
          IEnumerable<IPathTypeModel> pathTypeModels =
                        PathTypeModelFactory.Singleton.Run(
                            tour.Household.RandomUtility,
                            tour.OriginParcel,
                            destinationParcel,
                            arrivalPeriod.Middle,
                            0,
                            tour.DestinationPurpose,
                            tour.CostCoefficient,
                            tour.TimeCoefficient,
                            tour.Person.Age,
                            householdCars,
                            tour.Person.TransitPassOwnership,
                            tour.Household.OwnsAutomatedVehicles > 0,
                            tour.Person.PersonType,
                            false,
                            pathMode);

          IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == pathMode);

          modeTimes.ModeAvailableToDestination = pathTypeModel.Available;

          if (pathTypeModel.Available) {
            modeTimes.TravelTimeToDestination = pathTypeModel.PathTime;
            modeTimes.GeneralizedTimeToDestination = pathTypeModel.GeneralizedTimeLogsum;
          }

          pathTypeModels =
              PathTypeModelFactory.Singleton.Run(
                  tour.Household.RandomUtility,
                  destinationParcel,
                  tour.OriginParcel,
                  departurePeriod.Middle,
                  0,
                  tour.DestinationPurpose,
                  tour.CostCoefficient,
                  tour.TimeCoefficient,
                  tour.Person.Age,
                  householdCars,
                  tour.Person.TransitPassOwnership,
                  tour.Household.OwnsAutomatedVehicles > 0,
                  tour.Person.PersonType,
                  false,
                  pathMode);

          pathTypeModel = pathTypeModels.First(x => x.Mode == pathMode);

          modeTimes.ModeAvailableFromDestination = pathTypeModel.Available;

          if (pathTypeModel.Available) {
            modeTimes.TravelTimeFromDestination = pathTypeModel.PathTime;
            modeTimes.GeneralizedTimeFromDestination = pathTypeModel.GeneralizedTimeLogsum;
          }
        }
        if (tour.Household.Id == 2138 && tour.Person.Sequence == 1 && tour.Sequence == 1) {
        }

        if (modeTimes.ModeAvailableToDestination == false) {

        }
        if (modeTimes.ModeAvailableFromDestination == false) {

        }


        if (modeTimes.ModeAvailableToDestination && modeTimes.ModeAvailableFromDestination) {
          modeTimes.LongestFeasibleWindow = timeWindow.LongestAvailableFeasibleWindow(arrivalPeriod.End,
                                                                                      departurePeriod.Start,
                                                                                      modeTimes.TravelTimeToDestination,
                                                                                      modeTimes.TravelTimeFromDestination,
                                                                                      Global.Settings.Times.MinimumActivityDuration);
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

      return obj is HTourModeTime && Equals((HTourModeTime)obj);
    }

    public override int GetHashCode() {
      return Index;
    }
  }
}
