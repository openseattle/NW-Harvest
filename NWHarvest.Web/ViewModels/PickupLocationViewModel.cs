using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class PickupLocationViewModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        [StringLength(50)]
        public string Name { get; set; }
        public string UserName { get; set; }

        public AddressViewModel Address { get; set; }
        public string Comments { get; set; }
        public ListingViewModel Listing { get; set; }
        public GrowerViewModel Grower { get; set; }
    }
}
