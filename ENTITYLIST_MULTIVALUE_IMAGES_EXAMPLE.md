# EntityList.razor - Display Multiple Images Example

## How to Display Multiple Gallery Images in the Entity List

### Current Implementation (EntityList.razor)
```razor
@if (field.EditorType == EntityEditorType.ImageUpload)
{
	<td>
		@{
			var imagePath = GetManagerFieldValue(entity, field.FieldId);
			if (!string.IsNullOrEmpty(imagePath))
			{
				<img src="@imagePath" alt="@field.Label" style="max-width: 100px; max-height: 60px;" />
			}
		}
	</td>
}
```

### Enhanced for Multi-Value Display
```razor
@if (field.EditorType == EntityEditorType.ImageUpload)
{
	<td>
		@{
			var value = GetManagerFieldValue(entity, field.FieldId);
			var imagePaths = GetAllImagePathsFromValue(value);

			if (imagePaths.Count > 0)
			{
				if (imagePaths.Count == 1)
				{
					<img src="@imagePaths[0]" alt="@field.Label" style="max-width: 100px; max-height: 60px;" />
				}
				else
				{
					<div class="image-gallery-preview">
						@foreach (var imagePath in imagePaths.Take(3))
						{
							<img src="@imagePath" alt="@field.Label" style="max-width: 60px; max-height: 60px; margin-right: 5px;" />
						}
						@if (imagePaths.Count > 3)
						{
							<span class="badge badge-primary">+@(imagePaths.Count - 3)</span>
						}
					</div>
				}
			}
		}
	</td>
}
```

## Required Helper Methods for EntityList.razor

Add these methods to EntityList.razor:

```csharp
/// <summary>
/// Extract all image paths from a field value
/// Handles JSON array format (multi-value) and single JSON object (single-value)
/// </summary>
private List<string> GetAllImagePathsFromValue(string value)
{
	var paths = new List<string>();

	if (string.IsNullOrEmpty(value))
		return paths;

	try
	{
		// Try array format first (multi-value)
		if (value.StartsWith("[") && value.EndsWith("]"))
		{
			var imageArray = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(value);
			if (imageArray != null)
			{
				foreach (var imageObj in imageArray)
				{
					if (imageObj != null && imageObj.TryGetValue("filePath", out var filePath))
					{
						var path = filePath?.ToString();
						if (!string.IsNullOrEmpty(path))
							paths.Add(path);
					}
				}
			}
			return paths;
		}

		// Try single object format
		if (value.StartsWith("{") && value.EndsWith("}"))
		{
			var imageData = JsonSerializer.Deserialize<Dictionary<string, object>>(value);
			if (imageData != null && imageData.TryGetValue("filePath", out var filePath))
			{
				var path = filePath?.ToString();
				if (!string.IsNullOrEmpty(path))
					paths.Add(path);
			}
			return paths;
		}
	}
	catch
	{
		// If JSON parsing fails, treat as legacy path-only value
	}

	// Fallback: treat as direct path (backward compatibility)
	if (!string.IsNullOrEmpty(value))
		paths.Add(value);

	return paths;
}

/// <summary>
/// Extract all FileIds from a field value for referential integrity checks
/// </summary>
private List<int> GetAllImageFileIdsFromValue(string value)
{
	var fileIds = new List<int>();

	if (string.IsNullOrEmpty(value))
		return fileIds;

	try
	{
		// Try array format first (multi-value)
		if (value.StartsWith("[") && value.EndsWith("]"))
		{
			var imageArray = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(value);
			if (imageArray != null)
			{
				foreach (var imageObj in imageArray)
				{
					if (imageObj != null && imageObj.TryGetValue("fileId", out var fileIdObj))
					{
						if (int.TryParse(fileIdObj?.ToString() ?? "0", out var id) && id > 0)
							fileIds.Add(id);
					}
				}
			}
			return fileIds;
		}

		// Try single object format
		if (value.StartsWith("{") && value.EndsWith("}"))
		{
			var imageData = JsonSerializer.Deserialize<Dictionary<string, object>>(value);
			if (imageData != null && imageData.TryGetValue("fileId", out var fileIdObj))
			{
				if (int.TryParse(fileIdObj?.ToString() ?? "0", out var id) && id > 0)
					fileIds.Add(id);
			}
			return fileIds;
		}
	}
	catch
	{
		// If JSON parsing fails, return empty list
	}

	return fileIds;
}
```

## CSS for Gallery Preview (Optional)

Add to your module's stylesheet:

```css
.image-gallery-preview {
	display: flex;
	align-items: center;
	gap: 5px;
}

.image-gallery-preview img {
	border-radius: 3px;
	box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.image-gallery-preview .badge {
	background-color: #007bff;
	color: white;
	padding: 2px 6px;
	font-size: 12px;
	border-radius: 3px;
}
```

## Lightbox Enhancement

For better UX with multiple images, consider integrating a lightbox library:

```html
<!-- Add to EntityList.razor head or module imports -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/lightbox2@2.11.3/dist/css/lightbox.min.css">

<!-- In the image rendering loop -->
@foreach (var imagePath in imagePaths)
{
	<a href="@imagePath" data-lightbox="entity-gallery-@entity.EntityId" data-title="Gallery Image">
		<img src="@imagePath" alt="Gallery" style="max-width: 100px; max-height: 60px; cursor: pointer;" />
	</a>
}

<!-- Add to module layout -->
<script src="https://cdn.jsdelivr.net/npm/lightbox2@2.11.3/dist/js/lightbox.min.js"></script>
```

## Performance Notes

- For lists with many entities and many images per entity, consider:
  - Pagination to limit entities shown per page
  - Lazy loading images with `loading="lazy"` attribute
  - Using thumbnail URLs if available: `/api/file/{fileId}?size=thumbnail`
  - Caching the parsed image path results

## Example Result

### Single Image Field Display:
```
┌─────────────┐
│ [Image]     │
└─────────────┘
```

### Multi-Value Field Display:
```
┌──────────────────────────────────┐
│ [Image] [Image] [Image] +2       │
└──────────────────────────────────┘
```
Click any image to open in lightbox gallery view
