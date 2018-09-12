// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class HouseholdDayWrapper : Default.Wrappers.HouseholdDayWrapper, IActumHouseholdDayWrapper {
    private IActumHouseholdDay _householdDay;

    [UsedImplicitly]
    public HouseholdDayWrapper(IHouseholdDay householdDay, IHouseholdWrapper householdWrapper) : base(householdDay, householdWrapper) {
      _householdDay = (IActumHouseholdDay)householdDay;
    }

    #region domain model properies

    public int SharedActivityHomeStays {
      get => _householdDay.SharedActivityHomeStays;
      set => _householdDay.SharedActivityHomeStays = value;
    }

    public int NumberInLargestSharedHomeStay {
      get => _householdDay.NumberInLargestSharedHomeStay;
      set => _householdDay.NumberInLargestSharedHomeStay = value;
    }

    public int StartingMinuteSharedHomeStay {
      get => _householdDay.StartingMinuteSharedHomeStay;
      set => _householdDay.StartingMinuteSharedHomeStay = value;
    }

    public int DurationMinutesSharedHomeStay {
      get => _householdDay.DurationMinutesSharedHomeStay;
      set => _householdDay.DurationMinutesSharedHomeStay = value;
    }

    public int AdultsInSharedHomeStay {
      get => _householdDay.AdultsInSharedHomeStay;
      set => _householdDay.AdultsInSharedHomeStay = value;
    }

    public int ChildrenInSharedHomeStay {
      get => _householdDay.ChildrenInSharedHomeStay;
      set => _householdDay.ChildrenInSharedHomeStay = value;
    }

    public int PrimaryPriorityTimeFlag {
      get => _householdDay.PrimaryPriorityTimeFlag;
      set => _householdDay.PrimaryPriorityTimeFlag = value;
    }

    #endregion

    #region flags, choice model properties, etc.

    public int JointTourFlag {
      get; set;
    }

    #endregion

    #region init/utility/export methods

    protected override IHouseholdDay ResetHouseholdDay() {
      _householdDay = new HouseholdDay {
        Id = Id,
        HouseholdId = HouseholdId,
        Day = Day,
        DayOfWeek = DayOfWeek,
        ExpansionFactor = ExpansionFactor
      };

      return _householdDay;
    }

    #endregion
  }
}