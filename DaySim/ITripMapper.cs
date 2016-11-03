using DaySim.Framework.DomainModels.Models;
using System.Collections.Generic;

namespace DaySim {
    public interface ITripMapper {
        Dictionary<int, int> GetMapping(IEnumerable<ITrip> trips);
    }
}
