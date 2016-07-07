// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Daysim.DomainModels;
using Daysim.Framework.Core;
using Daysim.Framework.Persistence;
using Xunit;

namespace Daysim.Tests {
	
	public class ZoneReaderTest {
		/*private const int MAX_ZONE_ID = 3950;

		private readonly ImporterFactory _importerFactory;

		public ZoneReaderTest(Configuration configuration, ImporterFactory importerFactory) {
			Global.Configuration = configuration;

			_importerFactory = importerFactory;
		}

		[Fact]
		public void TestZoneImporter() {
			var zoneImporter = _importerFactory.GetImporter<Zone>(Global.Configuration.InputZonePath, Global.Configuration.InputZoneDelimiter);

			zoneImporter.BeginImport(Global.WorkingZonePath, "Importing zones...");
		}

		[Fact]
		public void TestZoneReader() {
			var zoneFile = new FileInfo(Global.WorkingZonePath);

			if (!zoneFile.Exists) {
				TestZoneImporter();
			}

			using (var reader = new Reader<Zone>(Global.WorkingZonePath)) {
				var count = reader.Count;

				Console.WriteLine("{0} zones found.", count);

				TestZoneSeekingForward(reader, MAX_ZONE_ID);
				TestZoneSeekingBackward(reader, MAX_ZONE_ID);
				TestZoneRandomSeek(reader, MAX_ZONE_ID + 1);
				TestZoneEnumerator(reader);
				TestZoneIntegrity(reader, MAX_ZONE_ID);
			}
		}

		private static void TestZoneSeekingForward(Reader<Zone> reader, int count) {
			Console.WriteLine("Seeking forward...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for (var i = 1; i <= count; i++) {
				var zone = reader.Seek(i);

				if (zone != null && zone.Id != i) {
					throw new Exception("Mismatch error.");
				}
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestZoneSeekingBackward(Reader<Zone> reader, int count) {
			Console.WriteLine("Seeking backward...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for (var i = count; i > 0; i--) {
				var zone = reader.Seek(i);

				if (zone != null && zone.Id != i) {
					throw new Exception("Mismatch error.");
				}
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestZoneRandomSeek(Reader<Zone> reader, int threshold) {
			Console.WriteLine("Random seek...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var random = new Random();

			for (var i = 0; i < 10; i++) {
				var id = random.Next(1, threshold);
				var zone = reader.Seek(id);

				if (zone != null && zone.Id != id) {
					throw new Exception("Mismatch error.");
				}
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestZoneEnumerator(IEnumerable<Zone> reader) {
			Console.WriteLine("Testing enumerator...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			foreach (var zone in reader) {}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestZoneIntegrity(Reader<Zone> reader, int count) {
			Console.WriteLine("Integrity check...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var list = new List<int>(reader.Count);

			for (var i = 1; i <= count; i++) {
				var zone = reader.Seek(i);

				if (zone != null) {
					list.Add(zone.Id);
				}
			}

			var zones = reader.Select(p => p.Id).ToList();

			if (zones.Where((t, i) => t != list[i]).Any()) {
				throw new Exception("Mismatch error.");
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}*/
	}
}