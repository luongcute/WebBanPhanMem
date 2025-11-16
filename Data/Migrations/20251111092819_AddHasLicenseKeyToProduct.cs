using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanPhanMem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHasLicenseKeyToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasLicenseKey",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeys_ProductId",
                table: "LicenseKeys",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicenseKeys_Products_ProductId",
                table: "LicenseKeys",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicenseKeys_Products_ProductId",
                table: "LicenseKeys");

            migrationBuilder.DropIndex(
                name: "IX_LicenseKeys_ProductId",
                table: "LicenseKeys");

            migrationBuilder.DropColumn(
                name: "HasLicenseKey",
                table: "Products");
        }
    }
}
