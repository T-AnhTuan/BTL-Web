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

        [Required(ErrorMessage = "Mã phiếu không được để trống")]
        [StringLength(50, ErrorMessage = "Mã phiếu không được vượt quá 50 ký tự")]
        [Display(Name = "Mã Phiếu")]
        public string MaPhieu { get; set; }

        [Required(ErrorMessage = "Ngày nhập không được để trống")]
        [Display(Name = "Ngày Nhập")]
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        // Khóa ngoại tới Nhà Cung Cấp
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        [Display(Name = "Nhà Cung Cấp")]
        public int NhaCungCapId { get; set; }

        [ForeignKey("NhaCungCapId")]
        [Display(Name = "Nhà Cung Cấp")]
        public NhaCungCap? NhaCungCap { get; set; }

        // Khóa ngoại tới Kho
        [Required(ErrorMessage = "Vui lòng chọn kho nhập")]
        [Display(Name = "Kho Nhập")]
        public int KhoId { get; set; }

        [ForeignKey("KhoId")]
        [Display(Name = "Kho Nhập")]
        public DanhMucKho? Kho { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi Chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Tổng Giá Trị")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongGiaTri { get; set; } = 0;

        // TRẠNG THÁI DUYỆT (Quan trọng)
        [Display(Name = "Trạng Thái")]
        public TrangThaiPhieuNhap TrangThai { get; set; } = TrangThaiPhieuNhap.ChoDuyet;

        // Navigation Properties (1 Phiếu Nhập có nhiều Chi tiết)
        // Nullable vì khi tạo mới phiếu chưa có chi tiết
        [Display(Name = "Chi Tiết Phiếu Nhập")]
        public ICollection<ChiTietPhieuNhap>? ChiTietPhieuNhaps { get; set; }
    }
}
