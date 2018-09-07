// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.DomainModels.Models {
  public interface IPartialHalfTour : IModel {
    int HouseholdDayId { get; set; }

    int HouseholdId { get; set; }

    int Day { get; set; }

    int Sequence { get; set; }

    int Direction { get; set; }

    int Participants { get; set; }

    int PersonSequence1 { get; set; }

    int TourSequence1 { get; set; }

    int PersonSequence2 { get; set; }

    int TourSequence2 { get; set; }

    int PersonSequence3 { get; set; }

    int TourSequence3 { get; set; }

    int PersonSequence4 { get; set; }

    int TourSequence4 { get; set; }

    int PersonSequence5 { get; set; }

    int TourSequence5 { get; set; }

    int PersonSequence6 { get; set; }

    int TourSequence6 { get; set; }

    int PersonSequence7 { get; set; }

    int TourSequence7 { get; set; }

    int PersonSequence8 { get; set; }

    int TourSequence8 { get; set; }
  }
}