using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class AddressViewModel
    {
        [Display(Name = "Address 1")]
        public virtual string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public virtual string Address2 { get; set; }

        [Display(Name = "Address 3")]
        public virtual string Address3 { get; set; }

        [Display(Name = "Address 4")]
        public virtual string Address4 { get; set; }

        public virtual string City { get; set; }

        public virtual string State { get; set; }

        public virtual string Zip { get; set; }
    }
}