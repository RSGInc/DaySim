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
using System;

namespace DaySim.ChoiceModels.H.Models {
    public class WorkUsualModeAndScheduleModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "HWorkUsualModeAndScheduleModel";
        private const int TOTAL_NESTED_ALTERNATIVES = 4;
        private const int TOTAL_LEVELS = 2;
        private const int MAX_PARAMETER = 199;
        private const int THETA_PARAMETER = 99;

        private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 22, 0 };
        private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 3, 0 };

        private readonly ITourCreator _creator =
            Global
            .ContainerDaySim.GetInstance<IWrapperFactory<ITourCreator>>()
            .Creator;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public void Run(IPersonWrapper person) {
            if (person == null) {
                throw new ArgumentNullException("person");
            }

            person.ResetRandom(5);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return;
                }

                var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

                RunModel(choiceProbabilityCalculator, person);

                choiceProbabilityCalculator.WriteObservation();
            } else {

                var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

                RunModel(choiceProbabilityCalculator, person);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);

                if (chosenAlternative == null) {
                    Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", person.Id);
                    return;
                }

                var choice = (int)chosenAlternative.Choice;

            }
        }


        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonWrapper person) {

            var household = person.Household;
            var householdTotals = household.HouseholdTotals;

            // household inputs
            var childrenUnder5 = householdTotals.ChildrenUnder5;
            var childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
            var nonworkingAdults = householdTotals.NonworkingAdults;
            var retiredAdults = householdTotals.RetiredAdults;
            var onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
            var twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
            var income0To25KFlag = household.Has0To25KIncome.ToFlag();

            // person inputs
            var maleFlag = person.IsMale.ToFlag();
            var ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();

            var originParcel = household.ResidenceParcel;
            var destinationParcel = person.UsualWorkParcel;
            var parkingDuration = ChoiceModelUtility.GetParkingDuration(person.IsFulltimeWorker);
            // parking at work is free if no paid parking at work and tour goes to usual workplace
            var destinationParkingCost = (Global.Configuration.ShouldRunPayToParkAtWorkplaceModel && person.PaidParkingAtWorkplace == 0) ? 0.0 : destinationParcel.ParkingCostBuffer1(parkingDuration);

            var income = household.Income < 0 ? Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel : household.Income; // missing converted to 30K
            var incomeMultiple = Math.Min(Math.Max(income / Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel, .1), 10); // ranges for extreme values
            var incomePower = Global.Configuration.Coefficients_CostCoefficientIncomePower_Work;

            var costCoefficient = Global.Coefficients_BaseCostCoefficientPerMonetaryUnit / Math.Pow(incomeMultiple, incomePower);
            /*
                        var pathTypeModels =
                            PathTypeModelFactory.Model.RunAll(
                                household.RandomUtility,
                                originParcel,
                                destinationParcel,
                                240,
                                720,
                                Global.Settings.Purposes.Work,
                                costCoefficient,
                                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                                person.IsDrivingAge,
                                householdTotals.Adults, //assumes householdCars = adults
                                0.0, // assumes transitDiscountFraction = 0,
                                false);

                        foreach (var pathTypeModel in pathTypeModels) {
                            var mode = pathTypeModel.Mode;
                            var generalizedTime = pathTypeModel.GeneralizedTimeLogsum;
            //				var travelTime = pathTypeModel.PathTime;
            //				var travelCost = pathTypeModel.PathCost;

                            var available = pathTypeModel.Available; //&& (travelTime < longestWindow);

                            var alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
                            alternative.Choice = mode;

                            alternative.AddNestedAlternative(_nestedAlternativeIds[pathTypeModel.Mode], _nestedAlternativeIndexes[pathTypeModel.Mode], THETA_PARAMETER);

            //				if (mode == Global.Settings.Modes.ParkAndRide) {
            //					Console.WriteLine("Park and ride logsum = {0}", generalizedTimeLogsum);
            //				}

                            if (!available) {
                                continue;
                            }

                            alternative.AddUtilityTerm(2, generalizedTime * tour.TimeCoefficient);
            //				alternative.AddUtility(3, Math.Log(1.0 - travelTime / longestWindow));
            //				alternative.AddUtility(4, travelTime < longestWindow - expectedDurationCurrentTour ? Math.Log(1.0 - travelTime / (longestWindow - expectedDurationCurrentTour)) : 0); 
            //				alternative.AddUtility(5, travelTime < longestWindow - expectedDurationCurrentTour ? 0 : 1); 
            //				alternative.AddUtility(6, travelTime < totalWindow - totalExpectedDuration ? Math.Log(1.0 - travelTime / (totalWindow - totalExpectedDuration)) : 0); 
            //				alternative.AddUtility(7, travelTime < totalWindow - totalExpectedDuration ? 0 : 1); 
            //				var vot = tour.TimeCoefficient / tour.CostCoefficient; 

                            switch (mode) {
                                case Global.Settings.Modes.ParkAndRide:
                                    alternative.AddUtilityTerm(10, 1);
                                    alternative.AddUtilityTerm(11, noCarsInHouseholdFlag);
                                    alternative.AddUtilityTerm(13, carsLessThanWorkersFlag);
            //						alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
                                    alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
                                    alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));

                                    break;
                                case Global.Settings.Modes.Transit:
                                    alternative.AddUtilityTerm(20, 1);
            //						alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
                                    alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
                                    alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(126, originParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(125, originParcel.HouseholdDensity1());
                                    alternative.AddUtilityTerm(124, originParcel.MixedUse2Index1());
            //						alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
            //						alternative.AddUtility(122, Math.Log(originParcel.StopsTransitBuffer1+1));

                                    break;
                                case Global.Settings.Modes.Hov3:
                                    alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / Global.Configuration.Coefficients_HOV3CostDivisor_Work));
                                    alternative.AddUtilityTerm(30, 1);
                                    alternative.AddUtilityTerm(31, childrenUnder5);
                                    alternative.AddUtilityTerm(32, childrenAge5Through15);
            //						alternative.AddUtility(34, nonworkingAdults + retiredAdults);
                                    alternative.AddUtilityTerm(35, ((double)pathTypeModel.PathDistance).AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
                                    alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
                                    alternative.AddUtilityTerm(39, twoPersonHouseholdFlag);
                                    alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
                                    alternative.AddUtilityTerm(42, carsLessThanDriversFlag);
                                    alternative.AddUtilityTerm(133, escortPercentage);
                                    alternative.AddUtilityTerm(134, nonEscortPercentage);

                                    break;
                                case Global.Settings.Modes.Hov2:
                                    alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / Global.Configuration.Coefficients_HOV2CostDivisor_Work));
                                    alternative.AddUtilityTerm(31, childrenUnder5);
                                    alternative.AddUtilityTerm(32, childrenAge5Through15);
            //						alternative.AddUtility(34, nonworkingAdults + retiredAdults);
                                    alternative.AddUtilityTerm(35, ((double)pathTypeModel.PathDistance).AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
                                    alternative.AddUtilityTerm(40, 1);
                                    alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
                                    alternative.AddUtilityTerm(42, carsLessThanDriversFlag);
                                    alternative.AddUtilityTerm(48, onePersonHouseholdFlag);
                                    alternative.AddUtilityTerm(133, escortPercentage);
                                    alternative.AddUtilityTerm(134, nonEscortPercentage);

                                    break;
                                case Global.Settings.Modes.Sov:
                                    alternative.AddUtilityTerm(1, (destinationParkingCost) * tour.CostCoefficient);
                                    alternative.AddUtilityTerm(50, 1);
                                    alternative.AddUtilityTerm(53, carsLessThanWorkersFlag);
                                    alternative.AddUtilityTerm(54, income0To25KFlag);
                                    alternative.AddUtilityTerm(131, escortPercentage);
                                    alternative.AddUtilityTerm(132, nonEscortPercentage);

                                    break;
                                case Global.Settings.Modes.Bike:
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

            //						double worstDist = Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions ?
            //						 ImpedanceRoster.GetValue("worstdistance", mode, Global.Settings.PathTypes.FullNetwork, 
            //							Global.Settings.VotGroups.Medium, tour.DestinationArrivalTime,originParcel, destinationParcel).Variable : 0;

                                    alternative.AddUtilityTerm(60, 1);
                                    alternative.AddUtilityTerm(61, maleFlag);
                                    alternative.AddUtilityTerm(63, ageBetween51And98Flag);
                                    alternative.AddUtilityTerm(169, destinationParcel.MixedUse4Index1());
                                    alternative.AddUtilityTerm(168, destinationParcel.TotalEmploymentDensity1());
            //						alternative.AddUtility(167, destinationParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(166, originParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(165, originParcel.HouseholdDensity1());
                                    alternative.AddUtilityTerm(164, originParcel.MixedUse4Index1());
                                    alternative.AddUtilityTerm(162, (class1Dist > 0).ToFlag());
                                    alternative.AddUtilityTerm(162, (class2Dist > 0).ToFlag());
            //						alternative.AddUtility(163, (worstDist > 0).ToFlag());

                                    break;
                                case Global.Settings.Modes.Walk:
                                    alternative.AddUtilityTerm(71, maleFlag);
            //						alternative.AddUtility(73, ageBetween51And98Flag);
                                    alternative.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
            //						alternative.AddUtility(178, destinationParcel.TotalEmploymentDensity1());
            //						alternative.AddUtility(177, destinationParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(176, originParcel.NetIntersectionDensity1());
            //						alternative.AddUtility(175, originParcel.HouseholdDensity1());
                                    alternative.AddUtilityTerm(179, originParcel.MixedUse4Index1());

                                    break;
                            }
                        }
            */
        }
    }
}