using System.Collections.Generic;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Wrappers;

namespace Daysim.PathTypeModels {
	public interface IPathTypeModel {
		int Mode { get; set; }

		double GeneralizedTimeLogsum { get; }

		double GeneralizedTimeChosen { get; }

		double PathTime { get; }

		double PathDistance { get; }

		double PathCost { get;}

        int PathType { get; }

		int PathParkAndRideNodeId { get; }

        double PathParkAndRideTransitTime { get; }
        double PathParkAndRideTransitDistance { get; }
        double PathParkAndRideTransitCost { get; }
        double PathParkAndRideTransitGeneralizedTime { get; }

		bool Available { get; }

		List<IPathTypeModel> RunAllPlusParkAndRide (IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice);

		List<IPathTypeModel> RunAll(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice);

		List<IPathTypeModel> Run(IRandomUtility randomUtility, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice, params int[] modes);

		List<IPathTypeModel> Run(IRandomUtility randomUtility, int originZoneId, int destinationZoneId, int outboundTime, int returnTime, int purpose, double tourCostCoefficient, double tourTimeCoefficient, bool isDrivingAge, int householdCars, double transitDiscountFraction, bool randomChoice, params int[] modes);
	}
}
