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

        [Display(Name = "FoodBanks")]
        public int NumberOfFoodBanks { get; set; }

        [Display(Name = "Listings")]
        public int NumberOfListings { get; set; }

        [Display(Name = "Available Listings")]
        public int NumberOfAvailableListings { get; set; }

        [Display(Name = "Pending Pickups")]
        public int NumberOfPendingPickupClaimListings { get; set; }

        [Display(Name = "Claimed Listings")]
        public int NumberOfClaimedListings { get; set; }

        [Display(Name = "Expired Listings")]
        public int NumberOfExpiredListings { get; set; }
        
        public ICollection<ListingViewModel> Listings { get; set; }
    }
}