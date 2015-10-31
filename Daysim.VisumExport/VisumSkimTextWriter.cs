using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Daysim.VisumExport {
	public class VisumSkimTextWriter : ITripSkimWriter
	{
		private string _outputPath;
		#region ITripSkimWriter Members

		public VisumSkimTextWriter(string outputPath)
		{
			_outputPath = outputPath;
		}

		public void WriteSkimToFile(double[][] skim, string filename, int[] mapping, int count, int transport, int startTime, int endTime, int factor)
		{
			Directory.CreateDirectory(_outputPath);
			using (TextWriter textWriter = new StreamWriter(_outputPath + filename))
			{
				textWriter.WriteLine("$O;D3");
				textWriter.WriteLine("* From  to");
				//FIX
				string startTimeString = (((double)startTime)/100).ToString("F");
				string endTimeString = (((double)endTime)/100).ToString("F");
				textWriter.WriteLine(startTimeString + " " + endTimeString);
				textWriter.WriteLine("* Factor");
				string factorString = ((double) factor).ToString("F");
				textWriter.WriteLine(factor);
				textWriter.WriteLine("*");
				textWriter.WriteLine("* PTVA INTERNAL");
				string dateString = DateTime.Now.ToShortDateString();
				textWriter.WriteLine("* " + dateString );
				for (int i = 0; i < count; i++)
				{
					for (int j = 0; j < count; j++)
					{
						if (skim[i][j] > 0)
						{
							textWriter.WriteLine(mapping[i] + " " + mapping[j] + " " + skim[i][j]);
						}
					}
				}
			}
		}

		#endregion
	}
}
