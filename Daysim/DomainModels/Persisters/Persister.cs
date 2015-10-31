// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections;
using System.Collections.Generic;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Persisters;
using Daysim.Framework.Persistence;
using Ninject;

namespace Daysim.DomainModels.Persisters {
	public abstract class Persister<TModel> : IPersisterReader<TModel>, IPersisterImporter, IPersisterExporter, IDisposable where TModel : class, IModel, new() {
		private readonly Lazy<Reader<TModel>> _reader = new Lazy<Reader<TModel>>(() =>
			Global
				.Kernel
				.Get<Reader<TModel>>());

		private readonly Lazy<IImporter> _importer = new Lazy<IImporter>(() =>
			Global
				.Kernel
				.Get<ImporterFactory>()
				.GetImporter<TModel>(Global.GetInputPath<TModel>(), Global.GetInputDelimiter<TModel>())
			);

		private readonly Lazy<IExporter<TModel>> _exporter = new Lazy<IExporter<TModel>>(() =>
			Global
				.Kernel
				.Get<ExporterFactory>()
				.GetExporter<TModel>(Global.GetOutputPath<TModel>(), Global.GetOutputDelimiter<TModel>())
			);

		private readonly Lazy<Hdf5Exporter<TModel>> _hdf5Exporter = new Lazy<Hdf5Exporter<TModel>>(() => new Hdf5Exporter<TModel>());

		public int Count {
			get {
				return
					_reader
						.Value
						.Count;
			}
		}

		public TModel Seek(int id) {
			return
				_reader
					.Value
					.Seek(id);
		}

		public IEnumerable<TModel> Seek(int id, string indexName) {
			return
				_reader
					.Value
					.Seek(id, indexName);
		}

		public void BuildIndex(string indexName, string idName, string parentIdName) {
			_reader
				.Value
				.BuildIndex(indexName, idName, parentIdName);
		}

		IEnumerator<TModel> IEnumerable<TModel>.GetEnumerator() {
			return (IEnumerator<TModel>) GetEnumerator();
		}

		public IEnumerator GetEnumerator() {
			return
				_reader
					.Value
					.GetEnumerator();
		}

		public void Import() {
			var message = string.Format("Importing {0} domain models...", typeof (TModel).Name);

			_importer
				.Value
				.BeginImport(Global.GetWorkingPath<TModel>(), message);
		}

		public virtual void Export(IModel model) {
			var m = (TModel) model;

			_exporter
				.Value
				.Export(m);

			if (!Global.Configuration.WriteTripsToHDF5) {
				return;
			}

			_hdf5Exporter
				.Value
				.Export(m);
		}

		public void Dispose() {
			if (Global.Configuration.WriteTripsToHDF5 && _hdf5Exporter != null) {
				_hdf5Exporter
					.Value
					.Flush();
			}

			if (_exporter != null) {
				_exporter
					.Value
					.Dispose();
			}
		}
	}
}