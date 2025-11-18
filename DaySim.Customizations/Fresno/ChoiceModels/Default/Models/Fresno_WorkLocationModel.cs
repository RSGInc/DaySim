using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class Fresno_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      int homedist = _person.Household.ResidenceParcel.District;
      int zonedist = destinationParcel.District;

      //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 49, 99,100 or other unused variable #
      //Global.PrintFile.WriteLine("Default Fresno_WorkLocationModel.RegionSpecificCustomizations called");
      int destDist1 = (zonedist == 1) ? 1 : 0;
      int destDist2 = (zonedist == 2) ? 1 : 0;
      int destDist3 = (zonedist == 3) ? 1 : 0;
      int destDist4 = (zonedist == 4) ? 1 : 0;
      int destDist5 = (zonedist == 5) ? 1 : 0;
      int destDist6 = (zonedist == 6) ? 1 : 0;
      int destDist7 = (zonedist == 7) ? 1 : 0;
      int destDist8 = (zonedist == 8) ? 1 : 0;
      int destDist9 = (zonedist == 9) ? 1 : 0;
      int destDist10 = (zonedist == 10) ? 1 : 0;

      alternative.AddUtilityTerm(101, destDist1);
      alternative.AddUtilityTerm(102, destDist2);
      alternative.AddUtilityTerm(103, destDist3);
      alternative.AddUtilityTerm(104, destDist4);
      alternative.AddUtilityTerm(105, destDist5);
      alternative.AddUtilityTerm(106, destDist6);
      alternative.AddUtilityTerm(107, destDist7);
      alternative.AddUtilityTerm(108, destDist8);
      alternative.AddUtilityTerm(109, destDist9);
      alternative.AddUtilityTerm(110, destDist10);

    }
  }
}
