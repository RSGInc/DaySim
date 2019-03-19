// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using DaySim.DomainModels.Actum.Wrappers;
using DaySim.DomainModels.Actum.Wrappers.Interfaces;
using DaySim.Framework.ChoiceModels;
using DaySim.Framework.Coefficients;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim.ChoiceModels.Actum.Models {
  public class PayToParkAtWorkplaceModel : ChoiceModel {
    public const string CHOICE_MODEL_NAME = "ActumPayToParkAtWorkplaceModel";
    private const int TOTAL_ALTERNATIVES = 2;
    private const int TOTAL_NESTED_ALTERNATIVES = 0;
    private const int TOTAL_LEVELS = 1;
    private const int MAX_PARAMETER = 99;

    public override void RunInitialize(ICoefficientsReader reader = null) {
      Initialize(CHOICE_MODEL_NAME, Global.Configuration.PayToParkAtWorkplaceModelCoefficients, TOTAL_ALTERNATIVES, TOTAL_NESTED_ALTERNATIVES, TOTAL_LEVELS, MAX_PARAMETER);
    }

    public void Run(PersonWrapper person) {
      if (person == null) {
        throw new ArgumentNullException("person");
      }

      person.ResetRandom(2);

      if (Global.Configuration.IsInEstimationMode) {
        if (!_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
          return;
        }
      }

      ChoiceProbabilityCalculator choiceProbabilityCalculator = _helpers[ParallelUtility.threadLocalAssignedIndex.Value].GetChoiceProbabilityCalculator(person.Id);

      if (_helpers[ParallelUtility.threadLocalAssignedIndex.Value].ModelIsInEstimationMode) {
        if (person.PaidParkingAtWorkplace < 0 || person.PaidParkingAtWorkplace > 1 || person.UsualWorkParcel == null) {
          return;
        }

        RunModel(choiceProbabilityCalculator, person, person.PaidParkingAtWorkplace);

        choiceProbabilityCalculator.WriteObservation();
      } else {
        RunModel(choiceProbabilityCalculator, person);

        ChoiceProbabilityCalculator.Alternative chosenAlternative = choiceProbabilityCalculator.SimulateChoice(person.Household.RandomUtility);
        int choice = (int)chosenAlternative.Choice;

        person.PaidParkingAtWorkplace = choice;
      }
    }

    
    private void RunModel(ChoiceProbabilityCalculator choiceProbabilityCalculator, IPersonWrapper person_x, int choice = Constants.DEFAULT_VALUE) {
      //MB check for access to new Actum person properties
      //requres a changing name in call, and cast to a new variable called person, and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header
      IActumPersonWrapper person = (IActumPersonWrapper)person_x;
      int checkPersInc = person.PersonalIncome;
      //end check

      //MB check for new hh properties
      //requres a cast to a household, and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header
      IActumHouseholdWrapper household = (IActumHouseholdWrapper)person.Household;
      int checkKids6To17 = household.Persons6to17;
      // end check

      //MB check for access to new Actum parcel properties
      //requires a cast and using DaySim.DomainModels.Actum.Wrappers.Interfaces in header - use new variable workerUsualParcel...
      //if (person.UsualWorkParcel != null) {
      //  IActumParcelWrapper workerUsualParcel = (IActumParcelWrapper)person.UsualWorkParcel;
      //  double checkWorkMZParkCost = workerUsualParcel.PublicParkingHourlyPriceBuffer1; 
      //}
      //end check

      //JB: 19.3.2019
      IActumParcelWrapper workerUsualParcel = (person.UsualWorkParcel != null) ?
         (IActumParcelWrapper)person.UsualWorkParcel : null; 
      

      //GV: 15.3. introducion CPH muni., Frederiksberg Muni. and CPHcity
      bool workLocationIsInCPHMuni = false;
      //if (person.UsualWorkParcel.LandUseCode == 101) {
      if (workerUsualParcel.LandUseCode == 101) {
        workLocationIsInCPHMuni = true;
      }

      //GV: 13.3.2019 - added Frederiksberg Mun.
      bool workLocationIsInFDBMuni = false;
      if (workerUsualParcel.LandUseCode == 147) {
        workLocationIsInFDBMuni = true;
      }

      //GV: 13.3.2019 - added CPHcity
      bool workLocationIsInCPHcity = false;
      if (workerUsualParcel.LandUseCode == 101 || workerUsualParcel.LandUseCode == 147) {
        workLocationIsInCPHcity = true;
      }


      //John: 19.3.2019 - "EmployeeOnlyParkingSpaces" (probably buffered) and "PublicParkingHourlyPrice"  

      //GV: 15. mar. 2019 - no. of parkig places in the workplace area
      //double destNoParking = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailySpaces));
      double destNoParking = (Math.Max(1.0, workerUsualParcel.EmployeeOnlyParkingSpaces));

      //GV: 15. mar. 2019 - no. of parkig places in Buffer1 area
      //double Bf1NoParking = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailySpacesBuffer1));
      double Bf1NoParking = (Math.Max(1.0, workerUsualParcel.EmployeeOnlyParkingSpacesBuffer1));

      //GV: 15. mar. 2019 - no. of parkig places in Buffer2 area
      //double Bf2NoParking = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailySpacesBuffer2));
      double Bf2NoParking = (Math.Max(1.0, workerUsualParcel.EmployeeOnlyParkingSpacesBuffer2));

      //GV: 15. mar. 2019 - parkig costs in the workplace area
      //double destParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailyPrice));
      //double destParkingCost = (Math.Max(1.0, workerUsualParcel.PublicParkingHourlyPrice));
      double destParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidHourlyPrice));

      //GV: 19. mar. 2019 - parkig costs in Buffer1 area
      double Bf1ParkingCost = workerUsualParcel.ParkingDataAvailable == 1 ? Math.Log(workerUsualParcel.PublicParkingHourlyPriceBuffer1 + 1) : 0;
      //double Bf1ParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailyPriceBuffer1));
      //double Bf1ParkingCost = (Math.Max(1.0, workerUsualParcel.PublicParkingHourlyPriceBuffer1));
      //double Bf1ParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailyPriceBuffer1));
      //double Bf1ParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidHourlyPriceBuffer1));

      //GV: 15. mar. 2019 - parkig costs in Buffer2 area
      //double Bf2ParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailyPriceBuffer2));
      //double Bf2ParkingCost = (Math.Max(1.0, workerUsualParcel.PublicParkingHourlyPriceBuffer2));
      double Bf2ParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidHourlyPriceBuffer2));


      // 0 No paid parking at work

      ChoiceProbabilityCalculator.Alternative alternative = choiceProbabilityCalculator.GetAlternative(0, true, choice == 0);

      alternative.Choice = 0;

      alternative.AddUtilityTerm(1, 0.0);

      // 1 Paid parking at work

      alternative = choiceProbabilityCalculator.GetAlternative(1, true, choice == 1);

      alternative.Choice = 1;

      alternative.AddUtilityTerm(1, 1.0);
      alternative.AddUtilityTerm(2, person.IsPartTimeWorker.ToFlag());
      //GV: 15.3.2019 - FTW instead
      //alternative.AddUtilityTerm(3, person.IsNotFullOrPartTimeWorker.ToFlag());
      alternative.AddUtilityTerm(3, person.IsFulltimeWorker.ToFlag());
      //GV: 15.3.2019 - self employed person
      alternative.AddUtilityTerm(4, (person.OccupationCode == 8).ToFlag());
      //GV: 15.3.2019 - male
      alternative.AddUtilityTerm(5, (person.IsFullOrPartTimeWorker && person.IsMale).ToFlag());

      //GV: 15.3.2019 - income
      //alternative.AddUtilityTerm(14, Math.Max(1, person.Household.Income) / 1000.0);
      //alternative.AddUtilityTerm(15, person.Household.HasMissingIncome.ToFlag());
      //GV: 20. feb. 2019 - income
      //JB: 20190224 Goran, Stefan's spec already has income effects that confound with these variables.  
      alternative.AddUtilityTerm(11, (household.Income >= 450000 && household.Income < 900000).ToFlag());
      alternative.AddUtilityTerm(12, (household.Income >= 900000).ToFlag());

      //GV: 15.3.2019 - parking places for Copenhagen, Frederiksberg, and out in GCA
      alternative.AddUtilityTerm(13, Bf1NoParking * workLocationIsInCPHMuni.ToFlag());
      alternative.AddUtilityTerm(14, Bf1NoParking * workLocationIsInFDBMuni.ToFlag());
      //alternative.AddUtilityTerm(15, destNoParking * (!workLocationIsInCPHcity).ToFlag()); //GV: if this included model fails due to 100% correlation wih ParkingCosts  

      //GV: 15.3.2019 - parking costs 
      alternative.AddUtilityTerm(19, Bf1ParkingCost);
      
      //GV: 18.3.2019 - 
      //coeff. 26: the more dense is the employment, the more likely is it that the employee will need to pay to park
      alternative.AddUtilityTerm(26, Math.Log(workerUsualParcel.EmploymentTotalBuffer1 + 1.0));
      //coeff. 27: the ratio of paid offstreet parking to employment is a good indicator that employees in that location are requied to pay to park and use those paid locations
      alternative.AddUtilityTerm(27, Math.Log((workerUsualParcel.EmployeeOnlyParkingSpacesBuffer1 + 1.0) / (workerUsualParcel.EmploymentTotalBuffer1 + 1.0))); 


      // GV. 14.3.2019 Employment types from JBs Buffered Microzone file  
      // Name: 1EmploymentEducation                  Explanation: Education and kindergarten
      // Name: EmploymentFood                       Explanation: Restaurants, cinema, sport, etc
      // Name: 1EmploymentGovernment                 Explanation: Public office
      // Name: 1EmploymentIndustrial                  Explanation: Industrial, transport, auto service, wholesale
      // Name: 1EmploymentMedical                    Explanation: Health, wellness and personal service
      // Name: 1EmploymentOffice                     Explanation: Private office
      // Name: 1EmploymentRetail                     Explanation: Retail
      // Name: 1EmploymentService                    Explanation: Supermarket, grocery, etc
      // Name: EmploymentAgricultureConstruction    Explanation: Agriculture, resources, construction  
      // Name: EmploymentTotal

      //GV: OBS! CPHcity ONLY
      alternative.AddUtilityTerm(29, (workerUsualParcel.EmploymentGovernmentBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(30, (workerUsualParcel.EmploymentOfficeBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(31, (workerUsualParcel.EmploymentRetailBuffer1  / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(32, (workerUsualParcel.EmploymentEducationBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(33, (workerUsualParcel.EmploymentIndustrialBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(34, (workerUsualParcel.EmploymentMedicalBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(35, (workerUsualParcel.EmploymentServiceBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      //GV: 18.3.2019 - EmploymentFood and -AgricultureConstruction added
      alternative.AddUtilityTerm(36, (workerUsualParcel.EmploymentFoodBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(37, (workerUsualParcel.EmploymentAgricultureConstructionBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1NoParking * workLocationIsInCPHcity.ToFlag()));
      

    }
  }
}
