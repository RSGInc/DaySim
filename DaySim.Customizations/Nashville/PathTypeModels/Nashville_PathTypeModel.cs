using DaySim.Framework.Core;
using DaySim.Framework.Roster;

namespace DaySim.PathTypeModels
{
    class Nashville_PathTypeModel : PathTypeModel
    {
        protected new static void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight)
        {
            Global.PrintFile.WriteLine("Nashville_PathTypeModel.RegionSpecificTransitImpedanceCalculation called");
            //Nashville BRT coded in Ferry
            //Nashville Commuter Rail coded in Commuter rail
            //Nashville Express Bus coded in Premium bus

            //ASC based on IVT share by sub-mode
            pathTypeSpecificTimeWeight = 1.0;

            pathTypeSpecificTime =
                  Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable / outboundInVehicleTime
                + Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable / outboundInVehicleTime
                + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable / outboundInVehicleTime
                + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable / outboundInVehicleTime;

            if (returnTime > 0)
            {

                pathTypeSpecificTime = pathTypeSpecificTime

                + Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable / returnInVehicleTime
                + Global.Configuration.PathImpedance_TransitFerryTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable / returnInVehicleTime
                + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable / returnInVehicleTime
                + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight *
                    ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable / returnInVehicleTime;
            }

            //IVT discounted by sub-mode
            double outboundLightRailInVehicleTime = ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            double outboundFerryInVehicleTime = ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            double outboundCommuterRailInVehicleTime = ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            double outboundPremiumBusInVehicleTime = ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
            double outboundLocalBusInVehicleTime = outboundInVehicleTime - (outboundLightRailInVehicleTime + outboundFerryInVehicleTime +
                   outboundCommuterRailInVehicleTime + outboundPremiumBusInVehicleTime);

            outboundInVehicleTime =
                  Global.Configuration.PathImpedance_TransitLightRailInVehicleTimeWeight * outboundLightRailInVehicleTime
                + Global.Configuration.PathImpedance_TransitFerryInVehicleTimeWeight * outboundFerryInVehicleTime
                + Global.Configuration.PathImpedance_TransitCommuterRailInVehicleTimeWeight * outboundCommuterRailInVehicleTime
                + Global.Configuration.PathImpedance_TransitPremiumBusInVehicleTimeWeight * outboundPremiumBusInVehicleTime
                + 1 * outboundLocalBusInVehicleTime;

            if (returnTime > 0)
            {
                double returnLightRailInVehicleTime = ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
                double returnFerryInVehicleTime = ImpedanceRoster.GetValue("ferrtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
                double returnCommuterRailInVehicleTime = ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
                double returnPremiumBusInVehicleTime = ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
                double returnLocalBusInVehicleTime = returnInVehicleTime - (returnLightRailInVehicleTime + returnFerryInVehicleTime +
                       returnCommuterRailInVehicleTime + returnPremiumBusInVehicleTime);

                returnInVehicleTime =
                  Global.Configuration.PathImpedance_TransitLightRailInVehicleTimeWeight * returnLightRailInVehicleTime
                + Global.Configuration.PathImpedance_TransitFerryInVehicleTimeWeight * returnFerryInVehicleTime
                + Global.Configuration.PathImpedance_TransitCommuterRailInVehicleTimeWeight * returnCommuterRailInVehicleTime
                + Global.Configuration.PathImpedance_TransitPremiumBusInVehicleTimeWeight * returnPremiumBusInVehicleTime
                + 1 * returnLocalBusInVehicleTime;

            } //end if (returnTime > 0)

        } //end RegionSpecificTransitImpedanceCalculation
    } //end class
} //end namespace
