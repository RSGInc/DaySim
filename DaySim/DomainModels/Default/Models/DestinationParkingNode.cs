// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Default.Models {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  [Factory(Factory.PersistenceFactory, Category = Category.Model, DataType = DataType.Default)]
  public class DestinationParkingNode : IDestinationParkingNode {
    [ColumnName("ID")]
    public int Id { get; set; }

    [ColumnName("zone_id")]
    public int ZoneId { get; set; }

    [ColumnName("xcoord")]
    public int XCoordinate { get; set; }

    [ColumnName("ycoord")]
    public int YCoordinate { get; set; }
 
    [ColumnName("MicroZoneID")]
    public int ParcelId { get; set; }

    [ColumnName("LocationType")]
    public int LocationType { get; set; }

    [ColumnName("ParkingType")]
    public int ParkingType { get; set; }

    [ColumnName("Spaces")]
    public int Capacity { get; set; }

    [ColumnName("MaxDuration")]
    public double MaxDuration { get; set; }

    [ColumnName("FreeDuration")]
    public double FreeDuration { get; set; }

    [ColumnName("Price00_01")]
    public double Price12AM { get; set; }

    [ColumnName("Price01_02")]
    public double Price1AM { get; set; }

    [ColumnName("Price02_03")]
    public double Price2AM { get; set; }

    [ColumnName("Price03_04")]
    public double Price3AM { get; set; }

    [ColumnName("Price04_05")]
    public double Price4AM { get; set; }

    [ColumnName("Price05_06")]
    public double Price5AM { get; set; }

    [ColumnName("Price06_07")]
    public double Price6AM { get; set; }

    [ColumnName("Price07_08")]
    public double Price7AM { get; set; }

    [ColumnName("Price08_09")]
    public double Price8AM { get; set; }

    [ColumnName("Price09_10")]
    public double Price9AM { get; set; }

    [ColumnName("Price10_11")]
    public double Price10AM { get; set; }

    [ColumnName("Price11_12")]
    public double Price11AM { get; set; }

    [ColumnName("Price12_13")]
    public double Price12PM { get; set; }

    [ColumnName("Price13_14")]
    public double Price1PM { get; set; }

    [ColumnName("Price14_15")]
    public double Price2PM { get; set; }

    [ColumnName("Price15_16")]
    public double Price3PM { get; set; }

    [ColumnName("Price16_17")]
    public double Price4PM { get; set; }

    [ColumnName("Price17_18")]
    public double Price5PM { get; set; }

    [ColumnName("Price18_19")]
    public double Price6PM { get; set; }

    [ColumnName("Price19_20")]
    public double Price7PM { get; set; }

    [ColumnName("Price20_21")]
    public double Price8PM { get; set; }

    [ColumnName("Price21_22")]
    public double Price9PM { get; set; }

    [ColumnName("Price22_23")]
    public double Price10PM { get; set; }

    [ColumnName("Price23_24")]
    public double Price11PM { get; set; }

    [ColumnName("MinimumTime")]
    public double MinimumTime { get; set; }

    [ColumnName("MinimumPrice")]
    public double MinimumPrice { get; set; }

    [ColumnName("FullDayPrice")]
    public double FullDayPrice { get; set; }

    [ColumnName("MonthlyPass")]
    public double MonthlyPassDayPrice { get; set; }

    [ColumnName("ResidentPermitPassAvailable")]
    public int ResidentPermitPassAvailable { get; set; }

    [ColumnName("ResidentPermitPassPrice")]
    public double ResidentPermitPassDayPrice { get; set; }

    [ColumnName("OpeningTime")]
    public int OpeningTime { get; set; }

    [ColumnName("ClosingTime")]
    public int ClosingTime { get; set; }

    [ColumnName("Avail_Morning")]
    public double AvailabilityMorning { get; set; }

    [ColumnName("Avail_Afternoon")]
    public double AvailabilityAfternoon { get; set; }

    [ColumnName("Avail_Night")]
    public double AvailabilityNight { get; set; }

  }
}
