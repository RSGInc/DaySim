using DaySim.Framework.Core;
using DaySim.Framework.Roster;
using System;
using System.Collections.Generic;

namespace DaySim.PathTypeModels {
  internal class PSRC_PathTypeModel : PathTypeModel {
    protected override void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, int destinationPurpose, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight) {
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

      //work tours to SEA airport (destination zone id = 1)
      //set ivt for destinations other than SEA airport to 999.0 to make them unavailable
      
      if (destinationZoneId == 1 || originZoneId ==1) {
        if (destinationPurpose != Global.Settings.Purposes.Work && pathType == Global.Settings.PathTypes.PremiumBus) {
          returnInVehicleTime = 999.0;
          outboundInVehicleTime = 999.0;
        }
      }

      //if origin and deestination is in the catchment area but destination isn't airport
      //disable the path
      List<int> shuttle_catchment = new List<int> {1,2,3,4,5,6,7,8,9,12,14,15,16,17,18,21,22,23,24,25,26,27,28,29,32,34,35,41,47,48,49,50,51,52,53,54,55,56,57,58,59,64,102,113,120,146,160};
      if (shuttle_catchment.Contains(originZoneId) && shuttle_catchment.Contains(destinationZoneId) && pathType == Global.Settings.PathTypes.PremiumBus) {
        if (destinationZoneId !=1 && originZoneId !=1) {
          returnInVehicleTime = 999.0;
          outboundInVehicleTime = 999.0;
        }
      }
    } //end RegionSpecificTransitImpedanceCalculation
  } //end class
} //end namespace
