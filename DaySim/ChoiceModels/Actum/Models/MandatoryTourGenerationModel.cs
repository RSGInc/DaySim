﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class MandatoryTourGenerationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumMandatoryTourGenerationModel";
    private const int TOTAL_ALTERNATIVES = 4;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 60;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.MandatoryTourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public int Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int nCallsForTour, int[] simulatedMandatoryTours) {
      return Run(personDay, householdDay, nCallsForTour, simulatedMandatoryTours, Global.Settings.Purposes.NoneOrHome);
    }

    public int Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int nCallsForTour, int[] simulatedMandatoryTours, int choice) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      personDay.Person.ResetRandom(904 + nCallsForTour);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return choice;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((personDay.Person.Id * 10 + personDay.Day) * 397) ^ nCallsForTour);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours, choice);
        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, choice);

        Global.Configuration.IsInEstimationMode = true;
      } else {
        RunModel(choiceProbabilityCalculator, personDay, householdDay, nCallsForTour, simulatedMandatoryTours);
        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
        choice = (int)chosenAlternative.Choice;
        if (choice == 1) {
          personDay.UsualWorkplaceTours++;
          personDay.WorkTours++;
        } else if (choice == 2) {
          personDay.BusinessTours++;
        } else if (choice == 3) {
          personDay.SchoolTours++;
        }
      }

      return choice;
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int nCallsForTour, int[] simulatedMandatoryTours, int choice = Constants.DEFAULT_VALUE) {
      Framework.DomainModels.Wrappers.IHouseholdWrapper household = personDay.Household;

      double workTourLogsum;
      if (personDay.Person.UsualWorkParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay.Person, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers, Global.Settings.Purposes.Work);
        workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
      } else {
        workTourLogsum = 0;
      }

      double schoolTourLogsum;
      if (personDay.Person.UsualSchoolParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
        //JLB
        //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(personDay.Person, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers, Global.Settings.Purposes.School);
        schoolTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
      } else {
        schoolTourLogsum = 0;
      }

      int carOwnership =
                        household.VehiclesAvailable == 0
                            ? Global.Settings.CarOwnerships.NoCars
                            : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
                                ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

      int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
      int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

      int votALSegment = Global.Settings.VotALSegments.Medium;  // TODO:  calculate a VOT segment that depends on household income
      int transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
      double personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
      double shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
      double mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
      double socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
      //var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
      double compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];

      //int hasAdultEducLevel12 = 0;
      //int allAdultEducLevel12 = 1;
      int youngestAge = 999;

      foreach (PersonWrapper person in personDay.Household.Persons) {
        // set characteristics here that depend on person characteristics
        //if (person.Age >= 18 && person.EducationLevel >= 12) hasAdultEducLevel12 = 1;
        //if (person.Age >= 18 && person.EducationLevel < 12) allAdultEducLevel12 = 0;
        if (person.Age < youngestAge) {
          youngestAge = person.Age;
        }
      }

      bool schoolAvailableFlag = true;
      if ((!personDay.Person.IsStudent) || (!Global.Configuration.IsInEstimationMode && personDay.Person.UsualSchoolParcel == null)) {
        schoolAvailableFlag = false;
      }

      // NONE_OR_HOME

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, nCallsForTour > 1, choice == Global.Settings.Purposes.NoneOrHome);

      alternative.Choice = Global.Settings.Purposes.NoneOrHome;
      //alternative.AddUtilityTerm(1, (nCallsForTour > 2).ToFlag()); // GV; 16.april 2013 - cannot be estimated

      //alternative.AddUtilityTerm(2, householdDay.Household.HasChildren.ToFlag());
      alternative.AddUtilityTerm(3, householdDay.Household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(4, householdDay.Household.HasChildrenAge5Through15.ToFlag());
      //alternative.AddUtilityTerm(6, householdDay.Household.HasChildrenUnder16.ToFlag());

      //alternative.AddUtilityTerm(10, (householdDay.Household.Size == 2).ToFlag()); // GV; 16. april 2013 - cannot be estimated
      alternative.AddUtilityTerm(11, (householdDay.Household.Size == 3).ToFlag());
      alternative.AddUtilityTerm(12, (householdDay.Household.Size >= 4).ToFlag());

      //alternative.AddUtilityTerm(14, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
      //alternative.AddUtilityTerm(15, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
      //alternative.AddUtilityTerm(16, (householdDay.Household.Income >= 900000).ToFlag());

      //alternative.AddNestedAlternative(11, 0, 60); 


      // USUAL WORK
      alternative = choiceProbabilityCalculator.GetAlternative(1, (personDay.Person.UsualWorkParcelId > 0 && simulatedMandatoryTours[2] == 0), choice == 1);
      alternative.Choice = 1;
      alternative.AddUtilityTerm(21, 1);

      //alternative.AddUtilityTerm(22, personDay.Person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(23, personDay.Person.WorksAtHome().ToFlag());
      alternative.AddUtilityTerm(24, personDay.Person.IsFulltimeWorker.ToFlag());
      alternative.AddUtilityTerm(25, personDay.Person.IsMale.ToFlag());
      //alternative.AddUtilityTerm(4, person.IsPartTimeWorker.ToFlag());

      alternative.AddUtilityTerm(26, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(14, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(15, (householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(27, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildren).ToFlag());

      alternative.AddUtilityTerm(28, workTourLogsum);

      //alternative.AddUtilityTerm(28, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(29, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      alternative.AddUtilityTerm(30, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddNestedAlternative(12, 1, 60);

      // BUSINESS
      alternative = choiceProbabilityCalculator.GetAlternative(2, (personDay.Person.IsWorker && simulatedMandatoryTours[3] == 0), choice == 2);
      alternative.Choice = 2;
      alternative.AddUtilityTerm(31, 1);

      //alternative.AddUtilityTerm(32, personDay.Person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(33, personDay.Person.WorksAtHome().ToFlag());

      alternative.AddUtilityTerm(34, personDay.Person.IsFulltimeWorker.ToFlag());

      alternative.AddUtilityTerm(35, personDay.Person.IsMale.ToFlag());
      //alternative.AddUtilityTerm(4, person.IsPartTimeWorker.ToFlag());

      alternative.AddUtilityTerm(36, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(14, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(15, (householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());

      //GV: 13. june 2016, not sign.
      //alternative.AddUtilityTerm(37, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildren).ToFlag());

      alternative.AddUtilityTerm(38, workTourLogsum);

      //alternative.AddUtilityTerm(38, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(39, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      alternative.AddUtilityTerm(40, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddNestedAlternative(12, 1, 60);

      // SCHOOL
      alternative = choiceProbabilityCalculator.GetAlternative(3, schoolAvailableFlag, choice == 3);
      alternative.Choice = 3;
      alternative.AddUtilityTerm(41, 1);

      //GV: 13. june 2016, not sign.
      //alternative.AddUtilityTerm(42, personDay.Person.IsNonworkingAdult.ToFlag()); 

      //alternative.AddUtilityTerm(43, personDay.Person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(43, personDay.Person.IsYouth.ToFlag());

      //alternative.AddUtilityTerm(46, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(14, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(15, (householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(47, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildren).ToFlag());

      alternative.AddUtilityTerm(48, workTourLogsum);
      //GV: 13. june 2016, not sign.
      //alternative.AddUtilityTerm(49, schoolTourLogsum);

      //alternative.AddUtilityTerm(48, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      //alternative.AddUtilityTerm(49, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      alternative.AddUtilityTerm(50, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddNestedAlternative(12, 1, 60);

    }
  }
}
