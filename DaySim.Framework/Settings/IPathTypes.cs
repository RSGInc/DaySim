// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Settings {
  public interface IPathTypes {
    int TotalPathTypes { get; }

    int None { get; }

    int FullNetwork { get; }

    int NoTolls { get; }

    int LocalBus { get; }

    int LightRail { get; }

    int PremiumBus { get; }

    int CommuterRail { get; }

    int Ferry { get; }

    int TransitType1 { get; }

    int TransitType2 { get; }

    int TransitType3 { get; }

    int TransitType4 { get; }

    int TransitType5 { get; }

    int LocalBus_Knr { get; }

    int LightRail_Knr { get; }

    int PremiumBus_Knr { get; }

    int CommuterRail_Knr { get; }

    int Ferry_Knr { get; }

    int TransitType1_Knr { get; }

    int TransitType2_Knr { get; }

    int TransitType3_Knr { get; }

    int TransitType4_Knr { get; }

    int TransitType5_Knr { get; }

    int LocalBus_TNC { get; }

    int LightRail_TNC { get; }

    int PremiumBus_TNC { get; }

    int CommuterRail_TNC { get; }

    int Ferry_TNC { get; }

    int TransitType1_TNC { get; }

    int TransitType2_TNC { get; }

    int TransitType3_TNC { get; }

    int TransitType4_TNC { get; }

    int TransitType5_TNC { get; }

  }
}
