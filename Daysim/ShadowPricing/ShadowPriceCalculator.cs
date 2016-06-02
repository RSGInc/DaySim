// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using Daysim.ChoiceModels;
using Daysim.Framework.Core;

namespace Daysim.ShadowPricing {
	public static class ShadowPriceCalculator {
		public static void CalculateAndWriteShadowPrices() {
			if (!Global.Configuration.ShouldUseShadowPricing || (!Global.Configuration.ShouldRunWorkLocationModel && !Global.Configuration.ShouldRunSchoolLocationModel)) {
				return;
			}

			double externalEmploymentTotal = 0;
			double employmentPrediction = 0;
			double externalStudentsK12 = 0;
			double studentsK12Prediction = 0;
			double externalStudentsUniversity = 0;
			double studentsUniversityPrediction = 0;

			var zoneShadowPrices = new Dictionary<int, ShadowPriceZone>();

			foreach (var parcel in ChoiceModelFactory.Parcels.Values) {
				if (!zoneShadowPrices.ContainsKey(parcel.ZoneId)) {
					zoneShadowPrices.Add(parcel.ZoneId, new ShadowPriceZone());
				}

				var shadowPriceZone = zoneShadowPrices[parcel.ZoneId];

				if (parcel.ExternalEmploymentTotal < Global.Configuration.UsualWorkParcelThreshold) {
					shadowPriceZone.ExternalEmploymentTotal += parcel.ExternalEmploymentTotal;
					shadowPriceZone.EmploymentPrediction += parcel.EmploymentPrediction;
				}
				if (parcel.GetStudentsK12() < Global.Configuration.UsualSchoolParcelThreshold) {
					shadowPriceZone.ExternalStudentsK12 += parcel.GetStudentsK12();
					shadowPriceZone.StudentsK12Prediction += parcel.StudentsK12Prediction;
				}
				if (parcel.StudentsUniversity < Global.Configuration.UsualUniversityParcelThreshold) {
					shadowPriceZone.ExternalUniversityStudents += parcel.ExternalStudentsUniversity;
					shadowPriceZone.StudentsUniversityPrediction += parcel.StudentsUniversityPrediction;
				}

				externalEmploymentTotal += parcel.ExternalEmploymentTotal; // EMPTOT_W
				employmentPrediction += parcel.EmploymentPrediction; // EMPPRD_P
				externalStudentsK12 += parcel.ExternalStudentsK12; // STUDK12W
				studentsK12Prediction += parcel.StudentsK12Prediction; // K12PRD_P
				externalStudentsUniversity += parcel.ExternalStudentsUniversity; // STUDUNIU
				studentsUniversityPrediction += parcel.StudentsUniversityPrediction; // UNIPRD_P
			}

			var employmentFactor = employmentPrediction / Math.Max(externalEmploymentTotal, 1);
			var studentsK12Factor = studentsK12Prediction / Math.Max(externalStudentsK12, 1);
			var studentsUniversityFactor = studentsUniversityPrediction / Math.Max(externalStudentsUniversity, 1);

            //issue #57 https://github.com/RSGInc/DaySim/issues/57 Must keep safe copy of shadow prices files before overwriting
            if (File.Exists(Global.ShadowPricesPath)) {
                File.Move(Global.ShadowPricesPath, Global.ArchiveShadowPricesPath);
            }

            using (var shadowPriceWriter = new ShadowPriceWriter(new FileInfo(Global.ShadowPricesPath))) {
				foreach (var shadowPriceParcel in ChoiceModelFactory.Parcels.Values) {
					if (!zoneShadowPrices.ContainsKey(shadowPriceParcel.ZoneId)) {
						zoneShadowPrices.Add(shadowPriceParcel.ZoneId, new ShadowPriceZone());
					}

					var shadowPriceZone = zoneShadowPrices[shadowPriceParcel.ZoneId];

					double shadowPrice;
					double absoluteDifference;
					double percentDifference;

					// usual work
					DetermineShadowPrice(Global.Configuration.UsualWorkParcelThreshold, Global.Configuration.UsualWorkPercentTolerance, Global.Configuration.UsualWorkAbsoluteTolerance, shadowPriceParcel.ShadowPriceForEmployment, shadowPriceParcel.EmploymentPrediction, shadowPriceParcel.ExternalEmploymentTotal * employmentFactor, shadowPriceZone.EmploymentPrediction, shadowPriceZone.ExternalEmploymentTotal * employmentFactor, out shadowPrice, out absoluteDifference, out percentDifference);
					shadowPriceParcel.EmploymentDifference = shadowPrice - shadowPriceParcel.ShadowPriceForEmployment;
					shadowPriceParcel.ShadowPriceForEmployment = shadowPrice;

					// usual school-K12
					DetermineShadowPrice(Global.Configuration.UsualSchoolParcelThreshold, Global.Configuration.UsualSchoolPercentTolerance, Global.Configuration.UsualSchoolAbsoluteTolerance, shadowPriceParcel.ShadowPriceForStudentsK12, shadowPriceParcel.StudentsK12Prediction, shadowPriceParcel.ExternalStudentsK12 * studentsK12Factor, shadowPriceZone.StudentsK12Prediction, shadowPriceZone.ExternalStudentsK12 * studentsK12Factor, out shadowPrice, out absoluteDifference, out percentDifference);
					shadowPriceParcel.StudentsK12Difference = shadowPrice - shadowPriceParcel.ShadowPriceForStudentsK12;

					// usual school-university
					DetermineShadowPrice(Global.Configuration.UsualUniversityParcelThreshold, Global.Configuration.UsualUniversityPercentTolerance, Global.Configuration.UsualUniversityAbsoluteTolerance, shadowPriceParcel.ShadowPriceForStudentsUniversity, shadowPriceParcel.StudentsUniversityPrediction, shadowPriceParcel.ExternalStudentsUniversity * studentsUniversityFactor, shadowPriceZone.StudentsUniversityPrediction, shadowPriceParcel.ExternalStudentsUniversity * studentsUniversityFactor, out shadowPrice, out absoluteDifference, out percentDifference); /*JLB 20080724*/
					shadowPriceParcel.StudentsUniversityDifference = shadowPrice - shadowPriceParcel.ShadowPriceForStudentsUniversity;
					shadowPriceParcel.ShadowPriceForStudentsUniversity = shadowPrice;

					// write shadow prices
					shadowPriceWriter.Write(shadowPriceParcel);
				}
			}
		}

		private static void DetermineShadowPrice(int threshold, int percentTolerance, int absoluteTolerance, double previousShadowPrice, double parcelPrediction, double parcelTotal, double zonePrediction, double zoneTotal, out double shadowPrice, out double absoluteDifference, out double percentDifference) {
			double prediction;
			double total;

			if (parcelTotal >= threshold) {
				prediction = parcelPrediction;
				total = parcelTotal;
			}
			else {
				prediction = zonePrediction;
				total = zoneTotal;
			}

			var targ =
				prediction > total
					? Math.Min(prediction, Math.Min(total * (1 + percentTolerance / 100D), total + absoluteTolerance))
					: Math.Max(prediction, Math.Max(total * (1 - percentTolerance / 100D), total - absoluteTolerance));

			shadowPrice = previousShadowPrice + Math.Log(Math.Max(targ, .01) * 1D / Math.Max(prediction, .01));
			absoluteDifference = prediction - total;
			percentDifference = absoluteDifference * 1D / Math.Max(total, 1);
		}
	}
}