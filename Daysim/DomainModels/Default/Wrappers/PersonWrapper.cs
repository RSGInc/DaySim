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
using Daysim.PathTypeModels;
using Ninject;

namespace Daysim.DomainModels.Default.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
	public class PersonWrapper : IPersonWrapper {
		private readonly IPerson _person;

		private readonly IPersisterExporter _exporter;

		[UsedImplicitly]
		public PersonWrapper(IPerson person, IHouseholdWrapper householdWrapper) {
			_person = person;

			_exporter =
				Global
					.Kernel
					.Get<IPersistenceFactory<IPerson>>()
					.Exporter;

			// relations properties

			Household = householdWrapper;

			SetParcelRelationships();

			// domain model properies

			SetExpansionFactor();

			// flags/choice model/etc. properties

			IsFullOrPartTimeWorker = PersonType <= Global.Settings.PersonTypes.PartTimeWorker;
			IsFulltimeWorker = PersonType == Global.Settings.PersonTypes.FullTimeWorker;
			IsPartTimeWorker = PersonType == Global.Settings.PersonTypes.PartTimeWorker;
			IsNotFullOrPartTimeWorker = PersonType > Global.Settings.PersonTypes.PartTimeWorker;
			IsStudentAge = PersonType >= Global.Settings.PersonTypes.UniversityStudent;
			IsRetiredAdult = PersonType == Global.Settings.PersonTypes.RetiredAdult;
			IsNonworkingAdult = PersonType == Global.Settings.PersonTypes.NonWorkingAdult;
			IsUniversityStudent = PersonType == Global.Settings.PersonTypes.UniversityStudent;
			IsDrivingAgeStudent = PersonType == Global.Settings.PersonTypes.DrivingAgeStudent;
			IsChildAge5Through15 = PersonType == Global.Settings.PersonTypes.ChildAge5Through15;
			IsChildUnder5 = PersonType == Global.Settings.PersonTypes.ChildUnder5;
			IsChildUnder16 = PersonType >= Global.Settings.PersonTypes.ChildAge5Through15;
			IsAdult = PersonType < Global.Settings.PersonTypes.DrivingAgeStudent;
			IsWorker = WorkerType > 0;
			IsStudent = StudentType > 0;
			IsFemale = Gender == Global.Settings.PersonGenders.Female;
			IsMale = Gender == Global.Settings.PersonGenders.Male;
			IsAdultFemale = IsFemale && IsAdult;
			IsAdultMale = IsMale && IsAdult;
			IsDrivingAge = PersonType <= Global.Settings.PersonTypes.DrivingAgeStudent;
			AgeIsBetween18And25 = person.Age.IsBetween(18, 25);
			AgeIsBetween26And35 = person.Age.IsBetween(26, 35);
			AgeIsBetween51And65 = person.Age.IsBetween(51, 65);
			AgeIsBetween51And98 = person.Age.IsBetween(51, 98);
			AgeIsLessThan35 = person.Age < 35;
			AgeIsLessThan30 = person.Age < 30;
			WorksAtHome = UsualWorkParcelId == Household.ResidenceParcelId;
			IsYouth = IsChildAge5Through15 || IsDrivingAgeStudent;
		}

		#region relations properties

		public IHouseholdWrapper Household { get; set; }

		public IParcelWrapper UsualWorkParcel { get; set; }

		public IParcelWrapper UsualSchoolParcel { get; set; }

		#endregion

		#region domain model properies

		public int Id {
			get { return _person.Id; }
			set { _person.Id = value; }
		}

		public int HouseholdId {
			get { return _person.HouseholdId; }
			set { _person.HouseholdId = value; }
		}

		public int Sequence {
			get { return _person.Sequence; }
			set { _person.Sequence = value; }
		}

		public int PersonType {
			get { return _person.PersonType; }
			set { _person.PersonType = value; }
		}

		public int Age {
			get { return _person.Age; }
			set { _person.Age = value; }
		}

		public int Gender {
			get { return _person.Gender; }
			set { _person.Gender = value; }
		}

		public int WorkerType {
			get { return _person.WorkerType; }
			set { _person.WorkerType = value; }
		}

		public int UsualWorkParcelId {
			get { return _person.UsualWorkParcelId; }
			set { _person.UsualWorkParcelId = value; }
		}

		public int UsualWorkZoneKey {
			get { return _person.UsualWorkZoneKey; }
			set { _person.UsualWorkZoneKey = value; }
		}

		public double AutoTimeToUsualWork {
			get { return _person.AutoTimeToUsualWork; }
			set { _person.AutoTimeToUsualWork = value; }
		}

		public double AutoDistanceToUsualWork {
			get { return _person.AutoDistanceToUsualWork; }
			set { _person.AutoDistanceToUsualWork = value; }
		}

		public int StudentType {
			get { return _person.StudentType; }
			set { _person.StudentType = value; }
		}

		public int UsualSchoolParcelId {
			get { return _person.UsualSchoolParcelId; }
			set { _person.UsualSchoolParcelId = value; }
		}

		public int UsualSchoolZoneKey {
			get { return _person.UsualSchoolZoneKey; }
			set { _person.UsualSchoolZoneKey = value; }
		}

		public double AutoTimeToUsualSchool {
			get { return _person.AutoTimeToUsualSchool; }
			set { _person.AutoTimeToUsualSchool = value; }
		}

		public double AutoDistanceToUsualSchool {
			get { return _person.AutoDistanceToUsualSchool; }
			set { _person.AutoDistanceToUsualSchool = value; }
		}

		public int UsualModeToWork {
			get { return _person.UsualModeToWork; }
			set { _person.UsualModeToWork = value; }
		}

		public int UsualArrivalPeriodToWork {
			get { return _person.UsualArrivalPeriodToWork; }
			set { _person.UsualArrivalPeriodToWork = value; }
		}

		public int UsualDeparturePeriodFromWork {
			get { return _person.UsualDeparturePeriodFromWork; }
			set { _person.UsualDeparturePeriodFromWork = value; }
		}

		public int TransitPassOwnership {
			get { return _person.TransitPassOwnership; }
			set { _person.TransitPassOwnership = value; }
		}

		public int PaidParkingAtWorkplace {
			get { return _person.PaidParkingAtWorkplace; }
			set { _person.PaidParkingAtWorkplace = value; }
		}

		public int PaperDiary {
			get { return _person.PaperDiary; }
			set { _person.PaperDiary = value; }
		}

		public int ProxyResponse {
			get { return _person.ProxyResponse; }
			set { _person.ProxyResponse = value; }
		}

		public double ExpansionFactor {
			get { return _person.ExpansionFactor; }
			set { _person.ExpansionFactor = value; }
		}

		#endregion

		#region flags/choice model/etc. properties

		public bool IsFullOrPartTimeWorker { get; set; }

		public bool IsFulltimeWorker { get; set; }

		public bool IsPartTimeWorker { get; set; }

		public bool IsNotFullOrPartTimeWorker { get; set; }

		public bool IsStudentAge { get; set; }

		public bool IsRetiredAdult { get; set; }

		public bool IsNonworkingAdult { get; set; }

		public bool IsUniversityStudent { get; set; }

		public bool IsDrivingAgeStudent { get; set; }

		public bool IsChildAge5Through15 { get; set; }

		public bool IsChildUnder5 { get; set; }

		public bool IsChildUnder16 { get; set; }

		public bool IsAdult { get; set; }

		public bool IsWorker { get; set; }

		public bool IsStudent { get; set; }

		public bool IsFemale { get; set; }

		public bool IsMale { get; set; }

		public bool IsAdultFemale { get; set; }

		public bool IsAdultMale { get; set; }

		public bool IsDrivingAge { get; set; }

		public bool AgeIsBetween18And25 { get; set; }

		public bool AgeIsBetween26And35 { get; set; }

		public bool AgeIsBetween51And65 { get; set; }

		public bool AgeIsBetween51And98 { get; set; }

		public bool AgeIsLessThan35 { get; set; }

		public bool AgeIsLessThan30 { get; set; }

		public bool WorksAtHome { get; set; }

		public bool IsYouth { get; set; }

		#endregion

		#region random/seed synchronization properties

		public int[] SeedValues { get; set; }

		#endregion

		#region wrapper methods

		public virtual bool IsOnlyFullOrPartTimeWorker() {
			return (IsFulltimeWorker || IsPartTimeWorker) && Household.HouseholdTotals.FullAndPartTimeWorkers == 1;
		}

		public virtual bool IsOnlyAdult() {
			return IsAdult && Household.HouseholdTotals.Adults == 1;
		}

		public virtual int GetCarOwnershipSegment() {
			return
				Age < 16
					? Global.Settings.CarOwnerships.Child
					: Household.VehiclesAvailable == 0
						? Global.Settings.CarOwnerships.NoCars
						: Household.VehiclesAvailable < Household.HouseholdTotals.DrivingAgeMembers
							? Global.Settings.CarOwnerships.LtOneCarPerAdult
							: Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;
		}

		public virtual double GetTransitFareDiscountFraction() {
			return
				!Global.Configuration.PathImpedance_TransitUseFareDiscountFractions
					? 0.0
					: Global.Configuration.IncludeTransitPassOwnershipModel && TransitPassOwnership > 0
						? 1.0
						: Math.Abs(Global.Configuration.Policy_UniversalTransitFareDiscountFraction) > Constants.EPSILON
							? Global.Configuration.Policy_UniversalTransitFareDiscountFraction
							: IsChildUnder5
								? Global.Configuration.PathImpedance_TransitFareDiscountFractionChildUnder5
								: IsChildAge5Through15
									? Global.Configuration.PathImpedance_TransitFareDiscountFractionChild5To15
									: IsDrivingAgeStudent
										? Global.Configuration.PathImpedance_TransitFareDiscountFractionHighSchoolStudent
										: IsUniversityStudent
											? Global.Configuration.PathImpedance_TransitFareDiscountFractionUniverityStudent
											: Age >= 65
												? Global.Configuration.PathImpedance_TransitFareDiscountFractionAge65Up
												: 0.0;
		}

		public virtual int GetHouseholdDayPatternParticipationPriority() {
			if (PersonType == Global.Settings.PersonTypes.FullTimeWorker) {
				return 5;
			}

			if (PersonType == Global.Settings.PersonTypes.PartTimeWorker) {
				return 4;
			}

			if (PersonType == Global.Settings.PersonTypes.RetiredAdult) {
				return 6;
			}

			if (PersonType == Global.Settings.PersonTypes.NonWorkingAdult) {
				return 3;
			}

			if (PersonType == Global.Settings.PersonTypes.UniversityStudent) {
				return 8;
			}

			if (PersonType == Global.Settings.PersonTypes.DrivingAgeStudent) {
				return 7;
			}

			if (PersonType == Global.Settings.PersonTypes.ChildAge5Through15) {
				return 2;
			}

			if (PersonType == Global.Settings.PersonTypes.ChildUnder5) {
				return 1;
			}

			return 9;
		}

		public virtual void UpdatePersonValues() {
			if (!Global.Configuration.IsInEstimationMode && UsualWorkParcel != null) {
				IEnumerable<dynamic> pathTypeModels =
					PathTypeModelFactory.Model
						.Run(Household.RandomUtility, Household.ResidenceParcel, UsualWorkParcel, Global.Settings.Times.SevenAM, Global.Settings.Times.FivePM, Global.Settings.Purposes.Work, Global.Coefficients_BaseCostCoefficientPerMonetaryUnit, Global.Configuration.Coefficients_MeanTimeCoefficient_Work, true, 1, 0, 0.0, false, Global.Settings.Modes.Sov);
				
				var	autoPathRoundTrip = pathTypeModels.First();

				AutoTimeToUsualWork = autoPathRoundTrip.PathTime / 2.0;
				AutoDistanceToUsualWork = autoPathRoundTrip.PathDistance / 2.0;
			}

			if (!Global.Configuration.IsInEstimationMode && UsualSchoolParcel != null) {
				IEnumerable<dynamic> pathTypeModels =
					PathTypeModelFactory.Model
						.Run(Household.RandomUtility, Household.ResidenceParcel, UsualSchoolParcel, Global.Settings.Times.SevenAM, Global.Settings.Times.ThreePM, Global.Settings.Purposes.School, Global.Coefficients_BaseCostCoefficientPerMonetaryUnit, Global.Configuration.Coefficients_MeanTimeCoefficient_Other, true, 1, 0, 0.0, false, Global.Settings.Modes.Sov);
				
				var autoPathRoundTrip = pathTypeModels.First();

				AutoTimeToUsualSchool = autoPathRoundTrip.PathTime / 2.0;
				AutoDistanceToUsualSchool = autoPathRoundTrip.PathDistance / 2.0;
			}
		}

		public virtual void SetWorkParcelPredictions() {
			if (UsualWorkParcelId != Constants.DEFAULT_VALUE && UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
				UsualWorkParcel.EmploymentPrediction += Household.ExpansionFactor;
			}
		}

		public virtual void SetSchoolParcelPredictions() {
			if (UsualSchoolParcelId == Constants.DEFAULT_VALUE || UsualSchoolParcelId == Global.Settings.OutOfRegionParcelId) {
				return;
			}

			if (IsAdult) {
				UsualSchoolParcel.StudentsUniversityPrediction += Household.ExpansionFactor;
			}
			else {
				UsualSchoolParcel.StudentsK12Prediction += Household.ExpansionFactor;
			}
		}

		#endregion

		#region init/utility/export methods

		public void Export() {
			_exporter.Export(_person);
		}

		public static void Close() {
			Global
				.Kernel
				.Get<IPersistenceFactory<IPerson>>()
				.Close();
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder
				.AppendLine(string.Format("Person ID: {0}",
					Id));

			builder
				.AppendLine(string.Format("Household ID: {0}, Sequence: {1}",
					HouseholdId,
					Sequence));

			builder
				.AppendLine(string.Format("Usual Work Parcel ID: {0}, Usual Work Zone Key: {1}, Auto Distance To Usual Work: {2}, Auto Time To Usual Work: {3}",
					UsualWorkParcelId,
					UsualWorkZoneKey,
					AutoDistanceToUsualWork,
					AutoTimeToUsualWork));

			builder
				.AppendLine(string.Format("Usual School Parcel ID: {0}, Usual School Zone Key: {1}, Auto Distance To Usual School: {2}, Auto Time To Usual School: {3}",
					UsualSchoolParcelId,
					UsualSchoolZoneKey,
					AutoDistanceToUsualSchool,
					AutoTimeToUsualSchool));

			return builder.ToString();
		}

		private void SetParcelRelationships() {
			IParcelWrapper usualWorkParcel;

			if (UsualWorkParcelId != Constants.DEFAULT_VALUE && ChoiceModelFactory.Parcels.TryGetValue(UsualWorkParcelId, out usualWorkParcel)) {
				UsualWorkParcel = usualWorkParcel;
			}

			IParcelWrapper usualSchoolParcel;

			if (UsualSchoolParcelId != Constants.DEFAULT_VALUE && ChoiceModelFactory.Parcels.TryGetValue(UsualSchoolParcelId, out usualSchoolParcel)) {
				UsualSchoolParcel = usualSchoolParcel;
			}
		}

		private void SetExpansionFactor() {
            ExpansionFactor = Household.ExpansionFactor * Global.Configuration.HouseholdSamplingRateOneInX;
		}

		#endregion
	}
}