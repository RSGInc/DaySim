// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using Daysim.Framework.Core;
using Daysim.Framework.ShadowPricing;

namespace Daysim.ParkAndRideShadowPricing
{
    public static class ParkAndRideShadowPriceReader
    {
        public static Dictionary<int, IParkAndRideShadowPriceNode> ReadParkAndRideShadowPrices()
        {
            var shadowPrices = new Dictionary<int, IParkAndRideShadowPriceNode>();
            var shadowPriceFile = new FileInfo(Global.ParkAndRideShadowPricesPath);

            if (!Global.ParkAndRideNodeIsEnabled || !shadowPriceFile.Exists || !Global.Configuration.ShouldUseParkAndRideShadowPricing || Global.Configuration.IsInEstimationMode)
            {
                return shadowPrices;
            }

            using (var reader = new CountingReader(shadowPriceFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                reader.ReadLine();

                string line = null;
                try
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        var tokens = line.Split(new[] { Global.Configuration.ParkAndRideShadowPriceDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                        var shadowPriceNode = new ParkAndRideShadowPriceNode
                        {
                            NodeId = Convert.ToInt32(tokens[0]),
                        };

                        shadowPriceNode.ShadowPriceDifference = new double[Global.Settings.Times.MinutesInADay];
                        shadowPriceNode.ShadowPrice = new double[Global.Settings.Times.MinutesInADay];
                        shadowPriceNode.ExogenousLoad = new double[Global.Settings.Times.MinutesInADay];
                        shadowPriceNode.ParkAndRideLoad = new double[Global.Settings.Times.MinutesInADay];

                        for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++)
                        {
                            shadowPriceNode.ShadowPriceDifference[i - 1] = Convert.ToDouble(tokens[i]);
                            shadowPriceNode.ShadowPrice[i - 1] = Convert.ToDouble(tokens[Global.Settings.Times.MinutesInADay + i]);
                            shadowPriceNode.ExogenousLoad[i - 1] = Convert.ToDouble(tokens[2 * Global.Settings.Times.MinutesInADay + i]);
                            shadowPriceNode.ParkAndRideLoad[i - 1] = Convert.ToDouble(tokens[3 * Global.Settings.Times.MinutesInADay + i]);
                        }

                        shadowPrices.Add(shadowPriceNode.NodeId, shadowPriceNode);
                    }
                }
                catch (FormatException e)
                {
                    throw new Exception("Format problem in file '" + shadowPriceFile.FullName + "' at line " + reader.LineNumber + " with content '" + line + "'.", e);
                }

            }

            return shadowPrices;
        }
    }
}