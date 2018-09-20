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

    [ColumnName("MAZ")]
    public int ParcelId { get; set; }

    [ColumnName("NODE")]
    public int NodeId { get; set; }

    [ColumnName("TYPE")]
    public int ParkingType { get; set; }

    [ColumnName("MAXDUR")]
    public int MaxDuration { get; set; }

    [ColumnName("CAPACITY")]
    public int Capacity { get; set; }

    [ColumnName("PREOCCDAY")]
    public double PreOccupiedDay { get; set; }

    [ColumnName("PEROCCOTH")]
    public double PreOccupiedOther { get; set; }

    [ColumnName("PRICE7A")]
    public double Price7AM { get; set; }

    [ColumnName("PRICE8A")]
    public double Price8AM { get; set; }

    [ColumnName("PRICE9A")]
    public double Price9AM { get; set; }

    [ColumnName("PRICE10A")]
    public double Price10AM { get; set; }

    [ColumnName("PRICE11A")]
    public double Price11AM { get; set; }

    [ColumnName("PRICE12P")]
    public double Price12PM { get; set; }

    [ColumnName("PRICE1P")]
    public double Price1PM { get; set; }

    [ColumnName("PRICE2P")]
    public double Price2PM { get; set; }

    [ColumnName("PRICE3P")]
    public double Price3PM { get; set; }

    [ColumnName("PRICE4P")]
    public double Price4PM { get; set; }

    [ColumnName("PRICE5P")]
    public double Price5PM { get; set; }

    [ColumnName("PRICE6P")]
    public double Price6PM { get; set; }

    [ColumnName("PRICE7P")]
    public double Price7PM { get; set; }

    [ColumnName("PRICE8P")]
    public double Price8PM { get; set; }

    [ColumnName("PRICE9P")]
    public double Price9PM { get; set; }

    [ColumnName("PRICE10P")]
    public double Price10PM { get; set; }

    [ColumnName("PRICE11P")]
    public double Price11PM { get; set; }

    [ColumnName("PRICE12A")]
    public double Price12AM { get; set; }
  }
}
