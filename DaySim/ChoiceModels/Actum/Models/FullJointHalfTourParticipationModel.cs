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
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Actum.Models {
  public class FullJointHalfTourParticipationModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "ActumFullJointHalfTourParticipationModel";
    private const int TOTAL_ALTERNATIVES = 32;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 60;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.FullJointHalfTourParticipationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public int[] Run(HouseholdDayWrapper householdDay, int jHTSimulated, int genChoice, bool[,] jHTAvailable, bool[] fHTAvailable, bool[,] jHTParticipation) {
      if (householdDay == null) {
        throw new ArgumentNullException("householdDay");
      }

      // array associating alternative with the participation of each HH person in the joint half tour
      //  also identifies minimum housheold size and number of participants for each alternative 
      //[alt,p1,p2,p3,p4,p5,MinHHSize,numPart]
      int[][] altParticipants = new int[32][];
      altParticipants[0] = new int[] { 0, 0, 0, 0, 0, 0, 2, 0 };
      altParticipants[1] = new int[] { 1, 1, 0, 0, 0, 0, 2, 1 };
      altParticipants[2] = new int[] { 2, 0, 1, 0, 0, 0, 2, 1 };
      altParticipants[3] = new int[] { 3, 1, 1, 0, 0, 0, 2, 2 };
      altParticipants[4] = new int[] { 4, 0, 0, 1, 0, 0, 3, 1 };
      altParticipants[5] = new int[] { 5, 1, 0, 1, 0, 0, 3, 2 };
      altParticipants[6] = new int[] { 6, 0, 1, 1, 0, 0, 3, 2 };
      altParticipants[7] = new int[] { 7, 1, 1, 1, 0, 0, 3, 3 };
      altParticipants[8] = new int[] { 8, 0, 0, 0, 1, 0, 4, 1 };
      altParticipants[9] = new int[] { 9, 1, 0, 0, 1, 0, 4, 2 };
      altParticipants[10] = new int[] { 10, 0, 1, 0, 1, 0, 4, 2 };
      altParticipants[11] = new int[] { 11, 1, 1, 0, 1, 0, 4, 3 };
      altParticipants[12] = new int[] { 12, 0, 0, 1, 1, 0, 4, 2 };
      altParticipants[13] = new int[] { 13, 1, 0, 1, 1, 0, 4, 3 };
      altParticipants[14] = new int[] { 14, 0, 1, 1, 1, 0, 4, 3 };
      altParticipants[15] = new int[] { 15, 1, 1, 1, 1, 0, 4, 4 };
      altParticipants[16] = new int[] { 16, 0, 0, 0, 0, 1, 5, 1 };
      altParticipants[17] = new int[] { 17, 1, 0, 0, 0, 1, 5, 2 };
      altParticipants[18] = new int[] { 18, 0, 1, 0, 0, 1, 5, 2 };
      altParticipants[19] = new int[] { 19, 1, 1, 0, 0, 1, 5, 3 };
      altParticipants[20] = new int[] { 20, 0, 0, 1, 0, 1, 5, 2 };
      altParticipants[21] = new int[] { 21, 1, 0, 1, 0, 1, 5, 3 };
      altParticipants[22] = new int[] { 22, 0, 1, 1, 0, 1, 5, 3 };
      altParticipants[23] = new int[] { 23, 1, 1, 1, 0, 1, 5, 4 };
      altParticipants[24] = new int[] { 24, 0, 0, 0, 1, 1, 5, 2 };
      altParticipants[25] = new int[] { 25, 1, 0, 0, 1, 1, 5, 3 };
      altParticipants[26] = new int[] { 26, 0, 1, 0, 1, 1, 5, 3 };
      altParticipants[27] = new int[] { 27, 1, 1, 0, 1, 1, 5, 4 };
      altParticipants[28] = new int[] { 28, 0, 0, 1, 1, 1, 5, 3 };
      altParticipants[29] = new int[] { 29, 1, 0, 1, 1, 1, 5, 4 };
      altParticipants[30] = new int[] { 30, 0, 1, 1, 1, 1, 5, 4 };
      altParticipants[31] = new int[] { 31, 1, 1, 1, 1, 1, 5, 5 };

      householdDay.ResetRandom(925 + jHTSimulated);

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
      int choice = 0;

      if (Global.Configuration.IsInEstimationMode) {
        int i = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          i++;
          if (i <= 5) {
            choice = choice + (jHTParticipation[jHTSimulated, personDay.Person.Sequence] == true ? 1 : 0) * (int)Math.Pow(2, i - 1);
          }
        }
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return altParticipants[choice];
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ jHTSimulated);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, fHTAvailable, altParticipants, choice);

        choiceProbabilityCalculator.WriteObservation();

        return altParticipants[choice];
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, fHTAvailable, altParticipants);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, choice);

        Global.Configuration.IsInEstimationMode = true;

        return altParticipants[choice];
      } else {
        RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, fHTAvailable, altParticipants);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", householdDay.Household.Id);
          if (!Global.Configuration.IsInEstimationMode) {
            householdDay.IsValid = false;
          }
          return null;
        }

        choice = (int)chosenAlternative.Choice;

        return altParticipants[choice];
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int jHTSimulated, int genChoice, bool[,] jHTAvailable, bool[] fHTAvailable, int[][] altParticipants, int choice = Constants.DEFAULT_VALUE) {

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

      IPersonDayWrapper pPersonDay = null;

      // set household characteristics here that don't depend on person characteristics

      int hhsize = householdDay.Household.Size;

      int hhinc1 = householdDay.Household.Income <= 300000 ? 1 : 0;
      int hhinc2 = (householdDay.Household.Income > 300000 && householdDay.Household.Income <= 600000) ? 1 : 0;
      int hhinc3 = (householdDay.Household.Income > 600000 && householdDay.Household.Income <= 900000) ? 1 : 0;
      //int hhinc4 = (householdDay.Household.Income > 900000 && householdDay.Household.Income <= 1200000) ? 1 : 0;
      int hhinc4 = (householdDay.Household.Income > 900000) ? 1 : 0;

      IParcelWrapper[] pUsualLocation = new IParcelWrapper[6];
      int[] pPatternType = new int[6];
      int[] pConstant = new int[6];

      int[] pType9 = new int[6];
      int[] pType8 = new int[6];
      int[] pType7 = new int[6];
      int[] pType6 = new int[6];
      int[] pType5 = new int[6];
      int[] pType4 = new int[6];
      int[] pType3 = new int[6];
      int[] pType2 = new int[6];
      int[] pType1 = new int[6];
      int[] pAdult = new int[6];
      int[] pAdultWithChildrenUnder16 = new int[6];
      int[] pAdultFemale = new int[6];
      int[] pAdultNonMandatory = new int[6];
      int[] pType7AgeUnder12 = new int[6];
      int[] pType7Age12Plus = new int[6];
      int[] pAgeUnder12 = new int[6];

      int[] pType8Mandatory = new int[6];
      int[] pType8NonMandatory = new int[6];

      int[] pType7Mandatory = new int[6];
      int[] pType7NonMandatory = new int[6];

      int[] pAgeUnder16 = new int[6];
      int[] pYouthMandatory = new int[6];
      int[] pYouthNonMandatory = new int[6];
      int[] pYouth = new int[6];
      int[] pAdultMandatory = new int[6];
      int[] pMandatory = new int[6];
      int[] pNonMandatory = new int[6];

      bool[] pHasMandatoryTourToUsualLocation = new bool[6];
      bool[] pIsDrivingAge = new bool[6];

      int count = 0;
      foreach (PersonDayWrapper personDay in orderedPersonDays) {
        count++;
        if (count <= 5) {
          if (count == 1) {
            pPersonDay = personDay;
          }

          // set characteristics here that depend on person characteristics
          if (personDay.Person.IsFullOrPartTimeWorker) {
            pUsualLocation[count] = personDay.Person.UsualWorkParcel;
          } else if (personDay.Person.IsStudent) {
            pUsualLocation[count] = personDay.Person.UsualSchoolParcel;
          } else if (personDay.Person.IsWorker && personDay.Person.IsNotFullOrPartTimeWorker) {
            pUsualLocation[count] = personDay.Person.UsualWorkParcel;
          } else {
            pUsualLocation[count] = personDay.Household.ResidenceParcel;
          }

          pPatternType[count] = personDay.PatternType;
          pConstant[count] = 1;

          pType9[count] = personDay.Person.IsChildUnder16.ToFlag(); // not one og Type 1 to 8

          pType8[count] = personDay.Person.IsChildUnder5.ToFlag(); // All ACTUM TU persons are one of Type 1 to 8 
          pType7[count] = personDay.Person.IsChildAge5Through15.ToFlag();
          pType6[count] = personDay.Person.IsDrivingAgeStudent.ToFlag();
          pType5[count] = personDay.Person.IsUniversityStudent.ToFlag();
          pType4[count] = personDay.Person.IsNonworkingAdult.ToFlag();
          pType3[count] = personDay.Person.IsRetiredAdult.ToFlag();
          pType2[count] = personDay.Person.IsPartTimeWorker.ToFlag();
          pType1[count] = personDay.Person.IsFulltimeWorker.ToFlag();
          pAdult[count] = personDay.Person.IsAdult.ToFlag();
          pAdultWithChildrenUnder16[count] = (personDay.Person.IsAdult && personDay.Household.HasChildrenUnder16).ToFlag(); // THIS person is adult and HH has child. under 16
          pAdultFemale[count] = personDay.Person.IsAdultFemale.ToFlag();
          pAdultNonMandatory[count] = (personDay.Person.IsAdult && personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();
          pType7AgeUnder12[count] = (personDay.Person.IsChildAge5Through15 && personDay.Person.Age < 12).ToFlag(); // THIS person is both 5-15 AND below 12
          pType7Age12Plus[count] = (personDay.Person.IsChildAge5Through15 && personDay.Person.Age >= 12).ToFlag();
          pAgeUnder12[count] = (personDay.Person.Age < 12).ToFlag();
          pAgeUnder16[count] = (personDay.Person.Age < 16).ToFlag();

          pType8Mandatory[count] = (personDay.Person.IsChildUnder5 && personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
          pType8NonMandatory[count] = (personDay.Person.IsChildUnder5 && personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();

          pType7Mandatory[count] = (personDay.Person.IsChildAge5Through15 && personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
          pType7NonMandatory[count] = (personDay.Person.IsChildAge5Through15 && personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();

          pYouthMandatory[count] = (!personDay.Person.IsChildUnder5 && !personDay.Person.IsAdult && personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
          pYouthNonMandatory[count] = (!personDay.Person.IsChildUnder5 && !personDay.Person.IsAdult && personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();
          pYouth[count] = (!personDay.Person.IsChildUnder5 && !personDay.Person.IsAdult).ToFlag();

          pAdultMandatory[count] = (personDay.Person.IsAdult && personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();

          pMandatory[count] = (personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
          pNonMandatory[count] = (personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();

          pHasMandatoryTourToUsualLocation[count] = personDay.HasMandatoryTourToUsualLocation;
          pIsDrivingAge[count] = personDay.Person.IsDrivingAge;

        }
      }

      int componentIndex = 0;
      //Create person utility components
      int[] componentPerson = new int[6];
      for (int p = 1; p <= 5; p++) {
        // create component for basic person-purposes
        componentIndex++;
        componentPerson[p] = componentIndex;
        choiceProbabilityCalculator.CreateUtilityComponent(componentPerson[p]);
        // these are dummies compared to base one, i.e. Type 5+6 in one. 
        // OBS! - We apply "Adult Mandatory" as the base
        //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(1, pAdultMandatory[p]); // impact of adult with mandatory travel 
        choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(2, pAdultNonMandatory[p]); // impact of adult with non-mandatory travel 

        choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(3, pType8Mandatory[p]); // impact of child under 5 with mandatory travel
        choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(4, pType8NonMandatory[p]); // impact of child under 5 with non-mandatory travel

        //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(04, pYouthMandatory[p]); // impact of youth with mandatory travel
        //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(05, pYouthNonMandatory[p]); // impact of youth with non-mandatory travel
        choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(5, pType7Mandatory[p]); // impact of Child5-16 with mandatory travel
        choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(6, pType7NonMandatory[p]); // impact of Child5-16 with non-mandatory travel

        choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(7, pAdultFemale[p]); //female

      }

      //create two-way match interaction utility components
      int[,] componentMatch = new int[6, 6];
      int[,] iMatchAgeUnder12 = new int[6, 6];
      int[,] iMatchAgeUnder16 = new int[6, 6];
      int[,] iMatchAgeUnder5 = new int[6, 6];
      int[,] iMatchAge5to16 = new int[6, 6];

      int[,] iMatchAdult = new int[6, 6];
      int[,] iMatchAdultWithChildrenUnder16 = new int[6, 6];
      int[,] iMatchAdultWithChildrenUnder5 = new int[6, 6];

      int[,] iMatchAdultMandatory = new int[6, 6];
      int[,] iMatchAdultNonMandatory = new int[6, 6];

      int[,] iMatchMandatory = new int[6, 6];
      int[,] iMatchNonMandatory = new int[6, 6];

      int[,] iMatchAdultMandatoryAndAdultMandatory = new int[6, 6];
      int[,] iMatchAdultNonMandatoryAndAdultNonMandatory = new int[6, 6];

      for (int t2 = 1; t2 <= 5; t2++) {
        for (int t1 = 1; t1 < t2; t1++) {
          // iMatch joints only persons of the same type 
          // lets the base alt, be adult*adult 

          iMatchAgeUnder12[t1, t2] = pAgeUnder12[t1] * pAgeUnder12[t2]; // children under 12
          iMatchAgeUnder16[t1, t2] = pAgeUnder16[t1] * pAgeUnder16[t2]; // children under 16

          iMatchAgeUnder5[t1, t2] = pType8[t1] * pType8[t2]; // children under 5
          iMatchAge5to16[t1, t2] = pType7[t1] * pType7[t2]; // children 5 to 16

          iMatchAdult[t1, t2] = pAdult[t1] * pAdult[t2]; // two adults (very important but difficult to understand)

          iMatchAdultMandatory[t1, t2] = pAdultMandatory[t1] * pAdultMandatory[t2]; // two adults with mandatory travel
          iMatchAdultNonMandatory[t1, t2] = pAdultNonMandatory[t1] * pAdultNonMandatory[t2]; // two adults with non-mandatory travel

          iMatchMandatory[t1, t2] = pMandatory[t1] * pMandatory[t2]; //those with mandatory
          iMatchNonMandatory[t1, t2] = pNonMandatory[t1] * pNonMandatory[t2]; //those tith non-mandatory

          //iMatchAdultWithChildrenUnder16[t1, t2] = pAdultWithChildrenUnder16[t1] * pAdultWithChildrenUnder16[t2];

          iMatchAdultWithChildrenUnder16[t1, t2] = pAdult[t1] * pType7[t2]; //adult plus child 5-16
          iMatchAdultWithChildrenUnder5[t1, t2] = pAdult[t1] * pType8[t2]; //adult plus child5

          iMatchAdultMandatoryAndAdultMandatory[t1, t2] = pAdultMandatory[t1] * pAdultMandatory[t2]; //2 adults with Mandatory
          iMatchAdultNonMandatoryAndAdultNonMandatory[t1, t2] = pAdultNonMandatory[t1] * pAdultNonMandatory[t2]; //2 adults with Mandatory


          //create and populate components
          // OBS! - We apply "Adult * Adult" as the base
          componentIndex++;
          componentMatch[t1, t2] = componentIndex;
          choiceProbabilityCalculator.CreateUtilityComponent(componentMatch[t1, t2]);
          choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(21, iMatchAgeUnder5[t1, t2]);
          choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(22, iMatchAge5to16[t1, t2]);

          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(23, iMatchAdultMandatoryAndAdultMandatory[t1, t2]);
          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(24, iMatchAdultNonMandatoryAndAdultNonMandatory[t1, t2]);

          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(23, iMatchAdult[t1, t2]);
          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(24, iMatchAdultMandatory[t1, t2]);
          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(25, iMatchAdultNonMandatory[t1, t2]);

          // commented out 22nd, but they work well
          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(26, iMatchMandatory[t1, t2]);
          //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(27, iMatchNonMandatory[t1, t2]);


        }
      }

      //create two-way cross interaction utility components

      int[,] componentCross = new int[6, 6];
      int[,] iCrossAgeUnderTwelveAndAdult = new int[6, 6];
      int[,] iCrossAgeUnderTwelveAndAdultNonMandatory = new int[6, 6];
      int[,] iCrossAdultWithChildUnder5 = new int[6, 6];
      int[,] iCrossAdultWithChild5to16 = new int[6, 6];

      int[,] iCrossAdultFemaleWithChildUnder5 = new int[6, 6];
      int[,] iCrossAdultFemaleWithChild5to16 = new int[6, 6];

      int[,] iCrossAdultMandatoryAndAdultNonMandatory = new int[6, 6];
      int[,] iCrossAdultMandatoryAndAdultMandatory = new int[6, 6];
      int[,] iCrossAdultNonMandatoryAndAdultNonMandatory = new int[6, 6];

      int[,] iCrossYouthAndChildUnder5 = new int[6, 6];
      int[,] iCrossChild5to16AndChildUnder5 = new int[6, 6];

      for (int t2 = 1; t2 <= 5; t2++) {
        for (int t1 = 1; t1 <= 5; t1++) {
          if (!(t1 == t2)) {
            //populate cross variables
            // again, one is base, all others are dummies

            iCrossAdultWithChildUnder5[t1, t2] = pAdult[t1] * pType8[t2]; //adult plus child5
            iCrossAdultWithChild5to16[t1, t2] = pAdult[t1] * pType7[t2]; //adult plus child 5-16

            iCrossAdultFemaleWithChildUnder5[t1, t2] = pAdultFemale[t1] * pType8[t2]; //adult mom plus child5
            iCrossAdultFemaleWithChild5to16[t1, t2] = pAdult[t1] * pType7[t2]; //adult mom plus child 5-16


            iCrossAgeUnderTwelveAndAdult[t1, t2] = pAgeUnder12[t1] * pAdult[t2];
            iCrossAgeUnderTwelveAndAdultNonMandatory[t1, t2] = pAgeUnder12[t1] * pAdultNonMandatory[t2];

            iCrossAdultMandatoryAndAdultNonMandatory[t1, t2] = pAdultMandatory[t1] * pAdultNonMandatory[t2];
            iCrossAdultMandatoryAndAdultMandatory[t1, t2] = pAdultMandatory[t1] * pAdultMandatory[t2];
            iCrossAdultNonMandatoryAndAdultNonMandatory[t1, t2] = pAdultNonMandatory[t1] * pAdultNonMandatory[t2];

            iCrossYouthAndChildUnder5[t1, t2] = pYouth[t1] * pType8[t2];
            iCrossChild5to16AndChildUnder5[t1, t2] = pType7[t1] * pType8[t2];

            //create and populate cross components
            // OBS! - We apply "Adult Mandatory * Adult Non-mandatory" as the base
            componentIndex++;
            componentCross[t1, t2] = componentIndex;
            choiceProbabilityCalculator.CreateUtilityComponent(componentCross[t1, t2]);

            choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(41, iCrossAdultWithChildUnder5[t1, t2]);
            choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(42, iCrossAdultWithChild5to16[t1, t2]);

            //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(43, iCrossAdultMandatoryAndAdultNonMandatory[t1, t2]);

            choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(46, iCrossAdultFemaleWithChildUnder5[t1, t2]);

            //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(47, iCrossYouthAndChildUnder5[t1, t2]);
            choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(47, iCrossChild5to16AndChildUnder5[t1, t2]);

            //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(45, iCrossAdultFemaleWithChild5to16[t1, t2]);

            //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(41, iCrossAgeUnderTwelveAndAdult[t1, t2]);
            //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(42, iCrossAgeUnderTwelveAndAdultNonMandatory[t1, t2]);

          }
        }
      }

      //Generate utility funtions for the alternatives
      bool[] available = new bool[32];
      bool[] chosen = new bool[32];

      bool[] threeParticipants = new bool[32];
      bool[] fourPlusParticipants = new bool[32];

      for (int alt = 0; alt < 32; alt++) {

        available[alt] = false;
        chosen[alt] = false;
        // set availability based on household size
        if (hhsize >= altParticipants[alt][6]) {
          available[alt] = true;
        }
        // restrict availability based on person unavailability
        for (int i = 1; i <= 5; i++) {
          if (altParticipants[alt][i] == 1 && (jHTAvailable[genChoice - 1, i] == false || fHTAvailable[i] == false)) {
            available[alt] = false;
          }
        }
        // restrict availability to cases where all non-adult participants have same usual location
        // first determine first non-adult's usual location
        IParcelWrapper sameUsualLocation = householdDay.Household.ResidenceParcel;
        for (int i = 1; i <= 5; i++) {
          if (altParticipants[alt][i] == 1 && pPatternType[i] == 1 && pUsualLocation[i] != null && pUsualLocation[i].Id > 0 && !(pAdult[i] == 1)) {
            sameUsualLocation = pUsualLocation[i];
            break;
          }
        }

        // then make alt unavailable if any M-Usual participant has a different usualLocation 
        for (int i = 1; i <= 5; i++) {
          if (altParticipants[alt][i] == 1 && pHasMandatoryTourToUsualLocation[i] && !(pUsualLocation[i] == sameUsualLocation) && !(pAdult[i] == 1)) {
            available[alt] = false;
            break;
          }
        }

        // restrict availability of alts that include less than 2 participants 
        if (altParticipants[alt][7] < 2) {
          available[alt] = false;
        }

        // restrict availability if 2+ participants lack tour to usual location
        int numberLackMandatoryTourToUsualLocation = 0;
        for (int i = 1; i <= 5; i++) {
          if (altParticipants[alt][i] == 1 && !pHasMandatoryTourToUsualLocation[i]) {
            numberLackMandatoryTourToUsualLocation++;
          }
        }
        if (numberLackMandatoryTourToUsualLocation > 1) {
          available[alt] = false;
        }

        // require at least one driving age (as chauffeur) //GV:out july 2013
        //int numberDrivingAge = 0;
        //for (int i = 1; i <= 5; i++) {
        //    if (altParticipants[alt][i] == 1 && pIsDrivingAge[i]) {
        //        numberDrivingAge++;
        //    }
        //}
        //if (numberDrivingAge == 0) {
        //    available[alt] = false;
        //}


        double tourLogsum;
        int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
        int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
        //var nestedAlternative = ActumWorkTourModeModel.RunNested(pPersonDay, pPersonDay.Household.ResidenceParcel, sameUsualLocation, destinationArrivalTime, destinationDepartureTime, pPersonDay.Household.VehiclesAvailable);
        //var nestedAlternative = (Global.ChoiceModelDictionary.Get("ActumWorkTourModeModel") as ActumWorkTourModeModel).RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(pPersonDay, pPersonDay.Household.ResidenceParcel, sameUsualLocation, destinationArrivalTime, destinationDepartureTime, pPersonDay.Household.VehiclesAvailable);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(pPersonDay, pPersonDay.Household.ResidenceParcel, sameUsualLocation, destinationArrivalTime, destinationDepartureTime, pPersonDay.Household.VehiclesAvailable);
        tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();


        // determine choice
        if (choice == alt) { chosen[alt] = true; }

        //Get the alternative
        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(alt, available[alt], chosen[alt]);

        alternative.Choice = alt;

        //Add utility terms that are not in components
        //alternative.AddUtilityTerm(399, 0);

        // OBS!!! This is new - 21nd January 2013 - it sais that these tris are less expected to be done with 3 or 4+ persons (compared to to people)
        alternative.AddUtilityTerm(59, altParticipants[alt][7] >= 3 ? 1 : 0);
        //alternative.AddUtilityTerm(60, altParticipants[alt][7] >= 4 ? 1 : 0); // OBS! no observations with 4+ HHsize
        alternative.AddUtilityTerm(58, tourLogsum);
        //Add utility components

        for (int p = 1; p <= 5; p++) {
          if (altParticipants[alt][p] == 1) {
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]));
          }
        }
        for (int t2 = 1; t2 <= 5; t2++) {
          for (int t1 = 1; t1 < t2; t1++) {
            if (altParticipants[alt][t1] == 1 && altParticipants[alt][t2] == 1) {
              alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]));

            }
          }
        }
        for (int t2 = 1; t2 <= 5; t2++) {
          for (int t1 = 1; t1 <= 5; t1++) {
            if (!(t1 == t2)) {
              if (altParticipants[alt][t1] == 1 && altParticipants[alt][t2] == 1) {
                alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]));
              }
            }
          }
        }
      }
    }
  }
}
