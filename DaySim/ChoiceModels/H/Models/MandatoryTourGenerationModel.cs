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
using DaySim.Framework.DomainModels.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaySim.ChoiceModels.H.Models {
    public class MandatoryTourGenerationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HMandatoryTourGenerationModel";
        private const int TOTAL_ALTERNATIVES = 4;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 100;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.MandatoryTourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public int Run(IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int nCallsForTour, int[] simulatedMandatoryTours) {
            return Run(personDay, householdDay, nCallsForTour, simulatedMandatoryTours, Global.Settings.Purposes.NoneOrHome);
        }

        public int Run(IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int nCallsForTour, int[] simulatedMandatoryTours, int choice) {

            // to know what the last choice was for a person on the previous step


            if (personDay == null) {
                throw new ArgumentNullException("personDay");
            }

            personDay.Person.ResetRandom(904 + nCallsForTour);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return choice;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((personDay.Person.Id * 10 + personDay.Day) * 397) ^ nCallsForTour);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

                RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours, choice);
                choiceProbabilityCalculator.WriteObservation();

            } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
                Global.Configuration.IsInEstimationMode = false;
                RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours);
                var observedChoice = choice;
                var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility, personDay.Id, choice);
                Global.Configuration.IsInEstimationMode = true;
            }

              //                else if (Global.Configuration.TestEstimationModelInApplicationMode==true){

              //                Global.Configuration.IsInEstimationMode = false;

              //                if (householdDay.Household.Id == 2015) {
              //                    bool testbreak = true;
              //                }
              //                RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours);

              // need to determine the choice on this particular simulated tour
              //                int[] totalMandatoryTours = new int[4];

              //                totalMandatoryTours[1] = personDay.UsualWorkplaceTours;
              //                totalMandatoryTours[2] = personDay.WorkTours - totalMandatoryTours[1];
              //                totalMandatoryTours[3] = personDay.SchoolTours;
              //                totalMandatoryTours[0] = totalMandatoryTours[1] + totalMandatoryTours[2] + totalMandatoryTours[3];
              //                if (personDay.UsualWorkplaceTours + personDay.SchoolTours > 0) {
              //                            personDay.HasMandatoryTourToUsualLocation = true;
              //                        }

              //using nCallsForTour - 1 will give the choice
              //                if (nCallsForTour - 1 < totalMandatoryTours[1]) { choice = 1; }
              //                else if (nCallsForTour - 1 < totalMandatoryTours[1] + totalMandatoryTours[2]) { choice = 2; }
              //                else if (nCallsForTour - 1 < totalMandatoryTours[0]) { choice = 3; }
              //                else { choice = 0; }

              //                var observedChoice = choice ;
              //                var simulatedChoice =choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility, personDay.Id, observedChoice);

              //                int tourPurpose =0;

              //                    if ( simulatedChoice!= null)
              //                    {
              //                    tourPurpose = (int) simulatedChoice.Choice;
              //                    }

              //                choice = tourPurpose;

              //                Global.Configuration.IsInEstimationMode = true;
              //            }

              else {
                if (householdDay.Household.Id == 2015) {
                    bool testbreak = true;
                }
                RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours);
                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
                int tourPurpose = (int)chosenAlternative.Choice;
                if (tourPurpose == 1) {
                    personDay.UsualWorkplaceTours++;
                    personDay.WorkTours++;
                } else if (tourPurpose == 2) {
                    personDay.WorkTours++;
                } else if (tourPurpose == 3) {
                    personDay.SchoolTours++;
                }
                choice = tourPurpose;
            }

            return choice;
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonDayWrapper personDay, IHouseholdDayWrapper householdDay, int nCallsForTour, int[] simulatedMandatoryTours, int choice = Constants.DEFAULT_VALUE) {
            var household = personDay.Household;

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();


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

            Double workTourLogsum = 0;
            Double schoolTourLogsum = 0;
            Double schoolPclUniStu = 0;
            Double schoolPclStudents = 0;
            Double schoolIntrDens = 0;
            Double workPclWrkrs = 0;
            Double workIntrDens = 0;
            int noUsualWorkZone = 1;


            if (personDay.Person.UsualWorkParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
                if (personDay.Person.UsualDeparturePeriodFromWork != Constants.DEFAULT_VALUE && personDay.Person.UsualArrivalPeriodToWork != Constants.DEFAULT_VALUE) {
                    var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, (int)personDay.Person.UsualArrivalPeriodToWork, (int)personDay.Person.UsualDeparturePeriodFromWork, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
                    workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
                    workPclWrkrs = Math.Log(1 + (personDay.Person.UsualWorkParcel.EmploymentTotalBuffer2)) / 10;
                    workIntrDens = Math.Log(1 + personDay.Person.UsualWorkParcel.C34RatioBuffer1());
                } else {
                    var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
                    workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
                }

                noUsualWorkZone = 0;

            }


            if (personDay.Person.UsualSchoolParcelId != 0 && personDay.Person.UsualSchoolParcelId != -1 && personDay.Person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
                schoolPclUniStu = Math.Log(1 + (personDay.Person.UsualSchoolParcel.StudentsUniversityBuffer2)) / 10;
                var schoolNestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.TwoPM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
                schoolTourLogsum = schoolNestedAlternative == null ? 0 : schoolNestedAlternative.ComputeLogsum();
            }


            int countNonMandatory = (from personDayHH in orderedPersonDays where personDayHH.PatternType == 2 select personDayHH.PatternType).Count();

            bool schoolAvailableFlag = true;
            if (!personDay.Person.IsStudent || (!Global.Configuration.IsInEstimationMode && personDay.Person.UsualSchoolParcel == null)) {
                schoolAvailableFlag = false;
            }

            // NONE_OR_HOME

            var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, nCallsForTour > 1, choice == Global.Settings.Purposes.NoneOrHome);

            alternative.Choice = Global.Settings.Purposes.NoneOrHome;
            alternative.AddUtilityTerm(3, (nCallsForTour > 2).ToFlag());


            // USUAL WORK
            alternative = choiceProbabilityCalculator.GetAlternative(1, (personDay.Person.UsualWorkParcelId > 0 && simulatedMandatoryTours[2] == 0 && simulatedMandatoryTours[3] == 0), choice == 1);
            alternative.Choice = 1;
            alternative.AddUtilityTerm(21, 1);
            alternative.AddUtilityTerm(23, workTourLogsum);
            alternative.AddUtilityTerm(25, (personDay.Person.IsPartTimeWorker).ToFlag());
            alternative.AddUtilityTerm(26, (personDay.Person.IsUniversityStudent).ToFlag());
            alternative.AddUtilityTerm(28, (personDay.Person.Household.Has100KPlusIncome).ToFlag());
            alternative.AddUtilityTerm(29, personDay.Person.Household.ResidenceParcel.MixedUse2Index1());
            alternative.AddUtilityTerm(30, (personDay.Person.Age <= 30).ToFlag());
            alternative.AddUtilityTerm(31, workPclWrkrs);
            alternative.AddUtilityTerm(32, ((simulatedMandatoryTours[1] > 0).ToFlag()));
            alternative.AddUtilityTerm(33, workIntrDens);
            alternative.AddUtilityTerm(36, (personDay.WorksAtHomeFlag));
            alternative.AddUtilityTerm(38, (personDay.Person.Household.ResidenceParcel.GetDistanceToTransit() > 1).ToFlag());

            // OTHER WORK
            alternative = choiceProbabilityCalculator.GetAlternative(2, (personDay.Person.IsWorker && simulatedMandatoryTours[3] == 0), choice == 2);
            alternative.Choice = 2;
            alternative.AddUtilityTerm(41, 1);
            alternative.AddUtilityTerm(42, (personDay.Person.IsPartTimeWorker).ToFlag());
            alternative.AddUtilityTerm(43, (personDay.Person.IsUniversityStudent).ToFlag());
            alternative.AddUtilityTerm(45, personDay.Person.Household.ResidenceParcel.MixedUse2Index1());
            alternative.AddUtilityTerm(47, (personDay.Person.Age <= 30).ToFlag());
            alternative.AddUtilityTerm(48, noUsualWorkZone);
            alternative.AddUtilityTerm(49, workIntrDens);
            alternative.AddUtilityTerm(50, workPclWrkrs);
            alternative.AddUtilityTerm(51, countNonMandatory);
            alternative.AddUtilityTerm(52, ((simulatedMandatoryTours[2] > 0).ToFlag()));
            alternative.AddUtilityTerm(53, (household.HouseholdTotals.AllWorkers == 1).ToFlag());
            alternative.AddUtilityTerm(55, noCarsFlag + carCompetitionFlag);
            alternative.AddUtilityTerm(56, (personDay.Person.Household.ResidenceParcel.GetDistanceToTransit() > 1).ToFlag());
            alternative.AddUtilityTerm(57, (personDay.Person.Household.Has0To25KIncome).ToFlag());

            // SCHOOL
            alternative = choiceProbabilityCalculator.GetAlternative(3, schoolAvailableFlag, choice == 3);
            alternative.Choice = 3;
            alternative.AddUtilityTerm(61, 1);
            alternative.AddUtilityTerm(62, schoolTourLogsum);
            alternative.AddUtilityTerm(63, noCarsFlag + carCompetitionFlag);
            alternative.AddUtilityTerm(64, personDay.Person.Household.ResidenceParcel.MixedUse2Index1());
            alternative.AddUtilityTerm(65, (personDay.Person.IsChildUnder5).ToFlag());
            alternative.AddUtilityTerm(66, (personDay.Person.IsUniversityStudent).ToFlag());
            alternative.AddUtilityTerm(67, (personDay.Person.IsDrivingAgeStudent).ToFlag());
            alternative.AddUtilityTerm(68, schoolPclUniStu);
            alternative.AddUtilityTerm(69, schoolPclStudents);
            alternative.AddUtilityTerm(71, ((simulatedMandatoryTours[3] > 0).ToFlag()));
            alternative.AddUtilityTerm(72, schoolIntrDens);
            alternative.AddUtilityTerm(73, (personDay.Person.Age > 25).ToFlag());
            alternative.AddUtilityTerm(74, (personDay.Person.Household.ResidenceParcel.GetDistanceToTransit() > 1).ToFlag());

        }
    }
}