// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Actum.Models.Interfaces {
  public interface IActumTrip : ITrip {
    int EscortedDestinationPurpose { get; set; }

    int BikePTCombination { get; set; }

    int AccessMode { get; set; }

    int AccessPathType { get; set; }

    double AccessTime { get; set; }

    double AccessCost { get; set; }

    double AccessDistance { get; set; }

    int AccessStopArea { get; set; }

    int EgressMode { get; set; }

    int EgressPathType { get; set; }

    double EgressTime { get; set; }

    double EgressCost { get; set; }

    double EgressDistance { get; set; }

    int EgressStopArea { get; set; }

  }
}