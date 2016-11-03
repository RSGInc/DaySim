// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Default.Wrappers {
    [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
    public class ZoneWrapper : IZoneWrapper {
        private readonly IZone _zone;

        [UsedImplicitly]
        public ZoneWrapper(IZone zone) {
            _zone = zone;
        }

        #region domain model properies

        public int Id {
            get { return _zone.Id; }
            set { _zone.Id = value; }
        }

        public int Key {
            get { return _zone.Key; }
            set { _zone.Key = value; }
        }

        public bool DestinationEligible {
            get { return _zone.DestinationEligible; }
            set { _zone.DestinationEligible = value; }
        }

        public int External {
            get { return _zone.External; }
            set { _zone.External = value; }
        }

        public int XCoordinate {
            get { return _zone.XCoordinate; }
            set { _zone.XCoordinate = value; }
        }

        public int YCoordinate {
            get { return _zone.YCoordinate; }
            set { _zone.YCoordinate = value; }
        }

        public double FractionWorkersWithJobsOutsideRegion {
            get { return _zone.FractionWorkersWithJobsOutsideRegion; }
            set { _zone.FractionWorkersWithJobsOutsideRegion = value; }
        }

        public double FractionJobsFilledByWorkersFromOutsideRegion {
            get { return _zone.FractionJobsFilledByWorkersFromOutsideRegion; }
            set { _zone.FractionJobsFilledByWorkersFromOutsideRegion = value; }
        }

        public int NearestStopAreaId {
            get { return _zone.NearestStopAreaId; }
            set { _zone.NearestStopAreaId = value; }
        }


        #endregion
    }
}