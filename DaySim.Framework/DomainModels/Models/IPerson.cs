// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface IPerson : IModel {
    int HouseholdId { get; set; }

    int Sequence { get; set; }

    int PersonType { get; set; }

    int Age { get; set; }

    int Gender { get; set; }

    int WorkerType { get; set; }

    int UsualWorkParcelId { get; set; }

    int UsualWorkZoneKey { get; set; }

    double AutoTimeToUsualWork { get; set; }

    double AutoDistanceToUsualWork { get; set; }

    int StudentType { get; set; }

    int UsualSchoolParcelId { get; set; }

    int UsualSchoolZoneKey { get; set; }

    double AutoTimeToUsualSchool { get; set; }

    double AutoDistanceToUsualSchool { get; set; }

    int UsualModeToWork { get; set; }

    int UsualArrivalPeriodToWork { get; set; }

    int UsualDeparturePeriodFromWork { get; set; }

    int TransitPassOwnership { get; set; }

    int PaidParkingAtWorkplace { get; set; }

    int PaperDiary { get; set; }

    int ProxyResponse { get; set; }

    double ExpansionFactor { get; set; }
  }
}
