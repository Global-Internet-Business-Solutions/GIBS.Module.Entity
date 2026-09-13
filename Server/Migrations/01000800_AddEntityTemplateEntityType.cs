using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.08.00")]
    public class AddEntityTemplateEntityType : MultiDatabaseMigration
    {
        public AddEntityTemplateEntityType(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityTypeId",
                table: ActiveDatabase.RewriteName("GIBS_EntityTemplate"),
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityTemplate_Module_TemplateType_EntityType",
                table: ActiveDatabase.RewriteName("GIBS_EntityTemplate"),
                columns: new[] { "ModuleId", "TemplateType", "EntityTypeId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GIBS_EntityTemplate_Module_TemplateType_EntityType",
                table: ActiveDatabase.RewriteName("GIBS_EntityTemplate"));

            migrationBuilder.DropColumn(
                name: "EntityTypeId",
                table: ActiveDatabase.RewriteName("GIBS_EntityTemplate"));
        }
    }
}
