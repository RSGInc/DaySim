// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.ShadowPricing {
	public interface IShadowPriceParcel {
		int ParcelId { get; set; }

		double EmploymentDifference { get; set; }

		double ShadowPriceForEmployment { get; set; }

		double StudentsK12Difference { get; set; }

		double ShadowPriceForStudentsK12 { get; set; }

		double StudentsUniversityDifference { get; set; }

		double ShadowPriceForStudentsUniversity { get; set; }
	}
}
