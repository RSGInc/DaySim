// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.ChoiceModels;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Exceptions;
using DaySim.Framework.Roster;


namespace DaySim.PathTypeModels {
  public class PathTypeModel_Actum : IPathTypeModel {
    private const double MAX_UTILITY = 80D;
    private const double MIN_UTILITY = -80D;

    private ParcelWrapper _originParcel;
    private ParcelWrapper _destinationParcel;
    private int _originZoneId;
    private int _destinationZoneId;
    private int _outboundTime;
    private int _returnTime;
    private int _purpose;
    private double _tourCostCoefficient;
    private double _tourTimeCoefficient;
    private int _personAge;
    private int _householdCars;
    private bool _carsAreAVs;
    private int _transitPassOwnership;
    private int _personType;
    private bool _randomChoice;
    private int _choice;

    // model variables
    private readonly double[] _utility = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _expUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathGenTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly int[] _pathParkAndRideNodeId = new int[Global.Settings.PathTypes.TotalPathTypes];
    private readonly int[] _pathOriginStopAreaKey = new int[Global.Settings.PathTypes.TotalPathTypes];
    private readonly int[] _pathDestinationStopAreaKey = new int[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathTransitTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathTransitDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathTransitCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathTransitUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathWalkTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathWalkDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathBikeTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathBikeDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathBikeCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly int[] _pathOriginAccessMode = new int[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathOriginAccessTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathOriginAccessDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathOriginAccessCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathOriginAccessUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly int[] _pathDestinationAccessMode = new int[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathDestinationAccessTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathDestinationAccessDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathDestinationAccessCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    private readonly double[] _pathDestinationAccessUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
    private static List<ParkAndRideNodeWrapper> _parkAndRideNodesWithCapacity = new List<ParkAndRideNodeWrapper>();
    private static List<ParkAndRideNodeWrapper> _parkAndRideAutoNodesWithCapacity = new List<ParkAndRideNodeWrapper>();
    private static List<ParkAndRideNodeWrapper> _parkAndRideBikeNodesWithCapacity = new List<ParkAndRideNodeWrapper>();

    static PathTypeModel_Actum() {
      foreach (ParkAndRideNodeWrapper node in ChoiceModelFactory.ParkAndRideNodeDao.Nodes) {
        if (node.Capacity >= Constants.EPSILON) {
          _parkAndRideNodesWithCapacity.Add(node);
          if (node.Auto == 1 && node.Capacity > 0) {
            _parkAndRideAutoNodesWithCapacity.Add(node);
          } else if (node.Auto == 0 && node.Capacity > 2) {  //a lot of nodes with capacity=2 not included in lists
            _parkAndRideBikeNodesWithCapacity.Add(node);
          }
        }
      }
    }

    public PathTypeModel_Actum() {
      GeneralizedTimeLogsum = Global.Settings.GeneralizedTimeUnavailable;
      GeneralizedTimeChosen = Global.Settings.GeneralizedTimeUnavailable;
    }

    public int Mode { get; set; }
    public double GeneralizedTimeLogsum { get; protected set; }
    public double GeneralizedTimeChosen { get; protected set; }
    public double PathTime { get; protected set; }
    public double PathDistance { get; protected set; }
    public double PathCost { get; protected set; }
    public int PathType { get; protected set; }
    public int PathParkAndRideNodeId { get; protected set; }
    public int PathOriginStopAreaKey { get; protected set; }
    public int PathDestinationStopAreaKey { get; protected set; }
    public double PathTransitTime { get; protected set; }
    public double PathTransitDistance { get; protected set; }
    public double PathTransitCost { get; protected set; }
    public double PathTransitGeneralizedTime { get; protected set; }

    public double PathWalkTime { get; protected set; }
    public double PathWalkDistance { get; protected set; }
    public double PathBikeTime { get; protected set; }
    public double PathBikeDistance { get; protected set; }
    public double PathBikeCost { get; protected set; }
    public int PathOriginAccessMode { get; protected set; }
    public double PathOriginAccessTime { get; protected set; }
    public double PathOriginAccessDistance { get; protected set; }
    public double PathOriginAccessCost { get; protected set; }
    public double PathOriginAccessUtility { get; protected set; }
    public int PathDestinationAccessMode { get; protected set; }
    public double PathDestinationAccessTime { get; protected set; }
    public double PathDestinationAccessDistance { get; protected set; }
    public double PathDestinationAccessCost { get; protected set; }
    public double PathDestinationAccessUtility { get; protected set; }

    public bool Available { get; protected set; }

    public int PathDestinationParkingNodeId => throw new NotImplementedException("Not Implemented in Actum");

    public int PathDestinationParkingType => throw new NotImplementedException("Not Implemented in Actum");

    public double PathDestinationParkingCost => throw new NotImplementedException("Not Implemented in Actum");

    public double PathDestinationParkingWalkTime => throw new NotImplementedException("Not Implemented in Actum");

    public double PathParkAndRideTransitTime => throw new NotImplementedException("Not Implemented in Actum");

    public double PathParkAndRideTransitDistance => throw new NotImplementedException("Not Implemented in Actum");

    public double PathParkAndRideTransitCost => throw new NotImplementedException("Not Implemented in Actum");

    public double PathParkAndRideTransitGeneralizedTime => throw new NotImplementedException("Not Implemented in Actum");

    public double PathParkAndRideWalkAccessEgressTime => throw new NotImplementedException("Not Implemented in Actum");

    public double PathTransitWalkAccessEgressTime => throw new NotImplementedException("Not Implemented in Actum");

    public List<IPathTypeModel> RunAllPlusParkAndRide(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, bool carsAreAVs, int personType, bool randomChoice) {
      //return (RunAllPlusParkAndRide(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, personAge, householdCars, /*transitPassOwnership*/ 0, carsAreAVs, personType, randomChoice));
      throw new NotImplementedException("This needs to pass in TransitPassOwnership");
    }

    public List<IPathTypeModel> RunAllPlusParkAndRide(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int personType, bool randomChoice) {
      //		public List<PathTypeModel_Actum> RunAllPlusParkAndRide(IRandomUtility randomUtility, ParcelWrapper originParcel, ParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int personType, bool randomChoice) {
      List<int> modes = new List<int>();

      for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.ParkAndRide; mode++) {
        modes.Add(mode);
      }

      return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, personAge, householdCars, transitPassOwnership, carsAreAVs, personType, randomChoice, modes.ToArray());
    }

    public List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, bool carsAreAVs, int personType, bool randomChoice) {
      //return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, personAge, householdCars,  /*transitPassOwnership*/ 0, carsAreAVs, personType, randomChoice);
      throw new NotImplementedException("This needs to pass in TransitPassOwnership");
    }

    public List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int personType, bool randomChoice) {
      //		public List<PathTypeModel_Actum> RunAll(IRandomUtility randomUtility, ParcelWrapper originParcel, ParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int personType, bool randomChoice) {
      List<int> modes = new List<int>();

      //for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.Transit; mode++) {
      //	modes.Add(mode);
      //}

      //JB 20190516 replaced the following lines because it stopped before adding Transit mode (==7) since PaidRideShare mode == 6
      //for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.PaidRideShare; mode++) {
      //if (mode <= Global.Settings.Modes.Transit
      //|| (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideShareModeIsAvailable)) {
      for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.Transit; mode++) {
        if ((mode < Global.Settings.Modes.PaidRideShare)
        || (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideShareModeIsAvailable)
        || (mode == Global.Settings.Modes.Transit)) {
          modes.Add(mode);
        }
      }


      return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, personAge, householdCars, transitPassOwnership, carsAreAVs, personType, randomChoice, modes.ToArray());
    }

    public List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int personType, bool randomChoice, params int[] modes) {
      //		public List<PathTypeModel_Actum> Run(IRandomUtility randomUtility, ParcelWrapper originParcel, ParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int personType, bool randomChoice, params int[] modes) {
      List<IPathTypeModel> list = new List<IPathTypeModel>();
      //			var list = new List<PathTypeModel_Actum>();

      foreach (int mode in modes) {
        PathTypeModel_Actum pathTypeModel = new PathTypeModel_Actum { _originParcel = (ParcelWrapper)originParcel, _destinationParcel = (ParcelWrapper)destinationParcel, _outboundTime = outboundTime, _returnTime = returnTime, _purpose = purpose, _tourCostCoefficient = tourCostCoefficient, _tourTimeCoefficient = tourTimeCoefficient, _personAge = personAge, _householdCars = householdCars, _transitPassOwnership = transitPassOwnership, _carsAreAVs = carsAreAVs, _personType = personType, _randomChoice = randomChoice, Mode = mode };
        pathTypeModel.RunModel(randomUtility);

        list.Add(pathTypeModel);
      }

      return list;
    }

    public List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int personType, bool randomChoice, params int[] modes) {
      List<IPathTypeModel> list = new List<IPathTypeModel>();

      foreach (PathTypeModel_Actum pathTypeModel in modes.Select(mode => new PathTypeModel_Actum { _originZoneId = originZoneId, _destinationZoneId = destinationZoneId, _outboundTime = outboundTime, _returnTime = returnTime, _purpose = purpose, _tourCostCoefficient = tourCostCoefficient, _tourTimeCoefficient = tourTimeCoefficient, _personAge = personAge, _householdCars = householdCars, _transitPassOwnership = transitPassOwnership, _carsAreAVs = carsAreAVs, _personType = personType, _randomChoice = randomChoice, Mode = mode })) {
        pathTypeModel.RunModel(randomUtility, true);

        list.Add(pathTypeModel);
      }

      return list;
    }

    public void RunModel(IRandomUtility randomUtility, bool useZones = false) {

      //adjust cost coefficient for the distance effect
      if (!useZones) {
        double mzDistance = ImpedanceRoster.GetValue("distance-mz", Global.Settings.Modes.Walk, Global.Settings.PathTypes.FullNetwork, 60, _outboundTime, _originParcel, _destinationParcel).Variable;


        double baseDistance = (_purpose == Global.Settings.Purposes.Work) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Work
                             : (_purpose == Global.Settings.Purposes.School) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Education
                             : (_purpose == Global.Settings.Purposes.Business) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Business
                             : (_purpose == Global.Settings.Purposes.Shopping) ? Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_Shop
                             : Global.Configuration.COMPASS_BaseCostCoefficientDistanceLevel_HBOther;

        double distanceMultiple =
               Math.Min(Math.Max(mzDistance / baseDistance, Global.Configuration.COMPASS_CostCoefficientDistanceMultipleMinimum), Global.Configuration.COMPASS_CostCoefficientDistanceMultipleMaximum); // ranges for extreme values

        double distanceElasticity = (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School) ? Global.Configuration.COMPASS_CostCoefficientDistanceElasticity_Commute
                                : (_purpose == Global.Settings.Purposes.Business) ? Global.Configuration.COMPASS_CostCoefficientDistanceElasticity_Business
                                : Global.Configuration.COMPASS_CostCoefficientDistanceElasticity_Leisure;

        _tourCostCoefficient = _tourCostCoefficient * Math.Pow(distanceMultiple, distanceElasticity);
      }
     
      double votValue = 60; // (60.0 * _tourTimeCoefficient) / _tourCostCoefficient; // not using skims by vot group

           int skimMode = (Mode == Global.Settings.Modes.BikeOnTransit) ? Global.Settings.Modes.BikeOnTransit
                : (Mode == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.HovPassenger
                : (Mode >= Global.Settings.Modes.Transit) ? Global.Settings.Modes.Transit
                : Mode;
      int availablePathTypes = 0;
      double expUtilitySum = 0D;
      double bestExpUtility = 0D;
      int bestPathType = Constants.DEFAULT_VALUE;

      // loop on all relevant path types for the mode
      for (int pathType = Global.Settings.PathTypes.FullNetwork; pathType < Global.Settings.PathTypes.TotalPathTypes; pathType++) {
        _expUtility[pathType] = 0D;

        if (!ImpedanceRoster.IsActualCombination(skimMode, pathType)) {
          continue;
        }
        int ridemode = Global.Settings.Modes.Transit;
        int bonbmode = Global.Settings.Modes.BikeOnTransit;
        int walkmode = Global.Settings.Modes.Walk;
        int bikemode = Global.Settings.Modes.Bike;
        int sharemode = Global.Settings.Modes.PaidRideShare;
        int sovmode = Global.Settings.Modes.Sov;
        int hovmode = Global.Settings.Modes.HovPassenger;

        // set path type utility and impedance, depending on the mode
        if (Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Walk) {
          RunWalkBikeModel(skimMode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.HovDriver
          || Mode == Global.Settings.Modes.HovPassenger
          || Mode == Global.Settings.Modes.PaidRideShare
          || (Mode == Global.Settings.Modes.Sov && _personAge >= Global.Configuration.COMPASS_MinimumAutoDrivingAge && _householdCars > 0)) {
          RunAutoModel(skimMode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.Transit) {
          if (useZones) { RunZonalWalkTransitModel(skimMode, pathType, votValue, useZones); } // for aggregate logsums
          else { RunStopAreaGeneralTransitModel(ridemode, walkmode, walkmode, pathType, votValue, useZones); }
        } else
        if (Mode == Global.Settings.Modes.WalkRideBike) {
          RunStopAreaGeneralTransitModel(ridemode, walkmode, bikemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.WalkRideShare) {
          RunStopAreaGeneralTransitModel(ridemode, walkmode, sharemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.BikeParkRideWalk) {
          RunStopAreaGeneralTransitModel(ridemode, bikemode, walkmode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.BikeParkRideBike) {
          RunStopAreaGeneralTransitModel(ridemode, bikemode, bikemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.BikeParkRideShare) {
          RunStopAreaGeneralTransitModel(ridemode, bikemode, sharemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.BikeOnTransit) {
          RunStopAreaGeneralTransitModel(bonbmode, bikemode, bikemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.ShareRideWalk) {
          RunStopAreaGeneralTransitModel(ridemode, sharemode, walkmode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.ShareRideBike) {
          RunStopAreaGeneralTransitModel(ridemode, sharemode, bikemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.ShareRideShare) {
          RunStopAreaGeneralTransitModel(ridemode, sharemode, sharemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.CarKissRideWalk) {
          RunStopAreaGeneralTransitModel(ridemode, hovmode, walkmode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.CarKissRideBike) {
          RunStopAreaGeneralTransitModel(ridemode, hovmode, bikemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.CarKissRideShare) {
          RunStopAreaGeneralTransitModel(ridemode, hovmode, sharemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.CarParkRideWalk) {
          RunStopAreaGeneralTransitModel(ridemode, sovmode, walkmode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.CarParkRideBike) {
          RunStopAreaGeneralTransitModel(ridemode, sovmode, bikemode, pathType, votValue, useZones);
        } else
        if (Mode == Global.Settings.Modes.CarParkRideShare) {
          RunStopAreaGeneralTransitModel(ridemode, sovmode, sharemode, pathType, votValue, useZones);
        }


        if (_expUtility[pathType] < Constants.EPSILON) {
          continue;
        }

        // add to total utility and see if it is the best so far
        availablePathTypes++;
        expUtilitySum += _expUtility[pathType];

        if (_expUtility[pathType] <= bestExpUtility) {
          continue;
        }

        // make the best current path type and utility
        bestPathType = pathType;
        bestExpUtility = _expUtility[pathType];
      }

      if (expUtilitySum < Constants.EPSILON) {
        Available = false;
        if (Mode > Global.Settings.Modes.WalkRideWalk) {
          //Global.PrintFile.WriteLine("No available path for mode {0} from mz {1} to mz {2} at times {3} and {4}",
          //  Mode, _originParcel.Id, _destinationParcel.Id, _outboundTime, _returnTime);
        }
        return;
      }

      // set the generalized time logsum
      double logsum = Math.Log(expUtilitySum);
      double tourTimeCoefficient = (Global.Configuration.PathImpedance_PathChoiceScaleFactor * _tourTimeCoefficient);

      if (double.IsNaN(expUtilitySum) || double.IsNaN(logsum) || double.IsNaN(tourTimeCoefficient)) {
        throw new ValueIsNaNException(string.Format("Value is NaN for utilitySum: {0}, logsum: {1}, tourTimeCoefficient: {2}.", expUtilitySum, logsum, tourTimeCoefficient));
      }

      GeneralizedTimeLogsum = logsum / tourTimeCoefficient; // need to make sure _tourTimeCoefficient is not 0

      if (double.IsNaN(GeneralizedTimeLogsum)) {
        throw new ValueIsNaNException(string.Format("Value is NaN for GeneralizedTimeLogsum where utilitySum: {0}, logsum: {1}, tourTimeCoefficient: {2}.", expUtilitySum, logsum, tourTimeCoefficient));
      }

      // draw a choice using a random number if requested (and in application mode), otherwise return best utility
      if (_randomChoice && availablePathTypes > 1 && !Global.Configuration.IsInEstimationMode) {
        double random = randomUtility.Uniform01();

        for (int pathType = Global.Settings.PathTypes.FullNetwork; pathType <= Global.Settings.PathTypes.TotalPathTypes; pathType++) {
          _choice = pathType;
          random -= _expUtility[pathType] / expUtilitySum;

          if (random < 0) {
            break;
          }
        }
      } else {
        _choice = bestPathType;
      }

      Available = true;
      PathType = _choice;
      PathTime = _pathTime[_choice];
      PathDistance = _pathDistance[_choice];
      PathCost = _pathCost[_choice];
      GeneralizedTimeChosen = _utility[_choice] / tourTimeCoefficient;
      PathParkAndRideNodeId = _pathParkAndRideNodeId[_choice];
      PathOriginStopAreaKey = _pathOriginStopAreaKey[_choice];
      PathDestinationStopAreaKey = _pathDestinationStopAreaKey[_choice];
      PathTransitTime = _pathTransitTime[_choice];
      PathTransitDistance = _pathTransitDistance[_choice];
      PathTransitCost = _pathTransitCost[_choice];
      PathTransitGeneralizedTime = _pathTransitUtility[_choice] / tourTimeCoefficient;
      PathWalkTime = _pathWalkTime[_choice];
      PathWalkDistance = _pathWalkDistance[_choice];
      PathBikeTime = _pathBikeTime[_choice];
      PathBikeDistance = _pathBikeDistance[_choice];
      PathBikeCost = _pathBikeCost[_choice];
      PathOriginAccessMode = _pathOriginAccessMode[_choice];
      PathOriginAccessTime = _pathOriginAccessTime[_choice];
      PathOriginAccessDistance = _pathOriginAccessDistance[_choice];
      PathOriginAccessCost = _pathOriginAccessCost[_choice];
      PathOriginAccessUtility = _pathOriginAccessUtility[_choice];
      PathDestinationAccessMode = _pathDestinationAccessMode[_choice];
      PathDestinationAccessTime = _pathDestinationAccessTime[_choice];
      PathDestinationAccessDistance = _pathDestinationAccessDistance[_choice];
      PathDestinationAccessCost = _pathDestinationAccessCost[_choice];
      PathDestinationAccessUtility = _pathDestinationAccessUtility[_choice];
    }


    private void RunWalkBikeModel(int skimMode, int pathType, double votValue, bool useZones) {


      WalkBikePath path = GetWalkBikePath(skimMode, pathType, votValue, useZones, _outboundTime, _returnTime, _originZoneId, _destinationZoneId, _originParcel, _destinationParcel);

      if (!path.Available) {
        return;
      }

      //set pathType properties
      _pathTime[pathType] = path.Time;
      _pathDistance[pathType] = path.Distance;
      _pathCost[pathType] = 0;
      _utility[pathType] = path.Utility;
      _pathParkAndRideNodeId[pathType] = 0;

      _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);

      _pathOriginStopAreaKey[pathType] = 0;
      _pathDestinationStopAreaKey[pathType] = 0;
      _pathTransitTime[pathType] = 0;
      _pathTransitDistance[pathType] = 0;
      _pathTransitCost[pathType] = 0;
      _pathTransitUtility[pathType] = MIN_UTILITY;
      _pathBikeCost[pathType] = 0;
      if (skimMode == Global.Settings.Modes.Bike) {
        _pathWalkTime[pathType] = 0;
        _pathWalkDistance[pathType] = 0;
        _pathBikeTime[pathType] = _pathTime[pathType];
        _pathBikeDistance[pathType] = _pathDistance[pathType];
      } else {
        _pathWalkTime[pathType] = _pathTime[pathType];
        _pathWalkDistance[pathType] = _pathDistance[pathType];
        _pathBikeTime[pathType] = 0;
        _pathBikeDistance[pathType] = 0;
      }
      _pathOriginAccessMode[pathType] = Global.Settings.Modes.None;
      _pathOriginAccessTime[pathType] = 0.0;
      _pathOriginAccessDistance[pathType] = 0.0;
      _pathOriginAccessCost[pathType] = 0.0;
      _pathOriginAccessUtility[pathType] = 0.0;
      _pathDestinationAccessMode[pathType] = Global.Settings.Modes.None;
      _pathDestinationAccessTime[pathType] = 0.0;
      _pathDestinationAccessDistance[pathType] = 0.0;
      _pathDestinationAccessCost[pathType] = 0.0;
      _pathDestinationAccessUtility[pathType] = 0.0;
    }

    private void RunAutoModel(int skimModeIn, int pathType, double votValue, bool useZones) {
      bool useAVVOT = ((skimModeIn != Global.Settings.Modes.PaidRideShare && _carsAreAVs && Global.Configuration.AV_IncludeAutoTypeChoice)
                          || (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs));


      AutoPath path = GetAutoPath(skimModeIn, pathType, votValue, useZones, useAVVOT, _outboundTime, _returnTime, _originZoneId, _destinationZoneId, _originParcel, _destinationParcel);

      if (!path.Available) {
        return;
      }

      //calculate parking cost utility   JLB 201508
      double parkingCost = 0.0;
      if ((skimModeIn == Global.Settings.Modes.HovPassenger && !Global.Configuration.HOVPassengersIncurCosts) || skimModeIn == Global.Settings.Modes.PaidRideShare) {
      } else {
        if (!useZones) {
          parkingCost = _destinationParcel.PublicParkingHourlyPriceBuffer1;
          int parkingDuration = 1; // assume 1 hour if return time isn't known
          if (_returnTime > 0) {
            parkingDuration = (_returnTime - _outboundTime) / 60;
          }
          parkingCost = parkingCost * parkingDuration;  //in monetary units
        }
      }

      double costFraction = 1.0;
      if (skimModeIn == Global.Settings.Modes.HovDriver || skimModeIn == Global.Settings.Modes.HovPassenger) {
        int hovOccupancy = 2; // will pass in later
        costFraction = _purpose == Global.Settings.Purposes.Work ?
          (hovOccupancy == 2 ? Global.Configuration.COMPASS_HOVCostShare2Occupants_Commute
          : hovOccupancy == 3 ? Global.Configuration.COMPASS_HOVCostShare3Occupants_Commute
         : hovOccupancy == 4 ? Global.Configuration.COMPASS_HOVCostShare4Occupants_Commute
         : Global.Configuration.COMPASS_HOVCostShare5PlusOccupants_Commute)
        : _purpose == Global.Settings.Purposes.Business ?
          (hovOccupancy == 2 ? Global.Configuration.COMPASS_HOVCostShare2Occupants_Business
          : hovOccupancy == 3 ? Global.Configuration.COMPASS_HOVCostShare3Occupants_Business
         : hovOccupancy == 4 ? Global.Configuration.COMPASS_HOVCostShare4Occupants_Business
         : Global.Configuration.COMPASS_HOVCostShare5PlusOccupants_Business)
        :
          (hovOccupancy == 2 ? Global.Configuration.COMPASS_HOVCostShare2Occupants_Leisure
          : hovOccupancy == 3 ? Global.Configuration.COMPASS_HOVCostShare3Occupants_Leisure
         : hovOccupancy == 4 ? Global.Configuration.COMPASS_HOVCostShare4Occupants_Leisure
         : Global.Configuration.COMPASS_HOVCostShare5PlusOccupants_Leisure);
      }


      //set pathType properties
      _pathTime[pathType] = path.Time;
      _pathDistance[pathType] = path.Distance;
      _pathCost[pathType] = path.Cost + parkingCost;
      _utility[pathType] = path.Utility
                         + parkingCost * costFraction * _tourCostCoefficient;

      _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);

      _pathParkAndRideNodeId[pathType] = 0;
      _pathOriginStopAreaKey[pathType] = 0;
      _pathDestinationStopAreaKey[pathType] = 0;
      _pathTransitTime[pathType] = 0;
      _pathTransitDistance[pathType] = 0;
      _pathTransitCost[pathType] = 0;
      _pathTransitUtility[pathType] = MIN_UTILITY;
      _pathBikeCost[pathType] = 0;
      _pathWalkTime[pathType] = 0;
      _pathWalkDistance[pathType] = 0;
      _pathBikeTime[pathType] = 0;
      _pathBikeDistance[pathType] = 0;
      _pathOriginAccessMode[pathType] = Global.Settings.Modes.None;
      _pathOriginAccessTime[pathType] = 0.0;
      _pathOriginAccessDistance[pathType] = 0.0;
      _pathOriginAccessCost[pathType] = 0.0;
      _pathOriginAccessUtility[pathType] = 0.0;
      _pathDestinationAccessMode[pathType] = Global.Settings.Modes.None;
      _pathDestinationAccessTime[pathType] = 0.0;
      _pathDestinationAccessDistance[pathType] = 0.0;
      _pathDestinationAccessCost[pathType] = 0.0;
      _pathDestinationAccessUtility[pathType] = 0.0;
    }

    protected void RunZonalWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {
      // for COMPASS, just used for aggregate logsums, so simplified

      if (!useZones) {
        // get zones associated with parcels for transit path
        _originZoneId = _originParcel.ZoneId;
        _destinationZoneId = _destinationParcel.ZoneId;
      }

      double originWalkTime = 5.0;
      double destinationWalkTime = 5.0;

      if (_returnTime > 0) {
        originWalkTime *= 2;
        destinationWalkTime *= 2;
      }
      double walkTime = originWalkTime + destinationWalkTime;

      string varname = "time";
      SkimValue skimValue =
              ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId);
      double pathTime = skimValue.Variable;

      if (_returnTime > 0) {
        skimValue =
              ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, _returnTime, _originZoneId, _destinationZoneId);
        pathTime += skimValue.Variable;
      }

      varname = "gentime";
      skimValue =
           ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId);
      double genTime = skimValue.Variable;

      if (_returnTime > 0) {
        skimValue =
           ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, _returnTime, _originZoneId, _destinationZoneId);
        genTime += skimValue.Variable;
      }

      double fareCost = 0;
      varname = "farezones";
      skimValue =
             ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId);
      int fareZones = (int)Math.Round(skimValue.Variable);
      if (fareZones > _transitPassOwnership && _personAge > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel) {
        double fare = Global.TransitBaseFare_Adult[fareZones];
        if (_personAge <= Global.Configuration.COMPASS_TransitFareMaximumAgeForChildDiscount) {
          fare = fare * (1 - Global.TransitBaseFare_ChildDiscount[fareZones] / 100.0);
        }
        double outboundTime2 = _outboundTime - pathTime;
        if ((_outboundTime + 180 < Global.Configuration.COMPASS_TransitFareAMPeakPeriodStartMinute ||
          (_outboundTime + 180 >= Global.Configuration.COMPASS_TransitFareAMPeakPeriodEndMinute - 180 && _outboundTime + 180 < Global.Configuration.COMPASS_TransitFarePMPeakPeriodStartMinute)
          || _outboundTime + 180 >= Global.Configuration.COMPASS_TransitFarePMPeakPeriodEndMinute) &&
          (outboundTime2 + 180 < Global.Configuration.COMPASS_TransitFareAMPeakPeriodStartMinute ||
          (outboundTime2 + 180 >= Global.Configuration.COMPASS_TransitFareAMPeakPeriodEndMinute - 180 && outboundTime2 + 180 < Global.Configuration.COMPASS_TransitFarePMPeakPeriodStartMinute)
          || outboundTime2 + 180 >= Global.Configuration.COMPASS_TransitFarePMPeakPeriodEndMinute)) {
          fare = fare * (1 - Global.TransitBaseFare_OffPeakDiscount[fareZones] / 100.0);
        }
        fareCost = fare;
      }
      if (_returnTime > 0) {
        skimValue =
           ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId);
        fareZones = (int)Math.Round(skimValue.Variable);
        if (fareZones > _transitPassOwnership && _personAge > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel) {
          double fare = Global.TransitBaseFare_Adult[fareZones];
          if (_personAge <= Global.Configuration.COMPASS_TransitFareMaximumAgeForChildDiscount) {
            fare = fare * (1 - Global.TransitBaseFare_ChildDiscount[fareZones] / 100.0);
          }
          double returnTime2 = _returnTime + pathTime;
          if ((_returnTime + 180 < Global.Configuration.COMPASS_TransitFareAMPeakPeriodStartMinute ||
            (_returnTime + 180 >= Global.Configuration.COMPASS_TransitFareAMPeakPeriodEndMinute - 180 && _returnTime + 180 < Global.Configuration.COMPASS_TransitFarePMPeakPeriodStartMinute)
            || _returnTime + 180 >= Global.Configuration.COMPASS_TransitFarePMPeakPeriodEndMinute) &&
            (returnTime2 + 180 < Global.Configuration.COMPASS_TransitFareAMPeakPeriodStartMinute ||
            (returnTime2 + 180 >= Global.Configuration.COMPASS_TransitFareAMPeakPeriodEndMinute - 180 && returnTime2 + 180 < Global.Configuration.COMPASS_TransitFarePMPeakPeriodStartMinute)
            || returnTime2 + 180 >= Global.Configuration.COMPASS_TransitFarePMPeakPeriodEndMinute)) {
            fare = fare * (1 - Global.TransitBaseFare_OffPeakDiscount[fareZones] / 100.0);
          }
          fareCost += fare;
        }
      }

      _pathParkAndRideNodeId[pathType] = 0;

      // set utility

      double transitPathUtility = genTime * _tourTimeCoefficient
         + fareCost * _tourCostCoefficient;
      double fullPathUtility = transitPathUtility
         + walkTime * _tourTimeCoefficient * Global.Configuration.PathImpedance_WalkAccessTimeWeight;



      //set final values
      _pathDistance[pathType] = 0;
      _pathParkAndRideNodeId[pathType] = 0;
      _pathTime[pathType] = pathTime + walkTime;
      _pathCost[pathType] = fareCost;
      _utility[pathType] = fullPathUtility;
      _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);

    }


    private void RunStopAreaGeneralTransitModel(int skimMode, int accessMode, int egressMode, int pathType, double votValue, bool useZones) {

      if (useZones) {
        return;
      }

      List<ParkAndRideNodeWrapper> autoParkAndRideNodes = _parkAndRideAutoNodesWithCapacity;
      List<ParkAndRideNodeWrapper> bikeParkAndRideNodes = _parkAndRideBikeNodesWithCapacity;

      int pathTypeAccEgr = Global.Settings.PathTypes.FullNetwork;

      bool walkAccess = (accessMode == Global.Settings.Modes.Walk);
      bool bikeAccess = (accessMode == Global.Settings.Modes.Bike);
      bool shareAccess = (accessMode == Global.Settings.Modes.PaidRideShare);
      bool sovAccess = (accessMode == Global.Settings.Modes.Sov);
      bool hovAccess = (accessMode == Global.Settings.Modes.HovPassenger);
      bool walkEgress = (egressMode == Global.Settings.Modes.Walk);
      bool bikeEgress = (egressMode == Global.Settings.Modes.Bike);
      bool shareEgress = (egressMode == Global.Settings.Modes.PaidRideShare);
      bool bikeOnBoard = (skimMode == Global.Settings.Modes.BikeOnTransit);

      //skip share access or egress if not available
      if ((shareAccess || shareEgress) && !Global.Configuration.ShareModeIsAvailableForTransit) {
        return;
      }

      //user-set limits on search - use high values if not set
      int maxStopAreasToSearchAccess = walkAccess ? Global.Configuration.COMPASS_MaximumTerminalsToSearchWalk
                                     : bikeOnBoard ? Global.Configuration.COMPASS_MaximumTerminalsToSearchBikeOnTransit
                                     : bikeAccess ? Global.Configuration.COMPASS_MaximumParkingNodesToSearchBikeParkAndRide
                                     : sovAccess ? Global.Configuration.COMPASS_MaximumParkingNodesToSearchAutoParkAndRide
                                     : Global.Configuration.COMPASS_MaximumTerminalsToSearchAutoKissAndRide;
      if (maxStopAreasToSearchAccess < Constants.EPSILON) {
        maxStopAreasToSearchAccess = 50;
      }

      int maxStopAreasToSearchEgress = walkEgress ? Global.Configuration.COMPASS_MaximumTerminalsToSearchWalk
                                     : bikeOnBoard ? Global.Configuration.COMPASS_MaximumTerminalsToSearchBikeOnTransit
                                    : bikeEgress ? Global.Configuration.COMPASS_MaximumParkingNodesToSearchBikeParkAndRide
                                    : Global.Configuration.COMPASS_MaximumTerminalsToSearchAutoKissAndRide;
      if (maxStopAreasToSearchEgress < Constants.EPSILON) {
        maxStopAreasToSearchEgress = 50;
      }

      double maxStopAreaLengthAccess = walkAccess ? Global.Configuration.COMPASS_MaximumParcelToTerminalDistanceWalk
                                     : bikeAccess ? Global.Configuration.COMPASS_MaximumParcelToParkingNodeDistanceBike
                                     : sovAccess ? Global.Configuration.COMPASS_MaximumParcelToParkingNodeDistanceParkAndRide
                                     : Global.Configuration.COMPASS_MaximumParcelToTerminalDistanceKissAndRide;
      if (maxStopAreaLengthAccess < Constants.EPSILON) {
        maxStopAreaLengthAccess = 999.99;
      }

      double maxStopAreaLengthEgress = walkEgress ? Global.Configuration.COMPASS_MaximumParcelToTerminalDistanceWalk
                                     : bikeEgress ? Global.Configuration.COMPASS_MaximumParcelToParkingNodeDistanceBike
                                     : Global.Configuration.COMPASS_MaximumParcelToTerminalDistanceKissAndRide;
      if (maxStopAreaLengthEgress < Constants.EPSILON) {
        maxStopAreaLengthEgress = 999.99;
      }

      ParcelWrapper originParcelUsed = _originParcel;
      ParcelWrapper destinationParcelUsed = _destinationParcel;

      double mzDist = ImpedanceRoster.GetValue("distance-mz", Global.Settings.Modes.Walk, Global.Settings.PathTypes.FullNetwork, 60, _outboundTime, originParcelUsed, destinationParcelUsed).Variable / 1000.0; //in km
      double fullODDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.HovPassenger, Global.Settings.PathTypes.FullNetwork, 60, _outboundTime, originParcelUsed.ZoneId, destinationParcelUsed.ZoneId).Variable;


      //Global.PrintFile.WriteLine("* OD distance from mz {0} to mz {1} : zz-based {2}, mz-based {3}",   originParcelUsed.Id, destinationParcelUsed.Id, fullODDist, mzDist);

      double maxRatioLengthAccess = (sovAccess && Global.Configuration.COMPASS_MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0)
                              ? (fullODDist * Global.Configuration.COMPASS_MaximumRatioDriveToParkAndRideVersusDriveToDestination)
                              : (bikeAccess && Global.Configuration.COMPASS_MaximumRatioBikeToParkAndRideVersusDriveToDestination > 0)
                              ? (fullODDist * Global.Configuration.COMPASS_MaximumRatioBikeToParkAndRideVersusDriveToDestination)
                              : ((hovAccess || shareAccess) && Global.Configuration.COMPASS_MaximumRatioDriveToKissAndRideVersusDriveToDestination > 0)
                              ? (fullODDist * Global.Configuration.COMPASS_MaximumRatioDriveToKissAndRideVersusDriveToDestination)
                              : 999.99;

      maxStopAreaLengthAccess = Math.Min(maxStopAreaLengthAccess, maxRatioLengthAccess);

      double maxRatioLengthEgress = (bikeEgress && Global.Configuration.COMPASS_MaximumRatioBikeToParkAndRideVersusDriveToDestination > 0)
                              ? (fullODDist * Global.Configuration.COMPASS_MaximumRatioBikeToParkAndRideVersusDriveToDestination)
                              : (shareEgress && Global.Configuration.COMPASS_MaximumRatioDriveToKissAndRideVersusDriveToDestination > 0)
                              ? (fullODDist * Global.Configuration.COMPASS_MaximumRatioDriveToKissAndRideVersusDriveToDestination)
                              : 999.99;

      maxStopAreaLengthEgress = Math.Min(maxStopAreaLengthEgress, maxRatioLengthEgress);


      int firstIndexAccess = walkAccess ? originParcelUsed.FirstPositionInStopAreaDistanceArray
                      : bikeOnBoard ? originParcelUsed.FirstPositionInBikeOnBoardTerminalDistanceArray
                      : bikeAccess ? originParcelUsed.FirstPositionInBikeParkAndRideNodeDistanceArray
                      : sovAccess ? originParcelUsed.FirstPositionInAutoParkAndRideNodeDistanceArray
                      : originParcelUsed.FirstPositionInAutoKissAndRideTerminalDistanceArray;

      int LastIndexAccess = Math.Min(firstIndexAccess + maxStopAreasToSearchAccess - 1,
                        walkAccess ? originParcelUsed.LastPositionInStopAreaDistanceArray
                      : bikeOnBoard ? originParcelUsed.LastPositionInBikeOnBoardTerminalDistanceArray
                      : bikeAccess ? originParcelUsed.LastPositionInBikeParkAndRideNodeDistanceArray
                      : sovAccess ? originParcelUsed.LastPositionInAutoParkAndRideNodeDistanceArray
                      : originParcelUsed.LastPositionInAutoKissAndRideTerminalDistanceArray);

      int firstIndexEgress = walkEgress ? destinationParcelUsed.FirstPositionInStopAreaDistanceArray
                      : bikeOnBoard ? destinationParcelUsed.FirstPositionInBikeOnBoardTerminalDistanceArray
                      : bikeEgress ? destinationParcelUsed.FirstPositionInBikeParkAndRideNodeDistanceArray
                      : destinationParcelUsed.FirstPositionInAutoKissAndRideTerminalDistanceArray;

      int LastIndexEgress = Math.Min(firstIndexEgress + maxStopAreasToSearchEgress - 1,
                        walkEgress ? destinationParcelUsed.LastPositionInStopAreaDistanceArray
                      : bikeOnBoard ? destinationParcelUsed.LastPositionInBikeOnBoardTerminalDistanceArray
                      : bikeEgress ? destinationParcelUsed.LastPositionInBikeParkAndRideNodeDistanceArray
                      : destinationParcelUsed.LastPositionInAutoKissAndRideTerminalDistanceArray);



      if (firstIndexAccess <= 0 || firstIndexEgress <= 0) {
        return;
      }

      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
      double bestPathUtility = -99999D;

      for (int indexAccess = firstIndexAccess; indexAccess <= LastIndexAccess; indexAccess++) {

        double stopAreaLengthAccess = (walkAccess ? Global.ParcelStopAreaLengths[indexAccess]
                       : bikeOnBoard ? Global.ParcelToBikeOnBoardTerminalLength[indexAccess]
                       : bikeAccess ? Global.ParcelToBikeParkAndRideNodeLength[indexAccess]
                       : sovAccess ? Global.ParcelToAutoParkAndRideNodeLength[indexAccess]
                       : Global.ParcelToAutoKissAndRideTerminalLength[indexAccess]) / 1000.0; //convert to km

        if (stopAreaLengthAccess > maxStopAreaLengthAccess) {
         // Global.PrintFile.WriteLine("Access mode {2} for mz {3} index {4}, distance {0} exceeds max {1}",
         //         stopAreaLengthAccess,maxStopAreaLengthAccess,accessMode,originParcelUsed.Id,indexAccess);
          continue;
        }

        int accessTerminalKey = -1;
        int accessTerminalIndex = -1;
        int egressTerminalKey = -1;
        int egressTerminalIndex = -1;

        double accessDistance = stopAreaLengthAccess;
        double accessTime = 0;
        double accessCost = 0;
        double accessUtility = 0;
        int accessParkAndRideNodeID = 0;

        double roundTripFactor = (_returnTime > 0) ? 2 : 1;

        if (walkAccess) {
          accessTerminalKey = Global.ParcelStopAreaStopAreaKeys[indexAccess];
          accessTerminalIndex = Global.ParcelStopAreaStopAreaIds[indexAccess];
          accessTime = accessDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit * roundTripFactor;
          accessUtility = accessTime * _tourTimeCoefficient * Global.Configuration.PathImpedance_WalkAccessTimeWeight;
        } else if (bikeAccess && bikeOnBoard) {
          accessTerminalKey = Global.ParcelToBikeOnBoardTerminalKeys[indexAccess];
          accessTerminalIndex = Global.ParcelToBikeOnBoardTerminalIndices[indexAccess];
          int terminalMicrozoneId = Global.ParcelToBikeOnBoardMicrozoneIds[indexAccess];
          ParcelWrapper bikeOnBoardParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[terminalMicrozoneId];
          WalkBikePath accessPath = GetWalkBikePath(Global.Settings.Modes.HovPassenger, pathTypeAccEgr, votValue, useZones, _outboundTime, _returnTime, 0, 0, originParcelUsed, bikeOnBoardParcel);
          accessTime = accessPath.Time * roundTripFactor;
          accessCost = 0;
          accessUtility = accessPath.Utility * roundTripFactor;
        } else if (bikeAccess && !bikeOnBoard) {
          int nodeId = Global.ParcelToBikeParkAndRideNodeIds[indexAccess];
          ParkAndRideNodeWrapper node = bikeParkAndRideNodes.First(x => x.ZoneId == nodeId);
          accessTerminalKey = node.NearestStopAreaId;
          accessTerminalIndex = Global.TransitStopAreaMapping[accessTerminalKey];
          ParcelWrapper parkAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[node.NearestParcelId];
          WalkBikePath accessPath = GetWalkBikePath(accessMode, pathTypeAccEgr, votValue, useZones, _outboundTime, _returnTime, 0, 0, originParcelUsed, parkAndRideParcel);
          double parkingCost = node.CostDaily;
          accessTime = accessPath.Time * roundTripFactor;
          accessCost = parkingCost;
          accessUtility = accessPath.Utility * roundTripFactor;
        } else if (sovAccess) {
          int nodeId = Global.ParcelToAutoParkAndRideNodeIds[indexAccess];
          ParkAndRideNodeWrapper node = autoParkAndRideNodes.First(x => x.ZoneId == nodeId);
          accessTerminalKey = node.NearestStopAreaId;
          accessTerminalIndex = Global.TransitStopAreaMapping[accessTerminalKey];
          ParcelWrapper parkAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[node.NearestParcelId];
          AutoPath accessPath = GetAutoPath(accessMode, pathTypeAccEgr, votValue, useZones, false, _outboundTime, _returnTime, 0, 0, originParcelUsed, parkAndRideParcel);

          double duration = 0.0;
          if (_returnTime > 0) {
            duration = 1.0 + Math.Truncate((_returnTime - _outboundTime) / 60.0);
          } else if (_purpose == Global.Settings.Purposes.Work) {
            duration = 8.0;
          } else if (_purpose == Global.Settings.Purposes.School) {
            duration = 6.0;
          } else if (_purpose == Global.Settings.Purposes.Social) {
            duration = 3.0;
          } else {
            duration = 2.0;
          }

          double parkingCostByTheHour =
             (node.ParkingTypeId == 1 || node.ParkingTypeId == 3)
                    ? 0.0
                    : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ElevenPM, Global.Settings.Times.MinutesInADay)
                        ? node.CostPerHour23_08 * duration
                        : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.EightAM)
                            ? node.CostPerHour23_08 * duration
                            : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.SixPM)
                                ? node.CostPerHour08_18 * duration
                                : node.CostPerHour18_23 * duration;

          double parkingCost = Math.Min(parkingCostByTheHour, node.CostDaily);

          accessTime = accessPath.Time * roundTripFactor;
          accessCost = accessPath.Cost + parkingCost;
          accessUtility = accessPath.Utility * roundTripFactor;
          accessParkAndRideNodeID = nodeId;
        } else if (hovAccess || shareAccess) {
          accessTerminalKey = Global.ParcelToAutoKissAndRideTerminalKeys[indexAccess];
          accessTerminalIndex = Global.ParcelToAutoKissAndRideTerminalIndices[indexAccess];
          int terminalMicrozoneId = Global.ParcelToAutoKissAndRideMicrozoneIds[indexAccess];
          ParcelWrapper kissAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[terminalMicrozoneId];
          AutoPath accessPath = GetAutoPath(Global.Settings.Modes.HovPassenger, pathTypeAccEgr, votValue, useZones, false, _outboundTime, _returnTime, 0, 0, originParcelUsed, kissAndRideParcel);
          accessTime = accessPath.Time * roundTripFactor;
          accessCost = accessPath.Cost * roundTripFactor;
          accessUtility = accessPath.Utility * roundTripFactor;
        }

        for (int indexEgress = firstIndexEgress; indexEgress <= LastIndexEgress; indexEgress++) {


          double stopAreaLengthEgress = (walkEgress ? Global.ParcelStopAreaLengths[indexEgress]
                         : bikeOnBoard ? Global.ParcelToBikeOnBoardTerminalLength[indexEgress]
                         : bikeEgress ? Global.ParcelToBikeParkAndRideNodeLength[indexEgress]
                         : Global.ParcelToAutoKissAndRideTerminalLength[indexEgress]) / 1000.0; //convert to km

          if (stopAreaLengthEgress > maxStopAreaLengthEgress) {
            //Global.PrintFile.WriteLine("Egress mode {2} for mz {3} index {4}, distance {0} exceeds max {1}",
            //        stopAreaLengthEgress, maxStopAreaLengthEgress, egressMode, destinationParcelUsed.Id, indexEgress);
            continue;
          }


          double egressDistance = stopAreaLengthEgress;
          double egressTime = 0;
          double egressCost = 0;
          double egressUtility = 0;

          if (walkEgress) {
            egressTerminalKey = Global.ParcelStopAreaStopAreaKeys[indexEgress];
            egressTerminalIndex = Global.ParcelStopAreaStopAreaIds[indexEgress];
            egressTime = egressDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit * roundTripFactor;
            egressUtility = egressTime * _tourTimeCoefficient * Global.Configuration.PathImpedance_WalkAccessTimeWeight;
          } else if (bikeEgress && bikeOnBoard) {
            egressTerminalKey = Global.ParcelToBikeOnBoardTerminalKeys[indexEgress];
            egressTerminalIndex = Global.ParcelToBikeOnBoardTerminalIndices[indexEgress];
            int terminalMicrozoneId = Global.ParcelToBikeOnBoardMicrozoneIds[indexEgress];
            ParcelWrapper bikeOnBoardParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[terminalMicrozoneId];
            WalkBikePath egressPath = GetWalkBikePath(egressMode, pathTypeAccEgr, votValue, useZones, _outboundTime, _returnTime, 0, 0, bikeOnBoardParcel, destinationParcelUsed);
            egressTime = egressPath.Time * roundTripFactor;
            egressCost = 0;
            egressUtility = egressPath.Utility * roundTripFactor;
          } else if (bikeEgress && !bikeOnBoard) {
            int nodeId = Global.ParcelToBikeParkAndRideNodeIds[indexEgress];
            ParkAndRideNodeWrapper node = bikeParkAndRideNodes.First(x => x.ZoneId == nodeId);
            egressTerminalKey = node.NearestStopAreaId;
            egressTerminalIndex = Global.TransitStopAreaMapping[egressTerminalKey];
            ParcelWrapper parkAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[node.NearestParcelId];
            WalkBikePath egressPath = GetWalkBikePath(egressMode, pathTypeAccEgr, votValue, useZones, _outboundTime, _returnTime, 0, 0, parkAndRideParcel, destinationParcelUsed);
            egressTime = egressPath.Time * roundTripFactor;
            egressCost = 0;
            egressUtility = egressPath.Utility * roundTripFactor;
          } else if (shareEgress) {
            egressTerminalKey = Global.ParcelToAutoKissAndRideTerminalKeys[indexEgress];
            egressTerminalIndex = Global.ParcelToAutoKissAndRideTerminalIndices[indexEgress];
            int terminalMicrozoneId = Global.ParcelToAutoKissAndRideMicrozoneIds[indexEgress];
            ParcelWrapper kissAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[terminalMicrozoneId];
            AutoPath egressPath = GetAutoPath(Global.Settings.Modes.HovPassenger, pathTypeAccEgr, votValue, useZones, false, _outboundTime, _returnTime, 0, 0, kissAndRideParcel, destinationParcelUsed);
            egressTime = egressPath.Time * roundTripFactor;
            egressCost = egressPath.Cost * roundTripFactor;
            egressUtility = egressPath.Utility * roundTripFactor;
          }


          TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, accessTerminalIndex, egressTerminalIndex, _transitPassOwnership);

          if (!transitPath.Available) {
            //Global.PrintFile.WriteLine("No transit path 1: access mode {2} for mz {3} index {4}, terminal {0}, out time {1}",
            //  accessTerminalIndex, _outboundTime, accessMode, originParcelUsed.Id, indexAccess);
            //Global.PrintFile.WriteLine("No transit path 2: egress mode {2} for mz {3} index {4}, terminal {0}, ret time {1}",
            //  egressTerminalIndex, _returnTime, egressMode, destinationParcelUsed.Id, indexEgress);
            continue;
          }

          if (transitPath.Time + accessTime + egressTime > pathTimeLimit) {
            //Global.PrintFile.WriteLine("Transit path too long 1: access mode {3} for mz {4}, terminal {0}, access time {1} transit time {4}",
            //  egressTerminalIndex, accessTime, accessMode, originParcelUsed.Id, transitPath.Time);
            //Global.PrintFile.WriteLine("Transit path too long 2: egress mode {3} for mz {4}, terminal {0}, egress time {1} total time {4}",
            //   egressTerminalIndex, egressTime, egressMode, destinationParcelUsed.Id, transitPath.Time+accessTime+egressTime);
            continue;
          }
          // set utility

          double fullPathUtility = transitPath.Utility + accessUtility + egressUtility;

          // if the best path so far, reset pathType properties
          if (fullPathUtility <= bestPathUtility) {
            continue;
          }

          bestPathUtility = fullPathUtility;

          _pathOriginStopAreaKey[pathType] = accessTerminalKey;
          _pathDestinationStopAreaKey[pathType] = egressTerminalKey;
          _pathDistance[pathType] = transitPath.Distance + accessDistance + egressDistance;
          _pathTime[pathType] = transitPath.Time + accessTime + egressTime;
          _pathCost[pathType] = transitPath.Cost + accessCost + egressCost;
          _utility[pathType] = fullPathUtility;
          _expUtility[pathType] = fullPathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : fullPathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(fullPathUtility);
          _pathWalkDistance[pathType] = ((walkAccess) ? accessDistance : 0) + ((walkEgress) ? egressDistance : 0);
          _pathWalkTime[pathType] = ((walkAccess) ? accessTime : 0) + ((walkEgress) ? egressTime : 0);
          _pathBikeDistance[pathType] = ((bikeAccess) ? accessDistance : 0) + ((bikeEgress) ? egressDistance : 0);
          _pathBikeTime[pathType] = ((bikeAccess) ? accessTime : 0) + ((bikeEgress) ? egressTime : 0);
          _pathBikeCost[pathType] = ((bikeAccess) ? accessCost : 0) + ((bikeEgress) ? egressCost : 0);
          _pathTransitTime[pathType] = transitPath.Time;
          _pathTransitDistance[pathType] = transitPath.Distance;
          _pathTransitCost[pathType] = transitPath.Cost;
          _pathTransitUtility[pathType] = transitPath.Utility;

          _pathOriginAccessMode[pathType] = accessMode;
          _pathOriginAccessTime[pathType] = accessTime;
          _pathOriginAccessDistance[pathType] = accessDistance;
          _pathOriginAccessCost[pathType] = accessCost;
          _pathOriginAccessUtility[pathType] = accessUtility;
          _pathDestinationAccessMode[pathType] = egressMode;
          _pathDestinationAccessTime[pathType] = egressTime;
          _pathDestinationAccessDistance[pathType] = egressDistance;
          _pathDestinationAccessCost[pathType] = egressCost;
          _pathDestinationAccessUtility[pathType] = egressUtility;
          _pathParkAndRideNodeId[pathType] = accessParkAndRideNodeID;
        }
      }

    }


    public class WalkBikePath {
      public bool Available { get; set; }
      public double Time { get; set; }
      public double GenTime { get; set; }
      public double Distance { get; set; }
      public double Utility { get; set; }
    }

    private WalkBikePath GetWalkBikePath(int skimMode, int pathType, double votValue, bool useZones, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ParcelWrapper originParcel, ParcelWrapper destinationParcel) {

      WalkBikePath path = new WalkBikePath {
        Available = false
      };

      if (!useZones) {

        originZoneId = originParcel.ZoneId;
        destinationZoneId = destinationParcel.ZoneId;
        //JB 20190428 add the following to initialize the values when !useZones; needed on line 1007
        _originZoneId = originParcel.ZoneId;
        _destinationZoneId = destinationParcel.ZoneId;
      }

      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);
      bool commuter = (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Business);

      double zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;

      bool blendMZ = (!useZones && zzDist > Constants.EPSILON && zzDist < Global.Configuration.MaximumBlendingDistance);

      bool intraZonal = (_originZoneId == _destinationZoneId);

      double mzDistance = 0.0;
      double mzTime = 0.0;
      double mzGenTime = 0.0;

      if ((intraZonal || blendMZ) && !useZones) {
        mzDistance = ImpedanceRoster.GetValue("distance-mz", Global.Settings.Modes.Walk, Global.Settings.PathTypes.FullNetwork, 60, _outboundTime, _originParcel, _destinationParcel).Variable / 1000.0; //in km
        double roundTripFactor = (_returnTime > 0) ? 2.0 : 1.0;
        if (_originParcel.Id != _destinationParcel.Id) {
          mzDistance = mzDistance * Global.Configuration.COMPASS_IntrazonalStraightLineDistanceFactor;
        }

        if (skimMode == Global.Settings.Modes.Walk) {
          mzTime = mzDistance * Global.Configuration.COMPASS_IntrazonalMinutesPerKM_Walk
                              + Global.Configuration.COMPASS_IntrazonalMinutesExtra_Walk;
          mzGenTime = mzTime * Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesPerMinute_Walk
                             + Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesExtra_Walk;

        } else {
          mzTime = mzDistance * Global.Configuration.COMPASS_IntrazonalMinutesPerKM_Bike
                              + Global.Configuration.COMPASS_IntrazonalMinutesExtra_Bike;
          mzGenTime = mzTime * Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesPerMinute_Bike
                             + Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesExtra_Bike;
        }
        mzDistance = mzDistance * roundTripFactor;
        mzTime = mzTime * roundTripFactor;
        mzGenTime = mzGenTime * roundTripFactor;
      }

      double circuityDistance = Constants.DEFAULT_VALUE;

      if (intraZonal) {
        path.Distance = mzDistance;
        path.Time = mzTime;
        path.GenTime = mzGenTime;
      } else {
        string varname = commuter ? "time-co" : "time";
        SkimValue skimValue =
                useZones
                    ? ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
                     : ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        path.Time = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          path.Time += skimValue.Variable;
        }

        if (path.Time > pathTimeLimit) {
          return path;
        }

        varname = commuter ? "distance-co" : "distance";
        skimValue =
          useZones
              ? ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
              : ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        path.Distance = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          path.Distance += skimValue.Variable;
        }

        varname = commuter ? "lstime-co" : "lstime";
        skimValue =
          useZones
              ? ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
              : ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        path.GenTime = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          path.GenTime += skimValue.Variable;
        }


        //if blending factor down and blend with mz values
        if (blendMZ) {
          double zoneFactor = zzDist / Math.Max(Global.Configuration.MaximumBlendingDistance, Constants.EPSILON);

          path.Distance = zoneFactor * path.Distance + (1.0 - zoneFactor) * mzDistance;
          path.Time = zoneFactor * path.Time + (1.0 - zoneFactor) * mzTime;
          path.GenTime = zoneFactor * path.GenTime + (1.0 - zoneFactor) * mzGenTime;
        }

        /* a fix for intra-parcels, which happen once in a great while for school
        if (!useZones && _originParcel.Id == _destinationParcel.Id && skimMode == Global.Settings.Modes.Walk
          //JLB 20130628 added destination scale condition because ImpedanceRoster assigns time and cost values for intrazonals 
          && Global.Configuration.DestinationScale != Global.Settings.DestinationScales.Zone) {
          path.Time = 1.0;
          path.Distance = 0.01 * Global.Settings.DistanceUnitsPerMile;  // JLBscale.  multiplied by distance units per mile
        }*/
      }
      path.Utility = path.GenTime * _tourTimeCoefficient;

      path.Available = true;
      return path;

    }

    public class AutoPath {
      public bool Available { get; set; }
      public double Time { get; set; }
      public double GenTime { get; set; }
      public double Distance { get; set; }
      public double Cost { get; set; }
      public double Utility { get; set; }
    }

    private AutoPath GetAutoPath(int skimModeIn, int pathType, double votValue, bool useZones, bool useAVVOT, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ParcelWrapper originParcel, ParcelWrapper destinationParcel) {

      AutoPath path = new AutoPath {
        Available = false
      };

     //JB 20190528 add the following to initialize the values when !useZones; needed on subsequent lines
      if (!useZones) {
        originZoneId = originParcel.ZoneId;
        destinationZoneId = destinationParcel.ZoneId;
        _originZoneId = originParcel.ZoneId;
        _destinationZoneId = destinationParcel.ZoneId;
      }

      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);

      bool commuter = (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School);
      bool business = (_purpose == Global.Settings.Purposes.Business);

      bool intraZonal = (_originZoneId == _destinationZoneId);

      double mzDistance = 0.0;
      double mzTime = 0.0;
      double mzGenTime = 0.0;

      if (intraZonal && !useZones) {
        mzDistance = ImpedanceRoster.GetValue("distance-mz", Global.Settings.Modes.Walk, Global.Settings.PathTypes.FullNetwork, 60, _outboundTime, _originParcel, _destinationParcel).Variable / 1000.0; //in km
        double roundTripFactor = (_returnTime > 0) ? 2.0 : 1.0;
        if (_originParcel.Id != _destinationParcel.Id) {
          mzDistance = mzDistance * Global.Configuration.COMPASS_IntrazonalStraightLineDistanceFactor;
        }
        if (skimModeIn == Global.Settings.Modes.Sov) {
          mzTime = mzDistance * Global.Configuration.COMPASS_IntrazonalMinutesPerKM_SOV
                              + Global.Configuration.COMPASS_IntrazonalMinutesExtra_SOV;
          mzGenTime = mzTime * Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesPerMinute_SOV
                             + Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesExtra_SOV;
        } else {
          mzTime = mzDistance * Global.Configuration.COMPASS_IntrazonalMinutesPerKM_HOV
                              + Global.Configuration.COMPASS_IntrazonalMinutesExtra_HOV;
          mzGenTime = mzTime * Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesPerMinute_HOV
                             + Global.Configuration.COMPASS_IntrazonalGeneralizedMinutesExtra_HOV;
        }
        path.Distance = mzDistance * roundTripFactor;
        path.Time = mzTime * roundTripFactor;
        path.GenTime = mzGenTime * roundTripFactor;
      } else {
        double circuityDistance = Constants.DEFAULT_VALUE;

        string varname = commuter ? "time-co" : business ? "time-bu" : "time";
        SkimValue skimValue =
                useZones
                    ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
                    : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        //if (skimValue.Variable < Constants.EPSILON) {
        //  return path;
        //}
        path.Time = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          //if (skimValue.Variable < Constants.EPSILON) {
          //  return path;
          //}
          path.Time += skimValue.Variable;
        }

        if (path.Time > pathTimeLimit) {
          return path;
        }

        varname = commuter ? "distance-co" : business ? "distance-bu" : "distance";
        skimValue =
          useZones
              ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
              : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        path.Distance = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          path.Distance += skimValue.Variable;
        }

        varname = commuter ? "lstime-co" : business ? "lstime-bu" : "lstime";
        skimValue =
          useZones
              ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
              : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        path.GenTime = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          path.GenTime += skimValue.Variable;
        }

        varname = commuter ? "toll-co" : business ? "toll-bu" : "toll";
        skimValue =
          useZones
              ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originZoneId, destinationZoneId)
              : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, outboundTime, originParcel, destinationParcel, circuityDistance);
        path.Cost = skimValue.Variable;

        if (_returnTime > 0) {
          skimValue =
            useZones
                ? ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue(varname, skimModeIn, pathType, votValue, returnTime, destinationParcel, originParcel, circuityDistance);
          path.Cost += skimValue.Variable;
        }
      }
      path.Utility = path.GenTime * _tourTimeCoefficient;

      path.Available = true;
      return path;

    }

    private class TransitPath {
      public bool Available { get; set; }
      public double Time { get; set; }
      public double Distance { get; set; }
      public double GenTime { get; set; }
      public double Cost { get; set; }
      public double Utility { get; set; }
    }

    private TransitPath GetTransitPath(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, int transitPassOwnership) {

      TransitPath path = new TransitPath {
        Available = false
      };

      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);

      bool commuter = (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Business);
      string varname = commuter ? "distance-co" : "distance";

      SkimValue skimValue =
        ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId);
      double outboundDistance = skimValue.Variable;
      if (outboundDistance < Constants.EPSILON) {
        return path;
      }
      path.Distance = outboundDistance;

      if (_returnTime > 0) {
        skimValue =
          ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId);
        double returnDistance = skimValue.Variable;
        if (returnDistance < Constants.EPSILON) {
          return path;
        }
        path.Distance += returnDistance;
      }

      varname = commuter ? "time-co" : "time";
      skimValue =
              ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId);
      path.Time = skimValue.Variable;

      if (_returnTime > 0) {
        skimValue =
              ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId);
        path.Time += skimValue.Variable;
      }

      if (path.Time > pathTimeLimit) {
        return path;
      }

      varname = commuter ? "gentime-co" : "gentime";
      skimValue =
           ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId);
      path.GenTime = skimValue.Variable;

      if (_returnTime > 0) {
        skimValue =
           ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId);
        path.GenTime += skimValue.Variable;
      }

      path.Cost = 0;
      varname = "farezones";
      skimValue =
             ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId);
      int fareZones = (int)Math.Round(skimValue.Variable);
      if (fareZones > transitPassOwnership && _personAge > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel) {
        double fare = Global.TransitBaseFare_Adult[fareZones];
        if (_personAge <= Global.Configuration.COMPASS_TransitFareMaximumAgeForChildDiscount) {
          fare = fare * (1 - Global.TransitBaseFare_ChildDiscount[fareZones] / 100.0);
        }
        if (outboundTime + 180 < Global.Configuration.COMPASS_TransitFareAMPeakPeriodStartMinute ||
          (outboundTime + 180 >= Global.Configuration.COMPASS_TransitFareAMPeakPeriodEndMinute - 180 && outboundTime + 180 < Global.Configuration.COMPASS_TransitFarePMPeakPeriodStartMinute)
          || outboundTime + 180 >= Global.Configuration.COMPASS_TransitFarePMPeakPeriodEndMinute) {
          fare = fare * (1 - Global.TransitBaseFare_OffPeakDiscount[fareZones] / 100.0);
        }
        path.Cost = fare;
      }
      if (_returnTime > 0) {
        skimValue =
           ImpedanceRoster.GetValue(varname, skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId);
        fareZones = (int)Math.Round(skimValue.Variable);
        if (fareZones > transitPassOwnership && _personAge > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel) {
          double fare = Global.TransitBaseFare_Adult[fareZones];
          if (_personAge <= Global.Configuration.COMPASS_TransitFareMaximumAgeForChildDiscount) {
            fare = fare * (1 - Global.TransitBaseFare_ChildDiscount[fareZones] / 100.0);
          }
          if (returnTime + 180 < Global.Configuration.COMPASS_TransitFareAMPeakPeriodStartMinute ||
            (returnTime + 180 >= Global.Configuration.COMPASS_TransitFareAMPeakPeriodEndMinute - 180 && returnTime + 180 < Global.Configuration.COMPASS_TransitFarePMPeakPeriodStartMinute)
            || returnTime + 180 >= Global.Configuration.COMPASS_TransitFarePMPeakPeriodEndMinute) {
            fare = fare * (1 - Global.TransitBaseFare_OffPeakDiscount[fareZones] / 100.0);
          }
          path.Cost += fare;
        }
      }

      path.Utility = path.GenTime * _tourTimeCoefficient
                   + path.Cost * _tourCostCoefficient;

      path.Available = true;

      return path;
    }
    /*
        private void RunStopAreaWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

          if (useZones) {
            return;
          }
          //user-set limits on search - use high values if not set
          int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearch > 0) ? Global.Configuration.MaximumStopAreasToSearch : 99;
          int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnits > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnits : 99999;

          //_originParcel. SetFirstAndLastStopAreaDistanceIndexes();
          int oFirst = _originParcel.FirstPositionInStopAreaDistanceArray;
          int oLast = Math.Min(_originParcel.LastPositionInStopAreaDistanceArray, oFirst + maxStopAreasToSearch - 1);

          //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
          int dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
          int dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

          if (oFirst <= 0 || dFirst <= 0) {
            return;
          }

          double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
          double bestPathUtility = -99999D;

          for (int oIndex = oFirst; oIndex <= oLast; oIndex++) {
            int oStopArea = Global.ParcelStopAreaStopAreaIds[oIndex];
            int oStopAreaKey = Global.ParcelStopAreaStopAreaKeys[oIndex];
            float oWalkLength = Global.ParcelStopAreaLengths[oIndex];
            if (oWalkLength > maxStopAreaLength) {
              continue;
            }

            for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
              int dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
              int dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
              float dWalkLength = Global.ParcelStopAreaLengths[dIndex];
              if (dWalkLength > maxStopAreaLength) {
                continue;
              }

              double oWalkDistance = (oWalkLength) / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile * (_returnTime > 0 ? 2 : 1);
              double dWalkDistance = (dWalkLength) / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile * (_returnTime > 0 ? 2 : 1);
              double oWalkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * oWalkDistance;
              double dWalkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * dWalkDistance;
              double walkDistance = (oWalkDistance + dWalkDistance);
              double walkTime = (oWalkTime + dWalkTime);

              TransitPath path = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oStopArea, dStopArea, _transitPassOwnership);

              if (!path.Available) {
                continue;
              }
              // set utility

              double fullPathUtility = path.Utility
                 + walkTime * _tourTimeCoefficient * Global.Configuration.PathImpedance_WalkAccessTimeWeight;


              // if the best path so far, reset pathType properties
              if (fullPathUtility <= bestPathUtility) {
                continue;
              }

              bestPathUtility = fullPathUtility;

              _pathOriginStopAreaKey[pathType] = oStopAreaKey;
              _pathDestinationStopAreaKey[pathType] = dStopAreaKey;
              _pathDistance[pathType] = path.Distance + walkDistance;
              _pathTime[pathType] = path.Time + walkTime;
              _pathCost[pathType] = path.Cost;
              _utility[pathType] = fullPathUtility;
              _expUtility[pathType] = fullPathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : fullPathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(fullPathUtility);
              _pathWalkDistance[pathType] = walkDistance;
              _pathWalkTime[pathType] = walkTime;
              _pathTransitTime[pathType] = path.Time;
              _pathTransitDistance[pathType] = path.Distance;
              _pathTransitCost[pathType] = path.Cost;
              _pathTransitUtility[pathType] = path.Utility;
              _pathOriginAccessMode[pathType] = Global.Settings.Modes.Walk;
              _pathOriginAccessTime[pathType] = oWalkTime;
              _pathOriginAccessDistance[pathType] = oWalkDistance;
              _pathOriginAccessCost[pathType] = 0.0;
              _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Walk;
              _pathDestinationAccessTime[pathType] = dWalkTime;
              _pathDestinationAccessDistance[pathType] = dWalkDistance;
              _pathDestinationAccessCost[pathType] = 0.0;
            }
          }
          _pathParkAndRideNodeId[pathType] = 0;
          _pathBikeCost[pathType] = 0;
          _pathBikeTime[pathType] = 0;
          _pathBikeDistance[pathType] = 0;

        }
    */
    /*
    private void RunStopAreaParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
    if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
      return;
    }

    List<ParkAndRideNodeWrapper> parkAndRideNodes = _parkAndRideAutoNodesWithCapacity;

    // valid node(s), and tour-level call  
    double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
    double bestNodeUtility = -99999D;
    int originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
    double zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

    //user-set limits on search - use high values if not set
    int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
    int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide : 99999;
    double maxDistanceUnitsToDrive = (Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide > 0) ? Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide : 999D;
    double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

    //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
    int dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
    int dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

    if (dFirst <= 0) {
      return;
    }
    foreach (ParkAndRideNodeWrapper node in parkAndRideNodes) {
      // only look at nodes with positive capacity and (JLB 201508) auto parking nodes 
      // if (node.Capacity < Constants.EPSILON || !(node.Auto == 1)) {       //Added this logic to declaration/selection of parkAndRideNodes above
      //	continue;
      //}

      // use the nearest stop area for transit LOS  
      ParcelWrapper parkAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[node.NearestParcelId];
      int parkAndRideZoneId = node.ZoneId;
      int parkAndRideStopAreaKey = node.NearestStopAreaId;
      int parkAndRideStopArea = Global.TransitStopAreaMapping[node.NearestStopAreaId];
      //JLB 201508 logic in followign line replaced with new cost logic further below
      double parkAndRideParkingCost = 0.0;   //node.Cost / 100.0;    // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

      //test distance to park and ride against user-set limits
      double zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

      if (zzDist > maxDistanceUnitsToDrive) {
        continue;
      }


      if (zzDist / Math.Max(zzDist, 1.0) > maxDistanceRatio) {
        continue;
      }
      double duration = 0.0;
      if (node.ParkingTypeId == 2 || node.ParkingTypeId == 3) {
        if (_returnTime <= 0) {
          if (_purpose == Global.Settings.Purposes.Work) {
            duration = 8.0;
          } else if (_purpose == Global.Settings.Purposes.School) {
            duration = 6.0;
          } else if (_purpose == Global.Settings.Purposes.Social) {
            duration = 3.0;
          } else {
            duration = 2.0;
          }
        } else {
          duration = (_returnTime - _outboundTime) / 60.0;
        }
      }
      if (node.ParkingTypeId == 3 && duration > 2.0 + Constants.EPSILON) {
        continue;      // parking duration limited to 2 hours
      } else {
        parkAndRideParkingCost =
            (node.ParkingTypeId == 1 || node.ParkingTypeId == 3)
                ? 0.0
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ElevenPM, Global.Settings.Times.MinutesInADay)
                    ? node.CostPerHour23_08 * duration
                    : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.EightAM)
                        ? node.CostPerHour23_08 * duration
                        : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.SixPM)
                            ? node.CostPerHour08_18 * duration
                            : node.CostPerHour18_23 * duration;
      }

      double oStationWalkDistance = 2.0 * node.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits
      double oStationWalkTime = oStationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

      bool useAVVOT = _carsAreAVs;
      AutoPath autoPath = GetAutoPath(Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, useZones, useAVVOT, originZoneId, parkAndRideZoneId, _originParcel, parkAndRideParcel);

      //loop on stop areas near destination
      for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
        int dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
        int dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
        float dWalkLength = Global.ParcelStopAreaLengths[dIndex];
        if (dWalkLength > maxStopAreaLength) {
          continue;
        }

        double destinationWalkDistance = 2 * dWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
        double destinationWalkTime = destinationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

        TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dStopArea, _transitPassOwnership);
        if (!transitPath.Available) {
          continue;
        }

        int parkMinute = (int)(_outboundTime - (transitPath.Time / 2.0) - 3); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

        // set utility
        double nodePathTime = transitPath.Time + autoPath.Time + oStationWalkTime + destinationWalkTime;
        double nodePathDistance = autoPath.Distance + transitPath.Distance + oStationWalkDistance + destinationWalkDistance;
        double nodePathCost = transitPath.Cost + autoPath.Cost + parkAndRideParkingCost;

        if (nodePathTime > pathTimeLimit) {
          continue;
        }

        double nodeUtility = transitPath.Utility + autoPath.Utility +
                      Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                      (_tourCostCoefficient * parkAndRideParkingCost +
                      _tourTimeCoefficient * Global.Configuration.PathImpedance_WalkAccessTimeWeight * (destinationWalkTime + oStationWalkTime));

        if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
          nodeUtility += node.ShadowPrice[parkMinute];
        }

        // if the best path so far, reset pathType properties
        if (nodeUtility <= bestNodeUtility) {
          continue;
        }

        bestNodeUtility = nodeUtility;

        _pathParkAndRideNodeId[pathType] = node.Id;
        _pathOriginStopAreaKey[pathType] = parkAndRideStopAreaKey;
        _pathDestinationStopAreaKey[pathType] = dStopAreaKey;
        _pathTime[pathType] = nodePathTime;
        _pathDistance[pathType] = nodePathDistance;
        _pathCost[pathType] = nodePathCost;
        _pathTransitTime[pathType] = transitPath.Time;
        _pathTransitDistance[pathType] = transitPath.Distance;
        _pathTransitCost[pathType] = transitPath.Cost;
        _pathTransitUtility[pathType] = transitPath.Utility;
        _pathWalkTime[pathType] = oStationWalkTime + destinationWalkTime;
        _pathWalkDistance[pathType] = oStationWalkDistance + destinationWalkDistance;
        _utility[pathType] = nodeUtility;
        _expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
        _pathOriginAccessMode[pathType] = Global.Settings.Modes.Sov;
        _pathOriginAccessTime[pathType] = autoPath.Time;
        _pathOriginAccessDistance[pathType] = autoPath.Distance;
        _pathOriginAccessCost[pathType] = autoPath.Cost;
        _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Walk;
        _pathDestinationAccessTime[pathType] = destinationWalkTime;
        _pathDestinationAccessDistance[pathType] = destinationWalkDistance;
        _pathDestinationAccessCost[pathType] = 0.0;
      }
    }
    _pathBikeCost[pathType] = 0;
    _pathBikeTime[pathType] = 0;
    _pathBikeDistance[pathType] = 0;
  }
    */

    /*
       private class TransitPath {
         public bool Available { get; set; }
         public double Time { get; set; }
         public double Distance { get; set; }
         public double Cost { get; set; }
         public double Boardings1 { get; set; }
         public double Boardings2 { get; set; }
         public double Utility { get; set; }
       }

       private PathTypeModel_Actum.TransitPath GetTransitPath(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, int transitPassOwnership) {

         TransitPath path = new PathTypeModel_Actum.TransitPath {
           Available = true
         };

         // check for presence of valid path
         double outboundInVehicleDistance = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double returnInVehicleDistance = returnTime > 0
                       ? ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                       : 0;

         if (outboundInVehicleDistance < Constants.EPSILON || (returnTime > 0 && returnInVehicleDistance < Constants.EPSILON)) {
           path.Available = false;
           return path;
         }
         // valid path(s).  Proceed.

         path.Distance = outboundInVehicleDistance + returnInVehicleDistance;

         double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);

         //Actum:  nonlinear utility with submode-speciifc time components in a generic transit mode (labeled local bus)
         //get skim values
         double firstAndHiddenWaitTime = ImpedanceRoster.GetValue("firstandhiddenwaittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double waitTime = ImpedanceRoster.GetValue("waittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;  // was transfer wait time
         double fareCard10 = ImpedanceRoster.GetValue("farecard10", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         //var fareCardMonth = ImpedanceRoster.GetValue("farecardmonth", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double extraFareCard10 = skimMode == Global.Settings.Modes.BikeOnTransit ? ImpedanceRoster.GetValue("farecard10", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable : 0;
         //var accessEgressTime = ImpedanceRoster.GetValue("accegrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double fTime = ImpedanceRoster.GetValue("invehtimeferry", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double gTime = ImpedanceRoster.GetValue("invehtimemetro", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double bTime = ImpedanceRoster.GetValue("invehtimebus", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double pTime = ImpedanceRoster.GetValue("invehtimelrandlocaltrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double rTime = ImpedanceRoster.GetValue("invehtimeretrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double sTime = ImpedanceRoster.GetValue("invehtimestrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double iTime = ImpedanceRoster.GetValue("invehtimeictrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         //var noOfChanges = ImpedanceRoster.GetValue("noofchanges", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
         double transferWalkTime = ImpedanceRoster.GetValue("transferwalktime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;

         // add return LOS, if valid _departureTime passed			
         if (_returnTime > 0) {
           firstAndHiddenWaitTime += ImpedanceRoster.GetValue("firstandhiddenwaittime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           waitTime += ImpedanceRoster.GetValue("waittime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           fareCard10 += ImpedanceRoster.GetValue("farecard10", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           extraFareCard10 += skimMode == Global.Settings.Modes.BikeOnTransit ? ImpedanceRoster.GetValue("farecard10", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable : 0;
           //fareCardMonth += ImpedanceRoster.GetValue("farecardmonth", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           //accessEgressTime += ImpedanceRoster.GetValue("accegrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           fTime += ImpedanceRoster.GetValue("invehtimeferry", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           gTime += ImpedanceRoster.GetValue("invehtimemetro", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           bTime += ImpedanceRoster.GetValue("invehtimebus", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           pTime += ImpedanceRoster.GetValue("invehtimelrandlocaltrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           rTime += ImpedanceRoster.GetValue("invehtimeretrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           sTime += ImpedanceRoster.GetValue("invehtimestrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           iTime += ImpedanceRoster.GetValue("invehtimeictrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           //noOfChanges += ImpedanceRoster.GetValue("noofchanges", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
           transferWalkTime = ImpedanceRoster.GetValue("transferwalktime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
         }

         double fare = transitPassOwnership == 1 ? 0.0 : fareCard10 + extraFareCard10; // assumes that marginal cost is zero for pass holders
         fare = fare * (1.0 - _hovOccupancy); //fare adjustment

         //determine utility components 
         //accessEgressTime = accessEgressTime
         double trainTime = rTime + sTime + iTime;
         double busTime = bTime + fTime;
         double metroTime = gTime;
         double lightRailTime = pTime;

         path.Time = trainTime + busTime + metroTime + lightRailTime;
         if (path.Time > pathTimeLimit) {
           path.Available = false;
           return path;
         }
         path.Cost = fare;

         //determine submode ivt time weights, and weighted IVT 
         int aggregatePurpose = 0;
         if (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Escort) {
           aggregatePurpose = 1;
         } else if (_purpose == Global.Settings.Purposes.Business) {
           aggregatePurpose = 2;
         }

         double inVehicleTimeWeight_Train;
         double inVehicleTimeWeight_Bus;
         double inVehicleTimeWeight_Metro;
         double inVehicleTimeWeight_LightRail;

         if (aggregatePurpose == 1) {
           inVehicleTimeWeight_Train = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_Train;
           inVehicleTimeWeight_Bus = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_Bus;
           inVehicleTimeWeight_Metro = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_Metro;
           inVehicleTimeWeight_LightRail = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_LightRail;
         } else if (aggregatePurpose == 2) {
           inVehicleTimeWeight_Train = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_Train;
           inVehicleTimeWeight_Bus = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_Bus;
           inVehicleTimeWeight_Metro = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_Metro;
           inVehicleTimeWeight_LightRail = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_LightRail;
         } else {
           inVehicleTimeWeight_Train = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_Train;
           inVehicleTimeWeight_Bus = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_Bus;
           inVehicleTimeWeight_Metro = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_Metro;
           inVehicleTimeWeight_LightRail = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_LightRail;
         }
         double weightedInVehicleTime = trainTime * inVehicleTimeWeight_Train
                    + busTime * inVehicleTimeWeight_Bus
                    + metroTime * inVehicleTimeWeight_Metro
                    + lightRailTime * inVehicleTimeWeight_LightRail;

         //calculate time utility

         path.Utility =
              Global.Configuration.PathImpedance_PathChoiceScaleFactor *
              (_tourCostCoefficient * GammaFunction(fare, Global.Configuration.PathImpedance_Gamma_Cost)
              + _tourTimeCoefficient *
              (Global.Configuration.PathImpedance_TransitInVehicleTimeWeight * GammaFunction(weightedInVehicleTime, Global.Configuration.PathImpedance_Gamma_InVehicleTime)
              + Global.Configuration.PathImpedance_TransitFirstWaitTimeWeight * firstAndHiddenWaitTime
              + Global.Configuration.PathImpedance_TransitTransferWaitTimeWeight * waitTime
              + Global.Configuration.PathImpedance_WalkAccessTimeWeight * transferWalkTime
           ));
         return path;
       }

       private static double GetTransitWalkTime(IParcelWrapper parcel, int pathType, double boardings) {
         double walkDist = parcel.DistanceToLocalBus; // default is local bus (feeder), for any submode

         double altDist;

         if (pathType == Global.Settings.PathTypes.LightRail) {
           altDist = parcel.DistanceToLightRail;
         } else if (pathType == Global.Settings.PathTypes.PremiumBus) {
           altDist = parcel.DistanceToExpressBus;
         } else if (pathType == Global.Settings.PathTypes.CommuterRail) {
           altDist = parcel.DistanceToCommuterRail;
         } else if (pathType == Global.Settings.PathTypes.Ferry) {
           altDist = parcel.DistanceToFerry;
         } else {
           altDist = Constants.DEFAULT_VALUE;
         }

         if ((altDist >= 0 && altDist < walkDist) || (boardings < Global.Configuration.PathImpedance_TransitSingleBoardingLimit && altDist >= 0 && altDist < Global.Configuration.PathImpedance_TransitWalkAccessDirectLimit)) {
           walkDist = altDist;
         }

         return
             (walkDist >= 0 && walkDist < Global.Configuration.PathImpedance_TransitWalkAccessDistanceLimit)
                 ? walkDist * Global.PathImpedance_WalkMinutesPerDistanceUnit
                 : Constants.DEFAULT_VALUE; // -1 is "missing" value
       }
       */
    /*
    public class AutoPath {
      public bool Available { get; set; }
      public double Time { get; set; }
      public double Distance { get; set; }
      public double Cost { get; set; }
      public double Utility { get; set; }
    }

    private PathTypeModel_Actum.AutoPath GetAutoPath(int skimModeIn, int pathType, double votValue, bool useZones, bool useAVVOT, int originZoneId, int destinationZoneId, ParcelWrapper originParcel, ParcelWrapper destinationParcel) {

      bool useAVSkims = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatrices);

      int skimMode = useAVSkims ? Global.Settings.Modes.AV
                             : (skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.HovPassenger : skimModeIn;

      AutoPath path = new PathTypeModel_Actum.AutoPath {
        Available = true
      };

      //if full network path and no-tolls path exists check for duplicate
      double tollConstant = 0D;
      if (pathType == Global.Settings.PathTypes.FullNetwork && ImpedanceRoster.IsActualCombination(skimMode, Global.Settings.PathTypes.NoTolls)) {
        double noTollCost =
                    useZones
                        ? ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _outboundTime, originZoneId, destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _outboundTime, originParcel, destinationParcel).Variable;

        if (_returnTime > 0) {
          noTollCost +=
              useZones
                  ? ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _returnTime, destinationZoneId, originZoneId).Variable
                  : ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _returnTime, destinationParcel, originParcel).Variable;
        }
        // if the toll route doesn't have a higher cost than no toll route, than make it unavailable
        if (path.Cost - noTollCost < Constants.EPSILON) {
          path.Available = false;
          return path;
        }
        // else it is a toll route with a higher cost than no toll route, add a toll constant also
        tollConstant = Global.Configuration.PathImpedance_AutoTolledPathConstant;
      }

      // calculate circuity distance for blending
      double zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId).Variable;
      //double circuityDistance =
      //          (zzDist > Global.Configuration.MaximumBlendingDistance)
      //              ? Constants.DEFAULT_VALUE
      //              : (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
      //                  ? originParcel.NodeToNodeDistance(destinationParcel)
      //                  : (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
      //                      ? originParcel.CircuityDistance(destinationParcel)
      //                      : Constants.DEFAULT_VALUE;

      double circuityDistance =
                ((zzDist > Global.Configuration.MaximumBlendingDistance) || useZones)
                    ? Constants.DEFAULT_VALUE
                    : _originParcel.CalculateShortDistance(destinationParcel);

      //calculate times and distances outbound
      SkimValue freeFlowSkimValue =
                useZones
                    ? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId)
                    : ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, originParcel, destinationParcel, circuityDistance);
      double freeFlowTime = freeFlowSkimValue.Variable;
      double distance1 =
                freeFlowSkimValue.BlendVariable;
      //				useZones
      //					? ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId).Variable
      //					: freeFlowSkimValue.BlendVariable;
      SkimValue extraSkimValue =
            useZones
                ? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId)
                : ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, originParcel, destinationParcel, circuityDistance);
      double extraTime = extraSkimValue.Variable;

      double searchMinutes = 0.0;
      if ((!useZones) && (!(skimModeIn == Global.Settings.Modes.PaidRideShare))) {
        //time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenPM, Global.Settings.Times.MinutesInADay).ToFlag()
        searchMinutes =
            _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.NinePM, Global.Settings.Times.MinutesInADay)
            ? destinationParcel.ParkingSearchTime21_05
            : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.FiveAM)
                ? destinationParcel.ParkingSearchTime21_05
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.FiveAM, Global.Settings.Times.SixAM)
                    ? destinationParcel.ParkingSearchTime05_06
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.SevenAM)
                    ? destinationParcel.ParkingSearchTime06_07
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.EightAM)
                    ? destinationParcel.ParkingSearchTime07_08
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.NineAM)
                    ? destinationParcel.ParkingSearchTime08_09
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.ThreePM)
                    ? destinationParcel.ParkingSearchTime09_15
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.FourPM)
                    ? destinationParcel.ParkingSearchTime15_16
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.FivePM)
                    ? destinationParcel.ParkingSearchTime16_17
                : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.FivePM, Global.Settings.Times.SixPM)
                    ? destinationParcel.ParkingSearchTime17_18
                : //_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.NinePM)
                  //?
                    destinationParcel.ParkingSearchTime18_21;
      }
      extraTime += searchMinutes;

      //calculate times and distances on return
      double distance2 = 0.0;
      if (_returnTime > 0) {
        freeFlowSkimValue =
            useZones
                ? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _returnTime, destinationParcel, originParcel, circuityDistance);
        freeFlowTime += freeFlowSkimValue.Variable;
        distance2 =
            freeFlowSkimValue.BlendVariable;
        //					useZones
        //						? ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _returnTime, destinationZoneId, originZoneId).Variable
        //						: freeFlowSkimValue.BlendVariable;
        extraSkimValue =
            useZones
                ? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _returnTime, destinationZoneId, originZoneId)
                : ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _returnTime, destinationParcel, originParcel, circuityDistance);
        extraTime += extraSkimValue.Variable;
      }

      path.Time = freeFlowTime + extraTime;
      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

      if (path.Time > pathTimeLimit || path.Time < Constants.EPSILON) {
        path.Available = false;
        return path;
      }

      path.Distance = distance1 + distance2;

      //calculate costs outbound
      double cost1 =
                useZones
                    ? ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId).Variable
                    : ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, originParcel, destinationParcel).Variable;
      //implement mileage-based pricing policy
      if (Global.Configuration.Policy_TestMilageBasedPricing) {
        int minutesAfterMidnight = _outboundTime + 180;
        int centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
                     ? Global.Configuration.Policy_CentsPerMileInAMPeak :
                        (minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
                          ? Global.Configuration.Policy_CentsPerMileInPMPeak :
                             (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
                                ? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
        cost1 += distance1 * centsPerMile / 100.0;
      }

      if (skimModeIn == Global.Settings.Modes.PaidRideShare) {
        double extraCostPerDistanceUnit = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                Global.Configuration.AV_PaidRideShare_ExtraCostPerDistanceUnit : Global.Configuration.PaidRideShare_ExtraCostPerDistanceUnit;
        double fixedCostPerRide = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                Global.Configuration.AV_PaidRideShare_FixedCostPerRide : Global.Configuration.PaidRideShare_FixedCostPerRide;
        cost1 += distance1 * extraCostPerDistanceUnit + fixedCostPerRide;
      }

      //calculate costs on return
      double cost2 = 0.0;
      if (_returnTime > 0) {
        cost2 +=
            useZones
                ? ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, destinationZoneId, originZoneId).Variable
                : ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, destinationParcel, originParcel).Variable;
        //implement mileage-based pricing policy
        if (Global.Configuration.Policy_TestMilageBasedPricing) {
          int minutesAfterMidnight = _returnTime + 180;
          int centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
                         ? Global.Configuration.Policy_CentsPerMileInAMPeak :
                            (minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
                              ? Global.Configuration.Policy_CentsPerMileInPMPeak :
                                 (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
                                    ? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
          cost2 += distance2 * centsPerMile / 100.0;
        }
        if (skimModeIn == Global.Settings.Modes.PaidRideShare) {
          double extraCostPerDistanceUnit = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                    Global.Configuration.AV_PaidRideShare_ExtraCostPerDistanceUnit : Global.Configuration.PaidRideShare_ExtraCostPerDistanceUnit;
          double fixedCostPerRide = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                    Global.Configuration.AV_PaidRideShare_FixedCostPerRide : Global.Configuration.PaidRideShare_FixedCostPerRide;
          cost2 += distance2 * extraCostPerDistanceUnit + fixedCostPerRide;
        }
      }
      path.Cost = cost1 + cost2;
      if (!(skimModeIn == Global.Settings.Modes.PaidRideShare)) {
        path.Cost = cost1 + cost2 + path.Distance * Global.PathImpedance_AutoOperatingCostPerDistanceUnit;
      }
      //calculate time utility
      double gammaFreeFlowTime = GammaFunction(freeFlowTime, Global.Configuration.PathImpedance_Gamma_InVehicleTime);
      extraTime = Global.Configuration.Policy_CongestedTravelTimeMultiplier != 0 ? extraTime * Global.Configuration.Policy_CongestedTravelTimeMultiplier : extraTime;
      double gammaExtraTime = GammaFunction(extraTime, Global.Configuration.PathImpedance_Gamma_ExtraTime);
      //determine time weight
      //extra time weight for driver and passenger
      double inVehicleExtraTimeWeight;
      if (skimModeIn == Global.Settings.Modes.HovPassenger) {  //JLB 20180130 change from SkimMode so that PaidRideShare is not treated like HOVPassenger
        inVehicleExtraTimeWeight = Global.Configuration.PathImpedance_InVehicleExtraTimeWeight_Passenger;
      } else {
        inVehicleExtraTimeWeight = Global.Configuration.PathImpedance_InVehicleExtraTimeWeight_Driver;
      }
      //weights for purpose x mode
      int aggregatePurpose = 0;
      double inVehicleTimeWeight;
      if (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Escort) {
        aggregatePurpose = 1;
      } else if (_purpose == Global.Settings.Purposes.Business) {
        aggregatePurpose = 2;
      }
      if (skimModeIn == Global.Settings.Modes.Sov) {  //JLB 20180130 change from SkimMode so that AV is not necessarily treated like HOVPassenger
        if (aggregatePurpose == 1) {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_SOV;
        } else if (aggregatePurpose == 2) {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_SOV;
        } else {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_SOV;
        }
      } else if (skimModeIn == Global.Settings.Modes.HovDriver) { //JLB 20180130 change from SkimMode so that AV is not necessarily treated like HOVPassenger
        if (aggregatePurpose == 1) {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_HOVDriver;
        } else if (aggregatePurpose == 2) {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_HOVDriver;
        } else {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_HOVDriver;
        }
      } else {
        if (aggregatePurpose == 1) {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_HOVPassenger;
        } else if (aggregatePurpose == 2) {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_HOVPassenger;
        } else {
          inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_HOVPassenger;
        }
      }

      //calculate cost utility
      double gammaCost;
      if (skimModeIn == Global.Settings.Modes.HovPassenger && !Global.Configuration.HOVPassengersIncurCosts) { //JLB 20180130 change from SkimMode so that PaidRideShare is not treated like HOVPassenger
        gammaCost = 0;
      } else if (skimModeIn == Global.Settings.Modes.PaidRideShare) {
        gammaCost = GammaFunction(path.Cost, Global.Configuration.PaidRideShare_PathImpedance_Gamma_Cost);
      } else {
        gammaCost = GammaFunction(path.Cost, Global.Configuration.PathImpedance_Gamma_Cost);
      }

      double autoTimeCoefficient = useAVVOT
                 ? _tourTimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : _tourTimeCoefficient;


      //combine time and cost utility
      path.Utility = Global.Configuration.PathImpedance_PathChoiceScaleFactor
          * (_tourCostCoefficient * gammaCost
          + autoTimeCoefficient * inVehicleTimeWeight
          * (gammaFreeFlowTime + gammaExtraTime * inVehicleExtraTimeWeight)
          + tollConstant);

      return path;

    }
    */
    /*
    //private void RunStopAreaCarParkRideBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
    //  if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
    //    return;
    //  }
    //  List<ParkAndRideNodeWrapper> carParkAndRideNodes = _parkAndRideAutoNodesWithCapacity;
    //  List<ParkAndRideNodeWrapper> bikeParkAndRideNodes = _parkAndRideBikeNodesWithCapacity;

    //  // valid node(s), and tour-level call  
    //  double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
    //  double bestPathUtility = -99999D;
    //  int originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
    //  int destinationZoneId = useZones ? _destinationZoneId : _destinationParcel.ZoneId;
    //  double zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, destinationZoneId).Variable;


    //  //user-set limits on search - use high values if not set
    //  int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
    //  int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
    //  int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
    //  double maxDistanceUnitsToDrive = (Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide > 0) ? Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide : 999D;
    //  double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

    //  //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
    //  int dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
    //  int dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);

    //  if (dFirst <= 0) {
    //    return;
    //  }
    //  foreach (ParkAndRideNodeWrapper carParkAndRideNode in carParkAndRideNodes) {

    //    // use the nearest stop area for transit LOS  
    //    ParcelWrapper parkAndRideParcel = (ParcelWrapper)ChoiceModelFactory.Parcels[carParkAndRideNode.NearestParcelId];
    //    int parkAndRideZoneId = carParkAndRideNode.ZoneId;
    //    int parkAndRideStopAreaKey = carParkAndRideNode.NearestStopAreaId;
    //    int parkAndRideStopArea = Global.TransitStopAreaMapping[carParkAndRideNode.NearestStopAreaId];
    //    //JLB 201508 logic in followign line replaced with new cost logic further below
    //    double carParkAndRideParkingCost = 0.0;   //node.Cost / 100.0;    // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

    //    //test distance to park and ride against user-set limits
    //    double zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

    //    if (zzDist > maxDistanceUnitsToDrive) {
    //      continue;
    //    }

    //    if (zzDist / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
    //      continue;
    //    }
    //    double duration = 0.0;
    //    if (carParkAndRideNode.ParkingTypeId == 2 || carParkAndRideNode.ParkingTypeId == 3) { // TODO:  should make list of settings or constants for node.ParkingTypeIs, as is done for e.g. Purposes
    //      if (_returnTime <= 0) {
    //        if (_purpose == Global.Settings.Purposes.Work) {
    //          duration = 8.0;
    //        } else if (_purpose == Global.Settings.Purposes.School) {
    //          duration = 6.0;
    //        } else if (_purpose == Global.Settings.Purposes.Social) {
    //          duration = 3.0;
    //        } else {
    //          duration = 2.0;
    //        }
    //      } else {
    //        duration = (_returnTime - _outboundTime) / 60.0;
    //      }
    //    }
    //    if (carParkAndRideNode.ParkingTypeId == 3 && duration > 2.0 + Constants.EPSILON) {
    //      continue;      // parking duration limited to 2 hours for TypeId 3 lot so this lot is not available
    //    } else {
    //      carParkAndRideParkingCost =
    //          (carParkAndRideNode.ParkingTypeId == 1 || carParkAndRideNode.ParkingTypeId == 3)
    //              ? 0.0
    //              : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ElevenPM, Global.Settings.Times.MinutesInADay)
    //                  ? carParkAndRideNode.CostPerHour23_08 * duration
    //                  : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.EightAM)
    //                      ? carParkAndRideNode.CostPerHour23_08 * duration
    //                      : _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.SixPM)
    //                          ? carParkAndRideNode.CostPerHour08_18 * duration
    //                          : carParkAndRideNode.CostPerHour18_23 * duration;
    //    }

    //    double oStationWalkDistance = 2.0 * carParkAndRideNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits
    //    double oStationWalkTime = oStationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

    //    bool useAVVOT = false;
    //    AutoPath autoPath = GetAutoPath(Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, useZones, useAVVOT, originZoneId, parkAndRideZoneId, _originParcel, parkAndRideParcel);

    //    //loop on bike parking nodes near destination
    //    for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
    //      if (dIndex < 0) {
    //        continue;
    //      }
    //      float dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
    //      if (dBikeLength > maxStopAreaBikeLength) {
    //        break;
    //      }
    //      if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
    //        dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
    //        continue;
    //      }
    //      double dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
    //      if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
    //        break;
    //      }
    //      dBikeDistance *= 2;  //round trip
    //      int dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
    //      int dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
    //      double dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
    //      //loop on stop areas associated with the dnode
    //      foreach (ParkAndRideNodeWrapper bikeParkAndRideNodeWrapper in bikeParkAndRideNodes) {
    //        if (bikeParkAndRideNodeWrapper.ZoneId != dStopAreaNodeId) {
    //          continue;
    //        }
    //        ParkAndRideNodeWrapper dStopAreaNode = bikeParkAndRideNodeWrapper;
    //        int dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];

    //        TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dParkAndRideStopArea, _transitPassOwnership);
    //        if (!transitPath.Available) {
    //          continue;
    //        }

    //        int parkMinute = (int)(_outboundTime - (transitPath.Time / 2.0) - 3); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.
    //        int dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
    //        double dParkAndRideParkingCost = Math.Min(dStopAreaNode.Cost, dStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost

    //        double dStationWalkDistance = 2.0 * dStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
    //        double dStationWalkTime = dStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

    //        // set utility  
    //        double pathTime = transitPath.Time + autoPath.Time + oStationWalkTime + dStationWalkTime + dBikeTime;
    //        double pathDistance = autoPath.Distance + oStationWalkDistance + transitPath.Distance + dStationWalkDistance + dBikeDistance;
    //        double pathCost = transitPath.Cost + autoPath.Cost + carParkAndRideParkingCost + dParkAndRideParkingCost;

    //        if (pathTime > pathTimeLimit) {
    //          continue;
    //        }

    //        double pathUtility = transitPath.Utility + autoPath.Utility +
    //                        Global.Configuration.PathImpedance_PathChoiceScaleFactor *
    //                        (_tourCostCoefficient *
    //                            (carParkAndRideParkingCost + dParkAndRideParkingCost) +
    //                        _tourTimeCoefficient *
    //                            (Global.Configuration.PathImpedance_WalkAccessTimeWeight * (oStationWalkTime + dStationWalkTime)
    //                            + Global.Configuration.PathImpedance_BikeAccessTimeWeight * dBikeTime
    //                            )
    //                        );

    //        if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
    //          pathUtility += carParkAndRideNode.ShadowPrice[parkMinute];
    //        }

    //        // if the best path so far, reset pathType properties
    //        if (pathUtility <= bestPathUtility) {
    //          continue;
    //        }
    //        bestPathUtility = pathUtility;
    //        _pathParkAndRideNodeId[pathType] = carParkAndRideNode.Id;
    //        _pathOriginStopAreaKey[pathType] = parkAndRideStopAreaKey;
    //        _pathDestinationStopAreaKey[pathType] = dStopAreaNodeKey;
    //        _pathTime[pathType] = pathTime;
    //        _pathDistance[pathType] = pathDistance;
    //        _pathCost[pathType] = pathCost;
    //        _pathTransitTime[pathType] = transitPath.Time;
    //        _pathTransitDistance[pathType] = transitPath.Distance;
    //        _pathTransitCost[pathType] = transitPath.Cost;
    //        _pathTransitUtility[pathType] = transitPath.Utility;
    //        _pathWalkTime[pathType] = oStationWalkTime + dStationWalkTime;
    //        _pathWalkDistance[pathType] = oStationWalkDistance + dStationWalkDistance;
    //        _pathBikeTime[pathType] = dBikeTime;
    //        _pathBikeDistance[pathType] = dBikeDistance;
    //        _pathBikeCost[pathType] = dParkAndRideParkingCost;
    //        _utility[pathType] = pathUtility;
    //        _expUtility[pathType] = pathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : pathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(pathUtility);
    //        _pathOriginAccessMode[pathType] = Global.Settings.Modes.Sov;
    //        _pathOriginAccessTime[pathType] = autoPath.Time;
    //        _pathOriginAccessDistance[pathType] = autoPath.Distance;
    //        _pathOriginAccessCost[pathType] = autoPath.Cost;
    //        _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Bike;
    //        _pathDestinationAccessTime[pathType] = dBikeTime;
    //        _pathDestinationAccessDistance[pathType] = dBikeDistance;
    //        _pathDestinationAccessCost[pathType] = 0.0;

    //        //}
    //      }
    //    }
    //  }
    }
  */
    /*
    private void RunStopAreaBikeParkRideWalkModel(int skimMode, int pathType, double votValue, bool useZones) {
     if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
      return;
    }
    List<ParkAndRideNodeWrapper> bikeParkAndRideNodes = _parkAndRideBikeNodesWithCapacity;
    // valid node(s), and tour-level call  
    double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
    double bestPathUtility = -99999D;
    //user-set limits on search - use high values if not set
    int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
    int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
    int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
    int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide : 99999;
    double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
    //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
    int dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
    int dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);
    if (dFirst <= 0) {
      return;
    }

    //_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
    int oFirst = _originParcel.FirstPositionInParkAndRideNodeDistanceArray;
    int oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oFirst + maxStopAreasToSearch - 1);
    if (oFirst <= 0) {
      return;
    }
    // set parcel indexes for origin and destination parcels for getting microzone skim values
    int originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
    int destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
    double zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

    //loop on bike parking nodes near origin
    for (int oIndex = oFirst; oIndex <= oLast; oIndex++) {
      float oBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[oIndex];
      if (oBikeLength > maxStopAreaBikeLength) {
        break;
      }
      if (oBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
        oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oLast + 1);
        continue;
      }
      double oBikeDistance = oBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
      if (oBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
        break;
      }
      oBikeDistance *= 2;  //round trip
      int oStopAreaNodeId = Global.ParcelParkAndRideNodeIds[oIndex];
      double oBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * oBikeDistance;
      //loop on stop areas associated with the onode
      foreach (ParkAndRideNodeWrapper parkAndRideNodeWrapper in bikeParkAndRideNodes) {
        if (parkAndRideNodeWrapper.ZoneId != oStopAreaNodeId) {
          continue;
        }
        ParkAndRideNodeWrapper oStopAreaNode = parkAndRideNodeWrapper;
        int oParkAndRideStopArea = Global.TransitStopAreaMapping[oStopAreaNode.NearestStopAreaId];
        int oParkAndRideStopAreaKey = oStopAreaNode.NearestStopAreaId;
        double oParkAndRideParkingCost = Math.Min(oStopAreaNode.Cost, oStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
        double oStationWalkDistance = 2.0 * oStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits
        double oStationWalkTime = oStationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

        //loop on stop areas near destination
        for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
          int dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
          int dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
          float dWalkLength = Global.ParcelStopAreaLengths[dIndex];
          if (dWalkLength > maxStopAreaLength) {
            break;
          }

          double destinationWalkDistance = 2 * dWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
          double destinationWalkTime = destinationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;
          TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oParkAndRideStopArea, dStopArea, _transitPassOwnership);
          if (!transitPath.Available) {
            continue;
          }

          // set utility  
          double pathTime = oBikeTime + oStationWalkTime + transitPath.Time + destinationWalkTime;
          double pathDistance = oBikeDistance + oStationWalkDistance + transitPath.Distance + destinationWalkDistance;
          double pathCost = oParkAndRideParkingCost + transitPath.Cost;
          if (pathTime > pathTimeLimit) {
            continue;
          }

          double pathUtility = transitPath.Utility +
                          Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                          (_tourCostCoefficient *
                              (oParkAndRideParkingCost) +
                          _tourTimeCoefficient *
                              (Global.Configuration.PathImpedance_WalkAccessTimeWeight * (oStationWalkTime + destinationWalkTime)
                              + Global.Configuration.PathImpedance_BikeAccessTimeWeight * (oBikeTime)
                              )
                          );

          //if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
          //	pathUtility += node.ShadowPrice[dParkMinute];
          //}

          // if the best path so far, reset pathType properties
          if (pathUtility <= bestPathUtility) {
            continue;
          }

          bestPathUtility = pathUtility;

          _pathParkAndRideNodeId[pathType] = oStopAreaNode.Id;
          _pathOriginStopAreaKey[pathType] = oParkAndRideStopAreaKey;
          _pathDestinationStopAreaKey[pathType] = dStopAreaKey;
          _pathTime[pathType] = pathTime;
          _pathDistance[pathType] = pathDistance;
          _pathCost[pathType] = pathCost;
          _pathTransitTime[pathType] = transitPath.Time;
          _pathTransitDistance[pathType] = transitPath.Distance;
          _pathTransitCost[pathType] = transitPath.Cost;
          _pathTransitUtility[pathType] = transitPath.Utility;
          _pathWalkTime[pathType] = oStationWalkTime + destinationWalkTime;
          _pathWalkDistance[pathType] = oStationWalkDistance + destinationWalkDistance;
          _pathBikeTime[pathType] = oBikeTime;
          _pathBikeDistance[pathType] = oBikeDistance;
          _pathBikeCost[pathType] = oParkAndRideParkingCost;
          _utility[pathType] = pathUtility;
          _expUtility[pathType] = pathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : pathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(pathUtility);
          _pathOriginAccessMode[pathType] = Global.Settings.Modes.Bike;
          _pathOriginAccessTime[pathType] = oBikeTime;
          _pathOriginAccessDistance[pathType] = oBikeDistance;
          _pathOriginAccessCost[pathType] = 0.0;
          _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Walk;
          _pathDestinationAccessTime[pathType] = destinationWalkTime;
          _pathDestinationAccessDistance[pathType] = destinationWalkDistance;
          _pathDestinationAccessCost[pathType] = 0.0;
        }
      }
    }
  */


    /*
  private void RunStopAreaBikeParkRideBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
     return;
      if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
       return;
     }
     List<ParkAndRideNodeWrapper> bikeParkAndRideNodes = _parkAndRideBikeNodesWithCapacity;      // valid node(s), and tour-level call  
     double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
     double bestPathUtility = -99999D;
     //user-set limits on search - use high values if not set
     int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
     int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
     int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
     double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
     //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
     int dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
     int dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);
     if (dFirst <= 0) {
       return;
     }

     //_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
     int oFirst = _originParcel.FirstPositionInParkAndRideNodeDistanceArray;
     int oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oFirst + maxStopAreasToSearch - 1);
     if (oFirst <= 0) {
       return;
     }
     // set parcel indexes for origin and destination parcels for getting microzone skim values
     int originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
     int destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
     double zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

     //loop on bike parking nodes near origin
     for (int oIndex = oFirst; oIndex <= oLast; oIndex++) {
       float oBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[oIndex];
       if (oBikeLength > maxStopAreaBikeLength) {
         break;
       }
       if (oBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
         oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oLast + 1);
         continue;
       }
       double oBikeDistance = oBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
       if (oBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
         break;
       }
       oBikeDistance *= 2;  //round trip
       int oStopAreaNodeId = Global.ParcelParkAndRideNodeIds[oIndex];
       double oBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * oBikeDistance;
       //loop on stop areas associated with the onode
       foreach (ParkAndRideNodeWrapper parkAndRideNodeWrapper in bikeParkAndRideNodes) {
         if (parkAndRideNodeWrapper.ZoneId != oStopAreaNodeId) {
           continue;
         }
         ParkAndRideNodeWrapper oStopAreaNode = parkAndRideNodeWrapper;
         int oParkAndRideStopArea = Global.TransitStopAreaMapping[oStopAreaNode.NearestStopAreaId];
         int oParkAndRideStopAreaKey = oStopAreaNode.NearestStopAreaId;
         double oParkAndRideParkingCost = Math.Min(oStopAreaNode.Cost, oStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
         double oStationWalkDistance = 2.0 * oStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
         double oStationWalkTime = oStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

         //loop on bike parking nodes near destination
         for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
           float dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
           if (dBikeLength > maxStopAreaBikeLength) {
             break;
           }
           if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
             dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
             continue;
           }
           double dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
           if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
             break;
           }
           dBikeDistance *= 2;  //round trip
           int dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
           int dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
           double dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
           //loop on stop areas associated with the dnode
           foreach (ParkAndRideNodeWrapper destinationParkAndRideNodeWrapper in bikeParkAndRideNodes) {
             if (destinationParkAndRideNodeWrapper.ZoneId != dStopAreaNodeId) {
               continue;
             }
             ParkAndRideNodeWrapper dStopAreaNode = destinationParkAndRideNodeWrapper;
             int dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];
             TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oParkAndRideStopArea, dParkAndRideStopArea, _transitPassOwnership);
             if (!transitPath.Available) {
               continue;
             }
             int dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
             double dParkAndRideParkingCost = Math.Min(dStopAreaNode.Cost, dStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
             double dStationWalkDistance = 2.0 * dStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
             double dStationWalkTime = dStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

             // set utility  
             double pathTime = oBikeTime + oStationWalkTime + transitPath.Time + dStationWalkTime + dBikeTime;
             double pathDistance = oBikeDistance + oStationWalkDistance + transitPath.Distance + dStationWalkDistance + dBikeDistance;
             double pathCost = oParkAndRideParkingCost + transitPath.Cost + dParkAndRideParkingCost;
             if (pathTime > pathTimeLimit) {
               continue;
             }

             double pathUtility = transitPath.Utility +
                               Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                               (_tourCostCoefficient *
                                   (oParkAndRideParkingCost + dParkAndRideParkingCost) +
                               _tourTimeCoefficient *
                                   (Global.Configuration.PathImpedance_WalkAccessTimeWeight * (oStationWalkTime + dStationWalkTime)
                                   + Global.Configuration.PathImpedance_BikeAccessTimeWeight * (oBikeTime + dBikeTime)
                                   )
                               );

             //if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
             //	pathUtility += node.ShadowPrice[dParkMinute];
             //}

             // if the best path so far, reset pathType properties
             if (pathUtility <= bestPathUtility) {
               continue;
             }

             bestPathUtility = pathUtility;

             _pathParkAndRideNodeId[pathType] = oStopAreaNode.Id;
             _pathOriginStopAreaKey[pathType] = oParkAndRideStopAreaKey;
             _pathDestinationStopAreaKey[pathType] = dStopAreaNodeKey;
             _pathTime[pathType] = pathTime;
             _pathDistance[pathType] = pathDistance;
             _pathCost[pathType] = pathCost;
             _pathTransitTime[pathType] = transitPath.Time;
             _pathTransitDistance[pathType] = transitPath.Distance;
             _pathTransitCost[pathType] = transitPath.Cost;
             _pathTransitUtility[pathType] = transitPath.Utility;
             _pathWalkTime[pathType] = oStationWalkTime + dStationWalkTime;
             _pathWalkDistance[pathType] = oStationWalkDistance + dStationWalkDistance;
             _pathBikeTime[pathType] = oBikeTime + dBikeTime;
             _pathBikeDistance[pathType] = oBikeDistance + dBikeDistance;
             _pathBikeCost[pathType] = oParkAndRideParkingCost + dParkAndRideParkingCost;
             _utility[pathType] = pathUtility;
             _expUtility[pathType] = pathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : pathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(pathUtility);
             _pathOriginAccessMode[pathType] = Global.Settings.Modes.Bike;
             _pathOriginAccessTime[pathType] = oBikeTime;
             _pathOriginAccessDistance[pathType] = oBikeDistance;
             _pathOriginAccessCost[pathType] = 0.0;
             _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Bike;
             _pathDestinationAccessTime[pathType] = dBikeTime;
             _pathDestinationAccessDistance[pathType] = dBikeDistance;
             _pathDestinationAccessCost[pathType] = 0.0;

           }
         }
       }
     }
  }
  */

    /*
    private void RunStopAreaWalkRideBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
     if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
      return;
    }
    List<ParkAndRideNodeWrapper> bikeParkAndRideNodes = _parkAndRideBikeNodesWithCapacity;
    // valid node(s), and tour-level call  
    double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
    double bestPathUtility = -99999D;
    //user-set limits on search - use high values if not set
    int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
    int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnits > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnits : 99999;
    int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
    int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
    double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
    //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
    int dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
    int dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);
    if (dFirst <= 0) {
      return;
    }

    //_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
    int oFirst = _originParcel.FirstPositionInStopAreaDistanceArray;
    int oLast = Math.Min(_originParcel.LastPositionInStopAreaDistanceArray, oFirst + maxStopAreasToSearch - 1);
    if (oFirst <= 0) {
      return;
    }
    // set parcel indexes for origin and destination parcels for getting microzone skim values
    int originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
    int destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
    double zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

    //loop on PT stop areas near origin
    for (int oIndex = oFirst; oIndex <= oLast; oIndex++) {
      int oStopArea = Global.ParcelStopAreaStopAreaIds[oIndex];
      int oStopAreaKey = Global.ParcelStopAreaStopAreaKeys[oIndex];
      float oWalkLength = Global.ParcelStopAreaLengths[oIndex];
      if (oWalkLength > maxStopAreaLength) {
        break;
      }

      double oWalkDistance = 2 * oWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;

      double oWalkTime = Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit * oWalkDistance;

      //loop on bike parking nodes near destination
      for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
        float dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
        if (dBikeLength > maxStopAreaBikeLength) {
          break;
        }
        if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
          dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
          continue;
        }
        double dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
        if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
          break;
        }
        dBikeDistance *= 2;  //round trip
        int dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
        int dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
        double dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
        //loop on stop areas associated with the dnode
        foreach (ParkAndRideNodeWrapper parkAndRideNodeWrapper in bikeParkAndRideNodes) {
          if (parkAndRideNodeWrapper.ZoneId != dStopAreaNodeId) {
            continue;
          }
          ParkAndRideNodeWrapper dStopAreaNode = parkAndRideNodeWrapper;
          int dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];
          TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oStopArea, dParkAndRideStopArea, _transitPassOwnership);
          if (!transitPath.Available) {
            continue;
          }
          int dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
          double dParkAndRideParkingCost = Math.Min(dStopAreaNode.Cost, dStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
          double dStationWalkDistance = 2.0 * dStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
          double dStationWalkTime = dStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

          // set utility  
          double pathTime = oWalkTime + transitPath.Time + dStationWalkTime + dBikeTime;
          double pathDistance = oWalkDistance + transitPath.Distance + dStationWalkDistance + dBikeDistance;
          double pathCost = transitPath.Cost + dParkAndRideParkingCost;
          if (pathTime > pathTimeLimit) {
            continue;
          }

          double pathUtility = transitPath.Utility +
                          Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                          (_tourCostCoefficient *
                              (dParkAndRideParkingCost) +
                          _tourTimeCoefficient *
                              (Global.Configuration.PathImpedance_WalkAccessTimeWeight * (oWalkTime + dStationWalkTime)
                              + Global.Configuration.PathImpedance_BikeAccessTimeWeight * (dBikeTime)
                              )
                          );

          //if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
          //	pathUtility += node.ShadowPrice[dParkMinute];
          //}

          // if the best path so far, reset pathType properties
          if (pathUtility <= bestPathUtility) {
            continue;
          }

          bestPathUtility = pathUtility;

          _pathParkAndRideNodeId[pathType] = 0;
          _pathOriginStopAreaKey[pathType] = oStopAreaKey;
          _pathDestinationStopAreaKey[pathType] = dStopAreaNodeKey;
          _pathTime[pathType] = pathTime;
          _pathDistance[pathType] = pathDistance;
          _pathCost[pathType] = pathCost;
          _pathTransitTime[pathType] = transitPath.Time;
          _pathTransitDistance[pathType] = transitPath.Distance;
          _pathTransitCost[pathType] = transitPath.Cost;
          _pathTransitUtility[pathType] = transitPath.Utility;
          _pathWalkTime[pathType] = oWalkTime + dStationWalkTime;
          _pathWalkDistance[pathType] = oWalkDistance + dStationWalkDistance;
          _pathBikeTime[pathType] = dBikeTime;
          _pathBikeDistance[pathType] = dBikeDistance;
          _pathBikeCost[pathType] = dParkAndRideParkingCost;
          _utility[pathType] = pathUtility;
          _expUtility[pathType] = pathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : pathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(pathUtility);
          _pathOriginAccessMode[pathType] = Global.Settings.Modes.Walk;
          _pathOriginAccessTime[pathType] = oWalkTime;
          _pathOriginAccessDistance[pathType] = oWalkDistance;
          _pathOriginAccessCost[pathType] = 0.0;
          _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Bike;
          _pathDestinationAccessTime[pathType] = dBikeTime;
          _pathDestinationAccessDistance[pathType] = dBikeDistance;
          _pathDestinationAccessCost[pathType] = 0.0;
        }
      }
    }
  }
    */
    /*
    private void RunStopAreaBikeOnTransitModel(int skimMode, int pathType, double votValue, bool useZones) {
        //			var stopAreaReader =
        //				Global
        //					.ContainerDaySim
        //					.GetInstance<IPersistenceFactory<ITransitStopArea>>()
        //					.Reader;
        //			var eligibleTerminals = stopAreaReader.Where(s => s.BikeOnBoardTerminal == 1).ToDictionary(z => z.Key, z => z);
        Dictionary<int, ITransitStopAreaWrapper> eligibleTerminals = Global.TransitStopAreas.Where(s => ((TransitStopAreaWrapper)s).BikeOnBoardTerminal == 1).ToDictionary(z => z.Key, z => z);
        if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
          return;
        }
        List<ParkAndRideNodeWrapper> bikeParkAndRideNodes = _parkAndRideBikeNodesWithCapacity;
        // valid node(s), and tour-level call  
        double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
        double bestPathUtility = -99999D;
        //user-set limits on search - use high values if not set
        int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
        int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
        int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
        double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
        //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
        int dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
        int dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);
        if (dFirst <= 0) {
          return;
        }

        //_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
        int oFirst = _originParcel.FirstPositionInParkAndRideNodeDistanceArray;
        int oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oFirst + maxStopAreasToSearch - 1);
        if (oFirst <= 0) {
          return;
        }
        // set parcel indexes for origin and destination parcels for getting microzone skim values
        int originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
        int destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
        double zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

        //loop on bike parking nodes near origin
        for (int oIndex = oFirst; oIndex <= oLast; oIndex++) {
          float oBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[oIndex];
          if (oBikeLength > maxStopAreaBikeLength) {
            break;  // since nodes are in increasing distance order
          }
          if (oBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
            oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oLast + 1);
            continue;
          }
          double oBikeDistance = oBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
          if (oBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
            break;   // since nodes are in increasing distance order
          }
          oBikeDistance *= 2;  //round trip
          int oStopAreaNodeId = Global.ParcelParkAndRideNodeIds[oIndex];
          double oBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * oBikeDistance;
          //loop on stop areas associated with the onode
          foreach (ParkAndRideNodeWrapper parkAndRideNodeWrapper in bikeParkAndRideNodes) {
            if (parkAndRideNodeWrapper.ZoneId != oStopAreaNodeId) {
              continue;
            }
            ParkAndRideNodeWrapper oStopAreaNode = parkAndRideNodeWrapper;

            int oParkAndRideStopAreaKey = oStopAreaNode.NearestStopAreaId;
            if (eligibleTerminals.Values.FirstOrDefault(t => t.Key == oParkAndRideStopAreaKey) == default(ITransitStopArea)) {  // no bikeonboard terminals available at this node
              continue;
            }
            int oParkAndRideStopArea = Global.TransitStopAreaMapping[oStopAreaNode.NearestStopAreaId];

            //loop on bike parking nodes near destination
            for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
              float dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
              if (dBikeLength > maxStopAreaBikeLength) {
                break;
              }
              if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
                dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
                continue;
              }
              double dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
              if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
                break;
              }
              dBikeDistance *= 2;  //round trip
              int dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
              int dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
              double dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
              //loop on stop areas associated with the dnode
              foreach (ParkAndRideNodeWrapper destinationParkAndRideNodeWrapper in bikeParkAndRideNodes) {
                if (destinationParkAndRideNodeWrapper.ZoneId != dStopAreaNodeId) {
                  continue;
                }
                ParkAndRideNodeWrapper dStopAreaNode = destinationParkAndRideNodeWrapper;

                int dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
                if (eligibleTerminals.Values.FirstOrDefault(t => t.Key == dParkAndRideStopAreaKey) == default(ITransitStopArea)) {  // no bikeonboard terminals available at this node
                  continue;
                }
                int dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];
                TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oParkAndRideStopArea, dParkAndRideStopArea, _transitPassOwnership);
                if (!transitPath.Available) {
                  continue;
                }

                // set utility  
                double pathTime = oBikeTime + transitPath.Time + dBikeTime;
                double pathDistance = oBikeDistance + transitPath.Distance + dBikeDistance;
                double pathCost = transitPath.Cost;
                if (pathTime > pathTimeLimit) {
                  continue;
                }

                double pathUtility = transitPath.Utility +
                                  _tourTimeCoefficient *
                                      (Global.Configuration.PathImpedance_BikeAccessTimeWeight * (oBikeTime + dBikeTime)
                                      );

                //if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
                //	pathUtility += node.ShadowPrice[dParkMinute];
                //}

                // if the best path so far, reset pathType properties
                if (pathUtility <= bestPathUtility) {
                  continue;
                }

                bestPathUtility = pathUtility;

                _pathParkAndRideNodeId[pathType] = oStopAreaNode.Id;
                _pathOriginStopAreaKey[pathType] = oParkAndRideStopAreaKey;
                _pathDestinationStopAreaKey[pathType] = dStopAreaNodeKey;
                _pathTime[pathType] = pathTime;
                _pathDistance[pathType] = pathDistance;
                _pathCost[pathType] = pathCost;
                _pathTransitTime[pathType] = transitPath.Time;
                _pathTransitDistance[pathType] = transitPath.Distance;
                _pathTransitCost[pathType] = transitPath.Cost;
                _pathTransitUtility[pathType] = transitPath.Utility;
                _pathBikeTime[pathType] = oBikeTime + dBikeTime;
                _pathBikeDistance[pathType] = oBikeDistance + dBikeDistance;
                _utility[pathType] = pathUtility;
                _expUtility[pathType] = pathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : pathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(pathUtility);
                _pathOriginAccessMode[pathType] = Global.Settings.Modes.Bike;
                _pathOriginAccessTime[pathType] = oBikeTime;
                _pathOriginAccessDistance[pathType] = oBikeDistance;
                _pathOriginAccessCost[pathType] = 0.0;
                _pathDestinationAccessMode[pathType] = Global.Settings.Modes.Bike;
                _pathDestinationAccessTime[pathType] = dBikeTime;
                _pathDestinationAccessDistance[pathType] = dBikeDistance;
                _pathDestinationAccessCost[pathType] = 0.0;
              }
            }
          }
        }
        _pathWalkTime[pathType] = 0;
        _pathWalkDistance[pathType] = 0;
        _pathBikeCost[pathType] = 0;
      }
       */

    /*
    private double GammaFunction(double x, double gamma) {
      double xGamma;
      xGamma = gamma * x + (1 - gamma) * Math.Log(Math.Max(x, 1.0));
      return xGamma;
    }
    */
  } //end class

} //end namespace
