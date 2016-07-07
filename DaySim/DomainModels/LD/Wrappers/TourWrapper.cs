// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using Daysim.DomainModels.LD.Models.Interfaces;
using Daysim.DomainModels.LD.Wrappers.Interfaces;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;

namespace Daysim.DomainModels.LD.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.LD)]
	public class TourWrapper : Default.Wrappers.TourWrapper, ILDTourWrapper {
		private readonly ILDTour _tour;

		[UsedImplicitly]
		public TourWrapper(ITour tour, IPersonWrapper personWrapper, IPersonDayWrapper personDayWrapper, IParcelWrapper originParcel, IParcelWrapper destinationParcel, int destinationArrivalTime, int destinationDepartureTime, int destinationPurpose) : base(tour, personWrapper, personDayWrapper, originParcel, destinationParcel, destinationArrivalTime, destinationDepartureTime, destinationPurpose) {
			_tour = (ILDTour) tour;
		}

		[UsedImplicitly]
		public TourWrapper(ITour subtour, ITourWrapper tourWrapper, bool suppressRandomVOT = false) : base(subtour, tourWrapper, Global.Settings.Purposes.PersonalBusiness, suppressRandomVOT) {
			_tour = (ILDTour) subtour;
		}

		[UsedImplicitly]
		public TourWrapper(ITour subtour, ITourWrapper tourWrapper, int purpose, bool suppressRandomVOT = false) : base(subtour, tourWrapper, purpose, suppressRandomVOT) {
			_tour = (ILDTour) subtour;
		}

		[UsedImplicitly]
		public TourWrapper(ITour tour, IPersonDayWrapper personDayWrapper, bool suppressRandomVOT = false) : base(tour, personDayWrapper, Global.Settings.Purposes.PersonalBusiness, suppressRandomVOT) {
			_tour = (ILDTour) tour;
		}

		[UsedImplicitly]
		public TourWrapper(ITour tour, IPersonDayWrapper personDayWrapper, int purpose, bool suppressRandomVOT = false) : base(tour, personDayWrapper, purpose, suppressRandomVOT) {
			_tour = (ILDTour) tour;
		}

		#region wrapper methods

		public override int GetVotALSegment() {
			var segment =
				(DestinationPurpose == Global.Settings.Purposes.Work || DestinationPurpose == Global.Settings.Purposes.School || DestinationPurpose == Global.Settings.Purposes.Escort)
					? Global.Settings.VotALSegments.Medium
					: (DestinationPurpose == Global.Settings.Purposes.Business)
						? Global.Settings.VotALSegments.High
						: Global.Settings.VotALSegments.Low;

			return segment;
		}

		public virtual bool IsBusinessPurpose() {
			return DestinationPurpose == Global.Settings.Purposes.Business;
		}

		public virtual bool IsHovDriverMode() {
			return Mode == Global.Settings.Modes.HovDriver;
		}

		public virtual bool IsHovPassengerMode() {
			return Mode == Global.Settings.Modes.HovPassenger;
		}

		#endregion
	}
}