using System.Collections.Generic;
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
    int PathParkAndRideNodeCapacity { get; }

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
    int PathDestinationStopAreaKey { get; }
    double PathWalkTime { get; }
    double PathWalkDistance { get; }
    double PathBikeTime { get; }
    double PathBikeDistance { get; }
    double PathBikeCost { get; }
    int PathOriginAccessMode { get; }
    double PathOriginAccessTime { get; }
    double PathOriginAccessDistance { get; }
    double PathOriginAccessCost { get; }
    int PathDestinationAccessMode { get; }
    double PathDestinationAccessTime { get; }
    double PathDestinationAccessDistance { get; }
    double PathDestinationAccessCost { get; }



    bool Available { get; }

    List<IPathTypeModel> RunAllPlusParkAndRide(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice);

    List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice);

    List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes);

    List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, int transitPassOwnership, bool carsAreAVs, double transitDiscountFraction, bool randomChoice, params int[] modes);

    void RunModel(IRandomUtility randomUtility, bool useZones);

    double PathParkAndRideWalkAccessEgressTime { get; }

    double PathTransitWalkAccessEgressTime { get; }
  }
}
