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
	public class PrimaryPriorityTimeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "LDPrimaryPriorityTimeModel";
		private const int TOTAL_ALTERNATIVES = 4;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		//private const int THETA_PARAMETER = 99;
		private const int MAX_PARAMETER = 99;

		public override void RunInitialize(ICoefficientsReader reader = null) {
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.LDPrimaryPriorityTimeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(HouseholdDayWrapper householdDay) {
			if (householdDay == null) {
				throw new ArgumentNullException("householdDay");
			}

			householdDay.ResetRandom(901);

			// IEnumerable<PersonWrapper> personTypeOrderedPersons = householdDay.Household.Persons.OrderBy(p=> p.PersonType).ToList();



			if (Global.Configuration.IsInEstimationMode) {

				if (householdDay.SharedActivityHomeStays >= 1
					//&& householdDay.DurationMinutesSharedHomeStay >=60 
					 && householdDay.AdultsInSharedHomeStay >= 1
					 && householdDay.NumberInLargestSharedHomeStay >= (householdDay.Household.Size)
					 ) {
					householdDay.PrimaryPriorityTimeFlag = 1;
				}
				else householdDay.PrimaryPriorityTimeFlag = 0;

				if (householdDay.JointTours > 0) {
					householdDay.JointTourFlag = 1;
				}
				else {
					householdDay.JointTourFlag = 0;
				}
				
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(householdDay.Household.Id);

			if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

				//				// set choice variable here  (derive from available household properties)
				//				if (householdDay.SharedActivityHomeStays >= 1 
				//					//&& householdDay.DurationMinutesSharedHomeStay >=60 
				//					&& householdDay.AdultsInSharedHomeStay >= 1 
				//					&& householdDay.NumberInLargestSharedHomeStay >= (householdDay.Household.Size)
				//                   )
				//				{
				//					householdDay.PrimaryPriorityTimeFlag = 1;  
				//				}
				//				else 	householdDay.PrimaryPriorityTimeFlag = 0;

				int choice = householdDay.PrimaryPriorityTimeFlag + 2 * (householdDay.JointTours > 0 ? 1 : 0); 

				RunModel(choiceProbabilityCalculator, householdDay, choice);

				choiceProbabilityCalculator.WriteObservation();
			}

			else if (Global.Configuration.TestEstimationModelInApplicationMode) {
				Global.Configuration.IsInEstimationMode = false;

				RunModel(choiceProbabilityCalculator, householdDay);

				//var observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

				var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, householdDay.PrimaryPriorityTimeFlag);

				Global.Configuration.IsInEstimationMode = true;
			}

			else {
				RunModel(choiceProbabilityCalculator, householdDay);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
				var choice = (int) chosenAlternative.Choice;

				if (choice == 0) {
					householdDay.PrimaryPriorityTimeFlag = 0;
					householdDay.JointTourFlag = 0;
				}
				else if (choice == 1) {
					householdDay.PrimaryPriorityTimeFlag = 1;
					householdDay.JointTourFlag = 0;
				}
				else if (choice == 2) {
					householdDay.PrimaryPriorityTimeFlag = 0;
					householdDay.JointTourFlag = 1;
				}
				else { // if (choice == 3) {
					householdDay.PrimaryPriorityTimeFlag = 1;
					householdDay.JointTourFlag = 1;
				}

			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {

			var household = householdDay.Household;
			var residenceParcel = household.ResidenceParcel;

			// set household characteristics here that don't depend on person characteristics

			int hasAdultEducLevel12 = 0;
			int allAdultEducLevel12 = 1;
			int youngestAge = 999;


			double firstWorkLogsum = 0;
			double secondWorkLogsum = 0;
			bool firstWorkLogsumIsSet = false;
			bool secondWorkLogsumIsSet = false;
			int numberWorkers = 0;
			int numberAdults = 0;
			int numberChildren = 0;
			int numberChildrenUnder5 = 0;


			// set characteristics here that depend on person characteristics
			foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
				double workLogsum = 0;
				var person = (PersonWrapper) personDay.Person;
				if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId
					|| (person.PersonType != Global.Settings.PersonTypes.FullTimeWorker
					&& person.PersonType != Global.Settings.PersonTypes.PartTimeWorker))
				//	|| household.VehiclesAvailable == 0) 
				{
				}
				else {
					var destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
					var destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
					//JLB 201406
					//var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
					var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
					workLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				}
				if (person.Age >= 18 && person.EducationLevel >= 12) hasAdultEducLevel12 = 1;
				if (person.Age >= 18 && person.EducationLevel < 12) allAdultEducLevel12 = 0;
				if (person.Age < youngestAge) youngestAge = person.Age;
				if (workLogsum != 0 && !firstWorkLogsumIsSet) {
					firstWorkLogsum = workLogsum;
					firstWorkLogsumIsSet = true;
				}
				else if (workLogsum != 0 && !secondWorkLogsumIsSet) {
					secondWorkLogsum = workLogsum;
					secondWorkLogsumIsSet = true;
				}
				if (person.Age >= 18) {
					numberAdults++;
					if (person.PersonType == Global.Settings.PersonTypes.FullTimeWorker
						//|| person.PersonType == Constants.PersonType.PART_TIME_WORKER
						) {
						numberWorkers++;
					}
				}
				else {
					numberChildren++;
					if (person.PersonType == Global.Settings.PersonTypes.ChildUnder5) {
						numberChildrenUnder5++;
					}
				}
			}
			var singleWorkerWithChildUnder5 = (numberAdults == numberWorkers && numberAdults == 1
				&& numberChildrenUnder5 > 0) ? true : false;
			var workingCoupleNoChildren = (numberAdults == numberWorkers && numberAdults == 2
				&& numberChildren == 0) ? true : false;
			var workingCoupleAllChildrenUnder5 = (numberAdults == numberWorkers && numberAdults == 2
				&& numberChildren > 0 && numberChildren == numberChildrenUnder5) ? true : false;
			var otherHouseholdWithPTFTWorkers = false;
			if (!workingCoupleNoChildren && !workingCoupleAllChildrenUnder5 && firstWorkLogsum != 0) {
				otherHouseholdWithPTFTWorkers = true;
			}
			var nonWorkingHousehold = false;
			if (!workingCoupleNoChildren && !workingCoupleAllChildrenUnder5 && !otherHouseholdWithPTFTWorkers) {
				nonWorkingHousehold = true;
			}


			// var votSegment = household.VotALSegment;
			//var taSegment = household.ResidenceParcel.TransitAccessSegment();

			//var aggregateLogsumDifference = // full car ownership vs. no car ownership
			//	Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment] -
			//	Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votSegment][taSegment];


			//var householdDay = (LDHouseholdDayWrapper)tour.HouseholdDay;

			var carOwnership =
							household.VehiclesAvailable == 0
								 ? Global.Settings.CarOwnerships.NoCars
								 : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
									  ? Global.Settings.CarOwnerships.LtOneCarPerAdult
									  : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

			var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
			var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

			var votALSegment = Global.Settings.VotALSegments.Medium;  // TODO:  calculate a VOT segment that depends on household income
			var transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
			var personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
			var shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
			var mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
			var socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
			//var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
			var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];

			var componentIndex = 0;
			for (int pfpt = 0; pfpt < 2; pfpt++) {
				if (pfpt == 1) {
					componentIndex = 1;
					choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
					var pfptComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);
					pfptComponent.AddUtilityTerm(1, (householdDay.Household.Size == 3).ToFlag());
					pfptComponent.AddUtilityTerm(2, (householdDay.Household.Size >= 4).ToFlag());
					pfptComponent.AddUtilityTerm(3, householdDay.Household.HasChildrenUnder5.ToFlag());
					pfptComponent.AddUtilityTerm(4, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenAge5Through15).ToFlag());
					pfptComponent.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
					pfptComponent.AddUtilityTerm(6, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());
					pfptComponent.AddUtilityTerm(7, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
					pfptComponent.AddUtilityTerm(8, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());
					//pfptComponent.AddUtilityTerm(29, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
					//pfptComponent.AddUtilityTerm(30, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
					//pfptComponent.AddUtilityTerm(31, (householdDay.Household.Income >= 900000).ToFlag());
					pfptComponent.AddUtilityTerm(9, (firstWorkLogsum + secondWorkLogsum) * (workingCoupleNoChildren || workingCoupleAllChildrenUnder5).ToFlag());
					pfptComponent.AddUtilityTerm(9, (firstWorkLogsum + secondWorkLogsum) * otherHouseholdWithPTFTWorkers.ToFlag());
					pfptComponent.AddUtilityTerm(10, compositeLogsum);


				}
			}
			for (var jointTourFlag = 0; jointTourFlag < 2; jointTourFlag++) {
				if (jointTourFlag == 1) {
					componentIndex = 2;
					choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
					var jointComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);
					jointComponent.AddUtilityTerm(21, householdDay.Household.HasChildrenUnder5.ToFlag());
					jointComponent.AddUtilityTerm(22, householdDay.Household.HasChildrenAge5Through15.ToFlag());
					jointComponent.AddUtilityTerm(23, (youngestAge >= 40).ToFlag());
					//noJointComponent.AddUtilityTerm(50, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
					//noJointComponent.AddUtilityTerm(51, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
					//noJointComponent.AddUtilityTerm(52, (householdDay.Household.Income >= 900000).ToFlag());
					jointComponent.AddUtilityTerm(24, compositeLogsum);
				}
			}

			var available = true;
			for (int pfpt = 0; pfpt < 2; pfpt++) {
				for (var jointTourFlag = 0; jointTourFlag < 2; jointTourFlag++) {

					var altIndex = pfpt + jointTourFlag * 2;
					if (household.Size < 2 && altIndex > 0) {
						available = false;
					}
					else {
						available = true;
					}
					var alternative = choiceProbabilityCalculator.GetAlternative(altIndex, available, choice != null && choice == altIndex);
					
					alternative.Choice = altIndex;

					//NESTING WAS REJECTED BY TESTS
					//alternative.AddNestedAlternative(5 + pfpt,          pfpt, THETA_PARAMETER);  // pfpt on top
					//alternative.AddNestedAlternative(5 + jointTourFlag, jointTourFlag, THETA_PARAMETER); //jointTourFlag on top

					if (pfpt == 1) {
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(1));
					//alternative.AddUtilityTerm(20, 1);
					
					}
					if (jointTourFlag == 0) {
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(2));
					//alternative.AddUtilityTerm(40, 1);
					}

					if (pfpt == 0 && jointTourFlag == 0) {
					}
					else if (pfpt == 1 && jointTourFlag == 0) {
					alternative.AddUtilityTerm(51, 1);
					}
					else if (pfpt == 0 && jointTourFlag == 1) {
					alternative.AddUtilityTerm(61, 1);
					}
					else if (pfpt == 1 && jointTourFlag == 1) {
					alternative.AddUtilityTerm(71, 1);
					//alternative.AddUtilityTerm(72, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
					//alternative.AddUtilityTerm(73, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());
					//alternative.AddUtilityTerm(74, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
					//alternative.AddUtilityTerm(75, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());
					}

				}
			}



		}
	}
}