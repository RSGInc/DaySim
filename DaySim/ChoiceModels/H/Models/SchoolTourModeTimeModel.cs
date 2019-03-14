﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.ChoiceModels.H.Models {
  public class SchoolTourModeTimeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "HSchoolTourModeTimeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 999;
    private const int THETA_PARAMETER = 900;

    private readonly ITourCreator _creator =
        Global
        .ContainerDaySim.GetInstance<IWrapperFactory<ITourCreator>>()
        .Creator;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.SchoolTourModeTimeModelCoefficients, HTourModeTime.TotalTourModeTimes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(IHouseholdDayWrapper householdDay, ITourWrapper tour,
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

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        HTourModeTime observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

        RunModel(choiceProbabilityCalculator, householdDay, tour, tour.DestinationParcel, tour.Household.VehiclesAvailable,
            constrainedMode, constrainedArrivalTime, constrainedDepartureTime,
            observedChoice);

        choiceProbabilityCalculator.WriteObservation();

      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, householdDay, tour, tour.DestinationParcel, tour.Household.VehiclesAvailable,
            constrainedMode, constrainedArrivalTime, constrainedDepartureTime);

        HTourModeTime observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

        ChoiceProbabilityCalculator.Alternative simulatedChoice = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility, tour.Id, observedChoice.Index);

        Global.Configuration.IsInEstimationMode = true;
      } else {
        HTourModeTime choice;

        if (constrainedMode > 0 && constrainedArrivalTime > 0 && constrainedDepartureTime > 0) {
          choice = new HTourModeTime(constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else {
          RunModel(choiceProbabilityCalculator, householdDay, tour, tour.DestinationParcel, tour.Household.VehiclesAvailable,
              constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
          ChoiceProbabilityCalculator.Alternative simulatedChoice = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

          if (simulatedChoice == null) {
            Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
            if (!Global.Configuration.IsInEstimationMode) {
              tour.PersonDay.IsValid = false;
              tour.PersonDay.HouseholdDay.IsValid = false;
            }
            return;
          }
          choice = (HTourModeTime)simulatedChoice.Choice;
        }

        tour.Mode = choice.Mode;
        MinuteSpan arrivalPeriod = choice.ArrivalPeriod;
        MinuteSpan departurePeriod = choice.DeparturePeriod;
        //use constrained times to set temporary arrival and departure times with minimum duration of stay for time window calculations
        if (constrainedArrivalTime > 0 || constrainedDepartureTime > 0) {
          if (constrainedArrivalTime > 0) {
            tour.DestinationArrivalTime = constrainedArrivalTime;
          } else {
            tour.DestinationArrivalTime = Math.Min(arrivalPeriod.End, constrainedDepartureTime - Global.Settings.Times.MinimumActivityDuration);
          }
          if (constrainedDepartureTime > 0) {
            tour.DestinationDepartureTime = constrainedDepartureTime;
          } else {
            tour.DestinationDepartureTime = Math.Max(departurePeriod.Start, constrainedArrivalTime + Global.Settings.Times.MinimumActivityDuration);
          }
        }
        //or if times aren't constrained use periods to set temporary arrival and departure times with minimum duration of stay for time window calculations 
        else if (arrivalPeriod == departurePeriod) {
          int departureTime = Math.Max(choice.GetRandomDepartureTime(householdDay, tour), departurePeriod.Start + Global.Settings.Times.MinimumActivityDuration);
          tour.DestinationArrivalTime = departureTime - Global.Settings.Times.MinimumActivityDuration;
          tour.DestinationDepartureTime = departureTime;
        } else if (arrivalPeriod.End == departurePeriod.Start - 1) {
          tour.DestinationArrivalTime = arrivalPeriod.End;
          tour.DestinationDepartureTime = arrivalPeriod.End + Global.Settings.Times.MinimumActivityDuration;
        } else {
          tour.DestinationArrivalTime = arrivalPeriod.End;
          tour.DestinationDepartureTime = departurePeriod.Start;
        }


      }
    }

    //MBADD runnested
    public ChoiceProbabilityCalculator.Alternative RunNested(IPersonWrapper person, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars, double transitDiscountFraction) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      ITourWrapper tour = _creator.CreateWrapper(person, null, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);

      return RunNested(tour, destinationParcel, householdCars, transitDiscountFraction);
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(IPersonDayWrapper personDay, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      ITourWrapper tour = _creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);

      return RunNested(tour, destinationParcel, householdCars, personDay.Person.GetTransitFareDiscountFraction());
    }

    private ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel, int householdCars, double transitDiscountFraction) {
      if (Global.Configuration.AvoidDisaggregateModeChoiceLogsums) {
        return null;
      }
      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();


      IHouseholdDayWrapper householdDay = (tour.PersonDay == null) ? null : tour.PersonDay.HouseholdDay;

      int constrainedMode = 0;
      int constrainedArrivalTime = (Global.Configuration.ConstrainTimesForModeChoiceLogsums) ? tour.DestinationArrivalTime : 0;
      int constrainedDepartureTime = (Global.Configuration.ConstrainTimesForModeChoiceLogsums) ? tour.DestinationDepartureTime : 0;

      tour.DestinationParcel = destinationParcel;
      HTourModeTime.SetModeTimeImpedances(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime, householdCars, transitDiscountFraction);


      RunModel(choiceProbabilityCalculator, householdDay, tour, destinationParcel, householdCars, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);

      return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
    }


    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IHouseholdDayWrapper householdDay, ITourWrapper tour,
                IParcelWrapper destinationParcel, int householdCars,
            int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime, HTourModeTime choice = null) {

      IHouseholdWrapper household = tour.Household;
      IPersonWrapper person = tour.Person;
      IPersonDayWrapper personDay = tour.PersonDay;
      Framework.DomainModels.Models.IHouseholdTotals householdTotals = household.HouseholdTotals;

      // household inputs
      int childrenUnder5 = householdTotals.ChildrenUnder5;
      int childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      int nonworkingAdults = householdTotals.NonworkingAdults;
      int retiredAdults = householdTotals.RetiredAdults;
      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
      //var householdCars = household.VehiclesAvailable; MABADD now an input parameter
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);
      int carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(householdCars);
      int income0To25KFlag = household.Has0To25KIncome.ToFlag();
      int income100KPlusFlag = household.Has100KPlusIncome.ToFlag();

      // person inputs
      int partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
      int nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
      int universityStudentFlag = person.IsUniversityStudent.ToFlag();
      int retiredAdultFlag = person.IsRetiredAdult.ToFlag();
      int drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
      int fulltimeWorkerFlag = person.IsFulltimeWorker.ToFlag();
      int childAge5Through15Flag = person.IsChildAge5Through15.ToFlag();
      int childUnder5Flag = person.IsChildUnder5.ToFlag();
      int maleFlag = person.IsMale.ToFlag();
      int ageUnder30Flag = person.AgeIsLessThan30.ToFlag();
      int ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();
      int adultFlag = person.IsAdult.ToFlag();

      // person-day inputs
      int homeBasedToursOnlyFlag = (personDay == null) ? 1 : personDay.OnlyHomeBasedToursExist().ToFlag();
      int firstSimulatedHomeBasedTourFlag = (personDay == null) ? 1 : personDay.IsFirstSimulatedHomeBasedTour().ToFlag();
      int laterSimulatedHomeBasedTourFlag = (personDay == null) ? 0 : personDay.IsLaterSimulatedHomeBasedTour().ToFlag();
      int totalStops = (personDay == null) ? 0 : personDay.GetTotalStops();
      int totalSimulatedStops = (personDay == null) ? 0 : personDay.GetTotalSimulatedStops();
      int escortStops = (personDay == null) ? 0 : personDay.EscortStops;
      int homeBasedTours = (personDay == null) ? 1 : personDay.HomeBasedTours;
      int simulatedHomeBasedTours = (personDay == null) ? 0 : personDay.SimulatedHomeBasedTours;

      // tour inputs
      int escortTourFlag = tour.IsEscortPurpose().ToFlag();
      int shoppingTourFlag = tour.IsShoppingPurpose().ToFlag();
      int mealTourFlag = tour.IsMealPurpose().ToFlag();
      int socialTourFlag = tour.IsSocialPurpose().ToFlag();
      int personalBusinessTourFlag = tour.IsPersonalBusinessPurpose().ToFlag();
      int recreationTourFlag = tour.IsRecreationPurpose().ToFlag();
      int medicalTourFlag = tour.IsMedicalPurpose().ToFlag();
      IParcelWrapper originParcel = tour.OriginParcel;
      //var destinationParcel = tour.DestinationParcel; MABADD now an input parameter
      int jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
      int partialHalfTour1Flag = (tour.PartialHalfTour1Sequence > 0) ? 1 : 0;
      int partialHalfTour2Flag = (tour.PartialHalfTour2Sequence > 0) ? 1 : 0;
      int fullHalfTour1Flag = (tour.FullHalfTour1Sequence > 0) ? 1 : 0;
      int fullHalfTour2Flag = (tour.FullHalfTour2Sequence > 0) ? 1 : 0;
      int parentTourMode = tour.ParentTour == null ? 0 : tour.ParentTour.Mode;


      // remaining inputs
      // Higher priority tour of 2+ tours for the same purpose
      int highPrioritySameFlag = (personDay == null) ? 1 : (tour.GetTotalToursByPurpose() > tour.GetTotalSimulatedToursByPurpose() && tour.GetTotalSimulatedToursByPurpose() == 1).ToFlag();

      // Lower priority tour(s) of 2+ tours for the same purpose
      int lowPrioritySameFlag = (personDay == null) ? 0 : (tour.GetTotalSimulatedToursByPurpose() > 1).ToFlag();

      // Higher priority tour of 2+ tours for different purposes
      int highPriorityDifferentFlag = (personDay == null) ? 0 : (personDay.IsFirstSimulatedHomeBasedTour() && personDay.HomeBasedToursExist()).ToFlag() * (1 - highPrioritySameFlag);

      // Lower priority tour of 2+ tours for different purposes
      int lowPriorityDifferentFlag = (personDay == null) ? 0 : (personDay.IsLaterSimulatedHomeBasedTour() && personDay.HomeBasedToursExist()).ToFlag() * (1 - lowPrioritySameFlag);

      ITimeWindow timeWindow = (householdDay == null) ? new TimeWindow() : tour.GetRelevantTimeWindow(householdDay);
      int totalMinutesAvailableInDay = timeWindow.TotalAvailableMinutes(1, 1440);


      int bigPeriodCount = DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES;
      int nPeriodCombs = bigPeriodCount * (bigPeriodCount + 1) / 2;


      //set components
      int componentIndex = 0;
      int periodComb = -1;

      bool useTimeComponents = Global.Configuration.IsInEstimationMode || constrainedArrivalTime == 0 || constrainedDepartureTime == 0;
      if (useTimeComponents) {
        for (int arrivalPeriodIndex = 0; arrivalPeriodIndex < bigPeriodCount; arrivalPeriodIndex++) {
          MinuteSpan arrivalPeriod = DayPeriod.HBigDayPeriods[arrivalPeriodIndex];
          int arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

          for (int departurePeriodIndex = arrivalPeriodIndex; departurePeriodIndex < bigPeriodCount; departurePeriodIndex++) {
            MinuteSpan departurePeriod = DayPeriod.HBigDayPeriods[departurePeriodIndex];
            int departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);

            if (arrivalPeriod == departurePeriod) {

              componentIndex = arrivalPeriodIndex;
              choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
              ChoiceProbabilityCalculator.Component arrivalComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

              if (arrivalPeriodAvailableMinutes > 0) {
                double hoursArrival = arrivalPeriod.Middle / 60.0;
                int firstCoef = 300;
                arrivalComponent.AddUtilityTerm(300, Math.Log(arrivalPeriodAvailableMinutes));
                //arrival shift variables
                arrivalComponent.AddUtilityTerm(firstCoef + 2, partTimeWorkerFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 3, nonworkingAdultFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 4, universityStudentFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 5, retiredAdultFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 6, drivingAgeStudentFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 7, childAge5Through15Flag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 8, childUnder5Flag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 9, escortTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 10, shoppingTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 11, mealTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 12, socialTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 13, personalBusinessTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 14, recreationTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 15, medicalTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 16, income0To25KFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 17, income100KPlusFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 18, highPrioritySameFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 19, lowPrioritySameFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 20, highPriorityDifferentFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 21, lowPriorityDifferentFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 22, jointTourFlag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 23, partialHalfTour1Flag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 24, fullHalfTour1Flag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 25, partialHalfTour2Flag * hoursArrival);
                arrivalComponent.AddUtilityTerm(firstCoef + 26, fullHalfTour2Flag * hoursArrival);
              }

              componentIndex = bigPeriodCount + departurePeriodIndex;
              choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
              ChoiceProbabilityCalculator.Component departureComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);


              if (departurePeriodAvailableMinutes > 0) {

                departureComponent.AddUtilityTerm(300, Math.Log(departurePeriodAvailableMinutes));
              }
            }
            // set period combination component
            periodComb++;
            componentIndex = 2 * bigPeriodCount + periodComb;
            choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
            ChoiceProbabilityCalculator.Component combinationComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

            if (arrivalPeriodAvailableMinutes > 0 && departurePeriodAvailableMinutes > 0) {
              double hoursDuration = (departurePeriod.Middle - arrivalPeriod.Middle) / 60.0;

              int firstCoef = 700;
              //combination constants
              combinationComponent.AddUtilityTerm(firstCoef + periodComb, 1.0);
              // duration shift variables
              combinationComponent.AddUtilityTerm(firstCoef + 31, escortTourFlag * hoursDuration);
              combinationComponent.AddUtilityTerm(firstCoef + 32, shoppingTourFlag * hoursDuration);
              combinationComponent.AddUtilityTerm(firstCoef + 33, mealTourFlag * hoursDuration);
              combinationComponent.AddUtilityTerm(firstCoef + 34, socialTourFlag * hoursDuration);
              combinationComponent.AddUtilityTerm(firstCoef + 35, personalBusinessTourFlag * hoursDuration);
              combinationComponent.AddUtilityTerm(firstCoef + 36, recreationTourFlag * hoursDuration);
              combinationComponent.AddUtilityTerm(firstCoef + 37, medicalTourFlag * hoursDuration);
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
                combinationComponent.AddUtilityTerm(firstCoef + 48, fulltimeWorkerFlag);
                combinationComponent.AddUtilityTerm(firstCoef + 49, income0To25KFlag);
                combinationComponent.AddUtilityTerm(firstCoef + 50, income100KPlusFlag);
              }
            }
          }
        }
      }

      for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.SchoolBus; mode++) {
        componentIndex = 2 * bigPeriodCount + nPeriodCombs + mode - 1;
        choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
        ChoiceProbabilityCalculator.Component modeComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

        if (mode == Global.Settings.Modes.SchoolBus) {
          modeComponent.AddUtilityTerm(10, 1);
          modeComponent.AddUtilityTerm(11, childUnder5Flag);
          modeComponent.AddUtilityTerm(12, adultFlag);
        } else if (mode == Global.Settings.Modes.ParkAndRide) {
          modeComponent.AddUtilityTerm(10, 1);
          modeComponent.AddUtilityTerm(16, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(17, carsLessThanWorkersFlag);
          modeComponent.AddUtilityTerm(129, destinationParcel.MixedUse2Index1());
          modeComponent.AddUtilityTerm(129, destinationParcel.TotalEmploymentDensity1() / 5000.0);
          modeComponent.AddUtilityTerm(129, destinationParcel.NetIntersectionDensity1() / 50.0);
          //modeComponent.AddUtilityTerm(123, Math.Log(destinationParcel.StopsTransitBuffer1 + 1));
        } else if (mode == Global.Settings.Modes.Transit) {
          modeComponent.AddUtilityTerm(20, 1);
          //        modeComponent.AddUtilityTerm(21, maleFlag);
          modeComponent.AddUtilityTerm(22, ageUnder30Flag);
          //    modeComponent.AddUtilityTerm(23, ageBetween51And98Flag);
          modeComponent.AddUtilityTerm(24, income0To25KFlag);
          //    modeComponent.AddUtilityTerm(25, income100KPlusFlag);
          modeComponent.AddUtilityTerm(26, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(27, carsLessThanDriversFlag);
          modeComponent.AddUtilityTerm(129, destinationParcel.MixedUse2Index1());
          modeComponent.AddUtilityTerm(129, destinationParcel.TotalEmploymentDensity1() / 5000.0);
          modeComponent.AddUtilityTerm(129, destinationParcel.NetIntersectionDensity1() / 50.0);
          //    modeComponent.AddUtilityTerm(124, originParcel.NetIntersectionDensity1()/50.0);
          //    modeComponent.AddUtilityTerm(124, originParcel.HouseholdDensity1()/1000.0);
          //    modeComponent.AddUtilityTerm(124, originParcel.MixedUse2Index1());
          //modeComponent.AddUtilityTerm(123, Math.Log(destinationParcel.StopsTransitBuffer1 + 1));
          //modeComponent.AddUtilityTerm(122, Math.Log(originParcel.StopsTransitBuffer1 + 1));
        } else if (mode == Global.Settings.Modes.Hov3) {
          modeComponent.AddUtilityTerm(30, 1);
          modeComponent.AddUtilityTerm(31, childrenUnder5);
          modeComponent.AddUtilityTerm(32, childrenAge5Through15);
          modeComponent.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
          modeComponent.AddUtilityTerm(38, onePersonHouseholdFlag);
          modeComponent.AddUtilityTerm(39, twoPersonHouseholdFlag);
          modeComponent.AddUtilityTerm(36, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(37, carsLessThanDriversFlag);
        } else if (mode == Global.Settings.Modes.Hov2) {
          modeComponent.AddUtilityTerm(31, childrenUnder5);
          modeComponent.AddUtilityTerm(32, childrenAge5Through15);
          modeComponent.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
          modeComponent.AddUtilityTerm(36, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(37, carsLessThanDriversFlag);
          modeComponent.AddUtilityTerm(40, 1);
          modeComponent.AddUtilityTerm(41, onePersonHouseholdFlag);
        } else if (mode == Global.Settings.Modes.Sov) {
          //    modeComponent.AddUtilityTerm(50, 1);
          //    modeComponent.AddUtilityTerm(54, income0To25KFlag);
          modeComponent.AddUtilityTerm(55, income100KPlusFlag);
          modeComponent.AddUtilityTerm(57, carsLessThanWorkersFlag);
        } else if (mode == Global.Settings.Modes.Bike) {
          modeComponent.AddUtilityTerm(60, 1);
          modeComponent.AddUtilityTerm(61, maleFlag);
          modeComponent.AddUtilityTerm(62, ageUnder30Flag);
          modeComponent.AddUtilityTerm(63, ageBetween51And98Flag);
          //        modeComponent.AddUtilityTerm(64, income0To25KFlag);
          //      modeComponent.AddUtilityTerm(65, income100KPlusFlag);
          modeComponent.AddUtilityTerm(66, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(67, carsLessThanDriversFlag);
          modeComponent.AddUtilityTerm(169, destinationParcel.MixedUse4Index2());
          modeComponent.AddUtilityTerm(169, destinationParcel.TotalEmploymentDensity2() / 20000.0);
          modeComponent.AddUtilityTerm(169, destinationParcel.NetIntersectionDensity2() / 200.0);
          modeComponent.AddUtilityTerm(164, originParcel.NetIntersectionDensity2() / 200.0);
          modeComponent.AddUtilityTerm(164, originParcel.HouseholdDensity2() / 4000.0);
          modeComponent.AddUtilityTerm(164, originParcel.MixedUse4Index2());
        } else if (mode == Global.Settings.Modes.Walk) {
          modeComponent.AddUtilityTerm(70, 1.0);
          modeComponent.AddUtilityTerm(71, maleFlag);
          modeComponent.AddUtilityTerm(72, ageUnder30Flag);
          //        modeComponent.AddUtilityTerm(73, ageBetween51And98Flag);
          //        modeComponent.AddUtilityTerm(74, income0To25KFlag);
          modeComponent.AddUtilityTerm(75, income100KPlusFlag);
          //        modeComponent.AddUtilityTerm(76, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(77, carsLessThanDriversFlag);
          modeComponent.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
          modeComponent.AddUtilityTerm(179, destinationParcel.TotalEmploymentDensity1() / 5000.0);
          modeComponent.AddUtilityTerm(179, destinationParcel.NetIntersectionDensity1() / 50.0);
          modeComponent.AddUtilityTerm(179, originParcel.NetIntersectionDensity1() / 50.0);
          modeComponent.AddUtilityTerm(179, originParcel.HouseholdDensity1() / 1000.0);
          modeComponent.AddUtilityTerm(179, originParcel.MixedUse4Index1());
        }

        if (mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.Hov2
             || mode == Global.Settings.Modes.Hov3 || mode == Global.Settings.Modes.Transit) {
          int firstCoef = 200 + 10 * mode;
          //modeComponent.AddUtilityTerm(firstCoef + 0, escortTourFlag);
          //modeComponent.AddUtilityTerm(firstCoef + 1, shoppingTourFlag);
          //modeComponent.AddUtilityTerm(firstCoef + 2, mealTourFlag);
          //modeComponent.AddUtilityTerm(firstCoef + 3, socialTourFlag);
          //modeComponent.AddUtilityTerm(firstCoef + 4, personalBusinessTourFlag);
          //modeComponent.AddUtilityTerm(firstCoef + 5, recreationTourFlag);
          //    modeComponent.AddUtilityTerm(firstCoef + 6, medicalTourFlag);
          //modeComponent.AddUtilityTerm(firstCoef + 7, jointTourFlag);
          modeComponent.AddUtilityTerm(firstCoef + 8, Math.Min(partialHalfTour1Flag + partialHalfTour2Flag, 1.0));
          modeComponent.AddUtilityTerm(firstCoef + 9, Math.Min(fullHalfTour1Flag + fullHalfTour2Flag, 1.0));

          modeComponent.AddUtilityTerm(290 + mode, mode == parentTourMode ? 1 : 0);
        }
        modeComponent.AddUtilityTerm(298, mode >= Global.Settings.Modes.Sov && mode <= Global.Settings.Modes.Hov3 && parentTourMode == Global.Settings.Modes.Sov ? 1 : 0);
        modeComponent.AddUtilityTerm(299, mode >= Global.Settings.Modes.Sov && mode <= Global.Settings.Modes.Hov3 && parentTourMode >= Global.Settings.Modes.Hov2 && parentTourMode <= Global.Settings.Modes.Hov3 ? 1 : 0);
      }



      //loop on all alternatives, using modeTimes objects
      {
        foreach (HTourModeTime modeTimes in HTourModeTime.ModeTimes[ParallelUtility.threadLocalAssignedIndex.Value]) {
          MinuteSpan arrivalPeriod = modeTimes.ArrivalPeriod;
          int arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

          MinuteSpan departurePeriod = modeTimes.DeparturePeriod;
          int departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);
          periodComb = modeTimes.PeriodCombinationIndex;

          int mode = modeTimes.Mode;

          int altIndex = modeTimes.Index;

          //set availabillity based on time window variables and any constrained choices
          bool available = (modeTimes.LongestFeasibleWindow != null) && (mode > 0)
              && (mode != Global.Settings.Modes.Sov || (tour.Person.IsDrivingAge && tour.Household.VehiclesAvailable > 0))
              && (constrainedMode <= 0 || constrainedMode == mode)
              && (constrainedArrivalTime <= 0 || (constrainedArrivalTime >= arrivalPeriod.Start && constrainedArrivalTime <= arrivalPeriod.End))
              && (constrainedDepartureTime <= 0 || (constrainedDepartureTime >= departurePeriod.Start && constrainedDepartureTime <= departurePeriod.End));


          ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(altIndex, available,
                                                                                 choice != null && choice.Index == altIndex);

          alternative.Choice = modeTimes; // JLB added 20130420

          //alternative.AddNestedAlternative(HTourModeTime.TOTAL_TOUR_MODE_TIMES + periodComb + 1, periodComb, THETA_PARAMETER);

          if (Global.Configuration.IsInEstimationMode && choice != null && altIndex == choice.Index) {
            Global.PrintFile.WriteLine("Aper Dper Mode {0} {1} {2} Travel Times {3} {4} Window {5} {6}",
                                       arrivalPeriod.Index, departurePeriod.Index, mode,
                                       modeTimes.ModeAvailableToDestination ? modeTimes.TravelTimeToDestination : -1,
                                       modeTimes.ModeAvailableFromDestination ? modeTimes.TravelTimeFromDestination : -1,
                                       modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.Start : -1,
                                       modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.End : -1);

          }
          /*                if (altIndex == 0)
                              {
                                  alternative.AddUtilityTerm(991, tour.Household.Id);
                                  alternative.AddUtilityTerm(992, tour.Person.Id);
                                  alternative.AddUtilityTerm(993, tour.PersonDay.Day);
                                  alternative.AddUtilityTerm(994, tour.Sequence);
                                  alternative.AddUtilityTerm(995, constrainedMode);
                                  alternative.AddUtilityTerm(996, constrainedArrivalTime);
                                  alternative.AddUtilityTerm(997, constrainedDepartureTime);
                                  alternative.AddUtilityTerm(998, tour.DestinationPurpose);
                                  alternative.AddUtilityTerm(999, (tour.ParentTour == null) ? 0 : 1);
                              }
          */
          //if in application mode and combination is not available, can skip the rest
          if (!Global.Configuration.IsInEstimationMode && !alternative.Available) {
            continue;
          }
          if (useTimeComponents) {
            // arrival period utility component
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(arrivalPeriod.Index));

            // departure period utility component
            alternative.AddUtilityComponent(
                choiceProbabilityCalculator.GetUtilityComponent(bigPeriodCount + departurePeriod.Index));

            // period combination utility component
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(2 * bigPeriodCount + periodComb));
          }
          // mode utility component
          alternative.AddUtilityComponent(
              choiceProbabilityCalculator.GetUtilityComponent(2 * bigPeriodCount + nPeriodCombs + mode - 1));

          //even in estimation mode, do not need the rest of the code if not available
          if (!alternative.Available) {
            continue;
          }

          // set parking cost for period combination
          double parkingDuration = (departurePeriod == arrivalPeriod
                                               ? (arrivalPeriod.End - arrivalPeriod.Start) / 2.0
                                               : departurePeriod.Middle - arrivalPeriod.Middle) / 60.0;

          // parking at work is free if no paid parking at work and tour goes to usual workplace
          double destinationParkingCost = (!Global.Configuration.IsInEstimationMode
                                                  && Global.Configuration.ShouldRunPayToParkAtWorkplaceModel
                                                  && tour.Person.UsualWorkParcel != null
                                                  && destinationParcel == tour.Person.UsualWorkParcel &&
                                                  person.PaidParkingAtWorkplace == 0)
                                                     ? 0.0
                                                     : destinationParcel.ParkingCostBuffer1(parkingDuration);
          double parkingCostFraction = (mode == Global.Settings.Modes.Sov)
                                                  ? 1.0
                                                  : (mode == Global.Settings.Modes.Hov2)
                                                        ? 1.0 / Global.Configuration.Coefficients_HOV2CostDivisor_Work
                                                        : (mode == Global.Settings.Modes.Hov3)
                                                              ? 1.0 / Global.Configuration.Coefficients_HOV3CostDivisor_Work
                                                              : 0.0;


          double minimumTimeNeeded = modeTimes.TravelTimeToDestination + modeTimes.TravelTimeFromDestination +
                                            Global.Settings.Times.MinimumActivityDuration;

          alternative.AddUtilityTerm(1, modeTimes.GeneralizedTimeToDestination + modeTimes.GeneralizedTimeFromDestination);
          //alternative.AddUtilityTerm(2, destinationParkingCost*parkingCostFraction);
          //alternative.AddUtilityTerm(3,
          //                           Math.Log(Math.Min(1140, modeTimes.LongestFeasibleWindow.End - modeTimes.LongestFeasibleWindow.Start -
          //                                                     minimumTimeNeeded + 1.0 )));
          // JLB 20140204 replaced coeff 3 with a different time window formulation:  time pressure
          //    instead of having positive utility for increasing time window, have negative utility for decreasing time window
          alternative.AddUtilityTerm(3,
           Math.Log(Math.Max(Constants.EPSILON, 1 -
                  Math.Pow(minimumTimeNeeded / (Math.Min(1140, modeTimes.LongestFeasibleWindow.End - modeTimes.LongestFeasibleWindow.Start)), 0.3)
                  )));

          //alternative.AddUtilityTerm(4, Math.Log((totalMinutesAvailableInDay + 1.0)/(minimumTimeNeeded + 1.0)));
          //alternative.AddUtilityTerm(4, 
          //                    Math.Log(Math.Max(Constants.EPSILON, 1 - minimumTimeNeeded/(Math.Min(1140, totalMinutesAvailableInDay)))));

          //alternative.AddUtilityTerm(5,
          //                           (maleFlag == 0 && mode == Global.Settings.Modes.Walk &&
          //                            arrivalPeriod.Index >= DayPeriod.EVENING)
          //                                   ? 1
          //                                   : 0);
          //    alternative.AddUtilityTerm(5,
          //                               (maleFlag == 0 && mode == Global.Settings.Modes.Walk &&
          //                                departurePeriod.Index >= DayPeriod.EVENING)
          //                                   ? 1
          //                                   : 0);



        }
      }
    }
  }
}