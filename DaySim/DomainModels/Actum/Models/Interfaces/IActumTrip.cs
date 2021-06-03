﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Actum.Models.Interfaces {
  public interface IActumTrip : ITrip {
    //JLB20160323
    long TripId { get; set; }

    int TourMode { get; set; }

    int AccessMode { get; set; }

    int AccessPathType { get; set; }

    double AccessTime { get; set; }

    double AccessCost { get; set; }

    double AccessDistance { get; set; }

    int AccessTerminalID { get; set; }

    int AccessTerminalParcelID { get; set; }

    int AccessTerminalZoneID { get; set; }

    int AccessParkingNodeID { get; set; }

    int EgressMode { get; set; }

    int EgressPathType { get; set; }

    double EgressTime { get; set; }

    double EgressCost { get; set; }

    double EgressDistance { get; set; }

    int EgressTerminalID { get; set; }

    int EgressTerminalParcelID { get; set; }

    int EgressTerminalZoneID { get; set; }

    int EgressParkingNodeID { get; set; }

    int AutoType { get; set; }

    int AutoOccupancy { get; set; }
  }
}
