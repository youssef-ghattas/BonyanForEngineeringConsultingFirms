using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonyan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_Employees_EmployeeId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_Tasks_TaskId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Tasks_TaskId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialInventories_Inventories_InventoryID",
                table: "MaterialInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialInventories_Materials_MaterialID",
                table: "MaterialInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialSuppliers_Materials_MaterialID",
                table: "MaterialSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialSuppliers_Suppliers_SupplierID",
                table: "MaterialSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialTasks_Materials_MaterialID",
                table: "MaterialTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialTasks_Tasks_TaskID",
                table: "MaterialTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteVisits_Employees_EmployeeId",
                table: "SiteVisits");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteVisits_Projects_ProjId",
                table: "SiteVisits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaterialInventories",
                table: "MaterialInventories");

            migrationBuilder.DropIndex(
                name: "IX_MaterialInventories_InventoryID_MaterialID",
                table: "MaterialInventories");

            migrationBuilder.AddColumn<int>(
                name: "PreferredSupplierID",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "Materials",
                type: "decimal(15,3)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetInventoryID",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaterialInventories",
                table: "MaterialInventories",
                columns: new[] { "InventoryID", "MaterialID" });

            migrationBuilder.CreateTable(
                name: "MaterialInvoices",
                columns: table => new
                {
                    MaterialInvoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialID = table.Column<int>(type: "int", nullable: false),
                    SupplierID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(15,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialInvoices", x => x.MaterialInvoiceID);
                    table.ForeignKey(
                        name: "FK_MaterialInvoices_Materials_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materials",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialInvoices_Suppliers_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_PreferredSupplierID",
                table: "Materials",
                column: "PreferredSupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_TargetInventoryID",
                table: "Materials",
                column: "TargetInventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInvoices_MaterialID",
                table: "MaterialInvoices",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInvoices_SupplierID",
                table: "MaterialInvoices",
                column: "SupplierID");

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_Employees_EmployeeId",
                table: "Drawings",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_Tasks_TaskId",
                table: "Drawings",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Tasks_TaskId",
                table: "Invoices",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialInventories_Inventories_InventoryID",
                table: "MaterialInventories",
                column: "InventoryID",
                principalTable: "Inventories",
                principalColumn: "InventoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialInventories_Materials_MaterialID",
                table: "MaterialInventories",
                column: "MaterialID",
                principalTable: "Materials",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Inventories_TargetInventoryID",
                table: "Materials",
                column: "TargetInventoryID",
                principalTable: "Inventories",
                principalColumn: "InventoryID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Suppliers_PreferredSupplierID",
                table: "Materials",
                column: "PreferredSupplierID",
                principalTable: "Suppliers",
                principalColumn: "SupplierID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialSuppliers_Materials_MaterialID",
                table: "MaterialSuppliers",
                column: "MaterialID",
                principalTable: "Materials",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialSuppliers_Suppliers_SupplierID",
                table: "MaterialSuppliers",
                column: "SupplierID",
                principalTable: "Suppliers",
                principalColumn: "SupplierID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialTasks_Materials_MaterialID",
                table: "MaterialTasks",
                column: "MaterialID",
                principalTable: "Materials",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialTasks_Tasks_TaskID",
                table: "MaterialTasks",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceID",
                table: "Payments",
                column: "InvoiceID",
                principalTable: "Invoices",
                principalColumn: "Invoice_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteVisits_Employees_EmployeeId",
                table: "SiteVisits",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteVisits_Projects_ProjId",
                table: "SiteVisits",
                column: "ProjId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_Employees_EmployeeId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_Tasks_TaskId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Tasks_TaskId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialInventories_Inventories_InventoryID",
                table: "MaterialInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialInventories_Materials_MaterialID",
                table: "MaterialInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Inventories_TargetInventoryID",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Suppliers_PreferredSupplierID",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialSuppliers_Materials_MaterialID",
                table: "MaterialSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialSuppliers_Suppliers_SupplierID",
                table: "MaterialSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialTasks_Materials_MaterialID",
                table: "MaterialTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialTasks_Tasks_TaskID",
                table: "MaterialTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteVisits_Employees_EmployeeId",
                table: "SiteVisits");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteVisits_Projects_ProjId",
                table: "SiteVisits");

            migrationBuilder.DropTable(
                name: "MaterialInvoices");

            migrationBuilder.DropIndex(
                name: "IX_Materials_PreferredSupplierID",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_TargetInventoryID",
                table: "Materials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaterialInventories",
                table: "MaterialInventories");

            migrationBuilder.DropColumn(
                name: "PreferredSupplierID",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "TargetInventoryID",
                table: "Materials");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaterialInventories",
                table: "MaterialInventories",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInventories_InventoryID_MaterialID",
                table: "MaterialInventories",
                columns: new[] { "InventoryID", "MaterialID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_Employees_EmployeeId",
                table: "Drawings",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_Tasks_TaskId",
                table: "Drawings",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Tasks_TaskId",
                table: "Invoices",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialInventories_Inventories_InventoryID",
                table: "MaterialInventories",
                column: "InventoryID",
                principalTable: "Inventories",
                principalColumn: "InventoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialInventories_Materials_MaterialID",
                table: "MaterialInventories",
                column: "MaterialID",
                principalTable: "Materials",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialSuppliers_Materials_MaterialID",
                table: "MaterialSuppliers",
                column: "MaterialID",
                principalTable: "Materials",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialSuppliers_Suppliers_SupplierID",
                table: "MaterialSuppliers",
                column: "SupplierID",
                principalTable: "Suppliers",
                principalColumn: "SupplierID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialTasks_Materials_MaterialID",
                table: "MaterialTasks",
                column: "MaterialID",
                principalTable: "Materials",
                principalColumn: "MaterialID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialTasks_Tasks_TaskID",
                table: "MaterialTasks",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceID",
                table: "Payments",
                column: "InvoiceID",
                principalTable: "Invoices",
                principalColumn: "Invoice_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteVisits_Employees_EmployeeId",
                table: "SiteVisits",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteVisits_Projects_ProjId",
                table: "SiteVisits",
                column: "ProjId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
