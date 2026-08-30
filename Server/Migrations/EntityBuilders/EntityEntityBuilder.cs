using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityEntityBuilder : AuditableBaseEntityBuilder<EntityEntityBuilder>
    {
        private const string _entityTableName = "GIBS_Entity";
        private readonly PrimaryKey<EntityEntityBuilder> _primaryKey = new("PK_GIBS_Entity", x => x.EntityId);
        private readonly ForeignKey<EntityEntityBuilder> _moduleForeignKey = new("FK_GIBS_Entity_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);
        private readonly ForeignKey<EntityEntityBuilder> _entityTypeForeignKey = new("FK_GIBS_Entity_EntityType", x => x.EntityTypeId, "GIBS_EntityType", "EntityTypeId", ReferentialAction.Restrict);
        private readonly ForeignKey<EntityEntityBuilder> _parentEntityForeignKey = new("FK_GIBS_Entity_ParentEntity", x => x.ParentEntityId, "GIBS_Entity", "EntityId", ReferentialAction.Restrict);

        public EntityEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            //ForeignKeys.Add(_moduleForeignKey);
            ForeignKeys.Add(_entityTypeForeignKey);
            ForeignKeys.Add(_parentEntityForeignKey);
        }

        protected override EntityEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EntityId = AddAutoIncrementColumn(table, "EntityId");
            SiteId = AddIntegerColumn(table, "SiteId");
            ModuleId = AddIntegerColumn(table, "ModuleId");
            EntityTypeId = AddIntegerColumn(table, "EntityTypeId");
            ParentEntityId = AddIntegerColumn(table, "ParentEntityId", true);
            Name = AddStringColumn(table, "Name", 256);
            Key = AddStringColumn(table, "Key", 100, true);
            Description = AddMaxStringColumn(table, "Description", true);
            Status = AddStringColumn(table, "Status", 50, true);
            IsEnabled = AddBooleanColumn(table, "IsEnabled", false, true);
            IsFeatured = AddBooleanColumn(table, "IsFeatured", false, false);
            IsPublished = AddBooleanColumn(table, "IsPublished", false, false);
            PublishStartDate = AddDateTimeColumn(table, "PublishStartDate", true);
            PublishEndDate = AddDateTimeColumn(table, "PublishEndDate", true);
            SortOrder = AddIntegerColumn(table, "SortOrder", false, 0);
            Settings = AddMaxStringColumn(table, "Settings", true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> EntityId { get; set; }
        public OperationBuilder<AddColumnOperation> SiteId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> EntityTypeId { get; set; }
        public OperationBuilder<AddColumnOperation> ParentEntityId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Key { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
        public OperationBuilder<AddColumnOperation> Status { get; set; }
        public OperationBuilder<AddColumnOperation> IsEnabled { get; set; }
        public OperationBuilder<AddColumnOperation> IsFeatured { get; set; }
        public OperationBuilder<AddColumnOperation> IsPublished { get; set; }
        public OperationBuilder<AddColumnOperation> PublishStartDate { get; set; }
        public OperationBuilder<AddColumnOperation> PublishEndDate { get; set; }
        public OperationBuilder<AddColumnOperation> SortOrder { get; set; }
        public OperationBuilder<AddColumnOperation> Settings { get; set; }
    }
}
