using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class PSRC_WorkTourModeModel : WorkTourModeModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {
      //Global.PrintFile.WriteLine("Default PSRC_WorkTourModeModel.RegionSpecificCustomizations called");
      int homedist = tour.OriginParcel.District;
      int originPierceCounty = (homedist == 8 || homedist == 10 || homedist == 11) ? 1 : 0;
      int originSeaTac = (homedist == 12) ? 1 : 0;
      int SeaTacEmpZone1Index = Global.Configuration.SeaTacEmpZone1 - 1;
      int SeaTacEmpZone2Index = Global.Configuration.SeaTacEmpZone2 - 1;
      int SeaTacEmpZone3Index = Global.Configuration.SeaTacEmpZone3 - 1;

      if (mode == Global.Settings.Modes.Transit && pathType != Global.Settings.PathTypes.LightRail && pathType != Global.Settings.PathTypes.CommuterRail && pathType != Global.Settings.PathTypes.Ferry) {

        alternative.AddUtilityTerm(200 + tour.OriginParcel.District, 1);//district specific transit calibration constant
        alternative.AddUtilityTerm(300 + destinationParcel.District, 1);//district specific transit calibration constant
      }

      if (mode == Global.Settings.Modes.ParkAndRide) {
        alternative.AddUtilityTerm(250, pathType == Global.Settings.PathTypes.LocalBus ? 1 : 0);
        alternative.AddUtilityTerm(251, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        alternative.AddUtilityTerm(252, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
        alternative.AddUtilityTerm(253, pathType == Global.Settings.PathTypes.CommuterRail ? 1 : 0);
        alternative.AddUtilityTerm(254, pathType == Global.Settings.PathTypes.Ferry ? 1 : 0);

        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(410, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(510, originSeaTac == 1 ? 1 : 0);

        //sea-tac specific constants for drive to transit 
        if (Global.Configuration.SeaTacAirportZoneIndex>=0 && destinationParcel.ZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          alternative.AddUtilityTerm(261, pathType==Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(262, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

        //sea-tac specific constants for drive to transit to employment zone 1
        if (SeaTacEmpZone1Index>=0 && destinationParcel.ZoneId == SeaTacEmpZone1Index) {
          alternative.AddUtilityTerm(261, pathType==Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(262, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

        //sea-tac specific constants for drive to transit to employment zone 2
        if (SeaTacEmpZone2Index>=0 && destinationParcel.ZoneId == SeaTacEmpZone2Index) {
          alternative.AddUtilityTerm(261, pathType==Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(262, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

        //sea-tac specific constants for drive to transit to employment zone 3
        if (SeaTacEmpZone3Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone3Index) {
          alternative.AddUtilityTerm(261, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(262, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

      } else if (mode == Global.Settings.Modes.Transit) {
        alternative.AddUtilityTerm(255, pathType == Global.Settings.PathTypes.LocalBus ? 1 : 0);
        alternative.AddUtilityTerm(256, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        alternative.AddUtilityTerm(257, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
        alternative.AddUtilityTerm(258, pathType == Global.Settings.PathTypes.CommuterRail ? 1 : 0);
        alternative.AddUtilityTerm(259, pathType == Global.Settings.PathTypes.Ferry ? 1 : 0);

        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(420, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(520, originSeaTac == 1 ? 1 : 0);

        //sea-tac specific constants for walk to transit 
        if (Global.Configuration.SeaTacAirportZoneIndex >= 0 && destinationParcel.ZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          alternative.AddUtilityTerm(263, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(264, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

        //sea-tac specific constants for walk to transit to employment zone 1
        if (SeaTacEmpZone1Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone1Index) {
          alternative.AddUtilityTerm(263, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(264, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

        //sea-tac specific constants for walk to transit to employment zone 2
        if (SeaTacEmpZone2Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone2Index) {
          alternative.AddUtilityTerm(263, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(264, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

        //sea-tac specific constants for walk to transit to employment zone 3
        if (SeaTacEmpZone3Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone3Index) {
          alternative.AddUtilityTerm(263, pathType == Global.Settings.PathTypes.PremiumBus ? 1 : 0);
          alternative.AddUtilityTerm(264, pathType == Global.Settings.PathTypes.LightRail ? 1 : 0);
        }

      } else if (mode == Global.Settings.Modes.Hov3) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(430, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(530, originSeaTac == 1 ? 1 : 0);

        //sea-tac specific constant 
        if (Global.Configuration.SeaTacAirportZoneIndex >= 0 && destinationParcel.ZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          alternative.AddUtilityTerm(265, 1);
         }

        //sea-tac specific constant employment zone 1
        if (SeaTacEmpZone1Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone1Index) {
          alternative.AddUtilityTerm(265, 1);
         }

        //sea-tac specific constant employment zone 2
        if (SeaTacEmpZone2Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone2Index) {
          alternative.AddUtilityTerm(265, 1);
        }

        //sea-tac specific constant employment zone 3
        if (SeaTacEmpZone3Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone3Index) {
          alternative.AddUtilityTerm(265, 1);
        }

      } else if (mode == Global.Settings.Modes.Hov2) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(440, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(540, originSeaTac == 1 ? 1 : 0);

        //sea-tac specific constant 
        if (Global.Configuration.SeaTacAirportZoneIndex >= 0 && destinationParcel.ZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          alternative.AddUtilityTerm(266, 1);
        }

        //sea-tac specific constant employment zone 1
        if (SeaTacEmpZone1Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone1Index) {
          alternative.AddUtilityTerm(266, 1);
         }

        //sea-tac specific constant employment zone 2
        if (SeaTacEmpZone2Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone2Index) {
          alternative.AddUtilityTerm(266, 1);
        }

        //sea-tac specific constant employment zone 3
        if (SeaTacEmpZone3Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone3Index) {
          alternative.AddUtilityTerm(266, 1);
        }

      } else if (mode == Global.Settings.Modes.Sov) {
        //pierce county specific constant - added for PierceCast
        alternative.AddUtilityTerm(450, originPierceCounty == 1 ? 1 : 0);
        alternative.AddUtilityTerm(550, originSeaTac == 1 ? 1 : 0);
        
        //sea-tac specific constant 
        if (Global.Configuration.SeaTacAirportZoneIndex >= 0 && destinationParcel.ZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          alternative.AddUtilityTerm(267, 1);
        }

        //sea-tac specific constant employment zone 1
        if (SeaTacEmpZone1Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone1Index) {
          alternative.AddUtilityTerm(267, 1);
         }

        //sea-tac specific constant employment zone 2
        if (SeaTacEmpZone2Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone2Index) {
          alternative.AddUtilityTerm(267, 1);
        }

        //sea-tac specific constant employment zone 3
        if (SeaTacEmpZone3Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone3Index) {
          alternative.AddUtilityTerm(267, 1);
        }

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
   
        //sea-tac specific constant 
        if (Global.Configuration.SeaTacAirportZoneIndex >= 0 && destinationParcel.ZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          alternative.AddUtilityTerm(268, 1);
        }

        //sea-tac specific constant employment zone 1
        if (SeaTacEmpZone1Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone1Index) {
          alternative.AddUtilityTerm(268, 1);
         }

        //sea-tac specific constant employment zone 2
        if (SeaTacEmpZone2Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone2Index) {
          alternative.AddUtilityTerm(268, 1);
        }

        //sea-tac specific constant employment zone 3
        if (SeaTacEmpZone3Index >= 0 && destinationParcel.ZoneId == SeaTacEmpZone3Index) {
          alternative.AddUtilityTerm(268, 1);
        }

      }

    }
  }
}
