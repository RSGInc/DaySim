// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using Daysim.DomainModels.Factories;
using Daysim.Framework.Core;
using Daysim.Settings;
using NDesk.Options;
using Ninject;

namespace Daysim {
	public static class Program {
		private static string _configurationPath;
		private static string _printFilePath;
		private static int _start = -1;
		private static int _end = -1;
		private static int _index = -1;
		private static bool _showHelp;

		private static void Main(string[] args) {
			try {
				var options = new OptionSet {
					{"c|configuration=", "Path to configuration file", v => _configurationPath = v},
					{"p|printfile=", "Path to print file", v => _printFilePath = v},
					{"s|start=", "Start index of household range", v => _start = int.Parse(v)},
					{"e|end=", "End index of household range", v => _end = int.Parse(v)},
					{"i|index=", "Cluser index", v => _index = int.Parse(v)},
					{"h|?|help", "Show help and syntax summary", v => _showHelp = v != null}
				};

				options.Parse(args);

				if (_showHelp) {
					options.WriteOptionDescriptions(Console.Out);

					Console.WriteLine();
					Console.WriteLine("If you do not provide a configuration then the default is to use {0}, in the same directory as the executable.", ConfigurationManager.DEFAULT_CONFIGURATION_NAME);

					Console.WriteLine();
					Console.WriteLine("If you do not provide a printfile then the default is to create {0}, in the same directory as the executable.", PrintFile.DEFAULT_PRINT_FILE_NAME);

					Console.WriteLine("Please press any key to exit");
					Console.ReadKey();

					Environment.Exit(0);
				}

				var configurationManager = new ConfigurationManager(_configurationPath);
				var configuration = configurationManager.Open();
				var settingsFactory = new SettingsFactory(configuration);
				var settings = settingsFactory.Create();
				var printFile = new PrintFile(_printFilePath, configuration);

				configurationManager.Write(configuration, printFile);

				ParallelUtility.Init(configuration);

				Global.Configuration = configuration;
				Global.Settings = settings;
				Global.PrintFile = printFile;
				Global.Kernel = new StandardKernel(new DaysimModule());

				var moduleFactory = new ModuleFactory(configuration);
				var modelModule = moduleFactory.Create();

				Global.Kernel.Load(modelModule);

				Engine.BeginProgram(_start, _end, _index);
				//Engine.BeginTestMode();
			}
			catch (Exception e) {
				Console.WriteLine();
				Console.WriteLine(e.GetBaseException().Message);

				Console.WriteLine();
				Console.WriteLine(e.StackTrace);

				Console.WriteLine();
				Console.WriteLine("Please press any key to exit");

				if (Global.PrintFile != null) {
					Global.PrintFile.WriteLine(e.GetBaseException().Message);
					Global.PrintFile.WriteLine();
					Global.PrintFile.WriteLine(e.StackTrace);
				}

				Console.ReadKey();

				Environment.Exit(2);
			}
			finally {
				if (Global.PrintFile != null) {
					Global.PrintFile.Dispose();
				}
			}
			Environment.Exit(0);
		}
	}
}