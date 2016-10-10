using System;
using System.Collections.Generic;
using System.IO;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Extensions;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Roster;
using Daysim.ParkAndRideShadowPricing;
using Xunit;

namespace Daysim.Tests
{
	public class CondensedParcelTest 
	{
		[Fact]
		public void TestCondensedParcel()
		{
			const int id = 1;
			const int zoneId = 2;
			const int sequence = 3;
			const int xCoordinate = 4;
			const int yCoordinate = 5;
			const int transimsActivityLocation = 6;
			const int landUseCode19 = 7;
			const double households = 8;
			const double studentsK8 = 9;
			const double studentsHighSchool = 10;
			const double studentsK12 = 11;
			const double studentsUniversity = 12;
			const double employmentEducation = 13;
			const double employmentFood = 14;
			const double employmentGovernment = 15;
			const double employmentIndustrial = 16;
			const double employmentMedical = 17;
			const double employmentOffice = 18;
			const double employmentRetail = 19;
			const double employmentService = 20;
			const double employmentAgricultureConstruction = 21;
			const double employmentTotal = 22;
			const double parkingOffStreetPaidDailySpaces = 23;
			const double studentsK8Buffer1 = 24;
			const double studentsHighSchoolBuffer1 = 25;
			const double studentsK8Buffer2 = 26;
			const double studentsHighSchoolBuffer2 = 27;
			const double employmentFoodBuffer1 = 28;
			const double employmentMedicalBuffer1 = 29;
			const double employmentMedicalBuffer2 = 30;
			const double employmentRetailBuffer1 = 31;
			const double employmentServiceBuffer1 = 32;
			const double parkingOffStreetPaidDailyPriceBuffer1 = 33;
			const double parkingOffStreetPaidHourlyPriceBuffer1 = 34;
			const double parkingOffStreetPaidDailyPriceBuffer2 = 35;
			const double parkingOffStreetPaidHourlyPriceBuffer2 = 36;
			const double stopsTransitBuffer1 = 37;
			const double stopsTransitBuffer2 = 38;
			const double nodesSingleLinkBuffer1 = 39;
			const double nodesThreeLinksBuffer1 = 40;
			const double nodesFourLinksBuffer1 = 41;
			const double openSpaceType1Buffer1 = 42;
			const double openSpaceType2Buffer1 = 43;
			const double openSpaceType1Buffer2 = 44;
			const double openSpaceType2Buffer2 = 45;
			const double employmentFoodBuffer2 = 46;
			const double employmentRetailBuffer2 = 47;
			const double employmentServiceBuffer2 = 48;
			const double householdsBuffer2 = 49;
			const double nodesSingleLinkBuffer2 = 50;
			const double nodesThreeLinksBuffer2 = 51;
			const double nodesFourLinksBuffer2 = 52;
			const double distanceToLocalBus = 53;
			const double distanceToLightRail = 54;
			const double distanceToExpressBus = 55;
			const double distanceToCommuterRail = 56;
			const double distanceToFerry = 57;
			const double distanceToTransit = 58;
			const double shadowPriceForEmployment = 59;
			const double shadowPriceForStudentsK12 = 60;
			const double shadowPriceForStudentsUniversity = 61;
			const double externalEmploymentTotal = 62;
			const double employmentDifference = 63;
			const double employmentPrediction = 64;
			const double externalStudentsK12 = 65;
			const double studentsK12Difference = 66;
			const double studentsK12Prediction = 67;
			const double externalStudentsUniversity = 68;
			const double studentsUniversityDifference = 69;
			const double studentsUniversityPrediction = 70;
			const double parkingOffStreetPaidHourlySpaces = 71;
			const double employmentGovernmentBuffer1 = 72;
			const double employmentOfficeBuffer1 = 73;
			const double employmentGovernmentBuffer2 = 74;
			const double employmentOfficeBuffer2 = 75;
			const double employmentEducationBuffer1 = 76;
			const double employmentEducationBuffer2 = 77;
			const double employmentAgricultureConstructionBuffer1 = 78;
			const double employmentIndustrialBuffer1 = 79;
			const double employmentAgricultureConstructionBuffer2 = 80;
			const double employmentIndustrialBuffer2 = 81;
			const double employmentTotalBuffer1 = 82;
			const double employmentTotalBuffer2 = 83;
			const double householdsBuffer1 = 84;
			const double studentsUniversityBuffer1 = 85;
			const double studentsUniversityBuffer2 = 86;
			const double parkingOffStreetPaidHourlySpacesBuffer1 = 87;
			const double parkingOffStreetPaidDailySpacesBuffer1 = 88;
			const double parkingOffStreetPaidHourlySpacesBuffer2 = 89;
			const double parkingOffStreetPaidDailySpacesBuffer2 = 90;
			const double circuityRatio_E1 = 91;
			const double circuityRatio_E2 = 92;
			const double circuityRatio_E3 = 93;
			const double circuityRatio_NE1 = 94;
			const double circuityRatio_NE2 = 95;
			const double circuityRatio_NE3 = 96;
			const double circuityRatio_N1 = 97;
			const double circuityRatio_N2 = 98;
			const double circuityRatio_N3 = 99;
			const double circuityRatio_NW1 = 100;
			const double circuityRatio_NW2 = 101;
			const double circuityRatio_NW3 = 102;
			const double circuityRatio_W1 = 103;
			const double circuityRatio_W2 = 104;
			const double circuityRatio_W3 = 105;
			const double circuityRatio_SW1 = 106;
			const double circuityRatio_SW2 = 107;
			const double circuityRatio_SW3 = 108;
			const double circuityRatio_S1 = 109;
			const double circuityRatio_S2 = 110;
			const double circuityRatio_S3 = 111;
			const double circuityRatio_SE1 = 112;
			const double circuityRatio_SE2 = 113;
			const double circuityRatio_SE3 = 114;


			CondensedParcel condensedParcel = new CondensedParcel
				                                  {
					                                  Id = id,
					                                  ZoneId = zoneId,
					                                  Sequence = sequence,
					                                  XCoordinate = xCoordinate,
					                                  YCoordinate = yCoordinate,
					                                  TransimsActivityLocation = transimsActivityLocation,
					                                  LandUseCode19 = landUseCode19,
					                                  Households = households,
					                                  StudentsK8 = studentsK8,
					                                  StudentsHighSchool = studentsHighSchool,
					                                  StudentsK12 = studentsK12,
					                                  StudentsUniversity = studentsUniversity,
					                                  EmploymentEducation = employmentEducation,
					                                  EmploymentFood = employmentFood,
					                                  EmploymentGovernment = employmentGovernment,
					                                  EmploymentIndustrial = employmentIndustrial,
					                                  EmploymentMedical = employmentMedical,
					                                  EmploymentOffice = employmentOffice,
					                                  EmploymentRetail = employmentRetail,
					                                  EmploymentService = employmentService,
					                                  EmploymentAgricultureConstruction = employmentAgricultureConstruction,
					                                  EmploymentTotal = employmentTotal,
					                                  ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                                  StudentsK8Buffer1 = studentsK8Buffer1,
					                                  StudentsHighSchoolBuffer1 = studentsHighSchoolBuffer1,
					                                  StudentsK8Buffer2 = studentsK8Buffer2,
					                                  StudentsHighSchoolBuffer2 = studentsHighSchoolBuffer2,
					                                  EmploymentFoodBuffer1 = employmentFoodBuffer1,
					                                  EmploymentMedicalBuffer1 = employmentMedicalBuffer1,
					                                  EmploymentMedicalBuffer2 = employmentMedicalBuffer2,
					                                  EmploymentRetailBuffer1 = employmentRetailBuffer1,
					                                  EmploymentServiceBuffer1 = employmentServiceBuffer1,
					                                  ParkingOffStreetPaidDailyPriceBuffer1 = parkingOffStreetPaidDailyPriceBuffer1,
					                                  ParkingOffStreetPaidHourlyPriceBuffer1 = parkingOffStreetPaidHourlyPriceBuffer1,
					                                  ParkingOffStreetPaidDailyPriceBuffer2 = parkingOffStreetPaidDailyPriceBuffer2,
					                                  ParkingOffStreetPaidHourlyPriceBuffer2 = parkingOffStreetPaidHourlyPriceBuffer2,
					                                  StopsTransitBuffer1 = stopsTransitBuffer1,
					                                  StopsTransitBuffer2 = stopsTransitBuffer2,
					                                  NodesSingleLinkBuffer1 = nodesSingleLinkBuffer1,
					                                  NodesThreeLinksBuffer1 = nodesThreeLinksBuffer1,
					                                  NodesFourLinksBuffer1 = nodesFourLinksBuffer1,
					                                  OpenSpaceType1Buffer1 = openSpaceType1Buffer1,
					                                  OpenSpaceType2Buffer1 = openSpaceType2Buffer1,
					                                  OpenSpaceType1Buffer2 = openSpaceType1Buffer2,
					                                  OpenSpaceType2Buffer2 = openSpaceType2Buffer2,
					                                  EmploymentFoodBuffer2 = employmentFoodBuffer2,
					                                  EmploymentRetailBuffer2 = employmentRetailBuffer2,
					                                  EmploymentServiceBuffer2 = employmentServiceBuffer2,
					                                  HouseholdsBuffer2 = householdsBuffer2,
					                                  NodesSingleLinkBuffer2 = nodesSingleLinkBuffer2,
					                                  NodesThreeLinksBuffer2 = nodesThreeLinksBuffer2,
					                                  NodesFourLinksBuffer2 = nodesFourLinksBuffer2,
					                                  DistanceToLocalBus = distanceToLocalBus,
					                                  DistanceToLightRail = distanceToLightRail,
					                                  DistanceToExpressBus = distanceToExpressBus,
					                                  DistanceToCommuterRail = distanceToCommuterRail,
					                                  DistanceToFerry = distanceToFerry,
					                                  DistanceToTransit = distanceToTransit,
					                                  ShadowPriceForEmployment = shadowPriceForEmployment,
					                                  ShadowPriceForStudentsK12 = shadowPriceForStudentsK12,
					                                  ShadowPriceForStudentsUniversity = shadowPriceForStudentsUniversity,
					                                  ExternalEmploymentTotal = externalEmploymentTotal,
					                                  EmploymentDifference = employmentDifference,
					                                  EmploymentPrediction = employmentPrediction,
					                                  ExternalStudentsK12 = externalStudentsK12,
					                                  StudentsK12Difference = studentsK12Difference,
					                                  StudentsK12Prediction = studentsK12Prediction,
					                                  ExternalStudentsUniversity = externalStudentsUniversity,
					                                  StudentsUniversityDifference = studentsUniversityDifference,
					                                  StudentsUniversityPrediction = studentsUniversityPrediction,
					                                  ParkingOffStreetPaidHourlySpaces = parkingOffStreetPaidHourlySpaces,
					                                  EmploymentGovernmentBuffer1 = employmentGovernmentBuffer1,
					                                  EmploymentOfficeBuffer1 = employmentOfficeBuffer1,
					                                  EmploymentGovernmentBuffer2 = employmentGovernmentBuffer2,
					                                  EmploymentOfficeBuffer2 = employmentOfficeBuffer2,
					                                  EmploymentEducationBuffer1 = employmentEducationBuffer1,
					                                  EmploymentEducationBuffer2 = employmentEducationBuffer2,
					                                  EmploymentAgricultureConstructionBuffer1 =
						                                  employmentAgricultureConstructionBuffer1,
					                                  EmploymentIndustrialBuffer1 = employmentIndustrialBuffer1,
					                                  EmploymentAgricultureConstructionBuffer2 =
						                                  employmentAgricultureConstructionBuffer2,
					                                  EmploymentIndustrialBuffer2 = employmentIndustrialBuffer2,
					                                  EmploymentTotalBuffer1 = employmentTotalBuffer1,
					                                  EmploymentTotalBuffer2 = employmentTotalBuffer2,
					                                  HouseholdsBuffer1 = householdsBuffer1,
					                                  StudentsUniversityBuffer1 = studentsUniversityBuffer1,
					                                  StudentsUniversityBuffer2 = studentsUniversityBuffer2,
					                                  ParkingOffStreetPaidHourlySpacesBuffer1 = parkingOffStreetPaidHourlySpacesBuffer1,
					                                  ParkingOffStreetPaidDailySpacesBuffer1 = parkingOffStreetPaidDailySpacesBuffer1,
					                                  ParkingOffStreetPaidHourlySpacesBuffer2 = parkingOffStreetPaidHourlySpacesBuffer2,
					                                  ParkingOffStreetPaidDailySpacesBuffer2 = parkingOffStreetPaidDailySpacesBuffer2,
					                                  CircuityRatio_E1 = circuityRatio_E1,
					                                  CircuityRatio_E2 = circuityRatio_E2,
					                                  CircuityRatio_E3 = circuityRatio_E3,
					                                  CircuityRatio_NE1 = circuityRatio_NE1,
					                                  CircuityRatio_NE2 = circuityRatio_NE2,
					                                  CircuityRatio_NE3 = circuityRatio_NE3,
					                                  CircuityRatio_N1 = circuityRatio_N1,
					                                  CircuityRatio_N2 = circuityRatio_N2,
					                                  CircuityRatio_N3 = circuityRatio_N3,
					                                  CircuityRatio_NW1 = circuityRatio_NW1,
					                                  CircuityRatio_NW2 = circuityRatio_NW2,
					                                  CircuityRatio_NW3 = circuityRatio_NW3,
					                                  CircuityRatio_W1 = circuityRatio_W1,
					                                  CircuityRatio_W2 = circuityRatio_W2,
					                                  CircuityRatio_W3 = circuityRatio_W3,
					                                  CircuityRatio_SW1 = circuityRatio_SW1,
					                                  CircuityRatio_SW2 = circuityRatio_SW2,
					                                  CircuityRatio_SW3 = circuityRatio_SW3,
					                                  CircuityRatio_S1 = circuityRatio_S1,
					                                  CircuityRatio_S2 = circuityRatio_S2,
					                                  CircuityRatio_S3 = circuityRatio_S3,
					                                  CircuityRatio_SE1 = circuityRatio_SE1,
					                                  CircuityRatio_SE2 = circuityRatio_SE2,
					                                  CircuityRatio_SE3 = circuityRatio_SE3,
				                                  };

			Assert.Equal(id, condensedParcel.Id);
			Assert.Equal(zoneId, condensedParcel.ZoneId);
			Assert.Equal(sequence, condensedParcel.Sequence);
			Assert.Equal(xCoordinate, condensedParcel.XCoordinate);
			Assert.Equal(yCoordinate, condensedParcel.YCoordinate);
			Assert.Equal(transimsActivityLocation, condensedParcel.TransimsActivityLocation);
			Assert.Equal(landUseCode19, condensedParcel.LandUseCode19);
			Assert.Equal(households, condensedParcel.Households);
			Assert.Equal(studentsK8, condensedParcel.StudentsK8);
			Assert.Equal(studentsHighSchool, condensedParcel.StudentsHighSchool);
			Assert.Equal(studentsK12, condensedParcel.StudentsK12);
			Assert.Equal(studentsUniversity, condensedParcel.StudentsUniversity);
			Assert.Equal(employmentEducation, condensedParcel.EmploymentEducation);
			Assert.Equal(employmentFood, condensedParcel.EmploymentFood);
			Assert.Equal(employmentGovernment, condensedParcel.EmploymentGovernment);
			Assert.Equal(employmentIndustrial, condensedParcel.EmploymentIndustrial);
			Assert.Equal(employmentMedical, condensedParcel.EmploymentMedical);
			Assert.Equal(employmentOffice, condensedParcel.EmploymentOffice);
			Assert.Equal(employmentRetail, condensedParcel.EmploymentRetail);
			Assert.Equal(employmentService, condensedParcel.EmploymentService);
			Assert.Equal(employmentAgricultureConstruction, condensedParcel.EmploymentAgricultureConstruction);
			Assert.Equal(employmentTotal, condensedParcel.EmploymentTotal);
			Assert.Equal(parkingOffStreetPaidDailySpaces, condensedParcel.ParkingOffStreetPaidDailySpaces);
			Assert.Equal(studentsK8Buffer1, condensedParcel.StudentsK8Buffer1);
			Assert.Equal(studentsHighSchoolBuffer1, condensedParcel.StudentsHighSchoolBuffer1);
			Assert.Equal(studentsK8Buffer2, condensedParcel.StudentsK8Buffer2);
			Assert.Equal(studentsHighSchoolBuffer2, condensedParcel.StudentsHighSchoolBuffer2);
			Assert.Equal(employmentFoodBuffer1, condensedParcel.EmploymentFoodBuffer1);
			Assert.Equal(employmentMedicalBuffer1, condensedParcel.EmploymentMedicalBuffer1);
			Assert.Equal(employmentMedicalBuffer2, condensedParcel.EmploymentMedicalBuffer2);
			Assert.Equal(employmentRetailBuffer1, condensedParcel.EmploymentRetailBuffer1);
			Assert.Equal(employmentServiceBuffer1, condensedParcel.EmploymentServiceBuffer1);
			Assert.Equal(parkingOffStreetPaidDailyPriceBuffer1, condensedParcel.ParkingOffStreetPaidDailyPriceBuffer1);
			Assert.Equal(parkingOffStreetPaidHourlyPriceBuffer1, condensedParcel.ParkingOffStreetPaidHourlyPriceBuffer1);
			Assert.Equal(parkingOffStreetPaidDailyPriceBuffer2, condensedParcel.ParkingOffStreetPaidDailyPriceBuffer2);
			Assert.Equal(parkingOffStreetPaidHourlyPriceBuffer2, condensedParcel.ParkingOffStreetPaidHourlyPriceBuffer2);
			Assert.Equal(stopsTransitBuffer1, condensedParcel.StopsTransitBuffer1);
			Assert.Equal(stopsTransitBuffer2, condensedParcel.StopsTransitBuffer2);
			Assert.Equal(nodesSingleLinkBuffer1, condensedParcel.NodesSingleLinkBuffer1);
			Assert.Equal(nodesThreeLinksBuffer1, condensedParcel.NodesThreeLinksBuffer1);
			Assert.Equal(nodesFourLinksBuffer1, condensedParcel.NodesFourLinksBuffer1);
			Assert.Equal(openSpaceType1Buffer1, condensedParcel.OpenSpaceType1Buffer1);
			Assert.Equal(openSpaceType2Buffer1, condensedParcel.OpenSpaceType2Buffer1);
			Assert.Equal(openSpaceType1Buffer2, condensedParcel.OpenSpaceType1Buffer2);
			Assert.Equal(openSpaceType2Buffer2, condensedParcel.OpenSpaceType2Buffer2);
			Assert.Equal(employmentFoodBuffer2, condensedParcel.EmploymentFoodBuffer2);
			Assert.Equal(employmentRetailBuffer2, condensedParcel.EmploymentRetailBuffer2);
			Assert.Equal(employmentServiceBuffer2, condensedParcel.EmploymentServiceBuffer2);
			Assert.Equal(householdsBuffer2, condensedParcel.HouseholdsBuffer2);
			Assert.Equal(nodesSingleLinkBuffer2, condensedParcel.NodesSingleLinkBuffer2);
			Assert.Equal(nodesThreeLinksBuffer2, condensedParcel.NodesThreeLinksBuffer2);
			Assert.Equal(nodesFourLinksBuffer2, condensedParcel.NodesFourLinksBuffer2);
			Assert.Equal(distanceToLocalBus, condensedParcel.DistanceToLocalBus);
			Assert.Equal(distanceToLightRail, condensedParcel.DistanceToLightRail);
			Assert.Equal(distanceToExpressBus, condensedParcel.DistanceToExpressBus);
			Assert.Equal(distanceToCommuterRail, condensedParcel.DistanceToCommuterRail);
			Assert.Equal(distanceToFerry, condensedParcel.DistanceToFerry);
			Assert.Equal(distanceToTransit, condensedParcel.DistanceToTransit);
			Assert.Equal(shadowPriceForEmployment, condensedParcel.ShadowPriceForEmployment);
			Assert.Equal(shadowPriceForStudentsK12, condensedParcel.ShadowPriceForStudentsK12);
			Assert.Equal(shadowPriceForStudentsUniversity, condensedParcel.ShadowPriceForStudentsUniversity);
			Assert.Equal(externalEmploymentTotal, condensedParcel.ExternalEmploymentTotal);
			Assert.Equal(employmentDifference, condensedParcel.EmploymentDifference);
			Assert.Equal(employmentPrediction, condensedParcel.EmploymentPrediction);
			Assert.Equal(externalStudentsK12, condensedParcel.ExternalStudentsK12);
			Assert.Equal(studentsK12Difference, condensedParcel.StudentsK12Difference);
			Assert.Equal(studentsK12Prediction, condensedParcel.StudentsK12Prediction);
			Assert.Equal(externalStudentsUniversity, condensedParcel.ExternalStudentsUniversity);
			Assert.Equal(studentsUniversityDifference, condensedParcel.StudentsUniversityDifference);
			Assert.Equal(studentsUniversityPrediction, condensedParcel.StudentsUniversityPrediction);
			Assert.Equal(parkingOffStreetPaidHourlySpaces, condensedParcel.ParkingOffStreetPaidHourlySpaces);
			Assert.Equal(employmentGovernmentBuffer1, condensedParcel.EmploymentGovernmentBuffer1);
			Assert.Equal(employmentOfficeBuffer1, condensedParcel.EmploymentOfficeBuffer1);
			Assert.Equal(employmentGovernmentBuffer2, condensedParcel.EmploymentGovernmentBuffer2);
			Assert.Equal(employmentOfficeBuffer2, condensedParcel.EmploymentOfficeBuffer2);
			Assert.Equal(employmentEducationBuffer1, condensedParcel.EmploymentEducationBuffer1);
			Assert.Equal(employmentEducationBuffer2, condensedParcel.EmploymentEducationBuffer2);
			Assert.Equal(employmentAgricultureConstructionBuffer1, condensedParcel.EmploymentAgricultureConstructionBuffer1);
			Assert.Equal(employmentIndustrialBuffer1, condensedParcel.EmploymentIndustrialBuffer1);
			Assert.Equal(employmentAgricultureConstructionBuffer2, condensedParcel.EmploymentAgricultureConstructionBuffer2);
			Assert.Equal(employmentIndustrialBuffer2, condensedParcel.EmploymentIndustrialBuffer2);
			Assert.Equal(employmentTotalBuffer1, condensedParcel.EmploymentTotalBuffer1);
			Assert.Equal(employmentTotalBuffer2, condensedParcel.EmploymentTotalBuffer2);
			Assert.Equal(householdsBuffer1, condensedParcel.HouseholdsBuffer1);
			Assert.Equal(studentsUniversityBuffer1, condensedParcel.StudentsUniversityBuffer1);
			Assert.Equal(studentsUniversityBuffer2, condensedParcel.StudentsUniversityBuffer2);
			Assert.Equal(parkingOffStreetPaidHourlySpacesBuffer1, condensedParcel.ParkingOffStreetPaidHourlySpacesBuffer1);
			Assert.Equal(parkingOffStreetPaidDailySpacesBuffer1, condensedParcel.ParkingOffStreetPaidDailySpacesBuffer1);
			Assert.Equal(parkingOffStreetPaidHourlySpacesBuffer2, condensedParcel.ParkingOffStreetPaidHourlySpacesBuffer2);
			Assert.Equal(parkingOffStreetPaidDailySpacesBuffer2, condensedParcel.ParkingOffStreetPaidDailySpacesBuffer2);
			Assert.Equal(circuityRatio_E1, condensedParcel.CircuityRatio_E1);
			Assert.Equal(circuityRatio_E2, condensedParcel.CircuityRatio_E2);
			Assert.Equal(circuityRatio_E3, condensedParcel.CircuityRatio_E3);
			Assert.Equal(circuityRatio_NE1, condensedParcel.CircuityRatio_NE1);
			Assert.Equal(circuityRatio_NE2, condensedParcel.CircuityRatio_NE2);
			Assert.Equal(circuityRatio_NE3, condensedParcel.CircuityRatio_NE3);
			Assert.Equal(circuityRatio_N1, condensedParcel.CircuityRatio_N1);
			Assert.Equal(circuityRatio_N2, condensedParcel.CircuityRatio_N2);
			Assert.Equal(circuityRatio_N3, condensedParcel.CircuityRatio_N3);
			Assert.Equal(circuityRatio_NW1, condensedParcel.CircuityRatio_NW1);
			Assert.Equal(circuityRatio_NW2, condensedParcel.CircuityRatio_NW2);
			Assert.Equal(circuityRatio_NW3, condensedParcel.CircuityRatio_NW3);
			Assert.Equal(circuityRatio_W1, condensedParcel.CircuityRatio_W1);
			Assert.Equal(circuityRatio_W2, condensedParcel.CircuityRatio_W2);
			Assert.Equal(circuityRatio_W3, condensedParcel.CircuityRatio_W3);
			Assert.Equal(circuityRatio_SW1, condensedParcel.CircuityRatio_SW1);
			Assert.Equal(circuityRatio_SW2, condensedParcel.CircuityRatio_SW2);
			Assert.Equal(circuityRatio_SW3, condensedParcel.CircuityRatio_SW3);
			Assert.Equal(circuityRatio_S1, condensedParcel.CircuityRatio_S1);
			Assert.Equal(circuityRatio_S2, condensedParcel.CircuityRatio_S2);
			Assert.Equal(circuityRatio_S3, condensedParcel.CircuityRatio_S3);
			Assert.Equal(circuityRatio_SE1, condensedParcel.CircuityRatio_SE1);
			Assert.Equal(circuityRatio_SE2, condensedParcel.CircuityRatio_SE2);
			Assert.Equal(circuityRatio_SE3, condensedParcel.CircuityRatio_SE3);
		}

		[Fact]
		public void TestCondensedParcelExtensions()
		{
			const int id = 1;
			const int zoneId = 2;
			const int sequence = 3;
			const int xCoordinate = 4;
			const int yCoordinate = 5;
			const int transimsActivityLocation = 6;
			const int landUseCode19 = 7;
			double households = 8;
			const double studentsK8 = 9;
			const double studentsHighSchool = 10;
			const double studentsK12 = 11;
			const double studentsUniversity = 12;
			const double employmentEducation = 13;
			const double employmentFood = 14;
			const double employmentGovernment = 15;
			const double employmentIndustrial = 16;
			const double employmentMedical = 17;
			const double employmentOffice = 18;
			const double employmentRetail = 19;
			const double employmentService = 20;
			const double employmentAgricultureConstruction = 21;
			const double employmentTotal = 22;
			const double parkingOffStreetPaidDailySpaces = 23;
			const double studentsK8Buffer1 = 24;
			const double studentsHighSchoolBuffer1 = 25;
			const double studentsK8Buffer2 = 26;
			const double studentsHighSchoolBuffer2 = 27;
			const double employmentFoodBuffer1 = 28;
			const double employmentMedicalBuffer1 = 29;
			const double employmentMedicalBuffer2 = 30;
			const double employmentRetailBuffer1 = 31;
			const double employmentServiceBuffer1 = 32;
			const double parkingOffStreetPaidDailyPriceBuffer1 = 33;
			const double parkingOffStreetPaidHourlyPriceBuffer1 = 34;
			const double parkingOffStreetPaidDailyPriceBuffer2 = 35;
			const double parkingOffStreetPaidHourlyPriceBuffer2 = 36;
			const double stopsTransitBuffer1 = 37;
			const double stopsTransitBuffer2 = 38;
			const double nodesSingleLinkBuffer1 = 39;
			const double nodesThreeLinksBuffer1 = 40;
			const double nodesFourLinksBuffer1 = 41;
			const double openSpaceType1Buffer1 = 42;
			const double openSpaceType2Buffer1 = 43;
			const double openSpaceType1Buffer2 = 44;
			const double openSpaceType2Buffer2 = 45;
			const double employmentFoodBuffer2 = 46;
			const double employmentRetailBuffer2 = 47;
			const double employmentServiceBuffer2 = 48;
			const double householdsBuffer2 = 49;
			const double nodesSingleLinkBuffer2 = 50;
			const double nodesThreeLinksBuffer2 = 51;
			const double nodesFourLinksBuffer2 = 52;
			const double distanceToLocalBus = 53;
			const double distanceToLightRail = 54;
			const double distanceToExpressBus = 55;
			const double distanceToCommuterRail = 56;
			const double distanceToFerry = 57;
			const double distanceToTransit = 58;
			const double shadowPriceForEmployment = 59;
			const double shadowPriceForStudentsK12 = 60;
			const double shadowPriceForStudentsUniversity = 61;
			const double externalEmploymentTotal = 62;
			const double employmentDifference = 63;
			const double employmentPrediction = 64;
			const double externalStudentsK12 = 65;
			const double studentsK12Difference = 66;
			const double studentsK12Prediction = 67;
			const double externalStudentsUniversity = 68;
			const double studentsUniversityDifference = 69;
			const double studentsUniversityPrediction = 70;
			const double parkingOffStreetPaidHourlySpaces = 71;
			const double employmentGovernmentBuffer1 = 72;
			const double employmentOfficeBuffer1 = 73;
			const double employmentGovernmentBuffer2 = 74;
			const double employmentOfficeBuffer2 = 75;
			const double employmentEducationBuffer1 = 76;
			const double employmentEducationBuffer2 = 77;
			const double employmentAgricultureConstructionBuffer1 = 78;
			const double employmentIndustrialBuffer1 = 79;
			const double employmentAgricultureConstructionBuffer2 = 80;
			const double employmentIndustrialBuffer2 = 81;
			const double employmentTotalBuffer1 = 82;
			const double employmentTotalBuffer2 = 83;
			const double householdsBuffer1 = 84;
			const double studentsUniversityBuffer1 = 85;
			const double studentsUniversityBuffer2 = 86;
			const double parkingOffStreetPaidHourlySpacesBuffer1 = 87;
			const double parkingOffStreetPaidDailySpacesBuffer1 = 88;
			const double parkingOffStreetPaidHourlySpacesBuffer2 = 89;
			const double parkingOffStreetPaidDailySpacesBuffer2 = 90;
			const double circuityRatio_E1 = 91;
			const double circuityRatio_E2 = 92;
			const double circuityRatio_E3 = 93;
			const double circuityRatio_NE1 = 94;
			const double circuityRatio_NE2 = 95;
			const double circuityRatio_NE3 = 96;
			const double circuityRatio_N1 = 97;
			const double circuityRatio_N2 = 98;
			const double circuityRatio_N3 = 99;
			const double circuityRatio_NW1 = 100;
			const double circuityRatio_NW2 = 101;
			const double circuityRatio_NW3 = 102;
			const double circuityRatio_W1 = 103;
			const double circuityRatio_W2 = 104;
			const double circuityRatio_W3 = 105;
			const double circuityRatio_SW1 = 106;
			const double circuityRatio_SW2 = 107;
			const double circuityRatio_SW3 = 108;
			const double circuityRatio_S1 = 109;
			const double circuityRatio_S2 = 110;
			const double circuityRatio_S3 = 111;
			const double circuityRatio_SE1 = 112;
			const double circuityRatio_SE2 = 113;
			const double circuityRatio_SE3 = 114;


			CondensedParcel parcel = new CondensedParcel
				                         {
					                         Id = id,
					                         ZoneId = zoneId,
					                         Sequence = sequence,
					                         XCoordinate = xCoordinate,
					                         YCoordinate = yCoordinate,
					                         TransimsActivityLocation = transimsActivityLocation,
					                         LandUseCode19 = landUseCode19,
					                         Households = households,
					                         StudentsK8 = studentsK8,
					                         StudentsHighSchool = studentsHighSchool,
					                         StudentsK12 = studentsK12,
					                         StudentsUniversity = studentsUniversity,
					                         EmploymentEducation = employmentEducation,
					                         EmploymentFood = employmentFood,
					                         EmploymentGovernment = employmentGovernment,
					                         EmploymentIndustrial = employmentIndustrial,
					                         EmploymentMedical = employmentMedical,
					                         EmploymentOffice = employmentOffice,
					                         EmploymentRetail = employmentRetail,
					                         EmploymentService = employmentService,
					                         EmploymentAgricultureConstruction = employmentAgricultureConstruction,
					                         EmploymentTotal = employmentTotal,
					                         ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                         StudentsK8Buffer1 = studentsK8Buffer1,
					                         StudentsHighSchoolBuffer1 = studentsHighSchoolBuffer1,
					                         StudentsK8Buffer2 = studentsK8Buffer2,
					                         StudentsHighSchoolBuffer2 = studentsHighSchoolBuffer2,
					                         EmploymentFoodBuffer1 = employmentFoodBuffer1,
					                         EmploymentMedicalBuffer1 = employmentMedicalBuffer1,
					                         EmploymentMedicalBuffer2 = employmentMedicalBuffer2,
					                         EmploymentRetailBuffer1 = employmentRetailBuffer1,
					                         EmploymentServiceBuffer1 = employmentServiceBuffer1,
					                         ParkingOffStreetPaidDailyPriceBuffer1 = parkingOffStreetPaidDailyPriceBuffer1,
					                         ParkingOffStreetPaidHourlyPriceBuffer1 = parkingOffStreetPaidHourlyPriceBuffer1,
					                         ParkingOffStreetPaidDailyPriceBuffer2 = parkingOffStreetPaidDailyPriceBuffer2,
					                         ParkingOffStreetPaidHourlyPriceBuffer2 = parkingOffStreetPaidHourlyPriceBuffer2,
					                         StopsTransitBuffer1 = stopsTransitBuffer1,
					                         StopsTransitBuffer2 = stopsTransitBuffer2,
					                         NodesSingleLinkBuffer1 = nodesSingleLinkBuffer1,
					                         NodesThreeLinksBuffer1 = nodesThreeLinksBuffer1,
					                         NodesFourLinksBuffer1 = nodesFourLinksBuffer1,
					                         OpenSpaceType1Buffer1 = openSpaceType1Buffer1,
					                         OpenSpaceType2Buffer1 = openSpaceType2Buffer1,
					                         OpenSpaceType1Buffer2 = openSpaceType1Buffer2,
					                         OpenSpaceType2Buffer2 = openSpaceType2Buffer2,
					                         EmploymentFoodBuffer2 = employmentFoodBuffer2,
					                         EmploymentRetailBuffer2 = employmentRetailBuffer2,
					                         EmploymentServiceBuffer2 = employmentServiceBuffer2,
					                         HouseholdsBuffer2 = householdsBuffer2,
					                         NodesSingleLinkBuffer2 = nodesSingleLinkBuffer2,
					                         NodesThreeLinksBuffer2 = nodesThreeLinksBuffer2,
					                         NodesFourLinksBuffer2 = nodesFourLinksBuffer2,
					                         DistanceToLocalBus = distanceToLocalBus,
					                         DistanceToLightRail = distanceToLightRail,
					                         DistanceToExpressBus = distanceToExpressBus,
					                         DistanceToCommuterRail = distanceToCommuterRail,
					                         DistanceToFerry = distanceToFerry,
					                         DistanceToTransit = distanceToTransit,
					                         ShadowPriceForEmployment = shadowPriceForEmployment,
					                         ShadowPriceForStudentsK12 = shadowPriceForStudentsK12,
					                         ShadowPriceForStudentsUniversity = shadowPriceForStudentsUniversity,
					                         ExternalEmploymentTotal = externalEmploymentTotal,
					                         EmploymentDifference = employmentDifference,
					                         EmploymentPrediction = employmentPrediction,
					                         ExternalStudentsK12 = externalStudentsK12,
					                         StudentsK12Difference = studentsK12Difference,
					                         StudentsK12Prediction = studentsK12Prediction,
					                         ExternalStudentsUniversity = externalStudentsUniversity,
					                         StudentsUniversityDifference = studentsUniversityDifference,
					                         StudentsUniversityPrediction = studentsUniversityPrediction,
					                         ParkingOffStreetPaidHourlySpaces = parkingOffStreetPaidHourlySpaces,
					                         EmploymentGovernmentBuffer1 = employmentGovernmentBuffer1,
					                         EmploymentOfficeBuffer1 = employmentOfficeBuffer1,
					                         EmploymentGovernmentBuffer2 = employmentGovernmentBuffer2,
					                         EmploymentOfficeBuffer2 = employmentOfficeBuffer2,
					                         EmploymentEducationBuffer1 = employmentEducationBuffer1,
					                         EmploymentEducationBuffer2 = employmentEducationBuffer2,
					                         EmploymentAgricultureConstructionBuffer1 =
						                         employmentAgricultureConstructionBuffer1,
					                         EmploymentIndustrialBuffer1 = employmentIndustrialBuffer1,
					                         EmploymentAgricultureConstructionBuffer2 =
						                         employmentAgricultureConstructionBuffer2,
					                         EmploymentIndustrialBuffer2 = employmentIndustrialBuffer2,
					                         EmploymentTotalBuffer1 = employmentTotalBuffer1,
					                         EmploymentTotalBuffer2 = employmentTotalBuffer2,
					                         HouseholdsBuffer1 = householdsBuffer1,
					                         StudentsUniversityBuffer1 = studentsUniversityBuffer1,
					                         StudentsUniversityBuffer2 = studentsUniversityBuffer2,
					                         ParkingOffStreetPaidHourlySpacesBuffer1 = parkingOffStreetPaidHourlySpacesBuffer1,
					                         ParkingOffStreetPaidDailySpacesBuffer1 = parkingOffStreetPaidDailySpacesBuffer1,
					                         ParkingOffStreetPaidHourlySpacesBuffer2 = parkingOffStreetPaidHourlySpacesBuffer2,
					                         ParkingOffStreetPaidDailySpacesBuffer2 = parkingOffStreetPaidDailySpacesBuffer2,
					                         CircuityRatio_E1 = circuityRatio_E1,
					                         CircuityRatio_E2 = circuityRatio_E2,
					                         CircuityRatio_E3 = circuityRatio_E3,
					                         CircuityRatio_NE1 = circuityRatio_NE1,
					                         CircuityRatio_NE2 = circuityRatio_NE2,
					                         CircuityRatio_NE3 = circuityRatio_NE3,
					                         CircuityRatio_N1 = circuityRatio_N1,
					                         CircuityRatio_N2 = circuityRatio_N2,
					                         CircuityRatio_N3 = circuityRatio_N3,
					                         CircuityRatio_NW1 = circuityRatio_NW1,
					                         CircuityRatio_NW2 = circuityRatio_NW2,
					                         CircuityRatio_NW3 = circuityRatio_NW3,
					                         CircuityRatio_W1 = circuityRatio_W1,
					                         CircuityRatio_W2 = circuityRatio_W2,
					                         CircuityRatio_W3 = circuityRatio_W3,
					                         CircuityRatio_SW1 = circuityRatio_SW1,
					                         CircuityRatio_SW2 = circuityRatio_SW2,
					                         CircuityRatio_SW3 = circuityRatio_SW3,
					                         CircuityRatio_S1 = circuityRatio_S1,
					                         CircuityRatio_S2 = circuityRatio_S2,
					                         CircuityRatio_S3 = circuityRatio_S3,
					                         CircuityRatio_SE1 = circuityRatio_SE1,
					                         CircuityRatio_SE2 = circuityRatio_SE2,
					                         CircuityRatio_SE3 = circuityRatio_SE3,
				                         };
			double ans = (nodesThreeLinksBuffer1 + nodesFourLinksBuffer1)/
			             (Math.Max(1, nodesSingleLinkBuffer1 + nodesThreeLinksBuffer1 + nodesFourLinksBuffer1));
			Assert.Equal(ans, parcel.C34RatioBuffer1());

			ans = (nodesThreeLinksBuffer1 + nodesFourLinksBuffer1) - nodesSingleLinkBuffer1;
			Assert.Equal(ans, parcel.NetIntersectionDensity1());

			ans = (nodesThreeLinksBuffer2 + nodesFourLinksBuffer2) - nodesSingleLinkBuffer2;
			Assert.Equal(ans, parcel.NetIntersectionDensity2());

			ans = openSpaceType1Buffer1;
			Assert.Equal(ans, parcel.OpenSpaceDensity1());

			ans = openSpaceType1Buffer2;
			Assert.Equal(ans, parcel.OpenSpaceDensity2());

			ans = parcel.OpenSpaceType1Buffer1*parcel.OpenSpaceType2Buffer1/1000000.0;
			Assert.Equal(ans, parcel.OpenSpaceMillionSqFtBuffer1());

			ans = parcel.OpenSpaceType1Buffer2*parcel.OpenSpaceType2Buffer2/1000000.0;
			Assert.Equal(ans, parcel.OpenSpaceMillionSqFtBuffer2());

			ans = employmentRetailBuffer1 + employmentFoodBuffer1;
			Assert.Equal(ans, parcel.RetailEmploymentDensity1());

			ans = employmentRetailBuffer2 + employmentFoodBuffer2;
			Assert.Equal(ans, parcel.RetailEmploymentDensity2());

			ans = employmentServiceBuffer1 + employmentMedicalBuffer1;
			Assert.Equal(ans, parcel.ServiceEmploymentDensity1());

			ans = employmentServiceBuffer2 + employmentMedicalBuffer2;
			Assert.Equal(ans, parcel.ServiceEmploymentDensity2());

			ans = employmentOfficeBuffer1 + employmentGovernmentBuffer1;
			Assert.Equal(ans, parcel.OfficeEmploymentDensity1());

			ans = employmentOfficeBuffer2 + employmentGovernmentBuffer2;
			Assert.Equal(ans, parcel.OfficeEmploymentDensity2());

			ans = employmentTotalBuffer1;
			Assert.Equal(ans, parcel.TotalEmploymentDensity1());

			ans = employmentTotalBuffer2;
			Assert.Equal(ans, parcel.TotalEmploymentDensity2());

			ans = studentsHighSchoolBuffer1 + studentsUniversityBuffer1 + studentsK8Buffer1;
			Assert.Equal(ans, parcel.StudentEnrolmentDensity1());

			ans = studentsHighSchoolBuffer2 + studentsUniversityBuffer2 + studentsK8Buffer2;
			Assert.Equal(ans, parcel.StudentEnrolmentDensity2());

			ans = householdsBuffer1;
			Assert.Equal(ans, parcel.HouseholdDensity1());

			ans = householdsBuffer2;
			Assert.Equal(ans, parcel.HouseholdDensity2());

			var total = householdsBuffer1 + employmentTotalBuffer1;
			ans = -1.0*(householdsBuffer1/total*Math.Log(householdsBuffer1/total)
			            + employmentTotalBuffer1/total*Math.Log(employmentTotalBuffer1/total))/Math.Log(2.0);
			Assert.Equal(ans, parcel.MixedUse2Index1());

			total = householdsBuffer2 + employmentTotalBuffer2;
			ans = -1.0*(householdsBuffer2/total*Math.Log(householdsBuffer2/total)
			            + employmentTotalBuffer2/total*Math.Log(employmentTotalBuffer2/total))/Math.Log(2.0);
			Assert.Equal(ans, parcel.MixedUse2Index2());

			var hh = parcel.HouseholdDensity2();
			var ret = parcel.RetailEmploymentDensity2();
			var svc = parcel.ServiceEmploymentDensity2();
			total = hh + ret + svc;
			ans = -1.0*(hh/total*Math.Log(hh/total)
			            + ret/total*Math.Log(ret/total)
			            + svc/total*Math.Log(svc/total))/Math.Log(4.0);
			Assert.Equal(ans, parcel.MixedUse3Index2());

			hh = parcel.HouseholdDensity1();
			ret = parcel.RetailEmploymentDensity1();
			svc = parcel.ServiceEmploymentDensity1();
			var ofc = parcel.OfficeEmploymentDensity1();
			total = hh + ret + svc + ofc;
			ans = -1.0*(hh/total*Math.Log(hh/total)
			            + ret/total*Math.Log(ret/total)
			            + svc/total*Math.Log(svc/total)
			            + ofc/total*Math.Log(ofc/total))/Math.Log(4.0);
			Assert.Equal(ans, parcel.MixedUse4Index1());

			hh = parcel.HouseholdDensity2();
			ret = parcel.RetailEmploymentDensity2();
			svc = parcel.ServiceEmploymentDensity2();
			ofc = parcel.OfficeEmploymentDensity2();
			total = hh + ret + svc + ofc;
			ans = -1.0*(hh/total*Math.Log(hh/total)
			            + ret/total*Math.Log(ret/total)
			            + svc/total*Math.Log(svc/total)
			            + ofc/total*Math.Log(ofc/total))/Math.Log(4.0);
			Assert.Equal(ans, parcel.MixedUse4Index2());

			Global.Configuration = new Configuration();
			Assert.Equal(Constants.TransitAccess.NONE, parcel.TransitAccessSegment());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit*2;
			Assert.Equal(Constants.TransitAccess.GT_QTR_MI_AND_LTE_H_MI, parcel.TransitAccessSegment());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit*4;
			Assert.Equal(Constants.TransitAccess.GT_0_AND_LTE_QTR_MI, parcel.TransitAccessSegment());

			Global.Configuration.DistanceUnitsPerMile = 999999;
			Assert.Equal(Constants.TransitAccess.GT_0_AND_LTE_QTR_MI, parcel.TransitAccessSegment());

			ans =
				Math.Log(1 + parkingOffStreetPaidDailySpaces*employmentTotal/Math.Max(.001, parkingOffStreetPaidDailySpaces + employmentTotal));
			Assert.Equal(ans, parcel.ParcelParkingPerTotalEmployment());

			var spaces = parcel.ParkingOffStreetPaidHourlySpaces;
			var emp = parcel.EmploymentFood + parcel.EmploymentRetail + parcel.EmploymentService + parcel.EmploymentMedical;
			total = spaces + emp;
			ans = -1.0 * (spaces / total * Math.Log(spaces / total)
			               + emp / total * Math.Log(emp / total)) / Math.Log(2.0);
			Assert.Equal(ans, parcel.ParkingHourlyEmploymentCommercialMixInParcel());

			spaces = parcel.ParkingOffStreetPaidHourlySpacesBuffer1;
			emp = parcel.EmploymentFoodBuffer1 + parcel.EmploymentRetailBuffer1 + parcel.EmploymentServiceBuffer1 + parcel.EmploymentMedicalBuffer1;
			total = spaces + emp;
			ans = -1.0 * (spaces / total * Math.Log(spaces / total)
			               + emp / total * Math.Log(emp / total)) / Math.Log(2.0);
			Assert.Equal(ans, parcel.ParkingHourlyEmploymentCommercialMixBuffer1());

			spaces = parcel.ParkingOffStreetPaidDailySpaces;
			emp = parcel.EmploymentTotal;
			total = spaces + emp;
			ans = -1.0 * (spaces / total * Math.Log(spaces / total)
			               + emp / total * Math.Log(emp / total)) / Math.Log(2.0);
			Assert.Equal(ans, parcel.ParkingDailyEmploymentTotalMixInParcel());

			spaces = parcel.ParkingOffStreetPaidDailySpacesBuffer1;
			emp = parcel.EmploymentTotalBuffer1;
			total = spaces + emp;
			ans =-1.0 * (spaces / total * Math.Log(spaces / total)
			               + emp / total * Math.Log(emp / total)) / Math.Log(2.0);
			Assert.Equal(ans, parcel.ParkingDailyEmploymentTotalMixBuffer1());

			total = parkingOffStreetPaidDailySpacesBuffer1 + employmentTotalBuffer1;
			ans = -1.0 * (parkingOffStreetPaidDailySpacesBuffer1 / total * Math.Log(parkingOffStreetPaidDailySpacesBuffer1 / total)
			               + employmentTotalBuffer1 / total * Math.Log(employmentTotalBuffer1 / total)) / Math.Log(2.0);
			Assert.Equal(ans, parcel.ParkingDailyEmploymentTotalMixBuffer1());

			spaces = parcel.ParkingOffStreetPaidDailySpaces;
			total = parcel.EmploymentFood + parcel.EmploymentRetail + parcel.EmploymentService + parcel.EmploymentMedical;
			ans = Math.Log(1 + spaces * total / Math.Max(.001, spaces + total));
			Assert.Equal(ans, parcel.ParcelParkingPerFoodRetailServiceMedicalEmployment());

			ZoneTotals zoneTotals = new ZoneTotals {StudentsK12 = 5, StudentsUniversity = 6};
			const double millionsSquareLengthUnits = 2.01;
			spaces = parcel.ParkingOffStreetPaidDailySpaces / millionsSquareLengthUnits;
			total = (parcel.EmploymentTotal + zoneTotals.StudentsK12 + zoneTotals.StudentsUniversity) * 100 / millionsSquareLengthUnits;
			ans = Math.Log(1 + spaces * total / Math.Max(.001, spaces + total));
			Assert.Equal(ans, parcel.ZoneParkingPerTotalEmploymentAndK12UniversityStudents(zoneTotals, millionsSquareLengthUnits));

			spaces = parcel.ParkingOffStreetPaidDailySpaces / millionsSquareLengthUnits;
			total = (parcel.EmploymentFood + parcel.EmploymentRetail + parcel.EmploymentService + parcel.EmploymentMedical) * 100 / millionsSquareLengthUnits;
			ans = Math.Log(1 + spaces * total / Math.Max(.001, spaces + total));
			Assert.Equal(ans, parcel.ZoneParkingPerFoodRetailServiceMedicalEmployment(millionsSquareLengthUnits));

			households = parcel.HouseholdsBuffer2 * 100;
			total = (parcel.EmploymentRetailBuffer2 + parcel.EmploymentServiceBuffer2) * 100;
			ans = Math.Min(100000, .001 * households * total / (households + total + .1));
			Assert.Equal(ans, parcel.ParcelHouseholdsPerRetailServiceEmploymentBuffer2());

			households = parcel.HouseholdsBuffer2 * 100;
			total = (parcel.EmploymentRetailBuffer2 + parcel.EmploymentServiceBuffer2 + parcel.EmploymentFoodBuffer2) * 100;
			ans = households*total/(households + total + 1);
			Assert.Equal(ans, parcel.ParcelHouseholdsPerRetailServiceFoodEmploymentBuffer2());

			ans = parcel.NodesThreeLinksBuffer2*.5 + parcel.NodesFourLinksBuffer2;
			Assert.Equal(ans, parcel.IntersectionDensity34Buffer2());

			ans = parcel.IntersectionDensity34Buffer2() - parcel.NodesSingleLinkBuffer2;
			Assert.Equal(ans, parcel.IntersectionDensity34Minus1Buffer2());
			//ans = (parcel.NodesThreeLinksBuffer1 + parcel.NodesFourLinksBuffer1) / (Math.Max(1, parcel.NodesSingleLinkBuffer1 + parcel.NodesThreeLinksBuffer1 + parcel.NodesFourLinksBuffer1));
			//Assert.Equal(ans, parcel.CircuityDistance(destination));

			double duration = .25;
			ans = parcel.ParkingOffStreetPaidHourlyPriceBuffer1*duration / 100;
			Assert.Equal(ans, parcel.ParkingCostBuffer1(duration));

			duration = 5;
			ans = parcel.ParkingOffStreetPaidDailyPriceBuffer1 / 100;
			Assert.Equal(ans, parcel.ParkingCostBuffer1(duration));

			/*
			//parcel.DistanceToTransit / Global.DistanceUnitsPerMile
			Global.Configuration.DistanceUnitsPerMile = -1;
			ans = 0;
			Assert.Equal(ans, parcel.DistanceToTransitCappedUnderQtrMile());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit;
			ans = 0;
			Assert.Equal(ans, parcel.DistanceToTransitCappedUnderQtrMile());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit/4;
			ans = 0;
			Assert.Equal(ans, parcel.DistanceToTransitCappedUnderQtrMile());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit*5;
			ans = .25 - parcel.DistanceToTransit/Global.DistanceUnitsPerMile;
			Assert.Equal(ans, parcel.DistanceToTransitCappedUnderQtrMile());
						
			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit*999999;
			ans = .25 - parcel.DistanceToTransit/Global.DistanceUnitsPerMile;
			Assert.Equal(ans, parcel.DistanceToTransitCappedUnderQtrMile());



			Global.Configuration.DistanceUnitsPerMile = -1;
			ans = 0;
			Assert.Equal(ans, parcel.DistanceToTransitQtrToHalfMile());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit;
			ans = 0;
			Assert.Equal(ans, parcel.DistanceToTransitQtrToHalfMile());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit/4;
			ans = 0;
			Assert.Equal(ans, parcel.DistanceToTransitQtrToHalfMile());

			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit*5;
			ans = .5 - parcel.DistanceToTransit/Global.DistanceUnitsPerMile;
			Assert.Equal(ans, parcel.DistanceToTransitQtrToHalfMile());
						
			Global.Configuration.DistanceUnitsPerMile = parcel.DistanceToTransit*999999;
			ans = .5 - parcel.DistanceToTransit/Global.DistanceUnitsPerMile;
			Assert.Equal(ans, parcel.DistanceToTransitQtrToHalfMile());
			*/

			ans = Math.Log(1 + parcel.EmploymentFoodBuffer1 + parcel.EmploymentRetailBuffer1 + parcel.EmploymentServiceBuffer1 + parcel.EmploymentMedicalBuffer1);
			Assert.Equal(ans, parcel.FoodRetailServiceMedicalLogBuffer1());

			ans = Math.Log(1 + parcel.StudentsK8Buffer1 + parcel.StudentsHighSchoolBuffer1);
			Assert.Equal(ans, parcel.K8HighSchoolQtrMileLogBuffer1());

		}



		[Fact]
		public void TestCondensedParcelExtensionsStopAreaDistanceIndex()
		{
			const int id = 1;
			const int zoneId = 2;
			const int sequence = 3;
			const int xCoordinate = 4;
			const int yCoordinate = 5;
			const int transimsActivityLocation = 6;
			const int landUseCode19 = 7;
			double households = 8;
			const double studentsK8 = 9;
			const double studentsHighSchool = 10;
			const double studentsK12 = 11;
			const double studentsUniversity = 12;
			const double employmentEducation = 13;
			const double employmentFood = 14;
			const double employmentGovernment = 15;
			const double employmentIndustrial = 16;
			const double employmentMedical = 17;
			const double employmentOffice = 18;
			const double employmentRetail = 19;
			const double employmentService = 20;
			const double employmentAgricultureConstruction = 21;
			const double employmentTotal = 22;
			const double parkingOffStreetPaidDailySpaces = 23;
			const double studentsK8Buffer1 = 24;
			const double studentsHighSchoolBuffer1 = 25;
			const double studentsK8Buffer2 = 26;
			const double studentsHighSchoolBuffer2 = 27;
			const double employmentFoodBuffer1 = 28;
			const double employmentMedicalBuffer1 = 29;
			const double employmentMedicalBuffer2 = 30;
			const double employmentRetailBuffer1 = 31;
			const double employmentServiceBuffer1 = 32;
			const double parkingOffStreetPaidDailyPriceBuffer1 = 33;
			const double parkingOffStreetPaidHourlyPriceBuffer1 = 34;
			const double parkingOffStreetPaidDailyPriceBuffer2 = 35;
			const double parkingOffStreetPaidHourlyPriceBuffer2 = 36;
			const double stopsTransitBuffer1 = 37;
			const double stopsTransitBuffer2 = 38;
			const double nodesSingleLinkBuffer1 = 39;
			const double nodesThreeLinksBuffer1 = 40;
			const double nodesFourLinksBuffer1 = 41;
			const double openSpaceType1Buffer1 = 42;
			const double openSpaceType2Buffer1 = 43;
			const double openSpaceType1Buffer2 = 44;
			const double openSpaceType2Buffer2 = 45;
			const double employmentFoodBuffer2 = 46;
			const double employmentRetailBuffer2 = 47;
			const double employmentServiceBuffer2 = 48;
			const double householdsBuffer2 = 49;
			const double nodesSingleLinkBuffer2 = 50;
			const double nodesThreeLinksBuffer2 = 51;
			const double nodesFourLinksBuffer2 = 52;
			const double distanceToLocalBus = 53;
			const double distanceToLightRail = 54;
			const double distanceToExpressBus = 55;
			const double distanceToCommuterRail = 56;
			const double distanceToFerry = 57;
			const double distanceToTransit = 58;
			const double shadowPriceForEmployment = 59;
			const double shadowPriceForStudentsK12 = 60;
			const double shadowPriceForStudentsUniversity = 61;
			const double externalEmploymentTotal = 62;
			const double employmentDifference = 63;
			const double employmentPrediction = 64;
			const double externalStudentsK12 = 65;
			const double studentsK12Difference = 66;
			const double studentsK12Prediction = 67;
			const double externalStudentsUniversity = 68;
			const double studentsUniversityDifference = 69;
			const double studentsUniversityPrediction = 70;
			const double parkingOffStreetPaidHourlySpaces = 71;
			const double employmentGovernmentBuffer1 = 72;
			const double employmentOfficeBuffer1 = 73;
			const double employmentGovernmentBuffer2 = 74;
			const double employmentOfficeBuffer2 = 75;
			const double employmentEducationBuffer1 = 76;
			const double employmentEducationBuffer2 = 77;
			const double employmentAgricultureConstructionBuffer1 = 78;
			const double employmentIndustrialBuffer1 = 79;
			const double employmentAgricultureConstructionBuffer2 = 80;
			const double employmentIndustrialBuffer2 = 81;
			const double employmentTotalBuffer1 = 82;
			const double employmentTotalBuffer2 = 83;
			const double householdsBuffer1 = 84;
			const double studentsUniversityBuffer1 = 85;
			const double studentsUniversityBuffer2 = 86;
			const double parkingOffStreetPaidHourlySpacesBuffer1 = 87;
			const double parkingOffStreetPaidDailySpacesBuffer1 = 88;
			const double parkingOffStreetPaidHourlySpacesBuffer2 = 89;
			const double parkingOffStreetPaidDailySpacesBuffer2 = 90;
			const double circuityRatio_E1 = 91;
			const double circuityRatio_E2 = 92;
			const double circuityRatio_E3 = 93;
			const double circuityRatio_NE1 = 94;
			const double circuityRatio_NE2 = 95;
			const double circuityRatio_NE3 = 96;
			const double circuityRatio_N1 = 97;
			const double circuityRatio_N2 = 98;
			const double circuityRatio_N3 = 99;
			const double circuityRatio_NW1 = 100;
			const double circuityRatio_NW2 = 101;
			const double circuityRatio_NW3 = 102;
			const double circuityRatio_W1 = 103;
			const double circuityRatio_W2 = 104;
			const double circuityRatio_W3 = 105;
			const double circuityRatio_SW1 = 106;
			const double circuityRatio_SW2 = 107;
			const double circuityRatio_SW3 = 108;
			const double circuityRatio_S1 = 109;
			const double circuityRatio_S2 = 110;
			const double circuityRatio_S3 = 111;
			const double circuityRatio_SE1 = 112;
			const double circuityRatio_SE2 = 113;
			const double circuityRatio_SE3 = 114;


			CondensedParcel parcel = new CondensedParcel
				                         {
					                         Id = id,
					                         ZoneId = zoneId,
					                         Sequence = sequence,
					                         XCoordinate = xCoordinate,
					                         YCoordinate = yCoordinate,
					                         TransimsActivityLocation = transimsActivityLocation,
					                         LandUseCode19 = landUseCode19,
					                         Households = households,
					                         StudentsK8 = studentsK8,
					                         StudentsHighSchool = studentsHighSchool,
					                         StudentsK12 = studentsK12,
					                         StudentsUniversity = studentsUniversity,
					                         EmploymentEducation = employmentEducation,
					                         EmploymentFood = employmentFood,
					                         EmploymentGovernment = employmentGovernment,
					                         EmploymentIndustrial = employmentIndustrial,
					                         EmploymentMedical = employmentMedical,
					                         EmploymentOffice = employmentOffice,
					                         EmploymentRetail = employmentRetail,
					                         EmploymentService = employmentService,
					                         EmploymentAgricultureConstruction = employmentAgricultureConstruction,
					                         EmploymentTotal = employmentTotal,
					                         ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                         StudentsK8Buffer1 = studentsK8Buffer1,
					                         StudentsHighSchoolBuffer1 = studentsHighSchoolBuffer1,
					                         StudentsK8Buffer2 = studentsK8Buffer2,
					                         StudentsHighSchoolBuffer2 = studentsHighSchoolBuffer2,
					                         EmploymentFoodBuffer1 = employmentFoodBuffer1,
					                         EmploymentMedicalBuffer1 = employmentMedicalBuffer1,
					                         EmploymentMedicalBuffer2 = employmentMedicalBuffer2,
					                         EmploymentRetailBuffer1 = employmentRetailBuffer1,
					                         EmploymentServiceBuffer1 = employmentServiceBuffer1,
					                         ParkingOffStreetPaidDailyPriceBuffer1 = parkingOffStreetPaidDailyPriceBuffer1,
					                         ParkingOffStreetPaidHourlyPriceBuffer1 = parkingOffStreetPaidHourlyPriceBuffer1,
					                         ParkingOffStreetPaidDailyPriceBuffer2 = parkingOffStreetPaidDailyPriceBuffer2,
					                         ParkingOffStreetPaidHourlyPriceBuffer2 = parkingOffStreetPaidHourlyPriceBuffer2,
					                         StopsTransitBuffer1 = stopsTransitBuffer1,
					                         StopsTransitBuffer2 = stopsTransitBuffer2,
					                         NodesSingleLinkBuffer1 = nodesSingleLinkBuffer1,
					                         NodesThreeLinksBuffer1 = nodesThreeLinksBuffer1,
					                         NodesFourLinksBuffer1 = nodesFourLinksBuffer1,
					                         OpenSpaceType1Buffer1 = openSpaceType1Buffer1,
					                         OpenSpaceType2Buffer1 = openSpaceType2Buffer1,
					                         OpenSpaceType1Buffer2 = openSpaceType1Buffer2,
					                         OpenSpaceType2Buffer2 = openSpaceType2Buffer2,
					                         EmploymentFoodBuffer2 = employmentFoodBuffer2,
					                         EmploymentRetailBuffer2 = employmentRetailBuffer2,
					                         EmploymentServiceBuffer2 = employmentServiceBuffer2,
					                         HouseholdsBuffer2 = householdsBuffer2,
					                         NodesSingleLinkBuffer2 = nodesSingleLinkBuffer2,
					                         NodesThreeLinksBuffer2 = nodesThreeLinksBuffer2,
					                         NodesFourLinksBuffer2 = nodesFourLinksBuffer2,
					                         DistanceToLocalBus = distanceToLocalBus,
					                         DistanceToLightRail = distanceToLightRail,
					                         DistanceToExpressBus = distanceToExpressBus,
					                         DistanceToCommuterRail = distanceToCommuterRail,
					                         DistanceToFerry = distanceToFerry,
					                         DistanceToTransit = distanceToTransit,
					                         ShadowPriceForEmployment = shadowPriceForEmployment,
					                         ShadowPriceForStudentsK12 = shadowPriceForStudentsK12,
					                         ShadowPriceForStudentsUniversity = shadowPriceForStudentsUniversity,
					                         ExternalEmploymentTotal = externalEmploymentTotal,
					                         EmploymentDifference = employmentDifference,
					                         EmploymentPrediction = employmentPrediction,
					                         ExternalStudentsK12 = externalStudentsK12,
					                         StudentsK12Difference = studentsK12Difference,
					                         StudentsK12Prediction = studentsK12Prediction,
					                         ExternalStudentsUniversity = externalStudentsUniversity,
					                         StudentsUniversityDifference = studentsUniversityDifference,
					                         StudentsUniversityPrediction = studentsUniversityPrediction,
					                         ParkingOffStreetPaidHourlySpaces = parkingOffStreetPaidHourlySpaces,
					                         EmploymentGovernmentBuffer1 = employmentGovernmentBuffer1,
					                         EmploymentOfficeBuffer1 = employmentOfficeBuffer1,
					                         EmploymentGovernmentBuffer2 = employmentGovernmentBuffer2,
					                         EmploymentOfficeBuffer2 = employmentOfficeBuffer2,
					                         EmploymentEducationBuffer1 = employmentEducationBuffer1,
					                         EmploymentEducationBuffer2 = employmentEducationBuffer2,
					                         EmploymentAgricultureConstructionBuffer1 =
						                         employmentAgricultureConstructionBuffer1,
					                         EmploymentIndustrialBuffer1 = employmentIndustrialBuffer1,
					                         EmploymentAgricultureConstructionBuffer2 =
						                         employmentAgricultureConstructionBuffer2,
					                         EmploymentIndustrialBuffer2 = employmentIndustrialBuffer2,
					                         EmploymentTotalBuffer1 = employmentTotalBuffer1,
					                         EmploymentTotalBuffer2 = employmentTotalBuffer2,
					                         HouseholdsBuffer1 = householdsBuffer1,
					                         StudentsUniversityBuffer1 = studentsUniversityBuffer1,
					                         StudentsUniversityBuffer2 = studentsUniversityBuffer2,
					                         ParkingOffStreetPaidHourlySpacesBuffer1 = parkingOffStreetPaidHourlySpacesBuffer1,
					                         ParkingOffStreetPaidDailySpacesBuffer1 = parkingOffStreetPaidDailySpacesBuffer1,
					                         ParkingOffStreetPaidHourlySpacesBuffer2 = parkingOffStreetPaidHourlySpacesBuffer2,
					                         ParkingOffStreetPaidDailySpacesBuffer2 = parkingOffStreetPaidDailySpacesBuffer2,
					                         CircuityRatio_E1 = circuityRatio_E1,
					                         CircuityRatio_E2 = circuityRatio_E2,
					                         CircuityRatio_E3 = circuityRatio_E3,
					                         CircuityRatio_NE1 = circuityRatio_NE1,
					                         CircuityRatio_NE2 = circuityRatio_NE2,
					                         CircuityRatio_NE3 = circuityRatio_NE3,
					                         CircuityRatio_N1 = circuityRatio_N1,
					                         CircuityRatio_N2 = circuityRatio_N2,
					                         CircuityRatio_N3 = circuityRatio_N3,
					                         CircuityRatio_NW1 = circuityRatio_NW1,
					                         CircuityRatio_NW2 = circuityRatio_NW2,
					                         CircuityRatio_NW3 = circuityRatio_NW3,
					                         CircuityRatio_W1 = circuityRatio_W1,
					                         CircuityRatio_W2 = circuityRatio_W2,
					                         CircuityRatio_W3 = circuityRatio_W3,
					                         CircuityRatio_SW1 = circuityRatio_SW1,
					                         CircuityRatio_SW2 = circuityRatio_SW2,
					                         CircuityRatio_SW3 = circuityRatio_SW3,
					                         CircuityRatio_S1 = circuityRatio_S1,
					                         CircuityRatio_S2 = circuityRatio_S2,
					                         CircuityRatio_S3 = circuityRatio_S3,
					                         CircuityRatio_SE1 = circuityRatio_SE1,
					                         CircuityRatio_SE2 = circuityRatio_SE2,
					                         CircuityRatio_SE3 = circuityRatio_SE3,
				                         };
			MemoryStream testMemoryStream = new MemoryStream();
			TextWriter writer = new StreamWriter(testMemoryStream);
			writer.WriteLine("header info here");
			writer.WriteLine("1 1 10");
			writer.WriteLine("1 2 10");
			writer.WriteLine("1 3 10");
			writer.WriteLine("1 4 10");
			writer.WriteLine("2 1 11");
			writer.WriteLine("2 2 12");
			writer.WriteLine("2 3 13");
			writer.WriteLine("2 4 14");

			writer.Flush();
			
			testMemoryStream.Seek(0, SeekOrigin.Begin);
			TextReader reader = new StreamReader(testMemoryStream);
			ICondensedParcelExtensions.InitializeParcelStopAreaIndex(reader);

			parcel.Id = 1;
			parcel.SetFirstLastStopAreaDistanceIndexes();
			Assert.Equal(0, parcel.FirstPositioninStopAreaDistanceArray);
			Assert.Equal(3, parcel.LastPositionInStopAreaDistanceArray);


			parcel.Id = 2;
			parcel.SetFirstLastStopAreaDistanceIndexes();
			Assert.Equal(4, parcel.FirstPositioninStopAreaDistanceArray);
			Assert.Equal(7, parcel.LastPositionInStopAreaDistanceArray);

		}

		[Fact]
		public void TestCircuityDistance1()
		{
			Global.Configuration = new Configuration();
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel origin = GetDefaultCondensedParcel(xCoordinate:1, yCoordinate:2);
			CondensedParcel destination = new CondensedParcel{XCoordinate = 4, YCoordinate = 6, Id = 2};
			
			double ans = ((5 / 1) / 5280D) * 1 * 1.4;
			Assert.Equal(ans, origin.CircuityDistance(destination));
			
			Global.Configuration.MaximumBlendingDistance = 10;
			ans = ((5 * (.75 * origin.CircuityRatio_NE1 + (1 - .75) * origin.CircuityRatio_N1) / Global.LengthUnitsPerFoot) / 5280D) * Global.DistanceUnitsPerMile;
			Assert.Equal(ans, origin.CircuityDistance(destination));
		}

		[Fact]
		public void TestCircuityDistance2()
		{
			Global.Configuration = new Configuration();
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel origin = GetDefaultCondensedParcel(xCoordinate:1, yCoordinate:2);
			CondensedParcel destination = new CondensedParcel{XCoordinate = 1201, YCoordinate = 3502, Id = 2};
			
			Global.Configuration.MaximumBlendingDistance = 10;
			const double dWeight2 = (3700 - 2640.0) / (5280.0 - 2640.0);
			const double dWeight1 = 1.0 - dWeight2;
			const double odAngle = 12.0 / 35.0;

			double cr = dWeight1 * (odAngle * origin.CircuityRatio_NE1 + (1 - odAngle) * origin.CircuityRatio_N1) +
							dWeight2 * (odAngle * origin.CircuityRatio_NE2 + (1 - odAngle) * origin.CircuityRatio_N2);

			double ans = ((3700 * cr) / Global.LengthUnitsPerFoot / 5280D) * Global.DistanceUnitsPerMile;
			Assert.Equal(ans, origin.CircuityDistance(destination));
		}


		[Fact]
		public void TestCircuityDistance3()
		{
			Global.Configuration = new Configuration();
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel origin = GetDefaultCondensedParcel(xCoordinate:1, yCoordinate:2);
			CondensedParcel destination = new CondensedParcel{XCoordinate = 2801, YCoordinate = 4502, Id = 2};
			
			Global.Configuration.MaximumBlendingDistance = 10;
			const double dWeight3 = (5300.0 - 5280.0) / (7920.0 - 5280.0);
			const double dWeight2 = 1.0 - dWeight3;
			const double odAngle = 28.0 / 45.0;

			double cr = dWeight2 * (odAngle * origin.CircuityRatio_NE2 + (1 - odAngle) * origin.CircuityRatio_N2) +
							dWeight3 * (odAngle * origin.CircuityRatio_NE3 + (1 - odAngle) * origin.CircuityRatio_N3);

			double ans = ((5300 * cr) / Global.LengthUnitsPerFoot / 5280D) * Global.DistanceUnitsPerMile;
			Assert.Equal(ans, origin.CircuityDistance(destination));
		}

		[Fact]
		public void TestCircuityDistance4()
		{
			Global.Configuration = new Configuration();
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel origin = GetDefaultCondensedParcel(xCoordinate:1, yCoordinate:2);
			CondensedParcel destination = new CondensedParcel{XCoordinate = 2801, YCoordinate = 19502, Id = 2};
			
			Global.Configuration.MaximumBlendingDistance = 10;
			
			const double odAngle = 28.0 / 195.0;

			double cr = (odAngle*origin.CircuityRatio_NE3 + (1 - odAngle)*origin.CircuityRatio_N3);

			double ans = (((10560 * cr + (19700 - 10560) * 1.4) / Global.LengthUnitsPerFoot) / 5280D) * Global.DistanceUnitsPerMile ;//((19700 * cr) / Global.LengthUnitsPerFoot / 5280D) * Global.DistanceUnitsPerMile;
			Assert.Equal(ans, origin.CircuityDistance(destination));
		}

		[Fact]
		public void TestNodeToNode1()
		{
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			Global.Configuration = new Configuration();
			Global.Configuration.NProcessors = 1;
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel parcel = GetDefaultCondensedParcel();
			double ans = -1;
			CondensedParcel destination = new CondensedParcel();
			Assert.Equal(ans, parcel.NodeToNodeDistance(destination, 0));

			ans = -1;
			Global.ANodeId = new int[1];
			destination = new CondensedParcel{Id=2};
			Assert.Equal(ans, parcel.NodeToNodeDistance(destination, 0));
			//Assert.Equal(ans, parcel.CircuityDistance(destination));
		}

		[Fact]
		public void TestNodeToNode2()
		{
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			Global.Configuration = new Configuration();
			Global.Configuration.NProcessors = 1;
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel parcel = GetDefaultCondensedParcel();
			Global.ANodeFirstRecord = new int[50];
			Global.ANodeLastRecord = new int[50];
			Global.NodePairBNodeId = new int[50];
			Global.NodePairDistance = new ushort[50];
			Global.ANodeFirstRecord[10] = 5;
			Global.ANodeLastRecord[10] = 8;
			Global.NodePairBNodeId[5] = 9;
			Global.NodePairBNodeId[6] = 12;
			Global.NodePairDistance[6] = 10000;
			const double ans = 10000.0/5280.0;
			CondensedParcel destination = new CondensedParcel {Id = 2};
			Global.ANodeId = new int[15];
			Assert.Equal(ans, parcel.NodeToNodeDistance(destination, 0));
			
		}

		[Fact]
		public void TestNodeToNode3()
		{
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			Global.Configuration = new Configuration();
			Global.Configuration.NProcessors = 1;
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel parcel = GetDefaultCondensedParcel();
			Global.ANodeFirstRecord = new int[50];
			Global.ANodeLastRecord = new int[50];
			Global.NodePairBNodeId = new int[50];
			Global.NodePairDistance = new ushort[50];
			Global.ANodeFirstRecord[10] = 5;
			Global.ANodeLastRecord[10] = 9;
			Global.NodePairBNodeId[5] = 9;
			Global.NodePairBNodeId[6] = 10;
			Global.NodePairBNodeId[7] = 11;
			Global.NodePairBNodeId[8] = 12;
			Global.NodePairDistance[8] = 10000;
			const double ans = 10000.0/5280.0;
			CondensedParcel destination = new CondensedParcel {Id = 2};
			Global.ANodeId = new int[15];
			Assert.Equal(ans, parcel.NodeToNodeDistance(destination, 0));
		}

		[Fact]
		public void TestNodeToNode4()
		{
			TestParcelNodeReader reader = GetTestParcelNodeReader();
			Global.Configuration = new Configuration();
			Global.Configuration.NProcessors = 1;
			ICondensedParcelExtensions.InitializeNodeNodeIndex(reader);
			CondensedParcel parcel = GetDefaultCondensedParcel();
			Global.ANodeFirstRecord = new int[50];
			Global.ANodeLastRecord = new int[50];
			Global.NodePairBNodeId = new int[50];
			Global.NodePairDistance = new ushort[50];
			Global.ANodeFirstRecord[10] = 5;
			Global.ANodeLastRecord[10] = 8;
			Global.NodePairBNodeId[5] = 9;
			Global.NodePairBNodeId[6] = 10;
			Global.NodePairBNodeId[7] = 11;
			Global.NodePairBNodeId[8] = 12;
			Global.NodePairDistance[8] = 10000;
			const double ans = -1;//can't find it
			CondensedParcel destination = new CondensedParcel {Id = 2};
			Global.ANodeId = new int[15];
			Assert.Equal(ans, parcel.NodeToNodeDistance(destination, 0));
		}

		private TestParcelNodeReader GetTestParcelNodeReader() 
		{
			TestParcelNodeReader reader = new TestParcelNodeReader();
			ParcelNode parcel1 = new ParcelNode{Id=1, NodeId = 11};
			ParcelNode parcel2 = new ParcelNode{Id=2, NodeId = 12};
			ParcelNode parcel3 = new ParcelNode{Id=3, NodeId = 13};
			ParcelNode parcel4 = new ParcelNode{Id=4, NodeId = 14};
			reader.AddParcelNode(parcel1);
			reader.AddParcelNode(parcel2);
			reader.AddParcelNode(parcel3);
			reader.AddParcelNode(parcel4);
			return reader;
		}




		private CondensedParcel GetDefaultCondensedParcel( int xCoordinate = 4, int yCoordinate = 5)
		{
			const int id = 1;
			const int zoneId = 2;
			const int sequence = 3;
			const int transimsActivityLocation = 6;
			const int landUseCode19 = 7;
			const double households = 8;
			const double studentsK8 = 9;
			const double studentsHighSchool = 10;
			const double studentsK12 = 11;
			const double studentsUniversity = 12;
			const double employmentEducation = 13;
			const double employmentFood = 14;
			const double employmentGovernment = 15;
			const double employmentIndustrial = 16;
			const double employmentMedical = 17;
			const double employmentOffice = 18;
			const double employmentRetail = 19;
			const double employmentService = 20;
			const double employmentAgricultureConstruction = 21;
			const double employmentTotal = 22;
			const double parkingOffStreetPaidDailySpaces = 23;
			const double studentsK8Buffer1 = 24;
			const double studentsHighSchoolBuffer1 = 25;
			const double studentsK8Buffer2 = 26;
			const double studentsHighSchoolBuffer2 = 27;
			const double employmentFoodBuffer1 = 28;
			const double employmentMedicalBuffer1 = 29;
			const double employmentMedicalBuffer2 = 30;
			const double employmentRetailBuffer1 = 31;
			const double employmentServiceBuffer1 = 32;
			const double parkingOffStreetPaidDailyPriceBuffer1 = 33;
			const double parkingOffStreetPaidHourlyPriceBuffer1 = 34;
			const double parkingOffStreetPaidDailyPriceBuffer2 = 35;
			const double parkingOffStreetPaidHourlyPriceBuffer2 = 36;
			const double stopsTransitBuffer1 = 37;
			const double stopsTransitBuffer2 = 38;
			const double nodesSingleLinkBuffer1 = 39;
			const double nodesThreeLinksBuffer1 = 40;
			const double nodesFourLinksBuffer1 = 41;
			const double openSpaceType1Buffer1 = 42;
			const double openSpaceType2Buffer1 = 43;
			const double openSpaceType1Buffer2 = 44;
			const double openSpaceType2Buffer2 = 45;
			const double employmentFoodBuffer2 = 46;
			const double employmentRetailBuffer2 = 47;
			const double employmentServiceBuffer2 = 48;
			const double householdsBuffer2 = 49;
			const double nodesSingleLinkBuffer2 = 50;
			const double nodesThreeLinksBuffer2 = 51;
			const double nodesFourLinksBuffer2 = 52;
			const double distanceToLocalBus = 53;
			const double distanceToLightRail = 54;
			const double distanceToExpressBus = 55;
			const double distanceToCommuterRail = 56;
			const double distanceToFerry = 57;
			const double distanceToTransit = 58;
			const double shadowPriceForEmployment = 59;
			const double shadowPriceForStudentsK12 = 60;
			const double shadowPriceForStudentsUniversity = 61;
			const double externalEmploymentTotal = 62;
			const double employmentDifference = 63;
			const double employmentPrediction = 64;
			const double externalStudentsK12 = 65;
			const double studentsK12Difference = 66;
			const double studentsK12Prediction = 67;
			const double externalStudentsUniversity = 68;
			const double studentsUniversityDifference = 69;
			const double studentsUniversityPrediction = 70;
			const double parkingOffStreetPaidHourlySpaces = 71;
			const double employmentGovernmentBuffer1 = 72;
			const double employmentOfficeBuffer1 = 73;
			const double employmentGovernmentBuffer2 = 74;
			const double employmentOfficeBuffer2 = 75;
			const double employmentEducationBuffer1 = 76;
			const double employmentEducationBuffer2 = 77;
			const double employmentAgricultureConstructionBuffer1 = 78;
			const double employmentIndustrialBuffer1 = 79;
			const double employmentAgricultureConstructionBuffer2 = 80;
			const double employmentIndustrialBuffer2 = 81;
			const double employmentTotalBuffer1 = 82;
			const double employmentTotalBuffer2 = 83;
			const double householdsBuffer1 = 84;
			const double studentsUniversityBuffer1 = 85;
			const double studentsUniversityBuffer2 = 86;
			const double parkingOffStreetPaidHourlySpacesBuffer1 = 87;
			const double parkingOffStreetPaidDailySpacesBuffer1 = 88;
			const double parkingOffStreetPaidHourlySpacesBuffer2 = 89;
			const double parkingOffStreetPaidDailySpacesBuffer2 = 90;
			const double circuityRatio_E1 = 91;
			const double circuityRatio_E2 = 92;
			const double circuityRatio_E3 = 93;
			const double circuityRatio_NE1 = 94;
			const double circuityRatio_NE2 = 95;
			const double circuityRatio_NE3 = 96;
			const double circuityRatio_N1 = 97;
			const double circuityRatio_N2 = 98;
			const double circuityRatio_N3 = 99;
			const double circuityRatio_NW1 = 100;
			const double circuityRatio_NW2 = 101;
			const double circuityRatio_NW3 = 102;
			const double circuityRatio_W1 = 103;
			const double circuityRatio_W2 = 104;
			const double circuityRatio_W3 = 105;
			const double circuityRatio_SW1 = 106;
			const double circuityRatio_SW2 = 107;
			const double circuityRatio_SW3 = 108;
			const double circuityRatio_S1 = 109;
			const double circuityRatio_S2 = 110;
			const double circuityRatio_S3 = 111;
			const double circuityRatio_SE1 = 112;
			const double circuityRatio_SE2 = 113;
			const double circuityRatio_SE3 = 114;


			CondensedParcel parcel = new CondensedParcel
				                         {
					                         Id = id,
					                         ZoneId = zoneId,
					                         Sequence = sequence,
					                         XCoordinate = xCoordinate,
					                         YCoordinate = yCoordinate,
					                         TransimsActivityLocation = transimsActivityLocation,
					                         LandUseCode19 = landUseCode19,
					                         Households = households,
					                         StudentsK8 = studentsK8,
					                         StudentsHighSchool = studentsHighSchool,
					                         StudentsK12 = studentsK12,
					                         StudentsUniversity = studentsUniversity,
					                         EmploymentEducation = employmentEducation,
					                         EmploymentFood = employmentFood,
					                         EmploymentGovernment = employmentGovernment,
					                         EmploymentIndustrial = employmentIndustrial,
					                         EmploymentMedical = employmentMedical,
					                         EmploymentOffice = employmentOffice,
					                         EmploymentRetail = employmentRetail,
					                         EmploymentService = employmentService,
					                         EmploymentAgricultureConstruction = employmentAgricultureConstruction,
					                         EmploymentTotal = employmentTotal,
					                         ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                         StudentsK8Buffer1 = studentsK8Buffer1,
					                         StudentsHighSchoolBuffer1 = studentsHighSchoolBuffer1,
					                         StudentsK8Buffer2 = studentsK8Buffer2,
					                         StudentsHighSchoolBuffer2 = studentsHighSchoolBuffer2,
					                         EmploymentFoodBuffer1 = employmentFoodBuffer1,
					                         EmploymentMedicalBuffer1 = employmentMedicalBuffer1,
					                         EmploymentMedicalBuffer2 = employmentMedicalBuffer2,
					                         EmploymentRetailBuffer1 = employmentRetailBuffer1,
					                         EmploymentServiceBuffer1 = employmentServiceBuffer1,
					                         ParkingOffStreetPaidDailyPriceBuffer1 = parkingOffStreetPaidDailyPriceBuffer1,
					                         ParkingOffStreetPaidHourlyPriceBuffer1 = parkingOffStreetPaidHourlyPriceBuffer1,
					                         ParkingOffStreetPaidDailyPriceBuffer2 = parkingOffStreetPaidDailyPriceBuffer2,
					                         ParkingOffStreetPaidHourlyPriceBuffer2 = parkingOffStreetPaidHourlyPriceBuffer2,
					                         StopsTransitBuffer1 = stopsTransitBuffer1,
					                         StopsTransitBuffer2 = stopsTransitBuffer2,
					                         NodesSingleLinkBuffer1 = nodesSingleLinkBuffer1,
					                         NodesThreeLinksBuffer1 = nodesThreeLinksBuffer1,
					                         NodesFourLinksBuffer1 = nodesFourLinksBuffer1,
					                         OpenSpaceType1Buffer1 = openSpaceType1Buffer1,
					                         OpenSpaceType2Buffer1 = openSpaceType2Buffer1,
					                         OpenSpaceType1Buffer2 = openSpaceType1Buffer2,
					                         OpenSpaceType2Buffer2 = openSpaceType2Buffer2,
					                         EmploymentFoodBuffer2 = employmentFoodBuffer2,
					                         EmploymentRetailBuffer2 = employmentRetailBuffer2,
					                         EmploymentServiceBuffer2 = employmentServiceBuffer2,
					                         HouseholdsBuffer2 = householdsBuffer2,
					                         NodesSingleLinkBuffer2 = nodesSingleLinkBuffer2,
					                         NodesThreeLinksBuffer2 = nodesThreeLinksBuffer2,
					                         NodesFourLinksBuffer2 = nodesFourLinksBuffer2,
					                         DistanceToLocalBus = distanceToLocalBus,
					                         DistanceToLightRail = distanceToLightRail,
					                         DistanceToExpressBus = distanceToExpressBus,
					                         DistanceToCommuterRail = distanceToCommuterRail,
					                         DistanceToFerry = distanceToFerry,
					                         DistanceToTransit = distanceToTransit,
					                         ShadowPriceForEmployment = shadowPriceForEmployment,
					                         ShadowPriceForStudentsK12 = shadowPriceForStudentsK12,
					                         ShadowPriceForStudentsUniversity = shadowPriceForStudentsUniversity,
					                         ExternalEmploymentTotal = externalEmploymentTotal,
					                         EmploymentDifference = employmentDifference,
					                         EmploymentPrediction = employmentPrediction,
					                         ExternalStudentsK12 = externalStudentsK12,
					                         StudentsK12Difference = studentsK12Difference,
					                         StudentsK12Prediction = studentsK12Prediction,
					                         ExternalStudentsUniversity = externalStudentsUniversity,
					                         StudentsUniversityDifference = studentsUniversityDifference,
					                         StudentsUniversityPrediction = studentsUniversityPrediction,
					                         ParkingOffStreetPaidHourlySpaces = parkingOffStreetPaidHourlySpaces,
					                         EmploymentGovernmentBuffer1 = employmentGovernmentBuffer1,
					                         EmploymentOfficeBuffer1 = employmentOfficeBuffer1,
					                         EmploymentGovernmentBuffer2 = employmentGovernmentBuffer2,
					                         EmploymentOfficeBuffer2 = employmentOfficeBuffer2,
					                         EmploymentEducationBuffer1 = employmentEducationBuffer1,
					                         EmploymentEducationBuffer2 = employmentEducationBuffer2,
					                         EmploymentAgricultureConstructionBuffer1 =
						                         employmentAgricultureConstructionBuffer1,
					                         EmploymentIndustrialBuffer1 = employmentIndustrialBuffer1,
					                         EmploymentAgricultureConstructionBuffer2 =
						                         employmentAgricultureConstructionBuffer2,
					                         EmploymentIndustrialBuffer2 = employmentIndustrialBuffer2,
					                         EmploymentTotalBuffer1 = employmentTotalBuffer1,
					                         EmploymentTotalBuffer2 = employmentTotalBuffer2,
					                         HouseholdsBuffer1 = householdsBuffer1,
					                         StudentsUniversityBuffer1 = studentsUniversityBuffer1,
					                         StudentsUniversityBuffer2 = studentsUniversityBuffer2,
					                         ParkingOffStreetPaidHourlySpacesBuffer1 = parkingOffStreetPaidHourlySpacesBuffer1,
					                         ParkingOffStreetPaidDailySpacesBuffer1 = parkingOffStreetPaidDailySpacesBuffer1,
					                         ParkingOffStreetPaidHourlySpacesBuffer2 = parkingOffStreetPaidHourlySpacesBuffer2,
					                         ParkingOffStreetPaidDailySpacesBuffer2 = parkingOffStreetPaidDailySpacesBuffer2,
					                         CircuityRatio_E1 = circuityRatio_E1,
					                         CircuityRatio_E2 = circuityRatio_E2,
					                         CircuityRatio_E3 = circuityRatio_E3,
					                         CircuityRatio_NE1 = circuityRatio_NE1,
					                         CircuityRatio_NE2 = circuityRatio_NE2,
					                         CircuityRatio_NE3 = circuityRatio_NE3,
					                         CircuityRatio_N1 = circuityRatio_N1,
					                         CircuityRatio_N2 = circuityRatio_N2,
					                         CircuityRatio_N3 = circuityRatio_N3,
					                         CircuityRatio_NW1 = circuityRatio_NW1,
					                         CircuityRatio_NW2 = circuityRatio_NW2,
					                         CircuityRatio_NW3 = circuityRatio_NW3,
					                         CircuityRatio_W1 = circuityRatio_W1,
					                         CircuityRatio_W2 = circuityRatio_W2,
					                         CircuityRatio_W3 = circuityRatio_W3,
					                         CircuityRatio_SW1 = circuityRatio_SW1,
					                         CircuityRatio_SW2 = circuityRatio_SW2,
					                         CircuityRatio_SW3 = circuityRatio_SW3,
					                         CircuityRatio_S1 = circuityRatio_S1,
					                         CircuityRatio_S2 = circuityRatio_S2,
					                         CircuityRatio_S3 = circuityRatio_S3,
					                         CircuityRatio_SE1 = circuityRatio_SE1,
					                         CircuityRatio_SE2 = circuityRatio_SE2,
					                         CircuityRatio_SE3 = circuityRatio_SE3,
				                         };

			return parcel;
		}
		[Fact]
		public void TestParcel()
		{
			const int id = 1;
			const int zoneId = 2;
			const int sequence = 3;
			const int xCoordinate = 4;
			const int yCoordinate = 5;
			const double households = 8;
			const double studentsK8 = 9;
			const double studentsHighSchool = 10;
			const double studentsUniversity = 12;
			const double employmentEducation = 13;
			const double employmentFood = 14;
			const double employmentGovernment = 15;
			const double employmentIndustrial = 16;
			const double employmentMedical = 17;
			const double employmentOffice = 18;
			const double employmentRetail = 19;
			const double employmentService = 20;
			const double employmentAgricultureConstruction = 21;
			const double employmentTotal = 22;
			const double parkingOffStreetPaidDailySpaces = 23;
			const double studentsK8Buffer1 = 24;
			const double studentsHighSchoolBuffer1 = 25;
			const double studentsK8Buffer2 = 26;
			const double studentsHighSchoolBuffer2 = 27;
			const double employmentFoodBuffer1 = 28;
			const double employmentMedicalBuffer1 = 29;
			const double employmentMedicalBuffer2 = 30;
			const double employmentRetailBuffer1 = 31;
			const double employmentServiceBuffer1 = 32;
			const double parkingOffStreetPaidDailyPriceBuffer1 = 33;
			const double parkingOffStreetPaidHourlyPriceBuffer1 = 34;
			const double parkingOffStreetPaidDailyPriceBuffer2 = 35;
			const double parkingOffStreetPaidHourlyPriceBuffer2 = 36;
			const double stopsTransitBuffer1 = 37;
			const double stopsTransitBuffer2 = 38;
			const double nodesSingleLinkBuffer1 = 39;
			const double nodesThreeLinksBuffer1 = 40;
			const double nodesFourLinksBuffer1 = 41;
			const double openSpaceType1Buffer1 = 42;
			const double openSpaceType2Buffer1 = 43;
			const double openSpaceType1Buffer2 = 44;
			const double openSpaceType2Buffer2 = 45;
			const double employmentFoodBuffer2 = 46;
			const double employmentRetailBuffer2 = 47;
			const double employmentServiceBuffer2 = 48;
			const double householdsBuffer2 = 49;
			const double nodesSingleLinkBuffer2 = 50;
			const double nodesThreeLinksBuffer2 = 51;
			const double nodesFourLinksBuffer2 = 52;
			const double distanceToLocalBus = 53;
			const double distanceToLightRail = 54;
			const double distanceToExpressBus = 55;
			const double distanceToCommuterRail = 56;
			const double distanceToFerry = 57;
			const double parkingOffStreetPaidHourlySpaces = 71;
			const double employmentGovernmentBuffer1 = 72;
			const double employmentOfficeBuffer1 = 73;
			const double employmentGovernmentBuffer2 = 74;
			const double employmentOfficeBuffer2 = 75;
			const double employmentEducationBuffer1 = 76;
			const double employmentEducationBuffer2 = 77;
			const double employmentAgricultureConstructionBuffer1 = 78;
			const double employmentIndustrialBuffer1 = 79;
			const double employmentAgricultureConstructionBuffer2 = 80;
			const double employmentIndustrialBuffer2 = 81;
			const double employmentTotalBuffer1 = 82;
			const double employmentTotalBuffer2 = 83;
			const double householdsBuffer1 = 84;
			const double studentsUniversityBuffer1 = 85;
			const double studentsUniversityBuffer2 = 86;
			const double parkingOffStreetPaidHourlySpacesBuffer1 = 87;
			const double parkingOffStreetPaidDailySpacesBuffer1 = 88;
			const double parkingOffStreetPaidHourlySpacesBuffer2 = 89;
			const double parkingOffStreetPaidDailySpacesBuffer2 = 90;
			const double circuityRatio_E1 = 91;
			const double circuityRatio_E2 = 92;
			const double circuityRatio_E3 = 93;
			const double circuityRatio_NE1 = 94;
			const double circuityRatio_NE2 = 95;
			const double circuityRatio_NE3 = 96;
			const double circuityRatio_N1 = 97;
			const double circuityRatio_N2 = 98;
			const double circuityRatio_N3 = 99;
			const double circuityRatio_NW1 = 100;
			const double circuityRatio_NW2 = 101;
			const double circuityRatio_NW3 = 102;
			const double circuityRatio_W1 = 103;
			const double circuityRatio_W2 = 104;
			const double circuityRatio_W3 = 105;
			const double circuityRatio_SW1 = 106;
			const double circuityRatio_SW2 = 107;
			const double circuityRatio_SW3 = 108;
			const double circuityRatio_S1 = 109;
			const double circuityRatio_S2 = 110;
			const double circuityRatio_S3 = 111;
			const double circuityRatio_SE1 = 112;
			const double circuityRatio_SE2 = 113;
			const double circuityRatio_SE3 = 114;

			const int landUseCode = 115;
			const double parkingOffStreetPaidDailyPrice = 116;
			const double parkingOffStreetPaidHourlyPrice = 117;
			const double thousandsSquareLengthUnits = 118;
			const double zoneKey = 119;


			Parcel parcel = new Parcel
				                                  {
					                                  Id = id,
					                                  ZoneId = zoneId,
					                                  Sequence = sequence,
					                                  XCoordinate = xCoordinate,
					                                  YCoordinate = yCoordinate,
					                                  Households = households,
					                                  StudentsK8 = studentsK8,
					                                  StudentsHighSchool = studentsHighSchool,
					                                  StudentsUniversity = studentsUniversity,
					                                  EmploymentEducation = employmentEducation,
					                                  EmploymentFood = employmentFood,
					                                  EmploymentGovernment = employmentGovernment,
					                                  EmploymentIndustrial = employmentIndustrial,
					                                  EmploymentMedical = employmentMedical,
					                                  EmploymentOffice = employmentOffice,
					                                  EmploymentRetail = employmentRetail,
					                                  EmploymentService = employmentService,
					                                  EmploymentAgricultureConstruction = employmentAgricultureConstruction,
					                                  EmploymentTotal = employmentTotal,
					                                  ParkingOffStreetPaidDailySpaces = parkingOffStreetPaidDailySpaces,
					                                  StudentsK8Buffer1 = studentsK8Buffer1,
					                                  StudentsHighSchoolBuffer1 = studentsHighSchoolBuffer1,
					                                  StudentsK8Buffer2 = studentsK8Buffer2,
					                                  StudentsHighSchoolBuffer2 = studentsHighSchoolBuffer2,
					                                  EmploymentFoodBuffer1 = employmentFoodBuffer1,
					                                  EmploymentMedicalBuffer1 = employmentMedicalBuffer1,
					                                  EmploymentMedicalBuffer2 = employmentMedicalBuffer2,
					                                  EmploymentRetailBuffer1 = employmentRetailBuffer1,
					                                  EmploymentServiceBuffer1 = employmentServiceBuffer1,
					                                  ParkingOffStreetPaidDailyPriceBuffer1 = parkingOffStreetPaidDailyPriceBuffer1,
					                                  ParkingOffStreetPaidHourlyPriceBuffer1 = parkingOffStreetPaidHourlyPriceBuffer1,
					                                  ParkingOffStreetPaidDailyPriceBuffer2 = parkingOffStreetPaidDailyPriceBuffer2,
					                                  ParkingOffStreetPaidHourlyPriceBuffer2 = parkingOffStreetPaidHourlyPriceBuffer2,
					                                  StopsTransitBuffer1 = stopsTransitBuffer1,
					                                  StopsTransitBuffer2 = stopsTransitBuffer2,
					                                  NodesSingleLinkBuffer1 = nodesSingleLinkBuffer1,
					                                  NodesThreeLinksBuffer1 = nodesThreeLinksBuffer1,
					                                  NodesFourLinksBuffer1 = nodesFourLinksBuffer1,
					                                  OpenSpaceType1Buffer1 = openSpaceType1Buffer1,
					                                  OpenSpaceType2Buffer1 = openSpaceType2Buffer1,
					                                  OpenSpaceType1Buffer2 = openSpaceType1Buffer2,
					                                  OpenSpaceType2Buffer2 = openSpaceType2Buffer2,
					                                  EmploymentFoodBuffer2 = employmentFoodBuffer2,
					                                  EmploymentRetailBuffer2 = employmentRetailBuffer2,
					                                  EmploymentServiceBuffer2 = employmentServiceBuffer2,
					                                  HouseholdsBuffer2 = householdsBuffer2,
					                                  NodesSingleLinkBuffer2 = nodesSingleLinkBuffer2,
					                                  NodesThreeLinksBuffer2 = nodesThreeLinksBuffer2,
					                                  NodesFourLinksBuffer2 = nodesFourLinksBuffer2,
					                                  DistanceToLocalBus = distanceToLocalBus,
					                                  DistanceToLightRail = distanceToLightRail,
					                                  DistanceToExpressBus = distanceToExpressBus,
					                                  DistanceToCommuterRail = distanceToCommuterRail,
					                                  DistanceToFerry = distanceToFerry,
					                                  ParkingOffStreetPaidHourlySpaces = parkingOffStreetPaidHourlySpaces,
					                                  EmploymentGovernmentBuffer1 = employmentGovernmentBuffer1,
					                                  EmploymentOfficeBuffer1 = employmentOfficeBuffer1,
					                                  EmploymentGovernmentBuffer2 = employmentGovernmentBuffer2,
					                                  EmploymentOfficeBuffer2 = employmentOfficeBuffer2,
					                                  EmploymentEducationBuffer1 = employmentEducationBuffer1,
					                                  EmploymentEducationBuffer2 = employmentEducationBuffer2,
					                                  EmploymentAgricultureConstructionBuffer1 =
						                                  employmentAgricultureConstructionBuffer1,
					                                  EmploymentIndustrialBuffer1 = employmentIndustrialBuffer1,
					                                  EmploymentAgricultureConstructionBuffer2 =
						                                  employmentAgricultureConstructionBuffer2,
					                                  EmploymentIndustrialBuffer2 = employmentIndustrialBuffer2,
					                                  EmploymentTotalBuffer1 = employmentTotalBuffer1,
					                                  EmploymentTotalBuffer2 = employmentTotalBuffer2,
					                                  HouseholdsBuffer1 = householdsBuffer1,
					                                  StudentsUniversityBuffer1 = studentsUniversityBuffer1,
					                                  StudentsUniversityBuffer2 = studentsUniversityBuffer2,
					                                  ParkingOffStreetPaidHourlySpacesBuffer1 = parkingOffStreetPaidHourlySpacesBuffer1,
					                                  ParkingOffStreetPaidDailySpacesBuffer1 = parkingOffStreetPaidDailySpacesBuffer1,
					                                  ParkingOffStreetPaidHourlySpacesBuffer2 = parkingOffStreetPaidHourlySpacesBuffer2,
					                                  ParkingOffStreetPaidDailySpacesBuffer2 = parkingOffStreetPaidDailySpacesBuffer2,
					                                  CircuityRatio_E1 = circuityRatio_E1,
					                                  CircuityRatio_E2 = circuityRatio_E2,
					                                  CircuityRatio_E3 = circuityRatio_E3,
					                                  CircuityRatio_NE1 = circuityRatio_NE1,
					                                  CircuityRatio_NE2 = circuityRatio_NE2,
					                                  CircuityRatio_NE3 = circuityRatio_NE3,
					                                  CircuityRatio_N1 = circuityRatio_N1,
					                                  CircuityRatio_N2 = circuityRatio_N2,
					                                  CircuityRatio_N3 = circuityRatio_N3,
					                                  CircuityRatio_NW1 = circuityRatio_NW1,
					                                  CircuityRatio_NW2 = circuityRatio_NW2,
					                                  CircuityRatio_NW3 = circuityRatio_NW3,
					                                  CircuityRatio_W1 = circuityRatio_W1,
					                                  CircuityRatio_W2 = circuityRatio_W2,
					                                  CircuityRatio_W3 = circuityRatio_W3,
					                                  CircuityRatio_SW1 = circuityRatio_SW1,
					                                  CircuityRatio_SW2 = circuityRatio_SW2,
					                                  CircuityRatio_SW3 = circuityRatio_SW3,
					                                  CircuityRatio_S1 = circuityRatio_S1,
					                                  CircuityRatio_S2 = circuityRatio_S2,
					                                  CircuityRatio_S3 = circuityRatio_S3,
					                                  CircuityRatio_SE1 = circuityRatio_SE1,
					                                  CircuityRatio_SE2 = circuityRatio_SE2,
					                                  CircuityRatio_SE3 = circuityRatio_SE3,
																						LandUseCode = landUseCode,
																						ParkingOffStreetPaidDailyPrice = parkingOffStreetPaidDailyPrice,
																						ParkingOffStreetPaidHourlyPrice = parkingOffStreetPaidHourlyPrice,
																						ThousandsSquareLengthUnits = thousandsSquareLengthUnits,
																						ZoneKey = zoneKey,
																						
				                                  };

			Assert.Equal(id, parcel.Id);
			Assert.Equal(zoneId, parcel.ZoneId);
			Assert.Equal(sequence, parcel.Sequence);
			Assert.Equal(xCoordinate, parcel.XCoordinate);
			Assert.Equal(yCoordinate, parcel.YCoordinate);
			Assert.Equal(landUseCode, parcel.LandUseCode);
			Assert.Equal(households, parcel.Households);
			Assert.Equal(studentsK8, parcel.StudentsK8);
			Assert.Equal(studentsHighSchool, parcel.StudentsHighSchool);
			Assert.Equal(studentsUniversity, parcel.StudentsUniversity);
			Assert.Equal(employmentEducation, parcel.EmploymentEducation);
			Assert.Equal(employmentFood, parcel.EmploymentFood);
			Assert.Equal(employmentGovernment, parcel.EmploymentGovernment);
			Assert.Equal(employmentIndustrial, parcel.EmploymentIndustrial);
			Assert.Equal(employmentMedical, parcel.EmploymentMedical);
			Assert.Equal(employmentOffice, parcel.EmploymentOffice);
			Assert.Equal(employmentRetail, parcel.EmploymentRetail);
			Assert.Equal(employmentService, parcel.EmploymentService);
			Assert.Equal(employmentAgricultureConstruction, parcel.EmploymentAgricultureConstruction);
			Assert.Equal(employmentTotal, parcel.EmploymentTotal);
			Assert.Equal(parkingOffStreetPaidDailySpaces, parcel.ParkingOffStreetPaidDailySpaces);
			Assert.Equal(studentsK8Buffer1, parcel.StudentsK8Buffer1);
			Assert.Equal(studentsHighSchoolBuffer1, parcel.StudentsHighSchoolBuffer1);
			Assert.Equal(studentsK8Buffer2, parcel.StudentsK8Buffer2);
			Assert.Equal(studentsHighSchoolBuffer2, parcel.StudentsHighSchoolBuffer2);
			Assert.Equal(employmentFoodBuffer1, parcel.EmploymentFoodBuffer1);
			Assert.Equal(employmentMedicalBuffer1, parcel.EmploymentMedicalBuffer1);
			Assert.Equal(employmentMedicalBuffer2, parcel.EmploymentMedicalBuffer2);
			Assert.Equal(employmentRetailBuffer1, parcel.EmploymentRetailBuffer1);
			Assert.Equal(employmentServiceBuffer1, parcel.EmploymentServiceBuffer1);
			Assert.Equal(parkingOffStreetPaidDailyPriceBuffer1, parcel.ParkingOffStreetPaidDailyPriceBuffer1);
			Assert.Equal(parkingOffStreetPaidHourlyPriceBuffer1, parcel.ParkingOffStreetPaidHourlyPriceBuffer1);
			Assert.Equal(parkingOffStreetPaidDailyPriceBuffer2, parcel.ParkingOffStreetPaidDailyPriceBuffer2);
			Assert.Equal(parkingOffStreetPaidHourlyPriceBuffer2, parcel.ParkingOffStreetPaidHourlyPriceBuffer2);
			Assert.Equal(stopsTransitBuffer1, parcel.StopsTransitBuffer1);
			Assert.Equal(stopsTransitBuffer2, parcel.StopsTransitBuffer2);
			Assert.Equal(nodesSingleLinkBuffer1, parcel.NodesSingleLinkBuffer1);
			Assert.Equal(nodesThreeLinksBuffer1, parcel.NodesThreeLinksBuffer1);
			Assert.Equal(nodesFourLinksBuffer1, parcel.NodesFourLinksBuffer1);
			Assert.Equal(openSpaceType1Buffer1, parcel.OpenSpaceType1Buffer1);
			Assert.Equal(openSpaceType2Buffer1, parcel.OpenSpaceType2Buffer1);
			Assert.Equal(openSpaceType1Buffer2, parcel.OpenSpaceType1Buffer2);
			Assert.Equal(openSpaceType2Buffer2, parcel.OpenSpaceType2Buffer2);
			Assert.Equal(employmentFoodBuffer2, parcel.EmploymentFoodBuffer2);
			Assert.Equal(employmentRetailBuffer2, parcel.EmploymentRetailBuffer2);
			Assert.Equal(employmentServiceBuffer2, parcel.EmploymentServiceBuffer2);
			Assert.Equal(householdsBuffer2, parcel.HouseholdsBuffer2);
			Assert.Equal(nodesSingleLinkBuffer2, parcel.NodesSingleLinkBuffer2);
			Assert.Equal(nodesThreeLinksBuffer2, parcel.NodesThreeLinksBuffer2);
			Assert.Equal(nodesFourLinksBuffer2, parcel.NodesFourLinksBuffer2);
			Assert.Equal(distanceToLocalBus, parcel.DistanceToLocalBus);
			Assert.Equal(distanceToLightRail, parcel.DistanceToLightRail);
			Assert.Equal(distanceToExpressBus, parcel.DistanceToExpressBus);
			Assert.Equal(distanceToCommuterRail, parcel.DistanceToCommuterRail);
			Assert.Equal(distanceToFerry, parcel.DistanceToFerry);
			Assert.Equal(parkingOffStreetPaidHourlySpaces, parcel.ParkingOffStreetPaidHourlySpaces);
			Assert.Equal(employmentGovernmentBuffer1, parcel.EmploymentGovernmentBuffer1);
			Assert.Equal(employmentOfficeBuffer1, parcel.EmploymentOfficeBuffer1);
			Assert.Equal(employmentGovernmentBuffer2, parcel.EmploymentGovernmentBuffer2);
			Assert.Equal(employmentOfficeBuffer2, parcel.EmploymentOfficeBuffer2);
			Assert.Equal(employmentEducationBuffer1, parcel.EmploymentEducationBuffer1);
			Assert.Equal(employmentEducationBuffer2, parcel.EmploymentEducationBuffer2);
			Assert.Equal(employmentAgricultureConstructionBuffer1, parcel.EmploymentAgricultureConstructionBuffer1);
			Assert.Equal(employmentIndustrialBuffer1, parcel.EmploymentIndustrialBuffer1);
			Assert.Equal(employmentAgricultureConstructionBuffer2, parcel.EmploymentAgricultureConstructionBuffer2);
			Assert.Equal(employmentIndustrialBuffer2, parcel.EmploymentIndustrialBuffer2);
			Assert.Equal(employmentTotalBuffer1, parcel.EmploymentTotalBuffer1);
			Assert.Equal(employmentTotalBuffer2, parcel.EmploymentTotalBuffer2);
			Assert.Equal(householdsBuffer1, parcel.HouseholdsBuffer1);
			Assert.Equal(studentsUniversityBuffer1, parcel.StudentsUniversityBuffer1);
			Assert.Equal(studentsUniversityBuffer2, parcel.StudentsUniversityBuffer2);
			Assert.Equal(parkingOffStreetPaidHourlySpacesBuffer1, parcel.ParkingOffStreetPaidHourlySpacesBuffer1);
			Assert.Equal(parkingOffStreetPaidDailySpacesBuffer1, parcel.ParkingOffStreetPaidDailySpacesBuffer1);
			Assert.Equal(parkingOffStreetPaidHourlySpacesBuffer2, parcel.ParkingOffStreetPaidHourlySpacesBuffer2);
			Assert.Equal(parkingOffStreetPaidDailySpacesBuffer2, parcel.ParkingOffStreetPaidDailySpacesBuffer2);
			Assert.Equal(circuityRatio_E1, parcel.CircuityRatio_E1);
			Assert.Equal(circuityRatio_E2, parcel.CircuityRatio_E2);
			Assert.Equal(circuityRatio_E3, parcel.CircuityRatio_E3);
			Assert.Equal(circuityRatio_NE1, parcel.CircuityRatio_NE1);
			Assert.Equal(circuityRatio_NE2, parcel.CircuityRatio_NE2);
			Assert.Equal(circuityRatio_NE3, parcel.CircuityRatio_NE3);
			Assert.Equal(circuityRatio_N1, parcel.CircuityRatio_N1);
			Assert.Equal(circuityRatio_N2, parcel.CircuityRatio_N2);
			Assert.Equal(circuityRatio_N3, parcel.CircuityRatio_N3);
			Assert.Equal(circuityRatio_NW1, parcel.CircuityRatio_NW1);
			Assert.Equal(circuityRatio_NW2, parcel.CircuityRatio_NW2);
			Assert.Equal(circuityRatio_NW3, parcel.CircuityRatio_NW3);
			Assert.Equal(circuityRatio_W1, parcel.CircuityRatio_W1);
			Assert.Equal(circuityRatio_W2, parcel.CircuityRatio_W2);
			Assert.Equal(circuityRatio_W3, parcel.CircuityRatio_W3);
			Assert.Equal(circuityRatio_SW1, parcel.CircuityRatio_SW1);
			Assert.Equal(circuityRatio_SW2, parcel.CircuityRatio_SW2);
			Assert.Equal(circuityRatio_SW3, parcel.CircuityRatio_SW3);
			Assert.Equal(circuityRatio_S1, parcel.CircuityRatio_S1);
			Assert.Equal(circuityRatio_S2, parcel.CircuityRatio_S2);
			Assert.Equal(circuityRatio_S3, parcel.CircuityRatio_S3);
			Assert.Equal(circuityRatio_SE1, parcel.CircuityRatio_SE1);
			Assert.Equal(circuityRatio_SE2, parcel.CircuityRatio_SE2);
			Assert.Equal(circuityRatio_SE3, parcel.CircuityRatio_SE3);

			Assert.Equal(parkingOffStreetPaidDailyPrice, parcel.ParkingOffStreetPaidDailyPrice);
			Assert.Equal(parkingOffStreetPaidHourlyPrice, parcel.ParkingOffStreetPaidHourlyPrice);
			Assert.Equal(thousandsSquareLengthUnits, parcel.ThousandsSquareLengthUnits);
			Assert.Equal(zoneKey, parcel.ZoneKey);
		}

		[Fact]
		public void TestListExtensions()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 1;
			List<ITripWrapper> trips = new List<ITripWrapper>();
		  trips.Add(TestHelper.GetTripWrapper(1));
			trips.Add(TestHelper.GetTripWrapper(2));
			trips.Add(TestHelper.GetTripWrapper(3));
			trips.Add(TestHelper.GetTripWrapper(4));
			List<ITripWrapper> inverted = trips.Invert();

			Assert.Equal(trips[0], inverted[3]);
			Assert.Equal(trips[1], inverted[2]);
			Assert.Equal(trips[2], inverted[1]);
			Assert.Equal(trips[3], inverted[0]);
		}

		[Fact]
		public void TestParcelNode()
		{
			const int id = 1;
			const int nodeId = 2;
			ParcelNode node = new ParcelNode
				                  {
					                  Id = id,
					                  NodeId = nodeId,
				                  };
			Assert.Equal(id, node.Id);
			Assert.Equal(nodeId, node.NodeId);
		}

		[Fact]
		public void TestParcelNodeWrapper()
		{
			const int id = 1;
			const int nodeId = 2;
			ParcelNode node = new ParcelNode
				                  {
					                  Id = id,
					                  NodeId = nodeId,
				                  };
			ParcelNodeWrapper wrapper = new ParcelNodeWrapper(node);

			Assert.Equal(id, wrapper.Id);
			Assert.Equal(nodeId, wrapper.NodeId);
		}

		[Fact]
		public void TestParkAndRideNode()
		{
			const int id = 1;
			const int zoneId = 2;
			const int xCoordinate = 3;
			const int yCoordinate = 4;
			const int capacity = 5;
			const int cost = 6;
			const int nearestParcelId = 7;

			ParkAndRideNode node = new ParkAndRideNode
				                       {
					                       Capacity = capacity,
					                       Cost = cost,
					                       Id = id,
					                       NearestParcelId = nearestParcelId,
					                       XCoordinate = xCoordinate,
					                       YCoordinate = yCoordinate,
					                       ZoneId = zoneId,
				                       };

			Assert.Equal(capacity, node.Capacity);
			Assert.Equal(cost, node.Cost);
			Assert.Equal(id, node.Id);
			Assert.Equal(nearestParcelId, node.NearestParcelId);
			Assert.Equal(xCoordinate, node.XCoordinate);
			Assert.Equal(yCoordinate, node.YCoordinate);
			Assert.Equal(zoneId, node.ZoneId);
		}

		[Fact]
		public void TestParkAndRideNodeWrapper()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 256;

			const int id = 1;
			const int zoneId = 2;
			const int xCoordinate = 3;
			const int yCoordinate = 4;
			const int capacity = 5;
			const int cost = 6;
			const int nearestParcelId = 7;

			ParkAndRideNode node = new ParkAndRideNode
				                       {
					                       Capacity = capacity,
					                       Cost = cost,
					                       Id = id,
					                       NearestParcelId = nearestParcelId,
					                       XCoordinate = xCoordinate,
					                       YCoordinate = yCoordinate,
					                       ZoneId = zoneId,
				                       };
			ParkAndRideNodeWrapper wrapper = new ParkAndRideNodeWrapper(node);

			Assert.Equal(capacity, wrapper.Capacity);
			Assert.Equal(cost, wrapper.Cost);
			Assert.Equal(id, wrapper.Id);
			Assert.Equal(nearestParcelId, wrapper.NearestParcelId);
			Assert.Equal(xCoordinate, wrapper.XCoordinate);
			Assert.Equal(yCoordinate, wrapper.YCoordinate);
			Assert.Equal(zoneId, wrapper.ZoneId);

			Assert.Equal(null, wrapper.ShadowPriceDifference);
			Assert.Equal(null, wrapper.ShadowPrice);
			Assert.Equal(null, wrapper.ExogenousLoad);
			Assert.Equal(null, wrapper.ParkAndRideLoad);

			Dictionary<int, ParkAndRideShadowPriceNode> dictionary = new Dictionary<int, ParkAndRideShadowPriceNode>();

			Global.Configuration.RawParkAndRideNodePath = "c:\aaa.txt";
			Global.Configuration.InputParkAndRideNodePath = "c:\aaa.txt";
			Global.Configuration.ShouldUseParkAndRideShadowPricing = false;
			Global.Configuration.IsInEstimationMode = true;

			wrapper.SetParkAndRideShadowPricing(dictionary);

			Assert.Equal(null, wrapper.ShadowPriceDifference);
			Assert.Equal(null, wrapper.ShadowPrice);
			Assert.Equal(null, wrapper.ExogenousLoad);
			Assert.Equal(null, wrapper.ParkAndRideLoad);

			Global.Configuration.ShouldUseParkAndRideShadowPricing = true;
			Global.Configuration.IsInEstimationMode = false;

			wrapper.SetParkAndRideShadowPricing(dictionary);

			Assert.NotNull(wrapper.ShadowPriceDifference);
			Assert.NotNull(wrapper.ShadowPrice);
			Assert.NotNull(wrapper.ExogenousLoad);
			Assert.NotNull(wrapper.ParkAndRideLoad);

			Assert.Equal(0, wrapper.ShadowPriceDifference[0]);
			Assert.Equal(0, wrapper.ShadowPrice[0]);
			Assert.Equal(0, wrapper.ExogenousLoad[0]);
			Assert.Equal(0, wrapper.ParkAndRideLoad[0]);

			var shadowPriceDifference = new double[Constants.Time.MINUTES_IN_A_DAY];
			var shadowPrice = new double[Constants.Time.MINUTES_IN_A_DAY];
			var exogenousLoad = new double[Constants.Time.MINUTES_IN_A_DAY];
			var parkAndRideLoad = new double[Constants.Time.MINUTES_IN_A_DAY];
			shadowPriceDifference[0] = 1;
			shadowPrice[0] = 1;
			exogenousLoad[0] = 1;
			parkAndRideLoad[0] = 1;

			dictionary.Add(id, new ParkAndRideShadowPriceNode
				                   {
					                   ExogenousLoad = exogenousLoad,
														 NodeId = 1,
														 ParkAndRideLoad = parkAndRideLoad,
														 ShadowPrice = shadowPrice,
														 ShadowPriceDifference = shadowPriceDifference,
				                   });

			wrapper.SetParkAndRideShadowPricing(dictionary);

			Assert.Equal(1, wrapper.ShadowPriceDifference[0]);
			Assert.Equal(1, wrapper.ShadowPrice[0]);
			Assert.Equal(1, wrapper.ExogenousLoad[0]);
			Assert.Equal(0, wrapper.ParkAndRideLoad[0]);
		}
		
		[Fact]
		public void TestPointExtensions()
		{
			//Todo Test Figure out how to get the impedence Roster to work
			Global.Configuration = new Configuration();

			CondensedParcel origin = new CondensedParcel();
			CondensedParcel destination = new CondensedParcel();

			if (Global.PrintFile == null || !(Global.PrintFile is TestPrintFile))
				Global.PrintFile = new TestPrintFile();

			Dictionary<int, int> zoneMapping = new Dictionary<int, int>();
			Dictionary<int, int> transitStopAreaMapping = new Dictionary<int, int>();
			TestImpedanceRosterLoader loader = new TestImpedanceRosterLoader
				                                   {
					                                   TestEntries = new List<RosterEntry>
						                                                 {
							                                                 new RosterEntry
								                                                 {
									                                                 Variable = "distance",
									                                                 StartMinute = 10,
									                                                 EndMinute = 1000
								                                                 },
							                                                 new RosterEntry
								                                                 {
									                                                 Variable = "onea",
									                                                 StartMinute = 10,
									                                                 EndMinute = 1000
								                                                 }
						                                                 },
					                                   TestVotRanges = new List<ImpedanceRoster.VotRange>
						                                                   {
							                                                   new ImpedanceRoster.VotRange(10, 1, 2),
							                                                   new ImpedanceRoster.VotRange(12, 3, 15),
						                                                   },
					                                   TestRosterEntries = new RosterEntry[20][][][][]
				                                   };


			loader.TestRosterEntries[0] = new RosterEntry[15][][][];
			loader.TestRosterEntries[0][3] = new RosterEntry[15][][];
			loader.TestRosterEntries[0][3][1] = new RosterEntry[15][];
			loader.TestRosterEntries[0][3][1][12] = new RosterEntry[15];
			loader.TestRosterEntries[0][3][1][12][1] = new RosterEntry{BlendVariable = "onea"};

			loader.TestRosterEntries[1] = new RosterEntry[15][][][];
			loader.TestRosterEntries[1][0] = new RosterEntry[15][][];
			loader.TestRosterEntries[1][0][0] = new RosterEntry[15][];
			loader.TestRosterEntries[1][0][0][12] = new RosterEntry[15];
			loader.TestRosterEntries[1][0][0][12][1] = new RosterEntry{BlendVariable = "one", Name = "two", MatrixIndex = 12, Scaling = 2, Factor = .5,};

			
			loader.TestSkimMatrices = new SkimMatrix[100];
			ushort[][] skim = new ushort[5][];
			skim[0] = new ushort[5];
			skim[0][0] = 10;
			loader.TestSkimMatrices[12] = new SkimMatrix(skim);
			
			ImpedanceRoster.Initialize(zoneMapping, transitStopAreaMapping, loader);
			
			double dist = origin.DistanceFromOrigin(destination, 1);
			Assert.Equal(.25, dist);
			
			
		}

		[Fact]
		public void TestPointExtensionsNoName()
		{
			Global.Configuration = new Configuration();

			CondensedParcel origin = new CondensedParcel();
			CondensedParcel destination = new CondensedParcel();

			if (Global.PrintFile == null || !(Global.PrintFile is TestPrintFile))
				Global.PrintFile = new TestPrintFile();
			
			Dictionary<int, int> zoneMapping = new Dictionary<int, int>();
			Dictionary<int, int> transitStopAreaMapping = new Dictionary<int, int>();
			TestImpedanceRosterLoader loader = new TestImpedanceRosterLoader
				                                   {
					                                   TestEntries = new List<RosterEntry>
						                                                 {
							                                                 new RosterEntry
								                                                 {
									                                                 Variable = "distance",
									                                                 StartMinute = 10,
									                                                 EndMinute = 1000
								                                                 },
							                                                 new RosterEntry
								                                                 {
									                                                 Variable = "onea",
									                                                 StartMinute = 10,
									                                                 EndMinute = 1000
								                                                 }
						                                                 },
					                                   TestVotRanges = new List<ImpedanceRoster.VotRange>
						                                                   {
							                                                   new ImpedanceRoster.VotRange(10, 1, 2),
							                                                   new ImpedanceRoster.VotRange(12, 3, 15),
						                                                   },
					                                   TestRosterEntries = new RosterEntry[20][][][][]
				                                   };


			loader.TestRosterEntries[0] = new RosterEntry[15][][][];
			loader.TestRosterEntries[0][3] = new RosterEntry[15][][];
			loader.TestRosterEntries[0][3][1] = new RosterEntry[15][];
			loader.TestRosterEntries[0][3][1][12] = new RosterEntry[15];
			loader.TestRosterEntries[0][3][1][12][1] = new RosterEntry{BlendVariable = "onea"};

			loader.TestRosterEntries[1] = new RosterEntry[15][][][];
			loader.TestRosterEntries[1][0] = new RosterEntry[15][][];
			loader.TestRosterEntries[1][0][0] = new RosterEntry[15][];
			loader.TestRosterEntries[1][0][0][12] = new RosterEntry[15];
			//No name
			loader.TestRosterEntries[1][0][0][12][1] = new RosterEntry{BlendVariable = "one", MatrixIndex = 12, Scaling = 2, Factor = .5,};

			
			loader.TestSkimMatrices = new SkimMatrix[100];
			ushort[][] skim = new ushort[5][];
			skim[0] = new ushort[5];
			skim[0][0] = 10;
			loader.TestSkimMatrices[12] = new SkimMatrix(skim);
			Console.Out.WriteLine("Hello World");
			ImpedanceRoster.Initialize(zoneMapping, transitStopAreaMapping, loader);
			
			double dist = origin.DistanceFromOrigin(destination, 1);
			Assert.Equal(0, dist);
			
			
		}

		[Fact]
		public void TestPointExtensions2()
		{
			//Todo Test something else
			Global.Configuration = new Configuration();

			CondensedParcel origin = new CondensedParcel();
			CondensedParcel destination = new CondensedParcel();

			if (Global.PrintFile == null || !(Global.PrintFile is TestPrintFile))
				Global.PrintFile = new TestPrintFile();

			Dictionary<int, int> zoneMapping = new Dictionary<int, int>();
			Dictionary<int, int> transitStopAreaMapping = new Dictionary<int, int>();
			TestImpedanceRosterLoader loader = new TestImpedanceRosterLoader
				                                   {
					                                   TestEntries = new List<RosterEntry>
						                                                 {
							                                                 new RosterEntry
								                                                 {
									                                                 Variable = "distance",
									                                                 StartMinute = 10,
									                                                 EndMinute = 1000
								                                                 },
							                                                 new RosterEntry
								                                                 {
									                                                 Variable = "onea",
									                                                 StartMinute = 10,
									                                                 EndMinute = 1000
								                                                 }
						                                                 },
					                                   TestVotRanges = new List<ImpedanceRoster.VotRange>
						                                                   {
							                                                   new ImpedanceRoster.VotRange(10, 1, 2),
							                                                   new ImpedanceRoster.VotRange(12, 3, 15),
						                                                   },
					                                   TestRosterEntries = new RosterEntry[20][][][][]
				                                   };


			loader.TestRosterEntries[0] = new RosterEntry[15][][][];
			loader.TestRosterEntries[0][3] = new RosterEntry[15][][];
			loader.TestRosterEntries[0][3][1] = new RosterEntry[15][];
			loader.TestRosterEntries[0][3][1][12] = new RosterEntry[15];
			loader.TestRosterEntries[0][3][1][12][1] = new RosterEntry{BlendVariable = "onea"};

			loader.TestRosterEntries[1] = new RosterEntry[15][][][];
			loader.TestRosterEntries[1][0] = new RosterEntry[15][][];
			loader.TestRosterEntries[1][0][0] = new RosterEntry[15][];
			loader.TestRosterEntries[1][0][0][12] = new RosterEntry[15];
			loader.TestRosterEntries[1][0][0][12][1] = new RosterEntry{BlendVariable = "one", Name = "two", MatrixIndex = 12, Scaling = 2, Factor = .5,};

			
			loader.TestSkimMatrices = new SkimMatrix[100];
			ushort[][] skim = new ushort[5][];
			skim[0] = new ushort[5];
			skim[0][0] = 10;
			loader.TestSkimMatrices[12] = new SkimMatrix(skim);
			
			ImpedanceRoster.Initialize(zoneMapping, transitStopAreaMapping, loader);
			
			double dist = origin.DistanceFromOrigin(destination, 1);
			Assert.Equal(.25, dist);
			
			
		}
	}
}
