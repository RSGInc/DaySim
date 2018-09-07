// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Default.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
  public class ParcelNodeWrapper : IParcelNodeWrapper {
    private readonly IParcelNode _parcelNode;

    [UsedImplicitly]
    public ParcelNodeWrapper(IParcelNode parcelNode) {
      _parcelNode = parcelNode;
    }

    #region domain model properies

    public int Id {
      get => _parcelNode.Id;
      set => _parcelNode.Id = value;
    }

    public int NodeId {
      get => _parcelNode.NodeId;
      set => _parcelNode.NodeId = value;
    }

    #endregion
  }
}