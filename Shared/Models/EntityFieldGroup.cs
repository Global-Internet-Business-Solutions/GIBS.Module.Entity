using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.Entity.Models
{
    /// <summary>
    /// Organizes fields into logical groups for better UI organization.
    /// Examples: "Basic Information", "Vehicle Details", "Pricing"
    /// </summary>
    [Table("GIBS_EntityFieldGroup")]
    public class EntityFieldGroup : ModelBase
    {
        [Key]
        public int FieldGroupId { get; set; }

        [Required]
        public int EntityTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
