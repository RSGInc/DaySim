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
  public class PersonDay : DomainModels.Default.Models.PersonDay, IActumPersonDay {
    //JLB 20161323
    [ColumnName("ppattype")]
    public int PatternType { get; set; }

    [ColumnName("pwrkhday")]
    public int WorkHomeAllDay { get; set; }

    [ColumnName("pschhmin")]
    public int MinutesStudiedHome { get; set; }

    [ColumnName("pweekday")]
    public int DiaryWeekday { get; set; }

    [ColumnName("pdaytype")]
    public int DiaryDaytype { get; set; }

    [ColumnName("pdayspurp")]
    public int DayStartPurpose { get; set; }

    [ColumnName("pdayjtyp")]
    public int DayJourneyType { get; set; }

    [ColumnName("butours")]
    public int BusinessTours { get; set; }

    [ColumnName("bustops")]
    public int BusinessStops { get; set; }
  }
}
