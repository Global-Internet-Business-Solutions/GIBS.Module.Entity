using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.Entity.Models
{
    /// <summary>
    /// Defines options for select-style editors (Dropdown, RadioList, etc.)
    /// References the option by stable value/key rather than display text.
    /// </summary>
    [Table("GIBS_EntityFieldOption")]
    public class EntityFieldOption : ModelBase
    {
        [Key]
        public int FieldOptionId { get; set; }

        [Required]
        public int FieldId { get; set; }

        [Required]
        [StringLength(200)]
        public string DisplayText { get; set; }

        [Required]
        [StringLength(100)]
        public string Value { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        public int SortOrder { get; set; }

        public bool IsDefault { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
