using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu.Migrations
{
    /// <inheritdoc />
    public partial class v101 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaiKhoanId1",
                table: "PhieuXuats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiNhap",
                table: "PhieuNhaps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaiKhoanId",
                table: "PhieuNhaps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TaiKhoanId1",
                table: "PhieuNhaps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuats_TaiKhoanId1",
                table: "PhieuXuats",
                column: "TaiKhoanId1");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhaps_TaiKhoanId1",
                table: "PhieuNhaps",
                column: "TaiKhoanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuNhaps_TaiKhoans_TaiKhoanId1",
                table: "PhieuNhaps",
                column: "TaiKhoanId1",
                principalTable: "TaiKhoans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuXuats_TaiKhoans_TaiKhoanId1",
                table: "PhieuXuats",
                column: "TaiKhoanId1",
                principalTable: "TaiKhoans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhieuNhaps_TaiKhoans_TaiKhoanId1",
                table: "PhieuNhaps");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuXuats_TaiKhoans_TaiKhoanId1",
                table: "PhieuXuats");

            migrationBuilder.DropIndex(
                name: "IX_PhieuXuats_TaiKhoanId1",
                table: "PhieuXuats");

            migrationBuilder.DropIndex(
                name: "IX_PhieuNhaps_TaiKhoanId1",
                table: "PhieuNhaps");

            migrationBuilder.DropColumn(
                name: "TaiKhoanId1",
                table: "PhieuXuats");

            migrationBuilder.DropColumn(
                name: "NguoiNhap",
                table: "PhieuNhaps");

            migrationBuilder.DropColumn(
                name: "TaiKhoanId",
                table: "PhieuNhaps");

            migrationBuilder.DropColumn(
                name: "TaiKhoanId1",
                table: "PhieuNhaps");
        }
    }
}
