using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonyan.DAL.Migrations
{
    /// <inheritdoc />
    public partial class m3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// السطر ده عشان يمسح المفتاح القديم الأحادي قبل ما الـ EF يعمل المفتاح المركب الجديد
			migrationBuilder.DropPrimaryKey(
				name: "PK_MaterialInventories", // تأكد إن ده اسم المفتاح القديم عندك في الداتا بيز
				table: "MaterialInventories");

			migrationBuilder.AddColumn<string>(
                name: "SuppliedMaterialTypes",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeFactorM3",
                table: "Materials",
                type: "decimal(8,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuppliedMaterialTypes",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "VolumeFactorM3",
                table: "Materials");
        }
    }
}
