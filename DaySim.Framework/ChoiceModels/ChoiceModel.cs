using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;

namespace DaySim.Framework.ChoiceModels {
  public abstract class ChoiceModel : IChoiceModel {
    protected ChoiceModelHelper[] _helpers = new ChoiceModelHelper[ParallelUtility.NThreads];
    protected ICoefficientsReader _reader = null;
    public abstract void RunInitialize(ICoefficientsReader reader = null);

    protected void Initialize(string choiceModelName, string coefficientsPath, int totalAlternatives,
                              int totalNestedAlternatives, int totalLevels, int maxParameter,
                              ICoefficientsReader reader = null) {

      if (coefficientsPath == null) {
        return;
      }

      _reader = reader;

      for (int x = 0; x < ParallelUtility.NThreads; x++) {
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
