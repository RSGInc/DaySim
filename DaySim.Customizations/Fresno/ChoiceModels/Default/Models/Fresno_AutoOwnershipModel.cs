using System;
using DaySim.Framework.Core;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models
{
    internal class Fresno_AutoOwnershipModel : AutoOwnershipModel  {

      protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IHouseholdWrapper household) {
      //home district
      int homedist = household.ResidenceParcel.District;
      int homedist_clovis = (homedist == 5 || homedist == 6 || homedist == 8) ? 1 : 0; //clovis area is dist =5,6,8

      //clovis specific constants
      if (homedist_clovis == 1) 
      {
        alternative.AddUtilityTerm(101 + alternative.Id, household.Has1Driver.ToFlag()); //101,102 (not used),103,104,105
        alternative.AddUtilityTerm(106 + alternative.Id, household.Has2Drivers.ToFlag()); //106,107,108 (not used),109,110
        alternative.AddUtilityTerm(111 + alternative.Id, household.Has3Drivers.ToFlag()); //111,112,113,114 (not used),115
        alternative.AddUtilityTerm(116 + alternative.Id, household.Has4OrMoreDrivers.ToFlag()); //116,117,118,119,120 (not used)
      }
    }
  }
}
