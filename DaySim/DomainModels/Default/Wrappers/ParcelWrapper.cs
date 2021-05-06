// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.ShadowPricing;

namespace DaySim.DomainModels.Default.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
  public class ParcelWrapper : IParcelWrapper {
    private readonly IParcel _parcel;

    [UsedImplicitly]
    public ParcelWrapper(IParcel parcel) {
      _parcel = parcel;
    }

    #region domain model properies

    public int Id {
      get => _parcel.Id;
      set => _parcel.Id = value;
    }

    public int SequenceFrom0 {
      get => _parcel.SequenceFrom0;
      set => _parcel.SequenceFrom0 = value;
    }

    public int Sequence {
      get => _parcel.Sequence;
      set => _parcel.Sequence = value;
    }

    public int ZoneId {
      get => _parcel.ZoneId;
      set => _parcel.ZoneId = value;
    }

    public double ZoneKey {
      get => _parcel.ZoneKey;
      set => _parcel.ZoneKey = value;
    }

    public int XCoordinate {
      get => _parcel.XCoordinate;
      set => _parcel.XCoordinate = value;
    }

    public int YCoordinate {
      get => _parcel.YCoordinate;
      set => _parcel.YCoordinate = value;
    }

    public double ThousandsSquareLengthUnits {
      get => _parcel.ThousandsSquareLengthUnits;
      set => _parcel.ThousandsSquareLengthUnits = value;
    }

    public int LandUseCode {
      get => _parcel.LandUseCode;
      set => _parcel.LandUseCode = value;
    }

    public double Households {
      get => _parcel.Households;
      set => _parcel.Households = value;
    }

    public double StudentsK8 {
      get => _parcel.StudentsK8;
      set => _parcel.StudentsK8 = value;
    }

    public double StudentsHighSchool {
      get => _parcel.StudentsHighSchool;
      set => _parcel.StudentsHighSchool = value;
    }

    public double StudentsUniversity {
      get => _parcel.StudentsUniversity;
      set => _parcel.StudentsUniversity = value;
    }

    public double EmploymentEducation {
      get => _parcel.EmploymentEducation;
      set => _parcel.EmploymentEducation = value;
    }

    public double EmploymentFood {
      get => _parcel.EmploymentFood;
      set => _parcel.EmploymentFood = value;
    }

    public double EmploymentGovernment {
      get => _parcel.EmploymentGovernment;
      set => _parcel.EmploymentGovernment = value;
    }

    public double EmploymentIndustrial {
      get => _parcel.EmploymentIndustrial;
      set => _parcel.EmploymentIndustrial = value;
    }

    public double EmploymentMedical {
      get => _parcel.EmploymentMedical;
      set => _parcel.EmploymentMedical = value;
    }

    public double EmploymentOffice {
      get => _parcel.EmploymentOffice;
      set => _parcel.EmploymentOffice = value;
    }

    public double EmploymentRetail {
      get => _parcel.EmploymentRetail;
      set => _parcel.EmploymentRetail = value;
    }

    public double EmploymentService {
      get => _parcel.EmploymentService;
      set => _parcel.EmploymentService = value;
    }

    public double EmploymentAgricultureConstruction {
      get => _parcel.EmploymentAgricultureConstruction;
      set => _parcel.EmploymentAgricultureConstruction = value;
    }

    public double EmploymentTotal {
      get => _parcel.EmploymentTotal;
      set => _parcel.EmploymentTotal = value;
    }

    public double ParkingOffStreetPaidDailySpaces {
      get => _parcel.ParkingOffStreetPaidDailySpaces;
      set => _parcel.ParkingOffStreetPaidDailySpaces = value;
    }

    public double ParkingOffStreetPaidHourlySpaces {
      get => _parcel.ParkingOffStreetPaidHourlySpaces;
      set => _parcel.ParkingOffStreetPaidHourlySpaces = value;
    }

    public double ParkingOffStreetPaidDailyPrice {
      get => _parcel.ParkingOffStreetPaidDailyPrice;
      set => _parcel.ParkingOffStreetPaidDailyPrice = value;
    }

    public double ParkingOffStreetPaidHourlyPrice {
      get => _parcel.ParkingOffStreetPaidHourlyPrice;
      set => _parcel.ParkingOffStreetPaidHourlyPrice = value;
    }

    public double HouseholdsBuffer1 {
      get => _parcel.HouseholdsBuffer1;
      set => _parcel.HouseholdsBuffer1 = value;
    }

    public double StudentsK8Buffer1 {
      get => _parcel.StudentsK8Buffer1;
      set => _parcel.StudentsK8Buffer1 = value;
    }

    public double StudentsHighSchoolBuffer1 {
      get => _parcel.StudentsHighSchoolBuffer1;
      set => _parcel.StudentsHighSchoolBuffer1 = value;
    }

    public double StudentsUniversityBuffer1 {
      get => _parcel.StudentsUniversityBuffer1;
      set => _parcel.StudentsUniversityBuffer1 = value;
    }

    public double EmploymentEducationBuffer1 {
      get => _parcel.EmploymentEducationBuffer1;
      set => _parcel.EmploymentEducationBuffer1 = value;
    }

    public double EmploymentFoodBuffer1 {
      get => _parcel.EmploymentFoodBuffer1;
      set => _parcel.EmploymentFoodBuffer1 = value;
    }

    public double EmploymentGovernmentBuffer1 {
      get => _parcel.EmploymentGovernmentBuffer1;
      set => _parcel.EmploymentGovernmentBuffer1 = value;
    }

    public double EmploymentIndustrialBuffer1 {
      get => _parcel.EmploymentIndustrialBuffer1;
      set => _parcel.EmploymentIndustrialBuffer1 = value;
    }

    public double EmploymentMedicalBuffer1 {
      get => _parcel.EmploymentMedicalBuffer1;
      set => _parcel.EmploymentMedicalBuffer1 = value;
    }

    public double EmploymentOfficeBuffer1 {
      get => _parcel.EmploymentOfficeBuffer1;
      set => _parcel.EmploymentOfficeBuffer1 = value;
    }

    public double EmploymentRetailBuffer1 {
      get => _parcel.EmploymentRetailBuffer1;
      set => _parcel.EmploymentRetailBuffer1 = value;
    }

    public double EmploymentServiceBuffer1 {
      get => _parcel.EmploymentServiceBuffer1;
      set => _parcel.EmploymentServiceBuffer1 = value;
    }

    public double EmploymentAgricultureConstructionBuffer1 {
      get => _parcel.EmploymentAgricultureConstructionBuffer1;
      set => _parcel.EmploymentAgricultureConstructionBuffer1 = value;
    }

    public double EmploymentTotalBuffer1 {
      get => _parcel.EmploymentTotalBuffer1;
      set => _parcel.EmploymentTotalBuffer1 = value;
    }

    public double ParkingOffStreetPaidDailySpacesBuffer1 {
      get => _parcel.ParkingOffStreetPaidDailySpacesBuffer1;
      set => _parcel.ParkingOffStreetPaidDailySpacesBuffer1 = value;
    }

    public double ParkingOffStreetPaidHourlySpacesBuffer1 {
      get => _parcel.ParkingOffStreetPaidHourlySpacesBuffer1;
      set => _parcel.ParkingOffStreetPaidHourlySpacesBuffer1 = value;
    }

    public double ParkingOffStreetPaidDailyPriceBuffer1 {
      get => _parcel.ParkingOffStreetPaidDailyPriceBuffer1;
      set => _parcel.ParkingOffStreetPaidDailyPriceBuffer1 = value;
    }

    public double ParkingOffStreetPaidHourlyPriceBuffer1 {
      get => _parcel.ParkingOffStreetPaidHourlyPriceBuffer1;
      set => _parcel.ParkingOffStreetPaidHourlyPriceBuffer1 = value;
    }

    public double NodesSingleLinkBuffer1 {
      get => _parcel.NodesSingleLinkBuffer1;
      set => _parcel.NodesSingleLinkBuffer1 = value;
    }

    public double NodesThreeLinksBuffer1 {
      get => _parcel.NodesThreeLinksBuffer1;
      set => _parcel.NodesThreeLinksBuffer1 = value;
    }

    public double NodesFourLinksBuffer1 {
      get => _parcel.NodesFourLinksBuffer1;
      set => _parcel.NodesFourLinksBuffer1 = value;
    }

    public double StopsTransitBuffer1 {
      get => _parcel.StopsTransitBuffer1;
      set => _parcel.StopsTransitBuffer1 = value;
    }

    public double OpenSpaceType1Buffer1 {
      get => _parcel.OpenSpaceType1Buffer1;
      set => _parcel.OpenSpaceType1Buffer1 = value;
    }

    public double OpenSpaceType2Buffer1 {
      get => _parcel.OpenSpaceType2Buffer1;
      set => _parcel.OpenSpaceType2Buffer1 = value;
    }

    public double HouseholdsBuffer2 {
      get => _parcel.HouseholdsBuffer2;
      set => _parcel.HouseholdsBuffer2 = value;
    }

    public double StudentsK8Buffer2 {
      get => _parcel.StudentsK8Buffer2;
      set => _parcel.StudentsK8Buffer2 = value;
    }

    public double StudentsHighSchoolBuffer2 {
      get => _parcel.StudentsHighSchoolBuffer2;
      set => _parcel.StudentsHighSchoolBuffer2 = value;
    }

    public double StudentsUniversityBuffer2 {
      get => _parcel.StudentsUniversityBuffer2;
      set => _parcel.StudentsUniversityBuffer2 = value;
    }

    public double EmploymentEducationBuffer2 {
      get => _parcel.EmploymentEducationBuffer2;
      set => _parcel.EmploymentEducationBuffer2 = value;
    }

    public double EmploymentFoodBuffer2 {
      get => _parcel.EmploymentFoodBuffer2;
      set => _parcel.EmploymentFoodBuffer2 = value;
    }

    public double EmploymentGovernmentBuffer2 {
      get => _parcel.EmploymentGovernmentBuffer2;
      set => _parcel.EmploymentGovernmentBuffer2 = value;
    }

    public double EmploymentIndustrialBuffer2 {
      get => _parcel.EmploymentIndustrialBuffer2;
      set => _parcel.EmploymentIndustrialBuffer2 = value;
    }

    public double EmploymentMedicalBuffer2 {
      get => _parcel.EmploymentMedicalBuffer2;
      set => _parcel.EmploymentMedicalBuffer2 = value;
    }

    public double EmploymentOfficeBuffer2 {
      get => _parcel.EmploymentOfficeBuffer2;
      set => _parcel.EmploymentOfficeBuffer2 = value;
    }

    public double EmploymentRetailBuffer2 {
      get => _parcel.EmploymentRetailBuffer2;
      set => _parcel.EmploymentRetailBuffer2 = value;
    }

    public double EmploymentServiceBuffer2 {
      get => _parcel.EmploymentServiceBuffer2;
      set => _parcel.EmploymentServiceBuffer2 = value;
    }

    public double EmploymentAgricultureConstructionBuffer2 {
      get => _parcel.EmploymentAgricultureConstructionBuffer2;
      set => _parcel.EmploymentAgricultureConstructionBuffer2 = value;
    }

    public double EmploymentTotalBuffer2 {
      get => _parcel.EmploymentTotalBuffer2;
      set => _parcel.EmploymentTotalBuffer2 = value;
    }

    public double ParkingOffStreetPaidDailySpacesBuffer2 {
      get => _parcel.ParkingOffStreetPaidDailySpacesBuffer2;
      set => _parcel.ParkingOffStreetPaidDailySpacesBuffer2 = value;
    }

    public double ParkingOffStreetPaidHourlySpacesBuffer2 {
      get => _parcel.ParkingOffStreetPaidHourlySpacesBuffer2;
      set => _parcel.ParkingOffStreetPaidHourlySpacesBuffer2 = value;
    }

    public double ParkingOffStreetPaidDailyPriceBuffer2 {
      get => _parcel.ParkingOffStreetPaidDailyPriceBuffer2;
      set => _parcel.ParkingOffStreetPaidDailyPriceBuffer2 = value;
    }

    public double ParkingOffStreetPaidHourlyPriceBuffer2 {
      get => _parcel.ParkingOffStreetPaidHourlyPriceBuffer2;
      set => _parcel.ParkingOffStreetPaidHourlyPriceBuffer2 = value;
    }

    public double NodesSingleLinkBuffer2 {
      get => _parcel.NodesSingleLinkBuffer2;
      set => _parcel.NodesSingleLinkBuffer2 = value;
    }

    public double NodesThreeLinksBuffer2 {
      get => _parcel.NodesThreeLinksBuffer2;
      set => _parcel.NodesThreeLinksBuffer2 = value;
    }

    public double NodesFourLinksBuffer2 {
      get => _parcel.NodesFourLinksBuffer2;
      set => _parcel.NodesFourLinksBuffer2 = value;
    }

    public double StopsTransitBuffer2 {
      get => _parcel.StopsTransitBuffer2;
      set => _parcel.StopsTransitBuffer2 = value;
    }

    public double OpenSpaceType1Buffer2 {
      get => _parcel.OpenSpaceType1Buffer2;
      set => _parcel.OpenSpaceType1Buffer2 = value;
    }

    public double OpenSpaceType2Buffer2 {
      get => _parcel.OpenSpaceType2Buffer2;
      set => _parcel.OpenSpaceType2Buffer2 = value;
    }

    public double DistanceToLocalBus {
      get => _parcel.DistanceToLocalBus;
      set => _parcel.DistanceToLocalBus = value;
    }

    public double DistanceToExpressBus {
      get => _parcel.DistanceToExpressBus;
      set => _parcel.DistanceToExpressBus = value;
    }

    public double DistanceToFerry {
      get => _parcel.DistanceToFerry;
      set => _parcel.DistanceToFerry = value;
    }

    public double DistanceToCommuterRail {
      get => _parcel.DistanceToCommuterRail;
      set => _parcel.DistanceToCommuterRail = value;
    }

    public double DistanceToLightRail {
      get => _parcel.DistanceToLightRail;
      set => _parcel.DistanceToLightRail = value;
    }

    public double CircuityRatio_E1 {
      get => _parcel.CircuityRatio_E1;
      set => _parcel.CircuityRatio_E1 = value;
    }

    public double CircuityRatio_E2 {
      get => _parcel.CircuityRatio_E2;
      set => _parcel.CircuityRatio_E2 = value;
    }

    public double CircuityRatio_E3 {
      get => _parcel.CircuityRatio_E3;
      set => _parcel.CircuityRatio_E3 = value;
    }

    public double CircuityRatio_NE1 {
      get => _parcel.CircuityRatio_NE1;
      set => _parcel.CircuityRatio_NE1 = value;
    }

    public double CircuityRatio_NE2 {
      get => _parcel.CircuityRatio_NE2;
      set => _parcel.CircuityRatio_NE2 = value;
    }

    public double CircuityRatio_NE3 {
      get => _parcel.CircuityRatio_NE3;
      set => _parcel.CircuityRatio_NE3 = value;
    }

    public double CircuityRatio_N1 {
      get => _parcel.CircuityRatio_N1;
      set => _parcel.CircuityRatio_N1 = value;
    }

    public double CircuityRatio_N2 {
      get => _parcel.CircuityRatio_N2;
      set => _parcel.CircuityRatio_N2 = value;
    }

    public double CircuityRatio_N3 {
      get => _parcel.CircuityRatio_N3;
      set => _parcel.CircuityRatio_N3 = value;
    }

    public double CircuityRatio_NW1 {
      get => _parcel.CircuityRatio_NW1;
      set => _parcel.CircuityRatio_NW1 = value;
    }

    public double CircuityRatio_NW2 {
      get => _parcel.CircuityRatio_NW2;
      set => _parcel.CircuityRatio_NW2 = value;
    }

    public double CircuityRatio_NW3 {
      get => _parcel.CircuityRatio_NW3;
      set => _parcel.CircuityRatio_NW3 = value;
    }

    public double CircuityRatio_W1 {
      get => _parcel.CircuityRatio_W1;
      set => _parcel.CircuityRatio_W1 = value;
    }

    public double CircuityRatio_W2 {
      get => _parcel.CircuityRatio_W2;
      set => _parcel.CircuityRatio_W2 = value;
    }

    public double CircuityRatio_W3 {
      get => _parcel.CircuityRatio_W3;
      set => _parcel.CircuityRatio_W3 = value;
    }

    public double CircuityRatio_SW1 {
      get => _parcel.CircuityRatio_SW1;
      set => _parcel.CircuityRatio_SW1 = value;
    }

    public double CircuityRatio_SW2 {
      get => _parcel.CircuityRatio_SW2;
      set => _parcel.CircuityRatio_SW2 = value;
    }

    public double CircuityRatio_SW3 {
      get => _parcel.CircuityRatio_SW3;
      set => _parcel.CircuityRatio_SW3 = value;
    }

    public double CircuityRatio_S1 {
      get => _parcel.CircuityRatio_S1;
      set => _parcel.CircuityRatio_S1 = value;
    }

    public double CircuityRatio_S2 {
      get => _parcel.CircuityRatio_S2;
      set => _parcel.CircuityRatio_S2 = value;
    }

    public double CircuityRatio_S3 {
      get => _parcel.CircuityRatio_S3;
      set => _parcel.CircuityRatio_S3 = value;
    }

    public double CircuityRatio_SE1 {
      get => _parcel.CircuityRatio_SE1;
      set => _parcel.CircuityRatio_SE1 = value;
    }

    public double CircuityRatio_SE2 {
      get => _parcel.CircuityRatio_SE2;
      set => _parcel.CircuityRatio_SE2 = value;
    }

    public double CircuityRatio_SE3 {
      get => _parcel.CircuityRatio_SE3;
      set => _parcel.CircuityRatio_SE3 = value;
    }

    #endregion

    #region flags/choice model/etc. properties

    public double ShadowPriceForEmployment { get; set; }

    public double ShadowPriceForStudentsK12 { get; set; }

    public double ShadowPriceForStudentsUniversity { get; set; }

    public double ExternalEmploymentTotal { get; set; }

    public double EmploymentDifference { get; set; }

    public double EmploymentPrediction { get; set; }

    public double ExternalStudentsK12 { get; set; }

    public double StudentsK12Difference { get; set; }

    public double StudentsK12Prediction { get; set; }

    public double ExternalStudentsUniversity { get; set; }

    public double StudentsUniversityDifference { get; set; }

    public double StudentsUniversityPrediction { get; set; }

    public int District { get; set; }

    public bool StopAreaDistanceArrayPositionsSet { get; set; } = false;

    public int FirstPositionInStopAreaDistanceArray { get; set; } = -1;

    public int LastPositionInStopAreaDistanceArray { get; set; } = -1;

    #endregion

    #region wrapper methods


    public virtual int GetLandUseCode19() {
      return LandUseCode == 19 ? 1 : 0;
    }

    public virtual double GetStudentsK12() {
      return StudentsK8 + StudentsHighSchool;
    }

    public virtual double GetDistanceToTransit() {
      double distance = Constants.DEFAULT_VALUE;

      if (DistanceToFerry >= 0) {
        distance = DistanceToFerry;
      }

      if (DistanceToCommuterRail >= 0 && (distance < 0 || DistanceToCommuterRail < distance)) {
        distance = DistanceToCommuterRail;
      }

      if (DistanceToLightRail >= 0 && (distance < 0 || DistanceToLightRail < distance)) {
        distance = DistanceToLightRail;
      }

      if (DistanceToExpressBus >= 0 && (distance < 0 || DistanceToExpressBus < distance)) {
        distance = DistanceToExpressBus;
      }

      if (DistanceToLocalBus >= 0 && (distance < 0 || DistanceToLocalBus < distance)) {
        distance = DistanceToLocalBus;
      }

      return distance;
    }

    //        public virtual void SetFirstStopAreaDistanceIndex( int index) {
    //            FirstPositionInStopAreaDistanceArray = index;
    //        }

    //        public virtual void SetLastStopAreaDistanceIndex( int index) {
    //            LastPositionInStopAreaDistanceArray = index;
    //        }


    /*     is now done in Engine instead
            public virtual void SetFirstAndLastStopAreaDistanceIndexes() {
                if ( StopAreaDistanceArrayPositionsSet) {
                    return;
                }

                StopAreaDistanceArrayPositionsSet = true;

                FirstPositionInStopAreaDistanceArray = -1;
                LastPositionInStopAreaDistanceArray = -1;

                for (var x = 0; x < Global.ParcelStopAreaParcelIds.Length; x++) {
                    if (Id == Global.ParcelStopAreaParcelIds[x]) {
                        if (FirstPositionInStopAreaDistanceArray < 0) {
                            FirstPositionInStopAreaDistanceArray = x;
                        }

                        LastPositionInStopAreaDistanceArray = x;
                    }
                    else if (FirstPositionInStopAreaDistanceArray >= 0) {
                        return;
                    }
                }
            }
    */
    public virtual double NetIntersectionDensity1() {
      return NodesFourLinksBuffer1 + NodesThreeLinksBuffer1 - NodesSingleLinkBuffer1;
    }

    public virtual double NetIntersectionDensity2() {
      return NodesFourLinksBuffer2 + NodesThreeLinksBuffer2 - NodesSingleLinkBuffer2;
    }

    public virtual double OpenSpaceDensity1() {
      return OpenSpaceType1Buffer1; //converted to million square feet
    }

    public virtual double OpenSpaceDensity2() {
      return OpenSpaceType1Buffer2; //converted to million square feet
    }

    public virtual double OpenSpaceMillionSqFtBuffer1() {
      return OpenSpaceType1Buffer1 * OpenSpaceType2Buffer1 / 1000000.0; //converted to million square feet
    }

    public virtual double OpenSpaceMillionSqFtBuffer2() {
      return OpenSpaceType1Buffer2 * OpenSpaceType2Buffer2 / 1000000.0; //converted to million square feet
    }

    public virtual double RetailEmploymentDensity1() {
      return EmploymentRetailBuffer1 + EmploymentFoodBuffer1;
    }

    public virtual double RetailEmploymentDensity2() {
      return EmploymentRetailBuffer2 + EmploymentFoodBuffer2;
    }

    public virtual double ServiceEmploymentDensity1() {
      return EmploymentServiceBuffer1 + EmploymentMedicalBuffer1;
    }

    public virtual double ServiceEmploymentDensity2() {
      return EmploymentServiceBuffer2 + EmploymentMedicalBuffer2;
    }

    public virtual double OfficeEmploymentDensity1() {
      return EmploymentOfficeBuffer1 + EmploymentGovernmentBuffer1;
    }

    public virtual double OfficeEmploymentDensity2() {
      return EmploymentOfficeBuffer2 + EmploymentGovernmentBuffer2;
    }

    public virtual double TotalEmploymentDensity1() {
      return EmploymentTotalBuffer1;
    }

    public virtual double TotalEmploymentDensity2() {
      return EmploymentTotalBuffer2;
    }

    public virtual double StudentEnrolmentDensity1() {
      return StudentsHighSchoolBuffer1 + StudentsUniversityBuffer1 + StudentsK8Buffer1;
    }

    public virtual double StudentEnrolmentDensity2() {
      return StudentsHighSchoolBuffer2 + StudentsUniversityBuffer2 + StudentsK8Buffer2;
    }

    public virtual double HouseholdDensity1() {
      return HouseholdsBuffer1;
    }

    public virtual double HouseholdDensity2() {
      return HouseholdsBuffer2;
    }

    public virtual double MixedUse2Index1() {
      double hh = HouseholdDensity1();
      double emp = TotalEmploymentDensity1();

      return Log2(hh, emp);
    }

    public virtual double MixedUse2Index2() {
      double hh = HouseholdDensity2();
      double emp = TotalEmploymentDensity2();

      return Log2(hh, emp);
    }

    public virtual double MixedUse3Index2() {
      double hh = HouseholdDensity2();
      double ret = RetailEmploymentDensity2();
      double svc = ServiceEmploymentDensity2();

      return Log3(hh, ret, svc);
    }

    public virtual double MixedUse4Index1() {
      double hh = HouseholdDensity1();
      double ret = RetailEmploymentDensity1();
      double svc = ServiceEmploymentDensity1();
      double ofc = OfficeEmploymentDensity1();

      return Log4(hh, ret, svc, ofc);
    }

    public virtual double MixedUse4Index2() {
      double hh = HouseholdDensity2();
      double ret = RetailEmploymentDensity2();
      double svc = ServiceEmploymentDensity2();
      double ofc = OfficeEmploymentDensity2();

      return Log4(hh, ret, svc, ofc);
    }

    public virtual int TransitAccessSegment() {
      return
          // JLBscal:  divided by DistanceUnitsPerMile in following four lines to convert to miles scale
          GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile >= 0 && GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile <= .25
              ? Global.Settings.TransitAccesses.Gt0AndLteQtrMi
              : GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile > .25 && GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile <= .5
                  ? Global.Settings.TransitAccesses.GtQtrMiAndLteHMi
                  : Global.Settings.TransitAccesses.None;
    }

    public virtual double ParcelParkingPerTotalEmployment() {
      double spaces = ParkingOffStreetPaidDailySpaces;
      double total = EmploymentTotal;

      return MaxLog(spaces, total);
    }

    public virtual double ParkingHourlyEmploymentCommercialMixInParcel() {
      double spaces = ParkingOffStreetPaidHourlySpaces;
      double emp = EmploymentFood + EmploymentRetail + EmploymentService + EmploymentMedical;

      return Log2(spaces, emp);
    }

    public virtual double ParkingHourlyEmploymentCommercialMixBuffer1() {
      double spaces = ParkingOffStreetPaidHourlySpacesBuffer1;
      double emp = EmploymentFoodBuffer1 + EmploymentRetailBuffer1 + EmploymentServiceBuffer1 + EmploymentMedicalBuffer1;

      return Log2(spaces, emp);
    }

    public virtual double ParkingDailyEmploymentTotalMixInParcel() {
      double spaces = ParkingOffStreetPaidDailySpaces;
      double emp = EmploymentTotal;

      return Log2(spaces, emp);
    }

    public virtual double ParkingDailyEmploymentTotalMixBuffer1() {
      double spaces = ParkingOffStreetPaidDailySpacesBuffer1;
      double emp = EmploymentTotalBuffer1;

      return Log2(spaces, emp);
    }

    public virtual double ParcelParkingPerFoodRetailServiceMedicalEmployment() {
      double spaces = ParkingOffStreetPaidDailySpaces;
      double total = EmploymentFood + EmploymentRetail + EmploymentService + EmploymentMedical;

      return MaxLog(spaces, total);
    }

    public virtual double ZoneParkingPerTotalEmploymentAndK12UniversityStudents(IZoneTotals zoneTotals, double millionsSquareLengthUnits) {
      double spaces = ParkingOffStreetPaidDailySpaces / millionsSquareLengthUnits;
      double total = (EmploymentTotal + zoneTotals.StudentsK12 + zoneTotals.StudentsUniversity) * 100 / millionsSquareLengthUnits;

      return MaxLog(spaces, total);
    }

    public virtual double ZoneParkingPerFoodRetailServiceMedicalEmployment(double millionsSquareLengthUnits) {
      double spaces = ParkingOffStreetPaidDailySpaces / millionsSquareLengthUnits;
      double total = (EmploymentFood + EmploymentRetail + EmploymentService + EmploymentMedical) * 100 / millionsSquareLengthUnits;

      return MaxLog(spaces, total);
    }

    public virtual double C34RatioBuffer1() {
      return (NodesThreeLinksBuffer1 + NodesFourLinksBuffer1) / (Math.Max(1, NodesSingleLinkBuffer1 + NodesThreeLinksBuffer1 + NodesFourLinksBuffer1));
    }

    public virtual double ParcelHouseholdsPerRetailServiceEmploymentBuffer2() {
      double households = HouseholdsBuffer2 * 100;
      double total = (EmploymentRetailBuffer2 + EmploymentServiceBuffer2) * 100;

      return Math.Min(100000, .001 * households * total / (households + total + .1));
    }

    public virtual double ParcelHouseholdsPerRetailServiceFoodEmploymentBuffer2() {
      double households = HouseholdsBuffer2 * 100;
      double total = (EmploymentRetailBuffer2 + EmploymentServiceBuffer2 + EmploymentFoodBuffer2) * 100;

      return households * total / (households + total + 1);
    }

    public virtual double IntersectionDensity34Buffer2() {
      return NodesThreeLinksBuffer2 * .5 + NodesFourLinksBuffer2;
    }

    public virtual double IntersectionDensity34Minus1Buffer2() {
      return IntersectionDensity34Buffer2() - NodesSingleLinkBuffer2;
    }

    public virtual double ParkingCostBuffer1(double parkingDuration) {
      double parkingCost =
                ParkingOffStreetPaidDailyPriceBuffer1 < parkingDuration * ParkingOffStreetPaidHourlyPriceBuffer1
                    ? ParkingOffStreetPaidDailyPriceBuffer1
                    : parkingDuration * ParkingOffStreetPaidHourlyPriceBuffer1;

      return parkingCost / 100; // convert to Monetary units from hundredths of monetary units
    }

    public virtual double FoodRetailServiceMedicalLogBuffer1() {
      return Math.Log(1 + EmploymentFoodBuffer1 + EmploymentRetailBuffer1 + EmploymentServiceBuffer1 + EmploymentMedicalBuffer1);
    }

    // MB tried these factors based on a quick principal components analysis, but they don't work very well in estimation
    public virtual double PCA_DensityTerm_Buffer1() {
      return
        0.682 * HouseholdDensity1() +
        0.760 * RetailEmploymentDensity1() +
        0.869 * ServiceEmploymentDensity1() +
        0.829 * OfficeEmploymentDensity1() +
        0.626 * NetIntersectionDensity1() +
        0.334 * StopsTransitBuffer1 +
        0.377 * OpenSpaceType1Buffer1 +
        0.825 * ParkingOffStreetPaidHourlySpacesBuffer1 +
        0.061 * MixedUse2Index1();
    }

    public virtual double PCA_WalkabilityTerm_Buffer1() {
      return
        0.300 * HouseholdDensity1() +
       -0.331 * RetailEmploymentDensity1() +
       -0.204 * ServiceEmploymentDensity1() +
       -0.368 * OfficeEmploymentDensity1() +
        0.628 * NetIntersectionDensity1() +
        0.430 * StopsTransitBuffer1 +
        0.682 * OpenSpaceType1Buffer1 +
       -0.349 * ParkingOffStreetPaidHourlySpacesBuffer1 +
        0.380 * MixedUse2Index1();
    }

    public virtual double PCA_MixedUseTerm_Buffer1() {
      return
        0.157 * HouseholdDensity1() +
        0.113 * RetailEmploymentDensity1() +
        0.055 * ServiceEmploymentDensity1() +
       -0.020 * OfficeEmploymentDensity1() +
       -0.139 * NetIntersectionDensity1() +
       -0.262 * StopsTransitBuffer1 +
       -0.224 * OpenSpaceType1Buffer1 +
       -0.023 * ParkingOffStreetPaidHourlySpacesBuffer1 +
        0.890 * MixedUse2Index1();
    }

    public virtual double PCA_TransitAccessTerm_Buffer1() {
      return
        0.222 * HouseholdDensity1() +
        0.000 * RetailEmploymentDensity1() +
        0.056 * ServiceEmploymentDensity1() +
       -0.087 * OfficeEmploymentDensity1() +
       -0.192 * NetIntersectionDensity1() +
        0.729 * StopsTransitBuffer1 +
       -0.469 * OpenSpaceType1Buffer1 +
       -0.092 * ParkingOffStreetPaidHourlySpacesBuffer1 +
        0.020 * MixedUse2Index1();
    }

    public virtual int CoreCBD_AreaType_Buffer1() {
      return (HouseholdsBuffer1 + EmploymentTotalBuffer1 >= 9000).ToFlag();
    }

    public virtual int FringCBD_AreaType_Buffer1() {
      return (HouseholdsBuffer1 + EmploymentTotalBuffer1 >= 7800
           && HouseholdsBuffer1 + EmploymentTotalBuffer1 <  9000).ToFlag();
    }

    public virtual int CBD_AreaType_Buffer1() {
      return CoreCBD_AreaType_Buffer1() + FringCBD_AreaType_Buffer1();
    }

    public virtual int Urban_AreaType_Buffer1() {
      return (HouseholdsBuffer1 + EmploymentTotalBuffer1 >= 2750
           && HouseholdsBuffer1 + EmploymentTotalBuffer1 <  7800).ToFlag();
    }

    public virtual int Suburban_AreaType_Buffer1() {
      return (HouseholdsBuffer1 + EmploymentTotalBuffer1 >= 450
           && HouseholdsBuffer1 + EmploymentTotalBuffer1 < 2750).ToFlag();
    }

    public virtual int Rural_AreaType_Buffer1() {
      return (HouseholdsBuffer1 + EmploymentTotalBuffer1 >= 40
           && HouseholdsBuffer1 + EmploymentTotalBuffer1 < 450).ToFlag();
    }

    public virtual int OpenRural_AreaType_Buffer1() {
      return (HouseholdsBuffer1 + EmploymentTotalBuffer1 < 40).ToFlag();
    }


    public virtual int AllRural_AreaType_Buffer1() {
      return Rural_AreaType_Buffer1() + OpenRural_AreaType_Buffer1();
    }

    public virtual double K8HighSchoolQtrMileLogBuffer1() {
      return Math.Log(1 + StudentsK8Buffer1 + StudentsHighSchoolBuffer1);
    }

    public virtual int UsualWorkParcelFlag(int usualWorkParcelId) {
      return (Id == usualWorkParcelId).ToFlag();
    }

    public virtual int NotUsualWorkParcelFlag(int usualWorkParcelId) {
      return (Id != usualWorkParcelId).ToFlag();
    }

    public virtual int RuralFlag() {
      return (TotalEmploymentDensity1() + HouseholdDensity1() < Global.Configuration.UrbanThreshold).ToFlag();
    }

    public virtual void SetShadowPricing(Dictionary<int, IZone> zones, Dictionary<int, IShadowPriceParcel> shadowPrices) {
      if (!Global.Configuration.ShouldUseShadowPricing || (!Global.Configuration.ShouldRunWorkLocationModel && !Global.Configuration.ShouldRunSchoolLocationModel)) {
        return;
      }


      if (shadowPrices.TryGetValue(Id, out IShadowPriceParcel shadowPriceParcel)) {
        ShadowPriceForEmployment = shadowPrices[Id].ShadowPriceForEmployment;
        ShadowPriceForStudentsK12 = shadowPrices[Id].ShadowPriceForStudentsK12;
        ShadowPriceForStudentsUniversity = shadowPrices[Id].ShadowPriceForStudentsUniversity;
      }


      if (zones.TryGetValue(ZoneId, out IZone zone)) {
        ExternalEmploymentTotal = EmploymentTotal * (1 - zone.FractionJobsFilledByWorkersFromOutsideRegion);

        // TODO: Missing information about external students. Zero is the placeholder for university student fraction.
        ExternalStudentsK12 = 0;
        ExternalStudentsUniversity = StudentsUniversity * (1 - 0);
      }

    }

    private readonly object _getEmploymentPredictionLock = new object();

    public virtual void AddEmploymentPrediction(double employmentPrediction) {
      lock (_getEmploymentPredictionLock) {
        EmploymentPrediction = EmploymentPrediction + employmentPrediction;
      }
    }

    private readonly object _getStudentsUniversityPredictionLock = new object();

    public virtual void AddStudentsUniversityPrediction(double studentsUniversityPrediction) {
      lock (_getStudentsUniversityPredictionLock) {
        StudentsUniversityPrediction = StudentsUniversityPrediction + studentsUniversityPrediction;
      }
    }

    private readonly object _getStudentsK12PredictionLock = new object();

    public virtual void AddStudentsK12Prediction(double studentsK12Prediction) {
      lock (_getStudentsK12PredictionLock) {
        StudentsK12Prediction = StudentsK12Prediction + studentsK12Prediction;
      }
    }


    public double CalculateShortDistance(IParcelWrapper destination) {
      if (Global.Configuration.UseShortDistanceNodeToNodeMeasures) {
        return NodeToNodeDistance(destination);
      }
      if (Global.Configuration.UseShortDistanceCircuityMeasures) {
        return CircuityDistance(destination);
      }
      // if corrected calculation becomes default, move this to top of this method and remove from top of two methods below
      if (Id == destination.Id && ThousandsSquareLengthUnits > Constants.EPSILON && Global.Configuration.CorrectIntraParcelAreaToDistanceCalculation) {
        double calcDistance = (Global.Configuration.CorrectIntraParcelAreaToDistanceCalculation)
                 ? Math.Sqrt(1000 * ThousandsSquareLengthUnits) / (2.0 * 5280 * Global.Settings.DistanceUnitsPerMile)
                 : Math.Sqrt(ThousandsSquareLengthUnits) / (2.0);
        return Math.Max(calcDistance, Global.Configuration.MinimumIntraParcelDistanceCutoff);
      }
      return Constants.DEFAULT_VALUE;
    }

    public double NodeToNodeDistance(IParcelWrapper destination) {
      if (Id == destination.Id && ThousandsSquareLengthUnits > Constants.EPSILON) {
        double calcDistance = (Global.Configuration.CorrectIntraParcelAreaToDistanceCalculation)
                 ? Math.Sqrt(1000 * ThousandsSquareLengthUnits) / (2.0 * 5280 * Global.Settings.DistanceUnitsPerMile)
                 : Math.Sqrt(ThousandsSquareLengthUnits) / (2.0);
        return Math.Max(calcDistance, Global.Configuration.MinimumIntraParcelDistanceCutoff);
      }
      int threadAssignedIndex = ParallelUtility.threadLocalAssignedIndex.Value;
      //added for intra-microzone distance, square root of area over 2   MB 20180305 This code is wrong - should also divide by 5280. Use a configuration override for now.
      if (Id == Global.NodeNodePreviousOriginParcelId[threadAssignedIndex] && destination.Id == Global.NodeNodePreviousDestinationParcelId[threadAssignedIndex]) {
        return Global.NodeNodePreviousDistance[threadAssignedIndex];
      }

      Global.NodeNodePreviousOriginParcelId[threadAssignedIndex] = Id;
      Global.NodeNodePreviousDestinationParcelId[threadAssignedIndex] = destination.Id;
      Global.NodeNodePreviousDistance[threadAssignedIndex] = Constants.DEFAULT_VALUE;

      // get the origin parcel node id and then the index of the node_id
      bool foundOrigin = Global.NodeIndex.TryGetValue(Id, out int oNodeId);
      foundOrigin = Global.ANodeId.TryGetValue(oNodeId, out int oNodeIdIndex);

      //same logic for destination
      bool foundDestination = Global.NodeIndex.TryGetValue(destination.Id, out int dNodeId);
      foundDestination = Global.ANodeId.TryGetValue(dNodeId, out int dNodeIdIndex);

      if (!foundDestination || !foundOrigin) {
        return Constants.DEFAULT_VALUE;
      }

      if (oNodeId == dNodeId || oNodeId < 1 || dNodeId < 1 || oNodeId > Global.ANodeId.Count || dNodeId > Global.ANodeId.Count) {
        return Constants.DEFAULT_VALUE;
      }

      int aNodeId = oNodeId;
      int aNodeIdIndex = oNodeIdIndex;
      int bNodeId = dNodeId;
      int bNodeIdIndex = dNodeIdIndex;

      // if symmetry assumed - use smaller node # as aNode
      if (!Global.Configuration.AllowNodeDistanceAsymmetry) {
        if (oNodeId > dNodeId) {
          aNodeId = dNodeId;
          bNodeId = oNodeId;
          aNodeIdIndex = dNodeIdIndex;
        }
      }

      int firstRecord = Global.ANodeFirstRecord[aNodeIdIndex - 1];
      int lastRecord = Global.ANodeLastRecord[aNodeIdIndex - 1];

      if (firstRecord <= 0 || lastRecord <= 0) {
        return Constants.DEFAULT_VALUE; //there are no b nodes for a node            
      }

      // binary search for bnode_Id in relevant records in node-node distance arrays
      int minIndex = firstRecord - 1;
      int maxIndex = lastRecord - 1;

      int index;
      int bNodeComp;

      do {
        index = (maxIndex + minIndex) / 2;
        bNodeComp = Global.NodePairBNodeId[index];

        if (bNodeComp < bNodeId) {
          minIndex = index + 1;
        } else if (bNodeComp > bNodeId) {
          maxIndex = index - 1;
        }
      } while (bNodeComp != bNodeId && maxIndex >= minIndex);

      if (bNodeComp != bNodeId) {
        return Constants.DEFAULT_VALUE; //there are no b nodes for a node            
      }

      double distance = Global.NodePairDistance[index] / 5280.0; // convert feet to miles

      Global.NodeNodePreviousDistance[threadAssignedIndex] = distance;

      return distance;
    }

    public double CircuityDistance(IParcelWrapper destination) {
      //added for intra-microzone distance, square root of area over 2
      if (Id == destination.Id && ThousandsSquareLengthUnits > Constants.EPSILON) {
        double calcDistance = (Global.Configuration.CorrectIntraParcelAreaToDistanceCalculation)
                  ? Math.Sqrt(1000 * ThousandsSquareLengthUnits) / (2.0 * 5280 * Global.Settings.DistanceUnitsPerMile)
                  : Math.Sqrt(ThousandsSquareLengthUnits) / (2.0);
        return Math.Max(calcDistance, Global.Configuration.MinimumIntraParcelDistanceCutoff);
      }
      // JLBscale:  change so calculations work in length units instead of ft.
      double maxCircLength = 10560.0 * Global.Settings.LengthUnitsPerFoot; // only apply circuity multiplier out to 2 miles = 10560 feet
      double lengthLimit1 = 2640.0 * Global.Settings.LengthUnitsPerFoot; // circuity distance 1 = 1/2 mile
      double lengthLimit2 = 5280.0 * Global.Settings.LengthUnitsPerFoot; // circuity distance 2 = 1 mile
      double lengthLimit3 = 7920.0 * Global.Settings.LengthUnitsPerFoot; // circuity distance 3 = 1 1/2 mile

      const double defaultCircuity = 1.4;

      int ox = XCoordinate;
      int oy = YCoordinate;
      int dx = destination.XCoordinate;
      int dy = destination.YCoordinate;

      double circuityRatio;

      double dWeight1 = 0.0;
      double dWeight2 = 0.0;
      double dWeight3 = 0.0;

      double xDiff = Math.Abs(dx - ox);
      double yDiff = Math.Abs(dy - oy);

      double xyLength = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);

      // JLBscale.  rescale from miles to distance units
      if (((xyLength / Global.Settings.LengthUnitsPerFoot) / 5280D) * Global.Settings.DistanceUnitsPerMile > Global.Configuration.MaximumBlendingDistance) {
        return ((xyLength / Global.Settings.LengthUnitsPerFoot) / 5280D) * Global.Settings.DistanceUnitsPerMile * defaultCircuity;
      }

      if (xyLength < lengthLimit1) {
        dWeight1 = 1.0;
      } else if (xyLength < lengthLimit2) {
        dWeight2 = (xyLength - lengthLimit1) / (lengthLimit2 - lengthLimit1);
        dWeight1 = 1.0 - dWeight2;
      } else if (xyLength < lengthLimit3) {
        dWeight3 = (xyLength - lengthLimit2) / (lengthLimit3 - lengthLimit2);
        dWeight2 = 1.0 - dWeight3;
      } else {
        dWeight3 = 1.0;
      }

      // Octant  dx-ox  dy-oy  Xdiff vs. Ydiff
      //1  E-NE   pos    pos     greater
      //2  N-NE   pos    pos     less
      //3  N-NW   neg    pos     less
      //4  W-NW   neg    pos     greater
      //5  W-SW   neg    neg     greater
      //6  S-SW   neg    neg     leYass
      //7  S-SE   pos    neg     less
      //8  E-SE   pos    neg     greater}

      if (Math.Abs(xDiff) < Constants.EPSILON && Math.Abs(yDiff) < Constants.EPSILON) {
        // same point
        circuityRatio = 1.0;
      } else if (Math.Abs(yDiff) < Constants.EPSILON) {
        // due E or W
        if (dx > ox) {
          // due E
          circuityRatio =
              dWeight1 * CircuityRatio_E1 +
              dWeight2 * CircuityRatio_E2 +
              dWeight3 * CircuityRatio_E3;
        } else {
          // due W
          circuityRatio =
              dWeight1 * CircuityRatio_W1 +
              dWeight2 * CircuityRatio_W2 +
              dWeight3 * CircuityRatio_W3;
        }
      } else if (Math.Abs(xDiff) < Constants.EPSILON) {
        // due N or S
        if (dy > oy) {
          // due N
          circuityRatio =
              dWeight1 * CircuityRatio_N1 +
              dWeight2 * CircuityRatio_N2 +
              dWeight3 * CircuityRatio_N3;
        } else {
          // due S
          circuityRatio =
              dWeight1 * CircuityRatio_S1 +
              dWeight2 * CircuityRatio_S2 +
              dWeight3 * CircuityRatio_S3;
        }
      } else if (dy > oy) {
        // towards N
        if (dx > ox) {
          // NE quadrant
          if (xDiff > yDiff) {
            // E-NE
            double odAngle = yDiff / xDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_NE1 + (1 - odAngle) * CircuityRatio_E1) +
                dWeight2 * (odAngle * CircuityRatio_NE2 + (1 - odAngle) * CircuityRatio_E2) +
                dWeight3 * (odAngle * CircuityRatio_NE3 + (1 - odAngle) * CircuityRatio_E3);
          } else {
            // N-NE
            double odAngle = xDiff / yDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_NE1 + (1 - odAngle) * CircuityRatio_N1) +
                dWeight2 * (odAngle * CircuityRatio_NE2 + (1 - odAngle) * CircuityRatio_N2) +
                dWeight3 * (odAngle * CircuityRatio_NE3 + (1 - odAngle) * CircuityRatio_N3);
          }
        } else {
          if (xDiff < yDiff) {
            // N-NW
            double odAngle = xDiff / yDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_NW1 + (1 - odAngle) * CircuityRatio_N1) +
                dWeight2 * (odAngle * CircuityRatio_NW2 + (1 - odAngle) * CircuityRatio_N2) +
                dWeight3 * (odAngle * CircuityRatio_NW3 + (1 - odAngle) * CircuityRatio_N3);
          } else {
            // W-NW
            double odAngle = yDiff / xDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_NW1 + (1 - odAngle) * CircuityRatio_W1) +
                dWeight2 * (odAngle * CircuityRatio_NW2 + (1 - odAngle) * CircuityRatio_W2) +
                dWeight3 * (odAngle * CircuityRatio_NW3 + (1 - odAngle) * CircuityRatio_W3);
          }
        }
      } else {
        // toward South
        if (dx < ox) {
          // SW quadrant
          if (xDiff > yDiff) {
            // W-SW
            double odAngle = yDiff / xDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_SW1 + (1 - odAngle) * CircuityRatio_W1) +
                dWeight2 * (odAngle * CircuityRatio_SW2 + (1 - odAngle) * CircuityRatio_W2) +
                dWeight3 * (odAngle * CircuityRatio_SW3 + (1 - odAngle) * CircuityRatio_W3);
          } else {
            // S-SW
            double odAngle = xDiff / yDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_SW1 + (1 - odAngle) * CircuityRatio_S1) +
                dWeight2 * (odAngle * CircuityRatio_SW2 + (1 - odAngle) * CircuityRatio_S2) +
                dWeight3 * (odAngle * CircuityRatio_SW3 + (1 - odAngle) * CircuityRatio_S3);
          }
        } else {
          if (xDiff < yDiff) {
            // S-SE
            double odAngle = xDiff / yDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_SE1 + (1 - odAngle) * CircuityRatio_S1) +
                dWeight2 * (odAngle * CircuityRatio_SE2 + (1 - odAngle) * CircuityRatio_S2) +
                dWeight3 * (odAngle * CircuityRatio_SE3 + (1 - odAngle) * CircuityRatio_S3);
          } else {
            // E-SE
            double odAngle = yDiff / xDiff;

            circuityRatio =
                dWeight1 * (odAngle * CircuityRatio_SE1 + (1 - odAngle) * CircuityRatio_E1) +
                dWeight2 * (odAngle * CircuityRatio_SE2 + (1 - odAngle) * CircuityRatio_E2) +
                dWeight3 * (odAngle * CircuityRatio_SE3 + (1 - odAngle) * CircuityRatio_E3);
          }
        }
      }

      if (xyLength < maxCircLength) {
        // JLBscale.  rescale from miles to distance units
        // return (xyLength * circuityRatio) / 5280D;
        return ((xyLength * circuityRatio / Global.Settings.LengthUnitsPerFoot) / 5280D) * Global.Settings.DistanceUnitsPerMile;
      }

      // default adjustment applied to portion of distance over maxCircDist
      // return (maxCircLength * circuityRatio + (xyLength - maxCircLength) * defaultCircuity) / 5280D;
      return (((maxCircLength * circuityRatio + (xyLength - maxCircLength) * defaultCircuity) / Global.Settings.LengthUnitsPerFoot) / 5280D) * Global.Settings.DistanceUnitsPerMile;
    }

    #endregion

    #region init/utility/export methods

    public override string ToString() {
      return string.Format("Parcel: {0}", Id);
    }

    private static double Log2(double var1, double var2) {
      if (var1 < Constants.EPSILON || var2 < Constants.EPSILON) {
        return 0.0;
      }

      double total = var1 + var2;

      return
          -1.0 *
          (var1 / total *
           Math.Log(var1 / total) +
           var2 / total *
           Math.Log(var2 / total)) /
          Math.Log(2.0);
    }

    private static double Log3(double var1, double var2, double var3) {
      if (var1 < Constants.EPSILON || var2 < Constants.EPSILON || var3 < Constants.EPSILON) {
        return 0.0;
      }

      double total = var1 + var2 + var3;

      return
          -1.0 *
          (var1 / total *
           Math.Log(var1 / total) +
           var2 / total *
           Math.Log(var2 / total) +
           var3 / total *
           Math.Log(var3 / total)) /
          Math.Log(4.0);
    }

    private static double Log4(double var1, double var2, double var3, double var4) {
      if (var1 < Constants.EPSILON || var2 < Constants.EPSILON || var3 < Constants.EPSILON || var4 < Constants.EPSILON) {
        return 0.0;
      }

      double total = var1 + var2 + var3 + var4;

      return
          -1.0 *
          (var1 / total *
           Math.Log(var1 / total) +
           var2 / total *
           Math.Log(var2 / total) +
           var3 / total *
           Math.Log(var3 / total) +
           var4 / total *
           Math.Log(var4 / total)) /
          Math.Log(4.0);
    }

    private static double MaxLog(double spaces, double total) {
      return Math.Log(1 + spaces * total / Math.Max(.001, spaces + total));
    }

    #endregion
  }
}
