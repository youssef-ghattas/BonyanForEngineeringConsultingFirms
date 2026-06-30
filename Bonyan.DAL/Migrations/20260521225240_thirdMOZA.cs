using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonyan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class thirdMOZA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToEmployeeId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToEmployeeId",
                table: "Tasks",
                column: "AssignedToEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Employees_AssignedToEmployeeId",
                table: "Tasks",
                column: "AssignedToEmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Employees_AssignedToEmployeeId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_AssignedToEmployeeId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignedToEmployeeId",
                table: "Tasks");
        }
    }
}
