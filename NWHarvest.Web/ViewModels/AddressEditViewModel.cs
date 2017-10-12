using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class AddressEditViewModel
    {
        [Display(Name = "Address 1")]
        public virtual string Address1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address 2")]
        public virtual string Address2 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address 3")]
        public virtual string Address3 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address 4")]
        public virtual string Address4 { get; set; }

        [StringLength(100)]
        public virtual string City { get; set; }

        [StringLength(2)]
        public virtual string State { get; set; }

        [StringLength(9)]
        public virtual string Zip { get; set; }
    }
}