// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;

namespace DaySim.Framework.Factories {
  public class ChoiceModelRunnerTypeLocator : TypeLocator {
    public ChoiceModelRunnerTypeLocator(Configuration configuration) : base(configuration) { }

    public Type GetChoiceModelRunnerType() {
      List<Type> types = new List<Type>();
      System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

      foreach (System.Reflection.Assembly assembly in assemblies) {
        types
            .AddRange(
                assembly
                    .GetTypes()
                    .Where(type => Attribute.IsDefined(type, typeof(FactoryAttribute)) && typeof(IChoiceModelRunner).IsAssignableFrom(type)));
      }

      foreach (Type type in types) {
        FactoryAttribute attribute =
                    type
                        .GetCustomAttributes(typeof(FactoryAttribute), false)
                        .Cast<FactoryAttribute>()
                        .FirstOrDefault(x => x.Factory == Factory.ChoiceModelFactory && x.ChoiceModelRunner == ChoiceModelRunner);

        if (attribute != null) {
          return type;
        }
      }

      throw new Exception(string.Format("Unable to determine type. The combination using {0} and {1} for type {2} was not found.", Factory.ChoiceModelFactory, ChoiceModelRunner, typeof(IChoiceModelRunner).Name));
    }
  }
}