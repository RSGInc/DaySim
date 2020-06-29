using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySim.ChoiceModels.Default.Models {
  internal class DVRPC_WorkTourModeModel : WorkTourModeModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {

      int originInNJ = ((tour.OriginParcel.ZoneKey >= 18000 && tour.OriginParcel.ZoneKey < 50000) || (tour.OriginParcel.ZoneKey >= 53000 && tour.OriginParcel.ZoneKey < 58000)).ToFlag();
      int destinInNJ = ((destinationParcel.ZoneKey >= 18000 && destinationParcel.ZoneKey < 50000) || (destinationParcel.ZoneKey >= 53000 && destinationParcel.ZoneKey < 58000)).ToFlag();

      int coreCBDOrig = (tour.OriginParcel.HouseholdsBuffer1 + tour.OriginParcel.EmploymentTotalBuffer1 >= 20000).ToFlag();
      int coreCBDDest = (destinationParcel.HouseholdsBuffer1 + destinationParcel.EmploymentTotalBuffer1 >= 20000).ToFlag();


      if (mode == Global.Settings.Modes.ParkAndRide) {
        alternative.AddUtilityTerm(211, tour.OriginParcel.CBD_AreaType_Buffer1());
        alternative.AddUtilityTerm(212, tour.OriginParcel.Urban_AreaType_Buffer1());
        alternative.AddUtilityTerm(213, tour.OriginParcel.Suburban_AreaType_Buffer1());
        alternative.AddUtilityTerm(214, tour.OriginParcel.AllRural_AreaType_Buffer1());
        alternative.AddUtilityTerm(215, destinationParcel.CBD_AreaType_Buffer1());
        alternative.AddUtilityTerm(216, destinationParcel.Urban_AreaType_Buffer1());
        alternative.AddUtilityTerm(217, destinationParcel.Suburban_AreaType_Buffer1());
        alternative.AddUtilityTerm(218, destinationParcel.AllRural_AreaType_Buffer1());
        alternative.AddUtilityTerm(219, (tour.OriginParcel.DistanceToFerry > 0 && tour.OriginParcel.DistanceToFerry <= 0.5).ToFlag());
        alternative.AddUtilityTerm(220, (destinationParcel.DistanceToFerry > 0 && destinationParcel.DistanceToFerry <= 0.5).ToFlag());
        alternative.AddUtilityTerm(221, originInNJ);
        alternative.AddUtilityTerm(222, destinInNJ);
        alternative.AddUtilityTerm(223, destinInNJ * originInNJ);
        alternative.AddUtilityTerm(224, coreCBDOrig);
        alternative.AddUtilityTerm(225, coreCBDDest);

        // alternative.AddUtilityTerm(225, destinationParcel.PCA_DensityTerm_Buffer1());
        // alternative.AddUtilityTerm(226, destinationParcel.PCA_WalkabilityTerm_Buffer1());
        // alternative.AddUtilityTerm(227, destinationParcel.PCA_MixedUseTerm_Buffer1());
        // alternative.AddUtilityTerm(228, destinationParcel.PCA_TransitAccessTerm_Buffer1());
      } else if (mode == Global.Settings.Modes.Transit) {
        alternative.AddUtilityTerm(231, tour.OriginParcel.CBD_AreaType_Buffer1());
        alternative.AddUtilityTerm(232, tour.OriginParcel.Urban_AreaType_Buffer1());
        alternative.AddUtilityTerm(233, tour.OriginParcel.Suburban_AreaType_Buffer1());
        alternative.AddUtilityTerm(234, tour.OriginParcel.AllRural_AreaType_Buffer1());
        alternative.AddUtilityTerm(235, destinationParcel.CBD_AreaType_Buffer1());
        alternative.AddUtilityTerm(236, destinationParcel.Urban_AreaType_Buffer1());
        alternative.AddUtilityTerm(237, destinationParcel.Suburban_AreaType_Buffer1());
        alternative.AddUtilityTerm(238, destinationParcel.AllRural_AreaType_Buffer1());
        alternative.AddUtilityTerm(239, (tour.OriginParcel.DistanceToFerry > 0 && tour.OriginParcel.DistanceToFerry <= 0.5).ToFlag());
        alternative.AddUtilityTerm(240, (destinationParcel.DistanceToFerry > 0 && destinationParcel.DistanceToFerry <= 0.5).ToFlag());
        alternative.AddUtilityTerm(241, originInNJ);
        alternative.AddUtilityTerm(242, destinInNJ);
        alternative.AddUtilityTerm(243, destinInNJ * originInNJ);
        alternative.AddUtilityTerm(244, coreCBDOrig);
        alternative.AddUtilityTerm(245, coreCBDDest);

        //  alternative.AddUtilityTerm(221, originParcel.PCA_DensityTerm_Buffer1());
        //  alternative.AddUtilityTerm(222, originParcel.PCA_WalkabilityTerm_Buffer1());
        //  alternative.AddUtilityTerm(223, originParcel.PCA_MixedUseTerm_Buffer1());
        //  alternative.AddUtilityTerm(224, originParcel.PCA_TransitAccessTerm_Buffer1());
        //  alternative.AddUtilityTerm(225, destinationParcel.PCA_DensityTerm_Buffer1());
        //  alternative.AddUtilityTerm(226, destinationParcel.PCA_WalkabilityTerm_Buffer1());
        //  alternative.AddUtilityTerm(227, destinationParcel.PCA_MixedUseTerm_Buffer1());
        //  alternative.AddUtilityTerm(228, destinationParcel.PCA_TransitAccessTerm_Buffer1());
      }
    }
  }
}

