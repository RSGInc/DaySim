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
  public class MandatoryStopPresenceModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumMandatoryStopPresenceModel";
    private const int TOTAL_ALTERNATIVES = 4;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 80;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.MandatoryStopPresenceModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
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

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        choice = Math.Min(personDay.BusinessStops, 1) + 2 * Math.Min(personDay.SchoolStops, 1);

        RunModel(choiceProbabilityCalculator, personDay, householdDay, choice);

        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        choice = Math.Min(personDay.BusinessStops, 1) + 2 * Math.Min(personDay.SchoolStops, 1);

        RunModel(choiceProbabilityCalculator, personDay, householdDay);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, choice);

        Global.Configuration.IsInEstimationMode = true;
      } else {
        RunModel(choiceProbabilityCalculator, personDay, householdDay);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
        choice = (int)chosenAlternative.Choice;

        if (choice == 1 || choice == 3) {
          personDay.BusinessStops = 1;
        }
        if (choice == 2 || choice == 3) {
          personDay.SchoolStops = 1;
        }
      }

    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {
      Framework.DomainModels.Wrappers.IHouseholdWrapper household = personDay.Household;
      Framework.DomainModels.Wrappers.IPersonWrapper person = personDay.Person;

      double workTourLogsum;
      if (personDay.Person.UsualWorkParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay.Person, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers, Global.Settings.Purposes.Work);
        workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
      } else {
        workTourLogsum = 0;
      }

      double schoolTourLogsum;
      if (personDay.Person.UsualSchoolParcelId != Constants.DEFAULT_VALUE && personDay.Person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(personDay.Person, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, personDay.Person.Household.ResidenceParcel, personDay.Person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers, Global.Settings.Purposes.School);
        schoolTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
      } else {
        schoolTourLogsum = 0;
      }


      // No mandatory stops
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      //alternative.AddUtilityTerm(2, householdDay.Household.HasChildren.ToFlag());
      alternative.AddUtilityTerm(3, householdDay.Household.HasChildrenUnder5.ToFlag());
      //alternative.AddUtilityTerm(4, householdDay.Household.HasChildrenAge5Through15.ToFlag());
      //alternative.AddUtilityTerm(6, householdDay.Household.HasChildrenUnder16.ToFlag());

      //alternative.AddNestedAlternative(11, 0, 60); 

      //GV: added 14. june 2016
      alternative.AddUtilityTerm(4, householdDay.PrimaryPriorityTimeFlag);
      //alternative.AddUtilityTerm(5, (householdDay.Household.VehiclesAvailable >= 1).ToFlag());

      // Business stop(s)
      alternative = choiceProbabilityCalculator.GetAlternative(1, personDay.Person.IsWorker, choice == 1);
      alternative.Choice = 1;
      alternative.AddUtilityTerm(21, 1);

      //alternative.AddUtilityTerm(22, personDay.Person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(23, personDay.Person.WorksAtHome().ToFlag());
      alternative.AddUtilityTerm(24, personDay.Person.IsFulltimeWorker.ToFlag());
      //alternative.AddUtilityTerm(25, personDay.Person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(25, personDay.Person.IsMale.ToFlag());
      //alternative.AddUtilityTerm(4, person.IsPartTimeWorker.ToFlag());

      //GV: 15. june 2016, not sign.
      //alternative.AddUtilityTerm(26, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());

      //alternative.AddUtilityTerm(14, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(15, (householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(27, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildren).ToFlag());

      alternative.AddUtilityTerm(28, workTourLogsum);

      //alternative.AddUtilityTerm(28, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(29, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());


      // School stop(s)
      alternative = choiceProbabilityCalculator.GetAlternative(2, personDay.Person.IsStudent, choice == 2);
      alternative.Choice = 2;
      alternative.AddUtilityTerm(41, 1);

      //alternative.AddUtilityTerm(43, personDay.Person.WorksAtHome().ToFlag());
      //alternative.AddUtilityTerm(44, personDay.Person.IsFulltimeWorker.ToFlag());
      //alternative.AddUtilityTerm(45, personDay.Person.IsPartTimeWorker.ToFlag());
      //alternative.AddUtilityTerm(46, personDay.Person.IsMale.ToFlag());
      alternative.AddUtilityTerm(47, personDay.Person.IsYouth.ToFlag());

      //GV: 15. june 2016, not sign.
      //alternative.AddUtilityTerm(48, schoolTourLogsum); 


      // Business and school stops
      alternative = choiceProbabilityCalculator.GetAlternative(3, (personDay.Person.IsWorker && personDay.Person.IsStudent), choice == 3);
      alternative.Choice = 3;
      alternative.AddUtilityTerm(61, 1);
      alternative.AddUtilityTerm(28, workTourLogsum);

      //GV: 15. june 2016, not sign.
      //alternative.AddUtilityTerm(48, schoolTourLogsum);

    }
  }
}
