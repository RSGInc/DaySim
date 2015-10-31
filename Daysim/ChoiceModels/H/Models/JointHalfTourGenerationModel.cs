// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Extensions;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;
using Daysim.Framework.Roster;

namespace Daysim.ChoiceModels.H.Models {
	public class JointHalfTourGenerationModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "HJointHalfTourGenerationModel";
		private const int TOTAL_ALTERNATIVES = 7;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 181;
		
		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.JointHalfTourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public int Run(HouseholdDayWrapper householdDay, int nCallsForTour, bool[] available) {
			return Run(householdDay, nCallsForTour, available, Global.Settings.Purposes.NoneOrHome, Global.Settings.Purposes.NoneOrHome);
		}

		public int Run(HouseholdDayWrapper householdDay, int nCallsForTour, bool[] available, int type, int subType) {
			if (householdDay == null) {
				throw new ArgumentNullException("householdDay");
			}
			
			householdDay.ResetRandom(920 + nCallsForTour);

			int choice = 0;

			if (Global.Configuration.IsInEstimationMode) {

				choice = type == 0 ? 0 : (type - 1) * 3 + subType + 1;

				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return choice;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ nCallsForTour);

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {
				RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour, available, choice);

				choiceProbabilityCalculator.WriteObservation();

			}
			else {
				RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour, available);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
				choice = (int) chosenAlternative.Choice;

			}

			return choice;
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int nCallsForTour, bool[] available, int choice = Constants.DEFAULT_VALUE) {

	
			if (available[4]) {
				bool testbreak = true;
			}

			
			
			var household = householdDay.Household;

			IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

			var carOwnership =
						household.VehiclesAvailable == 0
							? Global.Settings.CarOwnerships.NoCars
							: household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
								? Global.Settings.CarOwnerships.LtOneCarPerAdult
								: Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);
			var carsGrAdults = household.VehiclesAvailable > household.HouseholdTotals.DrivingAgeMembers ? 1 : 0;

			var votALSegment = household.GetVotALSegment();
			var transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
			var totAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
				 [Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];

			int countWorkingAtHome = 0;
			int transitPassOwnership = 0;
			int oldestChild = 0;
			int payParkWork = 0;
            int numSchoolMatch = 0;

            double totHomeSchoolTime =0;
            double countHomeSchoolTime= 0;

            double totHomeWorkTime = 0;
            double countHomeWorkTime = 0;

            double totDetourTime= 0;
            double countDetourTime = 0;

            double aveHomeSchoolTime = 0;
            double aveDetourWorkTime = 0;
            //double minHomeSchoolTime = 1000;
            //double minDetourWorkTime = 1000;
            double aveHomeWorkTime = 0;
           

			int count = 0;
            List<KeyValuePair<int, double>> hhSchools = new List<KeyValuePair<int,double>>();
            List<KeyValuePair<int,double>>hhJobs = new List<KeyValuePair<int,double>>();


			foreach (PersonDayWrapper personDay in orderedPersonDays) {
				var person = personDay.Person;
				count++;
				if (count > 8) {
					break;
				}
                if (person.UsualSchoolParcelId > 0)
                {
                    hhSchools.Add(new KeyValuePair<int, double> (person.UsualSchoolParcelId, person.AutoTimeToUsualSchool));
                }

                 if (person.UsualWorkParcelId > 0)
                {
                    hhJobs.Add(new KeyValuePair<int, double>(person.UsualWorkParcelId, person.AutoTimeToUsualWork));
                }

				if (personDay.WorksAtHomeFlag == 1) {
					countWorkingAtHome++;
				}
				if (person.TransitPassOwnership == 1) {
					transitPassOwnership++;
				}

				if (person.TransitPassOwnership == 1) {
					payParkWork++;
				}

			}

          

			int countMandatoryAdults = (from personDayHH in orderedPersonDays
												 where personDayHH.PatternType == 1 && personDayHH.Person.IsAdult
												 select personDayHH.PatternType).Count();
			int countMandatoryChildren = (from personDayHH in orderedPersonDays
													where personDayHH.PatternType == 1 && personDayHH.Person.IsChildUnder16
													select personDayHH.PatternType).Count();
			int countNonMandatoryAdults = (from personDayHH in orderedPersonDays
													 where personDayHH.PatternType == 2 && personDayHH.Person.IsAdult
													 select personDayHH.PatternType).Count();
			int countKidsAtHome = (from personDayHH in orderedPersonDays 
													where personDayHH.PatternType == 3 && personDayHH.Person.Age<12
													select personDayHH.PatternType).Count();

			int youngestAge = (from person in household.Persons
									 select person.Age).Min();


			if (youngestAge <= 18) {
				oldestChild = (from person in household.Persons
									where person.Age <= 18
									select person.Age).Max();
			}


            if (hhSchools.Count > 1)
            {
                //number of schools - number of unique schools gives matches
                numSchoolMatch = hhSchools.Count - (from schoolm in hhSchools
                                                    select schoolm).Distinct().Count();
            }

			double lnYoungestAge = Math.Log(1 + youngestAge);
			double lnOldestChild = Math.Log(1 + oldestChild);
            int noStudents = household.HouseholdTotals.AllStudents == 0 ? 1 : 0;
            int noWorkers = household.HouseholdTotals.AllWorkers == 0 ? 1 : 0;
            
			// NONE_OR_HOME
			var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, available[0], choice == Global.Settings.Purposes.NoneOrHome);

			alternative.Choice = Global.Settings.Purposes.NoneOrHome;
			alternative.AddUtilityTerm(1, (nCallsForTour == 2).ToFlag());
			alternative.AddUtilityTerm(2, (nCallsForTour == 3).ToFlag());
			alternative.AddUtilityTerm(3, (nCallsForTour >= 4).ToFlag());
			alternative.AddUtilityTerm(4, noCarsFlag);
			alternative.AddUtilityTerm(5, carsGrAdults);
			alternative.AddUtilityTerm(6, (countKidsAtHome>0).ToFlag());
            alternative.AddUtilityTerm(7, noStudents);
            alternative.AddUtilityTerm(8, noWorkers);


			// FULL PAIRED
			// base is two person- two worker household
			alternative = choiceProbabilityCalculator.GetAlternative(1, available[1], choice == 1);
			alternative.Choice = 1;
			alternative.AddUtilityTerm(11, 1);
			alternative.AddUtilityTerm(20, (countMandatoryAdults>2).ToFlag());
			alternative.AddUtilityTerm(13, (household.Has0To25KIncome).ToFlag());
			alternative.AddUtilityTerm(12, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(14, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(15, (countMandatoryChildren >= 3).ToFlag());
			alternative.AddUtilityTerm(16, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 0).ToFlag());
			alternative.AddUtilityTerm(17, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(18, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 2).ToFlag());
            alternative.AddUtilityTerm(19, numSchoolMatch);
			alternative.AddUtilityTerm(21, lnYoungestAge);
            alternative.AddUtilityTerm(22, (household.HouseholdTotals.DrivingAgeStudents>0).ToFlag());
            

			// FULL HalfTour 1
			alternative = choiceProbabilityCalculator.GetAlternative(2, available[2], choice == 2);
			alternative.Choice = 2;
			alternative.AddUtilityTerm(31, 1);
			alternative.AddUtilityTerm(32, (countMandatoryAdults>2).ToFlag());
			alternative.AddUtilityTerm(39, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(40, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(41,  (countMandatoryChildren >= 3).ToFlag());
			alternative.AddUtilityTerm(42, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 0).ToFlag());
			alternative.AddUtilityTerm(43, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(44, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 2).ToFlag());
            alternative.AddUtilityTerm(45, numSchoolMatch);
			alternative.AddUtilityTerm(46, (household.HouseholdTotals.DrivingAgeStudents>0).ToFlag());
			alternative.AddUtilityTerm(47, lnOldestChild);
			alternative.AddUtilityTerm(49, totAggregateLogsum);
			alternative.AddUtilityTerm(53, (household.HouseholdTotals.ChildrenUnder5 > 1).ToFlag());
			alternative.AddUtilityTerm(54, countWorkingAtHome);



			// Full HalfTour 2
			alternative = choiceProbabilityCalculator.GetAlternative(3, available[3], choice == 3);
			alternative.Choice = 3;
			alternative.AddUtilityTerm(51, 1);
            alternative.AddUtilityTerm(32, (countMandatoryAdults > 2).ToFlag());
            alternative.AddUtilityTerm(39, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 1).ToFlag());
            alternative.AddUtilityTerm(40, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 2).ToFlag());
            alternative.AddUtilityTerm(41, (countMandatoryChildren >= 3).ToFlag());
            alternative.AddUtilityTerm(42, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 0).ToFlag());
            alternative.AddUtilityTerm(43, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 1).ToFlag());
            alternative.AddUtilityTerm(44, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 2).ToFlag());
            alternative.AddUtilityTerm(45, numSchoolMatch);
            alternative.AddUtilityTerm(46, (household.HouseholdTotals.DrivingAgeStudents > 0).ToFlag());
            alternative.AddUtilityTerm(47, lnOldestChild);
            alternative.AddUtilityTerm(49, totAggregateLogsum);
            alternative.AddUtilityTerm(53, (household.HouseholdTotals.ChildrenUnder5 > 1).ToFlag());
            alternative.AddUtilityTerm(54, countWorkingAtHome);

			// PARTIAL PAIRED
			alternative = choiceProbabilityCalculator.GetAlternative(4, available[4], choice == 4);
			alternative.Choice = 4;
			alternative.AddUtilityTerm(61, 1);
			alternative.AddUtilityTerm(62, (countMandatoryAdults>2).ToFlag());
			alternative.AddUtilityTerm(69, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(70, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(72, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 0).ToFlag());
			alternative.AddUtilityTerm(73, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(74, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(75, lnYoungestAge);
			alternative.AddUtilityTerm(78, household.Has75KPlusIncome.ToFlag());
			alternative.AddUtilityTerm(86, (household.HouseholdTotals.DrivingAgeStudents>0).ToFlag());
			alternative.AddUtilityTerm(87, (countMandatoryChildren >= 3).ToFlag());
		
			// PARTIAL HalfTour 1
			alternative = choiceProbabilityCalculator.GetAlternative(5, available[5], choice == 5);
			alternative.Choice = 5;
			alternative.AddUtilityTerm(91, 1);
			alternative.AddUtilityTerm(92, (countMandatoryAdults>2).ToFlag());
			alternative.AddUtilityTerm(98, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(99, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(102, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 0).ToFlag());
			alternative.AddUtilityTerm(103, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(104, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(105, (countMandatoryChildren >= 3).ToFlag());
			alternative.AddUtilityTerm(108, lnYoungestAge);
			alternative.AddUtilityTerm(109, lnOldestChild);
			alternative.AddUtilityTerm(114, (household.Has0To25KIncome).ToFlag());
			alternative.AddUtilityTerm(115, (household.Has100KPlusIncome).ToFlag());
			alternative.AddUtilityTerm(117, (household.HouseholdTotals.ChildrenAge5Through15 > 1).ToFlag());
			alternative.AddUtilityTerm(118, (household.HouseholdTotals.PartTimeWorkers > 0).ToFlag());

			// PARTIAL HalfTour 2
			alternative = choiceProbabilityCalculator.GetAlternative(6, available[6], choice == 6);
			alternative.Choice = 6;
			alternative.AddUtilityTerm(101, 1);
			alternative.AddUtilityTerm(92, (countMandatoryAdults>2).ToFlag());
			alternative.AddUtilityTerm(98, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(99, (countMandatoryAdults == household.HouseholdTotals.Adults).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(102, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 0).ToFlag());
			alternative.AddUtilityTerm(103, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 1).ToFlag());
			alternative.AddUtilityTerm(104, (countNonMandatoryAdults > 0).ToFlag() * (countMandatoryChildren == 2).ToFlag());
			alternative.AddUtilityTerm(105, (countMandatoryChildren >= 3).ToFlag());
			alternative.AddUtilityTerm(108, lnYoungestAge);
			alternative.AddUtilityTerm(109, lnOldestChild);
			alternative.AddUtilityTerm(114, (household.Has0To25KIncome).ToFlag());
			alternative.AddUtilityTerm(115, (household.Has100KPlusIncome).ToFlag());
			alternative.AddUtilityTerm(117, (household.HouseholdTotals.ChildrenAge5Through15 > 1).ToFlag());
			alternative.AddUtilityTerm(118, (household.HouseholdTotals.PartTimeWorkers > 0).ToFlag());
			
		}
	}
}