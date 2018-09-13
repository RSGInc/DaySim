// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;
using DaySim.Framework.DomainModels.Models;
//using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Actum.Models {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  [Factory(Factory.PersistenceFactory, Category = Category.Model, DataType = DataType.Actum)]
  public sealed class ParkAndRideNode : IParkAndRideNode {
    //	public sealed class ParkAndRideNode {     //JLB 201505 Removed interface requirement so I could eliminate zoneID, and x and y coordinates

    [ColumnName("id")]               //sequential ID of parking node not in raw input, added by RawConverter
    public int Id { get; set; }

    [ColumnName("ParkAndRideNodeId")]
    public int ZoneId { get; set; }

    [ColumnName("xcoord")]   // included for compatibility with IParkAndRideNode; not in input data
    public int XCoordinate { get; set; }

    [ColumnName("ycoord")]   // included for compatibility with IParkAndRideNode; not in input data
    public int YCoordinate { get; set; }

    //[ColumnName("TerminalName")]
    //public string TerminalName { get; set; }

    [ColumnName("TerminalId")]
    public int NearestStopAreaId { get; set; }

    [ColumnName("MicroZoneId")]
    public int NearestParcelId { get; set; }

    [ColumnName("ParkingTypeId")]
    public int ParkingTypeId { get; set; }

    [ColumnName("Capacity")]
    public int Capacity { get; set; }

    [ColumnName("CostPerHour08_18")]
    public double CostPerHour08_18 { get; set; }

    [ColumnName("CostPerHour18_23")]
    public double CostPerHour18_23 { get; set; }

    [ColumnName("CostPerHour23_08")]
    public double CostPerHour23_08 { get; set; }

    [ColumnName("CostAnnual")]
    public double CostAnnual { get; set; }

    [ColumnName("CostDaily")]
    public int Cost { get; set; }

    [ColumnName("PRFacility")]
    public int PRFacility { get; set; }

    [ColumnName("Distance")]
    public int LengthToStopArea { get; set; }

    [ColumnName("Auto")]
    public int Auto { get; set; }






  }
}