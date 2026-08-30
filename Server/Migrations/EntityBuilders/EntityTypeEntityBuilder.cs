using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityTypeEntityBuilder : AuditableBaseEntityBuilder<EntityTypeEntityBuilder>
    {
        private const string _entityTableName = "GIBS_EntityType";
        private readonly PrimaryKey<EntityTypeEntityBuilder> _primaryKey = new("PK_GIBS_EntityType", x => x.EntityTypeId);
        private readonly ForeignKey<EntityTypeEntityBuilder> _moduleForeignKey = new("FK_GIBS_EntityType_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);
        private readonly ForeignKey<EntityTypeEntityBuilder> _parentForeignKey = new("FK_GIBS_EntityType_Parent", x => x.ParentEntityTypeId, "GIBS_EntityType", "EntityTypeId", ReferentialAction.Restrict);

        public EntityTypeEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
            ForeignKeys.Add(_parentForeignKey);
        }

        protected override EntityTypeEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EntityTypeId = AddAutoIncrementColumn(table, "EntityTypeId");
            SiteId = AddIntegerColumn(table, "SiteId");
            ModuleId = AddIntegerColumn(table, "ModuleId");
            Name = AddStringColumn(table, "Name", 100);
            Key = AddStringColumn(table, "Key", 100);
            Description = AddStringColumn(table, "Description", 500, nullable: true);
            ParentEntityTypeId = AddIntegerColumn(table, "ParentEntityTypeId", nullable: true);
            IsEnabled = AddBooleanColumn(table, "IsEnabled", false, true);
            IsSystem = AddBooleanColumn(table, "IsSystem", false, false);
            SortOrder = AddIntegerColumn(table, "SortOrder", false, 0);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> EntityTypeId { get; set; }
        public OperationBuilder<AddColumnOperation> SiteId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Key { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
        public OperationBuilder<AddColumnOperation> ParentEntityTypeId { get; set; }
        public OperationBuilder<AddColumnOperation> IsEnabled { get; set; }
        public OperationBuilder<AddColumnOperation> IsSystem { get; set; }
        public OperationBuilder<AddColumnOperation> SortOrder { get; set; }
    }
}
