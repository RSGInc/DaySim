using System;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.Framework.Core;
using Xunit;

namespace Daysim.Tests {
	
	public class TourTest {
		[Fact]
		public void TestTour()
		{
			int id = 1;
			int personId = 2;
			int personDayId = 3;
			int householdId = 4;
			int personSequence = 5;
			int day = 6;
			int sequence = 7;
			int jointTourSequence = 8;
			int parentTourSequence = 9;
			int subtours = 10;
			int destinationPurpose = 11;
			int originDepartureTime = 12;
			int destinationArrivalTime = 13;
			int destinationDepartureTime = 14;
			int originArrivalTime = 15;
			int originAddressType = 16;
			int destinationAddressType = 17;
			int originParcelId = 18;
			int originZoneKey = 19;
			int destinationParcelId = 20;
			int destinationZoneKey = 21;
			int mode = 22;
			int pathType = 23;
			double autoTimeOneWay = 24.01;
			double autoCostOneWay = 25.01;
			double autoDistanceOneWay = 26.01;
			int halfTour1Trips = 27;
			int halfTour2Trips = 28;
			int partialHalfTour1Sequence = 29;
			int partialHalfTour2Sequence = 30;
			int fullHalfTour1Sequence = 31;
			int fullHalfTour2Sequence = 32;
			double expansionFactor = 33.01;

			Tour tour = new Tour
				            {
					            Id = id,
					            PersonId = personId,
					            PersonDayId = personDayId,
					            HouseholdId = householdId,
					            PersonSequence = personSequence,
					            Day = day,
					            Sequence = sequence,
					            JointTourSequence = jointTourSequence,
					            ParentTourSequence = parentTourSequence,
					            Subtours = subtours,
					            DestinationPurpose = destinationPurpose,
					            OriginDepartureTime = originDepartureTime,
					            DestinationArrivalTime = destinationArrivalTime,
					            DestinationDepartureTime = destinationDepartureTime,
					            OriginArrivalTime = originArrivalTime,
					            OriginAddressType = originAddressType,
					            DestinationAddressType = destinationAddressType,
					            OriginParcelId = originParcelId,
					            OriginZoneKey = originZoneKey,
					            DestinationParcelId = destinationParcelId,
					            DestinationZoneKey = destinationZoneKey,
					            Mode = mode,
					            PathType = pathType,
					            AutoTimeOneWay = autoTimeOneWay,
					            AutoCostOneWay = autoCostOneWay,
					            AutoDistanceOneWay = autoDistanceOneWay,
					            HalfTour1Trips = halfTour1Trips,
					            HalfTour2Trips = halfTour2Trips,
					            PartialHalfTour1Sequence = partialHalfTour1Sequence,
					            PartialHalfTour2Sequence = partialHalfTour2Sequence,
					            FullHalfTour1Sequence = fullHalfTour1Sequence,
					            FullHalfTour2Sequence = fullHalfTour2Sequence,
					            ExpansionFactor = expansionFactor,
				            };
			Assert.Equal(id, tour.Id);
			Assert.Equal(personId, tour.PersonId);
			Assert.Equal(personDayId, tour.PersonDayId);
			Assert.Equal(householdId, tour.HouseholdId);
			Assert.Equal(personSequence, tour.PersonSequence);
			Assert.Equal(day, tour.Day);
			Assert.Equal(sequence, tour.Sequence);
			Assert.Equal(jointTourSequence, tour.JointTourSequence);
			Assert.Equal(parentTourSequence, tour.ParentTourSequence);
			Assert.Equal(subtours, tour.Subtours);
			Assert.Equal(destinationPurpose, tour.DestinationPurpose);
			Assert.Equal(originDepartureTime, tour.OriginDepartureTime);
			Assert.Equal(destinationArrivalTime, tour.DestinationArrivalTime);
			Assert.Equal(destinationDepartureTime, tour.DestinationDepartureTime);
			Assert.Equal(originArrivalTime, tour.OriginArrivalTime);
			Assert.Equal(originAddressType, tour.OriginAddressType);
			Assert.Equal(destinationAddressType, tour.DestinationAddressType);
			Assert.Equal(originParcelId, tour.OriginParcelId);
			Assert.Equal(originZoneKey, tour.OriginZoneKey);
			Assert.Equal(destinationParcelId, tour.DestinationParcelId);
			Assert.Equal(destinationZoneKey, tour.DestinationZoneKey);
			Assert.Equal(mode, tour.Mode);
			Assert.Equal(pathType, tour.PathType);
			Assert.Equal(autoTimeOneWay, tour.AutoTimeOneWay);
			Assert.Equal(autoCostOneWay, tour.AutoCostOneWay);
			Assert.Equal(autoDistanceOneWay, tour.AutoDistanceOneWay);
			Assert.Equal(halfTour1Trips, tour.HalfTour1Trips);
			Assert.Equal(halfTour2Trips, tour.HalfTour2Trips);
			Assert.Equal(partialHalfTour1Sequence, tour.PartialHalfTour1Sequence);
			Assert.Equal(partialHalfTour2Sequence, tour.PartialHalfTour2Sequence);
			Assert.Equal(fullHalfTour1Sequence, tour.FullHalfTour1Sequence);
			Assert.Equal(fullHalfTour2Sequence, tour.FullHalfTour2Sequence);
			Assert.Equal(expansionFactor, tour.ExpansionFactor);
		}

		[Fact]
		public void TestTourWrapper()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.BUSINESS;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			TourWrapper wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);

			Assert.Equal(null, wrapper.ParentTour);
			Assert.Equal(destinationDepartureTime, wrapper.DestinationDepartureTime);
			Assert.Equal(destinationArrivalTime, wrapper.DestinationArrivalTime);
			Assert.Equal(destinationPurpose, wrapper.DestinationPurpose);
			Assert.Equal(destinationParcel, wrapper.DestinationParcel);
			Assert.Equal(originParcel, wrapper.OriginParcel);
			Assert.Equal(person, wrapper.Person);
			Assert.Equal(personDay, wrapper.PersonDay);

			Assert.Equal(0, wrapper.DestinationAddressType);
			Assert.Equal(null, wrapper.DestinationArrivalBigPeriod);
			Assert.Equal(false, wrapper.DestinationModeAndTimeHaveBeenSimulated);
			Assert.Equal(0, wrapper.DestinationParcelId);
			Assert.Equal(0, wrapper.DestinationZoneKey);
			Assert.Equal(0, wrapper.EarliestOriginDepartureTime);
			Assert.Equal(0, wrapper.FullHalfTour1Sequence);
			Assert.Equal(0, wrapper.FullHalfTour2Sequence);
			Assert.Equal(null, wrapper.GetHalfTour(Constants.TourDirection.DESTINATION_TO_ORIGIN));
			Assert.Equal(null, wrapper.GetHalfTour(Constants.TourDirection.ORIGIN_TO_DESTINATION));
			Assert.Equal(false, wrapper.HalfTour1HasBeenSimulated);
			Assert.Equal(0, wrapper.HalfTour1Trips);
			Assert.Equal(false, wrapper.HalfTour2HasBeenSimulated);
			Assert.Equal(0, wrapper.HalfTour2Trips);
//			Assert.Equal(false, wrapper.HasSubtours);
			Assert.Equal(0, wrapper.IndicatedTravelTimeFromDestination);
			Assert.Equal(0, wrapper.IndicatedTravelTimeToDestination);

			
			Assert.Equal(false, wrapper.IsHomeBasedTour);
			Assert.Equal(false, wrapper.IsMissingData);
			Assert.Equal(false, wrapper.IsParkAndRideMode);
			
			
		}

		[Fact]
		public void TestTourWrapper2()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.BUSINESS;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			Tour subtour = new Tour();
			TourWrapper tour = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			TourWrapper wrapper = new TourWrapper(subtour, tour);

			Assert.Equal(tour, wrapper.ParentTour);
			Assert.Equal(1261, wrapper.DestinationDepartureTime);
			Assert.Equal(1261, wrapper.DestinationArrivalTime);
			Assert.Equal(0, wrapper.DestinationPurpose);
			Assert.Equal(null, wrapper.DestinationParcel);
			Assert.Equal(null, wrapper.OriginParcel);
			Assert.Equal(person, wrapper.Person);
			Assert.Equal(personDay, wrapper.PersonDay);

			Assert.Equal(0, wrapper.DestinationAddressType);
			Assert.Equal(null, wrapper.DestinationArrivalBigPeriod);
			Assert.Equal(false, wrapper.DestinationModeAndTimeHaveBeenSimulated);
			Assert.Equal(0, wrapper.DestinationParcelId);
			Assert.Equal(0, wrapper.DestinationZoneKey);
			Assert.Equal(0, wrapper.EarliestOriginDepartureTime);
			Assert.Equal(0, wrapper.FullHalfTour1Sequence);
			Assert.Equal(0, wrapper.FullHalfTour2Sequence);
			Assert.Equal(null, wrapper.GetHalfTour(Constants.TourDirection.DESTINATION_TO_ORIGIN));
			Assert.Equal(null, wrapper.GetHalfTour(Constants.TourDirection.ORIGIN_TO_DESTINATION));
			Assert.Equal(false, wrapper.HalfTour1HasBeenSimulated);
			Assert.Equal(0, wrapper.HalfTour1Trips);
			Assert.Equal(false, wrapper.HalfTour2HasBeenSimulated);
			Assert.Equal(0, wrapper.HalfTour2Trips);
//			Assert.Equal(false, wrapper.HasSubtours);
			Assert.Equal(0, wrapper.IndicatedTravelTimeFromDestination);
			Assert.Equal(0, wrapper.IndicatedTravelTimeToDestination);

			
			Assert.Equal(false, wrapper.IsHomeBasedTour);
			Assert.Equal(false, wrapper.IsMissingData);
			Assert.Equal(false, wrapper.IsParkAndRideMode);
			
			
		}

		[Fact]
		public void TestTourWrapper3()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			int destinationPurpose = Constants.Purpose.BUSINESS;
			
			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			Tour tour = new Tour();
			TourWrapper wrapper = new TourWrapper(tour, personDay, destinationPurpose, false);

			Assert.Equal(null, wrapper.ParentTour);
			Assert.Equal(1261, wrapper.DestinationDepartureTime);
			Assert.Equal(1261, wrapper.DestinationArrivalTime);
			Assert.Equal(0, wrapper.DestinationPurpose);
			Assert.Equal(null, wrapper.DestinationParcel);
			Assert.Equal(null, wrapper.OriginParcel);
			Assert.Equal(person, wrapper.Person);
			Assert.Equal(personDay, wrapper.PersonDay);

			Assert.Equal(0, wrapper.DestinationAddressType);
			Assert.Equal(null, wrapper.DestinationArrivalBigPeriod);
			Assert.Equal(false, wrapper.DestinationModeAndTimeHaveBeenSimulated);
			Assert.Equal(0, wrapper.DestinationParcelId);
			Assert.Equal(0, wrapper.DestinationZoneKey);
			Assert.Equal(0, wrapper.EarliestOriginDepartureTime);
			Assert.Equal(0, wrapper.FullHalfTour1Sequence);
			Assert.Equal(0, wrapper.FullHalfTour2Sequence);
			Assert.Equal(null, wrapper.GetHalfTour(Constants.TourDirection.DESTINATION_TO_ORIGIN));
			Assert.Equal(null, wrapper.GetHalfTour(Constants.TourDirection.ORIGIN_TO_DESTINATION));
			Assert.Equal(false, wrapper.HalfTour1HasBeenSimulated);
			Assert.Equal(0, wrapper.HalfTour1Trips);
			Assert.Equal(false, wrapper.HalfTour2HasBeenSimulated);
			Assert.Equal(0, wrapper.HalfTour2Trips);
			Assert.Equal(0, wrapper.IndicatedTravelTimeFromDestination);
			Assert.Equal(0, wrapper.IndicatedTravelTimeToDestination);

			
			Assert.Equal(true, wrapper.IsHomeBasedTour);//Set by the constructor
			Assert.Equal(false, wrapper.IsMissingData);
			Assert.Equal(false, wrapper.IsParkAndRideMode);
		}



		[Fact]
		public void TestTourWrapperPurpose()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.BUSINESS;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Work = .76;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Work = .46;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			TourWrapper wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);

			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.ESCORT;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(true, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.MEAL;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(true, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.MEDICAL;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(true, wrapper.IsMedicalPurpose);
			Assert.Equal(true, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.PERSONAL_BUSINESS;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(true, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.RECREATION;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(true, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(true, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.SCHOOL;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(true, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.SHOPPING;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(true, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.SOCIAL;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(true, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(true, wrapper.IsSocialPurpose);
			Assert.Equal(false, wrapper.IsWorkPurpose);

			destinationPurpose = Constants.Purpose.WORK;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurpose);
			Assert.Equal(false, wrapper.IsMealPurpose);
			Assert.Equal(false, wrapper.IsMedicalPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalPurpose);
			Assert.Equal(false, wrapper.IsRecreationPurpose);
			Assert.Equal(false, wrapper.IsSchoolPurpose);
			Assert.Equal(false, wrapper.IsShoppingPurpose);
			Assert.Equal(false, wrapper.IsSocialOrRecreationPurpose);
			Assert.Equal(false, wrapper.IsSocialPurpose);
			Assert.Equal(true, wrapper.IsWorkPurpose);

		}

		[Fact]
		public void TestTourWrapperMode()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};

			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income: -1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.BUSINESS;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			TourWrapper wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);

			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.BIKE;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(true, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(true, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.HOV2;
			Assert.Equal(true, wrapper.IsAnAutoMode);
			Assert.Equal(true, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(true, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.HOV3;
			Assert.Equal(true, wrapper.IsAnAutoMode);
			Assert.Equal(true, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(true, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.HOVDRIVER;
			Assert.Equal(true, wrapper.IsAnAutoMode);
			Assert.Equal(true, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(true, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.HOVPASSENGER;
			Assert.Equal(true, wrapper.IsAnAutoMode);
			Assert.Equal(true, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(true, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.NONE;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.OTHER;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.PARK_AND_RIDE;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.SCHOOL_BUS;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(true, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.SOV;
			Assert.Equal(true, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(true, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.TRANSIT;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(true, wrapper.IsTransitMode);
			Assert.Equal(false, wrapper.IsWalkMode);
			Assert.Equal(false, wrapper.IsWalkOrBikeMode);

			wrapper.Mode = Constants.Mode.WALK;
			Assert.Equal(false, wrapper.IsAnAutoMode);
			Assert.Equal(false, wrapper.IsAnHovMode);
			Assert.Equal(false, wrapper.IsBikeMode);
			Assert.Equal(false, wrapper.IsHov2Mode);
			Assert.Equal(false, wrapper.IsHov3Mode);
			Assert.Equal(false, wrapper.IsSchoolBusMode);
			Assert.Equal(false, wrapper.IsSovMode);
			Assert.Equal(false, wrapper.IsTransitMode);
			Assert.Equal(true, wrapper.IsWalkMode);
			Assert.Equal(true, wrapper.IsWalkOrBikeMode);
		}

		[Fact]
		public void TestTourWrapperCostCoefficient()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.BUSINESS;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			TourWrapper wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);

			Assert.Equal(5, wrapper.CostCoefficient);

			Global.Configuration.Coefficients_CostCoefficientIncomePower_Other = 2;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(5, wrapper.CostCoefficient);

			person = TestHelper.GetPersonWrapper(income:50000);
			personDay = TestHelper.GetPersonDayWrapper(income:50000);
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(1.25, wrapper.CostCoefficient);


			Global.Configuration.Coefficients_CostCoefficientIncomePower_Other = 8;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(5.0 / 256.0, wrapper.CostCoefficient);
		}

		[Fact]
		public void TestTourWrapperTimeCoefficient()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 1};
			PersonWrapper person = TestHelper.GetPersonWrapper();
			PersonDayWrapper personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			CondensedParcel originParcel = new CondensedParcel();
			CondensedParcel destinationParcel = new CondensedParcel();
			int destinationDepartureTime = 1;
			int destinationPurpose = Constants.Purpose.SCHOOL;
			int destinationArrivalTime = 3;

			Global.Configuration.Coefficients_BaseCostCoefficientIncomeLevel = 25000;
			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Other = .75;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Other = .45;

			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Work = .76;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Work = .46;
			Global.Configuration.Coefficients_BaseCostCoefficientPerMonetaryUnit = 5;
			TourWrapper wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);

			Assert.Equal(.45, wrapper.TimeCoefficient);

			destinationPurpose = Constants.Purpose.WORK;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(.46, wrapper.TimeCoefficient);

			Global.Configuration.UseRandomVotDistribution = true;
			wrapper = new TourWrapper(person, personDay, originParcel, destinationParcel, destinationArrivalTime,
			                                      destinationDepartureTime, destinationPurpose);
			Assert.Equal(.46, wrapper.TimeCoefficient);//This constructor suppresses randomVOT


			person = TestHelper.GetPersonWrapper();
			personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			destinationPurpose = Constants.Purpose.BUSINESS;
			Tour tour = new Tour();
			wrapper = new TourWrapper(tour, personDay, destinationPurpose, false);
			Assert.Equal(-.001, wrapper.TimeCoefficient);//This constructor does not suppress randomVOT

			Global.Configuration.Coefficients_StdDeviationTimeCoefficient_Work = .8;
			Global.Configuration.Coefficients_MeanTimeCoefficient_Work = -.03;
			person = TestHelper.GetPersonWrapper();
			personDay = TestHelper.GetPersonDayWrapper(personWrapper: person, income:-1);
			destinationPurpose = Constants.Purpose.WORK;
			tour = new Tour();
			wrapper = new TourWrapper(tour, personDay, destinationPurpose, false);
			Assert.Equal(-.0303002, Math.Round(wrapper.TimeCoefficient, 7));//This constructor does not suppress randomVOT
		}
	}
}
