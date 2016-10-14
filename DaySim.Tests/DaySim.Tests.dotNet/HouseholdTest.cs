using System;
using System.Collections.Generic;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Shared;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Xunit;
using SimpleInjector;

namespace Daysim.Tests 
{
	


	public class HouseholdTest 
	{
		[Fact]
		public void TestHouseholdTotals()
		{
			const int fulltimeWorkers = 1;
			const int partTimeWorkers = 2;
			const int retiredAdults = 3;
			const int nonworkingAdults = 4;
			const int universityStudents = 5;
			const int drivingAgeStudents = 6;
			const int childrenAge5Through15 = 7;
			const int childrenUnder5 = 8;
			const int childrenUnder16 = 9;
			const int adults = 10;
			const int drivingAgeMembers = 11;
			const int workersPlusStudents = 12;
			const int fullAndPartTimeWorkers = 13;
			const int allWorkers = 14;
			const int allStudents = 15;
			const double partTimeWorkersPerDrivingAgeMembers = 16.1;
			const double retiredAdultsPerDrivingAgeMembers = 17.1;
			const double universityStudentsPerDrivingAgeMembers = 18.1;
			const double drivingAgeStudentsPerDrivingAgeMembers = 19.1;
			const double homeBasedPersonsPerDrivingAgeMembers = 21.1;

			HouseholdTotals totals = new HouseholdTotals
				                         {
					                         Adults = adults,
					                         AllStudents = allStudents,
					                         AllWorkers = allWorkers,
					                         ChildrenAge5Through15 = childrenAge5Through15,
					                         ChildrenUnder16 = childrenUnder16,
					                         ChildrenUnder5 = childrenUnder5,
					                         ChildrenUnder5PerDrivingAgeMembers = childrenAge5Through15,
					                         DrivingAgeMembers = drivingAgeMembers,
																	 DrivingAgeStudents = drivingAgeStudents,
																	 DrivingAgeStudentsPerDrivingAgeMembers = drivingAgeStudentsPerDrivingAgeMembers,
																	 FullAndPartTimeWorkers = fullAndPartTimeWorkers,
																	 FulltimeWorkers = fulltimeWorkers,
																	 HomeBasedPersonsPerDrivingAgeMembers = homeBasedPersonsPerDrivingAgeMembers,
																	 NonworkingAdults = nonworkingAdults,
																	 PartTimeWorkers = partTimeWorkers,
																	 PartTimeWorkersPerDrivingAgeMembers = partTimeWorkersPerDrivingAgeMembers,
																	 RetiredAdults = retiredAdults,
																	 RetiredAdultsPerDrivingAgeMembers = retiredAdultsPerDrivingAgeMembers,
																	 UniversityStudents = universityStudents,
																	 UniversityStudentsPerDrivingAgeMembers = universityStudentsPerDrivingAgeMembers,
																	 WorkersPlusStudents = workersPlusStudents
				                         };
			Assert.Equal(adults, totals.Adults);
			Assert.Equal(allStudents, totals.AllStudents);
			Assert.Equal(allWorkers, totals.AllWorkers);
			Assert.Equal(childrenAge5Through15, totals.ChildrenAge5Through15);
			Assert.Equal(childrenUnder16, totals.ChildrenUnder16);
			Assert.Equal(childrenUnder5, totals.ChildrenUnder5);
			Assert.Equal(childrenAge5Through15, totals.ChildrenUnder5PerDrivingAgeMembers);
			Assert.Equal(drivingAgeMembers, totals.DrivingAgeMembers);
			Assert.Equal(drivingAgeStudents, totals.DrivingAgeStudents);
			Assert.Equal(drivingAgeStudentsPerDrivingAgeMembers, totals.DrivingAgeStudentsPerDrivingAgeMembers);
			Assert.Equal(fullAndPartTimeWorkers, totals.FullAndPartTimeWorkers);
			Assert.Equal(fulltimeWorkers, totals.FulltimeWorkers);
			Assert.Equal(homeBasedPersonsPerDrivingAgeMembers, totals.HomeBasedPersonsPerDrivingAgeMembers);
			Assert.Equal(nonworkingAdults, totals.NonworkingAdults);
			Assert.Equal(partTimeWorkers, totals.PartTimeWorkers);
			Assert.Equal(partTimeWorkersPerDrivingAgeMembers, totals.PartTimeWorkersPerDrivingAgeMembers);
			Assert.Equal(retiredAdults, totals.RetiredAdults);
			Assert.Equal(retiredAdultsPerDrivingAgeMembers, totals.RetiredAdultsPerDrivingAgeMembers);
			Assert.Equal(universityStudents, totals.UniversityStudents);
			Assert.Equal(universityStudentsPerDrivingAgeMembers, totals.UniversityStudentsPerDrivingAgeMembers);
			Assert.Equal(workersPlusStudents, totals.WorkersPlusStudents);
		}

		[Fact]
		public void TestHousehold()
		{
			const int id = 3;
			const double fractionWorkersWithJobsOutsideRegion = .25;
			const int size = 16;
			const int vehiclesAvailable = 13;
			const int workers = 19;
			const int fulltimeWorkers = 17;
			const int partTimeWorkers = 2;
			const int retiredAdults = 3;
			const int otherAdults = 1;
			const int collegeStudents = 4;
			const int highSchoolStudents = 5;
			const int kidsBetween5And15 = 6;
			const int kidsBetween0And4 = 7;
			const int income = 16000;
			const int ownOrRent = 1;
			const int residenceType = 2;
			const int residenceParcelId = 3;
			const int residenceZoneId = 4;
			const int residenceZoneKey = 5;
			const double expansionFactor = .67;
			const int sampleType = 7;

			Household household = new Household
				                      {
					                      CollegeStudents = collegeStudents,
					                      ExpansionFactor = expansionFactor,
					                      FractionWorkersWithJobsOutsideRegion = fractionWorkersWithJobsOutsideRegion,
					                      FulltimeWorkers = fulltimeWorkers,
					                      HighSchoolStudents = highSchoolStudents,
					                      Income = income,
					                      Id = id,
					                      KidsBetween0And4 = kidsBetween0And4,
					                      KidsBetween5And15 = kidsBetween5And15,
																OtherAdults = otherAdults,
																OwnOrRent = ownOrRent,
																PartTimeWorkers = partTimeWorkers,
																ResidenceParcelId = residenceParcelId,
																ResidenceType = residenceType,
																ResidenceZoneId = residenceZoneId,
																ResidenceZoneKey = residenceZoneKey,
																RetiredAdults = retiredAdults,
																SampleType = sampleType,
																Size = size,
																VehiclesAvailable = vehiclesAvailable,
																Workers = workers
				                      };
			Assert.Equal(collegeStudents, household.CollegeStudents);
			Assert.Equal(expansionFactor, household.ExpansionFactor);
			Assert.Equal(fractionWorkersWithJobsOutsideRegion, household.FractionWorkersWithJobsOutsideRegion);
			Assert.Equal(fulltimeWorkers, household.FulltimeWorkers);
			Assert.Equal(highSchoolStudents, household.HighSchoolStudents);
			Assert.Equal(income, household.Income);
			Assert.Equal(id, household.Id);
			Assert.Equal(kidsBetween0And4, household.KidsBetween0And4);
			Assert.Equal(kidsBetween5And15, household.KidsBetween5And15);
			Assert.Equal(otherAdults, household.OtherAdults);
			Assert.Equal(ownOrRent, household.OwnOrRent);
			Assert.Equal(partTimeWorkers, household.PartTimeWorkers);
			Assert.Equal(residenceParcelId, household.ResidenceParcelId);
			Assert.Equal(residenceType, household.ResidenceType );
			Assert.Equal(residenceZoneId, household.ResidenceZoneId);
			Assert.Equal(residenceZoneKey, household.ResidenceZoneKey);
			Assert.Equal(retiredAdults, household.RetiredAdults);
			Assert.Equal(sampleType, household.SampleType);
			Assert.Equal(size, household.Size);
			Assert.Equal(vehiclesAvailable, household.VehiclesAvailable);
			Assert.Equal(workers, household.Workers);
		}

		
		[Fact]
		public void TestHouseholdWrapper()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 256;
			
			const int id = 3;

			List<IPerson> persons = new List<IPerson> {
				new Person {HouseholdId = id, Age = 20, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 35, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 20, PersonType = Constants.PersonType.PART_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 35, PersonType = Constants.PersonType.PART_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER},
				new Person {HouseholdId = id, Age = 10, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15},};

			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			
			Assert.Equal(1, wrapper.HouseholdDays.Count);
			Assert.Equal(0, wrapper.HouseholdDays[0].AttemptedSimulations);
			Assert.Equal(0, wrapper.HouseholdDays[0].FullHalfTours);
			Assert.Equal(id, wrapper.HouseholdDays[0].Household.Id);
			Assert.Equal(false, wrapper.HouseholdDays[0].IsMissingData);
			Assert.Equal(0, wrapper.HouseholdDays[0].JointTours);
			Assert.Equal(0, wrapper.HouseholdDays[0].PartialHalfTours);
			Assert.Equal(7, wrapper.HouseholdDays[0].PersonDays.Count);

		}

		[Fact]
		public void TestHouseholdWrapperCarsPerDriver()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 256;
			//var daysimModule = new DaysimModule();
			
			
			const int id = 3;
			
			List<IPerson> persons = new List<IPerson> {
				new Person {HouseholdId = id, Age = 20, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 35, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 20, PersonType = Constants.PersonType.PART_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 35, PersonType = Constants.PersonType.PART_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER},
				new Person {HouseholdId = id, Age = 10, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15},};

			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:1000);
			wrapper.Init();

			Assert.Equal(1, wrapper.CarsPerDriver);//Even though there are more than 1 car per driver, the max is 1

			persons = new List<IPerson> {
				new Person {HouseholdId = id, Age = 20, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 35, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 20, PersonType = Constants.PersonType.PART_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 35, PersonType = Constants.PersonType.PART_TIME_WORKER}, 
				new Person {HouseholdId = id, Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER},
				new Person {HouseholdId = id, Age = 10, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15},};
			wrapper = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:3);
			wrapper.Init();

			Assert.Equal(0, wrapper.CarsPerDriver);//Even though there are more than 1 car per driver, the max is 1

			wrapper = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:6);
			wrapper.Init();

			Assert.Equal(1, wrapper.CarsPerDriver);//Even though there are more than 1 car per driver, the max is 1

			wrapper = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:5);
			wrapper.Init();

			Assert.Equal(0, wrapper.CarsPerDriver);//Even though there are more than 1 car per driver, the max is 1
		}

		[Fact]
		public void TestHouseholdWrapperHouseholdTypes()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons, size:1);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.INDIVIDUAL_WORKER_STUDENT, wrapper.HouseholdType);
			
			persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.UNIVERSITY_STUDENT, StudentType = 1}};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:1);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.INDIVIDUAL_WORKER_STUDENT, wrapper.HouseholdType);

			persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:1);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.INDIVIDUAL_NONWORKER_NONSTUDENT, wrapper.HouseholdType);

			persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}, new Person{Age = 4, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15}};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.ONE_ADULT_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}, new Person{Age = 4, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15}};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.ONE_ADULT_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				          {
					          new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}, 
										new Person{Age = 38, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}, 
										new Person{Age = 8, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15}
				          };
			wrapper = TestHelper.GetHouseholdWrapper(persons, 3);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_WORKER_STUDENT_ADULTS_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				          {
					          new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}, 
										new Person{Age = 38, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}, 
										new Person{Age = 16, PersonType = Constants.PersonType.DRIVING_AGE_STUDENT}
				          };
			wrapper = TestHelper.GetHouseholdWrapper(persons, 3);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_WORKER_STUDENT_ADULTS_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				          {
					          new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}, 
										new Person{Age = 38, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}, 
										new Person{Age = 16, PersonType = Constants.PersonType.DRIVING_AGE_STUDENT}
				          };
			wrapper = TestHelper.GetHouseholdWrapper(persons, 3);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_ADULTS_ONE_PLUS_WORKERS_STUDENTS_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				          {
					          new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}, 
										new Person{Age = 38, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}
				          };
			wrapper = TestHelper.GetHouseholdWrapper(persons, 2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_WORKER_STUDENT_ADULTS_WITHOUT_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				          {
					          new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}, 
										new Person{Age = 38, PersonType = Constants.PersonType.FULL_TIME_WORKER, StudentType = 0, WorkerType = 1}
				          };
			wrapper = TestHelper.GetHouseholdWrapper(persons, 2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.ONE_PLUS_WORKER_STUDENT_ADULTS_AND_ONE_PLUS_NONWORKER_NONSTUDENT_ADULTS_WITHOUT_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				          {
					          new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}, 
										new Person{Age = 38, PersonType = Constants.PersonType.RETIRED_ADULT, StudentType = 0, WorkerType = 0}
				          };
			wrapper = TestHelper.GetHouseholdWrapper(persons, 2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_NONWORKER_NONSTUDENT_ADULTS_WITHOUT_CHILDREN, wrapper.HouseholdType);
			
		}

		[Fact]
		public void TestHouseholdWrapperExpansionFactor()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons, expansionFactor:.25);
			wrapper.Init();
			Assert.Equal(.25, wrapper.ExpansionFactor);

			Global.Configuration.HouseholdSamplingRateOneInX = 4;
			wrapper = TestHelper.GetHouseholdWrapper(persons, expansionFactor:.25);
			wrapper.Init();
			Assert.Equal(1, wrapper.ExpansionFactor);

			Global.Configuration.HouseholdSamplingRateOneInX = 256;
			wrapper = TestHelper.GetHouseholdWrapper(persons, expansionFactor:.25);
			wrapper.Init();
			Assert.Equal(64, wrapper.ExpansionFactor);
		}

		[Fact]
		public void TestHouseholdWrapperFractionWorkersWithJobsOutsideRegion()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons, fractionWorkersWithJobsOutsideRegion:.24);
			wrapper.Init();
			Assert.Equal(.24, wrapper.FractionWorkersWithJobsOutsideRegion);
		}

		[Fact]
		public void TestHouseholdWrapperIncome()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson> {new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons, income:25000);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(false, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(true, wrapper.Has25To45KIncome);
			Assert.Equal(true, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:24999);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(true, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:14999);
			wrapper.Init();
			Assert.Equal(true, wrapper.Has0To15KIncome);
			Assert.Equal(true, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:45000);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(false, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(true, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:74999);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(false, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(true, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:85000);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(false, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(true, wrapper.Has75KPlusIncome);
			Assert.Equal(true, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:100000);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(false, wrapper.Has0To25KIncome);
			Assert.Equal(true, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(true, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:0);
			wrapper.Init();
			Assert.Equal(true, wrapper.Has0To15KIncome);
			Assert.Equal(true, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(false, wrapper.HasMissingIncome);
			Assert.Equal(true, wrapper.HasValidIncome);

			wrapper = TestHelper.GetHouseholdWrapper(persons, income:-1);
			wrapper.Init();
			Assert.Equal(false, wrapper.Has0To15KIncome);
			Assert.Equal(false, wrapper.Has0To25KIncome);
			Assert.Equal(false, wrapper.Has100KPlusIncome);
			Assert.Equal(false, wrapper.Has25To45KIncome);
			Assert.Equal(false, wrapper.Has25To50KIncome);
			Assert.Equal(false, wrapper.Has50To75KIncome);
			Assert.Equal(false, wrapper.Has75KPlusIncome);
			Assert.Equal(false, wrapper.Has75To100KIncome);
			Assert.Equal(true, wrapper.HasMissingIncome);
			Assert.Equal(false, wrapper.HasValidIncome);
		}

		[Fact]
		public void TestHouseholdWrapperCarsLessFlag()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}
				};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(0));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(0));
			Assert.Equal(true, wrapper.Has1Driver);
			Assert.Equal(false, wrapper.Has2Drivers);
			Assert.Equal(false, wrapper.Has3Drivers);
			Assert.Equal(false, wrapper.Has4OrMoreDrivers);
			Assert.Equal(true, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(false, wrapper.HasMoreDriversThan1);
			Assert.Equal(false, wrapper.HasMoreDriversThan2);
			Assert.Equal(false, wrapper.HasMoreDriversThan3);
			Assert.Equal(false, wrapper.HasMoreDriversThan4);
			Assert.Equal(false, wrapper.HasNoFullOrPartTimeWorker);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(false, wrapper.Has1Driver);
			Assert.Equal(true, wrapper.Has2Drivers);
			Assert.Equal(false, wrapper.Has3Drivers);
			Assert.Equal(false, wrapper.Has4OrMoreDrivers);
			Assert.Equal(true, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.HasMoreDriversThan1);
			Assert.Equal(false, wrapper.HasMoreDriversThan2);
			Assert.Equal(false, wrapper.HasMoreDriversThan3);
			Assert.Equal(false, wrapper.HasMoreDriversThan4);
			Assert.Equal(false, wrapper.HasNoFullOrPartTimeWorker);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 7,  PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15, StudentType = 1}
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(false, wrapper.Has1Driver);
			Assert.Equal(true, wrapper.Has2Drivers);
			Assert.Equal(false, wrapper.Has3Drivers);
			Assert.Equal(false, wrapper.Has4OrMoreDrivers);
			Assert.Equal(true, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.HasMoreDriversThan1);
			Assert.Equal(false, wrapper.HasMoreDriversThan2);
			Assert.Equal(false, wrapper.HasMoreDriversThan3);
			Assert.Equal(false, wrapper.HasMoreDriversThan4);
			Assert.Equal(false, wrapper.HasNoFullOrPartTimeWorker);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(3));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(3));
			Assert.Equal(false, wrapper.Has1Driver);
			Assert.Equal(false, wrapper.Has2Drivers);
			Assert.Equal(true, wrapper.Has3Drivers);
			Assert.Equal(false, wrapper.Has4OrMoreDrivers);
			Assert.Equal(false, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.HasMoreDriversThan1);
			Assert.Equal(true, wrapper.HasMoreDriversThan2);
			Assert.Equal(false, wrapper.HasMoreDriversThan3);
			Assert.Equal(false, wrapper.HasMoreDriversThan4);
			Assert.Equal(false, wrapper.HasNoFullOrPartTimeWorker);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.PART_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, WorkerType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(3));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(3));
			Assert.Equal(false, wrapper.Has1Driver);
			Assert.Equal(false, wrapper.Has2Drivers);
			Assert.Equal(true, wrapper.Has3Drivers);
			Assert.Equal(false, wrapper.Has4OrMoreDrivers);
			Assert.Equal(true, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.HasMoreDriversThan1);
			Assert.Equal(true, wrapper.HasMoreDriversThan2);
			Assert.Equal(false, wrapper.HasMoreDriversThan3);
			Assert.Equal(false, wrapper.HasMoreDriversThan4);
			Assert.Equal(false, wrapper.HasNoFullOrPartTimeWorker);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.PART_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, WorkerType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.PART_TIME_WORKER, WorkerType = 1},
					new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, WorkerType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.PART_TIME_WORKER, WorkerType = 1},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(1, wrapper.GetCarsLessThanDriversFlag(3));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(1, wrapper.GetCarsLessThanWorkersFlag(3));
			Assert.Equal(false, wrapper.Has1Driver);
			Assert.Equal(false, wrapper.Has2Drivers);
			Assert.Equal(false, wrapper.Has3Drivers);
			Assert.Equal(true, wrapper.Has4OrMoreDrivers);
			Assert.Equal(false, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(false, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(false, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.HasMoreDriversThan1);
			Assert.Equal(true, wrapper.HasMoreDriversThan2);
			Assert.Equal(true, wrapper.HasMoreDriversThan3);
			Assert.Equal(true, wrapper.HasMoreDriversThan4);
			Assert.Equal(false, wrapper.HasNoFullOrPartTimeWorker);


			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.RETIRED_ADULT, WorkerType = 0},
					
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanDriversFlag(3));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(1));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(2));
			Assert.Equal(0, wrapper.GetCarsLessThanWorkersFlag(3));
			Assert.Equal(true, wrapper.Has1Driver);
			Assert.Equal(false, wrapper.Has2Drivers);
			Assert.Equal(false, wrapper.Has3Drivers);
			Assert.Equal(false, wrapper.Has4OrMoreDrivers);
			Assert.Equal(true, wrapper.Has2OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has3OrLessFullOrPartTimeWorkers);
			Assert.Equal(true, wrapper.Has4OrLessFullOrPartTimeWorkers);
			Assert.Equal(false, wrapper.HasMoreDriversThan1);
			Assert.Equal(false, wrapper.HasMoreDriversThan2);
			Assert.Equal(false, wrapper.HasMoreDriversThan3);
			Assert.Equal(false, wrapper.HasMoreDriversThan4);
			Assert.Equal(true, wrapper.HasNoFullOrPartTimeWorker);
		}
		[Fact]
		public void TestHouseholdWrapperChildren()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1}
				};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();

			Assert.Equal(false, wrapper.HasChildrenAge5Through15);
			Assert.Equal(false, wrapper.HasChildrenUnder16);
			Assert.Equal(false, wrapper.HasChildrenUnder5);
			Assert.Equal(false, wrapper.HasChildren);


			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 4, PersonType = Constants.PersonType.CHILD_UNDER_5, WorkerType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();

			Assert.Equal(false, wrapper.HasChildrenAge5Through15);
			Assert.Equal(true, wrapper.HasChildrenUnder16);
			Assert.Equal(true, wrapper.HasChildrenUnder5);
			Assert.Equal(true, wrapper.HasChildren);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 7, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15, WorkerType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();

			Assert.Equal(true, wrapper.HasChildrenAge5Through15);
			Assert.Equal(true, wrapper.HasChildrenUnder16);
			Assert.Equal(false, wrapper.HasChildrenUnder5);
			Assert.Equal(true, wrapper.HasChildren);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 7, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15, WorkerType = 0},
					new Person{Age = 4, PersonType = Constants.PersonType.CHILD_UNDER_5, WorkerType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();

			Assert.Equal(true, wrapper.HasChildrenAge5Through15);
			Assert.Equal(true, wrapper.HasChildrenUnder16);
			Assert.Equal(true, wrapper.HasChildrenUnder5);
			Assert.Equal(true, wrapper.HasChildren);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1},
					new Person{Age = 17, PersonType = Constants.PersonType.DRIVING_AGE_STUDENT, WorkerType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons);
			wrapper.Init();

			Assert.Equal(false, wrapper.HasChildrenAge5Through15);
			Assert.Equal(false, wrapper.HasChildrenUnder16);
			Assert.Equal(false, wrapper.HasChildrenUnder5);
			Assert.Equal(true, wrapper.HasChildren);
		}

		[Fact]
		public void TestHouseholdWrapperHouseholdType()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<IPerson> persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.UNIVERSITY_STUDENT, WorkerType = 1, StudentType = 1}
				};
			HouseholdWrapper wrapper = TestHelper.GetHouseholdWrapper(persons, size:1);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.INDIVIDUAL_WORKER_STUDENT, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.NON_WORKING_ADULT, WorkerType = 0, StudentType = 0}
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:1);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.INDIVIDUAL_NONWORKER_NONSTUDENT, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.NON_WORKING_ADULT, WorkerType = 0, StudentType = 0},
					new Person{Age = 6, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15, WorkerType = 0, StudentType = 1},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.ONE_ADULT_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1, StudentType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.PART_TIME_WORKER, WorkerType = 1, StudentType = 0},
					new Person{Age = 6, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15, WorkerType = 0, StudentType = 1},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:3);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_WORKER_STUDENT_ADULTS_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1, StudentType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.NON_WORKING_ADULT, WorkerType = 0, StudentType = 0},
					new Person{Age = 6, PersonType = Constants.PersonType.CHILD_AGE_5_THROUGH_15, WorkerType = 0, StudentType = 1},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:3);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_ADULTS_ONE_PLUS_WORKERS_STUDENTS_WITH_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1, StudentType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1, StudentType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_WORKER_STUDENT_ADULTS_WITHOUT_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 1, StudentType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.NON_WORKING_ADULT, WorkerType = 0, StudentType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.ONE_PLUS_WORKER_STUDENT_ADULTS_AND_ONE_PLUS_NONWORKER_NONSTUDENT_ADULTS_WITHOUT_CHILDREN, wrapper.HouseholdType);

			persons = new List<IPerson>
				{
					new Person{Age = 37, PersonType = Constants.PersonType.NON_WORKING_ADULT, WorkerType = 0, StudentType = 0},
					new Person{Age = 37, PersonType = Constants.PersonType.NON_WORKING_ADULT, WorkerType = 0, StudentType = 0},
				};
			wrapper = TestHelper.GetHouseholdWrapper(persons, size:2);
			wrapper.Init();
			Assert.Equal(Constants.HouseholdType.TWO_PLUS_NONWORKER_NONSTUDENT_ADULTS_WITHOUT_CHILDREN, wrapper.HouseholdType);
		}
	}


}
