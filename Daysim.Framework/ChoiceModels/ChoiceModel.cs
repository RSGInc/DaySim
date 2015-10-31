using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.Framework.Coefficients;
using Daysim.Framework.Core;

namespace Daysim.Framework.ChoiceModels {
	public abstract class ChoiceModel : IChoiceModel
	{
		protected ChoiceModelHelper[] _helpers = new ChoiceModelHelper[ParallelUtility.NBatches];
		protected ICoefficientsReader _reader = null;
		public abstract void RunInitialize(ICoefficientsReader reader = null);

		protected void Initialize(string choiceModelName, string coefficientsPath, int totalAlternatives,
		                          int totalNestedAlternatives, int totalLevels, int maxParameter,
		                          ICoefficientsReader reader = null)
		{

			if (coefficientsPath == null)
				return;

			_reader = reader;

			for (int x = 0; x < ParallelUtility.NBatches; x++)
			{
				ChoiceModelHelper.Initialize(ref _helpers[x], choiceModelName,
				                             Global.GetInputPath(coefficientsPath),
				                             totalAlternatives, totalNestedAlternatives, totalLevels, maxParameter, _reader);
			}
		}

		#region IChoiceModel Members

		//public abstract void Run(IHouseholdWrapper household, ICoefficientsReader reader = null);

		#endregion

	}
}
