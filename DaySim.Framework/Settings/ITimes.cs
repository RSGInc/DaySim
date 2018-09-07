// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Settings {
  public interface ITimes {
    int MinutesInADay { get; }

    int MinimumActivityDuration { get; }

    int ZeroHours { get; }

    int OneHour { get; }

    int TwoHours { get; }

    int ThreeHours { get; }

    int FourHours { get; }

    int FiveHours { get; }

    int SixHours { get; }

    int SevenHours { get; }

    int EightHours { get; }

    int NineHours { get; }

    int TenHours { get; }

    int ElevenHours { get; }

    int TwelveHours { get; }

    int ThirteenHours { get; }

    int FourteenHours { get; }

    int FifteenHours { get; }

    int SixteenHours { get; }

    int SeventeenHours { get; }

    int EighteenHours { get; }

    int NineteenHours { get; }

    int TwentyHours { get; }

    int TwentyOneHours { get; }

    int TwentyTwoHours { get; }

    int TwentyThreeHours { get; }

    int TwentyFourHours { get; }

    int ThreeAM { get; }

    int FourAM { get; }

    int FiveAM { get; }

    int SixAM { get; }

    int SevenAM { get; }

    int EightAM { get; }

    int NineAM { get; }

    int TenAM { get; }

    int ElevenAM { get; }

    int Noon { get; }

    int OnePM { get; }

    int TwoPM { get; }

    int ThreePM { get; }

    int FourPM { get; }

    int FivePM { get; }

    int SixPM { get; }

    int SevenPM { get; }

    int EightPM { get; }

    int NinePM { get; }

    int TenPM { get; }

    int ElevenPM { get; }

    int Midnight { get; }

    int OneAM { get; }

    int TwoAM { get; }

    int EndOfRelevantWindow { get; }
  }
}
