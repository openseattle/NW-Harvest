using NWHarvest.Web.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class GrowerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [Display(Name = "Notification Preference")]
        public string NotificationPreference { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public AddressViewModel Address { get; set; }
        public ICollection<PickupLocationViewModel> PickupLocations { get; set; }

        public ICollection<ListingViewModel> Listings { get; set; }

        public string UserId { get; set; }
    }
}