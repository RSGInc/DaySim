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
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class HouseholdWrapper : Default.Wrappers.HouseholdWrapper, IActumHouseholdWrapper {
    private readonly IActumHousehold _household;

    [UsedImplicitly]
    public HouseholdWrapper(IHousehold household) : base(household) {
      _household = (IActumHousehold)household;
    }

    #region domain model properies

    public int MunicipalCode {
      get => _household.MunicipalCode;
      set => _household.MunicipalCode = value;
    }

    public double StationDistance {
      get => _household.StationDistance;
      set => _household.StationDistance = value;
    }

    public int ParkingAvailability {
      get => _household.ParkingAvailability;
      set => _household.ParkingAvailability = value;
    }

    public int InternetPaymentMethod {
      get => _household.InternetPaymentMethod;
      set => _household.InternetPaymentMethod = value;
    }

    #endregion
  }
}