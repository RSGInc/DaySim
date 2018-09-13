// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Actum.Models {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  [Factory(Factory.PersistenceFactory, Category = Category.Model, DataType = DataType.Actum)]
  public sealed class Parcel : IParcel {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("sequence")]
    public int Sequence { get; set; }

    [ColumnName("zone_id")]
    public int ZoneId { get; set; }

    [ColumnName("taz_p")]
    public double ZoneKey { get; set; }

    [ColumnName("xcoord_p")]
    public int XCoordinate { get; set; }

    [ColumnName("ycoord_p")]
    public int YCoordinate { get; set; }

    [ColumnName("sqft_p")]
    public double ThousandsSquareLengthUnits { get; set; }

    [ColumnName("distr_p")]
    public int District { get; set; }

    [ColumnName("lutype_p")]
    public int LandUseCode { get; set; }

    [ColumnName("hh_p")]
    public double Households { get; set; }

    [ColumnName("stugrd_p")]
    public double StudentsK8 { get; set; }

    [ColumnName("stuhgh_p")]
    public double StudentsHighSchool { get; set; }

    [ColumnName("stuuni_p")]
    public double StudentsUniversity { get; set; }

    [ColumnName("empedu_p")]
    public double EmploymentEducation { get; set; }

    [ColumnName("empfoo_p")]
    public double EmploymentFood { get; set; }

    [ColumnName("empgov_p")]
    public double EmploymentGovernment { get; set; }

    [ColumnName("empind_p")]
    public double EmploymentIndustrial { get; set; }

    [ColumnName("empmed_p")]
    public double EmploymentMedical { get; set; }

    [ColumnName("empofc_p")]
    public double EmploymentOffice { get; set; }

    [ColumnName("empret_p")]
    public double EmploymentRetail { get; set; }

    [ColumnName("empsvc_p")]
    public double EmploymentService { get; set; }

    [ColumnName("empoth_p")]
    public double EmploymentAgricultureConstruction { get; set; }

    [ColumnName("emptot_p")]
    public double EmploymentTotal { get; set; }

    [ColumnName("parkdy_p")]
    public double ParkingOffStreetPaidDailySpaces { get; set; }

    [ColumnName("parkhr_p")]
    public double ParkingOffStreetPaidHourlySpaces { get; set; }

    [ColumnName("ppricdyp")]
    public double ParkingOffStreetPaidDailyPrice { get; set; }

    [ColumnName("pprichrp")]
    public double ParkingOffStreetPaidHourlyPrice { get; set; }

    /*		[ColumnName("Circ_E1")]
            public double ParkingCostPerHour8_18 { get; set; }

            [ColumnName("Circ_E2")]
            public double ParkingCostPerHour18_23 { get; set; }

            [ColumnName("Circ_E3")]
            public double ParkingCostPerHour23_08 { get; set; }

            [ColumnName("Circ_NE1")]
            public double ResidentAnnualParkingCost { get; set; }

            [ColumnName("Circ_NE2")]
            public double ParkingSearchTime21_05 { get; set; }

            [ColumnName("Circ_NE3")]
            public double ParkingSearchTime05_06 { get; set; }

            [ColumnName("Circ_N1")]
            public double ParkingSearchTime06_07 { get; set; }

            [ColumnName("Circ_N2")]
            public double ParkingSearchTime07_08 { get; set; }

            [ColumnName("Circ_N3")]
            public double ParkingSearchTime08_09 { get; set; }

            [ColumnName("Circ_NW1")]
            public double ParkingSearchTime09_15 { get; set; }

            [ColumnName("Circ_NW2")]
            public double ParkingSearchTime15_16 { get; set; }

            [ColumnName("Circ_NW3")]
            public double ParkingSearchTime16_17 { get; set; }

            [ColumnName("Circ_W1")]
            public double ParkingSearchTime17_18 { get; set; }

            [ColumnName("Circ_W2")]
            public double ParkingSearchTime18_21 { get; set; }
    */
    [ColumnName("hh_1")]
    public double HouseholdsBuffer1 { get; set; }

    [ColumnName("stugrd_1")]
    public double StudentsK8Buffer1 { get; set; }

    [ColumnName("stuhgh_1")]
    public double StudentsHighSchoolBuffer1 { get; set; }

    [ColumnName("stuuni_1")]
    public double StudentsUniversityBuffer1 { get; set; }

    [ColumnName("empedu_1")]
    public double EmploymentEducationBuffer1 { get; set; }

    [ColumnName("empfoo_1")]
    public double EmploymentFoodBuffer1 { get; set; }

    [ColumnName("empgov_1")]
    public double EmploymentGovernmentBuffer1 { get; set; }

    [ColumnName("empind_1")]
    public double EmploymentIndustrialBuffer1 { get; set; }

    [ColumnName("empmed_1")]
    public double EmploymentMedicalBuffer1 { get; set; }

    [ColumnName("empofc_1")]
    public double EmploymentOfficeBuffer1 { get; set; }

    [ColumnName("empret_1")]
    public double EmploymentRetailBuffer1 { get; set; }

    [ColumnName("empsvc_1")]
    public double EmploymentServiceBuffer1 { get; set; }

    [ColumnName("empoth_1")]
    public double EmploymentAgricultureConstructionBuffer1 { get; set; }

    [ColumnName("emptot_1")]
    public double EmploymentTotalBuffer1 { get; set; }

    [ColumnName("parkdy_1")]
    public double ParkingOffStreetPaidDailySpacesBuffer1 { get; set; }

    [ColumnName("parkhr_1")]
    public double ParkingOffStreetPaidHourlySpacesBuffer1 { get; set; }

    [ColumnName("ppricdy1")]
    public double ParkingOffStreetPaidDailyPriceBuffer1 { get; set; }

    [ColumnName("pprichr1")]
    public double ParkingOffStreetPaidHourlyPriceBuffer1 { get; set; }

    /*		[ColumnName("Circ_W3")]
            public double ParkingCostPerHour8_18Buffer1 { get; set; }

            [ColumnName("Circ_SW1")]
            public double ParkingCostPerHour18_23Buffer1 { get; set; }

            [ColumnName("Circ_SW2")]
            public double ParkingCostPerHour23_08Buffer1 { get; set; }

            [ColumnName("Circ_SW3")]
            public double ResidentAnnualParkingCostBuffer1 { get; set; }

            [ColumnName("Circ_S1")]
            public double ParkingSearchTime21_05Buffer1 { get; set; }

            [ColumnName("Circ_S2")]
            public double ParkingSearchTime05_06Buffer1 { get; set; }

            [ColumnName("Circ_S3")]
            public double ParkingSearchTime06_07Buffer1 { get; set; }

            [ColumnName("Circ_SE1")]
            public double ParkingSearchTime07_08Buffer1 { get; set; }

            [ColumnName("Circ_SE2")]
            public double ParkingSearchTime08_09Buffer1 { get; set; }

            [ColumnName("Circ_SE3")]
            public double ParkingSearchTime09_15Buffer1 { get; set; }

            [ColumnName("Dist_fry")]
            public double ParkingSearchTime15_16Buffer1 { get; set; }

            [ColumnName("dist_lrt")]
            public double ParkingSearchTime16_17Buffer1 { get; set; }

            [ColumnName("parkdy_p")]
            public double ParkingSearchTime17_18Buffer1 { get; set; }

            [ColumnName("parkhr_p")]
            public double ParkingSearchTime18_21Buffer1 { get; set; }
    */
    [ColumnName("nodes1_1")]
    public double NodesSingleLinkBuffer1 { get; set; }

    [ColumnName("nodes3_1")]
    public double NodesThreeLinksBuffer1 { get; set; }

    [ColumnName("nodes4_1")]
    public double NodesFourLinksBuffer1 { get; set; }

    [ColumnName("tstops_1")]
    public double StopsTransitBuffer1 { get; set; }

    [ColumnName("nparks_1")]
    public double OpenSpaceType1Buffer1 { get; set; }

    [ColumnName("aparks_1")]
    public double OpenSpaceType2Buffer1 { get; set; }

    [ColumnName("hh_2")]
    public double HouseholdsBuffer2 { get; set; }

    [ColumnName("stugrd_2")]
    public double StudentsK8Buffer2 { get; set; }

    [ColumnName("stuhgh_2")]
    public double StudentsHighSchoolBuffer2 { get; set; }

    [ColumnName("stuuni_2")]
    public double StudentsUniversityBuffer2 { get; set; }

    [ColumnName("empedu_2")]
    public double EmploymentEducationBuffer2 { get; set; }

    [ColumnName("empfoo_2")]
    public double EmploymentFoodBuffer2 { get; set; }

    [ColumnName("empgov_2")]
    public double EmploymentGovernmentBuffer2 { get; set; }

    [ColumnName("empind_2")]
    public double EmploymentIndustrialBuffer2 { get; set; }

    [ColumnName("empmed_2")]
    public double EmploymentMedicalBuffer2 { get; set; }

    [ColumnName("empofc_2")]
    public double EmploymentOfficeBuffer2 { get; set; }

    [ColumnName("empret_2")]
    public double EmploymentRetailBuffer2 { get; set; }

    [ColumnName("empsvc_2")]
    public double EmploymentServiceBuffer2 { get; set; }

    [ColumnName("empoth_2")]
    public double EmploymentAgricultureConstructionBuffer2 { get; set; }

    [ColumnName("emptot_2")]
    public double EmploymentTotalBuffer2 { get; set; }

    [ColumnName("parkdy_2")]
    public double ParkingOffStreetPaidDailySpacesBuffer2 { get; set; }

    [ColumnName("parkhr_2")]
    public double ParkingOffStreetPaidHourlySpacesBuffer2 { get; set; }

    [ColumnName("ppricdy2")]
    public double ParkingOffStreetPaidDailyPriceBuffer2 { get; set; }

    [ColumnName("pprichr2")]
    public double ParkingOffStreetPaidHourlyPriceBuffer2 { get; set; }

    /*		[ColumnName("ppricdyp")]
            public double ParkingCostPerHour8_18Buffer2 { get; set; }

            [ColumnName("pprichrp")]
            public double ParkingCostPerHour18_23Buffer2 { get; set; }

            [ColumnName("parkdy_1")]
            public double ParkingCostPerHour23_08Buffer2 { get; set; }

            [ColumnName("parkhr_1")]
            public double ResidentAnnualParkingCostBuffer2 { get; set; }

            [ColumnName("ppricdy1")]
            public double ParkingSearchTime21_05Buffer2 { get; set; }

            [ColumnName("pprichr1")]
            public double ParkingSearchTime05_06Buffer2 { get; set; }

            [ColumnName("parkdy_2")]
            public double ParkingSearchTime06_07Buffer2 { get; set; }

            [ColumnName("parkhr_2")]
            public double ParkingSearchTime07_08Buffer2 { get; set; }

            [ColumnName("ppricdy2")]
            public double ParkingSearchTime08_09Buffer2 { get; set; }

            [ColumnName("pprichr2")]
            public double ParkingSearchTime09_15Buffer2 { get; set; }

            [ColumnName("nparks_1")]
            public double ParkingSearchTime15_16Buffer2 { get; set; }

            [ColumnName("aparks_1")]
            public double ParkingSearchTime16_17Buffer2 { get; set; }

            [ColumnName("nparks_2")]
            public double ParkingSearchTime17_18Buffer2 { get; set; }

            [ColumnName("aparks_2")]
            public double ParkingSearchTime18_21Buffer2 { get; set; }
    */
    [ColumnName("nodes1_2")]
    public double NodesSingleLinkBuffer2 { get; set; }

    [ColumnName("nodes3_2")]
    public double NodesThreeLinksBuffer2 { get; set; }

    [ColumnName("nodes4_2")]
    public double NodesFourLinksBuffer2 { get; set; }

    [ColumnName("tstops_2")]
    public double StopsTransitBuffer2 { get; set; }

    [ColumnName("nparks_2")]
    public double OpenSpaceType1Buffer2 { get; set; }

    [ColumnName("aparks_2")]
    public double OpenSpaceType2Buffer2 { get; set; }

    [ColumnName("dist_lbus")]
    public double DistanceToLocalBus { get; set; }

    [ColumnName("dist_ebus")]
    public double DistanceToExpressBus { get; set; }

    [ColumnName("dist_fry")]
    public double DistanceToFerry { get; set; }

    [ColumnName("dist_crt")]
    public double DistanceToCommuterRail { get; set; }

    [ColumnName("dist_lrt")]
    public double DistanceToLightRail { get; set; }

    [ColumnName("Circ_E1")]
    public double CircuityRatio_E1 { get; set; }

    [ColumnName("Circ_E2")]
    public double CircuityRatio_E2 { get; set; }

    [ColumnName("Circ_E3")]
    public double CircuityRatio_E3 { get; set; }

    [ColumnName("Circ_NE1")]
    public double CircuityRatio_NE1 { get; set; }

    [ColumnName("Circ_NE2")]
    public double CircuityRatio_NE2 { get; set; }

    [ColumnName("Circ_NE3")]
    public double CircuityRatio_NE3 { get; set; }

    [ColumnName("Circ_N1")]
    public double CircuityRatio_N1 { get; set; }

    [ColumnName("Circ_N2")]
    public double CircuityRatio_N2 { get; set; }

    [ColumnName("Circ_N3")]
    public double CircuityRatio_N3 { get; set; }

    [ColumnName("Circ_NW1")]
    public double CircuityRatio_NW1 { get; set; }

    [ColumnName("Circ_NW2")]
    public double CircuityRatio_NW2 { get; set; }

    [ColumnName("Circ_NW3")]
    public double CircuityRatio_NW3 { get; set; }

    [ColumnName("Circ_W1")]
    public double CircuityRatio_W1 { get; set; }

    [ColumnName("Circ_W2")]
    public double CircuityRatio_W2 { get; set; }

    [ColumnName("Circ_W3")]
    public double CircuityRatio_W3 { get; set; }

    [ColumnName("Circ_SW1")]
    public double CircuityRatio_SW1 { get; set; }

    [ColumnName("Circ_SW2")]
    public double CircuityRatio_SW2 { get; set; }

    [ColumnName("Circ_SW3")]
    public double CircuityRatio_SW3 { get; set; }

    [ColumnName("Circ_S1")]
    public double CircuityRatio_S1 { get; set; }

    [ColumnName("Circ_S2")]
    public double CircuityRatio_S2 { get; set; }

    [ColumnName("Circ_S3")]
    public double CircuityRatio_S3 { get; set; }

    [ColumnName("Circ_SE1")]
    public double CircuityRatio_SE1 { get; set; }

    [ColumnName("Circ_SE2")]
    public double CircuityRatio_SE2 { get; set; }

    [ColumnName("Circ_SE3")]
    public double CircuityRatio_SE3 { get; set; }

  }
}