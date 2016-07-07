// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.ShadowPricing;

namespace DaySim.Framework.DomainModels.Wrappers {
	public interface IParcelWrapper : IParcel {
		#region flags/choice model/etc. properties

		double ShadowPriceForEmployment { get; set; }

		double ShadowPriceForStudentsK12 { get; set; }

		double ShadowPriceForStudentsUniversity { get; set; }

		double ExternalEmploymentTotal { get; set; }

		double EmploymentDifference { get; set; }

		double EmploymentPrediction { get; set; }

		double ExternalStudentsK12 { get; set; }

		double StudentsK12Difference { get; set; }

		double StudentsK12Prediction { get; set; }

		double ExternalStudentsUniversity { get; set; }

		double StudentsUniversityDifference { get; set; }

		double StudentsUniversityPrediction { get; set; }

		int District { get; set; }

		int FirstPositionInStopAreaDistanceArray { get; set; }

		int LastPositionInStopAreaDistanceArray { get; set; }

		bool StopAreaDistanceArrayPositionsSet { get; set; }

		#endregion

		#region wrapper methods

		int GetLandUseCode19();

		double GetStudentsK12();

		double GetDistanceToTransit();

//		void SetFirstStopAreaDistanceIndex();

//		void SetLastStopAreaDistanceIndex();

		double NetIntersectionDensity1();

		double NetIntersectionDensity2();

		double OpenSpaceDensity1();

		double OpenSpaceDensity2();

		double OpenSpaceMillionSqFtBuffer1();

		double OpenSpaceMillionSqFtBuffer2();

		double RetailEmploymentDensity1();

		double RetailEmploymentDensity2();

		double ServiceEmploymentDensity1();

		double ServiceEmploymentDensity2();

		double OfficeEmploymentDensity1();

		double OfficeEmploymentDensity2();

		double TotalEmploymentDensity1();

		double TotalEmploymentDensity2();

		double StudentEnrolmentDensity1();

		double StudentEnrolmentDensity2();

		double HouseholdDensity1();

		double HouseholdDensity2();

		double MixedUse2Index1();

		double MixedUse2Index2();

		double MixedUse3Index2();

		double MixedUse4Index1();

		double MixedUse4Index2();

		int TransitAccessSegment();

		double ParcelParkingPerTotalEmployment();

		double ParkingHourlyEmploymentCommercialMixInParcel();

		double ParkingHourlyEmploymentCommercialMixBuffer1();

		double ParkingDailyEmploymentTotalMixInParcel();

		double ParkingDailyEmploymentTotalMixBuffer1();

		double ParcelParkingPerFoodRetailServiceMedicalEmployment();

		double ZoneParkingPerTotalEmploymentAndK12UniversityStudents(IZoneTotals zoneTotals, double millionsSquareLengthUnits);

		double ZoneParkingPerFoodRetailServiceMedicalEmployment(double millionsSquareLengthUnits);

		double C34RatioBuffer1();

		double ParcelHouseholdsPerRetailServiceEmploymentBuffer2();

		double ParcelHouseholdsPerRetailServiceFoodEmploymentBuffer2();

		double IntersectionDensity34Buffer2();

		double IntersectionDensity34Minus1Buffer2();

		double ParkingCostBuffer1(double parkingDuration);

		double FoodRetailServiceMedicalLogBuffer1();

		double K8HighSchoolQtrMileLogBuffer1();

		int UsualWorkParcelFlag(int usualWorkParcelId);

		int NotUsualWorkParcelFlag(int usualWorkParcelId);

		int RuralFlag();

		void SetShadowPricing(Dictionary<int, IZone> zones, Dictionary<int, IShadowPriceParcel> shadowPrices);

        void AddEmploymentPrediction(double employmentPrediction);

        void AddStudentsUniversityPrediction(double studentsUniversityPrediction);

        void AddStudentsK12Prediction(double studentsK12Prediction);

		double NodeToNodeDistance(IParcelWrapper destination, int batch);

		double CircuityDistance(IParcelWrapper destination);

//		void SetFirstAndLastStopAreaDistanceIndexes();

		#endregion
	}
}