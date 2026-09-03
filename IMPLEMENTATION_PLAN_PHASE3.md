# Phase 3: Integrate EntityValueService into EntityEdit.razor

## Objective
Update **EntityEdit.razor** to save custom field values to the new normalized **EntityValue** table instead of storing them as JSON in Entity.Settings.

## Current State
- **Storage:** Custom field values stored in `Entity.Settings` as JSON dictionary
- **Retrieval:** Values loaded from `Entity.Settings` during edit mode
- **Saving:** `BuildSettingsWithCustomFields()` serializes `_customFieldValues` to JSON
- **Service:** `IEntityValueService` exists but is not yet integrated into the client-side edit flow

## Key Changes Required

### 1. Update EntityEdit.razor Injection
Add `IEntityValueService` to the service injections:
```razor
@inject IEntityValueService EntityValueService
```

### 2. Load Custom Field Values from EntityValue Table
Replace current JSON deserialization with EntityValueService calls:
- **Current:** Load from `Entity.Settings` JSON
- **New:** `EntityValueService.GetEntityValuesAsync(entityId)` → populate UI fields
- **Fallback:** If no EntityValue rows exist, optionally load from Entity.Settings for migration purposes

### 3. Save Custom Field Values to EntityValue Table
Replace `BuildSettingsWithCustomFields()` storage with:
- After entity is created/updated, call `EntityValueService.ReplaceEntityFieldValuesAsync()`
- This method will:
  - Delete existing EntityValue rows for the entity
  - Insert new rows from collected custom field values
  - Handle type conversion (TextValue → IntegerValue, etc.)

### 4. Handle Type Conversion
When saving, map form input to correct EntityValue column based on `EntityField.DataType`:
- **Text/Guid → TextValue** (nvarchar)
- **Integer → IntegerValue** (int)
- **Decimal/Currency → DecimalValue** (decimal)
- **Boolean → BooleanValue** (bit)
- **Date → DateValue** (datetime)
- **DateTime → DateTimeValue** (datetime)

### 5. Support Multi-Value Fields
If `EntityField.IsMultiValue == true`:
- Increment `ValueIndex` for each value in the collection
- Store multiple EntityValue rows with same `(EntityId, FieldId)` but different `ValueIndex`

### 6. Update Entity.Settings (Migration Path)
- Optionally keep Entity.Settings for backward compatibility during migration
- Or clear Settings once values are moved to EntityValue table
- **Decision Point:** Full cutover vs. dual-write strategy

## Implementation Tasks

### Step 1: Add IEntityValueService Injection
- Location: `Client/Modules/GIBS.Module.Entity/EntityEdit.razor`
- Add `@inject IEntityValueService EntityValueService` to directives

### Step 2: Update LoadCustomFieldsAsync() for Edit Mode
- When editing an existing entity, load values from EntityValueService instead of Settings JSON
- Populate `_customFieldValues` and `_customFieldBoolValues` with retrieved data
- Handle type conversion (reverse of save): IntegerValue → string, etc.

### Step 3: Create LoadCustomFieldValuesFromDatabase() Method
- Query: `var values = await EntityValueService.GetEntityValuesAsync(_id);`
- Group by FieldId and ValueIndex
- Populate UI field bindings with retrieved values
- For multi-value fields, consider UI representation (comma-separated, multi-select, etc.)

### Step 4: Refactor Save Logic
- After `ClientEntityService.AddEntityAsync()` or `UpdateEntityAsync()` succeeds
- Collect current custom field values: convert form inputs to EntityValue objects
- Call `EntityValueService.ReplaceEntityFieldValuesAsync(entityId, values)`
- Handle potential errors gracefully

### Step 5: Create ConvertFormToEntityValues() Method
- Input: `_customFieldValues`, `_customFieldBoolValues`, `_customFields`
- Output: `List<EntityValue>`
- Logic:
  - For each custom field
  - Determine data type from `EntityField.DataType`
  - Create EntityValue with appropriate typed column (TextValue, IntegerValue, etc.)
  - If multi-value and input is comma-separated or array, create multiple rows with ValueIndex

### Step 6: Create ConvertEntityValuesToForm() Method
- Input: `List<EntityValue>`, `List<EntityField>`
- Output: Populate `_customFieldValues`, `_customFieldBoolValues`
- Reverse of ConvertFormToEntityValues()
- Handle multi-value consolidation (multiple rows → single form field or array)

### Step 7: Error Handling & Migration Safety
- Wrap EntityValueService calls in try-catch
- If EntityValue save fails, optionally fall back to Entity.Settings save
- Log migration status and warnings
- Consider cleanup task: "Remove orphaned EntityValue rows after Settings migration"

## Service Contract Assumptions

The following methods are expected to exist in `IEntityValueService`:
```csharp
// Get all values for an entity
Task<List<EntityValue>> GetEntityValuesAsync(int entityId);

// Replace all custom field values for an entity
Task ReplaceEntityFieldValuesAsync(int entityId, List<EntityValue> values);

// Delete all values for an entity
Task DeleteEntityValuesAsync(int entityId);
```

## Testing Strategy

1. **Create Mode:**
   - Create new entity with custom fields
   - Verify EntityValue rows are created with correct data types
   - Verify Entity.Settings is empty or populated (depending on migration strategy)

2. **Edit Mode:**
   - Load existing entity with EntityValue data
   - Verify UI fields populate correctly from EntityValue table
   - Modify field values and save
   - Verify EntityValue table is updated

3. **Type Conversion:**
   - Test each EntityDataType (Text, Integer, Boolean, Decimal, Date, DateTime, Guid)
   - Verify correct column is populated in EntityValue table

4. **Multi-Value Fields:**
   - Create field with IsMultiValue = true
   - Save multiple values
   - Verify ValueIndex increments correctly
   - Load entity and verify all values are retrieved

5. **Migration Scenario:**
   - Existing entities with JSON in Settings
   - Load in edit mode and save with new logic
   - Verify EntityValue rows are created and Settings can be cleared

## Backward Compatibility

- Existing entities may still have data in Entity.Settings
- Load logic should check both Settings and EntityValue table
- Display preference: EntityValue table takes precedence
- Save logic: Write to EntityValue table (can optionally clear Settings after successful save)

## Database Impact

- Existing: Entity.Settings continues to exist
- New: EntityValue table used for all custom field storage
- Index strategy: Uses created indexes on (FieldId, ValueColumn) for searches
- No migration of existing Settings data in this phase (manual or later task)

## Next Phase (Phase 4)

- Implement EntityList.razor/Index.razor to display EntityValue data
- Add filtering/searching by EntityValue columns
- Create hierarchical tree rendering using EntityValue parent relationships
