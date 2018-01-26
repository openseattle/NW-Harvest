using NWHarvest.Web.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [Table("Listing")]
    public class Listing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Product { get; set; }

        [Required]
        public decimal QuantityAvailable { get; set; }

        [Required]
        public decimal QuantityClaimed { get; set; }

        [Required]
        [StringLength(100)]
        public string UnitOfMeasure { get; set; }

        [Required]
        public DateTime HarvestedDate { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public decimal CostPerUnit { get; set; }

        public bool IsAvailable { get; set; }
        public string Comments { get; set; }
        public int PickupLocationId { get; set; }

        [Required]
        public bool IsPickedUp { get; set; }

        public string ListerRole { get; set; }

        [Required]
        [ForeignKey("User")]
        public string ListerUserId { get; set; }
        public ApplicationUser User { get; set; }
        public PickupLocation PickupLocation { get; set; }

        // Navigation properties
        [Obsolete]
        public int? GrowerId { get; set; }
        [Obsolete]
        public Grower Grower { get; set; }
        [Obsolete]
        public int? FoodBankId { get; set; }
        [Obsolete]
        public FoodBank FoodBank { get; set; }
    }
}