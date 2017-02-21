// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Persisters;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Sampling;
using DaySim.PathTypeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaySim.DomainModels.Default.Wrappers {
    [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
    public class TripWrapper : ISamplingTrip, ITripWrapper {
        private readonly ITrip _trip;

        private readonly IPersisterExporter _exporter;

        [UsedImplicitly]
        public TripWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour) {
            _trip = trip;

            _exporter =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<ITrip>>()
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
            if (Mode == Global.Settings.Modes.Transit) {
                if (Tour.Mode == Global.Settings.Modes.ParkAndRide) {
                    DriverType = (int)(Tour.ParkAndRideWalkAccessEgressTime / 2.0);
                }
            } else if (Mode == Global.Settings.Modes.Walk || Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.SchoolBus || Mode == Global.Settings.Modes.Other) {
                DriverType = Global.Settings.DriverTypes.NotApplicable;
            }
            else if (Mode == Global.Settings.Modes.PaidRideShare) {
                DriverType = Global.Configuration.AV_PaidRideShareModeUsesAVs? Global.Settings.DriverTypes.AV_Passenger : Global.Settings.DriverTypes.Passenger;
            }
            else if (Global.Configuration.AV_IncludeAutoTypeChoice && Tour.Household.OwnsAutomatedVehicles > 0) {
                DriverType = Global.Settings.DriverTypes.AV_Passenger;
            }
            else if (Mode == Global.Settings.Modes.Sov) {
                DriverType = Global.Settings.DriverTypes.Driver;
            } else if (Mode == Global.Settings.Modes.Hov2 || Mode == Global.Settings.Modes.Hov3) {
                if (Person.IsChildUnder16 || Household.VehiclesAvailable == 0 || (!Tour.IsHov2Mode() && !Tour.IsHov3Mode())) {
                    DriverType = Global.Settings.DriverTypes.Passenger;
                } else {
                    if (trips.Any(t => ((TripWrapper)t).IsWalkMode() || ((TripWrapper)t).IsBikeMode())) {
                        DriverType = Global.Settings.DriverTypes.Passenger;
                    } else if (trips.Any(t => ((TripWrapper)t).IsSovMode())) {
                        DriverType = Global.Settings.DriverTypes.Driver;
                    } else if (trips.Any(t => ((TripWrapper)t).IsHov2Mode())) {
                        var randomNumber = Household.RandomUtility.Uniform01();

                        DriverType = randomNumber > .799 ? Global.Settings.DriverTypes.Passenger : Global.Settings.DriverTypes.Driver;
                    } else {
                        // HOV3 mode
                        var randomNumber = Household.RandomUtility.Uniform01();

                        DriverType = randomNumber > .492 ? Global.Settings.DriverTypes.Passenger : Global.Settings.DriverTypes.Driver;
                    }
                }
            }
        }

        public virtual void UpdateTripValues() {
            if ((Global.Configuration.IsInEstimationMode && !Global.Configuration.ShouldOutputStandardFilesInEstimationMode)
                || ((Global.Configuration.ShouldRunTourTripModels || Global.Configuration.ShouldRunSubtourTripModels) && !Global.Configuration.ShouldRunIntermediateStopLocationModel) || ((Global.Configuration.ShouldRunTourTripModels || Global.Configuration.ShouldRunSubtourTripModels) && !Global.Configuration.ShouldRunTripModeModel) || ((Global.Configuration.ShouldRunTourTripModels || Global.Configuration.ShouldRunSubtourTripModels) && !Global.Configuration.ShouldRunTripTimeModel)) {
                return;
            }

            var modeImpedance = GetTripModeImpedance(DepartureTime, true);

            TravelTime = modeImpedance.TravelTime;
            TravelCost = modeImpedance.TravelCost;
            TravelDistance = modeImpedance.TravelDistance;

            if (Global.Configuration.IsInEstimationMode) {
                return;
            }
            PathType = modeImpedance.PathType;
            // new - for transit use DriverType for walk time
            if (Mode == Global.Settings.Modes.Transit) {
                DriverType = (int)modeImpedance.WalkAccessEgressTime;
            }

            var duration = (int)TravelTime;

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
                } else {
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

                var duration = (int)(TravelTime + 0.5);

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
                } else //check if another trip needs to be scheduled and there only a few minutes left
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
                    } else {
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
            } else {
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
            ActivityEndTime = activityEndTime;  //.ToMinutesAfterMidnight(); redundant- done on output
        }

        public virtual void SetOriginAddressType(int originAddressType) {
            OriginAddressType = originAddressType;
        }

        public virtual void SetTourSequence(int tourSequence) {
            TourSequence = tourSequence;
        }

        public virtual void SetTripValueOfTime() {
            var costDivisor =
                Mode == Global.Settings.Modes.Hov2 && Tour.DestinationPurpose == Global.Settings.Purposes.Work
                    ? Global.Configuration.Coefficients_HOV2CostDivisor_Work
                    : Mode == Global.Settings.Modes.Hov2 && Tour.DestinationPurpose != Global.Settings.Purposes.Work
                        ? Global.Configuration.Coefficients_HOV2CostDivisor_Other
                        : Mode == Global.Settings.Modes.Hov3 && Tour.DestinationPurpose == Global.Settings.Purposes.Work
                            ? Global.Configuration.Coefficients_HOV3CostDivisor_Work
                            : Mode == Global.Settings.Modes.Hov3 && Tour.DestinationPurpose != Global.Settings.Purposes.Work
                                ? Global.Configuration.Coefficients_HOV3CostDivisor_Other
                                : 1.0;

            ValueOfTime = (Tour.TimeCoefficient * 60) / (Tour.CostCoefficient / costDivisor);
        }

        #endregion

        #region init/utility/export methods

        public void Export() {
            _exporter.Export(_trip);
        }

        public static void Close() {
            Global
                .ContainerDaySim
                .GetInstance<IPersistenceFactory<ITrip>>()
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

            IPathTypeModel pathType;

            if (Mode == Global.Settings.Modes.Transit && DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
                //if (Global.StopAreaIsEnabled)  {
                modeImpedance.TravelTime = Tour.ParkAndRideTransitTime / 2.0;
                modeImpedance.GeneralizedTime = Tour.ParkAndRideTransitGeneralizedTime / 2.0;
                modeImpedance.PathType = Tour.ParkAndRidePathType;
                modeImpedance.WalkAccessEgressTime = Tour.ParkAndRideWalkAccessEgressTime / 2.0;

                if (!includeCostAndDistance) {
                    return modeImpedance;
                }
                modeImpedance.TravelDistance = Tour.ParkAndRideTransitDistance / 2.0;
                modeImpedance.TravelCost = Tour.ParkAndRideTransitCost / 2.0;
                //}
                //else {
                //    var parkAndRideZoneId =
                //        ChoiceModelFactory
                //        .ParkAndRideNodeDao
                //        .Get(Tour.ParkAndRideNodeId)
                //       .ZoneId;

                //    var origin = IsHalfTourFromOrigin ? parkAndRideZoneId : OriginParcel.ZoneId;
                //    var destination = IsHalfTourFromOrigin ? OriginParcel.ZoneId : parkAndRideZoneId;

                //    IEnumerable<IPathTypeModel> pathTypeModels =
                //        PathTypeModelFactory.Model
                //        .Run(Household.RandomUtility, origin, destination, minute, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Person.GetTransitFareDiscountFraction(), false, Global.Settings.Modes.Transit);
                //    pathType = pathTypeModels.First();

                //    modeImpedance.TravelTime = pathType.PathTime;
                //    modeImpedance.GeneralizedTime = pathType.GeneralizedTimeLogsum;
                //    modeImpedance.PathType = pathType.PathType;

                //    if (!includeCostAndDistance) {
                //        return modeImpedance;
                //    }

                //    modeImpedance.TravelCost = pathType.PathCost;
                //    modeImpedance.TravelDistance = pathType.PathDistance;
                //}
            } else {
                var useMode = Mode >= Global.Settings.Modes.SchoolBus ? Global.Settings.Modes.Hov3 : Mode;

                var origin = IsHalfTourFromOrigin ? DestinationParcel : OriginParcel;
                var destination = IsHalfTourFromOrigin ? OriginParcel : DestinationParcel;

                IEnumerable<IPathTypeModel> pathTypeModels =
                    PathTypeModelFactory.Singleton
                        .Run(Household.RandomUtility, origin, destination, minute, 0, DestinationPurpose, costCoefficient, timeCoefficient, true, 1, Household.OwnsAutomatedVehicles>0, Person.GetTransitFareDiscountFraction(), false, useMode);
                pathType = pathTypeModels.First();

                modeImpedance.TravelTime = pathType.PathTime;
                modeImpedance.GeneralizedTime = pathType.GeneralizedTimeLogsum;
                modeImpedance.PathType = pathType.PathType;
                modeImpedance.WalkAccessEgressTime = pathType.PathTransitWalkAccessEgressTime;

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