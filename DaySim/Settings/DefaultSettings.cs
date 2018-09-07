// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.Framework.Settings;

namespace DaySim.Settings {
  [UsedImplicitly]
  [Factory(Factory.SettingsFactory, ChoiceModelRunner = ChoiceModelRunner.Default)]
  public class DefaultSettings : ISettings {
    public DefaultSettings() {
      DestinationScales = new DefaultDestinationScales();
      PersonTypes = new DefaultPersonTypes();
      PatternTypes = new DefaultPatternTypes();
      Purposes = new DefaultPurposes();
      TourCategories = new DefaultTourCatetories();
      TourPriorities = new DefaultTourPriorities();
      Modes = new DefaultModes();
      DriverTypes = new DefaultDriverTypes();
      PathTypes = new DefaultPathTypes();
      VotGroups = new DefaultVotGroups();
      TimeDirections = new DefaultTimeDirections();
      TourDirections = new DefaultTourDirections();
      PersonGenders = new DefaultPersonGenders();
      TransitAccesses = new DefaultTransitAccesses();
      VotALSegments = new DefaultVotALSegments();
      CarOwnerships = new DefaultCarOwnerships();
      AddressTypes = new DefaultAddressTypes();
      ValueOfTimes = new DefaultValueOfTimes();
      Models = new DefaultModels();
      Times = new DefaultTimes();
      HouseholdTypes = new DefaultHouseholdTypes();
      MaxInputs = new DefaultMaxInputs();
    }

    public virtual double LengthUnitsPerFoot => 1.0;

    public virtual double DistanceUnitsPerMile => 1.0;

    public virtual double MonetaryUnitsPerDollar => 1.0;

    public virtual bool UseJointTours => false;

    public virtual int OutOfRegionParcelId => -999;

    public virtual double GeneralizedTimeUnavailable => -999.0;

    public virtual int NumberOfRandomSeeds => 1200;

    public IDestinationScales DestinationScales { get; set; }

    public IPersonTypes PersonTypes { get; set; }

    public IPatternTypes PatternTypes { get; set; }

    public IPurposes Purposes { get; set; }

    public ITourCategories TourCategories { get; set; }

    public ITourPriorities TourPriorities { get; set; }

    public IModes Modes { get; set; }

    public IDriverTypes DriverTypes { get; set; }

    public IPathTypes PathTypes { get; set; }

    public IVotGroups VotGroups { get; set; }

    public ITimeDirections TimeDirections { get; set; }

    public ITourDirections TourDirections { get; set; }

    public IPersonGenders PersonGenders { get; set; }

    public ITransitAccesses TransitAccesses { get; set; }

    public IVotALSegments VotALSegments { get; set; }

    public ICarOwnerships CarOwnerships { get; set; }

    public IAddressTypes AddressTypes { get; set; }

    public IValueOfTimes ValueOfTimes { get; set; }

    public IModels Models { get; set; }

    public ITimes Times { get; set; }

    public IHouseholdTypes HouseholdTypes { get; set; }

    public IMaxInputs MaxInputs { get; set; }
  }

  public class DefaultDestinationScales : IDestinationScales {
    public virtual int Parcel => 0;

    public virtual int MicroZone => 1;

    public virtual int Zone => 2;
  }

  public class DefaultPersonTypes : IPersonTypes {
    public virtual int FullTimeWorker => 1;

    public virtual int PartTimeWorker => 2;

    public virtual int RetiredAdult => 3;

    public virtual int NonWorkingAdult => 4;

    public virtual int UniversityStudent => 5;

    public virtual int DrivingAgeStudent => 6;

    public virtual int ChildAge5Through15 => 7;

    public virtual int ChildUnder5 => 8;
  }

  public class DefaultPatternTypes : IPatternTypes {
    public virtual int Mandatory => 1;

    public virtual int Optional => 2;

    public virtual int Home => 3;
  }

  public class DefaultPurposes : IPurposes {
    public virtual int TotalPurposes => 12;

    public virtual int NoneOrHome => 0;

    public virtual int Work => 1;

    public virtual int HomeBasedComposite => 1;

    public virtual int School => 2;

    public virtual int WorkBased => 2;

    public virtual int Escort => 3;

    public virtual int PersonalBusiness => 4;

    public virtual int Shopping => 5;

    public virtual int Meal => 6;

    public virtual int Social => 7;

    public virtual int Recreation => 8;

    public virtual int Medical => 9;

    public virtual int ChangeMode => 10;

    public virtual int Business => 11;
  }

  public class DefaultTourCatetories : ITourCategories {
    public virtual int Primary => 0;

    public virtual int Secondary => 1;

    public virtual int WorkBased => 2;

    public virtual int HomeBased => 3;
  }

  public class DefaultTourPriorities : ITourPriorities {
    public virtual int UsualLocation => 0;

    public virtual int HomeBasedTour => 1;

    public virtual int WorkBasedTour => 2;
  }

  public class DefaultModes : IModes {
    public virtual int TotalModes => 10;

    public virtual int RosterModes => 12;

    public virtual int MaxMode => 9;

    public virtual int None => 0;

    public virtual int Walk => 1;

    public virtual int Bike => 2;

    public virtual int Sov => 3;

    public virtual int Hov2 => 4;

    public virtual int Hov3 => 5;

    public virtual int Transit => 6;

    public virtual int ParkAndRide => 7;

    public virtual int SchoolBus => 8;

    public virtual int PaidRideShare => 9;

    public virtual int AV => 9;
    public virtual int AV1 => 9;
    public virtual int AV2 => 10;
    public virtual int AV3 => 11;

    public virtual int Other => 9;

    public virtual int HovDriver => throw new NotImplementedException();

    public virtual int HovPassenger => throw new NotImplementedException();

    public virtual int KissAndRide => throw new NotImplementedException();

    public virtual int BikeParkRideWalk => throw new NotImplementedException();

    public virtual int BikeParkRideBike => throw new NotImplementedException();

    public virtual int BikeOnTransit => throw new NotImplementedException();

    public virtual int CarParkRideWalk => throw new NotImplementedException();

    public virtual int CarKissRideWalk => throw new NotImplementedException();

    public virtual int CarParkRideBike => throw new NotImplementedException();

    public virtual int WalkRideBike => throw new NotImplementedException();



  }

  public class DefaultDriverTypes : IDriverTypes {
    public virtual int NotApplicable => 0;

    public virtual int Driver => 1;

    public virtual int Passenger => 2;
    public virtual int AV_MainPassenger => 3;
    public virtual int AV_OtherPassenger => 4;
  }

  public class DefaultPathTypes : IPathTypes {
    public virtual int TotalPathTypes => 18;

    public virtual int None => 0;

    public virtual int FullNetwork => 1;

    public virtual int NoTolls => 2;

    public virtual int LocalBus => 3;

    public virtual int LightRail => 4;

    public virtual int PremiumBus => 5;

    public virtual int CommuterRail => 6;

    public virtual int Ferry => 7;

    public virtual int TransitType1 => 3;

    public virtual int TransitType2 => 4;

    public virtual int TransitType3 => 5;

    public virtual int TransitType4 => 6;

    public virtual int TransitType5 => 7;

    public virtual int LocalBus_Knr => 8;

    public virtual int LightRail_Knr => 9;

    public virtual int PremiumBus_Knr => 10;

    public virtual int CommuterRail_Knr => 11;

    public virtual int Ferry_Knr => 12;

    public virtual int TransitType1_Knr => 8;

    public virtual int TransitType2_Knr => 9;

    public virtual int TransitType3_Knr => 10;

    public virtual int TransitType4_Knr => 11;

    public virtual int TransitType5_Knr => 12;

    public virtual int LocalBus_TNC => 13;

    public virtual int LightRail_TNC => 14;

    public virtual int PremiumBus_TNC => 15;

    public virtual int CommuterRail_TNC => 16;

    public virtual int Ferry_TNC => 17;

    public virtual int TransitType1_TNC => 13;

    public virtual int TransitType2_TNC => 14;

    public virtual int TransitType3_TNC => 15;

    public virtual int TransitType4_TNC => 16;

    public virtual int TransitType5_TNC => 17;

  }

  public class DefaultVotGroups : IVotGroups {
    public virtual int TotalVotGroups => 6;

    public virtual int None => 0;

    public virtual int VeryLow => 1;

    public virtual int Low => 2;

    public virtual int Medium => 3;

    public virtual int High => 4;

    public virtual int VeryHigh => 5;

    public virtual int Default => -1;
  }

  public class DefaultTimeDirections : ITimeDirections {
    public virtual int Before => 1;

    public virtual int After => 2;

    public virtual int Both => 3;
  }

  public class DefaultTourDirections : ITourDirections {
    public virtual int TotalTourDirections => 2;

    public virtual int OriginToDestination => 1;

    public virtual int DestinationToOrigin => 2;
  }

  public class DefaultPersonGenders : IPersonGenders {
    public virtual int Male => 1;

    public virtual int Female => 2;
  }

  public class DefaultTransitAccesses : ITransitAccesses {
    public virtual int TotalTransitAccesses => 3;

    public virtual int Gt0AndLteQtrMi => 0;

    public virtual int GtQtrMiAndLteHMi => 1;

    public virtual int None => 2;
  }

  public class DefaultVotALSegments : IVotALSegments {
    public virtual int TotalVotALSegments => 3;

    public virtual int Low => 0;

    public virtual int Medium => 1;

    public virtual int High => 2;

    public virtual int IncomeLowMedium => 20000;

    public virtual int IncomeMediumHigh => 80000;

    public virtual double VotLowMedium => 4.0;

    public virtual double VotMediumHigh => 12.0;

    public virtual double TimeCoefficient => -0.02;

    public virtual double CostCoefficientLow => -0.60;

    public virtual double CostCoefficientMedium => -0.15;

    public virtual double CostCoefficientHigh => -0.06;
  }

  public class DefaultCarOwnerships : ICarOwnerships {
    public virtual int TotalCarOwnerships => 4;

    public virtual int Child => 0;

    public virtual int NoCars => 1;

    public virtual int LtOneCarPerAdult => 2;

    public virtual int OneOrMoreCarsPerAdult => 3;
  }

  public class DefaultAddressTypes : IAddressTypes {
    public virtual int None => 0;

    public virtual int Home => 1;

    public virtual int UsualWorkplace => 2;

    public virtual int UsualSchool => 3;

    public virtual int Other => 4;

    public virtual int Missing => 5;

    public virtual int ChangeMode => 6;
  }

  public class DefaultValueOfTimes : IValueOfTimes {
    public virtual int Low => 1;

    public virtual int Medium => 2;

    public virtual int High => 3;

    public virtual double DefaultVot => 10;
  }

  public class DefaultModels : IModels {
    public virtual int WorkTourModeModel => 0;

    public virtual int SchoolTourModeModel => 1;

    public virtual int WorkbasedSubtourModeModel => 2;

    public virtual int EscortTourModeModel => 3;

    public virtual int OtherHomeBasedTourMode => 4;
  }

  public class DefaultTimes : ITimes {
    public virtual int MinutesInADay => 1440;

    public virtual int MinimumActivityDuration => 1;

    public virtual int ZeroHours => 0;

    public virtual int OneHour => 60 * 1;

    public virtual int TwoHours => 60 * 2;

    public virtual int ThreeHours => 60 * 3;

    public virtual int FourHours => 60 * 4;

    public virtual int FiveHours => 60 * 5;

    public virtual int SixHours => 60 * 6;

    public virtual int SevenHours => 60 * 7;

    public virtual int EightHours => 60 * 8;

    public virtual int NineHours => 60 * 9;

    public virtual int TenHours => 60 * 10;

    public virtual int ElevenHours => 60 * 11;

    public virtual int TwelveHours => 60 * 12;

    public virtual int ThirteenHours => 60 * 13;

    public virtual int FourteenHours => 60 * 14;

    public virtual int FifteenHours => 60 * 15;

    public virtual int SixteenHours => 60 * 16;

    public virtual int SeventeenHours => 60 * 17;

    public virtual int EighteenHours => 60 * 18;

    public virtual int NineteenHours => 60 * 19;

    public virtual int TwentyHours => 60 * 20;

    public virtual int TwentyOneHours => 60 * 21;

    public virtual int TwentyTwoHours => 60 * 22;

    public virtual int TwentyThreeHours => 60 * 23;

    public virtual int TwentyFourHours => 60 * 24;

    public virtual int ThreeAM => 1;

    public virtual int FourAM => 60 * 1 + 1;

    public virtual int FiveAM => 60 * 2 + 1;

    public virtual int SixAM => 60 * 3 + 1;

    public virtual int SevenAM => 60 * 4 + 1;

    public virtual int EightAM => 60 * 5 + 1;

    public virtual int NineAM => 60 * 6 + 1;

    public virtual int TenAM => 60 * 7 + 1;

    public virtual int ElevenAM => 60 * 8 + 1;

    public virtual int Noon => 60 * 9 + 1;

    public virtual int OnePM => 60 * 10 + 1;

    public virtual int TwoPM => 60 * 11 + 1;

    public virtual int ThreePM => 60 * 12 + 1;

    public virtual int FourPM => 60 * 13 + 1;

    public virtual int FivePM => 60 * 14 + 1;

    public virtual int SixPM => 60 * 15 + 1;

    public virtual int SevenPM => 60 * 16 + 1;

    public virtual int EightPM => 60 * 17 + 1;

    public virtual int NinePM => 60 * 18 + 1;

    public virtual int TenPM => 60 * 19 + 1;

    public virtual int ElevenPM => 60 * 20 + 1;

    public virtual int Midnight => 60 * 21 + 1;

    public virtual int OneAM => 60 * 22 + 1;

    public virtual int TwoAM => 60 * 23 + 1;

    public virtual int EndOfRelevantWindow => 60 * 21 + 1;
  }

  public class DefaultHouseholdTypes : IHouseholdTypes {
    public virtual int IndividualWorkerStudent => 1;

    public virtual int IndividualNonworkerNonstudent => 2;

    public virtual int OneAdultWithChildren => 3;

    public virtual int TwoPlusWorkerStudentAdultsWithChildren => 4;

    public virtual int TwoPlusAdultsOnePlusWorkersStudentsWithChildren => 5;

    public virtual int TwoPlusWorkerStudentAdultsWithoutChildren => 6;

    public virtual int OnePlusWorkerStudentAdultsAndOnePlusNonworkerNonstudentAdultsWithoutChildren => 7;

    public virtual int TwoPlusNonworkerNonstudentAdultsWithoutChildren => 8;
  }

  public class DefaultMaxInputs : IMaxInputs {
    public virtual int MaxParcelVals => 200000;

    public virtual int MaxHhSize => 20;

    public virtual int MaxAge => 200;

    public virtual int MaxGend => 3;

    public virtual int MaxPwtyp => 3;

    public virtual int MaxPstyp => 3;

    public virtual int MaxPnrCap => 50000;

    public virtual int MaxPnrCost => 10000;
  }
}