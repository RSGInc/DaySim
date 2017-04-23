// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.ChoiceModels;
using DaySim.Framework.Core;
using System;
using System.IO;

namespace DaySim.DestinationParkingShadowPricing { 
    public static class DestinationParkingShadowPriceCalculator    {
        public static void CalculateAndWriteShadowPrices() {
            if (!Global.DestinationParkingNodeIsEnabled || !Global.Configuration.ShouldUseDestinationParkingShadowPricing || Global.Configuration.IsInEstimationMode) {
                return;
            }

            //issue #57 https://github.com/RSGInc/DaySim/issues/57 Must keep safe copy of shadow prices files before overwriting
            if (File.Exists(Global.DestinationParkingShadowPricesPath)) {
                if (File.Exists(Global.ArchiveDestinationParkingShadowPricesPath)) {
                    File.Delete(Global.ArchiveDestinationParkingShadowPricesPath);
                }
                File.Move(Global.DestinationParkingShadowPricesPath, Global.ArchiveDestinationParkingShadowPricesPath);
            }

            using (var shadowPriceWriter = new DestinationParkingShadowPriceWriter(new FileInfo(Global.DestinationParkingShadowPricesPath))) {
                foreach (var node in ChoiceModelFactory.DestinationParkingNodeDao.Nodes) {
                    var capacity = node.Capacity;
                    for (var i = 1; i < Global.Settings.Times.MinutesInADay; i++) {
                        double shadowPrice;
                        double maxLoad = 0;
                        for (var j = Math.Max(i - (Global.Configuration.DestinationParkingShadowPriceTimeSpread), 1); j < Math.Min(i + (Global.Configuration.DestinationParkingShadowPriceTimeSpread), Global.Settings.Times.MinutesInADay); j++) {
                            maxLoad = node.ParkingLoad[i - 1] > maxLoad ? node.ParkingLoad[i - 1] : maxLoad;
                        }
                        DetermineShadowPrice(node.ShadowPrice[i - 1], node.ExogenousLoad[i - 1] + maxLoad, capacity, out shadowPrice);
                        node.ShadowPriceDifference[i - 1] = shadowPrice - node.ShadowPrice[i - 1];
                        node.ShadowPrice[i - 1] = shadowPrice;
                    }
                    // write shadow prices
                    shadowPriceWriter.Write(node);
                }
            }
        }

        private static void DetermineShadowPrice(double previousShadowPrice, double maxLoad, int capacity, out double shadowPrice) {
            //            shadowPrice = Math.Min(0, previousShadowPrice + Math.Log(Math.Max(capacity, .01) / Math.Max(prediction, .01)));
            shadowPrice = (capacity > 0 && maxLoad > 0) ?
                (1 - Global.Configuration.DestinationParkingShadowPriceStepSize) * previousShadowPrice
                    + Global.Configuration.DestinationParkingShadowPriceStepSize * Global.Configuration.DestinationParkingShadowPriceMaximumPenalty * alglib.poissondistr.poissoncdistribution(capacity, maxLoad)
                    : (1 - Global.Configuration.DestinationParkingShadowPriceStepSize) * previousShadowPrice;

        }
    }
}