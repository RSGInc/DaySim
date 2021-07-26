using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;
using DaySim.DomainModels.Extensions;

namespace DaySim.ChoiceModels.Default.Models {
  internal class DVRPC_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel, IPersonWrapper person) {


      //areas 

      int o_int_paphi = (_tour.OriginParcel.ZoneKey <= 4000).ToFlag();
      int o_int_paoth = (_tour.OriginParcel.ZoneKey <= 18000).ToFlag() * (1 - o_int_paphi);
      int o_int_nj = (_tour.OriginParcel.ZoneKey > 18000 && _tour.OriginParcel.ZoneKey <= 30000).ToFlag();
      int o_ext_pa = (_tour.OriginParcel.ZoneKey > 50000 && _tour.OriginParcel.ZoneKey <= 53000).ToFlag();
      int o_ext_nnj = (_tour.OriginParcel.ZoneKey > 53000 && _tour.OriginParcel.ZoneKey <= 56000).ToFlag();
      int o_ext_snj = (_tour.OriginParcel.ZoneKey > 56000 && _tour.OriginParcel.ZoneKey <= 58000).ToFlag();
      int o_ext_oth = (_tour.OriginParcel.ZoneKey > 58000).ToFlag();

      int d_int_paphi = (destinationParcel.ZoneKey <= 4000).ToFlag();
      int d_int_paoth = (destinationParcel.ZoneKey <= 18000).ToFlag() * (1 - d_int_paphi);
      int d_int_nj = (destinationParcel.ZoneKey > 18000 && destinationParcel.ZoneKey <= 30000).ToFlag();
      int d_ext_pa = (destinationParcel.ZoneKey > 50000 && destinationParcel.ZoneKey <= 53000).ToFlag();
      int d_ext_nnj = (destinationParcel.ZoneKey > 53000 && destinationParcel.ZoneKey <= 56000).ToFlag();
      int d_ext_snj = (destinationParcel.ZoneKey > 56000 && destinationParcel.ZoneKey <= 58000).ToFlag();
      int d_ext_oth = (destinationParcel.ZoneKey > 58000).ToFlag();

      int cbdDest = (destinationParcel.HouseholdsBuffer1 + destinationParcel.EmploymentTotalBuffer1 >= 30000).ToFlag();
      double distanceFromOrigin = _tour.OriginParcel.DistanceFromOrigin(destinationParcel, _tour.DestinationArrivalTime);

      alternative.AddUtilityTerm(121, o_int_nj * distanceFromOrigin);
      alternative.AddUtilityTerm(122, o_int_paoth * distanceFromOrigin);
      alternative.AddUtilityTerm(123, cbdDest * distanceFromOrigin);
      alternative.AddUtilityTerm(124, cbdDest * (_tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness).ToFlag());
      alternative.AddUtilityTerm(125, cbdDest * (_tour.DestinationPurpose == Global.Settings.Purposes.Shopping).ToFlag());
      alternative.AddUtilityTerm(126, cbdDest * (_tour.DestinationPurpose == Global.Settings.Purposes.Meal).ToFlag());
      alternative.AddUtilityTerm(127, cbdDest * (_tour.DestinationPurpose == Global.Settings.Purposes.Social).ToFlag());
      alternative.AddUtilityTerm(201, o_int_paphi * distanceFromOrigin);

      alternative.AddUtilityTerm(130, (_tour.OriginParcel.Id == destinationParcel.Id).ToFlag());

      alternative.AddUtilityTerm(131, o_int_paphi * d_int_paphi);
      alternative.AddUtilityTerm(132, o_int_paphi * d_int_paoth);
      alternative.AddUtilityTerm(133, o_int_paphi * d_int_nj);
      alternative.AddUtilityTerm(134, o_int_paphi * d_ext_pa);
      alternative.AddUtilityTerm(135, o_int_paphi * d_ext_nnj);
      alternative.AddUtilityTerm(136, o_int_paphi * d_ext_oth);
      alternative.AddUtilityTerm(137, o_int_paphi * d_ext_snj);

      alternative.AddUtilityTerm(141, o_int_paoth * d_int_paphi);
      alternative.AddUtilityTerm(142, o_int_paoth * d_int_paoth);
      alternative.AddUtilityTerm(143, o_int_paoth * d_int_nj);
      alternative.AddUtilityTerm(144, o_int_paoth * d_ext_pa);
      alternative.AddUtilityTerm(145, o_int_paoth * d_ext_nnj);
      alternative.AddUtilityTerm(146, o_int_paoth * d_ext_oth);
      alternative.AddUtilityTerm(147, o_int_paoth * d_ext_snj);

      alternative.AddUtilityTerm(151, o_int_nj * d_int_paphi);
      alternative.AddUtilityTerm(152, o_int_nj * d_int_paoth);
      alternative.AddUtilityTerm(153, o_int_nj * d_int_nj);
      alternative.AddUtilityTerm(154, o_int_nj * d_ext_pa);
      alternative.AddUtilityTerm(155, o_int_nj * d_ext_nnj);
      alternative.AddUtilityTerm(156, o_int_nj * d_ext_oth);
      alternative.AddUtilityTerm(157, o_int_nj * d_ext_snj);

      alternative.AddUtilityTerm(161, o_ext_pa * d_int_paphi);
      alternative.AddUtilityTerm(162, o_ext_pa * d_int_paoth);
      alternative.AddUtilityTerm(163, o_ext_pa * d_int_nj);
      alternative.AddUtilityTerm(164, o_ext_pa * d_ext_pa);
      alternative.AddUtilityTerm(165, o_ext_pa * d_ext_nnj);
      alternative.AddUtilityTerm(166, o_ext_pa * d_ext_oth);
      alternative.AddUtilityTerm(167, o_ext_pa * d_ext_snj);

      alternative.AddUtilityTerm(171, o_ext_nnj * d_int_paphi);
      alternative.AddUtilityTerm(172, o_ext_nnj * d_int_paoth);
      alternative.AddUtilityTerm(173, o_ext_nnj * d_int_nj);
      alternative.AddUtilityTerm(174, o_ext_nnj * d_ext_pa);
      alternative.AddUtilityTerm(175, o_ext_nnj * d_ext_nnj);
      alternative.AddUtilityTerm(176, o_ext_nnj * d_ext_oth);
      alternative.AddUtilityTerm(177, o_ext_nnj * d_ext_snj);

      alternative.AddUtilityTerm(181, o_ext_oth * d_int_paphi);
      alternative.AddUtilityTerm(182, o_ext_oth * d_int_paoth);
      alternative.AddUtilityTerm(183, o_ext_oth * d_int_nj);
      alternative.AddUtilityTerm(184, o_ext_oth * d_ext_pa);
      alternative.AddUtilityTerm(185, o_ext_oth * d_ext_nnj);
      alternative.AddUtilityTerm(186, o_ext_oth * d_ext_oth);
      alternative.AddUtilityTerm(187, o_ext_oth * d_ext_snj);

      alternative.AddUtilityTerm(191, o_ext_snj * d_int_paphi);
      alternative.AddUtilityTerm(192, o_ext_snj * d_int_paoth);
      alternative.AddUtilityTerm(193, o_ext_snj * d_int_nj);
      alternative.AddUtilityTerm(194, o_ext_snj * d_ext_pa);
      alternative.AddUtilityTerm(195, o_ext_snj * d_ext_nnj);
      alternative.AddUtilityTerm(196, o_ext_snj * d_ext_oth);
      alternative.AddUtilityTerm(197, o_ext_snj * d_ext_snj);

 

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
