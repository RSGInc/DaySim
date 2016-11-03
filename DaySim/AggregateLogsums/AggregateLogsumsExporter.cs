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
using SimpleInjector;

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
            var directory = Path.GetDirectoryName(path);
            var filename = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);
            var zoneCount = Global.AggregateLogsums.GetLength(0);

            var zoneReader =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IZone>>()
                    .Reader;

            for (var purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
                if (directory == null) {
                    throw new DirectoryNotFoundException();
                }

                var file = new FileInfo(Path.Combine(directory, string.Format("{0}.{1}{2}", filename, purpose, extension)));

                using (var writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
                    writer.WriteLine("ZONE\tCHILD/SHO\tCHILD/LON\tCHILD/NAV\tNOCAR/SHO\tNOCAR/LON\tNOCAR/NAV\tCCOMP/SHO\tCCOMP/LON\tCCOMP/NAV\tCFULL/SHO\tCFULL/LON\tCFULL/NAV");

                    for (var id = 0; id < zoneCount; id++) {
                        var zone = zoneReader.Seek(id);

                        writer.Write(string.Format("{0,4:0}", zone.Key));
                        writer.Write("\t");

                        var carOwnerships = Global.AggregateLogsums[id][purpose];

                        for (var carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
                            var votALSegments = carOwnerships[carOwnership];

                            for (var votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
                                var transitAccesses = votALSegments[votALSegment];

                                for (var transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
                                    writer.Write(string.Format("{0,9:f5}", transitAccesses[transitAccess]));

                                    if ((carOwnership + 1) * (transitAccess + 1) != Global.Settings.CarOwnerships.TotalCarOwnerships * Global.Settings.TransitAccesses.TotalTransitAccesses) {
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