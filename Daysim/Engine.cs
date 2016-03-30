// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Daysim.AggregateLogsums;
using Daysim.ChoiceModels;
//using Daysim.DomainModels.Actum.Models.Interfaces;
using Daysim.Framework.ChoiceModels;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Creators;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Exceptions;
using Daysim.Framework.Factories;
using Daysim.Framework.Roster;
using Daysim.ParkAndRideShadowPricing;
using Daysim.Sampling;
using Daysim.ShadowPricing;
using HDF5DotNet;
using Ninject;
using Timer = Daysim.Framework.Core.Timer;

namespace Daysim {
	public static class Engine {
		private static int _start = -1;
		private static int _end = -1;
		private static int _index = -1;

		public static void BeginTestMode() {
			var randomUtility = new RandomUtility();
			randomUtility.ResetUniform01(Global.Configuration.RandomSeed);
			randomUtility.ResetHouseholdSynchronization(Global.Configuration.RandomSeed);

			BeginInitialize();

			//RawConverter.RunTestMode();
		}

		public static void BeginProgram(int start, int end, int index) {
			_start = start;
			_end = end;
			_index = index;

			var timer = new Timer("Starting Daysim...");

			var randomUtility = new RandomUtility();
			randomUtility.ResetUniform01(Global.Configuration.RandomSeed);
			randomUtility.ResetHouseholdSynchronization(Global.Configuration.RandomSeed);

			BeginInitialize();
			if (Global.Configuration.ShouldRunInputTester == true) {
				BeginTestInputs();
			}

			BeginRunRawConversion();
			BeginImportData();
			BeginBuildIndexes();

			BeginLoadRoster();

			//moved this up to load data dictionaires sooner
			ChoiceModelFactory.Initialize(Global.Configuration.ChoiceModelRunner, ParallelUtility.NBatches);
			//ChoiceModelFactory.LoadData();

			BeginLoadNodeIndex();
			BeginLoadNodeDistances();
			BeginLoadNodeStopAreaDistances();
			BeginLoadMicrozoneToBikeCarParkAndRideNodeDistances();

			BeginCalculateAggregateLogsums(randomUtility);
			BeginOutputAggregateLogsums();
			BeginCalculateSamplingWeights();
			BeginOutputSamplingWeights();

			BeginRunChoiceModels(randomUtility);
			BeginPerformHousekeeping();

			if (_start == -1 || _end == -1 || _index == -1) {
				BeginUpdateShadowPricing();
			}

			timer.Stop("Total running time");
		}

		public static void BeginInitialize() {
			var timer = new Timer("Initializing...");

			Initialize();

			timer.Stop();
		}

		public static void BeginTestInputs() {
			var timer = new Timer("Checking Input Validity...");
			InputTester.RunTest();
			timer.Stop();
		}

		private static void Initialize() {
			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("Application mode: {0}", Global.Configuration.IsInEstimationMode ? "Estimation" : "Simulation");

				if (Global.Configuration.IsInEstimationMode) {
					Global.PrintFile.WriteLine("Estimation model: {0}", Global.Configuration.EstimationModel);
				}
			}

			InitializePersistenceFactories();
			InitializeWrapperFactories();
			InitializeSkimFactories();
			InitializeSamplingFactories();
			InitializeAggregateLogsumsFactories();

			InitializeOther();

			InitializeOutput();
			InitializeInput();
			InitializeWorking();

			if (Global.Configuration.ShouldOutputAggregateLogsums) {
				Global.GetOutputPath(Global.Configuration.OutputAggregateLogsumsPath).CreateDirectory();
			}

			if (Global.Configuration.ShouldOutputSamplingWeights) {
				Global.GetOutputPath(Global.Configuration.OutputSamplingWeightsPath).CreateDirectory();
			}

			if (Global.Configuration.ShouldOutputTDMTripList) {
				Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath).CreateDirectory();
			}

			if (!Global.Configuration.IsInEstimationMode) {
				return;
			}

			if (Global.Configuration.ShouldOutputAlogitData) {
				Global.GetOutputPath(Global.Configuration.OutputAlogitDataPath).CreateDirectory();
			}

			Global.GetOutputPath(Global.Configuration.OutputAlogitControlPath).CreateDirectory();
		}

		private static void InitializePersistenceFactories() {
			Global
				.Kernel
				.Get<IPersistenceFactory<IParcel>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IParcelNode>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IParkAndRideNode>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<ITransitStopArea>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IZone>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IHousehold>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IPerson>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IHouseholdDay>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IPersonDay>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<ITour>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<ITrip>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IJointTour>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IFullHalfTour>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IPersistenceFactory<IPartialHalfTour>>()
				.Initialize(Global.Configuration);
		}

		private static void InitializeWrapperFactories() {
			Global
				.Kernel
				.Get<IWrapperFactory<IParcelCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IParcelNodeCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IParkAndRideNodeCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IZoneCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IHouseholdCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IPersonCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IHouseholdDayCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IPersonDayCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<ITourCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<ITripCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IJointTourCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IFullHalfTourCreator>>()
				.Initialize(Global.Configuration);

			Global
				.Kernel
				.Get<IWrapperFactory<IPartialHalfTourCreator>>()
				.Initialize(Global.Configuration);
		}

		private static void InitializeSkimFactories() {
			Global.Kernel.Get<SkimFileReaderFactory>().Register("text_ij", new TextIJSkimFileReaderCreator());
			Global.Kernel.Get<SkimFileReaderFactory>().Register("visum-bin", new VisumSkimReaderCreator());
			Global.Kernel.Get<SkimFileReaderFactory>().Register("bin", new BinarySkimFileReaderCreator());
			Global.Kernel.Get<SkimFileReaderFactory>().Register("emme", new EMMEReaderCreator());
			Global.Kernel.Get<SkimFileReaderFactory>().Register("hdf5", new HDF5ReaderCreator());
			Global.Kernel.Get<SkimFileReaderFactory>().Register("cube", new CubeReaderCreator());
			Global.Kernel.Get<SkimFileReaderFactory>().Register("transcad", new TranscadReaderCreator());
		}

		private static void InitializeSamplingFactories() {
			Global.Kernel.Get<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettings", new SamplingWeightsSettings());
			Global.Kernel.Get<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettingsSimple", new SamplingWeightsSettingsSimple());
			Global.Kernel.Get<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettingsSACOG", new SamplingWeightsSettingsSACOG());
			Global.Kernel.Get<SamplingWeightsSettingsFactory>().Initialize();
		}

		private static void InitializeAggregateLogsumsFactories() {
			Global.Kernel.Get<AggregateLogsumsCalculatorFactory>().Register("AggregateLogsumCalculator", new AggregateLogsumsCalculatorCreator());
			Global.Kernel.Get<AggregateLogsumsCalculatorFactory>().Register("OtherAggregateLogsumCalculator", new OtherAggregateLogsumsCalculatorCreator());
			Global.Kernel.Get<AggregateLogsumsCalculatorFactory>().Initialize();
		}

		private static void InitializeOther() {
			Global.ChoiceModelSession = new ChoiceModelSession();

			HTripTime.InitializeTripTimes();
			HTourModeTime.InitializeTourModeTimes();
		}

		private static void InitializeOutput() {
			if (_start == -1 || _end == -1 || _index == -1) {
				return;
			}

			Global.Configuration.OutputHouseholdPath = Global.GetOutputPath(Global.Configuration.OutputHouseholdPath).ToIndexedPath(_index);
			Global.Configuration.OutputPersonPath = Global.GetOutputPath(Global.Configuration.OutputPersonPath).ToIndexedPath(_index);
			Global.Configuration.OutputHouseholdDayPath = Global.GetOutputPath(Global.Configuration.OutputHouseholdDayPath).ToIndexedPath(_index);
			Global.Configuration.OutputJointTourPath = Global.GetOutputPath(Global.Configuration.OutputJointTourPath).ToIndexedPath(_index);
			Global.Configuration.OutputFullHalfTourPath = Global.GetOutputPath(Global.Configuration.OutputFullHalfTourPath).ToIndexedPath(_index);
			Global.Configuration.OutputPartialHalfTourPath = Global.GetOutputPath(Global.Configuration.OutputPartialHalfTourPath).ToIndexedPath(_index);
			Global.Configuration.OutputPersonDayPath = Global.GetOutputPath(Global.Configuration.OutputPersonDayPath).ToIndexedPath(_index);
			Global.Configuration.OutputTourPath = Global.GetOutputPath(Global.Configuration.OutputTourPath).ToIndexedPath(_index);
			Global.Configuration.OutputTripPath = Global.GetOutputPath(Global.Configuration.OutputTripPath).ToIndexedPath(_index);
			Global.Configuration.OutputTDMTripListPath = Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath).ToIndexedPath(_index);
		}

		private static void InitializeInput() {
			if (string.IsNullOrEmpty(Global.Configuration.InputParkAndRideNodePath)) {
				Global.Configuration.InputParkAndRideNodePath = Global.DefaultInputParkAndRideNodePath;
				Global.Configuration.InputParkAndRideNodeDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputParcelNodePath)) {
				Global.Configuration.InputParcelNodePath = Global.DefaultInputParcelNodePath;
				Global.Configuration.InputParcelNodeDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputParcelPath)) {
				Global.Configuration.InputParcelPath = Global.DefaultInputParcelPath;
				Global.Configuration.InputParcelDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputParcelPath)) {
				Global.Configuration.InputParcelPath = Global.DefaultInputParcelPath;
				Global.Configuration.InputParcelDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputZonePath)) {
				Global.Configuration.InputZonePath = Global.DefaultInputZonePath;
				Global.Configuration.InputZoneDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputTransitStopAreaPath)) {
				Global.Configuration.InputTransitStopAreaPath = Global.DefaultInputTransitStopAreaPath;
				Global.Configuration.InputTransitStopAreaDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputHouseholdPath)) {
				Global.Configuration.InputHouseholdPath = Global.DefaultInputHouseholdPath;
				Global.Configuration.InputHouseholdDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputPersonPath)) {
				Global.Configuration.InputPersonPath = Global.DefaultInputPersonPath;
				Global.Configuration.InputPersonDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputHouseholdDayPath)) {
				Global.Configuration.InputHouseholdDayPath = Global.DefaultInputHouseholdDayPath;
				Global.Configuration.InputHouseholdDayDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputJointTourPath)) {
				Global.Configuration.InputJointTourPath = Global.DefaultInputJointTourPath;
				Global.Configuration.InputJointTourDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputFullHalfTourPath)) {
				Global.Configuration.InputFullHalfTourPath = Global.DefaultInputFullHalfTourPath;
				Global.Configuration.InputFullHalfTourDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputPartialHalfTourPath)) {
				Global.Configuration.InputPartialHalfTourPath = Global.DefaultInputPartialHalfTourPath;
				Global.Configuration.InputPartialHalfTourDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputPersonDayPath)) {
				Global.Configuration.InputPersonDayPath = Global.DefaultInputPersonDayPath;
				Global.Configuration.InputPersonDayDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputTourPath)) {
				Global.Configuration.InputTourPath = Global.DefaultInputTourPath;
				Global.Configuration.InputTourDelimiter = '\t';
			}

			if (string.IsNullOrEmpty(Global.Configuration.InputTripPath)) {
				Global.Configuration.InputTripPath = Global.DefaultInputTripPath;
				Global.Configuration.InputTripDelimiter = '\t';
			}

			var inputParkAndRideNodeFile = Global.ParkAndRideNodeIsEnabled ? Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile() : null;
			var inputParcelNodeFile = Global.ParcelNodeIsEnabled ? Global.GetInputPath(Global.Configuration.InputParcelNodePath).ToFile() : null;
			var inputParcelFile = Global.GetInputPath(Global.Configuration.InputParcelPath).ToFile();
			var inputZoneFile = Global.GetInputPath(Global.Configuration.InputZonePath).ToFile();
			var inputHouseholdFile = Global.GetInputPath(Global.Configuration.InputHouseholdPath).ToFile();
			var inputPersonFile = Global.GetInputPath(Global.Configuration.InputPersonPath).ToFile();
			var inputHouseholdDayFile = Global.GetInputPath(Global.Configuration.InputHouseholdDayPath).ToFile();
			var inputJointTourFile = Global.GetInputPath(Global.Configuration.InputJointTourPath).ToFile();
			var inputFullHalfTourFile = Global.GetInputPath(Global.Configuration.InputFullHalfTourPath).ToFile();
			var inputPartialHalfTourFile = Global.GetInputPath(Global.Configuration.InputPartialHalfTourPath).ToFile();
			var inputPersonDayFile = Global.GetInputPath(Global.Configuration.InputPersonDayPath).ToFile();
			var inputTourFile = Global.GetInputPath(Global.Configuration.InputTourPath).ToFile();
			var inputTripFile = Global.GetInputPath(Global.Configuration.InputTripPath).ToFile();

			InitializeInputDirectories(inputParkAndRideNodeFile, inputParcelNodeFile, inputParcelFile, inputZoneFile, inputHouseholdFile, inputPersonFile, inputHouseholdDayFile, inputJointTourFile, inputFullHalfTourFile, inputPartialHalfTourFile, inputPersonDayFile, inputTourFile, inputTripFile);

			if (Global.PrintFile == null) {
				return;
			}

			Global.PrintFile.WriteLine("Input files:");
			Global.PrintFile.IncrementIndent();

			Global.PrintFile.WriteFileInfo(inputParkAndRideNodeFile, "Park-and-ride node is not enabled, input park-and-ride node file not set.");
			Global.PrintFile.WriteFileInfo(inputParcelNodeFile, "Parcel node is not enabled, input parcel node file not set.");
			Global.PrintFile.WriteFileInfo(inputParcelFile);
			Global.PrintFile.WriteFileInfo(inputZoneFile);
			Global.PrintFile.WriteFileInfo(inputHouseholdFile);
			Global.PrintFile.WriteFileInfo(inputPersonFile);

			if (Global.Configuration.IsInEstimationMode && !Global.Configuration.ShouldRunRawConversion) {
				Global.PrintFile.WriteFileInfo(inputHouseholdDayFile);
				Global.PrintFile.WriteFileInfo(inputJointTourFile);
				Global.PrintFile.WriteFileInfo(inputFullHalfTourFile);
				Global.PrintFile.WriteFileInfo(inputPartialHalfTourFile);
				Global.PrintFile.WriteFileInfo(inputPersonDayFile);
				Global.PrintFile.WriteFileInfo(inputTourFile);
				Global.PrintFile.WriteFileInfo(inputTripFile);
			}

			Global.PrintFile.DecrementIndent();
		}

		private static void InitializeInputDirectories(FileInfo inputParkAndRideNodeFile, FileInfo inputParcelNodeFile, FileInfo inputParcelFile, FileInfo inputZoneFile, FileInfo inputHouseholdFile, FileInfo inputPersonFile, FileInfo inputHouseholdDayFile, FileInfo inputJointTourFile, FileInfo inputFullHalfTourFile, FileInfo inputPartialHalfTourFile, FileInfo inputPersonDayFile, FileInfo inputTourFile, FileInfo inputTripFile) {
			if (inputParkAndRideNodeFile != null) {
				inputParkAndRideNodeFile.CreateDirectory();
			}

			if (inputParcelNodeFile != null) {
				inputParcelNodeFile.CreateDirectory();
			}

			inputParcelFile.CreateDirectory();
			inputZoneFile.CreateDirectory();

			inputHouseholdFile.CreateDirectory();
			Global.GetOutputPath(Global.Configuration.OutputHouseholdPath).CreateDirectory();

			inputPersonFile.CreateDirectory();
			Global.GetOutputPath(Global.Configuration.OutputPersonPath).CreateDirectory();

			var override1 = (inputParkAndRideNodeFile != null && !inputParkAndRideNodeFile.Exists) || (inputParcelNodeFile != null && !inputParcelNodeFile.Exists) || !inputParcelFile.Exists || !inputZoneFile.Exists || !inputHouseholdFile.Exists || !inputPersonFile.Exists;
			var override2 = false;

			if (Global.Configuration.IsInEstimationMode) {
				inputHouseholdDayFile.CreateDirectory();
				Global.GetOutputPath(Global.Configuration.OutputHouseholdDayPath).CreateDirectory();

				if (Global.Settings.UseJointTours) {
					inputJointTourFile.CreateDirectory();
					Global.GetOutputPath(Global.Configuration.OutputJointTourPath).CreateDirectory();

					inputFullHalfTourFile.CreateDirectory();
					Global.GetOutputPath(Global.Configuration.OutputFullHalfTourPath).CreateDirectory();

					inputPartialHalfTourFile.CreateDirectory();
					Global.GetOutputPath(Global.Configuration.OutputPartialHalfTourPath).CreateDirectory();
				}

				inputPersonDayFile.CreateDirectory();
				Global.GetOutputPath(Global.Configuration.OutputPersonDayPath).CreateDirectory();

				inputTourFile.CreateDirectory();
				Global.GetOutputPath(Global.Configuration.OutputTourPath).CreateDirectory();

				inputTripFile.CreateDirectory();
				Global.GetOutputPath(Global.Configuration.OutputTripPath).CreateDirectory();

				override2 = !inputHouseholdDayFile.Exists || !inputJointTourFile.Exists || !inputFullHalfTourFile.Exists || !inputPartialHalfTourFile.Exists || !inputPersonDayFile.Exists || !inputTourFile.Exists || !inputTripFile.Exists;
			}

			if (override1 || override2) {
				OverrideShouldRunRawConversion();
			}
		}

		private static void InitializeWorking() {
			var workingParkAndRideNodeFile = Global.ParkAndRideNodeIsEnabled ? Global.WorkingParkAndRideNodePath.ToFile() : null;
			var workingParcelNodeFile = Global.ParcelNodeIsEnabled ? Global.WorkingParcelNodePath.ToFile() : null;
			var workingParcelFile = Global.WorkingParcelPath.ToFile();
			var workingZoneFile = Global.WorkingZonePath.ToFile();
			var workingHouseholdFile = Global.WorkingHouseholdPath.ToFile();
			var workingHouseholdDayFile = Global.WorkingHouseholdDayPath.ToFile();
			var workingJointTourFile = Global.WorkingJointTourPath.ToFile();
			var workingFullHalfTourFile = Global.WorkingFullHalfTourPath.ToFile();
			var workingPartialHalfTourFile = Global.WorkingPartialHalfTourPath.ToFile();
			var workingPersonFile = Global.WorkingPersonPath.ToFile();
			var workingPersonDayFile = Global.WorkingPersonDayPath.ToFile();
			var workingTourFile = Global.WorkingTourPath.ToFile();
			var workingTripFile = Global.WorkingTripPath.ToFile();

			InitializeWorkingDirectory();

			InitializeWorkingImports(workingParkAndRideNodeFile, workingParcelNodeFile, workingParcelFile, workingZoneFile, workingHouseholdFile, workingPersonFile, workingHouseholdDayFile, workingJointTourFile, workingFullHalfTourFile, workingPartialHalfTourFile, workingPersonDayFile, workingTourFile, workingTripFile);

			if (Global.PrintFile == null) {
				return;
			}

			Global.PrintFile.WriteLine("Working files:");
			Global.PrintFile.IncrementIndent();

			Global.PrintFile.WriteFileInfo(workingParkAndRideNodeFile, "Park-and-ride node is not enabled, working park-and-ride node file not set.");
			Global.PrintFile.WriteFileInfo(workingParcelNodeFile, "Parcel node is not enabled, working parcel node file not set.");
			Global.PrintFile.WriteFileInfo(workingParcelFile);
			Global.PrintFile.WriteFileInfo(workingZoneFile);
			Global.PrintFile.WriteFileInfo(workingHouseholdFile);
			Global.PrintFile.WriteFileInfo(workingPersonFile);
			Global.PrintFile.WriteFileInfo(workingHouseholdDayFile);
			Global.PrintFile.WriteFileInfo(workingJointTourFile);
			Global.PrintFile.WriteFileInfo(workingFullHalfTourFile);
			Global.PrintFile.WriteFileInfo(workingPartialHalfTourFile);
			Global.PrintFile.WriteFileInfo(workingPersonDayFile);
			Global.PrintFile.WriteFileInfo(workingTourFile);
			Global.PrintFile.WriteFileInfo(workingTripFile);

			Global.PrintFile.DecrementIndent();
		}

		private static void InitializeWorkingDirectory() {
			var workingDirectory = new DirectoryInfo(Global.GetWorkingPath(""));

			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("Working directory: {0}", workingDirectory);
			}

			if (workingDirectory.Exists) {
				return;
			}

			workingDirectory.CreateDirectory();
			OverrideShouldRunRawConversion();
		}

		private static void InitializeWorkingImports(FileInfo workingParkAndRideNodeFile, FileInfo workingParcelNodeFile, FileInfo workingParcelFile, FileInfo workingZoneFile, FileInfo workingHouseholdFile, FileInfo workingPersonFile, FileInfo workingHouseholdDayFile, FileInfo workingJointTourFile, FileInfo workingFullHalfTourFile, FileInfo workingPartialHalfTourFile, FileInfo workingPersonDayFile, FileInfo workingTourFile, FileInfo workingTripFile) {
			if (Global.Configuration.ShouldRunRawConversion || (workingParkAndRideNodeFile != null && !workingParkAndRideNodeFile.Exists) || (workingParcelNodeFile != null && !workingParcelNodeFile.Exists) || !workingParcelFile.Exists || !workingZoneFile.Exists || !workingHouseholdFile.Exists || !workingPersonFile.Exists) {
				if (workingParkAndRideNodeFile != null) {
					OverrideImport(Global.Configuration, x => x.ImportParkAndRideNodes);
				}

				if (workingParcelNodeFile != null) {
					OverrideImport(Global.Configuration, x => x.ImportParcelNodes);
				}

				OverrideImport(Global.Configuration, x => x.ImportParcels);
				OverrideImport(Global.Configuration, x => x.ImportZones);
				OverrideImport(Global.Configuration, x => x.ImportHouseholds);
				OverrideImport(Global.Configuration, x => x.ImportPersons);
			}

			if (!Global.Configuration.IsInEstimationMode || (!Global.Configuration.ShouldRunRawConversion && workingHouseholdDayFile.Exists && workingJointTourFile.Exists && workingFullHalfTourFile.Exists && workingPartialHalfTourFile.Exists && workingPersonDayFile.Exists && workingTourFile.Exists && workingTripFile.Exists)) {
				return;
			}

			OverrideImport(Global.Configuration, x => x.ImportHouseholdDays);
			OverrideImport(Global.Configuration, x => x.ImportJointTours);
			OverrideImport(Global.Configuration, x => x.ImportFullHalfTours);
			OverrideImport(Global.Configuration, x => x.ImportPartialHalfTours);
			OverrideImport(Global.Configuration, x => x.ImportPersonDays);
			OverrideImport(Global.Configuration, x => x.ImportTours);
			OverrideImport(Global.Configuration, x => x.ImportTrips);
		}

		public static void BeginRunRawConversion() {
			if (!Global.Configuration.ShouldRunRawConversion) {
				return;
			}

			var timer = new Timer("Running raw conversion...");

			if (Global.PrintFile != null) {
				Global.PrintFile.IncrementIndent();
			}

			RawConverter.Run();

			if (Global.PrintFile != null) {
				Global.PrintFile.DecrementIndent();
			}

			timer.Stop();
		}

		public static void BeginImportData() {
			ImportParcels();
			ImportParcelNodes();
			ImportParkAndRideNodes();
			ImportTransitStopAreas();
			ImportZones();
			ImportHouseholds();
			ImportPersons();

			if (!Global.Configuration.IsInEstimationMode) {
				return;
			}

			ImportHouseholdDays();
			ImportPersonDays();
			ImportTours();
			ImportTrips();

			if (!Global.Settings.UseJointTours) {
				return;
			}

			ImportJointTours();
			ImportFullHalfTours();
			ImportPartialHalfTours();
		}

		private static void ImportParcels() {
			if (!Global.Configuration.ImportParcels) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IParcel>>()
				.Importer
				.Import();
		}

		private static void ImportParcelNodes() {
			if (!Global.ParcelNodeIsEnabled || !Global.Configuration.ImportParcelNodes) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IParcelNode>>()
				.Importer
				.Import();
		}

		private static void ImportParkAndRideNodes() {
			if (!Global.ParkAndRideNodeIsEnabled || !Global.Configuration.ImportParkAndRideNodes) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IParkAndRideNode>>()
				.Importer
				.Import();

//JLB 20160323
//            if (!Global.StopAreaIsEnabled || !(Global.Configuration.DataType == "Actum"))
            if (!Global.StopAreaIsEnabled || !(Global.Configuration.DataType == "Default"))
            {
                return;
            }

			var parkAndRideNodeReader =
				Global
					.Kernel
					.Get<IPersistenceFactory<IParkAndRideNode>>()
					.Reader;

			Global.ParkAndRideNodeMapping = new Dictionary<int, int>(parkAndRideNodeReader.Count);

			foreach (var parkAndRideNode in parkAndRideNodeReader) {
				Global.ParkAndRideNodeMapping.Add(parkAndRideNode.ZoneId, parkAndRideNode.Id);
			}
		}


		private static void ImportTransitStopAreas() {
			if (!Global.Configuration.ImportTransitStopAreas) {
				return;
			}

			if (string.IsNullOrEmpty(Global.WorkingTransitStopAreaPath) || string.IsNullOrEmpty(Global.Configuration.InputTransitStopAreaPath)) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<ITransitStopArea>>()
				.Importer
				.Import();
		}

		private static void ImportZones() {
			if (!Global.Configuration.ImportZones) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IZone>>()
				.Importer
				.Import();
		}

		private static void ImportHouseholds() {
			if (!Global.Configuration.ImportHouseholds) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IHousehold>>()
				.Importer
				.Import();
		}

		private static void ImportPersons() {
			if (!Global.Configuration.ImportPersons) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IPerson>>()
				.Importer
				.Import();
		}

		private static void ImportHouseholdDays() {
			if (!Global.Configuration.ImportHouseholdDays) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IHouseholdDay>>()
				.Importer
				.Import();
		}

		private static void ImportPersonDays() {
			if (!Global.Configuration.ImportPersonDays) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IPersonDay>>()
				.Importer
				.Import();
		}

		private static void ImportTours() {
			if (!Global.Configuration.ImportTours) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<ITour>>()
				.Importer
				.Import();
		}

		private static void ImportTrips() {
			if (!Global.Configuration.ImportTrips) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<ITrip>>()
				.Importer
				.Import();
		}

		private static void ImportJointTours() {
			if (!Global.Configuration.ImportJointTours) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IJointTour>>()
				.Importer
				.Import();
		}

		private static void ImportFullHalfTours() {
			if (!Global.Configuration.ImportFullHalfTours) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IFullHalfTour>>()
				.Importer
				.Import();
		}

		private static void ImportPartialHalfTours() {
			if (!Global.Configuration.ImportPartialHalfTours) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IPartialHalfTour>>()
				.Importer
				.Import();
		}

		public static void BeginBuildIndexes() {
			var timer = new Timer("Building indexes...");

			BuildIndexes();

			timer.Stop();
		}

		private static void BuildIndexes() {
			if (Global.ParcelNodeIsEnabled) {
				Global
					.Kernel
					.Get<IPersistenceFactory<IParcelNode>>()
					.Reader
					.BuildIndex("parcel_fk", "Id", "NodeId");
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IPerson>>()
				.Reader
				.BuildIndex("household_fk", "Id", "HouseholdId");

			if (!Global.Configuration.IsInEstimationMode) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IHouseholdDay>>()
				.Reader
				.BuildIndex("household_fk", "Id", "HouseholdId");

			Global
				.Kernel
				.Get<IPersistenceFactory<IPersonDay>>()
				.Reader
				.BuildIndex("household_day_fk", "Id", "HouseholdDayId");

			Global
				.Kernel
				.Get<IPersistenceFactory<ITour>>()
				.Reader
				.BuildIndex("person_day_fk", "Id", "PersonDayId");

			Global
				.Kernel
				.Get<IPersistenceFactory<ITrip>>()
				.Reader
				.BuildIndex("tour_fk", "Id", "TourId");

			if (!Global.Settings.UseJointTours) {
				return;
			}

			Global
				.Kernel
				.Get<IPersistenceFactory<IJointTour>>()
				.Reader
				.BuildIndex("household_day_fk", "Id", "HouseholdDayId");

			Global
				.Kernel
				.Get<IPersistenceFactory<IFullHalfTour>>()
				.Reader
				.BuildIndex("household_day_fk", "Id", "HouseholdDayId");

			Global
				.Kernel
				.Get<IPersistenceFactory<IPartialHalfTour>>()
				.Reader
				.BuildIndex("household_day_fk", "Id", "HouseholdDayId");
		}

		private static void BeginLoadNodeIndex() {
			if (!Global.ParcelNodeIsEnabled || !Global.Configuration.UseShortDistanceNodeToNodeMeasures) {
				return;
			}

			var timer = new Timer("Loading node index...");

			LoadNodeIndex();

			timer.Stop();
		}

		private static void LoadNodeIndex() {
			var file = new FileInfo(Global.GetInputPath(Global.Configuration.NodeIndexPath));

			var aNodeId = new List<int>();
			var aNodeFirstRecord = new List<int>();
			var aNodeLastRecord = new List<int>();

			using (var reader = new StreamReader(file.OpenRead())) {
				reader.ReadLine();

				string line;

				while ((line = reader.ReadLine()) != null) {
					var tokens = line.Split(Global.Configuration.NodeIndexDelimiter);

					aNodeId.Add(int.Parse(tokens[0]));
					aNodeFirstRecord.Add(int.Parse(tokens[1]));
					aNodeLastRecord.Add(int.Parse(tokens[2]));
				}
			}

			Global.ANodeId = aNodeId.ToArray();
			Global.ANodeFirstRecord = aNodeFirstRecord.ToArray();
			Global.ANodeLastRecord = aNodeLastRecord.ToArray();
		}

		private static void BeginLoadNodeDistances() {
			if (!Global.ParcelNodeIsEnabled || !Global.Configuration.UseShortDistanceNodeToNodeMeasures) {
				return;
			}

			Global.InitializeNodeIndex();

			var timer = new Timer("Loading node distances...");

			if (Global.Configuration.NodeDistancesPath.Contains(".dat")) {
				LoadNodeDistancesFromBinary();
			}
			else {
				LoadNodeDistancesFromHDF5();
			}

			timer.Stop();
		}

		private static void BeginLoadNodeStopAreaDistances() {
			//mb moved this from Global to Engine
			if (!Global.StopAreaIsEnabled) {
				return;
			}
			if (string.IsNullOrEmpty(Global.Configuration.NodeStopAreaIndexPath)) {
				throw new ArgumentNullException("NodeStopAreaIndexPath");
			}

			var timer = new Timer("Loading node stop area distances...");
			var filename = Global.GetInputPath(Global.Configuration.NodeStopAreaIndexPath);
			using (var reader = File.OpenText(filename)) {
				InitializeParcelStopAreaIndex(reader);
			}

			timer.Stop();
		}

		public static void InitializeParcelStopAreaIndex(TextReader reader) {
			//mb moved this from global to engine in order to use Daysim.ChoiceModels.ChoiceModelFactory
			//mb tried to change this code to set parcel first and last indeces here instead of later, but did not work

			//var parcelIds = new List<int>();  
			var stopAreaKeys = new List<int>();
			var stopAreaIds = new List<int>();
			var lengths = new List<float>();

			// read header
			reader.ReadLine();

			string line;
			int lastParcelId = -1;
			IParcelWrapper parcel = null;
			int arrayIndex = 0;
			//start arrays at index 0 with dummy values, since valid indeces start with 1
			//parcelIds.Add(0);
			stopAreaIds.Add(0);
			stopAreaKeys.Add(0);
			lengths.Add(0F);

			while ((line = reader.ReadLine()) != null) {
				var tokens = line.Split(new[] { ' ' });

				arrayIndex++;
				int parcelId = int.Parse(tokens[0]);
				if (parcelId != lastParcelId) {
					//Console.WriteLine(parcelId);
					parcel = ChoiceModelFactory.Parcels[parcelId];
					parcel.FirstPositionInStopAreaDistanceArray = arrayIndex;
					lastParcelId = parcelId;
				}
				parcel.LastPositionInStopAreaDistanceArray = arrayIndex;

				//parcelIds.Add(int.Parse(tokens[0]));
				int stopAreaId = int.Parse(tokens[1]);
				stopAreaKeys.Add(stopAreaId);
				//mb changed this array to use mapping of stop area ids 
				int stopAreaIndex = Global.TransitStopAreaMapping[stopAreaId];
				stopAreaIds.Add(stopAreaIndex);
				lengths.Add(float.Parse(tokens[2]));
			}

			//Global.ParcelStopAreaParcelIds = parcelIds.ToArray();
			Global.ParcelStopAreaStopAreaKeys = stopAreaKeys.ToArray();
			Global.ParcelStopAreaStopAreaIds = stopAreaIds.ToArray();
			Global.ParcelStopAreaLengths = lengths.ToArray();

		}

		private static void BeginLoadMicrozoneToBikeCarParkAndRideNodeDistances() {
//JLB 20160323
//			if (!Global.StopAreaIsEnabled || !(Global.Configuration.DataType == "Actum")) {
			if (!Global.StopAreaIsEnabled || !(Global.Configuration.DataType == "Default")) {
				return;
			}
			if (string.IsNullOrEmpty(Global.Configuration.MicrozoneToParkAndRideNodeIndexPath)) {
				throw new ArgumentNullException("MicrozoneToParkAndRideNodeIndexPath");
			}

			var timer = new Timer("MicrozoneToParkAndRideNode distances...");
			var filename = Global.GetInputPath(Global.Configuration.MicrozoneToParkAndRideNodeIndexPath);
			using (var reader = File.OpenText(filename)) {
				InitializeMicrozoneToBikeCarParkAndRideNodeIndex(reader);
			}

			timer.Stop();
		}

		public static void InitializeMicrozoneToBikeCarParkAndRideNodeIndex(TextReader reader) {

			//var parcelIds = new List<int>();  
			var nodeSequentialIds = new List<int>();
			var parkAndRideNodeIds = new List<int>();
			var distances = new List<float>();

			// read header
			reader.ReadLine();

			string line;
			int lastParcelId = -1;
			IParcelWrapper parcel = null;
			int arrayIndex = 0;
			//start arrays at index 0 with dummy values, since valid indeces start with 1
			//parcelIds.Add(0);
			nodeSequentialIds.Add(0);
			parkAndRideNodeIds.Add(0);
			distances.Add(0F);

			while ((line = reader.ReadLine()) != null) {
				var tokens = line.Split(new[] {Global.Configuration.MicrozoneToParkAndRideNodeIndexDelimiter });

				arrayIndex++;
				int parcelId = int.Parse(tokens[0]);
				if (parcelId != lastParcelId) {
					//Console.WriteLine(parcelId);
					parcel = ChoiceModelFactory.Parcels[parcelId];
					parcel.FirstPositionInParkAndRideNodeDistanceArray = arrayIndex;
					lastParcelId = parcelId;
				}
				parcel.LastPositionInParkAndRideNodeDistanceArray = arrayIndex;

				//parcelIds.Add(int.Parse(tokens[0]));
				int parkAndRideNodeId = int.Parse(tokens[1]);
				parkAndRideNodeIds.Add(parkAndRideNodeId);
				//mb changed this array to use mapping of stop area ids 
				int nodeSequentialIndex = Global.ParkAndRideNodeMapping[parkAndRideNodeId];
				nodeSequentialIds.Add(nodeSequentialIndex);
				double distance = double.Parse(tokens[2]);
				distances.Add((float) distance);
			}

			Global.ParcelParkAndRideNodeIds = parkAndRideNodeIds.ToArray();
			Global.ParcelParkAndRideNodeSequentialIds = nodeSequentialIds.ToArray();
			Global.ParcelToBikeCarParkAndRideNodeLength = distances.ToArray();

		}



		private static void LoadNodeDistancesFromBinary() {
			var file = new FileInfo(Global.GetInputPath(Global.Configuration.NodeDistancesPath));

			using (var reader = new BinaryReader(file.OpenRead())) {
				Global.NodePairBNodeId = new int[file.Length / 8];
				Global.NodePairDistance = new ushort[file.Length / 8];

				var i = 0;
				var length = reader.BaseStream.Length;
				while (reader.BaseStream.Position < length) {
					Global.NodePairBNodeId[i] = reader.ReadInt32();

					var distance = reader.ReadInt32();

					Global.NodePairDistance[i] = (ushort) Math.Min(distance, ushort.MaxValue);

					i++;
				}
			}
		}

		private static void LoadNodeDistancesFromHDF5() {
			var file = H5F.open(Global.GetInputPath(Global.Configuration.NodeDistancesPath),
				H5F.OpenMode.ACC_RDONLY);

			// Read nodes
			var nodes = H5D.open(file, "node");
			var dspace = H5D.getSpace(nodes);

			var numNodes = H5S.getSimpleExtentDims(dspace)[0];
			Global.NodePairBNodeId = new int[numNodes];
			var wrapArray = new H5Array<int>(Global.NodePairBNodeId);

			var dataType = new H5DataTypeId(H5T.H5Type.NATIVE_INT);

			H5D.read(nodes, dataType, wrapArray);

			H5S.close(dspace);
			H5D.close(nodes);

			// Read distances
			var dist = H5D.open(file, "distance");
			dspace = H5D.getSpace(nodes);

			Global.NodePairDistance = new ushort[numNodes];
			var distArray = new H5Array<ushort>(Global.NodePairDistance);

			dataType = new H5DataTypeId(H5T.H5Type.NATIVE_SHORT);

			H5D.read(nodes, dataType, distArray);

			H5S.close(dspace);
			H5D.close(dist);

			// All done
			H5F.close(file);
		}

		private static void BeginLoadRoster() {
			var timer = new Timer("Loading roster...");

			LoadRoster();

			timer.Stop();
		}

		private static void LoadRoster() {
			var zoneReader =
				Global
					.Kernel
					.Get<IPersistenceFactory<IZone>>()
					.Reader;

			var zoneMapping = new Dictionary<int, int>(zoneReader.Count);

			foreach (var zone in zoneReader) {
				zoneMapping.Add(zone.Key, zone.Id);
			}

			Global.TransitStopAreaMapping = new Dictionary<int, int>();

			if (Global.Configuration.ImportTransitStopAreas) {
				var transitStopAreaReader =
					Global
						.Kernel
						.Get<IPersistenceFactory<ITransitStopArea>>()
						.Reader;

				Global.TransitStopAreaMapping = new Dictionary<int, int>(transitStopAreaReader.Count);

				foreach (var transitStopArea in transitStopAreaReader) {
					Global.TransitStopAreaMapping.Add(transitStopArea.Key, transitStopArea.Id);
				}
			}

			Global.MicrozoneMapping = new Dictionary<int, int>();

			if (Global.Configuration.UseMicrozoneSkims) {
				var microzoneReader =
					Global
						.Kernel
						.Get<IPersistenceFactory<IParcel>>()
						.Reader;

				Global.MicrozoneMapping = new Dictionary<int, int>(microzoneReader.Count);

				int mzSequence = 0;
				foreach (var microzone in microzoneReader) {
					Global.MicrozoneMapping.Add(microzone.Id, mzSequence++);
				}

			}

			ImpedanceRoster.Initialize(zoneMapping, Global.TransitStopAreaMapping, Global.MicrozoneMapping);


		}

		private static void BeginCalculateAggregateLogsums(IRandomUtility randomUtility) {
			var timer = new Timer("Calculating aggregate logsums...");

			var calculator = Global.Kernel.Get<AggregateLogsumsCalculatorFactory>().AggregateLogsumCalculatorCreator.Create();
			calculator.Calculate(randomUtility);

			timer.Stop();
		}

		private static void BeginOutputAggregateLogsums() {
			if (!Global.Configuration.ShouldOutputAggregateLogsums) {
				return;
			}

			var timer = new Timer("Outputting aggregate logsums...");

			AggregateLogsumsExporter.Export(Global.GetOutputPath(Global.Configuration.OutputAggregateLogsumsPath));

			timer.Stop();
		}

		private static void BeginCalculateSamplingWeights() {
			var timer = new Timer("Calculating sampling weights...");

			SamplingWeightsCalculator.Calculate("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, 180);

			timer.Stop();
		}

		private static void BeginOutputSamplingWeights() {
			if (!Global.Configuration.ShouldOutputSamplingWeights) {
				return;
			}

			var timer = new Timer("Outputting sampling weights...");

			SamplingWeightsExporter.Export(Global.GetOutputPath(Global.Configuration.OutputSamplingWeightsPath));

			timer.Stop();
		}

		private static void BeginRunChoiceModels(IRandomUtility randomUtility) {
			if (!Global.Configuration.ShouldRunChoiceModels) {
				return;
			}

			var timer = new Timer("Running choice models...");

			RunChoiceModels(randomUtility);

			timer.Stop();
		}

		private static void RunChoiceModels(IRandomUtility randomUtility) {
			var current = 0;
			var total =
				Global
					.Kernel
					.Get<IPersistenceFactory<IHousehold>>()
					.Reader
					.Count;

			if (Global.Configuration.HouseholdSamplingRateOneInX < 1) {
				Global.Configuration.HouseholdSamplingRateOneInX = 1;
			}

			ChoiceModelFactory.Initialize(Global.Configuration.ChoiceModelRunner, ParallelUtility.NBatches, false);

			var nBatches = ParallelUtility.NBatches;
			var batchSize = total / nBatches;
			var randoms = new Dictionary<int, int>();

			var batches = new List<IHousehold>[nBatches];

			for (var i = 0; i < nBatches; i++) {
				batches[i] = new List<IHousehold>();
			}

			var j = 0;
			foreach (var household in Global.Kernel.Get<IPersistenceFactory<IHousehold>>().Reader) {
				randoms[household.Id] = randomUtility.GetNext();
				var i = j / batchSize;
				if (i >= nBatches) {
					i = nBatches - 1;
				}

				batches[i].Add(household);

				j++;
			}
			var index = 0;
			Parallel.For((int) 0, nBatches,
				new ParallelOptions { MaxDegreeOfParallelism = ParallelUtility.LargeDegreeOfParallelism },
				batchNumber => {
					ParallelUtility.Register(Thread.CurrentThread.ManagedThreadId, batchNumber);

					foreach (var household in batches[batchNumber]) {
						//var index = 0;
						if ((household.Id % Global.Configuration.HouseholdSamplingRateOneInX == (Global.Configuration.HouseholdSamplingStartWithY - 1))) {
#if RELEASE
					try {
#endif
							if (_start == -1 || _end == -1 || _index == -1 || index++.IsBetween(_start, _end)) {
								var randomSeed = randoms[household.Id];
								var choiceModelRunner = ChoiceModelFactory.Get(household, randomSeed);

								choiceModelRunner.RunChoiceModels(batchNumber);
							}
#if RELEASE
					}
					catch (Exception e) {
						throw new ChoiceModelRunnerException(string.Format("An error occurred in ChoiceModelRunner for household {0}.", household.Id), e);
					}
#endif
						}

						if (!Global.Configuration.ShowRunChoiceModelsStatus) {
							continue;
						}

						current++;

						if (current != 1 && current != total && current % 1000 != 0) {
							continue;
						}

						var countf = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) : ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalPersonDays);
						var countStringf = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? "Household" : "Person";
						var ivcountf = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalInvalidAttempts);

						Console.Write("\r{0:p} {1}", (double) current / total,
							Global.Configuration.ReportInvalidPersonDays
								? string.Format("Total {0} Days: {1}, Total Invalid Attempts: {2}",
									countStringf, countf, ivcountf)
								: string.Format("Total {0} Days: {1}",
									countStringf, countf));
					}
				});
			var countg = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) : ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalPersonDays);
			var countStringg = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? "Household" : "Person";
			var ivcountg = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalInvalidAttempts);
			Console.Write("\r{0:p} {1}", (double) 1.0,
				Global.Configuration.ReportInvalidPersonDays
					? string.Format("Total {0} Days: {1}, Total Invalid Attempts: {2}",
						countStringg, countg, ivcountg)
					: string.Format("Total {0} Days: {1}",
						countStringg, countg));
			Console.WriteLine();
		}

		private static void BeginPerformHousekeeping() {
			if (!Global.Configuration.ShouldRunChoiceModels) {
				return;
			}
			var timer = new Timer("Performing housekeeping...");

			PerformHousekeeping();

			timer.Stop();
		}

		private static void PerformHousekeeping() {
			ChoiceProbabilityCalculator.Close();

			ChoiceModelHelper.WriteTimesModelsRun();
			ChoiceModelFactory.WriteCounters();
			ChoiceModelFactory.SignalShutdown();

			if (Global.Configuration.ShouldOutputTDMTripList) {
				ChoiceModelFactory.TDMTripListExporter.Dispose();
			}
		}

		public static void BeginUpdateShadowPricing() {
			if (!Global.Configuration.ShouldRunChoiceModels) {
				return;
			}

			var timer = new Timer("Updating shadow pricing...");

			ShadowPriceCalculator.CalculateAndWriteShadowPrices();
			ParkAndRideShadowPriceCalculator.CalculateAndWriteShadowPrices();

			timer.Stop();
		}

		private static void OverrideShouldRunRawConversion() {
			if (Global.Configuration.ShouldRunRawConversion) {
				return;
			}

			Global.Configuration.ShouldRunRawConversion = true;

			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("ShouldRunRawConversion in the configuration file has been overridden, a raw conversion is required.");
			}
		}

		private static void OverrideImport(Configuration configuration, Expression<Func<Configuration, bool>> expression) {
			var body = (MemberExpression) expression.Body;
			var property = (PropertyInfo) body.Member;
			var value = (bool) property.GetValue(configuration, null);

			if (value) {
				return;
			}

			property.SetValue(configuration, true, null);

			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("{0} in the configuration file has been overridden, an import is required.", property.Name);
			}
		}
	}
}