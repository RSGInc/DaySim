using System;

namespace DaySim.Framework.Core {
  public interface IMinuteSpan : IEquatable<IMinuteSpan> {
    int Index { get; }

    int Start { get; set; }

    int End { get; set; }

    bool Keep { get; set; }

    int Middle { get; }

    int Duration { get; }
  }
}
