// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface IHouseholdTotals {
    int FulltimeWorkers { get; set; }

    int PartTimeWorkers { get; set; }

    int RetiredAdults { get; set; }

    int NonworkingAdults { get; set; }

    int UniversityStudents { get; set; }

    int DrivingAgeStudents { get; set; }

    int ChildrenAge5Through15 { get; set; }

    int ChildrenUnder5 { get; set; }

    int ChildrenUnder16 { get; set; }

    int Adults { get; set; }

    int DrivingAgeMembers { get; set; }

    int WorkersPlusStudents { get; set; }

    int FullAndPartTimeWorkers { get; set; }

    int AllWorkers { get; set; }

    int AllStudents { get; set; }

    double PartTimeWorkersPerDrivingAgeMembers { get; set; }

    double RetiredAdultsPerDrivingAgeMembers { get; set; }

    double UniversityStudentsPerDrivingAgeMembers { get; set; }

    double DrivingAgeStudentsPerDrivingAgeMembers { get; set; }

    double ChildrenUnder5PerDrivingAgeMembers { get; set; }

    double HomeBasedPersonsPerDrivingAgeMembers { get; set; }
  }
}