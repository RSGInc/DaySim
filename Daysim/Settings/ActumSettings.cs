// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using Daysim.Framework.Core;
using Daysim.Framework.Factories;

namespace Daysim.Settings {
    [UsedImplicitly]
    [Factory(Factory.SettingsFactory)]
    public class ActumSettings : DefaultSettings
    {
        public ActumSettings()
        {
            Modes = new ActumModes();
        }

        public override double LengthUnitsPerFoot
        {
            get { return 0.3048; }
        }

        public override double DistanceUnitsPerMile
        {
            get { return 1.60934; }
        }

        public override double MonetaryUnitsPerDollar
        {
            get { return 5.75; }
        }

    }

	public class ActumModes : DefaultModes {
		//public override int Hov2 {
            //get { throw new NotImplementedException(); }
		//}

		//public override int Hov3 {
			//get { throw new NotImplementedException(); }
		//}

		public override int TotalModes {
			get { return 13; }
		}

		public override int MaxMode {
			get {	return 13; }
		}
		
		public override int Walk {
			get { return 1; }
		}

		public override int Bike {
			get { return 2; }
		}

		public override int Sov {
			get { return 3; }
		}

		public override int HovDriver {
			get { return 4; }
		}

		public override int HovPassenger {
			get { return 5; }
		}

		public override int Transit {
			get { return 6; }
		}

		public virtual int CarParkRideWalk {
			get { return 7; }
		}

		public virtual int CarKissRideWalk {
			get { return 8; }
		}

		public override int BikeParkRideWalk {
			get { return 9; }
		}

		public override int BikeParkRideBike {
			get { return 10; }
		}

		public override int BikeOnTransit {
			get { return 11; }
		}

		public virtual int CarParkRideBike {
			get { return 12; }
		}

		public virtual int WalkRideBike {
			get { return 13; }
		}

	
	}
}