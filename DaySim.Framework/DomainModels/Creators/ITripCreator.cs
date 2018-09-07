// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.Framework.DomainModels.Creators {
  public interface ITripCreator : ICreator {
    ITrip CreateModel();

    ITripWrapper CreateWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour);

    ITripWrapper CreateWrapper(ITourWrapper tourWrapper, int nextTripId, int direction, int sequence, bool isToTourOrigin, IHalfTour halfTour);

    ITripWrapper CreateWrapper(ITourWrapper tourWrapper, ITripWrapper trip, int nextTripId, int intermediateStopPurpose, int destinationPurpose, IHalfTour halfTour);
  }
}