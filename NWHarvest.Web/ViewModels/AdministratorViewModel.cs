using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class AdministratorViewModel
    {
        [Display(Name = "Users")]
        public int NumberOfUsers {
            get => NumberOfGrowers + NumberOfFoodBanks;
        }

        [Display(Name = "Growers")]
        public int NumberOfGrowers { get; set; }
        public int NumberOfGrowersWeekToDate { get; set; }
        public int NumberOfGrowersMonthToDate { get; set; }
        public int NumberOfGrowersYearToDate { get; set; }

        [Display(Name = "FoodBanks")]
        public int NumberOfFoodBanks { get; set; }
        public int NumberOfFoodBanksWeekToDate { get; set; }
        public int NumberOfFoodBanksMonthToDate { get; set; }
        public int NumberOfFoodBanksYearToDate { get; set; }

        [Display(Name = "Listings")]
        public int NumberOfListings { get; set; }

        [Display(Name = "Available Listings")]
        public int NumberOfAvailableListings { get; set; }

        [Obsolete]
        [Display(Name = "Pending Pickups")]
        public int NumberOfPendingPickupClaimListings { get; set; }

        [Display(Name = "Claimed Listings")]
        public int NumberOfClaimedListings { get; set; }

        [Display(Name = "Partially Claimed Listings")]
        public int NumberOfPartiallyClaimedListings { get; set; }

        [Display(Name = "Unavailable Listings")]
        public int NumberOfUnavailableListings { get; set; }
        
        public ICollection<ListingViewModel> Listings { get; set; }
    }
}