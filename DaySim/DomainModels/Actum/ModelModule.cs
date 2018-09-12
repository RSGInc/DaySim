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
using Ninject.Modules;

namespace DaySim.DomainModels.Actum {
  [UsedImplicitly]
  [Factory(Factory.ModuleFactory, DataType = DataType.Actum)]
  public class ModelModule : NinjectModule {
    public override void Load() {
      Bind<Reader<Parcel>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingParcelPath);

      Bind<Reader<ParcelNode>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingParcelNodePath);

      Bind<Reader<ParkAndRideNode>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingParkAndRideNodePath);

      Bind<Reader<TransitStopArea>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingTransitStopAreaPath);

      Bind<Reader<Zone>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingZonePath);

      Bind<Reader<Household>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingHouseholdPath);

      Bind<Reader<Person>>().
          ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingPersonPath);

      Bind<Reader<HouseholdDay>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingHouseholdDayPath);

      Bind<Reader<PersonDay>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingPersonDayPath);

      Bind<Reader<Tour>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingTourPath);

      Bind<Reader<Trip>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingTripPath);

      Bind<Reader<JointTour>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingJointTourPath);

      Bind<Reader<FullHalfTour>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingFullHalfTourPath);

      Bind<Reader<PartialHalfTour>>()
          .ToSelf()
          .InSingletonScope()
          .WithConstructorArgument("path", Global.WorkingPartialHalfTourPath);
    }
  }
}