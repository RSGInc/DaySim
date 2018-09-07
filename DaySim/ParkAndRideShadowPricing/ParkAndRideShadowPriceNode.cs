// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.ShadowPricing;

namespace DaySim.ParkAndRideShadowPricing {
  public sealed class ParkAndRideShadowPriceNode : IParkAndRideShadowPriceNode {
    /// <summary>
    /// Gets or sets the node id.
    /// </summary>
    /// <value>
    /// The node id of the park and ride lot.
    /// </value>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the shadow price difference.
    /// </summary>
    /// <value>
    /// The current shadow price minus the shadow price from the prior iteration for all minutes after 3AM.
    /// </value>
    public double[] ShadowPriceDifference { get; set; }

    /// <summary>
    /// Gets or sets the shadow price of the lot for all minutes after 3AM.
    /// </summary>
    /// <value>
    /// The shadow price for all minutes after 3AM.
    /// </value>
    public double[] ShadowPrice { get; set; }

    /// <summary>
    /// Gets or sets the load in the park and ride lot from exogenous sources.
    /// </summary>
    /// <value>
    /// The load in the park and ride lot from exogenous sources.  Assumed constant throughout the day.
    /// </value>
    public double[] ExogenousLoad { get; set; }

    /// <summary>
    /// Gets or sets the load in the park and ride lot from park and ride demand from all minutes after 3AM.
    /// </summary>
    /// <value>
    /// The load in the park and ride lot arising from park and ride demand for all minutes after 3AM.
    /// </value>
    public double[] ParkAndRideLoad { get; set; }
  }
}