using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<NguoiDung> Users { get; set; }
        public DbSet<PhanQuyen> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Role entity
            modelBuilder.Entity<PhanQuyen>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure User entity
            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId);
            });

            // Seed default roles
            modelBuilder.Entity<PhanQuyen>().HasData(
                new PhanQuyen { Id = 1, RoleName = "Admin", Description = "Quản trị viên hệ thống" },
                new PhanQuyen { Id = 2, RoleName = "Manager", Description = "Quản lý kho" },
                new PhanQuyen { Id = 3, RoleName = "Staff", Description = "Nhân viên" }
            );

            // Seed default admin user (password: Admin@123)
            modelBuilder.Entity<NguoiDung>().HasData(
                new NguoiDung
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@quatuvatu.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FullName = "Quản trị viên",
                    RoleId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}

