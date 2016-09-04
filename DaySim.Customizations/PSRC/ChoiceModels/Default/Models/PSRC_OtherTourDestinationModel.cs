using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
    class PSRC_OtherTourDestinationModel : OtherTourDestinationModel {

        protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel) {


             //add any region-specific new terms in region-specific class, using coefficient numbers 121-200
             //Global.PrintFile.WriteLine("PSRC_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
            var origdist = _tour.OriginParcel.District;
            var destdist = destinationParcel.District;
            var origKitDestTRP = (origdist == 9 || origdist == 11) && (destdist == 8 || destdist == 10 || destdist == 7) ? 1 : 0;
            var origEastDestCBD = origdist == 6 && destdist == 4 ? 1 : 0;
            var origTacDestKit = origdist == 8 && destdist == 9 || destdist == 11 ? 1 : 0;
            var origKitDestNotKit = (origdist == 9 || origdist == 11) && (destdist != 9 && destdist != 11) ? 1 : 0;
            var origSTacWorkCBD = (origdist == 11 && destdist == 4) ? 1 : 0;

            alternative.AddUtilityTerm(121, origEastDestCBD);
            alternative.AddUtilityTerm(122, origKitDestTRP);
            alternative.AddUtilityTerm(123, origTacDestKit);
            alternative.AddUtilityTerm(124, origKitDestNotKit);
            alternative.AddUtilityTerm(125, origSTacWorkCBD);
        }
    }
}
