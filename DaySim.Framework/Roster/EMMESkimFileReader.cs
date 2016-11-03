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
using System.Text;

namespace DaySim.Framework.Roster {
    public class EMMESkimFileReader : ISkimFileReader {
        private string _path;
        private Dictionary<int, int> _mapping;
        private FileInfo _file;
        private BinaryReader _reader;

        private UInt32 _mmat;
        private UInt32 _mcent;
        private UInt32 _ptr64;
        private UInt16[][] _matrix;

        public EMMESkimFileReader(string path, Dictionary<int, int> mapping) {
            _path = path;
            _mapping = mapping;

            _file = new FileInfo(path);
            Console.WriteLine("Loading skim file: {0}", _file.Name);


            if (!_file.Exists) {
                throw new FileNotFoundException(string.Format("The skim file {0} could not be found.", _file.FullName));
            }

            _reader = new BinaryReader(_file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public SkimMatrix Read(string filename, int field, float scale) {
            GetEMMEMatrix(filename, field, (float)scale);
            return new SkimMatrix(_matrix);
        }

        private void GetEMMEMatrix(string filename, int matrixNumber, float scale) {

            ReadEMMEBankHeader();
            Int64 curptr = _ptr64 + _mcent * _mcent * (matrixNumber - 1);


            var count = _mapping.Count;
            _matrix = new ushort[count][];
            for (var i = 0; i < count; i++) {
                _matrix[i] = new ushort[count];
            }

            _reader.BaseStream.Seek(curptr * 4, SeekOrigin.Begin);

            for (int i = 1; i <= _mcent; i++)
                for (int j = 1; j <= _mcent; j++) {
                    float value = _reader.ReadSingle() * scale;
                    if (_mapping.ContainsKey(i) && _mapping.ContainsKey(j)) {
                        if (value >= ushort.MaxValue - 1) {
                            value = ushort.MaxValue - 1;
                        } else if (value < 0) {
                            value = 0;
                        } else {
                            value = (float)Math.Round(value);
                        }
                        _matrix[_mapping[i]][_mapping[j]] = ((ushort)Convert.ChangeType(value, typeof(ushort)));
                    }
                }
        }

        private void ReadEMMEBankHeader() {
            UInt32 mask = 0x0FFFFFFF;
            _reader.BaseStream.Seek(4, SeekOrigin.Begin);
            UInt32 ptr1 = _reader.ReadUInt32();
            ptr1 = (ptr1 & mask);
            ptr1 *= 4;
            ptr1 += 52 * 4;
            _reader.BaseStream.Seek((long)(ptr1 - 4), SeekOrigin.Begin);
            _mcent = _reader.ReadUInt32();

            ptr1 += 6 * 4;
            _reader.BaseStream.Seek((long)(ptr1 - 4), SeekOrigin.Begin);
            _mmat = _reader.ReadUInt32();

            _reader.BaseStream.Seek(60 * 4, SeekOrigin.Begin);
            UInt32 ptr60 = _reader.ReadUInt32() & mask;

            _reader.BaseStream.Seek(64 * 4, SeekOrigin.Begin);
            _ptr64 = _reader.ReadUInt32() & mask;

            Console.WriteLine("Header information in file : " + _mmat + " matrices of dimension " + _mcent + " pointer is " + _ptr64);
        }
    }
}
