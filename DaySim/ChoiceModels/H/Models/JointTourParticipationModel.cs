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
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.H.Models {
    public class JointTourParticipationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HJointTourParticipationModel";
        private const int TOTAL_ALTERNATIVES = 32;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 200;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.JointTourParticipationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public int[] Run(HouseholdDayWrapper householdDay, int nCallsForTour, int[] purpose, bool[,] jTParticipation) {
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

            householdDay.ResetRandom(941 + nCallsForTour);

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
            int choice = 0;

            var hhsize = householdDay.Household.Size;
            if (Global.Configuration.IsInEstimationMode) {
                int i = 0;
                foreach (PersonDayWrapper personDay in orderedPersonDays) {
                    i++;
                    if (i <= 5) {
                        choice = choice + (jTParticipation[nCallsForTour, personDay.Person.Sequence] == true ? 1 : 0) * (int)Math.Pow(2, i - 1);
                    }
                }
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return altParticipants[choice];
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ nCallsForTour);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

                RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour, purpose, altParticipants, choice);

                choiceProbabilityCalculator.WriteObservation();

                return altParticipants[choice];
            } else {
                RunModel(choiceProbabilityCalculator, householdDay, nCallsForTour, purpose, altParticipants);

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

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int nCallsForTour, int[] purpose, int[][] altParticipants, int choice = Constants.DEFAULT_VALUE) {

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
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

            int tourPurpose = purpose[nCallsForTour];

            int escortPurpose = purpose[nCallsForTour] == Global.Settings.Purposes.Escort ? 1 : 0;
            int shopPurpose = purpose[nCallsForTour] == Global.Settings.Purposes.Shopping ? 1 : 0;
            int socialPurpose = (purpose[nCallsForTour] == Global.Settings.Purposes.Social ? 1 : 0);
            int recreationPurpose = (purpose[nCallsForTour] == Global.Settings.Purposes.Recreation) ? 1 : 0;
            int personalBusinessMedicalPurpose = (purpose[nCallsForTour] == Global.Settings.Purposes.PersonalBusiness
                                                         || purpose[nCallsForTour] == Global.Settings.Purposes.Medical) ? 1 : 0;
            int meal = (purpose[nCallsForTour] == Global.Settings.Purposes.Meal) ? 1 : 0;
            int socialRecreationPurpose = (purpose[nCallsForTour] == Global.Settings.Purposes.Social ||
                                                    purpose[nCallsForTour] == Global.Settings.Purposes.Recreation) ? 1 : 0;
            //nt recreationPurpose =	(purpose[nCallsForTour] == Global.Settings.Purposes.Recreation)	 ? 1 : 0;

            var votALSegment = householdDay.Household.GetVotALSegment();
            var transitAccessSegment = householdDay.Household.ResidenceParcel.TransitAccessSegment();
            var totalAggregateLogsum = Global.AggregateLogsums[householdDay.Household.ResidenceParcel.ZoneId]
                                                [Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];

            PersonDayWrapper pPersonDay = null;

            bool[] pLessThan3NonMandatoryTours = new bool[6];
            bool[] pLessThan3TourPurposes = new bool[6];
            int[] pUsualLocationParcelId = new int[6];
            int[] pUsualLocationZoneId = new int[6];
            IParcelWrapper[] pUsualLocationParcel = new IParcelWrapper[6];
            int[] pPatternType = new int[6];
            int[] pConstant = new int[6];
            int[,] pType = new int[9, 6];
            int[] pAdult = new int[6];
            int[] pChild = new int[6];
            int[] pAge = new int[6];


            //int[] pAdultMale = new int[6];
            int[] pAdultFemale = new int[6];
            int[] pAdultMandatory = new int[6];
            int[] pNonMandatory = new int[6];
            int[] pMandatory = new int[6];
            int[] pAdultNonMandatory = new int[6];
            int[] pAdultWithChildrenUnder16 = new int[6];

            int[] pAge5To8 = new int[6];
            int[] pAge9To12 = new int[6];
            int[] pAge13To15 = new int[6];
            int[] pAgeUnder13 = new int[6];

            int[] pTransit = new int[6];

            //int[] pJointNonMandatoryTours = new int[6];
            int[] pJointEscortTours = new int[6];
            int[] pMandatoryTours = new int[6];
            int[] pJointSocialRecreationTours = new int[6];
            //int[] pJointPersMedtours = new int[6];
            int[] pNonMandatoryChild = new int[6];
            int[] pMandatoryChild = new int[6];
            //int[] pJointShopTours = new int[6];
            int[] pJointMealTours = new int[6];

            int count = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count++;
                if (count <= 5) {
                    if (count == 1) {
                        pPersonDay = personDay;
                    }
                    for (int i = 1; i < 9; i++) {
                        pType[personDay.Person.PersonType, count] = 1;
                    }

                    if (personDay.Person.IsFullOrPartTimeWorker && personDay.Person.UsualWorkParcel != null) {
                        pUsualLocationParcelId[count] = personDay.Person.UsualWorkParcelId;
                        pUsualLocationParcel[count] = personDay.Person.UsualWorkParcel;
                        pUsualLocationZoneId[count] = personDay.Person.UsualWorkParcel.ZoneId;
                    } else if (personDay.Person.IsStudent && personDay.Person.UsualSchoolParcel != null) {
                        pUsualLocationParcelId[count] = personDay.Person.UsualSchoolParcelId;
                        pUsualLocationParcel[count] = personDay.Person.UsualSchoolParcel;
                        pUsualLocationZoneId[count] = personDay.Person.UsualSchoolParcel.ZoneId;
                    } else if (personDay.Person.IsWorker && personDay.Person.IsNotFullOrPartTimeWorker && personDay.Person.UsualWorkParcel != null) {
                        pUsualLocationParcelId[count] = personDay.Person.UsualWorkParcelId;
                        pUsualLocationParcel[count] = personDay.Person.UsualWorkParcel;
                        pUsualLocationZoneId[count] = personDay.Person.UsualWorkParcel.ZoneId;
                    } else {
                        pUsualLocationParcelId[count] = personDay.Household.ResidenceParcelId;
                        pUsualLocationParcel[count] = personDay.Household.ResidenceParcel;
                        pUsualLocationZoneId[count] = personDay.Household.ResidenceZoneId;
                    }

                    pLessThan3NonMandatoryTours[count] = personDay.GetCreatedNonMandatoryTours() < 3;
                    pLessThan3TourPurposes[count] = personDay.GetTotalCreatedTourPurposes() < 3;
                    pPatternType[count] = personDay.PatternType;
                    pConstant[count] = 1;
                    pAdult[count] = personDay.Person.IsAdult.ToFlag();
                    pChild[count] = (!(personDay.Person.IsAdult)).ToFlag();
                    pAdultWithChildrenUnder16[count] = (personDay.Person.IsAdult && personDay.Household.HasChildrenUnder16).ToFlag();
                    pAdultFemale[count] = personDay.Person.IsAdultFemale.ToFlag();
                    //pAdultMale[count] = personDay.Person.IsAdultMale.ToFlag();
                    pAdultMandatory[count] = personDay.Person.IsAdult.ToFlag() * (personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
                    pNonMandatory[count] = (personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag();
                    pAdultNonMandatory[count] = (personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag() * personDay.Person.IsAdult.ToFlag();
                    pAgeUnder13[count] = (personDay.Person.Age < 13).ToFlag();
                    pAge5To8[count] = (personDay.Person.Age >= 5 && personDay.Person.Age <= 8).ToFlag();
                    //pAge9To12[count] = (personDay.Person.Age >= 9 && personDay.Person.Age <= 12).ToFlag();
                    pAge13To15[count] = (personDay.Person.Age >= 13 && personDay.Person.Age <= 15).ToFlag();
                    pTransit[count] = (personDay.Person.TransitPassOwnership == 1 ? 1 : 0);
                    pMandatory[count] = (personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag();
                    pMandatoryTours[count] = personDay.WorkTours + personDay.SchoolTours;
                    pJointEscortTours[count] = personDay.CreatedEscortTours;
                    pJointMealTours[count] = personDay.CreatedMealTours;
                    //pJointShopTours[count]= personDay.CreatedShoppingTours;
                    pJointSocialRecreationTours[count] = personDay.CreatedSocialTours + personDay.CreatedRecreationTours;
                    pNonMandatoryChild[count] = (personDay.PatternType == Global.Settings.PatternTypes.Optional).ToFlag() * personDay.Person.IsChildUnder16.ToFlag();
                    pMandatoryChild[count] = (personDay.PatternType == Global.Settings.PatternTypes.Mandatory).ToFlag() * personDay.Person.IsChildUnder16.ToFlag();


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

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(01, pAge5To8[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(02, pAdultFemale[p]);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(03, pAge9To12[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(05, (pMandatoryTours[p] > 1).ToFlag());
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(06, (pJointNonMandatoryTours[p]>1).ToFlag());
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(07, pAdultMandatory[p]);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(08, pTransit[p]);

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(21, pAdult[p] * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(26, pType[6, p] * pNonMandatory[p] * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(28, pType[8, p] * pNonMandatory[p] * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(29, pType[6, p] * pMandatory[p] * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(30, pType[7, p] * pMandatory[p] * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(31, pType[8, p] * pMandatory[p] * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(32, (pJointEscortTours[p] == 1).ToFlag() * escortPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(33, (pJointEscortTours[p] >= 2).ToFlag() * escortPurpose);

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(41, pAdult[p] * personalBusinessMedicalPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(44, pNonMandatoryChild[p] * personalBusinessMedicalPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(45, pMandatoryChild[p] * personalBusinessMedicalPurpose);

                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(51, pAdultMandatory[p]*  shopPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(52, pAdultNonMandatory[p] * shopPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(54, pNonMandatoryChild[p] * shopPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(56, pType[6, p] * pMandatory[p] * shopPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(57, pType[7, p] * pMandatory[p] * shopPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(58, pType[8, p] * pMandatory[p] * shopPurpose);
                //choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(59, (pJointShopTours[p]>1).ToFlag() *  shopPurpose);

                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(71, pAdult[p] * meal);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(77, (pJointMealTours[p] > 0).ToFlag() * meal);


                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(92, pAdult[p] * socialRecreationPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(96, pType[6, p] * pMandatory[p] * socialRecreationPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(97, pType[7, p] * pMandatory[p] * socialRecreationPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(98, pType[8, p] * pMandatory[p] * socialRecreationPurpose);
                choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(99, (pJointSocialRecreationTours[p] >= 1).ToFlag() * socialRecreationPurpose);
            }

            //create two-way match interaction utility components
            int[,] componentMatch = new int[6, 6];
            int[,] iMatchAdult = new int[6, 6];
            int[,] iMatchAdultWithChildrenUnder16 = new int[6, 6];

            int[,] iMatchMandatoryAdults = new int[6, 6];
            int[,] iMatchNonMandatoryAdults = new int[6, 6];
            int[,] iMatchChild = new int[6, 6];
            int[,] iMatchNonMandatoryKids = new int[6, 6];
            int[,] iMatchMandatoryKids = new int[6, 6];
            int[,] iMatchNonMandatory = new int[6, 6];
            int[,] iMatchUsualLocation = new int[6, 6];

            for (var t2 = 1; t2 <= 5; t2++) {
                for (var t1 = 1; t1 < t2; t1++) {
                    //populate match variables
                    iMatchAdultWithChildrenUnder16[t1, t2] = pAdultWithChildrenUnder16[t1] * pAdultWithChildrenUnder16[t2];
                    iMatchNonMandatoryAdults[t1, t2] = pAdultNonMandatory[t1] * pAdultNonMandatory[t2];
                    iMatchMandatoryAdults[t1, t2] = pAdultMandatory[t1] * pAdultMandatory[t2];

                    iMatchNonMandatory[t1, t2] = pNonMandatory[t1] * pNonMandatory[t2];
                    iMatchUsualLocation[t1, t2] = (pUsualLocationParcelId[t1] == pUsualLocationParcelId[t2]) ? 1 : 0;

                    iMatchChild[t1, t2] = (1 - pAdult[t1]) * (1 - pAdult[t2]);
                    iMatchNonMandatoryKids[t1, t2] = pNonMandatory[t1] * pAgeUnder13[t1] * pNonMandatory[t2] * pAgeUnder13[t2];
                    iMatchMandatoryKids[t1, t2] = pMandatory[t1] * pAgeUnder13[t1] * pMandatory[t2] * pAgeUnder13[t2];

                    //create and populate components
                    componentIndex++;
                    componentMatch[t1, t2] = componentIndex;
                    choiceProbabilityCalculator.CreateUtilityComponent(componentMatch[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(109, iMatchNonMandatoryKids[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(110, iMatchMandatoryKids[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(111, iMatchMandatoryAdults[t1, t2]);
                    //choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(113, iMatchAdultNonMandatorys[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(114, iMatchAdultWithChildrenUnder16[t1, t2]);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(116, iMatchNonMandatory[t1, t2] * socialRecreationPurpose);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(124, iMatchChild[t1, t2] * shopPurpose);
                    choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(125, iMatchUsualLocation[t1, t2]);

                }
            }

            //create two-way cross interaction utility components
            int[,] componentCross = new int[6, 6];
            int iCrossAgeUnder5AndNonMandatory;
            int iCrossAge5To12AndNonMandatory;
            int iCrossAge13To15AndNonMandatory;
            int iCrossDrivingAgeChildAndNonMandatory;
            int iCrossMandatoryAdAndChild;
            int iCrossMandatoryAdAndNonMandAd;

            for (var t2 = 1; t2 <= 5; t2++) {
                for (var t1 = 1; t1 <= 5; t1++) {
                    if (!(t1 == t2)) {
                        //populate cross variables
                        iCrossAge5To12AndNonMandatory = pAdultNonMandatory[t1] * (pAge5To8[t2] + pAge9To12[t2]);
                        iCrossAgeUnder5AndNonMandatory = pAdultNonMandatory[t1] * pType[8, t2];
                        iCrossAge13To15AndNonMandatory = pNonMandatory[t1] * pAge13To15[t2];
                        iCrossDrivingAgeChildAndNonMandatory = pNonMandatory[t1] * pType[6, t2];
                        iCrossMandatoryAdAndChild = pAdultMandatory[t1] * pChild[t2];
                        //create and populate cross components
                        componentIndex++;
                        componentCross[t1, t2] = componentIndex;
                        choiceProbabilityCalculator.CreateUtilityComponent(componentCross[t1, t2]);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(133, iCrossAge13To15AndNonMandatory);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(135, iCrossAgeUnder5AndNonMandatory * escortPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(136, iCrossAgeUnder5AndNonMandatory * personalBusinessMedicalPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(137, iCrossAgeUnder5AndNonMandatory * shopPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(138, iCrossAgeUnder5AndNonMandatory * meal);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(139, iCrossAgeUnder5AndNonMandatory * socialRecreationPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(144, iCrossAge5To12AndNonMandatory * socialRecreationPurpose);
                        //choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(145, iCrossMandatoryAdAndChild*escortPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(146, iCrossMandatoryAdAndChild * personalBusinessMedicalPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(147, iCrossMandatoryAdAndChild * shopPurpose);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(148, iCrossMandatoryAdAndChild * meal);
                        choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(149, iCrossMandatoryAdAndChild * socialRecreationPurpose);

                    }
                }
            }

            //Generate utility funtions for the alternatives
            bool[] available = new bool[32];
            bool[] chosen = new bool[32];
            for (int alt = 0; alt < 32; alt++) {

                available[alt] = false;
                chosen[alt] = false;
                // set availability based on household size
                if (hhsize >= altParticipants[alt][6]) {
                    available[alt] = true;
                }
                // restrict availability of alts that include less than 2 participants 
                if (altParticipants[alt][7] < 2) {
                    available[alt] = false;
                }
                // restrict availability if any participant has at-home patternType
                int numberAtHomePatternTypes = 0;
                for (int i = 1; i <= 5; i++) {
                    if (altParticipants[alt][i] == 1 && pPatternType[i] == Global.Settings.PatternTypes.Home) {
                        numberAtHomePatternTypes++;
                    }
                }
                if (numberAtHomePatternTypes > 0) {
                    available[alt] = false;
                }
                // restrict availability if any participant has 3 or more nonmandatory tours already
                for (int i = 1; i <= 5; i++) {
                    if (altParticipants[alt][i] == 1 && !pLessThan3NonMandatoryTours[i]) {
                        available[alt] = false;
                    }
                }

                // restrict availability if any participant has 3 or more tour purposes already
                for (int i = 1; i <= 5; i++) {
                    if (altParticipants[alt][i] == 1 && !pLessThan3TourPurposes[i]) {
                        available[alt] = false;
                    }
                }

                int numberOfParticipants = 0;
                int workersParticipating = 0;
                int numberOfParticipatingChildren = 0;
                int numberOfParticipatingAdults = 0;
                int numberOfAvailableChildren = 0;
                int numberOfAvailablePersons = 0;
                int numberOfAvailableAdults = 0;

                if (available[alt] == true) {
                    for (int i = 1; i <= 5; i++) {
                        if (pChild[i] == 1) {
                            numberOfAvailableChildren++;
                            numberOfAvailablePersons++;
                        }
                        if (pAdult[i] == 1) {
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

                // determine choice
                if (choice == alt) { chosen[alt] = true; }

                //Get the alternative
                var alternative = choiceProbabilityCalculator.GetAlternative(alt, available[alt], chosen[alt]);

                alternative.Choice = alt;

                //Add alt-specific utility terms
                alternative.AddUtilityTerm(160, (numberOfParticipatingAdults > 1 && numberOfParticipatingChildren > 0) ? 1 : 0);
                //alternative.AddUtilityTerm(161, (numberOfParticipatingChildren < numberOfAvailableChildren) ? 1 : 0);
                //alternative.AddUtilityTerm(162, (numberOfParticipatingAdults >1).ToFlag() *socialRecreationPurpose);
                //alternative.AddUtilityTerm(163, (numberOfParticipatingAdults == 1).ToFlag() *personalBusinessMedicalPurpose);

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
