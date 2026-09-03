# EntityValue System - Implementation & Migration Roadmap

## Executive Summary

This document outlines the **phased implementation plan** for the EntityValue normalized storage system, moving custom field values from JSON-in-Settings to a properly indexed relational design. The plan is organized into **5 phases** with clear deliverables, validation steps, and rollback procedures.

**Timeline:** 2-3 weeks for full implementation (depending on testing scope and data volume)  
**Risk Level:** Low (reads always work, writes are dual-tracked during migration phases)  
**Rollback:** At any phase boundary, revert to previous storage mode

---

## Phase 1: Database Schema & EF Core Setup (1-2 days)

### Objectives
- Create `GIBS_EntityValue` table with all typed columns
- Configure EF Core mapping, foreign keys, and indexes
- Add migration safely without affecting existing `Entity.Settings`

### Deliverables

#### 1.1 Create EntityValue Model
**File:** `Shared\Models\EntityValue.cs`

```csharp
namespace GIBS.Module.Entity.Shared.Models;

public class EntityValue
{
	public int EntityValueId { get; set; }
	public int EntityId { get; set; }
	public int EntityFieldId { get; set; }
	public int ValueIndex { get; set; } = 0;

	// Typed value columns
	public string? TextValue { get; set; }
	public int? IntegerValue { get; set; }
	public long? LongValue { get; set; }
	public decimal? DecimalValue { get; set; }
	public bool? BooleanValue { get; set; }
	public DateTime? DateValue { get; set; }
	public DateTime? DateTimeValue { get; set; }
	public Guid? GuidValue { get; set; }
	public int? ReferencedEntityId { get; set; }

	// Audit
	public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
	public DateTime? ModifiedDate { get; set; }
	public string? CreatedBy { get; set; }
	public string? ModifiedBy { get; set; }

	// Navigation
	public Entity? Entity { get; set; }
	public EntityField? EntityField { get; set; }
	public Entity? ReferencedEntity { get; set; }
}
```

#### 1.2 Create EF Core Configuration
**File:** `Server\Data\Configuration\EntityValueConfiguration.cs`

```csharp
using GIBS.Module.Entity.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIBS.Module.Entity.Server.Data.Configuration;

public class EntityValueConfiguration : IEntityTypeConfiguration<EntityValue>
{
	public void Configure(EntityTypeBuilder<EntityValue> builder)
	{
		builder.ToTable("GIBS_EntityValue");

		builder.HasKey(ev => ev.EntityValueId);

		// Foreign keys
		builder.HasOne(ev => ev.Entity)
			.WithMany(e => e.Values)
			.HasForeignKey(ev => ev.EntityId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(ev => ev.EntityField)
			.WithMany()
			.HasForeignKey(ev => ev.EntityFieldId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(ev => ev.ReferencedEntity)
			.WithMany()
			.HasForeignKey(ev => ev.ReferencedEntityId)
			.OnDelete(DeleteBehavior.SetNull);

		// Indexes
		builder.HasIndex(ev => new { ev.EntityId, ev.EntityFieldId })
			.HasName("IX_EntityValue_Entity_Field");

		builder.HasIndex(ev => new { ev.EntityFieldId, ev.TextValue })
			.HasName("IX_EntityValue_Field_TextValue")
			.IsUnique(false);

		builder.HasIndex(ev => new { ev.EntityFieldId, ev.IntegerValue })
			.HasName("IX_EntityValue_Field_IntegerValue")
			.IsUnique(false);

		builder.HasIndex(ev => new { ev.EntityFieldId, ev.DecimalValue })
			.HasName("IX_EntityValue_Field_DecimalValue")
			.IsUnique(false);

		builder.HasIndex(ev => new { ev.EntityFieldId, ev.BooleanValue })
			.HasName("IX_EntityValue_Field_BooleanValue")
			.IsUnique(false);

		builder.HasIndex(ev => new { ev.EntityFieldId, ev.DateValue })
			.HasName("IX_EntityValue_Field_DateValue")
			.IsUnique(false);

		builder.HasIndex(ev => new { ev.EntityFieldId, ev.ReferencedEntityId })
			.HasName("IX_EntityValue_Field_ReferencedEntity")
			.IsUnique(false);
	}
}
```

#### 1.3 Update Entity Model
**File:** `Shared\Models\Entity.cs`

Add this navigation property:

```csharp
public virtual ICollection<EntityValue> Values { get; set; } = new List<EntityValue>();
```

#### 1.4 Create EF Core Migration

```bash
cd Server
dotnet ef migrations add CreateEntityValueTable --output-dir Data/Migrations
dotnet ef database update
```

**Verification:**
- Run the migration in a test database
- Confirm `GIBS_EntityValue` table exists with all columns and indexes
- Verify no errors in `Entity.Settings` (should be untouched)

---

## Phase 2: Service Contract & Query Helpers (1 day)

### Objectives
- Define clean API for reading/writing EntityValues
- Create LINQ extension methods for type-safe filtering/sorting
- Support multi-value and reference fields

### Deliverables

#### 2.1 Create Service Interface
**File:** `Shared\Interfaces\IEntityValueService.cs`

```csharp
namespace GIBS.Module.Entity.Shared.Interfaces;

public interface IEntityValueService
{
	// CRUD
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
	Task<List<Entity>> SearchEntityValuesAsync(List<EntityFieldFilter> filters, 
		int? sortFieldId = null, bool sortDescending = false,
		int skip = 0, int take = 20);

	Task<int> CountEntityValuesAsync(List<EntityFieldFilter> filters);

	// Bulk operations
	Task DeleteEntitiesAsync(List<int> entityIds);
}

public class EntityFieldFilter
{
	public int FieldId { get; set; }
	public string Operator { get; set; } = "Equals"; // Equals, NotEquals, GreaterThan, LessThan, Contains, etc.
	public string Value { get; set; } = "";
}
```

#### 2.2 Create Query Extension Methods
**File:** `Server\Services\Extensions\EntityValueQueryExtensions.cs`

```csharp
using GIBS.Module.Entity.Shared.Models;
using GIBS.Module.Entity.Shared.Interfaces;

namespace GIBS.Module.Entity.Server.Services.Extensions;

public static class EntityValueQueryExtensions
{
	// Filter by string value
	public static IQueryable<EntityValue> TextContains(
		this IQueryable<EntityValue> query, int fieldId, string searchText)
		=> query.Where(ev => ev.EntityFieldId == fieldId && 
							ev.TextValue != null && 
							ev.TextValue.Contains(searchText));

	public static IQueryable<EntityValue> TextEquals(
		this IQueryable<EntityValue> query, int fieldId, string value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.TextValue == value);

	// Filter by integer value
	public static IQueryable<EntityValue> IntegerEquals(
		this IQueryable<EntityValue> query, int fieldId, int value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.IntegerValue == value);

	public static IQueryable<EntityValue> IntegerGreaterThan(
		this IQueryable<EntityValue> query, int fieldId, int value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.IntegerValue > value);

	public static IQueryable<EntityValue> IntegerGreaterThanOrEqual(
		this IQueryable<EntityValue> query, int fieldId, int value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.IntegerValue >= value);

	public static IQueryable<EntityValue> IntegerLessThan(
		this IQueryable<EntityValue> query, int fieldId, int value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.IntegerValue < value);

	// Filter by decimal value (for prices, measurements)
	public static IQueryable<EntityValue> DecimalEquals(
		this IQueryable<EntityValue> query, int fieldId, decimal value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.DecimalValue == value);

	public static IQueryable<EntityValue> DecimalGreaterThan(
		this IQueryable<EntityValue> query, int fieldId, decimal value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.DecimalValue > value);

	public static IQueryable<EntityValue> DecimalBetween(
		this IQueryable<EntityValue> query, int fieldId, decimal minValue, decimal maxValue)
		=> query.Where(ev => ev.EntityFieldId == fieldId && 
							ev.DecimalValue >= minValue && 
							ev.DecimalValue <= maxValue);

	// Filter by boolean value
	public static IQueryable<EntityValue> BooleanEquals(
		this IQueryable<EntityValue> query, int fieldId, bool value)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.BooleanValue == value);

	// Filter by date value
	public static IQueryable<EntityValue> DateOn(
		this IQueryable<EntityValue> query, int fieldId, DateTime date)
		=> query.Where(ev => ev.EntityFieldId == fieldId && 
							ev.DateValue == date.Date);

	public static IQueryable<EntityValue> DateAfter(
		this IQueryable<EntityValue> query, int fieldId, DateTime date)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.DateValue >= date.Date);

	public static IQueryable<EntityValue> DateBefore(
		this IQueryable<EntityValue> query, int fieldId, DateTime date)
		=> query.Where(ev => ev.EntityFieldId == fieldId && ev.DateValue <= date.Date);

	// Filter by entity reference
	public static IQueryable<EntityValue> ReferencesEntity(
		this IQueryable<EntityValue> query, int fieldId, int referencedEntityId)
		=> query.Where(ev => ev.EntityFieldId == fieldId && 
							ev.ReferencedEntityId == referencedEntityId);

	// Filter by value existence
	public static IQueryable<EntityValue> HasValue(
		this IQueryable<EntityValue> query, int fieldId)
		=> query.Where(ev => ev.EntityFieldId == fieldId && 
							(ev.TextValue != null || ev.IntegerValue.HasValue ||
							 ev.DecimalValue.HasValue || ev.BooleanValue.HasValue ||
							 ev.DateValue.HasValue || ev.ReferencedEntityId.HasValue));

	// Ordering
	public static IQueryable<EntityValue> OrderByTextValue(
		this IQueryable<EntityValue> query, int fieldId, bool descending = false)
		=> descending
			? query.Where(ev => ev.EntityFieldId == fieldId).OrderByDescending(ev => ev.TextValue)
			: query.Where(ev => ev.EntityFieldId == fieldId).OrderBy(ev => ev.TextValue);

	public static IQueryable<EntityValue> OrderByIntegerValue(
		this IQueryable<EntityValue> query, int fieldId, bool descending = false)
		=> descending
			? query.Where(ev => ev.EntityFieldId == fieldId).OrderByDescending(ev => ev.IntegerValue)
			: query.Where(ev => ev.EntityFieldId == fieldId).OrderBy(ev => ev.IntegerValue);

	public static IQueryable<EntityValue> OrderByDecimalValue(
		this IQueryable<EntityValue> query, int fieldId, bool descending = false)
		=> descending
			? query.Where(ev => ev.EntityFieldId == fieldId).OrderByDescending(ev => ev.DecimalValue)
			: query.Where(ev => ev.EntityFieldId == fieldId).OrderBy(ev => ev.DecimalValue);

	public static IQueryable<EntityValue> OrderByDateValue(
		this IQueryable<EntityValue> query, int fieldId, bool descending = false)
		=> descending
			? query.Where(ev => ev.EntityFieldId == fieldId).OrderByDescending(ev => ev.DateValue)
			: query.Where(ev => ev.EntityFieldId == fieldId).OrderBy(ev => ev.DateValue);
}
```

#### 2.3 Create Service Implementation Stub
**File:** `Server\Services\EntityValueService.cs`

```csharp
using GIBS.Module.Entity.Shared.Models;
using GIBS.Module.Entity.Shared.Interfaces;
using GIBS.Module.Entity.Server.Data;
using GIBS.Module.Entity.Server.Services.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GIBS.Module.Entity.Server.Services;

public class EntityValueService : IEntityValueService
{
	private readonly EntityContext _context;

	public EntityValueService(EntityContext context)
	{
		_context = context;
	}

	public async Task<EntityValue> AddEntityValueAsync(int entityId, int fieldId, 
		int valueIndex, object typedValue, string? createdBy = null)
	{
		var entityValue = new EntityValue
		{
			EntityId = entityId,
			EntityFieldId = fieldId,
			ValueIndex = valueIndex,
			CreatedBy = createdBy
		};

		StoreTypedValue(entityValue, typedValue);

		_context.EntityValues.Add(entityValue);
		await _context.SaveChangesAsync();

		return entityValue;
	}

	public async Task AddEntityValuesAsync(int entityId, int fieldId, 
		List<object> typedValues, string? createdBy = null)
	{
		for (int i = 0; i < typedValues.Count; i++)
		{
			await AddEntityValueAsync(entityId, fieldId, i, typedValues[i], createdBy);
		}
	}

	public async Task<List<EntityValue>> GetEntityValuesAsync(int entityId, int fieldId)
	{
		return await _context.EntityValues
			.Where(ev => ev.EntityId == entityId && ev.EntityFieldId == fieldId)
			.OrderBy(ev => ev.ValueIndex)
			.ToListAsync();
	}

	public async Task<EntityValue?> GetEntityValueAsync(int entityValueId)
	{
		return await _context.EntityValues
			.FirstOrDefaultAsync(ev => ev.EntityValueId == entityValueId);
	}

	public async Task UpdateEntityValueAsync(int entityValueId, object typedValue, 
		string? modifiedBy = null)
	{
		var entityValue = await GetEntityValueAsync(entityValueId);
		if (entityValue == null) throw new InvalidOperationException("EntityValue not found");

		StoreTypedValue(entityValue, typedValue);
		entityValue.ModifiedDate = DateTime.UtcNow;
		entityValue.ModifiedBy = modifiedBy;

		_context.EntityValues.Update(entityValue);
		await _context.SaveChangesAsync();
	}

	public async Task ReplaceEntityFieldValuesAsync(int entityId, int fieldId, 
		List<object> typedValues, string? modifiedBy = null)
	{
		var existing = await GetEntityValuesAsync(entityId, fieldId);

		_context.EntityValues.RemoveRange(existing);
		await _context.SaveChangesAsync();

		await AddEntityValuesAsync(entityId, fieldId, typedValues, modifiedBy);
	}

	public async Task DeleteEntityValueAsync(int entityValueId)
	{
		var entityValue = await GetEntityValueAsync(entityValueId);
		if (entityValue == null) return;

		_context.EntityValues.Remove(entityValue);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteFieldValuesAsync(int entityId, int fieldId)
	{
		var values = await GetEntityValuesAsync(entityId, fieldId);
		_context.EntityValues.RemoveRange(values);
		await _context.SaveChangesAsync();
	}

	public async Task<List<Entity>> SearchEntityValuesAsync(List<EntityFieldFilter> filters, 
		int? sortFieldId = null, bool sortDescending = false, 
		int skip = 0, int take = 20)
	{
		// TODO: Implement filter logic
		// This will build a query with multiple filters and apply sorting
		throw new NotImplementedException("Phase 3 deliverable");
	}

	public async Task<int> CountEntityValuesAsync(List<EntityFieldFilter> filters)
	{
		// TODO: Implement count logic
		throw new NotImplementedException("Phase 3 deliverable");
	}

	public async Task DeleteEntitiesAsync(List<int> entityIds)
	{
		var values = await _context.EntityValues
			.Where(ev => entityIds.Contains(ev.EntityId))
			.ToListAsync();

		_context.EntityValues.RemoveRange(values);
		await _context.SaveChangesAsync();
	}

	private static void StoreTypedValue(EntityValue entityValue, object typedValue)
	{
		// Clear all typed columns first
		entityValue.TextValue = null;
		entityValue.IntegerValue = null;
		entityValue.LongValue = null;
		entityValue.DecimalValue = null;
		entityValue.BooleanValue = null;
		entityValue.DateValue = null;
		entityValue.DateTimeValue = null;
		entityValue.GuidValue = null;
		entityValue.ReferencedEntityId = null;

		// Store in appropriate typed column
		if (typedValue == null) return;

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
			case DateTime dt when dt.TimeOfDay == TimeSpan.Zero:
				entityValue.DateValue = dt;
				break;
			case DateTime dt:
				entityValue.DateTimeValue = dt;
				break;
			case Guid g:
				entityValue.GuidValue = g;
				break;
			default:
				throw new ArgumentException($"Unsupported type: {typedValue.GetType().Name}");
		}
	}
}
```

**Verification:**
- Compile successfully with no errors
- Create a simple unit test: add/get/update/delete EntityValue works
- Verify queries execute without errors

---

## Phase 3: Search & Filter Implementation (1 day)

### Objectives
- Implement multi-filter query builder
- Support dynamic filter operators (Equals, GreaterThan, Contains, etc.)
- Enable sorting by any field type

### Deliverables

#### 3.1 Filter Builder Logic
Update `EntityValueService.SearchEntityValuesAsync()`:

```csharp
public async Task<List<Entity>> SearchEntityValuesAsync(
	List<EntityFieldFilter> filters, 
	int? sortFieldId = null, 
	bool sortDescending = false, 
	int skip = 0, int take = 20)
{
	var query = _context.EntityValues.AsQueryable();

	// Apply each filter
	foreach (var filter in filters)
	{
		query = filter.Operator switch
		{
			"Equals" => query.TextEquals(filter.FieldId, filter.Value),
			"NotEquals" => query.Where(ev => ev.EntityFieldId == filter.FieldId && 
											 ev.TextValue != filter.Value),
			"Contains" => query.TextContains(filter.FieldId, filter.Value),
			"GreaterThan" => query.IntegerGreaterThan(filter.FieldId, 
									int.Parse(filter.Value)),
			"GreaterThanOrEqual" => query.IntegerGreaterThanOrEqual(filter.FieldId, 
									int.Parse(filter.Value)),
			"LessThan" => query.IntegerLessThan(filter.FieldId, 
									int.Parse(filter.Value)),
			_ => throw new ArgumentException($"Unknown operator: {filter.Operator}")
		};
	}

	// Apply sorting
	if (sortFieldId.HasValue)
	{
		// Peek at the field's data type and sort by appropriate column
		var field = await _context.EntityFields.FindAsync(sortFieldId.Value);
		if (field != null)
		{
			query = field.DataType switch
			{
				"String" => query.OrderByTextValue(sortFieldId.Value, sortDescending),
				"Integer" => query.OrderByIntegerValue(sortFieldId.Value, sortDescending),
				"Decimal" => query.OrderByDecimalValue(sortFieldId.Value, sortDescending),
				"Date" => query.OrderByDateValue(sortFieldId.Value, sortDescending),
				_ => query
			};
		}
	}

	// Get distinct entities and paginate
	var entities = await query
		.Select(ev => ev.Entity)
		.Distinct()
		.Skip(skip)
		.Take(take)
		.ToListAsync();

	return entities ?? new List<Entity>();
}

public async Task<int> CountEntityValuesAsync(List<EntityFieldFilter> filters)
{
	var query = _context.EntityValues.AsQueryable();

	foreach (var filter in filters)
	{
		query = filter.Operator switch
		{
			"Equals" => query.TextEquals(filter.FieldId, filter.Value),
			"NotEquals" => query.Where(ev => ev.EntityFieldId == filter.FieldId && 
											 ev.TextValue != filter.Value),
			"Contains" => query.TextContains(filter.FieldId, filter.Value),
			"GreaterThan" => query.IntegerGreaterThan(filter.FieldId, 
									int.Parse(filter.Value)),
			_ => throw new ArgumentException($"Unknown operator: {filter.Operator}")
		};
	}

	return await query
		.Select(ev => ev.EntityId)
		.Distinct()
		.CountAsync();
}
```

**Verification:**
- Test multi-filter queries with various operators
- Confirm sorting by different field types works
- Verify pagination (skip/take) functions correctly

---

## Phase 4: Client-Side Integration (2 days)

### Objectives
- Update `EntityEdit.razor` to write to EntityValue instead of Settings
- Update `EntityList.razor` to read from EntityValue for display/search
- Maintain backward compatibility (still read Settings during transition)

### Key Changes

#### 4.1 EntityEdit.razor Updates

**File:** `Client\Modules\GIBS.Module.Entity\EntityEdit.razor`

Changes to `SaveEntity()`:

```csharp
private async Task SaveEntity()
{
	// ... existing entity save logic ...

	// NEW: Save custom field values to EntityValue
	if (_customFieldValues.Any())
	{
		foreach (var kvp in _customFieldValues)
		{
			int fieldId = int.Parse(kvp.Key);
			var fieldIds = kvp.Value as List<object> ?? new List<object> { kvp.Value };

			await EntityValueService.ReplaceEntityFieldValuesAsync(
				_entity.EntityId, fieldId, fieldIds, ModuleState.CurrentUser?.Username
			);
		}
	}

	// ... rest of save logic ...
}
```

Changes to `LoadEntity()`:

```csharp
private async Task LoadEntity()
{
	// ... existing entity load logic ...

	// NEW: Load custom field values from EntityValue
	var customFields = await EntityFieldService.GetEntityFieldsAsync(_entity.EntityTypeId);

	_customFieldValues = new Dictionary<string, object>();
	foreach (var field in customFields)
	{
		var values = await EntityValueService.GetEntityValuesAsync(_entity.EntityId, field.EntityFieldId);
		if (values.Any())
		{
			if (values.Count == 1)
			{
				_customFieldValues[field.EntityFieldId.ToString()] = GetTypedValue(values[0]);
			}
			else
			{
				_customFieldValues[field.EntityFieldId.ToString()] = 
					values.Select(v => GetTypedValue(v)).ToList();
			}
		}
	}

	// ... rest of load logic ...
}

private object GetTypedValue(EntityValue ev)
{
	if (ev.TextValue != null) return ev.TextValue;
	if (ev.IntegerValue.HasValue) return ev.IntegerValue.Value;
	if (ev.DecimalValue.HasValue) return ev.DecimalValue.Value;
	if (ev.BooleanValue.HasValue) return ev.BooleanValue.Value;
	if (ev.DateValue.HasValue) return ev.DateValue.Value;
	if (ev.ReferencedEntityId.HasValue) return ev.ReferencedEntityId.Value;
	return "";
}
```

#### 4.2 EntityList.razor Updates

**File:** `Client\Modules\GIBS.Module.Entity\EntityList.razor`

Add filtering/search support:

```csharp
private async Task SearchEntities()
{
	var filters = new List<EntityFieldFilter>();

	// Example: Build filters from UI inputs
	if (!string.IsNullOrEmpty(_searchText))
	{
		filters.Add(new EntityFieldFilter
		{
			FieldId = _nameFieldId,
			Operator = "Contains",
			Value = _searchText
		});
	}

	_entities = await EntityValueService.SearchEntityValuesAsync(
		filters, 
		sortFieldId: _sortFieldId,
		sortDescending: _sortDescending,
		skip: _pageIndex * _pageSize,
		take: _pageSize
	);
}
```

**Verification:**
- Test EntityEdit: save a custom field, reload page, value persists
- Test EntityList: filter by custom field, results are accurate
- Verify both read and write paths work without errors

---

## Phase 5: Data Migration & Cleanup (1-2 days)

### Objectives
- Migrate existing custom field values from Entity.Settings to EntityValue
- Verify data integrity
- Remove obsolete Settings logic

### 5.1 Data Migration Strategy

Create a one-time **migration script** that:

```csharp
// File: Server\Services\Migration\EntityValueMigrationService.cs

public async Task MigrateSettingsToEntityValuesAsync()
{
	var entities = await _context.Entities
		.Include(e => e.EntityType)
		.ThenInclude(et => et.EntityFields)
		.ToListAsync();

	foreach (var entity in entities)
	{
		if (string.IsNullOrEmpty(entity.Settings)) continue;

		try
		{
			var settingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings);

			foreach (var kvp in settingsDict)
			{
				if (!int.TryParse(kvp.Key, out int fieldId)) continue;

				var field = entity.EntityType?.EntityFields
					.FirstOrDefault(f => f.EntityFieldId == fieldId);

				if (field == null) continue;

				// Handle multi-value or single value
				var values = new List<object>();
				if (kvp.Value is JsonElement je)
				{
					if (je.ValueKind == JsonValueKind.Array)
					{
						values = je.EnumerateArray()
							.Select(e => ConvertToTypedValue(e, field.DataType))
							.ToList();
					}
					else
					{
						values.Add(ConvertToTypedValue(je, field.DataType));
					}
				}

				await _entityValueService.AddEntityValuesAsync(
					entity.EntityId, fieldId, values, "SYSTEM_MIGRATION"
				);
			}
		}
		catch (Exception ex)
		{
			// Log migration error, continue
			_logger.LogError($"Failed to migrate entity {entity.EntityId}: {ex.Message}");
		}
	}
}
```

### 5.2 Validation & Cleanup

```csharp
public async Task ValidateMigrationAsync()
{
	var migrationErrors = new List<string>();

	var entities = await _context.Entities.ToListAsync();
	foreach (var entity in entities)
	{
		var valueCount = await _context.EntityValues
			.CountAsync(ev => ev.EntityId == entity.EntityId);

		// TODO: Add business logic validation
		if (valueCount == 0 && !string.IsNullOrEmpty(entity.Settings))
		{
			migrationErrors.Add($"Entity {entity.EntityId} has Settings but no EntityValues");
		}
	}

	if (migrationErrors.Any())
	{
		throw new InvalidOperationException(
			$"Migration validation failed:\n{string.Join("\n", migrationErrors)}"
		);
	}
}

public async Task CleanupSettingsAsync()
{
	// One-time cleanup after validation
	var entities = await _context.Entities.ToListAsync();
	foreach (var entity in entities)
	{
		// Optionally back up Settings, then clear
		entity.Settings = null;
	}
	await _context.SaveChangesAsync();
}
```

**Verification:**
- Run migration on dev database
- Verify row counts: EntityValues ≥ old Settings count
- Spot-check migrated values match original
- Test EntityList/EntityEdit with migrated data
- Backup production database before migration

---

## Rollback Procedures

### At Any Phase Boundary

**If issues are found:**

1. **Phase 1 Rollback:** Remove Migration
   ```bash
   dotnet ef migrations remove
   ```

2. **Phase 2-3 Rollback:** Remove Service (re-compile with old Settings logic)
   ```bash
   rm Server/Services/EntityValueService.cs
   rm Server/Services/Extensions/EntityValueQueryExtensions.cs
   ```

3. **Phase 4 Rollback:** Revert EntityEdit/EntityList to Settings-based logic
   ```bash
   git checkout Client/Modules/GIBS.Module.Entity/EntityEdit.razor
   git checkout Client/Modules/GIBS.Module.Entity/EntityList.razor
   ```

4. **Phase 5 Rollback:** Restore Entity.Settings from backup
   ```sql
   UPDATE GIBS_Entity SET Settings = [backup_value]
   ```

---

## Testing Strategy

### Manual Testing Checklist

- [ ] **Create Entity:** Custom fields save to EntityValue
- [ ] **Edit Entity:** Custom fields load from EntityValue, edits persist
- [ ] **Delete Entity:** EntityValues cascade-delete
- [ ] **Search:** Filter by custom field returns correct results
- [ ] **Sort:** Results sort by custom field correctly
- [ ] **Multi-value:** Fields with multiple values display/edit correctly
- [ ] **References:** Entity reference fields show/filter correctly
- [ ] **Migration:** Old Settings data migrates accurately
- [ ] **Bulk Ops:** Deleting multiple entities deletes their values

### Performance Testing

- **Baseline:** Entity search with 1000 entities (current Settings)
- **After EntityValue:** Same search (should be 10-100x faster due to indexes)
- **Complex Filters:** 3+ custom field filters simultaneously

---

## Timeline & Effort Estimate

| Phase | Duration | Risk | Owner |
|-------|----------|------|-------|
| Phase 1: Schema & EF Setup | 1-2 days | Low | Backend |
| Phase 2: Service & Queries | 1 day | Low | Backend |
| Phase 3: Search & Filter | 1 day | Medium | Backend |
| Phase 4: Client Integration | 2 days | Medium | Frontend + Backend |
| Phase 5: Migration & Cleanup | 1-2 days | Medium | Backend + DevOps |
| **TOTAL** | **2-3 weeks** | **Low-Medium** | **Team** |

---

## Success Criteria

✅ All 5 phases complete and compile  
✅ EntityValue reads/writes work in EntityEdit/EntityList  
✅ Search/filter queries execute and return correct results  
✅ Multi-value and reference fields work correctly  
✅ Migration runs without errors on dev/test databases  
✅ Performance improved by 10x or more for filtered searches  
✅ Zero data loss after migration  
✅ Rollback procedure tested and documented  

---

## Next Steps

1. **Review & Approve** this implementation plan with the team
2. **Start Phase 1:** Create the model, configuration, and migration
3. **Daily standups:** Track progress and blockers
4. **Testing gates:** Each phase must have validation tests pass before proceeding
5. **Pilot migration:** Test on a copy of production data before prod cutover

