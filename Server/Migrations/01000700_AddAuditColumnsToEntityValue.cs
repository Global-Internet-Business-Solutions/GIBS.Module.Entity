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
            // No-op: audit columns are part of EntityValue table creation in 01.00.06.00.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: audit columns are removed with table drop in 01.00.06.00.
        }
    }
}
