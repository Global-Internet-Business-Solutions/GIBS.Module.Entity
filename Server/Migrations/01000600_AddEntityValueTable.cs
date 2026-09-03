using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Migrations.EntityBuilders;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.06.00")]
    public class AddEntityValueTable : MultiDatabaseMigration
    {
        public AddEntityValueTable(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityValueBuilder = new EntityValueEntityBuilder(migrationBuilder, ActiveDatabase);
            entityValueBuilder.Create();

            // Create indexes for efficient searching and filtering
            // Note: TextValue is nvarchar(MAX) and cannot be used in index key columns
            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityValue_Entity_Field",
                table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
                columns: new[] { "EntityId", "FieldId" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityValue_Field_IntegerValue",
                table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
                columns: new[] { "FieldId", "IntegerValue" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityValue_Field_DecimalValue",
                table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
                columns: new[] { "FieldId", "DecimalValue" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityValue_Field_BooleanValue",
                table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
                columns: new[] { "FieldId", "BooleanValue" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityValue_Field_DateValue",
                table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
                columns: new[] { "FieldId", "DateValue" });

            migrationBuilder.CreateIndex(
                name: "IX_GIBS_EntityValue_Field_ReferencedEntity",
                table: ActiveDatabase.RewriteName("GIBS_EntityValue"),
                columns: new[] { "FieldId", "ReferencedEntityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityValueBuilder = new EntityValueEntityBuilder(migrationBuilder, ActiveDatabase);
            entityValueBuilder.Drop();
        }
    }
}
