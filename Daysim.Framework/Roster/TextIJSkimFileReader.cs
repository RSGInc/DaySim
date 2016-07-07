// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaySim.Framework.Core;
using DaySim.Framework.Exceptions;

namespace DaySim.Framework.Roster {
	public sealed class TextIJSkimFileReader : ISkimFileReader {
		private const int FIELD_OFFSET = -1; // This value accounts for the fact that the first two columns are the origin and destination whicn aren't included in the field parameter.

//		private readonly Dictionary<string, List<float[]>> _cache;
		private readonly Dictionary<string, List<double[]>> _cache;  // 20150703 JLB
		private readonly string _path;
		private readonly Dictionary<int, int> _mapping;

//		public TextIJSkimFileReader(Dictionary<string, List<float[]>> cache, string path, Dictionary<int, int> mapping) {
		public TextIJSkimFileReader(Dictionary<string, List<double[]>> cache, string path, Dictionary<int, int> mapping) { // 20150703 JLB
			_cache = cache;
			_path = path;
			_mapping = mapping;
		}

		public SkimMatrix Read(string filename, int field, float scale) {
			var file = new FileInfo(Path.Combine(_path, filename));

			Console.WriteLine("Loading skim file: {0}, field: {1}.", file.Name, field);
			Global.PrintFile.WriteFileInfo(file, true);

			if (!file.Exists) {
				throw new FileNotFoundException(string.Format("The skim file {0} could not be found.", file.FullName));
			}

			var count = _mapping.Count;
			var matrix = new ushort[count][];

			for (var i = 0; i < count; i++) {
				matrix[i] = new ushort[count];
			}

//			List<float[]> rows;
			List<double[]> rows;   // 20150703 JLB

			if (!_cache.TryGetValue(file.Name, out rows)) {
//				rows = new List<float[]>();
				rows = new List<double[]>();  // 20150703 JLB 

				using (var reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
					string line;
					if (Global.TextSkimFilesContainHeaderRecord)
						reader.ReadLine();
					while ((line = reader.ReadLine()) != null) {

						try
						{
							char delimiter = Global.SkimDelimiter;
							var p1 = line.Split(new[] {delimiter}, StringSplitOptions.RemoveEmptyEntries);
//							var row = p1.Select(Convert.ToSingle).ToArray();
							var row = p1.Select(Convert.ToDouble).ToArray();     // 20150703 JLB

							rows.Add(row);
						}
						catch (Exception e) {
							throw new InvalidSkimRowException(string.Format("Error parsing row in {0}.\nError in row {1}.\n{{{2}}}", file.FullName, reader.LineNumber, line), e);
						}
					}
				}

				_cache.Add(file.Name, rows);
			}
			for (var i = 0; i < rows.Count; i++) {
				var row = rows[i];

				try {

					// test code
					var index = Convert.ToInt32(row[0]);
					var origin = _mapping[index];
					//var origin = _mapping[Convert.ToInt32(row[0])];

					index = Convert.ToInt32(row[1]);
					var destination = _mapping[index];
					//var destination = _mapping[Convert.ToInt32(row[1])];

					var rawValue = Convert.ToSingle(row[field + FIELD_OFFSET]) * scale;

					if (rawValue > ushort.MaxValue - 1) {
						rawValue = ushort.MaxValue - 1;
					}
					else if (rawValue < 0) {
						rawValue = 0;
					}

					var value = Convert.ToUInt16(rawValue);

					matrix[origin][destination] = value;
				}
				catch (Exception e) {
					throw new ErrorReadingSkimFileException(string.Format("Error reading row {0}, {1}", i, string.Join(" ", row)), e);
				}
			}

			var skimMatrix = new SkimMatrix(matrix);

			return skimMatrix;
		}
	}
}