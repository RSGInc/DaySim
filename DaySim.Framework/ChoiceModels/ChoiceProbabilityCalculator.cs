// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.Exceptions;

namespace DaySim.Framework.ChoiceModels {
  public sealed class ChoiceProbabilityCalculator {
    private readonly object _getNestedAlternativeLock = new object();
    private readonly object _getLevelLock = new object();
    private readonly object _getNextPositionLock = new object();
    private readonly object _createUtilityComponentLock = new object();
    private readonly object _addSizeComponentLock = new object();

    private static ChoiceProbabilityCalculator _estimationCalculator;

    private readonly bool _modelIsInEstimationMode;
    private readonly ICoefficient[] _coefficients;
    private readonly ICoefficient _sizeFunctionMultiplier;
    private readonly string _title;

    private readonly Alternative[] _alternatives;
    private readonly NestedAlternative[] _nestedAlternatives;
    private readonly Level[] _levels;
    private readonly List<Component> _utilityComponents = new List<Component>();
    private readonly List<Component> _sizeComponents = new List<Component>();

    private int _nAlternatives;
    private int[] _nChosenAndAvail;
    private int[] _nPredicted;
    private int[] _nAvailable;
    private int[] _nChosenNotAvail;
    private int[] _nChosenOnlyAvail;
    private double[] _totalProb;

    private readonly int _totalUtilities;
    private readonly IObservationItem[] _observation;
    private readonly StreamWriter _tempWriter;

    private int _key;
    private int _position;
    private int _rejectedObservations;
    private int _acceptedObservations;
    private static int _instance = 0;
    private readonly int _instanceId;

    private ChosenAlternative _chosenAlternative;

    public ChoiceProbabilityCalculator(bool modelIsInEstimationMode, ICoefficient[] coefficients, ICoefficient sizeFunctionMultiplier, string title, int totalAlternatives, int totalNestedAlternatives, int totalLevels, int totalUtilities, int totalObservationItems) {
      if (coefficients == null) {
        throw new FileNotFoundException("The coefficient file was not found.");
      }
      _instanceId = _instance++;

      _modelIsInEstimationMode = modelIsInEstimationMode;
      _coefficients = coefficients;
      _sizeFunctionMultiplier = sizeFunctionMultiplier;
      _title = title;

      _alternatives = new Alternative[totalAlternatives];
      _nestedAlternatives = new NestedAlternative[totalNestedAlternatives];
      _levels = new Level[totalLevels];


      if (Global.Configuration.TestEstimationModelInApplicationMode && _estimationCalculator == null) {
        FileInfo temp1 = new FileInfo(Global.GetEstimationPath(Global.Configuration.OutputAlogitDataPath) + ".tst");
        _tempWriter = new StreamWriter(temp1.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

        _estimationCalculator = this;
        _nAlternatives = totalAlternatives;
        _nChosenAndAvail = new int[totalAlternatives];
        _nPredicted = new int[totalAlternatives];
        _nAvailable = new int[totalAlternatives];
        _nChosenNotAvail = new int[totalAlternatives];
        _nChosenOnlyAvail = new int[totalAlternatives];
        _totalProb = new double[totalAlternatives];
      }

      if (!modelIsInEstimationMode) {
        return;
      }

      _estimationCalculator = this;

      _totalUtilities = totalUtilities;
      _observation = new IObservationItem[totalObservationItems];

      if (!Global.Configuration.ShouldOutputAlogitData) {
        return;
      }

      FileInfo temp2 = new FileInfo(Global.GetEstimationPath(Global.Configuration.OutputAlogitDataPath) + ".tmp");

      if (temp2.Directory != null && !temp2.Directory.Exists) {
        temp2.Directory.Create();
      }

      _tempWriter = new StreamWriter(temp2.Open(FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    public bool ModelIsInEstimationMode => _modelIsInEstimationMode;

    public void StartObservation(int key) {
      _key = key;
    }

    private static bool AlternativesUseSizeVariables(IEnumerable<Alternative> alternatives) {
      return (alternatives.Any(a => (a.Utilities.Any(u => u != null && u.HasSizeVariable) || a.AltSizeComponents.Any(c => c != null))));  // JLB 20140703 added size component condition
    }

    public void WriteObservation() {
      if ((!IsValidObservation())) {
        _rejectedObservations++;

        return;
      }

      _acceptedObservations++;

      List<Alternative> alternatives = _alternatives
                .Where(a => a != null && a.Key == _key)
                .ToList();

      bool alternativesUseSizeVariables = AlternativesUseSizeVariables(alternatives);

      foreach (Alternative alternative in alternatives) {
        alternative.AvailableInSummary =
            alternative.Available
            &&
            (!alternativesUseSizeVariables ||
             alternative.Utilities
                .Where(u => u != null && u.Key == _key)
                .Any(u => u.HasSizeVariable && !u.Data.AlmostEquals(0)) ||
            alternative.AltSizeComponents.Any(c => c != null && c.Utilities.Any(u => u != null && !u.Data.AlmostEquals(0))) // JLB 20140703 added the component condition    
            )
            ;
      }

      foreach (IObservationItem observationItem in _observation.Where(oi => oi != null)) {
        if (observationItem.Key == _key) {
          if (_tempWriter != null) {
            _tempWriter.Write(observationItem.Data);
          }

          Alternative alternative = observationItem as Alternative;

          if (alternative != null) {
            alternative.TotalChosenOccurrences += (alternative.IsChosenAlternative ? 1 : 0);
            alternative.TotalAvailableOccurrences += (alternative.AvailableInSummary ? 1 : 0);
          }

          Utility utility = observationItem as Utility;

          if (utility != null) {
            utility.TotalValue += utility.Data;

            if (!utility.Data.AlmostEquals(0)) {
              utility.TotalNonZeroOccurrences++;
            }
          }
        } else {
          if (_tempWriter != null) {
            _tempWriter.Write(0);
          }
        }

        if (_tempWriter != null) {
          _tempWriter.Write(",");
        }
      }

      if (_tempWriter != null) {
        _tempWriter.WriteLine();
      }
    }

    private bool IsValidObservation() {
      List<Alternative> chosenAlternatives =
                _alternatives
                    .Where(a => a != null && a.Key == _key && a.IsChosenAlternative)
                    .ToList();

      if (chosenAlternatives.Count() != 1) {
        return false;
      }

      Alternative chosenAlternative = chosenAlternatives.First();

      bool hasSizeVariables =
                chosenAlternative
                    .Utilities
                    .Where(u => u != null && u.Key == _key)
                    .Any(u => u.HasSizeVariable);

      return
          !hasSizeVariables ||
          chosenAlternative
              .Utilities
              .Where(u => u != null && u.Key == _key)
              .Any(u => u.HasSizeVariable && !u.Data.AlmostEquals(0));
    }

    private void CreateDataFile() {
      if (!Global.Configuration.ShouldOutputAlogitData) {
        return;
      }

      int totalObservationItems = _observation.Count(oi => oi != null);
      FileInfo tempFile = new FileInfo(Global.GetEstimationPath(Global.Configuration.OutputAlogitDataPath) + ".tmp");
      FileInfo dataFile = new FileInfo(Global.GetEstimationPath(Global.Configuration.OutputAlogitDataPath));

      using (CountingReader tempReader = new CountingReader(tempFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (StreamWriter dataWriter = new StreamWriter(dataFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          string line;

          while ((line = tempReader.ReadLine()) != null) {
            double[] tokens = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();

            dataWriter.Write(tempReader.LineNumber);
            dataWriter.Write(" ");

            for (int i = 0; i < totalObservationItems; i++) {
              if (i < tokens.Length) {
                dataWriter.Write(tokens[i]);
                dataWriter.Write(" ");
              } else {
                dataWriter.Write(0);
                dataWriter.Write(" ");
              }
            }

            dataWriter.WriteLine();
          }
        }
      }

      tempFile.Delete();
    }

    private void CreateControlFile() {
      FileInfo controlFile = new FileInfo(Global.GetEstimationPath(Global.Configuration.OutputAlogitControlPath));

      using (StreamWriter controlWriter = new StreamWriter(controlFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
        WriteSection1(controlWriter);
        WriteSection2(controlWriter);
        WriteSection3(controlWriter);
        WriteSection4(controlWriter);
        WriteSection5(controlWriter);
        WriteSection6(controlWriter);
        WriteSection7(controlWriter);
        WriteSection8(controlWriter);
        WriteSection9(controlWriter);
      }
    }

    /// <summary>
    /// Writes the headers in the control file.
    /// </summary>
    private void WriteSection1(TextWriter controlWriter) {
      controlWriter.WriteLine("$TITLE {0}", _title);
      controlWriter.WriteLine("$ESTIMATE");
      controlWriter.WriteLine("$GEN.STATS utilities");
      controlWriter.WriteLine("$ALGOR maxit = 20");
      controlWriter.WriteLine("$ALGOR Zeta  = 0.15");
      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the parameter labels, constraints, and starting values in the control file.
    /// </summary>
    private void WriteSection2(TextWriter controlWriter) {
      List<ICoefficient> coefficients = _coefficients
                .Where(c => c != null)
                .ToList();

      foreach (ICoefficient coefficient in coefficients) {
        controlWriter.Write("{0,3:0}", coefficient.Parameter);
        controlWriter.Write(coefficient.Label.Truncate(10).PadLeft(11));
        controlWriter.Write(coefficient.IsParFixed ? " T " : " F ");
        controlWriter.Write("{0,6:0.0000}", coefficient.Value);
        controlWriter.WriteLine();
      }

      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the nesting structure in the control file.
    /// </summary>
    private void WriteSection3(TextWriter controlWriter) {
      if (_levels.Length == 1) {
        return;
      }

      for (int i = 1; i < _levels.Length; i++) {
        int levelIndex = i;

        foreach (NestedAlternative nestedAlternative in _nestedAlternatives.Where(na => na != null && na.LevelIndex == levelIndex)) {
          controlWriter.Write("$NEST {0} ({1})", nestedAlternative.Id, nestedAlternative.ThetaParameter);

          if (levelIndex == 1) {
            List<Alternative> children = _alternatives
                            .Where(a => a != null && a.Nest.Id == nestedAlternative.Id)
                            .ToList();

            int childrenOnLine = 0;
            foreach (Alternative child in children) {
              childrenOnLine++;
              if (childrenOnLine >= 10) {
                // maximum 10 per line
                controlWriter.WriteLine();
                controlWriter.Write(" +");
                childrenOnLine = 0;
              }
              controlWriter.Write(" {0}", child.Id);
            }
          } else {
            List<NestedAlternative> children = _nestedAlternatives
                            .Where(na => na != null && na.Nest.Id == nestedAlternative.Id)
                            .ToList();

            int childrenOnLine = 0;
            foreach (NestedAlternative child in children) {
              childrenOnLine++;
              if (childrenOnLine >= 10) {
                // maximum 10 per line
                controlWriter.WriteLine();
                controlWriter.Write(" +");
                childrenOnLine = 0;
              }
              controlWriter.Write(" {0}", child.Id);
            }
          }

          controlWriter.WriteLine();
        }
      }

      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the path to the input data file in the control file. 
    /// </summary>
    private void WriteSection4(TextWriter controlWriter) {
      int totalObservationItems = _observation.Count(oi => oi != null);
      FileInfo dataFile = new FileInfo(Global.GetEstimationPath(Global.Configuration.OutputAlogitDataPath));

      controlWriter.WriteLine("$ARRAY DD({0})", totalObservationItems + 1);
      controlWriter.WriteLine("FILE (name={0}) DD", dataFile.Name);
      controlWriter.WriteLine("ID = DD(1)");
      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the choice specifier in the control file.
    /// </summary>
    private void WriteSection5(TextWriter controlWriter) {
      controlWriter.WriteLine("choice = DD({0})", _chosenAlternative.Position + 1);
      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the statistical summary (for checking against ALOGIT output) in the control file.
    /// </summary>
    private void WriteSection6(TextWriter controlWriter) {
      controlWriter.WriteLine("- rejected observations: {0}", _rejectedObservations);
      controlWriter.WriteLine();

      List<Alternative> alternatives =
                _alternatives
                    .Where(a => a != null)
                    .ToList();

      controlWriter.WriteLine("- choice and availability summary -");
      controlWriter.WriteLine();

      foreach (Alternative alternative in alternatives) {
        controlWriter.WriteLine(
            "- alt {0} - {1} / Chosen {2} Available {3}",
            string.Format("{0,3:0}", alternative.Id),
            alternative.Label.Truncate(10).PadLeft(10),
            string.Format("{0,8:0}", alternative.TotalChosenOccurrences),
            string.Format("{0,8:0}", alternative.TotalAvailableOccurrences));
      }

      controlWriter.WriteLine();
      controlWriter.WriteLine("- utility summary -");

      foreach (Alternative alternative in alternatives) {
        controlWriter.WriteLine();

        foreach (Utility utility in alternative.Utilities.Where(u => u != null)) {
          ICoefficient coefficient = _coefficients[utility.Parameter];
          string label =
                        coefficient == null
                            ? utility.Label
                            : coefficient.Label;

          controlWriter.WriteLine(
              "- alt {0} - {1} / {2} - {3} % Non-0 {4} Mean {5}",
              string.Format("{0,3:0}", alternative.Id),
              alternative.Label.Truncate(10).PadLeft(10),
              string.Format("{0,3:0}", utility.Parameter),
              label.Truncate(10).PadLeft(10),
              string.Format("{0,6:0.00}", (utility.TotalNonZeroOccurrences * 100D / _acceptedObservations)),
              string.Format("{0,8:0.00}", utility.TotalValue / (utility.TotalNonZeroOccurrences + Constants.EPSILON)));
        }
      }

      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the availability in the control file.
    /// </summary>
    private void WriteSection7(TextWriter controlWriter) {
      List<Alternative> alternatives =
                _alternatives
                    .Where(a => a != null)
                    .ToList();

      foreach (Alternative alternative in alternatives) {
        controlWriter.WriteLine(
            "avail({0})=DD({1})",
            alternative.Id,
            alternative.Position + 1);
      }

      controlWriter.WriteLine();
    }

    /// <summary>
    /// Writes the utility functions in the control file.
    /// </summary>
    private void WriteSection8(TextWriter controlWriter) {
      List<Alternative> alternatives =
                _alternatives
                    .Where(a => a != null)
                    .ToList();

      foreach (Alternative alternative in alternatives) {
        controlWriter.WriteLine("util({0})=0", alternative.Id);

        foreach (Utility utility in alternative.Utilities
            //.Where(u => u != null && !u.HasSizeVariable && !u.TotalValue.AlmostEquals(0))) {  //JLB 20121119 per MAB to stop suppression when var = 0 for all observations
            .Where(u => u != null && !u.HasSizeVariable)) {
          controlWriter.WriteLine(
              "{0} p{1}*DD({2})",
              "+".PadLeft(3),
              utility.Parameter,
              utility.Position + 1);
        }

        foreach (Component component in alternative.AltUtilityComponents.Where(c => c != null)) {
          //foreach (var utility in component.Utilities.Where(u => u != null && !u.HasSizeVariable && !u.TotalValue.AlmostEquals(0))) {  //JLB 20121119 per MAB to stop suppression when var = 0 for all observations
          foreach (Utility utility in component.Utilities.Where(u => u != null && !u.HasSizeVariable)) {
            controlWriter.WriteLine(
                "{0} p{1}*DD({2})",
                "+".PadLeft(3),
                utility.Parameter,
                utility.Position + 1);
          }
        }
        controlWriter.WriteLine();
      }
    }

    /// <summary>
    /// Writes the size functions in the control file.
    /// Size functions are only added if there are any size variables at all. (MB)
    /// </summary>
    private void WriteSection9(TextWriter controlWriter) {
      List<Alternative> alternatives =
                _alternatives
                    .Where(a => a != null)
                    .ToList();

      if (!AlternativesUseSizeVariables(alternatives)) {
        return;
      }

      foreach (Alternative alternative in alternatives) {
        controlWriter.WriteLine("size({0})=0", alternative.Id);

        foreach (Utility utility in alternative.Utilities.Where(u => u != null)) {
          ICoefficient coefficient = _coefficients[utility.Parameter];

          if (coefficient == null) {
            continue;
          }

          if (coefficient.IsBaseSizeVariable) {
            controlWriter.WriteLine(
                "{0} DD({1}) + p{2}*0",
                "+".PadLeft(3),
                utility.Position + 1,
                utility.Parameter);
          } else if (coefficient.IsSizeVariable) {
            controlWriter.WriteLine(
                "{0} p{1}*DD({2})",
                "+".PadLeft(3),
                utility.Parameter,
                utility.Position + 1);
          }
        }

        foreach (Component component in alternative.AltSizeComponents.Where(c => c != null)) {
          foreach (Utility utility in component.Utilities.Where(u => u != null)) {
            ICoefficient coefficient = _coefficients[utility.Parameter];

            if (coefficient == null) {
              continue;
            }

            if (coefficient.IsBaseSizeVariable) {
              controlWriter.WriteLine(
                  "{0} DD({1}) + p{2}*0",
                  "+".PadLeft(3),
                  utility.Position + 1,
                  utility.Parameter);
            } else if (coefficient.IsSizeVariable) {
              controlWriter.WriteLine(
                  "{0} p{1}*DD({2})",
                  "+".PadLeft(3),
                  utility.Parameter,
                  utility.Position + 1);
            }
          }
        }
        controlWriter.WriteLine();
      }

      if (_sizeFunctionMultiplier != null) {
        controlWriter.WriteLine("$L_S_M {0}", _sizeFunctionMultiplier.Parameter);
      }
    }

    private int GetNextPosition() {
      if (_modelIsInEstimationMode) {
        return _position++;
      }

      return _position;
    }

    public Alternative GetAlternative(int index, bool available, bool isChosenAlternative = false) {
      Alternative alternative = _alternatives[index];

      if (alternative == null) {
        alternative = new Alternative(this, index);

        _alternatives[index] = alternative;
      }

      alternative.Update(_key, available, isChosenAlternative);

      if (_modelIsInEstimationMode) {
        AddObservation(alternative);
        alternative.NewUtilityComponentsList();
        alternative.NewSizeComponentsList();
      }

      return alternative;
    }

    private NestedAlternative GetNestedAlternative(int id, int index, int levelIndex, int thetaParameter) {

      NestedAlternative nestedAlternative = _nestedAlternatives[index];

      if (nestedAlternative == null) {
        nestedAlternative = new NestedAlternative(this, id, index, levelIndex, thetaParameter, _modelIsInEstimationMode ? Constants.DEFAULT_VALUE : _coefficients[thetaParameter].Value);

        _nestedAlternatives[index] = nestedAlternative;
      }

      nestedAlternative.Update(_key);

      return nestedAlternative;

    }

    private Level GetLevel(int levelIndex) {

      Level level = _levels[levelIndex];

      if (level == null) {
        level = new Level(_nestedAlternatives.Length);

        _levels[levelIndex] = level;
      }

      return level;

    }

    public void CreateUtilityComponent(int index) {

      if (index >= _utilityComponents.Count) {
        for (int i = _utilityComponents.Count; i <= index; i++) {
          _utilityComponents.Add(null);
        }
      }

      Component component = _utilityComponents[index];

      if (component == null) {
        component = new Component(this, index);

        _utilityComponents[index] = component;
      }

      component.Update(_key);

    }

    public Component GetUtilityComponent(int index) {
      return _utilityComponents[index];
    }

    public void CreateSizeComponent(int index) {
      if (index >= _sizeComponents.Count) {
        for (int i = _sizeComponents.Count; i <= index; i++) {
          _sizeComponents.Add(null);
        }
      }

      Component component = _sizeComponents[index];

      if (component == null) {
        component = new Component(this, index);

        _sizeComponents[index] = component;
      }

      component.Update(_key);

    }

    public Component GetSizeComponent(int index) {
      return _sizeComponents[index];
    }

    private void AddObservation(Alternative alternative) {
      AddObservation((IObservationItem)alternative);

      if (!alternative.IsChosenAlternative) {
        return;
      }

      if (_chosenAlternative == null) {
        _chosenAlternative = new ChosenAlternative(GetNextPosition());
      }

      _chosenAlternative.Update(_key, alternative.Id);

      AddObservation(_chosenAlternative);
    }

    private void AddObservation(IObservationItem observationItem) {
      _observation[observationItem.PositionIndex] = observationItem;
    }

    public Alternative SimulateChoice(IRandomUtility randomUtility, int id = Constants.DEFAULT_VALUE, int observed = Constants.DEFAULT_VALUE) {

      foreach (Alternative alternative in _alternatives.Where(a => a != null && a.Key == _key && a.Nest != null)) {
        alternative.Nest.UtilitySum = 0;
      }

      foreach (Alternative alternative in _alternatives.Where(a => a != null && a.Key == _key && a.Available)) {
        if (alternative.Size >= Constants.EPSILON) {
          alternative.Utility += Math.Log(alternative.Size) * _sizeFunctionMultiplier.Value;
        }

        alternative.Utility = Math.Exp(alternative.Utility);
        alternative.SumUtility(alternative.Utility);
      }

      for (int i = 1; i < _levels.Length; i++) {
        int levelIndex = i;

        foreach (NestedAlternative nestedAlternative in _nestedAlternatives.Where(na => na != null && na.Key == _key && na.LevelIndex == levelIndex)) {
          if (nestedAlternative.UtilitySum >= Constants.EPSILON) {
            nestedAlternative.Utility = Math.Exp(nestedAlternative.Theta * Math.Log(nestedAlternative.UtilitySum));
            nestedAlternative.SumUtility(nestedAlternative.Utility);
          } else {
            nestedAlternative.Utility = 0.0;
          }
        }

        foreach (NestedAlternative nestedAlternative in _nestedAlternatives.Where(na => na != null && na.Key == _key && na.LevelIndex == levelIndex)) {
          nestedAlternative.Probability = nestedAlternative.Utility / nestedAlternative.Sum;
        }
      }

      foreach (Alternative alternative in _alternatives.Where(a => a != null && a.Key == _key && a.Available)) {
        alternative.Probability = alternative.Utility / alternative.Sum;

        NestedAlternative nestedAlternative = alternative.Nest;

        while (nestedAlternative != null) {
          alternative.Probability *= nestedAlternative.Probability;

          nestedAlternative = nestedAlternative.Nest;
        }
      }

      return DrawAlternative(randomUtility, id, observed);

    }

    private Alternative DrawAlternative(IRandomUtility randomUtility, int id = Constants.DEFAULT_VALUE, int observed = Constants.DEFAULT_VALUE) {
      Alternative chosenAlternative = null;
      double random = randomUtility.Uniform01();

      foreach (Alternative alternative in _alternatives.Where(a => a != null && a.Key == _key && a.Available)) {
        chosenAlternative = alternative;
        random -= alternative.Probability;

        if (random < 0) {
          break;
        }
      }

      if (Global.Configuration.TraceSimulatedChoiceOutcomes && _key != 0) {
        Global.PrintFile.WriteLine("> Key {0} Alternative {1} chosen for model {2}", _key,
                                   chosenAlternative == null ? Constants.DEFAULT_VALUE : chosenAlternative.Id, _title);
      }

      if (chosenAlternative != null && Global.Configuration.TestEstimationModelInApplicationMode && observed >= 0 && _tempWriter != null) {
        bool observedAvail = false;
        bool nonObservedAvail = false;
        double probObserved = -1;

        foreach (Alternative alternative in _alternatives.Where(a => a != null && a.Key == _key)) {
          if (alternative.Id - 1 == observed) {
            observedAvail = alternative.Available;
            probObserved = alternative.Probability;
          } else if (!nonObservedAvail) {
            nonObservedAvail = alternative.Available;
          }
        }
        _tempWriter.Write("{0} {1} {2} {3}", id, observed, observedAvail && nonObservedAvail ? probObserved : -1,
             chosenAlternative.Id - 1);
        if (!observedAvail) {
          _nChosenNotAvail[observed]++;
        } else if (!nonObservedAvail) {
          _nChosenOnlyAvail[observed]++;
        } else {
          _nChosenAndAvail[observed]++;
          _nPredicted[chosenAlternative.Id - 1]++;
        }
        foreach (Alternative alternative in _alternatives.Where(a => a != null && a.Key == _key)) {
          _tempWriter.Write(" {0}", alternative.Available ? alternative.Probability : -1);
          if (alternative.Available && observedAvail && nonObservedAvail) {
            _nAvailable[alternative.Id - 1]++;
            _totalProb[alternative.Id - 1] += alternative.Probability;
          }
        }
        _tempWriter.WriteLine();
      }



      //            if (chosenAlternative == null) {
      //                chosenAlternative = _alternatives[0];
      //                Global.PrintFile.WriteLine("SimulateChoice.DrawAlternative had no available alternatives to choose from. Alternative 0 set as chosen, key is {0}",chosenAlternative.Key);
      //            }

      return chosenAlternative;
    }

    public static void Close() {

      if (_estimationCalculator == null) {
        return;
      }

      if (_estimationCalculator._tempWriter != null) {
        _estimationCalculator._tempWriter.Close();

        if (!Global.Configuration.TestEstimationModelInApplicationMode) {
          _estimationCalculator.CreateDataFile();
        }
      }

      if (!Global.Configuration.TestEstimationModelInApplicationMode) {
        _estimationCalculator.CreateControlFile();
      } else {
        Global.PrintFile.WriteLine();
        Global.PrintFile.WriteLine("TEST OF ESTIMATION MODEL {0} IN APPLICATION MODE", Global.Configuration.EstimationModel);
        for (int altN = 0; altN < _estimationCalculator._nAlternatives; altN++) {
          Global.PrintFile.WriteLine("Alt {0} NChosenOnlyAvailable= {1} NChosenNotAvailable= {2} NAvailableValid= {3} NChosenValid= {4} NPredicted= {5} SumProbabilities= {6}", altN,
               _estimationCalculator._nChosenOnlyAvail[altN],
               _estimationCalculator._nChosenNotAvail[altN],
               _estimationCalculator._nAvailable[altN],
               _estimationCalculator._nChosenAndAvail[altN],
               _estimationCalculator._nPredicted[altN],
               _estimationCalculator._totalProb[altN]);
        }
      }

    }

    public sealed class Alternative : IObservationItem {
      private bool _available;
      private readonly ChoiceProbabilityCalculator _choiceProbabilityCalculator;
      private readonly Level _level;
      private readonly int _index;

      public Alternative(ChoiceProbabilityCalculator choiceProbabilityCalculator, int index) {
        _choiceProbabilityCalculator = choiceProbabilityCalculator;
        _level = _choiceProbabilityCalculator.GetLevel(0);

        PositionIndex = _choiceProbabilityCalculator.GetNextPosition();
        _index = index;

        if (_choiceProbabilityCalculator._modelIsInEstimationMode) {
          Utilities = new Utility[_choiceProbabilityCalculator._totalUtilities];
        }
      }

      public int PositionIndex { get; private set; }

      public int Position => PositionIndex + 1;

      public int Id => _index + 1;

      public List<Component> AltUtilityComponents { get; private set; }

      public List<Component> AltSizeComponents { get; private set; }

      public Utility[] Utilities { get; private set; }

      public int Key { get; private set; }

      public double Size { get; private set; }

      public NestedAlternative Nest { get; private set; }

      public bool IsChosenAlternative { get; private set; }

      public double Data { get; private set; }

      public string Label => "alt_" + Id;

      public bool Available {
        get => _available;
        set {
          _available = value;
          Data = value ? 1 : 0;
        }
      }

      public double Utility { get; set; }

      public double Probability { get; set; }

      public object Choice { get; set; }

      public int TotalAvailableOccurrences { get; set; }

      public int TotalChosenOccurrences { get; set; }

      public bool AvailableInSummary { get; set; }

      public double Sum => Nest == null ? _level.DefaultSum : _level.Sums[Nest.Index];

      public void NewUtilityComponentsList() {
        AltUtilityComponents = new List<Component>();
      }

      public void NewSizeComponentsList() {
        AltSizeComponents = new List<Component>();
      }

      public void Update(int key, bool available, bool isChosenAlternative) {
        //if (Global.TraceResults && true) {
        //  Global.PrintFile.WriteLine("Alternative {0}.Update(key={1}, available={2}, isChosenAlternative={3}", this, key, available, isChosenAlternative);
        //}
        Key = key;
        Size = 0;
        Nest = null;
        IsChosenAlternative = isChosenAlternative;
        Available = available;
        Utility = 0;
        Probability = 0;
        Choice = null;

        _level.Reset();
      }

      public void SumUtility(double utility) {
        if (Nest == null) {
          _level.DefaultSum += utility;
        } else {
          _level.Sums[Nest.Index] += utility;

          Nest.UtilitySum = _level.Sums[Nest.Index];
        }
      }

      public void AddUtilityTerm(int parameter, double value) {
        //if (Global.TraceResults && true) {
        //  Global.PrintFile.WriteLine("Alternative {0}.AddUtilityTerm(parameter={1},value={2}", this, parameter, value);
        //}

        if (double.IsNaN(value)) {
          throw new ValueIsNaNException(string.Format(@"Value is NaN for alternative {0}, parameter {1}.", _index, parameter));
        }

        if (double.IsInfinity(value)) {
          throw new ValueIsInfinityException(string.Format(@"Value is Infinity for alternative {0}, parameter {1}.", _index, parameter));
        }

        if (parameter >= _choiceProbabilityCalculator._coefficients.Length) {
          return;
        }

        ICoefficient coefficient = _choiceProbabilityCalculator._coefficients[parameter];

        if (coefficient == null) {
          return;
        }

        if (_choiceProbabilityCalculator._modelIsInEstimationMode) {
          Utility utility = Utilities[parameter];

          if (utility == null) {
            utility = new Utility(_choiceProbabilityCalculator.GetNextPosition(), parameter, coefficient.IsBaseSizeVariable || coefficient.IsSizeVariable);

            Utilities[parameter] = utility;
          }

          utility.Update(Key, value);

          _choiceProbabilityCalculator.AddObservation(utility);
        } else {
          if (value == 0.0) {
            return;
          }

          if (coefficient.IsBaseSizeVariable) {
            Size += value;
          } else if (coefficient.IsSizeVariable) {
            Size += (value * Math.Exp(coefficient.Value));
          } else if (!coefficient.IsSizeFunctionMultiplier) {
            Utility += (value * coefficient.Value);
          }
        }
      }

      public void AddNestedAlternative(int id, int index, int thetaParameter) {
        Nest = _choiceProbabilityCalculator.GetNestedAlternative(id, index, 1, thetaParameter);
      }

      public void AddUtilityComponent(Component component) {
        //if (Global.TraceResults && true) {
        //  Global.PrintFile.WriteLine("Alternative {0}.AddUtilityComponent(component={1}", this, component);
        //}
        if (_choiceProbabilityCalculator._modelIsInEstimationMode) {
          AltUtilityComponents.Add(component);
        } else {
          Utility += (component.Utility);
        }
      }

      public void AddSizeComponent(Component component) {
        //if (Global.TraceResults && true) {
        //  Global.PrintFile.WriteLine("Alternative {0}.AddSizeComponent(component={1}", this, component);
        //}
        if (_choiceProbabilityCalculator._modelIsInEstimationMode) {
          AltSizeComponents.Add(component);
        } else {
          Size += (component.Size);
        }
      }

      public double ComputeLogsum() {
        if (Nest == null) {
          double logsum = Math.Log(Sum);

          return double.IsInfinity(logsum) ? 0 : logsum;
        } else {
          NestedAlternative nestedAlternative = Nest;

          while (nestedAlternative.Nest != null) {
            nestedAlternative = nestedAlternative.Nest;
          }

          double logsum = Math.Log(nestedAlternative.Sum);

          return double.IsInfinity(logsum) ? 0 : logsum;
        }
      }
    }

    public sealed class NestedAlternative {
      private readonly ChoiceProbabilityCalculator _choiceProbabilityCalculator;
      private readonly Level _level;

      public NestedAlternative(ChoiceProbabilityCalculator choiceProbabilityCalculator, int id, int index, int levelIndex, int thetaParameter, double theta) {
        _choiceProbabilityCalculator = choiceProbabilityCalculator;
        _level = _choiceProbabilityCalculator.GetLevel(levelIndex);

        Id = id;
        Index = index;
        LevelIndex = levelIndex;
        ThetaParameter = thetaParameter;
        Theta = theta;
      }

      public int Id { get; private set; }

      public int Index { get; private set; }

      public int LevelIndex { get; private set; }

      public int ThetaParameter { get; private set; }

      public double Theta { get; private set; }

      public NestedAlternative Nest { get; private set; }

      public int Key { get; private set; }

      public double Utility { get; set; }

      public double Probability { get; set; }

      public double Sum => Nest == null ? _level.DefaultSum : _level.Sums[Nest.Index];

      public double UtilitySum { get; set; }

      public void Update(int key) {
        Key = key;

        _level.Reset();
      }

      public void SumUtility(double utility) {
        if (Nest == null) {
          _level.DefaultSum += utility;
        } else {
          _level.Sums[Nest.Index] += utility;

          Nest.UtilitySum = _level.Sums[Nest.Index];
        }
      }

      public void AddNestedAlternative(int id, int index, int thetaParameter) {
        Nest = _choiceProbabilityCalculator.GetNestedAlternative(id, index, LevelIndex + 1, thetaParameter);
      }
    }

    private sealed class Level {
      private readonly double[] _sums;

      public Level(int totalNestedAlternatives) {
        _sums = new double[totalNestedAlternatives];
      }

      public double DefaultSum { get; set; }

      public double[] Sums => _sums;

      public void Reset() {
        DefaultSum = 0;

        for (int i = 0; i < _sums.Length; i++) {
          _sums[i] = 0;
        }
      }
    }

    public sealed class Component {
      private readonly ChoiceProbabilityCalculator _choiceProbabilityCalculator;
      private readonly int _index;
      //private readonly double _multiplier; //no longer used


      public Component(ChoiceProbabilityCalculator choiceProbabilityCalculator, int index) {
        _choiceProbabilityCalculator = choiceProbabilityCalculator;
        _index = index;
        //_multiplier = 1.0; //no longer used

        if (_choiceProbabilityCalculator._modelIsInEstimationMode) {
          Utilities = new Utility[_choiceProbabilityCalculator._totalUtilities];
        }
      }

      public Utility[] Utilities { get; private set; }

      public int Key { get; private set; }

      public double Size { get; private set; }

      public double Utility { get; set; }

      public int Index => _index;

      public void Update(int key) {
        Key = key;
        Size = 0;
        Utility = 0;
      }

      public void AddUtilityTerm(int parameter, double value) {
        //if (Global.TraceResults && true) {
        //  Global.PrintFile.WriteLine("Component {0}.AddUtilityTerm(parameter={1}, value={2}", this, parameter, value);
        //}

        if (double.IsNaN(value)) {
          throw new ValueIsNaNException(string.Format(@"Value is NaN for component {0}, parameter {1}.", _index, parameter));
        }

        if (double.IsInfinity(value)) {
          throw new ValueIsInfinityException(string.Format(@"Value is Infinity for component {0}, parameter {1}.", _index, parameter));
        }

        if (parameter >= _choiceProbabilityCalculator._coefficients.Length) {
          return;
        }

        ICoefficient coefficient = _choiceProbabilityCalculator._coefficients[parameter];

        if (coefficient == null) {
          return;
        }

        if (_choiceProbabilityCalculator._modelIsInEstimationMode) {
          Utility utility = Utilities[parameter];

          if (utility == null) {
            utility = new Utility(_choiceProbabilityCalculator.GetNextPosition(), parameter, coefficient.IsBaseSizeVariable || coefficient.IsSizeVariable);

            Utilities[parameter] = utility;
          }

          utility.Update(Key, value);

          _choiceProbabilityCalculator.AddObservation(utility);
        } else {
          if (value == 0.0) {
            return;
          }

          if (coefficient.IsBaseSizeVariable) {
            Size += value;
          } else if (coefficient.IsSizeVariable) {
            Size += (value * Math.Exp(coefficient.Value));
          } else if (!coefficient.IsSizeFunctionMultiplier) {
            Utility += (value * coefficient.Value);
          }
        }
      }
    }
  }
}
