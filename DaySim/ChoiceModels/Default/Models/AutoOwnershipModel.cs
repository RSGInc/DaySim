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
using DaySim.Framework.Roster;

namespace DaySim.ChoiceModels.Default.Models
{
    public class AutoOwnershipModel : ChoiceModel
    {
        private const string CHOICE_MODEL_NAME = "AutoOwnershipModel";
        private const int TOTAL_ALTERNATIVES = 5;
        private const int TOTAL_NESTED_ALTERNATIVES = 0;
        private const int TOTAL_LEVELS = 1;
        private const int MAX_PARAMETER = 200;

        public override void RunInitialize(ICoefficientsReader reader = null)
        {
            Initialize(CHOICE_MODEL_NAME, Global.Configuration.AutoOwnershipModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER, reader as CoefficientsReader);
        }

        protected virtual void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IHouseholdWrapper household)
        {
            //Global.PrintFile.WriteLine("Generic AutoOwmnershipModel.RegionSpecificAutoOwnershipCoefficients being called so must not be overridden by CustomizationDll");
        }

        public void Run(IHouseholdWrapper household)
        {
            if (household == null)
            {
                throw new ArgumentNullException("household");
            }

            household.ResetRandom(4);

            if (Global.Configuration.IsInEstimationMode)
            {
                if (Global.Configuration.EstimationModel != CHOICE_MODEL_NAME)
                {
                    return;
                }
            }
            else if (Global.Configuration.AV_IncludeAutoTypeChoice)
            {
                ChoiceProbabilityCalculator AVchoiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(household.Id);
                RunAVModel(AVchoiceProbabilityCalculator, household);
                ChoiceProbabilityCalculator.Alternative chosenAlternative = AVchoiceProbabilityCalculator.SimulateChoice(household.RandomUtility);
                int choice = (int)chosenAlternative.Choice;

                household.OwnsAutomatedVehicles = choice;
            }


            ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(household.Id);

            if (household.VehiclesAvailable > 4)
            {
                household.VehiclesAvailable = 4;
            }

            if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode)
            {
                RunModel(choiceProbabilityCalculator, household, household.VehiclesAvailable);

                choiceProbabilityCalculator.WriteObservation();
            }
            else
            {
                RunModel(choiceProbabilityCalculator, household);

                ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(household.RandomUtility);
                int choice = (int)chosenAlternative.Choice;

                household.VehiclesAvailable = choice;
            }
        }

        private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IHouseholdWrapper household, int choice = Constants.DEFAULT_VALUE)
        {
            //            var distanceToTransitCappedUnderQtrMile = household.ResidenceParcel.DistanceToTransitCappedUnderQtrMile();
            //            var distanceToTransitQtrToHalfMile = household.ResidenceParcel.DistanceToTransitQtrToHalfMile();
            double foodRetailServiceMedicalLogBuffer1 = household.ResidenceParcel.FoodRetailServiceMedicalLogBuffer1();

            double workTourLogsumDifference = 0D; // (full or part-time workers) full car ownership vs. no car ownership
            double schoolTourLogsumDifference = 0D; // (school) full car ownership vs. no car ownership
                                                    //            const double workTourOtherLogsumDifference = 0D; // (other workers) full car ownership vs. no car ownership

            foreach (IPersonWrapper person in household.Persons)
            {
                if (person.IsWorker && person.UsualWorkParcel != null && person.UsualWorkParcelId != household.ResidenceParcelId)
                {
                    int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
                    int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);

                    ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers);
                    ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<WorkTourModeModel>().RunNested(person, household.ResidenceParcel, person.UsualWorkParcel, destinationArrivalTime, destinationDepartureTime, 0);

                    workTourLogsumDifference += nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
                    workTourLogsumDifference -= nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
                }

                if (person.IsDrivingAgeStudent && person.UsualSchoolParcel != null && person.UsualSchoolParcelId != household.ResidenceParcelId)
                {
                    int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.SchoolTourModeModel);
                    int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.SchoolTourModeModel);

                    ChoiceProbabilityCalculator.Alternative nestedAlternative1 = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, household.HouseholdTotals.DrivingAgeMembers);
                    ChoiceProbabilityCalculator.Alternative nestedAlternative2 = Global.ChoiceModelSession.Get<SchoolTourModeModel>().RunNested(person, household.ResidenceParcel, person.UsualSchoolParcel, destinationArrivalTime, destinationDepartureTime, 0);

                    schoolTourLogsumDifference += nestedAlternative1 == null ? 0 : nestedAlternative1.ComputeLogsum();
                    schoolTourLogsumDifference -= nestedAlternative2 == null ? 0 : nestedAlternative2.ComputeLogsum();
                }
            }

            // var votSegment = household.VotALSegment;
            //var taSegment = household.ResidenceParcel.TransitAccessSegment();

            //var aggregateLogsumDifference = // full car ownership vs. no car ownership
            //    Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.OneOrMoreCarsPerAdult][votSegment][taSegment] -
            //    Global.AggregateLogsums[household.ResidenceZoneId][Global.Settings.Purposes.HomeBasedComposite][Global.Settings.CarOwnerships.NoCars][votSegment][taSegment];

            double distanceToStop
                      = household.ResidenceParcel.GetDistanceToTransit() > 0
                            ? Math.Min(household.ResidenceParcel.GetDistanceToTransit(), 2 * Global.Settings.DistanceUnitsPerMile)  // JLBscale
                            : 2 * Global.Settings.DistanceUnitsPerMile;

            int ruralFlag = household.ResidenceParcel.RuralFlag();

            double zeroVehAVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0) ? Global.Configuration.AV_Own0VehiclesCoefficientForAVHouseholds : 0;
            double oneVehAVEffect = (Global.Configuration.AV_IncludeAutoTypeChoice && household.OwnsAutomatedVehicles > 0) ? Global.Configuration.AV_Own1VehicleCoefficientForAVHouseholds : 0;

            double zeroVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_DensityCoefficientForOwning0Vehicles * Math.Min(household.ResidenceBuffer2Density, 6000) : 0;
            double oneVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning1Vehicle : 0;
            double twoVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning2Vehicles : 0;
            double threeVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning3Vehicles : 0;
            double fourVehSEEffect = (Global.Configuration.PaidRideShareModeIsAvailable && Global.Configuration.AV_PaidRideShareModeUsesAVs) ? Global.Configuration.AV_SharingEconomy_ConstantForOwning4Vehicles : 0;

            // 0 AUTOS

            ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);

            alternative.Choice = 0;

            alternative.AddUtilityTerm(90, 1); //new calibration constant - can be used in estimation if coefficient 5 is constrained to 0
            alternative.AddUtilityTerm(1, household.Has1Driver.ToFlag());
            alternative.AddUtilityTerm(5, household.Has2Drivers.ToFlag());
            alternative.AddUtilityTerm(9, household.Has3Drivers.ToFlag());
            alternative.AddUtilityTerm(13, household.Has4OrMoreDrivers.ToFlag());
            alternative.AddUtilityTerm(18, household.HasNoFullOrPartTimeWorker.ToFlag());
            alternative.AddUtilityTerm(19, household.HouseholdTotals.PartTimeWorkersPerDrivingAgeMembers);
            alternative.AddUtilityTerm(23, household.HouseholdTotals.RetiredAdultsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(27, household.HouseholdTotals.UniversityStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(31, household.HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(35, household.HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(39, household.HouseholdTotals.ChildrenUnder5PerDrivingAgeMembers);
            alternative.AddUtilityTerm(43, household.Has0To15KIncome.ToFlag());
            alternative.AddUtilityTerm(47, household.Has50To75KIncome.ToFlag());
            alternative.AddUtilityTerm(51, household.Has75KPlusIncome.ToFlag());
            alternative.AddUtilityTerm(55, household.HasMissingIncome.ToFlag());
            alternative.AddUtilityTerm(59, workTourLogsumDifference);
            //            alternative.AddUtility(61, workTourOtherLogsumDifference);
            alternative.AddUtilityTerm(63, schoolTourLogsumDifference);
            //            alternative.AddUtility(67, aggregateLogsumDifference);
            alternative.AddUtilityTerm(69, Math.Log(distanceToStop));
            alternative.AddUtilityTerm(70, Math.Log(1 + household.ResidenceParcel.StopsTransitBuffer1));
            alternative.AddUtilityTerm(73, household.ResidenceParcel.ParkingOffStreetPaidDailyPriceBuffer1);
            alternative.AddUtilityTerm(75, foodRetailServiceMedicalLogBuffer1);
            alternative.AddUtilityTerm(77, workTourLogsumDifference * ruralFlag);
            alternative.AddUtilityTerm(81, household.ResidenceParcel.MixedUse4Index1());
            alternative.AddUtilityTerm(83, ruralFlag);
            alternative.AddUtilityTerm(85, household.ResidenceParcel.NetIntersectionDensity1());

            alternative.AddUtilityTerm(200, zeroVehAVEffect);
            alternative.AddUtilityTerm(200, zeroVehSEEffect);

            RegionSpecificCustomizations(alternative, household);


            // 1 AUTO

            alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);

            alternative.Choice = 1;

            alternative.AddUtilityTerm(91, 1); //new calibration constant - can be used in estimation if coefficient 6 is constrained to 0
            alternative.AddUtilityTerm(6, household.Has2Drivers.ToFlag());
            alternative.AddUtilityTerm(10, household.Has3Drivers.ToFlag());
            alternative.AddUtilityTerm(14, household.Has4OrMoreDrivers.ToFlag());
            //            alternative.AddUtility(17, 1D / Math.Max(household.HouseholdTotals.DrivingAgeMembers, 1)); // ratio of 1 car per driving age members
            alternative.AddUtilityTerm(18, household.Has1OrLessFullOrPartTimeWorkers.ToFlag());
            alternative.AddUtilityTerm(20, household.HouseholdTotals.PartTimeWorkersPerDrivingAgeMembers);
            alternative.AddUtilityTerm(24, household.HouseholdTotals.RetiredAdultsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(28, household.HouseholdTotals.UniversityStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(32, household.HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(36, household.HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(40, household.HouseholdTotals.ChildrenUnder5PerDrivingAgeMembers);
            alternative.AddUtilityTerm(44, household.Has0To15KIncome.ToFlag());
            alternative.AddUtilityTerm(48, household.Has50To75KIncome.ToFlag());
            alternative.AddUtilityTerm(52, household.Has75KPlusIncome.ToFlag());
            alternative.AddUtilityTerm(56, household.HasMissingIncome.ToFlag());
            alternative.AddUtilityTerm(60, workTourLogsumDifference * household.HasMoreDriversThan1.ToFlag());
            //            alternative.AddUtility(62, workTourOtherLogsumDifference * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(64, schoolTourLogsumDifference * household.HasMoreDriversThan1.ToFlag());
            //            alternative.AddUtility(68, aggregateLogsumDifference * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(72, Math.Log(1 + household.ResidenceParcel.StopsTransitBuffer1) * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(74, household.ResidenceParcel.ParkingOffStreetPaidDailyPriceBuffer1 * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(76, foodRetailServiceMedicalLogBuffer1 * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(82, household.ResidenceParcel.MixedUse4Index1() * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(84, ruralFlag * household.HasMoreDriversThan1.ToFlag());
            alternative.AddUtilityTerm(86, household.ResidenceParcel.NetIntersectionDensity1() * household.HasMoreDriversThan1.ToFlag());

            alternative.AddUtilityTerm(200, oneVehAVEffect);
            alternative.AddUtilityTerm(200, oneVehSEEffect);

            RegionSpecificCustomizations(alternative, household);


            // 2 AUTOS

            alternative = choiceProbabilityCalculator.GetAlternative(2, true, choice == 2);

            alternative.Choice = 2;

            alternative.AddUtilityTerm(92, 1); //new calibration constant - must be constrained to 0 in estimation
            alternative.AddUtilityTerm(2, household.Has1Driver.ToFlag());
            alternative.AddUtilityTerm(11, household.Has3Drivers.ToFlag());
            alternative.AddUtilityTerm(15, household.Has4OrMoreDrivers.ToFlag());
            //            alternative.AddUtility(17, 2D / Math.Max(household.HouseholdTotals.DrivingAgeMembers, 1)); // ratio of 2 cars per driving age members
            alternative.AddUtilityTerm(18, household.Has2OrLessFullOrPartTimeWorkers.ToFlag());
            alternative.AddUtilityTerm(60, workTourLogsumDifference * household.HasMoreDriversThan2.ToFlag());
            //            alternative.AddUtility(62, workTourOtherLogsumDifference * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(64, schoolTourLogsumDifference * household.HasMoreDriversThan2.ToFlag());
            //            alternative.AddUtility(68, aggregateLogsumDifference * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(72, Math.Log(1 + household.ResidenceParcel.StopsTransitBuffer1) * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(74, household.ResidenceParcel.ParkingOffStreetPaidDailyPriceBuffer1 * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(76, foodRetailServiceMedicalLogBuffer1 * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(82, household.ResidenceParcel.MixedUse4Index1() * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(84, ruralFlag * household.HasMoreDriversThan2.ToFlag());
            alternative.AddUtilityTerm(86, household.ResidenceParcel.NetIntersectionDensity1() * household.HasMoreDriversThan2.ToFlag());

            alternative.AddUtilityTerm(200, twoVehSEEffect);

            RegionSpecificCustomizations(alternative, household);

            // 3 AUTOS

            alternative = choiceProbabilityCalculator.GetAlternative(3, true, choice == 3);

            alternative.Choice = 3;

            alternative.AddUtilityTerm(93, 1); //new calibration constant - can be used in estimation if coefficient 7 is constrained to 0
            alternative.AddUtilityTerm(3, household.Has1Driver.ToFlag());
            alternative.AddUtilityTerm(7, household.Has2Drivers.ToFlag());
            alternative.AddUtilityTerm(16, household.Has4OrMoreDrivers.ToFlag());
            //          alternative.AddUtility(17, 3D / Math.Max(household.HouseholdTotals.DrivingAgeMembers, 1)); // ratio of 3 cars per driving age members
            alternative.AddUtilityTerm(18, household.Has3OrLessFullOrPartTimeWorkers.ToFlag());
            alternative.AddUtilityTerm(21, household.HouseholdTotals.PartTimeWorkersPerDrivingAgeMembers);
            alternative.AddUtilityTerm(25, household.HouseholdTotals.RetiredAdultsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(29, household.HouseholdTotals.UniversityStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(33, household.HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(37, household.HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(41, household.HouseholdTotals.ChildrenUnder5PerDrivingAgeMembers);
            alternative.AddUtilityTerm(45, household.Has0To15KIncome.ToFlag());
            alternative.AddUtilityTerm(49, household.Has50To75KIncome.ToFlag());
            alternative.AddUtilityTerm(53, household.Has75KPlusIncome.ToFlag());
            alternative.AddUtilityTerm(57, household.HasMissingIncome.ToFlag());
            alternative.AddUtilityTerm(60, workTourLogsumDifference * household.HasMoreDriversThan3.ToFlag());
            //            alternative.AddUtility(62, workTourOtherLogsumDifference * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(64, schoolTourLogsumDifference * household.HasMoreDriversThan3.ToFlag());
            //            alternative.AddUtility(68, aggregateLogsumDifference * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(72, Math.Log(1 + household.ResidenceParcel.StopsTransitBuffer1) * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(74, household.ResidenceParcel.ParkingOffStreetPaidDailyPriceBuffer1 * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(76, foodRetailServiceMedicalLogBuffer1 * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(82, household.ResidenceParcel.MixedUse4Index1() * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(84, ruralFlag * household.HasMoreDriversThan3.ToFlag());
            alternative.AddUtilityTerm(86, household.ResidenceParcel.NetIntersectionDensity1() * household.HasMoreDriversThan3.ToFlag());

            alternative.AddUtilityTerm(200, threeVehSEEffect);

            RegionSpecificCustomizations(alternative, household);

            // 4+ AUTOS

            alternative = choiceProbabilityCalculator.GetAlternative(4, true, choice == 4);

            alternative.Choice = 4;

            alternative.AddUtilityTerm(94, 1); //new calibration constant - can be used in estimation if coefficient 8 is constrained to 0
            alternative.AddUtilityTerm(4, household.Has1Driver.ToFlag());
            alternative.AddUtilityTerm(8, household.Has2Drivers.ToFlag());
            alternative.AddUtilityTerm(12, household.Has3Drivers.ToFlag());
            //            alternative.AddUtility(17, 4D / Math.Max(household.HouseholdTotals.DrivingAgeMembers, 1)); // ratio of 4 cars per driving age members
            alternative.AddUtilityTerm(18, household.Has4OrLessFullOrPartTimeWorkers.ToFlag());
            alternative.AddUtilityTerm(22, household.HouseholdTotals.PartTimeWorkersPerDrivingAgeMembers);
            alternative.AddUtilityTerm(26, household.HouseholdTotals.RetiredAdultsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(30, household.HouseholdTotals.UniversityStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(34, household.HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(38, household.HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers);
            alternative.AddUtilityTerm(42, household.HouseholdTotals.ChildrenUnder5PerDrivingAgeMembers);
            alternative.AddUtilityTerm(46, household.Has0To15KIncome.ToFlag());
            alternative.AddUtilityTerm(50, household.Has50To75KIncome.ToFlag());
            alternative.AddUtilityTerm(54, household.Has75KPlusIncome.ToFlag());
            alternative.AddUtilityTerm(58, household.HasMissingIncome.ToFlag());
            alternative.AddUtilityTerm(60, workTourLogsumDifference * household.HasMoreDriversThan4.ToFlag());
            //            alternative.AddUtility(62, workTourOtherLogsumDifference * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(64, schoolTourLogsumDifference * household.HasMoreDriversThan4.ToFlag());
            //            alternative.AddUtility(68, aggregateLogsumDifference * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(72, Math.Log(1 + household.ResidenceParcel.StopsTransitBuffer1) * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(74, household.ResidenceParcel.ParkingOffStreetPaidDailyPriceBuffer1 * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(76, foodRetailServiceMedicalLogBuffer1 * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(82, household.ResidenceParcel.MixedUse4Index1() * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(84, ruralFlag * household.HasMoreDriversThan4.ToFlag());
            alternative.AddUtilityTerm(86, household.ResidenceParcel.NetIntersectionDensity1() * household.HasMoreDriversThan4.ToFlag());

            alternative.AddUtilityTerm(200, fourVehSEEffect);

            RegionSpecificCustomizations(alternative, household);

        }
        private void RunAVModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IHouseholdWrapper household, int choice = Constants.DEFAULT_VALUE)
        {

            int ageOfHouseholdHead = 0;
            double totalCommuteTime = 0;

            foreach (IPersonWrapper person in household.Persons)
            {

                if (person.Sequence == 1)
                {
                    ageOfHouseholdHead = person.Age;
                }
                if (person.IsWorker && person.UsualWorkParcel != null && person.UsualWorkParcelId != household.ResidenceParcelId)
                {
                    int destinationArrivalTime = ChoiceModelUtility.GetDestinationArrivalTime(Global.Settings.Models.WorkTourModeModel);
                    int destinationDepartureTime = ChoiceModelUtility.GetDestinationDepartureTime(Global.Settings.Models.WorkTourModeModel);

                    totalCommuteTime += ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot,
                      destinationArrivalTime, household.ResidenceParcel, person.UsualWorkParcel).Variable;
                    totalCommuteTime += ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot,
                      destinationDepartureTime, person.UsualWorkParcel, household.ResidenceParcel).Variable;
                }
            }

            // 0 Conventional auotos

            ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);
            alternative.Choice = 0;
            //utility is 0

            // 1 AVs

            alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);
            alternative.Choice = 1;

            alternative.AddUtilityTerm(200, Global.Configuration.AV_AutoTypeConstant);
            alternative.AddUtilityTerm(200, Global.Configuration.AV_HHIncomeUnder50KCoefficient * household.HasIncomeUnder50K.ToFlag());
            alternative.AddUtilityTerm(200, Global.Configuration.AV_HHIncomeOver100KCoefficient * household.Has100KPlusIncome.ToFlag());
            alternative.AddUtilityTerm(200, Global.Configuration.AV_HHHeadUnder35Coefficient * (ageOfHouseholdHead < 35).ToFlag());
            alternative.AddUtilityTerm(200, Global.Configuration.AV_HHHeadOver65Coefficient * (ageOfHouseholdHead >= 65).ToFlag());
            alternative.AddUtilityTerm(200, Global.Configuration.AV_CoefficientPerHourCommuteTime * (totalCommuteTime / 60.0));

            // 2 not available

            alternative = choiceProbabilityCalculator.GetAlternative(2, false, choice == 2);
            alternative.Choice = 2;

            // 3 not available

            alternative = choiceProbabilityCalculator.GetAlternative(3, false, choice == 3);
            alternative.Choice = 3;


            // 4 not available

            alternative = choiceProbabilityCalculator.GetAlternative(4, false, choice == 4);
            alternative.Choice = 4;
        }
    }
}
