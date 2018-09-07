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
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Factories {
  public class WrapperFactory<TWrapper, TCreator, TModel> : IWrapperFactory<TCreator> where TCreator : ICreator where TModel : IModel {
    public TCreator Creator { get; private set; }

    public void Initialize(Configuration configuration) {
      FactoryHelper helper = new FactoryHelper(configuration);

      Type type1 = helper.Wrapper.GetWrapperType<TWrapper>();
      Type type2 = helper.Wrapper.GetCreatorType<TWrapper>();
      Type type3 = helper.Persistence.GetModelType<TModel>();

      Type[] args = new[] { type1, type3 };
      Type constructed = type2.MakeGenericType(args);

      Creator = (TCreator)Activator.CreateInstance(constructed);
    }
  }
}