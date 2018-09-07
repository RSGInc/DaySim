// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.IO;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ShadowPricing {
  public sealed class ShadowPriceWriter : IDisposable {
    private readonly StreamWriter _writer;

    public ShadowPriceWriter(FileInfo file) {
      if (file == null) {
        throw new ArgumentNullException("file");
      }

      _writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

      _writer.Write("PARCELID" + Global.Configuration.ShadowPriceDelimiter);
      _writer.Write("DELTSPUW" + Global.Configuration.ShadowPriceDelimiter);
      _writer.Write("SHADPEMP" + Global.Configuration.ShadowPriceDelimiter);
      _writer.Write("DELTSPUS" + Global.Configuration.ShadowPriceDelimiter);
      _writer.Write("SHADPK12" + Global.Configuration.ShadowPriceDelimiter);
      _writer.Write("DELTSPUU" + Global.Configuration.ShadowPriceDelimiter);
      _writer.Write("SHADPUNI");
      _writer.WriteLine();
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
      if (disposing) {
        _writer.Dispose();
      }
    }

    public void Write(IParcelWrapper parcel) {
      if (parcel == null) {
        throw new ArgumentNullException("parcel");
      }

      _writer.Write(string.Format("{0}{1}", parcel.Id, Global.Configuration.ShadowPriceDelimiter));
      _writer.Write(string.Format("{0:0.000000}{1}", parcel.EmploymentDifference, Global.Configuration.ShadowPriceDelimiter));
      _writer.Write(string.Format("{0:0.000000}{1}", parcel.ShadowPriceForEmployment, Global.Configuration.ShadowPriceDelimiter));
      _writer.Write(string.Format("{0:0.000000}{1}", parcel.StudentsK12Difference, Global.Configuration.ShadowPriceDelimiter));
      _writer.Write(string.Format("{0:0.000000}{1}", parcel.ShadowPriceForStudentsK12, Global.Configuration.ShadowPriceDelimiter));
      _writer.Write(string.Format("{0:0.000000}{1}", parcel.StudentsUniversityDifference, Global.Configuration.ShadowPriceDelimiter));
      _writer.Write(string.Format("{0:0.000000}", parcel.ShadowPriceForStudentsUniversity));
      _writer.WriteLine();
    }
  }
}