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
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;

namespace Daysim.DomainModels.Creators {
	[UsedImplicitly]
	[Factory(Factory.WrapperFactory, Category = Category.Creator)]
	public class JointTourCreator<TWrapper, TModel> : IJointTourCreator where TWrapper : IJointTourWrapper where TModel : IJointTour, new() {
		IJointTour IJointTourCreator.CreateModel() {
			return CreateModel();
		}

		private static TModel CreateModel() {
			return new TModel();
		}

		IJointTourWrapper IJointTourCreator.CreateWrapper(IJointTour jointTour, IHouseholdDayWrapper householdDayWrapper) {
			return CreateWrapper(jointTour, householdDayWrapper);
		}

		private static TWrapper CreateWrapper(IJointTour jointTour, IHouseholdDayWrapper householdDayWrapper) {
			var type = typeof (TWrapper);
			var instance = Activator.CreateInstance(type, jointTour, householdDayWrapper);

			return (TWrapper) instance;
		}
	}
}