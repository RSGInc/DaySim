// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.DomainModels.Actum.Wrappers.Interfaces {
  public interface IActumParcelWrapper : IParcelWrapper, IActumParcel {
    // for actum

    int DistrictID { get; set; }

    int ParkingDataAvailable { get; set; }

    double ResidentialPermitOnlyParkingSpaces { get; set; }

    double PublicWithResidentialPermitAllowedParkingSpaces { get; set; }

    double PublicNoResidentialPermitAllowedParkingSpaces { get; set; }

    double EmployeeOnlyParkingSpaces { get; set; }

    double ElectricVehicleOnlyParkingSpaces { get; set; }

    double ResidentialPermitDailyParkingPrices { get; set; }

    double PublicParkingHourlyPrice { get; set; }

    double ResidentialPermitOnlyParkingSpacesBuffer1 { get; set; }

    double PublicWithResidentialPermitAllowedParkingSpacesBuffer1 { get; set; }

    double PublicNoResidentialPermitAllowedParkingSpacesBuffer1 { get; set; }

    double EmployeeOnlyParkingSpacesBuffer1 { get; set; }

    double ElectricVehicleOnlyParkingSpacesBuffer1 { get; set; }

    double ResidentialPermitDailyParkingPricesBuffer1 { get; set; }

    double PublicParkingHourlyPriceBuffer1 { get; set; }

    double ResidentialPermitOnlyParkingSpacesBuffer2 { get; set; }

    double PublicWithResidentialPermitAllowedParkingSpacesBuffer2 { get; set; }

    double PublicNoResidentialPermitAllowedParkingSpacesBuffer2 { get; set; }

    double EmployeeOnlyParkingSpacesBuffer2 { get; set; }

    double ElectricVehicleOnlyParkingSpacesBuffer2 { get; set; }

    double ResidentialPermitDailyParkingPricesBuffer2 { get; set; }

    double PublicParkingHourlyPriceBuffer2 { get; set; }

    int NearestTerminalID { get; set; }

    //int FirstPositionInStopAreaDistanceArray { get; set; } 

    //int LastPositionInStopAreaDistanceArray { get; set; }

    //bool StopAreaDistanceArrayPositionsSet { get; set; }

    int FirstPositionInAutoParkAndRideNodeDistanceArray { get; set; }

    int LastPositionInAutoParkAndRideNodeDistanceArray { get; set; }

    bool AutoParkAndRideNodeDistanceArrayPositionsSet { get; set; }

    int FirstPositionInBikeParkAndRideNodeDistanceArray { get; set; }

    int LastPositionInBikeParkAndRideNodeDistanceArray { get; set; }

    bool BikeParkAndRideNodeDistanceArrayPositionsSet { get; set; }

    int FirstPositionInAutoKissAndRideTerminalDistanceArray { get; set; }

    int LastPositionInAutoKissAndRideTerminalDistanceArray { get; set; }

    bool AutoKissAndRideTerminalDistanceArrayPositionsSet { get; set; }

    int FirstPositionInBikeOnBoardTerminalDistanceArray { get; set; }

    int LastPositionInBikeOnBoardTerminalDistanceArray { get; set; }

    bool BikeOnBoardTerminalDistanceArrayPositionsSet { get; set; }
  }
}
