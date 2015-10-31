using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;

namespace Daysim {
	public class TripSelector : ITripSelector
	{
		public TripSelector()
		{
		}

		public List<ITrip> Select(List<ITrip> trips, int mode, int pathType, int startTime, int endTime)
		{
			if (endTime < startTime)
			return
				trips.Where(
					t => (t.Mode == mode) && (t.PathType == pathType) && ((((t.DepartureTime + t.ArrivalTime) / 2).ToMinutesAfter3AM() >= startTime) || (((t.DepartureTime + t.ArrivalTime) / 2).ToMinutesAfter3AM() <= endTime)))
				     .ToList();

			else
			{
				return trips.Where(
					t => (t.Mode == mode) && (t.PathType == pathType) && ((((t.DepartureTime + t.ArrivalTime) / 2).ToMinutesAfter3AM() >= startTime) && (((t.DepartureTime + t.ArrivalTime) / 2).ToMinutesAfter3AM() <= endTime)))
				     .ToList();
			}
		}

		public double[][] GetTripTable(List<ITrip> trips, double factor, int size, Dictionary<int, int> mapping )
		{
			var table = new double[size][];
			for (int x = 0; x < size; x++)
			{
				table[x] = new double[size];
			}
			foreach (var trip in trips)
			{
				table[mapping[trip.OriginZoneKey]][mapping[trip.DestinationZoneKey]] += factor;
			}

			return table;
		}
	}
}
