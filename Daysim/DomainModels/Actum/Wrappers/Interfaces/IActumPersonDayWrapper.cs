// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using Daysim.DomainModels.Actum.Models.Interfaces;
using Daysim.Framework.DomainModels.Wrappers;
using System.Collections.Generic;

namespace Daysim.DomainModels.Actum.Wrappers.Interfaces {
	public interface IActumPersonDayWrapper : IPersonDayWrapper, IActumPersonDay {

		#region relations properties

		//IActumHouseholdWrapper Household { get; set; }

		//IActumPersonWrapper Person { get; set; }

		//IActumHouseholdDayWrapper HouseholdDay { get; set; }

		//List<IActumTourWrapper> Tours { get; set; }

		#endregion
	
		
		
		int CreatedBusinessTours { get; set; }

		int SimulatedBusinessTours { get; set; }

		int SimulatedBusinessStops { get; set; }

		bool SimulatedBusinessStopsExist();
	}
}