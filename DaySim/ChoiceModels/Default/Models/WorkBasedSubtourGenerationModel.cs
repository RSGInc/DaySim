// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using System;

namespace DaySim.ChoiceModels.Default.Models {
    public class WorkBasedSubtourGenerationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "WorkBasedSubtourGenerationModel";
        private readonly int _totalAlternatives = Global.Settings.Purposes.Social + 1;
        private const int TOTAL_NESTED_ALTERNATIVES = 2;
        private const int TOTAL_LEVELS = 2;
        private const int MAX_PARAMETER = 50;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkBasedSubtourGenerationModelCoefficients, _totalAlternatives, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public int Run(ITourWrapper tour, int nCallsForTour) {
            return Run(tour, nCallsForTour, Global.Settings.Purposes.NoneOrHome);
        }

        public int Run(ITourWrapper tour, int nCallsForTour, int choice) {
            if (tour == null) {
                throw new ArgumentNullException("tour");
            }

            tour.PersonDay.ResetRandom(30 + tour.Sequence + nCallsForTour - 1);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return choice;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator((tour.Id * 397) ^ nCallsForTour);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                if (tour.PersonDay.GetTotalStops() > 0) {
                    RunModel(choiceProbabilityCalculator, tour, nCallsForTour, choice);

                    choiceProbabilityCalculator.WriteObservation();
                }
            } else {
                if (tour.PersonDay.GetTotalStops() > 0) {
                    RunModel(choiceProbabilityCalculator, tour, nCallsForTour);

                    var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
                    choice = (int)chosenAlternative.Choice;
                } else {
                    choice = Global.Settings.Purposes.NoneOrHome;
                }
            }

            return choice;
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, int nCallsForTour, int choice = Constants.DEFAULT_VALUE) {
            var person = tour.Person;
            var personDay = tour.PersonDay;

            //            var foodRetailServiceMedicalQtrMileLog = tour.DestinationParcel.FoodRetailServiceMedicalQtrMileLogBuffer1();
            //            var mixedUseIndex = tour.DestinationParcel.MixedUse4Index1();
            var k8HighSchoolQtrMileLog = tour.DestinationParcel.K8HighSchoolQtrMileLogBuffer1();
            var carOwnership = person.GetCarOwnershipSegment();

            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);
            //            var notUsualWorkParcelFlag = tour.DestinationParcel.NotUsualWorkParcelFlag(person.UsualWorkParcelId);

            var votALSegment = tour.GetVotALSegment();

            var workTaSegment = tour.DestinationParcel.TransitAccessSegment();
            var workAggregateLogsum = Global.AggregateLogsums[tour.DestinationParcel.ZoneId]
                [Global.Settings.Purposes.WorkBased][carOwnership][votALSegment][workTaSegment];

            // NONE_OR_HOME

            var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

            alternative.Choice = Global.Settings.Purposes.NoneOrHome;

            alternative.AddUtilityTerm(15, (nCallsForTour > 1).ToFlag());
            alternative.AddUtilityTerm(16, Math.Log(personDay.HomeBasedTours));
            alternative.AddUtilityTerm(18, personDay.TwoOrMoreWorkToursExist().ToFlag());
            //            alternative.AddUtility(19, notUsualWorkParcelFlag);
            alternative.AddUtilityTerm(22, noCarsFlag);
            alternative.AddUtilityTerm(23, carCompetitionFlag);
            alternative.AddUtilityTerm(32, workAggregateLogsum);
            //            alternative.AddUtility(32, mixedUseIndex);

            alternative.AddNestedAlternative(11, 0, 50);

            // WORK

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, personDay.WorkStops > 0, choice == Global.Settings.Purposes.Work);

            alternative.Choice = Global.Settings.Purposes.Work;

            alternative.AddUtilityTerm(1, 1);

            alternative.AddNestedAlternative(12, 1, 50);

            // SCHOOL

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, personDay.SchoolStops > 0, choice == Global.Settings.Purposes.School);

            alternative.Choice = Global.Settings.Purposes.School;

            alternative.AddUtilityTerm(3, 1);

            alternative.AddNestedAlternative(12, 1, 50);

            // ESCORT

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, personDay.EscortStops > 0, choice == Global.Settings.Purposes.Escort);

            alternative.Choice = Global.Settings.Purposes.Escort;

            alternative.AddUtilityTerm(4, 1);
            alternative.AddUtilityTerm(39, k8HighSchoolQtrMileLog);

            alternative.AddNestedAlternative(12, 1, 50);

            // PERSONAL_BUSINESS

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, personDay.PersonalBusinessStops > 0, choice == Global.Settings.Purposes.PersonalBusiness);

            alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

            alternative.AddUtilityTerm(6, 1);

            alternative.AddNestedAlternative(12, 1, 50);

            // SHOPPING

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, personDay.ShoppingStops > 0, choice == Global.Settings.Purposes.Shopping);

            alternative.Choice = Global.Settings.Purposes.Shopping;

            alternative.AddUtilityTerm(8, 1);

            alternative.AddNestedAlternative(12, 1, 50);

            // MEAL

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, personDay.MealStops > 0, choice == Global.Settings.Purposes.Meal);

            alternative.Choice = Global.Settings.Purposes.Meal;

            alternative.AddUtilityTerm(10, 1);

            alternative.AddNestedAlternative(12, 1, 50);

            // SOCIAL

            alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, personDay.SocialStops > 0, choice == Global.Settings.Purposes.Social);

            alternative.Choice = Global.Settings.Purposes.Social;

            alternative.AddUtilityTerm(13, 1);

            alternative.AddNestedAlternative(12, 1, 50);
        }
    }
}