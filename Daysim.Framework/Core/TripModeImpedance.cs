// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


namespace Daysim.Framework.Core {
	public sealed class TripModeImpedance : ITripModeImpedance {
		public TripModeImpedance() {
			TravelTime = Constants.DEFAULT_VALUE;
		}

		public int AdjacentMinutesBefore { get; set; }

		public int MaxMinutesBefore { get; set; }

		public int TotalMinutesBefore { get; set; }

		public int AdjacentMinutesAfter { get; set; }

		public int MaxMinutesAfter { get; set; }

		public int TotalMinutesAfter { get; set; }

		public double TravelTime { get; set; }

		public double GeneralizedTime { get; set; }

		public double TravelCost { get; set; }

		public double TravelDistance { get; set; }

        public double WalkAccessEgressTime { get; set; }
        
        public int PathType { get; set; }
	}
}