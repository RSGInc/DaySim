using DaySim.Framework.ChoiceModels;
using DaySim.Framework.DomainModels.Wrappers;
using System;

namespace DaySim.ChoiceModels.Default.Models {
    class SFCTA_AutoOwnershipModel : AutoOwnershipModel {
        protected override void RegionSpecificCustomizations(ChoiceProbabilityCalculator.Alternative alternative, IHouseholdWrapper household) {
            //sfcta inserted special fields on the microzne file for residental parking - using unused properties
            double offStreetResidentParkingSpacesPerHH = household.ResidenceParcel.LandUseCode / Math.Max(household.ResidenceParcel.Households, 1);
            double onStreetResidentParkingSpacesPerHH = household.ResidenceParcel.OpenSpaceType1Buffer1 / Math.Max(household.ResidenceParcel.HouseholdsBuffer1, 1);

            //put on each alternative except 3 (2 vehicles), using the alternative number to number coefficient
            if (alternative.Id != 3) {
                alternative.AddUtilityTerm(100 + alternative.Id, Math.Min(offStreetResidentParkingSpacesPerHH, 4));
                alternative.AddUtilityTerm(110 + alternative.Id, Math.Min(onStreetResidentParkingSpacesPerHH, 4));
                alternative.AddUtilityTerm(120 + alternative.Id, (offStreetResidentParkingSpacesPerHH + offStreetResidentParkingSpacesPerHH) > 0 ? 1 : 0);
            }
        }
    }
}
