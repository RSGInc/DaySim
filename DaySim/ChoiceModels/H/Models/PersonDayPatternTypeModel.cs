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

namespace DaySim.ChoiceModels.H.Models {
	public class PersonDayPatternTypeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "HPersonDayPatternTypeModel";
		private const int TOTAL_ALTERNATIVES = 3;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 80;

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.PersonDayPatternTypeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}
		
		public void Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
			if (personDay == null) {
				throw new ArgumentNullException("personDay");
			}
			
			personDay.Person.ResetRandom(903); 

			int choice = 0;

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

			if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

				choice = personDay.PatternType;

				RunModel(choiceProbabilityCalculator, personDay, householdDay, choice);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {
				RunModel(choiceProbabilityCalculator, personDay, householdDay);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
				choice = (int) chosenAlternative.Choice;

				personDay.PatternType = choice;

			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {
			var household = personDay.Household;
			var person = personDay.Person;

			IEnumerable<PersonDayWrapper> personTypeOrderedPersonDays = householdDay.PersonDays.OrderBy(p => p.Person.PersonType).ToList().Cast<PersonDayWrapper>();
			int mandatoryCount = 0;
			int nonMandatoryCount = 0;
			int homeCount = 0;
        // int mandatoryKids = 0;
         //int kidsAtHome = 0;
         //int nonMandatoryKids = 0;
			//int adultsAtHome = 0;
			double mandatoryLogsum = 0.0;

			int i = 0;
			foreach (PersonDayWrapper pDay in personTypeOrderedPersonDays) {
				i++;
				if (i <= 4) {
                    if (pDay.PatternType == Global.Settings.PatternTypes.Mandatory)
                    {
                       mandatoryCount++;
                    }
                    else if (pDay.PatternType == Global.Settings.PatternTypes.Optional) { 
                       nonMandatoryCount++;
                    }
                    else { homeCount++;}
				}
			}

           /*int oldestAge = (from persons in household.Persons select persons.Age).Max();
            int youngestAge = (from persons in household.Persons select persons.Age).Min();
            int countTransitPassses = (from persons in household.Persons
                                       where persons.TransitPassOwnershipFlag == 1
                                       select persons.TransitPassOwnershipFlag).Count();*/
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
            var totalAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];

            var distanceToStop
                = household.ResidenceParcel.GetDistanceToTransit() > 0
                      ? Math.Min(household.ResidenceParcel.GetDistanceToTransit(), 2 * Global.Settings.DistanceUnitsPerMile)  // JLBscale
                      : 2 * Global.Settings.DistanceUnitsPerMile;

			if (person.PersonType <= Global.Settings.PersonTypes.PartTimeWorker) {
						if (person.UsualWorkParcelId != Constants.DEFAULT_VALUE && person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
							if (person.UsualDeparturePeriodFromWork != Constants.DEFAULT_VALUE && person.UsualArrivalPeriodToWork != Constants.DEFAULT_VALUE) {
								var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualWorkParcel, person.UsualArrivalPeriodToWork, person.UsualDeparturePeriodFromWork, person.Household.HouseholdTotals.DrivingAgeMembers);
								mandatoryLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
							}
							else {
								var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, person.Household.HouseholdTotals.DrivingAgeMembers);
								mandatoryLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
							}
						}
						else {
							mandatoryLogsum = 0;
						}
					}
					else if (person.PersonType >= Global.Settings.PersonTypes.UniversityStudent) {
						if (person.UsualSchoolParcelId != 0 && person.UsualSchoolParcelId != -1 && person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
							var schoolNestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.TwoPM, person.Household.HouseholdTotals.DrivingAgeMembers);
							mandatoryLogsum = schoolNestedAlternative == null ? 0 : schoolNestedAlternative.ComputeLogsum();
						}
						else {
							mandatoryLogsum = 0;
						}
					}

			bool mandatoryAvailableFlag = true;
					if (personDay.Person.IsNonworkingAdult || personDay.Person.IsRetiredAdult||
						(!personDay.Person.IsWorker && !personDay.Person.IsStudent)||
  						(!Global.Configuration.IsInEstimationMode && !personDay.Person.IsWorker && personDay.Person.UsualSchoolParcel == null)
						) {
						mandatoryAvailableFlag = false;
					}


			// Pattern Type Mandatory on tour (at least one work or school tour)
			var alternative = choiceProbabilityCalculator.GetAlternative(0, mandatoryAvailableFlag, choice == 1);
			alternative.Choice = 1;

			//alternative.AddUtilityTerm(1, 1);
			//alternative.AddUtilityTerm(3, (person.IsStudent && person.IsWorker).ToFlag());
			//alternative.AddUtilityTerm(4, person.IsPartTimeWorker.ToFlag());
			alternative.AddUtilityTerm(6, (person.Age>5).ToFlag() *(person.Age<=18).ToFlag());
        // alternative.AddUtilityTerm(10, (mandatoryCount == 2).ToFlag());
			alternative.AddUtilityTerm(11, (mandatoryCount >= 3).ToFlag());
			alternative.AddUtilityTerm(12, mandatoryLogsum);
         //alternative.AddUtilityTerm(13, totalAggregateLogsum);
			alternative.AddUtilityTerm(15, household.Has100KPlusIncome.ToFlag());

			// PatternType NonMandatory on tour (tours, but none for work or school)
			alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 2);
			alternative.Choice = 2;
         alternative.AddUtilityTerm(21, 1);
			alternative.AddUtilityTerm(23, person.IsPartTimeWorker.ToFlag());
			alternative.AddUtilityTerm(24, person.IsDrivingAgeStudent.ToFlag());
			alternative.AddUtilityTerm(25, person.IsUniversityStudent.ToFlag());
			alternative.AddUtilityTerm(26,  person.IsNonworkingAdult.ToFlag());
			//alternative.AddUtilityTerm(28, person.IsRetiredAdult.ToFlag());
			alternative.AddUtilityTerm(29, (nonMandatoryCount == 0).ToFlag());
			//alternative.AddUtilityTerm(30, (nonMandatoryKids==1).ToFlag());
         alternative.AddUtilityTerm(31, ((nonMandatoryCount==2).ToFlag()));
			 alternative.AddUtilityTerm(32, ((nonMandatoryCount>=3).ToFlag()));
         alternative.AddUtilityTerm(34, totalAggregateLogsum);
         alternative.AddUtilityTerm(35, person.IsAdultFemale.ToFlag());
			//alternative.AddUtilityTerm(37, household.Has75KPlusIncome.ToFlag());
	
			// PatternType Home (all day)
			alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 3);
			alternative.Choice = 3;
			alternative.AddUtilityTerm(41, 1);
			alternative.AddUtilityTerm(42, person.IsPartTimeWorker.ToFlag());
			alternative.AddUtilityTerm(45,  person.IsDrivingAgeStudent.ToFlag());
			alternative.AddUtilityTerm(48, person.IsUniversityStudent.ToFlag());
         alternative.AddUtilityTerm(52, (person.Age>75).ToFlag());
         alternative.AddUtilityTerm(53, (homeCount==0).ToFlag());
         alternative.AddUtilityTerm(57, (homeCount>=2).ToFlag());
			//alternative.AddUtilityTerm(58, person.IsAdultFemale.ToFlag());
        // alternative.AddUtilityTerm(60, noCarsFlag + carCompetitionFlag);
         alternative.AddUtilityTerm(62, distanceToStop);
			//alternative.AddUtilityTerm(63, totalAggregateLogsum);
			alternative.AddUtilityTerm(64, mandatoryLogsum);
			//alternative.AddUtilityTerm(65, Math.Log(1+youngestAge));
			//alternative.AddUtilityTerm(66, Math.Log(1+oldestAge));

           }
       
	}
}