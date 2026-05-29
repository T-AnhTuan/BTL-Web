using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.ViewModels
{
    public class TaiKhoanCaNhanVM
    {
        public int TaiKhoanId { get; set; }
        public int NhanVienId { get; set; }

        // --- THÔNG TIN CƠ BẢN ---
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Tên đầy đủ")]
        public string HoTen { get; set; }

        [Display(Name = "Tên tài khoản")]
        public string TenDangNhap { get; set; } // Readonly

        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } // Readonly

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email liên hệ")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        public string? AvatarUrl { get; set; }

        [Display(Name = "Ảnh đại diện mới")]
        public IFormFile? AvatarUpload { get; set; } // Dùng để nhận file ảnh từ form gửi lên

        // --- BẢO MẬT (ĐỔI MẬT KHẨU) ---
        [Display(Name = "Mật khẩu hiện tại")]
        public string? MatKhauHienTai { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string? MatKhauMoi { get; set; }

        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? XacNhanMatKhauMoi { get; set; }
    }
}