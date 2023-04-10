using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.DomainModels.Extensions;
using System;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Default.Models
{
    internal class PSRC_WorkLocationModel : WorkLocationModel
    {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel)
        {
            int homedist = _person.Household.ResidenceParcel.District;
            int zonedist = destinationParcel.District;


            //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 99,100 or other unused variable #
            //Global.PrintFile.WriteLine("Default PSRC_WorkLocationModel.RegionSpecificCustomizations called");
            int homeSKitWorkTRP = homedist == 11 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
            int homeKitWorkTRP = homedist == 9 && (zonedist == 8 || zonedist == 10 || zonedist == 7) ? 1 : 0;
            int homeEastWorkCBD = homedist == 6 && zonedist == 4 ? 1 : 0;
            int homeKitWorkCBD = (homedist == 9 || homedist == 11) && (zonedist == 4) ? 1 : 0;
            int homeTacWorkKit = homedist == 8 && (zonedist == 9 || zonedist == 11) ? 1 : 0;
            int homeEvWorkEv = homedist == 2 && zonedist == 2 ? 1 : 0;
            int homeWSWorkEast = homedist == 5 && zonedist == 6 ? 1 : 0;
            int homeEastWorkEast = homedist == 6 && zonedist == 6 ? 1 : 0;
            int homeKitWorkNotKit = (homedist == 9 || homedist == 11) && zonedist != 9 && zonedist != 11 ? 1 : 0;
            int homeSTacWorkCBD = (homedist == 5 || homedist == 8) && zonedist == 9 ? 1 : 0;

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

            //pierce county specific constants

            double distanceFromOrigin = _person.Household.ResidenceParcel.DistanceFromOrigin(destinationParcel, 1);

            double distance1 = Math.Min(distanceFromOrigin, .35);
            double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
            double distance3 = Math.Max(0, distanceFromOrigin - 1);
            int homePierceCounty = (homedist == 8 || homedist == 10 || homedist == 11) ? 1 : 0;

            alternative.AddUtilityTerm(101, _person.IsFulltimeWorker.ToFlag() * distance1 * homePierceCounty);
            alternative.AddUtilityTerm(102, _person.IsFulltimeWorker.ToFlag() * distance2 * homePierceCounty);
            alternative.AddUtilityTerm(103, _person.IsFulltimeWorker.ToFlag() * distance3 * homePierceCounty);

        }
    }
}

