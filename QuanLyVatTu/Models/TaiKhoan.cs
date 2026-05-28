using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public enum TrangThaiTaiKhoan
    {
        [Display(Name = "Hoạt động")]
        Active = 1,
        [Display(Name = "Bị Khóa")]
        Locked = 0
    }
    public class TaiKhoan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Tên Đăng Nhập")]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(255)]
        public string MatKhauHash { get; set; } // KHÔNG lưu mật khẩu gốc, phải băm (Hash) bằng BCrypt hoặc PBKDF2

        public TrangThaiTaiKhoan TrangThai { get; set; } = TrangThaiTaiKhoan.Active;

        [Display(Name = "Lần Đăng Nhập Cuối")]
        public DateTime? LanDangNhapCuoi { get; set; }

        // Khóa ngoại 1: Liên kết với Nhân Viên (1-1)
        [Required]
        public int NhanVienId { get; set; }
        [ForeignKey("NhanVienId")]
        public NhanVien NhanVien { get; set; }

        // Khóa ngoại 2: Liên kết với Vai Trò (1-N)
        [Required]
        [Display(Name = "Vai Trò")]
        public int VaiTroId { get; set; }
        [ForeignKey("VaiTroId")]
        public VaiTro VaiTro { get; set; }
    }
}

