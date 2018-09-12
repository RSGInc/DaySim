// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Actum.Models.Interfaces {
  public interface IActumPerson : IPerson {
    int MainOccupation { get; set; }

    int EducationLevel { get; set; }

    int HasBike { get; set; }

    int HasDriversLicense { get; set; }

    int HasCarShare { get; set; }

    int Income { get; set; }

    int HasMC { get; set; }

    int HasMoped { get; set; }

    int HasWorkParking { get; set; }

    int WorkHoursPerWeek { get; set; }

    int FlexibleWorkHours { get; set; }

    int HasSchoolParking { get; set; }
  }
}