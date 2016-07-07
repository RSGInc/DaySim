using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.ChoiceModels;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Creators;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Factories;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;

namespace Daysim.Tests
{
	public static class TestHelper
	{
		public static PersonWrapper GetPersonWrapper(int age = 35, int personType = Constants.PersonType.FULL_TIME_WORKER, int gender = 1, int workerType = 0, int studentType = 0, List<IPerson> persons = null, HouseholdWrapper household = null, int income=25000, int sequence = 0 )
		{
			if (persons == null)
			{
				persons = new List<IPerson>
					          {
						          new Person()
							          {
								          Age = age,
								          PersonType = personType,
								          Gender = gender,
								          WorkerType = workerType,
								          StudentType = studentType,
													Sequence = sequence,
							          }
					          };
			}
			if (household == null )
				household = GetHouseholdWrapper(persons, income:income);
			household.Init();
			PersonWrapper wrapper = new PersonWrapper(persons[0], household);

			return wrapper;
		}

		public static HouseholdWrapper GetHouseholdWrapper(List<IPerson> persons, int size = 16, int vehiclesAvailable = 12,
		                                                   double expansionFactor = .67,
		                                                   double fractionWorkersWithJobsOutsideRegion = .25,
		                                                   int income = 25000,
		                                                   int id = 3,
		                                                   Household household = null,
																											 CondensedParcel residenceParcel = null)
		{
			ChoiceModelFactory.Parcels = new Dictionary<int, CondensedParcel>();
			int residenceParcelId = 99;
			ChoiceModelFactory.Parcels.Add(residenceParcelId, residenceParcel == null ? new CondensedParcel() : residenceParcel);


			int workers = 6;
			int fulltimeWorkers = 4;
			int partTimeWorkers = 2;
			int retiredAdults = 3;
			int otherAdults = 1;
			int collegeStudents = 0;
			int highSchoolStudents = 1;
			int kidsBetween5And15 = 1;
			int kidsBetween0And4 = 1;
			int ownOrRent = 1;
			int residenceType = 2;
			int residenceZoneId = 4;
			int residenceZoneKey = 5;
			int sampleType = 7;

			if (household == null)
				household = new Household
					            {
						            CollegeStudents = collegeStudents,
						            ExpansionFactor = expansionFactor,
						            FractionWorkersWithJobsOutsideRegion =
							            fractionWorkersWithJobsOutsideRegion,
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

			HouseholdWrapper wrapper = new HouseholdWrapper(household)
				                           {
					                           PersonPersister = new TestPersonPersister(persons),
					                           PersonWrapperCreator = new TestPersonWrapperCreator(),
					                           HouseholdDayPersister = new TestHouseholdDayPersister(),
					                           HouseholdDayWrapperCreator = new TestHouseholdDayWrapperCreator(),
					                           HouseholdPersister = new TestHouseholdPersister(),
				                           };
			return wrapper;
		}

		public static PersonDayWrapper GetPersonDayWrapper(PersonWrapper personWrapper = null, int income = 0)
		{
			List<IPerson> persons = new List<IPerson>{new Person()};
			HouseholdWrapper householdWrapper = TestHelper.GetHouseholdWrapper(persons, income: income);
			HouseholdDayWrapper householdDay = TestHelper.GetHouseholdDayWrapper(householdWrapper: householdWrapper);
			
			if (personWrapper == null)
				personWrapper = GetPersonWrapper(household:householdWrapper);
			return new PersonDayWrapper(new PersonDay(), personWrapper, householdDay);
		}

		public static HouseholdDayWrapper GetHouseholdDayWrapper(HouseholdWrapper householdWrapper = null, Household household = null, int id = 3) 
		{
			List<IPerson> persons = new List<IPerson>{new Person()};
			if ( householdWrapper == null)
				householdWrapper = GetHouseholdWrapper(persons, household:household);
			householdWrapper.Init();
			Global.Configuration.DataType = "Default";
			PersonDayWrapperFactory factory = new PersonDayWrapperFactory();
			factory.Register("Default", new PersonDayWrapperCreator());
			factory.Initialize();
			return new HouseholdDayWrapper(new HouseholdDay(){Id = id}, householdWrapper, factory);
		}

		public static PartialHalfTour GetPartialHalfTour() 
		{
			return new PartialHalfTour();
		}

		public static TourWrapper GetTourWrapper() 
		{
			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.BUSINESS;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			TourWrapper wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);

			return wrapper;
		}

		public static TripWrapper GetTripWrapper(int id = 0)
		{
			TourWrapper tour = GetTourWrapper();
			return new TripWrapper(new Trip(){Id=id}, tour, new TourWrapper.HalfTour(tour));
		}
	}
}

