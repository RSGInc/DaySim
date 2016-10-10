// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using Daysim.DomainModels;
using Daysim.DomainModels.LD;
using Daysim.DomainModels.LD.Wrappers;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Extensions;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Roster;
using Daysim.Framework.Sampling;
using Daysim.Sampling;
using Ninject;
using HouseholdDayWrapper = Daysim.DomainModels.LD.Wrappers.HouseholdDayWrapper;
using HouseholdWrapper = Daysim.DomainModels.LD.Wrappers.HouseholdWrapper;
using PersonDayWrapper = Daysim.DomainModels.LD.Wrappers.PersonDayWrapper;
using PersonWrapper = Daysim.DomainModels.LD.Wrappers.PersonWrapper;
using TourWrapper = Daysim.DomainModels.LD.Wrappers.TourWrapper;

namespace Daysim.ChoiceModels.LD.Models {
	public class TourDestinationModeTimeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "LDTourDestinationModeTimeModel";
		private const int MODES_USED = 6;
		private const int TOTAL_LEVELS = 3;
		private const int MAX_PARAMETER = 999;
		private const int THETA_PARAMETER_1 = 998;
		private const int THETA_PARAMETER_2 = 999;

		private static int timesStartedRunModel;
		private static int maxMode = Global.Settings.Modes.Transit;

		public override void RunInitialize(ICoefficientsReader reader = null) {
			int sampleSize = Global.Configuration.TourDestinationModelSampleSize;
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.TourDestinationModeTimeModelCoefficients, sampleSize * HTourModeTime.TotalTourModeTimes, HTourModeTime.TotalTourModeTimes, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(HouseholdDayWrapper householdDay, TourWrapper tour, IParcelWrapper constrainedParcel,
			int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime) {
			if (tour == null) {
				throw new ArgumentNullException("tour");
			}

			tour.PersonDay.ResetRandom(20 + tour.Sequence - 1);

			int sampleSize = Global.Configuration.TourDestinationModelSampleSize;

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
				if (!TourDestinationUtilities.ShouldRunInEstimationModeForModel(tour)) {
					return;
				}
				// JLB 20140421 add the following to keep from estimating twice for the same tour
				if (tour.DestinationModeAndTimeHaveBeenSimulated) {
					return;
				}
				// JLB 20140704 add the following to keep from processing unhandled modes
				if (tour.Mode > maxMode) {
					return;
				}
			}

			if (tour.DestinationPurpose == Global.Settings.Purposes.School) {
				return;
			}
			else if (tour.DestinationPurpose == Global.Settings.Purposes.Work) {
				return;
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);


			if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

				var observedModeTimes = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);
				var observedChoice = new HTourDestinationModeTime(tour.DestinationParcel, observedModeTimes);

				//If estimating conditional mode-time model coefficients only, uncomment the following line:
				//constrainedParcel = tour.DestinationParcel;
				
				//If estimating conditional destination model coefficients only, uncomment the following three lines:
				//constrainedMode = tour.Mode;
				//constrainedArrivalTime = tour.DestinationArrivalTime;
				//constrainedDepartureTime = tour.DestinationDepartureTime;


				RunModel(choiceProbabilityCalculator, householdDay, tour, sampleSize, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime, -1, -1.0, observedChoice);

				choiceProbabilityCalculator.WriteObservation();
			}
			else {

				HTourDestinationModeTime choice;

				if (constrainedParcel != null && constrainedMode > 0 && constrainedArrivalTime > 0 && constrainedDepartureTime > 0) {
					choice = new HTourDestinationModeTime(constrainedParcel, new HTourModeTime(constrainedMode, constrainedArrivalTime, constrainedDepartureTime));
				}
				else {

					RunModel(choiceProbabilityCalculator, householdDay, tour, sampleSize, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime, -1, -1.0);

					var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

					if (chosenAlternative == null) {
						Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
						if (!Global.Configuration.IsInEstimationMode) {
							tour.PersonDay.IsValid = false;
							tour.PersonDay.HouseholdDay.IsValid = false;
						}

						return;
					}

					choice = (HTourDestinationModeTime) chosenAlternative.Choice;
				}
				tour.DestinationParcelId = choice.Destination.Id;
				tour.DestinationParcel = choice.Destination;
				tour.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[choice.Destination.ZoneId];
				tour.DestinationAddressType = Global.Settings.AddressTypes.Other;

				tour.Mode = choice.ModeTimes.Mode;
				var arrivalPeriod = choice.ModeTimes.ArrivalPeriod;
				var departurePeriod = choice.ModeTimes.DeparturePeriod;
				//use constrained times to set temporary arrival and departure times with minimum duration of stay for time window calculations
				if (constrainedArrivalTime > 0 || constrainedDepartureTime > 0) {
					if (constrainedArrivalTime > 0) {
						tour.DestinationArrivalTime = constrainedArrivalTime;
					}
					else {
						tour.DestinationArrivalTime = Math.Min(arrivalPeriod.End, constrainedDepartureTime - Global.Settings.Times.MinimumActivityDuration);
					}
					if (constrainedDepartureTime > 0) {
						tour.DestinationDepartureTime = constrainedDepartureTime;
					}
					else {
						tour.DestinationDepartureTime = Math.Max(departurePeriod.Start, constrainedArrivalTime + Global.Settings.Times.MinimumActivityDuration);
					}
				}
				//or if times aren't constrained use periods to set temporary arrival and departure times with minimum duration of stay for time window calculations 
				else if (arrivalPeriod == departurePeriod) {
					var departureTime = Math.Max(choice.ModeTimes.GetRandomDepartureTime(householdDay, tour), departurePeriod.Start + Global.Settings.Times.MinimumActivityDuration);
					tour.DestinationArrivalTime = departureTime - Global.Settings.Times.MinimumActivityDuration;
					tour.DestinationDepartureTime = departureTime;
				}
				else if (arrivalPeriod.End == departurePeriod.Start - 1) {
					tour.DestinationArrivalTime = arrivalPeriod.End;
					tour.DestinationDepartureTime = arrivalPeriod.End + Global.Settings.Times.MinimumActivityDuration;
				}
				else {
					tour.DestinationArrivalTime = arrivalPeriod.End;
					tour.DestinationDepartureTime = departurePeriod.Start;
				}
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, TourWrapper tour, int sampleSize,
			IParcelWrapper constrainedParcel, int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime, int constrainedHouseholdCars, double constrainedTransitDiscountFraction, HTourDestinationModeTime choice = null) {
			timesStartedRunModel++;
			HouseholdWrapper household = (HouseholdWrapper) tour.Household;
			PersonWrapper person = (PersonWrapper) tour.Person;
			PersonDayWrapper personDay = (PersonDayWrapper) tour.PersonDay;

			if (personDay.Id == 299 || personDay.Id == 1349669) {
				bool testBreak = true;
			}


			//constraint booleans used below to avoid unnecessary sections of code
			bool destinationIsConstrained = constrainedParcel == null ? false : true;
			bool modeIsConstrained = constrainedMode > 0 ? true : false;
			bool timesAreConstrained = (constrainedArrivalTime > 0 && constrainedDepartureTime > 0) ? true : false;
			bool arrivalTimeIsConstrained = constrainedArrivalTime > 0 ? true : false;
			bool departureTimeIsConstrained = constrainedDepartureTime > 0 ? true : false;

			var householdTotals = household.HouseholdTotals;

			// household inputs
			var childrenUnder5 = householdTotals.ChildrenUnder5;
			var childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
			var nonworkingAdults = householdTotals.NonworkingAdults;
			var retiredAdults = householdTotals.RetiredAdults;

			var onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
			var twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();

			var householdCars = household.VehiclesAvailable;
			var noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
			var carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);
			var carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(householdCars);

			var HHwithChildrenFlag = household.HasChildren.ToFlag();
			var HHwithSmallChildrenFlag = household.HasChildrenUnder5.ToFlag();
			var HHwithLowIncomeFlag = (household.Income >= 300000 && household.Income < 600000).ToFlag();
			var HHwithMidleIncomeFlag = (household.Income >= 600000 && household.Income < 900000).ToFlag();
			var HHwithHighIncomeFlag = (household.Income >= 900000).ToFlag();

			var primaryFamilyTimeFlag = householdDay.PrimaryPriorityTimeFlag;


			// person inputs
			var partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
			var nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
			var universityStudentFlag = person.IsUniversityStudent.ToFlag();
			var retiredAdultFlag = person.IsRetiredAdult.ToFlag();
			var fullTimeWorkerFlag = person.IsFulltimeWorker.ToFlag();
			var childAge5Through15Flag = person.IsChildAge5Through15.ToFlag();
			var childUnder5Flag = person.IsChildUnder5.ToFlag();
			var adultFlag = person.IsAdult.ToFlag();

			var maleFlag = person.IsMale.ToFlag();
			var femaleFlag = person.IsFemale.ToFlag();

			var PTpass = person.TransitPassOwnership;

			// person-day inputs
			var homeBasedToursOnlyFlag = personDay.OnlyHomeBasedToursExist().ToFlag();
			var firstSimulatedHomeBasedTourFlag = personDay.IsFirstSimulatedHomeBasedTour().ToFlag();
			var laterSimulatedHomeBasedTourFlag = personDay.IsLaterSimulatedHomeBasedTour().ToFlag();
			var totalStops = personDay.GetTotalStops();
			var totalSimulatedStops = personDay.GetTotalSimulatedStops();
			var escortStops = personDay.EscortStops;
			var homeBasedTours = personDay.HomeBasedTours;
			var simulatedHomeBasedTours = personDay.SimulatedHomeBasedTours;


			// tour inputs
			var escortTourFlag = tour.IsEscortPurpose().ToFlag();
			var shoppingTourFlag = tour.IsShoppingPurpose().ToFlag();
			var socialTourFlag = tour.IsSocialPurpose().ToFlag();
			var personalBusinessTourFlag = tour.IsPersonalBusinessPurpose().ToFlag();
			var workTourFlag = tour.IsWorkPurpose().ToFlag();
			var educationTourFlag = tour.IsSchoolPurpose().ToFlag();
			var businessTourFlag = tour.IsBusinessPurpose().ToFlag();

			var originParcel = tour.OriginParcel;
			var jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
			var partialHalfTour1Flag = (tour.PartialHalfTour1Sequence > 0) ? 1 : 0;
			var partialHalfTour2Flag = (tour.PartialHalfTour2Sequence > 0) ? 1 : 0;
			bool partialHalfTour = (tour.PartialHalfTour1Sequence > 0 || tour.PartialHalfTour2Sequence > 0);
			var fullHalfTour1Flag = (tour.FullHalfTour1Sequence > 0) ? 1 : 0;
			var fullHalfTour2Flag = (tour.FullHalfTour2Sequence > 0) ? 1 : 0;


			// remaining inputs
			// Higher priority tour of 2+ tours for the same purpose
			var highPrioritySameFlag = (tour.GetTotalToursByPurpose() > tour.GetTotalSimulatedToursByPurpose() && tour.GetTotalSimulatedToursByPurpose() == 1).ToFlag();

			// Lower priority tour(s) of 2+ tours for the same purpose
			var lowPrioritySameFlag = (tour.GetTotalSimulatedToursByPurpose() > 1).ToFlag();

			// Higher priority tour of 2+ tours for different purposes
			var highPriorityDifferentFlag = (personDay.IsFirstSimulatedHomeBasedTour() && personDay.HomeBasedToursExist()).ToFlag() * (1 - highPrioritySameFlag);

			// Lower priority tour of 2+ tours for different purposes
			var lowPriorityDifferentFlag = (personDay.IsLaterSimulatedHomeBasedTour() && personDay.HomeBasedToursExist()).ToFlag() * (1 - lowPrioritySameFlag);

			var timeWindow = tour.GetRelevantTimeWindow(householdDay);
			var totalMinutesAvailableInDay = timeWindow.TotalAvailableMinutes(1, 1440);
			if (totalMinutesAvailableInDay < 0) {
				if (!Global.Configuration.IsInEstimationMode) {
					householdDay.IsValid = false;
				}
				totalMinutesAvailableInDay = 0;
			}

			var maxAvailableMinutes =
				 (tour.JointTourSequence > 0 || tour.ParentTour == null)
				 ? timeWindow.MaxAvailableMinutesAfter(Global.Settings.Times.FiveAM)
					  : tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime;


			var householdHasChildren = household.HasChildren;
			var householdHasNoChildren = householdHasChildren ? false : true;

			var fastestAvailableTimeOfDay =
				 tour.IsHomeBasedTour || tour.ParentTour == null
					  ? 1
					  : tour.ParentTour.DestinationArrivalTime + (tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime) / 2;

			var tourCategory = tour.GetTourCategory();
			var secondaryFlag = ChoiceModelUtility.GetSecondaryFlag(tourCategory);
			var workOrSchoolPatternFlag = personDay.GetIsWorkOrSchoolPattern().ToFlag();
			var otherPatternFlag = personDay.GetIsOtherPattern().ToFlag();

			int bigPeriodCount = DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES;
			int nPeriodCombs = bigPeriodCount * (bigPeriodCount + 1) / 2;

			// tour time and mode components code copied from TourModeAndTimeModel

			int componentIndex = 0;
			int periodComb = -1;

			//set time components
			for (var arrivalPeriodIndex = 0; arrivalPeriodIndex < bigPeriodCount; arrivalPeriodIndex++) {
				var arrivalPeriod = DayPeriod.HBigDayPeriods[arrivalPeriodIndex];
				// JLB 201406 avoid unnecessary calculations in case of constrained time
				if (arrivalTimeIsConstrained && (constrainedArrivalTime < arrivalPeriod.Start || constrainedArrivalTime > arrivalPeriod.End)) {
					continue;
				}
				var arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

				for (var departurePeriodIndex = arrivalPeriodIndex; departurePeriodIndex < bigPeriodCount; departurePeriodIndex++) {
					var departurePeriod = DayPeriod.HBigDayPeriods[departurePeriodIndex];
					// JLB 201406 avoid unnecessary calculations in case of constrained time
					if (departureTimeIsConstrained && (constrainedDepartureTime < departurePeriod.Start || constrainedDepartureTime > departurePeriod.End)) {
						continue;
					}

					var departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);

					if (arrivalPeriod == departurePeriod) {

						componentIndex = arrivalPeriodIndex;
						choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
						var arrivalComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

						if (arrivalPeriodAvailableMinutes > 0) {
							var hoursArrival = arrivalPeriod.Middle / 60.0;
							var firstCoef = 200;
							arrivalComponent.AddUtilityTerm(firstCoef, Math.Log(arrivalPeriodAvailableMinutes));
							//arrival shift variables
							arrivalComponent.AddUtilityTerm(firstCoef + 2, partTimeWorkerFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 3, nonworkingAdultFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 4, universityStudentFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 5, retiredAdultFlag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 6, childAge5Through15Flag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 7, childUnder5Flag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 8, educationTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 9, hoursArrival);  //jlb 20140704 was escort purpose
							arrivalComponent.AddUtilityTerm(firstCoef + 10, shoppingTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 11, businessTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 12, personalBusinessTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 13, socialTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 14, workTourFlag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 15, primaryFamilyTimeFlag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 16, HHwithLowIncomeFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 17, HHwithMidleIncomeFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 18, HHwithHighIncomeFlag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 19, highPrioritySameFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 20, lowPrioritySameFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 21, highPriorityDifferentFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 22, lowPriorityDifferentFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 23, jointTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 24, partialHalfTour1Flag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 25, fullHalfTour1Flag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 26, partialHalfTour2Flag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 27, fullHalfTour2Flag * hoursArrival);

						}

						componentIndex = bigPeriodCount + departurePeriodIndex;
						choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
						var departureComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);


						if (departurePeriodAvailableMinutes > 0) {

							departureComponent.AddUtilityTerm(200, Math.Log(departurePeriodAvailableMinutes));
						}
					}
					// set period combination component
					periodComb++;
					componentIndex = 2 * bigPeriodCount + periodComb;
					choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
					var combinationComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

					if (arrivalPeriodAvailableMinutes > 0 && departurePeriodAvailableMinutes > 0) {
						var hoursDuration = (departurePeriod.Middle - arrivalPeriod.Middle) / 60.0;

						var firstCoef = 300;
						//combination constants
						combinationComponent.AddUtilityTerm(firstCoef + periodComb, 1.0);
						// duration shift variables
						combinationComponent.AddUtilityTerm(firstCoef + 30, primaryFamilyTimeFlag * hoursDuration);

						combinationComponent.AddUtilityTerm(firstCoef + 31, escortTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 32, shoppingTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 33, educationTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 34, socialTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 35, personalBusinessTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 36, businessTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 37, workTourFlag * hoursDuration);

						combinationComponent.AddUtilityTerm(firstCoef + 38, highPrioritySameFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 39, lowPrioritySameFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 40, highPriorityDifferentFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 41, lowPriorityDifferentFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 42, partTimeWorkerFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 43, jointTourFlag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 44, partialHalfTour1Flag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 45, fullHalfTour1Flag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 46, partialHalfTour2Flag * hoursDuration);
						combinationComponent.AddUtilityTerm(firstCoef + 47, fullHalfTour2Flag * hoursDuration);

						// peak-to-peak variables 
						if (arrivalPeriod.Index == DayPeriod.AM_PEAK && departurePeriod.Index == DayPeriod.PM_PEAK) {
							combinationComponent.AddUtilityTerm(firstCoef + 48, fullTimeWorkerFlag);
							combinationComponent.AddUtilityTerm(firstCoef + 49, partTimeWorkerFlag);
							combinationComponent.AddUtilityTerm(firstCoef + 50, maleFlag);
						}
					}
				}
			}

			//set mode components
			for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.Transit; mode++) {
				// JLB 201406 avoid unnecessary calculations in case of constrained mode
				if (modeIsConstrained && constrainedMode != mode) {
					continue;
				}
				var firstCoef = 400 + 20 * (mode - 1);
				componentIndex = 2 * bigPeriodCount + nPeriodCombs + mode - 1;
				choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
				var modeComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

				if (mode == Global.Settings.Modes.Transit) {
					modeComponent.AddUtilityTerm(firstCoef + 0, 1);
					modeComponent.AddUtilityTerm(firstCoef + 1, femaleFlag);
					modeComponent.AddUtilityTerm(firstCoef + 2, PTpass);

					modeComponent.AddUtilityTerm(firstCoef + 3, HHwithLowIncomeFlag);
					modeComponent.AddUtilityTerm(firstCoef + 4, HHwithMidleIncomeFlag);
					modeComponent.AddUtilityTerm(firstCoef + 5, HHwithHighIncomeFlag);

					modeComponent.AddUtilityTerm(firstCoef + 6, childrenUnder5);
					modeComponent.AddUtilityTerm(firstCoef + 7, childrenAge5Through15);
					modeComponent.AddUtilityTerm(firstCoef + 8, nonworkingAdults + retiredAdults);

					modeComponent.AddUtilityTerm(firstCoef + 9, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Hov3) {
					modeComponent.AddUtilityTerm(firstCoef + 0, 1);
					modeComponent.AddUtilityTerm(firstCoef + 1, childrenUnder5);
					modeComponent.AddUtilityTerm(firstCoef + 2, childrenAge5Through15);
					modeComponent.AddUtilityTerm(firstCoef + 4, nonworkingAdults + retiredAdults);
					modeComponent.AddUtilityTerm(firstCoef + 5, femaleFlag);
					modeComponent.AddUtilityTerm(firstCoef + 6, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(firstCoef + 7, carsLessThanDriversFlag);
					modeComponent.AddUtilityTerm(firstCoef + 8, onePersonHouseholdFlag);
					modeComponent.AddUtilityTerm(firstCoef + 9, twoPersonHouseholdFlag);
				}
				else if (mode == Global.Settings.Modes.Hov2) {
					modeComponent.AddUtilityTerm(firstCoef + 0, 1);
					modeComponent.AddUtilityTerm(firstCoef + 1, maleFlag);
					//GV: these are significant and plus
					modeComponent.AddUtilityTerm(firstCoef + 2, HHwithLowIncomeFlag);
					modeComponent.AddUtilityTerm(firstCoef + 3, HHwithMidleIncomeFlag);
					modeComponent.AddUtilityTerm(firstCoef + 4, HHwithHighIncomeFlag);
					modeComponent.AddUtilityTerm(firstCoef + 6, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(firstCoef + 7, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Sov) {
					modeComponent.AddUtilityTerm(firstCoef + 0, 1);
					modeComponent.AddUtilityTerm(firstCoef + 1, maleFlag);
					modeComponent.AddUtilityTerm(firstCoef + 2, fullTimeWorkerFlag);
					modeComponent.AddUtilityTerm(firstCoef + 3, partTimeWorkerFlag);
					modeComponent.AddUtilityTerm(firstCoef + 4, onePersonHouseholdFlag);
					modeComponent.AddUtilityTerm(firstCoef + 7, carsLessThanWorkersFlag);
				}
				else if (mode == Global.Settings.Modes.Bike) {
					modeComponent.AddUtilityTerm(firstCoef + 0, 1);
					modeComponent.AddUtilityTerm(firstCoef + 1, femaleFlag);
					modeComponent.AddUtilityTerm(firstCoef + 2, childrenUnder5);
					modeComponent.AddUtilityTerm(firstCoef + 3, childAge5Through15Flag);
					modeComponent.AddUtilityTerm(firstCoef + 4, fullTimeWorkerFlag);
					modeComponent.AddUtilityTerm(firstCoef + 5, partTimeWorkerFlag);
					modeComponent.AddUtilityTerm(firstCoef + 6, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(firstCoef + 7, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Walk) {
					modeComponent.AddUtilityTerm(firstCoef + 0, 1.0);
					modeComponent.AddUtilityTerm(firstCoef + 2, nonworkingAdults);
					modeComponent.AddUtilityTerm(firstCoef + 3, retiredAdults);
					modeComponent.AddUtilityTerm(firstCoef + 6, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(firstCoef + 7, carsLessThanDriversFlag);
				}

				//GV: Estimation of importance of "purpose" per mode - SOV is zero-alt and personal business is zero-alt 
				// Note:  work and school are not in this model
				if (mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.Hov2
					 || mode == Global.Settings.Modes.Hov3 || mode == Global.Settings.Modes.Transit) {
					modeComponent.AddUtilityTerm(firstCoef + 10, escortTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 11, shoppingTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 12, socialTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 13, businessTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 14, jointTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 15, Math.Min(partialHalfTour1Flag + partialHalfTour2Flag, 1.0));
					modeComponent.AddUtilityTerm(firstCoef + 16, Math.Min(fullHalfTour1Flag + fullHalfTour2Flag, 1.0));

				}
			}

			int destIndex = -1;
			// destination component code from TourDestinationModel

			var segment = Global.Kernel.Get<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(tour.DestinationPurpose, tour.IsHomeBasedTour ? Global.Settings.TourPriorities.HomeBasedTour : Global.Settings.TourPriorities.WorkBasedTour, Global.Settings.Modes.Sov, person.PersonType);
			if (destinationIsConstrained) sampleSize = 1;  // so that only the constrained destination ends up in the sample
			var chosenDestination = choice == null ? null : choice.Destination;
			var destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, tour.OriginParcel, chosenDestination);
			var tourDestinationUtilities = new TourDestinationUtilities(tour, sampleSize, secondaryFlag, personDay.GetIsWorkOrSchoolPattern().ToFlag(), personDay.GetIsOtherPattern().ToFlag(), fastestAvailableTimeOfDay, maxAvailableMinutes);
			// get destination sample and perform code that used to be in SetUtilities below
			var sampleItems = destinationSampler.SampleAndReturnTourDestinations(tourDestinationUtilities);
			// first loop on destinations to set destination component
			foreach (var sampleItem in sampleItems) {
				destIndex++;
				double adjustmentFactor = sampleItem.Key.AdjustmentFactor;
				var destinationParcel = ChoiceModelFactory.Parcels[sampleItem.Key.ParcelId];

				componentIndex = 2 * bigPeriodCount + nPeriodCombs + MODES_USED + destIndex;
				choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
				var destUtilityComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

				componentIndex = 2 * bigPeriodCount + nPeriodCombs + MODES_USED + sampleSize + destIndex;
				choiceProbabilityCalculator.CreateSizeComponent(componentIndex);
				var destSizeComponent = choiceProbabilityCalculator.GetSizeComponent(componentIndex);

				//var purpose = tour.TourPurposeSegment;
				var carOwnership = person.GetCarOwnershipSegment();
				var votSegment = tour.GetVotALSegment();
				var transitAccess = destinationParcel.TransitAccessSegment();
				//var aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][purpose][carOwnership][votSegment][transitAccess];
				var aggregateLogsumHomeBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][transitAccess];
				var aggregateLogsumWorkBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.WorkBased][carOwnership][votSegment][transitAccess];

				var distanceFromOrigin = tour.OriginParcel.DistanceFromOrigin(destinationParcel, tour.DestinationArrivalTime);


				// 1. new from GV: Cph KM-distances
				var piecewiseDistanceFrom0To1Km = Math.Min(distanceFromOrigin, .10);

				var piecewiseDistanceFrom0To2Km = Math.Min(distanceFromOrigin, .20); //GV: added July 7th
				var piecewiseDistanceFrom0To5Km = Math.Min(distanceFromOrigin, .50); //GV: added July 7th

				var piecewiseDistanceFrom1To2Km = Math.Max(0, Math.Min(distanceFromOrigin - .1, .2 - .1));
				var piecewiseDistanceFrom2To5Km = Math.Max(0, Math.Min(distanceFromOrigin - .2, .5 - .2));
				var piecewiseDistanceFrom5To10Km = Math.Max(0, Math.Min(distanceFromOrigin - .5, 1 - .5));
				var piecewiseDistanceFrom10To20Km = Math.Max(0, Math.Min(distanceFromOrigin - 1, 2 - 1));
				var piecewiseDistanceFrom20KmToInfinity = Math.Max(0, distanceFromOrigin - 2);

				var piecewiseDistanceFrom10KmToInfinity = Math.Max(0, distanceFromOrigin - 1);
				// 1. finished

				var distanceFromOriginLog = Math.Log(1 + distanceFromOrigin);
				var distanceFromWorkLog = person.UsualWorkParcel.DistanceFromWorkLog(destinationParcel, 1);
				var distanceFromSchoolLog = person.UsualSchoolParcel.DistanceFromSchoolLog(destinationParcel, 1);


				// 2. new from GV: Cph buffers for neighborhood effects
				// log transforms of buffers for Neighborhood effects
				var logOfOnePlusEducationK8Buffer2 = Math.Log(destinationParcel.StudentsK8Buffer2 + 1.0);
				var logOfOnePlusEducationUniStuBuffer2 = Math.Log(destinationParcel.StudentsUniversityBuffer2 + 1.0);
				var logOfOnePlusEmploymentEducationBuffer2 = Math.Log(destinationParcel.EmploymentEducationBuffer2 + 1.0);
				var logOfOnePlusEmploymentGovernmentBuffer2 = Math.Log(destinationParcel.EmploymentGovernmentBuffer2 + 1.0);
				var logOfOnePlusEmploymentIndustrialBuffer2 = Math.Log(destinationParcel.EmploymentIndustrialBuffer2 + 1.0);
				var logOfOnePlusEmploymentOfficeBuffer2 = Math.Log(destinationParcel.EmploymentOfficeBuffer2 + 1.0);
				var logOfOnePlusEmploymentRetailBuffer2 = Math.Log(destinationParcel.EmploymentRetailBuffer2 + 1.0);
				var logOfOnePlusEmploymentServiceBuffer2 = Math.Log(destinationParcel.EmploymentServiceBuffer2 + 1.0);
				var logOfOnePlusEmploymentAgrConstrBuffer2 = Math.Log(destinationParcel.EmploymentAgricultureConstructionBuffer2 + 1.0);
				var logOfOnePlusEmploymentJobsBuffer2 = Math.Log(destinationParcel.EmploymentTotalBuffer2 + 1.0);
				var logOfOnePlusHouseholdsBuffer2 = Math.Log(destinationParcel.HouseholdsBuffer2 + 1.0);
				// 2. finished 


				var logOfOnePlusParkingOffStreetDailySpacesBuffer1 = Math.Log(1 + destinationParcel.ParkingOffStreetPaidDailySpacesBuffer1);
				// connectivity attributes
				var c34Ratio = destinationParcel.C34RatioBuffer1();

				var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership); // exludes no cars
				var noCarCompetitionFlag = FlagUtility.GetNoCarCompetitionFlag(carOwnership);
				var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);


				destUtilityComponent.AddUtilityTerm(8, adjustmentFactor);

				// 3. new from GV: definition of Cph variables
				var firstBeta = 600;

				destUtilityComponent.AddUtilityTerm(firstBeta + 0, secondaryFlag * workOrSchoolPatternFlag * piecewiseDistanceFrom0To2Km); //GV: added July 7th               
				destUtilityComponent.AddUtilityTerm(firstBeta + 2, secondaryFlag * workOrSchoolPatternFlag * piecewiseDistanceFrom2To5Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 3, secondaryFlag * workOrSchoolPatternFlag * piecewiseDistanceFrom5To10Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 4, secondaryFlag * workOrSchoolPatternFlag * piecewiseDistanceFrom10To20Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 5, secondaryFlag * workOrSchoolPatternFlag * piecewiseDistanceFrom20KmToInfinity);

				//destComponent.AddUtilityTerm(266, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom0To1Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 6, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom0To2Km); //GV: added July 7th               
				//destComponent.AddUtilityTerm(267, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom1To2Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 8, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom2To5Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 9, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom5To10Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 10, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom10To20Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 11, secondaryFlag * otherPatternFlag * piecewiseDistanceFrom20KmToInfinity);

				destUtilityComponent.AddUtilityTerm(firstBeta + 12, (!tour.IsHomeBasedTour).ToFlag() * distanceFromOriginLog);
				destUtilityComponent.AddUtilityTerm(firstBeta + 13, (household.Income >= 300000 && household.Income < 600000).ToFlag() * distanceFromOriginLog);
				destUtilityComponent.AddUtilityTerm(firstBeta + 14, (household.Income >= 600000 && household.Income < 900000).ToFlag() * distanceFromOriginLog);
				destUtilityComponent.AddUtilityTerm(firstBeta + 15, (household.Income >= 900000).ToFlag() * distanceFromOriginLog);

				destUtilityComponent.AddUtilityTerm(firstBeta + 19, person.IsUniversityStudent.ToFlag() * distanceFromOriginLog);
				destUtilityComponent.AddUtilityTerm(firstBeta + 20, person.IsAdultMale.ToFlag() * distanceFromOriginLog);
				destUtilityComponent.AddUtilityTerm(firstBeta + 21, person.IsAdultFemale.ToFlag() * distanceFromOriginLog);
				destUtilityComponent.AddUtilityTerm(firstBeta + 22, person.IsRetiredAdult.ToFlag() * distanceFromOriginLog);

				destUtilityComponent.AddUtilityTerm(firstBeta + 24, (tour.IsHomeBasedTour).ToFlag() * distanceFromSchoolLog);

				// GV commented out this - on TO DO list
				//destComponent.AddUtilityTerm(277, (carCompetitionFlag + noCarCompetitionFlag) * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
				//destComponent.AddUtilityTerm(278, noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
				//destComponent.AddUtilityTerm(279, carCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
				//destComponent.AddUtilityTerm(280, noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
				//destComponent.AddUtilityTerm(281, noCarsFlag * c34Ratio);
				//destComponent.AddUtilityTerm(282, noCarCompetitionFlag * c34Ratio);
				//destComponent.AddUtilityTerm(283, (carCompetitionFlag + noCarCompetitionFlag) * logOfOnePlusParkingOffStreetDailySpacesBuffer1);

				destUtilityComponent.AddUtilityTerm(firstBeta + 26, jointTourFlag * piecewiseDistanceFrom0To2Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 27, jointTourFlag * piecewiseDistanceFrom2To5Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 28, jointTourFlag * piecewiseDistanceFrom5To10Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 29, jointTourFlag * piecewiseDistanceFrom10To20Km);
				destUtilityComponent.AddUtilityTerm(firstBeta + 30, jointTourFlag * piecewiseDistanceFrom20KmToInfinity);
				// 3. finished


				//4. new from GV: purpose utilities
				// COMPAS puposes are: Work, Education, Escort, Shopping, Leisure, Personal business, business
				// You need NO "Work" and "Education", their destinations are known in the synthetic population
				//firstBeta = firstBeta + 30 + 20 * (tour.DestinationPurpose - 1);
				//var firstGamma = 860 + 20 * (tour.DestinationPurpose - 1);
				firstBeta = firstBeta + 40;
				var firstPurposeBeta = 0;
				var firstGamma = 800;
				var firstPurposeGamma = 0;
				var purposeIndex = 0;
				if (tour.DestinationPurpose == Global.Settings.Purposes.Business) {
					purposeIndex = 0;
					firstPurposeBeta = firstBeta + 20 * purposeIndex;
					firstPurposeGamma = firstGamma + 10 * purposeIndex;

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 2, piecewiseDistanceFrom0To5Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 3, piecewiseDistanceFrom5To10Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 4, piecewiseDistanceFrom10To20Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 5, piecewiseDistanceFrom20KmToInfinity);

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 6, aggregateLogsumWorkBased);

					// Neighborhood
					//GV: commented out just temp.
					//destComponent.AddUtilityTerm(20, logOfOnePlusEducationK8Buffer2);
					//destComponent.AddUtilityTerm(21, logOfOnePlusEducationUniStuBuffer2);
					//destComponent.AddUtilityTerm(22, logOfOnePlusEmploymentEducationBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 13, logOfOnePlusEmploymentGovernmentBuffer2);
					//destComponent.AddUtilityTerm(24, logOfOnePlusEmploymentIndustrialBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 15, logOfOnePlusEmploymentOfficeBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 16, logOfOnePlusEmploymentRetailBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 17, logOfOnePlusEmploymentServiceBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 18, logOfOnePlusEmploymentAgrConstrBuffer2);
					//destComponent.AddUtilityTerm(29, logOfOnePlusEmploymentJobsBuffer2);

					// Size terms
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 0, destinationParcel.EmploymentEducation);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 1, destinationParcel.EmploymentGovernment);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 2, destinationParcel.EmploymentIndustrial);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 3, destinationParcel.EmploymentOffice);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 4, destinationParcel.EmploymentRetail);
					// GV: 35 is fixed to zero
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 5, destinationParcel.EmploymentService);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 6, destinationParcel.EmploymentAgricultureConstruction);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 7, destinationParcel.EmploymentTotal);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 8, destinationParcel.Households);
				}
				else if (tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
					purposeIndex = 1;
					firstPurposeBeta = firstBeta + 20 * purposeIndex;
					firstPurposeGamma = firstGamma + 10 * purposeIndex;

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 1, piecewiseDistanceFrom0To2Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 2, piecewiseDistanceFrom2To5Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 3, piecewiseDistanceFrom5To10Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 5, piecewiseDistanceFrom10KmToInfinity);

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 6, aggregateLogsumHomeBased);

					// Neighborhood
					//GV: commented out just temp.
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 10, householdHasNoChildren.ToFlag() * logOfOnePlusEmploymentJobsBuffer2);
					//destComponent.AddUtilityTerm(61, householdHasNoChildren.ToFlag() * logOfOnePlusHouseholdsBuffer2);
					//destComponent.AddUtilityTerm(62, householdHasChildren.ToFlag() * logOfOnePlusHouseholdsBuffer2);
					//destComponent.AddUtilityTerm(64, logOfOnePlusEmploymentJobsBuffer2);

					// Size terms
					// GV: no observations   
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 0, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentEducation);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 1, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentGovernment);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 2, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentIndustrial);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 3, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentOffice);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 4, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentRetail);
					// GV: 75 is fixed to zero
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 5, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentService);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 6, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentAgricultureConstruction);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 7, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentTotal);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 8, (!householdHasChildren).ToFlag() * destinationParcel.Households);

					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 10, (householdHasChildren).ToFlag() * destinationParcel.EmploymentEducation);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 11, (householdHasChildren).ToFlag() * destinationParcel.EmploymentGovernment);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 12, (householdHasChildren).ToFlag() * destinationParcel.EmploymentIndustrial);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 13, (householdHasChildren).ToFlag() * destinationParcel.EmploymentOffice);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 14, (householdHasChildren).ToFlag() * destinationParcel.EmploymentRetail);
					// GV 85 is fixed to zero at the moment
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 15, (householdHasChildren).ToFlag() * destinationParcel.EmploymentService);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 16, (householdHasChildren).ToFlag() * destinationParcel.EmploymentAgricultureConstruction);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 17, (householdHasChildren).ToFlag() * destinationParcel.EmploymentTotal);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 18, (householdHasChildren).ToFlag() * destinationParcel.Households);
				}
				else if (tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness) {
					purposeIndex = 3;
					firstPurposeBeta = firstBeta + 20 * purposeIndex;
					firstPurposeGamma = firstGamma + 10 * purposeIndex;

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 0, piecewiseDistanceFrom0To2Km);
					//destComponent.AddUtilityTerm(91, piecewiseDistanceFrom1To2Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 2, piecewiseDistanceFrom2To5Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 3, piecewiseDistanceFrom5To10Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 4, piecewiseDistanceFrom10To20Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 5, piecewiseDistanceFrom20KmToInfinity);

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 6, aggregateLogsumHomeBased);

					// Neighborhood
					//GV: commented out just temp.
					//destComponent.AddUtilityTerm(100, logOfOnePlusEmploymentEducationBuffer2);
					//destComponent.AddUtilityTerm(101, logOfOnePlusEmploymentGovernmentBuffer2);
					//destComponent.AddUtilityTerm(102, logOfOnePlusEmploymentIndustrialBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 13, logOfOnePlusEmploymentOfficeBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 14, logOfOnePlusEmploymentRetailBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 15, logOfOnePlusEmploymentServiceBuffer2);
					//destComponent.AddUtilityTerm(106, logOfOnePlusEmploymentAgrConstrBuffer2);
					//destComponent.AddUtilityTerm(107, logOfOnePlusEmploymentJobsBuffer2);

					// Size terms
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 0, destinationParcel.EmploymentEducation);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 1, destinationParcel.EmploymentGovernment);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 2, destinationParcel.EmploymentIndustrial);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 3, destinationParcel.EmploymentOffice);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 4, destinationParcel.EmploymentRetail);
					// GV 115 is fixed to zero
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 5, destinationParcel.EmploymentService);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 6, destinationParcel.EmploymentAgricultureConstruction);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 7, destinationParcel.EmploymentTotal);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 8, destinationParcel.Households);
				}
				else if (tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
					purposeIndex = 4;
					firstPurposeBeta = firstBeta + 20 * purposeIndex;
					firstPurposeGamma = firstGamma + 10 * purposeIndex;

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 1, piecewiseDistanceFrom0To2Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 2, piecewiseDistanceFrom2To5Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 3, piecewiseDistanceFrom5To10Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 4, piecewiseDistanceFrom10To20Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 5, piecewiseDistanceFrom20KmToInfinity);

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 6, aggregateLogsumHomeBased);

					// Neighborhood
					//GV: commented out just temp.
					//destComponent.AddUtilityTerm(130, logOfOnePlusEmploymentEducationBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 11, logOfOnePlusEmploymentRetailBuffer2);
					//destComponent.AddUtilityTerm(132, logOfOnePlusEmploymentJobsBuffer2);

					// Size terms
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 0, destinationParcel.EmploymentOffice);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 1, destinationParcel.EmploymentRetail);
					// GV 142 is fixed to zero
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 2, destinationParcel.EmploymentService);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 3, destinationParcel.EmploymentTotal);
				}
				else if (tour.DestinationPurpose == Global.Settings.Purposes.Social) {
					purposeIndex = 5;
					firstPurposeBeta = firstBeta + 20 * purposeIndex;
					firstPurposeGamma = firstGamma + 10 * purposeIndex;

					//destComponent.AddUtilityTerm(170, piecewiseDistanceFrom0To1Km);
					//destComponent.AddUtilityTerm(171, piecewiseDistanceFrom1To2Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 0, piecewiseDistanceFrom0To2Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 2, piecewiseDistanceFrom2To5Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 3, piecewiseDistanceFrom5To10Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 4, piecewiseDistanceFrom10To20Km);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 5, piecewiseDistanceFrom20KmToInfinity);

					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 6, aggregateLogsumHomeBased);

					// Neighborhood
					//GV: commented out just temp.
					//destComponent.AddUtilityTerm(180, logOfOnePlusEmploymentOfficeBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 11, logOfOnePlusEmploymentRetailBuffer2);
					destUtilityComponent.AddUtilityTerm(firstPurposeBeta + 12, logOfOnePlusEmploymentServiceBuffer2);
					//destComponent.AddUtilityTerm(183, logOfOnePlusEmploymentJobsBuffer2);

					// Size terms
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 0, destinationParcel.EmploymentEducation);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 1, destinationParcel.EmploymentGovernment);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 2, destinationParcel.EmploymentIndustrial);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 3, destinationParcel.EmploymentOffice);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 4, destinationParcel.EmploymentRetail);
					// GV 195 is fixed to zero
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 5, destinationParcel.EmploymentService);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 6, destinationParcel.EmploymentAgricultureConstruction);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 7, destinationParcel.EmploymentTotal);
					destSizeComponent.AddUtilityTerm(firstPurposeGamma + 8, destinationParcel.Households);
				}
			}
			//	}

			// may want to insert additional two-way interaction components such as mode-destination
			// second loop on destinations to handle all elemental alternatives
			destIndex = -1;
			foreach (var sampleItem in sampleItems) {
				destIndex++;
				bool destAvailable = sampleItem.Key.Available;
				bool destIsChosen = sampleItem.Key.IsChosen;
				double adjustmentFactor = sampleItem.Key.AdjustmentFactor;
				var destinationParcel = ChoiceModelFactory.Parcels[sampleItem.Key.ParcelId];

				//if (isChosen) Global.PrintFile.WriteLine("Sequence {0}: Chosen parcel {1} Available {2} Sample item {3} of {4}", timesStartedRunModel, destinationParcel.Id, available, index, sampleItems.Count);

				if (constrainedParcel != null && constrainedParcel != destinationParcel) {
					destAvailable = false;
				}

				//if (!destAvailable && !Global.Configuration.IsInEstimationMode) {
				//	continue;
				//}
				// set tour destination parcel for within this loop in order to set other variables
				HTourModeTime.SetModeTimeImpedances(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime, constrainedHouseholdCars, constrainedTransitDiscountFraction, destinationParcel);

				//loop on all mode and time alternatives, using modeTimes objects
				foreach (var modeTimes in HTourModeTime.ModeTimes[ParallelUtility.threadLocalAssignedIndex.Value]) {
					var arrivalPeriod = modeTimes.ArrivalPeriod;
					var arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

					var departurePeriod = modeTimes.DeparturePeriod;
					var departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);
					periodComb = modeTimes.PeriodCombinationIndex;

					var mode = modeTimes.Mode;
					var modeTimeIndex = modeTimes.Index;
					var altIndex = destIndex * HTourModeTime.TotalTourModeTimes + modeTimeIndex;

					//skip unhandled modes
					if (mode > maxMode) {
						continue;
					}
					//set availabillity based on time window variables and any constrained choices
					bool modeAvailable = modeTimes.LongestFeasibleWindow != null
						&& (mode > 0)
						&& (mode <= Global.Settings.Modes.Transit)
						&& (person.Age >= 18 || (modeTimes.Mode != Global.Settings.Modes.Sov && modeTimes.Mode != Global.Settings.Modes.HovDriver))
						&& (constrainedMode > 0 || mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.HovDriver || mode == Global.Settings.Modes.Transit || !partialHalfTour)
						;

					var alternative = choiceProbabilityCalculator.GetAlternative(altIndex, destAvailable && modeAvailable,
																									 choice != null && choice.Destination == destinationParcel && choice.ModeTimes.Index == modeTimeIndex);

					var altChoice = new HTourDestinationModeTime(destinationParcel, modeTimes);
					alternative.Choice = altChoice;

					//NEST OPTIONS.  CHOOSE AND UNCOMMENT ONLY ! OPTION

					////Nest option:  MNL of joint DestModeTime
					//// no nesting

					////Nest option:  Dest --> ModeTime
					////only level of nesting - modetimes under dests
					//alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + destIndex + 1, destIndex, THETA_PARAMETER_1);

					////Nest option:  Dest --> Mode --> TIme
					////first level of nesting - period combinations under modes
					//alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + mode, mode - 1, THETA_PARAMETER_1);
					//// second level of nesting - modes and times under destinations 
					//var nestAlternative = choiceProbabilityCalculator.GetAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + mode, true, false);
					//nestAlternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + MODES_USED + destIndex + 1, destIndex, THETA_PARAMETER_2);

					////Nest option:  Dest --> Time --> Mode
					////first level of nesting - modes under period combinations
					//alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + periodComb + 1, periodComb, THETA_PARAMETER_1);
					//// second level of nesting - modes and times under destinations 
					//var nestAlternative = choiceProbabilityCalculator.GetAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + periodComb + 1, true, false);
					//nestAlternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIME_COMBINATIONS + destIndex + 1, destIndex, THETA_PARAMETER_2);

					////Nest option:  ModeTime --> Dest
					////only level of nesting - dests under modetimes
					//alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + modeTimeIndex + 1, modeTimeIndex, THETA_PARAMETER_1);

					////Nest option:  Mode --> Time --> Dest
					////first level of nesting - dest under times
					//alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + periodComb + 1, periodComb, THETA_PARAMETER_1);
					//// second level of nesting - dests and times under modes 
					//var nestAlternative = choiceProbabilityCalculator.GetAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + periodComb + 1, true, false);
					//nestAlternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIME_COMBINATIONS + mode, mode - 1, THETA_PARAMETER_2);

					////Nest option:  Time --> Mode --> Dest
					////first level of nesting - dest under modes
					//alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + mode, mode - 1, THETA_PARAMETER_1);
					//// second level of nesting - modes and times under destinations 
					//var nestAlternative = choiceProbabilityCalculator.GetAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + mode, true, false);
					//nestAlternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes * sampleSize + MODES_USED + periodComb + 1, periodComb, THETA_PARAMETER_2);



					if (Global.Configuration.IsInEstimationMode && choice.Destination == destinationParcel && choice.ModeTimes.Index == modeTimeIndex) {
						Global.PrintFile.WriteLine("Aper Dper Mode {0} {1} {2} Travel Times {3} {4} Window {5} {6}",
															arrivalPeriod.Index, departurePeriod.Index, mode,
															modeTimes.ModeAvailableToDestination ? modeTimes.TravelTimeToDestination : -1,
															modeTimes.ModeAvailableFromDestination ? modeTimes.TravelTimeFromDestination : -1,
															modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.Start : -1,
															modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.End : -1);

					}
					//Following code was used to test handling of partially joint half tours (JLB 20140603)
					//if (partialHalfTour) {
					//	Global.PrintFile.WriteLine("HH pers {0} {1} avail {2} Aper Dper Mode {3} {4} {5} Travel Times {6} {7} Window {8} {9}",
					//	   household.Id, person.Sequence,  
					//    available,  
					//		arrivalPeriod.Index, departurePeriod.Index, mode, 
					//	                           modeTimes.ModeAvailableToDestination ? modeTimes.TravelTimeToDestination : -1,
					//	                           modeTimes.ModeAvailableFromDestination ? modeTimes.TravelTimeFromDestination : -1,
					//	                           modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.Start : -1,
					//	                           modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.End : -1);
					//}

					//if in application mode and combination is not available, can skip the rest
					if (!Global.Configuration.IsInEstimationMode && !alternative.Available) {
						continue;
					}
					// arrival period utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(arrivalPeriod.Index));

					// departure period utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(bigPeriodCount + departurePeriod.Index));

					// period combination utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(2 * bigPeriodCount + periodComb));

					// mode utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(2 * bigPeriodCount + nPeriodCombs + mode - 1));

					// destination utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(2 * bigPeriodCount + nPeriodCombs + MODES_USED + destIndex));

					// destination size component
					alternative.AddSizeComponent(
						choiceProbabilityCalculator.GetSizeComponent(2 * bigPeriodCount + nPeriodCombs + MODES_USED + sampleSize + destIndex));

					//even in estimation mode, do not need the rest of the code if not available
					if (!alternative.Available) {
						continue;
					}

					//GV and JB: the parking cost are handled as part of genaralised time

					var minimumTimeNeeded = modeTimes.TravelTimeToDestination + modeTimes.TravelTimeFromDestination + Global.Settings.Times.MinimumActivityDuration;

					alternative.AddUtilityTerm(1, modeTimes.GeneralizedTimeToDestination + modeTimes.GeneralizedTimeFromDestination);

					alternative.AddUtilityTerm(3,
														Math.Log(modeTimes.LongestFeasibleWindow.End - modeTimes.LongestFeasibleWindow.Start -
																	minimumTimeNeeded + 1.0));
					alternative.AddUtilityTerm(4, Math.Log((totalMinutesAvailableInDay + 1.0) / (minimumTimeNeeded + 1.0)));

					alternative.AddUtilityTerm(5,
														(maleFlag == 0 && mode == Global.Settings.Modes.Walk &&
														 arrivalPeriod.Index >= DayPeriod.EVENING)
															? 1
															: 0);
				}
			}
		}

		private sealed class TourDestinationUtilities : ISamplingUtilities {
			private readonly TourWrapper _tour;
			private readonly int _secondaryFlag;
			private readonly int _workOrSchoolPatternFlag;
			private readonly int _otherPatternFlag;
			private readonly int _fastestAvailableTimeOfDay;
			private readonly int _maxAvailableMinutes;
			private readonly int[] _seedValues;

			public TourDestinationUtilities(TourWrapper tour, int sampleSize, int secondaryFlag, int workOrSchoolPatternFlag, int otherPatternFlag, int fastestAvailableTimeOfDay, int maxAvailableMinutes) {
				_tour = tour;
				_secondaryFlag = secondaryFlag;
				_workOrSchoolPatternFlag = workOrSchoolPatternFlag;
				_otherPatternFlag = otherPatternFlag;
				_fastestAvailableTimeOfDay = fastestAvailableTimeOfDay;
				_maxAvailableMinutes = maxAvailableMinutes;
				_seedValues = ChoiceModelUtility.GetRandomSampling(sampleSize, tour.Person.SeedValues[20 + tour.Sequence - 1]);
			}

			public int[] SeedValues {
				get { return _seedValues; }
			}

			public void SetUtilities(ISampleItem sampleItem, int sampleFrequency) {
				if (sampleItem == null) {
					throw new ArgumentNullException("sampleItem");
				}

			}

			public static bool ShouldRunInEstimationModeForModel(DomainModels.Default.Wrappers.TourWrapper tour) {
				// determine validity and need, then characteristics
				// detect and skip invalid trip records (error = true) and those that trips that don't require stop location choice (need = false)
				var excludeReason = 0;

				//				if (_maxZone == -1) {
				//					// TODO: Verify / Optimize
				//					_maxZone = ChoiceModelRunner.ZoneKeys.Max(z => z.Key);
				//				}
				//
				//				if (_maxParcel == -1) {
				//					// TODO: Optimize
				//					_maxParcel = ChoiceModelRunner.Parcels.Values.Max(parcel => parcel.Id);
				//				}

				if (Global.Configuration.IsInEstimationMode) {
					//					if (tour.OriginParcelId > _maxParcel) {
					//						excludeReason = 3;
					//					}

					if (tour.OriginParcelId <= 0) {
						excludeReason = 4;
					}
					//					else if (tour.DestinationAddressType > _maxParcel) {
					//						excludeReason = 5;
					//					}
					else if (tour.DestinationParcelId <= 0) {
						excludeReason = 6;
						tour.DestinationParcelId = tour.OriginParcelId;
						tour.DestinationParcel = tour.OriginParcel;
						tour.DestinationZoneKey = tour.OriginParcelId;
					}
					//					else if (tour.OriginParcelId > _maxParcel) {
					//						excludeReason = 7;
					//					}
					//					else if (tour.OriginParcelId <= 0) {
					//						excludeReason = 8;
					//					}
					//JLB 20130705 dropp following screen for LD
					//else if (tour.OriginParcelId == tour.DestinationParcelId) {
					//	excludeReason = 9;
					//}
					else if (tour.OriginParcel.ZoneId == -1) {
						// TODO: Verify this condition... it used to check that the zone was == null. 
						// I'm not sure what the appropriate condition should be though.

						excludeReason = 10;
					}

					if (excludeReason > 0) {
						Global.PrintFile.WriteEstimationRecordExclusionMessage(CHOICE_MODEL_NAME, "ShouldRunInEstimationModeForModel", tour.Household.Id, tour.Person.Sequence, 0, tour.Sequence, 0, 0, excludeReason);
					}
				}

				var shouldRun = (excludeReason == 0);

				return shouldRun;
			}
		}
	}
}