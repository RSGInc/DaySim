// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class WorkAtHomeModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "ActumWorkAtHomeModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkAtHomeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      personDay.Person.ResetRandom(904);

      if (Global.Configuration.IsInEstimationMode) {

        if (personDay.WorkAtHomeDuration >= 120 && personDay.Person.IsFullOrPartTimeWorker) { personDay.WorksAtHomeFlag = 1; } else {
          personDay.WorksAtHomeFlag = 0;
        }

        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode || !personDay.Person.IsFullOrPartTimeWorker) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }

        RunModel(choiceProbabilityCalculator, personDay, householdDay, personDay.WorksAtHomeFlag);

        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, personDay, householdDay);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, personDay.WorksAtHomeFlag);

        Global.Configuration.IsInEstimationMode = true;
      } else {

        int choice;

        if (!personDay.Person.IsFullOrPartTimeWorker) {
          choice = 0;
        } else {

          RunModel(choiceProbabilityCalculator, personDay, householdDay);

          ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
          choice = (int)chosenAlternative.Choice;
        }
        personDay.WorksAtHomeFlag = choice;
        personDay.WorkAtHomeDuration = choice * 120; //default predicted duration for output
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {

      Framework.DomainModels.Wrappers.IHouseholdWrapper household = householdDay.Household;

      // set household characteristics here that don't depend on person characteristics
      bool available = (householdDay.Household.Size > 1);

      int hasAdultEducLevel12 = 0;
      //int allAdultEducLevel12 = 1;
      int youngestAge = 999;

      foreach (PersonWrapper person in householdDay.Household.Persons) {
        // set characteristics here that depend on person characteristics
        if (person.Age >= 18 && person.EducationLevel >= 12) {
          hasAdultEducLevel12 = 1;
        }
        //if (person.Age >= 18 && person.EducationLevel < 12) allAdultEducLevel12 = 0;
        if (person.Age < youngestAge) {
          youngestAge = person.Age;
        }
      }


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

      int atHomeDay = personDay.PatternType == 3 ? 1 : 0;
      int nonMandatoryTourDay = personDay.PatternType == 2 ? 1 : 0;
      int MandatoryTourDay = personDay.PatternType == 1 ? 1 : 0;



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

      //var householdDay = (ActumHouseholdDayWrapper)tour.HouseholdDay;
      //var household = householdDay.Household;



      // 0 Person doesn't work at home more than specified number of minutes
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      alternative.AddUtilityTerm(1, 0.0);

      // 1 Works at home
      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      alternative.AddUtilityTerm(1, 1.0);

      alternative.AddUtilityTerm(2, householdDay.Household.HasChildrenUnder5.ToFlag());

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(3, householdDay.Household.HasChildrenAge5Through15.ToFlag());

      alternative.AddUtilityTerm(4, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      alternative.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(6, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(7, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());

      alternative.AddUtilityTerm(8, (youngestAge >= 40).ToFlag());

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(10, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
      //alternative.AddUtilityTerm(11, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
      //alternative.AddUtilityTerm(12, (householdDay.Household.Income >= 900000).ToFlag());

      alternative.AddUtilityTerm(15, householdDay.PrimaryPriorityTimeFlag);

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(16, (householdDay.Household.Size == 2).ToFlag());
      //alternative.AddUtilityTerm(17, (householdDay.Household.Size == 3).ToFlag());
      //alternative.AddUtilityTerm(18, (householdDay.Household.Size >= 4).ToFlag());
      ////alternative.AddUtilityTerm(18, (householdDay.Household.Size >= 5).ToFlag()); 

      alternative.AddUtilityTerm(21, personDay.Person.IsPartTimeWorker.ToFlag());
      //alternative.AddUtilityTerm(2, personDay.Person.IsFulltimeWorker.ToFlag());

      alternative.AddUtilityTerm(22, (personDay.Person.Gender == 1).ToFlag());
      alternative.AddUtilityTerm(23, (hasAdultEducLevel12 == 1).ToFlag());

      //alternative.AddUtilityTerm(24, MandatoryTourDay);
      alternative.AddUtilityTerm(25, nonMandatoryTourDay);

      alternative.AddUtilityTerm(26, atHomeDay);

      //alternative.AddUtilityTerm(27, workTourLogsum);

      //GV: put in instead of compositeLogsum - 13. june 2016
      alternative.AddUtilityTerm(27, workTourLogsum * MandatoryTourDay);
      alternative.AddUtilityTerm(28, workTourLogsum * nonMandatoryTourDay);

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(27, compositeLogsum);

      //alternative.AddUtilityTerm(27, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag()); //GV not significant
      //alternative.AddUtilityTerm(28, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag()); //GV not significant


    }
  }
}