using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
    class JAX_OtherHomeBasedTourModeModel : OtherHomeBasedTourModeModel {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {
            //Global.PrintFile.WriteLine("Default JAX_WorkTourModeModel.RegionSpecificCustomizations2 called");

            //currently empty

        }
    }
}
