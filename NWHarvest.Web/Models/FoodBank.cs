using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{

    [Table("FoodBank")]
    public partial class FoodBank
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Food Program")]
        public string name { get; set; }

        [StringLength(11)]
        [Display(Name = "Phone Number")]
        public string phone { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Address_1")]
        public string address1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address_2")]
        public string address2 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address_3")]
        public string address3 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address_4")]
        public string address4 { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string city { get; set; }

        [Required]
        [StringLength(2)]
        [Display(Name = "State")]
        public string state { get; set; }

        [Required]
        [StringLength(9)]
        [Display(Name = "Zip")]
        public string zip { get; set; }

        [Required]
        public string NotificationPreference { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
