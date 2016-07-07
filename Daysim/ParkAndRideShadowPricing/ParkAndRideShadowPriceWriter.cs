// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.IO;
using DaySim.DomainModels;
using DaySim.DomainModels.Default.Wrappers;
using DaySim.Framework.Core;

//using System.Linq;
//using System.Text;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ParkAndRideShadowPricing {
	public sealed class ParkAndRideShadowPriceWriter : IDisposable {
		private readonly StreamWriter _writer;

		public ParkAndRideShadowPriceWriter(FileInfo file) {
			if (file == null) {
				throw new ArgumentNullException("file");
			}

			_writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

			_writer.Write("NODEID" + Global.Configuration.ParkAndRideShadowPriceDelimiter);
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write("DIFF" + string.Format("{0:0000}{1}", i - 1, Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write("PRICE" + string.Format("{0:0000}{1}", i - 1, Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write("EXLOAD" + string.Format("{0:0000}{1}", i - 1, Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			for (var i = 1; i < Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write("PRLOAD" + string.Format("{0:0000}{1}", i - 1, Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			_writer.Write("PRLOAD" + string.Format("{0:0000}", Global.Settings.Times.MinutesInADay - 1));
			_writer.WriteLine();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				_writer.Dispose();
			}
		}

		public void Write(IParkAndRideNodeWrapper node) {
			if (node == null) {
				throw new ArgumentNullException("node");
			}

			_writer.Write(string.Format("{0}{1}", node.Id, Global.Configuration.ParkAndRideShadowPriceDelimiter));
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write(string.Format("{0:0.000000}{1}", node.ShadowPriceDifference[i - 1], Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write(string.Format("{0:0.000000}{1}", node.ShadowPrice[i - 1], Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay; i++) {
				_writer.Write(string.Format("{0:0.000000}{1}", node.ExogenousLoad[i - 1], Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			for (var i = 1; i <= Global.Settings.Times.MinutesInADay - 1; i++) {
				_writer.Write(string.Format("{0:0.000000}{1}", node.ParkAndRideLoad[i - 1], Global.Configuration.ParkAndRideShadowPriceDelimiter));
			}
			_writer.Write(string.Format("{0:0.000000}", node.ParkAndRideLoad[Global.Settings.Times.MinutesInADay - 1]));
			_writer.WriteLine();
		}
	}
}