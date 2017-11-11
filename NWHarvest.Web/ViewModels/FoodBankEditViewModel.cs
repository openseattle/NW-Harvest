using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class FoodBankEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Food Program")]
        public string Name { get; set; }

        public AddressEditViewModel Address { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}