// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface ITrip : IModel {
    int TourId { get; set; }

    int HouseholdId { get; set; }

    int PersonSequence { get; set; }

    int Day { get; set; }

    int TourSequence { get; set; }

    int Direction { get; set; }

    int Sequence { get; set; }

    int SurveyTripSequence { get; set; }

    int OriginPurpose { get; set; }

    int DestinationPurpose { get; set; }

    int OriginAddressType { get; set; }

    int DestinationAddressType { get; set; }

    int OriginParcelId { get; set; }

    int OriginZoneKey { get; set; }

    int DestinationParcelId { get; set; }

    int DestinationZoneKey { get; set; }

    int Mode { get; set; }

    int PathType { get; set; }

    int DriverType { get; set; }

    int DepartureTime { get; set; }

    int ArrivalTime { get; set; }

    int ActivityEndTime { get; set; }

    double TravelTime { get; set; }

    double TravelCost { get; set; }

    double TravelDistance { get; set; }

    double ValueOfTime { get; set; }

    double ExpansionFactor { get; set; }
  }
}