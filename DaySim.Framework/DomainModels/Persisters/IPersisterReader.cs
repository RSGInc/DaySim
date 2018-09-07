// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Collections.Generic;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.DomainModels.Persisters {
  public interface IPersisterReader<out TModel> : IEnumerable<TModel> where TModel : IModel {
    int Count { get; }

    TModel Seek(int id);

    IEnumerable<TModel> Seek(int id, string indexName);

    void BuildIndex(string indexName, string idName, string parentIdName);
  }
}