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
  public class SchoolLocationModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "ActumSchoolLocationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 2;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.SchoolLocationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.SchoolLocationModelCoefficients, sampleSize + 1, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonWrapper person, int sampleSize) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(1);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (person.UsualSchoolParcel == null) {
          return;
        }

        bool choseHome = person.UsualSchoolParcelId == person.Household.ResidenceParcelId; // JLB 20120403 added these two lines
        IParcelWrapper chosenParcel = choseHome ? null : person.UsualSchoolParcel;

        //RunModel(choiceProbabilityCalculator, person, sampleSize, person.UsualSchoolParcel);
        RunModel(choiceProbabilityCalculator, person, sampleSize, chosenParcel, choseHome); // JLB 20120403 replaced above line
                                                                                            // when chosenParcel is null:
                                                                                            // DestinationSampler doesn't try to assign one of the sampled destinations as chosen
                                                                                            // choseHome is NOT null, and RunModel sets the oddball location as chosen

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, person, sampleSize);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
        ParcelWrapper choice = (ParcelWrapper)chosenAlternative.Choice;
        person.SchoolLocationLogsum = chosenAlternative.ComputeLogsum();

        person.UsualSchoolParcelId = choice.Id;
        person.UsualSchoolParcel = choice;
        person.UsualSchoolZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];

        SkimValue skimValue = ImpedanceRoster.GetValue("time", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, 1, person.Household.ResidenceParcel, choice);

        person.AutoTimeToUsualSchool = skimValue.Variable;
        person.AutoDistanceToUsualSchool = skimValue.BlendVariable;

        person.SetSchoolParcelPredictions();
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

      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(Global.Settings.Purposes.School, Global.Settings.TourPriorities.UsualLocation, Global.Settings.Modes.Sov, person.PersonType);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, person.Household.ResidenceParcel);
      int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
      int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);
      SchoolLocationUtilities schoolLocationUtilities = new SchoolLocationUtilities(person, sampleSize, destinationArrivalTime, destinationDepartureTime);

      Dictionary<DestinationSampler.TourSampleItem, int> sampleItems = destinationSampler.SampleAndReturnTourDestinations(schoolLocationUtilities);

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

        //                var destinationZoneTotals = ChoiceModelRunner.ZoneTotals[destinationParcel.ZoneId];
        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(index++, available, isChosen);

        if (!available) {
          continue;
        }

        alternative.Choice = destinationParcel;

        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, person.Household.ResidenceParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, person.Household.HouseholdTotals.DrivingAgeMembers, 0.0, Global.Settings.Purposes.School);
        double schoolTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        int votSegment = person.Household.GetVotALSegment();
        int taSegment = destinationParcel.TransitAccessSegment();
        double aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment];

        double distanceFromOrigin = person.Household.ResidenceParcel.DistanceFromOrigin(destinationParcel, 1);
        double distance1 = Math.Min(distanceFromOrigin, .1);
        double distance2 = Math.Max(0, Math.Min(distanceFromOrigin - .1, .5 - .1));
        double distance3 = Math.Max(0, distanceFromOrigin - .5);
        double distanceLog = Math.Log(1 + distanceFromOrigin);
        double distanceFromWork = person.IsFullOrPartTimeWorker ? person.UsualWorkParcel.DistanceFromWorkLog(destinationParcel, 1) : 0;
        //                var millionsSquareFeet = destinationZoneTotals.MillionsSquareFeet();

        // zone densities
        //                var eduDensity = destinationZoneTotals.GetEmploymentEducationDensity(millionsSquareFeet);
        //                var govDensity = destinationZoneTotals.GetEmploymentGovernmentDensity(millionsSquareFeet);
        //                var offDensity = destinationZoneTotals.GetEmploymentOfficeDensity(millionsSquareFeet);
        //                var serDensity = destinationZoneTotals.GetEmploymentServiceDensity(millionsSquareFeet);
        //                var houDensity = destinationZoneTotals.GetHouseholdsDensity(millionsSquareFeet);

        // parcel buffers
        double educationBuffer1 = Math.Log(destinationParcel.EmploymentEducationBuffer1 + 1);
        //var governmentBuffer1 = Math.Log(destinationParcel.EmploymentGovernmentBuffer1 + 1);
        //var officeBuffer1 = Math.Log(destinationParcel.EmploymentOfficeBuffer1 + 1);
        //var serviceBuffer1 = Math.Log(destinationParcel.EmploymentServiceBuffer1 + 1);
        //var householdsBuffer1 = Math.Log(destinationParcel.HouseholdsBuffer1 + 1);
        //var retailBuffer1 = Math.Log(destinationParcel.EmploymentRetailBuffer1 + 1);
        //var industrialAgricultureConstructionBuffer1 = Math.Log(destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1 + 1);
        //var foodBuffer1 = Math.Log(destinationParcel.EmploymentFoodBuffer1 + 1);
        //var medicalBuffer1 = Math.Log(destinationParcel.EmploymentMedicalBuffer1 + 1);
        //var employmentTotalBuffer1 = Math.Log(destinationParcel.EmploymentTotalBuffer1 + 1);
        double studentsUniversityBuffer1 = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1);
        double studentsK8Buffer1 = Math.Log(destinationParcel.StudentsK8Buffer1 + 1);
        double studentsHighSchoolBuffer1 = Math.Log(destinationParcel.StudentsHighSchoolBuffer1 + 1);

        //var educationBuffer2 = Math.Log(destinationParcel.EmploymentEducationBuffer2 + 1);
        //var governmentBuffer2 = Math.Log(destinationParcel.EmploymentGovernmentBuffer2 + 1);
        //var officeBuffer2 = Math.Log(destinationParcel.EmploymentOfficeBuffer2 + 1);
        //var serviceBuffer2 = Math.Log(destinationParcel.EmploymentServiceBuffer2 + 1);
        double householdsBuffer2 = Math.Log(destinationParcel.HouseholdsBuffer2 + 1);
        //var retailBuffer2 = Math.Log(destinationParcel.EmploymentRetailBuffer2 + 1);
        //var industrialAgricultureConstructionBuffer2 = Math.Log(destinationParcel.EmploymentIndustrialBuffer2 + destinationParcel.EmploymentAgricultureConstructionBuffer2 + 1);
        //var foodBuffer2 = Math.Log(destinationParcel.EmploymentFoodBuffer2 + 1);
        //var medicalBuffer2 = Math.Log(destinationParcel.EmploymentMedicalBuffer2 + 1);
        double employmentTotalBuffer2 = Math.Log(destinationParcel.EmploymentTotalBuffer2 + 1);
        double studentsUniversityBuffer2 = Math.Log(destinationParcel.StudentsUniversityBuffer2 + 1);
        //var studentsK8Buffer2 = Math.Log(destinationParcel.StudentsK8Buffer2 + 1);
        //var studentsHighSchoolBuffer2 = Math.Log(destinationParcel.StudentsHighSchoolBuffer2 + 1);

        //                var educationBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentEducationBuffer1 - destinationParcel.EmploymentEducation)  + 1);
        //                var governmentBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentGovernmentBuffer1 - destinationParcel.EmploymentGovernment)  + 1);
        //                var officeBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentOfficeBuffer1 - destinationParcel.EmploymentOffice)  + 1);
        //                var serviceBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentServiceBuffer1 - destinationParcel.EmploymentService)  + 1);
        //                var householdsBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.HouseholdsBuffer1 - destinationParcel.Households)  + 1);
        //                var retailBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentRetailBuffer1 - destinationParcel.EmploymentRetail)  + 1);
        //                var industrialAgricultureConstructionBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1
        //                    - destinationParcel.EmploymentIndustrial - destinationParcel.EmploymentAgricultureConstruction)    + 1);
        //                var foodBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentFoodBuffer1 - destinationParcel.EmploymentFood)  + 1);
        //                var medicalBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentMedicalBuffer1 - destinationParcel.EmploymentMedical)  + 1);
        //                var employmentTotalBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentTotalBuffer1 - destinationParcel.EmploymentTotal)  + 1);
        //                var studentsUniversityBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.StudentsUniversityBuffer1 - destinationParcel.StudentsUniversity)  + 1);
        //                var studentsK8Buffer1 = Math.Log(Math.Max(0.0, destinationParcel.StudentsK8Buffer1 - destinationParcel.StudentsK8)  + 1);
        //                var studentsHighSchoolBuffer1 = Math.Log(Math.Max(0.0, destinationParcel.StudentsHighSchoolBuffer1 - destinationParcel.StudentsHighSchool)  + 1);
        //
        //                var educationBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentEducationBuffer2 - destinationParcel.EmploymentEducation)  + 1);
        //                var governmentBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentGovernmentBuffer2 - destinationParcel.EmploymentGovernment)  + 1);
        //                var officeBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentOfficeBuffer2 - destinationParcel.EmploymentOffice)  + 1);
        //                var serviceBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentServiceBuffer2 - destinationParcel.EmploymentService)  + 1);
        //                var householdsBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.HouseholdsBuffer2 - destinationParcel.Households)  + 1);
        //                var retailBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentRetailBuffer2 - destinationParcel.EmploymentRetail)  + 1);
        //                var industrialAgricultureConstructionBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentIndustrialBuffer2 + destinationParcel.EmploymentAgricultureConstructionBuffer2
        //                    - destinationParcel.EmploymentIndustrial - destinationParcel.EmploymentAgricultureConstruction)    + 1);
        //                var foodBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentFoodBuffer2 - destinationParcel.EmploymentFood)  + 1);
        //                var medicalBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentMedicalBuffer2 - destinationParcel.EmploymentMedical)  + 1);
        //                var employmentTotalBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.EmploymentTotalBuffer2 - destinationParcel.EmploymentTotal)  + 1);
        //                var studentsUniversityBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.StudentsUniversityBuffer2 - destinationParcel.StudentsUniversity)  + 1);
        //                var studentsK8Buffer2 = Math.Log(Math.Max(0.0, destinationParcel.StudentsK8Buffer2 - destinationParcel.StudentsK8)  + 1);
        //                var studentsHighSchoolBuffer2 = Math.Log(Math.Max(0.0, destinationParcel.StudentsHighSchoolBuffer2 - destinationParcel.StudentsHighSchool)  + 1);

        alternative.AddUtilityTerm(1, sampleItem.Key.AdjustmentFactor);

        alternative.AddUtilityTerm(2, person.IsChildUnder5.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(3, person.IsChildAge5Through15.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(4, person.IsDrivingAgeStudent.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(5, person.IsUniversityStudent.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(6, (!person.IsStudentAge).ToFlag() * schoolTourLogsum);

        alternative.AddUtilityTerm(7, person.IsChildUnder5.ToFlag() * distance1);
        alternative.AddUtilityTerm(8, person.IsChildUnder5.ToFlag() * distance2);
        alternative.AddUtilityTerm(9, person.IsChildUnder5.ToFlag() * distance3);
        alternative.AddUtilityTerm(10, person.IsChildAge5Through15.ToFlag() * distance1);
        alternative.AddUtilityTerm(11, person.IsChildAge5Through15.ToFlag() * distance2);
        alternative.AddUtilityTerm(12, person.IsChildAge5Through15.ToFlag() * distance3);
        alternative.AddUtilityTerm(13, person.IsDrivingAgeStudent.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(14, person.IsUniversityStudent.ToFlag() * distanceLog);
        alternative.AddUtilityTerm(15, (!person.IsStudentAge).ToFlag() * distanceLog);
        alternative.AddUtilityTerm(16, (!person.IsStudentAge).ToFlag() * distanceFromWork);

        alternative.AddUtilityTerm(17, person.IsChildUnder5.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(18, person.IsChildAge5Through15.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(19, person.IsDrivingAgeStudent.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(20, person.IsUniversityStudent.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(21, (!person.IsStudentAge).ToFlag() * aggregateLogsum);

        //Neighborhood
        alternative.AddUtilityTerm(30, person.IsChildUnder5.ToFlag() * householdsBuffer2);
        alternative.AddUtilityTerm(31, person.IsChildUnder5.ToFlag() * studentsHighSchoolBuffer1);
        alternative.AddUtilityTerm(32, person.IsChildUnder5.ToFlag() * employmentTotalBuffer2);
        alternative.AddUtilityTerm(33, person.IsChildAge5Through15.ToFlag() * studentsK8Buffer1);
        alternative.AddUtilityTerm(34, person.IsDrivingAgeStudent.ToFlag() * studentsHighSchoolBuffer1);
        alternative.AddUtilityTerm(35, person.IsUniversityStudent.ToFlag() * educationBuffer1);
        alternative.AddUtilityTerm(36, person.IsAdult.ToFlag() * studentsUniversityBuffer1);
        alternative.AddUtilityTerm(37, person.IsAdult.ToFlag() * studentsUniversityBuffer2);
        alternative.AddUtilityTerm(38, person.IsAdult.ToFlag() * studentsK8Buffer1);

        //Size
        alternative.AddUtilityTerm(61, person.IsChildUnder5.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(62, person.IsChildUnder5.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(63, person.IsChildUnder5.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(64, person.IsChildUnder5.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(65, person.IsChildUnder5.ToFlag() * 10.0 * destinationParcel.Households);
        alternative.AddUtilityTerm(66, person.IsChildUnder5.ToFlag() * destinationParcel.StudentsK8);
        alternative.AddUtilityTerm(67, person.IsChildAge5Through15.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(68, person.IsChildAge5Through15.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(69, person.IsChildAge5Through15.ToFlag() * destinationParcel.StudentsHighSchool);
        alternative.AddUtilityTerm(70, person.IsChildAge5Through15.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(71, person.IsChildAge5Through15.ToFlag() * 10.0 * destinationParcel.Households);
        alternative.AddUtilityTerm(72, person.IsChildAge5Through15.ToFlag() * destinationParcel.StudentsK8);
        alternative.AddUtilityTerm(73, person.IsDrivingAgeStudent.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(74, person.IsDrivingAgeStudent.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(75, person.IsDrivingAgeStudent.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(76, person.IsDrivingAgeStudent.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(77, person.IsDrivingAgeStudent.ToFlag() * 10.0 * destinationParcel.Households);
        alternative.AddUtilityTerm(78, person.IsDrivingAgeStudent.ToFlag() * destinationParcel.StudentsHighSchool);
        alternative.AddUtilityTerm(79, person.IsAdult.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(80, person.IsAdult.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(81, person.IsAdult.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(82, person.IsAdult.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(83, person.IsAdult.ToFlag() * destinationParcel.StudentsUniversity);
        alternative.AddUtilityTerm(84, person.IsAdult.ToFlag() * destinationParcel.StudentsHighSchool);

        // set shadow price depending on persontype and add it to utility
        // we are using the sampling adjustment factor assuming that it is 1

        if (Global.Configuration.ShouldUseShadowPricing) {
          alternative.AddUtilityTerm(1, person.IsAdult ? destinationParcel.ShadowPriceForStudentsUniversity : destinationParcel.ShadowPriceForStudentsK12);
        }

        //remove nesting for estimation of conditional MNL 
        alternative.AddNestedAlternative(sampleSize + 2, 0, 99);
      }


      // JLB 20120403 added third call parameter to idenitfy whether this alt is chosen or not
      ChoiceProbabilityCalculator.Alternative homeAlternative = choiceProbabilityCalculator.GetAlternative(sampleSize, true, choseHome);

      homeAlternative.Choice = person.Household.ResidenceParcel;

      homeAlternative.AddUtilityTerm(50, 1);
      homeAlternative.AddUtilityTerm(51, (!person.IsStudentAge).ToFlag());
      homeAlternative.AddUtilityTerm(52, person.Household.Size);
      homeAlternative.AddUtilityTerm(97, 1); //new dummy size variable for oddball alt
      homeAlternative.AddUtilityTerm(98, 100); //old dummy size variable for oddball alt

      //make oddball alt unavailable and remove nesting for estimation of conditional MNL 
      //            alternative.Available = false;
      homeAlternative.AddNestedAlternative(sampleSize + 3, 1, 99);
    }

    private sealed class SchoolLocationUtilities : ISamplingUtilities {
      private readonly IPersonWrapper _person;
      private readonly int _sampleSize;
      private readonly int _destinationArrivalTime;
      private readonly int _destinationDepartureTime;
      private readonly int[] _seedValues;

      public SchoolLocationUtilities(IPersonWrapper person, int sampleSize, int destinationArrivalTime, int destinationDepartureTime) {
        _person = person;
        _sampleSize = sampleSize;
        _destinationArrivalTime = destinationArrivalTime;
        _destinationDepartureTime = destinationDepartureTime;
        _seedValues = ChoiceModelUtility.GetRandomSampling(_sampleSize, person.SeedValues[1]);
      }

      public int[] SeedValues => _seedValues;

      public void SetUtilities(ISampleItem sampleItem, int sampleFrequency) {
        if (sampleItem == null) {
          throw new ArgumentNullException("sampleItem");
        }

//        ChoiceProbabilityCalculator.Alternative alternative = sampleItem.Alternative;


      }
    }
  }
}
