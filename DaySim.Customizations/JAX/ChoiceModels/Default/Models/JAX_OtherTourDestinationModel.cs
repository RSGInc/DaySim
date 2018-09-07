using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.Default.Models {
  internal class JAX_OtherTourDestinationModel : OtherTourDestinationModel {

    protected override void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper _tour, IParcelWrapper destinationParcel) {


      //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
      //Global.PrintFile.WriteLine("JAX_OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients called");
      double crossriver = ImpedanceRoster.GetValue("crossriver", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot,
                1, _tour.OriginParcel, destinationParcel).Variable;
      double intracounty = ImpedanceRoster.GetValue("intracounty", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot,
                1, _tour.OriginParcel, destinationParcel).Variable;


      if (_tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
        alternative.AddUtilityTerm(114, crossriver);
      } else if (_tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness || _tour.DestinationPurpose == Global.Settings.Purposes.Medical) {
        alternative.AddUtilityTerm(115, crossriver);
        alternative.AddUtilityTerm(119, intracounty);
      } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
        alternative.AddUtilityTerm(116, crossriver);
        alternative.AddUtilityTerm(119, intracounty);
      } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Meal) {
        alternative.AddUtilityTerm(117, crossriver);
        alternative.AddUtilityTerm(119, intracounty);
      } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Social || _tour.DestinationPurpose == Global.Settings.Purposes.Recreation) {
        alternative.AddUtilityTerm(118, crossriver);
        alternative.AddUtilityTerm(119, intracounty);
      }

    }
  }
}
