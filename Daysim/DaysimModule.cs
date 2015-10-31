// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using Daysim.AggregateLogsums;
using Daysim.DomainModels.Factories;
using Daysim.Framework.DomainModels.Creators;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;
using Daysim.Framework.Persistence;
using Daysim.Framework.Roster;
using Daysim.Sampling;
using Ninject.Modules;

namespace Daysim {
	public sealed class DaysimModule : NinjectModule {
		public override void Load() {
			Bind<ImporterFactory>()
				.ToSelf()
				.InSingletonScope();

			Bind<ExporterFactory>()
				.ToSelf()
				.InSingletonScope();

			Bind<IPersistenceFactory<IParcel>>()
				.To<PersistenceFactory<IParcel>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IParcelNode>>()
				.To<PersistenceFactory<IParcelNode>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IParkAndRideNode>>()
				.To<PersistenceFactory<IParkAndRideNode>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<ITransitStopArea>>()
				.To<PersistenceFactory<ITransitStopArea>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IZone>>()
				.To<PersistenceFactory<IZone>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IHousehold>>()
				.To<PersistenceFactory<IHousehold>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IPerson>>()
				.To<PersistenceFactory<IPerson>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IHouseholdDay>>()
				.To<PersistenceFactory<IHouseholdDay>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IPersonDay>>()
				.To<PersistenceFactory<IPersonDay>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<ITour>>()
				.To<PersistenceFactory<ITour>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<ITrip>>()
				.To<PersistenceFactory<ITrip>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IJointTour>>()
				.To<PersistenceFactory<IJointTour>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IFullHalfTour>>()
				.To<PersistenceFactory<IFullHalfTour>>()
				.InSingletonScope();

			Bind<IPersistenceFactory<IPartialHalfTour>>()
				.To<PersistenceFactory<IPartialHalfTour>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IParcelCreator>>()
				.To<WrapperFactory<IParcelWrapper, IParcelCreator, IParcel>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IParcelNodeCreator>>()
				.To<WrapperFactory<IParcelNodeWrapper, IParcelNodeCreator, IParcelNode>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IParkAndRideNodeCreator>>()
				.To<WrapperFactory<IParkAndRideNodeWrapper, IParkAndRideNodeCreator, IParkAndRideNode>>()
				.InSingletonScope();

			Bind<IWrapperFactory<ITransitStopAreaCreator>>()
				.To<WrapperFactory<ITransitStopAreaWrapper, ITransitStopAreaCreator, ITransitStopArea>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IZoneCreator>>()
				.To<WrapperFactory<IZoneWrapper, IZoneCreator, IZone>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IHouseholdCreator>>()
				.To<WrapperFactory<IHouseholdWrapper, IHouseholdCreator, IHousehold>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IPersonCreator>>()
				.To<WrapperFactory<IPersonWrapper, IPersonCreator, IPerson>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IHouseholdDayCreator>>()
				.To<WrapperFactory<IHouseholdDayWrapper, IHouseholdDayCreator, IHouseholdDay>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IPersonDayCreator>>()
				.To<WrapperFactory<IPersonDayWrapper, IPersonDayCreator, IPersonDay>>()
				.InSingletonScope();

			Bind<IWrapperFactory<ITourCreator>>()
				.To<WrapperFactory<ITourWrapper, ITourCreator, ITour>>()
				.InSingletonScope();

			Bind<IWrapperFactory<ITripCreator>>()
				.To<WrapperFactory<ITripWrapper, ITripCreator, ITrip>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IJointTourCreator>>()
				.To<WrapperFactory<IJointTourWrapper, IJointTourCreator, IJointTour>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IFullHalfTourCreator>>()
				.To<WrapperFactory<IFullHalfTourWrapper, IFullHalfTourCreator, IFullHalfTour>>()
				.InSingletonScope();

			Bind<IWrapperFactory<IPartialHalfTourCreator>>()
				.To<WrapperFactory<IPartialHalfTourWrapper, IPartialHalfTourCreator, IPartialHalfTour>>()
				.InSingletonScope();

			Bind<SkimFileReaderFactory>()
				.ToSelf()
				.InSingletonScope();

			Bind<SamplingWeightsSettingsFactory>()
				.ToSelf()
				.InSingletonScope();

			Bind<AggregateLogsumsCalculatorFactory>()
				.ToSelf()
				.InSingletonScope();
		}
	}
}