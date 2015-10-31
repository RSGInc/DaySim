// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.Framework.Core;
using HDF5DotNet;

namespace Daysim.Framework.Roster
{
    public class HDF5SkimFileReader : ISkimFileReader
    {
        private readonly string _path;
        private Dictionary<int, int> _mapping;
        private UInt16[][] _matrix;

        public HDF5SkimFileReader(string path, Dictionary<int, int> mapping)
        {
            _path = path;
            _mapping = mapping;
        }

        public SkimMatrix Read(string filename, int field, float scale)
        {
            Console.WriteLine("Reading {0}", filename);
            int hdf5NameEnd = filename.IndexOf("/");

            // the first part of the name in the roster file is the hdf5 file:
            string HDFName = filename.Substring(0, hdf5NameEnd);

            //rename filename to be only the name of the skim inside of the time period file
            filename = filename.Substring(hdf5NameEnd);

            string hdfFile = _path + "\\" + HDFName;

            var dataFile = H5F.open(hdfFile, H5F.OpenMode.ACC_RDONLY);
            var dataSet = H5D.open(dataFile, filename);
            var space = H5D.getSpace(dataSet);
            var size2 = H5S.getSimpleExtentDims(space);
            long count = size2[0];
            long count2 = size2[1];
            long numZones = _mapping.Count();

            var dataArray = new UInt16[count, count2];
            var wrapArray = new H5Array<UInt16>(dataArray);
            H5DataTypeId tid1 = H5D.getType(dataSet);

            H5D.read(dataSet, tid1, wrapArray);

            // if the count in the hdf5 file is larger than the number of
            // tazs in the mapping, ignore the values over the total number
            //of tazs in the mapping because these are not valid zones.
            _matrix = new ushort[numZones][];
            for (var i = 0; i < numZones; i++)
            {
                _matrix[i] = new ushort[numZones];
            }

            for (var i = 0; i < numZones; i++)
            {
                for (var j = 0; j < numZones; j++)
                {
                    _matrix[i][j] = dataArray[i, j];
                }
            }

            var skimMatrix = new SkimMatrix(_matrix);
            return skimMatrix;

        }




    }

}

