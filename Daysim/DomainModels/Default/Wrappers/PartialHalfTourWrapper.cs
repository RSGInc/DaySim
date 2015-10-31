// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Text;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Persisters;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;
using Ninject;

namespace Daysim.DomainModels.Default.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
	public class PartialHalfTourWrapper : IPartialHalfTourWrapper {
		private readonly IPartialHalfTour _partialHalfTour;

		private readonly IPersisterExporter _exporter;

		[UsedImplicitly]
		public PartialHalfTourWrapper(IPartialHalfTour partialHalfTour, IHouseholdDayWrapper householdDayWrapper) {
			_partialHalfTour = partialHalfTour;

			_exporter =
				Global
					.Kernel
					.Get<IPersistenceFactory<IPartialHalfTour>>()
					.Exporter;

			// relations properties

			Household = householdDayWrapper.Household;
			HouseholdDay = householdDayWrapper;
		}

		#region relations properties

		public IHouseholdWrapper Household { get; private set; }

		public IHouseholdDayWrapper HouseholdDay { get; private set; }

		#endregion

		#region domain model properies

		public int Id {
			get { return _partialHalfTour.Id; }
			set { _partialHalfTour.Id = value; }
		}

		public int HouseholdDayId {
			get { return _partialHalfTour.HouseholdDayId; }
			set { _partialHalfTour.HouseholdDayId = value; }
		}

		public int HouseholdId {
			get { return _partialHalfTour.HouseholdId; }
			set { _partialHalfTour.HouseholdId = value; }
		}

		public int Day {
			get { return _partialHalfTour.Day; }
			set { _partialHalfTour.Day = value; }
		}

		public int Sequence {
			get { return _partialHalfTour.Sequence; }
			set { _partialHalfTour.Sequence = value; }
		}

		public int Direction {
			get { return _partialHalfTour.Direction; }
			set { _partialHalfTour.Direction = value; }
		}

		public int Participants {
			get { return _partialHalfTour.Participants; }
			set { _partialHalfTour.Participants = value; }
		}

		public int PersonSequence1 {
			get { return _partialHalfTour.PersonSequence1; }
			set { _partialHalfTour.PersonSequence1 = value; }
		}

		public int TourSequence1 {
			get { return _partialHalfTour.TourSequence1; }
			set { _partialHalfTour.TourSequence1 = value; }
		}

		public int PersonSequence2 {
			get { return _partialHalfTour.PersonSequence2; }
			set { _partialHalfTour.PersonSequence2 = value; }
		}

		public int TourSequence2 {
			get { return _partialHalfTour.TourSequence2; }
			set { _partialHalfTour.TourSequence2 = value; }
		}

		public int PersonSequence3 {
			get { return _partialHalfTour.PersonSequence3; }
			set { _partialHalfTour.PersonSequence3 = value; }
		}

		public int TourSequence3 {
			get { return _partialHalfTour.TourSequence3; }
			set { _partialHalfTour.TourSequence3 = value; }
		}

		public int PersonSequence4 {
			get { return _partialHalfTour.PersonSequence4; }
			set { _partialHalfTour.PersonSequence4 = value; }
		}

		public int TourSequence4 {
			get { return _partialHalfTour.TourSequence4; }
			set { _partialHalfTour.TourSequence4 = value; }
		}

		public int PersonSequence5 {
			get { return _partialHalfTour.PersonSequence5; }
			set { _partialHalfTour.PersonSequence5 = value; }
		}

		public int TourSequence5 {
			get { return _partialHalfTour.TourSequence5; }
			set { _partialHalfTour.TourSequence5 = value; }
		}

		public int PersonSequence6 {
			get { return _partialHalfTour.PersonSequence6; }
			set { _partialHalfTour.PersonSequence6 = value; }
		}

		public int TourSequence6 {
			get { return _partialHalfTour.TourSequence6; }
			set { _partialHalfTour.TourSequence6 = value; }
		}

		public int PersonSequence7 {
			get { return _partialHalfTour.PersonSequence7; }
			set { _partialHalfTour.PersonSequence7 = value; }
		}

		public int TourSequence7 {
			get { return _partialHalfTour.TourSequence7; }
			set { _partialHalfTour.TourSequence7 = value; }
		}

		public int PersonSequence8 {
			get { return _partialHalfTour.PersonSequence8; }
			set { _partialHalfTour.PersonSequence8 = value; }
		}

		public int TourSequence8 {
			get { return _partialHalfTour.TourSequence8; }
			set { _partialHalfTour.TourSequence8 = value; }
		}

		#endregion

		#region flags/choice model/etc. properties

		public bool Paired { get; set; }

		#endregion

		#region wrapper methods

		public void SetParticipantTourSequence(ITourWrapper participantTour) {
			if (PersonSequence1 == participantTour.Person.Sequence) {
				TourSequence1 = participantTour.Sequence;
			}
			else if (PersonSequence2 == participantTour.Person.Sequence) {
				TourSequence2 = participantTour.Sequence;
			}
			else if (PersonSequence3 == participantTour.Person.Sequence) {
				TourSequence3 = participantTour.Sequence;
			}
			else if (PersonSequence4 == participantTour.Person.Sequence) {
				TourSequence4 = participantTour.Sequence;
			}
			else if (PersonSequence5 == participantTour.Person.Sequence) {
				TourSequence5 = participantTour.Sequence;
			}
			else if (PersonSequence6 == participantTour.Person.Sequence) {
				TourSequence6 = participantTour.Sequence;
			}
			else if (PersonSequence7 == participantTour.Person.Sequence) {
				TourSequence7 = participantTour.Sequence;
			}
			else if (PersonSequence8 == participantTour.Person.Sequence) {
				TourSequence8 = participantTour.Sequence;
			}
		}

		#endregion

		#region init/utility/export methods

		public void Export() {
			_exporter.Export(_partialHalfTour);
		}

		public static void Close() {
			Global
				.Kernel
				.Get<IPersistenceFactory<IPartialHalfTour>>()
				.Close();
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder
				.AppendLine(string.Format("Joint Tour ID: {0}",
					Id));

			builder
				.AppendLine(string.Format("Household ID: {0}, Day: {1}",
					HouseholdId,
					Day));

			return builder.ToString();
		}

		#endregion
	}
}