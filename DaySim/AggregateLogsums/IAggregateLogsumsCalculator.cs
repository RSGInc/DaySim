using DaySim.Framework.Core;

namespace DaySim.AggregateLogsums {
  public interface IAggregateLogsumsCalculator {
    void Calculate(IRandomUtility utility);
  }
}
