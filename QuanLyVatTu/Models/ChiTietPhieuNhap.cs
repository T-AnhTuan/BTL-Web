using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class ChiTietPhieuNhap
    {
        [Key]
        public int Id { get; set; }

        // --- KHÓA NGOẠI LIÊN KẾT ĐẾN PHIẾU NHẬP (HEADER) ---
        [Required]
        public int PhieuNhapId { get; set; }

        [ForeignKey("PhieuNhapId")]
        public virtual PhieuNhap PhieuNhap { get; set; } = null!;

        // --- KHÓA NGOẠI LIÊN KẾT ĐẾN VẬT TƯ ---
        [Required]
        public int VatTuId { get; set; }

        [ForeignKey("VatTuId")]
        public virtual VatTu VatTu { get; set; } = null!;

        //Thông tin chi tiết hàng hóa
        public int STT { get; set; } //cột STT
        public string MaSo { get; set; } = string.Empty;// Mã số quản lý riêng của lô hàng (nếu có, ví dụ: Mã vạch/Số lô)

        public decimal SoLuongTheoChungTu { get; set; } // Cột số lượng trên giấy tờ

        public decimal SoLuongThucNhap { get; set; } // Cột thực tế nhập vào kho

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DonGia { get; set; }//Đơn giá

        // Thuộc tính tự động tính Thành tiền
        [NotMapped]
        public decimal ThanhTien => SoLuongThucNhap * DonGia;
        
    }
}
