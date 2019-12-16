﻿using System.Collections.Generic;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.PathTypeModels {
  public interface IPathTypeModel {
    int Mode { get; set; }

    double GeneralizedTimeLogsum { get; }

    double GeneralizedTimeChosen { get; }

    double PathTime { get; }

    double PathDistance { get; }

    double PathCost { get; }

    int PathType { get; }

    int PathDestinationParkingNodeId { get; }
    int PathDestinationParkingType { get; }
    double PathDestinationParkingCost { get; }
    double PathDestinationParkingWalkTime { get; }


    int PathParkAndRideNodeId { get; }
    int PathParkAndRideEgressNodeId { get; }

    double PathParkAndRideTransitTime { get; }
    double PathParkAndRideTransitDistance { get; }
    double PathParkAndRideTransitCost { get; }
    double PathParkAndRideTransitGeneralizedTime { get; }

    //JLB
    double PathTransitTime { get; }
    double PathTransitDistance { get; }
    double PathTransitCost { get; }
    double PathTransitGeneralizedTime { get; }

    int PathOriginStopAreaKey { get; }
    int PathOriginStopAreaParcelID { get; }
    int PathOriginStopAreaZoneID { get; }
    int PathDestinationStopAreaKey { get; }
    int PathDestinationStopAreaParcelID { get; }
    int PathDestinationStopAreaZoneID { get; }
    double PathWalkTime { get; }
    double PathWalkDistance { get; }
    double PathBikeTime { get; }
    double PathBikeDistance { get; }
    double PathBikeCost { get; }
    int PathOriginAccessMode { get; }
    double PathOriginAccessTime { get; }
    double PathOriginAccessDistance { get; }
    double PathOriginAccessCost { get; }
    double PathOriginAccessUtility { get; }
    int PathDestinationAccessMode { get; }
    double PathDestinationAccessTime { get; }
    double PathDestinationAccessDistance { get; }
    double PathDestinationAccessCost { get; }
    double PathDestinationAccessUtility { get; }



    bool Available { get; }

    List<IPathTypeModel> RunAllPlusParkAndRide(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int hovOccupancy, int autoType, int personType, bool randomChoice);

    List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int hovOccupancy, int autoType, int personType, bool randomChoice);

    List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int hovOccupancy, int autoType, int personType, bool randomChoice, params int[] modes);

    List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, int personAge, int householdCars, int transitPassOwnership, bool carsAreAVs, int hovOccupancy, int autoType, int personType, bool randomChoice, params int[] modes);

    void RunModel(IRandomUtility randomUtility, bool useZones);

    double PathParkAndRideWalkAccessEgressTime { get; }

    double PathTransitWalkAccessEgressTime { get; }

  }
}
