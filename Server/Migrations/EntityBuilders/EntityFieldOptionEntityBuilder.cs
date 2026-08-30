using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityFieldOptionEntityBuilder : AuditableBaseEntityBuilder<EntityFieldOptionEntityBuilder>
    {
        private const string _entityTableName = "GIBS_EntityFieldOption";
        private readonly PrimaryKey<EntityFieldOptionEntityBuilder> _primaryKey = new("PK_GIBS_EntityFieldOption", x => x.FieldOptionId);
        private readonly ForeignKey<EntityFieldOptionEntityBuilder> _fieldForeignKey = new("FK_GIBS_EntityFieldOption_Field", x => x.FieldId, "GIBS_EntityField", "FieldId", ReferentialAction.Cascade);

        public EntityFieldOptionEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_fieldForeignKey);
        }

        protected override EntityFieldOptionEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FieldOptionId = AddAutoIncrementColumn(table, "FieldOptionId");
            FieldId = AddIntegerColumn(table, "FieldId");
            DisplayText = AddStringColumn(table, "DisplayText", 200);
            Value = AddStringColumn(table, "Value", 100);
            Key = AddStringColumn(table, "Key", 100);
            SortOrder = AddIntegerColumn(table, "SortOrder", false, 0);
            IsDefault = AddBooleanColumn(table, "IsDefault", false, false);
            IsEnabled = AddBooleanColumn(table, "IsEnabled", false, true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> FieldOptionId { get; set; }
        public OperationBuilder<AddColumnOperation> FieldId { get; set; }
        public OperationBuilder<AddColumnOperation> DisplayText { get; set; }
        public OperationBuilder<AddColumnOperation> Value { get; set; }
        public OperationBuilder<AddColumnOperation> Key { get; set; }
        public OperationBuilder<AddColumnOperation> SortOrder { get; set; }
        public OperationBuilder<AddColumnOperation> IsDefault { get; set; }
        public OperationBuilder<AddColumnOperation> IsEnabled { get; set; }
    }
}
