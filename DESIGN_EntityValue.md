# GIBS Entity Module - EntityValue Architecture Design

## Overview

This document describes the **EntityValue system** which replaces JSON Settings storage for custom field data with a properly normalized, searchable, and queryable relational design.

---

## Problem Statement

Previously, custom field values were stored as JSON in `Entity.Settings`:

```json
{
  "5": "Ford",
  "6": "2024",
  "7": "55000.00"
}
```

**Limitations:**
- Cannot search/filter by custom fields without parsing JSON
- Cannot sort by custom field values
- Cannot create indexes on field values
- Doesn't scale to thousands of entities
- No type safety (everything is a string)
- Difficult to report on custom fields

---

## Solution: EntityValue Table

Instead of JSON, we use a **normalized, typed relational design**.

### Data Model

```plaintext
GIBS_Entity
├── EntityId
├── EntityTypeId
├── Name
├── Settings  ← NOW ONLY for configuration, not business data
└── [other standard fields]

GIBS_EntityField
├── FieldId
├── EntityTypeId
├── DataType  ← String, Integer, Decimal, Boolean, Date, DateTime, Guid
├── IsMultiValue
├── IsSearchable
└── [metadata]

GIBS_EntityValue
├── EntityValueId (Primary Key)
├── EntityId  ← Which entity
├── EntityFieldId  ← Which field
├── ValueIndex  ← Ordinal for multi-value fields
├── TextValue  ← For String, Guid.ToString()
├── IntegerValue  ← For Integer
├── LongValue  ← For Long
├── DecimalValue  ← For Decimal (money, measurements)
├── BooleanValue  ← For Boolean
├── DateValue  ← For Date
├── DateTimeValue  ← For DateTime
├── GuidValue  ← For Guid
├── ReferencedEntityId  ← For EntityReferences (agent_id, property_manager_id, etc.)
└── Audit fields
```

### Example: Property Listing

**Entity:** "123 Main St, Chatham, MA"

```plaintext
Field: Address (String)
→ GIBS_EntityValue: TextValue = "123 Main St, Chatham, MA"

Field: Bedrooms (Integer)
→ GIBS_EntityValue: IntegerValue = 3

Field: Price (Decimal)
→ GIBS_EntityValue: DecimalValue = 575000.00

Field: ListingAgent (EntityReference)
→ GIBS_EntityValue: ReferencedEntityId = 42 (Agent entity ID)

Field: ListDate (Date)
→ GIBS_EntityValue: DateValue = 2024-06-15

Field: Features (Multi-value String)
ValueIndex=0 → TextValue = "Waterfront"
ValueIndex=1 → TextValue = "Pool"
ValueIndex=2 → TextValue = "Hot Tub"
```

---

## Benefits

### 1. **Efficient Searching**

**Query:** Find all properties with price > $500,000

```csharp
var results = await context.EntityValues
	.Where(ev => ev.EntityFieldId == priceFieldId)
	.DecimalGreaterThan(priceFieldId, 500000)
	.Select(ev => ev.Entity)
	.Distinct()
	.ToListAsync();
```

Translates to SQL:
```sql
SELECT DISTINCT e.* 
FROM GIBS_Entity e
JOIN GIBS_EntityValue ev ON e.EntityId = ev.EntityId
WHERE ev.EntityFieldId = 7 
  AND ev.DecimalValue > 500000
```

### 2. **Efficient Filtering**

**Query:** Properties with 3+ bedrooms AND price < $750k AND waterfront = true

```csharp
var bedroomFilter = new EntityFieldFilter 
	{ FieldId = bedroomFieldId, Operator = "GreaterThanOrEqual", Value = "3" };
var priceFilter = new EntityFieldFilter 
	{ FieldId = priceFieldId, Operator = "LessThan", Value = "750000" };
var waterfrontFilter = new EntityFieldFilter 
	{ FieldId = waterfrontFieldId, Operator = "Equals", Value = "true" };

var results = await entityValueService.SearchEntityValuesAsync(
	new List<EntityFieldFilter> { bedroomFilter, priceFilter, waterfrontFilter },
	sortFieldId: priceFieldId,
	sortDescending: false,
	skip: 0, take: 20,
	moduleId: ModuleState.ModuleId
);
```

### 3. **Efficient Sorting**

**Query:** Sort properties by price descending

```csharp
var results = await context.Entities
	.Join(context.EntityValues.Where(ev => ev.EntityFieldId == priceFieldId),
		  e => e.EntityId,
		  ev => ev.EntityId,
		  (e, ev) => new { Entity = e, Price = ev.DecimalValue })
	.OrderByDescending(x => x.Price)
	.Select(x => x.Entity)
	.ToListAsync();
```

### 4. **Type Safety**

Values are stored in their **actual type**:
- Prices as `decimal`, not `"55000.00"` string
- Dates as `DateTime`, not `"2024-06-15"` string
- Counts as `int`, not `"42"` string

No parsing needed!

### 5. **Indexing Strategy**

We create targeted indexes for common query patterns:

```
IX_EntityValue_Entity_Field
  ↓ Retrieves all values for an entity

IX_EntityValue_Field_TextValue
  ↓ Finds string values ("Which properties have 'waterfront' in description?")

IX_EntityValue_Field_DecimalValue
  ↓ Finds decimal values ("Price > $500k")

IX_EntityValue_Field_IntegerValue
  ↓ Finds integer values ("Bedrooms >= 3")

IX_EntityValue_Field_BooleanValue
  ↓ Finds boolean values ("Waterfront = true")

IX_EntityValue_Field_DateValue
  ↓ Finds by date ("Listed after 2024-01-01")

IX_EntityValue_Field_ReferencedEntity
  ↓ Finds by reference ("Agent = 42")
```

These indexes enable **fast queries** even at scale (1000s-10000s of entities).

### 6. **Supports Multi-Value Fields**

A single field can have multiple values via `ValueIndex`:

```plaintext
Entity: Beach House
Field: Amenities (Multi-value String)

ValueIndex=0 → TextValue = "Pool"
ValueIndex=1 → TextValue = "Hot Tub"
ValueIndex=2 → TextValue = "Waterfront"
ValueIndex=3 → TextValue = "Garage"
```

Query: "Find all properties with 'Pool' amenity"

```csharp
var results = await context.EntityValues
	.Where(ev => ev.EntityFieldId == amenitiesFieldId &&
				 ev.TextValue == "Pool")
	.Select(ev => ev.Entity)
	.Distinct()
	.ToListAsync();
```

### 7. **Supports Entity References**

Fields can reference other entities:

```plaintext
Field: ListingAgent (EntityReference)
→ ReferencedEntityId = 42 (points to Agent Entity)

Field: Property Manager (EntityReference)
→ ReferencedEntityId = 18 (points to Manager Entity)
```

Query: "Find all properties managed by Agent 42"

```csharp
var results = await context.EntityValues
	.Where(ev => ev.EntityFieldId == agentFieldId &&
				 ev.ReferencedEntityId == 42)
	.Select(ev => ev.Entity)
	.Distinct()
	.ToListAsync();
```

Or **with eager loading** of the referenced entity:

```csharp
var results = await context.EntityValues
	.Where(ev => ev.EntityFieldId == agentFieldId)
	.Include(ev => ev.ReferencedEntity)
	.Where(ev => ev.ReferencedEntity.Name == "John Smith")
	.Select(ev => ev.Entity)
	.Distinct()
	.ToListAsync();
```

---

## Architecture: How All Pieces Connect

### 1. Entity Creation/Editing Flow

```
User → EntityEdit.razor
	   ↓
	   Takes form input (custom fields)
	   ↓
	   Calls EntityService.AddEntityAsync()
	   ↓
	   EntityService:
		 ├─ Save GIBS_Entity (Name, Description, etc.)
		 ├─ Call EntityValueService.AddEntityValuesAsync()
		 │  └─ For each custom field value:
		 │     ├─ Create EntityValue record
		 │     ├─ Populate appropriate typed column (TextValue, IntegerValue, etc.)
		 │     └─ Save to GIBS_EntityValue
		 └─ Return created Entity
```

### 2. Entity Search/Filter Flow

```
User → EntityList.razor
	   ↓
	   Selects search/filter criteria
	   ↓
	   Calls EntityValueService.SearchEntityValuesAsync(filters)
	   ↓
	   EntityValueService:
		 ├─ Parse filters
		 ├─ Build LINQ query with typed column comparisons
		 │  ├─ price > 500000  → ev.DecimalValue > 500000
		 │  ├─ bedrooms >= 3   → ev.IntegerValue >= 3
		 │  └─ city = "Chatham" → ev.TextValue == "Chatham"
		 ├─ Apply indexes for performance
		 ├─ Execute query
		 └─ Return matching Entities
```

### 3. Entity Retrieval Flow

```
User → EntityDetail.razor (display a single entity)
	   ↓
	   Calls EntityValueService.GetEntityValuesAsync(entityId)
	   ↓
	   EntityValueService:
		 ├─ Query EntityValue table WHERE EntityId = ?
		 ├─ Group by EntityFieldId
		 ├─ For each field:
		 │  ├─ Read appropriate typed column value
		 │  ├─ Convert to field's DataType
		 │  └─ Return to caller
		 └─ Return all field values for entity
```

### 4. Migration: Settings → EntityValue

When we're ready to migrate from JSON Settings:

```
For each Entity in the database:
  1. Parse Entity.Settings JSON
  2. For each field ID and value in JSON:
	 a. Determine field's DataType
	 b. Convert string value to appropriate type
	 c. Create EntityValue record with typed column populated
	 d. Save to GIBS_EntityValue
  3. Clear Entity.Settings (or mark as migrated)
```

---

## Implementation Phases

### Phase 1 (Current): Design & Model Creation
- ✅ Create EntityValue model
- ✅ Create EF Core configuration with indexes
- ✅ Create LINQ extension methods
- ✅ Create IEntityValueService interface
- Create EntityValueService implementation

### Phase 2: Service & Repository
- Implement EntityValueService
- Create Entity/EntityValue repository methods
- Add audit field population (CreatedBy, ModifiedBy)
- Add validation (DataType → typed column)

### Phase 3: UI Components
- Update EntityEdit.razor to save to EntityValue (instead of Settings)
- Update EntityList.razor with search/filter UI
- Add dynamic field filtering UI
- Add dynamic field sorting UI

### Phase 4: Migration & Cutover
- Create migration script (Settings JSON → EntityValue)
- Test data integrity
- Cutover existing entities
- Keep Settings for config-only use

### Phase 5: Advanced Features (Optional)
- Reporting dashboard (pie charts by field values)
- Bulk import/export with EntityValue support
- Advanced search UI (AND/OR groups)
- Field value versioning/audit trail

---

## Next Steps

1. **Add navigation properties to Entity model:**
   ```csharp
   public ICollection<EntityValue> Values { get; set; } = new List<EntityValue>();
   ```

2. **Implement EntityValueService server-side**

3. **Create EF Core migration** to add GIBS_EntityValue table

4. **Update EntityEdit.razor** to populate EntityValue on save

5. **Create EntityList search/filter** UI component

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Separate column per DataType** | Type safety, no parsing, can use SQL comparisons |
| **ValueIndex for multi-value** | Preserves order, avoids separate table, efficient |
| **ReferencedEntityId column** | Enables entity relationship filtering without additional table |
| **Typed indexes per column** | Efficient querying without table scans |
| **Cascade delete on Entity** | Clean orphan prevention, data integrity |
| **Restrict delete on EntityField** | Prevent accidental field deletion breaking values |
| **Separate Guid column** | Supports UUID-based external references |

---

## Questions to Answer

1. Should EntityValueService be injectable in EntityEdit.razor (Blazor)?
   - Option A: Yes, inject at module level
   - Option B: Call through EntityService
   - **Recommendation:** Option B - keep EntityService as the main API

2. Should we cache field definitions in EntityEdit.razor?
   - **Recommendation:** Yes, cache during load to avoid repeat queries

3. Should EntityList show all entities or require a filter?
   - **Recommendation:** Start simple - show paginated list, add filters incrementally

4. Should we support complex AND/OR search groups immediately?
   - **Recommendation:** No, start with AND-only, add OR in Phase 4

---

## Performance Considerations

### For 1000 entities with 25 custom fields each:

**Table Size:**
- GIBS_EntityValue: 25,000 rows (1000 entities × 25 fields)
- With 10x multi-value average: 250,000 rows

**Query Performance:**
- Indexes ensure <10ms query time
- Grouped indexes prevent full table scans
- Partial indexes reduce index size

**Storage:**
- ~10-20 MB for 250k rows (vs. ~5 MB JSON)
- Worth it for query performance and correctness

---

## Rollback Plan

If we encounter issues:

1. EntityValue data remains independent of Entity
2. Keep Entity.Settings as fallback
3. EntityEdit can display from either source
4. No breaking changes to Entity table structure

Very low risk migration.

---

## References

- EntityValue.cs - Model definition
- EntityValueConfiguration.cs - EF Core indexes & foreign keys
- EntityValueQueryExtensions.cs - LINQ query helpers
- IEntityValueService.cs - Service contract
