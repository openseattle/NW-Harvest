namespace NWHarvest.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PickupLocation")]
    public partial class PickupLocation
    {
        public int id { get; set; }

        [DisplayName("Title")]
        [StringLength(50)]
        public string name { get; set; }

        [NotMapped]
        public decimal? latitude { get; set; }

        [NotMapped]
        public decimal? longitude { get; set; }

        [DisplayName("Address Line 1")]
        [StringLength(200)]
        public string address1 { get; set; }

        [DisplayName("Address Line 2")]
        [StringLength(200)]
        public string address2 { get; set; }

        [DisplayName("Address Line 3")]
        [StringLength(200)]
        public string address3 { get; set; }

        [DisplayName("Address Line 4")]
        [StringLength(200)]
        public string address4 { get; set; }

        [DisplayName("City")]
        [StringLength(100)]
        public string city { get; set; }

        [DisplayName("State")]
        [StringLength(2)]
        public string state { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "County")]
        public string county { get; set; }

        [DisplayName("Zip")]
        [StringLength(9)]
        public string zip { get; set; }

        [DisplayName("Comments")]
        public string comments { get; set; }

        [DisplayName("Grower")]
        public virtual Grower Grower { get; set; }
    }
}
