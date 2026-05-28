using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class ThongBao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255)]
        [Display(Name = "Tiêu Đề")]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội Dung")]
        public string NoiDung { get; set; }

        [Display(Name = "Ngày Tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Cờ đánh dấu người dùng đã click vào xem thông báo hay chưa
        [Display(Name = "Đã Xem")]
        public bool DaXem { get; set; } = false;

        // Người nhận thông báo (Nếu null có thể hiểu là gửi cho toàn hệ thống/Admin)
        [Required]
        public int TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public TaiKhoan TaiKhoan { get; set; }
    }
}
