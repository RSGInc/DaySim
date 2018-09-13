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
  public sealed class Tour : IActumTour {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("person_id")]
    public int PersonId { get; set; }

    [ColumnName("person_day_id")]
    public int PersonDayId { get; set; }

    [ColumnName("hhno")]
    public int HouseholdId { get; set; }

    [ColumnName("pno")]
    public int PersonSequence { get; set; }

    [ColumnName("day")]
    public int Day { get; set; }

    [ColumnName("tour")]
    public int Sequence { get; set; }

    [ColumnName("jtindex")]
    public int JointTourSequence { get; set; }

    [ColumnName("parent")]
    public int ParentTourSequence { get; set; }

    [ColumnName("subtrs")]
    public int TotalSubtours { get; set; }

    [ColumnName("pdpurp")]
    public int DestinationPurpose { get; set; }

    [ColumnName("tlvorig")]
    public int OriginDepartureTime { get; set; }

    [ColumnName("tardest")]
    public int DestinationArrivalTime { get; set; }

    [ColumnName("tlvdest")]
    public int DestinationDepartureTime { get; set; }

    [ColumnName("tarorig")]
    public int OriginArrivalTime { get; set; }

    [ColumnName("toadtyp")]
    public int OriginAddressType { get; set; }

    [ColumnName("tdadtyp")]
    public int DestinationAddressType { get; set; }

    [ColumnName("topcl")]
    public int OriginParcelId { get; set; }

    [ColumnName("totaz")]
    public int OriginZoneKey { get; set; }

    [ColumnName("tdpcl")]
    public int DestinationParcelId { get; set; }

    [ColumnName("tdtaz")]
    public int DestinationZoneKey { get; set; }

    [ColumnName("tmodetp")]
    public int Mode { get; set; }

    [ColumnName("tpathtp")]
    public int PathType { get; set; }

    [ColumnName("tautotime")]
    public double AutoTimeOneWay { get; set; }

    [ColumnName("tautocost")]
    public double AutoCostOneWay { get; set; }

    [ColumnName("tautodist")]
    public double AutoDistanceOneWay { get; set; }

    [ColumnName("tripsh1")]
    public int HalfTour1Trips { get; set; }

    [ColumnName("tripsh2")]
    public int HalfTour2Trips { get; set; }

    [ColumnName("phtindx1")]
    public int PartialHalfTour1Sequence { get; set; }

    [ColumnName("phtindx2")]
    public int PartialHalfTour2Sequence { get; set; }

    [ColumnName("fhtindx1")]
    public int FullHalfTour1Sequence { get; set; }

    [ColumnName("fhtindx2")]
    public int FullHalfTour2Sequence { get; set; }

    [ColumnName("toexpfac")]
    public double ExpansionFactor { get; set; }
  }
}