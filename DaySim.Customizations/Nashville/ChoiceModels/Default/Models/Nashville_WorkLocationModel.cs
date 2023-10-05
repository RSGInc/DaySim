using DaySim.Framework.Core;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class Nashville_WorkLocationModel : WorkLocationModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      int homedist = _person.Household.ResidenceParcel.District;
      int zonedist = destinationParcel.District;

      //add any region-specific new terms in region-specific class, using coefficient numbers 91-97, 49, 99,100 or other unused variable #
      //Global.PrintFile.WriteLine("Default Nashville_WorkLocationModel.RegionSpecificCustomizations called");

      int dest_CBD = (zonedist == 1) ? 1 : 0;       //1-CBD
      int dest_IC_I24E = (zonedist == 2) ? 1 : 0;   //2-Inner Core I-24 E
      int dest_IC_I65N = (zonedist == 3) ? 1 : 0;   //3-Inner Core I-24 / 65 N
      int dest_IC_I24W = (zonedist == 4) ? 1 : 0;   //4-Inner Core I-24W
      int dest_IC_I40W = (zonedist == 5) ? 1 : 0;   //5-Inner Core I-40 W
      int dest_IC_I65S = (zonedist == 6) ? 1 : 0;   //6-Inner Core I-65 S
      int dest_MC_I24E = (zonedist == 7) ? 1 : 0;   //7-Middle Core I-24 E
      int dest_MC_I24W = (zonedist == 8) ? 1 : 0;   //8-Middle Core I-24 W
      int dest_MC_I40E = (zonedist == 9) ? 1 : 0;   //9-Middle Core I-40 E
      int dest_MC_I40W = (zonedist == 10) ? 1 : 0;  //10-Middle Core I-40 W
      int dest_MC_I65N = (zonedist == 11) ? 1 : 0;  //11-Middle Core I-65 N
      int dest_MC_I65S = (zonedist == 12) ? 1 : 0;  //12-Middle Core I-65 S
      int dest_OC_I24E = (zonedist == 13) ? 1 : 0;  //13-Outer Core I-24 E
      int dest_OC_I24W = (zonedist == 14) ? 1 : 0;  //14-Outer Core I-24 W
      int dest_OC_I40E = (zonedist == 15) ? 1 : 0;  //15-Outer Core I-40 E
      int dest_OC_I40W = (zonedist == 16) ? 1 : 0;  //16-Outer Core I-40 W
      int dest_OC_I65N = (zonedist == 17) ? 1 : 0;  //17-Outer Core I-65 N
      int dest_OC_I65S = (zonedist == 18) ? 1 : 0;  //18-Outer Core I-65 S
      int dest_OC_SR386 = (zonedist == 19) ? 1 : 0; //19-Outer Core SR386
      int dest_FOC_I65S = (zonedist == 20) ? 1 : 0; //20-Far Outer Core I-65 S
      int orig12_dest12 = (homedist == 12) && (zonedist == 12) ? 1 : 0;
      int orig13_dest13 = (homedist == 13) && (zonedist == 13) ? 1 : 0;

      alternative.AddUtilityTerm(101, dest_CBD);
      alternative.AddUtilityTerm(102, dest_IC_I24E);
      alternative.AddUtilityTerm(103, dest_IC_I65N);
      alternative.AddUtilityTerm(104, dest_IC_I24W);
      alternative.AddUtilityTerm(105, dest_IC_I40W);
      alternative.AddUtilityTerm(106, dest_IC_I65S);
      alternative.AddUtilityTerm(107, dest_MC_I24E);
      alternative.AddUtilityTerm(108, dest_MC_I24W);
      alternative.AddUtilityTerm(109, dest_MC_I40E);
      alternative.AddUtilityTerm(110, dest_MC_I40W);
      alternative.AddUtilityTerm(111, dest_MC_I65N);
      alternative.AddUtilityTerm(112, dest_MC_I65S);
      alternative.AddUtilityTerm(113, dest_OC_I24E);
      alternative.AddUtilityTerm(114, dest_OC_I24W);
      alternative.AddUtilityTerm(115, dest_OC_I40E);
      alternative.AddUtilityTerm(116, dest_OC_I40W);
      alternative.AddUtilityTerm(117, dest_OC_I65N);
      alternative.AddUtilityTerm(118, dest_OC_I65S);
      alternative.AddUtilityTerm(119, dest_OC_SR386);
      alternative.AddUtilityTerm(120, dest_FOC_I65S);
      alternative.AddUtilityTerm(121, orig12_dest12);
      alternative.AddUtilityTerm(122, orig13_dest13);

    }
  }
}
