using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models
{
  internal class PSRC_SchoolTourModeModel : SchoolTourModeModel
  {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel)
    {
      //Global.PrintFile.WriteLine("Default PSRC_SchoolTourModeModel.RegionSpecificCustomizations called");

      if (mode == Global.Settings.Modes.Transit && pathType != Global.Settings.PathTypes.LightRail && pathType != Global.Settings.PathTypes.CommuterRail && pathType != Global.Settings.PathTypes.Ferry)
      {

        alternative.AddUtilityTerm(200 + tour.OriginParcel.District, 1);//district specific transit calibration constant
        alternative.AddUtilityTerm(300 + destinationParcel.District, 1);//district specific transit calibration constant
      }

      if (mode == Global.Settings.Modes.ParkAndRide)
      {
        alternative.AddUtilityTerm(250, pathType == 3 ? 1 : 0);
        alternative.AddUtilityTerm(251, pathType == 4 ? 1 : 0);
        alternative.AddUtilityTerm(252, pathType == 5 ? 1 : 0);
        alternative.AddUtilityTerm(253, pathType == 6 ? 1 : 0);
        alternative.AddUtilityTerm(254, pathType == 7 ? 1 : 0);

      }
      else if (mode == Global.Settings.Modes.Transit)
      {
        alternative.AddUtilityTerm(255, pathType == 3 ? 1 : 0);
        alternative.AddUtilityTerm(256, pathType == 4 ? 1 : 0);
        alternative.AddUtilityTerm(257, pathType == 5 ? 1 : 0);
        alternative.AddUtilityTerm(258, pathType == 6 ? 1 : 0);
        alternative.AddUtilityTerm(259, pathType == 7 ? 1 : 0);
      }


    }
  }
}
