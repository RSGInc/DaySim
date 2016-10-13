// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaySim.Framework.Core {
    public static class ParallelUtility {
        /// <summary>
        /// This will be a value from 0 to (configuration.NProcessors-1) and is a different value for each thread.
        /// </summary>
        public static ThreadLocal<int> threadLocalAssignedIndex = null;

#if DEBUG
        internal static Dictionary<string, long> lockCounts = new Dictionary<string, long>();
        internal static void countLocks(string lockObjectName) {
            long counter;
            lockCounts[lockObjectName] = lockCounts.TryGetValue(lockObjectName, out counter) ? ++counter : 1;
        }

        public static string getLockCounts() {
            StringBuilder sb = new StringBuilder("Lock counts:\n");
            foreach (KeyValuePair<string, long> kv in lockCounts) {
                sb.AppendLine("\t" + kv.Key + ": " + string.Format("{0:n0}", kv.Value));
            }
            return sb.ToString();
        }
#endif
        public static int NThreads { get; private set; }

        public static void Init(Configuration configuration) {
            NThreads =
                configuration.NProcessors < 1
                    ? 1
                    : configuration.NProcessors;
        }
        /// <summary>
        /// For the current thread, create and store an index in thread local memory. This can/will be used so that code can have store multithreaded generated data in arrays where each thread has a dedicated slot for storage. Any array that is created using [Configuration.NProcessors] should use this index to store data.
        /// </summary>
        public static void AssignThreadIndex(int maximumExpectedThreads) {
            Debug.Assert(threadLocalAssignedIndex == null, "AssignThreadIndex called when threadLocalAssignedIndex is already not null!");
            int threadsSoFarIndex = -1;

            //this function will be called automagically each time a new thread tries to read the value of threadLocalAssignedIndex for the first time.
            //threadSoFarIndex acts like a static variable in that it remains alive due to closure
            threadLocalAssignedIndex = new ThreadLocal<int>(() => {
                int threadLocalAssignedIndexSpecificValue = Interlocked.Increment(ref threadsSoFarIndex);
                Debug.Assert((Global.Configuration.IsInEstimationMode && (threadLocalAssignedIndexSpecificValue == 0) || !Global.Configuration.IsInEstimationMode), "In EstimationMode but more than one thread is being used!");
                Debug.Assert(threadLocalAssignedIndexSpecificValue < maximumExpectedThreads, "More threads allocated than expected! Maximum expected: " + maximumExpectedThreads + " but am now on thread " + threadsSoFarIndex);
                Global.PrintFile.WriteLine("Thread.CurrentThread.ManagedThreadId: " + Thread.CurrentThread.ManagedThreadId + " threadLocalAssignedIndexSpecificValue: " + threadLocalAssignedIndexSpecificValue, writeToConsole: true);
                return threadLocalAssignedIndexSpecificValue;
            });
        }   //end AssignThreadIndex

        /// <summary>
        /// Dispose of thread local memory allocated in AssignThreadIndex. Also useful to set threadLocalAssignedIndex to null to make incorrect accesses fail.
        /// </summary>
        public static void DisposeThreadIndex() {
            Debug.Assert(threadLocalAssignedIndex != null, "DisposeThreadIndex called when threadLocalAssignedIndex is already null!");
            threadLocalAssignedIndex.Dispose();
            threadLocalAssignedIndex = null;
        }   //end DisposeThreadIndex

    }
}