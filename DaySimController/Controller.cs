// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using DaySim;
using DaySim.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Settings;
using Timer = DaySim.Framework.Core.Timer;

namespace DaySimController {
  public static class Controller {
    private static readonly List<Tuple<Process, RemoteMachine, Timer>> _processes = new List<Tuple<Process, RemoteMachine, Timer>>();
    private static readonly List<Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer>> _instances = new List<Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer>>();
    private static double _householdCount;

    public static void BeginProgram() {
      Timer timer = new Timer("Starting DaySim Controller...");

      Console.WriteLine("Run initial DaySim steps locally");

      SettingsFactory settingsFactory = new SettingsFactory(Global.Configuration);
      Global.Settings = settingsFactory.Create();

      ParallelUtility.Init(Global.Configuration);

      Engine.InitializeDaySim();
      Engine.BeginInitialize();
      Engine.BeginRunRawConversion();
      Engine.BeginImportData();
      Engine.BeginBuildIndexes();

      BeginRunRemoteProcesses();
      BeginMerge();
      BeginLoadData();

      Engine.BeginUpdateShadowPricing();
      if (Global.Configuration.RemoteCopySPFilesToRemoteMachines) {
        BeginCopyFilesToRemoteMachines();
      }

      timer.Stop("Total running time");
    }

    private static void BeginRunRemoteProcesses() {
      Timer timer = new Timer("Running remote batches of households using DaySim's s=, e=, i= arguments...");

      RunRemoteProcesses();

      timer.Stop();
    }

    private static void RunRemoteProcesses() {
      using (CountingReader reader = new CountingReader(new FileInfo(Global.GetInputPath(Global.Configuration.RawHouseholdPath)).OpenRead())) {
        while (reader.ReadLine() != null) {
          _householdCount++;
        }
      }

      _householdCount--;

      List<RemoteMachine> machines = RemoteMachine.GetAll();
      int range = (int)Math.Ceiling(_householdCount / machines.Count);

      for (int i = 0; i < machines.Count; i++) {
        int start = (range * i);
        int end = (int)Math.Min((range * i) + range - 1, _householdCount - 1);

        RemoteMachine machine = machines[i];

        //run a local remote DaySim session for debugging if desired
        if (Environment.MachineName.Equals(machine.Name, StringComparison.OrdinalIgnoreCase)) {
          Process process = new Process {
            StartInfo = {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = machine.Filename,
                            Arguments = machine.Arguments + " /s=" + start + " /e=" + end + " /i=" + i
                        }
          };

          process.Start();

          _processes.Add(Tuple.Create(process, machine, new Timer()));
        } else {

          //remote a remote DaySim session using WMI and RPC
          ConnectionOptions connectionOptions = new ConnectionOptions { Username = Global.Configuration.RemoteUsername, Password = Global.Configuration.RemotePassword };
          ManagementScope managementScope = new ManagementScope(string.Format(@"\\{0}\ROOT\CIMV2", machine.Name), connectionOptions);

          managementScope.Connect();

          ManagementClass processClass = new ManagementClass(managementScope, new ManagementPath("Win32_Process"), new ObjectGetOptions());
          ManagementBaseObject inParameters = processClass.GetMethodParameters("Create");

          inParameters["CurrentDirectory"] = machine.CurrentDirectory;
          inParameters["CommandLine"] = machine.CommandLine + " /s=" + start + " /e=" + end + " /i=" + i;

          ManagementBaseObject outParameters = processClass.InvokeMethod("Create", inParameters, null);

          if ((uint)outParameters.Properties["ReturnValue"].Value == 0) {
            _instances.Add(Tuple.Create(managementScope, outParameters, machine, new Timer()));
          }
        }
      }

      List<Thread> threads = new List<Thread>();

      foreach (Tuple<Process, RemoteMachine, Timer> process in _processes) {
        ParameterizedThreadStart start = new ParameterizedThreadStart(BeginLocalWatch);
        Thread thread = new Thread(start);

        thread.Start(process);
        threads.Add(thread);
      }

      foreach (Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer> instance in _instances) {
        ParameterizedThreadStart start = new ParameterizedThreadStart(BeginRemoteWatch);
        Thread thread = new Thread(start);

        thread.Start(instance);
        threads.Add(thread);
      }

      foreach (Thread thread in threads) {
        thread.Join();
      }
    }

    private static void BeginLocalWatch(object obj) {
      Tuple<Process, RemoteMachine, Timer> process = (Tuple<Process, RemoteMachine, Timer>)obj;

      Console.WriteLine(process.Item2.Name + ": " + process.Item1.StartInfo.FileName);

      LocalWatch(process);

      process.Item3.Stop(process.Item2.Name + ": " + "Running time");
    }

    private static void LocalWatch(Tuple<Process, RemoteMachine, Timer> process) {
      process.Item1.WaitForExit();
    }

    private static void BeginRemoteWatch(object obj) {
      Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer> instance = (Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer>)obj;

      Console.WriteLine(instance.Item3.Name + ": " + instance.Item3.CommandLine);

      RemoteWatch(instance);

      instance.Item4.Stop(instance.Item3.Name + ": " + "Running time");
    }

    private static void RemoteWatch(Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer> instance) {
      WqlEventQuery eventQuery = new WqlEventQuery("Select * From __InstanceDeletionEvent Within 1 Where TargetInstance ISA 'Win32_Process' AND TargetInstance.ProcessID = '" + instance.Item2["processId"] + "'");
      ManagementEventWatcher watcher = new ManagementEventWatcher(instance.Item1, eventQuery);

      bool stopped = false;

      while (!stopped) {
        ManagementBaseObject o = watcher.WaitForNextEvent();

        if (((ManagementBaseObject)o["TargetInstance"])["ProcessID"].ToString() == instance.Item2["processId"].ToString()) {
          stopped = true;
        }
      }

      watcher.Stop();
    }

    private static void BeginMerge() {
      Timer timer = new Timer("Merging each remote output file into a master/local output file...");

      Merge();

      timer.Stop();
    }

    private static void Merge() {
      bool householdHeader = false;
      bool personHeader = false;
      bool householdDayHeader = false;
      bool jointTourHeader = false;
      bool fullHalfTourHeader = false;
      bool partialHalfTourHeader = false;
      bool personDayHeader = false;
      bool tourHeader = false;
      bool tripHeader = false;
      bool tdmTripListHeader = false;

      List<RemoteMachine> machines = RemoteMachine.GetAll();

      for (int i = 0; i < machines.Count; i++) {
        RemoteMachine machine = machines[i];
        ConfigurationManagerRSG configurationManager = new ConfigurationManagerRSG(machine.ConfigurationPath.ToUncPath(machine.Name));
        Configuration configuration = configurationManager.Open();

        FileInfo household = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputHouseholdPath));
        AppendFile(household, new FileInfo(Global.GetOutputPath(configuration.OutputHouseholdPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref householdHeader);

        FileInfo person = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputPersonPath));
        AppendFile(person, new FileInfo(Global.GetOutputPath(configuration.OutputPersonPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref personHeader);

        FileInfo householdDay = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputHouseholdDayPath));
        AppendFile(householdDay, new FileInfo(Global.GetOutputPath(configuration.OutputHouseholdDayPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref householdDayHeader);

        if (!string.IsNullOrEmpty(configuration.OutputJointTourPath)) {
          FileInfo jointTour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputJointTourPath));
          AppendFile(jointTour, new FileInfo(Global.GetOutputPath(configuration.OutputJointTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref jointTourHeader);
        }

        if (!string.IsNullOrEmpty(configuration.OutputFullHalfTourPath)) {
          FileInfo fullHalfTour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputFullHalfTourPath));
          AppendFile(fullHalfTour, new FileInfo(Global.GetOutputPath(configuration.OutputFullHalfTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref fullHalfTourHeader);
        }

        if (!string.IsNullOrEmpty(configuration.OutputPartialHalfTourPath)) {
          FileInfo partialHalfTour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputPartialHalfTourPath));
          AppendFile(partialHalfTour, new FileInfo(Global.GetOutputPath(configuration.OutputPartialHalfTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref partialHalfTourHeader);
        }

        FileInfo personDay = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputPersonDayPath));
        AppendFile(personDay, new FileInfo(Global.GetOutputPath(configuration.OutputPersonDayPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref personDayHeader);

        FileInfo tour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputTourPath));
        AppendFile(tour, new FileInfo(Global.GetOutputPath(configuration.OutputTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref tourHeader);

        FileInfo trip = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputTripPath));
        AppendFile(trip, new FileInfo(Global.GetOutputPath(configuration.OutputTripPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref tripHeader);

        FileInfo tdmTripList = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath));
        AppendFile(tdmTripList, new FileInfo(Global.GetOutputPath(configuration.OutputTDMTripListPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref tdmTripListHeader);
      }
    }

    private static void BeginLoadData() {
      Timer timer = new Timer("Loading data...");

      LoadData();

      timer.Stop();
    }

    private static void LoadData() {
      ChoiceModelFactory.LoadData();
    }

    private static void BeginCopyFilesToRemoteMachines() {
      Timer timer = new Timer("Copying files to remote machines...");

      CopyFilesToRemoteMachines();

      timer.Stop();
    }

    private static void CopyFilesToRemoteMachines() {
      FileInfo shadowPricesFile = new FileInfo(Global.ShadowPricesPath);
      FileInfo parkAndRideShadowPricesFile = new FileInfo(Global.ParkAndRideShadowPricesPath);
      List<RemoteMachine> machines = RemoteMachine.GetAll();

      foreach (RemoteMachine machine in machines) {
        if (shadowPricesFile.Exists) {
          Console.WriteLine("Copying updated shadowPricesFile");
          shadowPricesFile.CopyTo((machine.CurrentDirectory + @"\" + Global.GetWorkingPath("shadow_prices.txt", true)).ToUncPath(machine.Name), true);
        }

        if (parkAndRideShadowPricesFile.Exists) {
          Console.WriteLine("Copying updated parkAndRideShadowPricesFile");
          parkAndRideShadowPricesFile.CopyTo((machine.CurrentDirectory + @"\" + Global.GetWorkingPath("park_and_ride_shadow_prices.txt", true)).ToUncPath(machine.Name), true);
        }
      }
    }

    private static void AppendFile(FileInfo local, FileInfo remote, ref bool appendHeader) {
      if (!remote.Exists) {
        return;
      }

      FileMode fileMode =
                appendHeader
                    ? FileMode.Open
                    : FileMode.Create;

      using (CountingReader reader = new CountingReader(remote.OpenRead())) {
        using (FileStream stream = local.Open(fileMode, FileAccess.Write, FileShare.Read)) {
          stream.Seek(0, SeekOrigin.End);

          using (StreamWriter writer = new StreamWriter(stream)) {
            bool firstLine = true;
            string line;

            while ((line = reader.ReadLine()) != null) {
              if (firstLine) {
                if (!appendHeader) {
                  writer.WriteLine(line);

                  appendHeader = true;
                }

                firstLine = false;

                continue;
              }

              writer.WriteLine(line);
            }
          }
        }
      }
    }
  }
}