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
	public interface ITourWrapper : ITour {
		#region relations properties

		IHouseholdWrapper Household { get; set; }

		IPersonWrapper Person { get; set; }

		IPersonDayWrapper PersonDay { get; set; }

		ITourWrapper ParentTour { get; set; }

		List<ITourWrapper> Subtours { get; set; }

		IHalfTour HalfTourFromOrigin { get; set; }

		IHalfTour HalfTourFromDestination { get; set; }

		IParcelWrapper OriginParcel { get; set; }

		IParcelWrapper DestinationParcel { get; set; }

		#endregion

		#region flags/choice model/etc. properties

		bool IsHomeBasedTour { get; set; }

		ITimeWindow TimeWindow { get; set; }

		double IndicatedTravelTimeToDestination { get; set; }

		double IndicatedTravelTimeFromDestination { get; set; }

		int EarliestOriginDepartureTime { get; set; }

		int LatestOriginArrivalTime { get; set; }

		IMinuteSpan DestinationDepartureBigPeriod { get; set; }

		IMinuteSpan DestinationArrivalBigPeriod { get; set; }

		double TimeCoefficient { get; set; }

		double CostCoefficient { get; set; }

		int ParkAndRideNodeId { get; set; }

		int ParkAndRideOriginStopAreaKey { get; set; }

		int ParkAndRideDestinationStopAreaKey { get; set; }

		bool DestinationModeAndTimeHaveBeenSimulated { get; set; }

		bool HalfTour1HasBeenSimulated { get; set; }

		bool HalfTour2HasBeenSimulated { get; set; }

		bool IsMissingData { get; set; }

		#endregion

		#region wrapper methods

		bool IsWorkPurpose();

		bool IsSchoolPurpose();

		bool IsEscortPurpose();

		bool IsPersonalBusinessPurpose();

		bool IsShoppingPurpose();

		bool IsMealPurpose();

		bool IsSocialPurpose();

		bool IsRecreationPurpose();

		bool IsMedicalPurpose();

		bool IsPersonalBusinessOrMedicalPurpose();

		bool IsSocialOrRecreationPurpose();

		bool IsWalkMode();

		bool IsBikeMode();

		bool IsSovMode();

		bool IsHov2Mode();

		bool IsHov3Mode();

		bool IsTransitMode();

		bool IsParkAndRideMode();

		bool IsSchoolBusMode();

		bool IsWalkOrBikeMode();

		bool SubtoursExist();

		bool IsAnHovMode();

		bool IsAnAutoMode();

		bool UsesTransitModes();

		int GetTotalToursByPurpose();

		int GetTotalSimulatedToursByPurpose();

		int GetTourPurposeSegment();

		int GetTourCategory();

		void SetHomeBasedIsSimulated();

		void SetWorkBasedIsSimulated();

		void SetHalfTours(int direction);

		ITimeWindow GetRelevantTimeWindow(IHouseholdDayWrapper householdDay);

		void SetOriginTimes(int direction = 0);

		void UpdateTourValues();

		IHalfTour GetHalfTour(int direction);

		ITourModeImpedance[] GetTourModeImpedances();

		void SetParentTourSequence(int parentTourSequence);

		ITourWrapper CreateSubtour(int originAddressType, int originParcelId, int originZoneKey, int destinationPurpose);

		void SetParkAndRideStay();

		int GetVotALSegment();

		#endregion

		#region init/utility/export methods

		void Export();

		#endregion
	}
}