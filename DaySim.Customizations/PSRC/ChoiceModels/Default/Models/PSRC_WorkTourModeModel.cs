using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models
{
    class PSRC_WorkTourModeModel : WorkTourModeModel
    {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int mode)
        {
            //Global.PrintFile.WriteLine("Default PSRC_WorkTourModeModel.RegionSpecificCustomizations called");

            if (mode == Global.Settings.Modes.Transit)
            {

                alternative.AddUtilityTerm(200 + tour.OriginParcel.District, 1);//district specific transit calibration constant
            }


        }
    }
}
