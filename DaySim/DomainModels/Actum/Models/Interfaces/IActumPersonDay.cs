// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Actum.Models.Interfaces {
  public interface IActumPersonDay : IPersonDay {
    int WorkHomeAllDay { get; set; }

    int MinutesStudiedHome { get; set; }

    int DiaryWeekday { get; set; }

    int DiaryDaytype { get; set; }

    int DayStartPurpose { get; set; }

    int DayJourneyType { get; set; }

    int BusinessTours { get; set; }

    int BusinessStops { get; set; }
  }
}