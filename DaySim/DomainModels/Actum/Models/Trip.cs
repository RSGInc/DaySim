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
  public sealed class Trip : IActumTrip {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("tour_id")]
    public int TourId { get; set; }

    [ColumnName("hhno")]
    public int HouseholdId { get; set; }

    [ColumnName("pno")]
    public int PersonSequence { get; set; }

    [ColumnName("day")]
    public int Day { get; set; }

    [ColumnName("tour")]
    public int TourSequence { get; set; }

    [ColumnName("half")]
    public int Direction { get; set; }

    [ColumnName("tseg")]
    public int Sequence { get; set; }

    [ColumnName("tsvid")]
    public int SurveyTripSequence { get; set; }

    [ColumnName("opurp")]
    public int OriginPurpose { get; set; }

    [ColumnName("dpurp")]
    public int DestinationPurpose { get; set; }

    [ColumnName("oadtyp")]
    public int OriginAddressType { get; set; }

    [ColumnName("dadtyp")]
    public int DestinationAddressType { get; set; }

    [ColumnName("opcl")]
    public int OriginParcelId { get; set; }

    [ColumnName("otaz")]
    public int OriginZoneKey { get; set; }

    [ColumnName("dpcl")]
    public int DestinationParcelId { get; set; }

    [ColumnName("dtaz")]
    public int DestinationZoneKey { get; set; }

    [ColumnName("mode")]
    public int Mode { get; set; }

    [ColumnName("pathtype")]
    public int PathType { get; set; }

    [ColumnName("dorp")]
    public int DriverType { get; set; }

    [ColumnName("deptm")]
    public int DepartureTime { get; set; }

    [ColumnName("arrtm")]
    public int ArrivalTime { get; set; }

    [ColumnName("endacttm")]
    public int ActivityEndTime { get; set; }

    [ColumnName("travtime")]
    public double TravelTime { get; set; }

    [ColumnName("travcost")]
    public double TravelCost { get; set; }

    [ColumnName("travdist")]
    public double TravelDistance { get; set; }

    [ColumnName("vot")]
    public double ValueOfTime { get; set; }

    [ColumnName("trexpfac")]
    public double ExpansionFactor { get; set; }

    [ColumnName("tdescpur")]
    public int EscortedDestinationPurpose { get; set; }

    [ColumnName("tptbicty")]
    public int BikePTCombination { get; set; }

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

    [ColumnName("acstopar")]
    public int AccessStopArea { get; set; }

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

    [ColumnName("egstopar")]
    public int EgressStopArea { get; set; }
  }
}