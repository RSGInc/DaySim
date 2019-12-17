// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.IO;
using System.Runtime.InteropServices;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.Persistence {
  public abstract class Importer<TModel> : IImporter where TModel : IModel, new() {
    private readonly int _isize;
    private readonly byte[] _ibuffer;

    private readonly int _size;
    private readonly byte[] _buffer;

    private readonly string _inputPath;
    private readonly char _delimiter;

    protected Importer(string inputPath, char delimiter) {
      _isize = Marshal.SizeOf(typeof(Element));
      _ibuffer = new byte[_isize];

      _size = Marshal.SizeOf(typeof(TModel));
      _buffer = new byte[_size];

      _inputPath = inputPath;
      _delimiter = delimiter;
    }

    public void Import(string path) {
      FileInfo inputFile = new FileInfo(_inputPath);
      FileInfo ifile = ModelUtility.GetIndexFile(path);
      FileInfo file = new FileInfo(path);

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteLine(@"Importing ""{0}"" into ""{1}"" for type {2}.", inputFile.Name, file.Name, typeof(TModel).Name);
        Global.PrintFile.WriteLine(@"Creating file ""{0}"".", file.FullName);
        Global.PrintFile.WriteLine(@"Creating index file ""{0}"" for primary key.", ifile.FullName);
      }

      using (CountingReader reader = new CountingReader(inputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        using (BinaryWriter iwriter = new BinaryWriter(ifile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
          using (BinaryWriter writer = new BinaryWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))) {
            long position = 0L;

            reader.ReadLine();

            string line;

            while ((line = reader.ReadLine()) != null) {
              string[] row = line.Split(new[] { _delimiter });
 
              TModel model = new TModel();

              SetModel(model, row);

              Write(iwriter, new Element(model.Id, position));
              Write(writer, model);

              position += _size;

              if (reader.LineNumber % 1024 == 0) {
                iwriter.Flush();
                writer.Flush();
              }
            }
          }
        }
      }
    }

    [UsedImplicitly]
    public abstract void SetModel(TModel model, string[] row);

    protected virtual void Write(BinaryWriter writer, Element element) {
      System.IntPtr ptr = Marshal.AllocHGlobal(_isize);

      Marshal.StructureToPtr(element, ptr, false);
      Marshal.Copy(ptr, _ibuffer, 0, _isize);
      Marshal.FreeHGlobal(ptr);

      writer.Write(_ibuffer);
    }

    protected virtual void Write(BinaryWriter writer, object model) {
      System.IntPtr ptr = Marshal.AllocHGlobal(_size);

      Marshal.StructureToPtr(model, ptr, false);
      Marshal.Copy(ptr, _buffer, 0, _size);
      Marshal.FreeHGlobal(ptr);

      writer.Write(_buffer);
    }
  }
}
