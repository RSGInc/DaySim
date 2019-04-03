// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System.IO;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.Factories;

namespace DaySim.AggregateLogsums {
  public static class AggregateLogsumsExporter {
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
      string directory = Path.GetDirectoryName(path);
      string filename = Path.GetFileNameWithoutExtension(path);
      string extension = Path.GetExtension(path);
      int zoneCount = Global.AggregateLogsums.GetLength(0);

      Framework.DomainModels.Persisters.IPersisterReader<IZone> zoneReader =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IZone>>()
                    .Reader;

      for (int purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
        if (directory == null) {
          throw new DirectoryNotFoundException();
        }

        FileInfo file = new FileInfo(Path.Combine(directory, string.Format("{0}.{1}{2}", filename, purpose, extension)));

        using (StreamWriter writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          writer.Write("ZONE\t");
          writer.Write("CHILD/LVOT/CLOSE\tCHILD/LVOT/FAR\tCHILD/LVOT/NAV\tCHILD/MVOT/CLOSE\tCHILD/MVOT/FAR\tCHILD/MVOT/NAV\tCHILD/HVOT/CLOSE\tCHILD/HVOT/FAR\tCHILD/HVOT/NAV\t");
          writer.Write("NOCAR/LVOT/CLOSE\tNOCAR/LVOT/FAR\tNOCAR/LVOT/NAV\tNOCAR/MVOT/CLOSE\tNOCAR/MVOT/FAR\tNOCAR/MVOT/NAV\tNOCAR/HVOT/CLOSE\tNOCAR/HVOT/FAR\tNOCAR/HVOT/NAV\t");
          writer.Write("CCOMP/LVOT/CLOSE\tCCOMP/LVOT/FAR\tCCOMP/LVOT/NAV\tCCOMP/MVOT/CLOSE\tCCOMP/MVOT/FAR\tCCOMP/MVOT/NAV\tCCOMP/HVOT/CLOSE\tCCOMP/HVOT/FAR\tCCOMP/HVOT/NAV\t");
          writer.WriteLine("FULLCO/LVOT/CLOSE\tFULLCO/LVOT/FAR\tFULLCO/LVOT/NAV\tFULLCO/MVOT/CLOSE\tFULLCO/MVOT/FAR\tFULLCO/MVOT/NAV\tFULLCO/HVOT/CLOSE\tFULLCO/HVOT/FAR\tFULLCO/HVOT/NAV");

          for (int id = 0; id < zoneCount; id++) {
            IZone zone = zoneReader.Seek(id);

            writer.Write(string.Format("{0,4:0}", zone.Key));
            writer.Write("\t");

            double[][][] carOwnerships = Global.AggregateLogsums[id][purpose];

            for (int carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
              double[][] votALSegments = carOwnerships[carOwnership];

              for (int votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
                double[] transitAccesses = votALSegments[votALSegment];

                for (int transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
                  writer.Write(string.Format("{0,9:f5}", transitAccesses[transitAccess]));

                  if (carOwnership + 1 != Global.Settings.CarOwnerships.TotalCarOwnerships ||
                      votALSegment + 1 != Global.Settings.VotALSegments.TotalVotALSegments ||
                      transitAccess + 1 != Global.Settings.TransitAccesses.TotalTransitAccesses) {
                    writer.Write("\t");
                  }
                }
              }
            }

            writer.WriteLine();
          }
        }

        Global.PrintFile.WriteFileInfo(file, true);
      }
    }
  }
}
