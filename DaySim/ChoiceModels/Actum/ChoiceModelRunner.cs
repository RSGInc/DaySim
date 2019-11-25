// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.ChoiceModels.Actum.Models;
using DaySim.ChoiceModels.H;
//using DaySim.ChoiceModels.Default.Models;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;


namespace DaySim.ChoiceModels.Actum {
  [UsedImplicitly]
  [Factory(Factory.ChoiceModelFactory, ChoiceModelRunner = Framework.Factories.ChoiceModelRunner.Actum)]
  public sealed class ChoiceModelRunner : IChoiceModelRunner {
    private readonly HouseholdWrapper _household;

    public ChoiceModelRunner(IHousehold household) {

      _household =
          (HouseholdWrapper)Global
              .ContainerDaySim.GetInstance<IWrapperFactory<IHouseholdCreator>>()
              .Creator
              .CreateWrapper(household);
    }

    public void SetRandomSeed(int randomSeed) {
      _household.RandomUtility.ResetHouseholdSynchronization(randomSeed);
      _household.RandomUtility.ResetUniform01(randomSeed);
      _household.Init();
    }

    public void RunChoiceModels() {
      RunHouseholdModels();
      RunHouseholdDayModels();

      UpdateHousehold();

      if (ChoiceModelFactory.ThreadQueue != null) {
        ChoiceModelFactory.ThreadQueue.Add(this);
      }
    }

    private void RunHouseholdModels() {
      if (!Global.Configuration.ShouldRunHouseholdModels) {
        return;
      }

#if RELEASE
      try {
#endif

        ChoiceModelFactory.TotalTimesHouseholdModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        RunHouseholdModelSuite(_household);
#if RELEASE
      } catch (Exception e) {
        throw new Framework.Exceptions.HouseholdModelException(string.Format("Error running household models for {0}.", _household), e);
      }
#endif
    }


    private void RunHouseholdDayModels() {
      if (!Global.Configuration.ShouldRunPersonDayModels) {
        return;
      }

      foreach (HouseholdDayWrapper householdDay in _household.HouseholdDays) {
#if RELEASE
        try {
#endif

          ChoiceModelFactory.TotalHouseholdDays[ParallelUtility.threadLocalAssignedIndex.Value]++;  //TODO:  John M.  This replaces TotalPersonDays, but TotalPersonDays is used in Engine, so that code probably needs to be patched

          bool simulatedAnInvalidHouseholdDay = false;

          while (!householdDay.IsValid && (!Global.Configuration.IsInEstimationMode || !simulatedAnInvalidHouseholdDay)) { //don't retry household in estimation mode

            if (Global.Configuration.InvalidAttemptsBeforeContinue > 0 && householdDay.AttemptedSimulations > Global.Configuration.InvalidAttemptsBeforeContinue) {
              Global.PrintFile.WriteLine("***** Household day for household {0} invalid after {1} attempts", householdDay.Household.Id, householdDay.AttemptedSimulations);
              break;
            } else {
              householdDay.IsValid = true;
            }
            foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
              personDay.IsValid = true;
            }

            //mbtrace
            Global.TraceResults = (Global.Configuration.TraceModelResultValidity && householdDay.AttemptedSimulations >= Global.Configuration.InvalidAttemptsBeforeTrace);
            //mbtrace
            if (Global.TraceResults) {
              Global.PrintFile.WriteLine("> RunHouseholdDayModels for household {0}, attempt {1}", householdDay.Household.Id, householdDay.AttemptedSimulations);
            }

            ChoiceModelFactory.TotalTimesHouseholdDayModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

            /*if (householdDay.ToString().Contains("80205"))
            {

            }*/
            RunHouseholdDayModelSuite(householdDay);

            // householdDay is invalid if any person day is invalid
            foreach (IPersonDayWrapper personDay in householdDay.PersonDays) {
              if (personDay.IsValid == false) {
                householdDay.IsValid = false;
              }
            }

            // exits the loop if the household's day is valid
            if (householdDay.IsValid) {
              // after updating park and ride lot loads
              foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
                if (!Global.Configuration.IsInEstimationMode && personDay.Tours != null) {
                  foreach (ITourWrapper tour in personDay.Tours.Where(tour =>
                  (tour.Mode == Global.Settings.Modes.CarParkRideWalk ||
                   tour.Mode == Global.Settings.Modes.CarParkRideBike ||
                   tour.Mode == Global.Settings.Modes.CarParkRideShare))) {
                    tour.SetParkAndRideStay();
                  }
                }
              }

              break;
            }

            householdDay.AttemptedSimulations++;
            foreach (IPersonDayWrapper personDay in householdDay.PersonDays) {
              personDay.AttemptedSimulations++;
            }

            if (!simulatedAnInvalidHouseholdDay) {
              simulatedAnInvalidHouseholdDay = true;

              // counts unique instances where a household's day is invalid

              ChoiceModelFactory.TotalInvalidAttempts[ParallelUtility.threadLocalAssignedIndex.Value]++;

            }

            householdDay.Reset();
          }
#if RELEASE
        } catch (Exception e) {
          throw new Framework.Exceptions.HouseholdDayModelException(string.Format("Error running household day models for {0}.", _household), e);
        }
#endif


      }
    }


    private static void RunTourTripModels(TourWrapper tour, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int firstDirection, int lastDirection) {
      if (!Global.Configuration.ShouldRunTourTripModels) {
        return;
      }

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunTourTripModels Person {0} Tour {1} Direction {2} to {3}",
                 personDay.Person.Sequence, tour.Sequence, firstDirection, lastDirection);
      }

      ChoiceModelFactory.TotalTimesProcessHalfToursRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

      ProcessHalfTours(tour, personDay, householdDay, firstDirection, lastDirection);

      if (!tour.PersonDay.IsValid) {
        return;
      }

      tour.SetOriginTimes();
    }

    private static void RunSubtourModels(TourWrapper tour, PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      if (!Global.Configuration.ShouldRunSubtourModels) {
        return;
      }

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunSubtourModels Person {0} Tour {1}", personDay.Person.Sequence, tour.Sequence);
      }

      foreach (TourWrapper subtour in tour.Subtours) {
#if RELEASE
        try {
#endif

          ChoiceModelFactory.TotalTimesTourSubtourModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          RunSubtourModelSuite(subtour, householdDay);

          if (!tour.PersonDay.IsValid) {
            return;
          }


          ChoiceModelFactory.TotalTimesSubtourTripModelsRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          RunSubtourTripModels(subtour, personDay, householdDay, Global.Settings.TourDirections.OriginToDestination, Global.Settings.TourDirections.DestinationToOrigin);

          if (!tour.PersonDay.IsValid) {
            return;
          }

#if RELEASE
        } catch (Exception e) {
          throw new Framework.Exceptions.SubtourModelException(string.Format("Error running subtour models for {0}.", subtour), e);
        }
#endif
      }
    }

    private static void RunSubtourTripModels(TourWrapper subtour, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int firstDirection, int lastDirection) {
      if (!Global.Configuration.ShouldRunSubtourTripModels) {
        return;
      }

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > RunSubtourTripModels Subtour {0}", subtour.Sequence);
      }

      ChoiceModelFactory.TotalTimesProcessHalfSubtoursRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

      ProcessHalfTours(subtour, personDay, householdDay, firstDirection, lastDirection);

      if (!subtour.PersonDay.IsValid) {
        return;
      }

      subtour.SetOriginTimes();
    }

    private static void RunHouseholdModelSuite(HouseholdWrapper household) {

      //use different children age categories
      household.SetActumHouseholdTotals();

      //begin work location person loop
      foreach (PersonWrapper person in household.Persons) {

        if (Global.Configuration.ShouldRunWorkLocationModel && person.IsFullOrPartTimeWorker) {
          if (Global.Configuration.IsInEstimationMode || person.Household.RandomUtility.Uniform01() > household.FractionWorkersWithJobsOutsideRegion) {
            // sets a person's usual work location
            // for full or part-time workers
            ChoiceModelFactory.TotalTimesWorkLocationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;
            Global.ChoiceModelSession.Get<WorkLocationModel>().Run(person, Global.Configuration.WorkLocationModelSampleSize);
          } else {
            if (!Global.Configuration.IsInEstimationMode) {
              person.UsualWorkParcelId = Global.Settings.OutOfRegionParcelId;
            }
          }
        }

        if (Global.Configuration.ShouldRunSchoolLocationModel && person.IsStudent) {
          // sets a person's school location

          ChoiceModelFactory.TotalTimesSchoolLocationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<SchoolLocationModel>().Run(person, Global.Configuration.SchoolLocationModelSampleSize);
        }

        if (Global.Configuration.ShouldRunWorkLocationModel && person.IsWorker && person.IsNotFullOrPartTimeWorker) {
          if (Global.Configuration.IsInEstimationMode || person.Household.RandomUtility.Uniform01() > household.FractionWorkersWithJobsOutsideRegion) {
            // sets a person's usual work location
            // for other workers in relation to a person's school location
            ChoiceModelFactory.TotalTimesWorkLocationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;
            Global.ChoiceModelSession.Get<WorkLocationModel>().Run(person, Global.Configuration.WorkLocationModelSampleSize);
          } else {
            if (!Global.Configuration.IsInEstimationMode) {
              person.UsualWorkParcelId = Global.Settings.OutOfRegionParcelId;
            }
          }
        }
        if (person.IsWorker && person.UsualWorkParcel != null // && person.UsualWorkParcel.ParkingOffStreetPaidDailySpacesBuffer2 > 0 
             && Global.Configuration.IncludePayToParkAtWorkplaceModel) {
          if (Global.Configuration.ShouldRunPayToParkAtWorkplaceModel) {
            ChoiceModelFactory.TotalTimesPaidParkingAtWorkplaceModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;
            Global.ChoiceModelSession.Get<PayToParkAtWorkplaceModel>().Run(person);
          }
        } else {
          person.PaidParkingAtWorkplace = 1; // by default, people pay the parcel parking price
        }
      }
      // end work location person loop

      // begin household auto ownership section
      if (Global.Configuration.ShouldRunAutoOwnershipModel) {
        // sets number of vehicles in household
        ChoiceModelFactory.TotalTimesAutoOwnershipModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;
        Global.ChoiceModelSession.Get<AutoOwnershipModel>().Run(household);
      }
      // end household auto ownership section


      // begin transit pass ownership person loop
      foreach (PersonWrapper person in household.Persons) {

        if (person.Age > Global.Configuration.COMPASS_TransitFareMaximumAgeForFreeTravel && Global.Configuration.IncludeTransitPassOwnershipModel) {
          if (Global.Configuration.ShouldRunTransitPassOwnershipModel) {

            ChoiceModelFactory.TotalTimesTransitPassOwnershipModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

            Global.ChoiceModelSession.Get<TransitPassOwnershipModel>().Run(person);
          }
        }
      }
      // end transit pass ownership person loop

    }

    private static void RunHouseholdDayModelSuite(HouseholdDayWrapper householdDay) {

      if (householdDay.Household.Id == 81400) { //15454) {
      }
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > RunHouseholdDayModelSuite for household {0}", householdDay.Household.Id);
      }

      if (Global.Configuration.ShouldRunActumPrimaryPriorityTimeModel) {
        // determines if household day includes primary priority time

        ChoiceModelFactory.TotalTimesActumPrimaryPriorityTimeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<PrimaryPriorityTimeModel>().Run(householdDay);
        Global.ChoiceModelSession.Get<PrimaryPriorityTimeScheduleModel>().Run(householdDay);

        if (householdDay.PrimaryPriorityTimeFlag == 1) {
          foreach (PersonDayWrapper personDay in householdDay.PersonDays) {

            if (householdDay.Household.Id == 80138 && householdDay.AttemptedSimulations == 0 && personDay.Person.Sequence == 2) {
            }

            personDay.TimeWindow.SetBusyMinutes(householdDay.StartingMinuteSharedHomeStay - 180, householdDay.StartingMinuteSharedHomeStay - 180 + householdDay.DurationMinutesSharedHomeStay + 1);
          }
        }
      }
      if (householdDay.IsValid == false) {
        return;
      }

      if (Global.Configuration.ShouldRunHouseholdDayPatternTypeModel) {
        // determines if household day includes primary priority time

        ChoiceModelFactory.TotalTimesHouseholdDayPatternTypeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<HouseholdDayPatternTypeModel>().Run(householdDay);
        IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.Person.GetHouseholdDayPatternParticipationPriority()).ToList().Cast<PersonDayWrapper>();
        int i = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          i++;
          if (i > 4 || (Global.Configuration.IsInEstimationMode && Global.Configuration.EstimationModel == DaySim.ChoiceModels.Actum.Models.PersonDayPatternTypeModel.CHOICE_MODEL_NAME)) {

            ChoiceModelFactory.TotalTimesPersonDayPatternTypeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

            Global.ChoiceModelSession.Get<PersonDayPatternTypeModel>().Run(personDay, householdDay);
          }
          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > Predicted pattern type for person {0} is {1}", personDay.Person.Sequence, personDay.PatternType);
          }
        }

      }
      if (householdDay.IsValid == false) {
        return;
      }

      foreach (PersonDayWrapper personDay in householdDay.PersonDays) {

        ChoiceModelFactory.TotalTimesPersonDayMandatoryModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        RunPersonDayMandatoryModelSuite(personDay, householdDay);
        if (personDay.IsValid == false) {
          return;
        }
      }


      ChoiceModelFactory.TotalTimesJointHalfTourGenerationModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

      RunJointHalfTourGenerationModelSuite(householdDay);


      ChoiceModelFactory.TotalTimesJointTourGenerationModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

      RunJointTourGenerationModelSuite(householdDay);
      if (householdDay.IsValid == false) {
        return;
      }

      foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
#if RELEASE
        try {
#endif

          ChoiceModelFactory.TotalTimesPersonDayModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          RunPersonDayModelSuite(personDay, householdDay);
          if (personDay.IsValid == false) {
            return;
          }
          if (!Global.Configuration.IsInEstimationMode) {
            personDay.SetHomeBasedNonMandatoryTours();
          }


#if RELEASE
        } catch (Exception e) {
          throw new Framework.Exceptions.PersonDayModelException(string.Format("Error running person-day models for {0}.", personDay), e);
        }
#endif
      }

      if (!Global.Configuration.ShouldRunTourModels) {
        return;
      }

      foreach (IPartialHalfTourWrapper partialJointHalfTour in householdDay.PartialHalfToursList) {
        if (householdDay.IsMissingData) {
          break;
        }

        ChoiceModelFactory.TotalTimesPartialJointHalfTourModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        RunPartialJointHalfTourModelSuite(householdDay, partialJointHalfTour);
        if (householdDay.IsValid == false) {
          return;
        }
      }

      foreach (IFullHalfTourWrapper fullJointHalfTour in householdDay.FullHalfToursList) {

        ChoiceModelFactory.TotalTimesFullJointHalfTourModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;


        if (householdDay.Household.Id == 80138 && householdDay.AttemptedSimulations == 0) {
        }



        RunFullJointHalfTourModelSuite(householdDay, fullJointHalfTour);
        if (householdDay.IsValid == false) {
          return;
        }
      }

      foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
        foreach (TourWrapper tour in personDay.Tours) {
          if (tour.DestinationPurpose == Global.Settings.Purposes.Work
              || tour.DestinationPurpose == Global.Settings.Purposes.School
              || tour.DestinationPurpose == Global.Settings.Purposes.Business) {

            ChoiceModelFactory.TotalTimesMandatoryTourModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;


            RunMandatoryTourModelSuite(tour, personDay, householdDay);
            if (personDay.IsValid == false) {
              return;
            }
          }
        }
      }

      foreach (IJointTourWrapper jointTour in householdDay.JointToursList) {

        ChoiceModelFactory.TotalTimesJointTourModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        RunJointTourModelSuite(householdDay, jointTour);
      }
      if (householdDay.IsValid == false) {
        return;
      }

      foreach (PersonDayWrapper personDay in householdDay.PersonDays) {

        // creates and adds individual tours to person's day (if not in estimation mode)
        // (in estimation mode they were already created)
        // tours are created by purpose
        //if (!Global.Configuration.IsInEstimationMode) {
        //	personDay.GetIndividualTourSimulatedData(personDay, personDay.Tours);
        //}

        foreach (TourWrapper tour in personDay.Tours) {
          if (!(tour.DestinationPurpose == Global.Settings.Purposes.Work
              || tour.DestinationPurpose == Global.Settings.Purposes.School
              || tour.DestinationPurpose == Global.Settings.Purposes.Business)) {

            ChoiceModelFactory.TotalTimesNonMandatoryTourModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

            RunNonMandatoryTourModelSuite(tour, personDay, householdDay);
            if (personDay.IsValid == false) {
              return;
            }
          }
        }
      }



      //			foreach (ActumPersonDayWrapper personDay in householdDay.PersonDays) {
      //#if RELEASE
      //				try {
      //#endif
      //				if (personDay.IsValid) {
      //					// update park and ride lot loads
      //					if (!Global.Configuration.IsInEstimationMode && personDay.Tours != null) {
      //						foreach (ActumTourWrapper tour in personDay.Tours.Where(tour => tour.Mode == Global.Settings.Modes.ParkAndRide)) {
      //							tour.SetParkAndRideStay();
      //						}
      //					}
      //				}
      //
      //#if RELEASE
      //				}
      //				catch (Exception e) {
      //				throw new PersonDayModelException(string.Format("Error running person-day models for {0}.", personDay), e);
      //				}
      //#endif
      //			}

      // TODO:  add logic that deals with invalid person days.  Probably need to flag invalid householdDays instead.
    }

    private static void RunPersonDayMandatoryModelSuite(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunPersonDayMandatoryModelSuite for household {0} person {1}", householdDay.Household.Id, personDay.Person.Sequence);
      }

      if (Global.Configuration.ShouldRunWorkAtHomeModel) {
        // determines if full or part time worker works at home during day

        ChoiceModelFactory.TotalTimesWorkAtHomeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<WorkAtHomeModel>().Run(personDay, householdDay);
        //mbtrace
        if (Global.TraceResults) {
          Global.PrintFile.WriteLine("> > > > HWorkAtHomeModel predicts WorkAtHomeDuration {0}", personDay.WorkAtHomeDuration);
        }
      }

      if (Global.Configuration.ShouldRunMandatoryTourGenerationModel) {

        // creates tour list; full for estimation mode, empty for application mode
        personDay.InitializeTours();

        if (personDay.PatternType == Global.Settings.PatternTypes.Mandatory && (personDay.Person.IsWorker || personDay.Person.IsStudent)) {
          int[] totalMandatoryTours = new int[4];
          int[] simulatedMandatoryTours = new int[4];
          int choice;
          int ncallsfortour = 0;
          if (Global.Configuration.IsInEstimationMode) {
            totalMandatoryTours[1] = personDay.UsualWorkplaceTours;
            totalMandatoryTours[2] = personDay.BusinessTours;
            totalMandatoryTours[3] = personDay.SchoolTours;
            totalMandatoryTours[0] = totalMandatoryTours[1] + totalMandatoryTours[2] + totalMandatoryTours[3];
            if (personDay.UsualWorkplaceTours + personDay.SchoolTours > 0) {
              personDay.HasMandatoryTourToUsualLocation = true;
            }
            for (int i = 0; i <= totalMandatoryTours[0]; i++) {
              if (i < totalMandatoryTours[1]) { choice = 1; } else if (i < totalMandatoryTours[1] + totalMandatoryTours[2]) { choice = 2; } else if (i < totalMandatoryTours[0]) { choice = 3; } else { choice = 0; }

              ChoiceModelFactory.TotalTimesMandatoryTourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

              ncallsfortour++;
              Global.ChoiceModelSession.Get<MandatoryTourGenerationModel>().Run(personDay, householdDay, ncallsfortour, simulatedMandatoryTours, choice);
              if (choice > 0) {
                simulatedMandatoryTours[choice]++;
                simulatedMandatoryTours[0]++;
              }
            }
          } else {
            int maxNumberOfMandatoryTours = 2;

            for (int i = 0; i <= totalMandatoryTours[0]; i++) {

              ChoiceModelFactory.TotalTimesMandatoryTourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

              ncallsfortour++;
              int chosenAlternative = Global.ChoiceModelSession.Get<MandatoryTourGenerationModel>().Run(personDay, householdDay, ncallsfortour, simulatedMandatoryTours);
              if (chosenAlternative > 0) {
                simulatedMandatoryTours[chosenAlternative]++;
                simulatedMandatoryTours[0]++;
                totalMandatoryTours[0]++;
                if (chosenAlternative == 1 || chosenAlternative == 3) {
                  personDay.HasMandatoryTourToUsualLocation = true;
                }
              }
              if (totalMandatoryTours[0] >= maxNumberOfMandatoryTours) { break; }
            }
            // create the mandatory tours and add them to the person day's list of tours
            personDay.UsualWorkplaceTours = simulatedMandatoryTours[1];
            personDay.WorkTours = simulatedMandatoryTours[1];
            personDay.BusinessTours = simulatedMandatoryTours[2];
            personDay.SchoolTours = simulatedMandatoryTours[3];
            personDay.GetMandatoryTourSimulatedData(personDay, personDay.Tours);
            //mbtrace
            if (Global.TraceResults) {
              Global.PrintFile.WriteLine("> > > > Generated mandatory tours: Usual work {0} Business {1} School {2}",
                             simulatedMandatoryTours[1], simulatedMandatoryTours[2], simulatedMandatoryTours[3]);
            }
          }

          personDay.CreatedWorkTours = personDay.WorkTours;
          personDay.CreatedBusinessTours = personDay.BusinessTours;
          personDay.CreatedSchoolTours = personDay.SchoolTours;

          // determine presence of mandatory stops

          ChoiceModelFactory.TotalTimesMandatoryStopPresenceModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<MandatoryStopPresenceModel>().Run(personDay, householdDay);
          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > > Generated mandatory stops 0/1+: Business {0} School {1}",
                         personDay.BusinessStops, personDay.SchoolStops);
          }

          // generate subtours
          //foreach (ActumTourWrapper tour in personDay.Tours) {
          //	GenerateSubtours(tour, householdDay);
          //}
        }
      }

    }

    private static void RunJointHalfTourGenerationModelSuite(HouseholdDayWrapper householdDay) {

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > RunJointHalfTourGenerationModelSuite for Household {0}", householdDay.Household.Id);
      }

      if (householdDay.Household.Id == 3601) { //3913) { //3556 35552 2033 2495{
      }
      int maxNumberParticipants = 5;

      if (!Global.Configuration.ShouldRunJointHalfTourGenerationModels) {
        return;
      }

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointHalfTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

      // initialize availability for all HH members and for household
      //    This logic doesn't depend on survey data (ie, it works with synthetic pop too)
      //bool[] adult = new bool[9];
      bool[,] hTAvailable = new bool[3, 9]; //[subtype (0--paired, 1--outbound, 2--return), personDay.Person.Sequence]
      bool[] fHTAvailable = new bool[9];
      bool[] pHTAvailable = new bool[9];
      int[] fHTMandatoryAdultsAvailable = new int[3]; //[subtype]
      int[] fHTMandatoryPersonsAvailable = new int[3]; //[subtype]
      int[] fHTNonMandatoryAdultsAvailable = new int[3]; //[subtype]
      int[] pHTAdultsAvailable = new int[3]; //[subtype]
      int[] pHTPersonsAvailable = new int[3]; //[subtype]
                                              //int pDay = 0;
      int count = 0;
      foreach (PersonDayWrapper personDay in orderedPersonDays) {
        //pDay++;
        //if (personDay.Person.IsAdult) {
        //	adult[pDay] = true;
        //}
        count++;
        if (count <= maxNumberParticipants) {
          if (personDay.HasMandatoryTourToUsualLocation) {
            for (int i = 0; i < 3; i++) {
              fHTMandatoryPersonsAvailable[i]++;
              pHTPersonsAvailable[i]++;
              hTAvailable[i, count] = true;
            }
            fHTAvailable[count] = true;
            pHTAvailable[count] = true;
            if (personDay.Person.IsAdult) {
              for (int i = 0; i < 3; i++) {
                fHTMandatoryAdultsAvailable[i]++;
                pHTAdultsAvailable[i]++;
              }
            }
          } else if (!(personDay.PatternType == Global.Settings.PatternTypes.Home)
                && personDay.Person.IsAdult) {
            fHTAvailable[count] = true;
            for (int i = 0; i < 3; i++) {
              fHTNonMandatoryAdultsAvailable[i]++;
              hTAvailable[i, count] = true;
            }
          }
        }
      }

      int[] jHTId = new int[8];  //Id of the first (if paired) pht or fht survey record corresponding to the observed joint halftour or halftour pair
      int[] jHTType = new int[8];  //1--full; 2--partial
      int[] jHTSubType = new int[8]; //0--paired; 1--outbound halftour; 2--return halftour 
      int[] jHTChauffeurSequence = new int[8];  //person.Sequence of PartialJointHalfTour chauffeur
      bool[,] jHTParticipation = new bool[8, Global.MaximumHouseholdSize]; //jhtour x personDay.Sequence
      int jHTCount = 0;
      bool[] paired = new bool[9];

      if (Global.Configuration.IsInEstimationMode) {
        // Derive choice variables and identify participants for all joint half tours
        //   This logic depends on survey data (must be used only in estimation mode)
        int i1 = 0;
        foreach (IFullHalfTourWrapper hT1 in householdDay.FullHalfToursList) {
          i1++;
          int i2 = 0;
          foreach (IFullHalfTourWrapper hT2 in householdDay.FullHalfToursList) {
            i2++;
            if (!(hT1.Equals(hT2))
                && (i2 > i1)
                && (paired[i1] == false)
                && (paired[i2] == false)
                && !(hT1.Direction == hT2.Direction)
                && (hT1.PersonSequence1 == hT2.PersonSequence1)
                && (hT1.PersonSequence2 == hT2.PersonSequence2)
                && (hT1.PersonSequence3 == hT2.PersonSequence3)
                && (hT1.PersonSequence4 == hT2.PersonSequence4)
                && (hT1.PersonSequence5 == hT2.PersonSequence5)
                && (hT1.PersonSequence6 == hT2.PersonSequence6)
                && (hT1.PersonSequence7 == hT2.PersonSequence7)
                && (hT1.PersonSequence8 == hT2.PersonSequence8)
                ) {
              paired[i1] = hT1.Paired = true;
              paired[i2] = hT2.Paired = true;
              jHTCount++;
              jHTType[jHTCount] = 1;  //full
              jHTSubType[jHTCount] = 0; //paired
              jHTId[jHTCount] = hT1.Id;
              if (hT1.PersonSequence1 > 0 && hT1.PersonSequence1 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence1] = true; }
              if (hT1.PersonSequence2 > 0 && hT1.PersonSequence2 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence2] = true; }
              if (hT1.PersonSequence3 > 0 && hT1.PersonSequence3 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence3] = true; }
              if (hT1.PersonSequence4 > 0 && hT1.PersonSequence4 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence4] = true; }
              if (hT1.PersonSequence5 > 0 && hT1.PersonSequence5 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence5] = true; }
              if (hT1.PersonSequence6 > 0 && hT1.PersonSequence6 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence6] = true; }
              if (hT1.PersonSequence7 > 0 && hT1.PersonSequence7 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence7] = true; }
              if (hT1.PersonSequence8 > 0 && hT1.PersonSequence8 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence8] = true; }
            }
          }
        }
        i1 = 0;
        foreach (IFullHalfTourWrapper hT1 in householdDay.FullHalfToursList) {
          i1++;
          if (paired[i1] == false) {
            jHTCount++;
            jHTType[jHTCount] = 1;  //full
            jHTSubType[jHTCount] = hT1.Direction; //direction
            jHTId[jHTCount] = hT1.Id;
            if (hT1.PersonSequence1 > 0 && hT1.PersonSequence1 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence1] = true; }
            if (hT1.PersonSequence2 > 0 && hT1.PersonSequence2 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence2] = true; }
            if (hT1.PersonSequence3 > 0 && hT1.PersonSequence3 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence3] = true; }
            if (hT1.PersonSequence4 > 0 && hT1.PersonSequence4 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence4] = true; }
            if (hT1.PersonSequence5 > 0 && hT1.PersonSequence5 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence5] = true; }
            if (hT1.PersonSequence6 > 0 && hT1.PersonSequence6 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence6] = true; }
            if (hT1.PersonSequence7 > 0 && hT1.PersonSequence7 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence7] = true; }
            if (hT1.PersonSequence8 > 0 && hT1.PersonSequence8 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence8] = true; }
          }
        }

        for (int i = 0; i < 9; i++) { paired[i] = false; }
        i1 = 0;
        foreach (IPartialHalfTourWrapper hT1 in householdDay.PartialHalfToursList) {
          i1++;
          int i2 = 0;
          foreach (IPartialHalfTourWrapper hT2 in householdDay.PartialHalfToursList) {
            i2++;
            if (!(ReferenceEquals(hT1, hT2))
                && (i2 > i1)
                && (paired[i1] == false)
                && (paired[i2] == false)
                && !(hT1.Direction == hT2.Direction)
                && (hT1.PersonSequence1 == hT2.PersonSequence1)
                && (hT1.PersonSequence2 == hT2.PersonSequence2)
                && (hT1.PersonSequence3 == hT2.PersonSequence3)
                && (hT1.PersonSequence4 == hT2.PersonSequence4)
                && (hT1.PersonSequence5 == hT2.PersonSequence5)
                && (hT1.PersonSequence6 == hT2.PersonSequence6)
                && (hT1.PersonSequence7 == hT2.PersonSequence7)
                && (hT1.PersonSequence8 == hT2.PersonSequence8)
                ) {
              paired[i1] = hT1.Paired = true;
              paired[i2] = hT2.Paired = true;
              jHTCount++;
              jHTType[jHTCount] = 2;  //partial
              jHTSubType[jHTCount] = 0; //paired
              jHTChauffeurSequence[jHTCount] = hT1.PersonSequence1;
              jHTId[jHTCount] = hT1.Id;
              if (hT1.PersonSequence1 > 0 && hT1.PersonSequence1 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence1] = true; }
              if (hT1.PersonSequence2 > 0 && hT1.PersonSequence2 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence2] = true; }
              if (hT1.PersonSequence3 > 0 && hT1.PersonSequence3 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence3] = true; }
              if (hT1.PersonSequence4 > 0 && hT1.PersonSequence4 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence4] = true; }
              if (hT1.PersonSequence5 > 0 && hT1.PersonSequence5 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence5] = true; }
              if (hT1.PersonSequence6 > 0 && hT1.PersonSequence6 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence6] = true; }
              if (hT1.PersonSequence7 > 0 && hT1.PersonSequence7 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence7] = true; }
              if (hT1.PersonSequence8 > 0 && hT1.PersonSequence8 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence8] = true; }
            }
          }
        }
        i1 = 0;
        foreach (IPartialHalfTourWrapper hT1 in householdDay.PartialHalfToursList) {
          i1++;
          if (paired[i1] == false) {
            jHTCount++;
            jHTType[jHTCount] = 2;  //partial
            jHTSubType[jHTCount] = hT1.Direction; //direction
            jHTChauffeurSequence[jHTCount] = hT1.PersonSequence1;
            jHTId[jHTCount] = hT1.Id;
            if (hT1.PersonSequence1 > 0 && hT1.PersonSequence1 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence1] = true; }
            if (hT1.PersonSequence2 > 0 && hT1.PersonSequence2 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence2] = true; }
            if (hT1.PersonSequence3 > 0 && hT1.PersonSequence3 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence3] = true; }
            if (hT1.PersonSequence4 > 0 && hT1.PersonSequence4 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence4] = true; }
            if (hT1.PersonSequence5 > 0 && hT1.PersonSequence5 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence5] = true; }
            if (hT1.PersonSequence6 > 0 && hT1.PersonSequence6 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence6] = true; }
            if (hT1.PersonSequence7 > 0 && hT1.PersonSequence7 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence7] = true; }
            if (hT1.PersonSequence8 > 0 && hT1.PersonSequence8 <= Global.MaximumHouseholdSize) { jHTParticipation[jHTCount, hT1.PersonSequence8] = true; }
          }
        }
        int count2 = 0;
        foreach (IPersonDayWrapper personDay in householdDay.PersonDays) {
          count2++;
          foreach (ITourWrapper tour in personDay.Tours) {
            if (count2 <= maxNumberParticipants
                && (tour.FullHalfTour1Sequence > 0 || tour.FullHalfTour2Sequence > 0)
                && tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
              personDay.EscortFullHalfTours++;
              //personDay.CreatedEscortTours++;
            }
          }
        }
      } // end choice variable evaluation for estimation mode

      // run generation and participation models

      int type = 0;
      int subType = 0;
      int jHTSimulated = 0;
      bool[] jHTAvailable = new bool[7];  //alternatives
      int nCallsForTour = 0;
      int maxChoice = 0;
      int maxJointHalfTours = 5;
      for (int i = 0; i <= jHTSimulated; i++) {

        //determine availability of the seven choice alternatives for the generation model
        for (int j = 0; j < 7; j++) {
          if (j == 0) { jHTAvailable[j] = true; } else if (j < 4) {
            //						if ((fHTMandatoryAdultsAvailable[j - 1] >= 1 || fHTNonMandatoryAdultsAvailable[j - 1] >= 1)
            //							 && (fHTMandatoryPersonsAvailable[j - 1] >= 2)
            // JLB 201404 replace next line to allow fht with only mandatory children
            //if ((fHTMandatoryAdultsAvailable[j - 1] >= 1 && fHTMandatoryPersonsAvailable[j - 1] >= 2)
            if ((fHTMandatoryPersonsAvailable[j - 1] >= 2)
                || (fHTNonMandatoryAdultsAvailable[j - 1] >= 1 && fHTMandatoryPersonsAvailable[j - 1] >= 1)
                ) {
              jHTAvailable[j] = true;
            } else {
              jHTAvailable[j] = false;
            }
          } else {
            if (pHTAdultsAvailable[j - 4] >= 1 && pHTPersonsAvailable[j - 4] >= 2) {
              jHTAvailable[j] = true;
            } else {
              jHTAvailable[j] = false;
            }
          }
        }
        //Availability restrictions arising from sequence of data
        if (maxChoice >= 2) {
          jHTAvailable[1] = false;
        }
        if (maxChoice >= 4) {
          jHTAvailable[2] = false;
          jHTAvailable[3] = false;
        }
        if (maxChoice >= 5) {
          jHTAvailable[4] = false;
        }
        //user suppression of partial joint half tours
        if (Global.Configuration.ShouldSuppressPartiallyJointHalfTours) {
          jHTAvailable[4] = false;
          jHTAvailable[5] = false;
          jHTAvailable[6] = false;
        }

        //Set choice variables type and subType
        if (Global.Configuration.IsInEstimationMode) {
          if (jHTSimulated == jHTCount) {
            type = 0;
            subType = 0;
          } else {
            type = jHTType[i + 1];
            subType = jHTSubType[i + 1];
          }
        }

        ChoiceModelFactory.TotalTimesJointHalfTourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        nCallsForTour++;
        int genChoice = Global.ChoiceModelSession.Get<JointHalfTourGenerationModel>().Run(householdDay, nCallsForTour, jHTAvailable, type, subType);

        int[] participants = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        if (genChoice > 0) {
          jHTSimulated++;
          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > Generated joint half tour {0}, type {1}", jHTSimulated, genChoice);
          }
        }

        if (genChoice > maxChoice) { maxChoice = genChoice; }  // used to limit choice set due to ordering of data:  fht first, and paired first within that

        if (genChoice > 0 && genChoice <= 3) {
          // run full half tour participation model

          ChoiceModelFactory.TotalTimesFullJointHalfTourParticipationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          participants = Global.ChoiceModelSession.Get<FullJointHalfTourParticipationModel>().Run(householdDay, jHTSimulated, genChoice, hTAvailable, fHTAvailable, jHTParticipation);
          if (!Global.Configuration.IsInEstimationMode && !householdDay.IsValid) {
            return;
          }

          if (Global.Configuration.IsInEstimationMode) {
            int numberAdults = 0;
            int count2 = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
              count2++;
              if (count2 <= maxNumberParticipants) {
                if (participants[count2] == 1 && personDay.Person.IsAdult) {
                  numberAdults++;
                }
              }
            }
            if (genChoice == 1 || genChoice == 2) {  //half tour to mandatory
              int count3 = 0;
              foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count3++;
                if (count3 <= maxNumberParticipants) {
                  if (participants[count3] == 1 &&
                      (!(personDay.PatternType == Global.Settings.PatternTypes.Mandatory)
                      || (personDay.Person.IsAdult && numberAdults == 1))) { //assume that if only adult on tour it is an escort for them even if they have mandatory pattern
                    personDay.CreatedEscortTours++;
                  }
                }
              }
            }
            if (genChoice == 1 || genChoice == 3) { //half tour from mandatory
                                                    // if nonmandatory escorts both directions we assume that they make two tours to do it
              int count3 = 0;
              foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count3++;
                if (count3 <= maxNumberParticipants) {
                  if (participants[count3] == 1 &&
                      (!(personDay.PatternType == Global.Settings.PatternTypes.Mandatory)
                      || (personDay.Person.IsAdult && numberAdults == 1))) { //assume that if only adult on tour it is an escort for them even if they have mandatory pattern
                    personDay.CreatedEscortTours++;
                  }
                }
              }
            }
          } else {  // (!Global.Configuration.IsInEstimationMode) 
            int numberAdults = 0;
            int count2 = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
              count2++;
              if (count2 <= maxNumberParticipants) {
                if (participants[count2] == 1 && personDay.Person.IsAdult) {
                  numberAdults++;
                }
              }
            }
            if (genChoice == 1 || genChoice == 2) {  //half tour to mandatory
              IFullHalfTourWrapper fullHalfTour = householdDay.CreateFullHalfTour(householdDay, orderedPersonDays, participants, 1);
              int count3 = 0;
              foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count3++;
                if (count3 <= maxNumberParticipants) {
                  if (participants[count3] == 1 &&
                       ((!personDay.HasMandatoryTourToUsualLocation)
                      || (personDay.Person.IsAdult && numberAdults == 1))) { //assume that if only adult on tour it is an escort for them even if they have mandatory pattern
                                                                             // create nonmandatory tour for person and associate it with fullJointHalfTour
                    TourWrapper tour = (TourWrapper)personDay.GetEscortTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey);
                    tour.FullHalfTour1Sequence = fullHalfTour.Sequence;
                    fullHalfTour.SetParticipantTourSequence(tour);
                    personDay.EscortFullHalfTours++;
                    personDay.CreatedEscortTours++;
                  }
                  //update mandatory tour of mandatory participants with FullJointHalfTour index
                  else if ((participants[count3] == 1) && (personDay.HasMandatoryTourToUsualLocation)) {
                    if (personDay.Person.IsFullOrPartTimeWorker) {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      if (!(tour == null)) {
                        tour.FullHalfTour1Sequence = fullHalfTour.Sequence;
                        fullHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                        if (!(tour == null)) {
                          tour.FullHalfTour1Sequence = fullHalfTour.Sequence;
                          fullHalfTour.SetParticipantTourSequence(tour);
                        }
                      }
                    } else {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                      if (!(tour == null)) {
                        tour.FullHalfTour1Sequence = fullHalfTour.Sequence;
                        fullHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        if (!(tour == null)) {
                          tour.FullHalfTour1Sequence = fullHalfTour.Sequence;
                          fullHalfTour.SetParticipantTourSequence(tour);
                        }
                      }
                    }
                  }
                }
              }
            }
            if (genChoice == 1 || genChoice == 3) { //half tour from mandatory
                                                    // if nonmandatory escorts both directions we assume that they make two tours to do it
              IFullHalfTourWrapper fullHalfTour = householdDay.CreateFullHalfTour(householdDay, orderedPersonDays, participants, 2);
              int count3 = 0;
              foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count3++;
                if (count3 <= maxNumberParticipants) {
                  if ((participants[count3] == 1)
                      && ((!personDay.HasMandatoryTourToUsualLocation)
                      || (personDay.Person.IsAdult && numberAdults == 1))) { //assume that if only adult on tour it is an escort for them even if they have mandatory pattern
                                                                             // create nonmandatory tour for person and associate it with fullJointHalfTour
                    TourWrapper tour = (TourWrapper)personDay.GetEscortTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey);
                    tour.FullHalfTour2Sequence = fullHalfTour.Sequence;
                    fullHalfTour.SetParticipantTourSequence(tour);
                    personDay.EscortFullHalfTours++;
                    personDay.CreatedEscortTours++;
                  }
                  //update mandatory tour of mandatory participants with FullJointHalfTour index
                  else if ((participants[count3] == 1) && (personDay.HasMandatoryTourToUsualLocation)) {
                    if (personDay.Person.IsFullOrPartTimeWorker) {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      if (!(tour == null)) {
                        tour.FullHalfTour2Sequence = fullHalfTour.Sequence;
                        fullHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                        if (!(tour == null)) {
                          tour.FullHalfTour2Sequence = fullHalfTour.Sequence;
                          fullHalfTour.SetParticipantTourSequence(tour);
                        }
                        //TODO:  may need to reject tour or household day if person with mandatory day has no work or school tours to usual location.
                        //  But the logic for running the tour generation and participation should not let this happen
                      }
                    } else {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                      if (!(tour == null)) {
                        tour.FullHalfTour2Sequence = fullHalfTour.Sequence;
                        fullHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        if (!(tour == null)) {
                          tour.FullHalfTour2Sequence = fullHalfTour.Sequence;
                          fullHalfTour.SetParticipantTourSequence(tour);
                        }
                        //TODO:  may need to reject tour or household day if person with mandatory day has no work or school tours to usual location.
                        //  But the logic for running the tour generation and participation should not let this happen
                      }
                    }
                  }
                }
              }
            }
          }
        } else if (genChoice > 0) {

          ChoiceModelFactory.TotalTimesPartialJointHalfTourParticipationAndChauffeurModelsRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          participants = Global.ChoiceModelSession.Get<PartialJointHalfTourParticipationModel>().Run(householdDay, jHTSimulated, genChoice - 3, hTAvailable, pHTAvailable, jHTParticipation);

          if (!Global.Configuration.IsInEstimationMode && !householdDay.IsValid) {
            return;
          }

          int chauffeurSequence = Global.ChoiceModelSession.Get<PartialJointHalfTourChauffeurModel>().Run(householdDay, jHTSimulated, genChoice - 3, participants, jHTChauffeurSequence);
          if (!Global.Configuration.IsInEstimationMode) {
            // determine purpose and order of all participants
            int[] purpose = new int[] { 0, 0, 0, 0, 0, 0 };   //purpose[n] is destinationPurpose of person with person.Sequence = n
                                                              //						int[] pickSequence = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0 }; //pickSequence[n] is the pickup (or reverse dropoff) order of person with person.Sequence = n (chauffeur's pickSequence = 1)
            double[] distanceFromChauffeur = new double[] { 999, 999, 999, 999, 999, 999 }; //after sorting distanceFromChauffeur[n] is distance from chauffeur of pickup (drop off) number n+1 (chauffeur's distanceFromChauffeur is -1)
            IParcelWrapper[] destinationParcel = new IParcelWrapper[6];
            int[] pickOrder = new int[] { 0, 1, 2, 3, 4, 5 };// after sorting, pickOrder[n] is the orderedPerson order of pickup (drop off) number n+1 (chauffeur's pickOrder = 1)
            IParcelWrapper chauffeurParcel = null;
            int count2 = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
              count2++;
              if (count2 <= maxNumberParticipants) {
                //determine tour purposes of participants
                if (participants[count2] == 1) {
                  if (personDay.Person.IsFullOrPartTimeWorker) {
                    TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                    if (!(tour == null)) {
                      purpose[count2] = Global.Settings.Purposes.Work;
                      destinationParcel[count2] = personDay.Person.UsualWorkParcel;
                    } else {
                      tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                      if (!(tour == null)) {
                        purpose[count2] = Global.Settings.Purposes.School;
                        destinationParcel[count2] = personDay.Person.UsualSchoolParcel;

                      }
                    }
                  } else {
                    TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                    if (!(tour == null)) {
                      purpose[count2] = Global.Settings.Purposes.School;
                      destinationParcel[count2] = personDay.Person.UsualSchoolParcel;
                    } else {
                      tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      if (!(tour == null)) {
                        purpose[count2] = Global.Settings.Purposes.Work;
                        destinationParcel[count2] = personDay.Person.UsualWorkParcel;
                      }
                    }
                  }
                  if (personDay.Person.Sequence == chauffeurSequence) {
                    chauffeurParcel = destinationParcel[count2];
                  }

                }
              }
            }

            //determine chauffeur's order in the orderedPersonDays
            int chauffeurOrder = 0;
            int ct = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
              ct++;
              if (ct <= maxNumberParticipants) {
                if (personDay.Person.Sequence == chauffeurSequence) {
                  chauffeurOrder = ct;
                }
              }
            }
            //Loop on the participant array index and calculate distance from the chauffeurParcelID to each participant's destinationParcel
            for (int j = 1; j <= maxNumberParticipants; j++) {
              if (j == chauffeurOrder) {
                distanceFromChauffeur[j] = -1;  // forces chauffeur to be first in pick sequence
              } else if (destinationParcel[j] != null) {
                //double circuityDistance = Global.Configuration.UseShortDistanceNodeToNodeMeasures
                //                                      ? chauffeurParcel.NodeToNodeDistance(destinationParcel[j])
                //                                      : (Global.Configuration.UseShortDistanceCircuityMeasures)
                //                                                ? chauffeurParcel.CircuityDistance(destinationParcel[j])
                //                                                : Constants.DEFAULT_VALUE;
                double circuityDistance = chauffeurParcel.CalculateShortDistance(destinationParcel[j]);

                distanceFromChauffeur[j] = ImpedanceRoster.GetValue("distance", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, 1, chauffeurParcel, destinationParcel[j], circuityDistance).Variable;
              }
            }
            // Sort to find distance rank and set pickSequence in increasing distance from chauffeur parcelID.
            Array.Sort(distanceFromChauffeur, pickOrder);

            if (genChoice == 4 || genChoice == 5) {  //half tour to mandatory

              // Create the partial half tour, put the persons in pick sequence in it, and associate the persons' tours with it.
              IPartialHalfTourWrapper partialHalfTour = householdDay.CreatePartialHalfTour(householdDay, orderedPersonDays, participants, pickOrder, distanceFromChauffeur, 1);
              if (genChoice == 4) {
                partialHalfTour.Paired = true;
              }
              // associate persons' tours with partialHalfTour
              int count3 = 0;
              foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count3++;
                if (count3 <= maxNumberParticipants) {
                  //update mandatory tour of mandatory participants with FullJointHalfTour index
                  if (participants[count3] == 1) {
                    if (personDay.Person.IsFullOrPartTimeWorker) {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      if (!(tour == null)) {
                        tour.PartialHalfTour1Sequence = partialHalfTour.Sequence;
                        partialHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                        if (!(tour == null)) {
                          tour.PartialHalfTour1Sequence = partialHalfTour.Sequence;
                          partialHalfTour.SetParticipantTourSequence(tour);
                        }
                      }
                    } else {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                      if (!(tour == null)) {
                        tour.PartialHalfTour1Sequence = partialHalfTour.Sequence;
                        partialHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        if (!(tour == null)) {
                          tour.PartialHalfTour1Sequence = partialHalfTour.Sequence;
                          partialHalfTour.SetParticipantTourSequence(tour);
                        }
                      }
                    }
                  }
                }
              }
            }

            if (genChoice == 4 || genChoice == 6) {  //half tour from mandatory

              // Create the partial half tour, put the persons in pick sequence in it, and associate the persons' tours with it.
              IPartialHalfTourWrapper partialHalfTour = householdDay.CreatePartialHalfTour(householdDay, orderedPersonDays, participants, pickOrder, distanceFromChauffeur, 2);
              if (genChoice == 4) {
                partialHalfTour.Paired = true;
              }
              // associate persons' tours with partialHalfTour
              int count3 = 0;
              foreach (PersonDayWrapper personDay in orderedPersonDays) {
                count3++;
                if (count3 <= maxNumberParticipants) {
                  //update mandatory tour of mandatory participants with FullJointHalfTour index
                  if (participants[count3] == 1) {
                    if (personDay.Person.IsFullOrPartTimeWorker) {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                      if (!(tour == null)) {
                        tour.PartialHalfTour2Sequence = partialHalfTour.Sequence;
                        partialHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                        if (!(tour == null)) {
                          tour.PartialHalfTour2Sequence = partialHalfTour.Sequence;
                          partialHalfTour.SetParticipantTourSequence(tour);
                        }
                      }
                    } else {
                      TourWrapper tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsSchoolPurpose() == true);
                      //var tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsSchoolPurpose == true);
                      if (!(tour == null)) {
                        tour.PartialHalfTour2Sequence = partialHalfTour.Sequence;
                        partialHalfTour.SetParticipantTourSequence(tour);
                      } else {
                        tour = (TourWrapper)personDay.Tours.FirstOrDefault(x => x.IsWorkPurpose() == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        //tour = personDay.Tours.DefaultIfEmpty(null).First(x => x.IsWorkPurpose == true && x.DestinationParcelId == personDay.Person.UsualWorkParcelId);
                        if (!(tour == null)) {
                          tour.PartialHalfTour2Sequence = partialHalfTour.Sequence;
                          partialHalfTour.SetParticipantTourSequence(tour);
                        }
                      }
                    }
                  }
                }
              }
            }
            // TODO:  create trip records for all trips of all persons on the partialHalfTour
            //        using information about pickSequence and each person's destination parcel and purpose
            //     OR wait and create trip records in the course of the personDay simulation


          }
        }

        // update availability for all participants and for household
        //   based on results of this iteration of the generation and participation models
        //   Logic assumes that a person can have only one joitn half tour in each direction per day
        //     A partial joint halftour and a full joint halftour in the same direction are not both allowed
        int count4 = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          count4++;
          if (count4 <= maxNumberParticipants) {
            for (int j = 1; j <= 3; j++) {

              if ((genChoice == j || genChoice == j + 3) && participants[count4] == 1) {
                // paired alternatives become unavailable if any joint alternative was chosen
                hTAvailable[0, count4] = false;
                if (personDay.HasMandatoryTourToUsualLocation) {
                  fHTMandatoryPersonsAvailable[0]--;
                  pHTPersonsAvailable[0]--;
                  //if (personDay.Person.IsAdult) {
                  if (personDay.Person.IsDrivingAge) {
                    fHTMandatoryAdultsAvailable[0]--;
                    pHTAdultsAvailable[0]--;
                  }
                } else if (!(personDay.PatternType == Global.Settings.PatternTypes.Home)) {
                  fHTNonMandatoryAdultsAvailable[0]--;
                }
                if (j != 1) {
                  //if one direction was chosen, same direction alternatives also become unavailable
                  hTAvailable[j - 1, count4] = false;
                  if (personDay.HasMandatoryTourToUsualLocation) {
                    fHTMandatoryPersonsAvailable[j - 1]--;
                    pHTPersonsAvailable[j - 1]--;
                    //if (personDay.Person.IsAdult) {
                    if (personDay.Person.IsDrivingAge) {
                      fHTMandatoryAdultsAvailable[j - 1]--;
                      pHTAdultsAvailable[j - 1]--;
                    }
                  } else if (!(personDay.PatternType == Global.Settings.PatternTypes.Home)) {
                    fHTNonMandatoryAdultsAvailable[j - 1]--;
                  }
                }
                //if pair was chosen, both one-direction alternatives also become unavailable
                if (j == 1) {
                  hTAvailable[j, count4] = false;
                  hTAvailable[j + 1, count4] = false;
                  if (personDay.HasMandatoryTourToUsualLocation) {
                    fHTMandatoryPersonsAvailable[j]--;
                    pHTPersonsAvailable[j]--;
                    fHTMandatoryPersonsAvailable[j + 1]--;
                    pHTPersonsAvailable[j + 1]--;
                    //if (personDay.Person.IsAdult) {
                    if (personDay.Person.IsDrivingAge) {
                      fHTMandatoryAdultsAvailable[j]--;
                      pHTAdultsAvailable[j]--;
                      fHTMandatoryAdultsAvailable[j + 1]--;
                      pHTAdultsAvailable[j + 1]--;
                    }
                  } else if (!(personDay.PatternType == Global.Settings.PatternTypes.Home)) {
                    fHTNonMandatoryAdultsAvailable[j]--;
                    fHTNonMandatoryAdultsAvailable[j + 1]--;
                  }
                }
              }
            }
          }
        }
        if (jHTSimulated >= maxJointHalfTours && !Global.Configuration.IsInEstimationMode) { break; }
      } //end generation and participation loops
    }

    private static void RunJointTourGenerationModelSuite(HouseholdDayWrapper householdDay) {
      //don't run this model if no more than one person in household is eligible for joint tour
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > RunJointTourGenerationModelSuite for Household {0}", householdDay.Household.Id);
      }

      int maxNumberParticipants = 5;

      IEnumerable<PersonDayWrapper> orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

      int numberOfEligiblePersonsInHousehold = 0;
      foreach (PersonDayWrapper personDay in orderedPersonDays) {
        if (personDay.GetJointTourParticipationPriority() < 9) {
          numberOfEligiblePersonsInHousehold++;
        }
      }
      if (numberOfEligiblePersonsInHousehold <= 1 || !Global.Configuration.ShouldRunJointTourGenerationModel) {
        return;
      }

      // don't run if PFPT has determined that there are no joint tours.  JLB 20140820
      if (Global.Configuration.ShouldRunActumPrimaryPriorityTimeModel && householdDay.JointTourFlag == 0) {
        return;
      }

      int[] purpose = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      bool[,] jTParticipation = new bool[8, Global.MaximumHouseholdSize]; //jointTour x personDay.Sequence
      int jt = 0;
      int totalJointTours = 0;
      if (Global.Configuration.IsInEstimationMode) {
        foreach (IJointTourWrapper jointTour in householdDay.JointToursList) {
          jt++;
          if (jt <= 8) {
            purpose[jt] = jointTour.MainPurpose;
            if (jointTour.PersonSequence1 > 0 && jointTour.PersonSequence1 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence1] = true; }
            if (jointTour.PersonSequence2 > 0 && jointTour.PersonSequence2 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence2] = true; }
            if (jointTour.PersonSequence3 > 0 && jointTour.PersonSequence3 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence3] = true; }
            if (jointTour.PersonSequence4 > 0 && jointTour.PersonSequence4 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence4] = true; }
            if (jointTour.PersonSequence5 > 0 && jointTour.PersonSequence5 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence5] = true; }
            if (jointTour.PersonSequence6 > 0 && jointTour.PersonSequence6 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence6] = true; }
            if (jointTour.PersonSequence7 > 0 && jointTour.PersonSequence7 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence7] = true; }
            if (jointTour.PersonSequence8 > 0 && jointTour.PersonSequence8 <= Global.MaximumHouseholdSize) { jTParticipation[jt, jointTour.PersonSequence8] = true; }
            totalJointTours = jt;
          }
        }
        // update counts of person's joint tours by purpose
        int count = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          count++;
          foreach (TourWrapper tour in personDay.Tours) {
            if (count <= maxNumberParticipants && tour.JointTourSequence > 0) {
              personDay.JointTours++;
              if (tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
                personDay.EscortJointTours++;
              } else if (tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness) {
                personDay.PersonalBusinessJointTours++;
              } else if (tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
                personDay.ShoppingJointTours++;
              } else if (tour.DestinationPurpose == Global.Settings.Purposes.Meal) {
                personDay.MealJointTours++;
              } else if (tour.DestinationPurpose == Global.Settings.Purposes.Social) {
                personDay.SocialJointTours++;
              } else if (tour.DestinationPurpose == Global.Settings.Purposes.Recreation) {
                personDay.RecreationJointTours++;
              } else if (tour.DestinationPurpose == Global.Settings.Purposes.Medical) {
                personDay.MedicalJointTours++;
              }
            }
          }
        }
      }

      int nCallsForTour = 0;
      for (int i = 0; i < 8; i++) {

        ChoiceModelFactory.TotalTimesJointTourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        nCallsForTour++;
        purpose[nCallsForTour] = Global.ChoiceModelSession.Get<JointTourGenerationModel>().Run(householdDay, nCallsForTour, purpose[nCallsForTour]);
        if (purpose[nCallsForTour] > Global.Settings.Purposes.NoneOrHome) {
          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > Generated joint tour {0} with purpose {1}", nCallsForTour, purpose[nCallsForTour]);
          }

          // run tour participation model and create tour

          ChoiceModelFactory.TotalTimesJointTourParticipationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          int[] participants = Global.ChoiceModelSession.Get<JointTourParticipationModel>().Run(householdDay, nCallsForTour, purpose, jTParticipation);

          if (!Global.Configuration.IsInEstimationMode && !householdDay.IsValid) {
            return;
          }

          if (Global.Configuration.IsInEstimationMode) {
            int count = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
              count++;
              if (count <= maxNumberParticipants) {
                if (participants[count] == 1) {
                  //update the choices iteratively while the model runs in estimation
                  if (purpose[nCallsForTour] == Global.Settings.Purposes.Escort) {
                    personDay.CreatedEscortTours++;
                  } else if (purpose[nCallsForTour] == Global.Settings.Purposes.PersonalBusiness) {
                    personDay.CreatedPersonalBusinessTours++;
                  } else if (purpose[nCallsForTour] == Global.Settings.Purposes.Shopping) {
                    personDay.CreatedShoppingTours++;
                  } else if (purpose[nCallsForTour] == Global.Settings.Purposes.Meal) {
                    personDay.CreatedMealTours++;
                  } else if (purpose[nCallsForTour] == Global.Settings.Purposes.Social) {
                    personDay.CreatedSocialTours++;
                  } else if (purpose[nCallsForTour] == Global.Settings.Purposes.Recreation) {
                    personDay.CreatedRecreationTours++;
                  } else if (purpose[nCallsForTour] == Global.Settings.Purposes.Medical) {
                    personDay.CreatedMedicalTours++;
                  }
                }
              }
            }
          } else if (!Global.Configuration.IsInEstimationMode && !(purpose[nCallsForTour] == Global.Settings.Purposes.NoneOrHome)) {
            //create joint tour
            IJointTourWrapper jointTour = householdDay.CreateJointTour(householdDay, orderedPersonDays, participants, purpose[nCallsForTour]);
            int count = 0;
            foreach (PersonDayWrapper personDay in orderedPersonDays) {
              count++;
              if (count <= maxNumberParticipants) {
                if (participants[count] == 1) {
                  // create nonmandatory tour for person and associate it with JointTour
                  TourWrapper tour = (TourWrapper)personDay.GetNewTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey, purpose[nCallsForTour]);
                  tour.JointTourSequence = jointTour.Sequence;
                  jointTour.SetParticipantTourSequence(tour);
                  personDay.JointTours++;
                  if (jointTour.MainPurpose == Global.Settings.Purposes.Escort) {
                    personDay.EscortJointTours++;
                    personDay.CreatedEscortTours++;
                  } else if (jointTour.MainPurpose == Global.Settings.Purposes.PersonalBusiness) {
                    personDay.PersonalBusinessJointTours++;
                    personDay.CreatedPersonalBusinessTours++;
                  } else if (jointTour.MainPurpose == Global.Settings.Purposes.Shopping) {
                    personDay.ShoppingJointTours++;
                    personDay.CreatedShoppingTours++;
                  } else if (jointTour.MainPurpose == Global.Settings.Purposes.Meal) {
                    personDay.MealJointTours++;
                    personDay.CreatedMealTours++;
                  } else if (jointTour.MainPurpose == Global.Settings.Purposes.Social) {
                    personDay.SocialJointTours++;
                    personDay.CreatedSocialTours++;
                  } else if (jointTour.MainPurpose == Global.Settings.Purposes.Recreation) {
                    personDay.RecreationJointTours++;
                    personDay.CreatedRecreationTours++;
                  } else if (jointTour.MainPurpose == Global.Settings.Purposes.Medical) {
                    personDay.MedicalJointTours++;
                    personDay.CreatedMedicalTours++;
                  }
                }
              }
            }
          }
        }
        //conditionally break the generation loop
        if (Global.Configuration.IsInEstimationMode && nCallsForTour == totalJointTours + 1) {
          return;
        }
        if (!Global.Configuration.IsInEstimationMode && (purpose[nCallsForTour] == Global.Settings.Purposes.NoneOrHome || nCallsForTour == 5)) {
          return;
        }

        //update orderedPersonDays to account for people who are no longer eligible
        orderedPersonDays = householdDay.PersonDays.OrderBy(p => p.GetJointTourParticipationPriority()).ToList().Cast<PersonDayWrapper>();

        numberOfEligiblePersonsInHousehold = 0;
        foreach (PersonDayWrapper personDay in orderedPersonDays) {
          if (personDay.GetJointTourParticipationPriority() < 9) {
            numberOfEligiblePersonsInHousehold++;
          }
        }
        if (numberOfEligiblePersonsInHousehold <= 1 || !Global.Configuration.ShouldRunJointTourGenerationModel) {
          return;
        }
      }
    }

    private static void RunPersonDayModelSuite(PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      if (personDay.PatternType == Global.Settings.PatternTypes.Home) {
        return;
      }
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunPersonDayModelSuite for Household {0} Person {1}", householdDay.Household.Id, personDay.Person.Sequence);
      }

      if (Global.Configuration.ShouldRunHouseholdPersonDayPatternModel) {
        // determines if there are tours for a person's day
        // sets number of stops for a person's day

        ChoiceModelFactory.TotalTimesPersonDayPatternModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<PersonDayPatternModel>().Run(personDay, householdDay);
      }
      if (!Global.Configuration.ShouldRunPersonTourGenerationModel) {
        return;
      }

      int[] tourPurpose = new int[99];

      //set choice array tourPurpose for estimation
      if (Global.Configuration.IsInEstimationMode) {
        int count = 0;
        int createdEscortTours = personDay.CreatedEscortTours;
        int createdPersonalBusinessTours = personDay.CreatedPersonalBusinessTours;
        int createdShoppingTours = personDay.CreatedShoppingTours;
        int createdMealTours = personDay.CreatedMealTours;
        int createdSocialTours = personDay.CreatedSocialTours;
        int createdRecreationTours = personDay.CreatedRecreationTours;
        int createdMedicalTours = personDay.CreatedMedicalTours;
        foreach (TourWrapper tour in personDay.Tours) {
          if (tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
            if (createdEscortTours < personDay.EscortTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.Escort;
              createdEscortTours++;
            }
          } else if (tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness) {
            if (createdPersonalBusinessTours < personDay.PersonalBusinessTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.PersonalBusiness;
              createdPersonalBusinessTours++;
            }
          } else if (tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
            if (createdShoppingTours < personDay.ShoppingTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.Shopping;
              createdShoppingTours++;
            }
          } else if (tour.DestinationPurpose == Global.Settings.Purposes.Meal) {
            if (createdMealTours < personDay.MealTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.Meal;
              createdMealTours++;
            }
          } else if (tour.DestinationPurpose == Global.Settings.Purposes.Social) {
            if (createdSocialTours < personDay.SocialTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.Social;
              createdSocialTours++;
            }
          } else if (tour.DestinationPurpose == Global.Settings.Purposes.Recreation) {
            if (createdRecreationTours < personDay.RecreationTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.Recreation;
              createdRecreationTours++;
            }
          } else if (tour.DestinationPurpose == Global.Settings.Purposes.Medical) {
            if (createdMedicalTours < personDay.MedicalTours) {
              count++;
              tourPurpose[count] = Global.Settings.Purposes.Medical;
              createdMedicalTours++;
            }
          }
        }
      }

      int maxPurpose = 2;
      for (int count = 1; count <= 8; count++) {

        ChoiceModelFactory.TotalTimesPersonTourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        tourPurpose[count] = Global.ChoiceModelSession.Get<PersonTourGenerationModel>().Run(personDay, householdDay, maxPurpose, tourPurpose[count]);
        if (tourPurpose[count] == Global.Settings.Purposes.NoneOrHome || personDay.GetTotalCreatedTours() >= 8) {
          break;
        }
        maxPurpose = Math.Max(maxPurpose, tourPurpose[count]);
        if (tourPurpose[count] == Global.Settings.Purposes.Escort) {
          personDay.CreatedEscortTours++;
        } else if (tourPurpose[count] == Global.Settings.Purposes.PersonalBusiness) {
          personDay.CreatedPersonalBusinessTours++;
        } else if (tourPurpose[count] == Global.Settings.Purposes.Shopping) {
          personDay.CreatedShoppingTours++;
        } else if (tourPurpose[count] == Global.Settings.Purposes.Meal) {
          personDay.CreatedMealTours++;
        } else if (tourPurpose[count] == Global.Settings.Purposes.Social) {
          personDay.CreatedSocialTours++;
        } else if (tourPurpose[count] == Global.Settings.Purposes.Recreation) {
          personDay.CreatedRecreationTours++;
        } else if (tourPurpose[count] == Global.Settings.Purposes.Medical) {
          personDay.CreatedMedicalTours++;
        }
        if (!Global.Configuration.IsInEstimationMode) {
          TourWrapper tour = (TourWrapper)personDay.GetNewTour(Global.Settings.AddressTypes.Home, personDay.Household.ResidenceParcelId, personDay.Household.ResidenceZoneKey, tourPurpose[count]);
          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > > Generated Individual Tour {0} for Purpose {1}", tour.Sequence, tour.DestinationPurpose);
          }
        }
      }
    }

    private static void RunPartialJointHalfTourModelSuite(HouseholdDayWrapper householdDay, IPartialHalfTourWrapper partialJointHalfTour) {

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunPartialJointHalfTourModelSuite for Household {0} PartialHalfTour {1}", householdDay.Household.Id, partialJointHalfTour.Sequence);
      }

      if (householdDay.Household.Id == 30345 && partialJointHalfTour.Sequence == 1) {
      }
      // declare array variables 
      PersonDayWrapper[] participantDay = new PersonDayWrapper[9];
      TourWrapper[] tour = new TourWrapper[9];
      IHalfTour[] halfTour = new IHalfTour[9];

      //get partialJointHalfTour.TourSequence1 thru 8 tours into tour array
      participantDay[1] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence1);
      participantDay[2] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence2);
      participantDay[3] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence3);
      participantDay[4] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence4);
      participantDay[5] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence5);
      participantDay[6] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence6);
      participantDay[7] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence7);
      participantDay[8] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == partialJointHalfTour.PersonSequence8);
      //get partialJointHalfTour.PersonSequence1 thru 8 personDays into particpantDay array
      tour[1] = participantDay[1] == null ? null : (TourWrapper)participantDay[1].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence1);
      tour[2] = participantDay[2] == null ? null : (TourWrapper)participantDay[2].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence2);
      tour[3] = participantDay[3] == null ? null : (TourWrapper)participantDay[3].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence3);
      tour[4] = participantDay[4] == null ? null : (TourWrapper)participantDay[4].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence4);
      tour[5] = participantDay[5] == null ? null : (TourWrapper)participantDay[5].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence5);
      tour[6] = participantDay[6] == null ? null : (TourWrapper)participantDay[6].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence6);
      tour[7] = participantDay[7] == null ? null : (TourWrapper)participantDay[7].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence7);
      tour[8] = participantDay[8] == null ? null : (TourWrapper)participantDay[8].Tours.FirstOrDefault(t => t.Sequence == partialJointHalfTour.TourSequence8);

      //			if (householdDay.Household.Id == 7426) {
      //				int ii = 0;
      //			}

      int direction = partialJointHalfTour.Direction;

      if (Global.Configuration.IsInEstimationMode) {
        //sort the participants in order by decreasing number of trips on half tour
        int[] sortOrder = new int[9];
        int[] sortOrderClone = new int[9];
        sortOrder[0] = 0;
        sortOrderClone[1] = sortOrder[1] = tour[1] == null ? 999 : direction == 1 ? 20 - tour[1].HalfTour1Trips : 20 - tour[1].HalfTour2Trips;
        sortOrderClone[2] = sortOrder[2] = tour[2] == null ? 999 : direction == 1 ? 20 - tour[2].HalfTour1Trips : 20 - tour[2].HalfTour2Trips;
        sortOrderClone[3] = sortOrder[3] = tour[3] == null ? 999 : direction == 1 ? 20 - tour[3].HalfTour1Trips : 20 - tour[3].HalfTour2Trips;
        sortOrderClone[4] = sortOrder[4] = tour[4] == null ? 999 : direction == 1 ? 20 - tour[4].HalfTour1Trips : 20 - tour[4].HalfTour2Trips;
        sortOrderClone[5] = sortOrder[5] = tour[5] == null ? 999 : direction == 1 ? 20 - tour[5].HalfTour1Trips : 20 - tour[5].HalfTour2Trips;
        sortOrderClone[6] = sortOrder[6] = tour[6] == null ? 999 : direction == 1 ? 20 - tour[6].HalfTour1Trips : 20 - tour[6].HalfTour2Trips;
        sortOrderClone[7] = sortOrder[7] = tour[7] == null ? 999 : direction == 1 ? 20 - tour[7].HalfTour1Trips : 20 - tour[7].HalfTour2Trips;
        sortOrderClone[8] = sortOrder[8] = tour[8] == null ? 999 : direction == 1 ? 20 - tour[8].HalfTour1Trips : 20 - tour[8].HalfTour2Trips;

        Array.Sort(sortOrder, participantDay);
        Array.Sort(sortOrderClone, tour);
      }

      int[] halfTourTrips = new int[9];
      IParcelWrapper[] destination = new ParcelWrapper[9];
      int[] destinationSequence = new int[9];
      int[] destinationParcelID = new int[9];
      int[] travelers = new int[9];
      int[] tourWithDestination = new int[9];
      //determine the sequence number of each person's destination, taking into consideration that people with same tour destination have the same sequence
      destinationSequence[1] = 1;
      for (int i = 1; i <= partialJointHalfTour.Participants; i++) {
        if (Global.Configuration.IsInEstimationMode) {
          destinationParcelID[i] = tour[i].DestinationParcelId;
        } else {
          destinationParcelID[i] = tour[i].DestinationPurpose == Global.Settings.Purposes.Work ? tour[i].Person.UsualWorkParcelId : tour[i].Person.UsualSchoolParcelId;
        }
        if (i == 1) {
          destinationSequence[i] = 1;
        } else {
          destinationSequence[i] = destinationSequence[i - 1];
          if (!(destinationParcelID[i] == destinationParcelID[i - 1])) {
            destinationSequence[i]++;
          }
        }
      }
      //determine the number of travelers for each segment of the halftour
      for (int i = 1; i <= partialJointHalfTour.Participants; i++) {
        for (int j = 1; j <= destinationSequence[partialJointHalfTour.Participants] + 1; j++) {
          if (destinationSequence[i] <= j) {
            travelers[j]++;
          }
        }
      }

      //determine a tour with destination at each segment of the halftour
      for (int i = 1; i <= partialJointHalfTour.Participants; i++) {
        for (int j = 1; j <= destinationSequence[partialJointHalfTour.Participants]; j++) {
          if (destinationSequence[i] == j) {
            tourWithDestination[j] = i;
          }
        }
      }

      //if (!Global.Configuration.IsInEstimationMode) {
      //determine the number of trips in each participant's halfTour
      for (int i = 1; i <= partialJointHalfTour.Participants; i++) {
        halfTourTrips[i] = destinationSequence[partialJointHalfTour.Participants] - destinationSequence[i] + 1;
      }
      //determine the destination of each segment of the halfTour
      for (int j = 1; j <= destinationSequence[partialJointHalfTour.Participants]; j++) {
        for (int i = 1; i <= partialJointHalfTour.Participants; i++) {
          if (destinationSequence[i] == j) {
            destination[j] = tour[i].DestinationParcel;
            break;
          }
        }
      }
      //}   Note:  Use the above logic for estimation mode, which causes us to ignore trips other than drop-off/pickup trips on the halftour
      //else {
      //determine the number of trips in each participant's halfTour
      //	for (var i = 1; i <= partialJointHalfTour.Participants; i++) {
      //	halfTourTrips[i] = direction == 1 ? tour[i].HalfTour1Trips : tour[i].HalfTour2Trips;
      //}
      //determine the destination of each segment of the halfTour
      //for (var j = 1; j <= halfTourTrips[1]; j++) {
      //for (var i = 1; i <= partialJointHalfTour.Participants; i++) {
      //if (halfTourTrips[i] == halfTourTrips[1] - j + 1) {
      //destination[j] = tour[i].DestinationParcel;
      //break;
      //}
      //}
      //}

      //}

      //Model tour mode for escort, and that determines tour modes for other participants, by simplifying assumption
      int modetemp = 0;
      if (!tour[1].DestinationModeAndTimeHaveBeenSimulated) {
        if (tour[2].DestinationModeAndTimeHaveBeenSimulated) {
          if (tour[2].Mode == Global.Settings.Modes.HovPassenger) {
            modetemp = Global.Settings.Modes.HovDriver;
          } else if (tour[2].Mode == Global.Settings.Modes.Bike) {
            modetemp = Global.Settings.Modes.Bike;
          } else {
            modetemp = Global.Settings.Modes.Transit;
          }
          if (!(Global.Configuration.IsInEstimationMode)) {
            tour[1].Mode = modetemp;
          }
        }
          // model arrival and departure time too, but they will be changed later
          else {
          SetTourModeAndTime(householdDay, tour[1], 0, 0, 0);
        }

        if (householdDay.IsValid == false) {
          return;
        }
      }
      if (!(Global.Configuration.IsInEstimationMode)) {
        for (int i = 2; i <= partialJointHalfTour.Participants; i++) {
          if (!tour[i].DestinationModeAndTimeHaveBeenSimulated) {
            if (tour[1].Mode == Global.Settings.Modes.HovDriver) {
              tour[i].Mode = Global.Settings.Modes.HovPassenger;
            } else if (tour[1].Mode == Global.Settings.Modes.Transit) {
              tour[i].Mode = Global.Settings.Modes.Bike;
            } else if (tour[1].Mode == Global.Settings.Modes.Bike) {
              tour[i].Mode = Global.Settings.Modes.Bike;
            } else if (tour[1].Mode == Global.Settings.Modes.Walk) {
              tour[i].Mode = Global.Settings.Modes.Walk;
            } else {
              tour[i].Mode = Global.Settings.Modes.Bike;
            }
          }
        }
      }

      for (int i = 2; i <= partialJointHalfTour.Participants; i++) {
        string impedanceVariable;
        int mode;
        int pathType;
        if (tour[i - 1].Mode == Global.Settings.Modes.Walk) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.Walk;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[i - 1].Mode == Global.Settings.Modes.Bike) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.Bike;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[i - 1].Mode == Global.Settings.Modes.Sov) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.Sov;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[i - 1].Mode == Global.Settings.Modes.HovDriver) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.HovDriver;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[i - 1].Mode == Global.Settings.Modes.HovPassenger || tour[i - 1].Mode == Global.Settings.Modes.SchoolBus || tour[i - 1].Mode == Global.Settings.Modes.Other) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.HovPassenger;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[i - 1].Mode == Global.Settings.Modes.Transit || tour[i - 1].Mode == Global.Settings.Modes.ParkAndRide) {
          impedanceVariable = "time"; // TODO:  logic that uses this SHOULD also include initial wait time and transfer time
                                      //JLB 20160323 substitute HOV Passenger for Transit for now.  Logic that uses this should use transit total ivtime plus wait and transfer times 
                                      //mode = Global.Settings.Modes.Transit;
                                      //pathType = Global.Settings.PathTypes.LocalBus;
          mode = Global.Settings.Modes.HovPassenger;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.HovPassenger;
          pathType = Global.Settings.PathTypes.FullNetwork;
        }

        //double circuityDistance = Global.Configuration.UseShortDistanceNodeToNodeMeasures
        //                                        ? tour[i - 1].DestinationParcel.NodeToNodeDistance(tour[i].DestinationParcel)
        //                                        : (Global.Configuration.UseShortDistanceCircuityMeasures)
        //                                        ? tour[i - 1].DestinationParcel.CircuityDistance(tour[i].DestinationParcel)
        //                                        : Constants.DEFAULT_VALUE;

        // changed to deal with missing parcels in estimation model
        double circuityDistance = (Global.Configuration.IsInEstimationMode) ? Constants.DEFAULT_VALUE : tour[i - 1].DestinationParcel.CalculateShortDistance(tour[i].DestinationParcel);

        //if (!tour[i].DestinationModeAndTimeHaveBeenSimulated) {
        if (i == 2) {
          if (!tour[i].DestinationModeAndTimeHaveBeenSimulated) {
            // set tour TOD for i.  If i == 2 then arrival and departure time are modeled
            SetTourModeAndTime(householdDay, tour[i], tour[i].Mode, 0, 0);
            if (householdDay.IsValid == false) {
              return;
            }
            // set exact minute of tour destination arrival and departure times
            if (tour[i].HalfTourFromOrigin == null) {
              tour[i].SetHalfTours(1);
              if (!(Global.Configuration.IsInEstimationMode)) {
                IHalfTour hT = tour[i].GetHalfTour(1);
                TripWrapper trip = (TripWrapper)hT.Trips[0];
                trip.DestinationParcel = partialJointHalfTour.Participants > 2 ? tour[i + 1].DestinationParcel : tour[i].OriginParcel;
                trip.OriginParcel = trip.Tour.DestinationParcel;
                trip.Mode = tour[i].Mode;
                bool forTourTimesOnly = true;
                SetTripModeAndTime(householdDay, tour[i], trip, forTourTimesOnly);
                tour[i].DestinationArrivalTime = trip.DepartureTime;
              }
            }
            if (tour[i].HalfTourFromDestination == null) {
              tour[i].SetHalfTours(2);
              if (!(Global.Configuration.IsInEstimationMode)) {
                IHalfTour hT = tour[i].GetHalfTour(2);
                TripWrapper trip = (TripWrapper)hT.Trips[0];
                trip.DestinationParcel = partialJointHalfTour.Participants > 2 ? tour[i + 1].DestinationParcel : tour[i].OriginParcel;
                trip.OriginParcel = trip.Tour.DestinationParcel;
                trip.Mode = tour[i].Mode;
                bool forTourTimesOnly = true;
                SetTripModeAndTime(householdDay, tour[i], trip, forTourTimesOnly);
                tour[i].DestinationDepartureTime = trip.DepartureTime;
              }
            }
            // JLB 20130719  The following escape seems to hide a bug.  In particular a case with DySim version 2199
            // household 30345.  Tour mode was 5. The above code modeled trip mode = 1 on first half tour
            // and a trip departure time (tour dest arrivel time) of 749...the last minute of the modeled destination arrival/departure period
            // and the second half tour trip departure time was modeled as 1.
            if (tour[i].DestinationArrivalTime < Global.Settings.Times.FiveAM
                || tour[i].DestinationDepartureTime > Global.Settings.Times.EightPM
                || tour[i].DestinationDepartureTime < tour[i].DestinationArrivalTime + 60) {
              if (!Global.Configuration.IsInEstimationMode) {
                householdDay.IsValid = false;
              }

              return;
            }
          }
        }
        // i > 2
        else if (partialJointHalfTour.Paired) {
          if (!(Global.Configuration.IsInEstimationMode)) {
            //arrival and departure times are both determined (as below) rather than beign modeled.
            if (halfTourTrips[i] == halfTourTrips[i - 1]) {
              tour[i].DestinationArrivalTime = tour[i - 1].DestinationArrivalTime;
              tour[i].DestinationDepartureTime = tour[i - 1].DestinationDepartureTime;
            } else {
              tour[i].DestinationArrivalTime = tour[i - 1].DestinationArrivalTime
                   - (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0), tour[i - 1].DestinationArrivalTime - 1, tour[i].DestinationParcel, tour[i - 1].DestinationParcel, circuityDistance).Variable)
                   - 3;
              tour[i].DestinationDepartureTime = tour[i - 1].DestinationDepartureTime
                   + (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0), tour[i - 1].DestinationDepartureTime + 1, tour[i - 1].DestinationParcel, tour[i].DestinationParcel, circuityDistance).Variable)
                   + 3;
            }
          }
          // Mode and times were already set, but other properties need to be set by SetTourModeAndTime()
          SetTourModeAndTime(householdDay, tour[i], tour[i].Mode, tour[i].DestinationArrivalTime, tour[i].DestinationDepartureTime);
          if (householdDay.IsValid == false) {
            return;
          }
        } else if (direction == 1) {
          if (!(Global.Configuration.IsInEstimationMode)) {
            //arrival time is determined by i-1's arrival time and travel time by tour mode between i's destination and i-1's destination 
            if (halfTourTrips[i] == halfTourTrips[i - 1]) {
              tour[i].DestinationArrivalTime = tour[i - 1].DestinationArrivalTime;
            } else {
              tour[i].DestinationArrivalTime = tour[i - 1].DestinationArrivalTime
                   - (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0), tour[i - 1].DestinationArrivalTime - 1, tour[i].DestinationParcel, tour[i - 1].DestinationParcel, circuityDistance).Variable)
                   - 3;
            }
            //departure time is modeled
            //JLB 20140528 replaced the following line because tour mode is known at this point
            //SetTourModeAndTime(householdDay, tour[i], 0, destinationArrivalTime, 0);
          }
          if (!tour[i].DestinationModeAndTimeHaveBeenSimulated) {
            SetTourModeAndTime(householdDay, tour[i], tour[i].Mode, tour[i].DestinationArrivalTime, 0);
          } else {
            SetTourModeAndTime(householdDay, tour[i], tour[i].Mode, tour[i].DestinationArrivalTime, tour[i].DestinationDepartureTime);
          }
          if (householdDay.IsValid == false) {
            return;
          }
        } else if (direction == 2) {
          if (!(Global.Configuration.IsInEstimationMode)) {
            //departure time is determined by i-1's departure time and travel time by tour mode between i's destination and i-1's destination
            if (halfTourTrips[i] == halfTourTrips[i - 1]) {
              tour[i].DestinationDepartureTime = tour[i - 1].DestinationDepartureTime;
            } else {
              tour[i].DestinationDepartureTime = tour[i - 1].DestinationDepartureTime
                   + (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0), tour[i - 1].DestinationDepartureTime + 1, tour[i - 1].DestinationParcel, tour[i].DestinationParcel, circuityDistance).Variable)
                   + 3;
            }
            //arrival time is modeled
            //JLB 20140528 replaced the following line because tour mode is known at this point
            //SetTourModeAndTime(householdDay, tour[i], 0, 0, destinationDepartureTime);
          }
          if (!tour[i].DestinationModeAndTimeHaveBeenSimulated) {
            SetTourModeAndTime(householdDay, tour[i], tour[i].Mode, 0, tour[i].DestinationDepartureTime);
          } else {
            SetTourModeAndTime(householdDay, tour[i], tour[i].Mode, tour[i].DestinationArrivalTime, tour[i].DestinationDepartureTime);
          }
          if (householdDay.IsValid == false) {
            return;
          }
        }
        if (i > 2) {
          if (tour[i].HalfTourFromOrigin == null) {
            tour[i].SetHalfTours(1);
          }
          if (tour[i].HalfTourFromDestination == null) {
            tour[i].SetHalfTours(2);
          }
        }
        tour[i].DestinationModeAndTimeHaveBeenSimulated = true;
        //}
        halfTour[i] = tour[i].GetHalfTour(direction);

        //mbtrace
        if (Global.TraceResults) {
          Global.PrintFile.WriteLine("> > > > Tour results for participant {0} Destination {1} Mode {2} ArrivalTime {3} DepartureTime {4}",
                     i, tour[i].DestinationParcelId, tour[i].Mode, tour[i].DestinationArrivalTime, tour[i].DestinationDepartureTime);
        }
      }

      if (!tour[1].DestinationModeAndTimeHaveBeenSimulated) {

        string impedanceVariable;
        int mode;
        int pathType;
        if (tour[1].Mode == Global.Settings.Modes.Walk) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.Walk;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[1].Mode == Global.Settings.Modes.Bike) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.Bike;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[1].Mode == Global.Settings.Modes.Sov) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.Sov;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[1].Mode == Global.Settings.Modes.HovDriver) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.HovDriver;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[1].Mode == Global.Settings.Modes.HovPassenger || tour[1].Mode == Global.Settings.Modes.SchoolBus || tour[1].Mode == Global.Settings.Modes.Other) {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.HovPassenger;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else if (tour[1].Mode == Global.Settings.Modes.Transit || tour[1].Mode == Global.Settings.Modes.ParkAndRide) {
          impedanceVariable = "time";
          //JLB 20160323 substitute HOV Passenger for Transit for now.  Logic that uses this should use transit total ivtime plus wait and transfer times 
          //mode = Global.Settings.Modes.Transit;
          //pathType = Global.Settings.PathTypes.LocalBus;
          mode = Global.Settings.Modes.HovPassenger;
          pathType = Global.Settings.PathTypes.FullNetwork;
        } else {
          impedanceVariable = "time";
          mode = Global.Settings.Modes.HovPassenger;
          pathType = Global.Settings.PathTypes.FullNetwork;
        }

        //if (!(Global.Configuration.IsInEstimationMode)) {
        if (partialJointHalfTour.Paired) {
          //arrival and departure times are both determined (as below) rather than beign modeled.
          if (!(Global.Configuration.IsInEstimationMode)) {
            if (halfTourTrips[1] == halfTourTrips[2]) {
              tour[1].DestinationArrivalTime = tour[2].DestinationArrivalTime;
              tour[1].DestinationDepartureTime = tour[2].DestinationDepartureTime;
            } else {
              tour[1].DestinationArrivalTime = tour[2].DestinationArrivalTime
                   + (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0),
                              Math.Min(tour[2].DestinationArrivalTime + 4, 1440), tour[2].DestinationParcel, tour[1].DestinationParcel).Variable)
                   + 3;
              tour[1].DestinationDepartureTime = tour[2].DestinationDepartureTime
                   - (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0),
                              Math.Max(tour[2].DestinationDepartureTime - 4, 1), tour[1].DestinationParcel, tour[2].DestinationParcel).Variable)
                   - 3;
            }
          }
          // Mode and times were already set, but other properties need to be set by SetTourModeAndTime()
          SetTourModeAndTime(householdDay, tour[1], tour[1].Mode, tour[1].DestinationArrivalTime, tour[1].DestinationDepartureTime);
          if (householdDay.IsValid == false) {
            return;
          }
        } else if (direction == 1) {
          if (!(Global.Configuration.IsInEstimationMode)) {
            //int destinationArrivalTime = 0;
            //arrival time is determined by 2's arrival time and travel time by tour mode between 1's destination and 2's destination 
            if (halfTourTrips[1] == halfTourTrips[2]) {
              tour[1].DestinationArrivalTime = tour[2].DestinationArrivalTime;
            } else {
              //if (tour[2].DestinationParcel == null || tour[1].DestinationParcel == null) {
              //tour[2].DestinationParcel = tour[2].DestinationParcel;
              //}

              tour[1].DestinationArrivalTime = tour[2].DestinationArrivalTime
                   + (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0), tour[2].DestinationArrivalTime + 4, tour[2].DestinationParcel, tour[1].DestinationParcel).Variable)
                   + 3;
            }
            //departure time is modeled
            //JLB 20140528 replaced the following line because tour mode is known at this point
            //SetTourModeAndTime(householdDay, tour[1], 0, destinationArrivalTime, 0);
          }
          SetTourModeAndTime(householdDay, tour[1], tour[1].Mode, tour[1].DestinationArrivalTime, 0);
          if (householdDay.IsValid == false) {
            return;
          }
        } else if (direction == 2) {
          if (!(Global.Configuration.IsInEstimationMode)) {
            //departure time is determined by 2's departure time and travel time by tour mode between 1's destination and 2's destination
            if (halfTourTrips[1] == halfTourTrips[2]) {
              tour[1].DestinationDepartureTime = tour[2].DestinationDepartureTime;
            } else {
              if (tour[2].DestinationDepartureTime < 5) {
              }
              tour[1].DestinationDepartureTime = tour[2].DestinationDepartureTime
                                 - (int)Math.Round(ImpedanceRoster.GetValue(impedanceVariable, mode, pathType, Math.Max(Global.Configuration.VotLowMedium + 1, 10.0), tour[2].DestinationDepartureTime - 4, tour[1].DestinationParcel, tour[2].DestinationParcel).Variable)
                                 - 3;
            }
            //arrival time is modeled
          }
          SetTourModeAndTime(householdDay, tour[1], tour[1].Mode, 0, tour[1].DestinationDepartureTime);
          if (householdDay.IsValid == false) {
            return;
          }
        }
        if (tour[1].HalfTourFromOrigin == null) {
          tour[1].SetHalfTours(1);
        }
        if (tour[1].HalfTourFromDestination == null) {
          tour[1].SetHalfTours(2);
        }
        tour[1].DestinationModeAndTimeHaveBeenSimulated = true;
      }
      halfTour[1] = tour[1].GetHalfTour(direction);

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > Tour results for participant {0} Destination {1} Mode {2} ArrivalTime {3} DepartureTime {4}",
                 1, tour[1].DestinationParcelId, tour[1].Mode, tour[1].DestinationArrivalTime, tour[1].DestinationDepartureTime);
      }

      //update tours' time windows
      //JLB 20130716 comment out the temporary updating of time window
      //for (var i = 1; i <= partialJointHalfTour.Participants; i++) {
      //	UpdateTimeWindowForTourDestinationTimes(tour[i]);
      //}

      // simulate halftour

      IParcelWrapper stopLocation = null;
      int stopPurpose = Global.Settings.Purposes.NoneOrHome;
      int tripMode = Global.Settings.Modes.None;
      int tripDepartureTime = 0;
      for (int i = 1; i <= partialJointHalfTour.Participants; i++) {  // this loops on all participants
        for (int j = halfTourTrips[1] - halfTourTrips[i] + 1; j <= halfTourTrips[1]; j++) {  // this loops on the trips for the participant
          TripWrapper trip = (TripWrapper)halfTour[i].Trips[j + halfTourTrips[i] - halfTourTrips[1] - 1];
          halfTour[i].SimulatedTrips++;
          if (!Global.Configuration.IsInEstimationMode) {
            if (trip.IsHalfTourFromOrigin) {
              tour[i].HalfTour1Trips++;
            } else {
              tour[i].HalfTour2Trips++;
            }
          }
          stopPurpose = j == halfTourTrips[1] ? Global.Settings.Purposes.NoneOrHome : Global.Settings.Purposes.Escort;
          stopLocation = j == halfTourTrips[1] ? householdDay.Household.ResidenceParcel : destination[j + 1];
          if (i == 1) {
            if (tour[1].Mode == Global.Settings.Modes.HovDriver) {
              if (travelers[j] == 1) {
                tripMode = Global.Settings.Modes.Sov;
              } else {
                tripMode = Global.Settings.Modes.HovDriver;
              }
            } else {
              if (travelers[j] == 1) {
                tripMode = tour[1].Mode;
              } else {
                tripMode = tour[2].Mode;
              }
            }

          } else {
            tripMode = tour[i].Mode;
          }
          if (direction == 1) {
            tripDepartureTime = tour[tourWithDestination[j]].DestinationArrivalTime;
          } else {
            tripDepartureTime = tour[tourWithDestination[j]].DestinationDepartureTime;
          }
          RunPartialHalfTourTripModelSuite(tour[i], halfTour[i], trip, stopPurpose, stopLocation, tripMode, tripDepartureTime);
        }
        if (direction == Global.Settings.TourDirections.OriginToDestination) {
          tour[i].HalfTour1HasBeenSimulated = true;
        } else {
          tour[i].HalfTour2HasBeenSimulated = true;
        }
        tour[i].SetOriginTimes(direction);
        if (tour[i].HalfTour1HasBeenSimulated && tour[i].HalfTour2HasBeenSimulated) {
          UpdateTimeWindowForTourDestinationTimes(tour[i]);
        }
      }
    }

    private static void RunFullJointHalfTourModelSuite(HouseholdDayWrapper householdDay, IFullHalfTourWrapper fullJointHalfTour) {

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunFullJointHalfTourModelSuite for Household {0} FullHalfTour {1}", householdDay.Household.Id, fullJointHalfTour.Sequence);
      }

      if (householdDay.Household.Id == 81397 && householdDay.AttemptedSimulations == 2) { //80205) {// 3601) { //15454) { //2071) {
      }

      // declare array variables 
      PersonDayWrapper[] participantDay = new PersonDayWrapper[9];
      TourWrapper[] tour = new TourWrapper[9];

      //get fullJointHalfTour.TourSequence1 thru 8 tours into tour array
      participantDay[1] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence1);
      participantDay[2] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence2);
      participantDay[3] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence3);
      participantDay[4] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence4);
      participantDay[5] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence5);
      participantDay[6] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence6);
      participantDay[7] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence7);
      participantDay[8] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == fullJointHalfTour.PersonSequence8);
      //get fullJointHalfTour.PersonSequence1 thru 8 personDays into particpantDay array
      tour[1] = participantDay[1] == null ? null : (TourWrapper)participantDay[1].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence1);
      tour[2] = participantDay[2] == null ? null : (TourWrapper)participantDay[2].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence2);
      tour[3] = participantDay[3] == null ? null : (TourWrapper)participantDay[3].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence3);
      tour[4] = participantDay[4] == null ? null : (TourWrapper)participantDay[4].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence4);
      tour[5] = participantDay[5] == null ? null : (TourWrapper)participantDay[5].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence5);
      tour[6] = participantDay[6] == null ? null : (TourWrapper)participantDay[6].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence6);
      tour[7] = participantDay[7] == null ? null : (TourWrapper)participantDay[7].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence7);
      tour[8] = participantDay[8] == null ? null : (TourWrapper)participantDay[8].Tours.FirstOrDefault(t => t.Sequence == fullJointHalfTour.TourSequence8);

      int direction = fullJointHalfTour.Direction;

      int oldestNonEscortPersonID = 0;
      int oldestNonEscortAge = 0;
      int defaultMode = 0;
      int defaultArrivalTime = 0;
      int defaultDepartureTime = 0;
      IParcelWrapper destinationParcel = null;

      bool modeAndTimeAlreadySet = false;
      for (int i = 1; i <= 8; i++) {
        // if a non-escort participant's tour mode and time have been set already, use it
        if (!modeAndTimeAlreadySet && (tour[i] != null) && (tour[i].DestinationPurpose != Global.Settings.Purposes.Escort) && tour[i].DestinationModeAndTimeHaveBeenSimulated) {
          destinationParcel = tour[i].DestinationParcel;
          defaultMode = tour[i].Mode;
          defaultArrivalTime = tour[i].DestinationArrivalTime;
          defaultDepartureTime = tour[i].DestinationDepartureTime;
          modeAndTimeAlreadySet = true;
        }
      }
      // determine oldest nonEscort on halfTour
      int iOldest = 0;
      for (int i = 1; i <= 8; i++) {
        if (tour[i] != null && tour[i].DestinationPurpose != Global.Settings.Purposes.Escort && participantDay[i].Person.Age >= oldestNonEscortAge) {
          oldestNonEscortAge = participantDay[i].Person.Age;
          oldestNonEscortPersonID = participantDay[i].Person.Id;
          iOldest = i;
        }
      }
      // if tour mode and time haven't already been set, then set them for oldest nonEscort
      if (!modeAndTimeAlreadySet) {
        for (int i = 1; i <= 8; i++) {
          if ((participantDay[i] != null) && (oldestNonEscortPersonID == participantDay[i].Person.Id)) {
            destinationParcel = tour[i].DestinationPurpose == Global.Settings.Purposes.Work ? tour[i].Person.UsualWorkParcel : tour[i].Person.UsualSchoolParcel;
            SetTourDestination(householdDay, tour[i], destinationParcel);
            if (householdDay.IsValid == false) {
              return;
            }
            // reset destinationParcel in case TourDestinationModel reset it in error situations
            destinationParcel = tour[i].DestinationPurpose == Global.Settings.Purposes.Work ? tour[i].Person.UsualWorkParcel : tour[i].Person.UsualSchoolParcel;
            SetTourModeAndTime(householdDay, tour[i], 0, 0, 0);
            if (householdDay.IsValid == false) {
              return;
            }
            defaultMode = tour[i].Mode;
            defaultArrivalTime = tour[i].DestinationArrivalTime;
            defaultDepartureTime = tour[i].DestinationDepartureTime;
            modeAndTimeAlreadySet = true;
            tour[i].DestinationModeAndTimeHaveBeenSimulated = true;  // JLB 20140421  added this line
            break;
          }
        }
      }
      // loop to set tour mode and time for all participants on the half tour.
      int mode = 0;
      int arrivalTime = 0;
      int departureTime = 0;
      for (int i = 1; i <= 8; i++) {
        if (!(tour[i] == null)) {
          if (!Global.Configuration.IsInEstimationMode) {
            //use constrained version of tourmodetimemodel
            mode = Math.Max(defaultMode, tour[i].Mode);
            if (tour[i].DestinationPurpose == Global.Settings.Purposes.Escort) {
              if (direction == 1) {
                arrivalTime = defaultArrivalTime;
                departureTime = defaultArrivalTime + Global.Settings.Times.MinimumActivityDuration;
              } else {
                departureTime = defaultDepartureTime;
                arrivalTime = defaultDepartureTime - Global.Settings.Times.MinimumActivityDuration;
              }
            } else if (tour[i].DestinationModeAndTimeHaveBeenSimulated) {
              if (direction == 1) {
                arrivalTime = defaultArrivalTime;
                departureTime = Math.Max(tour[i].DestinationDepartureTime, arrivalTime + Global.Settings.Times.MinimumActivityDuration);
              } else {
                departureTime = defaultDepartureTime;
                arrivalTime = Math.Min(tour[i].DestinationArrivalTime, departureTime - Global.Settings.Times.MinimumActivityDuration);
              }
            } else {
              arrivalTime = defaultArrivalTime;
              departureTime = defaultDepartureTime;
            }
            SetTourDestination(householdDay, tour[i], destinationParcel);
            if (householdDay.IsValid == false) {
              return;
            }
            SetTourModeAndTime(householdDay, tour[i], mode, arrivalTime, departureTime);
            if (householdDay.IsValid == false) {
              return;
            }
          }
          tour[i].DestinationModeAndTimeHaveBeenSimulated = true;
        }
      }

      //update tours' time windows
      //JLB 20130716 comment out the temporary updating of time window
      //for (var i = 1; i <= fullJointHalfTour.Participants; i++) {
      //	if (tour[i] == null) {
      //		bool testbreak = true;
      //	}
      //	UpdateTimeWindowForTourDestinationTimes(tour[i]);
      //}

      // model halfTour for oldest nonEscortPerson
      TourWrapper sourceTour = null;
      for (int i = 1; i <= 8; i++) {
        if (!(participantDay[i] == null) && oldestNonEscortPersonID == participantDay[i].Person.Id) {
          ProcessHalfTours(tour[i], participantDay[i], householdDay, fullJointHalfTour.Direction, fullJointHalfTour.Direction);
          sourceTour = tour[i];
          if (fullJointHalfTour.Direction == Global.Settings.TourDirections.OriginToDestination) {
            tour[i].HalfTour1HasBeenSimulated = true;
          } else {
            tour[i].HalfTour2HasBeenSimulated = true;
          }
          if (!Global.Configuration.IsInEstimationMode) {
            tour[i].SetOriginTimes(fullJointHalfTour.Direction);
          }
          if (tour[i].HalfTour1HasBeenSimulated && tour[i].HalfTour2HasBeenSimulated) {
            UpdateTimeWindowForTourDestinationTimes(tour[i]);
          }

          break;
        }
      }
      // clone halfTour for other participants
      for (int i = 1; i <= 8; i++) {
        if (!(participantDay[i] == null) && !(oldestNonEscortPersonID == participantDay[i].Person.Id)) {
          if (!Global.Configuration.IsInEstimationMode) {
            CloneHalfTours(sourceTour, tour[i], fullJointHalfTour.Direction, fullJointHalfTour.Direction);
            tour[i].SetOriginTimes(fullJointHalfTour.Direction);
          }
          if (fullJointHalfTour.Direction == Global.Settings.TourDirections.OriginToDestination) {
            tour[i].HalfTour1HasBeenSimulated = true;
          } else {
            tour[i].HalfTour2HasBeenSimulated = true;
          }
          if (tour[i].HalfTour1HasBeenSimulated && tour[i].HalfTour2HasBeenSimulated) {
            UpdateTimeWindowForTourDestinationTimes(tour[i]);
          }
        }
      }

    }

    private static void RunMandatoryTourModelSuite(TourWrapper tour, PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
      //needs to include running subtour models of work tours;  see RunTourModels

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > RunMandatoryTourModelSuite for Household {0} Person {1} Tour {2}", householdDay.Household.Id, personDay.Person.Sequence, tour.Sequence);
      }

      //if (householdDay.Household.Id == 80142 && tour.PersonDay.Person.Sequence == 2 && tour.Sequence == 1) {
      if (householdDay.Household.Id == 81400 && tour.PersonDay.Person.Sequence == 2) {
      }

      tour.SetHomeBasedIsSimulated();

      if (!tour.DestinationModeAndTimeHaveBeenSimulated) {
        // JLB 201406 added this next conditional call
        if (Global.Configuration.ShouldRunTourDestinationModeTimeModel && tour.DestinationPurpose == Global.Settings.Purposes.Business) {
          SetTourDestinationModeAndTime(householdDay, tour, null, 0, 0, 0);
        } else {
          if (!(tour.DestinationParcelId > 0 && tour.DestinationParcelId == tour.Person.UsualWorkParcelId)) {
            SetTourDestination(householdDay, tour);
          }
          if (!tour.PersonDay.IsValid) {
            return;
          }

          if (tour.DestinationPurpose == Global.Settings.Purposes.Work) {
            GenerateSubtours(tour, householdDay);
            if (!tour.PersonDay.IsValid) {
              return;
            }
          }
          if (!tour.DestinationModeAndTimeHaveBeenSimulated) {
            SetTourModeAndTime(householdDay, tour, 0, 0, 0);
            if (!tour.PersonDay.IsValid) {
              return;
            }
          }
        }
      }
      tour.DestinationModeAndTimeHaveBeenSimulated = true;  // JLB 20140421 added this line


      if (householdDay.Household.Id == 80049 && tour.Person.Sequence == 1 && tour.Sequence == 1) {
      }

      //update tour's time windows
      //JLB 20130716 comment out the temporary updating of time window
      //UpdateTimeWindowForTourDestinationTimes(tour);

      // model half tours that have not already been simulated as part of joint half tours
      if (!tour.HalfTour1HasBeenSimulated || !tour.HalfTour2HasBeenSimulated) {
        int firstDirection = Global.Settings.TourDirections.OriginToDestination;
        int lastDirection = Global.Settings.TourDirections.DestinationToOrigin;
        if (tour.HalfTour1HasBeenSimulated) {
          firstDirection = Global.Settings.TourDirections.DestinationToOrigin;
        } else if (tour.HalfTour2HasBeenSimulated) {
          lastDirection = Global.Settings.TourDirections.OriginToDestination;
        }

        ChoiceModelFactory.TotalTimesTourTripModelsRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        RunTourTripModels(tour, personDay, householdDay, firstDirection, lastDirection);
        if (!personDay.IsValid) {
          return;
        }
      }
      UpdateTimeWindowForTourDestinationTimes(tour);


      ChoiceModelFactory.TotalTimesTourSubtourModelsRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

      RunSubtourModels(tour, personDay, householdDay);
      if (!personDay.IsValid) {
        return;
      }
    }

    private static void RunJointTourModelSuite(HouseholdDayWrapper householdDay, IJointTourWrapper jointTour) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > RunJointTourModelSuite for Household {0} JointTour {1}", householdDay.Household.Id, jointTour.Sequence);
      }

      if (householdDay.Household.Id == 80911) { //80205) {// 3601) { //15454) { //2071) {
      }


      // declare array variables 
      PersonDayWrapper[] participantDay = new PersonDayWrapper[9];
      TourWrapper[] tour = new TourWrapper[9];

      //get JointTour.TourSequence1 thru 8 tours into tour array
      participantDay[1] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence1);
      participantDay[2] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence2);
      participantDay[3] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence3);
      participantDay[4] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence4);
      participantDay[5] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence5);
      participantDay[6] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence6);
      participantDay[7] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence7);
      participantDay[8] = (PersonDayWrapper)householdDay.PersonDays.FirstOrDefault(pDay => pDay.Person.Sequence == jointTour.PersonSequence8);
      //get jointTour.PersonSequence1 thru 8 personDays into particpantDay array
      tour[1] = participantDay[1] == null ? null : (TourWrapper)participantDay[1].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence1);
      tour[2] = participantDay[2] == null ? null : (TourWrapper)participantDay[2].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence2);
      tour[3] = participantDay[3] == null ? null : (TourWrapper)participantDay[3].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence3);
      tour[4] = participantDay[4] == null ? null : (TourWrapper)participantDay[4].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence4);
      tour[5] = participantDay[5] == null ? null : (TourWrapper)participantDay[5].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence5);
      tour[6] = participantDay[6] == null ? null : (TourWrapper)participantDay[6].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence6);
      tour[7] = participantDay[7] == null ? null : (TourWrapper)participantDay[7].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence7);
      tour[8] = participantDay[8] == null ? null : (TourWrapper)participantDay[8].Tours.FirstOrDefault(t => t.Sequence == jointTour.TourSequence8);

      // set jointTour time window
      TimeWindow jointTourTimeWindow = new TimeWindow();
      for (int i = 1; i <= 8; i++) {
        if (!(tour[i] == null)) {
          jointTourTimeWindow.IncorporateAnotherTimeWindow(participantDay[i].TimeWindow);
        }
      }
      jointTour.TimeWindow = jointTourTimeWindow;

      int iOldest = 0;
      int oldestAge = 0;

      // set tour destination, mode and time for oldest participant
      // determine oldest
      for (int i = 1; i <= 8; i++) {
        if (!(tour[i] == null) && participantDay[i].Person.Age >= oldestAge) {
          oldestAge = participantDay[i].Person.Age;
          iOldest = i;
        }
      }

      // JLB 201406 added this conditional use of SetTourDestinaitonModeAndTime
      if (Global.Configuration.ShouldRunTourDestinationModeTimeModel) {
        SetTourDestinationModeAndTime(householdDay, tour[iOldest], null, 0, 0, 0);
      } else {

        SetTourDestination(householdDay, tour[iOldest]);

        if (!participantDay[iOldest].IsValid) { return; }

        SetTourModeAndTime(householdDay, tour[iOldest], 0, 0, 0);
      }

      if (!participantDay[iOldest].IsValid) { return; }

      // loop to set tour destination, mode and time for all participants on the tour.
      for (int i = 1; i <= 8; i++) {
        if (tour[i] != null) {
          if (i != iOldest) {
            if (!Global.Configuration.IsInEstimationMode) {
              // JLB 201406 added the conditional call to SetTourDestinationModeAndTime()
              if (Global.Configuration.ShouldRunTourDestinationModeTimeModel) {
                SetTourDestinationModeAndTime(householdDay, tour[i], tour[iOldest].DestinationParcel, tour[iOldest].Mode, tour[iOldest].DestinationArrivalTime, tour[iOldest].DestinationDepartureTime);
              } else {
                SetTourDestination(householdDay, tour[i], tour[iOldest].DestinationParcel);
                if (householdDay.IsValid == false) {
                  return;
                }
                int constrainedMode = (tour[iOldest].Mode == Global.Settings.Modes.HovDriver) ? Global.Settings.Modes.HovPassenger : tour[iOldest].Mode;
                SetTourModeAndTime(householdDay, tour[i], constrainedMode, tour[iOldest].DestinationArrivalTime, tour[iOldest].DestinationDepartureTime);
              }
              if (householdDay.IsValid == false) {
                return;
              }
            }
          }
          tour[i].DestinationModeAndTimeHaveBeenSimulated = true;
          //JLB 20130716 comment out the temporary updating of time window
          //UpdateTimeWindowForTourDestinationTimes(tour[i]);
        }
      }

      // model halfTours for oldest
      ProcessHalfTours(tour[iOldest], participantDay[iOldest], householdDay, Global.Settings.TourDirections.OriginToDestination, Global.Settings.TourDirections.DestinationToOrigin);

      if (!participantDay[iOldest].IsValid) { return; }

      tour[iOldest].HalfTour1HasBeenSimulated = true;
      tour[iOldest].HalfTour2HasBeenSimulated = true;
      UpdateTimeWindowForTourDestinationTimes(tour[iOldest]);

      if (!Global.Configuration.IsInEstimationMode) {
        tour[iOldest].SetOriginTimes();
      }

      // clone halfTours for other participants
      for (int i = 1; i <= 8; i++) {
        if (i != iOldest && participantDay[i] != null) {
          if (!Global.Configuration.IsInEstimationMode) {
            if (tour[i].Id == 21) {
            }
            CloneHalfTours(tour[iOldest], tour[i], Global.Settings.TourDirections.OriginToDestination, Global.Settings.TourDirections.DestinationToOrigin);
            if (tour[i].PersonDay.IsValid) {
              tour[i].SetOriginTimes();
              tour[i].HalfTour1HasBeenSimulated = true;
              tour[i].HalfTour2HasBeenSimulated = true;
              UpdateTimeWindowForTourDestinationTimes(tour[i]);

            }
          }
        }
      }
    }

    private static void RunNonMandatoryTourModelSuite(TourWrapper tour, PersonDayWrapper personDay, HouseholdDayWrapper householdDay) {
#if RELEASE
      try {
#endif
        //mbtrace
        if (Global.TraceResults) {
          Global.PrintFile.WriteLine("> > > > RunNonMandatoryTourModelSuite for Household {0} Person {1} Tour {2}", householdDay.Household.Id, personDay.Person.Sequence, tour.Sequence);
        }

        tour.SetHomeBasedIsSimulated();

        IParcelWrapper destinationParcel = null;
        int mode = 0;
        int destinationArrivalTime = 0;
        int destinationDepartureTime = 0;

        if (tour.FullHalfTour1Sequence > 0 || tour.FullHalfTour2Sequence > 0 || tour.JointTourSequence > 0 || tour.PartialHalfTour1Sequence > 0 || tour.PartialHalfTour2Sequence > 0) {
          destinationParcel = tour.DestinationParcel;
          mode = tour.Mode;
          destinationArrivalTime = tour.DestinationArrivalTime;
          destinationDepartureTime = tour.DestinationDepartureTime;
        }

        // JLB 201406 added the conditional call to SetTourDestinationModeAndTime
        if (Global.Configuration.ShouldRunTourDestinationModeTimeModel) {
          SetTourDestinationModeAndTime(householdDay, tour, destinationParcel, mode, destinationArrivalTime, destinationDepartureTime);
        } else {
          SetTourDestination(householdDay, tour, destinationParcel);
          if (!tour.PersonDay.IsValid) {
            return;
          }
          SetTourModeAndTime(householdDay, tour, mode, destinationArrivalTime, destinationDepartureTime);
        }
        if (!tour.PersonDay.IsValid) {
          return;
        }

        tour.DestinationModeAndTimeHaveBeenSimulated = true;  // JLB 20140421 added this line

        //JLB 20130716 comment out the temporary updating of time window
        //UpdateTimeWindowForTourDestinationTimes(tour);

        int firstDirection = Global.Settings.TourDirections.OriginToDestination;
        int lastDirection = Global.Settings.TourDirections.DestinationToOrigin;

        //return if both half tours have already been simulated;
        if (tour.JointTourSequence > 0 || (tour.DestinationPurpose == Global.Settings.Purposes.Escort && tour.FullHalfTour1Sequence > 0 && tour.FullHalfTour2Sequence > 0)) {
          return;
        }
        // only simulate escort half tours that have not already been simulated as part of fully joint half tours
        if (tour.DestinationPurpose == Global.Settings.Purposes.Escort && tour.FullHalfTour1Sequence > 0) {
          firstDirection = Global.Settings.TourDirections.DestinationToOrigin;
        } else if (tour.DestinationPurpose == Global.Settings.Purposes.Escort && tour.FullHalfTour2Sequence > 0) {
          lastDirection = Global.Settings.TourDirections.OriginToDestination;
        }

        ChoiceModelFactory.TotalTimesTourTripModelsRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        RunTourTripModels(tour, personDay, householdDay, firstDirection, lastDirection);
        if (!personDay.IsValid) {
          return;
        }
        UpdateTimeWindowForTourDestinationTimes(tour);


#if RELEASE
      } catch (Exception e) {
        throw new Framework.Exceptions.TourModelException(string.Format("Error running tour models for {0}.", tour), e);
      }
#endif


    }

    private static void UpdateTimeWindowForTourDestinationTimes(TourWrapper tour) {

      //if (Global.Configuration.IsInEstimationMode) {
      //	return;
      //}

      if (!Global.Configuration.IsInEstimationMode && tour.DestinationArrivalTime > tour.DestinationDepartureTime) {
        Global.PrintFile.WriteArrivalTimeGreaterThanDepartureTimeWarning("HouseholdChoiceModelRunner", "UpdateTimeWindowForTourDestinationTimes", tour.PersonDay.Id, tour.DestinationArrivalTime, tour.DestinationDepartureTime);
        tour.PersonDay.IsValid = false;

        return;
      }


      // # = busy :(
      // - = available :)

      // carves out a person's availability for the day in relation to the tour
      // person day availabilty [----###########----]

      if (tour.Household.Id == 80138 && tour.PersonDay.HouseholdDay.AttemptedSimulations == 0 && tour.Person.Sequence == 2) {
      }

      tour.PersonDay.TimeWindow.SetBusyMinutes(tour.DestinationArrivalTime, tour.DestinationDepartureTime + 1);

      if (tour.Subtours.Count == 0) {
        return;
      }

      // sets the availabilty for a tour's subtours 
      // tour availabilty [####-----------####]
      tour.TimeWindow.SetBusyMinutes(1, tour.DestinationArrivalTime + 1);
      tour.TimeWindow.SetBusyMinutes(tour.DestinationDepartureTime, Global.Settings.Times.MinutesInADay + 1);
    }

    private static void SetTourDestination(HouseholdDayWrapper householdDay, TourWrapper tour, IParcelWrapper constrainedParcel = null) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > SetTourDestination for Tour {0}", tour.Sequence);
      }

      if (householdDay.Household.Id == 80142 && tour.Person.Sequence == 2 && tour.Sequence == 1) {
      }


      //enhanced TourDestinationModel to handle constrainedParcel cases so that it can check for invalid data in estimation mode
      //if (constrainedParcel != null) {
      //	tour.DestinationParcel = constrainedParcel;
      //	tour.DestinationParcelId = constrainedParcel.Id;
      //	tour.DestinationZoneKey = constrainedParcel.ZoneId;
      //	return;
      //}

      if (tour.DestinationPurpose == Global.Settings.Purposes.Business) {
        if (Global.Configuration.ShouldRunTourDestinationModel) {
          // sets the destination for the business tour
          // not the usual work location, only some other location

          ChoiceModelFactory.TotalTimesTourDestinationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<TourDestinationModel>().Run(tour, householdDay, Global.Configuration.TourDestinationModelSampleSize, constrainedParcel);
        }

        return;
        //JLB 20130705 embedded school result in ActumTourDestinationModel so it would be checked in estimation mode for bad survey data
      } else if (tour.DestinationPurpose == Global.Settings.Purposes.School) {
        if (!Global.Configuration.IsInEstimationMode) {
          // sets the destination for the school tour
          //tour.DestinationParcelId = tour.Person.UsualSchoolParcelId;
          //tour.DestinationParcel = tour.Person.UsualSchoolParcel;
          //tour.DestinationZoneKey = tour.Person.UsualSchoolZoneKey;
          //tour.DestinationAddressType = Global.Settings.AddressTypes.UsualSchool;
          //add code to set simulated times of day for mode choice
          int tourCategory = tour.GetTourCategory();
          ChoiceModelUtility.DrawRandomTourTimePeriodsActum(tour, tourCategory);
        }

        return;
      } else {
        if (Global.Configuration.ShouldRunTourDestinationModel) {
          // sets the destination for the work tour
          // the usual work location or some another work location

          ChoiceModelFactory.TotalTimesTourDestinationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<TourDestinationModel>().Run(tour, householdDay, Global.Configuration.TourDestinationModelSampleSize, constrainedParcel);
        }

        return;
      }
    }

    private static void GenerateSubtours(TourWrapper tour, HouseholdDayWrapper householdDay) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > GenerateSubtours for Tour {0}", tour.Sequence);
      }

      // when the tour is to the usual work location then subtours for a work-based tour are created
      if (tour.Person.UsualWorkParcel == null || tour.DestinationParcel == null
                 || tour.DestinationParcel != tour.Person.UsualWorkParcel || !Global.Configuration.ShouldRunWorkBasedSubtourGenerationModel) {
        return;
      }

      if (Global.Configuration.IsInEstimationMode) {
        int nCallsForTour = 0;
        foreach (ITourWrapper subtour in tour.Subtours) {
          // -- in estimation mode --
          // sets the destination purpose of the subtour when in application mode

          ChoiceModelFactory.TotalTimesWorkBasedSubtourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          nCallsForTour++;
          Global.ChoiceModelSession.Get<WorkBasedSubtourGenerationModel>().Run(tour, householdDay, nCallsForTour, subtour.DestinationPurpose);
        }
        nCallsForTour++;

        ChoiceModelFactory.TotalTimesWorkBasedSubtourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<WorkBasedSubtourGenerationModel>().Run(tour, householdDay, nCallsForTour);
      } else {
        // creates the subtours for work tour 
        int nCallsForTour = 0;
        while (tour.Subtours.Count < 4 && tour.PersonDay.GetTotalCreatedTours() < 8) {
          // -- in application mode --
          // sets the destination purpose of the subtour

          ChoiceModelFactory.TotalTimesWorkBasedSubtourGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          nCallsForTour++;
          int destinationPurposeForSubtour = Global.ChoiceModelSession.Get<WorkBasedSubtourGenerationModel>().Run(tour, householdDay, nCallsForTour);

          if (destinationPurposeForSubtour == Global.Settings.Purposes.NoneOrHome) {
            break;
          }

          // the subtour is added to the tour's Subtours collection when the subtour's purpose is not NONE_OR_HOME
          tour.Subtours.Add(tour.CreateSubtour(tour.DestinationAddressType, tour.DestinationParcelId, tour.DestinationZoneKey, destinationPurposeForSubtour));
          tour.PersonDay.CreatedWorkBasedTours++;
        }

        tour.PersonDay.WorkBasedTours += tour.Subtours.Count;
      }
    }

    private static void SetTourModeAndTime(HouseholdDayWrapper householdDay, TourWrapper tour,
          int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime) {

      //if (householdDay.Household.Id == 80138 && tour.Person.Sequence == 2) {
      if (tour.Sequence > 10) {
      }

      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > SetTourModeAndTime for Tour {0}", tour.Sequence);
      }

      if (Global.Configuration.ShouldRunTourModeTimeModel) {
        if (tour.JointTourSequence > 0) {
        }

        ChoiceModelFactory.TotalTimesTourModeTimeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;
        if (tour.DestinationPurpose == Global.Settings.Purposes.Work) {
          //Global.ChoiceModelSession.Get<WorkTourModeTimeModel>().Run(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
          Global.ChoiceModelSession.Get<TourModeTimeModel>().Run(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else if (tour.DestinationPurpose == Global.Settings.Purposes.School) {
          //Global.ChoiceModelSession.Get<SchoolTourModeTimeModel>().Run(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
          Global.ChoiceModelSession.Get<TourModeTimeModel>().Run(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else if (!Global.Configuration.ShouldRunTourDestinationModeTimeModel) {
          Global.ChoiceModelSession.Get<TourModeTimeModel>().Run(householdDay, tour, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else {
          Global.ChoiceModelSession.Get<TourDestinationModeTimeModel>().Run(householdDay, tour, tour.DestinationParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        }
        if (!tour.PersonDay.IsValid) {
          return;
        }

        //set additional tour variables based on HTourModeTime object for chosen alternative
        HTourModeTime choice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);
        ITimeWindow timeWindow = (constrainedArrivalTime <= 0 && constrainedDepartureTime <= 0 && constrainedMode <= 0) ?
                    tour.GetRelevantTimeWindow(householdDay) : new TimeWindow();
        if (tour.Mode > 0 && tour.DestinationParcelId > 0 && tour.OriginParcelId > 0) {   // JLB 20150828 prevents exception caused by mode == 0 in estimation data set  //MB also changed to handle missing destinations
          HTourModeTime.SetImpedanceAndWindow(timeWindow, tour, choice, -1, -1.0);
        }
        if (choice != null && choice.LongestFeasibleWindow != null) {
          tour.DestinationArrivalBigPeriod = choice.ArrivalPeriod;
          tour.DestinationDepartureBigPeriod = choice.DeparturePeriod;
          tour.EarliestOriginDepartureTime = choice.LongestFeasibleWindow.Start;
          tour.LatestOriginArrivalTime = choice.LongestFeasibleWindow.End;
          tour.IndicatedTravelTimeToDestination = choice.TravelTimeToDestination;
          tour.IndicatedTravelTimeFromDestination = choice.TravelTimeFromDestination;
        }
        //				else {  JLB 20130705 trying to handle cases with bad data in estimation mode
        else if (!Global.Configuration.IsInEstimationMode) {
          tour.PersonDay.IsValid = false;
          householdDay.IsValid = false;
        }
        //mbtrace
        if (Global.TraceResults) {
          Global.PrintFile.WriteLine("> > > > > > Predicted Mode {0} ArrivalTime {1} DepartureTime {2} Valid {3}",
                              tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime, tour.PersonDay.IsValid);
        }
      }
    }

    private static void RunSubtourModelSuite(TourWrapper subtour, HouseholdDayWrapper householdDay) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > RunSubtourModelSuite for Subtour {0}", subtour.Sequence);
      }

      subtour.SetWorkBasedIsSimulated();

      if (Global.Configuration.ShouldRunTourDestinationModeTimeModel) {
        SetTourDestinationModeAndTime(householdDay, subtour, null, 0, 0, 0);
      } else {

        SetSubtourDestination(subtour, householdDay);

        if (!subtour.PersonDay.IsValid) {
          return;
        }

        SetSubtourModeAndTime(householdDay, subtour);

        if (!subtour.PersonDay.IsValid) {
          return;
        }
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

      if (subtour.ParentTour.Household.Id == 80138 && subtour.PersonDay.HouseholdDay.AttemptedSimulations == 0 && subtour.ParentTour.Person.Sequence == 2) {
      }

      subtour.ParentTour.TimeWindow.SetBusyMinutes(subtour.DestinationArrivalTime, subtour.DestinationDepartureTime + 1);
    }

    private static void SetSubtourDestination(TourWrapper subtour, HouseholdDayWrapper householdDay) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > SetSubtourGeneration for Subtour {0}", subtour.Sequence);
      }

      if (subtour.DestinationPurpose == Global.Settings.Purposes.Business) {
        if (Global.Configuration.ShouldRunTourDestinationModel) {
          // sets the destination for the business tour

          ChoiceModelFactory.TotalTimesBusinessSubtourDestinationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<TourDestinationModel>().Run(subtour, householdDay, Global.Configuration.TourDestinationModelSampleSize);
        }

        return;
      } else if (subtour.DestinationPurpose == Global.Settings.Purposes.School) {
        if (!Global.Configuration.IsInEstimationMode) {
          // sets the destination for the school subtour
          //subtour.DestinationParcelId = subtour.Person.UsualSchoolParcelId;
          //subtour.DestinationParcel = subtour.Person.UsualSchoolParcel;
          //subtour.DestinationZoneKey = subtour.Person.UsualSchoolZoneKey;
          //subtour.DestinationAddressType = Global.Settings.AddressTypes.UsualSchool;
          //add code to set simulated times of day for mode choice
          int subtourCategory = subtour.GetTourCategory();
          ChoiceModelUtility.DrawRandomTourTimePeriodsActum(subtour, subtourCategory);
        }

        return;
      } else {
        if (Global.Configuration.ShouldRunTourDestinationModel) {
          // sets the destination for the subtour

          ChoiceModelFactory.TotalTimesOtherSubtourDestinationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<TourDestinationModel>().Run(subtour, householdDay, Global.Configuration.TourDestinationModelSampleSize);
        }

        return;
      }
    }

    private static void SetSubtourModeAndTime(HouseholdDayWrapper householdDay, TourWrapper subtour) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > SetSubtourModeAndTime for Subtour {0}", subtour.Sequence);
      }

      if (subtour.ParentTour.Sequence == 1 && subtour.Person.Sequence == 2 && householdDay.Household.Id == 80767) {
      }

      if (Global.Configuration.ShouldRunTourModeTimeModel) {
        TimeWindow timeWindow = new TimeWindow();
        if (subtour.JointTourSequence > 0) {
          foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
            TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.JointTourSequence == subtour.JointTourSequence);
            if (!(tInJoint == null)) {
              // set jointTour time window
              timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
            }
          }
        } else if (subtour.FullHalfTour1Sequence > 0) {
          foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
            TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.FullHalfTour1Sequence == subtour.FullHalfTour1Sequence);
            if (!(tInJoint == null)) {
              // set jointTour time window
              timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
            }
          }
        } else if (subtour.FullHalfTour2Sequence > 0) {
          foreach (PersonDayWrapper pDay in householdDay.PersonDays) {
            TourWrapper tInJoint = (TourWrapper)pDay.Tours.Find(t => t.FullHalfTour2Sequence == subtour.FullHalfTour2Sequence);
            if (!(tInJoint == null)) {
              // set jointTour time window
              timeWindow.IncorporateAnotherTimeWindow(tInJoint.PersonDay.TimeWindow);
            }
          }
        } else if (subtour.ParentTour == null) {
          timeWindow.IncorporateAnotherTimeWindow(subtour.PersonDay.TimeWindow);
        } else {
          timeWindow.IncorporateAnotherTimeWindow(subtour.ParentTour.TimeWindow);
        }

        // timeWindow.SetBusyMinutes(Global.Settings.Times.EndOfRelevantWindow, Global.Settings.Times.MinutesInADay + 1);
        //JLB 20130811  commented out the above line to avoid cases where the only available time is after END_OF_RELEVANT_WINDOW
        //              causing zero available minutes.  Also added following test for no available time

        if (timeWindow.TotalAvailableMinutes(1, Global.Settings.Times.MinutesInADay) <= 0) {
          if (!Global.Configuration.IsInEstimationMode) {
            subtour.PersonDay.IsValid = false;
          }

          return;
        }


        //HTourModeTime.InitializeTourModeTimes();


        ChoiceModelFactory.TotalTimesTourModeTimeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<TourModeTimeModel>().Run(householdDay, subtour, 0, 0, 0);

        if (householdDay.IsValid == false) {
          return;
        }
        //set additional tour variables based on HTourModeTime object for chosen alternative
        HTourModeTime choice = new HTourModeTime(subtour.Mode, subtour.DestinationArrivalTime, subtour.DestinationDepartureTime);
        if (choice != null && subtour.DestinationParcel != null && subtour.OriginParcel != null && subtour.Mode > 0) {
          HTourModeTime.SetImpedanceAndWindow(timeWindow, subtour, choice, -1, -1.0);
        }
        if (choice != null && choice.LongestFeasibleWindow != null) {
          subtour.DestinationArrivalBigPeriod = choice.ArrivalPeriod;
          subtour.DestinationDepartureBigPeriod = choice.DeparturePeriod;
          subtour.EarliestOriginDepartureTime = choice.LongestFeasibleWindow.Start;
          subtour.LatestOriginArrivalTime = choice.LongestFeasibleWindow.End;
          subtour.IndicatedTravelTimeToDestination = choice.TravelTimeToDestination;
          subtour.IndicatedTravelTimeFromDestination = choice.TravelTimeFromDestination;
        } else {
          subtour.EarliestOriginDepartureTime = Constants.DEFAULT_VALUE;
          subtour.LatestOriginArrivalTime = Constants.DEFAULT_VALUE;
          subtour.IndicatedTravelTimeFromDestination = Constants.DEFAULT_VALUE;
          subtour.IndicatedTravelTimeToDestination = Constants.DEFAULT_VALUE;
        }

        //mbtrace
        if (Global.TraceResults) {
          Global.PrintFile.WriteLine("> > > > > > Predicted Mode {0} ArrivalTime {1} DepartureTime {2} Valid {3}",
                              subtour.Mode, subtour.DestinationArrivalTime, subtour.DestinationDepartureTime, subtour.PersonDay.IsValid);
        }
      }
    }

    private static void RunPartialHalfTourTripModelSuite(TourWrapper tour, IHalfTour halfTour, TripWrapper trip, int stopPurpose, IParcelWrapper stopLocation, int mode, int departureTime) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > RunPartialHalfTourTripModelSuite for HalfTour {0} Direction {1} Trip {2}", tour.Sequence, trip.Direction, trip.Sequence);
      }

      TripWrapper nextTrip = GeneratePartialHalfTourIntermediateStop(halfTour, trip, stopPurpose, stopLocation, mode, departureTime);

      SetPartialHalfTourIntermediateStopDestination(trip, nextTrip, stopPurpose, stopLocation, mode, departureTime);
      SetPartialHalfTourTripModeAndTime(tour, trip, stopPurpose, stopLocation, mode, departureTime);

      if (!trip.PersonDay.IsValid) {
        return;
      }

      //JB 20130616 commented out the following code that updates time window 
      //           because window is updated above in SetPartiaHalfTourTripModeAndTime()
      // retrieves window based on whether or not the trip's tour is home-based or work-based
      //			var timeWindow = tour.IsHomeBasedTour ? tour.PersonDay.TimeWindow : tour.ParentTour.TimeWindow;

      //			if (trip.IsHalfTourFromOrigin) {
      //				if (trip.Sequence == 1) {
      //					// occupies minutes in window between destination and stop
      //					timeWindow.SetBusyMinutes(trip.ArrivalTime, tour.DestinationArrivalTime + 1);
      //				}
      //				else {
      //					// occupies minutes in window from previous stop to stop
      //					timeWindow.SetBusyMinutes(trip.ArrivalTime, trip.PreviousTrip.DepartureTime + 1);
      //				}
      //			}
      //			else {
      //				if (trip.Sequence == 1) {
      //					// occupies minutes in window between destination and stop
      //					timeWindow.SetBusyMinutes(tour.DestinationDepartureTime, trip.ArrivalTime + 1);
      //				}
      //				else {
      //					// occupies minutes in window from previous stop to stop
      //					timeWindow.SetBusyMinutes(trip.PreviousTrip.DepartureTime, trip.ArrivalTime + 1);
      //				}
      //			}
    }

    private static TripWrapper GeneratePartialHalfTourIntermediateStop(IHalfTour halfTour, TripWrapper trip, int stopPurpose, IParcelWrapper stopLocation, int mode, int departureTime) {
      if (!Global.Configuration.ShouldRunIntermediateStopGenerationModel) {
        return null;
      }
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > GeneratePartialHalfTourTripIntermediateStop Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      TripWrapper nextTrip = null;

      if (Global.Configuration.IsInEstimationMode) {
        // -- in estimation mode --
        // sets the trip's destination purpose, determines whether or not a stop is generated in application mode
        // uses trip instead of nextTrip, deals with subtours with tour origin at work
        // need to set trip.IsToTourOrigin first
        //trip.IsToTourOrigin = trip.Sequence == trip.HalfTour.Trips.Count(); // last trip in half tour 
        //var intermediateStopPurpose = trip.IsToTourOrigin ? Global.Settings.Purposes.NoneOrHome : trip.DestinationPurpose;
        //nextTrip = trip.NextTrip;

        //if (intermediateStopPurpose != Global.Settings.Purposes.NoneOrHome) {
        //	ChoiceModelFactory.TotalTimesIntermediateStopGenerated++;
        //}
        // don't run generation model for partially joint half tour stops
        //if (trip.PersonDay.TotalStops > 0) {
        //	ChoiceModelFactory.TotalTimesIntermediateStopGenerationModelRun++;
        //	IntermediateStopGenerationModel.Run(trip, intermediateStopPurpose);
        //}
      } else {
        // -- in application mode --
        // sets the trip's destination purpose, determines whether or not a stop is generated

        // first, if it is the first trip on a park and ride half tour, then make it a change mode stop
        // TODO: this doesn't allow stops between the destination and the transit stop - can improve later
        //int intermediateStopPurpose = Global.Settings.Purposes.NoneOrHome;
        //don't need to deal with park and ride for partially joint half tours
        //if (trip.Sequence == 1 && trip.Tour.Mode == Global.Settings.Modes.ParkAndRide) {
        //	intermediateStopPurpose = Global.Settings.Purposes.ChangeMode;
        //	ChoiceModelFactory.TotalTimesChangeModeStopGenerated++;
        //}
        //else 
        //if (trip.PersonDay.TotalStops == 0) {
        //	intermediateStopPurpose = Global.Settings.Purposes.NoneOrHome;
        //}
        //else {
        //	ChoiceModelFactory.TotalTimesIntermediateStopGenerationModelRun++;
        //	intermediateStopPurpose = IntermediateStopGenerationModel.Run(trip);
        //}

        if (stopPurpose != Global.Settings.Purposes.NoneOrHome) {

          ChoiceModelFactory.TotalTimesIntermediateStopGenerated[ParallelUtility.threadLocalAssignedIndex.Value]++;

          int destinationPurposeForNextTrip = trip.DestinationPurpose;

          // creates the next trip in the half-tour 
          // the next trip's destination is set to the current trip's destination
          nextTrip = (TripWrapper)halfTour.CreateNextTrip(trip, stopPurpose, destinationPurposeForNextTrip);

          halfTour.Trips.Add(nextTrip);

          trip.DestinationAddressType = Global.Settings.AddressTypes.None;
          trip.DestinationPurpose = stopPurpose;
          trip.IsToTourOrigin = false;
        } else {
          trip.IsToTourOrigin = true;
        }
      }

      return nextTrip;
    }

    private static void SetPartialHalfTourIntermediateStopDestination(TripWrapper trip, TripWrapper nextTrip, int stopPurpose, IParcelWrapper stopLocation, int mode, int departureTime) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > SetPartialHalfTourTripIntermediateStopDestination Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (nextTrip == null || trip.IsToTourOrigin || !Global.Configuration.ShouldRunIntermediateStopLocationModel) {
        if (trip.IsToTourOrigin) {

          ChoiceModelFactory.TotalTimesTripIsToTourOrigin[ParallelUtility.threadLocalAssignedIndex.Value]++;

        } else if (nextTrip == null) {

          ChoiceModelFactory.TotalTimesNextTripIsNull[ParallelUtility.threadLocalAssignedIndex.Value]++;

        }

        if (trip.DestinationPurpose == Global.Settings.Purposes.NoneOrHome && Global.Configuration.IsInEstimationMode && Global.Configuration.EstimationModel == DaySim.ChoiceModels.Default.Models.IntermediateStopLocationModel.CHOICE_MODEL_NAME) {
          Global.PrintFile.WriteEstimationRecordExclusionMessage("ChoiceModelRunner", "SetIntermediateStopDestination", trip.Household.Id, trip.Person.Sequence, trip.Day, trip.Tour.Sequence, trip.Direction, trip.Sequence, 1);
        }

        return;
      }

      // sets the new destination for the trip

      //if (trip.DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
      // CHANGE_MODE location is always park and ride node for tour
      //var parkAndRideNode = ChoiceModelFactory.ParkAndRideNodeDao.Get(trip.Tour.ParkAndRideNodeId);

      //if (parkAndRideNode != null) {
      //trip.DestinationParcelId = parkAndRideNode.NearestParcelId;
      //trip.DestinationParcel = ChoiceModelFactory.Parcels[trip.DestinationParcelId];
      //trip.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[trip.DestinationParcel.ZoneId];
      //trip.DestinationAddressType = Global.Settings.AddressTypes.Other;

      //ChoiceModelFactory.TotalTimesChangeModeLocationSet++;
      //}
      //}
      //else {
      //	ChoiceModelFactory.TotalTimesIntermediateStopLocationModelRun++;
      //	IntermediateStopLocationModel.Run(trip, Global.Configuration.IntermediateStopLocationModelSampleSize);
      //}
      if (Global.Configuration.IsInEstimationMode) {
        return;
      }

      IParcelWrapper choice = stopLocation;

      trip.DestinationParcelId = choice.Id;
      trip.DestinationParcel = choice;
      trip.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[choice.ZoneId];
      trip.DestinationAddressType =
           choice.Id == trip.Person.UsualWorkParcelId
                ? Global.Settings.AddressTypes.UsualWorkplace
                : choice.Id == trip.Person.UsualSchoolParcelId ? Global.Settings.AddressTypes.UsualSchool : Global.Settings.AddressTypes.Other;

      nextTrip.OriginParcelId = trip.DestinationParcelId;
      nextTrip.OriginParcel = trip.DestinationParcel;
      nextTrip.OriginZoneKey = trip.DestinationZoneKey;
      nextTrip.SetOriginAddressType(trip.DestinationAddressType);
    }

    private static void SetPartialHalfTourTripModeAndTime(TourWrapper tour, TripWrapper trip, int stopPurpose, IParcelWrapper stopLocation, int mode, int departureTime) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > SetPartialHalfTourTripModeAndTime Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (Global.Configuration.ShouldRunTripModeModel) {
        // sets the trip's mode of travel to the destination
        //if (trip.DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
        // trips to change mode destination are always by transit
        //ChoiceModelFactory.TotalTimesChangeModeTransitModeSet++;
        //trip.Mode = Global.Settings.Modes.Transit;
        //}
        //else {
        //ChoiceModelFactory.TotalTimesTripModeModelRun++;
        //TripModeModel.Run(trip);
        if (!Global.Configuration.IsInEstimationMode) {
          trip.Mode = mode;
          trip.PathType = 0;
          //}
          //if (!trip.PersonDay.IsValid) {
          //	return;
        }
      }

      // sets the trip's destination arrival and departure times
      //if (trip.Sequence == 1) {
      //if (!Global.Configuration.IsInEstimationMode) {
      //trip.DepartureTime = trip.IsHalfTourFromOrigin ? tour.DestinationArrivalTime : tour.DestinationDepartureTime;
      //trip.UpdateTripValues();
      //}
      //}
      //else if (trip.OriginPurpose == Global.Settings.Purposes.ChangeMode) {
      //stay at park and ride lot assumed to be 3 minutes
      //if (!Global.Configuration.IsInEstimationMode) {
      //int endpoint;

      //if (trip.IsHalfTourFromOrigin) {
      //trip.DepartureTime = trip.PreviousTrip.ArrivalTime - 3;
      //endpoint = trip.DepartureTime + 1;
      //}
      //else {
      //trip.DepartureTime = trip.PreviousTrip.ArrivalTime + 3;
      //endpoint = trip.DepartureTime - 1;
      //}
      //if (trip.DepartureTime >= 1 && trip.DepartureTime <= Global.Settings.Times.MinutesInADay && trip.PersonDay.TimeWindow.EntireSpanIsAvailable(endpoint, trip.DepartureTime)) {
      //trip.UpdateTripValues();
      //}
      //else {
      //trip.PersonDay.IsValid = false;
      //}
      //}
      //}
      //else 
      if (!Global.Configuration.IsInEstimationMode) {
        if (Global.Configuration.ShouldRunTripTimeModel) {
          trip.DepartureTime = departureTime;
          //the next 3 are set just to make HUpdateTripValues work ok for now - may need to change later.
          trip.ArrivalTimeLimit = trip.Direction == 1 ? 1 : Global.Settings.Times.MinutesInADay;
          trip.EarliestDepartureTime = 1;
          trip.LatestDepartureTime = Global.Settings.Times.MinutesInADay;
          if (departureTime >= 1 && departureTime <= Global.Settings.Times.MinutesInADay) {
            trip.HUpdateTripValues();
          } else {
            trip.PersonDay.IsValid = false;
          }

        }
      }
    }

    private static void ProcessHalfTours(TourWrapper tour, PersonDayWrapper personDay, HouseholdDayWrapper householdDay, int firstDirection, int lastDirection) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > ProcessHalfTours Person {0} Tour {1} Directions {2} to {3}",
                     personDay.Person.Sequence, tour.Sequence, firstDirection, lastDirection);
      }

      if (tour.Household.Id == 80170 && tour.Person.Sequence == 1 && tour.Sequence == 1) {
      }

      // goes in two directions, from origin to destination and destination to origin
      for (int direction = firstDirection; direction <= lastDirection; direction++) {
        // creates origin and destination half-tours
        // creates or adds a trip to a tour based on application or estimation mode
        tour.SetHalfTours(direction);

        // the half-tour from the origin to destination or the half-tour from the destination to origin
        // depending on the direction
        IHalfTour halfTour = tour.GetHalfTour(direction);

        // halfTour.Trips will dynamically grow, so keep this in a for loop
        for (int i = 0; i < halfTour.Trips.Count; i++) {
          TripWrapper trip = (TripWrapper)halfTour.Trips[i];

#if RELEASE
          try {
#endif
            halfTour.SimulatedTrips++;

            if (trip.IsHalfTourFromOrigin) {
              tour.HalfTour1Trips++;
            } else {
              tour.HalfTour2Trips++;
            }


            ChoiceModelFactory.TotalTimesTripModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

            RunTripModelSuite(householdDay, personDay, tour, halfTour, trip);

            if (!trip.PersonDay.IsValid) {
              return;
            }
#if RELEASE
          } catch (Exception e) {
            throw new Framework.Exceptions.TripModelException(string.Format("Error running trip models for {0}.", trip), e);
          }
#endif
        }
      }
    }

    private static void RunTripModelSuite(HouseholdDayWrapper householdDay, PersonDayWrapper personDay, TourWrapper tour, IHalfTour halfTour, TripWrapper trip) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > RunTripModelSuite Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (householdDay.Household.Id == 80805 && personDay.Person.Sequence == 3 && tour.Sequence == 1
                && trip.Direction == 1 && trip.Sequence == 1 && householdDay.AttemptedSimulations == 1) {
      }


      TripWrapper nextTrip = GenerateIntermediateStop(halfTour, trip, householdDay);

      SetIntermediateStopDestination(trip, nextTrip, householdDay);
      if (!trip.PersonDay.IsValid) {
        return;
      }
      SetTripModeAndTime(householdDay, tour, trip);

      if (!trip.PersonDay.IsValid) {
        return;
      }

      // retrieves window based on whether or not the trip's tour is home-based or work-based
      ITimeWindow timeWindow = tour.IsHomeBasedTour ? tour.PersonDay.TimeWindow : tour.ParentTour.TimeWindow;
      int firstTime = 0;

      if (trip.IsHalfTourFromOrigin && trip.Sequence == 1) {
        // occupies minutes in window between destination and stop
        firstTime = tour.DestinationArrivalTime;
      } else if (!trip.IsHalfTourFromOrigin && trip.Sequence == 1) {
        // occupies minutes in window between destination and stop
        firstTime = tour.DestinationDepartureTime;
      } else {
        // occupies minutes in window from previous stop to stop
        firstTime = trip.GetPreviousTrip().DepartureTime;
      }

      if (tour.Household.Id == 80138 && tour.PersonDay.HouseholdDay.AttemptedSimulations == 0 && tour.Person.Sequence == 2) {
      }

      if (trip.IsHalfTourFromOrigin) {
        timeWindow.SetBusyMinutes(trip.ArrivalTime, firstTime + 1);
        if (tour.JointTourSequence > 0) {
          // updates joint tour time window
          householdDay.JointToursList.FirstOrDefault(jTour => jTour.Sequence == tour.JointTourSequence).TimeWindow.SetBusyMinutes(trip.ArrivalTime, firstTime + 1);
        }
      } else {
        timeWindow.SetBusyMinutes(firstTime, trip.ArrivalTime + 1);
        if (tour.JointTourSequence > 0) {
          // updates joint tour time window
          householdDay.JointToursList.FirstOrDefault(jTour => jTour.Sequence == tour.JointTourSequence).TimeWindow.SetBusyMinutes(firstTime, trip.ArrivalTime + 1);
        }
      }
    }

    private static TripWrapper GenerateIntermediateStop(IHalfTour halfTour, TripWrapper trip, HouseholdDayWrapper householdDay) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > GenerateIntermediateStops Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (!Global.Configuration.ShouldRunIntermediateStopGenerationModel) {
        return null;
      }

      TripWrapper nextTrip = null;

      if (Global.Configuration.IsInEstimationMode) {
        // -- in estimation mode --
        // sets the trip's destination purpose, determines whether or not a stop is generated in application mode
        // uses trip instead of nextTrip, deals with subtours with tour origin at work
        // need to set trip.IsToTourOrigin first
        trip.IsToTourOrigin = trip.Sequence == trip.HalfTour.Trips.Count(); // last trip in half tour 
        int intermediateStopPurpose = trip.IsToTourOrigin ? Global.Settings.Purposes.NoneOrHome : trip.DestinationPurpose;
        nextTrip = (TripWrapper)trip.GetNextTrip();

        if (trip.PersonDay.GetTotalStops() > 0 && halfTour.SimulatedTrips < 10) {

          ChoiceModelFactory.TotalTimesIntermediateStopGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<IntermediateStopGenerationModel>().Run(trip, householdDay, intermediateStopPurpose);

          if (intermediateStopPurpose != Global.Settings.Purposes.NoneOrHome) {

            ChoiceModelFactory.TotalTimesIntermediateStopGenerated[ParallelUtility.threadLocalAssignedIndex.Value]++;

            trip.PersonDay.IncrementSimulatedStops(intermediateStopPurpose);
          }
        }
      } else {
        // -- in application mode --
        // sets the trip's destination purpose, determines whether or not a stop is generated

        // first, if it is the first trip on a park and ride half tour, then make it a change mode stop
        // TODO: this doesn't allow stops between the destination and the transit stop - can improve later
        int intermediateStopPurpose;
        if (trip.Sequence == 1 &&
          (trip.Tour.Mode == Global.Settings.Modes.CarParkRideWalk ||
           trip.Tour.Mode == Global.Settings.Modes.CarParkRideBike ||
           trip.Tour.Mode == Global.Settings.Modes.CarParkRideShare)) {
          intermediateStopPurpose = Global.Settings.Purposes.ChangeMode;

          ChoiceModelFactory.TotalTimesChangeModeStopGenerated[ParallelUtility.threadLocalAssignedIndex.Value]++;

        } else if (trip.PersonDay.GetTotalStops() == 0) {
          intermediateStopPurpose = Global.Settings.Purposes.NoneOrHome;
        }
          // 201603 JLB  
          // 201903 MB For now, extended to all transit modes except walk-transit-walk and park and ride (handled above)
          else if (trip.Tour.Mode == Global.Settings.Modes.BikeOnTransit ||
                   trip.Tour.Mode == Global.Settings.Modes.BikeParkRideBike ||
                   trip.Tour.Mode == Global.Settings.Modes.BikeParkRideWalk ||
                   trip.Tour.Mode == Global.Settings.Modes.BikeParkRideShare ||
                   trip.Tour.Mode == Global.Settings.Modes.CarKissRideBike ||
                   trip.Tour.Mode == Global.Settings.Modes.CarKissRideWalk ||
                   trip.Tour.Mode == Global.Settings.Modes.CarKissRideShare ||
                   trip.Tour.Mode == Global.Settings.Modes.ShareRideBike ||
                   trip.Tour.Mode == Global.Settings.Modes.ShareRideWalk ||
                   trip.Tour.Mode == Global.Settings.Modes.ShareRideShare ||
                   trip.Tour.Mode == Global.Settings.Modes.WalkRideShare ||
                   trip.Tour.Mode == Global.Settings.Modes.WalkRideBike) {
          intermediateStopPurpose = Global.Settings.Purposes.NoneOrHome;
        } else {

          ChoiceModelFactory.TotalTimesIntermediateStopGenerationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;


          if (trip.Sequence > 5) {
          }

          intermediateStopPurpose = Global.ChoiceModelSession.Get<IntermediateStopGenerationModel>().Run(trip, householdDay);
        }

        if (intermediateStopPurpose != Global.Settings.Purposes.NoneOrHome && halfTour.SimulatedTrips < 10) {

          ChoiceModelFactory.TotalTimesIntermediateStopGenerated[ParallelUtility.threadLocalAssignedIndex.Value]++;

          int destinationPurposeForNextTrip = trip.DestinationPurpose;

          // creates the next trip in the half-tour 
          // the next trip's destination is set to the current trip's destination
          nextTrip = (TripWrapper)halfTour.CreateNextTrip(trip, intermediateStopPurpose, destinationPurposeForNextTrip);

          halfTour.Trips.Add(nextTrip);

          trip.DestinationAddressType = Global.Settings.AddressTypes.None;
          trip.DestinationPurpose = intermediateStopPurpose;
          trip.IsToTourOrigin = false;
        } else {
          trip.IsToTourOrigin = true;
        }
      }

      return nextTrip;
    }

    private static void SetIntermediateStopDestination(TripWrapper trip, TripWrapper nextTrip, HouseholdDayWrapper householdDay) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > SetIntermediateStopDestination Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (nextTrip == null || trip.IsToTourOrigin || !Global.Configuration.ShouldRunIntermediateStopLocationModel) {
        if (trip.IsToTourOrigin) {

          ChoiceModelFactory.TotalTimesTripIsToTourOrigin[ParallelUtility.threadLocalAssignedIndex.Value]++;

        } else if (nextTrip == null) {

          ChoiceModelFactory.TotalTimesNextTripIsNull[ParallelUtility.threadLocalAssignedIndex.Value]++;

        }

        if (trip.DestinationPurpose == Global.Settings.Purposes.NoneOrHome && Global.Configuration.IsInEstimationMode && Global.Configuration.EstimationModel == DaySim.ChoiceModels.Default.Models.IntermediateStopLocationModel.CHOICE_MODEL_NAME) {
          Global.PrintFile.WriteEstimationRecordExclusionMessage("ChoiceModelRunner", "SetIntermediateStopDestination", trip.Household.Id, trip.Person.Sequence, trip.Day, trip.Tour.Sequence, trip.Direction, trip.Sequence, 1);
        }

        return;
      }

      // sets the new destination for the trip

      if (trip.DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
        // CHANGE_MODE location is always park and ride node for tour
        IParkAndRideNodeWrapper parkAndRideNode = ChoiceModelFactory.ParkAndRideNodeDao.Get(trip.Tour.ParkAndRideNodeId);

        if (parkAndRideNode != null) {
          trip.DestinationParcelId = parkAndRideNode.NearestParcelId;
          trip.DestinationParcel = ChoiceModelFactory.Parcels[trip.DestinationParcelId];
          //trip.DestinationZoneKey = ChoiceModelFactory.ZoneKeys[trip.DestinationParcel.ZoneId];
          trip.DestinationZoneKey = parkAndRideNode.ZoneId;
          trip.DestinationAddressType = Global.Settings.AddressTypes.Other;


          ChoiceModelFactory.TotalTimesChangeModeLocationSet[ParallelUtility.threadLocalAssignedIndex.Value]++;

        }
      } else {

        ChoiceModelFactory.TotalTimesIntermediateStopLocationModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

        Global.ChoiceModelSession.Get<IntermediateStopLocationModel>().Run(trip, householdDay, Global.Configuration.IntermediateStopLocationModelSampleSize);
      }
      if (Global.Configuration.IsInEstimationMode) {
        return;
      }

      nextTrip.OriginParcelId = trip.DestinationParcelId;
      nextTrip.OriginParcel = trip.DestinationParcel;
      nextTrip.OriginZoneKey = trip.DestinationZoneKey;
      nextTrip.SetOriginAddressType(trip.DestinationAddressType);
    }

    private static void SetTripModeAndTime(HouseholdDayWrapper householdDay, TourWrapper tour, TripWrapper trip, bool forTourTimesOnly = false) {
      //mbtrace
      //			if (Global.TraceResults) Global.PrintFile.WriteLine("> > > > > > SetTripModeAndTime Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > SetTripModeAndTime Tour {0} Direction {1} Trip {2}", tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (Global.Configuration.IsInEstimationMode && (tour.DestinationArrivalBigPeriod == null || tour.DestinationDepartureBigPeriod == null)) {
        return;
      }


      if (tour.Household.Id == 81100 && tour.Person.Sequence == 3 && tour.Sequence == 1 && trip.Direction == 2 && trip.Sequence == 1) {
      }

      //set bounds on departure time window as new trip properties

      //set time needed from intermediate stop to tour origin
      int minimumTimeForIntermediateStop = 0;
      if (!trip.IsToTourOrigin && trip.DestinationParcel != null && tour.OriginParcel != null) {

        double fastestTravelTime = ImpedanceRoster.GetValue("time", Global.Settings.Modes.Hov3, Global.Settings.PathTypes.FullNetwork, 20.0, 1,
                    trip.IsHalfTourFromOrigin ? tour.OriginParcel : trip.DestinationParcel,
                    trip.IsHalfTourFromOrigin ? trip.DestinationParcel : tour.OriginParcel,
                    Constants.DEFAULT_VALUE).Variable;

        minimumTimeForIntermediateStop = (int)(fastestTravelTime + 0.5) + Global.Settings.Times.MinimumActivityDuration;
      }
      // HalfTour  Sequence   End         Equation
      //  1st         1st     Earliest    MAX(DestinationArrivalBigPeriod.Start , TourEarliestOriginDepartueTime + MinimumTimeForIntermediateStop)
      //  1st         1st     Latest      MIN(DestinationArrivalBigPeriod.End , TourLatestOriginArrivalTime - TourIndicatedTravelTimeFromDestination - MinimumActivityDuration)

      //  1st         Other   Earliest    TourEarliestOriginDepartueTime + MinimumTimeForIntermediateStop
      //  1st         Other   Latest      PreviousTripArrivalTime - MinimumActivityDuration

      //  2nd         1st     Earliest    MAX(DestinationDepartureBigPeriod.Start , DestinationArrivalTime + MinimumActivityDuration
      //  2nd         1st     Latest      MIN(DestinationDepartureBigPeriod.End , TourLatestOriginArrivalTime - MinimumTimeForIntermediateStop)

      //  2nd         Other   Earliest    PreviousTripArrivalTime + MinimumActivityDuration
      //  2nd         Other   Latest      LatestOriginArrivalTime - MinimumTimeForIntermediateStop 

      trip.EarliestDepartureTime = trip.Sequence == 1  // first simulated trip in half tour has to use tour big period
           ? (trip.IsHalfTourFromOrigin
                    ? Math.Max(tour.DestinationArrivalBigPeriod.Start, tour.EarliestOriginDepartureTime + minimumTimeForIntermediateStop)
                    : Math.Max(tour.DestinationDepartureBigPeriod.Start, tour.DestinationArrivalTime + Global.Settings.Times.MinimumActivityDuration))
           : (trip.IsHalfTourFromOrigin
                    ? tour.EarliestOriginDepartureTime + minimumTimeForIntermediateStop
                    : trip.GetPreviousTrip().ArrivalTime + Global.Settings.Times.MinimumActivityDuration);

      trip.LatestDepartureTime = trip.Sequence == 1 // first simulated trip in half tour has to use tour big period
              ? (trip.IsHalfTourFromOrigin
                       ? Math.Min(tour.DestinationArrivalBigPeriod.End, tour.LatestOriginArrivalTime - (int)(tour.IndicatedTravelTimeFromDestination + 0.5) - Global.Settings.Times.MinimumActivityDuration)
                       : Math.Min(tour.DestinationDepartureBigPeriod.End, tour.LatestOriginArrivalTime - minimumTimeForIntermediateStop))
               : (trip.IsHalfTourFromOrigin
                       ? trip.GetPreviousTrip().ArrivalTime - Global.Settings.Times.MinimumActivityDuration
                       : tour.LatestOriginArrivalTime - minimumTimeForIntermediateStop);

      trip.ArrivalTimeLimit = trip.IsHalfTourFromOrigin
            ? tour.EarliestOriginDepartureTime + minimumTimeForIntermediateStop
            : tour.LatestOriginArrivalTime - minimumTimeForIntermediateStop;


      if (Global.Configuration.ShouldRunTripModeModel) {
        // sets the trip's mode of travel to the destination

        if (trip.DestinationPurpose == Global.Settings.Purposes.ChangeMode) {
          // trips to change mode destination are always by transit

          ChoiceModelFactory.TotalTimesChangeModeTransitModeSet[ParallelUtility.threadLocalAssignedIndex.Value]++;

          trip.Mode = Global.Settings.Modes.Transit;
        }
        // 201603 JLB
        //				else if (trip.Tour.Mode == Global.Settings.Modes.BikeOnTransit || trip.Tour.Mode == Global.Settings.Modes.BikeParkRideBike
        //					|| trip.Tour.Mode == Global.Settings.Modes.BikeParkRideWalk || trip.Tour.Mode == Global.Settings.Modes.WalkRideBike) {
        else if (tour.Mode >= Global.Settings.Modes.WalkRideBike && tour.Mode <= Global.Settings.Modes.MaxMode) {
          trip.Mode = Global.Settings.Modes.Transit;
        } else if (tour.JointTourSequence > 0) {
          trip.Mode = tour.Mode;
        } else {

          ChoiceModelFactory.TotalTimesTripModeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<TripModeModel>().Run(householdDay, trip);

          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > > > > Predicted Trip Mode {0} Valid {1}",
                                  trip.Mode, trip.PersonDay.IsValid);
          }
        }
        if (!trip.PersonDay.IsValid) {
          return;
        }
      }

      // sets the trip's destination arrival and departure times
      if (trip.OriginPurpose == Global.Settings.Purposes.ChangeMode) {
        //stay at park and ride lot assumed to be 3 minutes
        if (!Global.Configuration.IsInEstimationMode) {
          int endpoint;

          if (trip.IsHalfTourFromOrigin) {
            trip.DepartureTime = trip.GetPreviousTrip().ArrivalTime - Global.Settings.Times.MinimumActivityDuration;
            endpoint = trip.DepartureTime + 1;
          } else {
            trip.DepartureTime = trip.GetPreviousTrip().ArrivalTime + Global.Settings.Times.MinimumActivityDuration;
            endpoint = trip.DepartureTime - 1;
          }
          if (trip.DepartureTime >= 1 && trip.DepartureTime <= Global.Settings.Times.MinutesInADay && trip.PersonDay.TimeWindow.EntireSpanIsAvailable(endpoint, trip.DepartureTime)) {
            if (!forTourTimesOnly) {
              trip.HUpdateTripValues();
            }
          } else {
            trip.PersonDay.IsValid = false;
          }
        }
      } else {
        if (Global.Configuration.ShouldRunTripTimeModel) {

          ChoiceModelFactory.TotalTimesTripTimeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

          Global.ChoiceModelSession.Get<TripTimeModel>().Run(householdDay, trip);

          //mbtrace
          if (Global.TraceResults) {
            Global.PrintFile.WriteLine("> > > > > > Predicted Trip DepartureTime {0} Valid {1}",
                                  trip.DepartureTime, trip.PersonDay.IsValid);
          }
          if (trip.PersonDay.IsValid) {
            if (!forTourTimesOnly) {
              //if (trip.Mode == Global.Settings.Modes.Walk || trip.Mode == Global.Settings.Modes.Bike) {
              //}
              // 201603 JLB
              if (trip.Tour.Mode > Global.Settings.Modes.WalkRideWalk) {
                trip.HPTBikeDriveTransitTourUpdateTripValues();
              } else {
                trip.HUpdateTripValues();
              }
            }
          }
        }
      }
    }

    private static void CloneHalfTours(TourWrapper sourceTour, TourWrapper tour, int firstDirection, int lastDirection) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > CloneHalfTours Tour {0} Directions {1} to {2}", tour.Sequence, firstDirection, lastDirection);
      }

      // goes in two directions, from origin to destination and destination to origin
      for (int direction = firstDirection; direction <= lastDirection; direction++) {
        // creates origin and destination half-tours
        // creates or adds a trip to a tour based on application or estimation mode
        tour.SetHalfTours(direction);

        // the half-tour from the origin to destination or the half-tour from the destination to origin
        // depending on the direction
        IHalfTour halfTour = tour.GetHalfTour(direction);
        IHalfTour sourceHalfTour = sourceTour.GetHalfTour(direction);

        // halfTour.Trips will dynamically grow, so keep this in a for loop
        for (int i = 0; i < sourceHalfTour.Trips.Count; i++) {
          TripWrapper trip = (TripWrapper)halfTour.Trips[i];
          TripWrapper sourceTrip = (TripWrapper)sourceHalfTour.Trips[i];

#if RELEASE
          try {
#endif
            halfTour.SimulatedTrips++;

            if (trip.IsHalfTourFromOrigin) {
              tour.HalfTour1Trips++;
            } else {
              tour.HalfTour2Trips++;
            }


            ChoiceModelFactory.TotalTimesTripModelSuiteRun[ParallelUtility.threadLocalAssignedIndex.Value]++;

            RunTripCloneSuite(sourceTour, sourceHalfTour, sourceTrip, tour, halfTour, trip);

            if (!trip.PersonDay.IsValid) {
              return;
            }
#if RELEASE
          } catch (Exception e) {
            throw new Framework.Exceptions.TripModelException(string.Format("Error running trip models for {0}.", trip), e);
          }
#endif
        }
      }
    }

    private static void RunTripCloneSuite(TourWrapper sourceTour, IHalfTour sourceHalfTour, TripWrapper sourceTrip, TourWrapper tour, IHalfTour halfTour, TripWrapper trip) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > RunTripCloneSuite Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      TripWrapper nextTrip = GenerateIntermediateStopClone(sourceHalfTour, sourceTrip, halfTour, trip);

      CloneIntermediateStopDestination(sourceTrip, trip, nextTrip);
      CloneTripModeAndTime(sourceTour, sourceTrip, tour, trip);

      if (!trip.PersonDay.IsValid) {
        return;
      }

      // retrieves window based on whether or not the trip's tour is home-based or work-based
      ITimeWindow timeWindow = tour.IsHomeBasedTour ? tour.PersonDay.TimeWindow : tour.ParentTour.TimeWindow;

      if (tour.Household.Id == 80138 && tour.PersonDay.HouseholdDay.AttemptedSimulations == 0 && tour.Person.Sequence == 2) {
      }

      if (trip.IsHalfTourFromOrigin) {
        if (trip.Sequence == 1) {
          // occupies minutes in window between destination and stop
          timeWindow.SetBusyMinutes(trip.ArrivalTime, tour.DestinationArrivalTime + 1);
        } else {
          // occupies minutes in window from previous stop to stop
          timeWindow.SetBusyMinutes(trip.ArrivalTime, trip.GetPreviousTrip().DepartureTime + 1);
        }
      } else {
        if (trip.Sequence == 1) {
          // occupies minutes in window between destination and stop
          timeWindow.SetBusyMinutes(tour.DestinationDepartureTime, trip.ArrivalTime + 1);
        } else {
          // occupies minutes in window from previous stop to stop
          timeWindow.SetBusyMinutes(trip.GetPreviousTrip().DepartureTime, trip.ArrivalTime + 1);
        }
      }
    }

    private static TripWrapper GenerateIntermediateStopClone(IHalfTour sourceHalfTour, TripWrapper sourceTrip, IHalfTour halfTour, TripWrapper trip) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > GenerateIntermediateStopClone Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      TripWrapper nextTrip = null;

      int intermediateStopPurpose = sourceTrip.DestinationPurpose;

      if (intermediateStopPurpose != Global.Settings.Purposes.NoneOrHome) {

        ChoiceModelFactory.TotalTimesIntermediateStopGenerated[ParallelUtility.threadLocalAssignedIndex.Value]++;

        int destinationPurposeForNextTrip = trip.DestinationPurpose;

        // creates the next trip in the half-tour 
        // the next trip's destination is set to the current trip's destination
        nextTrip = (TripWrapper)halfTour.CreateNextTrip(trip, intermediateStopPurpose, destinationPurposeForNextTrip);

        halfTour.Trips.Add(nextTrip);

        trip.DestinationAddressType = Global.Settings.AddressTypes.None;
        trip.DestinationPurpose = intermediateStopPurpose;
        trip.IsToTourOrigin = false;
      } else {
        trip.IsToTourOrigin = true;
      }
      return nextTrip;
    }

    private static void CloneIntermediateStopDestination(TripWrapper sourceTrip, TripWrapper trip, TripWrapper nextTrip) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > CloneIntermediateStopDestination Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      if (nextTrip == null || trip.IsToTourOrigin || !Global.Configuration.ShouldRunIntermediateStopLocationModel) {
        if (trip.IsToTourOrigin) {

          ChoiceModelFactory.TotalTimesTripIsToTourOrigin[ParallelUtility.threadLocalAssignedIndex.Value]++;

        } else if (nextTrip == null) {

          ChoiceModelFactory.TotalTimesNextTripIsNull[ParallelUtility.threadLocalAssignedIndex.Value]++;

        }

        if (trip.DestinationPurpose == Global.Settings.Purposes.NoneOrHome && Global.Configuration.IsInEstimationMode && Global.Configuration.EstimationModel == DaySim.ChoiceModels.Default.Models.IntermediateStopLocationModel.CHOICE_MODEL_NAME) {
          Global.PrintFile.WriteEstimationRecordExclusionMessage("ChoiceModelRunner", "SetIntermediateStopDestination", trip.Household.Id, trip.Person.Sequence, trip.Day, trip.Tour.Sequence, trip.Direction, trip.Sequence, 1);
        }

        return;
      }

      trip.DestinationParcelId = sourceTrip.DestinationParcelId;
      trip.DestinationParcel = sourceTrip.DestinationParcel;
      trip.DestinationZoneKey = sourceTrip.DestinationZoneKey;
      trip.DestinationAddressType = sourceTrip.DestinationAddressType;

      nextTrip.OriginParcelId = trip.DestinationParcelId;
      nextTrip.OriginParcel = trip.DestinationParcel;
      nextTrip.OriginZoneKey = trip.DestinationZoneKey;
      nextTrip.SetOriginAddressType(trip.DestinationAddressType);
    }

    private static void CloneTripModeAndTime(TourWrapper sourceTour, TripWrapper sourceTrip, TourWrapper tour, TripWrapper trip) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > > CloneTripModeAndTime Tour {0} Direction {1} Trip {2}", trip.Tour.Sequence, trip.Direction, trip.Sequence);
      }

      trip.Mode = (sourceTrip.Mode == Global.Settings.Modes.HovDriver) ? Global.Settings.Modes.HovPassenger : sourceTrip.Mode; ;
      trip.PathType = sourceTrip.PathType;

      trip.DepartureTime = sourceTrip.DepartureTime;
      trip.ArrivalTimeLimit = sourceTrip.ArrivalTimeLimit;
      trip.EarliestDepartureTime = sourceTrip.EarliestDepartureTime;
      trip.LatestDepartureTime = sourceTrip.LatestDepartureTime;
      trip.HUpdateTripValues();
    }

    private void UpdateHousehold() {

      // TODO:  check on this with John M or Mark.  John B added this loop to prevent
      //        null reference exceptions in cases where housheoldDay simulation is stopped after a user specified number of atttempts
      foreach (IHouseholdDayWrapper householdDay in _household.HouseholdDays) {
        if (householdDay.IsValid == false) {
          householdDay.Reset();
          return;
        }
      }

      foreach (PersonWrapper person in _household.Persons) {
        person.UpdatePersonValues();
      }

      if (Global.Configuration.ShouldRunTourModels) {

        foreach (TourWrapper tour in _household.HouseholdDays.SelectMany(householdDay => householdDay.PersonDays.Where(personDay => personDay.Tours != null)).SelectMany(personDay => personDay.Tours)) {
          tour.UpdateTourValues();

          foreach (TourWrapper subtour in tour.Subtours) {
            subtour.UpdateTourValues();
          }
        }
      }
    }

    private static void SetTourDestinationModeAndTime(HouseholdDayWrapper householdDay, TourWrapper tour, IParcelWrapper constrainedParcel, int constrainedMode, int constrainedArrivalTime, int constrainedDepartureTime) {
      //mbtrace
      if (Global.TraceResults) {
        Global.PrintFile.WriteLine("> > > > > SetTourDestinationModeAndTime for Tour {0}", tour.Sequence);
      }

      if (Global.Configuration.ShouldRunTourDestinationModeTimeModel) {

        ChoiceModelFactory.TotalTimesTourDestinationModeTimeModelRun[ParallelUtility.threadLocalAssignedIndex.Value]++;
        // Purpose-specific models can be substituted as they are developed.  For now all purposes cal the same model
        if (tour.DestinationPurpose == Global.Settings.Purposes.Escort) {
          Global.ChoiceModelSession.Get<TourDestinationModeTimeModel>().Run(householdDay, tour, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else if (tour.DestinationPurpose == Global.Settings.Purposes.PersonalBusiness) {
          Global.ChoiceModelSession.Get<TourDestinationModeTimeModel>().Run(householdDay, tour, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else if (tour.DestinationPurpose == Global.Settings.Purposes.Shopping) {
          Global.ChoiceModelSession.Get<TourDestinationModeTimeModel>().Run(householdDay, tour, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else if (tour.DestinationPurpose == Global.Settings.Purposes.Social) {
          Global.ChoiceModelSession.Get<TourDestinationModeTimeModel>().Run(householdDay, tour, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        } else if (tour.DestinationPurpose == Global.Settings.Purposes.Business) {
          Global.ChoiceModelSession.Get<TourDestinationModeTimeModel>().Run(householdDay, tour, constrainedParcel, constrainedMode, constrainedArrivalTime, constrainedDepartureTime);
        }
        if (!tour.PersonDay.IsValid) {
          return;
        }

        //set additional tour variables based on HTourModeTime object for chosen alternative
        // JLB 201406  Question for MAB.  The following code comes from SetTourModeAndTime.  Is it correct here too?
        HTourModeTime modeTimeChoice = new HTourModeTime(tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime);
        ITimeWindow timeWindow = (constrainedArrivalTime <= 0 && constrainedDepartureTime <= 0 && constrainedMode <= 0) ?
                    tour.GetRelevantTimeWindow(householdDay) : new TimeWindow();
        HTourModeTime.SetImpedanceAndWindow(timeWindow, tour, modeTimeChoice, -1, -1.0);

        if (modeTimeChoice != null && modeTimeChoice.LongestFeasibleWindow != null && tour.DestinationParcel != null) { // check also for existence of destination parcel
          tour.DestinationArrivalBigPeriod = modeTimeChoice.ArrivalPeriod;
          tour.DestinationDepartureBigPeriod = modeTimeChoice.DeparturePeriod;
          tour.EarliestOriginDepartureTime = modeTimeChoice.LongestFeasibleWindow.Start;
          tour.LatestOriginArrivalTime = modeTimeChoice.LongestFeasibleWindow.End;
          tour.IndicatedTravelTimeToDestination = modeTimeChoice.TravelTimeToDestination;
          tour.IndicatedTravelTimeFromDestination = modeTimeChoice.TravelTimeFromDestination;
        }
                //				else {  JLB 20130705 trying to handle cases with bad data in estimation mode
                else if (!Global.Configuration.IsInEstimationMode) {
          tour.PersonDay.IsValid = false;
          householdDay.IsValid = false;
        }
        //mbtrace
        if (Global.TraceResults) {
          Global.PrintFile.WriteLine("> > > > > > Predicted Destination {4} Mode {0} ArrivalTime {1} DepartureTime {2} Valid {3}",
                              tour.Mode, tour.DestinationArrivalTime, tour.DestinationDepartureTime, tour.PersonDay.IsValid, tour.DestinationParcelId);
        }
      }
    }

    public void Save() {
      _household.Export();

      foreach (PersonWrapper person in _household.Persons) {
        person.Export();
      }

      foreach (HouseholdDayWrapper householdDay in _household.HouseholdDays) {
        householdDay.Export();

        foreach (IJointTourWrapper jointTour in householdDay.JointToursList) {
          jointTour.Export();
        }

        foreach (IFullHalfTourWrapper fullHalfTour in householdDay.FullHalfToursList) {
          fullHalfTour.Export();
        }

        foreach (IPartialHalfTourWrapper partialHalfTour in householdDay.PartialHalfToursList) {
          partialHalfTour.Export();
        }

        foreach (PersonDayWrapper personDay in householdDay.PersonDays) {
          personDay.Export();

          if (personDay.Tours == null) {
            continue;
          }

          if (personDay.Tours.Count > 1) {
            // sorts tours chronologically
            personDay.Tours.Sort((tour1, tour2) => tour1.OriginDepartureTime.CompareTo(tour2.OriginDepartureTime));
          }

          foreach (TourWrapper tour in personDay.Tours) {
            tour.Export();

            if (tour.HalfTourFromOrigin != null && tour.HalfTourFromDestination != null) {
              foreach (TripWrapper trip in tour.HalfTourFromOrigin.Trips.Invert()) {
                trip.SetTourSequence(tour.Sequence);
                trip.SetTripValueOfTime();
                trip.Export();

                ChoiceModelUtility.WriteTripForTDM(trip, ChoiceModelFactory.TDMTripListExporter);
              }

              foreach (TripWrapper trip in tour.HalfTourFromDestination.Trips) {
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

            foreach (TourWrapper subtour in tour.Subtours) {
              subtour.SetParentTourSequence(tour.Sequence);
              subtour.Export();

              if (subtour.HalfTourFromOrigin == null || subtour.HalfTourFromDestination == null) {
                continue;
              }

              foreach (TripWrapper trip in subtour.HalfTourFromOrigin.Trips.Invert()) {
                trip.SetTourSequence(subtour.Sequence);
                trip.Export();

                ChoiceModelUtility.WriteTripForTDM(trip, ChoiceModelFactory.TDMTripListExporter);
              }

              foreach (TripWrapper trip in subtour.HalfTourFromDestination.Trips) {
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
