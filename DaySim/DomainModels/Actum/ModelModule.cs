// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;
using SimpleInjector;

namespace DaySim.DomainModels.Actum {
    public static class ModelModule {
        public static void registerDependencies() {

			Global.Container.RegisterSingleton<Reader<Parcel>>(new Reader<Parcel>(Global.WorkingParcelPath));

			Global.Container.RegisterSingleton<Reader<ParcelNode>>(new Reader<ParcelNode>(Global.WorkingParcelNodePath));

            Global.Container.RegisterSingleton<Reader<ParkAndRideNode>>(new Reader<ParkAndRideNode>(Global.WorkingParkAndRideNodePath));

            Global.Container.RegisterSingleton<Reader<TransitStopArea>>(new Reader<TransitStopArea>(Global.WorkingTransitStopAreaPath));

            Global.Container.RegisterSingleton<Reader<Zone>>(new Reader<Zone>(Global.WorkingZonePath));

            Global.Container.RegisterSingleton<Reader<Household>>(new Reader<Household>(Global.WorkingHouseholdPath));

            Global.Container.RegisterSingleton<Reader<Person>>(new Reader<Person>(Global.WorkingPersonPath));

            Global.Container.RegisterSingleton<Reader<HouseholdDay>>(new Reader<HouseholdDay>(Global.WorkingHouseholdDayPath));

            Global.Container.RegisterSingleton<Reader<PersonDay>>(new Reader<PersonDay>(Global.WorkingPersonDayPath));

            Global.Container.RegisterSingleton<Reader<Tour>>(new Reader<Tour>(Global.WorkingTourPath));

            Global.Container.RegisterSingleton<Reader<Trip>>(new Reader<Trip>(Global.WorkingTripPath));

            Global.Container.RegisterSingleton<Reader<JointTour>>(new Reader<JointTour>(Global.WorkingJointTourPath));

            Global.Container.RegisterSingleton<Reader<FullHalfTour>>(new Reader<FullHalfTour>(Global.WorkingFullHalfTourPath));

            Global.Container.RegisterSingleton<Reader<PartialHalfTour>>(new Reader<PartialHalfTour>(Global.WorkingPartialHalfTourPath));
		}
	}
}