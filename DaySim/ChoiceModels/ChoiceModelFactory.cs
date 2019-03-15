// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;
using DaySim.ShadowPricing;

namespace DaySim.ChoiceModels {
  public static class ChoiceModelFactory {
    private static Type _type;

    public static int[] TotalTimesHouseholdModelSuiteRun { get; set; }

    public static int[] TotalTimesPersonModelSuiteRun { get; set; }

    public static int[] TotalTimesHouseholdDayModelSuiteRun { get; set; }

    public static int[] TotalTimesJointHalfTourGenerationModelSuiteRun { get; set; }

    public static int[] TotalTimesJointTourGenerationModelSuiteRun { get; set; }

    public static int[] TotalTimesPersonDayMandatoryModelSuiteRun { get; set; }

    public static int[] TotalTimesPersonDayModelSuiteRun { get; set; }

    public static int[] TotalTimesPartialJointHalfTourModelSuiteRun { get; set; }

    public static int[] TotalTimesFullJointHalfTourModelSuiteRun { get; set; }

    public static int[] TotalTimesMandatoryTourModelSuiteRun { get; set; }

    public static int[] TotalTimesJointTourModelSuiteRun { get; set; }

    public static int[] TotalTimesNonMandatoryTourModelSuiteRun { get; set; }

    public static int[] TotalTimesTourModelSuiteRun { get; set; }

    public static int[] TotalTimesTourTripModelsRun { get; set; }

    public static int[] TotalTimesTourSubtourModelsRun { get; set; }

    public static int[] TotalTimesProcessHalfToursRun { get; set; }

    public static int[] TotalTimesTourSubtourModelSuiteRun { get; set; }

    public static int[] TotalTimesSubtourTripModelsRun { get; set; }

    public static int[] TotalTimesProcessHalfSubtoursRun { get; set; }

    public static int[] TotalTimesAutoOwnershipModelRun { get; set; }

    public static int[] TotalTimesWorkLocationModelRun { get; set; }

    public static int[] TotalTimesSchoolLocationModelRun { get; set; }

    public static int[] TotalTimesPaidParkingAtWorkplaceModelRun { get; set; }

    public static int[] TotalTimesWorkUsualModeAndScheduleModelRun { get; set; }

    public static int[] TotalTimesTransitPassOwnershipModelRun { get; set; }

    public static int[] TotalTimesMandatoryTourGenerationModelRun { get; set; }

    public static int[] TotalTimesMandatoryStopPresenceModelRun { get; set; }

    public static int[] TotalTimesJointHalfTourGenerationModelRun { get; set; }

    public static int[] TotalTimesFullJointHalfTourParticipationModelRun { get; set; }

    public static int[] TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun { get; set; }

    public static int[] TotalTimesJointTourGenerationModelRun { get; set; }

    public static int[] TotalTimesJointTourParticipationModelRun { get; set; }

    public static int[] TotalTimesPersonDayPatternModelRun { get; set; }

    public static int[] TotalTimesPersonTourGenerationModelRun { get; set; }

    public static int[] TotalTimesPersonExactNumberOfToursModelRun { get; set; }

    public static int[] TotalTimesWorkTourDestinationModelRun { get; set; }

    public static int[] TotalTimesTourDestinationModelRun { get; set; }

    public static int[] TotalTimesOtherTourDestinationModelRun { get; set; }

    public static int[] TotalTimesWorkBasedSubtourGenerationModelRun { get; set; }

    public static int[] TotalTimesTourModeTimeModelRun { get; set; }

    public static int[] TotalTimesTourDestinationModeTimeModelRun { get; set; }

    public static int[] TotalTimesWorkTourModeModelRun { get; set; }

    public static int[] TotalTimesWorkTourTimeModelRun { get; set; }

    public static int[] TotalTimesSchoolTourModeModelRun { get; set; }

    public static int[] TotalTimesSchoolTourTimeModelRun { get; set; }

    public static int[] TotalTimesEscortTourModeModelRun { get; set; }

    public static int[] TotalTimesOtherHomeBasedTourModeModelRun { get; set; }

    public static int[] TotalTimesOtherHomeBasedTourTimeModelRun { get; set; }

    public static int[] TotalTimesWorkSubtourDestinationModelRun { get; set; }

    public static int[] TotalTimesBusinessSubtourDestinationModelRun { get; set; }

    public static int[] TotalTimesOtherSubtourDestinationModelRun { get; set; }

    public static int[] TotalTimesWorkBasedSubtourModeModelRun { get; set; }

    public static int[] TotalTimesWorkBasedSubtourTimeModelRun { get; set; }

    public static int[] TotalTimesTripModelSuiteRun { get; set; }

    public static int[] TotalTimesIntermediateStopGenerationModelRun { get; set; }

    public static int[] TotalTimesIntermediateStopGenerated { get; set; }

    public static int[] TotalTimesChangeModeStopGenerated { get; set; }

    public static int[] TotalTimesChangeModeLocationSet { get; set; }

    public static int[] TotalTimesChangeModeTransitModeSet { get; set; }

    public static int[] TotalTimesTripIsToTourOrigin { get; set; }

    public static int[] TotalTimesNextTripIsNull { get; set; }

    public static int[] TotalTimesIntermediateStopLocationModelRun { get; set; }

    public static int[] TotalTimesTripModeTimeModelRun { get; set; }

    public static int[] TotalTimesTripModeModelRun { get; set; }

    public static int[] TotalTimesTripTimeModelRun { get; set; }

    public static int[] TotalTimesActumPrimaryPriorityTimeModelRun { get; set; }

    public static int[] TotalTimesLDPrimaryPriorityTimeModelRun { get; set; }

    public static int[] TotalTimesHouseholdDayPatternTypeModelRun { get; set; }

    public static int[] TotalTimesPersonDayPatternTypeModelRun { get; set; }

    public static int[] TotalTimesWorkAtHomeModelRun { get; set; }

    public static int HouseholdFileRecordsWritten { get; set; }

    public static int PersonFileRecordsWritten { get; set; }

    public static int HouseholdDayFileRecordsWritten { get; set; }

    public static int PersonDayFileRecordsWritten { get; set; }

    public static int TourFileRecordsWritten { get; set; }

    public static int TripFileRecordsWritten { get; set; }

    public static int JointTourFileRecordsWritten { get; set; }

    public static int PartialHalfTourFileRecordsWritten { get; set; }

    public static int FullHalfTourFileRecordsWritten { get; set; }

    public static long HouseholdVehiclesOwnedCheckSum { get; set; }

    public static long PersonUsualWorkParcelCheckSum { get; set; }

    public static long PersonUsualSchoolParcelCheckSum { get; set; }

    public static long PersonTransitPassOwnershipCheckSum { get; set; }

    public static long PersonPaidParkingAtWorkCheckSum { get; set; }

    public static long PersonDayHomeBasedToursCheckSum { get; set; }

    public static long PersonDayWorkBasedToursCheckSum { get; set; }

    public static long TourMainDestinationPurposeCheckSum { get; set; }

    public static long TourMainDestinationParcelCheckSum { get; set; }

    public static long TourMainModeTypeCheckSum { get; set; }

    public static long TourOriginDepartureTimeCheckSum { get; set; }

    public static long TourDestinationArrivalTimeCheckSum { get; set; }

    public static long TourDestinationDepartureTimeCheckSum { get; set; }

    public static long TourOriginArrivalTimeCheckSum { get; set; }

    public static long TripHalfTourCheckSum { get; set; }

    public static long TripCheckSum { get; set; }

    public static long TripDestinationPurposeCheckSum { get; set; }

    public static long TripDestinationParcelCheckSum { get; set; }

    public static long TripModeCheckSum { get; set; }

    public static long TripPathTypeCheckSum { get; set; }

    public static long TripDepartureTimeCheckSum { get; set; }

    public static long TripArrivalTimeCheckSum { get; set; }


    public static ThreadQueue ThreadQueue { get; private set; }

    //public static ExporterFactory ExporterFactory { get; private set; }

    public static Dictionary<int, IParcelWrapper> Parcels { get; set; }

    //public static Dictionary<int, ZoneTotals> ZoneTotals { get; private set; }

    public static Dictionary<int, int> ZoneKeys { get; private set; }

    public static int SmallPeriodDuration { get; private set; }

    public static TDMTripListExporter TDMTripListExporter { get; private set; }

    public static int[] TotalPersonDays { get; set; }

    public static int[] TotalHouseholdDays { get; set; }

    public static int[] TotalInvalidAttempts { get; set; }

    public static ParkAndRideNodeDao ParkAndRideNodeDao { get; private set; }

    public static DestinationParkingNodeDao DestinationParkingNodeDao { get; private set; }


    public static void Initialize(string name, bool loadData = true) {
      FactoryHelper helper = new FactoryHelper(Global.Configuration);

      _type = helper.ChoiceModelRunner.GetChoiceModelRunnerType();

      if (!Global.Configuration.IsInEstimationMode || Global.Configuration.ShouldOutputStandardFilesInEstimationMode) {
        string threadQueueThreadName = Thread.CurrentThread.Name + "_" + _type.ToString();
        ThreadQueue = new ThreadQueue(threadQueueThreadName);
      }

      //ExporterFactory = Global.ContainerDaySim.GetInstance<ExporterFactory>();

      // e.g. 30 minutes between each minute span
      SmallPeriodDuration = DayPeriod.SmallDayPeriods.First().Duration;

      if (Global.Configuration.ShouldOutputTDMTripList & loadData) {
        TDMTripListExporter = new TDMTripListExporter(Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath), Global.Configuration.TDMTripListDelimiter);
      }

      if (loadData) {
        LoadData();
      }

      int numberOfChoiceModelThreads = Engine.GetNumberOfChoiceModelThreads();
      TotalTimesHouseholdModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesHouseholdDayModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesJointHalfTourGenerationModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesJointTourGenerationModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonDayMandatoryModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonDayModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesPartialJointHalfTourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesFullJointHalfTourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesMandatoryTourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesJointTourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesNonMandatoryTourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourTripModelsRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourSubtourModelsRun = new int[numberOfChoiceModelThreads];

      TotalTimesProcessHalfToursRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourSubtourModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesSubtourTripModelsRun = new int[numberOfChoiceModelThreads];

      TotalTimesProcessHalfSubtoursRun = new int[numberOfChoiceModelThreads];

      TotalTimesAutoOwnershipModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkLocationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesSchoolLocationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesPaidParkingAtWorkplaceModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkUsualModeAndScheduleModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTransitPassOwnershipModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesMandatoryTourGenerationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesMandatoryStopPresenceModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesJointHalfTourGenerationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesFullJointHalfTourParticipationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun = new int[numberOfChoiceModelThreads];

      TotalTimesJointTourGenerationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesJointTourParticipationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonDayPatternModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonTourGenerationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonExactNumberOfToursModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkTourDestinationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourDestinationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesOtherTourDestinationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkBasedSubtourGenerationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourDestinationModeTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTourModeTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkTourModeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkTourTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesSchoolTourModeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesSchoolTourTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesEscortTourModeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesOtherHomeBasedTourModeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesOtherHomeBasedTourTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkSubtourDestinationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesBusinessSubtourDestinationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesOtherSubtourDestinationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkBasedSubtourModeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkBasedSubtourTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTripModelSuiteRun = new int[numberOfChoiceModelThreads];

      TotalTimesIntermediateStopGenerationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesIntermediateStopGenerated = new int[numberOfChoiceModelThreads];

      TotalTimesChangeModeStopGenerated = new int[numberOfChoiceModelThreads];

      TotalTimesChangeModeLocationSet = new int[numberOfChoiceModelThreads];

      TotalTimesChangeModeTransitModeSet = new int[numberOfChoiceModelThreads];

      TotalTimesTripIsToTourOrigin = new int[numberOfChoiceModelThreads];

      TotalTimesNextTripIsNull = new int[numberOfChoiceModelThreads];

      TotalTimesIntermediateStopLocationModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTripModeTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTripModeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesTripTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesActumPrimaryPriorityTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesLDPrimaryPriorityTimeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesHouseholdDayPatternTypeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesPersonDayPatternTypeModelRun = new int[numberOfChoiceModelThreads];

      TotalTimesWorkAtHomeModelRun = new int[numberOfChoiceModelThreads];
      TotalPersonDays = new int[numberOfChoiceModelThreads];

      TotalHouseholdDays = new int[numberOfChoiceModelThreads];

      TotalInvalidAttempts = new int[numberOfChoiceModelThreads];
    }

    public static void LoadData() {
      Framework.DomainModels.Persisters.IPersisterReader<IParcel> parcelReader =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IParcel>>()
                    .Reader;

      IParcelCreator parcelCreator =
                Global
                    .ContainerDaySim
                    .GetInstance<IWrapperFactory<IParcelCreator>>()
                    .Creator;

      Parcels = new Dictionary<int, IParcelWrapper>(parcelReader.Count);

      Framework.DomainModels.Persisters.IPersisterReader<IZone> zoneReader =
                Global
                    .ContainerDaySim
                    .GetInstance<IPersistenceFactory<IZone>>()
                    .Reader;

      //            ZoneTotals = new Dictionary<int, ZoneTotals>(zoneReader.Count);
      ZoneKeys = new Dictionary<int, int>(zoneReader.Count);

      Dictionary<int, IZone> zones = new Dictionary<int, IZone>();

      foreach (IZone zone in zoneReader) {
        ZoneKeys.Add(zone.Id, zone.Key);
        zones.Add(zone.Id, zone);
      }

      Dictionary<int, Framework.ShadowPricing.IShadowPriceParcel> shadowPrices = ShadowPriceReader.ReadShadowPrices();

      foreach (IParcel parcel in parcelReader) {
        IParcelWrapper parcelWrapper = parcelCreator.CreateWrapper(parcel);

        Parcels.Add(parcel.Id, parcelWrapper);


        if (zones.TryGetValue(parcel.ZoneId, out IZone zone)) {
          parcelWrapper.District = zone.External;
        }

        parcelWrapper.SetShadowPricing(zones, shadowPrices);

        //                ZoneTotals zoneTotals;
        //
        //                if (!ZoneTotals.TryGetValue(parcel.ZoneId, out zoneTotals)) {
        //                    zoneTotals = new ZoneTotals();
        //
        //                    ZoneTotals.Add(parcel.ZoneId, zoneTotals);
        //                }
        //
        //                zoneTotals.SumTotals(parcel);
      }

      if (Global.DestinationParkingNodeIsEnabled) {
        DestinationParkingNodeDao = new DestinationParkingNodeDao();
      }
      if (Global.ParkAndRideNodeIsEnabled) {
        ParkAndRideNodeDao = new ParkAndRideNodeDao();
      }
    }

    public static void WriteCounters() {
      Global.PrintFile.WriteLine("Person day statistics:");

      Global.PrintFile.IncrementIndent();
      if (GetTotal(TotalHouseholdDays) > 0) {
        Global.PrintFile.WriteLine("TotalHouseholdDays = {0:n0} ", GetTotal(TotalHouseholdDays));
      } else {
        Global.PrintFile.WriteLine("TotalPersonDays = {0:n0} ", GetTotal(TotalPersonDays));
      }
      Global.PrintFile.WriteLine("TotalInvalidAttempts = {0:n0} ", GetTotal(TotalInvalidAttempts));
      Global.PrintFile.DecrementIndent();

      Global.PrintFile.WriteLine("Counters:");

      Global.PrintFile.IncrementIndent();
      Global.PrintFile.WriteLine("TotalTimesHouseholdModelSuiteRun = {0:n0} ", GetTotal(TotalTimesHouseholdModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesPersonModelSuiteRun = {0:n0} ", GetTotal(TotalTimesPersonModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesHouseholdDayModelSuiteRun = {0:n0} ", GetTotal(TotalTimesHouseholdDayModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesActumPriorityTimeModelRun = {0:n0} ", GetTotal(TotalTimesActumPrimaryPriorityTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesLDPriorityTimeModelRun = {0:n0} ", GetTotal(TotalTimesLDPrimaryPriorityTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesHouseholdDayPatternTypeModelRun = {0:n0} ", GetTotal(TotalTimesHouseholdDayPatternTypeModelRun));
      Global.PrintFile.WriteLine("TotalTimesPersonDayPatternTypeModelRun = {0:n0} ", GetTotal(TotalTimesPersonDayPatternTypeModelRun));
      Global.PrintFile.WriteLine("TotalTimesJointHalfTourGenerationModelSuiteRun = {0:n0} ", GetTotal(TotalTimesJointHalfTourGenerationModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesJointHalfTourGenerationModelRun = {0:n0} ", GetTotal(TotalTimesJointHalfTourGenerationModelRun));
      Global.PrintFile.WriteLine("TotalTimesFullJointHalfTourParticipationModelRun = {0:n0} ", GetTotal(TotalTimesFullJointHalfTourParticipationModelRun));
      Global.PrintFile.WriteLine("TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun = {0:n0} ", GetTotal(TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun));
      Global.PrintFile.WriteLine("TotalTimesJointTourGenerationModelSuiteRun = {0:n0} ", GetTotal(TotalTimesJointTourGenerationModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesJointTourGenerationModelRun = {0:n0} ", GetTotal(TotalTimesJointTourGenerationModelRun));
      Global.PrintFile.WriteLine("TotalTimesJointTourParticipationModelRun = {0:n0} ", GetTotal(TotalTimesJointTourParticipationModelRun));
      Global.PrintFile.WriteLine("TotalTimesPersonDayMandatoryModelSuiteRun = {0:n0} ", GetTotal(TotalTimesPersonDayMandatoryModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesPersonDayModelSuiteRun = {0:n0} ", GetTotal(TotalTimesPersonDayModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesPartialJointHalfTourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesPartialJointHalfTourModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesFullJointHalfTourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesFullJointHalfTourModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesMandatoryTourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesMandatoryTourModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesJointTourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesJointTourModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesNonMandatoryTourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesNonMandatoryTourModelSuiteRun));

      Global.PrintFile.WriteLine("TotalTimesTourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesTourModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesTourTripModelsRun = {0:n0} ", GetTotal(TotalTimesTourTripModelsRun));
      Global.PrintFile.WriteLine("TotalTimesTourSubtourModelsRun = {0:n0} ", GetTotal(TotalTimesTourSubtourModelsRun));
      Global.PrintFile.WriteLine("TotalTimesProcessHalfToursRun = {0:n0} ", GetTotal(TotalTimesProcessHalfToursRun));
      Global.PrintFile.WriteLine("TotalTimesTourSubtourModelSuiteRun = {0:n0} ", GetTotal(TotalTimesTourSubtourModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesSubtourTripModelsRun = {0:n0} ", GetTotal(TotalTimesSubtourTripModelsRun));
      Global.PrintFile.WriteLine("TotalTimesProcessHalfSubtoursRun = {0:n0} ", GetTotal(TotalTimesProcessHalfSubtoursRun));
      Global.PrintFile.WriteLine("TotalTimesAutoOwnershipModelRun = {0:n0} ", GetTotal(TotalTimesAutoOwnershipModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkLocationModelRun = {0:n0} ", GetTotal(TotalTimesWorkLocationModelRun));
      Global.PrintFile.WriteLine("TotalTimesSchoolLocationModelRun = {0:n0} ", GetTotal(TotalTimesSchoolLocationModelRun));
      Global.PrintFile.WriteLine("TotalTimesPaidParkingAtWorkplaceModelRun = {0:n0} ", GetTotal(TotalTimesPaidParkingAtWorkplaceModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkUsualModeAndScheduleModelRun = {0:n0} ", GetTotal(TotalTimesWorkUsualModeAndScheduleModelRun));
      Global.PrintFile.WriteLine("TotalTimesTransitPassOwnershipModelRun = {0:n0} ", GetTotal(TotalTimesTransitPassOwnershipModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkAtHomeModelRun = {0:n0} ", GetTotal(TotalTimesWorkAtHomeModelRun));
      Global.PrintFile.WriteLine("TotalTimesMandatoryTourGenerationModelRun = {0:n0} ", GetTotal(TotalTimesMandatoryTourGenerationModelRun));
      Global.PrintFile.WriteLine("TotalTimesMandatoryStopPresenceModelRun = {0:n0} ", GetTotal(TotalTimesMandatoryStopPresenceModelRun));
      Global.PrintFile.WriteLine("TotalTimesPersonDayPatternModelRun = {0:n0} ", GetTotal(TotalTimesPersonDayPatternModelRun));
      Global.PrintFile.WriteLine("TotalTimesPersonTourGenerationModelRun = {0:n0} ", GetTotal(TotalTimesPersonTourGenerationModelRun));
      Global.PrintFile.WriteLine("TotalTimesPersonExactNumberOfToursModelRun = {0:n0} ", GetTotal(TotalTimesPersonExactNumberOfToursModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkTourDestinationModelRun = {0:n0} ", GetTotal(TotalTimesWorkTourDestinationModelRun));
      Global.PrintFile.WriteLine("TotalTimesTourDestinationModelRun = {0:n0} ", GetTotal(TotalTimesTourDestinationModelRun));
      Global.PrintFile.WriteLine("TotalTimesOtherTourDestinationModelRun = {0:n0} ", GetTotal(TotalTimesOtherTourDestinationModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkBasedSubtourGenerationModelRun = {0:n0} ", GetTotal(TotalTimesWorkBasedSubtourGenerationModelRun));
      Global.PrintFile.WriteLine("TotalTimesTourDestinationModeTimeModelRun = {0:n0} ", GetTotal(TotalTimesTourDestinationModeTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesTourModeTimeModelRun = {0:n0} ", GetTotal(TotalTimesTourModeTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkTourModeModelRun = {0:n0} ", GetTotal(TotalTimesWorkTourModeModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkTourTimeModelRun = {0:n0} ", GetTotal(TotalTimesWorkTourTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesSchoolTourModeModelRun = {0:n0} ", GetTotal(TotalTimesSchoolTourModeModelRun));
      Global.PrintFile.WriteLine("TotalTimesSchoolTourTimeModelRun = {0:n0} ", GetTotal(TotalTimesSchoolTourTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesEscortTourModeModelRun = {0:n0} ", GetTotal(TotalTimesEscortTourModeModelRun));
      Global.PrintFile.WriteLine("TotalTimesOtherHomeBasedTourModeModelRun = {0:n0} ", GetTotal(TotalTimesOtherHomeBasedTourModeModelRun));
      Global.PrintFile.WriteLine("TotalTimesOtherHomeBasedTourTimeModelRun = {0:n0} ", GetTotal(TotalTimesOtherHomeBasedTourTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesWork(Sub)TourDestinationModelRun = {0:n0} ", GetTotal(TotalTimesWorkSubtourDestinationModelRun));
      Global.PrintFile.WriteLine("TotalTimesBusiness(Sub)TourDestinationModelRun = {0:n0} ", GetTotal(TotalTimesBusinessSubtourDestinationModelRun));
      Global.PrintFile.WriteLine("TotalTimesOther(Sub)TourDestinationModelRun = {0:n0} ", GetTotal(TotalTimesOtherSubtourDestinationModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkBasedSubtourModeModelRun = {0:n0} ", GetTotal(TotalTimesWorkBasedSubtourModeModelRun));
      Global.PrintFile.WriteLine("TotalTimesWorkBasedSubtourTimeModelRun = {0:n0} ", GetTotal(TotalTimesWorkBasedSubtourTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesTripModelSuiteRun = {0:n0} ", GetTotal(TotalTimesTripModelSuiteRun));
      Global.PrintFile.WriteLine("TotalTimesIntermediateStopGenerationModelRun = {0:n0} ", GetTotal(TotalTimesIntermediateStopGenerationModelRun));
      Global.PrintFile.WriteLine("TotalTimesIntermediateStopGenerated = {0:n0} ", GetTotal(TotalTimesIntermediateStopGenerated));
      Global.PrintFile.WriteLine("TotalTimesChangeModeStopGenerated = {0:n0} ", GetTotal(TotalTimesChangeModeStopGenerated));
      Global.PrintFile.WriteLine("TotalTimesChangeModeLocationSet = {0:n0} ", GetTotal(TotalTimesChangeModeLocationSet));
      Global.PrintFile.WriteLine("TotalTimesChangeModeTransitModeSet = {0:n0} ", GetTotal(TotalTimesChangeModeTransitModeSet));
      Global.PrintFile.WriteLine("TotalTimesTripIsToTourOrigin = {0:n0} ", GetTotal(TotalTimesTripIsToTourOrigin));
      Global.PrintFile.WriteLine("TotalTimesNextTripIsNull = {0:n0} ", GetTotal(TotalTimesNextTripIsNull));
      Global.PrintFile.WriteLine("TotalTimesIntermediateStopLocationModelRun = {0:n0} ", GetTotal(TotalTimesIntermediateStopLocationModelRun));
      Global.PrintFile.WriteLine("TotalTimesTripModeModelRun = {0:n0} ", GetTotal(TotalTimesTripModeModelRun));
      Global.PrintFile.WriteLine("TotalTimesTripTimeModelRun = {0:n0} ", GetTotal(TotalTimesTripTimeModelRun));
      Global.PrintFile.WriteLine("TotalTimesTripModeTimeModelRun = {0:n0} ", GetTotal(TotalTimesTripModeTimeModelRun));
      Global.PrintFile.WriteLine();
      Global.PrintFile.WriteLine("HouseholdFileRecordsWritten      = {0:n0} ", GetTotal(HouseholdFileRecordsWritten));
      Global.PrintFile.WriteLine("PersonFileRecordsWritten         = {0:n0} ", GetTotal(PersonFileRecordsWritten));
      Global.PrintFile.WriteLine("HouseholdDayFileRecordsWritten   = {0:n0} ", GetTotal(HouseholdDayFileRecordsWritten));
      Global.PrintFile.WriteLine("PersonDayFileRecordsWritten      = {0:n0} ", GetTotal(PersonDayFileRecordsWritten));
      Global.PrintFile.WriteLine("TourFileRecordsWritten           = {0:n0} ", GetTotal(TourFileRecordsWritten));
      Global.PrintFile.WriteLine("TripFileRecordsWritten           = {0:n0} ", GetTotal(TripFileRecordsWritten));
      Global.PrintFile.WriteLine("JointTourFileRecordsWritten      = {0:n0} ", GetTotal(JointTourFileRecordsWritten));
      Global.PrintFile.WriteLine("PartialHalfTourFileRecordsWritten= {0:n0} ", GetTotal(PartialHalfTourFileRecordsWritten));
      Global.PrintFile.WriteLine("FullHalfTourFileRecordsWritten   = {0:n0} ", GetTotal(FullHalfTourFileRecordsWritten));
      Global.PrintFile.WriteLine();
      Global.PrintFile.WriteLine("HouseholdVehiclesOwnedCheckSum      = {0:n0} ", GetTotal(HouseholdVehiclesOwnedCheckSum));
      Global.PrintFile.WriteLine("PersonUsualWorkParcelCheckSum       = {0:n0} ", GetTotal(PersonUsualWorkParcelCheckSum));
      Global.PrintFile.WriteLine("PersonUsualSchoolParcelCheckSum     = {0:n0} ", GetTotal(PersonUsualSchoolParcelCheckSum));
      Global.PrintFile.WriteLine("PersonTransitPassOwnershipCheckSum  = {0:n0} ", GetTotal(PersonTransitPassOwnershipCheckSum));
      Global.PrintFile.WriteLine("PersonPaidParkingAtWorkCheckSum     = {0:n0} ", GetTotal(PersonPaidParkingAtWorkCheckSum));
      Global.PrintFile.WriteLine("PersonDayHomeBasedToursCheckSum     = {0:n0} ", GetTotal(PersonDayHomeBasedToursCheckSum));
      Global.PrintFile.WriteLine("PersonDayWorkBasedToursCheckSum     = {0:n0} ", GetTotal(PersonDayWorkBasedToursCheckSum));
      Global.PrintFile.WriteLine("TourMainDestinationPurposeCheckSum  = {0:n0} ", GetTotal(TourMainDestinationPurposeCheckSum));
      Global.PrintFile.WriteLine("TourMainDestinationParcelCheckSum   = {0:n0} ", GetTotal(TourMainDestinationParcelCheckSum));
      Global.PrintFile.WriteLine("TourMainModeTypeCheckSum            = {0:n0} ", GetTotal(TourMainModeTypeCheckSum));
      Global.PrintFile.WriteLine("TourOriginDepartureTimeCheckSum     = {0:n0} ", GetTotal(TourOriginDepartureTimeCheckSum));
      Global.PrintFile.WriteLine("TourDestinationArrivalTimeCheckSum  = {0:n0} ", GetTotal(TourDestinationArrivalTimeCheckSum));
      Global.PrintFile.WriteLine("TourDestinationDepartureTimeCheckSum= {0:n0} ", GetTotal(TourDestinationDepartureTimeCheckSum));
      Global.PrintFile.WriteLine("TourOriginArrivalTimeCheckSum       = {0:n0} ", GetTotal(TourOriginArrivalTimeCheckSum));
      Global.PrintFile.WriteLine("TripHalfTourCheckSum                = {0:n0} ", GetTotal(TripHalfTourCheckSum));
      Global.PrintFile.WriteLine("TripDestinationPurposeCheckSum      = {0:n0} ", GetTotal(TripDestinationPurposeCheckSum));
      Global.PrintFile.WriteLine("TripDestinationParcelCheckSum       = {0:n0} ", GetTotal(TripDestinationParcelCheckSum));
      Global.PrintFile.WriteLine("TripModeCheckSum                    = {0:n0} ", GetTotal(TripModeCheckSum));
      Global.PrintFile.WriteLine("TripPathTypeCheckSum                = {0:n0} ", GetTotal(TripPathTypeCheckSum));
      Global.PrintFile.WriteLine("TripDepartureTimeCheckSum           = {0:n0} ", GetTotal(TripDepartureTimeCheckSum));
      Global.PrintFile.WriteLine("TripArrivalTimeCheckSum             = {0:n0} ", GetTotal(TripArrivalTimeCheckSum));
      Global.PrintFile.DecrementIndent();
      Global.PrintFile.WriteLine();
      Global.PrintFile.WriteLine("Run completed at ", DateTime.Now.ToString(CultureInfo.InvariantCulture));
    }

    public static void SignalShutdown() {
      if (ThreadQueue == null) {
        return;
      }

      ThreadQueue.Shutdown();

      HouseholdWrapper.Close();
      PersonWrapper.Close();
      HouseholdDayWrapper.Close();
      PersonDayWrapper.Close();
      TourWrapper.Close();
      TripWrapper.Close();
      if (!string.IsNullOrEmpty(Global.Configuration.OutputJointTourPath)) {
        JointTourWrapper.Close();
      }

      if (!string.IsNullOrEmpty(Global.Configuration.OutputFullHalfTourPath)) {
        FullHalfTourWrapper.Close();
      }

      if (!string.IsNullOrEmpty(Global.Configuration.OutputPartialHalfTourPath)) {
        PartialHalfTourWrapper.Close();
      }
    }

    public static IChoiceModelRunner Get(IHousehold household, int randomSeed) {
      IChoiceModelRunner runner = (IChoiceModelRunner)Activator.CreateInstance(_type, household);
      runner.SetRandomSeed(randomSeed);
      return runner;
    }

    public static int GetTotal(int[] p) {
      int total = 0;
      for (int x = 0; x < p.Count(); x++) {
        total += p[x];
      }
      return total;
    }

    public static long GetTotal(long p) {
      return p;
    }
  }
}
