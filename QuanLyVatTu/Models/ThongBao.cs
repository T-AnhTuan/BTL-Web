using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class ThongBao
    {
        [Key]
        public int Id { get; set; }

        // Tiêu đề & nội dung
        [Required]
        [StringLength(100)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        // Thời gian tạo
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Trạng thái
        public bool DaXem { get; set; } = false;

        // Người nhận thông báo
        public int NguoiDungId { get; set; }

        [ForeignKey("NguoiDungId")]
        public NguoiDung NguoiDung { get; set; } = null!;
    }
}
