// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using Daysim.ChoiceModels;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Exceptions;
using Daysim.Framework.Roster;
//using Daysim.DomainModels.Actum.Wrappers;
//using Daysim.DomainModels.Actum.Wrappers.Interfaces;

//using System.IO;
using Daysim.Framework.Factories;
using Daysim.Framework.DomainModels.Models;
using Ninject;




namespace Daysim.PathTypeModels {
	public class PathTypeModel_Actum : IPathTypeModel {
		private const double MAX_UTILITY = 80D;
		private const double MIN_UTILITY = -80D;

		private IParcelWrapper _originParcel;
		private IParcelWrapper _destinationParcel;
		private int _originZoneId;
		private int _destinationZoneId;
		private int _outboundTime;
		private int _returnTime;
		private int _purpose;
		private double _tourCostCoefficient;
		private double _tourTimeCoefficient;
		private bool _isDrivingAge;
		private int _householdCars;
		private int _transitPassOwnership;
		private double _transitDiscountFraction;
		private bool _randomChoice;
		private int _choice;

		// model variables
		private readonly double[] _utility = new double[Global.Settings.PathTypes.TotalPathTypes];
		private readonly double[] _expUtility = new double[Global.Settings.PathTypes.TotalPathTypes];
		private readonly double[] _pathTime = new double[Global.Settings.PathTypes.TotalPathTypes];
		private readonly double[] _pathDistance = new double[Global.Settings.PathTypes.TotalPathTypes];
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


		public PathTypeModel_Actum() {
			GeneralizedTimeLogsum = Global.Settings.GeneralizedTimeUnavailable;
			GeneralizedTimeChosen = Global.Settings.GeneralizedTimeUnavailable;
		}

		public virtual int Mode { get; set; }
		public virtual double GeneralizedTimeLogsum { get; protected set; }
		public virtual double GeneralizedTimeChosen { get; protected set; }
		public virtual double PathTime { get; protected set; }
		public virtual double PathDistance { get; protected set; }
		public virtual double PathCost { get; protected set; }
		public virtual int PathType { get; protected set; }
		public virtual int PathParkAndRideNodeId { get; protected set; }
		public virtual int PathOriginStopAreaKey { get; protected set; }
		public virtual int PathDestinationStopAreaKey { get; protected set; }
		public virtual double PathTransitTime { get; protected set; }
		public virtual double PathTransitDistance { get; protected set; }
		public virtual double PathTransitCost { get; protected set; }
		public virtual double PathTransitGeneralizedTime { get; protected set; }

		public virtual double PathWalkTime { get; protected set; }
		public virtual double PathWalkDistance { get; protected set; }
		public virtual double PathBikeTime { get; protected set; }
		public virtual double PathBikeDistance { get; protected set; }
		public virtual double PathBikeCost { get; protected set; }

		public virtual bool Available { get; protected set; }

		public virtual List<IPathTypeModel> RunAllPlusParkAndRide(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, double transitDiscountFraction, bool randomChoice) {
			//		public virtual List<PathTypeModel_Actum> RunAllPlusParkAndRide(IRandomUtility randomUtility, IActumParcelWrapper originParcel, IActumParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice) {
			var modes = new List<int>();

			for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.ParkAndRide; mode++) {
				modes.Add(mode);
			}

			return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, transitPassOwnership, transitDiscountFraction, randomChoice, modes.ToArray());
		}

		public virtual List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, double transitDiscountFraction, bool randomChoice) {
			//		public virtual List<PathTypeModel_Actum> RunAll(IRandomUtility randomUtility, IActumParcelWrapper originParcel, IActumParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice) {
			var modes = new List<int>();

			for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.Transit; mode++) {
				modes.Add(mode);
			}

			return Run(randomUtility, originParcel, destinationParcel, outboundTime, returnTime, purpose, tourCostCoefficient, tourTimeCoefficient, isDrivingAge, householdCars, transitPassOwnership, transitDiscountFraction, randomChoice, modes.ToArray());
		}

		public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, double transitDiscountFraction, bool randomChoice, params int[] modes) {
			//		public virtual List<PathTypeModel_Actum> Run(IRandomUtility randomUtility, IActumParcelWrapper originParcel, IActumParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice, params int[] modes) {
			var list = new List<IPathTypeModel>();
			//			var list = new List<PathTypeModel_Actum>();

			foreach (int mode in modes) {
				var pathTypeModel = new PathTypeModel_Actum { _originParcel = originParcel, _destinationParcel = destinationParcel, _outboundTime = outboundTime, _returnTime = returnTime, _purpose = purpose, _tourCostCoefficient = tourCostCoefficient, _tourTimeCoefficient = tourTimeCoefficient, _isDrivingAge = isDrivingAge, _householdCars = householdCars, _transitPassOwnership = transitPassOwnership, _transitDiscountFraction = transitDiscountFraction, _randomChoice = randomChoice, Mode = mode };
				pathTypeModel.RunModel(randomUtility);

				list.Add(pathTypeModel);
			}

			return list;
		}

		public virtual List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, double transitDiscountFraction, bool randomChoice, params int[] modes) {
			var list = new List<IPathTypeModel>();

			foreach (var pathTypeModel in modes.Select(mode => new PathTypeModel_Actum { _originZoneId = originZoneId, _destinationZoneId = destinationZoneId, _outboundTime = outboundTime, _returnTime = returnTime, _purpose = purpose, _tourCostCoefficient = tourCostCoefficient, _tourTimeCoefficient = tourTimeCoefficient, _isDrivingAge = isDrivingAge, _householdCars = householdCars, _transitPassOwnership = transitPassOwnership, _transitDiscountFraction = transitDiscountFraction, _randomChoice = randomChoice, Mode = mode })) {
				pathTypeModel.RunModel(randomUtility, true);

				list.Add(pathTypeModel);
			}

			return list;
		}

		private void RunModel(IRandomUtility randomUtility, bool useZones = false) {
			if (Mode == Global.Settings.Modes.Hov2) {
				_tourCostCoefficient
					= _tourCostCoefficient /
					  (_purpose == Global.Settings.Purposes.Work
						  ? Global.Configuration.Coefficients_HOV2CostDivisor_Work
						  : Global.Configuration.Coefficients_HOV2CostDivisor_Other);
			}
			else if (Mode == Global.Settings.Modes.Hov3) {
				_tourCostCoefficient
					= _tourCostCoefficient /
					  (_purpose == Global.Settings.Purposes.Work
						  ? Global.Configuration.Coefficients_HOV3CostDivisor_Work
						  : Global.Configuration.Coefficients_HOV3CostDivisor_Other);
			}


			var votValue = (60.0 * _tourTimeCoefficient) / _tourCostCoefficient; // in $/hour

			//ACTUM Begin
			if (useZones == false && (Global.Configuration.PathImpedance_UtilityForm_Auto == 1
				|| Global.Configuration.PathImpedance_UtilityForm_Transit == 1)) {
				if (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Escort) {
					votValue = Global.Configuration.VotMediumHigh - 0.5;
				}
				else if (_purpose == Global.Settings.Purposes.Business) {
					votValue = Global.Configuration.VotHighVeryHigh - 0.5;
				}
				else votValue = Global.Configuration.VotLowMedium - 0.5;
			}
			//ACTUM End

			var skimMode = (Mode == Global.Settings.Modes.BikeOnTransit) ? Global.Settings.Modes.BikeOnTransit
				: (Mode >= Global.Settings.Modes.CarParkRideWalk) ? Global.Settings.Modes.Transit
				: Mode;
			var availablePathTypes = 0;
			var expUtilitySum = 0D;
			var bestExpUtility = 0D;
			var bestPathType = Constants.DEFAULT_VALUE;
			int batchNumber = ParallelUtility.GetBatchFromThreadId();

			// loop on all relevant path types for the mode
			for (var pathType = Global.Settings.PathTypes.FullNetwork; pathType < Global.Settings.PathTypes.TotalPathTypes; pathType++) {
				_expUtility[pathType] = 0D;

				if (!ImpedanceRoster.IsActualCombination(skimMode, pathType)) {
					continue;
				}

				// set path type utility and impedance, depending on the mode
				if (Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Walk) {
					RunWalkBikeModel(skimMode, pathType, votValue, useZones, batchNumber);
				}
				else if (Mode == Global.Settings.Modes.HovDriver || Mode == Global.Settings.Modes.HovPassenger || Mode == Global.Settings.Modes.Sov) {
					if (Mode != Global.Settings.Modes.Sov || (_isDrivingAge && _householdCars > 0)) {
						RunAutoModelNew(skimMode, pathType, votValue, useZones);
					}
				}
				else if (Mode == Global.Settings.Modes.Transit) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaWalkTransitModel(skimMode, pathType, votValue, useZones);
					}
					else {
						RunSimpleWalkTransitModel(skimMode, pathType, votValue, useZones);
					}
				}
				else if (Mode == Global.Settings.Modes.CarParkRideWalk) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaParkAndRideModel(skimMode, pathType, votValue, useZones);
					}
					else {
						RunSimpleParkAndRideModel(skimMode, pathType, votValue, useZones);
					}
				}
				else if (Mode == Global.Settings.Modes.CarKissRideWalk) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaParkAndRideModel(skimMode, pathType, votValue, useZones);
					}
					else {
					}
				}
				else if (Mode == Global.Settings.Modes.BikeParkRideWalk) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaBikeParkRideWalkModel(skimMode, pathType, votValue, useZones);
					}
					else {
					}
				}
				else if (Mode == Global.Settings.Modes.BikeParkRideBike) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaBikeParkRideBikeModel(skimMode, pathType, votValue, useZones);
					}
					else {
					}
				}
				else if (Mode == Global.Settings.Modes.BikeOnTransit) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaBikeOnTransitModel(skimMode, pathType, votValue, useZones);
					}
					else {
					}
				}
				else if (Mode == Global.Settings.Modes.CarParkRideBike) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaCarParkRideBikeModel(skimMode, pathType, votValue, useZones);
					}
					else {
					}
				}
				else if (Mode == Global.Settings.Modes.WalkRideBike) {
					if (Global.StopAreaIsEnabled) {
						RunStopAreaWalkRideBikeModel(skimMode, pathType, votValue, useZones);
					}
					else {
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
			}
			else {
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
		}


		private void RunWalkBikeModel(int skimMode, int pathType, double votValue, bool useZones, int batch) {
			var zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable;
			var circuityDistance =
				(zzDist > Global.Configuration.MaximumBlendingDistance)
					? Constants.DEFAULT_VALUE
					: (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
						? _originParcel.NodeToNodeDistance(_destinationParcel, batch)
						: (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
							? _originParcel.CircuityDistance(_destinationParcel)
							: Constants.DEFAULT_VALUE;
			//test output
			//var orth=(Math.Abs(_originParcel.XCoordinate - _destinationParcel.XCoordinate) + Math.Abs(_originParcel.YCoordinate - _destinationParcel.YCoordinate)) / 5280.0;
			//Global.PrintFile.WriteLine("Circuity distance for parcels {0} to {1} is {2} vs {3}",_originParcel.Id, _destinationParcel.Id, circuityDistance, orth);

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
			//			if (_pathTime[pathType] < Constants.EPSILON && _pathDistance[pathType] >= Constants.EPSILON ) {
			//				_pathTime[pathType] = _pathDistance[pathType] * (skimMode == Global.Settings.Modes.Walk ? 20.0 : 6.0) ; 
			//			}

			// a fix for intra-parcels, which happen once in a great while for school
			if (!useZones && _originParcel.Id == _destinationParcel.Id && skimMode == Global.Settings.Modes.Walk
				//JLB 20130628 added destination scale condition because ImpedanceRoster assigns time and cost values for intrazonals 
				&& Global.Settings.DestinationScale != Global.Settings.DestinationScales.Zone) {
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
			}
			else {
				_pathWalkTime[pathType] = _pathTime[pathType];
				_pathWalkDistance[pathType] = _pathDistance[pathType];
				_pathBikeTime[pathType] = 0;
				_pathBikeDistance[pathType] = 0;
			}
		}

		private void RunAutoModel(int skimMode, int pathType, double votValue, bool useZones) {
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

			var zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable;
			int batchNumber = ParallelUtility.GetBatchFromThreadId();
			var circuityDistance =
				(zzDist > Global.Configuration.MaximumBlendingDistance)
					? Constants.DEFAULT_VALUE
					: (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
						? _originParcel.NodeToNodeDistance(_destinationParcel, batchNumber)
						: (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
							? _originParcel.CircuityDistance(_destinationParcel)
							: Constants.DEFAULT_VALUE;

			var skimValue1 =
				useZones
					? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
					: ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);

			var skimValue2 =
				useZones
					? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
					: ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);

			_pathParkAndRideNodeId[pathType] = 0;
			_pathTime[pathType] = skimValue1.Variable + skimValue2.Variable;
			_pathDistance[pathType] = skimValue1.BlendVariable;

			//implement mileage-based pricing policy
			if (Global.Configuration.Policy_TestMilageBasedPricing) {
				var minutesAfterMidnight = _outboundTime + 180;
				var centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
					 ? Global.Configuration.Policy_CentsPerMileInAMPeak :
						(minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
						  ? Global.Configuration.Policy_CentsPerMileInPMPeak :
							 (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
								? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
				_pathCost[pathType] += skimValue1.BlendVariable * centsPerMile / 100.0;
			}
			if (_returnTime > 0) {

				skimValue1 =
					useZones
						? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId)
						: ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel, circuityDistance);

				skimValue2 =
					useZones
						? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _returnTime, _destinationZoneId, _originZoneId)
						: ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _returnTime, _destinationParcel, _originParcel, circuityDistance);

				_pathTime[pathType] += skimValue1.Variable + skimValue2.Variable;
				_pathDistance[pathType] += skimValue1.BlendVariable;

				//implement mileage-based pricing policy
				if (Global.Configuration.Policy_TestMilageBasedPricing) {
					var minutesAfterMidnight = _returnTime + 180;
					var centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
						 ? Global.Configuration.Policy_CentsPerMileInAMPeak :
							(minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
							  ? Global.Configuration.Policy_CentsPerMileInPMPeak :
								 (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
									? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
					_pathCost[pathType] += skimValue1.BlendVariable * centsPerMile / 100.0;
				}
			}

			// a fix for unconnected parcels/zones (sampling should be fixed to not sample them in the first place)
			//			if (_pathTime[pathType] < Constants.EPSILON && _pathDistance[pathType] >= Constants.EPSILON ) {
			//				_pathTime[pathType] = _pathDistance[pathType] * 2.0 ;  // correct missing time with speed of 30 mph 
			//			}
			//			else if (_pathTime[pathType] < Constants.EPSILON && _pathDistance[pathType] < Constants.EPSILON ) {
			//				_pathDistance[pathType] = (Math.Abs(_originParcel.XCoordinate - _destinationParcel.XCoordinate) 
			//					                      + Math.Abs(_originParcel.YCoordinate - _destinationParcel.YCoordinate))/5280D;
			//				_pathTime[pathType] = _pathDistance[pathType] * 2.0 ;  // correct missing time with speed of 30 mph 
			//			}

			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

			if (_pathTime[pathType] > pathTimeLimit || _pathTime[pathType] < Constants.EPSILON) {
				return;
			}

			_pathCost[pathType] += _pathDistance[pathType] * Global.PathImpedance_AutoOperatingCostPerDistanceUnit;

			//ACTUM Begin
			//if (useZones == false && Global.Configuration.PathImpedance_UtilityForm_Auto == 1) {

			//calculate and add parking cost   JLB 201508

			var parkingCost = 0.0;
			if (!useZones) {
				parkingCost =  // hourly
					_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ElevenPM, Global.Settings.Times.MinutesInADay)
						? _destinationParcel.ParkingCostPerHour23_08
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.EightAM)
							? _destinationParcel.ParkingCostPerHour23_08
							: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.SixPM)
								? _destinationParcel.ParkingCostPerHour8_18
							: //_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.ElevenPM)
					//?
							_destinationParcel.ParkingCostPerHour18_23;
				var parkingDuration = 1; // assume 1 our if return time isn't known
				if (_returnTime > 0) {
					parkingDuration = (_returnTime - _outboundTime) / 60;
				}
				parkingCost = parkingCost * parkingDuration;  //in monetary units
				_pathCost[pathType] += parkingCost;
			}
			//calculate time utility
			var freeFlowSkimValue =
				useZones
					? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
					: ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);
			var freeFlowTime = freeFlowSkimValue.Variable;
			var extraSkimValue =
			useZones
				? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
				: ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);
			var extraTime = extraSkimValue.Variable;

			var searchMinutes = 0.0;
			if (!useZones) {
				//time.DeparturePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenPM, Global.Settings.Times.MinutesInADay).ToFlag()
				searchMinutes =
					_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.NinePM, Global.Settings.Times.MinutesInADay)
					? _destinationParcel.ParkingSearchTime21_05
					: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.FiveAM)
						? _destinationParcel.ParkingSearchTime21_05
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.FiveAM, Global.Settings.Times.SixAM)
							? _destinationParcel.ParkingSearchTime05_06

						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.SevenAM)
							? _destinationParcel.ParkingSearchTime06_07
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.EightAM)
							? _destinationParcel.ParkingSearchTime07_08
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.NineAM)
							? _destinationParcel.ParkingSearchTime08_09
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.ThreePM)
							? _destinationParcel.ParkingSearchTime09_15

						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.FourPM)
							? _destinationParcel.ParkingSearchTime09_15
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.ThreePM)
							? _destinationParcel.ParkingSearchTime15_16
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.FivePM)
							? _destinationParcel.ParkingSearchTime16_17
						: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.FivePM, Global.Settings.Times.SixPM)
							? _destinationParcel.ParkingSearchTime17_18
						: //_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.NinePM)
					//?
							_destinationParcel.ParkingSearchTime18_21;
			}
			//extraTime += _destinationParcel.ParkingOffStreetPaidHourlyPrice; //this property represents average search time per trip in Actum data
			extraTime += searchMinutes;
			if (_returnTime > 0) {
				freeFlowSkimValue =
					useZones
						? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
						: ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);
				freeFlowTime += freeFlowSkimValue.Variable;
				extraSkimValue =
				useZones
					? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId)
					: ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel, circuityDistance);
				extraTime += extraSkimValue.Variable;
			}
			var gammaFreeFlowTime = GammaFunction(freeFlowTime, Global.Configuration.PathImpedance_Gamma_InVehicleTime);

			extraTime = Global.Configuration.Policy_CongestedTravelTimeMultiplier != 0 ? extraTime * Global.Configuration.Policy_CongestedTravelTimeMultiplier : extraTime;
			var gammaExtraTime = GammaFunction(extraTime, Global.Configuration.PathImpedance_Gamma_ExtraTime);
			//determine time weight
			//extra time weight for driver and passenger
			double inVehicleExtraTimeWeight;
			if (skimMode == Global.Settings.Modes.HovPassenger) {
				inVehicleExtraTimeWeight = Global.Configuration.PathImpedance_InVehicleExtraTimeWeight_Passenger;
			}
			else {
				inVehicleExtraTimeWeight = Global.Configuration.PathImpedance_InVehicleExtraTimeWeight_Driver;
			}
			//weights for purpose x mode
			int aggregatePurpose = 0;
			double inVehicleTimeWeight;
			if (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Escort) {
				aggregatePurpose = 1;
			}
			else if (_purpose == Global.Settings.Purposes.Business) {
				aggregatePurpose = 2;
			}
			if (skimMode == Global.Settings.Modes.Sov) {
				if (aggregatePurpose == 1) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_SOV;
				}
				else if (aggregatePurpose == 2) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_SOV;
				}
				else {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_SOV;
				}
			}
			else if (skimMode == Global.Settings.Modes.HovDriver) {
				if (aggregatePurpose == 1) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_HOVDriver;
				}
				else if (aggregatePurpose == 2) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_HOVDriver;
				}
				else {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_HOVDriver;
				}
			}
			else {
				if (aggregatePurpose == 1) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_HOVPassenger;
				}
				else if (aggregatePurpose == 2) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_HOVPassenger;
				}
				else {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_HOVPassenger;
				}
			}
			//calculate cost utility

			//JLB 201508  insert parking cost calculation above
			//_pathCost[pathType] += _destinationParcel.ParkingOffStreetPaidDailyPrice; // this property represents avg parking cost per trip in Actum

			double gammaCost;
			double distanceCoefficient;
			double modeConstant;
			if (skimMode == Global.Settings.Modes.HovPassenger && !Global.Configuration.HOVPassengersIncurCosts) {
				gammaCost = 0;
			}
			else {
				gammaCost = GammaFunction(_pathCost[pathType], Global.Configuration.PathImpedance_Gamma_Cost);
			}
			//calculate distance utility
			//calculate utility
			_utility[pathType] = Global.Configuration.PathImpedance_PathChoiceScaleFactor
				* (_tourCostCoefficient * gammaCost
				+ _tourTimeCoefficient * inVehicleTimeWeight
				* (gammaFreeFlowTime + gammaExtraTime * inVehicleExtraTimeWeight)
				+ tollConstant);
			//}
			//ACTUM End
			//else {
			//	_utility[pathType] = Global.Configuration.PathImpedance_PathChoiceScaleFactor *
			//	(_tourCostCoefficient * _pathCost[pathType] +
			//	 _tourTimeCoefficient * _pathTime[pathType] +
			//	 tollConstant);
			//}

			_expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);

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
		}

		private void RunAutoModelNew(int skimMode, int pathType, double votValue, bool useZones) {

			var path = GetAutoPath(skimMode, pathType, votValue, useZones, _originZoneId, _destinationZoneId, _originParcel, _destinationParcel);

			//calculate parking cost utility   JLB 201508
			var parkingCost = 0.0;
			var parkingUtility = 0.0;
			if (skimMode == Global.Settings.Modes.HovPassenger && !Global.Configuration.HOVPassengersIncurCosts) {
			}
			else {
				if (!useZones) {
					parkingCost =  // hourly
						_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ElevenPM, Global.Settings.Times.MinutesInADay)
							? _destinationParcel.ParkingCostPerHour23_08
							: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.EightAM)
								? _destinationParcel.ParkingCostPerHour23_08
								: _outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.SixPM)
									? _destinationParcel.ParkingCostPerHour8_18
								: //_outboundTime.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.ElevenPM)
						//?
								_destinationParcel.ParkingCostPerHour18_23;
					var parkingDuration = 1; // assume 1 hour if return time isn't known
					if (_returnTime > 0) {
						parkingDuration = (_returnTime - _outboundTime) / 60;
					}
					parkingCost = parkingCost * parkingDuration;  //in monetary units
					parkingUtility = Global.Configuration.PathImpedance_PathChoiceScaleFactor * _tourCostCoefficient * GammaFunction(parkingCost, Global.Configuration.PathImpedance_Gamma_Cost);
				}
			}

			//set pathType properties
			_pathCost[pathType] = path.Cost + parkingCost;
			_pathTime[pathType] = path.Time;
			_pathDistance[pathType] = path.Distance;
			_utility[pathType] = path.Utility + parkingUtility;
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
		}

		private void RunSimpleWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

			if (!useZones) {
				// get zones associated with parcels for transit path
				_originZoneId = _originParcel.ZoneId;
				_destinationZoneId = _destinationParcel.ZoneId;
			}

			var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, _originZoneId, _destinationZoneId, _transitPassOwnership);
			if (!transitPath.Available) {
				return;
			}

			//set final values
			_pathParkAndRideNodeId[pathType] = 0;
			_pathTime[pathType] = transitPath.Time;
			_pathCost[pathType] = transitPath.Cost;
			_utility[pathType] = transitPath.Utility;

			_expUtility[pathType] = _utility[pathType] > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : _utility[pathType] < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(_utility[pathType]);

			//for transit, use auto distance
			var distance = ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable;
			if (_returnTime > 0) {
				distance += ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _returnTime, _destinationZoneId, _originZoneId).Variable;
			}
			_pathDistance[pathType] = distance;
		}

		private void RunStopAreaWalkTransitModel(int skimMode, int pathType, double votValue, bool useZones) {

			if (useZones) {
				return;
			}
			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearch > 0) ? Global.Configuration.MaximumStopAreasToSearch : 99;
			int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnits > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnits : 99999;

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
				var oWalkLength = Global.ParcelStopAreaLengths[oIndex];
				if (oWalkLength > maxStopAreaLength) {
					continue;
				}

				for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
					var dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
					var dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
					var dWalkLength = Global.ParcelStopAreaLengths[dIndex];
					if (dWalkLength > maxStopAreaLength) {
						continue;
					}

					var walkDistance = (oWalkLength + dWalkLength) / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
					var walkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * walkDistance;
					walkDistance = walkDistance * (_returnTime > 0 ? 2 : 1);
					walkTime = walkTime * (_returnTime > 0 ? 2 : 1);

					var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oStopArea, dStopArea, _transitPassOwnership);
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
					 _tourTimeCoefficient * Global.Configuration.PathImpedance_TransitAccessEgressTimeWeight * walkTime;


					// if the best path so far, reset pathType properties
					if (fullPathUtility <= bestPathUtility) {
						continue;
					}

					bestPathUtility = fullPathUtility;

					_pathDistance[pathType] = transitPath.Distance + walkDistance;
					_pathOriginStopAreaKey[pathType] = oStopAreaKey;
					_pathDestinationStopAreaKey[pathType] = dStopAreaKey;
					_pathTime[pathType] = fullPathTime;
					_pathCost[pathType] = fullPathCost;
					_utility[pathType] = fullPathUtility;
					_expUtility[pathType] = fullPathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : fullPathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(fullPathUtility);
					_pathWalkDistance[pathType] = walkDistance;
					_pathWalkTime[pathType] = walkTime;
					_pathTransitTime[pathType] = transitPath.Time;
					_pathTransitDistance[pathType] = transitPath.Distance;
					_pathTransitCost[pathType] = transitPath.Cost;
					_pathTransitUtility[pathType] = transitPath.Utility;
				}
			}
			_pathParkAndRideNodeId[pathType] = 0;
			_pathBikeCost[pathType] = 0;
			_pathBikeTime[pathType] = 0;
			_pathBikeDistance[pathType] = 0;
		}


		private void RunSimpleParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

			if (Global.Configuration.ShouldReadParkAndRideNodeSkim) {
				var nodeId =
					useZones
						? (int) ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
						: (int) ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

				var node = ChoiceModelFactory.ParkAndRideNodeDao.Get(nodeId);

				parkAndRideNodes = new List<IParkAndRideNodeWrapper> { node };
			}
			else {
				parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity > 0);
			}

			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestNodeUtility = -99999D;
			var originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
			var destinationZoneId = useZones ? _destinationZoneId : _destinationParcel.ZoneId;

			//user-set limits on search - use high values if not set
			double maxMilesToDrive = (Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide > 0) ? Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide : 999D;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

			foreach (var node in parkAndRideNodes) {
				// only look at nodes with positive capacity
				if (node.Capacity < Constants.EPSILON) {
					continue;
				}

				// use the node rather than the nearest parcel for transit LOS, becuase more accurate, and distance blending is not relevant 
				var parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
				var parkAndRideZoneId = node.ZoneId;
				var parkAndRideParkingCost = node.Cost / 100.0; // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

				var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideZoneId, destinationZoneId, _transitPassOwnership);
				if (!transitPath.Available) {
					continue;
				}

				var zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

				var circuityDistance =
					(zzDist > Global.Configuration.MaximumBlendingDistance)
						? Constants.DEFAULT_VALUE
						: (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
							? _originParcel.NodeToNodeDistance(parkAndRideParcel, batchNumber)
							: (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
								? _originParcel.CircuityDistance(parkAndRideParcel)
								: Constants.DEFAULT_VALUE;

				var skimValue
					= useZones
						  ? ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originZoneId, parkAndRideZoneId)
						  : ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, _originParcel, parkAndRideParcel, circuityDistance);

				var driveTime = skimValue.Variable;
				var driveDistance = skimValue.BlendVariable;
				var parkMinute = (int) (_outboundTime - (transitPath.Time / 2.0) - 3); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

				var transitDistance =
					useZones
						? ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideZoneId, _destinationZoneId).Variable
						: ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, parkAndRideParcel, _destinationParcel).Variable;

				var destinationWalkTime = useZones ? 5.0 : GetTransitWalkTime(_destinationParcel, pathType, transitPath.Boardings1);

				// add return LOS
				if (destinationWalkTime < -1 * Constants.EPSILON) {
					continue;
				}

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
				_utility[pathType] = nodeUtility;
				_expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
			}

		}

		private void RunStopAreaParkAndRideModel(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			//int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

			parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 1);

			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestNodeUtility = -99999D;
			var originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
			var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide : 99999;
			double maxDistanceUnitsToDrive = (Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide > 0) ? Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide : 999D;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

			if (dFirst <= 0) {
				return;
			}
			foreach (var node in parkAndRideNodes) {
				// only look at nodes with positive capacity and (JLB 201508) auto parking nodes 
				// if (node.Capacity < Constants.EPSILON || !(node.Auto == 1)) {       //Added this logic to declaration/selection of parkAndRideNodes above
				//	continue;
				//}

				// use the nearest stop area for transit LOS  
				var parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
				var parkAndRideZoneId = node.ZoneId;
				var parkAndRideStopAreaKey = node.NearestStopAreaId;
				var parkAndRideStopArea = Global.TransitStopAreaMapping[node.NearestStopAreaId];
				//JLB 201508 logic in followign line replaced with new cost logic further below
				var parkAndRideParkingCost = 0.0;   //node.Cost / 100.0;    // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

				//test distance to park and ride against user-set limits
				var zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

				if (zzDist > maxDistanceUnitsToDrive) {
					continue;
				}


				if (zzDist / Math.Max(zzDist, 1.0) > maxDistanceRatio) {
					continue;
				}
				var duration = 0.0;
				if (node.ParkingTypeId == 2 || node.ParkingTypeId == 3) {
					if (_returnTime <= 0) {
						if (_purpose == Global.Settings.Purposes.Work) {
							duration = 8.0;
						}
						else if (_purpose == Global.Settings.Purposes.School) {
							duration = 6.0;
						}
						else if (_purpose == Global.Settings.Purposes.Social) {
							duration = 3.0;
						}
						else {
							duration = 2.0;
						}
					}
					else {
						duration = (_returnTime - _outboundTime) / 60.0;
					}
				}
				if (node.ParkingTypeId == 3 && duration > 2.0 + Constants.EPSILON) {
					continue;      // parking duration limited to 2 hours
				}
				else {
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

				var oStationWalkDistance = 2.0 * node.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits
				var oStationWalkTime = oStationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

				var autoPath = GetAutoPath(Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, useZones, originZoneId, parkAndRideZoneId, _originParcel, parkAndRideParcel);

				//loop on stop areas near destination
				for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
					var dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
					var dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
					var dWalkLength = Global.ParcelStopAreaLengths[dIndex];
					if (dWalkLength > maxStopAreaLength) {
						continue;
					}

					var destinationWalkDistance = 2 * dWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
					var destinationWalkTime = destinationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

					var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dStopArea, _transitPassOwnership);
					if (!transitPath.Available) {
						continue;
					}

					var parkMinute = (int) (_outboundTime - (transitPath.Time / 2.0) - 3); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

					// set utility
					var nodePathTime = transitPath.Time + autoPath.Time + oStationWalkTime + destinationWalkTime;
					var nodePathDistance = autoPath.Distance + transitPath.Distance + oStationWalkDistance + destinationWalkDistance;
					var nodePathCost = transitPath.Cost + autoPath.Cost + parkAndRideParkingCost;

					if (nodePathTime > pathTimeLimit) {
						continue;
					}

					var nodeUtility = transitPath.Utility + autoPath.Utility +
						Global.Configuration.PathImpedance_PathChoiceScaleFactor *
						(_tourCostCoefficient * parkAndRideParkingCost +
						_tourTimeCoefficient * Global.Configuration.PathImpedance_TransitAccessEgressTimeWeight * (destinationWalkTime + oStationWalkTime));

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
				}
			}
			_pathBikeCost[pathType] = 0;
			_pathBikeTime[pathType] = 0;
			_pathBikeDistance[pathType] = 0;
		}

		private void RunStopAreaParkAndRideModelOld(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> parkAndRideNodes;

			//if (Global.Configuration.ShouldReadParkAndRideNodeSkim) {
			//	var nodeId =
			//		useZones
			//			? (int) ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originZoneId, _destinationZoneId).Variable
			//			: (int) ImpedanceRoster.GetValue("przone", skimMode, pathType, votValue, _outboundTime, _originParcel, _destinationParcel).Variable;

			//	var node = ChoiceModelFactory.ParkAndRideNodeDao.Get(nodeId);

			//	parkAndRideNodes = new List<IParkAndRideNodeWrapper> { node };
			//}
			//else {
			parkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 1);
			//}

			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestNodeUtility = -99999D;
			var originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;

			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide : 99999;
			double maxDistanceUnitsToDrive = (Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide > 0) ? Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide : 999D;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);

			if (dFirst <= 0) {
				return;
			}
			foreach (var node in parkAndRideNodes) {
				// only look at nodes with positive capacity and (JLB 201508) auto parking nodes 
				// if (node.Capacity < Constants.EPSILON || !(node.Auto == 1)) {       //Added this logic to declaration/selection of parkAndRideNodes above
				//	continue;
				//}

				// use the nearest stop area for transit LOS  
				var parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
				var parkAndRideZoneId = node.ZoneId;
				var parkAndRideStopAreaKey = node.NearestStopAreaId;
				var parkAndRideStopArea = Global.TransitStopAreaMapping[node.NearestStopAreaId];
				//JLB 201508 logic in followign line replaced with new cost logic further below
				var parkAndRideParkingCost = 0.0;   //node.Cost / 100.0;    // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

				//test distance to park and ride against user-set limits
				var zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

				if (zzDist > maxDistanceUnitsToDrive) {
					continue;
				}

				var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, _destinationZoneId).Variable;

				if (zzDist / Math.Max(zzDist, 1.0) > maxDistanceRatio) {
					continue;
				}
				var duration = 0.0;
				if (node.ParkingTypeId == 2 || node.ParkingTypeId == 3) {
					if (_returnTime <= 0) {
						if (_purpose == Global.Settings.Purposes.Work) {
							duration = 8.0;
						}
						else if (_purpose == Global.Settings.Purposes.School) {
							duration = 6.0;
						}
						else if (_purpose == Global.Settings.Purposes.Social) {
							duration = 3.0;
						}
						else {
							duration = 2.0;
						}
					}
					else {
						duration = (_returnTime - _outboundTime) / 60.0;
					}
				}
				if (node.ParkingTypeId == 3 && duration > 2.0 + Constants.EPSILON) {
					continue;      // parking duration limited to 2 hours
				}
				else {
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

				var stationWalkTime = 2.0 * Global.PathImpedance_WalkMinutesPerDistanceUnit * node.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits

				var circuityDistance =
						(zzDist > Global.Configuration.MaximumBlendingDistance)
						? Constants.DEFAULT_VALUE
						: (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
							? _originParcel.NodeToNodeDistance(parkAndRideParcel, batchNumber)
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
					var dWalkLength = Global.ParcelStopAreaLengths[dIndex];
					if (dWalkLength > maxStopAreaLength) {
						continue;
					}



					var destinationWalkTime = Global.PathImpedance_WalkMinutesPerDistanceUnit * dWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
					destinationWalkTime *= 2; //round trip

					var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dStopArea, _transitPassOwnership);
					if (!transitPath.Available) {
						continue;
					}

					var parkMinute = (int) (_outboundTime - (transitPath.Time / 2.0) - 3); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.

					// set utility
					var nodePathTime = transitPath.Time + driveTime + stationWalkTime + destinationWalkTime;
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
						 Global.Configuration.PathImpedance_TransitAccessEgressTimeWeight * (destinationWalkTime + stationWalkTime)));

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
					_utility[pathType] = nodeUtility;
					_expUtility[pathType] = nodeUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : nodeUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(nodeUtility);
				}
			}
		}


		public class TransitPath {
			public bool Available { get; set; }
			public double Time { get; set; }
			public double Distance { get; set; }
			public double Cost { get; set; }
			public double Boardings1 { get; set; }
			public double Boardings2 { get; set; }
			public double Utility { get; set; }
		}

		private PathTypeModel_Actum.TransitPath GetTransitPath(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, int transitPassOwnership) {

			var path = new PathTypeModel_Actum.TransitPath();
			path.Available = true;

			// check for presence of valid path
			var outboundInVehicleDistance = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var returnInVehicleDistance = returnTime > 0
					? ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
					: 0;

			if (outboundInVehicleDistance < Constants.EPSILON || (returnTime > 0 && returnInVehicleDistance < Constants.EPSILON)) {
				path.Available = false;
				return path;
			}
			// valid path(s).  Proceed.

			path.Distance = outboundInVehicleDistance + returnInVehicleDistance;

			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (returnTime > 0 ? 2 : 1);

			//Actum:  nonlinear utility with submode-speciifc time components in a generic transit mode (labeled local bus)
			//get skim values
			var firstAndHiddenWaitTime = ImpedanceRoster.GetValue("firstandhiddenwaittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var waitTime = ImpedanceRoster.GetValue("waittime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;  // was transfer wait time
			var fareCard10 = ImpedanceRoster.GetValue("farecard10", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var fareCardMonth = ImpedanceRoster.GetValue("farecardmonth", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			//var accessEgressTime = ImpedanceRoster.GetValue("accegrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var fTime = ImpedanceRoster.GetValue("invehtimeferry", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var gTime = ImpedanceRoster.GetValue("invehtimemetro", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var bTime = ImpedanceRoster.GetValue("invehtimebus", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var pTime = ImpedanceRoster.GetValue("invehtimelrandlocaltrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var rTime = ImpedanceRoster.GetValue("invehtimeretrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var sTime = ImpedanceRoster.GetValue("invehtimestrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var iTime = ImpedanceRoster.GetValue("invehtimeictrain", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var noOfChanges = ImpedanceRoster.GetValue("noofchanges", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
			var transferWalkTime = ImpedanceRoster.GetValue("transferwalktime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;

			// add return LOS, if valid _departureTime passed			
			if (_returnTime > 0) {
				firstAndHiddenWaitTime += ImpedanceRoster.GetValue("firstandhiddenwaittime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				waitTime += ImpedanceRoster.GetValue("waittime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				fareCard10 += ImpedanceRoster.GetValue("farecard10", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				fareCardMonth += ImpedanceRoster.GetValue("farecardmonth", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				//accessEgressTime += ImpedanceRoster.GetValue("accegrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				fTime += ImpedanceRoster.GetValue("invehtimeferry", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				gTime += ImpedanceRoster.GetValue("invehtimemetro", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				bTime += ImpedanceRoster.GetValue("invehtimebus", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				pTime += ImpedanceRoster.GetValue("invehtimelrandlocaltrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				rTime += ImpedanceRoster.GetValue("invehtimeretrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				sTime += ImpedanceRoster.GetValue("invehtimestrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				iTime += ImpedanceRoster.GetValue("invehtimeictrain", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				noOfChanges += ImpedanceRoster.GetValue("noofchanges", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
				transferWalkTime = ImpedanceRoster.GetValue("transferwalktime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
			}

			var fare = transitPassOwnership == 1 ? 0.0 : fareCard10; // assumes that marginal cost is zero for pass holders
			fare = fare * (1.0 - _transitDiscountFraction); //fare adjustment

			//determine utility components 
			//accessEgressTime = accessEgressTime
			var trainTime = rTime + sTime + iTime;
			var busTime = bTime + fTime;
			var metroTime = gTime;
			var lightRailTime = pTime;

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
			}
			else if (_purpose == Global.Settings.Purposes.Business) {
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
			}
			else if (aggregatePurpose == 2) {
				inVehicleTimeWeight_Train = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_Train;
				inVehicleTimeWeight_Bus = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_Bus;
				inVehicleTimeWeight_Metro = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_Metro;
				inVehicleTimeWeight_LightRail = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_LightRail;
			}
			else {
				inVehicleTimeWeight_Train = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_Train;
				inVehicleTimeWeight_Bus = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_Bus;
				inVehicleTimeWeight_Metro = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_Metro;
				inVehicleTimeWeight_LightRail = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_LightRail;
			}
			var weightedInVehicleTime = trainTime * inVehicleTimeWeight_Train
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
				 + Global.Configuration.PathImpedance_TransitAccessEgressTimeWeight * transferWalkTime
			  ));
			return path;
		}


		private static double GetTransitWalkTime(IParcelWrapper parcel, int pathType, double boardings) {
			var walkDist = parcel.DistanceToLocalBus; // default is local bus (feeder), for any submode

			double altDist;

			if (pathType == Global.Settings.PathTypes.LightRail) {
				altDist = parcel.DistanceToLightRail;
			}
			else if (pathType == Global.Settings.PathTypes.PremiumBus) {
				altDist = parcel.DistanceToExpressBus;
			}
			else if (pathType == Global.Settings.PathTypes.CommuterRail) {
				altDist = parcel.DistanceToCommuterRail;
			}
			else if (pathType == Global.Settings.PathTypes.Ferry) {
				altDist = parcel.DistanceToFerry;
			}
			else {
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

		public class AutoPath {
			public bool Available { get; set; }
			public double Time { get; set; }
			public double Distance { get; set; }
			public double Cost { get; set; }
			public double Utility { get; set; }
		}

		private PathTypeModel_Actum.AutoPath GetAutoPath(int skimMode, int pathType, double votValue, bool useZones, int originZoneId, int destinationZoneId, IParcelWrapper originParcel, IParcelWrapper destinationParcel) {

			var path = new PathTypeModel_Actum.AutoPath();
			path.Available = true;

			//if full network path and no-tolls path exists check for duplicate
			var tollConstant = 0D;
			if (pathType == Global.Settings.PathTypes.FullNetwork && ImpedanceRoster.IsActualCombination(skimMode, Global.Settings.PathTypes.NoTolls)) {
				var noTollCost =
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
			var zzDist = ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId).Variable;
			int batchNumber = ParallelUtility.GetBatchFromThreadId();
			var circuityDistance =
				(zzDist > Global.Configuration.MaximumBlendingDistance)
					? Constants.DEFAULT_VALUE
					: (!useZones && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
						? originParcel.NodeToNodeDistance(destinationParcel, batchNumber)
						: (!useZones && Global.Configuration.UseShortDistanceCircuityMeasures)
							? originParcel.CircuityDistance(destinationParcel)
							: Constants.DEFAULT_VALUE;

			//calculate times and distances outbound
			var freeFlowSkimValue =
				useZones
					? ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId)
					: ImpedanceRoster.GetValue("ivtfree", skimMode, pathType, votValue, _outboundTime, originParcel, destinationParcel, circuityDistance);
			var freeFlowTime = freeFlowSkimValue.Variable;
			var distance1 =
				freeFlowSkimValue.BlendVariable;
			//				useZones
			//					? ImpedanceRoster.GetValue("distance", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId).Variable
			//					: freeFlowSkimValue.BlendVariable;
			var extraSkimValue =
			useZones
				? ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId)
				: ImpedanceRoster.GetValue("ivtextra", skimMode, pathType, votValue, _outboundTime, originParcel, destinationParcel, circuityDistance);
			var extraTime = extraSkimValue.Variable;

			var searchMinutes = 0.0;
			if (!useZones) {
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
			var distance2 = 0.0;
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
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);

			if (path.Time > pathTimeLimit || path.Time < Constants.EPSILON) {
				path.Available = false;
				return path;
			}

			path.Distance = distance1 + distance2;

			//calculate costs outbound
			var cost1 =
				useZones
					? ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, originZoneId, destinationZoneId).Variable
					: ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _outboundTime, originParcel, destinationParcel).Variable;
			//implement mileage-based pricing policy
			if (Global.Configuration.Policy_TestMilageBasedPricing) {
				var minutesAfterMidnight = _outboundTime + 180;
				var centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
					 ? Global.Configuration.Policy_CentsPerMileInAMPeak :
						(minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
						  ? Global.Configuration.Policy_CentsPerMileInPMPeak :
							 (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
								? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
				cost1 += distance1 * centsPerMile / 100.0;
			}

			//calculate costs on return
			var cost2 = 0.0;
			if (_returnTime > 0) {
				cost2 +=
					useZones
						? ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, destinationZoneId, originZoneId).Variable
						: ImpedanceRoster.GetValue("toll", skimMode, pathType, votValue, _returnTime, destinationParcel, originParcel).Variable;
				//implement mileage-based pricing policy
				if (Global.Configuration.Policy_TestMilageBasedPricing) {
					var minutesAfterMidnight = _returnTime + 180;
					var centsPerMile = (minutesAfterMidnight >= Global.Configuration.Policy_AMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_AMPricingPeriodEnd)
						 ? Global.Configuration.Policy_CentsPerMileInAMPeak :
							(minutesAfterMidnight >= Global.Configuration.Policy_PMPricingPeriodStart && minutesAfterMidnight <= Global.Configuration.Policy_PMPricingPeriodEnd)
							  ? Global.Configuration.Policy_CentsPerMileInPMPeak :
								 (minutesAfterMidnight > Global.Configuration.Policy_AMPricingPeriodEnd && minutesAfterMidnight < Global.Configuration.Policy_PMPricingPeriodStart)
									? Global.Configuration.Policy_CentsPerMileBetweenPeaks : Global.Configuration.Policy_CentsPerMileOutsidePeaks;
					cost2 += distance2 * centsPerMile / 100.0;
				}
			}
			path.Cost = cost1 + cost2 + path.Distance * Global.PathImpedance_AutoOperatingCostPerDistanceUnit;

			//calculate time utility
			var gammaFreeFlowTime = GammaFunction(freeFlowTime, Global.Configuration.PathImpedance_Gamma_InVehicleTime);
			extraTime = Global.Configuration.Policy_CongestedTravelTimeMultiplier != 0 ? extraTime * Global.Configuration.Policy_CongestedTravelTimeMultiplier : extraTime;
			var gammaExtraTime = GammaFunction(extraTime, Global.Configuration.PathImpedance_Gamma_ExtraTime);
			//determine time weight
			//extra time weight for driver and passenger
			double inVehicleExtraTimeWeight;
			if (skimMode == Global.Settings.Modes.HovPassenger) {
				inVehicleExtraTimeWeight = Global.Configuration.PathImpedance_InVehicleExtraTimeWeight_Passenger;
			}
			else {
				inVehicleExtraTimeWeight = Global.Configuration.PathImpedance_InVehicleExtraTimeWeight_Driver;
			}
			//weights for purpose x mode
			int aggregatePurpose = 0;
			double inVehicleTimeWeight;
			if (_purpose == Global.Settings.Purposes.Work || _purpose == Global.Settings.Purposes.School || _purpose == Global.Settings.Purposes.Escort) {
				aggregatePurpose = 1;
			}
			else if (_purpose == Global.Settings.Purposes.Business) {
				aggregatePurpose = 2;
			}
			if (skimMode == Global.Settings.Modes.Sov) {
				if (aggregatePurpose == 1) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_SOV;
				}
				else if (aggregatePurpose == 2) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_SOV;
				}
				else {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_SOV;
				}
			}
			else if (skimMode == Global.Settings.Modes.HovDriver) {
				if (aggregatePurpose == 1) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_HOVDriver;
				}
				else if (aggregatePurpose == 2) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_HOVDriver;
				}
				else {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_HOVDriver;
				}
			}
			else {
				if (aggregatePurpose == 1) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Commute_HOVPassenger;
				}
				else if (aggregatePurpose == 2) {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Business_HOVPassenger;
				}
				else {
					inVehicleTimeWeight = Global.Configuration.PathImpedance_InVehicleTimeWeight_Personal_HOVPassenger;
				}
			}

			//calculate cost utility
			double gammaCost;
			double distanceCoefficient;
			double modeConstant;
			if (skimMode == Global.Settings.Modes.HovPassenger && !Global.Configuration.HOVPassengersIncurCosts) {
				gammaCost = 0;
			}
			else {
				gammaCost = GammaFunction(path.Cost, Global.Configuration.PathImpedance_Gamma_Cost);
			}

			//combine time and cost utility
			path.Utility = Global.Configuration.PathImpedance_PathChoiceScaleFactor
				* (_tourCostCoefficient * gammaCost
				+ _tourTimeCoefficient * inVehicleTimeWeight
				* (gammaFreeFlowTime + gammaExtraTime * inVehicleExtraTimeWeight)
				+ tollConstant);

			return path;

		}

		private void RunStopAreaCarParkRideBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> carParkAndRideNodes;
			carParkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 1);
			IEnumerable<IParkAndRideNodeWrapper> bikeParkAndRideNodes;
			bikeParkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 0);

			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestPathUtility = -99999D;
			var originZoneId = useZones ? _originZoneId : _originParcel.ZoneId;
			var destinationZoneId = useZones ? _destinationZoneId : _destinationParcel.ZoneId;
			var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, destinationZoneId).Variable;


			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
			int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
			double maxDistanceUnitsToDrive = (Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide > 0) ? Global.Configuration.MaximumDistanceUnitsToDriveToParkAndRide : 999D;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;

			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);

			if (dFirst <= 0) {
				return;
			}
			foreach (var node in carParkAndRideNodes) {

				// use the nearest stop area for transit LOS  
				var parkAndRideParcel = ChoiceModelFactory.Parcels[node.NearestParcelId];
				var parkAndRideZoneId = node.ZoneId;
				var parkAndRideStopAreaKey = node.NearestStopAreaId;
				var parkAndRideStopArea = Global.TransitStopAreaMapping[node.NearestStopAreaId];
				//JLB 201508 logic in followign line replaced with new cost logic further below
				var carParkAndRideParkingCost = 0.0;   //node.Cost / 100.0;    // converts hundredths of Monetary Units to Monetary Units  // JLBscale: changed comment from cents and dollars

				//test distance to park and ride against user-set limits
				var zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originZoneId, parkAndRideZoneId).Variable;

				if (zzDist > maxDistanceUnitsToDrive) {
					continue;
				}

				if (zzDist / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
					continue;
				}
				var duration = 0.0;
				if (node.ParkingTypeId == 2 || node.ParkingTypeId == 3) { // TODO:  should make list of settings or constants for node.ParkingTypeIs, as is done for e.g. Purposes
					if (_returnTime <= 0) {
						if (_purpose == Global.Settings.Purposes.Work) {
							duration = 8.0;
						}
						else if (_purpose == Global.Settings.Purposes.School) {
							duration = 6.0;
						}
						else if (_purpose == Global.Settings.Purposes.Social) {
							duration = 3.0;
						}
						else {
							duration = 2.0;
						}
					}
					else {
						duration = (_returnTime - _outboundTime) / 60.0;
					}
				}
				if (node.ParkingTypeId == 3 && duration > 2.0 + Constants.EPSILON) {
					continue;      // parking duration limited to 2 hours for TypeId 3 lot so this lot is not available
				}
				else {
					carParkAndRideParkingCost =
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

				var oStationWalkDistance = 2.0 * node.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits
				var oStationWalkTime = oStationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

				var autoPath = GetAutoPath(Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, useZones, originZoneId, parkAndRideZoneId, _originParcel, parkAndRideParcel);

				//loop on bike parking nodes near destination
				for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
					if (dIndex < 0) {
						continue;
					}
					var dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
					if (dBikeLength > maxStopAreaBikeLength) {
						break;
					}
					if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
						dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
						continue;
					}
					var dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
					if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
						break;
					}
					dBikeDistance *= 2;  //round trip
					var dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
					var dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
					var dStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == dStopAreaNodeId);
					if (dStopAreaNodes == null) {
						continue;
					}
					var dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
					//loop on stop areas associated with the dnode
					foreach (var dStopAreaNode in dStopAreaNodes) {
						var dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];

						var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, parkAndRideStopArea, dParkAndRideStopArea, _transitPassOwnership);
						if (!transitPath.Available) {
							continue;
						}

						var parkMinute = (int) (_outboundTime - (transitPath.Time / 2.0) - 3); // estimate of change mode activity time, same as assumed when setting trip departure time in ChoiceModelRunner.
						var dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
						var dParkAndRideParkingCost = Math.Min(dStopAreaNode.Cost, dStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost

						var dStationWalkDistance = 2.0 * dStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
						var dStationWalkTime = dStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

						// set utility  
						var pathTime = transitPath.Time + autoPath.Time + oStationWalkTime + dStationWalkTime + dBikeTime;
						var pathDistance = autoPath.Distance + oStationWalkDistance + transitPath.Distance + dStationWalkDistance + dBikeDistance;
						var pathCost = transitPath.Cost + autoPath.Cost + carParkAndRideParkingCost + dParkAndRideParkingCost;

						if (pathTime > pathTimeLimit) {
							continue;
						}

						var pathUtility = transitPath.Utility + autoPath.Utility +
							Global.Configuration.PathImpedance_PathChoiceScaleFactor *
							(_tourCostCoefficient *
								(carParkAndRideParkingCost + dParkAndRideParkingCost) +
							_tourTimeCoefficient *
								(Global.Configuration.PathImpedance_WalkTimeWeight * (oStationWalkTime + dStationWalkTime)
								+ Global.Configuration.PathImpedance_BikeTimeWeight * dBikeTime
								)
							);

						if (Global.Configuration.ShouldUseParkAndRideShadowPricing && !Global.Configuration.IsInEstimationMode) {
							pathUtility += node.ShadowPrice[parkMinute];
						}

						// if the best path so far, reset pathType properties
						if (pathUtility <= bestPathUtility) {
							continue;
						}
						bestPathUtility = pathUtility;
						_pathParkAndRideNodeId[pathType] = node.Id;
						_pathOriginStopAreaKey[pathType] = parkAndRideStopAreaKey;
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
						_pathBikeTime[pathType] = dBikeTime;
						_pathBikeDistance[pathType] = dBikeDistance;
						_pathBikeCost[pathType] = dParkAndRideParkingCost;
						_utility[pathType] = pathUtility;
						_expUtility[pathType] = pathUtility > MAX_UTILITY ? Math.Exp(MAX_UTILITY) : pathUtility < MIN_UTILITY ? Math.Exp(MIN_UTILITY) : Math.Exp(pathUtility);
						//}
					}
				}
			}
		}


		private void RunStopAreaBikeParkRideWalkModel(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			//int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> bikeParkAndRideNodes;
			bikeParkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 0);
			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestPathUtility = -99999D;
			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
			int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
			int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsParkAndRide : 99999;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInStopAreaDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInStopAreaDistanceArray, dFirst + maxStopAreasToSearch - 1);
			if (dFirst <= 0) {
				return;
			}

			//_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var oFirst = _originParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oFirst + maxStopAreasToSearch - 1);
			if (oFirst <= 0) {
				return;
			}
			// set parcel indexes for origin and destination parcels for getting microzone skim values
			var originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
			var destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
			var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

			//loop on bike parking nodes near origin
			for (var oIndex = oFirst; oIndex <= oLast; oIndex++) {
				var oBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[oIndex];
				if (oBikeLength > maxStopAreaBikeLength) {
					break;
				}
				if (oBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
					oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oLast + 1);
					continue;
				}
				var oBikeDistance = oBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
				if (oBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
					break;
				}
				oBikeDistance *= 2;  //round trip
				var oStopAreaNodeId = Global.ParcelParkAndRideNodeIds[oIndex];
				var oStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == oStopAreaNodeId);
				if (oStopAreaNodes == null) {
					continue;
				}
				var oBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * oBikeDistance;
				//loop on stop areas associated with the onode
				foreach (var oStopAreaNode in oStopAreaNodes) {
					var oParkAndRideStopArea = Global.TransitStopAreaMapping[oStopAreaNode.NearestStopAreaId];
					var oParkAndRideStopAreaKey = oStopAreaNode.NearestStopAreaId;
					var oParkAndRideParkingCost = Math.Min(oStopAreaNode.Cost, oStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
					var oStationWalkDistance = 2.0 * oStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280 * Global.Settings.DistanceUnitsPerMile;  // in DistanceUnits
					var oStationWalkTime = oStationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;

					//loop on stop areas near destination
					for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
						var dStopArea = Global.ParcelStopAreaStopAreaIds[dIndex];
						var dStopAreaKey = Global.ParcelStopAreaStopAreaKeys[dIndex];
						var dWalkLength = Global.ParcelStopAreaLengths[dIndex];
						if (dWalkLength > maxStopAreaLength) {
							break;
						}

						var destinationWalkDistance = 2 * dWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
						var destinationWalkTime = destinationWalkDistance * Global.PathImpedance_WalkMinutesPerDistanceUnit;
						var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oParkAndRideStopArea, dStopArea, _transitPassOwnership);
						if (!transitPath.Available) {
							continue;
						}

						// set utility  
						var pathTime = oBikeTime + oStationWalkTime + transitPath.Time + destinationWalkTime;
						var pathDistance = oBikeDistance + oStationWalkDistance + transitPath.Distance + destinationWalkDistance;
						var pathCost = oParkAndRideParkingCost + transitPath.Cost;
						if (pathTime > pathTimeLimit) {
							continue;
						}

						var pathUtility = transitPath.Utility +
							Global.Configuration.PathImpedance_PathChoiceScaleFactor *
							(_tourCostCoefficient *
								(oParkAndRideParkingCost) +
							_tourTimeCoefficient *
								(Global.Configuration.PathImpedance_WalkTimeWeight * (oStationWalkTime + destinationWalkTime)
								+ Global.Configuration.PathImpedance_BikeTimeWeight * (oBikeTime)
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
					}
				}
			}
		}
		//		}

		private void RunStopAreaBikeParkRideBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			//int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> bikeParkAndRideNodes;
			bikeParkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 0);
			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestPathUtility = -99999D;
			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
			int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);
			if (dFirst <= 0) {
				return;
			}

			//_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var oFirst = _originParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oFirst + maxStopAreasToSearch - 1);
			if (oFirst <= 0) {
				return;
			}
			// set parcel indexes for origin and destination parcels for getting microzone skim values
			var originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
			var destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
			var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

			//loop on bike parking nodes near origin
			for (var oIndex = oFirst; oIndex <= oLast; oIndex++) {
				var oBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[oIndex];
				if (oBikeLength > maxStopAreaBikeLength) {
					break;
				}
				if (oBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
					oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oLast + 1);
					continue;
				}
				var oBikeDistance = oBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
				if (oBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
					break;
				}
				oBikeDistance *= 2;  //round trip
				var oStopAreaNodeId = Global.ParcelParkAndRideNodeIds[oIndex];
				var oStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == oStopAreaNodeId);
				if (oStopAreaNodes == null) {
					continue;
				}
				var oBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * oBikeDistance;
				//loop on stop areas associated with the onode
				foreach (var oStopAreaNode in oStopAreaNodes) {
					var oParkAndRideStopArea = Global.TransitStopAreaMapping[oStopAreaNode.NearestStopAreaId];
					var oParkAndRideStopAreaKey = oStopAreaNode.NearestStopAreaId;
					var oParkAndRideParkingCost = Math.Min(oStopAreaNode.Cost, oStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
					var oStationWalkDistance = 2.0 * oStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
					var oStationWalkTime = oStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

					//loop on bike parking nodes near destination
					for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
						var dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
						if (dBikeLength > maxStopAreaBikeLength) {
							break;
						}
						if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
							dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
							continue;
						}
						var dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
						if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
							break;
						}
						dBikeDistance *= 2;  //round trip
						var dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
						var dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
						var dStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == dStopAreaNodeId);
						if (dStopAreaNodes == null) {
							continue;
						}
						var dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
						//loop on stop areas associated with the dnode
						foreach (var dStopAreaNode in dStopAreaNodes) {
							var dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];
							var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oParkAndRideStopArea, dParkAndRideStopArea, _transitPassOwnership);
							if (!transitPath.Available) {
								continue;
							}
							var dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
							var dParkAndRideParkingCost = Math.Min(dStopAreaNode.Cost, dStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
							var dStationWalkDistance = 2.0 * dStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
							var dStationWalkTime = dStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

							// set utility  
							var pathTime = oBikeTime + oStationWalkTime + transitPath.Time + dStationWalkTime + dBikeTime;
							var pathDistance = oBikeDistance + oStationWalkDistance + transitPath.Distance + dStationWalkDistance + dBikeDistance;
							var pathCost = oParkAndRideParkingCost + transitPath.Cost + dParkAndRideParkingCost;
							if (pathTime > pathTimeLimit) {
								continue;
							}

							var pathUtility = transitPath.Utility +
								Global.Configuration.PathImpedance_PathChoiceScaleFactor *
								(_tourCostCoefficient *
									(oParkAndRideParkingCost + dParkAndRideParkingCost) +
								_tourTimeCoefficient *
									(Global.Configuration.PathImpedance_WalkTimeWeight * (oStationWalkTime + dStationWalkTime)
									+ Global.Configuration.PathImpedance_BikeTimeWeight * (oBikeTime + dBikeTime)
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
						}
					}
				}
			}
		}
		//		}

		private void RunStopAreaWalkRideBikeModel(int skimMode, int pathType, double votValue, bool useZones) {
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			//int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> bikeParkAndRideNodes;
			bikeParkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 0);
			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestPathUtility = -99999D;
			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnits > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnits : 99999;
			int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
			int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);
			if (dFirst <= 0) {
				return;
			}

			//_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var oFirst = _originParcel.FirstPositionInStopAreaDistanceArray;
			var oLast = Math.Min(_originParcel.LastPositionInStopAreaDistanceArray, oFirst + maxStopAreasToSearch - 1);
			if (oFirst <= 0) {
				return;
			}
			// set parcel indexes for origin and destination parcels for getting microzone skim values
			var originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
			var destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
			var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

			//loop on PT stop areas near origin
			for (var oIndex = oFirst; oIndex <= oLast; oIndex++) {
				var oStopArea = Global.ParcelStopAreaStopAreaIds[oIndex];
				var oStopAreaKey = Global.ParcelStopAreaStopAreaKeys[oIndex];
				var oWalkLength = Global.ParcelStopAreaLengths[oIndex];
				if (oWalkLength > maxStopAreaLength) {
					break;
				}

				var oWalkDistance = 2 * oWalkLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;

				var oWalkTime = Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit * oWalkDistance;

				//loop on bike parking nodes near destination
				for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
					var dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
					if (dBikeLength > maxStopAreaBikeLength) {
						break;
					}
					if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
						dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
						continue;
					}
					var dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
					if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
						break;
					}
					dBikeDistance *= 2;  //round trip
					var dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
					var dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
					var dStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == dStopAreaNodeId);
					if (dStopAreaNodes == null) {
						continue;
					}
					var dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
					//loop on stop areas associated with the dnode
					foreach (var dStopAreaNode in dStopAreaNodes) {
						var dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];
						var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oStopArea, dParkAndRideStopArea, _transitPassOwnership);
						if (!transitPath.Available) {
							continue;
						}
						var dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
						var dParkAndRideParkingCost = Math.Min(dStopAreaNode.Cost, dStopAreaNode.CostAnnual / 100.0); //assume that cost for tour is one hundredth of annual cost
						var dStationWalkDistance = 2.0 * dStopAreaNode.LengthToStopArea / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
						var dStationWalkTime = dStationWalkDistance * Global.Configuration.PathImpedance_WalkMinutesPerDistanceUnit;

						// set utility  
						var pathTime = oWalkTime + transitPath.Time + dStationWalkTime + dBikeTime;
						var pathDistance = oWalkDistance + transitPath.Distance + dStationWalkDistance + dBikeDistance;
						var pathCost = transitPath.Cost + dParkAndRideParkingCost;
						if (pathTime > pathTimeLimit) {
							continue;
						}

						var pathUtility = transitPath.Utility +
							Global.Configuration.PathImpedance_PathChoiceScaleFactor *
							(_tourCostCoefficient *
								(dParkAndRideParkingCost) +
							_tourTimeCoefficient *
								(Global.Configuration.PathImpedance_WalkTimeWeight * (oWalkTime + dStationWalkTime)
								+ Global.Configuration.PathImpedance_BikeTimeWeight * (dBikeTime)
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
					}
				}
			}
		}
		//}}

		private void RunStopAreaBikeOnTransitModel(int skimMode, int pathType, double votValue, bool useZones) {
			var stopAreaReader =
				Global
					.Kernel
					.Get<IPersistenceFactory<ITransitStopArea>>()
					.Reader;
			var eligibleTerminals = stopAreaReader.Where(s => s.BikeOnBoardTerminal == 1).ToDictionary(z => z.Key, z => z);
			if (ChoiceModelFactory.ParkAndRideNodeDao == null || _returnTime <= 0) {
				return;
			}
			//int batchNumber = ParallelUtility.GetBatchFromThreadId();
			IEnumerable<IParkAndRideNodeWrapper> bikeParkAndRideNodes;
			bikeParkAndRideNodes = ChoiceModelFactory.ParkAndRideNodeDao.Nodes.Where(n => n.Capacity >= Constants.EPSILON && n.Auto == 0);
			// valid node(s), and tour-level call  
			var pathTimeLimit = Global.Configuration.PathImpedance_AvailablePathUpperTimeLimit * (_returnTime > 0 ? 2 : 1);
			var bestPathUtility = -99999D;
			//user-set limits on search - use high values if not set
			int maxStopAreasToSearch = (Global.Configuration.MaximumStopAreasToSearchParkAndRide > 0) ? Global.Configuration.MaximumStopAreasToSearchParkAndRide : 99;
			int maxStopAreaBikeLength = (Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MaximumParcelToStopAreaLengthUnitsToBike : 99999;
			int minStopAreaBikeLength = (Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike > 0) ? Global.Configuration.MinimumParcelToStopAreaLengthUnitsToBike : 0;
			double maxDistanceRatio = (Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination > 0) ? Global.Configuration.MaximumRatioDriveToParkAndRideVersusDriveToDestination : 99D;
			//_destinationParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var dFirst = _destinationParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dFirst + maxStopAreasToSearch - 1);
			if (dFirst <= 0) {
				return;
			}

			//_doriginParcel. SetFirstAndLastStopAreaDistanceIndexes();
			var oFirst = _originParcel.FirstPositionInParkAndRideNodeDistanceArray;
			var oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oFirst + maxStopAreasToSearch - 1);
			if (oFirst <= 0) {
				return;
			}
			// set parcel indexes for origin and destination parcels for getting microzone skim values
			var originParcelIndex = Global.MicrozoneMapping[_originParcel.Id];
			var destinationParcelIndex = Global.MicrozoneMapping[_destinationParcel.Id];
			var zzDist2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Bike, Global.Settings.PathTypes.FullNetwork, votValue, _outboundTime, originParcelIndex, destinationParcelIndex).Variable;

			//loop on bike parking nodes near origin
			for (var oIndex = oFirst; oIndex <= oLast; oIndex++) {
				var oBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[oIndex];
				if (oBikeLength > maxStopAreaBikeLength) {
					break;  // since nodes are in increasing distance order
				}
				if (oBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
					oLast = Math.Min(_originParcel.LastPositionInParkAndRideNodeDistanceArray, oLast + 1);
					continue;
				}
				var oBikeDistance = oBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;
				if (oBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
					break;   // since nodes are in increasing distance order
				}
				oBikeDistance *= 2;  //round trip
				var oStopAreaNodeId = Global.ParcelParkAndRideNodeIds[oIndex];
				var oStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == oStopAreaNodeId);
				if (oStopAreaNodes == null) {
					continue;
				}
				var oBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * oBikeDistance;
				//loop on stop areas associated with the onode
				foreach (var oStopAreaNode in oStopAreaNodes) {
					var oParkAndRideStopAreaKey = oStopAreaNode.NearestStopAreaId;
					if (eligibleTerminals.Values.FirstOrDefault(t => t.Key == oParkAndRideStopAreaKey) == default(ITransitStopArea)) {  // no bikeonboard terminals available at this node
						continue;
					}
					var oParkAndRideStopArea = Global.TransitStopAreaMapping[oStopAreaNode.NearestStopAreaId];

					//loop on bike parking nodes near destination
					for (var dIndex = dFirst; dIndex <= dLast; dIndex++) {
						var dBikeLength = Global.ParcelToBikeCarParkAndRideNodeLength[dIndex];
						if (dBikeLength > maxStopAreaBikeLength) {
							break;
						}
						if (dBikeLength < minStopAreaBikeLength) {    // This excludes very short bike distances and still allows up to the maximum number of allowed stop areas
							dLast = Math.Min(_destinationParcel.LastPositionInParkAndRideNodeDistanceArray, dLast + 1);
							continue;
						}
						var dBikeDistance = dBikeLength / Global.Settings.LengthUnitsPerFoot / 5280.0 * Global.Settings.DistanceUnitsPerMile;  // round trip
						if (dBikeDistance / Math.Max(zzDist2, 1.0) > maxDistanceRatio) {
							break;
						}
						dBikeDistance *= 2;  //round trip
						var dStopAreaNodeId = Global.ParcelParkAndRideNodeIds[dIndex];
						var dStopAreaNodeKey = Global.ParcelParkAndRideNodeSequentialIds[dIndex];
						var dStopAreaNodes = bikeParkAndRideNodes.Where(n => n.ZoneId == dStopAreaNodeId);
						if (dStopAreaNodes == null) {
							continue;
						}
						var dBikeTime = Global.Configuration.PathImpedance_BikeMinutesPerDistanceUnit * dBikeDistance;
						//loop on stop areas associated with the dnode
						foreach (var dStopAreaNode in dStopAreaNodes) {
							var dParkAndRideStopAreaKey = dStopAreaNode.NearestStopAreaId;
							if (eligibleTerminals.Values.FirstOrDefault(t => t.Key == dParkAndRideStopAreaKey) == default(ITransitStopArea)) {  // no bikeonboard terminals available at this node
								continue;
							}
							var dParkAndRideStopArea = Global.TransitStopAreaMapping[dStopAreaNode.NearestStopAreaId];
							var transitPath = GetTransitPath(skimMode, pathType, votValue, _outboundTime, _returnTime, oParkAndRideStopArea, dParkAndRideStopArea, _transitPassOwnership);
							if (!transitPath.Available) {
								continue;
							}

							// set utility  
							var pathTime = oBikeTime + transitPath.Time + dBikeTime;
							var pathDistance = oBikeDistance + transitPath.Distance + dBikeDistance;
							var pathCost = transitPath.Cost;
							if (pathTime > pathTimeLimit) {
								continue;
							}

							var pathUtility = transitPath.Utility +
								_tourTimeCoefficient *
									(Global.Configuration.PathImpedance_BikeTimeWeight * (oBikeTime + dBikeTime)
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
						}
					}
				}
			}
			_pathWalkTime[pathType] = 0;
			_pathWalkDistance[pathType] = 0;
			_pathBikeCost[pathType] = 0;
		}



		double GammaFunction(double x, double gamma) {
			double xGamma;
			xGamma = gamma * x + (1 - gamma) * Math.Log(Math.Max(x, 1.0));
			return xGamma;
		}
	}

}
