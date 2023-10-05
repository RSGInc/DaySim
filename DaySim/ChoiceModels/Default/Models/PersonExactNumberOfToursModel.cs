// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Default.Models {
  public class PersonExactNumberOfToursModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "PersonExactNumberOfToursModel";
    private const int TOTAL_ALTERNATIVES = 3;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 753;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.PersonExactNumberOfToursModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(IPersonDayWrapper personDay, int purpose) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }


      personDay.ResetRandom(890 + purpose);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
        //mb added code so that this model is only estimated on full days of data
        if (personDay.DayBeginsAtHome == 0 || personDay.DayEndsAtHome == 0) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator((personDay.Id * 397) ^ purpose);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        int tours;

        if (purpose == Global.Settings.Purposes.Work) {
          tours = personDay.WorkTours;
        } else if (purpose == Global.Settings.Purposes.School) {
          tours = personDay.SchoolTours;
        } else if (purpose == Global.Settings.Purposes.Escort) {
          tours = personDay.EscortTours;
        } else if (purpose == Global.Settings.Purposes.PersonalBusiness) {
          tours = personDay.PersonalBusinessTours;
        } else if (purpose == Global.Settings.Purposes.Shopping) {
          tours = personDay.ShoppingTours;
        } else if (purpose == Global.Settings.Purposes.Meal) {
          tours = personDay.MealTours;
        } else if (purpose == Global.Settings.Purposes.Social) {
          tours = personDay.SocialTours;
        } else {
          tours = Constants.DEFAULT_VALUE;
        }

        RunModel(choiceProbabilityCalculator, personDay, purpose, tours);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, personDay, purpose);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        personDay.HomeBasedTours += choice;

        if (purpose == Global.Settings.Purposes.Work) {
          personDay.WorkTours = choice;
        } else if (purpose == Global.Settings.Purposes.School) {
          personDay.SchoolTours = choice;
        } else if (purpose == Global.Settings.Purposes.Escort) {
          personDay.EscortTours = choice;
        } else if (purpose == Global.Settings.Purposes.PersonalBusiness) {
          personDay.PersonalBusinessTours = choice;
        } else if (purpose == Global.Settings.Purposes.Shopping) {
          personDay.ShoppingTours = choice;
        } else if (purpose == Global.Settings.Purposes.Meal) {
          personDay.MealTours = choice;
        } else if (purpose == Global.Settings.Purposes.Social) {
          personDay.SocialTours = choice;
        }
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonDayWrapper personDay, int purpose, int choice = Constants.DEFAULT_VALUE) {
      IHouseholdWrapper household = personDay.Household;
      IParcelWrapper residenceParcel = household.ResidenceParcel;
      IPersonWrapper person = personDay.Person;

      int workAtHome = (Global.Configuration.UseWorkAtHomeModelAndVariables && personDay.WorkAtHomeDuration > Global.Configuration.WorkAtHome_DurationThreshold).ToFlag();

      //added code so that these bias variables only apply in estimation mode
      int diaryBased = (Global.Configuration.IsInEstimationMode && Global.Configuration.UseDiaryVsSmartphoneBiasVariables && person.PaperDiary > 0).ToFlag();
      int proxyBased = (Global.Configuration.IsInEstimationMode && Global.Configuration.UseProxyBiasVariables && person.ProxyResponse > 0).ToFlag();
 
      int carsPerDriver = household.GetCarsPerDriver();
      double mixedDensity = residenceParcel.ParcelHouseholdsPerRetailServiceFoodEmploymentBuffer2();
      double intersectionDensity = residenceParcel.IntersectionDensity34Minus1Buffer2();

      double purposeLogsum;

      if (purpose == Global.Settings.Purposes.Work) {
        if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId) {
          purposeLogsum = 0;
        } else {
          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);

          purposeLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }
      } else if (purpose == Global.Settings.Purposes.School) {
        if (person.UsualSchoolParcel == null || person.UsualSchoolParcelId == household.ResidenceParcelId) {
          purposeLogsum = 0;
        } else {
          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(personDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);

          purposeLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }
      } else {
        int carOwnership = person.GetCarOwnershipSegment();
        int votSegment = person.Household.GetVotALSegment();
        int transitAccess = residenceParcel.TransitAccessSegment();

        purposeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][purpose][carOwnership][votSegment][transitAccess];
      }

      // 1 TOUR

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 1);

      alternative.Choice = 1;

      alternative.AddUtilityTerm(1, purpose);

      // 2 TOURS

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 2);

      const int two = 2;
      alternative.Choice = two;

      alternative.AddUtilityTerm(100 * purpose + 1, person.IsFulltimeWorker.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 2, person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 3, person.IsRetiredAdult.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 4, person.IsNonworkingAdult.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 5, person.IsUniversityStudent.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 6, person.IsDrivingAgeStudent.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 7, person.IsChildAge5Through15.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 8, person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 9, household.Has0To25KIncome.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 10, household.Has25To45KIncome.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 11, household.Has75KPlusIncome.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 12, carsPerDriver);
      alternative.AddUtilityTerm(100 * purpose + 13, person.IsOnlyAdult().ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 14, person.IsOnlyFullOrPartTimeWorker().ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 15, 0);
      alternative.AddUtilityTerm(100 * purpose + 16, person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * (!household.HasChildrenUnder16).ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 17, person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 18, person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 19, person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 20, person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 21, person.AgeIsBetween18And25.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 22, person.AgeIsBetween26And35.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 23, person.AgeIsBetween51And65.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 24, person.WorksAtHome().ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 25, mixedDensity);
      alternative.AddUtilityTerm(100 * purpose + 26, intersectionDensity);
      alternative.AddUtilityTerm(100 * purpose + 31, personDay.WorkTours);
      alternative.AddUtilityTerm(100 * purpose + 32, personDay.SchoolTours);
      alternative.AddUtilityTerm(100 * purpose + 33, personDay.EscortTours);
      alternative.AddUtilityTerm(100 * purpose + 34, personDay.PersonalBusinessTours);
      alternative.AddUtilityTerm(100 * purpose + 35, personDay.ShoppingTours);
      alternative.AddUtilityTerm(100 * purpose + 36, personDay.MealTours);
      alternative.AddUtilityTerm(100 * purpose + 37, personDay.SocialTours);
      alternative.AddUtilityTerm(100 * purpose + 38, workAtHome);
      alternative.AddUtilityTerm(100 * purpose + 39, diaryBased);
      alternative.AddUtilityTerm(100 * purpose + 40, proxyBased);
      alternative.AddUtilityTerm(100 * purpose + 41, personDay.WorkStops);

      if (purpose <= Global.Settings.Purposes.Escort) {
        alternative.AddUtilityTerm(100 * purpose + 42, personDay.SchoolStops);
      }

      alternative.AddUtilityTerm(100 * purpose + 43, personDay.EscortStops);
      alternative.AddUtilityTerm(100 * purpose + 44, personDay.PersonalBusinessStops);
      alternative.AddUtilityTerm(100 * purpose + 45, personDay.ShoppingStops);
      alternative.AddUtilityTerm(100 * purpose + 46, personDay.MealStops);
      alternative.AddUtilityTerm(100 * purpose + 47, personDay.SocialStops);
      alternative.AddUtilityTerm(100 * purpose + 50 + two, 1); // ASC
      alternative.AddUtilityTerm(100 * purpose + 23 + 2 * two, purposeLogsum); // accessibility effect has different coefficient for 2 and 3+

      // 3+ TOURS

      alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 3);

      const int three = 3;
      alternative.Choice = three;

      alternative.AddUtilityTerm(100 * purpose + 1, person.IsFulltimeWorker.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 2, person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 3, person.IsRetiredAdult.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 4, person.IsNonworkingAdult.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 5, person.IsUniversityStudent.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 6, person.IsDrivingAgeStudent.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 7, person.IsChildAge5Through15.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 8, person.IsChildUnder5.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 9, household.Has0To25KIncome.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 10, household.Has25To45KIncome.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 11, household.Has75KPlusIncome.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 12, carsPerDriver);
      alternative.AddUtilityTerm(100 * purpose + 13, person.IsOnlyAdult().ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 14, person.IsOnlyFullOrPartTimeWorker().ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 15, 0);
      alternative.AddUtilityTerm(100 * purpose + 16, person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * (!household.HasChildrenUnder16).ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 17, person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 18, person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 19, person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 20, person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 21, person.AgeIsBetween18And25.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 22, person.AgeIsBetween26And35.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 23, person.AgeIsBetween51And65.ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 24, person.WorksAtHome().ToFlag());
      alternative.AddUtilityTerm(100 * purpose + 25, mixedDensity);
      alternative.AddUtilityTerm(100 * purpose + 26, intersectionDensity);
      alternative.AddUtilityTerm(100 * purpose + 31, personDay.WorkTours);
      alternative.AddUtilityTerm(100 * purpose + 32, personDay.SchoolTours);
      alternative.AddUtilityTerm(100 * purpose + 33, personDay.EscortTours);
      alternative.AddUtilityTerm(100 * purpose + 34, personDay.PersonalBusinessTours);
      alternative.AddUtilityTerm(100 * purpose + 35, personDay.ShoppingTours);
      alternative.AddUtilityTerm(100 * purpose + 36, personDay.MealTours);
      alternative.AddUtilityTerm(100 * purpose + 37, personDay.SocialTours);
      alternative.AddUtilityTerm(100 * purpose + 38, workAtHome);
      alternative.AddUtilityTerm(100 * purpose + 39, diaryBased);
      alternative.AddUtilityTerm(100 * purpose + 40, proxyBased);
      alternative.AddUtilityTerm(100 * purpose + 41, personDay.WorkStops);

      if (purpose <= Global.Settings.Purposes.Escort) {
        alternative.AddUtilityTerm(100 * purpose + 42, personDay.SchoolStops);
      }

      alternative.AddUtilityTerm(100 * purpose + 43, personDay.EscortStops);
      alternative.AddUtilityTerm(100 * purpose + 44, personDay.PersonalBusinessStops);
      alternative.AddUtilityTerm(100 * purpose + 45, personDay.ShoppingStops);
      alternative.AddUtilityTerm(100 * purpose + 46, personDay.MealStops);
      alternative.AddUtilityTerm(100 * purpose + 47, personDay.SocialStops);
      alternative.AddUtilityTerm(100 * purpose + 50 + three, 1); // ASC
      alternative.AddUtilityTerm(100 * purpose + 23 + 2 * three, purposeLogsum); // accessibility effect has different coefficient for 2 and 3+
    }
  }
}
