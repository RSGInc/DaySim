// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models;
using DaySim.Framework.Core;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Actum {
    public static class ModelModule {
        public static void registerDependencies() {

            Global.ContainerDaySim.RegisterSingleton<Reader<Parcel>>(new Reader<Parcel>(Global.WorkingParcelPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<ParcelNode>>(new Reader<ParcelNode>(Global.WorkingParcelNodePath));

            Global.ContainerDaySim.RegisterSingleton<Reader<ParkAndRideNode>>(new Reader<ParkAndRideNode>(Global.WorkingParkAndRideNodePath));

            Global.ContainerDaySim.RegisterSingleton<Reader<TransitStopArea>>(new Reader<TransitStopArea>(Global.WorkingTransitStopAreaPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<Zone>>(new Reader<Zone>(Global.WorkingZonePath));

            Global.ContainerDaySim.RegisterSingleton<Reader<Household>>(new Reader<Household>(Global.WorkingHouseholdPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<Person>>(new Reader<Person>(Global.WorkingPersonPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<HouseholdDay>>(new Reader<HouseholdDay>(Global.WorkingHouseholdDayPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<PersonDay>>(new Reader<PersonDay>(Global.WorkingPersonDayPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<Tour>>(new Reader<Tour>(Global.WorkingTourPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<Trip>>(new Reader<Trip>(Global.WorkingTripPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<JointTour>>(new Reader<JointTour>(Global.WorkingJointTourPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<FullHalfTour>>(new Reader<FullHalfTour>(Global.WorkingFullHalfTourPath));

            Global.ContainerDaySim.RegisterSingleton<Reader<PartialHalfTour>>(new Reader<PartialHalfTour>(Global.WorkingPartialHalfTourPath));
        }
    }
}