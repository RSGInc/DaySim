// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;

namespace DaySim.Framework.Factories {
  [AttributeUsage(AttributeTargets.Class)]
  public class FactoryAttribute : Attribute {
    public FactoryAttribute(Factory factory) {
      Factory = factory;

      DataType = DataType.Default;
    }

    public Factory Factory { get; private set; }

    public DataType DataType { get; set; }

    public ChoiceModelRunner ChoiceModelRunner { get; set; }

    public Category Category { get; set; }
  }
}