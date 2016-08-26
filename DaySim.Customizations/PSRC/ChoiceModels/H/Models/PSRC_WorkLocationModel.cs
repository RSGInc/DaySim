using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.H.Models
{
    class PSRC_WorkLocationModel : WorkLocationModel
    {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, int homedist, int zonedist)
        {
            //Global.PrintFile.WriteLine("H PSRC_WorkLocationModel.RegionSpecificCustomizations called");
            var homeSKitWorkTRP = homedist == 11 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
            var homeKitWorkTRP = homedist == 9 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
            var homeEastWorkCBD = homedist == 6 && zonedist == 4 ? 1 : 0;
            var homeKitWorkCBD = homedist == 9 && (zonedist == 4) ? 1 : 0;
            var homeTacWorkKit = homedist == 8 && (zonedist == 9 || zonedist == 11) ? 1 : 0;
            var homeEvWorkEv = homedist == 2 && zonedist == 2 ? 1 : 0;
            var homeWSWorkEast = homedist == 5 && zonedist == 6 ? 1 : 0;

            alternative.AddUtilityTerm(37, homeTacWorkKit);
            alternative.AddUtilityTerm(38, homeEvWorkEv);
            alternative.AddUtilityTerm(39, homeWSWorkEast);
            alternative.AddUtilityTerm(40, homeSKitWorkTRP);
            alternative.AddUtilityTerm(45, homeKitWorkTRP);
            alternative.AddUtilityTerm(47, homeKitWorkTRP);
            alternative.AddUtilityTerm(48, homeEastWorkCBD);
            alternative.AddUtilityTerm(49, homeKitWorkCBD);
        }
    }
}
