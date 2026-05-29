using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Models;

namespace QuanLyVatTu.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ==========================================
        // 1. KHAI BÁO CÁC DBSET (Các bảng trong SQL Server)
        // ==========================================

        // Nhóm Danh Mục & Hàng Hóa
        public DbSet<DanhMucVatTu> DanhMucVatTus { get; set; }
        public DbSet<VatTu> VatTus { get; set; }
        public DbSet<DanhMucKho> DanhMucKhos { get; set; }
        public DbSet<ChiTietKho> ChiTietKhos { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }

        // Nhóm Nghiệp Vụ Nhập - Xuất
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public DbSet<PhieuXuat> PhieuXuats { get; set; }
        public DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }

        // Nhóm Bảo Mật & Phân Quyền
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<PhanQuyen> PhanQuyens { get; set; }

        // Nhóm Theo Dõi (Logs & Notifications)
        public DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }


        // ==========================================
        // 2. CẤU HÌNH FLUENT API & SEED DATA
        // ==========================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- A. BẢO VỆ DỮ LIỆU: TẮT CASCADE DELETE TOÀN CỤC ---
            // Tránh tình trạng lỡ tay xóa 1 Nhà cung cấp làm mất sạch Phiếu nhập của họ
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // --- B. TẠO DỮ LIỆU MẶC ĐỊNH LÚC KHỞI TẠO (SEED DATA) ---

            // 1. Tạo các Vai trò (Roles) cơ bản
            modelBuilder.Entity<VaiTro>().HasData(
                new VaiTro { Id = 1, TenVaiTro = "Quản trị viên", MoTa = "Toàn quyền hệ thống" },
                new VaiTro { Id = 2, TenVaiTro = "Quản lý kho", MoTa = "Duyệt phiếu, kiểm kê" },
                new VaiTro { Id = 3, TenVaiTro = "Nhân viên kho", MoTa = "Lập phiếu xuất/nhập" }
            );

            // 2. Tạo Hồ sơ Nhân viên mặc định (cho Admin)
            modelBuilder.Entity<NhanVien>().HasData(
                new NhanVien
                {
                    Id = 1,
                    MaNV = "NV0001",
                    HoTen = "Administrator"
                }
            );

            // 3. Tạo Tài khoản Đăng nhập cho Admin
            // LƯU Ý QUAN TRỌNG: Chuỗi băm dưới đây tương đương mật khẩu "Admin@123" được tạo bằng BCrypt.
            // Sử dụng chuỗi cứng (Hardcoded string) để tránh lỗi PendingModelChangesWarning khi chạy Migration.
            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    Id = 1,
                    TenDangNhap = "admin",
                    MatKhauHash = "$2a$11$x2YJ/.iX1K.8lXkQ7wN5r.T2U5vWl3v9M7Q/R/Z8z5T/Y/U9k5G", // "Admin@123"
                    TrangThai = TrangThaiTaiKhoan.Active,
                    NhanVienId = 1,
                    VaiTroId = 1
                }
            );
        }
    }
}