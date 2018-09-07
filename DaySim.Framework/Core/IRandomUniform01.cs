namespace DaySim.Framework.Core {
  public interface IRandomUniform01 {
    //RandomUniform01(int randseed = 1);

    double Uniform01();

    void ResetUniform01(int randomSeed = 1);
  }
}