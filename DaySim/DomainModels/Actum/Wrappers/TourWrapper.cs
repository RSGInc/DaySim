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
  public class TourWrapper : Default.Wrappers.TourWrapper, IActumTourWrapper {
    private readonly IActumTour _tour;

    [UsedImplicitly]
    public TourWrapper(ITour tour, IPersonWrapper personWrapper, IPersonDayWrapper personDayWrapper, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int destinationPurpose) : base(tour, personWrapper, personDayWrapper, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, destinationPurpose) {
      _tour = (IActumTour)tour;
    }

    [UsedImplicitly]
    public TourWrapper(ITour subtour, ITourWrapper tourWrapper, bool suppressRandomVOT = false) : base(subtour, tourWrapper, Global.Settings.Purposes.PersonalBusiness, suppressRandomVOT) {
      _tour = (IActumTour)subtour;
    }

    [UsedImplicitly]
    public TourWrapper(ITour subtour, ITourWrapper tourWrapper, int purpose, bool suppressRandomVOT = false) : base(subtour, tourWrapper, purpose, suppressRandomVOT) {
      _tour = (IActumTour)subtour;
    }

    [UsedImplicitly]
    public TourWrapper(ITour tour, IPersonDayWrapper personDayWrapper, bool suppressRandomVOT = false) : base(tour, personDayWrapper, Global.Settings.Purposes.PersonalBusiness, suppressRandomVOT) {
      _tour = (IActumTour)tour;
    }

    [UsedImplicitly]
    public TourWrapper(ITour tour, IPersonDayWrapper personDayWrapper, int purpose, bool suppressRandomVOT = false) : base(tour, personDayWrapper, purpose, suppressRandomVOT) {
      _tour = (IActumTour)tour;
    }


    #region relations properties

    //public IActumHouseholdWrapper Household { get; set; }

    //public IActumPersonWrapper Person { get; set; }

    //public IActumPersonDayWrapper PersonDay { get; set; }

    //public IActumTourWrapper ParentTour { get; set; }

    #endregion


    #region flags/choice model/etc. properties

    public int HalfTour1AccessMode { get; set; }

    public int HalfTour1AccessPathType { get; set; }

    public double HalfTour1AccessTime { get; set; }

    public double HalfTour1AccessCost { get; set; }

    public double HalfTour1AccessDistance { get; set; }

    public int HalfTour1AccessStopAreaKey { get; set; }

    public int HalfTour1EgressMode { get; set; }

    public int HalfTour1EgressPathType { get; set; }

    public double HalfTour1EgressTime { get; set; }

    public double HalfTour1EgressCost { get; set; }

    public double HalfTour1EgressDistance { get; set; }

    public int HalfTour1EgressStopAreaKey { get; set; }

    public int HalfTour2AccessMode { get; set; }

    public int HalfTour2AccessPathType { get; set; }

    public double HalfTour2AccessTime { get; set; }

    public double HalfTour2AccessCost { get; set; }

    public double HalfTour2AccessDistance { get; set; }

    public int HalfTour2AccessStopAreaKey { get; set; }

    public int HalfTour2EgressMode { get; set; }

    public int HalfTour2EgressPathType { get; set; }

    public double HalfTour2EgressTime { get; set; }

    public double HalfTour2EgressCost { get; set; }

    public double HalfTour2EgressDistance { get; set; }

    public int HalfTour2EgressStopAreaKey { get; set; }

    public double HalfTour1TravelTime { get; set; }

    public double HalfTour2TravelTime { get; set; }

    public double TravelCostForPTBikeTour { get; set; }

    public double TravelDistanceForPTBikeTour { get; set; }




    #endregion

    #region wrapper methods

    public override int GetVotALSegment() {
      int segment =
                (DestinationPurpose == Global.Settings.Purposes.Work || DestinationPurpose == Global.Settings.Purposes.School || DestinationPurpose == Global.Settings.Purposes.Escort)
                    ? Global.Settings.VotALSegments.Medium
                    : (DestinationPurpose == Global.Settings.Purposes.Business)
                        ? Global.Settings.VotALSegments.High
                        : Global.Settings.VotALSegments.Low;

      return segment;
    }

    public virtual bool IsBusinessPurpose() {
      return DestinationPurpose == Global.Settings.Purposes.Business;
    }

    public virtual bool IsHovDriverMode() {
      return Mode == Global.Settings.Modes.HovDriver;
    }

    public virtual bool IsHovPassengerMode() {
      return Mode == Global.Settings.Modes.HovPassenger;
    }

    #endregion




  }
}