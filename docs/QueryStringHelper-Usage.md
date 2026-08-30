# QueryString Helper Usage Guide

## Overview
The `QueryStringHelper` class provides safe methods to access query string parameters from `PageState.QueryString` without throwing exceptions when keys are missing.

## Location
`Shared/Helpers/QueryStringHelper.cs`

## Available Methods

### 1. GetValueOrDefault
Gets a string value with a default fallback.

```csharp
string value = QueryStringHelper.GetValueOrDefault(PageState.QueryString, "key", "default");
```

### 2. TryGetInt
Safely tries to parse an integer value.

```csharp
// Basic validation - checks if key exists and is valid integer
if (QueryStringHelper.TryGetInt(PageState.QueryString, "entitytypeid", out int entityTypeId))
{
	// Use entityTypeId
}
else
{
	// Handle missing or invalid parameter
	AddModuleMessage(Localizer["Error.MissingEntityType"], MessageType.Error);
	return;
}

// Additional validation - also check if value is greater than 0 (for IDs)
if (QueryStringHelper.TryGetInt(PageState.QueryString, "entitytypeid", out int entityTypeId) && entityTypeId > 0)
{
	// Use entityTypeId - guaranteed to be valid
}
else
{
	AddModuleMessage(Localizer["Error.MissingEntityType"], MessageType.Error);
	return;
}

// Or use a shorter pattern with OR condition
if (!QueryStringHelper.TryGetInt(PageState.QueryString, "entitytypeid", out int entityTypeId) || entityTypeId <= 0)
{
	AddModuleMessage(Localizer["Error.MissingEntityType"], MessageType.Error);
	return;
}
// entityTypeId is now guaranteed to be > 0
```

### 3. GetIntOrDefault
Gets an integer value with a default fallback.

```csharp
int pageNumber = QueryStringHelper.GetIntOrDefault(PageState.QueryString, "page", 1);
```

### 4. HasValue
Checks if a parameter exists and has a non-empty value.

```csharp
if (QueryStringHelper.HasValue(PageState.QueryString, "search"))
{
	string searchTerm = PageState.QueryString["search"];
}
```

### 5. TryGetBool
Safely tries to parse a boolean value.

```csharp
if (QueryStringHelper.TryGetBool(PageState.QueryString, "isActive", out bool isActive))
{
	// Use isActive
}
```

### 6. GetBoolOrDefault
Gets a boolean value with a default fallback.

```csharp
bool showAll = QueryStringHelper.GetBoolOrDefault(PageState.QueryString, "showall", false);
```

## Examples

### Before (Unsafe)
```csharp
protected override async Task OnInitializedAsync()
{
	// This throws KeyNotFoundException if entitytypeid is missing
	_entityTypeId = int.Parse(PageState.QueryString["entitytypeid"]);
}
```

### After (Safe)
```csharp
protected override async Task OnInitializedAsync()
{
	// Validates that entitytypeid exists, is a valid integer, AND is greater than 0
	if (!QueryStringHelper.TryGetInt(PageState.QueryString, "entitytypeid", out _entityTypeId) || _entityTypeId <= 0)
	{
		AddModuleMessage(Localizer["Error.MissingEntityType"], MessageType.Error);
		return;
	}

	// Safe to use _entityTypeId here - guaranteed to be valid
}

// In save methods, validate null objects returned from async calls
private async Task SaveField()
{
	if (_fieldId != -1)
	{
		field = await EntityFieldService.GetFieldAsync(_fieldId, ModuleState.ModuleId);

		// Always check if the entity was found
		if (field == null)
		{
			AddModuleMessage(Localizer["Error.FieldNotFound"], MessageType.Error);
			return;
		}
	}

	// Safe to use field here
}
```

## Updated Files
The following files now use `QueryStringHelper`:

1. `Client/Modules/GIBS.Module.Entity/FieldsIndex.razor`
2. `Client/Modules/GIBS.Module.Entity/FieldsEdit.razor`
3. `Client/Modules/GIBS.Module.Entity/FieldOptionsIndex.razor`
4. `Client/Modules/GIBS.Module.Entity/FieldOptionsEdit.razor`

## Best Practices

1. **Always validate required parameters**: Use `TryGetInt` or similar methods and handle the case when the parameter is missing.

2. **Validate ID values are greater than zero**: When dealing with database IDs, always check that the parsed value is > 0.
   ```csharp
   if (!QueryStringHelper.TryGetInt(PageState.QueryString, "id", out var id) || id <= 0)
   {
       // Handle invalid ID
       return;
   }
   ```

3. **Check for null objects from async calls**: When retrieving entities by ID, always verify the result isn't null before using it.
   ```csharp
   var entity = await Service.GetAsync(id, moduleId);
   if (entity == null)
   {
       AddModuleMessage(Localizer["Error.NotFound"], MessageType.Error);
       return;
   }
   ```

4. **Use default values for optional parameters**: Use `GetIntOrDefault`, `GetBoolOrDefault`, etc. for optional parameters.

5. **Show user-friendly error messages**: When required parameters are missing, display a helpful error message using `AddModuleMessage`.

6. **Return early on validation failure**: After showing an error message, return immediately to prevent further execution with invalid data.

## Benefits

- ✅ No more `KeyNotFoundException` errors
- ✅ No more `NullReferenceException` errors from invalid IDs
- ✅ Built-in type conversion with validation
- ✅ Validates that ID values are greater than zero
- ✅ Null-safety checks for async entity retrieval
- ✅ Consistent error handling across all pages
- ✅ Easier to test and maintain
- ✅ Better user experience with proper error messages
