using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class PickupLocationEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        [StringLength(50)]
        public string Name { get; set; }

        public AddressEditViewModel Address { get; set; }

        public string Comments { get; set; }
        public GrowerViewModel Grower { get; set; }
    }
}
