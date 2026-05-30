using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu.Migrations
{
    /// <inheritdoc />
    public partial class v7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThongBaos_TaiKhoans_TaiKhoanId",
                table: "ThongBaos");

            migrationBuilder.AlterColumn<int>(
                name: "TaiKhoanId",
                table: "ThongBaos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "LoaiThongBao",
                table: "ThongBaos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBaos_TaiKhoans_TaiKhoanId",
                table: "ThongBaos",
                column: "TaiKhoanId",
                principalTable: "TaiKhoans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThongBaos_TaiKhoans_TaiKhoanId",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "LoaiThongBao",
                table: "ThongBaos");

            migrationBuilder.AlterColumn<int>(
                name: "TaiKhoanId",
                table: "ThongBaos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBaos_TaiKhoans_TaiKhoanId",
                table: "ThongBaos",
                column: "TaiKhoanId",
                principalTable: "TaiKhoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
