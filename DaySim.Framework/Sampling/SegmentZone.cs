// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DaySim.Framework.Sampling {
	[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
	public sealed class SegmentZone {
		private readonly ParcelSize[] _parcelSizes;
		private int[] _rankedSizeIndices;

		private readonly double[] _zoneWeights;
		private int[] _rankedWeightIndices;

		public SegmentZone(int parcelCount, int zoneCount) {
			_parcelSizes = new ParcelSize[parcelCount];
			_zoneWeights = new double[zoneCount];
		}

		public int Id { get; set; }

		public int Key { get; set; }

		public double TotalSize { get; set; }

		public double TotalWeight { get; set; }

		public IEnumerable<SizeSegmentItem> RankedSizes {
			get { return _rankedSizeIndices.Select(index => new SizeSegmentItem(index, _parcelSizes[index].Id, _parcelSizes[index].Value)); }
		}

		public IEnumerable<WeightSegmentItem> RankedWeights {
			get { return _rankedWeightIndices.Select(index => new WeightSegmentItem(index, _zoneWeights[index])); }
		}

		public void SetSize(int parcelSequence, int parcelId, double size) {
			_parcelSizes[parcelSequence] = new ParcelSize(parcelId, size);
		}

		public void SetWeight(int zoneId, double weight) {
			_zoneWeights[zoneId] = weight;
		}

		public double GetSize(int parcelSequence) {
			return _parcelSizes[parcelSequence].Value;
		}

		public double GetWeight(int zoneId) {
			return _zoneWeights[zoneId];
		}

		public void RankSizes() {
			_rankedSizeIndices = _parcelSizes.Select((parcelSize, i) => new {Size = parcelSize.Value, Index = i}).OrderByDescending(x => x.Size).Select(x => x.Index).ToArray();
		}

		public void RankWeights() {
			_rankedWeightIndices = _zoneWeights.Select((weight, i) => new {Weight = weight, Index = i}).OrderByDescending(x => x.Weight).Select(x => x.Index).ToArray();
		}

		public static void Save(SegmentZone segmentZone, BinaryWriter writer) {
			writer.Write(segmentZone._parcelSizes.Length);
			writer.Write(segmentZone._zoneWeights.Length);

			writer.Write(segmentZone.Id);
			writer.Write(segmentZone.TotalSize);
			writer.Write(segmentZone.TotalWeight);

			foreach (var item in segmentZone._parcelSizes) {
				writer.Write(item.Id);
				writer.Write(item.Value);
			}

			foreach (var index in segmentZone._rankedSizeIndices) {
				writer.Write(index);
			}

			foreach (var weight in segmentZone._zoneWeights) {
				writer.Write(weight);
			}

			foreach (var index in segmentZone._rankedWeightIndices) {
				writer.Write(index);
			}
		}

		public static SegmentZone Load(BinaryReader reader) {
			var parcelCount = reader.ReadInt32();
			var zoneCount = reader.ReadInt32();

			var segmentZone = new SegmentZone(parcelCount, zoneCount) {
				Id = reader.ReadInt32(),
				TotalSize = reader.ReadDouble(),
				TotalWeight = reader.ReadDouble()
			};

			for (var i = 0; i < parcelCount; i++) {
				segmentZone._parcelSizes[i] = new ParcelSize(reader.ReadInt32(), reader.ReadDouble());
			}

			segmentZone._rankedSizeIndices = new int[parcelCount];

			for (var i = 0; i < parcelCount; i++) {
				segmentZone._rankedSizeIndices[i] = reader.ReadInt32();
			}

			for (var i = 0; i < zoneCount; i++) {
				segmentZone._zoneWeights[i] = reader.ReadDouble();
			}

			segmentZone._rankedWeightIndices = new int[zoneCount];

			for (var i = 0; i < zoneCount; i++) {
				segmentZone._rankedWeightIndices[i] = reader.ReadInt32();
			}

			return segmentZone;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
		private struct ParcelSize {
			public ParcelSize(int id, double value) : this() {
				Id = id;
				Value = value;
			}

			public int Id { get; private set; }

			public double Value { get; private set; }
		}
	}
}