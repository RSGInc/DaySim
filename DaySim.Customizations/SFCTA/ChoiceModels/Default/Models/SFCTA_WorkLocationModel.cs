using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class SFCTA_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      int homedist = _person.Household.ResidenceParcel.District;
      int zonedist = destinationParcel.District;

      //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 99,100 or other unused variable #
      //Global.PrintFile.WriteLine("Default PSRC_WorkLocationModel.RegionSpecificCustomizations called");
      /* example from PSRC
      var homeSKitWorkTRP = homedist == 11 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
      var homeKitWorkTRP = homedist == 9 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
      var homeEastWorkCBD = homedist == 6 && zonedist == 4 ? 1 : 0;
      var homeKitWorkCBD = (homedist == 9 || homedist == 11) && (zonedist == 4) ? 1 : 0;
      var homeTacWorkKit = homedist == 8 && (zonedist == 9 || zonedist == 11) ? 1 : 0;
      var homeEvWorkEv = homedist == 2 && zonedist == 2 ? 1 : 0;
      var homeWSWorkEast = homedist == 5 && zonedist == 6 ? 1 : 0;
      var homeEastWorkEast = homedist == 6 && zonedist == 6 ? 1 : 0;
      var homeKitWorkNotKit = (homedist == 9 || homedist == 11) && zonedist != 9 && zonedist != 11 ? 1 : 0;
      var homeSTacWorkCBD = (homedist == 5 || homedist == 8) && zonedist == 9 ? 1 : 0;

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
      */
    }
  }
}
