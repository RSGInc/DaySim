// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaySim.ChoiceModels.Default.Models {
    public class WorkTourModeModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "WorkTourModeModel";
        private const int TOTAL_NESTED_ALTERNATIVES = 4;
        private const int TOTAL_LEVELS = 2;
        private const int MAX_PARAMETER = 199;
        private const int THETA_PARAMETER = 99;

        private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 22, 0, 23 };
        private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 3, 0, 2 };

        private readonly ITourCreator _creator =
            Global
            .ContainerDaySim.GetInstance<IWrapperFactory<ITourCreator>>()
            .Creator;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public void Run(ITourWrapper tour) {
            if (tour == null) {
                throw new ArgumentNullException("tour");
            }

            tour.PersonDay.ResetRandom(40 + tour.Sequence - 1);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                if (tour.DestinationParcel == null || tour.Mode <= Global.Settings.Modes.None || tour.Mode > Global.Settings.Modes.ParkAndRide) {
                    return;
                }

                IEnumerable<IPathTypeModel> pathTypeModels =
                    PathTypeModelFactory.Singleton.RunAllPlusParkAndRide(
                    tour.Household.RandomUtility,
                        tour.OriginParcel,
                        tour.DestinationParcel,
                        tour.DestinationArrivalTime,
                        tour.DestinationDepartureTime,
                        tour.DestinationPurpose,
                        tour.CostCoefficient,
                        tour.TimeCoefficient,
                        tour.Person.IsDrivingAge,
                        tour.Household.VehiclesAvailable,
                        tour.Person.GetTransitFareDiscountFraction(),
                        false);

                var pathTypeModel = pathTypeModels.First(x => x.Mode == tour.Mode);

                if (!pathTypeModel.Available) {
                    return;
                }

                RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Household.VehiclesAvailable, tour.Mode);

                choiceProbabilityCalculator.WriteObservation();
            } else {
                IEnumerable<IPathTypeModel> pathTypeModels =
                    PathTypeModelFactory.Singleton.RunAllPlusParkAndRide(
                    tour.Household.RandomUtility,
                        tour.OriginParcel,
                        tour.DestinationParcel,
                        tour.DestinationArrivalTime,
                        tour.DestinationDepartureTime,
                        tour.DestinationPurpose,
                        tour.CostCoefficient,
                        tour.TimeCoefficient,
                        tour.Person.IsDrivingAge,
                        tour.Household.VehiclesAvailable,
                        tour.Person.GetTransitFareDiscountFraction(),
                        false);

                RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Household.VehiclesAvailable);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

                if (chosenAlternative == null) {
                    Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
                    tour.Mode = Global.Settings.Modes.Hov3;
                    tour.PersonDay.IsValid = false;
                    return;
                }

                var choice = (int)chosenAlternative.Choice;
                tour.Mode = choice;

                if (choice == Global.Settings.Modes.SchoolBus || choice == Global.Settings.Modes.PaidRideShare) {
                    tour.PathType = 0;
                } else {
                    var chosenPathType = pathTypeModels.First(x => x.Mode == choice);
                    tour.PathType = chosenPathType.PathType;
                    if (choice == Global.Settings.Modes.ParkAndRide) {
                        tour.ParkAndRideNodeId = chosenPathType.PathParkAndRideNodeId;
                        tour.ParkAndRidePathType = chosenPathType.PathType;
                        tour.ParkAndRideTransitTime = chosenPathType.PathParkAndRideTransitTime;
                        tour.ParkAndRideTransitDistance = chosenPathType.PathParkAndRideTransitDistance;
                        tour.ParkAndRideTransitCost = chosenPathType.PathParkAndRideTransitCost;
                        tour.ParkAndRideWalkAccessEgressTime = chosenPathType.PathParkAndRideWalkAccessEgressTime;
                        tour.ParkAndRideTransitGeneralizedTime = chosenPathType.PathParkAndRideTransitGeneralizedTime;
                        if (Global.StopAreaIsEnabled) {
                            tour.ParkAndRideOriginStopAreaKey = chosenPathType.PathOriginStopAreaKey;
                            tour.ParkAndRideDestinationStopAreaKey = chosenPathType.PathDestinationStopAreaKey;
                        }
                    }
                }
            }
        }

        public ChoiceProbabilityCalculator.Alternative RunNested(IPersonWrapper person, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
            if (person == null) {
                throw new ArgumentNullException("person");
            }

            var tour = _creator.CreateWrapper(person, null, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);

            return RunNested(tour, destinationParcel, householdCars, 0.0);
        }

        public ChoiceProbabilityCalculator.Alternative RunNested(IPersonDayWrapper personDay, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
            if (personDay == null) {
                throw new ArgumentNullException("personDay");
            }

            var tour = _creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);

            return RunNested(tour, destinationParcel, householdCars, personDay.Person.GetTransitFareDiscountFraction());
        }

        public ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel, int householdCars, double transitDiscountFraction) {
            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

            IEnumerable<IPathTypeModel> pathTypeModels =
                PathTypeModelFactory.Singleton.RunAll(
                tour.Household.RandomUtility,
                    tour.OriginParcel,
                    destinationParcel,
                    tour.DestinationArrivalTime,
                    tour.DestinationDepartureTime,
                    tour.DestinationPurpose,
                    tour.CostCoefficient,
                    tour.TimeCoefficient,
                    tour.Person.IsDrivingAge,
                    householdCars,
                    transitDiscountFraction,
                    false);

            RunModel(choiceProbabilityCalculator, tour, pathTypeModels, destinationParcel, householdCars);

            return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
        }

        protected virtual void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int mode) {
            //see PSRC customization dll for example
            //Global.PrintFile.WriteLine("Generic Default WorkTourModeModel.RegionSpecificCustomizations being called so must not be overridden by CustomizationDll");
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int householdCars, int choice = Constants.DEFAULT_VALUE) {
            var household = tour.Household;
            var householdTotals = household.HouseholdTotals;
            var personDay = tour.PersonDay;
            var person = tour.Person;

            // household inputs
            var childrenUnder5 = householdTotals.ChildrenUnder5;
            var childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
            //            var nonworkingAdults = householdTotals.NonworkingAdults;
            //            var retiredAdults = householdTotals.RetiredAdults;
            var onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
            var twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
            var noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
            var carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);
            var carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(householdCars);
            var income0To25KFlag = household.Has0To25KIncome.ToFlag();
            var income75kPlusFlag = household.Has75KPlusIncome.ToFlag();

            // person inputs
            var maleFlag = person.IsMale.ToFlag();
            var ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();
            var univStudentFlag = person.IsUniversityStudent.ToFlag();

            var originParcel = tour.OriginParcel;
            var parkingDuration = ChoiceModelUtility.GetParkingDuration(person.IsFulltimeWorker);
            // parking at work is free if no paid parking at work and tour goes to usual workplace
            var destinationParkingCost = (Global.Configuration.ShouldRunPayToParkAtWorkplaceModel && tour.Person.UsualWorkParcel != null
                                          && destinationParcel == tour.Person.UsualWorkParcel && person.PaidParkingAtWorkplace == 0) ? 0.0 : destinationParcel.ParkingCostBuffer1(parkingDuration);

            double escortPercentage;
            double nonEscortPercentage;

            ChoiceModelUtility.SetEscortPercentages(personDay, out escortPercentage, out nonEscortPercentage);

            //            var timeWindow = (originParcel == tour.Household.ResidenceParcel) ? personDay.TimeWindow : tour.ParentTour.TimeWindow;
            //            var longestWindow = timeWindow.MaxAvailableMinutesAfter(1);
            //            var totalWindow = timeWindow.TotalAvailableMinutesAfter(1);
            //            var expectedDurationCurrentTour = person.IsFulltimeWorker ? Global.Settings.Times.EightHours : Global.Settings.Times.FourHours;
            //            var expectedDurationOtherTours = (personDay.TotalTours - personDay.TotalSimulatedTours) * Global.Settings.Times.TwoHours;
            //            var expectedDurationStops = (Math.Min(personDay.TotalStops,1) - Math.Min(personDay.TotalSimulatedStops,1)) * Global.Settings.Times.OneHour;
            //            var totalExpectedDuration = expectedDurationCurrentTour + expectedDurationOtherTours + expectedDurationStops;

            // paidRideShare is a special case  - set in config file - use HOV2 impedance 
            if (Global.Configuration.SetPaidRideShareModeAvailable) {
                var pathTypeExtra = pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Hov2);
                var modeExtra = Global.Settings.Modes.PaidRideShare;
                var availableExtra = pathTypeExtra.Available;
                var generalizedTimeLogsumExtra = pathTypeExtra.GeneralizedTimeLogsum;
                var distanceExtra = pathTypeExtra.PathDistance;

                var alternative = choiceProbabilityCalculator.GetAlternative(modeExtra, availableExtra, choice == modeExtra);
                alternative.Choice = modeExtra;

                alternative.AddNestedAlternative(_nestedAlternativeIds[modeExtra], _nestedAlternativeIndexes[modeExtra], THETA_PARAMETER);

                if (availableExtra) {
                    //    case Global.Settings.Modes.PaidRideShare
                    alternative.AddUtilityTerm(2, generalizedTimeLogsumExtra * tour.TimeCoefficient);
                    alternative.AddUtilityTerm(2, distanceExtra * Global.Configuration.PaidRideShare_ExtraCostPerDistanceUnit * tour.CostCoefficient);
                    alternative.AddUtilityTerm(2, Global.Configuration.PaidRideShare_FixedCostPerRide * tour.CostCoefficient);

                    alternative.AddUtilityTerm(90, 1);
                    alternative.AddUtilityTerm(91, tour.Person.AgeIsBetween18And25.ToFlag());
                    alternative.AddUtilityTerm(92, tour.Person.AgeIsBetween26And35.ToFlag());
                    alternative.AddUtilityTerm(93, tour.Person.IsYouth.ToFlag());
                }
            }


            foreach (var pathTypeModel in pathTypeModels) {
                var mode = pathTypeModel.Mode;
                var generalizedTime = pathTypeModel.GeneralizedTimeLogsum;
                //                var travelTime = pathTypeModel.PathTime;
                //                var travelCost = pathTypeModel.PathCost;

                var available = pathTypeModel.Available; //&& (travelTime < longestWindow);

                var alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
                alternative.Choice = mode;

                alternative.AddNestedAlternative(_nestedAlternativeIds[pathTypeModel.Mode], _nestedAlternativeIndexes[pathTypeModel.Mode], THETA_PARAMETER);

                //                if (mode == Global.Settings.Modes.ParkAndRide) {
                //                    Console.WriteLine("Park and ride logsum = {0}", generalizedTimeLogsum);
                //                }

                if (!available) {
                    continue;
                }

                alternative.AddUtilityTerm(2, generalizedTime * tour.TimeCoefficient);
                //                alternative.AddUtility(3, Math.Log(1.0 - travelTime / longestWindow));
                //                alternative.AddUtility(4, travelTime < longestWindow - expectedDurationCurrentTour ? Math.Log(1.0 - travelTime / (longestWindow - expectedDurationCurrentTour)) : 0); 
                //                alternative.AddUtility(5, travelTime < longestWindow - expectedDurationCurrentTour ? 0 : 1); 
                //                alternative.AddUtility(6, travelTime < totalWindow - totalExpectedDuration ? Math.Log(1.0 - travelTime / (totalWindow - totalExpectedDuration)) : 0); 
                //                alternative.AddUtility(7, travelTime < totalWindow - totalExpectedDuration ? 0 : 1); 
                //                var vot = tour.TimeCoefficient / tour.CostCoefficient; 

                if (mode == Global.Settings.Modes.ParkAndRide) {
                    alternative.AddUtilityTerm(10, 1);
                    alternative.AddUtilityTerm(11, noCarsInHouseholdFlag);
                    alternative.AddUtilityTerm(13, carsLessThanWorkersFlag);
                    //                        alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
                    alternative.AddUtilityTerm(130, Math.Log(destinationParcel.TotalEmploymentDensity1() + 1) * 2553.0 / Math.Log(2553.0));
                    alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
                    alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
                } else if (mode == Global.Settings.Modes.Transit) {
                    alternative.AddUtilityTerm(20, 1);
                    //                        alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
                    alternative.AddUtilityTerm(130, Math.Log(destinationParcel.TotalEmploymentDensity1() + 1) * 2553.0 / Math.Log(2553.0));
                    alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
                    alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(126, originParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(125, originParcel.HouseholdDensity1());
                    alternative.AddUtilityTerm(124, originParcel.MixedUse2Index1());
                    //                        alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
                    //                        alternative.AddUtility(122, Math.Log(originParcel.StopsTransitBuffer1+1));
                    alternative.AddUtilityTerm(180, univStudentFlag);
                    alternative.AddUtilityTerm(100, income75kPlusFlag);
                } else if (mode == Global.Settings.Modes.Hov3) {
                    alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / Global.Configuration.Coefficients_HOV3CostDivisor_Work));
                    alternative.AddUtilityTerm(30, 1);
                    alternative.AddUtilityTerm(31, childrenUnder5);
                    alternative.AddUtilityTerm(32, childrenAge5Through15);
                    //                        alternative.AddUtility(34, nonworkingAdults + retiredAdults);
                    alternative.AddUtilityTerm(35, ((double)pathTypeModel.PathDistance).AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
                    alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
                    alternative.AddUtilityTerm(39, twoPersonHouseholdFlag);
                    alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
                    alternative.AddUtilityTerm(42, carsLessThanDriversFlag);
                    alternative.AddUtilityTerm(133, escortPercentage);
                    alternative.AddUtilityTerm(134, nonEscortPercentage);
                } else if (mode == Global.Settings.Modes.Hov2) {
                    alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / Global.Configuration.Coefficients_HOV2CostDivisor_Work));
                    alternative.AddUtilityTerm(31, childrenUnder5);
                    alternative.AddUtilityTerm(32, childrenAge5Through15);
                    //                        alternative.AddUtility(34, nonworkingAdults + retiredAdults);
                    alternative.AddUtilityTerm(35, ((double)pathTypeModel.PathDistance).AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
                    alternative.AddUtilityTerm(40, 1);
                    alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
                    alternative.AddUtilityTerm(42, carsLessThanDriversFlag);
                    alternative.AddUtilityTerm(48, onePersonHouseholdFlag);
                    alternative.AddUtilityTerm(133, escortPercentage);
                    alternative.AddUtilityTerm(134, nonEscortPercentage);
                } else if (mode == Global.Settings.Modes.Sov) {
                    alternative.AddUtilityTerm(1, (destinationParkingCost) * tour.CostCoefficient);
                    alternative.AddUtilityTerm(50, 1);
                    alternative.AddUtilityTerm(53, carsLessThanWorkersFlag);
                    alternative.AddUtilityTerm(54, income0To25KFlag);
                    alternative.AddUtilityTerm(131, escortPercentage);
                    alternative.AddUtilityTerm(132, nonEscortPercentage);
                } else if (mode == Global.Settings.Modes.Bike) {
                    double class1Dist
                        = Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
                            ? ImpedanceRoster.GetValue("class1distance", mode, Global.Settings.PathTypes.FullNetwork,
                                Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
                            : 0;

                    double class2Dist =
                        Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
                            ? ImpedanceRoster.GetValue("class2distance", mode, Global.Settings.PathTypes.FullNetwork,
                                Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
                            : 0;

                    //                  double worstDist = Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions ?
                    //                         ImpedanceRoster.GetValue("worstdistance", mode, Global.Settings.PathTypes.FullNetwork, 
                    //                            Global.Settings.VotGroups.Medium, tour.DestinationArrivalTime,originParcel, destinationParcel).Variable : 0;

                    alternative.AddUtilityTerm(60, 1);
                    alternative.AddUtilityTerm(61, maleFlag);
                    alternative.AddUtilityTerm(63, ageBetween51And98Flag);
                    alternative.AddUtilityTerm(169, destinationParcel.MixedUse4Index1());
                    alternative.AddUtilityTerm(168, destinationParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(167, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(166, originParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(165, originParcel.HouseholdDensity1());
                    alternative.AddUtilityTerm(164, originParcel.MixedUse4Index1());
                    alternative.AddUtilityTerm(162, (class1Dist > 0).ToFlag());
                    alternative.AddUtilityTerm(162, (class2Dist > 0).ToFlag());
                    //                        alternative.AddUtility(163, (worstDist > 0).ToFlag());
                } else if (mode == Global.Settings.Modes.Walk) {
                    alternative.AddUtilityTerm(70, 1);
                    alternative.AddUtilityTerm(71, maleFlag);
                    //                        alternative.AddUtility(73, ageBetween51And98Flag);
                    alternative.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
                    //                        alternative.AddUtility(178, destinationParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(177, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(176, originParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(175, originParcel.HouseholdDensity1());
                    alternative.AddUtilityTerm(179, originParcel.MixedUse4Index1());
                }

                RegionSpecificCustomizations(alternative, tour, mode);
            }
        }
    }
}