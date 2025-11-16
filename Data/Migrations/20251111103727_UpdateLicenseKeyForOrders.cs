using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanPhanMem.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLicenseKeyForOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🚨 SỬA LỖI ĐỔI TÊN CỘT: Thay thế RenameColumn bằng DropColumn và AddColumn.
            // THAO TÁC NÀY SẼ LÀM MẤT DỮ LIỆU CŨ TRONG CỘT [Key]

            // 1. Xóa cột "Key" cũ
            migrationBuilder.DropColumn(
                name: "Key",
                table: "LicenseKeys");

            // 2. Thêm cột "KeyContent" mới
            migrationBuilder.AddColumn<string>(
                name: "KeyContent",
                table: "LicenseKeys",
                type: "nvarchar(max)",
                nullable: false, // Hoặc true tùy theo Model của bạn, nhưng thường Key không null
                defaultValue: "");

            // (Giữ nguyên logic thêm OrderId và Foreign Key)

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "LicenseKeys",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeys_OrderId",
                table: "LicenseKeys",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicenseKeys_Orders_OrderId",
                table: "LicenseKeys",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicenseKeys_Orders_OrderId",
                table: "LicenseKeys");

            migrationBuilder.DropIndex(
                name: "IX_LicenseKeys_OrderId",
                table: "LicenseKeys");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "LicenseKeys");

            // 🚨 SỬA LỖI ĐỔI TÊN CỘT (Rollback): Thay thế RenameColumn bằng DropColumn và AddColumn

            // 1. Xóa cột "KeyContent" mới (khi rollback)
            migrationBuilder.DropColumn(
                name: "KeyContent",
                table: "LicenseKeys");

            // 2. Thêm lại cột "Key" cũ (khi rollback)
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "LicenseKeys",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}