// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.Framework.DomainModels.Models {
  public interface IHalfTour {
    List<ITripWrapper> Trips { get; }

    int SimulatedTrips { get; set; }

    int OneSimulatedTripFlag { get; }

    int TwoSimulatedTripsFlag { get; }

    int ThreeSimulatedTripsFlag { get; }

    int FourSimulatedTripsFlag { get; }

    int FiveSimulatedTripsFlag { get; }

    int FivePlusSimulatedTripsFlag { get; }

    void SetTrips(int direction);

    ITripWrapper CreateNextTrip(ITripWrapper trip, int intermediateStopPurpose, int destinationPurpose);
  }
}