// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.Actum.Models {
  public class TransitPassOwnershipModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "ActumTransitPassOwnershipModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.TransitPassOwnershipModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonWrapper person) {
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

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonWrapper person, int choice = Constants.DEFAULT_VALUE) {
      Framework.DomainModels.Wrappers.IParcelWrapper homeParcel = person.Household.ResidenceParcel;
      Framework.DomainModels.Wrappers.IParcelWrapper workParcel = person.IsUniversityStudent ? person.UsualSchoolParcel : person.UsualWorkParcel;
      Framework.DomainModels.Wrappers.IParcelWrapper schoolParcel = person.IsUniversityStudent ? null : person.UsualSchoolParcel;

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

        IEnumerable<dynamic> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            person.Household.RandomUtility,
                homeParcel,
                workParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.FivePM,
                Global.Settings.Purposes.Work,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                true,
                1,
                0,
                false,
                0.0,
                false,
                Global.Settings.Modes.Transit);

        dynamic path = pathTypeModels.First();

        workGenTimeNoPass = path.GeneralizedTimeLogsum;

        // intermediate variable of type IEnumerable<dynamic> is needed to acquire First() method as extension
        pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            person.Household.RandomUtility,
                homeParcel,
                workParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.FivePM,
                Global.Settings.Purposes.Work,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                true /* isDrivingAge */,
                1 /* householdCars */,
                        1 /* transitPassOwnership */,
                false /* carsAreAVs */,
                        1.0 /*transitDiscountFraction */,
                false /* randomChoice */,
                Global.Settings.Modes.Transit);

        path = pathTypeModels.First();

        workGenTimeWithPass = path.GeneralizedTimeLogsum;
      }

      //			double schoolGenTimeNoPass = -99.0;
      double schoolGenTimeWithPass = -99.0;

      if (!schoolParcelMissing && schoolTranDist < maxTranDist && homeTranDist < maxTranDist) {
        //				schoolGenTimeNoPass = path.GeneralizedTimeLogsum;

        IEnumerable<dynamic> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            person.Household.RandomUtility,
                homeParcel,
                schoolParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.ThreePM,
                Global.Settings.Purposes.School,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Other,
                true,
                1,
                1,
                false,
                1.0,
                false,
                Global.Settings.Modes.Transit);

        dynamic path = pathTypeModels.First();
        schoolGenTimeWithPass = path.GeneralizedTimeLogsum;
      }

      const double inflection = 0.50;

      double homeTranDist1 = Math.Pow(Math.Min(inflection, homeTranDist), 2.0);
      double homeTranDist2 = Math.Pow(Math.Max(homeTranDist - inflection, 0), 0.5);

      //			var workTranDist1 = Math.Pow(Math.Min(inflection, workTranDist),2.0);
      //			var workTranDist2 = Math.Pow(Math.Max(workTranDist - inflection, 0),0.5);

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

      double workTourLogsumDifference = 0D; // (full or part-time workers) full car ownership vs. no car ownership
      double schoolTourLogsumDifference = 0D; // (school) full car ownership vs. no car ownership
      Framework.DomainModels.Wrappers.IHouseholdWrapper household = person.Household;
      if (person.UsualWorkParcel != null && person.UsualWorkParcelId != household.ResidenceParcelId) {
        int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
        int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
        //JLB 201602
        //var nestedAlternative1 = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 1.0);
        //var nestedAlternative2 = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 0.0);
        ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 1.0, Global.Settings.Purposes.Work);
        ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 0.0, Global.Settings.Purposes.Work);

        workTourLogsumDifference = nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
        workTourLogsumDifference = nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
      }

      if (person.UsualSchoolParcel != null && person.UsualSchoolParcelId != household.ResidenceParcelId) {
        int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
        int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);

        //JLB 201602
        //var nestedAlternative1 = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 1.0);
        //var nestedAlternative2 = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 0.0);
        ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 1.0, Global.Settings.Purposes.School);
        ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, 0.0, Global.Settings.Purposes.School);

        schoolTourLogsumDifference = nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
        schoolTourLogsumDifference = nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
      }




      //Stefan variables
      double netIncomeNetCarOwnership = Math.Max(0, (person.Household.Income / 1000.0) / 2.0 - 2.441 * 15.0 * person.Household.VehiclesAvailable);  //net income minus annual cost to use household's cars in 1000s of DKK
                                                                                                                                                    //set household characteristics here that depend on person characteristics
      int numberAdults = 0;
      int numberChildren = 0;
      foreach (PersonWrapper p in person.Household.Persons) {
        if (p.Age >= 18) {
          numberAdults++;
        } else {
          numberChildren++;
        }
      }
      Framework.DomainModels.Wrappers.IParcelWrapper usualParcel = person.IsFullOrPartTimeWorker ? person.UsualWorkParcel : null;
      usualParcel = (usualParcel == null && person.UsualSchoolParcel != null) ? person.UsualSchoolParcel : null;
      int parkingSearchTime = 0;
      double commuteDistance = 0.0;
      int parkingCost = 0;
      int model = 3;
      if (usualParcel != null) {
        //parkingSearchTime = usualParcel.PSearchTime07_08; //uncomment when the new parcel attributes have been defined
        commuteDistance = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, 1.0, Global.Settings.Times.EightAM, household.ResidenceParcel, usualParcel).Variable;
        //parkingCost = usualParcel.ParkingCostPerHour8_18;  //uncomment when the new parcel attributes have been defined
        if (person.IsFulltimeWorker && usualParcel == person.UsualWorkParcel) {
          parkingCost = parkingCost * 8;
          model = 1;
        } else if (person.IsPartTimeWorker && usualParcel == person.UsualWorkParcel) {
          parkingCost = parkingCost * 4;
          model = 1;
        } else {
          parkingCost = parkingCost * 6;  // parking for school
          model = 2;
        }
      }


      // 0 No transit pass
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      alternative.AddUtilityTerm(1, 0.0);

      // 1 Transit pass

      double stefanUtility = 0.0;

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      if (model == 1 && person.Household.VehiclesAvailable == 0) {
        double beta001 = -0.33;
        double beta002 = -0.34;
        double beta003 = -1.15;
        double beta004 = -0.34;
        double beta005 = 0.0;
        double beta006 = 0.0;
        double beta007 = 0.0;
        double beta008 = 0.0;
        double beta009 = 0.0;
        double beta010 = 0.0;
        double beta011 = 0.0;
        double beta012 = 0.0;
        stefanUtility =
            beta001 * 1.0 +
            beta002 * numberChildren +
            beta003 * netIncomeNetCarOwnership +
            beta004 * person.IsMale.ToFlag() +
            beta005 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta006 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta007 * person.Age +
            beta008 * Math.Pow(person.Age, 2.0) +
            beta009 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta010 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta011 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta012 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(1, 1.0);
        //alternative.AddUtilityTerm(2, numberChildren);
        //alternative.AddUtilityTerm(3, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(4, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(5, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(6, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(7, person.Age);
        //alternative.AddUtilityTerm(8, Math.Pow(person.Age, 2.0));
        //non-worker/non-student models only
        //alternative.AddUtilityTerm(9, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(10, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(11, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(12, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(1, stefanUtility); // this composite replaces terms *1-*12 above

        //Stefan impedance (try replacign these with logsums)
        alternative.AddUtilityTerm(13, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(14, (commuteDistance > 0 && commuteDistance < 3).ToFlag());
        alternative.AddUtilityTerm(15, (commuteDistance >= 7 && commuteDistance < 13).ToFlag());
        alternative.AddUtilityTerm(16, (commuteDistance >= 13).ToFlag());
        alternative.AddUtilityTerm(17, parkingCost);
        alternative.AddUtilityTerm(18, parkingSearchTime);
        //commute logsum difference variable (with and without transit pass)
        alternative.AddUtilityTerm(19, workTourLogsumDifference);
      } else if (model == 1 && person.Household.VehiclesAvailable == 1) {
        double beta101 = -1.16;
        int beta102 = 0;
        int beta103 = 0;
        int beta104 = 0;
        int beta105 = 0;
        double beta106 = 0.63;
        double beta107 = -0.76;
        double beta108 = 0.09;
        int beta109 = 0;
        int beta110 = 0;
        int beta111 = 0;
        int beta112 = 0;
        stefanUtility =
            beta101 * 1.0 +
            beta102 * numberChildren +
            beta103 * netIncomeNetCarOwnership +
            beta104 * person.IsMale.ToFlag() +
            beta105 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta106 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta107 * person.Age +
            beta108 * Math.Pow(person.Age, 2.0) +
            beta109 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta110 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta111 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta112 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(101, 1.0);
        //alternative.AddUtilityTerm(102, numberChildren);
        //alternative.AddUtilityTerm(103, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(104, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(105, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(106, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(107, person.Age);
        //alternative.AddUtilityTerm(108, Math.Pow(person.Age, 2.0));
        ////non-worker/non-student models only
        //alternative.AddUtilityTerm(109, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(110, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(111, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(112, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(101, stefanUtility); // this composite replaces terms *1-*12 above
                                                        //Stefan impedance (try replacign these with logsums)
        alternative.AddUtilityTerm(113, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(114, (commuteDistance > 0 && commuteDistance < 3).ToFlag());
        alternative.AddUtilityTerm(115, (commuteDistance >= 7 && commuteDistance < 13).ToFlag());
        alternative.AddUtilityTerm(116, (commuteDistance >= 13).ToFlag());
        alternative.AddUtilityTerm(117, parkingCost);
        alternative.AddUtilityTerm(118, parkingSearchTime);
        //commute logsum difference variable (with and without transit pass)
        alternative.AddUtilityTerm(119, workTourLogsumDifference);
      } else if (model == 1 && person.Household.VehiclesAvailable >= 2) {
        double beta201 = -0.54;
        int beta202 = 0;
        int beta203 = 0;
        int beta204 = 0;
        double beta205 = 1.35;
        double beta206 = 0.42;
        double beta207 = -1.5;
        double beta208 = 0.17;
        int beta209 = 0;
        int beta210 = 0;
        int beta211 = 0;
        int beta212 = 0;
        stefanUtility =
            beta201 * 1.0 +
            beta202 * numberChildren +
            beta203 * netIncomeNetCarOwnership +
            beta204 * person.IsMale.ToFlag() +
            beta205 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta206 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta207 * person.Age +
            beta208 * Math.Pow(person.Age, 2.0) +
            beta209 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta210 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta211 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta212 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(201, 1.0);
        //alternative.AddUtilityTerm(202, numberChildren);
        //alternative.AddUtilityTerm(203, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(204, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(205, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(206, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(207, person.Age);
        //alternative.AddUtilityTerm(208, Math.Pow(person.Age, 2.0));
        //non-worker/non-student models only
        //alternative.AddUtilityTerm(209, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(210, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(211, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(212, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(201, stefanUtility); // this composite replaces terms *1-*12 above
                                                        //Stefan impedance (try replacign these with logsums)
        alternative.AddUtilityTerm(213, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(214, (commuteDistance > 0 && commuteDistance < 3).ToFlag());
        alternative.AddUtilityTerm(215, (commuteDistance >= 7 && commuteDistance < 13).ToFlag());
        alternative.AddUtilityTerm(216, (commuteDistance >= 13).ToFlag());
        alternative.AddUtilityTerm(217, parkingCost);
        alternative.AddUtilityTerm(218, parkingSearchTime);
        //commute logsum difference variable (with and without transit pass)
        alternative.AddUtilityTerm(219, workTourLogsumDifference);
      } else if (model == 2 && person.Household.VehiclesAvailable == 0) {
        double beta301 = 4.74;
        double beta302 = 0.39;
        int beta303 = 0;
        int beta304 = 0;
        int beta305 = 0;
        int beta306 = 0;
        double beta307 = -3.95;
        double beta308 = 0.62;
        int beta309 = 0;
        int beta310 = 0;
        int beta311 = 0;
        int beta312 = 0;
        stefanUtility =
            beta301 * 1.0 +
            beta302 * numberChildren +
            beta303 * netIncomeNetCarOwnership +
            beta304 * person.IsMale.ToFlag() +
            beta305 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta306 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta307 * person.Age +
            beta308 * Math.Pow(person.Age, 2.0) +
            beta309 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta310 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta311 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta312 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(301, 1.0);
        //alternative.AddUtilityTerm(302, numberChildren);
        //alternative.AddUtilityTerm(303, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(304, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(305, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(306, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(307, person.Age);
        //alternative.AddUtilityTerm(308, Math.Pow(person.Age, 2.0));
        ////non-worker/non-student models only
        //alternative.AddUtilityTerm(309, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(310, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(311, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(312, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(301, stefanUtility); // this composite replaces terms *1-*12 above
                                                        //Stefan impedance (try replacign these with logsums)
        alternative.AddUtilityTerm(313, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(314, (commuteDistance > 0 && commuteDistance < 3).ToFlag());
        alternative.AddUtilityTerm(315, (commuteDistance >= 7 && commuteDistance < 13).ToFlag());
        alternative.AddUtilityTerm(316, (commuteDistance >= 13).ToFlag());
        alternative.AddUtilityTerm(317, parkingCost);
        alternative.AddUtilityTerm(318, parkingSearchTime);
        //commute logsum difference variable (with and without transit pass)
        alternative.AddUtilityTerm(319, schoolTourLogsumDifference);
      } else if (model == 2 && person.Household.VehiclesAvailable >= 1) {
        double beta401 = 3.75;
        int beta402 = 0;
        int beta403 = 0;
        int beta404 = 0;
        int beta405 = 0;
        int beta406 = 0;
        double beta407 = 2.81;
        double beta408 = 0.33;
        int beta409 = 0;
        int beta410 = 0;
        int beta411 = 0;
        int beta412 = 0;
        stefanUtility =
            beta401 * 1.0 +
            beta402 * numberChildren +
            beta403 * netIncomeNetCarOwnership +
            beta404 * person.IsMale.ToFlag() +
            beta405 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta406 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta407 * person.Age +
            beta408 * Math.Pow(person.Age, 2.0) +
            beta409 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta410 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta411 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta412 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(401, 1.0);
        //alternative.AddUtilityTerm(402, numberChildren);
        //alternative.AddUtilityTerm(403, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(404, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(405, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(406, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(407, person.Age);
        //alternative.AddUtilityTerm(408, Math.Pow(person.Age, 2.0));
        ////non-worker/non-student models only
        //alternative.AddUtilityTerm(409, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(410, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(411, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(412, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(401, stefanUtility); // this composite replaces terms *1-*12 above
                                                        //Stefan impedance (try replacign these with logsums)
        alternative.AddUtilityTerm(413, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(414, (commuteDistance > 0 && commuteDistance < 3).ToFlag());
        alternative.AddUtilityTerm(415, (commuteDistance >= 7 && commuteDistance < 13).ToFlag());
        alternative.AddUtilityTerm(416, (commuteDistance >= 13).ToFlag());
        alternative.AddUtilityTerm(417, parkingCost);
        alternative.AddUtilityTerm(418, parkingSearchTime);
        //commute logsum difference variable (with and without transit pass)
        alternative.AddUtilityTerm(419, schoolTourLogsumDifference);
      } else if (model == 3 && person.Household.VehiclesAvailable == 0) {
        double beta501 = 0.05;
        int beta502 = 0;
        int beta503 = 0;
        int beta504 = 0;
        double beta505 = 0.56;
        double beta506 = 0.41;
        int beta507 = 0;
        int beta508 = 0;
        int beta509 = 0;
        int beta510 = 0;
        double beta511 = -0.45;
        int beta512 = 0;
        stefanUtility =
            beta501 * 1.0 +
            beta502 * numberChildren +
            beta503 * netIncomeNetCarOwnership +
            beta504 * person.IsMale.ToFlag() +
            beta505 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta506 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta507 * person.Age +
            beta508 * Math.Pow(person.Age, 2.0) +
            beta509 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta510 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta511 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta512 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(501, 1.0);
        //alternative.AddUtilityTerm(502, numberChildren);
        //alternative.AddUtilityTerm(503, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(504, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(505, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(506, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(507, person.Age);
        //alternative.AddUtilityTerm(508, Math.Pow(person.Age, 2.0));
        ////non-worker/non-student models only
        //alternative.AddUtilityTerm(509, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(510, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(511, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(512, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(501, stefanUtility); // this composite replaces terms *1-*12 above

      } else { //(model == 3 && person.Household.VehiclesAvailable >= 1)
        double beta601 = -1.7;
        int beta602 = 0;
        double beta603 = 0.47;
        int beta604 = 0;
        double beta605 = 0.63;
        double beta606 = 0.46;
        int beta607 = 0;
        int beta608 = 0;
        double beta609 = -0.32;
        double beta610 = 0.35;
        double beta611 = -0.37;
        double beta612 = -0.09;
        stefanUtility =
            beta601 * 1.0 +
            beta602 * numberChildren +
            beta603 * netIncomeNetCarOwnership +
            beta604 * person.IsMale.ToFlag() +
            beta605 * (person.IsAdultFemale && numberAdults == 1).ToFlag() +
            beta606 * (person.IsAdultFemale && numberAdults > 1).ToFlag() +
            beta607 * person.Age +
            beta608 * Math.Pow(person.Age, 2.0) +
            beta609 * (person.Household.VehiclesAvailable >= 2).ToFlag() +
            beta610 * (person.IsAdultMale && numberAdults == 1).ToFlag() +
            beta611 * Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0) +
            beta612 * Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus) +
            0.0;
        //Stefan utility
        //alternative.AddUtilityTerm(601, 1.0);
        //alternative.AddUtilityTerm(602, numberChildren);
        //alternative.AddUtilityTerm(603, netIncomeNetCarOwnership);
        //alternative.AddUtilityTerm(604, person.IsMale.ToFlag());
        //alternative.AddUtilityTerm(605, (person.IsAdultFemale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(606, (person.IsAdultFemale && numberAdults > 1).ToFlag());
        //alternative.AddUtilityTerm(607, person.Age);
        //alternative.AddUtilityTerm(608, Math.Pow(person.Age, 2.0));
        //non-worker/non-student models only
        //alternative.AddUtilityTerm(609, (person.Household.VehiclesAvailable >= 2).ToFlag());
        //alternative.AddUtilityTerm(610, (person.IsAdultMale && numberAdults == 1).ToFlag());
        //alternative.AddUtilityTerm(611, Math.Min(person.Household.ResidenceParcel.DistanceToLocalBus, 2.0));
        //alternative.AddUtilityTerm(612, Math.Min(5.0, person.Household.ResidenceParcel.DistanceToExpressBus));
        alternative.AddUtilityTerm(601, stefanUtility); // this composite replaces terms *1-*12 above
      }

      //alternative.AddUtilityTerm(2, person.IsPartTimeWorker.ToFlag());
      //alternative.AddUtilityTerm(3, (person.IsWorker && person.IsNotFullOrPartTimeWorker).ToFlag());
      //alternative.AddUtilityTerm(4, person.IsUniversityStudent.ToFlag());
      //alternative.AddUtilityTerm(5, person.IsRetiredAdult.ToFlag());
      //alternative.AddUtilityTerm(6, person.IsNonworkingAdult.ToFlag());
      //alternative.AddUtilityTerm(7, person.IsDrivingAgeStudent.ToFlag());
      //alternative.AddUtilityTerm(8, person.IsChildUnder16.ToFlag());
      //alternative.AddUtilityTerm(9, Math.Log(Math.Max(1, person.Household.Income)));
      //alternative.AddUtilityTerm(10, person.Household.HasMissingIncome.ToFlag());
      //alternative.AddUtilityTerm(11, workParcelMissing.ToFlag());
      //alternative.AddUtilityTerm(12, schoolParcelMissing.ToFlag());
      //alternative.AddUtilityTerm(13, (homeTranDist < 90.0) ? homeTranDist1 : 0);
      //alternative.AddUtilityTerm(14, (homeTranDist < 90.0) ? homeTranDist2 : 0);
      //alternative.AddUtilityTerm(15, (homeTranDist > 90.0) ? 1 : 0);
      //			//alternative.AddUtility(16, (workTranDist < 90.0) ? workTranDist : 0);
      //			//alternative.AddUtility(17, (workTranDist < 90.0) ? workTranDist2 : 0);
      //			//alternative.AddUtility(18, (workTranDist > 90.0) ? 1 : 0);
      //			//alternative.AddUtility(19, (schoolTranDist < 90.0) ? schoolTranDist : 0);
      //			//alternative.AddUtility(20, (schoolTranDist > 90.0) ? 1 : 0);
      //			//alternative.AddUtility(21, (!workParcelMissing && workGenTimeWithPass > -90 ) ? workGenTimeWithPass : 0);
      //alternative.AddUtilityTerm(22, (!workParcelMissing && workGenTimeWithPass <= -90) ? 1 : 0);
      //alternative.AddUtilityTerm(23, (!workParcelMissing && workGenTimeWithPass > -90 && workGenTimeNoPass > -90) ? workGenTimeNoPass - workGenTimeWithPass : 0);
      //			//alternative.AddUtility(24, (!schoolParcelMissing && schoolGenTimeWithPass > -90 ) ? schoolGenTimeWithPass : 0);
      //alternative.AddUtilityTerm(25, (!schoolParcelMissing && schoolGenTimeWithPass <= -90) ? 1 : 0);
      //alternative.AddUtilityTerm(26, homeAggregateLogsumNoCar * (person.IsFullOrPartTimeWorker || person.IsUniversityStudent).ToFlag());
      //alternative.AddUtilityTerm(27, homeAggregateLogsumNoCar * (person.IsDrivingAgeStudent || person.IsChildUnder16).ToFlag());
      //alternative.AddUtilityTerm(28, homeAggregateLogsumNoCar * (person.IsNonworkingAdult).ToFlag());
      //alternative.AddUtilityTerm(29, homeAggregateLogsumNoCar * (person.IsRetiredAdult).ToFlag());
      //alternative.AddUtilityTerm(30, workParcelMissing ? 0 : workAggregateLogsumNoCar);
      //alternative.AddUtilityTerm(31, schoolParcelMissing ? 0 : schoolAggregateLogsumNoCar);
      //alternative.AddUtilityTerm(32, transitPassCostChange);
    }
  }
}
