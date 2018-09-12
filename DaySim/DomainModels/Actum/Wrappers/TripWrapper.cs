// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class TripWrapper : Default.Wrappers.TripWrapper, IActumTripWrapper {
    private readonly IActumTrip _trip;

    [UsedImplicitly]
    public TripWrapper(IActumTrip trip, ITourWrapper tourWrapper, IHalfTour halfTour)
        //public TripWrapper(IActumTrip trip, IActumTourWrapper tourWrapper, IHalfTour halfTour)
        : base(trip, tourWrapper, halfTour) {
      _trip = trip;
    }

    #region relations properties

    //public IActumHouseholdWrapper Household { get; set; }

    //public IActumPersonWrapper Person { get; set; }

    //public IActumPersonDayWrapper PersonDay { get; set; }

    public IActumTourWrapper Tour { get; set; }

    //public IActumParcelWrapper OriginParcel { get; set; }

    //IParcel ISamplingTrip.OriginParcel {
    //	get { return OriginParcel; }
    //}

    //public IActumParcelWrapper DestinationParcel { get; set; }

    #endregion

    #region domain model properies

    public int BikePTCombination {
      get => _trip.BikePTCombination;
      set => _trip.BikePTCombination = value;
    }

    public int EscortedDestinationPurpose {
      get => _trip.EscortedDestinationPurpose;
      set => _trip.EscortedDestinationPurpose = value;
    }

    public int AccessMode {
      get => _trip.AccessMode;
      set => _trip.AccessMode = value;
    }

    public int AccessPathType {
      get => _trip.AccessPathType;
      set => _trip.AccessPathType = value;
    }

    public double AccessTime {
      get => _trip.AccessTime;
      set => _trip.AccessTime = value;
    }

    public double AccessCost {
      get => _trip.AccessCost;
      set => _trip.AccessCost = value;
    }

    public double AccessDistance {
      get => _trip.AccessDistance;
      set => _trip.AccessDistance = value;
    }

    public int AccessStopArea {
      get => _trip.AccessStopArea;
      set => _trip.AccessStopArea = value;
    }

    public int EgressMode {
      get => _trip.EgressMode;
      set => _trip.EgressMode = value;
    }

    public int EgressPathType {
      get => _trip.EgressPathType;
      set => _trip.EgressPathType = value;
    }

    public double EgressTime {
      get => _trip.EgressTime;
      set => _trip.EgressTime = value;
    }

    public double EgressCost {
      get => _trip.EgressCost;
      set => _trip.EgressCost = value;
    }

    public double EgressDistance {
      get => _trip.EgressDistance;
      set => _trip.EgressDistance = value;
    }

    public int EgressStopArea {
      get => _trip.EgressStopArea;
      set => _trip.EgressStopArea = value;
    }


    #endregion

    #region wrapper methods

    public virtual void SetDriverOrPassenger(List<ITripWrapper> trips) {
      if (Mode == Global.Settings.Modes.Walk || Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Transit || Mode == Global.Settings.Modes.SchoolBus || Mode == Global.Settings.Modes.Other) {
        DriverType = Global.Settings.DriverTypes.NotApplicable;
      } else if (Mode == Global.Settings.Modes.Sov || Mode == Global.Settings.Modes.HovDriver) {
        DriverType = Global.Settings.DriverTypes.Driver;
      } else if (Mode == Global.Settings.Modes.HovPassenger) {
        DriverType = Global.Settings.DriverTypes.Passenger;
      }
      if (Mode >= Global.Settings.Modes.Sov && Mode <= Global.Settings.Modes.Hov3 && Global.Configuration.AV_IncludeAutoTypeChoice && Tour.Household.OwnsAutomatedVehicles > 0) {
        DriverType = DriverType + 2; //two types of AV passengers so we know which trips to assign to network
      }
      if ((Mode == Global.Settings.Modes.PaidRideShare) && (Global.Configuration.AV_PaidRideShareModeUsesAVs)) {
        DriverType = DriverType + 2; //two types of AV passengers so we know which trips to assign to network
      }
    }

    public virtual void SetTripValueOfTime() {
      double costDivisor =
                Mode == Global.Settings.Modes.HovDriver && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
                    ? Global.Configuration.Coefficients_HOV2CostDivisor_Work
                    : Mode == Global.Settings.Modes.HovDriver && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
                        ? Global.Configuration.Coefficients_HOV2CostDivisor_Other
                        : Mode == Global.Settings.Modes.HovPassenger && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
                            ? Global.Configuration.Coefficients_HOV3CostDivisor_Work
                            : Mode == Global.Settings.Modes.HovPassenger && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
                                ? Global.Configuration.Coefficients_HOV3CostDivisor_Other
                                : 1.0;

      ValueOfTime = (Tour.TimeCoefficient * 60) / (Tour.CostCoefficient / costDivisor);
    }

    public virtual bool IsBusinessDestinationPurpose() {
      return DestinationPurpose == Global.Settings.Purposes.Business;
    }

    public virtual bool IsBusinessOriginPurpose() {
      return OriginPurpose == Global.Settings.Purposes.Business;
    }


    public virtual void HPTBikeTourUpdateTripValues() {
      //new version for trips on tours with mode 9, 10, 11, 13
      // for Actum 
      // assumes that mode and departure time have been set
      // assumes one of tour modes 9, 10, 11, 13

      //time windows also reset in estimation mode  - this just resets for one window
      ITimeWindow timeWindow = Tour.IsHomeBasedTour ? Tour.PersonDay.TimeWindow : Tour.ParentTour.TimeWindow;

      if (!Global.Configuration.IsInEstimationMode) {
        //some variables reset only in application mode
        HTripTime time = new HTripTime(DepartureTime);
        MinuteSpan period = time.DeparturePeriod;

        // set availability
        if (period.End < EarliestDepartureTime || period.Start > LatestDepartureTime) {
          time.Available = false;
        }


        double travelTime = Direction == 1 ? Tour.HalfTour1TravelTime : Tour.HalfTour2TravelTime;

        //set the feasible window within the small period, accounting for travel time, and recheck availability
        if (time.Available) {

          time.EarliestFeasibleDepatureTime = Math.Max(period.Start,
                  IsHalfTourFromOrigin
                  //JLB 20130723 replace next line
                  //? trip.ArrivalTimeLimit + - (int) (time.ModeLOS.PathTime + 0.5)
                  ? ArrivalTimeLimit + (int)(travelTime + 0.5)
                  : EarliestDepartureTime);

          time.LatestFeasibleDepartureTime = Math.Min(period.End,
                  IsHalfTourFromOrigin
                  ? LatestDepartureTime
                  : ArrivalTimeLimit - (int)(travelTime + 0.5));

          time.Available = time.EarliestFeasibleDepatureTime < time.LatestFeasibleDepartureTime;
        }

        //HTripTime.SetTimeImpedanceAndWindow(this, time);

        if (!time.Available) {
          if (!Global.Configuration.IsInEstimationMode) {
            PersonDay.IsValid = false;
          }
          return;
        }

        TravelTime = travelTime;
        TravelCost = Tour.TravelCostForPTBikeTour / 2.0;
        TravelDistance = Tour.TravelDistanceForPTBikeTour / 2.0;
        PathType = Tour.PathType;

        if (Direction == 1) {
          AccessCost = Tour.HalfTour1AccessCost;
          AccessDistance = Tour.HalfTour1AccessDistance;
          AccessMode = Tour.HalfTour1AccessMode;
          AccessPathType = Tour.HalfTour1AccessPathType;
          AccessStopArea = Tour.HalfTour1AccessStopAreaKey;
          AccessTime = Tour.HalfTour1AccessTime;
          EgressCost = Tour.HalfTour1EgressCost;
          EgressDistance = Tour.HalfTour1EgressDistance;
          EgressMode = Tour.HalfTour1EgressMode;
          EgressPathType = Tour.HalfTour1EgressPathType;
          EgressStopArea = Tour.HalfTour1EgressStopAreaKey;
          EgressTime = Tour.HalfTour1EgressTime;

        } else {
          AccessCost = Tour.HalfTour2AccessCost;
          AccessDistance = Tour.HalfTour2AccessDistance;
          AccessMode = Tour.HalfTour2AccessMode;
          AccessPathType = Tour.HalfTour2AccessPathType;
          AccessStopArea = Tour.HalfTour2AccessStopAreaKey;
          AccessTime = Tour.HalfTour2AccessTime;
          EgressCost = Tour.HalfTour2EgressCost;
          EgressDistance = Tour.HalfTour2EgressDistance;
          EgressMode = Tour.HalfTour2EgressMode;
          EgressPathType = Tour.HalfTour2EgressPathType;
          EgressStopArea = Tour.HalfTour2EgressStopAreaKey;
          EgressTime = Tour.HalfTour2EgressTime;
        }

        int duration = (int)(TravelTime + 0.5);

        if (duration == Constants.DEFAULT_VALUE && Global.Configuration.ReportInvalidPersonDays) {
          Global.PrintFile.WriteDurationIsInvalidWarning("TripWrapper", "UpdateTripValues", PersonDay.Id, TravelTime, TravelCost, TravelDistance);

          if (!Global.Configuration.IsInEstimationMode) {
            PersonDay.IsValid = false;
          }

          return;
        }

        ArrivalTime =
            IsHalfTourFromOrigin
                ? Math.Max(1, DepartureTime - duration)
                : Math.Min(Global.Settings.Times.MinutesInADay, DepartureTime + duration);

        /* doesn't have much effect - turn off for now
                             if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime))   {
                                  // move entire trip up to 15 minutes later or earlier depending on half tour direction.  
                                  // Find the smallest shift that will make the arrival time a non-busy minute while still leaving 
                                  // a gap between the departure time and the arrival time at the trip origin (non-0 activity duration)
                                  //NOTE: This was copied over from the old version above.
                                  // This could possibly cause some inconsistencies for times for different people on joint tours, if it is done separately for each
                                  // (will work better if done before cloning....)
                                  const int moveLimit = 15;

                                  if (IsHalfTourFromOrigin)     {
                                        int originArrivalTime = Sequence == 1 ? Tour.DestinationDepartureTime : PreviousTrip.ArrivalTime;
                                        int moveLater = 0;
                                        do       {
                                             moveLater++;
                                        } while (moveLater <= moveLimit && DepartureTime + moveLater < originArrivalTime && timeWindow.IsBusy(ArrivalTime + moveLater));

                                        if (!timeWindow.IsBusy(ArrivalTime + moveLater)) {
                                             ArrivalTime += moveLater;
                                             DepartureTime += moveLater;
                                             if (Sequence == 1) Tour.DestinationArrivalTime += moveLater;
                                             if (Global.Configuration.ReportInvalidPersonDays) Global.PrintFile.WriteLine("Tour {0}. Arrival time moved later by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveLater, DepartureTime, originArrivalTime);
                                        }
                                  }
                                  else  {
                                        int originArrivalTime = Sequence == 1 ? Tour.DestinationArrivalTime : PreviousTrip.ArrivalTime;
                                        int moveEarlier = 0;
                                        do   {
                                             moveEarlier++;
                                        } while (moveEarlier <= moveLimit && DepartureTime - moveEarlier > originArrivalTime && timeWindow.IsBusy(ArrivalTime - moveEarlier));

                                        if (!timeWindow.IsBusy(ArrivalTime - moveEarlier))   {
                                             ArrivalTime -= moveEarlier;
                                             DepartureTime -= moveEarlier;
                                             if (Sequence == 1) Tour.DestinationDepartureTime -= moveEarlier;
                                             if (Global.Configuration.ReportInvalidPersonDays) Global.PrintFile.WriteLine("Tour {0}. Arrival time moved earlier by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveEarlier, DepartureTime, originArrivalTime);
                                        }
                                  }
                             }
        */
        //check again after possible adjustment

        if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime)) {
          if (Global.Configuration.ReportInvalidPersonDays) {
            Global.PrintFile.WriteLine("Arrival time {0} is busy for trip {1}.", ArrivalTime, Id);
          }

          if (!Global.Configuration.IsInEstimationMode) {
            PersonDay.IsValid = false;
          }
        } else //check if another trip needs to be scheduled and there only a few minutes left
            if ((IsHalfTourFromOrigin && ArrivalTime < Tour.EarliestOriginDepartureTime + 3 && DestinationParcel != Tour.OriginParcel) || (!IsHalfTourFromOrigin && ArrivalTime > Tour.LatestOriginArrivalTime - 3 && DestinationParcel != Tour.OriginParcel)) {
          if (!Global.Configuration.IsInEstimationMode) {
            PersonDay.IsValid = false;
          }
        }

        if (Global.Configuration.TraceModelResultValidity) {
          if (PersonDay.HouseholdDay.AttemptedSimulations >= Global.Configuration.InvalidAttemptsBeforeTrace) {
            Global.PrintFile.WriteLine("  >> HUpdateTripValues HH/P/T/Hf/T/Arrival time/valid {0} {1} {2} {3} {4} {5} {6}", Household.Id, Person.Sequence, Tour.Sequence, Direction, Sequence, ArrivalTime, PersonDay.IsValid);
          }
        }

        if (!PersonDay.IsValid) {
          return;
        }

        //if first trip in half tour, use departure time to reset tour times
        if (Sequence == 1) {
          if (IsHalfTourFromOrigin) {
            Tour.DestinationArrivalTime = DepartureTime;
          } else {
            Tour.DestinationDepartureTime = DepartureTime;
          }
        }
      }

      //adjust the time window for busy minutes at the stop origin and during the trip - done also in estimation mode
      int earliestBusyMinute =
                IsHalfTourFromOrigin
                    ? ArrivalTime
                    : Sequence == 1
                        ? Tour.DestinationDepartureTime
                        : GetPreviousTrip().ArrivalTime;

      int latestBusyMinute =
                !IsHalfTourFromOrigin
                    ? ArrivalTime
                    : Sequence == 1
                        ? Tour.DestinationArrivalTime
                        : GetPreviousTrip().ArrivalTime;

      timeWindow.SetBusyMinutes(earliestBusyMinute, latestBusyMinute + 1);

      if (!Global.Configuration.TraceModelResultValidity || PersonDay.HouseholdDay.AttemptedSimulations < Global.Configuration.InvalidAttemptsBeforeTrace) {
        return;
      }

      if (Tour.IsHomeBasedTour) {
        Global.PrintFile.WriteLine("  >> HUpdateTripValues SetBusyMinutes HH/P/PDay/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, PersonDay.Id, earliestBusyMinute, latestBusyMinute + 1);
      } else {
        Global.PrintFile.WriteLine("  >> HUpdateTripValues SetBusyMinutes HH/P/TOUR/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, Tour.ParentTour.Sequence, earliestBusyMinute, latestBusyMinute + 1);
      }
    }





    #endregion
  }
}