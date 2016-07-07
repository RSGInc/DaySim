using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.DomainModels.Default.Models;
using Daysim.Framework.Persistence;

namespace Daysim.Tests {
	public class TestJointTourExporter : Exporter<JointTour>
	{
		public override void WriteModel(System.IO.StreamWriter writer, JointTour model, char delimiter)
		{
			HasWritten = true;
		}

		public bool HasWritten { get; set; }
	}
}
