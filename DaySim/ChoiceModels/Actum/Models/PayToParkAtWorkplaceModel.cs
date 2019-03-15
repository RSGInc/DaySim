// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Actum.Models {
  public class PayToParkAtWorkplaceModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumPayToParkAtWorkplaceModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.PayToParkAtWorkplaceModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonWrapper person) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(2);

      if (Global.Configuration.IsInEstimationMode) {
        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (person.PaidParkingAtWorkplace < 0 || person.PaidParkingAtWorkplace > 1) {
          return;
        }

        RunModel(choiceProbabilityCalculator, person, person.PaidParkingAtWorkplace);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, person);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        person.PaidParkingAtWorkplace = choice;
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonWrapper person_x, int choice = Constants.DEFAULT_VALUE) {
      //MB check for access to new Actum person properties
      //requres a changing name in call, and cast to a new variable called person, and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header
      IActumPersonWrapper person = (IActumPersonWrapper)person_x;
      int checkPersInc = person.PersonalIncome;
      //end check

      //MB check for new hh properties
      //requres a cast to a household, and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)person.Household;
      int checkKids6To17 = household.Persons6to17;
      // end check

      //MB check for access to new Actum parcel properties
      //requires a cast and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header - use new variable workerUsualParcel...
      if (person.UsualWorkParcel != null) {
        IActumParcelWrapper workerUsualParcel = (IActumParcelWrapper)person.UsualWorkParcel;
        double checkWorkMZParkCost = workerUsualParcel.PublicParkingHourlyPriceBuffer1;
      }
      //end check


      // 0 No paid parking at work

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);

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
      alternative.AddUtilityTerm(10, person.UsualWorkParcel.EmploymentOfficeBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
      alternative.AddUtilityTerm(11, (person.UsualWorkParcel.EmploymentRetailBuffer1
                                     + person.UsualWorkParcel.EmploymentFoodBuffer1) / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
      alternative.AddUtilityTerm(12, person.UsualWorkParcel.EmploymentEducationBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
      alternative.AddUtilityTerm(13, person.UsualWorkParcel.EmploymentIndustrialBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
      alternative.AddUtilityTerm(14, person.UsualWorkParcel.EmploymentMedicalBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));
      alternative.AddUtilityTerm(15, person.UsualWorkParcel.EmploymentServiceBuffer1 / Math.Max(person.UsualWorkParcel.EmploymentTotalBuffer1, 1));  
    }
  }
}
