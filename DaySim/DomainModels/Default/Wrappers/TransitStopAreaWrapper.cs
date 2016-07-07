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
	public class TransitStopAreaWrapper : ITransitStopAreaWrapper {
		private readonly ITransitStopArea _transitStopArea;

		[UsedImplicitly]
		public TransitStopAreaWrapper(ITransitStopArea transitStopArea) {
			_transitStopArea = transitStopArea;
		}

		#region domain model properies

		public int Id {
			get { return _transitStopArea.Id; }
			set { _transitStopArea.Id = value; }
		}

		public int Key {
			get { return _transitStopArea.Key; }
			set { _transitStopArea.Key = value; }
		}

		//public bool DestinationEligible {
		//	get { return _transitStopArea.DestinationEligible; }
		//	set { _transitStopArea.DestinationEligible = value; }
		//}

		//public bool External {
		//	get { return _transitStopArea.External; }
		//	set { _transitStopArea.External = value; }
		//}

		public int XCoordinate {
			get { return _transitStopArea.XCoordinate; }
			set { _transitStopArea.XCoordinate = value; }
		}

		public int YCoordinate {
			get { return _transitStopArea.YCoordinate; }
			set { _transitStopArea.YCoordinate = value; }
		}

		#endregion
	}
}