# Foreign Key Mismatch Fix - EntityValue to EntityField

## Problem

The migration failed with:
```
Foreign key 'FK_GIBS_EntityValue_EntityField' references invalid column 
'EntityFieldId' in referenced table 'GIBS_EntityField'.
```

## Root Cause

The `GIBS_EntityField` table's primary key is `FieldId`, not `EntityFieldId`. The original EntityValue model used `EntityFieldId` as the foreign key property, which doesn't match the referenced column name in the EntityField table.

## Solution

Renamed the foreign key property in EntityValue from `EntityFieldId` to `FieldId` to match EntityField's primary key naming convention.

### Changes Made

#### 1. EntityValue Model (`Shared\Models\EntityValue.cs`)
```csharp
// Before
public int EntityFieldId { get; set; }

// After
public int FieldId { get; set; }
```

#### 2. EntityContext Configuration (`Server\Repository\EntityContext.cs`)
```csharp
// Updated relationship to use FieldId
builder.Entity<Models.EntityValue>()
	.HasOne(ev => ev.EntityField)
	.WithMany()
	.HasForeignKey(ev => ev.FieldId)  // ← Changed from EntityFieldId
	.OnDelete(DeleteBehavior.Restrict);
```

#### 3. EntityValueEntityBuilder (`Server\Migrations\EntityBuilders\EntityValueEntityBuilder.cs`)
```csharp
// Updated foreign key definition
private readonly ForeignKey<EntityValueEntityBuilder> _fieldForeignKey = 
	new("FK_GIBS_EntityValue_EntityField", x => x.FieldId, "GIBS_EntityField", "FieldId", ReferentialAction.Restrict);

// Updated column creation
FieldId = AddIntegerColumn(table, "FieldId");

// Updated property
public OperationBuilder<AddColumnOperation> FieldId { get; set; } = null!;
```

#### 4. Migration (`Server\Migrations\01000600_AddEntityValueTable.cs`)
```csharp
// Updated all index definitions to use FieldId instead of EntityFieldId
migrationBuilder.CreateIndex(
	name: "IX_GIBS_EntityValue_Entity_Field",
	table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
	columns: new[] { "EntityId", "FieldId" });  // ← Changed from EntityFieldId
```

#### 5. EntityValueService (`Server\Services\EntityValueService.cs`)
```csharp
// Updated CRUD operations to use FieldId
FieldId = fieldId,  // ← Changed from EntityFieldId = fieldId

// Updated query
.Where(ev => ev.EntityId == entityId && ev.FieldId == fieldId)  // ← Changed from EntityFieldId
```

#### 6. Query Extensions (`Server\Repository\Extensions\EntityValueQueryExtensions.cs`)
```csharp
// Mass replacement: ev.EntityFieldId → ev.FieldId
// Updated 40+ filter/sort methods to use ev.FieldId
```

## Database Schema Impact

The GIBS_EntityValue table will now have:
- Column: `FieldId` (instead of `EntityFieldId`)
- Foreign Key: `FK_GIBS_EntityValue_EntityField` 
  - References: `GIBS_EntityField.FieldId`
  - Delete Behavior: Restrict (prevents deleting fields with existing values)

## Index Names (Unchanged)

All index names remain the same, just use FieldId:
- IX_GIBS_EntityValue_Entity_Field (EntityId, FieldId)
- IX_GIBS_EntityValue_Field_TextValue (FieldId, TextValue)
- IX_GIBS_EntityValue_Field_IntegerValue (FieldId, IntegerValue)
- IX_GIBS_EntityValue_Field_DecimalValue (FieldId, DecimalValue)
- IX_GIBS_EntityValue_Field_BooleanValue (FieldId, BooleanValue)
- IX_GIBS_EntityValue_Field_DateValue (FieldId, DateValue)
- IX_GIBS_EntityValue_Field_ReferencedEntity (FieldId, ReferencedEntityId)

## Build Status

✅ Build Successful

## Next Steps

The migration should now complete successfully because:
1. ✅ Foreign key references the correct column (`FieldId` → `FieldId`)
2. ✅ Table schema is properly defined
3. ✅ All indexes use the correct column names
4. ✅ Relationships are explicitly configured in EF Core
5. ✅ No naming mismatches

The GIBS_EntityValue table will be created with proper foreign key constraints and all strategic indexes for efficient searching and filtering.

