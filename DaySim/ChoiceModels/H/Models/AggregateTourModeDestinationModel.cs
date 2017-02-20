// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Sampling;
using DaySim.PathTypeModels;
using DaySim.Sampling;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Threading.Tasks;


namespace DaySim.ChoiceModels.H.Models {
    public class AggregateTourModeDestinationModel : ChoiceModel {
        private const string CHOICE_MODEL_NAME = "AggregateTourModeDestinationModel";
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 99;

        //        private const int TOTAL_SUBZONES = 2;
        //        private static int _zoneCount;
        //        private static Dictionary<int, IZone> _eligibleZones;
        //        private static ISubzone[][] _zoneSubzones;
        //private static int timesStarted;
        private static int timesStartedRunModel;

        public override void RunInitialize(ICoefficientsReader reader = null) {
            int sampleSize = Global.Configuration.OtherTourDestinationModelSampleSize;
            int modesUsed = 4;
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.AggregateTourModeDestinationModelCoefficients, sampleSize * modesUsed, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
            /*
                        var zoneReader =
                            Global
                                .Kernel
                                .GetInstance<IPersistenceFactory<IZone>>()
                                .Reader;

                        _eligibleZones = zoneReader.Where(z => z.DestinationEligible).ToDictionary(z => z.Id, z => z);
                        _zoneCount = zoneReader.Count;
                        _zoneSubzones = CalculateZoneSubzones();
            */
        }

        public void Run(ITourWrapper tour, IHouseholdDayWrapper householdDay, int sampleSize) {
            if (tour == null) {
                throw new ArgumentNullException("tour");
            }
            //timesStarted++;

            tour.PersonDay.ResetRandom(20 + tour.Sequence - 1);

            if (Global.Configuration.IsInEstimationMode) {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
                    return;
                }
                if (tour.DestinationParcel == null || tour.OriginParcel == null
                    || tour.Mode < Global.Settings.Modes.Walk || tour.Mode > Global.Settings.Modes.Transit) {
                    return;
                }
                // JLB 20140421 add the following to keep from estimatign twice for the same tour
                //if (tour.DestinationModeAndTimeHaveBeenSimulated) {
                //    return;
                //}
            }

            var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(tour.Id);

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
                RunModel(choiceProbabilityCalculator, tour, householdDay, sampleSize, tour.DestinationParcel);

                choiceProbabilityCalculator.WriteObservation();

            }
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITourWrapper tour, IHouseholdDayWrapper householdDay, int sampleSize, IParcelWrapper choice = null) {
            timesStartedRunModel++;
            //Console.WriteLine("Started {0}  Finished {1}", timesStarted,timesStartedRunModel);

            var household = tour.Household;
            var person = tour.Person;
            var personDay = tour.PersonDay;
            var householdTotals = household.HouseholdTotals;
            var originParcel = tour.OriginParcel;

            int childUnder16Flag = (person.Age < 16).ToFlag();
            int noCarsInHouseholdFlag = (household.VehiclesAvailable == 0).ToFlag();
            int fewerCarsThanDriversFlag = household.GetFlagForCarsLessThanDrivers(household.VehiclesAvailable);

            int lowIncomeFlag = (household.Income >= 0 && household.Income <= 40000).ToFlag();
            int highIncomeFlag = (household.Income >= 80000).ToFlag();
            int missingIncomeFlag = (household.Income < 0).ToFlag();

            int originTransitBand1 = (originParcel.GetDistanceToTransit() >= 0 && originParcel.GetDistanceToTransit() <= 0.25).ToFlag();
            int originTransitBand3 = (originParcel.GetDistanceToTransit() > 0.5).ToFlag();


            TimeWindow timeWindow = new TimeWindow();
            if (tour.JointTourSequence > 0) {
                foreach (IPersonDayWrapper pDay in householdDay.PersonDays) {
                    ITourWrapper tInJoint = (ITourWrapper)pDay.Tours.Find(t => t.JointTourSequence == tour.JointTourSequence);
                    if (!(tInJoint == null)) {
                        // set jointTour time window
                        timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
                    }
                }
            } else if (tour.ParentTour == null) {
                timeWindow.IncorporateAnotherTimeWindow(personDay.TimeWindow);
            }

            timeWindow.SetBusyMinutes(Global.Settings.Times.EndOfRelevantWindow, Global.Settings.Times.MinutesInADay + 1);

            var maxAvailableMinutes =
                 (tour.JointTourSequence > 0 || tour.ParentTour == null)
                 ? timeWindow.MaxAvailableMinutesAfter(Global.Settings.Times.FiveAM)
                      : tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime;


            var householdHasChildren = household.HasChildren;
            var householdHasNoChildren = householdHasChildren ? false : true;

            var fastestAvailableTimeOfDay =
                 tour.IsHomeBasedTour || tour.ParentTour == null
                      ? 1
                      : tour.ParentTour.DestinationArrivalTime + (tour.ParentTour.DestinationDepartureTime - tour.ParentTour.DestinationArrivalTime) / 2;

            var tourCategory = tour.GetTourCategory();
            var secondaryFlag = ChoiceModelUtility.GetSecondaryFlag(tourCategory);
            var workOrSchoolPatternFlag = personDay.GetIsWorkOrSchoolPattern().ToFlag();
            var otherPatternFlag = personDay.GetIsOtherPattern().ToFlag();
            int jointTourFlag = (tour.JointTourSequence > 0).ToFlag();

            ChoiceModelUtility.DrawRandomTourTimePeriods(tour, tourCategory);

            var segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetTourDestinationSegment(tour.DestinationPurpose, tour.IsHomeBasedTour ? Global.Settings.TourPriorities.HomeBasedTour : Global.Settings.TourPriorities.WorkBasedTour, Global.Settings.Modes.Sov, person.PersonType);

            var destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, tour.OriginParcel);
            var tourDestinationUtilities = new TourDestinationUtilities(tour, sampleSize, secondaryFlag, personDay.GetIsWorkOrSchoolPattern().ToFlag(), personDay.GetIsOtherPattern().ToFlag(), fastestAvailableTimeOfDay, maxAvailableMinutes);

            // get destination sample and perform code that used to be in SetUtilities below
            var sampleItems = destinationSampler.SampleAndReturnTourDestinations(tourDestinationUtilities);

            int observedMode = 0;
            if (tour.Mode == Global.Settings.Modes.Bike || tour.Mode == Global.Settings.Modes.Walk) { observedMode = 0; } else if (tour.Mode == Global.Settings.Modes.Sov) { observedMode = 1; } else if (tour.Mode == Global.Settings.Modes.Hov2 || tour.Mode == Global.Settings.Modes.Hov3) { observedMode = 2; } else if (tour.Mode == Global.Settings.Modes.Transit) { observedMode = 3; }

            int purpose = tour.DestinationPurpose;
            if (tour.ParentTour != null) { purpose = 0; }
            int prefix = 0;

            int index = 0;
            int destindex = 0;
            int indChosen = -1;
            foreach (var sampleItem in sampleItems) {
                bool parcelAvailable = sampleItem.Key.Available;
                bool isChosen = sampleItem.Key.IsChosen;
                double adjustmentFactor = sampleItem.Key.AdjustmentFactor;
                var destinationParcel = ChoiceModelFactory.Parcels[sampleItem.Key.ParcelId];

                destindex++;
                if (isChosen) indChosen = destindex;

                if (!parcelAvailable) {
                    continue;
                }


                int destinationTransitBand1 = (destinationParcel.GetDistanceToTransit() >= 0 && destinationParcel.GetDistanceToTransit() <= 0.25).ToFlag();
                int destinationTransitBand3 = (destinationParcel.GetDistanceToTransit() > 0.5).ToFlag();

                //                var destinationSubzone = _zoneSubzones[destinationParcel.ZoneId][destinationTransitBand3];

                IEnumerable<IPathTypeModel> pathTypeModels =
                    PathTypeModelFactory.Singleton.Run(
                    tour.Household.RandomUtility,
                        originParcel.ZoneId,
                        destinationParcel.ZoneId,
                        tour.DestinationArrivalTime,
                        tour.DestinationDepartureTime,
                        tour.DestinationPurpose,
                        tour.CostCoefficient,
                        tour.TimeCoefficient,
                        tour.Person.IsDrivingAge,
                        tour.Household.VehiclesAvailable,
                       (tour.Household.OwnsAutomatedVehicles > 0),
                        tour.Person.GetTransitFareDiscountFraction(),
                        false,
                        Global.Settings.Modes.Walk, Global.Settings.Modes.Sov, Global.Settings.Modes.Hov2, Global.Settings.Modes.Transit);

                for (var modeIndex = 0; modeIndex <= 3; modeIndex++) {
                    var pathTypeModel = modeIndex == 0 ? pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Walk)
                                       : modeIndex == 1 ? pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Sov)
                                        : modeIndex == 2 ? pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Hov2)
                                                          : pathTypeModels.First(x => x.Mode == Global.Settings.Modes.Transit);
                    var modeAvailable = pathTypeModel.Available;
                    var chosen = modeIndex == observedMode && destinationParcel == tour.DestinationParcel;

                    //if (chosen) Global.PrintFile.WriteLine("Sequence {0}: Chosen parcel {1} Mode {2} Available {3}",timesStartedRunModel,destinationParcel.Id,observedMode,modeAvailable); 
                    var alternative = choiceProbabilityCalculator.GetAlternative(index++, modeAvailable, chosen);
                    alternative.Choice = destinationParcel;

                    if (!modeAvailable) {
                        continue;
                    }

                    alternative.AddUtilityTerm(1, tour.Id);
                    alternative.AddUtilityTerm(9, purpose);

                    if (modeIndex == 0) {
                        // WALK
                        alternative.AddUtilityTerm(prefix + 3, pathTypeModel.GeneralizedTimeLogsum);
                        alternative.AddUtilityTerm(prefix + 4, destinationParcel.NetIntersectionDensity2());
                        alternative.AddUtilityTerm(prefix + 5, destinationParcel.MixedUse4Index2());
                    } else if (modeIndex == 1) {
                        // SOV
                        alternative.AddUtilityTerm(prefix + 3, pathTypeModel.GeneralizedTimeLogsum);
                        alternative.AddUtilityTerm(prefix + 11, 1.0);
                        alternative.AddUtilityTerm(prefix + 12, destinationParcel.ParkingOffStreetPaidHourlyPriceBuffer2);
                        alternative.AddUtilityTerm(prefix + 14, fewerCarsThanDriversFlag);
                        alternative.AddUtilityTerm(prefix + 15, lowIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 16, highIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 17, missingIncomeFlag);
                    } else if (modeIndex == 2) {
                        // HOV
                        alternative.AddUtilityTerm(prefix + 3, pathTypeModel.GeneralizedTimeLogsum);
                        alternative.AddUtilityTerm(prefix + 21, 1.0);
                        alternative.AddUtilityTerm(prefix + 22, childUnder16Flag);
                        alternative.AddUtilityTerm(prefix + 23, noCarsInHouseholdFlag);
                        alternative.AddUtilityTerm(prefix + 24, fewerCarsThanDriversFlag);
                        alternative.AddUtilityTerm(prefix + 25, lowIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 26, highIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 27, missingIncomeFlag);
                    } else {
                        // TRANSIT
                        alternative.AddUtilityTerm(prefix + 3, pathTypeModel.GeneralizedTimeLogsum);
                        alternative.AddUtilityTerm(prefix + 31, 1.0);
                        alternative.AddUtilityTerm(prefix + 32, childUnder16Flag);
                        alternative.AddUtilityTerm(prefix + 33, noCarsInHouseholdFlag);
                        alternative.AddUtilityTerm(prefix + 34, fewerCarsThanDriversFlag);
                        alternative.AddUtilityTerm(prefix + 35, lowIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 36, highIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 37, missingIncomeFlag);
                        alternative.AddUtilityTerm(prefix + 38, originTransitBand1);
                        alternative.AddUtilityTerm(prefix + 39, originTransitBand3);
                        //alternative.AddUtilityTerm(prefix + 40, destinationTransitBand3);
                    }



                    alternative.AddUtilityTerm(2, adjustmentFactor);

                    // Size terms
                    alternative.AddUtilityTerm(prefix + 50, destinationParcel.EmploymentService);
                    alternative.AddUtilityTerm(prefix + 51, destinationParcel.EmploymentEducation);
                    alternative.AddUtilityTerm(prefix + 52, destinationParcel.EmploymentGovernment);
                    //alternative.AddUtilityTerm(prefix + 53, destinationParcel.EmploymentIndustrial);
                    alternative.AddUtilityTerm(prefix + 54, destinationParcel.EmploymentOffice);
                    alternative.AddUtilityTerm(prefix + 55, destinationParcel.EmploymentRetail);
                    alternative.AddUtilityTerm(prefix + 56, destinationParcel.EmploymentMedical);
                    alternative.AddUtilityTerm(prefix + 57, destinationParcel.EmploymentFood);
                    //alternative.AddUtilityTerm(prefix + 58, destinationParcel.EmploymentAgricultureConstruction);
                    alternative.AddUtilityTerm(prefix + 59, destinationParcel.Households);
                    //alternative.AddUtilityTerm(prefix + 60, destinationParcel.StudentsK8 + destinationParcel.StudentsHighSchool);
                    //alternative.AddUtilityTerm(prefix + 61, destinationParcel.StudentsUniversity);
                }

            }
            //Global.PrintFile.WriteLine("Sequence {0}: Chosen parcel {1} is sample item {2} of {3}",timesStartedRunModel,tour.DestinationParcelId,indChosen,sampleItems.Count); 

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

            public int[] SeedValues {
                get { return _seedValues; }
            }

            public void SetUtilities(ISampleItem sampleItem, int sampleFrequency) {
                if (sampleItem == null) {
                    throw new ArgumentNullException("sampleItem");
                }

            }

        }
        /*
                private ISubzone[][] CalculateZoneSubzones() {
                    var subzoneFactory = new SubzoneFactory(Global.Configuration);
                    var zoneSubzones = new ISubzone[_zoneCount][];

                    for (var id = 0; id < _zoneCount; id++) {
                        var subzones = new ISubzone[TOTAL_SUBZONES];

                        zoneSubzones[id] = subzones;

                        for (var subzone = 0; subzone < TOTAL_SUBZONES; subzone++) {
                            subzones[subzone] = subzoneFactory.Create(subzone);
                        }
                    }

                    var parcelReader = 
                        Global
                            .Kernel
                            .GetInstance<IPersistenceFactory<IParcel>>()
                            .Reader;

                    var parcelCreator =
                        Global
                            .Kernel
                            .GetInstance<IWrapperFactory<IParcelCreator>>()
                            .Creator;

                    foreach (var parcel in parcelReader) {
                        var parcelWrapper = parcelCreator.CreateWrapper(parcel);

                        var subzones = zoneSubzones[parcelWrapper.ZoneId];
                        // var subzone = (parcel.GetDistanceToTransit() > 0 && parcel.GetDistanceToTransit() <= .5) ? 0 : 1;  
                        // JLBscale replaced above with following:
                        var subzone = (parcelWrapper.GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile > 0 && parcelWrapper.GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile <= .5) ? 0 : 1;

                        subzones[subzone].Households += parcelWrapper.Households;
                        subzones[subzone].StudentsK8 += parcelWrapper.StudentsK8;
                        subzones[subzone].StudentsHighSchool += parcelWrapper.StudentsHighSchool;
                        subzones[subzone].StudentsUniversity += parcelWrapper.StudentsUniversity;
                        subzones[subzone].EmploymentEducation += parcelWrapper.EmploymentEducation;
                        subzones[subzone].EmploymentFood += parcelWrapper.EmploymentFood;
                        subzones[subzone].EmploymentGovernment += parcelWrapper.EmploymentGovernment;
                        subzones[subzone].EmploymentIndustrial += parcelWrapper.EmploymentIndustrial;
                        subzones[subzone].EmploymentMedical += parcelWrapper.EmploymentMedical;
                        subzones[subzone].EmploymentOffice += parcelWrapper.EmploymentOffice;
                        subzones[subzone].EmploymentRetail += parcelWrapper.EmploymentRetail;
                        subzones[subzone].EmploymentService += parcelWrapper.EmploymentService;
                        subzones[subzone].EmploymentTotal += parcelWrapper.EmploymentTotal;
                        subzones[subzone].ParkingOffStreetPaidDailySpaces += parcelWrapper.ParkingOffStreetPaidDailySpaces;
                        subzones[subzone].ParkingOffStreetPaidHourlySpaces += parcelWrapper.ParkingOffStreetPaidHourlySpaces;
                    }

                    foreach (var subzones in _eligibleZones.Values.Select(zone => zoneSubzones[zone.Id])) {
                        for (var subzone = 0; subzone < TOTAL_SUBZONES; subzone++) {
                            var hou = subzones[subzone].Households;
                            var k12 = subzones[subzone].StudentsK8 + subzones[subzone].StudentsHighSchool;
                            var uni = subzones[subzone].StudentsUniversity;
                            var edu = subzones[subzone].EmploymentEducation;
                            var foo = subzones[subzone].EmploymentFood;
                            var gov = subzones[subzone].EmploymentGovernment;
                            var ind = subzones[subzone].EmploymentIndustrial;
                            var med = subzones[subzone].EmploymentMedical;
                            var off = subzones[subzone].EmploymentOffice;
                            var ret = subzones[subzone].EmploymentRetail;
                            var ser = subzones[subzone].EmploymentService;
                            var tot = subzones[subzone].EmploymentTotal;
                            const double oth = 0;

                            var subtotal = foo + ret + ser + med;

                            subzones[subzone].MixedUseMeasure = Math.Log(1 + subtotal * (subzones[subzone]).ParkingOffStreetPaidHourlySpaces * 100 / Math.Max(subtotal + (subzones[subzone]).ParkingOffStreetPaidHourlySpaces * 100, Constants.EPSILON));

                        }
                    }

                    return zoneSubzones;
        */
    }
}