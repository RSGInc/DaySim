// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
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
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;
using Ninject;

namespace DaySim.ChoiceModels.Default.Models {
	public class SchoolTourModeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "SchoolTourModeModel";
		private const int TOTAL_NESTED_ALTERNATIVES = 5;
		private const int TOTAL_LEVELS = 2;
		private const int MAX_PARAMETER = 199;
		private const int THETA_PARAMETER = 99;

		private readonly int[] _nestedAlternativeIds = new[] {0, 19, 19, 20, 21, 21, 22, 0, 23};
		private readonly int[] _nestedAlternativeIndexes = new[] {0, 0, 0, 1, 2, 2, 3, 0, 4};
		
		private readonly ITourCreator _creator = 
			Global
			.Kernel
			.Get<IWrapperFactory<ITourCreator>>()
			.Creator;

		public override void RunInitialize(ICoefficientsReader reader = null)
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.SchoolTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(ITourWrapper tour) {
			if (tour == null) {
				throw new ArgumentNullException("tour");
			}
			
			tour.PersonDay.ResetRandom(40 + tour.Sequence - 1);

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(tour.Id);

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {
				if (tour.DestinationParcel == null || tour.Mode <= Global.Settings.Modes.None || (tour.Mode > Global.Settings.Modes.Transit && tour.Mode != Global.Settings.Modes.SchoolBus)) {
					return;
				}

				IEnumerable<dynamic> pathTypeModels =
					PathTypeModelFactory.Model.RunAll(
					tour.Household.RandomUtility,
						tour.OriginParcel,
						tour.DestinationParcel,
						tour.DestinationArrivalTime,
						tour.DestinationDepartureTime,
						tour.DestinationPurpose,
						tour.CostCoefficient,
						tour.TimeCoefficient,
						tour.Person.IsDrivingAge,
						tour.Household.VehiclesAvailable,
						tour.Person.GetTransitFareDiscountFraction(),
						false);

				var mode = (tour.Mode == Global.Settings.Modes.SchoolBus) ? Global.Settings.Modes.Hov3 : tour.Mode; // use HOV3 for school bus impedance
				var pathTypeModel = pathTypeModels.First(x => x.Mode == mode);

				if (!pathTypeModel.Available) {
					return;
				}

				RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Household.VehiclesAvailable, tour.Mode);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {
				IEnumerable<dynamic> pathTypeModels =
					PathTypeModelFactory.Model.RunAll(
					tour.Household.RandomUtility,
						tour.OriginParcel,
						tour.DestinationParcel,
						tour.DestinationArrivalTime,
						tour.DestinationDepartureTime,
						tour.DestinationPurpose,
						tour.CostCoefficient,
						tour.TimeCoefficient,
						tour.Person.IsDrivingAge,
						tour.Household.VehiclesAvailable,
						tour.Person.GetTransitFareDiscountFraction(),
						false);

				RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Household.VehiclesAvailable);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

				if (chosenAlternative == null) {
					Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
					tour.Mode = Global.Settings.Modes.Hov3;
					tour.PersonDay.IsValid = false;
					return;
				}

				var choice = (int) chosenAlternative.Choice;

				tour.Mode = choice;
				if (choice == Global.Settings.Modes.SchoolBus) {
					tour.PathType = 0;
				}
				else {
					var chosenPathType = pathTypeModels.First(x => x.Mode == choice);
					tour.PathType = chosenPathType.PathType;
					tour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
				}
			}
		}

		public ChoiceProbabilityCalculator.Alternative RunNested(IPersonWrapper person, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
			if (person == null) {
				throw new ArgumentNullException("person");
			}

			var tour = _creator.CreateWrapper(person, null, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.School);

			return RunNested(tour, destinationParcel, householdCars, 0.0);
		}

		public ChoiceProbabilityCalculator.Alternative RunNested(IPersonDayWrapper personDay, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
			if (personDay == null) {
				throw new ArgumentNullException("personDay");
			}

			var tour = _creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.School);

			return RunNested(tour, destinationParcel, householdCars, tour.Person.GetTransitFareDiscountFraction());
		}

		private ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel, int householdCars, double transitDiscountFraction) {
			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetNestedChoiceProbabilityCalculator();

            IEnumerable<dynamic> pathTypeModels =
				PathTypeModelFactory.Model.RunAll(
				tour.Household.RandomUtility,
					tour.OriginParcel,
					destinationParcel,
					tour.DestinationArrivalTime,
					tour.DestinationDepartureTime,
					tour.DestinationPurpose,
					tour.CostCoefficient,
					tour.TimeCoefficient,
					tour.Person.IsDrivingAge,
					householdCars,
					transitDiscountFraction,
					false);

			RunModel(choiceProbabilityCalculator, tour, pathTypeModels, destinationParcel, householdCars);

			return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IEnumerable<dynamic> pathTypeModels, IParcelWrapper destinationParcel, int householdCars, int choice = Constants.DEFAULT_VALUE) {
			var household = tour.Household;
			var person = tour.Person;
			var personDay = tour.PersonDay;

			// household inputs
			var income0To25KFlag = household.Has0To25KIncome.ToFlag();
			var income25To50KFlag = household.Has25To50KIncome.ToFlag();
			var income75KPlusFlag = household.Has75KPlusIncome.ToFlag();
			var onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
			var twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
			var noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
			var carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);

			// person inputs
			var childUnder5Flag = person.IsChildUnder5.ToFlag();
			var adultFlag = person.IsAdult.ToFlag();
			var drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
			var maleFlag = person.IsMale.ToFlag();

			// remaining inputs
			var originParcel = tour.OriginParcel;
			var destinationParkingCost = destinationParcel.ParkingCostBuffer1(6);

			double escortPercentage;
			double nonEscortPercentage;

			ChoiceModelUtility.SetEscortPercentages(personDay, out escortPercentage, out nonEscortPercentage);

			// school bus is a special case - use HOV3 impedance
			var pathTypeExtra = pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Hov3);
			var modeExtra = Global.Settings.Modes.SchoolBus;
			var availableExtra = pathTypeExtra.Available;
			var generalizedTimeLogsumExtra = pathTypeExtra.GeneralizedTimeLogsum;

			var alternative = choiceProbabilityCalculator.GetAlternative(modeExtra, availableExtra, choice == modeExtra);
			alternative.Choice = modeExtra;

			alternative.AddNestedAlternative(_nestedAlternativeIds[modeExtra], _nestedAlternativeIndexes[modeExtra], THETA_PARAMETER);

			if (availableExtra) {
				//	case Global.Settings.Modes.SchoolBus:
				alternative.AddUtilityTerm(2, generalizedTimeLogsumExtra * tour.TimeCoefficient);
				alternative.AddUtilityTerm(10, 1);
				alternative.AddUtilityTerm(17, childUnder5Flag);
				alternative.AddUtilityTerm(18, adultFlag);
			}

			foreach (var pathTypeModel in pathTypeModels) {
				var mode = pathTypeModel.Mode;
				var available = pathTypeModel.Available;
				var generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

				alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
				alternative.Choice = mode;

				alternative.AddNestedAlternative(_nestedAlternativeIds[mode], _nestedAlternativeIndexes[mode], THETA_PARAMETER);

				if (!available) {
					continue;
				}

				alternative.AddUtilityTerm(2, generalizedTimeLogsum * tour.TimeCoefficient);

				if (mode == Global.Settings.Modes.Transit) {
					alternative.AddUtilityTerm(20, 1);
					alternative.AddUtilityTerm(21, noCarsInHouseholdFlag);
					alternative.AddUtilityTerm(22, carsLessThanDriversFlag);
					alternative.AddUtilityTerm(27, childUnder5Flag);
					alternative.AddUtilityTerm(28, adultFlag);
					alternative.AddUtilityTerm(29, drivingAgeStudentFlag);
					alternative.AddUtilityTerm(129, destinationParcel.MixedUse2Index1());
//						alternative.AddUtility(128, destinationParcel.TotalEmploymentDensity1());
//						alternative.AddUtility(127, destinationParcel.NetIntersectionDensity1());
//						alternative.AddUtility(126, originParcel.NetIntersectionDensity1());
//						alternative.AddUtility(125, originParcel.HouseholdDensity1());
//						alternative.AddUtility(124, originParcel.MixedUse2Index1());
//						alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
					alternative.AddUtilityTerm(122, Math.Log(originParcel.StopsTransitBuffer1 + 1));
				}
				else if (mode == Global.Settings.Modes.Hov3) {
					alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT3));
					alternative.AddUtilityTerm(30, 1);
					alternative.AddUtilityTerm(37, twoPersonHouseholdFlag);
					alternative.AddUtilityTerm(37, onePersonHouseholdFlag);
					alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
					alternative.AddUtilityTerm(44, income0To25KFlag);
					alternative.AddUtilityTerm(45, income25To50KFlag);
					alternative.AddUtilityTerm(47, childUnder5Flag);
					alternative.AddUtilityTerm(133, escortPercentage);
					alternative.AddUtilityTerm(134, nonEscortPercentage);
				}
				else if (mode == Global.Settings.Modes.Hov2) {
					alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT2));
					alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
					alternative.AddUtilityTerm(40, 1);
					alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
					alternative.AddUtilityTerm(44, income0To25KFlag);
					alternative.AddUtilityTerm(45, income25To50KFlag);
					alternative.AddUtilityTerm(47, childUnder5Flag);
					alternative.AddUtilityTerm(133, escortPercentage);
					alternative.AddUtilityTerm(134, nonEscortPercentage);
				}
				else if (mode == Global.Settings.Modes.Sov) {
					alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient));
					alternative.AddUtilityTerm(50, 1);
					alternative.AddUtilityTerm(52, carsLessThanDriversFlag);
					alternative.AddUtilityTerm(54, income0To25KFlag);
					alternative.AddUtilityTerm(56, income75KPlusFlag);
					alternative.AddUtilityTerm(59, drivingAgeStudentFlag);
					alternative.AddUtilityTerm(131, escortPercentage);
					alternative.AddUtilityTerm(132, nonEscortPercentage);
				}
				else if (mode == Global.Settings.Modes.Bike) {
					double class1Dist =
						Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
							? ImpedanceRoster.GetValue("class1distance", mode, Global.Settings.PathTypes.FullNetwork,
								Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
							: 0;

                    double class2Dist =
						Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
							? ImpedanceRoster.GetValue("class2distance", mode, Global.Settings.PathTypes.FullNetwork,
								Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
							: 0;

                    double worstDist =
						Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
							? ImpedanceRoster.GetValue("worstdistance", mode, Global.Settings.PathTypes.FullNetwork,
								Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
							: 0;

					alternative.AddUtilityTerm(60, 1);
					alternative.AddUtilityTerm(61, maleFlag);
					alternative.AddUtilityTerm(69, adultFlag);
					alternative.AddUtilityTerm(169, destinationParcel.MixedUse4Index1());
					alternative.AddUtilityTerm(168, destinationParcel.TotalEmploymentDensity1());
//						alternative.AddUtility(167, destinationParcel.NetIntersectionDensity1());
//						alternative.AddUtility(166, originParcel.NetIntersectionDensity1());
//						alternative.AddUtility(165, originParcel.HouseholdDensity1());
					alternative.AddUtilityTerm(164, originParcel.MixedUse4Index1());
					alternative.AddUtilityTerm(161, (class1Dist > 0).ToFlag());
					alternative.AddUtilityTerm(162, (class2Dist > 0).ToFlag());
					alternative.AddUtilityTerm(163, (worstDist > 0).ToFlag());
				}
				else if (mode == Global.Settings.Modes.Walk) {
                    alternative.AddUtilityTerm(70, 1);
                    alternative.AddUtilityTerm(79, adultFlag);
					alternative.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
//						alternative.AddUtility(178, destinationParcel.TotalEmploymentDensity1());
//						alternative.AddUtility(177, destinationParcel.NetIntersectionDensity1());
//						alternative.AddUtility(176, originParcel.NetIntersectionDensity1());
//						alternative.AddUtility(175, originParcel.HouseholdDensity1());
					alternative.AddUtilityTerm(179, originParcel.MixedUse4Index1());
				}
			}
		}
	}
}