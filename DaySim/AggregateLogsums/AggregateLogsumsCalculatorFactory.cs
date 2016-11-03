using DaySim.Framework.Core;
using System;
using System.Collections.Generic;

namespace DaySim.AggregateLogsums {
    class AggregateLogsumsCalculatorFactory {
        private readonly string _key = Global.AggregateLogsumCalculator;

        private readonly Dictionary<String, IAggregateLogsumsCalculatorCreator> _creators = new Dictionary<string, IAggregateLogsumsCalculatorCreator>();

        public IAggregateLogsumsCalculatorCreator AggregateLogsumCalculatorCreator { get; private set; }

        public void Register(String key, IAggregateLogsumsCalculatorCreator value) {
            _creators.Add(key, value);
        }

        public void Initialize() {
            AggregateLogsumCalculatorCreator = _creators[_key];
        }
    }
}
