using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class PSRC_OtherHomeBasedTourModeModel : OtherHomeBasedTourModeModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {

      //Global.PrintFile.WriteLine("Default PSRC_OtherHomeBasedTourModeModel.RegionSpecificCustomizations2 called");
      int homedist = tour.OriginParcel.District;
      int originPierceCounty = (homedist == 8 || homedist == 10 || homedist == 11) ? 1 : 0;
      int originSeaTac = (homedist == 12) ? 1 : 0;

      if (mode == Global.Settings.Modes.Transit && pathType != Global.Settings.PathTypes.LightRail && pathType != Global.Settings.PathTypes.CommuterRail && pathType != Global.Settings.PathTypes.Ferry) {

        alternative.AddUtilityTerm(200 + tour.OriginParcel.District, 1);//district specific transit calibration constant
        alternative.AddUtilityTerm(300 + destinationParcel.District, 1);//district specific transit calibration constant
      }

      if (mode == Global.Settings.Modes.ParkAndRide) {
        alternative.AddUtilityTerm(250, pathType == 3 ? 1 : 0);
        alternative.AddUtilityTerm(251, pathType == 4 ? 1 : 0);
        alternative.AddUtilityTerm(252, pathType == 5 ? 1 : 0);
        alternative.AddUtilityTerm(253, pathType == 6 ? 1 : 0);
        alternative.AddUtilityTerm(254, pathType == 7 ? 1 : 0);

        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(410, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(510, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.Transit) {
        alternative.AddUtilityTerm(255, pathType == 3 ? 1 : 0);
        alternative.AddUtilityTerm(256, pathType == 4 ? 1 : 0);
        alternative.AddUtilityTerm(257, pathType == 5 ? 1 : 0);
        alternative.AddUtilityTerm(258, pathType == 6 ? 1 : 0);
        alternative.AddUtilityTerm(259, pathType == 7 ? 1 : 0);

        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(420, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(520, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.Hov3) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(430, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(530, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.Hov2) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(440, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(540, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.Sov) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(450, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(550, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.Bike) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(460, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(560, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.Walk) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(470, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(570, originSeaTac == 1 ? 1 : 0);

      } else if (mode == Global.Settings.Modes.PaidRideShare) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(480, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(580, originSeaTac == 1 ? 1 : 0);

      }

    }
  }
}
