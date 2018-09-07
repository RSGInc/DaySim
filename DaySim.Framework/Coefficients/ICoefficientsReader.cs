namespace DaySim.Framework.Coefficients {
  public interface ICoefficientsReader {
    ICoefficient[] Read(string path, out string title, out ICoefficient sizeFunctionMultiplier,
                       out ICoefficient nestCoefficient);
  }
}
