// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface IDestinationParkingNode : IModel, IPoint {
    int ParcelId { get; set; }

    int LocationType { get; set; }

    int ParkingType { get; set; }

    double MaxDuration { get; set; }

    double FreeDuration { get; set; }

    int Capacity { get; set; }

    double Price12AM { get; set; }

    double Price1AM { get; set; }

    double Price2AM { get; set; }

    double Price3AM { get; set; }

    double Price4AM { get; set; }

    double Price5AM { get; set; }

    double Price6AM { get; set; }

    double Price7AM { get; set; }

    double Price8AM { get; set; }

    double Price9AM { get; set; }

    double Price10AM { get; set; }

    double Price11AM { get; set; }

    double Price12PM { get; set; }

    double Price1PM { get; set; }

    double Price2PM { get; set; }

    double Price3PM { get; set; }

    double Price4PM { get; set; }

    double Price5PM { get; set; }

    double Price6PM { get; set; }

    double Price7PM { get; set; }

    double Price8PM { get; set; }

    double Price9PM { get; set; }

    double Price10PM { get; set; }

    double Price11PM { get; set; }

    double MinimumTime { get; set; }

    double MinimumPrice { get; set; }

    double FullDayPrice { get; set; }

    double MonthlyPassDayPrice { get; set; }

    int ResidentPermitPassAvailable { get; set; }

    double ResidentPermitPassDayPrice { get; set; }

    int OpeningTime { get; set; }

    int ClosingTime { get; set; }

    int OccupancyMorning { get; set; }

    int OccupancyAfternoon { get; set; }

    int OccupancyNight { get; set; }

  }
}
