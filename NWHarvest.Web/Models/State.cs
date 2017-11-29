using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [Table("State")]
    public partial class State
    {
        public State()
        {
            this.Counties = new HashSet<County>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(2)]
        public string ShortName { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<County> Counties { get; set; }
    }
}