// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Persisters;
using Daysim.Framework.Factories;

namespace Daysim.DomainModels.Factories {
	public class PersistenceFactory<TModel> : IPersistenceFactory<TModel> where TModel : IModel {
		private IDisposable _persister;

		public IPersisterReader<TModel> Reader { get; private set; }

		public IPersisterImporter Importer { get; private set; }

		public IPersisterExporter Exporter { get; private set; }

		public void Initialize(Configuration configuration) {
			var helper = new FactoryHelper(configuration);

			var type1 = helper.Persistence.GetModelType<TModel>();
			var type2 = helper.Persistence.GetPersisterType<TModel>();

			var args = new[] {type1};
			var constructed = type2.MakeGenericType(args);

			_persister = (IDisposable) Activator.CreateInstance(constructed);

			Reader = (IPersisterReader<TModel>) _persister;
			Importer = (IPersisterImporter) _persister;
			Exporter = (IPersisterExporter) _persister;
		}

		public void Close() {
			_persister.Dispose();
		}
	}
}