using Daysim.DomainModels;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.Framework.Core;
using Xunit;

namespace Daysim.Tests 
{
	
	public class PersonDayTest 
	{
		[Fact]
		public void TestPersonDay()
		{
			int id = 1;
			int personId = 2;
			int householdDayId = 3;
			int householdId = 4;
			int personSequence = 5;
			int day = 6;
			int dayBeginsAtHome = 7;
			int dayEndsAtHome = 8;
			int homeBasedTours = 9;
			int workBasedTours = 10;
			int usualWorkplaceTours = 11;
			int workTours = 12;
			int schoolTours = 13;
			int escortTours = 14;
			int personalBusinessTours = 15;
			int shoppingTours = 16;
			int mealTours = 17;
			int socialTours = 18;
			int recreationTours = 19;
			int medicalTours = 20;
			int workStops = 21;
			int schoolStops = 22;
			int escortStops = 23;
			int personalBusinessStops = 24;
			int shoppingStops = 25;
			int mealStops = 26;
			int socialStops = 27;
			int recreationStops = 28;
			int medicalStops = 29;
			int workAtHomeDuration = 30;
			double expansionFactor = 30.01;

			PersonDay personDay = new PersonDay
				                      {
																Id = id,
			PersonId = personId ,
			HouseholdDayId = householdDayId,
			HouseholdId = householdId,
			PersonSequence = personSequence,
			Day = day,
			DayBeginsAtHome = dayBeginsAtHome,
			DayEndsAtHome = dayEndsAtHome,
			HomeBasedTours = homeBasedTours,
			WorkBasedTours = workBasedTours,
			UsualWorkplaceTours = usualWorkplaceTours,
			WorkTours = workTours,
			SchoolTours = schoolTours,
			EscortTours = escortTours,
			PersonalBusinessTours = personalBusinessTours,
			ShoppingTours = shoppingTours,
			MealTours = mealTours,
			SocialTours = socialTours,
			RecreationTours = recreationTours,
			MedicalTours = medicalTours,
			WorkStops = workStops,
			SchoolStops = schoolStops,
			EscortStops = escortStops,
			PersonalBusinessStops = personalBusinessStops,
			ShoppingStops = shoppingStops,
			MealStops = mealStops,
			SocialStops = socialStops,
			RecreationStops = recreationStops,
			MedicalStops = medicalStops,
			WorkAtHomeDuration = workAtHomeDuration,
			ExpansionFactor = expansionFactor,
				                      };
			
			Assert.Equal(id, personDay.Id);
			Assert.Equal(personId, personDay.PersonId);
			Assert.Equal(householdDayId, personDay.HouseholdDayId);
			Assert.Equal(householdId, personDay.HouseholdId);
			Assert.Equal(personSequence, personDay.PersonSequence);
			Assert.Equal(day, personDay.Day);
			Assert.Equal(dayBeginsAtHome, personDay.DayBeginsAtHome);
			Assert.Equal(dayEndsAtHome, personDay.DayEndsAtHome);
			Assert.Equal(homeBasedTours, personDay.HomeBasedTours);
			Assert.Equal(workBasedTours, personDay.WorkBasedTours);
			Assert.Equal(usualWorkplaceTours, personDay.UsualWorkplaceTours);
			Assert.Equal(workTours, personDay.WorkTours);
			Assert.Equal(schoolTours, personDay.SchoolTours);
			Assert.Equal(escortTours, personDay.EscortTours);
			Assert.Equal(personalBusinessTours, personDay.PersonalBusinessTours);
			Assert.Equal(shoppingTours, personDay.ShoppingTours);
			Assert.Equal(mealTours, personDay.MealTours);
			Assert.Equal(socialTours, personDay.SocialTours);
			Assert.Equal(recreationTours, personDay.RecreationTours);
			Assert.Equal(medicalTours, personDay.MedicalTours);
			Assert.Equal(workStops, personDay.WorkStops);
			Assert.Equal(schoolStops, personDay.SchoolStops);
			Assert.Equal(escortStops, personDay.EscortStops);
			Assert.Equal(personalBusinessStops, personDay.PersonalBusinessStops);
			Assert.Equal(shoppingStops, personDay.ShoppingStops);
			Assert.Equal(mealStops, personDay.MealStops);
			Assert.Equal(socialStops, personDay.SocialStops);
			Assert.Equal(recreationStops, personDay.RecreationStops);
			Assert.Equal(medicalStops, personDay.MedicalStops);
			Assert.Equal(workAtHomeDuration, personDay.WorkAtHomeDuration);
			Assert.Equal(expansionFactor, personDay.ExpansionFactor);
		}

		[Fact]
		public void TestPersonDayWrapper()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			
			PersonDayWrapper wrapper = TestHelper.GetPersonDayWrapper();
		}

	}
}
