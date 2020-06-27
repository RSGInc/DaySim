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

      //JB 20190427 If parking data isn't available we asume person doesn't pay to park
      IActumParcelWrapper workerUsualParcel_x = (IActumParcelWrapper)person.UsualWorkParcel;
      if (workerUsualParcel_x.ParkingDataAvailable == 0) {
        person.PaidParkingAtWorkplace = 0;
        return;
      }

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
      int personalIncome = person.PersonalIncome;
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
      IActumParcelWrapper workerUsualParcel = (IActumParcelWrapper)person.UsualWorkParcel;

      //JB 20190427
      int personalIncomeMissingFlag = personalIncome < 0 ? 1 : 0;
      int personalIncomeUnder300Flag = (personalIncome >= 0 && personalIncome < 300000) ? 1 : 0;
      int personalIncomeOver600Flag = personalIncome >= 600000 ? 1 : 0;
      double personalIncomeInThousands = personalIncomeMissingFlag == 1 ? 0 : personalIncome / 1000;
      int householdIncomeMissingFlag = household.Income < 0 ? 1 : 0;
      double householdIncomeInThousands = householdIncomeMissingFlag == 1 ? 0 : household.Income / 1000;


      //GV: 15.3. introducion CPH muni., Frederiksberg Muni. and CPHcity
      bool workLocationIsInCPHMuni = false;
      //if (person.UsualWorkParcel.LandUseCode == 101) {
      if (workerUsualParcel.LandUseCode == 101) {
        workLocationIsInCPHMuni = true;
      }

      //JB: 20200625
      bool workLocationIsInCPHMuni1 = false;
      bool workLocationIsInCPHMuni2 = false;
      if (workLocationIsInCPHMuni) {
        if (workerUsualParcel.DistrictID == 1) {
          workLocationIsInCPHMuni1 = true;
        } else {
          workLocationIsInCPHMuni2 = true;
        }
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
      double Bf1FreeEmpParkingSpaces = workerUsualParcel.EmployeeOnlyParkingSpacesBuffer1;
      double Bf1FreeEmpParkingSpaces_x = (Math.Max(1.0, workerUsualParcel.EmployeeOnlyParkingSpacesBuffer1));
      double Bf1FreeEmpParkingRatio = Math.Min(1.0, Bf1FreeEmpParkingSpaces / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1.0));
      double Bf1LogFreeEmpParkingRatio = Math.Log(Bf1FreeEmpParkingRatio + 0.001);
      //GV: 15. mar. 2019 - no. of parkig places in Buffer2 area
      //double Bf2NoParking = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailySpacesBuffer2));
      double Bf2NoParking = (Math.Max(1.0, workerUsualParcel.EmployeeOnlyParkingSpacesBuffer2));

      //GV: 13.6.2020 - mail from COH where EmployeeONLY parking places should be without Buffer
      double FreeEmpParkingSpaces = workerUsualParcel.EmployeeOnlyParkingSpaces;


      //GV: 15. mar. 2019 - parkig costs in the workplace area
      //double destParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidDailyPrice));
      //double destParkingCost = (Math.Max(1.0, workerUsualParcel.PublicParkingHourlyPrice));
      double destParkingCost = (Math.Max(1.0, workerUsualParcel.ParkingOffStreetPaidHourlyPrice));

      //GV: 19. mar. 2019 - parkig costs in Buffer1 area
      double Bf1LogParkingPrice = Math.Log(workerUsualParcel.PublicParkingHourlyPriceBuffer1 + 1);
      double Bf1ParkingPrice = workerUsualParcel.PublicParkingHourlyPriceBuffer1;

      //GV: 03. dec. 2019 - new from JB
      //JB 20191202
      double Bf1PubNoResSpaces = workerUsualParcel.PublicNoResidentialPermitAllowedParkingSpacesBuffer1;
      double Bf1PubWithResSpaces = workerUsualParcel.PublicWithResidentialPermitAllowedParkingSpacesBuffer1;


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

      //JB: 20200625 comment out the following three AddUtilityTerm statements for coefficients 3, 4 and 5
      //alternative.AddUtilityTerm(3, person.IsFulltimeWorker.ToFlag());
      //GV: 15.3.2019 - self employed person
      //alternative.AddUtilityTerm(4, (person.OccupationCode == 8).ToFlag());
      //GV: 15.3.2019 - male
      //alternative.AddUtilityTerm(5, (person.IsFullOrPartTimeWorker && person.IsMale).ToFlag());

      //JB: 20200625 split KK coef (coef 4) into two (coef 3 and 4)
      //GV: 16.6.2020 - KK municip.
      //alternative.AddUtilityTerm(4, (workLocationIsInCPHMuni).ToFlag());
      alternative.AddUtilityTerm(3, (workLocationIsInCPHMuni1).ToFlag());
      alternative.AddUtilityTerm(4, (workLocationIsInCPHMuni2).ToFlag());
      //GV: 16.6.2020 - Frederiksberg municipality
      alternative.AddUtilityTerm(5, (workLocationIsInFDBMuni).ToFlag());
      //GV: 12.6.2019 - CPHcity
      alternative.AddUtilityTerm(6, (workLocationIsInCPHcity).ToFlag());
      //GV: 13.6.2019 - rest of GCA
      alternative.AddUtilityTerm(7, (!workLocationIsInCPHcity).ToFlag());

      //GV: 15.3.2019 - income
      //JB 20190427 Try the following variations and use the best one:
      //  v1:  10, 11, 12
      //  v2:  10, 13
      //  v3:  14, 15, 16
      //  v4:  14, 17
      //Note: parms not listed in a version are constrained to zero.
      //Note: use either HH or personal as a group, not both
      //Note:  must use missing inc nuisance parm
      //note:  use either inc categories or linear income variable, not both

      alternative.AddUtilityTerm(10, householdIncomeMissingFlag);
      alternative.AddUtilityTerm(11, (household.Income >= 0 && household.Income < 450000).ToFlag());
      alternative.AddUtilityTerm(12, (household.Income >= 900000).ToFlag());
      alternative.AddUtilityTerm(13, householdIncomeInThousands);

      alternative.AddUtilityTerm(14, personalIncomeMissingFlag);
      alternative.AddUtilityTerm(15, personalIncomeUnder300Flag);
      alternative.AddUtilityTerm(16, personalIncomeOver600Flag);
      alternative.AddUtilityTerm(17, personalIncomeInThousands);


      //GV: 15.3.2019 - parking places for Copenhagen, Frederiksberg, and out in GCA
      //alternative.AddUtilityTerm(13, Bf1NoParking * workLocationIsInCPHMuni.ToFlag());
      //alternative.AddUtilityTerm(14, Bf1NoParking * workLocationIsInFDBMuni.ToFlag());
      //alternative.AddUtilityTerm(15, destNoParking * (!workLocationIsInCPHcity).ToFlag()); //GV: if this included model fails due to 100% correlation wih ParkingCosts  


      //GV: 02.12.2019 
      //Response: Only two price variables are available in the microzone data, and one of them is the daily residential permit price, which is irrelevant.
      //We used the other one, the ‘public’ price variable, which is the weighted average hourly price that includes two categories of parking:
      //Category 2—public with residential permit allowed
      //Category 3—public no residential permit allowed
      //Category 3 includes the high-priced parking that I believe is what you refer to as private.  Category 2 does not.  
      //(We us the term ‘public’ to refer to parking that is available to the public.  
      //I believe you use the term ‘private’ to refer to parking provided by a private company, but which is also available to the public. )  
      //So, the existing price variable captures the effect of very high prices in areas like Ørestad.
      //It may be the case that the price variable fails to completely capture the difference in employer-provided parking between areas that have low-priced parking and those that don’t.
      //Given the available data, one way to try to capture the effect would be to specify the following ‘type of available parking’ variable:
      //(Buffer 1 Category 3 parking spaces available)/(Buffer 1 Categories 2 + 3 parking spaces available)

      //JB 20191202
      //This is the share of publicly available spaces that are in high-priced lots/garages.
      alternative.AddUtilityTerm(18, Bf1PubNoResSpaces / Math.Max(1, (Bf1PubNoResSpaces + Bf1PubWithResSpaces)));


      //GV: 15.3.2019 - parking costs
      //JB 20190427 test separately and use one or the other of the two price terms. beta should be positive
      alternative.AddUtilityTerm(19, Bf1ParkingPrice);
      alternative.AddUtilityTerm(20, Bf1LogParkingPrice);

      //JB 20190427 test separately and use one or the other of the three free parking spaces terms. beta should be negative  
      alternative.AddUtilityTerm(21, Bf1FreeEmpParkingSpaces);

      //alternative.AddUtilityTerm(22, Bf1FreeEmpParkingRatio);
      //alternative.AddUtilityTerm(23, Bf1LogFreeEmpParkingRatio);

      //GV: 13.6.2020 - mail from COH where parking places for Emoloyers should net be Buffered
      //alternative.AddUtilityTerm(21, (FreeEmpParkingSpaces * workLocationIsInCPHcity.ToFlag()));


      //JB: 12.6.20202
      //The second idea:  Add another term to represent some or all of the spaces not included in the beta 21 term(Bf1FreeEmpParkingSpaces).
      //I’m not sure whether it should be a linear form like beta21, or perhaps a log form.  
      //Perhaps if few of these spaces are available the person is more likely to have to pay to park at work.  
      //(PublicNoResidentialPermitAllowedParkingSpacesBuffer1; PublicWithResidentialPermitAllowedParkingSpacesBuffer1; 
      //ResidentialPermitOnlyParkingSpacesBuffer1(probably don’t use this); ElectricVehicleOnlyParkingSpacesBuffer1(perhaps don’t use this)
      //GV: added on 12.6.2020 - see above
      //alternative.AddUtilityTerm(24, workerUsualParcel.PublicNoResidentialPermitAllowedParkingSpacesBuffer1);
      //alternative.AddUtilityTerm(25, workerUsualParcel.PublicWithResidentialPermitAllowedParkingSpacesBuffer1); 

      //GV: 13.6.2020 - mail from COH where Public parking places (type 1+2) should be Buffered and in CPH
      alternative.AddUtilityTerm(24, (Bf1PubNoResSpaces + Bf1PubWithResSpaces) * workLocationIsInCPHcity.ToFlag());

      //GV: 13.6.2020 - mail from COH where Public parking places (type 1+2) should be Buffered and in the rest of GCA
      alternative.AddUtilityTerm(25, (Bf1PubNoResSpaces + Bf1PubWithResSpaces) * (!workLocationIsInCPHcity).ToFlag());


      //GV: 18.3.2019 - 
      //coeff. 26: the more dense is the employment, the more likely is it that the employee will need to pay to park
      alternative.AddUtilityTerm(26, Math.Log(workerUsualParcel.EmploymentTotalBuffer1 + 1.0));
      //coeff. 27: the ratio of paid offstreet parking to employment is a good indicator that employees in that location are requied to pay to park and use those paid locations
      // JB 20190427 coef 23 replaces this
      //alternative.AddUtilityTerm(27, Math.Log((workerUsualParcel.EmployeeOnlyParkingSpacesBuffer1 + 1.0) / (workerUsualParcel.EmploymentTotalBuffer1 + 1.0))); 


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
      alternative.AddUtilityTerm(29, (workerUsualParcel.EmploymentGovernmentBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(30, (workerUsualParcel.EmploymentOfficeBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(31, (workerUsualParcel.EmploymentRetailBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(32, (workerUsualParcel.EmploymentEducationBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(33, (workerUsualParcel.EmploymentIndustrialBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(34, (workerUsualParcel.EmploymentMedicalBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(35, (workerUsualParcel.EmploymentServiceBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      //GV: 18.3.2019 - EmploymentFood and -AgricultureConstruction added 
      alternative.AddUtilityTerm(36, (workerUsualParcel.EmploymentFoodBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(37, (workerUsualParcel.EmploymentAgricultureConstructionBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * Bf1FreeEmpParkingSpaces_x * workLocationIsInCPHcity.ToFlag()));

      //JB 20190427 39-47 are an alternative to try instead of 29-37.  Use one set or the other; don't mix and match
      alternative.AddUtilityTerm(39, (workerUsualParcel.EmploymentGovernmentBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(40, (workerUsualParcel.EmploymentOfficeBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(41, (workerUsualParcel.EmploymentRetailBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(42, (workerUsualParcel.EmploymentEducationBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(43, (workerUsualParcel.EmploymentIndustrialBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(44, (workerUsualParcel.EmploymentMedicalBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(45, (workerUsualParcel.EmploymentServiceBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      //GV: 18.3.2019 - EmploymentFood and -AgricultureConstruction added
      alternative.AddUtilityTerm(46, (workerUsualParcel.EmploymentFoodBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));
      alternative.AddUtilityTerm(47, (workerUsualParcel.EmploymentAgricultureConstructionBuffer1 / Math.Max(workerUsualParcel.EmploymentTotalBuffer1, 1) * workLocationIsInCPHcity.ToFlag()));

    }
  }
}
