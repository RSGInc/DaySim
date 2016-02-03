// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using Daysim.Framework.Core;
using Daysim.Framework.Factories;
using Daysim.Framework.Settings;

namespace Daysim.Settings {
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

		public virtual double LengthUnitsPerFoot {
			get { return 1.0; }
		}

		public virtual double DistanceUnitsPerMile {
			get { return 1.0; }
		}

		public virtual double MonetaryUnitsPerDollar {
			get { return 1.0; }
		}

		public virtual bool UseJointTours {
//			get { return true; }
			get { return false; }
		}

		public virtual int OutOfRegionParcelId {
			get { return -999; }
		}

		public virtual double GeneralizedTimeUnavailable {
			get { return -999.0; }
		}

		public virtual int NumberOfRandomSeeds {
			get { return 1200; }
		}

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
		public virtual int Parcel {
			get { return 0; }
		}

		public virtual int MicroZone {
			get { return 1; }
		}

		public virtual int Zone {
			get { return 2; }
		}
	}

	public class DefaultPersonTypes : IPersonTypes {
		public virtual int FullTimeWorker {
			get { return 1; }
		}

		public virtual int PartTimeWorker {
			get { return 2; }
		}

		public virtual int RetiredAdult {
			get { return 3; }
		}

		public virtual int NonWorkingAdult {
			get { return 4; }
		}

		public virtual int UniversityStudent {
			get { return 5; }
		}

		public virtual int DrivingAgeStudent {
			get { return 6; }
		}

		public virtual int ChildAge5Through15 {
			get { return 7; }
		}

		public virtual int ChildUnder5 {
			get { return 8; }
		}
	}

	public class DefaultPatternTypes : IPatternTypes {
		public virtual int Mandatory {
			get { return 1; }
		}

		public virtual int Optional {
			get { return 2; }
		}

		public virtual int Home {
			get { return 3; }
		}
	}

	public class DefaultPurposes : IPurposes {
		public virtual int TotalPurposes {
			get { return 12; }
		}

		public virtual int NoneOrHome {
			get { return 0; }
		}

		public virtual int Work {
			get { return 1; }
		}

		public virtual int HomeBasedComposite {
			get { return 1; }
		}

		public virtual int School {
			get { return 2; }
		}

		public virtual int WorkBased {
			get { return 2; }
		}

		public virtual int Escort {
			get { return 3; }
		}

		public virtual int PersonalBusiness {
			get { return 4; }
		}

		public virtual int Shopping {
			get { return 5; }
		}

		public virtual int Meal {
			get { return 6; }
		}

		public virtual int Social {
			get { return 7; }
		}

		public virtual int Recreation {
			get { return 8; }
		}

		public virtual int Medical {
			get { return 9; }
		}

		public virtual int ChangeMode {
			get { return 10; }
		}

		public virtual int Business {
			get { return 11; }
		}
	}

	public class DefaultTourCatetories : ITourCategories {
		public virtual int Primary {
			get { return 0; }
		}

		public virtual int Secondary {
			get { return 1; }
		}

		public virtual int WorkBased {
			get { return 2; }
		}

		public virtual int HomeBased {
			get { return 3; }
		}
	}

	public class DefaultTourPriorities : ITourPriorities {
		public virtual int UsualLocation {
			get { return 0; }
		}

		public virtual int HomeBasedTour {
			get { return 1; }
		}

		public virtual int WorkBasedTour {
			get { return 2; }
		}
	}

	public class DefaultModes : IModes {
		public virtual int TotalModes {
			get { return 10; }
		}

		public virtual int MaxMode {
			get { return 8; }
		}

		public virtual int None {
			get { return 0; }
		}

		public virtual int Walk {
			get { return 1; }
		}

		public virtual int Bike {
			get { return 2; }
		}

		public virtual int Sov {
			get { return 3; }
		}

		public virtual int Hov2 {
			get { return 4; }
		}

		public virtual int Hov3 {
			get { return 5; }
		}

		public virtual int Transit {
			get { return 6; }
		}

		public virtual int ParkAndRide {
			get { return 7; }
		}

		public virtual int SchoolBus {
			get { return 8; }
		}

		public virtual int Other {
			get { return 9; }
		}

		public virtual int HovDriver {
			get { throw new NotImplementedException(); }
		}

		public virtual int HovPassenger {
			get { throw new NotImplementedException(); }
		}

		public virtual int KissAndRide {
			get { throw new NotImplementedException(); }
		}

		public virtual int BikeParkRideWalk {
			get { throw new NotImplementedException(); }
		}

		public virtual int BikeParkRideBike {
			get { throw new NotImplementedException(); }
		}

		public virtual int BikeOnTransit {
			get { throw new NotImplementedException(); }
		}

		public virtual int CarParkRideWalk {
			get { throw new NotImplementedException(); }
		}

		public virtual int CarKissRideWalk {
			get { throw new NotImplementedException(); }
		}

		public virtual int CarParkRideBike {
			get { throw new NotImplementedException(); }
		}

		public virtual int WalkRideBike {
			get { throw new NotImplementedException(); }
		}



	}

	public class DefaultDriverTypes : IDriverTypes {
		public virtual int NotApplicable {
			get { return 0; }
		}

		public virtual int Driver {
			get { return 1; }
		}

		public virtual int Passenger {
			get { return 2; }
		}
	}

	public class DefaultPathTypes : IPathTypes {
		public virtual int TotalPathTypes {
			get { return 8; }
		}

		public virtual int None {
			get { return 0; }
		}

		public virtual int FullNetwork {
			get { return 1; }
		}

		public virtual int NoTolls {
			get { return 2; }
		}

		public virtual int LocalBus {
			get { return 3; }
		}

		public virtual int LightRail {
			get { return 4; }
		}

		public virtual int PremiumBus {
			get { return 5; }
		}

		public virtual int CommuterRail {
			get { return 6; }
		}

		public virtual int Ferry {
			get { return 7; }
		}

		public virtual int NewMode {
			get { return 4; }
		}

		public virtual int Brt {
			get { return 5; }
		}

		public virtual int FixedGuideway {
			get { return 6; }
		}

		public virtual int BrtC2 {
			get { return 7; }
		}

		public virtual int FixedGuidewayC2 {
			get { return 8; }
		}

		public virtual int LocalBusPnr {
			get { return 9; }
		}

		public virtual int NewModePnr {
			get { return 10; }
		}

		public virtual int BrtPnr {
			get { return 11; }
		}

		public virtual int FixedGuidewayPnr {
			get { return 12; }
		}

		public virtual int BrtC2Pnr {
			get { return 13; }
		}

		public virtual int FixedGuidewayC2Pnr {
			get { return 14; }
		}

		public virtual int LocalBusKnr {
			get { return 15; }
		}

		public virtual int NewModeKnr {
			get { return 16; }
		}

		public virtual int BrtKnr {
			get { return 17; }
		}

		public virtual int FixedGuidewayKnr {
			get { return 18; }
		}

		public virtual int BrtC2Knr {
			get { return 19; }
		}

		public virtual int FixedGuidewayC2Knr {
			get { return 20; }
		}
	}

	public class DefaultVotGroups : IVotGroups {
		public virtual int TotalVotGroups {
			get { return 6; }
		}

		public virtual int None {
			get { return 0; }
		}

		public virtual int VeryLow {
			get { return 1; }
		}

		public virtual int Low {
			get { return 2; }
		}

		public virtual int Medium {
			get { return 3; }
		}

		public virtual int High {
			get { return 4; }
		}

		public virtual int VeryHigh {
			get { return 5; }
		}

		public virtual int Default {
			get { return -1; }
		}
	}

	public class DefaultTimeDirections : ITimeDirections {
		public virtual int Before {
			get { return 1; }
		}

		public virtual int After {
			get { return 2; }
		}

		public virtual int Both {
			get { return 3; }
		}
	}

	public class DefaultTourDirections : ITourDirections {
		public virtual int TotalTourDirections {
			get { return 2; }
		}

		public virtual int OriginToDestination {
			get { return 1; }
		}

		public virtual int DestinationToOrigin {
			get { return 2; }
		}
	}

	public class DefaultPersonGenders : IPersonGenders {
		public virtual int Male {
			get { return 1; }
		}

		public virtual int Female {
			get { return 2; }
		}
	}

	public class DefaultTransitAccesses : ITransitAccesses {
		public virtual int TotalTransitAccesses {
			get { return 3; }
		}

		public virtual int Gt0AndLteQtrMi {
			get { return 0; }
		}

		public virtual int GtQtrMiAndLteHMi {
			get { return 1; }
		}

		public virtual int None {
			get { return 2; }
		}
	}

	public class DefaultVotALSegments : IVotALSegments {
		public virtual int TotalVotALSegments {
			get { return 3; }
		}

		public virtual int Low {
			get { return 0; }
		}

		public virtual int Medium {
			get { return 1; }
		}

		public virtual int High {
			get { return 2; }
		}

		public virtual int IncomeLowMedium {
			get { return 20000; }
		}

		public virtual int IncomeMediumHigh {
			get { return 80000; }
		}

		public virtual double VotLowMedium {
			get { return 4.0; }
		}

		public virtual double VotMediumHigh {
			get { return 12.0; }
		}

		public virtual double TimeCoefficient {
			get { return -0.02; }
		}

		public virtual double CostCoefficientLow {
			get { return -0.60; }
		}

		public virtual double CostCoefficientMedium {
			get { return -0.15; }
		}

		public virtual double CostCoefficientHigh {
			get { return -0.06; }
		}
	}

	public class DefaultCarOwnerships : ICarOwnerships {
		public virtual int TotalCarOwnerships {
			get { return 4; }
		}

		public virtual int Child {
			get { return 0; }
		}

		public virtual int NoCars {
			get { return 1; }
		}

		public virtual int LtOneCarPerAdult {
			get { return 2; }
		}

		public virtual int OneOrMoreCarsPerAdult {
			get { return 3; }
		}
	}

	public class DefaultAddressTypes : IAddressTypes {
		public virtual int None {
			get { return 0; }
		}

		public virtual int Home {
			get { return 1; }
		}

		public virtual int UsualWorkplace {
			get { return 2; }
		}

		public virtual int UsualSchool {
			get { return 3; }
		}

		public virtual int Other {
			get { return 4; }
		}

		public virtual int Missing {
			get { return 5; }
		}

		public virtual int ChangeMode {
			get { return 6; }
		}
	}

	public class DefaultValueOfTimes : IValueOfTimes {
		public virtual int Low {
			get { return 1; }
		}

		public virtual int Medium {
			get { return 2; }
		}

		public virtual int High {
			get { return 3; }
		}

		public virtual double DefaultVot {
			get { return 10; }
		}
	}

	public class DefaultModels : IModels {
		public virtual int WorkTourModeModel {
			get { return 0; }
		}

		public virtual int SchoolTourModeModel {
			get { return 1; }
		}

		public virtual int WorkbasedSubtourModeModel {
			get { return 2; }
		}

		public virtual int EscortTourModeModel {
			get { return 3; }
		}

		public virtual int OtherHomeBasedTourMode {
			get { return 4; }
		}
	}

	public class DefaultTimes : ITimes {
		public virtual int MinutesInADay {
			get { return 1440; }
		}

		public virtual int MinimumActivityDuration {
			get { return 1; }
		}

		public virtual int ZeroHours {
			get { return 0; }
		}

		public virtual int OneHour {
			get { return 60 * 1; }
		}

		public virtual int TwoHours {
			get { return 60 * 2; }
		}

		public virtual int ThreeHours {
			get { return 60 * 3; }
		}

		public virtual int FourHours {
			get { return 60 * 4; }
		}

		public virtual int FiveHours {
			get { return 60 * 5; }
		}

		public virtual int SixHours {
			get { return 60 * 6; }
		}

		public virtual int SevenHours {
			get { return 60 * 7; }
		}

		public virtual int EightHours {
			get { return 60 * 8; }
		}

		public virtual int NineHours {
			get { return 60 * 9; }
		}

		public virtual int TenHours {
			get { return 60 * 10; }
		}

		public virtual int ElevenHours {
			get { return 60 * 11; }
		}

		public virtual int TwelveHours {
			get { return 60 * 12; }
		}

		public virtual int ThirteenHours {
			get { return 60 * 13; }
		}

		public virtual int FourteenHours {
			get { return 60 * 14; }
		}

		public virtual int FifteenHours {
			get { return 60 * 15; }
		}

		public virtual int SixteenHours {
			get { return 60 * 16; }
		}

		public virtual int SeventeenHours {
			get { return 60 * 17; }
		}

		public virtual int EighteenHours {
			get { return 60 * 18; }
		}

		public virtual int NineteenHours {
			get { return 60 * 19; }
		}

		public virtual int TwentyHours {
			get { return 60 * 20; }
		}

		public virtual int TwentyOneHours {
			get { return 60 * 21; }
		}

		public virtual int TwentyTwoHours {
			get { return 60 * 22; }
		}

		public virtual int TwentyThreeHours {
			get { return 60 * 23; }
		}

		public virtual int TwentyFourHours {
			get { return 60 * 24; }
		}

		public virtual int ThreeAM {
			get { return 1; }
		}

		public virtual int FourAM {
			get { return 60 * 1 + 1; }
		}

		public virtual int FiveAM {
			get { return 60 * 2 + 1; }
		}

		public virtual int SixAM {
			get { return 60 * 3 + 1; }
		}

		public virtual int SevenAM {
			get { return 60 * 4 + 1; }
		}

		public virtual int EightAM {
			get { return 60 * 5 + 1; }
		}

		public virtual int NineAM {
			get { return 60 * 6 + 1; }
		}

		public virtual int TenAM {
			get { return 60 * 7 + 1; }
		}

		public virtual int ElevenAM {
			get { return 60 * 8 + 1; }
		}

		public virtual int Noon {
			get { return 60 * 9 + 1; }
		}

		public virtual int OnePM {
			get { return 60 * 10 + 1; }
		}

		public virtual int TwoPM {
			get { return 60 * 11 + 1; }
		}

		public virtual int ThreePM {
			get { return 60 * 12 + 1; }
		}

		public virtual int FourPM {
			get { return 60 * 13 + 1; }
		}

		public virtual int FivePM {
			get { return 60 * 14 + 1; }
		}

		public virtual int SixPM {
			get { return 60 * 15 + 1; }
		}

		public virtual int SevenPM {
			get { return 60 * 16 + 1; }
		}

		public virtual int EightPM {
			get { return 60 * 17 + 1; }
		}

		public virtual int NinePM {
			get { return 60 * 18 + 1; }
		}

		public virtual int TenPM {
			get { return 60 * 19 + 1; }
		}

		public virtual int ElevenPM {
			get { return 60 * 20 + 1; }
		}

		public virtual int Midnight {
			get { return 60 * 21 + 1; }
		}

		public virtual int OneAM {
			get { return 60 * 22 + 1; }
		}

		public virtual int TwoAM {
			get { return 60 * 23 + 1; }
		}

		public virtual int EndOfRelevantWindow {
			get { return 60 * 21 + 1; }
		}
	}

	public class DefaultHouseholdTypes : IHouseholdTypes {
		public virtual int IndividualWorkerStudent {
			get { return 1; }
		}

		public virtual int IndividualNonworkerNonstudent {
			get { return 2; }
		}

		public virtual int OneAdultWithChildren {
			get { return 3; }
		}

		public virtual int TwoPlusWorkerStudentAdultsWithChildren {
			get { return 4; }
		}

		public virtual int TwoPlusAdultsOnePlusWorkersStudentsWithChildren {
			get { return 5; }
		}

		public virtual int TwoPlusWorkerStudentAdultsWithoutChildren {
			get { return 6; }
		}

		public virtual int OnePlusWorkerStudentAdultsAndOnePlusNonworkerNonstudentAdultsWithoutChildren {
			get { return 7; }
		}

		public virtual int TwoPlusNonworkerNonstudentAdultsWithoutChildren {
			get { return 8; }
		}
	}

	public class DefaultMaxInputs : IMaxInputs {
		public virtual int MaxParcelVals {
			get { return 200000; }
		}

		public virtual int MaxHhSize {
			get { return 20; }
		}

		public virtual int MaxAge {
			get { return 200; }
		}

		public virtual int MaxGend {
			get { return 3; }
		}

		public virtual int MaxPwtyp {
			get { return 3; }
		}

		public virtual int MaxPstyp {
			get { return 3; }
		}

		public virtual int MaxPnrCap {
			get { return 50000; }
		}

		public virtual int MaxPnrCost {
			get { return 10000; }
		}
	}
}