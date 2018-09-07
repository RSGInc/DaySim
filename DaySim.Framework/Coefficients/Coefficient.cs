// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


namespace DaySim.Framework.Coefficients {
  public sealed class Coefficient : ICoefficient {
    private string _label;

    public int Parameter { get; set; }

    public string Label {
      get => string.IsNullOrEmpty(_label) ? "par_" + Parameter : _label;
      set => _label = value;
    }

    public string Constraint { get; set; }

    public double Value { get; set; }

    public bool IsSizeVariable { get; set; }

    public bool IsBaseSizeVariable { get; set; }

    public bool IsParFixed { get; set; }

    public bool IsSizeFunctionMultiplier { get; set; }

    public bool IsNestCoefficient { get; set; }
  }
}