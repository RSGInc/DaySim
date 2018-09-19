// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Actum.Models {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  [Factory(Factory.PersistenceFactory, Category = Category.Model, DataType = DataType.Actum)]
  public sealed class TransitStopArea : DomainModels.Default.Models.TransitStopArea, IActumTransitStopArea {
    [ColumnName("MicroZoneID")]
    public int Microzone { get; set; }

    [ColumnName("Bike_on_board")]
    public int BikeOnBoardTerminal { get; set; }

    //[ColumnName("fraction_with_jobs_outside")]
    //public double FractionWorkersWithJobsOutsideRegion { get; set; }

    //[ColumnName("fraction_filled_by_workers_from_outside")]
    //public double FractionJobsFilledByWorkersFromOutsideRegion { get; set; }
  }
}
