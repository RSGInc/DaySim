// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using DaySim.DomainModels.Default.Models;

namespace DaySim.DomainModels.Extensions {
  public static class ZoneTotalsExtensions {
    public static double GetEmploymentEducationDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.EmploymentEducation * 100 / squareLengthUnits);
    }

    public static double GetEmploymentGovernmentDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.EmploymentGovernment * 100 / squareLengthUnits);
    }

    public static double GetEmploymentOfficeDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.EmploymentOffice * 100 / squareLengthUnits);
    }

    public static double GetEmploymentRetailDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.EmploymentRetail * 100 / squareLengthUnits);
    }

    public static double GetEmploymentServiceDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.EmploymentService * 100 / squareLengthUnits);
    }

    public static double GetEmploymentMedicalDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.EmploymentMedical * 100 / squareLengthUnits);
    }

    public static double GetHouseholdsDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.Households * 100 / squareLengthUnits);
    }

    public static double GetStudentsK12Density(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.StudentsK12 * 100 / squareLengthUnits);
    }

    public static double GetStudentsUniversityDensity(this ZoneTotals zoneTotals, double squareLengthUnits) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return Math.Log(1 + zoneTotals.StudentsUniversity * 100 / squareLengthUnits);
    }

    public static void SumTotals(this ZoneTotals zoneTotals, Parcel parcel) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      if (parcel == null) {
        throw new ArgumentNullException("parcel");
      }

      zoneTotals.ThousandsSquareLengthUnits += parcel.ThousandsSquareLengthUnits;
      zoneTotals.EmploymentEducation += parcel.EmploymentEducation;
      zoneTotals.EmploymentFood += parcel.EmploymentFood;
      zoneTotals.EmploymentGovernment += parcel.EmploymentGovernment;
      zoneTotals.EmploymentOffice += parcel.EmploymentOffice;
      zoneTotals.EmploymentRetail += parcel.EmploymentRetail;
      zoneTotals.EmploymentService += parcel.EmploymentService;
      zoneTotals.EmploymentMedical += parcel.EmploymentMedical;
      zoneTotals.EmploymentIndustrial += parcel.EmploymentIndustrial;
      zoneTotals.EmploymentTotal += parcel.EmploymentTotal;
      zoneTotals.Households += parcel.Households;
      zoneTotals.StudentsUniversity += parcel.StudentsUniversity;
      zoneTotals.ParkingOffStreetPaidDailySpaces += parcel.ParkingOffStreetPaidDailySpaces;
      zoneTotals.StudentsK8 += parcel.StudentsK8;
      zoneTotals.StudentsHighSchool += parcel.StudentsHighSchool;
      zoneTotals.StudentsK12 += (parcel.StudentsK8 + parcel.StudentsHighSchool);
    }

    public static double MillionsSquareLengthUnits(this ZoneTotals zoneTotals) {
      if (zoneTotals == null) {
        throw new ArgumentNullException("zoneTotals");
      }

      return zoneTotals.ThousandsSquareLengthUnits / 1000 + .0001;
    }
  }
}