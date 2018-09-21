// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class TransitStopAreaWrapper : Default.Wrappers.TransitStopAreaWrapper, IActumTransitStopAreaWrapper {
    private readonly IActumTransitStopArea _transitStopArea;

    [UsedImplicitly]
    public TransitStopAreaWrapper(Framework.DomainModels.Models.ITransitStopArea transitStopArea) : base(transitStopArea) {
      _transitStopArea = (IActumTransitStopArea)transitStopArea;
    }

    public int BikeOnBoardTerminal {
      get => _transitStopArea.BikeOnBoardTerminal;
      set => _transitStopArea.BikeOnBoardTerminal = value;
    }
    public int Microzone {
      get => _transitStopArea.Microzone;
      set => _transitStopArea.Microzone = value;
    }
  }
}
