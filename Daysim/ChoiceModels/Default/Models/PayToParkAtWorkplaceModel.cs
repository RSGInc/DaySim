// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Wrappers;

namespace Daysim.ChoiceModels.Default.Models {
	public class PayToParkAtWorkplaceModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "PayToParkAtWorkplaceModel";
		private const int TOTAL_ALTERNATIVES = 2;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 99;

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.PayToParkAtWorkplaceModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(IPersonWrapper person) {
			if (person == null) {
				throw new ArgumentNullException("person");
			}
			
			person.ResetRandom(2);

			if (Global.Configuration.IsInEstimationMode) {
				if (!_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(person.Id);

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {
				if (person.PaidParkingAtWorkplace < 0 || person.PaidParkingAtWorkplace > 1) {
					return;
				}

				RunModel(choiceProbabilityCalculator, person, person.PaidParkingAtWorkplace);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {
				RunModel(choiceProbabilityCalculator, person);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
				var choice = (int) chosenAlternative.Choice;

				person.PaidParkingAtWorkplace = choice;
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonWrapper person, int choice = Constants.DEFAULT_VALUE) {
			// 0 No paid parking at work

			var alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);

			alternative.Choice = 0;

			alternative.AddUtilityTerm(1, 0.0);

			// 1 Paid parking at work

			alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);

			alternative.Choice = 1;

			alternative.AddUtilityTerm(1, 1.0);
			alternative.AddUtilityTerm(2, person.IsPartTimeWorker.ToFlag());
			alternative.AddUtilityTerm(3, person.IsNotFullOrPartTimeWorker.ToFlag());
			alternative.AddUtilityTerm(4, Math.Max(1, person.Household.Income) / 1000.0);
			alternative.AddUtilityTerm(5, person.Household.HasMissingIncome.ToFlag());
			alternative.AddUtilityTerm(6, Math.Log(person.UsualWorkParcel.EmploymentTotalBuffer1 + 1.0));
			alternative.AddUtilityTerm(7, Math.Log((person.UsualWorkParcel.ParkingOffStreetPaidDailySpacesBuffer1 + 1.0) / (person.UsualWorkParcel.EmploymentTotalBuffer1 + 1.0)));
			alternative.AddUtilityTerm(8, person.UsualWorkParcel.ParkingOffStreetPaidDailyPriceBuffer1);
			alternative.AddUtilityTerm(9, person.UsualWorkParcel.EmploymentGovernmentBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
//			alternative.AddUtility(10, person.UsualWorkParcel.EmploymentOfficeBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1,1));
//			alternative.AddUtility(11, (person.UsualWorkParcel.EmploymentRetailBuffer1
//				                        +person.UsualWorkParcel.EmploymentFoodBuffer1) / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1,1));
			alternative.AddUtilityTerm(12, person.UsualWorkParcel.EmploymentEducationBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
//			alternative.AddUtility(13, person.UsualWorkParcel.EmploymentIndustrialBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1,1));
//			alternative.AddUtility(14, person.UsualWorkParcel.EmploymentMedicalBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1,1));
//			alternative.AddUtility(15, person.UsualWorkParcel.EmploymentServiceBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1,1));
		}
	}
}