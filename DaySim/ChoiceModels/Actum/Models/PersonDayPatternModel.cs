// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class PersonDayPatternModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumPersonDayPatternModel";
    private const int TOTAL_ALTERNATIVES = 214;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 1716;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.HouseholdPersonDayPatternModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      DayPattern[] dayPatterns = new DayPattern[TOTAL_ALTERNATIVES];
      if (householdDay.Household.Id == 80170 && personDay.Person.Sequence == 1) {
      }

      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      personDay.ResetRandom(948);

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          DayPattern dayPattern = new DayPattern(personDay);
          if (dayPattern.EscortTours > 0 && personDay.CreatedEscortTours == 0) {
            personDay.CreatedEscortTours++;
          }
          if (dayPattern.PersonalBusinessTours > 0 && personDay.CreatedPersonalBusinessTours == 0) {
            personDay.CreatedPersonalBusinessTours++;
          }
          if (dayPattern.ShoppingTours > 0 && personDay.CreatedShoppingTours == 0) {
            personDay.CreatedShoppingTours++;
          }
          if (dayPattern.SocialTours > 0 && personDay.CreatedSocialTours == 0) {
            personDay.CreatedSocialTours++;
          }
          return;
        }
      }

      InitializeDayPatterns(personDay, dayPatterns);

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        DayPattern dayPattern = new DayPattern(personDay);
        RunModel(choiceProbabilityCalculator, personDay, householdDay, dayPatterns, dayPattern);

        if (dayPattern.EscortTours > 0 && personDay.CreatedEscortTours == 0) {
          personDay.CreatedEscortTours++;
        }
        if (dayPattern.PersonalBusinessTours > 0 && personDay.CreatedPersonalBusinessTours == 0) {
          personDay.CreatedPersonalBusinessTours++;
        }
        if (dayPattern.ShoppingTours > 0 && personDay.CreatedShoppingTours == 0) {
          personDay.CreatedShoppingTours++;
        }
        if (dayPattern.SocialTours > 0 && personDay.CreatedSocialTours == 0) {
          personDay.CreatedSocialTours++;
        }
        choiceProbabilityCalculator.WriteObservation();
      } else {
        if (personDay.Person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId && personDay.Person.UsualSchoolParcelId != Global.Settings.OutOfRegionParcelId) {

          RunModel(choiceProbabilityCalculator, personDay, householdDay, dayPatterns);
          ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
          if (chosenAlternative == null) {
            personDay.IsValid = false;
            householdDay.IsValid = false;
            return;
          }

          DayPattern dayPattern = (DayPattern)chosenAlternative.Choice;

          if (dayPattern.EscortTours > 0 && personDay.CreatedEscortTours == 0) {
            personDay.GetNewTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey, Global.Settings.Purposes.Escort);
            personDay.CreatedEscortTours++;
          }
          if (dayPattern.PersonalBusinessTours > 0 && personDay.CreatedPersonalBusinessTours == 0) {
            personDay.GetNewTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey, Global.Settings.Purposes.PersonalBusiness);
            personDay.CreatedPersonalBusinessTours++;
          }
          if (dayPattern.ShoppingTours > 0 && personDay.CreatedShoppingTours == 0) {
            personDay.GetNewTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey, Global.Settings.Purposes.Shopping);
            personDay.CreatedShoppingTours++;
          }
          if (dayPattern.SocialTours > 0 && personDay.CreatedSocialTours == 0) {
            personDay.GetNewTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey, Global.Settings.Purposes.Social);
            personDay.CreatedSocialTours++;
          }

          personDay.EscortStops = dayPattern.EscortStops;
          personDay.PersonalBusinessStops = dayPattern.PersonalBusinessStops;
          personDay.ShoppingStops = dayPattern.ShoppingStops;
          personDay.SocialStops = dayPattern.SocialStops;

        }
      }
    }

    private void InitializeDayPatterns(PersonDayWrapper personDay, DayPattern[] dayPatterns) {
      //			if (_dayPatterns != null) {
      //				return;
      //			}

      int workTours = personDay.WorkTours > 0 ? 1 : 0;
      int schoolTours = personDay.SchoolTours > 0 ? 1 : 0;
      int businessTours = personDay.BusinessTours > 0 ? 1 : 0;
      int workStops = personDay.WorkStops > 0 ? 1 : 0;
      int schoolStops = personDay.SchoolStops > 0 ? 1 : 0;
      int businessStops = personDay.BusinessStops > 0 ? 1 : 0;
      int priorEscortTours = (personDay.EscortFullHalfTours > 0 || personDay.EscortJointTours > 0) ? 1 : 0;
      int priorPersonalBusinessTours = personDay.PersonalBusinessJointTours > 0 ? 1 : 0;
      int priorShoppingTours = personDay.ShoppingJointTours > 0 ? 1 : 0;
      int priorSocialTours = personDay.SocialJointTours > 0 ? 1 : 0;


      int alternativeIndex = -1;

      for (int escortTours = 0; escortTours <= 1; escortTours++) {
        for (int personalBusinessTours = 0; personalBusinessTours <= 1; personalBusinessTours++) {
          for (int shoppingTours = 0; shoppingTours <= 1; shoppingTours++) {
            for (int socialTours = 0; socialTours <= 1; socialTours++) {
              for (int escortStops = 0; escortStops <= 1; escortStops++) {
                for (int personalBusinessStops = 0; personalBusinessStops <= 1; personalBusinessStops++) {
                  for (int shoppingStops = 0; shoppingStops <= 1; shoppingStops++) {
                    for (int socialStops = 0; socialStops <= 1; socialStops++) {
                      int totalNonMandatoryTourPurposes = escortTours + personalBusinessTours + shoppingTours + socialTours;
                      int totalNonMandatoryStopPurposes = escortStops + personalBusinessStops + shoppingStops + socialStops;
                      int totalTourPurposes = totalNonMandatoryTourPurposes + workTours + schoolTours + businessTours;
                      int totalStopPurposes = totalNonMandatoryStopPurposes + workStops + schoolStops + businessStops;

                      // checks for:
                      // three tours or less
                      // four stops or less
                      // five stops total or less
                      if (totalNonMandatoryTourPurposes > 3 || totalNonMandatoryStopPurposes > 4 || totalNonMandatoryTourPurposes + totalNonMandatoryStopPurposes > 5) {
                        continue;
                      }

                      alternativeIndex++; // next alternative

                      int[] tours = new int[Global.Settings.Purposes.TotalPurposes];

                      tours[Global.Settings.Purposes.Work] = workTours;
                      tours[Global.Settings.Purposes.School] = schoolTours;
                      tours[Global.Settings.Purposes.Business] = businessTours;
                      tours[Global.Settings.Purposes.Escort] = escortTours;
                      tours[Global.Settings.Purposes.PersonalBusiness] = personalBusinessTours;
                      tours[Global.Settings.Purposes.Shopping] = shoppingTours;
                      tours[Global.Settings.Purposes.Social] = socialTours;

                      int[] stops = new int[Global.Settings.Purposes.TotalPurposes];

                      stops[Global.Settings.Purposes.Work] = workStops;
                      stops[Global.Settings.Purposes.School] = schoolStops;
                      stops[Global.Settings.Purposes.Business] = businessStops;
                      stops[Global.Settings.Purposes.Escort] = escortStops;
                      stops[Global.Settings.Purposes.PersonalBusiness] = personalBusinessStops;
                      stops[Global.Settings.Purposes.Shopping] = shoppingStops;
                      stops[Global.Settings.Purposes.Social] = socialStops;

                      bool available = totalNonMandatoryStopPurposes > 0 && totalTourPurposes == 0 ? false :
                          priorEscortTours > 0 && escortTours == 0 ? false :
                          priorPersonalBusinessTours > 0 && personalBusinessTours == 0 ? false :
                          priorShoppingTours > 0 && shoppingTours == 0 ? false :
                          priorSocialTours > 0 && socialTours == 0 ? false :
                          totalTourPurposes > 3 || totalStopPurposes > 4 || totalTourPurposes + totalStopPurposes > 5 ? false :
                          Math.Min(totalStopPurposes, 1) > totalTourPurposes ? false :
                          true;

                      dayPatterns[alternativeIndex] = new DayPattern(tours, totalTourPurposes, stops, totalStopPurposes, available);
                    }
                  }
                }
              }
            }
          }
        }
      }

    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, DayPattern[] dayPatterns, DayPattern choice = null) {
      Framework.DomainModels.Wrappers.IHouseholdWrapper household = personDay.Household;
      Framework.DomainModels.Wrappers.IParcelWrapper residenceParcel = household.ResidenceParcel;
      Framework.DomainModels.Wrappers.IPersonWrapper person = personDay.Person;

      int carsPerDriver = household.GetCarsPerDriver();
      double mixedDensity = residenceParcel.MixedUse3Index2();
      double intersectionDensity = residenceParcel.IntersectionDensity34Minus1Buffer2();

      double[] purposeLogsums = new double[Global.Settings.Purposes.TotalPurposes + 2];
      double[] atUsualLogsums = new double[3];
      //var carOwnership = person.CarOwnershipSegment; //GV: sat car ownership not to impact logsums
      int carOwnership = 0;
      int votSegment = person.Household.GetVotALSegment();
      int transitAccess = residenceParcel.TransitAccessSegment();

      //GV: input 26. july 2013
      // household inputs
      //var childrenUnder5 = householdTotals.ChildrenUnder5;
      //var childrenAge5Through15 = householdTotals.ChildrenAge5Through15;
      //var nonworkingAdults = householdTotals.NonworkingAdults;
      //var retiredAdults = householdTotals.RetiredAdults;

      int onePersonHouseholdFlag = household.IsOnePersonHousehold.ToFlag();
      int twoPersonHouseholdFlag = household.IsTwoPersonHousehold.ToFlag();

      int householdCars = household.VehiclesAvailable;
      //var noCarsInHouseholdFlag = HouseholdWrapper.GetNoCarsInHouseholdFlag(householdCars);
      //var carsLessThanDriversFlag = household.GetCarsLessThanDriversFlag(householdCars);
      //var carsLessThanWorkersFlag = household.GetCarsLessThanWorkersFlag(householdCars);

      int HHwithChildrenFlag = household.HasChildren.ToFlag();
      int HHwithSmallChildrenFlag = household.HasChildrenUnder5.ToFlag();
      int HHwithLowIncomeFlag = (household.Income >= 300000 && household.Income < 600000).ToFlag();
      int HHwithMidleIncomeFlag = (household.Income >= 600000 && household.Income < 900000).ToFlag();
      int HHwithHighIncomeFlag = (household.Income >= 900000).ToFlag();

      int primaryFamilyTimeFlag = householdDay.PrimaryPriorityTimeFlag;

      //GV: input 26. july 2013
      // person inputs
      int partTimeWorkerFlag = person.IsPartTimeWorker.ToFlag();
      int nonworkingAdultFlag = person.IsNonworkingAdult.ToFlag();
      int universityStudentFlag = person.IsUniversityStudent.ToFlag();
      int retiredAdultFlag = person.IsRetiredAdult.ToFlag();
      int fullTimeWorkerFlag = person.IsFulltimeWorker.ToFlag();
      int childAge5Through15Flag = person.IsChildAge5Through15.ToFlag();
      int childUnder5Flag = person.IsChildUnder5.ToFlag();
      int adultFlag = person.IsAdult.ToFlag();

      int maleFlag = person.IsMale.ToFlag();
      int femaleFlag = person.IsFemale.ToFlag();



      if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId) {
        purposeLogsums[Global.Settings.Purposes.Work] = 0;
      } else {
        int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
        int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, Global.Settings.Purposes.Work);

        purposeLogsums[Global.Settings.Purposes.Work] = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        atUsualLogsums[Global.Settings.Purposes.Work] = Global.AggregateLogsums[person.UsualWorkParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][person.UsualWorkParcel.TransitAccessSegment()];
      }

      if (person.UsualSchoolParcel == null || person.UsualSchoolParcelId == household.ResidenceParcelId) {
        purposeLogsums[Global.Settings.Purposes.School] = 0;
      } else {
        int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
        int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(personDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, Global.Settings.Purposes.School);

        purposeLogsums[Global.Settings.Purposes.School] = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        atUsualLogsums[Global.Settings.Purposes.School] = Global.AggregateLogsums[person.UsualSchoolParcel.ZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][person.UsualSchoolParcel.TransitAccessSegment()];
      }

      double compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votSegment][transitAccess];

      purposeLogsums[Global.Settings.Purposes.Escort] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Escort][carOwnership][votSegment][transitAccess];
      purposeLogsums[Global.Settings.Purposes.PersonalBusiness] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.PersonalBusiness][carOwnership][votSegment][transitAccess];
      purposeLogsums[Global.Settings.Purposes.Shopping] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Shopping][carOwnership][votSegment][transitAccess];
      purposeLogsums[Global.Settings.Purposes.Social] = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.Social][carOwnership][votSegment][transitAccess];

      for (int xPurpose = Global.Settings.Purposes.Escort; xPurpose <= Global.Settings.Purposes.Social + 10; xPurpose++) {
        // extra components 1-5 are for 2,3,4,5,6 tour purposes
        // extra components 6-10 are for 2,3,4,5,6 stop puroposes

        // recode purpose to match coefficients
        int purpose = xPurpose <= Global.Settings.Purposes.Social ? xPurpose :
                    xPurpose <= Global.Settings.Purposes.Social + 5 ? Global.Settings.Purposes.Social + 1 :
                    Global.Settings.Purposes.Social + 2;

        // get correct multiplier on coefficients.
        double xMultiplier = xPurpose <= Global.Settings.Purposes.Social ? 1.0 :
                    xPurpose <= Global.Settings.Purposes.Social + 5 ? Math.Log(xPurpose - Global.Settings.Purposes.Social + 1) :
                    Math.Log(xPurpose - Global.Settings.Purposes.Social - 5 + 1);

        choiceProbabilityCalculator.CreateUtilityComponent(xPurpose);
        ChoiceProbabilityCalculator.Component component = choiceProbabilityCalculator.GetUtilityComponent(xPurpose);

        component.AddUtilityTerm(100 * purpose + 1, xMultiplier * person.IsFulltimeWorker.ToFlag());
        component.AddUtilityTerm(100 * purpose + 2, xMultiplier * person.IsPartTimeWorker.ToFlag());
        component.AddUtilityTerm(100 * purpose + 3, xMultiplier * person.IsRetiredAdult.ToFlag());
        component.AddUtilityTerm(100 * purpose + 4, xMultiplier * person.IsNonworkingAdult.ToFlag());
        component.AddUtilityTerm(100 * purpose + 5, xMultiplier * person.IsUniversityStudent.ToFlag());
        component.AddUtilityTerm(100 * purpose + 6, xMultiplier * person.IsDrivingAgeStudent.ToFlag());
        component.AddUtilityTerm(100 * purpose + 7, xMultiplier * person.IsChildAge5Through15.ToFlag());
        component.AddUtilityTerm(100 * purpose + 8, xMultiplier * person.IsChildUnder5.ToFlag());

        component.AddUtilityTerm(100 * purpose + 9, xMultiplier * HHwithLowIncomeFlag);
        component.AddUtilityTerm(100 * purpose + 10, xMultiplier * HHwithMidleIncomeFlag);
        component.AddUtilityTerm(100 * purpose + 11, xMultiplier * HHwithHighIncomeFlag);

        //component.AddUtilityTerm(100 * purpose + 12, xMultiplier * carsPerDriver);
        component.AddUtilityTerm(100 * purpose + 12, xMultiplier * householdCars);

        component.AddUtilityTerm(100 * purpose + 13, xMultiplier * person.IsOnlyAdult().ToFlag());
        component.AddUtilityTerm(100 * purpose + 14, xMultiplier * person.IsOnlyFullOrPartTimeWorker().ToFlag());
        component.AddUtilityTerm(100 * purpose + 15, xMultiplier * 0);
        component.AddUtilityTerm(100 * purpose + 16, xMultiplier * person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * (!household.HasChildrenUnder16).ToFlag());
        component.AddUtilityTerm(100 * purpose + 17, xMultiplier * person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
        component.AddUtilityTerm(100 * purpose + 18, xMultiplier * person.IsFemale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());
        component.AddUtilityTerm(100 * purpose + 19, xMultiplier * person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenUnder5.ToFlag());
        component.AddUtilityTerm(100 * purpose + 20, xMultiplier * person.IsMale.ToFlag() * person.IsAdult.ToFlag() * household.HasChildrenAge5Through15.ToFlag());

        //component.AddUtilityTerm(100 * purpose + 21, xMultiplier * primaryFamilyTimeFlag); //GV: wrong sign

        //component.AddUtilityTerm(100 * purpose + 21, xMultiplier * person.AgeIsBetween18And25.ToFlag());
        //component.AddUtilityTerm(100 * purpose + 22, xMultiplier * person.AgeIsBetween26And35.ToFlag());
        //component.AddUtilityTerm(100 * purpose + 23, xMultiplier * person.AgeIsBetween51And65.ToFlag());

        component.AddUtilityTerm(100 * purpose + 24, xMultiplier * person.WorksAtHome.ToFlag());
        component.AddUtilityTerm(100 * purpose + 25, xMultiplier * mixedDensity);
        component.AddUtilityTerm(100 * purpose + 26, xMultiplier * intersectionDensity);
        //component.AddUtilityTerm(100 * purpose + 27, xMultiplier * purposeLogsums[purpose]); //GV: 17.08.2013, the logsums are wrong
        //component.AddUtilityTerm(100 * purpose + 28, xMultiplier * person.TransitPassOwnershipFlag);
      }

      // tour utility
      const int tourComponentIndex = 18;
      choiceProbabilityCalculator.CreateUtilityComponent(tourComponentIndex);
      ChoiceProbabilityCalculator.Component tourComponent = choiceProbabilityCalculator.GetUtilityComponent(tourComponentIndex);
      //tourComponent.AddUtilityTerm(1701, carsPerDriver);
      tourComponent.AddUtilityTerm(1701, householdCars);

      tourComponent.AddUtilityTerm(1702, person.WorksAtHome.ToFlag());
      tourComponent.AddUtilityTerm(1703, mixedDensity);
      tourComponent.AddUtilityTerm(1704, mixedDensity * person.IsChildAge5Through15.ToFlag());
      tourComponent.AddUtilityTerm(1705, compositeLogsum);

      //tourComponent.AddUtilityTerm(1706, person.TransitPassOwnershipFlag);
      tourComponent.AddUtilityTerm(1706, primaryFamilyTimeFlag);

      // stop utility
      const int stopComponentIndex = 19;
      choiceProbabilityCalculator.CreateUtilityComponent(stopComponentIndex);
      ChoiceProbabilityCalculator.Component stopComponent = choiceProbabilityCalculator.GetUtilityComponent(stopComponentIndex);
      //stopComponent.AddUtilityTerm(1711, carsPerDriver);
      stopComponent.AddUtilityTerm(1711, householdCars);

      stopComponent.AddUtilityTerm(1712, person.WorksAtHome.ToFlag());
      stopComponent.AddUtilityTerm(1713, mixedDensity);
      stopComponent.AddUtilityTerm(1714, mixedDensity * person.IsChildAge5Through15.ToFlag());
      stopComponent.AddUtilityTerm(1715, compositeLogsum);

      //stopComponent.AddUtilityTerm(1716, person.TransitPassOwnershipFlag);
      //stopComponent.AddUtilityTerm(1716, primaryFamilyTimeFlag); //GV: 17.08.2013, the logsums are wrong

      for (int alternativeIndex = 0; alternativeIndex < TOTAL_ALTERNATIVES; alternativeIndex++) {

        DayPattern dayPattern = dayPatterns[alternativeIndex];
        bool available = dayPattern.Available;
        ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(alternativeIndex, available, choice != null && choice.Equals(dayPattern));

        if (!Global.Configuration.IsInEstimationMode && !alternative.Available) {
          continue;
        }

        alternative.Choice = dayPattern;

        // components for the purposes
        for (int purpose = Global.Settings.Purposes.Escort; purpose <= Global.Settings.Purposes.Social; purpose++) {
          if (dayPattern.Tours[purpose] > 0 || dayPattern.Stops[purpose] > 0) {
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(purpose));

            if (dayPattern.Tours[purpose] > 0) {
              alternative.AddUtilityTerm(100 * purpose + 50, 1); // tour purpose ASC
                                                                 //alternative.AddUtilityTerm(100 * purpose + 51, purposeLogsums[purpose]); // tour purpose logsum GV: 17.08.2013, the logsums are wrong
            }

            if (dayPattern.Stops[purpose] > 0) {
              alternative.AddUtilityTerm(100 * purpose + 60, 1); // stop purpose ASC
                                                                 //alternative.AddUtilityTerm(100 * purpose + 61, purposeLogsums[purpose]); // stop purpose logsum GV: 17.08.2013, the logsums are wrong
            }
            if (Global.Configuration.IsInEstimationMode) {
              //GV commented out
              //alternative.AddUtilityTerm(100 * purpose + 70, 1 - person.PaperDiary);
              //GV commented out
              //alternative.AddUtilityTerm(100 * purpose + 71, person.ProxyResponse);
            }
          }
        }

        // multiple tour purposes component
        if (dayPattern.TotalTourPurposes > 1) {
          alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(Global.Settings.Purposes.Social + (dayPattern.TotalTourPurposes - 1)));
        }

        // multiple stop purposes component
        if (dayPattern.TotalStopPurposes > 1) {
          alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(Global.Settings.Purposes.Social + 5 + (dayPattern.TotalStopPurposes - 1)));
        }

        for (int tourPurpose = Global.Settings.Purposes.Work; tourPurpose <= Global.Settings.Purposes.Social; tourPurpose++) {
          for (int stopPurpose = Global.Settings.Purposes.Work; stopPurpose <= Global.Settings.Purposes.Social - 1; stopPurpose++) {
            if (tourPurpose > Global.Settings.Purposes.School && stopPurpose <= Global.Settings.Purposes.School) {
              continue;
            }

            if (dayPattern.Tours[tourPurpose] > 0 && dayPattern.Stops[stopPurpose] > 0) {
              alternative.AddUtilityTerm(1200 + 10 * tourPurpose + stopPurpose, 1); // tour-stop comb. utility
            }
          }
        }

        for (int tourPurpose = Global.Settings.Purposes.Work; tourPurpose <= Global.Settings.Purposes.School; tourPurpose++) {
          if (dayPattern.Tours[tourPurpose] == 1 && dayPattern.TotalStopPurposes >= 1) {
            alternative.AddUtilityTerm(1300 + 10 * tourPurpose, purposeLogsums[tourPurpose]); // usual location logsum x presence of stops in work or school pattern
            alternative.AddUtilityTerm(1300 + 10 * tourPurpose + 1, compositeLogsum); // home aggregate logsum x  presence of stops in work or school pattern
                                                                                      //alternative.AddUtilityTerm(1300 + 10 * tourPurpose + 2, atUsualLogsums[tourPurpose]); // at usual location aggregate logsum x  presence of stops in work or school pattern GV: commented out as the sign is wrong
          }
        }

        for (int tourPurpose = Global.Settings.Purposes.Work; tourPurpose <= Global.Settings.Purposes.Social - 2; tourPurpose++) {
          for (int tourPurpose2 = tourPurpose + 1; tourPurpose2 <= Global.Settings.Purposes.Social; tourPurpose2++) {
            if (dayPattern.Tours[tourPurpose] > 0 && dayPattern.Tours[tourPurpose2] > 0) {
              alternative.AddUtilityTerm(1400 + 10 * tourPurpose + tourPurpose2, 1); // tour-tour comb. utility
            }
          }
        }

        for (int tourPurpose = Global.Settings.Purposes.Business; tourPurpose <= Global.Settings.Purposes.Business; tourPurpose++) {
          for (int tourPurpose2 = Global.Settings.Purposes.Escort; tourPurpose2 <= Global.Settings.Purposes.Social; tourPurpose2++) {
            if (dayPattern.Tours[tourPurpose] > 0 && dayPattern.Tours[tourPurpose2] > 0) {
              alternative.AddUtilityTerm(1461, 1); // tour-tour comb. utility for business combos
            }
          }
        }

        for (int stopPurpose = Global.Settings.Purposes.Work; stopPurpose <= Global.Settings.Purposes.Social - 2; stopPurpose++) {
          for (int stopPurpose2 = stopPurpose + 1; stopPurpose2 <= Global.Settings.Purposes.Social; stopPurpose2++) {
            if (dayPattern.Stops[stopPurpose] > 0 && dayPattern.Stops[stopPurpose2] > 0) {
              alternative.AddUtilityTerm(1500 + 10 * stopPurpose + stopPurpose2, 1); // stop-stop comb. utility
            }
          }
        }

        if (dayPattern.TotalTourPurposes > 0 && dayPattern.TotalStopPurposes > 0) {
          int totalStopPurposes = dayPattern.TotalStopPurposes;

          if (totalStopPurposes > 3) {
            totalStopPurposes = 3;
          }

          alternative.AddUtilityTerm(1600 + 10 * dayPattern.TotalTourPurposes + totalStopPurposes, 1); // nttour-ntstop utility
        }
        if (dayPattern.TotalTourPurposes - dayPattern.Tours[Global.Settings.Purposes.Work]
            - dayPattern.Tours[Global.Settings.Purposes.Business]
            - dayPattern.Tours[Global.Settings.Purposes.School] > 0) {
          // tour utility
          alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(tourComponentIndex));
        }
        if (dayPattern.TotalStopPurposes - dayPattern.Stops[Global.Settings.Purposes.Work]
            - dayPattern.Stops[Global.Settings.Purposes.Business]
            - dayPattern.Stops[Global.Settings.Purposes.School] > 0) {
          // stop utility
          alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(stopComponentIndex));
        }

      }
    }

    private sealed class DayPattern {
      private readonly int _hashCode;

      public DayPattern(int[] tours, int totalTourPurposes, int[] stops, int totalStopPurposes, bool available) {
        Tours = tours;

        WorkTours = tours[Global.Settings.Purposes.Work];
        SchoolTours = tours[Global.Settings.Purposes.School];
        BusinessTours = tours[Global.Settings.Purposes.Business];
        EscortTours = tours[Global.Settings.Purposes.Escort];
        PersonalBusinessTours = tours[Global.Settings.Purposes.PersonalBusiness];
        ShoppingTours = tours[Global.Settings.Purposes.Shopping];
        SocialTours = tours[Global.Settings.Purposes.Social];

        TotalTourPurposes = totalTourPurposes;

        Stops = stops;

        WorkStops = stops[Global.Settings.Purposes.Work];
        SchoolStops = stops[Global.Settings.Purposes.School];
        BusinessStops = stops[Global.Settings.Purposes.Business];
        EscortStops = stops[Global.Settings.Purposes.Escort];
        PersonalBusinessStops = stops[Global.Settings.Purposes.PersonalBusiness];
        ShoppingStops = stops[Global.Settings.Purposes.Shopping];
        SocialStops = stops[Global.Settings.Purposes.Social];

        TotalStopPurposes = totalStopPurposes;

        Available = available;

        _hashCode = ComputeHashCode();
      }

      public DayPattern(PersonDayWrapper personDay) {
        Tours = new int[Global.Settings.Purposes.TotalPurposes];

        WorkTours = Tours[Global.Settings.Purposes.Work] = personDay.WorkTours > 0 ? 1 : 0;
        SchoolTours = Tours[Global.Settings.Purposes.School] = personDay.SchoolTours > 0 ? 1 : 0;
        BusinessTours = Tours[Global.Settings.Purposes.Business] = personDay.BusinessTours > 0 ? 1 : 0;
        EscortTours = Tours[Global.Settings.Purposes.Escort] = personDay.EscortTours > 0 ? 1 : 0;
        PersonalBusinessTours = Tours[Global.Settings.Purposes.PersonalBusiness] = personDay.PersonalBusinessTours > 0 ? 1 : 0;
        ShoppingTours = Tours[Global.Settings.Purposes.Shopping] = personDay.ShoppingTours > 0 ? 1 : 0;
        SocialTours = Tours[Global.Settings.Purposes.Social] = personDay.SocialTours > 0 ? 1 : 0;

        TotalTourPurposes = WorkTours + SchoolTours + BusinessTours + EscortTours + PersonalBusinessTours + ShoppingTours + SocialTours;

        Stops = new int[Global.Settings.Purposes.TotalPurposes];

        WorkStops = Stops[Global.Settings.Purposes.Work] = personDay.WorkStops > 0 ? 1 : 0;
        SchoolStops = Stops[Global.Settings.Purposes.School] = personDay.SchoolStops > 0 ? 1 : 0;
        BusinessStops = Stops[Global.Settings.Purposes.Business] = personDay.BusinessStops > 0 ? 1 : 0;
        EscortStops = Stops[Global.Settings.Purposes.Escort] = personDay.EscortStops > 0 ? 1 : 0;
        PersonalBusinessStops = Stops[Global.Settings.Purposes.PersonalBusiness] = personDay.PersonalBusinessStops > 0 ? 1 : 0;
        ShoppingStops = Stops[Global.Settings.Purposes.Shopping] = personDay.ShoppingStops > 0 ? 1 : 0;
        SocialStops = Stops[Global.Settings.Purposes.Social] = personDay.SocialStops > 0 ? 1 : 0;

        TotalStopPurposes = WorkStops + SchoolStops + BusinessStops + EscortStops + PersonalBusinessStops + ShoppingStops + SocialStops;

        Available = true;

        _hashCode = ComputeHashCode();
      }

      public int[] Tours { get; private set; }

      public int[] Stops { get; private set; }

      public int WorkTours { get; private set; }

      public int SchoolTours { get; private set; }

      public int BusinessTours { get; private set; }

      public int EscortTours { get; private set; }

      public int PersonalBusinessTours { get; private set; }

      public int ShoppingTours { get; private set; }

      public int SocialTours { get; private set; }

      public int TotalTourPurposes { get; private set; }

      public int WorkStops { get; private set; }

      public int SchoolStops { get; private set; }

      public int BusinessStops { get; private set; }

      public int EscortStops { get; private set; }

      public int PersonalBusinessStops { get; private set; }

      public int ShoppingStops { get; private set; }

      public int SocialStops { get; private set; }

      public int TotalStopPurposes { get; private set; }

      public bool Available { get; private set; }

      private int ComputeHashCode() {
        unchecked {
          int hashCode = (WorkTours > 0).ToFlag();

          hashCode = (hashCode * 397) ^ (SchoolTours > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (BusinessTours > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (EscortTours > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (PersonalBusinessTours > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (ShoppingTours > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (SocialTours > 0 ? 1 : 0);

          hashCode = (hashCode * 397) ^ (WorkStops > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (SchoolStops > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (BusinessStops > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (EscortStops > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (PersonalBusinessStops > 0 ? 1 : 0);
          hashCode = (hashCode * 397) ^ (ShoppingStops > 0 ? 1 : 0);
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

        int workToursFlag = (WorkTours > 0).ToFlag();
        int schoolToursFlag = (SchoolTours > 0).ToFlag();
        int businessToursFlag = (BusinessTours > 0).ToFlag();
        int escortToursFlag = (EscortTours > 0).ToFlag();
        int personalBusinessToursFlag = (PersonalBusinessTours > 0).ToFlag();
        int shoppingToursFlag = (ShoppingTours > 0).ToFlag();
        int socialToursFlag = (SocialTours > 0).ToFlag();

        int workTours2Flag = (other.WorkTours > 0).ToFlag();
        int schoolTours2Flag = (other.SchoolTours > 0).ToFlag();
        int businessTours2Flag = (other.BusinessTours > 0).ToFlag();
        int escortTours2Flag = (other.EscortTours > 0).ToFlag();
        int personalBusinessTours2Flag = (other.PersonalBusinessTours > 0).ToFlag();
        int shoppingTours2Flag = (other.ShoppingTours > 0).ToFlag();
        int socialTours2Flag = (other.SocialTours > 0).ToFlag();

        int workStopsFlag = (WorkStops > 0).ToFlag();
        int schoolStopsFlag = (SchoolStops > 0).ToFlag();
        int businessStopsFlag = (BusinessStops > 0).ToFlag();
        int escortStopsFlag = (EscortStops > 0).ToFlag();
        int personalBusinessStopsFlag = (PersonalBusinessStops > 0).ToFlag();
        int shoppingStopsFlag = (ShoppingStops > 0).ToFlag();
        int socialStopsFlag = (SocialStops > 0).ToFlag();

        int workStops2Flag = (other.WorkStops > 0).ToFlag();
        int schoolStops2Flag = (other.SchoolStops > 0).ToFlag();
        int businessStops2Flag = (other.BusinessStops > 0).ToFlag();
        int escortStops2Flag = (other.EscortStops > 0).ToFlag();
        int personalBusinessStops2Flag = (other.PersonalBusinessStops > 0).ToFlag();
        int shoppingStops2Flag = (other.ShoppingStops > 0).ToFlag();
        int socialStops2Flag = (other.SocialStops > 0).ToFlag();

        return
            workToursFlag == workTours2Flag &&
            schoolToursFlag == schoolTours2Flag &&
                  businessToursFlag == businessTours2Flag &&
                  escortToursFlag == escortTours2Flag &&
            personalBusinessToursFlag == personalBusinessTours2Flag &&
            shoppingToursFlag == shoppingTours2Flag &&
            socialToursFlag == socialTours2Flag &&
            workStopsFlag == workStops2Flag &&
            schoolStopsFlag == schoolStops2Flag &&
                  businessStopsFlag == businessStops2Flag &&
                  escortStopsFlag == escortStops2Flag &&
            personalBusinessStopsFlag == personalBusinessStops2Flag &&
            shoppingStopsFlag == shoppingStops2Flag &&
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
