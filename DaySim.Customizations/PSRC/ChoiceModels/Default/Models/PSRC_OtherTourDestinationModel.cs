using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
    class PSRC_OtherTourDestinationModel : OtherTourDestinationModel {

        protected static new void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, IParcelWrapper originParcel, IParcelWrapper destinationParcel) {
            Global.PrintFile.WriteLine("PSRC_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
            var origdist = originParcel.District;
            var destdist = destinationParcel.District;
            var origKitDestTRP = (origdist == 9 || origdist == 11) && (destdist == 8 || destdist == 10 || destdist == 7) ? 1 : 0;
            var origEastDestCBD = origdist == 6 && destdist == 4 ? 1 : 0;
            var origTacDestKit = origdist == 8 && destdist == 9 || destdist == 11 ? 1 : 0;
            var origKitDestNotKit = (origdist == 9 || origdist == 11) && (destdist != 9 && destdist != 11) ? 1 : 0;
            var origSTacWorkCBD = (origdist == 11 && destdist == 4) ? 1 : 0;

            alternative.AddUtilityTerm(58, origEastDestCBD);
            alternative.AddUtilityTerm(59, origKitDestTRP);
            alternative.AddUtilityTerm(60, origTacDestKit);
            alternative.AddUtilityTerm(61, origKitDestNotKit);
            alternative.AddUtilityTerm(62, origSTacWorkCBD);
        }
    }
}
