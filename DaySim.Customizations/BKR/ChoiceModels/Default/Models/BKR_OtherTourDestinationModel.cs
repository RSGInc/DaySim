using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Default.Models {
  class BKR_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel,  IPersonWrapper person) {

      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
      //Global.PrintFile.WriteLine("PSRC_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
      int origdist = _tour.OriginParcel.District;
      int destdist = destinationParcel.District;
      int origKitDestTRP = (origdist == 9 || origdist == 11) && (destdist == 8 || destdist == 10 || destdist == 7) ? 1 : 0;
      int origEastDestCBD = origdist >= 60 && origdist <= 65 && destdist == 4 ? 1 : 0;
      int origTacDestKit = origdist == 8 && destdist == 9 || destdist == 11 ? 1 : 0;
      int origKitDestNotKit = (origdist == 9 || origdist == 11) && (destdist != 9 && destdist != 11) ? 1 : 0;
      int origSTacWorkCBD = (origdist == 11 && destdist == 4) ? 1 : 0;

      alternative.AddUtilityTerm(115, origEastDestCBD);
      alternative.AddUtilityTerm(116, origKitDestTRP);
      alternative.AddUtilityTerm(117, origTacDestKit);
      alternative.AddUtilityTerm(118, origKitDestNotKit);
      alternative.AddUtilityTerm(119, origSTacWorkCBD);


      // Below is BKR customization
      //Global.PrintFile.WriteLine("Default BKR_OtherHomeBasedTourModeModel.RegionSpecificCustomizations2 called");
      if (_tour.IsShoppingPurpose() && (_tour.Mode == Global.Settings.Modes.Sov || _tour.Mode == Global.Settings.Modes.Hov2 || _tour.Mode == Global.Settings.Modes.Hov3)) {
        if (destinationParcel.District == 62 && _tour.OriginParcel.District != 62) {
          alternative.AddUtilityTerm(120, 1);
        }
      }
      if (_tour.IsMealPurpose() && (_tour.Mode == Global.Settings.Modes.Sov || _tour.Mode == Global.Settings.Modes.Hov2 || _tour.Mode == Global.Settings.Modes.Hov3)) {
        if (destinationParcel.District == 62 && _tour.OriginParcel.District != 62) {
          alternative.AddUtilityTerm(121, 1);
        }
      }
      if (_tour.IsPersonalBusinessPurpose() && (_tour.Mode == Global.Settings.Modes.Sov || _tour.Mode == Global.Settings.Modes.Hov2 || _tour.Mode == Global.Settings.Modes.Hov3)) {
        if (destinationParcel.District == 62 && _tour.OriginParcel.District != 62) {
          alternative.AddUtilityTerm(122, 1);
        }
      }
      if (_tour.IsSocialPurpose() && (_tour.Mode == Global.Settings.Modes.Sov || _tour.Mode == Global.Settings.Modes.Hov2 || _tour.Mode == Global.Settings.Modes.Hov3)) {
        if (destinationParcel.District == 62 && _tour.OriginParcel.District != 62) {
          alternative.AddUtilityTerm(123, 1);
        }
      }
      if (_tour.IsEscortPurpose() && (_tour.Mode == Global.Settings.Modes.Sov || _tour.Mode == Global.Settings.Modes.Hov2 || _tour.Mode == Global.Settings.Modes.Hov3)) {
        if (destinationParcel.District == 62 && _tour.OriginParcel.District != 62) {
          alternative.AddUtilityTerm(124, 1);
        }
      }

      // to Belleuve Square from outside of Bel CBD
      // even thoug the zoneid is clearly labeled, it is zero-based. that means if you want to reference a TAZ nunber, you need to use TAZ number - 1 in ZoneId.
      // in this case, Bellevue Square is in TAZ 11. its ZoneId in destinationParcel is 10. 
      if (destinationParcel.ZoneId == 10 && origdist != 62)
        alternative.AddUtilityTerm(125, 1);
    }
  }
}
