// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim;
using DaySim.ChoiceModels;
using DaySim.Framework.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using Timer = DaySim.Framework.Core.Timer;

namespace DaySimController {
    public static class Controller {
        private static readonly List<Tuple<Process, RemoteMachine, Timer>> _processes = new List<Tuple<Process, RemoteMachine, Timer>>();
        private static readonly List<Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer>> _instances = new List<Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer>>();
        private static double _householdCount;

        public static void BeginProgram() {
            var timer = new Timer("Starting DaySim Controller...");

            Engine.BeginInitialize();
            Engine.BeginRunRawConversion();
            Engine.BeginImportData();
            Engine.BeginBuildIndexes();

            BeginRunRemoteProcesses();
            BeginMerge();
            BeginLoadData();

            Engine.BeginUpdateShadowPricing();

            BeginCopyFilesToRemoteMachines();

            timer.Stop("Total running time");
        }

        private static void BeginRunRemoteProcesses() {
            var timer = new Timer("Running remote processes...");

            RunRemoteProcesses();

            timer.Stop();
        }

        private static void RunRemoteProcesses() {
            using (var reader = new CountingReader(new FileInfo(Global.GetInputPath(Global.Configuration.RawHouseholdPath)).OpenRead())) {
                while (reader.ReadLine() != null) {
                    _householdCount++;
                }
            }

            _householdCount--;

            var machines = RemoteMachine.GetAll();
            var range = (int)Math.Ceiling(_householdCount / machines.Count);

            for (var i = 0; i < machines.Count; i++) {
                var start = (range * i);
                var end = (int)Math.Min((range * i) + range - 1, _householdCount - 1);

                var machine = machines[i];

                if (Environment.MachineName.Equals(machine.Name, StringComparison.OrdinalIgnoreCase)) {
                    var process = new Process {
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
                    var connectionOptions = new ConnectionOptions { Username = Global.Configuration.RemoteUsername, Password = Global.Configuration.RemotePassword };
                    var managementScope = new ManagementScope(String.Format(@"\\{0}\ROOT\CIMV2", machine.Name), connectionOptions);

                    managementScope.Connect();

                    var processClass = new ManagementClass(managementScope, new ManagementPath("Win32_Process"), new ObjectGetOptions());
                    var inParameters = processClass.GetMethodParameters("Create");

                    inParameters["CurrentDirectory"] = machine.CurrentDirectory;
                    inParameters["CommandLine"] = machine.CommandLine + " /s=" + start + " /e=" + end + " /i=" + i;

                    var outParameters = processClass.InvokeMethod("Create", inParameters, null);

                    if ((uint)outParameters.Properties["ReturnValue"].Value == 0) {
                        _instances.Add(Tuple.Create(managementScope, outParameters, machine, new Timer()));
                    }
                }
            }

            var threads = new List<Thread>();

            foreach (var process in _processes) {
                var start = new ParameterizedThreadStart(BeginLocalWatch);
                var thread = new Thread(start);

                thread.Start(process);
                threads.Add(thread);
            }

            foreach (var instance in _instances) {
                var start = new ParameterizedThreadStart(BeginRemoteWatch);
                var thread = new Thread(start);

                thread.Start(instance);
                threads.Add(thread);
            }

            foreach (var thread in threads) {
                thread.Join();
            }
        }

        private static void BeginLocalWatch(object obj) {
            var process = (Tuple<Process, RemoteMachine, Timer>)obj;

            Console.WriteLine(process.Item2.Name + ": " + process.Item1.StartInfo.FileName);

            LocalWatch(process);

            process.Item3.Stop(process.Item2.Name + ": " + "Running time");
        }

        private static void LocalWatch(Tuple<Process, RemoteMachine, Timer> process) {
            process.Item1.WaitForExit();
        }

        private static void BeginRemoteWatch(object obj) {
            var instance = (Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer>)obj;

            Console.WriteLine(instance.Item3.Name + ": " + instance.Item3.CommandLine);

            RemoteWatch(instance);

            instance.Item4.Stop(instance.Item3.Name + ": " + "Running time");
        }

        private static void RemoteWatch(Tuple<ManagementScope, ManagementBaseObject, RemoteMachine, Timer> instance) {
            var eventQuery = new WqlEventQuery("Select * From __InstanceDeletionEvent Within 1 Where TargetInstance ISA 'Win32_Process' AND TargetInstance.ProcessID = '" + instance.Item2["processId"] + "'");
            var watcher = new ManagementEventWatcher(instance.Item1, eventQuery);

            var stopped = false;

            while (!stopped) {
                var o = watcher.WaitForNextEvent();

                if (((ManagementBaseObject)o["TargetInstance"])["ProcessID"].ToString() == instance.Item2["processId"].ToString()) {
                    stopped = true;
                }
            }

            watcher.Stop();
        }

        private static void BeginMerge() {
            var timer = new Timer("Merging...");

            Merge();

            timer.Stop();
        }

        private static void Merge() {
            var householdHeader = false;
            var personHeader = false;
            var householdDayHeader = false;
            var jointTourHeader = false;
            var fullHalfTourHeader = false;
            var partialHalfTourHeader = false;
            var personDayHeader = false;
            var tourHeader = false;
            var tripHeader = false;
            var tdmTripListHeader = false;

            var machines = RemoteMachine.GetAll();

            for (var i = 0; i < machines.Count; i++) {
                var machine = machines[i];
                var configurationManager = new ConfigurationManagerRSG(machine.ConfigurationPath.ToUncPath(machine.Name));
                var configuration = configurationManager.Open();

                var household = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputHouseholdPath));
                AppendFile(household, new FileInfo(Global.GetOutputPath(configuration.OutputHouseholdPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref householdHeader);

                var person = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputPersonPath));
                AppendFile(person, new FileInfo(Global.GetOutputPath(configuration.OutputPersonPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref personHeader);

                var householdDay = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputHouseholdDayPath));
                AppendFile(householdDay, new FileInfo(Global.GetOutputPath(configuration.OutputHouseholdDayPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref householdDayHeader);

                if (!string.IsNullOrEmpty(configuration.OutputJointTourPath)) {
                    var jointTour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputJointTourPath));
                    AppendFile(jointTour, new FileInfo(Global.GetOutputPath(configuration.OutputJointTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref jointTourHeader);
                }

                if (!string.IsNullOrEmpty(configuration.OutputFullHalfTourPath)) {
                    var fullHalfTour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputFullHalfTourPath));
                    AppendFile(fullHalfTour, new FileInfo(Global.GetOutputPath(configuration.OutputFullHalfTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref fullHalfTourHeader);
                }

                if (!string.IsNullOrEmpty(configuration.OutputPartialHalfTourPath)) {
                    var partialHalfTour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputPartialHalfTourPath));
                    AppendFile(partialHalfTour, new FileInfo(Global.GetOutputPath(configuration.OutputPartialHalfTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref partialHalfTourHeader);
                }

                var personDay = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputPersonDayPath));
                AppendFile(personDay, new FileInfo(Global.GetOutputPath(configuration.OutputPersonDayPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref personDayHeader);

                var tour = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputTourPath));
                AppendFile(tour, new FileInfo(Global.GetOutputPath(configuration.OutputTourPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref tourHeader);

                var trip = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputTripPath));
                AppendFile(trip, new FileInfo(Global.GetOutputPath(configuration.OutputTripPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref tripHeader);

                var tdmTripList = new FileInfo(Global.GetOutputPath(Global.Configuration.OutputTDMTripListPath));
                AppendFile(tdmTripList, new FileInfo(Global.GetOutputPath(configuration.OutputTDMTripListPath).ToUncPath(machine.Name).ToIndexedPath(i)), ref tdmTripListHeader);
            }
        }

        private static void BeginLoadData() {
            var timer = new Timer("Loading data...");

            LoadData();

            timer.Stop();
        }

        private static void LoadData() {
            ChoiceModelFactory.LoadData();
        }

        private static void BeginCopyFilesToRemoteMachines() {
            var timer = new Timer("Copying files to remote machines...");

            CopyFilesToRemoteMachines();

            timer.Stop();
        }

        private static void CopyFilesToRemoteMachines() {
            var shadowPricesFile = new FileInfo(Global.ShadowPricesPath);
            var parkAndRideShadowPricesFile = new FileInfo(Global.ParkAndRideShadowPricesPath);
            var machines = RemoteMachine.GetAll();

            foreach (var machine in machines) {
                if (shadowPricesFile.Exists) {
                    shadowPricesFile.CopyTo(Global.GetWorkingPath("shadow_prices.txt").ToUncPath(machine.Name), true);
                }

                if (parkAndRideShadowPricesFile.Exists) {
                    parkAndRideShadowPricesFile.CopyTo(Global.GetWorkingPath("park_and_ride_shadow_prices.txt").ToUncPath(machine.Name), true);
                }
            }
        }

        private static void AppendFile(FileInfo local, FileInfo remote, ref bool appendHeader) {
            if (!remote.Exists) {
                return;
            }

            var fileMode =
                appendHeader
                    ? FileMode.Open
                    : FileMode.Create;

            using (var reader = new CountingReader(remote.OpenRead())) {
                using (var stream = local.Open(fileMode, FileAccess.Write, FileShare.Read)) {
                    stream.Seek(0, SeekOrigin.End);

                    using (var writer = new StreamWriter(stream)) {
                        var firstLine = true;
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