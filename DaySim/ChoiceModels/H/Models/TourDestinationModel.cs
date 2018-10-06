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

namespace DaySim.ChoiceModels.H.Models {
  public class TourDestinationModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "HTourDestinationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 300;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.OtherTourDestinationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.OtherTourDestinationModelCoefficients, sampleSize + 1, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(ITourWrapper tour, IHouseholdDayWrapper householdDay, int sampleSize, IParcelWrapper constrainedParcel = null) {
      if (tour == null) {
        throw new ArgumentNullException("tour");
      }

      tour.PersonDay.ResetRandom(20 + tour.Sequence - 1);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
        if (!TourDestinationUtilities.ShouldRunInEstimationModeForModel(tour)) {
          return;
        }
        // JLB 20140421 add the following to keep from estimatign twice for the same tour
        if (tour.DestinationModeAndTimeHaveBeenSimulated) {
          return;
        }

      } else if (tour.DestinationPurpose == Global.Settings.Purposes.School) {
        // the following lines were redundant.  Usual destination properties are set in GetMandatoryTourSimulatedData(); 
        // sets the destination for the school tour
        //tour.DestinationParcelId = tour.Person.UsualSchoolParcelId;
        //tour.DestinationParcel = tour.Person.UsualSchoolParcel;
        //tour.DestinationZoneKey = tour.Person.UsualSchoolZoneKey;
        //tour.DestinationAddressType = Global.Settings.AddressTypes.UsualSchool;
        return;
      } else if (tour.DestinationPurpose == Global.Settings.Purposes.Work
            && tour.DestinationAddressType == Global.Settings.AddressTypes.UsualWorkplace) {
        return;
      } else if (constrainedParcel != null) {
        tour.DestinationParcel = constrainedParcel;
        tour.DestinationParcelId = constrainedParcel.Id;
        tour.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[constrainedParcel.ZoneId];  // mb fixed 
        tour.DestinationAddressType = Global.Settings.AddressTypes.Other;
        return;
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (constrainedParcel == null) {
          RunModel(choiceProbabilityCalculator, tour, householdDay, sampleSize, tour.DestinationParcel);

          choiceProbabilityCalculator.WriteObservation();
        }
      } else {
        RunModel(choiceProbabilityCalculator, tour, householdDay, sampleSize);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
          if (!Global.Configuration.IsInEstimationMode) {
            tour.PersonDay.IsValid = false;
            tour.PersonDay.HouseholdDay.IsValid = false;
          }
          return;
        }

        ParcelWrapper choice = (ParcelWrapper)chosenAlternative.Choice;

        tour.DestinationParcelId = choice.Id;
        tour.DestinationParcel = choice;
        tour.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];
        tour.DestinationAddressType = Global.Settings.AddressTypes.Other;

      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IHouseholdDayWrapper householdDay, int sampleSize, IParcelWrapper choice = null) {
      //            var household = tour.Household;
      IPersonWrapper person = tour.Person;
      IPersonDayWrapper personDay = tour.PersonDay;

      //            var totalAvailableMinutes =
      //                tour.ParentTour == null
      //                    ? personDay.TimeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay)
      //                    : tour.ParentTour.TimeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay);


      TimeWindow timeWindow = new TimeWindow();
      if (tour.JointTourSequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          ITourWrapper tInJoint = pDay.Tours.Find(t => t.JointTourSequence == tour.JointTourSequence);
          if (!(tInJoint == null)) {
            // set jointTour time window
            timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
          }
        }
      } else if (tour.ParentTour == null) {
        timeWindow.IncorporateAnotherTimeWindow(personDay.TimeWindow);
      }

      timeWindow.SetBusyMinutes(Global.Settings.Times.EndOfRelevantWindow, Global.Settings.Times.MinutesInADay + 1);

      int maxAvailableMinutes =
                (tour.JointTourSequence > 0 || tour.ParentTour == null)
                ? timeWindow.MaxAvailableMinutesAfter(Global.Settings.Times.FiveAM)
                    : tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime;


      //            var hoursAvailableInverse =
      //                tour.IsHomeBasedTour
      //                    ? (personDay.HomeBasedTours - personDay.SimulatedHomeBasedTours + 1) / (Math.Max(totalAvailableMinutes - 360, 30) / 60D)
      //                    : 1 / (Math.Max(totalAvailableMinutes, 1) / 60D);

      int fastestAvailableTimeOfDay =
                tour.IsHomeBasedTour || tour.ParentTour == null
                    ? 1
                    : tour.ParentTour.DestinationArrivalTime + (tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime) / 2;

      int tourCategory = tour.GetTourCategory();
      //            var primaryFlag = ChoiceModelUtility.GetPrimaryFlag(tourCategory);
      int secondaryFlag = ChoiceModelUtility.GetSecondaryFlag(tourCategory);

      ChoiceModelUtility.DrawRandomTourTimePeriodsActum(tour, tourCategory);

      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(tour.DestinationPurpose, tour.IsHomeBasedTour ? Global.Settings.TourPriorities.HomeBasedTour : Global.Settings.TourPriorities.WorkBasedTour, Global.Settings.Modes.Sov, person.PersonType);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, tour.OriginParcel);
      TourDestinationUtilities tourDestinationUtilities = new TourDestinationUtilities(tour, sampleSize, secondaryFlag, personDay.GetIsWorkOrSchoolPattern().ToFlag(), personDay.GetIsOtherPattern().ToFlag(), fastestAvailableTimeOfDay, maxAvailableMinutes);

      destinationSampler.SampleTourDestinations(tourDestinationUtilities);
    }

    private sealed class TourDestinationUtilities : ISamplingUtilities {
      private readonly ITourWrapper _tour;
      private readonly int _secondaryFlag;
      private readonly int _workOrSchoolPatternFlag;
      private readonly int _otherPatternFlag;
      private readonly int _fastestAvailableTimeOfDay;
      private readonly int _maxAvailableMinutes;
      private readonly int[] _seedValues;

      public TourDestinationUtilities(ITourWrapper tour, int sampleSize, int secondaryFlag, int workOrSchoolPatternFlag, int otherPatternFlag, int fastestAvailableTimeOfDay, int maxAvailableMinutes) {
        _tour = tour;
        _secondaryFlag = secondaryFlag;
        _workOrSchoolPatternFlag = workOrSchoolPatternFlag;
        _otherPatternFlag = otherPatternFlag;
        _fastestAvailableTimeOfDay = fastestAvailableTimeOfDay;
        _maxAvailableMinutes = maxAvailableMinutes;
        _seedValues = ChoiceModelUtility.GetRandomSampling(sampleSize, tour.Person.SeedValues[20 + tour.Sequence - 1]);
      }

      public int[] SeedValues => _seedValues;

      public void SetUtilities(ISampleItem sampleItem, int sampleFrequency) {
        if (sampleItem == null) {
          throw new ArgumentNullException("sampleItem");
        }

        ChoiceProbabilityCalculator.Alternative alternative = sampleItem.Alternative;

        if (!alternative.Available) {
          return;
        }

        IHouseholdWrapper household = _tour.Household;
        IPersonWrapper person = _tour.Person;
        //var personDay = _tour.PersonDay;
        bool householdHasChildren = household.HasChildren;
        bool householdHasNoChildren = householdHasChildren ? false : true;

        IParcelWrapper destinationParcel = ChoiceModelFactory.Parcels[sampleItem.ParcelId];


        int jointTourFlag = (_tour.JointTourSequence > 0).ToFlag();


        double fastestTravelTime =
                    ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _fastestAvailableTimeOfDay, _tour.OriginParcel, destinationParcel).Variable +
                    ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _fastestAvailableTimeOfDay, destinationParcel, _tour.OriginParcel).Variable;

        if (fastestTravelTime >= _maxAvailableMinutes) {
          alternative.Available = false;

          return;
        }

        alternative.Choice = destinationParcel;

        //double tourLogsum = 0;
        double jointTourLogsum = 0;
        double workTourLogsum = 0;
        double otherTourLogsum = 0;
        double subtourLogsum = 0;

        if (_tour.IsHomeBasedTour) {
          if (_tour.DestinationPurpose == Global.Settings.Purposes.Work) {
            //var destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
            //var destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
            ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(_tour, destinationParcel, _tour.Household.VehiclesAvailable, _tour.Person.GetTransitFareDiscountFraction());
            workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
          }
          //                    else if (_tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
          //                        var nestedAlternative = (Global.ChoiceModelDictionary.Get("HEscortTourModeModel") as HEscortTourModeModel).RunNested(_tour, destinationParcel);
          //                        tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
          //                    }
          else {
            ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<OtherHomeBasedTourModeTimeModel>().RunNested(_tour, destinationParcel, _tour.Household.VehiclesAvailable, _tour.Person.GetTransitFareDiscountFraction());
            if (_tour.JointTourSequence > 0) {
              jointTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
            } else {
              otherTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
            }
          }
        } else {
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkBasedSubtourModeTimeModel>().RunNested(_tour, destinationParcel, _tour.Household.VehiclesAvailable, _tour.Person.GetTransitFareDiscountFraction());
          subtourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }

        //var purpose = _tour.TourPurposeSegment;
        //var carOwnership = person.CarOwnershipSegment;
        //var votSegment = _tour.VotALSegment;
        //var transitAccess = destinationParcel.TransitAccessSegment();
        //var aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][purpose][carOwnership][votSegment][transitAccess];
        //var aggregateLogsumHomeBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][transitAccess];
        //var aggregateLogsumWorkBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.Work_BASED][carOwnership][votSegment][transitAccess];

        double distanceFromOrigin = _tour.OriginParcel.DistanceFromOrigin(destinationParcel, _tour.DestinationArrivalTime);
        double piecewiseDistanceFrom5To10Miles = Math.Max(0, Math.Min(distanceFromOrigin - .5, 1 - .5));
        double piecewiseDistanceFrom10MilesToInfinity = Math.Max(0, distanceFromOrigin - 1);
        double piecewiseDistanceFrom0To1Mile = Math.Min(distanceFromOrigin, .10);
        double piecewiseDistanceFrom1To5Miles = Math.Max(0, Math.Min(distanceFromOrigin - .1, .5 - .1));
        double piecewiseDistanceFrom1To3AndAHalfMiles = Math.Max(0, Math.Min(distanceFromOrigin - .1, .35 - .1));
        double piecewiseDistanceFrom3AndAHalfTo10Miles = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
        double distanceFromOriginLog = Math.Log(1 + distanceFromOrigin);
        //var distanceFromWorkLog = person.UsualWorkParcel.DistanceFromWorkLog(destinationParcel, 1);
        double distanceFromSchoolLog = person.UsualSchoolParcel.DistanceFromSchoolLog(destinationParcel, 1);

        double timePressure = Math.Log(1 - fastestTravelTime / _maxAvailableMinutes);

        // log transforms of buffers for Neighborhood effects
        double logOfOnePlusEmploymentEducationBuffer1 = Math.Log(destinationParcel.EmploymentEducationBuffer1 + 1.0);
        double logOfOnePlusEmploymentFoodBuffer1 = Math.Log(destinationParcel.EmploymentFoodBuffer1 + 1.0);
        double logOfOnePlusEmploymentGovernmentBuffer1 = Math.Log(destinationParcel.EmploymentGovernmentBuffer1 + 1.0);
        double logOfOnePlusEmploymentOfficeBuffer1 = Math.Log(destinationParcel.EmploymentOfficeBuffer1 + 1.0);
        double logOfOnePlusEmploymentRetailBuffer1 = Math.Log(destinationParcel.EmploymentRetailBuffer1 + 1.0);
        double logOfOnePlusEmploymentServiceBuffer1 = Math.Log(destinationParcel.EmploymentServiceBuffer1 + 1.0);
        double logOfOnePlusEmploymentMedicalBuffer1 = Math.Log(destinationParcel.EmploymentMedicalBuffer1 + 1.0);
        double logOfOnePlusEmploymentIndustrial_Ag_ConstructionBuffer1 = Math.Log(destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1 + 1.0);
        double logOfOnePlusEmploymentTotalBuffer1 = Math.Log(destinationParcel.EmploymentTotalBuffer1 + 1.0);
        double logOfOnePlusHouseholdsBuffer1 = Math.Log(destinationParcel.HouseholdsBuffer1 + 1.0);
        double logOfOnePlusStudentsK12Buffer1 = Math.Log(destinationParcel.StudentsK8Buffer1 + destinationParcel.StudentsHighSchoolBuffer1 + 1.0);
        double logOfOnePlusStudentsUniversityBuffer1 = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1.0);
        //                var EMPHOU_B = Math.Log(destinationParcel.EmploymentTotalBuffer1 + destinationParcel.HouseholdsBuffer1 + 1.0);

        //var logOfOnePlusParkingOffStreetDailySpacesBuffer1 = Math.Log(1 + destinationParcel.ParkingOffStreetPaidDailySpacesBuffer1);
        // connectivity attributes
        //var c34Ratio = destinationParcel.C34RatioBuffer1();

        //var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership); // exludes no cars
        //var noCarCompetitionFlag = FlagUtility.GetNoCarCompetitionFlag(carOwnership);
        //var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);

        alternative.AddUtilityTerm(2, sampleItem.AdjustmentFactor);
        alternative.AddUtilityTerm(3, workTourLogsum);
        alternative.AddUtilityTerm(4, otherTourLogsum);
        alternative.AddUtilityTerm(5, jointTourLogsum);
        alternative.AddUtilityTerm(6, subtourLogsum);

        //subpopulation-specific terms
        //alternative.AddUtilityTerm(260, _secondaryFlag * _workOrSchoolPatternFlag * piecewiseDistanceFrom0To1Mile);
        //alternative.AddUtilityTerm(261, _secondaryFlag * _workOrSchoolPatternFlag * piecewiseDistanceFrom1To5Miles);
        //alternative.AddUtilityTerm(262, _secondaryFlag * _workOrSchoolPatternFlag * piecewiseDistanceFrom5To10Miles);
        //alternative.AddUtilityTerm(263, _secondaryFlag * _workOrSchoolPatternFlag * piecewiseDistanceFrom10MilesToInfinity);
        alternative.AddUtilityTerm(260, _secondaryFlag * _workOrSchoolPatternFlag * distanceFromOriginLog);
        //alternative.AddUtilityTerm(264, _secondaryFlag * _otherPatternFlag * piecewiseDistanceFrom0To1Mile);
        //alternative.AddUtilityTerm(265, _secondaryFlag * _otherPatternFlag * piecewiseDistanceFrom1To5Miles);
        //alternative.AddUtilityTerm(266, _secondaryFlag * _otherPatternFlag * piecewiseDistanceFrom5To10Miles);
        //alternative.AddUtilityTerm(267, _secondaryFlag * _otherPatternFlag * piecewiseDistanceFrom10MilesToInfinity);
        alternative.AddUtilityTerm(264, _secondaryFlag * _otherPatternFlag * distanceFromOriginLog);

        alternative.AddUtilityTerm(268, (!_tour.IsHomeBasedTour).ToFlag() * distanceFromOriginLog);
        //alternative.AddUtilityTerm(269, household.Has0To15KIncome.ToFlag() * distanceFromOriginLog);
        //alternative.AddUtilityTerm(270, household.HasMissingIncome.ToFlag() * distanceFromOriginLog);
        //alternative.AddUtilityTerm(271, person.IsRetiredAdult.ToFlag() * distanceFromOriginLog);
        //alternative.AddUtilityTerm(272, person.IsUniversityStudent.ToFlag() * distanceFromOriginLog);
        alternative.AddUtilityTerm(273, person.IsChildAge5Through15.ToFlag() * distanceFromOriginLog);
        //alternative.AddUtilityTerm(274, person.IsChildUnder5.ToFlag() * distanceFromOriginLog);

        alternative.AddUtilityTerm(275, (_tour.IsHomeBasedTour).ToFlag() * timePressure);
        alternative.AddUtilityTerm(276, (_tour.IsHomeBasedTour).ToFlag() * distanceFromSchoolLog);
        //alternative.AddUtilityTerm(14, distanceFromWorkLog);

        //alternative.AddUtilityTerm(277, (carCompetitionFlag + noCarCompetitionFlag) * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
        //alternative.AddUtilityTerm(278, noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
        //alternative.AddUtilityTerm(279, carCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
        //alternative.AddUtilityTerm(280, noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());

        //alternative.AddUtilityTerm(281, noCarsFlag * c34Ratio);
        //alternative.AddUtilityTerm(282, noCarCompetitionFlag * c34Ratio);
        //alternative.AddUtilityTerm(283, (carCompetitionFlag + noCarCompetitionFlag) * logOfOnePlusParkingOffStreetDailySpacesBuffer1);

        alternative.AddUtilityTerm(284, jointTourFlag * piecewiseDistanceFrom0To1Mile);
        alternative.AddUtilityTerm(285, jointTourFlag * piecewiseDistanceFrom1To5Miles);
        alternative.AddUtilityTerm(286, jointTourFlag * piecewiseDistanceFrom5To10Miles);
        alternative.AddUtilityTerm(287, jointTourFlag * piecewiseDistanceFrom10MilesToInfinity);

        if (_tour.DestinationPurpose == Global.Settings.Purposes.Work) {
          alternative.AddUtilityTerm(10, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(11, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(12, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(13, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(14, aggregateLogsumWorkBased);

          // Neighborhood
          alternative.AddUtilityTerm(20, logOfOnePlusEmploymentEducationBuffer1);
          //alternative.AddUtilityTerm(21, logOfOnePlusEmploymentGovernmentBuffer1);
          alternative.AddUtilityTerm(22, logOfOnePlusEmploymentOfficeBuffer1);
          //alternative.AddUtilityTerm(23, logOfOnePlusEmploymentServiceBuffer1);
          alternative.AddUtilityTerm(24, logOfOnePlusEmploymentMedicalBuffer1);
          alternative.AddUtilityTerm(25, logOfOnePlusHouseholdsBuffer1);
          alternative.AddUtilityTerm(26, logOfOnePlusStudentsUniversityBuffer1);
          alternative.AddUtilityTerm(27, logOfOnePlusStudentsK12Buffer1);
          alternative.AddUtilityTerm(28, logOfOnePlusEmploymentIndustrial_Ag_ConstructionBuffer1);

          // Size terms
          alternative.AddUtilityTerm(30, destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(31, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(32, destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(33, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(34, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(35, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(36, destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(37, destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
          alternative.AddUtilityTerm(38, destinationParcel.Households);
          alternative.AddUtilityTerm(39, destinationParcel.GetStudentsK12());
          //(for application) alternative.AddUtilityTerm(40, destinationParcel.StudentsUniversity);
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
          alternative.AddUtilityTerm(50, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(51, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(52, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(53, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(54, aggregateLogsumHomeBased);

          // Neighborhood
          alternative.AddUtilityTerm(60, householdHasNoChildren.ToFlag() * logOfOnePlusEmploymentGovernmentBuffer1);
          alternative.AddUtilityTerm(61, householdHasNoChildren.ToFlag() * logOfOnePlusHouseholdsBuffer1);
          //alternative.AddUtilityTerm(62, householdHasChildren.ToFlag() * logOfOnePlusHouseholdsBuffer1);
          alternative.AddUtilityTerm(63, householdHasChildren.ToFlag() * logOfOnePlusStudentsK12Buffer1);
          alternative.AddUtilityTerm(64, logOfOnePlusEmploymentTotalBuffer1);

          // Size terms
          alternative.AddUtilityTerm(70, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(71, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(72, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(73, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(74, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(75, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(76, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(77, (!householdHasChildren).ToFlag() * (destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction));
          alternative.AddUtilityTerm(78, (!householdHasChildren).ToFlag() * destinationParcel.Households);
          alternative.AddUtilityTerm(79, (!householdHasChildren).ToFlag() * destinationParcel.GetStudentsK12());

          alternative.AddUtilityTerm(80, householdHasChildren.ToFlag() * destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(81, householdHasChildren.ToFlag() * destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(82, householdHasChildren.ToFlag() * destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(83, householdHasChildren.ToFlag() * destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(84, householdHasChildren.ToFlag() * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(85, householdHasChildren.ToFlag() * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(86, householdHasChildren.ToFlag() * destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(87, householdHasChildren.ToFlag() * (destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction));
          alternative.AddUtilityTerm(88, householdHasChildren.ToFlag() * destinationParcel.Households);
          alternative.AddUtilityTerm(89, householdHasChildren.ToFlag() * destinationParcel.GetStudentsK12());
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness) {
          alternative.AddUtilityTerm(90, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(91, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(92, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(93, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(94, aggregateLogsumHomeBased);

          // Neighborhood
          alternative.AddUtilityTerm(100, logOfOnePlusEmploymentEducationBuffer1);
          alternative.AddUtilityTerm(101, logOfOnePlusEmploymentOfficeBuffer1);
          //alternative.AddUtilityTerm(102, logOfOnePlusEmploymentServiceBuffer1);
          //alternative.AddUtilityTerm(103, logOfOnePlusEmploymentMedicalBuffer1);
          alternative.AddUtilityTerm(104, logOfOnePlusHouseholdsBuffer1);
          //alternative.AddUtilityTerm(105, logOfOnePlusStudentsUniversityBuffer1);
          alternative.AddUtilityTerm(106, logOfOnePlusEmploymentGovernmentBuffer1);
          alternative.AddUtilityTerm(107, logOfOnePlusEmploymentRetailBuffer1);

          // Size terms
          alternative.AddUtilityTerm(110, destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(111, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(112, destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(113, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(114, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(115, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(116, destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(117, destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
          alternative.AddUtilityTerm(118, destinationParcel.Households);
          //(for application) alternative.AddUtilityTerm(119, destinationParcel.GetStudentsK12());
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
          alternative.AddUtilityTerm(120, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(121, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(122, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(123, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(124, aggregateLogsumHomeBased);

          // Neighborhood
          alternative.AddUtilityTerm(130, logOfOnePlusEmploymentEducationBuffer1);
          alternative.AddUtilityTerm(131, logOfOnePlusEmploymentRetailBuffer1);

          // Size terms
          alternative.AddUtilityTerm(140, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(141, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(142, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(143, destinationParcel.EmploymentService);
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Meal) {
          alternative.AddUtilityTerm(150, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(151, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(152, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(153, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(154, aggregateLogsumHomeBased);

          // Neighborhood
          alternative.AddUtilityTerm(156, logOfOnePlusEmploymentFoodBuffer1);
          //alternative.AddUtilityTerm(157, logOfOnePlusEmploymentRetailBuffer1);
          alternative.AddUtilityTerm(158, logOfOnePlusEmploymentServiceBuffer1);

          // Size terms
          alternative.AddUtilityTerm(160, destinationParcel.EmploymentFood);
          //(for application) alternative.AddUtilityTerm(161, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(162, destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(163, destinationParcel.Households);
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Social) {
          alternative.AddUtilityTerm(170, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(171, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(172, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(173, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(174, aggregateLogsumHomeBased);

          // Neighborhood
          //alternative.AddUtilityTerm(180, logOfOnePlusEmploymentOfficeBuffer1);
          //alternative.AddUtilityTerm(181, logOfOnePlusEmploymentServiceBuffer1);
          //alternative.AddUtilityTerm(182, logOfOnePlusHouseholdsBuffer1);
          //alternative.AddUtilityTerm(183, logOfOnePlusStudentsK12Buffer1);
          //alternative.AddUtilityTerm(184, logOfOnePlusStudentsUniversityBuffer1);
          //alternative.AddUtilityTerm(185, logOfOnePlusEmploymentTotalBuffer1);

          // Size terms
          alternative.AddUtilityTerm(190, destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(191, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(192, destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(193, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(194, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(195, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(196, destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(197, destinationParcel.Households);
          alternative.AddUtilityTerm(198, destinationParcel.StudentsUniversity);
          //(for application) alternative.AddUtilityTerm(199, destinationParcel.GetStudentsK12());
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Recreation) {
          alternative.AddUtilityTerm(200, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(201, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(202, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(203, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(204, aggregateLogsumHomeBased);

          // Neighborhood
          alternative.AddUtilityTerm(210, logOfOnePlusEmploymentOfficeBuffer1);
          alternative.AddUtilityTerm(211, logOfOnePlusEmploymentServiceBuffer1);
          alternative.AddUtilityTerm(212, logOfOnePlusHouseholdsBuffer1);
          //alternative.AddUtilityTerm(213, logOfOnePlusStudentsK12Buffer1);
          alternative.AddUtilityTerm(214, logOfOnePlusStudentsUniversityBuffer1);
          //alternative.AddUtilityTerm(215, logOfOnePlusEmploymentTotalBuffer1);

          // Size terms
          alternative.AddUtilityTerm(220, destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(221, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(222, destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(223, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(224, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(225, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(226, destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(227, destinationParcel.Households);
          alternative.AddUtilityTerm(228, destinationParcel.StudentsUniversity);
          alternative.AddUtilityTerm(229, destinationParcel.GetStudentsK12());
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Medical) {
          alternative.AddUtilityTerm(230, piecewiseDistanceFrom0To1Mile);
          alternative.AddUtilityTerm(231, piecewiseDistanceFrom1To3AndAHalfMiles);
          alternative.AddUtilityTerm(232, piecewiseDistanceFrom3AndAHalfTo10Miles);
          alternative.AddUtilityTerm(233, piecewiseDistanceFrom10MilesToInfinity);
          //alternative.AddUtilityTerm(234, aggregateLogsumHomeBased);

          // Neighborhood
          //alternative.AddUtilityTerm(240, logOfOnePlusEmploymentEducationBuffer1);
          //alternative.AddUtilityTerm(241, logOfOnePlusEmploymentOfficeBuffer1);
          //alternative.AddUtilityTerm(242, logOfOnePlusEmploymentServiceBuffer1);
          //alternative.AddUtilityTerm(243, logOfOnePlusEmploymentMedicalBuffer1);
          alternative.AddUtilityTerm(244, logOfOnePlusHouseholdsBuffer1);
          //alternative.AddUtilityTerm(245, logOfOnePlusStudentsUniversityBuffer1);
          //alternative.AddUtilityTerm(246, logOfOnePlusEmploymentGovernmentBuffer1);
          alternative.AddUtilityTerm(247, logOfOnePlusEmploymentRetailBuffer1);

          // Size terms
          //(for application) alternative.AddUtilityTerm(250, destinationParcel.EmploymentEducation);
          //(for application) alternative.AddUtilityTerm(251, destinationParcel.EmploymentFood);
          //(for application) alternative.AddUtilityTerm(252, destinationParcel.EmploymentGovernment);
          //(for application) alternative.AddUtilityTerm(253, destinationParcel.EmploymentOffice);
          //(for application) alternative.AddUtilityTerm(254, destinationParcel.EmploymentRetail);
          //(for application) alternative.AddUtilityTerm(255, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(256, destinationParcel.EmploymentMedical);
          //(for application) alternative.AddUtilityTerm(257, destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
          //(for application) alternative.AddUtilityTerm(258, destinationParcel.Households);
          //(for application) alternative.AddUtilityTerm(259, destinationParcel.GetStudentsK12());
        }
      }

      public static bool ShouldRunInEstimationModeForModel(ITourWrapper tour) {
        // determine validity and need, then characteristics
        // detect and skip invalid trip records (error = true) and those that trips that don't require stop location choice (need = false)
        int excludeReason = 0;

        //                if (_maxZone == -1) {
        //                    // TODO: Verify / Optimize
        //                    _maxZone = ChoiceModelRunner.ZoneKeys.Max(z => z.Key);
        //                }
        //
        //                if (_maxParcel == -1) {
        //                    // TODO: Optimize
        //                    _maxParcel = ChoiceModelRunner.Parcels.Values.Max(parcel => parcel.Id);
        //                }

        if (Global.Configuration.IsInEstimationMode) {
          //                    if (tour.OriginParcelId > _maxParcel) {
          //                        excludeReason = 3;
          //                    }

          if (tour.OriginParcelId <= 0) {
            excludeReason = 4;
          }
          //                    else if (tour.DestinationAddressType > _maxParcel) {
          //                        excludeReason = 5;
          //                    }
          else if (tour.DestinationParcelId <= 0) {
            excludeReason = 6;
            tour.DestinationParcelId = tour.OriginParcelId;
            tour.DestinationParcel = tour.OriginParcel;
            tour.DestinationZoneKey = tour.OriginParcelId;
          }
          //                    else if (tour.OriginParcelId > _maxParcel) {
          //                        excludeReason = 7;
          //                    }
          //                    else if (tour.OriginParcelId <= 0) {
          //                        excludeReason = 8;
          //                    }
          else if (tour.OriginParcelId == tour.DestinationParcelId) {
            excludeReason = 9;
          } else if (tour.OriginParcel.ZoneId == -1) {
            // TODO: Verify this condition... it used to check that the zone was == null. 
            // I'm not sure what the appropriate condition should be though.

            excludeReason = 10;
          }

          if (excludeReason > 0) {
            Global.PrintFile.WriteEstimationRecordExclusionMessage(CHOICE_MODEL_NAME, "ShouldRunInEstimationModeForModel", tour.Household.Id, tour.Person.Sequence, 0, tour.Sequence, 0, 0, excludeReason);
          }
        }

        bool shouldRun = (excludeReason == 0);

        return shouldRun;
      }
    }
  }
}
