// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Daysim.DomainModels.Factories;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Creators;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Factories;
using Daysim.Framework.Roster;
using Daysim.PathTypeModels;
using Ninject;

namespace Daysim.AggregateLogsums {
	public sealed class AggregateLogsumsCalculator : IAggregateLogsumsCalculator {
		#region fields

		private const double UPPER_LIMIT = 88;
		private const double LOWER_LIMIT = -88;

		private const int TOTAL_SUBZONES = 2;

		private const double CP_FACTOR = 2.11;
		private const double OPERATING_COST_PER_MILE = 12 / 100D;

		private const double HBB002 = 3.12388134576; // Logs
		private const double HBB003 = -2.16590737971; // Logs nocar
		private const double HBB007 = -.776826932662; // Logs cardef notacc
		private const double HBB008 = -.752994057693; // Logs nocarcomp notacc
		private const double HBB012 = .255437071437; // Mix  carcomp
		private const double HBB013 = .209999652021; // Mix  nocarcomp
		private const double HBB014 = .156561356225; // Mix  kid
		private const double HBB016 = -2.50578111289; // Dist nocar
		private const double HBB018 = -.437673659522; // Dist kid

		private const double WBB002 = 1.70001268283; // Logs
		private const double WBB011 = -.496403160879; // Logs nocarcomp shtacc
		private const double WBB012 = .195668005751; // Mix  carcomp
		private const double WBB013 = .182866099603; // Mix  nocarcomp
		private const double WBB015 = -1.49203420961; // Dist

		private const double PSB006 = 2.33573201673; // logsum-escort
		private const double PSB007 = -1.29601730724; // logsum-nocar
		private const double PSB008 = -.266979325073; // logsum-escort-cardef
		private const double PSB009 = .436012769748; // logsum-escort-kid
		private const double PSB011 = -1.26787087053; // logsum-escort-cardef-no trans
		private const double PSB012 = -.827387496450; // logsum-escort-fullcar-no trans
		private const double PSB014 = -.367660123913; // logsum-cardef-shorttran
		private const double PSB016 = -.792851200220; // dist-escort
		private const double PSB017 = -1.98077342014; // dist-nocar
		private const double PSB018 = -.776260182170; // dist-escort-cardef
		private const double PSB020 = .361667839945; // mix-escort-carcomp
		private const double PSB021 = .189948294419; // mix-escort-fullcar
		private const double PSB022 = .036384904155; // mix-escort-kid
		private const double PSB023 = 2.82606645523; // logsum-pbus
		private const double PSB024 = -.742169132509; // logsum-pbus-cardef
		private const double PSB025 = -1.38993113443;
		private const double PSB027 = -.622830026131;
		private const double PSB029 = -.723252096509;
		private const double PSB030 = -1.78526472355;
		private const double PSB031 = .233440902312;
		private const double PSB032 = .209891014920;
		private const double PSB033 = .103676352142;
		private const double PSB034 = 2.89015368047;
		private const double PSB035 = -.261573643037;
		private const double PSB036 = .302722833229;
		private const double PSB037 = -1.64992937999;
		private const double PSB038 = -1.32810647135;
		private const double PSB045 = 2.71982242315;
		private const double PSB047 = -1.61983672991;
		private const double PSB049 = 1.15445556706;
		private const double PSB053 = .120021457132;
		private const double PSB054 = .306341459234;
		private const double PSB055 = .182264781393;
		private const double PSB056 = 2.07584489709;
		private const double PSB057 = -.126488991621;
		private const double PSB059 = -.630250565955;
		private const double PSB060 = -.562518734549;
		private const double PSB063 = -.803416471565;
		private const double PSB064 = .304756963914;
		private const double PSB065 = .288427801983;
		private const double PSB066 = .320091662041;

		private const double HBG019 = -.176367207995; // edu
		private const double HBG020 = 1.40311192602; // foo
		private const double HBG021 = .531367308949; // gov
		private const double HBG022 = -1.38308191771; // off
		private const double HBG023 = -.130709284415; // agr
		private const double HBG024 = .916691323384; // ret
		private const double HBG025 = 0; // ser
		private const double HBG026 = -.0269761476821; // med
		private const double HBG027 = -1.80011026418; // ind
		private const double HBG029 = -3.25084777395; // hou
		private const double HBG030 = -.959690488049; // uni
		private const double HBG031 = -3.09664722877; // shs

		private const double WBG019 = -2.60518100153; // edu
		private const double WBG020 = 0; // foo
		private const double WBG021 = -1.27398655693; // gov
		private const double WBG022 = -2.90026841500; // off
		private const double WBG023 = -.872694197449; // agr
		private const double WBG024 = -1.59639913257; // ret
		private const double WBG025 = -1.72356143640; // ser
		private const double WBG026 = -2.94223758686; // med
		private const double WBG027 = -2.47772854756; // ind
		private const double WBG029 = -5.57856345378; // hou
		private const double WBG030 = -2.64139245948; // shs

		private const double PSG067 = 1.47592989653;
		private const double PSG068 = .826499933382;
		private const double PSG069 = 1.24621448062;
		private const double PSG070 = -2.00173706237;
		private const double PSG071 = .497495733739;
		private const double PSG072 = -.949177656726;
		private const double PSG073 = 1.30488407539;
		private const double PSG074 = -1.30594917109;
		private const double PSG075 = -.630026028603;
		private const double PSG077 = -2.01545875852;
		private const double PSG078 = .616977219331;
		private const double PSG080 = -1.02245321566;
		private const double PSG081 = -.244810856403;
		private const double PSG082 = -.347661104214;
		private const double PSG083 = -.976273868441;
		private const double PSG084 = -3.40771150887;
		private const double PSG085 = -.856665446208;
		private const double PSG086 = -1.49634243979;
		private const double PSG087 = 0;
		private const double PSG088 = -2.15270273194;
		private const double PSG090 = -4.15355309474;
		private const double PSG091 = -1.28942776363;
		private const double PSG094 = -.606047614827;
		private const double PSG095 = -4.18797424830;
		private const double PSG097 = -3.14148055499;
		private const double PSG098 = 0;
		private const double PSG099 = -4.14168378582;
		private const double PSG100 = -4.16750359064;
		private const double PSG103 = -5.25760570242;
		private const double PSG105 = -3.13892615296;
		private const double PSG107 = 0;
		private const double PSG115 = -3.50458575189;
		private const double PSG117 = -1.97448229250;
		private const double PSG119 = -1.63321108382;
		private const double PSG120 = 0;
		private const double PSG121 = -1.18014333257;
		private const double PSG122 = -1.18183615431;
		private const double PSG123 = -2.48769062703;
		private const double PSG125 = -.550501994079;
		private const double PSG126 = -2.30761023730;
		private const double PSG129 = -3.23454701949;
		private const double PSG130 = -1.93793528420;

		private static readonly double[][][] _distanceParameters =
			new[] {
				new[] {
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D}
				},
				new[] {
					new[] {1520D, 08340D},
					new[] {1121D, 03754D},
					new[] {1651D, 10496D},
					new[] {1767D, 15984D}
				},
				new[] {
					new[] {1520D, 08340D},
					new[] {0900D, 03754D},
					new[] {1284D, 06567D},
					new[] {1412D, 09382D}
				},
				new[] {
					new[] {1234D, 05233D},
					new[] {0900D, 03754D},
					new[] {1347D, 07478D},
					new[] {1411D, 13810D}
				},
				new[] {
					new[] {1763D, 07778D},
					new[] {1308D, 03754D},
					new[] {1825D, 10120D},
					new[] {1905D, 13618D}
				},
				new[] {
					new[] {1401D, 05478D},
					new[] {1002D, 03754D},
					new[] {1575D, 09708D},
					new[] {1586D, 12886D}
				},
				new[] {
					new[] {1400D, 05481D},
					new[] {1000D, 03754D},
					new[] {1364D, 06851D},
					new[] {1874D, 10524D}
				},
				new[] {
					new[] {1400D, 05481D},
					new[] {1000D, 03754D},
					new[] {1364D, 06851D},
					new[] {1874D, 10524D}
				},
				new[] {
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D}
				},
				new[] {
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D}
				},
				new[] {
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D},
					new[] {0D, 0D}
				}
			};

		/*  cost   */
		private static readonly double[] _p01 = new[] {0, -.1826, -.2008, -.1200, -.2361, -.4386, -.1215, -.3194, 0, 0, 0};

		/*  ivt    */
		private static readonly double[] _p02 = new[] {0, -.025, -.025, -.04, -.02, -.025, -.03, -.025, 0, 0, 0};

		/*  ovt    */
		private static readonly double[] _p03 = new[] {0, -.07227, -.08339, -.1296, -.05341, -.0757, -.09441, -.06894, 0, 0, 0};

		/*d-const  */
		private static readonly double[] _p11 = new[] {0, -.5821, -1.358, -5.619, .4807, .03664, -1.793, -.7357, 0, 0, 0};

		/*d-carcomp*/
		private static readonly double[] _p14 = new[] {0, -.3404, -.5896, .3267, -.3777, -.3622, -.4127, -.9187, 0, 0, 0};

		/*s-const  */
		private static readonly double[] _p21 = new[] {0, -.4841, -2.396, -1.073, -.03517, -.4242, -.6850, -1.047, 0, 0, 0};

		/*s-child  */
		private static readonly double[] _p22 = new[] {0, .2458, 1.033, 1.822, 1.194, .3822, -1.720, .2101, 0, 0, 0};

		/*s-nocars */
		private static readonly double[] _p23 = new[] {0, -2.518, -1.782, -5.265, -1.730, -2.187, -2.472, -1.933, 0, 0, 0};

		/*s-carcomp*/
		private static readonly double[] _p24 = new[] {0, -.1648, -.1185, .4327, -.1929, -.3522, -.4882, -.4833, 0, 0, 0};

		/*t-const  */
		private static readonly double[] _p31 = new[] {0, -3.911, -4.792, -3.447, -2.712, -3, -4.013, -3.589, 0, 0, 0};

		/*t-child  */
		private static readonly double[] _p32 = new[] {0, -1D, 0, -1, -1, -1, -1, -1, 0, 0, 0};

		/*t-nocars */
		private static readonly double[] _p33 = new[] {0, 2.722, 2.048, 1, 3.117, 1.910, 2.663, 2.485, 0, 0, 0};

		/*t-carcomp*/
		private static readonly double[] _p34 = new[] {0, .7025, 1.226, .00472, -.1959, 1.049, 1.798, -.2369, 0, 0, 0};

		/*t-longwo */
		private static readonly double[] _p37 = new[] {0, -1.958, -1.268, -2, -2, -2, -2, -2, 0, 0, 0};

		private readonly int _zoneCount;
		private readonly Dictionary<int, IZone> _eligibleZones;
		private readonly ISubzone[][] _zoneSubzones;
		private readonly int _middayStartMinute;

		#endregion

		public AggregateLogsumsCalculator() {
            var file = Global.AggregateLogsumsPath.ToFile();

		    if (Global.Configuration.ShouldLoadAggregateLogsumsFromFile && file.Exists)
		        return;

			var zoneReader =
				Global
					.Kernel
					.Get<IPersistenceFactory<IZone>>()
//					.Get<IPersistenceFactory<IZone>>()
					.Reader;

			_eligibleZones = zoneReader.Where(z => z.DestinationEligible).ToDictionary(z => z.Id, z => z);
			_zoneCount = zoneReader.Count;
			_zoneSubzones = CalculateZoneSubzones();
			_middayStartMinute = DayPeriod.BigDayPeriods[DayPeriod.MIDDAY].Start;
		}

		public void Calculate(IRandomUtility randomUtility) {
			var file = Global.AggregateLogsumsPath.ToFile();

			if (Global.Configuration.ShouldLoadAggregateLogsumsFromFile && file.Exists) {
				Global.AggregateLogsums = LoadAggregateLogsumsFromFile(file);

				return;
			}

			Global.AggregateLogsums = new double[_zoneCount][][][][];

			Parallel.For(0, _zoneCount, new ParallelOptions {MaxDegreeOfParallelism = ParallelUtility.LargeDegreeOfParallelism}, id => CalculateZone(randomUtility, id));

			for (var id = 0; id < _zoneCount; id++) {
				
				//CalculateZone(randomUtility, id);    //instead of parallel for above; used in testing
				
				var purposes = Global.AggregateLogsums[id];

				for (var purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
					var carOwnerships = purposes[purpose];

					for (var carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
						var votALSegments = carOwnerships[carOwnership];

						for (var votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
							var transitAccesses = votALSegments[votALSegment];

							for (var transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
								transitAccesses[transitAccess] = Math.Log(transitAccesses[transitAccess]);
							}
						}
					}
				}
			}

			if (Global.Configuration.ShouldLoadAggregateLogsumsFromFile && !file.Exists) {
				SaveAggregateLogsumsToFile(file);
			}
		}

		private void CalculateZone(IRandomUtility randomUtility, int id) {
			Global.AggregateLogsums[id] = ComputeZone(randomUtility, id);
		}

		private double[][][][][] LoadAggregateLogsumsFromFile(FileInfo file) {
			using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
				var formatter = new BinaryFormatter();

				return (double[][][][][]) formatter.Deserialize(stream);
			}
		}

		private void SaveAggregateLogsumsToFile(FileInfo file) {
			using (var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read)) {
				var formatter = new BinaryFormatter();

				formatter.Serialize(stream, Global.AggregateLogsums);
			}
		}

		private double[][][][] ComputeZone(IRandomUtility randomUtility, int id) {
			var purposes = new double[Global.Settings.Purposes.TotalPurposes][][][];

			for (var purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
				var carOwnerships = new double[Global.Settings.CarOwnerships.TotalCarOwnerships][][];

				purposes[purpose] = carOwnerships;

				for (var carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
					var votALSegments = new double[Global.Settings.VotALSegments.TotalVotALSegments][];

					carOwnerships[carOwnership] = votALSegments;

					for (var votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
						var transitAccesses = new double[Global.Settings.TransitAccesses.TotalTransitAccesses];

						votALSegments[votALSegment] = transitAccesses;

						for (var transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
							transitAccesses[transitAccess] = Constants.EPSILON;
						}
					}
				}
			}

			IZone origin;

			if (!_eligibleZones.TryGetValue(id, out origin)) {
				return purposes;
			}

			foreach (var destination in _eligibleZones.Values) {
				var setImpedance = true;
				var subzones = _zoneSubzones[destination.Id];

				//const double parkingCost = 0;

				// mode impedance
				var sovInVehicleTimeFromOrigin = ImpedanceRoster.GetValue("ivtime", Global.Settings.Modes.Sov, Global.Settings.PathTypes.FullNetwork, Global.Settings.ValueOfTimes.DefaultVot, _middayStartMinute, id, destination.Id).Variable;
				var scaledSovDistanceFromOrigin = 0D;
				var transitGenTime = 0D;
				var walkGenTime = 0D;
				var sovGenTime = 0D;
				var hov2GenTime = 0D;

				for (var purpose = Global.Settings.Purposes.HomeBasedComposite; purpose <= Global.Settings.Purposes.Social; purpose++) {
					var carOwnerships = purposes[purpose];
					var distanceParameters = _distanceParameters[purpose];

					// set purpose inputs
					var escortFlag = (purpose == Global.Settings.Purposes.Escort).ToFlag();
					var personalBusinessFlag = (purpose == Global.Settings.Purposes.PersonalBusiness).ToFlag();
					var shoppingFlag = (purpose == Global.Settings.Purposes.Shopping).ToFlag();
					var mealFlag = (purpose == Global.Settings.Purposes.Meal).ToFlag();
					var socialFlag = (purpose == Global.Settings.Purposes.Social).ToFlag();

					var p01 = _p01[purpose];
					var p02 = _p02[purpose];
					var p03 = _p03[purpose];
					var p11 = _p11[purpose];
					var p14 = _p14[purpose];
					var p21 = _p21[purpose];
					var p22 = _p22[purpose];
					var p23 = _p23[purpose];
					var p24 = _p24[purpose];
					var p31 = _p31[purpose];
					var p32 = _p32[purpose];
					var p33 = _p33[purpose];
					var p34 = _p34[purpose];
					var p37 = _p37[purpose];

					for (var carOwnership = Global.Settings.CarOwnerships.Child; carOwnership < Global.Settings.CarOwnerships.TotalCarOwnerships; carOwnership++) {
						var votALSegments = carOwnerships[carOwnership];

						// set car ownership inputs
                        var childFlag = FlagUtility.GetChildFlag(carOwnership);
                        var noCarsFlag = FlagUtility.GetNoCarsFlag(carOwnership);
                        var carCompetitionFlag = FlagUtility.GetCarCompetitionFlag(carOwnership);
                        var noCarCompetitionFlag = FlagUtility.GetNoCarCompetitionFlag(carOwnership);
                        var carDeficitFlag = FlagUtility.GetCarDeficitFlag(carOwnership);

						var distanceParameter = distanceParameters[carOwnership][1] / 100D; // converts hundreths of minutes to minutes

						for (var votALSegment = Global.Settings.VotALSegments.Low; votALSegment < Global.Settings.VotALSegments.TotalVotALSegments; votALSegment++) {
							var transitAccesses = votALSegments[votALSegment];

							// set vot specific variables
							double timeCoefficient = Global.Settings.VotALSegments.TimeCoefficient;
							var costCoefficient = (votALSegment == Global.Settings.VotALSegments.Low)
							                      	? Global.Settings.VotALSegments.CostCoefficientLow
							                      	: (votALSegment == Global.Settings.VotALSegments.Medium)
							                      	  	? Global.Settings.VotALSegments.CostCoefficientMedium
							                      	  	: Global.Settings.VotALSegments.CostCoefficientHigh;

							for (var transitAccess = Global.Settings.TransitAccesses.Gt0AndLteQtrMi; transitAccess < Global.Settings.TransitAccesses.TotalTransitAccesses; transitAccess++) {
								var purposeUtility = 0D;

								// set transit access flags
								var hasNearTransitAccessFlag = (transitAccess == Global.Settings.TransitAccesses.Gt0AndLteQtrMi).ToFlag();
								var hasNoTransitAccessFlag = (transitAccess == Global.Settings.TransitAccesses.None).ToFlag();

								foreach (var subzone in subzones) {
									var size = subzone.GetSize(purpose);

									if (size <= -50 || sovInVehicleTimeFromOrigin <= 0 || (2 * sovInVehicleTimeFromOrigin) > distanceParameter) {
										continue;
									}

									// set subzone flags
									var hasNoTransitEgressFlag = 1 - (subzone.Sequence == 0 ? 1 : 0);

									if (setImpedance) {
										setImpedance = false;

                    // intermediate variable of type IEnumerable<dynamic> is needed to acquire First() method as extension
                    IEnumerable<dynamic> pathTypeModels;
										                                        
										pathTypeModels = PathTypeModelFactory.Model.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                                            costCoefficient, timeCoefficient, true, 1, 0, 0.0, false, Global.Settings.Modes.Walk);
                    var walkPath = pathTypeModels.First();

										walkGenTime = walkPath.GeneralizedTimeLogsum;

                                        
										pathTypeModels = PathTypeModelFactory.Model.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                                            costCoefficient, timeCoefficient, true, 1, 0, 0.0, false, Global.Settings.Modes.Sov);
                    var sovPath = pathTypeModels.First();

										var sovDistanceFromOrigin = (sovPath.PathDistance/ Global.Settings.DistanceUnitsPerMile) / 2D;
										scaledSovDistanceFromOrigin = sovDistanceFromOrigin / 10D;

										sovGenTime = sovPath.GeneralizedTimeLogsum;

                    pathTypeModels = PathTypeModelFactory.Model.Run(randomUtility, id, destination.Id, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                                            costCoefficient, timeCoefficient, true, 1, 0, 0.0, false, Global.Settings.Modes.Hov2);
                    var hov2Path = pathTypeModels.First();

										hov2GenTime = hov2Path.GeneralizedTimeLogsum;

                    //if using stop areas, use stop area nearest to the zone centroid
                    var transitOid = (!Global.StopAreaIsEnabled) ? id 
											: (origin.NearestStopAreaId > 0) ? Global.TransitStopAreaMapping[origin.NearestStopAreaId] 
											: id;
										var transitDid = (!Global.StopAreaIsEnabled) ? destination.Id 
											: (destination.NearestStopAreaId > 0) ? Global.TransitStopAreaMapping[destination.NearestStopAreaId] 
											: id;
										
										pathTypeModels = PathTypeModelFactory.Model.Run(randomUtility, transitOid, transitDid, _middayStartMinute, _middayStartMinute, Global.Settings.Purposes.PersonalBusiness,
                                            costCoefficient, timeCoefficient, true, 1, 0, Global.Configuration.Policy_UniversalTransitFareDiscountFraction, false, Global.Settings.Modes.Transit);
                    var transitPath = pathTypeModels.First();

										transitGenTime = transitPath.GeneralizedTimeLogsum;
									}

									var modeUtilitySum = 0D;

									// SOV
									if (childFlag == 0 && noCarsFlag == 0 && sovGenTime != Global.Settings.GeneralizedTimeUnavailable) {
										modeUtilitySum += ComputeUtility(
											//p01 * (OPERATING_COST_PER_MILE * sovDistance + sovToll) +
											//p01 * parkingCost +
											//p02 * sovInVehicleTime +
											timeCoefficient * sovGenTime +
											p11 +
											p14 * carCompetitionFlag);
									}

									// HOV
									if (hov2GenTime != Global.Settings.GeneralizedTimeUnavailable) {
										modeUtilitySum += ComputeUtility(
											//p01 * ((OPERATING_COST_PER_MILE * hov2Distance + hov2Toll) / CP_FACTOR) +
											//p01 * parkingCost / CP_FACTOR +
											//p02 * hov2InVehicleTime +
											timeCoefficient * hov2GenTime +
											p21 +
											p22 * childFlag +
											p23 * noCarsFlag +
											p24 * carCompetitionFlag);
									}

									// TRANSIT
									if (transitGenTime != Global.Settings.GeneralizedTimeUnavailable && hasNoTransitAccessFlag == 0 && hasNoTransitEgressFlag == 0) {
										modeUtilitySum += ComputeUtility(
											//p01 * transitFare +
											//p02 * transitInVehicleTime +
											//p03 * transitInitialWaitTime +
											//p03 * transitNumberOfBoards +
											timeCoefficient * transitGenTime +
											p31 +
											p32 * childFlag +
											p33 * noCarsFlag +
											p34 * carCompetitionFlag +
											p37 * hasNoTransitAccessFlag);
									}

									// WALK
									if (walkGenTime != Global.Settings.GeneralizedTimeUnavailable) {
										modeUtilitySum += ComputeUtility(
											//p03 * walkDistance * 20);
											timeCoefficient * walkGenTime);
									}

									var modeLogsum = modeUtilitySum > Constants.EPSILON ? Math.Log(modeUtilitySum) : -30D;

									switch (purpose) {
										case 1: // HOME_BASED_COMPOSITE
											purposeUtility += ComputeUtility(
												size +
												HBB002 * modeLogsum +
												HBB003 * modeLogsum * noCarsFlag +
												HBB007 * modeLogsum * (noCarsFlag + carCompetitionFlag) * hasNoTransitAccessFlag +
												HBB008 * modeLogsum * noCarCompetitionFlag * hasNoTransitAccessFlag +
												HBB012 * subzone.MixedUseMeasure * carCompetitionFlag +
												HBB013 * subzone.MixedUseMeasure * noCarCompetitionFlag +
												HBB014 * subzone.MixedUseMeasure * childFlag +
												HBB016 * scaledSovDistanceFromOrigin * noCarsFlag +
												HBB018 * scaledSovDistanceFromOrigin * childFlag);

											break;
										case 2: // WORK_BASED
											purposeUtility += ComputeUtility(
												size +
												WBB002 * modeLogsum +
												WBB011 * modeLogsum * noCarCompetitionFlag * hasNearTransitAccessFlag +
												WBB012 * subzone.MixedUseMeasure * carCompetitionFlag +
												WBB013 * subzone.MixedUseMeasure * noCarCompetitionFlag +
												WBB015 * scaledSovDistanceFromOrigin);

											break;
										default: // ESCORT, PERSONAL_BUSINESS, SHOPPING, MEAL, SOCIAL or RECREATION
											purposeUtility += ComputeUtility(
												size +
												PSB006 * escortFlag * modeLogsum +
												PSB007 * noCarsFlag * modeLogsum +
												PSB008 * escortFlag * carDeficitFlag * modeLogsum +
												PSB009 * escortFlag * childFlag * modeLogsum +
												PSB011 * escortFlag * carDeficitFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB012 * escortFlag * noCarCompetitionFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB014 * carDeficitFlag * hasNearTransitAccessFlag * modeLogsum +
												PSB016 * escortFlag * scaledSovDistanceFromOrigin +
												PSB017 * noCarsFlag * scaledSovDistanceFromOrigin +
												PSB018 * escortFlag * carDeficitFlag * scaledSovDistanceFromOrigin +
												PSB020 * escortFlag * carCompetitionFlag * subzone.MixedUseMeasure +
												PSB021 * escortFlag * noCarCompetitionFlag * subzone.MixedUseMeasure +
												PSB022 * escortFlag * childFlag * subzone.MixedUseMeasure +
												PSB023 * personalBusinessFlag * modeLogsum +
												PSB024 * personalBusinessFlag * carDeficitFlag * modeLogsum +
												PSB025 * personalBusinessFlag * childFlag * modeLogsum +
												PSB027 * personalBusinessFlag * noCarCompetitionFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB029 * personalBusinessFlag * carDeficitFlag * scaledSovDistanceFromOrigin +
												PSB030 * personalBusinessFlag * childFlag * scaledSovDistanceFromOrigin +
												PSB031 * personalBusinessFlag * carCompetitionFlag * subzone.MixedUseMeasure +
												PSB032 * personalBusinessFlag * noCarCompetitionFlag * subzone.MixedUseMeasure +
												PSB033 * personalBusinessFlag * childFlag * subzone.MixedUseMeasure +
												PSB034 * shoppingFlag * modeLogsum +
												PSB035 * shoppingFlag * carDeficitFlag * modeLogsum +
												PSB036 * shoppingFlag * childFlag * modeLogsum +
												PSB037 * shoppingFlag * carDeficitFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB038 * shoppingFlag * noCarCompetitionFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB045 * mealFlag * modeLogsum +
												PSB047 * mealFlag * childFlag * modeLogsum +
												PSB049 * mealFlag * noCarCompetitionFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB053 * mealFlag * carCompetitionFlag * subzone.MixedUseMeasure +
												PSB054 * mealFlag * noCarCompetitionFlag * subzone.MixedUseMeasure +
												PSB055 * mealFlag * childFlag * subzone.MixedUseMeasure +
												PSB056 * socialFlag * modeLogsum +
												PSB057 * socialFlag * carDeficitFlag * modeLogsum +
												PSB059 * socialFlag * carDeficitFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB060 * socialFlag * noCarCompetitionFlag * hasNoTransitAccessFlag * modeLogsum +
												PSB063 * socialFlag * childFlag * scaledSovDistanceFromOrigin +
												PSB064 * socialFlag * carCompetitionFlag * subzone.MixedUseMeasure +
												PSB065 * socialFlag * noCarCompetitionFlag * subzone.MixedUseMeasure +
												PSB066 * socialFlag * childFlag * subzone.MixedUseMeasure);

											break;
									}
								}

								transitAccesses[transitAccess] += purposeUtility;
							}
						}
					}
				}
			}

			return purposes;
		}

		private ISubzone[][] CalculateZoneSubzones() {
			var subzoneFactory = new SubzoneFactory(Global.Configuration);
			var zoneSubzones = new ISubzone[_zoneCount][];

			for (var id = 0; id < _zoneCount; id++) {
				var subzones = new ISubzone[TOTAL_SUBZONES];

				zoneSubzones[id] = subzones;

				for (var subzone = 0; subzone < TOTAL_SUBZONES; subzone++) {
					subzones[subzone] = (ISubzone) subzoneFactory.Create(subzone);
				}
			}

			var parcelReader = 
				Global
					.Kernel
					.Get<IPersistenceFactory<IParcel>>()
					.Reader;

			var parcelCreator =
				Global
					.Kernel
					.Get<IWrapperFactory<IParcelCreator>>()
					.Creator;

			foreach (var parcel in parcelReader) {
				var parcelWrapper = (IParcelWrapper) parcelCreator.CreateWrapper(parcel);

				var subzones = zoneSubzones[parcelWrapper.ZoneId];
				// var subzone = (parcel.GetDistanceToTransit() > 0 && parcel.GetDistanceToTransit() <= .5) ? 0 : 1;  
				// JLBscale replaced above with following:
				var subzone = (parcelWrapper.GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile > 0 && parcelWrapper.GetDistanceToTransit() / Global.Settings.DistanceUnitsPerMile <= .5) ? 0 : 1;

				subzones[subzone].Households += parcelWrapper.Households;
				subzones[subzone].StudentsK8 += parcelWrapper.StudentsK8;
				subzones[subzone].StudentsHighSchool += parcelWrapper.StudentsHighSchool;
				subzones[subzone].StudentsUniversity += parcelWrapper.StudentsUniversity;
				subzones[subzone].EmploymentEducation += parcelWrapper.EmploymentEducation;
				subzones[subzone].EmploymentFood += parcelWrapper.EmploymentFood;
				subzones[subzone].EmploymentGovernment += parcelWrapper.EmploymentGovernment;
				subzones[subzone].EmploymentIndustrial += parcelWrapper.EmploymentIndustrial;
				subzones[subzone].EmploymentMedical += parcelWrapper.EmploymentMedical;
				subzones[subzone].EmploymentOffice += parcelWrapper.EmploymentOffice;
				subzones[subzone].EmploymentRetail += parcelWrapper.EmploymentRetail;
				subzones[subzone].EmploymentService += parcelWrapper.EmploymentService;
				subzones[subzone].EmploymentTotal += parcelWrapper.EmploymentTotal;
				//Removed following that are not defined the same way in actum.  Need to accommodate parking soem other way
				//subzones[subzone].ParkingOffStreetPaidDailySpaces += parcelWrapper.ParkingOffStreetPaidDailySpaces;
				//subzones[subzone].ParkingOffStreetPaidHourlySpaces += parcelWrapper.ParkingOffStreetPaidHourlySpaces;
			}

			foreach (var subzones in _eligibleZones.Values.Select(zone => zoneSubzones[zone.Id])) {
				for (var subzone = 0; subzone < TOTAL_SUBZONES; subzone++) {
					var hou = subzones[subzone].Households;
					var k12 = subzones[subzone].StudentsK8 + subzones[subzone].StudentsHighSchool;
					var uni = subzones[subzone].StudentsUniversity;
					var edu = subzones[subzone].EmploymentEducation;
					var foo = subzones[subzone].EmploymentFood;
					var gov = subzones[subzone].EmploymentGovernment;
					var ind = subzones[subzone].EmploymentIndustrial;
					var med = subzones[subzone].EmploymentMedical;
					var off = subzones[subzone].EmploymentOffice;
					var ret = subzones[subzone].EmploymentRetail;
					var ser = subzones[subzone].EmploymentService;
					var tot = subzones[subzone].EmploymentTotal;
					const double oth = 0;

					var subtotal = foo + ret + ser + med;


					//subzones[subzone].MixedUseMeasure = Math.Log(1 + subtotal * (subzones[subzone]).ParkingOffStreetPaidHourlySpaces * 100 / Math.Max(subtotal + (subzones[subzone]).ParkingOffStreetPaidHourlySpaces * 100, Constants.EPSILON));
					//zeroed out above MixedUseMeasure because it relies on parking variable not available for actum
					subzones[subzone].MixedUseMeasure = 0; 

					subzones[subzone].SetSize(Global.Settings.Purposes.HomeBasedComposite, ComputeSize(Math.Exp(HBG019) * edu + Math.Exp(HBG020) * foo + Math.Exp(HBG021) * gov + Math.Exp(HBG022) * off + Math.Exp(HBG023) * oth + Math.Exp(HBG024) * ret + Math.Exp(HBG025) * ser + Math.Exp(HBG026) * med + Math.Exp(HBG027) * ind + Math.Exp(HBG029) * hou + Math.Exp(HBG030) * uni + Math.Exp(HBG031) * k12));
					subzones[subzone].SetSize(Global.Settings.Purposes.WorkBased, ComputeSize(Math.Exp(WBG019) * edu + Math.Exp(WBG020) * foo + Math.Exp(WBG021) * gov + Math.Exp(WBG022) * off + Math.Exp(WBG023) * oth + Math.Exp(WBG024) * ret + Math.Exp(WBG025) * ser + Math.Exp(WBG026) * med + Math.Exp(WBG027) * ind + Math.Exp(WBG029) * hou + Math.Exp(WBG030) * uni));
					subzones[subzone].SetSize(Global.Settings.Purposes.Escort, ComputeSize(Math.Exp(PSG067) * edu + Math.Exp(PSG068) * foo + Math.Exp(PSG069) * gov + Math.Exp(PSG070) * off + Math.Exp(PSG071) * oth + Math.Exp(PSG072) * ret + Math.Exp(PSG073) * ser + Math.Exp(PSG074) * med + Math.Exp(PSG075) * ind + Math.Exp(PSG077) * hou + Math.Exp(PSG078) * uni + k12));
					subzones[subzone].SetSize(Global.Settings.Purposes.PersonalBusiness, ComputeSize(Math.Exp(PSG080) * edu + Math.Exp(PSG081) * foo + Math.Exp(PSG082) * gov + Math.Exp(PSG083) * off + Math.Exp(PSG084) * oth + Math.Exp(PSG085) * ret + Math.Exp(PSG086) * ser + Math.Exp(PSG087) * med + Math.Exp(PSG088) * ind + Math.Exp(PSG090) * hou + Math.Exp(PSG091) * uni));
					subzones[subzone].SetSize(Global.Settings.Purposes.Shopping, ComputeSize(Math.Exp(PSG094) * foo + Math.Exp(PSG095) * gov + Math.Exp(PSG097) * oth + Math.Exp(PSG098) * ret + Math.Exp(PSG099) * ser + Math.Exp(PSG100) * med + Math.Exp(PSG103) * hou + Math.Exp(PSG105) * k12));
					subzones[subzone].SetSize(Global.Settings.Purposes.Meal, ComputeSize(Math.Exp(PSG107) * foo + Math.Exp(PSG115) * tot + Math.Exp(PSG117) * uni));
					subzones[subzone].SetSize(Global.Settings.Purposes.Social, ComputeSize(Math.Exp(PSG119) * edu + Math.Exp(PSG120) * foo + Math.Exp(PSG121) * gov + Math.Exp(PSG122) * off + Math.Exp(PSG123) * oth + Math.Exp(PSG125) * ser + Math.Exp(PSG126) * med + Math.Exp(PSG129) * hou + Math.Exp(PSG130) * uni));
				}
			}

			return zoneSubzones;
		}

		private double ComputeSize(double size) {
			if (size < Constants.EPSILON) {
				return -99;
			}

			return Math.Log(size);
		}

		private double ComputeUtility(double utility) {
			if (utility > UPPER_LIMIT || utility < LOWER_LIMIT) {
				utility = utility > UPPER_LIMIT ? UPPER_LIMIT : LOWER_LIMIT;
			}

			return Math.Exp(utility);
		}

		
	}
}