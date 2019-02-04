// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HDF5DotNet;

namespace DaySim.Framework.Roster {
  public class OMXSkimFileReader : ISkimFileReader {
    private readonly string _path;
    private Dictionary<int, int> _mapping;
    private ushort[][] _matrix;

    public OMXSkimFileReader(string path, Dictionary<int, int> mapping) {
      _path = path;
      _mapping = mapping;
    }

    public SkimMatrix Read(string filename, int field, float scale) {

      //hdf5 filename contain "filename/group/skim"
      //get the index of group
      int hdf5GroupEnd = filename.LastIndexOf("/");

      //get the index of filename
      int hdf5NameEnd = hdf5GroupEnd > 0 ? filename.LastIndexOf("/", hdf5GroupEnd - 1) : -1;

      //get the omx/hdf5 filename
      string HDFName = filename.Substring(0, hdf5NameEnd);

      //rename filename to be only the name of the skim matrix inside of the skim file
      filename = filename.Substring(hdf5NameEnd);

      string hdfFile = Path.Combine(_path, HDFName);

      FileInfo file = new FileInfo(hdfFile);
      if (!file.Exists) {
        throw new FileNotFoundException(string.Format("The skim file {0} could not be found.", file.FullName));
      }

      H5FileId dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
      H5DataSetId dataSet = H5D.open(dataFile, filename);
      H5DataSpaceId space = H5D.getSpace(dataSet);
      long[] size2 = H5S.getSimpleExtentDims(space);
      long nRows = size2[0];
      long nCols = size2[1];
      long numZones = _mapping.Count();

      Console.WriteLine("Loading skim file: {0} dataset: {1} with nRows={2}, nCols={3} and will read data for {4} zones", hdfFile, filename, nRows, nCols, numZones);
      // if the count in the hdf5 file is larger than the number of
      // tazs in the mapping, ignore the values over the total number
      //of tazs in the mapping because these are not valid zones.
      _matrix = new ushort[numZones][];
      for (int i = 0; i < numZones; i++) {
        _matrix[i] = new ushort[numZones];
      }

      //OMX is a square matrix of doubles
      //In addition to the data folder for matrices, an OMX file has a lookup folder
      //with a zone mapping vector.  However, this is ignored since DaySim also has one.
      //Therefore, it is assumed the OMX matrix does not skip rows/cols and every row/col
      //corresponds to an actual zone in the DaySim zone mapping file by index
      double[,] dataArray = new double[nRows, nCols];
      H5Array<double> wrapArray = new H5Array<double>(dataArray);
      H5DataTypeId tid1 = H5D.getType(dataSet);

      H5D.read(dataSet, tid1, wrapArray);

      for (int row = 0; row < nRows; row++) {
        if (_mapping.TryGetValue(row + 1, out int mappedRow)) {
          for (int col = 0; col < nCols; col++) {
            if (_mapping.TryGetValue(col + 1, out int mappedCol)) {
              double value = dataArray[row, col] * scale;

              if (value > 0) {
                if (value > ushort.MaxValue - 1) {
                  value = ushort.MaxValue - 1;
                }

                _matrix[mappedRow][mappedCol] = (ushort)value;
              }
            }
          }
        }
      }

      SkimMatrix skimMatrix = new SkimMatrix(_matrix);
      return skimMatrix;

    }

  }

}

