// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System.IO;
using DaySim.Framework.Coefficients;

namespace DaySim.Framework.ChoiceModels {
  public sealed class ChoiceProbabilityFactory {
    private readonly string _title;
    private readonly ICoefficient _sizeFunctionMultiplier;
    private readonly ICoefficient[] _coefficients;
    private readonly bool _modelIsInEstimationMode;
    private readonly int _totalAlternatives;
    private readonly int _totalNestedAlternatives;
    private readonly int _totalLevels;
    private readonly int _totalUtilities;

    private ChoiceProbabilityCalculator _choiceProbabilityCalculator;

    public ChoiceProbabilityFactory(string coefficientsPath, bool modelIsInEstimationMode, int totalAlternatives, int totalNestedAlternatives, int totalLevels, int maxParameter, ICoefficientsReader reader = null) {
      ICoefficientsReader coefficientsReader = null;
      if (reader != null) {
        coefficientsReader = reader;
      }

      if (reader == null) {
        FileInfo file = new FileInfo(coefficientsPath);

        if (file.Exists) {
          coefficientsReader = new CoefficientsReader();

        }
      }
      if (coefficientsReader == null) {

      }

      if (coefficientsReader != null) {

        _coefficients = coefficientsReader.Read(coefficientsPath, out _title, out _sizeFunctionMultiplier,
                                                out ICoefficient nestCoefficient);
      }
      _modelIsInEstimationMode = modelIsInEstimationMode;
      _totalAlternatives = totalAlternatives;
      _totalNestedAlternatives = totalNestedAlternatives;
      _totalLevels = totalLevels;
      _totalUtilities = maxParameter + 1;
    }

    public ChoiceProbabilityCalculator GetChoiceProbabilityCalculator(int key, bool nested = false) {
      ChoiceProbabilityCalculator choiceProbabilityCalculator;

      if (nested) {
        choiceProbabilityCalculator = new ChoiceProbabilityCalculator(_modelIsInEstimationMode, _coefficients, _sizeFunctionMultiplier, _title, _totalAlternatives, _totalNestedAlternatives, _totalLevels, _totalUtilities, (_totalAlternatives * _totalUtilities) + 1);
      } else {
        if (_choiceProbabilityCalculator == null) {
          _choiceProbabilityCalculator = new ChoiceProbabilityCalculator(_modelIsInEstimationMode, _coefficients, _sizeFunctionMultiplier, _title, _totalAlternatives, _totalNestedAlternatives, _totalLevels, _totalUtilities, (_totalAlternatives * _totalUtilities) + 1);
        }

        choiceProbabilityCalculator = _choiceProbabilityCalculator;
      }

      choiceProbabilityCalculator.StartObservation(key);

      return choiceProbabilityCalculator;
    }
  }
}