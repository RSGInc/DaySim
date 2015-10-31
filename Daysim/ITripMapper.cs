using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.Framework.DomainModels.Models;

namespace Daysim {
	public interface ITripMapper
	{
		Dictionary<int, int> GetMapping(IEnumerable<ITrip> trips);
	}
}
