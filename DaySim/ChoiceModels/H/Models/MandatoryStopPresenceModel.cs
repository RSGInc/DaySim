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
    public class MandatoryStopPresenceModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HMandatoryStopPresenceModel";
        private const int TOTAL_ALTERNATIVES = 4;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 80;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.MandatoryStopPresenceModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public void Run(IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay) {
            if (personDay == null) {
                throw new ArgumentNullException("personDay");
            }

            personDay.Person.ResetRandom(961);

            int choice = 0;

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

                choice = Math.Min(personDay.WorkStops, 1) + 2 * Math.Min(personDay.SchoolStops, 1);


                RunModel(choiceProbabilityCalculator, personDay, householdDay, choice);

                choiceProbabilityCalculator.WriteObservation();
            } else if (Global.Configuration.TestEstimationModelInApplicationMode) {

                Global.Configuration.IsInEstimationMode = false;

                RunModel(choiceProbabilityCalculator, personDay, householdDay);

                var observedChoice = Math.Min(personDay.WorkStops, 1) + 2 * Math.Min(personDay.SchoolStops, 1);

                var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, observedChoice);

                Global.Configuration.IsInEstimationMode = true;
            } else {
                RunModel(choiceProbabilityCalculator, personDay, householdDay);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
                choice = (int)chosenAlternative.Choice;

                if (choice == 1 || choice == 3) {
                    personDay.WorkStops = 1;
                }
                if (choice == 2 || choice == 3) {
                    personDay.SchoolStops = 1;
                }
            }
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {
            var household = personDay.Household;
            var person = personDay.Person;

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

            int countMandatory = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 1 select personDayHH.PatternType).Count();
            int countNonMandatory = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 2 select personDayHH.PatternType).Count();
            int countAtHome = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 3 select personDayHH.PatternType).Count();

            var carOwnership =
                        household.VehiclesAvailable == 0
                            ? Global.Settings.CarOwnerships.NoCars
                            : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
                                ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

            Double workTourLogsum = 0;
            Double schoolPclUniStu = 0;
            Double schoolPclStudents = 0;
            Double schoolIntrDens = 0;
            int noUsualWorkZone = 1;


            if (personDay.Person.UsualWorkParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
                if (personDay.Person.UsualDeparturePeriodFromWork != Constants.DEFAULT_VALUE && personDay.Person.UsualArrivalPeriodToWork != Constants.DEFAULT_VALUE) {
                    var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, personDay.Person.UsualArrivalPeriodToWork, personDay.Person.UsualDeparturePeriodFromWork, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
                    workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();

                } else {
                    var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
                    workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
                }

                noUsualWorkZone = 0;

            }

            if (personDay.Person.UsualSchoolParcelId != 0 && personDay.Person.UsualSchoolParcelId != -1 && personDay.Person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
                schoolPclUniStu = Math.Log(1 + (personDay.Person.UsualSchoolParcel.StudentsUniversityBuffer2)) / 10;
                var schoolNestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.TwoPM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
                schoolPclStudents = Math.Log(1 + (personDay.Person.UsualSchoolParcel.GetStudentsK12())) / 10;
            }


            // No mandatory stops
            var alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
            alternative.Choice = 0;

            // Work stop(s)
            alternative = choiceProbabilityCalculator.GetAlternative(1, personDay.Person.IsWorker, choice == 1);
            alternative.Choice = 1;
            alternative.AddUtilityTerm(21, 1);
            alternative.AddUtilityTerm(22, (personDay.WorkTours + personDay.SchoolTours > 1).ToFlag());
            alternative.AddUtilityTerm(24, workTourLogsum);
            alternative.AddUtilityTerm(26, household.Has0To25KIncome.ToFlag());
            alternative.AddUtilityTerm(27, (person.Age < 30).ToFlag());
            alternative.AddUtilityTerm(29, noUsualWorkZone);
            alternative.AddUtilityTerm(30, countMandatory);
            alternative.AddUtilityTerm(31, countAtHome);
            alternative.AddUtilityTerm(33, person.IsPartTimeWorker.ToFlag());
            alternative.AddUtilityTerm(34, person.IsUniversityStudent.ToFlag());
            alternative.AddUtilityTerm(35, household.Has100KPlusIncome.ToFlag());

            // School stop(s)
            alternative = choiceProbabilityCalculator.GetAlternative(2, personDay.Person.IsStudent, choice == 2);
            alternative.Choice = 2;
            alternative.AddUtilityTerm(41, 1);
            alternative.AddUtilityTerm(42, (personDay.SchoolTours == 0).ToFlag());
            alternative.AddUtilityTerm(45, person.IsChildUnder5.ToFlag());
            alternative.AddUtilityTerm(46, person.IsUniversityStudent.ToFlag());
            alternative.AddUtilityTerm(49, (household.HouseholdTotals.AllWorkers >= 2).ToFlag());
            alternative.AddUtilityTerm(50, carCompetitionFlag + noCarsFlag);
            alternative.AddUtilityTerm(53, (household.HouseholdTotals.ChildrenUnder16 > 2).ToFlag());
            alternative.AddUtilityTerm(54, schoolPclStudents);
            alternative.AddUtilityTerm(55, schoolPclUniStu);
            alternative.AddUtilityTerm(56, (person.Age > 25).ToFlag());
            alternative.AddUtilityTerm(59, personDay.Person.Household.ResidenceParcel.C34RatioBuffer1());


            // Work and school stops
            alternative = choiceProbabilityCalculator.GetAlternative(3, (personDay.Person.IsWorker && personDay.Person.IsStudent), choice == 3);
            alternative.Choice = 3;
            alternative.AddUtilityTerm(61, 1);



        }
    }
}