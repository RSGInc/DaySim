// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System.Collections.Generic;
using System.Linq;
using DaySim.DestinationParkingShadowPricing;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;

namespace DaySim {
  public sealed class DestinationParkingNodeDao {
    private readonly Dictionary<int, IDestinationParkingNodeWrapper> _nodes = new Dictionary<int, IDestinationParkingNodeWrapper>();
    private readonly Dictionary<int, int[]> _zoneIdKeys = new Dictionary<int, int[]>();
    private readonly Dictionary<int, int[]> _parcelIdKeys = new Dictionary<int, int[]>();

    public DestinationParkingNodeDao() {
      Framework.DomainModels.Persisters.IPersisterReader<IDestinationParkingNode> reader =
                Global
                    .ContainerDaySim.GetInstance<IPersistenceFactory<IDestinationParkingNode>>()
                    .Reader;

      IDestinationParkingNodeCreator creator =
                Global
                    .ContainerDaySim
                    .GetInstance<IWrapperFactory<IDestinationParkingNodeCreator>>()
                    .Creator;

      Dictionary<int, HashSet<int>> zoneIdKeys = new Dictionary<int, HashSet<int>>();
      Dictionary<int, HashSet<int>> parcelIdKeys = new Dictionary<int, HashSet<int>>();

      Dictionary<int, Framework.ShadowPricing.IDestinationParkingShadowPriceNode> destinationParkingShadowPrices = DestinationParkingShadowPriceReader.ReadDestinationParkingShadowPrices();

      foreach (IDestinationParkingNode destinationParkingNode in reader) {
        IDestinationParkingNodeWrapper node = creator.CreateWrapper(destinationParkingNode);
        int id = node.Id;

        _nodes.Add(id, node);

        int zoneId = node.ZoneId;

        if (!zoneIdKeys.TryGetValue(zoneId, out HashSet<int> zoneIdKey)) {
          zoneIdKey = new HashSet<int>();

          zoneIdKeys.Add(zoneId, zoneIdKey);
        }

        zoneIdKey.Add(id);

        int parcelId = node.ParcelId;

        if (!parcelIdKeys.TryGetValue(parcelId, out HashSet<int> parcelIdKey)) {
          parcelIdKey = new HashSet<int>();

          parcelIdKeys.Add(parcelId, parcelIdKey);
        }

        node.SetDestinationParkingShadowPricing(destinationParkingShadowPrices);

        parcelIdKey.Add(id);
      }

      foreach (KeyValuePair<int, HashSet<int>> entry in zoneIdKeys) {
        _zoneIdKeys.Add(entry.Key, entry.Value.ToArray());
      }

      foreach (KeyValuePair<int, HashSet<int>> entry in parcelIdKeys) {
        _parcelIdKeys.Add(entry.Key, entry.Value.ToArray());
      }
    }

    public IEnumerable<IDestinationParkingNodeWrapper> Nodes => _nodes.Values;

    public IDestinationParkingNodeWrapper Get(int id) {

      return _nodes.TryGetValue(id, out IDestinationParkingNodeWrapper destinationParkingNode) ? destinationParkingNode : null;
    }

    public IDestinationParkingNodeWrapper[] GetAllByZoneId(int zoneId) {

      if (!_zoneIdKeys.TryGetValue(zoneId, out int[] key)) {
        return new IDestinationParkingNodeWrapper[0];
      }

      IDestinationParkingNodeWrapper[] nodes = new IDestinationParkingNodeWrapper[key.Length];

      for (int i = 0; i < key.Length; i++) {
        nodes[i] = _nodes[key[i]];
      }

      return nodes;
    }

    public IDestinationParkingNodeWrapper[] GetAllByNearestParcelId(int parcelId) {

      if (!_parcelIdKeys.TryGetValue(parcelId, out int[] key)) {
        return new IDestinationParkingNodeWrapper[0];
      }

      IDestinationParkingNodeWrapper[] nodes = new IDestinationParkingNodeWrapper[key.Length];

      for (int i = 0; i < key.Length; i++) {
        nodes[i] = _nodes[key[i]];
      }

      return nodes;
    }
  }
}