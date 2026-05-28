using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.Models
{
    public class NhanVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Mã Nhân Viên")]
        public string MaNV { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [StringLength(20)]
        [Display(Name = "Số Điện Thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa Chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(255)]
        [Display(Name = "Ảnh Đại Diện (Avatar)")]
        public string? AvatarUrl { get; set; } // Đường dẫn lưu ảnh đại diện

        // Navigation Property: 1 Nhân viên có 1 Tài khoản (One-to-One)
        public TaiKhoan? TaiKhoan { get; set; }
    }
}
