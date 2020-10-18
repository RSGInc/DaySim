// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.IO;
using DaySim.ChoiceModels;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;

namespace DaySim.DestinationParkingShadowPricing {
  public static class DestinationParkingShadowPriceCalculator {
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

      using (DestinationParkingShadowPriceWriter shadowPriceWriter = new DestinationParkingShadowPriceWriter(new FileInfo(Global.DestinationParkingShadowPricesPath))) {
        foreach (Framework.DomainModels.Wrappers.IDestinationParkingNodeWrapper node in ChoiceModelFactory.DestinationParkingNodeDao.Nodes) {
          //int capacity = node.Capacity;
          IActumDestinationParkingNodeWrapper pnode = (IActumDestinationParkingNodeWrapper)node;
          for (int i = 1; i < Global.Settings.Times.MinutesInADay; i++) {
            double maxLoad = 0;
            for (int j = Math.Max(i - (Global.Configuration.DestinationParkingShadowPriceTimeSpread), 1); j < Math.Min(i + (Global.Configuration.DestinationParkingShadowPriceTimeSpread), Global.Settings.Times.MinutesInADay); j++) {
              maxLoad = node.ParkingLoad[i - 1] > maxLoad ? node.ParkingLoad[i - 1] : maxLoad;
            }
            double effectiveCapacity = Math.Max(1.0, pnode.CalculateEffectiveCapacity(i - 1));
            DetermineShadowPrice(node.ShadowPrice[i - 1], node.ExogenousLoad[i - 1] + maxLoad, effectiveCapacity, out double shadowPrice);
            node.ShadowPriceDifference[i - 1] = shadowPrice - node.ShadowPrice[i - 1];
            node.ShadowPrice[i - 1] = shadowPrice;
          }
          // write shadow prices
          shadowPriceWriter.Write(node);
        }
      }
    }

    private static void DetermineShadowPrice(double previousShadowPrice, double maxLoad, double effectiveCapacity, out double shadowPrice) {
      //            shadowPrice = Math.Min(0, previousShadowPrice + Math.Log(Math.Max(capacity, .01) / Math.Max(prediction, .01)));
      //shadowPrice = (capacity > 0 && maxLoad > 0) ?
      //    (1 - Global.Configuration.DestinationParkingShadowPriceStepSize) * previousShadowPrice
      //        + Global.Configuration.DestinationParkingShadowPriceStepSize * Global.Configuration.DestinationParkingShadowPriceMaximumPenalty * alglib.poissondistr.poissoncdistribution(capacity, maxLoad)
      //        : (1 - Global.Configuration.DestinationParkingShadowPriceStepSize) * previousShadowPrice;
      shadowPrice = (effectiveCapacity > Constants.EPSILON ) ?
          (1 - Global.Configuration.DestinationParkingShadowPriceStepSize) * previousShadowPrice
             + Global.Configuration.DestinationParkingShadowPriceStepSize * 
               (Global.Configuration.DestinationParkingShadowPriceAlpha * 
               Math.Exp(Global.Configuration.DestinationParkingShadowPriceBeta * Math.Min(Global.Configuration.DestinationParkingShadowPriceMaximumPenalty,(maxLoad/effectiveCapacity))))              
               : (1 - Global.Configuration.DestinationParkingShadowPriceStepSize) * previousShadowPrice;
    }
  }
}
