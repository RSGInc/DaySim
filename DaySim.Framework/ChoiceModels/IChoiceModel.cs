using DaySim.Framework.Coefficients;

namespace DaySim.Framework.ChoiceModels {
  public interface IChoiceModel {
    void RunInitialize(ICoefficientsReader reader = null);
    //void Run(IHouseholdWrapper household, ICoefficientsReader reader = null);
  }
}
