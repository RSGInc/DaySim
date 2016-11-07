// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;
using System;

namespace DaySim.ChoiceModels {
    public static class ChoiceModelUtility {
        public const double CPFACT2 = 2;
        public const double CPFACT3 = 3.33;

        private static readonly double[,,] _timeOfDayFraction = new[, ,] {
            
            //  {AM-AM,  AM-MD,  AM-PM,  AM-NT,  MD-AM,  MD-MD,  MD-PM,  MD-NT,  PM-AM,  PM-MD,  PM-PM,  PM-NT,  NT-AM, NT-MD,  NT-PM,   NT-NT }
            
            // NONE_OR_HOME
            {
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // WORK
            {
                {0.0080, 0.1612, 0.5652, 0.0624, 0.0000, 0.0348, 0.0650, 0.0501, 0.0000, 0.0000, 0.0036, 0.0134, 0.0022, 0.0203, 0.0094, 0.0044},
                {0.0614, 0.2780, 0.0000, 0.0000, 0.0000, 0.1552, 0.2744, 0.0144, 0.0000, 0.0000, 0.0722, 0.0722, 0.0108, 0.0000, 0.0000, 0.0614},
                {0.0568, 0.1875, 0.0284, 0.0000, 0.0000, 0.5284, 0.1080, 0.0057, 0.0000, 0.0000, 0.0682, 0.0057, 0.0000, 0.0000, 0.0000, 0.0114},
                {0.0100, 0.2880, 0.2100, 0.0200, 0.0000, 0.1240, 0.1460, 0.0700, 0.0000, 0.0000, 0.0100, 0.1000, 0.0000, 0.0060, 0.0000, 0.0160}
            },
            // SCHOOL
            {
                {0.0008, 0.7006, 0.2414, 0.0050, 0.0000, 0.0378, 0.0126, 0.0000, 0.0000, 0.0000, 0.0000, 0.0017, 0.0000, 0.0000, 0.0000, 0.0000},
                {0.0470, 0.2685, 0.0134, 0.0000, 0.0000, 0.1477, 0.0604, 0.0067, 0.0000, 0.0000, 0.0940, 0.2215, 0.0000, 0.0000, 0.0000, 0.1409},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.8000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.2000, 0.0000, 0.0000, 0.0000, 0.0000},
                {0.0089, 0.2902, 0.1696, 0.0491, 0.0000, 0.2321, 0.1384, 0.0313, 0.0000, 0.0000, 0.0223, 0.0491, 0.0000, 0.0000, 0.0000, 0.0089}
            },
            // ESCORT
            {
                {0.3155, 0.0278, 0.0046, 0.0000, 0.0000, 0.3619, 0.0348, 0.0000, 0.0000, 0.0000, 0.1717, 0.0116, 0.0070, 0.0000, 0.0000, 0.0650},
                {0.3700, 0.0057, 0.0000, 0.0000, 0.0000, 0.2258, 0.0190, 0.0019, 0.0000, 0.0000, 0.1708, 0.0095, 0.0000, 0.0000, 0.0000, 0.1973},
                {0.0556, 0.0000, 0.0000, 0.0000, 0.0000, 0.7222, 0.0000, 0.0000, 0.0000, 0.0000, 0.2222, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // PERSONAL_BUSINESS
            {
                {0.0698, 0.1356, 0.0239, 0.0020, 0.0000, 0.4497, 0.1007, 0.0110, 0.0000, 0.0000, 0.0728, 0.0578, 0.0000, 0.0000, 0.0000, 0.0768},
                {0.0850, 0.0547, 0.0012, 0.0000, 0.0000, 0.3155, 0.0547, 0.0012, 0.0000, 0.0000, 0.1595, 0.1141, 0.0012, 0.0000, 0.0000, 0.2130},
                {0.1190, 0.0556, 0.0000, 0.0000, 0.0000, 0.6429, 0.0556, 0.0000, 0.0000, 0.0000, 0.0714, 0.0476, 0.0000, 0.0000, 0.0000, 0.0079},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // SHOPPING
            {
                {0.0371, 0.0416, 0.0015, 0.0000, 0.0000, 0.6597, 0.0877, 0.0015, 0.0000, 0.0000, 0.1159, 0.0223, 0.0000, 0.0000, 0.0000, 0.0327},
                {0.0569, 0.0201, 0.0000, 0.0000, 0.0000, 0.3337, 0.0614, 0.0000, 0.0000, 0.0000, 0.2455, 0.0413, 0.0000, 0.0000, 0.0000, 0.2411},
                {0.0417, 0.0000, 0.0000, 0.0000, 0.0000, 0.8333, 0.0208, 0.0000, 0.0000, 0.0000, 0.0625, 0.0000, 0.0000, 0.0000, 0.0000, 0.0417},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // MEAL
            {
                {0.0093, 0.0841, 0.0093, 0.0000, 0.0000, 0.3925, 0.0374, 0.0280, 0.0000, 0.0000, 0.0935, 0.1869, 0.0000, 0.0000, 0.0000, 0.1589},
                {0.0371, 0.0257, 0.0000, 0.0000, 0.0000, 0.1943, 0.0200, 0.0000, 0.0000, 0.0000, 0.0857, 0.1800, 0.0029, 0.0000, 0.0000, 0.4543},
                {0.0124, 0.0124, 0.0000, 0.0000, 0.0000, 0.9378, 0.0083, 0.0000, 0.0000, 0.0000, 0.0124, 0.0041, 0.0000, 0.0000, 0.0000, 0.0124},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // SOCIAL
            {
                {0.0171, 0.1672, 0.0580, 0.0102, 0.0000, 0.2662, 0.1980, 0.0205, 0.0000, 0.0000, 0.0273, 0.1092, 0.0000, 0.0000, 0.0000, 0.1263},
                {0.0499, 0.0435, 0.0042, 0.0000, 0.0000, 0.1136, 0.0722, 0.0117, 0.0000, 0.0000, 0.1210, 0.2070, 0.0106, 0.0000, 0.0000, 0.3662},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.6667, 0.0303, 0.0000, 0.0000, 0.0000, 0.0909, 0.0909, 0.0000, 0.0000, 0.0000, 0.1212},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // RECREATION (USE SOCIAL VALUES)
            {
                {0.0171, 0.1672, 0.0580, 0.0102, 0.0000, 0.2662, 0.1980, 0.0205, 0.0000, 0.0000, 0.0273, 0.1092, 0.0000, 0.0000, 0.0000, 0.1263},
                {0.0499, 0.0435, 0.0042, 0.0000, 0.0000, 0.1136, 0.0722, 0.0117, 0.0000, 0.0000, 0.1210, 0.2070, 0.0106, 0.0000, 0.0000, 0.3662},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.6667, 0.0303, 0.0000, 0.0000, 0.0000, 0.0909, 0.0909, 0.0000, 0.0000, 0.0000, 0.1212},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // MEDICAL (USE PERSONAL_BUSINESS VALUES)
            {
                {0.0698, 0.1356, 0.0239, 0.0020, 0.0000, 0.4497, 0.1007, 0.0110, 0.0000, 0.0000, 0.0728, 0.0578, 0.0000, 0.0000, 0.0000, 0.0768},
                {0.0850, 0.0547, 0.0012, 0.0000, 0.0000, 0.3155, 0.0547, 0.0012, 0.0000, 0.0000, 0.1595, 0.1141, 0.0012, 0.0000, 0.0000, 0.2130},
                {0.1190, 0.0556, 0.0000, 0.0000, 0.0000, 0.6429, 0.0556, 0.0000, 0.0000, 0.0000, 0.0714, 0.0476, 0.0000, 0.0000, 0.0000, 0.0079},
                {0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000}
            },
            // ppurpose 10 (placeholder, has work values for now)
            {
                {0.0080, 0.1612, 0.5652, 0.0624, 0.0000, 0.0348, 0.0650, 0.0501, 0.0000, 0.0000, 0.0036, 0.0134, 0.0022, 0.0203, 0.0094, 0.0044},
                {0.0614, 0.2780, 0.0000, 0.0000, 0.0000, 0.1552, 0.2744, 0.0144, 0.0000, 0.0000, 0.0722, 0.0722, 0.0108, 0.0000, 0.0000, 0.0614},
                {0.0568, 0.1875, 0.0284, 0.0000, 0.0000, 0.5284, 0.1080, 0.0057, 0.0000, 0.0000, 0.0682, 0.0057, 0.0000, 0.0000, 0.0000, 0.0114},
                {0.0100, 0.2880, 0.2100, 0.0200, 0.0000, 0.1240, 0.1460, 0.0700, 0.0000, 0.0000, 0.0100, 0.1000, 0.0000, 0.0060, 0.0000, 0.0160}
            },
            // BUSINESS (placeholder, has work values for now)
            {
                {0.0080, 0.1612, 0.5652, 0.0624, 0.0000, 0.0348, 0.0650, 0.0501, 0.0000, 0.0000, 0.0036, 0.0134, 0.0022, 0.0203, 0.0094, 0.0044},
                {0.0614, 0.2780, 0.0000, 0.0000, 0.0000, 0.1552, 0.2744, 0.0144, 0.0000, 0.0000, 0.0722, 0.0722, 0.0108, 0.0000, 0.0000, 0.0614},
                {0.0568, 0.1875, 0.0284, 0.0000, 0.0000, 0.5284, 0.1080, 0.0057, 0.0000, 0.0000, 0.0682, 0.0057, 0.0000, 0.0000, 0.0000, 0.0114},
                {0.0100, 0.2880, 0.2100, 0.0200, 0.0000, 0.1240, 0.1460, 0.0700, 0.0000, 0.0000, 0.0100, 0.1000, 0.0000, 0.0060, 0.0000, 0.0160}
            }
        };

        //  {AM-AM,  AM-MD,  AM-PM,  AM-NT,  MD-AM,  MD-MD,  MD-PM,  MD-NT,  PM-AM,  PM-MD,  PM-PM,  PM-NT,  NT-AM, NT-MD,  NT-PM,   NT-NT }

        private static readonly int[] _arrivalTimeMinute = new[]
            {300,    300,    300,    300,    0,      540,    540,    540,    0,      0,      840,    840,    120,   120,    120,     1020};

        private static readonly int[] _departureTimeMinute = new[]
            {360,    600,    840,    1020,   0,      600,    840,    1020,   0,      0,      900,    1020,   360,   600,    840,     1140};

        public static void SetEscortPercentages(IPersonDayWrapper personDay, out double escortPercentage, out double nonEscortPercentage, bool excludeWorkAndSchool = false) {
            if (personDay == null || personDay.HomeBasedTours == 0) {
                escortPercentage = 0;
                nonEscortPercentage = 0;
            } else {
                var totalTours = excludeWorkAndSchool ? personDay.GetTotalToursExcludingWorkAndSchool() : personDay.GetTotalTours();
                var totalStops = excludeWorkAndSchool ? personDay.GetTotalStopsExcludingWorkAndSchool() : personDay.GetTotalStops();
                var escortStopFlag = (personDay.EscortStops > 0).ToFlag();

                escortPercentage = escortStopFlag / Math.Max(totalTours, 1.0);
                nonEscortPercentage = (totalStops - escortStopFlag) / Math.Max(totalTours, 1.0);
            }
        }

        public static void DrawRandomTourTimePeriods(ITourWrapper tour, int tourCategory) {
            if (tour == null) {
                throw new ArgumentNullException("tour");
            }

            int startAndEndPeriod = 0;
            var random = tour.Household.RandomUtility.Uniform01();
            double cumulativePercent = 0;

            while (random > cumulativePercent & startAndEndPeriod < _arrivalTimeMinute.Length) {
                cumulativePercent = cumulativePercent + _timeOfDayFraction[tour.DestinationPurpose, tourCategory, startAndEndPeriod];
                if (random < cumulativePercent) { break; }
                startAndEndPeriod = startAndEndPeriod + 1;
            }
            if (random > cumulativePercent) {
                startAndEndPeriod = startAndEndPeriod - 1; //in case fractions do not quite sum to 1 and random is greater
            }

            tour.DestinationArrivalTime = _arrivalTimeMinute[startAndEndPeriod];
            tour.DestinationDepartureTime = _departureTimeMinute[startAndEndPeriod];

        }

        public static void WriteTripForTDM(ITripWrapper trip, TDMTripListExporter tdmTripListExporter) {
            if (tdmTripListExporter == null) {
                return;
            }

            tdmTripListExporter.Export(trip);
        }

        public static int GetDestinationArrivalTime(int model) {
            if (model == Global.Settings.Models.WorkTourModeModel) {
                return DayPeriod.BigDayPeriods[DayPeriod.AM_PEAK].Middle;
            } else if (model == Global.Settings.Models.SchoolTourModeModel) {
                return DayPeriod.BigDayPeriods[DayPeriod.AM_PEAK].Middle;
            } else {
                return DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Middle;
            }
        }

        public static int GetDestinationDepartureTime(int model) {
            if (model == Global.Settings.Models.WorkTourModeModel) {
                return DayPeriod.BigDayPeriods[DayPeriod.PM_PEAK].Middle;
            } else if (model == Global.Settings.Models.SchoolTourModeModel) {
                return DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Middle;
            } else {
                return DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Middle;
            }
        }

        public static int GetParkingDuration(bool isFulltimeWorker) {
            return isFulltimeWorker ? 9 : 6;
        }

        public static int GetPrimaryFlag(int tourCategory) {
            return (tourCategory == Global.Settings.TourCategories.Primary).ToFlag();
        }

        public static int GetSecondaryFlag(int tourCategory) {
            return (tourCategory == Global.Settings.TourCategories.Secondary).ToFlag();
        }

        public static int GetDurationUnder1HourFlag(int durationInMinutes) {
            return (durationInMinutes < Global.Settings.Times.OneHour).ToFlag();
        }

        public static int GetDuration1To2HoursFlag(int durationInMinutes) {
            return (durationInMinutes.IsRightExclusiveBetween(Global.Settings.Times.OneHour, Global.Settings.Times.TwoHours)).ToFlag();
        }

        public static int GetDurationUnder4HoursFlag(int durationInMinutes) {
            return (durationInMinutes < Global.Settings.Times.FourHours).ToFlag();
        }

        public static int GetDurationUnder8HoursFlag(int durationInMinutes) {
            return (durationInMinutes < Global.Settings.Times.EightHours).ToFlag();
        }

        public static int GetDurationUnder9HoursFlag(int durationInMinutes) {
            return (durationInMinutes < Global.Settings.Times.NineHours).ToFlag();
        }

        public static void ResetRandom(this IHouseholdWrapper household, int index) {
            if (household == null) {
                throw new ArgumentNullException("household");
            }

            ResetRandom(household.RandomUtility, household.SeedValues, index);
        }

        public static void ResetRandom(this IHouseholdDayWrapper householdDay, int index) {
            if (householdDay == null) {
                throw new ArgumentNullException("householdDay");
            }

            ResetRandom(householdDay.Household.RandomUtility, householdDay.Household.SeedValues, index, householdDay.AttemptedSimulations);
        }

        public static void ResetRandom(this IPersonWrapper person, int index) {
            if (person == null) {
                throw new ArgumentNullException("person");
            }

            ResetRandom(person.Household.RandomUtility, person.SeedValues, index);
        }

        public static void ResetRandom(this IPersonDayWrapper personDay, int index) {
            if (personDay == null) {
                throw new ArgumentNullException("personDay");
            }

            ResetRandom(personDay.Household.RandomUtility, personDay.Person.SeedValues, index, personDay.AttemptedSimulations, personDay.Day);
        }

        private static void ResetRandom(IRandomUtility randomUtility, int[] seedValues, int index, int attemptedSimulations = 0, int day = 1) {
            if (!Global.Configuration.ShouldSynchronizeRandomSeed) {
                return;
            }

            // increasing the number of random seeds instead

            while (index >= Global.Settings.NumberOfRandomSeeds) {
                index -= Global.Settings.NumberOfRandomSeeds; // makes sure index is in range 
            }

            if (attemptedSimulations > 0) {
                var randomSeed = seedValues[index];

                if (randomSeed > 0) {
                    randomSeed -= attemptedSimulations;
                } else {
                    randomSeed += attemptedSimulations;
                }

                seedValues[index] = randomSeed;
            }

            var dayIndex = day - 1;

            if (dayIndex > 0) {
                var randomSeed = seedValues[index];

                if (randomSeed > 0) {
                    randomSeed -= dayIndex;
                } else {
                    randomSeed += dayIndex;
                }

                seedValues[index] = randomSeed;
            }

            //Global.PrintFile.WriteLine("Seed reset to index {0} value {1}", index, seedValues[index]);

            randomUtility.ResetUniform01(seedValues[index]);
        }

        public static int[] GetRandomSampling(int size, int randomSeed) {
            var random = new Random(randomSeed);
            var seedValues = new int[size];

            for (var i = 0; i < size; i++) {
                seedValues[i] = random.Next(short.MinValue, short.MaxValue + 1);
            }

            return seedValues;
        }
    }
}