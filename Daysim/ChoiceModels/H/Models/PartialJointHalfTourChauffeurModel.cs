// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using Daysim.DomainModels;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;

namespace Daysim.ChoiceModels.H.Models {
	public class PartialJointHalfTourChauffeurModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "HPartialJointHalfTourChauffeurModel";
		private const int TOTAL_ALTERNATIVES = 5;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 60;

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.PartialJointHalfTourChauffeurModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public int Run(HouseholdDayWrapper householdDay, int jHTSimulated, int genChoice, int[] participants, int[] jHTChauffeurSequence) {
			if (householdDay == null) {
				throw new ArgumentNullException("householdDay");
			}
			
			householdDay.ResetRandom(930 + jHTSimulated);

			IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

			int chauffeurSequence = 0;
			int choice = 0;
			if (Global.Configuration.IsInEstimationMode) {
				chauffeurSequence = jHTChauffeurSequence[jHTSimulated];
				int i = 0;
				foreach (PersonDayWrapper personDay in orderedPersonDays) {
					i++;
					if (personDay.Person.Sequence == chauffeurSequence && i <= 5) {
						choice = i;
					}
				}
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME || choice == 0) {
					return choice;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(((householdDay.Household.Id * 10 + householdDay.Day) * 397) ^ jHTSimulated);

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {

				RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, participants, choice);

				choiceProbabilityCalculator.WriteObservation();

				return choice;
			}
			else {
				RunModel(choiceProbabilityCalculator, householdDay, jHTSimulated, genChoice, participants);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);

				int i = 0;
				foreach (PersonDayWrapper personDay in orderedPersonDays) {
					i++;
					if ((int) chosenAlternative.Choice == i) {
						choice = personDay.Person.Sequence;
					}
				}
				return choice;
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int jHTSimulated, int genChoice, int[] participants, int choice = Constants.DEFAULT_VALUE) {

			IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

			int pairedHalfTour = genChoice == 1 ? 1 : 0;
			int firstHalfTour = genChoice == 2 ? 1 : 0;
			int secondHalfTour = genChoice == 3 ? 1 : 0;

			int[,] pType = new int[9, 6];
			int[] pDrivingAge = new int[6];

			int count = 0;
			foreach (PersonDayWrapper personDay in orderedPersonDays) {
				count++;
				if (count <= 5) {
					for (int i = 1; i < 9; i++) {
						pType[personDay.Person.PersonType, count] = 1;
					}
					pDrivingAge[count] = personDay.Person.IsDrivingAge.ToFlag();
				}
			}

			//Generate utility funtions for the alternatives
			bool[] available = new bool[5];
			bool[] chosen = new bool[5];
			int alt = 0;
			for (int i = 0; i < 5; i++) {
				alt = i + 1;
				available[i] = false;
				chosen[i] = false;

				// set availability based on participation in tour
				if (participants[alt] == 1) {
					available[i] = true;
				}
				// restrict availability if person is not driving age
				if (pDrivingAge[alt] == 0) {
					available[i] = false;
				}

				// determine choice
				if (choice == alt) { chosen[i] = true; }

				//Get the alternative
				var alternative = choiceProbabilityCalculator.GetAlternative(i, available[i], chosen[i]);

				alternative.Choice = alt;

				//Add utility terms
				alternative.AddUtilityTerm(1, pType[1, alt] * firstHalfTour);
				alternative.AddUtilityTerm(2, pType[2, alt] * firstHalfTour);
				alternative.AddUtilityTerm(5, pType[5, alt] * firstHalfTour);
				alternative.AddUtilityTerm(6, pType[6, alt] * firstHalfTour);

				alternative.AddUtilityTerm(11, pType[1, alt] * secondHalfTour);
				alternative.AddUtilityTerm(12, pType[2, alt] * secondHalfTour);
				alternative.AddUtilityTerm(15, pType[5, alt] * secondHalfTour);
				alternative.AddUtilityTerm(16, pType[6, alt] * secondHalfTour);

				alternative.AddUtilityTerm(21, pType[1, alt] * pairedHalfTour);
				alternative.AddUtilityTerm(22, pType[2, alt] * pairedHalfTour);
				alternative.AddUtilityTerm(25, pType[5, alt] * pairedHalfTour);
				alternative.AddUtilityTerm(26, pType[6, alt] * pairedHalfTour);

			}
		}
	}
}
