using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DaySim.Framework.Core {

    //code from Mike Larah: https://blogs.endjin.com/2015/10/using-lazy-and-concurrentdictionary-to-ensure-a-thread-safe-run-once-lazy-loaded-collection/
    //Could also achieve this without a special class. See http://reedcopsey.com/2011/01/16/concurrentdictionarytkeytvalue-used-with-lazyt/
    public class LazyConcurrentDictionary<TKey, TValue> {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> concurrentDictionary;

        public LazyConcurrentDictionary() {
            this.concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }


        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
            var lazyResult = this.concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));

            return lazyResult.Value;
        }
    }
}
