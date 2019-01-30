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
  public static class RawConverter {
    /*
            public static void RunTestMode()
            {
                var ixxi = ImportIxxiTestMode();
                var parkAndRideNodes = ImportParkAndRideNodesTestMode();
                var parcelNodes = ImportParcelNodesTestMode();

                ConvertParcelNodeFile(parcelNodes);

                var zones = ConvertZoneFileTestMode(ref ixxi, ref parkAndRideNodes);
                var transitStopAreas = ConvertTransitStopAreaFileTestMode(ref ixxi, ref parkAndRideNodes);
                var parcels = ConvertParcelFileTestMode(parcelNodes, zones);


                ConvertParkAndRideNodeFile(parkAndRideNodes, parcels);
                Dictionary<Tuple<int,int>,int> personKeys;

                if (Global.Configuration.ReadHDF5)
                {
                    ConvertHouseholdFileHDF5(parcels, ixxi);
                    personKeys = ConvertPersonFileHDF5();
                }
                else
                {
                    ConvertHouseholdFile(parcels, ixxi);
                    personKeys = ConvertPersonFile();
                }

                if (Global.Configuration.ImportHouseholdDays)
                {
                    var householdDayKeys = ConvertHouseholdDayFile();

                    if (Global.Configuration.ImportPersonDays)
                    {
                        var personDayKeys = ConvertPersonDayFile(personKeys, householdDayKeys);
                        var tourKeys = ConvertTourFile(personKeys, personDayKeys);

                        ConvertTripFile(tourKeys);
                    }
                    if (Global.Settings.UseJointTours) {
                        ConvertJointTourFile(householdDayKeys);
                        ConvertFullHalfTourFile(householdDayKeys);
                        ConvertPartialHalfTourFile(householdDayKeys);
                    }
                }

            }
    */
    public static void Run() {
      Dictionary<int, Tuple<double, double>> ixxi = ImportIxxi();

      Dictionary<int, Tuple<int, int, int>> destinationParkingNodes = ConvertDestinationParkingNodeFile();

      Dictionary<int, int> parcelNodes = ImportParcelNodes();

      ConvertParcelNodeFile(parcelNodes);

      Dictionary<int, Tuple<int, int, int>> transitStopAreas = ConvertTransitStopAreaFile();

      Dictionary<int, int> zones = new Dictionary<int, int>();
      Dictionary<int, Tuple<int, int, int>> parcels = new Dictionary<int, Tuple<int, int, int>>();

      if (Global.Configuration.DataType == "Actum") {
        zones = ConvertActumZoneFile(ref ixxi, ref transitStopAreas);
        parcels = ConvertActumParcelFile(parcelNodes, zones);
        ConvertActumParkAndRideNodeFile(parcels, transitStopAreas);
      } else {
        Dictionary<int, Tuple<int, int, int, int, int>> parkAndRideNodes = ImportParkAndRideNodes();
        zones = ConvertZoneFile(ref ixxi, ref parkAndRideNodes, ref transitStopAreas);
        parcels = ConvertParcelFile(parcelNodes, zones);
        ConvertParkAndRideNodeFile(parkAndRideNodes, parcels, transitStopAreas);
      }

      Dictionary<Tuple<int, int>, int> personKeys;

      if (Global.Configuration.ReadHDF5) {
        ConvertHouseholdFileHDF5(parcels, ixxi);
        personKeys = ConvertPersonFileHDF5();
      } else {
        ConvertHouseholdFile(parcels, ixxi);
        personKeys = ConvertPersonFile();
      }

      if (Global.Configuration.ImportHouseholdDays) {
        Dictionary<Tuple<int, int>, int> householdDayKeys = ConvertHouseholdDayFile();

        if (Global.Configuration.ImportPersonDays) {
          Dictionary<Tuple<int, int, int>, int> personDayKeys = ConvertPersonDayFile(personKeys, householdDayKeys);
          Dictionary<Tuple<int, int, int, int>, int> tourKeys = ConvertTourFile(personKeys, personDayKeys);

          ConvertTripFile(tourKeys);
        }
        if (Global.Settings.UseJointTours) {
          ConvertJointTourFile(householdDayKeys);
          ConvertFullHalfTourFile(householdDayKeys);
          ConvertPartialHalfTourFile(householdDayKeys);
        }
      }
    }
    /*
            private static Dictionary<int, Tuple<double, double>> ImportIxxiTestMode()
            {
                if (string.IsNullOrEmpty(Global.Configuration.IxxiPath))
                {
                    return new Dictionary<int, Tuple<double, double>>();
                }

                var ixxi = new Dictionary<int, Tuple<double, double>>();
                var file = Global.GetInputPath(Global.Configuration.IxxiPath).ToFile();

                if (Global.PrintFile != null)
                {
                    Global.PrintFile.WriteFileInfo(file, true);
                }

                using (var reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    int i = 1;

                    if (Global.Configuration.IxxiFirstLineIsHeader)
                    {
                        reader.ReadLine();
                        i++;
                    }

                    try
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var row = line.Split(new[] {Global.Configuration.IxxiDelimiter}, StringSplitOptions.RemoveEmptyEntries);
                            var zoneId = Convert.ToInt32(row[0]);
                            var workersWithJobsOutside = Convert.ToDouble(row[1]);
                            var workersWithJobsFilledFromOutside = Convert.ToDouble(row[2]);

                            if (!ixxi.ContainsKey(zoneId))
                            {
                                ixxi.Add(zoneId, new Tuple<double, double>(workersWithJobsOutside, workersWithJobsFilledFromOutside));
                            }
                            i++;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error reading ixxi file on line " + i, innerException:e);
                    }

                }
                return ixxi;
            }
    */
    private static Dictionary<int, Tuple<double, double>> ImportIxxi() {
      if (string.IsNullOrEmpty(Global.Configuration.IxxiPath)) {
        return new Dictionary<int, Tuple<double, double>>();
      }

      Dictionary<int, Tuple<double, double>> ixxi = new Dictionary<int, Tuple<double, double>>();
      FileInfo file = Global.GetInputPath(Global.Configuration.IxxiPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(file, true);
      }

      using (CountingReader reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        if (Global.Configuration.IxxiFirstLineIsHeader) {
          reader.ReadLine();
        }

        string line = null;
        try {
          while ((line = reader.ReadLine()) != null) {
            string[] row = line.Split(new[] { Global.Configuration.IxxiDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            int zoneId = Convert.ToInt32(row[0]);
            double workersWithJobsOutside = Convert.ToDouble(row[1]);
            double workersWithJobsFilledFromOutside = Convert.ToDouble(row[2]);

            if (!ixxi.ContainsKey(zoneId)) {
              ixxi.Add(zoneId, new Tuple<double, double>(workersWithJobsOutside, workersWithJobsFilledFromOutside));
            }
          }
        } catch (FormatException e) {
          throw new Exception("Format problem in file '" + file.FullName + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
        }
      }

      return ixxi;
    }


    private static Dictionary<int, Tuple<int, int, int, int, int>> ImportParkAndRideNodes() {
      if (!Global.ParkAndRideNodeIsEnabled) {
        return new Dictionary<int, Tuple<int, int, int, int, int>>();
      }

      Dictionary<int, Tuple<int, int, int, int, int>> parkAndRideNodes = new Dictionary<int, Tuple<int, int, int, int, int>>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawParkAndRideNodePath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        reader.ReadLine();

        string line = null;

        try {
          while ((line = reader.ReadLine()) != null) {
            string[] row = line.Split(new[] { Global.Configuration.RawParkAndRideNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            int id = Convert.ToInt32(row[0]);
            int zoneId = Convert.ToInt32(row[1]);
            int xCoordinate = Convert.ToInt32(Math.Round(double.Parse(row[2])));
            int yCoordinate = Convert.ToInt32(Math.Round(double.Parse(row[3])));
            int capacity = Convert.ToInt32(row[4]);
            int cost = Convert.ToInt32(row[5]);

            if (!parkAndRideNodes.ContainsKey(id)) {
              parkAndRideNodes.Add(id, new Tuple<int, int, int, int, int>(zoneId, xCoordinate, yCoordinate, capacity, cost));
            }
          }   //end while
        } catch (FormatException e) {
          throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
        }
      }

      return parkAndRideNodes;
    }



    /*
    private static Dictionary<int, Tuple<int, int, int, int, int>> ImportParkAndRideNodesTestMode() {
        if (!Global.ParkAndRideNodeIsEnabled) {
            return new Dictionary<int, Tuple<int, int, int, int, int>>();
        }

        var parkAndRideNodes = new Dictionary<int, Tuple<int, int, int, int, int>>();
        var rawFile = Global.GetInputPath(Global.Configuration.RawParkAndRideNodePath).ToFile();
        var inputFile = Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile();

        if (Global.PrintFile != null) {
            Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
        }
        int i = 1;
        try
        {

        using (var reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
            reader.ReadLine();

            string line;

            while ((line = reader.ReadLine()) != null) {
                var row = line.Split(new[] {Global.Configuration.RawParkAndRideNodeDelimiter}, StringSplitOptions.RemoveEmptyEntries);
                var id = Convert.ToInt32(row[0]);
                var zoneId = Convert.ToInt32(row[1]);
                var xCoordinate = Convert.ToInt32(row[2]);
                var yCoordinate = Convert.ToInt32(row[3]);
                var capacity = Convert.ToInt32(row[4]);
                var cost = Convert.ToInt32(row[5]);

                if (!parkAndRideNodes.ContainsKey(id)) {
                    parkAndRideNodes.Add(id, new Tuple<int, int, int, int, int>(zoneId, xCoordinate, yCoordinate, capacity, cost));
                }
                i++;
            }
        }

        }
        catch (Exception e)
        {
            throw new Exception("Error reading Park and Ride Nodes file on line " + i, innerException:e);
        }

        return parkAndRideNodes;
    }
*/




    private static Dictionary<int, int> ImportParcelNodes() {
      if (!Global.ParcelNodeIsEnabled) {
        return new Dictionary<int, int>();
      }

      Dictionary<int, int> parcelNodes = new Dictionary<int, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawParcelNodePath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParcelNodePath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        reader.ReadLine();

        string line = null;
        try {
          while ((line = reader.ReadLine()) != null) {
            string[] row = line.Split(new[] { Global.Configuration.RawParcelNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            int parcelId = Convert.ToInt32(row[0]);
            int nodeId = Convert.ToInt32(row[1]);

            if (!parcelNodes.ContainsKey(parcelId)) {
              parcelNodes.Add(parcelId, nodeId);
            }
          }
        } catch (FormatException e) {
          throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
        }

      }

      return parcelNodes;
    }
    /*
            private static Dictionary<int, int> ImportParcelNodesTestMode() {
                if (!Global.ParcelNodeIsEnabled) {
                    return new Dictionary<int, int>();
                }

                var parcelNodes = new Dictionary<int, int>();
                var rawFile = Global.GetInputPath(Global.Configuration.RawParcelNodePath).ToFile();
                var inputFile = Global.GetInputPath(Global.Configuration.InputParcelNodePath).ToFile();

                if (Global.PrintFile != null) {
                    Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
                }

                int i = 0;
                try
                {
                    using (var reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        reader.ReadLine();

                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var row = line.Split(new[] {Global.Configuration.RawParcelNodeDelimiter}, StringSplitOptions.RemoveEmptyEntries);
                            var parcelId = Convert.ToInt32(row[0]);
                            var nodeId = Convert.ToInt32(row[1]);

                            if (!parcelNodes.ContainsKey(parcelId))
                            {
                                parcelNodes.Add(parcelId, nodeId);
                            }
                            i++;
                        }
                    }

                }
                catch (Exception e)
                {
                    throw new Exception("Error reading Parcel Nodes file on line " + i, innerException:e);
                }

                return parcelNodes;
            }
    */
    private static void ConvertParcelNodeFile(Dictionary<int, int> parcelNodes) {
      if (!Global.ParcelNodeIsEnabled) {
        return;
      }

      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParcelNodePath).ToFile();

      using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
        writer.WriteHeader(Global.Configuration.InputParcelNodeDelimiter, "id", "node_id");

        foreach (KeyValuePair<int, int> entry in parcelNodes) {
          writer.Write(entry.Key); // id
          writer.Write(Global.Configuration.InputParcelNodeDelimiter);

          writer.Write(entry.Value); // node_id
          writer.WriteLine();
        }
      }
    }

    private static Dictionary<int, int> ConvertZoneFile(ref Dictionary<int, Tuple<double, double>> ixxi,
                                                                         ref Dictionary<int, Tuple<int, int, int, int, int>> parkAndRideNodes,
                                                                         ref Dictionary<int, Tuple<int, int, int>> transitStopAreas) {
      Dictionary<int, Tuple<double, double>> newIxxi = new Dictionary<int, Tuple<double, double>>();
      Dictionary<int, int> zones = new Dictionary<int, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawZonePath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputZonePath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteZoneHeader(reader, writer);

          int newId = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawZoneDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int originalId = int.Parse(row[0]);

              zones.Add(originalId, newId);
              int xCoordinate = (row.Length > 4) ? Convert.ToInt32(Math.Round(double.Parse(row[4]))) : 0;
              int yCoordinate = (row.Length > 4) ? Convert.ToInt32(Math.Round(double.Parse(row[5]))) : 0;

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:
                    Tuple<double, double> ixxiEntry;

                    if (!ixxi.TryGetValue(originalId, out ixxiEntry)) {
                      ixxiEntry = new Tuple<double, double>(0, 0);
                    }

                    newIxxi.Add(newId, ixxiEntry);

                    writer.Write(newId++); // id
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    writer.Write(originalId); // taz
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    writer.Write(ixxiEntry.Item1); // fraction_with_jobs_outside
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    writer.Write(ixxiEntry.Item2); // fraction_filled_by_workers_from_outside
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    break;
                  case 4: // xcoord
                    writer.Write(xCoordinate);
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    break;
                  case 5: // ycoord
                    writer.Write(yCoordinate);
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    break;
                  default:
                    writer.Write(row[i]);
                    writer.Write(Global.Configuration.InputZoneDelimiter);
                    break;
                }
              }
              if (row.Length <= 4) {
                //if no coordinates on input file, write 0 values for coordinates and nearest stop area
                writer.Write(0);
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write(0);
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write(0);
              } else if (string.IsNullOrEmpty(Global.Configuration.RawTransitStopAreaPath)) {
                //if no stop area input file, nearest stop area is 0
                writer.Write(0);
              } else {
                double shortDist = 9999999D;
                int shortId = 0;
                foreach (KeyValuePair<int, Tuple<int, int, int>> stopArea in transitStopAreas) {
                  double distance = Math.Sqrt(Math.Pow(Math.Abs(xCoordinate - stopArea.Value.Item2), 2)
                                                                    + Math.Pow(Math.Abs(yCoordinate - stopArea.Value.Item3), 2));
                  if (distance < shortDist) {
                    shortDist = distance;
                    shortId = stopArea.Key;
                  }
                }
                writer.Write(shortId);
                if (Global.PrintFile != null) {
                  Global.PrintFile.WriteLine("Zone {0} Feet to nearest stop area {1} ID of nearest stop area {2}", originalId, shortDist, shortId);
                }
              }
              writer.WriteLine();
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      ixxi = newIxxi;

      Dictionary<int, Tuple<int, int, int, int, int>> newParkAndRide = new Dictionary<int, Tuple<int, int, int, int, int>>();

      foreach (KeyValuePair<int, Tuple<int, int, int, int, int>> entry in parkAndRideNodes) {
        int key = entry.Key;
        Tuple<int, int, int, int, int> value = entry.Value;

        newParkAndRide.Add(key,
                                 new Tuple<int, int, int, int, int>(zones[value.Item1], value.Item2, value.Item3, value.Item4,
                                                                                value.Item5));
      }

      parkAndRideNodes = newParkAndRide;

      return zones;
    }

    private static Dictionary<int, int> ConvertActumZoneFile(ref Dictionary<int, Tuple<double, double>> ixxi,
                                                                         ref Dictionary<int, Tuple<int, int, int>> transitStopAreas) {
      Dictionary<int, Tuple<double, double>> newIxxi = new Dictionary<int, Tuple<double, double>>();
      Dictionary<int, int> zones = new Dictionary<int, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawZonePath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputZonePath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteZoneHeader(reader, writer);

          int newId = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawZoneDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int originalId = int.Parse(row[0]);

              zones.Add(originalId, newId);
              int xCoordinate = (row.Length > 4) ? Convert.ToInt32(Math.Round(double.Parse(row[4]))) : 0;
              int yCoordinate = (row.Length > 4) ? Convert.ToInt32(Math.Round(double.Parse(row[5]))) : 0;

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:
                    Tuple<double, double> ixxiEntry;

                    if (!ixxi.TryGetValue(originalId, out ixxiEntry)) {
                      ixxiEntry = new Tuple<double, double>(0, 0);
                    }

                    newIxxi.Add(newId, ixxiEntry);

                    writer.Write(newId++); // id
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    writer.Write(originalId); // taz
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    writer.Write(ixxiEntry.Item1); // fraction_with_jobs_outside
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    writer.Write(ixxiEntry.Item2); // fraction_filled_by_workers_from_outside
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    break;
                  case 4: // xcoord
                    writer.Write(xCoordinate);
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    break;
                  case 5: // ycoord
                    writer.Write(yCoordinate);
                    writer.Write(Global.Configuration.InputZoneDelimiter);

                    break;
                  default:
                    writer.Write(row[i]);

                    if (i != row.Length - 1) {
                      writer.Write(Global.Configuration.InputZoneDelimiter);
                    }

                    break;
                }
              }
              if (row.Length <= 4) {
                //if no coordinates on input file, write 0 values for coordinates and nearest stop area
                writer.Write(0);
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write(0);
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write(0);
              } else if (string.IsNullOrEmpty(Global.Configuration.RawTransitStopAreaPath)) {
                //if no stop area input file, nearest stop area is 0
                writer.Write(0);
              } else {
              }
              writer.WriteLine();
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      ixxi = newIxxi;

      return zones;
    }


    /*
    private static Dictionary<int, int> ConvertZoneFileTestMode(ref Dictionary<int, Tuple<double, double>> ixxi,
                                                                         ref Dictionary<int, Tuple<int, int, int, int, int>>
                                                                             parkAndRideNodes)
    {
        var newIxxi = new Dictionary<int, Tuple<double, double>>();
        var zones = new Dictionary<int, int>();
        var rawFile = Global.GetInputPath(Global.Configuration.RawZonePath).ToFile();
        var inputFile = Global.GetInputPath(Global.Configuration.InputZonePath).ToFile();

        if (Global.PrintFile != null)
        {
            Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
        }
        int z = 0;
        try
        {
            using (var reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                using (var writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read)))
                {
                    WriteZoneHeader(reader, writer);

                    var newId = 0;
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var row = line.Split(new[] {Global.Configuration.RawZoneDelimiter}, StringSplitOptions.RemoveEmptyEntries);
                        var originalId = int.Parse(row[0]);

                        zones.Add(originalId, newId);

                        for (var i = 0; i < row.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    Tuple<double, double> ixxiEntry;

                                    if (!ixxi.TryGetValue(originalId, out ixxiEntry))
                                    {
                                        ixxiEntry = new Tuple<double, double>(0, 0);
                                    }

                                    newIxxi.Add(newId, ixxiEntry);

                                    writer.Write(newId++); // id
                                    writer.Write(Global.Configuration.InputZoneDelimiter);

                                    writer.Write(originalId); // taz
                                    writer.Write(Global.Configuration.InputZoneDelimiter);

                                    writer.Write(ixxiEntry.Item1); // fraction_with_jobs_outside
                                    writer.Write(Global.Configuration.InputZoneDelimiter);

                                    writer.Write(ixxiEntry.Item2); // fraction_filled_by_workers_from_outside

                                    break;
                                default:
                                    writer.Write(row[i]);

                                    break;
                            }

                            if (i == row.Length - 1)
                            {
                                writer.WriteLine();
                            }
                            else
                            {
                                writer.Write(Global.Configuration.InputZoneDelimiter);
                            }
                        }
                        z++;
                    }
                }
            }


        }
        catch (Exception e)
        {

            throw new Exception("Error reading Zone file on line " + z, innerException:e);
        }

        ixxi = newIxxi;

        var newParkAndRide = new Dictionary<int, Tuple<int, int, int, int, int>>();

        foreach (var entry in parkAndRideNodes)
        {
            var key = entry.Key;
            var value = entry.Value;

            newParkAndRide.Add(key,
                                     new Tuple<int, int, int, int, int>(zones[value.Item1], value.Item2, value.Item3, value.Item4,
                                                                                    value.Item5));
        }

        parkAndRideNodes = newParkAndRide;

        return zones;
    }
*/
    private static void WriteZoneHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the zone file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawZoneDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("id");
            writer.Write(Global.Configuration.InputZoneDelimiter);

            writer.Write("taz");
            writer.Write(Global.Configuration.InputZoneDelimiter);

            writer.Write("fraction_with_jobs_outside");
            writer.Write(Global.Configuration.InputZoneDelimiter);

            writer.Write("fraction_filled_by_workers_from_outside");
            writer.Write(Global.Configuration.InputZoneDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {

              if (fields.Length < 5) {
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write("xcoord");
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write("ycoord");
              }
              if (!(Global.Configuration.DataType == "Actum")) {
                writer.Write(Global.Configuration.InputZoneDelimiter);
                writer.Write("nearest_stoparea_id");
              }
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputZoneDelimiter);
            }

            break;
        }
      }
    }

    /*
            private static Dictionary<int, int> ConvertTransitStopAreaFileTestMode(
                ref Dictionary<int, Tuple<double, double>> ixxi, ref Dictionary<int, Tuple<int, int, int, int, int>> parkAndRideNodes)
            {

                if (String.IsNullOrEmpty(Global.Configuration.RawTransitStopAreaPath))
                    return null;


                //var newIxxi = new Dictionary<int, Tuple<double, double>>();
                var transitStopAreas = new Dictionary<int, int>();
                var rawFile = Global.GetInputPath(Global.Configuration.RawTransitStopAreaPath).ToFile();
                var inputFile = Global.GetInputPath(Global.Configuration.InputTransitStopAreaPath).ToFile();

                if (Global.PrintFile != null)
                {
                    Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
                }

                int z = 0;
                try
                {
                    using (var reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        using (var writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read)))
                        {
                            WriteTransitStopAreaHeader(reader, writer);

                            var newId = 0;
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                var row = line.Split(new[] {Global.Configuration.RawTransitStopAreaDelimiter},
                                                            StringSplitOptions.RemoveEmptyEntries);
                                var originalId = int.Parse(row[0]);

                                transitStopAreas.Add(originalId, newId);

                                for (var i = 0; i < row.Length; i++)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            Tuple<double, double> ixxiEntry;

                                            if (!ixxi.TryGetValue(originalId, out ixxiEntry))
                                            {
                                                ixxiEntry = new Tuple<double, double>(0, 0);
                                            }

                                            //newIxxi.Add(newId, ixxiEntry);

                                            writer.Write(newId++); // id
                                            writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                                            writer.Write(originalId); // taz
                                            writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                                            writer.Write(ixxiEntry.Item1); // fraction_with_jobs_outside
                                            writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                                            writer.Write(ixxiEntry.Item2); // fraction_filled_by_workers_from_outside

                                            break;
                                        default:
                                            writer.Write(row[i]);

                                            break;
                                    }

                                    if (i == row.Length - 1)
                                    {
                                        writer.WriteLine();
                                    }
                                    else
                                    {
                                        writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);
                                    }
                                }
                                z++;
                            }
                        }
                    }


                }
                catch (Exception e)
                {

                    throw new Exception("Error reading TransitStopArea file on line " + z, innerException: e);

                }

                //ixxi = newIxxi;

                var newParkAndRide = new Dictionary<int, Tuple<int, int, int, int, int>>();

                foreach (var entry in parkAndRideNodes)
                {
                    var key = entry.Key;
                    var value = entry.Value;

                    newParkAndRide.Add(key,
                                             new Tuple<int, int, int, int, int>(transitStopAreas[value.Item1], value.Item2, value.Item3,
                                                                                            value.Item4, value.Item5));
                }

                parkAndRideNodes = newParkAndRide;

                return transitStopAreas;
            }
    */
    private static Dictionary<int, Tuple<int, int, int>> ConvertTransitStopAreaFile() {

      if (string.IsNullOrEmpty(Global.Configuration.RawTransitStopAreaPath)) {
        return null;
      }

      Dictionary<int, Tuple<int, int, int>> transitStopAreas = new Dictionary<int, Tuple<int, int, int>>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawTransitStopAreaPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputTransitStopAreaPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {

          WriteTransitStopAreaHeader(reader, writer);
          int newId = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawTransitStopAreaDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int originalId = int.Parse(row[0]);
              int xcoord = Convert.ToInt32(Math.Round(double.Parse(row[4])));
              int ycoord = Convert.ToInt32(Math.Round(double.Parse(row[5])));

              transitStopAreas.Add(originalId, new Tuple<int, int, int>(newId, xcoord, ycoord));

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:

                    writer.Write(newId++); // id
                    writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                    writer.Write(originalId); // stoparea
                                              //writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                    break;
                  case 4: // xcoord
                    writer.Write(xcoord);
                    //writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                    break;
                  case 5: // ycoord
                    writer.Write(ycoord);
                    //writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

                    break;
                  default:
                    writer.Write(row[i]);

                    break;
                }

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }
        }
      }

      return transitStopAreas;
    }

    private static void WriteTransitStopAreaHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the transit stop area file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawTransitStopAreaDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("id");
            writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

            writer.Write("taz");
            writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputTransitStopAreaDelimiter);
            }

            break;
        }
      }
    }

    private static Dictionary<int, Tuple<int, int, int>> ConvertDestinationParkingNodeFile() {

      if (string.IsNullOrEmpty(Global.Configuration.RawDestinationParkingNodePath)) {
        return null;
      }

      Dictionary<int, Tuple<int, int, int>> destinationParkingNodes = new Dictionary<int, Tuple<int, int, int>>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawDestinationParkingNodePath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputDestinationParkingNodePath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {

          WriteDestinationParkingNodeHeader(reader, writer);
          int newId = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawDestinationParkingNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int originalId = int.Parse(row[0]);
              int xcoord = Convert.ToInt32(Math.Round(double.Parse(row[2])));
              int ycoord = Convert.ToInt32(Math.Round(double.Parse(row[3])));

              destinationParkingNodes.Add(originalId, new Tuple<int, int, int>(newId, xcoord, ycoord));

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:
                    writer.Write(originalId);

                    break;
                  case 2: // xcoord
                    writer.Write(xcoord);

                    break;
                  case 3: // ycoord
                    writer.Write(ycoord);

                    break;
                  default:
                    writer.Write(row[i]);

                    break;
                }

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputDestinationParkingNodeDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }
        }
      }

      return destinationParkingNodes;
    }

    private static void WriteDestinationParkingNodeHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the DestinationParkingNode file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawDestinationParkingNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("ID");
            writer.Write(Global.Configuration.InputDestinationParkingNodeDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputDestinationParkingNodeDelimiter);
            }

            break;
        }
      }
    }

    private static Dictionary<int, Tuple<int, int, int>> ConvertParcelFile(IDictionary<int, int> parcelNodes,
                                                                                                  IDictionary<int, int> zones) {
      Dictionary<int, Tuple<int, int, int>> parcels = new Dictionary<int, Tuple<int, int, int>>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawParcelPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParcelPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }


      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          bool hasShortDistanceCircuityMeasures = WriteParcelHeader(reader, writer);

          Dictionary<int, int> sequences = new Dictionary<int, int>();
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawParcelDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int id = int.Parse(row[0]);
              int originalZoneId = int.Parse(row[4]);
              int newZoneId = zones[originalZoneId];
              int xCoordinate = int.Parse(row[1]);
              int yCoordinate = Convert.ToInt32(Math.Round(double.Parse(row[2])));

              parcels.Add(id, new Tuple<int, int, int>(newZoneId, xCoordinate, yCoordinate));

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:
                    int sequence;

                    if (sequences.ContainsKey(newZoneId)) {
                      sequences[newZoneId]++;

                      sequence = sequences[newZoneId];
                    } else {
                      sequence = 0;

                      sequences.Add(newZoneId, sequence);
                    }

                    writer.Write(id); // id
                    writer.Write(Global.Configuration.InputParcelDelimiter);

                    writer.Write(sequence); // sequence
                    writer.Write(Global.Configuration.InputParcelDelimiter);

                    int nodeId;

                    writer.Write(parcelNodes.TryGetValue(id, out nodeId) ? nodeId : 0); // node_id
                    writer.Write(Global.Configuration.InputParcelDelimiter);

                    writer.Write(newZoneId); // zone_id

                    break;
                  case 2: // ycoord_p
                    writer.Write(Convert.ToInt32(Math.Round(double.Parse(row[i]))));

                    break;
                  case 3: // sqft_p
                    writer.Write(double.Parse(row[i]) / 1000);

                    break;
                  case 72: // dist_lbus
                  case 73: // dist_ebus
                  case 74: // dist_crt
                  case 75: // dist_fry
                  case 76: // dist_lrt
                    if ((double.Parse(row[i]) > 5) && (Global.Configuration.DataType != "Actum")) {
                      writer.Write(Constants.DEFAULT_VALUE);
                    } else {
                      writer.Write(row[i]);
                    }

                    break;
                  default:
                    writer.Write(row[i]);

                    break;
                }

                if (i == row.Length - 1) {
                  if (!hasShortDistanceCircuityMeasures) {
                    for (int j = 0; j < 24; j++) {
                      writer.Write(Global.Configuration.InputParcelDelimiter);
                      writer.Write("0");
                    }
                  }

                  if (Global.Configuration.DataType == "Actum") {
                    for (int j = 0; j < 12; j++) {
                      writer.Write(Global.Configuration.InputParcelDelimiter);
                      writer.Write("0");
                    }
                  }

                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputParcelDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      return parcels;
    }

    private static Dictionary<int, Tuple<int, int, int>> ConvertActumParcelFile(IDictionary<int, int> parcelNodes,
                                                                                                  IDictionary<int, int> zones) {
      Dictionary<int, Tuple<int, int, int>> parcels = new Dictionary<int, Tuple<int, int, int>>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawParcelPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParcelPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }


      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteActumParcelHeader(reader, writer);

          Dictionary<int, int> sequences = new Dictionary<int, int>();
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawParcelDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int id = int.Parse(row[0]);
              int originalZoneId = int.Parse(row[4]);
              int newZoneId = zones[originalZoneId];
              int xCoordinate = int.Parse(row[1]);
              int yCoordinate = Convert.ToInt32(Math.Round(double.Parse(row[2])));

              parcels.Add(id, new Tuple<int, int, int>(newZoneId, xCoordinate, yCoordinate));

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:
                    int sequence;

                    if (sequences.ContainsKey(newZoneId)) {
                      sequences[newZoneId]++;

                      sequence = sequences[newZoneId];
                    } else {
                      sequence = 0;

                      sequences.Add(newZoneId, sequence);
                    }

                    writer.Write(id); // id
                    writer.Write(Global.Configuration.InputParcelDelimiter);

                    writer.Write(sequence); // sequence
                    writer.Write(Global.Configuration.InputParcelDelimiter);

                    int nodeId;

                    writer.Write(parcelNodes.TryGetValue(id, out nodeId) ? nodeId : 0); // node_id
                    writer.Write(Global.Configuration.InputParcelDelimiter);

                    writer.Write(newZoneId); // zone_id

                    break;
                  case 2: // ycoord_p
                    writer.Write(Convert.ToInt32(Math.Round(double.Parse(row[i]))));

                    break;
                  case 3: // sqft_p
                    writer.Write(double.Parse(row[i]) / 1000);

                    break;

                  default:
                    writer.Write(row[i]);

                    break;
                }

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputParcelDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      return parcels;
    }


    /*
            private static Dictionary<int, Tuple<int, int, int>> ConvertParcelFileTestMode(IDictionary<int, int> parcelNodes,
                                                                                                          IDictionary<int, int> zones)
            {
                var parcels = new Dictionary<int, Tuple<int, int, int>>();
                var rawFile = Global.GetInputPath(Global.Configuration.RawParcelPath).ToFile();
                var inputFile = Global.GetInputPath(Global.Configuration.InputParcelPath).ToFile();

                if (Global.PrintFile != null)
                {
                    Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
                }


                int z = 0;
                try
                {
                    using (var reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        using (var writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read)))
                        {
                            var hasShortDistanceCircuityMeasures = WriteParcelHeader(reader, writer);

                            var sequences = new Dictionary<int, int>();
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                var row = line.Split(new[] {Global.Configuration.RawParcelDelimiter}, StringSplitOptions.RemoveEmptyEntries);
                                var id = int.Parse(row[0]);
                                var originalZoneId = int.Parse(row[4]);
                                var newZoneId = zones[originalZoneId];
                                var xCoordinate = int.Parse(row[1]);
                                var yCoordinate = Convert.ToInt32(Math.Round(double.Parse(row[2])));

                                parcels.Add(id, new Tuple<int, int, int>(newZoneId, xCoordinate, yCoordinate));

                                for (var i = 0; i < row.Length; i++)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            int sequence;

                                            if (sequences.ContainsKey(newZoneId))
                                            {
                                                sequences[newZoneId]++;

                                                sequence = sequences[newZoneId];
                                            }
                                            else
                                            {
                                                sequence = 0;

                                                sequences.Add(newZoneId, sequence);
                                            }

                                            writer.Write(id); // id
                                            writer.Write(Global.Configuration.InputParcelDelimiter);

                                            writer.Write(sequence); // sequence
                                            writer.Write(Global.Configuration.InputParcelDelimiter);

                                            int nodeId;

                                            writer.Write(parcelNodes.TryGetValue(id, out nodeId) ? nodeId : 0); // node_id
                                            writer.Write(Global.Configuration.InputParcelDelimiter);

                                            writer.Write(newZoneId); // zone_id

                                            break;
                                        case 1: // xcoord_p
                                            writer.Write(Convert.ToInt32(Math.Round(double.Parse(row[i]))));

                                            break;
                                        case 2: // ycoord_p
                                            writer.Write(Convert.ToInt32(Math.Round(double.Parse(row[i]))));

                                            break;
                                        case 3: // sqft_p
                                            writer.Write(double.Parse(row[i])/1000);

                                            break;
                                        case 72: // dist_lbus
                                        case 73: // dist_ebus
                                        case 74: // dist_crt
                                        case 75: // dist_fry
                                        case 76: // dist_lrt
                                            if (double.Parse(row[i]) > 5)
                                            {
                                                writer.Write(Constants.DEFAULT_VALUE);
                                            }
                                            else
                                            {
                                                writer.Write(row[i]);
                                            }

                                            break;
                                        default:
                                            writer.Write(row[i]);

                                            break;
                                    }

                                    if (i == row.Length - 1)
                                    {
                                        if (!hasShortDistanceCircuityMeasures)
                                        {
                                            for (var j = 0; j < 24; j++)
                                            {
                                                writer.Write(Global.Configuration.InputParcelDelimiter);
                                                writer.Write("0");
                                            }
                                        }

                                        writer.WriteLine();
                                    }
                                    else
                                    {
                                        writer.Write(Global.Configuration.InputParcelDelimiter);
                                    }
                                }
                            }
                        }
                    }


                }
                catch (Exception e)
                {

                    throw new Exception("Error reading Parcel file on line " + z, innerException: e);
                }

                return parcels;
            }
    */
    private static bool WriteParcelHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the parcel file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawParcelDelimiter }, StringSplitOptions.RemoveEmptyEntries);
      bool hasShortDistanceCircuityMeasures = false;

      foreach (string field in fields) {
        switch (field) {
          case "Circ_E1":
          case "Circ_E2":
          case "Circ_E3":
          case "Circ_NE1":
          case "Circ_NE2":
          case "Circ_NE3":
          case "Circ_N1":
          case "Circ_N2":
          case "Circ_N3":
          case "Circ_NW1":
          case "Circ_NW2":
          case "Circ_NW3":
          case "Circ_W1":
          case "Circ_W2":
          case "Circ_W3":
          case "Circ_SW1":
          case "Circ_SW2":
          case "Circ_SW3":
          case "Circ_S1":
          case "Circ_S2":
          case "Circ_S3":
          case "Circ_SE1":
          case "Circ_SE2":
          case "Circ_SE3":
            hasShortDistanceCircuityMeasures = true;
            break;
        }

        if (hasShortDistanceCircuityMeasures) {
          break;
        }
      }

      if (!hasShortDistanceCircuityMeasures && Global.Configuration.UseShortDistanceCircuityMeasures) {
        throw new MissingShortDistanceCircuityMeasuresException();
      }

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("id");
            writer.Write(Global.Configuration.InputParcelDelimiter);

            writer.Write("sequence");
            writer.Write(Global.Configuration.InputParcelDelimiter);

            writer.Write("node_id");
            writer.Write(Global.Configuration.InputParcelDelimiter);

            writer.Write("zone_id");
            writer.Write(Global.Configuration.InputParcelDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {
              if (!hasShortDistanceCircuityMeasures) {
                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_E1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_E2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_E3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_NE1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_NE2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_NE3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_N1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_N2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_N3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_NW1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_NW2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_NW3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_W1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_W2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_W3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_SW1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_SW2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_SW3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_S1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_S2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_S3");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_SE1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_SE2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("Circ_SE3");
              }

              if (Global.Configuration.DataType == "Actum") {
                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("parkdy_p");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("parkhr_p");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("ppricdyp");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("pprichrp");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("parkdy_1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("parkhr_1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("ppricdy1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("pprichr1");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("parkdy_2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("parkhr_2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("ppricdy2");

                writer.Write(Global.Configuration.InputParcelDelimiter);
                writer.Write("pprichr2");

              }

              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputParcelDelimiter);
            }

            break;
        }
      }

      return hasShortDistanceCircuityMeasures;
    }


    private static void WriteActumParcelHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the parcel file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawParcelDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        if (i == 0) {
          writer.Write("id");
          writer.Write(Global.Configuration.InputParcelDelimiter);

          writer.Write("sequence");
          writer.Write(Global.Configuration.InputParcelDelimiter);

          writer.Write("node_id");
          writer.Write(Global.Configuration.InputParcelDelimiter);

          writer.Write("zone_id");

        }
        else if (fields[i] == "District_p") { writer.Write("Circ_E1"); } 
        else if (fields[i] == "PkDatAv_p") { writer.Write("Circ_E2"); } 
        else if (fields[i] == "ResPk_p") { writer.Write("Circ_E3"); } 
        else if (fields[i] == "PubResPk_p") { writer.Write("Circ_NE1"); } 
        else if (fields[i] == "PubPk_p") { writer.Write("Circ_NE2"); } 
        else if (fields[i] == "EmpPk_p") { writer.Write("Circ_NE3"); } 
        else if (fields[i] == "ElecPk_p") { writer.Write("Circ_N1"); } 
        else if (fields[i] == "ResDayPr_p") { writer.Write("Circ_N2"); } 
        else if (fields[i] == "PubHrPr_p") { writer.Write("Circ_N3"); } 
        else if (fields[i] == "ResPk_1") { writer.Write("Circ_NW1"); } 
        else if (fields[i] == "PubResPk_1") { writer.Write("Circ_NW2"); } 
        else if (fields[i] == "PubPk_1") { writer.Write("Circ_NW3"); } 
        else if (fields[i] == "EmpPk_1") { writer.Write("Circ_W1"); } 
        else if (fields[i] == "ElecPk_1") { writer.Write("Circ_W2"); } 
        else if (fields[i] == "ResDayPr_1") { writer.Write("Circ_W3"); } 
        else if (fields[i] == "PubHrPr_1") { writer.Write("Circ_SW1"); } 
        else if (fields[i] == "ResPk_2") { writer.Write("Circ_SW2"); } 
        else if (fields[i] == "PubResPk_2") { writer.Write("Circ_SW3"); } 
        else if (fields[i] == "PubPk_2") { writer.Write("Circ_S1"); } 
        else if (fields[i] == "EmpPk_2") { writer.Write("Circ_S2"); } 
        else if (fields[i] == "ElecPk_2") { writer.Write("Circ_S3"); } 
        else if (fields[i] == "ResDayPr_2") { writer.Write("Circ_SE1"); } 
        else if (fields[i] == "PubHrPr_2") { writer.Write("Circ_SE2"); } 
        else if (fields[i] == "District2_p") { writer.Write("Circ_SE3"); } 
        else { writer.Write(fields[i]); }

        if (i < fields.Length - 1) { writer.Write(Global.Configuration.InputParcelDelimiter); } else { writer.WriteLine(); }
      }
    }
                            



    private static void ConvertParkAndRideNodeFile(Dictionary<int, Tuple<int, int, int, int, int>> parkAndRideNodes,
        Dictionary<int, Tuple<int, int, int>> parcels, Dictionary<int, Tuple<int, int, int>> transitStopAreas) {
      if (!Global.ParkAndRideNodeIsEnabled) {
        return;
      }

      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile();

      using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
        writer.WriteHeader(Global.Configuration.InputParkAndRideNodeDelimiter, "id", "zone_id", "xcoord", "ycoord", "capacity", "cost", "nearest_parcel_id", "nearest_stoparea_id");

        foreach (KeyValuePair<int, Tuple<int, int, int, int, int>> entry in parkAndRideNodes.Where(x => x.Value.Item1 != Constants.DEFAULT_VALUE)) {
          writer.Write(entry.Key); // id
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

          writer.Write(entry.Value.Item1); // zone_id
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

          writer.Write(entry.Value.Item2); // xcoord
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

          writer.Write(entry.Value.Item3); // ycoord
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

          writer.Write(entry.Value.Item4); // capacity
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

          writer.Write(entry.Value.Item5); // cost
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

          //find and write nearest parcel ID
          double shortDist = 9999999D;
          int shortId = 0;
          int shortZone = 0;
          foreach (KeyValuePair<int, Tuple<int, int, int>> parcel in parcels) {
            if (entry.Value.Item1 == parcel.Value.Item1) {
              double distance = Math.Sqrt(Math.Pow(Math.Abs(entry.Value.Item2 - parcel.Value.Item2), 2) + Math.Pow(Math.Abs(entry.Value.Item3 - parcel.Value.Item3), 2));
              if (distance < shortDist) {
                shortDist = distance;
                shortId = parcel.Key;
                shortZone = parcel.Value.Item1;
              }
            }
          }
          if (shortZone == 0) {
            //if no parcels in lot zone, allow a different zone
            foreach (KeyValuePair<int, Tuple<int, int, int>> parcel in parcels) {
              double distance = Math.Sqrt(Math.Pow(Math.Abs(entry.Value.Item2 - parcel.Value.Item2), 2) + Math.Pow(Math.Abs(entry.Value.Item3 - parcel.Value.Item3), 2));
              if (distance < shortDist) {
                shortDist = distance;
                shortId = parcel.Key;
                shortZone = parcel.Value.Item1;
              }
            }
          }
          writer.Write(shortId); // nearest_parcel_id
          writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);
          if (Global.PrintFile != null) {
            Global.PrintFile.WriteLine("PR zone {0} Feet to nearest parcel {1} Zone of nearest parcel {2}", entry.Value.Item1, shortDist, shortZone);
          }

          //if stop areas are input, find and write nearest stop area ID
          if (string.IsNullOrEmpty(Global.Configuration.RawTransitStopAreaPath)) {
            shortId = 0;
          } else {
            shortDist = 9999999D;
            shortId = 0;
            foreach (KeyValuePair<int, Tuple<int, int, int>> stopArea in transitStopAreas) {
              double distance = Math.Sqrt(Math.Pow(Math.Abs(entry.Value.Item2 - stopArea.Value.Item2), 2) + Math.Pow(Math.Abs(entry.Value.Item3 - stopArea.Value.Item3), 2));
              if (distance < shortDist) {
                shortDist = distance;
                shortId = stopArea.Key;
              }
            }
            if (Global.PrintFile != null) {
              Global.PrintFile.WriteLine("PR zone {0} Feet to nearest stop area {1} ID of nearest stop area {2}", entry.Value.Item1, shortDist, shortId);
            }
          }
          writer.Write(shortId); // nearest_stoparea_id
          writer.WriteLine();

        }
      }
    }


    private static void ConvertActumParkAndRideNodeFile(Dictionary<int, Tuple<int, int, int>> parcels, Dictionary<int, Tuple<int, int, int>> transitStopAreas) {

      if (!Global.ParkAndRideNodeIsEnabled) {
        return;
      }
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawParkAndRideNodePath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputParkAndRideNodePath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteActumParkAndRideNodeHeader(reader, writer);

          int newId = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawParkAndRideNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int originalId = int.Parse(row[0]);
              int xCoordinate = 0;
              int yCoordinate = 0;

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:

                    writer.Write(newId++); // id
                    writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

                    writer.Write(originalId); // Parking Lot ID
                    writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

                    writer.Write(xCoordinate); // zero value
                    writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

                    writer.Write(yCoordinate); // zero value
                                               //writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);


                    break;
                  default:
                    writer.Write(row[i]);

                    break;
                }

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

    }


    private static void ConvertHouseholdFile(IDictionary<int, Tuple<int, int, int>> parcels, IDictionary<int, Tuple<double, double>> ixxi) {
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawHouseholdPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputHouseholdPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteHouseholdHeader(reader, writer);

          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawHouseholdDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int parcelId = int.Parse(row[15]);
              //Console.WriteLine(parcelId);
              int newZoneId = parcels[parcelId].Item1;

              for (int i = 0; i < row.Length; i++) {
                switch (i) {
                  case 0:
                    Tuple<double, double> entry = ixxi[newZoneId];

                    writer.Write(row[i]); // hhno
                    writer.Write(Global.Configuration.InputHouseholdDelimiter);

                    writer.Write(newZoneId); // zone_id
                    writer.Write(Global.Configuration.InputHouseholdDelimiter);

                    writer.Write(entry.Item1); // fraction_with_jobs_outside

                    break;
                  default:
                    writer.Write(row[i]);

                    break;
                }

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputHouseholdDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }
    }

    private static void WriteHouseholdHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the household file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawHouseholdDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("hhno");
            writer.Write(Global.Configuration.InputHouseholdDelimiter);

            writer.Write("zone_id");
            writer.Write(Global.Configuration.InputHouseholdDelimiter);

            writer.Write("fraction_with_jobs_outside");
            writer.Write(Global.Configuration.InputHouseholdDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputHouseholdDelimiter);
            }

            break;
        }
      }
    }


    private static void ConvertHouseholdFileHDF5(IDictionary<int, Tuple<int, int, int>> parcels,
                                                                IDictionary<int, Tuple<double, double>> ixxi) {
      string[] essentials = { "hhno", "hhsize", "hhincome", "hhparcel" };
      int parcelIndex = 3;
      string[] importants =
          {
                    "hhvehs", "hhwkrs", "hhftw", "hhptw", "hhret", "hhoad", "hhuni", "hhhsc", "hh515", "hhcu5",
                    "hownrent", "hrestype", "hhtaz", "samptype"
                };
      string[] importantDoubles = { "hhexpfac" };
      //string[] optional = {};

      string path = Global.GetInputPath(Global.Configuration.RosterPath).ToFile().DirectoryName;

      string hdfFile = path + "\\" + Global.Configuration.HDF5Filename;
      H5FileId dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
      string baseDataSetName = "/Household/";

      int nEssentials = essentials.Count();
      int nImportants = importants.Count();
      int nImportantDoubles = importantDoubles.Count();
      string[] headers = new string[nEssentials + nImportants + nImportantDoubles + 2];

      int[][] values = new int[nEssentials + nImportants][];
      double[][] doubleValues = new double[nImportantDoubles][];
      int x = 0;
      int size = -1;
      foreach (string essential in essentials) {
        if (x == 0) {
          headers[0] = essential;
          headers[1] = "zone_id";
          headers[2] = "fraction_with_jobs_outside";
        } else {
          headers[x + 2] = essential;
        }
        values[x] = GetInt32DataSet(dataFile, baseDataSetName + essential);
        if (values[x] == null) {
          throw new Exception(essential + " column does not exist for Household");
        }

        int vSize = values[x].Count();
        if (size == -1) {
          size = vSize;
        } else {
          if (vSize != size) {
            throw new Exception(essential + " column for Household is the wrong size " + size + " vs " + vSize);
          }
        }
        x++;
      }

      foreach (string important in importants) {
        headers[x + 2] = important;
        values[x] = GetInt32DataSet(dataFile, baseDataSetName + important);
        if (values[x] == null) {
          values[x] = new int[size];
          for (int y = 0; y < size; y++) {
            values[x][y] = -1;
          }
        }
        int vSize = values[x].Count();
        if (size == -1) {
          size = vSize;
        } else {
          if (vSize != size) {
            throw new Exception(important + " column for Household is the wrong size " + size + " vs " + vSize);
          }
        }
        x++;
      }

      int z = 0;
      foreach (string important in importantDoubles) {

        headers[x + 2 + z] = important;

        //this is for hhexpfac which defaults to 1 if not in the H5 file 
        //and may need to be converted from int32 to float64
        if (IsIntegerDataSet(dataFile, baseDataSetName + important)) {
          int[] tempValues = new int[size];
          doubleValues[z] = new double[size];
          tempValues = GetInt32DataSet(dataFile, baseDataSetName + important);
          for (int y = 0; y < size; y++) {
            doubleValues[z][y] = tempValues[y];
          }
        }

        if (IsFloatDataSet(dataFile, baseDataSetName + important)) {
          doubleValues[z] = GetDoubleDataSet(dataFile, baseDataSetName + important);
        }

        if (doubleValues[z] == null) {
          doubleValues[z] = new double[size];
          for (int y = 0; y < size; y++) {
            doubleValues[z][y] = 1;
          }
        }
        int vSize = doubleValues[z].Count();
        if (size == -1) {
          size = vSize;
        } else {
          if (vSize != size) {
            throw new Exception(important + " column for Household is the wrong size " + size + " vs " + vSize);
          }
        }
        z++;
      }

      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputHouseholdPath).ToFile();

      using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
        WriteHeader(headers, writer, Global.Configuration.InputHouseholdDelimiter);

        for (int s = 0; s < size; s++) {
          int parcelId = values[parcelIndex][s];
          int newZoneId = parcels[parcelId].Item1;

          for (int i = 0; i < nEssentials + nImportants; i++) {
            switch (i) {
              case 0:
                Tuple<double, double> entry = ixxi[newZoneId];

                writer.Write(values[i][s]); // hhno
                writer.Write(Global.Configuration.InputHouseholdDelimiter);

                writer.Write(newZoneId); // zone_id
                writer.Write(Global.Configuration.InputHouseholdDelimiter);

                writer.Write(entry.Item1); // fraction_with_jobs_outside

                break;
              default:
                writer.Write(values[i][s]);

                break;
            }
            writer.Write(Global.Configuration.InputHouseholdDelimiter);
          }
          for (int i = 0; i < nImportantDoubles; i++) {

            writer.Write(doubleValues[i][s]);

            if (i == nImportantDoubles - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputHouseholdDelimiter);
            }
          }
        }
      }

    }

    private static void WriteHeader(string[] headers, StreamWriter writer, char delimeter) {
      for (int i = 0; i < headers.Length; i++) {
        writer.Write(headers[i]);

        if (i == headers.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(delimeter);
        }
      }
    }

    private static double[] GetDoubleDataSet(H5FileId dataFile, string path) {
      if (H5L.Exists(dataFile, path)) {
        H5DataSetId dataSet = H5D.open(dataFile, path);
        H5DataSpaceId space = H5D.getSpace(dataSet);
        long[] size2 = H5S.getSimpleExtentDims(space);
        long count = size2[0];
        double[] dataArray = new double[count];
        H5Array<double> wrapArray = new H5Array<double>(dataArray);
        H5DataTypeId tid1 = H5D.getType(dataSet);

        H5D.read(dataSet, tid1, wrapArray);

        return dataArray;
      }
      return null;
    }

    private static bool IsIntegerDataSet(H5FileId dataFile, string path) {
      H5DataSetId dataSet = H5D.open(dataFile, path);
      H5T.H5TClass shCl = H5T.getClass(H5D.getType(dataSet));
      if (shCl.Equals(H5T.H5TClass.INTEGER)) {
        return true;
      } else {
        return false;
      }
    }

    private static bool IsFloatDataSet(H5FileId dataFile, string path) {
      H5DataSetId dataSet = H5D.open(dataFile, path);
      H5T.H5TClass shCl = H5T.getClass(H5D.getType(dataSet));
      if (shCl.Equals(H5T.H5TClass.FLOAT)) {
        return true;
      } else {
        return false;
      }
    }

    private static int[] GetInt32DataSet(H5FileId dataFile, string path) {
      if (H5L.Exists(dataFile, path)) {
        H5DataSetId dataSet = H5D.open(dataFile, path);
        H5DataSpaceId space = H5D.getSpace(dataSet);
        long[] size2 = H5S.getSimpleExtentDims(space);
        long count = size2[0];
        int[] dataArray = new int[count];
        H5Array<int> wrapArray = new H5Array<int>(dataArray);
        H5DataTypeId tid1 = H5D.getType(dataSet);

        H5D.read(dataSet, tid1, wrapArray);

        return dataArray;
      }
      return null;
    }

    private static void WriteHouseholdHeaderHDF5(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the household file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawHouseholdDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("hhno");
            writer.Write(Global.Configuration.InputHouseholdDelimiter);

            writer.Write("zone_id");
            writer.Write(Global.Configuration.InputHouseholdDelimiter);

            writer.Write("fraction_with_jobs_outside");
            writer.Write(Global.Configuration.InputHouseholdDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputHouseholdDelimiter);
            }

            break;
        }
      }
    }

    private static Dictionary<Tuple<int, int>, int> ConvertPersonFileHDF5() {
      Dictionary<Tuple<int, int>, int> personKeys = new Dictionary<Tuple<int, int>, int>();
      string[] essentials = { "hhno", "pno", "pptyp", "pagey", "pgend", "pwtyp", "pstyp" };
      //            int parcelIndex = 3;
      string[] importants =
          {
                    "pwpcl", "pwtaz", "pspcl", "pstaz", "puwmode", "puwarrp", "puwdepp", "ptpass", "ppaidprk", "pdiary", "pproxy"
                };
      string[] importantDoubles = { "pwautime", "pwaudist", "psautime", "psaudist", "psexpfac" };
      //string[] optional = {};

      string path = Global.GetInputPath(Global.Configuration.RosterPath).ToFile().DirectoryName;

      string hdfFile = path + "\\" + Global.Configuration.HDF5Filename;
      H5FileId dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
      string baseDataSetName = "/Person/";

      int nEssentials = essentials.Count();
      int nImportants = importants.Count();
      int nImportantDoubles = importantDoubles.Count();
      string[] headers = new string[nEssentials + nImportants + nImportantDoubles + 1];

      int[][] values = new int[nEssentials + nImportants][];
      double[][] doubleValues = new double[nImportantDoubles][];
      int x = 0;
      int size = -1;
      foreach (string essential in essentials) {
        if (x == 0) {
          headers[1] = essential;
          headers[0] = "id";
        } else {
          headers[x + 1] = essential;
        }
        values[x] = GetInt32DataSet(dataFile, baseDataSetName + essential);
        if (values[x] == null) {
          throw new Exception(essential + " column does not exist for Person");
        }

        int vSize = values[x].Count();
        if (size == -1) {
          size = vSize;
        } else {
          if (vSize != size) {
            throw new Exception(essential + " column for Person is the wrong size " + size + " vs " + vSize);
          }
        }
        x++;
      }

      foreach (string important in importants) {
        headers[x + 1] = important;
        values[x] = GetInt32DataSet(dataFile, baseDataSetName + important);
        if (values[x] == null) {
          values[x] = new int[size];
          for (int y = 0; y < size; y++) {
            values[x][y] = Constants.DEFAULT_VALUE;
          }
        }
        int vSize = values[x].Count();
        if (size == -1) {
          size = vSize;
        } else {
          if (vSize != size) {
            throw new Exception(important + " column for Person is the wrong size " + size + " vs " + vSize);
          }
        }
        x++;
      }

      int z = 0;
      foreach (string important in importantDoubles) {
        headers[x + 1 + z] = important;
        doubleValues[z] = GetDoubleDataSet(dataFile, baseDataSetName + important);
        if (doubleValues[z] == null || important == "psexpfac") {
          double defaultDouble = Constants.DEFAULT_VALUE;
          if (important == "psexpfac") {
            defaultDouble = 1;
          }

          doubleValues[z] = new double[size];
          for (int y = 0; y < size; y++) {
            doubleValues[z][y] = defaultDouble;
          }
        }
        int vSize = values[z].Count();
        if (size == -1) {
          size = vSize;
        } else {
          if (vSize != size) {
            throw new Exception(important + " column for Person is the wrong size " + size + " vs " + vSize);
          }
        }
        z++;
      }

      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputPersonPath).ToFile();

      using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
        WriteHeader(headers, writer, Global.Configuration.InputPersonDelimiter);
        int id = 0;

        for (int s = 0; s < size; s++) {
          int householdId = values[0][s];
          int sequence = values[1][s];

          personKeys.Add(new Tuple<int, int>(householdId, sequence), ++id);

          writer.Write(id);
          writer.Write(Global.Configuration.InputPersonDelimiter);


          for (int i = 0; i < nEssentials + nImportants; i++) {
            writer.Write(values[i][s]);
            writer.Write(Global.Configuration.InputPersonDelimiter);
          }
          for (int i = 0; i < nImportantDoubles; i++) {
            writer.Write(doubleValues[i][s]);
            if (i == nImportantDoubles - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputPersonDelimiter);
            }
          }
        }
      }
      return personKeys;
    }


    private static Dictionary<Tuple<int, int>, int> ConvertPersonFile() {
      Dictionary<Tuple<int, int>, int> personKeys = new Dictionary<Tuple<int, int>, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawPersonPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputPersonPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WritePersonHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawPersonDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int sequence = int.Parse(row[1]);

              personKeys.Add(new Tuple<int, int>(householdId, sequence), ++id);

              writer.Write(id);
              writer.Write(Global.Configuration.InputPersonDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputPersonDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      return personKeys;
    }

    private static void WritePersonHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the person file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawPersonDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputPersonDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputPersonDelimiter);
        }
      }
    }

    private static Dictionary<Tuple<int, int>, int> ConvertHouseholdDayFile() {
      if (string.IsNullOrEmpty(Global.Configuration.RawHouseholdDayPath)) {
        return new Dictionary<Tuple<int, int>, int>();
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputHouseholdDayPath)) {
        throw new UndefinedInputPathException("The input path for household day is missing or empty from the configuration file.");
      }

      Dictionary<Tuple<int, int>, int> householdDayKeys = new Dictionary<Tuple<int, int>, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawHouseholdDayPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputHouseholdDayPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteHouseholdDayHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawHouseholdDayDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int day = int.Parse(row[1]);

              householdDayKeys.Add(new Tuple<int, int>(householdId, day), ++id);

              writer.Write(id);
              writer.Write(Global.Configuration.InputHouseholdDayDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputHouseholdDayDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      return householdDayKeys;
    }

    private static void WriteHouseholdDayHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the household day file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawHouseholdDayDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputHouseholdDayDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputHouseholdDayDelimiter);
        }
      }
    }

    private static Dictionary<Tuple<int, int, int>, int> ConvertPersonDayFile(IDictionary<Tuple<int, int>, int> personKeys, Dictionary<Tuple<int, int>, int> householdDayKeys) {
      if (string.IsNullOrEmpty(Global.Configuration.RawPersonDayPath)) {
        return new Dictionary<Tuple<int, int, int>, int>();
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputPersonDayPath)) {
        throw new UndefinedInputPathException("The input path for person day is missing or empty from the configuration file.");
      }

      Dictionary<Tuple<int, int, int>, int> personDayKeys = new Dictionary<Tuple<int, int, int>, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawPersonDayPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputPersonDayPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WritePersonDayHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawPersonDayDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int sequence = int.Parse(row[1]);
              int day = int.Parse(row[2]);

              personDayKeys.Add(new Tuple<int, int, int>(householdId, sequence, day), ++id);

              int personId = personKeys[new Tuple<int, int>(householdId, sequence)];
              int householdDayId = householdDayKeys[new Tuple<int, int>(householdId, day)];

              writer.Write(id);
              writer.Write(Global.Configuration.InputPersonDayDelimiter);

              writer.Write(personId);
              writer.Write(Global.Configuration.InputPersonDayDelimiter);

              writer.Write(householdDayId);
              writer.Write(Global.Configuration.InputPersonDayDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputPersonDayDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      return personDayKeys;
    }

    private static void WritePersonDayHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the person day file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawPersonDayDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputPersonDayDelimiter);

      writer.Write("person_id");
      writer.Write(Global.Configuration.InputPersonDayDelimiter);

      writer.Write("household_day_id");
      writer.Write(Global.Configuration.InputPersonDayDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputPersonDayDelimiter);
        }
      }
    }

    private static Dictionary<Tuple<int, int, int, int>, int> ConvertTourFile(IDictionary<Tuple<int, int>, int> personKeys, Dictionary<Tuple<int, int, int>, int> personDayKeys) {
      if (string.IsNullOrEmpty(Global.Configuration.RawTourPath)) {
        return new Dictionary<Tuple<int, int, int, int>, int>();
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputTourPath)) {
        throw new UndefinedInputPathException("The input path for tour is missing or empty from the configuration file.");
      }

      Dictionary<Tuple<int, int, int, int>, int> tourKeys = new Dictionary<Tuple<int, int, int, int>, int>();
      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawTourPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputTourPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteTourHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int sequence = int.Parse(row[1]);
              int day = int.Parse(row[2]);
              int tour = int.Parse(row[3]);

              tourKeys.Add(new Tuple<int, int, int, int>(householdId, sequence, day, tour), ++id);

              int personId = personKeys[new Tuple<int, int>(householdId, sequence)];
              int personDayId = personDayKeys[new Tuple<int, int, int>(householdId, sequence, day)];

              writer.Write(id);
              writer.Write(Global.Configuration.InputTourDelimiter);

              writer.Write(personId);
              writer.Write(Global.Configuration.InputTourDelimiter);

              writer.Write(personDayId);
              writer.Write(Global.Configuration.InputTourDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputTourDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      if (!Global.Configuration.InputFileTimesAlreadyInMinutesPastMidnight) {
        ConvertTime(Global.GetInputPath(Global.Configuration.InputTourPath), Global.Configuration.InputTourDelimiter, new[] { "tlvorig", "tardest", "tlvdest", "tarorig" });
      }
      return tourKeys;
    }

    private static void WriteTourHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the tour file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputTourDelimiter);

      writer.Write("person_id");
      writer.Write(Global.Configuration.InputTourDelimiter);

      writer.Write("person_day_id");
      writer.Write(Global.Configuration.InputTourDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputTourDelimiter);
        }
      }
    }

    private static void ConvertTripFile(IDictionary<Tuple<int, int, int, int>, int> tourKeys) {
      if (string.IsNullOrEmpty(Global.Configuration.RawTripPath)) {
        return;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputTripPath)) {
        throw new UndefinedInputPathException("The input path for trip is missing or empty from the configuration file.");
      }

      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawTripPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputTripPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteTripHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawTripDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int sequence = int.Parse(row[1]);
              int day = int.Parse(row[2]);
              int tour = int.Parse(row[3]);

              int tourId = tourKeys[new Tuple<int, int, int, int>(householdId, sequence, day, tour)];

              writer.Write(++id);
              writer.Write(Global.Configuration.InputTripDelimiter);

              writer.Write(tourId);
              writer.Write(Global.Configuration.InputTripDelimiter);

              if (Global.Configuration.DataType != "Actum") {
                writer.Write(0);
                writer.Write(Global.Configuration.InputTripDelimiter);
              }

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputTripDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      if (!Global.Configuration.InputFileTimesAlreadyInMinutesPastMidnight) {
        ConvertTime(Global.GetInputPath(Global.Configuration.InputTripPath), Global.Configuration.InputTripDelimiter, new[] { "deptm", "arrtm", "endacttm" });
      }
    }

    private static void WriteTripHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the trip file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawTripDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputTripDelimiter);

      writer.Write("tour_id");
      writer.Write(Global.Configuration.InputTripDelimiter);

      if (Global.Configuration.DataType != "Actum") {
        writer.Write("vot");
        writer.Write(Global.Configuration.InputTripDelimiter);
      }
      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputTripDelimiter);
        }
      }
    }

    private static void ConvertJointTourFile(IDictionary<Tuple<int, int>, int> householdDayKeys) {
      if (string.IsNullOrEmpty(Global.Configuration.RawJointTourPath)) {
        return;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputJointTourPath)) {
        throw new UndefinedInputPathException("The input path for joint tour is missing or empty from the configuration file.");
      }

      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawJointTourPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputJointTourPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteJointTourHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawJointTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int day = int.Parse(row[1]);

              int householdDayId = householdDayKeys[new Tuple<int, int>(householdId, day)];

              writer.Write(++id);
              writer.Write(Global.Configuration.InputJointTourDelimiter);

              writer.Write(householdDayId);
              writer.Write(Global.Configuration.InputJointTourDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputJointTourDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }
    }

    private static void WriteJointTourHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the joint tour file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawJointTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputJointTourDelimiter);

      writer.Write("household_day_id");
      writer.Write(Global.Configuration.InputJointTourDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputJointTourDelimiter);
        }
      }
    }

    private static void ConvertFullHalfTourFile(IDictionary<Tuple<int, int>, int> householdDayKeys) {
      if (string.IsNullOrEmpty(Global.Configuration.RawFullHalfTourPath)) {
        return;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputFullHalfTourPath)) {
        throw new UndefinedInputPathException("The input path for full half-tour is missing or empty from the configuration file.");
      }

      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawFullHalfTourPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputFullHalfTourPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WriteFullHalfTourHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawFullHalfTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int day = int.Parse(row[1]);

              int householdDayId = householdDayKeys[new Tuple<int, int>(householdId, day)];

              writer.Write(++id);
              writer.Write(Global.Configuration.InputFullHalfTourDelimiter);

              writer.Write(householdDayId);
              writer.Write(Global.Configuration.InputFullHalfTourDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputFullHalfTourDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }
    }

    private static void WriteFullHalfTourHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the full half-tour file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawFullHalfTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputFullHalfTourDelimiter);

      writer.Write("household_day_id");
      writer.Write(Global.Configuration.InputFullHalfTourDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputFullHalfTourDelimiter);
        }
      }
    }

    private static void ConvertPartialHalfTourFile(IDictionary<Tuple<int, int>, int> householdDayKeys) {
      if (string.IsNullOrEmpty(Global.Configuration.RawPartialHalfTourPath)) {
        return;
      }

      if (string.IsNullOrEmpty(Global.Configuration.InputPartialHalfTourPath)) {
        throw new UndefinedInputPathException("The input path for partial half-tour is missing or empty from the configuration file.");
      }

      FileInfo rawFile = Global.GetInputPath(Global.Configuration.RawPartialHalfTourPath).ToFile();
      FileInfo inputFile = Global.GetInputPath(Global.Configuration.InputPartialHalfTourPath).ToFile();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteFileInfo(rawFile, true, inputFile.Name);
      }

      using (CountingReader reader = new CountingReader(rawFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(inputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          WritePartialHalfTourHeader(reader, writer);

          int id = 0;
          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { Global.Configuration.RawPartialHalfTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);
              int householdId = int.Parse(row[0]);
              int day = int.Parse(row[1]);

              int householdDayId = householdDayKeys[new Tuple<int, int>(householdId, day)];

              writer.Write(++id);
              writer.Write(Global.Configuration.InputPartialHalfTourDelimiter);

              writer.Write(householdDayId);
              writer.Write(Global.Configuration.InputPartialHalfTourDelimiter);

              for (int i = 0; i < row.Length; i++) {
                writer.Write(row[i]);

                if (i == row.Length - 1) {
                  writer.WriteLine();
                } else {
                  writer.Write(Global.Configuration.InputPartialHalfTourDelimiter);
                }
              }
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + rawFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }
    }

    private static void WritePartialHalfTourHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the partial half-tour file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawPartialHalfTourDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      writer.Write("id");
      writer.Write(Global.Configuration.InputPartialHalfTourDelimiter);

      writer.Write("household_day_id");
      writer.Write(Global.Configuration.InputPartialHalfTourDelimiter);

      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(Global.Configuration.InputPartialHalfTourDelimiter);
        }
      }
    }

    private static void ConvertTime(string path, char delimiter, string[] fields) {
      FileInfo inputFile = path.ToFile();
      FileInfo tempFile = new FileInfo(path + ".tmp");

      using (CountingReader reader = new CountingReader(inputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter writer = new StreamWriter(tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          Dictionary<string, int> header = ParseHeader(reader, delimiter);

          // writes the header fields
          WriteRow(header.Keys.ToArray(), writer, delimiter);

          string line = null;
          try {
            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

              AdjustTimes(row, header, fields);
              WriteRow(row, writer, delimiter);
            }
          } catch (FormatException e) {
            throw new Exception("Format problem in file '" + inputFile + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
          }

        }
      }

      // deletes the input file
      inputFile.Delete();

      // renames the temp file to the input file
      tempFile.MoveTo(inputFile.FullName);
    }

    private static Dictionary<string, int> ParseHeader(TextReader reader, char delimiter) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the file. Please ensure that the raw file contains the appropriate header.");
      }

      Dictionary<string, int> header = new Dictionary<string, int>();
      string[] fields = line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        header.Add(fields[i], i);
      }

      return header;
    }

    private static void WriteRow(string[] row, TextWriter writer, char delimiter) {
      string line = string.Join(delimiter.ToString(CultureInfo.InvariantCulture), row);

      writer.WriteLine(line);
    }

    private static void AdjustTimes(IList<string> row, IDictionary<string, int> header, IEnumerable<string> fields) {
      foreach (string field in fields) {
        if (!header.ContainsKey(field)) {
          continue;
        }

        int index = header[field];
        int time = Convert.ToInt32(row[index]);

        row[index] = ToMinutesAfterMidnight(time).ToString(CultureInfo.InvariantCulture);
      }
    }

    private static int ToMinutesAfterMidnight(int clockTime24Hour) {
      int hours = Math.DivRem(clockTime24Hour, 100, out int minutes);

      return 60 * (hours) + minutes; // subtract 3 hours
    }

    private static void WriteHeader(this TextWriter writer, char delimiter, params string[] fields) {
      for (int i = 0; i < fields.Length; i++) {
        writer.Write(fields[i]);

        if (i == fields.Length - 1) {
          writer.WriteLine();
        } else {
          writer.Write(delimiter);
        }
      }
    }

    private static void WriteActumParkAndRideNodeHeader(TextReader reader, TextWriter writer) {
      string line = reader.ReadLine();

      if (line == null) {
        throw new MissingHeaderException("The header is missing from the park and ride node file. Please ensure that the raw file contains the appropriate header.");
      }

      string[] fields = line.Split(new[] { Global.Configuration.RawParkAndRideNodeDelimiter }, StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < fields.Length; i++) {
        switch (i) {
          case 0:
            writer.Write("id");
            writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

            writer.Write("ParkAndRideNodeId");
            writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

            writer.Write("xcoord");
            writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

            writer.Write("ycoord");
            writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);

            break;
          default:
            writer.Write(fields[i]);

            if (i == fields.Length - 1) {
              writer.WriteLine();
            } else {
              writer.Write(Global.Configuration.InputParkAndRideNodeDelimiter);
            }

            break;
        }
      }
    }



  }
}
