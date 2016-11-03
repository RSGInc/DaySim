using System;
using System.Collections.Generic;
using System.IO;
using CaliperMTX;

namespace DaySim.Framework.Roster {
    public class TranscadFileSkimReader : ISkimFileReader {
        private string _path;
        private Dictionary<int, int> _mapping;
        private string _file;

        private UInt16[][] _matrix = null;

        public TranscadFileSkimReader(string path, Dictionary<int, int> mapping) {
            _path = path;
            _mapping = mapping;
        }

        public SkimMatrix Read(string filename, int field, float scale) {
            _file = Path.Combine(_path, filename);
            Console.WriteLine("Loading skim file: {0}", _file);

            GetTranscadMatrix(_file, field, scale);
            return new SkimMatrix(_matrix);
        }

        private void GetTranscadMatrix(string matrixPath, int matrixNumber, float scale) {
            var m = new Matrix(matrixPath);
            matrixNumber -= 1;
            m.SetCore((short)matrixNumber);
            var nrows = m.GetBaseNRows();
            var ncols = m.GetBaseNCols();
            var mrow = new float[ncols];

            int count = _mapping.Count;

            _matrix = new ushort[count][];
            for (int i = 0; i < count; i++) {
                _matrix[i] = new ushort[count];
            }

            for (var row = 0; row < nrows; row++) {
                m.GetBaseVector(MATRIX_DIM.MATRIX_ROW, row, mrow);

                for (var col = 0; col < ncols; col++) {
                    var value = mrow[col] * scale;

                    //mapping doesn't seem to be needed for TransCAD matrices?
                    //if (_mapping.ContainsKey(row+1) && _mapping.ContainsKey(col+1)) {
                    if (value > ushort.MaxValue - 1) {
                        value = ushort.MaxValue - 1;
                    } else if (value < 0) {
                        value = 0;
                    }

                    value = Convert.ToUInt16(value);

                    _matrix[row][col] = (ushort)value;
                    //_matrix[_mapping[row+1]][_mapping[col+1]] = (ushort) value;
                    //}
                }
            }
        }
    }
}
