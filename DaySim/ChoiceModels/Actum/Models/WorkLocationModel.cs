// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;
using DaySim.Framework.Sampling;
using DaySim.Sampling;

namespace DaySim.ChoiceModels.Actum.Models {
  public class WorkLocationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumWorkLocationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 2;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 350; 

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.WorkLocationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkLocationModelCoefficients, sampleSize + 1, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonWrapper person, int sampleSize) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(0);

      if (Global.Configuration.IsInEstimationMode) {
        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (person.UsualWorkParcel == null) {
          return;
        }

        bool choseHome = person.UsualWorkParcelId == person.Household.ResidenceParcelId; // JLB 20120329 added these two lines
        IParcelWrapper chosenParcel = choseHome ? null : person.UsualWorkParcel;

        //RunModel(choiceProbabilityCalculator, person, sampleSize, person.UsualWorkParcel);
        RunModel(choiceProbabilityCalculator, person, sampleSize, chosenParcel, choseHome); // JLB 20120329 replaced above line
                                                                                            // when chosenParcel is null:
                                                                                            // DestinationSampler doesn't try to assign one of the sampled destinations as chosen
                                                                                            // choseHome is NOT null, and RunModel sets the oddball location as chosen

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, person, sampleSize);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
        ParcelWrapper choice = (ParcelWrapper)chosenAlternative.Choice;

        person.UsualWorkParcelId = choice.Id;
        person.UsualWorkParcel = choice;
        person.UsualWorkZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];

        SkimValue skimValue = ImpedanceRoster.GetValue("time", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, 1, person.Household.ResidenceParcel, choice);

        person.AutoTimeToUsualWork = skimValue.Variable;
        person.AutoDistanceToUsualWork = skimValue.BlendVariable;

        person.SetWorkParcelPredictions();
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonWrapper person, int sampleSize, IParcelWrapper choice = null, bool choseHome = false) {
      //MB check for access to new Actum person properties
      int checkPersInc = person.PersonalIncome;
      //end check
      //MB check for new hh properties
      //requres a cast to a household, and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)person.Household;
      IActumParcelWrapper residenceParcel = (IActumParcelWrapper)household.ResidenceParcel;
      int checkKids6To17 = household.Persons6to17;
      // end check

      bool incomeMissing = false;
      bool lowIncome = false;
      bool lowMediumIncome = false;
      bool mediumHighIncome = false;
      bool highIncome = false;
      double income = -1;

      int incomeBasis = 1;  // 1-person; 2-HH

      if (incomeBasis == 1) { //person income is basis
        income = person.PersonalIncome;
        if (income < 0) { incomeMissing = true; } else if (income < 300000) { lowIncome = true; }  // 20th percentile
                                                  else if (income < 400000) { lowMediumIncome = true; }  // 48th percentile
                                                  else if (income < 600000) { mediumHighIncome = true; }  // 82nd percentile
                                                  else { highIncome = true; }
      } else {  //household income is basis
        income = household.Income;
        if (income < 0) {incomeMissing = true;}
        else if (income < 300000) {lowIncome = true;}
        else if (income < 600000) { lowMediumIncome = true; }
        else if (income < 900000) {mediumHighIncome = true;}
        else {highIncome = true; }

      }

      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(Global.Settings.Purposes.Work, Global.Settings.TourPriorities.HomeBasedTour, Global.Settings.Modes.Sov, person.PersonType);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, residenceParcel);
      int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
      int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);

      WorkLocationUtilities workLocationUtilites = new WorkLocationUtilities(person, sampleSize, destinationArrivalTime, destinationDepartureTime);
      
      Dictionary<DestinationSampler.TourSampleItem, int> sampleItems = destinationSampler.SampleAndReturnTourDestinations(workLocationUtilites);
           
      int index = 0;
      foreach (KeyValuePair<DestinationSampler.TourSampleItem, int> sampleItem in sampleItems) {
        bool available = sampleItem.Key.Available;
        bool isChosen = sampleItem.Key.IsChosen;
        double adjustmentFactor = sampleItem.Key.AdjustmentFactor;
        IActumParcelWrapper destinationParcel = (IActumParcelWrapper)ChoiceModelFactory.Parcels[sampleItem.Key.ParcelId];

        //MB check for access to new Actum parcel properties
        //requires a cast above (DaySim.DomainModels.Actum.Wrappers.Interfaces was already in header)-can keep using variable destinationParcel
        double checkDestinationMZParkCost = destinationParcel.PublicParkingHourlyPriceBuffer1;
        //end check

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(index++, available, isChosen);

        if (!available) {
          continue;
        }

        alternative.Choice = destinationParcel;

        int hhDrivers = household.Size - household.Persons6to17 - household.KidsBetween0And4;  //uses new household attribute

        double workTourLogsum = 0D;
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(person, residenceParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers, 0.0);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, residenceParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, hhDrivers, 0.0, Global.Settings.Purposes.Work);
        workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();

        //int votSegment = household.GetVotALSegment();
        //GV: 12.3.2019 - getting values from MB's memo
        int votSegment =
          (household.Income <= 450000)
                    ? Global.Settings.VotALSegments.Low
                    : (household.Income <= 900000)
                        ? Global.Settings.VotALSegments.Medium
                        : Global.Settings.VotALSegments.High;

        //int taSegment = destinationParcel.TransitAccessSegment();
        //GV: 12.3.2019 - getting values from MB's memo
        //OBS - it has to be in km
        int taSegment =
           destinationParcel.GetDistanceToTransit() >= 0 && destinationParcel.GetDistanceToTransit() <= 0.4
              ? 0
              : destinationParcel.GetDistanceToTransit() > 0.4 && destinationParcel.GetDistanceToTransit() <= 1.6
                  ? 1
                  : 2;

        double aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment];

        double distanceFromOrigin = residenceParcel.DistanceFromOrigin(destinationParcel, 1);

        //GV: 14.3.2019 - piecewise distance
        // JB: I checked residenceParcel.DistanceFromOrigin, and I see that it looks up SOV distance in the LOS skims and then divides it by 10.  
        // So the thresholds as specified are 3.5km and 10 km.  
        // Those thresholds were probably set for a US model that would have made the threshold 3.5 miles and 10 miles.
        //double distance1 = Math.Min(distanceFromOrigin, .35);
        //double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
        //double distance3 = Math.Max(0, distanceFromOrigin - 1);
        double distance1 = Math.Min(distanceFromOrigin, 0.4);
        double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - 0.4, 2.0 - 0.4));
        double distance3 = Math.Max(0, distanceFromOrigin - 2.0);

        double distanceLog = Math.Log(1 + distanceFromOrigin);
        double distanceFromSchool = person.IsFullOrPartTimeWorker ? 0 : person.UsualSchoolParcel.DistanceFromSchoolLog(destinationParcel, 1);


        // parcel buffers
        double educationBuffer = Math.Log(destinationParcel.EmploymentEducationBuffer2 + 1);
        double governmentBuffer = Math.Log(destinationParcel.EmploymentGovernmentBuffer2 + 1);
        double officeBuffer = Math.Log(destinationParcel.EmploymentOfficeBuffer2 + 1);
        double serviceBuffer = Math.Log(destinationParcel.EmploymentServiceBuffer2 + 1);
        double householdsBuffer = Math.Log(destinationParcel.HouseholdsBuffer2 + 1);

        //				var retailBuffer = Math.Log(destinationParcel.EmploymentRetailBuffer2 + 1);
        double industrialAgricultureConstructionBuffer = Math.Log(destinationParcel.EmploymentIndustrialBuffer2 + destinationParcel.EmploymentAgricultureConstructionBuffer2 + 1);
        double foodBuffer = Math.Log(destinationParcel.EmploymentFoodBuffer2 + 1);
        double medicalBuffer = Math.Log(destinationParcel.EmploymentMedicalBuffer2 + 1);
        double employmentCommercialBuffer = Math.Log(destinationParcel.EmploymentRetailBuffer2 + destinationParcel.EmploymentServiceBuffer2 + 1);
        double employmentTotalBuffer = Math.Log(destinationParcel.EmploymentTotalBuffer2 + 1);
        double studentsUniversityBuffer = Math.Log(destinationParcel.StudentsUniversityBuffer2 + 1);
        double studentsK12Buffer = Math.Log(destinationParcel.StudentsK8Buffer2 + destinationParcel.StudentsHighSchoolBuffer2 + 1);

        //				var mixedUse4Index = destinationParcel.MixedUse4Index2();

        //size attributes (derived)
        double employmentIndustrialAgricultureConstruction = destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction;
        double employmentCommercial = destinationParcel.EmploymentRetail + destinationParcel.EmploymentService;

        // parking attributes
        double parcelParkingDensity = destinationParcel.ParkingDataAvailable * destinationParcel.EmployeeOnlyParkingSpaces / Math.Max(1.0, destinationParcel.EmploymentTotal);

        bool workLocationIsInCPHMuni = false;
        if (destinationParcel.LandUseCode == 101) {
          workLocationIsInCPHMuni = true;
        }

        bool workLocationIsInFDBMuni = false;
        if (destinationParcel.LandUseCode == 147) {
          workLocationIsInFDBMuni = true;
        }

        //GV: 13.3.2019 - added Frederiksberg Mun.
        bool workLocationIsInCPHcity = false;
        if (destinationParcel.LandUseCode == 101 || destinationParcel.LandUseCode == 147) {
          workLocationIsInCPHcity = true;
        }

        //GV: 13. mar. 2019 - no. of parkig places in the residental area
        double destNoParking = (
          //destinationParcel.ResidentialPermitOnlyParkingSpaces +
          //destinationParcel.PublicWithResidentialPermitAllowedParkingSpaces +
          //destinationParcel.PublicNoResidentialPermitAllowedParkingSpaces +
          destinationParcel.EmployeeOnlyParkingSpaces +
          destinationParcel.ElectricVehicleOnlyParkingSpaces);

        //GV: 13. mar. 2019 - no. of parkig places in Buffer1 area
        double Bf1NoParking = (
          //destinationParcel.ResidentialPermitOnlyParkingSpacesBuffer1 +
          //destinationParcel.PublicWithResidentialPermitAllowedParkingSpacesBuffer1 +
          //destinationParcel.PublicNoResidentialPermitAllowedParkingSpacesBuffer1 +
          destinationParcel.EmployeeOnlyParkingSpacesBuffer1 +
          destinationParcel.ElectricVehicleOnlyParkingSpacesBuffer1);

        //GV: 13. mar. 2019 - no. of parkig places in Buffer2 area
        double Bf2NoParking = (
          //destinationParcel.ResidentialPermitOnlyParkingSpacesBuffer2 +
          //destinationParcel.PublicWithResidentialPermitAllowedParkingSpacesBuffer2 +
          //destinationParcel.PublicNoResidentialPermitAllowedParkingSpacesBuffer2 +
          destinationParcel.EmployeeOnlyParkingSpacesBuffer2 +
          destinationParcel.ElectricVehicleOnlyParkingSpacesBuffer2);

        //GV: 13. mar. 2019 - no. of parkig places in the destination area 
        //double resParkingSpacesPerHH = (Math.Max(1.0, resNoParking)) / (Math.Max(1.0, residenceParcel.Households));
        double destParkingSpaces = (Math.Max(1.0, destNoParking));

        //GV: 13. mar. 2019 - no. of parkig places in the Buffer1 area 
        //double Bf1ParkingSpacesPerHH = (Math.Max(1.0, Bf1NoParking)) / (Math.Max(1.0, residenceParcel.HouseholdsBuffer1));
        double destBf1ParkingSpaces = (Math.Max(1.0, Bf1NoParking));

        //GV: 13. mar. 2019 - no. of parkig places in the Buffer2 area 
        //double Bf2ParkingSpacesPerHH = (Math.Max(1.0, Bf2NoParking)) / (Math.Max(1.0, residenceParcel.HouseholdsBuffer2));
        double destBf2ParkingSpaces = (Math.Max(1.0, Bf2NoParking));



        //non-size terms. 

        // sampling adjustment factor
        alternative.AddUtilityTerm(1, sampleItem.Key.AdjustmentFactor);

        // Residential density
        alternative.AddUtilityTerm(2, destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits / 1000.0);
        alternative.AddUtilityTerm(3, (lowIncome).ToFlag() * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits / 1000.0);
        alternative.AddUtilityTerm(4, person.IsFemale.ToFlag() * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits / 1000.0);

        //CPH Muni
        //alternative.AddUtilityTerm(5, workLocationIsInCPHMuni.ToFlag());
        //alternative.AddUtilityTerm(6, (lowIncome).ToFlag() * workLocationIsInCPHMuni.ToFlag());
        //alternative.AddUtilityTerm(7, (lowMediumIncome).ToFlag() * workLocationIsInCPHMuni.ToFlag());
        //alternative.AddUtilityTerm(8, (highIncome).ToFlag() * workLocationIsInCPHMuni.ToFlag());
        //alternative.AddUtilityTerm(9, (incomeMissing).ToFlag() * workLocationIsInCPHMuni.ToFlag());
        //alternative.AddUtilityTerm(10, person.Age * workLocationIsInCPHMuni.ToFlag());

        //GV: 13.3.2019 - CPH city
        alternative.AddUtilityTerm(5, workLocationIsInCPHcity.ToFlag());
        alternative.AddUtilityTerm(6, (lowIncome).ToFlag() * workLocationIsInCPHcity.ToFlag());
        alternative.AddUtilityTerm(7, (lowMediumIncome).ToFlag() * workLocationIsInCPHcity.ToFlag());
        alternative.AddUtilityTerm(8, (highIncome).ToFlag() * workLocationIsInCPHcity.ToFlag());
        alternative.AddUtilityTerm(9, (incomeMissing).ToFlag() * workLocationIsInCPHcity.ToFlag());
        alternative.AddUtilityTerm(10, person.Age * workLocationIsInCPHcity.ToFlag());

        //Live and work in same Muni
        alternative.AddUtilityTerm(11, (residenceParcel.LandUseCode == destinationParcel.LandUseCode).ToFlag());

        //Parking availability and price  (Goran, you need to add these, using parking attributes from COMPASS microzone file)
        //GV: 13. 3. 2019 - parking avail. in CPH
        alternative.AddUtilityTerm(12, destinationParcel.ParkingDataAvailable * Math.Log(destBf1ParkingSpaces) * (workLocationIsInCPHMuni).ToFlag());
        //GV: 13. 3. 2019 - parking avail. in Frederiksberg
        alternative.AddUtilityTerm(13, destinationParcel.ParkingDataAvailable * Math.Log(destBf1ParkingSpaces) * (workLocationIsInFDBMuni).ToFlag());
        //GV: 13. 3. 2019 - parking avail. in the rest of GCA
        alternative.AddUtilityTerm(14, destinationParcel.ParkingDataAvailable * Math.Log(destParkingSpaces) * (!workLocationIsInCPHcity).ToFlag());

        //GV: 13.3.2019 - testing parking costs separately for CPH, Frederiksberg, and rest of GCA gave not effect for the last two
        //GV: also, Parking Residental Permit happens only in the CPHcity, but the negative coeff. is not signf.
        //alternative.AddUtilityTerm(33, residenceParcel.ParkingDataAvailable * residenceParcel.ResidentialPermitDailyParkingPrices * (hhLivesInCPHCity).ToFlag());
        //alternative.AddUtilityTerm(34, residenceParcel.ParkingDataAvailable * residenceParcel.PublicParkingHourlyPrice);
        //alternative.AddUtilityTerm(33, residenceParcel.ParkingDataAvailable * (residenceParcel.ResidentialPermitDailyParkingPrices + residenceParcel.PublicParkingHourlyPrice));
        //alternative.AddUtilityTerm(33, residenceParcel.ParkingDataAvailable * (residenceParcel.ResidentialPermitDailyParkingPrices)); //wrong sign
        alternative.AddUtilityTerm(15, destinationParcel.ParkingDataAvailable * (destinationParcel.PublicParkingHourlyPrice));


        // Work tour logsum
        alternative.AddUtilityTerm(20, workTourLogsum);  // base logsum term
        alternative.AddUtilityTerm(21, (lowIncome).ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(22, (lowMediumIncome).ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(23, (highIncome).ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(24, incomeMissing.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(25, person.IsFemale.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(26, person.IsPartTimeWorker.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(27, person.IsNotFullOrPartTimeWorker.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(28, (person.OccupationCode == 8).ToFlag() * workTourLogsum); // self-employed
        alternative.AddUtilityTerm(29, person.Age * workTourLogsum);

        // Distance
        alternative.AddUtilityTerm(40, distanceLog);  // base distance term
        alternative.AddUtilityTerm(41, (lowIncome).ToFlag() * distanceLog);
        alternative.AddUtilityTerm(42, (lowMediumIncome).ToFlag() * distanceLog);
        //GV: 14.3.2019 - piecewise distance for high Imcome group
        //alternative.AddUtilityTerm(43, (highIncome).ToFlag() * distanceLog); //GV: this distance coeff. gives a positive sigh 
        alternative.AddUtilityTerm(43, (highIncome).ToFlag() * distance1);
        alternative.AddUtilityTerm(44, (highIncome).ToFlag() * distance2); //GV: HighIncome people travel long for work, i.e. the coeff. are positive  
        alternative.AddUtilityTerm(45, (highIncome).ToFlag() * distance3); //GV: HighIncome people travel long for work, i.e. the coeff. are positive 

        alternative.AddUtilityTerm(46, incomeMissing.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(47, person.IsFemale.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(48, person.IsPartTimeWorker.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(49, person.IsNotFullOrPartTimeWorker.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(50, (person.OccupationCode == 8).ToFlag() * distanceLog); // self-employed
        alternative.AddUtilityTerm(51, person.Age * distanceLog);

        //Distance from school for student worker
        alternative.AddUtilityTerm(60, person.IsStudentAge.ToFlag() * distanceFromSchool);

        //Aggregate logsum at work location
        alternative.AddUtilityTerm(61, person.IsFulltimeWorker.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(62, person.IsPartTimeWorker.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(63, person.IsNotFullOrPartTimeWorker.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(64, parcelParkingDensity);

        //Neighborhood
        // consider splitting into income categories
        alternative.AddUtilityTerm(101, (!incomeMissing).ToFlag() * serviceBuffer);
        alternative.AddUtilityTerm(102, (!incomeMissing).ToFlag() * educationBuffer);
        alternative.AddUtilityTerm(103, (!incomeMissing).ToFlag() * foodBuffer);
        alternative.AddUtilityTerm(104, (!incomeMissing).ToFlag() * governmentBuffer);
        alternative.AddUtilityTerm(105, (!incomeMissing).ToFlag() * officeBuffer);
        alternative.AddUtilityTerm(106, (!incomeMissing).ToFlag() * medicalBuffer);
        alternative.AddUtilityTerm(107, (!incomeMissing).ToFlag() * householdsBuffer);
        alternative.AddUtilityTerm(108, (!incomeMissing).ToFlag() * studentsUniversityBuffer);

        alternative.AddUtilityTerm(150, (!incomeMissing).ToFlag() * person.IsFulltimeWorker.ToFlag() * studentsK12Buffer);
        alternative.AddUtilityTerm(151, (!incomeMissing).ToFlag() * person.IsFulltimeWorker.ToFlag() * studentsUniversityBuffer);
        alternative.AddUtilityTerm(152, (!incomeMissing).ToFlag() * person.IsPartTimeWorker.ToFlag() * industrialAgricultureConstructionBuffer);
        alternative.AddUtilityTerm(153, (!incomeMissing).ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * foodBuffer);
        alternative.AddUtilityTerm(154, (!incomeMissing).ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * medicalBuffer);

        alternative.AddUtilityTerm(160, incomeMissing.ToFlag() * employmentTotalBuffer);
        alternative.AddUtilityTerm(161, incomeMissing.ToFlag() * studentsUniversityBuffer);
        alternative.AddUtilityTerm(162, incomeMissing.ToFlag() * employmentCommercialBuffer);


        //Size

        //The following set of terms comes from Stefan M's spec.  Consider trying them as a simpler alternative to the subsequent more detailed sets of size terms
        //alternative.AddUtilityTerm(201, (lowIncome).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        //alternative.AddUtilityTerm(202, (lowMediumIncome).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        //alternative.AddUtilityTerm(203, (mediumHighIncome).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        //alternative.AddUtilityTerm(204, (highIncome).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        //alternative.AddUtilityTerm(205, (highIncome).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial)); // second term allows first one to have base coef of 0
        //alternative.AddUtilityTerm(206, (incomeMissing.ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial)));
        //alternative.AddUtilityTerm(207, (lowIncome).ToFlag() * employmentCommercial);
        //alternative.AddUtilityTerm(208, (lowMediumIncome).ToFlag() * employmentCommercial);
        //alternative.AddUtilityTerm(209, (mediumHighIncome).ToFlag() * employmentCommercial);
        //alternative.AddUtilityTerm(210, (highIncome).ToFlag() * employmentCommercial);

        // GV. 14.3.2019 Employment types from JBs Buffered Microzone file  
        // Name: 1EmploymentEducation                  Explanation: Education and kindergarten
        // Name: 1EmploymentFood                       Explanation: Restaurants, cinema, sport, etc
        // Name: 1EmploymentGovernment                 Explanation: Public office
        // Name: EmploymentIndustrial                 Explanation: Industrial, transport, auto service, wholesale //GV: this one was missing, now incl. as 220, 230, 240, 250
        // Name: 1EmploymentMedical                    Explanation: Health, wellness and personal service
        // Name: 1EmploymentOffice                     Explanation: Private office
        // Name: 1EmploymentRetail                     Explanation: Retail
        // Name: 1EmploymentService                    Explanation: Supermarket, grocery, etc
        // Name: 1EmploymentAgricultureConstruction    Explanation: Agriculture, resources, construction
        // Name: EmploymentTotal

        alternative.AddUtilityTerm(220, (lowIncome).ToFlag() * destinationParcel.EmploymentIndustrial);
        alternative.AddUtilityTerm(221, (lowIncome).ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(222, (lowIncome).ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(223, (lowIncome).ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(224, (lowIncome).ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(225, (lowIncome).ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(226, (lowIncome).ToFlag() * destinationParcel.EmploymentRetail);
        alternative.AddUtilityTerm(227, (lowIncome).ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(228, (lowIncome).ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(229, (lowIncome).ToFlag() * destinationParcel.StudentsUniversity);

        alternative.AddUtilityTerm(230, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentIndustrial);
        alternative.AddUtilityTerm(231, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(232, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentEducation); 
        alternative.AddUtilityTerm(233, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(234, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(235, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(236, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentRetail);  
        alternative.AddUtilityTerm(237, (lowMediumIncome).ToFlag() * destinationParcel.EmploymentMedical);   
        alternative.AddUtilityTerm(238, (lowMediumIncome).ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(239, (lowMediumIncome).ToFlag() * destinationParcel.StudentsUniversity);

        alternative.AddUtilityTerm(240, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentIndustrial);
        alternative.AddUtilityTerm(241, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(242, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(243, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(244, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(245, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(246, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentRetail);
        alternative.AddUtilityTerm(247, (mediumHighIncome).ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(248, (mediumHighIncome).ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(249, (mediumHighIncome).ToFlag() * destinationParcel.StudentsUniversity);

        alternative.AddUtilityTerm(250, (highIncome).ToFlag() * destinationParcel.EmploymentIndustrial);
        alternative.AddUtilityTerm(251, (highIncome).ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(252, (highIncome).ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(253, (highIncome).ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(254, (highIncome).ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(255, (highIncome).ToFlag() * destinationParcel.EmploymentOffice); 
        alternative.AddUtilityTerm(256, (highIncome).ToFlag() * destinationParcel.EmploymentRetail);
        alternative.AddUtilityTerm(257, (highIncome).ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(258, (highIncome).ToFlag() * employmentIndustrialAgricultureConstruction); 
        alternative.AddUtilityTerm(259, (highIncome).ToFlag() * destinationParcel.StudentsUniversity);

        alternative.AddUtilityTerm(260, (!incomeMissing).ToFlag() * person.IsFulltimeWorker.ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(261, (!incomeMissing).ToFlag() * person.IsFulltimeWorker.ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(262, (!incomeMissing).ToFlag() * person.IsPartTimeWorker.ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(263, (!incomeMissing).ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(264, (!incomeMissing).ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(265, (!incomeMissing).ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentRetail);

        alternative.AddUtilityTerm(281, incomeMissing.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(282, incomeMissing.ToFlag() * destinationParcel.StudentsUniversity);
        alternative.AddUtilityTerm(283, incomeMissing.ToFlag() * employmentCommercial);

        // set shadow price depending on persontype and add it to utility
        // we are using the sampling adjustment factor assuming that it is 1
        alternative.AddUtilityTerm(1, destinationParcel.ShadowPriceForEmployment);

        //remove nesting for estimation of conditional MNL 
        alternative.AddNestedAlternative(sampleSize + 2, 0, 350);

      }

      // JLB 20120329 added third call parameter to idenitfy whether this alt is chosen or not
      ChoiceProbabilityCalculator.Alternative homeAlternative = choiceProbabilityCalculator.GetAlternative(sampleSize, true, choseHome);

      homeAlternative.Choice = residenceParcel;

      homeAlternative.AddUtilityTerm(180, 1);  //ASC
      homeAlternative.AddUtilityTerm(340, 1); //Size variable dummy 

      //make oddball alt unavailable and remove nesting for estimation of conditional MNL 
      //			alternative.Available = false;
      homeAlternative.AddNestedAlternative(sampleSize + 3, 1, 350);
    }

    private sealed class WorkLocationUtilities : ISamplingUtilities {
      private readonly IPersonWrapper _person;
      private readonly int _sampleSize;
      private readonly int _destinationArrivalTime;
      private readonly int _destinationDepartureTime;
      private readonly int[] _seedValues;

      public WorkLocationUtilities(IPersonWrapper person, int sampleSize, int destinationArrivalTime, int destinationDepartureTime) {
        _person = person;
        _sampleSize = sampleSize;
        _destinationArrivalTime = destinationArrivalTime;
        _destinationDepartureTime = destinationDepartureTime;
        _seedValues = ChoiceModelUtility.GetRandomSampling(_sampleSize, person.SeedValues[0]);
      }

      public int[] SeedValues => _seedValues;

      public void SetUtilities(ISampleItem sampleItem, int sampleFrequency) {
        if (sampleItem == null) {
          throw new ArgumentNullException("sampleItem");
        }
      }
    }
  }
}
