using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NWHarvest.Web.Models;

namespace NWHarvest.Web.ViewModels
{
    public class AdministratorListingViewModel
    {
        public virtual int Id { get; set; }
        
        [Required]
        public virtual string Product { get; set; }

        [Required]
        [DisplayName("Available Qty")]
        public virtual decimal AvailableQuantity { get; set; }

        [DisplayName("Claimed Qty")]
        public decimal ClaimedQuantity { get; set; }
        
        [DisplayName("Expires")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public virtual DateTime ExpirationDate { get; set; }
                
        public Grower Grower { get; set; }
        public FoodBank FoodBank { get; set; }
        public ListerViewModel Lister { get; set; }
    }
}