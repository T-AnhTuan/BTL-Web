using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.ViewModels.NguoiDung
{
    /// <summary>
    /// ViewModel cho trang Hồ sơ cá nhân (QuanLyTaiKhoan.cshtml)
    /// Dùng để hiển thị và chỉnh sửa thông tin người dùng
    /// </summary>
    public class HoSoCaNhanVM
    {
        // --- Thông tin cơ bản ---
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Tên đầy đủ")]
        [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Display(Name = "Tên tài khoản")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } // ReadOnly

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [Display(Name = "Vai trò")]
        [StringLength(50)]
        public string VaiTro { get; set; } // ReadOnly - lấy từ PhanQuyen

        // --- Bảo mật ---
        [Display(Name = "Mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string MatKhauHienTai { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string MatKhauMoi { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string MatKhauXacNhan { get; set; }

        // --- Avatar ---
        [Display(Name = "Avatar")]
        public IFormFile AvatarFile { get; set; }

        public string AvatarUrl { get; set; } // URL ảnh hiện tại
    }
}
