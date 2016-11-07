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
    public class FullJointHalfTourParticipationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HFullJointHalfTourParticipationModel";
        private const int TOTAL_ALTERNATIVES = 32;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 120;

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

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ jHTSimulated);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

                RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, fHTAvailable, altParticipants, choice);

                choiceProbabilityCalculator.WriteObservation();

                return altParticipants[choice];
            } else {
                RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, fHTAvailable, altParticipants);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);

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

            int pairedHalfTour = genChoice == 1 ? 1 : 0;
            int firstHalfTour = genChoice == 2 ? 1 : 0;
            int secondHalfTour = genChoice == 3 ? 1 : 0;

            // set household characteristics here that don't depend on person characteristics

            var carOwnership =
                        householdDay.Household.VehiclesAvailable == 0
                             ? Global.Settings.CarOwnerships.NoCars
                             : householdDay.Household.VehiclesAvailable < householdDay.Household.HouseholdTotals.DrivingAgeMembers
                                  ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                  : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;
            int hhsize = householdDay.Household.Size;
            var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

            var votALSegment = householdDay.Household.GetVotALSegment();
            var transitAccessSegment = householdDay.Household.ResidenceParcel.TransitAccessSegment();
            var totalAggregateLogsum = Global.AggregateLogsums[householdDay.Household.ResidenceParcel.ZoneId]
                                                [Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];

            IPersonDayWrapper pPersonDay = null;

            IParcelWrapper[] pUsualLocation = new IParcelWrapper[6];
            int[] pUsualLocationId = new int[6];
            int[] pPatternType = new int[6];
            int[] pConstant = new int[6];
            int[,] pType = new int[9, 6];
            int[] pAdult = new int[6];
            int[] pChild = new int[6];
            int[] pAge = new int[6];


            int[] pAdultFemale = new int[6];
            int[] pAdultMandatory = new int[6];
            int[] pNonMandatory = new int[6];
            int[] pFemaleNonMandatory = new int[6];

            int[] pAdultWithChildrenUnder16 = new int[6];

            int[] pType7AgeUnder12 = new int[6];
            int[] pType7Age12Plus = new int[6];
            int[] pAgeUnder13 = new int[6];
            int[] pAge5To8 = new int[6];
            int[] pAge9To12 = new int[6];
            int[] pAge13To15 = new int[6];

            int[] pTransitAdult = new int[6];
            int[] pTransitChild = new int[6];
            int[] pDrivingAge = new int[6];
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
                    if (personDay.Person.IsFullOrPartTimeWorker && personDay.Person.UsualWorkParcel != null) {
                        pUsualLocation[count] = personDay.Person.UsualWorkParcel;
                    } else if (personDay.Person.IsStudent && personDay.Person.UsualSchoolParcel != null) {
                        pUsualLocation[count] = personDay.Person.UsualSchoolParcel;
                    } else if (personDay.Person.IsWorker && personDay.Person.IsNotFullOrPartTimeWorker && personDay.Person.UsualWorkParcel != null) {
                        pUsualLocation[count] = personDay.Person.UsualWorkParcel;
                    } else {
                        pUsualLocation[count] = personDay.Household.ResidenceParcel;
                    }

                    for (int i = 1; i < 9; i++) {
                        pType[personDay.Person.PersonType, count] = 1;
                    }

                    pPatternType[count] = personDay.PatternType;
                    pConstant[count] = 1;
                    pAdult[count] = personDay.Person.IsAdult.ToFlag();
                    pChild[count] = (!(personDay.Person.IsAdult)).ToFlag();
                    pAdultWithChildrenUnder16[count] = (personDay.Person.IsAdult && personDay.Household.HasChildrenUnder16).ToFlag();
                    pAdultFemale[count] = personDay.Person.IsAdultFemale.ToFlag();
                    pAdultMandatory[count] = personDay.Person.IsAdult.ToFlag() * (personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
                    pNonMandatory[count] = (personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();
                    pFemaleNonMandatory[count] = personDay.Person.IsAdultFemale.ToFlag() * (personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();
                    pType7AgeUnder12[count] = (personDay.Person.IsChildAge5Through15 && personDay.Person.Age < 12).ToFlag();
                    pType7Age12Plus[count] = (personDay.Person.IsChildAge5Through15 && personDay.Person.Age >= 12).ToFlag();
                    pAgeUnder13[count] = (personDay.Person.Age < 13).ToFlag();
                    pAge5To8[count] = (personDay.Person.Age >= 5 && personDay.Person.Age <= 8).ToFlag();
                    pAge9To12[count] = (personDay.Person.Age >= 9 && personDay.Person.Age <= 12).ToFlag();
                    pAge13To15[count] = (personDay.Person.Age >= 13 && personDay.Person.Age <= 15).ToFlag();
                    pTransitAdult[count] = (personDay.Person.TransitPassOwnership == 1 ? 1 : 0) * (personDay.Person.IsAdult.ToFlag());
                    pTransitChild[count] = (personDay.Person.TransitPassOwnership == 1 ? 1 : 0) * ((!personDay.Person.IsAdult).ToFlag());
                    pDrivingAge[count] = personDay.Person.IsDrivingAge.ToFlag();

                    pHasMandatoryTourToUsualLocation[count] = personDay.HasMandatoryTourToUsualLocation;
                    pIsDrivingAge[count] = personDay.Person.IsDrivingAge;
                }
            }
            var componentIndex = 0;
            //Create person utility components
            int[] componentPerson = new int[6];
            for (var p = 1; p <= 5; p++) {
                // create component for basic person-purposes
                componentIndex++;
                componentPerson[p] = componentIndex;
                choiceProbabilityCalculator.CreateUtilityComponent(componentPerson[p]);

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(01, pType[1, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(02, pType[2, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(03, pType[3, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(04, pType[4, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(05, pType[5, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(06, pType[6, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(07, pType[7, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(08, pType[8, p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(09, pAge5To8[p]);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(10, pAge9To12[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(11, pAdultFemale[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(12, pNonMandatory[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(13, pFemaleNonMandatory[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(14, pTransitAdult[p]);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(15, pTransitChild[p]);

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(21, pType[1, p] * secondHalfTour);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(22, pType[2, p] * secondHalfTour);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(23, pType[3, p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(24, pType[4, p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(25, pType[5, p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(26, pType[6, p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(27, pType[7, p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(28, pType[8, p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(29, pAge5To8[p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(30, pAge9To12[p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(31, pAdultFemale[p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(32, pNonMandatory[p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(33, pFemaleNonMandatory[p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(34, pTransitAdult[p] * secondHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(35, pTransitChild[p] * secondHalfTour);

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(41, pType[1, p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(42, pType[2, p] * pairedHalfTour);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(43, pType[3, p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(44, pType[4, p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(45, pType[5, p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(46, pType[6, p] * pairedHalfTour);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(47, pType[7, p] * pairedHalfTour);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(48, pType[8, p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(49, pAge5To8[p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(50, pAge9To12[p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(51, pAdultFemale[p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(52, pNonMandatory[p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(53, pFemaleNonMandatory[p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(54, pTransitAdult[p] * pairedHalfTour);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(55, pTransitChild[p] * pairedHalfTour);


            }

            //create two-way match interaction utility components
            int[,] componentMatch = new int[6, 6];
            int[,] iMatchAgeUnder13 = new int[6, 6];
            int[,] iMatchAdult = new int[6, 6];
            int[,] iMatchAdultWithChildrenUnder16 = new int[6, 6];

            int[,] iMatchMandatoryAdults = new int[6, 6];
            int[,] iMatchChild = new int[6, 6];
            int[,] iMatchNonMandatory = new int[6, 6];
            int[,] iMatchAdultsCarDeficit = new int[6, 6];
            int[,] iMatchAdultCountAtHome = new int[6, 6];


            for (var t2 = 1; t2 <= 5; t2++) {
                for (var t1 = 1; t1 < t2; t1++) {
                    //populate match variables
                    iMatchAgeUnder13[t1, t2] = pAgeUnder13[t1] * pAgeUnder13[t2];
                    iMatchAdult[t1, t2] = pAdult[t1] * pAdult[t2];
                    iMatchAdultWithChildrenUnder16[t1, t2] = pAdultWithChildrenUnder16[t1] * pAdultWithChildrenUnder16[t2];

                    iMatchMandatoryAdults[t1, t2] = pAdultMandatory[t1] * pAdultMandatory[t2];
                    iMatchChild[t1, t2] = (1 - pAdult[t1]) * (1 - pAdult[t2]);
                    iMatchNonMandatory[t1, t2] = pNonMandatory[t1] * pNonMandatory[t2];
                    iMatchAdultsCarDeficit[t1, t2] = pAdult[t1] * pAdult[t2] * (carCompetitionFlag + noCarsFlag);


                    //create and populate components
                    componentIndex++;
                    componentMatch[t1, t2] = componentIndex;
                    choiceProbabilityCalculator.CreateUtilityComponent(componentMatch[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(61, iMatchAgeUnder13[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(62, iMatchAdult[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(63, iMatchMandatoryAdults[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(64, iMatchNonMandatory[t1, t2]);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(65, iMatchAdultWithChildrenUnder16[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(66, iMatchChild[t1, t2]);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(67, iMatchAdultsCarDeficit[t1, t2]);

                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(71, iMatchAgeUnder13[t1, t2] * secondHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(72, iMatchAdult[t1, t2] * secondHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(73, iMatchMandatoryAdults[t1, t2] * secondHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(74, iMatchNonMandatory[t1, t2] * secondHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(75, iMatchAdultWithChildrenUnder16[t1, t2] * secondHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(76, iMatchChild[t1, t2] * secondHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(77, iMatchAdultsCarDeficit[t1, t2] * secondHalfTour);

                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(81, iMatchAgeUnder13[t1, t2] * pairedHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(82, iMatchAdult[t1, t2] * pairedHalfTour);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(83, iMatchMandatoryAdults[t1, t2] * pairedHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(84, iMatchNonMandatory[t1, t2] * pairedHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(85, iMatchAdultWithChildrenUnder16[t1, t2] * pairedHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(86, iMatchChild[t1, t2] * pairedHalfTour);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(87, iMatchAdultsCarDeficit[t1, t2] * pairedHalfTour);

                }
            }

            //create two-way cross interaction utility components
            int[,] componentCross = new int[6, 6];
            int iCrossAgeUnder5AndNonMandatory;
            int iCrossAge5To12AndNonMandatory;
            int iCrossAge13To15AndNonMandatory;
            int iCrossDrivingAgeChildAndNonMandatory;

            for (var t2 = 1; t2 <= 5; t2++) {
                for (var t1 = 1; t1 <= 5; t1++) {
                    if (!(t1 == t2)) {
                        //populate cross variables
                        iCrossAge5To12AndNonMandatory = pNonMandatory[t1] * (pAge5To8[t2] + pAge9To12[t2]);
                        iCrossAgeUnder5AndNonMandatory = pNonMandatory[t1] * pType[8, t2];
                        iCrossAge13To15AndNonMandatory = pNonMandatory[t1] * pAge13To15[t2];
                        iCrossDrivingAgeChildAndNonMandatory = pNonMandatory[t1] * pType[6, t2];

                        //create and populate cross components
                        componentIndex++;
                        componentCross[t1, t2] = componentIndex;
                        choiceProbabilityCalculator.CreateUtilityComponent(componentCross[t1, t2]);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(91, iCrossAgeUnder5AndNonMandatory);
                        //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(92, iCrossAge5To12AndNonMandatory);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(93, iCrossAge13To15AndNonMandatory);
                        //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(94, iCrossDrivingAgeChildAndNonMandatory);

                        //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(95, iCrossAgeUnder5AndNonMandatory * secondHalfTour);
                        //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(96, iCrossAgeUnder5AndNonMandatory * pairedHalfTour);
                    }
                }
            }

            //Generate utility funtions for the alternatives
            bool[] available = new bool[32];
            bool[] chosen = new bool[32];
            int numberOfParticipants;
            int workersParticipating;
            int numberOfParticipatingChildren;
            int numberOfParticipatingAdults;
            int numberOfAvailableChildren;
            int numberOfAvailablePersons;
            int numberOfAvailableAdults;

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
                // restrict availability to cases where all non-driving age participants have same usual location
                // first determine first non-adult's usual location
                IParcelWrapper sameUsualLocation = householdDay.Household.ResidenceParcel;
                for (int i = 1; i <= 5; i++) {
                    if (altParticipants[alt][i] == 1 && pPatternType[i] == 1 && pUsualLocation[i].Id > 0 && !(pDrivingAge[i] == 1)) {
                        sameUsualLocation = pUsualLocation[i];
                        break;
                    }
                }
                // then make alt unavailable if any M-Usual participant has a different usualLocation 
                for (int i = 1; i <= 5; i++) {
                    if (altParticipants[alt][i] == 1 && pHasMandatoryTourToUsualLocation[i] && !(pUsualLocation[i].Id == sameUsualLocation.Id) && !(pDrivingAge[i] == 1)) {
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

                //JLB 20140404 remove the drivign age requirement because children can walk, bik or take bus to/from school without an adult.
                // require at least one driving age (as chauffeur)
                //int numberDrivingAge = 0;
                //for (int i = 1; i <= 5; i++) {
                //    if (altParticipants[alt][i] == 1 && pIsDrivingAge[i]) {
                //        numberDrivingAge++;
                //    }
                //}
                //if (numberDrivingAge == 0) {
                //    available[alt] = false;    
                //}


                // Generate alt-specific variables
                numberOfParticipants = 0;
                workersParticipating = 0;
                numberOfParticipatingChildren = 0;
                numberOfParticipatingAdults = 0;
                numberOfAvailableChildren = 0;
                numberOfAvailablePersons = 0;
                numberOfAvailableAdults = 0;
                if (available[alt] == true) {
                    for (int i = 1; i <= 5; i++) {
                        if (pChild[i] == 1 && pUsualLocation[i].Id == sameUsualLocation.Id) {
                            numberOfAvailableChildren++;
                            numberOfAvailablePersons++;
                        }
                        if (pAdult[i] == 1 && (pPatternType[i] == Global.Settings.PatternTypes.Optional || pUsualLocation[i].Id == sameUsualLocation.Id)) {
                            numberOfAvailableChildren++;
                            numberOfAvailableAdults++;
                        }

                        if (altParticipants[alt][i] == 1) {
                            numberOfParticipants++;
                            if (pType[0, i] == 1 || pType[1, i] == 1) {
                                workersParticipating++;
                            }
                            if (pAdult[i] == 1) { numberOfParticipatingAdults++; }
                            if (pChild[i] == 1) { numberOfParticipatingChildren++; }
                        }
                    }
                }

                double tourLogsum = 0;
                var destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
                var destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
                var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(pPersonDay, pPersonDay.Household.ResidenceParcel, sameUsualLocation, destinationArrivalTime, destinationDepartureTime, pPersonDay.Household.VehiclesAvailable);
                tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();

                // determine choice
                if (choice == alt) { chosen[alt] = true; }

                //Get the alternative
                var alternative = choiceProbabilityCalculator.GetAlternative(alt, available[alt], chosen[alt]);

                alternative.Choice = alt;

                //Add alt-specific utility terms
                alternative.AddUtilityTerm(101, (numberOfParticipatingAdults > 1 && numberOfParticipatingChildren > 0) ? 1 : 0);
                alternative.AddUtilityTerm(102, (numberOfParticipatingChildren < numberOfAvailableChildren) ? 1 : 0);
                alternative.AddUtilityTerm(103, (numberOfParticipatingAdults == 0) ? 1 : 0);
                //alternative.AddUtilityTerm(104, (numberOfParticipants < numberOfAvailablePersons) ? 1 : 0);
                alternative.AddUtilityTerm(105, tourLogsum);

                //alternative.AddUtilityTerm(105, ((numberOfParticipatingAdults > 1 && numberOfParticipatingChildren > 0) ? 1 : 0) * secondHalfTour);
                //alternative.AddUtilityTerm(106, ((numberOfParticipatingAdults > 1 && numberOfParticipatingChildren > 0) ? 1 : 0) * pairedHalfTour);



                //Add utility components

                for (int p = 1; p <= 5; p++) {
                    if (altParticipants[alt][p] == 1) {
                        alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]));
                    }
                }
                for (var t2 = 1; t2 <= 5; t2++) {
                    for (var t1 = 1; t1 < t2; t1++) {
                        if (altParticipants[alt][t1] == 1 && altParticipants[alt][t2] == 1) {
                            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]));

                        }
                    }
                }
                for (var t2 = 1; t2 <= 5; t2++) {
                    for (var t1 = 1; t1 <= 5; t1++) {
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
