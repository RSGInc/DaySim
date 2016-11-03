// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels;
using DaySim.DomainModels.Default;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.H.Models {
    public class IntermediateStopGenerationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HIntermediateStopGenerationModel";
        private const int TOTAL_ALTERNATIVES = 10;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 250;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.IntermediateStopGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public int Run(ITripWrapper trip, IHouseholdDayWrapper householdDay) {
            return Run(trip, householdDay, Global.Settings.Purposes.NoneOrHome);
        }

        public int Run(ITripWrapper trip, IHouseholdDayWrapper householdDay, int choice) {
            if (trip == null) {
                throw new ArgumentNullException("trip");
            }

            trip.PersonDay.ResetRandom(40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 20 + trip.Sequence - 1);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return choice;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(trip.Id);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                if (trip.OriginParcel == null) {
                    return Constants.DEFAULT_VALUE;
                }
                RunModel(choiceProbabilityCalculator, trip, householdDay, choice);

                choiceProbabilityCalculator.WriteObservation();
            } else {
                RunModel(choiceProbabilityCalculator, trip, householdDay);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);
                choice = (int)chosenAlternative.Choice;
            }

            return choice;
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITripWrapper trip, IHouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {
            var household = trip.Household;
            var person = trip.Person;
            var personDay = trip.PersonDay;
            var tour = trip.Tour;
            var halfTour = trip.HalfTour;
            var personDays = householdDay.PersonDays;

            var isJointTour = tour.JointTourSequence > 0 ? 1 : 0;
            var destinationParcel = tour.DestinationParcel;
            var fullJointHalfTour = ((trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.FullHalfTour1Sequence > 0)
                    || (trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.FullHalfTour2Sequence > 0)).ToFlag();
            var partialJointHalfTour = ((trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.PartialHalfTour1Sequence > 0)
                    || (trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.PartialHalfTour2Sequence > 0)).ToFlag();
            var isJoint = (isJointTour + fullJointHalfTour + partialJointHalfTour > 0) ? 1 : 0;
            var isIndividual = 1 - isJoint;

            //destination parcel variables
            var foodBuffer2 = 0.0;
            var totEmpBuffer2 = 0.0;
            var retailBuffer2 = 0.0;

            if (destinationParcel != null) {
                foodBuffer2 = Math.Log(1 + destinationParcel.EmploymentFoodBuffer2);
                totEmpBuffer2 = Math.Log(1 + destinationParcel.EmploymentTotalBuffer2);
                retailBuffer2 = Math.Log(1 + destinationParcel.EmploymentRetailBuffer2);
            }

            var carOwnership = person.GetCarOwnershipSegment();

            // household inputs
            var onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
            //var householdInc75KP = household.Has75KPlusIncome;

            var votALSegment = tour.GetVotALSegment();
            var transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();

            var totalAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];

            var homeFoodBuffer2 = Math.Log(1 + household.ResidenceParcel.EmploymentFoodBuffer2);
            var homeTotEmpBuffer2 = Math.Log(1 + household.ResidenceParcel.EmploymentTotalBuffer2);
            var homeRetailBuffer2 = Math.Log(1 + household.ResidenceParcel.EmploymentRetailBuffer2);

            // person-day inputs
            var homeBasedTours = personDay.HomeBasedTours;
            var simulatedToursFlag = personDay.SimulatedToursExist().ToFlag();
            var simulatedWorkStops = personDay.SimulatedWorkStops;
            var simulatedWorkStopsFlag = personDay.SimulatedWorkStopsExist().ToFlag();
            var simulatedSchoolStops = personDay.SimulatedSchoolStops;
            var simulatedEscortStops = personDay.SimulatedEscortStops;
            var simulatedPersonalBusinessStops = personDay.SimulatedPersonalBusinessStops;
            var simulatedShoppingStops = personDay.SimulatedShoppingStops;
            var simulatedMealStops = personDay.SimulatedMealStops;
            var simulatedSocialStops = personDay.SimulatedSocialStops;
            var simulatedRecreationStops = personDay.SimulatedRecreationStops;
            var simulatedMedicalStops = personDay.SimulatedMedicalStops;

            // tour inputs
            var hov2TourFlag = tour.IsHov2Mode().ToFlag();
            var hov3TourFlag = tour.IsHov3Mode().ToFlag();
            var transitTourFlag = tour.IsTransitMode().ToFlag();
            var notHomeBasedTourFlag = (!tour.IsHomeBasedTour).ToFlag();
            var workTourFlag = tour.IsWorkPurpose().ToFlag();
            var personalBusinessOrMedicalTourFlag = tour.IsPersonalBusinessOrMedicalPurpose().ToFlag();
            var socialTourFlag = tour.IsSocialPurpose().ToFlag();
            var socialOrRecreationTourFlag = tour.IsSocialOrRecreationPurpose().ToFlag();
            var schoolTourFlag = tour.IsSchoolPurpose().ToFlag();
            var escortTourFlag = tour.IsEscortPurpose().ToFlag();
            var shoppingTourFlag = tour.IsShoppingPurpose().ToFlag();
            var mealTourFlag = tour.IsMealPurpose().ToFlag();

            // trip inputs
            var oneSimulatedTripFlag = halfTour.OneSimulatedTripFlag;
            var twoSimulatedTripsFlag = halfTour.TwoSimulatedTripsFlag;
            var threeSimulatedTripsFlag = halfTour.ThreeSimulatedTripsFlag;
            var fourSimulatedTripsFlag = halfTour.FourSimulatedTripsFlag;
            var fivePlusSimulatedTripsFlag = halfTour.FivePlusSimulatedTripsFlag;  //JLB changed 5 to 5Plus
            var halfTourFromOriginFlag = trip.IsHalfTourFromOrigin.ToFlag();
            var halfTourFromDestinationFlag = (!trip.IsHalfTourFromOrigin).ToFlag();
            var beforeMandatoryDestinationFlag = trip.IsBeforeMandatoryDestination().ToFlag();

            // remaining inputs, including joint tour variables
            var time = trip.IsHalfTourFromOrigin ? tour.DestinationArrivalTime : tour.DestinationDepartureTime;

            var from7AMto9AMFlag = (time >= Global.Settings.Times.SevenAM && time < Global.Settings.Times.NineAM).ToFlag();
            var from9AMto11AMFlag = (time >= Global.Settings.Times.NineAM && time < Global.Settings.Times.ElevenAM).ToFlag();
            var from11AMto1PMFlag = (time >= Global.Settings.Times.ElevenAM && time < Global.Settings.Times.OnePM).ToFlag();
            var from1PMto3PMFlag = (time >= Global.Settings.Times.OnePM && time < Global.Settings.Times.ThreePM).ToFlag();
            var from3PMto5PMFlag = (time >= Global.Settings.Times.ThreePM && time < Global.Settings.Times.FivePM).ToFlag();
            var from7PMto9PMFlag = (time >= Global.Settings.Times.SevenPM && time < Global.Settings.Times.NinePM).ToFlag();
            var from9PMto11PMFlag = (time >= Global.Settings.Times.NinePM && time < Global.Settings.Times.ElevenPM).ToFlag();
            var from11PMto7AMFlag = (time >= Global.Settings.Times.ElevenPM).ToFlag();

            var remainingToursCount = personDay.HomeBasedTours - personDay.GetTotalSimulatedTours();

            // connectivity attributes
            var c34Ratio = trip.OriginParcel.C34RatioBuffer1();

            var adis = 0.0;
            var logDist = 0.0;
            var minute = DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Start;

            //distance from origin to destination
            if (tour.OriginParcel != null && tour.DestinationParcel != null) {
                if (trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
                    adis = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, tour.OriginParcel, tour.DestinationParcel).Variable;

                } else {
                    adis = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, destinationParcel, tour.OriginParcel).Variable;
                }
                logDist = Math.Log(1 + adis);
            }

            var shortestTravelTime = 0D;
            if (trip.Sequence > 1 && tour.OriginParcel != null && trip.OriginParcel != null) {
                if (trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
                    shortestTravelTime = ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, tour.OriginParcel, trip.OriginParcel).Variable;
                } else {
                    shortestTravelTime = ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov2, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, trip.OriginParcel, tour.OriginParcel).Variable;
                }
            }

            var windowDuration = Math.Max(1.0,
                trip.IsHalfTourFromOrigin // first trip in half tour, use tour destination time
                    ? trip.Sequence == 1
                          ? tour.DestinationArrivalTime - tour.EarliestOriginDepartureTime - tour.IndicatedTravelTimeToDestination
                          : trip.GetPreviousTrip().ArrivalTime - tour.EarliestOriginDepartureTime - shortestTravelTime
                    : trip.Sequence == 1
                          ? tour.LatestOriginArrivalTime - tour.DestinationDepartureTime - tour.IndicatedTravelTimeFromDestination
                          : tour.LatestOriginArrivalTime - trip.GetPreviousTrip().ArrivalTime - shortestTravelTime);

            var duration = windowDuration / 60D;

            IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();
            int numChildrenOnJointTour = 0;
            int numAdultsOnJointTour = 0;
            int totHHToursJT = 0;
            //int totHHStopsJT=0;

            //MB use calculated variables at tour level rather than redoing time window
            //TimeWindow timeWindow = new TimeWindow();
            if (tour.JointTourSequence > 0) {
                foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
                    var tInJoint = pDay.Tours.Find(t => t.JointTourSequence == tour.JointTourSequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        //timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                        totHHToursJT = personDay.HomeBasedTours + totHHToursJT;

                        if (pDay.Person.Age < 18) {
                            numChildrenOnJointTour++;
                        }

                        if (pDay.Person.Age >= 18) {
                            numAdultsOnJointTour++;
                        }

                    }
                }
            } else if (trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.FullHalfTour1Sequence > 0) {
                foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
                    var tInJoint = pDay.Tours.Find(t => t.FullHalfTour1Sequence == tour.FullHalfTour1Sequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        //timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                        totHHToursJT = personDay.HomeBasedTours + totHHToursJT;

                        if (pDay.Person.Age < 18) {
                            numChildrenOnJointTour++;
                        }

                        if (pDay.Person.Age >= 18) {
                            numAdultsOnJointTour++;
                        }

                    }
                }
            } else if (trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.FullHalfTour2Sequence > 0) {
                foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
                    var tInJoint = pDay.Tours.Find(t => t.FullHalfTour2Sequence == tour.FullHalfTour2Sequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        //timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                        totHHToursJT = personDay.HomeBasedTours + totHHToursJT;

                        if (pDay.Person.Age < 18) {
                            numChildrenOnJointTour++;
                        }

                        if (pDay.Person.Age >= 18) {
                            numAdultsOnJointTour++;
                        }
                    }
                }
            } else if (tour.ParentTour == null) {
                //timeWindow.IncorporateAnotherTimeWindow(personDay.TimeWindow);
            } else {
                //timeWindow.IncorporateAnotherTimeWindow(tour.ParentTour.TimeWindow);
            }

            //timeWindow.SetBusyMinutes(Global.Settings.Times.EndOfRelevantWindow, Global.Settings.Times.MinutesInADay + 1);

            // time window in minutes for yet unmodeled portion of halftour, only consider persons on this trip
            //var availableWindow = timeWindow.AvailableWindow(destinationDepartureTime, Global.Settings.TimeDirections.Both);

            //var duration = availableWindow/ 60D;


            // 0 - NO MORE STOPS

            var alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

            alternative.Choice = Global.Settings.Purposes.NoneOrHome;

            alternative.AddUtilityTerm(1, twoSimulatedTripsFlag * halfTourFromOriginFlag * isIndividual);
            alternative.AddUtilityTerm(2, threeSimulatedTripsFlag * halfTourFromOriginFlag * isIndividual);
            alternative.AddUtilityTerm(3, fourSimulatedTripsFlag * halfTourFromOriginFlag * isIndividual);
            alternative.AddUtilityTerm(4, fivePlusSimulatedTripsFlag * halfTourFromOriginFlag * isIndividual);
            alternative.AddUtilityTerm(5, twoSimulatedTripsFlag * halfTourFromDestinationFlag * isIndividual);
            alternative.AddUtilityTerm(6, threeSimulatedTripsFlag * halfTourFromDestinationFlag * isIndividual);
            alternative.AddUtilityTerm(7, fourSimulatedTripsFlag * halfTourFromDestinationFlag * isIndividual);
            alternative.AddUtilityTerm(8, fivePlusSimulatedTripsFlag * halfTourFromDestinationFlag * isIndividual);
            alternative.AddUtilityTerm(9, homeBasedTours * isIndividual);
            alternative.AddUtilityTerm(10, homeBasedTours * isJointTour);
            alternative.AddUtilityTerm(11, notHomeBasedTourFlag);
            //alternative.AddUtilityTerm(12, beforeMandatoryDestinationFlag*isJointTour);
            alternative.AddUtilityTerm(13, beforeMandatoryDestinationFlag);
            alternative.AddUtilityTerm(14, numAdultsOnJointTour);
            alternative.AddUtilityTerm(15, numChildrenOnJointTour);
            alternative.AddUtilityTerm(16, totHHToursJT);
            //alternative.AddUtilityTerm(17, totHHStopsJT);
            alternative.AddUtilityTerm(22, (threeSimulatedTripsFlag + fourSimulatedTripsFlag + fivePlusSimulatedTripsFlag) * halfTourFromOriginFlag * isJoint);
            alternative.AddUtilityTerm(26, threeSimulatedTripsFlag * halfTourFromDestinationFlag * isJoint);
            alternative.AddUtilityTerm(27, fourSimulatedTripsFlag * halfTourFromDestinationFlag * isJoint);
            alternative.AddUtilityTerm(28, fivePlusSimulatedTripsFlag * halfTourFromDestinationFlag * isJoint);
            alternative.AddUtilityTerm(29, totalAggregateLogsum);

            // 1 - WORK STOP

            if ((personDay.WorkStops > 0 && tour.DestinationPurpose <= Global.Settings.Purposes.School) || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, true, choice == Global.Settings.Purposes.Work);

                alternative.Choice = Global.Settings.Purposes.Work;

                //alternative.AddUtilityTerm(32, isIndividualTour);
                alternative.AddUtilityTerm(33, workTourFlag);
                alternative.AddUtilityTerm(34, schoolTourFlag);
                alternative.AddUtilityTerm(35, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(36, Math.Log(1 + simulatedWorkStops));
                alternative.AddUtilityTerm(37, simulatedWorkStopsFlag);
                alternative.AddUtilityTerm(38, (5 - simulatedWorkStops));
                alternative.AddUtilityTerm(39, duration);
                alternative.AddUtilityTerm(40, from9AMto11AMFlag + from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
                //alternative.AddUtilityTerm(42, logDist);
                //alternative.AddUtilityTerm(43, transitTourFlag);
                //alternative.AddUtilityTerm(44, (person.IsPartTimeWorker).ToFlag());
                alternative.AddUtilityTerm(46, totalAggregateLogsum);
                //alternative.AddUtilityTerm(47,totEmpBuffer2);
                alternative.AddUtilityTerm(48, hov2TourFlag + hov3TourFlag);

            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, false, choice == Global.Settings.Purposes.Work);
            }

            // 2 - SCHOOL STOP

            if ((personDay.SchoolStops > 0 && tour.DestinationPurpose <= Global.Settings.Purposes.School) || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, true, choice == Global.Settings.Purposes.School);

                alternative.Choice = Global.Settings.Purposes.School;

                alternative.AddUtilityTerm(51, workTourFlag);
                alternative.AddUtilityTerm(52, schoolTourFlag);
                //alternative.AddUtilityTerm(53, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(54, Math.Log(1 + simulatedSchoolStops));
                alternative.AddUtilityTerm(55, (3 - simulatedSchoolStops));
                //alternative.AddUtilityTerm(55, remainingToursCount);
                alternative.AddUtilityTerm(56, duration);
                alternative.AddUtilityTerm(57, from7AMto9AMFlag + from7PMto9PMFlag + from9PMto11PMFlag + from11PMto7AMFlag);
                alternative.AddUtilityTerm(58, oneSimulatedTripFlag);
                //alternative.AddUtilityTerm(59, logDist);
                alternative.AddUtilityTerm(61, fullJointHalfTour * numChildrenOnJointTour);
                alternative.AddUtilityTerm(65, (person.Age < 12).ToFlag());
                alternative.AddUtilityTerm(66, totalAggregateLogsum);
                //alternative.AddUtilityTerm(66,  (person.IsUniversityStudent).ToFlag());
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);
            }

            // 3 - ESCORT STOP

            if ((personDay.EscortStops > 0 && tour.DestinationPurpose <= Global.Settings.Purposes.Escort) || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, true, choice == Global.Settings.Purposes.Escort);

                alternative.Choice = Global.Settings.Purposes.Escort;

                alternative.AddUtilityTerm(71, workTourFlag + schoolTourFlag);
                alternative.AddUtilityTerm(72, isJointTour);
                alternative.AddUtilityTerm(74, escortTourFlag);
                //alternative.AddUtilityTerm(75, socialOrRecreationTourFlag);
                //alternative.AddUtilityTerm(76, remainingToursCount);
                alternative.AddUtilityTerm(77, duration);
                alternative.AddUtilityTerm(78, from7AMto9AMFlag);
                //alternative.AddUtilityTerm(79, from9AMto11AMFlag + from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
                alternative.AddUtilityTerm(81, hov2TourFlag);
                alternative.AddUtilityTerm(82, hov3TourFlag);
                alternative.AddUtilityTerm(83, Math.Log(1 + simulatedEscortStops * isJointTour));
                alternative.AddUtilityTerm(84, Math.Log(1 + simulatedEscortStops * isIndividual));
                //alternative.AddUtilityTerm(85, (3 - simulatedEscortStops));

                alternative.AddUtilityTerm(85, totalAggregateLogsum);
                alternative.AddUtilityTerm(86, fullJointHalfTour);
                //alternative.AddUtilityTerm(88, enrollmentK8Buffer2);
                alternative.AddUtilityTerm(89, numChildrenOnJointTour);
                alternative.AddUtilityTerm(90, halfTourFromOriginFlag);
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, false, choice == Global.Settings.Purposes.Escort);
            }

            // 4 - PERSONAL BUSINESS STOP

            if (personDay.PersonalBusinessStops > 0 || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, true, choice == Global.Settings.Purposes.PersonalBusiness);

                alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

                alternative.AddUtilityTerm(91, (workTourFlag + schoolTourFlag));
                alternative.AddUtilityTerm(92, isJointTour);
                alternative.AddUtilityTerm(93, escortTourFlag);
                alternative.AddUtilityTerm(94, personalBusinessOrMedicalTourFlag * isIndividual);
                alternative.AddUtilityTerm(95, shoppingTourFlag);
                alternative.AddUtilityTerm(96, mealTourFlag);
                alternative.AddUtilityTerm(97, socialOrRecreationTourFlag);
                alternative.AddUtilityTerm(98, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(99, Math.Log(1 + simulatedPersonalBusinessStops * isIndividual));
                alternative.AddUtilityTerm(100, Math.Log(1 + simulatedPersonalBusinessStops * isJointTour));
                alternative.AddUtilityTerm(104, (3 - simulatedPersonalBusinessStops));
                alternative.AddUtilityTerm(101, duration);
                alternative.AddUtilityTerm(102, (from7AMto9AMFlag + from7PMto9PMFlag + from9PMto11PMFlag + from11PMto7AMFlag));
                alternative.AddUtilityTerm(103, from9AMto11AMFlag + from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
                alternative.AddUtilityTerm(105, hov2TourFlag);
                alternative.AddUtilityTerm(106, hov3TourFlag);
                alternative.AddUtilityTerm(109, fullJointHalfTour);
                alternative.AddUtilityTerm(110, totEmpBuffer2);
                alternative.AddUtilityTerm(111, totalAggregateLogsum);
                alternative.AddUtilityTerm(112, personalBusinessOrMedicalTourFlag * isJointTour);

            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, false, choice == Global.Settings.Purposes.PersonalBusiness);
            }

            // 5 - SHOPPING STOP

            if (personDay.ShoppingStops > 0 || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, true, choice == Global.Settings.Purposes.Shopping);

                alternative.Choice = Global.Settings.Purposes.Shopping;

                alternative.AddUtilityTerm(121, workTourFlag + schoolTourFlag);
                alternative.AddUtilityTerm(122, isJointTour);
                alternative.AddUtilityTerm(123, escortTourFlag);
                alternative.AddUtilityTerm(124, personalBusinessOrMedicalTourFlag);
                alternative.AddUtilityTerm(125, shoppingTourFlag * isIndividual);
                alternative.AddUtilityTerm(126, mealTourFlag);
                alternative.AddUtilityTerm(127, socialOrRecreationTourFlag);
                alternative.AddUtilityTerm(128, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(129, Math.Log(1 + simulatedShoppingStops * isIndividual));
                alternative.AddUtilityTerm(130, Math.Log(1 + simulatedShoppingStops * isJointTour));
                alternative.AddUtilityTerm(131, duration);
                alternative.AddUtilityTerm(132, from7AMto9AMFlag + from9PMto11PMFlag + from11PMto7AMFlag);
                alternative.AddUtilityTerm(133, (from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag));
                //alternative.AddUtilityTerm(134, adultFemaleOnJointTour);
                alternative.AddUtilityTerm(135, hov2TourFlag);
                alternative.AddUtilityTerm(136, hov3TourFlag);
                alternative.AddUtilityTerm(137, Math.Log(1 + adis));
                alternative.AddUtilityTerm(138, shoppingTourFlag * isJointTour);
                alternative.AddUtilityTerm(140, totalAggregateLogsum);
                //alternative.AddUtilityTerm(141, retailBuffer2);
                //alternative.AddUtilityTerm(142, numChildrenOnJointTour);
                //alternative.AddUtilityTerm(143, (household.Has100KPlusIncome).ToFlag());
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, false, choice == Global.Settings.Purposes.Shopping);
            }

            // 6 - MEAL STOP

            if (personDay.MealStops > 0 || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, true, choice == Global.Settings.Purposes.Meal);

                alternative.Choice = Global.Settings.Purposes.Meal;

                alternative.AddUtilityTerm(151, workTourFlag);
                //alternative.AddUtilityTerm(152, isJointTour);
                alternative.AddUtilityTerm(153, schoolTourFlag);
                alternative.AddUtilityTerm(154, escortTourFlag);
                alternative.AddUtilityTerm(155, personalBusinessOrMedicalTourFlag);
                alternative.AddUtilityTerm(156, shoppingTourFlag);
                alternative.AddUtilityTerm(157, mealTourFlag);
                alternative.AddUtilityTerm(158, socialOrRecreationTourFlag);
                alternative.AddUtilityTerm(159, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(160, Math.Log(1 + simulatedMealStops * isIndividual));
                alternative.AddUtilityTerm(161, Math.Log(1 + simulatedMealStops * isJointTour));
                alternative.AddUtilityTerm(162, duration);
                alternative.AddUtilityTerm(164, from11AMto1PMFlag + from1PMto3PMFlag);
                alternative.AddUtilityTerm(166, onePersonHouseholdFlag);
                alternative.AddUtilityTerm(167, hov2TourFlag);
                alternative.AddUtilityTerm(168, hov3TourFlag);
                alternative.AddUtilityTerm(170, numChildrenOnJointTour);
                alternative.AddUtilityTerm(171, oneSimulatedTripFlag);
                alternative.AddUtilityTerm(172, Math.Log(1 + adis));
                alternative.AddUtilityTerm(174, fullJointHalfTour);
                alternative.AddUtilityTerm(175, foodBuffer2);
                //alternative.AddUtilityTerm(176, homeFoodBuffer2);
                //alternative.AddUtilityTerm(177, (household.Has100KPlusIncome).ToFlag());
                alternative.AddUtilityTerm(178, numAdultsOnJointTour);
                alternative.AddUtilityTerm(179, totalAggregateLogsum);
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, false, choice == Global.Settings.Purposes.Meal);
            }

            // 7 - SOCIAL STOP

            if (personDay.SocialStops > 0 || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, true, choice == Global.Settings.Purposes.Social);

                alternative.Choice = Global.Settings.Purposes.Social;

                alternative.AddUtilityTerm(181, workTourFlag + schoolTourFlag);
                alternative.AddUtilityTerm(182, isJointTour);
                alternative.AddUtilityTerm(183, escortTourFlag);
                alternative.AddUtilityTerm(184, personalBusinessOrMedicalTourFlag);
                alternative.AddUtilityTerm(185, shoppingTourFlag);
                alternative.AddUtilityTerm(186, mealTourFlag);
                alternative.AddUtilityTerm(187, socialOrRecreationTourFlag);
                alternative.AddUtilityTerm(188, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(189, Math.Log(1 + simulatedSocialStops * isIndividual));
                alternative.AddUtilityTerm(197, Math.Log(1 + simulatedSocialStops * isJointTour));
                alternative.AddUtilityTerm(190, remainingToursCount);
                alternative.AddUtilityTerm(191, duration);
                alternative.AddUtilityTerm(192, from7AMto9AMFlag + from11PMto7AMFlag);
                alternative.AddUtilityTerm(194, hov2TourFlag);
                alternative.AddUtilityTerm(195, hov3TourFlag);
                alternative.AddUtilityTerm(196, logDist);
                alternative.AddUtilityTerm(198, totalAggregateLogsum);
                alternative.AddUtilityTerm(200, numAdultsOnJointTour);
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, false, choice == Global.Settings.Purposes.Social);
            }


            // 8 - RECREATION STOP

            if (personDay.RecreationStops > 0 || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Recreation, true, choice == Global.Settings.Purposes.Recreation);

                alternative.Choice = Global.Settings.Purposes.Recreation;

                alternative.AddUtilityTerm(211, workTourFlag + schoolTourFlag);
                alternative.AddUtilityTerm(212, isJointTour);
                alternative.AddUtilityTerm(213, escortTourFlag);
                alternative.AddUtilityTerm(214, personalBusinessOrMedicalTourFlag);
                alternative.AddUtilityTerm(215, shoppingTourFlag);
                alternative.AddUtilityTerm(216, mealTourFlag);
                alternative.AddUtilityTerm(217, socialOrRecreationTourFlag);
                alternative.AddUtilityTerm(218, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(219, Math.Log(1 + simulatedRecreationStops * isIndividual));
                alternative.AddUtilityTerm(229, Math.Log(1 + simulatedRecreationStops * isJointTour));
                alternative.AddUtilityTerm(220, remainingToursCount);
                alternative.AddUtilityTerm(221, duration);
                alternative.AddUtilityTerm(222, from7AMto9AMFlag + from11PMto7AMFlag);
                alternative.AddUtilityTerm(223, from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
                alternative.AddUtilityTerm(225, hov3TourFlag);
                alternative.AddUtilityTerm(226, numChildrenOnJointTour);
                alternative.AddUtilityTerm(227, numAdultsOnJointTour);
                //alternative.AddUtilityTerm(228, fullJointHalfTour);
                //alternative.AddUtilityTerm(229, openSpaceBuffer2);
                alternative.AddUtilityTerm(228, totalAggregateLogsum);
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Recreation, false, choice == Global.Settings.Purposes.Recreation);
            }

            // 9 - MEDICAL STOP

            if (personDay.MedicalStops > 0 || isJoint > 0) {
                alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Medical, true, choice == Global.Settings.Purposes.Medical);

                alternative.Choice = Global.Settings.Purposes.Medical;

                alternative.AddUtilityTerm(231, workTourFlag + schoolTourFlag + escortTourFlag);
                alternative.AddUtilityTerm(232, isJointTour);
                alternative.AddUtilityTerm(233, personalBusinessOrMedicalTourFlag * isIndividual);
                alternative.AddUtilityTerm(234, personalBusinessOrMedicalTourFlag * isJointTour);
                alternative.AddUtilityTerm(235, shoppingTourFlag);
                alternative.AddUtilityTerm(236, mealTourFlag);
                alternative.AddUtilityTerm(237, socialOrRecreationTourFlag);
                alternative.AddUtilityTerm(238, halfTourFromOriginFlag);
                alternative.AddUtilityTerm(239, Math.Log(1 + simulatedMedicalStops * isJointTour));
                alternative.AddUtilityTerm(240, Math.Log(1 + simulatedMedicalStops * isIndividual));
                //alternative.AddUtilityTerm(240, fullJointHalfTour);
                alternative.AddUtilityTerm(241, duration);
                alternative.AddUtilityTerm(242, from7AMto9AMFlag + from11PMto7AMFlag);
                alternative.AddUtilityTerm(243, from11AMto1PMFlag + from1PMto3PMFlag + from3PMto5PMFlag);
                //alternative.AddUtilityTerm(248, numChildrenOnJointTour);
                //alternative.AddUtilityTerm(249, adultFemaleOnJointTour);
                alternative.AddUtilityTerm(244, totalAggregateLogsum);
            } else {
                choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Medical, false, choice == Global.Settings.Purposes.Medical);
            }


        }
    }
}