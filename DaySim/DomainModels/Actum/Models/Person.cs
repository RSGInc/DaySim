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
  public sealed class Person : IActumPerson {
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("hhno")]
    public int HouseholdId { get; set; }

    [ColumnName("pno")]
    public int Sequence { get; set; }

    [ColumnName("pptyp")]
    public int PersonType { get; set; }

    [ColumnName("pagey")]
    public int Age { get; set; }

    [ColumnName("pgend")]
    public int Gender { get; set; }

    [ColumnName("pwtyp")]
    public int WorkerType { get; set; }

    [ColumnName("pwpcl")]
    public int UsualWorkParcelId { get; set; }

    [ColumnName("pwtaz")]
    public int UsualWorkZoneKey { get; set; }

    [ColumnName("pwautime")]
    public double AutoTimeToUsualWork { get; set; }

    [ColumnName("pwaudist")]
    public double AutoDistanceToUsualWork { get; set; }

    [ColumnName("pstyp")]
    public int StudentType { get; set; }

    [ColumnName("pspcl")]
    public int UsualSchoolParcelId { get; set; }

    [ColumnName("pstaz")]
    public int UsualSchoolZoneKey { get; set; }

    [ColumnName("psautime")]
    public double AutoTimeToUsualSchool { get; set; }

    [ColumnName("psaudist")]
    public double AutoDistanceToUsualSchool { get; set; }

    [ColumnName("puwmode")]
    public int UsualModeToWork { get; set; }

    [ColumnName("puwarrp")]
    public int UsualArrivalPeriodToWork { get; set; }

    [ColumnName("puwdepp")]
    public int UsualDeparturePeriodFromWork { get; set; }

    [ColumnName("ptpass")]
    public int TransitPassOwnership { get; set; }

    [ColumnName("ppaidprk")]
    public int PaidParkingAtWorkplace { get; set; }

    [ColumnName("pdiary")]
    public int PaperDiary { get; set; }

    [ColumnName("pproxy")]
    public int ProxyResponse { get; set; }

    [ColumnName("psexpfac")]
    public double ExpansionFactor { get; set; }

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