// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
	public interface IPersonDay : IModel {
		int PersonId { get; set; }

		int HouseholdDayId { get; set; }

		int HouseholdId { get; set; }

		int PersonSequence { get; set; }

		int Day { get; set; }

		int DayBeginsAtHome { get; set; }

		int DayEndsAtHome { get; set; }

		int HomeBasedTours { get; set; }

		int WorkBasedTours { get; set; }

		int UsualWorkplaceTours { get; set; }

		int WorkTours { get; set; }

		int SchoolTours { get; set; }

		int EscortTours { get; set; }

		int PersonalBusinessTours { get; set; }

		int ShoppingTours { get; set; }

		int MealTours { get; set; }

		int SocialTours { get; set; }

		int RecreationTours { get; set; }

		int MedicalTours { get; set; }

		int WorkStops { get; set; }

		int SchoolStops { get; set; }

		int EscortStops { get; set; }

		int PersonalBusinessStops { get; set; }

		int ShoppingStops { get; set; }

		int MealStops { get; set; }

		int SocialStops { get; set; }

		int RecreationStops { get; set; }

		int MedicalStops { get; set; }

		int WorkAtHomeDuration { get; set; }

		double ExpansionFactor { get; set; }
	}
}