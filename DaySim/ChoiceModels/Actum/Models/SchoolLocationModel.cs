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
    public const string CHOICE_MODEL_NAME = "ActumSchoolLocationModel";
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
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)person.Household;
      int numberAdults = household.Size - household.KidsBetween0And4 - household.KidsBetween5And15 - household.Persons6to17;

      bool isAge0to5 = person.PersonType == 8? true:false;
      bool isPrimaryStudent = person.PersonType == 7? true:false;
      bool isSecondaryStudent = person.PersonType == 6? true:false;
      bool isUniversityStudent = person.PersonType == 5? true:false;

      IActumParcelWrapper residenceParcel = (IActumParcelWrapper) household.ResidenceParcel;

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

        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(index++, available, isChosen);

        if (!available) {
          continue;
        }

        alternative.Choice = destinationParcel;

        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(person, residenceParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, numberAdults, 0.0, Global.Settings.Purposes.School);
        double schoolTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
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

        //GV: 8. 3. 2019 - Distance differt. per Age
        //Kintergarder; 0.2 and 0.5 km
        double distanceCh05_1 = Math.Min(distanceFromOrigin, .2);
        double distanceCh05_2 = Math.Max(0, Math.Min(distanceFromOrigin - .2, .5 - .2)); 
        double distanceCh05_3 = Math.Max(0, distanceFromOrigin - .5);

        //Primary school; 0.25 and 0.8 km 
        double distancePS_1 = Math.Min(distanceFromOrigin, .25);
        double distancePS_2 = Math.Max(0, Math.Min(distanceFromOrigin - .25, .8 - .25));
        double distancePS_3 = Math.Max(0, distanceFromOrigin - .8);

        //Secondary school; 0.5 and 2.5 km
        double distanceSS_1 = Math.Min(distanceFromOrigin, .5);
        double distanceSS_2 = Math.Max(0, Math.Min(distanceFromOrigin - .5, 2.5 - .5));
        double distanceSS_3 = Math.Max(0, distanceFromOrigin - 2.5);

        //University; 2 and 5 km
        double distanceUni_1 = Math.Min(distanceFromOrigin, 2);
        double distanceUni_2 = Math.Max(0, Math.Min(distanceFromOrigin - 2, 5 - 2));
        double distanceUni_3 = Math.Max(0, distanceFromOrigin - 5);

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
        double householdsBuffer1 = Math.Log(destinationParcel.HouseholdsBuffer1 + 1);
        double studentsUniversityBuffer1 = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1);
        double studentsK8Buffer1 = Math.Log(destinationParcel.StudentsK8Buffer1 + 1);
        double studentsHighSchoolBuffer1 = Math.Log(destinationParcel.StudentsHighSchoolBuffer1 + 1);

        double educationBuffer2 = Math.Log(destinationParcel.EmploymentEducationBuffer2 + 1);
        double householdsBuffer2 = Math.Log(destinationParcel.HouseholdsBuffer2 + 1);
        double employmentTotalBuffer2 = Math.Log(destinationParcel.EmploymentTotalBuffer2 + 1);
        double studentsUniversityBuffer2 = Math.Log(destinationParcel.StudentsUniversityBuffer2 + 1);
        double studentsK8Buffer2 = Math.Log(destinationParcel.StudentsK8Buffer2 + 1);
        double studentsHighSchoolBuffer2 = Math.Log(destinationParcel.StudentsHighSchoolBuffer2 + 1);

        alternative.AddUtilityTerm(1, sampleItem.Key.AdjustmentFactor);  

        //GV: 7. mar. 2019 - estimate coeff. 2-5
        alternative.AddUtilityTerm(2, isAge0to5.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(3, isPrimaryStudent.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(4, isSecondaryStudent.ToFlag() * schoolTourLogsum);
        alternative.AddUtilityTerm(5, isUniversityStudent.ToFlag() * schoolTourLogsum);

        alternative.AddUtilityTerm(6, (!person.IsStudentAge).ToFlag() * schoolTourLogsum);

        //GV: 12.3.2019 - piecewise linear specificaion, in km
        alternative.AddUtilityTerm(7, isAge0to5.ToFlag() * distanceCh05_1);
        alternative.AddUtilityTerm(8, isAge0to5.ToFlag() * distanceCh05_2);
        alternative.AddUtilityTerm(9, isAge0to5.ToFlag() * distanceCh05_3);

        //GV: 12.3.2019 - piecewise linear specificaion, in km
        alternative.AddUtilityTerm(10, isPrimaryStudent.ToFlag() * distancePS_1);
        alternative.AddUtilityTerm(11, isPrimaryStudent.ToFlag() * distancePS_2);
        alternative.AddUtilityTerm(12, isPrimaryStudent.ToFlag() * distancePS_3);

        //GV: 12.3.2019 - piecewise linear specificaion, in km
        alternative.AddUtilityTerm(13, isSecondaryStudent.ToFlag() * distanceSS_1);
        alternative.AddUtilityTerm(14, isSecondaryStudent.ToFlag() * distanceSS_2);
        alternative.AddUtilityTerm(15, isSecondaryStudent.ToFlag() * distanceSS_3);

        //GV: 12.3.2019 - piecewise linear specificaion, in km
        alternative.AddUtilityTerm(16, isUniversityStudent.ToFlag() * distanceUni_1);
        alternative.AddUtilityTerm(17, isUniversityStudent.ToFlag() * distanceUni_2);
        alternative.AddUtilityTerm(18, isUniversityStudent.ToFlag() * distanceUni_3);

        //alternative.AddUtilityTerm(13, isSecondaryStudent.ToFlag() * distanceLog);
        //alternative.AddUtilityTerm(14, isUniversityStudent.ToFlag() * distanceLog);
        //GV: 13.5.2019 - change as JB wanted from LogDist to Dist
        //alternative.AddUtilityTerm(19, (!person.IsStudentAge).ToFlag() * distanceLog);
        alternative.AddUtilityTerm(19, (!person.IsStudentAge).ToFlag() * distanceFromOrigin);
        alternative.AddUtilityTerm(20, (!person.IsStudentAge).ToFlag() * distanceFromWork);

        //GV: 13.5.2019 - piecewise linear specificaion, in km
        alternative.AddUtilityTerm(21, (!person.IsStudentAge).ToFlag() * distanceUni_1);
        alternative.AddUtilityTerm(22, (!person.IsStudentAge).ToFlag() * distanceUni_2);
        alternative.AddUtilityTerm(23, (!person.IsStudentAge).ToFlag() * distanceUni_3);
        
        alternative.AddUtilityTerm(24, isAge0to5.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(25, isPrimaryStudent.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(26, isSecondaryStudent.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(27, isUniversityStudent.ToFlag() * aggregateLogsum);
        alternative.AddUtilityTerm(28, (!person.IsStudentAge).ToFlag() * aggregateLogsum);

        //Neighborhood
        alternative.AddUtilityTerm(30, isAge0to5.ToFlag() * householdsBuffer2);
        alternative.AddUtilityTerm(31, isAge0to5.ToFlag() * studentsHighSchoolBuffer1);
        alternative.AddUtilityTerm(32, isAge0to5.ToFlag() * employmentTotalBuffer2);
        alternative.AddUtilityTerm(33, isPrimaryStudent.ToFlag() * studentsK8Buffer1);
        alternative.AddUtilityTerm(34, isSecondaryStudent.ToFlag() * studentsHighSchoolBuffer1);
        alternative.AddUtilityTerm(35, person.IsAdult.ToFlag() * educationBuffer1);
        alternative.AddUtilityTerm(36, person.IsAdult.ToFlag() * studentsUniversityBuffer1);
        alternative.AddUtilityTerm(37, person.IsAdult.ToFlag() * studentsUniversityBuffer2);
        alternative.AddUtilityTerm(38, person.IsAdult.ToFlag() * studentsK8Buffer1);

        //Size
        alternative.AddUtilityTerm(61, isAge0to5.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(62, isAge0to5.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(63, isAge0to5.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(64, isAge0to5.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(65, isAge0to5.ToFlag() * 10.0 * destinationParcel.Households);
        alternative.AddUtilityTerm(66, isAge0to5.ToFlag() * destinationParcel.StudentsK8);
        alternative.AddUtilityTerm(67, isPrimaryStudent.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(68, isPrimaryStudent.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(69, isPrimaryStudent.ToFlag() * destinationParcel.StudentsHighSchool);
        alternative.AddUtilityTerm(70, isPrimaryStudent.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(71, isPrimaryStudent.ToFlag() * 10.0 * destinationParcel.Households);
        alternative.AddUtilityTerm(72, isPrimaryStudent.ToFlag() * destinationParcel.StudentsK8);
        alternative.AddUtilityTerm(73, isSecondaryStudent.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(74, isSecondaryStudent.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(75, isSecondaryStudent.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(76, isSecondaryStudent.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(77, isSecondaryStudent.ToFlag() * 10.0 * destinationParcel.Households);
        alternative.AddUtilityTerm(78, isSecondaryStudent.ToFlag() * destinationParcel.StudentsHighSchool);
        alternative.AddUtilityTerm(79, person.IsAdult.ToFlag() * destinationParcel.EmploymentEducation);
        alternative.AddUtilityTerm(80, person.IsAdult.ToFlag() * destinationParcel.EmploymentService);
        alternative.AddUtilityTerm(81, person.IsAdult.ToFlag() * destinationParcel.EmploymentOffice);
        alternative.AddUtilityTerm(82, person.IsAdult.ToFlag() * destinationParcel.EmploymentTotal);
        alternative.AddUtilityTerm(83, person.IsAdult.ToFlag() * destinationParcel.StudentsUniversity);
        alternative.AddUtilityTerm(84, person.IsAdult.ToFlag() * destinationParcel.StudentsHighSchool);
        alternative.AddUtilityTerm(85, person.IsAdult.ToFlag() * destinationParcel.EmploymentGovernment);
        alternative.AddUtilityTerm(86, isAge0to5.ToFlag() * destinationParcel.EmploymentGovernment); 
         
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

      homeAlternative.Choice = household.ResidenceParcel;

      homeAlternative.AddUtilityTerm(50, 1);
      homeAlternative.AddUtilityTerm(51, (!person.IsStudentAge).ToFlag());
      homeAlternative.AddUtilityTerm(52, household.Size);
      homeAlternative.AddUtilityTerm(97, 1); //new dummy size variable for oddball alt
      //homeAlternative.AddUtilityTerm(98, 100); //old dummy size variable for oddball alt

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
