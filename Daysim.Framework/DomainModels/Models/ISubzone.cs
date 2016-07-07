// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
	public interface ISubzone {
		int Sequence { get; }

		double Households { get; set; }

		double StudentsK8 { get; set; }

		double StudentsHighSchool { get; set; }

		double StudentsUniversity { get; set; }

		double EmploymentEducation { get; set; }

		double EmploymentFood { get; set; }

		double EmploymentGovernment { get; set; }

		double EmploymentIndustrial { get; set; }

		double EmploymentMedical { get; set; }

		double EmploymentOffice { get; set; }

		double EmploymentRetail { get; set; }

		double EmploymentService { get; set; }

		double EmploymentTotal { get; set; }

		double ParkingOffStreetPaidDailySpaces { get; set; }

		double ParkingOffStreetPaidHourlySpaces { get; set; }

		double MixedUseMeasure { get; set; }

		void SetSize(int purpose, double size);

		double GetSize(int purpose);
	}
}