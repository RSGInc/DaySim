using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.DomainModels.Default.Models;
using Daysim.Framework.Persistence;

namespace Daysim.Tests {
	public class TestParcelNodeReader : Reader<ParcelNode>
	{
		private List<ParcelNode> _nodes;
		public TestParcelNodeReader()
		{
			_nodes = new List<ParcelNode>();
		}

		public override IEnumerator<ParcelNode> GetEnumerator()
		{
			return _nodes.GetEnumerator();
		}

		public void AddParcelNode(ParcelNode parcelNode)
		{
			_nodes.Add(parcelNode);
		}

		public override void Dispose() 
		{
			

		}

		protected override void Dispose(bool disposing) 
		{
			
		}
	}
}
