using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaySim.Framework.DomainModels.Models;

namespace DaySim {
    public interface ITripMapper {
        Dictionary<int, int> GetMapping(IEnumerable<ITrip> trips);
    }
}
