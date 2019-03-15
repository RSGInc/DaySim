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
  public class WorkBasedSubtourGenerationModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumWorkBasedSubtourGenerationModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 50;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkBasedSubtourGenerationModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public int Run(TourWrapper tour, HouseholdDayWrapper householdDay, int nCallsForTour) {
      return Run(tour, householdDay, nCallsForTour, Global.Settings.Purposes.NoneOrHome);
    }

    public int Run(TourWrapper tour, HouseholdDayWrapper householdDay, int nCallsForTour, int choice) {
      if (tour == null) {
        throw new ArgumentNullException("tour");
      }

      tour.PersonDay.Person.ResetRandom(908 + (tour.Sequence - 1) * 3 + nCallsForTour);

      if (Global.Configuration.IsInEstimationMode) {
        if (!(choice == Global.Settings.Purposes.NoneOrHome)) {  // simplifying choice.  TODO:  will need to deal with personal business subtours distinctly from home based in tour models
          choice = Global.Settings.Purposes.PersonalBusiness;
        }
        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return choice;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator((tour.Id * 397) ^ nCallsForTour);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (tour.PersonDay.GetTotalStops() > 0) {
          RunModel(choiceProbabilityCalculator, tour, householdDay, nCallsForTour, choice);

          choiceProbabilityCalculator.WriteObservation();
        }
      } else {
        if (tour.PersonDay.GetTotalStops() > 0) {
          RunModel(choiceProbabilityCalculator, tour, householdDay, nCallsForTour);

          ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(tour.Household.RandomUtility);
          choice = (int)chosenAlternative.Choice;
        } else {
          choice = Global.Settings.Purposes.NoneOrHome;
        }
      }

      return choice;
    }

  private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, TourWrapper tour, HouseholdDayWrapper householdDay, int nCallsForTour, int choice = Constants.DEFAULT_VALUE) {
      PersonWrapper person = (PersonWrapper)tour.Person;
      PersonDayWrapper personDay = (PersonDayWrapper)tour.PersonDay;
      ParcelWrapper tourDestinationParcel = (ParcelWrapper)tour.DestinationParcel;

      //			var foodRetailServiceMedicalQtrMileLog = tourDestinationParcel.FoodRetailServiceMedicalQtrMileLogBuffer1();
      //			var mixedUseIndex = tourDestinationParcel.MixedUse4Index1();
      double k8HighSchoolQtrMileLog = tourDestinationParcel.K8HighSchoolQtrMileLogBuffer1();
      //var carOwnership = person.CarOwnershipSegment;
      int carOwnership = 0;

      int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
      int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);
      //			var notUsualWorkParcelFlag = tourDestinationParcel.NotUsualWorkParcelFlag(person.UsualWorkParcelId);

      int votALSegment = tour.GetVotALSegment();

      int workTaSegment = tourDestinationParcel.TransitAccessSegment();
      double workAggregateLogsum = Global.AggregateLogsums[tourDestinationParcel.ZoneId][Global.Settings.Purposes.WorkBased][carOwnership][votALSegment][workTaSegment];

      //var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
      //var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];


      //GV: 25.feb. 2019 - self employed person
      int SelfEmpFlag = (person.OccupationCode == 8).ToFlag();

      bool hhLivesInCPHCity = false;
      if (householdDay.Household.ResidenceParcel.LandUseCode == 101 || householdDay.Household.ResidenceParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }


      // NONE_OR_HOME

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(Global.Settings.Purposes.NoneOrHome, true, choice == Global.Settings.Purposes.NoneOrHome);

      alternative.Choice = Global.Settings.Purposes.NoneOrHome;

      alternative.AddUtilityTerm(1, (nCallsForTour > 1).ToFlag());

      alternative.AddUtilityTerm(2, Math.Log(personDay.HomeBasedTours));  

      //alternative.AddUtilityTerm(3, personDay.HasTwoOrMoreWorkTours.ToFlag());
      //alternative.AddUtility(4, notUsualWorkParcelFlag);

      //alternative.AddUtilityTerm(5, noCarsFlag);
      //alternative.AddUtilityTerm(6, carCompetitionFlag);

      //alternative.AddUtilityTerm(7, householdDay.PrimaryPriorityTimeFlag);

      //alternative.AddUtilityTerm(9, householdDay.Household.HasChildrenUnder5.ToFlag());
      //alternative.AddUtilityTerm(10, householdDay.Household.HasChildrenAge5Through15.ToFlag());

      //alternative.AddUtilityTerm(11, (householdDay.Household.Size == 2).ToFlag());
      //alternative.AddUtilityTerm(12, (householdDay.Household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());

      //alternative.AddUtilityTerm(14, person.WorksAtHome.ToFlag());
      //alternative.AddUtilityTerm(15, person.IsNonworkingAdult.ToFlag());

      //GV: 26. feb. 2019 
      //alternative.AddUtilityTerm(16, SelfEmpFlag);
      
      //alternative.AddUtilityTerm(15, (person.IsNonworkingAdult).ToFlag()); //out of scope, non available

      //alternative.AddUtilityTerm(41, workAggregateLogsum); 
      //alternative.AddUtility(32, mixedUseIndex); 

      // WORK-BASED

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == Global.Settings.Purposes.PersonalBusiness);

      alternative.Choice = Global.Settings.Purposes.PersonalBusiness;

      alternative.AddUtilityTerm(21, 1);

      //alternative.AddUtilityTerm(22, (person.IsWorker).ToFlag());

      //GV: 27. feb 2019 - OBS Wrong sign
      alternative.AddUtilityTerm(22, (householdDay.Household.VehiclesAvailable >= 1).ToFlag());

      //alternative.AddUtilityTerm(22, (person.FlexibleWorkHours == 1).ToFlag());
      //alternative.AddUtilityTerm(23, (person.EducationLevel >= 12).ToFlag());

      //alternative.AddUtilityTerm(22, person.IsFulltimeWorker.ToFlag());
      //alternative.AddUtilityTerm(23, person.IsPartTimeWorker.ToFlag());

      //GV, 16. june 2016 - cannot be estimated
      //alternative.AddUtilityTerm(24, (person.WorksAtHome).ToFlag());
      ////alternative.AddUtilityTerm(25, person.IsFulltimeWorker.ToFlag());
      //alternative.AddUtilityTerm(26, (person.MainOccupation == 50).ToFlag()); // self employed

      alternative.AddUtilityTerm(27, (person.Gender == 1).ToFlag());
      //alternative.AddUtilityTerm(44, (hasAdultEducLevel12 == 1).ToFlag());

      //GV. 16. june 2016 - not signif.
      //alternative.AddUtilityTerm(28, (householdDay.Household.VehiclesAvailable == 1 && householdDay.Household.Has2Drivers).ToFlag());
      //alternative.AddUtilityTerm(29, (householdDay.Household.VehiclesAvailable >= 2 && householdDay.Household.Has2Drivers).ToFlag());

      alternative.AddUtilityTerm(30, householdDay.PrimaryPriorityTimeFlag);

      //GV. 16. june 2016 - not signif.
      //alternative.AddUtilityTerm(31, (householdDay.Household.Income >= 300000 && householdDay.Household.Income < 600000).ToFlag());
      //alternative.AddUtilityTerm(32, (householdDay.Household.Income >= 600000 && householdDay.Household.Income < 900000).ToFlag());
      //alternative.AddUtilityTerm(33, (householdDay.Household.Income >= 900000).ToFlag());

      alternative.AddUtilityTerm(42, workAggregateLogsum);
      //alternative.AddUtilityTerm(43, workAggregateLogsum * (hhLivesInCPHCity).ToFlag()); //GV: 27. feb 2019 - not sign.
      
      //alternative.AddUtilityTerm(36, (householdDay.Household.Size == 2).ToFlag()); 
      //alternative.AddUtilityTerm(37, (householdDay.Household.Size == 3).ToFlag());
      //alternative.AddUtilityTerm(38, (householdDay.Household.Size >= 4).ToFlag());


    }
  }
}
