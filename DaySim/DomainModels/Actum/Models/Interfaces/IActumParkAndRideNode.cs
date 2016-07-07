// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.DomainModels.Models;

namespace DaySim.DomainModels.Actum.Models.Interfaces {
	public interface IActumParkAndRideNode : IParkAndRideNode {
	//public interface IActumParkAndRideNode : IModel {

		//string TerminalName { get; set; }

		int ParkingTypeId { get; set; }

		double CostPerHour08_18 { get; set; }

		double CostPerHour18_23 { get; set; }

		double CostPerHour23_08 { get; set; }

		double CostAnnual { get; set; }

		int PRFacility { get; set; }

		int LengthToStopArea { get; set; }

		int Auto { get; set; }

	}

}