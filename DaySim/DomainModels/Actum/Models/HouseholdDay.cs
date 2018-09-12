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
  public sealed class HouseholdDay : IActumHouseholdDay {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("hhno")]
    public int HouseholdId { get; set; }

    [ColumnName("day")]
    public int Day { get; set; }

    [ColumnName("dow")]
    public int DayOfWeek { get; set; }

    [ColumnName("jttours")]
    public int JointTours { get; set; }

    [ColumnName("phtours")]
    public int PartialHalfTours { get; set; }

    [ColumnName("fhtours")]
    public int FullHalfTours { get; set; }

    [ColumnName("hdexpfac")]
    public double ExpansionFactor { get; set; }

    [ColumnName("pfptfrq")]
    public int SharedActivityHomeStays { get; set; }

    [ColumnName("pfptmax")]
    public int NumberInLargestSharedHomeStay { get; set; }

    [ColumnName("pfptbeg")]
    public int StartingMinuteSharedHomeStay { get; set; }

    [ColumnName("pfptdur")]
    public int DurationMinutesSharedHomeStay { get; set; }

    [ColumnName("pfptad")]
    public int AdultsInSharedHomeStay { get; set; }

    [ColumnName("pfptch")]
    public int ChildrenInSharedHomeStay { get; set; }

    [ColumnName("pfpt")]
    public int PrimaryPriorityTimeFlag { get; set; }
  }
}