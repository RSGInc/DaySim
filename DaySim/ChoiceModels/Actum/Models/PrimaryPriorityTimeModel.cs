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
using DaySim.DomainModels.Actum.Wrappers.Interfaces; 


namespace DaySim.ChoiceModels.Actum.Models {
  public class PrimaryPriorityTimeModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumPrimaryPriorityTimeModel";
    private const int TOTAL_ALTERNATIVES = 4;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    //private const int THETA_PARAMETER = 99; 
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.ActumPrimaryPriorityTimeModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(HouseholdDayWrapper householdDay) {
      if (householdDay == null) {
        throw new ArgumentNullException("householdDay");
      }

      householdDay.ResetRandom(901);

      // IEnumerable<PersonWrapper> personTypeOrderedPersons = householdDay.Household.Persons.OrderBy(p=> p.PersonType).ToList();



      if (Global.Configuration.IsInEstimationMode) {

        if (householdDay.SharedActivityHomeStays >= 1
             //&& householdDay.DurationMinutesSharedHomeStay >=60 
             && householdDay.AdultsInSharedHomeStay >= 1
             && householdDay.NumberInLargestSharedHomeStay >= (householdDay.Household.Size)
             ) {
          householdDay.PrimaryPriorityTimeFlag = 1;
        } else {
          householdDay.PrimaryPriorityTimeFlag = 0;
        }
         
        if (householdDay.JointTours > 0) {
          householdDay.JointTourFlag = 1;
        } else {
          householdDay.JointTourFlag = 0;
        }

        if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(householdDay.Household.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

        //				// set choice variable here  (derive from available household properties)
        //				if (householdDay.SharedActivityHomeStays >= 1 
        //					//&& householdDay.DurationMinutesSharedHomeStay >=60 
        //					&& householdDay.AdultsInSharedHomeStay >= 1 
        //					&& householdDay.NumberInLargestSharedHomeStay >= (household.Size)
        //                   )
        //				{
        //					householdDay.PrimaryPriorityTimeFlag = 1;  
        //				}
        //				else 	householdDay.PrimaryPriorityTimeFlag = 0;

        int choice = householdDay.PrimaryPriorityTimeFlag + 2 * (householdDay.JointTours > 0 ? 1 : 0);

        RunModel(choiceProbabilityCalculator, householdDay, choice);

        choiceProbabilityCalculator.WriteObservation();
      } else if (Global.Configuration.TestEstimationModelInApplicationMode) {
        Global.Configuration.IsInEstimationMode = false;

        RunModel(choiceProbabilityCalculator, householdDay);

        //var observedChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);

        ChoiceProbabilityCalculator.Alternative simulatedChoice = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility, householdDay.Household.Id, householdDay.PrimaryPriorityTimeFlag);

        Global.Configuration.IsInEstimationMode = true;
      } else {
        RunModel(choiceProbabilityCalculator, householdDay);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        if (choice == 0) {
          householdDay.PrimaryPriorityTimeFlag = 0;
          householdDay.JointTourFlag = 0;
        } else if (choice == 1) {
          householdDay.PrimaryPriorityTimeFlag = 1;
          householdDay.JointTourFlag = 0;
        } else if (choice == 2) {
          householdDay.PrimaryPriorityTimeFlag = 0;
          householdDay.JointTourFlag = 1;
        } else { // if (choice == 3) {
          householdDay.PrimaryPriorityTimeFlag = 1;
          householdDay.JointTourFlag = 1;
        }

      }
    }
    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int choice = Constants.DEFAULT_VALUE) {

      //JLB 20190126 adjusted Peter's version
      //Framework.DomainModels.Wrappers.IHouseholdWrapper household = householdDay.Household;
      IActumHouseholdWrapper household = (IActumHouseholdWrapper) householdDay.Household;
      //Framework.DomainModels.Wrappers.IParcelWrapper residenceParcel = household.ResidenceParcel;
      IActumParcelWrapper residenceParcel = (IActumParcelWrapper) household.ResidenceParcel;

      // set household characteristics here that don't depend on person characteristics

      int hasAdultEducLevel12 = 0;
      int youngestAge = 999;


      double firstWorkLogsum = 0;
      double secondWorkLogsum = 0;
      bool firstWorkLogsumIsSet = false;
      bool secondWorkLogsumIsSet = false;
      int numberWorkers = 0;
      int numberAdults = 0;
      int numberChildren = 0;
      int numberChildrenUnder5 = 0;
      int numberSelfEmpl = 0;


      //bool hhLivesInCPHCity = false;
      //if (household.ResidenceParcel.LandUseCode == 101 || household.ResidenceParcel.LandUseCode == 147) {
      //  hhLivesInCPHCity = true;

      bool hhLivesInCPHCity = false;
      if (household.ResidenceParcel.LandUseCode == 101 || household.ResidenceParcel.LandUseCode == 147) {
        hhLivesInCPHCity = true;
      }

      // set characteristics here that depend on person characteristics
      foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
        double workLogsum = 0;
        PersonWrapper person = (PersonWrapper)personDay.Person;

        if (person.UsualWorkParcel == null || person.UsualWorkParcelId == household.ResidenceParcelId
            || (person.PersonType != Global.Settings.PersonTypes.FullTimeWorker
            && person.PersonType != Global.Settings.PersonTypes.PartTimeWorker))
        //	|| household.VehiclesAvailable == 0) 
        {
        } else {
          int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
          int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);
          //JLB 201406
          //var nestedAlternative = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);

          //JLB 201602
          //var nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable);
          ChoiceProbabilityCalculator.Alternative nestedAlternative = Global.ChoiceModelSession.Get<TourModeTimeModel>().RunNested(personDay, residenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.VehiclesAvailable, Global.Settings.Purposes.Work);
          workLogsum = nestedAlternative == null ? 0 : nestedAlternative.ComputeLogsum();
        }
        //if (person.Age >= 18 && person.EducationLevel >= 12) {
        //  hasAdultEducLevel12 = 1;
        //}

        //if (person.Age >= 18 && person.EducationLevel < 12) {
        //}

        if (person.Age < youngestAge) {
          youngestAge = person.Age;
        }

        if (workLogsum != 0 && !firstWorkLogsumIsSet) {
          firstWorkLogsum = workLogsum;
          firstWorkLogsumIsSet = true;
        } else if (workLogsum != 0 && !secondWorkLogsumIsSet) {
          secondWorkLogsum = workLogsum;
          secondWorkLogsumIsSet = true;
        }

        if (person.OccupationCode == 8) {    
             numberSelfEmpl++;
               } 

        if (person.Age >= 18) {
          numberAdults++;
          if (person.PersonType == Global.Settings.PersonTypes.FullTimeWorker
              //|| person.PersonType == Constants.PersonType.PART_TIME_WORKER
              ) {
            numberWorkers++;
          }
        } else {

 //         if (person.OccupationCode == 8) {    
 //             numberSelfEmpl++;
 //         } else {

            numberChildren++;
          if (person.PersonType == Global.Settings.PersonTypes.ChildUnder5) {
            numberChildrenUnder5++;
          }
//        }
        } 
        }
        bool singleWorkerWithChildUnder5 = (numberAdults == numberWorkers && numberAdults == 1
                && numberChildrenUnder5 > 0) ? true : false;
      bool workingCoupleNoChildren = (numberAdults == numberWorkers && numberAdults == 2
                && numberChildren == 0) ? true : false;
      bool workingCoupleAllChildrenUnder5 = (numberAdults == numberWorkers && numberAdults == 2
                && numberChildren > 0 && numberChildren == numberChildrenUnder5) ? true : false;
      bool otherHouseholdWithPTFTWorkers = false;
      if (!workingCoupleNoChildren && !workingCoupleAllChildrenUnder5 && firstWorkLogsum != 0) {
        otherHouseholdWithPTFTWorkers = true;
      }
      if (!workingCoupleNoChildren && !workingCoupleAllChildrenUnder5 && !otherHouseholdWithPTFTWorkers) {
      }


      // var votSegment = household.VotALSegment;
      //var taSegment = household.ResidenceParcel.TransitAccessSegment();

      //var aggregateLogsumDifference = // full car ownership vs. no car ownership
      //	Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment] -
      //	Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votSegment][taSegment];


      //var householdDay = (ActumHouseholdDayWrapper)tour.HouseholdDay;

      int carOwnership =
                            household.VehiclesAvailable == 0
                                 ? Global.Settings.CarOwnerships.NoCars
                                 : household.VehiclesAvailable < household.HouseholdTotals.DrivingAgeMembers
                                      ? Global.Settings.CarOwnerships.LtOneCarPerAdult
                                      : Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult;

      int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
      int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);

      int votALSegment = Global.Settings.VotALSegments.Medium;  // TODO:  calculate a VOT segment that depends on household income
      int transitAccessSegment = household.ResidenceParcel.TransitAccessSegment();
      double personalBusinessAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.PersonalBusiness][carOwnership][votALSegment][transitAccessSegment];
      double shoppingAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Shopping][carOwnership][votALSegment][transitAccessSegment];
      double mealAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Meal][carOwnership][votALSegment][transitAccessSegment];
      double socialAggregateLogsum = Global.AggregateLogsums[household.ResidenceParcel.ZoneId]
                 [Global.Settings.Purposes.Social][carOwnership][votALSegment][transitAccessSegment];
      //var compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][carOwnership][votALSegment][transitAccessSegment];
      double compositeLogsum = Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votALSegment][transitAccessSegment];

      int componentIndex = 0;
      for (int pfpt = 0; pfpt < 2; pfpt++) {
        if (pfpt == 1) {
          componentIndex = 1;
          choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
          ChoiceProbabilityCalculator.Component pfptComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);
          pfptComponent.AddUtilityTerm(1, (household.Size == 3).ToFlag());
          pfptComponent.AddUtilityTerm(2, (household.Size >= 4).ToFlag());

          pfptComponent.AddUtilityTerm(3, household.HasChildrenUnder5.ToFlag());
          pfptComponent.AddUtilityTerm(4, household.HasChildrenAge5Through15.ToFlag());

          //GV: JBsmail fromfeb. 6. 2019 explains that "AdultsInSharedHomeStay" is endogenous and cannot be used for estimaions
          //pfptComponent.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildrenUnder16).ToFlag());
          //pfptComponent.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildrenAge5Through15).ToFlag());
          //pfptComponent.AddUtilityTerm(5, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildrenUnder5).ToFlag());
          //pfptComponent.AddUtilityTerm(7, (householdDay.AdultsInSharedHomeStay == 2 && household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
          //pfptComponent.AddUtilityTerm(7, (household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
          pfptComponent.AddUtilityTerm(8, (household.Size == 2 && household.HouseholdTotals.FullAndPartTimeWorkers == 2).ToFlag());
          //pfptComponent.AddUtilityTerm(8, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());

          //pfptComponent.AddUtilityTerm(9, (household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
          pfptComponent.AddUtilityTerm(10, (household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag());
      
          //GV; 5. feb. 2019, Self Employed in the HH 
          pfptComponent.AddUtilityTerm(11, (numberSelfEmpl >= 1).ToFlag());
          pfptComponent.AddUtilityTerm(12, (household.PartTimeWorkers >= 1).ToFlag());
                   
          //GV; 4. feb. 2019, CPHcity constant
          pfptComponent.AddUtilityTerm(13, (hhLivesInCPHCity).ToFlag());

          //pfptComponent.AddUtilityTerm(15, (household.Income >= 300000 && household.Income < 600000).ToFlag());
          //pfptComponent.AddUtilityTerm(16, (household.Income >= 600000 && household.Income < 900000).ToFlag());
          //pfptComponent.AddUtilityTerm(17, (household.Income >= 900000).ToFlag());
          //pfptComponent.AddUtilityTerm(16, (household.Income >= 600000).ToFlag());

          // OBS; 27. aug., work tour mode logsum does not work - see what happens in the old PFPT model 
          // GV, sep. 1st - it is not significant                    
          pfptComponent.AddUtilityTerm(18, (firstWorkLogsum + secondWorkLogsum) *
               (workingCoupleNoChildren || workingCoupleAllChildrenUnder5).ToFlag());
          pfptComponent.AddUtilityTerm(18, (firstWorkLogsum + secondWorkLogsum) * otherHouseholdWithPTFTWorkers.ToFlag());

          // dette er gamle at-work logsum - it should be plus and significant
          //alternative.AddUtilityTerm(31, (firstWorkLogsum + secondWorkLogsum) *
          //(workingCoupleNoChildren || workingCoupleAllChildrenUnder5).ToFlag());
          //alternative.AddUtilityTerm(31, (firstWorkLogsum + secondWorkLogsum) * otherHouseholdWithPTFTWorkers.ToFlag());

          // at-home logsum works
          pfptComponent.AddUtilityTerm(19, compositeLogsum);

          //pfpt constant
          pfptComponent.AddUtilityTerm(51, 1);

        }
      }
      for (int jointTourFlag = 0; jointTourFlag < 2; jointTourFlag++) {
        if (jointTourFlag == 1) {
          componentIndex = 2;
          choiceProbabilityCalculator.CreateUtilityComponent(componentIndex);
          ChoiceProbabilityCalculator.Component jointComponent = choiceProbabilityCalculator.GetUtilityComponent(componentIndex);

          jointComponent.AddUtilityTerm(21, (household.Size == 3).ToFlag());
          jointComponent.AddUtilityTerm(22, (household.Size >= 4).ToFlag());

          // GV: 1st sep.
          //jointComponent.AddUtilityTerm(23, (household.Size == 2 && household.HasChildren).ToFlag());
          //jointComponent.AddUtilityTerm(23, (household.Size >= 2 && household.HasChildren).ToFlag()); 

          jointComponent.AddUtilityTerm(23, (household.HasChildren).ToFlag());
          //jointComponent.AddUtilityTerm(21, household.HasChildrenUnder5.ToFlag());
          //jointComponent.AddUtilityTerm(22, household.HasChildrenAge5Through15.ToFlag());

          //jointComponent.AddUtilityTerm(24, (household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());
          jointComponent.AddUtilityTerm(24, (household.Size == 2 && household.HouseholdTotals.FullAndPartTimeWorkers == 2).ToFlag());
          //jointComponent.AddUtilityTerm(25, (householdDay.AdultsInSharedHomeStay == 2 && household.HouseholdTotals.FullAndPartTimeWorkers >= 2).ToFlag());
          //jointComponent.AddUtilityTerm(25, (householdDay.AdultsInSharedHomeStay == 2 && hasAdultEducLevel12 == 1).ToFlag());

          //jointComponent.AddUtilityTerm(26, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildrenAge5Through15).ToFlag());

          //jointComponent.AddUtilityTerm(27, (household.Size == 2 && household.HasChildrenUnder5).ToFlag());
          //jointComponent.AddUtilityTerm(28, (household.Size == 2 && household.HasChildrenAge5Through15).ToFlag());
          //jointComponent.AddUtilityTerm(27, (household.Size == 2 && household.HasChildrenUnder16).ToFlag());

          //jointComponent.AddUtilityTerm(26, (householdDay.AdultsInSharedHomeStay == 1 && household.HasChildrenUnder16).ToFlag());

          //jointComponent.AddUtilityTerm(37, (household.VehiclesAvailable == 1 && household.Has2Drivers).ToFlag());
          //jointComponent.AddUtilityTerm(38, (household.VehiclesAvailable >= 2 && household.Has2Drivers).ToFlag()); 

          //GV; 5. feb. 2019, Self Employed in the HH 
          jointComponent.AddUtilityTerm(25, (numberSelfEmpl >= 1).ToFlag());
          jointComponent.AddUtilityTerm(26, (household.PartTimeWorkers >= 1).ToFlag());
                           
          //GV; 4. feb. 2019, only HH car availability
          //jointComponent.AddUtilityTerm(30, (household.VehiclesAvailable >= 1 && household.Has2Drivers).ToFlag());
          jointComponent.AddUtilityTerm(27, (household.VehiclesAvailable >= 1).ToFlag());

          //jointComponent.AddUtilityTerm(28, (household.PartTimeWorkers >= 1).ToFlag());

          //GV; 4. feb. 2019, CPHcity constant
          //jointComponent.AddUtilityTerm(29, (hhLivesInCPHCity).ToFlag());

          jointComponent.AddUtilityTerm(31, (household.Income >= 300000 && household.Income < 600000).ToFlag());
          //jointComponent.AddUtilityTerm(32, (household.Income >= 600000 && household.Income < 900000).ToFlag());
          //jointComponent.AddUtilityTerm(33, (household.Income >= 900000).ToFlag());
          jointComponent.AddUtilityTerm(32, (household.Income >= 600000).ToFlag());

          // GV, sep. 1st - it is not significant 
          //jointComponent.AddUtilityTerm(41, compositeLogsum);

          // joint non-mandatory tour constant
          jointComponent.AddUtilityTerm(61, 1); 

        }
      }

      bool available = true;
      for (int pfpt = 0; pfpt < 2; pfpt++) {
        for (int jointTourFlag = 0; jointTourFlag < 2; jointTourFlag++) {

          int altIndex = pfpt + jointTourFlag * 2;
          if (household.Size < 2 && altIndex > 0) {
            available = false;
          } else {
            available = true;
          }
          ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(altIndex, available, choice == altIndex);

          alternative.Choice = altIndex;

          //NESTING WAS REJECTED BY TESTS 
          //GV: PFPT on top - cannot be estimated
          //alternative.AddNestedAlternative(5 + pfpt,          pfpt, THETA_PARAMETER); 
          //alternative.AddNestedAlternative(5 + jointTourFlag, jointTourFlag, THETA_PARAMETER); //jointTourFlag on top

          if (pfpt == 1) {
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(1));
            //alternative.AddUtilityTerm(20, 1);

          }
          if (jointTourFlag == 1) {
            alternative.AddUtilityComponent(choiceProbabilityCalculator.GetUtilityComponent(2));
            //alternative.AddUtilityTerm(40, 1);
          }

          if (pfpt == 1 && jointTourFlag == 1) {
            alternative.AddUtilityTerm(71, 1);
            //alternative.AddUtilityTerm(72, (household.Size == 2 && householdDay.AdultsInSharedHomeStay == 2).ToFlag());

            // GV: comented out sep. 1st    
            //GV: coeff. 73 is with a wrong sign - 13. june 2016
            //alternative.AddUtilityTerm(73, household.HasChildren.ToFlag());
            //alternative.AddUtilityTerm(74, household.HasChildrenUnder16.ToFlag());
          }

        }
      }



    }
  }
}
