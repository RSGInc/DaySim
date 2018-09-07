// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Default.Models {
  public sealed class ZoneTotals : IZoneTotals {
    public double ThousandsSquareLengthUnits { get; set; }

    public double EmploymentEducation { get; set; }

    public double EmploymentFood { get; set; }

    public double EmploymentGovernment { get; set; }

    public double EmploymentOffice { get; set; }

    public double EmploymentRetail { get; set; }

    public double EmploymentService { get; set; }

    public double EmploymentMedical { get; set; }

    public double EmploymentIndustrial { get; set; }

    public double EmploymentTotal { get; set; }

    public double Households { get; set; }

    public double StudentsUniversity { get; set; }

    public double ParkingOffStreetPaidDailySpaces { get; set; }

    public double StudentsK8 { get; set; }

    public double StudentsHighSchool { get; set; }

    public double StudentsK12 { get; set; }
  }
}