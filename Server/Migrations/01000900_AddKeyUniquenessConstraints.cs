using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.09.00")]
    public class AddKeyUniquenessConstraints : MultiDatabaseMigration
    {
        public AddKeyUniquenessConstraints(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GIBS_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"));

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityType_Site_Module_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                columns: new[] { "SiteId", "ModuleId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityFieldGroup_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityFieldGroup"),
                columns: new[] { "EntityTypeId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityField_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                columns: new[] { "EntityTypeId", "Key" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GIBS_EntityType_Site_Module_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"));

            migrationBuilder.DropIndex(
                name: "IX_GIBS_EntityFieldGroup_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityFieldGroup"));

            migrationBuilder.DropIndex(
                name: "IX_GIBS_EntityField_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"));

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                column: "Key");
        }
    }
}
