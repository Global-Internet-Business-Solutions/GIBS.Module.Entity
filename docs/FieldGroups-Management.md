# Field Groups Management

## Overview
Field Groups allow you to organize entity fields into logical groups for better UI organization. For example, you might have groups like "Basic Information", "Vehicle Details", "Pricing", etc.

## Pages Created

### 1. FieldGroupsIndex.razor
**Purpose**: Lists all field groups for a specific entity type

**Features**:
- Displays field groups in a paginated table
- Shows Name, Key, Description, Sort Order, and Enabled status
- Edit and Delete actions for each group
- "Add Field Group" button to create new groups
- Back button to return to Entity Types

**URL Pattern**: `FieldGroupsIndex?entitytypeid={id}`

**Security**: View access level

### 2. FieldGroupsEdit.razor
**Purpose**: Create or edit field groups

**Fields**:
- **Name** (required, max 100 chars): Display name of the field group
- **Key** (required, max 100 chars): Unique stable key for programmatic access
- **Description** (optional, max 500 chars): Description of the field group
- **Sort Order** (number): Display order for the group
- **Enabled** (boolean): Whether the group is enabled

**Features**:
- Form validation
- Audit information display (for existing groups)
- Save and Cancel buttons
- Null-safety checks and ID validation

**URL Pattern**: 
- New: `FieldGroupsEdit?entitytypeid={id}`
- Edit: `FieldGroupsEdit?entitytypeid={id}&id={fieldGroupId}`

**Security**: Edit access level

## Navigation Added

### BackOffice Dashboard
Added new "Field Groups" card with:
- Manage Field Groups button
- Add New Field Group button
- Only enabled when an entity type is selected

### EntityTypesEdit Page
Added "Manage Field Groups" button next to "Manage Fields" in a button group

### FieldsIndex Page
Added "Manage Field Groups" button to allow quick navigation from Fields to Field Groups

## Database Model

```csharp
public class EntityFieldGroup : ModelBase
{
	public int FieldGroupId { get; set; }
	public int EntityTypeId { get; set; }
	public string Name { get; set; }
	public string Key { get; set; }
	public string Description { get; set; }
	public int SortOrder { get; set; }
	public bool IsEnabled { get; set; } = true;
}
```

## Service Operations

The `IEntityFieldGroupService` provides:
- `GetFieldGroupsAsync(entityTypeId, moduleId)` - List all groups for an entity type
- `GetFieldGroupAsync(fieldGroupId, moduleId)` - Get a specific group
- `AddFieldGroupAsync(fieldGroup)` - Create a new group
- `UpdateFieldGroupAsync(fieldGroup)` - Update an existing group
- `DeleteFieldGroupAsync(fieldGroupId, moduleId)` - Delete a group

## Usage Workflow

1. **Navigate to BackOffice** → Select an entity type from dropdown
2. **Click "Manage" in Field Groups card** → Opens FieldGroupsIndex
3. **Click "Add Field Group"** → Opens FieldGroupsEdit
4. **Fill in group details** and save
5. **Groups are now available** when creating/editing fields in the FieldsEdit page

## Field Assignment

Once field groups are created, they appear in the **FieldsEdit** page as a dropdown under "Field Group". Fields can optionally be assigned to a group to organize them logically in the UI.

## Validation & Safety

All pages include:
- ✅ Query string validation with `QueryStringHelper`
- ✅ Entity type ID must be > 0
- ✅ Null checks for async operations
- ✅ User-friendly error messages
- ✅ Form validation
- ✅ Audit trail (Created/Modified by/on)

## Benefits

- 📦 **Better Organization**: Group related fields together
- 🎨 **Improved UI**: Can render fields in logical sections
- 🔄 **Flexible**: Groups are optional - fields can exist without a group
- 🔒 **Secure**: Proper access control and validation
- 📊 **Sortable**: Control the display order of groups

## Example Use Case

For a "Product" entity type, you might create these field groups:

1. **Basic Info** (Sort: 1)
   - Name, SKU, Description

2. **Pricing** (Sort: 2)
   - Price, Cost, MSRP, Tax Category

3. **Inventory** (Sort: 3)
   - Stock Level, Reorder Point, Warehouse Location

4. **Specifications** (Sort: 4)
   - Weight, Dimensions, Color, Material

This organization makes it easier for users to find and manage fields, and allows the UI to render fields in collapsible sections or tabs.
