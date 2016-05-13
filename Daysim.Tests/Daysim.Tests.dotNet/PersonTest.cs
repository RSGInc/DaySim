using System;
using System.Collections.Generic;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Xunit;


namespace Daysim.Tests {
	
	public class PersonTest {
		[Fact]
		public void TestPerson()
		{
			int id = 1;
			int householdId = 2;
			int sequence = 3;
			int personType = 4;
			int age = 5;
			int gender = 6;
			int workerType = 7;
			int usualWorkParcelId = 8;
			int usualWorkZoneKey = 9;
			double autoTimeToUsualWork = 10.1;
			double autoDistanceToUsualWork = 11.1;
			int studentType = 12;
			int usualSchoolParcelId = 13;
			int usualSchoolZoneKey = 14;
			double autoTimeToUsualSchool = 15.1;
			double autoDistanceToUsualSchool = 16.1;
			int usualModeToWork = 17;
			int usualArrivalPeriodToWork = 18;
			int usualDeparturePeriodFromWork = 19;
			int transitPassOwnership = 20;
			int paidParkingAtWorkplace = 21;
			int paperDiary = 22;
			int proxyResponse = 23;
			double expansionFactor = 24.1;

			Person person = new Person
				                {
					                Age = age,
					                AutoDistanceToUsualSchool = autoDistanceToUsualSchool,
					                AutoDistanceToUsualWork = autoDistanceToUsualWork,
					                AutoTimeToUsualSchool = autoTimeToUsualSchool,
					                AutoTimeToUsualWork = autoTimeToUsualWork,
					                ExpansionFactor = expansionFactor,
					                Gender = gender,
					                HouseholdId = householdId,
					                Id = id,
					                PaidParkingAtWorkplace = paidParkingAtWorkplace,
					                PaperDiary = paperDiary,
					                PersonType = personType,
					                ProxyResponse = proxyResponse,
					                Sequence = sequence,
					                StudentType = studentType,
					                TransitPassOwnership = transitPassOwnership,
					                UsualArrivalPeriodToWork = usualArrivalPeriodToWork,
					                UsualDeparturePeriodFromWork = usualDeparturePeriodFromWork,
					                UsualModeToWork = usualModeToWork,
					                UsualSchoolParcelId = usualSchoolParcelId,
					                UsualSchoolZoneKey = usualSchoolZoneKey,
					                UsualWorkParcelId = usualWorkParcelId,
					                UsualWorkZoneKey = usualWorkZoneKey,
					                WorkerType = workerType
				                };
			Assert.Equal(age, person.Age);

			Assert.Equal(autoDistanceToUsualSchool, person.AutoDistanceToUsualSchool);
			Assert.Equal(autoDistanceToUsualWork, person.AutoDistanceToUsualWork);
			Assert.Equal(autoTimeToUsualSchool, person.AutoTimeToUsualSchool);
			Assert.Equal(autoTimeToUsualWork, person.AutoTimeToUsualWork);
			Assert.Equal(expansionFactor, person.ExpansionFactor);
			Assert.Equal(gender, person.Gender);
			Assert.Equal(householdId, person.HouseholdId);
			Assert.Equal(id, person.Id);
			Assert.Equal(paidParkingAtWorkplace, person.PaidParkingAtWorkplace);
			Assert.Equal(paperDiary, person.PaperDiary);
			Assert.Equal(personType, person.PersonType);
			Assert.Equal(proxyResponse, person.ProxyResponse);
			Assert.Equal(sequence, person.Sequence);
			Assert.Equal(studentType, person.StudentType);
			Assert.Equal(transitPassOwnership, person.TransitPassOwnership);
			Assert.Equal(usualArrivalPeriodToWork, person.UsualArrivalPeriodToWork);
			Assert.Equal(usualDeparturePeriodFromWork, person.UsualDeparturePeriodFromWork);
			Assert.Equal(usualModeToWork, person.UsualModeToWork);
			Assert.Equal(usualSchoolParcelId, person.UsualSchoolParcelId);
			Assert.Equal(usualSchoolZoneKey, person.UsualSchoolZoneKey);
			Assert.Equal(usualWorkParcelId, person.UsualWorkParcelId);
			Assert.Equal(usualWorkZoneKey, person.UsualWorkZoneKey);
			Assert.Equal(workerType, person.WorkerType);
		}

		[Fact]
		public void TestPersonWrapperAge()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			PersonWrapper wrapper = TestHelper.GetPersonWrapper(age:37, personType:Constants.PersonType.FULL_TIME_WORKER, gender:1, workerType:1, studentType:0);
			Assert.Equal(37, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(false, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(true, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(true, wrapper.IsOnlyFullOrPartTimeWorker);


			wrapper = TestHelper.GetPersonWrapper(age:6, personType:Constants.PersonType.CHILD_AGE_5_THROUGH_15, gender:2, studentType:1, workerType:0);
			Assert.Equal(6, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(true, wrapper.AgeIsLessThan30);
			Assert.Equal(true, wrapper.AgeIsLessThan35);
			Assert.Equal(true, wrapper.IsChildAge5Through15);
			Assert.Equal(false, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(true, wrapper.IsStudentAge);
			Assert.Equal(true, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(false, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(true, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(false, wrapper.IsWorker);
			Assert.Equal(true, wrapper.IsYouth);
			Assert.Equal(false, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(false, wrapper.IsAdultMale);
			Assert.Equal(true, wrapper.IsFemale);
			Assert.Equal(false, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(true, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsOnlyAdult);
			Assert.Equal(false, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:16, personType:Constants.PersonType.DRIVING_AGE_STUDENT, workerType:2, studentType:1, gender:1);
			Assert.Equal(16, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(true, wrapper.AgeIsLessThan30);
			Assert.Equal(true, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(true, wrapper.IsDrivingAgeStudent);
			Assert.Equal(true, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(false, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(true, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(true, wrapper.IsYouth);
			Assert.Equal(false, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(false, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(true, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsOnlyAdult);
			Assert.Equal(false, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:18, personType:Constants.PersonType.DRIVING_AGE_STUDENT, gender:1, workerType:0, studentType:1);
			Assert.Equal(18, wrapper.Age);
			Assert.Equal(true, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(true, wrapper.AgeIsLessThan30);
			Assert.Equal(true, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(true, wrapper.IsDrivingAgeStudent);
			Assert.Equal(true, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(false, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(true, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(false, wrapper.IsWorker);
			Assert.Equal(true, wrapper.IsYouth);
			Assert.Equal(false, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(false, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(true, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsOnlyAdult);
			Assert.Equal(false, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:18, personType:Constants.PersonType.FULL_TIME_WORKER, workerType:1, studentType:0, gender:2);
			Assert.Equal(18, wrapper.Age);
			Assert.Equal(true, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(true, wrapper.AgeIsLessThan30);
			Assert.Equal(true, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(false, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(true, wrapper.IsAdultFemale);
			Assert.Equal(false, wrapper.IsAdultMale);
			Assert.Equal(true, wrapper.IsFemale);
			Assert.Equal(false, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(true, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:28, personType:Constants.PersonType.FULL_TIME_WORKER, workerType:1, studentType:0, gender:2);
			Assert.Equal(28, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(true, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(true, wrapper.AgeIsLessThan30);
			Assert.Equal(true, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(false, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(true, wrapper.IsAdultFemale);
			Assert.Equal(false, wrapper.IsAdultMale);
			Assert.Equal(true, wrapper.IsFemale);
			Assert.Equal(false, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(true, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:38, personType:Constants.PersonType.FULL_TIME_WORKER, workerType:1, studentType:0, gender:2);
			Assert.Equal(38, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(false, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(true, wrapper.IsAdultFemale);
			Assert.Equal(false, wrapper.IsAdultMale);
			Assert.Equal(true, wrapper.IsFemale);
			Assert.Equal(false, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(true, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:51, personType:Constants.PersonType.PART_TIME_WORKER, workerType:1, studentType:0, gender:1);
			Assert.Equal(51, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(true, wrapper.AgeIsBetween51And65);
			Assert.Equal(true, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(true, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(false, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(true, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(true, wrapper.IsOnlyFullOrPartTimeWorker);


			wrapper = TestHelper.GetPersonWrapper(age:65, personType:Constants.PersonType.PART_TIME_WORKER, workerType:1, studentType:0, gender:1);
			Assert.Equal(65, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(true, wrapper.AgeIsBetween51And65);
			Assert.Equal(true, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(true, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(false, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(true, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(true, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(true, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:66, personType:Constants.PersonType.UNIVERSITY_STUDENT, workerType:0, studentType:1, gender:1);
			Assert.Equal(66, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(true, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(true, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(false, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(true, wrapper.IsStudent);
			Assert.Equal(true, wrapper.IsUniversityStudent);
			Assert.Equal(false, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(true, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(true, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(false, wrapper.IsOnlyFullOrPartTimeWorker);

			wrapper = TestHelper.GetPersonWrapper(age:99, personType:Constants.PersonType.RETIRED_ADULT, workerType:0, studentType:1, gender:1);
			Assert.Equal(99, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(false, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(true, wrapper.IsRetiredAdult);
			Assert.Equal(true, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(false, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(true, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(true, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsOnlyAdult);
			Assert.Equal(false, wrapper.IsOnlyFullOrPartTimeWorker);

			List<IPerson> persons = new List<IPerson>()
				                       {
					                       new Person(){Age = 99, PersonType = Constants.PersonType.FULL_TIME_WORKER, WorkerType = 0, StudentType = 1, Gender = 1},
																 new Person(){Age = 25, PersonType = Constants.PersonType.PART_TIME_WORKER, WorkerType = 1, StudentType = 0, Gender = 2},
				                       };

			wrapper = TestHelper.GetPersonWrapper(age:99, personType:Constants.PersonType.RETIRED_ADULT, workerType:0, studentType:1, gender:1, persons:persons);
			Assert.Equal(99, wrapper.Age);
			Assert.Equal(false, wrapper.AgeIsBetween18And25);
			Assert.Equal(false, wrapper.AgeIsBetween26And35);
			Assert.Equal(false, wrapper.AgeIsBetween51And65);
			Assert.Equal(false, wrapper.AgeIsBetween51And98);
			Assert.Equal(false, wrapper.AgeIsLessThan30);
			Assert.Equal(false, wrapper.AgeIsLessThan35);
			Assert.Equal(false, wrapper.IsChildAge5Through15);
			Assert.Equal(true, wrapper.IsDrivingAge);
			Assert.Equal(false, wrapper.IsDrivingAgeStudent);
			Assert.Equal(false, wrapper.IsStudentAge);
			Assert.Equal(false, wrapper.IsChildUnder16);
			Assert.Equal(false, wrapper.IsChildUnder5);
			Assert.Equal(true, wrapper.IsFullOrPartTimeWorker);
			Assert.Equal(true, wrapper.IsFulltimeWorker);
			Assert.Equal(false, wrapper.IsPartTimeWorker);
			Assert.Equal(false, wrapper.IsRetiredAdult);
			Assert.Equal(true, wrapper.IsStudent);
			Assert.Equal(false, wrapper.IsUniversityStudent);
			Assert.Equal(false, wrapper.IsWorker);
			Assert.Equal(false, wrapper.IsYouth);
			Assert.Equal(true, wrapper.IsAdult);
			Assert.Equal(false, wrapper.IsAdultFemale);
			Assert.Equal(true, wrapper.IsAdultMale);
			Assert.Equal(false, wrapper.IsFemale);
			Assert.Equal(true, wrapper.IsMale);
			Assert.Equal(false, wrapper.IsNonworkingAdult);
			Assert.Equal(false, wrapper.IsNotFullOrPartTimeWorker);
			Assert.Equal(false, wrapper.IsOnlyAdult);
			Assert.Equal(false, wrapper.IsOnlyFullOrPartTimeWorker);
		}

		[Fact]
		public void TestPersonWrapper()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			int autoDistanceToUsualSchool = 1;
			int autoDistanceToUsualWork = 2;
			int autoTimeToUsualSchool = 3;
			int autoTimeToUsualWork = 4;
			int gender = 2;
			int id = 99;
			int paperDiary = 5;
			int payParkingAtWorkplace = 1;
			int personType = Constants.PersonType.CHILD_UNDER_5;
			int proxyResponse = 6;
			int sequence = 7;
			int transitPassOwnershipFlag = 1;
			int usualArrivalPeriodToWork = 2;
			int usualDeparturePeriodFromWork = 3;
			int usualModeToWork = 4;
			CondensedParcel usualSchoolParcel = new CondensedParcel();
			int usualSchoolParcelId = 5;
			int usualSchoolZoneKey = 6;
			CondensedParcel usualWorkParcel = new CondensedParcel();
			int usualWorkParcelId = 7;
			int usualWorkZoneKey = 8;
			bool worksAtHome = false;

			List<IPerson> persons = new List<IPerson>(){new Person()
				                                            {
					                                            AutoDistanceToUsualSchool = autoDistanceToUsualSchool, 
																											AutoDistanceToUsualWork = autoDistanceToUsualWork, 
																											AutoTimeToUsualSchool = autoTimeToUsualSchool, 
																											AutoTimeToUsualWork = autoTimeToUsualWork,
																											Gender = gender,
																											Id = id,
																											PaperDiary = paperDiary,
																											PaidParkingAtWorkplace = payParkingAtWorkplace, 
																											PersonType = personType,
																											ProxyResponse = proxyResponse,
																											Sequence = sequence,
																											TransitPassOwnership =  transitPassOwnershipFlag,
																											UsualArrivalPeriodToWork = usualArrivalPeriodToWork,
																											UsualDeparturePeriodFromWork = usualDeparturePeriodFromWork,
																											UsualModeToWork = usualModeToWork,
																											UsualSchoolParcelId = usualSchoolParcelId,
																											UsualSchoolZoneKey = usualSchoolZoneKey,
																											UsualWorkParcelId = usualWorkParcelId,
																											UsualWorkZoneKey = usualWorkZoneKey,
																										
				                                            }};
			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons);
			PersonWrapper wrapper = new PersonWrapper(persons[0], household){UsualSchoolParcel = usualSchoolParcel, UsualWorkParcel = usualWorkParcel};//TestHelper.GetPersonWrapper(age: 37, personType: Constants.PersonType.FULL_TIME_WORKER,
			                          //                          gender: 1, workerType: 1, studentType: 0, persons:persons);
			Assert.Equal(autoDistanceToUsualSchool, wrapper.AutoDistanceToUsualSchool);
			Assert.Equal(autoDistanceToUsualWork, wrapper.AutoDistanceToUsualWork);
			Assert.Equal(autoTimeToUsualSchool, wrapper.AutoTimeToUsualSchool);
			Assert.Equal(autoTimeToUsualWork, wrapper.AutoTimeToUsualWork);
			Assert.Equal(gender, wrapper.Gender);
			Assert.Equal(household, wrapper.Household);
			Assert.Equal(id, wrapper.Id);
			Assert.Equal(paperDiary, wrapper.PaperDiary);
			Assert.Equal(payParkingAtWorkplace, wrapper.PayToParkAtWorkplaceFlag);
			Assert.Equal(personType, wrapper.PersonType);
			Assert.Equal(proxyResponse, wrapper.ProxyResponse);
			Assert.Equal(sequence, wrapper.Sequence);
			
			Assert.Equal(transitPassOwnershipFlag, wrapper.TransitPassOwnershipFlag);
			Assert.Equal(usualArrivalPeriodToWork, wrapper.UsualArrivalPeriodToWork);
			Assert.Equal(usualDeparturePeriodFromWork, wrapper.UsualDeparturePeriodFromWork);
			Assert.Equal(usualModeToWork, wrapper.UsualModeToWork);
			Assert.Equal(usualSchoolParcel, wrapper.UsualSchoolParcel);
			Assert.Equal(usualSchoolParcelId, wrapper.UsualSchoolParcelId);
			Assert.Equal(usualSchoolZoneKey, wrapper.UsualSchoolZoneKey);
			Assert.Equal(usualWorkParcel, wrapper.UsualWorkParcel);
			Assert.Equal(usualWorkParcelId, wrapper.UsualWorkParcelId);
			Assert.Equal(usualWorkZoneKey, wrapper.UsualWorkZoneKey);
			Assert.Equal(worksAtHome, wrapper.WorksAtHome);
		}

		[Fact]
		public void TestPersonWrapperCarOwnershipSegment()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			List<IPerson> persons = new List<IPerson>(){new Person()
				                                            {
					                                            Age = 15
				                                            }};
			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons);
			household.Init();
			PersonWrapper wrapper = new PersonWrapper(persons[0], household){};
			
			Assert.Equal(Constants.CarOwnership.CHILD, wrapper.CarOwnershipSegment);


			persons = new List<IPerson>(){new Person()
				                                            {
					                                            Age = 25
				                                            }};
			household = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:0);
			household.Init();
			wrapper = new PersonWrapper(persons[0], household){};
			
			Assert.Equal(Constants.CarOwnership.NO_CARS, wrapper.CarOwnershipSegment);

			persons = new List<IPerson>()
				          {
					          new Person() {Age = 25, PersonType = Constants.PersonType.FULL_TIME_WORKER},
										new Person() {Age = 25, PersonType = Constants.PersonType.FULL_TIME_WORKER}
				          };
			household = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:1);
			household.Init();
			wrapper = new PersonWrapper(persons[0], household){};
			
			Assert.Equal(Constants.CarOwnership.LT_ONE_CAR_PER_ADULT, wrapper.CarOwnershipSegment);

			persons = new List<IPerson>()
				          {
					          new Person() {Age = 25, PersonType = Constants.PersonType.FULL_TIME_WORKER},
										new Person() {Age = 25, PersonType = Constants.PersonType.FULL_TIME_WORKER}
				          };
			household = TestHelper.GetHouseholdWrapper(persons, vehiclesAvailable:2);
			household.Init();
			wrapper = new PersonWrapper(persons[0], household){};
			
			Assert.Equal(Constants.CarOwnership.ONE_OR_MORE_CARS_PER_ADULT, wrapper.CarOwnershipSegment);
			
		}

		[Fact]
		public void TestPersonWrapperHouseholdDayPatternParticipationPriority()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			PersonWrapper wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.CHILD_UNDER_5);
			Assert.Equal(1, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.CHILD_AGE_5_THROUGH_15);
			Assert.Equal(2, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.DRIVING_AGE_STUDENT);
			Assert.Equal(7, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.FULL_TIME_WORKER);
			Assert.Equal(5, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.NON_WORKING_ADULT);
			Assert.Equal(3, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.PART_TIME_WORKER);
			Assert.Equal(4, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.RETIRED_ADULT);
			Assert.Equal(6, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.UNIVERSITY_STUDENT);
			Assert.Equal(8, wrapper.HouseholdDayPatternParticipationPriority);

			wrapper = TestHelper.GetPersonWrapper(personType: 99);
			Assert.Equal(9, wrapper.HouseholdDayPatternParticipationPriority);
		}

		[Fact]
		public void TestPersonWrapperTransitFareDiscountFraction()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			Global.Configuration.PathImpedance_TransitUseFareDiscountFractions = false;
			List<IPerson> persons = new List<IPerson>(){new Person()
				                                            {
					                                            Age = 4,
																											TransitPassOwnership = 1,
																											PersonType = Constants.PersonType.CHILD_UNDER_5,
				                                            }};
			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons);
			household.Init();
			PersonWrapper wrapper = new PersonWrapper(persons[0], household){};
			
			Assert.Equal(0, wrapper.TransitFareDiscountFraction);
			Global.Configuration.PathImpedance_TransitUseFareDiscountFractions = true;
			Global.Configuration.IncludeTransitPassOwnershipModel = true;
			Assert.Equal(1, wrapper.TransitFareDiscountFraction);

			Global.Configuration.IncludeTransitPassOwnershipModel = false;
			Global.Configuration.Policy_UniversalTransitFareDiscountFraction = .1;
			Assert.Equal(.1, wrapper.TransitFareDiscountFraction);

			Global.Configuration.PathImpedance_TransitFareDiscountFractionChildUnder5 = .2;
			Global.Configuration.Policy_UniversalTransitFareDiscountFraction = 0;
			Assert.Equal(.2, wrapper.TransitFareDiscountFraction);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.CHILD_AGE_5_THROUGH_15);
			Global.Configuration.PathImpedance_TransitFareDiscountFractionChild5To15 = .3;
			Assert.Equal(.3, wrapper.TransitFareDiscountFraction);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.DRIVING_AGE_STUDENT);
			Global.Configuration.PathImpedance_TransitFareDiscountFractionHighSchoolStudent = .4;
			Assert.Equal(.4, wrapper.TransitFareDiscountFraction);

			wrapper = TestHelper.GetPersonWrapper(personType: Constants.PersonType.UNIVERSITY_STUDENT);
			Global.Configuration.PathImpedance_TransitFareDiscountFractionUniverityStudent = .5;
			Assert.Equal(.5, wrapper.TransitFareDiscountFraction);

			wrapper = TestHelper.GetPersonWrapper(age:65);
			Global.Configuration.PathImpedance_TransitFareDiscountFractionAge65Up = .6;
			Assert.Equal(.6, wrapper.TransitFareDiscountFraction);

			wrapper = TestHelper.GetPersonWrapper(age:64);
			Assert.Equal(0, wrapper.TransitFareDiscountFraction);
		}
	}
}
