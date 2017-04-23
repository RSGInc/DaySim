// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Exceptions;
using DaySim.Framework.Roster;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var modes = new List<int>();

            for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.PaidRideShare; mode++) {
                if (mode <= Global.Settings.Modes.ParkAndRide 
                || (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideShareModeIsAvailable))   {
                    modes.Add(mode);
                }
            }

            return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, modes.ToArray());
        }

        public virtual List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice) {
            var modes = new List<int>();

            for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.PaidRideShare; mode++) {
                if (mode <= Global.Settings.Modes.Transit 
                || (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.PaidRideShareModeIsAvailable))   {
                    modes.Add(mode);
                }
            }

            return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, carsAreAVs, transitDiscountFraction, randomChoice, modes.ToArray());
        }

        public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes) {
            var list = new List<IPathTypeModel>();

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
            var list = new List<IPathTypeModel>();

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


        public void RunModel(IRandomUtility randomUtility, bool useZones = false) {
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


            var votValue = (60.0 * _tourTimeCoefficient) / _tourCostCoefficient; // in $/hour


            var skimMode = (Mode == Global.Settings.Modes.ParkAndRide) ? Global.Settings.Modes.Transit : Mode;
            var availablePathTypes = 0;
            var expUtilitySum = 0D;
            var bestExpUtility = 0D;
            var bestPathType = Constants.DEFAULT_VALUE;
            // loop on all relevant path types for the mode
            for (var pathType = Global.Settings.PathTypes.FullNetwork; pathType < Global.Settings.PathTypes.TotalPathTypes; pathType++) {
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
                            && Global.Configuration.FirstZoneNumberForDestinationParkingChoice > 0
                            && _destinationParcel.ZoneKey >= Global.Configuration.FirstZoneNumberForDestinationParkingChoice
                            && _destinationParcel.ZoneKey <= Global.Configuration.LastZoneNumberForDestinationParkingChoice)   {
                            RunAutoModelWithDestinationParkingChoice(skimMode, pathType, votValue, useZones);
                        }
                        else
                            RunAutoModel(skimMode, pathType, votValue, useZones);
                    }
                } else if (Mode == Global.Settings.Modes.Transit) {
                    if (Global.StopAreaIsEnabled) {
                        RunStopAreaWalkTransitModel(skimMode, pathType, votValue, useZones);
                    } else {
                        RunSimpleWalkTransitModel(skimMode, pathType, votValue, useZones);
                    }
                } else if (Mode == Global.Settings.Modes.ParkAndRide) {
                    if (Global.StopAreaIsEnabled) {
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
            var logsum = Math.Log(expUtilitySum);
            var tourTimeCoefficient = (Global.Configuration.PathImpedance_PathChoiceScaleFactor * _tourTimeCoefficient);

            if (Double.IsNaN(expUtilitySum) || Double.IsNaN(logsum) || Double.IsNaN(tourTimeCoefficient)) {
                throw new ValueIsNaNException(string.Format("Value is NaN for utilitySum: {0}, logsum: {1}, tourTimeCoefficient: {2}.", expUtilitySum, logsum, tourTimeCoefficient));
            }

            GeneralizedTimeLogsum = logsum / tourTimeCoefficient; // need to make sure _tourTimeCoefficient is not 0

            if (Double.IsNaN(GeneralizedTimeLogsum)) {
                throw new ValueIsNaNException(string.Format("Value is NaN for GeneralizedTimeLogsum where utilitySum: {0}, logsum: {1}, tourTimeCoefficient: {2}.", expUtilitySum, logsum, tourTimeCoefficient));
            }

            // draw a choice using a random number if requested (and in application mode), otherwise return best utility
            if (_randomChoice && availablePathTypes > 1 && !Global.Configuration.IsInEstimationMode) {
                var random = randomUtility.Uniform01();

                for (var pathType = Global.Settings.PathTypes.FullNetwork; pathType <= Global.Settings.PathTypes.TotalPathTypes; pathType++) {
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


        protected void RunWalkBikeModel(int skimMode, int pathType, double votValue, bool useZones) {

            var circuityDistance = useZones ? Constants.DEFAULT_VALUE : GetCircuityDistance(skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel);

            var skimValue =
              useZones
                ? ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
                : ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);

            _pathTime[pathType] = skimValue.Variable;
            _pathDistance[pathType] = skimValue.BlendVariable;
            _pathCost[pathType] = 0;
            _pathParkAndRideNodeId[pathType] = 0;

            if (_returnTime > 0) {

                skimValue =
                  useZones
                    ? ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId)
                    : ImpedanceRoster.GetValue("time", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel, circuityDistance);

                _pathTime[pathType] += skimValue.Variable;
                _pathDistance[pathType] += skimValue.BlendVariable;
            }

            // sacog-specific adjustment of generalized time for bike mode
            if (_pathDistance[pathType] > Constants.EPSILON && skimMode == Global.Settings.Modes.Bike && Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions) {
                var d1 =
                  Math.Abs(Global.Configuration.PathImpedance_BikeType1DistanceFractionAdditiveWeight) < Constants.EPSILON
                    ? 0D
                    : useZones
                        ? ImpedanceRoster.GetValue("class1distance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("class1distance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

                var d2 =
                  Math.Abs(Global.Configuration.PathImpedance_BikeType2DistanceFractionAdditiveWeight) < Constants.EPSILON
                    ? 0D
                    : useZones
                        ? ImpedanceRoster.GetValue("class2distance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("class2distance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

                var d3 =
                  Math.Abs(Global.Configuration.PathImpedance_BikeType3DistanceFractionAdditiveWeight) < Constants.EPSILON
                    ? 0D
                    : useZones
                        ? ImpedanceRoster.GetValue("baddistance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                        : ImpedanceRoster.GetValue("baddistance", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

                var d4 = Math.Abs(Global.Configuration.PathImpedance_BikeType4DistanceFractionAdditiveWeight) < Constants.EPSILON
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

                var adjFactor =
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
            if (!useZones && _originParcel.Id == _destinationParcel.Id && skimMode == Global.Settings.Modes.Walk
              //JLB 20130628 added destination scale condition because ImpedanceRoster assigns time and cost values for intrazonals 
              && Global.Configuration.DestinationScale != Global.Settings.DestinationScales.Zone) {
                _pathTime[pathType] = 1.0;
                _pathDistance[pathType] = 0.01 * Global.Settings.DistanceUnitsPerMile;  // JLBscale.  multiplied by distance units per mile
            }



            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

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

        protected void RunAutoModel(int skimModeIn, int pathType, double votValue, bool useZones) {

            bool useAVVOT = ((skimModeIn!=Global.Settings.Modes.PaidRideShare && _carsAreAVs && Global.Configuration.AV_IncludeAutoTypeChoice)
                           ||(skimModeIn==Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs));

            bool useAVSkims = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatrices);

            var skimMode = useAVSkims ? Global.Settings.Modes.AV
                         : (skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.Hov3 : skimModeIn;
                
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
            var tollConstant = 0D;
            if (pathType == Global.Settings.PathTypes.FullNetwork && ImpedanceRoster.IsActualCombination(skimMode, Global.Settings.PathTypes.NoTolls)) {
                var noTollCost =
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

            var circuityDistance = useZones ? Constants.DEFAULT_VALUE : GetCircuityDistance(skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel);

            var skimValue =
              useZones
                ? ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
                : ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);

            _pathParkAndRideNodeId[pathType] = 0;
            _pathTime[pathType] = skimValue.Variable;
            _pathDistance[pathType] = skimValue.BlendVariable;

            //implement mileage-based pricing policy
            if (Global.Configuration.Policy_TestMilageBasedPricing) {
                var minutesAfterMidnight = _outboundTime + 180;
                var centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
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
                    var minutesAfterMidnight = _returnTime + 180;
                    var centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
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

            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

            if (_pathTime[pathType] > pathTimeLimit || _pathTime[pathType] < Constants.EPSILON) {
                return;
            }
            _pathCost[pathType] += _pathDistance[pathType] * Global.PathImpedance_AutoOperatingCostPerDistanceUnit;

            var autoTimeCoefficient = useAVVOT 
                ? _tourTimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : _tourTimeCoefficient; 
                
            _utility[pathType] = Global.Configuration.PathImpedance_PathChoiceScaleFactor *
            (_tourCostCoefficient * _pathCost[pathType] +
              autoTimeCoefficient * _pathTime[pathType] +
             tollConstant);

            _expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);
        }

        protected void RunAutoModelWithDestinationParkingChoice(int skimModeIn, int pathType, double votValue, bool useZones)     {

            bool useAVVOT = ((skimModeIn != Global.Settings.Modes.PaidRideShare && _carsAreAVs && Global.Configuration.AV_IncludeAutoTypeChoice)
                           || (skimModeIn == Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs));

            bool useAVSkims = (useAVVOT && Global.Configuration.AV_UseSeparateAVSkimMatrices);

            var skimMode = useAVSkims ? Global.Settings.Modes.AV
                         : (skimModeIn == Global.Settings.Modes.PaidRideShare) ? Global.Settings.Modes.Hov3 : skimModeIn;

            IEnumerable<IDestinationParkingNodeWrapper> destinationParkingNodes = ChoiceModelFactory.DestinationParkingNodeDao.Nodes.Where(n => n.Capacity > Constants.EPSILON);

            // valid node(s), and tour-level call  
            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
            var bestNodeUtility = -99999D;
            var originZoneId = _originParcel.ZoneId;
            var destinationZoneId = _destinationParcel.ZoneId;
            var xyDistOrigDest = Math.Max(GetXYDistance(_originParcel.XCoordinate, _originParcel.YCoordinate, _destinationParcel.XCoordinate, _destinationParcel.YCoordinate), Constants.EPSILON);

            //user-set limits on search - use high values if not set
            double maxMilesToWalk = (Global.Configuration.MaximumXYDistanceToDestinationParking > 0) ? Global.Configuration.MaximumXYDistanceToDestinationParking : 1.5D;
            double maxDistanceRatio = (Global.Configuration.MaximumRatioDistanceFromParkVersusDistanceToDestination > 0) ? Global.Configuration.MaximumRatioDistanceFromParkVersusDistanceToDestination : 1.0D;

            //double zzDistOD = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

            foreach (var node in destinationParkingNodes)  {
                // only look at nodes with positive capacity
                if (node.Capacity <= Constants.EPSILON) {
                    continue;
                }

                //test XY distance to park and ride against user-set limits
                var xyDistNodeDest = GetXYDistance(node.XCoordinate, node.YCoordinate, _destinationParcel.XCoordinate, _destinationParcel.YCoordinate);
                if (xyDistNodeDest / 5280D > maxMilesToWalk || xyDistNodeDest / xyDistOrigDest > maxDistanceRatio) { 
                    continue;
                }


                var parkingParcel = ChoiceModelFactory.Parcels[node.ParcelId];

                var arriveTime = _outboundTime;
                var departTime = _returnTime > 0 ? _returnTime : _outboundTime + 60; //revisit this assumption!!
                var parkingCost = node.SetDestinationParkingEffectivePrice(arriveTime, departTime, _purpose);

                if (parkingCost < -100) {
                    continue;
                }

                var walkDistance = 2.0 * (
                     (Global.Configuration.UseShortDistanceNodeToNodeMeasures)
                   ? parkingParcel.NodeToNodeDistance(_destinationParcel)
                   : (Global.Configuration.UseShortDistanceCircuityMeasures)
                   ? parkingParcel.CircuityDistance(_destinationParcel)
                   : xyDistNodeDest);

                var walkTime = walkDistance * Global.Configuration.PathImpedance_WalkMinutesPerMile;

                var tollCost = ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, _originParcel, parkingParcel).Variable;

                if (_returnTime > 0)  {
                    tollCost += ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, parkingParcel, _originParcel).Variable;
                }

                //if full network path and no-tolls path exists check for duplicate
                var tollConstant = 0D;
                if (pathType == Global.Settings.PathTypes.FullNetwork && ImpedanceRoster.IsActualCombination(skimMode, Global.Settings.PathTypes.NoTolls))   {
                    var noTollCost = ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _outboundTime, _originParcel, parkingParcel).Variable;

                    if (_returnTime > 0)               {
                        noTollCost += ImpedanceRoster.GetValue("toll", skimMode, Global.Settings.PathTypes.NoTolls, votValue, _returnTime, parkingParcel, _originParcel).Variable;
                    }
                    // if the toll route doesn't have a higher cost than no toll route, than make it unavailable
                    if (tollCost - noTollCost < Constants.EPSILON)   {
                        continue;
                    }
                    // else it is a toll route with a higher cost than no toll route, add a toll constant also
                    tollConstant = Global.Configuration.PathImpedance_AutoTolledPathConstant;
                }

                var circuityDistance = useZones ? Constants.DEFAULT_VALUE : GetCircuityDistance(skimMode, pathType, votValue, _outboundTime, _originParcel, parkingParcel);

                var skimValue = ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _outboundTime, _originParcel, parkingParcel, circuityDistance);

                var driveTime = skimValue.Variable;
                var driveDistance = skimValue.BlendVariable;

                if (_returnTime > 0) {

                    skimValue = ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, _returnTime, parkingParcel, _originParcel, circuityDistance);

                    driveTime += skimValue.Variable;
                    driveDistance += skimValue.BlendVariable;
                }

                var nodePathDistance = driveDistance + walkDistance;
                var nodePathTime = driveTime + walkTime;

                if (nodePathTime > pathTimeLimit || nodePathTime < Constants.EPSILON)  {
                    continue;
                }
                var nodePathCost = tollCost
                                 + driveDistance * Global.PathImpedance_AutoOperatingCostPerDistanceUnit
                                 + parkingCost;

                var autoTimeCoefficient = useAVVOT
                    ? _tourTimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : _tourTimeCoefficient;

                var nodeUtility = Global.Configuration.PathImpedance_PathChoiceScaleFactor *
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
                _pathTime[pathType] = nodePathTime;
                _pathDistance[pathType] = nodePathDistance;
                _pathCost[pathType] = nodePathCost;
                _utility[pathType] = nodeUtility;
                _expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
            }

        }

        protected void RunSimpleWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

            if (!useZones) {
                // get zones associated with parcels for transit path
                _originZoneId = _originParcel.ZoneId;
                _destinationZoneId = _destinationParcel.ZoneId;
            }

            var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, _originZoneId, _destinationZoneId);
            if (!transitPath.Available) {
                return;
            }

            var originWalkTime = useZones ? 5.0 : GetTransitWalkTime(_originParcel, pathType, transitPath.Boardings1);
            var destinationWalkTime = useZones ? 5.0 : GetTransitWalkTime(_destinationParcel, pathType, transitPath.Boardings2);

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
            var distance = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable;
            if (_returnTime > 0) {
                distance += ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable;
            }
            _pathDistance[pathType] = distance;
        }

        protected void RunStopAreaWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

            if (useZones) {
                return;
            }
            //user-set limits on search - use high values if not set
            int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearch > 0) ? Global.Configuration.MaximumStopAreasToSearch : 99;
            int maxStopAreaDistance = (Global.Configuration.MaximumParcelToStopAreaDistance > 0) ? Global.Configuration.MaximumParcelToStopAreaDistance : 99999;

            //_originParcel. SetFirstAndLastStopAreaDistanceIndexes();
            var oFirst = _originParcel.FirstPositionInStopAreaDistanceArray;
            var oLast = Math.Min(_originParcel.LastPositionInStopAreaDistanceArray, oFirst + maxStopAreasToSearch - 1);

            //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
            var dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
            var dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

            if (oFirst <= 0 || dFirst <= 0) {
                return;
            }

            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
            var bestPathUtility = -99999D;

            for (var oIndex = oFirst; oIndex <= oLast; oIndex++) {
                var oStopArea = Global.ParcelStopAreaStopAreaIds[oIndex];
                var oStopAreaKey = Global.ParcelStopAreaStopAreaKeys[oIndex];
                var oWalkDistance = Global.ParcelStopAreaDistances[oIndex];
                if (oWalkDistance > maxStopAreaDistance) {
                    continue;
                }

                for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
                    var dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
                    var dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
                    var dWalkDistance = Global.ParcelStopAreaDistances[dIndex];
                    if (dWalkDistance > maxStopAreaDistance) {
                        continue;
                    }

                    var walkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * (oWalkDistance + dWalkDistance) * Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
                    if (_returnTime > 0) {
                        walkTime *= 2;
                    }

                    var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oStopArea, dStopArea);
                    if (!transitPath.Available) {
                        continue;
                    }

                    // set utility
                    var fullPathTime = transitPath.Time + walkTime;
                    var fullPathCost = transitPath.Cost;


                    if (fullPathTime > pathTimeLimit) {
                        continue;
                    }

                    var fullPathUtility = transitPath.Utility +
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
            var distance = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;
            if (_returnTime > 0) {
                distance += ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, _destinationParcel, _originParcel).Variable;
            }
            _pathDistance[pathType] = distance;
        }


        protected void RunSimpleParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
            if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
                return;
            }
            int threadAssignedIndex = ParallelUtility.threadLocalAssignedIndex.Value;
            IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

            if (Global.Configuration.ShouldReadParkAndRideNodeSkim) {
                var nodeId =
                  useZones
                    ? (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                    : (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

                var node = ChoiceModelFactory.ParkAndRideNodeDao.Get(nodeId);

                parkAndRideNodes = new List<IParkAndRideNodeWrapper> { node };
            } else {
                parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity > 0);
            }

            // valid node(s), and tour-level call  
            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
            var bestNodeUtility = -99999D;
            var originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
            var destinationZoneId = useZones ? _destinationZoneId : _destinationParcel.ZoneId;

            //user-set limits on search - use high values if not set
            double maxMilesToDrive = (Global.Configuration.MaximumMilesToDriveToParkAndRide > 0) ? Global.Configuration.MaximumMilesToDriveToParkAndRide : 999D;
            double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

            double zzDistOD = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

            foreach (var node in parkAndRideNodes) {
                // only look at nodes with positive capacity
                if (node.Capacity < Constants.EPSILON) {
                    continue;
                }

                // use the node rather than the nearest parcel for transit LOS, becuase more accurate, and distance blending is not relevant 
                var parkAndRideZoneId = node.ZoneId;
                //test distance to park and ride against user-set limits


                double zzDistPR = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

                if (zzDistPR > maxMilesToDrive || zzDistPR / Math.Max(zzDistOD, 1.0) > maxDistanceRatio) {
                    continue;
                }

                var parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
                var parkAndRideParkingCost = node.Cost / 100.0; // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

                var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideZoneId, destinationZoneId);
                if (!transitPath.Available) {
                    continue;
                }

                var destinationWalkTime = useZones ? 5.0 : GetTransitWalkTime(_destinationParcel, pathType, transitPath.Boardings1);

                if (destinationWalkTime < -1 * Constants.EPSILON) {
                    continue;
                }

                var circuityDistance =
                  (zzDistPR > Global.Configuration.MaximumBlendingDistance)
                    ? Constants.DEFAULT_VALUE
                    : (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
                      ? _originParcel.NodeToNodeDistance(parkAndRideParcel)
                      : (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
                        ? _originParcel.CircuityDistance(parkAndRideParcel)
                        : Constants.DEFAULT_VALUE;

                var skimValue
                  = useZones
                      ? ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, parkAndRideZoneId)
                      : ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, parkAndRideParcel, circuityDistance);

                var driveTime = skimValue.Variable;
                var driveDistance = skimValue.BlendVariable;
                var parkMinute = (int)Math.Max(1, (_outboundTime - (transitPath.Time / 2.0) - 3)); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

                var transitDistance =
                  useZones
                    ? ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideZoneId, _destinationZoneId).Variable
                    : ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideParcel, _destinationParcel).Variable;

                // add return LOS

                skimValue =
                  useZones
                    ? ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideZoneId, _originZoneId)
                    : ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideParcel, _originParcel, circuityDistance);

                driveTime += skimValue.Variable;
                driveDistance += skimValue.BlendVariable;
                transitDistance *= 2;
                destinationWalkTime *= 2;

                // set utility
                var nodePathTime = transitPath.Time + driveTime + destinationWalkTime;
                var nodePathDistance = driveDistance + transitDistance;
                var nodePathCost = transitPath.Cost + parkAndRideParkingCost;

                if (nodePathTime > pathTimeLimit) {
                    continue;
                }

                var nodeUtility = transitPath.Utility +
                  Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                  (_tourCostCoefficient * parkAndRideParkingCost +
                   _tourTimeCoefficient *
                   (Global.Configuration.PathImpedance_TransitDriveAccessTimeWeight * driveTime +
                    Global.Configuration.PathImpedance_TransitWalkAccessTimeWeight * destinationWalkTime));

                if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
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

        protected void RunStopAreaParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
            if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
                return;
            }
            int threadAssignedIndex = ParallelUtility.threadLocalAssignedIndex.Value;
            IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

            if (Global.Configuration.ShouldReadParkAndRideNodeSkim) {
                var nodeId =
                  useZones
                    ? (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
                    : (int)ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

                var node = ChoiceModelFactory.ParkAndRideNodeDao.Get(nodeId);

                parkAndRideNodes = new List<IParkAndRideNodeWrapper> { node };
            } else {
                parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity > 0);
            }

            // valid node(s), and tour-level call  
            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
            var bestNodeUtility = -99999D;
            var originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;

            //user-set limits on search - use high values if not set
            int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
            int maxStopAreaDistance = (Global.Configuration.MaximumParcelToStopAreaDistanceParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaDistanceParkAndRide : 99999;
            double maxMilesToDrive = (Global.Configuration.MaximumMilesToDriveToParkAndRide > 0) ? Global.Configuration.MaximumMilesToDriveToParkAndRide : 999D;
            double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

            //_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
            var dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
            var dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

            if (dFirst <= 0) {
                return;
            }

            double zzDistOD = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

            foreach (var node in parkAndRideNodes) {
                // only look at nodes with positive capacity
                if (node.Capacity < Constants.EPSILON) {
                    continue;
                }

                var parkAndRideZoneId = node.ZoneId;
                //test distance to park and ride against user-set limits
                var zzDistPR = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

                if (zzDistPR > maxMilesToDrive || zzDistPR / Math.Max(zzDistOD, 1.0) > maxDistanceRatio) {
                    continue;
                }

                // use the nearest stop area for transit LOS  
                var parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
                var parkAndRideStopAreaKey = node.NearestStopAreaId;
                var parkAndRideStopArea = Global.TransitStopAreaMapping[node.NearestStopAreaId];
                var parkAndRideParkingCost = node.Cost / 100.0; // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars


                var circuityDistance =
                    (zzDistPR > Global.Configuration.MaximumBlendingDistance)
                    ? Constants.DEFAULT_VALUE
                    : (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
                      ? _originParcel.NodeToNodeDistance(parkAndRideParcel)
                      : (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
                        ? _originParcel.CircuityDistance(parkAndRideParcel)
                        : Constants.DEFAULT_VALUE;

                var skimValue
                    = useZones
                      ? ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, parkAndRideZoneId)
                      : ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, parkAndRideParcel, circuityDistance);

                var driveDistance = skimValue.BlendVariable;
                var driveTime = skimValue.Variable;

                var transitDistance =
                    useZones
                    ? ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideZoneId, _destinationZoneId).Variable
                    : ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideParcel, _destinationParcel).Variable;

                // add return los
                skimValue =
                    useZones
                    ? ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideZoneId, _originZoneId)
                    : ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, parkAndRideParcel, _originParcel, circuityDistance);

                driveTime += skimValue.Variable;
                driveDistance += skimValue.BlendVariable;
                transitDistance *= 2;

                //loop on stop areas near destination
                for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
                    var dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
                    var dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
                    var dWalkDistance = Global.ParcelStopAreaDistances[dIndex];
                    if (dWalkDistance > maxStopAreaDistance) {
                        continue;
                    }

                    var destinationWalkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * dWalkDistance * Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
                    destinationWalkTime *= 2; //round trip

                    var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dStopArea);
                    if (!transitPath.Available) {
                        continue;
                    }

                    var parkMinute = (int)Math.Max(1, (_outboundTime - (transitPath.Time / 2.0) - 3)); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

                    // set utility
                    var nodePathTime = transitPath.Time + driveTime + destinationWalkTime;
                    var nodePathDistance = driveDistance + transitDistance;
                    var nodePathCost = transitPath.Cost + parkAndRideParkingCost;

                    if (nodePathTime > pathTimeLimit) {
                        continue;
                    }

                    var nodeUtility = transitPath.Utility +
                      Global.Configuration.PathImpedance_PathChoiceScaleFactor *
                      (_tourCostCoefficient * parkAndRideParkingCost +
                      _tourTimeCoefficient *
                      (Global.Configuration.PathImpedance_TransitDriveAccessTimeWeight * driveTime +
                       Global.Configuration.PathImpedance_TransitWalkAccessTimeWeight * destinationWalkTime));

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
            var zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, outboundTime, oParcel.ZoneId, dParcel.ZoneId).Variable;
            if (oParcel.ZoneId == dParcel.ZoneId) {
                zzDist += 0;
            }
            var circuityDistance =
                (zzDist < Constants.EPSILON || zzDist > Global.Configuration.MaximumBlendingDistance)
                    ? Constants.DEFAULT_VALUE
                    : (Global.Configuration.UseShortDistanceNodeToNodeMeasures)
                        ? oParcel.NodeToNodeDistance(dParcel)
                        : (Global.Configuration.UseShortDistanceCircuityMeasures)
                            ? oParcel.CircuityDistance(dParcel)
                            : Constants.DEFAULT_VALUE;
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

            var path = new PathTypeModel.TransitPath();
            path.Available = true;

            // check for presence of valid path
            var outboundInVehicleTime = ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            var returnInVehicleTime = returnTime > 0
                ? ImpedanceRoster.GetValue("ivtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                : 0;

            if (outboundInVehicleTime < Constants.EPSILON || (returnTime > 0 && returnInVehicleTime < Constants.EPSILON)) {
                path.Available = false;
                return path;
            }
            // valid path(s).  Proceed.

            var pathTypeConstant =
                pathType == Global.Settings.PathTypes.LocalBus
                  ? Global.Configuration.PathImpedance_TransitLocalBusPathConstant
                  : pathType == Global.Settings.PathTypes.LightRail
                      ? Global.Configuration.PathImpedance_TransitLightRailPathConstant
                      : pathType == Global.Settings.PathTypes.PremiumBus
                         ? Global.Configuration.PathImpedance_TransitPremiumBusPathConstant
                         : pathType == Global.Settings.PathTypes.CommuterRail
                            ? Global.Configuration.PathImpedance_TransitCommuterRailPathConstant
                            : pathType == Global.Settings.PathTypes.Ferry
                                ? Global.Configuration.PathImpedance_TransitFerryPathConstant
                                : 0;

            var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);


            // get outbound los
            var initialWaitTime = ImpedanceRoster.GetValue("iwaittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            var transferWaitTime = ImpedanceRoster.GetValue("xwaittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            var numberOfBoards1 = ImpedanceRoster.GetValue("nboard", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            var fare = ImpedanceRoster.GetValue("fare", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;

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
            var pathTypeSpecificTime = 0D;
            var pathTypeSpecificTimeWeight =
                  pathType == Global.Settings.PathTypes.LightRail
                    ? Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight
                    : pathType == Global.Settings.PathTypes.PremiumBus
                        ? Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight
                        : pathType == Global.Settings.PathTypes.CommuterRail
                             ? Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight
                             : pathType == Global.Settings.PathTypes.Ferry
                                  ? Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight
                                  : 0D;

            RegionSpecificTransitImpedanceCalculation(skimMode, pathType, votValue, outboundTime, returnTime, originZoneId, destinationZoneId, ref outboundInVehicleTime, ref returnInVehicleTime, ref pathTypeSpecificTime, ref pathTypeSpecificTimeWeight);

            var totalInVehicleTime = outboundInVehicleTime + returnInVehicleTime;

            var boardingsWeight = (Global.Configuration.PathImpedance_TransitNumberBoardingsWeight_Rail > Constants.EPSILON &&
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
            var walkDist = parcel.DistanceToLocalBus; // default is local bus (feeder), for any submode

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
        protected static double GetXYDistance(double x1, double y1, double x2, double y2)        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
    }

}
