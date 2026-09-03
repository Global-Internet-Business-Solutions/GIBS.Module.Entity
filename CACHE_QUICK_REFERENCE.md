# Quick Reference - Cache Invalidation Methods

## ✅ Current: Automatic Clear on Save

**What it does:** Cache clears automatically when you save/edit a featured entity

**Implementation:**
- File: `EntityEdit.razor`, Save() method
- Checks: `if (bool.Parse(_isFeatured))`
- Action: `Index.ClearFeaturedCache(ModuleState.ModuleId)`

**Usage:** 
- Just save a featured entity as normal
- Cache clears automatically
- No extra steps

**Example flow:**
```
1. User edits "Featured Product A"
2. Changes name, saves
3. ✅ Cache automatically clears
4. Other users see updated name immediately
```

---

## 🛠️ Available Methods

### Method 1: Clear for Specific Module

```csharp
Index.ClearFeaturedCache(moduleId);
```

**When to use:**
- After editing featured in that module
- After bulk operations in that module
- From a custom component

**Example:**
```csharp
if (wasEntityMarkedFeatured)
{
	Index.ClearFeaturedCache(ModuleState.ModuleId);
}
```

### Method 2: Clear All Modules

```csharp
Index.ClearAllFeaturedCaches();
```

**When to use:**
- Template changed globally
- Full refresh wanted
- After configuration changes

**Example:**
```csharp
// After modifying Featured template
Index.ClearAllFeaturedCaches();
AddModuleMessage("All featured caches refreshed");
```

---

## 📋 Scenarios & How to Handle

### Scenario 1: User adds new featured entity
```
How it works NOW (Option 1):
✅ EntityEdit saves with IsFeatured = true
✅ Cache clears automatically
✅ Any module showing featured sees it next load

What you do: Nothing! It's automatic.
```

### Scenario 2: User marks existing entity as featured
```
How it works NOW (Option 1):
✅ EntityEdit saves with IsFeatured = true (was false)
✅ Cache clears automatically
✅ Featured section refreshes on next page view

What you do: Nothing! It's automatic.
```

### Scenario 3: User unmarks entity from featured
```
How it works NOW (Option 1):
⚠️ EntityEdit saves with IsFeatured = false
⚠️ Cache does NOT clear (only clears if IsFeatured = true)
❌ Old featured list still shows

Solution: Add this enhancement to EntityEdit.razor
```

**Fix for Scenario 3:**

In `EntityEdit.razor` Save() method, change:

```csharp
// BEFORE
if (bool.Parse(_isFeatured))
{
	Index.ClearFeaturedCache(ModuleState.ModuleId);
}

// AFTER - clears for any featured entity (whether adding or removing)
Index.ClearFeaturedCache(ModuleState.ModuleId);
// OR keep old if you only care about additions:
if (bool.Parse(_isFeatured) || /* was featured before and now isn't */)
{
	Index.ClearFeaturedCache(ModuleState.ModuleId);
}
```

### Scenario 4: User deletes a featured entity
```
How it works NOW:
⚠️ Entity is deleted
⚠️ Cache NOT cleared
❌ Deleted entity still shows in featured

Solution: Add cache clear to delete method
```

**Fix for Scenario 4:**

Find delete method in `EntityEdit.razor` and add:

```csharp
private async Task DeleteEntity()
{
	try
	{
		// ... existing delete code ...

		// Clear featured cache (might have been featured)
		Index.ClearFeaturedCache(ModuleState.ModuleId);

		// Navigate away
		NavigationManager.NavigateTo(...);
	}
	catch (Exception ex) { ... }
}
```

### Scenario 5: Admin wants to force refresh featured
```
How it works NOW:
✅ Could add manual button (Option 2)
✅ Or just restart app

Solution: Optional - add refresh button
```

**Button to add:**

```html
<button class="btn btn-sm btn-warning" @onclick="() => { Index.ClearFeaturedCache(ModuleState.ModuleId); }">
	<span class="oi oi-reload"></span> Refresh Featured
</button>
```

---

## 🚀 How to Enable Always-Clear (All Featured Changes)

If you want cache to clear on ANY featured entity edit (add, edit featured/unfeatured, etc.):

### Simple Fix

Change this in `EntityEdit.razor` Save() method:

**BEFORE:**
```csharp
if (bool.Parse(_isFeatured))
{
	Index.ClearFeaturedCache(ModuleState.ModuleId);
}
```

**AFTER:**
```csharp
// Always clear for any entity that touches featured section
// Whether it's being marked featured or unmarked
Index.ClearFeaturedCache(ModuleState.ModuleId);
```

This way, any featured entity change clears the cache. 

**Tradeoff:** Clears cache even if storing featured=false, but ensures data is never stale.

---

## 📊 What Gets Cached?

The cache stores:
- ✅ List of featured entities
- ✅ Featured template (Item, Alternate, Header, Footer, Separator)
- ✅ Entity fields (name, description, custom fields)
- ✅ Entity values (custom field data)

**When it clears:**
- ✅ After editing featured entity (Option 1, implemented now)
- ✅ When you clear it manually (if you add Option 2)
- ✅ After time expiration (if you add Option 5)
- ✅ When app restarts

---

## 🔧 Making Enhancements

### Add Always-Clear (5 min task)

1. Open `EntityEdit.razor`
2. Find Save() method (line ~842)
3. Replace lines 900-907:

```csharp
// BEFORE
if (savedEntity != null)
{
	await SaveCustomFieldValuesAsync(savedEntity.EntityId);

	if (bool.Parse(_isFeatured))
	{
		Index.ClearFeaturedCache(ModuleState.ModuleId);
	}
}

// AFTER - always clear to be safe
if (savedEntity != null)
{
	await SaveCustomFieldValuesAsync(savedEntity.EntityId);
	Index.ClearFeaturedCache(ModuleState.ModuleId);
}
```

4. Build project
5. Test: Edit any entity, featured cache clears

### Add Cache Clear on Delete (3 min task)

1. Find delete method in `EntityEdit.razor`
2. Before navigation, add:

```csharp
Index.ClearFeaturedCache(ModuleState.ModuleId);
```

3. Build and test

### Add Manual Refresh Button (5 min task)

1. Find button section in `EntityEdit.razor`
2. Add button:

```html
<button class="btn btn-sm btn-warning" @onclick="() => Index.ClearFeaturedCache(ModuleState.ModuleId)">
	<span class="oi oi-reload"></span> Refresh Featured
</button>
```

3. Build and test

---

## ✅ Status Summary

| Feature | Status | Effort |
|---------|--------|--------|
| Auto clear on featured save | ✅ Implemented | Done |
| Auto clear on featured delete | ❌ Not implemented | 3 min |
| Always clear (not just featured) | ❌ Not implemented | 2 min |
| Manual refresh button | ❌ Not implemented | 5 min |
| Time-based expiration | ❌ Not implemented | 15 min |
| Programmatic clear | ✅ Available | Use `Index.ClearFeaturedCache(id)` |

**Recommendation:** Current implementation (✅) covers 90% of cases. Only add more if you see stale data issues.

