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
    double ParkingCostPerHour8_18 { get; set; }
    double ParkingCostPerHour18_23 { get; set; }
    double ParkingCostPerHour23_08 { get; set; }
    double ResidentAnnualParkingCost { get; set; }
    double ParkingSearchTime21_05 { get; set; }
    double ParkingSearchTime05_06 { get; set; }
    double ParkingSearchTime06_07 { get; set; }
    double ParkingSearchTime07_08 { get; set; }
    double ParkingSearchTime08_09 { get; set; }
    double ParkingSearchTime09_15 { get; set; }
    double ParkingSearchTime15_16 { get; set; }
    double ParkingSearchTime16_17 { get; set; }
    double ParkingSearchTime17_18 { get; set; }
    double ParkingSearchTime18_21 { get; set; }

    double ParkingCostPerHour8_18Buffer1 { get; set; }
    double ParkingCostPerHour18_23Buffer1 { get; set; }
    double ParkingCostPerHour23_08Buffer1 { get; set; }
    double ResidentAnnualParkingCostBuffer1 { get; set; }
    double ParkingSearchTime21_05Buffer1 { get; set; }
    double ParkingSearchTime05_06Buffer1 { get; set; }
    double ParkingSearchTime06_07Buffer1 { get; set; }
    double ParkingSearchTime07_08Buffer1 { get; set; }
    double ParkingSearchTime08_09Buffer1 { get; set; }
    double ParkingSearchTime09_15Buffer1 { get; set; }
    double ParkingSearchTime15_16Buffer1 { get; set; }
    double ParkingSearchTime16_17Buffer1 { get; set; }
    double ParkingSearchTime17_18Buffer1 { get; set; }
    double ParkingSearchTime18_21Buffer1 { get; set; }

    double ParkingCostPerHour8_18Buffer2 { get; set; }
    double ParkingCostPerHour18_23Buffer2 { get; set; }
    double ParkingCostPerHour23_08Buffer2 { get; set; }
    double ResidentAnnualParkingCostBuffer2 { get; set; }
    double ParkingSearchTime21_05Buffer2 { get; set; }
    double ParkingSearchTime05_06Buffer2 { get; set; }
    double ParkingSearchTime06_07Buffer2 { get; set; }
    double ParkingSearchTime07_08Buffer2 { get; set; }
    double ParkingSearchTime08_09Buffer2 { get; set; }
    double ParkingSearchTime09_15Buffer2 { get; set; }
    double ParkingSearchTime15_16Buffer2 { get; set; }
    double ParkingSearchTime16_17Buffer2 { get; set; }
    double ParkingSearchTime17_18Buffer2 { get; set; }
    double ParkingSearchTime18_21Buffer2 { get; set; }

    int FirstPositionInParkAndRideNodeDistanceArray { get; set; }

    int LastPositionInParkAndRideNodeDistanceArray { get; set; }

    bool ParkAndRideNodeDistanceArrayPositionsSet { get; set; }

  }
}
