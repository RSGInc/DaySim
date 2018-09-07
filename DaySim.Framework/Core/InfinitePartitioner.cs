// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DaySim.Framework.Core {
  public sealed class InfinitePartitioner : Partitioner<bool> {
    public override IList<IEnumerator<bool>> GetPartitions(int partitionCount) {
      if (partitionCount < 1) {
        throw new ArgumentOutOfRangeException("partitionCount");
      }
      return (from i in Enumerable.Range(0, partitionCount)
              select InfiniteEnumerator()).ToArray();
    }

    public override bool SupportsDynamicPartitions => true;

    public override IEnumerable<bool> GetDynamicPartitions() {
      return new InfiniteEnumerators();
    }

    private static IEnumerator<bool> InfiniteEnumerator() {
      while (true) {
        yield return true;
      }
    }

    private class InfiniteEnumerators : IEnumerable<bool> {
      public IEnumerator<bool> GetEnumerator() {
        return InfiniteEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
      }
    }
  }
}