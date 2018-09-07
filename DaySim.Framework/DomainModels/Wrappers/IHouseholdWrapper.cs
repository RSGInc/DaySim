// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.DomainModels.Wrappers {
  public interface IHouseholdWrapper : IHousehold {
    #region relations properties

    List<IPersonWrapper> Persons { get; set; }

    List<IHouseholdDayWrapper> HouseholdDays { get; set; }

    IHouseholdTotals HouseholdTotals { get; set; }

    IParcelWrapper ResidenceParcel { get; set; }

    #endregion

    #region flags/choice model/etc. properties

    int OwnsAutomatedVehicles { get; set; }

    int ResidenceBuffer2Density { get; set; }

    bool IsOnePersonHousehold { get; set; }

    bool IsTwoPersonHousehold { get; set; }

    bool Has0To15KIncome { get; set; }

    bool Has0To25KIncome { get; set; }

    bool Has25To45KIncome { get; set; }

    bool Has25To50KIncome { get; set; }

    bool Has50To75KIncome { get; set; }

    bool Has75To100KIncome { get; set; }

    bool Has75KPlusIncome { get; set; }

    bool Has100KPlusIncome { get; set; }

    bool HasIncomeUnder50K { get; set; }

    bool HasIncomeOver50K { get; set; }

    bool HasValidIncome { get; set; }

    bool HasMissingIncome { get; set; }

    bool Has1Driver { get; set; }

    bool Has2Drivers { get; set; }

    bool Has3Drivers { get; set; }

    bool Has4OrMoreDrivers { get; set; }

    bool HasMoreDriversThan1 { get; set; }

    bool HasMoreDriversThan2 { get; set; }

    bool HasMoreDriversThan3 { get; set; }

    bool HasMoreDriversThan4 { get; set; }

    bool HasNoFullOrPartTimeWorker { get; set; }

    bool Has1OrLessFullOrPartTimeWorkers { get; set; }

    bool Has2OrLessFullOrPartTimeWorkers { get; set; }

    bool Has3OrLessFullOrPartTimeWorkers { get; set; }

    bool Has4OrLessFullOrPartTimeWorkers { get; set; }

    bool HasChildrenUnder16 { get; set; }

    bool HasChildrenUnder5 { get; set; }

    bool HasChildrenAge5Through15 { get; set; }

    bool HasChildren { get; set; }

    int HouseholdType { get; set; }

    #endregion

    #region random/seed synchronization properties

    IRandomUtility RandomUtility { get; set; }

    int[] SeedValues { get; }

    #endregion

    #region wrapper methods

    int GetVotALSegment();

    int GetCarsPerDriver();

    int GetFlagForCarsLessThanDrivers(int householdCars);

    int GetFlagForCarsLessThanWorkers(int householdCars);

    int GetFlagForNoCarsInHousehold(int householdCars);

    #endregion

    #region init/utility/export methods

    void Init();

    void Export();

    #endregion
  }
}