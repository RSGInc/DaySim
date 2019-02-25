// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.Actum.Models {
  public class AutoOwnershipModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "ActumAutoOwnershipModel";
    private const int TOTAL_ALTERNATIVES = 3;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 234;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.AutoOwnershipModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER, reader as CoefficientsReader);
    }

    public void Run(HouseholdWrapper household) {
      if (household == null) {
        throw new ArgumentNullException("household");  
      }

      household.ResetRandom(4);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      } else if (Global.Configuration.AV_IncludeAutoTypeChoice) {
        ChoiceProbabilityCalculator AVchoiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(household.Id);
        RunAVModel(AVchoiceProbabilityCalculator, household);
        ChoiceProbabilityCalculator.Alternative chosenAlternative = AVchoiceProbabilityCalculator.SimulateChoice(household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        household.OwnsAutomatedVehicles = choice;
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(household.Id);

      int vehicles = household.VehiclesAvailable;

      if (household.VehiclesAvailable > 2) {
        vehicles = 2;
      }

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        RunModel(choiceProbabilityCalculator, household, vehicles);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, household);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        household.VehiclesAvailable = choice;
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdWrapper household, int choice = Constants.DEFAULT_VALUE) {
      //			//			var distanceToTransitCappedUnderQtrMile = household.ResidenceParcel.DistanceToTransitCappedUnderQtrMile();
      //			//			var distanceToTransitQtrToHalfMile = household.ResidenceParcel.DistanceToTransitQtrToHalfMile();
      //var foodRetailServiceMedicalLogBuffer1 = household.ResidenceParcel.FoodRetailServiceMedicalLogBuffer1();

      //MB check for new hh properties
      int checkKids6To17 = household.Persons6to17; 
      // end check

      double workTourLogsumDifference = 0D; // (full or part-time workers) full car ownership vs. no car ownership
      double schoolTourLogsumDifference = 0D; // (school) full car ownership vs. no car ownership
                                              //															 //			const double workTourOtherLogsumDifference = 0D; // (other workers) full car ownership vs. no car ownership
                                              //
                                              // Stefan
      double netIncome = (household.Income / 1000.0) / 2.0; // in 1000s of DKK
      double userCost = 2.441 * 15.0;  //annual cost to use 1 car in 1000s of DKK
      bool isInCopenhagenMunicipality = household.MunicipalCode == 101;

      bool municipality101 = household.MunicipalCode == 101;
      bool municipality147 = household.MunicipalCode == 147;
      bool municipality151 = household.MunicipalCode == 151;
      bool municipality153 = household.MunicipalCode == 153;
      bool municipality155 = household.MunicipalCode == 155;
      bool municipality157 = household.MunicipalCode == 157;
      bool municipality159 = household.MunicipalCode == 159;
      bool municipality161 = household.MunicipalCode == 161;
      bool municipality163 = household.MunicipalCode == 163;
      bool municipality165 = household.MunicipalCode == 165;
      bool municipality167 = household.MunicipalCode == 167;
      bool municipality169 = household.MunicipalCode == 169;
      bool municipality173 = household.MunicipalCode == 173;
      bool municipality175 = household.MunicipalCode == 175;
      bool municipality183 = household.MunicipalCode == 183;
      bool municipality185 = household.MunicipalCode == 185;
      bool municipality187 = household.MunicipalCode == 187;
      bool municipality190 = household.MunicipalCode == 190;
      bool municipality201 = household.MunicipalCode == 201;
      bool municipality210 = household.MunicipalCode == 210;
      bool municipality217 = household.MunicipalCode == 217;
      bool municipality219 = household.MunicipalCode == 219;
      bool municipality223 = household.MunicipalCode == 223;
      bool municipality230 = household.MunicipalCode == 230;
      bool municipality240 = household.MunicipalCode == 240;
      bool municipality250 = household.MunicipalCode == 250;
      bool municipality253 = household.MunicipalCode == 253;
      bool municipality259 = household.MunicipalCode == 259;
      bool municipality260 = household.MunicipalCode == 260;
      bool municipality265 = household.MunicipalCode == 265;
      bool municipality269 = household.MunicipalCode == 269;
      bool municipality270 = household.MunicipalCode == 270;
      bool municipality336 = household.MunicipalCode == 336;
      bool municipality350 = household.MunicipalCode == 350;

      int numberAdults = 0;
      int numberChildren = 0;
      int numberWorkers = 0;
      int sumAdultAges = 0;
      double averageAdultAge = 0.0;
      bool isMale = false;

      foreach (PersonWrapper person in household.Persons) {
        //MB check for access to new Actum person properties
        int checkPersInc = person.PersonalIncome;
       //end check

        if (person.IsWorker && person.UsualWorkParcel != null && person.UsualWorkParcelId != household.ResidenceParcelId) {
          //MB check for access to new Actum parcel properties
          //requires a cast and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header - use new variable workerUsualParcel...
          IActumParcelWrapper workerUsualParcel = (IActumParcelWrapper)person.UsualWorkParcel;
          double checkWorkMZParkCost = workerUsualParcel.PublicParkingHourlyPriceBuffer1;
          //end check

          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
          //JLB 201602
          //var nestedAlternative1 = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers, 0.0);
          //var nestedAlternative2 = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, 0, 0.0);
          ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers, 0.0, Global.Settings.Purposes.Work);
          ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, 0, 0.0, Global.Settings.Purposes.Work);

          workTourLogsumDifference += nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
          workTourLogsumDifference -= nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
        }

        if (person.IsDrivingAgeStudent && person.UsualSchoolParcel != null && person.UsualSchoolParcelId != household.ResidenceParcelId) {
          //MB check for access to new Actum parcel properties
          //requires a cast and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header - use new variable studentUsualParcel...
          IActumParcelWrapper studentUsualParcel = (IActumParcelWrapper)person.UsualSchoolParcel;
          double checkSchoolMZParkCost = studentUsualParcel.PublicParkingHourlyPriceBuffer1;
          //end check



          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);

          //TODO: change the following school logsum retrievals when estimating this model after schoolTourModeTimeModel is enhanced for COMPAS2 
          ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers, 0.0, Global.Settings.Purposes.School);
          ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, 0, 0.0, Global.Settings.Purposes.School);

          schoolTourLogsumDifference += nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
          schoolTourLogsumDifference -= nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
        }
        if (person.Age >= 18) {
          numberAdults++;
          sumAdultAges = sumAdultAges + person.Age;
          isMale = person.IsMale;
          if (person.PersonType == Global.Settings.PersonTypes.FullTimeWorker
              //|| person.PersonType == Constants.PersonType.PART_TIME_WORKER
              ) {
            numberWorkers++;
          }
        } else {
          numberChildren++;
        }

      }
      averageAdultAge = sumAdultAges / Math.Max(numberAdults, 1);


      // var votSegment = household.VotALSegment;
      //var taSegment = household.ResidenceParcel.TransitAccessSegment();

      //var aggregateLogsumDifference = // full car ownership vs. no car ownership
      //	Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment] -
      //	Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votSegment][taSegment];

      //var distanceToStop
      //	 = household.ResidenceParcel.GetDistanceToTransit() > 0
      //			 ? Math.Min(household.ResidenceParcel.GetDistanceToTransit(), 2 * Global.Settings.DistanceUnitsPerMile)  // JLBscale
      //			 : 2 * Global.Settings.DistanceUnitsPerMile;

      //var ruralFlag = household.ResidenceParcel.RuralFlag();

      double zeroVehAVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0) ? Global.Configuration.AV_Own0VehiclesCoefficientForAVHouseholds : 0;
      double oneVehAVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0) ? Global.Configuration.AV_Own1VehicleCoefficientForAVHouseholds : 0;

      double zeroVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_DensityCoefficientForOwning0Vehicles * Math.Min(household.ResidenceBuffer2Density, 6000) : 0;
      double oneVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning1Vehicle : 0;
      double twoVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning2Vehicles : 0;
      //var threeVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning3Vehicles : 0;
      //var fourVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning4Vehicles : 0;

      bool hhLivesInCPHCity = false;
      if (household.ResidenceParcel.LandUseCode == 101 || household.ResidenceParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }
               

      // 0 AUTOS

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;
      alternative.AddUtilityTerm(14, Math.Log(Math.Max(netIncome, 1)));
      alternative.AddUtilityTerm(15, workTourLogsumDifference);  // instead of all Stefan's work-related and logsum variables
      
      //GV: 20. feb. 2019 - CPHcity logsum
      alternative.AddUtilityTerm(16, workTourLogsumDifference * (hhLivesInCPHCity).ToFlag());

      //GV: 20. feb. 2019 - income
      alternative.AddUtilityTerm(17, (household.Income >= 300000 && household.Income < 600000).ToFlag());
      alternative.AddUtilityTerm(18, (household.Income >= 600000).ToFlag());

      //GV: 20. feb. 2019 - HHsize 1 or 2
      alternative.AddUtilityTerm(19, (household.Size <= 2).ToFlag());
      

      alternative.AddUtilityTerm(90, 1); //calibration constant
      alternative.AddUtilityTerm(100, zeroVehAVEffect);
      alternative.AddUtilityTerm(100, zeroVehSEEffect);
      alternative.AddUtilityTerm(101, municipality101.ToFlag());
      alternative.AddUtilityTerm(102, municipality147.ToFlag());
      alternative.AddUtilityTerm(103, municipality151.ToFlag());
      alternative.AddUtilityTerm(104, municipality153.ToFlag());
      alternative.AddUtilityTerm(105, municipality155.ToFlag());
      alternative.AddUtilityTerm(106, municipality157.ToFlag());
      alternative.AddUtilityTerm(107, municipality159.ToFlag());
      alternative.AddUtilityTerm(108, municipality161.ToFlag());
      alternative.AddUtilityTerm(109, municipality163.ToFlag());
      alternative.AddUtilityTerm(110, municipality165.ToFlag());
      alternative.AddUtilityTerm(111, municipality167.ToFlag());
      alternative.AddUtilityTerm(112, municipality169.ToFlag());
      alternative.AddUtilityTerm(113, municipality173.ToFlag());
      alternative.AddUtilityTerm(114, municipality175.ToFlag());
      alternative.AddUtilityTerm(115, municipality183.ToFlag());
      alternative.AddUtilityTerm(116, municipality185.ToFlag());
      alternative.AddUtilityTerm(117, municipality187.ToFlag());
      alternative.AddUtilityTerm(118, municipality190.ToFlag());
      alternative.AddUtilityTerm(119, municipality201.ToFlag());
      alternative.AddUtilityTerm(120, municipality210.ToFlag());
      alternative.AddUtilityTerm(121, municipality217.ToFlag());
      alternative.AddUtilityTerm(122, municipality219.ToFlag());
      alternative.AddUtilityTerm(123, municipality223.ToFlag());
      alternative.AddUtilityTerm(124, municipality230.ToFlag());
      alternative.AddUtilityTerm(125, municipality240.ToFlag());
      alternative.AddUtilityTerm(126, municipality250.ToFlag());
      alternative.AddUtilityTerm(127, municipality253.ToFlag());
      alternative.AddUtilityTerm(128, municipality259.ToFlag());
      alternative.AddUtilityTerm(129, municipality260.ToFlag());
      alternative.AddUtilityTerm(130, municipality265.ToFlag());
      alternative.AddUtilityTerm(131, municipality269.ToFlag());
      alternative.AddUtilityTerm(132, municipality270.ToFlag());
      alternative.AddUtilityTerm(133, municipality336.ToFlag());
      alternative.AddUtilityTerm(134, municipality350.ToFlag());


      // 1 AUTO

      double beta010 = -6.59;
      double beta011 = 4.25;
      double beta012 = 5.53;
      double beta013 = 6.54;
      double beta014 = 1.17;
      double beta015 = 0.54;
      double beta016 = 0.81;
      double beta017 = 1.20;
      double beta018 = -0.54;
      double beta019 = 0.0;
      double beta020 = 0.45;
      double beta021 = 0.0;
      double beta022 = -0.04;
      double beta023 = 0.57;
      double beta024 = 0.18;
      double beta025 = -0.82;

      double stefanOneCarUtility =
                beta010 * 1.0 +
                beta011 * household.Has1Driver.ToFlag() +
                beta012 * household.Has2Drivers.ToFlag() +
                beta013 * (household.Has3Drivers || household.Has4OrMoreDrivers).ToFlag() +
                beta014 * Math.Log(Math.Max(netIncome - userCost, 1)) +
                beta015 * (numberChildren == 1).ToFlag() +
                beta016 * (numberChildren == 2).ToFlag() +
                beta017 * (numberChildren > 2).ToFlag() +
                beta018 * (numberAdults == 1 && !isMale).ToFlag() +
                beta019 * (numberAdults == 1 && isMale).ToFlag() +
                beta020 * averageAdultAge / 10.0 +
                beta021 * Math.Pow(averageAdultAge / 10.0, 2.0) +
                beta022 * 0 + //household.ResidenceParcel.PSearchTime16_17 +  // Add this when new parcel variables with seach time are available
                beta023 * household.ResidenceParcel.DistanceToLocalBus +
                beta024 * household.ResidenceParcel.DistanceToExpressBus +
                beta025 * isInCopenhagenMunicipality.ToFlag() +
                0;

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;
      //Stefan
      //alternative.AddUtilityTerm(10, 1.0);
      //alternative.AddUtilityTerm(11, household.Has1Driver.ToFlag());
      //alternative.AddUtilityTerm(12, household.Has2Drivers.ToFlag());
      //alternative.AddUtilityTerm(13, (household.Has3Drivers || household.Has4OrMoreDrivers).ToFlag());
      //alternative.AddUtilityTerm(14, Math.Log(Math.Max(netIncome - userCost, 1)));
      //alternative.AddUtilityTerm(15, (numberChildren == 1).ToFlag());
      //alternative.AddUtilityTerm(16, (numberChildren == 2).ToFlag());
      //alternative.AddUtilityTerm(17, (numberChildren > 2).ToFlag());
      //alternative.AddUtilityTerm(18, (numberAdults == 1 && !isMale).ToFlag());
      //alternative.AddUtilityTerm(19, (numberAdults == 1 && isMale).ToFlag());
      //alternative.AddUtilityTerm(20, averageAdultAge / 10.0);
      //alternative.AddUtilityTerm(21, Math.Pow(averageAdultAge / 10.0, 2.0));
      ////alternative.AddUtilityTerm(22, household.ResidenceParcel.PSearchTime16_17);  // Add this when new parcel variables with seach time are available
      //alternative.AddUtilityTerm(23, household.ResidenceParcel.DistanceToLocalBus);
      //alternative.AddUtilityTerm(24, household.ResidenceParcel.DistanceToExpressBus);
      //alternative.AddUtilityTerm(25, isInCopenhagenMunicipality.ToFlag());
      alternative.AddUtilityTerm(26, stefanOneCarUtility);  //this composite replaces above separate terms 10-25

      alternative.AddUtilityTerm(27, workTourLogsumDifference * household.HasMoreDriversThan1.ToFlag());  // instead of all Stefan's work-related and logsum variables

      //GV: 20. feb. 2019 - CPHcity logsum
      alternative.AddUtilityTerm(28, workTourLogsumDifference * household.HasMoreDriversThan1.ToFlag() * (hhLivesInCPHCity).ToFlag());

      //GV: 20. feb. 2019 - incme
      alternative.AddUtilityTerm(29, (household.Income >= 300000 && household.Income < 600000).ToFlag()); 
      alternative.AddUtilityTerm(30, (household.Income >= 600000).ToFlag());

      //GV: 20. feb. 2019 - HHsize 3+
      alternative.AddUtilityTerm(31, (household.Size >= 3).ToFlag());

      //GV: 20. feb. 2019 - HH has child/children
      alternative.AddUtilityTerm(32, (numberChildren >= 1).ToFlag());

      alternative.AddUtilityTerm(91, 1); //calibration constant
      alternative.AddUtilityTerm(100, oneVehAVEffect);
      alternative.AddUtilityTerm(100, oneVehSEEffect);

      //alternative.AddUtilityTerm(24, household.HouseholdTotals.RetiredAdultsPerDrivingAgeMembers);
      //alternative.AddUtilityTerm(25, household.HouseholdTotals.UniversityStudentsPerDrivingAgeMembers);
      //alternative.AddUtilityTerm(26, household.HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers);
      //alternative.AddUtilityTerm(27, household.HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers);

      // 2+ AUTOS

      double beta040 = -9.540;
      double beta041 = 2.79;
      double beta042 = 6.09;
      double beta043 = 7.77;
      double beta045 = 0.35;
      double beta046 = 0.81;
      double beta047 = 1.33;
      double beta048 = -1.13;
      double beta049 = 0.60;
      double beta050 = 0.92;
      double beta051 = -0.05;
      double beta052 = -0.09;
      double beta053 = 0.94;
      double beta054 = 0.31;
      double beta055 = -1.54;

      double stefanTwoCarUtility =
                beta040 * 1.0 +
                beta041 * household.Has1Driver.ToFlag() +
                beta042 * household.Has2Drivers.ToFlag() +
                beta043 * (household.Has3Drivers || household.Has4OrMoreDrivers).ToFlag() +
                beta014 * Math.Log(Math.Max(netIncome - userCost * 2.0, 1)) +
                beta045 * (numberChildren == 1).ToFlag() +
                beta046 * (numberChildren == 2).ToFlag() +
                beta047 * (numberChildren > 2).ToFlag() +
                beta048 * (numberAdults == 1 && !isMale).ToFlag() +
                beta049 * (numberAdults == 1 && isMale).ToFlag() +
                beta050 * averageAdultAge / 10.0 +
                beta051 * Math.Pow(averageAdultAge / 10.0, 2.0) +
                beta052 * 0 + //household.ResidenceParcel.PSearchTime16_17 +  // Add this when new parcel variables with seach time are available
                beta053 * household.ResidenceParcel.DistanceToLocalBus +
                beta054 * household.ResidenceParcel.DistanceToExpressBus +
                beta055 * isInCopenhagenMunicipality.ToFlag() +
                0;

      alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 2);
      alternative.Choice = 2;
      //Stefan
      //alternative.AddUtilityTerm(40, 1.0);
      //alternative.AddUtilityTerm(41, household.Has1Driver.ToFlag());
      //alternative.AddUtilityTerm(42, household.Has2Drivers.ToFlag());
      //alternative.AddUtilityTerm(43, (household.Has3Drivers || household.Has4OrMoreDrivers).ToFlag());
      //alternative.AddUtilityTerm(14, Math.Log(Math.Max(netIncome - userCost * 2.0, 1)));
      //alternative.AddUtilityTerm(45, (numberChildren == 1).ToFlag());
      //alternative.AddUtilityTerm(46, (numberChildren == 2).ToFlag());
      //alternative.AddUtilityTerm(47, (numberChildren > 2).ToFlag());
      //alternative.AddUtilityTerm(48, (numberAdults == 1 && !isMale).ToFlag());
      //alternative.AddUtilityTerm(49, (numberAdults == 1 && isMale).ToFlag());
      //alternative.AddUtilityTerm(50, averageAdultAge / 10.0);
      //alternative.AddUtilityTerm(51, Math.Pow(averageAdultAge / 10.0, 2.0));
      ////alternative.AddUtilityTerm(52, household.ResidenceParcel.PSearchTime16_17);  // Add this when new parcel variables with seach time are available
      //alternative.AddUtilityTerm(53, household.ResidenceParcel.DistanceToLocalBus);
      //alternative.AddUtilityTerm(54, household.ResidenceParcel.DistanceToExpressBus);
      //alternative.AddUtilityTerm(55, isInCopenhagenMunicipality.ToFlag());
      alternative.AddUtilityTerm(56, stefanTwoCarUtility);  //this composite replaces above separate terms 40-55

      //GV: 20. feb. 2019 - HHsize 4+
      alternative.AddUtilityTerm(57, (household.Size >= 4).ToFlag());

      //GV: 20. feb. 2019 - HH has children
      alternative.AddUtilityTerm(58, (numberChildren >= 2).ToFlag());       //alternative.AddUtilityTerm(57, workTourLogsumDifference);
      alternative.AddUtilityTerm(92, 1); //new calibration constant - must be constrained to 0 in estimation
      alternative.AddUtilityTerm(100, twoVehSEEffect);
      alternative.AddUtilityTerm(201, municipality101.ToFlag());
      alternative.AddUtilityTerm(202, municipality147.ToFlag());
      alternative.AddUtilityTerm(203, municipality151.ToFlag());
      alternative.AddUtilityTerm(204, municipality153.ToFlag());
      alternative.AddUtilityTerm(205, municipality155.ToFlag());
      alternative.AddUtilityTerm(206, municipality157.ToFlag());
      alternative.AddUtilityTerm(207, municipality159.ToFlag());
      alternative.AddUtilityTerm(208, municipality161.ToFlag());
      alternative.AddUtilityTerm(209, municipality163.ToFlag());
      alternative.AddUtilityTerm(210, municipality165.ToFlag());
      alternative.AddUtilityTerm(211, municipality167.ToFlag());
      alternative.AddUtilityTerm(212, municipality169.ToFlag());
      alternative.AddUtilityTerm(213, municipality173.ToFlag());
      alternative.AddUtilityTerm(214, municipality175.ToFlag());
      alternative.AddUtilityTerm(215, municipality183.ToFlag());
      alternative.AddUtilityTerm(216, municipality185.ToFlag());
      alternative.AddUtilityTerm(217, municipality187.ToFlag());
      alternative.AddUtilityTerm(218, municipality190.ToFlag());
      alternative.AddUtilityTerm(219, municipality201.ToFlag());
      alternative.AddUtilityTerm(220, municipality210.ToFlag());
      alternative.AddUtilityTerm(221, municipality217.ToFlag());
      alternative.AddUtilityTerm(222, municipality219.ToFlag());
      alternative.AddUtilityTerm(223, municipality223.ToFlag());
      alternative.AddUtilityTerm(224, municipality230.ToFlag());
      alternative.AddUtilityTerm(225, municipality240.ToFlag());
      alternative.AddUtilityTerm(226, municipality250.ToFlag());
      alternative.AddUtilityTerm(227, municipality253.ToFlag());
      alternative.AddUtilityTerm(228, municipality259.ToFlag());
      alternative.AddUtilityTerm(229, municipality260.ToFlag());
      alternative.AddUtilityTerm(230, municipality265.ToFlag());
      alternative.AddUtilityTerm(231, municipality269.ToFlag());
      alternative.AddUtilityTerm(232, municipality270.ToFlag());
      alternative.AddUtilityTerm(233, municipality336.ToFlag());
      alternative.AddUtilityTerm(234, municipality350.ToFlag());

      //alternative.AddUtilityTerm(44, household.HouseholdTotals.RetiredAdultsPerDrivingAgeMembers);
      //alternative.AddUtilityTerm(45, household.HouseholdTotals.UniversityStudentsPerDrivingAgeMembers);
      //alternative.AddUtilityTerm(46, household.HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers);
      //alternative.AddUtilityTerm(47, household.HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers);

    }
    private void RunAVModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IHouseholdWrapper household, int choice = Constants.DEFAULT_VALUE) {

      int ageOfHouseholdHead = 0;
      double totalCommuteTime = 0;

      foreach (IPersonWrapper person in household.Persons) {

        if (person.Sequence == 1) {
          ageOfHouseholdHead = person.Age;
        }
        if (person.IsWorker && person.UsualWorkParcel != null && person.UsualWorkParcelId != household.ResidenceParcelId) {
          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);

          totalCommuteTime += ImpedanceRoster.GetValue("time", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Configuration.VotHighVeryHigh - 0.5,
            destinationArrivalTime, household.ResidenceParcel, person.UsualWorkParcel).Variable;
          totalCommuteTime += ImpedanceRoster.GetValue("time", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Configuration.VotHighVeryHigh - 0.5,
            destinationDepartureTime, person.UsualWorkParcel, household.ResidenceParcel).Variable;
        }
      }

      // 0 Conventional auotos

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;
      //utility is 0

      // 1 AVs

      int lowIncome = household.Income < 300000 ? 1 : 0;
      int highIncome = household.Income > 900000 ? 1 : 0;

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      alternative.AddUtilityTerm(100, Global.Configuration.AV_AutoTypeConstant);
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHIncomeUnder50KCoefficient * lowIncome);
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHIncomeOver100KCoefficient * highIncome);
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHHeadUnder35Coefficient * (ageOfHouseholdHead < 35).ToFlag());
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHHeadOver65Coefficient * (ageOfHouseholdHead >= 65).ToFlag());
      alternative.AddUtilityTerm(100, Global.Configuration.AV_CoefficientPerHourCommuteTime * (totalCommuteTime / 60.0));

      // 2+ not available

      alternative = choiceProbabilityCalculator.GetAlternative(2, false, choice == 2);
      alternative.Choice = 2;
    }
  }
}
