// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class PersonWrapper : Default.Wrappers.PersonWrapper, IActumPersonWrapper {
    private readonly IActumPerson _person;

    [UsedImplicitly]
    public PersonWrapper(IPerson person, IHouseholdWrapper householdWrapper) : base(person, householdWrapper) {
      _person = (IActumPerson)person;
    }

    #region domain model properies

    public int MainOccupation {
      get => _person.MainOccupation;
      set => _person.MainOccupation = value;
    }

    public int EducationLevel {
      get => _person.EducationLevel;
      set => _person.EducationLevel = value;
    }

    public int HasBike {
      get => _person.HasBike;
      set => _person.HasBike = value;
    }

    public int HasDriversLicense {
      get => _person.HasDriversLicense;
      set => _person.HasDriversLicense = value;
    }

    public int HasCarShare {
      get => _person.HasCarShare;
      set => _person.HasCarShare = value;
    }

    public int Income {
      get => _person.Income;
      set => _person.Income = value;
    }

    public int HasMC {
      get => _person.HasMC;
      set => _person.HasMC = value;
    }

    public int HasMoped {
      get => _person.HasMoped;
      set => _person.HasMoped = value;
    }

    public int HasWorkParking {
      get => _person.HasWorkParking;
      set => _person.HasWorkParking = value;
    }

    public int WorkHoursPerWeek {
      get => _person.WorkHoursPerWeek;
      set => _person.WorkHoursPerWeek = value;
    }

    public int FlexibleWorkHours {
      get => _person.FlexibleWorkHours;
      set => _person.FlexibleWorkHours = value;
    }

    public int HasSchoolParking {
      get => _person.HasSchoolParking;
      set => _person.HasSchoolParking = value;
    }

    #endregion

    #region wrapper methods

    public virtual bool BikeExists() {
      return HasBike == 1;
    }

    public virtual bool DriversLicenseExists() {
      return HasDriversLicense == 1;
    }

    public virtual bool CarShareExists() {
      return HasCarShare == 1;
    }

    public virtual bool MCExists() {
      return HasMC == 1;
    }

    public virtual bool MopedExists() {
      return HasMoped == 1;
    }

    public virtual bool WorkParkingExists() {
      return HasWorkParking == 1;
    }

    public virtual bool SchoolParkingExists() {
      return HasSchoolParking == 1;
    }

    public virtual bool IsGymnasiumOrUniversityStudent() {
      return
          PersonType == Global.Settings.PersonTypes.DrivingAgeStudent ||
          PersonType == Global.Settings.PersonTypes.UniversityStudent;
    }

    public virtual int CollapseActumPersonTypes() {
      return
          PersonType <= Global.Settings.PersonTypes.UniversityStudent
              ? PersonType
              : PersonType - 1; //driving age students and children go from 6-8 to 5-7
    }

    #endregion
  }
}