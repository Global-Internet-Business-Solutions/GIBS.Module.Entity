using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;
using GIBS.Module.Entity.Enums;

namespace GIBS.Module.Entity.Models
{
    /// <summary>
    /// Defines a field within an Entity Type.
    /// Separates data type from editor type for flexibility.
    /// </summary>
    [Table("GIBS_EntityField")]
    public class EntityField : ModelBase
    {
        [Key]
        public int FieldId { get; set; }

        [Required]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Optional field group for organization
        /// </summary>
        public int? FieldGroupId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        [StringLength(200)]
        public string Label { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Determines which typed column in EntityValue will be used
        /// </summary>
        [Required]
        public EntityDataType DataType { get; set; }

        /// <summary>
        /// Determines how the field is rendered in the UI
        /// </summary>
        [Required]
        public EntityEditorType EditorType { get; set; }

        /// <summary>
        /// Maximum length for string values
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Precision for decimal values
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Scale for decimal values
        /// </summary>
        public int? Scale { get; set; }

        [StringLength(500)]
        public string DefaultValue { get; set; }

        [StringLength(200)]
        public string Placeholder { get; set; }

        [StringLength(1000)]
        public string HelpText { get; set; }

        public string HtmlContent { get; set; }

        public bool IsRequired { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsHidden { get; set; }

        /// <summary>
        /// Supports multiple values for this field
        /// </summary>
        public bool IsMultiValue { get; set; }

        /// <summary>
        /// Minimum number of values required if IsMultiValue
        /// </summary>
        public int? MinimumValues { get; set; }

        /// <summary>
        /// Maximum number of values allowed if IsMultiValue
        /// </summary>
        public int? MaximumValues { get; set; }

        /// <summary>
        /// Can be used in search queries
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Can be used as a filter
        /// </summary>
        public bool IsFilterable { get; set; }

        /// <summary>
        /// Can be used for sorting
        /// </summary>
        public bool IsSortable { get; set; }

        /// <summary>
        /// Highlight this field in featured views
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Include this field in list/grid displays
        /// </summary>
        public bool IsListed { get; set; }

        /// <summary>
        /// Restrict this field to manager/admin contexts
        /// </summary>
        public bool IsManagerOnly { get; set; }

        /// <summary>
        /// Hide the field caption/label in rendered UI
        /// </summary>
        public bool IsCaptionHidden { get; set; }

        /// <summary>
        /// Prevent this field from being edited except by system processes
        /// </summary>
        public bool IsLockedDown { get; set; }

        /// <summary>
        /// For EntityLookup editor type - references another Entity Type
        /// </summary>
        public int? ReferencedEntityTypeId { get; set; }

        /// <summary>
        /// JSON configuration for additional validation rules
        /// </summary>
        public string ValidationSettings { get; set; }

        /// <summary>
        /// JSON configuration for editor-specific settings
        /// </summary>
        public string EditorSettings { get; set; }

        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
