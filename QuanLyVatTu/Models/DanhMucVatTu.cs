using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.Models
{
    public enum TrangThaiHoatDong
    {
        Active = 1,
        Inactive = 0
    }
    public class DanhMucVatTu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã danh mục là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Mã Danh Mục")]
        public string MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Tên Danh Mục")]
        public string TenDanhMuc { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô Tả")]
        public string MoTa { get; set; }

        [Display(Name = "Ngày Tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Người Tạo")]
        public string NguoiTao { get; set; }

        public TrangThaiHoatDong TrangThai { get; set; } = TrangThaiHoatDong.Active;

        // Navigation Property: 1 Danh mục có nhiều Vật tư
        public ICollection<VatTu> VatTus { get; set; }
    }
}