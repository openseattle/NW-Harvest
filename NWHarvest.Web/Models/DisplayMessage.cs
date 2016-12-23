using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [Table("DisplayMessage")]
    public partial class DisplayMessage
    {

        [Key]
        public int DisplayMessageId { get; set; }

        public int DisplayDescriptionId { get; set; }

        [Required]
        [Index("DescriptionId_SortOrder_Unique", 2, IsUnique = true)]
        public int SortOrder { get; set; }

        [ForeignKey("DisplayDescriptionId")]
        [Index("DescriptionId_SortOrder_Unique", 1, IsUnique = true)]
        public DisplayDescription DisplayDescription { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }
    }
}
