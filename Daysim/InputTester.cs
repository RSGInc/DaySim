// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DaySim.Framework.Core;
using DaySim.Framework.Exceptions;
using HDF5DotNet;

namespace DaySim {
	public static class InputTester {
		public static int errorCount;

		public static void RunTest() {
			errorCount = 0;
			var zones = GetValidZones();
			var parcels = TestParcels(zones);

			if (Global.Configuration.ReadHDF5) {
				var households = TestHouseholds(parcels);
				TestPersons(households, parcels);
			}

			TestIxxi(zones);
			TestParkAndRideNodes(zones);

			if (errorCount > 0) {
				Console.WriteLine("There were {0} errors. Please check errors.log", errorCount);
				Console.WriteLine("Press any key to exit.");
				Console.ReadKey();
				Environment.Exit(2);
			}
		}

		//Input Test Methods

		//In this method, we are getting the list of valid zones, to ensure that all other files only 
		// include these zone_ids
		private static Dictionary<int, int> GetValidZones() {
			var zones = new Dictionary<int, int>();
			var inputFile = Global.GetInputPath(Global.Configuration.RawZonePath).ToFile();

			using (var reader = new CountingReader(inputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
				var newId = 0;
				string line;
				int lineNum = 1;

				while ((line = reader.ReadLine()) != null) {
					//the first line is the header
					if (lineNum != 1) {
						var row = line.Split(new[] { Global.Configuration.RawZoneDelimiter }, StringSplitOptions.RemoveEmptyEntries);
						var originalId = Convert.ToInt32(row[0]);
						zones.Add(originalId, newId);
						if (originalId < 0) {
							PrintError("Zone", "zoneId", originalId, lineNum);
							errorCount++;
						}
					}
					lineNum++;
				}
				return zones;
			}
		}

		private static Dictionary<int, int> TestParcels(Dictionary<int, int> zones) {
			var parcels = new Dictionary<int, int>();
			var inputFile = Global.GetInputPath(Global.Configuration.RawParcelPath).ToFile();

			using (var reader = new CountingReader(inputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
				int lineNum = 1;
				var sequences = new Dictionary<int, int>();
				string line;

				while ((line = reader.ReadLine()) != null) {
					//first line is header
					if (lineNum != 1) {
						var row = line.Split(new[] { Global.Configuration.RawParcelDelimiter }, StringSplitOptions.RemoveEmptyEntries);
						var id = int.Parse(row[0]);
						int originalZoneId = int.Parse(row[4]);
						int countBadZones = 0;

						if (!zones.ContainsKey(originalZoneId)) {
							PrintError("Parcels", "ZoneID", originalZoneId, lineNum);
							countBadZones++;
							errorCount++;
						}

						parcels.Add(id, originalZoneId);

						for (int i = 2; i < row.Length; i++) {
							//only for variables after lutype_p and not the variable:aparks (in sq ft)
							float value = Convert.ToSingle(row[i]);
							int value_int = (int) value;
							// some of variables are sqft in area so they end up being pretty large
							// and overflow the cast from single to int; like i=47; so don't test them
							if (value_int < 0 && i > 5 && i < 47) {
								PrintError("Parcels", "a variable", value_int, lineNum, i);
								errorCount++;
							}
							else if (value_int > Global.Settings.MaxInputs.MaxParcelVals && i > 5 && i != 40 && i != 41 && i < 47) {
								PrintError("Parcels", "a variable", value_int, lineNum, i);
								errorCount++;
							}
						}
					}

					lineNum++;
				}
			}
			return parcels;
		}

		private static Dictionary<int, int> TestHouseholds(Dictionary<int, int> parcels) {
			var households = new Dictionary<int, int>();
			//has hhno and hhsize
			string[] checkVectors = { "hhno", "hhsize", "hhincome", "hhparcel" };
			string path = Global.GetInputPath(Global.Configuration.RosterPath).ToFile().DirectoryName;
			string hdfFile = path + "\\" + Global.Configuration.HDF5Filename;
			var dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
			string baseDataSetName = "/Household/";

			int nVectors = checkVectors.Count();
			string[] headers = new string[nVectors + 2];
			Int32[][] values = new Int32[nVectors][];

			int x = 0;
			int size = -1;

			foreach (string vector in checkVectors) {
				if (x == 0) {
					headers[0] = vector;
					headers[1] = "zone_id";
					headers[2] = "fraction_with_jobs_outside";
				}
				else {
					headers[x + 2] = vector;
				}
				values[x] = GetInt32DataSet(dataFile, baseDataSetName + vector);

				//first check that the data vector exists and it's the right size
				if (values[x] == null)
					throw new Exception(vector + " column does not exist for Household");
				int vSize = values[x].Count();
				if (size == -1)
					size = vSize;
				else {
					if (vSize != size) {
						throw new Exception(vector + " column for Household is the wrong size " + size + " vs " + vSize);
					}

					//now check that the values all lie within reasonable ranges
					int minVal;
					int maxVal;
					int lineNum;

					switch (vector) {
						case "hhno":
							minVal = values[x].Min();
							if (minVal <= 0) {
								lineNum = Array.FindIndex(values[x], y => y < 0);
								PrintError("Households", "hhno", minVal, lineNum);
								errorCount++;
							}
							break;
						case "hhsize":
							minVal = values[x].Min();
							maxVal = values[x].Max();
							if (minVal < 0) {
								lineNum = Array.FindIndex(values[x], y => y <= 0);
								PrintError("Households", "hhsize", minVal, lineNum);
								errorCount++;
							}
							if (maxVal > Global.Settings.MaxInputs.MaxHhSize) {
								lineNum = Array.FindIndex(values[x], y => y > Global.Settings.MaxInputs.MaxHhSize);
								PrintWarning("Households", "hhsize", maxVal, lineNum);
							}
							break;
						case "hhparcel":
							if (values[x].Except(parcels.Keys).Count() > 0) {
								var badPcls = values[x].Except(parcels.Keys);
								foreach (int pcl in badPcls) {
									int index = Array.FindIndex(values[x], y => y == pcl);
									PrintError("Households", "hhparcel", pcl, index);
									errorCount++;
								}
							}
							break;
						default:
							break;
					}
				}
				x++;
			}

			//create a dictionary with hhno, household size to compare against the persons table
			households = values[0].Zip(values[1], (k, v) => new { k, v }).ToDictionary(o => o.k, o => o.v);
			return households;
		}

		private static void TestPersons(Dictionary<int, int> households, Dictionary<int, int> parcels) {
			var personKeys = new Dictionary<Tuple<int, int>, int>();
			string[] checkVectors = { "hhno", "pno", "pptyp", "pagey", "pgend", 
												"pwtyp", "pstyp", "pwpcl", "pspcl", };
			string path = Global.GetInputPath(Global.Configuration.RosterPath).ToFile().DirectoryName;

			string hdfFile = path + "\\" + Global.Configuration.HDF5Filename;
			var dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
			string baseDataSetName = "/Person/";

			// add a extra parcel id for the -1s, that are used for worker parcels, and student parcels
			// of non-workers and non-students
			parcels.Add(-1, parcels.Values.Max() + 1);
			int nVectors = checkVectors.Count();

			string[] headers = new string[nVectors + 2];
			Int32[][] values = new Int32[nVectors][];
			int x = 0;
			int size = -1;
			int minVal;
			int maxVal;

			foreach (string vector in checkVectors) {
				if (x == 0) {
					headers[1] = vector;
					headers[0] = "id";
				}
				else {
					headers[x + 1] = vector;
				}
				values[x] = GetInt32DataSet(dataFile, baseDataSetName + vector);

				//first check the data vector exists and it's the right size
				if (values[x] == null)
					throw new Exception(vector + " column does not exist for Person");
				int vSize = values[x].Count();
				if (size == -1)
					size = vSize;
				else {
					if (vSize != size) {
						throw new Exception(vector + " column for Person is the wrong size " + size + " vs " + vSize);
					}
				}

				//now check that the values are in a reasonable range
				switch (vector) {
					case "hhno":
						//first check to see that the household numbers also exist on the households table.
						if (values[x].Except(households.Keys).Count() > 0) {
							var badPeople = values[x].Except(households.Keys);
							foreach (int badPerson in badPeople) {
								int index = Array.FindIndex(values[x], y => y == badPerson);
								PrintWarning("Persons", "hhno", badPerson, index);
							}
							Global.PrintFile.WriteLine("Fatal error: Some of the household parcels ids are not on the parcel file.");
							Environment.Exit(2);
						}
						// check also to see that the number of persons == hhsize
						int countPersons = values[x].Count();
						int sumHHPersons = households.Values.Sum();
						int lineNum;
						if (countPersons != sumHHPersons) {
							Console.WriteLine("The persons file doesn't have the same number of records as the household file sum of the field hhsize.");
							// this will make the code crash later anyway, so stop it now.
							Global.PrintFile.WriteLine("Fatal error:The persons file doesn't have the same number of records as the household file sum of the field hhsize.");
							errorCount++;
						}
						break;
					case "pptyp":
						minVal = values[x].Min();
						maxVal = values[x].Max();
						if (minVal < Global.Settings.PersonTypes.FullTimeWorker) {
							lineNum = Array.FindIndex(values[x], y => y <Global.Settings.PersonTypes.FullTimeWorker);
							PrintWarning("Persons", "pptyp", minVal, lineNum);
						}
						if (maxVal > Global.Settings.PersonTypes.ChildUnder5) {
							lineNum = Array.FindIndex(values[x], y => y>Global.Settings.PersonTypes.ChildUnder5);
							PrintWarning("Persons", "pptyp", maxVal, lineNum);
						}
						break;
					case "pagey":
						minVal = values[x].Min();
						maxVal = values[x].Max();
						if (minVal < 0) {
							lineNum = Array.FindIndex(values[x], y => y < 0);
							PrintWarning("Persons", "pagey", minVal, lineNum);
						}
						if (maxVal > Global.Settings.MaxInputs.MaxAge) {
							lineNum = Array.FindIndex(values[x], y => y > Global.Settings.MaxInputs.MaxAge);
							PrintWarning("Persons", "pagey", maxVal, lineNum);
						}
						break;
					case "pgend":
						minVal = values[x].Min();
						maxVal = values[x].Max();
						if (minVal < 0) {
							lineNum = Array.FindIndex(values[x], y => y < 0);
							PrintWarning("Persons", "pgend", minVal, lineNum);
						}
						if (maxVal > Global.Settings.MaxInputs.MaxGend) {
							lineNum = Array.FindIndex(values[x], y => y > Global.Settings.MaxInputs.MaxGend);
							PrintWarning("Persons", "pgend", maxVal, lineNum);
						}
						break;
					case "pwtyp":
						minVal = values[x].Min();
						maxVal = values[x].Max();
						if (minVal < 0) {
							lineNum = Array.FindIndex(values[x], y => y < 0);
							PrintWarning("Persons", "pwtyp", minVal, lineNum);
						}
						if (maxVal > Global.Settings.MaxInputs.MaxPwtyp) {
							lineNum = Array.FindIndex(values[x], y => y > Global.Settings.MaxInputs.MaxPwtyp);
							PrintWarning("Persons", "pwtyp", maxVal, lineNum);
						}
						break;
					case "pwpcl":
						if (values[x].Except(parcels.Keys).Count() > 0) {
							var badPcls = values[x].Except(parcels.Keys);
							foreach (int pcl in badPcls) {
								int index = Array.FindIndex(values[x], y => y == pcl);
								PrintError("Persons", "pwpcl", pcl, index);
								errorCount++;
							}
						}
						break;
					case "pspcl":
						if (values[x].Except(parcels.Keys).Count() > 0) {
							var badPcls = values[x].Except(parcels.Keys);
							foreach (int pcl in badPcls) {
								int index = Array.FindIndex(values[x], y => y == pcl);
								PrintWarning("Persons", "pspcl", pcl, index);
							}
						}
						break;
					case "pstyp":
						minVal = values[x].Min();
						maxVal = values[x].Max();
						if (minVal < 0) {
							lineNum = Array.FindIndex(values[x], y => y < 0);
							PrintWarning("Persons", "pstyp", minVal, lineNum);
						}
						if (maxVal > Global.Settings.MaxInputs.MaxPstyp) {
							lineNum = Array.FindIndex(values[x], y => y > Global.Settings.MaxInputs.MaxPstyp);
							PrintWarning("Persons", "pstyp", maxVal, lineNum);
						}
						break;
				}

				x++;
			}
		}

		private static void TestIxxi(Dictionary<int, int> zones) {
			// the ixxi dictionary holds, for each zone, the share of workers living in the region, 
			// who work outside and vice versa
			var ixxi = new Dictionary<int, Tuple<double, double>>();
			var file = Global.GetInputPath(Global.Configuration.IxxiPath).ToFile();

			using (var reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
				int lineNum = 1;

				if (Global.Configuration.IxxiFirstLineIsHeader) {
					reader.ReadLine();
					lineNum++;
				}

				try {
					string line;
					while ((line = reader.ReadLine()) != null) {
						var row = line.Split(new[] { Global.Configuration.IxxiDelimiter }, StringSplitOptions.RemoveEmptyEntries);
						var zoneId = Convert.ToInt32(row[0]);
						if (!zones.ContainsKey(zoneId)) {
							PrintWarning("Internal-external worker", "zoneId", zoneId, lineNum);
						}

						var workersWithJobsOutside = Convert.ToDouble(row[1]);
						{
							if (workersWithJobsOutside > 1 || workersWithJobsOutside < 0) {
								PrintWarning("Internal-external worker", "fraction of workers with jobs outside", workersWithJobsOutside, lineNum);
							}
						}
						var workersWithJobsFilledFromOutside = Convert.ToDouble(row[2]);
						if (workersWithJobsFilledFromOutside > 1 || workersWithJobsFilledFromOutside < 0) {
							PrintWarning("Internal-external worker", "fraction of workers with jobs filled from outside", workersWithJobsOutside, lineNum);
						}
						lineNum++;
					}
				}
				catch (Exception e) {
					throw new Exception("Error reading internal-external worker file on line " + lineNum, innerException: e);
				}
			}
		}

		private static void TestParkAndRideNodes(Dictionary<int, int> zones) {
			// this dictionary holds the
			var parkAndRideNodes = new Dictionary<int, Tuple<int, int, int, int, int>>();
			var rawFile = Global.GetInputPath(Global.Configuration.RawParkAndRideNodePath).ToFile();
			var inputFile = Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile();
			int lineNum = 1;

			try {
				using (var reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
					reader.ReadLine();

					string line;

					while ((line = reader.ReadLine()) != null) {
						var row = line.Split(new[] { Global.Configuration.RawParkAndRideNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
						var id = Convert.ToInt32(row[0]);
						var zoneId = Convert.ToInt32(row[1]);

						if (!zones.ContainsKey(zoneId)) {
							PrintWarning("Park and Ride Node", "zoneID", zoneId, lineNum);
						}

						var capacity = Convert.ToInt32(row[4]);
						if (capacity < 0 || capacity > Global.Settings.MaxInputs.MaxPnrCap) {
							PrintWarning("Park and Ride Node", "Capacity",capacity, lineNum);
						}
						var cost = Convert.ToInt32(row[5]);
						if (cost < 0 || cost > Global.Settings.MaxInputs.MaxPnrCost) {
							PrintWarning("Park and Ride Node", "cost", cost, lineNum);
						}
						lineNum++;
					}
				}
			}
			catch (Exception e) {
				throw new Exception("Error reading Park and Ride Nodes file on line " + lineNum, innerException: e);
			}
		}

		//Reading and Printing Methods
		private static double[] GetDoubleDataSet(H5FileId dataFile, string path) {
			if (H5L.Exists(dataFile, path)) {
				var dataSet = H5D.open(dataFile, path);
				var space = H5D.getSpace(dataSet);
				var size2 = H5S.getSimpleExtentDims(space);
				long count = size2[0];
				var dataArray = new double[count];
				var wrapArray = new H5Array<double>(dataArray);
				H5DataTypeId tid1 = H5D.getType(dataSet);

				H5D.read(dataSet, tid1, wrapArray);

				return dataArray;
			}
			return null;
		}

		private static int[] GetInt32DataSet(H5FileId dataFile, string path) {
			if (H5L.Exists(dataFile, path)) {
				var dataSet = H5D.open(dataFile, path);
				var space = H5D.getSpace(dataSet);
				var size2 = H5S.getSimpleExtentDims(space);
				long count = size2[0];
				var dataArray = new Int32[count];
				var wrapArray = new H5Array<Int32>(dataArray);
				H5DataTypeId tid1 = H5D.getType(dataSet);

				H5D.read(dataSet, tid1, wrapArray);

				return dataArray;
			}
			return null;
		}

		private static void PrintWarning(string fileName, string varName, int badValue, int lineNumber) {
			Console.WriteLine("Warning: The {0} file contains an invalid value {1} for {2} on line {3}", fileName, badValue, varName, lineNumber);
			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("Warning: The {0} file contains an invalid value {0} for {1} which is {2} on line {3}", fileName, varName, badValue, lineNumber);
			}
		}

		private static void PrintWarning(string fileName, string varName, double badValue, int lineNumber) {
			Console.WriteLine("Warning: The {0} file contains an invalid value {1} for {2} on line {3}", fileName, badValue, varName, lineNumber);
			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("Warning: The {0} file contains an invalid value {0} for {1} which is {2} on line {3}", fileName, varName, badValue, lineNumber);
			}
		}

		private static void PrintError(string fileName, string varName, int badValue, int lineNumber) {
			Console.WriteLine("Fatal Error: The {0} file contains an invalid value {1} for {2} on line {3}", fileName, badValue, varName, lineNumber);
			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("Fatal Error: The {0} file contains an invalid value {0} for {1} which is {2} on line {3}", fileName, varName, badValue, lineNumber);
			}
		}

		private static void PrintError(string fileName, string varName, int badValue, int lineNumber, int colID) {
			Console.WriteLine("The {0} file contains an invalid value {1} for {2} on line {3} in 0-based index column {4}", fileName, badValue, varName, lineNumber, colID);
			if (Global.PrintFile != null) {
				Global.PrintFile.WriteLine("Fatal Error: The {0} file contains an invalid value {0} for {1} which is {2} on line {3} in 0-based index column {4}", fileName, varName, badValue, lineNumber, colID);
			}
		}

		private static Dictionary<string, int> ParseHeader(TextReader reader, char delimiter) {
			var line = reader.ReadLine();

			if (line == null) {
				throw new MissingHeaderException("The header is missing from the file. Please ensure that the raw file contains the appropriate header.");
			}

			var header = new Dictionary<string, int>();
			var fields = line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

			for (var i = 0; i < fields.Length; i++) {
				header.Add(fields[i], i);
			}

			return header;
		}
	}
}