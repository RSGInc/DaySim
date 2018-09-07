// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.



namespace DaySim.Framework.Core {
  public sealed class MinuteSpan : IMinuteSpan {
    public MinuteSpan(int start, int end) {
      Index = Constants.DEFAULT_VALUE;
      Start = start;
      End = end;
    }

    public MinuteSpan(int index, int start, int end) : this(start, end) {
      Index = index;
    }

    public int Index { get; private set; }

    public int Start { get; set; }

    public int End { get; set; }

    public bool Keep { get; set; }

    public int Middle {
      get {
        int start = Start;
        int end = End;

        if (start > end) {
          int temp = start;

          start = end;
          end = temp;
        }

        return start + ((end - start) / 2);
      }
    }

    public int Duration => End - Start + 1;

    public override string ToString() {
      return string.Format("Start: {0}, End: {1}", Start, End);
    }

    public bool Equals(IMinuteSpan other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      return other.Start == Start && other.End == End;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }

      if (ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj.GetType() != typeof(MinuteSpan)) {
        return false;
      }

      return Equals((MinuteSpan)obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Start * 397) ^ End;
      }
    }
  }
}