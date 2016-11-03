// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.Framework.ChoiceModels {
    public sealed class ChoiceModelHelper {
        private ChoiceProbabilityFactory _choiceProbabilityFactory;

        public bool ModelIsInEstimationMode { get; private set; }


        public static void Initialize(ref ChoiceModelHelper helper, string choiceModelName, string coefficientsPath,
                                      int totalAlternatives, int totalNestedAlternatives, int totalLevels, int maxParameter,
                                      ICoefficientsReader reader = null) {
            if (helper != null) {
                return;
            }

            var modelIsInEstimationMode = Global.Configuration.IsInEstimationMode &&
                                          Global.Configuration.EstimationModel == choiceModelName
                                          && !Global.Configuration.TestEstimationModelInApplicationMode;

            helper = new ChoiceModelHelper {
                ModelIsInEstimationMode = modelIsInEstimationMode
            };

            helper.InitializeFactory(coefficientsPath, modelIsInEstimationMode, totalAlternatives, totalNestedAlternatives,
                                     totalLevels, maxParameter, reader);

            UpdateTimesModelRun(choiceModelName);

        }

        private const int maxModelsRun = 500;
        private static int _nModelsRun = 0;
        private static string[] _modelName = new string[maxModelsRun];
        private static int[] _timesModelRun = new int[maxModelsRun];

        public static void UpdateTimesModelRun(string choiceModelName) {
            var nModel = 0;
            while (nModel < _nModelsRun && choiceModelName != _modelName[nModel]) {
                nModel++;
            }
            if (nModel >= _nModelsRun) {
                _nModelsRun = nModel;
                _modelName[nModel] = choiceModelName;
            }
            _timesModelRun[nModel]++;
        }

        public static void WriteTimesModelsRun() {
            for (var nModel = 0; nModel < _nModelsRun; nModel++) {
                Global.PrintFile.WriteLine("Model {0} run {1} times", _modelName[nModel], _timesModelRun[nModel]);
            }
        }


        private void InitializeFactory(string coefficientsPath, bool modelIsInEstimationMode, int totalAlternatives,
                                       int totalNestedAlternatives, int totalLevels, int maxParameter,
                                       ICoefficientsReader reader = null) {
            _choiceProbabilityFactory = new ChoiceProbabilityFactory(coefficientsPath, modelIsInEstimationMode,
                                                                     totalAlternatives, totalNestedAlternatives,
                                                                     totalLevels, maxParameter, reader);
        }

        public ChoiceProbabilityCalculator GetChoiceProbabilityCalculator(int key) {
            return _choiceProbabilityFactory.GetChoiceProbabilityCalculator(key);
        }

        public ChoiceProbabilityCalculator GetNestedChoiceProbabilityCalculator() {
            return _choiceProbabilityFactory.GetChoiceProbabilityCalculator(0, true);
        }
    }
}