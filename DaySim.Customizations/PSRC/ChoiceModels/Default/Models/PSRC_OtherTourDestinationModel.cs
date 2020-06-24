using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.Core;
using System;
namespace DaySim.ChoiceModels.Default.Models {
  internal class PSRC_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel, IPersonWrapper person) {


      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
      //Global.PrintFile.WriteLine("PSRC_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
      int origdist = _tour.OriginParcel.District;
      int destdist = destinationParcel.District;
      int origKitDestTRP = (origdist == 9 || origdist == 11) && (destdist == 8 || destdist == 10 || destdist == 7) ? 1 : 0;
      int origEastDestCBD = origdist == 6 && destdist == 4 ? 1 : 0;
      int origTacDestKit = origdist == 8 && destdist == 9 || destdist == 11 ? 1 : 0;
      int origKitDestNotKit = (origdist == 9 || origdist == 11) && (destdist != 9 && destdist != 11) ? 1 : 0;
      int origSTacWorkCBD = (origdist == 11 && destdist == 4) ? 1 : 0;
      double distanceFromOrigin = _tour.OriginParcel.DistanceFromOrigin(destinationParcel, _tour.DestinationArrivalTime);
      double distanceFromOriginLog = Math.Log(1 + distanceFromOrigin);
      alternative.AddUtilityTerm(114, person.WorksAtHome().ToFlag() * distanceFromOriginLog);
      alternative.AddUtilityTerm(115, origEastDestCBD);
      alternative.AddUtilityTerm(116, origKitDestTRP);
      alternative.AddUtilityTerm(117, origTacDestKit);
      alternative.AddUtilityTerm(118, origKitDestNotKit);
      alternative.AddUtilityTerm(119, origSTacWorkCBD);
    }
  }
}
