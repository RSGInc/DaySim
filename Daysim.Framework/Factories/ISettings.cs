// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Settings;

namespace DaySim.Framework.Factories {
	public interface ISettings {
		double LengthUnitsPerFoot { get; }

		double DistanceUnitsPerMile { get; }

		double MonetaryUnitsPerDollar { get; }

		bool UseJointTours { get; }

		int OutOfRegionParcelId { get; }

		double GeneralizedTimeUnavailable { get; }

		int NumberOfRandomSeeds { get; }

		IDestinationScales DestinationScales { get; set;  }

		IPersonTypes PersonTypes { get; set; }

		IPatternTypes PatternTypes { get; set; }

		IPurposes Purposes { get; set; }

		ITourCategories TourCategories { get; set; }
 
		ITourPriorities TourPriorities { get; set; }

		IModes Modes { get; set; }

		IDriverTypes DriverTypes { get; set; }

		IPathTypes PathTypes { get; set; }

		IVotGroups VotGroups { get; set; }

		ITimeDirections TimeDirections { get; set; }

		ITourDirections TourDirections { get; set; }

		IPersonGenders PersonGenders { get; set; }

		ITransitAccesses TransitAccesses { get; set; }

		IVotALSegments VotALSegments { get; set; }

		ICarOwnerships CarOwnerships { get; set; }

		IAddressTypes AddressTypes { get; set; }

		IValueOfTimes ValueOfTimes { get; set; }

		IModels Models { get; set; }

		ITimes Times { get; set; }

		IHouseholdTypes HouseholdTypes { get; set; }

		IMaxInputs MaxInputs { get; set; }
	}
}