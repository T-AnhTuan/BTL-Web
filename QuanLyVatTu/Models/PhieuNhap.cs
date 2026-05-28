using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu.Models
{
    public enum TrangThaiPhieuNhap
    {
        ChoDuyet = 0,    // Mặc định khi mới tạo
        DaDuyet = 1,     // Khi quản lý đã duyệt
        TuChoi = 2       // Khi bị từ chối
    }
    public class PhieuNhap
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã Phiếu")]
        public string MaPhieu { get; set; }

        [Required]
        [Display(Name = "Ngày Nhập")]
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        // Khóa ngoại tới Nhà Cung Cấp
        [Required]
        public int NhaCungCapId { get; set; }
        [ForeignKey("NhaCungCapId")]
        public NhaCungCap NhaCungCap { get; set; }

        [StringLength(100)]
        [Display(Name = "Kho Nhập")]
        public int KhoId { get; set; }
        [ForeignKey("KhoId")]
        public DanhMucKho Kho { get; set; }
        [StringLength(500)]
        [Display(Name = "Ghi Chú")]
        public string GhiChu { get; set; }

        [Display(Name = "Tổng Giá Trị")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongGiaTri { get; set; } // Thường được tự động tính: Sum(ThanhTien) của ChiTiet

        // TRẠNG THÁI DUYỆT (Quan trọng)
        public TrangThaiPhieuNhap TrangThai { get; set; } = TrangThaiPhieuNhap.ChoDuyet;

        // Navigation Properties (1 Phiếu Nhập có nhiều Chi tiết)
        public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}
