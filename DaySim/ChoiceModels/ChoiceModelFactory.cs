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
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;
using DaySim.ShadowPricing;
using Ninject;

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

		public static Int64 HouseholdVehiclesOwnedCheckSum { get; set; }

		public static Int64 PersonUsualWorkParcelCheckSum { get; set; }

  	public static Int64 PersonUsualSchoolParcelCheckSum { get; set; }

		public static Int64 PersonTransitPassOwnershipCheckSum { get; set; }

  	public static Int64 PersonPaidParkingAtWorkCheckSum { get; set; }

  	public static Int64 PersonDayHomeBasedToursCheckSum { get; set; }

  	public static Int64 PersonDayWorkBasedToursCheckSum { get; set; }

  	public static Int64 TourMainDestinationPurposeCheckSum { get; set; }

  	public static Int64 TourMainDestinationParcelCheckSum { get; set; }

  	public static Int64 TourMainModeTypeCheckSum { get; set; }

  	public static Int64 TourOriginDepartureTimeCheckSum { get; set; }

  	public static Int64 TourDestinationArrivalTimeCheckSum { get; set; }

  	public static Int64 TourDestinationDepartureTimeCheckSum { get; set; }

  	public static Int64 TourOriginArrivalTimeCheckSum { get; set; }

  	public static Int64 TripHalfTourCheckSum { get; set; }

		public static Int64 TripCheckSum { get; set; }

		public static Int64 TripDestinationPurposeCheckSum { get; set; }

		public static Int64 TripDestinationParcelCheckSum { get; set; }

		public static Int64 TripModeCheckSum { get; set; }

		public static Int64 TripPathTypeCheckSum { get; set; }

		public static Int64 TripDepartureTimeCheckSum { get; set; }

		public static Int64 TripArrivalTimeCheckSum { get; set; }


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


		public static void Initialize(string name, int nBatches, bool loadData = true) {
			var helper = new FactoryHelper(Global.Configuration);

			_type = helper.ChoiceModelRunner.GetChoiceModelRunnerType();
			
			if (!Global.Configuration.IsInEstimationMode || Global.Configuration.ShouldOutputStandardFilesInEstimationMode) {
				ThreadQueue = new ThreadQueue();
			}

			//ExporterFactory = Global.Kernel.Get<ExporterFactory>();

			// e.g. 30 minutes between each minute span
			SmallPeriodDuration = DayPeriod.SmallDayPeriods.First().Duration;

			if (Global.Configuration.ShouldOutputTDMTripList & loadData) {
				TDMTripListExporter = new TDMTripListExporter(Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath), Global.Configuration.TDMTripListDelimiter);
			}

			if (loadData) {
				LoadData();
			}


		TotalTimesHouseholdModelSuiteRun = new int[nBatches];

		TotalTimesPersonModelSuiteRun = new int[nBatches];
		
		TotalTimesHouseholdDayModelSuiteRun = new int[nBatches];

		TotalTimesJointHalfTourGenerationModelSuiteRun = new int[nBatches];

		TotalTimesJointTourGenerationModelSuiteRun = new int[nBatches];

		TotalTimesPersonDayMandatoryModelSuiteRun = new int[nBatches];
		
		TotalTimesPersonDayModelSuiteRun = new int[nBatches];
		
		TotalTimesPartialJointHalfTourModelSuiteRun = new int[nBatches];

		TotalTimesFullJointHalfTourModelSuiteRun = new int[nBatches];

		TotalTimesMandatoryTourModelSuiteRun = new int[nBatches];
		
		TotalTimesJointTourModelSuiteRun = new int[nBatches];

		TotalTimesNonMandatoryTourModelSuiteRun = new int[nBatches];
		
		TotalTimesTourModelSuiteRun = new int[nBatches];
		
		TotalTimesTourTripModelsRun = new int[nBatches];
		
		TotalTimesTourSubtourModelsRun = new int[nBatches];
		
		TotalTimesProcessHalfToursRun = new int[nBatches];
		
		TotalTimesTourSubtourModelSuiteRun = new int[nBatches];
		
		TotalTimesSubtourTripModelsRun = new int[nBatches];
		
		TotalTimesProcessHalfSubtoursRun = new int[nBatches];
		
		TotalTimesAutoOwnershipModelRun = new int[nBatches];
		
		TotalTimesWorkLocationModelRun = new int[nBatches];
		
		TotalTimesSchoolLocationModelRun = new int[nBatches];
		
		TotalTimesPaidParkingAtWorkplaceModelRun = new int[nBatches];
		
		TotalTimesWorkUsualModeAndScheduleModelRun = new int[nBatches];

		TotalTimesTransitPassOwnershipModelRun = new int[nBatches];

		TotalTimesMandatoryTourGenerationModelRun = new int[nBatches];

		TotalTimesMandatoryStopPresenceModelRun = new int[nBatches];

		TotalTimesJointHalfTourGenerationModelRun = new int[nBatches];
		
		TotalTimesFullJointHalfTourParticipationModelRun = new int[nBatches];
		
		TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun = new int[nBatches];

		TotalTimesJointTourGenerationModelRun = new int[nBatches];

		TotalTimesJointTourParticipationModelRun = new int[nBatches];

		TotalTimesPersonDayPatternModelRun = new int[nBatches];

		TotalTimesPersonTourGenerationModelRun = new int[nBatches];
		
		TotalTimesPersonExactNumberOfToursModelRun = new int[nBatches];
		
		TotalTimesWorkTourDestinationModelRun = new int[nBatches];
		
		TotalTimesTourDestinationModelRun = new int[nBatches];
		
		TotalTimesOtherTourDestinationModelRun = new int[nBatches];
		
		TotalTimesWorkBasedSubtourGenerationModelRun = new int[nBatches];
		
		TotalTimesTourDestinationModeTimeModelRun = new int[nBatches];

		TotalTimesTourModeTimeModelRun = new int[nBatches];

		TotalTimesWorkTourModeModelRun = new int[nBatches];
		
		TotalTimesWorkTourTimeModelRun = new int[nBatches];
		
		TotalTimesSchoolTourModeModelRun = new int[nBatches];
		
		TotalTimesSchoolTourTimeModelRun = new int[nBatches];
		
		TotalTimesEscortTourModeModelRun = new int[nBatches];
		
		TotalTimesOtherHomeBasedTourModeModelRun = new int[nBatches];
		
		TotalTimesOtherHomeBasedTourTimeModelRun = new int[nBatches];
		
		TotalTimesWorkSubtourDestinationModelRun = new int[nBatches];
		
		TotalTimesBusinessSubtourDestinationModelRun = new int[nBatches];
		
		TotalTimesOtherSubtourDestinationModelRun = new int[nBatches];
		
		TotalTimesWorkBasedSubtourModeModelRun = new int[nBatches];
		
		TotalTimesWorkBasedSubtourTimeModelRun = new int[nBatches];
		
		TotalTimesTripModelSuiteRun = new int[nBatches];
		
		TotalTimesIntermediateStopGenerationModelRun = new int[nBatches];
		
		TotalTimesIntermediateStopGenerated = new int[nBatches];
		
		TotalTimesChangeModeStopGenerated = new int[nBatches];
		
		TotalTimesChangeModeLocationSet = new int[nBatches];
		
		TotalTimesChangeModeTransitModeSet = new int[nBatches];
		
		TotalTimesTripIsToTourOrigin = new int[nBatches];
		
		TotalTimesNextTripIsNull = new int[nBatches];
		
		TotalTimesIntermediateStopLocationModelRun = new int[nBatches];

    TotalTimesTripModeTimeModelRun = new int[nBatches];

    TotalTimesTripModeModelRun = new int[nBatches];
		
		TotalTimesTripTimeModelRun = new int[nBatches];

		TotalTimesActumPrimaryPriorityTimeModelRun = new int[nBatches];

		TotalTimesLDPrimaryPriorityTimeModelRun = new int[nBatches];

		TotalTimesHouseholdDayPatternTypeModelRun = new int[nBatches];

		TotalTimesPersonDayPatternTypeModelRun = new int[nBatches];

		TotalTimesWorkAtHomeModelRun = new int[nBatches];
		TotalPersonDays = new int[nBatches];

		TotalHouseholdDays = new int[nBatches];

		TotalInvalidAttempts = new int[nBatches];
		}

		public static void LoadData() {
			var parcelReader =
				Global
					.Kernel
					.Get<IPersistenceFactory<IParcel>>()
					.Reader;

			var parcelCreator =
				Global
					.Kernel
					.Get<IWrapperFactory<IParcelCreator>>()
					.Creator;

			Parcels = new Dictionary<int, IParcelWrapper>(parcelReader.Count);

			var zoneReader =
				Global
					.Kernel
					.Get<IPersistenceFactory<IZone>>()
					.Reader;

//			ZoneTotals = new Dictionary<int, ZoneTotals>(zoneReader.Count);
			ZoneKeys = new Dictionary<int, int>(zoneReader.Count);

			var zones = new Dictionary<int, IZone>();

			foreach (var zone in zoneReader) {
				ZoneKeys.Add(zone.Id, zone.Key);
				zones.Add(zone.Id, zone);
			}

			var shadowPrices = ShadowPriceReader.ReadShadowPrices();

			foreach (var parcel in parcelReader) {
				var parcelWrapper = parcelCreator.CreateWrapper(parcel);

				Parcels.Add(parcel.Id, parcelWrapper);

				IZone zone;

				if (zones.TryGetValue(parcel.ZoneId, out zone)) {
					parcelWrapper.District = zone.External;
				}

				parcelWrapper.SetShadowPricing(zones, shadowPrices);

//				ZoneTotals zoneTotals;
//
//				if (!ZoneTotals.TryGetValue(parcel.ZoneId, out zoneTotals)) {
//					zoneTotals = new ZoneTotals();
//
//					ZoneTotals.Add(parcel.ZoneId, zoneTotals);
//				}
//
//				zoneTotals.SumTotals(parcel);
			}

			if (Global.ParkAndRideNodeIsEnabled) {
				ParkAndRideNodeDao = new ParkAndRideNodeDao();
			}
		}

		public static void WriteCounters() {
			Global.PrintFile.WriteLine("Person day statistics:");

			Global.PrintFile.IncrementIndent();
			if (GetTotal(TotalHouseholdDays) > 0) {
				Global.PrintFile.WriteLine("TotalHouseholdDays = {0} ", GetTotal(TotalHouseholdDays));
			}
			else {
				Global.PrintFile.WriteLine("TotalPersonDays = {0} ",  GetTotal(TotalPersonDays));
			}
			Global.PrintFile.WriteLine("TotalInvalidAttempts = {0} ",  GetTotal(TotalInvalidAttempts));
			Global.PrintFile.DecrementIndent();

			Global.PrintFile.WriteLine("Counters:");

			Global.PrintFile.IncrementIndent();
			Global.PrintFile.WriteLine("TotalTimesHouseholdModelSuiteRun = {0} ",  GetTotal(TotalTimesHouseholdModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesPersonModelSuiteRun = {0} ",  GetTotal(TotalTimesPersonModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesHouseholdDayModelSuiteRun = {0} ",  GetTotal(TotalTimesHouseholdDayModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesActumPriorityTimeModelRun = {0} ",  GetTotal(TotalTimesActumPrimaryPriorityTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesLDPriorityTimeModelRun = {0} ",  GetTotal(TotalTimesLDPrimaryPriorityTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesHouseholdDayPatternTypeModelRun = {0} ",  GetTotal(TotalTimesHouseholdDayPatternTypeModelRun));
			Global.PrintFile.WriteLine("TotalTimesPersonDayPatternTypeModelRun = {0} ",  GetTotal(TotalTimesPersonDayPatternTypeModelRun));
			Global.PrintFile.WriteLine("TotalTimesJointHalfTourGenerationModelSuiteRun = {0} ",  GetTotal(TotalTimesJointHalfTourGenerationModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesJointHalfTourGenerationModelRun = {0} ",  GetTotal(TotalTimesJointHalfTourGenerationModelRun));
			Global.PrintFile.WriteLine("TotalTimesFullJointHalfTourParticipationModelRun = {0} ",  GetTotal(TotalTimesFullJointHalfTourParticipationModelRun));
			Global.PrintFile.WriteLine("TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun = {0} ",  GetTotal(TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun));
			Global.PrintFile.WriteLine("TotalTimesJointTourGenerationModelSuiteRun = {0} ",  GetTotal(TotalTimesJointTourGenerationModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesJointTourGenerationModelRun = {0} ",  GetTotal(TotalTimesJointTourGenerationModelRun));
			Global.PrintFile.WriteLine("TotalTimesJointTourParticipationModelRun = {0} ",  GetTotal(TotalTimesJointTourParticipationModelRun));
			Global.PrintFile.WriteLine("TotalTimesPersonDayMandatoryModelSuiteRun = {0} ",  GetTotal(TotalTimesPersonDayMandatoryModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesPersonDayModelSuiteRun = {0} ",  GetTotal(TotalTimesPersonDayModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesPartialJointHalfTourModelSuiteRun = {0} ",  GetTotal(TotalTimesPartialJointHalfTourModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesFullJointHalfTourModelSuiteRun = {0} ",  GetTotal(TotalTimesFullJointHalfTourModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesMandatoryTourModelSuiteRun = {0} ",  GetTotal(TotalTimesMandatoryTourModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesJointTourModelSuiteRun = {0} ",  GetTotal(TotalTimesJointTourModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesNonMandatoryTourModelSuiteRun = {0} ",  GetTotal(TotalTimesNonMandatoryTourModelSuiteRun));
		
			Global.PrintFile.WriteLine("TotalTimesTourModelSuiteRun = {0} ", GetTotal(TotalTimesTourModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesTourTripModelsRun = {0} ", GetTotal(TotalTimesTourTripModelsRun));
			Global.PrintFile.WriteLine("TotalTimesTourSubtourModelsRun = {0} ", GetTotal(TotalTimesTourSubtourModelsRun));
			Global.PrintFile.WriteLine("TotalTimesProcessHalfToursRun = {0} ", GetTotal(TotalTimesProcessHalfToursRun));
			Global.PrintFile.WriteLine("TotalTimesTourSubtourModelSuiteRun = {0} ", GetTotal(TotalTimesTourSubtourModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesSubtourTripModelsRun = {0} ", GetTotal(TotalTimesSubtourTripModelsRun));
			Global.PrintFile.WriteLine("TotalTimesProcessHalfSubtoursRun = {0} ", GetTotal(TotalTimesProcessHalfSubtoursRun));
			Global.PrintFile.WriteLine("TotalTimesAutoOwnershipModelRun = {0} ", GetTotal(TotalTimesAutoOwnershipModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkLocationModelRun = {0} ", GetTotal(TotalTimesWorkLocationModelRun));
			Global.PrintFile.WriteLine("TotalTimesSchoolLocationModelRun = {0} ", GetTotal(TotalTimesSchoolLocationModelRun));
			Global.PrintFile.WriteLine("TotalTimesPaidParkingAtWorkplaceModelRun = {0} ", GetTotal(TotalTimesPaidParkingAtWorkplaceModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkUsualModeAndScheduleModelRun = {0} ", GetTotal(TotalTimesWorkUsualModeAndScheduleModelRun));
			Global.PrintFile.WriteLine("TotalTimesTransitPassOwnershipModelRun = {0} ", GetTotal(TotalTimesTransitPassOwnershipModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkAtHomeModelRun = {0} ", GetTotal(TotalTimesWorkAtHomeModelRun));
			Global.PrintFile.WriteLine("TotalTimesMandatoryTourGenerationModelRun = {0} ", GetTotal(TotalTimesMandatoryTourGenerationModelRun));
			Global.PrintFile.WriteLine("TotalTimesMandatoryStopPresenceModelRun = {0} ", GetTotal(TotalTimesMandatoryStopPresenceModelRun));
			Global.PrintFile.WriteLine("TotalTimesPersonDayPatternModelRun = {0} ", GetTotal(TotalTimesPersonDayPatternModelRun));
			Global.PrintFile.WriteLine("TotalTimesPersonTourGenerationModelRun = {0} ", GetTotal(TotalTimesPersonTourGenerationModelRun));
			Global.PrintFile.WriteLine("TotalTimesPersonExactNumberOfToursModelRun = {0} ", GetTotal(TotalTimesPersonExactNumberOfToursModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkTourDestinationModelRun = {0} ", GetTotal(TotalTimesWorkTourDestinationModelRun));
			Global.PrintFile.WriteLine("TotalTimesTourDestinationModelRun = {0} ", GetTotal(TotalTimesTourDestinationModelRun));
			Global.PrintFile.WriteLine("TotalTimesOtherTourDestinationModelRun = {0} ", GetTotal(TotalTimesOtherTourDestinationModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkBasedSubtourGenerationModelRun = {0} ", GetTotal(TotalTimesWorkBasedSubtourGenerationModelRun));
			Global.PrintFile.WriteLine("TotalTimesTourDestinationModeTimeModelRun = {0} ", GetTotal(TotalTimesTourDestinationModeTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesTourModeTimeModelRun = {0} ", GetTotal(TotalTimesTourModeTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkTourModeModelRun = {0} ", GetTotal(TotalTimesWorkTourModeModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkTourTimeModelRun = {0} ", GetTotal(TotalTimesWorkTourTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesSchoolTourModeModelRun = {0} ", GetTotal(TotalTimesSchoolTourModeModelRun));
			Global.PrintFile.WriteLine("TotalTimesSchoolTourTimeModelRun = {0} ", GetTotal(TotalTimesSchoolTourTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesEscortTourModeModelRun = {0} ", GetTotal(TotalTimesEscortTourModeModelRun));
			Global.PrintFile.WriteLine("TotalTimesOtherHomeBasedTourModeModelRun = {0} ", GetTotal(TotalTimesOtherHomeBasedTourModeModelRun));
			Global.PrintFile.WriteLine("TotalTimesOtherHomeBasedTourTimeModelRun = {0} ", GetTotal(TotalTimesOtherHomeBasedTourTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesWork(Sub)TourDestinationModelRun = {0} ", GetTotal(TotalTimesWorkSubtourDestinationModelRun));
			Global.PrintFile.WriteLine("TotalTimesBusiness(Sub)TourDestinationModelRun = {0} ", GetTotal(TotalTimesBusinessSubtourDestinationModelRun));
			Global.PrintFile.WriteLine("TotalTimesOther(Sub)TourDestinationModelRun = {0} ", GetTotal(TotalTimesOtherSubtourDestinationModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkBasedSubtourModeModelRun = {0} ", GetTotal(TotalTimesWorkBasedSubtourModeModelRun));
			Global.PrintFile.WriteLine("TotalTimesWorkBasedSubtourTimeModelRun = {0} ", GetTotal(TotalTimesWorkBasedSubtourTimeModelRun));
			Global.PrintFile.WriteLine("TotalTimesTripModelSuiteRun = {0} ", GetTotal(TotalTimesTripModelSuiteRun));
			Global.PrintFile.WriteLine("TotalTimesIntermediateStopGenerationModelRun = {0} ", GetTotal(TotalTimesIntermediateStopGenerationModelRun));
			Global.PrintFile.WriteLine("TotalTimesIntermediateStopGenerated = {0} ", GetTotal(TotalTimesIntermediateStopGenerated));
			Global.PrintFile.WriteLine("TotalTimesChangeModeStopGenerated = {0} ", GetTotal(TotalTimesChangeModeStopGenerated));
			Global.PrintFile.WriteLine("TotalTimesChangeModeLocationSet = {0} ", GetTotal(TotalTimesChangeModeLocationSet));
			Global.PrintFile.WriteLine("TotalTimesChangeModeTransitModeSet = {0} ", GetTotal(TotalTimesChangeModeTransitModeSet));
			Global.PrintFile.WriteLine("TotalTimesTripIsToTourOrigin = {0} ", GetTotal(TotalTimesTripIsToTourOrigin));
			Global.PrintFile.WriteLine("TotalTimesNextTripIsNull = {0} ", GetTotal(TotalTimesNextTripIsNull));
			Global.PrintFile.WriteLine("TotalTimesIntermediateStopLocationModelRun = {0} ", GetTotal(TotalTimesIntermediateStopLocationModelRun));
			Global.PrintFile.WriteLine("TotalTimesTripModeModelRun = {0} ", GetTotal(TotalTimesTripModeModelRun));
			Global.PrintFile.WriteLine("TotalTimesTripTimeModelRun = {0} ", GetTotal(TotalTimesTripTimeModelRun));
            Global.PrintFile.WriteLine("TotalTimesTripModeTimeModelRun = {0} ", GetTotal(TotalTimesTripModeTimeModelRun));
            Global.PrintFile.WriteLine();
			Global.PrintFile.WriteLine("HouseholdFileRecordsWritten      = {0} ", GetTotal(HouseholdFileRecordsWritten));
			Global.PrintFile.WriteLine("PersonFileRecordsWritten         = {0} ", GetTotal(PersonFileRecordsWritten));
			Global.PrintFile.WriteLine("HouseholdDayFileRecordsWritten   = {0} ", GetTotal(HouseholdDayFileRecordsWritten));
			Global.PrintFile.WriteLine("PersonDayFileRecordsWritten      = {0} ", GetTotal(PersonDayFileRecordsWritten));
			Global.PrintFile.WriteLine("TourFileRecordsWritten           = {0} ", GetTotal(TourFileRecordsWritten));
			Global.PrintFile.WriteLine("TripFileRecordsWritten           = {0} ", GetTotal(TripFileRecordsWritten));
			Global.PrintFile.WriteLine("JointTourFileRecordsWritten      = {0} ", GetTotal(JointTourFileRecordsWritten));
			Global.PrintFile.WriteLine("PartialHalfTourFileRecordsWritten= {0} ", GetTotal(PartialHalfTourFileRecordsWritten));
			Global.PrintFile.WriteLine("FullHalfTourFileRecordsWritten   = {0} ", GetTotal(FullHalfTourFileRecordsWritten));
			Global.PrintFile.WriteLine();
			Global.PrintFile.WriteLine("HouseholdVehiclesOwnedCheckSum      = {0} ", GetTotal(HouseholdVehiclesOwnedCheckSum));
			Global.PrintFile.WriteLine("PersonUsualWorkParcelCheckSum       = {0} ", GetTotal(PersonUsualWorkParcelCheckSum));
			Global.PrintFile.WriteLine("PersonUsualSchoolParcelCheckSum     = {0} ", GetTotal(PersonUsualSchoolParcelCheckSum));
			Global.PrintFile.WriteLine("PersonTransitPassOwnershipCheckSum  = {0} ", GetTotal(PersonTransitPassOwnershipCheckSum));
			Global.PrintFile.WriteLine("PersonPaidParkingAtWorkCheckSum     = {0} ", GetTotal(PersonPaidParkingAtWorkCheckSum));
			Global.PrintFile.WriteLine("PersonDayHomeBasedToursCheckSum     = {0} ", GetTotal(PersonDayHomeBasedToursCheckSum));
			Global.PrintFile.WriteLine("PersonDayWorkBasedToursCheckSum     = {0} ", GetTotal(PersonDayWorkBasedToursCheckSum));
			Global.PrintFile.WriteLine("TourMainDestinationPurposeCheckSum  = {0} ", GetTotal(TourMainDestinationPurposeCheckSum));
			Global.PrintFile.WriteLine("TourMainDestinationParcelCheckSum   = {0} ", GetTotal(TourMainDestinationParcelCheckSum));
			Global.PrintFile.WriteLine("TourMainModeTypeCheckSum            = {0} ", GetTotal(TourMainModeTypeCheckSum));
			Global.PrintFile.WriteLine("TourOriginDepartureTimeCheckSum     = {0} ", GetTotal(TourOriginDepartureTimeCheckSum));
			Global.PrintFile.WriteLine("TourDestinationArrivalTimeCheckSum  = {0} ", GetTotal(TourDestinationArrivalTimeCheckSum));
			Global.PrintFile.WriteLine("TourDestinationDepartureTimeCheckSum= {0} ", GetTotal(TourDestinationDepartureTimeCheckSum));
			Global.PrintFile.WriteLine("TourOriginArrivalTimeCheckSum       = {0} ", GetTotal(TourOriginArrivalTimeCheckSum));
			Global.PrintFile.WriteLine("TripHalfTourCheckSum                = {0} ", GetTotal(TripHalfTourCheckSum));
			Global.PrintFile.WriteLine("TripDestinationPurposeCheckSum      = {0} ", GetTotal(TripDestinationPurposeCheckSum));
			Global.PrintFile.WriteLine("TripDestinationParcelCheckSum       = {0} ", GetTotal(TripDestinationParcelCheckSum));
			Global.PrintFile.WriteLine("TripModeCheckSum                    = {0} ", GetTotal(TripModeCheckSum));
			Global.PrintFile.WriteLine("TripPathTypeCheckSum                = {0} ", GetTotal(TripPathTypeCheckSum));
			Global.PrintFile.WriteLine("TripDepartureTimeCheckSum           = {0} ", GetTotal(TripDepartureTimeCheckSum));
			Global.PrintFile.WriteLine("TripArrivalTimeCheckSum             = {0} ", GetTotal(TripArrivalTimeCheckSum));
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
			if (!string.IsNullOrEmpty(Global.Configuration.OutputJointTourPath )) JointTourWrapper.Close();
			if (!string.IsNullOrEmpty(Global.Configuration.OutputFullHalfTourPath )) FullHalfTourWrapper.Close();
			if (!string.IsNullOrEmpty(Global.Configuration.OutputPartialHalfTourPath )) PartialHalfTourWrapper.Close();
		}

		public static IChoiceModelRunner Get(IHousehold household, int randomSeed) {
			var runner = (IChoiceModelRunner) Activator.CreateInstance(_type, household);
			runner.SetRandomSeed(randomSeed);
			return runner;
		}

		public static int GetTotal(int[] p)
		{
			int total = 0;
			for (int x = 0; x < p.Count(); x++)
			{
				total += p[x];
			}
			return total;
		}

		public static long GetTotal(long p)
		{
			return p;
		}
	}
}