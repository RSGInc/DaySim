// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;

using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class ParcelWrapper : Default.Wrappers.ParcelWrapper, IParcelWrapper {
    private readonly IParcel _parcel;

    [UsedImplicitly]
    public ParcelWrapper(IParcel parcel) : base(parcel) {
      _parcel = parcel;
    }

    //See RawConverter, where the Actum input names are renamed to the Default names, which are then used in the Actum Parcel class. 
    //The following wrapper methods rename them back to the names used by Actum version.  
    public double ParkingCostPerHour8_18 {
      get => _parcel.CircuityRatio_E1;
      set => _parcel.CircuityRatio_E1 = value;
    }

    public double ParkingCostPerHour18_23 {
      get => _parcel.CircuityRatio_E2;
      set => _parcel.CircuityRatio_E2 = value;
    }

    public double ParkingCostPerHour23_08 {
      get => _parcel.CircuityRatio_E3;
      set => _parcel.CircuityRatio_E3 = value;
    }

    public double ResidentAnnualParkingCost {
      get => _parcel.CircuityRatio_NE1;
      set => _parcel.CircuityRatio_NE1 = value;
    }

    public double ParkingSearchTime21_05 {
      get => _parcel.CircuityRatio_NE2;
      set => _parcel.CircuityRatio_NE2 = value;
    }

    public double ParkingSearchTime05_06 {
      get => _parcel.CircuityRatio_NE3;
      set => _parcel.CircuityRatio_NE3 = value;
    }

    public double ParkingSearchTime06_07 {
      get => _parcel.CircuityRatio_N1;
      set => _parcel.CircuityRatio_N1 = value;
    }

    public double ParkingSearchTime07_08 {
      get => _parcel.CircuityRatio_N2;
      set => _parcel.CircuityRatio_N2 = value;
    }

    public double ParkingSearchTime08_09 {
      get => _parcel.CircuityRatio_N3;
      set => _parcel.CircuityRatio_N3 = value;
    }

    public double ParkingSearchTime09_15 {
      get => _parcel.CircuityRatio_NW1;
      set => _parcel.CircuityRatio_NW1 = value;
    }

    public double ParkingSearchTime15_16 {
      get => _parcel.CircuityRatio_NW2;
      set => _parcel.CircuityRatio_NW2 = value;
    }

    public double ParkingSearchTime16_17 {
      get => _parcel.CircuityRatio_NW3;
      set => _parcel.CircuityRatio_NW3 = value;
    }

    public double ParkingSearchTime17_18 {
      get => _parcel.CircuityRatio_W1;
      set => _parcel.CircuityRatio_W1 = value;
    }

    public double ParkingSearchTime18_21 {
      get => _parcel.CircuityRatio_W2;
      set => _parcel.CircuityRatio_W2 = value;
    }


    public double ParkingCostPerHour8_18Buffer1 {
      get => _parcel.CircuityRatio_W3;
      set => _parcel.CircuityRatio_W3 = value;
    }

    public double ParkingCostPerHour18_23Buffer1 {
      get => _parcel.CircuityRatio_SW1;
      set => _parcel.CircuityRatio_SW1 = value;
    }

    public double ParkingCostPerHour23_08Buffer1 {
      get => _parcel.CircuityRatio_SW2;
      set => _parcel.CircuityRatio_SW2 = value;
    }

    public double ResidentAnnualParkingCostBuffer1 {
      get => _parcel.CircuityRatio_SW3;
      set => _parcel.CircuityRatio_SW3 = value;
    }

    public double ParkingSearchTime21_05Buffer1 {
      get => _parcel.CircuityRatio_S1;
      set => _parcel.CircuityRatio_S1 = value;
    }

    public double ParkingSearchTime05_06Buffer1 {
      get => _parcel.CircuityRatio_S2;
      set => _parcel.CircuityRatio_S2 = value;
    }

    public double ParkingSearchTime06_07Buffer1 {
      get => _parcel.CircuityRatio_S3;
      set => _parcel.CircuityRatio_S3 = value;
    }

    public double ParkingSearchTime07_08Buffer1 {
      get => _parcel.CircuityRatio_SE1;
      set => _parcel.CircuityRatio_SE1 = value;
    }

    public double ParkingSearchTime08_09Buffer1 {
      get => _parcel.CircuityRatio_SE2;
      set => _parcel.CircuityRatio_SE2 = value;
    }

    public double ParkingSearchTime09_15Buffer1 {
      get => _parcel.CircuityRatio_SE3;
      set => _parcel.CircuityRatio_SE3 = value;
    }

    public double ParkingSearchTime15_16Buffer1 {
      get => _parcel.DistanceToFerry;
      set => _parcel.DistanceToFerry = value;
    }

    public double ParkingSearchTime16_17Buffer1 {
      get => _parcel.DistanceToLightRail;
      set => _parcel.DistanceToLightRail = value;
    }

    public double ParkingSearchTime17_18Buffer1 {
      get => _parcel.ParkingOffStreetPaidDailySpaces;
      set => _parcel.ParkingOffStreetPaidDailySpaces = value;
    }

    public double ParkingSearchTime18_21Buffer1 {
      get => _parcel.ParkingOffStreetPaidHourlySpaces;
      set => _parcel.ParkingOffStreetPaidHourlySpaces = value;
    }



    public double ParkingCostPerHour8_18Buffer2 {
      get => _parcel.ParkingOffStreetPaidDailyPrice;
      set => _parcel.ParkingOffStreetPaidDailyPrice = value;
    }

    public double ParkingCostPerHour18_23Buffer2 {
      get => _parcel.ParkingOffStreetPaidHourlyPrice;
      set => _parcel.ParkingOffStreetPaidHourlyPrice = value;
    }

    public double ParkingCostPerHour23_08Buffer2 {
      get => _parcel.ParkingOffStreetPaidDailySpacesBuffer1;
      set => _parcel.ParkingOffStreetPaidDailySpacesBuffer1 = value;
    }

    public double ResidentAnnualParkingCostBuffer2 {
      get => _parcel.ParkingOffStreetPaidHourlySpacesBuffer1;
      set => _parcel.ParkingOffStreetPaidHourlySpacesBuffer1 = value;
    }

    public double ParkingSearchTime21_05Buffer2 {
      get => _parcel.ParkingOffStreetPaidDailyPriceBuffer1;
      set => _parcel.ParkingOffStreetPaidDailyPriceBuffer1 = value;
    }

    public double ParkingSearchTime05_06Buffer2 {
      get => _parcel.ParkingOffStreetPaidHourlyPriceBuffer1;
      set => _parcel.ParkingOffStreetPaidHourlyPriceBuffer1 = value;
    }

    public double ParkingSearchTime06_07Buffer2 {
      get => _parcel.ParkingOffStreetPaidDailySpacesBuffer2;
      set => _parcel.ParkingOffStreetPaidDailySpacesBuffer2 = value;
    }

    public double ParkingSearchTime07_08Buffer2 {
      get => _parcel.ParkingOffStreetPaidHourlySpacesBuffer2;
      set => _parcel.ParkingOffStreetPaidHourlySpacesBuffer2 = value;
    }

    public double ParkingSearchTime08_09Buffer2 {
      get => _parcel.ParkingOffStreetPaidDailyPriceBuffer2;
      set => _parcel.ParkingOffStreetPaidDailyPriceBuffer2 = value;
    }

    public double ParkingSearchTime09_15Buffer2 {
      get => _parcel.ParkingOffStreetPaidHourlyPriceBuffer2;
      set => _parcel.ParkingOffStreetPaidHourlyPriceBuffer2 = value;
    }

    public double ParkingSearchTime15_16Buffer2 {
      get => _parcel.OpenSpaceType1Buffer1;
      set => _parcel.OpenSpaceType1Buffer1 = value;
    }

    public double ParkingSearchTime16_17Buffer2 {
      get => _parcel.OpenSpaceType2Buffer1;
      set => _parcel.OpenSpaceType2Buffer1 = value;
    }

    public double ParkingSearchTime17_18Buffer2 {
      get => _parcel.OpenSpaceType1Buffer2;
      set => _parcel.OpenSpaceType1Buffer2 = value;
    }

    public double ParkingSearchTime18_21Buffer2 {
      get => _parcel.OpenSpaceType2Buffer2;
      set => _parcel.OpenSpaceType2Buffer2 = value;
    }









  }
}