namespace DaySim.AggregateLogsums {
  public class OtherAggregateLogsumsCalculatorCreator : IAggregateLogsumsCalculatorCreator {

    public IAggregateLogsumsCalculator Create() {
      return new AggregateLogsumsCalculator();
    }
  }
}
