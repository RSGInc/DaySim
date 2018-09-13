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
  public class PersonDay : IActumPersonDay {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("person_id")]
    public int PersonId { get; set; }

    [ColumnName("household_day_id")]
    public int HouseholdDayId { get; set; }

    [ColumnName("hhno")]
    public int HouseholdId { get; set; }

    [ColumnName("pno")]
    public int PersonSequence { get; set; }

    [ColumnName("day")]
    public int Day { get; set; }

    [ColumnName("beghom")]
    public int DayBeginsAtHome { get; set; }

    [ColumnName("endhom")]
    public int DayEndsAtHome { get; set; }

    [ColumnName("hbtours")]
    public int HomeBasedTours { get; set; }

    [ColumnName("wbtours")]
    public int WorkBasedTours { get; set; }

    [ColumnName("uwtours")]
    public int UsualWorkplaceTours { get; set; }

    [ColumnName("wktours")]
    public int WorkTours { get; set; }

    [ColumnName("sctours")]
    public int SchoolTours { get; set; }

    [ColumnName("estours")]
    public int EscortTours { get; set; }

    [ColumnName("pbtours")]
    public int PersonalBusinessTours { get; set; }

    [ColumnName("shtours")]
    public int ShoppingTours { get; set; }

    [ColumnName("mltours")]
    public int MealTours { get; set; }

    [ColumnName("sotours")]
    public int SocialTours { get; set; }

    [ColumnName("retours")]
    public int RecreationTours { get; set; }

    [ColumnName("metours")]
    public int MedicalTours { get; set; }

    [ColumnName("wkstops")]
    public int WorkStops { get; set; }

    [ColumnName("scstops")]
    public int SchoolStops { get; set; }

    [ColumnName("esstops")]
    public int EscortStops { get; set; }

    [ColumnName("pbstops")]
    public int PersonalBusinessStops { get; set; }

    [ColumnName("shstops")]
    public int ShoppingStops { get; set; }

    [ColumnName("mlstops")]
    public int MealStops { get; set; }

    [ColumnName("sostops")]
    public int SocialStops { get; set; }

    [ColumnName("restops")]
    public int RecreationStops { get; set; }

    [ColumnName("mestops")]
    public int MedicalStops { get; set; }

    [ColumnName("wkathome")]
    public int WorkAtHomeDuration { get; set; }

    [ColumnName("pdexpfac")]
    public double ExpansionFactor { get; set; }

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