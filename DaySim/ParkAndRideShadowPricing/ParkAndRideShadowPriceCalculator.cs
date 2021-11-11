﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.IO;
using DaySim.ChoiceModels;
using DaySim.Framework.Core;

namespace DaySim.ParkAndRideShadowPricing {
  public static class ParkAndRideShadowPriceCalculator {
    public static void CalculateAndWriteShadowPrices() {
      if (!Global.ParkAndRideNodeIsEnabled || !Global.Configuration.ShouldUseParkAndRideShadowPricing || Global.Configuration.IsInEstimationMode) {
        return;
      }

      //issue #57 https://github.com/RSGInc/DaySim/issues/57 Must keep safe copy of shadow prices files before overwriting
      if (File.Exists(Global.ParkAndRideShadowPricesPath)) {
        if (File.Exists(Global.ArchiveParkAndRideShadowPricesPath)) {
          File.Delete(Global.ArchiveParkAndRideShadowPricesPath);
        }
        File.Move(Global.ParkAndRideShadowPricesPath, Global.ArchiveParkAndRideShadowPricesPath);
      }

      using (ParkAndRideShadowPriceWriter shadowPriceWriter = new ParkAndRideShadowPriceWriter(new FileInfo(Global.ParkAndRideShadowPricesPath))) {
        foreach (Framework.DomainModels.Wrappers.IParkAndRideNodeWrapper node in ChoiceModelFactory.ParkAndRideNodeDao.Nodes) {
          int capacity = node.Capacity;
          for (int i = 1; i < Global.Settings.Times.MinutesInADay; i++) {
            double maxLoad = 0;
            for (int j = Math.Max(i - (Global.Configuration.ParkAndRideShadowPriceTimeSpread), 1); j < Math.Min(i + (Global.Configuration.ParkAndRideShadowPriceTimeSpread), Global.Settings.Times.MinutesInADay); j++) {
              maxLoad = node.ParkAndRideLoad[j - 1] > maxLoad ? node.ParkAndRideLoad[j - 1] : maxLoad;
            }
            DetermineShadowPrice(node.ShadowPrice[i - 1], node.ExogenousLoad[i - 1] + maxLoad, capacity, out double shadowPrice);
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
          (1 - Global.Configuration.ParkAndRideShadowPriceStepSize) * previousShadowPrice
              + Global.Configuration.ParkAndRideShadowPriceStepSize * Global.Configuration.ParkAndRideShadowPriceMaximumPenalty * alglib.poissondistr.poissoncdistribution(capacity, maxLoad)
              : (1 - Global.Configuration.ParkAndRideShadowPriceStepSize) * previousShadowPrice;

    }
  }
}
