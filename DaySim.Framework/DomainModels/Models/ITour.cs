// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface ITour : IModel {
    int PersonId { get; set; }

    int PersonDayId { get; set; }

    int HouseholdId { get; set; }

    int PersonSequence { get; set; }

    int Day { get; set; }

    int Sequence { get; set; }

    int JointTourSequence { get; set; }

    int ParentTourSequence { get; set; }

    int TotalSubtours { get; set; }

    int DestinationPurpose { get; set; }

    int OriginDepartureTime { get; set; }

    int DestinationArrivalTime { get; set; }

    int DestinationDepartureTime { get; set; }

    int OriginArrivalTime { get; set; }

    int OriginAddressType { get; set; }

    int DestinationAddressType { get; set; }

    int OriginParcelId { get; set; }

    int OriginZoneKey { get; set; }

    int DestinationParcelId { get; set; }

    int DestinationZoneKey { get; set; }

    int Mode { get; set; }

    int PathType { get; set; }

    double AutoTimeOneWay { get; set; }

    double AutoCostOneWay { get; set; }

    double AutoDistanceOneWay { get; set; }

    int HalfTour1Trips { get; set; }

    int HalfTour2Trips { get; set; }

    int PartialHalfTour1Sequence { get; set; }

    int PartialHalfTour2Sequence { get; set; }

    int FullHalfTour1Sequence { get; set; }

    int FullHalfTour2Sequence { get; set; }

    double ExpansionFactor { get; set; }
  }
}