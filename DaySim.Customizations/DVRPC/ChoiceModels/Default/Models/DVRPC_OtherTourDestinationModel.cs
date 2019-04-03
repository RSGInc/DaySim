using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.Default.Models {
  internal class DVRPC_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel) {

      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #

      int origState = _tour.OriginParcel.District;
      int destState = destinationParcel.District;

      int bridgeFromNJ = (origState == 34 && destState > 0 && destState != 34).ToFlag();
      int bridgeToNJ = (destState == 34 && origState > 0 && origState != 34).ToFlag();

      alternative.AddUtilityTerm(114, bridgeFromNJ);
      alternative.AddUtilityTerm(115, bridgeToNJ);
    }

  
  }
}
