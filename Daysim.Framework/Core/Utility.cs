// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


namespace Daysim.Framework.Core {
	public sealed class Utility : IObservationItem {
		public Utility(int position, int parameter, bool hasSizeVariable) {
			PositionIndex = position;
			Parameter = parameter;
			HasSizeVariable = hasSizeVariable;
		}

		public int PositionIndex { get; private set; }

		public int Position {
			get { return PositionIndex + 1; }
		}

		public int Key { get; private set; }

		public double Data { get; private set; }

		public string Label {
			get { return "par_" + Parameter; }
		}

		public int Parameter { get; private set; }

		public bool HasSizeVariable { get; private set; }

		public double TotalValue { get; set; }

		public int TotalNonZeroOccurrences { get; set; }

		public void Update(int key, double value) {
			if (Key != key) {
				Data = 0;
			}

			Key = key;
			Data += value;
		}
	}
}