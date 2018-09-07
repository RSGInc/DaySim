// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.DomainModels.Wrappers {
  public interface IJointTourWrapper : IJointTour {
    #region relations properties

    IHouseholdWrapper Household { get; }

    IHouseholdDayWrapper HouseholdDay { get; }

    #endregion

    #region flags/choice model/etc. properties

    ITimeWindow TimeWindow { get; set; }

    #endregion

    #region wrapper methods

    void SetParticipantTourSequence(ITourWrapper participantTour);

    #endregion

    #region init/utility/export methods

    void Export();

    #endregion
  }
}