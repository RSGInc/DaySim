// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.IO;
using DaySim.Framework.Core;

namespace DaySim.Framework.Roster {
  public sealed class BinarySkimFileReader : ISkimFileReader {
    private readonly string _path;
    private readonly Dictionary<int, int> _mapping;

    public BinarySkimFileReader(string path, Dictionary<int, int> mapping) {
      _path = path;
      _mapping = mapping;
    }

    public SkimMatrix Read(string filename, int field, float scale) {
      FileInfo file = new FileInfo(Path.Combine(_path, filename));

      Console.WriteLine("Loading skim file: {0}, field: {1}.", file.Name, field);
      Global.PrintFile.WriteFileInfo(file, true);

      if (!file.Exists) {
        throw new FileNotFoundException(string.Format("The skim file {0} could not be found.", file.FullName));
      }

      int count = _mapping.Count;
      ushort[][] matrix = new ushort[count][];

      for (int i = 0; i < count; i++) {
        matrix[i] = new ushort[count];
      }

      using (BinaryReader reader = new BinaryReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        long position = 0L;
        long length = reader.BaseStream.Length;

        for (int origin = 0; origin < count; origin++) {
          for (int destination = 0; destination < count; destination++) {
            if (position >= length) {
              goto end;
            }

            ushort value = reader.ReadUInt16();

            // binary matrices are already mapped to consecutive zone indices, matching the zone index file
            matrix[origin][destination] = value;

            position += sizeof(ushort);
          }
        }
      }

end:

      SkimMatrix skimMatrix = new SkimMatrix(matrix);

      return skimMatrix;
    }
  }
}