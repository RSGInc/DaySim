// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels;
using DaySim.DomainModels.Default;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.H.Models {
	public class PersonTourGenerationModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "HPersonTourGenerationModel";

		// Add one alternative for the stop choice; Change this hard code
		private const int TOTAL_ALTERNATIVES = 10;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 200;

		public override void RunInitialize(ICoefficientsReader reader = null)
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.PersonTourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public int Run(IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int maxPurpose) {
			return Run(personDay, householdDay, maxPurpose, Global.Settings.Purposes.NoneOrHome);
		}

		public int Run(IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int maxPurpose, int choice) {
			if (householdDay == null) {
				throw new ArgumentNullException("householdDay");
			}
			
			householdDay.ResetRandom(949 + personDay.GetTotalCreatedTours());

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return choice;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalBatchIndex.Value].GetChoiceProbabilityCalculator(((personDay.Person.Id * 10 + personDay.Day) *397) ^ personDay.GetTotalCreatedTours());

			if (_helpers[ParallelUtility.threadLocalBatchIndex.Value].ModelIsInEstimationMode) {
				RunModel(choiceProbabilityCalculator, personDay, householdDay, maxPurpose, choice);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {
				RunModel(choiceProbabilityCalculator, personDay, householdDay, maxPurpose);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
				choice = (int) chosenAlternative.Choice;
			}

			return choice;
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int maxPurpose, int choice = Constants.DEFAULT_VALUE) {

			IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
			
			var person =personDay.Person;
			var household = householdDay.Household;
			var residenceParcel = household.ResidenceParcel;

			var carOwnership =
							household.VehiclesAvailable == 0
								 ? Global.Settings.CarOwnerships.NoCars
								 : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
									  ? Global.Settings.CarOwnerships.LtOneCarPerAdult
									  : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

			var votALSegment = household.GetVotALSegment();
			var transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
			var personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
			var shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
			var mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
			var socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
			var totalAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				[Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
			// var recreationAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
			// [Global.Settings.Purposes.Recreation][carOwnership][votALSegment][transitAccessSegment];
			//  var medicalAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
			//  [Global.Settings.Purposes.Medical][carOwnership][votALSegment][transitAccessSegment];

			int[] nonMandPerstype = new int[8];
			int[] mandPerstype = new int[8];

			//int mandatoryAdult=0;
			//int mandatoryChild=0;
			//int nonMandatoryWorker=0;
			//int nonMandatoryNonWorker=0;
			//int nonMandatoryRetired=0;
			//int nonMandatoryChild=0;

			int countNonMandatory = 0;
			int countMandatory = 0;

			double workLogsum = 0;
			double schoolLogsum= 0;

			//int worksAtHome=0;
			int countWorkingAtHome=0;

			var workDestinationArrivalTime=0;
			var workDestinationDepartureTime=0;

			int numStopPurposes= 0;
			int numTourPurposes =0;

			numTourPurposes= (personDay.CreatedEscortTours>1).ToFlag() +(personDay.CreatedShoppingTours>1).ToFlag()+ (personDay.CreatedMealTours>1).ToFlag()+
											(personDay.CreatedPersonalBusinessTours>1).ToFlag()+(personDay.CreatedSocialTours>1).ToFlag()+
											(personDay.CreatedRecreationTours>1).ToFlag()+(personDay.CreatedMedicalTours>1).ToFlag();
										

			numStopPurposes = (personDay.SimulatedEscortStops>1).ToFlag() +(personDay.SimulatedShoppingStops>1).ToFlag()+ (personDay.SimulatedMealStops>1).ToFlag()+
											(personDay.SimulatedPersonalBusinessStops>1).ToFlag()+(personDay.SimulatedSocialStops>1).ToFlag()+
											(personDay.SimulatedRecreationStops>1).ToFlag()+(personDay.SimulatedMedicalStops>1).ToFlag();
											

				if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId) {
					workLogsum = 0;
				}
				else {
					workDestinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
					workDestinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
					var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, workDestinationArrivalTime, workDestinationDepartureTime, household.VehiclesAvailable);

					workLogsum= nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				}

				if (person.UsualSchoolParcel == null || person.UsualSchoolParcelId == household.ResidenceParcelId) {
					schoolLogsum = 0;
				}
				else {
					var destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
					var destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);
					var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);

					schoolLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				}

				//if (personDay.WorksAtHomeFlag == 1) {
				//	worksAtHome =1;
				//}
				

			int count =0;
			foreach (PersonDayWrapper pDay in orderedPersonDays) {
				count++;
				if (count > 8) {
					break;
				}
				if (pDay.WorksAtHomeFlag == 1) {
					countWorkingAtHome++;
				}
				if (pDay.PatternType == 1) {
					countMandatory++;
				}
				if (pDay.PatternType == 2) {
					countNonMandatory++;
				}

			}


			// NONE_OR_HOME

			var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

			alternative.Choice = Global.Settings.Purposes.NoneOrHome;
            //alternative.AddUtilityTerm(2, (personDay.GetTotalCreatedTours()== 2).ToFlag());
			//alternative.AddUtilityTerm(3,  (personDay.GetTotalCreatedTours()==3).ToFlag());
			alternative.AddUtilityTerm(4,  (personDay.GetTotalCreatedTours()>=4).ToFlag());
			alternative.AddUtilityTerm(5,  (numStopPurposes>=1).ToFlag());
			alternative.AddUtilityTerm(6,  (numTourPurposes>=2).ToFlag());
			alternative.AddUtilityTerm(7,  personDay.CreatedWorkTours+personDay.CreatedSchoolTours);
			alternative.AddUtilityTerm(8,  (personDay.JointTours));
			alternative.AddUtilityTerm(9,  Math.Log(1+ (workDestinationDepartureTime-workDestinationArrivalTime)/60));
			//alternative.AddUtilityTerm(10,  (household.Size==1).ToFlag());
			//alternative.AddNestedAlternative(11, 0, 200);

			// WORK
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, false, choice == Global.Settings.Purposes.Work);
			alternative.Choice = Global.Settings.Purposes.Work;
			alternative.AddUtilityTerm(198, 1);
			//alternative.AddNestedAlternative(12, 1, 200);

			//  SCHOOL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);
			alternative.Choice = Global.Settings.Purposes.School;
			alternative.AddUtilityTerm(199, 1);
			//alternative.AddNestedAlternative(12, 1, 200);

			// ESCORT
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, maxPurpose <= Global.Settings.Purposes.Escort && personDay.CreatedEscortTours > 0, choice == Global.Settings.Purposes.Escort);
			alternative.Choice = Global.Settings.Purposes.Escort;

			alternative.AddUtilityTerm(11, 1);
			alternative.AddUtilityTerm(12, (personDay.PatternType ==2).ToFlag());
			alternative.AddUtilityTerm(13, personDay.JointTours);
			alternative.AddUtilityTerm(14, (personDay.EscortStops>0).ToFlag());
			alternative.AddUtilityTerm(15, (personDay.CreatedEscortTours > 1).ToFlag());
			alternative.AddUtilityTerm(17, (household.HouseholdType== Global.Settings.HouseholdTypes.TwoPlusWorkerStudentAdultsWithChildren).ToFlag() +(
												household.HouseholdType== Global.Settings.HouseholdTypes.TwoPlusAdultsOnePlusWorkersStudentsWithChildren).ToFlag());
			alternative.AddUtilityTerm(20, (household.HouseholdType== Global.Settings.HouseholdTypes.OneAdultWithChildren).ToFlag());
			alternative.AddUtilityTerm(25, (person.PersonType==Global.Settings.PersonTypes.PartTimeWorker).ToFlag());
			alternative.AddUtilityTerm(28, (person.PersonType==Global.Settings.PersonTypes.ChildUnder5).ToFlag());
			alternative.AddUtilityTerm(29, (person.PersonType==Global.Settings.PersonTypes.ChildAge5Through15).ToFlag());
			alternative.AddUtilityTerm(31, (person.PersonType==Global.Settings.PersonTypes.UniversityStudent).ToFlag());
			alternative.AddUtilityTerm(32, countMandatory);
           // alternative.AddUtilityTerm(33, personDay.CreatedWorkTours);
	
			//alternative.AddNestedAlternative(12, 1, 200);

			// PERSONAL_BUSINESS
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, maxPurpose <= Global.Settings.Purposes.PersonalBusiness && personDay.CreatedPersonalBusinessTours > 0, choice == Global.Settings.Purposes.PersonalBusiness);
			alternative.Choice = Global.Settings.Purposes.PersonalBusiness;
			alternative.AddUtilityTerm(41, 1);
			alternative.AddUtilityTerm(42, (personDay.PatternType==2).ToFlag());
			alternative.AddUtilityTerm(43, (personDay.PersonalBusinessStops>0).ToFlag());
			alternative.AddUtilityTerm(44, personalBusinessAggregateLogsum);
			alternative.AddUtilityTerm(45, (household.HouseholdType== Global.Settings.HouseholdTypes.IndividualWorkerStudent).ToFlag());
			alternative.AddUtilityTerm(46, (household.HouseholdType== Global.Settings.HouseholdTypes.IndividualNonworkerNonstudent).ToFlag());
            alternative.AddUtilityTerm(47, (personDay.CreatedWorkTours + personDay.CreatedSchoolTours));


			// SHOPPING
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, maxPurpose <= Global.Settings.Purposes.Shopping && personDay.CreatedShoppingTours > 0, choice == Global.Settings.Purposes.Shopping);
			alternative.Choice = Global.Settings.Purposes.Shopping;

			alternative.AddUtilityTerm(61, 1);
			alternative.AddUtilityTerm(52, (personDay.PatternType==2).ToFlag());
			alternative.AddUtilityTerm(54, (personDay.ShoppingStops>0).ToFlag());
			//alternative.AddUtilityTerm(55, Math.Log(1+person.Household.ResidenceParcel.EmploymentRetailBuffer2));
			alternative.AddUtilityTerm(56, shoppingAggregateLogsum);
			alternative.AddUtilityTerm(57, (person.PersonType==Global.Settings.PersonTypes.RetiredAdult).ToFlag());
			alternative.AddUtilityTerm(58, (household.HouseholdType== Global.Settings.HouseholdTypes.OnePlusWorkerStudentAdultsAndOnePlusNonworkerNonstudentAdultsWithoutChildren).ToFlag());
			alternative.AddUtilityTerm(60, (household.HouseholdType== Global.Settings.HouseholdTypes.IndividualWorkerStudent).ToFlag());
			alternative.AddUtilityTerm(69, (person.PersonType==Global.Settings.PersonTypes.UniversityStudent).ToFlag());
			alternative.AddUtilityTerm(70, (household.Has100KPlusIncome).ToFlag());

			// MEAL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, maxPurpose <= Global.Settings.Purposes.Meal && personDay.CreatedMealTours > 0, choice == Global.Settings.Purposes.Meal);
			alternative.Choice = Global.Settings.Purposes.Meal;
			alternative.AddUtilityTerm(71, 1);
			alternative.AddUtilityTerm(72, (personDay.PatternType ==2).ToFlag());
			alternative.AddUtilityTerm(74, mealAggregateLogsum);
			alternative.AddUtilityTerm(80, (household.HouseholdType== Global.Settings.HouseholdTypes.IndividualWorkerStudent).ToFlag());
			alternative.AddUtilityTerm(82, (household.HouseholdType== Global.Settings.HouseholdTypes.TwoPlusNonworkerNonstudentAdultsWithoutChildren).ToFlag());
			alternative.AddUtilityTerm(85, (person.PersonType==Global.Settings.PersonTypes.RetiredAdult).ToFlag());

			// SOCIAL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, maxPurpose <= Global.Settings.Purposes.Social && personDay.CreatedSocialTours > 0, choice == Global.Settings.Purposes.Social);
			alternative.Choice = Global.Settings.Purposes.Social;

			alternative.AddUtilityTerm(111, 1);
			alternative.AddUtilityTerm(112, (personDay.PatternType ==2).ToFlag());
			alternative.AddUtilityTerm(113, household.HouseholdTotals.ChildrenUnder16);
			//alternative.AddUtilityTerm(114, socialAggregateLogsum);
			alternative.AddUtilityTerm(115,  Math.Log(1+person.Household.ResidenceParcel.HouseholdsBuffer2));
			alternative.AddUtilityTerm(122, (household.HouseholdType== Global.Settings.HouseholdTypes.TwoPlusNonworkerNonstudentAdultsWithoutChildren).ToFlag());
			alternative.AddUtilityTerm(123, (person.PersonType==Global.Settings.PersonTypes.PartTimeWorker).ToFlag());
			alternative.AddUtilityTerm(126, (person.PersonType==Global.Settings.PersonTypes.ChildUnder5).ToFlag());
			alternative.AddUtilityTerm(127, (person.PersonType==Global.Settings.PersonTypes.ChildAge5Through15).ToFlag());
			alternative.AddUtilityTerm(130, (person.PersonType==Global.Settings.PersonTypes.RetiredAdult).ToFlag());

			// RECREATION
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Recreation, maxPurpose <= Global.Settings.Purposes.Recreation && personDay.CreatedRecreationTours > 0, choice == Global.Settings.Purposes.Recreation);
			alternative.Choice = Global.Settings.Purposes.Recreation;
			alternative.AddUtilityTerm(111, 1);
			alternative.AddUtilityTerm(112, (personDay.PatternType ==2).ToFlag());
			alternative.AddUtilityTerm(113, household.HouseholdTotals.ChildrenUnder16);
			//alternative.AddUtilityTerm(114, totalAggregateLogsum);
			alternative.AddUtilityTerm(115,  Math.Log(1+person.Household.ResidenceParcel.HouseholdsBuffer2));
			alternative.AddUtilityTerm(122, (household.HouseholdType== Global.Settings.HouseholdTypes.TwoPlusNonworkerNonstudentAdultsWithoutChildren).ToFlag());
			alternative.AddUtilityTerm(123, (person.PersonType==Global.Settings.PersonTypes.PartTimeWorker).ToFlag());
			alternative.AddUtilityTerm(126, (person.PersonType==Global.Settings.PersonTypes.ChildUnder5).ToFlag());
			alternative.AddUtilityTerm(127, (person.PersonType==Global.Settings.PersonTypes.ChildAge5Through15).ToFlag());
			alternative.AddUtilityTerm(128, (household.Has100KPlusIncome).ToFlag());

			// MEDICAL
			alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Medical, maxPurpose <= Global.Settings.Purposes.Medical && personDay.CreatedMedicalTours > 0, choice == Global.Settings.Purposes.Medical);
			alternative.Choice = Global.Settings.Purposes.Medical;
			alternative.AddUtilityTerm(131, 1);
			alternative.AddUtilityTerm(132, Math.Log(1+household.ResidenceParcel.EmploymentMedicalBuffer2));
			alternative.AddUtilityTerm(133, (person.PersonType==Global.Settings.PersonTypes.RetiredAdult).ToFlag());
			//alternative.AddNestedAlternative(11, 1, 60);

		}
	}
}
