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

        household.AutoType = choice;
        household.OwnsAutomatedVehicles = choice == 3? 1:0;
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
      IActumParcelWrapper residenceParcel = (IActumParcelWrapper)household.ResidenceParcel;


      double workTourLogsumDifference = 0D; // (full or part-time workers) full car ownership vs. no car ownership
      double schoolTourLogsumDifference = 0D; // (school) full car ownership vs. no car ownership
                                              //															 //			const double workTourOtherLogsumDifference = 0D; // (other workers) full car ownership vs. no car ownership
                                              //
                                              // Stefan

      bool incomeMissing = household.Income < 0? true:false;
      double netIncome = incomeMissing?  0 : (household.Income / 1000.0) * Global.Configuration.COMPASS_IncomeToNetIncomeMultiplier; // in 1000s of DKK
      double userCost = Global.Configuration.COMPASS_AnnualCostToUseOneCarInMonetaryUnts / 1000.0;  //annual cost to use 1 car in 1000s of DKK
      double incomeRemainder1Car = netIncome - userCost >= 0? netIncome-userCost: 0;
      double incomeRemainder2Cars = netIncome - 2*userCost >= 0? netIncome-2*userCost: 0;
      double incomeDeficit1Car = !incomeMissing && netIncome -userCost < 0? userCost - netIncome: 0;
      double incomeDeficit2Cars = !incomeMissing && netIncome -2*userCost < 0? 2*userCost - netIncome: 0;

      double resParkingSpacesPerHH = 
        (Math.Max(1.0, residenceParcel.ResidentialPermitOnlyParkingSpacesBuffer1 + residenceParcel.PublicWithResidentialPermitAllowedParkingSpacesBuffer1))/
        (Math.Max(1.0, residenceParcel.HouseholdsBuffer1));
      
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

      int numberChildren = household.Persons6to17 + household.KidsBetween0And4;
      int numberAdults = household.Size - numberChildren;
      int numberWorkers = 0;
      int sumAdultAges = 0;
      double averageAdultAge = 0.0;
      bool isMale = false;

      int i = 0;
      foreach (PersonWrapper person in household.Persons) {
        //MB check for access to new Actum person properties
        int checkPersInc = person.PersonalIncome;
        //end check
        i++;
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

        if (person.IsAdult && person.UsualSchoolParcel != null && person.UsualSchoolParcelId != household.ResidenceParcelId) {
          //MB check for access to new Actum parcel properties
          //requires a cast and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header - use new variable studentUsualParcel...
          IActumParcelWrapper studentUsualParcel = (IActumParcelWrapper)person.UsualSchoolParcel;
          double checkSchoolMZParkCost = studentUsualParcel.PublicParkingHourlyPriceBuffer1;
          //end check



          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);

          ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers, 0.0, Global.Settings.Purposes.School);
          ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, 0, 0.0, Global.Settings.Purposes.School);

          schoolTourLogsumDifference += nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
          schoolTourLogsumDifference -= nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
        }
        if (person.Age >= 18) {
          //numberAdults++;
          sumAdultAges = sumAdultAges + person.Age;
          isMale = person.IsMale;
          if (person.PersonType == Global.Settings.PersonTypes.FullTimeWorker
              //|| person.PersonType == Constants.PersonType.PART_TIME_WORKER
              ) {
            numberWorkers++;
          }
        } else {
          //numberChildren++;
        }

      }

      averageAdultAge = sumAdultAges / Math.Max(numberAdults, 1);
      //JB 20190224 if household persons are missing from sample we won't be able to use variables calculated in the above loop on persons 
      bool hhPersonDataComplete = true;
      if (i < household.Size) {
        hhPersonDataComplete = false;
      }

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

      double zeroVehEVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.AutoType == 2) ? Global.Configuration.EV_Own0VehiclesCoefficientForAVHouseholds : 0;
      double oneVehEVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.AutoType == 2) ? Global.Configuration.EV_Own1VehicleCoefficientForAVHouseholds : 0;

      double zeroVehAVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.AutoType ==3) ? Global.Configuration.AV_Own0VehiclesCoefficientForAVHouseholds : 0;
      double oneVehAVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.AutoType == 3) ? Global.Configuration.AV_Own1VehicleCoefficientForAVHouseholds : 0;

      double zeroVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_DensityCoefficientForOwning0Vehicles * Math.Min(household.ResidenceBuffer2Density, 6000) : 0;
      double oneVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning1Vehicle : 0;
      double twoVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning2Vehicles : 0;
      //var threeVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning3Vehicles : 0;
      //var fourVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning4Vehicles : 0;

      bool hhLivesInCPHCity = false;
      if (residenceParcel.LandUseCode == 101 || residenceParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }
               

      // 0 AUTOS

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      //JB 20190224 betas 3-5 confound with Stefan's variables
      //GV: 20. feb. 2019 - income
      //alternative.AddUtilityTerm(3, (household.Income >= 300000 && household.Income < 600000).ToFlag());
      //alternative.AddUtilityTerm(4, (household.Income >= 600000).ToFlag());
      //GV: 20. feb. 2019 - HHsize 1 or 2
      //alternative.AddUtilityTerm(5, (household.Size <= 2).ToFlag());

      //alternative.AddUtilityTerm(14, Math.Log(Math.Max(netIncome, 1)));

      alternative.AddUtilityTerm(1, 1); //calibration constant. Constrain to zero for estimation
      alternative.AddUtilityTerm(100, zeroVehEVEffect);
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

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      alternative.AddUtilityTerm(11, 1); //calibration constant.  Constrain to zero for estimation
      alternative.AddUtilityTerm(12, (numberAdults <= 1).ToFlag());
      alternative.AddUtilityTerm(13, (numberAdults == 2).ToFlag());
      alternative.AddUtilityTerm(14, (numberAdults >= 3).ToFlag());
      alternative.AddUtilityTerm(15, (numberChildren == 1).ToFlag());
      alternative.AddUtilityTerm(16, (numberChildren == 2).ToFlag());
      alternative.AddUtilityTerm(17, (numberChildren > 2).ToFlag());
      alternative.AddUtilityTerm(18, (!hhPersonDataComplete).ToFlag()); //nuisance parameter for missing person records in TU data
      alternative.AddUtilityTerm(19, (hhPersonDataComplete && numberAdults == 1 && !isMale).ToFlag());  // requires complete HH data
      alternative.AddUtilityTerm(20, hhPersonDataComplete.ToFlag() * averageAdultAge / 10.0);  //JB--I don't like this variable.  //requires complete HH data  
      alternative.AddUtilityTerm(21, hhPersonDataComplete.ToFlag() * workTourLogsumDifference); 
      //GV: 20. feb. 2019 - CPHcity logsum
      alternative.AddUtilityTerm(22, hhPersonDataComplete.ToFlag() * workTourLogsumDifference * (hhLivesInCPHCity).ToFlag());
      alternative.AddUtilityTerm(23, residenceParcel.GetDistanceToTransit());  // distance to nearest PT
      alternative.AddUtilityTerm(24, residenceParcel.DistanceToLightRail);     // distance to Metro
      alternative.AddUtilityTerm(25, isInCopenhagenMunicipality.ToFlag());
      alternative.AddUtilityTerm(26, residenceParcel.ParkingDataAvailable * Math.Log(resParkingSpacesPerHH));
      alternative.AddUtilityTerm(27, (residenceParcel.ParkingDataAvailable == 0).ToFlag());
      alternative.AddUtilityTerm(28, Math.Log(Math.Max(incomeRemainder1Car, 1)));  //should be positive coefficient
      alternative.AddUtilityTerm(29, incomeDeficit1Car);  // should be negative coefficient
      alternative.AddUtilityTerm(30, incomeMissing.ToFlag()); //nuisance parameter for missing income
      //GV: 20. feb. 2019 - incme
      //JB: 20190224 Goran, Stefan's spec already has income effects that confound with these variables.  
      //alternative.AddUtilityTerm(29, (household.Income >= 300000 && household.Income < 600000).ToFlag()); 
      //alternative.AddUtilityTerm(30, (household.Income >= 600000).ToFlag());

      //GV: 20. feb. 2019 - HHsize 3+
      //JB:  20190224 Stefan's spec already has household size-related variables that confound with this variable
      //alternative.AddUtilityTerm(31, (household.Size >= 3).ToFlag());

      //GV: 20. feb. 2019 - HH has child/children
      //JB:  20190224 Stefan's spec already has number of children variables that confound with this variable
      //alternative.AddUtilityTerm(32, (numberChildren >= 1).ToFlag());

      alternative.AddUtilityTerm(100, oneVehEVEffect);
      alternative.AddUtilityTerm(100, oneVehAVEffect);
      alternative.AddUtilityTerm(100, oneVehSEEffect);

      // 2+ AUTOS

      alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 2);
      alternative.Choice = 2;

      alternative.AddUtilityTerm(51, 1); //Calibration constant - must be constrained to 0 in estimation
      alternative.AddUtilityTerm(52, (numberAdults <= 1).ToFlag());
      alternative.AddUtilityTerm(53, (numberAdults == 2).ToFlag());
      alternative.AddUtilityTerm(54, (numberAdults >= 3).ToFlag());
      alternative.AddUtilityTerm(55, (numberChildren == 1).ToFlag());
      alternative.AddUtilityTerm(56, (numberChildren == 2).ToFlag());
      alternative.AddUtilityTerm(57, (numberChildren > 2).ToFlag());
      alternative.AddUtilityTerm(58, (!hhPersonDataComplete).ToFlag()); //nuisance parameter for missing person records in TU data
      alternative.AddUtilityTerm(59, (hhPersonDataComplete && numberAdults == 1 && !isMale).ToFlag());  // requires complete HH data
      alternative.AddUtilityTerm(60, hhPersonDataComplete.ToFlag() * averageAdultAge / 10.0);  //JB--I don't like this variable.  //requires complete HH data  
      alternative.AddUtilityTerm(61, hhPersonDataComplete.ToFlag() * workTourLogsumDifference * (numberAdults > 1).ToFlag()); 
      //GV: 20. feb. 2019 - CPHcity logsum
      alternative.AddUtilityTerm(62, hhPersonDataComplete.ToFlag() * workTourLogsumDifference * (numberAdults > 1).ToFlag() * (hhLivesInCPHCity).ToFlag());
      alternative.AddUtilityTerm(63, residenceParcel.GetDistanceToTransit()* (numberAdults > 1).ToFlag());  // distance to nearest PT
      alternative.AddUtilityTerm(64, residenceParcel.DistanceToLightRail* (numberAdults > 1).ToFlag());     // distance to Metro
      alternative.AddUtilityTerm(65, isInCopenhagenMunicipality.ToFlag()* (numberAdults > 1).ToFlag());
      alternative.AddUtilityTerm(66, residenceParcel.ParkingDataAvailable * Math.Log(resParkingSpacesPerHH) * (numberAdults > 1).ToFlag());
      alternative.AddUtilityTerm(67, (residenceParcel.ParkingDataAvailable == 0 ).ToFlag() * (numberAdults > 1).ToFlag());
      alternative.AddUtilityTerm(68, Math.Log(Math.Max(incomeRemainder2Cars, 1))* (numberAdults > 1).ToFlag() ); //should be positive coeficient
      alternative.AddUtilityTerm(69, incomeDeficit2Cars * (numberAdults > 1).ToFlag()  ); // should be negative coefficient
      alternative.AddUtilityTerm(70, incomeMissing.ToFlag()* (numberAdults > 1).ToFlag()); //nuisance parameter for missing income

      //GV: 20. feb. 2019 - HHsize 4+
      //JB: see above comments in 1-car utility
      //alternative.AddUtilityTerm(57, (household.Size >= 4).ToFlag());

      //GV: 20. feb. 2019 - HH has children
      //JB: see above comments in 1-car utility
      //alternative.AddUtilityTerm(58, (numberChildren >= 2).ToFlag());

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

    }
    private void RunAVModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IHouseholdWrapper household, int choice = Constants.DEFAULT_VALUE) {

      int lowIncome = household.Income < 300000 ? 1 : 0;
      int highIncome = household.Income > 900000 ? 1 : 0;

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

      // 1--Gas Conventional autos

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, Global.Configuration.GV_GasConventionalVehicleAvailable, choice == 1);
      alternative.Choice = 1;
      //utility is 0

      // 2--Electric Conventional autos


      alternative = choiceProbabilityCalculator.GetAlternative(1, Global.Configuration.EV_ElectricConventionalVehicleAvailable, choice == 2);
      alternative.Choice = 2;

      alternative.AddUtilityTerm(100, Global.Configuration.EV_AutoTypeConstant);
      alternative.AddUtilityTerm(100, Global.Configuration.EV_HHIncomeUnder50KCoefficient * lowIncome);
      alternative.AddUtilityTerm(100, Global.Configuration.EV_HHIncomeOver100KCoefficient * highIncome);
      alternative.AddUtilityTerm(100, Global.Configuration.EV_HHHeadUnder35Coefficient * (ageOfHouseholdHead < 35).ToFlag());
      alternative.AddUtilityTerm(100, Global.Configuration.EV_HHHeadOver65Coefficient * (ageOfHouseholdHead >= 65).ToFlag());
      alternative.AddUtilityTerm(100, Global.Configuration.EV_CoefficientPerHourCommuteTime * (totalCommuteTime / 60.0));

      // 3--AVs


      alternative = choiceProbabilityCalculator.GetAlternative(2, Global.Configuration.AV_ElectricAutonomousVehicleAvailable, choice == 3);
      alternative.Choice = 3;

      alternative.AddUtilityTerm(100, Global.Configuration.AV_AutoTypeConstant);
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHIncomeUnder50KCoefficient * lowIncome);
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHIncomeOver100KCoefficient * highIncome);
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHHeadUnder35Coefficient * (ageOfHouseholdHead < 35).ToFlag());
      alternative.AddUtilityTerm(100, Global.Configuration.AV_HHHeadOver65Coefficient * (ageOfHouseholdHead >= 65).ToFlag());
      alternative.AddUtilityTerm(100, Global.Configuration.AV_CoefficientPerHourCommuteTime * (totalCommuteTime / 60.0));

    }
  }
}
