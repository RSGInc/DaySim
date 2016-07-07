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
		
		int NewMode { get; }
		
		int Brt { get; }
		
		int FixedGuideway { get; }
		
		int BrtC2 { get; }
		
		int FixedGuidewayC2 { get; }
		
		int LocalBusPnr { get; }
		
		int NewModePnr { get; }
		
		int BrtPnr { get; }
		
		int FixedGuidewayPnr { get; }
		
		int BrtC2Pnr { get; }
		
		int FixedGuidewayC2Pnr { get; }
		
		int LocalBusKnr { get; }
		
		int NewModeKnr { get; }
		
		int BrtKnr { get; }
		
		int FixedGuidewayKnr { get; }
		
		int BrtC2Knr { get; }
		
		int FixedGuidewayC2Knr { get; }
	}
}
