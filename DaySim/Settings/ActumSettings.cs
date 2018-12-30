// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using DaySim.Framework.Factories;

namespace DaySim.Settings {
  [UsedImplicitly]
  [Factory(Factory.SettingsFactory)]
  public class ActumSettings : DefaultSettings {
    public ActumSettings() {
      Modes = new ActumModes();
      Times = new ActumTimes();
    }

    public override double LengthUnitsPerFoot => 0.3048;

    public override double DistanceUnitsPerMile => 1.60934;

    public override double MonetaryUnitsPerDollar => 5.75;

    public override bool UseJointTours => true;
  }

  public class ActumTimes : DefaultTimes {
    public override int MinimumActivityDuration => 2;
  }

  public class ActumModes : DefaultModes {
    //public override int Hov2 {
    //get { throw new NotImplementedException(); }
    //}

    //public override int Hov3 {
    //get { throw new NotImplementedException(); }
    //}

    public override int TotalModes => 23;

    public override int RosterModes => 15;

    public override int MaxMode => 22;

    public override int Walk => 1;

    public override int Bike => 2;

    public override int Sov => 3;

    public override int HovDriver => 4;

    public override int HovPassenger => 5;

    public override int PaidRideShare => 6;

    public override int Transit => 7;

    public override int WalkRideWalk => 7;

    public override int WalkRideBike => 8;

    public override int WalkRideShare => 9;

    public override int BikeParkRideWalk => 10;

    public override int BikeParkRideBike => 11;

    public override int BikeParkRideShare => 12;

    public override int BikeOnTransit => 13;

    public override int ShareRideWalk => 14;

    public override int ShareRideBike => 15;

    public override int ShareRideShare => 16;

    public override int CarKissRideWalk => 17;

    public override int CarKissRideBike => 18;

    public override int CarKissRideShare => 19;

    public override int CarParkRideWalk => 20;
       
    public override int CarParkRideBike => 21;

    public override int CarParkRideShare => 22;



  }
}
