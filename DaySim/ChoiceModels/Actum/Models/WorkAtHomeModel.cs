﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  


using System;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
  public class WorkAtHomeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumWorkAtHomeModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.WorkAtHomeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER); 
    }

    public void Run(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      if (personDay == null) {
        throw new ArgumentNullException("personDay");
      }

      personDay.Person.ResetRandom(904);

      if (Global.Configuration.IsInEstimationMode) {

        if (personDay.WorkAtHomeDuration >= 120 && personDay.Person.IsFullOrPartTimeWorker) { personDay.WorksAtHomeFlag = 1; } else {
          personDay.WorksAtHomeFlag = 0;
        }

        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode || !personDay.Person.IsFullOrPartTimeWorker) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(personDay.Person.Id * 10 + personDay.Day);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }

        RunModel(choiceProbabilityCalculator, personDay, householdDay, personDay.WorksAtHomeFlag);

        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, personDay, householdDay);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, personDay.Id, personDay.WorksAtHomeFlag);

        Global.Configuration.IsInEstimationMode = true;
      } else {

        int choice;

        if (!personDay.Person.IsFullOrPartTimeWorker) {
          choice = 0;
        } else {

          RunModel(choiceProbabilityCalculator, personDay, householdDay);

          ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(personDay.Household.RandomUtility);
          choice = (int)chosenAlternative.Choice;
        }
        personDay.WorksAtHomeFlag = choice;
        personDay.WorkAtHomeDuration = choice * 120; //default predicted duration for output
      }
    }

    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {

      IActumHouseholdWrapper household = (IActumHouseholdWrapper)householdDay.Household;
      IActumPersonWrapper person = (IActumPersonWrapper)personDay.Person;
      IActumParcelWrapper householdResidenceParcel = (IActumParcelWrapper)household.ResidenceParcel;
      IActumParcelWrapper personUsualWorkParcel = (IActumParcelWrapper)person.UsualWorkParcel;



      // set household characteristics here that don't depend on person characteristics
      bool available = (household.Size > 1);

      int hasAdultEducLevel12 = 0;
      //int allAdultEducLevel12 = 1;
      int youngestAge = 999;

      foreach (PersonWrapper person_x in household.Persons) {
        // set characteristics here that depend on person characteristics
        //        if (person.Age >= 18 && person.EducationLevel >= 12) {
        //          hasAdultEducLevel12 = 1;
        //        }
        //if (person.Age >= 18 && person.EducationLevel < 12) allAdultEducLevel12 = 0;
        if (person_x.Age < youngestAge) {
          youngestAge = person_x.Age;
        }
      }

      int numberSelfEmpl = 0;
      if (person.OccupationCode == 8) {
        numberSelfEmpl++;
      }


      double workTourLogsum;
      if (person.UsualWorkParcelId != Constants.DEFAULT_VALUE && person.UsualWorkParcelId != Global.Settings.OutOfRegionParcelId) {
        //JLB 201406
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(person, person.Household.ResidenceParcel, person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        //JLB 201602
        //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().RunNested(personDay, household.ResidenceParcel, person.UsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, personDay.Person.Household.HouseholdTotals.DrivingAgeMembers);
        ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, householdResidenceParcel, personUsualWorkParcel, Global.Settings.Times.EightAM, Global.Settings.Times.FivePM, household.HouseholdTotals.DrivingAgeMembers, Global.Settings.Purposes.Work);
        workTourLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
      } else {
        workTourLogsum = 0;
      }

      int atHomeDay = personDay.PatternType == 3 ? 1 : 0;
      int nonMandatoryTourDay = personDay.PatternType == 2 ? 1 : 0;
      int MandatoryTourDay = personDay.PatternType == 1 ? 1 : 0;



      int carOwnership =
                            household.VehiclesAvailable == 0
                                 ? Global.Settings.CarOwnerships.NoCars
                                 : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
                                      ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                      : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

      int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
      int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

      //int votALSegment = Global.Settings.VotALSegments.Medium;  // TODO:  calculate a VOT segment that depends on household income
      //GV: 01.4.2019 - getting values from MB's memo
      int votALSegment =
        (household.Income <= 450000)
                  ? Global.Settings.VotALSegments.Low
                  : (household.Income <= 900000)
                      ? Global.Settings.VotALSegments.Medium
                      : Global.Settings.VotALSegments.High;

      
      //int transitAccessSegment = householdResidenceParcel.TransitAccessSegment();
      //GV: 01.4.2019 - getting values from MB's memo
      //OBS - it has to be in km
      int transitAccessSegment =
         household.ResidenceParcel.GetDistanceToTransit() >= 0 && household.ResidenceParcel.GetDistanceToTransit() <= 0.4
            ? 0
            : household.ResidenceParcel.GetDistanceToTransit() > 0.4 && household.ResidenceParcel.GetDistanceToTransit() <= 1.6
                ? 1
                : 2;


      double personalBusinessAggregateLogsum = Global.AggregateLogsums[householdResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment]; 
      double shoppingAggregateLogsum = Global.AggregateLogsums[householdResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
      double mealAggregateLogsum = Global.AggregateLogsums[householdResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
      double socialAggregateLogsum = Global.AggregateLogsums[householdResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
      //var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];

      double compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];

      //var householdDay = (ActumHouseholdDayWrapper)tour.HouseholdDay;
      //var household = householdDay.Household;

      //bool hhLivesInCPHCity = false;
      //if (household.ResidenceParcel.LandUseCode == 101 || household.ResidenceParcel.LandUseCode == 147) {
      //  hhLivesInCPHCity = true;

      bool hhLivesInCPHCity = false;
      if (household.ResidenceParcel.LandUseCode == 101 || household.ResidenceParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }

      
      // 0 Person doesn't work at home more than specified number of minutes
      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
      alternative.Choice = 0;

      alternative.AddUtilityTerm(1, 0.0);

      // 1 Works at home
      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
      alternative.Choice = 1;

      alternative.AddUtilityTerm(1, 1.0);

      alternative.AddUtilityTerm(6, person.IsMale.ToFlag());
      //GV; 12. feb. 2019, Self Employed in the HH 
      alternative.AddUtilityTerm(7, (person.OccupationCode == 8).ToFlag());
      //alternative.AddUtilityTerm(8, person.IsPartTimeWorker.ToFlag());

      //alternative.AddUtilityTerm(9, (household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(10, (household.HasChildren).ToFlag());

      //GV: 13, feb 2019 - HH size
      //alternative.AddUtilityTerm(30, (household.Size == 2 && numberAdult == 2).ToFlag()); //GV: HH==2 plus boh are adults
      //alternative.AddUtilityTerm(11, (household.Size == 2).ToFlag());
      //alternative.AddUtilityTerm(12, (household.Size == 3).ToFlag()); //GV; 16. april 2013, not significant
      //alternative.AddUtilityTerm(13, (household.Size >= 4).ToFlag()); //GV; 16. april 2013, not significant

      //GV: 1.4.2019
      //alternative.AddUtilityTerm(8, (household.VehiclesAvailable >= 1).ToFlag());
      //alternative.AddUtilityTerm(28, (household.VehiclesAvailable >= 2).ToFlag());
      //alternative.AddUtilityTerm(31, (household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
      //alternative.AddUtilityTerm(14, (household.VehiclesAvailable == 0).ToFlag());

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(3, household.HasChildrenAge5Through15.ToFlag());

      //GV: 18. feb 2019 - "householdDay.AdultsInSharedHomeStay" cannot be used
      //alternative.AddUtilityTerm(4, (household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
      //alternative.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildrenUnder16).ToFlag());

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(6, (householdDay.AdultsInSharedHomeStay == 2 && household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
      //alternative.AddUtilityTerm(7, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());

      //alternative.AddUtilityTerm(15, (youngestAge >= 40).ToFlag());

      //GV: 1. 4. 2019 - non signif.
      //alternative.AddUtilityTerm(16, (household.Income >= 450000 && household.Income < 900000).ToFlag());
      //alternative.AddUtilityTerm(11, (household.Income >= 650000 && household.Income < 900000).ToFlag());
      //alternative.AddUtilityTerm(17, (household.Income >= 900000).ToFlag());

      alternative.AddUtilityTerm(18, householdDay.PrimaryPriorityTimeFlag); 

      //GV: not sign. - 13. june 2016
      //alternative.AddUtilityTerm(16, (household.Size == 2).ToFlag());
      //alternative.AddUtilityTerm(17, (household.Size == 3).ToFlag());
      //alternative.AddUtilityTerm(18, (household.Size >= 4).ToFlag());
      ////alternative.AddUtilityTerm(18, (household.Size >= 5).ToFlag());   
            
      //alternative.AddUtilityTerm(24, MandatoryTourDay);
      alternative.AddUtilityTerm(25, nonMandatoryTourDay);

      alternative.AddUtilityTerm(26, atHomeDay);

      //alternative.AddUtilityTerm(27, workTourLogsum);

      //GV: put in instead of compositeLogsum - 13. june 2016
      alternative.AddUtilityTerm(27, workTourLogsum * MandatoryTourDay);
      //: GV 8.4.2019 - comment from JB on 6.4.2019
      //alternative.AddUtilityTerm(28, workTourLogsum * nonMandatoryTourDay);
      alternative.AddUtilityTerm(27, workTourLogsum * nonMandatoryTourDay); 

      alternative.AddUtilityTerm(29, compositeLogsum);
      
      //GV: CPH logsum - 18. feb 2019 - not signif.
      //alternative.AddUtilityTerm(30, workTourLogsum * MandatoryTourDay * (hhLivesInCPHCity).ToFlag());
      //alternative.AddUtilityTerm(31, workTourLogsum * nonMandatoryTourDay * (hhLivesInCPHCity).ToFlag());
           
      //GV: CPH logsum - 18. feb. 2019 - not signif.
      //alternative.AddUtilityTerm(32, compositeLogsum * (hhLivesInCPHCity).ToFlag());

    }
  }
}
