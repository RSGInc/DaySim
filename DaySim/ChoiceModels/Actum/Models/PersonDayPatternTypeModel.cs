// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class PersonDayPatternTypeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumPersonDayPatternTypeModel";
    private const int TOTAL_ALTERNATIVES = 3;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 60;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.PersonDayPatternTypeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      personDay.Person.ResetRandom(903);

      int choice = 0;

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        choice = personDay.PatternType;

        RunModel(choiceProbabilityCalculator, personDay, householdDay, choice);

        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        //choice = personDay.PatternType;

        RunModel(choiceProbabilityCalculator, personDay, householdDay);

        //var observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

        //var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, altPTypes.);

        //var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, choice);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, personDay.PatternType - 1);

        Global.Configuration.IsInEstimationMode = true;
      } else {
        RunModel(choiceProbabilityCalculator, personDay, householdDay);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
        choice = (int)chosenAlternative.Choice;

        personDay.PatternType = choice;

      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {
      Framework.DomainModels.Wrappers.IHouseholdWrapper household = personDay.Household;
      Framework.DomainModels.Wrappers.IPersonWrapper person = personDay.Person;

      IEnumerable<PersonDayWrapper> personTypeOrderedPersonDays = householdDay.PersonDays.OrderBy(p => p.Person.PersonType).ToList().Cast<PersonDayWrapper>();
      int mandatoryCount = 0;
      int nonMandatoryCount = 0;
      int homeCount = 0;
      int i = 0;
      foreach (PersonDayWrapper pDay in personTypeOrderedPersonDays) {
        i++;
        if (i <= 5) {
          if (pDay.PatternType == Global.Settings.PatternTypes.Mandatory) { mandatoryCount++; } else if (pDay.PatternType == Global.Settings.PatternTypes.Optional) { nonMandatoryCount++; } else { homeCount++; }
        }
      }

      bool mandatoryAvailableFlag = true;
      if (personDay.Person.IsNonworkingAdult || personDay.Person.IsRetiredAdult ||
          (!personDay.Person.IsWorker && !personDay.Person.IsStudent) ||
          (!Global.Configuration.IsInEstimationMode && !personDay.Person.IsWorker && personDay.Person.UsualSchoolParcel == null)
          ) {
        mandatoryAvailableFlag = false;
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

      // Pattern Type Mandatory on tour (at least one work or school tour)
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, mandatoryAvailableFlag, choice == 1);
      alternative.Choice = 1;

      alternative.AddUtilityTerm(1, 1);

      alternative.AddUtilityTerm(2, person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(3, person.IsChildAge5Through15.ToFlag());
      alternative.AddUtilityTerm(4, person.IsFulltimeWorker.ToFlag());
      alternative.AddUtilityTerm(5, person.IsMale.ToFlag());
      //alternative.AddUtilityTerm(4, person.IsPartTimeWorker.ToFlag());

      alternative.AddUtilityTerm(7, householdDay.Household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(8, householdDay.Household.HasChildrenAge5Through15.ToFlag());

      alternative.AddUtilityTerm(10, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(14, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(15, (householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(11, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildren).ToFlag());

      //alternative.AddUtilityTerm(12, (householdDay.Household.Size == 2).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(13, (householdDay.Household.Size == 3).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(14, (householdDay.Household.Size >= 4).ToFlag()); //GV; 16. april 2013, not significant

      //alternative.AddUtilityTerm(12, (householdDay.Household.VehiclesAvailable == 1).ToFlag());
      //alternative.AddUtilityTerm(13, (householdDay.Household.VehiclesAvailable >= 2).ToFlag());

      //GV: not sign. 10. juni 2016
      //alternative.AddUtilityTerm(15, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(16, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      //GV: introduced again - 10. june 2016
      //GV: logsum for mandatory - wrong sign
      //alternative.AddUtilityTerm(17, compositeLogsum);  

      //alternative.AddUtilityTerm(17, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(18, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(19, (householdDay.Household.Income >= 900000).ToFlag()); //GV; 16. april 2013, not significant

      alternative.AddUtilityTerm(20, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(19, (mandatoryCount == 0)? 1 : 0); //GV - goes to infinity



      // PatternType NonMandatory on tour (tours, but none for work or school)
      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 2);
      alternative.Choice = 2;

      alternative.AddUtilityTerm(22, person.IsRetiredAdult.ToFlag());
      alternative.AddUtilityTerm(23, person.IsNonworkingAdult.ToFlag());

      //GV: not sign. 10. june 2016
      //alternative.AddUtilityTerm(24, householdDay.Household.HasChildrenUnder5.ToFlag());
      //alternative.AddUtilityTerm(25, householdDay.Household.HasChildrenAge5Through15.ToFlag());

      //alternative.AddUtilityTerm(31, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(33, (householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(26, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(27, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildren).ToFlag());

      //alternative.AddUtilityTerm(28, (householdDay.Household.Size == 2).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(29, (householdDay.Household.Size == 3).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(30, (householdDay.Household.Size >= 4).ToFlag()); //GV; 16. april 2013, not significant

      //alternative.AddUtilityTerm(27, (householdDay.Household.VehiclesAvailable == 1).ToFlag());
      //alternative.AddUtilityTerm(28, (householdDay.Household.VehiclesAvailable >= 2).ToFlag());
      alternative.AddUtilityTerm(31, (householdDay.Household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(32, (householdDay.Household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      //alternative.AddUtilityTerm(33, compositeLogsum); //GV: logsum for non-mandatory 

      //alternative.AddUtilityTerm(33, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(34, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(35, (householdDay.Household.Income >= 900000).ToFlag()); //GV; 16. april 2013, not significant

      alternative.AddUtilityTerm(36, householdDay.PrimaryPriorityTimeFlag);


      //alternative.AddUtilityTerm(24, person.IsChildUnder5.ToFlag());
      //alternative.AddUtilityTerm(25, person.IsNonworkingAdult.ToFlag());

      // PatternType Home (all day)
      alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 3);
      alternative.Choice = 3;

      alternative.AddUtilityTerm(41, 1);
      alternative.AddUtilityTerm(42, person.WorksAtHome().ToFlag());

      //GV: introduced again - 10. june 2016; not sign.
      //alternative.AddUtilityTerm(43, person.IsUniversityStudent.ToFlag());

      //alternative.AddUtilityTerm(54, (homeCount > 0)? 1 : 0); //GV: can be estimated but the valus is huge 

    }
  }
}
