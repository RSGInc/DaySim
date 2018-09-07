// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.IO;
using System.Linq;

namespace DaySim.Framework.Sampling {
  public static class SegmentZoneFormatter {
    public static SegmentZone[] Deserialize(Stream serializationStream) {
      using (BinaryReader reader = new BinaryReader(serializationStream)) {
        int totalSegmentZones = reader.ReadInt32();
        int arraySize = reader.ReadInt32();
        SegmentZone[] segmentZones = new SegmentZone[arraySize];

        for (int i = 0; i < totalSegmentZones; i++) {
          int index = reader.ReadInt32();

          segmentZones[index] = SegmentZone.Load(reader);
        }

        return segmentZones;
      }
    }

    public static void Serialize(Stream serializationStream, SegmentZone[] segmentZones) {
      using (BinaryWriter writer = new BinaryWriter(serializationStream)) {
        int totalSegmentZones = segmentZones.Count(x => x != null);
        int arraySize = segmentZones.Length;

        writer.Write(totalSegmentZones);
        writer.Write(arraySize);

        for (int index = 0; index < arraySize; index++) {
          if (segmentZones[index] == null) {
            continue;
          }

          writer.Write(index);

          SegmentZone.Save(segmentZones[index], writer);
        }
      }
    }
  }
}