// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.DomainModels.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DaySim.Framework.Core {
    public static class Extensions {
        private const long ONE_KILOBYTE = 1024L;
        private const long ONE_MEGABYTE = ONE_KILOBYTE * 1024L;
        private const long ONE_GIGABYTE = ONE_MEGABYTE * 1024L;

        public static bool IsBetween(this double value, double start, double end) {
            return value >= start && value <= end;
        }

        public static bool IsBetween(this int value, int start, int end) {
            return value >= start && value <= end;
        }

        public static bool IsLeftExclusiveBetween(this double value, double start, double end) {
            return value > start && value <= end;
        }

        public static bool IsLeftExclusiveBetween(this int value, int start, int end) {
            return value > start && value <= end;
        }

        public static bool IsRightExclusiveBetween(this double value, double start, double end) {
            return value >= start && value < end;
        }

        public static bool IsRightExclusiveBetween(this int value, int start, int end) {
            return value >= start && value < end;
        }

        public static bool AlmostEquals(this double x, double y) {
            //var epsilon = Math.Pow(.1, precision); // 14

            return (Math.Abs(x - y) < Constants.EPSILON);
        }

        public static string Truncate(this string s, int length) {
            return s.Substring(0, Math.Min(length, s.Length));
        }

        public static string Clean(this string s) {
            if (s == null || s == "null") {
                return null;
            }

            return s.ToLower();
        }

        public static int ToMode(this string s) {
            switch (s) {
                case "walk":
                    return Global.Settings.Modes.Walk;

                case "bike":
                    return Global.Settings.Modes.Bike;

                case "sov":
                    return Global.Settings.Modes.Sov;

                case "hov2":
                    return Global.Settings.Modes.Hov2;

                case "hov3":
                    return Global.Settings.Modes.Hov3;

                case "hovDriver":
                    return Global.Settings.Modes.HovDriver;

                case "hovPassenger":
                    return Global.Settings.Modes.HovPassenger;

                case "transit":
                    return Global.Settings.Modes.Transit;

                case "park-and-ride":
                    return Global.Settings.Modes.ParkAndRide;

                case "school-bus":
                    return Global.Settings.Modes.SchoolBus;

                case "kiss-and-ride":
                    return Global.Settings.Modes.KissAndRide;

                case "carParkRideWalk":
                    return Global.Settings.Modes.CarParkRideWalk;

                case "carKissRideWalk":
                    return Global.Settings.Modes.CarKissRideWalk;

                case "bikeParkRideWalk":
                    return Global.Settings.Modes.BikeParkRideWalk;

                case "bikeParkRideBike":
                    return Global.Settings.Modes.BikeParkRideBike;

                case "bikeOnTransit":
                    return Global.Settings.Modes.BikeOnTransit;

                case "carParkRideBike":
                    return Global.Settings.Modes.CarParkRideBike;

                case "walkRideBike":
                    return Global.Settings.Modes.WalkRideBike;

                case "av":
                    return Global.Settings.Modes.AV;
            }

            return Global.Settings.Modes.None;
        }

        public static int ToPathType(this string s) {
            switch (s) {
                case "full-network":
                    return Global.Settings.PathTypes.FullNetwork;
                case "no-tolls":
                case "no-toll-network":
                    return Global.Settings.PathTypes.NoTolls;
                case "local-bus":
                case "transit-type-1":
                case "local-bus-pnr":
                case "transit-type-1-pnr":
                    return Global.Settings.PathTypes.TransitType1;
                case "light-rail":
                case "transit-type-2":
                case "light-rail-pnr":
                case "transit-type-2-pnr":
                    return Global.Settings.PathTypes.TransitType2;
                case "premium-bus":
                case "transit-type-3":
                case "premium-bus-pnr":
                case "transit-type-3-pnr":
                    return Global.Settings.PathTypes.TransitType3;
                case "commuter-rail":
                case "transit-type-4":
                case "commuter-rail-pnr":
                case "transit-type-4-pnr":
                    return Global.Settings.PathTypes.TransitType4;
                case "ferry":
                case "transit-type-5":
                case "ferry-pnr":
                case "transit-type-5-pnr":
                    return Global.Settings.PathTypes.TransitType5;
                case "local-bus-knr":
                case "transit-type-1-knr":
                    return Global.Settings.PathTypes.TransitType1_Knr;
                case "light-rail-knr":
                case "transit-type-2-knr":
                    return Global.Settings.PathTypes.TransitType2_Knr;
                case "premium-bus-knr":
                case "transit-type-3-knr":
                    return Global.Settings.PathTypes.TransitType3_Knr;
                case "commuter-rail-knr":
                case "transit-type-4-knr":
                    return Global.Settings.PathTypes.TransitType4_Knr;
                case "ferry-knr":
                case "transit-type-5-knr":
                    return Global.Settings.PathTypes.TransitType5_Knr;
            }

            return Global.Settings.PathTypes.None;
        }

        public static int ToVotGroup(this string s) {
            switch (s) {
                case "very-low":
                    return Global.Settings.VotGroups.VeryLow;
                case "low":
                    return Global.Settings.VotGroups.Low;
                case "medium":
                    return Global.Settings.VotGroups.Medium;
                case "high":
                    return Global.Settings.VotGroups.High;
                case "very-high":
                    return Global.Settings.VotGroups.VeryHigh;
                case "any":
                case "all":
                case "default":
                    return Global.Settings.VotGroups.Default;
            }

            return Global.Settings.VotGroups.None;
        }

        public static double ToFactor(this string s) {
            if (s == null || s == "null") {
                return 1;
            }

            return double.Parse(s.Trim());
        }

        public static int GetIndex(this int[] a, string s) {
            var hashCode = s.GetHashCode();

            for (var i = 0; i < a.Length; i++) {
                if (a[i] == hashCode) {
                    return i;
                }
            }

            return -1;
        }

        public static int GetIndex(this int[] a, int hashCode) {
            for (var i = 0; i < a.Length; i++) {
                if (a[i] == hashCode) {
                    return i;
                }
            }

            return -1;
        }

        public static int ToMinutesAfter3AM(this int minute) {
            var offset = minute - 179;

            if (offset <= 0) {
                return 1440 + offset;
            }

            return offset;

            //            int minutes;
            //            var hours = Math.DivRem(clockTime24Hour, 100, out minutes);
            //            
            //            if (hours >= 3) {
            //                return 60 * (hours - 3) + minutes; // subtract 3 hours
            //            }
            //
            //            return 60 * (hours - 3 + 24) + minutes; // subtract 3 hours and add 24 hours to make it greater than 0
        }

        public static int ToMinutesAfterMidnight(this int minute) {
            var offset = 179 + minute;

            if (offset >= 1440) {
                return offset - 1440;
            }

            return offset;

            //            int minutes;
            //            var hours = Math.DivRem(minutesAfter3AM, 60, out minutes);
            //            
            //            if (hours < 21) {
            //                return 100 * (hours + 3) + minutes; // add 3 hours
            //            }
            //
            //            return 100 * (hours + 3 - 24) + minutes; // add 3 hours and subtract 24 hours to make it less than 2400
        }

        public static int ToFlag(this bool expression) {
            return expression ? 1 : 0;
        }

        public static string ToSentenceCase(this string s) {
            return Regex.Replace(s, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
        }

        public static string ToFileSize(this long l) {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", l);
        }

        public static FileInfo ToFile(this string s) {
            return string.IsNullOrEmpty(s) ? null : new FileInfo(s);
        }

        public static string ToMD5Checksum(this FileInfo file) {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var md5 = new MD5CryptoServiceProvider();
                var hashCode = md5.ComputeHash(stream);
                var checksum = BitConverter.ToString(hashCode);

                return checksum;
            }
        }

        public static string ToUncPath(this string path, string machineName) {
            return
                Environment.MachineName.Equals(machineName, StringComparison.OrdinalIgnoreCase)
                    ? path
                    : Regex.Replace(path, "^[A-Z]:", string.Format(@"\\{0}", machineName));
        }

        public static List<ITripWrapper> Invert(this List<ITripWrapper> trips) {
            if (trips == null) {
                throw new ArgumentNullException("trips");
            }

            var list = new List<ITripWrapper>();
            var sequence = trips.Count;

            foreach (var trip in trips) {
                trip.Invert(sequence--);

                list.Insert(0, trip);
            }

            return list;
        }
    }
}