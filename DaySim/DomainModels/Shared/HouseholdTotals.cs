// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Shared {
  public sealed class HouseholdTotals : IHouseholdTotals {
    public int FulltimeWorkers { get; set; }

    public int PartTimeWorkers { get; set; }

    public int RetiredAdults { get; set; }

    public int NonworkingAdults { get; set; }

    public int UniversityStudents { get; set; }

    public int DrivingAgeStudents { get; set; }

    public int ChildrenAge5Through15 { get; set; }

    public int ChildrenUnder5 { get; set; }

    public int ChildrenUnder16 { get; set; }

    public int Adults { get; set; }

    public int DrivingAgeMembers { get; set; }

    public int WorkersPlusStudents { get; set; }

    public int FullAndPartTimeWorkers { get; set; }

    public int AllWorkers { get; set; }

    public int AllStudents { get; set; }

    public double PartTimeWorkersPerDrivingAgeMembers { get; set; }

    public double RetiredAdultsPerDrivingAgeMembers { get; set; }

    public double UniversityStudentsPerDrivingAgeMembers { get; set; }

    public double DrivingAgeStudentsPerDrivingAgeMembers { get; set; }

    public double ChildrenUnder5PerDrivingAgeMembers { get; set; }

    public double HomeBasedPersonsPerDrivingAgeMembers { get; set; }
  }
}