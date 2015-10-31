// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.PathTypeModels;
using HouseholdDayWrapper = Daysim.DomainModels.LD.Wrappers.HouseholdDayWrapper;
using TripWrapper = Daysim.DomainModels.LD.Wrappers.TripWrapper;

namespace Daysim.ChoiceModels.LD.Models
{
	public class TripModeModel : ChoiceModel
	{
		private const string CHOICE_MODEL_NAME = "LDTripModeModel";
		private const int MAX_PARAMETER = 199;

		// this is Mark B (leave it out)
		//private const int TOTAL_NESTED_ALTERNATIVES = 5;
		//private const int TOTAL_LEVELS = 2;

		// GV for tree Logit uncomment this
		private const int TOTAL_NESTED_ALTERNATIVES = 6;
		private const int TOTAL_LEVELS = 2;
		// GV for tree Logit comment out this
		//private const int TOTAL_NESTED_ALTERNATIVES = 0;
		//private const int TOTAL_LEVELS = 1;

		// GV for tree Logit uncomment this
		private const int THETA_PARAMETER = 199;

		// GV for tree Logit uncomment this (both lines)
		// this is Mark B (leave it out)
		//private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 0, 0 };
		//private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 0, 0 };

		// GVs Tree Logit uncomment this
		// 1 works: bike, walk and PT in one and all 3 car modes in one. Theta is estimated to 0.33 and significant
		private readonly int[] _nestedAlternativeIds = new[] {0, 19, 19, 20, 20, 20, 19, 0, 0};
		private readonly int[] _nestedAlternativeIndexes = new[] {0, 1, 1, 2, 2, 2, 1, 0, 0};
		// 2 works: bike&walk in one, PT alone, CD1 (SOV) alone, and 2 car-Pass modes in one. Theta is estimated to 0.93 and significant
		// Structure-1 works much better than Structure-2
		//private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 21, 20, 20, 22, 0, 0 };
		//private readonly int[] _nestedAlternativeIndexes = new[] { 0, 1, 1, 0, 2, 2, 0, 0, 0 };
		// 3 works: bike&walk&PT in one, CD1 (SOV) alone, and 2 car-Pass modes in one. Theta is estimated to 0.35 and significant
		// Structure-1 works better than Structure-3. Structure-3 works much better than Structure-2
		//private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 21, 20, 20, 19, 0, 0 };
		//private readonly int[] _nestedAlternativeIndexes = new[] { 0, 1, 1, 0, 2, 2, 1, 0, 0 };
		// 4 DOES NOT work: bike&walk&PT&Two_Car_Pass_Modes in one and CD1 (SOV) alone. Theta is estimated to value greater than 1.00
		//private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 21, 19, 19, 19, 0, 0 };
		//private readonly int[] _nestedAlternativeIndexes = new[] { 0, 1, 1, 0, 1, 1, 1, 0, 0 };
		// 5 works: bike&walk&PT&Car_pass in one, CD1 (SOV) & CD2 in one. Theta is estimated to 0.35 and significant
		// Structure-1 works better than Structure-5. 
		//private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 21, 21, 19, 19, 0, 0 };
		//private readonly int[] _nestedAlternativeIndexes = new[] { 0, 1, 1, 2, 2, 1, 1, 0, 0 };

		public override void RunInitialize(ICoefficientsReader reader = null)
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.TripModeModelCoefficients, Global.Settings.Modes.TotalModes,
			           TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(HouseholdDayWrapper householdDay, TripWrapper trip)
		{
			if (trip == null)
			{
				throw new ArgumentNullException("trip");
			}

			if (householdDay.Household.Id == 80073 && trip.Day == 1 && trip.Person.Sequence == 1
			    && trip.Tour.Sequence == 2 && trip.Direction == 1 && trip.Sequence == 1)
			{
				bool testbreak = true;
			}

			trip.PersonDay.ResetRandom(40*(2*trip.Tour.Sequence - 1 + trip.Direction - 1) + 40 + trip.Sequence - 1);

			if (Global.Configuration.IsInEstimationMode)
			{
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME)
				{
					return;
				}
			}

			var choiceProbabilityCalculator =
				_helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(trip.Id);

			var originParcel =
				trip.IsHalfTourFromOrigin
					? trip.DestinationParcel
					: trip.OriginParcel;

			// for skims - use actual travel direction, not simulation direction
			var destinationParcel =
				trip.IsHalfTourFromOrigin
					? trip.OriginParcel
					: trip.DestinationParcel;


			var departureTime = trip.IsHalfTourFromOrigin ? trip.LatestDepartureTime : trip.EarliestDepartureTime;

			if (departureTime < 1)
			{
				Global.PrintFile.WriteLine("From origin / latest / earliest  {0} {1} {2}", trip.IsHalfTourFromOrigin,
				                           trip.LatestDepartureTime, trip.EarliestDepartureTime);
				if (!Global.Configuration.IsInEstimationMode) {
					trip.PersonDay.IsValid = false;
					householdDay.IsValid = false;
				}
				return;
			}

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode)
			{
				if (destinationParcel == null || originParcel == null || trip.Mode <= Global.Settings.Modes.None ||
				    trip.Mode > Global.Settings.Modes.Transit)
				{
					return;
				}

				IEnumerable<dynamic> pathTypeModels =
					PathTypeModelFactory.Model.RunAll(
						trip.Household.RandomUtility,
						originParcel,
						destinationParcel,
						departureTime,
						0,
						trip.Tour.DestinationPurpose,
						trip.Tour.CostCoefficient,
						trip.Tour.TimeCoefficient,
						trip.Person.IsDrivingAge,
						trip.Household.VehiclesAvailable,
						trip.Person.GetTransitFareDiscountFraction(),
						false);

				// GV, July 9th: COMPAS modes are: Bike, Walk, PT, SOV (car driver alone), HOV2 (car driver in a car with a passenger) and HOV3 (car passenger) 
				// GV, July 9th: COMPAS has no follwing modes: School bus

				//// there is no path type model for school bus, use HOV3
				//var mode = trip.Mode == Global.Settings.Modes.SchoolBus ? Global.Settings.Modes.Hov3 : trip.Mode;
				//var pathTypeModel = pathTypeModels.First(x => x.Mode == mode);
				//
				//if (!pathTypeModel.Available) {
				//	return;
				//}

				RunModel(choiceProbabilityCalculator, trip, pathTypeModels, originParcel, destinationParcel, trip.Mode);

				choiceProbabilityCalculator.WriteObservation();
			}
			else
			{
				IEnumerable<dynamic> pathTypeModels =
					PathTypeModelFactory.Model.RunAll(
						trip.Household.RandomUtility,
						originParcel,
						destinationParcel,
						departureTime,
						0,
						trip.Tour.DestinationPurpose,
						trip.Tour.CostCoefficient,
						trip.Tour.TimeCoefficient,
						trip.Person.IsDrivingAge,
						trip.Household.VehiclesAvailable,
						trip.Person.GetTransitFareDiscountFraction(),
						false);

				RunModel(choiceProbabilityCalculator, trip, pathTypeModels, originParcel, destinationParcel);

				var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);

				if (chosenAlternative == null)
				{
					Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", 100*trip.Tour.DestinationPurpose + trip.Tour.Mode);
					trip.Mode = Global.Settings.Modes.Hov3;
					if (!Global.Configuration.IsInEstimationMode)
					{
						trip.PersonDay.IsValid = false;
					}
					return;
				}

				var choice = (int) chosenAlternative.Choice;

				trip.Mode = choice;
				if (choice == Global.Settings.Modes.SchoolBus)
				{
					trip.PathType = 0;
				}

					//else if (Global.Configuration.TestEstimationModelInApplicationMode)
					//{
					//    Global.Configuration.IsInEstimationMode = false;
					//
					//    RunModel(choiceProbabilityCalculator, trip, pathTypeModels, originParcel, destinationParcel);
					//
					//    var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, householdDay.PrimaryPriorityTimeFlag);
					//
					//    Global.Configuration.IsInEstimationMode = true;
					//}

				else
				{
					var chosenPathType = pathTypeModels.First(x => x.Mode == choice);
					trip.PathType = chosenPathType.PathType;
				}
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, TripWrapper trip,
		                      IEnumerable<dynamic> pathTypeModels, IParcelWrapper originParcel,
		                      IParcelWrapper destinationParcel,
		                      int choice = Constants.DEFAULT_VALUE)
		{

	
			var household = trip.Household;
			var householdTotals = household.HouseholdTotals;
			var person = trip.Person;
			var tour = trip.Tour;
			var halfTour = trip.HalfTour;

			// household inputs
			var onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
			var HHwithChildrenFlag = household.HasChildren.ToFlag();
			var HHwithSmallChildrenFlag = household.HasChildrenUnder5.ToFlag();
			var childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
			var HHwithLowIncomeFlag = (household.Income >= 300000 && household.Income < 600000).ToFlag();
			var HHwithMidleIncomeFlag = (household.Income >= 600000 && household.Income < 900000).ToFlag();
			var HHwithHighIncomeFlag = (household.Income >= 900000).ToFlag();
			var nonworkingAdults = householdTotals.NonworkingAdults;
			var retiredAdults = householdTotals.RetiredAdults;
			var twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
			var noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(household.VehiclesAvailable);
			var carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(household.VehiclesAvailable);

			// person inputs
			//var drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
			var maleFlag = person.IsMale.ToFlag();
			var femaleFlag = person.IsFemale.ToFlag();

			var PTpass = person.TransitPassOwnership;

			//var ageLessThan35Flag = person.AgeIsLessThan35.ToFlag();

			// tour inputs
			var bikeTourFlag = tour.IsBikeMode().ToFlag();
			var walkTourFlag = tour.IsWalkMode().ToFlag();
			var CarDrivAloneFlag = tour.IsSovMode().ToFlag();
			var CarDrivNotAloneFlag = tour.IsHov2Mode().ToFlag();
			var CarPassengerFlag = tour.IsHov3Mode().ToFlag();
			var transitTourFlag = tour.IsTransitMode().ToFlag();

			var homeBasedWorkTourFlag = (tour.IsHomeBasedTour && tour.IsWorkPurpose()).ToFlag();
			var homeBasedSchoolTourFlag = (tour.IsHomeBasedTour && tour.IsSchoolPurpose()).ToFlag();
			var homeBasedEscortTourFlag = (tour.IsHomeBasedTour && tour.IsEscortPurpose()).ToFlag();
			var homeBasedShoppingTourFlag = (tour.IsHomeBasedTour && tour.IsShoppingPurpose()).ToFlag();
			var homeBasedSocialTourFlag = (tour.IsHomeBasedTour && tour.IsSocialPurpose()).ToFlag();
			var notHomeBasedTourFlag = (!tour.IsHomeBasedTour).ToFlag();
			var homeBasedNotWorkSchoolEscortTourFlag =
				(tour.IsHomeBasedTour && tour.DestinationPurpose > Global.Settings.Purposes.Escort).ToFlag();

			var jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
			var partialHalfTourFlag = (trip.IsHalfTourFromOrigin
				                           ? tour.PartialHalfTour1Sequence > 0
				                           : tour.PartialHalfTour2Sequence > 0)
				                          ? 1
				                          : 0;
			var fullHalfTourFlag = (trip.IsHalfTourFromOrigin ? tour.FullHalfTour1Sequence > 0 : tour.FullHalfTour2Sequence > 0)
				                       ? 1
				                       : 0;


			// trip inputs
			var originHomeEscortFlag = (trip.IsNoneOrHomePurposeByOrigin() && trip.IsEscortPurposeByDestination()).ToFlag();
			var originWorkEscortFlag = (trip.IsWorkPurposeByOrigin() && trip.IsEscortPurposeByDestination()).ToFlag();

			var destinationHomeEscortFlag = (trip.IsNoneOrHomePurposeByDestination() && trip.IsEscortPurposeByOrigin()).ToFlag();
			var destinationWorkEscortFlag = (trip.IsWorkPurposeByDestination() && trip.IsEscortPurposeByOrigin()).ToFlag();

			// only trip on first half-tour
			var onlyTripOnFirstHalfFlag =
				(trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && trip.IsToTourOrigin).ToFlag();

			// first trip on first half-tour, not only one
			var firstTripOnFirstHalfFlag =
				(trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && !trip.IsToTourOrigin).ToFlag();

			// last trip first half-tour, not only one
			var lastTripOnFirstHalfFlag =
				(trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips > 1 && trip.IsToTourOrigin).ToFlag();

			// only trip on second half-tour
			var onlyTripOnSecondHalfFlag =
				(!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && trip.IsToTourOrigin).ToFlag();

			// first trip on second half-tour, not only one
			var firstTripOnSecondHalfFlag =
				(!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips == 1 && !trip.IsToTourOrigin).ToFlag();

			// last trip second half-tour, not only one
			var lastTripOnSecondHalfFlag =
				(!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips > 1 && trip.IsToTourOrigin).ToFlag();

			// remaining inputs
			var departureTime = trip.IsHalfTourFromOrigin ? trip.LatestDepartureTime : trip.EarliestDepartureTime;

			var originMixedDensity = originParcel.MixedUse4Index1();
			var originIntersectionDensity = originParcel.NetIntersectionDensity1();
			var destinationParkingCost = destinationParcel.ParkingCostBuffer1(2);
			var amPeriodFlag = departureTime.IsLeftExclusiveBetween(Global.Settings.Times.SixAM, Global.Settings.Times.NineAM).ToFlag();
				//GV changed to 6-9 am
			var middayPeriodFlag = departureTime.IsLeftExclusiveBetween(Global.Settings.Times.NineAM, Global.Settings.Times.ThreePM).ToFlag();
			var pmPeriodFlag = departureTime.IsLeftExclusiveBetween(Global.Settings.Times.ThreePM, Global.Settings.Times.SixPM).ToFlag();
				//GV changed to 3-6 pm
			var eveningPeriodFlag = (departureTime > Global.Settings.Times.SixPM).ToFlag(); //GV changed to 6 pm

			// availability
			var tripModeAvailable = new bool[Global.Settings.Modes.TotalModes];

			var isLastTripInTour = (!trip.IsHalfTourFromOrigin && halfTour.SimulatedTrips >= 1 && trip.IsToTourOrigin);
			var frequencyPreviousTripModeIsTourMode = 0;
			if (trip.IsHalfTourFromOrigin)
			{
				frequencyPreviousTripModeIsTourMode +=
					tour.HalfTourFromOrigin.Trips.Where(x => x.Sequence < trip.Sequence).Count(x => tour.Mode == x.Mode);
			}
			else
			{
				if (tour.HalfTourFromOrigin != null)
				{
					frequencyPreviousTripModeIsTourMode +=
						tour.HalfTourFromOrigin.Trips.Where(x => x.Sequence > 0).Count(x => tour.Mode == x.Mode);
				}
				frequencyPreviousTripModeIsTourMode +=
					tour.HalfTourFromDestination.Trips.Where(x => x.Sequence < trip.Sequence).Count(x => tour.Mode == x.Mode);
			}

			// GV commented out park and ride
			//// if a park and ride tour, only car is available
			//if (tour.Mode == Global.Settings.Modes.ParkAndRide) {
			//	tripModeAvailable[Global.Settings.Modes.Sov] = tour.Household.VehiclesAvailable > 0 && tour.Person.IsDrivingAge;
			//	tripModeAvailable[Global.Settings.Modes.Hov2] = !tripModeAvailable[Global.Settings.Modes.Sov];
			//}
			//// if the last trip of the tour and tour mode not yet used, only the tour mode is available
			if (isLastTripInTour && frequencyPreviousTripModeIsTourMode == 0)
			{
				tripModeAvailable[tour.Mode] = true;
			}
			else
			{
				// set availability based on tour mode
				for (var mode = Global.Settings.Modes.Walk; mode <= tour.Mode; mode++)
				{
					tripModeAvailable[mode] = true;
				}
			}
			if (person.Age < 18)
			{
				tripModeAvailable[Global.Settings.Modes.Sov] = false;
				tripModeAvailable[Global.Settings.Modes.HovDriver] = false;
			}

			// GV commented out School Bus
			// school bus is a special case - use HOV3 impedance and only available for school bus tours
			//var pathTypeExtra = pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Hov3);
			//const int modeExtra = Global.Settings.Modes.SchoolBus;
			//var availableExtra = pathTypeExtra.Available && tour.IsSchoolBusMode && tripModeAvailable[modeExtra]
			//	 && (trip.IsHalfTourFromOrigin
			//		  ? trip.LatestDepartureTime - pathTypeExtra.PathTime >= trip.ArrivalTimeLimit
			//		  : trip.EarliestDepartureTime + pathTypeExtra.PathTime <= trip.ArrivalTimeLimit);
			//var generalizedTimeLogsumExtra = pathTypeExtra.GeneralizedTimeLogsum;

			//var alternative = choiceProbabilityCalculator.GetAlternative(modeExtra, availableExtra, choice == modeExtra);
			//alternative.Choice = modeExtra;

			//alternative.AddNestedAlternative(_nestedAlternativeIds[modeExtra], _nestedAlternativeIndexes[modeExtra], THETA_PARAMETER);
			//
			//if (availableExtra) {
			//	//	case Global.Settings.Modes.SchoolBus:
			//	alternative.AddUtilityTerm(2, generalizedTimeLogsumExtra * tour.TimeCoefficient);
			//	alternative.AddUtilityTerm(18, 1);
			//	alternative.AddUtilityTerm(100, schoolBusTourFlag);
			//	alternative.AddUtilityTerm(102, (schoolBusTourFlag * onlyTripOnFirstHalfFlag));
			//	alternative.AddUtilityTerm(103, (schoolBusTourFlag * onlyTripOnSecondHalfFlag));
			//	alternative.AddUtilityTerm(104, (schoolBusTourFlag * firstTripOnFirstHalfFlag));
			//	alternative.AddUtilityTerm(105, (schoolBusTourFlag * firstTripOnSecondHalfFlag));
			//	alternative.AddUtilityTerm(106, (schoolBusTourFlag * lastTripOnFirstHalfFlag));
			//	alternative.AddUtilityTerm(107, (schoolBusTourFlag * lastTripOnSecondHalfFlag));
			//	alternative.AddUtilityTerm(112, parkAndRideTourFlag);
			//	alternative.AddUtilityTerm(113, transitTourFlag);
			//}

			ChoiceProbabilityCalculator.Alternative alternative;

			foreach (var pathTypeModel in pathTypeModels)
			{
				var mode = pathTypeModel.Mode;
				var available = pathTypeModel.Available && tripModeAvailable[mode]
				                && (trip.IsHalfTourFromOrigin
					                    ? trip.LatestDepartureTime - pathTypeModel.PathTime >= trip.ArrivalTimeLimit
					                    : trip.EarliestDepartureTime + pathTypeModel.PathTime <= trip.ArrivalTimeLimit);
				var generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

				alternative = choiceProbabilityCalculator.GetAlternative(mode - 1, available, choice == mode);
				alternative.Choice = mode;

				// GV for tree Logit uncomment this
				alternative.AddNestedAlternative(_nestedAlternativeIds[mode], _nestedAlternativeIndexes[mode], THETA_PARAMETER);

				if (!available)
				{
					continue;
				}

				alternative.AddUtilityTerm(2, generalizedTimeLogsum*tour.TimeCoefficient);

				if (mode == Global.Settings.Modes.Transit) {
					alternative.AddUtilityTerm(20, 1);
					alternative.AddUtilityTerm(22, carsLessThanDriversFlag);

					alternative.AddUtilityTerm(54, HHwithLowIncomeFlag);
					alternative.AddUtilityTerm(55, HHwithMidleIncomeFlag);
					alternative.AddUtilityTerm(56, HHwithHighIncomeFlag);

					alternative.AddUtilityTerm(57, PTpass);

					alternative.AddUtilityTerm(100, transitTourFlag);
					alternative.AddUtilityTerm(102, (transitTourFlag * onlyTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(103, (transitTourFlag * onlyTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(104, (transitTourFlag * firstTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(105, (transitTourFlag * firstTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(106, (transitTourFlag * lastTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(107, (transitTourFlag * lastTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(181, jointTourFlag);
					alternative.AddUtilityTerm(182, fullHalfTourFlag + partialHalfTourFlag);
				}
				else if (mode == Global.Settings.Modes.HovPassenger) {
					alternative.AddUtilityTerm(30, 1);
					alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT3));
					alternative.AddUtilityTerm(32, childrenAge5Through15);
					alternative.AddUtilityTerm(33, femaleFlag);
					alternative.AddUtilityTerm(34, (nonworkingAdults + retiredAdults));
					alternative.AddUtilityTerm(36, onePersonHouseholdFlag);
					alternative.AddUtilityTerm(37, twoPersonHouseholdFlag);
					alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
					alternative.AddUtilityTerm(100, CarPassengerFlag);
					alternative.AddUtilityTerm(102, (CarPassengerFlag * onlyTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(103, (CarPassengerFlag * onlyTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(104, (CarPassengerFlag * firstTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(105, (CarPassengerFlag * firstTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(106, (CarPassengerFlag * lastTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(107, (CarPassengerFlag * lastTripOnSecondHalfFlag));

					alternative.AddUtilityTerm(115, transitTourFlag);

					alternative.AddUtilityTerm(149, homeBasedWorkTourFlag);
					alternative.AddUtilityTerm(150, homeBasedSchoolTourFlag);
					alternative.AddUtilityTerm(152, homeBasedEscortTourFlag);
					alternative.AddUtilityTerm(153, homeBasedShoppingTourFlag);
					alternative.AddUtilityTerm(155, homeBasedSocialTourFlag);

					alternative.AddUtilityTerm(161, (destinationWorkEscortFlag * amPeriodFlag));
					alternative.AddUtilityTerm(162, (originWorkEscortFlag * pmPeriodFlag));
					alternative.AddUtilityTerm(163, (originHomeEscortFlag * amPeriodFlag));
					alternative.AddUtilityTerm(164, (originHomeEscortFlag * middayPeriodFlag));
					alternative.AddUtilityTerm(165, (originHomeEscortFlag * pmPeriodFlag));
					alternative.AddUtilityTerm(166, (originHomeEscortFlag * eveningPeriodFlag));
					alternative.AddUtilityTerm(167, (destinationHomeEscortFlag * amPeriodFlag));
					alternative.AddUtilityTerm(168, (destinationHomeEscortFlag * middayPeriodFlag));
					alternative.AddUtilityTerm(169, (destinationHomeEscortFlag * pmPeriodFlag));
					alternative.AddUtilityTerm(170, (destinationHomeEscortFlag * eveningPeriodFlag));
					alternative.AddUtilityTerm(183, jointTourFlag);
					alternative.AddUtilityTerm(184, fullHalfTourFlag + partialHalfTourFlag);
				}
				else if (mode == Global.Settings.Modes.HovDriver) {
					alternative.AddUtilityTerm(40, 1);
					alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT2));
					alternative.AddUtilityTerm(42, (childrenAge5Through15 * (1 - homeBasedEscortTourFlag)));
					alternative.AddUtilityTerm(43, (femaleFlag * (1 - homeBasedEscortTourFlag)));
					alternative.AddUtilityTerm(44, ((nonworkingAdults + retiredAdults) * (1 - homeBasedEscortTourFlag)));
					alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
					alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
					alternative.AddUtilityTerm(100, CarDrivNotAloneFlag);
					alternative.AddUtilityTerm(102, (CarDrivNotAloneFlag * onlyTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(103, (CarDrivNotAloneFlag * onlyTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(104, (CarDrivNotAloneFlag * firstTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(105, (CarDrivNotAloneFlag * firstTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(106, (CarDrivNotAloneFlag * lastTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(107, (CarDrivNotAloneFlag * lastTripOnSecondHalfFlag));

					alternative.AddUtilityTerm(118, transitTourFlag);
					alternative.AddUtilityTerm(120, CarPassengerFlag);

					alternative.AddUtilityTerm(149, homeBasedWorkTourFlag);
					alternative.AddUtilityTerm(150, homeBasedSchoolTourFlag);
					alternative.AddUtilityTerm(152, homeBasedEscortTourFlag);
					alternative.AddUtilityTerm(153, homeBasedShoppingTourFlag);
					alternative.AddUtilityTerm(155, homeBasedSocialTourFlag);

					alternative.AddUtilityTerm(161, (destinationWorkEscortFlag * amPeriodFlag));
					alternative.AddUtilityTerm(162, (originWorkEscortFlag * pmPeriodFlag));
					alternative.AddUtilityTerm(163, (originHomeEscortFlag * amPeriodFlag));
					alternative.AddUtilityTerm(164, (originHomeEscortFlag * middayPeriodFlag));
					alternative.AddUtilityTerm(165, (originHomeEscortFlag * pmPeriodFlag));
					alternative.AddUtilityTerm(166, (originHomeEscortFlag * eveningPeriodFlag));
					alternative.AddUtilityTerm(167, (destinationHomeEscortFlag * amPeriodFlag));
					alternative.AddUtilityTerm(168, (destinationHomeEscortFlag * middayPeriodFlag));
					alternative.AddUtilityTerm(169, (destinationHomeEscortFlag * pmPeriodFlag));
					alternative.AddUtilityTerm(170, (destinationHomeEscortFlag * eveningPeriodFlag));
				}
				else if (mode == Global.Settings.Modes.Sov) {
					alternative.AddUtilityTerm(50, 1);
					alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient));
					alternative.AddUtilityTerm(52, carsLessThanDriversFlag);

					alternative.AddUtilityTerm(100, CarDrivAloneFlag);
					alternative.AddUtilityTerm(102, (CarDrivAloneFlag * onlyTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(103, (CarDrivAloneFlag * onlyTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(104, (CarDrivAloneFlag * firstTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(105, (CarDrivAloneFlag * firstTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(106, (CarDrivAloneFlag * lastTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(107, (CarDrivAloneFlag * lastTripOnSecondHalfFlag));

					alternative.AddUtilityTerm(122, transitTourFlag);
					alternative.AddUtilityTerm(124, CarPassengerFlag);
					alternative.AddUtilityTerm(125, CarDrivNotAloneFlag);

					alternative.AddUtilityTerm(126, maleFlag);

					alternative.AddUtilityTerm(185, jointTourFlag);
					alternative.AddUtilityTerm(186, fullHalfTourFlag + partialHalfTourFlag);
				}
				else if (mode == Global.Settings.Modes.Bike) {
					alternative.AddUtilityTerm(60, 1);
					//alternative.AddUtilityTerm(65, originIntersectionDensity);

					alternative.AddUtilityTerm(100, bikeTourFlag);
					alternative.AddUtilityTerm(102, (bikeTourFlag * onlyTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(103, (bikeTourFlag * onlyTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(104, (bikeTourFlag * firstTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(105, (bikeTourFlag * firstTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(106, (bikeTourFlag * lastTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(107, (bikeTourFlag * lastTripOnSecondHalfFlag));

					alternative.AddUtilityTerm(127, transitTourFlag);
					alternative.AddUtilityTerm(130, CarDrivNotAloneFlag);
					alternative.AddUtilityTerm(131, CarDrivAloneFlag);

					alternative.AddUtilityTerm(132, maleFlag);

					alternative.AddUtilityTerm(147, notHomeBasedTourFlag);
					alternative.AddUtilityTerm(187, jointTourFlag);
					alternative.AddUtilityTerm(188, fullHalfTourFlag + partialHalfTourFlag);
				}
				else if (mode == Global.Settings.Modes.Walk) {
					alternative.AddUtilityTerm(70, 1);

					//alternative.AddUtilityTerm(75, originIntersectionDensity);
					alternative.AddUtilityTerm(78, originMixedDensity);
					// origin and destination mixed use measures - geometric avg. - half mile from cell, in 1000s

					alternative.AddUtilityTerm(100, walkTourFlag);
					alternative.AddUtilityTerm(102, (walkTourFlag * onlyTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(103, (walkTourFlag * onlyTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(104, (walkTourFlag * firstTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(105, (walkTourFlag * firstTripOnSecondHalfFlag));
					alternative.AddUtilityTerm(106, (walkTourFlag * lastTripOnFirstHalfFlag));
					alternative.AddUtilityTerm(107, (walkTourFlag * lastTripOnSecondHalfFlag));

					alternative.AddUtilityTerm(141, homeBasedWorkTourFlag);
					alternative.AddUtilityTerm(142, homeBasedSchoolTourFlag);

					alternative.AddUtilityTerm(187, jointTourFlag);
					alternative.AddUtilityTerm(188, fullHalfTourFlag + partialHalfTourFlag);
				}
			}
		}
	}
}

