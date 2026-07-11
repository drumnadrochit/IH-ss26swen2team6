using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTourCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FromLat",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FromLon",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ToLat",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ToLon",
                table: "Tours",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromLat",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "FromLon",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ToLat",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ToLon",
                table: "Tours");
        }
    }
}
