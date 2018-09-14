// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Exceptions;

namespace DaySim.Framework.Sampling {
  public sealed class DestinationSampler {
    private readonly ChoiceProbabilityCalculator _choiceProbabilityCalculator;
    private readonly SegmentZone[] _segmentZones;

    private readonly IParcel _tourOriginParcel;
    private readonly SegmentZone _tourOriginSegmentZone;

    private readonly IParcel _tripOriginParcel;
    private readonly SegmentZone _tripOriginSegmentZone;

    private readonly IParcel _originParcel;
    private readonly SegmentZone _originSegmentZone;

    private readonly IParcel _excludedParcel;
    private readonly SegmentZone _excludedSegmentZone;

    private readonly IParcel _usualParcel;
    private readonly SegmentZone _usualSegmentZone;

    private readonly IParcel _chosenParcel;
    private readonly SegmentZone _chosenSegmentZone;

    private readonly int _sampleSize;

    private readonly int _segmentZonesIndex;

    private int _alternativeIndex;
    private const int NULL_DESTINATION_WARNING = 2000000;
    private const int NULL_DESTINATION_ERROR = NULL_DESTINATION_WARNING * 10;   //MUST BE A MULTIPLE OF NULL_DESTINATION_WARNING

    private DestinationSampler(ChoiceProbabilityCalculator choiceProbabilityCalculator, int segmentZonesIndex, int sampleSize, IParcel chosenParcel) {
      _choiceProbabilityCalculator = choiceProbabilityCalculator;
      _segmentZonesIndex = segmentZonesIndex;
      _segmentZones = Global.SegmentZones[_segmentZonesIndex];
      _sampleSize = sampleSize;

      if (chosenParcel != null) {
        _chosenParcel = chosenParcel;
        _chosenSegmentZone = _segmentZones[chosenParcel.ZoneId];
      }
    }

    //PCA from JLB
    public DestinationSampler(ChoiceProbabilityCalculator choiceProbabilityCalculator, int segment, int sampleSize, ITourWrapper tour, ITripWrapper trip, IParcel chosenParcel) {
      _choiceProbabilityCalculator = choiceProbabilityCalculator;
      _segmentZones = Global.SegmentZones[segment];
      _sampleSize = sampleSize;

      _tourOriginParcel = tour.OriginParcel;
      _tourOriginSegmentZone = _segmentZones[_tourOriginParcel.ZoneId];

      _tripOriginParcel = trip.OriginParcel;
      _tripOriginSegmentZone = _segmentZones[_tripOriginParcel.ZoneId];

      if (_tourOriginParcel == null || _tripOriginSegmentZone == null) {

      }

      if (chosenParcel != null) {
        _chosenParcel = chosenParcel;
        _chosenSegmentZone = _segmentZones[chosenParcel.ZoneId];
      }

      if (_choiceProbabilityCalculator.ModelIsInEstimationMode && chosenParcel == null) {
        throw new ChosenParcelNotSetInEstimationModeException();
      }
    }


    public DestinationSampler(ChoiceProbabilityCalculator choiceProbabilityCalculator, int segmentZonesIndex, int sampleSize, IParcel chosenParcel, ITourWrapper tour, ITripWrapper trip) : this(choiceProbabilityCalculator, segmentZonesIndex, sampleSize, chosenParcel) {
      _tourOriginParcel = tour.OriginParcel;
      _tourOriginSegmentZone = _segmentZones[_tourOriginParcel.ZoneId];

      _tripOriginParcel = trip.OriginParcel;
      _tripOriginSegmentZone = _segmentZones[_tripOriginParcel.ZoneId];

      if (_tourOriginParcel == null || _tripOriginSegmentZone == null) {

      }

      if (_choiceProbabilityCalculator.ModelIsInEstimationMode && chosenParcel == null) {
        throw new ChosenParcelNotSetInEstimationModeException();
      }
    }


    public DestinationSampler(ChoiceProbabilityCalculator choiceProbabilityCalculator, int segmentZonesIndex, int sampleSize, IParcel chosenParcel, IParcel originParcel) : this(choiceProbabilityCalculator, segmentZonesIndex, sampleSize, chosenParcel) {
      _originParcel = originParcel;
      _originSegmentZone = _segmentZones[originParcel.ZoneId];

      //JLB 20120329 removed these lines because usual work location model doesn't set a sampled dest to chosen when it is the residence location
      //            if (_choiceProbabilityCalculator.ModelIsInEstimationMode && chosenParcel == null) {
      //                throw new ChosenParcelNotSetInEstimationModeException();
      //            }
    }

    public DestinationSampler(ChoiceProbabilityCalculator choiceProbabilityCalculator, int segmentZonesIndex, int sampleSize, IParcel chosenParcel, IParcel originParcel, IParcel excludedParcel, IParcel usualParcel) : this(choiceProbabilityCalculator, segmentZonesIndex, sampleSize, chosenParcel, originParcel) {

      if (excludedParcel != null) {
        _excludedParcel = excludedParcel;
        _excludedSegmentZone = _segmentZones[excludedParcel.ZoneId];
      }

      if (usualParcel != null) {
        _usualParcel = usualParcel;
        _usualSegmentZone = _segmentZones[usualParcel.ZoneId];
      }
      if (_choiceProbabilityCalculator.ModelIsInEstimationMode && chosenParcel == null) {
        throw new ChosenParcelNotSetInEstimationModeException();
      }
    }



    public int SampleTourDestinations(ISamplingUtilities samplingUtilities) {
      TourSampleItem[] sampleItems = new TourSampleItem[_sampleSize];

      for (int i = 0; i < _sampleSize; i++) {
        TourSampleItem sampleItem;

        RandomUniform01 randomUniform01 = new RandomUniform01(samplingUtilities.SeedValues[i]);

        int nullDestinationCounter = 0;
        // draw repeatedly until a sample item is drawn
        do {
          sampleItem = GetDestination<TourSampleItem>(randomUniform01, _originParcel, _originSegmentZone, _excludedParcel, _excludedSegmentZone);
          if (sampleItem != null) {
            sampleItems[i] = sampleItem;
          } else {
            ++nullDestinationCounter;
            if ((nullDestinationCounter % NULL_DESTINATION_WARNING) == 0) {
              OutputDebugForFailedDestination
                  (samplingUtilities, nullDestinationCounter);
              if (nullDestinationCounter >= NULL_DESTINATION_ERROR) {
                throw new Exception(OutputDebugForFailedDestination
                (samplingUtilities, nullDestinationCounter));
              }
            }
          }   //end else got null returned
        } while (sampleItem == null);
      }   //end loop for _sampleSize

      Dictionary<TourSampleItem, int> sample = new Dictionary<TourSampleItem, int>();

      foreach (TourSampleItem sampleItem in sampleItems) {
        // SetAlternative will store the associated alternative for the sample item and calculate the sampling probabilities
        sampleItem.SetAlternative(this, sample, _chosenParcel != null && _chosenParcel.Id == sampleItem.ParcelId);
      }

      //JLB 20120329
      //            if (_choiceProbabilityCalculator.ModelIsInEstimationMode) {
      if (_choiceProbabilityCalculator.ModelIsInEstimationMode && _chosenParcel != null) {
        // creates a tour sample item that represents the chosen alternative
        if (_usualParcel == null || _usualParcel.Id != _chosenParcel.Id) {
          TourSampleItem chosenSampleItem = new TourSampleItem();

          chosenSampleItem.Initialize(this, _chosenParcel.Id, _chosenParcel.Sequence, _chosenSegmentZone);
          chosenSampleItem.SetAlternative(this, sample, true, AlternativeType.Chosen);
        } else if (_usualParcel.Id == _chosenParcel.Id) {
          TourSampleItem chosenSampleItem = new TourSampleItem();

          chosenSampleItem.Initialize(this, _chosenParcel.Id, _chosenParcel.Sequence, _chosenSegmentZone);
          chosenSampleItem.SetAlternative(this, sample, true, AlternativeType.Usual);
        }
        if (_usualParcel != null && _usualParcel.Id != _chosenParcel.Id) {
          TourSampleItem usualSampleItem = new TourSampleItem();

          usualSampleItem.Initialize(this, _usualParcel.Id, _usualParcel.Sequence, _usualSegmentZone);
          usualSampleItem.SetAlternative(this, sample, false, AlternativeType.Usual);
        }
      } else if (_usualParcel != null) {
        // creates a tour sample item that represents the usual alternative
        TourSampleItem usualSampleItem = new TourSampleItem();

        usualSampleItem.Initialize(this, _usualParcel.Id, _usualParcel.Sequence, _usualSegmentZone);
        usualSampleItem.SetAlternative(this, sample, false, AlternativeType.Usual);
      }

      foreach (KeyValuePair<TourSampleItem, int> sampleItem in sample) {
        // calculates adjustment factor for each tour sample item
        if (sampleItem.Key.Probability >= Constants.EPSILON) {
          sampleItem.Key.AdjustmentFactor = -Math.Log(_sampleSize * sampleItem.Key.Probability / sampleItem.Value);
        }

        samplingUtilities.SetUtilities(sampleItem.Key, sampleItem.Value);
      }

      return sample.Count;
    }

    private string OutputDebugForFailedDestination(ISamplingUtilities samplingUtilities, int nullDestinationCounter) {
      string errorMessage = "Thread-" + Thread.CurrentThread.Name + " GetDestination<TourSampleItem> returned null " + nullDestinationCounter + ""
          + " times. DestinationSampler object: " + ObjectDumper.Dump(this, /* depth */ 2, /* maxEnumeratedItems */ 3) + "\n\nsamplingUtilities: " + ObjectDumper.Dump(samplingUtilities, /* depth */ 2, /* maxEnumeratedItems */ 3);
      Global.PrintFile.WriteLine(errorMessage, /* writeToConsole */ true);
      return errorMessage;
    }

    public Dictionary<TourSampleItem, int> SampleAndReturnTourDestinations(ISamplingUtilities samplingUtilities) {
      TourSampleItem[] sampleItems = new TourSampleItem[_sampleSize];

      for (int i = 0; i < _sampleSize; i++) {
        TourSampleItem sampleItem;

        RandomUniform01 randomUniform01 = new RandomUniform01(samplingUtilities.SeedValues[i]);
        int nullDestinationCounter = 0;
        // draw repeatedly until a sample item is drawn
        do {
          sampleItem = GetDestination<TourSampleItem>(randomUniform01, _originParcel, _originSegmentZone, _excludedParcel, _excludedSegmentZone);
          if (sampleItem != null) {
            if ((nullDestinationCounter % NULL_DESTINATION_WARNING) == 0) {
              OutputDebugForFailedDestination
                  (samplingUtilities, nullDestinationCounter);
              if (nullDestinationCounter >= NULL_DESTINATION_ERROR) {
                throw new Exception(OutputDebugForFailedDestination
                (samplingUtilities, nullDestinationCounter));
              }
            }
          }   //end else got null returned
        } while (sampleItem == null);
      }   //end loop for _sampleSize
      Dictionary<TourSampleItem, int> sample = new Dictionary<TourSampleItem, int>();

      bool skipChoiceProbabilityCalculator = true;

      foreach (TourSampleItem sampleItem in sampleItems) {
        // SetAlternative will store the associated alternative for the sample item and calculate the sampling probabilities
        sampleItem.SetAlternative(this, sample, _chosenParcel != null && _chosenParcel.Id == sampleItem.ParcelId, AlternativeType.Default, skipChoiceProbabilityCalculator);
      }

      //JLB 20120329
      //            if (_choiceProbabilityCalculator.ModelIsInEstimationMode) {
      if (_choiceProbabilityCalculator.ModelIsInEstimationMode && _chosenParcel != null) {
        // creates a tour sample item that represents the chosen alternative
        if (_usualParcel == null || _usualParcel.Id != _chosenParcel.Id) {
          TourSampleItem chosenSampleItem = new TourSampleItem();
          chosenSampleItem.Initialize(this, _chosenParcel.Id, _chosenParcel.Sequence, _chosenSegmentZone);
          chosenSampleItem.SetAlternative(this, sample, true, AlternativeType.Chosen, skipChoiceProbabilityCalculator);

        } else if (_usualParcel.Id == _chosenParcel.Id) {
          TourSampleItem chosenSampleItem = new TourSampleItem();

          chosenSampleItem.Initialize(this, _chosenParcel.Id, _chosenParcel.Sequence, _chosenSegmentZone);
          chosenSampleItem.SetAlternative(this, sample, true, AlternativeType.Usual, skipChoiceProbabilityCalculator);
        }
        if (_usualParcel != null && _usualParcel.Id != _chosenParcel.Id) {
          TourSampleItem usualSampleItem = new TourSampleItem();

          usualSampleItem.Initialize(this, _usualParcel.Id, _usualParcel.Sequence, _usualSegmentZone);
          usualSampleItem.SetAlternative(this, sample, false, AlternativeType.Usual, skipChoiceProbabilityCalculator);
        }
      } else if (_usualParcel != null) {
        // creates a tour sample item that represents the usual alternative
        TourSampleItem usualSampleItem = new TourSampleItem();

        usualSampleItem.Initialize(this, _usualParcel.Id, _usualParcel.Sequence, _usualSegmentZone);
        usualSampleItem.SetAlternative(this, sample, false, AlternativeType.Usual, skipChoiceProbabilityCalculator);
      }

      foreach (KeyValuePair<TourSampleItem, int> sampleItem in sample) {
        // calculates adjustment factor for each tour sample item
        if (sampleItem.Key.Probability >= Constants.EPSILON) {
          sampleItem.Key.AdjustmentFactor = -Math.Log(_sampleSize * sampleItem.Key.Probability / sampleItem.Value);
        }

        //not called in this version 
        //samplingUtilities.SetUtilities(sampleItem.Key, sampleItem.Value);
      }

      return sample;
    }


    public int SampleIntermediateStopDestinations(ISamplingUtilities samplingUtilities) {
      IntermediateStopSampleItem[] sampleItems = new IntermediateStopSampleItem[_sampleSize];

      int sampleSize1 = _sampleSize / 2;
      int sampleSize2 = _sampleSize - sampleSize1;

      for (int i = 0; i < _sampleSize; i++) {
        IntermediateStopSampleItem sampleItem;

        RandomUniform01 randomUniform01 = new RandomUniform01(samplingUtilities.SeedValues[i]);
        int count = 0;
        // draw repeatedly until a sample item is drawn
        do {
          sampleItem =
              i < sampleSize1
                  ? GetDestination<IntermediateStopSampleItem>(randomUniform01, _tourOriginParcel, _tourOriginSegmentZone, _tripOriginParcel, _tripOriginSegmentZone)
                  : GetDestination<IntermediateStopSampleItem>(randomUniform01, _tripOriginParcel, _tripOriginSegmentZone, _tourOriginParcel, _tourOriginSegmentZone);

          if (sampleItem != null) {
            sampleItems[i] = sampleItem;
          }
          count++;
        } while (sampleItem == null && count < 1000);

        if (sampleItem == null) {
          return -1;
        }
      }

      Dictionary<IntermediateStopSampleItem, int> sample = new Dictionary<IntermediateStopSampleItem, int>();

      foreach (IntermediateStopSampleItem sampleItem in sampleItems) {
        // SetAlternative will store the associated alternative for the sample item and calculate the sampling probabilities
        sampleItem.SetAlternative(this, sample, _chosenParcel != null && _chosenParcel.Id == sampleItem.ParcelId);
      }

      if (_choiceProbabilityCalculator.ModelIsInEstimationMode && _chosenParcel != null) {
        // creates a intermediate stop sample item that represents the chosen alternative
        IntermediateStopSampleItem chosenSampleItem = new IntermediateStopSampleItem();

        chosenSampleItem.Initialize(this, _chosenParcel.Id, _chosenParcel.Sequence, _chosenSegmentZone);
        chosenSampleItem.SetAlternative(this, sample, true, AlternativeType.Chosen);
      }

      foreach (KeyValuePair<IntermediateStopSampleItem, int> sampleItem in sample) {
        // calculates adjustment factor for each intermediate stop sample item
        if (sampleItem.Key.Probability1 < Constants.EPSILON && sampleItem.Key.Probability2 < Constants.EPSILON) {
          sampleItem.Key.Probability1 = 2 * Constants.EPSILON;
          sampleItem.Key.Probability2 = 2 * Constants.EPSILON;
        }

        sampleItem.Key.AdjustmentFactor = -Math.Log((sampleSize1 * sampleItem.Key.Probability1 + sampleSize2 * sampleItem.Key.Probability2) / sampleItem.Value);

        samplingUtilities.SetUtilities(sampleItem.Key, sampleItem.Value);
      }

      return sample.Count;
    }

    private TSampleItem GetDestination<TSampleItem>(RandomUniform01 randomUniform01, IParcel originParcel, SegmentZone originSegmentZone, IParcel excludedParcel, SegmentZone excludedSegmentZone) where TSampleItem : ISampleItem, new() {
      int destinationZoneId = 0;
      SegmentZone destinationSegmentZone = null;

      int destinationParcelSequence = 0;
      int destinationParcelId = 0;
      bool destinationParcelIsValid = false;

      double random = randomUniform01.Uniform01() * originSegmentZone.TotalWeight;

      if (random > .001) {
        double total = 0D;

        // draw the zone
        foreach (WeightSegmentItem weight in originSegmentZone.RankedWeights) {
          total += weight.Value;

          if (total <= random) {
            continue;
          }

          destinationZoneId = weight.Id;
          destinationSegmentZone = _segmentZones[weight.Id];

          break;
        }
      }

      if (destinationSegmentZone != null && destinationSegmentZone.Key == 0) {

      }

      if (destinationSegmentZone == null) {
        destinationZoneId = originParcel.ZoneId;
        destinationSegmentZone = originSegmentZone;
      }

      double excludedSize = 0D;

      if (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Parcel) {
        if (destinationZoneId == originParcel.ZoneId) {
          excludedSize += originSegmentZone.GetSize(originParcel.Sequence);
        }

        if (excludedParcel != null && destinationZoneId == excludedParcel.ZoneId) {
          excludedSize += excludedSegmentZone.GetSize(excludedParcel.Sequence);
        }
      }
      if (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Zone) {
        if (destinationSegmentZone.Key == 0) {

        }
        destinationParcelId = destinationSegmentZone.Key;
        destinationParcelSequence = 0;
        destinationParcelIsValid = true;
      } else {
        //                if (destinationSegmentZone.TotalSize - excludedSize < Constants.EPSILON) {
        //                    Console.WriteLine(originSegmentZone.Id);
        //                    Console.WriteLine(originSegmentZone.TotalWeight);
        //                    Console.WriteLine(originSegmentZone.TotalSize);
        //                    Console.WriteLine(destinationSegmentZone.Id);
        //                    Console.WriteLine(destinationSegmentZone.TotalWeight);
        //                    Console.WriteLine(destinationSegmentZone.TotalSize);
        //                    Console.WriteLine(excludedSize);
        //                }

        random = randomUniform01.Uniform01() * (destinationSegmentZone.TotalSize - excludedSize);

        if (random > .001) {
          double total = 0D;

          // draw the parcel within zone
          foreach (SizeSegmentItem size in destinationSegmentZone.RankedSizes) {
            if (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.MicroZone ||
                 (originParcel.Id != size.Id && (excludedParcel == null || excludedParcel.Id != size.Id))) {
              total += size.Value;
            }

            if (total <= random) {
              continue;
            }

            // don't include the drawn parcel if the parcel has no size
            if (size.Value >= Global.Configuration.MinParcelSize) {
              destinationParcelId = size.Id;
              destinationParcelSequence = size.Sequence;
              destinationParcelIsValid = true;
            }

            break;
          }
        }
      }

      if (Global.Configuration.DestinationScale != Global.Settings.DestinationScales.Zone && !destinationParcelIsValid) {
        return default(TSampleItem);
      }

      TSampleItem sampleItem = new TSampleItem();

      sampleItem.Initialize(this, destinationParcelId, destinationParcelSequence, destinationSegmentZone);

      if (destinationParcelIsValid) {
        sampleItem.ExcludedSize = excludedSize;
      }

      return sampleItem;
    }

    //private sealed class TourSampleItem : SampleItem {
    public class TourSampleItem : SampleItem {
      private double _weightFromOrigin;
      private double _totalWeightFromOrigin;

      public double Probability { get; private set; }
      //            public bool Available { get; set; }
      //            public bool IsChosen { get; set; }

      public override void Initialize(DestinationSampler destinationSampler, int destinationParcelId, int destinationParcelSequence, SegmentZone destinationSegmentZone) {
        _weightFromOrigin = destinationSampler._originSegmentZone.GetWeight(destinationSegmentZone.Id);
        _totalWeightFromOrigin = destinationSampler._originSegmentZone.TotalWeight;

        TotalWeightFromDestination = destinationSegmentZone.TotalWeight;

        ParcelId = destinationParcelId;

        //if (Global.Configuration.DestinationScale == Constants.DestinationScale.ZONE) {
        //    return;
        //}

        SizeFromDestination = destinationSegmentZone.GetSize(destinationParcelSequence);
        TotalSizeFromDestination = destinationSegmentZone.TotalSize;
      }

      protected override void SetProbability(DestinationSampler destinationSampler, bool skipChoicePropabilityCalculator = false) {
        double zoneProbability = _weightFromOrigin / Math.Max(_totalWeightFromOrigin, Constants.EPSILON);

        // set chosen alternative availability if model is in estimation mode and the sample item is the chosen alternative
        //JLB 20120329 replaced followign to allow for no chosen alt in destinationSampler when in estimation mode (usual work location model)
        //                var setAvailability = destinationSampler._choiceProbabilityCalculator.ModelIsInEstimationMode && ParcelId == destinationSampler._chosenParcel.Id;
        bool setAvailability = destinationSampler._choiceProbabilityCalculator.ModelIsInEstimationMode && destinationSampler._chosenParcel != null && ParcelId == destinationSampler._chosenParcel.Id;

        if (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Zone) {
          Probability = zoneProbability;

          // set chosen alternative availability if model is in estimation mode and the sample item is the chosen alternative
          if (setAvailability) {
            Available = Probability > 0 && _weightFromOrigin > Global.Configuration.MinParcelSize;
            if (!skipChoicePropabilityCalculator) { Alternative.Available = Available; }
          }

          return;
        }

        // excludedSize will be 0 when in model is in estimation mode and the sample item is the chosen alternative
        double excludedSize = setAvailability ? 0 : ExcludedSize;
        double parcelProbability = SizeFromDestination / Math.Max(TotalSizeFromDestination - excludedSize, Constants.EPSILON);

        Probability = zoneProbability * parcelProbability;

        // set chosen alternative availability if model is in estimation mode and the sample item is the chosen alternative
        if (setAvailability) {
          Available = Probability > 0 && SizeFromDestination >= Global.Configuration.MinParcelSize;
          if (!skipChoicePropabilityCalculator) { Alternative.Available = Available; }

        }
      }
    }

    private sealed class IntermediateStopSampleItem : SampleItem {
      private double _tourWeightFromOrigin;
      private double _totalTourWeightFromOrigin;
      private double _tripWeightFromOrigin;
      private double _totalTripWeightFromOrigin;

      public double Probability1 { get; set; }

      public double Probability2 { get; set; }

      public override void Initialize(DestinationSampler destinationSampler, int destinationParcelId, int destinationParcelSequence, SegmentZone destinationSegmentZone) {
        _tourWeightFromOrigin = destinationSampler._tourOriginSegmentZone.GetWeight(destinationSegmentZone.Id);
        _totalTourWeightFromOrigin = destinationSampler._tourOriginSegmentZone.TotalWeight;
        _tripWeightFromOrigin = destinationSampler._tripOriginSegmentZone.GetWeight(destinationSegmentZone.Id);
        _totalTripWeightFromOrigin = destinationSampler._tripOriginSegmentZone.TotalWeight;

        TotalWeightFromDestination = destinationSegmentZone.TotalWeight;

        //if (Global.Configuration.DestinationScale == Constants.DestinationScale.ZONE) {
        //    return;
        //}

        ParcelId = destinationParcelId;
        SizeFromDestination = destinationSegmentZone.GetSize(destinationParcelSequence);
        TotalSizeFromDestination = destinationSegmentZone.TotalSize;
      }

      protected override void SetProbability(DestinationSampler destinationSampler, bool skipChoicePropabilityCalculator = false) {
        double zoneProbability1 = _tourWeightFromOrigin / Math.Max(_totalTourWeightFromOrigin, Constants.EPSILON);
        double zoneProbability2 = _tripWeightFromOrigin / Math.Max(_totalTripWeightFromOrigin, Constants.EPSILON);

        // set chosen alternative availability if model is in estimation mode and the sample item is the chosen alternative
        bool setAvailability = destinationSampler._choiceProbabilityCalculator.ModelIsInEstimationMode && ParcelId == destinationSampler._chosenParcel.Id;

        if (Global.Configuration.DestinationScale == Global.Settings.DestinationScales.Zone) {
          Probability1 = zoneProbability1;
          Probability2 = zoneProbability2;

          // set chosen alternative availability if model is in estimation mode and the sample item is the chosen alternative
          if (setAvailability) {
            Available = (Probability1 > 0 || Probability2 > 0) && _tourWeightFromOrigin > Global.Configuration.MinParcelSize;
            if (!skipChoicePropabilityCalculator) {
              Alternative.Available = Available;
            }
          }

          return;
        }

        // excludedSize will be 0 when in model is in estimation mode and the sample item is the chosen alternative
        double excludedSize = setAvailability ? 0 : ExcludedSize;
        double parcelProbability = SizeFromDestination / Math.Max(TotalSizeFromDestination - excludedSize, Constants.EPSILON);

        Probability1 = zoneProbability1 * parcelProbability;
        Probability2 = zoneProbability2 * parcelProbability;

        // set chosen alternative availability if model is in estimation mode and the sample item is the chosen alternative
        if (setAvailability) {
          Available = (Probability1 > 0 || Probability2 > 0) && SizeFromDestination >= Global.Configuration.MinParcelSize;
          if (!skipChoicePropabilityCalculator) {
            Alternative.Available = Available;
          }
        }
      }
    }

    //private abstract class SampleItem : ISampleItem {
    public abstract class SampleItem : ISampleItem {
      public int ParcelId { get; protected set; }

      protected double SizeFromDestination { get; set; }

      protected double TotalSizeFromDestination { get; set; }

      public double ExcludedSize { protected get; set; }

      protected double TotalWeightFromDestination { private get; set; }

      public ChoiceProbabilityCalculator.Alternative Alternative { get; private set; }

      public double AdjustmentFactor { get; set; }

      public bool Available { get; set; }

      public bool IsChosen { get; set; }

      public abstract void Initialize(DestinationSampler destinationSampler, int destinationParcelId, int destinationParcelSequence, SegmentZone destinationSegmentZone);

      public void SetAlternative<TSample>(DestinationSampler destinationSampler, Dictionary<TSample, int> sample,
                                bool isChosenAlternative, AlternativeType alternativeType = AlternativeType.Default, bool skipChoiceProbabilityCalculator = false) where TSample : ISampleItem {
        TSample key = (TSample)(object)this;

        Available = true;
        IsChosen = isChosenAlternative;

        if (sample.ContainsKey(key)) {
          sample[key] += 1;
        } else {
          int alternativeIndex;

          switch (alternativeType) {
            case AlternativeType.Chosen:
              if (sample.Count == destinationSampler._sampleSize) {
                sample.Remove(sample.Last().Key);
              }

              alternativeIndex = sample.Count;

              break;
            case AlternativeType.Usual:
              //                            alternativeIndex = sample.Count;
              alternativeIndex = destinationSampler._sampleSize;

              break;
            default:
              alternativeIndex = destinationSampler._alternativeIndex++;

              break;
          }

          sample.Add(key, 1);

          if (!skipChoiceProbabilityCalculator) {
            Alternative = destinationSampler._choiceProbabilityCalculator.GetAlternative(alternativeIndex, true, isChosenAlternative);
          }

          SetProbability(destinationSampler, skipChoiceProbabilityCalculator);
        }
      }

      protected abstract void SetProbability(DestinationSampler destinationSampler, bool skipChoiceProbabilityCalculator = false);

      private bool Equals(ISampleItem other) {
        if (ReferenceEquals(null, other)) {
          return false;
        }

        if (ReferenceEquals(this, other)) {
          return true;
        }

        return other.ParcelId == ParcelId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as SampleItem);
      }

      public override int GetHashCode() {
        return ParcelId;
      }

      public override string ToString() {
        return string.Format("Parcel ID: {0}, Size From Destination: {1}, Total Size From Destination: {2}, Excluded Size : {3}, Total Weight From Destination: {4}, Adjustment Factor: {5}", ParcelId, SizeFromDestination, TotalSizeFromDestination, ExcludedSize, TotalWeightFromDestination, AdjustmentFactor);
      }
    }
  }
}
