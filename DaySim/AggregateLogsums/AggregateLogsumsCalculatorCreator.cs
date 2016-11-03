namespace DaySim.AggregateLogsums {
    class AggregateLogsumsCalculatorCreator : IAggregateLogsumsCalculatorCreator {
        public IAggregateLogsumsCalculator Create() {
            return new AggregateLogsumsCalculator();
        }
    }
}
