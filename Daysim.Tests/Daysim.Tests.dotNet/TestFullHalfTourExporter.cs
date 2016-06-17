using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.Framework.Persistence;

namespace Daysim.Tests {
	public class TestFullHalfTourExporter : Exporter<FullHalfTour>
	{
		public bool HasWritten { get; set; }

		public override void WriteModel(StreamWriter writer, FullHalfTour model, char delimiter)
		{
			HasWritten = true;
		}
	}
}
