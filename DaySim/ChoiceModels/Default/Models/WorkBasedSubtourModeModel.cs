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
  public class WorkBasedSubtourModeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "WorkBasedSubtourModeModel";
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

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(subtour.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (subtour.DestinationParcel == null || subtour.OriginParcel == null ||
        subtour.Mode <= Global.Settings.Modes.None ||
        subtour.Mode == Global.Settings.Modes.ParkAndRide ||
        subtour.Mode == Global.Settings.Modes.SchoolBus ||
       (subtour.Mode == Global.Settings.Modes.PaidRideShare && !Global.Configuration.PaidRideShareModeIsAvailable) ||
        subtour.Mode > Global.Settings.Modes.PaidRideShare) {
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
                subtour.Person.Age,
                subtour.Household.VehiclesAvailable,
                 subtour.Person.TransitPassOwnership,
                subtour.Household.OwnsAutomatedVehicles > 0,
                /* hov occ */ 2, /* auto type */ 1,
                 subtour.Person.PersonType,
                false);

        IPathTypeModel pathTypeModel = pathTypeModels.First(x => x.Mode == subtour.Mode);

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
                subtour.Person.Age,
                subtour.Household.VehiclesAvailable,
                subtour.Person.TransitPassOwnership,
                subtour.Household.OwnsAutomatedVehicles > 0,
                 /* hov occ */ 2, /* auto type */ 1,
                subtour.Person.PersonType,
                false);

        RunModel(choiceProbabilityCalculator, subtour, pathTypeModels, subtour.DestinationParcel, subtour.ParentTour.Mode);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(subtour.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", subtour.PersonDay.Id);
          subtour.Mode = Global.Settings.Modes.Hov3;
          subtour.PersonDay.IsValid = false;
          return;
        }
        int choice = (int)chosenAlternative.Choice;

        subtour.Mode = choice;
        if (choice == Global.Settings.Modes.SchoolBus || choice == Global.Settings.Modes.PaidRideShare) {
          subtour.PathType = 0;
        } else {

          IPathTypeModel chosenPathType = pathTypeModels.First(x => x.Mode == choice);
          subtour.PathType = chosenPathType.PathType;
          subtour.ParkAndRideNodeId = choice == Global.Settings.Modes.ParkAndRide ? chosenPathType.PathParkAndRideNodeId : 0;
        }
      }
    }


    public ChoiceProbabilityCalculator.Alternative RunNested(ITourWrapper subtour, IParcelWrapper destinationParcel) {
      if (subtour == null) {
        throw new ArgumentNullException("subtour");
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetNestedChoiceProbabilityCalculator();

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
              subtour.Person.Age,
              subtour.Household.VehiclesAvailable,
               subtour.Person.TransitPassOwnership,
              subtour.Household.OwnsAutomatedVehicles > 0,
                 /* hov occ */ 2, /* auto type */ 1,
              subtour.Person.PersonType,
              false);

      RunModel(choiceProbabilityCalculator, subtour, pathTypeModels, destinationParcel, subtour.ParentTour.Mode);

      return choiceProbabilityCalculator.SimulateChoice(subtour.Household.RandomUtility);
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper subtour, IEnumerable<IPathTypeModel> pathTypeModels, IParcelWrapper destinationParcel, int parentTourMode, int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = subtour.Household;
      IPersonWrapper person = subtour.Person;
      IPersonDayWrapper personDay = subtour.PersonDay;

      // household inputs
      int income0To25KFlag = household.Has0To25KIncome.ToFlag();
      int income25To50KFlag = household.Has25To50KIncome.ToFlag();
      int incomeOver100Flag = household.Has100KPlusIncome.ToFlag();

      // person inputs
      int maleFlag = person.IsMale.ToFlag();

      // tour inputs
      int sovTourFlag = (parentTourMode == Global.Settings.Modes.Sov).ToFlag();
      int hov2TourFlag = (parentTourMode == Global.Settings.Modes.Hov2).ToFlag();
      int bikeTourFlag = (parentTourMode == Global.Settings.Modes.Bike).ToFlag();
      int walkTourFlag = (parentTourMode == Global.Settings.Modes.Walk).ToFlag();
      int tncTourFlag = (parentTourMode == Global.Settings.Modes.PaidRideShare).ToFlag();

      // remaining inputs
      IParcelWrapper originParcel = subtour.OriginParcel;
      int parkingDuration = ChoiceModelUtility.GetParkingDuration(person.IsFulltimeWorker);
      double destinationParkingCost = destinationParcel.ParkingCostBuffer1(parkingDuration);

      ChoiceModelUtility.SetEscortPercentages(personDay, out double escortPercentage, out double nonEscortPercentage, true);


      foreach (IPathTypeModel pathTypeModel in pathTypeModels) {
        int mode = pathTypeModel.Mode;
        bool available = (pathTypeModel.Mode != Global.Settings.Modes.PaidRideShare || Global.Configuration.PaidRideShareModeIsAvailable)
                    && pathTypeModel.Available;
        double generalizedTimeLogsum = pathTypeModel.GeneralizedTimeLogsum;

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(mode, available, choice == mode);
        alternative.Choice = mode;

        alternative.AddNestedAlternative(_nestedAlternativeIds[pathTypeModel.Mode], _nestedAlternativeIndexes[pathTypeModel.Mode], THETA_PARAMETER);

        if (!available) {
          continue;
        }

        double modeTimeCoefficient = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0 && mode >= Global.Settings.Modes.Sov && mode <= Global.Settings.Modes.Hov3) ?
                     subtour.TimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) :
                     (mode == Global.Settings.Modes.PaidRideShare && Global.Configuration.AV_PaidRideShareModeUsesAVs) ?
                     subtour.TimeCoefficient * (1.0 - Global.Configuration.AV_InVehicleTimeCoefficientDiscountFactor) : subtour.TimeCoefficient;
        alternative.AddUtilityTerm(2, generalizedTimeLogsum * modeTimeCoefficient);


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
          alternative.AddUtilityTerm(169, destinationParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(168, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(167, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(166, originParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(165, originParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(164, originParcel.MixedUse4Index1());
        } else if (mode == Global.Settings.Modes.Walk) {
          alternative.AddUtilityTerm(70, 1);
          alternative.AddUtilityTerm(79, walkTourFlag);
          alternative.AddUtilityTerm(179, destinationParcel.MixedUse4Index1());
          alternative.AddUtilityTerm(178, destinationParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(177, destinationParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(176, originParcel.NetIntersectionDensity1());
          alternative.AddUtilityTerm(175, originParcel.TotalEmploymentDensity1());
          alternative.AddUtilityTerm(174, originParcel.MixedUse4Index1());
        } else if (mode == Global.Settings.Modes.PaidRideShare) {
          if (Global.Configuration.PaidRideshare_UseEstimatedInsteadOfAssertedCoefficients) {
            alternative.AddUtilityTerm(80, 1.0);
            alternative.AddUtilityTerm(81, sovTourFlag);
            alternative.AddUtilityTerm(82, walkTourFlag + bikeTourFlag);
            alternative.AddUtilityTerm(83, subtour.Person.AgeIsBetween26And35.ToFlag());
            alternative.AddUtilityTerm(83, subtour.Person.AgeIsBetween18And25.ToFlag());
            alternative.AddUtilityTerm(84, tncTourFlag);
            alternative.AddUtilityTerm(85, originParcel.HouseholdsBuffer2 + originParcel.StudentsUniversityBuffer2 + originParcel.EmploymentTotalBuffer2);
            alternative.AddUtilityTerm(86, income0To25KFlag);
            alternative.AddUtilityTerm(87, incomeOver100Flag);
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
            alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_Age26to35Coefficient * subtour.Person.AgeIsBetween26And35.ToFlag());
            alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_Age18to25Coefficient * subtour.Person.AgeIsBetween18And25.ToFlag());
            alternative.AddUtilityTerm(90, Global.Configuration.PaidRideShare_AgeOver65Coefficient * (subtour.Person.Age >= 65).ToFlag());
          }
        }
      }
    }
  }
}
