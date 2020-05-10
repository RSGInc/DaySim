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
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
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
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)personDay.Household;
      IActumPersonWrapper person = (IActumPersonWrapper)personDay.Person;

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

      //GV: Number of Parents in the HH
      int numberParent = 0;
      if (person.Age > 25) {
        numberParent++;
      }

      //GV: Number of Adults in the HH
      int numberAdult = 0;
      if (person.Age > 18) {
        numberAdult++;
      }

      int carOwnership =
                            household.VehiclesAvailable == 0
                                 ? Global.Settings.CarOwnerships.NoCars
                                 : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
                                      ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                      : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

      int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
      int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

      bool hhLivesInCPHCity = false;
      if (household.ResidenceParcel.LandUseCode == 101 || household.ResidenceParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }

      //int votALSegment = Global.Settings.VotALSegments.Medium;  // TODO:  calculate a VOT segment that depends on household income
      //GV: 29.3.2019 - getting values from MB's memo
      int votALSegment =
        (household.Income <= 450000)
                  ? Global.Settings.VotALSegments.Low
                  : (household.Income <= 900000)
                      ? Global.Settings.VotALSegments.Medium
                      : Global.Settings.VotALSegments.High;
           
      //int transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
      //GV: 29.3.2019 - getting values from MB's memo
      //OBS - it has to be in km
      int transitAccessSegment =
         household.ResidenceParcel.GetDistanceToTransit() >= 0 && household.ResidenceParcel.GetDistanceToTransit() <= 0.4
            ? 0
            : household.ResidenceParcel.GetDistanceToTransit() > 0.4 && household.ResidenceParcel.GetDistanceToTransit() <= 1.6
                ? 1
                : 2;


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
      alternative.AddUtilityTerm(6, person.IsPartTimeWorker.ToFlag());
      //GV; 12. feb. 2019, Self Employed in the HH 
      alternative.AddUtilityTerm(7, (person.OccupationCode == 8).ToFlag());

      alternative.AddUtilityTerm(8, household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(9, household.HasChildrenAge5Through15.ToFlag());
      //GV: 13. feb 2019, "householdDay.AdultsInSharedHomeStay" acnnot be used
      //alternative.AddUtilityTerm(9, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildren).ToFlag());
      alternative.AddUtilityTerm(10, (household.HasChildren).ToFlag());

      //GV: Inroducing a HH with a sinle parent, female, with children
      //alternative.AddUtilityTerm(10, (person.IsFemale && numberParent == 1 && household.HasChildren).ToFlag());

      //GV: 13. feb 2019, "householdDay.AdultsInSharedHomeStay" acnnot be used
      //alternative.AddUtilityTerm(10, (householdDay.AdultsInSharedHomeStay == 2 && household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(11, (household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());

      //alternative.AddUtilityTerm(14, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(15, (household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());

      //GV: 13, feb 2019 - HH size
      //alternative.AddUtilityTerm(12, (household.Size == 2 && numberAdult == 2).ToFlag()); //GV: HH==2 plus boh are adults
      alternative.AddUtilityTerm(12, (household.Size == 2).ToFlag());
      alternative.AddUtilityTerm(13, (household.Size == 3).ToFlag()); //GV; 16. april 2013, not significant
      alternative.AddUtilityTerm(14, (household.Size >= 4).ToFlag()); //GV; 16. april 2013, not significant

      //alternative.AddUtilityTerm(12, (household.VehiclesAvailable == 1).ToFlag());
      //alternative.AddUtilityTerm(13, (household.VehiclesAvailable >= 2).ToFlag());

      //GV: not sign. 10. juni 2016
      //alternative.AddUtilityTerm(15, (household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(16, (household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      //GV; 12. feb. 2019, CPHcity constant
      alternative.AddUtilityTerm(17, (hhLivesInCPHCity).ToFlag());

      //GV: introduced again - 10. june 2016
      //GV: logsum for mandatory - wrong sign
      //alternative.AddUtilityTerm(17, compositeLogsum);  

      //GV: 29. mar. 2019, income
      alternative.AddUtilityTerm(18, (household.Income >= 450000 && household.Income < 900000).ToFlag());
      alternative.AddUtilityTerm(19, (household.Income >= 900000).ToFlag()); 

      alternative.AddUtilityTerm(20, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(19, (mandatoryCount == 0)? 1 : 0); //GV - goes to infinity


      // PatternType NonMandatory on tour (tours, but none for work or school)
      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 2);
      alternative.Choice = 2;

      alternative.AddUtilityTerm(22, person.IsRetiredAdult.ToFlag());
      alternative.AddUtilityTerm(23, person.IsNonworkingAdult.ToFlag());
      alternative.AddUtilityTerm(24, person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(25, person.IsChildAge5Through15.ToFlag());
      alternative.AddUtilityTerm(26, person.IsFemale.ToFlag());
      //GV; 12. feb. 2019, Self Employed in the HH 
      alternative.AddUtilityTerm(27, (person.OccupationCode == 8).ToFlag());

      //GV: Inroducing a HH with a sinle parent, female, with children
      //alternative.AddUtilityTerm(10, (person.IsFemale && numberParent ==1 && household.HasChildren).ToFlag());

      //GV: not sign. 10. june 2016
      //alternative.AddUtilityTerm(24, household.HasChildrenUnder5.ToFlag());
      //alternative.AddUtilityTerm(25, household.HasChildrenAge5Through15.ToFlag());

      //alternative.AddUtilityTerm(31, (householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(33, (household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());

      //GV: 13. feb 2019, "householdDay.AdultsInSharedHomeStay" acnnot be used
      //alternative.AddUtilityTerm(26, (householdDay.AdultsInSharedHomeStay == 2 && household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(27, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildren).ToFlag());
      alternative.AddUtilityTerm(28, (household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      alternative.AddUtilityTerm(29, (household.HasChildren).ToFlag());

      //GV: 13, feb 2019 - HH size
      //alternative.AddUtilityTerm(30, (household.Size == 2 && numberAdult == 2).ToFlag()); //GV: HH==2 plus boh are adults
      alternative.AddUtilityTerm(30, (household.Size == 2).ToFlag());
      alternative.AddUtilityTerm(31, (household.Size == 3).ToFlag()); //GV; 16. april 2013, not significant
      alternative.AddUtilityTerm(32, (household.Size >= 4).ToFlag()); //GV; 16. april 2013, not significant

      //alternative.AddUtilityTerm(27, (household.VehiclesAvailable == 1).ToFlag());
      //alternative.AddUtilityTerm(28, (household.VehiclesAvailable >= 2).ToFlag());
      //alternative.AddUtilityTerm(31, (household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      alternative.AddUtilityTerm(33, (household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());

      //GV; 12. feb. 2019, CPHcity constant
      alternative.AddUtilityTerm(34, (hhLivesInCPHCity).ToFlag());

      //alternative.AddUtilityTerm(33, compositeLogsum); //GV: logsum for non-mandatory 

      //GV: 29. mar. 2019 - income
      alternative.AddUtilityTerm(35, (household.Income >= 450000 && household.Income < 900000).ToFlag()); //GV; 16. april 2013, not significant
      alternative.AddUtilityTerm(36, (household.Income >= 900000).ToFlag()); //GV; 16. april 2013, not significant

      alternative.AddUtilityTerm(37, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(24, person.IsChildUnder5.ToFlag());
      //alternative.AddUtilityTerm(25, person.IsNonworkingAdult.ToFlag()); 

      // PatternType Home (all day)
      alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 3);
      alternative.Choice = 3;

      alternative.AddUtilityTerm(41, 1);
      alternative.AddUtilityTerm(42, person.WorksAtHome().ToFlag());
      //GV: Inroducing a HH with a sinle parent, female, with children
      alternative.AddUtilityTerm(43, (person.IsFemale && numberParent == 1 && household.HasChildren).ToFlag());

      //GV: introduced again - 10. june 2016; not sign.
      //alternative.AddUtilityTerm(43, person.IsUniversityStudent.ToFlag());

      //alternative.AddUtilityTerm(54, (homeCount > 0)? 1 : 0); //GV: can be estimated but the valus is huge 

    }
  }
}
