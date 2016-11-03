// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaySim.ChoiceModels.H.Models {
    public class JointTourGenerationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HJointTourGenerationModel";
        private const int TOTAL_ALTERNATIVES = 10;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 200;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.JointTourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public int Run(HouseholdDayWrapper householdDay, int nCallsForTour) {
            return Run(householdDay, nCallsForTour, Global.Settings.Purposes.NoneOrHome);
        }

        public int Run(HouseholdDayWrapper householdDay, int nCallsForTour, int choice) {
            if (householdDay == null) {
                throw new ArgumentNullException("householdDay");
            }

            householdDay.ResetRandom(935 + nCallsForTour);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return choice;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ nCallsForTour);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour, choice);

                choiceProbabilityCalculator.WriteObservation();
            } else {
                RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
                choice = (int)chosenAlternative.Choice;
            }

            return choice;
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int nCallsForTour, int choice = Constants.DEFAULT_VALUE) {

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

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

            int countNonMandatory = 0;
            int countMandatory = 0;
            int countWorkingAtHome = 0;
            int countAge5to8 = 0;
            int countAge9to12 = 0;
            int countAge13to15 = 0;
            int countAgeUnd13 = 0;
            int countAdultFemale = 0;
            int countAdultMale = 0;

            int youngestAge = 150;
            int oldestAge = 0;

            int mandatoryHTours = 0;
            int mandatoryHStops = 0;

            int[] mandPerstype = new int[8];
            int[] nonMandPerstype = new int[8];
            int[] atHomePersType = new int[8];

            double totHomeSchoolTime = 0;
            double countHomeSchoolTime = 0;
            double aveSchoolTime = 0;

            double totHomeWorkTime = 0;
            double countHomeWorkTime = 0;
            double aveWorkTime = 0;

            List<KeyValuePair<int, double>> hhSchools = new List<KeyValuePair<int, double>>();
            List<KeyValuePair<int, double>> hhJobs = new List<KeyValuePair<int, double>>();


            int count = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
                var person = personDay.Person;
                count++;
                if (count > 8) {
                    break;
                }
                if (personDay.Person.Age >= 5 && personDay.Person.Age <= 8) {
                    countAge5to8++;
                }

                if (personDay.Person.Age >= 9 && personDay.Person.Age <= 12) {
                    countAge9to12++;
                }

                if (personDay.Person.Age >= 13 && personDay.Person.Age <= 15) {
                    countAge13to15++;
                }

                if (personDay.Person.IsAdultFemale) {
                    countAdultFemale++;
                } else if (personDay.Person.IsAdultMale) {
                    countAdultMale++;
                }

                if (personDay.Person.Age < youngestAge) {
                    youngestAge = personDay.Person.Age;
                }

                if (personDay.Person.Age > oldestAge) {
                    oldestAge = personDay.Person.Age;
                }

                mandatoryHTours = mandatoryHTours + personDay.WorkTours + personDay.SchoolTours;
                mandatoryHStops = mandatoryHStops + (personDay.WorkStops > 0).ToFlag() + (personDay.SchoolTours > 0).ToFlag();

                if (personDay.WorksAtHomeFlag == 1) {
                    countWorkingAtHome++;
                }
                if (personDay.PatternType == 1) {
                    countMandatory++;
                    mandPerstype[personDay.Person.PersonType - 1]++;
                }
                if (personDay.PatternType == 2) {
                    countNonMandatory++;
                    nonMandPerstype[personDay.Person.PersonType - 1]++;
                }
                if (personDay.PatternType == 3) {

                    atHomePersType[personDay.Person.PersonType - 1]++;
                }

            }


            youngestAge = youngestAge == 150 ? 0 : youngestAge;

            // NONE_OR_HOME

            var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

            alternative.Choice = Global.Settings.Purposes.NoneOrHome;

            //alternative.AddUtilityTerm(1, (nCallsForTour == 1).ToFlag());
            alternative.AddUtilityTerm(2, (nCallsForTour == 2).ToFlag());
            alternative.AddUtilityTerm(3, (nCallsForTour >= 3).ToFlag());
            alternative.AddUtilityTerm(12, atHomePersType[4]);
            // alternative.AddUtilityTerm(15, Math.Log(1+aveWorkTime));
            //  alternative.AddUtilityTerm(16, Math.Log(1+aveSchoolTime));

            // WORK
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, false, choice == Global.Settings.Purposes.Work);
            alternative.Choice = Global.Settings.Purposes.Work;
            alternative.AddUtilityTerm(202, 1);


            //  SCHOOL
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);
            alternative.Choice = Global.Settings.Purposes.School;
            alternative.AddUtilityTerm(203, 1);


            // ESCORT
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, true, choice == Global.Settings.Purposes.Escort);
            alternative.Choice = Global.Settings.Purposes.Escort;

            alternative.AddUtilityTerm(151, 1);
            alternative.AddUtilityTerm(152, nonMandPerstype[0]);
            alternative.AddUtilityTerm(153, nonMandPerstype[1]);
            alternative.AddUtilityTerm(154, nonMandPerstype[2]);
            alternative.AddUtilityTerm(155, nonMandPerstype[3]);
            alternative.AddUtilityTerm(156, nonMandPerstype[4]);
            alternative.AddUtilityTerm(157, nonMandPerstype[5]);
            alternative.AddUtilityTerm(158, nonMandPerstype[6]);
            alternative.AddUtilityTerm(159, nonMandPerstype[7]);
            alternative.AddUtilityTerm(160, countMandatory);
            alternative.AddUtilityTerm(162, countWorkingAtHome);
            alternative.AddUtilityTerm(165, countAdultFemale);
            alternative.AddUtilityTerm(166, countAdultMale);
            alternative.AddUtilityTerm(167, countAge5to8);
            alternative.AddUtilityTerm(168, countAge9to12);
            alternative.AddUtilityTerm(169, countAge13to15);
            alternative.AddUtilityTerm(170, Math.Log(1 + youngestAge));
            alternative.AddUtilityTerm(173, Math.Log(1 + household.ResidenceParcel.HouseholdsBuffer2));


            // PERSONAL_BUSINESS
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, true, choice == Global.Settings.Purposes.PersonalBusiness);
            alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

            alternative.AddUtilityTerm(21, 1);
            alternative.AddUtilityTerm(22, nonMandPerstype[0]);
            alternative.AddUtilityTerm(23, nonMandPerstype[1]);
            alternative.AddUtilityTerm(24, nonMandPerstype[2]);
            alternative.AddUtilityTerm(25, nonMandPerstype[3]);
            alternative.AddUtilityTerm(26, nonMandPerstype[4]);
            alternative.AddUtilityTerm(27, nonMandPerstype[5]);
            alternative.AddUtilityTerm(28, nonMandPerstype[6]);
            alternative.AddUtilityTerm(29, nonMandPerstype[7]);
            alternative.AddUtilityTerm(30, countMandatory);
            alternative.AddUtilityTerm(37, Math.Log(1 + oldestAge));


            // SHOPPING
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, true, choice == Global.Settings.Purposes.Shopping);
            alternative.Choice = Global.Settings.Purposes.Shopping;

            alternative.AddUtilityTerm(41, 1);
            alternative.AddUtilityTerm(42, nonMandPerstype[0]);
            alternative.AddUtilityTerm(43, nonMandPerstype[1]);
            alternative.AddUtilityTerm(44, nonMandPerstype[2]);
            alternative.AddUtilityTerm(45, nonMandPerstype[3]);
            alternative.AddUtilityTerm(46, nonMandPerstype[4]);
            alternative.AddUtilityTerm(47, nonMandPerstype[5]);
            alternative.AddUtilityTerm(48, nonMandPerstype[6]);
            alternative.AddUtilityTerm(49, nonMandPerstype[7]);
            alternative.AddUtilityTerm(50, countMandatory);
            alternative.AddUtilityTerm(51, shoppingAggregateLogsum);
            alternative.AddUtilityTerm(52, householdDay.Household.Has0To25KIncome.ToFlag());
            alternative.AddUtilityTerm(58, Math.Log(1 + youngestAge));
            alternative.AddUtilityTerm(59, Math.Log(1 + oldestAge));


            // MEAL
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, true, choice == Global.Settings.Purposes.Meal);
            alternative.Choice = Global.Settings.Purposes.Meal;

            alternative.AddUtilityTerm(61, 1);

            alternative.AddUtilityTerm(62, nonMandPerstype[0]);
            alternative.AddUtilityTerm(63, nonMandPerstype[1]);
            alternative.AddUtilityTerm(64, nonMandPerstype[2]);
            alternative.AddUtilityTerm(65, nonMandPerstype[3]);
            alternative.AddUtilityTerm(66, nonMandPerstype[4]);
            alternative.AddUtilityTerm(67, nonMandPerstype[5]);
            alternative.AddUtilityTerm(68, nonMandPerstype[6]);
            alternative.AddUtilityTerm(69, nonMandPerstype[7]);
            alternative.AddUtilityTerm(70, countMandatory);
            alternative.AddUtilityTerm(71, mealAggregateLogsum);
            alternative.AddUtilityTerm(78, Math.Log(1 + youngestAge));
            alternative.AddUtilityTerm(79, Math.Log(1 + oldestAge));
            alternative.AddUtilityTerm(80, Math.Log(1 + household.ResidenceParcel.HouseholdsBuffer2));


            // SOCIAL
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, true, choice == Global.Settings.Purposes.Social);
            alternative.Choice = Global.Settings.Purposes.Social;

            alternative.AddUtilityTerm(81, 1);
            alternative.AddUtilityTerm(82, nonMandPerstype[0]);
            alternative.AddUtilityTerm(83, nonMandPerstype[1]);
            alternative.AddUtilityTerm(84, nonMandPerstype[2]);
            alternative.AddUtilityTerm(85, nonMandPerstype[3]);
            alternative.AddUtilityTerm(86, nonMandPerstype[4]);
            alternative.AddUtilityTerm(87, nonMandPerstype[5]);
            alternative.AddUtilityTerm(88, nonMandPerstype[6]);
            alternative.AddUtilityTerm(89, nonMandPerstype[7]);
            alternative.AddUtilityTerm(90, countMandatory);
            alternative.AddUtilityTerm(91, socialAggregateLogsum);
            alternative.AddUtilityTerm(93, Math.Log(1 + householdDay.Household.ResidenceParcel.HouseholdsBuffer1));
            alternative.AddUtilityTerm(96, countAge5to8);
            alternative.AddUtilityTerm(97, countAge9to12);
            alternative.AddUtilityTerm(98, countAge13to15);
            alternative.AddUtilityTerm(100, Math.Log(1 + oldestAge));

            // RECREATION
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Recreation, true, choice == Global.Settings.Purposes.Recreation);
            alternative.Choice = Global.Settings.Purposes.Recreation;

            alternative.AddUtilityTerm(101, 1);

            alternative.AddUtilityTerm(102, nonMandPerstype[0]);
            alternative.AddUtilityTerm(103, nonMandPerstype[1]);
            alternative.AddUtilityTerm(104, nonMandPerstype[2]);
            alternative.AddUtilityTerm(105, nonMandPerstype[3]);
            alternative.AddUtilityTerm(106, nonMandPerstype[4]);
            alternative.AddUtilityTerm(107, nonMandPerstype[5]);
            alternative.AddUtilityTerm(108, nonMandPerstype[6]);
            alternative.AddUtilityTerm(109, nonMandPerstype[7]);
            alternative.AddUtilityTerm(110, countMandatory);
            alternative.AddUtilityTerm(111, totalAggregateLogsum);
            alternative.AddUtilityTerm(112, Math.Log(1 + householdDay.Household.ResidenceParcel.OpenSpaceType1Buffer1));
            alternative.AddUtilityTerm(113, countAdultFemale);
            alternative.AddUtilityTerm(114, countAdultMale);
            alternative.AddUtilityTerm(115, countAge5to8);
            alternative.AddUtilityTerm(116, countAge9to12);
            alternative.AddUtilityTerm(117, countAge13to15);
            alternative.AddUtilityTerm(118, Math.Log(1 + youngestAge));



            // MEDICAL
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Medical, true, choice == Global.Settings.Purposes.Medical);
            alternative.Choice = Global.Settings.Purposes.Medical;

            alternative.AddUtilityTerm(121, 1);

            alternative.AddUtilityTerm(122, nonMandPerstype[0]);
            alternative.AddUtilityTerm(123, nonMandPerstype[1]);
            alternative.AddUtilityTerm(124, nonMandPerstype[2]);
            alternative.AddUtilityTerm(125, nonMandPerstype[3]);
            alternative.AddUtilityTerm(126, nonMandPerstype[4]);
            alternative.AddUtilityTerm(127, nonMandPerstype[5]);
            alternative.AddUtilityTerm(128, nonMandPerstype[6]);
            alternative.AddUtilityTerm(129, nonMandPerstype[7]);
            alternative.AddUtilityTerm(130, countMandatory);
            alternative.AddUtilityTerm(131, totalAggregateLogsum);

        }
    }
}
