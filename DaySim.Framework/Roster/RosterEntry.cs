// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Roster {
	public sealed class RosterEntry {
		private int _matrixKey;

		/// <summary>
		/// Gets or sets the variable.
		/// </summary>
		/// <value>
		/// The variable.
		/// </value>
		public string Variable { get; set; }

		/// <summary>
		/// Gets or sets the mode.
		/// </summary>
		/// <value>
		/// The mode.
		/// </value>
		public int Mode { get; set; }

		/// <summary>
		/// Gets or sets the path type.
		/// </summary>
		/// <value>
		/// The path type.
		/// </value>
		public int PathType { get; set; }

		/// <summary>
		/// Gets or sets the value of time group.
		/// </summary>
		/// <value>
		/// The value of time group.
		/// </value>
		public int VotGroup { get; set; }

		/// <summary>
		/// Gets or sets the start minute.
		/// </summary>
		/// <value>
		/// The start minute.
		/// </value>
		public int StartMinute { get; set; }

		/// <summary>
		/// Gets or sets the end minute.
		/// </summary>
		/// <value>
		/// The end minute.
		/// </value>
		public int EndMinute { get; set; }

		/// <summary>
		/// Gets or sets the length.
		/// </summary>
		/// <value>
		/// The length.
		/// </value>
		public string Length { get; set; }

		/// <summary>
		/// Gets or sets the type of the file.
		/// </summary>
		/// <value>
		/// The type of the file.
		/// </value>
		public string FileType { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the field.
		/// </summary>
		/// <value>
		/// The field.
		/// </value>
		public int Field { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="RosterEntry"/> is transpose.
		/// </summary>
		/// <value>
		/// <c>true</c> if transpose; otherwise, <c>false</c>.
		/// </value>
		public bool Transpose { get; set; }

		/// <summary>
		/// Gets or sets the blend variable.
		/// </summary>
		/// <value>
		/// The blend variable.
		/// </value>
		public string BlendVariable { get; set; }

		/// <summary>
		/// Gets or sets the blend path type.
		/// </summary>
		/// <value>
		/// The blend path type.
		/// </value>
		public int BlendPathType { get; set; }

		/// <summary>
		/// The factor is used to scale the skim value.
		/// </summary>
		/// <value>
		/// The factor.
		/// </value>
		public double Factor { get; set; }

		/// <summary>
		/// Determines if the skim value should be scaled.
		/// </summary>
		public double Scaling { get; set; }

		public int VariableIndex { get; set; }

		public int MatrixIndex { get; set; }

		public int MatrixKey {
			get {
				if (_matrixKey == 0) {
					unchecked {
						_matrixKey = (FileType != null ? FileType.GetHashCode() : 0);
						_matrixKey = (_matrixKey * 397) ^ (Name != null ? Name.GetHashCode() : 0);
						_matrixKey = (_matrixKey * 397) ^ Field;
					}
				}

				return _matrixKey;
			}
		}
	}
}