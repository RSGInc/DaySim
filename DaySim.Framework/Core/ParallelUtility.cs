// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DaySim.Framework.Core {
	public static class ParallelUtility {
        private static ThreadLocal<int> threadLocalBatchIndex = null;

        private static int threadsSoFarIndex = -1000; 
        public static int NThreads { get; private set; }

		public static void Init(Configuration configuration) {
            NThreads =
				configuration.NProcessors == 0
					? 1
					: configuration.NProcessors;

			if (configuration.IsInEstimationMode) {
				NThreads = 1;
			}
		}

        public static void InitThreadLocalBatchIndex() {
            threadsSoFarIndex = -1;
            threadLocalBatchIndex = new ThreadLocal<int>(() => {
                int threadLocalBatchIndexThreadSpecificValue = Interlocked.Increment(ref threadsSoFarIndex);

                Console.WriteLine("Thread.CurrentThread.ManagedThreadId: " + Thread.CurrentThread.ManagedThreadId + " assigned threadLocalBatchIndexThreadSpecificValue: " + threadLocalBatchIndexThreadSpecificValue);
            return threadLocalBatchIndexThreadSpecificValue;
        });
    }

    private static long GetBatchFromThreadIdCounter = 0;
        public static int GetBatchFromThreadId() {
            if (GetBatchFromThreadIdCounter % 10000 == 0 || GetBatchFromThreadIdCounter < 20) {
               
                Console.WriteLine("GetBatchFromThreadIdCounter=" + GetBatchFromThreadIdCounter + " from thread: " + Thread.CurrentThread.ManagedThreadId + " threadLocalBatchIndex.IsValueCreated: " + threadLocalBatchIndex.IsValueCreated + " returning batch index: " + threadLocalBatchIndex.Value);
            }
            GetBatchFromThreadIdCounter++;
            int batchIndex = threadLocalBatchIndex.Value;
            return batchIndex;
        }

        public static void While(ParallelOptions parallelOptions, Func<bool> condition, Action<ParallelLoopState> body) {
			Parallel.ForEach(new InfinitePartitioner(), parallelOptions,
				(ignored, state) => {
					if (condition()) {
						body(state);
					}
					else {
						state.Stop();
					}
				});
		}
	}
}