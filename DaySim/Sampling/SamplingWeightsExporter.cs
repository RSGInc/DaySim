// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System.Collections.Generic;
using System.IO;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Persisters;
using DaySim.Framework.Factories;
using DaySim.Framework.Sampling;
using SimpleInjector;

namespace DaySim.Sampling {
	public static class SamplingWeightsExporter {
		public static void Export(string path) {
			BeginRunExport(path);
		}

		private static void BeginRunExport(string path) {
			Global.PrintFile.WriteLine("Output files:");
			Global.PrintFile.IncrementIndent();

			RunExport(path);

			Global.PrintFile.DecrementIndent();
		}

		private static void RunExport(string path) {
			var directory = Path.GetDirectoryName(path);

			if (directory == null) {
				return;
			}

			var filename = Path.GetFileNameWithoutExtension(path);
			var extension = Path.GetExtension(path);
			var segmentCount = Global.SegmentZones.GetLength(0);
			
			var zoneReader = 
				Global
					.ContainerDaySim.GetInstance<IPersistenceFactory<IZone>>()
					.Reader;

			for (var segment = 0; segment < segmentCount; segment++) {
				var file = new FileInfo(Path.Combine(directory, string.Format("{0}.{1}{2}", filename, segment, extension)));

				using (var writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
					var segmentZones = Global.SegmentZones[segment];
					var zoneCount = segmentZones.GetLength(0);

					var sizesFile = new FileInfo(Path.Combine(directory, string.Format("{0}.{1}.sizes{2}", filename, segment, extension)));
					var sizesWriter = new StreamWriter(sizesFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

					var weightsFile = new FileInfo(Path.Combine(directory, string.Format("{0}.{1}.weights{2}", filename, segment, extension)));
					var weightsWriter = new StreamWriter(weightsFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

					for (var zoneId = 0; zoneId < zoneCount; zoneId++) {
						var segmentZone = segmentZones[zoneId];

						if (segmentZone == null) {
							continue;
						}

						var zone = zoneReader.Seek(zoneId);

						writer.Write(zone.Key);
						writer.Write("\t");
						writer.Write(segmentZone.TotalSize.ToString("f7"));
						writer.Write("\t");
						writer.Write(segmentZone.TotalWeight.ToString("f7"));
						writer.WriteLine();

						ExportRankedSizes(sizesWriter, zone.Key, segmentZone.RankedSizes);
						ExportRankedWeights(weightsWriter, zone.Key, zoneReader, segmentZone.RankedWeights);

						sizesWriter.Flush();
						weightsWriter.Flush();
					}

					sizesWriter.Dispose();
					weightsWriter.Dispose();
				}

				Global.PrintFile.WriteFileInfo(file, true);
			}
		}

		private static void ExportRankedSizes(TextWriter writer, int key, IEnumerable<SizeSegmentItem> segmentItems) {
			foreach (var segmentItem in segmentItems) {
				writer.Write(key);
				writer.Write("\t");

				writer.Write(segmentItem.Id);
				writer.Write("\t");

				writer.Write(segmentItem.Sequence);
				writer.Write("\t");

				writer.Write(segmentItem.Value.ToString("f7"));
				writer.WriteLine();
			}
		}

		private static void ExportRankedWeights(TextWriter writer, int key, IPersisterReader<IZone> zoneReader, IEnumerable<WeightSegmentItem> segmentItems) {
			foreach (var segmentItem in segmentItems) {
				var zone = zoneReader.Seek(segmentItem.Id);

				writer.Write(key);
				writer.Write("\t");

				writer.Write(zone.Key);
				writer.Write("\t");

				writer.Write(segmentItem.Value.ToString("f7"));
				writer.WriteLine();
			}
		}
	}
}