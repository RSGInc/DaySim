// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using DaySim.ChoiceModels.H;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.PathTypeModels;
using DaySim.Framework.Roster;
using DaySim.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class TripWrapper : Default.Wrappers.TripWrapper, IActumTripWrapper {
    private readonly IActumTrip _trip;

    [UsedImplicitly]
    public TripWrapper(Framework.DomainModels.Models.ITrip trip, Framework.DomainModels.Wrappers.ITourWrapper tourWrapper, Framework.DomainModels.Models.IHalfTour halfTour)
        //public TripWrapper(IActumTrip trip, IActumTourWrapper tourWrapper, IHalfTour halfTour)
        : base(trip, tourWrapper, halfTour) {
      _trip = (IActumTrip)trip;
    }

    #region relations properties

    //public IActumHouseholdWrapper Household { get; set; }

    //public IActumPersonWrapper Person { get; set; }

    //public IActumPersonDayWrapper PersonDay { get; set; }

    private new IActumTourWrapper Tour {
      get => (IActumTourWrapper)base.Tour;
      set => base.Tour = value;
    }

    //public IActumParcelWrapper OriginParcel { get; set; }

    //IParcel ISamplingTrip.OriginParcel {
    //	get { return OriginParcel; }
    //}

    //public IActumParcelWrapper DestinationParcel { get; set; }

    #endregion

    #region domain model properies
    //JLB20160323
    public int AutoType {
      get => _trip.AutoType;
      set => _trip.AutoType = value;
    }

    public int AutoOccupancy {
      get => _trip.AutoOccupancy;
      set => _trip.AutoOccupancy = value;
    }

    public int TourMode {
      get => _trip.TourMode;
      set => _trip.TourMode = value;
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

    public int AccessTerminalID {
      get => _trip.AccessTerminalID;
      set => _trip.AccessTerminalID = value;
    }

    public int AccessTerminalParcelID {
      get => _trip.AccessTerminalParcelID;
      set => _trip.AccessTerminalParcelID = value;
    }

    public int AccessTerminalZoneID {
      get => _trip.AccessTerminalZoneID;
      set => _trip.AccessTerminalZoneID = value;
    }

    public int AccessParkingNodeID {
      get => _trip.AccessParkingNodeID;
      set => _trip.AccessParkingNodeID = value;
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

    public int EgressTerminalID {
      get => _trip.EgressTerminalID;
      set => _trip.EgressTerminalID = value;
    }

    public int EgressTerminalParcelID {
      get => _trip.EgressTerminalParcelID;
      set => _trip.EgressTerminalParcelID = value;
    }

    public int EgressTerminalZoneID {
      get => _trip.EgressTerminalZoneID;
      set => _trip.EgressTerminalZoneID = value;
    }

    public int EgressParkingNodeID {
      get => _trip.EgressParkingNodeID;
      set => _trip.EgressParkingNodeID = value;
    }


    #endregion

    #region wrapper methods

    public override void SetDriverOrPassenger(List<Framework.DomainModels.Wrappers.ITripWrapper> trips) {
      if (Mode == Global.Settings.Modes.PaidRideShare) {
        //set main and other passenger randomly by tour purpose to get right percentage of trips to assign to network
        //may want to change these to 
        double randomNumber = Household.RandomUtility.Uniform01();
        DriverType =
             (Tour.DestinationPurpose == Global.Settings.Purposes.Work && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_Work
             || Tour.DestinationPurpose == Global.Settings.Purposes.Business && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_Business
             || Tour.DestinationPurpose == Global.Settings.Purposes.School && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_School
             || Tour.DestinationPurpose == Global.Settings.Purposes.Escort && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_Escort
             || Tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_PersonalBusiness
             || Tour.DestinationPurpose == Global.Settings.Purposes.Shopping && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_Shopping
             || Tour.DestinationPurpose == Global.Settings.Purposes.Social && randomNumber < Global.Configuration.COMPASS_PercentSharedCarTripsByMainPassenger_Social) ?
             Global.Settings.DriverTypes.Driver : Global.Settings.DriverTypes.Passenger;
        //if (Global.Configuration.AV_PaidRideShareModeUsesAVs) {
        //  DriverType = DriverType + 2; //two types of AV passengers so we know which trips to assign to network
       // } MB: no longer needed because autotype is on the trip file.
      } else if (Mode == Global.Settings.Modes.Walk || Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Transit || Mode == Global.Settings.Modes.SchoolBus || Mode == Global.Settings.Modes.Other) {
        DriverType = Global.Settings.DriverTypes.NotApplicable;
      } else if (Mode == Global.Settings.Modes.Sov || Mode == Global.Settings.Modes.HovDriver) {
        DriverType = Global.Settings.DriverTypes.Driver;
      } else if (Mode == Global.Settings.Modes.HovPassenger) {
        DriverType = Global.Settings.DriverTypes.Passenger;
      }
      //if (Mode >= Global.Settings.Modes.Sov && Mode <= Global.Settings.Modes.HovPassenger && Global.Configuration.AV_IncludeAutoTypeChoice && Tour.Household.OwnsAutomatedVehicles > 0) {
      //  DriverType = DriverType + 2; //two types of AV passengers so we know which trips to assign to network
      //} MB: no longer needed because autotype is on the trip file.
    }

    public override void SetTripValueOfTime() {

      if (!Global.Configuration.IsInEstimationMode) {
        int purpose = Tour.DestinationPurpose;
        double timeCoefficient = Tour.TimeCoefficient;
        double costCoefficient = Tour.CostCoefficient;

        double costFraction = 1.0;
        if (Mode == Global.Settings.Modes.HovDriver || Mode == Global.Settings.Modes.HovPassenger) {
          costFraction = purpose == Global.Settings.Purposes.Work ?
            (AutoOccupancy == 2 ? Global.Configuration.COMPASS_HOVCostShare2Occupants_Commute
           : AutoOccupancy == 3 ? Global.Configuration.COMPASS_HOVCostShare3Occupants_Commute
           : AutoOccupancy == 4 ? Global.Configuration.COMPASS_HOVCostShare4Occupants_Commute
           : Global.Configuration.COMPASS_HOVCostShare5PlusOccupants_Commute)
         : purpose == Global.Settings.Purposes.Business ?
            (AutoOccupancy == 2 ? Global.Configuration.COMPASS_HOVCostShare2Occupants_Business
           : AutoOccupancy == 3 ? Global.Configuration.COMPASS_HOVCostShare3Occupants_Business
           : AutoOccupancy == 4 ? Global.Configuration.COMPASS_HOVCostShare4Occupants_Business
           : Global.Configuration.COMPASS_HOVCostShare5PlusOccupants_Business)
         :
            (AutoOccupancy == 2 ? Global.Configuration.COMPASS_HOVCostShare2Occupants_Leisure
           : AutoOccupancy == 3 ? Global.Configuration.COMPASS_HOVCostShare3Occupants_Leisure
           : AutoOccupancy == 4 ? Global.Configuration.COMPASS_HOVCostShare4Occupants_Leisure
           : Global.Configuration.COMPASS_HOVCostShare5PlusOccupants_Leisure);
        }

        double mzDistance = ImpedanceRoster.GetValue("distance-mz", Global.Settings.Modes.Walk, Global.Settings.PathTypes.FullNetwork, 60, DepartureTime, OriginParcel, DestinationParcel).Variable;

        double baseDistance = (purpose == Global.Settings.Purposes.Work) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Work
                             : (purpose == Global.Settings.Purposes.School) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Education
                             : (purpose == Global.Settings.Purposes.Business) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Business
                             : (purpose == Global.Settings.Purposes.Shopping) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Shop
                             : Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_HBOther;

        double distanceMultiple =
               Math.Min(Math.Max(mzDistance / baseDistance, Global.Configuration.COMPASS_CostCoefficientDistanceMultipleMinimum), Global.Configuration.COMPASS_CostCoefficientDistanceMultipleMaximum); // ranges for extreme values

        double distanceElasticity = (purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.School) ? Global.Configuration.COMPASS_CostCoefficientDistanceElasticity_Commute
                                : (purpose == Global.Settings.Purposes.Business) ? Global.Configuration.COMPASS_CostCoefficientDistanceElasticity_Business
                                : Global.Configuration.COMPASS_CostCoefficientDistanceElasticity_Leisure;

        double distanceFactor = Math.Pow(distanceMultiple, distanceElasticity);


        double AVFactor = ((Global.Configuration.AV_IncludeAutoTypeChoice && Household.OwnsAutomatedVehicles > 0 && Mode >= Global.Settings.Modes.Sov && Mode <= Global.Settings.Modes.Hov3)
                               || (Global.Configuration.AV_PaidRideShareModeUsesAVs && Mode == Global.Settings.Modes.PaidRideShare))
                               ? (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : 1.0;

        ValueOfTime = (timeCoefficient * 60 * AVFactor) / (costCoefficient * costFraction * distanceFactor);
      }
    }

    public override void SetDestinationParkingStay(int min1, int min2) {
      if (!Global.DestinationParkingNodeIsEnabled || !Global.Configuration.ShouldUseDestinationParkingShadowPricing || Global.Configuration.IsInEstimationMode) {
        return;
      }

      int arrivalIndex = Math.Min(Math.Max(0, Math.Min(min1, min2) - 1), Global.Settings.Times.MinutesInADay - 1);
      int departureIndex = Math.Min(Math.Max(0, Math.Max(min1, min2) - 1), Global.Settings.Times.MinutesInADay - 1);

      double[] parkingLoad =
                ChoiceModelFactory
                    .DestinationParkingNodeDao
                    .Get(EgressParkingNodeID)
                    .ParkingLoad;

      for (int minute = arrivalIndex; minute < departureIndex; minute++) {
        parkingLoad[minute] += Household.ExpansionFactor;
      }
    }



    public override void RunDestinationParkingLocationModel() {
      PersonDay.ResetRandom(40 * (2 * Tour.Sequence - 1 + Direction - 1) + 60 + Sequence - 1);

      int destPurpose = (IsHalfTourFromOrigin) ? OriginPurpose : DestinationPurpose;
      IActumParcelWrapper destParcel = (IsHalfTourFromOrigin) ? (IActumParcelWrapper)OriginParcel : (IActumParcelWrapper)DestinationParcel;
      bool destIsUsualWorkplace = ((destPurpose == Global.Settings.Purposes.Work || destPurpose == Global.Settings.Purposes.Business) && Tour.Person.UsualWorkParcel != null && destParcel == Tour.Person.UsualWorkParcel);

      if ((Mode != Global.Settings.Modes.Sov && Mode != Global.Settings.Modes.HovDriver)
        || destPurpose == Global.Settings.Purposes.NoneOrHome
        || (destIsUsualWorkplace && Tour.Person.PaidParkingAtWorkplace == 0)
        || destParcel.ParkingDataAvailable == 0) {
        return;
      }
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)Household;
      IActumParcelWrapper origParcel = (IsHalfTourFromOrigin) ? (IActumParcelWrapper)DestinationParcel : (IActumParcelWrapper)OriginParcel;
      int destArriveTime = (IsHalfTourFromOrigin) ? DepartureTime : ArrivalTime;
      int destDepartTime = ActivityEndTime;
      bool span3AM = (destDepartTime < destArriveTime);
      int minParkDuration = Math.Max(10, destDepartTime - destArriveTime + span3AM.ToFlag() * Global.Settings.Times.MinutesInADay);

      /*
      List<DestinationParkingNodeWrapper> destinationParkingNodes = new List<DestinationParkingNodeWrapper>();
      foreach (DestinationParkingNodeWrapper node in ChoiceModelFactory.DestinationParkingNodeDao.Nodes) {
        if (node.Capacity > Constants.EPSILON   // has to have positive capacity
             && node.ParkingType != 3 && node.ParkingType != 6  // resident-only and bike spaces not considered
             && !(node.ParkingType == 4 && !destIsUsualWorkplace)  // employee-only spaces only if it is the usual workplace microzone
             && !(node.ParkingType == 5 && !(household.AutoType > 1)) // eclectric veh-only spaces only if the HH has electric vehicles
             && (node.MaxDuration == 24 || (node.MaxDuration * 60.0) >= minParkDuration)  // check minimum duration
             && (node.OpeningTime ==  0 || (node.OpeningTime * 60.0) <= (destArriveTime + 180))   // check opening time
             && (node.ClosingTime == 24 || (node.ClosingTime * 60.0) >= (destDepartTime + 180))   // check closing time
          ) {
          destinationParkingNodes.Add(node);
        }
      }
      */

      int firstIndex = destParcel.FirstPositionInDestinationParkingLocationDistanceArray;
      int lastIndex = destParcel.LastPositionInDestinationParkingLocationDistanceArray;
      if (firstIndex <= 0 || lastIndex <= 0) {
        return;
      }

      double maxUtil = -9999999;
      int bestLocationId = -1;
      int bestLocationParcelId = -1;
      int bestLocationZoneId = -1;
      double bestLocationWalkDistance = -1;
      double bestLocationWalkTime = -1;
      double bestLocationParkPrice = -1;
      int parkArriveTime = 0;
      int parkDepartTime = 0;

      for (int index = firstIndex; index <= lastIndex; index++) {

        int parkLocationId = Global.ParcelToDestinationParkingLocationIds[index];
        IDestinationParkingNodeWrapper node = ChoiceModelFactory.DestinationParkingNodeDao.Get(parkLocationId);

        if (node == null) {
          continue;
        }
        if (node.Capacity < Constants.EPSILON   // has to have positive capacity
             || (node.ParkingType == 3 || node.ParkingType == 6)  // resident-only and bike spaces not considered
             || (node.ParkingType == 4 && !destIsUsualWorkplace)  // employee-only spaces only if it is the usual workplace microzone
             || (node.ParkingType == 5 && (household.AutoType < 2)) // eclectric veh-only spaces only if the HH has electric vehicles
             || (node.MaxDuration != 24 && (node.MaxDuration * 60.0) < minParkDuration)  // check minimum duration
             || (node.OpeningTime != 0 && (node.OpeningTime * 60.0) > (destArriveTime + 180))   // check opening time
             || (node.ClosingTime != 24 && (node.ClosingTime * 60.0) < (destDepartTime + 180))) {  // check closing time 
          continue;
        }

        int parkParcelId = Global.ParcelToDestinationParkingLocationMicrozoneIds[index];
        ParcelWrapper parkParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[parkParcelId];

        SkimValue autoSkim = ImpedanceRoster.GetValue("lstime", Mode, Global.Settings.PathTypes.FullNetwork, 60, ArrivalTime, origParcel, parkParcel, Constants.DEFAULT_VALUE);
        double driveGenTime = autoSkim.Variable;

        double walkDistance = Global.ParcelToDestinationParkingLocationLength[index] / 1000.0; //convert to km
        double walkTime = walkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;
        parkArriveTime = Math.Max(1, destArriveTime - (int)walkTime);
        parkDepartTime = Math.Max(1, destDepartTime + (int)walkTime);

        // set utility

        double parkPrice = 0.0;
        if (node.ParkingType != 2) {  //skip price for free parking
          double paidArriveTime = Math.Min(parkArriveTime + 60.0 * node.FreeDuration, Global.Settings.Times.MinutesInADay) ;
          double hourPrice = 0
           + (!(parkDepartTime < Global.Settings.Times.ThreeAM || paidArriveTime >= Global.Settings.Times.FourAM)).ToFlag() * node.Price3AM
           + (!(parkDepartTime < Global.Settings.Times.FourAM || paidArriveTime >= Global.Settings.Times.FiveAM)).ToFlag() * node.Price4AM
           + (!(parkDepartTime < Global.Settings.Times.FiveAM || paidArriveTime >= Global.Settings.Times.SixAM)).ToFlag() * node.Price5AM
           + (!(parkDepartTime < Global.Settings.Times.SixAM || paidArriveTime >= Global.Settings.Times.SevenAM)).ToFlag() * node.Price6AM
           + (!(parkDepartTime < Global.Settings.Times.SevenAM || paidArriveTime >= Global.Settings.Times.EightAM)).ToFlag() * node.Price7AM
           + (!(parkDepartTime < Global.Settings.Times.EightAM || paidArriveTime >= Global.Settings.Times.NineAM)).ToFlag() * node.Price8AM
           + (!(parkDepartTime < Global.Settings.Times.NineAM || paidArriveTime >= Global.Settings.Times.TenAM)).ToFlag() * node.Price9AM
           + (!(parkDepartTime < Global.Settings.Times.TenAM || paidArriveTime >= Global.Settings.Times.ElevenAM)).ToFlag() * node.Price10AM
           + (!(parkDepartTime < Global.Settings.Times.ElevenAM || paidArriveTime >= Global.Settings.Times.Noon)).ToFlag() * node.Price11AM
           + (!(parkDepartTime < Global.Settings.Times.Noon || paidArriveTime >= Global.Settings.Times.OnePM)).ToFlag() * node.Price12PM
           + (!(parkDepartTime < Global.Settings.Times.OnePM || paidArriveTime >= Global.Settings.Times.TwoPM)).ToFlag() * node.Price1PM
           + (!(parkDepartTime < Global.Settings.Times.TwoPM || paidArriveTime >= Global.Settings.Times.ThreePM)).ToFlag() * node.Price2PM
           + (!(parkDepartTime < Global.Settings.Times.ThreePM || paidArriveTime >= Global.Settings.Times.FourPM)).ToFlag() * node.Price3PM
           + (!(parkDepartTime < Global.Settings.Times.FourPM || paidArriveTime >= Global.Settings.Times.FivePM)).ToFlag() * node.Price4PM
           + (!(parkDepartTime < Global.Settings.Times.FivePM || paidArriveTime >= Global.Settings.Times.SixPM)).ToFlag() * node.Price5PM
           + (!(parkDepartTime < Global.Settings.Times.SixPM || paidArriveTime >= Global.Settings.Times.SevenPM)).ToFlag() * node.Price6PM
           + (!(parkDepartTime < Global.Settings.Times.SevenPM || paidArriveTime >= Global.Settings.Times.EightPM)).ToFlag() * node.Price7PM
           + (!(parkDepartTime < Global.Settings.Times.EightPM || paidArriveTime >= Global.Settings.Times.NinePM)).ToFlag() * node.Price8PM
           + (!(parkDepartTime < Global.Settings.Times.NinePM || paidArriveTime >= Global.Settings.Times.TenPM)).ToFlag() * node.Price9PM
           + (!(parkDepartTime < Global.Settings.Times.TenPM || paidArriveTime >= Global.Settings.Times.ElevenPM)).ToFlag() * node.Price10PM
           + (!(parkDepartTime < Global.Settings.Times.ElevenPM || paidArriveTime >= Global.Settings.Times.Midnight)).ToFlag() * node.Price11PM
           + (!(parkDepartTime < Global.Settings.Times.Midnight || paidArriveTime >= Global.Settings.Times.OneAM)).ToFlag() * node.Price12AM
           + (!(parkDepartTime < Global.Settings.Times.OneAM || paidArriveTime >= Global.Settings.Times.TwoAM)).ToFlag() * node.Price1AM
           + (!(parkDepartTime < Global.Settings.Times.TwoAM || paidArriveTime >= Global.Settings.Times.MinutesInADay)).ToFlag() * node.Price2AM;

          parkPrice = Math.Min(Math.Max(hourPrice, node.MinimumPrice), node.FullDayPrice);

          if (node.MonthlyPassDayPrice > 0 && (destPurpose == Global.Settings.Purposes.Work || destPurpose == Global.Settings.Purposes.School)
             && node.MonthlyPassDayPrice < parkPrice) {
            parkPrice = node.MonthlyPassDayPrice;
          }
        }
        double randomNormalTerm = Household.RandomUtility.Normal(0, 1);

        double parkingTypeConstant = (node.LocationType == 2) ? Global.Configuration.COMPASS_DestinationParkingOffStreetGarageTypeConstant
                                   : (node.LocationType == 3) ? Global.Configuration.COMPASS_DestinationParkingOffStreetLotTypeConstant
                                   : (node.LocationType == 4) ? Global.Configuration.COMPASS_DestinationParkingOffStreetPrivateTypeConstant
                                   : 0.0;

        double searchTime = (Global.Configuration.ShouldUseDestinationParkingShadowPricing && !Global.Configuration.IsInEstimationMode) ? node.ShadowPrice[parkArriveTime] : 0.0;

        double timeCost = parkingTypeConstant + searchTime
                       + (parkPrice / Math.Min(Math.Max(ValueOfTime,5),500)) * 60.0 
                       + driveGenTime * Global.Configuration.COMPASS_DestinationParkingDriveAccessTimeCoefficient
                       + walkTime * Global.Configuration.COMPASS_DestinationParkingWalkEgressTimeCoefficient
                       + randomNormalTerm * Global.Configuration.COMPASS_DestinationParkingRandomTermCoefficient;



        double utility = Math.Log(node.Capacity) * Global.Configuration.COMPASS_DestinationParkingLogCapacityCoefficient
                      + timeCost * Global.Configuration.COMPASS_BaseTimeCoefficientPerMinute;

        if (utility > maxUtil) {
          maxUtil = utility;
          bestLocationId = node.Id;
          bestLocationParcelId = parkParcelId;
          bestLocationZoneId = (int) parkParcel.ZoneKey;
          bestLocationWalkDistance = walkDistance;
          bestLocationWalkTime = walkTime;
          bestLocationParkPrice = parkPrice;
        }
      }
      if (bestLocationId > 0) {
        EgressParkingNodeID = bestLocationId;
        EgressTerminalParcelID = bestLocationParcelId;
        EgressTerminalZoneID = bestLocationZoneId;
        EgressMode = Global.Settings.Modes.Walk;
        EgressDistance = bestLocationWalkDistance;
        EgressTime = bestLocationWalkTime;
        EgressCost = bestLocationParkPrice;
        SetDestinationParkingStay(parkArriveTime, parkDepartTime);
      };
    }

    public override void HUpdateTripValues() {
      //new version for household models - assumes that mode and departure time have been set

      //time windows also reset in estimation mode  - this just resets for one window
      ITimeWindow timeWindow = Tour.IsHomeBasedTour ? Tour.PersonDay.TimeWindow : Tour.ParentTour.TimeWindow;

      if (!Global.Configuration.IsInEstimationMode) {
        //some variables reset only in application mode
        HTripTime time = new HTripTime(DepartureTime);

        HTripTime.SetTimeImpedanceAndWindow(this, time);

        if (!time.Available) {
          if (!Global.Configuration.IsInEstimationMode) {
            PersonDay.IsValid = false;
          }

          return;
        }

        IPathTypeModel modeImpedance = time.ModeLOS;

        TravelTime = modeImpedance.PathTime;
        TravelCost = modeImpedance.PathCost;
        TravelDistance = modeImpedance.PathDistance;
        PathType = modeImpedance.PathType;

        TourMode = Tour.Mode;

        IActumHouseholdWrapper household = (IActumHouseholdWrapper) Household;

        AutoType = household.AutoType;

        AutoOccupancy =
          Mode == Global.Settings.Modes.HovDriver || Mode == Global.Settings.Modes.HovPassenger ? Tour.HovOccupancy :
          Mode == Global.Settings.Modes.PaidRideShare ? 2 :
          Mode == Global.Settings.Modes.Sov ? 1 : 0;

        if (Mode == Global.Settings.Modes.Transit) {
          if (Direction == 0) { //MB changed to use same for first half tour, to get the output correct
            AccessCost = time.DestinationAccessCost;
            AccessDistance = time.DestinationAccessDistance;
            AccessMode = time.DestinationAccessMode;
            AccessTerminalID = time.PathDestinationStopAreaKey;
            AccessTerminalParcelID = time.PathDestinationStopAreaParcelID;
            AccessTerminalZoneID = time.PathDestinationStopAreaZoneID;
            AccessParkingNodeID = time.PathDestinationParkingNodeKey;
            AccessTime = time.DestinationAccessTime;
            EgressCost = time.OriginAccessCost;
            EgressDistance = time.OriginAccessDistance;
            EgressMode = time.OriginAccessMode;
            EgressTerminalID = time.PathOriginStopAreaKey;
            EgressTerminalParcelID = time.PathOriginStopAreaParcelID;
            EgressTerminalZoneID = time.PathOriginStopAreaZoneID;
            EgressParkingNodeID = time.PathOriginParkingNodeKey;
            EgressTime = time.OriginAccessTime;

          } else {
            AccessCost = time.OriginAccessCost;
            AccessDistance = time.OriginAccessDistance;
            AccessMode = time.OriginAccessMode;
            AccessTerminalID = time.PathOriginStopAreaKey;
            AccessTerminalParcelID = time.PathOriginStopAreaParcelID;
            AccessTerminalZoneID = time.PathOriginStopAreaZoneID;
            AccessParkingNodeID = time.PathOriginParkingNodeKey;
            AccessTime = time.OriginAccessTime;
            EgressCost = time.DestinationAccessCost;
            EgressDistance = time.DestinationAccessDistance;
            EgressMode = time.DestinationAccessMode;
            EgressTerminalID = time.PathDestinationStopAreaKey;
            EgressTerminalParcelID = time.PathDestinationStopAreaParcelID;
            EgressTerminalZoneID = time.PathDestinationStopAreaZoneID;
            EgressParkingNodeID = time.PathDestinationParkingNodeKey;
            EgressTime = time.DestinationAccessTime;
          }
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


    public bool IsBusinessDestinationPurpose() {
      return DestinationPurpose == Global.Settings.Purposes.Business;
    }

    public bool IsBusinessOriginPurpose() {
      return OriginPurpose == Global.Settings.Purposes.Business;
    }


    public void HPTBikeDriveTransitTourUpdateTripValues() {
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
        } else {
          time.Available = true;
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

        TourMode = Tour.Mode;

        IActumHouseholdWrapper household = (IActumHouseholdWrapper)Household;

        AutoType = household.AutoType;


        Mode = Global.Settings.Modes.Transit;

        if (Direction == 1) {
          AccessCost = Tour.HalfTour1AccessCost;
          AccessDistance = Tour.HalfTour1AccessDistance;
          AccessMode = Tour.HalfTour1AccessMode;
          AccessPathType = Tour.HalfTour1AccessPathType;
          AccessTerminalID = Tour.HalfTour1AccessStopAreaKey;
          AccessTerminalParcelID = Tour.HalfTour1AccessStopAreaParcelID;
          AccessTerminalZoneID = Tour.HalfTour1AccessStopAreaZoneID;
          AccessParkingNodeID = Tour.HalfTour1AccessParkingNodeID;
          AccessTime = Tour.HalfTour1AccessTime;
          EgressCost = Tour.HalfTour1EgressCost;
          EgressDistance = Tour.HalfTour1EgressDistance;
          EgressMode = Tour.HalfTour1EgressMode;
          EgressPathType = Tour.HalfTour1EgressPathType;
          EgressTerminalID = Tour.HalfTour1EgressStopAreaKey;
          EgressTerminalParcelID = Tour.HalfTour1EgressStopAreaParcelID;
          EgressTerminalZoneID = Tour.HalfTour1EgressStopAreaZoneID;
          EgressParkingNodeID = Tour.HalfTour1EgressParkingNodeID;
          EgressTime = Tour.HalfTour1EgressTime;

        } else {
          AccessCost = Tour.HalfTour2AccessCost;
          AccessDistance = Tour.HalfTour2AccessDistance;
          AccessMode = Tour.HalfTour2AccessMode;
          AccessPathType = Tour.HalfTour2AccessPathType;
          AccessTerminalID = Tour.HalfTour2AccessStopAreaKey;
          AccessTerminalParcelID = Tour.HalfTour2AccessStopAreaParcelID;
          AccessTerminalZoneID = Tour.HalfTour2AccessStopAreaZoneID;
          AccessParkingNodeID = Tour.HalfTour2AccessParkingNodeID;
          AccessTime = Tour.HalfTour2AccessTime;
          EgressCost = Tour.HalfTour2EgressCost;
          EgressDistance = Tour.HalfTour2EgressDistance;
          EgressMode = Tour.HalfTour2EgressMode;
          EgressPathType = Tour.HalfTour2EgressPathType;
          EgressTerminalID = Tour.HalfTour2EgressStopAreaKey;
          EgressTerminalParcelID = Tour.HalfTour2EgressStopAreaParcelID;
          EgressTerminalZoneID = Tour.HalfTour2EgressStopAreaZoneID;
          EgressParkingNodeID = Tour.HalfTour2EgressParkingNodeID;
          EgressTime = Tour.HalfTour2EgressTime;
        }

        AutoOccupancy =
        Mode == Global.Settings.Modes.HovDriver     || AccessMode == Global.Settings.Modes.HovDriver     || EgressMode == Global.Settings.Modes.HovDriver ||
        Mode == Global.Settings.Modes.HovPassenger  || AccessMode == Global.Settings.Modes.HovPassenger  || EgressMode == Global.Settings.Modes.HovPassenger ? Math.Max(Tour.HovOccupancy, 2) :
        Mode == Global.Settings.Modes.PaidRideShare || AccessMode == Global.Settings.Modes.PaidRideShare || EgressMode == Global.Settings.Modes.PaidRideShare ? 2 :
        Mode == Global.Settings.Modes.Sov           || AccessMode == Global.Settings.Modes.Sov           || EgressMode == Global.Settings.Modes.Sov ? 1 : 0;

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
            Global.PrintFile.WriteLine("  >> HPTUpdateTripValues HH/P/T/Hf/T/Arrival time/valid {0} {1} {2} {3} {4} {5} {6}", Household.Id, Person.Sequence, Tour.Sequence, Direction, Sequence, ArrivalTime, PersonDay.IsValid);
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
        Global.PrintFile.WriteLine("  >> HPTUpdateTripValues SetBusyMinutes HH/P/PDay/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, PersonDay.Id, earliestBusyMinute, latestBusyMinute + 1);
      } else {
        Global.PrintFile.WriteLine("  >> HPTUpdateTripValues SetBusyMinutes HH/P/TOUR/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, Tour.ParentTour.Sequence, earliestBusyMinute, latestBusyMinute + 1);
      }
    }





    #endregion
  }
}
