// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using DaySim.DomainModels.Factories;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.Factories;
using DaySim.Framework.Roster;
using DaySim.PathTypeModels;

namespace DaySim.AggregateLogsums {
  public sealed class AggregateLogsumsCalculator_Actum : IAggregateLogsumsCalculator {
    #region fields

    private const double UPPER_LIMIT = 88;
    private const double LOWER_LIMIT = -88;

    private const int TOTAL_SUBZONES = 2;


   
    //purpose   home-based all, work-based,escort,pers.bus.,shopping,business,social/rec
    private static readonly double[] walk_constant = new[] { 0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
    private static readonly double[] walk_intrazonal = new[] { 0, -3.61, -3.36, -5.32, -4.02, -5.09, -0.248, -2.65 };
    private static readonly double[] walk_gen_time = new[] { 0, -0.0446, -0.0428, -0.0590, -0.0470, -0.0568, -0.0282, -0.0379 };
    private static readonly double[] bike_constant = new[] { 0, -2.18, -2.6, 0.055, -1.63, -1.84, 0.141, -2.48 };
    private static readonly double[] bike_lowincome = new[] { 0, 0.0, 0.0, 0.397, 0.755, 0.0, 0.0, 0.0 };
    private static readonly double[] bike_nocar = new[] { 0, 0.525, 2.041, -0.224, 0.517, 0.0633, 0.0, 1.25 };
    private static readonly double[] bike_carcomp = new[] { 0, 0.388, 2.604, -0.512, 0.486, 0.316, 0.0, 0.735 };
    private static readonly double[] bike_child = new[] { 0, 1.05, 0.0, 0, 0.0, 0, 0.0, 1.74 };
    private static readonly double[] bike_intrazonal = new[] { 0, -3.8, -24.87, -6.95, -4.06, -5.37, -13.7, -3.15 };
    private static readonly double[] bike_gen_time = new[] { 0, -0.0500, -0.0441, -0.0821, -0.0525, -0.0704, -0.0292, -0.0438 };
    private static readonly double[] sov_constant = new[] { 0, -3.67, -3.135, -4.75, -2.52, -2.46, -1.15, -3.53 };
    private static readonly double[] sov_highincome = new[] { 0, 0.451,-1.00,0.875,0.431,0.0,0.755,0.337};
    private static readonly double[] sov_nocar = new[] { 0, -3.5,-1.641,-2.12,-3.46,-5.05,-4.07,-2.61};
    private static readonly double[] sov_carcomp = new[] { 0, -0.855,1.139,-1.05,-0.743,-1.04,-0.958,-0.676};
    private static readonly double[] sov_intrazonal = new[] { 0, -2.05,-23.08,-2.99,-2.86,-3.66,0.0904,-1.85};
    private static readonly double[] sov_gen_time = new[] { 0, -0.0303,-0.0273,-0.0502,-0.0322,-0.0483,-0.0125,-0.0286};
    private static readonly double[] hov_constant = new[] { 0, -3.28,-3.251,-2.08,-3.8,-4.94,-1.47,-3.93};
    private static readonly double[] hov_nocar = new[] { 0, -2.73,-0.911,-4.25,-2.72,-2.96,-2.89,-1.36};
    private static readonly double[] hov_carcomp = new[] { 0, -0.284,0.804,-1.26,0.346,0.0181,-1.31,0.117};
    private static readonly double[] hov_child = new[] { 0, -0.142,0.0,-0.637,0.0,0.18,0.0,0.812};
    private static readonly double[] hov_intrazonal = new[] { 0, -2.21,-0.264,-3.28,-2.49,-2.6,0.665,-1.77};
    private static readonly double[] hov_gen_time = new[] { 0, -0.0335,-0.0300,-0.0417,-0.0309,-0.0377,-0.0155,-0.0293};
    private static readonly double[] transit_constant = new[] { 0, -9.18,-21.71,-11.6,-9.23,-9.34,-4.8,-9.02};
    private static readonly double[] transit_lowincome = new[] { 0, 0.0,-0.441,0.0,0.0,-0.877,0.0,-0.284};
    private static readonly double[] transit_highincome = new[] { 0, 0.65,0.48,0.0,0.599,0.835,0.0,0.807};
    private static readonly double[] transit_nocar = new[] { 0, 1.5,13.11,1.1,1.99,0.9,0.464,2.08};
    private static readonly double[] transit_carcomp = new[] { 0, 0.541,14.61,0.0,1.34,-0.098,0.0,0.88};
    private static readonly double[] transit_child = new[] { 0, 1.23,0.0,0.0,1.61,1.19,0.0,1.58};
    private static readonly double[] transit_gen_time = new[] { 0, -0.0266,-0.0254,-0.0246,-0.028,-0.0451,-0.0148,-0.0233};
    private static readonly double[] transit_oclose = new[] { 0, 0.285,1.768,0.662,0.593,-0.148,0.0973,0.405};
    private static readonly double[] transit_dclose = new[] { 0, 0.0462,0.79,0.45,0.175,-0.0044,0.484,-0.0212};
    private static readonly double[] transit_ofar = new[] { 0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0 };
    private static readonly double[] transit_dfar = new[] { 0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0 };

    private static readonly double log_size_mult = 1.00 ;
    private static readonly double[] size_service   = new[] { 0,  1.000,1.000,1.000,1.000,1.000,1.000,1.000};
    private static readonly double[] size_education = new[] { 0, 0.267, 0.295, 6.959, 0.141, 0.000, 0.441, 4.855 };
    private static readonly double[] size_food = new[] { 0, 0.604, 0.582, 2.641, 0.582, 0.006, 0.000, 21.54 };
    private static readonly double[] size_government = new[] { 0, 0.026, 0.000, 0.000, 0.000, 0.002, 0.247, 0.411 };
    private static readonly double[] size_industrial = new[] { 0, 0.021, 0.000, 0.254, 0.061, 0.000, 0.357, 0.369 };
    private static readonly double[] size_medical = new[] { 0, 0.093, 0.394, 0.217, 0.756, 0.002, 0.307, 1.404 };
    private static readonly double[] size_office = new[] { 0, 0.008, 0.174, 0.000, 0.017, 0.001, 0.346, 0.170 };
    private static readonly double[] size_retail = new[] { 0, 0.470, 0.542, 0.636, 0.605, 0.333, 0.000, 1.597 };
    private static readonly double[] size_households = new[] { 0, 0.060, 0.000, 0.323, 0.061, 0.002, 0.000, 1.774 };
    private static readonly double[] size_otheremp = new[] { 0, 0.084, 0.000, 0.000, 0.113, 0.008, 0.000, 2.177 };


    private readonly int _zoneCount;
    private readonly Dictionary<int, IZone> _eligibleZones;
    private readonly ISubzone[][] _zoneSubzones;
    private readonly int _middayStartMinute;

    #endregion

    public AggregateLogsumsCalculator_Actum() {
      FileInfo file = Global.AggregateLogsumsPath.ToFile();

      //if (Global.Configuration.ShouldLoadAggregateLogsumsFromFile && file.Exists) {
      //  return;
      //}

      Framework.DomainModels.Persisters.IPersisterReader<IZone> zoneReader =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IZone>>()
                    .Reader;

      _eligibleZones = zoneReader.Where(z => z.DestinationEligible).ToDictionary(z => z.Id, z => z);
      _zoneCount = zoneReader.Count;
      _zoneSubzones = CalculateZoneSubzones();
      _middayStartMinute = DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Start;
    }

    public void Calculate(IRandomUtility randomUtility) {
      FileInfo file = Global.AggregateLogsumsPath.ToFile();

      //if (Global.Configuration.ShouldLoadAggregateLogsumsFromFile && file.Exists) {
      //  Global.AggregateLogsums = LoadAggregateLogsumsFromFile(file);

      //   return;
      // }

      Global.AggregateLogsums = new double[_zoneCount][][][][];

      Parallel.For(0, _zoneCount, new ParallelOptions { MaxDegreeOfParallelism = ParallelUtility.NThreads }, id => CalculateZone(randomUtility, id));

      for (int id = 0; id < _zoneCount; id++) {
        double[][][][] purposes = Global.AggregateLogsums[id];

        for (int purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
          double[][][] carOwnerships = purposes[purpose];

          for (int carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
            double[][] votALSegments = carOwnerships[carOwnership];

            for (int votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
              double[] transitAccesses = votALSegments[votALSegment];

              for (int transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
                transitAccesses[transitAccess] = Math.Log(transitAccesses[transitAccess]);
              }
            }
          }
        }
      }

      //if (Global.Configuration.ShouldLoadAggregateLogsumsFromFile && !file.Exists) {
      //  SaveAggregateLogsumsToFile(file);
      //}
    }

    private void CalculateZone(IRandomUtility randomUtility, int id) {
      Global.AggregateLogsums[id] = ComputeZone(randomUtility, id);
    }

    private double[][][][][] LoadAggregateLogsumsFromFile(FileInfo file) {
      using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
        BinaryFormatter formatter = new BinaryFormatter();

        return (double[][][][][])formatter.Deserialize(stream);
      }
    }

    private void SaveAggregateLogsumsToFile(FileInfo file) {
      using (FileStream stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read)) {
        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, Global.AggregateLogsums);
      }
    }

    private double[][][][] ComputeZone(IRandomUtility randomUtility, int id) {
      double[][][][] purposes = new double[Global.Settings.Purposes.TotalPurposes][][][];

      for (int purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
        double[][][] carOwnerships = new double[Global.Settings.CarOwnerships.TotalCarOwnerships][][];

        purposes[purpose] = carOwnerships;

        for (int carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
          double[][] votALSegments = new double[Global.Settings.VotALSegments.TotalVotALSegments][];

          carOwnerships[carOwnership] = votALSegments;

          for (int votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
            double[] transitAccesses = new double[Global.Settings.TransitAccesses.TotalTransitAccesses];

            votALSegments[votALSegment] = transitAccesses;

            for (int transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
              transitAccesses[transitAccess] = Constants.EPSILON;
            }
          }
        }
      }


      if (!_eligibleZones.TryGetValue(id, out IZone origin)) {
        return purposes;
      }

      foreach (IZone destination in _eligibleZones.Values) {
        bool setImpedance = true;
        ISubzone[] subzones = _zoneSubzones[destination.Id];

        //const double parkingCost = 0;

        // mode impedance
        double transitGenTime = 0D;
        double walkGenTime = 0D;
        double bikeGenTime = 0D;
        double sovGenTime = 0D;
        double hovGenTime = 0D;

        for (int purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
          double[][][] carOwnerships = purposes[purpose];

          // set purpose inputs
          int escortFlag = (purpose == Global.Settings.Purposes.Escort).ToFlag();
          int personalBusinessFlag = (purpose == Global.Settings.Purposes.PersonalBusiness).ToFlag();
          int shoppingFlag = (purpose == Global.Settings.Purposes.Shopping).ToFlag();
          int businessFlag = (purpose == Global.Settings.Purposes.ALSBusiness).ToFlag();
          int socialFlag = (purpose == Global.Settings.Purposes.Social).ToFlag();


          for (int carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
            double[][] votALSegments = carOwnerships[carOwnership];

            // set car ownership inputs
            int childFlag = FlagUtility.GetChildFlag(carOwnership);
            int noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
            int carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);


            for (int votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
              double[] transitAccesses = votALSegments[votALSegment];

              // set vot specific variables
              int lowIncomeFlag = (votALSegment == Global.Settings.VotALSegments.Low).ToFlag();
              int highIncomeFlag = (votALSegment == Global.Settings.VotALSegments.High).ToFlag();

              double timeCoefficient = Global.Configuration.COMPASS_BaseTimeCoefficientPerMinute;
              double costCoefficient = (votALSegment == Global.Settings.VotALSegments.Low)
                                                      ? timeCoefficient * 60.0/50.0
                                                      : (votALSegment == Global.Settings.VotALSegments.Medium)
                                                            ? timeCoefficient * 60.0/60.0
                                                            : timeCoefficient * 60.0/70.0;

              for (int transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
                double purposeUtility = 0D;

                // set transit access flags
                int hasNearTransitAccessFlag = (transitAccess == Global.Settings.TransitAccesses.Gt0AndLteQtrMi).ToFlag();
                int hasNoTransitAccessFlag = (transitAccess == Global.Settings.TransitAccesses.None).ToFlag();

                foreach (ISubzone subzone in subzones) {
                  double size = subzone.GetSize(purpose);

                  if (size <= -50 ) {
                    continue;
                  }

                  int intrazonalFlag = (id == destination.Id).ToFlag();

                  // set subzone flags
                  int hasNoTransitEgressFlag = 1 - (subzone.Sequence == 0 ? 1 : 0);

                  if (setImpedance) {
                    setImpedance = false;

                    // intermediate variable of type IEnumerable<IPathTypeModel> is needed to acquire First() method as extension
                    IEnumerable<IPathTypeModel> pathTypeModels;

                    int drivingAge = 22;
                    int fullFareType = Global.Settings.PersonTypes.FullTimeWorker;

                    pathTypeModels = PathTypeModelFactory.Singleton.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                        costCoefficient, timeCoefficient, /* isDrivingAge */ drivingAge, /* householdVehicles */ 1, /* transitPassOwnership */ 0, false, fullFareType, false, Global.Settings.Modes.Walk);
                    IPathTypeModel walkPath = pathTypeModels.First();

                    walkGenTime = walkPath.GeneralizedTimeLogsum;

                    pathTypeModels = PathTypeModelFactory.Singleton.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                        costCoefficient, timeCoefficient, /* isDrivingAge */ drivingAge, /* householdVehicles */ 1, /* transitPassOwnership */ 0, false, fullFareType, false, Global.Settings.Modes.Bike);
                    IPathTypeModel bikePath = pathTypeModels.First();

                    bikeGenTime = bikePath.GeneralizedTimeLogsum;

                    pathTypeModels = PathTypeModelFactory.Singleton.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                        costCoefficient, timeCoefficient, /* isDrivingAge */ drivingAge, /* householdVehicles */ 1, /* transitPassOwnership */ 0, false, fullFareType, false, Global.Settings.Modes.Sov);
                    IPathTypeModel sovPath = pathTypeModels.First();

                    sovGenTime = sovPath.GeneralizedTimeLogsum;

                    pathTypeModels = PathTypeModelFactory.Singleton.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                                            costCoefficient, timeCoefficient, /* isDrivingAge */ drivingAge, /* householdVehicles */ 1, /* transitPassOwnership */ 0, false, fullFareType, false, Global.Settings.Modes.HovPassenger);
                    IPathTypeModel hovPath = pathTypeModels.First();

                    hovGenTime = hovPath.GeneralizedTimeLogsum;

                    //if using stop areas, use stop area nearest to the zone centroid
                    int transitOid = (!Global.StopAreaIsEnabled) ? id
                                                                : (origin.NearestStopAreaId > 0) ? Global.TransitStopAreaMapping[origin.NearestStopAreaId]
                                                                : id;
                    int transitDid = (!Global.StopAreaIsEnabled) ? destination.Id
                                            : (destination.NearestStopAreaId > 0) ? Global.TransitStopAreaMapping[destination.NearestStopAreaId]
                                            : id;

                    pathTypeModels = PathTypeModelFactory.Singleton.Run(randomUtility, transitOid, transitDid, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                        costCoefficient, timeCoefficient, /* isDrivingAge */ drivingAge, /* householdVehicles */ 1, /* transitPassOwnership */ 0, false, fullFareType, false, Global.Settings.Modes.Transit);
                    IPathTypeModel transitPath = pathTypeModels.First();

                    transitGenTime = transitPath.GeneralizedTimeLogsum;
                  }

                  double modeUtilitySum = 0D;

                  // walk 
                  if (walkGenTime > 0 || intrazonalFlag > 0) {
                    modeUtilitySum += ComputeUtility(
                        walk_constant[purpose] +
                        walk_intrazonal[purpose] * intrazonalFlag +
                        walk_gen_time[purpose] * walkGenTime);
                  }
                  // bike 
                  if (bikeGenTime > 0 || intrazonalFlag > 0) {
                    modeUtilitySum += ComputeUtility(
                        bike_constant[purpose] +
                        bike_lowincome[purpose] * lowIncomeFlag +
                        bike_nocar[purpose] * noCarsFlag +
                        bike_carcomp[purpose] * carCompetitionFlag +
                        bike_child[purpose] * childFlag +
                        bike_intrazonal[purpose] * intrazonalFlag +
                        bike_gen_time[purpose] * bikeGenTime);
                  }

                  // SOV
                  if ((sovGenTime > 0 || intrazonalFlag > 0) && (childFlag == 0)) {
                    modeUtilitySum += ComputeUtility(
                        sov_constant[purpose] +
                        sov_highincome[purpose] * highIncomeFlag +
                        sov_nocar[purpose] * noCarsFlag +
                        sov_carcomp[purpose] * carCompetitionFlag +
                        sov_intrazonal[purpose] * intrazonalFlag +
                        sov_gen_time[purpose] * sovGenTime );
                  }

                  // hov
                  if (hovGenTime > 0 || intrazonalFlag > 0) {
                    modeUtilitySum += ComputeUtility(
                        hov_constant[purpose] +
                        hov_nocar[purpose] * noCarsFlag +
                        hov_carcomp[purpose] * carCompetitionFlag +
                        hov_child[purpose] * childFlag +
                        hov_intrazonal[purpose] * intrazonalFlag +
                        hov_gen_time[purpose] * hovGenTime);
                  }

                  // transit
                  if (transitGenTime > 0 && intrazonalFlag == 0) {
                    modeUtilitySum += ComputeUtility(
                        transit_constant[purpose] +
                        transit_lowincome[purpose] * lowIncomeFlag +
                        transit_highincome[purpose] * highIncomeFlag +
                        transit_nocar[purpose] * noCarsFlag +
                        transit_carcomp[purpose] * carCompetitionFlag +
                        transit_child[purpose] * childFlag +
                        transit_gen_time[purpose] * transitGenTime +
                        transit_oclose[purpose] * hasNearTransitAccessFlag +
                        transit_ofar[purpose] * hasNoTransitAccessFlag +
                        transit_dfar[purpose] * hasNoTransitEgressFlag );
                  }

                  double modeLogsum = modeUtilitySum > Constants.EPSILON ? Math.Log(modeUtilitySum) : -30D;
                  purposeUtility = modeLogsum + size;

                }

                transitAccesses[transitAccess] += purposeUtility;
              }
            }
          }
        }
      }

      return purposes;
    }

    private ISubzone[][] CalculateZoneSubzones() {
      SubzoneFactory subzoneFactory = new SubzoneFactory(Global.Configuration);
      ISubzone[][] zoneSubzones = new ISubzone[_zoneCount][];

      for (int id = 0; id < _zoneCount; id++) {
        ISubzone[] subzones = new ISubzone[TOTAL_SUBZONES];

        zoneSubzones[id] = subzones;

        for (int subzone = 0; subzone < TOTAL_SUBZONES; subzone++) {
          subzones[subzone] = subzoneFactory.Create(subzone);
        }
      }

      Framework.DomainModels.Persisters.IPersisterReader<IParcel> parcelReader =
                Global
                    .ContainerDaySim
                    .GetInstance<IPersistenceFactory<IParcel>>()
                    .Reader;

      IParcelCreator parcelCreator =
                Global
                    .ContainerDaySim
                    .GetInstance<IWrapperFactory<IParcelCreator>>()
                    .Creator;

      foreach (IParcel parcel in parcelReader) {
        Framework.DomainModels.Wrappers.IParcelWrapper parcelWrapper = parcelCreator.CreateWrapper(parcel);

        ISubzone[] subzones = zoneSubzones[parcelWrapper.ZoneId];
        // var subzone = (parcel.GetDistanceToTransit() > 0 && parcel.GetDistanceToTransit() <= .5) ? 0 : 1;  
        // JLBscale replaced above with following:
        int subzone = (parcelWrapper.GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile > 0 && parcelWrapper.GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile <= .5) ? 0 : 1;

        subzones[subzone].Households += parcelWrapper.Households;
        subzones[subzone].StudentsK8 += parcelWrapper.StudentsK8;
        subzones[subzone].StudentsHighSchool += parcelWrapper.StudentsHighSchool;
        subzones[subzone].StudentsUniversity += parcelWrapper.StudentsUniversity;
        subzones[subzone].EmploymentEducation += parcelWrapper.EmploymentEducation;
        subzones[subzone].EmploymentFood += parcelWrapper.EmploymentFood;
        subzones[subzone].EmploymentGovernment += parcelWrapper.EmploymentGovernment;
        subzones[subzone].EmploymentIndustrial += parcelWrapper.EmploymentIndustrial;
        subzones[subzone].EmploymentMedical += parcelWrapper.EmploymentMedical;
        subzones[subzone].EmploymentOffice += parcelWrapper.EmploymentOffice;
        subzones[subzone].EmploymentRetail += parcelWrapper.EmploymentRetail;
        subzones[subzone].EmploymentService += parcelWrapper.EmploymentService;
        subzones[subzone].EmploymentTotal += parcelWrapper.EmploymentTotal;
        subzones[subzone].ParkingOffStreetPaidDailySpaces += parcelWrapper.ParkingOffStreetPaidDailySpaces;
        subzones[subzone].ParkingOffStreetPaidHourlySpaces += parcelWrapper.ParkingOffStreetPaidHourlySpaces;
        subzones[subzone].OpenSpace += (Global.Configuration.UseParcelLandUseCodeAsSquareFeetOpenSpace) ? parcelWrapper.LandUseCode : 0.0;
      }

      foreach (ISubzone[] subzones in _eligibleZones.Values.Select(zone => zoneSubzones[zone.Id])) {
        for (int subzone = 0; subzone < TOTAL_SUBZONES; subzone++) {
          double hou = subzones[subzone].Households;
          double k12 = subzones[subzone].StudentsK8 + subzones[subzone].StudentsHighSchool;
          double uni = subzones[subzone].StudentsUniversity;
          double edu = subzones[subzone].EmploymentEducation;
          double foo = subzones[subzone].EmploymentFood;
          double gov = subzones[subzone].EmploymentGovernment;
          double ind = subzones[subzone].EmploymentIndustrial;
          double med = subzones[subzone].EmploymentMedical;
          double off = subzones[subzone].EmploymentOffice;
          double ret = subzones[subzone].EmploymentRetail;
          double ser = subzones[subzone].EmploymentService;
          double tot = subzones[subzone].EmploymentTotal;
          double osp = subzones[subzone].OpenSpace;
          double oth = tot - edu - foo - gov - ind - med - off - ret - ser;

          for (int purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {

            subzones[subzone].SetSize(purpose,
              ComputeSize(size_education[purpose] * edu
                        + size_food[purpose] * foo
                        + size_government[purpose] * gov
                        + size_industrial[purpose] * ind
                        + size_medical[purpose] * med
                        + size_office[purpose] * off
                        + size_otheremp[purpose] * oth
                        + size_retail[purpose] * ret
                        + size_service[purpose] * ser
                        + size_households[purpose] * hou));
          }
        }
      }

      return zoneSubzones;
    }

    private double ComputeSize(double size) {
      if (size < Constants.EPSILON) {
        return -99;
      }

      return Math.Log(size) * log_size_mult;
    }

    private double ComputeUtility(double utility) {
      if (utility > UPPER_LIMIT || utility < LOWER_LIMIT) {
        utility = utility > UPPER_LIMIT ? UPPER_LIMIT : LOWER_LIMIT;
      }

      return Math.Exp(utility);
    }


  }
}
