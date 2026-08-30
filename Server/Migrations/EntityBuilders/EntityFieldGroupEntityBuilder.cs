using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityFieldGroupEntityBuilder : AuditableBaseEntityBuilder<EntityFieldGroupEntityBuilder>
    {
        private const string _entityTableName = "GIBS_EntityFieldGroup";
        private readonly PrimaryKey<EntityFieldGroupEntityBuilder> _primaryKey = new("PK_GIBS_EntityFieldGroup", x => x.FieldGroupId);
        private readonly ForeignKey<EntityFieldGroupEntityBuilder> _entityTypeForeignKey = new("FK_GIBS_EntityFieldGroup_EntityType", x => x.EntityTypeId, "GIBS_EntityType", "EntityTypeId", ReferentialAction.Cascade);

        public EntityFieldGroupEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_entityTypeForeignKey);
        }

        protected override EntityFieldGroupEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FieldGroupId = AddAutoIncrementColumn(table, "FieldGroupId");
            EntityTypeId = AddIntegerColumn(table, "EntityTypeId");
            Name = AddStringColumn(table, "Name", 100);
            Key = AddStringColumn(table, "Key", 100);
            Description = AddStringColumn(table, "Description", 500, nullable: true);
            SortOrder = AddIntegerColumn(table, "SortOrder", false, 0);
            IsEnabled = AddBooleanColumn(table, "IsEnabled", false, true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> FieldGroupId { get; set; }
        public OperationBuilder<AddColumnOperation> EntityTypeId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Key { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
        public OperationBuilder<AddColumnOperation> SortOrder { get; set; }
        public OperationBuilder<AddColumnOperation> IsEnabled { get; set; }
    }
}
