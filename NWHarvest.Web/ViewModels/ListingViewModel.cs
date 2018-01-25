using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NWHarvest.Web.ViewModels
{
    public class ListingViewModel
    {
        public virtual int Id { get; set; }

        [DisplayName("Pickup Location")]
        public virtual int PickupLocationId { get; set; }

        [Required]
        public virtual string Product { get; set; }

        [Required]
        [DisplayName("Quantity")]
        public virtual decimal QuantityAvailable { get; set; }

        [DisplayName("Quantity Claimed")]
        public decimal QuantityClaimed { get; set; }

        [Required]
        [DisplayName("Unit of Measure")]
        [StringLength(100)]
        public virtual string UnitOfMeasure { get; set; }

        [DisplayName("Harvest")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime HarvestDate { get; set; }

        [DisplayName("Expires")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public virtual DateTime ExpirationDate { get; set; }

        [DisplayName("Cost per Unit")]
        public virtual decimal CostPerUnit { get; set; }

        [DisplayName("Is Available")]
        public bool IsAvailable { get; set; }

        public bool IsPickedUp { get; set; }

        [DisplayName("Comments")]
        public string Comments { get; set; }

        public IEnumerable<SelectListItem> PickupLocations { get; set; }

        public GrowerViewModel Grower { get; set; }
        public FoodBankViewModel FoodBank { get; set; }

        [DisplayName("Grower")]
        public string GrowerName { get; set; }
        public string UserName { get; set; }

        public PickupLocationViewModel PickupLocation { get; set; }
    }
}