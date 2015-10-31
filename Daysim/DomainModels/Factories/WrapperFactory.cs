// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Creators;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.Factories;

namespace Daysim.DomainModels.Factories {
	public class WrapperFactory<TWrapper, TCreator, TModel> : IWrapperFactory<TCreator> where TCreator : ICreator where TModel : IModel {
		public TCreator Creator { get; private set; }

		public void Initialize(Configuration configuration) {
			var helper = new FactoryHelper(configuration);

			var type1 = helper.Wrapper.GetWrapperType<TWrapper>();
			var type2 = helper.Wrapper.GetCreatorType<TWrapper>();
			var type3 = helper.Persistence.GetModelType<TModel>();

			var args = new[] {type1, type3};
			var constructed = type2.MakeGenericType(args);

			Creator = (TCreator) Activator.CreateInstance(constructed);
		}
	}
}