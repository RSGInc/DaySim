// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Persisters;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Default.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
  public class HouseholdDayWrapper : IHouseholdDayWrapper {
    private IHouseholdDay _householdDay;

    private readonly IPersisterExporter _exporter;

    private readonly IHouseholdDayCreator _householdDayCreator;

    private readonly IPersisterReader<IPersonDay> _personDayReader;
    private readonly IPersonDayCreator _personDayCreator;

    private readonly IPersisterReader<IJointTour> _jointTourReader;
    private readonly IJointTourCreator _jointTourCreator;

    private readonly IPersisterReader<IFullHalfTour> _fullHalfTourReader;
    private readonly IFullHalfTourCreator _fullHalfTourCreator;

    private readonly IPersisterReader<IPartialHalfTour> _partialHalfTourReader;
    private readonly IPartialHalfTourCreator _partialHalfTourCreator;

    private int _fullHalfTourSequence;
    private int _partialHalfTourSequence;
    private int _jointTourSequence;

    [UsedImplicitly]
    public HouseholdDayWrapper(IHouseholdDay householdDay, IHouseholdWrapper householdWrapper) {
      _householdDay = householdDay;

      _exporter =
          Global
              .ContainerDaySim.GetInstance<IPersistenceFactory<IHouseholdDay>>()
              .Exporter;

      // household day fields

      _householdDayCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IHouseholdDayCreator>>()
              .Creator;

      // person day fields

      _personDayReader =
          Global
              .ContainerDaySim
              .GetInstance<IPersistenceFactory<IPersonDay>>()
              .Reader;

      _personDayCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IPersonDayCreator>>()
              .Creator;

      // joint tour fields

      _jointTourReader =
          Global
              .ContainerDaySim
              .GetInstance<IPersistenceFactory<IJointTour>>()
              .Reader;

      _jointTourCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IJointTourCreator>>()
              .Creator;

      // full half tour fields

      _fullHalfTourReader =
          Global
              .ContainerDaySim
              .GetInstance<IPersistenceFactory<IFullHalfTour>>()
              .Reader;

      _fullHalfTourCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IFullHalfTourCreator>>()
              .Creator;

      // partial half tour fields

      _partialHalfTourReader =
          Global
              .ContainerDaySim
              .GetInstance<IPersistenceFactory<IPartialHalfTour>>()
              .Reader;

      _partialHalfTourCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IPartialHalfTourCreator>>()
              .Creator;

      // relations properties

      Household = householdWrapper;

      SetPersonDays();

      // domain model properies

      SetExpansionFactor();

      if (!Global.Settings.UseJointTours) {
        return;
      }

      SetJointTours();
      SetFullHalfTours();
      SetPartialHalfTours();
    }

    #region relations properties

    public IHouseholdWrapper Household { get; set; }

    public List<IPersonDayWrapper> PersonDays { get; set; }

    public List<IJointTourWrapper> JointToursList { get; set; }

    public List<IFullHalfTourWrapper> FullHalfToursList { get; set; }

    public List<IPartialHalfTourWrapper> PartialHalfToursList { get; set; }

    #endregion

    #region domain model properies

    public int Id {
      get => _householdDay.Id;
      set => _householdDay.Id = value;
    }

    public int HouseholdId {
      get => _householdDay.HouseholdId;
      set => _householdDay.HouseholdId = value;
    }

    public int Day {
      get => _householdDay.Day;
      set => _householdDay.Day = value;
    }

    public int DayOfWeek {
      get => _householdDay.DayOfWeek;
      set => _householdDay.DayOfWeek = value;
    }

    public int JointTours {
      get => _householdDay.JointTours;
      set => _householdDay.JointTours = value;
    }

    public int FullHalfTours {
      get => _householdDay.FullHalfTours;
      set => _householdDay.FullHalfTours = value;
    }

    public int PartialHalfTours {
      get => _householdDay.PartialHalfTours;
      set => _householdDay.PartialHalfTours = value;
    }

    public double ExpansionFactor {
      get => _householdDay.ExpansionFactor;
      set => _householdDay.ExpansionFactor = value;
    }

    #endregion

    #region flags/choice model/etc. properties

    public int AttemptedSimulations { get; set; }

    public bool IsMissingData { get; set; }

    public bool IsValid { get; set; }

    #endregion

    #region wrapper methods

    public virtual IJointTourWrapper CreateJointTour(IHouseholdDayWrapper householdDay, IEnumerable<IPersonDayWrapper> orderedPersonDays, int[] participants, int purpose) {
      householdDay.JointTours++;

      int i = 0;
      int j = 0;
      int[] personSequence = new int[9];

      foreach (IPersonDayWrapper personDay in orderedPersonDays) {
        i++;

        if (i > 5 || participants[i] != 1) {
          continue;
        }

        j++;

        personSequence[j] = personDay.Person.Sequence;
      }

      IJointTour model = _jointTourCreator.CreateModel();

      model.Sequence = ++_jointTourSequence;
      model.Id = Id * 10 + _jointTourSequence;
      model.HouseholdDayId = Id;
      model.HouseholdId = HouseholdId;
      model.Day = Day;
      model.MainPurpose = purpose;
      model.Participants = participants[7];
      model.PersonSequence1 = personSequence[1];
      model.TourSequence1 = 0;
      model.PersonSequence2 = personSequence[2];
      model.TourSequence2 = 0;
      model.PersonSequence3 = personSequence[3];
      model.TourSequence3 = 0;
      model.PersonSequence4 = personSequence[4];
      model.TourSequence4 = 0;
      model.PersonSequence5 = personSequence[5];
      model.TourSequence5 = 0;
      model.PersonSequence6 = personSequence[6];
      model.TourSequence6 = 0;
      model.PersonSequence7 = personSequence[7];
      model.TourSequence7 = 0;
      model.PersonSequence8 = personSequence[8];
      model.TourSequence8 = 0;

      IJointTourWrapper jointTour = _jointTourCreator.CreateWrapper(model, this);

      householdDay.JointToursList.Add(jointTour);

      return jointTour;
    }

    public virtual IFullHalfTourWrapper CreateFullHalfTour(IHouseholdDayWrapper householdDay, IEnumerable<IPersonDayWrapper> orderedPersonDays, int[] participants, int direction) {
      householdDay.FullHalfTours++;

      int i = 0;
      int j = 0;
      int[] personSequence = new int[9];

      foreach (IPersonDayWrapper personDay in orderedPersonDays) {
        i++;

        if (i > 5 || participants[i] != 1) {
          continue;
        }

        j++;

        personSequence[j] = personDay.Person.Sequence;
      }

      IFullHalfTour model = _fullHalfTourCreator.CreateModel();

      model.Sequence = ++_fullHalfTourSequence;
      model.Id = Id * 10 + _fullHalfTourSequence;
      model.HouseholdDayId = Id;
      model.HouseholdId = HouseholdId;
      model.Day = Day;
      model.Direction = direction;
      model.Participants = participants[7];
      model.PersonSequence1 = personSequence[1];
      model.TourSequence1 = 0;
      model.PersonSequence2 = personSequence[2];
      model.TourSequence2 = 0;
      model.PersonSequence3 = personSequence[3];
      model.TourSequence3 = 0;
      model.PersonSequence4 = personSequence[4];
      model.TourSequence4 = 0;
      model.PersonSequence5 = personSequence[5];
      model.TourSequence5 = 0;
      model.PersonSequence6 = personSequence[6];
      model.TourSequence6 = 0;
      model.PersonSequence7 = personSequence[7];
      model.TourSequence7 = 0;
      model.PersonSequence8 = personSequence[8];
      model.TourSequence8 = 0;

      IFullHalfTourWrapper fullHalfTour = _fullHalfTourCreator.CreateWrapper(model, householdDay);

      householdDay.FullHalfToursList.Add(fullHalfTour);

      return fullHalfTour;
    }

    public virtual IPartialHalfTourWrapper CreatePartialHalfTour(IHouseholdDayWrapper householdDay, IEnumerable<IPersonDayWrapper> orderedPersonDays, int[] participants, int[] pickOrder, double[] distanceFromChauffeur, int direction) {
      householdDay.PartialHalfTours++;

      int[] personSequence = new int[9];
      int i = 0;

      foreach (IPersonDayWrapper personDay in orderedPersonDays) {
        i++;

        for (int i2 = 0; i2 < 5; i2++) {
          if (pickOrder[i2] == i) {
            personSequence[i2] = personDay.Person.Sequence;
          }
        }
      }

      IPartialHalfTour model = _partialHalfTourCreator.CreateModel();

      model.Sequence = ++_partialHalfTourSequence;
      model.Id = Id * 10 + _partialHalfTourSequence;
      model.HouseholdDayId = Id;
      model.HouseholdId = HouseholdId;
      model.Day = Day;
      model.Direction = direction;
      model.Participants = participants[7];
      model.PersonSequence1 = distanceFromChauffeur[0] > 998 ? 0 : personSequence[0];
      model.TourSequence1 = 0;
      model.PersonSequence2 = distanceFromChauffeur[1] > 998 ? 0 : personSequence[1];
      model.TourSequence2 = 0;
      model.PersonSequence3 = distanceFromChauffeur[2] > 998 ? 0 : personSequence[2];
      model.TourSequence3 = 0;
      model.PersonSequence4 = distanceFromChauffeur[3] > 998 ? 0 : personSequence[3];
      model.TourSequence4 = 0;
      model.PersonSequence5 = distanceFromChauffeur[4] > 998 ? 0 : personSequence[4];
      model.TourSequence5 = 0;
      model.PersonSequence6 = 0; //distanceFromChauffeur[5] > 998 ? 0 : personSequence[5],
      model.TourSequence6 = 0;
      model.PersonSequence7 = 0; //distanceFromChauffeur[6] > 998 ? 0 : personSequence[6],
      model.TourSequence7 = 0;
      model.PersonSequence8 = 0; //distanceFromChauffeur[7] > 998 ? 0 : personSequence[7],
      model.TourSequence8 = 0;

      IPartialHalfTourWrapper partialHalfTour = _partialHalfTourCreator.CreateWrapper(model, householdDay);

      householdDay.PartialHalfToursList.Add(partialHalfTour);

      return partialHalfTour;
    }

    #endregion

    #region persistence methods

    private IEnumerable<IPersonDay> LoadPersonDaysFromFile() {
      return
          _personDayReader
              .Seek(Id, "household_day_fk");
    }

    private IEnumerable<IJointTour> LoadJointToursFromFile() {
      return
          _jointTourReader
              .Seek(Id, "household_day_fk");
    }

    private IEnumerable<IFullHalfTour> LoadFullHalfToursFromFile() {
      return
          _fullHalfTourReader
              .Seek(Id, "household_day_fk");
    }

    private IEnumerable<IPartialHalfTour> LoadPartialHalfToursFromFile() {
      return
          _partialHalfTourReader
              .Seek(Id, "household_day_fk");
    }

    private IPersonDayWrapper CreatePersonDay(IPersonWrapper person) {
      IPersonDay model = _personDayCreator.CreateModel();

      model.Id = person.Id;
      model.PersonId = person.Id;
      model.HouseholdDayId = Id;
      model.HouseholdId = HouseholdId;
      model.PersonSequence = person.Sequence;
      model.Day = Day;

      return _personDayCreator.CreateWrapper(model, person, this);
    }

    private List<IPersonDayWrapper> GetPersonDaySurveyData() {
      List<IPersonDayWrapper> data = new List<IPersonDayWrapper>();
      List<IPersonDay> personDaysForHousehold = LoadPersonDaysFromFile().ToList();

      foreach (IPersonWrapper person in Household.Persons) {
        int personId = person.Id;

        IEnumerable<IPersonDay> personDays =
                    personDaysForHousehold
                        .Where(pd => pd.PersonId == personId);

        data.AddRange(
            personDays
                .Select(personDay =>
                    _personDayCreator
                        .CreateWrapper(personDay, person, this)));
      }

      return data;
    }

    private List<IPersonDayWrapper> GetPersonDaySimulatedData() {
      return
          Household
              .Persons
              .Select(CreatePersonDay)
              .ToList();
    }

    #endregion

    #region init/utility/export methods

    public void Export() {
      _exporter.Export(_householdDay);
    }

    public void Reset() {
      _householdDay = ResetHouseholdDay();

      _fullHalfTourSequence = 0;
      _partialHalfTourSequence = 0;
      _jointTourSequence = 0;

      SetJointTours();
      SetPartialHalfTours();
      SetFullHalfTours();

      foreach (IPersonDayWrapper personDay in PersonDays) {
        personDay.Reset();
      }
    }

    protected virtual IHouseholdDay ResetHouseholdDay() {
      IHouseholdDay model = _householdDayCreator.CreateModel();

      model.Id = Id;
      model.HouseholdId = HouseholdId;
      model.Day = Day;
      model.DayOfWeek = DayOfWeek;
      model.ExpansionFactor = ExpansionFactor;

      return model;
    }

    public static void Close() {
      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHouseholdDay>>()
          .Close();
    }

    public override string ToString() {
      StringBuilder builder = new StringBuilder();

      builder
          .AppendLine(string.Format("Household Day ID: {0}",
              Id));

      builder
          .AppendLine(string.Format("Household ID: {0}, Day: {1}",
              HouseholdId,
              Day));

      return builder.ToString();
    }

    private void SetPersonDays() {
      PersonDays =
          Global.Configuration.IsInEstimationMode
              ? GetPersonDaySurveyData()
              : GetPersonDaySimulatedData();
    }

    private void SetJointTours() {
      JointToursList =
          Global.Configuration.IsInEstimationMode && Global.Settings.UseJointTours
              ? LoadJointToursFromFile()
                  .Select(x => _jointTourCreator.CreateWrapper(x, this))
                  .ToList()
              : new List<IJointTourWrapper>();
    }

    private void SetFullHalfTours() {
      FullHalfToursList =
          Global.Configuration.IsInEstimationMode
              ? LoadFullHalfToursFromFile()
                  .Select(x => _fullHalfTourCreator.CreateWrapper(x, this))
                  .ToList()
              : new List<IFullHalfTourWrapper>();
    }

    private void SetPartialHalfTours() {
      PartialHalfToursList =
          Global.Configuration.IsInEstimationMode
              ? LoadPartialHalfToursFromFile()
                  .Select(x => _partialHalfTourCreator.CreateWrapper(x, this))
                  .ToList()
              : new List<IPartialHalfTourWrapper>();
    }

    private void SetExpansionFactor() {
      ExpansionFactor = Household.ExpansionFactor * Global.Configuration.HouseholdSamplingRateOneInX;
    }

    #endregion
  }
}