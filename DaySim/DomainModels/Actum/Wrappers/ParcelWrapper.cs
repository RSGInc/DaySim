// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class ParcelWrapper : Default.Wrappers.ParcelWrapper, IActumParcelWrapper {
    private readonly IActumParcel _parcel;

    [UsedImplicitly]
    public ParcelWrapper(Framework.DomainModels.Models.IParcel parcel) : base(parcel) {
      _parcel = (IActumParcel)parcel;
    }



    //See RawConverter, where the Actum input names are renamed to the Default names, which are then used in the Actum Parcel class. 
    //The following wrapper methods rename them back to the names used by Actum version.  
    public int DistrictID {
      get => (int)_parcel.CircuityRatio_E1;
      set => _parcel.CircuityRatio_E1 = value;
    }

    public int ParkingDataAvailable {
      get => (int)_parcel.CircuityRatio_E2;
      set => _parcel.CircuityRatio_E2 = value;
    }

    public double ResidentialPermitOnlyParkingSpaces {
      get => _parcel.CircuityRatio_E3;
      set => _parcel.CircuityRatio_E3 = value;
    }

    public double PublicWithResidentialPermitAllowedParkingSpaces {
      get => _parcel.CircuityRatio_NE1;
      set => _parcel.CircuityRatio_NE1 = value;
    }

    public double PublicNoResidentialPermitAllowedParkingSpaces {
      get => _parcel.CircuityRatio_NE2;
      set => _parcel.CircuityRatio_NE2 = value;
    }

    public double EmployeeOnlyParkingSpaces {
      get => _parcel.CircuityRatio_NE3;
      set => _parcel.CircuityRatio_NE3 = value;
    }

    public double ElectricVehicleOnlyParkingSpaces {
      get => _parcel.CircuityRatio_N1;
      set => _parcel.CircuityRatio_N1 = value;
    }

    public double ResidentialPermitDailyParkingPrices {
      get => _parcel.CircuityRatio_N2;
      set => _parcel.CircuityRatio_N2 = value;
    }

    public double PublicParkingHourlyPrice {
      get => _parcel.CircuityRatio_N3;
      set => _parcel.CircuityRatio_N3 = value;
    }

    public double ResidentialPermitOnlyParkingSpacesBuffer1 {
      get => _parcel.CircuityRatio_NW1;
      set => _parcel.CircuityRatio_NW1 = value;
    }

    public double PublicWithResidentialPermitAllowedParkingSpacesBuffer1 {
      get => _parcel.CircuityRatio_NW2;
      set => _parcel.CircuityRatio_NW2 = value;
    }

    public double PublicNoResidentialPermitAllowedParkingSpacesBuffer1 {
      get => _parcel.CircuityRatio_NW3;
      set => _parcel.CircuityRatio_NW3 = value;
    }

    public double EmployeeOnlyParkingSpacesBuffer1 {
      get => _parcel.CircuityRatio_W1;
      set => _parcel.CircuityRatio_W1 = value;
    }

    public double ElectricVehicleOnlyParkingSpacesBuffer1 {
      get => _parcel.CircuityRatio_W2;
      set => _parcel.CircuityRatio_W2 = value;
    }


    public double ResidentialPermitDailyParkingPricesBuffer1 {
      get => _parcel.CircuityRatio_W3;
      set => _parcel.CircuityRatio_W3 = value;
    }

    public double PublicParkingHourlyPriceBuffer1 {
      get => _parcel.CircuityRatio_SW1;
      set => _parcel.CircuityRatio_SW1 = value;
    }

    public double ResidentialPermitOnlyParkingSpacesBuffer2 {
      get => _parcel.CircuityRatio_SW2;
      set => _parcel.CircuityRatio_SW2 = value;
    }

    public double PublicWithResidentialPermitAllowedParkingSpacesBuffer2 {
      get => _parcel.CircuityRatio_SW3;
      set => _parcel.CircuityRatio_SW3 = value;
    }

    public double PublicNoResidentialPermitAllowedParkingSpacesBuffer2 {
      get => _parcel.CircuityRatio_S1;
      set => _parcel.CircuityRatio_S1 = value;
    }

    public double EmployeeOnlyParkingSpacesBuffer2 {
      get => _parcel.CircuityRatio_S2;
      set => _parcel.CircuityRatio_S2 = value;
    }

    public double ElectricVehicleOnlyParkingSpacesBuffer2 {
      get => _parcel.CircuityRatio_S3;
      set => _parcel.CircuityRatio_S3 = value;
    }

    public double ResidentialPermitDailyParkingPricesBuffer2 {
      get => _parcel.CircuityRatio_SE1;
      set => _parcel.CircuityRatio_SE1 = value;
    }

    public double PublicParkingHourlyPriceBuffer2 {
      get => _parcel.CircuityRatio_SE2;
      set => _parcel.CircuityRatio_SE2 = value;
    }

    public int DistrictID2 {
      get => (int)_parcel.CircuityRatio_SE3;
      set => _parcel.CircuityRatio_SE3 = value;
    }


    //public int FirstPositionInParkAndRideNodeDistanceArray { get; set; } = -1;

    //public int LastPositionInParkAndRideNodeDistanceArray { get; set; } = -1;

    //public bool ParkAndRideNodeDistanceArrayPositionsSet { get; set; } = false;


    //public int FirstPositionInStopAreaDistanceArray { get; set; } = -1;

    //public int LastPositionInStopAreaDistanceArray { get; set; } = -1;

    //public bool StopAreaDistanceArrayPositionsSet { get; set; } = false;

    public int FirstPositionInAutoParkAndRideNodeDistanceArray { get; set; } = -1;

    public int LastPositionInAutoParkAndRideNodeDistanceArray { get; set; } = -1;

    public bool AutoParkAndRideNodeDistanceArrayPositionsSet { get; set; } = false;

    public int FirstPositionInBikeParkAndRideNodeDistanceArray { get; set; } = -1;

    public int LastPositionInBikeParkAndRideNodeDistanceArray { get; set; } = -1;

    public bool BikeParkAndRideNodeDistanceArrayPositionsSet { get; set; } = false;

    public int FirstPositionInAutoKissAndRideTerminalDistanceArray { get; set; } = -1;

    public int LastPositionInAutoKissAndRideTerminalDistanceArray { get; set; } = -1;

    public bool AutoKissAndRideTerminalDistanceArrayPositionsSet { get; set; } = false;

    public int FirstPositionInBikeOnBoardTerminalDistanceArray { get; set; }

    public int LastPositionInBikeOnBoardTerminalDistanceArray { get; set; }

    public bool BikeOnBoardTerminalDistanceArrayPositionsSet { get; set; }

  }
}
