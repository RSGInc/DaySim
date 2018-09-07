using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class Fresno_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      int homedist = _person.Household.ResidenceParcel.District;
      int zonedist = destinationParcel.District;

      //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 49, 99,100 or other unused variable #
      //Global.PrintFile.WriteLine("Default Fresno_WorkLocationModel.RegionSpecificCustomizations called");
      int homeNorthWorkSCBD = (homedist == 2 || homedist == 3 || homedist == 4 || homedist == 5 || homedist == 6 || homedist == 7) && (zonedist == 10) ? 1 : 0;
      int homeSCBDWorkSCBD = homedist == 10 && (zonedist == 10) ? 1 : 0;

      alternative.AddUtilityTerm(91, homeNorthWorkSCBD);
      alternative.AddUtilityTerm(92, homeSCBDWorkSCBD);

    }
  }
}
