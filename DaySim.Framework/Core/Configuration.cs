// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace DaySim.Framework.Core {
    public sealed class Configuration {
        [XmlAttribute]
        public string Settings { get; set; }

        [XmlAttribute]
        public string RemoteUsername { get; set; }

        [XmlAttribute]
        public string RemotePassword { get; set; }

        [XmlAttribute]
        public string RemoteMachines { get; set; }


        [XmlAttribute]
        public string NodeIndexPath { get; set; }

        [XmlAttribute]
        public char NodeIndexDelimiter { get; set; }

        [XmlAttribute]
        public string NodeDistancesPath { get; set; }

        [XmlAttribute]
        public char NodeDistancesDelimiter { get; set; }

        [XmlAttribute]
        public bool AllowNodeDistanceAsymmetry { get; set; }

        //new since 203
        [XmlAttribute]
        public string NodeStopAreaIndexPath { get; set; }

        [XmlAttribute]
        //[Metadata("Foo...")]
        public string RosterPath { get; set; }

        [XmlAttribute]
        public string RosterCombinationsPath { get; set; }

        [XmlAttribute]
        public string CustomizationDll { get; set; }

        [XmlAttribute]
        public double VotVeryLowLow { get; set; }

        [XmlAttribute]
        public double VotLowMedium { get; set; }

        [XmlAttribute]
        public double VotMediumHigh { get; set; }

        [XmlAttribute]
        public double VotHighVeryHigh { get; set; }


        [XmlAttribute]
        public string IxxiPath { get; set; }

        [XmlAttribute]
        public char IxxiDelimiter { get; set; }

        [XmlAttribute]
        public bool IxxiFirstLineIsHeader { get; set; }


        [XmlAttribute]
        public bool ImportParkAndRideNodes { get; set; }

        [XmlAttribute]
        public string RawParkAndRideNodePath { get; set; }

        [XmlAttribute]
        public char RawParkAndRideNodeDelimiter { get; set; }

        [XmlAttribute]
        public string InputParkAndRideNodePath { get; set; }


        [XmlAttribute]
        public char InputParkAndRideNodeDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public bool ShouldReadParkAndRideNodeSkim { get; set; }

        [XmlAttribute]
        public char SkimDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportParcelNodes { get; set; }

        [XmlAttribute]
        public string RawParcelNodePath { get; set; }

        [XmlAttribute]
        public char RawParcelNodeDelimiter { get; set; }

        [XmlAttribute]
        public string InputParcelNodePath { get; set; }

        [XmlAttribute]
        public char InputParcelNodeDelimiter { get; set; } = '\t';


        [XmlAttribute]
        public bool ImportParcels { get; set; }

        [XmlAttribute]
        public string RawParcelPath { get; set; }

        [XmlAttribute]
        public char RawParcelDelimiter { get; set; }

        [XmlAttribute]
        public string InputParcelPath { get; set; }

        [XmlAttribute]
        public char InputParcelDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public bool ImportZones { get; set; }

        [XmlAttribute]
        public string RawZonePath { get; set; }

        [XmlAttribute]
        public char RawZoneDelimiter { get; set; }

        [XmlAttribute]
        public string InputZonePath { get; set; }

        [XmlAttribute]
        public char InputZoneDelimiter { get; set; } = '\t';


        [XmlAttribute]
        public bool ImportHouseholds { get; set; }

        [XmlAttribute]
        public string RawHouseholdPath { get; set; }

        [XmlAttribute]
        public char RawHouseholdDelimiter { get; set; }

        [XmlAttribute]
        public string InputHouseholdPath { get; set; }

        [XmlAttribute]
        public char InputHouseholdDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputHouseholdPath { get; set; }

        [XmlAttribute]
        public char OutputHouseholdDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportPersons { get; set; }

        [XmlAttribute]
        public string RawPersonPath { get; set; }

        [XmlAttribute]
        public char RawPersonDelimiter { get; set; }

        [XmlAttribute]
        public string InputPersonPath { get; set; }

        [XmlAttribute]
        public char InputPersonDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputPersonPath { get; set; }

        [XmlAttribute]
        public char OutputPersonDelimiter { get; set; }

        [XmlAttribute]
        public string DataType { get; set; }

        [XmlAttribute]
        public string TripType { get; set; }

        [XmlAttribute]
        public string TourType { get; set; }

        [XmlAttribute]
        public string PersonDayType { get; set; }

        [XmlAttribute]
        public string HouseholdDayType { get; set; }

        [XmlAttribute]
        public bool ImportHouseholdDays { get; set; }

        [XmlAttribute]
        public string RawHouseholdDayPath { get; set; }

        [XmlAttribute]
        public char RawHouseholdDayDelimiter { get; set; }

        [XmlAttribute]
        public string InputHouseholdDayPath { get; set; }

        [XmlAttribute]
        public char InputHouseholdDayDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputHouseholdDayPath { get; set; }

        [XmlAttribute]
        public char OutputHouseholdDayDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportJointTours { get; set; }

        [XmlAttribute]
        public string RawJointTourPath { get; set; }

        [XmlAttribute]
        public char RawJointTourDelimiter { get; set; }

        [XmlAttribute]
        public string InputJointTourPath { get; set; }

        [XmlAttribute]
        public char InputJointTourDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputJointTourPath { get; set; }

        [XmlAttribute]
        public char OutputJointTourDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportFullHalfTours { get; set; }

        [XmlAttribute]
        public string RawFullHalfTourPath { get; set; }

        [XmlAttribute]
        public char RawFullHalfTourDelimiter { get; set; }

        [XmlAttribute]
        public string InputFullHalfTourPath { get; set; }

        [XmlAttribute]
        public char InputFullHalfTourDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputFullHalfTourPath { get; set; }

        [XmlAttribute]
        public char OutputFullHalfTourDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportPartialHalfTours { get; set; }

        [XmlAttribute]
        public string RawPartialHalfTourPath { get; set; }

        [XmlAttribute]
        public char RawPartialHalfTourDelimiter { get; set; }

        [XmlAttribute]
        public string InputPartialHalfTourPath { get; set; }

        [XmlAttribute]
        public char InputPartialHalfTourDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputPartialHalfTourPath { get; set; }

        [XmlAttribute]
        public char OutputPartialHalfTourDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportPersonDays { get; set; }

        [XmlAttribute]
        public string RawPersonDayPath { get; set; }

        [XmlAttribute]
        public char RawPersonDayDelimiter { get; set; }

        [XmlAttribute]
        public string InputPersonDayPath { get; set; }

        [XmlAttribute]
        public char InputPersonDayDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputPersonDayPath { get; set; }

        [XmlAttribute]
        public char OutputPersonDayDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportTours { get; set; }

        [XmlAttribute]
        public string RawTourPath { get; set; }

        [XmlAttribute]
        public char RawTourDelimiter { get; set; }

        [XmlAttribute]
        public string InputTourPath { get; set; }

        [XmlAttribute]
        public char InputTourDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputTourPath { get; set; }

        [XmlAttribute]
        public char OutputTourDelimiter { get; set; }


        [XmlAttribute]
        public bool ImportTrips { get; set; }

        [XmlAttribute]
        public string RawTripPath { get; set; }

        [XmlAttribute]
        public char RawTripDelimiter { get; set; }

        [XmlAttribute]
        public string InputTripPath { get; set; }

        [XmlAttribute]
        public char InputTripDelimiter { get; set; } = '\t';

        [XmlAttribute]
        public string OutputTripPath { get; set; }

        [XmlAttribute]
        public char OutputTripDelimiter { get; set; }


        [XmlAttribute]
        public string ChoiceModelRunner { get; set; }

        [XmlAttribute]
        public bool ShouldRunRawConversion { get; set; }

        [XmlAttribute]
        public bool ShouldOutputStandardFilesInEstimationMode { get; set; }

        //Deprecated
        [XmlAttribute]
        public string WorkingDirectory { get; set; }

        [XmlAttribute]
        public int HouseholdSamplingRateOneInX { get; set; }

        [XmlAttribute]
        public int HouseholdSamplingStartWithY { get; set; }

        [XmlAttribute]
        public int MinParcelSize { get; set; }

        [XmlAttribute]
        public int UrbanThreshold { get; set; }

        [XmlAttribute]
        public bool UseShortDistanceCircuityMeasures { get; set; }

        [XmlAttribute]
        public bool UseShortDistanceNodeToNodeMeasures { get; set; }

        [XmlAttribute]
        public double PathImpedance_PathChoiceScaleFactor { get; set; }

        //new since 203
        [XmlAttribute]
        public int PathImpedance_UtilityForm_Auto { get; set; }

        //new since 203
        [XmlAttribute]
        public int PathImpedance_UtilityForm_Transit { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_Gamma_Cost { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_Gamma_InVehicleTime { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_Gamma_ExtraTime { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleExtraTimeWeight_Driver { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleExtraTimeWeight_Passenger { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_Train { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_Bus { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_Metro { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_LightRail { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_Train { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_Bus { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_Metro { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_LightRail { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_Train { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_Bus { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_Metro { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_LightRail { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_SOV { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_HOVDriver { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Commute_HOVPassenger { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_SOV { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_HOVDriver { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Personal_HOVPassenger { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_SOV { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_HOVDriver { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_InVehicleTimeWeight_Business_HOVPassenger { get; set; }

        //new since 203
        [XmlAttribute]
        public double Coefficients_Distance_HOVPassenger { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitInVehicleTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFirstWaitTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitTransferWaitTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitNumberBoardingsWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitNumberBoardingsWeight_Rail { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitDriveAccessTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitWalkAccessTimeWeight { get; set; }

        //new since 203
        [XmlAttribute]
        public double PathImpedance_TransitAccessEgressTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_WalkTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_BikeTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_WalkMinutesPerDistanceUnit { get; set; }

        [XmlAttribute]
        public double PathImpedance_WalkMinutesPerMile { get; set; }
        // replaced by PathImpedance_WalkMinutesPerDistanceUnit.  Retained for backward comatibility.

        [XmlAttribute]
        public double PathImpedance_TransitWalkAccessDistanceLimit { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitWalkAccessDirectLimit { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitSingleBoardingLimit { get; set; }

        [XmlAttribute]
        public double PathImpedance_AutoTolledPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_AvailablePathUpperTimeLimit { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitLocalBusPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitPremiumBusPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitLightRailPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitCommuterRailPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFerryPathConstant { get; set; }

        // new constants for Florida
        [XmlAttribute]
        public double PathImpedance_TransitLocalBusPNRPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitLocalBusKNRPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitNEWMODEPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitBRTPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFIXEDGUIDEWAYPathConstant { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitBRTTimeAdditiveWeight { get; set; }
        // new constants for Florida

        [XmlAttribute]
        public bool PathImpedance_TransitUsePathTypeSpecificTime { get; set; }

        [XmlAttribute]
        public bool PathImpedance_TransitUseFloridaSubmodes { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitPremiumBusTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitLightRailTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitCommuterRailTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitSubwayTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitPATTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitTrolleyTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFerryTimeAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitLightRailInVehicleTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitPremiumBusInVehicleTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitCommuterRailInVehicleTimeWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFerryInVehicleTimeWeight { get; set; }

        [XmlAttribute]
        public bool PathImpedance_BikeUseTypeSpecificDistanceFractions { get; set; }

        [XmlAttribute]
        public double PathImpedance_BikeType1DistanceFractionAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_BikeType2DistanceFractionAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_BikeType3DistanceFractionAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_BikeType4DistanceFractionAdditiveWeight { get; set; }

        [XmlAttribute]
        public double PathImpedance_AutoOperatingCostPerDistanceUnit { get; set; }

        [XmlAttribute]
        public double PathImpedance_AutoOperatingCostPerMile { get; set; }
        // replaced by PathImpedance_AutoOperatingCostPerDistanceUnit.  Retained for backward compatibility.

        [XmlAttribute]
        public bool PathImpedance_TransitUseFareDiscountFractions { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFareDiscountFractionChildUnder5 { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFareDiscountFractionChild5To15 { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFareDiscountFractionHighSchoolStudent { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFareDiscountFractionUniverityStudent { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitFareDiscountFractionAge65Up { get; set; }

        [XmlAttribute]
        public double PathImpedance_TransitPassCostPercentChangeVersusBase { get; set; }

        [XmlAttribute]
        public double Coefficients_BaseCostCoefficientPerDollar { get; set; }
        // replaced by Coefficients_BaseCostCoefficientPerMonetaryUnit.  Retained for backward comatibility.

        [XmlAttribute]
        public double Coefficients_BaseCostCoefficientPerMonetaryUnit { get; set; }

        [XmlAttribute]
        public double Coefficients_BaseCostCoefficientIncomeLevel { get; set; }

        //new since 203
        [XmlAttribute]
        public double Coefficients_CostCoefficientIncomeMultipleMinimum { get; set; }

        //new since 203
        [XmlAttribute]
        public double Coefficients_CostCoefficientIncomeMultipleMaximum { get; set; }

        [XmlAttribute]
        public double Coefficients_CostCoefficientIncomePower_Work { get; set; }

        [XmlAttribute]
        public double Coefficients_CostCoefficientIncomePower_Other { get; set; }

        [XmlAttribute]
        public double Coefficients_MeanTimeCoefficient_Work { get; set; }

        [XmlAttribute]
        public double Coefficients_MeanTimeCoefficient_Other { get; set; }

        [XmlAttribute]
        public double Coefficients_StdDeviationTimeCoefficient_Work { get; set; }

        [XmlAttribute]
        public double Coefficients_StdDeviationTimeCoefficient_Other { get; set; }

        [XmlAttribute]
        public double Coefficients_HOV2CostDivisor_Work { get; set; }

        [XmlAttribute]
        public double Coefficients_HOV2CostDivisor_Other { get; set; }

        [XmlAttribute]
        public double Coefficients_HOV3CostDivisor_Work { get; set; }

        [XmlAttribute]
        public double Coefficients_HOV3CostDivisor_Other { get; set; }

        [XmlAttribute]
        public bool HOVPassengersIncurCosts { get; set; }



        [XmlAttribute]
        public bool IsInEstimationMode { get; set; }

        //new since 203
        [XmlAttribute]
        public bool TestEstimationModelInApplicationMode { get; set; }

        [XmlAttribute]
        public bool ShouldRunChoiceModels { get; set; }

        /// <summary>
        /// When true, run choice models with only 1 thread, regardless of NProcessors (similar to InEstimation mode)
        /// </summary>
        [XmlAttribute]
        public bool ChoiceModelDebugMode { get; set; } = false;


        [XmlAttribute]
        public bool ShouldRunHouseholdModels { get; set; }

        [XmlAttribute]
        public bool ShouldRunPersonModels { get; set; }

        [XmlAttribute]
        public bool ShouldRunPersonDayModels { get; set; }

        [XmlAttribute]
        public bool ShouldRunTourModels { get; set; }

        [XmlAttribute]
        public bool ShouldRunTourTripModels { get; set; }

        [XmlAttribute]
        public bool ShouldRunSubtourModels { get; set; }

        [XmlAttribute]
        public bool ShouldRunSubtourTripModels { get; set; }

        [XmlAttribute]
        public string EstimationModel { get; set; }

        [XmlAttribute]
        public double MaximumBlendingDistance { get; set; }

        [XmlAttribute]
        public bool ShowRunChoiceModelsStatus { get; set; }

        [XmlAttribute]
        public int SmallDegreeOfParallelism { get; set; }

        [XmlAttribute]
        public int LargeDegreeOfParallelism { get; set; }

        /// <summary>
        /// NO LONGER USED. NProcessors alone controls how many threads are allocated during multiprocessing.
        /// </summary>
        [XmlAttribute]
        public int NBatches { get; set; }

        //new since 203
        [XmlAttribute]
        public int NProcessors { get; set; } = Environment.ProcessorCount;

        [XmlAttribute]
        public bool ShouldOutputTDMTripList { get; set; }

        [XmlAttribute]
        public string OutputTDMTripListPath { get; set; }

        [XmlAttribute]
        public char TDMTripListDelimiter { get; set; }

        [XmlAttribute]
        public bool UseTransimsTDMTripListFormat { get; set; }

        [XmlAttribute]
        public bool UseRandomVotDistribution { get; set; }

        [XmlAttribute]
        public bool ShouldSynchronizeRandomSeed { get; set; }

        [XmlAttribute]
        public int RandomSeed { get; set; }

        [XmlAttribute]
        public bool ShouldLoadAggregateLogsumsFromFile { get; set; }

        [XmlAttribute]
        public bool ShouldOutputAggregateLogsums { get; set; }

        [XmlAttribute]
        public string OutputAggregateLogsumsPath { get; set; }


        [XmlAttribute]
        public bool ShouldLoadSamplingWeightsFromFile { get; set; }

        [XmlAttribute]
        public bool ShouldOutputSamplingWeights { get; set; }

        [XmlAttribute]
        public string OutputSamplingWeightsPath { get; set; }


        [XmlAttribute]
        public bool ShouldOutputAlogitData { get; set; }

        [XmlAttribute]
        public string OutputAlogitDataPath { get; set; }

        [XmlAttribute]
        public string OutputAlogitControlPath { get; set; }


        [XmlAttribute]
        public int WorkLocationModelSampleSize { get; set; }

        [XmlAttribute]
        public string WorkLocationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkLocationModel { get; set; }

        [XmlAttribute]
        public bool IncludeWorkLocationModel { get; set; }


        [XmlAttribute]
        public int SchoolLocationModelSampleSize { get; set; }

        [XmlAttribute]
        public string SchoolLocationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunSchoolLocationModel { get; set; }

        [XmlAttribute]
        public bool IncludeSchoolLocationModel { get; set; }


        [XmlAttribute]
        public string PayToParkAtWorkplaceModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkUsualModeAndScheduleModel { get; set; }

        [XmlAttribute]
        public bool ShouldRunPayToParkAtWorkplaceModel { get; set; }

        [XmlAttribute]
        public bool IncludePayToParkAtWorkplaceModel { get; set; }


        [XmlAttribute]
        public string TransitPassOwnershipModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunTransitPassOwnershipModel { get; set; }

        [XmlAttribute]
        public bool IncludeTransitPassOwnershipModel { get; set; }


        [XmlAttribute]
        public string AutoOwnershipModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunAutoOwnershipModel { get; set; }

        [XmlAttribute]
        public string JointHalfTourGenerationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunJointHalfTourGenerationModels { get; set; }

        [XmlAttribute]
        public bool ShouldSuppressPartiallyJointHalfTours { get; set; }

        [XmlAttribute]
        public string FullJointHalfTourParticipationModelCoefficients { get; set; }

        [XmlAttribute]
        public string PartialJointHalfTourParticipationModelCoefficients { get; set; }

        [XmlAttribute]
        public string PartialJointHalfTourChauffeurModelCoefficients { get; set; }

        [XmlAttribute]
        public string JointTourGenerationModelCoefficients { get; set; }

        [XmlAttribute]
        public string JointTourParticipationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunJointTourGenerationModel { get; set; }

        [XmlAttribute]
        public string HouseholdPersonDayPatternModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunHouseholdPersonDayPatternModel { get; set; }

        [XmlAttribute]
        public string IndividualPersonDayPatternModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunIndividualPersonDayPatternModel { get; set; }

        [XmlAttribute]
        public string PersonExactNumberOfToursModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunPersonExactNumberOfToursModel { get; set; }


        [XmlAttribute]
        public int WorkTourDestinationModelSampleSize { get; set; }

        [XmlAttribute]
        public string WorkTourDestinationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkTourDestinationModel { get; set; }

        [XmlAttribute]
        public bool ShouldRunTourDestinationModel { get; set; }

        [XmlAttribute]
        public int TourDestinationModelSampleSize { get; set; }

        [XmlAttribute]
        public string TourDestinationModelCoefficients { get; set; }

        [XmlAttribute]
        public int OtherTourDestinationModelSampleSize { get; set; }

        [XmlAttribute]
        public string OtherTourDestinationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunOtherTourDestinationModel { get; set; }


        [XmlAttribute]
        public string WorkBasedSubtourGenerationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkBasedSubtourGenerationModel { get; set; }


        [XmlAttribute]
        public string WorkTourModeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkTourModeModel { get; set; }


        [XmlAttribute]
        public string SchoolTourModeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunSchoolTourModeModel { get; set; }


        [XmlAttribute]
        public string WorkBasedSubtourModeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkBasedSubtourModeModel { get; set; }


        [XmlAttribute]
        public string EscortTourModeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunEscortTourModeModel { get; set; }


        [XmlAttribute]
        public string OtherHomeBasedTourModeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunOtherHomeBasedTourModeModel { get; set; }


        [XmlAttribute]
        public string WorkTourTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkTourTimeModel { get; set; }


        [XmlAttribute]
        public string SchoolTourTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunSchoolTourTimeModel { get; set; }


        [XmlAttribute]
        public string OtherHomeBasedTourTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunOtherHomeBasedTourTimeModel { get; set; }


        [XmlAttribute]
        public string WorkBasedSubtourTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkBasedSubtourTimeModel { get; set; }


        [XmlAttribute]
        public string IntermediateStopGenerationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunIntermediateStopGenerationModel { get; set; }


        [XmlAttribute]
        public int IntermediateStopLocationModelSampleSize { get; set; }

        [XmlAttribute]
        public string IntermediateStopLocationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunIntermediateStopLocationModel { get; set; }


        [XmlAttribute]
        public string TripModeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunTripModeModel { get; set; }


        [XmlAttribute]
        public string TripTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunTripTimeModel { get; set; }


        //new since 203
        [XmlAttribute]
        public bool ShouldRunTourModeTimeModel { get; set; }

        //new since 203
        [XmlAttribute]
        public string WorkTourModeTimeModelCoefficients { get; set; }

        //new since 203
        [XmlAttribute]
        public string SchoolTourModeTimeModelCoefficients { get; set; }

        //new since 203
        [XmlAttribute]
        public string OtherHomeBasedTourModeTimeModelCoefficients { get; set; }

        //new since 203
        [XmlAttribute]
        public string WorkBasedSubtourModeTimeModelCoefficients { get; set; }

        //new since 203
        [XmlAttribute]
        public string TourModeTimeModelCoefficients { get; set; }
        // fix this when copying to Actum



        [XmlAttribute]
        public bool ShouldUseShadowPricing { get; set; }

        [XmlAttribute]
        public char ShadowPriceDelimiter { get; set; }

        [XmlAttribute]
        public bool ShouldUseParkAndRideShadowPricing { get; set; }

        [XmlAttribute]
        public char ParkAndRideShadowPriceDelimiter { get; set; }

        [XmlAttribute]
        public double ParkAndRideShadowPriceStepSize { get; set; }

        [XmlAttribute]
        public double ParkAndRideShadowPriceMaximumPenalty { get; set; }

        [XmlAttribute]
        public int ParkAndRideShadowPriceTimeSpread { get; set; }

        [XmlAttribute]
        public int UsualWorkParcelThreshold { get; set; }

        [XmlAttribute]
        public int UsualSchoolParcelThreshold { get; set; }

        [XmlAttribute]
        public int UsualUniversityParcelThreshold { get; set; }

        [XmlAttribute]
        public int NumberOfParcelsInReportDiffs { get; set; }

        [XmlAttribute]
        public int UsualWorkPercentTolerance { get; set; }

        [XmlAttribute]
        public int UsualWorkAbsoluteTolerance { get; set; }

        [XmlAttribute]
        public int UsualSchoolPercentTolerance { get; set; }

        [XmlAttribute]
        public int UsualSchoolAbsoluteTolerance { get; set; }

        [XmlAttribute]
        public int UsualUniversityPercentTolerance { get; set; }

        [XmlAttribute]
        public int UsualUniversityAbsoluteTolerance { get; set; }

        [XmlAttribute]
        public string ActumPrimaryPriorityTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public string ActumPrimaryPriorityTimeScheduleModelCoefficients { get; set; }

        [XmlAttribute]
        public string LDPrimaryPriorityTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public string LDPrimaryPriorityTimeScheduleModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunActumPrimaryPriorityTimeModel { get; set; }

        [XmlAttribute]
        public bool ShouldRunLDPrimaryPriorityTimeModel { get; set; }

        [XmlAttribute]
        public string HouseholdDayPatternTypeModelCoefficients { get; set; }

        [XmlAttribute]
        public string PersonDayPatternTypeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunHouseholdDayPatternTypeModel { get; set; }

        [XmlAttribute]
        public string WorkAtHomeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunWorkAtHomeModel { get; set; }

        [XmlAttribute]
        public string MandatoryTourGenerationModelCoefficients { get; set; }

        [XmlAttribute]
        public string MandatoryStopPresenceModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunMandatoryTourGenerationModel { get; set; }

        [XmlAttribute]
        public string PersonTourGenerationModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunPersonTourGenerationModel { get; set; }

        [XmlAttribute]
        public bool TextSkimFilesContainHeaderRecord { get; set; }

        [XmlAttribute]
        public bool AllowTripArrivalTimeOverlaps { get; set; }

        [XmlAttribute]
        public bool ReportInvalidPersonDays { get; set; }

        [XmlAttribute]
        public string SamplingWeightsSettingsType { get; set; }

        [XmlAttribute]
        public int MaximumHouseholdSize { get; set; }

        //new since 203
        [XmlAttribute]
        public bool ImportTransitStopAreas { get; set; }

        //new since 203
        [XmlAttribute]
        public string InputTransitStopAreaPath { get; set; }

        //new since 203
        [XmlAttribute]
        public char InputTransitStopAreaDelimiter { get; set; } = '\t';

        //new since 203
        [XmlAttribute]
        public char RawTransitStopAreaDelimiter { get; set; }

        //new since 203
        [XmlAttribute]
        public string RawTransitStopAreaPath { get; set; }

        //new since 203
        [XmlAttribute]
        public string HDF5Filename { get; set; }

        //new since 203
        [XmlAttribute]
        public string HDF5Path { get; set; }

        //new since 203
        [XmlAttribute]
        public bool WriteTripsToHDF5 { get; set; }

        //new since 203
        [XmlAttribute]
        public bool Policy_TestMilageBasedPricing { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_CentsPerMileInAMPeak { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_CentsPerMileInPMPeak { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_CentsPerMileBetweenPeaks { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_CentsPerMileOutsidePeaks { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_AMPricingPeriodStart { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_AMPricingPeriodEnd { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_PMPricingPeriodStart { get; set; }

        //new since 203
        [XmlAttribute]
        public int Policy_PMPricingPeriodEnd { get; set; }


        //new since 203
        [XmlAttribute]
        public double Policy_FractionIncreaseInWorkAtHomeShare { get; set; }

        //new since 203
        [XmlAttribute]
        public bool Policy_UniversalTransitPassOwnership { get; set; }

        //new since 203
        [XmlAttribute]
        public double Policy_UniversalTransitFareDiscountFraction { get; set; }

        //new since 203
        [XmlAttribute]
        public double Policy_CongestedTravelTimeMultiplier { get; set; }

        //new since 203
        [XmlAttribute]
        public bool TraceSimulatedChoiceOutcomes { get; set; }

        //new since 203
        [XmlAttribute]
        public bool TraceModelResultValidity { get; set; }

        //new since 203
        [XmlAttribute]
        public int InvalidAttemptsBeforeTrace { get; set; }

        //new since 203
        [XmlAttribute]
        public int InvalidAttemptsBeforeContinue { get; set; }


        //new since 203
        [XmlAttribute]
        public bool ReadHDF5 { get; set; }

        //new since 203
        [XmlAttribute]
        public string BasePath { get; set; }

        //new since 203
        [XmlAttribute]
        public string WorkingSubpath { get; set; }

        //new since 203
        [XmlAttribute]
        public string OutputSubpath { get; set; }

        //new since 203
        [XmlAttribute]
        public string EstimationSubpath { get; set; }

        //new since 203
        [XmlAttribute]
        public bool ShouldRunInputTester { get; set; }

        //new since 203
        [XmlAttribute]
        public bool ConstrainTimesForModeChoiceLogsums { get; set; }

        //new since 203
        [XmlAttribute]
        public string AggregateLogsumCalculator { get; set; }

        //new since 203
        [XmlAttribute]
        public string PathTypeModel { get; set; }

        //new since 203
        [XmlAttribute]
        public bool DVRPC { get; set; }

        [XmlAttribute]
        public bool JAX { get; set; }

        [XmlAttribute]
        public bool Nashville { get; set; }

        [XmlAttribute]
        public bool PSRC { get; set; }

        [XmlAttribute]
        public bool SFCTA { get; set; }

        //new since 203
        [XmlAttribute]
        public bool AvoidDisaggregateModeChoiceLogsums { get; set; }

        [XmlAttribute]
        public string AggregateTourModeDestinationModelCoefficients { get; set; }

        [XmlAttribute]
        public string TourDestinationModeTimeModelCoefficients { get; set; }

        [XmlAttribute]
        public bool ShouldRunTourDestinationModeTimeModel { get; set; }

        //new since 203
        [XmlAttribute]
        public int MaximumParcelToStopAreaDistance { get; set; }
        [XmlAttribute]
        public int MaximumStopAreasToSearch { get; set; }
        [XmlAttribute]
        public int MaximumParcelToStopAreaDistanceParkAndRide { get; set; }
        [XmlAttribute]
        public int MaximumStopAreasToSearchParkAndRide { get; set; }

        [XmlAttribute]
        public double MaximumMilesToDriveToParkAndRide { get; set; }
        [XmlAttribute]
        public double MaximumRatioDriveToParkAndRideVersusDriveToDestination { get; set; }

        [XmlAttribute]
        public bool WriteStopAreaIDsInsteadOfZonesForTransitTrips { get; set; }

        [XmlAttribute]
        public bool UseMicrozoneSkims { get; set; }

        [XmlAttribute]
        public string MicrozoneToParkAndRideNodeIndexPath { get; set; }

        [XmlAttribute]
        public char MicrozoneToParkAndRideNodeIndexDelimiter { get; set; }

        [XmlAttribute]
        public int DestinationScale { get; set; }

        [XmlAttribute]
        public int NumberOfODShadowPricingDistricts { get; set; }

        [XmlAttribute]
        public bool ShouldUseODShadowPricing { get; set; }

        [XmlAttribute]
        public bool UseODShadowPricingForWorkAtHomeAlternative { get; set; } = true;
        [XmlAttribute]
        public bool UseWorkShadowPricingForWorkAtHomeAlternative { get; set; } = true;

        [XmlAttribute]
        public double WorkLocationODShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double WorkLocationOOShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double WorkTourDestinationODShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double WorkTourDestinationOOShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double OtherTourDestinationODShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double OtherTourDestinationOOShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double IntermediateStopLocationODShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double IntermediateStopLocationOOShadowPriceCoefficient { get; set; }

        [XmlAttribute]
        public double IntermediateStopLocationODShadowPriceStopOriginFraction { get; set; }

        [XmlAttribute]
        public double IntrazonalWalkMinutesPerMile_OverrideSkims { get; set; }

        [XmlAttribute]
        public double IntrazonalBikeMinutesPerMile_OverrideSkims { get; set; }

        [XmlAttribute]
        public double IntrazonalAutoMinutesPerMile_OverrideSkims { get; set; }

        [XmlAttribute]
        public double PaidRideShare_ModeConstant { get; set; }

        [XmlAttribute]
        public bool SetPaidRideShareModeAvailable { get; set; }

        [XmlAttribute]
        public double PaidRideShare_ExtraCostPerDistanceUnit { get; set; }

        [XmlAttribute]
        public double PaidRideShare_FixedCostPerRide { get; set; }

        [XmlAttribute]
        public double PaidRideShare_Age26to35Coefficient { get; set; }

        [XmlAttribute]
        public double PaidRideShare_Age18to25Coefficient { get; set; }

        [XmlAttribute]
        public double PaidRideShare_AgeUnder18Coefficient { get; set; }

        //new for AV capabilities
        [XmlAttribute]
        public bool AV_IncludeAutoTypeChoice { get; set; }

        [XmlAttribute]
        public double AV_AutoTypeConstant { get; set; }

        [XmlAttribute]
        public double AV_HHIncomeUnder50KCoefficient { get; set; }

        [XmlAttribute]
        public double AV_HHIncomeOver100KCoefficient { get; set; }

        [XmlAttribute]
        public double AV_HHHeadUnder35Coefficient { get; set; }

        [XmlAttribute]
        public double AV_HHHeadOver65Coefficient { get; set; }

        [XmlAttribute]
        public double AV_CoefficientPerHourCommuteTime { get; set; }

        [XmlAttribute]
        public double AV_Own0VehiclesCoefficientForAVHouseholds { get; set; }

        [XmlAttribute]
        public double AV_Own1VehicleCoefficientForAVHouseholds { get; set; }

        [XmlAttribute]
        public double AV_InVehicleTimeCoefficientDiscountFactor { get; set; }

        [XmlAttribute]
        public bool AV_PaidRideShareModeUsesAVs { get; set; }

        [XmlAttribute]
        public double AV_PaidRideShareModeConstant { get; set; }

        [XmlAttribute]
        public double AV_PaidRideShare_ExtraCostPerDistanceUnit { get; set; }

        [XmlAttribute]
        public double AV_PaidRideShare_FixedCostPerRide { get; set; }


        [XmlAttribute]
        public bool HDF5SkimScaledAndCondensed { get; set; } = false;

        public enum NodeDistanceReaderTypes { TextOrBinary, HDF5 };

        [XmlAttribute]
        public NodeDistanceReaderTypes NodeDistanceReaderType { get; set; } = NodeDistanceReaderTypes.TextOrBinary;

        private List<Type> pluginTypes = null;
        private readonly LazyConcurrentDictionary<Type, Type> assignableObjectTypes = new LazyConcurrentDictionary<Type, Type>();
        /**
         * Given a type, if that type was found in the customization dll return it, otherwise return the requested type
         **/
        public Type getAssignableObjectType(Type requestedType) {
            Type returnType = assignableObjectTypes.GetOrAdd(requestedType, (key) => getObjectType(key));
            return returnType;
        }   //end getAssignableObjectType

        private Type getObjectType(Type requestedType) {
            Type returnType = null;
            if (pluginTypes == null) {
                pluginTypes = LoadCustomizationTypes();
            }
            foreach (Type loadedType in pluginTypes) {
                if (requestedType.IsAssignableFrom(loadedType)) {
                    returnType = loadedType;
                    Global.PrintFile.WriteLine("CustomizationDll getAssignableObjectType for '" + requestedType + "' is returning type '" + loadedType);
                    break;
                }
            }   //end foreach
            if (returnType == null) {
                returnType = requestedType;
                Global.PrintFile.WriteLine("CustomizationDll getAssignableObjectType for '" + requestedType + "' could not find a custom version of that type so is just returning the passed in type");
            }
            return returnType;
        }   //end get getObjectType

        public static List<Type> LoadCustomizationTypes() {
            List<Type> pluginTypes = new List<Type>();
            Assembly assembly = null;
            if (!string.IsNullOrWhiteSpace(Global.Configuration.CustomizationDll)) {
                string dllFile = Global.GetInputPath(Global.Configuration.CustomizationDll);
                bool fileExists = File.Exists(dllFile);
                if (!fileExists) {
                    //if file not found relative to the basePath then look for it in the same directory as the .exe
                    string directoryName = ConfigurationManagerRSG.GetExecutingAssemblyLocation();
                    dllFile = Path.Combine(directoryName, Global.Configuration.CustomizationDll);
                    fileExists = File.Exists(dllFile);
                    Global.PrintFile.WriteLine("CustomizationDll LoadCustomizationTypes: dll '" + Global.Configuration.CustomizationDll + "' not found at location relative to BasePath so looked relative to executing assembly location: " + dllFile + ". File exists?: " + fileExists);
                }
                if (fileExists) {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    assembly = Assembly.Load(an);
                    if (assembly != null) {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types) {
                            if (type.IsInterface || type.IsAbstract) {
                                Global.PrintFile.WriteLine("CustomizationDll LoadCustomizationTypes: found type: " + type + " but is not saving because type.IsInterface=" + type.IsInterface + " or type.IsAbstract=" + type.IsAbstract);
                                continue;
                            } else {
                                Global.PrintFile.WriteLine("CustomizationDll LoadCustomizationTypes: found type: " + type + " and is keeping it.");
                                pluginTypes.Add(type);
                            }
                        }   //end if assembly != null
                    } //end if assembly not null
                } //end if file exists
                if (pluginTypes.Count == 0) {
                    throw new Exception("CustomizationDll LoadCustomizationTypes: For dll: " + dllFile + ". File exists?: " + fileExists + ". Assembly loaded?: " + (assembly != null) + " but no types loaded!");
                } else {
                    Global.PrintFile.WriteLine("CustomizationDll LoadCustomizationTypes: Successfully loaded dll: " + dllFile + " with " + pluginTypes.Count + " types: " + String.Join(", ", pluginTypes));
                }
            } // end if customization dll specified
            return pluginTypes;
        } //end LoadCustomizationTypes

    }   //end class
}   //end namespace
