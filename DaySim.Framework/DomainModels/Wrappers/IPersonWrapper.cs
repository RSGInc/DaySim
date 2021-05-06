// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.DomainModels.Wrappers {
  public interface IPersonWrapper : IPerson {
    #region relations properties

    IHouseholdWrapper Household { get; set; }

    IParcelWrapper UsualWorkParcel { get; set; }

    IParcelWrapper UsualSchoolParcel { get; set; }

    #endregion

    #region flags/choice model/etc. properties

    double WorkLocationLogsum { get; set; }

    double SchoolLocationLogsum { get; set; }


    bool IsFullOrPartTimeWorker { get; set; }

    bool IsFulltimeWorker { get; set; }

    bool IsPartTimeWorker { get; set; }

    bool IsNotFullOrPartTimeWorker { get; set; }

    bool IsStudentAge { get; set; }

    bool IsRetiredAdult { get; set; }

    bool IsNonworkingAdult { get; set; }

    bool IsUniversityStudent { get; set; }

    bool IsDrivingAgeStudent { get; set; }

    bool IsChildAge5Through15 { get; set; }

    bool IsChildUnder5 { get; set; }

    bool IsChildUnder16 { get; set; }

    bool IsAdult { get; set; }

    bool IsWorker { get; set; }

    bool IsStudent { get; set; }

    bool IsFemale { get; set; }

    bool IsMale { get; set; }

    bool IsAdultFemale { get; set; }

    bool IsAdultMale { get; set; }

    bool IsDrivingAge { get; set; }

    bool AgeIsBetween18And25 { get; set; }

    bool AgeIsBetween26And35 { get; set; }

    bool AgeIsBetween51And65 { get; set; }

    bool AgeIsBetween51And98 { get; set; }

    bool AgeIsLessThan35 { get; set; }

    bool AgeIsLessThan30 { get; set; }

    bool IsYouth { get; set; }

    #endregion

    #region random/seed synchronization properties

    int[] SeedValues { get; set; }

    #endregion

    #region wrapper methods

    bool IsOnlyFullOrPartTimeWorker();

    bool IsOnlyAdult();

    bool WorksAtHome();

    int GetCarOwnershipSegment();

    double GetTransitFareDiscountFraction();

    int GetHouseholdDayPatternParticipationPriority();

    void UpdatePersonValues();

    void SetWorkParcelPredictions();

    void SetSchoolParcelPredictions();

    #endregion

    #region init/utility/export methods

    void Export();

    #endregion
  }
}
