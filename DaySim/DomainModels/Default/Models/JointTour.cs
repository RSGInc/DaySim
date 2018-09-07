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
  public sealed class JointTour : IJointTour {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("household_day_id")]
    public int HouseholdDayId { get; set; }

    [ColumnName("qno")]
    public int HouseholdId { get; set; }

    [ColumnName("day")]
    public int Day { get; set; }

    [ColumnName("jtour")]
    public int Sequence { get; set; }

    [ColumnName("jtpurp")]
    public int MainPurpose { get; set; }

    [ColumnName("jtnpart")]
    public int Participants { get; set; }

    [ColumnName("jtpers1")]
    public int PersonSequence1 { get; set; }

    [ColumnName("jtptno1")]
    public int TourSequence1 { get; set; }

    [ColumnName("jtpers2")]
    public int PersonSequence2 { get; set; }

    [ColumnName("jtptno2")]
    public int TourSequence2 { get; set; }

    [ColumnName("jtpers3")]
    public int PersonSequence3 { get; set; }

    [ColumnName("jtptno3")]
    public int TourSequence3 { get; set; }

    [ColumnName("jtpers4")]
    public int PersonSequence4 { get; set; }

    [ColumnName("jtptno4")]
    public int TourSequence4 { get; set; }

    [ColumnName("jtpers5")]
    public int PersonSequence5 { get; set; }

    [ColumnName("jtptno5")]
    public int TourSequence5 { get; set; }

    [ColumnName("jtpers6")]
    public int PersonSequence6 { get; set; }

    [ColumnName("jtptno6")]
    public int TourSequence6 { get; set; }

    [ColumnName("jtpers7")]
    public int PersonSequence7 { get; set; }

    [ColumnName("jtptno7")]
    public int TourSequence7 { get; set; }

    [ColumnName("jtpers8")]
    public int PersonSequence8 { get; set; }

    [ColumnName("jtptno8")]
    public int TourSequence8 { get; set; }
  }
}