// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using NDesk.Options;
using System;
using System.IO;
using DaySim.Settings;

namespace DaySimController {
    public static class Program {
        private static string _configurationPath;
        private static string _printFilePath;
        private static bool _showHelp;

        private static void Main(string[] args) {
            int exitCode = 0;

            var options = new OptionSet {
                {"c|configuration=", "Path to configuration file", v => _configurationPath = v},
                {"p|printfile=", "Path to print file", v => _printFilePath = v},
                {"h|?|help", "Show help and syntax summary", v => _showHelp = v != null}
            };

            options.Parse(args);

            if (_showHelp) {
                options.WriteOptionDescriptions(Console.Out);

                Console.WriteLine();
                Console.WriteLine("DaySimController requires these additional properties:");
                Console.WriteLine("  RemoteUsername=your_username");
                Console.WriteLine("  RemotePassword=your_password");
                Console.WriteLine("  RemoteMachines=pormdlppw01|E:\\test\\Daysim.exe|/c=config.properties|/p=dclog.txt,sdmdlppw01|E:\\test\\Daysim.exe|/c=config.properties|/p=dclog.txt");
                Console.WriteLine("  RemoteCopySPFilesToRemoteMachines=false");
                
                Console.WriteLine();
                
                Console.WriteLine("All machines, including master, need the exact same folder and file setup");
                Console.WriteLine("This includes inputs since inputs are not copied from the master to the slaves");
                Console.WriteLine("Each machine's folder needs to be shared so DaySim can access it remotely");
                Console.WriteLine("Exceptions, or hanging, is probably due to configuration/path issues");
                Console.WriteLine("It is best to setup with the simplest possible file/folder setup");
                Console.WriteLine("Windows RPC server must be enabled on the remote machines");

                Console.WriteLine();
                Console.WriteLine("If you do not provide a configuration then the default is to use {0}, in the same directory as the executable.", ConfigurationManagerRSG.DEFAULT_CONFIGURATION_NAME);

                Console.WriteLine();
                Console.WriteLine("If you do not provide a printfile then the default is to create {0}, in the same directory as the executable.", PrintFile.DEFAULT_PRINT_FILENAME);

                Console.WriteLine();
                Console.WriteLine("Please press any key to exit");
                Console.ReadKey();

                Environment.Exit(exitCode);
            }
            
            Console.WriteLine("Configuration file: " + _configurationPath);
            if (!File.Exists(_configurationPath)) {
                throw new Exception("Configuration file '" + _configurationPath + "' does not exist. You must pass in a DaySim configuration file with -c or --configuration");
            }

            var configurationManager = new ConfigurationManagerRSG(_configurationPath);
            Global.Configuration = configurationManager.Open();
            Global.Configuration = configurationManager.ProcessPath(Global.Configuration, _configurationPath);
            Global.PrintFile = configurationManager.ProcessPrintPath(Global.PrintFile, _printFilePath);

            Controller.BeginProgram();
        }

    }
}