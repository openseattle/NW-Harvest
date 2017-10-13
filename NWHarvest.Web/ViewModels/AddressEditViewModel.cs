using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class AddressEditViewModel : AddressViewModel
    {
        [Required]
        public override string Address1 { get; set; }

        [StringLength(200)]
        public override string Address2 { get; set; }

        [StringLength(200)]
        public override string Address3 { get; set; }

        [StringLength(200)]
        public override string Address4 { get; set; }

        [Required]
        [StringLength(100)]
        public override string City { get; set; }

        [Required]
        [StringLength(2)]
        public override string State { get; set; }

        [Required]
        [StringLength(9)]
        public override string Zip { get; set; }
    }
}