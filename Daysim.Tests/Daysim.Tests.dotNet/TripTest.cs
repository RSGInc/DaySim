using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.Framework.Core;
using Xunit;


namespace Daysim.Tests 
{
	
	public class TripTest 
	{
		[Fact]
		public void TestTrip()
		{
			const int id = 1;
			const int tourId = 2;
			const int householdId = 3;
			const int personSequence = 4;
			const int day = 5;
			const int tourSequence = 6;
			const int halfTour = 7;
			const int sequence = 8;
			const int surveyTripSequence = 9;
			const int originPurpose = 10;
			const int destinationPurpose = 11;
			const int originAddressType = 12;
			const int destinationAddressType = 13;
			const int originParcelId = 14;
			const int originZoneKey = 15;
			const int destinationParcelId = 16;
			const int destinationZoneKey = 17;
			const int mode = 18;
			const int pathType = 19;
			const int driverType = 20;
			const int departureTime = 21;
			const int arrivalTime = 22;
			const int activityEndTime = 23;
			const double travelTime = 24.01;
			const double travelCost = 25.01;
			const double travelDistance = 26.01;
			const double valueOfTime = 27.01;
			const double expansionFactor = 28.01;

			Trip trip = new Trip
				            {
					            ActivityEndTime = activityEndTime,
					            ArrivalTime = arrivalTime,
					            Day = day,
					            DepartureTime = departureTime,
					            DestinationAddressType = destinationAddressType,
					            DestinationParcelId = destinationParcelId,
					            DestinationPurpose = destinationPurpose,
					            DestinationZoneKey = destinationZoneKey,
					            DriverType = driverType,
					            ExpansionFactor = expansionFactor,
					            HalfTour = halfTour,
					            HouseholdId = householdId,
					            Id = id,
					            Mode = mode,
					            OriginAddressType = originAddressType,
					            OriginParcelId = originParcelId,
					            OriginPurpose = originPurpose,
					            OriginZoneKey = originZoneKey,
					            PathType = pathType,
					            PersonSequence = personSequence,
					            Sequence = sequence,
					            SurveyTripSequence = surveyTripSequence,
					            TourId = tourId,
					            TourSequence = tourSequence,
					            TravelCost = travelCost,
					            TravelDistance = travelDistance,
					            TravelTime = travelTime,
					            ValueOfTime = valueOfTime,
				            };

			Assert.Equal(id, trip.Id);
			Assert.Equal(tourId, trip.TourId);
			Assert.Equal(householdId, trip.HouseholdId);
			Assert.Equal(personSequence, trip.PersonSequence);
			Assert.Equal(day, trip.Day);
			Assert.Equal(tourSequence, trip.TourSequence);
			Assert.Equal(halfTour, trip.HalfTour);
			Assert.Equal(sequence, trip.Sequence);
			Assert.Equal(surveyTripSequence, trip.SurveyTripSequence);
			Assert.Equal(originPurpose, trip.OriginPurpose);
			Assert.Equal(destinationPurpose, trip.DestinationPurpose);
			Assert.Equal(originAddressType, trip.OriginAddressType);
			Assert.Equal(destinationAddressType, trip.DestinationAddressType);
			Assert.Equal(originParcelId, trip.OriginParcelId);
			Assert.Equal(originZoneKey, trip.OriginZoneKey);
			Assert.Equal(destinationParcelId, trip.DestinationParcelId);
			Assert.Equal(destinationZoneKey, trip.DestinationZoneKey);
			Assert.Equal(mode, trip.Mode);
			Assert.Equal(pathType, trip.PathType);
			Assert.Equal(driverType, trip.DriverType);
			Assert.Equal(departureTime, trip.DepartureTime);
			Assert.Equal(arrivalTime, trip.ArrivalTime);
			Assert.Equal(activityEndTime, trip.ActivityEndTime);
			Assert.Equal(travelTime, trip.TravelTime);
			Assert.Equal(travelCost, trip.TravelCost);
			Assert.Equal(travelDistance, trip.TravelDistance);
			Assert.Equal(valueOfTime, trip.ValueOfTime);
			Assert.Equal(expansionFactor, trip.ExpansionFactor);
		}

		[Fact]
		public void TestTripWrapper()
		{
			Global.Configuration = new Configuration();
			Global.Configuration.HouseholdSamplingRateOneInX = 256;

			int id = 1;
			int tourId = 2;
			int householdId = 3;
			int personSequence = 4;
			int day = 5;
			int tourSequence = 6;
			int halfTourId = 7;
			int sequence = 8;
			int surveyTripSequence = 9;
			int originPurpose = 10;
			int destinationPurpose = 11;
			int originAddressType = 12;
			int destinationAddressType = 13;
			int originParcelId = 14;
			int originZoneKey = 15;
			int destinationParcelId = 16;
			int destinationZoneKey = 17;
			int mode = 18;
			int pathType = 19;
			int driverType = 20;
			int departureTime = 21;
			int arrivalTime = 22;
			int activityEndTime = 23;
			double travelTime = 24.01;
			double travelCost = 25.01;
			double travelDistance = 26.01;
			double valueOfTime = 27.01;
			double expansionFactor = 28.01;

			Trip trip = new Trip
				            {
					            ActivityEndTime = activityEndTime,
					            ArrivalTime = arrivalTime,
					            Day = day,
					            DepartureTime = departureTime,
					            DestinationAddressType = destinationAddressType,
					            DestinationParcelId = destinationParcelId,
					            DestinationPurpose = destinationPurpose,
					            DestinationZoneKey = destinationZoneKey,
					            DriverType = driverType,
					            ExpansionFactor = expansionFactor,
					            HalfTour = halfTourId,
					            HouseholdId = householdId,
					            Id = id,
					            Mode = mode,
					            OriginAddressType = originAddressType,
					            OriginParcelId = originParcelId,
					            OriginPurpose = originPurpose,
					            OriginZoneKey = originZoneKey,
					            PathType = pathType,
					            PersonSequence = personSequence,
					            Sequence = sequence,
					            SurveyTripSequence = surveyTripSequence,
					            TourId = tourId,
					            TourSequence = tourSequence,
					            TravelCost = travelCost,
					            TravelDistance = travelDistance,
					            TravelTime = travelTime,
					            ValueOfTime = valueOfTime,
				            };

			TourWrapper tour = TestHelper.GetTourWrapper();
			TourWrapper.HalfTour halfTour = new TourWrapper.HalfTour(tour);
			TripWrapper wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(id, wrapper.Id);
			Assert.Equal(tour, wrapper.Tour);
			Assert.Equal(tour.Household, wrapper.Household);
			Assert.Equal(tour.Person, wrapper.Person);
			Assert.Equal(day, wrapper.Day);
			Assert.Equal(halfTour, wrapper.HalfTour);
			Assert.Equal(sequence, wrapper.Sequence);
			Assert.Equal(originPurpose, wrapper.OriginPurpose);
			Assert.Equal(destinationPurpose, wrapper.DestinationPurpose);
			Assert.Equal(destinationAddressType, wrapper.DestinationAddressType);
			Assert.Equal(originParcelId, wrapper.OriginParcelId);
			Assert.Equal(originZoneKey, wrapper.OriginZoneKey);
			Assert.Equal(destinationParcelId, wrapper.DestinationParcelId);
			Assert.Equal(destinationZoneKey, wrapper.DestinationZoneKey);
			Assert.Equal(mode, wrapper.Mode);
			Assert.Equal(pathType, wrapper.PathType);
			Assert.Equal(driverType, wrapper.DriverType);
			Assert.Equal(departureTime.ToMinutesAfter3AM(), wrapper.DepartureTime);
			Assert.Equal(arrivalTime.ToMinutesAfter3AM(), wrapper.ArrivalTime);
			Assert.Equal(activityEndTime.ToMinutesAfter3AM(), wrapper.ActivityEndTime);
			Assert.Equal(valueOfTime, wrapper.ValueOfTime);

			int newDepartureTime = 100;
			wrapper.DepartureTime = newDepartureTime;
			Assert.Equal(newDepartureTime.ToMinutesAfterMidnight().ToMinutesAfter3AM(), wrapper.DepartureTime);

			Assert.Equal(0, wrapper.ArrivalTimeLimit);

			wrapper.ArrivalTimeLimit = 2;
			Assert.Equal(2, wrapper.ArrivalTimeLimit);

			Assert.Equal(0, wrapper.EarliestDepartureTime);
			wrapper.EarliestDepartureTime = 2;
			Assert.Equal(2, wrapper.EarliestDepartureTime);

			Assert.Equal(0, wrapper.LatestDepartureTime);
			wrapper.LatestDepartureTime = 2;
			Assert.Equal(2, wrapper.LatestDepartureTime);


			Assert.Equal(null, wrapper.DestinationParcel);
			CondensedParcel destinationParcel = new CondensedParcel();
			wrapper.DestinationParcel = destinationParcel;
			Assert.Equal(destinationParcel, wrapper.DestinationParcel);


			Assert.Equal(false,wrapper.IsMissingData);
			wrapper.IsMissingData = true;
			Assert.Equal(true,wrapper.IsMissingData);
			/*TripModeImpedance[] impedances = wrapper.GetTripModeImpedances();
			wrapper.HUpdateTripValues();
			wrapper.SetActivityEndTime();
			wrapper.SetDriverOrPassenger();
			wrapper.SetOriginAddressType();
			wrapper.SetTourSequence();
			wrapper.SetTripValueOfTime();
			wrapper.Invert();
			*/
		}

		[Fact]
		public void TestTripWrapperIsBeforeMandatoryDestination()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 256};
			Trip trip = new Trip();
			TourWrapper tour = TestHelper.GetTourWrapper();
			TourWrapper.HalfTour halfTour = new TourWrapper.HalfTour(tour);
			TripWrapper wrapper = new TripWrapper(trip, tour, halfTour);
			
			Assert.Equal(false,wrapper.IsBeforeMandatoryDestination);

			trip = new Trip{HalfTour	= 1};
			wrapper = new TripWrapper(trip, tour, halfTour);
			tour.DestinationPurpose = Constants.Purpose.MEDICAL;
			Assert.Equal(false,wrapper.IsBeforeMandatoryDestination);

			tour.DestinationPurpose = Constants.Purpose.WORK;
			Assert.Equal(true,wrapper.IsBeforeMandatoryDestination);

			tour.DestinationPurpose = Constants.Purpose.PERSONAL_BUSINESS;
			Assert.Equal(false,wrapper.IsBeforeMandatoryDestination);

			tour.DestinationPurpose = Constants.Purpose.SCHOOL;
			Assert.Equal(true,wrapper.IsBeforeMandatoryDestination);

			trip = new Trip{HalfTour	= 0};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsBeforeMandatoryDestination);

			tour.DestinationPurpose = Constants.Purpose.WORK;
			Assert.Equal(false,wrapper.IsBeforeMandatoryDestination);
		}

		[Fact]
		public void TestTripWrapperIsOrigin()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 256};
			Trip trip = new Trip();
			TourWrapper tour = TestHelper.GetTourWrapper();
			TourWrapper.HalfTour halfTour = new TourWrapper.HalfTour(tour);
			TripWrapper wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(true,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.BUSINESS};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.ESCORT};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(true,wrapper.IsEscortOriginPurpose);
			Assert.Equal(true,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.MEAL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(true,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);


			trip = new Trip{OriginPurpose = Constants.Purpose.MEDICAL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);


			trip = new Trip{OriginPurpose = Constants.Purpose.PERSONAL_BUSINESS};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(true,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.RECREATION};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.SCHOOL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(true,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.SHOPPING};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(true,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.SOCIAL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(true,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(false,wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{OriginPurpose = Constants.Purpose.WORK};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false,wrapper.IsEscortOriginPurpose);
			Assert.Equal(false,wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false,wrapper.IsHalfTourFromOrigin);
			Assert.Equal(false,wrapper.IsMealOriginPurpose);
			Assert.Equal(false,wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false,wrapper.IsPersonalBusinessOriginPurpose);
			Assert.Equal(false,wrapper.IsSchoolOriginPurpose);
			Assert.Equal(false,wrapper.IsShoppingOriginPurpose);
			Assert.Equal(false,wrapper.IsSocialOriginPurpose);
			Assert.Equal(false,wrapper.IsToTourOrigin);
			Assert.Equal(true,wrapper.IsWorkPurposeByOrigin);
			
		}

		[Fact]
		public void TestTripWrapperIsDestination()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 256};
			Trip trip = new Trip();
			TourWrapper tour = TestHelper.GetTourWrapper();
			TourWrapper.HalfTour halfTour = new TourWrapper.HalfTour(tour);
			TripWrapper wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(true, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.BUSINESS};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.ESCORT};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(true, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(true, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.MEAL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(true, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(true, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.MEDICAL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(true, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(true, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.PERSONAL_BUSINESS};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(true, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(true, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(true, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.RECREATION};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(true, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.SCHOOL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(true, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(true, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.SHOPPING};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(true, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(true, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.SOCIAL};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(true, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(true, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);

			trip = new Trip {DestinationPurpose = Constants.Purpose.WORK};
			wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(false, wrapper.IsEscortDestinationPurpose);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsMealDestinationPurpose);
			Assert.Equal(false, wrapper.IsMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsPersonalBusinessDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalBusinessOrMedicalDestinationPurpose);
			Assert.Equal(false, wrapper.IsPersonalReasonsDestinationPurpose);
			Assert.Equal(false, wrapper.IsRecreationDestinationPurpose);
			Assert.Equal(false, wrapper.IsSchoolDestinationPurpose);
			Assert.Equal(false, wrapper.IsShoppingDestinationPurpose);
			Assert.Equal(false, wrapper.IsSocialDestinationPurpose);
			Assert.Equal(true, wrapper.IsWorkDestinationPurpose);
			Assert.Equal(true, wrapper.IsWorkOrSchoolDestinationPurpose);
			Assert.Equal(true, wrapper.IsWorkPurposeByDestination);
		}


		[Fact]
		public void TestTripWrapperByOriginDestination()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 256};
			Trip trip = new Trip{HalfTour = 1, OriginPurpose = Constants.Purpose.ESCORT, DestinationPurpose = Constants.Purpose.NONE_OR_HOME};
			TourWrapper tour = TestHelper.GetTourWrapper();
			TourWrapper.HalfTour halfTour = new TourWrapper.HalfTour(tour);
			TripWrapper wrapper = new TripWrapper(trip, tour, halfTour);

			Assert.Equal(true, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);
			Assert.Equal(false, wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(true, wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false, wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{HalfTour = 1, OriginPurpose = Constants.Purpose.NONE_OR_HOME, DestinationPurpose = Constants.Purpose.WORK};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(true, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);
			Assert.Equal(false, wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(true, wrapper.IsWorkPurposeByOrigin);
			
			trip = new Trip{HalfTour = 1, OriginPurpose = Constants.Purpose.WORK, DestinationPurpose = Constants.Purpose.ESCORT};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(true, wrapper.IsWorkPurposeByDestination);
			Assert.Equal(true, wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false, wrapper.IsWorkPurposeByOrigin);

			trip = new Trip{HalfTour = 1, OriginPurpose = Constants.Purpose.BUSINESS, DestinationPurpose = Constants.Purpose.BUSINESS};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false, wrapper.IsEscortPurposeByDestination);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByDestination);
			Assert.Equal(false, wrapper.IsWorkPurposeByDestination);
			Assert.Equal(false, wrapper.IsEscortPurposeByOrigin);
			Assert.Equal(false, wrapper.IsNoneOrHomePurposeByOrigin);
			Assert.Equal(false, wrapper.IsWorkPurposeByOrigin);


		}

		[Fact]
		public void TestTripWrapperIsOther()
		{
			Global.Configuration = new Configuration {HouseholdSamplingRateOneInX = 256};
			Trip trip = new Trip{Mode = Constants.Mode.BIKE};
			TourWrapper tour = TestHelper.GetTourWrapper();
			TourWrapper.HalfTour halfTour = new TourWrapper.HalfTour(tour);
			TripWrapper wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);

			trip = new Trip{Mode = Constants.Mode.HOV2};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(true,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.HOV3};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(true,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.HOVDRIVER};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(true,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.HOVPASSENGER};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(true,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.NONE};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.OTHER};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.PARK_AND_RIDE};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.SCHOOL_BUS};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.SOV};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(true,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.TRANSIT};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(true,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);
			
			trip = new Trip{Mode = Constants.Mode.WALK};
			wrapper = new TripWrapper(trip, tour, halfTour);
			Assert.Equal(false,wrapper.IsTransitMode);
			Assert.Equal(false,wrapper.UsesSovOrHovModes);
			
		}
	
	}
}
