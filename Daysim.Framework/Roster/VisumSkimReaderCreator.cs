using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daysim.Framework.Roster
{
    public class VisumSkimReaderCreator : IFileReaderCreator
	{
//		public ISkimFileReader CreateReader(Dictionary<string, List<float[]>> cache, string path, Dictionary<int, int> mapping)
		public ISkimFileReader CreateReader(Dictionary<string, List<double[]>> cache, string path, Dictionary<int, int> mapping)  // 20150703 JLB
		{
			return new VisumSkimReader(path, mapping);
		}
	}
}
