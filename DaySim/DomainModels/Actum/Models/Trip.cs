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
  public sealed class Trip : DomainModels.Default.Models.Trip, IActumTrip {

    [ColumnName("tourmode")]
    public int TourMode { get; set; }

    [ColumnName("acmode")]
    public int AccessMode { get; set; }

    [ColumnName("acpathtp")]
    public int AccessPathType { get; set; }

    [ColumnName("actime")]
    public double AccessTime { get; set; }

    [ColumnName("accost")]
    public double AccessCost { get; set; }

    [ColumnName("acdist")]
    public double AccessDistance { get; set; }

    [ColumnName("actermid")]
    public int AccessTerminalID { get; set; }

    [ColumnName("actermpcl")]
    public int AccessTerminalParcelID { get; set; }

    [ColumnName("actermtaz")]
    public int AccessTerminalZoneID { get; set; }

    [ColumnName("acparknode")]
    public int AccessParkingNodeID { get; set; }

    [ColumnName("egmode")]
    public int EgressMode { get; set; }

    [ColumnName("egpathtp")]
    public int EgressPathType { get; set; }

    [ColumnName("egtime")]
    public double EgressTime { get; set; }

    [ColumnName("egcost")]
    public double EgressCost { get; set; }

    [ColumnName("egdist")]
    public double EgressDistance { get; set; }

    [ColumnName("egtermid")]
    public int EgressTerminalID { get; set; }

    [ColumnName("egtermpcl")]
    public int EgressTerminalParcelID { get; set; }

    [ColumnName("egtermtaz")]
    public int EgressTerminalZoneID { get; set; }

    [ColumnName("egparknode")]
    public int EgressParkingNodeID { get; set; }

    [ColumnName("autotype")]
    public int AutoType { get; set; }

    [ColumnName("autoocc")]
    public int AutoOccupancy { get; set; }

  }
}
