// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DaySim.Framework.Core {
    public static class ParallelUtility {
        /// <summary>
        /// This will be a value from 0 to (configuration.NProcessors-1) and is a different value for each thread.
        /// </summary>
        public static ThreadLocal<int> threadLocalAssignedIndex = null;

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
        /// <summary>
        /// For the current thread, create and store an index in thread local memory. This can/will be used so that code can have store multithreaded generated data in arrays where each thread has a dedicated slot for storage. Any array that is created using [Configuration.NProcessors] should use this index to store data.
        /// </summary>
        public static void AssignThreadIndex() {
            Debug.Assert(threadLocalAssignedIndex == null, "AssignThreadIndex called when threadLocalAssignedIndex is already not null!");
            int threadsSoFarIndex = -1;

            //this function will be called automagically each time a new thread tries to read the value of threadLocalAssignedIndex for the first time.
            //threadSoFarIndex acts like a static variable in that it remains alive due to closure
            threadLocalAssignedIndex = new ThreadLocal<int>(() => {
                int threadLocalAssignedIndexSpecificValue = Interlocked.Increment(ref threadsSoFarIndex);

                Global.PrintFile.WriteLine("Thread.CurrentThread.ManagedThreadId: " + Thread.CurrentThread.ManagedThreadId + " threadLocalAssignedIndexSpecificValue: " + threadLocalAssignedIndexSpecificValue);
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