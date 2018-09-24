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

namespace DaySim.ChoiceModels.H.Models {
  public class TransitPassOwnershipModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "HTransitPassOwnershipModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.TransitPassOwnershipModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(IPersonWrapper person) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(3);

      if (Global.Configuration.IsInEstimationMode) {
        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (person.TransitPassOwnership < 0 || person.TransitPassOwnership > 1) {
          return;
        }

        RunModel(choiceProbabilityCalculator, person, person.TransitPassOwnership);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, person);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        person.TransitPassOwnership = choice;
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonWrapper person, int choice = Constants.DEFAULT_VALUE) {
      IParcelWrapper homeParcel = person.Household.ResidenceParcel;
      IParcelWrapper workParcel = person.IsUniversityStudent ? person.UsualSchoolParcel : person.UsualWorkParcel;
      IParcelWrapper schoolParcel = person.IsUniversityStudent ? null : person.UsualSchoolParcel;

      bool workParcelMissing = workParcel == null;
      bool schoolParcelMissing = schoolParcel == null;

      const double maxTranDist = 1.5;

      double homeTranDist = 99.0;

      if (homeParcel.GetDistanceToTransit() >= 0.0001 && homeParcel.GetDistanceToTransit() <= maxTranDist) {
        homeTranDist = homeParcel.GetDistanceToTransit();
      }

      double workTranDist = 99.0;

      if (!workParcelMissing && workParcel.GetDistanceToTransit() >= 0.0001 && workParcel.GetDistanceToTransit() <= maxTranDist) {
        workTranDist = workParcel.GetDistanceToTransit();
      }

      double schoolTranDist = 99.0;

      if (!schoolParcelMissing && schoolParcel.GetDistanceToTransit() >= 0.0001 && schoolParcel.GetDistanceToTransit() <= maxTranDist) {
        schoolTranDist = schoolParcel.GetDistanceToTransit();
      }

      double workGenTimeNoPass = -99.0;
      double workGenTimeWithPass = -99.0;

      if (!workParcelMissing && workTranDist < maxTranDist && homeTranDist < maxTranDist) {

        IEnumerable<IPathTypeModel> pathTypeModels = PathTypeModelFactory.Singleton.Run(
            person.Household.RandomUtility,
                homeParcel,
                workParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.FivePM,
                Global.Settings.Purposes.Work,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                /* isDrivingAge */ true,
                /* householdCars */ 1,
                /* transitPassOwnership */ 0,
                /* carsAreAvs */ false,
                0.0,
                false,
                Global.Settings.Modes.Transit);

        IPathTypeModel path = pathTypeModels.First();

        workGenTimeNoPass = path.GeneralizedTimeLogsum;

        pathTypeModels = PathTypeModelFactory.Singleton.Run(
            person.Household.RandomUtility,
                homeParcel,
                workParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.FivePM,
                Global.Settings.Purposes.Work,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                /* isDrivingAge */ true,
                /* householdCars */ 1,
                /* transitPassOwnership */ 0,
                /* carsAreAvs */ false,
                1.0,
                false,
                Global.Settings.Modes.Transit);

        path = pathTypeModels.First();

        workGenTimeWithPass = path.GeneralizedTimeLogsum;
      }

      //            double schoolGenTimeNoPass = -99.0;
      double schoolGenTimeWithPass = -99.0;

      if (!schoolParcelMissing && schoolTranDist < maxTranDist && homeTranDist < maxTranDist) {
        //                schoolGenTimeNoPass = path.GeneralizedTimeLogsum;
        IEnumerable<IPathTypeModel> pathTypeModels =
          PathTypeModelFactory.Singleton.Run(
          person.Household.RandomUtility,
              homeParcel,
              schoolParcel,
              Global.Settings.Times.EightAM,
              Global.Settings.Times.ThreePM,
              Global.Settings.Purposes.School,
              Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
              Global.Configuration.Coefficients_MeanTimeCoefficient_Other,
                /* isDrivingAge */ true,
                /* householdCars */ 1,
                /* transitPassOwnership */ 0,
                /* carsAreAvs */ false,
              1.0,
              false,
              Global.Settings.Modes.Transit);

        IPathTypeModel path = pathTypeModels.First();

        schoolGenTimeWithPass = path.GeneralizedTimeLogsum;
      }

      const double inflection = 0.50;

      double homeTranDist1 = Math.Pow(Math.Min(inflection, homeTranDist), 2.0);
      double homeTranDist2 = Math.Pow(Math.Max(homeTranDist - inflection, 0), 0.5);

      //            var workTranDist1 = Math.Pow(Math.Min(inflection, workTranDist),2.0);
      //            var workTranDist2 = Math.Pow(Math.Max(workTranDist - inflection, 0),0.5);

      const double minimumAggLogsum = -15.0;
      int votSegment = person.Household.GetVotALSegment();

      int homeTaSegment = homeParcel.TransitAccessSegment();
      double homeAggregateLogsumNoCar = Math.Max(minimumAggLogsum, Global.AggregateLogsums[homeParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votSegment][homeTaSegment]);

      int workTaSegment = workParcelMissing ? 0 : workParcel.TransitAccessSegment();
      double workAggregateLogsumNoCar =
                workParcelMissing
                    ? 0
                    : Math.Max(minimumAggLogsum, Global.AggregateLogsums[workParcel.ZoneId][Global.Settings.Purposes.WorkBased][Global.Settings.CarOwnerships.NoCars][votSegment][workTaSegment]);

      int schoolTaSegment = schoolParcelMissing ? 0 : schoolParcel.TransitAccessSegment();
      double schoolAggregateLogsumNoCar =
                schoolParcelMissing
                    ? 0
                    : Math.Max(minimumAggLogsum, Global.AggregateLogsums[schoolParcel.ZoneId][Global.Settings.Purposes.WorkBased][Global.Settings.CarOwnerships.NoCars][votSegment][schoolTaSegment]);

      double transitPassCostChange = !Global.Configuration.IsInEstimationMode ? Global.Configuration.PathImpedance_TransitPassCostPercentChangeVersusBase : 0;

      // 0 No transit pass
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      alternative.AddUtilityTerm(1, 0.0);

      // 1 Transit pass
      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      alternative.AddUtilityTerm(1, 1.0);
      alternative.AddUtilityTerm(2, person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(3, (person.IsWorker && person.IsNotFullOrPartTimeWorker).ToFlag());
      alternative.AddUtilityTerm(4, person.IsUniversityStudent.ToFlag());
      alternative.AddUtilityTerm(5, person.IsRetiredAdult.ToFlag());
      alternative.AddUtilityTerm(6, person.IsNonworkingAdult.ToFlag());
      alternative.AddUtilityTerm(7, person.IsDrivingAgeStudent.ToFlag());
      alternative.AddUtilityTerm(8, person.IsChildUnder16.ToFlag());
      alternative.AddUtilityTerm(9, Math.Log(Math.Max(1, person.Household.Income)));
      alternative.AddUtilityTerm(10, person.Household.HasMissingIncome.ToFlag());
      alternative.AddUtilityTerm(11, workParcelMissing.ToFlag());
      alternative.AddUtilityTerm(12, schoolParcelMissing.ToFlag());
      alternative.AddUtilityTerm(13, (homeTranDist < 90.0) ? homeTranDist1 : 0);
      alternative.AddUtilityTerm(14, (homeTranDist < 90.0) ? homeTranDist2 : 0);
      alternative.AddUtilityTerm(15, (homeTranDist > 90.0) ? 1 : 0);
      //            alternative.AddUtility(16, (workTranDist < 90.0) ? workTranDist : 0);
      //            alternative.AddUtility(17, (workTranDist < 90.0) ? workTranDist2 : 0);
      //            alternative.AddUtility(18, (workTranDist > 90.0) ? 1 : 0);
      //            alternative.AddUtility(19, (schoolTranDist < 90.0) ? schoolTranDist : 0);
      //            alternative.AddUtility(20, (schoolTranDist > 90.0) ? 1 : 0);
      //            alternative.AddUtility(21, (!workParcelMissing && workGenTimeWithPass > -90 ) ? workGenTimeWithPass : 0);
      alternative.AddUtilityTerm(22, (!workParcelMissing && workGenTimeWithPass <= -90) ? 1 : 0);
      alternative.AddUtilityTerm(23, (!workParcelMissing && workGenTimeWithPass > -90 && workGenTimeNoPass > -90) ? workGenTimeNoPass - workGenTimeWithPass : 0);
      //            alternative.AddUtility(24, (!schoolParcelMissing && schoolGenTimeWithPass > -90 ) ? schoolGenTimeWithPass : 0);
      alternative.AddUtilityTerm(25, (!schoolParcelMissing && schoolGenTimeWithPass <= -90) ? 1 : 0);
      alternative.AddUtilityTerm(26, homeAggregateLogsumNoCar * (person.IsFullOrPartTimeWorker || person.IsUniversityStudent).ToFlag());
      alternative.AddUtilityTerm(27, homeAggregateLogsumNoCar * (person.IsDrivingAgeStudent || person.IsChildUnder16).ToFlag());
      alternative.AddUtilityTerm(28, homeAggregateLogsumNoCar * (person.IsNonworkingAdult).ToFlag());
      alternative.AddUtilityTerm(29, homeAggregateLogsumNoCar * (person.IsRetiredAdult).ToFlag());
      alternative.AddUtilityTerm(30, workParcelMissing ? 0 : workAggregateLogsumNoCar);
      alternative.AddUtilityTerm(31, schoolParcelMissing ? 0 : schoolAggregateLogsumNoCar);
      alternative.AddUtilityTerm(32, transitPassCostChange);
    }
  }
}
