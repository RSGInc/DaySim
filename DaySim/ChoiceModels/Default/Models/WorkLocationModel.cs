// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;
using DaySim.Framework.Sampling;
using DaySim.Sampling;
//using DaySim.Framework.Factories;
//using System.Linq;


namespace DaySim.ChoiceModels.Default.Models {
  public class WorkLocationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "WorkLocationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 2;
    private const int TOTAL_LEVELS = 2;
    // regular and size parameters must be <= MAX_REGULAR_PARAMETER, balance is for OD shadow pricing coefficients
    private const int THETA_PARAMETER = 98;
    private const int MAX_REGULAR_PARAMETER = 166;
    private const int MaxDistrictNumber = 100;
    private const int MAX_PARAMETER = MAX_REGULAR_PARAMETER + MaxDistrictNumber * MaxDistrictNumber;

    private const bool ESTIMATE_NESTED_MODEL = true;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.WorkLocationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkLocationModelCoefficients, sampleSize + 1, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(IPersonWrapper person, int sampleSize) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(0);

      if (Global.Configuration.IsInEstimationMode) {
        //    if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        //        return;
        //    }
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
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
        person.WorkLocationLogsum = chosenAlternative.ComputeLogsum();

        person.UsualWorkParcelId = choice.Id;
        person.UsualWorkParcel = choice;
        person.UsualWorkZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];

        SkimValue skimValue = ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, 1, person.Household.ResidenceParcel, choice);

        person.AutoTimeToUsualWork = skimValue.Variable;
        person.AutoDistanceToUsualWork = skimValue.BlendVariable;

        person.SetWorkParcelPredictions();
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonWrapper person, int sampleSize, IParcelWrapper choice = null, bool choseHome = false) {
      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(Global.Settings.Purposes.Work, Global.Settings.TourPriorities.HomeBasedTour, Global.Settings.Modes.Sov, person.PersonType);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, person.Household.ResidenceParcel);
      int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
      int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
      WorkLocationUtilities workLocationUtilites = new WorkLocationUtilities(this, person, sampleSize, destinationArrivalTime, destinationDepartureTime);

      destinationSampler.SampleTourDestinations(workLocationUtilites);

      //var alternative = choiceProbabilityCalculator.GetAlternative(countSampled, true);  

      // JLB 20120329 added third call parameter to idenitfy whether this alt is chosen or not
      if (Global.Configuration.IsInEstimationMode && !ESTIMATE_NESTED_MODEL) {
        return;
      }
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(sampleSize, true, choseHome);

      alternative.Choice = person.Household.ResidenceParcel;

      alternative.AddUtilityTerm(41, 1);
      alternative.AddUtilityTerm(42, person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(43, person.IsStudentAge.ToFlag());
      alternative.AddUtilityTerm(44, person.IsFemale.ToFlag());
      alternative.AddUtilityTerm(89, 1);  //new dummy size variable for oddball alt
      alternative.AddUtilityTerm(90, 100);  //old dummy size variable for oddball alt

      // OD shadow pricing - add for work at home
      if (Global.Configuration.ShouldUseODShadowPricing && Global.Configuration.UseODShadowPricingForWorkAtHomeAlternative) {
        int res = person.Household.ResidenceParcel.District;
        int des = person.Household.ResidenceParcel.District;
        //var first = res <= des? res : des;
        //var second = res <= des? des : res;
        double shadowPriceConfigurationParameter = res == des ? Global.Configuration.WorkLocationOOShadowPriceCoefficient : Global.Configuration.WorkLocationODShadowPriceCoefficient;
        int odShadowPriceF12Value = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (res - 1) + des;
        alternative.AddUtilityTerm(odShadowPriceF12Value, shadowPriceConfigurationParameter);
      }

      // set shadow price depending on persontype and add it to utility
      // we are using the sampling adjustment factor assuming that it is 1
      if (Global.Configuration.ShouldUseShadowPricing && Global.Configuration.UseWorkShadowPricingForWorkAtHomeAlternative) {
        alternative.AddUtilityTerm(1, person.Household.ResidenceParcel.ShadowPriceForEmployment);
      }

      //make oddball alt unavailable and remove nesting for estimation of conditional MNL 
      alternative.AddNestedAlternative(sampleSize + 3, 1, THETA_PARAMETER);
    }

    protected virtual void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IPersonWrapper _person, IParcelWrapper destinationParcel) {
      //PSRC customization dll for example
      //Global.PrintFile.WriteLine("Generic Default WorkLocationModel.RegionSpecificCustomizations being called so must not be overridden by CustomizationDll");
    }

    private sealed class WorkLocationUtilities : ISamplingUtilities {
      private readonly WorkLocationModel _parentClass;
      private readonly IPersonWrapper _person;
      private readonly int _sampleSize;
      private readonly int _destinationArrivalTime;
      private readonly int _destinationDepartureTime;
      private readonly int[] _seedValues;

      public WorkLocationUtilities(WorkLocationModel parentClass, IPersonWrapper person, int sampleSize, int destinationArrivalTime, int destinationDepartureTime) {
        _parentClass = parentClass;
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

        ChoiceProbabilityCalculator.Alternative alternative = sampleItem.Alternative;

        //remove nesting for estimation of conditional MNL 
        if (!Global.Configuration.IsInEstimationMode || ESTIMATE_NESTED_MODEL) {
          alternative.AddNestedAlternative(_sampleSize + 2, 0, THETA_PARAMETER);
        }
        if (!alternative.Available) {
          return;
        }
        IParcelWrapper originParcel = _person.Household.ResidenceParcel;
        IParcelWrapper destinationParcel = ChoiceModelFactory.Parcels[sampleItem.ParcelId];
        //                var destinationZoneTotals = ChoiceModelRunner.ZoneTotals[destinationParcel.ZoneId];

        alternative.Choice = destinationParcel;



        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(_person, _person.Household.ResidenceParcel, destinationParcel, _destinationArrivalTime, _destinationDepartureTime, _person.Household.HouseholdTotals.DrivingAgeMembers);
        double workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        int votSegment = _person.Household.GetVotALSegment();
        int taSegment = destinationParcel.TransitAccessSegment();
        double aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment];

        double distanceFromOrigin = _person.Household.ResidenceParcel.DistanceFromOrigin(destinationParcel, 1);
        double distance1 = Math.Min(distanceFromOrigin, .35);
        double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
        double distance3 = Math.Max(0, distanceFromOrigin - 1);
        double distance20 = Math.Max(0, distanceFromOrigin - 2);
        double distance30 = Math.Max(0, distanceFromOrigin - 3);
        double distance40 = Math.Max(0, distanceFromOrigin - 4);
        double distanceLog = Math.Log(1 + distanceFromOrigin);
        double distanceFromSchool = _person.IsFullOrPartTimeWorker ? 0 : _person.UsualSchoolParcel.DistanceFromSchoolLog(destinationParcel, 1);

        // parcel buffers
        double educationBuffer = Math.Log(destinationParcel.EmploymentEducationBuffer2 + 1);
        double governmentBuffer = Math.Log(destinationParcel.EmploymentGovernmentBuffer2 + 1);
        double officeBuffer = Math.Log(destinationParcel.EmploymentOfficeBuffer2 + 1);
        double serviceBuffer = Math.Log(destinationParcel.EmploymentServiceBuffer2 + 1);
        double householdsBuffer = Math.Log(destinationParcel.HouseholdsBuffer2 + 1);

        //                var retailBuffer = Math.Log(destinationParcel.EmploymentRetailBuffer2 + 1);
        double industrialAgricultureConstructionBuffer = Math.Log(destinationParcel.EmploymentIndustrialBuffer2 + destinationParcel.EmploymentAgricultureConstructionBuffer2 + 1);
        double foodBuffer = Math.Log(destinationParcel.EmploymentFoodBuffer2 + 1);
        double medicalBuffer = Math.Log(destinationParcel.EmploymentMedicalBuffer2 + 1);
        double employmentTotalBuffer = Math.Log(destinationParcel.EmploymentTotalBuffer2 + 1);
        double studentsUniversityBuffer = Math.Log(destinationParcel.StudentsUniversityBuffer2 + 1);
        double studentsK12Buffer = Math.Log(destinationParcel.StudentsK8Buffer2 + destinationParcel.StudentsHighSchoolBuffer2 + 1);

        //                var mixedUse4Index = destinationParcel.MixedUse4Index2();

        //size attributes (derived)
        double employmentIndustrialAgricultureConstruction = destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction;

        // parking attributes
        double parcelParkingDensity = destinationParcel.ParcelParkingPerTotalEmployment();

        // connectivity attributes
        double c34Ratio = destinationParcel.C34RatioBuffer1();


        alternative.AddUtilityTerm(1, sampleItem.AdjustmentFactor);  //constrain coeff to 1
        alternative.AddUtilityTerm(2, _person.IsFulltimeWorker.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(3, _person.IsPartTimeWorker.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(4, _person.IsNotFullOrPartTimeWorker.ToFlag() * workTourLogsum);
        alternative.AddUtilityTerm(5, distanceLog); // for distance calibration
        alternative.AddUtilityTerm(6, _person.IsFulltimeWorker.ToFlag() * distance1);
        alternative.AddUtilityTerm(7, _person.IsFulltimeWorker.ToFlag() * distance2);
        alternative.AddUtilityTerm(8, _person.IsFulltimeWorker.ToFlag() * distance3);
        alternative.AddUtilityTerm(9, _person.IsPartTimeWorker.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(10, _person.IsNotFullOrPartTimeWorker.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(11, _person.Household.Has0To15KIncome.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(12, _person.Household.Has50To75KIncome.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(13, _person.Household.Has75To100KIncome.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(14, _person.IsFemale.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(15, _person.IsStudentAge.ToFlag() * distanceFromSchool);
        alternative.AddUtilityTerm(16, _person.IsFulltimeWorker.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(17, _person.IsPartTimeWorker.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(18, _person.IsNotFullOrPartTimeWorker.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(19, parcelParkingDensity);
        alternative.AddUtilityTerm(20, c34Ratio);
        //extra additive distance terms for calibrating longer distances
        alternative.AddUtilityTerm(46, distance20);
        alternative.AddUtilityTerm(47, distance30);
        alternative.AddUtilityTerm(48, distance40);

        //Neighborhood
        alternative.AddUtilityTerm(21, _person.Household.HasValidIncome.ToFlag() * serviceBuffer);
        alternative.AddUtilityTerm(22, _person.Household.HasValidIncome.ToFlag() * educationBuffer);
        alternative.AddUtilityTerm(23, _person.Household.HasValidIncome.ToFlag() * foodBuffer);
        alternative.AddUtilityTerm(24, _person.Household.HasValidIncome.ToFlag() * governmentBuffer);
        alternative.AddUtilityTerm(25, _person.Household.HasValidIncome.ToFlag() * officeBuffer);
        alternative.AddUtilityTerm(26, _person.Household.HasValidIncome.ToFlag() * medicalBuffer);
        alternative.AddUtilityTerm(27, _person.Household.HasValidIncome.ToFlag() * householdsBuffer);
        alternative.AddUtilityTerm(28, _person.Household.HasValidIncome.ToFlag() * studentsUniversityBuffer);

        alternative.AddUtilityTerm(29, _person.Household.HasValidIncome.ToFlag() * _person.IsFulltimeWorker.ToFlag() * studentsK12Buffer);
        alternative.AddUtilityTerm(30, _person.Household.HasValidIncome.ToFlag() * _person.IsFulltimeWorker.ToFlag() * studentsUniversityBuffer);
        alternative.AddUtilityTerm(31, _person.Household.HasValidIncome.ToFlag() * _person.IsPartTimeWorker.ToFlag() * industrialAgricultureConstructionBuffer);
        alternative.AddUtilityTerm(32, _person.Household.HasValidIncome.ToFlag() * _person.IsNotFullOrPartTimeWorker.ToFlag() * foodBuffer);
        alternative.AddUtilityTerm(33, _person.Household.HasValidIncome.ToFlag() * _person.IsNotFullOrPartTimeWorker.ToFlag() * medicalBuffer);

        alternative.AddUtilityTerm(34, _person.IsFulltimeWorker.ToFlag() * _person.Household.Has75KPlusIncome.ToFlag() * employmentTotalBuffer);
        alternative.AddUtilityTerm(35, _person.IsNotFullOrPartTimeWorker.ToFlag() * _person.Household.HasIncomeUnder50K.ToFlag() * governmentBuffer);
        alternative.AddUtilityTerm(36, _person.IsNotFullOrPartTimeWorker.ToFlag() * _person.Household.HasIncomeUnder50K.ToFlag() * employmentTotalBuffer);

        //Size
        alternative.AddUtilityTerm(51, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(52, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(53, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(54, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(55, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(56, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentRetail);
        alternative.AddUtilityTerm(57, _person.Household.HasValidIncome.ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(58, _person.Household.HasValidIncome.ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(59, _person.Household.HasValidIncome.ToFlag() * destinationParcel.StudentsUniversity);

        alternative.AddUtilityTerm(60, _person.Household.HasValidIncome.ToFlag() * _person.IsFulltimeWorker.ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(61, _person.Household.HasValidIncome.ToFlag() * _person.IsFulltimeWorker.ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(62, _person.Household.HasValidIncome.ToFlag() * _person.IsPartTimeWorker.ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(63, _person.Household.HasValidIncome.ToFlag() * _person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(64, _person.Household.HasValidIncome.ToFlag() * _person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentFood);
        alternative.AddUtilityTerm(65, _person.Household.HasValidIncome.ToFlag() * _person.IsNotFullOrPartTimeWorker.ToFlag() * destinationParcel.EmploymentRetail);

        alternative.AddUtilityTerm(66, _person.Household.HasIncomeUnder50K.ToFlag() * destinationParcel.EmploymentRetail);
        alternative.AddUtilityTerm(67, _person.Household.HasIncomeUnder50K.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(68, _person.Household.Has50To75KIncome.ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(69, _person.Household.Has50To75KIncome.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(70, _person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(71, _person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(72, _person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(73, _person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentOffice);

        alternative.AddUtilityTerm(74, _person.IsFulltimeWorker.ToFlag() * _person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(75, _person.IsFulltimeWorker.ToFlag() * (!_person.Household.Has75KPlusIncome).ToFlag() * employmentIndustrialAgricultureConstruction);
        alternative.AddUtilityTerm(76, _person.IsPartTimeWorker.ToFlag() * (!_person.Household.HasIncomeUnder50K).ToFlag() * destinationParcel.EmploymentMedical);
        alternative.AddUtilityTerm(77, (!_person.IsFulltimeWorker).ToFlag() * _person.Household.Has75KPlusIncome.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(78, _person.IsNotFullOrPartTimeWorker.ToFlag() * (!_person.Household.HasIncomeUnder50K).ToFlag() * destinationParcel.EmploymentRetail);

        alternative.AddUtilityTerm(79, _person.Household.HasMissingIncome.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(80, _person.Household.HasMissingIncome.ToFlag() * destinationParcel.StudentsUniversity);


        //add any region-specific new terms in region-specific class, using coefficient numbers 91-100 or other unused variable #
        _parentClass.RegionSpecificCustomizations(alternative, _person, destinationParcel);


        // OD shadow pricing
        if (Global.Configuration.ShouldUseODShadowPricing) {
          int res = _person.Household.ResidenceParcel.District;
          int des = destinationParcel.District;
          //var first = res <= des? res : des;
          //var second = res <= des? des : res;
          double shadowPriceConfigurationParameter = res == des ? Global.Configuration.WorkLocationOOShadowPriceCoefficient : Global.Configuration.WorkLocationODShadowPriceCoefficient;
          int odShadowPriceF12Value = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (res - 1) + des;
          alternative.AddUtilityTerm(odShadowPriceF12Value, shadowPriceConfigurationParameter);
        }


        // set shadow price depending on persontype and add it to utility
        // we are using the sampling adjustment factor assuming that it is 1
        if (Global.Configuration.ShouldUseShadowPricing) {
          alternative.AddUtilityTerm(1, destinationParcel.ShadowPriceForEmployment);
        }

      }
    }
  }
}
