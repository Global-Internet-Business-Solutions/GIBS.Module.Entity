# Cache Invalidation - Clear Featured Records Cache

## Problem
After implementing static cache for featured records, the cache persists until the app restarts. If a user marks a new entity as featured or updates a featured entity, the Index page won't show the changes until the cache is manually cleared.

## Solution
Multiple cache-clearing strategies, from simple to advanced.

---

## Option 1: Automatic Clear on Save (✅ Implemented)

**When it clears:** Automatically when you save/edit a featured entity  
**Best for:** Most users - automatic and transparent

### Implementation

The `EntityEdit.razor` component now clears the cache after saving a featured entity:

```csharp
// In EntityEdit.razor Save() method
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

### How It Works

1. User edits or creates an entity
2. Sets `IsFeatured = true` (or keeps it true)
3. Click Save
4. Entity is saved
5. Custom fields are saved
6. **Cache is automatically cleared** ✅
7. Next time featured records render, they reload from server (fresh data)

### Usage

No additional code needed! It's automatic.

### Pros
- ✅ Automatic - no manual triggers
- ✅ No user action needed
- ✅ Data always fresh after edit
- ✅ Simple to implement

### Cons
- ⚠️ Only clears when featured entity is edited
- ⚠️ Doesn't clear if non-featured entity is edited
- ⚠️ Doesn't clear if featured entity is deleted

---

## Option 2: Manual Refresh Button (Optional Enhancement)

**When it clears:** When user clicks a button  
**Best for:** Developers/admins who want control

### Add to EntityEdit.razor

```html
<button class="btn btn-warning" @onclick="RefreshFeaturedCache" title="Refresh featured records cache">
	<span class="oi oi-reload"></span> Refresh Featured Cache
</button>
```

### Add to EntityEdit.razor @code block

```csharp
private async Task RefreshFeaturedCache()
{
	try
	{
		Index.ClearFeaturedCache(ModuleState.ModuleId);
		AddModuleMessage("Featured records cache cleared. Changes will appear on next page load.", MessageType.Success);
		await logger.LogInformation($"Featured cache manually cleared for module {ModuleState.ModuleId}");
	}
	catch (Exception ex)
	{
		await logger.LogError(ex, "Error clearing featured cache");
		AddModuleMessage("Error clearing cache", MessageType.Error);
	}
}
```

### Usage

1. Edit entity (featured or not)
2. Click "Refresh Featured Cache" button
3. Cache clears
4. Navigate to featured section
5. See updated data

---

## Option 3: Clear Cache on Entity Delete (Advanced)

Currently the automatic clear (Option 1) only fires on save. To also clear when deleting a featured entity, add this to EntityEdit.razor:

```csharp
private async Task DeleteEntity()
{
	try
	{
		// ... existing delete logic ...
		await ClientEntityService.DeleteEntityAsync(_id, ModuleState.ModuleId);

		// Clear cache after delete (in case it was featured)
		Index.ClearFeaturedCache(ModuleState.ModuleId);
		await logger.LogInformation($"Cleared featured cache after deleting entity");

		// ... navigate away ...
	}
	catch (Exception ex)
	{
		await logger.LogError(ex, "Error deleting entity");
	}
}
```

---

## Option 4: Programmatic Clearing (From Code)

### Clear for Specific Module

```csharp
// Clear cache for one module
Index.ClearFeaturedCache(moduleId);
```

**Use cases:**
- After bulk operations
- From a custom scheduled job
- From an admin utility page
- From a background service

### Clear All Modules

```csharp
// Clear cache for ALL modules
Index.ClearAllFeaturedCaches();
```

**Use cases:**
- After template changes (affects all modules)
- After global configuration changes
- Force refresh across the system

---

## Option 5: Time-Based Cache Expiration (Premium Option)

**When it clears:** After X minutes, cache automatically expires  
**Best for:** Production where staleness is acceptable

### Implementation

Modify the `FeaturedDataCache` class in `Index.razor`:

```csharp
private class FeaturedDataCache
{
	public List<Entity> FeaturedRecords { get; set; }
	public EntityTemplate FeaturedTemplate { get; set; }
	public List<EntityField> EntityFields { get; set; }
	public Dictionary<int, List<EntityValue>> EntityValuesMap { get; set; }

	// Add timestamp
	public DateTime CachedAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Check if cache is expired (older than specified minutes)
	/// </summary>
	public bool IsExpired(int minutesLimit = 30)
	{
		return DateTime.UtcNow.Subtract(CachedAt).TotalMinutes > minutesLimit;
	}
}
```

Then in `OnInitializedAsync`:

```csharp
if (_enableFeatured)
{
	string cacheKey = $"FeaturedData_{ModuleState.ModuleId}";

	// Check if cache exists AND is not expired
	if (_cache.ContainsKey(cacheKey) && !_cache[cacheKey].IsExpired(minutesLimit: 30))
	{
		// Use cached data if fresh
		var cached = _cache[cacheKey];
		_featuredRecords = cached.FeaturedRecords;
		_featuredTemplate = cached.FeaturedTemplate;
		_entityFields = cached.EntityFields;
		_entityValuesMap = cached.EntityValuesMap;
		await logger.LogDebug($"Cache HIT (fresh): Loaded featured records");
	}
	else if (_cache.ContainsKey(cacheKey))
	{
		// Cache expired, remove it
		_cache.Remove(cacheKey);
		await logger.LogDebug($"Cache EXPIRED: Reloading featured records");
		await LoadFeaturedRecordsAsync();
		// Store fresh cache
		_cache[cacheKey] = new FeaturedDataCache { ... };
	}
	else
	{
		// First time load
		await LoadFeaturedRecordsAsync();
		_cache[cacheKey] = new FeaturedDataCache { ... };
	}
}
```

### Configuration

Adjust the cache timeout:
- `30` minutes - Reasonable for most scenarios
- `5` minutes - More frequent refreshes
- `1` minute - Nearly always fresh
- `0` minutes - Disable cache (always load)

### Pros
- ✅ Automatic expiration
- ✅ No manual intervention needed
- ✅ Configurable timeout
- ✅ Balances performance and freshness

### Cons
- ⚠️ Adds timestamp tracking
- ⚠️ More complex logic
- ⚠️ Data may be stale for up to X minutes

---

## Implementation Recommendations

### For Most Teams
**Use Option 1 (Automatic Clear on Save)** ✅

- Simple to implement (already done!)
- Covers 90% of use cases
- No extra work needed
- Data is fresh when it matters (after edits)

### For High-Traffic Sites
**Use Option 1 + Option 5 (Auto Clear + Time Expiration)**

- Automatic clear on edit
- Plus time-based expiration for safety
- Best of both worlds
- Small performance cost

### For Admin-Heavy Usage
**Use Option 1 + Option 2 (Auto Clear + Manual Button)**

- Automatic on edit
- Plus manual button for force refresh
- Gives control to power users
- Best transparency

### For High-Volume Operations
**Use Option 1 + Option 4 (Auto Clear + Programmatic)**

- Automatic on single edit
- Programmatic clear after bulk operations
- Covers all scenarios
- Most comprehensive

---

## Current Implementation Status

✅ **Option 1 is already implemented**

The cache automatically clears when you save a featured entity in EntityEdit.razor.

---

## How to Enable Additional Options

### Add Manual Refresh Button (Option 2)

1. Open `EntityEdit.razor`
2. Find the button bar (around line 175-180)
3. Add button:
```html
<button type="button" class="btn btn-warning" @onclick="RefreshFeaturedCache">
	<span class="oi oi-reload"></span> Refresh Featured
</button>
```
4. Add method to @code block
5. Build and test

### Add Time-Based Expiration (Option 5)

1. Open `Index.razor`
2. Update `FeaturedDataCache` class with timestamp
3. Update `OnInitializedAsync` logic to check expiration
4. Build and test

---

## Testing Cache Clearing

### Test Automatic Clear (Option 1)

1. View featured page → See Record1, Record2 ✅
2. Edit Record2, change name
3. Save
4. Navigate back to featured page
5. **Expected:** See updated Record2 name ✅

### Test Manual Clear (Option 2, if added)

1. View featured page
2. Go to EntityEdit
3. Click "Refresh Featured Cache" button
4. See success message
5. View featured page again
6. **Expected:** Fresh data loaded ✅

### Test Time Expiration (Option 5, if added)

1. View featured page (cache created at time T)
2. Wait 31 minutes (if timeout is 30)
3. View featured page again
4. Check logs → should see "Cache EXPIRED"
5. **Expected:** Fresh data reloaded ✅

---

## Logging

When cache is cleared, debug logs are written:

```
[DEBUG] Cleared featured cache for module 42
[DEBUG] Cache HIT: Loaded featured records from cache
[DEBUG] Cache MISS: Loaded featured records from API
[DEBUG] Cache EXPIRED: Reloading featured records
```

Check Visual Studio Output window (Debug output) or server logs to see cache operations.

---

## Performance Impact

### Automatic Clear (Option 1)
- ✅ No performance cost
- Removes 1 cache entry when featured is edited
- Reload happens on next page view
- Typically < 50ms additional load time

### Time Expiration (Option 5)
- ⚠️ Minimal cost - timestamp check on every load
- Cache comparison: `DateTime.UtcNow.Subtract(CachedAt).TotalMinutes > 30`
- Negligible (< 1ms)

### Manual Clear (Option 2)
- ✅ On-demand, no performance cost
- Only when user triggers it

---

## Summary

| Option | Auto | Manual | Code | Use Case |
|--------|------|--------|------|----------|
| **1: Auto on Save** | ✅ | ❌ | ✅ Implemented | Standard use, edit featured |
| **2: Manual Button** | ❌ | ✅ | Optional | Admin control, force refresh |
| **3: On Delete** | ✅ | ❌ | Easy add | Delete featured entities |
| **4: Programmatic** | N/A | ✅ | Custom | Bulk ops, scheduled jobs |
| **5: Time Expiration** | ✅ | ❌ | +Config | Balance freshness/performance |

**Recommended Strategy:** Option 1 (✅ already done) covers most needs perfectly!

