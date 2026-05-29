using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyVatTu.Migrations
{
    /// <inheritdoc />
    public partial class V0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKho = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenKho = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucKhos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucVatTus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDanhMuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiTao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucVatTus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhaCungCaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNCC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenNhaCungCap = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaCungCaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhieuXuats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KhachHang = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    KhoId = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    NguoiXuat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LyDoXuat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuXuats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VaiTroId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaiTros_VaiTros_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTros",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VatTus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVatTu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenVatTu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DanhMucId = table.Column<int>(type: "int", nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TonKhoHienTai = table.Column<int>(type: "int", nullable: false),
                    TonToiThieu = table.Column<int>(type: "int", nullable: false),
                    GiaVonBinhQuan = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatTus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VatTus_DanhMucVatTus_DanhMucId",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMucVatTus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuNhaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhaCungCapId = table.Column<int>(type: "int", nullable: false),
                    KhoId = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TongGiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuNhaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuNhaps_NhaCungCaps_NhaCungCapId",
                        column: x => x.NhaCungCapId,
                        principalTable: "NhaCungCaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanQuyens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VaiTroId = table.Column<int>(type: "int", nullable: false),
                    MaChucNang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenChucNang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CoQuyenXem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenThem = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenSua = table.Column<bool>(type: "bit", nullable: false),
                    CoQuyenXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanQuyens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanQuyens_VaiTros_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienId = table.Column<int>(type: "int", nullable: false),
                    VaiTroId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_NhanViens_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_VaiTros_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhoId = table.Column<int>(type: "int", nullable: false),
                    VatTuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietKhos_DanhMucKhos_KhoId",
                        column: x => x.KhoId,
                        principalTable: "DanhMucKhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietKhos_VatTus_VatTuId",
                        column: x => x.VatTuId,
                        principalTable: "VatTus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietPhieuXuats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuXuatId = table.Column<int>(type: "int", nullable: false),
                    VatTuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGiaXuat = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietPhieuXuats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuXuats_PhieuXuats_PhieuXuatId",
                        column: x => x.PhieuXuatId,
                        principalTable: "PhieuXuats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuXuats_VatTus_VatTuId",
                        column: x => x.VatTuId,
                        principalTable: "VatTus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietPhieuNhaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuNhapId = table.Column<int>(type: "int", nullable: false),
                    VatTuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietPhieuNhaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuNhaps_PhieuNhaps_PhieuNhapId",
                        column: x => x.PhieuNhapId,
                        principalTable: "PhieuNhaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuNhaps_VatTus_VatTuId",
                        column: x => x.VatTuId,
                        principalTable: "VatTus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHeThongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHeThongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKyHeThongs_TaiKhoans_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXem = table.Column<bool>(type: "bit", nullable: false),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBaos_TaiKhoans_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "NhanViens",
                columns: new[] { "Id", "AvatarUrl", "DiaChi", "Email", "HoTen", "MaNV", "SoDienThoai" },
                values: new object[] { 1, null, null, null, "Administrator", "NV0001", null });

            migrationBuilder.InsertData(
                table: "VaiTros",
                columns: new[] { "Id", "MoTa", "TenVaiTro", "VaiTroId" },
                values: new object[,]
                {
                    { 1, "Toàn quyền hệ thống", "Quản trị viên", null },
                    { 2, "Duyệt phiếu, kiểm kê", "Quản lý kho", null },
                    { 3, "Lập phiếu xuất/nhập", "Nhân viên kho", null }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoans",
                columns: new[] { "Id", "LanDangNhapCuoi", "MatKhauHash", "NhanVienId", "TenDangNhap", "TrangThai", "VaiTroId" },
                values: new object[] { 1, null, "$2a$11$x2YJ/.iX1K.8lXkQ7wN5r.T2U5vWl3v9M7Q/R/Z8z5T/Y/U9k5G", 1, "admin", 1, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKhos_KhoId",
                table: "ChiTietKhos",
                column: "KhoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKhos_VatTuId",
                table: "ChiTietKhos",
                column: "VatTuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuNhaps_PhieuNhapId",
                table: "ChiTietPhieuNhaps",
                column: "PhieuNhapId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuNhaps_VatTuId",
                table: "ChiTietPhieuNhaps",
                column: "VatTuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuXuats_PhieuXuatId",
                table: "ChiTietPhieuXuats",
                column: "PhieuXuatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuXuats_VatTuId",
                table: "ChiTietPhieuXuats",
                column: "VatTuId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThongs_TaiKhoanId",
                table: "NhatKyHeThongs",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanQuyens_VaiTroId",
                table: "PhanQuyens",
                column: "VaiTroId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhaps_NhaCungCapId",
                table: "PhieuNhaps",
                column: "NhaCungCapId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_NhanVienId",
                table: "TaiKhoans",
                column: "NhanVienId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_VaiTroId",
                table: "TaiKhoans",
                column: "VaiTroId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_TaiKhoanId",
                table: "ThongBaos",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTros_VaiTroId",
                table: "VaiTros",
                column: "VaiTroId");

            migrationBuilder.CreateIndex(
                name: "IX_VatTus_DanhMucId",
                table: "VatTus",
                column: "DanhMucId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietKhos");

            migrationBuilder.DropTable(
                name: "ChiTietPhieuNhaps");

            migrationBuilder.DropTable(
                name: "ChiTietPhieuXuats");

            migrationBuilder.DropTable(
                name: "NhatKyHeThongs");

            migrationBuilder.DropTable(
                name: "PhanQuyens");

            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropTable(
                name: "DanhMucKhos");

            migrationBuilder.DropTable(
                name: "PhieuNhaps");

            migrationBuilder.DropTable(
                name: "PhieuXuats");

            migrationBuilder.DropTable(
                name: "VatTus");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "NhaCungCaps");

            migrationBuilder.DropTable(
                name: "DanhMucVatTus");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "VaiTros");
        }
    }
}
