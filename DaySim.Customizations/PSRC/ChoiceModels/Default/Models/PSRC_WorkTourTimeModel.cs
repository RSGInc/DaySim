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
      int SeaTacAirportZone = (Global.Configuration.SeaTacAirportZoneIndex + 1);
      int SeaTacEmpZone1 = Global.Configuration.SeaTacEmpZone1;
      int SeaTacEmpZone2 = Global.Configuration.SeaTacEmpZone2;
      int SeaTacEmpZone3 = Global.Configuration.SeaTacEmpZone3;

      //SeaTac specific constants for airport work trips
      if (tour.IsWorkPurpose() && (tourDestinationZone == SeaTacAirportZone || tourDestinationZone == SeaTacEmpZone1 || tourDestinationZone == SeaTacEmpZone2 || tourDestinationZone == SeaTacEmpZone3) && tour.IsParkAndRideMode()) {

        arrivalComponent.AddUtilityTerm(522, arrivalPeriod.Middle.IsBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.FiveAM).ToFlag()); // 3-5
        arrivalComponent.AddUtilityTerm(511, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FiveAM, Global.Settings.Times.SixAM).ToFlag()); //5-6
        arrivalComponent.AddUtilityTerm(512, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.SevenAM).ToFlag()); //6-7
        arrivalComponent.AddUtilityTerm(513, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.EightAM).ToFlag()); //7-8
        arrivalComponent.AddUtilityTerm(514, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.NineAM).ToFlag()); //8-9
        arrivalComponent.AddUtilityTerm(515, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.TenAM).ToFlag()); //9-10
        arrivalComponent.AddUtilityTerm(516, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenAM, Global.Settings.Times.TwoPM).ToFlag()); //10-14
        arrivalComponent.AddUtilityTerm(517, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TwoPM, Global.Settings.Times.ThreePM).ToFlag()); //14-15
        arrivalComponent.AddUtilityTerm(518, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.FourPM).ToFlag()); //15-16
        arrivalComponent.AddUtilityTerm(519, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.FivePM).ToFlag()); //16-17
        arrivalComponent.AddUtilityTerm(520, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FivePM, Global.Settings.Times.SixPM).ToFlag()); //17-18
        arrivalComponent.AddUtilityTerm(521, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.EightPM).ToFlag()); //18-20
        arrivalComponent.AddUtilityTerm(522, arrivalPeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.EightPM, Global.Settings.Times.MinutesInADay).ToFlag()); //20-3

        departureComponent.AddUtilityTerm(622, departurePeriod.Middle.IsBetween(Global.Settings.Times.ThreeAM, Global.Settings.Times.FiveAM).ToFlag()); // 3-5
        departureComponent.AddUtilityTerm(611, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FiveAM, Global.Settings.Times.SixAM).ToFlag()); //5-6
        departureComponent.AddUtilityTerm(612, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.SevenAM).ToFlag()); //6-7
        departureComponent.AddUtilityTerm(613, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SevenAM, Global.Settings.Times.EightAM).ToFlag()); //7-8
        departureComponent.AddUtilityTerm(614, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.EightAM, Global.Settings.Times.NineAM).ToFlag()); //8-9
        departureComponent.AddUtilityTerm(615, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.TenAM).ToFlag()); //9-10
        departureComponent.AddUtilityTerm(616, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TenAM, Global.Settings.Times.TwoPM).ToFlag()); //10-14
        departureComponent.AddUtilityTerm(617, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.TwoPM, Global.Settings.Times.ThreePM).ToFlag()); //14-15
        departureComponent.AddUtilityTerm(618, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.FourPM).ToFlag()); //15-16
        departureComponent.AddUtilityTerm(619, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FourPM, Global.Settings.Times.FivePM).ToFlag()); //16-17
        departureComponent.AddUtilityTerm(620, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.FivePM, Global.Settings.Times.SixPM).ToFlag()); //17-18
        departureComponent.AddUtilityTerm(621, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.SixPM, Global.Settings.Times.EightPM).ToFlag()); //18-20
        departureComponent.AddUtilityTerm(622, departurePeriod.Middle.IsLeftExclusiveBetween(Global.Settings.Times.EightPM, Global.Settings.Times.MinutesInADay).ToFlag()); //20-3

      };
    }
  }
}
