using DaySim.Framework.Core;
using DaySim.Framework.Roster;
using System;
using System.Collections.Generic;

namespace DaySim.PathTypeModels {
  internal class PSRC_PathTypeModel : PathTypeModel {
    protected override void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, int destinationPurpose, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight, ref double fare) {
      //Global.PrintFile.WriteLine("PSRC_PathTypeModel.RegionSpecificTransitImpedanceCalculation called");
      //this is the outer weight on the sum of all the path specific terms
      pathTypeSpecificTimeWeight = 1.0;
      pathTypeSpecificTime = Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
          + Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight * ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
          + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
          + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      if (returnTime > 0) {
        pathTypeSpecificTime +=
        +Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
        + Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight * ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
        + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
        + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
      }
      // only run this code if that if the SeaTac Airport zone index is set by user (it = 0 - the zone number is 1 but the internal zone id is 0, if not set by user it is -1)
      if (Global.Configuration.SeaTacAirportZoneIndex >= 0) {

        //if it is to or from the airport and the purpose is not work but the path type is premium bus (airport shuttle), make it a long trip

        if (destinationZoneId == Global.Configuration.SeaTacAirportZoneIndex || originZoneId == Global.Configuration.SeaTacAirportZoneIndex) {
          if (pathType == Global.Settings.PathTypes.PremiumBus) {
            if (destinationPurpose != Global.Settings.Purposes.Work) {
              returnInVehicleTime = 9999.0;
              outboundInVehicleTime = 9999.0;
            } else {
              fare = 0.0;
            }
          }
        }

        //if either origin or deestination are in the shuttle catchment area but neither the the origin or destination zone is the airport
        //disable the path - note catchment zone numbers decrease by one to match zoneid, which is zone number minus 1
        ///List<int> shuttle_catchment = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 11, 13, 14, 15, 16, 17, 20, 21, 22, 23, 24, 25, 26, 27, 28, 31, 33, 34, 40, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 63, 101, 112, 119, 145, 159 };
        //if ((shuttle_catchment.Contains(originZoneId) || shuttle_catchment.Contains(destinationZoneId)) && pathType == Global.Settings.PathTypes.PremiumBus) {
        int catchmentAreaLowestZone = 1;
        int catchmentAreaHighestZone = 159;
        bool destinationInCatchmentArea = destinationZoneId >= catchmentAreaLowestZone && destinationZoneId <= catchmentAreaHighestZone;
        bool originInCatchmentArea = originZoneId >= catchmentAreaLowestZone && originZoneId <= catchmentAreaHighestZone;
        if (pathType == Global.Settings.PathTypes.PremiumBus && (originInCatchmentArea || destinationInCatchmentArea)) {
          if (destinationZoneId != Global.Configuration.SeaTacAirportZoneIndex && originZoneId != Global.Configuration.SeaTacAirportZoneIndex) {
            returnInVehicleTime = 9999.0;
            outboundInVehicleTime = 9999.0;
          }
        }
      }


    } //end RegionSpecificTransitImpedanceCalculation
  } //end class
} //end namespace
