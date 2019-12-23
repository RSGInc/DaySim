// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using DaySim.DomainModels.Actum.Models;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Actum {
  [UsedImplicitly]
  [Factory(Factory.ModuleFactory, DataType = DataType.Actum)]
  public class ModelModule {
    public ModelModule() {
      Global.ContainerDaySim.RegisterInstance<Func<Reader<Parcel>>>(() => new Reader<Parcel>(Global.WorkingParcelPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<ParcelNode>>>(() => new Reader<ParcelNode>(Global.WorkingParcelNodePath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<DestinationParkingNode>>>(() => new Reader<DestinationParkingNode>(Global.WorkingDestinationParkingNodePath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<ParkAndRideNode>>>(() => new Reader<ParkAndRideNode>(Global.WorkingParkAndRideNodePath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<TransitStopArea>>>(() => new Reader<TransitStopArea>(Global.WorkingTransitStopAreaPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<Zone>>>(() => new Reader<Zone>(Global.WorkingZonePath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<Household>>>(() => new Reader<Household>(Global.WorkingHouseholdPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<Person>>>(() => new Reader<Person>(Global.WorkingPersonPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<HouseholdDay>>>(() => new Reader<HouseholdDay>(Global.WorkingHouseholdDayPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<PersonDay>>>(() => new Reader<PersonDay>(Global.WorkingPersonDayPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<Tour>>>(() => new Reader<Tour>(Global.WorkingTourPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<Trip>>>(() => new Reader<Trip>(Global.WorkingTripPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<JointTour>>>(() => new Reader<JointTour>(Global.WorkingJointTourPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<FullHalfTour>>>(() => new Reader<FullHalfTour>(Global.WorkingFullHalfTourPath));

      Global.ContainerDaySim.RegisterInstance<Func<Reader<PartialHalfTour>>>(() => new Reader<PartialHalfTour>(Global.WorkingPartialHalfTourPath));
    }   //end constructor
  }   //end class
}   //end namespace
