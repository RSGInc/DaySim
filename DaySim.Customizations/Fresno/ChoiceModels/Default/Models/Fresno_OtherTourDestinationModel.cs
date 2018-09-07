using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class Fresno_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel) {


      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
      //Global.PrintFile.WriteLine("Fresno_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
      int origdist = _tour.OriginParcel.District;
      //var destZone = destinationParcel.ZoneId;
      int destParcel = destinationParcel.Id;

      //var destRiverPark = (destZone == 514 || destZone == 515 || destZone == 516 || destZone == 1486 || destZone == 1487) ? 1 : 0;
      int destRiverPark = (destParcel == 19599 || destParcel == 19600 || destParcel == 19601 || destParcel == 19598 || destParcel == 12156 || destParcel == 12158 || destParcel == 19603) ? 1 : 0;
      int origDistDestRiverPark = (origdist == 4 || origdist == 7 || origdist == 10) && (destParcel == 19599 || destParcel == 19600 || destParcel == 19601 || destParcel == 19598 || destParcel == 12156 || destParcel == 12158 || destParcel == 19603) ? 1 : 0;
      int origEastDestRiverPark = (origdist == 5) && (destParcel == 19599 || destParcel == 19600 || destParcel == 19601 || destParcel == 19598 || destParcel == 12156 || destParcel == 12158 || destParcel == 19603) ? 1 : 0;

      if (_tour.DestinationPurpose == Global.Settings.Purposes.Shopping || _tour.DestinationPurpose == Global.Settings.Purposes.Meal) {
        //Global.PrintFile.WriteLine("Destination is River Park Mall");
        alternative.AddUtilityTerm(115, destRiverPark);
        alternative.AddUtilityTerm(116, origDistDestRiverPark);
        alternative.AddUtilityTerm(117, origEastDestRiverPark);

      }

      //alternative.AddUtilityTerm(115, origEastDestCBD);
      //alternative.AddUtilityTerm(116, origKitDestTRP);
      //alternative.AddUtilityTerm(117, origTacDestKit);
      //alternative.AddUtilityTerm(118, origKitDestNotKit);
      //alternative.AddUtilityTerm(119, origSTacWorkCBD);
    }
  }
}
