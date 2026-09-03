# EF Core Relationship Configuration Fix

## Problem

The migration was failing with:
```
System.InvalidOperationException: Unable to determine the relationship represented by 
navigation 'Entity.Values' of type 'ICollection<EntityValue>'. Either manually configure 
the relationship, or ignore this property using the '[NotMapped]' attribute or by using 
'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
```

## Root Cause

EF Core couldn't automatically determine the relationship between `Entity` and `EntityValue` because:
1. The navigation property `Entity.Values` was added to Entity.cs
2. But the relationship wasn't explicitly configured in `EntityContext.OnModelCreating()`
3. EF Core needs explicit foreign key configuration for complex relationships

## Solution

Added explicit relationship configuration in `EntityContext.OnModelCreating()`:

```csharp
// Configure EntityValue relationships
builder.Entity<Models.EntityValue>()
	.HasOne(ev => ev.Entity)
	.WithMany(e => e.Values)
	.HasForeignKey(ev => ev.EntityId)
	.OnDelete(DeleteBehavior.Cascade);

builder.Entity<Models.EntityValue>()
	.HasOne(ev => ev.EntityField)
	.WithMany()
	.HasForeignKey(ev => ev.EntityFieldId)
	.OnDelete(DeleteBehavior.Restrict);

builder.Entity<Models.EntityValue>()
	.HasOne(ev => ev.ReferencedEntity)
	.WithMany()
	.HasForeignKey(ev => ev.ReferencedEntityId)
	.OnDelete(DeleteBehavior.SetNull);
```

## Configuration Details

**Entity → EntityValue Relationship:**
- Type: One-to-Many
- Foreign Key: EntityValueId.EntityId → Entity.EntityId
- Delete Behavior: Cascade (deleting an Entity deletes all its values)
- Navigation: Entity.Values (ICollection<EntityValue>)

**EntityField → EntityValue Relationship:**
- Type: One-to-Many
- Foreign Key: EntityValueId.EntityFieldId → EntityField.EntityFieldId
- Delete Behavior: Restrict (prevent deleting fields with existing values)
- Navigation: None (navigation collection not needed)

**ReferencedEntity → EntityValue Relationship:**
- Type: One-to-Many
- Foreign Key: EntityValue.ReferencedEntityId → Entity.EntityId
- Delete Behavior: SetNull (if referenced entity is deleted, set reference to null)
- Navigation: None (navigation collection not needed)

## Build Status

✅ Build Successful

## Migration Status

The migration should now complete successfully because:
1. All relationships are now explicitly configured
2. Foreign keys are mapped
3. Delete behaviors are defined
4. EF Core can generate the schema without ambiguity
5. The GIBS_EntityValue table will be created with proper constraints

## Testing

The migration will:
1. Create GIBS_EntityValue table with all typed columns
2. Add foreign key constraints to GIBS_Entity and GIBS_EntityField
3. Create all 7 strategic indexes defined in the migration
4. Enable cascade delete from Entity
5. Enable restrict delete from EntityField
6. Enable null reference when ReferencedEntity is deleted

