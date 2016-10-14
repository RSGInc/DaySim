// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Text;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Persisters;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using SimpleInjector;

namespace DaySim.DomainModels.Default.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
	public class FullHalfTourWrapper : IFullHalfTourWrapper {
		private readonly IFullHalfTour _fullHalfTour;

		private readonly IPersisterExporter _exporter;

		[UsedImplicitly]
		public FullHalfTourWrapper(IFullHalfTour fullHalfTour, IHouseholdDayWrapper householdDayWrapper) {
			_fullHalfTour = fullHalfTour;

			_exporter =
				Global
					.ContainerDaySim.GetInstance<IPersistenceFactory<IFullHalfTour>>()
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
			get { return _fullHalfTour.Id; }
			set { _fullHalfTour.Id = value; }
		}

		public int HouseholdDayId {
			get { return _fullHalfTour.HouseholdDayId; }
			set { _fullHalfTour.HouseholdDayId = value; }
		}

		public int HouseholdId {
			get { return _fullHalfTour.HouseholdId; }
			set { _fullHalfTour.HouseholdId = value; }
		}

		public int Day {
			get { return _fullHalfTour.Day; }
			set { _fullHalfTour.Day = value; }
		}

		public int Sequence {
			get { return _fullHalfTour.Sequence; }
			set { _fullHalfTour.Sequence = value; }
		}

		public int Direction {
			get { return _fullHalfTour.Direction; }
			set { _fullHalfTour.Direction = value; }
		}

		public int Participants {
			get { return _fullHalfTour.Participants; }
			set { _fullHalfTour.Participants = value; }
		}

		public int PersonSequence1 {
			get { return _fullHalfTour.PersonSequence1; }
			set { _fullHalfTour.PersonSequence1 = value; }
		}

		public int TourSequence1 {
			get { return _fullHalfTour.TourSequence1; }
			set { _fullHalfTour.TourSequence1 = value; }
		}

		public int PersonSequence2 {
			get { return _fullHalfTour.PersonSequence2; }
			set { _fullHalfTour.PersonSequence2 = value; }
		}

		public int TourSequence2 {
			get { return _fullHalfTour.TourSequence2; }
			set { _fullHalfTour.TourSequence2 = value; }
		}

		public int PersonSequence3 {
			get { return _fullHalfTour.PersonSequence3; }
			set { _fullHalfTour.PersonSequence3 = value; }
		}

		public int TourSequence3 {
			get { return _fullHalfTour.TourSequence3; }
			set { _fullHalfTour.TourSequence3 = value; }
		}

		public int PersonSequence4 {
			get { return _fullHalfTour.PersonSequence4; }
			set { _fullHalfTour.PersonSequence4 = value; }
		}

		public int TourSequence4 {
			get { return _fullHalfTour.TourSequence4; }
			set { _fullHalfTour.TourSequence4 = value; }
		}

		public int PersonSequence5 {
			get { return _fullHalfTour.PersonSequence5; }
			set { _fullHalfTour.PersonSequence5 = value; }
		}

		public int TourSequence5 {
			get { return _fullHalfTour.TourSequence5; }
			set { _fullHalfTour.TourSequence5 = value; }
		}

		public int PersonSequence6 {
			get { return _fullHalfTour.PersonSequence6; }
			set { _fullHalfTour.PersonSequence6 = value; }
		}

		public int TourSequence6 {
			get { return _fullHalfTour.TourSequence6; }
			set { _fullHalfTour.TourSequence6 = value; }
		}

		public int PersonSequence7 {
			get { return _fullHalfTour.PersonSequence7; }
			set { _fullHalfTour.PersonSequence7 = value; }
		}

		public int TourSequence7 {
			get { return _fullHalfTour.TourSequence7; }
			set { _fullHalfTour.TourSequence7 = value; }
		}

		public int PersonSequence8 {
			get { return _fullHalfTour.PersonSequence8; }
			set { _fullHalfTour.PersonSequence8 = value; }
		}

		public int TourSequence8 {
			get { return _fullHalfTour.TourSequence8; }
			set { _fullHalfTour.TourSequence8 = value; }
		}

		#endregion

		#region flags/choice model/etc. properties

		public bool Paired { get; set; }

		#endregion

		#region wrapper methods

		public virtual void SetParticipantTourSequence(ITourWrapper participantTour) {
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
			_exporter.Export(_fullHalfTour);
		}

		public static void Close() {
			Global
				.ContainerDaySim
				.GetInstance<IPersistenceFactory<IFullHalfTour>>()
				.Close();
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder
				.AppendLine(string.Format("Joint Tour ID: {0}",
					Id));

			builder.AppendLine(string.Format("Household ID: {0}, Day: {1}",
				HouseholdId,
				Day));

			return builder.ToString();
		}

		#endregion
	}
}