// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;

namespace DaySim.Framework.Core {
  public class RandomUtility : IRandomUtility {
    private readonly object _uniform01Lock = new object();
    private readonly object _resetUniform01Lock = new object();


    private Random _random;
    private RandomUniform01 _uniformRandom = new RandomUniform01();

    public int[] GetSeedValues(int size) {
      int[] seedValues = new int[size];

      for (int i = 0; i < size; i++) {
        seedValues[i] = GetRandom().Next(short.MinValue, short.MaxValue + 1);
      }

      return seedValues;
    }

    public int GetNext() {
      return GetRandom().Next();
    }

    private Random GetRandom() {
      return _random;
    }

    private IRandomUniform01 GetRandomUniform() {
      return _uniformRandom;
    }


    public double Uniform01() {
      lock (_uniform01Lock) {
#if DEBUG
        ParallelUtility.countLocks("_uniform01Lock");
#endif

        return GetRandomUniform().Uniform01();
      }
    }

    public void ResetUniform01(int randomSeed = 1) {
      lock (_resetUniform01Lock) {
#if DEBUG
        ParallelUtility.countLocks("_resetUniform01Lock");
#endif

        _uniformRandom.ResetUniform01(randomSeed);
      }
    }

    public void ResetHouseholdSynchronization(int randomSeed = 1) {
      _random = new Random(randomSeed);
    }

    public double Normal(double mean, double stdDev) {
      // Adapted from the following Fortran 77 code
      // ALGORITHM 712, COLLECTED ALGORITHMS FROM ACM.
      // THIS WORK PUBLISHED IN TRANSACTIONS ON MATHEMATICAL SOFTWARE,
      // VOL. 18, NO. 4, DECEMBER, 1992, PP. 434-435.
      // The algorithm uses the ratio of uniforms method of A.J. Kinderman
      // and J.F. Monahan augmented with quadratic bounding curves.

      const double s = .449871;
      const double t = -.386595;
      const double a = .19600;
      const double b = .25472;
      const double r1 = .27597;
      const double r2 = .27846;
      const double vmult = 1.7156;
      const double tiny = .000000000001;

      // Generate P = (u,v) uniform in rectangle enclosing acceptance region
      bool done = false;

      double u;
      double v;

      do {
        do {
          u = Uniform01();
        } while (u < tiny);

        v = vmult * (Uniform01() - .5);

        // Evaluate the quadratic form
        double x = u - s;
        double y = Math.Abs(v) - t;
        double q = (x * x) + y * (a * y - b * x);

        // Accept P if inside inner ellipse
        if (q < r1) {
          done = true;
        } else if ((q <= r2) && (v * v < -4 * Math.Log(u) * (u * u))) {
          done = true;
        }
      } while (!done);

      return mean + stdDev * (v / u);
    }

    public double LogNormal(double mean, double stdDev) {
      const double tiny = .000000000001;

      if (mean <= tiny || stdDev <= tiny) {
        // TODO: generate a warning... ('LogNormal mean and stdDev must be > 0. Value set to 0.');
        return 0;
      }

      double c = stdDev / mean;
      double cSqr = c * c;
      double m = Math.Log(mean) - .5 * Math.Log(cSqr + 1);
      double s = Math.Sqrt(Math.Log(cSqr + 1));

      return Math.Exp(Normal(m, s));
    }
  }
}
