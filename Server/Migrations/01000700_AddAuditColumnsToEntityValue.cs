using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.07.00")]
    public class AddAuditColumnsToEntityValue : MultiDatabaseMigration
    {
        public AddAuditColumnsToEntityValue(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string tableName = ActiveDatabase.RewriteName("GIBS_EntityValue");

            // Add audit columns to GIBS_EntityValue table
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: tableName,
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<System.DateTime>(
                name: "CreatedOn",
                table: tableName,
                nullable: false,
                defaultValue: System.DateTime.UtcNow);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: tableName,
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<System.DateTime>(
                name: "ModifiedOn",
                table: tableName,
                nullable: false,
                defaultValue: System.DateTime.UtcNow);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string tableName = ActiveDatabase.RewriteName("GIBS_EntityValue");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: tableName);
        }
    }
}
