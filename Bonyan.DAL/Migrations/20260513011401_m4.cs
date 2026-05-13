using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonyan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class m4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_User_CreatedBy_UserID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Employees_EmployeeId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "UserAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_User_EmployeeId",
                table: "UserAccounts",
                newName: "IX_UserAccounts_EmployeeId");

            migrationBuilder.AddColumn<int>(
                name: "AssignedByAdminId",
                table: "EmployeeProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "AdminAccounts",
                columns: table => new
                {
                    AdminAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAccounts", x => x.AdminAccountId);
                    table.ForeignKey(
                        name: "FK_AdminAccounts_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProjects_AssignedByAdminId",
                table: "EmployeeProjects",
                column: "AssignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAccounts_AdminId",
                table: "AdminAccounts",
                column: "AdminId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeProjects_Admins_AssignedByAdminId",
                table: "EmployeeProjects",
                column: "AssignedByAdminId",
                principalTable: "Admins",
                principalColumn: "AdminId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_UserAccounts_CreatedBy_UserID",
                table: "Tasks",
                column: "CreatedBy_UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_Employees_EmployeeId",
                table: "UserAccounts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeProjects_Admins_AssignedByAdminId",
                table: "EmployeeProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_UserAccounts_CreatedBy_UserID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_Employees_EmployeeId",
                table: "UserAccounts");

            migrationBuilder.DropTable(
                name: "AdminAccounts");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeProjects_AssignedByAdminId",
                table: "EmployeeProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "AssignedByAdminId",
                table: "EmployeeProjects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserAccounts");

            migrationBuilder.RenameTable(
                name: "UserAccounts",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_UserAccounts_EmployeeId",
                table: "User",
                newName: "IX_User_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_User_CreatedBy_UserID",
                table: "Tasks",
                column: "CreatedBy_UserID",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employees_EmployeeId",
                table: "User",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
