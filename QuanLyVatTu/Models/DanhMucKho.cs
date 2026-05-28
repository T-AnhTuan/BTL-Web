using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.Models
{
    public enum TrangThaiKho
    {
        [Display(Name = "Hoạt động")]
        Active = 1,
        [Display(Name = "Tạm ngừng")]
        Inactive = 0
    }
    public class DanhMucKho
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã Kho")]
        public string MaKho { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tên Kho")]
        public string TenKho { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa Chỉ Kho")]
        public string? DiaChi { get; set; }

        public TrangThaiKho TrangThai { get; set; } = TrangThaiKho.Active;
    }
}
