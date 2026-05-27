using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string? TenDangNhap { get; set; } 

        [Required]
        public string? MatKhau { get; set; } // Bảo mật: Không lưu mật khẩu thô

        [Required, StringLength(100)]
        public string? HoTen { get; set; } 

        public bool IsKhóa { get; set; } = false;

        public int PhanQuyenId { get; set; }

        [ForeignKey("PhanQuyenId")]
        public PhanQuyen PhanQuyen { get; set; } = null!;
    }
}
