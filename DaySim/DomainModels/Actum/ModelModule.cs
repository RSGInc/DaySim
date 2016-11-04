// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Default.Models;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;
using System;

namespace DaySim.DomainModels.Actum {
    [UsedImplicitly]
    [Factory(Factory.ModuleFactory, DataType = DataType.Default)]
    public class ModelModule {
        public ModelModule() {
            Global.ContainerDaySim.RegisterSingleton<Func<Reader<Parcel>>>(() => new Reader<Parcel>(Global.WorkingParcelPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<ParcelNode>>>(() => new Reader<ParcelNode>(Global.WorkingParcelNodePath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<ParkAndRideNode>>>(() => new Reader<ParkAndRideNode>(Global.WorkingParkAndRideNodePath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<TransitStopArea>>>(() => new Reader<TransitStopArea>(Global.WorkingTransitStopAreaPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<Zone>>>(() => new Reader<Zone>(Global.WorkingZonePath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<Household>>>(() => new Reader<Household>(Global.WorkingHouseholdPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<Person>>>(() => new Reader<Person>(Global.WorkingPersonPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<HouseholdDay>>>(() => new Reader<HouseholdDay>(Global.WorkingHouseholdDayPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<PersonDay>>>(() => new Reader<PersonDay>(Global.WorkingPersonDayPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<Tour>>>(() => new Reader<Tour>(Global.WorkingTourPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<Trip>>>(() => new Reader<Trip>(Global.WorkingTripPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<JointTour>>>(() => new Reader<JointTour>(Global.WorkingJointTourPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<FullHalfTour>>>(() => new Reader<FullHalfTour>(Global.WorkingFullHalfTourPath));

            Global.ContainerDaySim.RegisterSingleton<Func<Reader<PartialHalfTour>>>(() => new Reader<PartialHalfTour>(Global.WorkingPartialHalfTourPath));
        }   //end constructor
    }   //end class
}   //end namespace