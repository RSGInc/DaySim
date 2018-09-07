// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  public class IntermediateStopGenerationModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "IntermediateStopGenerationModel";
    private const int TOTAL_ALTERNATIVES = 8;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 240;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.IntermediateStopGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public int Run(ITripWrapper trip) {
      return Run(trip, Global.Settings.Purposes.NoneOrHome);
    }

    public int Run(ITripWrapper trip, int choice) {
      if (trip == null) {
        throw new ArgumentNullException("trip");
      }

      trip.PersonDay.ResetRandom(40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 20 + trip.Sequence - 1);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return choice;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(trip.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (trip.OriginParcel == null) {
          return Constants.DEFAULT_VALUE;
        }
        RunModel(choiceProbabilityCalculator, trip, choice);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, trip);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);
        choice = (int)chosenAlternative.Choice;
      }

      return choice;
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITripWrapper trip, int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = trip.Household;
      IPersonWrapper person = trip.Person;
      IPersonDayWrapper personDay = trip.PersonDay;
      ITourWrapper tour = trip.Tour;
      Framework.DomainModels.Models.IHalfTour halfTour = trip.HalfTour;

      // household inputs
      int childrenFlag = household.HasChildren.ToFlag();
      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();

      // person inputs
      int adultMaleFlag = person.IsAdultMale.ToFlag();
      int adultFemaleFlag = person.IsAdultFemale.ToFlag();
      int partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
      int retiredAdultFlag = person.IsRetiredAdult.ToFlag();
      int drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
      int nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
      int childAge5Through15Flag = person.IsChildAge5Through15.ToFlag();
      int childUnder5Flag = person.IsChildUnder5.ToFlag();

      // person-day inputs
      int homeBasedTours = personDay.HomeBasedTours;
      int simulatedToursFlag = personDay.SimulatedToursExist().ToFlag();

      int simulatedWorkStops = personDay.SimulatedWorkStops;
      int simulatedWorkStopsFlag = personDay.SimulatedWorkStopsExist().ToFlag();

      int simulatedSchoolStops = personDay.SimulatedSchoolStops;
      //            var simulatedSchoolStopsFlag = personDay.HasSimulatedSchoolStops.ToFlag();

      int simulatedEscortStops = personDay.SimulatedEscortStops;
      //            var simulatedEscortStopsFlag = personDay.HasSimulatedEscortStops.ToFlag();

      int simulatedPersonalBusinessStops = personDay.SimulatedPersonalBusinessStops;
      //            var simulatedPersonalBusinessStopsFlag = personDay.HasSimulatedPersonalBusinessStops.ToFlag();

      int simulatedShoppingStops = personDay.SimulatedShoppingStops;
      //            var simulatedShoppingStopsFlag = personDay.HasSimulatedShoppingStops.ToFlag();

      int simulatedMealStops = personDay.SimulatedMealStops;
      //            var simulatedMealStopsFlag = personDay.HasSimulatedMealStops.ToFlag();

      int simulatedSocialStops = personDay.SimulatedSocialStops;
      //            var simulatedSocialStopsFlag = personDay.HasSimulatedSocialStops.ToFlag();

      // tour inputs
      int hov2TourFlag = tour.IsHov2Mode().ToFlag();
      int hov3TourFlag = tour.IsHov3Mode().ToFlag();
      int transitTourFlag = tour.IsTransitMode().ToFlag();
      int bikeTourFlag = tour.IsBikeMode().ToFlag();
      int walkTourFlag = tour.IsWalkMode().ToFlag();
      int notHomeBasedTourFlag = (!tour.IsHomeBasedTour).ToFlag();
      int workTourFlag = tour.IsWorkPurpose().ToFlag();
      int personalBusinessOrMedicalTourFlag = tour.IsPersonalBusinessOrMedicalPurpose().ToFlag();
      int socialOrRecreationTourFlag = tour.IsSocialOrRecreationPurpose().ToFlag();
      int schoolTourFlag = tour.IsSchoolPurpose().ToFlag();
      int escortTourFlag = tour.IsEscortPurpose().ToFlag();
      int shoppingTourFlag = tour.IsShoppingPurpose().ToFlag();
      int mealTourFlag = tour.IsMealPurpose().ToFlag();

      // trip inputs
      int oneSimulatedTripFlag = halfTour.OneSimulatedTripFlag;
      int twoSimulatedTripsFlag = halfTour.TwoSimulatedTripsFlag;
      int threeSimulatedTripsFlag = halfTour.ThreeSimulatedTripsFlag;
      int fourSimulatedTripsFlag = halfTour.FourSimulatedTripsFlag;
      int fiveSimulatedTripsFlag = halfTour.FiveSimulatedTripsFlag;
      int halfTourFromOriginFlag = trip.IsHalfTourFromOrigin.ToFlag();
      int halfTourFromDestinationFlag = (!trip.IsHalfTourFromOrigin).ToFlag();
      int beforeMandatoryDestinationFlag = trip.IsBeforeMandatoryDestination().ToFlag();

      // remaining inputs
      int time = trip.IsHalfTourFromOrigin ? tour.DestinationArrivalTime : tour.DestinationDepartureTime;

      int from7AMto9AMFlag = (time >= Global.Settings.Times.SevenAM && time < Global.Settings.Times.NineAM).ToFlag();
      int from9AMto11AMFlag = (time >= Global.Settings.Times.NineAM && time < Global.Settings.Times.ElevenAM).ToFlag();
      int from11AMto1PMFlag = (time >= Global.Settings.Times.ElevenAM && time < Global.Settings.Times.OnePM).ToFlag();
      int from1PMto3PMFlag = (time >= Global.Settings.Times.OnePM && time < Global.Settings.Times.ThreePM).ToFlag();
      int from3PMto5PMFlag = (time >= Global.Settings.Times.ThreePM && time < Global.Settings.Times.FivePM).ToFlag();
      int from7PMto9PMFlag = (time >= Global.Settings.Times.SevenPM && time < Global.Settings.Times.NinePM).ToFlag();
      int from9PMto11PMFlag = (time >= Global.Settings.Times.NinePM && time < Global.Settings.Times.ElevenPM).ToFlag();
      int from11PMto7AMFlag = (time >= Global.Settings.Times.ElevenPM).ToFlag();

      double foodRetailServiceMedicalQtrMileLog = household.ResidenceParcel.FoodRetailServiceMedicalLogBuffer1();
      int remainingToursCount = personDay.HomeBasedTours - personDay.GetTotalSimulatedTours();

      // time-of-day for trip window calculations
      int startTime = trip.GetStartTime();

      // time window in minutes and in hours for yet unmodeled portion of half-tour, only consider persons on this trip
      int timeWindow =
                tour.IsHomeBasedTour
                    ? personDay.TimeWindow.AvailableWindow(startTime, Global.Settings.TimeDirections.Both)
                    : tour.ParentTour.TimeWindow.AvailableWindow(startTime, Global.Settings.TimeDirections.Both);

      double duration = timeWindow / 60D;

      // connectivity attributes
      double c34Ratio = trip.OriginParcel.C34RatioBuffer1();

      // 0 - NO MORE STOPS

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

      alternative.Choice = Global.Settings.Purposes.NoneOrHome;

      alternative.AddUtilityTerm(1, twoSimulatedTripsFlag * halfTourFromOriginFlag);
      alternative.AddUtilityTerm(2, threeSimulatedTripsFlag * halfTourFromOriginFlag);
      alternative.AddUtilityTerm(3, fourSimulatedTripsFlag * halfTourFromOriginFlag);
      alternative.AddUtilityTerm(4, fiveSimulatedTripsFlag * halfTourFromOriginFlag);
      alternative.AddUtilityTerm(5, twoSimulatedTripsFlag * halfTourFromDestinationFlag);
      alternative.AddUtilityTerm(6, threeSimulatedTripsFlag * halfTourFromDestinationFlag);
      alternative.AddUtilityTerm(7, fourSimulatedTripsFlag * halfTourFromDestinationFlag);
      alternative.AddUtilityTerm(8, fiveSimulatedTripsFlag * halfTourFromDestinationFlag);
      alternative.AddUtilityTerm(9, homeBasedTours);
      alternative.AddUtilityTerm(10, simulatedToursFlag);
      alternative.AddUtilityTerm(11, notHomeBasedTourFlag);
      alternative.AddUtilityTerm(12, beforeMandatoryDestinationFlag);
      alternative.AddUtilityTerm(16, (transitTourFlag + walkTourFlag + bikeTourFlag) * c34Ratio * foodRetailServiceMedicalQtrMileLog);
      alternative.AddUtilityTerm(17, transitTourFlag);
      alternative.AddUtilityTerm(40, schoolTourFlag);

      // 1 - WORK STOP

      if (personDay.WorkStops > 0 && tour.DestinationPurpose <= Global.Settings.Purposes.School) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, true, choice == Global.Settings.Purposes.Work);

        alternative.Choice = Global.Settings.Purposes.Work;

        alternative.AddUtilityTerm(33, workTourFlag + schoolTourFlag);
        //                alternative.AddUtility(47, escortTourFlag);
        //                alternative.AddUtility(54, personalBusinessOrMedicalTourFlag);
        //                alternative.AddUtility(61, shoppingTourFlag);
        //                alternative.AddUtility(68, mealTourFlag);
        //                alternative.AddUtility(75, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(82, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(89, simulatedWorkStops);
        alternative.AddUtilityTerm(96, simulatedWorkStopsFlag);
        alternative.AddUtilityTerm(103, remainingToursCount);
        alternative.AddUtilityTerm(110, duration);
        alternative.AddUtilityTerm(131, from9AMto11AMFlag + from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
        alternative.AddUtilityTerm(175, adultMaleFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, false, choice == Global.Settings.Purposes.Work);
      }

      // 2 - SCHOOL STOP

      if (personDay.SchoolStops > 0 && tour.DestinationPurpose <= Global.Settings.Purposes.School) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, true, choice == Global.Settings.Purposes.School);

        alternative.Choice = Global.Settings.Purposes.School;

        alternative.AddUtilityTerm(34, workTourFlag + schoolTourFlag);
        //                alternative.AddUtility(48, escortTourFlag);
        //                alternative.AddUtility(55, personalBusinessOrMedicalTourFlag);
        //                alternative.AddUtility(62, shoppingTourFlag);
        //                alternative.AddUtility(69, mealTourFlag);
        //                alternative.AddUtility(76, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(83, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(90, simulatedSchoolStops);
        //                alternative.AddUtility(97, simulatedSchoolStopsFlag);
        alternative.AddUtilityTerm(104, remainingToursCount);
        alternative.AddUtilityTerm(111, duration);
        alternative.AddUtilityTerm(138, from7AMto9AMFlag + from7PMto9PMFlag + from9PMto11PMFlag + from11PMto7AMFlag);
        alternative.AddUtilityTerm(237, oneSimulatedTripFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);
      }

      // 3 - ESCORT STOP

      if (personDay.EscortStops > 0 && tour.DestinationPurpose <= Global.Settings.Purposes.Escort) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, true, choice == Global.Settings.Purposes.Escort);

        alternative.Choice = Global.Settings.Purposes.Escort;

        alternative.AddUtilityTerm(35, workTourFlag + schoolTourFlag);
        alternative.AddUtilityTerm(49, escortTourFlag);
        //                alternative.AddUtility(56, personalBusinessOrMedicalTourFlag);
        //                alternative.AddUtility(63, shoppingTourFlag);
        //                alternative.AddUtility(70, mealTourFlag);
        //                alternative.AddUtility(77, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(84, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(91, simulatedEscortStops);
        //                alternative.AddUtility(98, simulatedEscortStopsFlag);
        alternative.AddUtilityTerm(105, remainingToursCount);
        alternative.AddUtilityTerm(112, duration);
        alternative.AddUtilityTerm(146, from7AMto9AMFlag);
        alternative.AddUtilityTerm(147, from9AMto11AMFlag + from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
        alternative.AddUtilityTerm(181, childrenFlag * adultFemaleFlag);
        alternative.AddUtilityTerm(183, hov2TourFlag);
        alternative.AddUtilityTerm(184, hov3TourFlag);
        alternative.AddUtilityTerm(238, oneSimulatedTripFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, false, choice == Global.Settings.Purposes.Escort);
      }

      // 4 - PERSONAL BUSINESS STOP

      if (personDay.PersonalBusinessStops > 0) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, true, choice == Global.Settings.Purposes.PersonalBusiness);

        alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

        alternative.AddUtilityTerm(36, workTourFlag + schoolTourFlag);
        alternative.AddUtilityTerm(50, escortTourFlag);
        alternative.AddUtilityTerm(57, personalBusinessOrMedicalTourFlag);
        alternative.AddUtilityTerm(64, shoppingTourFlag);
        alternative.AddUtilityTerm(71, mealTourFlag);
        alternative.AddUtilityTerm(78, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(85, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(92, simulatedPersonalBusinessStops);
        //                alternative.AddUtility(99, simulatedPersonalBusinessStopsFlag);
        alternative.AddUtilityTerm(106, remainingToursCount);
        alternative.AddUtilityTerm(113, duration);
        alternative.AddUtilityTerm(154, from7AMto9AMFlag + from7PMto9PMFlag + from9PMto11PMFlag + from11PMto7AMFlag);
        alternative.AddUtilityTerm(155, from9AMto11AMFlag + from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
        alternative.AddUtilityTerm(195, onePersonHouseholdFlag);
        alternative.AddUtilityTerm(196, hov2TourFlag);
        alternative.AddUtilityTerm(197, hov3TourFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, false, choice == Global.Settings.Purposes.PersonalBusiness);
      }

      // 5 - SHOPPING STOP

      if (personDay.ShoppingStops > 0) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, true, choice == Global.Settings.Purposes.Shopping);

        alternative.Choice = Global.Settings.Purposes.Shopping;

        alternative.AddUtilityTerm(37, workTourFlag + schoolTourFlag);
        alternative.AddUtilityTerm(51, escortTourFlag);
        alternative.AddUtilityTerm(58, personalBusinessOrMedicalTourFlag);
        alternative.AddUtilityTerm(65, shoppingTourFlag);
        alternative.AddUtilityTerm(72, mealTourFlag);
        alternative.AddUtilityTerm(79, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(86, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(93, simulatedShoppingStops);
        //                alternative.AddUtility(100, simulatedShoppingStopsFlag);
        alternative.AddUtilityTerm(107, remainingToursCount);
        alternative.AddUtilityTerm(114, duration);
        alternative.AddUtilityTerm(162, from7AMto9AMFlag + from9PMto11PMFlag + from11PMto7AMFlag);
        alternative.AddUtilityTerm(164, from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
        alternative.AddUtilityTerm(207, childrenFlag * adultFemaleFlag);
        alternative.AddUtilityTerm(209, hov2TourFlag);
        alternative.AddUtilityTerm(210, hov3TourFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, false, choice == Global.Settings.Purposes.Shopping);
      }

      // 6 - MEAL STOP

      if (personDay.MealStops > 0) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, true, choice == Global.Settings.Purposes.Meal);

        alternative.Choice = Global.Settings.Purposes.Meal;

        alternative.AddUtilityTerm(38, workTourFlag);
        alternative.AddUtilityTerm(46, schoolTourFlag);
        alternative.AddUtilityTerm(52, escortTourFlag);
        alternative.AddUtilityTerm(59, personalBusinessOrMedicalTourFlag);
        alternative.AddUtilityTerm(66, shoppingTourFlag);
        alternative.AddUtilityTerm(73, mealTourFlag);
        alternative.AddUtilityTerm(80, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(87, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(94, simulatedMealStops);
        //                alternative.AddUtility(101, simulatedMealStopsFlag);
        alternative.AddUtilityTerm(108, remainingToursCount);
        alternative.AddUtilityTerm(115, duration);
        alternative.AddUtilityTerm(170, from7AMto9AMFlag + from11PMto7AMFlag);
        alternative.AddUtilityTerm(171, from11AMto1PMFlag + from1PMto3PMFlag);
        alternative.AddUtilityTerm(172, from7PMto9PMFlag);
        alternative.AddUtilityTerm(221, onePersonHouseholdFlag);
        alternative.AddUtilityTerm(222, hov2TourFlag);
        alternative.AddUtilityTerm(223, hov3TourFlag);
        alternative.AddUtilityTerm(226, partTimeWorkerFlag + retiredAdultFlag + drivingAgeStudentFlag);
        alternative.AddUtilityTerm(228, nonworkingAdultFlag + childAge5Through15Flag + childUnder5Flag);
        alternative.AddUtilityTerm(239, oneSimulatedTripFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, false, choice == Global.Settings.Purposes.Meal);
      }

      // 7 - SOCIAL (OR RECREATION) STOP

      if (personDay.SocialStops > 0) {
        alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, true, choice == Global.Settings.Purposes.Social);

        alternative.Choice = Global.Settings.Purposes.Social;

        alternative.AddUtilityTerm(39, workTourFlag + schoolTourFlag);
        alternative.AddUtilityTerm(53, escortTourFlag);
        alternative.AddUtilityTerm(60, personalBusinessOrMedicalTourFlag);
        alternative.AddUtilityTerm(67, shoppingTourFlag);
        alternative.AddUtilityTerm(74, mealTourFlag);
        alternative.AddUtilityTerm(81, socialOrRecreationTourFlag);
        alternative.AddUtilityTerm(88, halfTourFromOriginFlag);
        alternative.AddUtilityTerm(95, simulatedSocialStops);
        //                alternative.AddUtility(102, simulatedSocialStopsFlag);
        alternative.AddUtilityTerm(109, remainingToursCount);
        alternative.AddUtilityTerm(116, duration);
        alternative.AddUtilityTerm(173, from7AMto9AMFlag + from11PMto7AMFlag);
        alternative.AddUtilityTerm(174, from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
        alternative.AddUtilityTerm(235, hov2TourFlag);
        alternative.AddUtilityTerm(236, hov3TourFlag);
      } else {
        choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, false, choice == Global.Settings.Purposes.Social);
      }
    }
  }
}