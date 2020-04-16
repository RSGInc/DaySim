﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.Default.Models {
  public class WorkTourModeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "WorkTourModeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 5;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 333;
    private const int THETA_PARAMETER = 99;

    private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 22, 0, 23 };
    private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 3, 0, 4 };

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

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        if (tour.DestinationParcel == null ||
        tour.Mode <= Global.Settings.Modes.None ||
        tour.Mode == Global.Settings.Modes.SchoolBus ||
       (tour.Mode == Global.Settings.Modes.PaidRideShare && !Global.Configuration.PaidRideShareModeIsAvailable) ||
        tour.Mode > Global.Settings.Modes.PaidRideShare) {
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
                 tour.Person.TransitPassOwnership,
                tour.Household.OwnsAutomatedVehicles > 0,
                tour.Person.GetTransitFareDiscountFraction(),
                false);

        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == tour.Mode);

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
               tour.Person.TransitPassOwnership,
                tour.Household.OwnsAutomatedVehicles > 0,
                tour.Person.GetTransitFareDiscountFraction(),
                false);

        RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Household.VehiclesAvailable);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
          tour.Mode = Global.Settings.Modes.Hov3;
          tour.PersonDay.IsValid = false;
          return;
        }

        int choice = (int)chosenAlternative.Choice;
        tour.Mode = choice;

        if (choice == Global.Settings.Modes.SchoolBus || choice == Global.Settings.Modes.PaidRideShare) {
          tour.PathType = 0;
        } else {
          IPathTypeModel chosenPathType = pathTypeModels.First(x => x.Mode == choice);
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

      ITourWrapper tour = _creator.CreateWrapper(person, null, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);

      return RunNested(tour, destinationParcel, householdCars, 0.0);
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(IPersonDayWrapper personDay, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      ITourWrapper tour = _creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.Work);

      return RunNested(tour, destinationParcel, householdCars, personDay.Person.GetTransitFareDiscountFraction());
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel, int householdCars, double transitDiscountFraction) {
      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

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
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              transitDiscountFraction,
              false);

      RunModel(choiceProbabilityCalculator, tour, pathTypeModels, destinationParcel, householdCars);

      return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
    }

    protected virtual void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {
      //see PSRC customization dll for example
      //Global.PrintFile.WriteLine("Generic Default WorkTourModeModel.RegionSpecificCustomizations being called so must not be overridden by CustomizationDll");
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int householdCars, int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = tour.Household;
      Framework.DomainModels.Models.IHouseholdTotals householdTotals = household.HouseholdTotals;
      IPersonDayWrapper personDay = tour.PersonDay;
      IPersonWrapper person = tour.Person;

      // household inputs
      int childrenUnder5 = householdTotals.ChildrenUnder5;
      int childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      //            var nonworkingAdults = householdTotals.NonworkingAdults;
      //            var retiredAdults = householdTotals.RetiredAdults;
      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);
      int carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(householdCars);
      int income0To25KFlag = household.Has0To25KIncome.ToFlag();
      int income75kPlusFlag = household.Has75KPlusIncome.ToFlag();
      int incomeOver100Flag = household.Has100KPlusIncome.ToFlag();

      // person inputs
      int maleFlag = person.IsMale.ToFlag();
      int ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();
      int univStudentFlag = person.IsUniversityStudent.ToFlag();

      IParcelWrapper originParcel = tour.OriginParcel;
      int parkingDuration = ChoiceModelUtility.GetParkingDuration(person.IsFulltimeWorker);
      // parking at work is free if no paid parking at work and tour goes to usual workplace
      double destinationParkingCost = (Global.Configuration.ShouldRunPayToParkAtWorkplaceModel && tour.Person.UsualWorkParcel != null
                                          && destinationParcel == tour.Person.UsualWorkParcel && person.PaidParkingAtWorkplace == 0) ? 0.0 : destinationParcel.ParkingCostBuffer1(parkingDuration);

      ChoiceModelUtility.SetEscortPercentages(personDay, out double escortPercentage, out double nonEscortPercentage);

      foreach (IPathTypeModel pathTypeModel in pathTypeModels) {
        int mode = pathTypeModel.Mode;
        double generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

        bool available = (pathTypeModel.Mode != Global.Settings.Modes.PaidRideShare || Global.Configuration.PaidRideShareModeIsAvailable)
                    && pathTypeModel.Available;

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
        alternative.Choice = mode;

        alternative.AddNestedAlternative(_nestedAlternativeIds[pathTypeModel.Mode], _nestedAlternativeIndexes[pathTypeModel.Mode], THETA_PARAMETER);

        if (!available) {
          continue;
        }

        double modeTimeCoefficient = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0 && mode >= Global.Settings.Modes.Sov && mode <= Global.Settings.Modes.Hov3) ?
                     tour.TimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) :
                     (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs) ?
                     tour.TimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : tour.TimeCoefficient;
        alternative.AddUtilityTerm(2, generalizedTimeLogsum * modeTimeCoefficient);

        if (mode == Global.Settings.Modes.ParkAndRide) {
          alternative.AddUtilityTerm(10, 1);
          alternative.AddUtilityTerm(11, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(13, carsLessThanWorkersFlag);
          //                        alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
          alternative.AddUtilityTerm(130, Math.Log(destinationParcel.TotalEmploymentDensity1() + 1) * 2553.0 / Math.Log(2553.0));
          alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
          //alternative.AddUtilityTerm(120, Math.Log(destinationParcel.TotalEmploymentDensity1() + 1) * 2553.0 / Math.Log(2553.0));
          alternative.AddUtilityTerm(118, destinationParcel.TotalEmploymentDensity1());
          //alternative.AddUtilityTerm(117, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(113, Math.Log(destinationParcel.StopsTransitBuffer1 + 1));
          alternative.AddUtilityTerm(112, Math.Log(originParcel.StopsTransitBuffer1 + 1));
          alternative.AddUtilityTerm(115, Math.Log(pathTypeModel.PathParkAndRideNodeCapacity+1));

        } else if (mode == Global.Settings.Modes.Transit) {
          alternative.AddUtilityTerm(20, 1);
          alternative.AddUtilityTerm(21, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(22, carsLessThanDriversFlag); //for calibration
                                                                   //                        alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
          alternative.AddUtilityTerm(130, Math.Log(destinationParcel.TotalEmploymentDensity1() + 1) * 2553.0 / Math.Log(2553.0));
          alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(126, originParcel.NetIntersectionDensity1());
          //alternative.AddUtilityTerm(125, originParcel.HouseholdDensity1());
          //alternative.AddUtilityTerm(124, originParcel.MixedUse2Index1());
          alternative.AddUtilityTerm(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
          alternative.AddUtilityTerm(122, Math.Log(originParcel.StopsTransitBuffer1+1));
          alternative.AddUtilityTerm(180, univStudentFlag);
          alternative.AddUtilityTerm(100, income75kPlusFlag);

        } else if (mode == Global.Settings.Modes.Hov3) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / Global.Configuration.Coefficients_HOV3CostDivisor_Work));
          alternative.AddUtilityTerm(30, 1);
          alternative.AddUtilityTerm(31, childrenUnder5);
          alternative.AddUtilityTerm(32, childrenAge5Through15);
          //                       alternative.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
          alternative.AddUtilityTerm(35, pathTypeModel.PathDistance.AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
          alternative.AddUtilityTerm(36, noCarsInHouseholdFlag);   // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(37, carsLessThanDriversFlag); // for calibration of hov3 vs hov2
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
          //                        alternative.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
          alternative.AddUtilityTerm(35, pathTypeModel.PathDistance.AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
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
          alternative.AddUtilityTerm(67, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(68, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(169, destinationParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(168, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(167, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(166, originParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(165, originParcel.HouseholdDensity1());
          alternative.AddUtilityTerm(164, originParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(162, (class1Dist > 0).ToFlag());
          alternative.AddUtilityTerm(162, (class2Dist > 0).ToFlag());
          //                        alternative.AddUtility(163, (worstDist > 0).ToFlag());
          alternative.AddUtilityTerm(170, 1.0 * destinationParcel.MixedUse4Index1()
                                                              + 0.00002 * destinationParcel.TotalEmploymentDensity1()
                                                              + 0.001 * destinationParcel.NetIntersectionDensity1()
                                                              + 0.001 * originParcel.NetIntersectionDensity1()
                                                              + 0.0002 * originParcel.HouseholdDensity1()
                                                              + 1.0 * originParcel.MixedUse4Index1());
          
          //alternative.AddUtilityTerm(261, originParcel.PCA_DensityTerm_Buffer1());
          //alternative.AddUtilityTerm(262, originParcel.PCA_WalkabilityTerm_Buffer1());
          //alternative.AddUtilityTerm(263, originParcel.PCA_MixedUseTerm_Buffer1());
          //alternative.AddUtilityTerm(264, originParcel.PCA_TransitAccessTerm_Buffer1());
          //alternative.AddUtilityTerm(261, destinationParcel.PCA_DensityTerm_Buffer1());
          //alternative.AddUtilityTerm(262, destinationParcel.PCA_WalkabilityTerm_Buffer1());
          //alternative.AddUtilityTerm(263, destinationParcel.PCA_MixedUseTerm_Buffer1());
          //alternative.AddUtilityTerm(264, destinationParcel.PCA_TransitAccessTerm_Buffer1());
          //alternative.AddUtilityTerm(265, destinationParcel.PCA_DensityTerm_Buffer1());
          //alternative.AddUtilityTerm(266, destinationParcel.PCA_WalkabilityTerm_Buffer1());
          //alternative.AddUtilityTerm(267, destinationParcel.PCA_MixedUseTerm_Buffer1());
          //alternative.AddUtilityTerm(268, destinationParcel.PCA_TransitAccessTerm_Buffer1());
          
        } else if (mode == Global.Settings.Modes.Walk) {
          alternative.AddUtilityTerm(70, 1); //for calibration
          alternative.AddUtilityTerm(71, maleFlag);
          alternative.AddUtilityTerm(73, ageBetween51And98Flag);
          alternative.AddUtilityTerm(75, pathTypeModel.PathDistance * pathTypeModel.PathDistance);
          alternative.AddUtilityTerm(77, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(78, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(178, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(177, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(176, originParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(175, originParcel.HouseholdDensity1());
          alternative.AddUtilityTerm(179, originParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(181, 1.0 * destinationParcel.MixedUse4Index1()
                                                   + 0.00001 * destinationParcel.TotalEmploymentDensity1()
                                                   + 0.001 * destinationParcel.NetIntersectionDensity1()
                                                   + 0.001 * originParcel.NetIntersectionDensity1()
                                                   + 0.0001 * originParcel.HouseholdDensity1()
                                                   + 1.0 * originParcel.MixedUse4Index1());
          
          //alternative.AddUtilityTerm(271, originParcel.PCA_DensityTerm_Buffer1());
          //alternative.AddUtilityTerm(272, originParcel.PCA_WalkabilityTerm_Buffer1());
          //alternative.AddUtilityTerm(273, originParcel.PCA_MixedUseTerm_Buffer1());
          //alternative.AddUtilityTerm(274, originParcel.PCA_TransitAccessTerm_Buffer1());
          //alternative.AddUtilityTerm(271, destinationParcel.PCA_DensityTerm_Buffer1());
          //alternative.AddUtilityTerm(272, destinationParcel.PCA_WalkabilityTerm_Buffer1());
          //alternative.AddUtilityTerm(273, destinationParcel.PCA_MixedUseTerm_Buffer1());
          //alternative.AddUtilityTerm(274, destinationParcel.PCA_TransitAccessTerm_Buffer1());
          //alternative.AddUtilityTerm(275, destinationParcel.PCA_DensityTerm_Buffer1());
          //alternative.AddUtilityTerm(276, destinationParcel.PCA_WalkabilityTerm_Buffer1());
          //alternative.AddUtilityTerm(277, destinationParcel.PCA_MixedUseTerm_Buffer1());
          //alternative.AddUtilityTerm(278, destinationParcel.PCA_TransitAccessTerm_Buffer1());

        } else if (mode == Global.Settings.Modes.PaidRideShare) {
          if (Global.Configuration.PaidRideshare_UseEstimatedInsteadOfAssertedCoefficients) {
            alternative.AddUtilityTerm(80, 1.0);
            alternative.AddUtilityTerm(81, noCarsInHouseholdFlag); //for calibration
            alternative.AddUtilityTerm(82, carsLessThanDriversFlag); //for calibration
            alternative.AddUtilityTerm(83, tour.Person.AgeIsBetween26And35.ToFlag());
            alternative.AddUtilityTerm(83, tour.Person.AgeIsBetween18And25.ToFlag());
            alternative.AddUtilityTerm(84, (tour.Person.Age >= 65).ToFlag());                        //alternative.AddUtilityTerm(81, tour.Person.AgeIsBetween26And35.ToFlag());
            alternative.AddUtilityTerm(85, originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2);
            alternative.AddUtilityTerm(86, destinationParcel.HouseholdsBuffer2 + destinationParcel.StudentsUniversityBuffer2 + destinationParcel.EmploymentTotalBuffer2);
            alternative.AddUtilityTerm(87, income0To25KFlag);
            alternative.AddUtilityTerm(88, incomeOver100Flag);
          } else {
            double modeConstant = Global.Configuration.AV_PaidRideShareModeUsesAVs
                        ? Global.Configuration.AV_PaidRideShare_ModeConstant
                        + Global.Configuration.AV_PaidRideShare_DensityCoefficient * Math.Min(originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2,
                        (Global.Configuration.PaidRideShare_DensityMeasureCapValue > 0) ? Global.Configuration.PaidRideShare_DensityMeasureCapValue : 6000)
                        + Global.Configuration.AV_PaidRideShare_AVOwnerCoefficient * (household.OwnsAutomatedVehicles > 0).ToFlag()
                        : Global.Configuration.PaidRideShare_ModeConstant
                        + Global.Configuration.PaidRideShare_DensityCoefficient * Math.Min(originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2,
                        (Global.Configuration.PaidRideShare_DensityMeasureCapValue > 0) ? Global.Configuration.PaidRideShare_DensityMeasureCapValue : 6000);

            alternative.AddUtilityTerm(90, modeConstant);
            alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_Age26to35Coefficient * tour.Person.AgeIsBetween26And35.ToFlag());
            alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_Age18to25Coefficient * tour.Person.AgeIsBetween18And25.ToFlag());
            alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_AgeOver65Coefficient * (tour.Person.Age >= 65).ToFlag());
          }
        }



        RegionSpecificCustomizations(alternative, tour, pathTypeModel.PathType, mode, destinationParcel);
      }
    }
  }
}
