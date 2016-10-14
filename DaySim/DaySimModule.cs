// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.AggregateLogsums;
using DaySim.DomainModels.Factories;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;
using DaySim.Framework.Roster;
using DaySim.Sampling;
using SimpleInjector;

namespace DaySim {
	public static class DaySimModule {
		public static void registerDependencies() {
			Global.Container.Register<ImporterFactory>(Lifestyle.Singleton);

			Global.Container.Register<ExporterFactory>(Lifestyle.Singleton);

            Global.Container.Register<IPersistenceFactory<IParcel>,PersistenceFactory<IParcel>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IParcelNode>,PersistenceFactory<IParcelNode>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IParkAndRideNode>,PersistenceFactory<IParkAndRideNode>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<ITransitStopArea>,PersistenceFactory<ITransitStopArea>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IZone>,PersistenceFactory<IZone>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IHousehold>,PersistenceFactory<IHousehold>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IPerson>,PersistenceFactory<IPerson>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IHouseholdDay>,PersistenceFactory<IHouseholdDay>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IPersonDay>,PersistenceFactory<IPersonDay>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<ITour>,PersistenceFactory<ITour>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<ITrip>,PersistenceFactory<ITrip>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IJointTour>,PersistenceFactory<IJointTour>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IFullHalfTour>,PersistenceFactory<IFullHalfTour>>(Lifestyle.Singleton);

			Global.Container.Register<IPersistenceFactory<IPartialHalfTour>,PersistenceFactory<IPartialHalfTour>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IParcelCreator>,WrapperFactory<IParcelWrapper, IParcelCreator, IParcel>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IParcelNodeCreator>,WrapperFactory<IParcelNodeWrapper, IParcelNodeCreator, IParcelNode>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IParkAndRideNodeCreator>,WrapperFactory<IParkAndRideNodeWrapper, IParkAndRideNodeCreator, IParkAndRideNode>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<ITransitStopAreaCreator>,WrapperFactory<ITransitStopAreaWrapper, ITransitStopAreaCreator, ITransitStopArea>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IZoneCreator>,WrapperFactory<IZoneWrapper, IZoneCreator, IZone>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IHouseholdCreator>,WrapperFactory<IHouseholdWrapper, IHouseholdCreator, IHousehold>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IPersonCreator>,WrapperFactory<IPersonWrapper, IPersonCreator, IPerson>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IHouseholdDayCreator>,WrapperFactory<IHouseholdDayWrapper, IHouseholdDayCreator, IHouseholdDay>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IPersonDayCreator>,WrapperFactory<IPersonDayWrapper, IPersonDayCreator, IPersonDay>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<ITourCreator>,WrapperFactory<ITourWrapper, ITourCreator, ITour>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<ITripCreator>,WrapperFactory<ITripWrapper, ITripCreator, ITrip>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IJointTourCreator>,WrapperFactory<IJointTourWrapper, IJointTourCreator, IJointTour>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IFullHalfTourCreator>,WrapperFactory<IFullHalfTourWrapper, IFullHalfTourCreator, IFullHalfTour>>(Lifestyle.Singleton);

			Global.Container.Register<IWrapperFactory<IPartialHalfTourCreator>,WrapperFactory<IPartialHalfTourWrapper, IPartialHalfTourCreator, IPartialHalfTour>>(Lifestyle.Singleton);

			Global.Container.Register<SkimFileReaderFactory>(Lifestyle.Singleton);

			Global.Container.Register<SamplingWeightsSettingsFactory>(Lifestyle.Singleton);

			Global.Container.Register<AggregateLogsumsCalculatorFactory>(Lifestyle.Singleton);
		}
	}
}