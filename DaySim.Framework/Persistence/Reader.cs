// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DaySim.Framework.Core;

namespace DaySim.Framework.Persistence {
  public class Reader<TModel> : IEnumerator<TModel>, IEnumerable<TModel> where TModel : class {
    private readonly Dictionary<int, long> _index;
    private readonly Dictionary<string, Dictionary<int, int[]>> _indexes = new Dictionary<string, Dictionary<int, int[]>>();

    private readonly int _size;
    private byte[] _buffer;
    private readonly FileStream _stream;
    private readonly int _count;

    private Dictionary<int, long>.ValueCollection.Enumerator _enumerator;
    private long _position;
    private TModel _model;
    private readonly string path;

    public Reader(string path) {
      this.path = path;
      FileInfo file = new FileInfo(path);

      if (!file.Exists) {
        throw new Exception("Reader passed path '" + path + "' which does not exist!");
      }
      _size = Marshal.SizeOf(typeof(TModel));
      _buffer = new byte[_size];
      _stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
      _count = (int)file.Length / _size;

      _index = new Dictionary<int, long>(_count);

      FileInfo ifile = ModelUtility.GetIndexFile(path);
      int isize = Marshal.SizeOf(typeof(Element));
      byte[] ibuffer = new byte[isize];

      using (FileStream istream = ifile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
        while (istream.Read(ibuffer, 0, isize) != 0) {
          Element element = ModelUtility.PtrToStructure<Element>(ref ibuffer);

          _index.Add(element.Id, element.Position);
        }
      }
    }

    public int Count => _count;

    public TModel Seek(int id) {
      lock (typeof(TModel)) {
#if DEBUG
        ParallelUtility.countLocks("typeof (TModel)");

#endif
#endif


        return !_index.TryGetValue(id, out long location) ? null : Seek(location);
      }
    }

    private TModel Seek(long location) {
      long offset = location - _position;

      if (offset != 0) {
        _stream.Seek(offset, SeekOrigin.Current);
      }

      _position = location + _size;

      _stream.Read(_buffer, 0, _size);

      _model = ModelUtility.PtrToStructure<TModel>(ref _buffer);

      return _model;
    }

    public virtual void Dispose() {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing && _stream != null) {
        _stream.Dispose();
      }
    }

    ~Reader() {
      Dispose(true);
    }

    public bool MoveNext() {
      if (!_enumerator.MoveNext()) {
        return false;
      }

      Seek(_enumerator.Current);

      return true;
    }

    public void Reset() {
      _enumerator = _index.Values.GetEnumerator();
      _position = 0;
      _stream.Position = 0;
      _model = null;
    }

    public TModel Current => _model;

    object IEnumerator.Current => Current;

    public virtual IEnumerator<TModel> GetEnumerator() {
      Reset();

      return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public void BuildIndex(string indexName, string idName, string parentIdName) {
      Type type = typeof(TModel);
      System.Reflection.PropertyInfo idProperty = type.GetProperty(idName);
      System.Reflection.PropertyInfo parentIdProperty = type.GetProperty(parentIdName);
      HashSet<int> parentIds = new HashSet<int>();
      Dictionary<int, int> keys = new Dictionary<int, int>(Count);

      foreach (TModel model in this) {
        int id = (int)idProperty.GetValue(model, null);
        int parentId = (int)parentIdProperty.GetValue(model, null);

        parentIds.Add(parentId);
        keys.Add(id, parentId);
      }

      keys = keys.OrderBy(entry => entry.Value).ToDictionary(entry => entry.Key, entry => entry.Value);

      Dictionary<int, int[]> index = new Dictionary<int, int[]>(parentIds.Count);
      Dictionary<int, int>.Enumerator enumerator = keys.GetEnumerator();

      enumerator.MoveNext();

      foreach (int parentId in parentIds.OrderBy(parentId => parentId)) {
        List<int> ids = new List<int>();

        while (enumerator.Current.Value == parentId) {
          ids.Add(enumerator.Current.Key);

          enumerator.MoveNext();
        }

        index.Add(parentId, ids.ToArray());
      }

      _indexes.Add(indexName, index);

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteLine(@"Building index {0} on {1} for foreign key {2}.", indexName, type.Name, parentIdName);
      }
    }

    public IList<TModel> Seek(int parentId, string indexName) {
      lock (typeof(TModel)) {
#if DEBUG
        ParallelUtility.countLocks("typeof (TModel)");
#endif

        Dictionary<int, int[]> index = _indexes[indexName];


        return index.TryGetValue(parentId, out int[] elements) ? elements.Select(Seek).ToList() : new List<TModel>();
      }
    }
  }
}