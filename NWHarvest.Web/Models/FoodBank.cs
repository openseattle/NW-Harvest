using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{

    [Table("FoodBank")]
    public partial class FoodBank : BaseEntity
    {
        [Key]
        public override int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Food Program")]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Address1 { get; set; }

        [StringLength(200)]
        public string Address2 { get; set; }

        [StringLength(200)]
        public string Address3 { get; set; }

        [StringLength(200)]
        public string Address4 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(2)]
        public string State { get; set; }

        [Required]
        [StringLength(50)]
        public string County { get; set; }

        [Required]
        [StringLength(9)]
        public string Zip { get; set; }

        [Required]
        public string NotificationPreference { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public ICollection<PickupLocation> PickupLocations { get; set; }
    }
}
