namespace NWHarvest.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Listing")]
    public partial class Listing
    {
        public Listing()
        {

        }

        public int id { get; set; }
        
        [Required]
        [DisplayName("Product")]
        public string product { get; set; }

        [Required]
        [DisplayName("Qty")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public decimal? qtyOffered { get; set; }

        [DisplayName("Quantity Claimed")]
        [DisplayFormat(DataFormatString = "{0:n0}", ApplyFormatInEditMode = true)]
        public decimal? qtyClaimed { get; set; }

        [Required]
        [DisplayName("UoM")]
        [StringLength(100)]
        public string qtyLabel { get; set; }

        [Column(TypeName ="date")]
        [DisplayName("Harvested Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? harvested_date { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [DisplayName("Expiration Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? expire_date { get; set; }

        [Required]
        [DisplayName("Unit Cost")]
        public decimal? cost { get; set; }

        [DisplayName("Claimed")]
        public bool? available { get; set; }

        [DisplayName("Schedule Pickup")]
        public string comments { get; set; }

        [DisplayName("Location")]
        public string location { get; set; }

        [DisplayName("Grower")]
        public virtual Grower Grower { get; set; }

        [DisplayName("FoodBank")]
        public virtual FoodBank FoodBank { get; set; }

        [DisplayName("Pickup Location")]
        public virtual PickupLocation PickupLocation { get; set; }

        [Required]
        [DisplayName("Picked Up")]
        public bool IsPickedUp { get; set; }
    }
}
