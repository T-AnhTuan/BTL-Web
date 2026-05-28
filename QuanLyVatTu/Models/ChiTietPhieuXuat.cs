using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class ChiTietPhieuXuat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuXuatId { get; set; }
        [ForeignKey("PhieuXuatId")]
        public PhieuXuat PhieuXuat { get; set; }

        [Required]
        public int VatTuId { get; set; }
        [ForeignKey("VatTuId")]
        public VatTu VatTu { get; set; }

        [Required]
        [Display(Name = "Số Lượng Xuất")]
        public int SoLuong { get; set; }

        [Display(Name = "Đơn Giá Xuất (Giá Vốn)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaXuat { get; set; } // Sẽ được hệ thống tự tính và điền vào dựa trên phương pháp Bình Quân Gia Quyền

        [Display(Name = "Thành Tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien => SoLuong * DonGiaXuat;
    }
}
