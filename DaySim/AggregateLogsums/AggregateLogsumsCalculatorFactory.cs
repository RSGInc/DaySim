using System.Collections.Generic;
using DaySim.Framework.Core;

namespace DaySim.AggregateLogsums {
  internal class AggregateLogsumsCalculatorFactory {
    private readonly string _key = Global.AggregateLogsumCalculator;

    private readonly Dictionary<string, IAggregateLogsumsCalculatorCreator> _creators = new Dictionary<string, IAggregateLogsumsCalculatorCreator>();

    public IAggregateLogsumsCalculatorCreator AggregateLogsumCalculatorCreator { get; private set; }

    public void Register(string key, IAggregateLogsumsCalculatorCreator value) {
      _creators.Add(key, value);
    }

    public void Initialize() {
      AggregateLogsumCalculatorCreator = _creators[_key];
    }
  }
}
