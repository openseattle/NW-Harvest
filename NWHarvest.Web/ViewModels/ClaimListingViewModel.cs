using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class ClaimListingViewModel
    {
        [Required]
        [Editable(false)]
        public int ListingId { get; set; }
        public string Product { get; set; }
        [Editable(false)]
        public decimal Available { get; set; }
        [Editable(false)]
        public decimal CostPerUnit { get; set; }
        [Editable(false)]
        public string UnitOfMeasure { get; set; }
        [Editable(false)]
        public string GrowerName { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }

    }
}