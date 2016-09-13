// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Linq;
using DaySim.ChoiceModels.Default.Models;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Exceptions;
using DaySim.Framework.Factories;
using Ninject;
//using TransitPassOwnershipModel = DaySim.ChoiceModels.H.Models.TransitPassOwnershipModel;

namespace DaySim.ChoiceModels.Default {
	[UsedImplicitly]
	[Factory(Factory.ChoiceModelFactory, ChoiceModelRunner = Framework.Factories.ChoiceModelRunner.Default)]
	public sealed class ChoiceModelRunner : IChoiceModelRunner {
		private readonly IHouseholdWrapper _household;

		public ChoiceModelRunner(IHousehold household) {
			_household =
				Global
					.Kernel
					.Get<IWrapperFactory<IHouseholdCreator>>()
					.Creator
					.CreateWrapper(household);
		}

		public void SetRandomSeed(int randomSeed) {
			_household.RandomUtility.ResetHouseholdSynchronization(randomSeed);
			_household.RandomUtility.ResetUniform01(randomSeed);
			_household.Init();
		}

		public void RunChoiceModels(int batchNumber) {
			RunPersonModels(batchNumber);
			RunHouseholdModels(batchNumber);
			if (RunPersonDayModels(batchNumber)) {
		 		UpdateHousehold();
			}

			if (ChoiceModelFactory.ThreadQueue != null) {
				ChoiceModelFactory.ThreadQueue.Add(this);
			}
		}

		private void RunHouseholdModels(int batchNumber) {
			if (!Global.Configuration.ShouldRunHouseholdModels) {
				return;
			}

#if RELEASE
			try {
#endif
				ChoiceModelFactory.TotalTimesHouseholdModelSuiteRun[batchNumber]++;
			RunHouseholdModelSuite(_household, batchNumber);
#if RELEASE
			}
			catch (Exception e) {
				throw new HouseholdModelException(string.Format("Error running household models for {0}.", _household), e);
			}
#endif
		}

		private void RunPersonModels(int batchNumber) {
			if (!Global.Configuration.ShouldRunPersonModels) {
				return;
			}

			foreach (var person in _household.Persons) {
#if RELEASE
				try {
#endif
					ChoiceModelFactory.TotalTimesPersonModelSuiteRun[batchNumber]++;
				RunPersonModelSuite(person, batchNumber);
#if RELEASE
				}
				catch (Exception e) {
					throw new PersonModelException(string.Format("Error running person models for {0}.", person), e);
				}
#endif
			}
		}

		private bool RunPersonDayModels(int batchNumber) {
			if (!Global.Configuration.ShouldRunPersonDayModels) {
				return false;
			}

			foreach (var personDay in _household.HouseholdDays.SelectMany(householdDay => householdDay.PersonDays)) {
#if RELEASE
				try {
#endif

				ChoiceModelFactory.TotalPersonDays[batchNumber]++;
				var simulatedAnInvalidPersonDay = false;

				while (!personDay.IsValid  && (!Global.Configuration.IsInEstimationMode || !simulatedAnInvalidPersonDay)) { //don't retry household in estimation mode) {
					
					if (Global.Configuration.InvalidAttemptsBeforeContinue > 0 && personDay.AttemptedSimulations > Global.Configuration.InvalidAttemptsBeforeContinue) {
						Global.PrintFile.WriteLine("***** Person day for household {0} person {1} in zone {2} invalid after {3} attempts", personDay.HouseholdId, personDay.Person.Sequence, personDay.Household.ResidenceZoneKey, personDay.AttemptedSimulations);
						return false;
						break;
					}
					else {
						personDay.IsValid = true;
					}

					ChoiceModelFactory.TotalTimesPersonDayModelSuiteRun[batchNumber]++;
					RunPersonDayModelSuite(personDay, batchNumber);
					RunTourModels(personDay, batchNumber);

					// exits the loop if the person's day is valid
					if (personDay.IsValid) {
						// after updating park and ride lot loads
						if (!Global.Configuration.IsInEstimationMode && personDay.Tours != null) {
							foreach (var tour in personDay.Tours.Where(tour => tour.Mode == Global.Settings.Modes.ParkAndRide)) {
								tour.SetParkAndRideStay();
							}
						}

						break;
					}

					personDay.AttemptedSimulations++;

					if (!simulatedAnInvalidPersonDay) {
						simulatedAnInvalidPersonDay = true;

						// counts unique instances where a person's day is invalid
							ChoiceModelFactory.TotalInvalidAttempts[batchNumber]++;
					}

					personDay.Reset();
				}
#if RELEASE
				}
				catch (Exception e) {
					throw new PersonDayModelException(string.Format("Error running person-day models for {0}.", personDay), e);
				}
#endif
			}
			return true;
		}

		private static void RunTourModels(IPersonDayWrapper personDay, int batchNumber) {
			if (!Global.Configuration.ShouldRunTourModels) {
				return;
			}

			// creates or adds tours to a person's day based on application or estimation mode
			// tours are created by purpose
			personDay.SetTours();

			foreach (var tour in personDay.Tours) {
#if RELEASE
				try {
#endif
					ChoiceModelFactory.TotalTimesTourModelSuiteRun[batchNumber]++;
				RunTourModelSuite(tour, batchNumber);

				if (!personDay.IsValid) {
					if (Global.Configuration.IsInEstimationMode && Global.Configuration.EstimationModel == "IntermediateStopLocationModel") {
						Global.PrintFile.WriteEstimationRecordExclusionMessage("ChoiceModelRunner", "RunTourModels", tour.Household.Id, tour.Person.Sequence, -1, tour.Sequence, -1, -1, tour.HalfTour1Trips + tour.HalfTour2Trips);
					}

					return;
				}

					ChoiceModelFactory.TotalTimesTourTripModelsRun[batchNumber]++;
				RunTourTripModels(tour, batchNumber);

				if (!personDay.IsValid) {
					return;
				}

					ChoiceModelFactory.TotalTimesTourSubtourModelsRun[batchNumber]++;
				RunSubtourModels(tour, batchNumber);

				if (!personDay.IsValid) {
					return;
				}
#if RELEASE
				}
				catch (Exception e) {
					throw new TourModelException(string.Format("Error running tour models for {0}.", tour), e);
				}
#endif
			}
		}

		private static void RunTourTripModels(ITourWrapper tour, int batchNumber) {
			if (!Global.Configuration.ShouldRunTourTripModels) {
				return;
			}

				ChoiceModelFactory.TotalTimesProcessHalfToursRun[batchNumber]++;
			ProcessHalfTours(tour, batchNumber);

			if (!tour.PersonDay.IsValid) {
				return;
			}

			tour.SetOriginTimes();
		}

		private static void RunSubtourModels(ITourWrapper tour, int batchNumber) {
			if (!Global.Configuration.ShouldRunSubtourModels) {
				return;
			}

			foreach (var subtour in tour.Subtours) {
#if RELEASE
				try {
#endif
					ChoiceModelFactory.TotalTimesTourSubtourModelSuiteRun[batchNumber]++;
				RunSubtourModelSuite(subtour, batchNumber);

				if (!tour.PersonDay.IsValid) {
					return;
				}

					ChoiceModelFactory.TotalTimesSubtourTripModelsRun[batchNumber]++;
				RunSubtourTripModels(subtour, batchNumber);

				if (!tour.PersonDay.IsValid) {
					return;
				}
#if RELEASE
				}
				catch (Exception e) {
					throw new SubtourModelException(string.Format("Error running subtour models for {0}.", subtour), e);
				}
#endif
			}
		}

		private static void RunSubtourTripModels(ITourWrapper subtour, int batchNumber) {
			if (!Global.Configuration.ShouldRunSubtourTripModels) {
				return;
			}

				ChoiceModelFactory.TotalTimesProcessHalfSubtoursRun[batchNumber]++;
			ProcessHalfTours(subtour, batchNumber);

			if (!subtour.PersonDay.IsValid) {
				return;
			}

			subtour.SetOriginTimes();
		}

		private static void RunHouseholdModelSuite(IHouseholdWrapper household, int batchNumber) {
			if (!Global.Configuration.ShouldRunAutoOwnershipModel) {
				return;
			}

			// sets number of vehicles in household
				ChoiceModelFactory.TotalTimesAutoOwnershipModelRun[batchNumber]++;
			Global.ChoiceModelSession.Get<AutoOwnershipModel>().Run(household);
		}

		private void RunPersonModelSuite(IPersonWrapper person, int batchNumber) {
			if (Global.Configuration.ShouldRunWorkLocationModel && person.IsFullOrPartTimeWorker) {
				if (Global.Configuration.IsInEstimationMode || person.Household.RandomUtility.Uniform01() > _household.FractionWorkersWithJobsOutsideRegion) {
					// sets a person's usual work location
					// for full or part-time workers
						ChoiceModelFactory.TotalTimesWorkLocationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<WorkLocationModel>().Run(person, Global.Configuration.WorkLocationModelSampleSize);
				}
				else {
					if (!Global.Configuration.IsInEstimationMode) {
						person.UsualWorkParcelId = Global.Settings.OutOfRegionParcelId;
					}
				}
			}

			if (Global.Configuration.ShouldRunSchoolLocationModel && person.IsStudent) {
				// sets a person's school location
					ChoiceModelFactory.TotalTimesSchoolLocationModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<SchoolLocationModel>().Run(person, Global.Configuration.SchoolLocationModelSampleSize);
			}

			if (Global.Configuration.ShouldRunWorkLocationModel && person.IsWorker && person.IsNotFullOrPartTimeWorker) {
				if (Global.Configuration.IsInEstimationMode || person.Household.RandomUtility.Uniform01() > _household.FractionWorkersWithJobsOutsideRegion) {
					// sets a person's usual work location
					// for other workers in relation to a person's school location
						ChoiceModelFactory.TotalTimesWorkLocationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<WorkLocationModel>().Run(person, Global.Configuration.WorkLocationModelSampleSize);
				}
				else {
					if (!Global.Configuration.IsInEstimationMode) {
						person.UsualWorkParcelId = Global.Settings.OutOfRegionParcelId;
					}
				}
			}
			if (person.IsWorker && person.UsualWorkParcel != null // && person.UsualWorkParcel.ParkingOffStreetPaidDailySpacesBuffer2 > 0 
				 && Global.Configuration.IncludePayToParkAtWorkplaceModel) {
				if (Global.Configuration.ShouldRunPayToParkAtWorkplaceModel) {
						ChoiceModelFactory.TotalTimesPaidParkingAtWorkplaceModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<PayToParkAtWorkplaceModel>().Run(person);
				}
			}
			else {
				person.PaidParkingAtWorkplace = 1; // by default, people pay the parcel parking price
			}

            if (!Global.Configuration.IsInEstimationMode && Global.Configuration.Policy_UniversalTransitPassOwnership)
            {
                person.TransitPassOwnership = 1; //policy to turn on transit pass ownership
            }
            else if (!person.IsChildUnder5 && Global.Configuration.IncludeTransitPassOwnershipModel && Global.Configuration.ShouldRunTransitPassOwnershipModel)
            {
                    ChoiceModelFactory.TotalTimesTransitPassOwnershipModelRun[batchNumber]++;
                Global.ChoiceModelSession.Get<TransitPassOwnershipModel>().Run(person);
            }
            else
            {
                person.TransitPassOwnership = 0; // by default, people don't own a transit pass
            }
		}

		private static void RunPersonDayModelSuite(IPersonDayWrapper personDay, int batchNumber) {
			if (Global.Configuration.ShouldRunIndividualPersonDayPatternModel) {
				// determines if there are tours for a person's day
				// sets number of stops for a person's day
					ChoiceModelFactory.TotalTimesPersonDayPatternModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<IndividualPersonDayPatternModel>().Run(personDay);
			}

			if (!Global.Configuration.ShouldRunPersonExactNumberOfToursModel) {
				return;
			}

			if (personDay.WorkTours > 0) {
				// sets number of work tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.Work);
			}

			if (personDay.SchoolTours > 0) {
				// sets number of school tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.School);
			}

			if (personDay.EscortTours > 0) {
				// sets number of escort tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.Escort);
			}

			if (personDay.PersonalBusinessTours > 0) {
				// sets number of personal business tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.PersonalBusiness);
			}

			if (personDay.ShoppingTours > 0) {
				// sets number of shopping tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.Shopping);
			}

			if (personDay.MealTours > 0) {
				// sets number of meal tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.Meal);
			}

			if (personDay.SocialTours > 0) {
				// sets number of social tours for a person's day
					ChoiceModelFactory.TotalTimesPersonExactNumberOfToursModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<PersonExactNumberOfToursModel>().Run(personDay, Global.Settings.Purposes.Social);
			}
		}

		private static void RunTourModelSuite(ITourWrapper tour, int batchNumber) {
			tour.SetHomeBasedIsSimulated();

			SetTourDestination(tour, batchNumber);

			if (!tour.PersonDay.IsValid) {
				return;
			}

			GenerateSubtours(tour, batchNumber);

			if (!tour.PersonDay.IsValid) {
				return;
			}
			SetTourModeAndTime(tour, batchNumber);

			if (!tour.PersonDay.IsValid) {
				return;
			}

			if (!Global.Configuration.IsInEstimationMode && tour.DestinationArrivalTime > tour.DestinationDepartureTime) {
				Global.PrintFile.WriteArrivalTimeGreaterThanDepartureTimeWarning("ChoiceModelRunner", "RunTourModels", tour.PersonDay.Id, tour.DestinationArrivalTime, tour.DestinationDepartureTime);
				tour.PersonDay.IsValid = false;

				return;
			}

			// # = busy :(
			// - = available :)

			// carves out a person's availability for the day in relation to the tour
			// person day availabilty [----###########----]
			tour.PersonDay.TimeWindow.SetBusyMinutes(tour.DestinationArrivalTime, tour.DestinationDepartureTime);

			if (tour.Subtours.Count == 0) {
				return;
			}

			// sets the availabilty for a tour's subtours 
			// tour availabilty [####-----------####]
			tour.TimeWindow.SetBusyMinutes(1, tour.DestinationArrivalTime);
			tour.TimeWindow.SetBusyMinutes(tour.DestinationDepartureTime, Global.Settings.Times.MinutesInADay);
		}

		private static void SetTourDestination(ITourWrapper tour, int batchNumber) {
			if (tour.DestinationPurpose == Global.Settings.Purposes.Work) {
				if (Global.Configuration.ShouldRunWorkTourDestinationModel) {
					// sets the destination for the work tour
					// the usual work location or some another work location
					ChoiceModelFactory.TotalTimesWorkTourDestinationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<WorkTourDestinationModel>().Run(tour, Global.Configuration.WorkTourDestinationModelSampleSize);
				}

				return;
			}
			else if (tour.DestinationPurpose == Global.Settings.Purposes.School) {
				if (!Global.Configuration.IsInEstimationMode) {
					// sets the destination for the school tour
					tour.DestinationParcelId = tour.Person.UsualSchoolParcelId;
					tour.DestinationParcel = tour.Person.UsualSchoolParcel;
					tour.DestinationZoneKey = tour.Person.UsualSchoolZoneKey;
					tour.DestinationAddressType = Global.Settings.AddressTypes.UsualSchool;
					//add code to set simulated times of day for mode choice
					var tourCategory = tour.GetTourCategory();
					ChoiceModelUtility.DrawRandomTourTimePeriods(tour, tourCategory);
				}

				return;
			}
			else {
				if (Global.Configuration.ShouldRunOtherTourDestinationModel) {
					// sets the destination for the work tour
					// the usual work location or some another work location
					ChoiceModelFactory.TotalTimesOtherTourDestinationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<OtherTourDestinationModel>().Run(tour, Global.Configuration.OtherTourDestinationModelSampleSize);
				}

				return;
			}
		}

		private static void GenerateSubtours(ITourWrapper tour, int batchNumber) {
			// when the tour is to the usual work location then subtours for a work-based tour are created
			if (tour.Person.UsualWorkParcel == null || tour.DestinationParcel == null
				 || tour.DestinationParcel != tour.Person.UsualWorkParcel || !Global.Configuration.ShouldRunWorkBasedSubtourGenerationModel) {
				return;
			}

			if (Global.Configuration.IsInEstimationMode) {
				var nCallsForTour = 0;
				foreach (var subtour in tour.Subtours) {
					// -- in estimation mode --
					// sets the destination purpose of the subtour when in application mode

						ChoiceModelFactory.TotalTimesWorkBasedSubtourGenerationModelRun[batchNumber]++;
					nCallsForTour++;
					Global.ChoiceModelSession.Get<WorkBasedSubtourGenerationModel>().Run(tour, nCallsForTour, subtour.DestinationPurpose);
				}
				nCallsForTour++;
					ChoiceModelFactory.TotalTimesWorkBasedSubtourGenerationModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<WorkBasedSubtourGenerationModel>().Run(tour, nCallsForTour);
			}
			else {
				// creates the subtours for work tour 
				var nCallsForTour = 0;
				while (tour.Subtours.Count < 4) {
					// -- in application mode --
					// sets the destination purpose of the subtour
						ChoiceModelFactory.TotalTimesWorkBasedSubtourGenerationModelRun[batchNumber]++;
					nCallsForTour++;
					var destinationPurposeForSubtour = Global.ChoiceModelSession.Get<WorkBasedSubtourGenerationModel>().Run(tour, nCallsForTour);

					if (destinationPurposeForSubtour == Global.Settings.Purposes.NoneOrHome) {
						break;
					}
					// the subtour is added to the tour's Subtours collection when the subtour's purpose is not NONE_OR_HOME
					tour.Subtours.Add(tour.CreateSubtour(tour.DestinationAddressType, tour.DestinationParcelId, tour.DestinationZoneKey, destinationPurposeForSubtour));
				}

				tour.PersonDay.WorkBasedTours += tour.Subtours.Count;
			}
		}

		private static void SetTourModeAndTime(ITourWrapper tour, int batchNumber) {
			if (tour.DestinationPurpose == Global.Settings.Purposes.Work) {
				if (Global.Configuration.ShouldRunWorkTourModeModel) {
					// sets the work tour's mode of travel to the destination
					ChoiceModelFactory.TotalTimesWorkTourModeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<WorkTourModeModel>().Run(tour);
				}

				if (Global.Configuration.ShouldRunWorkTourTimeModel) {
					// sets the work tour's destination arrival and departure times
					ChoiceModelFactory.TotalTimesWorkTourTimeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<WorkTourTimeModel>().Run(tour);
				}

				return;
			}
			else if (tour.DestinationPurpose == Global.Settings.Purposes.School) {
				if (Global.Configuration.ShouldRunSchoolTourModeModel) {
					// sets the school tour's mode of travel to the destination
					ChoiceModelFactory.TotalTimesSchoolTourModeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<SchoolTourModeModel>().Run(tour);
				}

				if (Global.Configuration.ShouldRunSchoolTourTimeModel) {
					// sets the school tour's destination arrival and departure times
					ChoiceModelFactory.TotalTimesSchoolTourTimeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<SchoolTourTimeModel>().Run(tour);
				}

				return;
			}
			else if (tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
				if (Global.Configuration.ShouldRunEscortTourModeModel) {
					// sets the escort tour's mode of travel to the destination
					ChoiceModelFactory.TotalTimesEscortTourModeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<EscortTourModeModel>().Run(tour);
				}

				if (Global.Configuration.ShouldRunOtherHomeBasedTourTimeModel) {
					// sets the escort tour's destination arrival and departure times
					ChoiceModelFactory.TotalTimesOtherHomeBasedTourTimeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<OtherHomeBasedTourTimeModel>().Run(tour);
				}

				return;
			}
			else {
				if (Global.Configuration.ShouldRunOtherHomeBasedTourModeModel) {
					// sets the tour's mode of travel to the destination with the purposes personal business, shopping, meal, social
					ChoiceModelFactory.TotalTimesOtherHomeBasedTourModeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<OtherHomeBasedTourModeModel>().Run(tour);
				}

				if (Global.Configuration.ShouldRunOtherHomeBasedTourTimeModel) {
					// sets the tour's destination arrival and departure times with the purposes personal business, shopping, meal, social
					ChoiceModelFactory.TotalTimesOtherHomeBasedTourTimeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<OtherHomeBasedTourTimeModel>().Run(tour);
				}

				return;
			}
		}

		private static void RunSubtourModelSuite(ITourWrapper subtour, int batchNumber) {
			subtour.SetWorkBasedIsSimulated();

			SetSubtourDestination(subtour, batchNumber);

			if (!subtour.PersonDay.IsValid) {
				return;
			}

			SetSubtourModeAndTime(subtour, batchNumber);

			if (!subtour.PersonDay.IsValid) {
				return;
			}

			if (!Global.Configuration.IsInEstimationMode) {
				if (subtour.DestinationArrivalTime > subtour.DestinationDepartureTime) {
					Global.PrintFile.WriteArrivalTimeGreaterThanDepartureTimeWarning("ChoiceModelRunner", "RunTourModels", subtour.PersonDay.Id, subtour.DestinationArrivalTime, subtour.DestinationDepartureTime);
					subtour.PersonDay.IsValid = false;

					return;
				}

				if (subtour.DestinationArrivalTime < subtour.ParentTour.DestinationArrivalTime || subtour.DestinationDepartureTime > subtour.ParentTour.DestinationDepartureTime) {
					Global.PrintFile.WriteSubtourArrivalAndDepartureTimesOutOfRangeWarning("ChoiceModelRunner", "RunTourModels", subtour.PersonDay.Id, subtour.DestinationArrivalTime, subtour.DestinationDepartureTime, subtour.ParentTour.DestinationArrivalTime, subtour.ParentTour.DestinationDepartureTime);
					subtour.PersonDay.IsValid = false;

					return;
				}
			}

			// # = busy :(
			// - = available :)

			// updates the parent tour's availabilty [----###########----]
			subtour.ParentTour.TimeWindow.SetBusyMinutes(subtour.DestinationArrivalTime, subtour.DestinationDepartureTime);
		}

		private static void SetSubtourDestination(ITourWrapper subtour, int batchNumber) {
			if (subtour.DestinationPurpose == Global.Settings.Purposes.Work) {
				if (Global.Configuration.ShouldRunWorkTourDestinationModel) {
					// sets the destination for the work tour
					// the usual work location or some another work location
					ChoiceModelFactory.TotalTimesWorkSubtourDestinationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<WorkTourDestinationModel>().Run(subtour, Global.Configuration.WorkTourDestinationModelSampleSize);
				}

				return;
			}
			else if (subtour.DestinationPurpose == Global.Settings.Purposes.School) {
				if (!Global.Configuration.IsInEstimationMode) {
					// sets the destination for the school subtour
					subtour.DestinationParcelId = subtour.Person.UsualSchoolParcelId;
					subtour.DestinationParcel = subtour.Person.UsualSchoolParcel;
					subtour.DestinationZoneKey = subtour.Person.UsualSchoolZoneKey;
					subtour.DestinationAddressType = Global.Settings.AddressTypes.UsualSchool;
					//add code to set simulated times of day for mode choice
					var subtourCategory = subtour.GetTourCategory();
					ChoiceModelUtility.DrawRandomTourTimePeriods(subtour, subtourCategory);
				}

				return;
			}
			else {
				if (Global.Configuration.ShouldRunOtherTourDestinationModel) {
					// sets the destination for the subtour
					ChoiceModelFactory.TotalTimesOtherSubtourDestinationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<OtherTourDestinationModel>().Run(subtour, Global.Configuration.OtherTourDestinationModelSampleSize);
				}

				return;
			}
		}

		private static void SetSubtourModeAndTime(ITourWrapper subtour, int batchNumber) {
			if (Global.Configuration.ShouldRunWorkBasedSubtourModeModel) {
				// sets the subtour's mode of travel to the destination
					ChoiceModelFactory.TotalTimesWorkBasedSubtourModeModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<WorkBasedSubtourModeModel>().Run(subtour);
			}

			if (Global.Configuration.ShouldRunWorkBasedSubtourTimeModel) {
				// sets subtour's destination arrival and departure times
					ChoiceModelFactory.TotalTimesWorkBasedSubtourTimeModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<WorkBasedSubtourTimeModel>().Run(subtour);
			}
		}

		private static void ProcessHalfTours(ITourWrapper tour, int batchNumber) {
			// goes in two directions, from origin to destination and destination to origin
			for (var direction = Global.Settings.TourDirections.OriginToDestination; direction <= Global.Settings.TourDirections.TotalTourDirections; direction++) {
				// creates origin and destination half-tours
				// creates or adds a trip to a tour based on application or estimation mode
				tour.SetHalfTours(direction);

				// the half-tour from the origin to destination or the half-tour from the destination to origin
				// depending on the direction
				var halfTour = tour.GetHalfTour(direction);

				// halfTour.Trips will dynamically grow, so keep this in a for loop
				for (var i = 0; i < halfTour.Trips.Count; i++) {
					var trip = halfTour.Trips[i];

#if RELEASE
					try {
#endif
					halfTour.SimulatedTrips++;

					if (trip.IsHalfTourFromOrigin) {
						tour.HalfTour1Trips++;
					}
					else {
						tour.HalfTour2Trips++;
					}

					ChoiceModelFactory.TotalTimesTripModelSuiteRun[batchNumber]++;
					//Global.PrintFile.WriteLine("Before - trip {0} sequence {1} orig parcel {2} orig zone {3}", trip.Id, trip.Sequence, trip.OriginParcelId, trip.OriginZoneKey); 
					//Global.PrintFile.WriteLine("Before - trip {0} sequence {1} dest parcel {2} dest zone {3}", trip.Id, trip.Sequence, trip.DestinationParcelId, trip.DestinationZoneKey); 
					RunTripModelSuite(tour, halfTour, trip, batchNumber);
					//Global.PrintFile.WriteLine("*After - trip {0} sequence {1} orig parcel {2} orig zone {3}", trip.Id, trip.Sequence, trip.OriginParcelId, trip.OriginZoneKey); 
					//Global.PrintFile.WriteLine("*After - trip {0} sequence {1} dest parcel {2} dest zone {3}", trip.Id, trip.Sequence, trip.DestinationParcelId, trip.DestinationZoneKey); 

					if (!trip.PersonDay.IsValid) {
						return;
					}
#if RELEASE
					}
					catch (Exception e) {
						throw new TripModelException(string.Format("Error running trip models for {0}.", trip), e);
					}
#endif
				}
			}
		}

		private static void RunTripModelSuite(ITourWrapper tour, IHalfTour halfTour, ITripWrapper trip, int batchNumber) {
			var nextTrip = GenerateIntermediateStop(halfTour, trip, batchNumber);

			SetIntermediateStopDestination(trip, nextTrip, batchNumber);
			SetTripModeAndTime(tour, trip, batchNumber);

			if (!trip.PersonDay.IsValid) {
				return;
			}

			// retrieves window based on whether or not the trip's tour is home-based or work-based
			var timeWindow = tour.IsHomeBasedTour ? tour.PersonDay.TimeWindow : tour.ParentTour.TimeWindow;

			if (trip.IsHalfTourFromOrigin && trip.Sequence == 1) {
				// occupies minutes in window between destination and stop
				timeWindow.SetBusyMinutes(tour.DestinationArrivalTime, trip.ArrivalTime);
			}
			else if (!trip.IsHalfTourFromOrigin && trip.Sequence == 1) {
				// occupies minutes in window between destination and stop
				timeWindow.SetBusyMinutes(tour.DestinationDepartureTime, trip.ArrivalTime);
			}
			else {
				// occupies minutes in window from previous stop to stop
				timeWindow.SetBusyMinutes(trip.GetPreviousTrip().DepartureTime, trip.ArrivalTime);
			}
		}

		private static ITripWrapper GenerateIntermediateStop(IHalfTour halfTour, ITripWrapper trip, int batchNumber) {
			if (!Global.Configuration.ShouldRunIntermediateStopGenerationModel) {
				return null;
			}

			ITripWrapper nextTrip = null;

			if (Global.Configuration.IsInEstimationMode) {
				// -- in estimation mode --
				// sets the trip's destination purpose, determines whether or not a stop is generated in application mode
				// uses trip instead of nextTrip, deals with subtours with tour origin at work
				// need to set trip.IsToTourOrigin first
				trip.IsToTourOrigin = trip.Sequence == trip.HalfTour.Trips.Count(); // last trip in half tour 
				var intermediateStopPurpose = trip.IsToTourOrigin ? Global.Settings.Purposes.NoneOrHome : trip.DestinationPurpose;
				nextTrip = trip.GetNextTrip();

				if (intermediateStopPurpose != Global.Settings.Purposes.NoneOrHome) {
						ChoiceModelFactory.TotalTimesIntermediateStopGenerated[batchNumber]++;
					}
				if (trip.PersonDay.GetTotalStops() > 0) {
						ChoiceModelFactory.TotalTimesIntermediateStopGenerationModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<IntermediateStopGenerationModel>().Run(trip, intermediateStopPurpose);
				}
			}
			else {
				// -- in application mode --
				// sets the trip's destination purpose, determines whether or not a stop is generated

				// first, if it is the first trip on a park and ride half tour, then make it a change mode stop
				// TODO: this doesn't allow stops between the destination and the transit stop - can improve later
				int intermediateStopPurpose;
				if (trip.Sequence == 1 && trip.Tour.Mode == Global.Settings.Modes.ParkAndRide) {
					intermediateStopPurpose = Global.Settings.Purposes.ChangeMode;
						ChoiceModelFactory.TotalTimesChangeModeStopGenerated[batchNumber]++;
				}
				else if (trip.PersonDay.GetTotalStops() == 0) {
					intermediateStopPurpose = Global.Settings.Purposes.NoneOrHome;
				}
				else {
						ChoiceModelFactory.TotalTimesIntermediateStopGenerationModelRun[batchNumber]++;
					intermediateStopPurpose = Global.ChoiceModelSession.Get<IntermediateStopGenerationModel>().Run(trip);
				}

				if (intermediateStopPurpose != Global.Settings.Purposes.NoneOrHome) {
						ChoiceModelFactory.TotalTimesIntermediateStopGenerated[batchNumber]++;
					var destinationPurposeForNextTrip = trip.DestinationPurpose;

					// creates the next trip in the half-tour 
					// the next trip's destination is set to the current trip's destination
					nextTrip = halfTour.CreateNextTrip(trip, intermediateStopPurpose, destinationPurposeForNextTrip);

					halfTour.Trips.Add(nextTrip);

					trip.DestinationAddressType = Global.Settings.AddressTypes.None;
					trip.DestinationPurpose = intermediateStopPurpose;
					trip.IsToTourOrigin = false;
				}
				else {
					trip.IsToTourOrigin = true;
				}
			}

			return nextTrip;
		}

		private static void SetIntermediateStopDestination(ITripWrapper trip, ITripWrapper nextTrip, int batchNumber) {
			if (nextTrip == null || trip.IsToTourOrigin || !Global.Configuration.ShouldRunIntermediateStopLocationModel) {
				if (trip.IsToTourOrigin) {
						ChoiceModelFactory.TotalTimesTripIsToTourOrigin[batchNumber]++;
					}
				else if (nextTrip == null) {
						ChoiceModelFactory.TotalTimesNextTripIsNull[batchNumber]++;
					}

				if (trip.DestinationPurpose == Global.Settings.Purposes.NoneOrHome && Global.Configuration.IsInEstimationMode && Global.Configuration.EstimationModel == "IntermediateStopLocationModel") {
					Global.PrintFile.WriteEstimationRecordExclusionMessage("ChoiceModelRunner", "SetIntermediateStopDestination", trip.Household.Id, trip.Person.Sequence, trip.Day, trip.Tour.Sequence, trip.Direction, trip.Sequence, 1);
				}

				return;
			}

			// sets the new destination for the trip

			if (trip.DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
				// CHANGE_MODE location is always park and ride node for tour
				var parkAndRideNode = ChoiceModelFactory.ParkAndRideNodeDao.Get(trip.Tour.ParkAndRideNodeId);

				if (parkAndRideNode != null) {
					trip.DestinationParcelId = parkAndRideNode.NearestParcelId;
					trip.DestinationParcel = ChoiceModelFactory.Parcels[trip.DestinationParcelId];
					trip.DestinationZoneKey =ChoiceModelFactory.ZoneKeys[parkAndRideNode.ZoneId];
               //trip.DestinationZoneKey = parkAndRideNode.Id;
					trip.DestinationAddressType = Global.Settings.AddressTypes.Other;

						ChoiceModelFactory.TotalTimesChangeModeLocationSet[batchNumber]++;
				}
			}
			else {
					ChoiceModelFactory.TotalTimesIntermediateStopLocationModelRun[batchNumber]++;
				Global.ChoiceModelSession.Get<IntermediateStopLocationModel>().Run(trip, Global.Configuration.IntermediateStopLocationModelSampleSize);
			}
			if (Global.Configuration.IsInEstimationMode) {
				return;
			}

			nextTrip.OriginParcelId = trip.DestinationParcelId;
			nextTrip.OriginParcel = trip.DestinationParcel;
			nextTrip.OriginZoneKey = trip.DestinationZoneKey;
			nextTrip.SetOriginAddressType(trip.DestinationAddressType);
		}

		private static void SetTripModeAndTime(ITourWrapper tour, ITripWrapper trip, int batchNumber) {
			if (Global.Configuration.ShouldRunTripModeModel) {
				// sets the trip's mode of travel to the destination
				if (trip.DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
					// trips to change mode destination are always by transit
					ChoiceModelFactory.TotalTimesChangeModeTransitModeSet[batchNumber]++;
					trip.Mode = Global.Settings.Modes.Transit;
					if (Global.StopAreaIsEnabled && Global.Configuration.WriteStopAreaIDsInsteadOfZonesForTransitTrips) {
						trip.OriginZoneKey = trip.Tour.ParkAndRideOriginStopAreaKey;
						trip.DestinationZoneKey = trip.Tour.ParkAndRideDestinationStopAreaKey;
					}
				}
                else
                {
					ChoiceModelFactory.TotalTimesTripModeModelRun[batchNumber]++;
					Global.ChoiceModelSession.Get<TripModeModel>().Run(trip);
				}
				if (!trip.PersonDay.IsValid) {
					return;
				}
			}

			// sets the trip's destination arrival and departure times
			if (trip.Sequence == 1) {
				if (!Global.Configuration.IsInEstimationMode) {
					trip.DepartureTime = trip.IsHalfTourFromOrigin ? tour.DestinationArrivalTime : tour.DestinationDepartureTime;
					trip.UpdateTripValues();
				}
			}
			else if (trip.OriginPurpose == Global.Settings.Purposes.ChangeMode) {
				//stay at park and ride lot assumed to be 3 minutes
				if (!Global.Configuration.IsInEstimationMode) {
					int endpoint;

					if (trip.IsHalfTourFromOrigin) {
						trip.DepartureTime = trip.GetPreviousTrip().ArrivalTime - 3;
						endpoint = trip.DepartureTime + 1;
					}
					else {
						trip.DepartureTime = trip.GetPreviousTrip().ArrivalTime + 3;
						endpoint = trip.DepartureTime - 1;
					}
					if (trip.DepartureTime >= 1 && trip.DepartureTime <= Global.Settings.Times.MinutesInADay && trip.PersonDay.TimeWindow.EntireSpanIsAvailable(endpoint, trip.DepartureTime)) {
						trip.UpdateTripValues();
					}
					else {
						if (!Global.Configuration.IsInEstimationMode) 
						trip.PersonDay.IsValid = false;
					}
				}
			}
			else {
				if (Global.Configuration.ShouldRunTripTimeModel) {
						ChoiceModelFactory.TotalTimesTripTimeModelRun[batchNumber]++;

					Global.ChoiceModelSession.Get<TripTimeModel>().Run(trip);
				}
			}
		}

		private void UpdateHousehold() {
			foreach (var person in _household.Persons) {
				person.UpdatePersonValues();
			}

			foreach (var tour in _household.HouseholdDays.SelectMany(householdDay => householdDay.PersonDays.Where(personDay => personDay.Tours != null)).SelectMany(personDay => personDay.Tours)) {
				tour.UpdateTourValues();

				foreach (var subtour in tour.Subtours) {
					subtour.UpdateTourValues();
				}
			}
		}
		
		public void Save() {
			_household.Export();

			foreach (var person in _household.Persons) {
				person.Export();
			}

			foreach (var householdDay in _household.HouseholdDays) {
				householdDay.Export();

				if (Global.Settings.UseJointTours) {
					foreach (var jointTour in householdDay.JointToursList) {
						jointTour.Export();
					}

					foreach (var fullHalfTour in householdDay.FullHalfToursList) {
						fullHalfTour.Export();
					}

					foreach (var fullHalfTour in householdDay.FullHalfToursList) {
						fullHalfTour.Export();
					}

					foreach (var partialHalfTour in householdDay.PartialHalfToursList) {
						partialHalfTour.Export();
					}
				}

				foreach (var personDay in householdDay.PersonDays) {
					personDay.Export();

					if (personDay.Tours == null) {
						continue;
					}

					if (personDay.Tours.Count > 1) {
						// sorts tours chronologically
						personDay.Tours.Sort((tour1, tour2) => tour1.OriginDepartureTime.CompareTo(tour2.OriginDepartureTime));
					}

					foreach (var tour in personDay.Tours) {
						tour.Export();

						if (tour.HalfTourFromOrigin != null && tour.HalfTourFromDestination != null) {
							foreach (var trip in tour.HalfTourFromOrigin.Trips.Invert()) {
								trip.SetTourSequence(tour.Sequence);
								trip.SetTripValueOfTime();
								trip.Export();

								ChoiceModelUtility.WriteTripForTDM(trip, ChoiceModelFactory.TDMTripListExporter);
							}

							foreach (var trip in tour.HalfTourFromDestination.Trips) {
								trip.SetTourSequence(tour.Sequence);
								trip.SetTripValueOfTime();
								trip.Export();

								ChoiceModelUtility.WriteTripForTDM(trip, ChoiceModelFactory.TDMTripListExporter);
							}
						}

						if (tour.Subtours.Count > 1) {
							// sorts subtours chronologically
							tour.Subtours.Sort((tour1, tour2) => tour1.OriginDepartureTime.CompareTo(tour2.OriginDepartureTime));
						}

						foreach (var subtour in tour.Subtours) {
							subtour.SetParentTourSequence(tour.Sequence);
							subtour.Export();

							if (subtour.HalfTourFromOrigin == null || subtour.HalfTourFromDestination == null) {
								continue;
							}

							foreach (var trip in subtour.HalfTourFromOrigin.Trips.Invert()) {
								trip.SetTourSequence(subtour.Sequence);
								trip.Export();

								ChoiceModelUtility.WriteTripForTDM(trip, ChoiceModelFactory.TDMTripListExporter);
							}

							foreach (var trip in subtour.HalfTourFromDestination.Trips) {
								trip.SetTourSequence(subtour.Sequence);
								trip.Export();

								ChoiceModelUtility.WriteTripForTDM(trip, ChoiceModelFactory.TDMTripListExporter);
							}
						}
					}
				}
			}
		}
	}
}