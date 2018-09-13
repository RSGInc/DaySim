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

namespace DaySim.DomainModels.Actum.Models {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  [Factory(Factory.PersistenceFactory, Category = Category.Model, DataType = DataType.Actum)]
  public sealed class TransitStopArea : ITransitStopArea {
    [ColumnName("id")]
    public int Id { get; set; }

    //[ColumnName("Name")]
    //public string TerminalName { get; set; }

    [ColumnName("taz")]  // this is the TerminalId
    public int Key { get; set; }

    [ColumnName("MicroZoneID")]
    public int Microzone { get; set; }

    //[ColumnName("Dest_eligible")]
    //public bool DestinationEligible { get; set; }

    //[ColumnName("External")]
    //public bool External { get; set; }

    [ColumnName("POINT_X")]
    public int XCoordinate { get; set; }

    [ColumnName("POINT_Y")]
    public int YCoordinate { get; set; }

    [ColumnName("Bike_on_board")]
    public int BikeOnBoardTerminal { get; set; }

    //[ColumnName("fraction_with_jobs_outside")]
    //public double FractionWorkersWithJobsOutsideRegion { get; set; }

    //[ColumnName("fraction_filled_by_workers_from_outside")]
    //public double FractionJobsFilledByWorkersFromOutsideRegion { get; set; }
  }
}