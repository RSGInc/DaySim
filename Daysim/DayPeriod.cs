// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.Core;

namespace DaySim {
	public static class DayPeriod {
		public const int EARLY = 0;
		public const int NIGHT = 0;
		public const int AM_PEAK = 1;
		public const int MIDDAY = 2;
		public const int PM_PEAK = 3;
		public const int EVENING = 4;
		public const int LATE = 5;

		public const int SMALL_DAY_PERIOD_TOTAL_TRIP_TIMES = 48;
		public const int SMALL_DAY_PERIOD_TOTAL_TOUR_TIME_COMBINATIONS = 1176;

		public static readonly MinuteSpan[] SmallDayPeriods = new[] {
			new MinuteSpan(0, 1, 30),
			new MinuteSpan(1, 31, 60),
			new MinuteSpan(2, 61, 90),
			new MinuteSpan(3, 91, 120),
			new MinuteSpan(4, 121, 150),
			new MinuteSpan(5, 151, 180),
			new MinuteSpan(6, 181, 210),
			new MinuteSpan(7, 211, 240),
			new MinuteSpan(8, 241, 270),
			new MinuteSpan(9, 271, 300),
			new MinuteSpan(10, 301, 330),
			new MinuteSpan(11, 331, 360),
			new MinuteSpan(12, 361, 390),
			new MinuteSpan(13, 391, 420),
			new MinuteSpan(14, 421, 450),
			new MinuteSpan(15, 451, 480),
			new MinuteSpan(16, 481, 510),
			new MinuteSpan(17, 511, 540),
			new MinuteSpan(18, 541, 570),
			new MinuteSpan(19, 571, 600),
			new MinuteSpan(20, 601, 630),
			new MinuteSpan(21, 631, 660),
			new MinuteSpan(22, 661, 690),
			new MinuteSpan(23, 691, 720),
			new MinuteSpan(24, 721, 750),
			new MinuteSpan(25, 751, 780),
			new MinuteSpan(26, 781, 810),
			new MinuteSpan(27, 811, 840),
			new MinuteSpan(28, 841, 870),
			new MinuteSpan(29, 871, 900),
			new MinuteSpan(30, 901, 930),
			new MinuteSpan(31, 931, 960),
			new MinuteSpan(32, 961, 990),
			new MinuteSpan(33, 991, 1020),
			new MinuteSpan(34, 1021, 1050),
			new MinuteSpan(35, 1051, 1080),
			new MinuteSpan(36, 1081, 1110),
			new MinuteSpan(37, 1111, 1140),
			new MinuteSpan(38, 1141, 1170),
			new MinuteSpan(39, 1171, 1200),
			new MinuteSpan(40, 1201, 1230),
			new MinuteSpan(41, 1231, 1260),
			new MinuteSpan(42, 1261, 1290),
			new MinuteSpan(43, 1291, 1320),
			new MinuteSpan(44, 1321, 1350),
			new MinuteSpan(45, 1351, 1380),
			new MinuteSpan(46, 1381, 1410),
			new MinuteSpan(47, 1411, 1440)
		};

		public const int H_SMALL_DAY_PERIOD_TOTAL_TRIP_TIMES = 144;

		public static readonly MinuteSpan[] HSmallDayPeriods = new[] {
			new MinuteSpan(0, 1, 10),
			new MinuteSpan(1, 11, 20),
			new MinuteSpan(2, 21, 30),
			new MinuteSpan(3, 31, 40),
			new MinuteSpan(4, 41, 50),
			new MinuteSpan(5, 51, 60),
			new MinuteSpan(6, 61, 70),
			new MinuteSpan(7, 71, 80),
			new MinuteSpan(8, 81, 90),
			new MinuteSpan(9, 91, 100),
			new MinuteSpan(10, 101, 110),
			new MinuteSpan(11, 111, 120),
			new MinuteSpan(12, 121, 130),
			new MinuteSpan(13, 131, 140),
			new MinuteSpan(14, 141, 150),
			new MinuteSpan(15, 151, 160),
			new MinuteSpan(16, 161, 170),
			new MinuteSpan(17, 171, 180),
			new MinuteSpan(18, 181, 190),
			new MinuteSpan(19, 191, 200),
			new MinuteSpan(20, 201, 210),
			new MinuteSpan(21, 211, 220),
			new MinuteSpan(22, 221, 230),
			new MinuteSpan(23, 231, 240),
			new MinuteSpan(24, 241, 250),
			new MinuteSpan(25, 251, 260),
			new MinuteSpan(26, 261, 270),
			new MinuteSpan(27, 271, 280),
			new MinuteSpan(28, 281, 290),
			new MinuteSpan(29, 291, 300),
			new MinuteSpan(30, 301, 310),
			new MinuteSpan(31, 311, 320),
			new MinuteSpan(32, 321, 330),
			new MinuteSpan(33, 331, 340),
			new MinuteSpan(34, 341, 350),
			new MinuteSpan(35, 351, 360),
			new MinuteSpan(36, 361, 370),
			new MinuteSpan(37, 371, 380),
			new MinuteSpan(38, 381, 390),
			new MinuteSpan(39, 391, 400),
			new MinuteSpan(40, 401, 410),
			new MinuteSpan(41, 411, 420),
			new MinuteSpan(42, 421, 430),
			new MinuteSpan(43, 431, 440),
			new MinuteSpan(44, 441, 450),
			new MinuteSpan(45, 451, 460),
			new MinuteSpan(46, 461, 470),
			new MinuteSpan(47, 471, 480),
			new MinuteSpan(48, 481, 490),
			new MinuteSpan(49, 491, 500),
			new MinuteSpan(50, 501, 510),
			new MinuteSpan(51, 511, 520),
			new MinuteSpan(52, 521, 530),
			new MinuteSpan(53, 531, 540),
			new MinuteSpan(54, 541, 550),
			new MinuteSpan(55, 551, 560),
			new MinuteSpan(56, 561, 570),
			new MinuteSpan(57, 571, 580),
			new MinuteSpan(58, 581, 590),
			new MinuteSpan(59, 591, 600),
			new MinuteSpan(60, 601, 610),
			new MinuteSpan(61, 611, 620),
			new MinuteSpan(62, 621, 630),
			new MinuteSpan(63, 631, 640),
			new MinuteSpan(64, 641, 650),
			new MinuteSpan(65, 651, 660),
			new MinuteSpan(66, 661, 670),
			new MinuteSpan(67, 671, 680),
			new MinuteSpan(68, 681, 690),
			new MinuteSpan(69, 691, 700),
			new MinuteSpan(70, 701, 710),
			new MinuteSpan(71, 711, 720),
			new MinuteSpan(72, 721, 730),
			new MinuteSpan(73, 731, 740),
			new MinuteSpan(74, 741, 750),
			new MinuteSpan(75, 751, 760),
			new MinuteSpan(76, 761, 770),
			new MinuteSpan(77, 771, 780),
			new MinuteSpan(78, 781, 790),
			new MinuteSpan(79, 791, 800),
			new MinuteSpan(80, 801, 810),
			new MinuteSpan(81, 811, 820),
			new MinuteSpan(82, 821, 830),
			new MinuteSpan(83, 831, 840),
			new MinuteSpan(84, 841, 850),
			new MinuteSpan(85, 851, 860),
			new MinuteSpan(86, 861, 870),
			new MinuteSpan(87, 871, 880),
			new MinuteSpan(88, 881, 890),
			new MinuteSpan(89, 891, 900),
			new MinuteSpan(90, 901, 910),
			new MinuteSpan(91, 911, 920),
			new MinuteSpan(92, 921, 930),
			new MinuteSpan(93, 931, 940),
			new MinuteSpan(94, 941, 950),
			new MinuteSpan(95, 951, 960),
			new MinuteSpan(96, 961, 970),
			new MinuteSpan(97, 971, 980),
			new MinuteSpan(98, 981, 990),
			new MinuteSpan(99, 991, 1000),
			new MinuteSpan(100, 1001, 1010),
			new MinuteSpan(101, 1011, 1020),
			new MinuteSpan(102, 1021, 1030),
			new MinuteSpan(103, 1031, 1040),
			new MinuteSpan(104, 1041, 1050),
			new MinuteSpan(105, 1051, 1060),
			new MinuteSpan(106, 1061, 1070),
			new MinuteSpan(107, 1071, 1080),
			new MinuteSpan(108, 1081, 1090),
			new MinuteSpan(109, 1091, 1100),
			new MinuteSpan(110, 1101, 1110),
			new MinuteSpan(111, 1111, 1120),
			new MinuteSpan(112, 1121, 1130),
			new MinuteSpan(113, 1131, 1140),
			new MinuteSpan(114, 1141, 1150),
			new MinuteSpan(115, 1151, 1160),
			new MinuteSpan(116, 1161, 1170),
			new MinuteSpan(117, 1171, 1180),
			new MinuteSpan(118, 1181, 1190),
			new MinuteSpan(119, 1191, 1200),
			new MinuteSpan(120, 1201, 1210),
			new MinuteSpan(121, 1211, 1220),
			new MinuteSpan(122, 1221, 1230),
			new MinuteSpan(123, 1231, 1240),
			new MinuteSpan(124, 1241, 1250),
			new MinuteSpan(125, 1251, 1260),
			new MinuteSpan(126, 1261, 1270),
			new MinuteSpan(127, 1271, 1280),
			new MinuteSpan(128, 1281, 1290),
			new MinuteSpan(129, 1291, 1300),
			new MinuteSpan(130, 1301, 1310),
			new MinuteSpan(131, 1311, 1320),
			new MinuteSpan(132, 1321, 1330),
			new MinuteSpan(133, 1331, 1340),
			new MinuteSpan(134, 1341, 1350),
			new MinuteSpan(135, 1351, 1360),
			new MinuteSpan(136, 1361, 1370),
			new MinuteSpan(137, 1371, 1380),
			new MinuteSpan(138, 1381, 1390),
			new MinuteSpan(139, 1391, 1400),
			new MinuteSpan(140, 1401, 1410),
			new MinuteSpan(141, 1411, 1420),
			new MinuteSpan(142, 1421, 1430),
			new MinuteSpan(143, 1431, 1440)
		};

		public static readonly MinuteSpan[] BigDayPeriods = new[] {
			// 11:00 PM - 5:59 AM
			new MinuteSpan(NIGHT, 1201, 180),
			// 6:00 AM - 8:59 AM
			new MinuteSpan(AM_PEAK, 181, 360),
			// 9:00 AM - 3:29 PM
			new MinuteSpan(MIDDAY, 361, 750),
			// 3:30 PM - 6:29 PM
			new MinuteSpan(PM_PEAK, 751, 930),
			// 6:30 PM - 10:59 PM
			new MinuteSpan(EVENING, 931, 1200)
		};

		public const int H_BIG_DAY_PERIOD_TOTAL_TOUR_TIMES = 6;
		public const int H_BIG_DAY_PERIOD_TOTAL_TOUR_TIME_COMBINATIONS = 21;

		public static readonly MinuteSpan[] HBigDayPeriods = new[] {
			// 3:00 AM - 5:59 AM
			new MinuteSpan(EARLY, 1, 180),
			// 6:00 AM - 8:59 AM
			new MinuteSpan(AM_PEAK, 181, 360),
			// 9:00 AM - 3:29 PM
			new MinuteSpan(MIDDAY, 361, 750),
			// 3:30 PM - 6:29 PM
			new MinuteSpan(PM_PEAK, 751, 930),
			// 6:30 PM - 10:59 PM
			new MinuteSpan(EVENING, 931, 1200),
			// 11:00 PM - 2:59 AM
			new MinuteSpan(LATE, 1201, 1440),
		};
}
}