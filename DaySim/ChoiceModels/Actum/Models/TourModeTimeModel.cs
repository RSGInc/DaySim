// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels;
using DaySim.DomainModels.Actum;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using HouseholdDayWrapper = DaySim.DomainModels.Actum.Wrappers.HouseholdDayWrapper;
using HouseholdWrapper = DaySim.DomainModels.Default.Wrappers.HouseholdWrapper;
using TourWrapper = DaySim.DomainModels.Actum.Wrappers.TourWrapper;

namespace DaySim.ChoiceModels.Actum.Models {
	public class TourModeTimeModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "ActumTourModeTimeModel";
		private const int TOTAL_NESTED_ALTERNATIVES = 8;
		private const int TOTAL_LEVELS = 2;
		private const int MAX_PARAMETER = 999;
		private const int THETA_PARAMETER = 900;

		public override void RunInitialize(ICoefficientsReader reader = null) {
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.TourModeTimeModelCoefficients, HTourModeTime.TotalTourModeTimes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
		}

		public void Run(HouseholdDayWrapper householdDay, TourWrapper tour,
			int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime) {
			if (tour == null) {
				throw new ArgumentNullException("tour");
			}

			//HTourModeTime.InitializeTourModeTimes();

			tour.PersonDay.ResetRandom(50 + tour.Sequence - 1);

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
				// JLB 20140421 add the following to keep from estimatign twice for the same tour
				if (tour.DestinationModeAndTimeHaveBeenSimulated) {
					return;
				}
				if (tour.DestinationParcel == null || tour.OriginParcel == null || tour.Mode < Global.Settings.Modes.Walk || tour.Mode > Global.Settings.Modes.SchoolBus) {
					return;
				}
			}

			// set remaining inputs

			HTourModeTime.SetModeTimeImpedances(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime, -1, -1.0);


			var choiceProbabilityCalculator = _helpers[ParallelUtility.GetBatchFromThreadId()].GetChoiceProbabilityCalculator(tour.Id);

			if (_helpers[ParallelUtility.GetBatchFromThreadId()].ModelIsInEstimationMode) {

				var observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

				RunModel(choiceProbabilityCalculator, householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime,
					observedChoice);

				choiceProbabilityCalculator.WriteObservation();

			}
			else if (Global.Configuration.TestEstimationModelInApplicationMode) {
				Global.Configuration.IsInEstimationMode = false;

				RunModel(choiceProbabilityCalculator, householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);

				var observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

				var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility, tour.Id, observedChoice.Index);

				Global.Configuration.IsInEstimationMode = true;
			}
			else {
				HTourModeTime choice;

				if (constrainedMode > 0 && constrainedArrivalTime > 0 && constrainedDepartureTime > 0) {
					choice = new HTourModeTime(constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
				}
				else {
					RunModel(choiceProbabilityCalculator, householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
					var simulatedChoice = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

					if (simulatedChoice == null) {
						Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
						if (!Global.Configuration.IsInEstimationMode) {
							tour.PersonDay.IsValid = false;
							tour.PersonDay.HouseholdDay.IsValid = false;
						}
						return;
					}
					choice = (HTourModeTime) simulatedChoice.Choice;
				}

				tour.Mode = choice.Mode;
				var arrivalPeriod = choice.ArrivalPeriod;
				var departurePeriod = choice.DeparturePeriod;
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
					var departureTime = Math.Max(choice.GetRandomDepartureTime(householdDay, tour), departurePeriod.Start + Global.Settings.Times.MinimumActivityDuration);
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

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, TourWrapper tour,
					int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime, HTourModeTime choice = null) {

			var household = tour.Household;
			var person = tour.Person;
			var personDay = tour.PersonDay;
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
			var destinationParcel = tour.DestinationParcel;
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

			int bigPeriodCount = DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES;
			int nPeriodCombs = bigPeriodCount * (bigPeriodCount + 1) / 2;

			bool useTimeComponents = Global.Configuration.IsInEstimationMode || constrainedArrivalTime==0 || constrainedDepartureTime==0;
			int componentIndex = 0;
			int periodComb = -1;
			//set components
			if (useTimeComponents) {
			for (var arrivalPeriodIndex = 0; arrivalPeriodIndex < bigPeriodCount; arrivalPeriodIndex++) {
				var arrivalPeriod = DayPeriod.HBigDayPeriods[arrivalPeriodIndex];
				var arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

				for (var departurePeriodIndex = arrivalPeriodIndex; departurePeriodIndex < bigPeriodCount; departurePeriodIndex++) {
					var departurePeriod = DayPeriod.HBigDayPeriods[departurePeriodIndex];
					var departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);

					if (arrivalPeriod == departurePeriod) {

						componentIndex = arrivalPeriodIndex;
						choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
						var arrivalComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

						if (arrivalPeriodAvailableMinutes > 0) {
							var hoursArrival = arrivalPeriod.Middle / 60.0;
							var firstCoef = 300;
							arrivalComponent.AddUtilityTerm(300, Math.Log(arrivalPeriodAvailableMinutes));
							//arrival shift variables
							arrivalComponent.AddUtilityTerm(firstCoef + 2, partTimeWorkerFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 3, nonworkingAdultFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 4, universityStudentFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 5, retiredAdultFlag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 6, childAge5Through15Flag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 7, childUnder5Flag * hoursArrival);

							arrivalComponent.AddUtilityTerm(firstCoef + 8, educationTourFlag * hoursArrival);
							arrivalComponent.AddUtilityTerm(firstCoef + 9, escortTourFlag * hoursArrival);
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

							departureComponent.AddUtilityTerm(300, Math.Log(departurePeriodAvailableMinutes));
						}
					}
					// set period combination component
					periodComb++;
					componentIndex = 2 * bigPeriodCount + periodComb;
					choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
					var combinationComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

					if (arrivalPeriodAvailableMinutes > 0 && departurePeriodAvailableMinutes > 0) {
						var hoursDuration = (departurePeriod.Middle - arrivalPeriod.Middle) / 60.0;

						var firstCoef = 700;
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
			}
			for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.SchoolBus; mode++) {
				componentIndex = 2 * bigPeriodCount + nPeriodCombs + mode - 1;
				choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
				var modeComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

				if (mode == Global.Settings.Modes.SchoolBus) {
					modeComponent.AddUtilityTerm(10, 1);
				}
				else if (mode == Global.Settings.Modes.ParkAndRide) {
					modeComponent.AddUtilityTerm(10, 1);
				}
				else if (mode == Global.Settings.Modes.Transit) {
					modeComponent.AddUtilityTerm(20, 1);
					modeComponent.AddUtilityTerm(21, femaleFlag);
					//modeComponent.AddUtilityTerm(22, retiredAdultFlag);

					modeComponent.AddUtilityTerm(22, PTpass);

					modeComponent.AddUtilityTerm(23, HHwithLowIncomeFlag);
					modeComponent.AddUtilityTerm(24, HHwithMidleIncomeFlag);
					modeComponent.AddUtilityTerm(25, HHwithHighIncomeFlag);

					modeComponent.AddUtilityTerm(26, childrenUnder5);
					modeComponent.AddUtilityTerm(27, childrenAge5Through15);
					modeComponent.AddUtilityTerm(28, nonworkingAdults + retiredAdults);

					//modeComponent.AddUtilityTerm(26, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(29, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Hov3) {
					modeComponent.AddUtilityTerm(30, 1);
					modeComponent.AddUtilityTerm(31, childrenUnder5);
					modeComponent.AddUtilityTerm(32, childrenAge5Through15);
					modeComponent.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
					modeComponent.AddUtilityTerm(35, femaleFlag);

					modeComponent.AddUtilityTerm(38, onePersonHouseholdFlag);
					modeComponent.AddUtilityTerm(39, twoPersonHouseholdFlag);
					modeComponent.AddUtilityTerm(36, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(37, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Hov2) {
					modeComponent.AddUtilityTerm(40, 1);
					modeComponent.AddUtilityTerm(41, maleFlag);
					//modeComponent.AddUtilityTerm(41, onePersonHouseholdFlag);

					//modeComponent.AddUtilityTerm(42, childrenUnder5);
					//modeComponent.AddUtilityTerm(43, childrenAge5Through15);
					//modeComponent.AddUtilityTerm(44, nonworkingAdults + retiredAdults);

					//GV: these are significant and plus
					modeComponent.AddUtilityTerm(42, HHwithLowIncomeFlag);
					modeComponent.AddUtilityTerm(43, HHwithMidleIncomeFlag);
					modeComponent.AddUtilityTerm(44, HHwithHighIncomeFlag);

					modeComponent.AddUtilityTerm(46, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(47, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Sov) {
					modeComponent.AddUtilityTerm(50, 1);
					modeComponent.AddUtilityTerm(51, maleFlag);
					modeComponent.AddUtilityTerm(52, fullTimeWorkerFlag);
					modeComponent.AddUtilityTerm(53, partTimeWorkerFlag);
					modeComponent.AddUtilityTerm(54, onePersonHouseholdFlag);

					//GV: these are NOT significant 
					//modeComponent.AddUtilityTerm(53, HHwithLowIncomeFlag);
					//modeComponent.AddUtilityTerm(54, HHwithMidleIncomeFlag);
					//modeComponent.AddUtilityTerm(55, HHwithHighIncomeFlag);

					modeComponent.AddUtilityTerm(57, carsLessThanWorkersFlag);
				}
				else if (mode == Global.Settings.Modes.Bike) {
					modeComponent.AddUtilityTerm(60, 1);
					modeComponent.AddUtilityTerm(61, femaleFlag);
					modeComponent.AddUtilityTerm(62, childrenUnder5);
					modeComponent.AddUtilityTerm(63, childAge5Through15Flag);

					modeComponent.AddUtilityTerm(64, fullTimeWorkerFlag);
					modeComponent.AddUtilityTerm(65, partTimeWorkerFlag);

					modeComponent.AddUtilityTerm(66, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(67, carsLessThanDriversFlag);
				}
				else if (mode == Global.Settings.Modes.Walk) {
					modeComponent.AddUtilityTerm(70, 1.0);
					//modeComponent.AddUtilityTerm(71, femaleFlag);
					modeComponent.AddUtilityTerm(72, nonworkingAdults);
					modeComponent.AddUtilityTerm(73, retiredAdults);

					modeComponent.AddUtilityTerm(76, noCarsInHouseholdFlag);
					modeComponent.AddUtilityTerm(77, carsLessThanDriversFlag);
				}

				//GV: Estimation of importance of "purpose" per mode - SOV is zero-alt and Work is zero-alt 
				if (mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.Hov2
					 || mode == Global.Settings.Modes.Hov3 || mode == Global.Settings.Modes.Transit) {
					var firstCoef = 200 + 10 * mode;

					modeComponent.AddUtilityTerm(firstCoef + 0, escortTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 1, shoppingTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 2, educationTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 3, socialTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 4, personalBusinessTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 5, businessTourFlag);
					//modeComponent.AddUtilityTerm(firstCoef + 6, workTourFlag); //GV: "work" is zero alternative

					modeComponent.AddUtilityTerm(firstCoef + 7, jointTourFlag);
					modeComponent.AddUtilityTerm(firstCoef + 8, Math.Min(partialHalfTour1Flag + partialHalfTour2Flag, 1.0));
					modeComponent.AddUtilityTerm(firstCoef + 9, Math.Min(fullHalfTour1Flag + fullHalfTour2Flag, 1.0));

				}
			}



			//loop on all alternatives, using modeTimes objects
			{
				foreach (var modeTimes in HTourModeTime.ModeTimes[ParallelUtility.GetBatchFromThreadId()])
				{
					var arrivalPeriod = modeTimes.ArrivalPeriod;
					var arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

					var departurePeriod = modeTimes.DeparturePeriod;
					var departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);
					periodComb = modeTimes.PeriodCombinationIndex;

					var mode = modeTimes.Mode;

					var altIndex = modeTimes.Index;


				//set availabillity based on time window variables and any constrained choices
				bool available = modeTimes.LongestFeasibleWindow != null
					&& (mode > 0)
					&& (mode <= Global.Settings.Modes.Transit)
					&& (person.Age >= 18 || (modeTimes.Mode != Global.Settings.Modes.Sov && modeTimes.Mode != Global.Settings.Modes.HovDriver))
					&& (constrainedMode > 0 || mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.HovDriver || mode == Global.Settings.Modes.Transit || !partialHalfTour) 
					;

					var alternative = choiceProbabilityCalculator.GetAlternative(altIndex, available,
					                                                             choice != null && choice.Index == altIndex);

					alternative.Choice = modeTimes; // JLB added 20130420

					//alternative.AddNestedAlternative(HTourModeTime.TOTAL_TOUR_MODE_TIMES + periodComb + 1, periodComb, THETA_PARAMETER);
					alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes + mode, mode - 1, THETA_PARAMETER);

					if (Global.Configuration.IsInEstimationMode && altIndex == choice.Index)
					{
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
					if (!Global.Configuration.IsInEstimationMode && !alternative.Available)
					{
						continue;
					}
					if (useTimeComponents) {
					// arrival period utility component
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(arrivalPeriod.Index));

					// departure period utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(bigPeriodCount + departurePeriod.Index));

					// period combination utility component
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(2*bigPeriodCount + periodComb));
					}

					// mode utility component
					alternative.AddUtilityComponent(
						choiceProbabilityCalculator.GetUtilityComponent(2*bigPeriodCount + nPeriodCombs + mode - 1));

					//even in estimation mode, do not need the rest of the code if not available
					if (!alternative.Available)
					{
						continue;
					}

				//GV and JB: the parking cost are handled as part of genaralised time

				var minimumTimeNeeded = modeTimes.TravelTimeToDestination + modeTimes.TravelTimeFromDestination + Global.Settings.Times.MinimumActivityDuration;

					alternative.AddUtilityTerm(1, modeTimes.GeneralizedTimeToDestination + modeTimes.GeneralizedTimeFromDestination);

					alternative.AddUtilityTerm(3,
					                           Math.Log(modeTimes.LongestFeasibleWindow.End - modeTimes.LongestFeasibleWindow.Start -
					                                    minimumTimeNeeded + 1.0));
					alternative.AddUtilityTerm(4, Math.Log((totalMinutesAvailableInDay + 1.0)/(minimumTimeNeeded + 1.0)));

					alternative.AddUtilityTerm(5,
					                           (maleFlag == 0 && mode == Global.Settings.Modes.Walk &&
					                            arrivalPeriod.Index >= DayPeriod.EVENING)
						                           ? 1
						                           : 0);

					if (altIndex == 0)
					{
						alternative.AddUtilityTerm(998, tour.DestinationPurpose);
						alternative.AddUtilityTerm(999, (tour.ParentTour == null) ? 0 : 1);
					}
				}

			}

		}
	}
}