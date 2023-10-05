﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaySim.Framework.Core;
using DaySim.Framework.Exceptions;

namespace DaySim.Framework.Roster {
  public class ImpedanceRosterLoader {

    private string _path;

    public int[] VariableKeys { get; protected set; }
    public int[] MatrixKeys { get; protected set; }
    public bool[][] PossibleCombinations { get; protected set; }
    public bool[][] ActualCombinations { get; protected set; }
    public RosterEntry[][][][][] RosterEntries { get; protected set; }
    public List<ImpedanceRoster.VotRange> VotRanges { get; protected set; }
    public SkimMatrix[] SkimMatrices { get; set; }

    public virtual void LoadRosterCombinations() {
      FileInfo file = Global.GetInputPath(Global.Configuration.RosterCombinationsPath).ToFile();

      Global.PrintFile.WriteFileInfo(file, true);

      PossibleCombinations = new bool[Global.Settings.Modes.RosterModes][];
      ActualCombinations = new bool[Global.Settings.Modes.RosterModes][];

      for (int mode = Global.Settings.Modes.Walk; mode < Global.Settings.Modes.RosterModes; mode++) {
        PossibleCombinations[mode] = new bool[Global.Settings.PathTypes.TotalPathTypes];
        ActualCombinations[mode] = new bool[Global.Settings.PathTypes.TotalPathTypes];
      }

      using (CountingReader reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        string line;

        while ((line = reader.ReadLine()) != null) {
          if (line.StartsWith("#")) {
            continue;
          }

          string[] tokens = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

          if (tokens.Length == 0) {
            continue;
          }

          int pathType;

          switch (tokens[0]) {
            case "full-network":
              pathType = Global.Settings.PathTypes.FullNetwork;

              break;
            case "no-tolls":
            case "no-toll-network":
              pathType = Global.Settings.PathTypes.NoTolls;

              break;
            case "local-bus":
            case "transit-type-1":
            case "local-bus-pnr":
            case "transit-type-1-pnr":
              pathType = Global.Settings.PathTypes.LocalBus;

              break;
            case "light-rail":
            case "transit-type-2":
            case "light-rail-pnr":
            case "transit-type-2-pnr":
              pathType = Global.Settings.PathTypes.LightRail;

              break;
            case "premium-bus":
            case "transit-type-3":
            case "premium-bus-pnr":
            case "transit-type-3-pnr":
              pathType = Global.Settings.PathTypes.PremiumBus;

              break;
            case "commuter-rail":
            case "transit-type-4":
            case "commuter-rail-pnr":
            case "transit-type-4-pnr":
              pathType = Global.Settings.PathTypes.CommuterRail;

              break;
            case "ferry":
            case "transit-type-5":
            case "ferry-pnr":
            case "transit-type-5-pnr":
              pathType = Global.Settings.PathTypes.Ferry;

              break;
            case "local-bus-knr":
            case "transit-type-1-knr":
              pathType = Global.Settings.PathTypes.LocalBus_Knr;

              break;
            case "light-rail-knr":
            case "transit-type-2-knr":
              pathType = Global.Settings.PathTypes.LightRail_Knr;

              break;
            case "premium-bus-knr":
            case "transit-type-3-knr":
              pathType = Global.Settings.PathTypes.PremiumBus_Knr;

              break;
            case "commuter-rail-knr":
            case "transit-type-4-knr":
              pathType = Global.Settings.PathTypes.CommuterRail_Knr;

              break;
            case "ferry-knr":
            case "transit-type-5-knr":
              pathType = Global.Settings.PathTypes.Ferry_Knr;

              break;
            case "local-bus-tnc":
            case "transit-type-1-tnc":
              pathType = Global.Settings.PathTypes.LocalBus_TNC;

              break;
            case "light-rail-tnc":
            case "transit-type-2-tnc":
              pathType = Global.Settings.PathTypes.LightRail_TNC;

              break;
            case "premium-bus-tnc":
            case "transit-type-3-tnc":
              pathType = Global.Settings.PathTypes.PremiumBus_TNC;

              break;
            case "commuter-rail-tnc":
            case "transit-type-4-tnc":
              pathType = Global.Settings.PathTypes.CommuterRail_TNC;

              break;
            case "ferry-tnc":
            case "transit-type-5-tnc":
              pathType = Global.Settings.PathTypes.Ferry_TNC;

              break;
            default:
              throw new InvalidPathTypeException(string.Format("The value of \"{0}\" used for path type is invalid. Please adjust the roster accordingly.", tokens[0]));
          }

          int expectedRosterModes = tokens.Length;
          for (int mode = Global.Settings.Modes.Walk; mode < expectedRosterModes; mode++) {
            PossibleCombinations[mode][pathType] = bool.Parse(tokens[mode]);
          }
        }
      }
    }

    public virtual IEnumerable<RosterEntry> LoadRoster(string filename, bool checkCombination = true) {
      FileInfo file = filename.ToFile();

      Global.PrintFile.WriteFileInfo(file, true);

      _path = file.DirectoryName;

      List<RosterEntry> entries = new List<RosterEntry>();

      using (CountingReader reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        string line;

        while ((line = reader.ReadLine()) != null) {
          if (line.StartsWith("#")) {
            continue;
          }

          string[] tokens = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

          if (tokens.Length == 0) {
            continue;
          }

          RosterEntry entry = new RosterEntry {
            Variable = tokens[0].Clean(),
            Mode = tokens[1].ToMode(),
            PathType = tokens[2].ToPathType(),
            VotGroup = tokens[3].ToVotGroup(),
            StartMinute = int.Parse(tokens[4]).ToMinutesAfter3AM(),
            EndMinute = int.Parse(tokens[5]).ToMinutesAfter3AM(),
            Length = tokens[6].Clean(),
            FileType = tokens[7].Clean(),
            Name = tokens[8],
            Field = int.Parse(tokens[9]),
            Transpose = bool.Parse(tokens[10]),
            BlendVariable = tokens[11].Clean(),
            BlendPathType = tokens[12].ToPathType(),
            Factor = tokens[13].ToFactor(),
            Scaling = ParseScaling(tokens[14])
          };

          if (checkCombination) {
            if (!IsPossibleCombination(entry.Mode, entry.PathType)) {
              throw new InvalidCombinationException(
                  string.Format(
                      "The combination of mode: {0} and path type: {1} is invalid. Please adjust the roster accordingly.", entry.Mode,
                      entry.PathType));
            }

            ActualCombinations[entry.Mode][entry.PathType] = true;
          }

          entries.Add(entry);
        }
      }

      return entries;
    }

  
    public virtual void ProcessEntries(IEnumerable<RosterEntry> entries) {
      VariableKeys = entries.Select(x => x.Variable.GetHashCode()).Distinct().OrderBy(x => x).ToArray();
      MatrixKeys = entries.Select(x => x.MatrixKey).Distinct().OrderBy(x => x).ToArray();

      foreach (RosterEntry entry in entries) {
        entry.VariableIndex = GetVariableIndex(entry.Variable);
        entry.MatrixIndex = MatrixKeys.GetIndex(entry.MatrixKey);
      }

      RosterEntries = new RosterEntry[VariableKeys.Length][][][][];

      for (int variableIndex = 0; variableIndex < VariableKeys.Length; variableIndex++) {
        RosterEntries[variableIndex] = new RosterEntry[Global.Settings.Modes.RosterModes][][][]; // Initialize the mode array

        for (int mode = Global.Settings.Modes.Walk; mode < Global.Settings.Modes.RosterModes; mode++) {
          RosterEntries[variableIndex][mode] = new RosterEntry[Global.Settings.PathTypes.TotalPathTypes][][]; // Initialize the path type array

          for (int pathType = Global.Settings.PathTypes.FullNetwork; pathType < Global.Settings.PathTypes.TotalPathTypes; pathType++) {
            RosterEntries[variableIndex][mode][pathType] = new RosterEntry[Global.Settings.VotGroups.TotalVotGroups][]; // Initialize the vot groups

            for (int votGroup = Global.Settings.VotGroups.VeryLow; votGroup < Global.Settings.VotGroups.TotalVotGroups; votGroup++) {
              RosterEntries[variableIndex][mode][pathType][votGroup] = new RosterEntry[Global.Settings.Times.MinutesInADay + 1]; // Initialize the minute array    
            }
          }
        }
      }

      foreach (RosterEntry entry in entries) {
        int startMinute = entry.StartMinute;
        int endMinute = entry.EndMinute;

        // if roster entry for vot group is any or all or default, apply it to all vot groups
        int lowestVotGroup = entry.VotGroup == Global.Settings.VotGroups.Default ? Global.Settings.VotGroups.VeryLow : entry.VotGroup;
        int highestVotGroup = entry.VotGroup == Global.Settings.VotGroups.Default ? Global.Settings.VotGroups.VeryHigh : entry.VotGroup;

        for (int votGroup = lowestVotGroup; votGroup <= highestVotGroup; votGroup++) {
          if (startMinute > endMinute) {
            for (int minute = 1; minute <= endMinute; minute++) {
              RosterEntries[entry.VariableIndex][entry.Mode][entry.PathType][votGroup][minute] = entry;
            }

            for (int minute = startMinute; minute <= Global.Settings.Times.MinutesInADay; minute++) {
              RosterEntries[entry.VariableIndex][entry.Mode][entry.PathType][votGroup][minute] = entry;
            }
          } else {
            for (int minute = startMinute; minute <= endMinute; minute++) {
              RosterEntries[entry.VariableIndex][entry.Mode][entry.PathType][votGroup][minute] = entry;
            }
          }
        }
      }

      VotRanges = ImpedanceRoster.GetVotRanges();
    }

    private int GetVariableIndex(string variable) {
      int variableIndex = VariableKeys.GetIndex(variable);

      if (variableIndex == -1) {
        throw new VariableNotFoundException(string.Format("The variable \"{0}\" was not found in the roster configuration file. Please correct the problem and run the program again.", variable));
      }

      return variableIndex;
    }

    private double ParseScaling(string s) {
      if (bool.TryParse(s, out bool scale)) {
        if (scale) {
          return 100;
        }

        return 1;
      }
      return double.Parse(s);
    }

    public bool IsPossibleCombination(int mode, int pathType) {
      return PossibleCombinations[mode][pathType];
    }


    public virtual void LoadSkimMatrices(IEnumerable<RosterEntry> entries, Dictionary<int, int> zoneMapping, Dictionary<int, int> transitStopAreaMapping, Dictionary<int, int> microzoneMapping) {
      SkimMatrices = new SkimMatrix[MatrixKeys.Length];

      //            var cache = new Dictionary<string, List<float[]>>();
      Dictionary<string, List<double[]>> cache = new Dictionary<string, List<double[]>>(); // 20150703 JLB

      string currentFileName = "";
      foreach (var entry in entries.Where(x => x.FileType != null).Select(x => new { x.Name, x.Field, x.FileType, x.MatrixIndex, x.Scaling, x.Length }).Distinct().OrderBy(x => x.Name)) {
        ISkimFileReader skimFileReader = null;

        //Issue #40 -- caching is to prevent same file from being read in multiple times. Can clear cache when we change files since group by name
        if (!entry.Name.Equals(currentFileName)) {
          cache.Clear();
          currentFileName = entry.Name;
        }
        IFileReaderCreator creator = Global.ContainerDaySim.GetInstance<SkimFileReaderFactory>().GetFileReaderCreator(entry.FileType);

        /*switch (entry.FileType) {
            case "text_ij":
                skimFileReader = new TextIJSkimFileReader(cache, _path, mapping);
                break;
            case "bin":
                skimFileReader = new BinarySkimFileReader(_path, mapping);
                break;
        }*/

        if (creator == null) {
          if (entry.FileType == "deferred") {
            continue;
          }

          throw new SkimFileTypeNotSupportedException(string.Format("The specified skim file type of \"{0}\" is not supported.", entry.FileType));
        }
        Dictionary<int, int> mapping = zoneMapping;

        bool useTransitStopAreaMapping = (entry.Length == "transitstop");
        if (useTransitStopAreaMapping) {
          mapping = transitStopAreaMapping;
        }

        bool useMicrozoneMapping = (entry.Length == "microzone");
        if (useMicrozoneMapping) {
          mapping = microzoneMapping;
        }

        skimFileReader = creator.CreateReader(cache, _path, mapping);

        float scaleFactor = (float)entry.Scaling;
        SkimMatrix skimMatrix = skimFileReader.Read(entry.Name, entry.Field, scaleFactor);

        SkimMatrices[entry.MatrixIndex] = skimMatrix;

        //new code - report a summary of the skim matrix data
        int numZones = mapping.Count;
        int numPositive = 0;
        double avgPositive = 0;
        for (int row = 0; row < numZones; row++) {
          for (int col = 0; col < numZones; col++) {
            ushort value = skimMatrix.GetValue(row, col);
            if (value > Constants.EPSILON) {
              numPositive++;
              avgPositive += value / scaleFactor;
            }
          }
        }
        double pctPositive = Math.Round(numPositive * 100.0 / (numZones * numZones));
        double avgValue = avgPositive / Math.Max(numPositive, 1);
        Global.PrintFile.WriteLine("Skim File {0}, {1} percent of values are positive, with average value {2}.", entry.Name, pctPositive, avgValue);
      }

      foreach (
          var entry in
              entries.Where(x => x.FileType == null)
                     .Select(x => new { x.Name, x.Field, x.FileType, x.MatrixIndex, x.Scaling, x.Length })
                     .Distinct()
                     .OrderBy(x => x.Name)) {
        SkimMatrix skimMatrix = new SkimMatrix(null);
        SkimMatrices[entry.MatrixIndex] = skimMatrix;
      }

    }
  }
}
