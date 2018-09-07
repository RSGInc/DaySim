// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;

namespace DaySim.Framework.Sampling {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  public struct WeightSegmentItem {
    public WeightSegmentItem(int id, double value) : this() {
      Id = id;
      Value = value;
    }

    public int Id { get; private set; }

    public double Value { get; private set; }
  }
}