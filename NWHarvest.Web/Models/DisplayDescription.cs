using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [Table("DisplayDescription")]
    public partial class DisplayDescription
    {

        [Key]
        public int DisplayDescriptionId { get; set; }
        
        [Required]
        [StringLength(30)]
        public string Description { get; set; }
    }
}
