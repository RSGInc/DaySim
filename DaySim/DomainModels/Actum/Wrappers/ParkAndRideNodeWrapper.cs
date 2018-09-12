// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

//using DaySim.DomainModels.Actum.Models.Interfaces;
//using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Actum.Wrappers {
  [Factory(Factory.WrapperFactory, Category = Category.Wrapper, DataType = DataType.Actum)]
  public class ParkAndRideNodeWrapper : Default.Wrappers.ParkAndRideNodeWrapper, IParkAndRideNodeWrapper {
    private readonly IParkAndRideNode _parkAndRideNode;

    [UsedImplicitly]
    public ParkAndRideNodeWrapper(IParkAndRideNode parkAndRideNode) : base(parkAndRideNode) {
      _parkAndRideNode = parkAndRideNode;
    }

    #region domain model properies

    //public string TerminalName {
    //	get { return _parkAndRideNode.TerminalName; }
    //	set { _parkAndRideNode.TerminalName = value; }
    //}

    public int ParkingTypeId {
      get => _parkAndRideNode.ParkingTypeId;
      set => _parkAndRideNode.ParkingTypeId = value;
    }

    public double CostPerHour08_18 {
      get => _parkAndRideNode.CostPerHour08_18;
      set => _parkAndRideNode.CostPerHour08_18 = value;
    }

    public double CostPerHour18_23 {
      get => _parkAndRideNode.CostPerHour18_23;
      set => _parkAndRideNode.CostPerHour18_23 = value;
    }

    public double CostPerHour23_08 {
      get => _parkAndRideNode.CostPerHour23_08;
      set => _parkAndRideNode.CostPerHour23_08 = value;
    }

    public double CostAnnual {
      get => _parkAndRideNode.CostAnnual;
      set => _parkAndRideNode.CostAnnual = value;
    }

    public int PRFacility {
      get => _parkAndRideNode.PRFacility;
      set => _parkAndRideNode.PRFacility = value;
    }

    public int LengthToStopArea {
      get => _parkAndRideNode.LengthToStopArea;
      set => _parkAndRideNode.LengthToStopArea = value;
    }

    public int Auto {
      get => _parkAndRideNode.Auto;
      set => _parkAndRideNode.Auto = value;
    }

    #endregion





  }
}