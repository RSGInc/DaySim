// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using Daysim.DomainModels.Actum.Models;
using Daysim.DomainModels.Actum.Models.Interfaces;
using Daysim.DomainModels.Actum.Wrappers.Interfaces;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.DomainModels.Creators;
using Daysim.Framework.Factories;

namespace Daysim.DomainModels.Actum.Wrappers {
	[Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
	public class HouseholdDayWrapper : Default.Wrappers.HouseholdDayWrapper, IActumHouseholdDayWrapper {
		private IActumHouseholdDay _householdDay;

		[UsedImplicitly]
		public HouseholdDayWrapper(IHouseholdDay householdDay, IHouseholdWrapper householdWrapper) : base(householdDay, householdWrapper) {
			_householdDay = (IActumHouseholdDay) householdDay;
		}

		#region domain model properies

		public int SharedActivityHomeStays {
			get { return _householdDay.SharedActivityHomeStays; }
			set { _householdDay.SharedActivityHomeStays = value; }
		}

		public int NumberInLargestSharedHomeStay {
			get { return _householdDay.NumberInLargestSharedHomeStay; }
			set { _householdDay.NumberInLargestSharedHomeStay = value; }
		}

		public int StartingMinuteSharedHomeStay {
			get { return _householdDay.StartingMinuteSharedHomeStay; }
			set { _householdDay.StartingMinuteSharedHomeStay = value; }
		}

		public int DurationMinutesSharedHomeStay {
			get { return _householdDay.DurationMinutesSharedHomeStay; }
			set { _householdDay.DurationMinutesSharedHomeStay = value; }
		}

		public int AdultsInSharedHomeStay {
			get { return _householdDay.AdultsInSharedHomeStay; }
			set { _householdDay.AdultsInSharedHomeStay = value; }
		}

		public int ChildrenInSharedHomeStay {
			get { return _householdDay.ChildrenInSharedHomeStay; }
			set { _householdDay.ChildrenInSharedHomeStay = value; }
		}

		public int PrimaryPriorityTimeFlag {
			get { return _householdDay.PrimaryPriorityTimeFlag; }
			set { _householdDay.PrimaryPriorityTimeFlag = value; }
		}

		#endregion

		#region flags, choice model properties, etc.

		public int JointTourFlag {
			get; set;
		}
	
		#endregion

		#region init/utility/export methods

		protected override IHouseholdDay ResetHouseholdDay() {
			_householdDay = new HouseholdDay {
				Id = Id,
				HouseholdId = HouseholdId,
				Day = Day,
				DayOfWeek = DayOfWeek,
				ExpansionFactor = ExpansionFactor
			};

			return _householdDay;
		}

		#endregion
	}
}