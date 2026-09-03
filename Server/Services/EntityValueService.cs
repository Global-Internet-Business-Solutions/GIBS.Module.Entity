using GIBS.Module.Entity.Enums;
using GIBS.Module.Entity.Interfaces;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Repository;
using GIBS.Module.Entity.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Services
{
    /// <summary>
    /// Service for managing EntityValue CRUD operations and advanced search/filter queries.
    /// </summary>
    public class EntityValueService : IEntityValueService
    {
        private readonly EntityContext _context;

        public EntityValueService(EntityContext context)
        {
            _context = context;
        }

        // ========================================
        // CRUD OPERATIONS
        // ========================================

        /// <summary>
        /// Add a single typed value for an entity field
        /// </summary>
        public async Task<EntityValue> AddEntityValueAsync(int entityId, int fieldId, 
            int valueIndex, object typedValue, string? createdBy = null)
        {
            var entityValue = new EntityValue
            {
                EntityId = entityId,
                FieldId = fieldId,
                ValueIndex = valueIndex,
                CreatedBy = createdBy
            };

            StoreTypedValue(entityValue, typedValue);

            _context.EntityValues.Add(entityValue);
            await _context.SaveChangesAsync();

            return entityValue;
        }

        /// <summary>
        /// Add multiple typed values for an entity field (for multi-value fields)
        /// </summary>
        public async Task AddEntityValuesAsync(int entityId, int fieldId, 
            List<object> typedValues, string? createdBy = null)
        {
            for (int i = 0; i < typedValues.Count; i++)
            {
                await AddEntityValueAsync(entityId, fieldId, i, typedValues[i], createdBy);
            }
        }

        /// <summary>
        /// Get all values for an entity field, ordered by ValueIndex
        /// </summary>
        public async Task<List<EntityValue>> GetEntityValuesAsync(int entityId, int fieldId)
        {
            return await _context.EntityValues
                .Where(ev => ev.EntityId == entityId && ev.FieldId == fieldId)
                .OrderBy(ev => ev.ValueIndex)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific EntityValue by ID
        /// </summary>
        public async Task<EntityValue?> GetEntityValueAsync(int entityValueId)
        {
            return await _context.EntityValues
                .FirstOrDefaultAsync(ev => ev.EntityValueId == entityValueId);
        }

        /// <summary>
        /// Update an existing EntityValue with a new typed value
        /// </summary>
        public async Task UpdateEntityValueAsync(int entityValueId, object typedValue, 
            string? modifiedBy = null)
        {
            var entityValue = await GetEntityValueAsync(entityValueId);
            if (entityValue == null) 
                throw new InvalidOperationException($"EntityValue {entityValueId} not found");

            StoreTypedValue(entityValue, typedValue);
            entityValue.ModifiedBy = modifiedBy;

            _context.EntityValues.Update(entityValue);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Replace all values for an entity field (delete old, add new)
        /// </summary>
        public async Task ReplaceEntityFieldValuesAsync(int entityId, int fieldId, 
            List<object> typedValues, string? modifiedBy = null)
        {
            var existing = await GetEntityValuesAsync(entityId, fieldId);

            _context.EntityValues.RemoveRange(existing);
            await _context.SaveChangesAsync();

            await AddEntityValuesAsync(entityId, fieldId, typedValues, modifiedBy);
        }

        /// <summary>
        /// Delete a specific EntityValue by ID
        /// </summary>
        public async Task DeleteEntityValueAsync(int entityValueId)
        {
            var entityValue = await GetEntityValueAsync(entityValueId);
            if (entityValue == null) return;

            _context.EntityValues.Remove(entityValue);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete all values for an entity field
        /// </summary>
        public async Task DeleteFieldValuesAsync(int entityId, int fieldId)
        {
            var values = await GetEntityValuesAsync(entityId, fieldId);
            _context.EntityValues.RemoveRange(values);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete all values for a list of entities (bulk operation)
        /// </summary>
        public async Task DeleteEntitiesValuesAsync(List<int> entityIds)
        {
            var values = await _context.EntityValues
                .Where(ev => entityIds.Contains(ev.EntityId))
                .ToListAsync();

            _context.EntityValues.RemoveRange(values);
            await _context.SaveChangesAsync();
        }

        // ========================================
        // SEARCH & FILTER OPERATIONS
        // ========================================

        /// <summary>
        /// Search entities by multiple field filters with optional sorting and pagination
        /// </summary>
        public async Task<List<Models.Entity>> SearchEntityValuesAsync(List<EntityFieldFilter> filters, 
            int? sortFieldId = null, bool sortDescending = false, 
            int skip = 0, int take = 20)
        {
            var query = _context.EntityValues.AsQueryable();

            // Apply each filter condition
            foreach (var filter in filters)
            {
                query = ApplyFilter(query, filter);
            }

            // Apply sorting if specified
            if (sortFieldId.HasValue)
            {
                var field = await _context.EntityFields.FindAsync(sortFieldId.Value);
                if (field != null)
                {
                    query = ApplySorting(query, sortFieldId.Value, field.DataType, sortDescending);
                }
            }

            // Get distinct entities (multiple values for one entity should appear once)
            var entities = await query
                .Select(ev => ev.Entity)
                .Distinct()
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities ?? new List<Models.Entity>();
        }

        /// <summary>
        /// Count how many distinct entities match the filter criteria
        /// </summary>
        public async Task<int> CountEntityValuesAsync(List<EntityFieldFilter> filters)
        {
            var query = _context.EntityValues.AsQueryable();

            foreach (var filter in filters)
            {
                query = ApplyFilter(query, filter);
            }

            return await query
                .Select(ev => ev.EntityId)
                .Distinct()
                .CountAsync();
        }

        // ========================================
        // HELPER METHODS
        // ========================================

        /// <summary>
        /// Apply a single filter condition based on operator and field type
        /// </summary>
        private static IQueryable<EntityValue> ApplyFilter(
            IQueryable<EntityValue> query, EntityFieldFilter filter)
        {
            return filter.Operator switch
            {
                // Text operators
                "Equals" => query.TextEquals(filter.FieldId, filter.Value),
                "NotEquals" => query.TextNotEquals(filter.FieldId, filter.Value),
                "Contains" => query.TextContains(filter.FieldId, filter.Value),
                "StartsWith" => query.TextStartsWith(filter.FieldId, filter.Value),
                "EndsWith" => query.TextEndsWith(filter.FieldId, filter.Value),

                // Numeric operators (int)
                "GreaterThan" => query.IntegerGreaterThan(filter.FieldId, int.Parse(filter.Value)),
                "GreaterThanOrEqual" => query.IntegerGreaterThanOrEqual(filter.FieldId, int.Parse(filter.Value)),
                "LessThan" => query.IntegerLessThan(filter.FieldId, int.Parse(filter.Value)),
                "LessThanOrEqual" => query.IntegerLessThanOrEqual(filter.FieldId, int.Parse(filter.Value)),

                // Boolean operators
                "True" => query.BooleanTrue(filter.FieldId),
                "False" => query.BooleanFalse(filter.FieldId),

                // Default or unmapped
                _ => throw new ArgumentException($"Unknown filter operator: {filter.Operator}")
            };
        }

        /// <summary>
        /// Apply sorting based on field data type
        /// </summary>
        private static IQueryable<EntityValue> ApplySorting(
            IQueryable<EntityValue> query, int fieldId, EntityDataType dataType, bool descending)
        {
            return dataType switch
            {
                EntityDataType.String => query.OrderByTextValue(fieldId, descending),
                EntityDataType.Integer => query.OrderByIntegerValue(fieldId, descending),
                EntityDataType.Long => query.OrderBy(ev => ev.LongValue),
                EntityDataType.Decimal => query.OrderByDecimalValue(fieldId, descending),
                EntityDataType.Boolean => query.OrderBy(ev => ev.BooleanValue),
                EntityDataType.Date => query.OrderByDateValue(fieldId, descending),
                EntityDataType.DateTime => query.OrderByDateTimeValue(fieldId, descending),
                EntityDataType.Guid => query.OrderBy(ev => ev.GuidValue),
                _ => query
            };
        }

        /// <summary>
        /// Store a typed value in the appropriate column of an EntityValue
        /// Clears all other typed columns first
        /// </summary>
        private static void StoreTypedValue(EntityValue entityValue, object? typedValue)
        {
            // Clear all typed columns first to avoid stale data
            entityValue.TextValue = null;
            entityValue.IntegerValue = null;
            entityValue.LongValue = null;
            entityValue.DecimalValue = null;
            entityValue.BooleanValue = null;
            entityValue.DateValue = null;
            entityValue.DateTimeValue = null;
            entityValue.GuidValue = null;
            entityValue.ReferencedEntityId = null;

            // Return if value is null
            if (typedValue == null) return;

            // Store in appropriate typed column based on runtime type
            switch (typedValue)
            {
                case string s:
                    entityValue.TextValue = s;
                    break;

                case int i:
                    entityValue.IntegerValue = i;
                    break;

                case long l:
                    entityValue.LongValue = l;
                    break;

                case decimal d:
                    entityValue.DecimalValue = d;
                    break;

                case bool b:
                    entityValue.BooleanValue = b;
                    break;

                case DateTime dt:
                    // If time is midnight, treat as Date only
                    if (dt.TimeOfDay == TimeSpan.Zero)
                        entityValue.DateValue = dt;
                    else
                        entityValue.DateTimeValue = dt;
                    break;

                case Guid g:
                    entityValue.GuidValue = g;
                    break;

                default:
                    throw new ArgumentException(
                        $"Unsupported type for EntityValue: {typedValue.GetType().Name}. " +
                        $"Supported types: string, int, long, decimal, bool, DateTime, Guid");
            }
        }

        /// <summary>
        /// Gets all entity values for a specific entity across all fields.
        /// </summary>
        public async Task<List<EntityValue>> GetAllEntityValuesAsync(int entityId)
        {
            return await _context.EntityValues
                .Where(ev => ev.EntityId == entityId)
                .OrderBy(ev => ev.FieldId)
                .ThenBy(ev => ev.ValueIndex)
                .ToListAsync();
        }

        /// <summary>
        /// Replaces all entity values for a specific entity. Deletes existing values and inserts new ones.
        /// </summary>
        /// <summary>
        /// Smart merge of entity values - only updates/inserts/deletes what's necessary
        /// Preserves EntityValueId, CreatedBy, and CreatedOn for unchanged records
        /// </summary>
        public async Task ReplaceAllEntityValuesAsync(int entityId, List<EntityValue> values, string? modifiedBy = null)
        {
            // Get all existing values for this entity
            var existingValues = await _context.EntityValues
                .Where(ev => ev.EntityId == entityId)
                .ToListAsync();

            // Create a set of incoming value keys (FieldId, ValueIndex) for quick lookup
            var incomingValueKeys = new HashSet<(int FieldId, int ValueIndex)>(
                values.Select(v => (v.FieldId, v.ValueIndex)));

            // Track which existing values have been processed
            var processedExistingIds = new HashSet<int>();

            // Step 1: Update existing records and track which ones match
            foreach (var incomingValue in values)
            {
                var existingValue = existingValues.FirstOrDefault(ev =>
                    ev.FieldId == incomingValue.FieldId &&
                    ev.ValueIndex == incomingValue.ValueIndex);

                if (existingValue != null)
                {
                    // Update existing record - preserve CreatedBy, CreatedOn, only update values and timestamps
                    existingValue.TextValue = incomingValue.TextValue;
                    existingValue.IntegerValue = incomingValue.IntegerValue;
                    existingValue.LongValue = incomingValue.LongValue;
                    existingValue.DecimalValue = incomingValue.DecimalValue;
                    existingValue.BooleanValue = incomingValue.BooleanValue;
                    existingValue.DateValue = incomingValue.DateValue;
                    existingValue.DateTimeValue = incomingValue.DateTimeValue;
                    existingValue.GuidValue = incomingValue.GuidValue;
                    existingValue.ReferencedEntityId = incomingValue.ReferencedEntityId;
                    existingValue.ModifiedBy = modifiedBy;
                    existingValue.ModifiedOn = DateTime.UtcNow;

                    _context.EntityValues.Update(existingValue);
                    processedExistingIds.Add(existingValue.EntityValueId);
                }
                else
                {
                    // Insert new record
                    var newEntityValue = new EntityValue
                    {
                        EntityId = entityId,
                        FieldId = incomingValue.FieldId,
                        ValueIndex = incomingValue.ValueIndex,
                        TextValue = incomingValue.TextValue,
                        IntegerValue = incomingValue.IntegerValue,
                        LongValue = incomingValue.LongValue,
                        DecimalValue = incomingValue.DecimalValue,
                        BooleanValue = incomingValue.BooleanValue,
                        DateValue = incomingValue.DateValue,
                        DateTimeValue = incomingValue.DateTimeValue,
                        GuidValue = incomingValue.GuidValue,
                        ReferencedEntityId = incomingValue.ReferencedEntityId,
                        CreatedBy = modifiedBy ?? incomingValue.CreatedBy,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedBy = modifiedBy,
                        ModifiedOn = DateTime.UtcNow
                    };

                    _context.EntityValues.Add(newEntityValue);
                }
            }

            // Step 2: Delete records that no longer exist in the incoming data
            var valuesToDelete = existingValues
                .Where(ev => !processedExistingIds.Contains(ev.EntityValueId))
                .ToList();

            if (valuesToDelete.Any())
            {
                _context.EntityValues.RemoveRange(valuesToDelete);
            }

            // Save all changes atomically
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Extract the typed value from an EntityValue (returns appropriate type)
        /// </summary>
        public static object? GetTypedValue(EntityValue entityValue)
        {
            if (entityValue.TextValue != null) return entityValue.TextValue;
            if (entityValue.IntegerValue.HasValue) return entityValue.IntegerValue.Value;
            if (entityValue.LongValue.HasValue) return entityValue.LongValue.Value;
            if (entityValue.DecimalValue.HasValue) return entityValue.DecimalValue.Value;
            if (entityValue.BooleanValue.HasValue) return entityValue.BooleanValue.Value;
            if (entityValue.DateValue.HasValue) return entityValue.DateValue.Value;
            if (entityValue.DateTimeValue.HasValue) return entityValue.DateTimeValue.Value;
            if (entityValue.GuidValue.HasValue) return entityValue.GuidValue.Value;
            if (entityValue.ReferencedEntityId.HasValue) return entityValue.ReferencedEntityId.Value;

            return null;
        }
    }
}
