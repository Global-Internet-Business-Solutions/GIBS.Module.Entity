# ImageUpload Multi-Value Support Guide

## Overview
The ImageUpload field type now supports both **single and multiple images** per entity. The system stores both the **FileId** (for referential integrity with Oqtane) and **FilePath** (for fast template rendering + fallback).

## Data Storage Format

### Single-Value ImageUpload Field
```json
{
  "fileId": 123,
  "filePath": "/path/to/image.jpg"
}
```
Stored as a single EntityValue record with `ValueIndex=0`

### Multi-Value ImageUpload Field
```json
[
  {"fileId": 123, "filePath": "/path/to/image1.jpg"},
  {"fileId": 124, "filePath": "/path/to/image2.jpg"},
  {"fileId": 125, "filePath": "/path/to/image3.jpg"}
]
```
Stored as multiple EntityValue records:
- EntityValue 1: `ValueIndex=0, TextValue={"fileId": 123, ...}`
- EntityValue 2: `ValueIndex=1, TextValue={"fileId": 124, ...}`
- EntityValue 3: `ValueIndex=2, TextValue={"fileId": 125, ...}`

## Configuration for Multi-Value Images

### 1. Create/Edit EntityField with these properties:
```
Name: "Gallery Images"
Key: "gallery_images"
DataType: String (required for JSON storage)
EditorType: ImageUpload (required for image upload UI)
IsMultiValue: true ✓ (IMPORTANT: Enable this!)
MinimumValues: 1 (optional)
MaximumValues: 10 (optional - limits how many images can be uploaded)
```

### 2. Configure Module Settings:
- **FileFolderId**: Which folder in Oqtane's file manager to upload to
- **ImageThumbWidth**: Width for thumbnails
- **ImageThumbHeight**: Height for thumbnails
- **ImageMaxWidth**: Max width for stored images
- **ImageMaxHeight**: Max height for stored images
- **ShowPhotoTitle**: Display image titles

## UI Behavior

### Single-Value Field
- Shows form label and file upload button
- When an image is uploaded, displays a preview (single image)
- "Remove Image" button clears the selection

### Multi-Value Field (IsMultiValue=true)
- Shows form label and file upload button
- Each uploaded image is added to the gallery previews
- **Individual delete buttons** (X) appear on each image for removal
- Users can upload multiple images sequentially

## Usage in Templates

### Render Single Image
```html
<img src="[Field:gallery_images]" alt="Gallery" />
```

### Render Multiple Images (in featured content, lists, etc.)
In your featured template or list template rendering logic:

```csharp
// In TemplateEngine or Index.razor
var imageField = entityValues.FirstOrDefault(ev => ev.FieldId == galleryFieldId);
var imagePaths = ExtractAllImagePathsFromField(imageField);

foreach(var imagePath in imagePaths)
{
	<img src="@imagePath" alt="Gallery" class="gallery-image" />
}
```

### Get FileIds for Referential Checking
```csharp
// Check if a file is still used in any entity
var fileId = 123;
var usedInEntities = entityValues
	.Where(ev => ev.TextValue.Contains($"\"fileId\":{fileId}"))
	.Select(ev => ev.EntityId)
	.ToList();
```

## Database Schema
Images are stored in the `GIBS_EntityValue` table:

```
EntityValueId | EntityId | FieldId | ValueIndex | TextValue
--------------|----------|---------|------------|---------------------------------------------
1001          | 5        | 10      | 0          | {"fileId": 123, "filePath": "/photo1.jpg"}
1002          | 5        | 10      | 1          | {"fileId": 124, "filePath": "/photo2.jpg"}
```

## API Operations

### Add/Update with Multiple Images
PUT `/api/entityvalue/entity/{entityId}`
```json
{
  "entityId": 5,
  "values": [
	{
	  "entityId": 5,
	  "fieldId": 10,
	  "valueIndex": 0,
	  "textValue": "{\"fileId\": 123, \"filePath\": \"/photo1.jpg\"}"
	},
	{
	  "entityId": 5,
	  "fieldId": 10,
	  "valueIndex": 1,
	  "textValue": "{\"fileId\": 124, \"filePath\": \"/photo2.jpg\"}"
	}
  ]
}
```

### Query All Images for an Entity
GET `/api/entityvalue/entity/{entityId}`
Returns all EntityValue records, group by FieldId to find related images

## Fallback Behavior

If a file is deleted from Oqtane's file manager:
1. The stored `filePath` is used to display a placeholder or error
2. The `fileId` allows you to detect stale references
3. Can implement optional cleanup routine to remove orphaned values

## Code Examples

### In EntityEdit.razor - Accessing Images
```csharp
// Get all image paths for preview
var imagePaths = GetAllImagePathsFromField(fieldId);

// Get file IDs for referential checks
var imageFileIds = GetAllImageFileIdsFromField(fieldId);

// Remove specific image
await RemoveImageFromField(fieldId, imagePath);

// Clear all images
ClearImage(fieldId);
```

### In Templates/Lists - Displaying Multiple Images
```html
@foreach (var imagePath in GetAllImagePathsFromField(fieldId))
{
	<img src="@imagePath" alt="Image" class="gallery-thumbnail" />
}
```

## Benefits of This Approach

✅ **Both Values Stored**: FileId ensures data integrity, FilePath enables fast rendering  
✅ **Flexible**: One field supports 1-many images based on IsMultiValue setting  
✅ **Backward Compatible**: Legacy path-only format still works  
✅ **JSON Flexible**: Can extend with additional metadata (alt text, captions, etc.)  
✅ **Type Safe**: Stored in TextValue column, properly typed in EntityValue model  
✅ **Performance**: Paths cached, no runtime lookups needed for rendering  

## Migration Notes

- Existing single-image fields continue to work unchanged
- When converting a field to `IsMultiValue=true`, existing single images are automatically wrapped in array format on save
- Test templates with `GetAllImagePathsFromField()` helper for multi-value support
