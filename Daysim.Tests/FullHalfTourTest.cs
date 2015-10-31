using System.Collections.Generic;
using Daysim.ChoiceModels;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Persisters;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Xunit;

namespace Daysim.Tests 
{
	
	public class FullHalfTourTest 
	{
		[Fact]
		public void TestFullHalfTour() 
		{
			const int id = 1;
			const int householdDayId = 2;
			const int householdId = 3;
			const int day = 4;
			const int sequence = 5;
			const int direction = 6;
			const int participants = 7;
			const int personSequence1 = 8;
			const int tourSequence1 = 9;
			const int personSequence2 = 10;
			const int tourSequence2 = 11;
			const int personSequence3 = 12;
			const int tourSequence3 = 13;
			const int personSequence4 = 14;
			const int tourSequence4 = 15;
			const int personSequence5 = 16;
			const int tourSequence5 = 17;
			const int personSequence6 = 18;
			const int tourSequence6 = 19;
			const int personSequence7 = 20;
			const int tourSequence7 = 21;
			const int personSequence8 = 22;
			const int tourSequence8 = 23;

			FullHalfTour tour = new FullHalfTour
				                       {
					                       Day = day,
					                       HouseholdDayId = householdDayId,
					                       HouseholdId = householdId,
					                       Id = id,
					                       Participants = participants,
					                       PersonSequence1 = personSequence1,
					                       TourSequence1 = tourSequence1,
					                       PersonSequence2 = personSequence2,
					                       TourSequence2 = tourSequence2,
					                       PersonSequence3 = personSequence3,
					                       TourSequence3 = tourSequence3,
					                       PersonSequence4 = personSequence4,
					                       TourSequence4 = tourSequence4,
					                       PersonSequence5 = personSequence5,
					                       TourSequence5 = tourSequence5,
					                       PersonSequence6 = personSequence6,
					                       TourSequence6 = tourSequence6,
					                       PersonSequence7 = personSequence7,
					                       TourSequence7 = tourSequence7,
					                       PersonSequence8 = personSequence8,
					                       TourSequence8 = tourSequence8,
																 Sequence = sequence,
																 Direction = direction,
				                       };

			Assert.Equal(day, tour.Day);
			Assert.Equal(direction, tour.Direction);
			Assert.Equal(householdDayId, tour.HouseholdDayId);
			Assert.Equal(householdId, tour.HouseholdId);
			Assert.Equal(id, tour.Id);
			Assert.Equal(participants, tour.Participants);
			Assert.Equal(personSequence1, tour.PersonSequence1);
			Assert.Equal(tourSequence1, tour.TourSequence1);
			Assert.Equal(personSequence2, tour.PersonSequence2);
			Assert.Equal(tourSequence2, tour.TourSequence2);
			Assert.Equal(personSequence3, tour.PersonSequence3);
			Assert.Equal(tourSequence3, tour.TourSequence3);
			Assert.Equal(personSequence4, tour.PersonSequence4);
			Assert.Equal(tourSequence4, tour.TourSequence4);
			Assert.Equal(personSequence5, tour.PersonSequence5);
			Assert.Equal(tourSequence5, tour.TourSequence5);
			Assert.Equal(personSequence6, tour.PersonSequence6);
			Assert.Equal(tourSequence6, tour.TourSequence6);
			Assert.Equal(personSequence7, tour.PersonSequence7);
			Assert.Equal(tourSequence7, tour.TourSequence7);
			Assert.Equal(personSequence8, tour.PersonSequence8);
			Assert.Equal(tourSequence8, tour.TourSequence8);
		}

		[Fact]
		public void TestFullHalfTourWrapper()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 256;


			const int id = 1;
			const int householdDayId = 2;
			const int householdId = 3;
			const int day = 4;
			const int sequence = 5;
			const int direction = 6;
			const int participants = 7;
			const int personSequence1 = 8;
			const int tourSequence1 = 9;
			const int personSequence2 = 10;
			const int tourSequence2 = 11;
			const int personSequence3 = 12;
			const int tourSequence3 = 13;
			const int personSequence4 = 14;
			const int tourSequence4 = 15;
			const int personSequence5 = 16;
			const int tourSequence5 = 17;
			const int personSequence6 = 18;
			const int tourSequence6 = 19;
			const int personSequence7 = 20;
			const int tourSequence7 = 21;
			const int personSequence8 = 22;
			const int tourSequence8 = 23;
			
			FullHalfTour tour = new FullHalfTour
				                       {
					                       Day = day,
					                       HouseholdDayId = householdDayId,
					                       HouseholdId = householdId,
					                       Id = id,
																 Direction = direction,
					                       Participants = participants,
					                       PersonSequence1 = personSequence1,
					                       TourSequence1 = tourSequence1,
					                       PersonSequence2 = personSequence2,
					                       TourSequence2 = tourSequence2,
					                       PersonSequence3 = personSequence3,
					                       TourSequence3 = tourSequence3,
					                       PersonSequence4 = personSequence4,
					                       TourSequence4 = tourSequence4,
					                       PersonSequence5 = personSequence5,
					                       TourSequence5 = tourSequence5,
					                       PersonSequence6 = personSequence6,
					                       TourSequence6 = tourSequence6,
					                       PersonSequence7 = personSequence7,
					                       TourSequence7 = tourSequence7,
					                       PersonSequence8 = personSequence8,
					                       TourSequence8 = tourSequence8,
																 Sequence = sequence
				                       };

			List<IPerson> persons = new List<IPerson>{new Person()};

			HouseholdWrapper householdWrapper = TestHelper.GetHouseholdWrapper(persons);
			householdWrapper.Init();
			//HouseholdDayWrapper householdDayWrapper = new HouseholdDayWrapper(new HouseholdDay(), householdWrapper, new PersonDayWrapperFactory());
			
			HouseholdDayWrapper householdDayWrapper = TestHelper.GetHouseholdDayWrapper(householdWrapper);
			FullHalfTourWrapper.SetPersister(new PersisterWithHDF5<FullHalfTour>(new TestFullHalfTourExporter()));
			FullHalfTourWrapper wrapper = new FullHalfTourWrapper(tour, householdDayWrapper);

			Assert.Equal(direction, wrapper.Direction);
			Assert.Equal(id, wrapper.Id);
			Assert.Equal(participants, wrapper.Participants);
			Assert.Equal(personSequence1, wrapper.PersonSequence1);
			Assert.Equal(tourSequence1, wrapper.TourSequence1);
			Assert.Equal(personSequence2, wrapper.PersonSequence2);
			Assert.Equal(tourSequence2, wrapper.TourSequence2);
			Assert.Equal(personSequence3, wrapper.PersonSequence3);
			Assert.Equal(tourSequence3, wrapper.TourSequence3);
			Assert.Equal(personSequence4, wrapper.PersonSequence4);
			Assert.Equal(tourSequence4, wrapper.TourSequence4);
			Assert.Equal(personSequence5, wrapper.PersonSequence5);
			Assert.Equal(tourSequence5, wrapper.TourSequence5);
			Assert.Equal(personSequence6, wrapper.PersonSequence6);
			Assert.Equal(tourSequence6, wrapper.TourSequence6);
			Assert.Equal(personSequence7, wrapper.PersonSequence7);
			Assert.Equal(tourSequence7, wrapper.TourSequence7);
			Assert.Equal(personSequence8, wrapper.PersonSequence8);
			Assert.Equal(tourSequence8, wrapper.TourSequence8);
			Assert.Equal(sequence, wrapper.Sequence);

			
			Assert.Equal(householdWrapper, wrapper.Household);
			Assert.Equal(householdDayWrapper, wrapper.HouseholdDay);


			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			
		}

		[Fact]
		public void TestFullHalfTourSetParticipantTourSequence()
		{

			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 256;


			const int id = 1;
			const int householdDayId = 2;
			const int householdId = 3;
			const int day = 4;
			const int sequence = 5;
			const int direction = 6;
			const int participants = 7;
			const int personSequence1 = 8;
			const int tourSequence1 = 9;
			const int personSequence2 = 10;
			const int tourSequence2 = 11;
			const int personSequence3 = 12;
			const int tourSequence3 = 13;
			const int personSequence4 = 14;
			const int tourSequence4 = 15;
			const int personSequence5 = 16;
			const int tourSequence5 = 17;
			const int personSequence6 = 18;
			const int tourSequence6 = 19;
			const int personSequence7 = 20;
			const int tourSequence7 = 21;
			const int personSequence8 = 22;
			const int tourSequence8 = 23;
			
			FullHalfTour tour = new FullHalfTour
				                       {
					                       Day = day,
																 Direction = direction,
					                       HouseholdDayId = householdDayId,
					                       HouseholdId = householdId,
					                       Id = id,
					                       Participants = participants,
					                       PersonSequence1 = personSequence1,
					                       TourSequence1 = tourSequence1,
					                       PersonSequence2 = personSequence2,
					                       TourSequence2 = tourSequence2,
					                       PersonSequence3 = personSequence3,
					                       TourSequence3 = tourSequence3,
					                       PersonSequence4 = personSequence4,
					                       TourSequence4 = tourSequence4,
					                       PersonSequence5 = personSequence5,
					                       TourSequence5 = tourSequence5,
					                       PersonSequence6 = personSequence6,
					                       TourSequence6 = tourSequence6,
					                       PersonSequence7 = personSequence7,
					                       TourSequence7 = tourSequence7,
					                       PersonSequence8 = personSequence8,
					                       TourSequence8 = tourSequence8,
																 Sequence = sequence
				                       };

			List<IPerson> persons = new List<IPerson>{new Person()};

			HouseholdWrapper householdWrapper = TestHelper.GetHouseholdWrapper(persons);
			householdWrapper.Init();
			//HouseholdDayWrapper householdDayWrapper = new HouseholdDayWrapper(new HouseholdDay(), householdWrapper, new PersonDayWrapperFactory());
			
			HouseholdDayWrapper householdDayWrapper = TestHelper.GetHouseholdDayWrapper(householdWrapper);
			FullHalfTourWrapper.SetPersister(new PersisterWithHDF5<FullHalfTour>(new TestFullHalfTourExporter()));
			FullHalfTourWrapper wrapper = new FullHalfTourWrapper(tour, householdDayWrapper);
			int[] sequences = new[] {1, 2, 3, 4, 5, 6, 7, 8};
			int[] pSequences = new[]{personSequence1, personSequence2, personSequence3, personSequence4, personSequence5, personSequence6, personSequence7, personSequence8};

			for (int x = 0; x < 8; x++)
			{
				PersonWrapper person = TestHelper.GetPersonWrapper(sequence:pSequences[x]);
				PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
				const int destinationPurpose = Constants.Purpose.BUSINESS;
			
				Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
				Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
				Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
				Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
				Tour ptour = new Tour();
				TourWrapper tourWrapper = new TourWrapper(ptour, personDay, destinationPurpose, false) {Sequence = sequences[x]};
				wrapper.SetParticipantTourSequence(tourWrapper);
				
				Assert.Equal(sequences[x], GetWrapperTourSequence(wrapper, x));
			}
		}


		[Fact]
		public void TestFullHalfTourExport()
		{

			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 256};
			FullHalfTour tour = new FullHalfTour();
			HouseholdDayWrapper householdDayWrapper = TestHelper.GetHouseholdDayWrapper();
			var exporter = new TestFullHalfTourExporter();
			FullHalfTourWrapper.SetPersister(new PersisterWithHDF5<FullHalfTour>(exporter));
			FullHalfTourWrapper wrapper = new FullHalfTourWrapper(tour, householdDayWrapper);
			Assert.Equal(false, exporter.HasWritten);
			Assert.Equal(0, ChoiceModelFactory.FullHalfTourFileRecordsWritten);
			wrapper.Export();
			Assert.Equal(true, exporter.HasWritten);
			Assert.Equal(1, ChoiceModelFactory.FullHalfTourFileRecordsWritten);
		}


		private int GetWrapperTourSequence(FullHalfTourWrapper wrapper, int x) 
		{
			switch (x)
			{
				case 0:
					return wrapper.TourSequence1;
				case 1:
					return wrapper.TourSequence2;
				case 2:
					return wrapper.TourSequence3;
				case 3:
					return wrapper.TourSequence4;
				case 4:
					return wrapper.TourSequence5;
				case 5:
					return wrapper.TourSequence6;
				case 6:
					return wrapper.TourSequence7;
				default:
					return wrapper.TourSequence8;
			}
		}
	}
}
