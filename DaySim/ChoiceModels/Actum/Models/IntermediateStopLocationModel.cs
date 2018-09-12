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

using HouseholdDayWrapper = DaySim.DomainModels.Default.Wrappers.HouseholdDayWrapper;
using HouseholdWrapper = DaySim.DomainModels.Default.Wrappers.HouseholdWrapper;
using PersonDayWrapper = DaySim.DomainModels.Default.Wrappers.PersonDayWrapper;
using PersonWrapper = DaySim.DomainModels.Default.Wrappers.PersonWrapper;
using TourWrapper = DaySim.DomainModels.Default.Wrappers.TourWrapper;
using TripWrapper = DaySim.DomainModels.Default.Wrappers.TripWrapper;

namespace DaySim.ChoiceModels.Actum.Models {
  public class IntermediateStopLocationModel : ChoiceModel {
    private const string CHOICE_MODEL_NAME = "ActumIntermediateStopLocationModel";
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 160;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      int sampleSize = Global.Configuration.IntermediateStopLocationModelSampleSize;
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.IntermediateStopLocationModelCoefficients, sampleSize, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(TripWrapper trip, HouseholdDayWrapper householdDay, int sampleSize) {
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
        RunModel(choiceProbabilityCalculator, trip, householdDay, sampleSize, trip.DestinationParcel);
        if (trip.PersonDay.IsValid == false) {
          return;
        }

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, trip, householdDay, sampleSize);
        if (trip.PersonDay.IsValid == false) {
          return;
        }

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(trip.Household.RandomUtility);

        if (chosenAlternative == null) {
          Global.PrintFile.WriteNoAlternativesAvailableWarning(CHOICE_MODEL_NAME, "Run", trip.PersonDay.Id);
          if (!Global.Configuration.IsInEstimationMode) {
            trip.PersonDay.IsValid = false;
          }
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

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, TripWrapper trip, HouseholdDayWrapper householdDay, int sampleSize, IParcelWrapper choice = null) {
      PersonDayWrapper personDay = (PersonDayWrapper)trip.PersonDay;
      TourWrapper tour = (TourWrapper)trip.Tour;

      int destinationDepartureTime =
                trip.IsHalfTourFromOrigin // first trip in half tour, use tour destination time
                    ? trip.Sequence == 1
                          ? tour.DestinationArrivalTime
                          : trip.GetPreviousTrip().ArrivalTime
                    : trip.Sequence == 1
                          ? tour.DestinationDepartureTime
                          : trip.GetPreviousTrip().ArrivalTime;


      TimeWindow timeWindow = new TimeWindow();
      if (tour.JointTourSequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.JointTourSequence == tour.JointTourSequence);
          if (!(tInJoint == null)) {
            // set jointTour time window
            timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
          }
        }
      } else if (trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.FullHalfTour1Sequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.FullHalfTour1Sequence == tour.FullHalfTour1Sequence);
          if (!(tInJoint == null)) {
            // set jointTour time window
            timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
          }
        }
      } else if (trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.FullHalfTour2Sequence > 0) {
        foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
          TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.FullHalfTour2Sequence == tour.FullHalfTour2Sequence);
          if (!(tInJoint == null)) {
            // set jointTour time window
            timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
          }
        }
      } else if (tour.ParentTour == null) {
        timeWindow.IncorporateAnotherTimeWindow(personDay.TimeWindow);
      } else {
        timeWindow.IncorporateAnotherTimeWindow(tour.ParentTour.TimeWindow);
      }

      timeWindow.SetBusyMinutes(Global.Settings.Times.EndOfRelevantWindow, Global.Settings.Times.MinutesInADay + 1);


      // time window in minutes for yet unmodeled portion of halftour, only consider persons on this trip
      int availableWindow = timeWindow.AvailableWindow(destinationDepartureTime, Global.Settings.TimeDirections.Both);

      int segment = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.GetIntermediateStopSegment(trip.DestinationPurpose, trip.Tour.Mode);
      DestinationSampler destinationSampler = new DestinationSampler(choiceProbabilityCalculator, segment, sampleSize, trip.Tour, trip, choice);
      IntermediateStopLocationUtilities intermediateStopLocationUtilities = new IntermediateStopLocationUtilities(trip, sampleSize, destinationDepartureTime, availableWindow);

      //#if TRACE
      //			Global.Logger.AddToBuffer(string.Format("Segment: {0}", segment));
      //#endif

      if (trip.Id == 17151) {
      }

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
      private static readonly int[] _averageStopDuration = new[] { 0, 7521, 10357, 801, 2517, 1956, 4453, 7382, 7382, 2517, 0, 2517 };
      // above value of 2517 for business is placeholder.  Need to identify average duration for CPH

      private readonly TripWrapper _trip;
      //#if TRACE
      //			private readonly CondensedParcel _choice;
      //#endif
      private readonly int _destinationDepartureTime;
      private readonly int _availableWindow;
      private static int _maxZone = -1;
      private static int _maxParcel = -1;

      // JLB 20140319 patched WTHRESH and DLIMPARM to use distance units instead of miles
      private readonly float WTHRESH = 0.25F * (float)Global.Settings.DistanceUnitsPerMile; //if trip is less than this distance, then walk LOS is assumed  (distance units)
      private readonly double DLIMPARM = 30.0 * Global.Settings.DistanceUnitsPerMile; // soto distance limit (distance units) at which gtim sensivity stops changing
      private const double DFACPARM = 0.7; // gtim senstivity at soto distance limit (relative to senstivity at distance of 0)

      private readonly int[] _seedValues;

      public IntermediateStopLocationUtilities(TripWrapper trip, int sampleSize, int destinationDepartureTime, int availableWindow) {
        _trip = trip;
        //#if TRACE
        //				_choice = choice;
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

        HouseholdWrapper household = (HouseholdWrapper)_trip.Household;
        PersonWrapper person = (PersonWrapper)_trip.Person;
        TourWrapper tour = (TourWrapper)_trip.Tour;

        //#if TRACE				
        //	// tour mode
        //				var transitModesFlag = tour.UsesTransitModes.ToFlag();
        //#endif

        // stop purpose
        int businessDestinationPurposeFlag = _trip.IsBusinessDestinationPurpose().ToFlag();
        int schoolStopForChildUnderAge16 = (_trip.IsSchoolDestinationPurpose() && (person.IsChildAge5Through15 || person.IsChildUnder5)).ToFlag();
        int schoolStopForDrivingAgeStudent = (_trip.IsSchoolDestinationPurpose() && person.IsDrivingAgeStudent).ToFlag();
        int schoolStopForAdult = (_trip.IsSchoolDestinationPurpose() && (!person.IsChildAge5Through15 && !person.IsChildUnder5) && !person.IsDrivingAgeStudent).ToFlag();
        //var workOrSchoolDestinationPurposeFlag = _trip.IsWorkOrSchoolDestinationPurpose.ToFlag();
        int escortStop_HouseholdHasChildren = (_trip.IsEscortDestinationPurpose() && (household.HouseholdTotals.DrivingAgeStudents + household.HouseholdTotals.ChildrenAge5Through15 + household.HouseholdTotals.ChildrenUnder5 > 0)).ToFlag();
        int escortStop_HouseholdHasNoChildren = (_trip.IsEscortDestinationPurpose() && (household.HouseholdTotals.DrivingAgeStudents + household.HouseholdTotals.ChildrenAge5Through15 + household.HouseholdTotals.ChildrenUnder5 == 0)).ToFlag();
        //#if TRACE
        int personalBusinessStopPurposeFlag = _trip.IsPersonalBusinessDestinationPurpose().ToFlag();
        //#endif
        int shoppingStopPurposeFlag = _trip.IsShoppingDestinationPurpose().ToFlag();
        int mealStopPurposeFlag = _trip.IsMealDestinationPurpose().ToFlag();
        int socialStopPurposeFlag = _trip.IsSocialDestinationPurpose().ToFlag();
        int recreationStopPurposeFlag = _trip.IsRecreationDestinationPurpose().ToFlag();
        //var srec = (socialDestinationPurposeFlag == 1 || recreationDestinationPurposeFlag == 1).ToFlag();
        int medicalStopPurposeFlag = _trip.IsMedicalDestinationPurpose().ToFlag();
        int personalBusiness_Meal_Shop_SocialStopPurposeFlag = _trip.IsPersonalReasonsDestinationPurpose().ToFlag();
        int personalBusinessOrMedicalStopPurposeFlag = _trip.IsPersonalBusinessOrMedicalDestinationPurpose().ToFlag();

        // tour and trip characteristics
        int escortStopOnSchoolTour = (_trip.IsEscortDestinationPurpose() && tour.IsSchoolPurpose()).ToFlag();
        int shoppingStopOnShoppingTour = (_trip.IsShoppingDestinationPurpose() && tour.IsShoppingPurpose()).ToFlag(); // shoppingDestinationPurposeFlag trip on shoppingDestinationPurposeFlag tour
        int stopIsBeforeMandatoryTourDestination = (_trip.IsHalfTourFromOrigin && (tour.IsWorkPurpose() || tour.IsSchoolPurpose())).ToFlag();
        double parkingTime = _averageStopDuration[_trip.DestinationPurpose] / 6000D; // parking time (hrs)
        int notFirstStopOnHalfTourFlag = (_trip.Sequence != 1).ToFlag();

        double employmentGovernment_Office_Education = destinationParcel.EmploymentGovernment + destinationParcel.EmploymentOffice + destinationParcel.EmploymentEducation;
        double employmentIndustrial_Ag_Construction = destinationParcel.EmploymentIndustrial + destinationParcel.EmploymentAgricultureConstruction;
        double studentsK12 = destinationParcel.StudentsK8 + destinationParcel.StudentsHighSchool;

        double logOfOnePlusEmploymentServiceBuffer1 = Math.Log(destinationParcel.EmploymentServiceBuffer1 + 1);
        double logOfOnePlusEmploymentMedicalBuffer1 = Math.Log(destinationParcel.EmploymentMedicalBuffer1 + 1);
        double logOfOnePlusEmploymentRetailBuffer1 = Math.Log(destinationParcel.EmploymentRetailBuffer1 + 1);
        double logOfOnePlusEmploymentFoodBuffer1 = Math.Log(destinationParcel.EmploymentFoodBuffer1 + 1);
        double logOfOnePlusEmploymentGovernment_Office_EducationBuffer1 = Math.Log(destinationParcel.EmploymentGovernmentBuffer1 + destinationParcel.EmploymentOfficeBuffer1 + destinationParcel.EmploymentEducationBuffer1 + 1);
        double logOfOnePlusEmploymentIndustrial_Ag_ConstructionBuffer1 = Math.Log(destinationParcel.EmploymentIndustrialBuffer1 + destinationParcel.EmploymentAgricultureConstructionBuffer1 + 1);
        double logOfOnePlusEmploymentTotalBuffer1 = Math.Log(destinationParcel.EmploymentTotalBuffer1 + 1);
        double logOfOnePlusHouseholdsBuffer1 = Math.Log(destinationParcel.HouseholdsBuffer1 + 1);
        double logOfOnePlusStudentsK12Buffer1 = Math.Log(destinationParcel.StudentsK8Buffer1 + destinationParcel.StudentsHighSchoolBuffer1 + 1);
        double logOfOnePlusStudentsUniversityBuffer1 = Math.Log(destinationParcel.StudentsUniversityBuffer1 + 1);
        double logOfOnePlusParkingOffStreetPaidHourlySpacesBuffer1 = Math.Log(destinationParcel.ParkingOffStreetPaidHourlySpacesBuffer1 + 1);
        int openSpaceType2IsPresentInBuffer1 = (destinationParcel.OpenSpaceType2Buffer1 > 0).ToFlag();
        double numberOfNetworkNodesInBuffer1 = destinationParcel.NodesSingleLinkBuffer1 + destinationParcel.NodesThreeLinksBuffer1 + destinationParcel.NodesFourLinksBuffer1;

        int stopOnJointTour = (tour.JointTourSequence > 0).ToFlag();
        int stopOnFullJointHalfTour = ((_trip.Direction == Global.Settings.TourDirections.OriginToDestination && tour.FullHalfTour1Sequence > 0)
                    || (_trip.Direction == Global.Settings.TourDirections.DestinationToOrigin && tour.FullHalfTour2Sequence > 0)).ToFlag();

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
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, _trip.OriginParcel, _trip.OriginParcel, out wdis0, out wtime0, out gwtime0);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wdis1, out wtime1, out gwtime1);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wdis2, out wtime2, out gwtime2);
          adis0 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, tour.OriginParcel, _trip.OriginParcel).Variable;
          adis1 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, tour.OriginParcel, destinationParcel).Variable;
          adis2 = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, minute, destinationParcel, _trip.OriginParcel).Variable;
        } else {
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, tour.OriginParcel, tour.OriginParcel, out wdis0, out wtime0, out gwtime0);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wdis1, out wtime1, out gwtime1);
          GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Walk, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wdis2, out wtime2, out gwtime2);
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
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out bdis1, out btime1, out gbtime1);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out bdis2, out btime2, out gbtime2);
          } else {
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out bdis1, out btime1, out gbtime1);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Bike, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out bdis2, out btime2, out gbtime2);
          }

          ttim1 = btime1;
          ttim2 = btime2;
          gtim1 = gbtime1;
          gtim2 = gbtime2;
        } else if (tour.Mode == Global.Settings.Modes.SchoolBus) {
          if (wdis1 < WTHRESH && wdis2 < WTHRESH && wdis1 > Constants.EPSILON && wdis2 > Constants.EPSILON) {
            ttim1 = wtime1;
            ttim2 = wtime2;
            gtim1 = gwtime1;
            gtim2 = gwtime2;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2);
            }

            ttim1 = d1Time1;
            ttim2 = d1Time2;
            gtim1 = gd1Time1;
            gtim2 = gd1Time2;
          }
        } else if (tour.Mode == Global.Settings.Modes.Sov || tour.Mode == Global.Settings.Modes.ParkAndRide || tour.Mode == Global.Settings.Modes.PaidRideShare) {
          if (wdis1 < WTHRESH && wdis2 < WTHRESH && wdis1 > Constants.EPSILON && wdis2 > Constants.EPSILON) {
            ttim1 = wtime1;
            ttim2 = wtime2;
            gtim1 = gwtime1;
            gtim2 = gwtime2;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime, parkingTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime, parkingTime);
            }

            ttim1 = d1Time1;
            ttim2 = d1Time2;
            gtim1 = gd1Time1;
            gtim2 = gd1Time2;
          }
        } else if (tour.Mode == Global.Settings.Modes.HovDriver || tour.Mode == Global.Settings.Modes.HovPassenger) {
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
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.HovDriver, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d2Dis1, out d2Time1, out gd2Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.HovDriver, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d2Dis2, out d2Time2, out gd2Time2, _destinationDepartureTime, parkingTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.HovDriver, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d2Dis1, out d2Time1, out gd2Time1, _destinationDepartureTime, parkingTime);
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.HovDriver, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d2Dis2, out d2Time2, out gd2Time2, _destinationDepartureTime, parkingTime);
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
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wtdis1, out wttime1, out gwttime1, _destinationDepartureTime);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out wtdis2, out wttime2, out gwttime2, _destinationDepartureTime);
          } else {
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wtdis1, out wttime1, out gwttime1, _destinationDepartureTime);
            GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Transit, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out wtdis2, out wttime2, out gwttime2, _destinationDepartureTime);
          }
          if (wdis1 < WTHRESH && wdis1 > Constants.EPSILON) {
            ttim1 = wtime1;
            gtim1 = gwtime1;
          } else if (wttime1 > Constants.EPSILON) {
            ttim1 = wttime1;
            gtim1 = gwttime1;
          } else {
            if (_trip.Direction == Global.Settings.TourDirections.OriginToDestination) {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 1, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis1, out d1Time1, out gd1Time1, _destinationDepartureTime);
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
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, tour.OriginParcel, destinationParcel, _trip.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime);
            } else {
              GetGenTime(_trip.Household.RandomUtility, Global.Settings.Modes.Sov, purpose, costCoef, timeCoef, transitDiscountFraction, _trip.Person.TransitPassOwnership, 2, _trip.OriginParcel, destinationParcel, tour.OriginParcel, out d1Dis2, out d1Time2, out gd1Time2, _destinationDepartureTime);
            }

            ttim2 = d1Time2;
            gtim2 = gd1Time2;
          }
        }

        // calculate variables derived from ttim1, ttim2, gtim1 and gtim2
        double dist = Math.Max(adis1 + adis2 - adis0, Math.Min(adis1 * 0.1, adis2 * 0.1)); // incremental distance (with imposed lower limit), in units of miles
        double gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes = (gtim1 + gtim2) / 100.0; // approximate incremental gtime, rescale to units of (100 minutes)  //rescale to units of (100 minutes

        gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes = gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes * dist / Math.Max(0.1, (adis1 + adis2)); // convert to approximate incremental gtime  
        gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes = gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes * (1.0 + (DFACPARM - 1.0) * Math.Min(adis0 / DLIMPARM, 1.0)); // transform to make gtim sensitivity a function of distsoto 

        if (gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes > 1.8) {
          // cap added because of huge utilities - multipled by very large coefficient- MAB 0625
          gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes = 1.8;
        }

        double gtimeSquared = gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes;
        double gtimeCubed = gtimeSquared * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes;
        double ttim = Math.Min((ttim1 + ttim2) / 100.0, 3.6); // rescale to units of (100 minutes)
        double logOfOneMinusRatioOfTravelTimeToAvailableTimeWindow = ttim / (Math.Max(1.0, _availableWindow) / 100D); // unit-free ratio of ttim and window, both measured in 100 minute units

        if (logOfOneMinusRatioOfTravelTimeToAvailableTimeWindow >= 1) {
          alternative.Available = false;

          return;
        }

        logOfOneMinusRatioOfTravelTimeToAvailableTimeWindow = Math.Log(1 - logOfOneMinusRatioOfTravelTimeToAvailableTimeWindow);

        //				ttim = ttim * dist / Math.Max(0.1, (adis1 + adis2)); // ocnvert to approximate incremental ttime
        dist = dist / 100.0; // convert to units of 100 minutes

        //JLB 20140319 .5, .25 and .125 represented hundreds of miles, squared and cubed.
        //             adjusted code to use distance units
        //var diss = Math.Min(0.25, dist * dist);
        //var detourDistanceCubedInHundredsOfMilesCubed = Math.Min(0.125, diss * dist);
        double maxDist = .5 * Global.Settings.DistanceUnitsPerMile;  // in hundreds of distance units
        double diss = Math.Min(maxDist * maxDist, dist * dist);
        double detourDistanceCubedInHundredsOfDistanceUnitsCubed = Math.Min(maxDist * maxDist * maxDist, diss * dist);



        //				var prox = 1.0 / (Math.Max(1.0, Math.Min(ttim1, ttim2)) / 10.0);

        double proximityToStopOrigin_10Is1min_1Is10min_point1Is100min;
        double proximityToTourOrigin_10Is1min_1Is10min_point1Is100min;

        if (_trip.IsHalfTourFromOrigin) {
          proximityToStopOrigin_10Is1min_1Is10min_point1Is100min = 1 / (Math.Max(1, ttim2) / 10);
          proximityToTourOrigin_10Is1min_1Is10min_point1Is100min = 1 / (Math.Max(1, ttim1) / 10);
        } else {
          proximityToStopOrigin_10Is1min_1Is10min_point1Is100min = 1 / (Math.Max(1, ttim1) / 10);
          proximityToTourOrigin_10Is1min_1Is10min_point1Is100min = 1 / (Math.Max(1, ttim2) / 10);
        }

        // calculate walk and transit unavailability indicators (one leg and two leg)
        int walkAndTransitAreInaccessibleForOneLegOfDetour = 0;
        int walkAndTransitAreInaccessibleForBothLegsOfDetour = 0;
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
          walkAndTransitAreInaccessibleForOneLegOfDetour = 1;
        }

        if (wta == 0) {
          walkAndTransitAreInaccessibleForBothLegsOfDetour = 1;
        }

        //				 if (tour.Mode == Global.Settings.Modes.Transit && wtu1 > 0) {
        //				 	int check3 = 1;
        //				 }
        //
        //				int check = 0;
        //				int check1 = 0;

        // JLB 20110620 handle cases where LOS calcs failed to yield valid times
        if (ttim1 < Constants.EPSILON || ttim2 < Constants.EPSILON) {
          alternative.Available = false; // taltaval[i]:=0;
                                         //					check = 1;
        }
        //				if (tour.Mode == 2){
        //				check1 = 0;
        //				}
        //				if (tour.Mode == 3){
        //				check1 = 0;
        //				}
        //				if (tour.Mode == 5){
        //				check1 = 0;
        //				}
        //				if (tour.Mode == 6){
        //				check1 = 0;
        //				}
        //				if (tour.Mode == 7){
        //				check1 = 0;
        //				}
        //				if (tour.Mode == 8){
        //				check1 = 0;
        //				}

        int adultFemaleWithChildren = (person.Gender == Global.Settings.PersonGenders.Female && person.PersonType < Global.Settings.PersonTypes.DrivingAgeStudent && (household.HouseholdTotals.ChildrenAge5Through15 + household.HouseholdTotals.ChildrenUnder5 + household.HouseholdTotals.DrivingAgeStudents > 0)).ToFlag();
        //#if TRACE
        //				var wrkt = tour.IsWorkPurpose.ToFlag();
        //				var n34Qln = Math.Log(1 + destinationParcel.NodesThreeLinksBuffer1 / 2.0 + destinationParcel.NodesFourLinksBuffer1);
        //				var n1Sq = destinationParcel.NodesSingleLinkBuffer1 / (Math.Max(1, destinationParcel.NodesSingleLinkBuffer1 + destinationParcel.NodesThreeLinksBuffer1 + destinationParcel.NodesFourLinksBuffer1));
        //				var n34H = destinationParcel.NodesThreeLinksBuffer2 / 2.0 + destinationParcel.NodesFourLinksBuffer2;
        //				var n34Hln = Math.Log(1 + destinationParcel.NodesThreeLinksBuffer2 / 2.0 + destinationParcel.NodesFourLinksBuffer2);
        //				var wbas = (!tour.IsHomeBasedTour).ToFlag();
        //				var bwork = wrkt == 1 && _trip.Direction == 1 ? 1 : 0;
        //				var nwrkt = 1 - wrkt;
        //				var nwst = wrkt != 1 && tour.IsSchoolPurpose.ToFlag() != 1 ? 1 : 0; // not _t.WorkDestinationPurposeFlag or school tour
        //				var essc = _trip.DestinationPurpose == Global.Settings.Purposes.Escort && _trip.Tour.DestinationPurpose == Global.Settings.Purposes.School ? 1 : 0;
        //				var shsh = shoppingDestinationPurposeFlag == 1 && _trip.Tour.DestinationPurpose == Global.Settings.Purposes.Shopping ? 1 : 0; // shoppingDestinationPurposeFlag trip on shoppingDestinationPurposeFlag tour
        //				var bman = _trip.Direction == 1 && (_trip.Tour.DestinationPurpose == Global.Settings.Purposes.Work || _trip.Tour.DestinationPurpose == Global.Settings.Purposes.School) ? 1 : 0;
        //
        //				//              The following output was used for comparing Delphi and CS results at the alternative level (or for the chosen alternative)
        //				if (Global.Configuration.IsInEstimationMode && _choice.Id == destinationParcel.Id && alternative.Available) {
        //					Global.Logger.SetCondition(true);
        //					Global.Logger.WriteLine(string.Format("Household Id: {0}", household.Id));
        //					Global.Logger.WriteLine(string.Format("Day: {0}", _trip.Day));
        //					Global.Logger.WriteLine(string.Format("Person: {0}", person.Sequence));
        //					Global.Logger.WriteLine(string.Format("Tour: {0}", tour.Sequence));
        //					Global.Logger.WriteLine(string.Format("Half: {0}", _trip.Direction));
        //					Global.Logger.WriteLine(string.Format("Trip: {0}", _trip.Sequence));
        //					Global.Logger.WriteLine(string.Format("Work: {0}", _trip.IsWorkDestinationPurpose.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("schgFlag: {0}", schgFlag));
        //					Global.Logger.WriteLine(string.Format("schhFlag: {0}", schhFlag));
        //					Global.Logger.WriteLine(string.Format("schuFlag: {0}", schuFlag));
        //					Global.Logger.WriteLine(string.Format("worksch: {0}", workOrSchoolDestinationPurposeFlag));
        //					Global.Logger.WriteLine(string.Format("esco: {0}", escoFlag));
        //					Global.Logger.WriteLine(string.Format("eskdFlag: {0}", eskdFlag));
        //					Global.Logger.WriteLine(string.Format("esnkFlag: {0}", esnkFlag));
        //					Global.Logger.WriteLine(string.Format("pbus: {0}", personalBusinessDestinationPurposeFlag));
        //					Global.Logger.WriteLine(string.Format("ship: {0}", shoppingDestinationPurposeFlag));
        //					Global.Logger.WriteLine(string.Format("meal: {0}", mealDestinationPurposeFlag));
        //					Global.Logger.WriteLine(string.Format("srec: {0}", srec));
        //					Global.Logger.WriteLine(string.Format("medi: {0}", medicalDestinationPurposeFlag));
        //					Global.Logger.WriteLine(string.Format("personal: {0}", personalDestinationPurposeFlag));
        //					// {tour mode}
        //					Global.Logger.WriteLine(string.Format("walk: {0}", tour.IsWalkMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("bike: {0}", tour.IsBikeMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("aut1: {0}", tour.IsSovMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("aut2: {0}", tour.IsHov2Mode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("au2p: {0}", tour.UsesHovModes.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("au3p: {0}", tour.IsHov3Mode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("trnw: {0}", tour.IsTransitMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("trna: {0}", tour.IsParkAndRideMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("sbus: {0}", tour.IsSchoolBusMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("nmot: {0}", tour.IsWalkOrBikeMode.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("auto: {0}", tour.UsesSovOrHovModes.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("naut: {0}", (!tour.UsesSovOrHovModes).ToFlag()));
        //					Global.Logger.WriteLine(string.Format("tran: {0}", transitModesFlag));
        //					Global.Logger.WriteLine(string.Format("wbas: {0}", wbas));
        //					Global.Logger.WriteLine(string.Format("wrkt: {0}", wrkt));
        //					Global.Logger.WriteLine(string.Format("bwork: {0}", bwork));
        //					Global.Logger.WriteLine(string.Format("nwrkt: {0}", nwrkt));
        //					Global.Logger.WriteLine(string.Format("scht: {0}", tour.IsSchoolPurpose.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("nwst: {0}", nwst));
        //					Global.Logger.WriteLine(string.Format("essc: {0}", essc));
        //					Global.Logger.WriteLine(string.Format("shsh: {0}", shsh));
        //					Global.Logger.WriteLine(string.Format("bman: {0}", bman));
        //					Global.Logger.WriteLine(string.Format("notFirst: {0}", notFirstFlag));
        //					Global.Logger.WriteLine(string.Format("IsFulltimeWorker: {0}", person.IsFulltimeWorker));
        //					Global.Logger.WriteLine(string.Format("IsPartTimeWorker: {0}", person.IsPartTimeWorker));
        //					Global.Logger.WriteLine(string.Format("IsRetiredAdult: {0}", person.IsRetiredAdult));
        //					Global.Logger.WriteLine(string.Format("IsNonworkingAdult: {0}", person.IsNonworkingAdult));
        //					Global.Logger.WriteLine(string.Format("IsUniversityStudent: {0}", person.IsUniversityStudent));
        //					Global.Logger.WriteLine(string.Format("IsDrivingAgeStudent: {0}", person.IsDrivingAgeStudent));
        //					Global.Logger.WriteLine(string.Format("IsChildAge5Through15: {0}", person.IsChildAge5Through15));
        //					Global.Logger.WriteLine(string.Format("IsChildUnder5: {0}", person.IsChildUnder5));
        //					Global.Logger.WriteLine(string.Format("youth: {0}", person.IsYouth.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("fkid: {0}", fkid));
        //					Global.Logger.WriteLine(string.Format("msincome: {0}", household.HasMissingIncome.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("IncLo: {0}", household.HasLowIncome.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("IncHi: {0}", household.HasHighIncome.ToFlag()));
        //					Global.Logger.WriteLine(string.Format("window: {0}", _availableWindow));
        //					Global.Logger.WriteBuffer(); // Should write the buffered segment.
        //					Global.Logger.WriteLine(string.Format("emps: {0,12:0.0000000000}", emps));
        //					Global.Logger.WriteLine(string.Format("empm: {0,12:0.0000000000}", empm));
        //					Global.Logger.WriteLine(string.Format("empr: {0,12:0.0000000000}", empr));
        //					Global.Logger.WriteLine(string.Format("empf: {0,12:0.0000000000}", empf));
        //					Global.Logger.WriteLine(string.Format("empo: {0,12:0.0000000000}", empo));
        //					Global.Logger.WriteLine(string.Format("empi: {0,12:0.0000000000}", empi));
        //					Global.Logger.WriteLine(string.Format("empt: {0,12:0.0000000000}", empt));
        //					Global.Logger.WriteLine(string.Format("hhldx: {0,12:0.0000000000}", hhldx));
        //					Global.Logger.WriteLine(string.Format("enrs: {0,12:0.0000000000}", enrs));
        //					Global.Logger.WriteLine(string.Format("enru: {0,12:0.0000000000}", enru));
        //					Global.Logger.WriteLine(string.Format("park: {0,12:0.0000000000}", park));
        //					Global.Logger.WriteLine(string.Format("pxep: {0,12:0.0000000000}", pxep));
        //					Global.Logger.WriteLine(string.Format("penp: {0,12:0.0000000000}", penp));
        //					Global.Logger.WriteLine(string.Format("lu19: {0}", lu19));
        //					Global.Logger.WriteLine(string.Format("empsq: {0,12:0.0000000000}", empsq));
        //					Global.Logger.WriteLine(string.Format("empmq: {0,12:0.0000000000}", empmq));
        //					Global.Logger.WriteLine(string.Format("emprq: {0,12:0.0000000000}", emprq));
        //					Global.Logger.WriteLine(string.Format("empfq: {0,12:0.0000000000}", empfq));
        //					Global.Logger.WriteLine(string.Format("empoq: {0,12:0.0000000000}", empoq));
        //					Global.Logger.WriteLine(string.Format("empiq: {0,12:0.0000000000}", empiq));
        //					Global.Logger.WriteLine(string.Format("emptq: {0,12:0.0000000000}", emptq));
        //					Global.Logger.WriteLine(string.Format("hhldq: {0,12:0.0000000000}", hhldxq));
        //					Global.Logger.WriteLine(string.Format("enrsq: {0,12:0.0000000000}", enrsq));
        //					Global.Logger.WriteLine(string.Format("enruq: {0,12:0.0000000000}", enruq));
        //					Global.Logger.WriteLine(string.Format("parkq: {0,12:0.0000000000}", parkq));
        //					Global.Logger.WriteLine(string.Format("mixq: {0,12:0.0000000000}", mixq));
        //					Global.Logger.WriteLine(string.Format("n34qln: {0,12:0.0000000000}", n34Qln));
        //					Global.Logger.WriteLine(string.Format("n1sq: {0,12:0.0000000000}", n1Sq));
        //					Global.Logger.WriteLine(string.Format("n34h: {0,12:0.0000000000}", n34H));
        //					Global.Logger.WriteLine(string.Format("n34hln: {0,12:0.0000000000}", n34Hln));
        //					Global.Logger.WriteLine(string.Format("gtim: {0,12:0.0000000000}", gtim));
        //					Global.Logger.WriteLine(string.Format("ttim: {0,12:0.0000000000}", ttim));
        //					Global.Logger.WriteLine(string.Format("twin: {0,12:0.0000000000}", twin));
        //					Global.Logger.WriteLine(string.Format("dist: {0,12:0.0000000000}", dist));
        //					Global.Logger.WriteLine(string.Format("prox: {0,12:0.0000000000}", prox));
        //					Global.Logger.WriteLine(string.Format("prxs: {0,12:0.0000000000}", prxs));
        //					Global.Logger.WriteLine(string.Format("prxo: {0,12:0.0000000000}", prxo));
        //					Global.Logger.WriteLine(string.Format("disc: {0,12:0.0000000000}", disc));
        //					Global.Logger.WriteLine(string.Format("wtu2: {0}", wtu2));
        //					//					Global.Logger.WriteLine(string.Format("tripZoneId: {0}", tripOriginParcel.ZoneId));
        //					//					Global.Logger.WriteLine(string.Format("{0}", selectedParcel.ZoneId));
        //					//					Global.Logger.WriteLine(string.Format("{0}", tourOriginParcel.ZoneId));
        //					Global.Logger.WriteLine(string.Format("tripParcelId: {0}", _trip.OriginParcel.Id));
        //					Global.Logger.WriteLine(string.Format("selectedParcelId: {0}", destinationParcel.Id));
        //					Global.Logger.WriteLine(string.Format("tourOriginParcelId: {0}", _trip.Tour.OriginParcel.Id));
        //					//Global.Logger.WriteLine(string.Format("stopDestinationSize: {0}", sampleItem.DestinationSize));
        //					//Global.Logger.WriteLine(string.Format("stopDestinationWeight: {0}/{1}/{2}", sampleItem.OriginTripTotalWeight, sampleItem.OriginTourTotalWeight, sampleItem.DestinationTotalWeight));
        //					Global.Logger.WriteLine(string.Format("adjustmentFactor: {0}", sampleItem.AdjustmentFactor));
        //					Global.Logger.WriteLine(string.Format("stopsCount: {0}", "todo"));
        //					Global.Logger.WriteLine(string.Format("stime: {0}", _destinationDepartureTime));
        //				}
        //#endif

        // Generic attributes
        alternative.AddUtilityTerm(1, sampleItem.AdjustmentFactor); //GV: constrained to 1.00, 21. aug. 2013

        alternative.AddUtilityTerm(2, logOfOneMinusRatioOfTravelTimeToAvailableTimeWindow);

        //alternative.AddUtilityTerm(3, gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes); //GV commented out July 8th
        //alternative.AddUtilityTerm(4, gtimeSquared); //GV commented out July 8th
        //alternative.AddUtilityTerm(5, gtimeCubed); //GV commented out July 8th
        alternative.AddUtilityTerm(6, detourDistanceCubedInHundredsOfDistanceUnitsCubed);
        alternative.AddUtilityTerm(7, proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(8, proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        //				alternative.AddUtility(, parkq);
        //				alternative.AddUtility(, parkqln);
        //				alternative.AddUtility(, parkqdivEmp);

        //Attributes specific to Household and Person Characteristics
        //alternative.AddUtilityTerm(9, (household.Income >= 300000 && household.Income < 600000).ToFlag() * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
        //alternative.AddUtilityTerm(10, (household.Income >= 600000 && household.Income < 900000).ToFlag() * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
        //alternative.AddUtilityTerm(11, (household.Income >= 900000).ToFlag() * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
        //alternative.AddUtilityTerm(12, adultFemaleWithChildren * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
        //				alternative.AddUtility(, person.TransitPassOwnership * wtu1);
        //				alternative.AddUtility(, person.TransitPassOwnership * wtu2);

        //Attributes specific to Tour Characteristics
        //				alternative.AddUtility(, wbas * gtim);
        //alternative.AddUtilityTerm(13, (!tour.IsWorkPurpose).ToFlag() * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
        alternative.AddUtilityTerm(14, notFirstStopOnHalfTourFlag * proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
        //				alternative.AddUtility(, wbas * prxs);
        //				alternative.AddUtility(, tour.IsSchoolPurpose.ToFlag() * prxs);
        //alternative.AddUtilityTerm(15, (!tour.IsHomeBasedTour).ToFlag() * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);//GV commented out July 8th
        alternative.AddUtilityTerm(16, tour.IsSchoolPurpose().ToFlag() * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(17, stopIsBeforeMandatoryTourDestination * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        //alternative.AddUtilityTerm(78, stopOnJointTour * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);//GV commented out July 8th
        //alternative.AddUtilityTerm(79, stopOnFullJointHalfTour * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);//GV commented out July 8th

        // Attributes specific to Auto tour modes
        alternative.AddUtilityTerm(18, tour.IsAnHovMode().ToFlag() * proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(19, tour.IsAnHovMode().ToFlag() * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(20, tour.IsAnAutoMode().ToFlag() * numberOfNetworkNodesInBuffer1);
        //				alternative.AddUtility(, tour.IsAnAutoMode.ToFlag() * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
        alternative.AddUtilityTerm(21, tour.IsSovMode().ToFlag() * destinationParcel.ParkingHourlyEmploymentCommercialMixBuffer1());
        alternative.AddUtilityTerm(22, tour.IsAnAutoMode().ToFlag() * destinationParcel.ParkingHourlyEmploymentCommercialMixInParcel());
        //alternative.AddUtilityTerm(70, tour.IsAnAutoMode.ToFlag() * logOfOnePlusParkingOffStreetPaidHourlySpacesBuffer1);
        //				alternative.AddUtility(, tour.CarModeFlag * penpq);

        // Attributes specific to other modes
        //alternative.AddUtilityTerm(23, (!tour.IsAnAutoMode).ToFlag() * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes); //GV commented out July 8th
        //				alternative.AddUtility(, tour.IsBikeMode.ToFlag() * gtim);
        //				alternative.AddUtility(, tour.IsWalkMode.ToFlag() * gtim);
        //alternative.AddUtilityTerm(24, (!tour.IsAnAutoMode).ToFlag() * gtimeSquared); //GV commented out July 8th
        //alternative.AddUtilityTerm(25, (!tour.IsAnAutoMode).ToFlag() * gtimeCubed);//GV commented out July 8th
        //				alternative.AddUtility(, (!tour.IsAnAutoMode).ToFlag() * prxs);
        alternative.AddUtilityTerm(26, tour.IsWalkMode().ToFlag() * proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(27, (!tour.IsAnAutoMode()).ToFlag() * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(28, tour.IsBikeMode().ToFlag() * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(29, tour.IsWalkMode().ToFlag() * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
        alternative.AddUtilityTerm(30, tour.IsTransitMode().ToFlag() * walkAndTransitAreInaccessibleForOneLegOfDetour);
        alternative.AddUtilityTerm(31, tour.IsTransitMode().ToFlag() * walkAndTransitAreInaccessibleForBothLegsOfDetour);
        //				alternative.AddUtility(, tour.IsBikeMode.ToFlag() * deadEndRatio);

        // Attributes specific to Trip Characteristics
        //alternative.AddUtilityTerm(32, workOrSchoolDestinationPurposeFlag * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
        //alternative.AddUtilityTerm(33, personalBusiness_Meal_Shop_SocialStopPurposeFlag * person.IsYouth.ToFlag() * logOfOnePlusStudentsK12Buffer1);
        //alternative.AddUtilityTerm(77, personalBusiness_Meal_Shop_SocialStopPurposeFlag * person.IsUniversityStudent.ToFlag() * logOfOnePlusStudentsUniversityBuffer1);

        if (_trip.DestinationPurpose == Global.Settings.Purposes.Business) {
          // Neighborhood
          alternative.AddUtilityTerm(34, businessDestinationPurposeFlag * logOfOnePlusEmploymentTotalBuffer1);
          //alternative.AddUtilityTerm(35, businessDestinationPurposeFlag * logOfOnePlusStudentsK12Buffer1);

          // Size terms
          alternative.AddUtilityTerm(101, businessDestinationPurposeFlag * employmentGovernment_Office_Education);
          //GV: 158 is fixed to zero
          alternative.AddUtilityTerm(158, businessDestinationPurposeFlag * destinationParcel.EmploymentService);
          //alternative.AddUtilityTerm(102, businessDestinationPurposeFlag * destinationParcel.EmploymentTotal);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.School) {
          // Neighborhood

          // Size terms
          //alternative.AddUtilityTerm(103, schoolStopForChildUnderAge16 * employmentGovernment_Office_Education);
          //alternative.AddUtilityTerm(105, schoolStopForDrivingAgeStudent * employmentGovernment_Office_Education);
          //alternative.AddUtilityTerm(151, schoolStopForDrivingAgeStudent * destinationParcel.Households);

          //GV: 107 is fixed to zero
          alternative.AddUtilityTerm(107, schoolStopForAdult * employmentGovernment_Office_Education);
          alternative.AddUtilityTerm(108, schoolStopForAdult * destinationParcel.StudentsUniversity);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Escort) {
          //alternative.AddUtilityTerm(67, escortStop_HouseholdHasNoChildren * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
          //alternative.AddUtilityTerm(36, escortStop_HouseholdHasChildren * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
          //alternative.AddUtilityTerm(68, escortStop_HouseholdHasNoChildren * proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
          //alternative.AddUtilityTerm(37, escortStop_HouseholdHasChildren * proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
          //alternative.AddUtilityTerm(38, escortStop_HouseholdHasNoChildren * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
          //alternative.AddUtilityTerm(39, escortStop_HouseholdHasChildren * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);
          //alternative.AddUtilityTerm(69, escortStopOnSchoolTour * proximityToTourOrigin_10Is1min_1Is10min_point1Is100min);

          // Neighborhood
          alternative.AddUtilityTerm(042, escortStop_HouseholdHasNoChildren * logOfOnePlusEmploymentTotalBuffer1);
          //alternative.AddUtilityTerm(043, escortStop_HouseholdHasNoChildren * logOfOnePlusStudentsK12Buffer1);

          // Size terms
          alternative.AddUtilityTerm(110, escortStop_HouseholdHasChildren * destinationParcel.EmploymentTotal);
          alternative.AddUtilityTerm(111, escortStop_HouseholdHasChildren * destinationParcel.Households);
          //alternative.AddUtilityTerm(112, escortStop_HouseholdHasNoChildren * destinationParcel.EmploymentTotal);
          //alternative.AddUtilityTerm(114, escortStop_HouseholdHasNoChildren * destinationParcel.Households);
          //alternative.AddUtilityTerm(148, escortStop_HouseholdHasNoChildren * destinationParcel.StudentsUniversity);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness) {
          // Neighborhood
          alternative.AddUtilityTerm(46, personalBusinessStopPurposeFlag * logOfOnePlusEmploymentRetailBuffer1);

          // Size terms
          alternative.AddUtilityTerm(118, personalBusinessStopPurposeFlag * employmentGovernment_Office_Education);
          //alternative.AddUtilityTerm(119, personalBusinessStopPurposeFlag * destinationParcel.EmploymentRetail);
          //GV: 120 is fixed to zero
          alternative.AddUtilityTerm(120, personalBusinessStopPurposeFlag * destinationParcel.EmploymentService);
          //alternative.AddUtilityTerm(121, personalBusinessStopPurposeFlag * destinationParcel.Households);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Shopping) {
          //alternative.AddUtilityTerm(47, shoppingStopPurposeFlag * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);
          //						alternative.AddUtility(, shshFlag * gtim);
          alternative.AddUtilityTerm(48, shoppingStopOnShoppingTour * proximityToStopOrigin_10Is1min_1Is10min_point1Is100min);
          //						alternative.AddUtility(, shshFlag * prxo);

          // Neighborhood
          alternative.AddUtilityTerm(49, shoppingStopPurposeFlag * logOfOnePlusEmploymentRetailBuffer1);

          // Size terms 
          //GV: 122 is fixed to zero
          alternative.AddUtilityTerm(122, shoppingStopPurposeFlag * destinationParcel.EmploymentRetail);
          alternative.AddUtilityTerm(123, shoppingStopPurposeFlag * destinationParcel.EmploymentService);
          //alternative.AddUtilityTerm(124, shoppingStopPurposeFlag * destinationParcel.EmploymentTotal);
          //alternative.AddUtilityTerm(125, shoppingStopPurposeFlag * destinationParcel.Households);
        } else if (_trip.DestinationPurpose == Global.Settings.Purposes.Social) {
          alternative.AddUtilityTerm(52, socialStopPurposeFlag * gtime_DistanceSensitiveDetourGeneralizedTimeHundredsOfMinutes);

          // Neighborhood
          alternative.AddUtilityTerm(55, socialStopPurposeFlag * logOfOnePlusEmploymentServiceBuffer1);
          alternative.AddUtilityTerm(56, socialStopPurposeFlag * logOfOnePlusEmploymentTotalBuffer1);
          alternative.AddUtilityTerm(57, socialStopPurposeFlag * logOfOnePlusHouseholdsBuffer1);

          // Size terms
          alternative.AddUtilityTerm(152, socialStopPurposeFlag * destinationParcel.EmploymentRetail);
          //GV: 131 is fixed to zero
          alternative.AddUtilityTerm(131, socialStopPurposeFlag * destinationParcel.EmploymentService);
          //alternative.AddUtilityTerm(132, socialStopPurposeFlag * destinationParcel.EmploymentTotal);
          //alternative.AddUtilityTerm(133, socialStopPurposeFlag * openSpaceType2IsPresentInBuffer1);
          //alternative.AddUtilityTerm(134, socialStopPurposeFlag * destinationParcel.Households);
          //alternative.AddUtilityTerm(154, socialStopPurposeFlag * destinationParcel.StudentsUniversity); 
        }
      }

      private static void GetGenTime(IRandomUtility randomUtility, int mode, int purpose, double costCoef, double timeCoef, double transitDiscountFraction, int transitPassOwnership, int leg, IParcelWrapper tripOrigin, IParcelWrapper tripDestination, IParcelWrapper tourOrigin, out double dis, out double tim, out double gtim, int minute = -1, double parkHours = 0.0) {
        if (minute < 0) {
          // default minute of day
          minute = DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Start;
        }

        IParcelWrapper origin = (leg == 1) ? tripOrigin : tripDestination;
        IParcelWrapper destination = (leg == 1) ? tripDestination : tourOrigin;

        IEnumerable<dynamic> pathTypeModels =
            PathTypeModelFactory.Model.Run(
            randomUtility,
                origin,
                destination,
                minute,
                0,
                purpose,
                costCoef,
                timeCoef,
                true,
                1,
                transitPassOwnership,
                false,
                transitDiscountFraction,
                false,
                mode);

        dynamic path = pathTypeModels.First();

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
