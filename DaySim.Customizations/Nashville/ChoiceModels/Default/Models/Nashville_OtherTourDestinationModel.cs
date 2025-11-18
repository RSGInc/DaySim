using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class Nashville_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel, IPersonWrapper person) {

      int ori = _tour.OriginParcel.District;
      int des = destinationParcel.District;

      //spgen - this field identifies special generators 1-hospital, 2-shopping mall, 3-recreation, 4-airport, 5-industrial, and 6-other, 7-regional
      int spgen = destinationParcel.LandUseCode;
      //double spgen_hosp = (spgen == 1 && (_tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness || _tour.DestinationPurpose == Global.Settings.Purposes.Medical)) ? destinationParcel.EmploymentTotal : 0;
      //double spgen_shop = (spgen == 2 && (_tour.DestinationPurpose == Global.Settings.Purposes.Shopping)) ? destinationParcel.EmploymentTotal : 0;
      //double spgen_rec = (spgen == 3 && (_tour.DestinationPurpose == Global.Settings.Purposes.Social || _tour.DestinationPurpose == Global.Settings.Purposes.Recreation)) ? destinationParcel.EmploymentTotal : 0;
      //double spgen_airp = (spgen == 4 && (_tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness || _tour.DestinationPurpose == Global.Settings.Purposes.Medical)) ? destinationParcel.EmploymentTotal : 0;
      //double spgen_ind = (spgen == 5) ? destinationParcel.EmploymentTotal : 0;
      //double spgen_oth = (spgen == 6) ? destinationParcel.EmploymentTotal : 0;

      double spgen_hosp = (spgen == 1) ? destinationParcel.EmploymentTotal : 0;
      double spgen_shop = (spgen == 2) ? destinationParcel.EmploymentTotal : 0;
      //double spgen_rec = (spgen == 3) ? destinationParcel.EmploymentTotal : 0;
      double spgen_rec = (spgen == 3) ? 1 : 0;
      // double spgen_airp = (spgen == 4) ? destinationParcel.EmploymentTotal : 0;
      double spgen_airp = (spgen == 4) ? 1 : 0;
      double spgen_ind = (spgen == 5) ? destinationParcel.EmploymentTotal : 0;
      double spgen_oth = (spgen == 6) ? destinationParcel.EmploymentTotal : 0;
      double spgen_reg = (spgen == 7) ? 1 : 0;

      //int spgen_hosp = (destinationParcel.ZoneKey == 825 || destinationParcel.ZoneKey == 971) ? 1 : 0; //hospital
      //int spgen_shop = (destinationParcel.ZoneKey == 850 || destinationParcel.ZoneKey == 856) ? 1 : 0; //shopping mall
      //int spgen_rec = (destinationParcel.ZoneKey == 226 || destinationParcel.ZoneKey == 720) ? 1 : 0;  //recreation
      //int spgen_airp = (destinationParcel.ZoneKey == 256 || destinationParcel.ZoneKey == 1891) ? 1 : 0; //airport

      //add any region-specific new terms in region-specific class
      //Global.PrintFile.WriteLine("Fresno_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");

      int dist_CBD = (des == 1) ? 1 : 0;       //1-CBD
      int dist_IC_I24E = (des == 2) ? 1 : 0;   //2-Inner Core I-24 E
      int dist_IC_I65N = (des == 3) ? 1 : 0;   //3-Inner Core I-24 / 65 N
      int dist_IC_I24W = (des == 4) ? 1 : 0;   //4-Inner Core I-24W
      int dist_IC_I40W = (des == 5) ? 1 : 0;   //5-Inner Core I-40 W
      int dist_IC_I65S = (des == 6) ? 1 : 0;   //6-Inner Core I-65 S
      int dist_MC_I24E = (des == 7) ? 1 : 0;   //7-Middle Core I-24 E
      int dist_MC_I24W = (des == 8) ? 1 : 0;   //8-Middle Core I-24 W
      int dist_MC_I40E = (des == 9) ? 1 : 0;   //9-Middle Core I-40 E
      int dist_MC_I40W = (des == 10) ? 1 : 0;  //10-Middle Core I-40 W
      int dist_MC_I65N = (des == 11) ? 1 : 0;  //11-Middle Core I-65 N
      int dist_MC_I65S = (des == 12) ? 1 : 0;  //12-Middle Core I-65 S
      int dist_OC_I24E = (des == 13) ? 1 : 0;  //13-Outer Core I-24 E
      int dist_OC_I24W = (des == 14) ? 1 : 0;  //14-Outer Core I-24 W
      int dist_OC_I40E = (des == 15) ? 1 : 0;  //15-Outer Core I-40 E
      int dist_OC_I40W = (des == 16) ? 1 : 0;  //16-Outer Core I-40 W
      int dist_OC_I65N = (des == 17) ? 1 : 0;  //17-Outer Core I-65 N
      int dist_OC_I65S = (des == 18) ? 1 : 0;  //18-Outer Core I-65 S
      int dist_OC_SR386 = (des == 19) ? 1 : 0; //19-Outer Core SR386
      int dist_FOC_I65S = (des == 20) ? 1 : 0; //20-Far Outer Core I-65 S
      int orig1_dest1 = (ori == 1) && (des == 1) ? 1 : 0;
      int orig6_dest6 = (ori == 6) && (des == 6) ? 1 : 0;
      int orig12_dest12 = (ori == 12) && (des == 12) ? 1 : 0;
      int orig13_dest13 = (ori == 13) && (des == 13) ? 1 : 0;
      int orig20_dest20 = (ori == 20) && (des == 20) ? 1 : 0;

      //destination districts
      alternative.AddUtilityTerm(201, dist_CBD);
      alternative.AddUtilityTerm(202, dist_IC_I24E);
      alternative.AddUtilityTerm(203, dist_IC_I65N);
      alternative.AddUtilityTerm(204, dist_IC_I24W);
      alternative.AddUtilityTerm(205, dist_IC_I40W);
      alternative.AddUtilityTerm(206, dist_IC_I65S);
      alternative.AddUtilityTerm(207, dist_MC_I24E);
      alternative.AddUtilityTerm(208, dist_MC_I24W);
      alternative.AddUtilityTerm(209, dist_MC_I40E);
      alternative.AddUtilityTerm(210, dist_MC_I40W);
      alternative.AddUtilityTerm(211, dist_MC_I65N);
      alternative.AddUtilityTerm(212, dist_MC_I65S);
      alternative.AddUtilityTerm(213, dist_OC_I24E);
      alternative.AddUtilityTerm(214, dist_OC_I24W);
      alternative.AddUtilityTerm(215, dist_OC_I40E);
      alternative.AddUtilityTerm(216, dist_OC_I40W);
      alternative.AddUtilityTerm(217, dist_OC_I65N);
      alternative.AddUtilityTerm(218, dist_OC_I65S);
      alternative.AddUtilityTerm(219, dist_OC_SR386);
      alternative.AddUtilityTerm(220, dist_FOC_I65S);

      //origin-destination districts
      alternative.AddUtilityTerm(221, orig1_dest1);
      alternative.AddUtilityTerm(222, orig6_dest6);
      alternative.AddUtilityTerm(223, orig12_dest12);
      alternative.AddUtilityTerm(224, orig13_dest13);
      alternative.AddUtilityTerm(225, orig20_dest20);

      //special generators
      alternative.AddUtilityTerm(301, spgen_hosp);
      alternative.AddUtilityTerm(302, spgen_shop);
      alternative.AddUtilityTerm(303, spgen_rec);
      alternative.AddUtilityTerm(304, spgen_airp);
      alternative.AddUtilityTerm(305, spgen_ind);
      alternative.AddUtilityTerm(306, spgen_oth);
      alternative.AddUtilityTerm(307, spgen_reg);

    }
  }
}
