using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class GrowerEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public AddressEditViewModel Address { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Required]
        public string NotificationPreference { get; set; }
    }
}