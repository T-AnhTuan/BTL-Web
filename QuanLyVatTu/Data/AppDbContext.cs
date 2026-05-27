using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Models;

namespace QuanLyVatTu.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet cho tất cả các bảng
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<PhanQuyen> PhanQuyens { get; set; }
        public DbSet<VatTu> VatTus { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<PhieuXuat> PhieuXuats { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
        public DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PhanQuyen entity
            modelBuilder.Entity<PhanQuyen>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenNhomQuyen).IsRequired().HasMaxLength(30);
                entity.Property(e => e.MoTa).HasMaxLength(500);
                // Mối quan hệ: 1 quyền - Nhiều người dùng
                entity.HasMany(e => e.NguoiDung)
                    .WithOne(nd => nd.PhanQuyen)
                    .HasForeignKey(nd => nd.PhanQuyenId);
            });

            // Configure NguoiDung entity
            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenDangNhap).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MatKhau).IsRequired();
                entity.Property(e => e.HoTen).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsKhóa).HasDefaultValue(false);
                entity.Property(e => e.PhanQuyenId).IsRequired();
                // Mối quan hệ: Nhiều người dùng - Một quyền
                entity.HasOne(e => e.PhanQuyen)
                    .WithMany(pq => pq.NguoiDung)
                    .HasForeignKey(e => e.PhanQuyenId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Mối quan hệ: 1 người dùng - Nhiều nhật ký
                entity.HasMany(e => e.NhatKyHeThongs)
                    .WithOne(nk => nk.NguoiDung)
                    .HasForeignKey(nk => nk.NguoiDungId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Mối quan hệ: 1 người dùng - Nhiều thông báo
                entity.HasMany(e => e.ThongBaos)
                    .WithOne(tb => tb.NguoiDung)
                    .HasForeignKey(tb => tb.NguoiDungId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure VatTu entity
            modelBuilder.Entity<VatTu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenVatTu).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DonViTinh).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SoLuongTon).HasDefaultValue(0);
                entity.Property(e => e.NhaCungCap).HasMaxLength(200);
                // Mối quan hệ: 1 vật tư - Nhiều chi tiết phiếu nhập
                entity.HasMany(e => e.ChiTietPhieuNhaps)
                    .WithOne(ct => ct.VatTu)
                    .HasForeignKey(ct => ct.VatTuId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PhieuNhap entity
            modelBuilder.Entity<PhieuNhap>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NgayNhap).IsRequired();
                entity.Property(e => e.SoPhieu).HasMaxLength(50);
                entity.Property(e => e.DonVi).HasMaxLength(100);
                entity.Property(e => e.DiaChi).HasMaxLength(200);
                entity.Property(e => e.NguoiLapId).IsRequired();
                // Mối quan hệ: 1 phiếu nhập - Nhiều chi tiết
                entity.HasMany(e => e.ChiTietPhieuNhaps)
                    .WithOne(ct => ct.PhieuNhap)
                    .HasForeignKey(ct => ct.PhieuNhapId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PhieuXuat entity
            modelBuilder.Entity<PhieuXuat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NgayXuat).IsRequired();
                entity.Property(e => e.SoPhieu).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DonVi).HasMaxLength(100);
                entity.Property(e => e.DiaChi).HasMaxLength(200);
                entity.Property(e => e.NguoiMua).HasMaxLength(100);
                entity.Property(e => e.LyDoXuat).HasMaxLength(200);
                // Mối quan hệ: 1 phiếu xuất - Nhiều chi tiết
                entity.HasMany(e => e.ChiTietPhieuXuats)
                    .WithOne(ct => ct.PhieuXuat)
                    .HasForeignKey(ct => ct.PhieuXuatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ChiTietPhieuNhap entity
            modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhieuNhapId).IsRequired();
                entity.Property(e => e.VatTuId).IsRequired();
                entity.Property(e => e.MaSo).HasMaxLength(50);
                entity.Property(e => e.SoLuongTheoChungTu).HasPrecision(10, 2);
                entity.Property(e => e.SoLuongThucNhap).HasPrecision(10, 2);
                entity.Property(e => e.DonGia).HasColumnType("decimal(10,2)");
            });

            // Configure ChiTietPhieuXuat entity
            modelBuilder.Entity<ChiTietPhieuXuat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhieuXuatId).IsRequired();
                entity.Property(e => e.VatTuId).IsRequired();
                entity.Property(e => e.MaSo).HasMaxLength(50);
                entity.Property(e => e.SoLuongYeuCau).HasPrecision(10, 2);
                entity.Property(e => e.SoLuongThucXuat).HasPrecision(10, 2);
                entity.Property(e => e.DonGia).HasColumnType("decimal(10,2)");
            });

            // Configure NhatKyHeThong entity
            modelBuilder.Entity<NhatKyHeThong>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NguoiDungId).IsRequired();
                entity.Property(e => e.HanhDong).IsRequired().HasMaxLength(150);
                entity.Property(e => e.ThoiGian).IsRequired();
                entity.Property(e => e.DiaChiIP).HasMaxLength(50);
            });

            // Configure ThongBao entity
            modelBuilder.Entity<ThongBao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TieuDe).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NoiDung).IsRequired();
                entity.Property(e => e.NgayTao).IsRequired();
                entity.Property(e => e.DaXem).HasDefaultValue(false);
                entity.Property(e => e.NguoiDungId).IsRequired();
            });

            // Seed default roles
            modelBuilder.Entity<PhanQuyen>().HasData(
                new PhanQuyen { Id = 1, TenNhomQuyen = "Admin", MoTa = "Quản trị viên hệ thống" },
                new PhanQuyen { Id = 2, TenNhomQuyen = "Quản Lý", MoTa = "Quản lý kho" },
                new PhanQuyen { Id = 3, TenNhomQuyen = "Nhân Viên", MoTa = "Nhân viên" }
            );

            // Seed default admin user (password: Admin@123)
            modelBuilder.Entity<NguoiDung>().HasData(
                new NguoiDung
                {
                    Id = 1,
                    TenDangNhap = "admin",
                    MatKhau = "Admin@123",
                    HoTen = "Quản trị viên",
                    IsKhóa = false,
                    PhanQuyenId = 1
                }
            );
        }
    }
}

