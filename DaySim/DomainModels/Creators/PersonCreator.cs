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
  public class PersonCreator<TWrapper, TModel> : IPersonCreator where TWrapper : IPersonWrapper where TModel : IPerson, new() {
    IPerson IPersonCreator.CreateModel() {
      return CreateModel();
    }

    private static TModel CreateModel() {
      return new TModel();
    }

    IPersonWrapper IPersonCreator.CreateWrapper(IPerson person, IHouseholdWrapper householdWrapper) {
      return CreateWrapper(person, householdWrapper);
    }

    private static TWrapper CreateWrapper(IPerson person, IHouseholdWrapper householdWrapper) {
      Type type = typeof(TWrapper);
      object instance = Activator.CreateInstance(type, person, householdWrapper);

      return (TWrapper)instance;
    }
  }
}