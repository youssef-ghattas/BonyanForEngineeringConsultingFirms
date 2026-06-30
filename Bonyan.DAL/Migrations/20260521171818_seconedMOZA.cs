using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonyan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class seconedMOZA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Drawings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Drawings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Drawings");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Drawings");
        }
    }
}
