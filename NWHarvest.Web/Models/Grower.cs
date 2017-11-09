using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{

    [Table("Grower")]
    public partial class Grower
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Grower()
        {
            PickupLocations = new HashSet<PickupLocation>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Grower")]
        [StringLength(100)]
        public string name { get; set; }

        [Required]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string email { get; set; }

        [Required]
        [Display(Name = "Address1")]
        [StringLength(200)]
        public string address1 { get; set; }

        [Display(Name = "Address2")]
        [StringLength(200)]
        public string address2 { get; set; }

        [Display(Name = "Address3")]
        [StringLength(200)]
        public string address3 { get; set; }

        [Display(Name = "Addres 4")]
        [StringLength(200)]
        public string address4 { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(100)]
        public string city { get; set; }

        [Required]
        [Display(Name = "State")]
        [StringLength(2)]
        public string state { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "County")]
        public string county { get; set; }

        [Required]
        [Display(Name = "Zip")]
        [StringLength(9)]
        public string zip { get; set; }

        [Required]
        [Display(Name = "Notification Preference")]
        public string NotificationPreference { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PickupLocation> PickupLocations { get; set; }
    }
}
