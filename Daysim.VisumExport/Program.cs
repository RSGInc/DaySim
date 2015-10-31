using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using NDesk.Options;

namespace Daysim.VisumExport {
	class Program {
		
		private static string _tripPath;
		private static string _logPath;
		private static string _outputPath;
		private static bool _showHelp;

		static void Main(string[] args) 
		{
			//Global.Configuration = ConfigurationManager.OpenConfiguration(_configurationPath);
			Global.Configuration = new Configuration();

			

			var options = new OptionSet {
					{"t|tripfile=", "Path to trip file", v => _tripPath = v},
					{"o|outputpath=", "Path to output", v => _outputPath = v},
					{"l|logfile=", "Path to print file", v => _logPath = v},
					{"h|?|help", "Show help and syntax summary", v => _showHelp = v != null}
				};

				options.Parse(args);

				if (_showHelp) {
					options.WriteOptionDescriptions(Console.Out);

					Console.WriteLine("Please press any key to exit");
					Console.ReadKey();

					Environment.Exit(0);
				}

			if (string.IsNullOrEmpty(_logPath))
				_logPath = "log.log";
			Global.PrintFile = new PrintFile(_logPath);

			if (string.IsNullOrEmpty(_outputPath))
			{
				_outputPath = "";
			}
			else if (!_outputPath.EndsWith("\\"))
			{
				_outputPath += "\\";
			}

			BeginExportTrips(_tripPath, _outputPath);
		}

		public static void BeginExportTrips(string tripFilename, string outputPath)
		{
			List<ITrip> trips = GetExportedTrips(tripFilename, (char)9);
			TripMapper mapper = new TripMapper();

			var exporter = new TripSkimExporter(new TripSelector(), new VisumSkimTextWriter(outputPath), mapper);
			
			exporter.ExportTripsToSkims(trips);
		}

		private static List<ITrip> GetExportedTrips(string path, char delimiter)
		{
			var inputFile = new FileInfo(path);
			var trips = new List<ITrip>();

			using (var reader = new StreamReader(inputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				{
					reader.ReadLine();

					string line;

					while ((line = reader.ReadLine()) != null)
					{
						var row = line.Split(new[] {delimiter});
						var model = new Trip();

						SetModel(model, row);
						trips.Add(model);
					}
				}
			}
			return trips;
		}

		private static void SetModel(Trip model, string[] row)
		{
			int i = 0;
			model.Id = int.Parse(row[i++]);

			model.TourId = int.Parse(row[i++]);

			model.HouseholdId = int.Parse(row[i++]);

			model.PersonSequence = int.Parse(row[i++]);

			model.Day = int.Parse(row[i++]);

			model.TourSequence = int.Parse(row[i++]);

			model.Direction = int.Parse(row[i++]);

			model.Sequence = int.Parse(row[i++]);

			model.SurveyTripSequence = int.Parse(row[i++]);

			model.OriginPurpose = int.Parse(row[i++]);

			model.DestinationPurpose = int.Parse(row[i++]);

			model.OriginAddressType = int.Parse(row[i++]);

			model.DestinationAddressType = int.Parse(row[i++]);

			model.OriginParcelId = int.Parse(row[i++]);

			model.OriginZoneKey = int.Parse(row[i++]);

			model.DestinationParcelId = int.Parse(row[i++]);

			model.DestinationZoneKey = int.Parse(row[i++]);

			model.Mode = int.Parse(row[i++]);

			model.PathType = int.Parse(row[i++]);

			model.DriverType = int.Parse(row[i++]);

			model.DepartureTime = int.Parse(row[i++]);

			model.ArrivalTime = int.Parse(row[i++]);

			model.ActivityEndTime = int.Parse(row[i++]);

			model.TravelTime = double.Parse(row[i++]);

			model.TravelCost = double.Parse(row[i++]);

			model.TravelDistance = double.Parse(row[i++]);

			model.ValueOfTime = double.Parse(row[i++]);

			model.ExpansionFactor = double.Parse(row[i++]);
		}

	}
}
