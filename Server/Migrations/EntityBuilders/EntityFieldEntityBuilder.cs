using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityFieldEntityBuilder : AuditableBaseEntityBuilder<EntityFieldEntityBuilder>
    {
        private const string _entityTableName = "GIBS_EntityField";
        private readonly PrimaryKey<EntityFieldEntityBuilder> _primaryKey = new("PK_GIBS_EntityField", x => x.FieldId);
        private readonly ForeignKey<EntityFieldEntityBuilder> _entityTypeForeignKey = new("FK_GIBS_EntityField_EntityType", x => x.EntityTypeId, "GIBS_EntityType", "EntityTypeId", ReferentialAction.Cascade);
        private readonly ForeignKey<EntityFieldEntityBuilder> _fieldGroupForeignKey = new("FK_GIBS_EntityField_FieldGroup", x => x.FieldGroupId, "GIBS_EntityFieldGroup", "FieldGroupId", ReferentialAction.Restrict);
        private readonly ForeignKey<EntityFieldEntityBuilder> _referencedEntityTypeForeignKey = new("FK_GIBS_EntityField_ReferencedEntityType", x => x.ReferencedEntityTypeId, "GIBS_EntityType", "EntityTypeId", ReferentialAction.Restrict);

        public EntityFieldEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_entityTypeForeignKey);
            ForeignKeys.Add(_fieldGroupForeignKey);
            ForeignKeys.Add(_referencedEntityTypeForeignKey);
        }

        protected override EntityFieldEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FieldId = AddAutoIncrementColumn(table, "FieldId");
            EntityTypeId = AddIntegerColumn(table, "EntityTypeId");
            FieldGroupId = AddIntegerColumn(table, "FieldGroupId", nullable: true);
            Name = AddStringColumn(table, "Name", 100);
            Key = AddStringColumn(table, "Key", 100);
            Label = AddStringColumn(table, "Label", 200, nullable: true);
            Description = AddStringColumn(table, "Description", 1000, nullable: true);
            DataType = AddIntegerColumn(table, "DataType");
            EditorType = AddIntegerColumn(table, "EditorType");
            MaxLength = AddIntegerColumn(table, "MaxLength", nullable: true);
            Precision = AddIntegerColumn(table, "Precision", nullable: true);
            Scale = AddIntegerColumn(table, "Scale", nullable: true);
            DefaultValue = AddStringColumn(table, "DefaultValue", 500, nullable: true);
            Placeholder = AddStringColumn(table, "Placeholder", 200, nullable: true);
            HelpText = AddStringColumn(table, "HelpText", 1000, nullable: true);
            IsRequired = AddBooleanColumn(table, "IsRequired", false, false);
            IsReadOnly = AddBooleanColumn(table, "IsReadOnly", false, false);
            IsHidden = AddBooleanColumn(table, "IsHidden", false, false);
            IsMultiValue = AddBooleanColumn(table, "IsMultiValue", false, false);
            MinimumValues = AddIntegerColumn(table, "MinimumValues", nullable: true);
            MaximumValues = AddIntegerColumn(table, "MaximumValues", nullable: true);
            IsSearchable = AddBooleanColumn(table, "IsSearchable", false, false);
            IsFilterable = AddBooleanColumn(table, "IsFilterable", false, false);
            IsSortable = AddBooleanColumn(table, "IsSortable", false, false);
            ReferencedEntityTypeId = AddIntegerColumn(table, "ReferencedEntityTypeId", nullable: true);
            ValidationSettings = AddMaxStringColumn(table, "ValidationSettings", nullable: true);
            EditorSettings = AddMaxStringColumn(table, "EditorSettings", nullable: true);
            SortOrder = AddIntegerColumn(table, "SortOrder", false, 0);
            IsEnabled = AddBooleanColumn(table, "IsEnabled", false, true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> FieldId { get; set; }
        public OperationBuilder<AddColumnOperation> EntityTypeId { get; set; }
        public OperationBuilder<AddColumnOperation> FieldGroupId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Key { get; set; }
        public OperationBuilder<AddColumnOperation> Label { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
        public OperationBuilder<AddColumnOperation> DataType { get; set; }
        public OperationBuilder<AddColumnOperation> EditorType { get; set; }
        public OperationBuilder<AddColumnOperation> MaxLength { get; set; }
        public OperationBuilder<AddColumnOperation> Precision { get; set; }
        public OperationBuilder<AddColumnOperation> Scale { get; set; }
        public OperationBuilder<AddColumnOperation> DefaultValue { get; set; }
        public OperationBuilder<AddColumnOperation> Placeholder { get; set; }
        public OperationBuilder<AddColumnOperation> HelpText { get; set; }
        public OperationBuilder<AddColumnOperation> IsRequired { get; set; }
        public OperationBuilder<AddColumnOperation> IsReadOnly { get; set; }
        public OperationBuilder<AddColumnOperation> IsHidden { get; set; }
        public OperationBuilder<AddColumnOperation> IsMultiValue { get; set; }
        public OperationBuilder<AddColumnOperation> MinimumValues { get; set; }
        public OperationBuilder<AddColumnOperation> MaximumValues { get; set; }
        public OperationBuilder<AddColumnOperation> IsSearchable { get; set; }
        public OperationBuilder<AddColumnOperation> IsFilterable { get; set; }
        public OperationBuilder<AddColumnOperation> IsSortable { get; set; }
        public OperationBuilder<AddColumnOperation> ReferencedEntityTypeId { get; set; }
        public OperationBuilder<AddColumnOperation> ValidationSettings { get; set; }
        public OperationBuilder<AddColumnOperation> EditorSettings { get; set; }
        public OperationBuilder<AddColumnOperation> SortOrder { get; set; }
        public OperationBuilder<AddColumnOperation> IsEnabled { get; set; }
    }
}
