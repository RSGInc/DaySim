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
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.Default.Models {
  public class EscortTourModeModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "EscortTourModeModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 199;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.EscortTourModeModelCoefficients, Global.Settings.Modes.TotalModes, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
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
        if (tour.DestinationParcel == null || (tour.Mode > Global.Settings.Modes.Hov3 || tour.Mode < Global.Settings.Modes.Walk)) {
          return;
        }

        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
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
                (tour.Household.OwnsAutomatedVehicles > 0),
                tour.Person.PersonType,
                false,
                Global.Settings.Modes.Walk, Global.Settings.Modes.Bike, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Hov3);

        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == tour.Mode);

        if (!pathTypeModel.Available) {
          return;
        }

        RunModel(choiceProbabilityCalculator, tour, pathTypeModels, tour.DestinationParcel, tour.Mode);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
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
               (tour.Household.OwnsAutomatedVehicles > 0),
                 tour.Person.PersonType,
                false,
                Global.Settings.Modes.Walk, Global.Settings.Modes.Bike, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Hov3);

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
        IPathTypeModel chosenPathType = pathTypeModels.First(x => x.Mode == choice);
        tour.PathType = chosenPathType.PathType;
        tour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
      }
    }

    public ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper tour, IParcelWrapper destinationParcel) {
      if (tour == null) {
        throw new ArgumentNullException("tour");
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

      IEnumerable<IPathTypeModel> pathTypeModels =
          PathTypeModelFactory.Singleton.Run(
          tour.Household.RandomUtility,
              tour.OriginParcel,
              destinationParcel,
              tour.DestinationArrivalTime,
              tour.DestinationDepartureTime,
              Global.Settings.Purposes.Escort,
              tour.CostCoefficient,
              tour.TimeCoefficient,
              tour.Person.Age,
              tour.Household.VehiclesAvailable,
               tour.Person.TransitPassOwnership,
             (tour.Household.OwnsAutomatedVehicles > 0),
               tour.Person.PersonType,
              false,
              Global.Settings.Modes.Walk, Global.Settings.Modes.Bike, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Hov3);

      RunModel(choiceProbabilityCalculator, tour, pathTypeModels, destinationParcel);

      return choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = tour.Household;
      Framework.DomainModels.Models.IHouseholdTotals householdTotals = household.HouseholdTotals;
      IPersonWrapper person = tour.Person;

      // household inputs
      int childrenUnder5 = householdTotals.ChildrenUnder5;
      int childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      int drivingAgeStudents = householdTotals.DrivingAgeStudents;
      int noCarsInHouseholdFlag = household.GetFlagForNoCarsInHousehold(household.VehiclesAvailable);
      int carsLessThanDriversFlag = household.GetFlagForCarsLessThanDrivers(household.VehiclesAvailable);

      // person inputs
      int ageBetween51And98Flag = person.AgeIsBetween51And98.ToFlag();

      // other inputs

      // in estimation mode, add an unavailable alternative 0 so Alogit will estimate it
      if (Global.Configuration.IsInEstimationMode) {
        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, false, choice == 0);
        alternative.Choice = 0;
        alternative.AddUtilityTerm(30, 0);
      }

      foreach (IPathTypeModel pathTypeModel in pathTypeModels) {
        int mode = pathTypeModel.Mode;
        bool available = (mode <= Global.Settings.Modes.Hov3 && mode >= Global.Settings.Modes.Walk) && pathTypeModel.Available;
        double generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
        alternative.Choice = mode;

        if (!available) {
          continue;
        }

        double modeTimeCoefficient = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0 && mode >= Global.Settings.Modes.Sov && mode <= Global.Settings.Modes.Hov3) ?
                tour.TimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : tour.TimeCoefficient;

        if (mode == Global.Settings.Modes.Hov3) {
          alternative.AddUtilityTerm(2, generalizedTimeLogsum * modeTimeCoefficient);
          alternative.AddUtilityTerm(30, 1);
          alternative.AddUtilityTerm(31, childrenUnder5);
          alternative.AddUtilityTerm(32, childrenAge5Through15);
          alternative.AddUtilityTerm(33, drivingAgeStudents);
          alternative.AddUtilityTerm(36, noCarsInHouseholdFlag);   // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(37, carsLessThanDriversFlag); // for calibration of hov3 vs hov2
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
        } else if (mode == Global.Settings.Modes.Hov2) {
          alternative.AddUtilityTerm(2, generalizedTimeLogsum * modeTimeCoefficient);
          alternative.AddUtilityTerm(40, 1);
          alternative.AddUtilityTerm(41, noCarsInHouseholdFlag);
          alternative.AddUtilityTerm(42, carsLessThanDriversFlag);
        } else if (mode == Global.Settings.Modes.Sov) {
          alternative.AddUtilityTerm(50, 1);
          alternative.AddUtilityTerm(52, carsLessThanDriversFlag); // for calibration
        } else if (mode == Global.Settings.Modes.Bike) {
          alternative.AddUtilityTerm(60, 1);
          alternative.AddUtilityTerm(67, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(68, carsLessThanDriversFlag); //for calibration
        } else if (mode == Global.Settings.Modes.Walk) {
          alternative.AddUtilityTerm(2, generalizedTimeLogsum * modeTimeCoefficient);
          alternative.AddUtilityTerm(70, 1);
          alternative.AddUtilityTerm(73, ageBetween51And98Flag);
          alternative.AddUtilityTerm(76, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(77, noCarsInHouseholdFlag); //for calibration
          alternative.AddUtilityTerm(78, carsLessThanDriversFlag); //for calibration
          alternative.AddUtilityTerm(81, childrenUnder5);
          alternative.AddUtilityTerm(82, childrenAge5Through15);
          alternative.AddUtilityTerm(83, drivingAgeStudents);
        }
      }
    }
  }
}
