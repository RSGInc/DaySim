﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.H {
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

    public int EarliestFeasibleDepatureTime;

    public int LatestFeasibleDepartureTime;

    public int PathOriginStopAreaKey { get; private set; }
    public int PathOriginStopAreaParcelID { get; private set; }
    public int PathOriginStopAreaZoneID { get; private set; }
    public int PathDestinationStopAreaKey { get; private set; }
    public int PathOriginParkingNodeKey { get; private set; }
    public int PathDestinationParkingNodeKey { get; private set; }
    public int PathDestinationStopAreaParcelID { get; private set; }
    public int PathDestinationStopAreaZoneID { get; private set; }
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

    private void FindPeriod(int departureTime) {
      foreach (MinuteSpan period in DayPeriod.HSmallDayPeriods.Where(period => departureTime.IsBetween(period.Start, period.End))) {
        DeparturePeriod = period;
      }

      foreach (
          HTripTime time in Times[ParallelUtility.threadLocalAssignedIndex.Value].Where(time => time.DeparturePeriod == DeparturePeriod)) {
        Index = time.Index;

        break;
      }
    }

    public int GetRandomFeasibleMinute(ITripWrapper trip, HTripTime time) {
      if (trip == null || time == null) {
        throw new ArgumentNullException("trip time");
      }

      ITimeWindow timeWindow = trip.Tour.ParentTour == null ? trip.Tour.PersonDay.TimeWindow : trip.Tour.ParentTour.TimeWindow;
      int departureTime = timeWindow.GetAvailableMinute(trip.Household.RandomUtility, time.EarliestFeasibleDepatureTime, time.LatestFeasibleDepartureTime);

      //if (departureTime == Constants.DEFAULT_VALUE) {
      //    throw new InvalidDepartureTimeException();
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
        int alternativeIndex = 0;

        foreach (MinuteSpan minuteSpan in DayPeriod.HSmallDayPeriods) {
          HTripTime time = new HTripTime(alternativeIndex, minuteSpan);

          Times[i][alternativeIndex++] = time;
        }
      }
    }

    public static void SetTimeImpedances(ITripWrapper trip) {

      foreach (HTripTime time in Times[ParallelUtility.threadLocalAssignedIndex.Value]) {
        SetTimeImpedanceAndWindow(trip, time);
      }
    }

    public static void SetTimeImpedanceAndWindow(ITripWrapper trip, HTripTime time) {

      IActumHouseholdWrapper household = (IActumHouseholdWrapper)trip.Household;
      ITourWrapper tour = trip.Tour;
      int alternativeIndex = time.Index;
      MinuteSpan period = time.DeparturePeriod;

      // set mode LOS and mode availability
      if (period.End < trip.EarliestDepartureTime || period.Start > trip.LatestDepartureTime) {
        time.Available = false;
      } else {
        //int pathMode = (trip.Mode >= Global.Settings.Modes.SchoolBus - 1) ? Global.Settings.Modes.Hov3 : trip.Mode;
        int pathMode = (trip.Mode > Global.Settings.Modes.WalkRideWalk) ? Global.Settings.Modes.Hov3 : trip.Mode;  //JB 201905 proposed patch

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
                            tour.Person.Age,
                            tour.Household.VehiclesAvailable,
                            tour.Person.TransitPassOwnership,
                            tour.Household.OwnsAutomatedVehicles > 0,
                            tour.HovOccupancy,
                            household.AutoType,
                            tour.Person.PersonType,
                            true,
                            pathMode);

        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == pathMode);

        time.Available = pathTypeModel.Available;
        time.ModeLOS = pathTypeModel;

        if (time.Available) {
          time.PathOriginStopAreaKey = pathTypeModel.PathOriginStopAreaKey;
          time.PathOriginStopAreaParcelID = pathTypeModel.PathOriginStopAreaParcelID;
          time.PathOriginStopAreaZoneID = pathTypeModel.PathOriginStopAreaZoneID;
          time.PathDestinationStopAreaKey = pathTypeModel.PathDestinationStopAreaKey;
          time.PathDestinationStopAreaParcelID = pathTypeModel.PathDestinationStopAreaParcelID;
          time.PathDestinationStopAreaZoneID = pathTypeModel.PathDestinationStopAreaZoneID;
          time.PathOriginParkingNodeKey = pathTypeModel.PathParkAndRideNodeId;
          time.PathDestinationParkingNodeKey = pathTypeModel.PathParkAndRideEgressNodeId;
          time.OriginAccessMode = pathTypeModel.PathOriginAccessMode;
          time.OriginAccessTime = pathTypeModel.PathOriginAccessTime;
          time.OriginAccessDistance = pathTypeModel.PathOriginAccessDistance;
          time.OriginAccessCost = pathTypeModel.PathOriginAccessCost;
          time.DestinationAccessMode = pathTypeModel.PathDestinationAccessMode;
          time.DestinationAccessTime = pathTypeModel.PathDestinationAccessTime;
          time.DestinationAccessDistance = pathTypeModel.PathDestinationAccessDistance;
          time.DestinationAccessCost = pathTypeModel.PathDestinationAccessCost;
        }

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
