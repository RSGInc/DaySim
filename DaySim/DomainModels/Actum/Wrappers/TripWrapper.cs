// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class TripWrapper : Default.Wrappers.TripWrapper, IActumTripWrapper {
    private readonly IActumTrip _trip;

    [UsedImplicitly]
    public TripWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour) : base(trip, tourWrapper, halfTour) {
      _trip = (IActumTrip)trip;
    }

    #region domain model properies

    public int BikePTCombination {
      get => _trip.BikePTCombination;
      set => _trip.BikePTCombination = value;
    }

    public int EscortedDestinationPurpose {
      get => _trip.EscortedDestinationPurpose;
      set => _trip.EscortedDestinationPurpose = value;
    }

    public int AccessMode {
      get => _trip.AccessMode;
      set => _trip.AccessMode = value;
    }

    public int AccessPathType {
      get => _trip.AccessPathType;
      set => _trip.AccessPathType = value;
    }

    public double AccessTime {
      get => _trip.AccessTime;
      set => _trip.AccessTime = value;
    }

    public double AccessCost {
      get => _trip.AccessCost;
      set => _trip.AccessCost = value;
    }

    public double AccessDistance {
      get => _trip.AccessDistance;
      set => _trip.AccessDistance = value;
    }

    public int AccessStopArea {
      get => _trip.AccessStopArea;
      set => _trip.AccessStopArea = value;
    }

    public int EgressMode {
      get => _trip.EgressMode;
      set => _trip.EgressMode = value;
    }

    public int EgressPathType {
      get => _trip.EgressPathType;
      set => _trip.EgressPathType = value;
    }

    public double EgressTime {
      get => _trip.EgressTime;
      set => _trip.EgressTime = value;
    }

    public double EgressCost {
      get => _trip.EgressCost;
      set => _trip.EgressCost = value;
    }

    public double EgressDistance {
      get => _trip.EgressDistance;
      set => _trip.EgressDistance = value;
    }

    public int EgressStopArea {
      get => _trip.EgressStopArea;
      set => _trip.EgressStopArea = value;
    }


    #endregion

    #region wrapper methods

    public override void SetDriverOrPassenger(List<ITripWrapper> trips) {
      if (Mode == Global.Settings.Modes.Walk || Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Transit || Mode == Global.Settings.Modes.SchoolBus || Mode == Global.Settings.Modes.Other) {
        DriverType = Global.Settings.DriverTypes.NotApplicable;
      } else if (Mode == Global.Settings.Modes.Sov || Mode == Global.Settings.Modes.HovDriver) {
        DriverType = Global.Settings.DriverTypes.Driver;
      } else if (Mode == Global.Settings.Modes.HovPassenger) {
        DriverType = Global.Settings.DriverTypes.Passenger;
      }
    }

    public override void SetTripValueOfTime() {
      double costDivisor =
                Mode == Global.Settings.Modes.HovDriver && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
                    ? Global.Configuration.Coefficients_HOV2CostDivisor_Work
                    : Mode == Global.Settings.Modes.HovDriver && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
                        ? Global.Configuration.Coefficients_HOV2CostDivisor_Other
                        : Mode == Global.Settings.Modes.HovPassenger && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
                            ? Global.Configuration.Coefficients_HOV3CostDivisor_Work
                            : Mode == Global.Settings.Modes.HovPassenger && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
                                ? Global.Configuration.Coefficients_HOV3CostDivisor_Other
                                : 1.0;

      ValueOfTime = (Tour.TimeCoefficient * 60) / (Tour.CostCoefficient / costDivisor);
    }

    public virtual bool IsBusinessDestinationPurpose() {
      return DestinationPurpose == Global.Settings.Purposes.Business;
    }

    public virtual bool IsBusinessOriginPurpose() {
      return OriginPurpose == Global.Settings.Purposes.Business;
    }

    #endregion
  }
}