using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Persisters;
using Daysim.Framework.Persistence;

namespace Daysim.Tests {
	public class TestPartialHalfTourExporter : Exporter<PartialHalfTour>
	{
		public override void WriteModel(System.IO.StreamWriter writer, PartialHalfTour model, char delimiter)
		{
			HasWritten = true;
		}

		public bool HasWritten { get; set; }
	}


	public class TestPersisterWithHdf5 : PersisterWithHDF5<PartialHalfTour>
	{
		public TestPersisterWithHdf5() : base(new TestPartialHalfTourExporter())
		{
		}

		public bool HasWritten
		{
			get { return (_exporter as TestPartialHalfTourExporter).HasWritten; }
		}
	}
}
