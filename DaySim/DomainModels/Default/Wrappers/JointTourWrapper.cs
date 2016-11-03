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
    public class JointTourWrapper : IJointTourWrapper {
        private readonly IJointTour _jointTour;

        private readonly IPersisterExporter _exporter;

        [UsedImplicitly]
        public JointTourWrapper(IJointTour jointTour, IHouseholdDayWrapper householdDayWrapper) {
            _jointTour = jointTour;

            _exporter =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IJointTour>>()
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
            get { return _jointTour.Id; }
            set { _jointTour.Id = value; }
        }

        public int HouseholdDayId {
            get { return _jointTour.HouseholdDayId; }
            set { _jointTour.HouseholdDayId = value; }
        }

        public int HouseholdId {
            get { return _jointTour.HouseholdId; }
            set { _jointTour.HouseholdId = value; }
        }

        public int Day {
            get { return _jointTour.Day; }
            set { _jointTour.Day = value; }
        }

        public int Sequence {
            get { return _jointTour.Sequence; }
            set { _jointTour.Sequence = value; }
        }

        public int MainPurpose {
            get { return _jointTour.MainPurpose; }
            set { _jointTour.MainPurpose = value; }
        }

        public int Participants {
            get { return _jointTour.Participants; }
            set { _jointTour.Participants = value; }
        }

        public int PersonSequence1 {
            get { return _jointTour.PersonSequence1; }
            set { _jointTour.PersonSequence1 = value; }
        }

        public int TourSequence1 {
            get { return _jointTour.TourSequence1; }
            set { _jointTour.TourSequence1 = value; }
        }

        public int PersonSequence2 {
            get { return _jointTour.PersonSequence2; }
            set { _jointTour.PersonSequence2 = value; }
        }

        public int TourSequence2 {
            get { return _jointTour.TourSequence2; }
            set { _jointTour.TourSequence2 = value; }
        }

        public int PersonSequence3 {
            get { return _jointTour.PersonSequence3; }
            set { _jointTour.PersonSequence3 = value; }
        }

        public int TourSequence3 {
            get { return _jointTour.TourSequence3; }
            set { _jointTour.TourSequence3 = value; }
        }

        public int PersonSequence4 {
            get { return _jointTour.PersonSequence4; }
            set { _jointTour.PersonSequence4 = value; }
        }

        public int TourSequence4 {
            get { return _jointTour.TourSequence4; }
            set { _jointTour.TourSequence4 = value; }
        }

        public int PersonSequence5 {
            get { return _jointTour.PersonSequence5; }
            set { _jointTour.PersonSequence5 = value; }
        }

        public int TourSequence5 {
            get { return _jointTour.TourSequence5; }
            set { _jointTour.TourSequence5 = value; }
        }

        public int PersonSequence6 {
            get { return _jointTour.PersonSequence6; }
            set { _jointTour.PersonSequence6 = value; }
        }

        public int TourSequence6 {
            get { return _jointTour.TourSequence6; }
            set { _jointTour.TourSequence6 = value; }
        }

        public int PersonSequence7 {
            get { return _jointTour.PersonSequence7; }
            set { _jointTour.PersonSequence7 = value; }
        }

        public int TourSequence7 {
            get { return _jointTour.TourSequence7; }
            set { _jointTour.TourSequence7 = value; }
        }

        public int PersonSequence8 {
            get { return _jointTour.PersonSequence8; }
            set { _jointTour.PersonSequence8 = value; }
        }

        public int TourSequence8 {
            get { return _jointTour.TourSequence8; }
            set { _jointTour.TourSequence8 = value; }
        }

        #endregion

        #region flags/choice model/etc. properties

        public ITimeWindow TimeWindow { get; set; }

        #endregion

        #region wrapper methods

        public virtual void SetParticipantTourSequence(ITourWrapper participantTour) {
            if (PersonSequence1 == participantTour.Person.Sequence) {
                TourSequence1 = participantTour.Sequence;
            } else if (PersonSequence2 == participantTour.Person.Sequence) {
                TourSequence2 = participantTour.Sequence;
            } else if (PersonSequence3 == participantTour.Person.Sequence) {
                TourSequence3 = participantTour.Sequence;
            } else if (PersonSequence4 == participantTour.Person.Sequence) {
                TourSequence4 = participantTour.Sequence;
            } else if (PersonSequence5 == participantTour.Person.Sequence) {
                TourSequence5 = participantTour.Sequence;
            } else if (PersonSequence6 == participantTour.Person.Sequence) {
                TourSequence6 = participantTour.Sequence;
            } else if (PersonSequence7 == participantTour.Person.Sequence) {
                TourSequence7 = participantTour.Sequence;
            } else if (PersonSequence8 == participantTour.Person.Sequence) {
                TourSequence8 = participantTour.Sequence;
            }
        }

        #endregion

        #region init/utility/export methods

        public void Export() {
            _exporter.Export(_jointTour);
        }

        public static void Close() {
            Global
                .ContainerDaySim
                .GetInstance<IPersistenceFactory<IJointTour>>()
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