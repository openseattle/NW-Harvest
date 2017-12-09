using System;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public virtual DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
    }
}
