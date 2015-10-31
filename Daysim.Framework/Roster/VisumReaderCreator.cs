using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daysim.Framework.Roster {
	public class VisumReaderCreator : IFileReaderCreator 
	{
		public ISkimFileReader CreateReader(Dictionary<string, List<float[]>> cache, string path, Dictionary<int, int> mapping)
		{
			return new VisumSkimFileReader(path, mapping);
		}
	}
}
