// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
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
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.Default.Models {
  public class OtherHomeBasedTourModeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "OtherHomeBasedTourModeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 5;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 299;
    private const int THETA_PARAMETER = 99;

    private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 22, 0, 23 };
    private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 3, 0, 4 };

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.OtherHomeBasedTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
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
           (tour.Mode == Global.Settings.Modes.ParkAndRide && !Global.Configuration.IncludeParkAndRideInOtherHomeBasedTourModeModel) ||
           (tour.Mode == Global.Settings.Modes.PaidRideShare && !Global.Configuration.PaidRideShareModeIsAvailable) ||
            tour.Mode > Global.Settings.Modes.PaidRideShare) {
          return;
        }

        IEnumerable<IPathTypeModel> pathTypeModels = null;

        if (Global.Configuration.IncludeParkAndRideInOtherHomeBasedTourModeModel) {
          pathTypeModels =
          PathTypeModelFactory.Singleton.RunAllPlusParkAndRide(
          tour.Household.RandomUtility,
              tour.OriginParcel,
              tour.DestinationParcel,
              tour.DestinationArrivalTime,
              tour.DestinationDepartureTime,
              tour.DestinationPurpose,
              tour.CostCoefficient,
              tour.TimeCoefficient,
              tour.Person.Age,
              tour.Household.VehiclesAvailable,
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              tour.Person.PersonType,
              false);
        } else {
          pathTypeModels =
          PathTypeModelFactory.Singleton.RunAll(
          tour.Household.RandomUtility,
              tour.OriginParcel,
              tour.DestinationParcel,
              tour.DestinationArrivalTime,
              tour.DestinationDepartureTime,
              tour.DestinationPurpose,
              tour.CostCoefficient,
              tour.TimeCoefficient,
              tour.Person.Age,
              tour.Household.VehiclesAvailable,
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              tour.Person.PersonType,
              false);

        }

        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == tour.Mode);

        if (!pathTypeModel.Available) {
          return;
        }

        RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Mode);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        IEnumerable<IPathTypeModel> pathTypeModels = null;

        if (Global.Configuration.IncludeParkAndRideInOtherHomeBasedTourModeModel) {

          pathTypeModels =
          PathTypeModelFactory.Singleton.RunAllPlusParkAndRide(
          tour.Household.RandomUtility,
              tour.OriginParcel,
              tour.DestinationParcel,
              tour.DestinationArrivalTime,
              tour.DestinationDepartureTime,
              tour.DestinationPurpose,
              tour.CostCoefficient,
              tour.TimeCoefficient,
              tour.Person.Age,
              tour.Household.VehiclesAvailable,
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              tour.Person.PersonType,
              false);
        } else {
          pathTypeModels =
          PathTypeModelFactory.Singleton.RunAll(
          tour.Household.RandomUtility,
              tour.OriginParcel,
              tour.DestinationParcel,
              tour.DestinationArrivalTime,
              tour.DestinationDepartureTime,
              tour.DestinationPurpose,
              tour.CostCoefficient,
              tour.TimeCoefficient,
              tour.Person.Age,
              tour.Household.VehiclesAvailable,
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              tour.Person.PersonType,
              false);
        }
        RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel);

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
          tour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
        }
      }
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel) {
      if (tour == null) {
        throw new ArgumentNullException("tour");
      }

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
              tour.Person.Age,
              tour.Household.VehiclesAvailable,
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              tour.Person.PersonType,
              false);

      RunModel(choiceProbabilityCalculator, tour, pathTypeModels, destinationParcel);

      return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
    }

    protected virtual void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, int pathType, int mode, IParcelWrapper destinationParcel) {
      //see PSRC customization dll for example
      //Global.PrintFile.WriteLine("Generic Default WorkTourModeModel.RegionSpecificCustomizations being called so must not be overridden by CustomizationDll");
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = tour.Household;
      Framework.DomainModels.Models.IHouseholdTotals householdTotals = household.HouseholdTotals;
      IPersonWrapper person = tour.Person;
      IPersonDayWrapper personDay = tour.PersonDay;

      // household inputs
      int income0To25KFlag = household.Has0To25KIncome.ToFlag();
      int incomeOver100Flag = household.Has100KPlusIncome.ToFlag();
      int childrenUnder5 = householdTotals.ChildrenUnder5;
      int childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      int nonworkingAdults = householdTotals.NonworkingAdults;
      int retiredAdults = householdTotals.RetiredAdults;
      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(household.VehiclesAvailable);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(household.VehiclesAvailable);
      int carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(household.VehiclesAvailable);

      // person inputs
      int maleFlag = person.IsMale.ToFlag();
      int ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();
      int univStudentFlag = person.IsUniversityStudent.ToFlag();

      // tour inputs
      int shoppingTourFlag = (tour.DestinationPurpose == Global.Settings.Purposes.Shopping).ToFlag();
      int mealTourFlag = (tour.DestinationPurpose == Global.Settings.Purposes.Meal).ToFlag();
      int socialOrRecreationTourFlag = (tour.DestinationPurpose == Global.Settings.Purposes.Social).ToFlag();

      // remaining inputs
      IParcelWrapper originParcel = tour.OriginParcel;
      int parkingDuration = ChoiceModelUtility.GetParkingDuration(person.IsFulltimeWorker);
      double destinationParkingCost = destinationParcel.ParkingCostBuffer1(parkingDuration);

      ChoiceModelUtility.SetEscortPercentages(personDay, out double escortPercentage, out double nonEscortPercentage, true);


      //foreach (var pathTypeModel in pathTypeModels) {
      foreach (IPathTypeModel pathTypeModel in pathTypeModels) {
        IPathTypeModel ipathTypeModel = pathTypeModel;
        int mode = pathTypeModel.Mode;
        bool available = (pathTypeModel.Mode != Global.Settings.Modes.ParkAndRide || Global.Configuration.IncludeParkAndRideInOtherHomeBasedTourModeModel)
                             && (pathTypeModel.Mode != Global.Settings.Modes.PaidRideShare || Global.Configuration.PaidRideShareModeIsAvailable)
                             && pathTypeModel.Available;
        double generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

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
          alternative.AddUtilityTerm(5, 1);
          alternative.AddUtilityTerm(6, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(7, carsLessThanWorkersFlag);
          alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
        } else if (mode == Global.Settings.Modes.Transit) {
          alternative.AddUtilityTerm(20, 1);
          alternative.AddUtilityTerm(21, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(22, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(120, shoppingTourFlag);
          alternative.AddUtilityTerm(121, mealTourFlag);
          //                        alternative.AddUtility(129, destinationParcel.MixedUse2Index1());
          alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
          //                        alternative.AddUtility(127, destinationParcel.NetIntersectionDensity1());
          //                        alternative.AddUtility(126, originParcel.NetIntersectionDensity1());
          //                        alternative.AddUtility(125, originParcel.HouseholdDensity1());
          alternative.AddUtilityTerm(124, originParcel.MixedUse2Index1());
          //                        alternative.AddUtility(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
          //                        alternative.AddUtility(122, Math.Log(originParcel.StopsTransitBuffer1+1));
          alternative.AddUtilityTerm(180, univStudentFlag);
        } else if (mode == Global.Settings.Modes.Hov3) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT3));
          alternative.AddUtilityTerm(30, 1);
          alternative.AddUtilityTerm(31, childrenUnder5);
          alternative.AddUtilityTerm(32, childrenAge5Through15);
          alternative.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
          alternative.AddUtilityTerm(35, pathTypeModel.PathDistance.AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
          alternative.AddUtilityTerm(36, noCarsInHouseholdFlag);   // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(37, carsLessThanWorkersFlag); // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
          alternative.AddUtilityTerm(39, twoPersonHouseholdFlag);
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(43, carsLessThanWorkersFlag);
          alternative.AddUtilityTerm(133, escortPercentage);
          alternative.AddUtilityTerm(134, nonEscortPercentage);
          alternative.AddUtilityTerm(136, shoppingTourFlag);
          alternative.AddUtilityTerm(137, mealTourFlag);
          alternative.AddUtilityTerm(138, socialOrRecreationTourFlag);
        } else if (mode == Global.Settings.Modes.Hov2) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT2));
          alternative.AddUtilityTerm(31, childrenUnder5);
          alternative.AddUtilityTerm(32, childrenAge5Through15);
          alternative.AddUtilityTerm(34, nonworkingAdults + retiredAdults);
          alternative.AddUtilityTerm(35, pathTypeModel.PathDistance.AlmostEquals(0) ? 0 : Math.Log(pathTypeModel.PathDistance));
          alternative.AddUtilityTerm(40, 1);
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(43, carsLessThanWorkersFlag);
          alternative.AddUtilityTerm(48, onePersonHouseholdFlag);
          alternative.AddUtilityTerm(133, escortPercentage);
          alternative.AddUtilityTerm(134, nonEscortPercentage);
          alternative.AddUtilityTerm(136, shoppingTourFlag);
          alternative.AddUtilityTerm(137, mealTourFlag);
          alternative.AddUtilityTerm(138, socialOrRecreationTourFlag);
        } else if (mode == Global.Settings.Modes.Sov) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient));
          alternative.AddUtilityTerm(50, 1);
          alternative.AddUtilityTerm(52, carsLessThanDriversFlag);
          alternative.AddUtilityTerm(54, income0To25KFlag);
          alternative.AddUtilityTerm(131, escortPercentage);
          alternative.AddUtilityTerm(132, nonEscortPercentage);
        } else if (mode == Global.Settings.Modes.Bike) {
          double class1Dist = Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions ?
              ImpedanceRoster.GetValue("class1distance", mode, Global.Settings.PathTypes.FullNetwork,
                  Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable : 0;
          double class2Dist = Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions ?
              ImpedanceRoster.GetValue("class2distance", mode, Global.Settings.PathTypes.FullNetwork,
                  Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable : 0;
          double worstDist = Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions ?
              ImpedanceRoster.GetValue("worstdistance", mode, Global.Settings.PathTypes.FullNetwork,
                  Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable : 0;

          alternative.AddUtilityTerm(60, 1);
          alternative.AddUtilityTerm(61, maleFlag);
          alternative.AddUtilityTerm(63, ageBetween51And98Flag);
          alternative.AddUtilityTerm(67, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(68, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(160, socialOrRecreationTourFlag);
          alternative.AddUtilityTerm(169, destinationParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(168, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(167, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(166, originParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(165, originParcel.HouseholdDensity1());
          alternative.AddUtilityTerm(164, originParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(161, (class1Dist > 0).ToFlag());
          alternative.AddUtilityTerm(162, (class2Dist > 0).ToFlag());
          alternative.AddUtilityTerm(163, (worstDist > 0).ToFlag());
          alternative.AddUtilityTerm(170, 1.0 * destinationParcel.MixedUse4Index1()
                                                    + 0.00002 * destinationParcel.TotalEmploymentDensity1()
                                                    + 0.001 * destinationParcel.NetIntersectionDensity1()
                                                    + 0.001 * originParcel.NetIntersectionDensity1()
                                                    + 0.0002 * originParcel.HouseholdDensity1()
                                                    + 1.0 * originParcel.MixedUse4Index1());
        } else if (mode == Global.Settings.Modes.Walk) {
          alternative.AddUtilityTerm(70, 1);
          alternative.AddUtilityTerm(73, ageBetween51And98Flag);
          alternative.AddUtilityTerm(77, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(78, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(171, mealTourFlag);
          alternative.AddUtilityTerm(172, socialOrRecreationTourFlag);
          alternative.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(178, destinationParcel.HouseholdDensity1());
          alternative.AddUtilityTerm(177, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(176, originParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(175, originParcel.HouseholdDensity1());
          alternative.AddUtilityTerm(174, originParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(181, 1.0 * destinationParcel.MixedUse4Index1()
                                                   + 0.00001 * destinationParcel.TotalEmploymentDensity1()
                                                   + 0.001 * destinationParcel.NetIntersectionDensity1()
                                                   + 0.001 * originParcel.NetIntersectionDensity1()
                                                   + 0.0001 * originParcel.HouseholdDensity1()
                                                   + 1.0 * originParcel.MixedUse4Index1());
        } else if (mode == Global.Settings.Modes.PaidRideShare) {
          if (Global.Configuration.PaidRideshare_UseEstimatedInsteadOfAssertedCoefficients) {
            alternative.AddUtilityTerm(80, 1.0);
            //alternative.AddUtilityTerm(81, tour.Person.AgeIsBetween26And35.ToFlag());
            //alternative.AddUtilityTerm(82, tour.Person.AgeIsBetween18And25.ToFlag());
            alternative.AddUtilityTerm(81, noCarsInHouseholdFlag); //for calibration
            alternative.AddUtilityTerm(82, carsLessThanDriversFlag); //for calibration
            alternative.AddUtilityTerm(83, (tour.Person.Age >= 65).ToFlag());
            alternative.AddUtilityTerm(84, originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2);
            alternative.AddUtilityTerm(85, destinationParcel.HouseholdsBuffer2 + destinationParcel.StudentsUniversityBuffer2 + destinationParcel.EmploymentTotalBuffer2);
            alternative.AddUtilityTerm(86, income0To25KFlag);
            alternative.AddUtilityTerm(87, incomeOver100Flag);
            alternative.AddUtilityTerm(88, mealTourFlag);
            alternative.AddUtilityTerm(89, shoppingTourFlag);
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
