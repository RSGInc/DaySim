using System;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Default.Models {
  internal class PSRC_AutoOwnershipModel : AutoOwnershipModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IHouseholdWrapper household) {
      bool Has0To25KIncome = household.Income.IsRightExclusiveBetween(0, 25000);
      bool Has50To100KIncome = household.Income.IsRightExclusiveBetween(50000, 100000);
      bool Has100To150KIncome = household.Income.IsRightExclusiveBetween(100000, 150000);
      bool Has150KPlusIncome = household.Income >= 150000;

      //put on each alternative except 3 (2 vehicles), using the alternative number to number coefficient
      if (alternative.Id != 3) {
        alternative.AddUtilityTerm(100 + alternative.Id, Has0To25KIncome.ToFlag());
        alternative.AddUtilityTerm(110 + alternative.Id, Has50To100KIncome.ToFlag());
        alternative.AddUtilityTerm(120 + alternative.Id, Has100To150KIncome.ToFlag());
        alternative.AddUtilityTerm(130 + alternative.Id, Has150KPlusIncome.ToFlag());
        alternative.AddUtilityTerm(140 + alternative.Id, Math.Log(1 + household.ResidenceParcel.HouseholdDensity1()));
        alternative.AddUtilityTerm(150 + alternative.Id, Math.Log(1 + household.ResidenceParcel.EmploymentTotalBuffer1));
      }
    }
  }
}
