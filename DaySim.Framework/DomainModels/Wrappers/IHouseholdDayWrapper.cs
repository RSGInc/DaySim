// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.DomainModels.Wrappers {
  public interface IHouseholdDayWrapper : IHouseholdDay {
    #region relations properties

    IHouseholdWrapper Household { get; set; }

    List<IPersonDayWrapper> PersonDays { get; set; }

    List<IJointTourWrapper> JointToursList { get; set; }

    List<IFullHalfTourWrapper> FullHalfToursList { get; set; }

    List<IPartialHalfTourWrapper> PartialHalfToursList { get; set; }

    #endregion

    #region flags/choice model/etc. properties

    int AttemptedSimulations { get; set; }

    bool IsMissingData { get; set; }

    bool IsValid { get; set; }

    #endregion

    #region wrapper methods

    IJointTourWrapper CreateJointTour(IHouseholdDayWrapper householdDay, IEnumerable<IPersonDayWrapper> orderedPersonDays, int[] participants, int purpose);

    IFullHalfTourWrapper CreateFullHalfTour(IHouseholdDayWrapper householdDay, IEnumerable<IPersonDayWrapper> orderedPersonDays, int[] participants, int direction);

    IPartialHalfTourWrapper CreatePartialHalfTour(IHouseholdDayWrapper householdDay, IEnumerable<IPersonDayWrapper> orderedPersonDays, int[] participants, int[] pickOrder, double[] distanceFromChauffeur, int direction);

    #endregion

    #region init/utility/export methods

    void Export();

    void Reset();

    #endregion
  }
}