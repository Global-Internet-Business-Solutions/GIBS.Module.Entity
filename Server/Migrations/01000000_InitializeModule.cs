using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Migrations.EntityBuilders;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.00.00")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create EntityType table (metadata schema)
            var entityTypeBuilder = new EntityTypeEntityBuilder(migrationBuilder, ActiveDatabase);
            entityTypeBuilder.Create();

            // Create EntityFieldGroup table
            var fieldGroupBuilder = new EntityFieldGroupEntityBuilder(migrationBuilder, ActiveDatabase);
            fieldGroupBuilder.Create();

            // Create EntityField table
            var fieldBuilder = new EntityFieldEntityBuilder(migrationBuilder, ActiveDatabase);
            fieldBuilder.Create();

            // Create EntityFieldOption table
            var fieldOptionBuilder = new EntityFieldOptionEntityBuilder(migrationBuilder, ActiveDatabase);
            fieldOptionBuilder.Create();

            // Create Entity table (actual entity instances)
            var entityBuilder = new EntityEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();

            // Add indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityType_SiteId_ModuleId",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                columns: new[] { "SiteId", "ModuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityType_Key",
                table: ActiveDatabase.RewriteName("GIBS_EntityType"),
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityFieldGroup_EntityTypeId",
                table: ActiveDatabase.RewriteName("GIBS_EntityFieldGroup"),
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityField_EntityTypeId",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityField_FieldGroupId",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                column: "FieldGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityFieldOption_FieldId",
                table: ActiveDatabase.RewriteName("GIBS_EntityFieldOption"),
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_Entity_SiteId_ModuleId",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                columns: new[] { "SiteId", "ModuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_Entity_EntityTypeId",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_Entity_ParentEntityId",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_Entity_Key",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                column: "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop in reverse order due to foreign keys
            var entityBuilder = new EntityEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();

            var fieldOptionBuilder = new EntityFieldOptionEntityBuilder(migrationBuilder, ActiveDatabase);
            fieldOptionBuilder.Drop();

            var fieldBuilder = new EntityFieldEntityBuilder(migrationBuilder, ActiveDatabase);
            fieldBuilder.Drop();

            var fieldGroupBuilder = new EntityFieldGroupEntityBuilder(migrationBuilder, ActiveDatabase);
            fieldGroupBuilder.Drop();

            var entityTypeBuilder = new EntityTypeEntityBuilder(migrationBuilder, ActiveDatabase);
            entityTypeBuilder.Drop();
        }
    }
}
