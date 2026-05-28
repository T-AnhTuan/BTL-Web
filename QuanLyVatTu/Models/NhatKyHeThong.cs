using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class NhatKyHeThong
    {
        [Key]
        public int Id { get; set; }

        // Người thực hiện hành động
        [Required]
        [Display(Name = "Tài khoản thực hiện")]
        public int TaiKhoanId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public TaiKhoan TaiKhoan { get; set; } // Liên kết với bảng TaiKhoan ở phần trước

        [Required(ErrorMessage = "Hành động không được để trống")]
        [StringLength(255)]
        [Display(Name = "Hành Động")]
        public string HanhDong { get; set; }
        // Ví dụ: "Đăng nhập hệ thống", "Tạo phiếu nhập PN001", "Cập nhật tồn kho"

        [Display(Name = "Thời Gian")]
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Lưu vết thêm địa chỉ IP để tăng cường bảo mật (tùy chọn)
        [StringLength(50)]
        [Display(Name = "Địa Chỉ IP")]
        public string DiaChiIP { get; set; }
    }
}
