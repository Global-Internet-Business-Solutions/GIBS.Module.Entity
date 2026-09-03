# Featured Records Display Implementation

## Overview

Implemented dynamic featured records display on the Index.razor module page. When `_enableFeatured` is true, the page displays featured entities using a customizable Featured template type with full token replacement support.

## Features Implemented

### 1. **Featured Records Loading**
- Loads entities marked as `IsFeatured = true`, `IsEnabled = true`, and `IsPublished = true`
- Sorted by `SortOrder` for consistent display order
- Only loads if a Featured template is configured

### 2. **Template System**
- Uses the `EntityTemplate` model with `TemplateType = "Featured"`
- Supports multiple template sections:
  - **Header**: Rendered once at the top
  - **Item**: Main template for even-indexed records
  - **Alternate**: Optional alternate template for odd-indexed records (e.g., alternating layouts)
  - **Separator**: HTML/content between records
  - **Footer**: Rendered once at the bottom

### 3. **Token Replacement**
Uses the `TemplateEngine.Render()` helper to replace tokens in templates:

| Token | Source |
|-------|--------|
| `[Name]` | Entity.Name |
| `[Key]` | Entity.Key |
| `[Status]` | Entity.Status |
| `[SortOrder]` | Entity.SortOrder |
| `[IsEnabled]` | Entity.IsEnabled |
| `[Field:FieldKey]` | Custom field value (from Entity.Settings JSON) |
| `[HtmlContent:FieldKey]` | HTML content from EntityField definition |

### 4. **Entity Fields Integration**
- Automatically loads all fields for entity types represented in featured records
- Deduplicates field definitions to avoid redundant processing
- Supports both standard fields and custom fields

## Code Changes

### Index.razor Updates

#### Imports
```razor
@using GIBS.Module.Entity.Helpers
@inject IEntityTemplateService EntityTemplateService
```

#### Private Fields
```csharp
private List<Entity> _featuredRecords = new();
private EntityTemplate _featuredTemplate;
private List<EntityField> _entityFields = new();
```

#### Featured Content Rendering
```razor
@if (_enableFeatured && _featuredRecords != null && _featuredRecords.Any())
{
	<div class="featured-section">
		@if (!string.IsNullOrEmpty(_featuredTemplate?.Header))
		{
			<div class="featured-header">@((MarkupString)_featuredTemplate.Header)</div>
		}

		<div class="featured-content">
			@{
				int count = 0;
				foreach (var record in _featuredRecords)
				{
					// Use Item for even indexes, Alternate for odd indexes
					var templateContent = (count % 2 == 0 && !string.IsNullOrEmpty(_featuredTemplate?.Item)) 
						? _featuredTemplate.Item 
						: (!string.IsNullOrEmpty(_featuredTemplate?.Alternate) 
							? _featuredTemplate.Alternate 
							: _featuredTemplate?.Item);

					if (!string.IsNullOrEmpty(templateContent))
					{
						var rendered = TemplateEngine.Render(templateContent, record, _entityFields);
						<div class="featured-item">@((MarkupString)rendered)</div>
					}

					// Add separator between records (not after last)
					if (!string.IsNullOrEmpty(_featuredTemplate?.Separator) && count < _featuredRecords.Count - 1)
					{
						<div class="featured-separator">@((MarkupString)_featuredTemplate.Separator)</div>
					}

					count++;
				}
			}
		</div>

		@if (!string.IsNullOrEmpty(_featuredTemplate?.Footer))
		{
			<div class="featured-footer">@((MarkupString)_featuredTemplate.Footer)</div>
		}
	</div>
}
```

#### Loading Logic
```csharp
private async Task LoadFeaturedRecordsAsync()
{
	try
	{
		// Get the Featured template
		var templates = await EntityTemplateService.GetTemplatesAsync(ModuleState.ModuleId);
		_featuredTemplate = templates?.FirstOrDefault(t => t.TemplateType == "Featured");

		if (_featuredTemplate == null)
		{
			// No featured template configured
			return;
		}

		// Get featured entities ordered by SortOrder
		var entities = await EntityService.GetEntitysAsync(ModuleState.ModuleId);
		_featuredRecords = entities
			.Where(e => e.IsFeatured && e.IsEnabled && e.IsPublished)
			.OrderBy(e => e.SortOrder)
			.ToList();

		if (_featuredRecords.Any())
		{
			// Get all entity fields for the featured entity types
			var entityTypeIds = _featuredRecords.Select(e => e.EntityTypeId).Distinct();
			foreach (var typeId in entityTypeIds)
			{
				var fields = await EntityFieldService.GetFieldsAsync(typeId, ModuleState.ModuleId);
				_entityFields.AddRange(fields ?? new List<EntityField>());
			}

			// Remove duplicates
			_entityFields = _entityFields.GroupBy(f => f.FieldId).Select(g => g.First()).ToList();
		}
	}
	catch (Exception ex)
	{
		await logger.LogError(ex, "Error Loading Featured Records {Error}", ex.Message);
	}
}
```

## Example Usage

### Step 1: Create a Featured Template
Create an EntityTemplate record in the database:
```
TemplateType: "Featured"
Header: "<h2>Featured Items</h2>"
Item: "<div class='featured-item'><h3>[Name]</h3><p>[Field:description]</p></div>"
Alternate: "<div class='featured-item alt'><h4>[Name]</h4>[Field:summary]</div>"
Separator: "<hr />"
Footer: "<p>End of featured items</p>"
```

### Step 2: Configure Module Settings
Set `EnableFeatured` to `true` in the module settings.

### Step 3: Mark Entities as Featured
Set `IsFeatured = true` on entities you want to display, along with:
- `IsEnabled = true`
- `IsPublished = true`
- Set `SortOrder` to control display order

### Step 4: Display
When the Index page loads:
1. Loads the Featured template
2. Queries for featured entities
3. Loads all entity fields for token replacement
4. Renders the template with token replacement
5. Displays header, items with alternating templates, separators, and footer

## CSS Classes

The implementation uses the following CSS classes for styling:
- `.featured-section` - Main container
- `.featured-header` - Header section
- `.featured-content` - Items container
- `.featured-item` - Individual item
- `.featured-separator` - Separator between items
- `.featured-footer` - Footer section

You can style these in your module's CSS file.

## Error Handling

- If no Featured template is found, the featured section simply doesn't display
- If featured entity loading fails, an error is logged but doesn't crash the page
- Missing entity fields default to empty strings in templates
- Template rendering is defensive with null checks throughout

## Performance Considerations

- Featured records are loaded once during initialization
- Duplicate entity fields are deduplicated to reduce processing
- Uses LINQ efficiently to filter and organize data
- Template engine uses StringBuilder for efficient string replacement

## Dependencies

- `IEntityService.GetEntitysAsync()` - Load entities
- `IEntityTemplateService.GetTemplatesAsync()` - Load template
- `IEntityFieldService.GetFieldsAsync()` - Load field metadata
- `TemplateEngine.Render()` - Token replacement
- `ILogger` - Error logging

## Files Modified

- `Client\Modules\GIBS.Module.Entity\Index.razor` - Complete featured section implementation

## Build Status

✅ **Build Successful** - All compilation errors resolved

