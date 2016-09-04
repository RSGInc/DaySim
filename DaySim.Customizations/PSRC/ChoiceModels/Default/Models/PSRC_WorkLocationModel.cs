using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Default.Models
{
    class PSRC_WorkLocationModel : WorkLocationModel
    {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel)
        {
            var homedist = _person.Household.ResidenceParcel.District;
            var zonedist = destinationParcel.District;

            //add any region-specific new terms in region-specific class, using coefficient numbers 121-200
            //Global.PrintFile.WriteLine("Default PSRC_WorkLocationModel.RegionSpecificCustomizations called");
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

            alternative.AddUtilityTerm(121, homeEastWorkEast);
            alternative.AddUtilityTerm(122, homeTacWorkKit);
            alternative.AddUtilityTerm(123, homeEvWorkEv);
            alternative.AddUtilityTerm(124, homeWSWorkEast);
            alternative.AddUtilityTerm(125, homeSKitWorkTRP);
            alternative.AddUtilityTerm(126, homeSTacWorkCBD);
            alternative.AddUtilityTerm(127, homeKitWorkTRP);
            alternative.AddUtilityTerm(128, homeKitWorkNotKit);
            alternative.AddUtilityTerm(129, homeEastWorkCBD);
            alternative.AddUtilityTerm(130, homeKitWorkCBD);
        }
    }
}
