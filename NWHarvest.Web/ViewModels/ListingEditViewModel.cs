using System;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class ListingEditViewModel : ListingViewModel
    {
        [Required]
        public override int Id { get; set; }

        [Required]
        public override int PickupLocationId { get; set; }

        [Required]
        public override string Product { get; set; }

        [Required]
        public override decimal QuantityAvailable { get; set; }

        [Required]
        [StringLength(100)]
        public override string UnitOfMeasure { get; set; }

        [Required]
        public override DateTime ExpirationDate { get; set; }

        [Required]
        public override decimal CostPerUnit { get; set; }
    }
}