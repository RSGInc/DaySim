// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Core {
    public sealed class RandomUniform01 : IRandomUniform01 {
        private readonly object _randomUniform01Lock = new object();
        private readonly object _randomUniform01ResetLock = new object();
        private int _randseed;
        private int _randSy;
        private int _randSz;

        public RandomUniform01(int randseed = 1) {
            lock (_randomUniform01ResetLock) {
#if DEBUG
                ParallelUtility.countLocks("_randomUniform01ResetLock");
#endif

                ResetUniform01(randseed);
            }
        }

        public double Uniform01() {
            lock (_randomUniform01Lock) {
#if DEBUG
                ParallelUtility.countLocks("_randomUniform01Lock");
#endif

                var r = _randseed / 177;
                var s = _randseed - 177 * r;

                _randseed = 171 * s - 2 * r;

                if (_randseed < 0) {
                    _randseed += 30269;
                }

                r = _randSy / 176;
                s = _randSy - 176 * r;

                _randSy = 172 * s - 35 * r;

                if (_randSy < 0) {
                    _randSy += 30307;
                }

                r = _randSz / 178;
                s = _randSz - 178 * r;

                _randSz = 170 * s - 63 * r;

                if (_randSz < 0) {
                    _randSz += 30323;
                }

                var f = _randseed / 30269D + _randSy / 30307D + _randSz / 30323D;

                f = f - (int)f;

                return f;
            }
        }

        public void ResetUniform01(int randomSeed = 1) {
            _randseed = randomSeed & 0xffff;
            _randSy = 10000;
            _randSz = 3000;
        }
    }
}