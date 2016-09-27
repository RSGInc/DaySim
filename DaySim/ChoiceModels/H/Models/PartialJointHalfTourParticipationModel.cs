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
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.H.Models {
	public class PartialJointHalfTourParticipationModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "HPartialJointHalfTourParticipationModel";
		private const int TOTAL_ALTERNATIVES = 32;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 80;
		
		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.PartialJointHalfTourParticipationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public int[] Run(HouseholdDayWrapper householdDay, int jHTSimulated, int genChoice, bool[,] jHTAvailable, bool[] pHTAvailable, bool[,] jHTParticipation) {
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

			var hhsize = householdDay.Household.Size;
			if (Global.Configuration.IsInEstimationMode) {
				int i = 0;
				foreach (PersonDayWrapper personDay in orderedPersonDays) {
					i++;
					if (i <= 5) {
						choice = choice + (jHTParticipation[jHTSimulated, personDay.Person.Sequence] == true ? 1 : 0) * (int) Math.Pow(2, i - 1);
					}
				}
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return altParticipants[choice];
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalBatchIndex.Value].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ jHTSimulated);

			if (_helpers[ParallelUtility.threadLocalBatchIndex.Value].ModelIsInEstimationMode) {

				RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, pHTAvailable, altParticipants, choice);

				choiceProbabilityCalculator.WriteObservation();

				return altParticipants[choice];
			}
			else {
				RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, jHTAvailable, pHTAvailable, altParticipants);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);

                if (chosenAlternative == null)
                {
                    Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", householdDay.Household.Id);
                    if (!Global.Configuration.IsInEstimationMode)
                    {
                        householdDay.IsValid = false;
                    }
                    return null;
                }
                choice = (int)chosenAlternative.Choice;

				return altParticipants[choice];
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int jHTSimulated, int genChoice, bool[,] jHTAvailable, bool[] pHTAvailable, int[][] altParticipants, int choice = Constants.DEFAULT_VALUE) {

			IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
			int batchNumber = ParallelUtility.threadLocalBatchIndex.Value;
			int pairedHalfTour = genChoice == 1 ? 1 : 0;
			int firstHalfTour = genChoice == 2 ? 1 : 0;
			int secondHalfTour = genChoice == 3 ? 1 : 0;

			var income = householdDay.Household.Income < 0 ? Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel : householdDay.Household.Income; // missing converted to 30K
			var incomeMultiple = Math.Min(Math.Max(income / Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel, .1), 10); // ranges for extreme values
			var incomePower = Global.Configuration.Coefficients_CostCoefficientIncomePower_Work;
			var costCoefficient = Global.Coefficients_BaseCostCoefficientPerMonetaryUnit / Math.Pow(incomeMultiple, incomePower);
			var votValue = (60.0 * Global.Configuration.Coefficients_MeanTimeCoefficient_Work) / costCoefficient; // in $/hour

			// set household characteristics here that don't depend on person characteristics

			int hhsize = householdDay.Household.Size;

			int[] pUsualLocationParcelId = new int[6];
			int[] pUsualLocationZoneId = new int[6];
			IParcelWrapper[] pUsualLocationParcel = new IParcelWrapper[6];
			int[] pPatternType = new int[6];
			int[] pConstant = new int[6];
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
			int[] pType7AgeUnder12 = new int[6];
			int[] pType7Age12Plus = new int[6];
			int[] pAgeUnder12 = new int[6];
			int[] pAge5To8 = new int[6];
			int[] pAge9To12 = new int[6];
			int[] pAge13To15 = new int[6];
			int[] pAge = new int[6];
			int[] pDrivingAge = new int[6];

			int count = 0;
			foreach (PersonDayWrapper personDay in orderedPersonDays) {
				count++;
				if (count <= 5) {
					// set characteristics here that depend on person characteristics
					if (personDay.Person.IsFullOrPartTimeWorker && personDay.Person.UsualWorkParcel != null) {
						pUsualLocationParcelId[count] = personDay.Person.UsualWorkParcelId;
						pUsualLocationParcel[count] = personDay.Person.UsualWorkParcel;
						pUsualLocationZoneId[count] = personDay.Person.UsualWorkParcel.ZoneId;
					}
					else if (personDay.Person.IsStudent && personDay.Person.UsualSchoolParcel != null) {
						pUsualLocationParcelId[count] = personDay.Person.UsualSchoolParcelId;
						pUsualLocationParcel[count] = personDay.Person.UsualSchoolParcel;
						pUsualLocationZoneId[count] = personDay.Person.UsualSchoolParcel.ZoneId;
					}
					else if (personDay.Person.IsWorker && personDay.Person.IsNotFullOrPartTimeWorker && personDay.Person.UsualWorkParcel != null) {
						pUsualLocationParcelId[count] = personDay.Person.UsualWorkParcelId;
						pUsualLocationParcel[count] = personDay.Person.UsualWorkParcel;
						pUsualLocationZoneId[count] = personDay.Person.UsualWorkParcel.ZoneId;
					}
					else {
						pUsualLocationParcelId[count] = personDay.Household.ResidenceParcelId;
						pUsualLocationParcel[count] = personDay.Household.ResidenceParcel;
						pUsualLocationZoneId[count] = personDay.Household.ResidenceZoneId;
					}

					pPatternType[count] = personDay.PatternType;
					pConstant[count] = 1;
					pType8[count] = personDay.Person.IsChildUnder5.ToFlag();
					pType7[count] = personDay.Person.IsChildAge5Through15.ToFlag();
					pType6[count] = personDay.Person.IsDrivingAgeStudent.ToFlag();
					pType5[count] = personDay.Person.IsUniversityStudent.ToFlag();
					pType4[count] = personDay.Person.IsNonworkingAdult.ToFlag();
					pType3[count] = personDay.Person.IsRetiredAdult.ToFlag();
					pType2[count] = personDay.Person.IsPartTimeWorker.ToFlag();
					pType1[count] = personDay.Person.IsFulltimeWorker.ToFlag();
					pAdult[count] = personDay.Person.IsAdult.ToFlag();
					pAdultWithChildrenUnder16[count] = (personDay.Person.IsAdult && personDay.Household.HasChildrenUnder16).ToFlag();
					pAdultFemale[count] = personDay.Person.IsAdultFemale.ToFlag();
					pType7AgeUnder12[count] = (personDay.Person.IsChildAge5Through15 && personDay.Person.Age < 12).ToFlag();
					pType7Age12Plus[count] = (personDay.Person.IsChildAge5Through15 && personDay.Person.Age >= 12).ToFlag();
					pAgeUnder12[count] = (personDay.Person.Age < 12).ToFlag();
					pAge5To8[count] = (personDay.Person.Age >= 5 && personDay.Person.Age <= 8).ToFlag();
					pAge9To12[count] = (personDay.Person.Age >= 9 && personDay.Person.Age <= 12).ToFlag();
					pAge13To15[count] = (personDay.Person.Age >= 13 && personDay.Person.Age <= 15).ToFlag();
					pAge[count] = personDay.Person.Age;
					pDrivingAge[count] = personDay.Person.IsDrivingAge.ToFlag();
				}
			}

			// set household characteristics here that do depend on person characteristics
			//bool partTimeWorkerIsAvailable = false;
			//for (int i = 1; i <= 5; i++) {
			//	if (pType2[i] == 1 && pPatternType[i] == Constants.PatternType.MANDATORY && pHTAvailable[i] == true)
			//		partTimeWorkerIsAvailable = true;
			//
			//}

			// set person characteristics here that depend on household characteristics
			//int[] pFullTimeWorkerButPartTimeWorkerIsAvailable = new int[6];
			//for (int i = 1; i <= 5; i++) {
			//	if (pType1[i] == 1 && partTimeWorkerIsAvailable) {
			//		pFullTimeWorkerButPartTimeWorkerIsAvailable[i] = 1;
			//	}
			//}

			var componentIndex = 0;
			//Create person utility components
			int[] componentPerson = new int[6];
			for (var p = 1; p <= 5; p++) {
				// create component for basic person-purposes
				componentIndex++;
				componentPerson[p] = componentIndex;
				choiceProbabilityCalculator.CreateUtilityComponent(componentPerson[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(01, pType1[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(02, pType2[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(03, pType3[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(04, pType4[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(05, pType5[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(06, pType6[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(07, pType7[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(08, pType8[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(09, pAge5To8[p]);
				//choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(10, pAge9To12[p]);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(11, pAdultFemale[p]);
				//choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(12, pType1[p] * firstHalfTour);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(13, pType1[p] * secondHalfTour);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(14, pType2[p] * firstHalfTour);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(15, pType2[p] * secondHalfTour);
				//choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(16, (1 - pAdult[p]) * firstHalfTour);
				choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(17, (1 - pAdult[p]) * secondHalfTour);
				//choiceProbabilityCalculator.GetUtilityComponent(componentPerson[p]).AddUtilityTerm(18, pFullTimeWorkerButPartTimeWorkerIsAvailable[p]);
			}

			//create two-way match interaction utility components
			int[,] componentMatch = new int[6, 6];
			int iMatchAgeUnder12 = 0;
			int iMatchAdult = 0;
			int iMatchAdultWithChildrenUnder16 = 0;

			double distanceInMiles = 0;
			double inverseOfDistanceInMiles = 0;
			double logAgeDiffAdult = 0;
			double logAgeDiffChild = 0;

			for (var t2 = 1; t2 <= 5; t2++) {
				for (var t1 = 1; t1 < t2; t1++) {
					//populate match variables
					iMatchAgeUnder12 = pAgeUnder12[t1] * pAgeUnder12[t2];
					iMatchAdult = pAdult[t1] * pAdult[t2];
					iMatchAdultWithChildrenUnder16 = pAdultWithChildrenUnder16[t1] * pAdultWithChildrenUnder16[t2];

					logAgeDiffChild = Math.Log(1 + Math.Abs(pAge[t1] - pAge[t2])) * (pAge[t1] <= 18).ToFlag() * (pAge[t2] <= 18).ToFlag();
					logAgeDiffAdult = Math.Log(1 + Math.Abs(pAge[t1] - pAge[t2])) * (pAge[t1] >= 18).ToFlag() * (pAge[t2] >= 18).ToFlag();

					distanceInMiles = 0;
					inverseOfDistanceInMiles = 0;
					if (pPatternType[t1] > 0 && pPatternType[t2] > 0) {
						var zzDist = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, Global.Settings.Times.EightAM, pUsualLocationZoneId[t1], pUsualLocationZoneId[t2]).Variable;
						var circuityDistance =
							(zzDist > Global.Configuration.MaximumBlendingDistance)
								? Constants.DEFAULT_VALUE
								: (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Parcel && Global.Configuration.UseShortDistanceNodeToNodeMeasures)
									? pUsualLocationParcel[t1].NodeToNodeDistance(pUsualLocationParcel[t2])
                                    : (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Parcel && Global.Configuration.UseShortDistanceCircuityMeasures)
										? pUsualLocationParcel[t1].CircuityDistance(pUsualLocationParcel[t2])
										: Constants.DEFAULT_VALUE;
						var skimValue =
							Global.Configuration.DestinationScale != Global.Settings.DestinationScales.Parcel
								? ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, Global.Settings.Times.EightAM, pUsualLocationZoneId[t1], pUsualLocationZoneId[t2])
								: ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, votValue, Global.Settings.Times.EightAM, pUsualLocationParcel[t1], pUsualLocationParcel[t2], circuityDistance);
						distanceInMiles = skimValue.BlendVariable;
						inverseOfDistanceInMiles = 1 / (Math.Max(0.1, distanceInMiles));
					}

					//create and populate components
					componentIndex++;
					componentMatch[t1, t2] = componentIndex;
					choiceProbabilityCalculator.CreateUtilityComponent(componentMatch[t1, t2]);
					choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(21, iMatchAdultWithChildrenUnder16);
					choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(22, inverseOfDistanceInMiles);
					choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(23, logAgeDiffChild);
					choiceProbabilityCalculator.GetUtilityComponent(componentMatch[t1, t2]).AddUtilityTerm(24, logAgeDiffAdult);
				}
			}

			//create two-way cross interaction utility components
			int[,] componentCross = new int[6, 6];
			int iCrossAgeUnderTwelveAndAdult = 0;
			//int iCrossAge13To15AndAdultFemale = 0;
			//int iCrossAge9To12AndAdultFemale = 0;
			//int iCrossAge5To8AndAdultFemale = 0;
			//int iCrossAgeUnder5AndAdultFemale = 0;
			//int iCrossAgeUnder5AndFTWorker = 0;
			for (var t2 = 1; t2 <= 5; t2++) {
				for (var t1 = 1; t1 <= 5; t1++) {
					if (!(t1 == t2)) {
						//populate cross variables
						iCrossAgeUnderTwelveAndAdult = pAgeUnder12[t1] * pAdult[t2];
						//iCrossAge13To15AndAdultFemale = pAge13To15[t1] * pAdultFemale[t2];
						//iCrossAge9To12AndAdultFemale = pAge9To12[t1] * pAdultFemale[t2];
						//iCrossAge5To8AndAdultFemale = pAge5To8[t1] * pAdultFemale[t2];
						//iCrossAgeUnder5AndAdultFemale = pType8[t1] * pAdultFemale[t2];
						//iCrossAgeUnder5AndFTWorker = pType8[t1] * pType1[t2];

						//create and populate cross components
						componentIndex++;
						componentCross[t1, t2] = componentIndex;
						choiceProbabilityCalculator.CreateUtilityComponent(componentCross[t1, t2]);
						choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(41, iCrossAgeUnderTwelveAndAdult);
						//choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(42, iCrossAge13To15AndAdultFemale);
						//choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(43, iCrossAge9To12AndAdultFemale);
						//choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(44, iCrossAge5To8AndAdultFemale);
						//choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(45, iCrossAgeUnder5AndAdultFemale);
						//choiceProbabilityCalculator.GetUtilityComponent(componentCross[t1, t2]).AddUtilityTerm(46, iCrossAgeUnder5AndFTWorker);
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
				// restrict availability based on person unavailability
				for (int i = 1; i <= 5; i++) {
					if (altParticipants[alt][i] == 1 && (jHTAvailable[genChoice - 1, i] == false || pHTAvailable[i] == false)) {
						available[alt] = false;
					}
				}
				// restrict availability if nobody is an driving age
				bool altHasDrivingAge = false;
				for (int i = 1; i <= 5; i++) {
					if (altParticipants[alt][i] == 1 && pDrivingAge[i] > 0) {
						altHasDrivingAge = true;
					}
				}
				if (!altHasDrivingAge) {
					available[alt] = false;
				}
				// restrict availability if anybody has nonMandatory pattern type
				bool altHasNonMandatoryPatternType = false;
				for (int i = 1; i <= 5; i++) {
					if (altParticipants[alt][i] == 1 && !(pPatternType[i] == Global.Settings.PatternTypes.Mandatory)) {
						altHasNonMandatoryPatternType = true;
					}
				}
				if (altHasNonMandatoryPatternType) {
					available[alt] = false;
				}
				// restrict availability of alts that include less than 2 participants 
				if (altParticipants[alt][7] < 2) {
					available[alt] = false;
				}

				//Generate alt-specific variables
				int numberOfParticipatingAdults = 0;
				int numberOfParticipatingChildren = 0;
				//bool includesAllMandatoryPatternPersons = true;
				if (available[alt] == true) {
					for (int i = 1; i <= 5; i++) {
						if (altParticipants[alt][i] == 1) {
							//								totalParticipants++;
							if (pAdult[i] == 1) { numberOfParticipatingAdults++; }
							if (pAdult[i] == 0) { numberOfParticipatingChildren++; }
						}
						//else if (pPatternType[i] == Constants.PatternType.MANDATORY && pHTAvailable[i] == true) { includesAllMandatoryPatternPersons = false; }
					}
				}


				// determine choice
				if (choice == alt) { chosen[alt] = true; }

				//Get the alternative
				var alternative = choiceProbabilityCalculator.GetAlternative(alt, available[alt], chosen[alt]);

				alternative.Choice = alt;

				//Add alt-specific utility terms
				alternative.AddUtilityTerm(61, (numberOfParticipatingAdults > 1 && numberOfParticipatingChildren > 0) ? 1 : 0);
				//alternative.AddUtilityTerm(62, includesAllMandatoryPatternPersons.ToFlag());

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
