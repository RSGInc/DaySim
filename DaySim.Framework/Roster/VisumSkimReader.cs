using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DaySim.Framework.Core;

namespace DaySim.Framework.Roster {
  internal class VisumSkimReader : ISkimFileReader {
    private readonly string _path;
    private readonly Dictionary<int, int> _mapping;

    public VisumSkimReader(string path, Dictionary<int, int> mapping) {
      _path = path;
      _mapping = mapping;
    }

    public SkimMatrix Read(string filename, int field, float scale) {
      int count = _mapping.Count;
      ushort[][] matrix = new ushort[count][];

      for (int i = 0; i < count; i++) {
        matrix[i] = new ushort[count];
      }

      if (filename == "null") {
        return new SkimMatrix(matrix);
      }

      FileInfo file = new FileInfo(Path.Combine(_path, filename));

      Console.WriteLine("Loading skim file: {0}, field: {1}.", file.Name, field);
      Global.PrintFile.WriteFileInfo(file, true);

      if (!file.Exists) {
        throw new FileNotFoundException(string.Format("The skim file {0} could not be found.", file.FullName));
      }


      using (BinaryReader b = new BinaryReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        //var position = 0L;
        long length = b.BaseStream.Length;

        //Length of file ID...
        int k = b.ReadInt16();
        string idv = new string(b.ReadChars(k));
        //Print the file ID ... $BK, $BI etc.
        //Get header length...
        int l = b.ReadInt16();
        //Read file header... (bunch of characters)
        string header = new string(b.ReadChars(l));
        //Print the header...
        //Transport value
        int transportValue = b.ReadInt32();
        //Start time
        float startTime = b.ReadSingle();
        //End time
        float endTime = b.ReadSingle();
        //Factor
        float factor = b.ReadSingle();
        //Rows
        int rows = b.ReadInt32();
        //Data type
        short dataType = b.ReadInt16();
        //Check rounding procedure
        byte d = b.ReadByte();
        if (d == '\x01') {
        } else if (d == '\x00') {
        } else {
          Console.WriteLine("Binary flag is missing!");
        }
        int[] zonenums;
        int[] zonenumscol;

        if (idv == "$BI") {   //Matrix type - BI 
          zonenums = new int[rows];
          for (int z = 0; z < rows; z++) {
            zonenums[z] = b.ReadInt32();
          }
        } else {   // Matrix type not BI.. 
                   //Number of columns
          int colnumber = b.ReadInt32();
          zonenums = new int[rows];
          //Read zone numbers
          for (int z = 0; z < rows; z++) {
            zonenums[z] = b.ReadInt32();
            //Console.WriteLine(zonenums[z].ToString());
          }
          //Read zone numbers on columns
          zonenumscol = new int[rows];
          for (int z = 0; z < rows; z++) {
            zonenumscol[z] = b.ReadInt32();
            //Console.WriteLine(zonenumscol[z].ToString());
          }
          if (idv == "$BK") {   //Read the row and column names...
            int itemleng;
            string zonename;
            for (int z = 0; z < colnumber * 2; z++) {
              itemleng = b.ReadInt32();
              //Console.WriteLine(itemleng.ToString());
              if (itemleng > 0) {
                zonename = new string(b.ReadChars(itemleng * 2));
                //Console.WriteLine(zonename);

              }
            }
          }

        }
        //Check if all values are zero (null matrix)
        d = b.ReadByte();

        if (d == '\x01') {
          //Console.WriteLine("This is a zero matrix!");
        } else if (d == '\x00') {
          double diagsum = b.ReadDouble();
          Console.WriteLine("Diagonal sum: " + diagsum.ToString());
          //Variable for compressed length 
          int compresslength;

          for (int vl = 0; vl < rows; vl++) {
            //Get length of compressed bytes
            compresslength = b.ReadInt32();
            //Create memory streams for reading the zipped data and to store inflated data
            using (MemoryStream inStream = new MemoryStream(b.ReadBytes(compresslength)))
            using (MemoryStream decompressedFileStream = new MemoryStream())
            using (DeflateStream decompressionStream = new DeflateStream(inStream, CompressionMode.Decompress)) {
              //Why?
              //Because DelfateStream is same as zlib but zlib has two extra bytes as headers so we pop those                                       
              inStream.ReadByte();
              inStream.ReadByte();

              decompressionStream.CopyTo(decompressedFileStream);
              decompressedFileStream.Position = 0;
              BinaryReader sr = new BinaryReader(decompressedFileStream);
              //Console.Write("Matrix values for row " + vl.ToString() + " >> ");
              for (int vi = 0; vi < rows; vi++) {
                double rawValue = sr.ReadDouble() * scale;

                if (vi == vl && rawValue / scale > 999990) {
                  rawValue = 0;
                }

                if (rawValue > ushort.MaxValue - 1) {
                  rawValue = ushort.MaxValue - 1;
                } else if (rawValue < 0) {
                  rawValue = 0;
                }

                ushort value = Convert.ToUInt16(rawValue);

                matrix[_mapping[zonenums[vl]]][_mapping[zonenums[vi]]] = value;
                //matrix[vl][vi] = value;
                //Console.Write(sr.ReadDouble().ToString());
                //Console.Write(",");
              }
              //Console.WriteLine();
              inStream.Dispose();
              decompressedFileStream.Dispose();
              decompressionStream.Dispose();
              sr.Dispose();
            }

            //Get the Row and Column totals.
            //b.ReadDouble().ToString();
            //b.ReadDouble().ToString();
            double rowSum = b.ReadDouble();
            double columnSum = b.ReadDouble();
          }
        }
      }

      SkimMatrix skimMatrix = new SkimMatrix(matrix);

      return skimMatrix;
    }
  }
}
