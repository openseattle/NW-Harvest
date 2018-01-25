using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class FoodBankViewModel
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Food Program")]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public AddressViewModel Address { get; set; }

        [Required]
        public string NotificationPreference { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public ICollection<ListingViewModel> AvailableListings { get; set; }
        public ICollection<ListingViewModel> MyListings { get; set; }
        public ICollection<ClaimViewModel> Claims { get; set; }
    }
}