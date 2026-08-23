using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.Entity.Models
{
    [Table("GIBSEntity")]
    public class Entity : ModelBase
    {
        [Key]
        public int EntityId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}
