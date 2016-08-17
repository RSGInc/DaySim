// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.Factories;
using DaySim.Framework.Sampling;
using Ninject;

namespace DaySim.Framework.Core {
	public static class Global {

        private const string SHADOW_PRICES_FILENAME = "shadow_prices.txt";
        private const string ARCHIVE_SHADOW_PRICES_FILENAME = "archive_" + SHADOW_PRICES_FILENAME;
        private const string PARK_AND_RIDE_SHADOW_PRICES_FILENAME = "park_and_ride_" + SHADOW_PRICES_FILENAME;
        private const string ARCHIVE_PARK_AND_RIDE_SHADOW_PRICES_FILENAME = "archive_" + PARK_AND_RIDE_SHADOW_PRICES_FILENAME;

        public static IKernel Kernel { get; set; }

		public static Configuration Configuration { get; set; }

		public static ISettings Settings { get; set; }

		public static ChoiceModelSession ChoiceModelSession { get; set; }

		public static PrintFile PrintFile { get; set; }

		public static bool TraceResults { get; set; }

        public static int LastNodeDistanceRecord { get; set; }

		public static int[] ANodeId { get; set; }

		public static int[] ANodeFirstRecord { get; set; }

		public static int[] ANodeLastRecord { get; set; }

		public static int[] NodePairBNodeId { get; set; }

		public static ushort[] NodePairDistance { get; set; }

		public static int[] NodeNodePreviousOriginParcelId { get; set; }

		public static int[] NodeNodePreviousDestinationParcelId { get; set; }

		public static double[] NodeNodePreviousDistance { get; set; }

		public static double[][][][][] AggregateLogsums { get; set; }

		public static SegmentZone[][] SegmentZones { get; set; }

		public static Dictionary<int, int> NodeIndex { get; set; }

		public static Dictionary<int, int> TransitStopAreaMapping { get; set; }

		public static Dictionary<int, int> MicrozoneMapping { get; set; }

		public static Dictionary<int, int> ParkAndRideNodeMapping { get; set; }
		
		public static int[] ParcelStopAreaParcelIds { get; set; }
		public static int[] ParcelStopAreaStopAreaKeys { get; set; }
		public static int[] ParcelStopAreaStopAreaIds { get; set; }
		public static float[] ParcelStopAreaDistances { get; set; }

		public static int[] ParcelParkAndRideNodeIds { get; set; }
		public static int[] ParcelParkAndRideNodeSequentialIds { get; set; }
		public static float[] ParcelToBikeCarParkAndRideNodeDistance { get; set; }

		public static double Coefficients_CostCoefficientIncomeMultipleMinimum {
			get {
				return
					Math.Abs(Configuration.Coefficients_CostCoefficientIncomeMultipleMinimum) < Constants.EPSILON
						? 0.1
						: Configuration.Coefficients_CostCoefficientIncomeMultipleMinimum;
			}
		}

		public static double Coefficients_CostCoefficientIncomeMultipleMaximum {
			get {
				return
					Math.Abs(Configuration.Coefficients_CostCoefficientIncomeMultipleMaximum) < Constants.EPSILON
						? 10.0
						: Configuration.Coefficients_CostCoefficientIncomeMultipleMaximum;
			}
		}

		public static string AggregateLogsumCalculator {
			get {
				return
					string.IsNullOrEmpty(Configuration.AggregateLogsumCalculator)
						? "AggregateLogsumCalculator"
						: Configuration.AggregateLogsumCalculator;
			}
		}

		public static string PathTypeModel {
			get {
				return
					string.IsNullOrEmpty(Configuration.PathTypeModel)
						? "PathTypeModel"
						: Configuration.PathTypeModel;
			}
		}

		public static bool ParkAndRideNodeIsEnabled {
			get { return !string.IsNullOrEmpty(Configuration.RawParkAndRideNodePath) && !string.IsNullOrEmpty(Configuration.InputParkAndRideNodePath); }
		}

		public static string DefaultInputParkAndRideNodePath {
			get { return GetWorkingSubpath("park_and_ride_node.tsv"); }
		}

		public static bool ParcelNodeIsEnabled {
			get { return !string.IsNullOrEmpty(Configuration.RawParcelNodePath) && !string.IsNullOrEmpty(Configuration.InputParcelNodePath); }
		}

		public static bool StopAreaIsEnabled {
			get { return !string.IsNullOrEmpty(Configuration.RawTransitStopAreaPath) && !string.IsNullOrEmpty(Configuration.InputTransitStopAreaPath); }
		}

        public static string ArchiveShadowPricesPath
        {
            get { return GetOutputPath(ARCHIVE_SHADOW_PRICES_FILENAME); }
        }

        public static string ArchiveParkAndRideShadowPricesPath
        {
            get { return GetOutputPath(ARCHIVE_PARK_AND_RIDE_SHADOW_PRICES_FILENAME); }
        }

        public static string DefaultInputParcelPath
        {
            get { return GetWorkingSubpath("parcel.tsv"); }
        }

        public static string DefaultInputZonePath {
			get { return GetWorkingSubpath("zone.tsv"); }
		}

		public static string DefaultInputTransitStopAreaPath {
			get { return GetWorkingSubpath("stoparea.tsv"); }
		}

		public static string DefaultInputHouseholdPath {
			get { return GetWorkingSubpath("household.tsv"); }
		}

		public static string DefaultInputHouseholdDayPath {
			get { return GetWorkingSubpath("household_day.tsv"); }
		}

		public static string DefaultInputJointTourPath {
			get { return GetWorkingSubpath("joint_tour.tsv"); }
		}

		public static string DefaultInputFullHalfTourPath {
			get { return GetWorkingSubpath("full_half_tour.tsv"); }
		}

		public static string DefaultInputPartialHalfTourPath {
			get { return GetWorkingSubpath("partial_half_tour.tsv"); }
		}

		public static string DefaultInputPersonPath {
			get { return GetWorkingSubpath("person.tsv"); }
		}

		public static string DefaultInputPersonDayPath {
			get { return GetWorkingSubpath("person_day.tsv"); }
		}

		public static string DefaultInputTourPath {
			get { return GetWorkingSubpath("tour.tsv"); }
		}

		public static string DefaultInputTripPath {
			get { return GetWorkingSubpath("trip.tsv"); }
		}

		public static string WorkingParkAndRideNodePath {
			get { return GetWorkingPath("park_and_ride_node.bin"); }
		}

		public static string WorkingParcelNodePath {
			get { return GetWorkingPath("parcel_node.bin"); }
		}

		public static string WorkingParcelPath {
			get { return GetWorkingPath("parcel.bin"); }
		}

		public static string WorkingZonePath {
			get { return GetWorkingPath("zone.bin"); }
		}

		public static string WorkingTransitStopAreaPath {
			get { return GetWorkingPath("transit_stop_area.bin"); }
		}

		public static string WorkingHouseholdPath {
			get { return GetWorkingPath("household.bin"); }
		}

		public static string WorkingHouseholdDayPath {
			get { return GetWorkingPath("household_day.bin"); }
		}

		public static string WorkingJointTourPath {
			get { return GetWorkingPath("joint_tour.bin"); }
		}

		public static string WorkingFullHalfTourPath {
			get { return GetWorkingPath("full_half_tour.bin"); }
		}

		public static string WorkingPartialHalfTourPath {
			get { return GetWorkingPath("partial_half_tour.bin"); }
		}

		public static string WorkingPersonPath {
			get { return GetWorkingPath("person.bin"); }
		}

		public static string WorkingPersonDayPath {
			get { return GetWorkingPath("person_day.bin"); }
		}

		public static string WorkingTourPath {
			get { return GetWorkingPath("tour.bin"); }
		}

		public static string WorkingTripPath {
			get { return GetWorkingPath("trip.bin"); }
		}

		public static string AggregateLogsumsPath {
			get { return GetWorkingPath("aggregate_logsums.bin"); }
		}

		public static string SamplingWeightsPath {
			get { return GetWorkingPath("sampling_weights_{0}.bin"); }
		}

		public static string ShadowPricesPath {
			get { return GetWorkingPath(SHADOW_PRICES_FILENAME); }
		}

		public static string ParkAndRideShadowPricesPath {
			get { return GetWorkingPath(PARK_AND_RIDE_SHADOW_PRICES_FILENAME); }
		}

		public static double Coefficients_BaseCostCoefficientPerMonetaryUnit {
			get {
				return
					Math.Abs(Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit) > Constants.EPSILON
						? Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit
						: Configuration.Coefficients_BaseCostCoefficientPerDollar;
			}
		}

		public static double PathImpedance_WalkMinutesPerDistanceUnit {
			get {
				return Math.Abs(Configuration.PathImpedance_WalkMinutesPerDistanceUnit) > Constants.EPSILON
					? Configuration.PathImpedance_WalkMinutesPerDistanceUnit
					: Configuration.PathImpedance_WalkMinutesPerMile;
			}
		}

		public static double PathImpedance_AutoOperatingCostPerDistanceUnit {
			get {
				return Math.Abs(Configuration.PathImpedance_AutoOperatingCostPerDistanceUnit) > Constants.EPSILON
					? Configuration.PathImpedance_AutoOperatingCostPerDistanceUnit
					: Configuration.PathImpedance_AutoOperatingCostPerMile;
			}
		}

		public static char SkimDelimiter {
			get { return Configuration.SkimDelimiter == 0 ? ' ' : Configuration.SkimDelimiter; }
		}

		public static bool TextSkimFilesContainHeaderRecord {
			get { return Configuration.TextSkimFilesContainHeaderRecord; }
		}

		public static string SamplingWeightsSettingsType {
			get { return string.IsNullOrEmpty(Configuration.SamplingWeightsSettingsType) ? "SamplingWeightsSettings" : Configuration.SamplingWeightsSettingsType; }
		}

		public static int MaximumHouseholdSize {
			get {
				return
					Configuration.MaximumHouseholdSize != 0
						? Configuration.MaximumHouseholdSize
						: 20;
			}
		}

		private static string GetSubpath(string file, string subPath, bool ignoreBasePath = false) {
			if (file.Contains(":\\")) {
				return file;
			}

            if (ignoreBasePath || Configuration.BasePath == null) {
				return
					subPath == null
						? file
						: Path.Combine(subPath, file);
			}

			return
				subPath == null
					? Path.Combine(Configuration.BasePath, file)
					: Path.Combine(Configuration.BasePath, subPath, file);
		}

		public static string GetInputPath(string file) {
			return GetSubpath(file, "");
		}

		public static string GetInputPath<TModel>() where TModel : IModel {
			var path = GetConfigurationValue<TModel, string>("Input", "Path");
            return GetInputPath(path);
        }

        public static char GetInputDelimiter<TModel>() where TModel : IModel {
			return GetConfigurationValue<TModel, char>("Input", "Delimiter");
		}

		public static string GetOutputPath(string file) {
			return GetSubpath(file, Configuration.OutputSubpath);
		}

		public static string GetOutputPath<TModel>() where TModel : IModel {
			var path = GetConfigurationValue<TModel, string>("Output", "Path");
            return GetOutputPath(path);
		}

		public static char GetOutputDelimiter<TModel>() where TModel : IModel {
			return GetConfigurationValue<TModel, char>("Output", "Delimiter");
		}

		public static string GetWorkingPath<TModel>() where TModel : IModel {
			return GetGlobalValue<TModel, string>("Working", "Path");
		}

		private static TType GetConfigurationValue<TModel, TType>(string prefix, string suffix) where TModel : IModel {
			var type1 = typeof (TModel);
			var type2 = typeof (Configuration);

			var property = type2.GetProperty(prefix + type1.Name + suffix, BindingFlags.Public | BindingFlags.Instance);

			return (TType) property.GetValue(Configuration, null);
		}

		private static TType GetGlobalValue<TModel, TType>(string prefix, string suffix) where TModel : IModel {
			var type1 = typeof (TModel);
			var type2 = typeof (Global);

			var property = type2.GetProperty(prefix + type1.Name + suffix, BindingFlags.Public | BindingFlags.Static);

			return (TType) property.GetValue(null, null);
		}

		public static string GetEstimationPath(string file) {
			return GetSubpath(file, Configuration.EstimationSubpath);
		}

        public static string GetWorkingPath(string file, bool ignoreBasePath = false)
        {
            return
                GetSubpath(file, string.IsNullOrEmpty(Configuration.WorkingSubpath)
                    ? Configuration.WorkingDirectory
                    : Configuration.WorkingSubpath, ignoreBasePath);
        }

        //Special version that will not prepend the BasePath. This is used for default paths since the BasePath will be added in later.
        //Related to issue #65
        public static string GetWorkingSubpath(string file)
        {
            return GetWorkingPath(file, ignoreBasePath: true);
        }

        public static void InitializeNodeIndex() {
			var reader =
				Kernel
					.Get<IPersistenceFactory<IParcelNode>>()
					.Reader;

			NodeIndex = new Dictionary<int, int>();

			foreach (var node in reader) {
				NodeIndex.Add(node.Id, node.NodeId);
			}

			NodeNodePreviousOriginParcelId = new int[ParallelUtility.NBatches]; //Constants.DEFAULT_VALUE;
			NodeNodePreviousDestinationParcelId = new int[ParallelUtility.NBatches]; // = Constants.DEFAULT_VALUE;
			NodeNodePreviousDistance = new double[ParallelUtility.NBatches]; // = Constants.DEFAULT_VALUE;

			for (var i = 0; i < ParallelUtility.NBatches; i++) {
				NodeNodePreviousOriginParcelId[i] = Constants.DEFAULT_VALUE;
				NodeNodePreviousDestinationParcelId[i] = Constants.DEFAULT_VALUE;
				NodeNodePreviousDistance[i] = Constants.DEFAULT_VALUE;
			}
		}

	}
}