// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.ChoiceModels;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.Factories;

namespace DaySim.DomainModels.Persisters {
  [UsedImplicitly]
  [Factory(Factory.PersistenceFactory, Category = Category.Persister)]
  public class FullHalfTourPersister<TModel> : Persister<TModel> where TModel : class, IFullHalfTour, new() {
    public override void Export(IModel model) {
      base.Export(model);

      ChoiceModelFactory.FullHalfTourFileRecordsWritten++;
    }
  }
}