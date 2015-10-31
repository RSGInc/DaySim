using System.Collections.Generic;
using System.Linq;
using Daysim.ChoiceModels;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Persisters;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Xunit;

namespace Daysim.Tests 
{
	
	public class HouseholdDayTest 
	{
		[Fact]
		public void TestHouseholdDay()
		{
			const int id = 1;
			const int householdId = 2;
			const int day = 3;
			const int dayOfWeek = 4;
			const int jointTours = 5;
			const int partialHalfTours = 6;
			const int fullHalfTours = 7;
			const double expansionFactor = 7.01;

			HouseholdDay householdDay = new HouseholdDay
				                            {
																			Id = id,
																			HouseholdId = householdId,
																			Day = day,
																			DayOfWeek = dayOfWeek,
																			JointTours = jointTours,
																			PartialHalfTours = partialHalfTours,
																			FullHalfTours = fullHalfTours,
																			ExpansionFactor = expansionFactor,
				                            };

			Assert.Equal(id, householdDay.Id);
			Assert.Equal(householdId, householdDay.HouseholdId);
			Assert.Equal(day, householdDay.Day);
			Assert.Equal(dayOfWeek, householdDay.DayOfWeek);
			Assert.Equal(jointTours, householdDay.JointTours);
			Assert.Equal(partialHalfTours, householdDay.PartialHalfTours);
			Assert.Equal(fullHalfTours, householdDay.FullHalfTours);
			Assert.Equal(expansionFactor, householdDay.ExpansionFactor);
		}

		[Fact]
		public void TestHouseholdDayWrapper()
		{
			List<IPerson> persons = new List<IPerson>{new Person{Id = 58}};
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			ChoiceModelFactory.Parcels = new Dictionary<int, CondensedParcel>();
			const int residenceParcelId = 99;
			ChoiceModelFactory.Parcels.Add(residenceParcelId, new CondensedParcel());

			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons);
			HouseholdDayWrapper wrapper = TestHelper.GetHouseholdDayWrapper(householdWrapper:household);

			Assert.Equal(0, wrapper.AttemptedSimulations);
			wrapper.AttemptedSimulations++;
			Assert.Equal(1, wrapper.AttemptedSimulations);

			Assert.Equal(household, wrapper.Household);

			Assert.Equal(false, wrapper.IsMissingData);
			wrapper.IsMissingData = true;
			Assert.Equal(true, wrapper.IsMissingData);

			Assert.Equal(false, wrapper.IsValid);
			wrapper.IsValid = true;
			Assert.Equal(true, wrapper.IsValid);

			Assert.Equal(1, wrapper.PersonDays.Count);
			Assert.Equal(persons[0].Id, wrapper.PersonDays[0].Person.Id);

		}

		[Fact]
		public void TestHouseholdDayWrapperFullHalfTour()
		{
			List<IPerson> persons = new List<IPerson> {new Person()};
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			ChoiceModelFactory.Parcels = new Dictionary<int, CondensedParcel>();
			const int residenceParcelId = 99;
			ChoiceModelFactory.Parcels.Add(residenceParcelId, new CondensedParcel());
			Global.Configuration.IsInEstimationMode = false;
			Global.Configuration.UseJointTours = true;
			FullHalfTourWrapper.SetPersister( new PersisterWithHDF5<FullHalfTour>(new TestFullHalfTourExporter()));

			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons, id:9);
			
			HouseholdDayWrapper wrapper = TestHelper.GetHouseholdDayWrapper(householdWrapper: household, id:8);

			Assert.Equal(0, wrapper.FullHalfTours);
			Assert.Equal(0, wrapper.FullHalfToursList.Count);
			IEnumerable<PersonDayWrapper> orderedPersonDays = wrapper.PersonDays.OrderBy(p => p.JointHalfTourParticipationPriority).ToList().Cast<PersonDayWrapper>();
			int[] participants = new [] { 0, 0, 0, 0, 0, 0, 0, 0 };
			const int direction = 1;
			IFullHalfTourWrapper tourWrapper = wrapper.CreateFullHalfTour(wrapper, orderedPersonDays, participants, direction);

			Assert.Equal(1, tourWrapper.Direction);
			Assert.Equal(household, tourWrapper.Household);
			Assert.Equal(wrapper, tourWrapper.HouseholdDay);
			Assert.Equal(81, tourWrapper.Id);
			Assert.Equal(false, tourWrapper.Paired);
			tourWrapper.Paired = true;
			Assert.Equal(true, tourWrapper.Paired);
			Assert.Equal(0, tourWrapper.Participants);

		}

		[Fact]
		public void TestHouseholdDayWrapperPartialHalfTour()
		{
			List<IPerson> persons = new List<IPerson> {new Person()};
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			ChoiceModelFactory.Parcels = new Dictionary<int, CondensedParcel>();
			const int residenceParcelId = 99;
			ChoiceModelFactory.Parcels.Add(residenceParcelId, new CondensedParcel());

			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons);
			HouseholdDayWrapper wrapper = TestHelper.GetHouseholdDayWrapper(householdWrapper: household);
		}

		[Fact]
		public void TestHouseholdDayWrapperJointHalfTour()
		{
			List<IPerson> persons = new List<IPerson> {new Person()};
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			ChoiceModelFactory.Parcels = new Dictionary<int, CondensedParcel>();
			const int residenceParcelId = 99;
			ChoiceModelFactory.Parcels.Add(residenceParcelId, new CondensedParcel());

			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons);
			HouseholdDayWrapper wrapper = TestHelper.GetHouseholdDayWrapper(householdWrapper: household);
		}
	}
}
