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
    }

    public override double LengthUnitsPerFoot => 0.3048;

    public override double DistanceUnitsPerMile => 1.60934;

    public override double MonetaryUnitsPerDollar => 5.75;

  }

  public class ActumModes : DefaultModes {
    //public override int Hov2 {
    //get { throw new NotImplementedException(); }
    //}

    //public override int Hov3 {
    //get { throw new NotImplementedException(); }
    //}

    public override int TotalModes => 13;

    public override int MaxMode => 13;

    public override int Walk => 1;

    public override int Bike => 2;

    public override int Sov => 3;

    public override int HovDriver => 4;

    public override int HovPassenger => 5;

    public override int Transit => 6;

    public override int CarParkRideWalk => 7;

    public override int CarKissRideWalk => 8;

    public override int BikeParkRideWalk => 9;

    public override int BikeParkRideBike => 10;

    public override int BikeOnTransit => 11;

    public override int CarParkRideBike => 12;

    public override int WalkRideBike => 13;


  }
}
