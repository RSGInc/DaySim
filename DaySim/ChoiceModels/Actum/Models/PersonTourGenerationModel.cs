// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class PersonTourGenerationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumPersonTourGenerationModel";

    // Add one alternative for the stop choice; Change this hard code
    private const int TOTAL_ALTERNATIVES = 10;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 2;
    private const int MAX_PARAMETER = 200;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.PersonTourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public int Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int maxPurpose) {
      return Run(personDay, householdDay, maxPurpose, Global.Settings.Purposes.NoneOrHome);
    }

    public int Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int maxPurpose, int choice) {
      if (householdDay == null) {
        throw new ArgumentNullException("householdDay");
      }

      householdDay.ResetRandom(949 + personDay.GetTotalCreatedTours());

      if (Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return choice;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(((personDay.Person.Id * 10 + personDay.Day) * 397) ^ personDay.GetTotalCreatedTours());

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        RunModel(choiceProbabilityCalculator, personDay, householdDay, maxPurpose, choice);

        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, personDay, householdDay, maxPurpose);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, choice);

        Global.Configuration.IsInEstimationMode = true;
      } else {
        RunModel(choiceProbabilityCalculator, personDay, householdDay, maxPurpose);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
        choice = (int)chosenAlternative.Choice;
      }

      return choice;
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int maxPurpose, int choice = Constants.DEFAULT_VALUE) {

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

      Framework.DomainModels.Wrappers.IHouseholdWrapper household = householdDay.Household;
      Framework.DomainModels.Wrappers.IParcelWrapper residenceParcel = household.ResidenceParcel;

      int carOwnership =
                            household.VehiclesAvailable == 0
                                 ? Global.Settings.CarOwnerships.NoCars
                                 : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
                                      ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                      : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

      int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
      int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

      int votALSegment = household.GetVotALSegment();
      int transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
      double personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
      double shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Shopping][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];
      //var mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
      //	 [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
      double socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
      double totalAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                [Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
      // var recreationAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
      // [Global.Settings.Purposes.Recreation][carOwnership][votALSegment][transitAccessSegment];
      //  var medicalAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
      //  [Global.Settings.Purposes.Medical][carOwnership][votALSegment][transitAccessSegment];
      //var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
      double compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];


      int countNonMandatory = 0;
      int countMandatory = 0;
      int countWorkingAtHome = 0;


      int[] mandPerstype = new int[8];
      int[] nonMandPerstype = new int[8];

      double[] workLogsum = new double[8];
      double[] schoolLogsum = new double[8];
      int count = 0;
      foreach (PersonDayWrapper pDay in orderedPersonDays) {
        Framework.DomainModels.Wrappers.IPersonWrapper person = pDay.Person;
        count++;
        if (count > 8) {
          break;
        }
        if (pDay.WorksAtHomeFlag == 1) {
          countWorkingAtHome++;
        }
        if (pDay.PatternType == 1) {
          countMandatory++;
          mandPerstype[pDay.Person.PersonType - 1]++;
        }
        if (pDay.PatternType == 2) {
          countNonMandatory++;
          nonMandPerstype[pDay.Person.PersonType - 1]++;
        }

        if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId) {
          workLogsum[count - 1] = 0;
        } else {
          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
          //JLB 201406
          //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(pDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
          //JLB 201602
          //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(pDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(pDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, Global.Settings.Purposes.Work);

          workLogsum[count - 1] = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }

        if (person.UsualSchoolParcel == null || person.UsualSchoolParcelId == household.ResidenceParcelId) {
          schoolLogsum[count - 1] = 0;
        } else {
          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);
          //JLB 201406
          //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(pDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
          //JLB 201602
          //var nestedAlternative = Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().RunNested(pDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(pDay, residenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, Global.Settings.Purposes.School);

          schoolLogsum[count - 1] = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }
      }


      // NONE_OR_HOME

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

      alternative.Choice = Global.Settings.Purposes.NoneOrHome;

      //alternative.AddUtilityTerm(1, (personDay.TotalCreatedTours == 1).ToFlag());
      alternative.AddUtilityTerm(2, (personDay.GetTotalCreatedTours() == 2).ToFlag());
      alternative.AddUtilityTerm(3, (personDay.GetTotalCreatedTours() >= 3).ToFlag());
      //alternative.AddUtilityTerm(4, (personDay.TotalCreatedTours >= 4).ToFlag());

      //alternative.AddUtilityTerm(5, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(6, householdDay.Household.HasChildren.ToFlag());

      alternative.AddUtilityTerm(4, householdDay.Household.HasChildrenUnder5.ToFlag());
      alternative.AddUtilityTerm(5, householdDay.Household.HasChildrenAge5Through15.ToFlag());
      //alternative.AddUtilityTerm(6, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(7, (householdDay.AdultsInSharedHomeStay == 1 && householdDay.Household.HasChildrenUnder16).ToFlag());
      //alternative.AddUtilityTerm(8, (householdDay.AdultsInSharedHomeStay == 2 && householdDay.Household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());

      //alternative.AddUtilityTerm(10, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
      //alternative.AddUtilityTerm(11, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
      //alternative.AddUtilityTerm(12, (householdDay.Household.Income >= 900000).ToFlag());

      alternative.AddUtilityTerm(13, householdDay.PrimaryPriorityTimeFlag);

      alternative.AddUtilityTerm(14, personDay.Person.IsPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(15, personDay.Person.WorksAtHome().ToFlag());
      //alternative.AddUtilityTerm(16, personDay.Person.IsFulltimeWorker.ToFlag());

      //alternative.AddUtilityTerm(15, (personDay.Person.Gender == 1).ToFlag());

      //alternative.AddUtilityTerm(10, (householdDay.Household.Size == 3).ToFlag());
      //alternative.AddUtilityTerm(11, (householdDay.Household.Size == 4).ToFlag());
      //alternative.AddUtilityTerm(12, (householdDay.Household.Size >= 5).ToFlag());

      //alternative.AddNestedAlternative(11, 0, 200);


      // WORK
      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Work, false, choice == Global.Settings.Purposes.Work);
      alternative.Choice = Global.Settings.Purposes.Work;
      alternative.AddUtilityTerm(202, 1);
      //alternative.AddNestedAlternative(12, 1, 200);

      //  SCHOOL
      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.School, false, choice == Global.Settings.Purposes.School);
      alternative.Choice = Global.Settings.Purposes.School;
      alternative.AddUtilityTerm(203, 1);
      //alternative.AddNestedAlternative(12, 1, 200);

      // ESCORT
      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Escort, maxPurpose <= Global.Settings.Purposes.Escort && personDay.CreatedEscortTours > 0, choice == Global.Settings.Purposes.Escort);
      alternative.Choice = Global.Settings.Purposes.Escort;

      alternative.AddUtilityTerm(151, 1);
      alternative.AddUtilityTerm(152, (personDay.CreatedEscortTours > 1).ToFlag());

      //alternative.AddUtilityTerm(152, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(153, (householdDay.Household.Size == 3).ToFlag()); 
      //alternative.AddUtilityTerm(154, (householdDay.Household.Size >= 4).ToFlag());

      //alternative.AddUtilityTerm(155, (householdDay.Household.Size > 4).ToFlag());

      alternative.AddUtilityTerm(155, compositeLogsum);

      //alternative.AddUtilityTerm(156, (householdDay.Household.VehiclesAvailable == 0).ToFlag());

      //alternative.AddNestedAlternative(12, 1, 200);


      // PERSONAL_BUSINESS
      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.PersonalBusiness, maxPurpose <= Global.Settings.Purposes.PersonalBusiness && personDay.CreatedPersonalBusinessTours > 0, choice == Global.Settings.Purposes.PersonalBusiness);
      alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

      alternative.AddUtilityTerm(21, 1);
      //alternative.AddUtilityTerm(22, (personDay.CreatedPersonalBusinessTours > 1).ToFlag()); //GV: 30. april 2013 - goes to infinity

      alternative.AddUtilityTerm(156, compositeLogsum);

      //alternative.AddNestedAlternative(12, 1, 200);

      // SHOPPING
      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Shopping, maxPurpose <= Global.Settings.Purposes.Shopping && personDay.CreatedShoppingTours > 0, choice == Global.Settings.Purposes.Shopping);
      alternative.Choice = Global.Settings.Purposes.Shopping;

      alternative.AddUtilityTerm(41, 1);
      //alternative.AddUtilityTerm(42, (personDay.CreatedShoppingTours > 1).ToFlag()); //GV: cannot be estimated

      //alternative.AddUtilityTerm(42, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(43, (householdDay.Household.Size == 3).ToFlag());
      //alternative.AddUtilityTerm(44, (householdDay.Household.Size == 4).ToFlag());
      //alternative.AddUtilityTerm(45, (householdDay.Household.Size > 4).ToFlag());

      //alternative.AddUtilityTerm(46, (householdDay.Household.VehiclesAvailable == 0).ToFlag());

      //alternative.AddUtilityTerm(157, compositeLogsum); //GV wrong sign
      //alternative.AddUtilityTerm(157, shoppingAggregateLogsum); //GV wrong sign

      //alternative.AddNestedAlternative(12, 1, 200);


      // MEAL

      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Meal, false, choice == Global.Settings.Purposes.Meal);
      alternative.Choice = Global.Settings.Purposes.Meal;

      alternative.AddUtilityTerm(61, 1);
      alternative.AddUtilityTerm(62, (personDay.CreatedMealTours > 1).ToFlag());

      //alternative.AddNestedAlternative(12, 1, 200);

      // SOCIAL

      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Social, maxPurpose <= Global.Settings.Purposes.Social && personDay.CreatedSocialTours > 0, choice == Global.Settings.Purposes.Social);
      alternative.Choice = Global.Settings.Purposes.Social;

      alternative.AddUtilityTerm(81, 1);
      alternative.AddUtilityTerm(82, (personDay.CreatedSocialTours > 1).ToFlag());

      //alternative.AddUtilityTerm(82, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(83, (householdDay.Household.Size == 3).ToFlag());
      //alternative.AddUtilityTerm(84, (householdDay.Household.Size == 4).ToFlag());
      //alternative.AddUtilityTerm(85, (householdDay.Household.Size > 4).ToFlag());

      alternative.AddUtilityTerm(158, compositeLogsum);

      //alternative.AddNestedAlternative(12, 1, 200);

      // RECREATION

      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Recreation, false, choice == Global.Settings.Purposes.Recreation);
      alternative.Choice = Global.Settings.Purposes.Recreation;

      alternative.AddUtilityTerm(101, 1);
      //alternative.AddUtilityTerm(102, (personDay.CreatedRecreationTours > 1).ToFlag());

      //alternative.AddNestedAlternative(12, 1, 60);

      // MEDICAL

      alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.Medical, false, choice == Global.Settings.Purposes.Medical);
      alternative.Choice = Global.Settings.Purposes.Medical;

      alternative.AddUtilityTerm(121, 1);
      //alternative.AddUtilityTerm(122, (personDay.CreatedMedicalTours > 1).ToFlag());

      //alternative.AddNestedAlternative(11, 1, 60);

    }
  }
}
