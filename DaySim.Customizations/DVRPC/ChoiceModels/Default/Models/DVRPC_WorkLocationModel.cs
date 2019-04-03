using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;


namespace DaySim.ChoiceModels.Default.Models {
  internal class DVRPC_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {

      int origState = _person.Household.ResidenceParcel.District;
      int destState = destinationParcel.District;

      int bridgeFromNJ = (origState == 34 && destState > 0 && destState != 34).ToFlag();
      int bridgeToNJ   = (destState == 34 && origState > 0 && origState != 34).ToFlag();

      alternative.AddUtilityTerm(91, bridgeFromNJ);
      alternative.AddUtilityTerm(92, bridgeToNJ);
    }
  }
}
