using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public enum TrangThaiPhieuXuat
    {
        ChoDuyet = 0,    // Mặc định khi mới tạo
        DaDuyet = 1,     // Khi quản lý đã duyệt
        TuChoi = -1       // Khi bị từ chối
    }
    public class PhieuXuat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã Phiếu Xuất")]
        public string MaPhieu { get; set; }

        [Required]
        [Display(Name = "Ngày Xuất")]
        public DateTime NgayXuat { get; set; } = DateTime.Now;

        [Required]
        [StringLength(255)]
        [Display(Name = "Khách Hàng")]
        public string KhachHang { get; set; }
        [StringLength(100)]
        [Display(Name = "Kho xuất")]
        public int KhoId { get; set; }
        [ForeignKey("KhoId")]
        public DanhMucKho Kho { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Tài Khoản")]
        public string TaiKhoanId { get; set; }
        [StringLength(100)]
        [Display(Name = "Người Xuất")]
        public string? NguoiXuat { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý Do Xuất")]
        public string? LyDoXuat { get; set; }

        [Display(Name = "Tổng Tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; } // Thường được tự động tính: Sum(ThanhTien) của ChiTiet

        // TRẠNG THÁI DUYỆT (Quan trọng)
        public TrangThaiPhieuXuat TrangThai { get; set; } = TrangThaiPhieuXuat.ChoDuyet;

        // Navigation Properties (1 Phiếu Xuất có nhiều Chi tiết)
        public ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
    }
}
