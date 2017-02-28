// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.PathTypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaySim.ChoiceModels.Default.Models {
    public class WorkBasedSubtourModeModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "WorkBasedSubtourModeModel";
        private const int TOTAL_NESTED_ALTERNATIVES = 5;
        private const int TOTAL_LEVELS = 2;
        private const int MAX_PARAMETER = 199;
        private const int THETA_PARAMETER = 99;

        private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 0, 0, 23 };
        private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 0, 0, 4 };

        public override void RunInitialize(ICoefficientsReader reader = null) {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkBasedSubtourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
        }

        public void Run(ITourWrapper subtour) {
            if (subtour == null) {
                throw new ArgumentNullException("subtour");
            }

            subtour.PersonDay.ResetRandom(40 + subtour.Sequence - 1);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return;
                }
                // JLB 20150331 added following to exclude records with missing coordinates from estimation 
                if (subtour.OriginParcelId == 0 || subtour.DestinationParcelId == 0) {
                    return;
                }
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(subtour.Id);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                if (subtour.DestinationParcel == null || subtour.Mode <= Global.Settings.Modes.None || subtour.Mode > Global.Settings.Modes.Transit) {
                    return;
                }

                IEnumerable<IPathTypeModel> pathTypeModels =
                    PathTypeModelFactory.Singleton.RunAll(
                    subtour.Household.RandomUtility,
                        subtour.OriginParcel,
                        subtour.DestinationParcel,
                        subtour.DestinationArrivalTime,
                        subtour.DestinationDepartureTime,
                        subtour.DestinationPurpose,
                        subtour.CostCoefficient,
                        subtour.TimeCoefficient,
                        subtour.Person.IsDrivingAge,
                        subtour.Household.VehiclesAvailable,
                        subtour.Household.OwnsAutomatedVehicles > 0,
                        subtour.Person.GetTransitFareDiscountFraction(),
                        false);

                var pathTypeModel = pathTypeModels.First(x => x.Mode == subtour.Mode);

                if (!pathTypeModel.Available) {
                    return;
                }

                RunModel(choiceProbabilityCalculator, subtour, pathTypeModels, subtour.DestinationParcel, subtour.ParentTour.Mode, subtour.Mode);

                choiceProbabilityCalculator.WriteObservation();
            } else {
                IEnumerable<IPathTypeModel> pathTypeModels =
                    PathTypeModelFactory.Singleton.RunAll(
                    subtour.Household.RandomUtility,
                        subtour.OriginParcel,
                        subtour.DestinationParcel,
                        subtour.DestinationArrivalTime,
                        subtour.DestinationDepartureTime,
                        subtour.DestinationPurpose,
                        subtour.CostCoefficient,
                        subtour.TimeCoefficient,
                        subtour.Person.IsDrivingAge,
                        subtour.Household.VehiclesAvailable,
                        subtour.Household.OwnsAutomatedVehicles > 0,
                        subtour.Person.GetTransitFareDiscountFraction(),
                        false);

                RunModel(choiceProbabilityCalculator, subtour, pathTypeModels, subtour.DestinationParcel, subtour.ParentTour.Mode);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(subtour.Household.RandomUtility);

                if (chosenAlternative == null) {
                    Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", subtour.PersonDay.Id);
                    subtour.Mode = Global.Settings.Modes.Hov3;
                    subtour.PersonDay.IsValid = false;
                    return;
                }
                var choice = (int)chosenAlternative.Choice;

                subtour.Mode = choice;
                if (choice == Global.Settings.Modes.SchoolBus || choice == Global.Settings.Modes.PaidRideShare) {
                    subtour.PathType = 0;
                } else {

                    var chosenPathType = pathTypeModels.First(x => x.Mode == choice);
                    subtour.PathType = chosenPathType.PathType;
                    subtour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
                }
            }
        }


        public ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper subtour, IParcelWrapper destinationParcel) {
            if (subtour == null) {
                throw new ArgumentNullException("subtour");
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

            IEnumerable<IPathTypeModel> pathTypeModels =
                PathTypeModelFactory.Singleton.RunAll(
                subtour.Household.RandomUtility,
                    subtour.OriginParcel,
                    destinationParcel,
                    subtour.DestinationArrivalTime,
                    subtour.DestinationDepartureTime,
                    subtour.DestinationPurpose,
                    subtour.CostCoefficient,
                    subtour.TimeCoefficient,
                    subtour.Person.IsDrivingAge,
                    subtour.Household.VehiclesAvailable,
                    subtour.Household.OwnsAutomatedVehicles > 0,
                    subtour.Person.GetTransitFareDiscountFraction(),
                    false);

            RunModel(choiceProbabilityCalculator, subtour, pathTypeModels, destinationParcel, subtour.ParentTour.Mode);

            return choiceProbabilityCalculator.SimulateChoice(subtour.Household.RandomUtility);
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper subtour, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int parentTourMode, int choice = Constants.DEFAULT_VALUE) {
            var household = subtour.Household;
            var person = subtour.Person;
            var personDay = subtour.PersonDay;

            // household inputs
            var income0To25KFlag = household.Has0To25KIncome.ToFlag();
            var income25To50KFlag = household.Has25To50KIncome.ToFlag();

            // person inputs
            var maleFlag = person.IsMale.ToFlag();

            // tour inputs
            var sovTourFlag = (parentTourMode == Global.Settings.Modes.Sov).ToFlag();
            var hov2TourFlag = (parentTourMode == Global.Settings.Modes.Hov2).ToFlag();
            var bikeTourFlag = (parentTourMode == Global.Settings.Modes.Bike).ToFlag();
            var walkTourFlag = (parentTourMode == Global.Settings.Modes.Walk).ToFlag();

            // remaining inputs
            //            var originParcel = subtour.OriginParcel;
            var parkingDuration = ChoiceModelUtility.GetParkingDuration(person.IsFulltimeWorker);
            var destinationParkingCost = destinationParcel.ParkingCostBuffer1(parkingDuration);

            double escortPercentage;
            double nonEscortPercentage;

            ChoiceModelUtility.SetEscortPercentages(personDay, out escortPercentage, out nonEscortPercentage, true);

            // paidRideShare is a special case  
            if (Global.Configuration.PaidRideShareModeIsAvailable) {
                var pathTypeExtra = pathTypeModels.First(x => x.Mode == Global.Settings.Modes.PaidRideShare);
                var modeExtra = Global.Settings.Modes.PaidRideShare;
                var availableExtra = pathTypeExtra.Available;
                var generalizedTimeLogsumExtra = pathTypeExtra.GeneralizedTimeLogsum;
                var distanceExtra = pathTypeExtra.PathDistance;

                var alternative = choiceProbabilityCalculator.GetAlternative(modeExtra-2, availableExtra, choice == modeExtra);
                alternative.Choice = modeExtra;

                alternative.AddNestedAlternative(_nestedAlternativeIds[modeExtra], _nestedAlternativeIndexes[modeExtra], THETA_PARAMETER);

                if (availableExtra)  {
                    var extraCostPerMile = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                        Global.Configuration.AV_PaidRideShare_ExtraCostPerDistanceUnit : Global.Configuration.PaidRideShare_ExtraCostPerDistanceUnit;
                    var fixedCostPerRide = Global.Configuration.AV_PaidRideShareModeUsesAVs ?
                        Global.Configuration.AV_PaidRideShare_FixedCostPerRide : Global.Configuration.PaidRideShare_FixedCostPerRide;
                    //    case Global.Settings.Modes.PaidRideShare
                    alternative.AddUtilityTerm(2, generalizedTimeLogsumExtra * subtour.TimeCoefficient);
                    alternative.AddUtilityTerm(2, distanceExtra * extraCostPerMile * subtour.CostCoefficient);
                    alternative.AddUtilityTerm(2, fixedCostPerRide * subtour.CostCoefficient);

                    var modeConstant = Global.Configuration.AV_PaidRideShareModeUsesAVs
                      ? Global.Configuration.AV_PaidRideShareModeConstant + Global.Configuration.AV_PaidRideShareAVOwnerCoefficient * (household.OwnsAutomatedVehicles > 0).ToFlag()
                      : Global.Configuration.PaidRideShare_ModeConstant;

                    alternative.AddUtilityTerm(90, modeConstant);
                    alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_Age26to35Coefficient * subtour.Person.AgeIsBetween26And35.ToFlag());
                    alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_Age18to25Coefficient * subtour.Person.AgeIsBetween18And25.ToFlag());
                    alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_AgeOver65Coefficient * (subtour.Person.Age >= 65).ToFlag());
                }
            }


            foreach (var pathTypeModel in pathTypeModels) {
                var mode = pathTypeModel.Mode;
                var available = pathTypeModel.Mode != Global.Settings.Modes.ParkAndRide && pathTypeModel.Available;
                var generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

                var alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
                alternative.Choice = mode;

                alternative.AddNestedAlternative(_nestedAlternativeIds[pathTypeModel.Mode], _nestedAlternativeIndexes[pathTypeModel.Mode], THETA_PARAMETER);

                if (!available) {
                    continue;
                }

                alternative.AddUtilityTerm(2, generalizedTimeLogsum * subtour.TimeCoefficient);

                if (mode == Global.Settings.Modes.Transit) {
                    alternative.AddUtilityTerm(20, 1);
                    //                        alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
                    //                        alternative.AddUtility(128, destinationParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(127, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(126, originParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(125, originParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(124, originParcel.MixedUse2Index1());
                    //                        alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
                    //                        alternative.AddUtility(122, Math.Log(originParcel.StopsTransitBuffer1+1));
                } else if (mode == Global.Settings.Modes.Hov3) {
                    alternative.AddUtilityTerm(1, (destinationParkingCost * subtour.CostCoefficient / ChoiceModelUtility.CPFACT3));
                    alternative.AddUtilityTerm(30, 1);
                    alternative.AddUtilityTerm(88, sovTourFlag);
                    alternative.AddUtilityTerm(89, hov2TourFlag);
                } else if (mode == Global.Settings.Modes.Hov2) {
                    alternative.AddUtilityTerm(1, (destinationParkingCost * subtour.CostCoefficient / ChoiceModelUtility.CPFACT2));
                    alternative.AddUtilityTerm(40, 1);
                    alternative.AddUtilityTerm(88, sovTourFlag);
                    alternative.AddUtilityTerm(89, hov2TourFlag);
                } else if (mode == Global.Settings.Modes.Sov) {
                    alternative.AddUtilityTerm(1, (destinationParkingCost * subtour.CostCoefficient));
                    alternative.AddUtilityTerm(50, 1);
                    alternative.AddUtilityTerm(54, income0To25KFlag);
                    alternative.AddUtilityTerm(55, income25To50KFlag);
                    alternative.AddUtilityTerm(58, sovTourFlag);
                    alternative.AddUtilityTerm(59, hov2TourFlag);
                } else if (mode == Global.Settings.Modes.Bike) {
                    alternative.AddUtilityTerm(60, 1);
                    alternative.AddUtilityTerm(61, maleFlag);
                    alternative.AddUtilityTerm(69, bikeTourFlag);
                    //                        alternative.AddUtility(169, destinationParcel.MixedUse4Index1());
                    //                        alternative.AddUtility(168, destinationParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(167, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(166, originParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(165, originParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(164, originParcel.MixedUse4Index1());
                } else if (mode == Global.Settings.Modes.Walk) {
                    alternative.AddUtilityTerm(70, 1);
                    alternative.AddUtilityTerm(79, walkTourFlag);
                    //                        alternative.AddUtility(179, destinationParcel.MixedUse4Index1());
                    //                        alternative.AddUtility(178, destinationParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(177, destinationParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(176, originParcel.NetIntersectionDensity1());
                    //                        alternative.AddUtility(175, originParcel.TotalEmploymentDensity1());
                    //                        alternative.AddUtility(174, originParcel.MixedUse4Index1());
                }
            }
        }
    }
}