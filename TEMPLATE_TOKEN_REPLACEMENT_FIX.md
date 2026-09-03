# Template Token Replacement Fix - EntityValue Support

## Issues Fixed

### 1. **Syntax Error - Extra Closing Brace**
**Status:** ✅ Fixed

Removed the extra `}` at the end of Index.razor file that was causing compilation errors.

### 2. **Token Replacement Not Working**
**Status:** ✅ Fixed

Custom field tokens like `[Field:address]`, `[Field:st]`, `[Field:zipcode]` were not being replaced because:
- The template engine was looking for values in `Entity.Settings` JSON
- But custom fields are now stored in the normalized `GIBS_EntityValue` table
- Values in Settings are empty "{}"

## Solution Implemented

### Enhanced TemplateEngine with EntityValue Support

Updated `Shared\Helpers\TemplateEngine.cs` with:

1. **Two Render Overloads:**
   - **Legacy:** `Render(template, entity, fields)` - Uses Entity.Settings JSON
   - **New:** `Render(template, entity, fields, entityValues)` - Uses EntityValue records

2. **Smart Value Extraction:**
   ```csharp
   private static string ExtractStringValue(EntityValue ev)
   {
	   if (ev.TextValue != null) return ev.TextValue;
	   if (ev.IntegerValue.HasValue) return ev.IntegerValue.ToString();
	   if (ev.LongValue.HasValue) return ev.LongValue.ToString();
	   if (ev.DecimalValue.HasValue) return ev.DecimalValue.ToString();
	   if (ev.BooleanValue.HasValue) return ev.BooleanValue.ToString();
	   if (ev.DateValue.HasValue) return ev.DateValue.Value.ToString("yyyy-MM-dd");
	   if (ev.DateTimeValue.HasValue) return ev.DateTimeValue.Value.ToString("yyyy-MM-dd HH:mm:ss");
	   if (ev.GuidValue.HasValue) return ev.GuidValue.ToString();
	   return string.Empty;
   }
   ```

3. **Multi-Value Support:**
   - When a field has multiple EntityValue records (ValueIndex > 0)
   - Values are concatenated with comma separator: `"value1, value2, value3"`

### Updated Index.razor Component

1. **Injected IEntityValueService:**
   ```razor
   @inject IEntityValueService EntityValueService
   ```

2. **Added EntityValues Map:**
   ```csharp
   private Dictionary<int, List<EntityValue>> _entityValuesMap = new();
   ```

3. **Load EntityValue Data:**
   ```csharp
   foreach (var record in _featuredRecords)
   {
	   var entityValues = await EntityValueService.GetAllEntityValuesAsync(record.EntityId);
	   _entityValuesMap[record.EntityId] = entityValues ?? new List<EntityValue>();
   }
   ```

4. **Use New Render Overload:**
   ```csharp
   var entityValues = _entityValuesMap.ContainsKey(record.EntityId) 
	   ? _entityValuesMap[record.EntityId] 
	   : new List<EntityValue>();

   var rendered = TemplateEngine.Render(templateContent, record, _entityFields, entityValues);
   ```

### Created _Imports.razor

Added `Client\Modules\_Imports.razor` to expose module namespaces to all Razor components:

```razor
@using GIBS.Module.Entity.Models
@using GIBS.Module.Entity.Services
@using GIBS.Module.Entity.Helpers
@using GIBS.Module.Entity.Interfaces
```

## How Token Replacement Works Now

### Template Example
```html
<div class="address">
  <strong>[Name]</strong>
  <p>[Field:address], [Field:st] [Field:zipcode]</p>
</div>
```

### Processing Steps

1. **Load Featured Entities:**
   - Query all entities with `IsFeatured=true`, `IsEnabled=true`, `IsPublished=true`
   - Sort by `SortOrder`

2. **Load Entity Fields:**
   - Get field metadata for all entity types

3. **Load EntityValue Data:**
   - For each featured entity, load all EntityValue records
   - Store in `_entityValuesMap[entityId]`

4. **Render Template:**
   ```
   Input:  "[Field:address], [Field:st] [Field:zipcode]"
   Data:   EntityValue records with TextValue populated
   Output: "123 Main St, CA 90210"
   ```

5. **Token Replacement Process:**
   - Build dictionary from EntityValue records: `{FieldId → "value"}`
   - Look up field by Key (e.g., "address")
   - Replace token with value from dictionary
   - For multi-value fields, join with comma

## Supported Data Types

The template engine automatically handles:

| EntityValue Column | Output Format | Example |
|--------------------|---------------|---------|
| TextValue | As-is | "123 Main St" |
| IntegerValue | ToString | "42" |
| LongValue | ToString | "9223372036854775807" |
| DecimalValue | ToString | "123.45" |
| BooleanValue | ToString | "True" / "False" |
| DateValue | yyyy-MM-dd | "2024-01-15" |
| DateTimeValue | yyyy-MM-dd HH:mm:ss | "2024-01-15 14:30:00" |
| GuidValue | ToString | "550e8400-e29b-41d4-a716-446655440000" |

## Example Output

### Template
```html
<div class="featured-item">
  <h3>[Name]</h3>
  <p>[Field:address]</p>
  <p>[Field:city], [Field:state] [Field:zip]</p>
  <p>[Field:phone]</p>
</div>
```

### Rendered Output
```html
<div class="featured-item">
  <h3>Acme Corporation</h3>
  <p>123 Main Street</p>
  <p>San Francisco, CA 94105</p>
  <p>(555) 123-4567</p>
</div>
```

## Error Handling

- Missing field values default to empty string (no error)
- Missing EntityValue data treated as empty list
- Template rendering is defensive with null checks
- Render method validates template and entity before processing

## Performance

- EntityValue data loaded once during component initialization
- Efficient dictionary lookups for token replacement
- Uses StringBuilder for efficient string operations
- Deduplicates entity fields to reduce processing

## Backward Compatibility

- Legacy Settings-based templates still work via `Render(template, entity, fields)` overload
- New Razor components can use either overload
- Automatic fallback to Settings if EntityValue data not available

## Testing Template Tokens

### Expected Behavior
1. Template: `[Name]` → Output: Entity name
2. Template: `[Field:address]` → Output: Value from first EntityValue record with matching field key
3. Template: `[Field:tags]` (multi-value) → Output: "value1, value2, value3"
4. Template: `[HtmlContent:description]` → Output: HTML from EntityField definition
5. Template: `[Status]` → Output: Entity.Status value

## Files Modified

| File | Changes |
|------|---------|
| `Shared\Helpers\TemplateEngine.cs` | Added new Render overload with EntityValue support |
| `Client\Modules\GIBS.Module.Entity\Index.razor` | Load EntityValue data, use new render overload, fix syntax |
| `Client\Modules\_Imports.razor` | NEW: Global Razor imports for module services |

## Build Status

✅ **Build Successful** - All compilation errors resolved

## Notes

- The template engine now supports BOTH legacy Settings-based values AND new EntityValue-based values
- Tokens are case-insensitive for field keys (thanks to `StringComparer.OrdinalIgnoreCase`)
- Multi-value fields are automatically concatenated with comma separator
- Date/DateTime values are formatted to ISO standards for consistency

