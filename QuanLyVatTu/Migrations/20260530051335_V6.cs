using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu.Migrations
{
    /// <inheritdoc />
    public partial class V6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChiTiet",
                table: "NhatKyHeThongs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoaiHoatDong",
                table: "NhatKyHeThongs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChiTiet",
                table: "NhatKyHeThongs");

            migrationBuilder.DropColumn(
                name: "LoaiHoatDong",
                table: "NhatKyHeThongs");
        }
    }
}
