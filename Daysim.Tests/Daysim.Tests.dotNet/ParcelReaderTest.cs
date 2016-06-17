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
	
	public class ParcelReaderTest {
		/*private const int MAX_PARCEL_ID = 1500004;

		private readonly ImporterFactory _importerFactory;

		public ParcelReaderTest(Configuration configuration, ImporterFactory importerFactory) {
			Global.Configuration = configuration;

			_importerFactory = importerFactory;
		}

		[Fact]
		public void TestParcelImporter() {
			var parcelImporter = _importerFactory.GetImporter<Parcel>(Global.Configuration.InputParcelPath, Global.Configuration.InputParcelDelimiter);

			parcelImporter.BeginImport(Global.WorkingParcelPath, "Importing parcels...");
		}

		[Fact]
		public void TestParcelReader() {
			var parcelFile = new FileInfo(Global.WorkingParcelPath);

			if (!parcelFile.Exists) {
				TestParcelImporter();
			}

			using (var reader = new Reader<Parcel>(Global.WorkingParcelPath)) {
				var count = reader.Count;

				Console.WriteLine("{0} parcels found.", count);

				TestParcelSeekingForward(reader, MAX_PARCEL_ID);
				TestParcelSeekingBackward(reader, MAX_PARCEL_ID);
				TestParcelRandomSeek(reader, MAX_PARCEL_ID + 1);
				TestParcelEnumerator(reader);
				TestParcelIntegrity(reader, MAX_PARCEL_ID);
			}
		}

		private static void TestParcelSeekingForward(Reader<Parcel> reader, int count) {
			Console.WriteLine("Seeking forward...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for (var i = 1; i <= count; i++) {
				var parcel = reader.Seek(i);

				if (parcel != null && parcel.Id != i) {
					throw new Exception("Mismatch error.");
				}
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestParcelSeekingBackward(Reader<Parcel> reader, int count) {
			Console.WriteLine("Seeking backward...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for (var i = count; i > 0; i--) {
				var parcel = reader.Seek(i);

				if (parcel != null && parcel.Id != i) {
					throw new Exception("Mismatch error.");
				}
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestParcelRandomSeek(Reader<Parcel> reader, int threshold) {
			Console.WriteLine("Random seek...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var random = new Random();

			for (var i = 0; i < 10; i++) {
				var id = random.Next(1, threshold);
				var parcel = reader.Seek(id);

				if (parcel != null && parcel.Id != id) {
					throw new Exception("Mismatch error.");
				}
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestParcelEnumerator(IEnumerable<Parcel> reader) {
			Console.WriteLine("Testing enumerator...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			foreach (var parcel in reader) {}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}

		private static void TestParcelIntegrity(Reader<Parcel> reader, int count) {
			Console.WriteLine("Integrity check...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var list = new List<int>(reader.Count);

			for (var i = 1; i <= count; i++) {
				var parcel = reader.Seek(i);

				if (parcel != null) {
					list.Add(parcel.Id);
				}
			}

			var parcels = reader.Select(p => p.Id).ToList();

			if (parcels.Where((t, i) => t != list[i]).Any()) {
				throw new Exception("Mismatch error.");
			}

			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		}*/
	}
}