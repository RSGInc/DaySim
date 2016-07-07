// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.DomainModels.Wrappers {
	public interface ITripWrapper : ITrip {
		#region relations properties

		IHouseholdWrapper Household { get; set; }

		IPersonWrapper Person { get; set; }

		IPersonDayWrapper PersonDay { get; set; }

		ITourWrapper Tour { get; set; }

		IHalfTour HalfTour { get; set; }

		IParcelWrapper OriginParcel { get; set; }

		IParcelWrapper DestinationParcel { get; set; }

		#endregion

		#region flags/choice model/etc. properties

		int EarliestDepartureTime { get; set; }

		int LatestDepartureTime { get; set; }

		int ArrivalTimeLimit { get; set; }

		bool IsHalfTourFromOrigin { get; set; }

		bool IsToTourOrigin { get; set; }

		bool IsMissingData { get; set; }

		#endregion

		#region wrapper methods

		bool IsNoneOrHomePurposeByOrigin();

		bool IsWorkPurposeByOrigin();

		bool IsEscortPurposeByOrigin();

		bool IsNoneOrHomePurposeByDestination();

		bool IsWorkPurposeByDestination();

		bool IsEscortPurposeByDestination();

		bool IsWorkDestinationPurpose();

		bool IsSchoolDestinationPurpose();

		bool IsEscortDestinationPurpose();

		bool IsPersonalBusinessDestinationPurpose();

		bool IsShoppingDestinationPurpose();

		bool IsMealDestinationPurpose();

		bool IsSocialDestinationPurpose();

		bool IsRecreationDestinationPurpose();

		bool IsMedicalDestinationPurpose();

		bool IsPersonalBusinessOrMedicalDestinationPurpose();

		bool IsWorkOrSchoolDestinationPurpose();

		bool IsPersonalReasonsDestinationPurpose();

		bool IsSchoolOriginPurpose();

		bool IsEscortOriginPurpose();

		bool IsShoppingOriginPurpose();

		bool IsPersonalBusinessOriginPurpose();

		bool IsMealOriginPurpose();

		bool IsSocialOriginPurpose();

		bool UsesSovOrHovModes();

		bool IsWalkMode();

		bool IsBikeMode();

		bool IsSovMode();

		bool IsHov2Mode();

		bool IsHov3Mode();

		bool IsTransitMode();

		bool IsBeforeMandatoryDestination();

		ITripWrapper GetPreviousTrip();

		ITripWrapper GetNextTrip();

		int GetStartTime();

		void SetDriverOrPassenger(List<ITripWrapper> trips);

		void UpdateTripValues();

		void HUpdateTripValues();

		void Invert(int sequence);

		ITripModeImpedance[] GetTripModeImpedances();

		void SetActivityEndTime(int activityEndTime);

		void SetOriginAddressType(int originAddressType);

		void SetTourSequence(int tourSequence);

		void SetTripValueOfTime();

		#endregion

		#region init/utility/export methods

		void Export();

		#endregion
	}
}