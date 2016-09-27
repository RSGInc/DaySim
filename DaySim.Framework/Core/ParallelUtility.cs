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
        public static ThreadLocal<int> threadLocalBatchIndex = null;

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
            Debug.Assert(threadLocalBatchIndex == null, "InitThreadLocalBatchIndex called when threadLocalBatchIndex is already not null!");
            int threadsSoFarIndex = -1;
            threadLocalBatchIndex = new ThreadLocal<int>(() => {
                int threadLocalBatchIndexThreadSpecificValue = Interlocked.Increment(ref threadsSoFarIndex);

                Global.PrintFile.WriteLine("Thread.CurrentThread.ManagedThreadId: " + Thread.CurrentThread.ManagedThreadId + " assigned threadLocalBatchIndexThreadSpecificValue: " + threadLocalBatchIndexThreadSpecificValue);
            return threadLocalBatchIndexThreadSpecificValue;
        });
        }   //end InitThreadLocalBatchIndex

        public static void DisposeThreadLocalBatchIndex()
        {
            Debug.Assert(threadLocalBatchIndex != null, "DisposeThreadLocalBatchIndex called when threadLocalBatchIndex is already null!");
            threadLocalBatchIndex.Dispose();
            threadLocalBatchIndex = null;
        }   //end DisposeThreadLocalBatchIndex

	}
}