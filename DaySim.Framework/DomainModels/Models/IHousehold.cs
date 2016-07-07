// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
	public interface IHousehold : IModel {
		double FractionWorkersWithJobsOutsideRegion { get; set; }

		int Size { get; set; }

		int VehiclesAvailable { get; set; }

		int Workers { get; set; }

		int FulltimeWorkers { get; set; }

		int PartTimeWorkers { get; set; }

		int RetiredAdults { get; set; }

		int OtherAdults { get; set; }

		int CollegeStudents { get; set; }

		int HighSchoolStudents { get; set; }

		int KidsBetween5And15 { get; set; }

		int KidsBetween0And4 { get; set; }

		int Income { get; set; }

		int OwnOrRent { get; set; }

		int ResidenceType { get; set; }

		int ResidenceParcelId { get; set; }

		int ResidenceZoneId { get; set; }

		int ResidenceZoneKey { get; set; }

		double ExpansionFactor { get; set; }

		int SampleType { get; set; }
	}
}