using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class Nashville_WorkTourModeModel : WorkTourModeModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {
      //Global.PrintFile.WriteLine("Default PSRC_WorkTourModeModel.RegionSpecificCustomizations called");

      if (mode == Global.Settings.Modes.Transit) {
        //cap total employment density 1 to 35000.
        //paramter 128 is applied to total emp density 1 for transit work tour mode.
        //calculate excess emp density above the threshold and take off that amount from the utility.
        double empdens1 = destinationParcel.TotalEmploymentDensity1();
        //add a term for Vanderbilt University/Medical center (tazid=1471).
        double dest_zone = destinationParcel.ZoneKey;
        int dest_vanderbilt = (dest_zone == 1471) ? 1 : 0;
        alternative.AddUtilityTerm(200, dest_vanderbilt);

      }
    }
  }
}
