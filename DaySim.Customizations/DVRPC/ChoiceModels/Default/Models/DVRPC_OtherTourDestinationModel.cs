using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.Default.Models {
  internal class DVRPC_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel) {


      //areas 

      int o_int_paphi = (_tour.OriginParcel.ZoneKey <= 4000).ToFlag();
      int o_int_paoth = (_tour.OriginParcel.ZoneKey <= 18000).ToFlag() * (1 - o_int_paphi);
      int o_int_nj = (_tour.OriginParcel.ZoneKey > 18000 && _tour.OriginParcel.ZoneKey <= 30000).ToFlag();
      int o_ext_pa = (_tour.OriginParcel.ZoneKey > 50000 && _tour.OriginParcel.ZoneKey <= 53000).ToFlag();
      int o_ext_nj = (_tour.OriginParcel.ZoneKey > 53000 && _tour.OriginParcel.ZoneKey <= 58000).ToFlag();
      int o_ext_oth = (_tour.OriginParcel.ZoneKey > 58000).ToFlag();

      int d_int_paphi = (destinationParcel.ZoneKey <= 4000).ToFlag();
      int d_int_paoth = (destinationParcel.ZoneKey <= 18000).ToFlag() * (1 - d_int_paphi);
      int d_int_nj = (destinationParcel.ZoneKey > 18000 && destinationParcel.ZoneKey <= 30000).ToFlag();
      int d_ext_pa = (destinationParcel.ZoneKey > 50000 && destinationParcel.ZoneKey <= 53000).ToFlag();
      int d_ext_nj = (destinationParcel.ZoneKey > 53000 && destinationParcel.ZoneKey <= 58000).ToFlag();
      int d_ext_oth = (destinationParcel.ZoneKey > 58000).ToFlag();


      alternative.AddUtilityTerm(130, (_tour.OriginParcel.Id == destinationParcel.Id).ToFlag());

      alternative.AddUtilityTerm(131, o_int_paphi * d_int_paphi);
      alternative.AddUtilityTerm(132, o_int_paphi * d_int_paoth);
      alternative.AddUtilityTerm(133, o_int_paphi * d_int_nj);
      alternative.AddUtilityTerm(134, o_int_paphi * d_ext_pa);
      alternative.AddUtilityTerm(135, o_int_paphi * d_ext_nj);
      alternative.AddUtilityTerm(136, o_int_paphi * d_ext_oth);

      alternative.AddUtilityTerm(141, o_int_paoth * d_int_paphi);
      alternative.AddUtilityTerm(142, o_int_paoth * d_int_paoth);
      alternative.AddUtilityTerm(143, o_int_paoth * d_int_nj);
      alternative.AddUtilityTerm(144, o_int_paoth * d_ext_pa);
      alternative.AddUtilityTerm(145, o_int_paoth * d_ext_nj);
      alternative.AddUtilityTerm(146, o_int_paoth * d_ext_oth);

      alternative.AddUtilityTerm(151, o_int_nj * d_int_paphi);
      alternative.AddUtilityTerm(152, o_int_nj * d_int_paoth);
      alternative.AddUtilityTerm(153, o_int_nj * d_int_nj);
      alternative.AddUtilityTerm(154, o_int_nj * d_ext_pa);
      alternative.AddUtilityTerm(155, o_int_nj * d_ext_nj);
      alternative.AddUtilityTerm(156, o_int_nj * d_ext_oth);


      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #

      //int origState = _tour.OriginParcel.District;
      //int destState = destinationParcel.District;

      //int bridgeFromNJ = (origState == 34 && destState > 0 && destState != 34).ToFlag();
      //int bridgeToNJ = (destState == 34 && origState > 0 && origState != 34).ToFlag();

      //alternative.AddUtilityTerm(114, bridgeFromNJ);
      //alternative.AddUtilityTerm(115, bridgeToNJ);
    }


  }
}
