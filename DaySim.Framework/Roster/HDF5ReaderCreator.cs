// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaySim.Framework.Roster 
{
	public class HDF5ReaderCreator  : IFileReaderCreator
	{
		#region IFileReaderCreator Members

//		public ISkimFileReader CreateReader(Dictionary<string, List<float[]>> cache, string path, Dictionary<int, int> mapping)
		public ISkimFileReader CreateReader(Dictionary<string, List<double[]>> cache, string path, Dictionary<int, int> mapping)  // 20150703 JLB
		{
			return new HDF5SkimFileReader(path, mapping);
		}

		#endregion
	}
}
