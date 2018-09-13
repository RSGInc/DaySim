// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.ShadowPricing;

namespace DaySim.DomainModels.Default.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
  public class DestinationParkingNodeWrapper : IDestinationParkingNodeWrapper {
    private readonly IDestinationParkingNode _destinationParkingNode;
    private const int RESIDENTIAL_ON_STREET = 1;
    private const int FREE_ON_STREET = 2;
    private const int METERED_ON_STREET = 3;
    private const int PUBLIC_OFF_STREET = 4;
    public const int UNAVAILABLE_PRICE_INDICATOR = -999;


    [UsedImplicitly]
    public DestinationParkingNodeWrapper(IDestinationParkingNode destinationParkingNode) {
      _destinationParkingNode = destinationParkingNode;
    }

    #region domain model properies

    public int Id {
      get => _destinationParkingNode.Id;
      set => _destinationParkingNode.Id = value;
    }

    public int ZoneId {
      get => _destinationParkingNode.ZoneId;
      set => _destinationParkingNode.ZoneId = value;
    }

    public int XCoordinate {
      get => _destinationParkingNode.XCoordinate;
      set => _destinationParkingNode.XCoordinate = value;
    }

    public int YCoordinate {
      get => _destinationParkingNode.YCoordinate;
      set => _destinationParkingNode.YCoordinate = value;
    }

    public int ParkingType {
      get => _destinationParkingNode.ParkingType;
      set => _destinationParkingNode.ParkingType = value;
    }

    public int Capacity {
      get => _destinationParkingNode.Capacity;
      set => _destinationParkingNode.Capacity = value;
    }

    public int ParcelId {
      get => _destinationParkingNode.ParcelId;
      set => _destinationParkingNode.ParcelId = value;
    }

    public int NodeId {
      get => _destinationParkingNode.NodeId;
      set => _destinationParkingNode.NodeId = value;
    }

    public int MaxDuration {
      get => _destinationParkingNode.MaxDuration;
      set => _destinationParkingNode.MaxDuration = value;
    }

    public double PreOccupiedDay {
      get => _destinationParkingNode.PreOccupiedDay;
      set => _destinationParkingNode.PreOccupiedDay = value;
    }

    public double PreOccupiedOther {
      get => _destinationParkingNode.PreOccupiedOther;
      set => _destinationParkingNode.PreOccupiedOther = value;
    }

    public double Price7AM {
      get => _destinationParkingNode.Price7AM;
      set => _destinationParkingNode.Price7AM = value;
    }

    public double Price8AM {
      get => _destinationParkingNode.Price8AM;
      set => _destinationParkingNode.Price8AM = value;
    }

    public double Price9AM {
      get => _destinationParkingNode.Price9AM;
      set => _destinationParkingNode.Price9AM = value;
    }

    public double Price10AM {
      get => _destinationParkingNode.Price10AM;
      set => _destinationParkingNode.Price10AM = value;
    }
    public double Price11AM {
      get => _destinationParkingNode.Price11AM;
      set => _destinationParkingNode.Price11AM = value;
    }

    public double Price12PM {
      get => _destinationParkingNode.Price12PM;
      set => _destinationParkingNode.Price12PM = value;
    }

    public double Price1PM {
      get => _destinationParkingNode.Price1PM;
      set => _destinationParkingNode.Price1PM = value;
    }

    public double Price2PM {
      get => _destinationParkingNode.Price2PM;
      set => _destinationParkingNode.Price2PM = value;
    }

    public double Price3PM {
      get => _destinationParkingNode.Price3PM;
      set => _destinationParkingNode.Price3PM = value;
    }

    public double Price4PM {
      get => _destinationParkingNode.Price4PM;
      set => _destinationParkingNode.Price4PM = value;
    }

    public double Price5PM {
      get => _destinationParkingNode.Price5PM;
      set => _destinationParkingNode.Price5PM = value;
    }

    public double Price6PM {
      get => _destinationParkingNode.Price6PM;
      set => _destinationParkingNode.Price6PM = value;
    }

    public double Price7PM {
      get => _destinationParkingNode.Price7PM;
      set => _destinationParkingNode.Price7PM = value;
    }

    public double Price8PM {
      get => _destinationParkingNode.Price8PM;
      set => _destinationParkingNode.Price8PM = value;
    }

    public double Price9PM {
      get => _destinationParkingNode.Price9PM;
      set => _destinationParkingNode.Price9PM = value;
    }

    public double Price10PM {
      get => _destinationParkingNode.Price10PM;
      set => _destinationParkingNode.Price10PM = value;
    }

    public double Price11PM {
      get => _destinationParkingNode.Price11PM;
      set => _destinationParkingNode.Price11PM = value;
    }

    public double Price12AM {
      get => _destinationParkingNode.Price12AM;
      set => _destinationParkingNode.Price12AM = value;
    }

    // on-street properties do double duty with different names for off-street
    public double Price1Hour => _destinationParkingNode.Price7AM;

    public double Price2Hour => _destinationParkingNode.Price8AM;

    public double Price3Hour => _destinationParkingNode.Price9AM;

    public double Price4Hour => _destinationParkingNode.Price10AM;

    public double Price12Hour => _destinationParkingNode.Price11AM;

    public double PriceDay => _destinationParkingNode.Price12PM;

    public double PriceDayDiscount => _destinationParkingNode.Price1PM;

    public double OpeningTime => _destinationParkingNode.Price2PM;

    public double ClosingTime => _destinationParkingNode.Price3PM;

    public double EaryBirdPriceStartTime => _destinationParkingNode.Price4PM;

    public double EarlyBirdPriceEndTime => _destinationParkingNode.Price5PM;

    public double EarlyBirdHourlyPrice => _destinationParkingNode.Price6PM;

    public double EarlyBirdDailyPrice => _destinationParkingNode.Price7PM;

    public double EveningPriceStartTime => _destinationParkingNode.Price8PM;

    public double EveningPriceEndTime => _destinationParkingNode.Price9PM;

    public double EveningHourlyPrice => _destinationParkingNode.Price10PM;
    public double EveningDailyPrice => _destinationParkingNode.Price11PM;
    public double OffStreetType => _destinationParkingNode.Price12AM;
    #endregion


    #region flags/choice model/etc. properties

    public double[] ShadowPriceDifference { get; set; }

    public double[] ShadowPrice { get; set; }

    public double[] ExogenousLoad { get; set; }

    public double[] ParkingLoad { get; set; }

    #endregion

    #region wrapper methods

    public virtual double SetDestinationParkingEffectivePrice(int minArrive, int minDepart, int destPurpose) {
      double effectivePrice = UNAVAILABLE_PRICE_INDICATOR;

      if (ParkingType == PUBLIC_OFF_STREET && (minArrive < OpeningTime || minDepart > ClosingTime)) {
        return effectivePrice;
      }
      int minDuration = minDepart - minArrive;
      if (minDuration < 0) { minDuration += Global.Settings.Times.MinutesInADay; }  // jf going past 3 am, add 1440
      double hoursDuration = Math.Truncate((2.0 * minDuration + 59) / 60.0) / 2.0;  // round up to nearest half hour
      if (minDuration > MaxDuration) {
        return effectivePrice;
      }
      if (ParkingType == FREE_ON_STREET || ParkingType == RESIDENTIAL_ON_STREET) {
        effectivePrice = 0;
      } else if (ParkingType == METERED_ON_STREET) {
        double hourlyPrice =
                   minArrive < Global.Settings.Times.SevenAM ? 0 :
                   minArrive < Global.Settings.Times.EightAM ? Price7AM :
                   minArrive < Global.Settings.Times.NineAM ? Price8AM :
                   minArrive < Global.Settings.Times.TenAM ? Price9AM :
                   minArrive < Global.Settings.Times.ElevenAM ? Price10AM :
                   minArrive < Global.Settings.Times.Noon ? Price11AM :
                   minArrive < Global.Settings.Times.OnePM ? Price12PM :
                   minArrive < Global.Settings.Times.TwoPM ? Price1PM :
                   minArrive < Global.Settings.Times.ThreePM ? Price2PM :
                   minArrive < Global.Settings.Times.FourPM ? Price3PM :
                   minArrive < Global.Settings.Times.FivePM ? Price4PM :
                   minArrive < Global.Settings.Times.SixPM ? Price5PM :
                   minArrive < Global.Settings.Times.SevenPM ? Price6PM :
                   minArrive < Global.Settings.Times.EightPM ? Price7PM :
                   minArrive < Global.Settings.Times.NinePM ? Price8PM :
                   minArrive < Global.Settings.Times.TenPM ? Price9PM :
                   minArrive < Global.Settings.Times.ElevenPM ? Price10PM :
                   minArrive < Global.Settings.Times.Midnight ? Price11PM :
                   minArrive < Global.Settings.Times.OneAM ? Price12AM : 0;
        effectivePrice = hourlyPrice * hoursDuration;
      } else if (ParkingType == PUBLIC_OFF_STREET) {
        if (EaryBirdPriceStartTime > 0 && minArrive >= EaryBirdPriceStartTime && minArrive < EarlyBirdPriceEndTime) {
          effectivePrice = Math.Min(EarlyBirdHourlyPrice * hoursDuration, EarlyBirdDailyPrice);
        } else if (EveningPriceStartTime > 0 && minArrive >= EveningPriceStartTime && minArrive < EveningPriceEndTime) {
          effectivePrice = Math.Min(EveningHourlyPrice * hoursDuration, EveningDailyPrice);
        } else {
          double dailyPrice =
                        (destPurpose == Global.Settings.Purposes.Work ||
                         destPurpose == Global.Settings.Purposes.School
                         ? PriceDayDiscount : PriceDay);
          effectivePrice =
              hoursDuration <= 1 ? Math.Min(Price1Hour, dailyPrice) :
              hoursDuration <= 2 ? Math.Min(Price2Hour, dailyPrice) :
              hoursDuration <= 3 ? Math.Min(Price3Hour, dailyPrice) :
              hoursDuration <= 4 ? Math.Min(Price4Hour, dailyPrice) :
              hoursDuration <= 12 ? Math.Min(Price12Hour, dailyPrice) : dailyPrice;
        }
      }
      effectivePrice = effectivePrice / 100.0;
      // add shadow price for arrival time if turned on
      if (Global.Configuration.ShouldUseDestinationParkingShadowPricing) {
        effectivePrice += ShadowPrice[minArrive];
      }
      return effectivePrice;
    }



    public virtual void SetDestinationParkingShadowPricing(Dictionary<int, IDestinationParkingShadowPriceNode> destinationParkingShadowPrices) {
      if (destinationParkingShadowPrices == null) {
        throw new ArgumentNullException("destinationParkingShadowPrices");
      }

      if (!Global.DestinationParkingNodeIsEnabled || !Global.Configuration.ShouldUseDestinationParkingShadowPricing || Global.Configuration.IsInEstimationMode) {
        return;
      }


      ShadowPriceDifference = new double[Global.Settings.Times.MinutesInADay];
      ShadowPrice = new double[Global.Settings.Times.MinutesInADay];
      ExogenousLoad = new double[Global.Settings.Times.MinutesInADay];
      ParkingLoad = new double[Global.Settings.Times.MinutesInADay];

      if (!destinationParkingShadowPrices.TryGetValue(Id, out IDestinationParkingShadowPriceNode destinationParkingShadowPriceNode)) {
        return;
      }

      ShadowPriceDifference = destinationParkingShadowPrices[Id].ShadowPriceDifference;
      ShadowPrice = destinationParkingShadowPrices[Id].ShadowPrice;
      ExogenousLoad = destinationParkingShadowPrices[Id].ExogenousLoad;
      // ParkingLoad = destinationParkingShadowPrices[Id].ParkingLoad; {JLB 20121001 commented out this line so that initial values of load are zero for any run}
    }

    #endregion
  }
}
