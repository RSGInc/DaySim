using DaySim.Framework.Core;
using DaySim.Framework.Roster;

namespace DaySim.PathTypeModels
{
    class DVRPC_PathTypeModel : PathTypeModel
    {
        protected override void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight)
        {
            //Global.PrintFile.WriteLine("DVRPC_PathTypeModel.RegionSpecificTransitImpedanceCalculation called");
            //this is the outer weight on the sum of all the path specific terms
            pathTypeSpecificTimeWeight = 1.0;
            pathTypeSpecificTime =
                        Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                      + Global.Configuration.PathImpedance_TransitSubwayTimeAdditiveWeight * ImpedanceRoster.GetValue("subtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                      + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * ImpedanceRoster.GetValue("comrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                      + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * ImpedanceRoster.GetValue("brttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                      + Global.Configuration.PathImpedance_TransitPATTimeAdditiveWeight * ImpedanceRoster.GetValue("pattime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                      + Global.Configuration.PathImpedance_TransitTrolleyTimeAdditiveWeight * ImpedanceRoster.GetValue("troltime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            if (returnTime > 0)
            {
                pathTypeSpecificTime +=
                +Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitSubwayTimeAdditiveWeight * ImpedanceRoster.GetValue("subtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * ImpedanceRoster.GetValue("comrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * ImpedanceRoster.GetValue("brttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitPATTimeAdditiveWeight * ImpedanceRoster.GetValue("pattime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitTrolleyTimeAdditiveWeight * ImpedanceRoster.GetValue("troltime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
            }
        } //end RegionSpecificTransitImpedanceCalculation
    } //end class
} //end namespace
