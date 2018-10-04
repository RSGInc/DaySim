// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;

namespace DaySim.Framework.Roster {
  public sealed class SkimMatrix {
    private readonly ushort[][] _skimMatrix;
    public SkimMatrix(ushort[][] skimMatrix) {
      _skimMatrix = skimMatrix;
    }

    public ushort GetValue(int origin, int destination) {
      ushort uValue = _skimMatrix[origin][destination];
      if ((uValue > 32767) && Global.Configuration.DataType == "Actum") {
        uValue = _skimMatrix[origin][destination] = 32767;
      }
      return uValue;
    }

    public bool IsEmpty() {
      return _skimMatrix == null;
    }
  }
}
