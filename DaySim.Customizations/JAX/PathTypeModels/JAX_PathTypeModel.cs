using System;
using DaySim.Framework.Core;
using DaySim.Framework.Roster;

namespace DaySim.PathTypeModels {
  internal class JAX_PathTypeModel : PathTypeModel {
    protected override void RegionSpecificTransitImpedanceCalculation(int skimMode, int pathType, double votValue, int outboundTime, int returnTime, int originZoneId, int destinationZoneId, ref double outboundInVehicleTime, ref double returnInVehicleTime, ref double pathTypeSpecificTime, ref double pathTypeSpecificTimeWeight) {
      //this is the outer weight on the sum of all the path specific terms
      //copied from PSRC and adapted to use a different set of skim names and also constant terms - in units of minutes
      pathTypeSpecificTimeWeight = 1.0;

      double lrtTime = ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double comTime = ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double premTime = ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double circTime = ImpedanceRoster.GetValue("cirtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;
      double othTime = ImpedanceRoster.GetValue("othtime", skimMode, pathType, votValue, outboundTime, originZoneId, destinationZoneId).Variable;


      pathTypeSpecificTime = Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * lrtTime
          + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * comTime
          + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * premTime
          + Global.Configuration.PathImpedance_TransitCirculatorBusTimeAdditiveWeight * circTime
          + Global.Configuration.PathImpedance_TransitOtherModeTimeAdditiveWeight * othTime;

      pathTypeSpecificTime += Math.Min((othTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitOtherModePathConstant,
                               Math.Min((comTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitCommuterRailPathConstant,
                               Math.Min((lrtTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitLightRailPathConstant,
                               Math.Min((circTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitCirculatorBusPathConstant,
                                        (premTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitPremiumBusPathConstant))));
      if (returnTime > 0) {
        lrtTime = ImpedanceRoster.GetValue("lrttime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        comTime = ImpedanceRoster.GetValue("comtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        premTime = ImpedanceRoster.GetValue("premtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        circTime = ImpedanceRoster.GetValue("circtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;
        othTime = ImpedanceRoster.GetValue("othtime", skimMode, pathType, votValue, returnTime, destinationZoneId, originZoneId).Variable;

        pathTypeSpecificTime +=
        +Global.Configuration.PathImpedance_TransitLightRailTimeAdditiveWeight * lrtTime
        + Global.Configuration.PathImpedance_TransitCommuterRailTimeAdditiveWeight * comTime
        + Global.Configuration.PathImpedance_TransitPremiumBusTimeAdditiveWeight * premTime
        + Global.Configuration.PathImpedance_TransitCirculatorBusTimeAdditiveWeight * circTime
        + Global.Configuration.PathImpedance_TransitOtherModeTimeAdditiveWeight * othTime;

        pathTypeSpecificTime += Math.Min((othTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitOtherModePathConstant,
                                Math.Min((comTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitCommuterRailPathConstant,
                                Math.Min((lrtTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitLightRailPathConstant,
                                Math.Min((circTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitCirculatorBusPathConstant,
                                         (premTime > Constants.EPSILON).ToFlag() * Global.Configuration.PathImpedance_TransitPremiumBusPathConstant))));
      }
    } //end RegionSpecificTransitImpedanceCalculation
  } //end class
} //end namespace
