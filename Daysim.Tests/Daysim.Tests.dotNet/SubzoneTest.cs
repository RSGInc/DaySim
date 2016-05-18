using System;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Extensions;
using Xunit;

namespace Daysim.Tests 
{
	public class SubzoneTest 
	{
		[Fact]
		public void TetsSubZone()
		{
			int sequence = 1;
			double households = 2;
			double studentsK8 = 3;
			double studentsHighSchool = 4;
			double studentsUniversity = 5;
			double employmentEducation = 6;
			double employmentFood = 7;
			double employmentGovernment = 8;
			double employmentIndustrial = 9;
			double employmentMedical = 10;
			double employmentOffice = 11;
			double employmentRetail = 12;
			double employmentService = 13;
			double employmentTotal = 14;
			double parkingOffStreetPaidDailySpaces = 15;
			double parkingOffStreetPaidHourlySpaces = 16;
			double mixedUseMeasure = 17;

			Subzone subzone = new Subzone(sequence)
				                  {
														EmploymentEducation = employmentEducation,
														EmploymentFood = employmentFood,
														EmploymentGovernment = employmentGovernment,
														EmploymentIndustrial = employmentIndustrial,
														EmploymentMedical = employmentMedical,
														EmploymentOffice = employmentOffice,
														EmploymentRetail = employmentRetail,
														EmploymentService = employmentService,
														EmploymentTotal = employmentTotal,
														Households = households,
														MixedUseMeasure = mixedUseMeasure,
														ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
														ParkingOffStreetPaidHourlySpaces = parkingOffStreetPaidHourlySpaces,
														StudentsHighSchool = studentsHighSchool,
														StudentsK8 = studentsK8,
														StudentsUniversity = studentsUniversity,
														};
			Assert.Equal(employmentEducation, subzone.EmploymentEducation);
			Assert.Equal(employmentFood, subzone.EmploymentFood);
			Assert.Equal(employmentGovernment, subzone.EmploymentGovernment);
			Assert.Equal(employmentIndustrial, subzone.EmploymentIndustrial);
			Assert.Equal(employmentMedical, subzone.EmploymentMedical);
			Assert.Equal(employmentOffice, subzone.EmploymentOffice);
			Assert.Equal(employmentRetail, subzone.EmploymentRetail);
			Assert.Equal(employmentService, subzone.EmploymentService);
			Assert.Equal(employmentTotal, subzone.EmploymentTotal);
			Assert.Equal(households, subzone.Households);
			Assert.Equal(mixedUseMeasure, subzone.MixedUseMeasure);
			Assert.Equal(parkingOffStreetPaidDailySpaces, subzone.ParkingOffStreetPaidDailySpaces);
			Assert.Equal(parkingOffStreetPaidHourlySpaces, subzone.ParkingOffStreetPaidHourlySpaces);
			Assert.Equal(studentsHighSchool, subzone.StudentsHighSchool);
			Assert.Equal(studentsK8, subzone.StudentsK8);
			Assert.Equal(studentsUniversity, subzone.StudentsUniversity);
		}

		[Fact]
		public void TestZone()
		{
			int id = 1;
			int key = 2;
			bool destinationEligible = true;
			int external = 1;
			double fractionWorkersWithJobsOutsideRegion = 3.01;
			double fractionJobsFilledByWorkersFromOutsideRegion = 4.01;

			Zone zone = new Zone()
				            {
					            DestinationEligible = destinationEligible,
					            External = external,
					            FractionWorkersWithJobsOutsideRegion = fractionWorkersWithJobsOutsideRegion,
					            FractionJobsFilledByWorkersFromOutsideRegion = fractionJobsFilledByWorkersFromOutsideRegion,
					            Id = id,
					            Key = key,
				            };

			Assert.Equal(destinationEligible, zone.DestinationEligible);
			Assert.Equal(external, zone.External);
			Assert.Equal(fractionWorkersWithJobsOutsideRegion, zone.FractionWorkersWithJobsOutsideRegion);
			Assert.Equal(fractionJobsFilledByWorkersFromOutsideRegion, zone.FractionJobsFilledByWorkersFromOutsideRegion);
			Assert.Equal(id, zone.Id);
			Assert.Equal(key, zone.Key);
		}

		[Fact]
		public void TestZoneTotals()
		{
			double thousandsSquareLengthUnits = 1.01;
			double employmentEducation = 2.01;
			double employmentFood = 3.01;
			double employmentGovernment = 4.01;
			double employmentOffice = 5.01;
			double employmentRetail = 6.01;
			double employmentService = 7.01;
			double employmentMedical = 8.01;
			double employmentIndustrial = 9.01;
			double employmentTotal = 10.01;
			double households = 11.01;
			double studentsUniversity = 12.01;
			double parkingOffStreetPaidDailySpaces = 13.01;
			double studentsK8 = 14.01;
			double studentsHighSchool = 15.01;
			double studentsK12 = 16.01;

			ZoneTotals totals = new ZoneTotals()
				                    {
					                    EmploymentEducation = employmentEducation,
					                    EmploymentFood = employmentFood,
					                    EmploymentGovernment = employmentGovernment,
					                    EmploymentIndustrial = employmentIndustrial,
					                    EmploymentMedical = employmentMedical,
					                    EmploymentOffice = employmentOffice,
					                    EmploymentRetail = employmentRetail,
					                    EmploymentService = employmentService,
					                    EmploymentTotal = employmentTotal,
					                    Households = households,
					                    ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                    StudentsHighSchool = studentsHighSchool,
					                    StudentsK12 = studentsK12,
					                    StudentsK8 = studentsK8,
					                    StudentsUniversity = studentsUniversity,
					                    ThousandsSquareLengthUnits = thousandsSquareLengthUnits,
				                    };
			Assert.Equal(employmentEducation, totals.EmploymentEducation);
			Assert.Equal(employmentFood, totals.EmploymentFood);
			Assert.Equal(employmentGovernment, totals.EmploymentGovernment);
			Assert.Equal(employmentIndustrial, totals.EmploymentIndustrial);
			Assert.Equal(employmentMedical, totals.EmploymentMedical);
			Assert.Equal(employmentOffice, totals.EmploymentOffice);
			Assert.Equal(employmentRetail, totals.EmploymentRetail);
			Assert.Equal(employmentService, totals.EmploymentService);
			Assert.Equal(employmentTotal, totals.EmploymentTotal);
			Assert.Equal(households, totals.Households);
			Assert.Equal(parkingOffStreetPaidDailySpaces, totals.ParkingOffStreetPaidDailySpaces);
			Assert.Equal(studentsHighSchool, totals.StudentsHighSchool);
			Assert.Equal(studentsK12, totals.StudentsK12);
			Assert.Equal(studentsK8, totals.StudentsK8);
			Assert.Equal(studentsUniversity, totals.StudentsUniversity);
		}

		[Fact]
		public void TestZoneTotalsExtensions()
		{
			double thousandsSquareLengthUnits = 1.01;
			double employmentEducation = 2.01;
			double employmentFood = 3.01;
			double employmentGovernment = 4.01;
			double employmentOffice = 5.01;
			double employmentRetail = 6.01;
			double employmentService = 7.01;
			double employmentMedical = 8.01;
			double employmentIndustrial = 9.01;
			double employmentTotal = 10.01;
			double households = 11.01;
			double studentsUniversity = 12.01;
			double parkingOffStreetPaidDailySpaces = 13.01;
			double studentsK8 = 14.01;
			double studentsHighSchool = 15.01;
			double studentsK12 = 16.01;

			ZoneTotals totals = new ZoneTotals()
				                    {
					                    EmploymentEducation = employmentEducation,
					                    EmploymentFood = employmentFood,
					                    EmploymentGovernment = employmentGovernment,
					                    EmploymentIndustrial = employmentIndustrial,
					                    EmploymentMedical = employmentMedical,
					                    EmploymentOffice = employmentOffice,
					                    EmploymentRetail = employmentRetail,
					                    EmploymentService = employmentService,
					                    EmploymentTotal = employmentTotal,
					                    Households = households,
					                    ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                    StudentsHighSchool = studentsHighSchool,
					                    StudentsK12 = studentsK12,
					                    StudentsK8 = studentsK8,
					                    StudentsUniversity = studentsUniversity,
					                    ThousandsSquareLengthUnits = thousandsSquareLengthUnits,
				                    };

			double squareLengthUnits = 100;
			double density = Math.Log(1 + employmentEducation * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetEmploymentEducationDensity(squareLengthUnits));

			density = Math.Log(1 + employmentGovernment * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetEmploymentGovernmentDensity(squareLengthUnits));

			density = Math.Log(1 + employmentMedical * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetEmploymentMedicalDensity(squareLengthUnits));

			density = Math.Log(1 + employmentOffice * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetEmploymentOfficeDensity(squareLengthUnits));

			density = Math.Log(1 + employmentRetail * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetEmploymentRetailDensity(squareLengthUnits));
			
			density = Math.Log(1 + employmentService * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetEmploymentServiceDensity(squareLengthUnits));

			density = Math.Log(1 + households * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetHouseholdsDensity(squareLengthUnits));

			density = Math.Log(1 + studentsK12 * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetStudentsK12Density(squareLengthUnits));

			density = Math.Log(1 + studentsUniversity * 100 / squareLengthUnits);
			Assert.Equal(density, totals.GetStudentsUniversityDensity(squareLengthUnits));

			density = thousandsSquareLengthUnits/1000 + .0001;
			Assert.Equal(density, totals.MillionsSquareLengthUnits());

			Parcel parcel = new Parcel
				                {
													EmploymentEducation = employmentEducation,
													EmploymentFood = employmentFood,
													EmploymentGovernment = employmentGovernment,
													EmploymentIndustrial = employmentIndustrial,
													EmploymentMedical = employmentMedical,
													EmploymentOffice = employmentOffice,
													EmploymentRetail = employmentRetail,
													EmploymentService = employmentService,
													EmploymentTotal = employmentTotal,
													ThousandsSquareLengthUnits = thousandsSquareLengthUnits,
													Households = households,
													ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
													StudentsK8 = studentsK8,
													StudentsHighSchool = studentsHighSchool,
													StudentsUniversity = studentsUniversity,
				                };
			totals = new ZoneTotals();
			totals.SumTotals(parcel);
			Assert.Equal(employmentEducation, totals.EmploymentEducation);
			Assert.Equal(employmentFood, totals.EmploymentFood);
			Assert.Equal(employmentGovernment, totals.EmploymentGovernment);
			Assert.Equal(employmentIndustrial, totals.EmploymentIndustrial);
			Assert.Equal(employmentMedical, totals.EmploymentMedical);
			Assert.Equal(employmentOffice, totals.EmploymentOffice);
			Assert.Equal(employmentRetail, totals.EmploymentRetail);
			Assert.Equal(employmentService, totals.EmploymentService);
			Assert.Equal(employmentTotal, totals.EmploymentTotal);
			Assert.Equal(households, totals.Households);
			Assert.Equal(parkingOffStreetPaidDailySpaces, totals.ParkingOffStreetPaidDailySpaces);
			Assert.Equal(studentsHighSchool, totals.StudentsHighSchool);
			Assert.Equal(studentsK8 + studentsHighSchool, totals.StudentsK12);
			Assert.Equal(studentsK8, totals.StudentsK8);
			Assert.Equal(studentsUniversity, totals.StudentsUniversity);

			totals.SumTotals(parcel);
			Assert.Equal(2 * employmentEducation, totals.EmploymentEducation);
			Assert.Equal(2 * employmentFood, totals.EmploymentFood);
			Assert.Equal(2 * employmentGovernment, totals.EmploymentGovernment);
			Assert.Equal(2 * employmentIndustrial, totals.EmploymentIndustrial);
			Assert.Equal(2 * employmentMedical, totals.EmploymentMedical);
			Assert.Equal(2 * employmentOffice, totals.EmploymentOffice);
			Assert.Equal(2 * employmentRetail, totals.EmploymentRetail);
			Assert.Equal(2 * employmentService, totals.EmploymentService);
			Assert.Equal(2 * employmentTotal, totals.EmploymentTotal);
			Assert.Equal(2 * households, totals.Households);
			Assert.Equal(2 * parkingOffStreetPaidDailySpaces, totals.ParkingOffStreetPaidDailySpaces);
			Assert.Equal(2 * studentsHighSchool, totals.StudentsHighSchool);
			Assert.Equal(2 * (studentsHighSchool + studentsK8), totals.StudentsK12);
			Assert.Equal(2 * studentsK8, totals.StudentsK8);
			Assert.Equal(2 * studentsUniversity, totals.StudentsUniversity);
		}
	}
}
