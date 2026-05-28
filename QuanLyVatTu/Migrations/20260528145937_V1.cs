using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyVatTu.Migrations
{
    /// <inheritdoc />
    public partial class V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VaiTros_VaiTros_VaiTroId",
                table: "VaiTros");

            migrationBuilder.DropIndex(
                name: "IX_VaiTros_VaiTroId",
                table: "VaiTros");

            migrationBuilder.DropColumn(
                name: "VaiTroId",
                table: "VaiTros");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuats_KhoId",
                table: "PhieuXuats",
                column: "KhoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhaps_KhoId",
                table: "PhieuNhaps",
                column: "KhoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuNhaps_DanhMucKhos_KhoId",
                table: "PhieuNhaps",
                column: "KhoId",
                principalTable: "DanhMucKhos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuXuats_DanhMucKhos_KhoId",
                table: "PhieuXuats",
                column: "KhoId",
                principalTable: "DanhMucKhos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhieuNhaps_DanhMucKhos_KhoId",
                table: "PhieuNhaps");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuXuats_DanhMucKhos_KhoId",
                table: "PhieuXuats");

            migrationBuilder.DropIndex(
                name: "IX_PhieuXuats_KhoId",
                table: "PhieuXuats");

            migrationBuilder.DropIndex(
                name: "IX_PhieuNhaps_KhoId",
                table: "PhieuNhaps");

            migrationBuilder.AddColumn<int>(
                name: "VaiTroId",
                table: "VaiTros",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "VaiTros",
                keyColumn: "Id",
                keyValue: 1,
                column: "VaiTroId",
                value: null);

            migrationBuilder.UpdateData(
                table: "VaiTros",
                keyColumn: "Id",
                keyValue: 2,
                column: "VaiTroId",
                value: null);

            migrationBuilder.UpdateData(
                table: "VaiTros",
                keyColumn: "Id",
                keyValue: 3,
                column: "VaiTroId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTros_VaiTroId",
                table: "VaiTros",
                column: "VaiTroId");

            migrationBuilder.AddForeignKey(
                name: "FK_VaiTros_VaiTros_VaiTroId",
                table: "VaiTros",
                column: "VaiTroId",
                principalTable: "VaiTros",
                principalColumn: "Id");
        }
    }
}
