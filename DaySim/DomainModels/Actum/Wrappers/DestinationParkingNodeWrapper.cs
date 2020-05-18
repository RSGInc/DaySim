// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.Framework.ShadowPricing;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class DestinationParkingNodeWrapper : IActumDestinationParkingNodeWrapper {
    private readonly IActumDestinationParkingNode _destinationParkingNode;
    private const int RESIDENTIAL_ON_STREET = 1;
    private const int FREE_ON_STREET = 2;
    private const int METERED_ON_STREET = 3;
    private const int PUBLIC_OFF_STREET = 4;
    public const int UNAVAILABLE_PRICE_INDICATOR = -999;


    [UsedImplicitly]
    public DestinationParkingNodeWrapper(IActumDestinationParkingNode destinationParkingNode) {
      _destinationParkingNode = (IActumDestinationParkingNode)destinationParkingNode;
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

    public int LocationType {
      get => _destinationParkingNode.LocationType;
      set => _destinationParkingNode.LocationType = value;
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

    public double MaxDuration {
      get => _destinationParkingNode.MaxDuration;
      set => _destinationParkingNode.MaxDuration = value;
    }

    public double FreeDuration {
      get => _destinationParkingNode.FreeDuration;
      set => _destinationParkingNode.FreeDuration = value;
    }

    public double Price1AM {
      get => _destinationParkingNode.Price1AM;
      set => _destinationParkingNode.Price1AM = value;
    }

    public double Price2AM {
      get => _destinationParkingNode.Price2AM;
      set => _destinationParkingNode.Price2AM = value;
    }

    public double Price3AM {
      get => _destinationParkingNode.Price3AM;
      set => _destinationParkingNode.Price3AM = value;
    }

    public double Price4AM {
      get => _destinationParkingNode.Price4AM;
      set => _destinationParkingNode.Price4AM = value;
    }

    public double Price5AM {
      get => _destinationParkingNode.Price5AM;
      set => _destinationParkingNode.Price5AM = value;
    }

    public double Price6AM {
      get => _destinationParkingNode.Price6AM;
      set => _destinationParkingNode.Price6AM = value;
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

    public double MinimumTime {
      get => _destinationParkingNode.MinimumTime;
      set => _destinationParkingNode.MinimumTime = value;
    }

    public double MinimumPrice {
      get => _destinationParkingNode.MinimumPrice;
      set => _destinationParkingNode.MinimumPrice = value;
    }
    public double FullDayPrice {
      get => _destinationParkingNode.FullDayPrice;
      set => _destinationParkingNode.FullDayPrice = value;
    }
    public double MonthlyPassDayPrice {
      get => _destinationParkingNode.MonthlyPassDayPrice;
      set => _destinationParkingNode.MonthlyPassDayPrice = value;
    }
    public int ResidentPermitPassAvailable {
      get => _destinationParkingNode.ResidentPermitPassAvailable;
      set => _destinationParkingNode.ResidentPermitPassAvailable = value;
    }
    public double ResidentPermitPassDayPrice {
      get => _destinationParkingNode.ResidentPermitPassDayPrice;
      set => _destinationParkingNode.ResidentPermitPassDayPrice = value;
    }
    public int OpeningTime {
      get => _destinationParkingNode.OpeningTime;
      set => _destinationParkingNode.OpeningTime = value;
    }
    public int ClosingTime {
      get => _destinationParkingNode.ClosingTime;
      set => _destinationParkingNode.ClosingTime = value;
    }
    public int OccupancyMorning {
      get => _destinationParkingNode.OccupancyMorning;
      set => _destinationParkingNode.OccupancyMorning = value;
    }
    public int OccupancyAfternoon {
      get => _destinationParkingNode.OccupancyAfternoon;
      set => _destinationParkingNode.OccupancyAfternoon = value;
    }
    public int OccupancyNight {
      get => _destinationParkingNode.OccupancyNight;
      set => _destinationParkingNode.OccupancyNight = value;
    }

     #endregion


    #region flags/choice model/etc. properties

    public double[] ShadowPriceDifference { get; set; }

    public double[] ShadowPrice { get; set; }

    public double[] ExogenousLoad { get; set; }

    public double[] ParkingLoad { get; set; }

    #endregion

    #region wrapper methods

    public virtual double CalculateParkingPrice(int parkArriveTime, int parkDepartTime, int destPurpose) {
      double parkPrice = UNAVAILABLE_PRICE_INDICATOR;

      if (ParkingType != 2) {  //skip price for free parking
        double paidArriveTime = Math.Min(parkArriveTime + 60.0 * FreeDuration, Global.Settings.Times.MinutesInADay);
        double hourPrice = 0
         + (!(parkDepartTime < Global.Settings.Times.ThreeAM || paidArriveTime >= Global.Settings.Times.FourAM)).ToFlag() * Price3AM
         + (!(parkDepartTime < Global.Settings.Times.FourAM || paidArriveTime >= Global.Settings.Times.FiveAM)).ToFlag() * Price4AM
         + (!(parkDepartTime < Global.Settings.Times.FiveAM || paidArriveTime >= Global.Settings.Times.SixAM)).ToFlag() * Price5AM
         + (!(parkDepartTime < Global.Settings.Times.SixAM || paidArriveTime >= Global.Settings.Times.SevenAM)).ToFlag() * Price6AM
         + (!(parkDepartTime < Global.Settings.Times.SevenAM || paidArriveTime >= Global.Settings.Times.EightAM)).ToFlag() * Price7AM
         + (!(parkDepartTime < Global.Settings.Times.EightAM || paidArriveTime >= Global.Settings.Times.NineAM)).ToFlag() * Price8AM
         + (!(parkDepartTime < Global.Settings.Times.NineAM || paidArriveTime >= Global.Settings.Times.TenAM)).ToFlag() * Price9AM
         + (!(parkDepartTime < Global.Settings.Times.TenAM || paidArriveTime >= Global.Settings.Times.ElevenAM)).ToFlag() * Price10AM
         + (!(parkDepartTime < Global.Settings.Times.ElevenAM || paidArriveTime >= Global.Settings.Times.Noon)).ToFlag() * Price11AM
         + (!(parkDepartTime < Global.Settings.Times.Noon || paidArriveTime >= Global.Settings.Times.OnePM)).ToFlag() * Price12PM
         + (!(parkDepartTime < Global.Settings.Times.OnePM || paidArriveTime >= Global.Settings.Times.TwoPM)).ToFlag() * Price1PM
         + (!(parkDepartTime < Global.Settings.Times.TwoPM || paidArriveTime >= Global.Settings.Times.ThreePM)).ToFlag() * Price2PM
         + (!(parkDepartTime < Global.Settings.Times.ThreePM || paidArriveTime >= Global.Settings.Times.FourPM)).ToFlag() * Price3PM
         + (!(parkDepartTime < Global.Settings.Times.FourPM || paidArriveTime >= Global.Settings.Times.FivePM)).ToFlag() * Price4PM
         + (!(parkDepartTime < Global.Settings.Times.FivePM || paidArriveTime >= Global.Settings.Times.SixPM)).ToFlag() * Price5PM
         + (!(parkDepartTime < Global.Settings.Times.SixPM || paidArriveTime >= Global.Settings.Times.SevenPM)).ToFlag() * Price6PM
         + (!(parkDepartTime < Global.Settings.Times.SevenPM || paidArriveTime >= Global.Settings.Times.EightPM)).ToFlag() * Price7PM
         + (!(parkDepartTime < Global.Settings.Times.EightPM || paidArriveTime >= Global.Settings.Times.NinePM)).ToFlag() * Price8PM
         + (!(parkDepartTime < Global.Settings.Times.NinePM || paidArriveTime >= Global.Settings.Times.TenPM)).ToFlag() * Price9PM
         + (!(parkDepartTime < Global.Settings.Times.TenPM || paidArriveTime >= Global.Settings.Times.ElevenPM)).ToFlag() * Price10PM
         + (!(parkDepartTime < Global.Settings.Times.ElevenPM || paidArriveTime >= Global.Settings.Times.Midnight)).ToFlag() * Price11PM
         + (!(parkDepartTime < Global.Settings.Times.Midnight || paidArriveTime >= Global.Settings.Times.OneAM)).ToFlag() * Price12AM
         + (!(parkDepartTime < Global.Settings.Times.OneAM || paidArriveTime >= Global.Settings.Times.TwoAM)).ToFlag() * Price1AM
         + (!(parkDepartTime < Global.Settings.Times.TwoAM || paidArriveTime >= Global.Settings.Times.MinutesInADay)).ToFlag() * Price2AM;

        parkPrice = Math.Min(Math.Max(hourPrice, MinimumPrice), FullDayPrice);

        if (MonthlyPassDayPrice > 0 && (destPurpose == Global.Settings.Purposes.Work || destPurpose == Global.Settings.Purposes.School)
           && MonthlyPassDayPrice < parkPrice) {
          parkPrice = MonthlyPassDayPrice;
        }
      }

      return parkPrice;
    }

    public virtual double SetDestinationParkingEffectivePrice(int parkArriveTime, int parkDepartTime, int destPurpose) {
      double parkPrice = UNAVAILABLE_PRICE_INDICATOR;
      return parkPrice;
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
