// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.DomainModels.Actum.Models.Interfaces;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.DomainModels.Actum.Wrappers.Interfaces {
  public interface IActumTourWrapper : ITourWrapper, IActumTour {


    #region relations properties

    //IActumHouseholdWrapper Household { get; set; }

    //IActumPersonWrapper Person { get; set; }

    //IActumPersonDayWrapper PersonDay { get; set; }

    //IActumTourWrapper ParentTour { get; set; }

    #endregion


    #region flags/choice model/etc. properties

    int HalfTour1AccessMode { get; set; }

    int HalfTour1AccessPathType { get; set; }

    double HalfTour1AccessTime { get; set; }

    double HalfTour1AccessCost { get; set; }

    double HalfTour1AccessDistance { get; set; }

    int HalfTour1AccessStopAreaKey { get; set; }

    int HalfTour1EgressMode { get; set; }

    int HalfTour1EgressPathType { get; set; }

    double HalfTour1EgressTime { get; set; }

    double HalfTour1EgressCost { get; set; }

    double HalfTour1EgressDistance { get; set; }

    int HalfTour1EgressStopAreaKey { get; set; }

    int HalfTour2AccessMode { get; set; }

    int HalfTour2AccessPathType { get; set; }

    double HalfTour2AccessTime { get; set; }

    double HalfTour2AccessCost { get; set; }

    double HalfTour2AccessDistance { get; set; }

    int HalfTour2AccessStopAreaKey { get; set; }

    int HalfTour2EgressMode { get; set; }

    int HalfTour2EgressPathType { get; set; }

    double HalfTour2EgressTime { get; set; }

    double HalfTour2EgressCost { get; set; }

    double HalfTour2EgressDistance { get; set; }

    int HalfTour2EgressStopAreaKey { get; set; }

    double HalfTour1TravelTime { get; set; }

    double HalfTour2TravelTime { get; set; }

    double TravelCostForPTBikeTour { get; set; }

    double TravelDistanceForPTBikeTour { get; set; }


    #endregion



    bool IsBusinessPurpose();

    bool IsHovDriverMode();

    bool IsHovPassengerMode();
  }
}