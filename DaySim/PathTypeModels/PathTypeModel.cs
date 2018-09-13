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
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Exceptions;
using DaySim.Framework.Roster;

namespace DaySim.PathTypeModels {
  public class PathTypeModel : IPathTypeModel {
    protected const double MAX_UTILITY = 80D;
    protected const double MIN_UTILITY = -80D;

    protected IParcelWrapper _originParcel;
    protected IParcelWrapper _destinationParcel;
    protected int _originZoneId;
    protected int _destinationZoneId;
    protected int _outboundTime;
    protected int _returnTime;
    protected int _purpose;
    protected double _tourCostCoefficient;
    protected double _tourTimeCoefficient;
    protected bool _isDrivingAge;
    protected int _householdCars;
    protected bool _carsAreAVs;
    protected double _transitDiscountFraction;
    protected bool _randomChoice;
    protected int _choice;

    // model variables
    protected readonly double[] _utility = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _expUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly int[] _pathParkAndRideNodeId = new int[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly int[] _pathDestinationParkingNodeId = new int[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly int[] _pathDestinationParkingType = new int[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathDestinationParkingCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathDestinationParkingWalkTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly int[] _pathOriginStopAreaKey = new int[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly int[] _pathDestinationStopAreaKey = new int[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathParkAndRideTransitTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathParkAndRideTransitDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathParkAndRideTransitCost = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathParkAndRideTransitUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathParkAndRideWalkAccessEgressTime = new double[Global.Settings.PathTypes.TotalPathTypes];
    protected readonly double[] _pathTransitWalkAccessEgressTime = new double[Global.Settings.PathTypes.TotalPathTypes];

    private void initialize(int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, int mode) {
      _outboundTime = outboundTime;
      _returnTime = returnTime;
      _purpose = purpose;
      _tourCostCoefficient = tourCostCoefficient;
      _tourTimeCoefficient = tourTimeCoefficient;
      _isDrivingAge = isDrivingAge;
      _householdCars = householdCars;
      _carsAreAVs = carsAreAVs;
      _transitDiscountFraction = transitDiscountFraction;
      _randomChoice = randomChoice;
      Mode = mode;
    }

    private void initialize(IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, int mode) {
      initialize(outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, mode);
      _originParcel = originParcel;
      _destinationParcel = destinationParcel;
    }
    private void initialize(int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, int mode) {
      initialize(outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, mode);
      _originZoneId = originZoneId;
      _destinationZoneId = destinationZoneId;
    }

    public virtual int Mode { get; set; }

    public virtual double GeneralizedTimeLogsum { get; protected set; } = Global.Settings.GeneralizedTimeUnavailable;

    public virtual double GeneralizedTimeChosen { get; protected set; } = Global.Settings.GeneralizedTimeUnavailable;

    public virtual double PathTime { get; protected set; }

    public virtual double PathDistance { get; protected set; }

    public virtual double PathCost { get; protected set; }

    public virtual int PathType { get; protected set; }

    public virtual int PathParkAndRideNodeId { get; protected set; }
    public virtual int PathDestinationParkingNodeId { get; protected set; }
    public virtual int PathDestinationParkingType { get; protected set; }
    public virtual double PathDestinationParkingCost { get; protected set; }
    public virtual double PathDestinationParkingWalkTime { get; protected set; }

    public virtual int PathOriginStopAreaKey { get; protected set; }

    public virtual int PathDestinationStopAreaKey { get; protected set; }

    public virtual double PathParkAndRideTransitTime { get; protected set; }
    public virtual double PathParkAndRideTransitDistance { get; protected set; }
    public virtual double PathParkAndRideTransitCost { get; protected set; }
    public virtual double PathParkAndRideTransitGeneralizedTime { get; protected set; }
    public virtual double PathParkAndRideWalkAccessEgressTime { get; protected set; }
    public virtual double PathTransitWalkAccessEgressTime { get; protected set; }

    public virtual bool Available { get; protected set; }

    public virtual List<IPathTypeModel> RunAllPlusParkAndRide(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice) {
      List<int> modes = new List<int>();

      for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.PaidRideShare; mode++) {
        if (mode <= Global.Settings.Modes.ParkAndRide
        || (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideShareModeIsAvailable)) {
          modes.Add(mode);
        }
      }

      return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, modes.ToArray());
    }

    public virtual List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice) {
      List<int> modes = new List<int>();

      for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.PaidRideShare; mode++) {
        if (mode <= Global.Settings.Modes.Transit
        || (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideShareModeIsAvailable)) {
          modes.Add(mode);
        }
      }

      return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, modes.ToArray());
    }

    public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes) {
      List<IPathTypeModel> list = new List<IPathTypeModel>();

      foreach (int mode in modes) {
        object[] args = new object[] { originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, transitDiscountFraction, randomChoice, mode };
        //IPathTypeModel pathTypeModel = PathTypeModelFactory.New(args);
        IPathTypeModel pathTypeModel = PathTypeModelFactory.New(new object[] { });
        ((PathTypeModel)pathTypeModel).initialize(originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, mode);
        pathTypeModel.RunModel(randomUtility, /* useZones */ false);

        list.Add(pathTypeModel);
      }

      return list;
    }


    public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes) {
      List<IPathTypeModel> list = new List<IPathTypeModel>();

      foreach (int mode in modes) {
        object[] args = new object[] { originZoneId, destinationZoneId, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, mode };
        //IPathTypeModel pathTypeModel = PathTypeModelFactory.New(args);
        IPathTypeModel pathTypeModel = PathTypeModelFactory.New(new object[] { });
        ((PathTypeModel)pathTypeModel).initialize(originZoneId, destinationZoneId, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, mode);
        pathTypeModel.RunModel(randomUtility, /* useZones */ true);

        list.Add(pathTypeModel);
      }

      return list;
    }

    protected virtual void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight) {
      //Global.PrintFile.WriteLine("Generic RegionSpecificTransitImpedanceCalculation being called so must not be overridden by CustomizationDll");
      if (Global.Configuration.PathImpedance_TransitUsePathTypeSpecificTime) {

        if (Math.Abs(pathTypeSpecificTimeWeight) > Constants.EPSILON) {
          pathTypeSpecificTime =
             pathType == Global.Settings.PathTypes.LightRail
                ? ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                : pathType == Global.Settings.PathTypes.PremiumBus
                    ? ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                    : 0D;

          if (returnTime > 0) {
            pathTypeSpecificTime +=
               pathType == Global.Settings.PathTypes.LightRail
                  ? ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                  : pathType == Global.Settings.PathTypes.PremiumBus
                      ? ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                      : 0D;
          }
        }
      }

    }   //end RegionSpecificTransitImpedanceCalculation


    public virtual void RunModel(IRandomUtility randomUtility, bool useZones = false) {
      if (Mode == Global.Settings.Modes.Hov2) {
        _tourCostCoefficient
          = _tourCostCoefficient /
            (_purpose == Global.Settings.Purposes.Work
              ? Global.Configuration.Coefficients_HOV2CostDivisor_Work
              : Global.Configuration.Coefficients_HOV2CostDivisor_Other);
      } else if (Mode == Global.Settings.Modes.Hov3) {
        _tourCostCoefficient
          = _tourCostCoefficient /
            (_purpose == Global.Settings.Purposes.Work
              ? Global.Configuration.Coefficients_HOV3CostDivisor_Work
              : Global.Configuration.Coefficients_HOV3CostDivisor_Other);
      }


      double votValue = (60.0 * _tourTimeCoefficient) / _tourCostCoefficient; // in $/hour

      int skimMode = (Mode == Global.Settings.Modes.ParkAndRide && !Global.Configuration.UseParkAndRideModeInRosterForParkAndRidePaths)
                ? Global.Settings.Modes.Transit
                : (Mode == Global.Settings.Modes.PaidRideShare && (!Global.Configuration.AV_PaidRideShareModeUsesAVs || !Global.Configuration.AV_UseSeparateAVSkimMatrices))
                ? Global.Settings.Modes.Hov3
                : Mode;
      int availablePathTypes = 0;
      double expUtilitySum = 0D;
      double bestExpUtility = 0D;
      int bestPathType = Constants.DEFAULT_VALUE;
      // loop on all relevant path types for the mode
      for (int pathType = Global.Settings.PathTypes.FullNetwork; pathType < Global.Settings.PathTypes.TotalPathTypes; pathType++) {
        _utility[pathType] = 0D;

        if (!ImpedanceRoster.IsActualCombination(skimMode, pathType)) {
          continue;
        }

        // set path type utility and impedance, depending on the mode
        if (Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Walk) {
          RunWalkBikeModel(skimMode, pathType, votValue, useZones);
        } else if (Mode == Global.Settings.Modes.Hov3 || Mode == Global.Settings.Modes.Hov2 || Mode == Global.Settings.Modes.Sov || Mode == Global.Settings.Modes.PaidRideShare) {
          if (Mode != Global.Settings.Modes.Sov || (_isDrivingAge && _householdCars > 0)) {
            if (Global.DestinationParkingNodeIsEnabled && !useZones && ChoiceModelFactory.DestinationParkingNodeDao != null
                && Math.Abs(_returnTime) > Constants.EPSILON
                && Global.Configuration.FirstZoneNumberForDestinationParkingChoice > 0
                && _destinationParcel.ZoneKey >= Global.Configuration.FirstZoneNumberForDestinationParkingChoice
                && _destinationParcel.ZoneKey <= Global.Configuration.LastZoneNumberForDestinationParkingChoice) {
              RunAutoModelWithDestinationParkingChoice(skimMode, pathType, votValue, useZones);
            } else {
              RunAutoModel(Mode, pathType, votValue, useZones);
            }
          }
        } else if (Mode == Global.Settings.Modes.Transit) {
          if (pathType >= Global.Settings.PathTypes.TransitType1_Knr) {
            // don't run model for kiss and ride or TNC to transit path types
          } else if (Global.StopAreaIsEnabled) {
            RunStopAreaWalkTransitModel(skimMode, pathType, votValue, useZones);
          } else {
            RunSimpleWalkTransitModel(skimMode, pathType, votValue, useZones);
          }
        } else if (Mode == Global.Settings.Modes.ParkAndRide) {
          if (pathType >= Global.Settings.PathTypes.TransitType1_TNC && !Global.Configuration.PaidRideShareModeIsAvailable) {
            // don't run model for TNC to transit path types if paid rideshare mode not available
          } else if (Global.StopAreaIsEnabled) {
            RunStopAreaParkAndRideModel(skimMode, pathType, votValue, useZones);
          } else {
            RunSimpleParkAndRideModel(skimMode, pathType, votValue, useZones);
          }
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
      PathDestinationParkingNodeId = _pathDestinationParkingNodeId[_choice];
      PathDestinationParkingType = _pathDestinationParkingType[_choice];
      PathDestinationParkingCost = _pathDestinationParkingCost[_choice];
      PathDestinationParkingWalkTime = _pathDestinationParkingWalkTime[_choice];
      PathParkAndRideNodeId = _pathParkAndRideNodeId[_choice];
      if (Mode == Global.Settings.Modes.Transit) {
        PathTransitWalkAccessEgressTime = _pathTransitWalkAccessEgressTime[_choice];
      }
      if (Mode == Global.Settings.Modes.ParkAndRide) {
        PathParkAndRideTransitTime = _pathParkAndRideTransitTime[_choice];
        PathParkAndRideTransitDistance = _pathParkAndRideTransitDistance[_choice];
        PathParkAndRideTransitCost = _pathParkAndRideTransitCost[_choice];
        PathParkAndRideTransitGeneralizedTime = _pathParkAndRideTransitUtility[_choice] / tourTimeCoefficient;
        PathParkAndRideWalkAccessEgressTime = _pathParkAndRideWalkAccessEgressTime[_choice];
      }
      if (Global.StopAreaIsEnabled) {
        PathOriginStopAreaKey = _pathOriginStopAreaKey[_choice];
        PathDestinationStopAreaKey = _pathDestinationStopAreaKey[_choice];
      }
    }


    protected virtual void RunWalkBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
      bool useMicrozoneSkims = !useZones &&
                                ((skimMode == Global.Settings.Modes.Walk && Global.Configuration.UseMicrozoneSkimsForWalkMode) ||
                                 (skimMode == Global.Settings.Modes.Bike && Global.Configuration.UseMicrozoneSkimsForBikeMode));

      double circuityDistance = (useZones || useMicrozoneSkims) ? Constants.DEFAULT_VALUE : GetCircuityDistance(skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel);

      SkimValue skimValue =
              useZones
                ? ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
                : useMicrozoneSkims
                ? ImpedanceRoster.GetValue("time_mz", skimMode, pathType, votValue, _outboundTime, _originParcel.Sequence, _destinationParcel.Sequence)
                : ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);

      double skimTime = skimValue.Variable;
      double skimDistance = useMicrozoneSkims ? ImpedanceRoster.GetValue("distance_mz", skimMode, pathType, votValue, _outboundTime, _originParcel.Sequence, _destinationParcel.Sequence).Variable
                         : skimValue.BlendVariable;
      _pathTime[pathType] = skimTime;
      _pathDistance[pathType] = skimDistance;
      _pathCost[pathType] = 0;
      _pathParkAndRideNodeId[pathType] = 0;

      if (_returnTime > 0) {

        skimValue =
          useZones
            ? ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId)
            : useMicrozoneSkims
            ? ImpedanceRoster.GetValue("time_mz", skimMode, pathType, votValue, _returnTime, _destinationParcel.Sequence, _originParcel.Sequence)
            : ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel, circuityDistance);

        skimTime = skimValue.Variable;
        skimDistance = useMicrozoneSkims ? ImpedanceRoster.GetValue("distance_mz", skimMode, pathType, votValue, _returnTime, _destinationParcel.Sequence, _originParcel.Sequence).Variable
                     : skimValue.BlendVariable;
        _pathTime[pathType] += skimTime;
        _pathDistance[pathType] += skimDistance;
      }


      // sacog-specific adjustment of generalized time for bike mode
      if (_pathDistance[pathType] > Constants.EPSILON && skimMode == Global.Settings.Modes.Bike && Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions) {
        double d1 =
                  Math.Abs(Global.Configuration.PathImpedance_BikeType1DistanceFractionAdditiveWeight) < Constants.EPSILON
                    ? 0D
                    : useZones
                        ? ImpedanceRoster.GetValue("class1distance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("class1distance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        double d2 =
                  Math.Abs(Global.Configuration.PathImpedance_BikeType2DistanceFractionAdditiveWeight) < Constants.EPSILON
                    ? 0D
                    : useZones
                        ? ImpedanceRoster.GetValue("class2distance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("class2distance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        double d3 =
                  Math.Abs(Global.Configuration.PathImpedance_BikeType3DistanceFractionAdditiveWeight) < Constants.EPSILON
                    ? 0D
                    : useZones
                        ? ImpedanceRoster.GetValue("baddistance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("baddistance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        double d4 = Math.Abs(Global.Configuration.PathImpedance_BikeType4DistanceFractionAdditiveWeight) < Constants.EPSILON
                        ? 0D
                        : useZones
                            ? ImpedanceRoster.GetValue("worstdistance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                            : ImpedanceRoster.GetValue("worstdistance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        if (_returnTime > 0) {
          d1 +=
            Math.Abs(Global.Configuration.PathImpedance_BikeType1DistanceFractionAdditiveWeight) < Constants.EPSILON
              ? 0D
              : useZones
                  ? ImpedanceRoster.GetValue("class1distance", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable
                  : ImpedanceRoster.GetValue("class1distance", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel).Variable;

          d2 +=
            Math.Abs(Global.Configuration.PathImpedance_BikeType2DistanceFractionAdditiveWeight) < Constants.EPSILON
              ? 0D
              : useZones
                  ? ImpedanceRoster.GetValue("class2distance", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable
                  : ImpedanceRoster.GetValue("class2distance", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel).Variable;

          d3 +=
            Math.Abs(Global.Configuration.PathImpedance_BikeType3DistanceFractionAdditiveWeight) < Constants.EPSILON
              ? 0D
              : useZones
                  ? ImpedanceRoster.GetValue("baddistance", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable
                  : ImpedanceRoster.GetValue("baddistance", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel).Variable;

          d4 +=
            Math.Abs(Global.Configuration.PathImpedance_BikeType4DistanceFractionAdditiveWeight) < Constants.EPSILON
              ? 0D
              : useZones
                  ? ImpedanceRoster.GetValue("worstdistance", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable
                  : ImpedanceRoster.GetValue("worstdistance", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel).Variable;
        }

        double adjFactor =
                  1.0
                  + d1 / _pathDistance[pathType] * Global.Configuration.PathImpedance_BikeType1DistanceFractionAdditiveWeight
                  + d2 / _pathDistance[pathType] * Global.Configuration.PathImpedance_BikeType2DistanceFractionAdditiveWeight
                  + d3 / _pathDistance[pathType] * Global.Configuration.PathImpedance_BikeType3DistanceFractionAdditiveWeight
                  + d4 / _pathDistance[pathType] * Global.Configuration.PathImpedance_BikeType4DistanceFractionAdditiveWeight;

        _pathTime[pathType] *= adjFactor;
      }

      // a fix for unconnected parcels/zones (sampling should be fixed to not sample them)
      //            if (_pathTime[pathType] < Constants.EPSILON && _pathDistance[pathType] >= Constants.EPSILON ) {
      //                _pathTime[pathType] = _pathDistance[pathType] * (skimMode == Global.Settings.Modes.Walk ? 20.0 : 6.0) ; 
      //            }

      // a fix for intra-parcels, which happen once in a great while for school
      if (!Global.Configuration.OverrideIntraParcelDefaultWalkDistance &&  //MB20180305 This code is wrong for microzones - for now putting in a switch to turn it off
          !useZones && _originParcel.Id == _destinationParcel.Id && skimMode == Global.Settings.Modes.Walk
        //JLB 20130628 added destination scale condition because ImpedanceRoster assigns time and cost values for intrazonals 
        && Global.Settings.DestinationScale != Global.Settings.DestinationScales.Zone) {
        _pathTime[pathType] = 1.0;
        _pathDistance[pathType] = 0.01 * Global.Settings.DistanceUnitsPerMile;  // JLBscale.  multiplied by distance units per mile
      }



      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

      if (_pathTime[pathType] > pathTimeLimit || _pathTime[pathType] < Constants.EPSILON) {
        return;
      }

      _utility[pathType] =
        Global.Configuration.PathImpedance_PathChoiceScaleFactor *
        (_tourTimeCoefficient * _pathTime[pathType]
         * (skimMode == Global.Settings.Modes.Walk
             ? Global.Configuration.PathImpedance_WalkTimeWeight
             : Global.Configuration.PathImpedance_BikeTimeWeight));

      _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);
    }

    protected virtual void RunAutoModel(int skimModeIn, int pathType, double votValue, bool useZones) {

      bool useAVVOT = ((skimModeIn != Global.Settings.Modes.PaidRideShare && _carsAreAVs && Global.Configuration.AV_IncludeAutoTypeChoice)
                     || (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs));

      bool useAVSkimsByOccupancy = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatricesByOccupancy);

      bool useAVSkims = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatrices);

      int skimMode = (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.Sov) ? Global.Settings.Modes.AV1 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.Hov2) ? Global.Settings.Modes.AV2 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.Hov3) ? Global.Settings.Modes.AV3 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseSOVSkims) ? Global.Settings.Modes.AV1 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseHOV3Skims) ? Global.Settings.Modes.AV3 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.AV2 :
                           (useAVSkims) ? Global.Settings.Modes.AV :
                           (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseSOVSkims) ? Global.Settings.Modes.Sov :
                           (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseHOV3Skims) ? Global.Settings.Modes.Hov3 :
                           (skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.Hov2 : skimModeIn;

      _pathCost[pathType] =
        useZones
          ? ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
          : ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

      if (_returnTime > 0) {
        _pathCost[pathType] +=
          useZones
            ? ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable
            : ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel).Variable;
      }

      //if full network path and no-tolls path exists check for duplicate
      double tollConstant = 0D;
      if (pathType == Global.Settings.PathTypes.FullNetwork && ImpedanceRoster.IsActualCombination(skimMode, Global.Settings.PathTypes.NoTolls)) {
        double noTollCost =
                  useZones
                    ? ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                    : ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        if (_returnTime > 0) {
          noTollCost +=
            useZones
              ? ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable
              : ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _returnTime, _destinationParcel, _originParcel).Variable;
        }
        // if the toll route doesn't have a higher cost than no toll route, than make it unavailable
        if (_pathCost[pathType] - noTollCost < Constants.EPSILON) {
          return;
        }
        // else it is a toll route with a higher cost than no toll route, add a toll constant also
        tollConstant = Global.Configuration.PathImpedance_AutoTolledPathConstant;
      }

      double circuityDistance = useZones ? Constants.DEFAULT_VALUE : GetCircuityDistance(skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel);

      SkimValue skimValue =
              useZones
                ? ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
                : ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);

      _pathParkAndRideNodeId[pathType] = 0;
      _pathTime[pathType] = skimValue.Variable;
      _pathDistance[pathType] = skimValue.BlendVariable;

      //implement mileage-based pricing policy
      if (Global.Configuration.Policy_TestMilageBasedPricing) {
        int minutesAfterMidnight = _outboundTime + 180;
        int centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
                   ? Global.Configuration.Policy_CentsPerMileInAMPeak :
                    (minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
                      ? Global.Configuration.Policy_CentsPerMileInPMPeak :
                       (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
                        ? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
        _pathCost[pathType] += skimValue.BlendVariable * centsPerMile / 100.0;
      }
      if (_returnTime > 0) {

        skimValue =
          useZones
            ? ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId)
            : ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel, circuityDistance);

        _pathTime[pathType] += skimValue.Variable;
        _pathDistance[pathType] += skimValue.BlendVariable;

        //implement mileage-based pricing policy
        if (Global.Configuration.Policy_TestMilageBasedPricing) {
          int minutesAfterMidnight = _returnTime + 180;
          int centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
                       ? Global.Configuration.Policy_CentsPerMileInAMPeak :
                        (minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
                          ? Global.Configuration.Policy_CentsPerMileInPMPeak :
                           (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
                            ? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
          _pathCost[pathType] += skimValue.BlendVariable * centsPerMile / 100.0;
        }
      }

      // a fix for unconnected parcels/zones (sampling should be fixed to not sample them in the first place)
      //            if (_pathTime[pathType] < Constants.EPSILON && _pathDistance[pathType] >= Constants.EPSILON ) {
      //                _pathTime[pathType] = _pathDistance[pathType] * 2.0 ;  // correct missing time with speed of 30 mph 
      //            }
      //            else if (_pathTime[pathType] < Constants.EPSILON && _pathDistance[pathType] < Constants.EPSILON ) {
      //                _pathDistance[pathType] = (Math.Abs(_originParcel.XCoordinate - _destinationParcel.XCoordinate) 
      //                                          + Math.Abs(_originParcel.YCoordinate - _destinationParcel.YCoordinate))/5280D;
      //                _pathTime[pathType] = _pathDistance[pathType] * 2.0 ;  // correct missing time with speed of 30 mph 
      //            }

      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

      if (_pathTime[pathType] > pathTimeLimit || _pathTime[pathType] < Constants.EPSILON) {
        return;
      }
      if (skimModeIn != Global.Settings.Modes.PaidRideShare) {
        _pathCost[pathType] += _pathDistance[pathType] * Global.PathImpedance_AutoOperatingCostPerDistanceUnit;
      } else {
        double extraCostPerMile = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                 Global.Configuration.AV_PaidRideShare_ExtraCostPerDistanceUnit : Global.Configuration.PaidRideShare_ExtraCostPerDistanceUnit;
        double fixedCostPerRide = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                 Global.Configuration.AV_PaidRideShare_FixedCostPerRide : Global.Configuration.PaidRideShare_FixedCostPerRide;

        _pathCost[pathType] += _pathDistance[pathType] * extraCostPerMile + fixedCostPerRide * (_returnTime > 0 ? 2 : 1);
      }

      double autoTimeCoefficient = useAVVOT
            ? _tourTimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : _tourTimeCoefficient;

      _utility[pathType] = Global.Configuration.PathImpedance_PathChoiceScaleFactor *
      (_tourCostCoefficient * _pathCost[pathType] +
        autoTimeCoefficient * _pathTime[pathType] +
       tollConstant);

      _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);
    }

    protected void RunAutoModelWithDestinationParkingChoice(int skimModeIn, int pathType, double votValue, bool useZones) {

      bool useAVVOT = ((skimModeIn != Global.Settings.Modes.PaidRideShare && _carsAreAVs && Global.Configuration.AV_IncludeAutoTypeChoice)
                     || (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs));

      bool useAVSkimsByOccupancy = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatricesByOccupancy);

      bool useAVSkims = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatrices);

      int skimMode = (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.Sov) ? Global.Settings.Modes.AV1 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.Hov2) ? Global.Settings.Modes.AV2 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.Hov3) ? Global.Settings.Modes.AV3 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseSOVSkims) ? Global.Settings.Modes.AV1 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseHOV3Skims) ? Global.Settings.Modes.AV3 :
                           (useAVSkimsByOccupancy && skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.AV2 :
                           (useAVSkims) ? Global.Settings.Modes.AV :
                           (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseSOVSkims) ? Global.Settings.Modes.Sov :
                           (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideshare_UseHOV3Skims) ? Global.Settings.Modes.Hov3 :
                           (skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.Hov2 : skimModeIn;

      IEnumerable<IDestinationParkingNodeWrapper> destinationParkingNodes = ChoiceModelFactory.DestinationParkingNodeDao.Nodes.Where(n => n.Capacity > Constants.EPSILON);

      // valid node(s), and tour-level call  
      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
      double bestNodeUtility = -99999D;
      int originZoneId = _originParcel.ZoneId;
      int destinationZoneId = _destinationParcel.ZoneId;
      double xyDistOrigDest = Math.Max(GetXYDistance(_originParcel.XCoordinate, _originParcel.YCoordinate, _destinationParcel.XCoordinate, _destinationParcel.YCoordinate), Constants.EPSILON);

      //user-set limits on search - use high values if not set
      double maxMilesToWalk = (Global.Configuration.MaximumXYDistanceToDestinationParking > 0) ? Global.Configuration.MaximumXYDistanceToDestinationParking : 1.5D;
      double maxDistanceRatio = (Global.Configuration.MaximumRatioDistanceFromParkVersusDistanceToDestination > 0) ? Global.Configuration.MaximumRatioDistanceFromParkVersusDistanceToDestination : 1.0D;

      //double zzDistOD = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

      foreach (IDestinationParkingNodeWrapper node in destinationParkingNodes) {
        // only look at nodes with positive capacity
        if (node.Capacity <= Constants.EPSILON) {
          continue;
        }

        //test XY distance to park and ride against user-set limits
        double xyDistNodeDest = GetXYDistance(node.XCoordinate, node.YCoordinate, _destinationParcel.XCoordinate, _destinationParcel.YCoordinate);
        if (xyDistNodeDest / 5280D > maxMilesToWalk || xyDistNodeDest / xyDistOrigDest > maxDistanceRatio) {
          continue;
        }


        IParcelWrapper parkingParcel = ChoiceModelFactory.Parcels[node.ParcelId];

        int arriveTime = Math.Min(_outboundTime, Math.Abs(_returnTime));
        int departTime = Math.Max(_outboundTime, Math.Abs(_returnTime));
        double parkingCost = node.SetDestinationParkingEffectivePrice(arriveTime, departTime, _purpose);

        if (parkingCost < -500) {
          continue;
        }

        double walkDistance = 2.0 * (
                     (Global.Configuration.UseShortDistanceNodeToNodeMeasures)
                   ? parkingParcel.NodeToNodeDistance(_destinationParcel)
                   : (Global.Configuration.UseShortDistanceCircuityMeasures)
                   ? parkingParcel.CircuityDistance(_destinationParcel)
                   : xyDistNodeDest);

        double walkTime = walkDistance * Global.Configuration.PathImpedance_WalkMinutesPerMile;

        double tollCost = ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, _originParcel, parkingParcel).Variable;

        if (_returnTime > 0) {
          tollCost += ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, parkingParcel, _originParcel).Variable;
        }

        //if full network path and no-tolls path exists check for duplicate
        double tollConstant = 0D;
        if (pathType == Global.Settings.PathTypes.FullNetwork && ImpedanceRoster.IsActualCombination(skimMode, Global.Settings.PathTypes.NoTolls)) {
          double noTollCost = ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _outboundTime, _originParcel, parkingParcel).Variable;

          if (_returnTime > 0) {
            noTollCost += ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _returnTime, parkingParcel, _originParcel).Variable;
          }
          // if the toll route doesn't have a higher cost than no toll route, than make it unavailable
          if (tollCost - noTollCost < Constants.EPSILON) {
            continue;
          }
          // else it is a toll route with a higher cost than no toll route, add a toll constant also
          tollConstant = Global.Configuration.PathImpedance_AutoTolledPathConstant;
        }

        double circuityDistance = useZones ? Constants.DEFAULT_VALUE : GetCircuityDistance(skimMode, pathType, votValue, _outboundTime, _originParcel, parkingParcel);

        SkimValue skimValue = ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _outboundTime, _originParcel, parkingParcel, circuityDistance);

        double driveTime = skimValue.Variable;
        double driveDistance = skimValue.BlendVariable;

        if (_returnTime > 0) {

          skimValue = ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _returnTime, parkingParcel, _originParcel, circuityDistance);

          driveTime += skimValue.Variable;
          driveDistance += skimValue.BlendVariable;
        }

        double nodePathDistance = driveDistance + walkDistance;
        double nodePathTime = driveTime + walkTime;

        if (nodePathTime > pathTimeLimit || nodePathTime < Constants.EPSILON) {
          continue;
        }
        double nodePathCost = tollCost
                                 + driveDistance * Global.PathImpedance_AutoOperatingCostPerDistanceUnit
                                 + parkingCost;

        double autoTimeCoefficient = useAVVOT
                    ? _tourTimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : _tourTimeCoefficient;

        double nodeUtility = Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                (_tourCostCoefficient * nodePathCost +
                  autoTimeCoefficient * driveTime +
                 _tourTimeCoefficient * walkTime * Global.Configuration.PathImpedance_WalkTimeWeight +
                 tollConstant);

        // if the best path so far, reset pathType properties
        if (nodeUtility <= bestNodeUtility) {
          continue;
        }

        bestNodeUtility = nodeUtility;

        _pathDestinationParkingNodeId[pathType] = node.Id;
        _pathDestinationParkingType[pathType] = node.ParkingType;
        _pathDestinationParkingCost[pathType] = parkingCost;
        _pathDestinationParkingWalkTime[pathType] = walkTime;
        _pathTime[pathType] = nodePathTime;
        _pathDistance[pathType] = nodePathDistance;
        _pathCost[pathType] = nodePathCost;
        _utility[pathType] = nodeUtility;
        _expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
      }

    }

    protected virtual void RunSimpleWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

      if (!useZones) {
        // get zones associated with parcels for transit path
        _originZoneId = _originParcel.ZoneId;
        _destinationZoneId = _destinationParcel.ZoneId;
      }

      TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, _originZoneId, _destinationZoneId);
      if (!transitPath.Available) {
        return;
      }

      double originWalkTime = useZones ? 5.0 : GetTransitWalkTime(_originParcel, pathType, transitPath.Boardings1);
      double destinationWalkTime = useZones ? 5.0 : GetTransitWalkTime(_destinationParcel, pathType, transitPath.Boardings2);

      if (originWalkTime < -1 * Constants.EPSILON || destinationWalkTime < -1 * Constants.EPSILON) {
        return;
      }

      if (_returnTime > 0) {
        originWalkTime *= 2;
        destinationWalkTime *= 2;
      }

      //set final values
      _pathParkAndRideNodeId[pathType] = 0;
      _pathTime[pathType] = transitPath.Time + originWalkTime + destinationWalkTime;
      _pathCost[pathType] = transitPath.Cost;
      _pathTransitWalkAccessEgressTime[pathType] = originWalkTime + destinationWalkTime;
      _utility[pathType] = transitPath.Utility +
   Global.Configuration.PathImpedance_PathChoiceScaleFactor * _tourTimeCoefficient *
   Global.Configuration.PathImpedance_TransitWalkAccessTimeWeight * (originWalkTime + destinationWalkTime);

      _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);

      //for transit, use auto distance
      double distance = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable;
      if (_returnTime > 0) {
        distance += ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable;
      }
      _pathDistance[pathType] = distance;
    }

    protected virtual void RunStopAreaWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

      if (useZones) {
        return;
      }
      //user-set limits on search - use high values if not set
      int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearch > 0) ? Global.Configuration.MaximumStopAreasToSearch : 99;
      int maxStopAreaDistance = (Global.Configuration.MaximumParcelToStopAreaDistance > 0) ? Global.Configuration.MaximumParcelToStopAreaDistance : 99999;

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
        float oWalkDistance = Global.ParcelStopAreaDistances[oIndex];
        if (oWalkDistance > maxStopAreaDistance) {
          continue;
        }

        for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
          int dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
          int dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
          float dWalkDistance = Global.ParcelStopAreaDistances[dIndex];
          if (dWalkDistance > maxStopAreaDistance) {
            continue;
          }

          double walkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * (oWalkDistance + dWalkDistance) * Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
          if (_returnTime > 0) {
            walkTime *= 2;
          }

          TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oStopArea, dStopArea);
          if (!transitPath.Available) {
            continue;
          }

          // set utility
          double fullPathTime = transitPath.Time + walkTime;
          double fullPathCost = transitPath.Cost;


          if (fullPathTime > pathTimeLimit) {
            continue;
          }

          double fullPathUtility = transitPath.Utility +
                    Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                     _tourTimeCoefficient * Global.Configuration.PathImpedance_TransitWalkAccessTimeWeight * walkTime;


          // if the best path so far, reset pathType properties
          if (fullPathUtility <= bestPathUtility) {
            continue;
          }

          bestPathUtility = fullPathUtility;

          _pathParkAndRideNodeId[pathType] = 0;
          _pathOriginStopAreaKey[pathType] = oStopAreaKey;
          _pathDestinationStopAreaKey[pathType] = dStopAreaKey;
          _pathTime[pathType] = fullPathTime;
          _pathCost[pathType] = fullPathCost;
          _pathTransitWalkAccessEgressTime[pathType] = walkTime;
          _utility[pathType] = fullPathUtility;
          _expUtility[pathType] = fullPathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : fullPathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(fullPathUtility);
        }
      }

      //for transit, use auto distance
      double distance = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;
      if (_returnTime > 0) {
        distance += ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, _destinationParcel, _originParcel).Variable;
      }
      _pathDistance[pathType] = distance;
    }


    protected virtual void RunSimpleParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
      if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
        return;
      }
      int threadAssignedIndex = ParallelUtility.threadLocalAssignedIndex.Value;
      IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

      bool knrPathType = (pathType >= Global.Settings.PathTypes.TransitType1_Knr && pathType <= Global.Settings.PathTypes.TransitType5_Knr);
      bool tncPathType = (pathType >= Global.Settings.PathTypes.TransitType1_TNC && pathType <= Global.Settings.PathTypes.TransitType5_TNC);

      double knrAdditiveConstant =
          !knrPathType ? 0D
          : _purpose == Global.Settings.Purposes.Work
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_1VehicleHH
            : Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_2pVehicleHH)
          : _purpose == Global.Settings.Purposes.School
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_1VehicleHH
            : Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_2pVehicleHH)
          : (!_isDrivingAge ? Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_1VehicleHH
            : Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_2pVehicleHH);

      double tncAdditiveConstant =
          !tncPathType ? 0D
          : _purpose == Global.Settings.Purposes.Work
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_1VehicleHH
            : Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_2pVehicleHH)
          : _purpose == Global.Settings.Purposes.School
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_1VehicleHH
            : Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_2pVehicleHH)
          : (!_isDrivingAge ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_1VehicleHH
            : Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_2pVehicleHH)
            // density-related effect
            + Global.Configuration.TNCtoTransit_DensityCoefficient * Math.Min(_originParcel.HouseholdsBuffer2 + _originParcel.StudentsUniversityBuffer2 + _originParcel.EmploymentTotalBuffer2, 6000);

      double driveTimeWeightFactor = knrPathType ? (Global.Configuration.PathImpedance_KNRAutoAccessTimeFactor > 0 ? Global.Configuration.PathImpedance_KNRAutoAccessTimeFactor : 2.0)
                                      : tncPathType ? (Global.Configuration.PathImpedance_TNCAutoAccessTimeFactor > 0 ? Global.Configuration.PathImpedance_TNCAutoAccessTimeFactor : 1.0)
                                                     * Global.Configuration.AV_PaidRideShareModeUsesAVs.ToFlag() * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor)
                                      : 1.0;


      double driveTimeWeight = Global.Configuration.PathImpedance_TransitDriveAccessTimeWeight * driveTimeWeightFactor;

      if (Global.Configuration.ShouldReadParkAndRideNodeSkim) {
        int nodeId =
                  useZones
                    ? (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                    : (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        IParkAndRideNodeWrapper node = ChoiceModelFactory.ParkAndRideNodeDao.Get(nodeId);

        parkAndRideNodes = new List<IParkAndRideNodeWrapper> { node };
      } else {
        parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => (n.Capacity > Constants.EPSILON || (n.Capacity == 0 && knrPathType)));
      }

      // valid node(s), and tour-level call  
      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
      double bestNodeUtility = -99999D;
      int originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
      int destinationZoneId = useZones ? _destinationZoneId : _destinationParcel.ZoneId;

      //user-set limits on search - use high values if not set
      double maxMilesToDrive = (Global.Configuration.MaximumMilesToDriveToParkAndRide > 0) ? Global.Configuration.MaximumMilesToDriveToParkAndRide : 999D;
      double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

      bool useTNCAVSkimsByOccupancy = (Global.Configuration.AV_PaidRideShareModeUsesAVs && Global.Configuration.AV_UseSeparateAVSkimMatricesByOccupancy);
      bool useTNCAVSkims = (Global.Configuration.AV_PaidRideShareModeUsesAVs && Global.Configuration.AV_UseSeparateAVSkimMatrices);
      bool useKNRAVSkimsByOccupancy = (_carsAreAVs && Global.Configuration.AV_UseSeparateAVSkimMatricesByOccupancy);
      bool useKNRAVSkims = (_carsAreAVs && Global.Configuration.AV_UseSeparateAVSkimMatrices);

      int autoMode = tncPathType ? (useTNCAVSkimsByOccupancy ? Global.Settings.Modes.AV2 : useTNCAVSkims ? Global.Settings.Modes.AV : Global.Settings.Modes.Hov2)
                   : knrPathType ? (useKNRAVSkimsByOccupancy ? Global.Settings.Modes.AV2 : useKNRAVSkims ? Global.Settings.Modes.AV : Global.Settings.Modes.Hov2)
                   : Global.Settings.Modes.Sov;

      double zzDistOD = ImpedanceRoster.GetValue("distance", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

      foreach (IParkAndRideNodeWrapper node in parkAndRideNodes) {
        // only look at nodes with positive capacity
        if (node.Capacity < Constants.EPSILON && !knrPathType && !tncPathType) {
          continue;
        }

        // use the node rather than the nearest parcel for transit LOS, becuase more accurate, and distance blending is not relevant 
        int parkAndRideZoneId = node.ZoneId;
        //test distance to park and ride against user-set limits

        double zzDistPR = ImpedanceRoster.GetValue("distance", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

        if (zzDistPR > maxMilesToDrive || zzDistPR / Math.Max(zzDistOD, 1.0) > maxDistanceRatio) {
          continue;
        }

        IParcelWrapper parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
        double parkAndRideCost = knrPathType ? 0.0
                                           : tncPathType ? (Global.Configuration.TNCtoTransit_FixedCostPerRide + zzDistPR * Global.Configuration.TNCtoTransit_ExtraCostPerDistanceUnit)
                                           : node.Cost / 100.0; // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

        TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideZoneId, destinationZoneId);
        if (!transitPath.Available) {
          continue;
        }

        double destinationWalkTime = useZones ? 5.0 : GetTransitWalkTime(_destinationParcel, pathType, transitPath.Boardings1);

        if (destinationWalkTime < -1 * Constants.EPSILON) {
          continue;
        }

        double circuityDistance =
                  (zzDistPR > Global.Configuration.MaximumBlendingDistance || useZones)
                    ? Constants.DEFAULT_VALUE
                    : _originParcel.CalculateShortDistance(parkAndRideParcel);

        SkimValue skimValue
                  = useZones
                      ? ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, parkAndRideZoneId)
                      : ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, parkAndRideParcel, circuityDistance);

        double driveTime = skimValue.Variable;
        double driveDistance = skimValue.BlendVariable;
        int parkMinute = (int)Math.Max(1, (_outboundTime - (transitPath.Time / 2.0) - 3)); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

        double transitDistance =
                  useZones
                    ? ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideZoneId, _destinationZoneId).Variable
                    : ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideParcel, _destinationParcel).Variable;

        // add return LOS

        skimValue =
          useZones
            ? ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideZoneId, _originZoneId)
            : ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideParcel, _originParcel, circuityDistance);

        driveTime += skimValue.Variable;
        driveDistance += skimValue.BlendVariable;
        transitDistance *= 2;
        destinationWalkTime *= 2;

        // set utility
        double nodePathTime = transitPath.Time + driveTime + destinationWalkTime;
        double nodePathDistance = driveDistance + transitDistance;
        double nodePathCost = transitPath.Cost + parkAndRideCost;

        if (nodePathTime > pathTimeLimit) {
          continue;
        }

        double nodeUtility = transitPath.Utility + knrAdditiveConstant + tncAdditiveConstant +
                  Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                  (_tourCostCoefficient * parkAndRideCost +
                   _tourTimeCoefficient *
                   (driveTimeWeight * driveTime +
                    Global.Configuration.PathImpedance_TransitWalkAccessTimeWeight * destinationWalkTime));

        if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !knrPathType && !tncPathType && !Global.Configuration.IsInEstimationMode) {
          nodeUtility += node.ShadowPrice[parkMinute];
        }

        // if the best path so far, reset pathType properties
        if (nodeUtility <= bestNodeUtility) {
          continue;
        }

        bestNodeUtility = nodeUtility;

        _pathParkAndRideNodeId[pathType] = node.Id;
        _pathTime[pathType] = nodePathTime;
        _pathDistance[pathType] = nodePathDistance;
        _pathCost[pathType] = nodePathCost;
        _pathParkAndRideTransitTime[pathType] = transitPath.Time;
        _pathParkAndRideTransitDistance[pathType] = transitDistance;
        _pathParkAndRideTransitCost[pathType] = transitPath.Cost;
        _pathParkAndRideTransitUtility[pathType] = transitPath.Utility;
        _pathParkAndRideWalkAccessEgressTime[pathType] = destinationWalkTime;
        _utility[pathType] = nodeUtility;
        _expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
      }

    }

    protected virtual void RunStopAreaParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
      if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
        return;
      }
      int threadAssignedIndex = ParallelUtility.threadLocalAssignedIndex.Value;
      IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

      bool knrPathType = (pathType >= Global.Settings.PathTypes.TransitType1_Knr && pathType <= Global.Settings.PathTypes.TransitType5_Knr);
      bool tncPathType = (pathType >= Global.Settings.PathTypes.TransitType1_TNC && pathType <= Global.Settings.PathTypes.TransitType5_TNC);

      double knrAdditiveConstant =
          !knrPathType ? 0D
          : _purpose == Global.Settings.Purposes.Work
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_1VehicleHH
            : Global.Configuration.PathImpedance_KNRAdditiveConstant_WorkTour_2pVehicleHH)
          : _purpose == Global.Settings.Purposes.School
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_1VehicleHH
            : Global.Configuration.PathImpedance_KNRAdditiveConstant_SchoolTour_2pVehicleHH)
          : (!_isDrivingAge ? Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_1VehicleHH
            : Global.Configuration.PathImpedance_KNRAdditiveConstant_OtherTour_2pVehicleHH);


      double tncAdditiveConstant =
          !tncPathType ? 0D
          : _purpose == Global.Settings.Purposes.Work
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_1VehicleHH
            : Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_WorkTour_2pVehicleHH)
          : _purpose == Global.Settings.Purposes.School
          ? (!_isDrivingAge ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_1VehicleHH
            : Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_SchoolTour_2pVehicleHH)
          : (!_isDrivingAge ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_NonDriver
            : _householdCars == 0 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_0VehicleHH
            : _householdCars == 1 ? Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_1VehicleHH
            : Global.Configuration.PathImpedance_TNCtoTransitAdditiveConstant_OtherTour_2pVehicleHH)
            // density-related effect
            // density-related effect
            + Global.Configuration.TNCtoTransit_DensityCoefficient * Math.Min(_originParcel.HouseholdsBuffer2 + _originParcel.StudentsUniversityBuffer2 + _originParcel.EmploymentTotalBuffer2, 6000);

      double driveTimeWeightFactor = knrPathType ? (Global.Configuration.PathImpedance_KNRAutoAccessTimeFactor > 0 ? Global.Configuration.PathImpedance_KNRAutoAccessTimeFactor : 2.0)
                                      : tncPathType ? (Global.Configuration.PathImpedance_TNCAutoAccessTimeFactor > 0 ? Global.Configuration.PathImpedance_TNCAutoAccessTimeFactor : 1.0)
                                                     * Global.Configuration.AV_PaidRideShareModeUsesAVs.ToFlag() * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor)
                                      : 1.0;


      double driveTimeWeight = Global.Configuration.PathImpedance_TransitDriveAccessTimeWeight * driveTimeWeightFactor;


      if (Global.Configuration.ShouldReadParkAndRideNodeSkim) {
        int nodeId =
                  useZones
                    ? (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                    : (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

        IParkAndRideNodeWrapper node = ChoiceModelFactory.ParkAndRideNodeDao.Get(nodeId);

        parkAndRideNodes = new List<IParkAndRideNodeWrapper> { node };
      } else {
        parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => (n.Capacity > Constants.EPSILON || (n.Capacity == 0 && knrPathType) || (n.Capacity == 0 && tncPathType)));
      }

      // valid node(s), and tour-level call  
      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
      double bestNodeUtility = -99999D;
      int originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;

      //user-set limits on search - use high values if not set
      int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
      int maxStopAreaDistance = (Global.Configuration.MaximumParcelToStopAreaDistanceParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaDistanceParkAndRide : 99999;
      double maxMilesToDrive = (Global.Configuration.MaximumMilesToDriveToParkAndRide > 0) ? Global.Configuration.MaximumMilesToDriveToParkAndRide : 999D;
      double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

      //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
      int dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
      int dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

      if (dFirst <= 0) {
        return;
      }

      bool useTNCAVSkimsByOccupancy = (Global.Configuration.AV_PaidRideShareModeUsesAVs && Global.Configuration.AV_UseSeparateAVSkimMatricesByOccupancy);
      bool useTNCAVSkims = (Global.Configuration.AV_PaidRideShareModeUsesAVs && Global.Configuration.AV_UseSeparateAVSkimMatrices);
      bool useKNRAVSkimsByOccupancy = (_carsAreAVs && Global.Configuration.AV_UseSeparateAVSkimMatricesByOccupancy);
      bool useKNRAVSkims = (_carsAreAVs && Global.Configuration.AV_UseSeparateAVSkimMatrices);

      int autoMode = tncPathType ? (useTNCAVSkimsByOccupancy ? Global.Settings.Modes.AV2 : useTNCAVSkims ? Global.Settings.Modes.AV : Global.Settings.Modes.Hov2)
                   : knrPathType ? (useKNRAVSkimsByOccupancy ? Global.Settings.Modes.AV2 : useKNRAVSkims ? Global.Settings.Modes.AV : Global.Settings.Modes.Hov2)
                   : Global.Settings.Modes.Sov;

      double zzDistOD = ImpedanceRoster.GetValue("distance", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

      foreach (IParkAndRideNodeWrapper node in parkAndRideNodes) {
        // only look at nodes with positive capacity
        if (node.Capacity < Constants.EPSILON && !knrPathType) {
          continue;
        }

        int parkAndRideZoneId = node.ZoneId;
        //test distance to park and ride against user-set limits
        double zzDistPR = ImpedanceRoster.GetValue("distance", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

        if (zzDistPR > maxMilesToDrive || zzDistPR / Math.Max(zzDistOD, 1.0) > maxDistanceRatio) {
          continue;
        }

        // use the nearest stop area for transit LOS  
        IParcelWrapper parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
        int parkAndRideStopAreaKey = node.NearestStopAreaId;
        int parkAndRideStopArea = Global.TransitStopAreaMapping[node.NearestStopAreaId];
        double parkAndRideCost = knrPathType ? 0.0
                                           : tncPathType ? (Global.Configuration.TNCtoTransit_FixedCostPerRide + zzDistPR * Global.Configuration.TNCtoTransit_ExtraCostPerDistanceUnit)
                                           : node.Cost / 100.0; // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars


        double circuityDistance =
                    (zzDistPR > Global.Configuration.MaximumBlendingDistance || useZones)
                    ? Constants.DEFAULT_VALUE
                    : _originParcel.CalculateShortDistance(parkAndRideParcel);

        SkimValue skimValue
                    = useZones
                      ? ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, parkAndRideZoneId)
                      : ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, parkAndRideParcel, circuityDistance);

        double driveDistance = skimValue.BlendVariable;
        double driveTime = skimValue.Variable;

        double transitDistance =
                    useZones
                    ? ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideZoneId, _destinationZoneId).Variable
                    : ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideParcel, _destinationParcel).Variable;

        // add return los
        skimValue =
            useZones
            ? ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideZoneId, _originZoneId)
            : ImpedanceRoster.GetValue("ivtime", autoMode, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideParcel, _originParcel, circuityDistance);

        driveTime += skimValue.Variable;
        driveDistance += skimValue.BlendVariable;
        transitDistance *= 2;

        //loop on stop areas near destination
        for (int dIndex = dFirst; dIndex <= dLast; dIndex++) {
          int dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
          int dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
          float dWalkDistance = Global.ParcelStopAreaDistances[dIndex];
          if (dWalkDistance > maxStopAreaDistance) {
            continue;
          }

          double destinationWalkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * dWalkDistance * Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
          destinationWalkTime *= 2; //round trip

          TransitPath transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dStopArea);
          if (!transitPath.Available) {
            continue;
          }

          int parkMinute = (int)Math.Max(1, (_outboundTime - (transitPath.Time / 2.0) - 3)); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

          // set utility
          double nodePathTime = transitPath.Time + driveTime + destinationWalkTime;
          double nodePathDistance = driveDistance + transitDistance;
          double nodePathCost = transitPath.Cost + parkAndRideCost;

          if (nodePathTime > pathTimeLimit) {
            continue;
          }

          double nodeUtility = transitPath.Utility + knrAdditiveConstant + tncAdditiveConstant +
                      Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                      (_tourCostCoefficient * parkAndRideCost +
                      _tourTimeCoefficient *
                      (driveTimeWeight * driveTime +
                       Global.Configuration.PathImpedance_TransitWalkAccessTimeWeight * destinationWalkTime));

          if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !knrPathType && !tncPathType && !Global.Configuration.IsInEstimationMode) {
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
          _pathParkAndRideTransitTime[pathType] = transitPath.Time;
          _pathParkAndRideTransitDistance[pathType] = transitDistance;
          _pathParkAndRideTransitCost[pathType] = transitPath.Cost;
          _pathParkAndRideTransitUtility[pathType] = transitPath.Utility;
          _pathParkAndRideWalkAccessEgressTime[pathType] = destinationWalkTime;

          _utility[pathType] = nodeUtility;
          _expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
        }
      }
    }

    protected static double GetCircuityDistance(int skimMode, int pathType, double votValue, int outboundTime, IParcelWrapper oParcel, IParcelWrapper dParcel) {
      double zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, outboundTime, oParcel.ZoneId, dParcel.ZoneId).Variable;
      if (oParcel.ZoneId == dParcel.ZoneId) {
        zzDist += 0;
      }
      double circuityDistance =
                (zzDist < Constants.EPSILON || zzDist > Global.Configuration.MaximumBlendingDistance)
                    ? Constants.DEFAULT_VALUE
                    : oParcel.CalculateShortDistance(dParcel);
      return circuityDistance;
    }


    public class TransitPath {
      public bool Available { get; set; }
      public double Time { get; set; }
      public double Cost { get; set; }
      public double Boardings1 { get; set; }
      public double Boardings2 { get; set; }
      public double Utility { get; set; }
    }

    protected PathTypeModel.TransitPath GetTransitPath(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId) {

      TransitPath path = new PathTypeModel.TransitPath {
        Available = true
      };

      // check for presence of valid path
      double outboundInVehicleTime = ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double returnInVehicleTime = returnTime > 0
                ? ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                : 0;

      if (outboundInVehicleTime < Constants.EPSILON || (returnTime > 0 && returnInVehicleTime < Constants.EPSILON)) {
        path.Available = false;
        return path;
      }
      // valid path(s).  Proceed.

      double pathTypeConstant =
                pathType == Global.Settings.PathTypes.TransitType1 ?
                     (Global.Configuration.PathImpedance_TransitType1PathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType1PathConstant
                        : Global.Configuration.PathImpedance_TransitLocalBusPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType2 ?
                     (Global.Configuration.PathImpedance_TransitType2PathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType2PathConstant
                        : Global.Configuration.PathImpedance_TransitLightRailPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType3 ?
                     (Global.Configuration.PathImpedance_TransitType3PathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType3PathConstant
                        : Global.Configuration.PathImpedance_TransitPremiumBusPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType4 ?
                     (Global.Configuration.PathImpedance_TransitType4PathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType4PathConstant
                        : Global.Configuration.PathImpedance_TransitCommuterRailPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType5 ?
                     (Global.Configuration.PathImpedance_TransitType5PathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType5PathConstant
                        : Global.Configuration.PathImpedance_TransitFerryPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType1_Knr ?
                     (Global.Configuration.PathImpedance_TransitType1_KnrPathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType1_KnrPathConstant
                        : Global.Configuration.PathImpedance_TransitLocalBus_KnrPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType2_Knr ?
                     (Global.Configuration.PathImpedance_TransitType2_KnrPathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType2_KnrPathConstant
                        : Global.Configuration.PathImpedance_TransitLightRail_KnrPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType3_Knr ?
                     (Global.Configuration.PathImpedance_TransitType3_KnrPathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType3_KnrPathConstant
                        : Global.Configuration.PathImpedance_TransitPremiumBus_KnrPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType4_Knr ?
                     (Global.Configuration.PathImpedance_TransitType4_KnrPathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType4_KnrPathConstant
                        : Global.Configuration.PathImpedance_TransitCommuterRail_KnrPathConstant)
                : pathType == Global.Settings.PathTypes.TransitType5_Knr ?
                     (Global.Configuration.PathImpedance_TransitType5_KnrPathConstant != 0
                        ? Global.Configuration.PathImpedance_TransitType5_KnrPathConstant
                        : Global.Configuration.PathImpedance_TransitFerry_KnrPathConstant)
                : 0;



      double pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);


      // get outbound los
      double initialWaitTime = ImpedanceRoster.GetValue("iwaittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double transferWaitTime = ImpedanceRoster.GetValue("xwaittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double numberOfBoards1 = ImpedanceRoster.GetValue("nboard", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double fare = ImpedanceRoster.GetValue("fare", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;

      double numberOfBoards2 = 0;
      // add return LOS, if valid _departureTime passed            
      if (returnTime > 0) {
        initialWaitTime += ImpedanceRoster.GetValue("iwaittime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        transferWaitTime += ImpedanceRoster.GetValue("xwaittime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        numberOfBoards2 = ImpedanceRoster.GetValue("nboard", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        fare += ImpedanceRoster.GetValue("fare", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
      }

      fare = fare * (1.0 - _transitDiscountFraction); //fare adjustment

      // set utility
      path.Time = outboundInVehicleTime + returnInVehicleTime + initialWaitTime + transferWaitTime;
      if (path.Time > pathTimeLimit) {
        path.Available = false;
        return path;
      }
      path.Cost = fare;
      path.Boardings1 = numberOfBoards1;
      path.Boardings2 = numberOfBoards2;
      // for sacog, use pathtype-specific time skims and weights
      double pathTypeSpecificTime = 0D;
      double pathTypeSpecificTimeWeight =
                  (pathType == Global.Settings.PathTypes.TransitType2 || pathType == Global.Settings.PathTypes.TransitType2_Knr)
                    ? (Global.Configuration.PathImpedance_TransitType2TimeAdditiveWeight != 0
                        ? Global.Configuration.PathImpedance_TransitType2TimeAdditiveWeight
                        : Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight)
                  : (pathType == Global.Settings.PathTypes.TransitType3 || pathType == Global.Settings.PathTypes.TransitType3_Knr)
                    ? (Global.Configuration.PathImpedance_TransitType3TimeAdditiveWeight != 0
                        ? Global.Configuration.PathImpedance_TransitType3TimeAdditiveWeight
                        : Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight)
                  : (pathType == Global.Settings.PathTypes.TransitType4 || pathType == Global.Settings.PathTypes.TransitType4_Knr)
                    ? (Global.Configuration.PathImpedance_TransitType4TimeAdditiveWeight != 0
                        ? Global.Configuration.PathImpedance_TransitType4TimeAdditiveWeight
                        : Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight)
                  : (pathType == Global.Settings.PathTypes.TransitType5 || pathType == Global.Settings.PathTypes.TransitType5_Knr)
                    ? (Global.Configuration.PathImpedance_TransitType5TimeAdditiveWeight != 0
                        ? Global.Configuration.PathImpedance_TransitType5TimeAdditiveWeight
                        : Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight)
                  : 0;

      RegionSpecificTransitImpedanceCalculation(skimMode, pathType, votValue, outboundTime, returnTime, originZoneId, destinationZoneId, ref outboundInVehicleTime, ref returnInVehicleTime, ref pathTypeSpecificTime, ref pathTypeSpecificTimeWeight);

      double totalInVehicleTime = outboundInVehicleTime + returnInVehicleTime;

      double boardingsWeight = (Global.Configuration.PathImpedance_TransitNumberBoardingsWeight_Rail > Constants.EPSILON &&
                (pathType == Global.Settings.PathTypes.LightRail || pathType == Global.Settings.PathTypes.CommuterRail))
                ? Global.Configuration.PathImpedance_TransitNumberBoardingsWeight_Rail
                : Global.Configuration.PathImpedance_TransitNumberBoardingsWeight;

      path.Utility =
              Global.Configuration.PathImpedance_PathChoiceScaleFactor *
              (pathTypeConstant +
              _tourCostCoefficient * fare +
              _tourTimeCoefficient *
              (Global.Configuration.PathImpedance_TransitInVehicleTimeWeight * totalInVehicleTime +
               Global.Configuration.PathImpedance_TransitFirstWaitTimeWeight * initialWaitTime +
               Global.Configuration.PathImpedance_TransitTransferWaitTimeWeight * transferWaitTime +
               boardingsWeight * (numberOfBoards1 + numberOfBoards2) +
               pathTypeSpecificTime * pathTypeSpecificTimeWeight));

      return path;
    }
    protected static double GetTransitWalkTime(IParcelWrapper parcel, int pathType, double boardings) {
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

    protected static double GetXYDistance(double x1, double y1, double x2, double y2) {
      return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    public virtual List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice) {
      throw new NotImplementedException("Implemented in Actum only");
    }

    public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes) {
      throw new NotImplementedException("Implemented in Actum only");
    }

    public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes) {
      throw new NotImplementedException("Implemented in Actum only");
    }
  } //end class PathTypeModel

} //end namesapce
