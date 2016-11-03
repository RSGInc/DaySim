// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.DomainModels.Default.Wrappers;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;
using DaySim.Framework.Sampling;
using DaySim.Sampling;
using System;

namespace DaySim.ChoiceModels.Default.Models {
    public class WorkTourDestinationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "WorkTourDestinationModel";
        private const int TOTAL_NESTED_ALTERNATIVES = 102;
        private const int TOTAL_LEVELS = 2;
        // regular and size parameters must be <= MAX_REGULAR_PARAMETER, balance is for OD shadow pricing coefficients
        private const int MAX_REGULAR_PARAMETER = 120;
        private const int MaxDistrictNumber = 100;
        private const int MAX_PARAMETER = MAX_REGULAR_PARAMETER + MaxDistrictNumber * MaxDistrictNumber;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            int sampleSize = Global.Configuration.WorkTourDestinationModelSampleSize;
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkTourDestinationModelCoefficients, sampleSize + 1, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
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

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                RunModel(choiceProbabilityCalculator, tour, sampleSize, tour.DestinationParcel);

                choiceProbabilityCalculator.WriteObservation();
            } else {
                RunModel(choiceProbabilityCalculator, tour, sampleSize);

                var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);

                if (chosenAlternative == null) {
                    Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", tour.PersonDay.Id);
                    tour.PersonDay.IsValid = false;

                    return;
                }

                var choice = (ParcelWrapper)chosenAlternative.Choice;

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
            var household = tour.Household;
            var person = tour.Person;
            var personDay = tour.PersonDay;

            //			var totalAvailableMinutes =
            //				tour.ParentTour == null
            //					? personDay.TimeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay)
            //					: tour.ParentTour.TimeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay);

            var maxAvailableMinutes =
                tour.ParentTour == null
                    ? personDay.TimeWindow.MaxAvailableMinutesAfter(121)
                    : tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime;

            //			var hoursAvailableInverse =
            //				tour.IsHomeBasedTour
            //					? (personDay.HomeBasedTours - personDay.SimulatedHomeBasedTours + 1) / (Math.Max(totalAvailableMinutes - 360, 30) / 60D)
            //					: 1 / (Math.Max(totalAvailableMinutes, 1) / 60D);

            var fastestAvailableTimeOfDay =
                tour.IsHomeBasedTour || tour.ParentTour == null
                    ? 1
                    : tour.ParentTour.DestinationArrivalTime + (tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime) / 2;

            var tourCategory = tour.GetTourCategory();
            var primaryFlag = ChoiceModelUtility.GetPrimaryFlag(tourCategory);
            var secondaryFlag = ChoiceModelUtility.GetSecondaryFlag(tourCategory);

            ChoiceModelUtility.DrawRandomTourTimePeriods(tour, tourCategory);

            var segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(tour.DestinationPurpose, tour.IsHomeBasedTour ? Global.Settings.TourPriorities.HomeBasedTour : Global.Settings.TourPriorities.WorkBasedTour, Global.Settings.Modes.Sov, person.PersonType);
            var excludedParcel = person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId || tour.DestinationPurpose != Global.Settings.Purposes.Work || tour.GetTourCategory() == Global.Settings.TourCategories.WorkBased ? null : person.UsualWorkParcel;
            var destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, tour.OriginParcel, excludedParcel, excludedParcel);
            var tourDestinationUtilities = new TourDestinationUtilities(tour, sampleSize, primaryFlag, secondaryFlag, fastestAvailableTimeOfDay, maxAvailableMinutes);

            destinationSampler.SampleTourDestinations(tourDestinationUtilities);
        }

        private sealed class TourDestinationUtilities : ISamplingUtilities {
            private readonly ITourWrapper _tour;
            private readonly int _sampleSize;
            private readonly int _primaryFlag;
            private readonly int _secondaryFlag;
            private readonly int _fastestAvailableTimeOfDay;
            private readonly int _maxAvailableMinutes;
            private readonly int[] _seedValues;

            public TourDestinationUtilities(ITourWrapper tour, int sampleSize, int primaryFlag, int secondaryFlag, int fastestAvailableTimeOfDay, int maxAvailableMinutes) {
                _tour = tour;
                _sampleSize = sampleSize;
                _primaryFlag = primaryFlag;
                _secondaryFlag = secondaryFlag;
                _fastestAvailableTimeOfDay = fastestAvailableTimeOfDay;
                _maxAvailableMinutes = maxAvailableMinutes;
                _seedValues = ChoiceModelUtility.GetRandomSampling(_sampleSize, tour.Person.SeedValues[20 + tour.Sequence - 1]);
            }

            public int[] SeedValues {
                get { return _seedValues; }
            }

            public void SetUtilities(ISampleItem sampleItem, int sampleFrequency) {
                if (sampleItem == null) {
                    throw new ArgumentNullException("sampleItem");
                }

                var alternative = sampleItem.Alternative;

                var household = _tour.Household;
                var person = _tour.Person;
                var personDay = _tour.PersonDay;
                //				var householdHasChildren = household.HasChildren;

                var destinationParcel = ChoiceModelFactory.Parcels[sampleItem.ParcelId];

                var excludedParcel = person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId || _tour.DestinationPurpose != Global.Settings.Purposes.Work || _tour.GetTourCategory() == Global.Settings.TourCategories.WorkBased ? null : person.UsualWorkParcel;
                var usualWorkParcel = (excludedParcel != null && excludedParcel.Id == destinationParcel.Id); // only 1 for oddball alternative on tours with oddball alternative
                var usualWorkParcelFlag = usualWorkParcel.ToFlag();

                // Comment out these nesting calls when estimating the conditional flat model
                // model is NL with oddball alternative
                if (usualWorkParcelFlag == 0) {
                    // this alternative is in the non-oddball nest
                    alternative.AddNestedAlternative(_sampleSize + 2, 0, 60); // associates alternative with non-oddball nest
                } else {
                    // this is the oddball alternative
                    alternative.AddNestedAlternative(_sampleSize + 3, 1, 60); // associates alternative with oddball nest
                }


                if (!alternative.Available) {
                    return;
                }


                // use this block of code to eliminate the oddball alternative for estimation of the conditional model
                //if (usualWorkParcelFlag == 1) {
                //	alternative.Available = false;
                //
                //	return;
                //}

                var fastestTravelTime =
                    ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _fastestAvailableTimeOfDay, _tour.OriginParcel, destinationParcel).Variable +
                    ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _fastestAvailableTimeOfDay, destinationParcel, _tour.OriginParcel).Variable;

                if (fastestTravelTime >= _maxAvailableMinutes) {
                    alternative.Available = false;

                    return;
                }

                alternative.Choice = destinationParcel;

                double tourLogsum;

                if (_tour.IsHomeBasedTour) {
                    var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(_tour, destinationParcel, household.VehiclesAvailable, person.GetTransitFareDiscountFraction());
                    tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
                } else {
                    var nestedAlternative = Global.ChoiceModelSession.Get<WorkBasedSubtourModeModel>().RunNested(_tour, destinationParcel);
                    tourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
                }

                //var purpose = _tour.TourPurposeSegment;
                //var nonWorkPurpose = _tour.DestinationPurpose != Global.Settings.Purposes.Work;
                var carOwnership = person.GetCarOwnershipSegment();
                //var votSegment = _tour.VotALSegment;
                //var transitAccess = destinationParcel.TransitAccessSegment();
                //var aggregateLogsum = Global.AggregateLogsums[destinationParcel.ZoneId][purpose][carOwnership][votSegment][transitAccess];
                //var aggregateLogsumHomeBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][transitAccess];
                //var aggregateLogsumWorkBased = Global.AggregateLogsums[destinationParcel.ZoneId][Global.Settings.Purposes.Work_BASED][carOwnership][votSegment][transitAccess];

                var distanceFromOrigin = _tour.OriginParcel.DistanceFromOrigin(destinationParcel, _tour.DestinationArrivalTime);
                //				var distanceFromOrigin0 = Math.Max(0, Math.Min(distanceFromOrigin - .5, 1 - .5));
                //				var distanceFromOrigin3 = Math.Max(0, distanceFromOrigin - 1);
                //				var distanceFromOrigin4 = Math.Min(distanceFromOrigin, .10);
                //				var distanceFromOrigin5 = Math.Max(0, Math.Min(distanceFromOrigin - .1, .5 - .1));
                //				var distanceFromOrigin8 = Math.Max(0, Math.Min(distanceFromOrigin - .1, .35 - .1));
                //				var distanceFromOrigin9 = Math.Max(0, Math.Min(distanceFromOrigin - .35, 1 - .35));
                var distanceFromOriginLog = Math.Log(1 + distanceFromOrigin);
                var distanceFromWorkLog = person.UsualWorkParcel.DistanceFromWorkLog(destinationParcel, 1);
                var distanceFromSchoolLog = person.UsualSchoolParcel.DistanceFromSchoolLog(destinationParcel, 1);
                //				var millionsSquareFeet = destinationZoneTotals.MillionsSquareFeet();

                var timePressure = Math.Log(1 - fastestTravelTime / _maxAvailableMinutes);

                // log transforms of buffers for Neighborhood effects
                var empEduBuffer = Math.Log(destinationParcel.EmploymentEducationBuffer1 + 1.0);
                //				var EMPFOO_B = Math.Log(destinationParcel.EmploymentFoodBuffer1 + 1.0);
                //				var EMPGOV_B = Math.Log(destinationParcel.EmploymentGovernmentBuffer1 + 1.0);
                var empOfcBuffer = Math.Log(destinationParcel.EmploymentOfficeBuffer1 + 1.0);
                //				var EMPRET_B = Math.Log(destinationParcel.EmploymentRetailBuffer1 + 1.0);
                //				var EMPSVC_B = Math.Log(destinationParcel.EmploymentServiceBuffer1 + 1.0);
                //				var EMPMED_B = Math.Log(destinationParcel.EmploymentMedicalBuffer1 + 1.0);
                var empIndBuffer = Math.Log(destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1 + 1.0);
                //				var EMPTOT_B = Math.Log(destinationParcel.EmploymentTotalBuffer1 + 1.0);
                var housesBuffer = Math.Log(destinationParcel.HouseholdsBuffer1 + 1.0);
                //				var STUDK12B = Math.Log(destinationParcel.StudentsK8Buffer1 + destinationParcel.StudentsHighSchoolBuffer1 + 1.0);
                //				var STUDUNIB = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1.0);
                //				var EMPHOU_B = Math.Log(destinationParcel.EmploymentTotalBuffer1 + destinationParcel.HouseholdsBuffer1 + 1.0);

                // parking attributes
                //				var parcelParkingDensity = destinationParcel.ParcelParkingPerTotalEmployment();
                //				var zoneParkingDensity = destinationParcel.ZoneParkingPerTotalEmploymentAndK12UniversityStudents(destinationZoneTotals, millionsSquareFeet);
                //				var ParkingPaidDailyLogBuffer1 = Math.Log(1 + destinationParcel.ParkingOffStreetPaidDailySpacesBuffer1);

                // connectivity attributes
                //				var c34Ratio = destinationParcel.C34RatioBuffer1();

                //				var carDeficitFlag = FlagUtility.GetCarDeficitFlag(carOwnership);  // includes no cars
                //				var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership); // exludes no cars
                var noCarCompetitionFlag = FlagUtility.GetNoCarCompetitionFlag(carOwnership);
                //				var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);

                // Usual location attributes
                alternative.AddUtilityTerm(1, usualWorkParcelFlag);
                alternative.AddUtilityTerm(2, person.IsPartTimeWorker.ToFlag() * usualWorkParcelFlag);
                alternative.AddUtilityTerm(3, person.IsStudentAge.ToFlag() * usualWorkParcelFlag);
                alternative.AddUtilityTerm(4, _primaryFlag * personDay.TwoOrMoreWorkToursExist().ToFlag() * usualWorkParcelFlag);
                alternative.AddUtilityTerm(5, personDay.WorkStopsExist().ToFlag() * usualWorkParcelFlag);
                alternative.AddUtilityTerm(6, _secondaryFlag * usualWorkParcelFlag);

                // non-usual location attributes
                alternative.AddUtilityTerm(11, (!usualWorkParcel).ToFlag() * sampleItem.AdjustmentFactor);
                alternative.AddUtilityTerm(12, _tour.IsHomeBasedTour.ToFlag() * (!usualWorkParcel).ToFlag() * timePressure);

                alternative.AddUtilityTerm(13, (!usualWorkParcel).ToFlag() * person.IsFulltimeWorker.ToFlag() * tourLogsum);
                alternative.AddUtilityTerm(14, (!usualWorkParcel).ToFlag() * person.IsPartTimeWorker.ToFlag() * tourLogsum);
                alternative.AddUtilityTerm(15, (!usualWorkParcel).ToFlag() * person.IsNotFullOrPartTimeWorker.ToFlag() * tourLogsum);
                alternative.AddUtilityTerm(16, (!usualWorkParcel).ToFlag() * person.IsRetiredAdult.ToFlag() * distanceFromOriginLog);

                alternative.AddUtilityTerm(17, (!usualWorkParcel).ToFlag() * distanceFromWorkLog);
                alternative.AddUtilityTerm(18, (!usualWorkParcel).ToFlag() * person.IsStudentAge.ToFlag() * distanceFromSchoolLog);

                alternative.AddUtilityTerm(19, (!usualWorkParcel).ToFlag() * noCarCompetitionFlag * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());

                // non-usual location Neighborhood attributes 
                alternative.AddUtilityTerm(31, (!usualWorkParcel).ToFlag() * empEduBuffer);
                alternative.AddUtilityTerm(32, (!usualWorkParcel).ToFlag() * empOfcBuffer);
                alternative.AddUtilityTerm(33, (!usualWorkParcel).ToFlag() * housesBuffer);
                alternative.AddUtilityTerm(34, (!usualWorkParcel).ToFlag() * empIndBuffer);

                // non-usual location Size terms (consider conditioning these by fulltime, parttime, notFTPT, an income (see original sacog spec)
                alternative.AddUtilityTerm(40, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentEducation);
                alternative.AddUtilityTerm(41, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentFood);
                alternative.AddUtilityTerm(42, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentGovernment);
                alternative.AddUtilityTerm(43, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentOffice);
                alternative.AddUtilityTerm(44, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentRetail);
                alternative.AddUtilityTerm(45, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentService);
                alternative.AddUtilityTerm(46, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentMedical);
                alternative.AddUtilityTerm(47, (!usualWorkParcel).ToFlag() * destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction);
                alternative.AddUtilityTerm(48, (!usualWorkParcel).ToFlag() * destinationParcel.Households);
                alternative.AddUtilityTerm(49, (!usualWorkParcel).ToFlag() * destinationParcel.StudentsUniversity);


                // OD shadow pricing
                if (!usualWorkParcel && Global.Configuration.ShouldUseODShadowPricing) {
                    var ori = _tour.OriginParcel.District;
                    var des = destinationParcel.District;
                    //var first = res <= des? res : des;
                    //var second = res <= des? des : res;
                    var shadowPriceConfigurationParameter = ori == des ? Global.Configuration.WorkTourDestinationOOShadowPriceCoefficient : Global.Configuration.WorkTourDestinationODShadowPriceCoefficient;
                    var odShadowPriceF12Value = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (ori - 1) + des;
                    alternative.AddUtilityTerm(odShadowPriceF12Value, shadowPriceConfigurationParameter);
                }






                //		// usual location size term
                //		alternative.AddUtilityTerm(50, usualWorkParcelFlag * 1);

                //		// Comment out these nesting calls when estimating the conditional flat model
                //		// model is NL with oddball alternative
                //		if (usualWorkParcelFlag == 0) {
                //			// this alternative is in the non-oddball nest
                //			alternative.AddNestedAlternative(_sampleSize + 2, 0, 60); // associates alternative with non-oddball nest
                //		}
                //		else {
                //			// this is the oddball alternative
                //			alternative.AddNestedAlternative(_sampleSize + 3, 1, 60); // associates alternative with oddball nest
                //		}
            }

            public static bool ShouldRunInEstimationModeForModel(ITourWrapper tour) {
                // determine validity and need, then characteristics
                // detect and skip invalid trip records (error = true) and those that trips that don't require stop location choice (need = false)
                var excludeReason = 0;

                //				if (_maxZone == -1) {
                //					// TODO: Verify / Optimize
                //					_maxZone = ChoiceModelRunner.ZoneKeys.Max(z => z.Key);
                //				}
                //				if (_maxParcel == -1) {
                //					// TODO: Optimize
                //					_maxParcel = ChoiceModelRunner.Parcels.Values.Max(parcel => parcel.Id);
                //				}

                if (Global.Configuration.IsInEstimationMode) {
                    //					if (tour.OriginParcelId > _maxParcel) {
                    //						excludeReason = 3;
                    //					}

                    if (tour.OriginParcelId <= 0) {
                        excludeReason = 4;
                    }
                    //					else if (tour.DestinationAddressType > _maxParcel) {
                    //						excludeReason = 5;
                    //					}
                    else if (tour.DestinationParcelId <= 0) {
                        excludeReason = 6;
                    }
                    //					else if (tour.OriginParcelId > _maxParcel) {
                    //						excludeReason = 7;
                    //					}
                    //					else if (tour.OriginParcelId <= 0) {
                    //						excludeReason = 8;
                    //					}
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

                var shouldRun = (excludeReason == 0);

                return shouldRun;
            }
        }
    }
}