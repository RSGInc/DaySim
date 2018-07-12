using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
    class Fresno_WorkTourModeModel : WorkTourModeModel {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel)
        {
            //Global.PrintFile.WriteLine("Default Fresno_WorkTourModeModel.RegionSpecificCustomizations called");

            if (mode == Global.Settings.Modes.Bike){
                var origDist = tour.OriginParcel.District;
                var destDist = destinationParcel.District;
                var origCBDdestNorth = (origDist == 1) && (destDist > 1) && (destDist<5) ? 1 : 0; //cbd to north districts
                var origNorthdestCBD = (origDist > 1) && (origDist < 5) && (destDist == 1) ? 1 : 0; //north districts to cbd

                alternative.AddUtilityTerm(201, origCBDdestNorth);
                alternative.AddUtilityTerm(301, origNorthdestCBD);

                //alternative.AddUtilityTerm(200 + tour.OriginParcel.District, 1);//district specific calibration constant
                //alternative.AddUtilityTerm(300 + destinationParcel.District, 1);//district specific calibration constant
            }


        }
    }
}
