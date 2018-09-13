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
  public sealed class Household : IActumHousehold {
    [ColumnName("hhno")]
    public int Id { get; set; }

    [ColumnName("fraction_with_jobs_outside")]
    public double FractionWorkersWithJobsOutsideRegion { get; set; }

    [ColumnName("hhsize")]
    public int Size { get; set; }

    [ColumnName("hhvehs")]
    public int VehiclesAvailable { get; set; }

    [ColumnName("hhwkrs")]
    public int Workers { get; set; }

    [ColumnName("hhftw")]
    public int FulltimeWorkers { get; set; }

    [ColumnName("hhptw")]
    public int PartTimeWorkers { get; set; }

    [ColumnName("hhret")]
    public int RetiredAdults { get; set; }

    [ColumnName("hhoad")]
    public int OtherAdults { get; set; }

    [ColumnName("hhuni")]
    public int CollegeStudents { get; set; }

    [ColumnName("hhhsc")]
    public int HighSchoolStudents { get; set; }

    [ColumnName("hh515")]
    public int KidsBetween5And15 { get; set; }

    [ColumnName("hhcu5")]
    public int KidsBetween0And4 { get; set; }

    [ColumnName("hhincome")]
    public int Income { get; set; }

    [ColumnName("hownrent")]
    public int OwnOrRent { get; set; }

    [ColumnName("hrestype")]
    public int ResidenceType { get; set; }

    [ColumnName("hhparcel")]
    public int ResidenceParcelId { get; set; }

    [ColumnName("zone_id")]
    public int ResidenceZoneId { get; set; }

    [ColumnName("hhtaz")]
    public int ResidenceZoneKey { get; set; }

    [ColumnName("hhexpfac")]
    public double ExpansionFactor { get; set; }

    [ColumnName("samptype")]
    public int SampleType { get; set; }

    [ColumnName("hmuncode")]
    public int MunicipalCode { get; set; }

    [ColumnName("hdisstat")]
    public double StationDistance { get; set; }

    [ColumnName("hparkpos")]
    public int ParkingAvailability { get; set; }

    [ColumnName("inetpaym")]
    public int InternetPaymentMethod { get; set; }
  }
}