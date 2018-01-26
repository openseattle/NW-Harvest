using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class ClaimListingViewModel
    {
        [Required]
        public int ListingId { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        public string Product { get; set; }
        public decimal Available { get; set; }
        public decimal CostPerUnit { get; set; }
        public string UnitOfMeasure { get; set; }
        public string ListerRole { get; set; }
        public string ListerUserId { get; set; }
        public ListerViewModel Lister { get; set; }
    }
}