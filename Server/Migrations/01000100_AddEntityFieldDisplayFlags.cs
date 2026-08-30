using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.01.00")]
    public class AddEntityFieldDisplayFlags : MultiDatabaseMigration
    {
        public AddEntityFieldDisplayFlags(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<bool>(
            //    name: "IsFeatured",
            //    table: ActiveDatabase.RewriteName("GIBS_EntityField"),
            //    nullable: false,
            //    defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsListed",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManagerOnly",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCaptionHidden",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLockedDown",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"),
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "IsFeatured",
            //    table: ActiveDatabase.RewriteName("GIBS_EntityField"));

            migrationBuilder.DropColumn(
                name: "IsListed",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"));

            migrationBuilder.DropColumn(
                name: "IsManagerOnly",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"));

            migrationBuilder.DropColumn(
                name: "IsCaptionHidden",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"));

            migrationBuilder.DropColumn(
                name: "IsLockedDown",
                table: ActiveDatabase.RewriteName("GIBS_EntityField"));
        }
    }
}
