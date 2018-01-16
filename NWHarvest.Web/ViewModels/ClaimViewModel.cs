using System;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class ClaimViewModel
    {
        [Display(Name="Date")]
        [DisplayFormat(DataFormatString ="{0:MM/dd/yyyy}")]
        public DateTime ClaimedOn { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal CostPerUnit { get; set; }

        public int GrowerId { get; set; }
        public int FoodBankId { get; set; }

        public AddressViewModel Address { get; set; }
        public GrowerViewModel Grower { get; set; }
    }
}