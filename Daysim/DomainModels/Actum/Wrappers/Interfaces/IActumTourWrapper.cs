// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using Daysim.DomainModels.Actum.Models.Interfaces;
using Daysim.Framework.DomainModels.Wrappers;

namespace Daysim.DomainModels.Actum.Wrappers.Interfaces {
	public interface IActumTourWrapper : ITourWrapper, IActumTour {

	
		#region flags/choice model/etc. properties

		int HalfTour1AccessMode { get; set; }

		int HalfTour1AccessPathType { get; set; }

		double HalfTour1AccessTime { get; set; }

		double HalfTour1AccessCost { get; set; }

		double HalfTour1AccessDistance { get; set; }

		int HalfTour1AccessStopArea { get; set; }

		int HalfTour1EgressMode { get; set; }

		int HalfTour1EgressPathType { get; set; }

		double HalfTour1EgressTime { get; set; }

		double HalfTour1EgressCost { get; set; }

		double HalfTour1EgressDistance { get; set; }

		int HalfTour1EgressStopArea { get; set; }
	
		int HalfTour2AccessMode { get; set; }

		int HalfTour2AccessPathType { get; set; }

		double HalfTour2AccessTime { get; set; }

		double HalfTour2AccessCost { get; set; }

		double HalfTour2AccessDistance { get; set; }

		int HalfTour2AccessStopArea { get; set; }

		int HalfTour2EgressMode { get; set; }

		int HalfTour2EgressPathType { get; set; }

		double HalfTour2EgressTime { get; set; }

		double HalfTour2EgressCost { get; set; }

		double HalfTour2EgressDistance { get; set; }

		int HalfTour2EgressStopArea { get; set; }

		#endregion

		
		
		bool IsBusinessPurpose();

		bool IsHovDriverMode();

		bool IsHovPassengerMode();
	}
}