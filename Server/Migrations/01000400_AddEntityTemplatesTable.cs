using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Migrations.EntityBuilders;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.04.00")]
    public class AddEntityTemplatesTable : MultiDatabaseMigration
    {
        public AddEntityTemplatesTable(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var templateBuilder = new EntityTemplateEntityBuilder(migrationBuilder, ActiveDatabase);
            templateBuilder.Create();

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityTemplate_ModuleId",
                table: ActiveDatabase.RewriteName("GIBS_EntityTemplate"),
                column: "ModuleId");

            migrationBuilder.DropColumn(
                name: "ListTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"));

            migrationBuilder.DropColumn(
                name: "DetailTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"));

            migrationBuilder.DropColumn(
                name: "SearchTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"));

            migrationBuilder.DropColumn(
                name: "FeaturedTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ListTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedTemplate",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                nullable: true);

            var templateBuilder = new EntityTemplateEntityBuilder(migrationBuilder, ActiveDatabase);
            templateBuilder.Drop();
        }
    }
}
