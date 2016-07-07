// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;
using DaySim.Framework.Core;
using DaySim.Framework.ShadowPricing;

namespace DaySim.ShadowPricing {
	public static class ShadowPriceReader {
		public static Dictionary<int, IShadowPriceParcel> ReadShadowPrices() {
			var shadowPrices = new Dictionary<int, IShadowPriceParcel>();
			var shadowPriceFile = new FileInfo(Global.ShadowPricesPath);

			if (!shadowPriceFile.Exists || !Global.Configuration.ShouldUseShadowPricing || (!Global.Configuration.ShouldRunWorkLocationModel && !Global.Configuration.ShouldRunSchoolLocationModel)) {
				return shadowPrices;
			}

			using (var reader = new CountingReader(shadowPriceFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
				reader.ReadLine();

				string line;

				while ((line = reader.ReadLine()) != null) {
					var tokens = line.Split(new[] {Global.Configuration.ShadowPriceDelimiter}, StringSplitOptions.RemoveEmptyEntries);

					var shadowPriceParcel = new ShadowPriceParcel {
						ParcelId = Convert.ToInt32(tokens[0]),
						EmploymentDifference = Convert.ToDouble(tokens[1]), // DELTSPUW
						ShadowPriceForEmployment = Convert.ToDouble(tokens[2]), // SHADEMP
						StudentsK12Difference = Convert.ToDouble(tokens[3]), // DELTSPUS
						ShadowPriceForStudentsK12 = Convert.ToDouble(tokens[4]), // SHADPK12
						StudentsUniversityDifference = Convert.ToDouble(tokens[5]), // DELTSPUU
						ShadowPriceForStudentsUniversity = Convert.ToDouble(tokens[6]), // SHADPUNI
					};

					shadowPrices.Add(shadowPriceParcel.ParcelId, shadowPriceParcel);
				}
			}

			return shadowPrices;
		}
	}
}