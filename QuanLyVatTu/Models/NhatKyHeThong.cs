using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class NhatKyHeThong
    {
        [Key]
        public int Id { get; set; }

        // Người thực hiện
        public int NguoiDungId { get; set; }

        [ForeignKey("NguoiDungId")]
        public NguoiDung NguoiDung { get; set; } = null!;

        // Nội dung thao tác
        [Required]
        [StringLength(150)]
        public string HanhDong { get; set; } = string.Empty;

        // Ví dụ:
        // "Đăng nhập hệ thống"
        // "Tạo phiếu nhập"
        // "Xóa vật tư"

        // Thời gian
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Địa chỉ IP (nếu cần)
        [StringLength(50)]
        public string? DiaChiIP { get; set; }
    }
}
