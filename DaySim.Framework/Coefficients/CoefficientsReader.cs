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
using DaySim.Framework.Core;

namespace DaySim.Framework.Coefficients {
  public class CoefficientsReader : ICoefficientsReader {
    public virtual ICoefficient[] Read(string path, out string title, out ICoefficient sizeFunctionMultiplier, out ICoefficient nestCoefficient) {
      title = null;
      sizeFunctionMultiplier = null;
      nestCoefficient = null;

      Dictionary<int, Coefficient> coefficients = new Dictionary<int, Coefficient>();
      FileInfo file = new FileInfo(path);
      bool baseSizeVariableFound = false;

      using (CountingReader reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        string line;

        while ((line = reader.ReadLine()) != null) {
          if (line.Trim() == "END") {
            break;
          }

          if (string.IsNullOrEmpty(title)) {
            title = line;
          }
        }

        while ((line = reader.ReadLine()) != null) {
          if (line.Trim() == "-1") {
            break;
          }

          string[] tokens = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

          if (tokens.Length < 1) {
            continue;
          }


          int.TryParse(tokens[0], out int parameter);

          string label = tokens[1].Trim();
          string constraint = tokens[2];


          double.TryParse(tokens[3], out double coefficientValue);

          Coefficient coefficient = new Coefficient {
            Parameter = parameter,
            Label = label,
            Constraint = constraint,
            Value = coefficientValue,
            IsSizeVariable = label.StartsWith("Gamm"),
            IsParFixed = constraint.ToLower() == "t" || constraint.ToLower() == "c",
            IsSizeFunctionMultiplier = label.StartsWith("LSM_"),
            IsNestCoefficient = label.StartsWith("Nest")
          };

          if (coefficient.IsSizeFunctionMultiplier) {
            sizeFunctionMultiplier = coefficient;
          }

          if (coefficient.IsNestCoefficient) {
            nestCoefficient = coefficient;
          }

          if (!baseSizeVariableFound && coefficient.IsSizeVariable && coefficient.IsParFixed && coefficient.Value.AlmostEquals(0)) {
            baseSizeVariableFound = true;

            coefficient.IsBaseSizeVariable = true;
          }

          coefficients.Add(parameter, coefficient);
        }
      }

      int max = coefficients.Values.Max(c => c.Parameter) + 1;
      Coefficient[] array = new Coefficient[max];

      for (int i = 0; i <= max; i++) {

        if (coefficients.TryGetValue(i, out Coefficient coefficient)) {
          array[i] = coefficient;
        }
      }

      return array;
    }
  }
}