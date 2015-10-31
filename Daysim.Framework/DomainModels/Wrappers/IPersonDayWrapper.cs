// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;

namespace Daysim.Framework.DomainModels.Wrappers {
	public interface IPersonDayWrapper : IPersonDay {
		#region relations properties

		IHouseholdWrapper Household { get; set; }

		IPersonWrapper Person { get; set; }

		IHouseholdDayWrapper HouseholdDay { get; set; }

		List<ITourWrapper> Tours { get; set; }

		#endregion

		#region flags/choice model/etc. properties

		ITimeWindow TimeWindow { get; set; }

		int CreatedWorkTours { get; set; }

		int CreatedSchoolTours { get; set; }

		int CreatedEscortTours { get; set; }

		int CreatedPersonalBusinessTours { get; set; }

		int CreatedShoppingTours { get; set; }

		int CreatedMealTours { get; set; }

		int CreatedSocialTours { get; set; }

		int CreatedRecreationTours { get; set; }

		int CreatedMedicalTours { get; set; }

		int CreatedWorkBasedTours { get; set; }

		int SimulatedHomeBasedTours { get; set; }

		int SimulatedWorkTours { get; set; }

		int SimulatedSchoolTours { get; set; }

		int SimulatedEscortTours { get; set; }

		int SimulatedPersonalBusinessTours { get; set; }

		int SimulatedShoppingTours { get; set; }

		int SimulatedMealTours { get; set; }

		int SimulatedSocialTours { get; set; }

		int SimulatedRecreationTours { get; set; }

		int SimulatedMedicalTours { get; set; }

		int SimulatedWorkStops { get; set; }

		int SimulatedSchoolStops { get; set; }

		int SimulatedEscortStops { get; set; }

		int SimulatedPersonalBusinessStops { get; set; }

		int SimulatedShoppingStops { get; set; }

		int SimulatedMealStops { get; set; }

		int SimulatedSocialStops { get; set; }

		int SimulatedRecreationStops { get; set; }

		int SimulatedMedicalStops { get; set; }

		bool IsValid { get; set; }

		int AttemptedSimulations { get; set; }

		int PatternType { get; set; }

		bool HasMandatoryTourToUsualLocation { get; set; }

		int EscortFullHalfTours { get; set; }

		int WorksAtHomeFlag { get; set; }

		int JointTours { get; set; }

		int EscortJointTours { get; set; }

		int PersonalBusinessJointTours { get; set; }

		int ShoppingJointTours { get; set; }

		int MealJointTours { get; set; }

		int SocialJointTours { get; set; }

		int RecreationJointTours { get; set; }

		int MedicalJointTours { get; set; }

		bool IsMissingData { get; set; }

		#endregion

		#region wrapper methods

		int GetTotalTours();

		int GetTotalToursExcludingWorkAndSchool();

		int GetCreatedNonMandatoryTours();

		int GetTotalCreatedTours();

		int GetTotalCreatedTourPurposes();

		int GetTotalSimulatedTours();

		int GetTotalStops();

		int GetTotalStopPurposes();

		int GetTotalStopsExcludingWorkAndSchool();

		int GetTotalSimulatedStops();

		bool GetIsWorkOrSchoolPattern();

		bool GetIsOtherPattern();

		bool HomeBasedToursExist();

		bool TwoOrMoreWorkToursExist();

		bool WorkStopsExist();

		bool SimulatedToursExist();

		bool OnlyHomeBasedToursExist();

		bool IsFirstSimulatedHomeBasedTour();

		bool IsLaterSimulatedHomeBasedTour();

		bool SimulatedWorkStopsExist();

		bool SimulatedSchoolStopsExist();

		bool SimulatedEscortStopsExist();

		bool SimulatedPersonalBusinessStopsExist();

		bool SimulatedShoppingStopsExist();

		bool SimulatedMealStopsExist();

		bool SimulatedSocialStopsExist();

		int GetJointTourParticipationPriority();

		int GetJointHalfTourParticipationPriority();

		void InitializeTours();

		void SetTours();

		void GetMandatoryTourSimulatedData(IPersonDayWrapper personDay, List<ITourWrapper> tours);

		void GetIndividualTourSimulatedData(IPersonDayWrapper personDay, List<ITourWrapper> tours);

		void IncrementSimulatedTours(int destinationPurpose);

		void IncrementSimulatedStops(int destinationPurpose);

		ITourWrapper GetEscortTour(int originAddressType, int originParcelId, int originZoneKey);

		ITourWrapper GetNewTour(int originAddressType, int originParcelId, int originZoneKey, int purpose);

		int GetNextTourSequence();

		int GetCurrentTourSequence();

		void SetHomeBasedNonMandatoryTours();

		#endregion

		#region init/utility/export methods

		void Export();

		void Reset();

		#endregion
	}
}