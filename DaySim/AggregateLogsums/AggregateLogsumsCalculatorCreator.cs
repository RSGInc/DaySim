namespace DaySim.AggregateLogsums {
  internal class AggregateLogsumsCalculatorCreator : IAggregateLogsumsCalculatorCreator {
    public IAggregateLogsumsCalculator Create() {
      return new AggregateLogsumsCalculator();
    }
  }
}
