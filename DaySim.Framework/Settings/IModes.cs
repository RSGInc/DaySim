// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Settings {
	public interface IModes {
		int TotalModes { get; }

		int MaxMode { get; }
		
		int None { get; }
		
		int Walk { get; }
		
		int Bike { get; }
		
		int Sov { get; }
		
		int Hov2 { get; }
		
		int Hov3 { get; }
		
		int Transit { get; }
		
		int ParkAndRide { get; }
		
		int SchoolBus { get; }

        int PaidRideShare { get; }
		
		int Other { get; }
		
		int HovDriver { get; }
		
		int HovPassenger { get; }

		int KissAndRide { get; }

		int CarParkRideWalk { get; }

		int CarKissRideWalk { get; }

		int CarParkRideBike { get; }

		int WalkRideBike { get; }

		int BikeParkRideWalk { get; }
	
		int BikeParkRideBike { get; }
	
		int BikeOnTransit { get; }
	
	
	}
}