﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.ChoiceModels.H;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.ChoiceModels.Actum.Models {
  public class TourModeTimeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumTourModeTimeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 21;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 999;
    private const int THETA_PARAMETER = 900;

    private readonly ITourCreator _creator =
        Global
        .ContainerDaySim
        .GetInstance<IWrapperFactory<ITourCreator>>()
        .Creator;

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
        if (tour.DestinationParcel == null || tour.OriginParcel == null || tour.Mode < Global.Settings.Modes.Walk || tour.Mode > Global.Settings.Modes.WalkRideBike) {
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
        if (Global.Configuration.TraceModelResultValidity) {
          Global.PrintFile.WriteLine("  >> TourModeTimeModel HH/P/T/mode {0} {1} {2} {3}", tour.Household.Id, tour.Person.Sequence, tour.Sequence, choice.Mode);
        }
        tour.Mode = choice.Mode;
        tour.HalfTour1AccessCost = choice.OriginAccessCost / 2.0;
        tour.HalfTour1AccessDistance = choice.OriginAccessDistance / 2.0;
        tour.HalfTour1AccessMode = choice.OriginAccessMode;
        tour.HalfTour1AccessStopAreaKey = choice.ParkAndRideOriginStopAreaKey;
        tour.HalfTour1AccessTime = choice.OriginAccessTime / 2.0;
        tour.HalfTour1EgressCost = choice.DestinationAccessCost / 2.0;
        tour.HalfTour1EgressDistance = choice.DestinationAccessDistance / 2.0;
        tour.HalfTour1EgressMode = choice.DestinationAccessMode;
        tour.HalfTour1EgressStopAreaKey = choice.ParkAndRideDestinationStopAreaKey;
        tour.HalfTour1EgressTime = choice.DestinationAccessTime / 2.0;
        tour.HalfTour2AccessCost = choice.DestinationAccessCost / 2.0;
        tour.HalfTour2AccessDistance = choice.DestinationAccessDistance / 2.0;
        tour.HalfTour2AccessMode = choice.DestinationAccessMode;
        tour.HalfTour2AccessStopAreaKey = choice.ParkAndRideDestinationStopAreaKey;
        tour.HalfTour2AccessTime = choice.DestinationAccessTime / 2.0;
        tour.HalfTour2EgressCost = choice.OriginAccessCost / 2.0;
        tour.HalfTour2EgressDistance = choice.OriginAccessDistance / 2.0;
        tour.HalfTour2EgressMode = choice.OriginAccessMode;
        tour.HalfTour2EgressStopAreaKey = choice.ParkAndRideOriginStopAreaKey;
        tour.HalfTour2EgressTime = choice.OriginAccessTime / 2.0;
        tour.HalfTour1TravelTime = choice.TravelTimeToDestination;
        tour.HalfTour2TravelTime = choice.TravelTimeFromDestination;
        tour.TravelCostForPTBikeTour = choice.PathCost;
        tour.TravelDistanceForPTBikeTour = choice.PathDistance;


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
    public ChoiceProbabilityCalculator.Alternative RunNested(PersonWrapper person, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars, double transitDiscountFraction, int purpose) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      TourWrapper tour = (TourWrapper)_creator.CreateWrapper(person, null, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, purpose);

      return RunNested(tour, destinationParcel, householdCars, transitDiscountFraction);
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(PersonDayWrapper personDay, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars, int purpose) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      //var tour = (TourWrapper)_creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);
      TourWrapper tour = (TourWrapper)_creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, purpose);

      return RunNested(tour, destinationParcel, householdCars, personDay.Person.GetTransitFareDiscountFraction());
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(TourWrapper tour, IParcelWrapper destinationParcel, int householdCars, double transitDiscountFraction) {
      if (Global.Configuration.AvoidDisaggregateModeChoiceLogsums) {
        return null;
      }
      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

      HouseholdDayWrapper householdDay = (tour.PersonDay == null) ? null : (HouseholdDayWrapper)tour.PersonDay.HouseholdDay;

      int constrainedMode = 0;
      int constrainedArrivalTime = (Global.Configuration.ConstrainTimesForModeChoiceLogsums) ? tour.DestinationArrivalTime : 0;
      int constrainedDepartureTime = (Global.Configuration.ConstrainTimesForModeChoiceLogsums) ? tour.DestinationDepartureTime : 0;

      tour.DestinationParcel = destinationParcel;
      HTourModeTime.SetModeTimeImpedances(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime, householdCars, transitDiscountFraction);

      RunModel(choiceProbabilityCalculator, householdDay, tour, destinationParcel, householdCars, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);

      return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
    }


    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, TourWrapper tour,
                IParcelWrapper destinationParcel, int householdCars,
             int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime, HTourModeTime choice = null) {


      //private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, TourWrapper tour,
      //			int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime, HTourModeTime choice = null) {

      IHouseholdWrapper household = tour.Household;
      IPersonWrapper person = tour.Person;
      IPersonDayWrapper personDay = tour.PersonDay;
      IHouseholdTotals householdTotals = household.HouseholdTotals;

      // household inputs
      int childrenUnder5 = householdTotals.ChildrenUnder5;
      int childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      int nonworkingAdults = householdTotals.NonworkingAdults;
      int retiredAdults = householdTotals.RetiredAdults;

      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();

      //var householdCars = household.VehiclesAvailable;
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);
      int carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(householdCars);

      int HHwithChildrenFlag = household.HasChildren.ToFlag();
      int HHwithSmallChildrenFlag = household.HasChildrenUnder5.ToFlag();
      int HHwithLowIncomeFlag = (household.Income >= 300000 && household.Income < 600000).ToFlag();
      int HHwithMidleIncomeFlag = (household.Income >= 600000 && household.Income < 900000).ToFlag();
      int HHwithHighIncomeFlag = (household.Income >= 900000).ToFlag();

      int primaryFamilyTimeFlag = (householdDay == null) ? 0 : householdDay.PrimaryPriorityTimeFlag;

      // person inputs
      int partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
      int nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
      int universityStudentFlag = person.IsUniversityStudent.ToFlag();
      int retiredAdultFlag = person.IsRetiredAdult.ToFlag();
      int fullTimeWorkerFlag = person.IsFulltimeWorker.ToFlag();
      int childAge5Through15Flag = person.IsChildAge5Through15.ToFlag();
      int childUnder5Flag = person.IsChildUnder5.ToFlag();
      int adultFlag = person.IsAdult.ToFlag();

      int maleFlag = person.IsMale.ToFlag();
      int femaleFlag = person.IsFemale.ToFlag();

      int PTpass = person.TransitPassOwnership;

      // person-day inputs
      int homeBasedToursOnlyFlag = 1;
      int firstSimulatedHomeBasedTourFlag = 1;
      int laterSimulatedHomeBasedTourFlag = 0;
      int totalStops = 0;
      int totalSimulatedStops = 0;
      int escortStops = 0;
      int homeBasedTours = 1;
      int simulatedHomeBasedTours = 0;

      if (!(personDay == null)) {
        homeBasedToursOnlyFlag = personDay.OnlyHomeBasedToursExist().ToFlag();
        firstSimulatedHomeBasedTourFlag = personDay.IsFirstSimulatedHomeBasedTour().ToFlag();
        laterSimulatedHomeBasedTourFlag = personDay.IsLaterSimulatedHomeBasedTour().ToFlag();
        totalStops = personDay.GetTotalStops();
        totalSimulatedStops = personDay.GetTotalSimulatedStops();
        escortStops = personDay.EscortStops;
        homeBasedTours = personDay.HomeBasedTours;
        simulatedHomeBasedTours = personDay.SimulatedHomeBasedTours;
      }

      // tour inputs
      int escortTourFlag = tour.IsEscortPurpose().ToFlag();
      int shoppingTourFlag = tour.IsShoppingPurpose().ToFlag();
      int socialTourFlag = tour.IsSocialPurpose().ToFlag();
      int personalBusinessTourFlag = tour.IsPersonalBusinessPurpose().ToFlag();
      int workTourFlag = tour.IsWorkPurpose().ToFlag();
      int educationTourFlag = tour.IsSchoolPurpose().ToFlag();
      int businessTourFlag = tour.IsBusinessPurpose().ToFlag();

      IParcelWrapper originParcel = tour.OriginParcel;
      //var destinationParcel = tour.DestinationParcel;
      int jointTourFlag = (tour.JointTourSequence > 0) ? 1 : 0;
      int partialHalfTour1Flag = (tour.PartialHalfTour1Sequence > 0) ? 1 : 0;
      int partialHalfTour2Flag = (tour.PartialHalfTour2Sequence > 0) ? 1 : 0;
      bool partialHalfTour = (tour.PartialHalfTour1Sequence > 0 || tour.PartialHalfTour2Sequence > 0);
      int fullHalfTour1Flag = (tour.FullHalfTour1Sequence > 0) ? 1 : 0;
      int fullHalfTour2Flag = (tour.FullHalfTour2Sequence > 0) ? 1 : 0;


      // remaining inputs

      //Initialize a few variables in case personDay is null
      // Higher priority tour of 2+ tours for the same purpose
      int highPrioritySameFlag = 0;
      // Lower priority tour(s) of 2+ tours for the same purpose
      int lowPrioritySameFlag = 0;
      // Higher priority tour of 2+ tours for different purposes
      int highPriorityDifferentFlag = 0;
      // Lower priority tour of 2+ tours for different purposes
      int lowPriorityDifferentFlag = 0;

      if (!(personDay == null)) {
        // Higher priority tour of 2+ tours for the same purpose
        highPrioritySameFlag = (tour.GetTotalToursByPurpose() > tour.GetTotalSimulatedToursByPurpose() && tour.GetTotalSimulatedToursByPurpose() == 1).ToFlag();
        // Lower priority tour(s) of 2+ tours for the same purpose
        lowPrioritySameFlag = (tour.GetTotalSimulatedToursByPurpose() > 1).ToFlag();
        // Higher priority tour of 2+ tours for different purposes
        highPriorityDifferentFlag = (personDay.IsFirstSimulatedHomeBasedTour() && personDay.HomeBasedToursExist()).ToFlag() * (1 - highPrioritySameFlag);
        // Lower priority tour of 2+ tours for different purposes
        lowPriorityDifferentFlag = (personDay.IsLaterSimulatedHomeBasedTour() && personDay.HomeBasedToursExist()).ToFlag() * (1 - lowPrioritySameFlag);
      }

      ITimeWindow timeWindow = (householdDay == null) ? new TimeWindow() : tour.GetRelevantTimeWindow(householdDay);
      int totalMinutesAvailableInDay = timeWindow.TotalAvailableMinutes(1, 1440);
      if (totalMinutesAvailableInDay < 0) {
        if (!Global.Configuration.IsInEstimationMode) {
          householdDay.IsValid = false;
        }
        totalMinutesAvailableInDay = 0;
      }

      int bigPeriodCount = DayPeriod.H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES;
      int nPeriodCombs = bigPeriodCount * (bigPeriodCount + 1) / 2;

      bool useTimeComponents = Global.Configuration.IsInEstimationMode || constrainedArrivalTime == 0 || constrainedDepartureTime == 0;
      int componentIndex = 0;
      int periodComb = -1;
      //set components
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
      //for (var mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.WalkRideBike; mode++) {  replaced 20171120 JLB
      for (int mode = Global.Settings.Modes.Walk; mode <= Global.Settings.Modes.PaidRideShare; mode++) {
        componentIndex = 2 * bigPeriodCount + nPeriodCombs + mode - 1;
        choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
        ChoiceProbabilityCalculator.Component modeComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

        //if (mode == Global.Settings.Modes.SchoolBus) {
        //	modeComponent.AddUtilityTerm(10, 1);
        //}
        if (mode == Global.Settings.Modes.Transit) {
          modeComponent.AddUtilityTerm(20, 1);
          modeComponent.AddUtilityTerm(21, femaleFlag);
          //modeComponent.AddUtilityTerm(22, retiredAdultFlag);

          modeComponent.AddUtilityTerm(22, PTpass);

          //GV: one person HH; 8. juni 2016
          //modeComponent.AddUtilityTerm(23, onePersonHouseholdFlag);

          //GV: income effect is not with a correct sign
          //modeComponent.AddUtilityTerm(23, HHwithLowIncomeFlag);
          //modeComponent.AddUtilityTerm(24, HHwithMidleIncomeFlag);
          //modeComponent.AddUtilityTerm(25, HHwithHighIncomeFlag);

          //GV: not significant
          //modeComponent.AddUtilityTerm(26, childrenUnder5);
          //modeComponent.AddUtilityTerm(27, childrenAge5Through15);

          modeComponent.AddUtilityTerm(28, nonworkingAdults + retiredAdults);

          //modeComponent.AddUtilityTerm(26, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(29, carsLessThanDriversFlag);
        } else if (mode == Global.Settings.Modes.Hov3) {
          modeComponent.AddUtilityTerm(30, 1);
          modeComponent.AddUtilityTerm(31, childrenUnder5);
          modeComponent.AddUtilityTerm(32, childrenAge5Through15);
          modeComponent.AddUtilityTerm(33, nonworkingAdults + retiredAdults);
          modeComponent.AddUtilityTerm(34, femaleFlag);

          //modeComponent.AddUtilityTerm(38, onePersonHouseholdFlag);
          modeComponent.AddUtilityTerm(36, twoPersonHouseholdFlag);

          //GV: commented out 7. june 2016
          //modeComponent.AddUtilityTerm(37, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(38, carsLessThanDriversFlag);
        } else if (mode == Global.Settings.Modes.Hov2) {
          modeComponent.AddUtilityTerm(40, 1);
          modeComponent.AddUtilityTerm(41, maleFlag);
          //modeComponent.AddUtilityTerm(41, onePersonHouseholdFlag);

          //GV: Testing coeff. 42-44 again, 26. may 2016, coeff. numbering changed
          modeComponent.AddUtilityTerm(42, childrenUnder5);
          modeComponent.AddUtilityTerm(43, childrenAge5Through15);
          modeComponent.AddUtilityTerm(44, nonworkingAdults + retiredAdults);

          //GV: these are significant and plus; 8. juni 2016
          //modeComponent.AddUtilityTerm(45, HHwithLowIncomeFlag);
          //modeComponent.AddUtilityTerm(46, HHwithMidleIncomeFlag); 
          //modeComponent.AddUtilityTerm(47, HHwithHighIncomeFlag); 

          //GV coeff. numbering changed; 8. june 2016 - not significant
          //modeComponent.AddUtilityTerm(48, noCarsInHouseholdFlag);
          //modeComponent.AddUtilityTerm(49, carsLessThanDriversFlag);
        } else if (mode == Global.Settings.Modes.Sov) {
          modeComponent.AddUtilityTerm(50, 1);
          //not significant: modeComponent.AddUtilityTerm(51, maleFlag);
          modeComponent.AddUtilityTerm(52, fullTimeWorkerFlag);
          modeComponent.AddUtilityTerm(53, partTimeWorkerFlag);
          modeComponent.AddUtilityTerm(54, onePersonHouseholdFlag);

          //GV: these are NOT significant
          //modeComponent.AddUtilityTerm(55, HHwithLowIncomeFlag);
          //modeComponent.AddUtilityTerm(56, HHwithMidleIncomeFlag);
          //modeComponent.AddUtilityTerm(57, HHwithHighIncomeFlag);

          //GV: coeff. numbering changed, 26. may 2016
          modeComponent.AddUtilityTerm(58, carsLessThanWorkersFlag);
        } else if (mode == Global.Settings.Modes.Bike) {
          modeComponent.AddUtilityTerm(60, 1);
          modeComponent.AddUtilityTerm(61, femaleFlag);
          //modeComponent.AddUtilityTerm(62, childrenUnder5);
          //GV: changed small kids to retired; 3. june 2016
          modeComponent.AddUtilityTerm(62, retiredAdults);
          modeComponent.AddUtilityTerm(63, childAge5Through15Flag);

          //GV: 3. june 2016 - added "no cars in HH" for full/part_time workers
          modeComponent.AddUtilityTerm(64, fullTimeWorkerFlag + noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(65, partTimeWorkerFlag + noCarsInHouseholdFlag);

          //modeComponent.AddUtilityTerm(66, noCarsInHouseholdFlag);
          modeComponent.AddUtilityTerm(67, carsLessThanDriversFlag);

          //GV: university students; 3. june 2016 - not significant
          //modeComponent.AddUtilityTerm(68, universityStudentFlag);

          //GV: one person HH; 8. juni 2016
          modeComponent.AddUtilityTerm(69, onePersonHouseholdFlag);

        } else if (mode == Global.Settings.Modes.Walk) {
          modeComponent.AddUtilityTerm(70, 1.0);
          //GV: Testing female variable again; 26. may 2016 - not sign.
          //modeComponent.AddUtilityTerm(71, femaleFlag);
          modeComponent.AddUtilityTerm(72, nonworkingAdults);
          //not sign.: modeComponent.AddUtilityTerm(73, retiredAdults);

          //GV: one person HH
          modeComponent.AddUtilityTerm(74, onePersonHouseholdFlag);

          //GV: not significant
          //modeComponent.AddUtilityTerm(76, noCarsInHouseholdFlag);
          //modeComponent.AddUtilityTerm(77, carsLessThanDriversFlag);
        } else if (mode == Global.Settings.Modes.ParkAndRide) {
          modeComponent.AddUtilityTerm(80, 1.0);
        } else if (mode == Global.Settings.Modes.CarKissRideWalk) {
          modeComponent.AddUtilityTerm(90, 1.0);
        } else if (mode == Global.Settings.Modes.BikeParkRideWalk) {
          modeComponent.AddUtilityTerm(100, 1.0);
        } else if (mode == Global.Settings.Modes.BikeParkRideBike) {
          modeComponent.AddUtilityTerm(110, 1.0);
        } else if (mode == Global.Settings.Modes.BikeOnTransit) {
          modeComponent.AddUtilityTerm(120, 1.0);
        } else if (mode == Global.Settings.Modes.CarParkRideBike) {
          modeComponent.AddUtilityTerm(130, 1.0);
        } else if (mode == Global.Settings.Modes.WalkRideBike) {
          modeComponent.AddUtilityTerm(140, 1.0);
        } else if (mode == Global.Settings.Modes.PaidRideShare) {
          //modeComponent.AddUtilityTerm(150, 1.0);

          double modeConstant = Global.Configuration.AV_PaidRideShareModeUsesAVs
                     ? Global.Configuration.AV_PaidRideShare_ModeConstant
                     + Global.Configuration.AV_PaidRideShare_DensityCoefficient * Math.Min(originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2, 6000)
                     + Global.Configuration.AV_PaidRideShare_AVOwnerCoefficient * (household.OwnsAutomatedVehicles > 0).ToFlag()
                     : Global.Configuration.PaidRideShare_ModeConstant
                     + Global.Configuration.PaidRideShare_DensityCoefficient * Math.Min(originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2, 6000);

          modeComponent.AddUtilityTerm(150, modeConstant);
          modeComponent.AddUtilityTerm(150, Global.Configuration.PaidRideShare_Age26to35Coefficient * tour.Person.AgeIsBetween26And35.ToFlag());
          modeComponent.AddUtilityTerm(150, Global.Configuration.PaidRideShare_Age18to25Coefficient * tour.Person.AgeIsBetween18And25.ToFlag());
          modeComponent.AddUtilityTerm(150, Global.Configuration.PaidRideShare_AgeOver65Coefficient * (tour.Person.Age >= 65).ToFlag());
        }


        //GV: Estimation of importance of "purpose" per mode - SOV is zero-alt and Work is zero-alt 
        if (mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.Hov2
             || mode == Global.Settings.Modes.Hov3 || mode == Global.Settings.Modes.Transit) {
          int firstCoef = 200 + 10 * mode;

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
        foreach (HTourModeTime modeTimes in HTourModeTime.ModeTimes[ParallelUtility.threadLocalAssignedIndex.Value]) {
          MinuteSpan arrivalPeriod = modeTimes.ArrivalPeriod;
          int arrivalPeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(arrivalPeriod.Start, arrivalPeriod.End);

          MinuteSpan departurePeriod = modeTimes.DeparturePeriod;
          int departurePeriodAvailableMinutes = timeWindow.TotalAvailableMinutes(departurePeriod.Start, departurePeriod.End);
          periodComb = modeTimes.PeriodCombinationIndex;

          int mode = modeTimes.Mode;

          int altIndex = modeTimes.Index;




          //limit availability based on mode and tour purpose
          bool available = (mode == Global.Settings.Modes.BikeParkRideBike || mode == Global.Settings.Modes.CarParkRideBike || mode == Global.Settings.Modes.WalkRideBike)
              && (!(tour.IsWorkPurpose() || tour.IsSchoolPurpose())) ? false : true;

          //further limit availability on mode
          if (mode == Global.Settings.Modes.PaidRideShare && !Global.Configuration.PaidRideShareModeIsAvailable) {
            available = false;
          }

          //further limit availability if tour includes joint travel
          if ((tour.JointTourSequence > 0
              || tour.FullHalfTour1Sequence > 0 || tour.FullHalfTour2Sequence > 0
              || tour.PartialHalfTour1Sequence > 0 || tour.PartialHalfTour2Sequence > 0)
              && (mode > Global.Settings.Modes.Transit)) {
            available = false;
          }

          //further limit availability:  kissAndRide and carParkRideBike are not supported 
          available = (available == true) && (mode != Global.Settings.Modes.CarKissRideWalk) && (mode != Global.Settings.Modes.CarParkRideBike);

          //further limit availabillity based on time window variables and any constrained choices
          available = (available == true)
              && (modeTimes.LongestFeasibleWindow != null) && (mode > 0)
              //&& (mode <= Global.Settings.Modes.Transit)
              && (person.Age >= 18 || (modeTimes.Mode != Global.Settings.Modes.Sov && modeTimes.Mode != Global.Settings.Modes.HovDriver))
              && (constrainedMode > 0 || mode == Global.Settings.Modes.Walk || mode == Global.Settings.Modes.Bike || mode == Global.Settings.Modes.HovDriver || mode == Global.Settings.Modes.Transit || !partialHalfTour)
              && (constrainedMode <= 0 || constrainedMode == mode)
              && (constrainedArrivalTime <= 0 || (constrainedArrivalTime >= arrivalPeriod.Start && constrainedArrivalTime <= arrivalPeriod.End))
              && (constrainedDepartureTime <= 0 || (constrainedDepartureTime >= departurePeriod.Start && constrainedDepartureTime <= departurePeriod.End));

          ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(altIndex, available,
                                                                                                     choice != null && choice.Index == altIndex);

          alternative.Choice = modeTimes; // JLB added 20130420

          //alternative.AddNestedAlternative(HTourModeTime.TOTAL_TOUR_MODE_TIMES + periodComb + 1, periodComb, THETA_PARAMETER);
          alternative.AddNestedAlternative(HTourModeTime.TotalTourModeTimes + mode, mode - 1, THETA_PARAMETER);

          //if (Global.Configuration.IsInEstimationMode && altIndex == choice.Index) {
          //	Global.PrintFile.WriteLine("Aper Dper Mode {0} {1} {2} Travel Times {3} {4} Window {5} {6}",
          //										arrivalPeriod.Index, departurePeriod.Index, mode,
          //										modeTimes.ModeAvailableToDestination ? modeTimes.TravelTimeToDestination : -1,
          //										modeTimes.ModeAvailableFromDestination ? modeTimes.TravelTimeFromDestination : -1,
          //										modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.Start : -1,
          //										modeTimes.LongestFeasibleWindow != null ? modeTimes.LongestFeasibleWindow.End : -1);

          //}
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

          //GV and JB: the parking cost are handled as part of genaralised time

          double minimumTimeNeeded = modeTimes.TravelTimeToDestination + modeTimes.TravelTimeFromDestination + Global.Settings.Times.MinimumActivityDuration;

          alternative.AddUtilityTerm(1, modeTimes.GeneralizedTimeToDestination + modeTimes.GeneralizedTimeFromDestination);

          //alternative.AddUtilityTerm(3,
          //                           Math.Log(modeTimes.LongestFeasibleWindow.End - modeTimes.LongestFeasibleWindow.Start -
          //                                    minimumTimeNeeded + 1.0));

          // JLB 20140204 replaced coeff 3 with a different time window formulation:  time pressure
          //    instead of having positive utility for increasing time window, have negative utility for decreasing time window
          alternative.AddUtilityTerm(3,
                  Math.Log(Math.Max(Constants.EPSILON, 1 -
                  Math.Pow(minimumTimeNeeded / (Math.Min(840, modeTimes.LongestFeasibleWindow.End - modeTimes.LongestFeasibleWindow.Start)), 0.8)
                  )));



          alternative.AddUtilityTerm(4, Math.Log((totalMinutesAvailableInDay + 1.0) / (minimumTimeNeeded + 1.0)));

          alternative.AddUtilityTerm(5,
                                              (maleFlag == 0 && mode == Global.Settings.Modes.Walk &&
                                               arrivalPeriod.Index >= DayPeriod.EVENING)
                                                  ? 1
                                                  : 0);

          if (altIndex == 0) {
            alternative.AddUtilityTerm(998, tour.DestinationPurpose);
            alternative.AddUtilityTerm(999, (tour.ParentTour == null) ? 0 : 1);
          }
        }

      }

    }
  }
}
