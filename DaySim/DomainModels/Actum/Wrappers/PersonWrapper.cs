// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class PersonWrapper : Default.Wrappers.PersonWrapper, IActumPersonWrapper {
    private readonly IActumPerson _person;

    [UsedImplicitly]
    public PersonWrapper(Framework.DomainModels.Models.IPerson person, Framework.DomainModels.Wrappers.IHouseholdWrapper householdWrapper) : base(person, householdWrapper) {
      _person = (IActumPerson)person;
    }

    #region domain model properies


    public int HasDriversLicense {
      get => _person.HasDriversLicense;
      set => _person.HasDriversLicense = value;
    }

    public int PersonalIncome {
      get => _person.PersonalIncome;
      set => _person.PersonalIncome = value;
    }

    public int OccupationCode {
      get => _person.OccupationCode;
      set => _person.OccupationCode = value;
    }


    #endregion

    #region wrapper methods


    public bool DriversLicenseExists() {
      return HasDriversLicense == 1;
    }

    public override int GetPersonalIncome() {
      return PersonalIncome;
    }


    public bool IsGymnasiumOrUniversityStudent() {
      return
          PersonType == Global.Settings.PersonTypes.DrivingAgeStudent ||
          PersonType == Global.Settings.PersonTypes.UniversityStudent;
    }

    public int CollapseActumPersonTypes() {
      return
          PersonType <= Global.Settings.PersonTypes.UniversityStudent
              ? PersonType
              : PersonType - 1; //driving age students and children go from 6-8 to 5-7
    }

    #endregion
  }
}
