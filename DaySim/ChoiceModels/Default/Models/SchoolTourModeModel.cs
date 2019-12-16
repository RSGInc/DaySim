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
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.Default.Models {
  public class SchoolTourModeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "SchoolTourModeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 6;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 199;
    private const int THETA_PARAMETER = 99;

    private readonly int[] _nestedAlternativeIds = new[] { 0, 19, 19, 20, 21, 21, 22, 22, 23, 24 };
    private readonly int[] _nestedAlternativeIndexes = new[] { 0, 0, 0, 1, 2, 2, 3, 3, 4, 5 };

    private readonly ITourCreator _creator =
        Global
        .ContainerDaySim.GetInstance<IWrapperFactory<ITourCreator>>()
        .Creator;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.SchoolTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
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

      int highestMode = Global.Configuration.IncludeParkAndRideInOtherHomeBasedTourModeModel ? Global.Settings.Modes.ParkAndRide : Global.Settings.Modes.Transit;

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        if (tour.DestinationParcel == null ||
            tour.Mode <= Global.Settings.Modes.None ||
           (tour.Mode == Global.Settings.Modes.ParkAndRide && !Global.Configuration.IncludeParkAndRideInSchoolTourModeModel) ||
           (tour.Mode == Global.Settings.Modes.PaidRideShare && !Global.Configuration.PaidRideShareModeIsAvailable) ||
            tour.Mode > Global.Settings.Modes.PaidRideShare) {
          return;
        }

        IEnumerable<IPathTypeModel> pathTypeModels = null;

        if (Global.Configuration.IncludeParkAndRideInSchoolTourModeModel) {
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
              /* hov occ */ 2, /* auto type */ 1,
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
              /* hov occ */ 2, /* auto type */ 1,
              tour.Person.PersonType,
              false);
        }

        int mode = (tour.Mode >= Global.Settings.Modes.SchoolBus) ? Global.Settings.Modes.Hov3 : tour.Mode; // use HOV3 for school bus impedance
        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == mode);

        if (!pathTypeModel.Available) {
          return;
        }

        RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Household.VehiclesAvailable, tour.Mode);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        IEnumerable<IPathTypeModel> pathTypeModels = null;

        if (Global.Configuration.IncludeParkAndRideInSchoolTourModeModel) {
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
              /* hov occ */ 2, /* auto type */ 1,
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
              /* hov occ */ 2, /* auto type */ 1,
              tour.Person.PersonType,
              false);
        }
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
          tour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
        }
      }
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(IPersonWrapper person, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      ITourWrapper tour = _creator.CreateWrapper(person, null, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.School);

      return RunNested(tour, destinationParcel, householdCars, 0.0);
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(IPersonDayWrapper personDay, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int householdCars) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      ITourWrapper tour = _creator.CreateWrapper(personDay.Person, personDay, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, Global.Settings.Purposes.School);

      return RunNested(tour, destinationParcel, householdCars, tour.Person.PersonType);
    }

    private ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel, int householdCars, double transitDiscountFraction) {
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
              householdCars,
              tour.Person.TransitPassOwnership,
              tour.Household.OwnsAutomatedVehicles > 0,
              /* hov occ */ 2, /* auto type */ 1,
              tour.Person.PersonType,
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
      IPersonWrapper person = tour.Person;
      IPersonDayWrapper personDay = tour.PersonDay;

      // household inputs
      int income0To25KFlag = household.Has0To25KIncome.ToFlag();
      int income25To50KFlag = household.Has25To50KIncome.ToFlag();
      int income75KPlusFlag = household.Has75KPlusIncome.ToFlag();
      int incomeOver100Flag = household.Has100KPlusIncome.ToFlag();
      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(householdCars);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(householdCars);
      int carsLessThanWorkersFlag = household.GetFlagForCarsLessThanWorkers(householdCars);

      // person inputs
      int childUnder5Flag = person.IsChildUnder5.ToFlag();
      int adultFlag = person.IsAdult.ToFlag();
      int drivingAgeStudentFlag = person.IsDrivingAgeStudent.ToFlag();
      int maleFlag = person.IsMale.ToFlag();

      // remaining inputs
      IParcelWrapper originParcel = tour.OriginParcel;
      double destinationParkingCost = destinationParcel.ParkingCostBuffer1(6);

      ChoiceModelUtility.SetEscortPercentages(personDay, out double escortPercentage, out double nonEscortPercentage);

      // school bus is a special case - use HOV3 impedance
      {
        IPathTypeModel pathTypeExtra = pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Hov3);
        int modeExtra = Global.Settings.Modes.SchoolBus;
        bool availableExtra = pathTypeExtra.Available;
        double generalizedTimeLogsumExtra = pathTypeExtra.GeneralizedTimeLogsum;

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(modeExtra, availableExtra, choice == modeExtra);
        alternative.Choice = modeExtra;

        alternative.AddNestedAlternative(_nestedAlternativeIds[modeExtra], _nestedAlternativeIndexes[modeExtra], THETA_PARAMETER);

        if (availableExtra) {
          //    case Global.Settings.Modes.SchoolBus:
          alternative.AddUtilityTerm(2, generalizedTimeLogsumExtra * tour.TimeCoefficient);
          alternative.AddUtilityTerm(10, 1);
          alternative.AddUtilityTerm(11, noCarsInHouseholdFlag); // for calibration
          alternative.AddUtilityTerm(13, carsLessThanDriversFlag); // for calibration
          alternative.AddUtilityTerm(17, childUnder5Flag);
          alternative.AddUtilityTerm(18, adultFlag);
        }
      }


      foreach (IPathTypeModel pathTypeModel in pathTypeModels) {
        int mode = pathTypeModel.Mode;
        bool available = (pathTypeModel.Mode != Global.Settings.Modes.ParkAndRide || Global.Configuration.IncludeParkAndRideInSchoolTourModeModel)
                             && (pathTypeModel.Mode != Global.Settings.Modes.PaidRideShare || Global.Configuration.PaidRideShareModeIsAvailable)
                             && pathTypeModel.Available;
        double generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
        alternative.Choice = mode;

        alternative.AddNestedAlternative(_nestedAlternativeIds[mode], _nestedAlternativeIndexes[mode], THETA_PARAMETER);

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
          alternative.AddUtilityTerm(129, destinationParcel.MixedUse2Index1());
        } else if (mode == Global.Settings.Modes.Transit) {
          alternative.AddUtilityTerm(20, 1);
          alternative.AddUtilityTerm(21, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(22, carsLessThanDriversFlag);
          alternative.AddUtilityTerm(27, childUnder5Flag);
          alternative.AddUtilityTerm(28, adultFlag);
          alternative.AddUtilityTerm(29, drivingAgeStudentFlag);
          alternative.AddUtilityTerm(129, destinationParcel.MixedUse2Index1());
          alternative.AddUtilityTerm(128, destinationParcel.TotalEmploymentDensity1());
          //                        alternative.AddUtilityTerm(127, destinationParcel.NetIntersectionDensity1());
          //                        alternative.AddUtilityTerm(126, originParcel.NetIntersectionDensity1());
          //                        alternative.AddUtilityTerm(125, originParcel.HouseholdDensity1());
          //                        alternative.AddUtilityTerm(124, originParcel.MixedUse2Index1());
          //                        alternative.AddUtilityTerm(123, Math.Log(destinationParcel.StopsTransitBuffer1+1));
          alternative.AddUtilityTerm(122, Math.Log(originParcel.StopsTransitBuffer1 + 1));
        } else if (mode == Global.Settings.Modes.Hov3) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT3));
          alternative.AddUtilityTerm(30, 1);
          alternative.AddUtilityTerm(37, twoPersonHouseholdFlag);
          alternative.AddUtilityTerm(37, onePersonHouseholdFlag);
          alternative.AddUtilityTerm(36, noCarsInHouseholdFlag);   // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(35, carsLessThanDriversFlag); // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(44, income0To25KFlag);
          alternative.AddUtilityTerm(45, income25To50KFlag);
          alternative.AddUtilityTerm(47, childUnder5Flag);
          alternative.AddUtilityTerm(133, escortPercentage);
          alternative.AddUtilityTerm(134, nonEscortPercentage);
        } else if (mode == Global.Settings.Modes.Hov2) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient / ChoiceModelUtility.CPFACT2));
          alternative.AddUtilityTerm(38, onePersonHouseholdFlag);
          alternative.AddUtilityTerm(40, 1);
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(42, carsLessThanDriversFlag); // for calibration
          alternative.AddUtilityTerm(44, income0To25KFlag);
          alternative.AddUtilityTerm(45, income25To50KFlag);
          alternative.AddUtilityTerm(47, childUnder5Flag);
          alternative.AddUtilityTerm(133, escortPercentage);
          alternative.AddUtilityTerm(134, nonEscortPercentage);
        } else if (mode == Global.Settings.Modes.Sov) {
          alternative.AddUtilityTerm(1, (destinationParkingCost * tour.CostCoefficient));
          alternative.AddUtilityTerm(50, 1);
          alternative.AddUtilityTerm(52, carsLessThanDriversFlag);
          alternative.AddUtilityTerm(54, income0To25KFlag);
          alternative.AddUtilityTerm(56, income75KPlusFlag);
          alternative.AddUtilityTerm(59, drivingAgeStudentFlag);
          alternative.AddUtilityTerm(131, escortPercentage);
          alternative.AddUtilityTerm(132, nonEscortPercentage);
        } else if (mode == Global.Settings.Modes.Bike) {
          double class1Dist =
              Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
                  ? ImpedanceRoster.GetValue("class1distance", mode, Global.Settings.PathTypes.FullNetwork,
                      Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
                  : 0;

          double class2Dist =
              Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
                  ? ImpedanceRoster.GetValue("class2distance", mode, Global.Settings.PathTypes.FullNetwork,
                      Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
                  : 0;

          double worstDist =
              Global.Configuration.PathImpedance_BikeUseTypeSpecificDistanceFractions
                  ? ImpedanceRoster.GetValue("worstdistance", mode, Global.Settings.PathTypes.FullNetwork,
                      Global.Settings.ValueOfTimes.DefaultVot, tour.DestinationArrivalTime, originParcel, destinationParcel).Variable
                  : 0;

          alternative.AddUtilityTerm(60, 1);
          alternative.AddUtilityTerm(61, maleFlag);
          alternative.AddUtilityTerm(67, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(68, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(69, adultFlag);
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
          alternative.AddUtilityTerm(77, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(78, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(79, adultFlag);
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
        } else if (mode == Global.Settings.Modes.PaidRideShare) {
          if (Global.Configuration.PaidRideshare_UseEstimatedInsteadOfAssertedCoefficients) {
            alternative.AddUtilityTerm(80, 1.0);
            alternative.AddUtilityTerm(81, noCarsInHouseholdFlag); //for calibration
            alternative.AddUtilityTerm(82, carsLessThanDriversFlag); //for calibration
            alternative.AddUtilityTerm(83, tour.Person.AgeIsBetween26And35.ToFlag());
            alternative.AddUtilityTerm(83, tour.Person.AgeIsBetween18And25.ToFlag());
            alternative.AddUtilityTerm(84, (tour.Person.Age < 18).ToFlag());                        //alternative.AddUtilityTerm(81, tour.Person.AgeIsBetween26And35.ToFlag());
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
