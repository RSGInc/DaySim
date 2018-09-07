// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface IParcel : IModel, IPoint {
    int Sequence { get; set; }

    double ZoneKey { get; set; }

    double ThousandsSquareLengthUnits { get; set; }

    int LandUseCode { get; set; }

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

    double EmploymentAgricultureConstruction { get; set; }

    double EmploymentTotal { get; set; }

    double ParkingOffStreetPaidDailySpaces { get; set; }

    double ParkingOffStreetPaidHourlySpaces { get; set; }

    double ParkingOffStreetPaidDailyPrice { get; set; }

    double ParkingOffStreetPaidHourlyPrice { get; set; }

    double HouseholdsBuffer1 { get; set; }

    double StudentsK8Buffer1 { get; set; }

    double StudentsHighSchoolBuffer1 { get; set; }

    double StudentsUniversityBuffer1 { get; set; }

    double EmploymentEducationBuffer1 { get; set; }

    double EmploymentFoodBuffer1 { get; set; }

    double EmploymentGovernmentBuffer1 { get; set; }

    double EmploymentIndustrialBuffer1 { get; set; }

    double EmploymentMedicalBuffer1 { get; set; }

    double EmploymentOfficeBuffer1 { get; set; }

    double EmploymentRetailBuffer1 { get; set; }

    double EmploymentServiceBuffer1 { get; set; }

    double EmploymentAgricultureConstructionBuffer1 { get; set; }

    double EmploymentTotalBuffer1 { get; set; }

    double ParkingOffStreetPaidDailySpacesBuffer1 { get; set; }

    double ParkingOffStreetPaidHourlySpacesBuffer1 { get; set; }

    double ParkingOffStreetPaidDailyPriceBuffer1 { get; set; }

    double ParkingOffStreetPaidHourlyPriceBuffer1 { get; set; }

    double NodesSingleLinkBuffer1 { get; set; }

    double NodesThreeLinksBuffer1 { get; set; }

    double NodesFourLinksBuffer1 { get; set; }

    double StopsTransitBuffer1 { get; set; }

    double OpenSpaceType1Buffer1 { get; set; }

    double OpenSpaceType2Buffer1 { get; set; }

    double HouseholdsBuffer2 { get; set; }

    double StudentsK8Buffer2 { get; set; }

    double StudentsHighSchoolBuffer2 { get; set; }

    double StudentsUniversityBuffer2 { get; set; }

    double EmploymentEducationBuffer2 { get; set; }

    double EmploymentFoodBuffer2 { get; set; }

    double EmploymentGovernmentBuffer2 { get; set; }

    double EmploymentIndustrialBuffer2 { get; set; }

    double EmploymentMedicalBuffer2 { get; set; }

    double EmploymentOfficeBuffer2 { get; set; }

    double EmploymentRetailBuffer2 { get; set; }

    double EmploymentServiceBuffer2 { get; set; }

    double EmploymentAgricultureConstructionBuffer2 { get; set; }

    double EmploymentTotalBuffer2 { get; set; }

    double ParkingOffStreetPaidDailySpacesBuffer2 { get; set; }

    double ParkingOffStreetPaidHourlySpacesBuffer2 { get; set; }

    double ParkingOffStreetPaidDailyPriceBuffer2 { get; set; }

    double ParkingOffStreetPaidHourlyPriceBuffer2 { get; set; }

    double NodesSingleLinkBuffer2 { get; set; }

    double NodesThreeLinksBuffer2 { get; set; }

    double NodesFourLinksBuffer2 { get; set; }

    double StopsTransitBuffer2 { get; set; }

    double OpenSpaceType1Buffer2 { get; set; }

    double OpenSpaceType2Buffer2 { get; set; }

    double DistanceToLocalBus { get; set; }

    double DistanceToExpressBus { get; set; }

    double DistanceToFerry { get; set; }

    double DistanceToCommuterRail { get; set; }

    double DistanceToLightRail { get; set; }

    double CircuityRatio_E1 { get; set; }

    double CircuityRatio_E2 { get; set; }

    double CircuityRatio_E3 { get; set; }

    double CircuityRatio_NE1 { get; set; }

    double CircuityRatio_NE2 { get; set; }

    double CircuityRatio_NE3 { get; set; }

    double CircuityRatio_N1 { get; set; }

    double CircuityRatio_N2 { get; set; }

    double CircuityRatio_N3 { get; set; }

    double CircuityRatio_NW1 { get; set; }

    double CircuityRatio_NW2 { get; set; }

    double CircuityRatio_NW3 { get; set; }

    double CircuityRatio_W1 { get; set; }

    double CircuityRatio_W2 { get; set; }

    double CircuityRatio_W3 { get; set; }

    double CircuityRatio_SW1 { get; set; }

    double CircuityRatio_SW2 { get; set; }

    double CircuityRatio_SW3 { get; set; }

    double CircuityRatio_S1 { get; set; }

    double CircuityRatio_S2 { get; set; }

    double CircuityRatio_S3 { get; set; }

    double CircuityRatio_SE1 { get; set; }

    double CircuityRatio_SE2 { get; set; }

    double CircuityRatio_SE3 { get; set; }
  }
}