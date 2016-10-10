// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using Daysim.ChoiceModels.Shared.Models;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Extensions;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;

namespace Daysim.ChoiceModels.Actum.Models {
	public class EscortTourModeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "ActumEscortTourModeModel";
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 111;

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.EscortTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(TourWrapper tour) {
			if (tour == null) {
				throw new ArgumentNullException("tour");
			}

			tour.PersonDay.ResetRandom(40 + tour.Sequence - 1);

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

			if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
				if (tour.DestinationParcel == null || (tour.Mode > Global.Settings.Modes.Hov3 || tour.Mode < Global.Settings.Modes.Walk)) {
					return;
				}

				var pathTypeModels =
					PathTypeModel.Run(
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
						false,
						Global.Settings.Modes.Walk, Global.Settings.Modes.Bike, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Hov3);

				var pathTypeModel = pathTypeModels.First(x => x.Mode == tour.Mode);

				if (!pathTypeModel.Available) {
					return;
				}

				RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Mode);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {
				var pathTypeModels =
					PathTypeModel.Run(
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
						false,
						Global.Settings.Modes.Walk, Global.Settings.Modes.Bike, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Hov3);

				RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

				if (chosenAlternative == null) {
					Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
					tour.Mode = Global.Settings.Modes.Hov3;
					if (!Global.Configuration.IsInEstimationMode) {
						tour.PersonDay.IsValid = false;
					}
					return;
				}

				var choice = (int) chosenAlternative.Choice;

				tour.Mode = choice;
				var chosenPathType = pathTypeModels.First(x => x.Mode == choice);
				tour.PathType = chosenPathType.PathType;
				tour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
			}
		}

		public ChoiceProbabilityCalculator.Alternative RunNested(TourWrapper tour, IParcelWrapper destinationParcel) {
			if (tour == null) {
				throw new ArgumentNullException("tour");
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

			var pathTypeModels =
				PathTypeModel.Run(
				tour.Household.RandomUtility,
					tour.OriginParcel,
					destinationParcel,
					tour.DestinationArrivalTime,
					tour.DestinationDepartureTime,
					Global.Settings.Purposes.Escort,
					tour.CostCoefficient,
					tour.TimeCoefficient,
					tour.Person.IsDrivingAge,
					tour.Household.VehiclesAvailable,
					tour.Person.GetTransitFareDiscountFraction(),
					false,
					Global.Settings.Modes.Walk, Global.Settings.Modes.Bike, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Hov3);

			RunModel(choiceProbabilityCalculator, tour, pathTypeModels, destinationParcel);

			return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IEnumerable<PathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int choice = Constants.DEFAULT_VALUE) {
			var household = tour.Household;
			var householdTotals = household.HouseholdTotals;
			var person = tour.Person;

			// household inputs
			var childrenUnder5 = householdTotals.ChildrenUnder5;
			var childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
			var drivingAgeStudents = householdTotals.DrivingAgeStudents;
			var noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(household.VehiclesAvailable);
			var carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(household.VehiclesAvailable);

			// person inputs
			var ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();

			// other inputs

			foreach (var pathTypeModel in pathTypeModels) {
				var mode = pathTypeModel.Mode;
				var available = (mode <= Global.Settings.Modes.Hov3 && mode >= Global.Settings.Modes.Walk) && pathTypeModel.Available;
				var generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

				var alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
				alternative.Choice = mode;

				if (!available) {
					continue;
				}

				if (mode == Global.Settings.Modes.Hov3) {
					alternative.AddUtilityTerm(2, generalizedTimeLogsum * tour.TimeCoefficient);
					alternative.AddUtilityTerm(30, 1);
					alternative.AddUtilityTerm(31, childrenUnder5);
					alternative.AddUtilityTerm(32, childrenAge5Through15);
					alternative.AddUtilityTerm(33, drivingAgeStudents);
					alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
				}
				else if (mode == Global.Settings.Modes.Hov2) {
					alternative.AddUtilityTerm(2, generalizedTimeLogsum * tour.TimeCoefficient);
					alternative.AddUtilityTerm(40, 1);
					alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
					alternative.AddUtilityTerm(42, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Sov) {
					alternative.AddUtilityTerm(50, 1);
				}
				else if (mode == Global.Settings.Modes.Bike) {
					alternative.AddUtilityTerm(60, 1);
				}
				else if (mode == Global.Settings.Modes.Walk) {
					alternative.AddUtilityTerm(2, generalizedTimeLogsum * tour.TimeCoefficient);
					alternative.AddUtilityTerm(73, ageBetween51And98Flag);
					alternative.AddUtilityTerm(76, destinationParcel.NetIntersectionDensity1());
					alternative.AddUtilityTerm(81, childrenUnder5);
					alternative.AddUtilityTerm(82, childrenAge5Through15);
					alternative.AddUtilityTerm(83, drivingAgeStudents);
				}
			}
		}
	}
}