// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using DaySim.DomainModels.Factories;
using DaySim.Framework.Core;
using DaySim.Settings;
using NDesk.Options;
using Ninject;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

namespace DaySim {
    public static class Program {
        private static string _configurationPath;
        private static string _printFilePath;
        private static int _start = -1;
        private static int _end = -1;
        private static int _index = -1;
        private static bool _showHelp;
        private static bool _showVersion;
        private static string _overrides = "";

        private static void Main(string[] args) {
            try {
                var options = new OptionSet {
                    {"c|configuration=", "Path to configuration file", v => _configurationPath = v},
                    {"o|overrides=", "comma delimited name=value pairs to override configuration file values", v => _overrides = v},
                    {"p|printfile=", "Path to print file", v => _printFilePath = v},
                    {"s|start=", "Start index of household range", v => _start = int.Parse(v)},
                    {"e|end=", "End index of household range", v => _end = int.Parse(v)},
                    {"i|index=", "Cluser index", v => _index = int.Parse(v)},
                    {"v|version", "Show version information", v => _showVersion = v != null},
                    {"h|?|help", "Show help and syntax summary", v => _showHelp = v != null}
                };

                options.Parse(args);

                if (_showHelp) {
                    options.WriteOptionDescriptions(Console.Out);

                    Console.WriteLine();
                    Console.WriteLine("If you do not provide a configuration then the default is to use {0}, in the same directory as the executable.", ConfigurationManagerRSG.DEFAULT_CONFIGURATION_NAME);

                    Console.WriteLine();
                    Console.WriteLine("If you do not provide a printfile then the default is to create {0}, in the output directory.", PrintFile.DEFAULT_PRINT_FILENAME);

                    Console.WriteLine("Please press any key to exit");
                    if (Environment.UserInteractive) Console.ReadKey();

                    Environment.Exit(0);
                } else if (_showVersion) {
                    var assembly = typeof(Program).Assembly;
                    var assemblyName = assembly.GetName().Name;
                    var gitVersionInformationType = assembly.GetType(assemblyName + ".GitVersionInformation");

                    Console.WriteLine(string.Format("Version: {0}",
                        gitVersionInformationType.GetField("FullSemVer").GetValue(null)));
                    Console.WriteLine(string.Format("Branch: {0}",
                         gitVersionInformationType.GetField("BranchName").GetValue(null)));
                    Console.WriteLine(string.Format("Commit date: {0}",
                         gitVersionInformationType.GetField("CommitDate").GetValue(null)));
                    Console.WriteLine(string.Format("Commit Sha: {0}",
                         gitVersionInformationType.GetField("Sha").GetValue(null)));
                    //to get all fields
                    //var fields = gitVersionInformationType.GetFields();

                    //foreach (var field in fields)
                    //{
                    //    Console.WriteLine(string.Format("{0}: {1}", field.Name, field.GetValue(null)));
                    //}
                    Console.WriteLine("Please press any key to exit");
                    if (Environment.UserInteractive) Console.ReadKey();
                    Environment.Exit(0);
                }

                if (!File.Exists(_configurationPath)) {
                    throw new Exception("Configuration file '" + _configurationPath + "' does not exist.");
                }
                var configurationManager = new ConfigurationManagerRSG(_configurationPath);

                Global.Configuration = configurationManager.Open();

                overrideConfiguration(Global.Configuration);

                if (string.IsNullOrWhiteSpace(Global.Configuration.BasePath)) {
                    //issue #52 use configuration file folder as default basepath rather than arbitrary current working directory.
                    Global.Configuration.BasePath = Path.GetDirectoryName(Path.GetFullPath(_configurationPath));
                }

                if (string.IsNullOrWhiteSpace(_printFilePath)) {
                    _printFilePath = Global.GetOutputPath(PrintFile.DEFAULT_PRINT_FILENAME);
                }

                var settingsFactory = new SettingsFactory(Global.Configuration);
                Global.Settings = settingsFactory.Create();

                if (string.IsNullOrWhiteSpace(_printFilePath)) {
                    _printFilePath = Global.GetOutputPath(PrintFile.DEFAULT_PRINT_FILENAME);
                }
                _printFilePath.CreateDirectory(); //create printfile directory if needed
                Global.PrintFile = new PrintFile(_printFilePath, Global.Configuration);

                configurationManager.Write(Global.Configuration, Global.PrintFile);

                ParallelUtility.Init(Global.Configuration);

                Global.Kernel = new StandardKernel(new DaySimModule());

                //copy the configuration file into the output so we can tell if configuration changed before regression test called.
                var archiveConfigurationFilePath = Global.GetOutputPath("archive_" + Path.GetFileName(_configurationPath));
                archiveConfigurationFilePath.CreateDirectory(); //create output directory if needed
                File.Copy(_configurationPath, archiveConfigurationFilePath, /* overwrite */ true);

                if (Global.Configuration.PSRC || Global.Configuration.DVRPC || Global.Configuration.Nashville) {
                    throw new Exception("Region specific flag is set such as PSRC, DVRPC or Nashville. Use CustomizationDl instead to override behaviors");
                }

                var moduleFactory = new ModuleFactory(Global.Configuration);
                var modelModule = moduleFactory.Create();

                Global.Kernel.Load(modelModule);

                Engine.BeginProgram(_start, _end, _index);
                //Engine.BeginTestMode();
            } catch (Exception e) {
                string message = e.ToString();

                //even though Global.PrintFile.Dispose(); is called in Finally, it is useful to also
                //call it here because many times I would forget to close the output window and would be unable to delete outputs
                //in this odd case I wish I could put the finally before the catch
                if (Global.PrintFile != null) {
                    Global.PrintFile.WriteLine(message);
                    Global.PrintFile.Dispose();
                    Global.PrintFile = null;
                }
                Console.WriteLine();
                Console.Error.WriteLine(message);

                Console.WriteLine();
                Console.WriteLine("Please press any key to exit");
                if (Environment.UserInteractive) Console.ReadKey();

                Environment.Exit(2);
            } finally {
#if DEBUG
                string lockCounts = ParallelUtility.getLockCounts();
                Console.WriteLine(lockCounts);
                if (Global.PrintFile != null) {
                    Global.PrintFile.WriteLine(lockCounts);
                }
#endif
                if (Global.PrintFile != null) {
                    Global.PrintFile.Dispose();
                }
            }
            Environment.Exit(0);
        }

        private static void overrideConfiguration(object configuration) {
            //read possible overrides
            string[] nameValuePairs = _overrides.Trim().Split(',');
            // if (nameValuePairs.Length)
            if (nameValuePairs.Length > 0 && nameValuePairs[0].Trim().Length > 0) {
                Dictionary<string, string> keyValuePairs = nameValuePairs
               .Select(value => value.Split('='))
               .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());

                var type1 = configuration.GetType();
                foreach (KeyValuePair<string, string> entry in keyValuePairs) {
                    var property = type1.GetProperty(entry.Key, BindingFlags.Public | BindingFlags.Instance);

                    if (property == null) {
                        Console.WriteLine("WARNING: override key value pair ignored because key not found!: " + entry);
                        continue;
                    }

                    var type2 = property.PropertyType;

                    try {
                        if (type2 == typeof(char)) {
                            var b = Convert.ChangeType(entry.Value, typeof(byte));

                            property.SetValue(configuration, Convert.ChangeType(b, type2), null);
                        } else {
                            property.SetValue(configuration, Convert.ChangeType(entry.Value, type2), null);
                        }
                        Console.WriteLine("Configuration override applied: " + entry);
                    } catch {
                        var builder = new StringBuilder();

                        builder
                            .AppendFormat("Error overriding configuration file for entry {0}.", entry).AppendLine()
                            .AppendFormat("Cannot convert the value of \"{0}\" to the type of {1}.", entry.Value, type2.Name).AppendLine()
                            .AppendLine("Please ensure that the value is in the correct format for the given type.");

                        throw new Exception(builder.ToString());
                    }
                }
            }
        }   //end overrideConfiguration
    }   //end class Program
}   //end namespace
