using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  class BKR_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      int homedist = _person.Household.ResidenceParcel.District;
      int zonedist = destinationParcel.District;

      //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 99,100 or other unused variable #
      //Global.PrintFile.WriteLine("Default PSRC_WorkLocationModel.RegionSpecificCustomizations called");
      int homeSKitWorkTRP = homedist == 11 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
      int homeKitWorkTRP = homedist == 9 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
      int homeKitWorkCBD = (homedist == 9 || homedist == 11) && (zonedist == 4) ? 1 : 0;
      int homeTacWorkKit = homedist == 8 && (zonedist == 9 || zonedist == 11) ? 1 : 0;
      int homeEvWorkEv = homedist == 2 && zonedist == 2 ? 1 : 0;
      int homeKitWorkNotKit = (homedist == 9 || homedist == 11) && zonedist != 9 && zonedist != 11 ? 1 : 0;
      int homeSTacWorkCBD = (homedist == 5 || homedist == 8) && zonedist == 9 ? 1 : 0;

      int homeEastWorkCBD = homedist >= 60 && homedist <= 65 && zonedist == 4 ? 1 : 0;
      int homeEastWorkEast = homedist >= 60 && homedist <= 65 && zonedist >= 60 && zonedist <= 65 ? 1 : 0;
      int homeWSWorkEast = homedist == 5 && zonedist >= 60 && zonedist <= 65 ? 1 : 0;

      alternative.AddUtilityTerm(91, homeEastWorkEast);
      alternative.AddUtilityTerm(92, homeTacWorkKit);
      alternative.AddUtilityTerm(93, homeEvWorkEv);
      alternative.AddUtilityTerm(94, homeWSWorkEast);
      alternative.AddUtilityTerm(95, homeSKitWorkTRP);
      alternative.AddUtilityTerm(96, homeSTacWorkCBD);
      alternative.AddUtilityTerm(97, homeKitWorkTRP);
      alternative.AddUtilityTerm(49, homeKitWorkNotKit);
      alternative.AddUtilityTerm(99, homeEastWorkCBD);
      alternative.AddUtilityTerm(100, homeKitWorkCBD);

      // BKRCast customization below
      if (zonedist == 62)
        alternative.AddUtilityTerm(101, 1);
    }
  }
}
