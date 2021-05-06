﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using DaySim.AggregateLogsums;
using DaySim.ChoiceModels;
using DaySim.ChoiceModels.H;
using DaySim.DestinationParkingShadowPricing;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.DomainModels.Factories;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.ParkAndRideShadowPricing;
using DaySim.Sampling;
using DaySim.Settings;
using DaySim.ShadowPricing;
using HDF5DotNet;
using Timer = DaySim.Framework.Core.Timer;

namespace DaySim {
  public static class Engine {
    private static int _start = -1;
    private static int _end = -1;
    private static int _index = -1;
    private static Timer overallDaySimTimer = new Timer("DaySim...", true);

    //public static void BeginTestMode() {
    //  RandomUtility randomUtility = new RandomUtility();
    //  randomUtility.ResetUniform01(Global.Configuration.RandomSeed);
    //  randomUtility.ResetHouseholdSynchronization(Global.Configuration.RandomSeed);

    //  BeginInitialize();

    //  //RawConverter.RunTestMode();
    //}

    public static void BeginProgram(int start, int end, int index) {
      _start = start;
      _end = end;
      _index = index;

      RandomUtility randomUtility = new RandomUtility();
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

      //moved this up to load data dictionaries sooner
      ChoiceModelFactory.Initialize(Global.Configuration.ChoiceModelRunner);
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

      overallDaySimTimer.Stop("Total running time");
    }

    public static void BeginInitialize() {
      Timer timer = new Timer("Initializing...");

      Initialize();

      timer.Stop();
      overallDaySimTimer.Print();

    }

    public static void BeginTestInputs() {
      Timer timer = new Timer("Checking Input Validity...");
      InputTester.RunTest();
      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void Initialize() {
      if (Global.PrintFile != null) {
        Global.PrintFile.WriteLine("Application mode: {0}", Global.Configuration.IsInEstimationMode ? "Estimation" : "Simulation");

        if (Global.Configuration.IsInEstimationMode) {
          if (string.IsNullOrEmpty(Global.Configuration.EstimationModel)) {
            throw new Exception("IsInEstimationMode is true but EstimationModel is not set.");
          } else {
            /*
            wish to check that the passed in EstimationModel exists.
            The problem is that this may not be a full classname but a hybrid such as
            'ActumTourModeTimeModel' or 'HTourModeTimeModel' or 'TourModeTimeModel'
            */
            string middlePartOfNameSpace = Global.Configuration.EstimationModel.StartsWith("Actum") ? "Actum" : Global.Configuration.EstimationModel.StartsWith("H") ? "H" : "Default";
            string estimationModelClassName = string.Format("DaySim.ChoiceModels.{0}.Models.{1}", middlePartOfNameSpace, Global.Configuration.EstimationModel.Replace(middlePartOfNameSpace, ""));

            if (middlePartOfNameSpace != Global.Configuration.ChoiceModelRunner) {
              throw new Exception(string.Format("ChoiceModelRunner '{0}' does not match the middle part of the namespace of the EstimationModel '{1}' which was calculated to be '{2}.",
                Global.Configuration.ChoiceModelRunner, Global.Configuration.EstimationModel, middlePartOfNameSpace));
            }
            Type estimationModelType = Type.GetType(estimationModelClassName);
            if (estimationModelType == null) {
              throw new Exception(string.Format("EstimationModel '{0}' was expected to represent class '{1}' but that does not exist.", Global.Configuration.EstimationModel, estimationModelClassName));
            } else {
              Global.PrintFile.WriteLine(string.Format("EstimationModel '{0}' resolved to be Class {1}", Global.Configuration.EstimationModel, estimationModelType), true);

              FieldInfo choiceModelNameFieldInfo = estimationModelType.GetField("CHOICE_MODEL_NAME", BindingFlags.Public | BindingFlags.Static);
              if (choiceModelNameFieldInfo == null) {
                throw new Exception(string.Format("EstimationModel '{0}' class '{1}' does not have expected constant field 'CHOICE_MODEL_NAME'.", Global.Configuration.EstimationModel, estimationModelClassName));
              } else {
                object choiceModelNameValue = choiceModelNameFieldInfo.GetValue(null);
                if (choiceModelNameValue == null) {
                  throw new Exception(string.Format("EstimationModel '{0}' class '{1}' constant field 'CHOICE_MODEL_NAME' value could not be found.", Global.Configuration.EstimationModel, estimationModelClassName));
                } else if (choiceModelNameValue.ToString() != Global.Configuration.EstimationModel) {
                  throw new Exception(string.Format("EstimationModel '{0}' class '{1}' constant field CHOICE_MODEL_NAME's value is not identical to EstimationModel but is {2} instead.", Global.Configuration.EstimationModel, estimationModelClassName, choiceModelNameValue.ToString()));
                }
              }
            }
          }
        } //end EstimationModel checks

        bool usingASetRandomSeed = Global.Configuration.RandomSeed != Configuration.DefaultRandomSeedIfNotSet;
        if (usingASetRandomSeed) {
          Global.PrintFile.WriteLine(string.Format("randomSeed value of {0} WAS SET IN configuration file so runs will be repeatable.", Global.Configuration.RandomSeed), true);
        } else {
          Global.PrintFile.WriteLine(string.Format("randomSeed value WAS NOT SET in configuration file and has been dynamically set to {0}. Runs will not be repeatable unless the same seed is set.", Global.Configuration.RandomSeed), true);
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
          .ContainerDaySim.GetInstance<IPersistenceFactory<IParcel>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IParcelNode>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IParkAndRideNode>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IDestinationParkingNode>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITransitStopArea>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IZone>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHousehold>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPerson>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHouseholdDay>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPersonDay>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITour>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITrip>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IJointTour>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IFullHalfTour>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPartialHalfTour>>()
          .Initialize(Global.Configuration);
    }

    private static void InitializeWrapperFactories() {
      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IParcelCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IParcelNodeCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IParkAndRideNodeCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IDestinationParkingNodeCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IZoneCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IHouseholdCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IPersonCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IHouseholdDayCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IPersonDayCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<ITourCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<ITripCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IJointTourCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IFullHalfTourCreator>>()
          .Initialize(Global.Configuration);

      Global
          .ContainerDaySim
          .GetInstance<IWrapperFactory<IPartialHalfTourCreator>>()
          .Initialize(Global.Configuration);

      Global
         .ContainerDaySim
         .GetInstance<IWrapperFactory<ITransitStopAreaCreator>>()
         .Initialize(Global.Configuration);
    }

    private static void InitializeSkimFactories() {
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("text_ij", new TextIJSkimFileReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("visum-bin", new VisumSkimReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("bin", new BinarySkimFileReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("emme", new EMMEReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("hdf5", new HDF5ReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("cube", new CubeReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("transcad", new TranscadReaderCreator());
      Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().Register("omx", new OMXReaderCreator());
    }

    private static void InitializeSamplingFactories() {
      Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettings", new SamplingWeightsSettings());
      Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettingsSimple", new SamplingWeightsSettingsSimple());
      Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettingsUpdate", new SamplingWeightsSettingsUpdate());
      Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().Register("SamplingWeightsSettingsSACOG", new SamplingWeightsSettingsSACOG());
      Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().Initialize();
    }

    private static void InitializeAggregateLogsumsFactories() {
      Global.ContainerDaySim.GetInstance<AggregateLogsumsCalculatorFactory>().Register("AggregateLogsumCalculator", new AggregateLogsumsCalculatorCreator());
      Global.ContainerDaySim.GetInstance<AggregateLogsumsCalculatorFactory>().Register("AggregateLogsumCalculator_Actum", new ActumAggregateLogsumsCalculatorCreator());
      Global.ContainerDaySim.GetInstance<AggregateLogsumsCalculatorFactory>().Register("OtherAggregateLogsumCalculator", new OtherAggregateLogsumsCalculatorCreator());
      Global.ContainerDaySim.GetInstance<AggregateLogsumsCalculatorFactory>().Initialize();
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
      if (!string.IsNullOrEmpty(Global.Configuration.OutputJointTourPath)) {
        Global.Configuration.OutputJointTourPath = Global.GetOutputPath(Global.Configuration.OutputJointTourPath).ToIndexedPath(_index);
      }
      if (!string.IsNullOrEmpty(Global.Configuration.OutputFullHalfTourPath)) {
        Global.Configuration.OutputFullHalfTourPath = Global.GetOutputPath(Global.Configuration.OutputFullHalfTourPath).ToIndexedPath(_index);
      }
      if (!string.IsNullOrEmpty(Global.Configuration.OutputPartialHalfTourPath)) {
        Global.Configuration.OutputPartialHalfTourPath = Global.GetOutputPath(Global.Configuration.OutputPartialHalfTourPath).ToIndexedPath(_index);
      }
      Global.Configuration.OutputPersonDayPath = Global.GetOutputPath(Global.Configuration.OutputPersonDayPath).ToIndexedPath(_index);
      Global.Configuration.OutputTourPath = Global.GetOutputPath(Global.Configuration.OutputTourPath).ToIndexedPath(_index);
      Global.Configuration.OutputTripPath = Global.GetOutputPath(Global.Configuration.OutputTripPath).ToIndexedPath(_index);
      Global.Configuration.OutputTDMTripListPath = Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath).ToIndexedPath(_index);
    }

    private static void InitializeInput() {
      if (string.IsNullOrEmpty(Global.Configuration.InputDestinationParkingNodePath)) {
        Global.Configuration.InputDestinationParkingNodePath = Global.DefaultInputDestinationParkingNodePath;
      }
      if (string.IsNullOrEmpty(Global.Configuration.InputParkAndRideNodePath)) {
        Global.Configuration.InputParkAndRideNodePath = Global.DefaultInputParkAndRideNodePath;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputParcelNodePath)) {
        Global.Configuration.InputParcelNodePath = Global.DefaultInputParcelNodePath;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputParcelPath)) {
        Global.Configuration.InputParcelPath = Global.DefaultInputParcelPath;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputParcelPath)) {
        Global.Configuration.InputParcelPath = Global.DefaultInputParcelPath;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputZonePath)) {
        Global.Configuration.InputZonePath = Global.DefaultInputZonePath;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputTransitStopAreaPath)) {
        Global.Configuration.InputTransitStopAreaPath = Global.DefaultInputTransitStopAreaPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputHouseholdPath)) {
        Global.Configuration.InputHouseholdPath = Global.DefaultInputHouseholdPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputPersonPath)) {
        Global.Configuration.InputPersonPath = Global.DefaultInputPersonPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputHouseholdDayPath)) {
        Global.Configuration.InputHouseholdDayPath = Global.DefaultInputHouseholdDayPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputJointTourPath)) {
        Global.Configuration.InputJointTourPath = Global.DefaultInputJointTourPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputFullHalfTourPath)) {
        Global.Configuration.InputFullHalfTourPath = Global.DefaultInputFullHalfTourPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputPartialHalfTourPath)) {
        Global.Configuration.InputPartialHalfTourPath = Global.DefaultInputPartialHalfTourPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputPersonDayPath)) {
        Global.Configuration.InputPersonDayPath = Global.DefaultInputPersonDayPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputTourPath)) {
        Global.Configuration.InputTourPath = Global.DefaultInputTourPath;

      }

      if (string.IsNullOrEmpty(Global.Configuration.InputTripPath)) {
        Global.Configuration.InputTripPath = Global.DefaultInputTripPath;

      }

      FileInfo inputDestinationParkingNodeFile = Global.DestinationParkingNodeIsEnabled ? Global.GetInputPath(Global.Configuration.InputDestinationParkingNodePath).ToFile() : null;
      FileInfo inputParkAndRideNodeFile = Global.ParkAndRideNodeIsEnabled ? Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile() : null;
      FileInfo inputParcelNodeFile = Global.ParcelNodeIsEnabled ? Global.GetInputPath(Global.Configuration.InputParcelNodePath).ToFile() : null;
      FileInfo inputParcelFile = Global.GetInputPath(Global.Configuration.InputParcelPath).ToFile();
      FileInfo inputZoneFile = Global.GetInputPath(Global.Configuration.InputZonePath).ToFile();
      FileInfo inputHouseholdFile = Global.GetInputPath(Global.Configuration.InputHouseholdPath).ToFile();
      FileInfo inputPersonFile = Global.GetInputPath(Global.Configuration.InputPersonPath).ToFile();
      FileInfo inputHouseholdDayFile = Global.GetInputPath(Global.Configuration.InputHouseholdDayPath).ToFile();
      FileInfo inputJointTourFile = Global.GetInputPath(Global.Configuration.InputJointTourPath).ToFile();
      FileInfo inputFullHalfTourFile = Global.GetInputPath(Global.Configuration.InputFullHalfTourPath).ToFile();
      FileInfo inputPartialHalfTourFile = Global.GetInputPath(Global.Configuration.InputPartialHalfTourPath).ToFile();
      FileInfo inputPersonDayFile = Global.GetInputPath(Global.Configuration.InputPersonDayPath).ToFile();
      FileInfo inputTourFile = Global.GetInputPath(Global.Configuration.InputTourPath).ToFile();
      FileInfo inputTripFile = Global.GetInputPath(Global.Configuration.InputTripPath).ToFile();

      InitializeInputDirectories(inputDestinationParkingNodeFile, inputParkAndRideNodeFile, inputParcelNodeFile, inputParcelFile, inputZoneFile, inputHouseholdFile, inputPersonFile, inputHouseholdDayFile, inputJointTourFile, inputFullHalfTourFile, inputPartialHalfTourFile, inputPersonDayFile, inputTourFile, inputTripFile);

      if (Global.PrintFile == null) {
        return;
      }

      Global.PrintFile.WriteLine("Input files:");
      Global.PrintFile.IncrementIndent();

      Global.PrintFile.WriteFileInfo(inputDestinationParkingNodeFile, "Destination parking node is not enabled, input destination parking node file not set.");
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

    private static void InitializeInputDirectories(FileInfo inputDestinationParkingNodeFile, FileInfo inputParkAndRideNodeFile, FileInfo inputParcelNodeFile, FileInfo inputParcelFile, FileInfo inputZoneFile, FileInfo inputHouseholdFile, FileInfo inputPersonFile, FileInfo inputHouseholdDayFile, FileInfo inputJointTourFile, FileInfo inputFullHalfTourFile, FileInfo inputPartialHalfTourFile, FileInfo inputPersonDayFile, FileInfo inputTourFile, FileInfo inputTripFile) {

      if (inputDestinationParkingNodeFile != null) {
        inputDestinationParkingNodeFile.CreateDirectory();
      }

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

      bool override1 = (inputDestinationParkingNodeFile != null && !inputDestinationParkingNodeFile.Exists) || (inputParkAndRideNodeFile != null && !inputParkAndRideNodeFile.Exists) || (inputParcelNodeFile != null && !inputParcelNodeFile.Exists) || !inputParcelFile.Exists || !inputZoneFile.Exists || !inputHouseholdFile.Exists || !inputPersonFile.Exists;
      bool override2 = false;

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
      FileInfo workingDestinationParkingNodeFile = Global.DestinationParkingNodeIsEnabled ? Global.WorkingDestinationParkingNodePath.ToFile() : null;
      FileInfo workingParkAndRideNodeFile = Global.ParkAndRideNodeIsEnabled ? Global.WorkingParkAndRideNodePath.ToFile() : null;
      FileInfo workingParcelNodeFile = Global.ParcelNodeIsEnabled ? Global.WorkingParcelNodePath.ToFile() : null;
      FileInfo workingParcelFile = Global.WorkingParcelPath.ToFile();
      FileInfo workingZoneFile = Global.WorkingZonePath.ToFile();
      FileInfo workingHouseholdFile = Global.WorkingHouseholdPath.ToFile();
      FileInfo workingHouseholdDayFile = Global.WorkingHouseholdDayPath.ToFile();
      FileInfo workingJointTourFile = Global.WorkingJointTourPath.ToFile();
      FileInfo workingFullHalfTourFile = Global.WorkingFullHalfTourPath.ToFile();
      FileInfo workingPartialHalfTourFile = Global.WorkingPartialHalfTourPath.ToFile();
      FileInfo workingPersonFile = Global.WorkingPersonPath.ToFile();
      FileInfo workingPersonDayFile = Global.WorkingPersonDayPath.ToFile();
      FileInfo workingTourFile = Global.WorkingTourPath.ToFile();
      FileInfo workingTripFile = Global.WorkingTripPath.ToFile();

      InitializeWorkingDirectory();

      InitializeWorkingImports(workingDestinationParkingNodeFile, workingParkAndRideNodeFile, workingParcelNodeFile, workingParcelFile, workingZoneFile, workingHouseholdFile, workingPersonFile, workingHouseholdDayFile, workingJointTourFile, workingFullHalfTourFile, workingPartialHalfTourFile, workingPersonDayFile, workingTourFile, workingTripFile);

      if (Global.PrintFile == null) {
        return;
      }

      Global.PrintFile.WriteLine("Working files:");
      Global.PrintFile.IncrementIndent();

      Global.PrintFile.WriteFileInfo(workingDestinationParkingNodeFile, "Destination parking node is not enabled, working destination parking node file not set.");
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
      DirectoryInfo workingDirectory = new DirectoryInfo(Global.GetWorkingPath(""));

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteLine("Working directory: {0}", workingDirectory);
      }

      if (workingDirectory.Exists) {
        return;
      }

      workingDirectory.CreateDirectory();
      OverrideShouldRunRawConversion();
    }

    private static void InitializeWorkingImports(FileInfo workingDestinationParkingNodeFile, FileInfo workingParkAndRideNodeFile, FileInfo workingParcelNodeFile, FileInfo workingParcelFile, FileInfo workingZoneFile, FileInfo workingHouseholdFile, FileInfo workingPersonFile, FileInfo workingHouseholdDayFile, FileInfo workingJointTourFile, FileInfo workingFullHalfTourFile, FileInfo workingPartialHalfTourFile, FileInfo workingPersonDayFile, FileInfo workingTourFile, FileInfo workingTripFile) {
      if (Global.Configuration.ShouldRunRawConversion || (workingDestinationParkingNodeFile != null && !workingDestinationParkingNodeFile.Exists) || (workingParkAndRideNodeFile != null && !workingParkAndRideNodeFile.Exists) || (workingParcelNodeFile != null && !workingParcelNodeFile.Exists) || !workingParcelFile.Exists || !workingZoneFile.Exists || !workingHouseholdFile.Exists || !workingPersonFile.Exists) {
        if (workingDestinationParkingNodeFile != null) {
          OverrideImport(Global.Configuration, x => x.ImportDestinationParkingNodes);
        }

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

      Timer timer = new Timer("Running raw conversion...");

      if (Global.PrintFile != null) {
        Global.PrintFile.IncrementIndent();
      }

      RawConverter.Run();

      if (Global.PrintFile != null) {
        Global.PrintFile.DecrementIndent();
      }

      timer.Stop();
      overallDaySimTimer.Print();
    }

    public static void BeginImportData() {
      ImportParcels();
      ImportParcelNodes();
      ImportParkAndRideNodes();
      ImportDestinationParkingNodes();
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
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IParcel>>()
          .Importer
          .Import();
    }

    private static void ImportParcelNodes() {
      if (!Global.ParcelNodeIsEnabled || !Global.Configuration.ImportParcelNodes) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IParcelNode>>()
          .Importer
          .Import();
    }

    private static void ImportParkAndRideNodes() {
      if (!Global.ParkAndRideNodeIsEnabled || !Global.Configuration.ImportParkAndRideNodes) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IParkAndRideNode>>()
          .Importer
          .Import();

      if (!Global.StopAreaIsEnabled) {
        return;
      }

      Framework.DomainModels.Persisters.IPersisterReader<IParkAndRideNode> parkAndRideNodeReader =
                Global
                    .ContainerDaySim
                    .GetInstance<IPersistenceFactory<IParkAndRideNode>>()
                    .Reader;

      Global.ParkAndRideNodeMapping = new Dictionary<int, int>(parkAndRideNodeReader.Count);

      foreach (IParkAndRideNode parkAndRideNode in parkAndRideNodeReader) {
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
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITransitStopArea>>()
          .Importer
          .Import();
    }

    private static void ImportZones() {
      if (!Global.Configuration.ImportZones) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IZone>>()
          .Importer
          .Import();
    }

    private static void ImportDestinationParkingNodes() {
      if (!Global.DestinationParkingNodeIsEnabled || !Global.Configuration.ImportDestinationParkingNodes) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IDestinationParkingNode>>()
          .Importer
          .Import();

    }


    private static void ImportHouseholds() {
      if (!Global.Configuration.ImportHouseholds) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHousehold>>()
          .Importer
          .Import();
    }

    private static void ImportPersons() {
      if (!Global.Configuration.ImportPersons) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPerson>>()
          .Importer
          .Import();
    }

    private static void ImportHouseholdDays() {
      if (!Global.Configuration.ImportHouseholdDays) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHouseholdDay>>()
          .Importer
          .Import();
    }

    private static void ImportPersonDays() {
      if (!Global.Configuration.ImportPersonDays) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPersonDay>>()
          .Importer
          .Import();
    }

    private static void ImportTours() {
      if (!Global.Configuration.ImportTours) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITour>>()
          .Importer
          .Import();
    }

    private static void ImportTrips() {
      if (!Global.Configuration.ImportTrips) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITrip>>()
          .Importer
          .Import();
    }

    private static void ImportJointTours() {
      if (!Global.Configuration.ImportJointTours) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IJointTour>>()
          .Importer
          .Import();
    }

    private static void ImportFullHalfTours() {
      if (!Global.Configuration.ImportFullHalfTours) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IFullHalfTour>>()
          .Importer
          .Import();
    }

    private static void ImportPartialHalfTours() {
      if (!Global.Configuration.ImportPartialHalfTours) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPartialHalfTour>>()
          .Importer
          .Import();
    }

    public static void BeginBuildIndexes() {
      Timer timer = new Timer("Building indexes...");

      BuildIndexes();

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void BuildIndexes() {
      if (Global.ParcelNodeIsEnabled) {
        Global
            .ContainerDaySim
            .GetInstance<IPersistenceFactory<IParcelNode>>()
            .Reader
            .BuildIndex("parcel_fk", "Id", "NodeId");
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPerson>>()
          .Reader
          .BuildIndex("household_fk", "Id", "HouseholdId");

      if (!Global.Configuration.IsInEstimationMode) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHouseholdDay>>()
          .Reader
          .BuildIndex("household_fk", "Id", "HouseholdId");

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPersonDay>>()
          .Reader
          .BuildIndex("household_day_fk", "Id", "HouseholdDayId");

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITour>>()
          .Reader
          .BuildIndex("person_day_fk", "Id", "PersonDayId");

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<ITrip>>()
          .Reader
          .BuildIndex("tour_fk", "Id", "TourId");

      if (!Global.Settings.UseJointTours) {
        return;
      }

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IJointTour>>()
          .Reader
          .BuildIndex("household_day_fk", "Id", "HouseholdDayId");

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IFullHalfTour>>()
          .Reader
          .BuildIndex("household_day_fk", "Id", "HouseholdDayId");

      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IPartialHalfTour>>()
          .Reader
          .BuildIndex("household_day_fk", "Id", "HouseholdDayId");
    }

    private static void BeginLoadNodeIndex() {
      if (!Global.ParcelNodeIsEnabled || !Global.Configuration.UseShortDistanceNodeToNodeMeasures) {
        return;
      }

      Timer timer = new Timer("Loading node index...");

      LoadNodeIndex();

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void LoadNodeIndex() {
      FileInfo file = new FileInfo(Global.GetInputPath(Global.Configuration.NodeIndexPath));

      Global.ANodeId = new Dictionary<int, int>();
      List<int> aNodeFirstRecord = new List<int>();
      List<int> aNodeLastRecord = new List<int>();

      using (CountingReader reader = new CountingReader(file.OpenRead())) {
        reader.ReadLine();

        string line = null;
        int i = 0;
        try {
          while ((line = reader.ReadLine()) != null) {
            string[] tokens = line.Split(Global.Configuration.NodeIndexDelimiter);

            Global.ANodeId.Add(int.Parse(tokens[0]), i+1); //consistent with code in NodeToNodeDistance in ParcelWrapper
            aNodeFirstRecord.Add(int.Parse(tokens[1]));
            int lastRecord = int.Parse(tokens[2]);
            aNodeLastRecord.Add(lastRecord);
            if (lastRecord > Global.LastNodeDistanceRecord) {
              Global.LastNodeDistanceRecord = lastRecord;
            }

            i = i + 1;
          }
        } catch (FormatException e) {
          throw new Exception("Format problem in file '" + file.FullName + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
        }
      }

      Global.ANodeFirstRecord = aNodeFirstRecord.ToArray();
      Global.ANodeLastRecord = aNodeLastRecord.ToArray();
    }

    private static void BeginLoadNodeDistances() {
      if (!Global.ParcelNodeIsEnabled || !Global.Configuration.UseShortDistanceNodeToNodeMeasures) {
        return;
      }

      Global.InitializeNodeIndex();

      Timer timer = new Timer("Loading node distances...");

      switch (Global.Configuration.NodeDistanceReaderType) {
        case Configuration.NodeDistanceReaderTypes.HDF5:
          LoadNodeDistancesFromHDF5();
          break;
        case Configuration.NodeDistanceReaderTypes.TextOrBinary:
          if (Global.Configuration.NodeDistancesDelimiter == (char)0) {
            LoadNodeDistancesFromBinary();
          } else {
            LoadNodeDistancesFromText();
          }
          break;
        default:
          throw new Exception("Unhandled Global.Configuration.NodeDistanceReaderType: " + Global.Configuration.NodeDistanceReaderType);
          //break;
      }

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void BeginLoadNodeStopAreaDistances() {
      //mb moved this from Global to Engine
      if (!Global.StopAreaIsEnabled) {
        return;
      }
      if (string.IsNullOrEmpty(Global.Configuration.NodeStopAreaIndexPath)) {
        throw new ArgumentNullException("NodeStopAreaIndexPath");
      }

      Timer timer = new Timer("Loading node stop area distances...");
      string filename = Global.GetInputPath(Global.Configuration.NodeStopAreaIndexPath);
      using (StreamReader reader = File.OpenText(filename)) {
        InitializeParcelStopAreaIndex(reader);
      }

      timer.Stop();
      overallDaySimTimer.Print();
    }

    public static void InitializeParcelStopAreaIndex(TextReader reader) {
      //mb moved this from global to engine in order to use DaySim.ChoiceModels.ChoiceModelFactory
      //mb tried to change this code to set parcel first and last indices here instead of later, but did not work

      //var parcelIds = new List<int>();  
      List<int> stopAreaKeys = new List<int>();
      List<int> stopAreaIds = new List<int>();
      List<float> lengths = new List<float>(); /* raw values */
      List<float> distances = new List<float>(); /* lengths after division by Global.Settings.LengthUnitsPerFoot */

      // read header
      reader.ReadLine();

      string line;
      int lastParcelId = -1;
      IParcelWrapper parcel = null;
      int arrayIndex = 0;
      //start arrays at index 0 with dummy values, since valid indices start with 1
      //parcelIds.Add(0);
      stopAreaIds.Add(0);
      stopAreaKeys.Add(0);
      distances.Add(0F);
      lengths.Add(0F);

      while ((line = reader.ReadLine()) != null) {
        string[] tokens = line.Split(new[] { Global.Configuration.NodeStopAreaIndexPathDelimiter });

        arrayIndex++;
        int parcelId = int.Parse(tokens[0]);
        if (parcelId != lastParcelId) {
          //Console.WriteLine(parcelId);
          parcel = ChoiceModelFactory.Parcels[parcelId];
          parcel.FirstPositionInStopAreaDistanceArray = arrayIndex;
          parcel.StopAreaDistanceArrayPositionsSet = true;
          lastParcelId = parcelId;
        }
        parcel.LastPositionInStopAreaDistanceArray = arrayIndex;

        //parcelIds.Add(int.Parse(tokens[0]));
        int stopAreaId = int.Parse(tokens[1]);
        stopAreaKeys.Add(stopAreaId);
        //mb changed this array to use mapping of stop area ids 
        int stopAreaIndex = Global.TransitStopAreaMapping[stopAreaId];
        stopAreaIds.Add(stopAreaIndex);

        float length = float.Parse(tokens[2]);
        lengths.Add(length);
        float distance = (float)(length / Global.Settings.LengthUnitsPerFoot);
        distances.Add(distance);
      }

      //Global.ParcelStopAreaParcelIds = parcelIds.ToArray();
      Global.ParcelStopAreaStopAreaKeys = stopAreaKeys.ToArray();
      Global.ParcelStopAreaStopAreaIds = stopAreaIds.ToArray();
      Global.ParcelStopAreaLengths = lengths.ToArray();
      Global.ParcelStopAreaDistances = distances.ToArray();
    }

    private static void BeginLoadMicrozoneToBikeCarParkAndRideNodeDistances() {
      if (!Global.StopAreaIsEnabled || Global.Configuration.DataType != "Actum") {
        return;
      }
      if (string.IsNullOrEmpty(Global.Configuration.MicrozoneToParkAndRideNodeIndexPath)) {
        throw new ArgumentNullException("MicrozoneToParkAndRideNodeIndexPath");
      }

      Timer timer = new Timer("MicrozoneToParkAndRideNode distances...");
      string filename = Global.GetInputPath(Global.Configuration.MicrozoneToParkAndRideNodeIndexPath);
      using (StreamReader reader = File.OpenText(filename)) {
        InitializeMicrozoneToBikeCarParkAndRideNodeIndex(reader);
      }

      timer.Stop();
      overallDaySimTimer.Print();
    }

    public static void InitializeMicrozoneToBikeCarParkAndRideNodeIndex(TextReader reader) {

      //var parcelIds = new List<int>();  
      List<int> nodeSequentialIds = new List<int>();
      List<int> parkAndRideNodeIds = new List<int>();
      List<float> lengths = new List<float>(); /* raw values */
      List<float> distances = new List<float>(); /* lengths after division by Global.Settings.LengthUnitsPerFoot */

      // read header
      reader.ReadLine();

      string line;
      int lastParcelId = -1;
      IActumParcelWrapper parcel = null;
      int arrayIndex = 0;
      //start arrays at index 0 with dummy values, since valid indices start with 1
      //parcelIds.Add(0);
      nodeSequentialIds.Add(0);
      parkAndRideNodeIds.Add(0);
      distances.Add(0F);
      lengths.Add(0F);

      while ((line = reader.ReadLine()) != null) {
        string[] tokens = line.Split(new[] { Global.Configuration.MicrozoneToParkAndRideNodeIndexDelimiter });

        arrayIndex++;
        int parcelId = int.Parse(tokens[0]);
        if (parcelId != lastParcelId) {
          //Console.WriteLine(parcelId);
          parcel = (IActumParcelWrapper)ChoiceModelFactory.Parcels[parcelId];
          parcel.FirstPositionInParkAndRideNodeDistanceArray = arrayIndex;
          parcel.ParkAndRideNodeDistanceArrayPositionsSet = true;
          lastParcelId = parcelId;
        }
        parcel.LastPositionInParkAndRideNodeDistanceArray = arrayIndex;

        //parcelIds.Add(int.Parse(tokens[0]));
        int parkAndRideNodeId = int.Parse(tokens[1]);
        parkAndRideNodeIds.Add(parkAndRideNodeId);
        //mb changed this array to use mapping of stop area ids 
        int nodeSequentialIndex = Global.ParkAndRideNodeMapping[parkAndRideNodeId];
        nodeSequentialIds.Add(nodeSequentialIndex);
        float length = float.Parse(tokens[2]);
        lengths.Add(length);
        float distance = (float)(length / Global.Settings.LengthUnitsPerFoot);
        distances.Add(distance);
      }

      Global.ParcelParkAndRideNodeIds = parkAndRideNodeIds.ToArray();
      Global.ParcelParkAndRideNodeSequentialIds = nodeSequentialIds.ToArray();
      Global.ParcelToBikeCarParkAndRideNodeLength = lengths.ToArray();
      Global.ParcelToBikeCarParkAndRideNodeDistance = distances.ToArray();

    }


    private static void LoadNodeDistancesFromText() {
      FileInfo file = new FileInfo(Global.GetInputPath(Global.Configuration.NodeDistancesPath));

      using (CountingReader reader = new CountingReader(file.OpenRead())) {
        Global.NodePairBNodeId = new int[Global.LastNodeDistanceRecord];
        Global.NodePairDistance = new ushort[Global.LastNodeDistanceRecord];

        reader.ReadLine();

        string line;

        int i = 0;
        while ((line = reader.ReadLine()) != null) {
          string[] tokens = line.Split(Global.Configuration.NodeDistancesDelimiter);

          int aNodeId = int.Parse(tokens[0]);
          Global.NodePairBNodeId[i] = int.Parse(tokens[1]);
          double rdist = double.Parse(tokens[2]);
          int distance = (int)Math.Round(rdist);
          Global.NodePairDistance[i] = (ushort)Math.Min(distance, ushort.MaxValue);

          i++;
        }
      }
    }


    private static void LoadNodeDistancesFromBinary() {
      FileInfo file = new FileInfo(Global.GetInputPath(Global.Configuration.NodeDistancesPath));

      using (BinaryReader reader = new BinaryReader(file.OpenRead())) {
        Global.NodePairBNodeId = new int[file.Length / 8];
        Global.NodePairDistance = new ushort[file.Length / 8];

        int i = 0;
        long length = reader.BaseStream.Length;
        while (reader.BaseStream.Position < length) {
          Global.NodePairBNodeId[i] = reader.ReadInt32();

          int distance = reader.ReadInt32();

          Global.NodePairDistance[i] = (ushort)Math.Min(distance, ushort.MaxValue);

          i++;
        }
      }
    }

    private static void LoadNodeDistancesFromHDF5() {
      H5FileId file = H5F.open(Global.GetInputPath(Global.Configuration.NodeDistancesPath),
                H5F.OpenMode.ACC_RDONLY);

      // Read nodes
      H5DataSetId nodes = H5D.open(file, "node");
      H5DataSpaceId dspace = H5D.getSpace(nodes);

      long numNodes = H5S.getSimpleExtentDims(dspace)[0];
      Global.NodePairBNodeId = new int[numNodes];
      H5Array<int> wrapArray = new H5Array<int>(Global.NodePairBNodeId);

      H5DataTypeId dataType = new H5DataTypeId(H5T.H5Type.NATIVE_INT);

      H5D.read(nodes, dataType, wrapArray);

      H5S.close(dspace);
      H5D.close(nodes);

      // Read distances
      H5DataSetId dist = H5D.open(file, "distance");
      dspace = H5D.getSpace(nodes);

      Global.NodePairDistance = new ushort[numNodes];
      H5Array<ushort> distArray = new H5Array<ushort>(Global.NodePairDistance);

      dataType = new H5DataTypeId(H5T.H5Type.NATIVE_SHORT);

      H5D.read(nodes, dataType, distArray);

      H5S.close(dspace);
      H5D.close(dist);

      // All done
      H5F.close(file);
    }

    private static void BeginLoadRoster() {
      Timer timer = new Timer("Loading roster...");

      LoadRoster();

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void LoadRoster() {
      Framework.DomainModels.Persisters.IPersisterReader<IZone> zoneReader =
                Global
                    .ContainerDaySim
                    .GetInstance<IPersistenceFactory<IZone>>()
                    .Reader;

      Dictionary<int, int> zoneMapping = new Dictionary<int, int>(zoneReader.Count);

      foreach (IZone zone in zoneReader) {
        zoneMapping.Add(zone.Key, zone.Id);
      }

      Global.TransitStopAreaMapping = new Dictionary<int, int>();

      if (Global.Configuration.ImportTransitStopAreas) {
        Framework.DomainModels.Persisters.IPersisterReader<ITransitStopArea> transitStopAreaReader =
                    Global
                        .ContainerDaySim
                        .GetInstance<IPersistenceFactory<ITransitStopArea>>()
                        .Reader;

        ITransitStopAreaCreator transitStopAreaCreator =
                    Global
                        .ContainerDaySim
                        .GetInstance<IWrapperFactory<ITransitStopAreaCreator>>()
                        .Creator;

        Global.TransitStopAreaDictionary = new Dictionary<int, ITransitStopAreaWrapper>();
        foreach (ITransitStopArea transitStopArea in transitStopAreaReader) {
          ITransitStopAreaWrapper stopArea = transitStopAreaCreator.CreateWrapper(transitStopArea);
          int id = stopArea.Id;
          Global.TransitStopAreaDictionary.Add(id, stopArea);
        }
        Global.TransitStopAreas = Global.TransitStopAreaDictionary.Values;

        Global.TransitStopAreaMapping = new Dictionary<int, int>(transitStopAreaReader.Count);

        foreach (ITransitStopArea transitStopArea in transitStopAreaReader) {
          Global.TransitStopAreaMapping.Add(transitStopArea.Key, transitStopArea.Id);
        }
      }

      Global.MicrozoneMapping = new Dictionary<int, int>();

      if (Global.Configuration.UseMicrozoneSkims || Global.Configuration.UseMicrozoneSkimsForBikeMode || Global.Configuration.UseMicrozoneSkimsForWalkMode) {
        Framework.DomainModels.Persisters.IPersisterReader<IParcel> microzoneReader =
                    Global
                        .ContainerDaySim
                        .GetInstance<IPersistenceFactory<IParcel>>()
                        .Reader;

        Global.MicrozoneMapping = new Dictionary<int, int>(microzoneReader.Count);

        int mzSequence = 0;
        foreach (IParcel microzone in microzoneReader) {
          Global.MicrozoneMapping.Add(microzone.Id, mzSequence++);
        }

      }

      ImpedanceRoster.Initialize(zoneMapping, Global.TransitStopAreaMapping, Global.MicrozoneMapping);
    }

    private static void BeginCalculateAggregateLogsums(IRandomUtility randomUtility) {
      Timer timer = new Timer("Calculating aggregate logsums...");

      IAggregateLogsumsCalculator calculator = Global.ContainerDaySim.GetInstance<AggregateLogsumsCalculatorFactory>().AggregateLogsumCalculatorCreator.Create();
      calculator.Calculate(randomUtility);

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void BeginOutputAggregateLogsums() {
      if (!Global.Configuration.ShouldOutputAggregateLogsums) {
        return;
      }

      Timer timer = new Timer("Outputting aggregate logsums...");

      AggregateLogsumsExporter.Export(Global.GetOutputPath(Global.Configuration.OutputAggregateLogsumsPath));

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void BeginCalculateSamplingWeights() {
      Timer timer = new Timer("Calculating sampling weights...");

      SamplingWeightsCalculator.Calculate("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, 180);

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void BeginOutputSamplingWeights() {
      if (!Global.Configuration.ShouldOutputSamplingWeights) {
        return;
      }

      Timer timer = new Timer("Outputting sampling weights...");

      SamplingWeightsExporter.Export(Global.GetOutputPath(Global.Configuration.OutputSamplingWeightsPath));

      timer.Stop();
      overallDaySimTimer.Print();
    }

    private static void BeginRunChoiceModels(IRandomUtility randomUtility) {
      if (!Global.Configuration.ShouldRunChoiceModels) {
        return;
      }

      Timer timer = new Timer("Running choice models...");

      RunChoiceModels(randomUtility);

      timer.Stop();
      overallDaySimTimer.Print();
    }

    public static int GetNumberOfChoiceModelThreads() {
      int numberOfChoiceModelThreads;

      if (Global.Configuration.IsInEstimationMode || Global.Configuration.ChoiceModelDebugMode) {
        numberOfChoiceModelThreads = 1;
      } else {
        numberOfChoiceModelThreads = ParallelUtility.NThreads;
      }
      return numberOfChoiceModelThreads;
    }

    private static void RunChoiceModels(IRandomUtility randomUtility) {
      int current = 0;
      int total =
                Global
                    .ContainerDaySim
                    .GetInstance<IPersistenceFactory<IHousehold>>()
                    .Reader
                    .Count;

      if (Global.Configuration.HouseholdSamplingRateOneInX < 1) {
        Global.Configuration.HouseholdSamplingRateOneInX = 1;
      }
      Debug.Assert(Global.Configuration.HouseholdSamplingStartWithY <= Global.Configuration.HouseholdSamplingRateOneInX, "Error: Global.Configuration.HouseholdSamplingStartWithY (" + Global.Configuration.HouseholdSamplingStartWithY + ") must be less than or equal to Global.Configuration.HouseholdSamplingRateOneInX (" + Global.Configuration.HouseholdSamplingRateOneInX + ") or no models will be run!");
      ChoiceModelFactory.Initialize(Global.Configuration.ChoiceModelRunner, false);

      int numberOfChoiceModelThreads = GetNumberOfChoiceModelThreads();

      Dictionary<int, int> householdRandomValues = new Dictionary<int, int>();

      List<IHousehold>[] threadHouseholds = new List<IHousehold>[numberOfChoiceModelThreads];

      for (int i = 0; i < numberOfChoiceModelThreads; i++) {
        threadHouseholds[i] = new List<IHousehold>();
      }

      int overallHouseholdIndex = 0;
      int addedHousehouldCounter = 0;
      foreach (IHousehold household in Global.ContainerDaySim.GetInstance<IPersistenceFactory<IHousehold>>().Reader) {
        int nextRandom = randomUtility.GetNext();  //always get next random, even if won't be used so behavior identical with DaySimController and usual 
        if ((household.Id % Global.Configuration.HouseholdSamplingRateOneInX) == (Global.Configuration.HouseholdSamplingStartWithY - 1)) {
          if (_start == -1 || _end == -1 || _index == -1 || overallHouseholdIndex.IsBetween(_start, _end)) {
            householdRandomValues[household.Id] = nextRandom;
            int threadIndex = addedHousehouldCounter++ % numberOfChoiceModelThreads;

            threadHouseholds[threadIndex].Add(household);
          }
        }   //end if household being sampled
        overallHouseholdIndex++;
      }   //end foreach household

      //do not use Parallel.For because it may close and open new threads. Want steady threads since I am using thread local storage in Parallel.Utility
      ParallelUtility.AssignThreadIndex(numberOfChoiceModelThreads);
      List<Thread> threads = new List<Thread>();
      int displayInterval = Math.Min(1000, Math.Max(1, addedHousehouldCounter / 100));
      for (int threadIndex = 0; threadIndex < numberOfChoiceModelThreads; ++threadIndex) {
        Thread myThread = new Thread(new ThreadStart(delegate {
          //retrieve threadAssignedIndexIndex so can see logging output
          int threadAssignedIndex = ParallelUtility.threadLocalAssignedIndex.Value;
          List<IHousehold> currentThreadHouseholds = threadHouseholds[threadAssignedIndex];
          Global.PrintFile.WriteLine("For threadAssignedIndex: " + threadAssignedIndex + " there are " + string.Format("{0:n0}", currentThreadHouseholds.Count) + " households", writeToConsole: true);
          foreach (IHousehold household in currentThreadHouseholds) {
#if RELEASE //don't use try catch in release mode since wish to have Visual Studio debugger stop on unhandled exceptions
            try {
#endif
            int randomSeed = householdRandomValues[household.Id];
            IChoiceModelRunner choiceModelRunner = ChoiceModelFactory.Get(household, randomSeed);

            choiceModelRunner.RunChoiceModels();

            if (Global.Configuration.ShowRunChoiceModelsStatus) {
              if ((current % displayInterval) == 0) {
                int countLocal = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) : ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalPersonDays);
                //Actum and Default differ in that one Actum counts TotalHouseholdDays and Default counts TotalPersonDays
                string countStringLocal = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? "Household" : "Person";

                int ivcountLocal = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalInvalidAttempts);

                Console.Write(string.Format("\r{0:p}", (double)current / addedHousehouldCounter) +
                    string.Format(" Household: {0:n0}/{1:n0} Total {2} Days: {3:n0} ", current, addedHousehouldCounter, countStringLocal, countLocal) +
                    (Global.Configuration.ReportInvalidPersonDays
                        ? string.Format("Total Invalid Attempts: {0:n0}",
                            ivcountLocal)
                        : ""));
              }   //if outputting progress to console

              //WARNING: not threadsafe. It doesn't matter much though because this is only used for console output.
              //because of multithreaded issues may see skipped outputs or duplicated outputs. Could use Interlocked.Increment(ref threadsSoFarIndex) but not worth locking cost
              current++;
            }   //end if ShowRunChoiceModelsStatus
#if RELEASE
            } catch (Exception e) {
              throw new DaySim.Framework.Exceptions.ChoiceModelRunnerException(string.Format("An error occurred in ChoiceModelRunner for household {0}.", household.Id), e);
            }
#endif
          }   //end household loop for this threadAssignedIndex
        })) {
          Name = "ChoiceModelRunner_" + (threadIndex + 1)
        };    //end creating Thread and ThreadStart
        threads.Add(myThread);
      }   //end threads loop

      threads.ForEach(t => t.Start());
      threads.ForEach(t => t.Join());
      ParallelUtility.DisposeThreadIndex();
      int count = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) : ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalPersonDays);
      string countString = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalHouseholdDays) > 0 ? "Household" : "Person";
      int ivcount = ChoiceModelFactory.GetTotal(ChoiceModelFactory.TotalInvalidAttempts);
      Console.Write(string.Format("\r{0:p}", 1.0) +
          string.Format(" Household: {0:n0}/{1:n0} Total {2} Days: {3:n0} ", addedHousehouldCounter, addedHousehouldCounter, countString, count) +
          (Global.Configuration.ReportInvalidPersonDays
              ? string.Format("Total Invalid Attempts: {0:n0}",
                  ivcount)
              : ""));
      Console.WriteLine();
    }

    private static void BeginPerformHousekeeping() {
      if (!Global.Configuration.ShouldRunChoiceModels) {
        return;
      }
      Timer timer = new Timer("Performing housekeeping...");

      PerformHousekeeping();

      timer.Stop();
      overallDaySimTimer.Print();
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

      Timer timer = new Timer("Updating shadow pricing...");

      ShadowPriceCalculator.CalculateAndWriteShadowPrices();
      ParkAndRideShadowPriceCalculator.CalculateAndWriteShadowPrices();
      DestinationParkingShadowPriceCalculator.CalculateAndWriteShadowPrices();

      timer.Stop();
      overallDaySimTimer.Print();
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
      MemberExpression body = (MemberExpression)expression.Body;
      PropertyInfo property = (PropertyInfo)body.Member;
      bool value = (bool)property.GetValue(configuration, null);

      if (value) {
        return;
      }

      property.SetValue(configuration, true, null);

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteLine("{0} in the configuration file has been overridden, an import is required.", property.Name);
      }
    }

    public static void InitializeDaySim() {
      SettingsFactory settingsFactory = new SettingsFactory(Global.Configuration);
      Global.Settings = settingsFactory.Create();

      ParallelUtility.Init(Global.Configuration);

      //creating the DaySimModule does the non-model specific the SimpleInjector dependency injection registration
      new DaySimModule();
      //use the ModuleFactory to load the DaysimModule and the ModelModule (which could be Actum)
      // which does the SimpleInjector dependency injection registration
      ModuleFactory moduleFactory = new ModuleFactory(Global.Configuration);
      moduleFactory.Load();

      //after all dependency injection established, verify
      Global.ContainerDaySim.Verify();

      int totalCustomizationFlagsSet = Convert.ToInt32(Global.Configuration.Copenhagen)
        + Convert.ToInt32(Global.Configuration.DVRPC)
        + Convert.ToInt32(Global.Configuration.Fresno)
        + Convert.ToInt32(Global.Configuration.JAX)
        + Convert.ToInt32(Global.Configuration.Nashville)
        + Convert.ToInt32(Global.Configuration.PSRC)
        + Convert.ToInt32(Global.Configuration.SFCTA)
        + Convert.ToInt32(Global.Configuration.BKR);

      if (!string.IsNullOrEmpty(Global.Configuration.CustomizationDll) && (totalCustomizationFlagsSet != 0)) {
        throw new Exception("Region specific flag is set such as Copenhagen , DVRPC, Fresno, JAX, Nashville, PSRC or SFCTA but CustomizationDll is already set to: " + Global.Configuration.CustomizationDll);
      } else if (totalCustomizationFlagsSet > 1) {
        throw new Exception("More than one region specific flag such as Copenhagen, DVRPC, JAX, Nashville, PSRC, or SFCTA was specified in configuration file.");
      } else if (Global.Configuration.Copenhagen) {
        Global.Configuration.CustomizationDll = "Copenhagen.dll";
      } else if (Global.Configuration.DVRPC) {
        Global.Configuration.CustomizationDll = "DVRPC.dll";
      } else if (Global.Configuration.Fresno) {
        Global.Configuration.CustomizationDll = "Fresno.dll";
      } else if (Global.Configuration.JAX) {
        Global.Configuration.CustomizationDll = "JAX.dll";
      } else if (Global.Configuration.Nashville) {
        Global.Configuration.CustomizationDll = "Nashville.dll";
      } else if (Global.Configuration.PSRC) {
        Global.Configuration.CustomizationDll = "PSRC.dll";
        Console.WriteLine("PSRC.dll is loaded");
      } else if (Global.Configuration.SFCTA) {
        Global.Configuration.CustomizationDll = "SFCTA.dll";
      } else if (Global.Configuration.BKR) {
        Global.Configuration.CustomizationDll = "BKR.dll";
        Console.WriteLine("BKR.dll is loaded");
      }
    }
  }
}
