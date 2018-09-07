using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.H.Models {
  internal class PSRC_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      int homedist = _person.Household.ResidenceParcel.District;
      int zonedist = destinationParcel.District;

      //add any region-specific new terms in region-specific class, using coefficient numbers 91-100 or other unused var #
      //Global.PrintFile.WriteLine("H PSRC_WorkLocationModel.RegionSpecificCustomizations called");
      int homeSKitWorkTRP = homedist == 11 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
      int homeKitWorkTRP = homedist == 9 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
      int homeEastWorkCBD = homedist == 6 && zonedist == 4 ? 1 : 0;
      int homeKitWorkCBD = homedist == 9 && (zonedist == 4) ? 1 : 0;
      int homeTacWorkKit = homedist == 8 && (zonedist == 9 || zonedist == 11) ? 1 : 0;
      int homeEvWorkEv = homedist == 2 && zonedist == 2 ? 1 : 0;
      int homeWSWorkEast = homedist == 5 && zonedist == 6 ? 1 : 0;

      alternative.AddUtilityTerm(92, homeTacWorkKit);
      alternative.AddUtilityTerm(93, homeEvWorkEv);
      alternative.AddUtilityTerm(94, homeWSWorkEast);
      alternative.AddUtilityTerm(95, homeSKitWorkTRP);
      alternative.AddUtilityTerm(97, homeKitWorkTRP);
      alternative.AddUtilityTerm(99, homeEastWorkCBD);
      alternative.AddUtilityTerm(100, homeKitWorkCBD);
    }
  }
}
