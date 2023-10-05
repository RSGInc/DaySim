﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaySim.ChoiceModels;
using DaySim.DomainModels.Shared;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Persisters;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Default.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Default)]
  public class HouseholdWrapper : IHouseholdWrapper {
    private readonly IHousehold _household;

    private readonly IPersisterExporter _exporter;

    private readonly IPersisterReader<IPerson> _personReader;
    private readonly IPersonCreator _personCreator;

    private readonly IPersisterReader<IHouseholdDay> _householdDayReader;
    private readonly IHouseholdDayCreator _householdDayCreator;

    [UsedImplicitly]
    public HouseholdWrapper(IHousehold household) {
      _household = household;

      _exporter =
          Global
              .ContainerDaySim.GetInstance<IPersistenceFactory<IHousehold>>()
              .Exporter;

      // person fields

      _personReader =
          Global
              .ContainerDaySim
              .GetInstance<IPersistenceFactory<IPerson>>()
              .Reader;

      _personCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IPersonCreator>>()
              .Creator;

      // household day fields

      _householdDayReader =
          Global
              .ContainerDaySim
              .GetInstance<IPersistenceFactory<IHouseholdDay>>()
              .Reader;

      _householdDayCreator =
          Global
              .ContainerDaySim
              .GetInstance<IWrapperFactory<IHouseholdDayCreator>>()
              .Creator;

      RandomUtility = new RandomUtility();
    }

    #region relations properties

    public List<IPersonWrapper> Persons { get; set; }

    public List<IHouseholdDayWrapper> HouseholdDays { get; set; }

    public IHouseholdTotals HouseholdTotals { get; set; }

    public IParcelWrapper ResidenceParcel { get; set; }

    #endregion

    #region domain model properies

    public int Id {
      get => _household.Id;
      set => _household.Id = value;
    }

    public double FractionWorkersWithJobsOutsideRegion {
      get => _household.FractionWorkersWithJobsOutsideRegion;
      set => _household.FractionWorkersWithJobsOutsideRegion = value;
    }

    public int Size {
      get => _household.Size;
      set => _household.Size = value;
    }

    public int VehiclesAvailable {
      get => _household.VehiclesAvailable;
      set => _household.VehiclesAvailable = value;
    }

    public int Workers {
      get => _household.Workers;
      set => _household.Workers = value;
    }

    public int FulltimeWorkers {
      get => _household.FulltimeWorkers;
      set => _household.FulltimeWorkers = value;
    }

    public int PartTimeWorkers {
      get => _household.PartTimeWorkers;
      set => _household.PartTimeWorkers = value;
    }

    public int RetiredAdults {
      get => _household.RetiredAdults;
      set => _household.RetiredAdults = value;
    }

    public int OtherAdults {
      get => _household.OtherAdults;
      set => _household.OtherAdults = value;
    }

    public int CollegeStudents {
      get => _household.CollegeStudents;
      set => _household.CollegeStudents = value;
    }

    public int HighSchoolStudents {
      get => _household.HighSchoolStudents;
      set => _household.HighSchoolStudents = value;
    }

    public int KidsBetween5And15 {
      get => _household.KidsBetween5And15;
      set => _household.KidsBetween5And15 = value;
    }

    public int KidsBetween0And4 {
      get => _household.KidsBetween0And4;
      set => _household.KidsBetween0And4 = value;
    }

    public int Income {
      get => _household.Income;
      set => _household.Income = value;
    }

    public int OwnOrRent {
      get => _household.OwnOrRent;
      set => _household.OwnOrRent = value;
    }

    public int ResidenceType {
      get => _household.ResidenceType;
      set => _household.ResidenceType = value;
    }

    public int ResidenceParcelId {
      get => _household.ResidenceParcelId;
      set => _household.ResidenceParcelId = value;
    }

    public int ResidenceZoneId {
      get => _household.ResidenceZoneId;
      set => _household.ResidenceZoneId = value;
    }

    public int ResidenceZoneKey {
      get => _household.ResidenceZoneKey;
      set => _household.ResidenceZoneKey = value;
    }

    public double ExpansionFactor {
      get => _household.ExpansionFactor;
      set => _household.ExpansionFactor = value;
    }

    public int SampleType {
      get => _household.SampleType;
      set => _household.SampleType = value;
    }

    #endregion

    #region flags/choice model/etc. properties

    //use the Residence data field for this
    public int OwnsAutomatedVehicles {
      get => _household.ResidenceType;
      set => _household.ResidenceType = value;
    }

    public int ResidenceBuffer2Density { get; set; }

    public bool IsOnePersonHousehold { get; set; }

    public bool IsTwoPersonHousehold { get; set; }

    public bool Has0To15KIncome { get; set; }

    public bool Has0To25KIncome { get; set; }

    public bool Has25To45KIncome { get; set; }

    public bool Has25To50KIncome { get; set; }

    public bool Has50To75KIncome { get; set; }

    public bool Has75To100KIncome { get; set; }

    public bool Has75KPlusIncome { get; set; }

    public bool Has100KPlusIncome { get; set; }

    public bool HasIncomeUnder50K { get; set; }

    public bool HasIncomeOver50K { get; set; }

    public bool HasValidIncome { get; set; }

    public bool HasMissingIncome { get; set; }

    public bool Has1Driver { get; set; }

    public bool Has2Drivers { get; set; }

    public bool Has3Drivers { get; set; }

    public bool Has4OrMoreDrivers { get; set; }

    public bool HasMoreDriversThan1 { get; set; }

    public bool HasMoreDriversThan2 { get; set; }

    public bool HasMoreDriversThan3 { get; set; }

    public bool HasMoreDriversThan4 { get; set; }

    public bool HasNoFullOrPartTimeWorker { get; set; }

    public bool Has1OrLessFullOrPartTimeWorkers { get; set; }

    public bool Has2OrLessFullOrPartTimeWorkers { get; set; }

    public bool Has3OrLessFullOrPartTimeWorkers { get; set; }

    public bool Has4OrLessFullOrPartTimeWorkers { get; set; }

    public bool HasChildrenUnder16 { get; set; }

    public bool HasChildrenUnder5 { get; set; }

    public bool HasChildrenAge5Through15 { get; set; }

    public bool HasChildren { get; set; }

    public int HouseholdType { get; set; }

    #endregion

    #region random/seed synchronization properties

    public IRandomUtility RandomUtility { get; set; }

    public int[] SeedValues { get; private set; }

    #endregion

    #region wrapper methods

    public virtual int GetVotALSegment() {
      int income2000 = (int)(Income * Global.Configuration.HouseholdIncomeAdjustmentFactorTo2000Dollars);
      int segment =
                (income2000 < Global.Settings.VotALSegments.IncomeLowMedium * Global.Settings.MonetaryUnitsPerDollar)
                    ? Global.Settings.VotALSegments.Low
                    : (income2000 < Global.Settings.VotALSegments.IncomeMediumHigh * Global.Settings.MonetaryUnitsPerDollar)
                        ? Global.Settings.VotALSegments.Medium
                        : Global.Settings.VotALSegments.High;

      return segment;
    }

    public virtual int GetCarsPerDriver() {
      return Math.Min(VehiclesAvailable / Math.Max(HouseholdTotals.DrivingAgeMembers, 1), 1);
    }

    public virtual int GetFlagForCarsLessThanDrivers(int householdCars) {
      return (householdCars > 0 && householdCars < HouseholdTotals.DrivingAgeMembers).ToFlag();
    }

    public virtual int GetFlagForCarsLessThanWorkers(int householdCars) {
      return (householdCars > 0 && householdCars < HouseholdTotals.FullAndPartTimeWorkers).ToFlag();
    }

    public virtual int GetFlagForNoCarsInHousehold(int householdCars) {
      return (householdCars == 0).ToFlag();
    }

    #endregion

    #region persistence methods

    private IEnumerable<IPerson> LoadPersonsFromFile() {
      return
          _personReader
              .Seek(Id, "household_fk");
    }

    private IEnumerable<IHouseholdDay> LoadHouseholdDaysFromFile() {
      return
          _householdDayReader
              .Seek(Id, "household_fk");
    }

    private IHouseholdDayWrapper CreateHouseholdDay() {
      IHouseholdDay model = _householdDayCreator.CreateModel();

      model.Id = Id;
      model.HouseholdId = Id;
      model.Day = 1;

      return _householdDayCreator.CreateWrapper(model, this);
    }

    #endregion

    #region init/utility/export methods

    public void Init() {
      // relations properties

      SetPersons();
      SetHouseholdDays();
      SetHouseholdTotals();
      SetParcelRelationships();

      FulltimeWorkers = HouseholdTotals.FulltimeWorkers;
      PartTimeWorkers = HouseholdTotals.PartTimeWorkers;
      RetiredAdults = HouseholdTotals.RetiredAdults;
      OtherAdults = HouseholdTotals.NonworkingAdults;
      CollegeStudents = HouseholdTotals.UniversityStudents;
      HighSchoolStudents = HouseholdTotals.DrivingAgeStudents;
      KidsBetween5And15 = HouseholdTotals.ChildrenAge5Through15;
      KidsBetween0And4 = HouseholdTotals.ChildrenUnder5;

      // domain model properies

      SetExpansionFactor();

      // flags/choice model/etc. properties

      ResidenceBuffer2Density = (int)Math.Round(ResidenceParcel.EmploymentTotalBuffer2 + ResidenceParcel.HouseholdsBuffer2 + ResidenceParcel.StudentsUniversityBuffer2, 0);
      if (Global.Configuration.WriteResidenceBufferDensityToOwnOrRent) { OwnOrRent = ResidenceBuffer2Density; }
      IsOnePersonHousehold = Size == 1;
      IsTwoPersonHousehold = Size == 2;
      int income2000 = (int)(Income * Global.Configuration.HouseholdIncomeAdjustmentFactorTo2000Dollars);
      Has0To15KIncome = income2000.IsRightExclusiveBetween(0, 15000);
      Has0To25KIncome = income2000.IsRightExclusiveBetween(0, 25000);
      Has25To45KIncome = income2000.IsRightExclusiveBetween(25000, 45000);
      Has25To50KIncome = income2000.IsRightExclusiveBetween(25000, 50000);
      Has50To75KIncome = income2000.IsRightExclusiveBetween(50000, 75000);
      Has75To100KIncome = income2000.IsRightExclusiveBetween(75000, 100000);
      Has75KPlusIncome = income2000 >= 75000;
      Has100KPlusIncome = income2000 >= 100000;
      HasIncomeUnder50K = income2000.IsRightExclusiveBetween(0, 50000);
      HasIncomeOver50K = income2000 >= 50000;
      HasValidIncome = Income >= 0;
      HasMissingIncome = Income < 0;
      Has1Driver = HouseholdTotals.DrivingAgeMembers == 1;
      Has2Drivers = HouseholdTotals.DrivingAgeMembers == 2;
      Has3Drivers = HouseholdTotals.DrivingAgeMembers == 3;
      Has4OrMoreDrivers = HouseholdTotals.DrivingAgeMembers >= 4;
      HasMoreDriversThan1 = HouseholdTotals.DrivingAgeMembers > 1;
      HasMoreDriversThan2 = HouseholdTotals.DrivingAgeMembers > 2;
      HasMoreDriversThan3 = HouseholdTotals.DrivingAgeMembers > 3;
      HasMoreDriversThan4 = HouseholdTotals.DrivingAgeMembers > 4;
      HasNoFullOrPartTimeWorker = HouseholdTotals.FullAndPartTimeWorkers <= 0;
      Has1OrLessFullOrPartTimeWorkers = HouseholdTotals.FullAndPartTimeWorkers <= 1;
      Has2OrLessFullOrPartTimeWorkers = HouseholdTotals.FullAndPartTimeWorkers <= 2;
      Has3OrLessFullOrPartTimeWorkers = HouseholdTotals.FullAndPartTimeWorkers <= 3;
      Has4OrLessFullOrPartTimeWorkers = HouseholdTotals.FullAndPartTimeWorkers <= 4;
      HasChildrenUnder16 = HouseholdTotals.ChildrenUnder16 > 0;
      HasChildrenUnder5 = HouseholdTotals.ChildrenUnder5 > 0;
      HasChildrenAge5Through15 = HouseholdTotals.ChildrenAge5Through15 > 0;
      HasChildren = HouseholdTotals.DrivingAgeStudents > 0 || HouseholdTotals.ChildrenUnder16 > 0;
      HouseholdType = 0;

      if (Size == 1 && (HouseholdTotals.AllWorkers > 0 || HouseholdTotals.AllStudents > 0)) {
        HouseholdType =
            Global.Settings.HouseholdTypes.IndividualWorkerStudent;
      } else if (Size == 1) {
        HouseholdType =
            Global.Settings.HouseholdTypes.IndividualNonworkerNonstudent;
      } else if (HouseholdTotals.Adults == 1) {
        HouseholdType =
            Global.Settings.HouseholdTypes.OneAdultWithChildren;
      } else if (HouseholdTotals.RetiredAdults == 0 && HouseholdTotals.NonworkingAdults == 0 && (HouseholdTotals.DrivingAgeStudents > 0 || HouseholdTotals.ChildrenUnder16 > 0)) {
        HouseholdType =
            Global.Settings.HouseholdTypes.TwoPlusWorkerStudentAdultsWithChildren;
      } else if (HouseholdTotals.DrivingAgeStudents > 0 || HouseholdTotals.ChildrenUnder16 > 0) {
        HouseholdType =
            Global.Settings.HouseholdTypes.TwoPlusAdultsOnePlusWorkersStudentsWithChildren;
      } else if (HouseholdTotals.RetiredAdults == 0 && HouseholdTotals.NonworkingAdults == 0) {
        HouseholdType =
            Global.Settings.HouseholdTypes.TwoPlusWorkerStudentAdultsWithoutChildren;
      } else if (HouseholdTotals.FullAndPartTimeWorkers > 0 || HouseholdTotals.UniversityStudents > 0) {
        HouseholdType =
            Global.Settings.HouseholdTypes.OnePlusWorkerStudentAdultsAndOnePlusNonworkerNonstudentAdultsWithoutChildren;
      } else {
        HouseholdType =
            Global.Settings.HouseholdTypes.TwoPlusNonworkerNonstudentAdultsWithoutChildren;
      }
    }

    public void Export() {
      _exporter.Export(_household);
    }

    public static void Close() {
      Global
          .ContainerDaySim
          .GetInstance<IPersistenceFactory<IHousehold>>()
          .Close();
    }

    public override string ToString() {
      StringBuilder builder = new StringBuilder();

      builder
          .AppendLine(string.Format("Household ID: {0}",
              Id));

      builder
          .AppendLine(string.Format("Residence Parcel ID: {0}: Residence Zone Key: {1}",
              ResidenceParcelId,
              ResidenceZoneKey));

      builder
          .AppendLine(string.Format("Vehicles Available: {0}",
              VehiclesAvailable));

      return builder.ToString();
    }

    private void SetPersons() {
      Persons =
          LoadPersonsFromFile()
              .Select(person => _personCreator.CreateWrapper(person, this))
              .ToList();

      if (!Global.Configuration.ShouldSynchronizeRandomSeed) {
        return;
      }

      foreach (IPersonWrapper person in Persons) {
        person.SeedValues =
            RandomUtility
                .GetSeedValues(Global.Settings.NumberOfRandomSeeds);
      }

      SeedValues = Persons[0].SeedValues;
    }

    private void SetHouseholdDays() {
      HouseholdDays =
          Global.Configuration.IsInEstimationMode
              ? LoadHouseholdDaysFromFile()
                  .Select(householdDay =>
                      _householdDayCreator
                          .CreateWrapper(householdDay, this))
                  .ToList()
              : new List<IHouseholdDayWrapper> {
                        CreateHouseholdDay()
              };
    }

    private void SetHouseholdTotals() {
      HouseholdTotals = new HouseholdTotals();

      foreach (IPersonWrapper person in Persons) {
        HouseholdTotals.FullAndPartTimeWorkers += person.IsFullOrPartTimeWorker.ToFlag();
        HouseholdTotals.FulltimeWorkers += person.IsFulltimeWorker.ToFlag();
        HouseholdTotals.PartTimeWorkers += person.IsPartTimeWorker.ToFlag();
        HouseholdTotals.RetiredAdults += person.IsRetiredAdult.ToFlag();
        HouseholdTotals.NonworkingAdults += person.IsNonworkingAdult.ToFlag();
        HouseholdTotals.UniversityStudents += person.IsUniversityStudent.ToFlag();
        HouseholdTotals.DrivingAgeStudents += person.IsDrivingAgeStudent.ToFlag();
        HouseholdTotals.ChildrenAge5Through15 += person.IsChildAge5Through15.ToFlag();
        HouseholdTotals.ChildrenUnder5 += person.IsChildUnder5.ToFlag();
        HouseholdTotals.ChildrenUnder16 += person.IsChildUnder16.ToFlag();
        HouseholdTotals.Adults += person.IsAdult.ToFlag();
        HouseholdTotals.AllWorkers += person.IsWorker.ToFlag();
        HouseholdTotals.AllStudents += person.IsStudent.ToFlag();
        HouseholdTotals.DrivingAgeMembers += person.IsDrivingAge.ToFlag();
        HouseholdTotals.WorkersPlusStudents += (person.IsFulltimeWorker.ToFlag() + person.IsPartTimeWorker.ToFlag() + person.IsUniversityStudent.ToFlag() + person.IsDrivingAgeStudent.ToFlag());
      }

      // home-based workers and students in household
      int homeBasedPersons = Persons.Count(p => (p.IsWorker && p.UsualWorkParcelId == ResidenceParcelId) || (p.IsStudent && p.UsualSchoolParcelId == ResidenceParcelId));

      HouseholdTotals.PartTimeWorkersPerDrivingAgeMembers = HouseholdTotals.PartTimeWorkers / Math.Max(HouseholdTotals.DrivingAgeMembers, 1D);
      HouseholdTotals.RetiredAdultsPerDrivingAgeMembers = HouseholdTotals.RetiredAdults / Math.Max(HouseholdTotals.DrivingAgeMembers, 1D);
      HouseholdTotals.UniversityStudentsPerDrivingAgeMembers = HouseholdTotals.UniversityStudents / Math.Max(HouseholdTotals.DrivingAgeMembers, 1D);
      HouseholdTotals.DrivingAgeStudentsPerDrivingAgeMembers = HouseholdTotals.DrivingAgeStudents / Math.Max(HouseholdTotals.DrivingAgeMembers, 1D);
      HouseholdTotals.ChildrenUnder5PerDrivingAgeMembers = HouseholdTotals.ChildrenUnder5 / Math.Max(HouseholdTotals.DrivingAgeMembers, 1D);
      HouseholdTotals.HomeBasedPersonsPerDrivingAgeMembers = homeBasedPersons / Math.Max(HouseholdTotals.DrivingAgeMembers, 1D);
    }

    private void SetParcelRelationships() {
      ResidenceParcel = ChoiceModelFactory.Parcels[ResidenceParcelId];
    }

    private void SetExpansionFactor() {
      ExpansionFactor *= Global.Configuration.HouseholdSamplingRateOneInX;
    }


    #endregion
  }
}
