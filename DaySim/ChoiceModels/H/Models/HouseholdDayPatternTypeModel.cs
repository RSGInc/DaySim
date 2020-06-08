// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.H.Models {
  public class HouseholdDayPatternTypeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "HHouseholdDayPatternTypeModel";
    private const int TOTAL_ALTERNATIVES = 363;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 700;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.HouseholdDayPatternTypeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(HouseholdDayWrapper householdDay, int choice = 0) {
      if (householdDay == null) {
        throw new ArgumentNullException("householdDay");
      }

      int numberPersonsModeledJointly = 5;  // set this at compile time depending on whether we want to support 4 or 5 household members in this joint model

      householdDay.ResetRandom(902);

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.Person.GetHouseholdDayPatternParticipationPriority()).ToList().Cast<PersonDayWrapper>();

      int[] ptypes = new int[6];
      int hhsize = householdDay.Household.Size;
      if (Global.Configuration.IsInEstimationMode) {
        int count = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          count++;

          if (personDay.WorkTours > 0 || personDay.SchoolTours > 0) {
            personDay.PatternType = Global.Settings.PatternTypes.Mandatory;
          } else if (personDay.GetTotalTours() > 0) {
            personDay.PatternType = Global.Settings.PatternTypes.Optional;
          } else {
            personDay.PatternType = Global.Settings.PatternTypes.Home;
          }
          if (count <= numberPersonsModeledJointly) {
            ptypes[count] = personDay.PatternType;
          }
        }

        if (numberPersonsModeledJointly == 4) {
          if (hhsize == 1) { choice = ptypes[1] - 1; } else if (hhsize == 2) { choice = ptypes[1] * 3 + ptypes[2] - 1; } else if (hhsize == 3) { choice = ptypes[1] * 9 + ptypes[2] * 3 + ptypes[3] - 1; } else { choice = ptypes[1] * 27 + ptypes[2] * 9 + ptypes[3] * 3 + ptypes[4] - 1; }
        } else {  // ie numberPersonsModeledJointly == 5
          if (hhsize == 1) { choice = ptypes[1] - 1; } else if (hhsize == 2) { choice = ptypes[1] * 3 + ptypes[2] - 1; } else if (hhsize == 3) { choice = ptypes[1] * 9 + ptypes[2] * 3 + ptypes[3] - 1; } else if (hhsize == 4) { choice = ptypes[1] * 27 + ptypes[2] * 9 + ptypes[3] * 3 + ptypes[4] - 1; } else { choice = ptypes[1] * 81 + ptypes[2] * 27 + ptypes[3] * 9 + ptypes[4] * 3 + ptypes[5] - 1; }
        }

        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(householdDay.Household.Id * 10 + householdDay.Day);

      // array associating alternative with the purposes of each of the five possible positions in the alternative
      //  altPTypes[a,p] is the purpose of position p in alternative a  
      int[,] altPTypes = new int[,] {
                //{0,0,0,0,0,0},
                {0,1,0,0,0,0},
                {0,2,0,0,0,0},
                {0,3,0,0,0,0},
                {0,1,1,0,0,0},
                {0,1,2,0,0,0},
                {0,1,3,0,0,0},
                {0,2,1,0,0,0},
                {0,2,2,0,0,0},
                {0,2,3,0,0,0},
                {0,3,1,0,0,0},
                {0,3,2,0,0,0},
                {0,3,3,0,0,0},
                {0,1,1,1,0,0},
                {0,1,1,2,0,0},
                {0,1,1,3,0,0},
                {0,1,2,1,0,0},
                {0,1,2,2,0,0},
                {0,1,2,3,0,0},
                {0,1,3,1,0,0},
                {0,1,3,2,0,0},
                {0,1,3,3,0,0},
                {0,2,1,1,0,0},
                {0,2,1,2,0,0},
                {0,2,1,3,0,0},
                {0,2,2,1,0,0},
                {0,2,2,2,0,0},
                {0,2,2,3,0,0},
                {0,2,3,1,0,0},
                {0,2,3,2,0,0},
                {0,2,3,3,0,0},
                {0,3,1,1,0,0},
                {0,3,1,2,0,0},
                {0,3,1,3,0,0},
                {0,3,2,1,0,0},
                {0,3,2,2,0,0},
                {0,3,2,3,0,0},
                {0,3,3,1,0,0},
                {0,3,3,2,0,0},
                {0,3,3,3,0,0},
                {0,1,1,1,1,0},
                {0,1,1,1,2,0},
                {0,1,1,1,3,0},
                {0,1,1,2,1,0},
                {0,1,1,2,2,0},
                {0,1,1,2,3,0},
                {0,1,1,3,1,0},
                {0,1,1,3,2,0},
                {0,1,1,3,3,0},
                {0,1,2,1,1,0},
                {0,1,2,1,2,0},
                {0,1,2,1,3,0},
                {0,1,2,2,1,0},
                {0,1,2,2,2,0},
                {0,1,2,2,3,0},
                {0,1,2,3,1,0},
                {0,1,2,3,2,0},
                {0,1,2,3,3,0},
                {0,1,3,1,1,0},
                {0,1,3,1,2,0},
                {0,1,3,1,3,0},
                {0,1,3,2,1,0},
                {0,1,3,2,2,0},
                {0,1,3,2,3,0},
                {0,1,3,3,1,0},
                {0,1,3,3,2,0},
                {0,1,3,3,3,0},
                {0,2,1,1,1,0},
                {0,2,1,1,2,0},
                {0,2,1,1,3,0},
                {0,2,1,2,1,0},
                {0,2,1,2,2,0},
                {0,2,1,2,3,0},
                {0,2,1,3,1,0},
                {0,2,1,3,2,0},
                {0,2,1,3,3,0},
                {0,2,2,1,1,0},
                {0,2,2,1,2,0},
                {0,2,2,1,3,0},
                {0,2,2,2,1,0},
                {0,2,2,2,2,0},
                {0,2,2,2,3,0},
                {0,2,2,3,1,0},
                {0,2,2,3,2,0},
                {0,2,2,3,3,0},
                {0,2,3,1,1,0},
                {0,2,3,1,2,0},
                {0,2,3,1,3,0},
                {0,2,3,2,1,0},
                {0,2,3,2,2,0},
                {0,2,3,2,3,0},
                {0,2,3,3,1,0},
                {0,2,3,3,2,0},
                {0,2,3,3,3,0},
                {0,3,1,1,1,0},
                {0,3,1,1,2,0},
                {0,3,1,1,3,0},
                {0,3,1,2,1,0},
                {0,3,1,2,2,0},
                {0,3,1,2,3,0},
                {0,3,1,3,1,0},
                {0,3,1,3,2,0},
                {0,3,1,3,3,0},
                {0,3,2,1,1,0},
                {0,3,2,1,2,0},
                {0,3,2,1,3,0},
                {0,3,2,2,1,0},
                {0,3,2,2,2,0},
                {0,3,2,2,3,0},
                {0,3,2,3,1,0},
                {0,3,2,3,2,0},
                {0,3,2,3,3,0},
                {0,3,3,1,1,0},
                {0,3,3,1,2,0},
                {0,3,3,1,3,0},
                {0,3,3,2,1,0},
                {0,3,3,2,2,0},
                {0,3,3,2,3,0},
                {0,3,3,3,1,0},
                {0,3,3,3,2,0},
                {0,3,3,3,3,0},
                {0,1,1,1,1,1},
                {0,1,1,1,1,2},
                {0,1,1,1,1,3},
                {0,1,1,1,2,1},
                {0,1,1,1,2,2},
                {0,1,1,1,2,3},
                {0,1,1,1,3,1},
                {0,1,1,1,3,2},
                {0,1,1,1,3,3},
                {0,1,1,2,1,1},
                {0,1,1,2,1,2},
                {0,1,1,2,1,3},
                {0,1,1,2,2,1},
                {0,1,1,2,2,2},
                {0,1,1,2,2,3},
                {0,1,1,2,3,1},
                {0,1,1,2,3,2},
                {0,1,1,2,3,3},
                {0,1,1,3,1,1},
                {0,1,1,3,1,2},
                {0,1,1,3,1,3},
                {0,1,1,3,2,1},
                {0,1,1,3,2,2},
                {0,1,1,3,2,3},
                {0,1,1,3,3,1},
                {0,1,1,3,3,2},
                {0,1,1,3,3,3},
                {0,1,2,1,1,1},
                {0,1,2,1,1,2},
                {0,1,2,1,1,3},
                {0,1,2,1,2,1},
                {0,1,2,1,2,2},
                {0,1,2,1,2,3},
                {0,1,2,1,3,1},
                {0,1,2,1,3,2},
                {0,1,2,1,3,3},
                {0,1,2,2,1,1},
                {0,1,2,2,1,2},
                {0,1,2,2,1,3},
                {0,1,2,2,2,1},
                {0,1,2,2,2,2},
                {0,1,2,2,2,3},
                {0,1,2,2,3,1},
                {0,1,2,2,3,2},
                {0,1,2,2,3,3},
                {0,1,2,3,1,1},
                {0,1,2,3,1,2},
                {0,1,2,3,1,3},
                {0,1,2,3,2,1},
                {0,1,2,3,2,2},
                {0,1,2,3,2,3},
                {0,1,2,3,3,1},
                {0,1,2,3,3,2},
                {0,1,2,3,3,3},
                {0,1,3,1,1,1},
                {0,1,3,1,1,2},
                {0,1,3,1,1,3},
                {0,1,3,1,2,1},
                {0,1,3,1,2,2},
                {0,1,3,1,2,3},
                {0,1,3,1,3,1},
                {0,1,3,1,3,2},
                {0,1,3,1,3,3},
                {0,1,3,2,1,1},
                {0,1,3,2,1,2},
                {0,1,3,2,1,3},
                {0,1,3,2,2,1},
                {0,1,3,2,2,2},
                {0,1,3,2,2,3},
                {0,1,3,2,3,1},
                {0,1,3,2,3,2},
                {0,1,3,2,3,3},
                {0,1,3,3,1,1},
                {0,1,3,3,1,2},
                {0,1,3,3,1,3},
                {0,1,3,3,2,1},
                {0,1,3,3,2,2},
                {0,1,3,3,2,3},
                {0,1,3,3,3,1},
                {0,1,3,3,3,2},
                {0,1,3,3,3,3},
                {0,2,1,1,1,1},
                {0,2,1,1,1,2},
                {0,2,1,1,1,3},
                {0,2,1,1,2,1},
                {0,2,1,1,2,2},
                {0,2,1,1,2,3},
                {0,2,1,1,3,1},
                {0,2,1,1,3,2},
                {0,2,1,1,3,3},
                {0,2,1,2,1,1},
                {0,2,1,2,1,2},
                {0,2,1,2,1,3},
                {0,2,1,2,2,1},
                {0,2,1,2,2,2},
                {0,2,1,2,2,3},
                {0,2,1,2,3,1},
                {0,2,1,2,3,2},
                {0,2,1,2,3,3},
                {0,2,1,3,1,1},
                {0,2,1,3,1,2},
                {0,2,1,3,1,3},
                {0,2,1,3,2,1},
                {0,2,1,3,2,2},
                {0,2,1,3,2,3},
                {0,2,1,3,3,1},
                {0,2,1,3,3,2},
                {0,2,1,3,3,3},
                {0,2,2,1,1,1},
                {0,2,2,1,1,2},
                {0,2,2,1,1,3},
                {0,2,2,1,2,1},
                {0,2,2,1,2,2},
                {0,2,2,1,2,3},
                {0,2,2,1,3,1},
                {0,2,2,1,3,2},
                {0,2,2,1,3,3},
                {0,2,2,2,1,1},
                {0,2,2,2,1,2},
                {0,2,2,2,1,3},
                {0,2,2,2,2,1},
                {0,2,2,2,2,2},
                {0,2,2,2,2,3},
                {0,2,2,2,3,1},
                {0,2,2,2,3,2},
                {0,2,2,2,3,3},
                {0,2,2,3,1,1},
                {0,2,2,3,1,2},
                {0,2,2,3,1,3},
                {0,2,2,3,2,1},
                {0,2,2,3,2,2},
                {0,2,2,3,2,3},
                {0,2,2,3,3,1},
                {0,2,2,3,3,2},
                {0,2,2,3,3,3},
                {0,2,3,1,1,1},
                {0,2,3,1,1,2},
                {0,2,3,1,1,3},
                {0,2,3,1,2,1},
                {0,2,3,1,2,2},
                {0,2,3,1,2,3},
                {0,2,3,1,3,1},
                {0,2,3,1,3,2},
                {0,2,3,1,3,3},
                {0,2,3,2,1,1},
                {0,2,3,2,1,2},
                {0,2,3,2,1,3},
                {0,2,3,2,2,1},
                {0,2,3,2,2,2},
                {0,2,3,2,2,3},
                {0,2,3,2,3,1},
                {0,2,3,2,3,2},
                {0,2,3,2,3,3},
                {0,2,3,3,1,1},
                {0,2,3,3,1,2},
                {0,2,3,3,1,3},
                {0,2,3,3,2,1},
                {0,2,3,3,2,2},
                {0,2,3,3,2,3},
                {0,2,3,3,3,1},
                {0,2,3,3,3,2},
                {0,2,3,3,3,3},
                {0,3,1,1,1,1},
                {0,3,1,1,1,2},
                {0,3,1,1,1,3},
                {0,3,1,1,2,1},
                {0,3,1,1,2,2},
                {0,3,1,1,2,3},
                {0,3,1,1,3,1},
                {0,3,1,1,3,2},
                {0,3,1,1,3,3},
                {0,3,1,2,1,1},
                {0,3,1,2,1,2},
                {0,3,1,2,1,3},
                {0,3,1,2,2,1},
                {0,3,1,2,2,2},
                {0,3,1,2,2,3},
                {0,3,1,2,3,1},
                {0,3,1,2,3,2},
                {0,3,1,2,3,3},
                {0,3,1,3,1,1},
                {0,3,1,3,1,2},
                {0,3,1,3,1,3},
                {0,3,1,3,2,1},
                {0,3,1,3,2,2},
                {0,3,1,3,2,3},
                {0,3,1,3,3,1},
                {0,3,1,3,3,2},
                {0,3,1,3,3,3},
                {0,3,2,1,1,1},
                {0,3,2,1,1,2},
                {0,3,2,1,1,3},
                {0,3,2,1,2,1},
                {0,3,2,1,2,2},
                {0,3,2,1,2,3},
                {0,3,2,1,3,1},
                {0,3,2,1,3,2},
                {0,3,2,1,3,3},
                {0,3,2,2,1,1},
                {0,3,2,2,1,2},
                {0,3,2,2,1,3},
                {0,3,2,2,2,1},
                {0,3,2,2,2,2},
                {0,3,2,2,2,3},
                {0,3,2,2,3,1},
                {0,3,2,2,3,2},
                {0,3,2,2,3,3},
                {0,3,2,3,1,1},
                {0,3,2,3,1,2},
                {0,3,2,3,1,3},
                {0,3,2,3,2,1},
                {0,3,2,3,2,2},
                {0,3,2,3,2,3},
                {0,3,2,3,3,1},
                {0,3,2,3,3,2},
                {0,3,2,3,3,3},
                {0,3,3,1,1,1},
                {0,3,3,1,1,2},
                {0,3,3,1,1,3},
                {0,3,3,1,2,1},
                {0,3,3,1,2,2},
                {0,3,3,1,2,3},
                {0,3,3,1,3,1},
                {0,3,3,1,3,2},
                {0,3,3,1,3,3},
                {0,3,3,2,1,1},
                {0,3,3,2,1,2},
                {0,3,3,2,1,3},
                {0,3,3,2,2,1},
                {0,3,3,2,2,2},
                {0,3,3,2,2,3},
                {0,3,3,2,3,1},
                {0,3,3,2,3,2},
                {0,3,3,2,3,3},
                {0,3,3,3,1,1},
                {0,3,3,3,1,2},
                {0,3,3,3,1,3},
                {0,3,3,3,2,1},
                {0,3,3,3,2,2},
                {0,3,3,3,2,3},
                {0,3,3,3,3,1},
                {0,3,3,3,3,2},
                {0,3,3,3,3,3}};


      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        RunModel(choiceProbabilityCalculator, householdDay, altPTypes, numberPersonsModeledJointly, choice);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, householdDay, altPTypes, numberPersonsModeledJointly);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
        choice = (int)chosenAlternative.Choice;

        int i = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          i++;
          if (i <= numberPersonsModeledJointly) {
            personDay.PatternType = altPTypes[choice, i];
          }
        }
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int[,] altPTypes, int numberPersonsModeledJointly, int choice = Constants.DEFAULT_VALUE) {

      if (householdDay.Household.Id == 24391) {
      }


      bool includeThreeWayInteractions = true;   // set this at compile time, dependign on whether we want to include or exclude 3-way interactions.
      int numberPersonTypes = 8;  // set this at compile time; 7 for Actum; 8 for others
      int numberAlternatives = numberPersonsModeledJointly == 4 ? 120 : 363;

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.Person.GetHouseholdDayPatternParticipationPriority()).ToList().Cast<PersonDayWrapper>();
      int hhsize = householdDay.Household.Size;


      Framework.DomainModels.Wrappers.IHouseholdWrapper household = householdDay.Household;

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
      //var personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
      //    [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
      double shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
      //var mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
      //    [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
      //var socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
      //    [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];

      int[,] pt = new int[6, 9];
      int[] li = new int[6];
      int[] ui = new int[6];
      int[] hi = new int[6];
      int[] hc = new int[6];
      int[] lc = new int[6];
      double[] ra = new double[6];
      double[] ea = new double[6];
      int[] c0to1 = new int[6];
      int[] c4to5 = new int[6];
      int[] c6to9 = new int[6];
      int[] c13to15 = new int[6];
      int[] c18to21 = new int[6];
      int[] fem = new int[6];
      int[] rto80 = new int[6];
      int[] wku40 = new int[6];
      int[] wknok = new int[6];
      int[] wkhom = new int[6];
      int[] wtmis = new int[6];
      int[] stmis = new int[6];
      int[] utmis = new int[6];

      int ct = 0;
      foreach (PersonDayWrapper personDay in orderedPersonDays) {
        ct++;
        if (ct <= numberPersonsModeledJointly) {
          PersonWrapper person = (PersonWrapper)personDay.Person;
          // calculate accessibility to work or school
          double mandatoryLogsum = 0;
          if (person.PersonType <= Global.Settings.PersonTypes.PartTimeWorker) {
            if (person.UsualWorkParcelId != Constants.DEFAULT_VALUE && person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
              if (person.UsualDeparturePeriodFromWork != Constants.DEFAULT_VALUE && person.UsualArrivalPeriodToWork != Constants.DEFAULT_VALUE) {
                ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualWorkParcel, (int)person.UsualArrivalPeriodToWork, (int)person.UsualDeparturePeriodFromWork, person.Household.HouseholdTotals.DrivingAgeMembers);
                mandatoryLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
              } else {
                ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, person.Household.HouseholdTotals.DrivingAgeMembers);
                mandatoryLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
              }
            } else {
              mandatoryLogsum = 0;
            }
          } else if (person.PersonType >= Global.Settings.PersonTypes.UniversityStudent) {
            if (person.UsualSchoolParcelId != 0 && person.UsualSchoolParcelId != -1 && person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
              ChoiceProbabilityCalculator.Alternative schoolNestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, person.Household.ResidenceParcel, person.UsualSchoolParcel, Global.Settings.Times.EightAM, Global.Settings.Times.TwoPM, person.Household.HouseholdTotals.DrivingAgeMembers);
              mandatoryLogsum = schoolNestedAlternative == null ? 0 : schoolNestedAlternative.ComputeLogsum();
            } else {
              mandatoryLogsum = 0;
            }
          }

          // set characteristics here that depend on person characteristics
          pt[ct, person.PersonType] = 1;
          li[ct] = householdDay.Household.Has0To25KIncome ? 1 : 0;
          ui[ct] = householdDay.Household.Has25To50KIncome ? 1 : 0;
          hi[ct] = householdDay.Household.Has100KPlusIncome ? 1 : 0;
          hc[ct] = (householdDay.Household.VehiclesAvailable >= householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers) ? 1 : 0;
          lc[ct] = (householdDay.Household.VehiclesAvailable < householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers) ? 1 : 0;
          ra[ct] = shoppingAggregateLogsum;
          ea[ct] = mandatoryLogsum;
          c0to1[ct] = (person.Age == 0 || person.Age == 1) ? 1 : 0;
          c4to5[ct] = (person.Age == 4 || person.Age == 5) ? 1 : 0;
          c6to9[ct] = (person.Age >= 6 && person.Age <= 9) ? 1 : 0;
          c13to15[ct] = (person.Age >= 13 && person.Age <= 15) ? 1 : 0;
          c18to21[ct] = (person.Age >= 18 && person.Age <= 21 && (person.PersonType >= Global.Settings.PersonTypes.UniversityStudent)) ? 1 : 0;
          rto80[ct] = (person.Age >= 80 && person.Age <= 98 && person.PersonType == Global.Settings.PersonTypes.RetiredAdult) ? 1 : 0;
          wku40[ct] = (person.Age >= 16 && person.Age <= 39 && person.IsFullOrPartTimeWorker) ? 1 : 0;
          wknok[ct] = (person.IsFullOrPartTimeWorker && !householdDay.Household.HasChildren) ? 1 : 0;
          wkhom[ct] = (person.IsFullOrPartTimeWorker && person.WorksAtHome()) ? 1 : 0;
          wtmis[ct] = (person.IsFullOrPartTimeWorker && person.UsualWorkParcel == null) ? 1 : 0;
          stmis[ct] = (person.PersonType >= Global.Settings.PersonTypes.DrivingAgeStudent && person.UsualSchoolParcel == null) ? 1 : 0;
          utmis[ct] = (person.PersonType == Global.Settings.PersonTypes.UniversityStudent && person.UsualSchoolParcel == null) ? 1 : 0;
          fem[ct] = person.Gender == Global.Settings.PersonGenders.Female ? 1 : 0;
        }
      }

      //two-way interaction variables
      int[,,,,,,] i2 = new int[9, 9, 6, 6, 6, 6, 6];
      // interaction terms for each pair of two different persons in the householdDayPatternType
      // pt[p1,t1] = 1 if person p1 (who may have any Id between 1 and p2-1) has person type t1
      // pt[p2,t2] = 1 if person p2 (who may have any Id between 2 and 5) has person type t2
      if (hhsize >= 2) {
        for (int t2 = 1; t2 <= numberPersonTypes; t2++) {
          for (int t1 = 1; t1 <= t2; t1++) {
            //for (var t1 = 1; t1 <= numberPersonTypes; t1++) {

            for (int p2 = 2; p2 <= numberPersonsModeledJointly; p2++) {
              for (int p1 = 1; p1 < p2; p1++) {
                i2[t1, t2, p1, p2, 0, 0, 0] = pt[p1, t1] * pt[p2, t2];  // i2[t1,t2,p1,p2,0,0,0] = 1 if person p1 has person type t1 and person p2 has person type t2
                if (t1 != t2) {
                  i2[t1, t2, p1, p2, 0, 0, 0] = i2[t1, t2, p1, p2, 0, 0, 0] + pt[p1, t2] * pt[p2, t1]; // i2[t1,t2,p1,p2,0,0,0] = or if one person p2 person type t1 and person p1 has person type t2
                }
              }
            }


            // pairwise interaction terms for each triplet of three different persons in the householdDayPatternType
            // constructed from two-way interaction variables
            // p1, p2 and p3 must be in ascending personType sequence 

            if (hhsize >= 3) {
              for (int p3 = 3; p3 <= numberPersonsModeledJointly; p3++) {
                for (int p2 = 2; p2 < p3; p2++) {
                  for (int p1 = 1; p1 < p2; p1++) {
                    i2[t1, t2, p1, p2, p3, 0, 0] = i2[t1, t2, p1, p2, 0, 0, 0]
                                                          + i2[t1, t2, p1, p3, 0, 0, 0]
                                                          + i2[t1, t2, p2, p3, 0, 0, 0];
                  }
                }
              }
            }

            // pairwise interaction terms for each quadruplet of four different persons in the householdDayPatternType

            if (hhsize >= 4) {
              for (int p4 = 4; p4 <= numberPersonsModeledJointly; p4++) {
                for (int p3 = 3; p3 < p4; p3++) {
                  for (int p2 = 2; p2 < p3; p2++) {
                    for (int p1 = 1; p1 < p2; p1++) {
                      i2[t1, t2, p1, p2, p3, p4, 0] = i2[t1, t2, p1, p2, 0, 0, 0]
                                                              + i2[t1, t2, p1, p3, 0, 0, 0]
                                                              + i2[t1, t2, p1, p4, 0, 0, 0]
                                                              + i2[t1, t2, p2, p3, 0, 0, 0]
                                                              + i2[t1, t2, p2, p4, 0, 0, 0]
                                                              + i2[t1, t2, p3, p4, 0, 0, 0];
                    }
                  }
                }
              }
            }

            // pairwise interaction terms for the five different persons in the householdDayPatternType
            if (hhsize >= 5 && numberPersonsModeledJointly == 5) {
              i2[t1, t2, 1, 2, 3, 4, 5] = i2[t1, t2, 1, 2, 0, 0, 0]
                                                + i2[t1, t2, 1, 3, 0, 0, 0]
                                                + i2[t1, t2, 1, 4, 0, 0, 0]
                                                + i2[t1, t2, 1, 5, 0, 0, 0]
                                                + i2[t1, t2, 2, 3, 0, 0, 0]
                                                + i2[t1, t2, 2, 4, 0, 0, 0]
                                                + i2[t1, t2, 2, 5, 0, 0, 0]
                                                + i2[t1, t2, 3, 4, 0, 0, 0]
                                                + i2[t1, t2, 3, 5, 0, 0, 0]
                                                + i2[t1, t2, 4, 5, 0, 0, 0];
            }
          }
        }
      }

      // 3-way interaction variables
      int[,,,,,,,] i3 = new int[4, 4, 4, 6, 6, 6, 6, 6];
      int[,] xt = new int[6, 4];

      if (includeThreeWayInteractions) {

        ct = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          ct++;
          if (ct <= numberPersonsModeledJointly) {
            if (personDay.Person.PersonType == 1) { xt[ct, 1] = 1; }
            if (personDay.Person.PersonType == 2 || personDay.Person.PersonType == 4) { xt[ct, 2] = 1; }
            if (personDay.Person.PersonType == 7 || personDay.Person.PersonType == 8) { xt[ct, 3] = 1; }
          }
        }

        // 3-way interaction terms for each triplet of persons in the householdDayPatternType
        // p1, p2 and p3 must be in ascending personType sequence 
        if (hhsize >= 3) {
          for (int t3 = 1; t3 <= 3; t3++) {
            //for (var t2 = 1; t2 <= t3; t2++) {
            //for (var t1 = 1; t1 <= t2; t1++) {
            for (int t2 = 1; t2 <= 3; t2++) {
              for (int t1 = 1; t1 <= 3; t1++) {

                for (int p3 = 3; p3 <= numberPersonsModeledJointly; p3++) {
                  for (int p2 = 2; p2 < p3; p2++) {
                    for (int p1 = 1; p1 < p2; p1++) {
                      i3[t1, t2, t3, p1, p2, p3, 0, 0] = xt[p1, t1] * xt[p2, t2] * xt[p3, t3];
                      if (t2 != t3) {
                        i3[t1, t2, t3, p1, p2, p3, 0, 0] = i3[t1, t2, t3, p1, p2, p3, 0, 0] + xt[p1, t1] * xt[p2, t3] * xt[p3, t2];
                      }
                      if (t1 != t2) {
                        i3[t1, t2, t3, p1, p2, p3, 0, 0] = i3[t1, t2, t3, p1, p2, p3, 0, 0] + xt[p1, t2] * xt[p2, t1] * xt[p3, t3];
                      }
                      if (t1 != t3) {
                        i3[t1, t2, t3, p1, p2, p3, 0, 0] = i3[t1, t2, t3, p1, p2, p3, 0, 0] + xt[p1, t3] * xt[p2, t2] * xt[p3, t1];
                      }
                      if (t1 != t3 && t1 != t2 && t2 != t3) {
                        i3[t1, t2, t3, p1, p2, p3, 0, 0] = i3[t1, t2, t3, p1, p2, p3, 0, 0] + xt[p1, t2] * xt[p2, t3] * xt[p3, t1] + xt[p1, t3] * xt[p2, t1] * xt[p3, t2];
                      }
                    }
                  }
                }

                // 3-way interaction terms for each quadruplet of persons in the householdDayPatternType
                if (hhsize >= 4) {
                  for (int p4 = 4; p4 <= 5; p4++) {
                    for (int p3 = 3; p3 < p4; p3++) {
                      for (int p2 = 2; p2 < p3; p2++) {
                        for (int p1 = 1; p1 < p2; p1++) {
                          i3[t1, t2, t3, p1, p2, p3, p4, 0] = i3[t1, t2, t3, p1, p2, p3, 0, 0]
                                                                       + i3[t1, t2, t3, p1, p2, p4, 0, 0]
                                                                       + i3[t1, t2, t3, p1, p3, p4, 0, 0]
                                                                       + i3[t1, t2, t3, p2, p3, p4, 0, 0];
                        }
                      }
                    }
                  }
                }

                // 3-way interaction terms for the five different persons in the householdDayPatternType
                if (hhsize >= 5 && numberPersonsModeledJointly == 5) {
                  i3[t1, t2, t3, 1, 2, 3, 4, 5]
                  = i3[t1, t2, t3, 1, 2, 3, 0, 0]
                  + i3[t1, t2, t3, 1, 2, 4, 0, 0]
                  + i3[t1, t2, t3, 1, 2, 5, 0, 0]
                  + i3[t1, t2, t3, 1, 3, 4, 0, 0]
                  + i3[t1, t2, t3, 1, 3, 5, 0, 0]
                  + i3[t1, t2, t3, 1, 4, 5, 0, 0]
                  + i3[t1, t2, t3, 2, 3, 4, 0, 0]
                  + i3[t1, t2, t3, 2, 3, 5, 0, 0]
                  + i3[t1, t2, t3, 2, 4, 5, 0, 0]
                  + i3[t1, t2, t3, 3, 4, 5, 0, 0];
                }
              }
            }
          }
        }
      }

      int[,] component1 = new int[4, 6];
      int[,,,,,] component2 = new int[4, 6, 6, 6, 6, 6];
      int[,,,,,] component3 = new int[4, 6, 6, 6, 6, 6];
      int compNum = 0;

      for (int purp = 1; purp <= 3; purp++) {
        for (int p1 = 1; p1 <= numberPersonsModeledJointly; p1++) {
          //create the personPurpose component
          compNum++;
          component1[purp, p1] = compNum;
          choiceProbabilityCalculator.CreateUtilityComponent(compNum);
          //populate the personPurpose component with utility terms
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 1, pt[p1, 1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 2, pt[p1, 2]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 3, pt[p1, 3]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 4, pt[p1, 4]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 5, pt[p1, 5]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 6, pt[p1, 6]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 7, pt[p1, 7]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 8, pt[p1, 8]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 9, c0to1[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 10, c4to5[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 11, c6to9[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 12, c13to15[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 13, c18to21[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 14, wku40[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 15, rto80[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 16, pt[p1, 1] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 17, pt[p1, 2] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 18, pt[p1, 3] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 19, pt[p1, 4] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 20, pt[p1, 5] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 21, pt[p1, 6] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 22, pt[p1, 7] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 23, pt[p1, 8] * fem[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 24, pt[p1, 1] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 25, pt[p1, 2] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 26, pt[p1, 3] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 27, pt[p1, 4] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 28, pt[p1, 5] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 29, pt[p1, 6] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 30, pt[p1, 7] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 31, pt[p1, 8] * hc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 32, pt[p1, 1] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 33, pt[p1, 2] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 34, pt[p1, 3] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 35, pt[p1, 4] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 36, pt[p1, 5] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 38, pt[p1, 6] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 38, pt[p1, 7] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 39, pt[p1, 8] * lc[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 40, pt[p1, 1] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 41, pt[p1, 2] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 42, pt[p1, 3] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 43, pt[p1, 4] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 44, pt[p1, 5] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 46, pt[p1, 6] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 46, pt[p1, 7] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 47, pt[p1, 8] * li[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 48, pt[p1, 1] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 49, pt[p1, 2] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 50, pt[p1, 3] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 51, pt[p1, 4] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 52, pt[p1, 5] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 53, pt[p1, 6] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 54, pt[p1, 7] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 55, pt[p1, 8] * ui[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 56, pt[p1, 1] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 57, pt[p1, 2] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 58, pt[p1, 3] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 59, pt[p1, 4] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 60, pt[p1, 5] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 61, pt[p1, 6] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 62, pt[p1, 7] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 63, pt[p1, 8] * hi[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 64, pt[p1, 1] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 65, pt[p1, 2] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 66, pt[p1, 3] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 67, pt[p1, 4] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 68, pt[p1, 5] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 69, pt[p1, 6] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 70, pt[p1, 7] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 71, pt[p1, 8] * ea[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 72, pt[p1, 1] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 73, pt[p1, 2] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 74, pt[p1, 3] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 75, pt[p1, 4] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 76, pt[p1, 5] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 77, pt[p1, 6] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 78, pt[p1, 7] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 79, pt[p1, 8] * ra[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 80, wkhom[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 81, wtmis[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 82, stmis[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 83, utmis[p1]);
          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 84, wknok[p1]);
          // TODO : Add more personPurpose component terms

          // set up 2-person and 3-person interaction components (always need to set them in estimation mode)
          if (hhsize >= 2 || Global.Configuration.IsInEstimationMode) {
            for (int p2 = (p1 + 1); p2 <= numberPersonsModeledJointly; p2++) {
              //create the 2-way component for cases where 2 people share a purpose
              compNum++;
              component2[purp, p1, p2, 0, 0, 0] = compNum;
              choiceProbabilityCalculator.CreateUtilityComponent(compNum);
              //populate the 2-way component with utility terms
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 301, i2[1, 1, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 302, i2[1, 2, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 303, i2[1, 3, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 304, i2[1, 4, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 305, i2[1, 5, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 306, i2[1, 6, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 307, i2[1, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 308, i2[1, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 309, i2[2, 2, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 310, i2[2, 3, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 311, i2[2, 4, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 312, i2[2, 5, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 313, i2[2, 6, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 314, i2[2, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 315, i2[2, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 316, i2[3, 3, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 317, i2[3, 4, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 318, i2[3, 5, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 6, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 322, i2[4, 4, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 323, i2[4, 5, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 324, i2[4, 6, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 325, i2[4, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 326, i2[4, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 5, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 6, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 6, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 334, i2[7, 7, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 335, i2[7, 8, p1, p2, 0, 0, 0]);
              choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 336, i2[8, 8, p1, p2, 0, 0, 0]);

              if (hhsize >= 3 || Global.Configuration.IsInEstimationMode) {
                for (int p3 = (p2 + 1); p3 <= 5; p3++) {
                  //create the 2-way component for cases where three people share a purpose
                  compNum++;
                  component2[purp, p1, p2, p3, 0, 0] = compNum;
                  choiceProbabilityCalculator.CreateUtilityComponent(compNum);
                  //populate the 2-way component with utility terms for cases where three people share a purpose
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 301, i2[1, 1, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 302, i2[1, 2, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 303, i2[1, 3, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 304, i2[1, 4, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 305, i2[1, 5, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 306, i2[1, 6, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 307, i2[1, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 308, i2[1, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 309, i2[2, 2, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 310, i2[2, 3, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 311, i2[2, 4, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 312, i2[2, 5, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 313, i2[2, 6, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 314, i2[2, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 315, i2[2, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 316, i2[3, 3, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 317, i2[3, 4, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 318, i2[3, 5, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 6, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 322, i2[4, 4, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 323, i2[4, 5, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 324, i2[4, 6, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 325, i2[4, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 326, i2[4, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 5, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 6, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 6, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 334, i2[7, 7, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 335, i2[7, 8, p1, p2, p3, 0, 0]);
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 336, i2[8, 8, p1, p2, p3, 0, 0]);

                  //create the 3-way component with utility terms for cases where three people share a purpose
                  compNum++;
                  component3[purp, p1, p2, p3, 0, 0] = compNum;
                  choiceProbabilityCalculator.CreateUtilityComponent(compNum);
                  //populate the 3-way component with utility terms for cases where three people share a purpose
                  if (includeThreeWayInteractions) {
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 337, i3[1, 1, 1, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 338, i3[1, 1, 2, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 339, i3[1, 1, 3, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 340, i3[1, 2, 2, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 341, i3[1, 2, 3, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 342, i3[1, 3, 3, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 343, i3[2, 2, 2, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 344, i3[2, 2, 3, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 345, i3[2, 3, 3, p1, p2, p3, 0, 0]);
                    choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 346, i3[3, 3, 3, p1, p2, p3, 0, 0]);
                  }
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 353, hhsize >= 3 ? 1.0 : 0.0); // exactly 3 of up to 5 persons in HH have same pattern type
                  choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 356, hhsize == 3 ? 1.0 : 0.0); // exactly 3 of 3 persons in HH have same pattern type

                  if (hhsize >= 4 || Global.Configuration.IsInEstimationMode) {
                    for (int p4 = (p3 + 1); p4 <= 5; p4++) {
                      //create the 2-way component for cases where four people share a purpose
                      compNum++;
                      component2[purp, p1, p2, p3, p4, 0] = compNum;
                      choiceProbabilityCalculator.CreateUtilityComponent(compNum);
                      //populate the 2-way component with utility terms for cases where four people share a purpose
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 301, i2[1, 1, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 302, i2[1, 2, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 303, i2[1, 3, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 304, i2[1, 4, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 305, i2[1, 5, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 306, i2[1, 6, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 307, i2[1, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 308, i2[1, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 309, i2[2, 2, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 310, i2[2, 3, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 311, i2[2, 4, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 312, i2[2, 5, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 313, i2[2, 6, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 314, i2[2, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 315, i2[2, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 316, i2[3, 3, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 317, i2[3, 4, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 318, i2[3, 5, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 6, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 322, i2[4, 4, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 323, i2[4, 5, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 324, i2[4, 6, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 325, i2[4, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 326, i2[4, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 5, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 6, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 6, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 334, i2[7, 7, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 335, i2[7, 8, p1, p2, p3, p4, 0]);
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 336, i2[8, 8, p1, p2, p3, p4, 0]);

                      //create the 3-way component with utility terms for cases where four people share a purpose
                      compNum++;
                      component3[purp, p1, p2, p3, p4, 0] = compNum;
                      choiceProbabilityCalculator.CreateUtilityComponent(compNum);
                      //populate the 3-way component with utility terms for cases where four people share a purpose
                      if (includeThreeWayInteractions) {
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 337, i3[1, 1, 1, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 338, i3[1, 1, 2, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 339, i3[1, 1, 3, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 340, i3[1, 2, 2, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 341, i3[1, 2, 3, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 342, i3[1, 3, 3, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 343, i3[2, 2, 2, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 344, i3[2, 2, 3, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 345, i3[2, 3, 3, p1, p2, p3, p4, 0]);
                        choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 346, i3[3, 3, 3, p1, p2, p3, p4, 0]);
                      }
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 354, hhsize >= 4 ? 1.0 : 0.0);  // exactly 4 of up to 5 persons in HH have same pattern type
                      choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 356, hhsize == 4 ? 1.0 : 0.0);  // exactly 4 of 4 persons in HH have same pattern type

                      if ((hhsize >= 5 || Global.Configuration.IsInEstimationMode) && numberPersonsModeledJointly == 5) {
                        for (int p5 = (p4 + 1); p5 <= numberPersonsModeledJointly; p5++) {
                          //create the 2-way component for cases where five people share a purpose
                          compNum++;
                          component2[purp, p1, p2, p3, p4, p5] = compNum;
                          choiceProbabilityCalculator.CreateUtilityComponent(compNum);
                          //populate the 2-way component with utility terms for cases where five people share a purpose
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 301, i2[1, 1, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 302, i2[1, 2, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 303, i2[1, 3, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 304, i2[1, 4, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 305, i2[1, 5, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 306, i2[1, 6, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 307, i2[1, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 308, i2[1, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 309, i2[2, 2, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 310, i2[2, 3, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 311, i2[2, 4, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 312, i2[2, 5, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 313, i2[2, 6, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 314, i2[2, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 315, i2[2, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 316, i2[3, 3, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 317, i2[3, 4, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 318, i2[3, 5, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 6, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 319, i2[3, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 322, i2[4, 4, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 323, i2[4, 5, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 324, i2[4, 6, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 325, i2[4, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 326, i2[4, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 5, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 328, i2[5, 6, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 329, i2[5, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 6, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 331, i2[6, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 334, i2[7, 7, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 335, i2[7, 8, p1, p2, p3, p4, p5]);
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 336, i2[8, 8, p1, p2, p3, p4, p5]);

                          //create the 3-way component for cases where five people share a purpose
                          compNum++;
                          component3[purp, p1, p2, p3, p4, p5] = compNum;
                          choiceProbabilityCalculator.CreateUtilityComponent(compNum);
                          //populate the 3-way component with utility terms for cases where five people share a purpose
                          if (includeThreeWayInteractions) {
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 337, i3[1, 1, 1, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 338, i3[1, 1, 2, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 339, i3[1, 1, 3, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 340, i3[1, 2, 2, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 341, i3[1, 2, 3, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 342, i3[1, 3, 3, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 343, i3[2, 2, 2, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 344, i3[2, 2, 3, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 345, i3[2, 3, 3, p1, p2, p3, p4, p5]);
                            choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 346, i3[3, 3, 3, p1, p2, p3, p4, p5]);
                          }
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 354, hhsize >= 5 ? 1.0 : 0.0);  // exactly 5 of up to 5 persons in HH have same pattern type
                          choiceProbabilityCalculator.GetUtilityComponent(compNum).AddUtilityTerm(100 * purp + 356, hhsize == 5 ? 1.0 : 0.0);  // exactly 5 of 5 persons in HH have same pattern type
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }

      //Generate utility funtions for the alternatives
      bool[] available = new bool[numberAlternatives];
      bool[] chosen = new bool[numberAlternatives];
      for (int alt = 0; alt <= numberAlternatives - 1; alt++) {

        available[alt] = false;
        chosen[alt] = false;
        // set availability based on household size
        if ((hhsize == 1 && (alt + 1) <= 3)
        || (hhsize == 2 && (alt + 1) >= 4 && (alt + 1) <= 12)
        || (hhsize == 3 && (alt + 1) >= 13 && (alt + 1) <= 39)
        || ((hhsize == 4 || (hhsize >= 4 && numberPersonsModeledJointly == 4)) && (alt + 1) >= 40 && (alt + 1) <= 120)
        || (hhsize >= 5 && numberPersonsModeledJointly == 5 && (alt + 1) >= 121 && (alt + 1) <= 363)) {
          available[alt] = true;
        }

        // limit availability of work patterns for people who are neither worker nor student
        ct = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          if (personDay.Household.Id == 44843 && personDay.Person.Sequence == 6) {
          }
          ct++;
          //if (ct <= 5 && altPTypes[alt, ct] == 1 && !personDay.Person.IsWorker && !personDay.Person.IsStudent) {
          if (ct <= numberPersonsModeledJointly && altPTypes[alt, ct] == 1 &&
              (personDay.Person.IsNonworkingAdult || personDay.Person.IsRetiredAdult ||
              (!personDay.Person.IsWorker && !personDay.Person.IsStudent) ||
                (!Global.Configuration.IsInEstimationMode && !personDay.Person.IsWorker && personDay.Person.UsualSchoolParcel == null)
              )) {
            available[alt] = false;
          }
        }

        // determine choice
        if (choice == alt) { chosen[alt] = true; }

        //Get the alternative
        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(alt, available[alt], chosen[alt]);

        alternative.Choice = alt;

        for (int purp = 1; purp <= 3; purp++) {

          //Add utility components
          //MB>  This looks like it should work fine.  It's actually a bit different and potentially better than the way I coded 'writeut', because that only 
          // allowed the interactions to be applied to one purpose per alterantive, but if you have 4+ people, there could actually be 2 different 2-person pairs
          // such as in an M-N-N-M  pattern, that has both MM and NN pairs. 
          int componentPosition = 1;
          int[] componentP = new int[6];
          for (int p = 1; p <= numberPersonsModeledJointly; p++) {
            if (altPTypes[alt, p] == purp) {
              alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(component1[purp, p]));

              //alternatives position-purpose matches current purpose; note this position in current component position for component types 2 and 3  
              componentP[componentPosition] = p;
              componentPosition++;
            }
          }
          if (componentP[1] > 0 && componentP[2] > componentP[1]
              && (componentP[3] > componentP[2] || componentP[3] == 0)
              && (componentP[4] > componentP[3] || componentP[4] == 0)
              && (componentP[4] > componentP[2] || componentP[4] == 0)
              && (componentP[5] > componentP[4] || componentP[5] == 0)
              && (componentP[5] > componentP[3] || componentP[5] == 0)
              && (componentP[5] > componentP[2] || componentP[5] == 0)
              && choiceProbabilityCalculator.GetUtilityComponent(component2[purp, componentP[1], componentP[2], componentP[3], componentP[4], componentP[5]]) != null) {
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(component2[purp, componentP[1], componentP[2], componentP[3], componentP[4], componentP[5]]));
          }
          //            if (includeThreeWayInteractions) {
          if (componentP[1] > 0 && componentP[2] > componentP[1] && componentP[3] > componentP[2]
              && (componentP[4] > componentP[3] || componentP[4] == 0)
              && (componentP[5] > componentP[4] || componentP[5] == 0)
              && (componentP[5] > componentP[3] || componentP[5] == 0)
              && choiceProbabilityCalculator.GetUtilityComponent(component3[purp, componentP[1], componentP[2], componentP[3], componentP[4], componentP[5]]) != null) {
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(component3[purp, componentP[1], componentP[2], componentP[3], componentP[4], componentP[5]]));
          }
          //            }
        }
      }
    }
  }
}
