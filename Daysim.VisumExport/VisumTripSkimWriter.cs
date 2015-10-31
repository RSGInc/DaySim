using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Daysim.Framework.Core;
using Daysim.Framework.Roster;
using Ionic.Zlib;

namespace Daysim
{
	public class VisumTripSkimWriter : ITripSkimWriter
	{
		#region ITripSkimWriter Members


		private double GetDiagonalSum(double[][] skim)
		{
			return skim.Select((t, i) => t[i]).Sum();
		}

		private double GetColumnSums(double[][] skim)
		{
			return skim.Sum(t1 => skim.Select((t, j) => t1[j]).Sum());
		}

		private double GetColumnSum(double[][] skim, int column)
		{
			return skim.Sum(t => t[column]);
		}

		private double GetRowSum(double[][] skim, int row)
		{
			return skim[row].Sum();
		}

		public void WriteSkimToFile(double[][] skim, string filename, int[] mapping, int count, int transport, int startTime,
		                            int endTime, int factor)
		{
			var file = new FileInfo(filename);
			using (var writer = new BinaryWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
			{
				//var position = 0L;
				double colSum = GetColumnSums(skim);
				double diagonalSum = GetDiagonalSum(skim);

				string header = "\nMuuli-Matrix im gepackten Binary format.\nBezirke: " +
				                count + " \nVarTyp: 5 \nGesamtsumme: " +
				                colSum + " \nDiagonalsumme: " +
				                diagonalSum + " \nVMittel: " + transport +
				                " \nvon: " + startTime + " \nbis: " +
				                endTime + " \nFaktor: " + factor + " \n";
				short headerlength = (short) header.Length;

				int dataType = 5;
				int roundProc = 1;

				//write header info
				writer.Write((short) 3);
				writer.Write("$BI".ToCharArray());
				writer.Write((short) headerlength);
				writer.Write(header.ToCharArray());

				//write additional header info
				writer.Write((Int32) transport);
				writer.Write((float) startTime);
				writer.Write((float) endTime);
				writer.Write((float) factor);
				writer.Write((Int32) count);
				writer.Write((short) dataType);

				writer.Write(roundProc == 1 ? '\x01' : '\x00');

				for (int x = 0; x < count; x++)
				{
					writer.Write((Int32) mapping[x]);
				}

				for (int x = 0; x < count; x++)
				{
					writer.Write((Int32) mapping[x]);
				}

				writer.Write('\x00');
				writer.Write(diagonalSum);

				for (int i = 0; i < count; i++)
				{
					MemoryStream toCompressFileStream = new MemoryStream();
					BinaryWriter compressedWriter = new BinaryWriter(toCompressFileStream);
					for (int j = 0; j < count; j++)
						compressedWriter.Write(skim[i][j]);
					compressedWriter.Flush();

					int outputSize = sizeof (double)*count;
					byte[] output = new Byte[outputSize];
					using (MemoryStream ms = new MemoryStream())
					{
						ZlibCodec compressor = new ZlibCodec();
						compressor.InitializeDeflate(Ionic.Zlib.CompressionLevel.None, true);

						compressor.InputBuffer = toCompressFileStream.ToArray();
						compressor.AvailableBytesIn = (int) toCompressFileStream.Length;
						compressor.NextIn = 0;
						compressor.OutputBuffer = output;

						foreach (var f in new FlushType[] {FlushType.None, FlushType.Finish})
						{
							int bytesToWrite = 0;
							do
							{
								compressor.AvailableBytesOut = outputSize;
								compressor.NextOut = 0;
								compressor.Deflate(f);

								bytesToWrite = outputSize - compressor.AvailableBytesOut;
								if (bytesToWrite > 0)
									ms.Write(output, 0, bytesToWrite);
							} while ((f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
							         (f == FlushType.Finish && bytesToWrite != 0));
						}

						compressor.EndDeflate();

						ms.Flush();
						writer.Write((Int32) ms.Length);
						writer.Write(ms.ToArray());
						writer.Write(GetRowSum(skim, i));
						writer.Write(GetColumnSum(skim, i));
					}
				}
			}
		}

		#endregion
	}

}