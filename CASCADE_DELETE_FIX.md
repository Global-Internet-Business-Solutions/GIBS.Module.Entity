# SQL Server Cascade Delete Conflict Fix

## Problem

The migration failed with:
```
Introducing FOREIGN KEY constraint 'FK_GIBS_EntityValue_ReferencedEntity' on table 
'GIBS_EntityValue' may cause cycles or multiple cascade paths. Specify ON DELETE NO ACTION 
or ON UPDATE NO ACTION, or modify other FOREIGN KEY constraints.
```

## Root Cause

SQL Server doesn't allow multiple cascade delete paths to the same table. The schema had:
1. `GIBS_EntityValue.EntityId` → `GIBS_Entity.EntityId` (**CASCADE** delete)
2. `GIBS_EntityValue.ReferencedEntityId` → `GIBS_Entity.EntityId` (**SetNull** delete)

Both foreign keys reference the same table (`GIBS_Entity`), creating ambiguity about what should happen when an Entity is deleted. This violates SQL Server's constraint rules.

## Solution

Changed the `ReferencedEntityId` foreign key delete behavior from `SetNull` to `NoAction`.

### Why NoAction?

- **NoAction:** If a referenced entity is deleted, raise an error if there are child references. The application must handle cleanup.
- This is the safest approach for entity references since you don't want to silently null out cross-references.

### Changes Made

#### 1. EntityContext (`Server\Repository\EntityContext.cs`)
```csharp
// Before
builder.Entity<Models.EntityValue>()
	.HasOne(ev => ev.ReferencedEntity)
	.WithMany()
	.HasForeignKey(ev => ev.ReferencedEntityId)
	.OnDelete(DeleteBehavior.SetNull);  // ❌ SetNull

// After
builder.Entity<Models.EntityValue>()
	.HasOne(ev => ev.ReferencedEntity)
	.WithMany()
	.HasForeignKey(ev => ev.ReferencedEntityId)
	.OnDelete(DeleteBehavior.NoAction);  // ✅ NoAction
```

#### 2. EntityValueEntityBuilder (`Server\Migrations\EntityBuilders\EntityValueEntityBuilder.cs`)
```csharp
// Before
private readonly ForeignKey<EntityValueEntityBuilder> _referencedEntityForeignKey = 
	new("FK_GIBS_EntityValue_ReferencedEntity", x => x.ReferencedEntityId, 
	"GIBS_Entity", "EntityId", ReferentialAction.SetNull);  // ❌

// After
private readonly ForeignKey<EntityValueEntityBuilder> _referencedEntityForeignKey = 
	new("FK_GIBS_EntityValue_ReferencedEntity", x => x.ReferencedEntityId, 
	"GIBS_Entity", "EntityId", ReferentialAction.NoAction);  // ✅
```

## Foreign Key Summary

| Foreign Key | From Table | To Table | Behavior | Purpose |
|---|---|---|---|---|
| FK_GIBS_EntityValue_Entity | EntityValue.EntityId | Entity.EntityId | **CASCADE** | Delete all values when entity is deleted |
| FK_GIBS_EntityValue_EntityField | EntityValue.FieldId | EntityField.FieldId | **RESTRICT** | Prevent deleting fields with existing values |
| FK_GIBS_EntityValue_ReferencedEntity | EntityValue.ReferencedEntityId | Entity.EntityId | **NO ACTION** | Prevent deleting referenced entities (error if referenced) |

## Database Behavior

When an Entity is deleted:
1. ✅ All `EntityValue` rows with `EntityId` matching the deleted entity are automatically deleted (CASCADE)
2. ❌ Any `EntityValue` rows with `ReferencedEntityId` matching the deleted entity will **block the deletion** (NoAction prevents it)

**Application Responsibility:** The application should handle the NoAction constraint by either:
- Cleaning up references before deleting the entity
- Handling the foreign key constraint exception gracefully

## Build Status - First Issue ✅

✅ **Build Successful** (after fixing cascade delete)

---

# SQL Server Index Column Type Error - FIXED ✅

## Problem 2

After fixing the cascade delete issue, the migration then failed with:
```
Column 'TextValue' in table 'GIBS_EntityValue' is of a type that 
is invalid for use as a key column in an index.
Error Number: 1919
```

## Root Cause

The migration was attempting to create an index on the `TextValue` column, which is defined as `nvarchar(MAX)`. SQL Server does **not allow unbounded text types** (`nvarchar(MAX)`, `varbinary(MAX)`, `text`, `image`, etc.) to be used as key columns in indexes.

### Column Definitions in EntityValueEntityBuilder
```csharp
TextValue = AddMaxStringColumn(table, "TextValue", true);  // ❌ nvarchar(MAX)
```

## Solution

**Removed the problematic index** that included `TextValue`:

```csharp
// ❌ REMOVED - Cannot index nvarchar(MAX)
migrationBuilder.CreateIndex(
    name: "IX_GIBS_EntityValue_Field_TextValue",
    table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
    columns: new[] { "FieldId", "TextValue" });
```

### Remaining Valid Indexes

These indexes remain because they use bounded or fixed-size types:
- `IX_GIBS_EntityValue_Entity_Field` – `(EntityId, FieldId)` – int keys
- `IX_GIBS_EntityValue_Field_IntegerValue` – `(FieldId, IntegerValue)` – int and int
- `IX_GIBS_EntityValue_Field_DecimalValue` – `(FieldId, DecimalValue)` – int and decimal(18,4)
- `IX_GIBS_EntityValue_Field_BooleanValue` – `(FieldId, BooleanValue)` – int and bit
- `IX_GIBS_EntityValue_Field_DateValue` – `(FieldId, DateValue)` – int and datetime2
- `IX_GIBS_EntityValue_Field_ReferencedEntity` – `(FieldId, ReferencedEntityId)` – int and int

### For Text Search

If text searching becomes a performance concern later, use:
1. **Full-text search** – SQL Server's built-in FTS engine for text columns
2. **Filtered indexed view** – Create a persisted computed column with a substring for partial text indexing
3. **Application-level filtering** – Accept full table scans for text queries and filter in memory

## Build Status - Final ✅

✅ **Build Successful**

## Next Steps

The migration should now complete successfully because:
1. ✅ No multiple cascade paths to the same table
2. ✅ All foreign key constraints are valid
3. ✅ No unbounded types in index keys
4. ✅ SQL Server will accept all index and constraint creation
5. ✅ Referential integrity is maintained

