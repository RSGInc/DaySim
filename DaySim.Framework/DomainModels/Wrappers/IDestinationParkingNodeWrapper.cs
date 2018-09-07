// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.ShadowPricing;

namespace DaySim.Framework.DomainModels.Wrappers {
  public interface IParkAndRideNodeWrapper : IParkAndRideNode {
    #region flags/choice model/etc. properties

    double[] ShadowPriceDifference { get; set; }

    double[] ShadowPrice { get; set; }

    double[] ExogenousLoad { get; set; }

    double[] ParkAndRideLoad { get; set; }

    #endregion

    #region wrapper methods

    void SetParkAndRideShadowPricing(Dictionary<int, IParkAndRideShadowPriceNode> parkAndRideShadowPrices);

    #endregion
  }
}