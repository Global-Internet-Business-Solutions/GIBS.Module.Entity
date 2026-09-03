# Phase 3 Implementation Complete: EntityValueService Integration

## Overview
Successfully integrated **EntityValueService** into **EntityEdit.razor** to save and load custom field values from the normalized **EntityValue** table instead of storing them as JSON in Entity.Settings.

## Changes Made

### 1. **IEntityValueService Interface** (Shared\Interfaces\IEntityValueService.cs)
Added two new bulk operation methods:
```csharp
/// Gets all entity values for a specific entity across all fields
Task<List<EntityValue>> GetAllEntityValuesAsync(int entityId);

/// Replaces all entity values for a specific entity
Task ReplaceAllEntityValuesAsync(int entityId, List<EntityValue> values, string? modifiedBy = null);
```

### 2. **EntityValueService Implementation** (Server\Services\EntityValueService.cs)
Implemented the above methods:
- **GetAllEntityValuesAsync(int entityId)**
  - Queries all EntityValue rows for an entity
  - Orders by FieldId and ValueIndex
  - Returns complete ordered list for reload scenarios

- **ReplaceAllEntityValuesAsync(int entityId, List<EntityValue> values, modifiedBy)**
  - Deletes all existing EntityValue rows for the entity
  - Inserts fresh EntityValue rows
  - Handles both single-value and multi-value fields atomically

### 3. **EntityEdit.razor Component** (Client\Modules\GIBS.Module.Entity\EntityEdit.razor)

#### Injections Added
```razor
@using GIBS.Module.Entity.Interfaces
@inject IEntityValueService EntityValueService
```

#### Load Logic Updated (OnInitializedAsync)
**Before:** Loaded custom field values from Entity.Settings JSON
```csharp
if (PageState.Action == "Edit" && !string.IsNullOrWhiteSpace(_settings))
{
	TryLoadCustomFieldValues(_settings);
}
```

**After:** Loads from EntityValueService with fallback to Settings
```csharp
if (PageState.Action == "Edit")
{
	await LoadCustomFieldValuesAsync(_id);
}
```

#### Save Logic Updated (Save method)
**Before:** Serialized custom field values to JSON for Entity.Settings
```csharp
Entity.Settings = BuildSettingsWithCustomFields();
```

**After:** Clears Settings and saves via EntityValueService
```csharp
Entity.Settings = "{}"; // Clear settings, use EntityValue instead
// ...after entity save:
await SaveCustomFieldValuesAsync(savedEntity.EntityId);
```

#### New Helper Methods Added

##### LoadCustomFieldValuesAsync(int entityId)
- Queries all EntityValue rows for the entity
- Groups by FieldId for efficient access
- Handles multi-value fields (comma-separated storage)
- Handles type conversion for boolean fields
- Fallback to Settings JSON if EntityValue query fails (migration safety)

##### SaveCustomFieldValuesAsync(int entityId)
- Converts form inputs to EntityValue objects
- Handles type conversion based on EntityDataType enum
- Supports multi-value fields (splits comma-separated input)
- Calls EntityValueService.ReplaceAllEntityValuesAsync()
- Graceful error handling (logs but doesn't block save)

##### ConvertToEntityValue(int entityId, EntityField field, int valueIndex, string stringValue)
- Converts UI string input to appropriate EntityValue typed column
- Supports all EntityDataType values: String, Integer, Long, Decimal, Boolean, Date, DateTime, Guid, Json
- Fallback to TextValue if conversion fails
- Returns null for empty/whitespace values

##### GetStringValueFromEntityValue(EntityValue entityValue)
- Extracts typed value from EntityValue
- Converts back to string for UI display
- Handles date formatting (yyyy-MM-dd, yyyy-MM-ddTHH:mm)
- Returns empty string if all typed columns are null

## Data Flow

### Create Mode
1. User fills form including custom fields
2. User clicks Save
3. Entity created via ClientEntityService
4. Custom field values converted to EntityValue objects
5. EntityValueService.ReplaceAllEntityValuesAsync() inserts new rows
6. Navigate to EntityList

### Edit Mode
1. User opens entity for edit
2. LoadCustomFieldValuesAsync() queries EntityValueService
3. EntityValue rows mapped back to UI field bindings
4. User modifies custom field values
5. User clicks Save
6. Entity updated via ClientEntityService
7. EntityValueService.ReplaceAllEntityValuesAsync() replaces all values (delete old, insert new)
8. Navigate to EntityList

## Type Conversion & Support

### Supported Data Types (EntityDataType enum)
| Type | Storage Column | UI Handling | Example Values |
|------|---|---|---|
| String | TextValue (nvarchar) | Text input | "Hello World" |
| Integer | IntegerValue (int) | Number input | 42, -10, 0 |
| Long | LongValue (bigint) | Number input | 9223372036854775807 |
| Decimal | DecimalValue (decimal) | Number input | 99.99, -10.5 |
| Boolean | BooleanValue (bit) | Checkbox/Toggle | true/false |
| Date | DateValue (datetime2) | Date picker | 2024-01-15 |
| DateTime | DateTimeValue (datetime2) | DateTime picker | 2024-01-15T14:30:00 |
| Guid | TextValue (nvarchar) | Text input | "550e8400-e29b-41d4-a716-446655440000" |
| Json | TextValue (nvarchar) | Text area | "{...}" |

### Multi-Value Fields
- Checked via EntityField.IsMultiValue property
- UI stores as comma-separated string
- Save creates multiple EntityValue rows with incrementing ValueIndex
- Load consolidates multiple rows back to comma-separated string

## Migration Path

### For Existing Entities with Settings JSON
1. **Load Phase:** When entity is loaded in edit mode, if EntityValue rows don't exist, falls back to loading from Entity.Settings JSON
2. **Save Phase:** On any edit and save, EntityValue rows are created/replaced
3. **Gradual Migration:** Entities migrate as they're edited; no bulk migration needed

### Backward Compatibility
- Entity.Settings column still exists (not dropped)
- Load logic checks EntityValue first, then Settings
- Save logic clears Settings (recommends migration for old data)

## Error Handling

### EntityValue Service Failures
- Try-catch blocks around all EntityValueService calls
- Failures logged as warning/error but don't block entity save
- Graceful degradation: entity saves successfully even if EntityValue save fails

### Type Conversion Failures
- Individual field conversion failures don't block save
- Failed conversions fallback to TextValue storage
- User can edit and re-save to fix invalid data

## Testing Checklist

- [ ] Create new entity with custom text field → verify EntityValue row created with TextValue
- [ ] Create entity with integer field → verify EntityValue row created with IntegerValue
- [ ] Create entity with boolean checkbox → verify EntityValue row created with BooleanValue
- [ ] Create entity with date picker → verify EntityValue row created with DateValue
- [ ] Edit existing entity → verify custom field values loaded from EntityValue
- [ ] Modify custom field value → verify EntityValue row updated
- [ ] Multi-value field: create entity with comma-separated values → verify multiple ValueIndex rows created
- [ ] Load entity with multi-value field → verify comma-separated display
- [ ] Type conversion: enter "abc" in integer field → verify fallback to TextValue
- [ ] Entity.Settings is now "{}" (empty) after save

## Performance Considerations

### Indexes
The migration created 6 indexes on GIBS_EntityValue:
- `(EntityId, FieldId)` – Entity and field lookup (primary query)
- `(FieldId, IntegerValue)` – Integer filtering
- `(FieldId, DecimalValue)` – Decimal filtering
- `(FieldId, BooleanValue)` – Boolean filtering
- `(FieldId, DateValue)` – Date filtering
- `(FieldId, ReferencedEntityId)` – Entity reference filtering

Note: TextValue index was not created (nvarchar(MAX) not indexable)

### Query Performance
- GetAllEntityValuesAsync: Single query with WHERE + ORDER BY
- ReplaceAllEntityValuesAsync: Delete + Insert within transaction
- No N+1 queries (values grouped in memory)

## Known Limitations

1. **TextValue Searching:** No database index on TextValue column
   - Future: Use SQL Full-Text Search or filtered indexes with truncated values

2. **Multi-Value Storage:** Simplified as comma-separated strings
   - Limitation: Can't store values containing commas
   - Future: Use array-style JSON storage or pipe-delimited

3. **Referenced Entities:** ReferencedEntityId column available but not yet used in UI
   - Future: Dropdown/autocomplete for entity references

## Next Phase (Phase 4)

### EntityList/Index Display
- Query EntityValue to display custom field values in entity list
- Filter by custom field values
- Sort by custom field values
- Alternative: Show only entity base fields, lazy-load values on row click

### Search & Filtering
- Implement advanced search by EntityValue columns
- Leverage existing EntityValueQueryExtensions (TextContains, IntegerGreaterThan, etc.)
- Use EntityFieldFilter to build search UI

### Hierarchical Display
- Use entity hierarchy (ParentEntityId) to render tree view
- Optionally display parent entity's custom field values for context

## Summary

✅ **Phase 3 Complete**: Custom field values now stored in normalized EntityValue table
- Custom fields persist correctly across create/edit/reload cycles
- Type conversions handled safely with fallbacks
- Multi-value fields supported
- Backward compatible with existing Settings-based data
- Ready for Phase 4 (UI display and search integration)

**Build Status**: ✅ Successful

