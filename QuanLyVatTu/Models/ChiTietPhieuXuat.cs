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
         public virtual PhieuXuat PhieuXuat { get; set; } = null!;

         [Required]
         public int VatTuId { get; set; }

         [ForeignKey("VatTuId")]
         public virtual VatTu VatTu { get; set; } = null!;

         // Thông tin vật tư
         public int STT { get; set; }

         public string? MaSo { get; set; }

         public decimal SoLuongYeuCau { get; set; }

         public decimal SoLuongThucXuat { get; set; }

         [Required]
         [Column(TypeName = "decimal(10,2)")]
         public decimal DonGia { get; set; }

         // Thành tiền
         [NotMapped]
         public decimal ThanhTien => SoLuongThucXuat * DonGia;
    }
}
