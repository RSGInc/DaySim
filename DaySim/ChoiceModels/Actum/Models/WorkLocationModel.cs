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
    private const int MAX_PARAMETER = 99;

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
      int checkKids6To17 = household.Persons6to17;
      // end check



      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(Global.Settings.Purposes.Work, Global.Settings.TourPriorities.HomeBasedTour, Global.Settings.Modes.Sov, person.PersonType);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, person.Household.ResidenceParcel);
      int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
      int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
      WorkLocationUtilities workLocationUtilites = new WorkLocationUtilities(person, sampleSize, destinationArrivalTime, destinationDepartureTime);

      Dictionary<DestinationSampler.TourSampleItem, int> sampleItems = destinationSampler.SampleAndReturnTourDestinations(workLocationUtilites);

      int index = 0;
      foreach (KeyValuePair<DestinationSampler.TourSampleItem, int> sampleItem in sampleItems) {
        bool available = sampleItem.Key.Available;
        bool isChosen = sampleItem.Key.IsChosen;
        double adjustmentFactor = sampleItem.Key.AdjustmentFactor;
        IActumParcelWrapper destinationParcel = (IActumParcelWrapper) ChoiceModelFactory.Parcels[sampleItem.Key.ParcelId];

        //MB check for access to new Actum parcel properties
        //requires a cast above (DaySim.DomainModels.Actum.Wrappers.Interfaces was already in header)-can keep using variable destinationParcel
        double checkDestinationMZParkCost = destinationParcel.PublicParkingHourlyPriceBuffer1;
        //end check

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(index++, available, isChosen);

        if (!available) {
          continue;
        }

        alternative.Choice = destinationParcel;

        double workTourLogsum = 0D;
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(person, person.Household.ResidenceParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, person.Household.HouseholdTotals.DrivingAgeMembers, 0.0);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, person.Household.ResidenceParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, person.Household.HouseholdTotals.DrivingAgeMembers, 0.0, Global.Settings.Purposes.Work);
        workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();

        int votSegment = person.Household.GetVotALSegment();
        int taSegment = destinationParcel.TransitAccessSegment();
        double aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment];

        double distanceFromOrigin = person.Household.ResidenceParcel.DistanceFromOrigin(destinationParcel, 1);
        double distance1 = Math.Min(distanceFromOrigin, .35);
        double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
        double distance3 = Math.Max(0, distanceFromOrigin - 1);
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
        double employmentTotalBuffer = Math.Log(destinationParcel.EmploymentTotalBuffer2 + 1);
        double studentsUniversityBuffer = Math.Log(destinationParcel.StudentsUniversityBuffer2 + 1);
        double studentsK12Buffer = Math.Log(destinationParcel.StudentsK8Buffer2 + destinationParcel.StudentsHighSchoolBuffer2 + 1);

        //				var mixedUse4Index = destinationParcel.MixedUse4Index2();

        //size attributes (derived)
        double employmentIndustrialAgricultureConstruction = destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction;

        // parking attributes
        double parcelParkingDensity = destinationParcel.ParcelParkingPerTotalEmployment();

        // connectivity attributes
        double c34Ratio = destinationParcel.C34RatioBuffer1();


        // Stefan
        bool isInCopenhagenMunicipality = true; //destinationParcel.Municipality == 101;  Need to change this after Municipality property is added to Actum parcel file
        double employmentCommercial = destinationParcel.EmploymentRetail + destinationParcel.EmploymentService;
        double employmentCommercialBuffer1 = destinationParcel.EmploymentRetailBuffer1 + destinationParcel.EmploymentServiceBuffer1;

        double beta00002 = -2.53;
        double beta00003 = 2.65;
        double beta00004 = 1.57;
        double beta00005 = -0.18;
        double beta00006 = -0.43;
        double beta00007 = -0.19;
        double beta00008 = 0.33;
        double beta00009 = 0.007;

        double stefanUtility =
                beta00002 * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits +
                beta00003 * (person.Household.Income < 300000).ToFlag() * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits +
                beta00004 * person.IsFemale.ToFlag() * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits +
                beta00005 * isInCopenhagenMunicipality.ToFlag() +
                beta00006 * (person.Household.HasValidIncome && person.Household.Income < 300000).ToFlag() * isInCopenhagenMunicipality.ToFlag() +
                beta00007 * (person.Household.HasValidIncome && person.Household.Income >= 300000 && person.Household.Income < 600000).ToFlag() * isInCopenhagenMunicipality.ToFlag() +
                beta00008 * (person.Household.HasValidIncome && person.Household.Income >= 900000).ToFlag() * isInCopenhagenMunicipality.ToFlag() +
                beta00009 * person.Age * isInCopenhagenMunicipality.ToFlag() +
                0.0; // beta00010 * (person.Household.ResidenceParcel.Municipality == destination.Municipality).ToFlag();




        //Stefan non-size terms. 
        alternative.AddUtilityTerm(1, sampleItem.Key.AdjustmentFactor);
        //alternative.AddUtilityTerm(2, destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits);
        //alternative.AddUtilityTerm(3, (person.Household.Income < 300000).ToFlag() * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits);
        //alternative.AddUtilityTerm(4, person.IsFemale.ToFlag() * destinationParcel.Households / destinationParcel.ThousandsSquareLengthUnits);
        //alternative.AddUtilityTerm(5, isInCopenhagenMunicipality.ToFlag());
        //alternative.AddUtilityTerm(6, (person.Household.HasValidIncome && person.Household.Income < 300000).ToFlag() * isInCopenhagenMunicipality.ToFlag());
        //alternative.AddUtilityTerm(7, (person.Household.HasValidIncome && person.Household.Income >= 300000 && person.Household.Income < 600000).ToFlag() * isInCopenhagenMunicipality.ToFlag());
        //alternative.AddUtilityTerm(8, (person.Household.HasValidIncome && person.Household.Income >= 900000).ToFlag() * isInCopenhagenMunicipality.ToFlag());
        //alternative.AddUtilityTerm(9, person.Age * isInCopenhagenMunicipality.ToFlag());
        ////alternative.AddUtilityTerm(10, (person.Household.ResidenceParcel.Municipality == destination.Municipality).ToFlag());  // Acivate this after Municipality property is added to Actum parcel file
        //following logsums replace Stefan's car and public transport times 
        alternative.AddUtilityTerm(11, (person.Household.HasValidIncome && person.Household.Income < 300000).ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(12, (person.Household.HasValidIncome && person.Household.Income >= 300000 && person.Household.Income < 600000).ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(13, (person.Household.HasValidIncome && person.Household.Income >= 900000).ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(14, person.Household.HasMissingIncome.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(15, person.IsFemale.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(16, person.Age * workTourLogsum);
        //alternative.AddUtilityTerm(17, (person.MainOccupation == 50).ToFlag() * workTourLogsum); // self-employed
                                                                                                 //Stefan's composite term 18 replaces terms 2-10 above
        alternative.AddUtilityTerm(18, stefanUtility); // see above for this composite function of StefanMabitt's utility function

        //alternative.AddUtilityTerm(2, person.IsFulltimeWorker.ToFlag() * workTourLogsum);
        //alternative.AddUtilityTerm(3, person.IsPartTimeWorker.ToFlag() * workTourLogsum);
        //alternative.AddUtilityTerm(4, person.IsNotFullOrPartTimeWorker.ToFlag() * workTourLogsum);
        //alternative.AddUtilityTerm(5, distanceLog); // for distance calibration
        //alternative.AddUtilityTerm(6, person.IsFulltimeWorker.ToFlag() * distance1);
        //alternative.AddUtilityTerm(7, person.IsFulltimeWorker.ToFlag() * distance2);
        //alternative.AddUtilityTerm(8, person.IsFulltimeWorker.ToFlag() * distance3);
        //alternative.AddUtilityTerm(9, person.IsPartTimeWorker.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(10, person.IsNotFullOrPartTimeWorker.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(11, person.Household.Has0To15KIncome.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(12, person.Household.Has50To75KIncome.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(13, person.Household.Has75To100KIncome.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(14, person.IsFemale.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(15, person.IsStudentAge.ToFlag() * distanceFromSchool);
        //alternative.AddUtilityTerm(16, person.IsFulltimeWorker.ToFlag() * aggregateLogsum);
        //alternative.AddUtilityTerm(17, person.IsPartTimeWorker.ToFlag() * aggregateLogsum);
        //alternative.AddUtilityTerm(18, person.IsNotFullOrPartTimeWorker.ToFlag() * aggregateLogsum);
        //alternative.AddUtilityTerm(19, parcelParkingDensity);
        //alternative.AddUtilityTerm(20, c34Ratio);

        //Neighborhood
        //alternative.AddUtilityTerm(21, person.Household.HasValidIncome.ToFlag() * serviceBuffer);
        //alternative.AddUtilityTerm(22, person.Household.HasValidIncome.ToFlag() * educationBuffer);
        //alternative.AddUtilityTerm(23, person.Household.HasValidIncome.ToFlag() * foodBuffer);
        //alternative.AddUtilityTerm(24, person.Household.HasValidIncome.ToFlag() * governmentBuffer);
        //alternative.AddUtilityTerm(25, person.Household.HasValidIncome.ToFlag() * officeBuffer);
        //alternative.AddUtilityTerm(26, person.Household.HasValidIncome.ToFlag() * medicalBuffer);
        //alternative.AddUtilityTerm(27, person.Household.HasValidIncome.ToFlag() * householdsBuffer);
        //alternative.AddUtilityTerm(28, person.Household.HasValidIncome.ToFlag() * studentsUniversityBuffer);

        //alternative.AddUtilityTerm(29, person.Household.HasValidIncome.ToFlag() * person.IsFulltimeWorker.ToFlag() * studentsK12Buffer);
        //alternative.AddUtilityTerm(30, person.Household.HasValidIncome.ToFlag() * person.IsFulltimeWorker.ToFlag() * studentsUniversityBuffer);
        //alternative.AddUtilityTerm(31, person.Household.HasValidIncome.ToFlag() * person.IsPartTimeWorker.ToFlag() * industrialAgricultureConstructionBuffer);
        //alternative.AddUtilityTerm(32, person.Household.HasValidIncome.ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * foodBuffer);
        //alternative.AddUtilityTerm(33, person.Household.HasValidIncome.ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * medicalBuffer);

        //alternative.AddUtilityTerm(34, person.IsFulltimeWorker.ToFlag() * person.Household.Has75KPlusIncome.ToFlag() * employmentTotalBuffer);
        //alternative.AddUtilityTerm(35, person.IsNotFullOrPartTimeWorker.ToFlag() * person.Household.HasIncomeUnder50K.ToFlag() * governmentBuffer);
        //alternative.AddUtilityTerm(36, person.IsNotFullOrPartTimeWorker.ToFlag() * person.Household.HasIncomeUnder50K.ToFlag() * employmentTotalBuffer);

        //Size
        // Stefan size terms.  
        // Note:  the following assumes: (1) Stefan's size variables enter his utility function as a logsum, a la BAL; 
        //                               (2) Jobs--commercial and Jobs--finance apply to hh w unknown incomes in Stefan's spec
        //        If his size variables enter linearly, then if I want to replicate them I should not use alogit size functions.
        //        If Jobs--commercial and Jobs--finance don't apply to missing incomes, then I need to change the size functions below
        alternative.AddUtilityTerm(51, (person.Household.HasValidIncome && person.Household.Income < 300000).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        alternative.AddUtilityTerm(52, (person.Household.HasValidIncome && person.Household.Income >= 300000 && person.Household.Income < 600000).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        alternative.AddUtilityTerm(53, (person.Household.HasValidIncome && person.Household.Income >= 600000 && person.Household.Income < 900000).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        alternative.AddUtilityTerm(54, (person.Household.HasValidIncome && person.Household.Income >= 900000).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial));
        alternative.AddUtilityTerm(55, (person.Household.HasValidIncome && person.Household.Income >= 900000).ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial)); // second term allows first one to have base coef of 0
        alternative.AddUtilityTerm(56, (person.Household.HasMissingIncome.ToFlag() * (destinationParcel.EmploymentTotal - employmentCommercial)));
        alternative.AddUtilityTerm(57, (person.Household.HasValidIncome && person.Household.Income < 300000).ToFlag() * employmentCommercial);
        alternative.AddUtilityTerm(58, (person.Household.HasValidIncome && person.Household.Income >= 300000 && person.Household.Income < 600000).ToFlag() * employmentCommercial);
        alternative.AddUtilityTerm(59, (person.Household.HasValidIncome && person.Household.Income >= 600000 && person.Household.Income < 900000).ToFlag() * employmentCommercial);
        alternative.AddUtilityTerm(60, (person.Household.HasValidIncome && person.Household.Income >= 900000).ToFlag() * employmentCommercial);
        alternative.AddUtilityTerm(61, person.Household.HasMissingIncome.ToFlag() * employmentCommercial);
        alternative.AddUtilityTerm(62, destinationParcel.EmploymentOffice);
        //The following combine with 51-55, 56-60 and 61 to include size of entire buffer region in main size variables
        alternative.AddUtilityTerm(63, (destinationParcel.EmploymentTotalBuffer1 - destinationParcel.EmploymentTotal) - (employmentCommercialBuffer1 - employmentCommercial));
        alternative.AddUtilityTerm(64, employmentCommercialBuffer1 - employmentCommercial);
        alternative.AddUtilityTerm(65, destinationParcel.EmploymentOfficeBuffer1 - destinationParcel.EmploymentOffice);

        //alternative.AddUtilityTerm(51, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentService);
        //alternative.AddUtilityTerm(52, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentEducation);
        //alternative.AddUtilityTerm(53, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentFood);
        //alternative.AddUtilityTerm(54, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentGovernment);
        //alternative.AddUtilityTerm(55, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentOffice);
        //alternative.AddUtilityTerm(56, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentRetail);
        //alternative.AddUtilityTerm(57, person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentMedical);
        //alternative.AddUtilityTerm(58, person.Household.HasValidIncome.ToFlag() * employmentIndustrialAgricultureConstruction);
        //alternative.AddUtilityTerm(59, person.Household.HasValidIncome.ToFlag() * destinationParcel.StudentsUniversity);

        //alternative.AddUtilityTerm(60, person.Household.HasValidIncome.ToFlag() * person.IsFulltimeWorker.ToFlag() * destinationParcel.EmploymentGovernment);
        //alternative.AddUtilityTerm(61, person.Household.HasValidIncome.ToFlag() * person.IsFulltimeWorker.ToFlag() * employmentIndustrialAgricultureConstruction);
        //alternative.AddUtilityTerm(62, person.Household.HasValidIncome.ToFlag() * person.IsPartTimeWorker.ToFlag() * employmentIndustrialAgricultureConstruction);
        //alternative.AddUtilityTerm(63, person.Household.HasValidIncome.ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentEducation);
        //alternative.AddUtilityTerm(64, person.Household.HasValidIncome.ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentFood);
        //alternative.AddUtilityTerm(65, person.Household.HasValidIncome.ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentRetail);

        //alternative.AddUtilityTerm(66, person.Household.HasIncomeUnder50K.ToFlag() * destinationParcel.EmploymentRetail);
        //alternative.AddUtilityTerm(67, person.Household.HasIncomeUnder50K.ToFlag() * destinationParcel.EmploymentService);
        //alternative.AddUtilityTerm(68, person.Household.Has50To75KIncome.ToFlag() * destinationParcel.EmploymentMedical);
        //alternative.AddUtilityTerm(69, person.Household.Has50To75KIncome.ToFlag() * destinationParcel.EmploymentOffice);
        //alternative.AddUtilityTerm(70, person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentEducation);
        //alternative.AddUtilityTerm(71, person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentGovernment);
        //alternative.AddUtilityTerm(72, person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentMedical);
        //alternative.AddUtilityTerm(73, person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentOffice);

        //alternative.AddUtilityTerm(74, person.IsFulltimeWorker.ToFlag() * person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentGovernment);
        //alternative.AddUtilityTerm(75, person.IsFulltimeWorker.ToFlag() * (!person.Household.Has75KPlusIncome).ToFlag() * employmentIndustrialAgricultureConstruction);
        //alternative.AddUtilityTerm(76, person.IsPartTimeWorker.ToFlag() * (!person.Household.HasIncomeUnder50K).ToFlag() * destinationParcel.EmploymentMedical);
        //alternative.AddUtilityTerm(77, (!person.IsFulltimeWorker).ToFlag() * person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentOffice);
        //alternative.AddUtilityTerm(78, person.IsNotFullOrPartTimeWorker.ToFlag() * (!person.Household.HasIncomeUnder50K).ToFlag() * destinationParcel.EmploymentRetail);

        //alternative.AddUtilityTerm(79, person.Household.HasMissingIncome.ToFlag() * destinationParcel.EmploymentTotal);
        //alternative.AddUtilityTerm(80, person.Household.HasMissingIncome.ToFlag() * destinationParcel.StudentsUniversity);

        // set shadow price depending on persontype and add it to utility
        // we are using the sampling adjustment factor assuming that it is 1
        alternative.AddUtilityTerm(1, destinationParcel.ShadowPriceForEmployment);

        //remove nesting for estimation of conditional MNL 
        alternative.AddNestedAlternative(sampleSize + 2, 0, 98);

      }

      // JLB 20120329 added third call parameter to idenitfy whether this alt is chosen or not
      ChoiceProbabilityCalculator.Alternative homeAlternative = choiceProbabilityCalculator.GetAlternative(sampleSize, true, choseHome);

      homeAlternative.Choice = person.Household.ResidenceParcel;

      homeAlternative.AddUtilityTerm(41, 1);
     // homeAlternative.AddUtilityTerm(42, (person.MainOccupation == 50).ToFlag()); // self-employed

      //homeAlternative.AddUtilityTerm(42, person.IsPartTimeWorker.ToFlag());
      //homeAlternative.AddUtilityTerm(43, person.IsStudentAge.ToFlag());
      //homeAlternative.AddUtilityTerm(44, person.IsFemale.ToFlag());
      homeAlternative.AddUtilityTerm(90, 1);

      //make oddball alt unavailable and remove nesting for estimation of conditional MNL 
      //			alternative.Available = false;
      homeAlternative.AddNestedAlternative(sampleSize + 3, 1, 98);
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
