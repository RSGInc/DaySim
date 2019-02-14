// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DaySim.Framework.Core;
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

    public SkimMatrix Read(string fileNameAndGroupAndDataTable, int field, float scale) {

      //hdf5 filename contain "filename/group/skim"
      //get the index of group
      int hdf5GroupEnd = fileNameAndGroupAndDataTable.LastIndexOf("/");

      string fileNameAndGroup = hdf5GroupEnd > 0 ? fileNameAndGroupAndDataTable.Substring(0, hdf5GroupEnd) : fileNameAndGroupAndDataTable;

      //get the index of filename
      int hdf5NameEnd = hdf5GroupEnd > 0 ? fileNameAndGroup.LastIndexOf("/") : -1;

      string groupName = fileNameAndGroup.Substring(hdf5NameEnd + 1);

      //get the omx/hdf5 filename
      string HDFName = fileNameAndGroup.Substring(0, hdf5NameEnd);


      string groupAndDataTable = fileNameAndGroupAndDataTable.Substring(hdf5NameEnd);

      string hdfFile = Path.Combine(_path, HDFName);

      FileInfo file = new FileInfo(hdfFile);
      if (!file.Exists) {
        throw new FileNotFoundException(string.Format("The skim file {0} could not be found.", file.FullName));
      }

      H5FileId dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
      H5DataSetId dataSet = H5D.open(dataFile, groupAndDataTable);
      H5DataSpaceId space = H5D.getSpace(dataSet);
      long[] size2 = H5S.getSimpleExtentDims(space);
      long nRows = size2[0];
      long nCols = size2[1];
      Debug.Assert(nRows == nCols);
      long numZones = _mapping.Count();

      int[] lookupMap = null;
      string lookupMapName = null;
      string lookupGroupName = "lookup";
      H5GroupId luGroup = H5G.open(dataFile, lookupGroupName);

      if (H5G.getNumObjects(luGroup) == 1L) {
        lookupMapName = H5G.getObjectNameByIndex(luGroup, 0);
        H5DataSetId lookupDataSet = H5D.open(dataFile, string.Concat(lookupGroupName, "/", lookupMapName));

        H5DataTypeId lookupMapType = H5D.getType(lookupDataSet);
        H5DataSpaceId lookupMapSpace = H5D.getSpace(lookupDataSet);
        long lookupMapSize = H5S.getSimpleExtentDims(lookupMapSpace)[0];
        lookupMap = new int[lookupMapSize];
        H5Array<int> lookupWrapArray = new H5Array<int>(lookupMap);
        H5D.read(lookupDataSet, lookupMapType, lookupWrapArray);
      }
      if (lookupMap != null) {
        if (lookupMap.Length != nRows) {
          Global.PrintFile.WriteLine(string.Format("DATA WARNING: skim file: {0} has a lookup map named {1} but its length ({2}) is different than the matrix size ({3}) in group table {4}", hdfFile, lookupMapName, lookupMap.Length, nRows, groupAndDataTable));
          lookupMap = null;
        }
      }

      Console.WriteLine("Loading skim file: {0} dataset: {1} with nRows={2}, nCols={3} and will read data for {4} zones. {5}", hdfFile, groupAndDataTable, nRows, nCols, numZones, lookupMap != null ? string.Format("Using lookupMap {0}.", lookupMapName) : "No lookupMap.");
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
        int mappingKey = (lookupMap == null) ? (row + 1) : lookupMap[row];
        if (_mapping.TryGetValue(mappingKey, out int mappedRow)) {
          for (int col = 0; col < nCols; col++) {
            mappingKey = (lookupMap == null) ? (col + 1) : lookupMap[col];
            if (_mapping.TryGetValue(mappingKey, out int mappedCol)) {
              double value = dataArray[row, col] * scale;

              if (value > 0) {
                if (value > ushort.MaxValue - 1) {
                  value = ushort.MaxValue - 1;
                }

                _matrix[mappedRow][mappedCol] = (ushort)value; //bug #208 deferred but this will eventually changed be Convert.ToUInt16 to avoid 0.57*100=56 bug
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

