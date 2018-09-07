// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


namespace DaySim.Framework.Core {
  public sealed class ChosenAlternative : IObservationItem {
    public ChosenAlternative(int position) {
      PositionIndex = position;
    }

    public int PositionIndex { get; private set; }

    public int Position => PositionIndex + 1;

    public int Key { get; private set; }

    public double Data { get; private set; }

    public void Update(int key, int alternativeId) {
      Key = key;
      Data = alternativeId;
    }
  }
}