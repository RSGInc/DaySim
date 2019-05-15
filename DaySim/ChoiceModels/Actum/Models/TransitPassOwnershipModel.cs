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
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;

namespace DaySim.ChoiceModels.Actum.Models {
  public class TransitPassOwnershipModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumTransitPassOwnershipModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 700;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.TransitPassOwnershipModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonWrapper person) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(3);

      // Determine passCategory, fareZones and passPrice 
      int passCategory = -1;
      int fareZones = -1;
      double passPrice = -1.0;
      
      //senior card
      if (person.Age >= Global.Configuration.COMPASS_TransitFareMinimumAgeForSeniorCard) {
        passCategory = 1;
        fareZones = Global.Configuration.COMPASS_TransitFareDefaultNumberOfZonesForSeniorCard;
        passPrice = Global.TransitMonthlyPrice_SeniorCard[fareZones];
       
      //child commuter card
      } else if (person.Age > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel && person.Age <= Global.Configuration.COMPASS_TransitFareMaximumAgeForChildDiscount
        && !(person.UsualSchoolParcel == null)) {
        SkimValue skimValue = ImpedanceRoster.GetValue("farezones", Global.Settings.Modes.Transit, Global.Settings.PathTypes.TransitType1, 10, 10, person.Household.ResidenceZoneId, person.UsualSchoolParcel.ZoneId);
        passCategory = 5;
        fareZones = (int)Math.Round(skimValue.Variable);
        passPrice = Global.TransitMonthlyPrice_ChildCommuteCard[fareZones];
      
      //youth high school card
      } else if (person.StudentType > 0 && person.StudentType <= 3) {
        passCategory = 2;
        fareZones = Global.Configuration.COMPASS_TransitFareDefaultNumberOfZonesForYouthCardGymnasium;
        passPrice = Global.TransitMonthlyPrice_YouthCardGymnasium[fareZones];
       
      //university card
      } else if (person.StudentType == 4) {
        passCategory = 3;
        fareZones = Global.Configuration.COMPASS_TransitFareDefaultNumberOfZonesForYouthCardUniversity;
        passPrice = Global.TransitMonthlyPrice_YouthCardUniversity[fareZones];
       
      //youth non-student card
      } else if (person.Age <= Global.Configuration.COMPASS_TransitFareMaximumAgeForYouthNonStudentCard) {
        passCategory = 4;
        fareZones = Global.Configuration.COMPASS_TransitFareDefaultNumberOfZonesForYouthCardNonStudent;
        passPrice = Global.TransitMonthlyPrice_YouthCardNonStudent[fareZones];
       
      //adult commuter card
      } else if (!(person.UsualWorkParcel == null)) {
        SkimValue skimValue = ImpedanceRoster.GetValue("farezones", Global.Settings.Modes.Transit, Global.Settings.PathTypes.TransitType1, 10, 10, person.Household.ResidenceZoneId, person.UsualWorkParcel.ZoneId);
        passCategory = 6;
        fareZones = (int)Math.Round(skimValue.Variable);
        passPrice = Global.TransitMonthlyPrice_AdultCommuteCard[fareZones];
      } else {
        passCategory = 6;
        fareZones = Global.Configuration.COMPASS_TransitFareDefaultNumberOfZonesForCommuterCard;
        passPrice = Global.TransitMonthlyPrice_AdultCommuteCard[fareZones];
      }


      if (Global.Configuration.IsInEstimationMode) {
        
        if(passCategory < 0) { //passCategory couldn't be determined because of missing usual location information
          person.TransitPassOwnership = passCategory;
        } else if (person.TransitPassOwnership > 0) {  //reset pass ownership of passholder to number of fare zones  
          person.TransitPassOwnership = fareZones;
        }
        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (person.TransitPassOwnership < 0) {
          return;
        }

        int transitPassIndicator = person.TransitPassOwnership > 0 ? 1:0; 
        RunModel(choiceProbabilityCalculator, person, passCategory, fareZones, passPrice, transitPassIndicator);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, person, passCategory, fareZones, passPrice);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;
        if (choice > 0) { choice = fareZones; }  //set to number of farezones for person who chooses transit pass
        person.TransitPassOwnership = choice;
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonWrapper person, int passCategory, int fareZones, double passPrice, int choice = Constants.DEFAULT_VALUE) {

      //Accessing COMPASS-specific properties requires a cast, and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)person.Household;
      IActumParcelWrapper homeParcel = (IActumParcelWrapper)household.ResidenceParcel;
      //IActumParcelWrapper workParcel = (IActumParcelWrapper)(person.IsUniversityStudent ? person.UsualSchoolParcel : person.UsualWorkParcel);
      //IActumParcelWrapper schoolParcel = (IActumParcelWrapper)(person.IsUniversityStudent ? null : person.UsualSchoolParcel);
      IActumParcelWrapper workParcel = (IActumParcelWrapper) person.UsualWorkParcel;
      IActumParcelWrapper schoolParcel = (IActumParcelWrapper) person.UsualSchoolParcel;

      double workMZParkPrice = workParcel != null ? workParcel.PublicParkingHourlyPriceBuffer1 : 0;

      bool workParcelMissing = workParcel == null;
      bool schoolParcelMissing = schoolParcel == null;

      //GV: 21.3.2019 - ask JB for what this stands for
      //JB: 22.3.2019 - max dist. to access PT
      //const double maxTranDist = 5.0;  //Goran, 5.0km might be the wrong max distance threshold
      const double maxTranDist = 4.0;

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

      int drivingAge = 22;
      int fullFareType = Global.Settings.PersonTypes.FullTimeWorker;
      int freeFareType = Global.Settings.PersonTypes.ChildUnder5;

      if (!workParcelMissing && workTranDist < maxTranDist && homeTranDist < maxTranDist) {

        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            household.RandomUtility,
                homeParcel,
                workParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.FivePM,
                Global.Settings.Purposes.Work,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                drivingAge,
                1,
                0,
                false,
                fullFareType,
                false,
                Global.Settings.Modes.Transit);

        IPathTypeModel path = pathTypeModels.First();

        workGenTimeNoPass = path.GeneralizedTimeLogsum;

        // intermediate variable of type IEnumerable<dynamic> is needed to acquire First() method as extension 
        pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            household.RandomUtility,
                homeParcel,
                workParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.FivePM,
                Global.Settings.Purposes.Work,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Work,
                /* isDrivingAge */ drivingAge,
                /* householdCars */ 1,
                /* transitPassOwnership */ fareZones,
                /* carsAreAVs */ false,
                /* transitDiscountFraction */ //freeFareType,  JB 20190513
                /* transitDiscountFraction */ fullFareType,
                /* randomChoice */ false,
                Global.Settings.Modes.Transit);

        path = pathTypeModels.First();

        workGenTimeWithPass = path.GeneralizedTimeLogsum;
      }

      //			double schoolGenTimeNoPass = -99.0;
      double schoolGenTimeWithPass = -99.0;

      if (!schoolParcelMissing && schoolTranDist < maxTranDist && homeTranDist < maxTranDist) {
        //				schoolGenTimeNoPass = path.GeneralizedTimeLogsum;

        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            household.RandomUtility,
                homeParcel,
                schoolParcel,
                Global.Settings.Times.EightAM,
                Global.Settings.Times.ThreePM,
                Global.Settings.Purposes.School,
                Global.Coefficients_BaseCostCoefficientPerMonetaryUnit,
                Global.Configuration.Coefficients_MeanTimeCoefficient_Other,
                drivingAge,
                1,
                fareZones,
                false,
                freeFareType,
                false,
                Global.Settings.Modes.Transit);

        IPathTypeModel path = pathTypeModels.First();
        schoolGenTimeWithPass = path.GeneralizedTimeLogsum;
      }

      const double inflection = 0.50;

      double homeTranDist1 = Math.Pow(Math.Min(inflection, homeTranDist), 2.0);
      double homeTranDist2 = Math.Pow(Math.Max(homeTranDist - inflection, 0), 0.5);

      //			var workTranDist1 = Math.Pow(Math.Min(inflection, workTranDist),2.0);
      //			var workTranDist2 = Math.Pow(Math.Max(workTranDist - inflection, 0),0.5);

      const double minimumAggLogsum = -15.0;
      //int votSegment = household.GetVotALSegment();
      //GV: 20.3.2019 - getting values from MB's memo
      int votSegment =
        (household.Income <= 450000)
                  ? Global.Settings.VotALSegments.Low
                  : (household.Income <= 900000)
                      ? Global.Settings.VotALSegments.Medium
                      : Global.Settings.VotALSegments.High;
           
      //int homeTaSegment = homeParcel.TransitAccessSegment();
      //GV: 20.3.2019 - getting values from MB's memo
      //OBS - it has to be in km
      int homeTaSegment =
         homeParcel.GetDistanceToTransit() >= 0 && homeParcel.GetDistanceToTransit() <= 0.4
            ? 0
            : homeParcel.GetDistanceToTransit() > 0.4 && homeParcel.GetDistanceToTransit() <= 1.6
                ? 1
                : 2;

      double homeAggregateLogsumNoCar = Math.Max(minimumAggLogsum, Global.AggregateLogsums[homeParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votSegment][homeTaSegment]);

      //int workTaSegment = workParcelMissing ? 0 : workParcel.TransitAccessSegment();
      //GV: 20.3.2019 - getting values from MB's memo
      //OBS! - DaySim goes down with my commnad
      //OBS - it has to be in km
      int workTaSegment =
         workParcelMissing ? 0 :
         workParcel.GetDistanceToTransit() >= 0 && workParcel.GetDistanceToTransit() <= 0.4
            ? 0
            : workParcel.GetDistanceToTransit() > 0.4 && workParcel.GetDistanceToTransit() <= 1.6
                ? 1
                : 2;

      double workAggregateLogsumNoCar =
                workParcelMissing
                    ? 0
                    : Math.Max(minimumAggLogsum, Global.AggregateLogsums[workParcel.ZoneId][Global.Settings.Purposes.WorkBased][Global.Settings.CarOwnerships.NoCars][votSegment][workTaSegment]);

      //int schoolTaSegment = schoolParcelMissing ? 0 : schoolParcel.TransitAccessSegment();
      //GV: 20.3.2019 - getting values from MB's memo
      //OBS! - DaySim goes down with my commnad
      //OBS - it has to be in km
      int schoolTaSegment =
        schoolParcelMissing ? 0 :
        schoolParcel.GetDistanceToTransit() >= 0 && schoolParcel.GetDistanceToTransit() <= 0.4
            ? 0
            : schoolParcel.GetDistanceToTransit() > 0.4 && schoolParcel.GetDistanceToTransit() <= 1.6
                ? 1
                : 2;

      double schoolAggregateLogsumNoCar =
                schoolParcelMissing
                    ? 0
                    : Math.Max(minimumAggLogsum, Global.AggregateLogsums[schoolParcel.ZoneId][Global.Settings.Purposes.WorkBased][Global.Settings.CarOwnerships.NoCars][votSegment][schoolTaSegment]);



      //GV: 21. march 2019 - CPH city veriab.
      bool hhLivesInCPHCity = false;
      if (homeParcel.LandUseCode == 101 || homeParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }

      bool isInCopenhagenMunicipality = household.MunicipalCode == 101;
      bool isInFrederiksbergMunicipality = household.MunicipalCode == 147;

      //GV: 26. mar. 2019 - no. of parkig places in the residental area
      //JB: Employee parking places should not be used
      double resNoParking = (homeParcel.ResidentialPermitOnlyParkingSpaces + 
      homeParcel.PublicWithResidentialPermitAllowedParkingSpaces +
      //homeParcel.PublicNoResidentialPermitAllowedParkingSpaces +
      homeParcel.ElectricVehicleOnlyParkingSpaces);

      //GV: 26. mar. 2019 - no. of parkig places in Buffer1 area
      //JB: Employee parking places should not be used
      double Bf1NoParking = (homeParcel.ResidentialPermitOnlyParkingSpacesBuffer1 + 
      homeParcel.PublicWithResidentialPermitAllowedParkingSpacesBuffer1 + 
      //homeParcel.PublicNoResidentialPermitAllowedParkingSpacesBuffer1 +
      homeParcel.ElectricVehicleOnlyParkingSpacesBuffer1);

      //GV: 26. mar. 2019 - no. of parkig places in the residental area per HH
      double resParkingSpacesPerHH = (Math.Max(1.0, resNoParking)) / (Math.Max(1.0, homeParcel.Households));
      //GV: 26. mar. 2019 - no. of parkig places in the Buffer1 area per HH
      double Bf1ParkingSpacesPerHH = (Math.Max(1.0, Bf1NoParking)) / (Math.Max(1.0, homeParcel.HouseholdsBuffer1));


      //Stefan variables
      //Goran, if you want to use Stefan's netIncomeNetCarOwnership variable, let me know.  If so, we should use user-controlled parameters instead of the following hard-coded constants
      //double netIncomeNetCarOwnership = Math.Max(0, (household.Income / 1000.0) / 2.0 - 2.441 * 15.0 * household.VehiclesAvailable);  //net income minus annual cost to use household's cars in 1000s of DKK
      int numberChildren = household.Persons6to17 + household.KidsBetween0And4;
      int numberAdults = household.Size - numberChildren;

      Framework.DomainModels.Wrappers.IParcelWrapper usualParcel = person.IsFullOrPartTimeWorker ? person.UsualWorkParcel : null;
      usualParcel = (usualParcel == null && person.UsualSchoolParcel != null) ? person.UsualSchoolParcel : null;
      double commuteDistance = 0.0;
      if (usualParcel != null) {
        commuteDistance = ImpedanceRoster.GetValue("distance-co", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, 1.0, Global.Settings.Times.EightAM, homeParcel, usualParcel).Variable;
      }

      //pass category constants
      int senior = passCategory == 1 ? 1 : 0;
      int youthGymnasium = passCategory == 2 ? 1 : 0;
      int youthUniversity = passCategory == 3 ? 1 : 0;
      int youthNonStudent = passCategory == 4 ? 1 : 0;
      int commuterChild = passCategory == 5 ? 1 : 0;
      int commuterAdult = passCategory == 6 ? 1 : 0;

      //fare without pass
      double fare = 0.0;
      if (person.Age > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel) {
        fare = Global.TransitBaseFare_Adult[fareZones];
        if (person.Age <= Global.Configuration.COMPASS_TransitFareMaximumAgeForChildDiscount) {
          fare = fare * (1 - Global.TransitBaseFare_ChildDiscount[fareZones] / 100.0);
        }
      }

      double passPriceToFareRatio = 0;
      if (fare > 0) { passPriceToFareRatio = passPrice / fare; }

      // 0 No transit pass
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      alternative.AddUtilityTerm(1, 0.0);

      // 1 Transit pass
      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      //passType-specific ASCs
      //GV: 22.3.2019 - I have sat them to t=0 untill all oher variables are estimated 
      alternative.AddUtilityTerm(1, 1.0);  //base is commuterAdult
      alternative.AddUtilityTerm(2, commuterChild);
      alternative.AddUtilityTerm(3, senior);
      alternative.AddUtilityTerm(4, youthGymnasium);
      alternative.AddUtilityTerm(5, youthUniversity);
      alternative.AddUtilityTerm(6, youthNonStudent);

      //JB: Goran, consider augmenting some of the generic terms in the following sections with some pass-type-specific variables  
      alternative.AddUtilityTerm(11, person.IsFemale.ToFlag());
      alternative.AddUtilityTerm(12, person.IsNonworkingAdult.ToFlag());
      //GV: 21.3.2019
      //alternative.AddUtilityTerm(13, commuterAdult * (person.IsFulltimeWorker || person.IsPartTimeWorker || (person.OccupationCode == 8)).ToFlag());
      alternative.AddUtilityTerm(13, commuterAdult * (person.IsFulltimeWorker || person.IsPartTimeWorker).ToFlag());
      //alternative.AddUtilityTerm(14, senior * (person.IsNotFullOrPartTimeWorker || person.IsRetiredAdult).ToFlag());
      alternative.AddUtilityTerm(14, senior * (person.IsRetiredAdult).ToFlag());
      alternative.AddUtilityTerm(15, commuterChild * person.IsChildAge5Through15.ToFlag());
      alternative.AddUtilityTerm(16, youthGymnasium * person.IsStudent.ToFlag());
      alternative.AddUtilityTerm(17, youthUniversity * person.IsUniversityStudent.ToFlag());

      alternative.AddUtilityTerm(18, (household.VehiclesAvailable == 0).ToFlag()); //Base is 1 cars in the HH
      alternative.AddUtilityTerm(19, (household.VehiclesAvailable >= 2).ToFlag());
      alternative.AddUtilityTerm(21, (household.Size <= 2).ToFlag());
      alternative.AddUtilityTerm(22, (household.Size >= 3).ToFlag());
      alternative.AddUtilityTerm(23, household.HasChildrenUnder5.ToFlag());
      //alternative.AddUtilityTerm(24, household.HasChildrenAge5Through15.ToFlag());
      alternative.AddUtilityTerm(24, (household.Persons6to17 > 0).ToFlag());

      alternative.AddUtilityTerm(25, (!(household.HasMissingIncome)).ToFlag() * Math.Log(Math.Max(1, household.Income))); //From default version
      alternative.AddUtilityTerm(26, household.HasMissingIncome.ToFlag());  // nuisance parameter accounts for estimation cases with missing income


      //Home accessibility
      //GV: 6. may 2019 - JB wanted coeff.30 with homeTranDist variable
      alternative.AddUtilityTerm(30, homeTranDist);

      //alternative.AddUtilityTerm(31, Math.Min(2.0, homeParcel.DistanceToLocalBus));   //Stefan  (This is low frequency bus)
      //GV: 25.3.2019 - distance to local bus == 1km
      //GV: 6. may 2019 - JB wanted to omit math.min in coeff.31
      //alternative.AddUtilityTerm(31, Math.Min(1.0, homeParcel.DistanceToLocalBus)); 
      alternative.AddUtilityTerm(31, homeParcel.DistanceToLocalBus);

      //alternative.AddUtilityTerm(32, Math.Min(5.0, homeParcel.DistanceToExpressBus));   //Stefan (This is high frequency bus)
      //GV: 25.3.2019 - distance to A, S bus == 1km
      //alternative.AddUtilityTerm(32, Math.Min(1.0, homeParcel.DistanceToExpressBus));
      alternative.AddUtilityTerm(32, homeParcel.DistanceToExpressBus);

      //GV: 25.3.2019 - distance to S-tog == 2km
      //alternative.AddUtilityTerm(33, Math.Min(2.0, homeParcel.DistanceToCommuterRail));
      alternative.AddUtilityTerm(33, homeParcel.DistanceToCommuterRail);

      //GV: 25.3.2019 - distance to Metro == 2km
      //alternative.AddUtilityTerm(34, Math.Min(2.0, homeParcel.DistanceToLightRail));
      alternative.AddUtilityTerm(34, homeParcel.DistanceToLightRail);

      //GV: 2.3.2019 - no used
      //alternative.AddUtilityTerm(35, Math.Min(homeParcel.DistanceToLocalBus, homeParcel.DistanceToExpressBus));   //Stefan 


      //GV: 22.3.2019 - 40
      alternative.AddUtilityTerm(37, (homeTranDist < 40.0) ? homeTranDist1 : 0); //From default version == 90
      alternative.AddUtilityTerm(38, (homeTranDist < 40.0) ? homeTranDist2 : 0); //From default version == 90
      //GV: 22.3.2019 - coeff. 36 cannot be estimated
      alternative.AddUtilityTerm(39, (homeTranDist > 40.0) ? 1 : 0); //From default version


      //In the following, review and consider revising the segment definitions, perhaps using passCategory dummies
      //alternative.AddUtilityTerm(41, homeAggregateLogsumNoCar * (person.IsFullOrPartTimeWorker || person.IsUniversityStudent).ToFlag()); //From default version
      //alternative.AddUtilityTerm(42, homeAggregateLogsumNoCar * (person.IsDrivingAgeStudent || person.IsChildUnder16).ToFlag()); //From default version
      //alternative.AddUtilityTerm(43, homeAggregateLogsumNoCar * (person.IsNonworkingAdult).ToFlag()); //From default version
      //alternative.AddUtilityTerm(44, homeAggregateLogsumNoCar * (person.IsRetiredAdult).ToFlag()); //From default version
      //GV. 25.3.2019
      //alternative.AddUtilityTerm(41, homeAggregateLogsumNoCar * commuterAdult * (person.IsFullOrPartTimeWorker).ToFlag()); 
      //alternative.AddUtilityTerm(42, homeAggregateLogsumNoCar * youthGymnasium * (person.IsDrivingAgeStudent).ToFlag());
      //alternative.AddUtilityTerm(42, homeAggregateLogsumNoCar * youthUniversity * (person.IsDrivingAgeStudent).ToFlag());
      //alternative.AddUtilityTerm(43, homeAggregateLogsumNoCar * commuterChild * (person.IsChildUnder16).ToFlag());
      //alternative.AddUtilityTerm(43, homeAggregateLogsumNoCar * youthGymnasium * (person.IsChildUnder16).ToFlag());
      //alternative.AddUtilityTerm(44, homeAggregateLogsumNoCar * (person.IsNonworkingAdult).ToFlag()); 
      alternative.AddUtilityTerm(41, homeAggregateLogsumNoCar * (person.IsFullOrPartTimeWorker).ToFlag()); 
      alternative.AddUtilityTerm(42, homeAggregateLogsumNoCar * (person.IsDrivingAgeStudent).ToFlag());
      alternative.AddUtilityTerm(43, homeAggregateLogsumNoCar * (person.IsChildUnder16).ToFlag());
      alternative.AddUtilityTerm(44, homeAggregateLogsumNoCar * (person.IsNonworkingAdult).ToFlag()); 
      
      //GV: OK
      //Accessibility at commute location
      alternative.AddUtilityTerm(51, workParcelMissing ? 0 : workAggregateLogsumNoCar); //From default version
      alternative.AddUtilityTerm(52, schoolParcelMissing ? 0 : schoolAggregateLogsumNoCar); //From default version
      alternative.AddUtilityTerm(53, workMZParkPrice);

      //GV: OK
      //Accessibility to commute location for workers and students
      //JB 20190513 replace next two lines
      //alternative.AddUtilityTerm(61, (!workParcelMissing && workGenTimeWithPass > -90) ? workGenTimeWithPass : 0); //From default version
      //alternative.AddUtilityTerm(62, (!workParcelMissing && workGenTimeWithPass <= -90) ? 1 : 0); //From default version
      alternative.AddUtilityTerm(61, (!workParcelMissing && workGenTimeWithPass > -90 && workGenTimeNoPass > -90) ? workGenTimeWithPass : 0); //From default version
      alternative.AddUtilityTerm(62, (!workParcelMissing && (workGenTimeWithPass <= -90 || workGenTimeNoPass <= -90)) ? 1 : 0); //From default version
      alternative.AddUtilityTerm(63, (!workParcelMissing && workGenTimeWithPass > -90 && workGenTimeNoPass > -90) ? workGenTimeNoPass - workGenTimeWithPass : 0); //From default version
      alternative.AddUtilityTerm(64, (!schoolParcelMissing && schoolGenTimeWithPass > -90) ? schoolGenTimeWithPass : 0); //From default version
      alternative.AddUtilityTerm(65, (!schoolParcelMissing && schoolGenTimeWithPass <= -90) ? 1 : 0); //From default version
      alternative.AddUtilityTerm(66, (person.WorkerType > 0 && workParcelMissing) ? 1:0);  //Nuisance parameter for missing location data
      alternative.AddUtilityTerm(67, (person.StudentType > 0 && schoolParcelMissing) ? 1:0); //Nuisance parameter for missing location data 
      
      //GV: OK
      //Price
      alternative.AddUtilityTerm(71, passPriceToFareRatio);
                     
      //GV: 26. 3. 2019 - parking avail. in CPH
      alternative.AddUtilityTerm(81, homeParcel.ParkingDataAvailable * Math.Log(Math.Max(1, Bf1NoParking)) * (isInCopenhagenMunicipality).ToFlag());

      //GV: 26. 3. 2019 - parking avail. in Frederiksberg
      alternative.AddUtilityTerm(82, homeParcel.ParkingDataAvailable * Math.Log(Math.Max(1, Bf1NoParking)) * (isInFrederiksbergMunicipality).ToFlag());

      //GV: 26. 3. 2019 - parking avail. in the rest of GCA
      alternative.AddUtilityTerm(83, homeParcel.ParkingDataAvailable * Math.Log(Math.Max(1, Bf1NoParking)) * (!hhLivesInCPHCity).ToFlag());

      //JB: 8.5.2019 - Add beta 84 as a dummy variable: (1 - homeParcel.ParkingDataAvailable) to go along with betas 81 - 83.
      //GV: 8.5.2019 - 
      alternative.AddUtilityTerm(84, (1 - homeParcel.ParkingDataAvailable));  
               
      //GV: cannot be estimated with the correct sign
      //GV: 26. 3. 2019 - parking costs
      //alternative.AddUtilityTerm(84, homeParcel.ParkingDataAvailable * (homeParcel.ResidentialPermitDailyParkingPricesBuffer1) * (isInCopenhagenMunicipality).ToFlag());
      //GV: 26. 3. 2019 - parking costs
      //alternative.AddUtilityTerm(85, homeParcel.ParkingDataAvailable * (homeParcel.ResidentialPermitDailyParkingPricesBuffer1) * (isInFrederiksbergMunicipality).ToFlag());
      //GV: 26. 3. 2019 - parking costs
      //alternative.AddUtilityTerm(86, homeParcel.ParkingDataAvailable * (homeParcel.ResidentialPermitDailyParkingPricesBuffer1) * (!hhLivesInCPHCity).ToFlag());

    }
  }
}

