using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [ComplexType]
    public class Address
    {
        [StringLength(200)]
        public string Address1 { get; set; }

        [StringLength(200)]
        public string Address2 { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }

        [Required]
        [StringLength(50)]
        public string County { get; set; }

        [StringLength(9)]
        public string Zip { get; set; }
    }
}
