using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class PhieuXuat
    {
        [Key]
        public int Id { get; set; }

        // Thông tin chứng từ

        [Required]
        public DateTime NgayXuat { get; set; } = DateTime.Now;

        [Required]
        public string SoPhieu { get; set; } = string.Empty;

        public string? DonVi { get; set; }

        public string? DiaChi { get; set; }

        // Định khoản kế toán
        public string? TaiKhoanNo { get; set; }

        public string? TaiKhoanCo { get; set; }

        // Thông tin xuất kho
        public string? NguoiMua { get; set; }

        public string? XuatTaiKho { get; set; }

        public string? DiaDiemKho { get; set; }

        public string? LyDoXuat { get; set; }
    
        //Thông tin kỹ nhận, tổng kết
        public int NguoiLapId { get; set; }

        [ForeignKey("NguoiLapId")]
        public NguoiDung? NguoiLap { get; set; }

        public string? ThuKho { get; set; }

        public string? KeToanTruong { get; set; }

        public string? NguoiNhanHang { get; set; }

        public string? TrangThai { get; set; }
        public string? NguoiDuyet { get; set; }
        public ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
            = new List<ChiTietPhieuXuat>();
    }
}
