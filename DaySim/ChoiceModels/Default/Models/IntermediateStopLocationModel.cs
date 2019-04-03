// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Roster;
using DaySim.Framework.Sampling;
using DaySim.PathTypeModels;
using DaySim.Sampling;

namespace DaySim.ChoiceModels.Default.Models {
  public class IntermediateStopLocationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "IntermediateStopLocationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    // regular and size parameters must be <= MAX_REGULAR_PARAMETER, balance is for OD shadow pricing coefficients
    private const int MAX_REGULAR_PARAMETER = 100;
    private const int MaxDistrictNumber = 100;
    private const int MAX_PARAMETER = MAX_REGULAR_PARAMETER + MaxDistrictNumber * MaxDistrictNumber;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.IntermediateStopLocationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.IntermediateStopLocationModelCoefficients, sampleSize, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(ITripWrapper trip, int sampleSize) {
      if (trip == null) {
        throw new ArgumentNullException("trip");
      }

      trip.PersonDay.ResetRandom(40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 30 + trip.Sequence - 1);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }

        if (!IntermediateStopLocationUtilities.ShouldRunInEstimationModeForModel(trip, trip.Tour, trip.Tour.Mode)) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(trip.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        RunModel(choiceProbabilityCalculator, trip, sampleSize, trip.DestinationParcel);
        if (trip.PersonDay.IsValid == false) {
          return;
        }

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, trip, sampleSize);
        if (trip.PersonDay.IsValid == false) {
          return;
        }

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", trip.PersonDay.Id);
          trip.PersonDay.IsValid = false;

          return;
        }

        ParcelWrapper choice = (ParcelWrapper)chosenAlternative.Choice;

        trip.DestinationParcelId = choice.Id;
        trip.DestinationParcel = choice;
        trip.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];
        trip.DestinationAddressType =
            choice.Id == trip.Person.UsualWorkParcelId
                ? Global.Settings.AddressTypes.UsualWorkplace
                : choice.Id == trip.Person.UsualSchoolParcelId ? Global.Settings.AddressTypes.UsualSchool : Global.Settings.AddressTypes.Other;
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, ITripWrapper trip, int sampleSize, IParcelWrapper choice = null) {
      IPersonDayWrapper personDay = trip.PersonDay;
      ITourWrapper tour = trip.Tour;

      int destinationDepartureTime =
                trip.IsHalfTourFromOrigin // first trip in half tour, use tour destination time
                    ? trip.Sequence == 1
                          ? tour.DestinationArrivalTime
                          : trip.GetPreviousTrip().ArrivalTime
                    : trip.Sequence == 1
                          ? tour.DestinationDepartureTime
                          : trip.GetPreviousTrip().ArrivalTime;

      // time window in minutes for yet unmodeled portion of halftour, only consider persons on this trip
      int availableWindow =
                tour.ParentTour == null
                    ? personDay.TimeWindow.AvailableWindow(destinationDepartureTime, Global.Settings.TimeDirections.Both)
                    : tour.ParentTour.TimeWindow.AvailableWindow(destinationDepartureTime, Global.Settings.TimeDirections.Both);

      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetIntermediateStopSegment(trip.DestinationPurpose, trip.Tour.Mode);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, choice, trip.Tour, trip);
      IntermediateStopLocationUtilities intermediateStopLocationUtilities = new IntermediateStopLocationUtilities(trip, sampleSize, destinationDepartureTime, availableWindow);

      //#if TRACE
      //            Global.Logger.AddToBuffer(string.Format("Segment: {0}", segment));
      //#endif

      int ret = destinationSampler.SampleIntermediateStopDestinations(intermediateStopLocationUtilities);
      if (ret == -1) {
        if (!Global.Configuration.IsInEstimationMode) {
          personDay.IsValid = false;
        }

        Global.PrintFile.WriteInfiniteLoopWarning(trip.PersonDay.Id, trip.Tour.Id, trip.Id, tour.OriginParcelId,
                                                                        tour.OriginParcel.ZoneId, tour.DestinationParcel.ZoneId, tour.Mode,
                                                                        tour.DestinationArrivalTime,
                                                                        Global.SegmentZones[segment][tour.OriginParcel.ZoneId].TotalWeight,
                                                                        Global.SegmentZones[segment][tour.OriginParcel.ZoneId].TotalSize);
      }
    }

    private sealed class IntermediateStopLocationUtilities : ISamplingUtilities {
      private static readonly int[] _averageStopDuration = new[] { 0, 7521, 10357, 801, 2517, 1956, 4453, 7382, 7382, 2517 };

      private readonly ITripWrapper _trip;
      //#if TRACE
      //            private readonly CondensedParcel _choice;
      //#endif
      private readonly int _destinationDepartureTime;
      private readonly int _availableWindow;
      private static int _maxZone = -1;
      private static int _maxParcel = -1;

      private const float WTHRESH = 0.25F; //if trip is less than this distance, then walk LOS is assumed  (mi)
      private const double DLIMPARM = 30.0; // soto distance limit (miles) at which gtim sensivity stops changing
      private const double DFACPARM = 0.7; // gtim senstivity at soto distance limit (relative to senstivity at distance of 0)

      private readonly int[] _seedValues;

      public IntermediateStopLocationUtilities(ITripWrapper trip, int sampleSize, int destinationDepartureTime, int availableWindow) {
        _trip = trip;
        //#if TRACE
        //                _choice = choice;
        //#endif
        _destinationDepartureTime = destinationDepartureTime;
        _availableWindow = availableWindow;
        _seedValues = ChoiceModelUtility.GetRandomSampling(sampleSize, trip.Tour.Person.SeedValues[40 * (2 * trip.Tour.Sequence - 1 + trip.Direction - 1) + 30 + trip.Sequence - 1]);
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

        IParcelWrapper destinationParcel = ChoiceModelFactory.Parcels[sampleItem.ParcelId];
        alternative.Choice = destinationParcel;

        IHouseholdWrapper household = _trip.Household;
        IPersonWrapper person = _trip.Person;
        ITourWrapper tour = _trip.Tour;

        //#if TRACE                
        //    // tour mode
        //                var transitModesFlag = tour.UsesTransitModes.ToFlag();
        //#endif

        // stop purpose
        int workDestinationPurposeFlag = _trip.IsWorkDestinationPurpose().ToFlag();
        int schgFlag = (_trip.IsSchoolDestinationPurpose() && (person.IsChildAge5Through15 || person.IsChildUnder5)).ToFlag();
        int schhFlag = (_trip.IsSchoolDestinationPurpose() && person.IsDrivingAgeStudent).ToFlag();
        int schuFlag = (_trip.IsSchoolDestinationPurpose() && (!person.IsChildAge5Through15 && !person.IsChildUnder5) && !person.IsDrivingAgeStudent).ToFlag();
        int workOrSchoolDestinationPurposeFlag = _trip.IsWorkOrSchoolDestinationPurpose().ToFlag();
        int eskdFlag = (_trip.IsEscortDestinationPurpose() && (household.HouseholdTotals.DrivingAgeStudents + household.HouseholdTotals.ChildrenAge5Through15 + household.HouseholdTotals.ChildrenUnder5 > 0)).ToFlag();
        int esnkFlag = (_trip.IsEscortDestinationPurpose() && (household.HouseholdTotals.DrivingAgeStudents + household.HouseholdTotals.ChildrenAge5Through15 + household.HouseholdTotals.ChildrenUnder5 == 0)).ToFlag();
        //#if TRACE
        //                var personalBusinessDestinationPurposeFlag = _trip.IsPersonalBusinessDestinationPurpose.ToFlag();
        //#endif
        int shoppingDestinationPurposeFlag = _trip.IsShoppingDestinationPurpose().ToFlag();
        int mealDestinationPurposeFlag = _trip.IsMealDestinationPurpose().ToFlag();
        int socialDestinationPurposeFlag = _trip.IsSocialDestinationPurpose().ToFlag();
        int recreationDestinationPurposeFlag = _trip.IsRecreationDestinationPurpose().ToFlag();
        int srec = (socialDestinationPurposeFlag == 1 || recreationDestinationPurposeFlag == 1).ToFlag();
        int personalReasonsDestinationPurposeFlag = _trip.IsPersonalReasonsDestinationPurpose().ToFlag();
        int personalDestinationPurposeFlag = (personalReasonsDestinationPurposeFlag == 1 || recreationDestinationPurposeFlag == 1).ToFlag();
        int personalOrMedicalDestinationPurposeFlag = _trip.IsPersonalBusinessOrMedicalDestinationPurpose().ToFlag();

        // tour and trip characteristics
        int shshFlag = (_trip.IsShoppingDestinationPurpose() && tour.IsShoppingPurpose()).ToFlag(); // shoppingDestinationPurposeFlag trip on shoppingDestinationPurposeFlag tour
        int bmanFlag = (_trip.IsHalfTourFromOrigin && (tour.IsWorkPurpose() || tour.IsSchoolPurpose())).ToFlag();
        double parkingTime = _averageStopDuration[_trip.DestinationPurpose] / 6000D; // parking time (hrs)
        int notFirstFlag = (_trip.Sequence != 1).ToFlag();

        double empo = destinationParcel.EmploymentGovernment + destinationParcel.EmploymentOffice + destinationParcel.EmploymentEducation;
        double empi = destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction;
        double enrs = destinationParcel.StudentsK8 + destinationParcel.StudentsHighSchool;

        double empsq = Math.Log(destinationParcel.EmploymentServiceBuffer1 + 1);
        double empmq = Math.Log(destinationParcel.EmploymentMedicalBuffer1 + 1);
        double emprq = Math.Log(destinationParcel.EmploymentRetailBuffer1 + 1);
        double empfq = Math.Log(destinationParcel.EmploymentFoodBuffer1 + 1);
        //                var empoq = Math.Log((destinationParcel.EmploymentGovernmentBuffer1 + destinationParcel.EmploymentOfficeBuffer1 + destinationParcel.EmploymentEducationBuffer1 +1);
        double empiq = Math.Log(destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1 + 1);
        double emptq = Math.Log(destinationParcel.EmploymentTotalBuffer1 + 1);
        double hhldxq = Math.Log(destinationParcel.HouseholdsBuffer1 + 1);
        double enrsq = Math.Log(destinationParcel.StudentsK8Buffer1 + destinationParcel.StudentsHighSchoolBuffer1 + 1);
        double enruq = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1);
        //                var parkq = Math.Log(destinationParcel.ParkingOffStreetPaidHourlySpacesBuffer1 + 1);
        int openqFlag = (destinationParcel.OpenSpaceType2Buffer1 > 0).ToFlag();
        double n134Q = destinationParcel.NodesSingleLinkBuffer1 + destinationParcel.NodesThreeLinksBuffer1 + destinationParcel.NodesFourLinksBuffer1;

        double wdis0;
        double wtime0;
        double gwtime0;
        double wdis1;
        double wtime1;
        double gwtime1;
        double wdis2;
        double wtime2;
        double gwtime2;
        double adis0;
        double adis1;
        double adis2;

        double costCoef = tour.CostCoefficient;
        double timeCoef = tour.TimeCoefficient;
        double transitDiscountFraction = tour.Person.GetTransitFareDiscountFraction();
        int minute = DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Start;
        int purpose = tour.DestinationPurpose;

        if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, _trip.OriginParcel, _trip.OriginParcel, out wdis0, out wtime0, out gwtime0);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wdis1, out wtime1, out gwtime1);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wdis2, out wtime2, out gwtime2);
          adis0 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, tour.OriginParcel, _trip.OriginParcel).Variable;
          adis1 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, tour.OriginParcel, destinationParcel).Variable;
          adis2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, destinationParcel, _trip.OriginParcel).Variable;
        } else {
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, tour.OriginParcel, tour.OriginParcel, out wdis0, out wtime0, out gwtime0);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wdis1, out wtime1, out gwtime1);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wdis2, out wtime2, out gwtime2);
          adis0 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, _trip.OriginParcel, tour.OriginParcel).Variable;
          adis1 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, _trip.OriginParcel, destinationParcel).Variable;
          adis2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, destinationParcel, tour.OriginParcel).Variable;
        }

        double ttim1 = 0;
        double ttim2 = 0;
        double gtim1 = 0;
        double gtim2 = 0;
        double d1Dis1;
        double d1Time1;
        double gd1Time1;
        double d1Dis2;
        double d1Time2;
        double gd1Time2;
        double wttime1 = 0;
        double wttime2 = 0;

        if (tour.Mode == Global.Settings.Modes.Walk) {
          ttim1 = wtime1;
          ttim2 = wtime2;
          gtim1 = gwtime1;
          gtim2 = gwtime2;
        } else if (tour.Mode == Global.Settings.Modes.Bike) {
          double bdis1;
          double btime1;
          double gbtime1;
          double bdis2;
          double btime2;
          double gbtime2;

          if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out bdis1, out btime1, out gbtime1);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out bdis2, out btime2, out gbtime2);
          } else {
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out bdis1, out btime1, out gbtime1);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out bdis2, out btime2, out gbtime2);
          }

          ttim1 = btime1;
          ttim2 = btime2;
          gtim1 = gbtime1;
          gtim2 = gbtime2;
        } else if (tour.Mode >= Global.Settings.Modes.SchoolBus) {
          if (wdis1 < WTHRESH && wdis2 < WTHRESH && wdis1 > Constants.EPSILON && wdis2 > Constants.EPSILON) {
            ttim1 = wtime1;
            ttim2 = wtime2;
            gtim1 = gwtime1;
            gtim2 = gwtime2;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2);
            }

            ttim1 = d1Time1;
            ttim2 = d1Time2;
            gtim1 = gd1Time1;
            gtim2 = gd1Time2;
          }
        } else if (tour.Mode == Global.Settings.Modes.Sov || tour.Mode == Global.Settings.Modes.ParkAndRide) {
          if (wdis1 < WTHRESH && wdis2 < WTHRESH && wdis1 > Constants.EPSILON && wdis2 > Constants.EPSILON) {
            ttim1 = wtime1;
            ttim2 = wtime2;
            gtim1 = gwtime1;
            gtim2 = gwtime2;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime, parkingTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime, parkingTime);
            }

            ttim1 = d1Time1;
            ttim2 = d1Time2;
            gtim1 = gd1Time1;
            gtim2 = gd1Time2;
          }
        } else if (tour.Mode == Global.Settings.Modes.Hov2) {
          if (wdis1 < WTHRESH && wdis2 < WTHRESH && wdis1 > Constants.EPSILON && wdis2 > Constants.EPSILON) {
            ttim1 = wtime1;
            ttim2 = wtime2;
            gtim1 = gwtime1;
            gtim2 = gwtime2;
          } else {
            double d2Dis1;
            double gd2Time2;
            double d2Time2;
            double d2Dis2;
            double gd2Time1;
            double d2Time1;

            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov2, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d2Dis1, out d2Time1, out gd2Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov2, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d2Dis2, out d2Time2, out gd2Time2, _destinationDepartureTime, parkingTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov2, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d2Dis1, out d2Time1, out gd2Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov2, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d2Dis2, out d2Time2, out gd2Time2, _destinationDepartureTime, parkingTime);
            }

            ttim1 = d2Time1;
            ttim2 = d2Time2;
            gtim1 = gd2Time1;
            gtim2 = gd2Time2;
          }
        } else if (tour.Mode == Global.Settings.Modes.Hov3) {
          if (wdis1 < WTHRESH && wdis2 < WTHRESH && wdis1 > Constants.EPSILON && wdis2 > Constants.EPSILON) {
            ttim1 = wtime1;
            ttim2 = wtime2;
            gtim1 = gwtime1;
            gtim2 = gwtime2;
          } else {
            double d2Dis1;
            double gd2Time2;
            double d2Time2;
            double d2Dis2;
            double gd2Time1;
            double d2Time1;

            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov3, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d2Dis1, out d2Time1, out gd2Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov3, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d2Dis2, out d2Time2, out gd2Time2, _destinationDepartureTime, parkingTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov3, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d2Dis1, out d2Time1, out gd2Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Hov3, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d2Dis2, out d2Time2, out gd2Time2, _destinationDepartureTime, parkingTime);
            }

            ttim1 = d2Time1;
            ttim2 = d2Time2;
            gtim1 = gd2Time1;
            gtim2 = gd2Time2;
          }
        } else if (tour.Mode == Global.Settings.Modes.Transit) {
          double gwttime2;
          double wtdis2;
          double wtdis1;
          double gwttime1;

          if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wtdis1, out wttime1, out gwttime1, _destinationDepartureTime);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wtdis2, out wttime2, out gwttime2, _destinationDepartureTime);
          } else {
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wtdis1, out wttime1, out gwttime1, _destinationDepartureTime);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wtdis2, out wttime2, out gwttime2, _destinationDepartureTime);
          }
          if (wdis1 < WTHRESH && wdis1 > Constants.EPSILON) {
            ttim1 = wtime1;
            gtim1 = gwtime1;
          } else if (wttime1 > Constants.EPSILON) {
            ttim1 = wttime1;
            gtim1 = gwttime1;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime);
            }

            ttim1 = d1Time1;
            gtim1 = gd1Time1;
          }

          if (wdis2 < WTHRESH && wdis2 > Constants.EPSILON) {
            ttim2 = wtime2;
            gtim2 = gwtime2;
          } else if (wttime2 > Constants.EPSILON) {
            ttim2 = wttime2;
            gtim2 = gwttime2;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime);
            }

            ttim2 = d1Time2;
            gtim2 = gd1Time2;
          }
        }

        // calculate variables derived from ttim1, ttim2, gtim1 and gtim2
        double dist = Math.Max(adis1 + adis2 - adis0, Math.Min(adis1 * 0.1, adis2 * 0.1)); // incremental distance (with imposed lower limit), in units of miles
        double gtim = (gtim1 + gtim2) / 100.0; // approximate incremental gtime, rescale to units of (100 minutes)  //rescale to units of (100 minutes

        gtim = gtim * dist / Math.Max(0.1, (adis1 + adis2)); // convert to approximate incremental gtime  
        gtim = gtim * (1.0 + (DFACPARM - 1.0) * Math.Min(adis0 / DLIMPARM, 1.0)); // transform to make gtim sensitivity a function of distsoto 

        if (gtim > 1.8) {
          // cap added because of huge utilities - multipled by very large coefficient- MAB 0625
          gtim = 1.8;
        }

        double gtis = gtim * gtim;
        double gtic = gtis * gtim;
        double ttim = Math.Min((ttim1 + ttim2) / 100.0, 3.6); // rescale to units of (100 minutes)
        double twin = ttim / (Math.Max(1.0, _availableWindow) / 100D); // unit-free ratio of ttim and window, both measured in 100 minute units

        if (twin >= 1) {
          alternative.Available = false;

          return;
        }

        twin = Math.Log(1 - twin);

        //                ttim = ttim * dist / Math.Max(0.1, (adis1 + adis2)); // ocnvert to approximate incremental ttime
        dist = dist / 100.0; // convert to units of 100 minutes

        double diss = Math.Min(0.25, dist * dist);
        double disc = Math.Min(0.125, diss * dist);
        //                var prox = 1.0 / (Math.Max(1.0, Math.Min(ttim1, ttim2)) / 10.0);

        double prxs;
        double prxo;

        if (_trip.IsHalfTourFromOrigin) {
          prxs = 1 / (Math.Max(1, ttim2) / 10);
          prxo = 1 / (Math.Max(1, ttim1) / 10);
        } else {
          prxs = 1 / (Math.Max(1, ttim1) / 10);
          prxo = 1 / (Math.Max(1, ttim2) / 10);
        }

        // calculate walk and transit unavailability indicators (one leg and two leg)
        int wtu1 = 0;
        int wtu2 = 0;
        int wta = 0;

        if (wdis1 < WTHRESH && wdis1 > Constants.EPSILON) {
          wta = wta + 1;
        } else if (tour.Mode == Global.Settings.Modes.Transit && wttime1 > Constants.EPSILON) {
          wta = wta + 1;
        }

        if (wdis2 < WTHRESH && wdis2 > Constants.EPSILON) {
          wta = wta + 1;
        } else if (tour.Mode == Global.Settings.Modes.Transit && wttime2 > Constants.EPSILON) {
          wta = wta + 1;
        }

        if (wta == 1) {
          wtu1 = 1;
        }

        if (wta == 0) {
          wtu2 = 1;
        }

        //                 if (tour.Mode == Global.Settings.Modes.Transit && wtu1 > 0) {
        //                     int check3 = 1;
        //                 }
        //
        //                int check = 0;
        //                int check1 = 0;

        // JLB 20110620 handle cases where LOS calcs failed to yield valid times
        if (ttim1 < Constants.EPSILON || ttim2 < Constants.EPSILON) {
          alternative.Available = false; // taltaval[i]:=0;
                                         //                    check = 1;
        }
        //                if (tour.Mode == 2){
        //                check1 = 0;
        //                }
        //                if (tour.Mode == 3){
        //                check1 = 0;
        //                }
        //                if (tour.Mode == 5){
        //                check1 = 0;
        //                }
        //                if (tour.Mode == 6){
        //                check1 = 0;
        //                }
        //                if (tour.Mode == 7){
        //                check1 = 0;
        //                }
        //                if (tour.Mode == 8){
        //                check1 = 0;
        //                }

        int fkid = (person.Gender == Global.Settings.PersonGenders.Female && person.PersonType < Global.Settings.PersonTypes.DrivingAgeStudent && (household.HouseholdTotals.ChildrenAge5Through15 + household.HouseholdTotals.ChildrenUnder5 + household.HouseholdTotals.DrivingAgeStudents > 0)).ToFlag();
        //#if TRACE
        //                var wrkt = tour.IsWorkPurpose.ToFlag();
        //                var n34Qln = Math.Log(1 + destinationParcel.NodesThreeLinksBuffer1 / 2.0 + destinationParcel.NodesFourLinksBuffer1);
        //                var n1Sq = destinationParcel.NodesSingleLinkBuffer1 / (Math.Max(1, destinationParcel.NodesSingleLinkBuffer1 + destinationParcel.NodesThreeLinksBuffer1 + destinationParcel.NodesFourLinksBuffer1));
        //                var n34H = destinationParcel.NodesThreeLinksBuffer2 / 2.0 + destinationParcel.NodesFourLinksBuffer2;
        //                var n34Hln = Math.Log(1 + destinationParcel.NodesThreeLinksBuffer2 / 2.0 + destinationParcel.NodesFourLinksBuffer2);
        //                var wbas = (!tour.IsHomeBasedTour).ToFlag();
        //                var bwork = wrkt == 1 && _trip.Direction == 1 ? 1 : 0;
        //                var nwrkt = 1 - wrkt;
        //                var nwst = wrkt != 1 && tour.IsSchoolPurpose.ToFlag() != 1 ? 1 : 0; // not _t.WorkDestinationPurposeFlag or school tour
        //                var essc = _trip.DestinationPurpose == Global.Settings.Purposes.Escort && _trip.Tour.DestinationPurpose == Global.Settings.Purposes.School ? 1 : 0;
        //                var shsh = shoppingDestinationPurposeFlag == 1 && _trip.Tour.DestinationPurpose == Global.Settings.Purposes.Shopping ? 1 : 0; // shoppingDestinationPurposeFlag trip on shoppingDestinationPurposeFlag tour
        //                var bman = _trip.Direction == 1 && (_trip.Tour.DestinationPurpose == Global.Settings.Purposes.Work || _trip.Tour.DestinationPurpose == Global.Settings.Purposes.School) ? 1 : 0;
        //
        //                //              The following output was used for comparing Delphi and CS results at the alternative level (or for the chosen alternative)
        //                if (Global.Configuration.IsInEstimationMode && _choice.Id == destinationParcel.Id && alternative.Available) {
        //                    Global.Logger.SetCondition(true);
        //                    Global.Logger.WriteLine(string.Format("Household Id: {0}", household.Id));
        //                    Global.Logger.WriteLine(string.Format("Day: {0}", _trip.Day));
        //                    Global.Logger.WriteLine(string.Format("Person: {0}", person.Sequence));
        //                    Global.Logger.WriteLine(string.Format("Tour: {0}", tour.Sequence));
        //                    Global.Logger.WriteLine(string.Format("Half: {0}", _trip.Direction));
        //                    Global.Logger.WriteLine(string.Format("Trip: {0}", _trip.Sequence));
        //                    Global.Logger.WriteLine(string.Format("Work: {0}", _trip.IsWorkDestinationPurpose.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("schgFlag: {0}", schgFlag));
        //                    Global.Logger.WriteLine(string.Format("schhFlag: {0}", schhFlag));
        //                    Global.Logger.WriteLine(string.Format("schuFlag: {0}", schuFlag));
        //                    Global.Logger.WriteLine(string.Format("worksch: {0}", workOrSchoolDestinationPurposeFlag));
        //                    Global.Logger.WriteLine(string.Format("esco: {0}", escoFlag));
        //                    Global.Logger.WriteLine(string.Format("eskdFlag: {0}", eskdFlag));
        //                    Global.Logger.WriteLine(string.Format("esnkFlag: {0}", esnkFlag));
        //                    Global.Logger.WriteLine(string.Format("pbus: {0}", personalBusinessDestinationPurposeFlag));
        //                    Global.Logger.WriteLine(string.Format("ship: {0}", shoppingDestinationPurposeFlag));
        //                    Global.Logger.WriteLine(string.Format("meal: {0}", mealDestinationPurposeFlag));
        //                    Global.Logger.WriteLine(string.Format("srec: {0}", srec));
        //                    Global.Logger.WriteLine(string.Format("medi: {0}", medicalDestinationPurposeFlag));
        //                    Global.Logger.WriteLine(string.Format("personal: {0}", personalDestinationPurposeFlag));
        //                    // {tour mode}
        //                    Global.Logger.WriteLine(string.Format("walk: {0}", tour.IsWalkMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("bike: {0}", tour.IsBikeMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("aut1: {0}", tour.IsSovMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("aut2: {0}", tour.IsHov2Mode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("au2p: {0}", tour.UsesHovModes.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("au3p: {0}", tour.IsHov3Mode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("trnw: {0}", tour.IsTransitMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("trna: {0}", tour.IsParkAndRideMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("sbus: {0}", tour.IsSchoolBusMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("nmot: {0}", tour.IsWalkOrBikeMode.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("auto: {0}", tour.UsesSovOrHovModes.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("naut: {0}", (!tour.UsesSovOrHovModes).ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("tran: {0}", transitModesFlag));
        //                    Global.Logger.WriteLine(string.Format("wbas: {0}", wbas));
        //                    Global.Logger.WriteLine(string.Format("wrkt: {0}", wrkt));
        //                    Global.Logger.WriteLine(string.Format("bwork: {0}", bwork));
        //                    Global.Logger.WriteLine(string.Format("nwrkt: {0}", nwrkt));
        //                    Global.Logger.WriteLine(string.Format("scht: {0}", tour.IsSchoolPurpose.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("nwst: {0}", nwst));
        //                    Global.Logger.WriteLine(string.Format("essc: {0}", essc));
        //                    Global.Logger.WriteLine(string.Format("shsh: {0}", shsh));
        //                    Global.Logger.WriteLine(string.Format("bman: {0}", bman));
        //                    Global.Logger.WriteLine(string.Format("notFirst: {0}", notFirstFlag));
        //                    Global.Logger.WriteLine(string.Format("IsFulltimeWorker: {0}", person.IsFulltimeWorker));
        //                    Global.Logger.WriteLine(string.Format("IsPartTimeWorker: {0}", person.IsPartTimeWorker));
        //                    Global.Logger.WriteLine(string.Format("IsRetiredAdult: {0}", person.IsRetiredAdult));
        //                    Global.Logger.WriteLine(string.Format("IsNonworkingAdult: {0}", person.IsNonworkingAdult));
        //                    Global.Logger.WriteLine(string.Format("IsUniversityStudent: {0}", person.IsUniversityStudent));
        //                    Global.Logger.WriteLine(string.Format("IsDrivingAgeStudent: {0}", person.IsDrivingAgeStudent));
        //                    Global.Logger.WriteLine(string.Format("IsChildAge5Through15: {0}", person.IsChildAge5Through15));
        //                    Global.Logger.WriteLine(string.Format("IsChildUnder5: {0}", person.IsChildUnder5));
        //                    Global.Logger.WriteLine(string.Format("youth: {0}", person.IsYouth.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("fkid: {0}", fkid));
        //                    Global.Logger.WriteLine(string.Format("msincome: {0}", household.HasMissingIncome.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("IncLo: {0}", household.HasLowIncome.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("IncHi: {0}", household.HasHighIncome.ToFlag()));
        //                    Global.Logger.WriteLine(string.Format("window: {0}", _availableWindow));
        //                    Global.Logger.WriteBuffer(); // Should write the buffered segment.
        //                    Global.Logger.WriteLine(string.Format("emps: {0,12:0.0000000000}", emps));
        //                    Global.Logger.WriteLine(string.Format("empm: {0,12:0.0000000000}", empm));
        //                    Global.Logger.WriteLine(string.Format("empr: {0,12:0.0000000000}", empr));
        //                    Global.Logger.WriteLine(string.Format("empf: {0,12:0.0000000000}", empf));
        //                    Global.Logger.WriteLine(string.Format("empo: {0,12:0.0000000000}", empo));
        //                    Global.Logger.WriteLine(string.Format("empi: {0,12:0.0000000000}", empi));
        //                    Global.Logger.WriteLine(string.Format("empt: {0,12:0.0000000000}", empt));
        //                    Global.Logger.WriteLine(string.Format("hhldx: {0,12:0.0000000000}", hhldx));
        //                    Global.Logger.WriteLine(string.Format("enrs: {0,12:0.0000000000}", enrs));
        //                    Global.Logger.WriteLine(string.Format("enru: {0,12:0.0000000000}", enru));
        //                    Global.Logger.WriteLine(string.Format("park: {0,12:0.0000000000}", park));
        //                    Global.Logger.WriteLine(string.Format("pxep: {0,12:0.0000000000}", pxep));
        //                    Global.Logger.WriteLine(string.Format("penp: {0,12:0.0000000000}", penp));
        //                    Global.Logger.WriteLine(string.Format("lu19: {0}", lu19));
        //                    Global.Logger.WriteLine(string.Format("empsq: {0,12:0.0000000000}", empsq));
        //                    Global.Logger.WriteLine(string.Format("empmq: {0,12:0.0000000000}", empmq));
        //                    Global.Logger.WriteLine(string.Format("emprq: {0,12:0.0000000000}", emprq));
        //                    Global.Logger.WriteLine(string.Format("empfq: {0,12:0.0000000000}", empfq));
        //                    Global.Logger.WriteLine(string.Format("empoq: {0,12:0.0000000000}", empoq));
        //                    Global.Logger.WriteLine(string.Format("empiq: {0,12:0.0000000000}", empiq));
        //                    Global.Logger.WriteLine(string.Format("emptq: {0,12:0.0000000000}", emptq));
        //                    Global.Logger.WriteLine(string.Format("hhldq: {0,12:0.0000000000}", hhldxq));
        //                    Global.Logger.WriteLine(string.Format("enrsq: {0,12:0.0000000000}", enrsq));
        //                    Global.Logger.WriteLine(string.Format("enruq: {0,12:0.0000000000}", enruq));
        //                    Global.Logger.WriteLine(string.Format("parkq: {0,12:0.0000000000}", parkq));
        //                    Global.Logger.WriteLine(string.Format("mixq: {0,12:0.0000000000}", mixq));
        //                    Global.Logger.WriteLine(string.Format("n34qln: {0,12:0.0000000000}", n34Qln));
        //                    Global.Logger.WriteLine(string.Format("n1sq: {0,12:0.0000000000}", n1Sq));
        //                    Global.Logger.WriteLine(string.Format("n34h: {0,12:0.0000000000}", n34H));
        //                    Global.Logger.WriteLine(string.Format("n34hln: {0,12:0.0000000000}", n34Hln));
        //                    Global.Logger.WriteLine(string.Format("gtim: {0,12:0.0000000000}", gtim));
        //                    Global.Logger.WriteLine(string.Format("ttim: {0,12:0.0000000000}", ttim));
        //                    Global.Logger.WriteLine(string.Format("twin: {0,12:0.0000000000}", twin));
        //                    Global.Logger.WriteLine(string.Format("dist: {0,12:0.0000000000}", dist));
        //                    Global.Logger.WriteLine(string.Format("prox: {0,12:0.0000000000}", prox));
        //                    Global.Logger.WriteLine(string.Format("prxs: {0,12:0.0000000000}", prxs));
        //                    Global.Logger.WriteLine(string.Format("prxo: {0,12:0.0000000000}", prxo));
        //                    Global.Logger.WriteLine(string.Format("disc: {0,12:0.0000000000}", disc));
        //                    Global.Logger.WriteLine(string.Format("wtu2: {0}", wtu2));
        //                    //                    Global.Logger.WriteLine(string.Format("tripZoneId: {0}", tripOriginParcel.ZoneId));
        //                    //                    Global.Logger.WriteLine(string.Format("{0}", selectedParcel.ZoneId));
        //                    //                    Global.Logger.WriteLine(string.Format("{0}", tourOriginParcel.ZoneId));
        //                    Global.Logger.WriteLine(string.Format("tripParcelId: {0}", _trip.OriginParcel.Id));
        //                    Global.Logger.WriteLine(string.Format("selectedParcelId: {0}", destinationParcel.Id));
        //                    Global.Logger.WriteLine(string.Format("tourOriginParcelId: {0}", _trip.Tour.OriginParcel.Id));
        //                    //Global.Logger.WriteLine(string.Format("stopDestinationSize: {0}", sampleItem.DestinationSize));
        //                    //Global.Logger.WriteLine(string.Format("stopDestinationWeight: {0}/{1}/{2}", sampleItem.OriginTripTotalWeight, sampleItem.OriginTourTotalWeight, sampleItem.DestinationTotalWeight));
        //                    Global.Logger.WriteLine(string.Format("adjustmentFactor: {0}", sampleItem.AdjustmentFactor));
        //                    Global.Logger.WriteLine(string.Format("stopsCount: {0}", "todo"));
        //                    Global.Logger.WriteLine(string.Format("stime: {0}", _destinationDepartureTime));
        //                }
        //#endif

        // Generic attributes
        alternative.AddUtilityTerm(1, sampleItem.AdjustmentFactor);
        alternative.AddUtilityTerm(2, twin);
        alternative.AddUtilityTerm(3, gtim);
        alternative.AddUtilityTerm(4, gtis);
        alternative.AddUtilityTerm(5, gtic);
        alternative.AddUtilityTerm(6, disc);
        alternative.AddUtilityTerm(7, prxs);
        alternative.AddUtilityTerm(8, prxo);
        //                alternative.AddUtility(, parkq);
        //                alternative.AddUtility(, parkqln);
        //                alternative.AddUtility(, parkqdivEmp);

        //Attributes specific to Household and Person Characteristics
        alternative.AddUtilityTerm(9, household.HasIncomeUnder50K.ToFlag() * gtim);
        alternative.AddUtilityTerm(10, household.Has100KPlusIncome.ToFlag() * gtim);
        alternative.AddUtilityTerm(11, household.HasMissingIncome.ToFlag() * gtim);
        alternative.AddUtilityTerm(12, fkid * gtim);
        //                alternative.AddUtility(, person.TransitPassOwnership * wtu1);
        //                alternative.AddUtility(, person.TransitPassOwnership * wtu2);

        //Attributes specific to Tour Characteristics
        //                alternative.AddUtility(, wbas * gtim);
        alternative.AddUtilityTerm(13, (!tour.IsWorkPurpose()).ToFlag() * gtim);
        alternative.AddUtilityTerm(14, notFirstFlag * prxs);
        //                alternative.AddUtility(, wbas * prxs);
        //                alternative.AddUtility(, tour.IsSchoolPurpose.ToFlag() * prxs);
        alternative.AddUtilityTerm(15, (!tour.IsHomeBasedTour).ToFlag() * prxo);
        alternative.AddUtilityTerm(16, tour.IsSchoolPurpose().ToFlag() * prxo);
        alternative.AddUtilityTerm(17, bmanFlag * prxo);

        // Attributes specific to Auto tour modes
        alternative.AddUtilityTerm(18, tour.IsAnHovMode().ToFlag() * prxs);
        alternative.AddUtilityTerm(19, tour.IsAnHovMode().ToFlag() * prxo);
        alternative.AddUtilityTerm(20, tour.IsAnAutoMode().ToFlag() * n134Q);
        //                alternative.AddUtility(, tour.IsAnAutoMode.ToFlag() * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
        alternative.AddUtilityTerm(21, tour.IsSovMode().ToFlag() * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
        alternative.AddUtilityTerm(22, tour.IsAnAutoMode().ToFlag() * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
        //                alternative.AddUtility(, tour.IsAnAutoMode.ToFlag() * parkqln);
        //                alternative.AddUtility(, tour.CarModeFlag * penpq);

        // Attributes specific to other modes
        alternative.AddUtilityTerm(23, (!tour.IsAnAutoMode()).ToFlag() * gtim);
        //                alternative.AddUtility(, tour.IsBikeMode.ToFlag() * gtim);
        //                alternative.AddUtility(, tour.IsWalkMode.ToFlag() * gtim);
        alternative.AddUtilityTerm(24, (!tour.IsAnAutoMode()).ToFlag() * gtis);
        alternative.AddUtilityTerm(25, (!tour.IsAnAutoMode()).ToFlag() * gtic);
        //                alternative.AddUtility(, (!tour.IsAnAutoMode).ToFlag() * prxs);
        alternative.AddUtilityTerm(26, tour.IsWalkMode().ToFlag() * prxs);
        alternative.AddUtilityTerm(27, (!tour.IsAnAutoMode()).ToFlag() * prxo);
        alternative.AddUtilityTerm(28, tour.IsBikeMode().ToFlag() * prxo);
        alternative.AddUtilityTerm(29, tour.IsWalkMode().ToFlag() * prxo);
        alternative.AddUtilityTerm(30, tour.IsTransitMode().ToFlag() * wtu1);
        alternative.AddUtilityTerm(31, tour.IsTransitMode().ToFlag() * wtu2);
        //                alternative.AddUtility(, tour.IsBikeMode.ToFlag() * deadEndRatio);

        // Attributes specific to Trip Characteristics
        alternative.AddUtilityTerm(32, workOrSchoolDestinationPurposeFlag * gtim);
        alternative.AddUtilityTerm(33, personalDestinationPurposeFlag * person.IsUniversityStudent.ToFlag() * enruq);


        // OD shadow pricing
        if (Global.Configuration.ShouldUseODShadowPricing) {
          double stopOriginShadowPriceConfigurationParameter = _trip.OriginParcel.District == destinationParcel.District ? Global.Configuration.IntermediateStopLocationOOShadowPriceCoefficient : Global.Configuration.IntermediateStopLocationODShadowPriceCoefficient;
          double tourOriginShadowPriceConfigurationParameter = _trip.Tour.OriginParcel.District == destinationParcel.District ? Global.Configuration.IntermediateStopLocationOOShadowPriceCoefficient : Global.Configuration.IntermediateStopLocationODShadowPriceCoefficient;
          int stopOriginCoefficient = 0;
          int tourOriginCoefficient = 0;
          //var stopOriginCoefficient = _trip.IsToTourOrigin? 
          if (!_trip.IsHalfTourFromOrigin) {   // direction of movement is toward tour origin
            stopOriginCoefficient = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (_trip.OriginParcel.District - 1) + destinationParcel.District;
            tourOriginCoefficient = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (destinationParcel.District - 1) + _trip.Tour.OriginParcel.District;
          } else { //direction of movement is toward tour destination
            stopOriginCoefficient = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (destinationParcel.District - 1) + _trip.OriginParcel.District;
            tourOriginCoefficient = MAX_REGULAR_PARAMETER + Global.Configuration.NumberOfODShadowPricingDistricts * (_trip.Tour.OriginParcel.District - 1) + destinationParcel.District;
          }

          alternative.AddUtilityTerm(stopOriginCoefficient, stopOriginShadowPriceConfigurationParameter * Global.Configuration.IntermediateStopLocationODShadowPriceStopOriginFraction);
          alternative.AddUtilityTerm(tourOriginCoefficient, tourOriginShadowPriceConfigurationParameter * (1 - Global.Configuration.IntermediateStopLocationODShadowPriceStopOriginFraction));
        }



        if (_trip.DestinationPurpose == Global.Settings.Purposes.Work) {
          // Neighborhood
          alternative.AddUtilityTerm(34, workDestinationPurposeFlag * emptq);
          alternative.AddUtilityTerm(35, workDestinationPurposeFlag * enrsq);

          // Size terms
          alternative.AddUtilityTerm(61, workDestinationPurposeFlag * empo);
          alternative.AddUtilityTerm(62, workDestinationPurposeFlag * destinationParcel.EmploymentTotal);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.School) {
          // Neighborhood

          // Size terms
          alternative.AddUtilityTerm(63, schgFlag * empo);
          alternative.AddUtilityTerm(64, schgFlag * enrs);
          alternative.AddUtilityTerm(65, schhFlag * empo);
          alternative.AddUtilityTerm(66, schhFlag * enrs);
          alternative.AddUtilityTerm(67, schuFlag * empo);
          alternative.AddUtilityTerm(68, schuFlag * destinationParcel.StudentsUniversity);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Escort) {
          //                        alternative.AddUtility(, esnkFlag * gtim);
          alternative.AddUtilityTerm(36, eskdFlag * gtim);
          //                        alternative.AddUtility(, esnkFlag * prxs);
          alternative.AddUtilityTerm(37, eskdFlag * prxs);
          alternative.AddUtilityTerm(38, esnkFlag * prxo);
          alternative.AddUtilityTerm(39, eskdFlag * prxo);
          //                        alternative.AddUtility(, esscFlag * prxo);

          // Neighborhood
          alternative.AddUtilityTerm(040, eskdFlag * empiq);
          //                        alternative.AddUtility(, eskdFlag * emprq);
          //                        alternative.AddUtility(, eskdFlag * emptq);
          alternative.AddUtilityTerm(041, eskdFlag * enrsq);
          //                        alternative.AddUtility(, eskdFlag * enrhq);
          //                        alternative.AddUtility(, eskdFlag * hhldxq);
          alternative.AddUtilityTerm(042, esnkFlag * emptq);
          alternative.AddUtilityTerm(043, esnkFlag * enrsq);

          // Size terms
          alternative.AddUtilityTerm(69, eskdFlag * enrs);
          alternative.AddUtilityTerm(70, eskdFlag * destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(71, eskdFlag * destinationParcel.Households);
          alternative.AddUtilityTerm(72, esnkFlag * destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(73, esnkFlag * enrs);
          alternative.AddUtilityTerm(74, esnkFlag * destinationParcel.Households);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness || _trip.DestinationPurpose == Global.Settings.Purposes.Medical) {
          // Neighborhood
          alternative.AddUtilityTerm(044, personalOrMedicalDestinationPurposeFlag * empmq);
          //                        alternative.AddUtility(, personalOrMedicalDestinationPurposeFlag * enrsq);
          alternative.AddUtilityTerm(45, personalOrMedicalDestinationPurposeFlag * empfq);
          alternative.AddUtilityTerm(46, personalOrMedicalDestinationPurposeFlag * emprq);

          // Size terms
          alternative.AddUtilityTerm(75, personalOrMedicalDestinationPurposeFlag * destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(76, personalOrMedicalDestinationPurposeFlag * empi);
          alternative.AddUtilityTerm(77, personalOrMedicalDestinationPurposeFlag * destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(78, personalOrMedicalDestinationPurposeFlag * empo);
          alternative.AddUtilityTerm(79, personalOrMedicalDestinationPurposeFlag * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(80, personalOrMedicalDestinationPurposeFlag * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(81, personalOrMedicalDestinationPurposeFlag * destinationParcel.Households);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Shopping) {
          alternative.AddUtilityTerm(47, shoppingDestinationPurposeFlag * gtim);
          //                        alternative.AddUtility(, shshFlag * gtim);
          alternative.AddUtilityTerm(48, shshFlag * prxs);
          //                        alternative.AddUtility(, shshFlag * prxo);

          // Neighborhood
          alternative.AddUtilityTerm(49, shoppingDestinationPurposeFlag * emprq);

          // Size terms
          alternative.AddUtilityTerm(82, shoppingDestinationPurposeFlag * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(83, shoppingDestinationPurposeFlag * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(84, shoppingDestinationPurposeFlag * destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(85, shoppingDestinationPurposeFlag * destinationParcel.Households);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Meal) {
          alternative.AddUtilityTerm(50, mealDestinationPurposeFlag * gtim);

          // Neighborhood
          alternative.AddUtilityTerm(51, mealDestinationPurposeFlag * empfq);
          //                        alternative.AddUtility(, mealDestinationPurposeFlag * empoq);
          //                        alternative.AddUtility(, mealDestinationPurposeFlag * emprq);

          // Size terms
          alternative.AddUtilityTerm(86, mealDestinationPurposeFlag * destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(87, mealDestinationPurposeFlag * destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(88, mealDestinationPurposeFlag * destinationParcel.Households);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Social || _trip.DestinationPurpose == Global.Settings.Purposes.Recreation) {
          alternative.AddUtilityTerm(52, srec * gtim);

          // Neighborhood
          alternative.AddUtilityTerm(53, srec * empfq);
          alternative.AddUtilityTerm(54, srec * empiq);
          alternative.AddUtilityTerm(55, srec * empsq);
          alternative.AddUtilityTerm(56, srec * emptq);
          //                        alternative.AddUtility(, srec * enrsq);
          alternative.AddUtilityTerm(57, srec * hhldxq);

          // Size terms
          alternative.AddUtilityTerm(89, srec * destinationParcel.EmploymentFood);
          alternative.AddUtilityTerm(90, srec * destinationParcel.EmploymentMedical);
          alternative.AddUtilityTerm(91, srec * destinationParcel.EmploymentService);
          alternative.AddUtilityTerm(92, srec * destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(93, srec * openqFlag);
          alternative.AddUtilityTerm(94, srec * destinationParcel.Households);
        }
      }

      private static void GetGenTime(IRandomUtility randomUtility, int mode, int purpose, double costCoef, double timeCoef, double transitDiscountFraction, int leg, IParcelWrapper tripOrigin, IParcelWrapper tripDestination, IParcelWrapper tourOrigin, out double dis, out double tim, out double gtim, int minute = -1, double parkHours = 0.0) {
        if (minute < 0) {
          // default minute of day
          minute = DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Start;
        }

        IParcelWrapper origin = (leg == 1) ? tripOrigin : tripDestination;
        IParcelWrapper destination = (leg == 1) ? tripDestination : tourOrigin;

        IEnumerable<IPathTypeModel> pathTypeModels =
            PathTypeModelFactory.Singleton.Run(
            randomUtility,
                origin,
                destination,
                minute,
                0,
                purpose,
                costCoef,
                timeCoef,
                true,
                 /* householdCars */ 1,
                  /* transitPassOwnership */ 0,
                /* carsAreAvs */ false,
                transitDiscountFraction,
                false,
                mode);

        IPathTypeModel path = pathTypeModels.First();

        if (path.Available) {
          dis = path.PathDistance;
          tim = path.PathTime;
          gtim = path.GeneralizedTimeChosen;

          //if parking duration > 0, add parking cost equivalent to gtim
          if (parkHours > 0) {
            double pcost = tripDestination.ParkingCostBuffer1(parkHours);

            gtim += pcost * costCoef / (60 * timeCoef);
          }
        } else {
          dis = 0D;
          tim = 0D;
          gtim = Global.Settings.GeneralizedTimeUnavailable;
        }
      }

      public static bool ShouldRunInEstimationModeForModel(ITripWrapper trip, ITourWrapper tour, int tourMode) {
        // determine validity and need, then characteristics
        // detect and skip invalid trip records (error = true) and those that trips that don't require stop location choice (need = false)
        int excludeReason = 0;

        if (_maxZone == -1) {
          // TODO: Verify / Optimize
          _maxZone = ChoiceModelFactory.ZoneKeys.Max(z => z.Key);
        }

        if (_maxParcel == -1) {
          // TODO: Optimize
          _maxParcel = ChoiceModelFactory.Parcels.Values.Max(parcel => parcel.Id);
        }

        if (Global.Configuration.IsInEstimationMode) {
          // set need = false if tour mode is unmodeled mode
          if (tourMode == Global.Settings.Modes.None) {
            excludeReason = 1;
          } else if (tourMode >= Global.Settings.Modes.Other) {
            excludeReason = 2;
          } else if (tour.OriginParcelId > _maxParcel) {
            excludeReason = 3;
          } else if (tour.OriginParcelId <= 0) {
            excludeReason = 4;
          } else if (trip.DestinationParcelId > _maxParcel) {
            excludeReason = 5;
          } else if (trip.DestinationParcelId <= 0) {
            excludeReason = 6;
          } else if (trip.OriginParcelId > _maxParcel) {
            excludeReason = 7;
          } else if (trip.OriginParcelId <= 0) {
            excludeReason = 8;
          } else if (trip.OriginParcelId == trip.DestinationParcelId) {
            excludeReason = 9;
          } else if (tour.OriginParcel.ZoneId == -1) {
            // TODO: Verify this condition... it used to check that the zone was == null. 
            // I'm not sure what the appropriate condition should be though.

            excludeReason = 10;
          }

          if (excludeReason > 0) {
            Global.PrintFile.WriteEstimationRecordExclusionMessage(CHOICE_MODEL_NAME, "ShouldRunInEstimationModeForModel", trip.Household.Id, trip.Person.Sequence, trip.Day, trip.Tour.Sequence, trip.Direction, trip.Sequence, excludeReason);
          }
        }

        bool shouldRun = (excludeReason == 0);

        return shouldRun;
      }
    }
  }
}
