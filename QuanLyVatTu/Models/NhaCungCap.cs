using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.Models
{
    public enum TrangThaiNhaCungCap
    {
        [Display(Name = "Hoạt động")]
        Active = 1,
        [Display(Name = "Tạm ngừng")]
        Inactive = 0
    }
    public class NhaCungCap
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã NCC là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Mã NCC")]
        public string MaNCC { get; set; }

        [Required(ErrorMessage = "Tên Nhà cung cấp là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Tên Nhà Cung Cấp")]
        public string TenNhaCungCap { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa Chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(20)]
        [Display(Name = "Số Điện Thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        public TrangThaiNhaCungCap TrangThai { get; set; } = TrangThaiNhaCungCap.Active;

        // Navigation Properties
        public ICollection<PhieuNhap> PhieuNhaps { get; set; }
    }
}
