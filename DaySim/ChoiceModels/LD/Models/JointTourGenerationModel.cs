// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using Daysim.DomainModels;
using Daysim.DomainModels.LD;
using Daysim.DomainModels.LD.Wrappers;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Extensions;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;

namespace Daysim.ChoiceModels.LD.Models {
	public class JointTourGenerationModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "LDJointTourGenerationModel";
		private readonly int _totalAlternatives = Global.Settings.Purposes.Medical;
		private const int TOTAL_NESTED_ALTERNATIVES = 2;
		private const int TOTAL_LEVELS = 2;
		private const int MAX_PARAMETER = 60;

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.JointTourGenerationModelCoefficients, _totalAlternatives,
						  TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public int Run(HouseholdDayWrapper householdDay, int nCallsForTour) {
			return Run(householdDay, nCallsForTour, Global.Settings.Purposes.NoneOrHome);
		}

		public int Run(HouseholdDayWrapper householdDay, int nCallsForTour, int choice) {
			if (householdDay == null) {
				throw new ArgumentNullException("householdDay");
			}
			
			householdDay.ResetRandom(935 + nCallsForTour); // TODO:  fix the ResetRandom call parameter

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return choice;
				}
			}

			var choiceProbabilityCalculator =
				_helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day)* 397) ^ nCallsForTour);

			if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
				RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour, choice);

				choiceProbabilityCalculator.WriteObservation();
			}

			else if (Global.Configuration.TestEstimationModelInApplicationMode) {
				Global.Configuration.IsInEstimationMode = false;

				//choice = Math.Min(personDay.BusinessStops, 1) + 2 * Math.Min(personDay.SchoolStops, 1);

				RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, choice);

				Global.Configuration.IsInEstimationMode = true;
			}

			else {
				RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
				choice = (int) chosenAlternative.Choice;
			}

			return choice;
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay,
									 int nCallsForTour, int choice = Constants.DEFAULT_VALUE) {
			//var householdDay = (LDHouseholdDayWrapper)tour.HouseholdDay;
			var household = householdDay.Household;

			var carOwnership =
				household.VehiclesAvailable == 0
					? Global.Settings.CarOwnerships.NoCars
					: household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
						  ? Global.Settings.CarOwnerships.LtOneCarPerAdult
						  : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

			var votALSegment = Global.Settings.VotALSegments.Medium; // TODO:  calculate a VOT segment that depends on household income
			var transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
			var personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				[Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
			var shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				//[Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
				[Global.Settings.Purposes.Shopping][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];
			var mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				[Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
			var socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				[Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
			//var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
			var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];


			int hasAdultEducLevel12 = 0;
			//int allAdultEducLevel12 = 1;
			int youngestAge = 999;

			foreach (PersonWrapper person in householdDay.Household.Persons) {
				// set characteristics here that depend on person characteristics
				if (person.Age >= 18 && person.EducationLevel >= 12) hasAdultEducLevel12 = 1;
				//if (person.Age >= 18 && person.EducationLevel < 12) allAdultEducLevel12 = 0;
				if (person.Age < youngestAge) youngestAge = person.Age;
			}

			// NONE_OR_HOME

			var noneOrHomeAvailable = true;
			if (Global.Configuration.ShouldRunLDPrimaryPriorityTimeModel && householdDay.JointTourFlag == 1
				&& nCallsForTour == 1) {
				noneOrHomeAvailable = false;
			}

			var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, noneOrHomeAvailable, choice == Global.Settings.Purposes.NoneOrHome);

			alternative.Choice = Global.Settings.Purposes.NoneOrHome;

			alternative.AddUtilityTerm(1, (nCallsForTour == 2).ToFlag());
			alternative.AddUtilityTerm(13, (nCallsForTour > 2).ToFlag());
			//alternative.AddUtilityTerm(2, noCarsFlag);
			//alternative.AddUtilityTerm(3, carCompetitionFlag);
			//alternative.AddUtilityTerm(4, householdDay.PrimaryPriorityTimeFlag);

			//alternative.AddUtilityTerm(2, householdDay.Household.HasChildren.ToFlag());
			alternative.AddUtilityTerm(2, householdDay.Household.HasChildrenUnder5.ToFlag());
			alternative.AddUtilityTerm(3, householdDay.Household.HasChildrenAge5Through15.ToFlag());
			alternative.AddUtilityTerm(4, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
			alternative.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());
			alternative.AddUtilityTerm(6, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
			alternative.AddUtilityTerm(7, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());
			alternative.AddUtilityTerm(8, (youngestAge >= 40).ToFlag());

			alternative.AddUtilityTerm(10, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
			alternative.AddUtilityTerm(11, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
			alternative.AddUtilityTerm(12, (householdDay.Household.Income >= 900000).ToFlag());

			//alternative.AddUtilityTerm(13, hasAdultEducLevel12);
			//alternative.AddUtilityTerm(14, allAdultEducLevel12);
			//alternative.AddUtilityTerm(15, (youngestAge >= 40).ToFlag());
			//alternative.AddUtilityTerm(10, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());//

			//alternative.AddUtilityTerm(11, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());
			//alternative.AddUtilityTerm(11, (householdDay.AdultsInSharedHomeStay == 2 && allAdultEducLevel12 == 1).ToFlag());

			//alternative.AddNestedAlternative(11, 0, 60);

			// WORK
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, false, choice == Global.Settings.Purposes.Work);
			alternative.Choice = Global.Settings.Purposes.Work;
			alternative.AddUtilityTerm(52, 1);

			//alternative.AddNestedAlternative(12, 1, 60);

			// SCHOOL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);
			alternative.Choice = Global.Settings.Purposes.School;
			alternative.AddUtilityTerm(53, 1);

			//alternative.AddNestedAlternative(12, 1, 60);

			// ESCORT
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, false, choice == Global.Settings.Purposes.Escort);
			alternative.Choice = Global.Settings.Purposes.Escort;
			alternative.AddUtilityTerm(54, 1);
			//alternative.AddUtilityTerm(22, householdDay.PrimaryPriorityTimeFlag);
			//alternative.AddUtilityTerm(23, (householdDay.Household.Size == 3).ToFlag());
			//alternative.AddUtilityTerm(24, (householdDay.Household.Size >= 4).ToFlag());
			//alternative.AddUtilityTerm(25, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());
			//alternative.AddUtilityTerm(58, compositeLogsum);

			// PERSONAL_BUSINESS
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, true, choice == Global.Settings.Purposes.PersonalBusiness);
			alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

			alternative.AddUtilityTerm(21, 1);
			alternative.AddUtilityTerm(22, householdDay.PrimaryPriorityTimeFlag);

			alternative.AddUtilityTerm(23, (householdDay.Household.Size == 3).ToFlag());
			alternative.AddUtilityTerm(24, (householdDay.Household.Size >= 4).ToFlag());
			//alternative.AddUtilityTerm(25, (householdDay.Household.Size >= 5).ToFlag());

			//alternative.AddUtilityTerm(26, (householdDay.Household.VehiclesAvailable == 0).ToFlag());
			//alternative.AddUtilityTerm(27, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
			alternative.AddUtilityTerm(28, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

			//alternative.AddUtilityTerm(27, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
			//alternative.AddUtilityTerm(28, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());

			//alternative.AddUtilityTerm(56, personalBusinessAggregateLogsum);
			alternative.AddUtilityTerm(56, compositeLogsum);

			//alternative.AddNestedAlternative(12, 1, 60);

			// SHOPPING
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, true, choice == Global.Settings.Purposes.Shopping);
			alternative.Choice = Global.Settings.Purposes.Shopping;

			alternative.AddUtilityTerm(31, 1);
			alternative.AddUtilityTerm(32, householdDay.PrimaryPriorityTimeFlag);

			alternative.AddUtilityTerm(33, (householdDay.Household.Size == 3).ToFlag());
			alternative.AddUtilityTerm(34, (householdDay.Household.Size >= 4).ToFlag());
			//alternative.AddUtilityTerm(35, (householdDay.Household.Size >= 5).ToFlag());

			//alternative.AddUtilityTerm(36, (householdDay.Household.VehiclesAvailable == 0).ToFlag());
			alternative.AddUtilityTerm(37, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
			alternative.AddUtilityTerm(38, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

			//alternative.AddUtilityTerm(37, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
			//alternative.AddUtilityTerm(38, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());

			//alternative.AddUtilityTerm(57, shoppingAggregateLogsum);
			alternative.AddUtilityTerm(59, compositeLogsum);

			//alternative.AddNestedAlternative(12, 1, 60);

			// MEAL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, false, choice == Global.Settings.Purposes.Meal);
			alternative.Choice = Global.Settings.Purposes.Meal;

			alternative.AddUtilityTerm(55, 1);

			//alternative.AddNestedAlternative(12, 1, 60);

			// SOCIAL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, true, choice == Global.Settings.Purposes.Social);
			alternative.Choice = Global.Settings.Purposes.Social;

			alternative.AddUtilityTerm(41, 1);
			alternative.AddUtilityTerm(42, householdDay.PrimaryPriorityTimeFlag);

			alternative.AddUtilityTerm(43, (householdDay.Household.Size == 3).ToFlag());
			alternative.AddUtilityTerm(44, (householdDay.Household.Size >= 4).ToFlag());
			//alternative.AddUtilityTerm(45, (householdDay.Household.Size >= 5).ToFlag());

			//alternative.AddUtilityTerm(46, (householdDay.Household.VehiclesAvailable >= 1).ToFlag());
			//alternative.AddUtilityTerm(47, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
			alternative.AddUtilityTerm(48, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

			//alternative.AddUtilityTerm(46, (householdDay.Household.VehiclesAvailable > 0 && householdDay.Household.HasChildren).ToFlag());
			//alternative.AddUtilityTerm(46, (householdDay.Household.VehiclesAvailable == 0).ToFlag()); cars have no impact on fully joint social tour

			//alternative.AddUtilityTerm(47, householdDay.Household.HasChildrenUnder5.ToFlag());
			//alternative.AddUtilityTerm(48, householdDay.Household.HasChildrenAge5Through15.ToFlag());

			//alternative.AddUtilityTerm(47, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
			//alternative.AddUtilityTerm(48, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());

			//alternative.AddUtilityTerm(58, socialAggregateLogsum);
			//alternative.AddUtilityTerm(58, compositeLogsum);

			//alternative.AddNestedAlternative(12, 1, 60);

		}
	}
}