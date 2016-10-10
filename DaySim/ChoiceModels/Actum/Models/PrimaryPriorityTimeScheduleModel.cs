// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Actum;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.ChoiceModels.Actum.Models {
	public class PrimaryPriorityTimeScheduleModel : ChoiceModel {
		private const string CHOICE_MODEL_NAME = "ActumPrimaryPriorityTimeScheduleModel";
		private const int TOTAL_ALTERNATIVES = 206;
		private const int TOTAL_NESTED_ALTERNATIVES = 0;
		private const int TOTAL_LEVELS = 1;
		private const int MAX_PARAMETER = 99;
		private static readonly int[][] _pfptSchedule = new int[TOTAL_ALTERNATIVES + 1][];
			

		public override void RunInitialize(ICoefficientsReader reader = null) 
		{
			Initialize(CHOICE_MODEL_NAME, Global.Configuration.ActumPrimaryPriorityTimeScheduleModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
			// [kids][StartingMinuteSharedHomeStay][DurationMinutesSharedHomeStay]
			_pfptSchedule[1] = new int[] { 0, 912, 360 };
			_pfptSchedule[2] = new int[] { 0, 1082, 120 };
			_pfptSchedule[3] = new int[] { 0, 950, 400 };
			_pfptSchedule[4] = new int[] { 0, 550, 120 };
			_pfptSchedule[5] = new int[] { 0, 1145, 110 };
			_pfptSchedule[6] = new int[] { 0, 830, 30 };
			_pfptSchedule[7] = new int[] { 0, 565, 150 };
			_pfptSchedule[8] = new int[] { 0, 1050, 300 };
			_pfptSchedule[9] = new int[] { 0, 773, 420 };
			_pfptSchedule[10] = new int[] { 0, 1190, 60 };
			_pfptSchedule[11] = new int[] { 0, 1145, 90 };
			_pfptSchedule[12] = new int[] { 0, 940, 360 };
			_pfptSchedule[13] = new int[] { 0, 435, 525 };
			_pfptSchedule[14] = new int[] { 0, 955, 90 };
			_pfptSchedule[15] = new int[] { 0, 1035, 189 };
			_pfptSchedule[16] = new int[] { 0, 1120, 120 };
			_pfptSchedule[17] = new int[] { 0, 1095, 330 };
			_pfptSchedule[18] = new int[] { 0, 643, 217 };
			_pfptSchedule[19] = new int[] { 0, 1009, 60 };
			_pfptSchedule[20] = new int[] { 0, 1018, 180 };
			_pfptSchedule[21] = new int[] { 0, 989, 101 };
			_pfptSchedule[22] = new int[] { 0, 1180, 35 };
			_pfptSchedule[23] = new int[] { 0, 685, 60 };
			_pfptSchedule[24] = new int[] { 0, 610, 240 };
			_pfptSchedule[25] = new int[] { 0, 1025, 400 };
			_pfptSchedule[26] = new int[] { 0, 970, 360 };
			_pfptSchedule[27] = new int[] { 0, 1110, 135 };
			_pfptSchedule[28] = new int[] { 0, 1166, 210 };
			_pfptSchedule[29] = new int[] { 0, 915, 285 };
			_pfptSchedule[30] = new int[] { 0, 925, 180 };
			_pfptSchedule[31] = new int[] { 0, 905, 185 };
			_pfptSchedule[32] = new int[] { 0, 1010, 60 };
			_pfptSchedule[33] = new int[] { 0, 970, 60 };
			_pfptSchedule[34] = new int[] { 0, 1131, 120 };
			_pfptSchedule[35] = new int[] { 0, 1120, 80 };
			_pfptSchedule[36] = new int[] { 0, 971, 174 };
			_pfptSchedule[37] = new int[] { 0, 1035, 165 };
			_pfptSchedule[38] = new int[] { 0, 710, 90 };
			_pfptSchedule[39] = new int[] { 0, 745, 60 };
			_pfptSchedule[40] = new int[] { 0, 970, 470 };
			_pfptSchedule[41] = new int[] { 0, 1025, 240 };
			_pfptSchedule[42] = new int[] { 0, 1060, 330 };
			_pfptSchedule[43] = new int[] { 0, 1005, 30 };
			_pfptSchedule[44] = new int[] { 0, 1050, 180 };
			_pfptSchedule[45] = new int[] { 0, 1015, 240 };
			_pfptSchedule[46] = new int[] { 0, 935, 130 };
			_pfptSchedule[47] = new int[] { 0, 1174, 50 };
			_pfptSchedule[48] = new int[] { 0, 1010, 120 };
			_pfptSchedule[49] = new int[] { 0, 1020, 60 };
			_pfptSchedule[50] = new int[] { 0, 905, 60 };
			_pfptSchedule[51] = new int[] { 0, 960, 120 };
			_pfptSchedule[52] = new int[] { 0, 805, 95 };
			_pfptSchedule[53] = new int[] { 0, 1090, 190 };
			_pfptSchedule[54] = new int[] { 0, 510, 360 };
			_pfptSchedule[55] = new int[] { 0, 975, 180 };
			_pfptSchedule[56] = new int[] { 0, 495, 300 };
			_pfptSchedule[57] = new int[] { 0, 1015, 120 };
			_pfptSchedule[58] = new int[] { 0, 915, 90 };
			_pfptSchedule[59] = new int[] { 0, 965, 250 };
			_pfptSchedule[60] = new int[] { 0, 1025, 60 };
			_pfptSchedule[61] = new int[] { 0, 1115, 85 };
			_pfptSchedule[62] = new int[] { 0, 878, 290 };
			_pfptSchedule[63] = new int[] { 0, 1014, 180 };
			_pfptSchedule[64] = new int[] { 0, 1175, 40 };
			_pfptSchedule[65] = new int[] { 0, 960, 180 };
			_pfptSchedule[66] = new int[] { 0, 1005, 90 };
			_pfptSchedule[67] = new int[] { 0, 1035, 165 };
			_pfptSchedule[68] = new int[] { 0, 992, 330 };
			_pfptSchedule[69] = new int[] { 0, 983, 240 };
			_pfptSchedule[70] = new int[] { 0, 1013, 240 };
			_pfptSchedule[71] = new int[] { 0, 783, 420 };
			_pfptSchedule[72] = new int[] { 1, 1078, 120 };
			_pfptSchedule[73] = new int[] { 1, 1060, 290 };
			_pfptSchedule[74] = new int[] { 1, 965, 40 };
			_pfptSchedule[75] = new int[] { 1, 1175, 90 };
			_pfptSchedule[76] = new int[] { 1, 1112, 120 };
			_pfptSchedule[77] = new int[] { 1, 984, 180 };
			_pfptSchedule[78] = new int[] { 1, 1010, 300 };
			_pfptSchedule[79] = new int[] { 1, 1090, 180 };
			_pfptSchedule[80] = new int[] { 1, 1120, 80 };
			_pfptSchedule[81] = new int[] { 1, 937, 240 };
			_pfptSchedule[82] = new int[] { 1, 633, 195 };
			_pfptSchedule[83] = new int[] { 1, 970, 120 };
			_pfptSchedule[84] = new int[] { 1, 1182, 20 };
			_pfptSchedule[85] = new int[] { 1, 990, 270 };
			_pfptSchedule[86] = new int[] { 1, 1177, 120 };
			_pfptSchedule[87] = new int[] { 1, 1018, 180 };
			_pfptSchedule[88] = new int[] { 1, 1231, 44 };
			_pfptSchedule[89] = new int[] { 1, 1055, 180 };
			_pfptSchedule[90] = new int[] { 1, 1058, 120 };
			_pfptSchedule[91] = new int[] { 1, 1052, 88 };
			_pfptSchedule[92] = new int[] { 1, 1075, 300 };
			_pfptSchedule[93] = new int[] { 1, 1090, 50 };
			_pfptSchedule[94] = new int[] { 1, 990, 90 };
			_pfptSchedule[95] = new int[] { 1, 1193, 150 };
			_pfptSchedule[96] = new int[] { 1, 1120, 240 };
			_pfptSchedule[97] = new int[] { 1, 1038, 300 };
			_pfptSchedule[98] = new int[] { 1, 1020, 360 };
			_pfptSchedule[99] = new int[] { 1, 880, 165 };
			_pfptSchedule[100] = new int[] { 1, 1125, 45 };
			_pfptSchedule[101] = new int[] { 1, 960, 160 };
			_pfptSchedule[102] = new int[] { 1, 939, 265 };
			_pfptSchedule[103] = new int[] { 1, 1170, 90 };
			_pfptSchedule[104] = new int[] { 1, 1072, 240 };
			_pfptSchedule[105] = new int[] { 1, 955, 480 };
			_pfptSchedule[106] = new int[] { 1, 1080, 120 };
			_pfptSchedule[107] = new int[] { 1, 535, 180 };
			_pfptSchedule[108] = new int[] { 1, 1118, 82 };
			_pfptSchedule[109] = new int[] { 1, 867, 118 };
			_pfptSchedule[110] = new int[] { 1, 876, 324 };
			_pfptSchedule[111] = new int[] { 1, 1088, 110 };
			_pfptSchedule[112] = new int[] { 1, 1160, 40 };
			_pfptSchedule[113] = new int[] { 1, 1070, 300 };
			_pfptSchedule[114] = new int[] { 1, 965, 210 };
			_pfptSchedule[115] = new int[] { 1, 1032, 60 };
			_pfptSchedule[116] = new int[] { 1, 930, 270 };
			_pfptSchedule[117] = new int[] { 1, 1110, 180 };
			_pfptSchedule[118] = new int[] { 1, 1105, 30 };
			_pfptSchedule[119] = new int[] { 1, 1011, 179 };
			_pfptSchedule[120] = new int[] { 1, 991, 181 };
			_pfptSchedule[121] = new int[] { 1, 977, 240 };
			_pfptSchedule[122] = new int[] { 1, 1185, 60 };
			_pfptSchedule[123] = new int[] { 1, 773, 480 };
			_pfptSchedule[124] = new int[] { 1, 1100, 60 };
			_pfptSchedule[125] = new int[] { 1, 1050, 90 };
			_pfptSchedule[126] = new int[] { 1, 945, 240 };
			_pfptSchedule[127] = new int[] { 1, 1030, 60 };
			_pfptSchedule[128] = new int[] { 1, 995, 400 };
			_pfptSchedule[129] = new int[] { 1, 1028, 150 };
			_pfptSchedule[130] = new int[] { 1, 1060, 60 };
			_pfptSchedule[131] = new int[] { 1, 1025, 180 };
			_pfptSchedule[132] = new int[] { 1, 1174, 120 };
			_pfptSchedule[133] = new int[] { 1, 955, 168 };
			_pfptSchedule[134] = new int[] { 1, 1035, 60 };
			_pfptSchedule[135] = new int[] { 1, 1018, 107 };
			_pfptSchedule[136] = new int[] { 1, 1026, 174 };
			_pfptSchedule[137] = new int[] { 1, 1075, 125 };
			_pfptSchedule[138] = new int[] { 1, 875, 205 };
			_pfptSchedule[139] = new int[] { 1, 1155, 165 };
			_pfptSchedule[140] = new int[] { 1, 1000, 210 };
			_pfptSchedule[141] = new int[] { 1, 1129, 70 };
			_pfptSchedule[142] = new int[] { 1, 1083, 120 };
			_pfptSchedule[143] = new int[] { 1, 1025, 300 };
			_pfptSchedule[144] = new int[] { 1, 954, 210 };
			_pfptSchedule[145] = new int[] { 1, 960, 300 };
			_pfptSchedule[146] = new int[] { 1, 1140, 60 };
			_pfptSchedule[147] = new int[] { 1, 962, 300 };
			_pfptSchedule[148] = new int[] { 1, 840, 360 };
			_pfptSchedule[149] = new int[] { 1, 972, 100 };
			_pfptSchedule[150] = new int[] { 1, 1070, 120 };
			_pfptSchedule[151] = new int[] { 1, 965, 360 };
			_pfptSchedule[152] = new int[] { 1, 942, 180 };
			_pfptSchedule[153] = new int[] { 1, 1192, 60 };
			_pfptSchedule[154] = new int[] { 1, 1061, 90 };
			_pfptSchedule[155] = new int[] { 1, 914, 240 };
			_pfptSchedule[156] = new int[] { 1, 1055, 290 };
			_pfptSchedule[157] = new int[] { 1, 1060, 260 };
			_pfptSchedule[158] = new int[] { 1, 1035, 60 };
			_pfptSchedule[159] = new int[] { 1, 970, 240 };
			_pfptSchedule[160] = new int[] { 1, 1052, 340 };
			_pfptSchedule[161] = new int[] { 1, 943, 120 };
			_pfptSchedule[162] = new int[] { 1, 917, 300 };
			_pfptSchedule[163] = new int[] { 1, 1045, 360 };
			_pfptSchedule[164] = new int[] { 1, 970, 230 };
			_pfptSchedule[165] = new int[] { 1, 1141, 60 };
			_pfptSchedule[166] = new int[] { 1, 860, 260 };
			_pfptSchedule[167] = new int[] { 1, 1045, 395 };
			_pfptSchedule[168] = new int[] { 1, 230, 480 };
			_pfptSchedule[169] = new int[] { 1, 1060, 60 };
			_pfptSchedule[170] = new int[] { 1, 1047, 153 };
			_pfptSchedule[171] = new int[] { 1, 930, 420 };
			_pfptSchedule[172] = new int[] { 1, 1070, 60 };
			_pfptSchedule[173] = new int[] { 1, 980, 47 };
			_pfptSchedule[174] = new int[] { 1, 1005, 75 };
			_pfptSchedule[175] = new int[] { 1, 550, 330 };
			_pfptSchedule[176] = new int[] { 1, 1008, 192 };
			_pfptSchedule[177] = new int[] { 1, 975, 240 };
			_pfptSchedule[178] = new int[] { 1, 960, 215 };
			_pfptSchedule[179] = new int[] { 1, 980, 300 };
			_pfptSchedule[180] = new int[] { 1, 795, 290 };
			_pfptSchedule[181] = new int[] { 1, 890, 330 };
			_pfptSchedule[182] = new int[] { 1, 1140, 180 };
			_pfptSchedule[183] = new int[] { 1, 855, 255 };
			_pfptSchedule[184] = new int[] { 1, 1165, 200 };
			_pfptSchedule[185] = new int[] { 1, 980, 240 };
			_pfptSchedule[186] = new int[] { 1, 1020, 400 };
			_pfptSchedule[187] = new int[] { 1, 967, 380 };
			_pfptSchedule[188] = new int[] { 1, 970, 65 };
			_pfptSchedule[189] = new int[] { 1, 914, 480 };
			_pfptSchedule[190] = new int[] { 1, 843, 240 };
			_pfptSchedule[191] = new int[] { 1, 1020, 210 };
			_pfptSchedule[192] = new int[] { 1, 935, 265 };
			_pfptSchedule[193] = new int[] { 1, 1120, 80 };
			_pfptSchedule[194] = new int[] { 1, 925, 180 };
			_pfptSchedule[195] = new int[] { 1, 1085, 120 };
			_pfptSchedule[196] = new int[] { 1, 925, 390 };
			_pfptSchedule[197] = new int[] { 1, 1010, 360 };
			_pfptSchedule[198] = new int[] { 1, 1135, 60 };
			_pfptSchedule[199] = new int[] { 1, 795, 540 };
			_pfptSchedule[200] = new int[] { 1, 1046, 79 };
			_pfptSchedule[201] = new int[] { 1, 1065, 300 };
			_pfptSchedule[202] = new int[] { 1, 1163, 44 };
			_pfptSchedule[203] = new int[] { 1, 997, 260 };
			_pfptSchedule[204] = new int[] { 1, 990, 360 };
			_pfptSchedule[205] = new int[] { 1, 982, 210 };
			_pfptSchedule[206] = new int[] { 1, 990, 180 };
		}
		
		public void Run(HouseholdDayWrapper householdDay) {
			if (householdDay == null) {
				throw new ArgumentNullException("householdDay");
			}
			
			householdDay.ResetRandom(962);

			if (Global.Configuration.IsInEstimationMode) {
				return;
			}

			var choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(householdDay.Household.Id);

			

			//if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {

			//				// set choice variable here  (derive from available household properties)
			//				if (householdDay.SharedActivityHomeStays >= 1 
			//					//&& householdDay.DurationMinutesSharedHomeStay >=60 
			//					&& householdDay.AdultsInSharedHomeStay >= 1 
			//					&& householdDay.NumberInLargestSharedHomeStay >= (householdDay.Household.Size)
			//                   )
			//				{
			//					householdDay.PrimaryPriorityTimeFlag = 1;
			//				}
			//				else 	householdDay.PrimaryPriorityTimeFlag = 0;

			//RunModel(choiceProbabilityCalculator, householdDay, householdDay.PrimaryPriorityTimeFlag);

			//choiceProbabilityCalculator.WriteObservation();
			//}
			//else {
			RunModel(choiceProbabilityCalculator, householdDay, _pfptSchedule);

			var chosenAlternative = choiceProbabilityCalculator.SimulateChoice(householdDay.Household.RandomUtility);
			var choice = (int[]) chosenAlternative.Choice;
			householdDay.StartingMinuteSharedHomeStay = choice[1];
			householdDay.DurationMinutesSharedHomeStay = choice[2];
			//}
		}

		private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, HouseholdDayWrapper householdDay, int[][] pfptSchedule, int[] choice = null) {

			//var householdDay = (ActumHouseholdDayWrapper)tour.HouseholdDay;
			var household = householdDay.Household;

			//Generate utility funtions for the alternatives
			bool[] available = new bool[TOTAL_ALTERNATIVES + 1];
			bool[] chosen = new bool[TOTAL_ALTERNATIVES + 1];
			for (int alt = 1; alt <= TOTAL_ALTERNATIVES; alt++) {

				available[alt] = false;
				chosen[alt] = false;
				// set availability based on household CHILDREN
				if ((household.HasChildren && pfptSchedule[alt][0] == 1) || (!household.HasChildren && pfptSchedule[alt][0] == 0)) {
					available[alt] = true;
				}

				var alternative = choiceProbabilityCalculator.GetAlternative(alt - 1, available[alt], chosen[alt]);
				alternative.Choice = pfptSchedule[alt];

				// add utility terms for this alterative
				alternative.AddUtilityTerm(1, 1);   // asc  
			}
		}
	}
}