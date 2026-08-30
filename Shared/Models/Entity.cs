using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIBS.Module.Entity.Models
{
    [Table("GIBS_Entity")]
    public class Entity : ModelBase
    {

        // =========================================================
        // IDENTITY
        // =========================================================
        [Key]
        public int EntityId { get; set; }


        // =========================================================
        // MULTI-TENANCY / OWNERSHIP
        // =========================================================

        public int SiteId { get; set; }

        public int ModuleId { get; set; }

        public int EntityTypeId { get; set; }


        // =========================================================
        // ENTITY HIERARCHY
        // =========================================================

        public int? ParentEntityId { get; set; }


        // =========================================================
        // CORE IDENTIFICATION
        // =========================================================

        public string Name { get; set; } = string.Empty;

        public string? Key { get; set; }

        public string? Description { get; set; }


        // =========================================================
        // STATE / LIFECYCLE
        // =========================================================

        public string? Status { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool IsFeatured { get; set; }


        // =========================================================
        // PUBLICATION
        // =========================================================

        public bool IsPublished { get; set; }

        public DateTime? PublishStartDate { get; set; }

        public DateTime? PublishEndDate { get; set; }


        // =========================================================
        // ORGANIZATION
        // =========================================================

        public int SortOrder { get; set; }


        // =========================================================
        // EXTENSIBILITY
        // =========================================================

        public string? Settings { get; set; }


        // =========================================================
        // NAVIGATION PROPERTIES
        // =========================================================

        public EntityType EntityType { get; set; } = null!;

        public Entity? ParentEntity { get; set; }

        public ICollection<Entity> ChildEntities { get; set; }
            = new List<Entity>();

        // Phase 2: EntityValue navigation will be added later
        // public ICollection<EntityValue> Values { get; set; }
        //     = new List<EntityValue>();
    }
}
