// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using DaySim.Framework.Core;
using NDesk.Options;

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
      int exitCode = 0;
#if RELEASE //don't use try catch in release mode since wish to have Visual Studio debugger stop on unhandled exceptions
      try {
#endif
        OptionSet options = new OptionSet {
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

          if (Environment.UserInteractive && !Console.IsInputRedirected) {
            Console.WriteLine("Please press any key to exit");
            Console.ReadKey();
          }

          Environment.Exit(exitCode);
        } //end showHelp
        else if (_showVersion) {
          PrintVersion();

          if (Environment.UserInteractive && !Console.IsInputRedirected) {
            Console.WriteLine("Please press any key to exit");
            Console.ReadKey();
          }
          Environment.Exit(exitCode);
        } //end if _showVersion

        // Issue #164 Force use of decimal separator for numerical values
        bool needToChangeDecimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".";
        if (needToChangeDecimalSeparator) {
          //needs to be done very early before configuration file read in
          CultureInfo ci = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
          ci.NumberFormat.NumberDecimalSeparator = ".";
          Thread.CurrentThread.CurrentCulture = ci;
        }

        Console.WriteLine("Configuration file: " + _configurationPath);
        if (!File.Exists(_configurationPath)) {
          throw new Exception("Configuration file '" + _configurationPath + "' does not exist. You must pass in a DaySim configuration file with -c or --configuration");
        }
        ConfigurationManagerRSG configurationManager = new ConfigurationManagerRSG(_configurationPath);
        Global.Configuration = configurationManager.Open();

        Global.Configuration = configurationManager.OverrideConfiguration(Global.Configuration, _overrides);
        Global.Configuration = configurationManager.ProcessPath(Global.Configuration, _configurationPath);
        Global.PrintFile = configurationManager.ProcessPrintPath(Global.PrintFile, _printFilePath);

        string message = string.Format("--overrides = {0}", _overrides);
        Console.WriteLine(message);
        if (Global.PrintFile != null) {
          Global.PrintFile.WriteLine(message);
        }

        if (needToChangeDecimalSeparator) {
          //separator was already changed above but printfile was not ready so outputting warning message here.
          string decimalSeparatorMessage = string.Format("WARNING: default NumberDecimalSeparator is being overriden to use a period for the decimal point since DaySim requires this.");
          Console.WriteLine(decimalSeparatorMessage);
          if (Global.PrintFile != null) {
            Global.PrintFile.WriteLine(decimalSeparatorMessage);
          }
        }

        Engine.InitializeDaySim();

        if (Global.Configuration.LowerPrioritySetting) {
          var process = Process.GetCurrentProcess();
          process.PriorityClass = ProcessPriorityClass.BelowNormal;
        }

        Engine.BeginProgram(_start, _end, _index);
        //Engine.BeginTestMode();


#if RELEASE //don't use try catch in release mode since wish to have Visual Studio debugger stop on unhandled exceptions
      } catch (Exception e) {
        string message = e.ToString();

        //even though Global.PrintFile.Dispose(); is called in Finally, it is useful to also
        //call it here because many times I would forget to close the output window and would be unable to delete outputs
        //in this odd case I wish I could put the finally before the catch
        if (Global.PrintFile != null) {
          Global.PrintFile.WriteLine(message);
        }
        Console.WriteLine();
        Console.Error.WriteLine(message);

        if (Environment.UserInteractive && !(Console.IsInputRedirected || Console.IsOutputRedirected)) {
          Console.WriteLine();
          Console.WriteLine("Please press any key to exit");
          Console.ReadKey();
        }

        exitCode = 2;
      }
#endif
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
      Environment.Exit(exitCode);
    }

    public static void PrintVersion() {
      System.Reflection.Assembly assembly = typeof(Program).Assembly;
      string assemblyName = assembly.GetName().Name;
      Type gitVersionInformationType = assembly.GetType(assemblyName + ".GitVersionInformation");

      Console.WriteLine(string.Format("Version: {0}", gitVersionInformationType.GetField("FullSemVer").GetValue(null)));
      Console.WriteLine(string.Format("Branch: {0}", gitVersionInformationType.GetField("BranchName").GetValue(null)));
      Console.WriteLine(string.Format("Commit date: {0}", gitVersionInformationType.GetField("CommitDate").GetValue(null)));
      Console.WriteLine(string.Format("Commit Sha: {0}", gitVersionInformationType.GetField("Sha").GetValue(null)));
    }

  }   //end class Program
}   //end namespace
