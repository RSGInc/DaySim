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
            int destZoneId = destinationParcel.ZoneId;
            bool Has0To25KIncome = _person.Household.Income.IsRightExclusiveBetween(0, 25000);
            bool Has50To100KIncome = _person.Household.Income.IsRightExclusiveBetween(50000, 100000);
            bool Has100To150KIncome = _person.Household.Income.IsRightExclusiveBetween(100000, 150000);
            bool Has150KPlusIncome = _person.Household.Income >= 150000;


            double distanceFromOrigin = _person.Household.ResidenceParcel.DistanceFromOrigin(destinationParcel, 1);
            double distanceLog = Math.Log(1 + distanceFromOrigin);

            alternative.AddUtilityTerm(110, Has0To25KIncome.ToFlag() * distanceLog);
            alternative.AddUtilityTerm(111, Has50To100KIncome.ToFlag() * distanceLog);
            alternative.AddUtilityTerm(112, Has100To150KIncome.ToFlag() * distanceLog);
            alternative.AddUtilityTerm(113, Has150KPlusIncome.ToFlag() * distanceLog);


            //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 99,100 or other unused variable #
            //Global.PrintFile.WriteLine("Default PSRC_WorkLocationModel.RegionSpecificCustomizations called");
            int homeSKitWorkTRP = homedist == 11 && (zonedist == 8 || zonedist == 10 || zonedist == 7 || zonedist == 12) ? 1 : 0; // added seatac district 12
            int homeKitWorkTRP = homedist == 9 && (zonedist == 8 || zonedist == 10 || zonedist == 7 || zonedist == 12) ? 1 : 0; // added seatac district 12
            int homeEastWorkCBD = homedist == 6 && zonedist == 4 ? 1 : 0;
            int homeKitWorkCBD = (homedist == 9 || homedist == 11) && (zonedist == 4) ? 1 : 0;
            int homeTacWorkKit = homedist == 8 && (zonedist == 9 || zonedist == 11) ? 1 : 0;
            int homeEvWorkEv = homedist == 2 && zonedist == 2 ? 1 : 0;
            int homeWSWorkEast = (homedist == 5 || homedist == 12) && zonedist == 6 ? 1 : 0; // added seatac district 12
            int homeEastWorkEast = homedist == 6 && zonedist == 6 ? 1 : 0;
            int homeKitWorkNotKit = (homedist == 9 || homedist == 11) && zonedist != 9 && zonedist != 11 ? 1 : 0;
            int homeSTacWorkCBD = (homedist == 5 || homedist == 8 || homedist == 12) && zonedist == 9 ? 1 : 0;

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

            //double distanceFromOrigin = _person.Household.ResidenceParcel.DistanceFromOrigin(destinationParcel, 1);

            double distance1 = Math.Min(distanceFromOrigin, .35);
            double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
            double distance3 = Math.Max(0, distanceFromOrigin - 1);
            int homePierceCounty = (homedist == 8 || homedist == 10 || homedist == 11) ? 1 : 0;

            alternative.AddUtilityTerm(101, _person.IsFulltimeWorker.ToFlag() * distance1 * homePierceCounty);
            alternative.AddUtilityTerm(102, _person.IsFulltimeWorker.ToFlag() * distance2 * homePierceCounty);
            alternative.AddUtilityTerm(103, _person.IsFulltimeWorker.ToFlag() * distance3 * homePierceCounty);

            //seatac specific constants
            int homeSeaTac = (homedist == 12) ? 1 : 0;
            alternative.AddUtilityTerm(104, _person.IsFulltimeWorker.ToFlag() * distance1 * homeSeaTac);
            alternative.AddUtilityTerm(105, _person.IsFulltimeWorker.ToFlag() * distance2 * homeSeaTac);
            alternative.AddUtilityTerm(106, _person.IsFulltimeWorker.ToFlag() * distance3 * homeSeaTac);


            int SeaTacAirportZoneIndex = Global.Configuration.SeaTacAirportZoneIndex;
            int SeaTacEmpZone1Index = Global.Configuration.SeaTacEmpZone1 - 1;
            int SeaTacEmpZone2Index = Global.Configuration.SeaTacEmpZone2 - 1;
            int SeaTacEmpZone3Index = Global.Configuration.SeaTacEmpZone3 - 1;
            int AirPortZone = (SeaTacAirportZoneIndex == destZoneId || SeaTacEmpZone1Index == destZoneId || SeaTacEmpZone2Index == destZoneId || SeaTacEmpZone3Index == destZoneId) ? 1 : 0;

            //seatac specific constants
            int homeEvertEdmndssubSnoh = (homedist == 1 || homedist == 2) && AirPortZone == 1 ? 1 : 0;
            int homeNSeaShore = (homedist == 3) && AirPortZone == 1 ? 1 : 0;
            int homeSeaCBD = (homedist == 4) && AirPortZone == 1 ? 1 : 0;
            int homeWestSouthSea = (homedist == 5) && AirPortZone == 1 ? 1 : 0;
            int homeEastSide = (homedist == 6) && AirPortZone == 1 ? 1 : 0;
            int homeRenFedKent = (homedist == 7) && AirPortZone == 1 ? 1 : 0;
            int homeTacoma = (homedist == 8) && AirPortZone == 1 ? 1 : 0;
            int homeKitsap = (homedist == 9) && AirPortZone == 1 ? 1 : 0;
            int homeSKitSapSPierce = (homedist == 10 ||  homedist == 11) && AirPortZone == 1 ? 1 : 0;
            int homeStac = (homedist == 12) && AirPortZone == 1 ? 1 : 0;
            //int homeEvertEdmndssubSnoh = (homedist == 1 || homedist == 2) && zonedist == 12 ? 1 : 0;
            //int homeNSeaShore = (homedist == 3) && zonedist == 12 ? 1 : 0;
            //int homeSeaCBD = (homedist == 4) && zonedist == 12 ? 1 : 0;
            //int homeWestSouthSea = (homedist == 5) && zonedist == 12 ? 1 : 0;
            //int homeEastSide = (homedist == 6) && zonedist == 12 ? 1 : 0;
            //int homeRenFedKent = (homedist == 7) && zonedist == 12 ? 1 : 0;
            //int homeTacoma = (homedist == 8) && zonedist == 12 ? 1 : 0;
            //int homeKitsap = (homedist == 9) && zonedist == 12 ? 1 : 0;
            //int homeSKitSapSPierce = (homedist == 10 || homedist == 11) && zonedist == 12 ? 1 : 0;
            //int homeStac = (homedist == 12) && zonedist == 12 ? 1 : 0;
      alternative.AddUtilityTerm(120, homeEvertEdmndssubSnoh);
            alternative.AddUtilityTerm(121, homeNSeaShore);
            alternative.AddUtilityTerm(122, homeSeaCBD);
            alternative.AddUtilityTerm(123, homeWestSouthSea);
            alternative.AddUtilityTerm(124, homeEastSide);
            alternative.AddUtilityTerm(125, homeRenFedKent);
            alternative.AddUtilityTerm(126, homeTacoma);
            alternative.AddUtilityTerm(127, homeKitsap);
            alternative.AddUtilityTerm(128, homeSKitSapSPierce);
            alternative.AddUtilityTerm(129, homeStac);

    }
    }
}

