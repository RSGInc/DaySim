using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class SFCTA_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel, IPersonWrapper person) {


      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
      //Global.PrintFile.WriteLine("PSRC_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
      int origdist = _tour.OriginParcel.District;
      int destdist = destinationParcel.District;
      /* example from psrc
      var origKitDestTRP = (origdist == 9 || origdist == 11) && (destdist == 8 || destdist == 10 || destdist == 7) ? 1 : 0;
      var origEastDestCBD = origdist == 6 && destdist == 4 ? 1 : 0;
      var origTacDestKit = origdist == 8 && destdist == 9 || destdist == 11 ? 1 : 0;
      var origKitDestNotKit = (origdist == 9 || origdist == 11) && (destdist != 9 && destdist != 11) ? 1 : 0;
      var origSTacWorkCBD = (origdist == 11 && destdist == 4) ? 1 : 0;

      alternative.AddUtilityTerm(115, origEastDestCBD);
      alternative.AddUtilityTerm(116, origKitDestTRP);
      alternative.AddUtilityTerm(117, origTacDestKit);
      alternative.AddUtilityTerm(118, origKitDestNotKit);
      alternative.AddUtilityTerm(119, origSTacWorkCBD);
      */
    }
  }
}
