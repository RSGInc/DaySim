namespace DaySim.AggregateLogsums {
  internal class ActumAggregateLogsumsCalculatorCreator : IAggregateLogsumsCalculatorCreator {

    public IAggregateLogsumsCalculator Create() {
      return new AggregateLogsumsCalculator_Actum();
    }
  }
}
