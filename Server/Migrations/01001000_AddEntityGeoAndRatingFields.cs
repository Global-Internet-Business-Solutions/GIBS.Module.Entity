using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Migrations
{
    [DbContext(typeof(EntityContext))]
    [Migration("GIBS.Module.Entity.01.00.10.00")]
    public class AddEntityGeoAndRatingFields : MultiDatabaseMigration
    {
        public AddEntityGeoAndRatingFields(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                type: "decimal(18,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: ActiveDatabase.RewriteName("GIBS_Entity"),
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: ActiveDatabase.RewriteName("GIBS_Entity"));

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: ActiveDatabase.RewriteName("GIBS_Entity"));

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: ActiveDatabase.RewriteName("GIBS_Entity"));

            migrationBuilder.DropColumn(
                name: "Rating",
                table: ActiveDatabase.RewriteName("GIBS_Entity"));

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: ActiveDatabase.RewriteName("GIBS_Entity"));

            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: ActiveDatabase.RewriteName("GIBS_Entity"));
        }
    }
}
