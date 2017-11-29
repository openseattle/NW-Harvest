using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [Table("County")]
    public partial class County
    {
        [Key]
        public int Id { get; set; }
       
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int StateId { get; set; }

        public virtual State State { get; set; }
    }
}