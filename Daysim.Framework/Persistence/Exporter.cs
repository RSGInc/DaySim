// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.Persistence {
	public abstract class Exporter<TModel> : IExporter<TModel> where TModel : IModel, new() {
		private int _current;
		private readonly char _delimiter;
		private readonly StreamWriter _writer;

		protected Exporter(string outputPath, char delimiter) {
			var outputFile = new FileInfo(outputPath);

			_writer = new StreamWriter(outputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read)) {AutoFlush = false};
			_delimiter = delimiter;

			WriteHeader();
		}

		private void WriteHeader() {
			var type = typeof (TModel);
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var fields = properties.Select(property => property.GetCustomAttributes(typeof (ColumnNameAttribute), true).Cast<ColumnNameAttribute>().SingleOrDefault()).Where(attribute => attribute != null).Select(attribute => attribute.ColumnName).ToList();
			var i = 0;

			foreach (var field in fields) {
				i++;

				_writer.Write(field);

				if (i == fields.Count) {
					_writer.WriteLine();
				}
				else {
					_writer.Write(_delimiter);
				}
			}
		}

		public void Export(TModel model) {
			WriteModel(_writer, model, _delimiter);

			_current++;

			if (_current % 1000 == 0) {
				_writer.Flush();
			}
		}

		[UsedImplicitly]
		public abstract void WriteModel(StreamWriter writer, TModel model, char delimiter);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				_writer.Dispose();
			}
		}
	}
}