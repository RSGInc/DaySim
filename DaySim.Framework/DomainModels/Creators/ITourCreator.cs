// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;

// = Global.Settings.Purposes.PersonalBusiness
// = false

namespace DaySim.Framework.DomainModels.Creators {
  public interface ITourCreator : ICreator {
    ITour CreateModel();

    ITourWrapper CreateWrapper(IPersonWrapper personWrapper, IPersonDayWrapper personDayWrapper, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int destinationPurpose);

    ITourWrapper CreateWrapper(ITour subtour, ITourWrapper tourWrapper, int purpose, bool suppressRandomVOT);

    ITourWrapper CreateWrapper(ITour tour, IPersonDayWrapper personDayWrapper, int purpose, bool suppressRandomVOT);
  }
}