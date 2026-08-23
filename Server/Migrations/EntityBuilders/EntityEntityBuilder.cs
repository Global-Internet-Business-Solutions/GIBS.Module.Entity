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
        private const string _entityTableName = "GIBSEntity";
        private readonly PrimaryKey<EntityEntityBuilder> _primaryKey = new("PK_GIBSEntity", x => x.EntityId);
        private readonly ForeignKey<EntityEntityBuilder> _moduleForeignKey = new("FK_GIBSEntity_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public EntityEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override EntityEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EntityId = AddAutoIncrementColumn(table,"EntityId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> EntityId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
