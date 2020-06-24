using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.Core;
using System;
namespace DaySim.ChoiceModels.Default.Models
{
  internal class PSRC_WorkTourDestinationModel : WorkTourDestinationModel
  {

    protected override void RegionSpecificWorkTourCustomCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel, IPersonWrapper person)
    {


      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
      //Global.PrintFile.WriteLine("PSRC_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
      double distanceFromOrigin = _tour.OriginParcel.DistanceFromOrigin(destinationParcel, _tour.DestinationArrivalTime);
      double distanceFromOriginLog = Math.Log(1 + distanceFromOrigin);
      alternative.AddUtilityTerm(100, person.WorksAtHome().ToFlag() * distanceFromOriginLog);
      
    }
  }
}
