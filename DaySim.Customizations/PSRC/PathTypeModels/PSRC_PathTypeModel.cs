using DaySim.Framework.Core;
using DaySim.Framework.Roster;

namespace DaySim.PathTypeModels
{
    class PSRC_PathTypeModel : PathTypeModel
    {
        protected override void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight)
        {
            //Global.PrintFile.WriteLine("PSRC_PathTypeModel.RegionSpecificTransitImpedanceCalculation called");
            //this is the outer weight on the sum of all the path specific terms
            pathTypeSpecificTimeWeight = 1.0;
            pathTypeSpecificTime = Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                + Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight * ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable
                + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            if (returnTime > 0)
            {
                pathTypeSpecificTime +=
                +Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight * ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable
                + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
            }
        } //end RegionSpecificTransitImpedanceCalculation
    } //end class
} //end namespace
