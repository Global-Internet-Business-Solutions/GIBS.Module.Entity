using GIBS.Module.Entity.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Interfaces
{
    public interface IEntityValueService
    {
        // CRUD Operations
        Task<EntityValue> AddEntityValueAsync(int entityId, int fieldId, int valueIndex, 
            object typedValue, string? createdBy = null);

        Task AddEntityValuesAsync(int entityId, int fieldId, List<object> typedValues, 
            string? createdBy = null);

        Task<List<EntityValue>> GetEntityValuesAsync(int entityId, int fieldId);

        Task<EntityValue?> GetEntityValueAsync(int entityValueId);

        Task UpdateEntityValueAsync(int entityValueId, object typedValue, 
            string? modifiedBy = null);

        Task ReplaceEntityFieldValuesAsync(int entityId, int fieldId, List<object> typedValues,
            string? modifiedBy = null);

        Task DeleteEntityValueAsync(int entityValueId);

        Task DeleteFieldValuesAsync(int entityId, int fieldId);

        // Search & Filter
        Task<List<Models.Entity>> SearchEntityValuesAsync(List<EntityFieldFilter> filters,
            int? sortFieldId = null, bool sortDescending = false,
            int skip = 0, int take = 20);

        Task<int> CountEntityValuesAsync(List<EntityFieldFilter> filters);

        // Bulk operations
        Task DeleteEntitiesValuesAsync(List<int> entityIds);

        /// <summary>
        /// Gets all entity values for a specific entity across all fields.
        /// </summary>
        Task<List<EntityValue>> GetAllEntityValuesAsync(int entityId);

        /// <summary>
        /// Replaces all entity values for a specific entity. Deletes existing values and inserts new ones.
        /// </summary>
        Task ReplaceAllEntityValuesAsync(int entityId, List<EntityValue> values, string? modifiedBy = null);
    }

    /// <summary>
    /// Represents a filter condition for searching entity values by field.
    /// </summary>
    public class EntityFieldFilter
    {
        /// <summary>
        /// The EntityField ID to filter by
        /// </summary>
        public int FieldId { get; set; }

        /// <summary>
        /// The filter operator: Equals, NotEquals, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual, Contains, StartsWith, EndsWith
        /// </summary>
        public string Operator { get; set; } = "Equals";

        /// <summary>
        /// The filter value (will be converted to appropriate type based on field)
        /// </summary>
        public string Value { get; set; } = "";
    }
}
