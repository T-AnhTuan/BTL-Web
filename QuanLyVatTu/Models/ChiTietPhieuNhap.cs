using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class ChiTietPhieuNhap
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuNhapId { get; set; }
        [ForeignKey("PhieuNhapId")]
        public PhieuNhap PhieuNhap { get; set; }

        [Required]
        public int VatTuId { get; set; }
        [ForeignKey("VatTuId")]
        public VatTu VatTu { get; set; }

        [Required]
        [Display(Name = "Số Lượng")]
        public int SoLuong { get; set; }

        [Required]
        [Display(Name = "Đơn Giá")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [Display(Name = "Thành Tiền")]
        [NotMapped]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien => SoLuong * DonGia;
        
    }
}
