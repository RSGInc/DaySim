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
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.H.Models {
	public class WorkAtHomeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "HWorkAtHomeModel";
		private const int TOTAL_ALTERNATIVES = 2;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 99;
		
		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkAtHomeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay) {
			if (personDay == null) {
				throw new ArgumentNullException("personDay");
			}
			
			personDay.Person.ResetRandom(904);

			if (Global.Configuration.IsInEstimationMode) {

				if (personDay.WorkAtHomeDuration >= 120 && personDay.Person.IsFullOrPartTimeWorker) { personDay.WorksAtHomeFlag = 1; }
				else personDay.WorksAtHomeFlag = 0;
				if (!_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode || !personDay.Person.IsFullOrPartTimeWorker) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {

				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}

				RunModel(choiceProbabilityCalculator, personDay, householdDay, personDay.WorksAtHomeFlag);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {

				int choice;

				if (!personDay.Person.IsFullOrPartTimeWorker) {
					choice = 0;
				}
				else {

					RunModel(choiceProbabilityCalculator, personDay, householdDay);

					var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
					choice = (int) chosenAlternative.Choice;
				}
				personDay.WorksAtHomeFlag = choice;
                personDay.WorkAtHomeDuration = choice * 120; //default predicted duration for output
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {

			IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

			Double workTourLogsum;

			if (personDay.Person.UsualWorkParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
				if (personDay.Person.UsualDeparturePeriodFromWork != Constants.DEFAULT_VALUE && personDay.Person.UsualArrivalPeriodToWork != Constants.DEFAULT_VALUE) {
					var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, personDay.Person.UsualArrivalPeriodToWork, personDay.Person.UsualDeparturePeriodFromWork, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
					workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				}
				else {
					var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
					workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				}


			}
			else {
				workTourLogsum = 0;

			}

            int noUsualWorkLocation = 0;

            if (personDay.Person.UsualWorkParcelId == Constants.DEFAULT_VALUE || personDay.Person.UsualWorkParcelId == Global.Settings.OutOfRegionParcelId) {
                  noUsualWorkLocation = 1;	
            }
            
			int atHomeDay = personDay.PatternType == 3 ? 1 : 0;
			int nonMandatoryTourDay = personDay.PatternType == 2 ? 1 : 0;
			int mandatoryTourDay = personDay.PatternType == 1 ? 1 : 0;
			var household = householdDay.Household;
			var carOwnership =
								 household.VehiclesAvailable == 0
										? Global.Settings.CarOwnerships.NoCars
										: household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
											  ? Global.Settings.CarOwnerships.LtOneCarPerAdult
											  : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;
    


            var usualWorkAtHome = household.ResidenceParcelId == personDay.Person.UsualWorkParcelId ? 1 : 0;
            var usualWorkOutside = usualWorkAtHome == 1 ? 0 : 1;
           
            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

			int countMandatory = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 1 select personDayHH.PatternType).Count();
			int countNonMandatory = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 2 select personDayHH.PatternType).Count();
			int countAtHome = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 3 select personDayHH.PatternType).Count();

			// 0 Person doesn't work at home more than specified number of minutes
			var alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
			alternative.Choice = 0;

			alternative.AddUtilityTerm(1, 0.0);

			// 1 Works at home
			alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
			alternative.Choice = 1;
            
            var adjustedConstant = Global.Configuration.IsInEstimationMode ? 1.0 :
                (-2.0 + Math.Log(1.0 + Global.Configuration.Policy_FractionIncreaseInWorkAtHomeShare)) / -2.0;
            alternative.AddUtilityTerm(1, adjustedConstant);

			alternative.AddUtilityTerm(2, personDay.Person.IsPartTimeWorker.ToFlag());
            alternative.AddUtilityTerm(4, nonMandatoryTourDay * usualWorkOutside);
            alternative.AddUtilityTerm(7, atHomeDay * usualWorkOutside);
			alternative.AddUtilityTerm(8, (personDay.Person.Age < 30).ToFlag());
			alternative.AddUtilityTerm(9, (personDay.Person.Age >= 30 && personDay.Person.Age < 35).ToFlag());
            alternative.AddUtilityTerm(10, noUsualWorkLocation);
			alternative.AddUtilityTerm(11, countMandatory - mandatoryTourDay);
            alternative.AddUtilityTerm(12, countAtHome - atHomeDay);
			alternative.AddUtilityTerm(14, householdDay.Household.HouseholdTotals.ChildrenAge5Through15);
			alternative.AddUtilityTerm(16, usualWorkAtHome);
			alternative.AddUtilityTerm(17, ((householdDay.Household.Has0To25KIncome).ToFlag()));
            alternative.AddUtilityTerm(18, ((householdDay.Household.Has100KPlusIncome).ToFlag()));
			alternative.AddUtilityTerm(19, workTourLogsum);
			alternative.AddUtilityTerm(20, (personDay.Person.IsStudent.ToFlag()));
			alternative.AddUtilityTerm(31, carCompetitionFlag + noCarsFlag);


		}
	}
}