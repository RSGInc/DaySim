using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.Framework.DomainModels.Models;

namespace Daysim {
	public class TripMapper : ITripMapper
	{
		public Dictionary<int, int> GetMapping(IEnumerable<ITrip> trips)
		{
			List<int> zoneIds = new List<int>();
			foreach (var trip in trips)
			{
				zoneIds.Add(trip.DestinationZoneKey);
				zoneIds.Add(trip.OriginZoneKey);
			}
			List<int> sortedUniqueIds = zoneIds.Select(o => o).Distinct().ToList();
			sortedUniqueIds.Sort();
			int x = 0;
			Dictionary<int, int> mapping = new Dictionary<int, int>();
			foreach (var sortedUniqueId in sortedUniqueIds)
			{
				mapping.Add(sortedUniqueId, x);
				x++;
			}

			return mapping;
		}
	}
}
