using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  internal class PSRC_WorkTourTimeModel : WorkTourTimeModel {
    protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Component arrivalComponent, ChoiceProbabilityCalculator.Component departureComponent, ITourWrapper tour, int arrivalPeriodIndex, int departurePeriodIndex) {
      //Global.PrintFile.WriteLine("Default PSRC_WorkTourModeModel.RegionSpecificCustomizations called");
      int tourDestinationZone = tour.DestinationZoneKey;
      MinuteSpan arrivalPeriod = DayPeriod.SmallDayPeriods[arrivalPeriodIndex];
      MinuteSpan departurePeriod = DayPeriod.SmallDayPeriods[departurePeriodIndex];

      //SeaTac specific constants for airport work trips
      if (tour.IsWorkPurpose() && tourDestinationZone == 1 && tour.IsParkAndRideMode()) {

        arrivalComponent.AddUtilityTerm(511, arrivalPeriod.Middle.IsBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.SixAM).ToFlag());
        arrivalComponent.AddUtilityTerm(512, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.SevenAM).ToFlag());
        arrivalComponent.AddUtilityTerm(513, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.EightAM).ToFlag());
        arrivalComponent.AddUtilityTerm(514, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.NineAM).ToFlag());
        arrivalComponent.AddUtilityTerm(515, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.TenAM).ToFlag());
        arrivalComponent.AddUtilityTerm(516, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenAM, Global.Settings.Times.OnePM).ToFlag());
        arrivalComponent.AddUtilityTerm(517, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.OnePM, Global.Settings.Times.FourPM).ToFlag());
        arrivalComponent.AddUtilityTerm(518, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.SevenPM).ToFlag());
        arrivalComponent.AddUtilityTerm(519, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenPM, Global.Settings.Times.TenPM).ToFlag());
        arrivalComponent.AddUtilityTerm(520, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenPM, Global.Settings.Times.MinutesInADay).ToFlag());

      };
    }
  }
}
