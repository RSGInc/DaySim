namespace DaySim.Framework.Core {
  public interface IRandomUtility {
    int[] GetSeedValues(int size);
    int GetNext();
    double Uniform01();
    void ResetUniform01(int randomSeed = 1);
    void ResetHouseholdSynchronization(int randomSeed = 1);
    double LogNormal(double mean, double stdDev);
  }
}