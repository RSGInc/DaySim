using System;
using System.Collections.Generic;
using System.Threading;
using Daysim.ChoiceModels;
using Daysim.ChoiceModels.Default;
using Daysim.ChoiceModels.Default.Models;
using Daysim.DomainModels;
using Daysim.DomainModels.Default;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Models;
using Xunit;

namespace Daysim.Tests 
{
	public class AutoOwnershipModelTest 
	{
		[Fact]
		public void TestAutoOwnershipModel()
		{
			Global.Configuration = new Configuration {NProcessors = 1};
			ParallelUtility.Init();
			Global.Configuration.AutoOwnershipModelCoefficients = "c:\\a.txt";
			ParallelUtility.Register(Thread.CurrentThread.ManagedThreadId, 0);
			List<IPerson> persons = new List<IPerson>{new Person()};
			CondensedParcel residenceParcel = new CondensedParcel();
			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons, residenceParcel: residenceParcel);
			household.Init();
			AutoOwnershipModel model = new AutoOwnershipModel();
			model.RunInitialize(new TestCoefficientsReader());
			model.Run(household);

		}

		[Fact]
		public void TestAutoOwnershipModelNullHouseholdException()
		{
			Global.Configuration = new Configuration {NProcessors = 1};
			ParallelUtility.Init();
			Global.Configuration.AutoOwnershipModelCoefficients = "c:\\a.txt";
			ParallelUtility.Register(Thread.CurrentThread.ManagedThreadId, 0);
			List<IPerson> persons = new List<IPerson>{new Person()};
			CondensedParcel residenceParcel = new CondensedParcel();
			HouseholdWrapper household = TestHelper.GetHouseholdWrapper(persons, residenceParcel: residenceParcel);
			household.Init();
			AutoOwnershipModel model = new AutoOwnershipModel();
			model.RunInitialize(new TestCoefficientsReader());
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => model.Run(null));

			Assert.Equal("Value cannot be null.\r\nParameter name: household", ex.Message);

		}
	}

	public class TestCoefficientsReader : CoefficientsReader
	{
		public override ICoefficient[] Read(string path, out string title,
		                                   out ICoefficient sizeFunctionMultiplier,
		                                   out ICoefficient nestCoefficient)
		{
			title = null;
			sizeFunctionMultiplier = null;
			nestCoefficient = null;
			return new Coefficient[100];
		}
	}
}
