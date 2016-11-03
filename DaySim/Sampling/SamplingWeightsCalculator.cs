// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.Framework.Sampling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DaySim.Sampling {
    public sealed class SamplingWeightsCalculator {
        private readonly List<IParcelWrapper> _eligibleParcels = new List<IParcelWrapper>();
        private readonly Dictionary<int, int> _parcelCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, IZone> _eligibleZones;

        private readonly int _zoneCount;
        private readonly int _segmentCount;
        private readonly string _variableName;
        private readonly int _mode;
        private readonly int _pathType;
        private readonly double _vot;
        private readonly int _minute;

        private SamplingWeightsCalculator(string variableName, int mode, int pathType, double vot, int minute) {
            var zoneReader =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IZone>>()
                    .Reader;

            var parcelReader =
                Global
                    .ContainerDaySim
                    .GetInstance<IPersistenceFactory<IParcel>>()
                    .Reader;

            var parcelCreator =
                Global
                    .ContainerDaySim
                    .GetInstance<IWrapperFactory<IParcelCreator>>()
                    .Creator;

            _eligibleZones =
                zoneReader
                    .Where(z => z.DestinationEligible)
                    .ToDictionary(z => z.Id, z => z);

            foreach (var parcel in parcelReader) {
                IZone zone;

                if (_eligibleZones.TryGetValue(parcel.ZoneId, out zone)) {
                    _eligibleParcels.Add(parcelCreator.CreateWrapper(parcel));
                }

                if (_parcelCounts.ContainsKey(parcel.ZoneId)) {
                    _parcelCounts[parcel.ZoneId]++;
                } else {
                    _parcelCounts.Add(parcel.ZoneId, 1);
                }
            }

            _zoneCount = zoneReader.Count;
            _segmentCount = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.SizeFactors.GetLength(0);
            _variableName = variableName;
            _mode = mode;
            _pathType = pathType;
            _vot = vot;
            _minute = minute;
        }

        public static void Calculate(string variableName, int mode, int pathType, double vot, int minute) {
            var samplingWeightsCalculator = new SamplingWeightsCalculator(variableName, mode, pathType, vot, minute);
            var segmentCount = samplingWeightsCalculator._segmentCount;

            Global.SegmentZones = new SegmentZone[segmentCount][];

            Parallel.For(0, segmentCount, new ParallelOptions { MaxDegreeOfParallelism = ParallelUtility.NThreads }, segment => CalculateSegment(samplingWeightsCalculator, segment));
        }

        private static void CalculateSegment(SamplingWeightsCalculator samplingWeightsCalculator, int segment) {
            var file = new FileInfo(string.Format(Global.SamplingWeightsPath, segment));

            if (Global.Configuration.ShouldLoadSamplingWeightsFromFile && file.Exists) {
                Global.SegmentZones[segment] = LoadSamplingWeightsFromFile(file);

                return;
            }

            Global.SegmentZones[segment] = samplingWeightsCalculator.ComputeSegment(segment);

            if (Global.Configuration.ShouldLoadSamplingWeightsFromFile && !file.Exists) {
                SaveSamplingWeightsToFile(file, segment);
            }
        }

        private static SegmentZone[] LoadSamplingWeightsFromFile(FileInfo file) {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return SegmentZoneFormatter.Deserialize(stream);
            }
        }

        private static void SaveSamplingWeightsToFile(FileInfo file, int segment) {
            using (var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read)) {
                SegmentZoneFormatter.Serialize(stream, Global.SegmentZones[segment]);
            }
        }

        private SegmentZone[] ComputeSegment(int segment) {
            var sizeFactors = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.SizeFactors[segment];
            var weightFactors = Global.ContainerDaySim.GetInstance<SamplingWeightsSettingsFactory>().SamplingWeightsSettings.WeightFactors[segment];
            var segmentZones = new SegmentZone[_zoneCount];

            foreach (var parcel in _eligibleParcels) {
                var segmentZone = segmentZones[parcel.ZoneId];
                var zone = _eligibleZones[parcel.ZoneId];
                if (segmentZone == null) {
                    segmentZone = new SegmentZone(_parcelCounts[parcel.ZoneId], _zoneCount) { Id = parcel.ZoneId, Key = zone.Key };

                    segmentZones[segmentZone.Id] = segmentZone;
                }

                var factor = Math.Exp(sizeFactors[0]) * parcel.EmploymentEducation +
                                 Math.Exp(sizeFactors[1]) * parcel.EmploymentFood +
                                 Math.Exp(sizeFactors[2]) * parcel.EmploymentGovernment +
                                 Math.Exp(sizeFactors[3]) * parcel.EmploymentIndustrial +
                                 Math.Exp(sizeFactors[4]) * parcel.EmploymentMedical +
                                 Math.Exp(sizeFactors[5]) * parcel.EmploymentOffice +
                                 Math.Exp(sizeFactors[6]) * parcel.EmploymentAgricultureConstruction +
                                 Math.Exp(sizeFactors[7]) * parcel.EmploymentRetail +
                                 Math.Exp(sizeFactors[8]) * parcel.EmploymentService +
                                 Math.Exp(sizeFactors[9]) * parcel.EmploymentTotal +
                                 Math.Exp(sizeFactors[10]) * parcel.Households +
                                 Math.Exp(sizeFactors[11]) * parcel.StudentsK8 +
                                 Math.Exp(sizeFactors[12]) * parcel.StudentsUniversity +
                                 Math.Exp(sizeFactors[13]) * parcel.GetLandUseCode19() +
                                 Math.Exp(sizeFactors[14]) * parcel.OpenSpaceType1Buffer1 +
                                 Math.Exp(sizeFactors[15]) * parcel.OpenSpaceType2Buffer1 +
                                 Math.Exp(sizeFactors[16]) * parcel.StudentsHighSchool;

                var size = 100 * factor;

                if (size >= Global.Configuration.MinParcelSize) {
                    segmentZone.TotalSize += size;
                }

                segmentZone.SetSize(parcel.Sequence, parcel.Id, size);
            }

            foreach (var origin in segmentZones.Where(o => o != null)) {
                origin.RankSizes();

                foreach (var destination in segmentZones.Where(d => d != null)) {
                    var skimValue = ImpedanceRoster.GetValue(_variableName, _mode, _pathType, _vot, _minute, origin.Id, destination.Id);

                    //jb 20130707 mimic mb fix for 0 intrazonals
                    if (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Zone && origin.Id == destination.Id && skimValue.Variable < Constants.EPSILON) {
                        if (_variableName == "distance") {
                            skimValue.Variable = 0.25 * Global.Settings.DistanceUnitsPerMile;
                        } else if (_variableName == "ivtime" || _variableName == "time" || _variableName == "ivtfree") {
                            skimValue.Variable =
                                 (_mode == Global.Settings.Modes.Walk) ? 5 :
                                 (_mode == Global.Settings.Modes.Bike) ? 2 :
                                 (_mode > Global.Settings.Modes.Bike && _mode < Global.Settings.Modes.Transit) ? 1 : 0;
                        }
                    }
                    var sovInVehicleTime = skimValue.Variable;

                    // give 0 weight if not connected in SOV matrix
                    var weight = sovInVehicleTime < Constants.EPSILON ? 0.0 : Math.Exp(-2 * sovInVehicleTime * 100 / weightFactors[0]) * destination.TotalSize;

                    origin.TotalWeight += weight;
                    origin.SetWeight(destination.Id, weight);
                }

                origin.RankWeights();
            }

            return segmentZones;
        }
    }
}