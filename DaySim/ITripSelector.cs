using DaySim.Framework.DomainModels.Models;
using System.Collections.Generic;

namespace DaySim {
    public interface ITripSelector {
        List<ITrip> Select(List<ITrip> trips, int mode, int pathType, int startTime, int endTime);
        double[][] GetTripTable(List<ITrip> selectedTrips, double factor, int matrixLength, Dictionary<int, int> mapping);
    }
}
