// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;

namespace DaySim.Framework.Sampling {
	public interface ISampleItem {
		int ParcelId { get; }

		double ExcludedSize { set; }

		ChoiceProbabilityCalculator.Alternative Alternative { get; }

		double AdjustmentFactor { get; }

        bool Available { get; }

        bool IsChosen { get; }

		void Initialize(DestinationSampler destinationSampler, int destinationParcelId, int destinationParcelSequence, SegmentZone destinationSegmentZone);

		void SetAlternative<TSample>(DestinationSampler destinationSampler, Dictionary<TSample, int> sample, bool isChosenAlternative, 
                AlternativeType alternativeType = AlternativeType.Default, bool skipChoiceProbabilityCalculator = false) where TSample : ISampleItem;
	}

	public enum AlternativeType {
		Default,
		Chosen,
		Usual
	}
}