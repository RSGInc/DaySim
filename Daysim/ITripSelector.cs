using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.Framework.DomainModels.Models;

namespace Daysim {
	public interface ITripSelector
	{
		List<ITrip> Select(List<ITrip> trips, int mode, int pathType, int startTime, int endTime);
		double[][] GetTripTable(List<ITrip> selectedTrips, double factor, int matrixLength, Dictionary<int,int> mapping );
	}
}
