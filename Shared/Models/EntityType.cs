using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.Entity.Models
{
    /// <summary>
    /// Defines a category of entities (e.g., Vehicle, Property, Person).
    /// Entity Types define the schema through their associated Fields.
    /// </summary>
    [Table("GIBS_EntityType")]
    public class EntityType : ModelBase
    {
        [Key]
        public int EntityTypeId { get; set; }

        [Required]
        public int SiteId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Supports Entity Type inheritance. Child types inherit fields from parent.
        /// </summary>
        public int? ParentEntityTypeId { get; set; }

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// System types cannot be deleted and are used by the framework.
        /// </summary>
        public bool IsSystem { get; set; } = false;

        public int SortOrder { get; set; }

        // Template content moved to GIBS_EntityTemplate (module-scoped)
        // to keep EntityType focused on schema metadata.
    }
}
