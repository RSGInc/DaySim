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

namespace DaySim.ChoiceModels.Default.Models {
  public class OtherTourDestinationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "OtherTourDestinationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    // regular and size parameters must be <= MAX_REGULAR_PARAMETER, balance is for OD shadow pricing coefficients
    private const int MAX_REGULAR_PARAMETER = 190;
    private const int MaxDistrictNumber = 100;
    private const int MAX_PARAMETER = MAX_REGULAR_PARAMETER + MaxDistrictNumber * MaxDistrictNumber;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.OtherTourDestinationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.OtherTourDestinationModelCoefficients, sampleSize + 1, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    protected virtual void RegionSpecificOtherTourDistrictCoefficients(ChoiceProbabilityCalculator.Alternative alternative, ITourWrapper tour, IParcelWrapper destinationParcel) {
      //see PSRC_OtherTourDestinationModel for example
      //Global.PrintFile.WriteLine("Generic OtherTourDestinationModel.RegionSpecificOtherTourDistrictCoefficients being called so must not be overridden by CustomizationDll");
    }

    public void Run(ITourWrapper tour, int sampleSize) {
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
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        RunModel(choiceProbabilityCalculator, tour, sampleSize, tour.DestinationParcel);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, tour, sampleSize);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
          tour.PersonDay.IsValid = false;

          return;
        }

        ParcelWrapper choice = (ParcelWrapper)chosenAlternative.Choice;

        tour.DestinationParcelId = choice.Id;
        tour.DestinationParcel = choice;
        tour.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];
        tour.DestinationAddressType = choice.Id == tour.Person.UsualWorkParcelId ? Global.Settings.AddressTypes.UsualWorkplace : Global.Settings.AddressTypes.Other;

        if (choice.Id == tour.Person.UsualWorkParcelId) {
          tour.PersonDay.UsualWorkplaceTours++;
        }
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, int sampleSize, IParcelWrapper choice = null) {
      //            var household = tour.Household;
      IPersonWrapper person = tour.Person;
      IPersonDayWrapper personDay = tour.PersonDay;

      //            var totalAvailableMinutes =
      //                tour.ParentTour == null
      //                    ? personDay.TimeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay)
      //                    : tour.ParentTour.TimeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay);

      int maxAvailableMinutes =
                tour.ParentTour == null
                    ? personDay.TimeWindow.MaxAvailableMinutesAfter(121)
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

      ChoiceModelUtility.DrawRandomTourTimePeriods(tour, tourCategory);

      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(tour.DestinationPurpose, tour.IsHomeBasedTour ? Global.Settings.TourPriorities.HomeBasedTour : Global.Settings.TourPriorities.WorkBasedTour, Global.Settings.Modes.Sov, person.PersonType);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, tour.OriginParcel);
      TourDestinationUtilities tourDestinationUtilities = new TourDestinationUtilities(this, tour, sampleSize, secondaryFlag, personDay.GetIsWorkOrSchoolPattern().ToFlag(), personDay.GetIsOtherPattern().ToFlag(), fastestAvailableTimeOfDay, maxAvailableMinutes);

      destinationSampler.SampleTourDestinations(tourDestinationUtilities);
    }

    private class TourDestinationUtilities : ISamplingUtilities {
      private readonly OtherTourDestinationModel _parentClass;
      private readonly ITourWrapper _tour;
      private readonly int _secondaryFlag;
      private readonly int _workOrSchoolPatternFlag;
      private readonly int _otherPatternFlag;
      private readonly int _fastestAvailableTimeOfDay;
      private readonly int _maxAvailableMinutes;
      private readonly int[] _seedValues;

      public TourDestinationUtilities(OtherTourDestinationModel parentClass, ITourWrapper tour, int sampleSize, int secondaryFlag, int workOrSchoolPatternFlag, int otherPatternFlag, int fastestAvailableTimeOfDay, int maxAvailableMinutes) {
        _parentClass = parentClass;
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
        //                var personDay = _tour.PersonDay;
        bool householdHasChildren = household.HasChildren;

        IParcelWrapper originParcel = _tour.OriginParcel;
        IParcelWrapper destinationParcel = ChoiceModelFactory.Parcels[sampleItem.ParcelId];

        double fastestTravelTime =
                    ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _fastestAvailableTimeOfDay, _tour.OriginParcel, destinationParcel).Variable +
                    ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _fastestAvailableTimeOfDay, destinationParcel, _tour.OriginParcel).Variable;

        if (fastestTravelTime >= _maxAvailableMinutes) {
          alternative.Available = false;

          return;
        }

        alternative.Choice = destinationParcel;

        double tourLogsum;

        if (_tour.IsHomeBasedTour) {
          if (_tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
            ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<EscortTourModeModel>().RunNested(_tour, destinationParcel);
            tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
          } else {
            ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<OtherHomeBasedTourModeModel>().RunNested(_tour, destinationParcel);
            tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
          }
        } else {
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkBasedSubtourModeModel>().RunNested(_tour, destinationParcel);
          tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }

        //var purpose = _tour.TourPurposeSegment;
        int carOwnership = person.GetCarOwnershipSegment();
        //var votSegment = _tour.VotALSegment;
        //var transitAccess = destinationParcel.TransitAccessSegment();
        //var aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][purpose][carOwnership][votSegment][transitAccess];
        //var aggregateLogsumHomeBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][transitAccess];
        //var aggregateLogsumWorkBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.Work_BASED][carOwnership][votSegment][transitAccess];

        //distanceFromOrigin is in units of 10 miles, so 1=10 miles
        double distanceFromOrigin = _tour.OriginParcel.DistanceFromOrigin(destinationParcel, _tour.DestinationArrivalTime);
        double distanceFromOrigin0 = Math.Max(0, Math.Min(distanceFromOrigin - .5, 1 - .5));
        double distanceFromOrigin3 = Math.Max(0, distanceFromOrigin - 1); //distance over 10 miles
        double distanceFromOrigin4 = Math.Min(distanceFromOrigin, .10); //distance up to 1 mile
        double distanceFromOrigin5 = Math.Max(0, Math.Min(distanceFromOrigin - .1, .5 - .1)); //distance between 1 and 5 miles
        double distanceFromOrigin8 = Math.Max(0, Math.Min(distanceFromOrigin - .1, .35 - .1));
        double distanceFromOrigin9 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
        double distanceFromOriginLog = Math.Log(1 + distanceFromOrigin);
        double distanceFromWorkLog = person.UsualWorkParcel.DistanceFromWorkLog(destinationParcel, 1);
        double distanceFromSchoolLog = person.UsualSchoolParcel.DistanceFromSchoolLog(destinationParcel, 1);

        double timePressure = Math.Log(1 - fastestTravelTime / _maxAvailableMinutes);

        // log transforms of buffers for Neighborhood effects
        double empEduBuffer = Math.Log(destinationParcel.EmploymentEducationBuffer1 + 1.0);
        double empFooBuffer = Math.Log(destinationParcel.EmploymentFoodBuffer1 + 1.0);
        //                var EMPGOV_B = Math.Log(destinationParcel.EmploymentGovernmentBuffer1 + 1.0);
        double empOfcBuffer = Math.Log(destinationParcel.EmploymentOfficeBuffer1 + 1.0);
        double empRetBuffer = Math.Log(destinationParcel.EmploymentRetailBuffer1 + 1.0);
        double empSvcBuffer = Math.Log(destinationParcel.EmploymentServiceBuffer1 + 1.0);
        double empMedBuffer = Math.Log(destinationParcel.EmploymentMedicalBuffer1 + 1.0);
        //                var EMPIND_B = Math.Log(destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1 + 1.0);
        double empTotBuffer = Math.Log(destinationParcel.EmploymentTotalBuffer1 + 1.0);
        double housesBuffer = Math.Log(destinationParcel.HouseholdsBuffer1 + 1.0);
        double studK12Buffer = Math.Log(destinationParcel.StudentsK8Buffer1 + destinationParcel.StudentsHighSchoolBuffer1 + 1.0);
        double studUniBuffer = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1.0);
        //                var EMPHOU_B = Math.Log(destinationParcel.EmploymentTotalBuffer1 + destinationParcel.HouseholdsBuffer1 + 1.0);

         // connectivity attributes
        double c34Ratio = destinationParcel.C34RatioBuffer1();

        int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership); // exludes no cars
        int noCarCompetitionFlag = FlagUtility.GetNoCarCompetitionFlag(carOwnership);
        int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
        alternative.AddUtilityTerm(1, sampleItem.AdjustmentFactor);
        alternative.AddUtilityTerm(2, (_tour.IsHomeBasedTour).ToFlag() * timePressure);

        alternative.AddUtilityTerm(3, _secondaryFlag * _workOrSchoolPatternFlag * distanceFromOrigin0);
        alternative.AddUtilityTerm(4, _secondaryFlag * _otherPatternFlag * distanceFromOrigin5);
        alternative.AddUtilityTerm(5, _secondaryFlag * _otherPatternFlag * distanceFromOrigin0);
        alternative.AddUtilityTerm(6, _secondaryFlag * _otherPatternFlag * distanceFromOrigin3);

        alternative.AddUtilityTerm(7, (!_tour.IsHomeBasedTour).ToFlag() * distanceFromOriginLog);
        //new calibration constants work-based tours
        alternative.AddUtilityTerm(58, (!_tour.IsHomeBasedTour).ToFlag() * distanceFromOrigin4);
        alternative.AddUtilityTerm(59, (!_tour.IsHomeBasedTour).ToFlag() * distanceFromOrigin3);
        alternative.AddUtilityTerm(60, (!_tour.IsHomeBasedTour).ToFlag() * distanceFromOrigin5);

        alternative.AddUtilityTerm(8, household.Has0To15KIncome.ToFlag() * distanceFromOriginLog);
        alternative.AddUtilityTerm(9, household.HasMissingIncome.ToFlag() * distanceFromOriginLog);
        alternative.AddUtilityTerm(10, person.IsRetiredAdult.ToFlag() * distanceFromOriginLog);
        alternative.AddUtilityTerm(11, person.IsChildAge5Through15.ToFlag() * distanceFromOriginLog);
        alternative.AddUtilityTerm(12, person.IsChildUnder5.ToFlag() * distanceFromOriginLog);

        alternative.AddUtilityTerm(13, (_tour.IsHomeBasedTour).ToFlag() * distanceFromSchoolLog);
        alternative.AddUtilityTerm(14, distanceFromWorkLog);

        alternative.AddUtilityTerm(15, carCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
        alternative.AddUtilityTerm(16, noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
        alternative.AddUtilityTerm(17, carCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
        alternative.AddUtilityTerm(18, noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());

        alternative.AddUtilityTerm(19, noCarsFlag * c34Ratio);


        if (_tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
          alternative.AddUtilityTerm(20, tourLogsum);
          alternative.AddUtilityTerm(21, distanceFromOrigin4);
          alternative.AddUtilityTerm(22, distanceFromOrigin8);
          alternative.AddUtilityTerm(23, distanceFromOrigin9);

          // Neighborhood
          alternative.AddUtilityTerm(24, householdHasChildren.ToFlag() * studK12Buffer);
          alternative.AddUtilityTerm(25, empTotBuffer);

          // Size terms
          alternative.AddUtilityTerm(71, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(72, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(73, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(74, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(75, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(76, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(77, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(78, (!householdHasChildren).ToFlag() * destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
          alternative.AddUtilityTerm(79, (!householdHasChildren).ToFlag() * destinationParcel.Households);

          alternative.AddUtilityTerm(80, householdHasChildren.ToFlag() * destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(81, householdHasChildren.ToFlag() * destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(82, householdHasChildren.ToFlag() * destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(83, householdHasChildren.ToFlag() * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(84, householdHasChildren.ToFlag() * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(85, householdHasChildren.ToFlag() * destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(86, householdHasChildren.ToFlag() * destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
          alternative.AddUtilityTerm(87, householdHasChildren.ToFlag() * destinationParcel.Households);
          alternative.AddUtilityTerm(88, householdHasChildren.ToFlag() * destinationParcel.GetStudentsK12());
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness || _tour.DestinationPurpose == Global.Settings.Purposes.Medical) {
          alternative.AddUtilityTerm(26, tourLogsum);
          alternative.AddUtilityTerm(27, distanceFromOrigin4);
          alternative.AddUtilityTerm(28, distanceFromOrigin8);
          alternative.AddUtilityTerm(29, distanceFromOrigin9);
          alternative.AddUtilityTerm(30, distanceFromOrigin3);

          // Neighborhood
          alternative.AddUtilityTerm(31, empEduBuffer);
          alternative.AddUtilityTerm(32, empSvcBuffer);
          alternative.AddUtilityTerm(33, empMedBuffer);
          alternative.AddUtilityTerm(34, housesBuffer); // also psrc
          alternative.AddUtilityTerm(35, studUniBuffer);


          // Size terms
          alternative.AddUtilityTerm(89, destinationParcel.EmploymentEducation);
          alternative.AddUtilityTerm(90, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(91, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(92, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(93, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(94, destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(95, destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
          alternative.AddUtilityTerm(96, destinationParcel.Households);
          alternative.AddUtilityTerm(97, destinationParcel.GetStudentsK12());
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
          alternative.AddUtilityTerm(36, tourLogsum);
          alternative.AddUtilityTerm(37, distanceFromOrigin4);
          alternative.AddUtilityTerm(38, distanceFromOrigin8);
          alternative.AddUtilityTerm(39, distanceFromOrigin9);
          alternative.AddUtilityTerm(40, distanceFromOrigin3);

          // Neighborhood
          alternative.AddUtilityTerm(41, empEduBuffer); // also psrc
          alternative.AddUtilityTerm(42, empRetBuffer); // also psrc

          // Size terms
          alternative.AddUtilityTerm(98, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(99, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(100, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(101, destinationParcel.EmploymentService);
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Meal) {
          alternative.AddUtilityTerm(43, tourLogsum);
          alternative.AddUtilityTerm(44, distanceFromOrigin4);
          alternative.AddUtilityTerm(45, distanceFromOrigin8);
          alternative.AddUtilityTerm(46, distanceFromOrigin9);
          alternative.AddUtilityTerm(47, distanceFromOrigin3);

          // Neighborhood
          alternative.AddUtilityTerm(48, empFooBuffer); // psrc

          // Size terms
          alternative.AddUtilityTerm(102, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(103, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(104, destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(105, destinationParcel.Households);
        } else if (_tour.DestinationPurpose == Global.Settings.Purposes.Social || _tour.DestinationPurpose == Global.Settings.Purposes.Recreation) {
          alternative.AddUtilityTerm(49, tourLogsum);
          alternative.AddUtilityTerm(50, distanceFromOrigin4);
          alternative.AddUtilityTerm(51, distanceFromOrigin8);
          alternative.AddUtilityTerm(52, distanceFromOrigin9);
          alternative.AddUtilityTerm(53, distanceFromOrigin3);

          // Neighborhood
          alternative.AddUtilityTerm(54, empOfcBuffer); // also psrc
          alternative.AddUtilityTerm(55, empSvcBuffer); // also psrc
          alternative.AddUtilityTerm(56, housesBuffer); // also psrc
          alternative.AddUtilityTerm(57, studUniBuffer); // psrc

          // Size terms
          alternative.AddUtilityTerm(106, destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(107, destinationParcel.EmploymentGovernment);
          alternative.AddUtilityTerm(108, destinationParcel.EmploymentOffice);
          alternative.AddUtilityTerm(109, destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(110, destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(111, destinationParcel.Households);
          alternative.AddUtilityTerm(112, destinationParcel.StudentsUniversity);
          alternative.AddUtilityTerm(113, destinationParcel.GetStudentsK12());
          //new size variable for log(sq ft open space)
          if (destinationParcel.LandUseCode > 0) {
            alternative.AddUtilityTerm(120, (Global.Configuration.UseParcelLandUseCodeAsSquareFeetOpenSpace) ? Math.Log(destinationParcel.LandUseCode + 1.0) : 0.0);
          }

           
        }

        //add any region-specific new terms in region-specific class, using coefficient numbers 114-120, or other unused variable #
        _parentClass.RegionSpecificOtherTourDistrictCoefficients(alternative, _tour, destinationParcel);

        // OD shadow pricing
        if (Global.Configuration.ShouldUseODShadowPricing) {
          int ori = _tour.OriginParcel.District;
          int des = destinationParcel.District;
          //var first = res <= des? res : des;
          //var second = res <= des? des : res;
          double shadowPriceConfigurationParameter = ori == des ? Global.Configuration.OtherTourDestinationOOShadowPriceCoefficient : Global.Configuration.OtherTourDestinationODShadowPriceCoefficient;
          int odShadowPriceF12Value = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (ori - 1) + des;
          alternative.AddUtilityTerm(odShadowPriceF12Value, shadowPriceConfigurationParameter);
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
          }
          //                    else if (tour.OriginParcelId > _maxParcel) {
          //                        excludeReason = 7;
          //                    }
          //                    else if (tour.OriginParcelId <= 0) {
          //                        excludeReason = 8;
          //                    }
          else if (tour.OriginParcelId == tour.DestinationParcelId && Global.Configuration.DestinationScale == 0) {
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
