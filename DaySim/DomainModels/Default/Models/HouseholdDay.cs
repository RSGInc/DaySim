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
  public sealed class HouseholdDay : IHouseholdDay {
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
  }
}