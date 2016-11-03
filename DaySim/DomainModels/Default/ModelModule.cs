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
using SimpleInjector;

namespace DaySim.DomainModels.Default {
    public static class ModelModule {
        public static void registerDependencies() {

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<Parcel>>(new Reader<Parcel>(Global.WorkingParcelPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<ParcelNode>>(new Reader<ParcelNode>(Global.WorkingParcelNodePath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<ParkAndRideNode>>(new Reader<ParkAndRideNode>(Global.WorkingParkAndRideNodePath));


            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<TransitStopArea>>(new Reader<TransitStopArea>(Global.WorkingTransitStopAreaPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<Zone>>(new Reader<Zone>(Global.WorkingZonePath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<Household>>(new Reader<Household>(Global.WorkingHouseholdPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<Person>>(new Reader<Person>(Global.WorkingPersonPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<HouseholdDay>>(new Reader<HouseholdDay>(Global.WorkingHouseholdDayPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<PersonDay>>(new Reader<PersonDay>(Global.WorkingPersonDayPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<Tour>>(new Reader<Tour>(Global.WorkingTourPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<Trip>>(new Reader<Trip>(Global.WorkingTripPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<JointTour>>(new Reader<JointTour>(Global.WorkingJointTourPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<FullHalfTour>>(new Reader<FullHalfTour>(Global.WorkingFullHalfTourPath));

            Global.ContainerWorkingPathReaders.RegisterSingleton<Reader<PartialHalfTour>>(new Reader<PartialHalfTour>(Global.WorkingPartialHalfTourPath));

            Global.ContainerWorkingPathReaders.Verify();
        }
    }
}