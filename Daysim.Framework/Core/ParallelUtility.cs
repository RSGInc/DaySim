// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Daysim.Framework.Core {
	public static class ParallelUtility {
		private static int[] _threadToThreadIdMap;

		public static int NBatches { get; private set; }

		public static int SmallDegreeOfParallelism { get; private set; }

		public static int LargeDegreeOfParallelism { get; private set; }

		public static void Init(Configuration configuration) {
			var nProcessors =
				configuration.NProcessors == 0
					? 1
					: configuration.NProcessors;

			if (configuration.IsInEstimationMode) {
				NBatches = 1;
			}
			else {
				NBatches = configuration.NBatches == 0
					? nProcessors * 4
					: configuration.NBatches;
			}

			SmallDegreeOfParallelism =
				configuration
					.SmallDegreeOfParallelism == 0
					? nProcessors / 2
					: configuration.SmallDegreeOfParallelism;

			LargeDegreeOfParallelism =
				configuration.LargeDegreeOfParallelism == 0
					? nProcessors
					: configuration.LargeDegreeOfParallelism;

			_threadToThreadIdMap = new int[NBatches];
		}

		public static void Register(int threadId, int batchNumber) {
			try {
				_threadToThreadIdMap[batchNumber] = threadId;
			}
			catch (Exception) {
				throw new Exception("Invalid BatchNumber " + batchNumber + ":" + _threadToThreadIdMap.Length);
			}
		}

		public static int GetBatchFromThreadId() {
			for (var i = 0; i < NBatches; i++) {
				if (_threadToThreadIdMap[i] == Thread.CurrentThread.ManagedThreadId) {
					return i;
				}
			}

			return NBatches;
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