using Daysim.Framework.Core;

namespace Daysim.AggregateLogsums
{
    public interface IAggregateLogsumsCalculator
    {
        void Calculate(IRandomUtility utility);
    }
}
