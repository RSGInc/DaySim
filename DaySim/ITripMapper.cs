using System.Collections.Generic;
using DaySim.Framework.DomainModels.Models;

namespace DaySim {
  public interface ITripMapper {
    Dictionary<int, int> GetMapping(IEnumerable<ITrip> trips);
  }
}
