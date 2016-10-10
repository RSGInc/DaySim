// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels;
using DaySim.DomainModels.Default;
using DaySim.DomainModels.Extensions;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
	public class IndividualPersonDayPatternModel : ChoiceModel{
		private const string CHOICE_MODEL_NAME = "IndividualPersonDayPatternModel";
		private const int TOTAL_ALTERNATIVES = 2080;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 1416;

		private DayPattern[] _dayPatterns;

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.IndividualPersonDayPatternModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
			InitializeDayPatterns();
		}

		public void Run(IPersonDayWrapper personDay) {
			if (personDay == null) {
				throw new ArgumentNullException("personDay");
			}
			
			personDay.ResetRandom(5);

			if (Global.Configuration.IsInEstimationMode) {
				if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
					return;
				}
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Id);

			if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
				RunModel(choiceProbabilityCalculator, personDay, new DayPattern(personDay));

				choiceProbabilityCalculator.WriteObservation();
			}
			else {
				if (personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId && personDay.Person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {
					RunModel(choiceProbabilityCalculator, personDay);

					var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
					var choice = (DayPattern) chosenAlternative.Choice;

					personDay.WorkTours = choice.WorkTours;
					personDay.SchoolTours = choice.SchoolTours;
					personDay.EscortTours = choice.EscortTours;
					personDay.PersonalBusinessTours = choice.PersonalBusinessTours;
					personDay.ShoppingTours = choice.ShoppingTours;
					personDay.MealTours = choice.MealTours;
					personDay.SocialTours = choice.SocialTours;

					personDay.WorkStops = choice.WorkStops;
					personDay.SchoolStops = choice.SchoolStops;
					personDay.EscortStops = choice.EscortStops;
					personDay.PersonalBusinessStops = choice.PersonalBusinessStops;
					personDay.ShoppingStops = choice.ShoppingStops;
					personDay.MealStops = choice.MealStops;
					personDay.SocialStops = choice.SocialStops;
				}
			}
		}

		private void InitializeDayPatterns()
		{

			if (_dayPatterns != null)
			{
				return;
			}

			_dayPatterns = new DayPattern[TOTAL_ALTERNATIVES];

			var alternativeIndex = -1;

			for (var workTours = 0; workTours <= 1; workTours++)
			{
				for (var schoolTours = 0; schoolTours <= 1; schoolTours++)
				{
					for (var escortTours = 0; escortTours <= 1; escortTours++)
					{
						for (var personalBusinessTours = 0; personalBusinessTours <= 1; personalBusinessTours++)
						{
							for (var shoppingTours = 0; shoppingTours <= 1; shoppingTours++)
							{
								for (var mealTours = 0; mealTours <= 1; mealTours++)
								{
									for (var socialTours = 0; socialTours <= 1; socialTours++)
									{
										for (var workStops = 0; workStops <= 1; workStops++)
										{
											for (var schoolStops = 0; schoolStops <= 1; schoolStops++)
											{
												for (var escortStops = 0; escortStops <= 1; escortStops++)
												{
													for (var personalBusinessStops = 0; personalBusinessStops <= 1; personalBusinessStops++)
													{
														for (var shoppingStops = 0; shoppingStops <= 1; shoppingStops++)
														{
															for (var mealStops = 0; mealStops <= 1; mealStops++)
															{
																for (var socialStops = 0; socialStops <= 1; socialStops++)
																{
																	var totalTours = workTours + schoolTours + escortTours + personalBusinessTours + shoppingTours +
																	                 mealTours + socialTours;
																	var totalStops = workStops + schoolStops + escortStops + personalBusinessStops + shoppingStops +
																	                 mealStops + socialStops;

																	// checks for:
																	// three tours or less
																	// four stops or less
																	// five stops total or less
																	// stops are less than or equal to tours
																	// school and work stops are less than or equal to school and work tours
																	// not both work and school stops
																	if (totalTours > 3 || totalStops > 4 || totalTours + totalStops > 5 ||
																	    Math.Min(totalStops, 1) > totalTours ||
																	    Math.Min(workStops + schoolStops, 1) > workTours + schoolTours || workStops + schoolStops > 1)
																	{
																		continue;
																	}

																	alternativeIndex++; // next alternative

																	var tours = new int[Global.Settings.Purposes.TotalPurposes];

																	tours[Global.Settings.Purposes.Work] = workTours;
																	tours[Global.Settings.Purposes.School] = schoolTours;
																	tours[Global.Settings.Purposes.Escort] = escortTours;
																	tours[Global.Settings.Purposes.PersonalBusiness] = personalBusinessTours;
																	tours[Global.Settings.Purposes.Shopping] = shoppingTours;
																	tours[Global.Settings.Purposes.Meal] = mealTours;
																	tours[Global.Settings.Purposes.Social] = socialTours;

																	var stops = new int[Global.Settings.Purposes.TotalPurposes];

																	stops[Global.Settings.Purposes.Work] = workStops;
																	stops[Global.Settings.Purposes.School] = schoolStops;
																	stops[Global.Settings.Purposes.Escort] = escortStops;
																	stops[Global.Settings.Purposes.PersonalBusiness] = personalBusinessStops;
																	stops[Global.Settings.Purposes.Shopping] = shoppingStops;
																	stops[Global.Settings.Purposes.Meal] = mealStops;
																	stops[Global.Settings.Purposes.Social] = socialStops;

																	_dayPatterns[alternativeIndex] = new DayPattern(tours, totalTours, stops, totalStops);
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonDayWrapper personDay, DayPattern choice = null) {
			var household = personDay.Household;
			var residenceParcel = household.ResidenceParcel;
			var person = personDay.Person;

			var carsPerDriver = household.GetCarsPerDriver();
			var mixedDensity = residenceParcel.MixedUse3Index2();
			var intersectionDensity = residenceParcel.IntersectionDensity34Minus1Buffer2();

			var purposeLogsums = new double[Global.Settings.Purposes.TotalPurposes];
			var atUsualLogsums = new double[3];
			var carOwnership = person.GetCarOwnershipSegment();
			var votSegment = person.Household.GetVotALSegment();
			var transitAccess = residenceParcel.TransitAccessSegment();

			if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId) {
				purposeLogsums[Global.Settings.Purposes.Work] = 0;
			}
			else {
				var destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
				var destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
				var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);

				purposeLogsums[Global.Settings.Purposes.Work] = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				atUsualLogsums[Global.Settings.Purposes.Work] = Global.AggregateLogsums[person.UsualWorkParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][person.UsualWorkParcel.TransitAccessSegment()];
			}

			if (person.UsualSchoolParcel == null || person.UsualSchoolParcelId == household.ResidenceParcelId) {
				purposeLogsums[Global.Settings.Purposes.School] = 0;
			}
			else {
				var destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
				var destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);
				var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(personDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);

				purposeLogsums[Global.Settings.Purposes.School] = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
				atUsualLogsums[Global.Settings.Purposes.School] = Global.AggregateLogsums[person.UsualSchoolParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][person.UsualSchoolParcel.TransitAccessSegment()];
			}

			var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][transitAccess];

			purposeLogsums[Global.Settings.Purposes.Escort] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Escort][carOwnership][votSegment][transitAccess];
			purposeLogsums[Global.Settings.Purposes.PersonalBusiness] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.PersonalBusiness][carOwnership][votSegment][transitAccess];
			purposeLogsums[Global.Settings.Purposes.Shopping] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Shopping][carOwnership][votSegment][transitAccess];
			purposeLogsums[Global.Settings.Purposes.Meal] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Meal][carOwnership][votSegment][transitAccess];
			purposeLogsums[Global.Settings.Purposes.Social] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Social][carOwnership][votSegment][transitAccess];
			purposeLogsums[Global.Settings.Purposes.Recreation] = compositeLogsum;
			purposeLogsums[Global.Settings.Purposes.Medical] = compositeLogsum;

			for (var xPurpose = Global.Settings.Purposes.Work; xPurpose <= Global.Settings.Purposes.Social + 10; xPurpose++) {
				// extra components 1-5 are for 2,3,4,5,6 tour purposes
				// extra components 6-10 are for 2,3,4,5,6 stop puroposes

				// recode purpose to match coefficients
				var purpose = xPurpose <= Global.Settings.Purposes.Social ? xPurpose :
					                                                              xPurpose <= Global.Settings.Purposes.Social + 5 ?
						                                                                                                       Global.Settings.Purposes.Social + 1 : Global.Settings.Purposes.Social + 2;

				// get correct multiplier on coefficients.
				var xMultiplier = xPurpose <= Global.Settings.Purposes.Social ? 1.0 :
					                                                             xPurpose <= Global.Settings.Purposes.Social + 5 ?
						                                                                                                      Math.Log(xPurpose - Global.Settings.Purposes.Social + 1) : Math.Log(xPurpose - Global.Settings.Purposes.Social - 5 + 1);

				choiceProbabilityCalculator.CreateUtilityComponent(xPurpose);
				var component = choiceProbabilityCalculator.GetUtilityComponent(xPurpose);

				component.AddUtilityTerm(100 * purpose + 51, xMultiplier * person.IsFulltimeWorker.ToFlag());
				component.AddUtilityTerm(100 * purpose + 2, xMultiplier * person.IsPartTimeWorker.ToFlag());
				component.AddUtilityTerm(100 * purpose + 3, xMultiplier * person.IsRetiredAdult.ToFlag());
				component.AddUtilityTerm(100 * purpose + 4, xMultiplier * person.IsNonworkingAdult.ToFlag());
				component.AddUtilityTerm(100 * purpose + 5, xMultiplier * person.IsUniversityStudent.ToFlag());
				component.AddUtilityTerm(100 * purpose + 6, xMultiplier * person.IsDrivingAgeStudent.ToFlag());
				component.AddUtilityTerm(100 * purpose + 7, xMultiplier * person.IsChildAge5Through15.ToFlag());
				component.AddUtilityTerm(100 * purpose + 8, xMultiplier * person.IsChildUnder5.ToFlag());
				component.AddUtilityTerm(100 * purpose + 9, xMultiplier * household.Has0To25KIncome.ToFlag());
				component.AddUtilityTerm(100 * purpose + 10, xMultiplier * household.Has25To45KIncome.ToFlag());
				component.AddUtilityTerm(100 * purpose + 11, xMultiplier * household.Has75KPlusIncome.ToFlag());
				component.AddUtilityTerm(100 * purpose + 12, xMultiplier * carsPerDriver);
				component.AddUtilityTerm(100 * purpose + 13, xMultiplier * person.IsOnlyAdult().ToFlag());
				component.AddUtilityTerm(100 * purpose + 14, xMultiplier * person.IsOnlyFullOrPartTimeWorker().ToFlag());
				component.AddUtilityTerm(100 * purpose + 15, xMultiplier * 0);
				component.AddUtilityTerm(100 * purpose + 16, xMultiplier * person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * (!household.HasChildrenUnder16).ToFlag());
				component.AddUtilityTerm(100 * purpose + 17, xMultiplier * person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
				component.AddUtilityTerm(100 * purpose + 18, xMultiplier * person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
				component.AddUtilityTerm(100 * purpose + 19, xMultiplier * person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
				component.AddUtilityTerm(100 * purpose + 20, xMultiplier * person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
				component.AddUtilityTerm(100 * purpose + 21, xMultiplier * person.AgeIsBetween18And25.ToFlag());
				component.AddUtilityTerm(100 * purpose + 22, xMultiplier * person.AgeIsBetween26And35.ToFlag());
				component.AddUtilityTerm(100 * purpose + 23, xMultiplier * person.AgeIsBetween51And65.ToFlag());
				component.AddUtilityTerm(100 * purpose + 24, xMultiplier * person.WorksAtHome.ToFlag());
				component.AddUtilityTerm(100 * purpose + 25, xMultiplier * mixedDensity);
				component.AddUtilityTerm(100 * purpose + 26, xMultiplier * intersectionDensity);
				component.AddUtilityTerm(100 * purpose + 27, xMultiplier * purposeLogsums[purpose]);
				component.AddUtilityTerm(100 * purpose + 28, xMultiplier * person.TransitPassOwnership);
			}

			// tour utility
			var tourComponentIndex = Global.Settings.Purposes.Social + 11;
			choiceProbabilityCalculator.CreateUtilityComponent(tourComponentIndex);
			var tourComponent = choiceProbabilityCalculator.GetUtilityComponent(tourComponentIndex);
			tourComponent.AddUtilityTerm(1401, carsPerDriver);
			tourComponent.AddUtilityTerm(1402, person.WorksAtHome.ToFlag());
			tourComponent.AddUtilityTerm(1403, mixedDensity);
			tourComponent.AddUtilityTerm(1404, mixedDensity * person.IsChildAge5Through15.ToFlag());
			tourComponent.AddUtilityTerm(1405, compositeLogsum);
			tourComponent.AddUtilityTerm(1406, person.TransitPassOwnership);

			// stop utility
			var stopComponentIndex = Global.Settings.Purposes.Social + 12;
			choiceProbabilityCalculator.CreateUtilityComponent(stopComponentIndex);
			var stopComponent = choiceProbabilityCalculator.GetUtilityComponent(stopComponentIndex);
			stopComponent.AddUtilityTerm(1411, carsPerDriver);
			stopComponent.AddUtilityTerm(1412, person.WorksAtHome.ToFlag());
			stopComponent.AddUtilityTerm(1413, mixedDensity);
			stopComponent.AddUtilityTerm(1414, mixedDensity * person.IsChildAge5Through15.ToFlag());
			stopComponent.AddUtilityTerm(1415, compositeLogsum);
			stopComponent.AddUtilityTerm(1416, person.TransitPassOwnership);

            for (var alternativeIndex = 0; alternativeIndex < TOTAL_ALTERNATIVES; alternativeIndex++)
            {

				var dayPattern = _dayPatterns[alternativeIndex];
				var available =
					// work tours and stops only available for workers
					(person.IsWorker || (dayPattern.WorkTours <= 0 && dayPattern.WorkStops <= 0)) &&
					// school tours and stops only available for students with usual school parcel not at home
					((person.IsStudent && person.UsualSchoolParcel != null && person.UsualSchoolParcel != person.Household.ResidenceParcel) || (dayPattern.SchoolTours <= 0 && dayPattern.SchoolStops <= 0)) &&
					// school stops not available if usual school parcel is same as usual work parcel 
					((person.IsStudent && person.UsualSchoolParcel != null && person.UsualSchoolParcel != person.UsualWorkParcel) || (dayPattern.SchoolStops <= 0));

				var alternative = choiceProbabilityCalculator.GetAlternative(alternativeIndex, available, choice != null && choice.Equals(dayPattern));

				if (!Global.Configuration.IsInEstimationMode && !alternative.Available) {
                    continue;
				}

				alternative.Choice = dayPattern;

				// components for the purposes
				for (var purpose = Global.Settings.Purposes.Work; purpose <= Global.Settings.Purposes.Social; purpose++) {
					if (dayPattern.Tours[purpose] > 0 || dayPattern.Stops[purpose] > 0) {
						alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(purpose));

						if (dayPattern.Tours[purpose] > 0) {
							alternative.AddUtilityTerm(100 * purpose, 1); // tour purpose ASC
							alternative.AddUtilityTerm(100 * purpose + 29, purposeLogsums[purpose]); // tour purpose logsum
							alternative.AddUtilityTerm(100 * purpose + 30, person.PaidParkingAtWorkplace); // only use for work purpose
						}

						if (dayPattern.Stops[purpose] > 0) {
							alternative.AddUtilityTerm(100 * purpose + 1, 1); // stop purpose ASC
							alternative.AddUtilityTerm(100 * purpose + 31, purposeLogsums[purpose]); // stop purpose logsum
						}
						if (Global.Configuration.IsInEstimationMode) {
							alternative.AddUtilityTerm(100 * purpose + 32, 1 - person.PaperDiary);
							alternative.AddUtilityTerm(100 * purpose + 33, person.ProxyResponse);
						}
					}
				}

				// multiple tour purposes component
				if (dayPattern.TotalTours > 1) {
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(Global.Settings.Purposes.Social + (dayPattern.TotalTours - 1)));
				}

				// multiple stop purposes component
				if (dayPattern.TotalStops > 1) {
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(Global.Settings.Purposes.Social + 5 + (dayPattern.TotalStops - 1)));
				}

				for (var tourPurpose = Global.Settings.Purposes.Work; tourPurpose <= Global.Settings.Purposes.Social; tourPurpose++) {
					for (var stopPurpose = Global.Settings.Purposes.Work; stopPurpose <= Global.Settings.Purposes.Social - 1; stopPurpose++) {
						if (tourPurpose > Global.Settings.Purposes.School && stopPurpose <= Global.Settings.Purposes.School) {
							continue;
						}

						if (dayPattern.Tours[tourPurpose] > 0 && dayPattern.Stops[stopPurpose] > 0) {
							alternative.AddUtilityTerm(1000 + 10 * tourPurpose + stopPurpose, 1); // tour-stop comb. utility
						}
					}
				}

				for (var tourPurpose = Global.Settings.Purposes.Work; tourPurpose <= Global.Settings.Purposes.School; tourPurpose++) {
					if (dayPattern.Tours[tourPurpose] == 1 && dayPattern.TotalStops >= 1) {
						alternative.AddUtilityTerm(1000 + 10 * tourPurpose, purposeLogsums[tourPurpose]); // usual location logsum x presence of stops in work or school pattern
						alternative.AddUtilityTerm(1000 + 10 * tourPurpose + 8, compositeLogsum); // home aggregate logsum x  presence of stops in work or school pattern
						alternative.AddUtilityTerm(1000 + 10 * tourPurpose + 9, atUsualLogsums[tourPurpose]); // at usual location aggregate logsum x  presence of stops in work or school pattern
					}
				}

				for (var tourPurpose = Global.Settings.Purposes.Work; tourPurpose <= Global.Settings.Purposes.Social - 2; tourPurpose++) {
					for (var tourPurpose2 = tourPurpose + 1; tourPurpose2 <= Global.Settings.Purposes.Social; tourPurpose2++) {
						if (dayPattern.Tours[tourPurpose] > 0 && dayPattern.Tours[tourPurpose2] > 0) {
							alternative.AddUtilityTerm(1100 + 10 * tourPurpose + tourPurpose2, 1); // tour-tour comb. utility
						}
					}
				}

				for (var stopPurpose = Global.Settings.Purposes.Work; stopPurpose <= Global.Settings.Purposes.Social - 2; stopPurpose++) {
					for (var stopPurpose2 = stopPurpose + 1; stopPurpose2 <= Global.Settings.Purposes.Social; stopPurpose2++) {
						if (dayPattern.Stops[stopPurpose] > 0 && dayPattern.Stops[stopPurpose2] > 0) {
							alternative.AddUtilityTerm(1200 + 10 * stopPurpose + stopPurpose2, 1); // stop-stop comb. utility
						}
					}
				}

				if (dayPattern.TotalTours > 0 && dayPattern.TotalStops > 0) {
					var totalStops = dayPattern.TotalStops;

					if (totalStops > 3) {
						totalStops = 3;
					}

					alternative.AddUtilityTerm(1300 + 10 * dayPattern.TotalTours + totalStops, 1); // nttour-ntstop utility
				}
				if (dayPattern.TotalTours - dayPattern.Tours[Global.Settings.Purposes.Work] - dayPattern.Tours[Global.Settings.Purposes.School] > 0) {
					// tour utility
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(tourComponentIndex));
				}
				if (dayPattern.TotalStops - dayPattern.Stops[Global.Settings.Purposes.Work] - dayPattern.Stops[Global.Settings.Purposes.School] > 0) {
					// stop utility
					alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(stopComponentIndex));
				}

            }
		}

		private sealed class DayPattern {
			private readonly int _hashCode;

			public DayPattern(int[] tours, int totalTours, int[] stops, int totalStops) {
				Tours = tours;

				WorkTours = tours[Global.Settings.Purposes.Work];
				SchoolTours = tours[Global.Settings.Purposes.School];
				EscortTours = tours[Global.Settings.Purposes.Escort];
				PersonalBusinessTours = tours[Global.Settings.Purposes.PersonalBusiness];
				ShoppingTours = tours[Global.Settings.Purposes.Shopping];
				MealTours = tours[Global.Settings.Purposes.Meal];
				SocialTours = tours[Global.Settings.Purposes.Social];

				TotalTours = totalTours;

				Stops = stops;

				WorkStops = stops[Global.Settings.Purposes.Work];
				SchoolStops = stops[Global.Settings.Purposes.School];
				EscortStops = stops[Global.Settings.Purposes.Escort];
				PersonalBusinessStops = stops[Global.Settings.Purposes.PersonalBusiness];
				ShoppingStops = stops[Global.Settings.Purposes.Shopping];
				MealStops = stops[Global.Settings.Purposes.Meal];
				SocialStops = stops[Global.Settings.Purposes.Social];

				TotalStops = totalStops;

				_hashCode = ComputeHashCode();
			}

			public DayPattern(IPersonDayWrapper personDay) {
				Tours = new int[Global.Settings.Purposes.TotalPurposes];

				WorkTours = Tours[Global.Settings.Purposes.Work] = personDay.WorkTours;
				SchoolTours = Tours[Global.Settings.Purposes.School] = personDay.SchoolTours;
				EscortTours = Tours[Global.Settings.Purposes.Escort] = personDay.EscortTours;
				PersonalBusinessTours = Tours[Global.Settings.Purposes.PersonalBusiness] = personDay.PersonalBusinessTours;
				ShoppingTours = Tours[Global.Settings.Purposes.Shopping] = personDay.ShoppingTours;
				MealTours = Tours[Global.Settings.Purposes.Meal] = personDay.MealTours;
				SocialTours = Tours[Global.Settings.Purposes.Social] = personDay.SocialTours;

				TotalTours = personDay.GetTotalTours();

				Stops = new int[Global.Settings.Purposes.TotalPurposes];

				WorkStops = Stops[Global.Settings.Purposes.Work] = personDay.WorkStops;
				SchoolStops = Stops[Global.Settings.Purposes.School] = personDay.SchoolStops;
				EscortStops = Stops[Global.Settings.Purposes.Escort] = personDay.EscortStops;
				PersonalBusinessStops = Stops[Global.Settings.Purposes.PersonalBusiness] = personDay.PersonalBusinessStops;
				ShoppingStops = Stops[Global.Settings.Purposes.Shopping] = personDay.ShoppingStops;
				MealStops = Stops[Global.Settings.Purposes.Meal] = personDay.MealStops;
				SocialStops = Stops[Global.Settings.Purposes.Social] = personDay.SocialStops;

				TotalStops = personDay.GetTotalStops();

				_hashCode = ComputeHashCode();
			}

			public int[] Tours { get; private set; }

			public int[] Stops { get; private set; }

			public int WorkTours { get; private set; }

			public int SchoolTours { get; private set; }

			public int EscortTours { get; private set; }

			public int PersonalBusinessTours { get; private set; }

			public int ShoppingTours { get; private set; }

			public int MealTours { get; private set; }

			public int SocialTours { get; private set; }

			public int TotalTours { get; private set; }

			public int WorkStops { get; private set; }

			public int SchoolStops { get; private set; }

			public int EscortStops { get; private set; }

			public int PersonalBusinessStops { get; private set; }

			public int ShoppingStops { get; private set; }

			public int MealStops { get; private set; }

			public int SocialStops { get; private set; }

			public int TotalStops { get; private set; }

			private int ComputeHashCode() {
				unchecked {
					var hashCode = (WorkTours > 0).ToFlag();

					hashCode = (hashCode * 397) ^ (SchoolTours > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (EscortTours > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (PersonalBusinessTours > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (ShoppingTours > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (MealTours > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (SocialTours > 0 ? 1 : 0);

					hashCode = (hashCode * 397) ^ (WorkStops > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (SchoolStops > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (EscortStops > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (PersonalBusinessStops > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (ShoppingStops > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (MealStops > 0 ? 1 : 0);
					hashCode = (hashCode * 397) ^ (SocialStops > 0 ? 1 : 0);

					return hashCode;
				}
			}

			public bool Equals(DayPattern other) {
				if (ReferenceEquals(null, other)) {
					return false;
				}

				if (ReferenceEquals(this, other)) {
					return true;
				}

				var workToursFlag = (WorkTours > 0).ToFlag();
				var schoolToursFlag = (SchoolTours > 0).ToFlag();
				var escortToursFlag = (EscortTours > 0).ToFlag();
				var personalBusinessToursFlag = (PersonalBusinessTours > 0).ToFlag();
				var shoppingToursFlag = (ShoppingTours > 0).ToFlag();
				var mealToursFlag = (MealTours > 0).ToFlag();
				var socialToursFlag = (SocialTours > 0).ToFlag();

				var workTours2Flag = (other.WorkTours > 0).ToFlag();
				var schoolTours2Flag = (other.SchoolTours > 0).ToFlag();
				var escortTours2Flag = (other.EscortTours > 0).ToFlag();
				var personalBusinessTours2Flag = (other.PersonalBusinessTours > 0).ToFlag();
				var shoppingTours2Flag = (other.ShoppingTours > 0).ToFlag();
				var mealTours2Flag = (other.MealTours > 0).ToFlag();
				var socialTours2Flag = (other.SocialTours > 0).ToFlag();

				var workStopsFlag = (WorkStops > 0).ToFlag();
				var schoolStopsFlag = (SchoolStops > 0).ToFlag();
				var escortStopsFlag = (EscortStops > 0).ToFlag();
				var personalBusinessStopsFlag = (PersonalBusinessStops > 0).ToFlag();
				var shoppingStopsFlag = (ShoppingStops > 0).ToFlag();
				var mealStopsFLag = (MealStops > 0).ToFlag();
				var socialStopsFlag = (SocialStops > 0).ToFlag();

				var workStops2Flag = (other.WorkStops > 0).ToFlag();
				var schoolStops2Flag = (other.SchoolStops > 0).ToFlag();
				var escortStops2Flag = (other.EscortStops > 0).ToFlag();
				var personalBusinessStops2Flag = (other.PersonalBusinessStops > 0).ToFlag();
				var shoppingStops2Flag = (other.ShoppingStops > 0).ToFlag();
				var mealStops2Flag = (other.MealStops > 0).ToFlag();
				var socialStops2Flag = (other.SocialStops > 0).ToFlag();

				return
					workToursFlag == workTours2Flag &&
					schoolToursFlag == schoolTours2Flag &&
					escortToursFlag == escortTours2Flag &&
					personalBusinessToursFlag == personalBusinessTours2Flag &&
					shoppingToursFlag == shoppingTours2Flag &&
					mealToursFlag == mealTours2Flag &&
					socialToursFlag == socialTours2Flag &&
					workStopsFlag == workStops2Flag &&
					schoolStopsFlag == schoolStops2Flag &&
					escortStopsFlag == escortStops2Flag &&
					personalBusinessStopsFlag == personalBusinessStops2Flag &&
					shoppingStopsFlag == shoppingStops2Flag &&
					mealStopsFLag == mealStops2Flag &&
					socialStopsFlag == socialStops2Flag;
			}

			public override bool Equals(object obj) {
				return Equals(obj as DayPattern);
			}

			public override int GetHashCode() {
				return _hashCode;
			}
		}
	}
}