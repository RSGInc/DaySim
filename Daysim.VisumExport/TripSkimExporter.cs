using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.Roster;

namespace Daysim {
	public class TripSkimExporter
	{
		private ITripSelector _tripSelector;
		private ITripSkimWriter _exporter;
		private ITripMapper _mapper;

		public TripSkimExporter(ITripSelector tripSelector, ITripSkimWriter exporter, ITripMapper mapper)
		{
			_tripSelector = tripSelector;
			_exporter = exporter;
			_mapper = mapper;
		}

		public void ExportTripsToSkims(List<ITrip> trips)
		{
			IEnumerable<RosterEntry> entries = GetRosterEntries();
			Dictionary<string, List<RosterEntry>> entryGroups = new Dictionary<string, List<RosterEntry>>();

			foreach (var entry in entries)
			{
				if (entry.Variable == "trips")
				{
					if (!entryGroups.ContainsKey(entry.Name))
					{
						entryGroups.Add(entry.Name, new List<RosterEntry>());
					}
					entryGroups[entry.Name].Add(entry);
				}
			}

			foreach (var rosterList in entryGroups.Values)
			{
				List<ITrip> selectedTrips = new List<ITrip>();

				foreach (var rosterEntry in rosterList)
				{
					selectedTrips.AddRange(_tripSelector.Select(trips, rosterEntry.Mode, rosterEntry.PathType,
					                                            rosterEntry.StartMinute, rosterEntry.EndMinute));
				}
				double factor = 1;
				string name = rosterList[0].Name;
				int startMinute = rosterList[0].StartMinute;
				int endMinute = rosterList[0].EndMinute;
				Dictionary<int, int> mapping = _mapper.GetMapping(selectedTrips);
				double[][] table = _tripSelector.GetTripTable(selectedTrips, factor, mapping.Count, mapping);
				int transport = 0;
				int[] mapArray = new int[mapping.Count];
				for (int i = 0; i < mapping.Count; i++)
				{
					mapArray[i] = mapping.FirstOrDefault(t => t.Value == i).Key;
				}
				//I need logic here to make sure to get these entries right for multiple entries
				ExportTrips(table, Global.GetOutputPath(name), mapArray, table.Length, transport,
				            startMinute,
				            endMinute, factor);
			}
		}

		private void ExportTrips(double[][] table, string filename, int[] mapping, int count, int transport, int startTime, int endTime, double factor) 
		{
			_exporter.WriteSkimToFile(table, filename, mapping, count, transport, startTime, endTime, (int)factor);
		}

		private IEnumerable<RosterEntry> GetRosterEntries()
		{
			string rosterFilename = "C:\\temp\\PA\\roster_VISUM_tripoutput.csv";
			ImpedanceRosterLoader loader = new ImpedanceRosterLoader();
			return loader.LoadRoster(rosterFilename, false);
		}
	}
}
