using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  class BKR_OtherHomeBasedTourModeModel : OtherHomeBasedTourModeModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {
      //Global.PrintFile.WriteLine("Default PSRC_OtherHomeBasedTourModeModel.RegionSpecificCustomizations2 called");
      // below is PSRC customization (
      if (mode == Global.Settings.Modes.Transit && pathType != Global.Settings.PathTypes.LightRail && pathType != Global.Settings.PathTypes.CommuterRail && pathType != Global.Settings.PathTypes.Ferry) {
        if (tour.OriginParcel.District < 60)
          alternative.AddUtilityTerm(200 + tour.OriginParcel.District, 1);//district specific transit calibration constant
        else {
          // district is >= 60, equivelent to 6 in PSRC customization
          alternative.AddUtilityTerm(200 + 6, 1);
        }
        if (destinationParcel.District < 60)
          alternative.AddUtilityTerm(300 + destinationParcel.District, 1);//district specific transit calibration constant
        else // district is >= 60, equivalent to 6 in PSRC customization
          alternative.AddUtilityTerm(300 + 6, 1);
      }
    }
  }
}
