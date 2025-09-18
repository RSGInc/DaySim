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
          if (pathType == Global.Configuration.SeaTacAirportEmployeeShuttlePathType) {
            if (destinationPurpose != Global.Settings.Purposes.Work) {
              returnInVehicleTime = 9999.0;
              outboundInVehicleTime = 9999.0;
            } else {
              fare = Global.Configuration.SeaTacAirportEmployeeShuttleFare
                   + Global.Configuration.SeaTacAirportEmployeeShuttleCalibrationPenaltyInDollars;
            }
          }
        }

        //if either origin or deestination are in the shuttle catchment area but neither the the origin or destination zone is the airport disable the path 
        bool destinationInCatchmentArea = destinationZoneId >= Global.Configuration.SeaTacAirportEmployeeShuttleCatchmentAreaLowestZone-1 
                                       && destinationZoneId <= Global.Configuration.SeaTacAirportEmployeeShuttleCatchmentAreaHighestZone-1;
        bool originInCatchmentArea = originZoneId >= Global.Configuration.SeaTacAirportEmployeeShuttleCatchmentAreaLowestZone-1
                                  && originZoneId <= Global.Configuration.SeaTacAirportEmployeeShuttleCatchmentAreaHighestZone-1;
        if (pathType == Global.Configuration.SeaTacAirportEmployeeShuttlePathType && (originInCatchmentArea || destinationInCatchmentArea)) {
          if (destinationZoneId != Global.Configuration.SeaTacAirportZoneIndex && originZoneId != Global.Configuration.SeaTacAirportZoneIndex) {
            returnInVehicleTime = 9999.0;
            outboundInVehicleTime = 9999.0;
          }
        }
      }


    } //end RegionSpecificTransitImpedanceCalculation
  } //end class
} //end namespace
