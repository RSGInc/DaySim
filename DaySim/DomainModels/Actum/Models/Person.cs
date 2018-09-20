// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;
using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.Framework.Factories;
using DaySim.Framework.Persistence;

namespace DaySim.DomainModels.Actum.Models {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  [Factory(Factory.PersistenceFactory, Category = Category.Model, DataType = DataType.Actum)]
  public sealed class Person : DomainModels.Default.Models.Person, IActumPerson {

    [ColumnName("ppriocc")]
    public int MainOccupation { get; set; }

    [ColumnName("pedul")]
    public int EducationLevel { get; set; }

    [ColumnName("phasbike")]
    public int HasBike { get; set; }

    [ColumnName("pdrivlic")]
    public int HasDriversLicense { get; set; }

    [ColumnName("pcarshar")]
    public int HasCarShare { get; set; }

    [ColumnName("pinc")]
    public int Income { get; set; }

    [ColumnName("phasmc")]
    public int HasMC { get; set; }

    [ColumnName("phasmop")]
    public int HasMoped { get; set; }

    [ColumnName("pwprkpos")]
    public int HasWorkParking { get; set; }

    [ColumnName("pwhrspw")]
    public int WorkHoursPerWeek { get; set; }

    [ColumnName("pwhrstyp")]
    public int FlexibleWorkHours { get; set; }

    [ColumnName("psprkpos")]
    public int HasSchoolParking { get; set; }
  }
}
