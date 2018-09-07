// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Creators {
  [UsedImplicitly]
  [Factory(Factory.WrapperFactory, Category = Category.Creator)]
  public class TripCreator<TWrapper, TModel> : ITripCreator where TWrapper : ITripWrapper where TModel : ITrip, new() {
    ITrip ITripCreator.CreateModel() {
      return CreateModel();
    }

    private static TModel CreateModel() {
      return new TModel();
    }

    ITripWrapper ITripCreator.CreateWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour) {
      return CreateWrapper(trip, tourWrapper, halfTour);
    }

    private static TWrapper CreateWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour) {
      Type type = typeof(TWrapper);
      object instance = Activator.CreateInstance(type, trip, tourWrapper, halfTour);

      return (TWrapper)instance;
    }

    ITripWrapper ITripCreator.CreateWrapper(ITourWrapper tourWrapper, int nextTripId, int direction, int sequence, bool isToTourOrigin, IHalfTour halfTour) {
      return CreateWrapper(tourWrapper, nextTripId, direction, sequence, isToTourOrigin, halfTour);
    }

    private static TWrapper CreateWrapper(ITourWrapper tourWrapper, int nextTripId, int direction, int sequence, bool isToTourOrigin, IHalfTour halfTour) {
      TModel t = new TModel {
        Id = nextTripId,
        TourId = tourWrapper.Id,
        HouseholdId = tourWrapper.HouseholdId,
        PersonSequence = tourWrapper.PersonSequence,
        Day = tourWrapper.Day,
        Direction = direction,
        Sequence = sequence + 1,
        OriginAddressType = tourWrapper.DestinationAddressType,
        OriginParcelId = tourWrapper.DestinationParcelId,
        OriginZoneKey = tourWrapper.DestinationZoneKey,
        OriginPurpose = tourWrapper.DestinationPurpose,
        DestinationAddressType = tourWrapper.OriginAddressType,
        DestinationParcelId = tourWrapper.OriginParcelId,
        DestinationZoneKey = tourWrapper.OriginZoneKey,
        DestinationPurpose =
              tourWrapper.IsHomeBasedTour
                  ? Global.Settings.Purposes.NoneOrHome
                  : Global.Settings.Purposes.Work,
        DepartureTime = 180,
        ArrivalTime = 180,
        PathType = 1,
        ExpansionFactor = tourWrapper.Household.ExpansionFactor,
      };

      Type type = typeof(TWrapper);
      object instance = Activator.CreateInstance(type, t, tourWrapper, halfTour);
      TWrapper wrapper = (TWrapper)instance;

      wrapper.IsToTourOrigin = isToTourOrigin;

      return wrapper;
    }

    ITripWrapper ITripCreator.CreateWrapper(ITourWrapper tourWrapper, ITripWrapper trip, int nextTripId, int intermediateStopPurpose, int destinationPurpose, IHalfTour halfTour) {
      return CreateWrapper(tourWrapper, trip, nextTripId, intermediateStopPurpose, destinationPurpose, halfTour);
    }

    private static TWrapper CreateWrapper(ITourWrapper tourWrapper, ITripWrapper trip, int nextTripId, int intermediateStopPurpose, int destinationPurpose, IHalfTour halfTour) {
      TModel t = new TModel {
        Id = nextTripId,
        TourId = tourWrapper.Id,
        HouseholdId = tourWrapper.HouseholdId,
        PersonSequence = tourWrapper.PersonSequence,
        Day = tourWrapper.Day,
        Direction = trip.Direction,
        Sequence = trip.Sequence + 1,
        DestinationAddressType = trip.DestinationAddressType,
        DestinationParcelId = trip.DestinationParcelId,
        DestinationZoneKey = trip.DestinationZoneKey,
        OriginAddressType = trip.DestinationAddressType,
        OriginPurpose = intermediateStopPurpose,
        DestinationPurpose = destinationPurpose,
        DepartureTime = 180,
        ArrivalTime = 180,
        PathType = 1,
        ExpansionFactor = tourWrapper.Household.ExpansionFactor
      };

      Type type = typeof(TWrapper);
      object instance = Activator.CreateInstance(type, t, tourWrapper, halfTour);
      TWrapper wrapper = (TWrapper)instance;

      wrapper.IsToTourOrigin = trip.IsToTourOrigin;

      return wrapper;
    }
  }
}