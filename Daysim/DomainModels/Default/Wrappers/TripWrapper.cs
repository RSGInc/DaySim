// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.ChoiceModels;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Persisters;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;
using Daysim.Framework.Sampling;
using Daysim.PathTypeModels;
using Ninject;

namespace Daysim.DomainModels.Default.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
	public class TripWrapper : ISamplingTrip, ITripWrapper {
		private readonly ITrip _trip;

		private readonly IPersisterExporter _exporter;

		[UsedImplicitly]
		public TripWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour) {
			_trip = trip;

			_exporter =
				Global
					.Kernel
					.Get<IPersistenceFactory<ITrip>>()
					.Exporter;

			// relations properties

			Household = tourWrapper.Household;
			Person = tourWrapper.Person;
			PersonDay = tourWrapper.PersonDay;
			Tour = tourWrapper;
			HalfTour = halfTour;

			SetParcelRelationships(trip);

			// flags/choice model/etc. properties

			IsHalfTourFromOrigin = Direction == Global.Settings.TourDirections.OriginToDestination;
		}

		#region relations properties

		public IHouseholdWrapper Household { get; set; }

		public IPersonWrapper Person { get; set; }

		public IPersonDayWrapper PersonDay { get; set; }

		public ITourWrapper Tour { get; set; }

		public IHalfTour HalfTour { get; set; }

		public IParcelWrapper OriginParcel { get; set; }

		IParcel ISamplingTrip.OriginParcel {
			get { return OriginParcel; }
		}

		public IParcelWrapper DestinationParcel { get; set; }

		#endregion

		#region domain model properies

		public int Id {
			get { return _trip.Id; }
			set { _trip.Id = value; }
		}

		public int TourId {
			get { return _trip.TourId; }
			set { _trip.TourId = value; }
		}

		public int HouseholdId {
			get { return _trip.HouseholdId; }
			set { _trip.HouseholdId = value; }
		}

		public int PersonSequence {
			get { return _trip.PersonSequence; }
			set { _trip.PersonSequence = value; }
		}

		public int Day {
			get { return _trip.Day; }
			set { _trip.Day = value; }
		}

		public int TourSequence {
			get { return _trip.TourSequence; }
			set { _trip.TourSequence = value; }
		}

		public int Direction {
			get { return _trip.Direction; }
			set { _trip.Direction = value; }
		}

		public int Sequence {
			get { return _trip.Sequence; }
			set { _trip.Sequence = value; }
		}

		public int SurveyTripSequence {
			get { return _trip.SurveyTripSequence; }
			set { _trip.SurveyTripSequence = value; }
		}

		public int OriginPurpose {
			get { return _trip.OriginPurpose; }
			set { _trip.OriginPurpose = value; }
		}

		public int DestinationPurpose {
			get { return _trip.DestinationPurpose; }
			set { _trip.DestinationPurpose = value; }
		}

		public int OriginAddressType {
			get { return _trip.OriginAddressType; }
			set { _trip.OriginAddressType = value; }
		}

		public int DestinationAddressType {
			get { return _trip.DestinationAddressType; }
			set { _trip.DestinationAddressType = value; }
		}

		public int OriginParcelId {
			get { return _trip.OriginParcelId; }
			set { _trip.OriginParcelId = value; }
		}

		public int OriginZoneKey {
			get { return _trip.OriginZoneKey; }
			set { _trip.OriginZoneKey = value; }
		}

		public int DestinationParcelId {
			get { return _trip.DestinationParcelId; }
			set { _trip.DestinationParcelId = value; }
		}

		public int DestinationZoneKey {
			get { return _trip.DestinationZoneKey; }
			set { _trip.DestinationZoneKey = value; }
		}

		public int Mode {
			get { return _trip.Mode; }
			set { _trip.Mode = value; }
		}

		public int PathType {
			get { return _trip.PathType; }
			set { _trip.PathType = value; }
		}

		public int DriverType {
			get { return _trip.DriverType; }
			set { _trip.DriverType = value; }
		}

		public int DepartureTime {
			get { return _trip.DepartureTime.ToMinutesAfter3AM(); }
			set { _trip.DepartureTime = value.ToMinutesAfterMidnight(); }
		}

		public int ArrivalTime {
			get { return _trip.ArrivalTime.ToMinutesAfter3AM(); }
			set { _trip.ArrivalTime = value.ToMinutesAfterMidnight(); }
		}

		public int ActivityEndTime {
			get { return _trip.ActivityEndTime.ToMinutesAfter3AM(); }
			set { _trip.ActivityEndTime = value.ToMinutesAfterMidnight(); }
		}

		public double TravelTime {
			get { return _trip.TravelTime; }
			set { _trip.TravelTime = value; }
		}

		public double TravelCost {
			get { return _trip.TravelCost; }
			set { _trip.TravelCost = value; }
		}

		public double TravelDistance {
			get { return _trip.TravelDistance; }
			set { _trip.TravelDistance = value; }
		}

		public double ValueOfTime {
			get { return _trip.ValueOfTime; }
			set { _trip.ValueOfTime = value; }
		}

		public double ExpansionFactor {
			get { return _trip.ExpansionFactor; }
			set { _trip.ExpansionFactor = value; }
		}

		//JLB20160323
		public int BikePTCombination {
			get { return _trip.BikePTCombination; }
			set { _trip.BikePTCombination = value; }
		}

		public int EscortedDestinationPurpose {
			get { return _trip.EscortedDestinationPurpose; }
			set { _trip.EscortedDestinationPurpose = value; }
		}

		public int AccessMode {
			get { return _trip.AccessMode; }
			set { _trip.AccessMode = value; }
		}

		public int AccessPathType {
			get { return _trip.AccessPathType; }
			set { _trip.AccessPathType = value; }
		}

		public double AccessTime {
			get { return _trip.AccessTime; }
			set { _trip.AccessTime = value; }
		}

		public double AccessCost {
			get { return _trip.AccessCost; }
			set { _trip.AccessCost = value; }
		}

		public double AccessDistance {
			get { return _trip.AccessDistance; }
			set { _trip.AccessDistance = value; }
		}

		public int AccessStopArea {
			get { return _trip.AccessStopArea; }
			set { _trip.AccessStopArea = value; }
		}

		public int EgressMode {
			get { return _trip.EgressMode; }
			set { _trip.EgressMode = value; }
		}

		public int EgressPathType {
			get { return _trip.EgressPathType; }
			set { _trip.EgressPathType = value; }
		}

		public double EgressTime {
			get { return _trip.EgressTime; }
			set { _trip.EgressTime = value; }
		}

		public double EgressCost {
			get { return _trip.EgressCost; }
			set { _trip.EgressCost = value; }
		}

		public double EgressDistance {
			get { return _trip.EgressDistance; }
			set { _trip.EgressDistance = value; }
		}

		public int EgressStopArea {
			get { return _trip.EgressStopArea; }
			set { _trip.EgressStopArea = value; }
		}



		#endregion

		#region flags/choice model/etc. properties

		public int EarliestDepartureTime { get; set; }

		public int LatestDepartureTime { get; set; }

		public int ArrivalTimeLimit { get; set; }

		public bool IsHalfTourFromOrigin { get; set; }

		public bool IsToTourOrigin { get; set; }

		public bool IsMissingData { get; set; }

		#endregion

		#region wrapper methods

		public virtual bool IsNoneOrHomePurposeByOrigin() {
			var purpose =
				IsHalfTourFromOrigin
					? DestinationPurpose
					: OriginPurpose;

			return purpose == Global.Settings.Purposes.NoneOrHome;
		}

		public virtual bool IsWorkPurposeByOrigin() {
			var purpose =
				IsHalfTourFromOrigin
					? DestinationPurpose
					: OriginPurpose;

			return purpose == Global.Settings.Purposes.Work;
		}

		public virtual bool IsEscortPurposeByOrigin() {
			var purpose =
				IsHalfTourFromOrigin
					? DestinationPurpose
					: OriginPurpose;

			return purpose == Global.Settings.Purposes.Escort;
		}

		public virtual bool IsNoneOrHomePurposeByDestination() {
			var purpose =
				IsHalfTourFromOrigin
					? OriginPurpose
					: DestinationPurpose;

			return purpose == Global.Settings.Purposes.NoneOrHome;
		}

		public virtual bool IsWorkPurposeByDestination() {
			var purpose =
				IsHalfTourFromOrigin
					? OriginPurpose
					: DestinationPurpose;

			return purpose == Global.Settings.Purposes.Work;
		}

		public virtual bool IsEscortPurposeByDestination() {
			var purpose =
				IsHalfTourFromOrigin
					? OriginPurpose
					: DestinationPurpose;

			return purpose == Global.Settings.Purposes.Escort;
		}

		public virtual bool IsWorkDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Work;
		}

		public virtual bool IsSchoolDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.School;
		}

		public virtual bool IsEscortDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Escort;
		}

		public virtual bool IsPersonalBusinessDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.PersonalBusiness;
		}

		public virtual bool IsShoppingDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Shopping;
		}

		public virtual bool IsMealDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Meal;
		}

		public virtual bool IsSocialDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Social;
		}

		public virtual bool IsRecreationDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Recreation;
		}

		public virtual bool IsMedicalDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Medical;
		}

		public virtual bool IsPersonalBusinessOrMedicalDestinationPurpose() {
			return IsPersonalBusinessDestinationPurpose() || IsMedicalDestinationPurpose();
		}

		public virtual bool IsWorkOrSchoolDestinationPurpose() {
			return IsWorkDestinationPurpose() || IsSchoolDestinationPurpose();
		}

		public virtual bool IsPersonalReasonsDestinationPurpose() {
			return IsMealDestinationPurpose() || IsPersonalBusinessDestinationPurpose() || IsShoppingDestinationPurpose() || IsSocialDestinationPurpose();
		}

		public virtual bool IsSchoolOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.School;
		}

		public virtual bool IsEscortOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.Escort;
		}

		public virtual bool IsShoppingOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.Shopping;
		}

		public virtual bool IsPersonalBusinessOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.PersonalBusiness;
		}

		public virtual bool IsMealOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.Meal;
		}

		public virtual bool IsSocialOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.Social;
		}

		public virtual bool UsesSovOrHovModes() {
			return IsSovMode() || IsHov2Mode() || IsHov3Mode();
		}

		public virtual bool IsWalkMode() {
			return Mode == Global.Settings.Modes.Walk;
		}

		public virtual bool IsBikeMode() {
			return Mode == Global.Settings.Modes.Bike;
		}

		public virtual bool IsSovMode() {
			return Mode == Global.Settings.Modes.Sov;
		}

		public virtual bool IsHov2Mode() {
			return Mode == Global.Settings.Modes.Hov2;
		}

		public virtual bool IsHov3Mode() {
			return Mode == Global.Settings.Modes.Hov3;
		}

		public virtual bool IsTransitMode() {
			return Mode == Global.Settings.Modes.Transit;
		}

		public virtual bool IsBeforeMandatoryDestination() {
			return Direction == 1 && (Tour.IsWorkPurpose() || Tour.IsSchoolPurpose());
		}

		public virtual ITripWrapper GetPreviousTrip() {
			var index = Sequence - 1;
			var previousTripIndex = index - 1;

			return HalfTour.Trips[previousTripIndex];
		}

		public virtual ITripWrapper GetNextTrip() {
			var index = Sequence - 1;
			var nextTripIndex = index + 1;

			return nextTripIndex < HalfTour.Trips.Count ? HalfTour.Trips[nextTripIndex] : null;
		}

		public virtual int GetStartTime() {
			if (IsHalfTourFromOrigin && Sequence == 1) {
				return Tour.DestinationArrivalTime;
			}

			if (!IsHalfTourFromOrigin && Sequence == 1) {
				return Tour.DestinationDepartureTime;
			}

			return GetPreviousTrip().ArrivalTime; // arrival time of prior trip to prior stop location
		}

		public virtual void SetDriverOrPassenger(List<ITripWrapper> trips) {
			if (Mode == Global.Settings.Modes.Sov || Mode == Global.Settings.Modes.HovDriver) {
				DriverType = Global.Settings.DriverTypes.Driver;
			}
			else if (Mode == Global.Settings.Modes.HovPassenger) {
				DriverType = Global.Settings.DriverTypes.Passenger;
			}
			else {
				DriverType = Global.Settings.DriverTypes.NotApplicable;
			}
		}


		//		public virtual void SetDriverOrPassenger(List<ITripWrapper> trips) {
		//			if (Mode == Global.Settings.Modes.Walk || Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Transit || Mode == Global.Settings.Modes.SchoolBus || Mode == Global.Settings.Modes.Other) {
		//				DriverType = Global.Settings.DriverTypes.NotApplicable;
		//			}
		//			else if (Mode == Global.Settings.Modes.Sov) {
		//				DriverType = Global.Settings.DriverTypes.Driver;
		//			}
		//			else if (Mode == Global.Settings.Modes.Hov2 || Mode == Global.Settings.Modes.Hov3) {
		//				if (Person.IsChildUnder16 || Household.VehiclesAvailable == 0 || (!Tour.IsHov2Mode() && !Tour.IsHov3Mode())) {
		//					DriverType = Global.Settings.DriverTypes.Passenger;
		//				}
		//				else {
		//					if (trips.Any(t => ((TripWrapper) t).IsWalkMode() || ((TripWrapper) t).IsBikeMode())) {
		//						DriverType = Global.Settings.DriverTypes.Passenger;
		//					}
		//					else if (trips.Any(t => ((TripWrapper) t).IsSovMode())) {
		//						DriverType = Global.Settings.DriverTypes.Driver;
		//					}
		//					else if (trips.Any(t => ((TripWrapper) t).IsHov2Mode())) {
		//						var randomNumber = Household.RandomUtility.Uniform01();
		//
		//						DriverType = randomNumber > .799 ? Global.Settings.DriverTypes.Passenger : Global.Settings.DriverTypes.Driver;
		//					}
		//					else {
		//						// HOV3 mode
		//						var randomNumber = Household.RandomUtility.Uniform01();
		//
		//						DriverType = randomNumber > .492 ? Global.Settings.DriverTypes.Passenger : Global.Settings.DriverTypes.Driver;
		//					}
		//				}
		//			}
		//		}

		public virtual void UpdateTripValues() {
			if (Global.Configuration.IsInEstimationMode || ((Global.Configuration.ShouldRunTourTripModels || Global.Configuration.ShouldRunSubtourTripModels) && !Global.Configuration.ShouldRunIntermediateStopLocationModel) || ((Global.Configuration.ShouldRunTourTripModels || Global.Configuration.ShouldRunSubtourTripModels) && !Global.Configuration.ShouldRunTripModeModel) || ((Global.Configuration.ShouldRunTourTripModels || Global.Configuration.ShouldRunSubtourTripModels) && !Global.Configuration.ShouldRunTripTimeModel)) {
				return;
			}

			var modeImpedance = GetTripModeImpedance(DepartureTime, true);

			TravelTime = modeImpedance.TravelTime;
			TravelCost = modeImpedance.TravelCost;
			TravelDistance = modeImpedance.TravelDistance;
			PathType = modeImpedance.PathType;

			var duration = (int) TravelTime;

			if (duration == Constants.DEFAULT_VALUE && Global.Configuration.ReportInvalidPersonDays) {
				Global.PrintFile.WriteDurationIsInvalidWarning("TripWrapper", "UpdateTripValues", PersonDay.Id, TravelTime, TravelCost, TravelDistance);

				if (!Global.Configuration.IsInEstimationMode) {
					PersonDay.IsValid = false;
				}

				return;
			}

			ArrivalTime = IsHalfTourFromOrigin ? Math.Max(1, DepartureTime - duration) : Math.Min(Global.Settings.Times.MinutesInADay, DepartureTime + duration);

			var timeWindow = Tour.IsHomeBasedTour ? Tour.PersonDay.TimeWindow : Tour.ParentTour.TimeWindow;

			if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime)) {
				// move entire trip up to 15 minutes later or earlier depending on half tour direction.  
				// Find the smallest shift that will make the arrival time a non-busy minute while still leaving 
				// a gap between the departure time and the arrival time at the trip origin (non-0 activity duration)
				const int moveLimit = 15;

				if (IsHalfTourFromOrigin) {
					var originArrivalTime = Sequence == 1 ? Tour.DestinationDepartureTime : GetPreviousTrip().ArrivalTime;
					var moveLater = 0;

					do {
						moveLater++;
					} while (moveLater <= moveLimit && DepartureTime + moveLater < originArrivalTime && timeWindow.IsBusy(ArrivalTime + moveLater));

					if (!timeWindow.IsBusy(ArrivalTime + moveLater)) {
						ArrivalTime += moveLater;
						DepartureTime += moveLater;

						if (Sequence == 1) {
							Tour.DestinationArrivalTime += moveLater;
						}

						if (Global.Configuration.ReportInvalidPersonDays) {
							Global.PrintFile.WriteLine("Tour {0}. Arrival time moved later by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveLater, DepartureTime, originArrivalTime);
						}
					}
				}
				else {
					var originArrivalTime = Sequence == 1 ? Tour.DestinationArrivalTime : GetPreviousTrip().ArrivalTime;
					var moveEarlier = 0;

					do {
						moveEarlier++;
					} while (moveEarlier <= moveLimit && DepartureTime - moveEarlier > originArrivalTime && timeWindow.IsBusy(ArrivalTime - moveEarlier));

					if (!timeWindow.IsBusy(ArrivalTime - moveEarlier)) {
						ArrivalTime -= moveEarlier;
						DepartureTime -= moveEarlier;

						if (Sequence == 1) {
							Tour.DestinationDepartureTime -= moveEarlier;
						}

						if (Global.Configuration.ReportInvalidPersonDays) {
							Global.PrintFile.WriteLine("Tour {0}. Arrival time moved earlier by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveEarlier, DepartureTime, originArrivalTime);
						}
					}
				}
			}

			if (Global.Configuration.AllowTripArrivalTimeOverlaps || !timeWindow.IsBusy(ArrivalTime)) {
				return;
			}

			//check again after possible adjustment
			if (Global.Configuration.ReportInvalidPersonDays) {
				Global.PrintFile.WriteLine("Arrival time is busy for {0}.", Tour.Id);
			}

			if (!Global.Configuration.IsInEstimationMode) {
				PersonDay.IsValid = false;
			}
		}

		public virtual void HUpdateTripValues() {
			//new version for household models - assumes that mode and departure time have been set

			//time windows also reset in estimation mode  - this just resets for one window
			var timeWindow = Tour.IsHomeBasedTour ? Tour.PersonDay.TimeWindow : Tour.ParentTour.TimeWindow;

			if (!Global.Configuration.IsInEstimationMode) {
				//some variables reset only in application mode
				var time = new HTripTime(DepartureTime);

				HTripTime.SetTimeImpedanceAndWindow(this, time);

				if (!time.Available) {
					if (!Global.Configuration.IsInEstimationMode) {
						PersonDay.IsValid = false;
					}

					return;
				}

				var modeImpedance = time.ModeLOS;

				TravelTime = modeImpedance.PathTime;
				TravelCost = modeImpedance.PathCost;
				TravelDistance = modeImpedance.PathDistance;
				PathType = modeImpedance.PathType;

				if (this.Mode == Global.Settings.Modes.Transit) {
					if (this.Direction == 1) {
						this.AccessCost = time.DestinationAccessCost;
						this.AccessDistance = time.DestinationAccessDistance;
						this.AccessMode = time.DestinationAccessMode;
						this.AccessStopArea = time.ParkAndRideDestinationStopAreaKey;
						this.AccessTime = time.DestinationAccessTime;
						this.EgressCost = time.OriginAccessCost;
						this.EgressDistance = time.OriginAccessDistance;
						this.EgressMode = time.OriginAccessMode;
						this.EgressStopArea = time.ParkAndRideOriginStopAreaKey;
						this.EgressTime = time.OriginAccessTime;

					}
					else {
						this.AccessCost = time.OriginAccessCost;
						this.AccessDistance = time.OriginAccessDistance;
						this.AccessMode = time.OriginAccessMode;
						this.AccessStopArea = time.ParkAndRideOriginStopAreaKey;
						this.AccessTime = time.OriginAccessTime;
						this.EgressCost = time.DestinationAccessCost;
						this.EgressDistance = time.DestinationAccessDistance;
						this.EgressMode = time.DestinationAccessMode;
						this.EgressStopArea = time.ParkAndRideDestinationStopAreaKey;
						this.EgressTime = time.DestinationAccessTime;
					}
				}
				var duration = (int) (TravelTime + 0.5);

				if (duration == Constants.DEFAULT_VALUE && Global.Configuration.ReportInvalidPersonDays) {
					Global.PrintFile.WriteDurationIsInvalidWarning("TripWrapper", "UpdateTripValues", PersonDay.Id, TravelTime, TravelCost, TravelDistance);

					if (!Global.Configuration.IsInEstimationMode) {
						PersonDay.IsValid = false;
					}

					return;
				}

				ArrivalTime =
					IsHalfTourFromOrigin
						? Math.Max(1, DepartureTime - duration)
						: Math.Min(Global.Settings.Times.MinutesInADay, DepartureTime + duration);

				/* doesn't have much effect - turn off for now
									 if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime))   {
										  // move entire trip up to 15 minutes later or earlier depending on half tour direction.  
										  // Find the smallest shift that will make the arrival time a non-busy minute while still leaving 
										  // a gap between the departure time and the arrival time at the trip origin (non-0 activity duration)
										  //NOTE: This was copied over from the old version above.
										  // This could possibly cause some inconsistencies for times for different people on joint tours, if it is done separately for each
										  // (will work better if done before cloning....)
										  const int moveLimit = 15;

										  if (IsHalfTourFromOrigin)     {
												int originArrivalTime = Sequence == 1 ? Tour.DestinationDepartureTime : PreviousTrip.ArrivalTime;
												int moveLater = 0;
												do       {
													 moveLater++;
												} while (moveLater <= moveLimit && DepartureTime + moveLater < originArrivalTime && timeWindow.IsBusy(ArrivalTime + moveLater));

												if (!timeWindow.IsBusy(ArrivalTime + moveLater)) {
													 ArrivalTime += moveLater;
													 DepartureTime += moveLater;
													 if (Sequence == 1) Tour.DestinationArrivalTime += moveLater;
													 if (Global.Configuration.ReportInvalidPersonDays) Global.PrintFile.WriteLine("Tour {0}. Arrival time moved later by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveLater, DepartureTime, originArrivalTime);
												}
										  }
										  else  {
												int originArrivalTime = Sequence == 1 ? Tour.DestinationArrivalTime : PreviousTrip.ArrivalTime;
												int moveEarlier = 0;
												do   {
													 moveEarlier++;
												} while (moveEarlier <= moveLimit && DepartureTime - moveEarlier > originArrivalTime && timeWindow.IsBusy(ArrivalTime - moveEarlier));

												if (!timeWindow.IsBusy(ArrivalTime - moveEarlier))   {
													 ArrivalTime -= moveEarlier;
													 DepartureTime -= moveEarlier;
													 if (Sequence == 1) Tour.DestinationDepartureTime -= moveEarlier;
													 if (Global.Configuration.ReportInvalidPersonDays) Global.PrintFile.WriteLine("Tour {0}. Arrival time moved earlier by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveEarlier, DepartureTime, originArrivalTime);
												}
										  }
									 }
				*/
				//check again after possible adjustment

				if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime)) {
					if (Global.Configuration.ReportInvalidPersonDays) {
						Global.PrintFile.WriteLine("Arrival time {0} is busy for trip {1}.", ArrivalTime, Id);
					}

					if (!Global.Configuration.IsInEstimationMode) {
						PersonDay.IsValid = false;
					}
				}
				else //check if another trip needs to be scheduled and there only a few minutes left
					if ((IsHalfTourFromOrigin && ArrivalTime < Tour.EarliestOriginDepartureTime + 3 && DestinationParcel != Tour.OriginParcel) || (!IsHalfTourFromOrigin && ArrivalTime > Tour.LatestOriginArrivalTime - 3 && DestinationParcel != Tour.OriginParcel)) {
						if (!Global.Configuration.IsInEstimationMode) {
							PersonDay.IsValid = false;
						}
					}

				if (Global.Configuration.TraceModelResultValidity) {
					if (PersonDay.HouseholdDay.AttemptedSimulations >= Global.Configuration.InvalidAttemptsBeforeTrace) {
						Global.PrintFile.WriteLine("  >> HUpdateTripValues HH/P/T/Hf/T/Arrival time/valid {0} {1} {2} {3} {4} {5} {6}", Household.Id, Person.Sequence, Tour.Sequence, Direction, Sequence, ArrivalTime, PersonDay.IsValid);
					}
				}

				if (!PersonDay.IsValid) {
					return;
				}

				//if first trip in half tour, use departure time to reset tour times
				if (Sequence == 1) {
					if (IsHalfTourFromOrigin) {
						Tour.DestinationArrivalTime = DepartureTime;
					}
					else {
						Tour.DestinationDepartureTime = DepartureTime;
					}
				}
			}

			//adjust the time window for busy minutes at the stop origin and during the trip - done also in estimation mode
			var earliestBusyMinute =
				IsHalfTourFromOrigin
					? ArrivalTime
					: Sequence == 1
						? Tour.DestinationDepartureTime
						: GetPreviousTrip().ArrivalTime;

			var latestBusyMinute =
				!IsHalfTourFromOrigin
					? ArrivalTime
					: Sequence == 1
						? Tour.DestinationArrivalTime
						: GetPreviousTrip().ArrivalTime;

			timeWindow.SetBusyMinutes(earliestBusyMinute, latestBusyMinute + 1);

			if (!Global.Configuration.TraceModelResultValidity || PersonDay.HouseholdDay.AttemptedSimulations < Global.Configuration.InvalidAttemptsBeforeTrace) {
				return;
			}

			if (Tour.IsHomeBasedTour) {
				Global.PrintFile.WriteLine("  >> HUpdateTripValues SetBusyMinutes HH/P/PDay/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, PersonDay.Id, earliestBusyMinute, latestBusyMinute + 1);
			}
			else {
				Global.PrintFile.WriteLine("  >> HUpdateTripValues SetBusyMinutes HH/P/TOUR/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, Tour.ParentTour.Sequence, earliestBusyMinute, latestBusyMinute + 1);
			}
		}

		public virtual void Invert(int sequence) {
			var tempParcelId = OriginParcelId;
			OriginParcelId = DestinationParcelId;
			DestinationParcelId = tempParcelId;

			var tempParcel = OriginParcel;
			OriginParcel = DestinationParcel;
			DestinationParcel = tempParcel;

			var tempZoneKey = OriginZoneKey;
			OriginZoneKey = DestinationZoneKey;
			DestinationZoneKey = tempZoneKey;

			var tempPurpose = OriginPurpose;
			OriginPurpose = DestinationPurpose;
			DestinationPurpose = tempPurpose;

			var tempAddressType = OriginAddressType;
			OriginAddressType = DestinationAddressType;
			DestinationAddressType = tempAddressType;

			var tempTime = ArrivalTime;
			ArrivalTime = DepartureTime;
			DepartureTime = tempTime;

			Sequence = sequence;
		}

		public virtual ITripModeImpedance[] GetTripModeImpedances() {
			var modeImpedances = new ITripModeImpedance[DayPeriod.SmallDayPeriods.Length];
			var availableMinutes = Tour.IsHomeBasedTour ? PersonDay.TimeWindow : Tour.ParentTour.TimeWindow;

			for (var i = 0; i < DayPeriod.SmallDayPeriods.Length; i++) {
				var period = DayPeriod.SmallDayPeriods[i];
				var modeImpedance = GetTripModeImpedance(period.Middle);

				modeImpedances[i] = modeImpedance;

				modeImpedance.AdjacentMinutesBefore = availableMinutes.AdjacentAvailableMinutesBefore(period.Start) / ChoiceModelFactory.SmallPeriodDuration;
				modeImpedance.MaxMinutesBefore = availableMinutes.MaxAvailableMinutesBefore(period.Start) / ChoiceModelFactory.SmallPeriodDuration;
				modeImpedance.TotalMinutesBefore = availableMinutes.TotalAvailableMinutesBefore(period.Start) / ChoiceModelFactory.SmallPeriodDuration;

				modeImpedance.AdjacentMinutesAfter = availableMinutes.AdjacentAvailableMinutesAfter(period.End) / ChoiceModelFactory.SmallPeriodDuration;
				modeImpedance.MaxMinutesAfter = availableMinutes.MaxAvailableMinutesAfter(period.End) / ChoiceModelFactory.SmallPeriodDuration;
				modeImpedance.TotalMinutesAfter = availableMinutes.TotalAvailableMinutesAfter(period.End) / ChoiceModelFactory.SmallPeriodDuration;
			}

			return modeImpedances;
		}

		public virtual void SetActivityEndTime(int activityEndTime) {
			ActivityEndTime = activityEndTime.ToMinutesAfterMidnight();
		}

		public virtual void SetOriginAddressType(int originAddressType) {
			OriginAddressType = originAddressType;
		}

		public virtual void SetTourSequence(int tourSequence) {
			TourSequence = tourSequence;
		}

		//JLB 20160323
		public virtual void SetTripValueOfTime() {
			var costDivisor =
				Mode == Global.Settings.Modes.HovDriver && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
					? Global.Configuration.Coefficients_HOV2CostDivisor_Work
					: Mode == Global.Settings.Modes.HovDriver && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
						? Global.Configuration.Coefficients_HOV2CostDivisor_Other
						: Mode == Global.Settings.Modes.HovPassenger && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
							? Global.Configuration.Coefficients_HOV3CostDivisor_Work
							: Mode == Global.Settings.Modes.HovPassenger && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
								? Global.Configuration.Coefficients_HOV3CostDivisor_Other
								: 1.0;

			ValueOfTime = (Tour.TimeCoefficient * 60) / (Tour.CostCoefficient / costDivisor);
		}

		//		public virtual void SetTripValueOfTime() {
		//			var costDivisor =
		//				Mode == Global.Settings.Modes.Hov2 && Tour.DestinationPurpose == Global.Settings.Purposes.Work
		//					? Global.Configuration.Coefficients_HOV2CostDivisor_Work
		//					: Mode == Global.Settings.Modes.Hov2 && Tour.DestinationPurpose != Global.Settings.Purposes.Work
		//						? Global.Configuration.Coefficients_HOV2CostDivisor_Other
		//						: Mode == Global.Settings.Modes.Hov3 && Tour.DestinationPurpose == Global.Settings.Purposes.Work
		//							? Global.Configuration.Coefficients_HOV3CostDivisor_Work
		//							: Mode == Global.Settings.Modes.Hov3 && Tour.DestinationPurpose != Global.Settings.Purposes.Work
		//								? Global.Configuration.Coefficients_HOV3CostDivisor_Other
		//								: 1.0;
		//
		//			ValueOfTime = (Tour.TimeCoefficient * 60) / (Tour.CostCoefficient / costDivisor);
		//		}


		public virtual bool IsBusinessDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Business;
		}

		public virtual bool IsBusinessOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.Business;
		}


		public virtual void HPTBikeTourUpdateTripValues() {
			//new version for trips on tours with mode 9, 10, 11, 13
			// for Actum 
			// assumes that mode and departure time have been set
			// assumes one of tour modes 9, 10, 11, 13

			//time windows also reset in estimation mode  - this just resets for one window
			var timeWindow = Tour.IsHomeBasedTour ? Tour.PersonDay.TimeWindow : Tour.ParentTour.TimeWindow;

			if (!Global.Configuration.IsInEstimationMode) {
				//some variables reset only in application mode
				var time = new HTripTime(DepartureTime);
				var period = time.DeparturePeriod;

				// set availability
				if (period.End < this.EarliestDepartureTime || period.Start > this.LatestDepartureTime) {
					time.Available = false;
				}
				else {
					time.Available = true;
				}

				var travelTime = this.Direction == 1 ? this.Tour.HalfTour1TravelTime : this.Tour.HalfTour2TravelTime;

				//set the feasible window within the small period, accounting for travel time, and recheck availability
				if (time.Available) {

					time.EarliestFeasibleDepatureTime = Math.Max(period.Start,
							this.IsHalfTourFromOrigin
						//JLB 20130723 replace next line
						//? trip.ArrivalTimeLimit + - (int) (time.ModeLOS.PathTime + 0.5)
							? this.ArrivalTimeLimit + (int) (travelTime + 0.5)
							: this.EarliestDepartureTime);

					time.LatestFeasibleDepartureTime = Math.Min(period.End,
							this.IsHalfTourFromOrigin
							? this.LatestDepartureTime
							: this.ArrivalTimeLimit - (int) (travelTime + 0.5));

					time.Available = time.EarliestFeasibleDepatureTime < time.LatestFeasibleDepartureTime;
				}

				//HTripTime.SetTimeImpedanceAndWindow(this, time);

				if (!time.Available) {
					if (!Global.Configuration.IsInEstimationMode) {
						PersonDay.IsValid = false;
					}
					return;
				}

				TravelTime = travelTime;
				TravelCost = this.Tour.TravelCostForPTBikeTour / 2.0;
				TravelDistance = this.Tour.TravelDistanceForPTBikeTour / 2.0;
				PathType = this.Tour.PathType;

				this.Mode = this.Tour.Mode;

				if (this.Direction == 1) {
					this.AccessCost = this.Tour.HalfTour1AccessCost;
					this.AccessDistance = this.Tour.HalfTour1AccessDistance;
					this.AccessMode = this.Tour.HalfTour1AccessMode;
					this.AccessPathType = this.Tour.HalfTour1AccessPathType;
					this.AccessStopArea = this.Tour.HalfTour1AccessStopAreaKey;
					this.AccessTime = this.Tour.HalfTour1AccessTime;
					this.EgressCost = this.Tour.HalfTour1EgressCost;
					this.EgressDistance = this.Tour.HalfTour1EgressDistance;
					this.EgressMode = this.Tour.HalfTour1EgressMode;
					this.EgressPathType = this.Tour.HalfTour1EgressPathType;
					this.EgressStopArea = this.Tour.HalfTour1EgressStopAreaKey;
					this.EgressTime = this.Tour.HalfTour1EgressTime;

				}
				else {
					this.AccessCost = this.Tour.HalfTour2AccessCost;
					this.AccessDistance = this.Tour.HalfTour2AccessDistance;
					this.AccessMode = this.Tour.HalfTour2AccessMode;
					this.AccessPathType = this.Tour.HalfTour2AccessPathType;
					this.AccessStopArea = this.Tour.HalfTour2AccessStopAreaKey;
					this.AccessTime = this.Tour.HalfTour2AccessTime;
					this.EgressCost = this.Tour.HalfTour2EgressCost;
					this.EgressDistance = this.Tour.HalfTour2EgressDistance;
					this.EgressMode = this.Tour.HalfTour2EgressMode;
					this.EgressPathType = this.Tour.HalfTour2EgressPathType;
					this.EgressStopArea = this.Tour.HalfTour2EgressStopAreaKey;
					this.EgressTime = this.Tour.HalfTour2EgressTime;
				}

				var duration = (int) (TravelTime + 0.5);

				if (duration == Constants.DEFAULT_VALUE && Global.Configuration.ReportInvalidPersonDays) {
					Global.PrintFile.WriteDurationIsInvalidWarning("TripWrapper", "UpdateTripValues", PersonDay.Id, TravelTime, TravelCost, TravelDistance);

					if (!Global.Configuration.IsInEstimationMode) {
						PersonDay.IsValid = false;
					}

					return;
				}

				ArrivalTime =
					IsHalfTourFromOrigin
						? Math.Max(1, DepartureTime - duration)
						: Math.Min(Global.Settings.Times.MinutesInADay, DepartureTime + duration);

				/* doesn't have much effect - turn off for now
									 if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime))   {
										  // move entire trip up to 15 minutes later or earlier depending on half tour direction.  
										  // Find the smallest shift that will make the arrival time a non-busy minute while still leaving 
										  // a gap between the departure time and the arrival time at the trip origin (non-0 activity duration)
										  //NOTE: This was copied over from the old version above.
										  // This could possibly cause some inconsistencies for times for different people on joint tours, if it is done separately for each
										  // (will work better if done before cloning....)
										  const int moveLimit = 15;

										  if (IsHalfTourFromOrigin)     {
												int originArrivalTime = Sequence == 1 ? Tour.DestinationDepartureTime : PreviousTrip.ArrivalTime;
												int moveLater = 0;
												do       {
													 moveLater++;
												} while (moveLater <= moveLimit && DepartureTime + moveLater < originArrivalTime && timeWindow.IsBusy(ArrivalTime + moveLater));

												if (!timeWindow.IsBusy(ArrivalTime + moveLater)) {
													 ArrivalTime += moveLater;
													 DepartureTime += moveLater;
													 if (Sequence == 1) Tour.DestinationArrivalTime += moveLater;
													 if (Global.Configuration.ReportInvalidPersonDays) Global.PrintFile.WriteLine("Tour {0}. Arrival time moved later by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveLater, DepartureTime, originArrivalTime);
												}
										  }
										  else  {
												int originArrivalTime = Sequence == 1 ? Tour.DestinationArrivalTime : PreviousTrip.ArrivalTime;
												int moveEarlier = 0;
												do   {
													 moveEarlier++;
												} while (moveEarlier <= moveLimit && DepartureTime - moveEarlier > originArrivalTime && timeWindow.IsBusy(ArrivalTime - moveEarlier));

												if (!timeWindow.IsBusy(ArrivalTime - moveEarlier))   {
													 ArrivalTime -= moveEarlier;
													 DepartureTime -= moveEarlier;
													 if (Sequence == 1) Tour.DestinationDepartureTime -= moveEarlier;
													 if (Global.Configuration.ReportInvalidPersonDays) Global.PrintFile.WriteLine("Tour {0}. Arrival time moved earlier by {1} minutes, New departure time {2}, Origin arrival {3}", Tour.Id, moveEarlier, DepartureTime, originArrivalTime);
												}
										  }
									 }
				*/
				//check again after possible adjustment

				if (!Global.Configuration.AllowTripArrivalTimeOverlaps && timeWindow.IsBusy(ArrivalTime)) {
					if (Global.Configuration.ReportInvalidPersonDays) {
						Global.PrintFile.WriteLine("Arrival time {0} is busy for trip {1}.", ArrivalTime, Id);
					}

					if (!Global.Configuration.IsInEstimationMode) {
						PersonDay.IsValid = false;
					}
				}
				else //check if another trip needs to be scheduled and there only a few minutes left
					if ((IsHalfTourFromOrigin && ArrivalTime < Tour.EarliestOriginDepartureTime + 3 && DestinationParcel != Tour.OriginParcel) || (!IsHalfTourFromOrigin && ArrivalTime > Tour.LatestOriginArrivalTime - 3 && DestinationParcel != Tour.OriginParcel)) {
						if (!Global.Configuration.IsInEstimationMode) {
							PersonDay.IsValid = false;
						}
					}

				if (Global.Configuration.TraceModelResultValidity) {
					if (PersonDay.HouseholdDay.AttemptedSimulations >= Global.Configuration.InvalidAttemptsBeforeTrace) {
						Global.PrintFile.WriteLine("  >> HPTUpdateTripValues HH/P/T/Hf/T/Arrival time/valid {0} {1} {2} {3} {4} {5} {6}", Household.Id, Person.Sequence, Tour.Sequence, Direction, Sequence, ArrivalTime, PersonDay.IsValid);
					}
				}

				if (!PersonDay.IsValid) {
					return;
				}

				//if first trip in half tour, use departure time to reset tour times
				if (Sequence == 1) {
					if (IsHalfTourFromOrigin) {
						Tour.DestinationArrivalTime = DepartureTime;
					}
					else {
						Tour.DestinationDepartureTime = DepartureTime;
					}
				}
			}

			//adjust the time window for busy minutes at the stop origin and during the trip - done also in estimation mode
			var earliestBusyMinute =
				IsHalfTourFromOrigin
					? ArrivalTime
					: Sequence == 1
						? Tour.DestinationDepartureTime
						: GetPreviousTrip().ArrivalTime;

			var latestBusyMinute =
				!IsHalfTourFromOrigin
					? ArrivalTime
					: Sequence == 1
						? Tour.DestinationArrivalTime
						: GetPreviousTrip().ArrivalTime;

			timeWindow.SetBusyMinutes(earliestBusyMinute, latestBusyMinute + 1);

			if (!Global.Configuration.TraceModelResultValidity || PersonDay.HouseholdDay.AttemptedSimulations < Global.Configuration.InvalidAttemptsBeforeTrace) {
				return;
			}

			if (Tour.IsHomeBasedTour) {
				Global.PrintFile.WriteLine("  >> HPTUpdateTripValues SetBusyMinutes HH/P/PDay/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, PersonDay.Id, earliestBusyMinute, latestBusyMinute + 1);
			}
			else {
				Global.PrintFile.WriteLine("  >> HPTUpdateTripValues SetBusyMinutes HH/P/TOUR/Min1/Min2 {0} {1} {2} {3} {4}", Household.Id, Person.Sequence, Tour.ParentTour.Sequence, earliestBusyMinute, latestBusyMinute + 1);
			}
		}




		#endregion

		#region init/utility/export methods

		public void Export() {
			_exporter.Export(_trip);
		}

		public static void Close() {
			Global
				.Kernel
				.Get<IPersistenceFactory<ITrip>>()
				.Close();
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder
				.AppendLine(string.Format("Trip ID: {0}, Tour ID: {1}",
					_trip.Id,
					_trip.TourId));

			builder
				.AppendLine(string.Format("Household ID: {0}, Person Sequence: {1}, Day: {2}, Tour Sequence: {3}, Half-tour: {4}, Sequence {5}",
					_trip.HouseholdId,
					_trip.PersonSequence,
					_trip.Day,
					_trip.TourSequence,
					_trip.Direction,
					_trip.Sequence));

			builder
				.AppendLine(string.Format("Destination Parcel ID: {0}, Destination Zone Key: {1}, Destination Purpose: {2}, Mode: {3}, Departure Time: {4}",
					_trip.DestinationParcelId,
					_trip.DestinationZoneKey,
					_trip.DestinationPurpose,
					_trip.Mode,
					_trip.DepartureTime));

			return builder.ToString();
		}

		private void SetParcelRelationships(ITrip trip) {
			IParcelWrapper originParcel;

			if (trip.OriginParcelId != Constants.DEFAULT_VALUE && ChoiceModelFactory.Parcels.TryGetValue(trip.OriginParcelId, out originParcel)) {
				OriginParcel = originParcel;
			}

			IParcelWrapper destinationParcel;

			if (trip.DestinationParcelId != Constants.DEFAULT_VALUE && ChoiceModelFactory.Parcels.TryGetValue(trip.DestinationParcelId, out destinationParcel)) {
				DestinationParcel = destinationParcel;
			}
		}

		private TripModeImpedance GetTripModeImpedance(int minute, bool includeCostAndDistance = false) {
			var modeImpedance = new TripModeImpedance();
			var costCoefficient = Tour.CostCoefficient;
			var timeCoefficient = Tour.TimeCoefficient;

			dynamic pathType;




			if (Mode == Global.Settings.Modes.Transit && DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
				if (Global.StopAreaIsEnabled) {
					modeImpedance.TravelTime = Tour.ParkAndRideTransitTime / 2.0;
					modeImpedance.GeneralizedTime = Tour.ParkAndRideTransitGeneralizedTime / 2.0;
					modeImpedance.PathType = Tour.ParkAndRidePathType;

					if (!includeCostAndDistance) {
						return modeImpedance;
					}
					modeImpedance.TravelDistance = Tour.ParkAndRideTransitDistance / 2.0;
					modeImpedance.TravelCost = Tour.ParkAndRideTransitCost / 2.0;
				}
				else {
					var parkAndRideZoneId =
						 ChoiceModelFactory
						 .ParkAndRideNodeDao
						 .Get(Tour.ParkAndRideNodeId)
						 .ZoneId;

					var origin = IsHalfTourFromOrigin ? parkAndRideZoneId : OriginParcel.ZoneId;
					var destination = IsHalfTourFromOrigin ? OriginParcel.ZoneId : parkAndRideZoneId;

					IEnumerable<dynamic> pathTypeModels =
						 PathTypeModelFactory.Model
						//20151117 JLB following line was before I made change for Actum under SVN repository 
						//.Run(Household.RandomUtility, OriginParcel.ZoneId, parkAndRideZoneId, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.GetTransitFareDiscountFraction(), false, Mode);
						//20171117 JLB following line was after I made change for Actum under SVN repository
						//.Run(Household.RandomUtility, OriginParcel.ZoneId, parkAndRideZoneId, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.TransitPassOwnership, Person.GetTransitFareDiscountFraction(), false, Mode);
						//20171117 JLB following line came from RSG Git repository when I was synching new Git Actum repository with RSG Git repository
						//.Run(Household.RandomUtility, origin, destination, Tour.DestinationArrivalTime, Tour.DestinationDepartureTime, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.GetTransitFareDiscountFraction(), false, Global.Settings.Modes.ParkAndRide);
						//20151117 JLB following line inserts my change for Actum into the line that came from the RSG Git repository
						 .Run(Household.RandomUtility, origin, destination, Tour.DestinationArrivalTime, Tour.DestinationDepartureTime, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.TransitPassOwnership, Person.GetTransitFareDiscountFraction(), false, Global.Settings.Modes.ParkAndRide);

					pathType = pathTypeModels.First();

					modeImpedance.TravelTime = pathType.PathParkAndRideTransitTime;
					modeImpedance.GeneralizedTime = pathType.GeneralizedTimeLogsum;
					modeImpedance.PathType = pathType.PathType;

					if (!includeCostAndDistance) {
						return modeImpedance;
					}

					modeImpedance.TravelCost = pathType.PathParkAndRideTransitCost;
					modeImpedance.TravelDistance = pathType.PathParkAndRideTransitDistance;
				}
			}
			else {
				var useMode = Mode == Global.Settings.Modes.SchoolBus ? Global.Settings.Modes.Hov3 : Mode;

				var origin = IsHalfTourFromOrigin ? DestinationParcel : OriginParcel;
				var destination = IsHalfTourFromOrigin ? OriginParcel : DestinationParcel;

				IEnumerable<dynamic> pathTypeModels =
					 PathTypeModelFactory.Model
					//20151117 JLB following line was before I made change for Actum under SVN repository 
					//.Run(Household.RandomUtility, OriginParcel, DestinationParcel, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.GetTransitFareDiscountFraction(), false, useMode);
					//20171117 JLB following line was after I made change for Actum under SVN repository
					//.Run(Household.RandomUtility, OriginParcel, DestinationParcel, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.TransitPassOwnership, Person.GetTransitFareDiscountFraction(), false, useMode);
					//20171117 JLB following line came from RSG Git repository when I was synching new Git Actum repository with RSG Git repository
					//.Run(Household.RandomUtility, origin, destination, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.GetTransitFareDiscountFraction(), false, useMode);
					//20151117 JLB following line inserts my change for Actum into the line that came from the RSG Git repository
						  .Run(Household.RandomUtility, origin, destination, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.TransitPassOwnership, Person.GetTransitFareDiscountFraction(), false, useMode);
				pathType = pathTypeModels.First();

				modeImpedance.TravelTime = pathType.PathTime;
				modeImpedance.GeneralizedTime = pathType.GeneralizedTimeLogsum;
				modeImpedance.PathType = pathType.PathType;

				if (!includeCostAndDistance) {
					return modeImpedance;
				}

				modeImpedance.TravelCost = pathType.PathCost;
				modeImpedance.TravelDistance = pathType.PathDistance;
			}





			return modeImpedance;
		}

		#endregion
	}
}