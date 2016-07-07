// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using Daysim.DomainModels.LD.Models.Interfaces;
using Daysim.DomainModels.LD.Wrappers.Interfaces;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;

namespace Daysim.DomainModels.LD.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.LD)]
	public class TripWrapper : Default.Wrappers.TripWrapper, ILDTripWrapper {
		private readonly ILDTrip _trip;

		[UsedImplicitly]
		public TripWrapper(ITrip trip, ITourWrapper tourWrapper, IHalfTour halfTour) : base(trip, tourWrapper, halfTour) {
			_trip = (ILDTrip) trip;
		}

		#region domain model properies

		public int BikePTCombination {
			get { return _trip.BikePTCombination; }
			set { _trip.BikePTCombination = value; }
		}

		public int EscortedDestinationPurpose {
			get { return _trip.EscortedDestinationPurpose; }
			set { _trip.EscortedDestinationPurpose = value; }
		}

		#endregion

		#region wrapper methods

		public override void SetDriverOrPassenger(List<ITripWrapper> trips) {
			if (Mode == Global.Settings.Modes.Walk || Mode == Global.Settings.Modes.Bike || Mode == Global.Settings.Modes.Transit || Mode == Global.Settings.Modes.SchoolBus || Mode == Global.Settings.Modes.Other) {
				DriverType = Global.Settings.DriverTypes.NotApplicable;
			}
			else if (Mode == Global.Settings.Modes.Sov || Mode == Global.Settings.Modes.HovDriver) {
				DriverType = Global.Settings.DriverTypes.Driver;
			}
			else if (Mode == Global.Settings.Modes.HovPassenger) {
				DriverType = Global.Settings.DriverTypes.Passenger;
			}
		}

		public override void SetTripValueOfTime() {
			var costDivisor =
				Mode == Global.Settings.Modes.HovDriver && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
					? Global.Configuration.Coefficients_HOV2CostDivisor_Work
					: Mode == Global.Settings.Modes.HovDriver && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
						? Global.Configuration.Coefficients_HOV2CostDivisor_Other
						: Mode == Global.Settings.Modes.HovPassenger && (Tour.DestinationPurpose == Global.Settings.Purposes.Work || Tour.DestinationPurpose == Global.Settings.Purposes.Business)
							? Global.Configuration.Coefficients_HOV3CostDivisor_Work
							: Mode == Global.Settings.Modes.HovPassenger && Tour.DestinationPurpose != Global.Settings.Purposes.Work && Tour.DestinationPurpose != Global.Settings.Purposes.Business
								? Global.Configuration.Coefficients_HOV3CostDivisor_Other
								: 1.0;

			ValueOfTime = (Tour.TimeCoefficient * 60) / (Tour.CostCoefficient / costDivisor);
		}

		public virtual bool IsBusinessDestinationPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Business;
		}

		public virtual bool IsBusinessOriginPurpose() {
			return OriginPurpose == Global.Settings.Purposes.Business;
		}

		#endregion
	}
}