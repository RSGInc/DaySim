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
    public class WorkBasedSubtourGenerationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HWorkBasedSubtourGenerationModel";
        private const int TOTAL_ALTERNATIVES = 10;
        private const int TOTAL_NESTED_ALTERNATIVES = 1;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 150;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkBasedSubtourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public int Run(TourWrapper tour, int nCallsForTour, HouseholdDayWrapper householdDay) {
            return Run(tour, nCallsForTour, householdDay, Global.Settings.Purposes.NoneOrHome);
        }

        public int Run(TourWrapper tour, int nCallsForTour, HouseholdDayWrapper householdDay, int choice) {
            if (tour == null) {
                throw new ArgumentNullException("tour");
            }

            tour.PersonDay.Person.ResetRandom(908 + (tour.Sequence - 1) * 3 + nCallsForTour);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return choice;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator((tour.Id * 397) ^ nCallsForTour);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                if (tour.PersonDay.GetTotalStops() > 0) {
                    RunModel(choiceProbabilityCalculator, tour, nCallsForTour, householdDay, choice);

                    choiceProbabilityCalculator.WriteObservation();
                }
            } else {
                if (tour.PersonDay.GetTotalStops() > 0) {
                    RunModel(choiceProbabilityCalculator, tour, nCallsForTour, householdDay);

                    var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
                    choice = (int)chosenAlternative.Choice;
                } else {
                    choice = Global.Settings.Purposes.NoneOrHome;
                }
            }

            return choice;
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, TourWrapper tour, int nCallsForTour, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
            var person = tour.Person;
            var personDay = tour.PersonDay;

            var adultFemaleFlag = person.IsAdultFemale.ToFlag();
            var partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();

            var foodBuffer2 = tour.DestinationParcel.EmploymentFoodBuffer2;
            var medBuffer2 = tour.DestinationParcel.EmploymentMedicalBuffer2;
            var intDensBuffer2 = tour.DestinationParcel.IntersectionDensity34Buffer2();
            var serviceBuffer2 = tour.DestinationParcel.EmploymentServiceBuffer2;
            var totEmpBuffer2 = tour.DestinationParcel.EmploymentTotalBuffer2;
            var totHHBuffer2 = tour.DestinationParcel.HouseholdsBuffer2;
            var openSpaceBuffer2 = tour.DestinationParcel.OpenSpaceType1Buffer2;
            var mixedUse = tour.DestinationParcel.MixedUse2Index2();
            //var retailBuffer2 = tour.DestinationParcel.EmploymentRetailBuffer2;
            //var retailBuffer1 = tour.DestinationParcel.EmploymentRetailBuffer1;

            var carOwnership = person.GetCarOwnershipSegment();
            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            //var carCompetitionFlag = AggregateLogsumsCalculator.GetCarCompetitionFlag(carOwnership);

            var votALSegment = tour.GetVotALSegment();

            var workTaSegment = tour.DestinationParcel.TransitAccessSegment();
            var workAggregateLogsum = Global.AggregateLogsums[tour.DestinationParcel.ZoneId]
            [Global.Settings.Purposes.WorkBased][carOwnership][votALSegment][workTaSegment];
            var shopAggregateLogsum = Global.AggregateLogsums[tour.DestinationParcel.ZoneId]
            [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][workTaSegment];
            //var mealAggregateLogsum = Global.AggregateLogsums[tour.DestinationParcel.ZoneId]
            //[Global.Settings.Purposes.Meal][carOwnership][votALSegment][workTaSegment];
            //var persBusAggregateLogsum = Global.AggregateLogsums[tour.DestinationParcel.ZoneId]
            //[Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][workTaSegment];
            //var socialAggregateLogsum = Global.AggregateLogsums[tour.DestinationParcel.ZoneId]
            //[Global.Settings.Purposes.Social][carOwnership][votALSegment][workTaSegment];

            int numStopPurposes = (personDay.SimulatedEscortStops > 1).ToFlag() + (personDay.SimulatedShoppingStops > 1).ToFlag() + (personDay.SimulatedMealStops > 1).ToFlag() +
                                                          (personDay.SimulatedPersonalBusinessStops > 1).ToFlag() + (personDay.SimulatedSocialStops > 1).ToFlag() +
                                                          (personDay.SimulatedRecreationStops > 1).ToFlag() + (personDay.SimulatedMedicalStops > 1).ToFlag();

            var workDestinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
            var workDestinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
            double workLogsum = 0;
            var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualWorkParcel, workDestinationArrivalTime, workDestinationDepartureTime, person.Household.VehiclesAvailable);
            workLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();

            //double logTimeAtWork = Math.Log(1 + (workDestinationDepartureTime - workDestinationArrivalTime) / 60);

            //int countMandatoryKids = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 1 && personDayHH.Person.Age < 12 select personDayHH.PatternType).Count();
            //int countNonMandatory = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 2 select personDayHH.PatternType).Count();
            //int countAtHome = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 3 select personDayHH.PatternType).Count();
            int countNonMandatoryKids = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 2 && personDayHH.Person.Age < 12 select personDayHH.PatternType).Count();

            // NONE_OR_HOME

            var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

            alternative.Choice = Global.Settings.Purposes.NoneOrHome;

            alternative.AddUtilityTerm(1, (nCallsForTour > 1).ToFlag());
            alternative.AddUtilityTerm(3, personDay.TwoOrMoreWorkToursExist().ToFlag());
            alternative.AddUtilityTerm(4, noCarsFlag);
            //alternative.AddUtilityTerm(5, carCompetitionFlag);
            alternative.AddUtilityTerm(6, partTimeWorkerFlag);
            //alternative.AddUtilityTerm(8, (person.UsualModeToWork != Global.Settings.Modes.Sov).ToFlag());
            alternative.AddUtilityTerm(10, person.TransitPassOwnership);
            //alternative.AddUtilityTerm(15, logTimeAtWork);
            alternative.AddUtilityTerm(17, numStopPurposes);
            alternative.AddUtilityTerm(18, Math.Log(personDay.GetTotalCreatedTours() + 1));

            // WORK

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, personDay.WorkStops > 0, choice == Global.Settings.Purposes.Work);

            alternative.Choice = Global.Settings.Purposes.Work;

            alternative.AddUtilityTerm(21, 1);
            alternative.AddUtilityTerm(22, Math.Log(1 + totEmpBuffer2));
            //alternative.AddUtilityTerm(23, (person.Household.Income<30000).ToFlag());
            alternative.AddUtilityTerm(24, (person.Household.Has100KPlusIncome).ToFlag());
            alternative.AddUtilityTerm(25, workLogsum);

            // SCHOOL
            //no observations in the PSRC dataset
            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);

            alternative.Choice = Global.Settings.Purposes.School;

            //alternative.AddUtilityTerm(3, 1);

            // ESCORT

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, personDay.EscortStops > 0, choice == Global.Settings.Purposes.Escort);

            alternative.Choice = Global.Settings.Purposes.Escort;

            alternative.AddUtilityTerm(31, 1);
            //alternative.AddUtilityTerm(33, k8HighSchoolQtrMileLog);
            alternative.AddUtilityTerm(36, countNonMandatoryKids);
            alternative.AddUtilityTerm(37, adultFemaleFlag);
            //alternative.AddUtilityTerm(38, person.Household.Size);
            //alternative.AddUtilityTerm(39, householdDay.Household.HouseholdTotals.ChildrenAge5Through15);
            //alternative.AddUtilityTerm(40, partTimeWorkerFlag);
            alternative.AddUtilityTerm(39, workLogsum);

            // PERSONAL_BUSINESS

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, personDay.PersonalBusinessStops > 0, choice == Global.Settings.Purposes.PersonalBusiness);

            alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

            alternative.AddUtilityTerm(41, 1);
            //alternative.AddUtilityTerm(43, persBusAggregateLogsum);
            alternative.AddUtilityTerm(45, Math.Log(1 + totEmpBuffer2));
            //alternative.AddUtilityTerm(48, (person.Household.Income>90000).ToFlag());
            alternative.AddUtilityTerm(49, (person.Household.HouseholdTotals.ChildrenUnder16));


            // SHOPPING

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, personDay.ShoppingStops > 0, choice == Global.Settings.Purposes.Shopping);

            alternative.Choice = Global.Settings.Purposes.Shopping;

            alternative.AddUtilityTerm(51, 1);
            //alternative.AddUtilityTerm(53, retailBuffer1);
            //alternative.AddUtilityTerm(54, partTimeWorkerFlag);
            alternative.AddUtilityTerm(55, (person.Household.Has100KPlusIncome).ToFlag());
            alternative.AddUtilityTerm(57, person.Household.HouseholdTotals.ChildrenUnder16);
            alternative.AddUtilityTerm(58, shopAggregateLogsum);
            //alternative.AddUtilityTerm(59, person.Household.Size);
            //alternative.AddUtilityTerm(59, (person.Household.Income<30000).ToFlag());


            // MEAL

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, personDay.MealStops > 0, choice == Global.Settings.Purposes.Meal);

            alternative.Choice = Global.Settings.Purposes.Meal;

            alternative.AddUtilityTerm(71, 1);
            alternative.AddUtilityTerm(73, Math.Log(1 + foodBuffer2));
            alternative.AddUtilityTerm(74, mixedUse);
            alternative.AddUtilityTerm(75, intDensBuffer2);
            //alternative.AddUtilityTerm(76, (person.Household.Income<30000).ToFlag());
            //alternative.AddUtilityTerm(77, person.Household.HouseholdTotals.ChildrenUnder16);

            // SOCIAL

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, personDay.SocialStops > 0, choice == Global.Settings.Purposes.Social);

            alternative.Choice = Global.Settings.Purposes.Social;

            alternative.AddUtilityTerm(91, 1);
            alternative.AddUtilityTerm(93, person.Household.HouseholdTotals.ChildrenUnder16);
            alternative.AddUtilityTerm(94, (person.Age < 35).ToFlag());
            //alternative.AddUtilityTerm(115, workAggregateLogsum);
            alternative.AddUtilityTerm(96, Math.Log(1 + totHHBuffer2 + totEmpBuffer2));
            alternative.AddUtilityTerm(97, (person.Household.Income < 30000).ToFlag());
            //alternative.AddUtilityTerm(118, person.Household.Has100KPlusIncome.ToFlag());

            // RECREATION

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Recreation, personDay.RecreationStops > 0, choice == Global.Settings.Purposes.Recreation);

            alternative.Choice = Global.Settings.Purposes.Recreation;

            alternative.AddUtilityTerm(111, 1);
            alternative.AddUtilityTerm(113, person.Household.HouseholdTotals.ChildrenUnder16);
            alternative.AddUtilityTerm(114, (person.Age < 35).ToFlag());
            alternative.AddUtilityTerm(116, Math.Log(1 + totHHBuffer2 + totEmpBuffer2));
            alternative.AddUtilityTerm(117, (person.Household.Income < 30000).ToFlag());
            alternative.AddUtilityTerm(118, (person.Household.Income > 100000).ToFlag());
            alternative.AddUtilityTerm(119, Math.Log(1 + openSpaceBuffer2));

            // MEDICAL

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Medical, personDay.MedicalStops > 0, choice == Global.Settings.Purposes.Medical);

            alternative.Choice = Global.Settings.Purposes.Medical;

            alternative.AddUtilityTerm(121, 1);
            alternative.AddUtilityTerm(123, adultFemaleFlag);
            alternative.AddUtilityTerm(124, (person.Age > 65).ToFlag());
            alternative.AddUtilityTerm(125, Math.Log(1 + medBuffer2));
            //alternative.AddUtilityTerm(126, workAggregateLogsum);

            //alternative.AddNestedAlternative(12, 1, 60);


        }
    }
}