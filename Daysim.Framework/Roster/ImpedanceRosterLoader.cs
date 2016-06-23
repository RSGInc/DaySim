using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Daysim.Framework.Core;
using Daysim.Framework.Exceptions;
using Ninject;

namespace Daysim.Framework.Roster {
	public class ImpedanceRosterLoader 
	{

		private string _path;

		public int[] VariableKeys { get; protected set; }
		public int[] MatrixKeys { get; protected set; }
		public bool[][] PossibleCombinations { get; protected set; }
		public bool[][] ActualCombinations { get; protected set; }
		public RosterEntry[][][][][] RosterEntries { get; protected set; }
		public List<ImpedanceRoster.VotRange> VotRanges { get; protected set; }
		public SkimMatrix[] SkimMatrices { get; set; }

		public virtual void LoadRosterCombinations() {
			var file = Global.GetInputPath(Global.Configuration.RosterCombinationsPath).ToFile();

			Global.PrintFile.WriteFileInfo(file, true);

			PossibleCombinations = new bool[Global.Settings.Modes.TotalModes][];
			ActualCombinations = new bool[Global.Settings.Modes.TotalModes][];

			for (var mode = Global.Settings.Modes.Walk; mode < Global.Settings.Modes.TotalModes; mode++) {
				PossibleCombinations[mode] = new bool[Global.Settings.PathTypes.TotalPathTypes];
				ActualCombinations[mode] = new bool[Global.Settings.PathTypes.TotalPathTypes];
			}

			using (var reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
				string line;

				while ((line = reader.ReadLine()) != null) {
					if (line.StartsWith("#")) {
						continue;
					}

					var tokens = line.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

					if (tokens.Length == 0) {
						continue;
					}

					int pathType;

					switch (tokens[0]) {
						case "full-network":
							pathType = Global.Settings.PathTypes.FullNetwork;

							break;
						case "no-tolls":
							pathType = Global.Settings.PathTypes.NoTolls;

							break;
						case "local-bus":
							pathType = Global.Settings.PathTypes.LocalBus;

							break;
						case "light-rail":
							pathType = Global.Settings.PathTypes.LightRail;

							break;
						case "premium-bus":
							pathType = Global.Settings.PathTypes.PremiumBus;

							break;
						case "commuter-rail":
							pathType = Global.Settings.PathTypes.CommuterRail;

							break;
						case "ferry":
							pathType = Global.Settings.PathTypes.Ferry;

							break;
						default:
							throw new InvalidPathTypeException(string.Format("The value of \"{0}\" used for path type is invalid. Please adjust the roster accordingly.", tokens[0]));
					}

					for (var mode = Global.Settings.Modes.Walk; mode < Global.Settings.Modes.TotalModes; mode++) {
						PossibleCombinations[mode][pathType] = bool.Parse(tokens[mode]);
					}
				}
			}
		}

		public virtual IEnumerable<RosterEntry> LoadRoster(string filename, bool checkCombination = true) {
			var file = filename.ToFile();

			Global.PrintFile.WriteFileInfo(file, true);

			_path = file.DirectoryName;

			var entries = new List<RosterEntry>();

			using (var reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("#"))
					{
						continue;
					}

					var tokens = line.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

					if (tokens.Length == 0)
					{
						continue;
					}

					var entry = new RosterEntry
						            {
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

					if (checkCombination)
					{
						if (!IsPossibleCombination(entry.Mode, entry.PathType))
						{
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

		public virtual void ProcessEntries(IEnumerable<RosterEntry> entries)
		{
			VariableKeys = entries.Select(x => x.Variable.GetHashCode()).Distinct().OrderBy(x => x).ToArray();
			MatrixKeys = entries.Select(x => x.MatrixKey).Distinct().OrderBy(x => x).ToArray();

			foreach (var entry in entries) {
				entry.VariableIndex = GetVariableIndex(entry.Variable);
				entry.MatrixIndex = MatrixKeys.GetIndex(entry.MatrixKey);
			}

			RosterEntries = new RosterEntry[VariableKeys.Length][][][][];

			for (var variableIndex = 0; variableIndex < VariableKeys.Length; variableIndex++) {
				RosterEntries[variableIndex] = new RosterEntry[Global.Settings.Modes.TotalModes][][][]; // Initialize the mode array

				for (var mode = Global.Settings.Modes.Walk; mode < Global.Settings.Modes.TotalModes; mode++) {
					RosterEntries[variableIndex][mode] = new RosterEntry[Global.Settings.PathTypes.TotalPathTypes][][]; // Initialize the path type array

					for (var pathType = Global.Settings.PathTypes.FullNetwork; pathType < Global.Settings.PathTypes.TotalPathTypes; pathType++) {
						RosterEntries[variableIndex][mode][pathType] = new RosterEntry[Global.Settings.VotGroups.TotalVotGroups][]; // Initialize the vot groups

						for (var votGroup = Global.Settings.VotGroups.VeryLow; votGroup < Global.Settings.VotGroups.TotalVotGroups; votGroup++) {
							RosterEntries[variableIndex][mode][pathType][votGroup] = new RosterEntry[Global.Settings.Times.MinutesInADay + 1]; // Initialize the minute array	
						}
					}
				}
			}

			foreach (var entry in entries) {
				var startMinute = entry.StartMinute;
				var endMinute = entry.EndMinute;

				// if roster entry for vot group is any or all or default, apply it to all vot groups
				var lowestVotGroup = entry.VotGroup == Global.Settings.VotGroups.Default ? Global.Settings.VotGroups.VeryLow : entry.VotGroup;
				var highestVotGroup = entry.VotGroup == Global.Settings.VotGroups.Default ? Global.Settings.VotGroups.VeryHigh : entry.VotGroup;

				for (var votGroup = lowestVotGroup; votGroup <= highestVotGroup; votGroup++) {
					if (startMinute > endMinute) {
						for (var minute = 1; minute <= endMinute; minute++) {
							RosterEntries[entry.VariableIndex][entry.Mode][entry.PathType][votGroup][minute] = entry;
						}

						for (var minute = startMinute; minute <= Global.Settings.Times.MinutesInADay; minute++) {
							RosterEntries[entry.VariableIndex][entry.Mode][entry.PathType][votGroup][minute] = entry;
						}
					}
					else {
						for (var minute = startMinute; minute <= endMinute; minute++) {
							RosterEntries[entry.VariableIndex][entry.Mode][entry.PathType][votGroup][minute] = entry;
						}
					}
				}
			}

			VotRanges = ImpedanceRoster.GetVotRanges();
		}

		private int GetVariableIndex(string variable) {
			var variableIndex = VariableKeys.GetIndex(variable);

			if (variableIndex == -1) {
				throw new VariableNotFoundException(string.Format("The variable \"{0}\" was not found in the roster configuration file. Please correct the problem and run the program again.", variable));
			}

			return variableIndex;
		}

		private double ParseScaling(string s)
		{
			bool scale;
			if (bool.TryParse(s, out scale))
			{
				if (scale)
					return 100;
				return 1;
			}
			return double.Parse(s);
		}

		public bool IsPossibleCombination(int mode, int pathType) {
			return PossibleCombinations[mode][pathType];
		}

		
		public virtual void LoadSkimMatrices(IEnumerable<RosterEntry> entries, Dictionary<int, int> zoneMapping, Dictionary<int, int> transitStopAreaMapping, Dictionary<int, int> microzoneMapping) {
			SkimMatrices = new SkimMatrix[MatrixKeys.Length];

//			var cache = new Dictionary<string, List<float[]>>();
			var cache = new Dictionary<string, List<double[]>>(); // 20150703 JLB

            var currentFileName = "";
            foreach (var entry in entries.Where(x => x.FileType != null).Select(x => new { x.Name, x.Field, x.FileType, x.MatrixIndex, x.Scaling, x.Length }).Distinct().OrderBy(x => x.Name))
            {
                ISkimFileReader skimFileReader = null;

                //Issue #40 -- caching is to prevent same file from being read in multiple times. Can clear cache when we change files since group by name
                if (!entry.Name.Equals(currentFileName))
                {
                    cache.Clear();
                    currentFileName = entry.Name;
                }
                IFileReaderCreator creator = Global.Kernel.Get<SkimFileReaderFactory>().GetFileReaderCreator(entry.FileType);

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
				if (useTransitStopAreaMapping)
					mapping = transitStopAreaMapping;

				bool useMicrozoneMapping = (entry.Length == "microzone");
				if (useMicrozoneMapping)
					mapping = microzoneMapping;
		
				skimFileReader = creator.CreateReader(cache, _path, mapping);

				var skimMatrix = skimFileReader.Read(entry.Name, entry.Field, (float)entry.Scaling);

				SkimMatrices[entry.MatrixIndex] = skimMatrix;
			}

			foreach (
				var entry in
					entries.Where(x => x.FileType == null)
					       .Select(x => new {x.Name, x.Field, x.FileType, x.MatrixIndex, x.Scaling, x.Length})
					       .Distinct()
					       .OrderBy(x => x.Name))
			{
				var skimMatrix = new SkimMatrix(null);
				SkimMatrices[entry.MatrixIndex] = skimMatrix;
			}

		}
	}
}
