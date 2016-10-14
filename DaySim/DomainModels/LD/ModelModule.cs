// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using Daysim.DomainModels.LD.Models;
using Daysim.Framework.Core;
using Daysim.Framework.Factories;
using Daysim.Framework.Persistence;
using Ninject.Modules;

namespace Daysim.DomainModels.LD {
	[UsedImplicitly]
	[Factory(Factory.ModuleFactory, DataType = DataType.LD)]
	public class ModelModule : NinjectModule {
		public override void Load() {
			Global.Container.Register<Reader<Parcel>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingParcelPath);

			Global.Container.Register<Reader<ParcelNode>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingParcelNodePath);

			Global.Container.Register<Reader<ParkAndRideNode>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingParkAndRideNodePath);

			Global.Container.Register<Reader<TransitStopArea>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingTransitStopAreaPath);

			Global.Container.Register<Reader<Zone>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingZonePath);

			Global.Container.Register<Reader<Household>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingHouseholdPath);

			Global.Container.Register<Reader<Person>>().
				ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingPersonPath);

			Global.Container.Register<Reader<HouseholdDay>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingHouseholdDayPath);

			Global.Container.Register<Reader<PersonDay>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingPersonDayPath);

			Global.Container.Register<Reader<Tour>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingTourPath);

			Global.Container.Register<Reader<Trip>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingTripPath);

			Global.Container.Register<Reader<JointTour>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingJointTourPath);

			Global.Container.Register<Reader<FullHalfTour>>()
				.ToSelf()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingFullHalfTourPath);

			Global.Container.Register<Reader<PartialHalfTour>>()()
				(Lifestyle.Singleton)
				.WithConstructorArgument("path", Global.WorkingPartialHalfTourPath);
		}
	}
}