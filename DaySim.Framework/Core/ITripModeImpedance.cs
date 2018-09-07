namespace DaySim.Framework.Core {
  public interface ITripModeImpedance {
    int AdjacentMinutesBefore { get; set; }

    int MaxMinutesBefore { get; set; }

    int TotalMinutesBefore { get; set; }

    int AdjacentMinutesAfter { get; set; }

    int MaxMinutesAfter { get; set; }

    int TotalMinutesAfter { get; set; }

    double TravelTime { get; set; }

    double GeneralizedTime { get; set; }

    double TravelCost { get; set; }

    double TravelDistance { get; set; }

    int PathType { get; set; }
  }
}
