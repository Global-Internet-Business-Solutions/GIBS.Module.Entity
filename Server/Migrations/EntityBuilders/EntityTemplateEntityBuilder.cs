using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityTemplateEntityBuilder : AuditableBaseEntityBuilder<EntityTemplateEntityBuilder>
    {
        private const string _entityTableName = "GIBS_EntityTemplate";
        private readonly PrimaryKey<EntityTemplateEntityBuilder> _primaryKey = new("PK_GIBS_EntityTemplate", x => x.TemplateId);
        private readonly ForeignKey<EntityTemplateEntityBuilder> _moduleForeignKey = new("FK_GIBS_EntityTemplate_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public EntityTemplateEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override EntityTemplateEntityBuilder BuildTable(ColumnsBuilder table)
        {
            TemplateId = AddAutoIncrementColumn(table, "TemplateId");
            ModuleId = AddIntegerColumn(table, "ModuleId");
            Header = AddMaxStringColumn(table, "Header", nullable: true);
            Item = AddMaxStringColumn(table, "Item", nullable: true);
            Alternate = AddMaxStringColumn(table, "Alternate", nullable: true);
            Separator = AddStringColumn(table, "Separator", 200, nullable: true);
            Footer = AddMaxStringColumn(table, "Footer", nullable: true);
            PageTitle = AddStringColumn(table, "PageTitle", 200, nullable: true);
            PageDescription = AddStringColumn(table, "PageDescription", 500, nullable: true);
            PageKeywords = AddStringColumn(table, "PageKeywords", 500, nullable: true);
            PageHeader = AddStringColumn(table, "PageHeader", 200, nullable: true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> TemplateId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Header { get; set; }
        public OperationBuilder<AddColumnOperation> Item { get; set; }
        public OperationBuilder<AddColumnOperation> Alternate { get; set; }
        public OperationBuilder<AddColumnOperation> Separator { get; set; }
        public OperationBuilder<AddColumnOperation> Footer { get; set; }
        public OperationBuilder<AddColumnOperation> PageTitle { get; set; }
        public OperationBuilder<AddColumnOperation> PageDescription { get; set; }
        public OperationBuilder<AddColumnOperation> PageKeywords { get; set; }
        public OperationBuilder<AddColumnOperation> PageHeader { get; set; }
    }
}
