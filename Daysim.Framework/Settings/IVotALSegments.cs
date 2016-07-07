// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Settings {
	public interface IVotALSegments {
		int TotalVotALSegments { get; }

		int Low { get; }
		
		int Medium { get; }
		
		int High { get; }
		
		int IncomeLowMedium { get; }
		
		int IncomeMediumHigh { get; }
		
		double VotLowMedium { get; }
		
		double VotMediumHigh { get; }
		
		double TimeCoefficient { get; }
		
		double CostCoefficientLow { get; }
		
		double CostCoefficientMedium { get; }
		
		double CostCoefficientHigh { get; }
	}
}
