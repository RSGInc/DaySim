using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.Framework.Core;
using Daysim.Framework.Roster;

namespace Daysim.Tests {
	public class TestImpedanceRosterLoader : ImpedanceRosterLoader
	{
		public IEnumerable<RosterEntry> TestEntries { get; set; } 
		public List<ImpedanceRoster.VotRange> TestVotRanges { get; set; } 
		public RosterEntry[][][][][] TestRosterEntries { get; set; }
		public SkimMatrix[] TestSkimMatrices { get; set; }

		public override IEnumerable<RosterEntry> LoadRoster(string filename, bool checkCombinations = true)
		{
			var entries = TestEntries;
			VotRanges = TestVotRanges;
			RosterEntries = TestRosterEntries;

			VariableKeys = entries.Select(x => x.Variable.GetHashCode()).Distinct().OrderBy(x => x).ToArray();
			MatrixKeys = entries.Select(x => x.MatrixKey).Distinct().OrderBy(x => x).ToArray();
			
			return entries;
		}

		public override void ProcessEntries(IEnumerable<RosterEntry> entries) 
		{
//			base.ProcessEntries(entries);
			entries = TestEntries;
			VotRanges = TestVotRanges;
			RosterEntries = TestRosterEntries;

			VariableKeys = entries.Select(x => x.Variable.GetHashCode()).Distinct().OrderBy(x => x).ToArray();
			MatrixKeys = entries.Select(x => x.MatrixKey).Distinct().OrderBy(x => x).ToArray();
		}

		public override void LoadRosterCombinations()
		{
			PossibleCombinations = new bool[Constants.Mode.TOTAL_MODES][];
			ActualCombinations = new bool[Constants.Mode.TOTAL_MODES][];
		}

		public override void LoadSkimMatrices(IEnumerable<RosterEntry> entries, Dictionary<int, int> zoneMapping, Dictionary<int, int> transitStopAreaMapping)
		{
			SkimMatrices = TestSkimMatrices;
		}
	}
}
