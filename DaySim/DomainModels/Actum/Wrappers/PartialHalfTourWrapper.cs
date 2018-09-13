// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class PartialHalfTourWrapper : Default.Wrappers.PartialHalfTourWrapper, IActumPartialHalfTourWrapper {
    private readonly IActumPartialHalfTour _partialHalfTour;

    [UsedImplicitly]
    public PartialHalfTourWrapper(IPartialHalfTour partialHalfTour, IHouseholdDayWrapper householdDayWrapper) : base(partialHalfTour, householdDayWrapper) {
      _partialHalfTour = (IActumPartialHalfTour)partialHalfTour;
    }
  }
}