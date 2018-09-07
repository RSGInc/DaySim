namespace DaySim.Framework.Coefficients {
  public interface ICoefficient {
    int Parameter { get; set; }

    string Label { get; set; }

    string Constraint { get; set; }

    double Value { get; set; }

    bool IsSizeVariable { get; set; }

    bool IsBaseSizeVariable { get; set; }

    bool IsParFixed { get; set; }

    bool IsSizeFunctionMultiplier { get; set; }

    bool IsNestCoefficient { get; set; }
  }
}
