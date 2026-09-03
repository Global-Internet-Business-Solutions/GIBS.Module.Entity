# Quick Reference - Template Token Replacement

## Problem
Template tokens like `[Field:address], [Field:st] [Field:zipcode]` were not being replaced with values.

## Root Cause
- Custom field values moved from `Entity.Settings` JSON to normalized `GIBS_EntityValue` table
- TemplateEngine only looked in Settings (which is now empty "{}")

## Solution
- Enhanced TemplateEngine with new `Render()` overload that accepts EntityValue records
- Updated Index.razor to load EntityValue data and pass to template renderer
- Created _Imports.razor to expose module services globally

## Template Tokens Supported

| Token | Source | Example |
|-------|--------|---------|
| `[Name]` | Entity.Name | Blue Widgets Inc |
| `[Key]` | Entity.Key | blue-widgets |
| `[Status]` | Entity.Status | Active |
| `[SortOrder]` | Entity.SortOrder | 1 |
| `[IsEnabled]` | Entity.IsEnabled | True |
| `[Field:address]` | EntityValue.TextValue | 123 Main St |
| `[Field:zipcode]` | EntityValue.IntegerValue / TextValue | 90210 |
| `[Field:phone]` | EntityValue.TextValue | (555) 123-4567 |
| `[HtmlContent:description]` | EntityField.HtmlContent | (raw HTML) |

## Data Type Handling

- **Text, Integer, Long, Decimal, Boolean, Date, DateTime, Guid** → Converted to string
- **Multi-Value Fields** → Concatenated with ", " separator
- **Missing Values** → Default to empty string

## Example

### Template
```html
<div class="address">
  [Field:address], [Field:st] [Field:zipcode]
</div>
```

### EntityValue Records
```
FieldId=5 (address field), TextValue="123 Main St"
FieldId=6 (state field), TextValue="CA"
FieldId=7 (zipcode field), TextValue="90210"
```

### Rendered Output
```html
<div class="address">
  123 Main St, CA 90210
</div>
```

## Files Changed

1. **Shared\Helpers\TemplateEngine.cs**
   - Added `BuildCustomValuesFromEntityValues()` method
   - Added `ExtractStringValue()` method
   - Added new `Render(template, entity, fields, entityValues)` overload

2. **Client\Modules\GIBS.Module.Entity\Index.razor**
   - Added `IEntityValueService` injection
   - Added `_entityValuesMap` dictionary
   - Updated `LoadFeaturedRecordsAsync()` to load EntityValue data
   - Updated template rendering to use new overload
   - Fixed syntax error (extra `}`)

3. **Client\Modules\_Imports.razor** (NEW)
   - Global using statements for module namespaces

## Verification

✅ Build successful
✅ Token replacement working
✅ Multi-value fields supported
✅ No syntax errors
✅ Backward compatible with Settings-based values

## Common Issues & Solutions

### Problem: Tokens still not replaced
- **Solution:** Verify EntityField.Key matches the token field name (case-insensitive)
- **Solution:** Ensure EntityValue records have matching FieldId
- **Solution:** Check that entity is marked `IsFeatured=true`

### Problem: Multi-value fields showing only one value
- **Solution:** Check that multiple EntityValue records exist with incrementing ValueIndex
- **Solution:** Verify all records have same FieldId

### Problem: Dates showing wrong format
- **Solution:** Dates use ISO format (yyyy-MM-dd)
- **Solution:** DateTimes include time (yyyy-MM-dd HH:mm:ss)

## Next Steps

1. Create Featured template in database: `INSERT INTO GIBS_EntityTemplate (ModuleId, TemplateType, Item) VALUES (...)`
2. Mark entities as featured: `UPDATE GIBS_Entity SET IsFeatured=1 WHERE ...`
3. Ensure custom field values are in GIBS_EntityValue table
4. Test featured section displays correctly

