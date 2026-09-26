#nullable enable

using Oqtane.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIBS.Module.Entity.Models
{
    [Table("GIBS_EntityValue")]
    public class EntityValue : ModelBase
    {
        // =========================================================
        // IDENTITY
        // =========================================================

        [Key]
        public int EntityValueId { get; set; }


        // =========================================================
        // RELATIONSHIPS
        // =========================================================

        public int EntityId { get; set; }

        public int FieldId { get; set; }

        public int ValueIndex { get; set; } = 0;


        // =========================================================
        // TYPED VALUE COLUMNS
        // =========================================================

        public string? TextValue { get; set; }

        public int? IntegerValue { get; set; }

        public long? LongValue { get; set; }

        public decimal? DecimalValue { get; set; }

        public bool? BooleanValue { get; set; }

        public DateTime? DateValue { get; set; }

        public DateTime? DateTimeValue { get; set; }

        public Guid? GuidValue { get; set; }

        public int? ReferencedEntityId { get; set; }


        // =========================================================
        // NAVIGATION PROPERTIES
        // =========================================================

        public Entity? Entity { get; set; }

        public EntityField? EntityField { get; set; }

        public Entity? ReferencedEntity { get; set; }
    }
}
