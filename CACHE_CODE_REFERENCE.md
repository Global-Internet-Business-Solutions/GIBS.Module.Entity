# Cache Control - Code Reference Guide

## 📍 Where Cache Methods Are Defined

**File:** `Client\Modules\GIBS.Module.Entity\Index.razor`  
**Lines:** ~133-145 (in @code block)  
**Scope:** Public static methods (can be called from anywhere)

```csharp
/// <summary>
/// Public static method to clear featured cache for a specific module.
/// Call this from EntityEdit or other components when featured data changes.
/// </summary>
public static void ClearFeaturedCache(int moduleId)
{
	string cacheKey = $"FeaturedData_{moduleId}";
	if (_cache.ContainsKey(cacheKey))
	{
		_cache.Remove(cacheKey);
	}
}

/// <summary>
/// Public static method to clear all featured caches.
/// Use when you want to force refresh across all modules.
/// </summary>
public static void ClearAllFeaturedCaches()
{
	_cache.Clear();
}
```

---

## 🔗 Where Calls Are Made

### Current Implementation (Option 1)

**File:** `Client\Modules\GIBS.Module.Entity\EntityEdit.razor`  
**Lines:** 898-911 (in Save() method)  
**Scope:** Private method called after saving featured entity

```csharp
// Save custom field values to EntityValue table
if (savedEntity != null)
{
	await SaveCustomFieldValuesAsync(savedEntity.EntityId);

	// Clear featured cache if this entity is featured
	if (bool.Parse(_isFeatured))
	{
		Index.ClearFeaturedCache(ModuleState.ModuleId);
		await logger.LogDebug($"Cleared featured cache for module {ModuleState.ModuleId}");
	}
}
```

---

## 📋 How to Call from Other Files

### From Any Blazor Component

```csharp
@using GIBS.Module.Entity  // Add namespace

// In @code block or event handler
Index.ClearFeaturedCache(moduleId);
```

**Example: From a custom admin component**

```csharp
@namespace MyApp.Admin
@using GIBS.Module.Entity

@code {
	async Task RefreshFeatured()
	{
		Index.ClearFeaturedCache(42);  // Clear module 42
		AddModuleMessage("Featured refreshed");
	}
}
```

### From a Server-Side Service

Since these are static public methods in a Blazor component (client-side), you **cannot** call them from server code directly.

**Option A: Make them callable from server**

Create a controller endpoint:
```csharp
[HttpPost("api/featured-cache/clear/{moduleId}")]
public IActionResult ClearCache(int moduleId)
{
	// Note: Can't call Blazor static from server
	// Would need to implement cache clearing also on server
	return Ok();
}
```

Then call from Blazor:
```csharp
await Http.PostAsync($"api/featured-cache/clear/{moduleId}", null);
```

**Option B: Keep cache client-side (recommended)**

Keep current implementation - cache is per-client session anyway.

---

## 🎯 Recommended Usage Patterns

### Pattern 1: After Saving Featured Entity (Current)

```csharp
// In EntityEdit.razor
private async Task Save()
{
	// ... save logic ...
	if (savedEntity != null && bool.Parse(_isFeatured))
	{
		Index.ClearFeaturedCache(ModuleState.ModuleId);
	}
}
```

### Pattern 2: Always Clear on Any Entity Save

```csharp
// In EntityEdit.razor
private async Task Save()
{
	// ... save logic ...
	if (savedEntity != null)
	{
		// Clear cache on any entity save (safer)
		Index.ClearFeaturedCache(ModuleState.ModuleId);
	}
}
```

### Pattern 3: Clear on Delete

```csharp
// In EntityEdit.razor
private async Task DeleteEntity()
{
	await ClientEntityService.DeleteEntityAsync(_id, ModuleState.ModuleId);
	Index.ClearFeaturedCache(ModuleState.ModuleId);
	NavigationManager.NavigateTo(...);
}
```

### Pattern 4: Manual Refresh Button

```html
<!-- In EntityEdit.razor UI -->
<button @onclick="RefreshCache">Refresh Featured Cache</button>

@code {
	async Task RefreshCache()
	{
		Index.ClearFeaturedCache(ModuleState.ModuleId);
		AddModuleMessage("Featured cache cleared");
	}
}
```

### Pattern 5: Programmatic Bulk Operations

```csharp
// In EntityList.razor or custom admin component
private async Task BulkMarkFeatured(List<int> entityIds)
{
	foreach (var id in entityIds)
	{
		var entity = await EntityService.GetEntityAsync(id, ModuleState.ModuleId);
		entity.IsFeatured = true;
		await EntityService.UpdateEntityAsync(entity);
	}

	// Clear cache after bulk operation
	Index.ClearFeaturedCache(ModuleState.ModuleId);
}
```

---

## 🔍 Finding the Cache

### See What's Cached

The cache is a private static dictionary in `Index.razor`:

```csharp
private static readonly Dictionary<string, FeaturedDataCache> _cache = new();
```

**Cache structure:**
```
Key: "FeaturedData_42"  (module ID 42)
Value: {
	FeaturedRecords: [ Entity1, Entity2 ],
	FeaturedTemplate: TemplateObject,
	EntityFields: [ Field1, Field2 ],
	EntityValuesMap: { EntityId: [EntityValue1, ...] }
}
```

### Cache Lifecycle

```
First load: 
  Cache miss → Load from API → Store in _cache

Refresh page:
  Cache hit → Use stored data → Instant load (~100ms)

Edit featured entity:
  Save → ClearFeaturedCache() called → Remove from _cache
  Next load: Cache miss → Load from API again
```

---

## 🛠️ Adding New Cache Control Methods

If you need more methods, add them to the `@code` block in `Index.razor`:

### Example: Cache with Timestamp

```csharp
/// <summary>
/// Clear cache only if older than specified minutes (optional feature)
/// </summary>
public static void ClearExpiredFeaturedCache(int moduleId, int minutesOld)
{
	string cacheKey = $"FeaturedData_{moduleId}";
	// Would need to track timestamps in FeaturedDataCache
}

/// <summary>
/// Get cache status for debugging
/// </summary>
public static int GetCacheSize()
{
	return _cache.Count;
}

/// <summary>
/// List all cached module IDs
/// </summary>
public static List<int> GetCachedModules()
{
	return _cache.Keys
		.Select(k => int.Parse(k.Split('_')[1]))
		.ToList();
}
```

Then use from any component:

```csharp
var size = Index.GetCacheSize();  // Get how many entries cached
var modules = Index.GetCachedModules();  // See which modules are cached
```

---

## 📍 File Locations Summary

| File | Method | Purpose |
|------|--------|---------|
| `Index.razor` | `ClearFeaturedCache(int)` | Clear cache for module |
| `Index.razor` | `ClearAllFeaturedCaches()` | Clear all modules' cache |
| `EntityEdit.razor` | `Save()` | Calls `ClearFeaturedCache()` |
| `EntityEdit.razor` | DeleteEntity() | (Future: call clear) |

---

## ✅ Current Implementation Checklist

- [x] Cache methods defined in Index.razor
- [x] Static public methods (accessible from other components)
- [x] ClearFeaturedCache(moduleId) - clear specific module
- [x] ClearAllFeaturedCaches() - clear all modules
- [x] Call added to EntityEdit.razor Save() method
- [x] Logs debug message when cache cleared
- [x] No errors or breaking changes

---

## 🚀 Next Steps (Optional)

### To Improve Cache Control

1. **Add always-clear option:**
   - Remove `if (bool.Parse(_isFeatured))` check
   - Always call `ClearFeaturedCache()` after save
   - Safer, no stale data

2. **Add delete handling:**
   - Find delete method in EntityEdit.razor
   - Add cache clear before navigation

3. **Add manual button:**
   - Add button UI to EntityEdit.razor
   - Call ClearFeaturedCache() on click

4. **Add time-based expiration:**
   - Modify FeaturedDataCache with timestamp
   - Check `IsExpired()` before using
   - Auto-refresh after X minutes

---

## 📞 Usage Example

### Complete Flow

```csharp
// Step 1: User edits featured entity
// File: EntityEdit.razor
private async Task Save()
{
	// ... validation, entity creation ...

	savedEntity = await ClientEntityService.UpdateEntityAsync(Entity);

	// Step 2: Save custom fields
	if (savedEntity != null)
	{
		await SaveCustomFieldValuesAsync(savedEntity.EntityId);

		// Step 3: Clear cache if featured
		if (bool.Parse(_isFeatured))
		{
			// Step 4: Call static method in Index.razor
			Index.ClearFeaturedCache(ModuleState.ModuleId);
			// ↓ This line executes
			// string cacheKey = $"FeaturedData_{ModuleState.ModuleId}";
			// _cache.Remove(cacheKey);
		}
	}

	// Step 5: Navigate to list
	NavigationManager.NavigateTo(NavigateUrl(ModuleState.ModuleId, "EntityList"));
}

// Step 6: User navigates to page with featured module
// File: Index.razor
protected override async Task OnInitializedAsync()
{
	if (_enableFeatured)
	{
		string cacheKey = $"FeaturedData_{ModuleState.ModuleId}";

		if (_cache.ContainsKey(cacheKey))
		{
			// MISS (because we cleared it)
			// So we enter else block...
		}
		else
		{
			// MISS - Load from API
			await LoadFeaturedRecordsAsync();

			// Store fresh data in cache
			_cache[cacheKey] = new FeaturedDataCache { ... };
		}
	}
}

// Step 7: Featured section renders with fresh data ✅
```

---

## Summary

**Current Status:**
- ✅ Cache methods implemented
- ✅ Auto-clear on featured entity save
- ✅ Logging when cache cleared

**How to use:**
- `Index.ClearFeaturedCache(moduleId)` - Clear one module's cache
- `Index.ClearAllFeaturedCaches()` - Clear all caches

**Call from:** Any Blazor component with `@using GIBS.Module.Entity`

**Implemented locations:**
- `EntityEdit.razor` - after saving featured
- Can be added elsewhere if needed

